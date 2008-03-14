/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;

    /// <summary>
    /// Base class for all rules.
    /// 
    /// A rule consists of a test and a target.  The DLR compiles sets of rules
    /// into dynamic sites providing fast dispatch to commonly invoked functionality.
    /// </summary>
    public abstract class StandardRule {
        internal Expression _test;                  // the test that determines if the rule is applicable for its parameters
        internal Expression _target;                // the target that executes if the rule is true
        internal Expression[] _parameters;          // the parameters which the rule is processing
        internal List<object> _templateData;        // the templated parameters for this rule 
        internal List<Function<bool>> _validators;  // the list of validates which indicate when the rule is no longer valid
        private bool _error;                        // true if the rule represents an error

        // TODO revisit these fields and their uses when LambdaExpression moves down
        internal Variable[] _paramVariables;        // TODO: Remove me when we can refer to params as expressions
        internal List<Variable> _temps;             // TODO: Remove me when ASTs can have free-floating variables
        internal AnalyzedRule _analyzed;            // TODO: Remove me when the above 2 are gone
        private bool _canInterpretTarget = true;    // true if we can interpret this rule

        internal StandardRule() { }

        /// <summary>
        /// An expression that should return true iff Target should be executed
        /// </summary>
        public Expression Test {
            get { return _test; }
            set { _test = value; }
        }

        /// <summary>
        /// The code to execute if the Test is true.
        /// </summary>
        public Expression Target {
            get { return _target; }
            set { _target = value; }
        }

        /// <summary>
        /// Adds a validation delegate which determines if the rule is still valid.
        /// 
        /// A validator provides a dynamic test that can invalidate a rule at runtime.  
        /// The definition of an invalid rule is one whose Test will always return false.  
        /// In theory a set of validators is not needed as this could be encoded in the 
        /// test itself; however, in practice it is much simpler to include these helpers.
        /// 
        /// The validator returns true if the rule should still be considered valid.
        /// </summary>
        public void AddValidator(Function<bool> validator) {
            if (_validators == null) _validators = new List<Function<bool>>();
            _validators.Add(validator);
        }

        /// <summary>
        /// Gets the logical parameters to the dynamic site in the form of Expressions.
        /// </summary>
        public IList<Expression> Parameters {
            get {
                return _parameters;
            }
        }

        /// <summary>
        /// Allocates a temporary variable for use during the rule.
        /// </summary>
        public Variable GetTemporary(Type type, string name) {
            if (_temps == null) {
                _temps = new List<Variable>();
            }
            Variable ret = Variable.Temporary(SymbolTable.StringToId(name), type);
            _temps.Add(ret);
            return ret;
        }

        public Expression MakeReturn(ActionBinder binder, Expression expr) {
            // we create a temporary here so that ConvertExpression doesn't need to (because it has no way to declare locals).
            if (expr.Type != typeof(void)) {
                Variable variable = GetTemporary(expr.Type, "$retVal");
                Expression read = Ast.ReadDefined(variable);
                Expression conv = binder.ConvertExpression(read, ReturnType);
                if (conv == read) return Ast.Return(expr);

                return Ast.Return(Ast.Comma(Ast.Assign(variable, expr), conv));
            }
            return Ast.Return(binder.ConvertExpression(expr, ReturnType));
        }

        public Expression MakeError(Expression expr) {
            if (expr != null) {
                // TODO: Change to ConvertHelper
                if (!TypeUtils.CanAssign(typeof(Exception), expr.Type)) {
                    expr = Ast.Convert(expr, typeof(Exception));
                }
            }

            _error = true;
            return Ast.Throw(expr);
        }

        public bool IsError {
            get {
                return _error;
            }
            internal set {
                _error = value;
            }
        }

        public void AddTest(Expression expression) {
            Assert.NotNull(expression);
            if (_test == null) {
                _test = expression;
            } else {
                _test = Ast.AndAlso(_test, expression);
            }
        }

        /// <summary>
        /// If not valid, this indicates that the given Test can never return true and therefore
        /// this rule should be removed from any RuleSets when convenient in order to 
        /// reduce memory usage and the number of active rules.
        /// </summary>
        public bool IsValid {
            get {
                if (_validators == null) return true;

                foreach (Function<bool> v in _validators) {
                    if (!v()) return false;
                }
                return true;
            }
        }

        public abstract Type ReturnType {
            get;
        }

        public void MakeTest(params Type[] types) {
            _test = MakeTestForTypes(types, 0);
        }

        /// <summary>
        /// Gets the logical parameters to the dynamic site in the form of Variables.
        /// </summary>
        internal Variable[] ParamVariables {
            get {
                return _paramVariables;
            }
        }

        /// <summary>
        /// The ActionBinder might prefer to interpret the target in some case. This property controls whether
        /// interpreting the target is OK, or if it needs to be compiled. 
        /// 
        /// If it needs to be compiled, the compiled target will be executed in an empty context, and so it will 
        /// not be able to view any variables set by the test. The caller who sets CanInterpretTarget=false is 
        /// responsible for ensuring that the test does not set any variables that the target depends on. 
        /// 
        /// The test should be such that it does not become invalid immediately. Otherwise, ActionBinder.UpdateSiteAndExecute 
        /// can potentially loop infinitely.
        /// 
        /// This should go away once the interpreter can support all the features required by the generated 
        /// target statements
        /// </summary>
        public bool CanInterpretTarget {
            get { return _canInterpretTarget; }
            set {
                // InterpretedMode has not yet been updated to deal with CanInterpretTarget=false
                // Debug.Assert(value = true || !EngineOptions.InterpretedMode);
                _canInterpretTarget = value;
            }
        }

        /// <summary>
        ///  Gets the temporary variables allocated by this rule.
        /// </summary>
        internal Variable[] TemporaryVariables {
            get {
                return _temps == null ? new Variable[] { } : _temps.ToArray();
            }
        }

        public static Expression MakeTypeTestExpression(Type t, Expression expr) {
            // we must always check for non-sealed types explicitly - otherwise we end up
            // doing fast-path behavior on a subtype which overrides behavior that wasn't
            // present for the base type.
            //TODO there's a question about nulls here
            if (CompilerHelpers.IsSealed(t) && t == expr.Type) {
                if (t.IsValueType) {
                    return Ast.True();
                }
                return Ast.NotEqual(expr, Ast.Null());
            }

            return Ast.AndAlso(
                Ast.NotEqual(
                    expr,
                    Ast.Null()),
                Ast.Equal(
                    Ast.Call(
                        Ast.ConvertHelper(expr, typeof(object)),
                        typeof(object).GetMethod("GetType")
                    ),
                    Ast.Constant(t)
                )
            );
        }

        /// <summary>
        /// Adds a templated constant that can enable code sharing across rules.
        /// </summary>
        public Expression AddTemplatedConstant(Type type, object value) {
            Contract.RequiresNotNull(type, "type");
            if (value != null) {
                if (!type.IsAssignableFrom(value.GetType())) {
                    throw new ArgumentException("type must be assignable from value");
                }
            } else {
                if (!type.IsValueType) {
                    throw new ArgumentException("value must not be null for value types");
                }
            }

            if (_templateData == null) _templateData = new List<object>(1);
            Type genType = typeof(TemplatedValue<>).MakeGenericType(type);
            object template = Activator.CreateInstance(genType, value, _templateData.Count);

            _templateData.Add(value);
            return Ast.ReadProperty(Ast.RuntimeConstant(template), genType.GetProperty("Value"));
        }

        public Expression AddTemplatedWeakConstant(Type type, object value) {
            if (value != null) {
                if (!type.IsAssignableFrom(value.GetType())) {
                    throw new ArgumentException("type must be assignable from value");
                }
            } else {
                if (!type.IsValueType) {
                    throw new ArgumentException("value must not be null for value types");
                }
            }

            Expression expr = AddTemplatedConstant(typeof(WeakReference), new WeakReference(value));

            return Ast.ConvertHelper(
                Ast.ReadProperty(expr, typeof(WeakReference).GetProperty("Target")),
                type
            );
        }

        internal int TemplateParameterCount {
            get {
                if (_templateData == null) return 0;
                return _templateData.Count;
            }
        }

        internal object[] TemplateData {
            get {
                return _templateData.ToArray();
            }
        }

        internal void RewriteTest(Expression test) {
            Debug.Assert(test != null && (object)test != (object)_test);
            _test = test;
        }

        internal void RewriteTarget(Expression target) {
            Debug.Assert(target != null && (object)target != (object)_test);
            _target = target;
        }

        public Expression MakeTestForTypes(Type[] types, int index) {
            Expression test = MakeTypeTest(types[index], index);
            if (index < types.Length - 1) {
                Expression nextTests = MakeTestForTypes(types, index + 1);
                if (ConstantCheck.IsConstant(test, true)) {
                    return nextTests;
                } else if (ConstantCheck.IsConstant(nextTests, true)) {
                    return test;
                } else {
                    return Ast.AndAlso(test, nextTests);
                }
            } else {
                return test;
            }
        }

        public Expression MakeTypeTest(Type type, int index) {
            return MakeTypeTest(type, Parameters[index]);
        }

        public Expression MakeTypeTest(Type type, Expression tested) {
            if (type == null || type == typeof(None)) {
                return Ast.Equal(tested, Ast.Null());
            }

            return MakeTypeTestExpression(type, tested);
        }

        /// <summary>
        /// Gets the number of logical parameters the dynamic site is provided with.
        /// </summary>
        public int ParameterCount {
            get {
                return _parameters.Length;
            }
        }


        public Expression MakeTypeTestExpression(Type t, int param) {
            return MakeTypeTestExpression(t, Parameters[param]);
        }

#if DEBUG
        public string Dump {
            get {
                using (System.IO.StringWriter writer = new System.IO.StringWriter()) {
                    AstWriter.Dump(Test, "Test", writer);
                    writer.WriteLine();
                    AstWriter.Dump(Target, "Target", writer);
                    return writer.ToString();
                }
            }
        }
#endif
    }

    /// <summary>
    /// A rule is the mechanism that LanguageBinders use to specify both what code to execute (the Target)
    /// for a particular action on a particular set of objects, but also a Test that guards the Target.
    /// Whenver the Test returns true, it is assumed that the Target will be the correct action to
    /// take on the arguments.
        /// 
    /// In the current design, a StandardRule is also used to provide a mini binding scope for the
    /// parameters and temporary variables that might be needed by the Test and Target.  This will
    /// probably change in the future as we unify around the notion of Lambdas.
    /// </summary>
    /// <typeparam name="T">The type of delegate for the DynamicSites this rule may apply to.</typeparam>
    public class StandardRule<T> : StandardRule {
        private SmallRuleSet<T> _monomorphicRuleSet;

        public StandardRule() {
            int firstParameter = DynamicSiteHelpers.IsFastTarget(typeof(T)) ? 1 : 2;
            
            ParameterInfo[] pis = typeof(T).GetMethod("Invoke").GetParameters();
            if (!DynamicSiteHelpers.IsBigTarget(typeof(T))) {
                _parameters = new Expression[pis.Length - firstParameter];
                List<Variable> paramVars = new List<Variable>();
                for (int i = firstParameter; i < pis.Length; i++) {
                    Variable p = MakeParameter(i, "$arg" + (i - firstParameter), pis[i].ParameterType);
                    paramVars.Add(p);
                    _parameters[i - firstParameter] = Ast.ReadDefined(p);
                }
                _paramVariables = paramVars.ToArray();
            } else {
                MakeTupleParameters(firstParameter, typeof(T).GetGenericArguments()[0]);
            }
        }

        private void MakeTupleParameters(int firstParameter, Type tupleType) {
            int count = Tuple.GetSize(tupleType);

            Variable tupleVar = MakeParameter(firstParameter, "$arg0", tupleType);
            _paramVariables = new Variable[] { tupleVar };
            Expression tuple = Ast.ReadDefined(tupleVar);

            _parameters = new Expression[count];
            for (int i = 0; i < _parameters.Length; i++) {
                Expression tupleAccess = tuple;
                foreach (PropertyInfo pi in Tuple.GetAccessPath(tupleType, i)) {
                    tupleAccess = Ast.ReadProperty(tupleAccess, pi);
                }
                _parameters[i] = tupleAccess;
            }
        }

        private Variable MakeParameter(int index, string name, Type type) {
            Variable ret = Variable.Parameter(SymbolTable.StringToId(name), type);
            ret.ParameterIndex = index;
            return ret;
        }

        /// <summary>
        /// Each rule holds onto an immutable RuleSet that contains this rule only.
        /// This should heavily optimize monomorphic call sites.
        /// </summary>
        internal SmallRuleSet<T> MonomorphicRuleSet {
            get {
                if (_monomorphicRuleSet == null) {
                    _monomorphicRuleSet = new SmallRuleSet<T>(new StandardRule<T>[] { this });
                }
                return _monomorphicRuleSet;
            }
        }

        /// <summary>
        /// Execute the target of the rule (in either interpreted or compiled mode)
        /// </summary>
        internal object ExecuteTarget(object site, CodeContext context, object [] args) {
            if (CanInterpretTarget) {
                // Interpret the target in the common case
                return Interpreter.Execute(context, Target);
            }

            // The target cannot be interpreted. We will execute the compiled rule. However, this will
            // include the test as well. The caller who sets CanInterpretTarget=false is responsible
            // for ensuring that the test is (mostly) idempotent.

            // Caller should have packaged up arguments for BigTarget
            Debug.Assert(!DynamicSiteHelpers.IsBigTarget(typeof(T)) || (args[0] is Tuple));

            T targetDelegate = MonomorphicRuleSet.GetOrMakeTarget(context);

            object[] prefixArgs;
            if (DynamicSiteHelpers.IsFastTarget(typeof(T))) {
                prefixArgs = new object[] { site };
            } else {
                prefixArgs = new object[] { site, context };
            }

            args = ArrayUtils.AppendRange(prefixArgs, args);

            try {
                return typeof(T).GetMethod("Invoke").Invoke(targetDelegate, args);
            } catch (TargetInvocationException e) {
                // Unwrap the real (inner) exception and raise it
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        /// <summary>
        /// Emits the test and target of the rule emitting the code using the provided
        /// compiler, branching to ifFalse if the test is not satisfied.
        /// </summary>
        internal void Emit(LambdaCompiler cg, Label ifFalse) {
            Assert.NotNull(_test, _target);

            // Need to make sure we aren't generating into two different CodeGens at the same time
            lock (this) {
                // First, finish binding my variable references
                // And rewrite the AST if needed
                if (_analyzed == null) {
                    AstRewriter.RewriteRule(this);
                    _analyzed = RuleBinder.Bind(_test, _target, ReturnType);
                }

                LambdaInfo top = _analyzed.Top;
                Compiler tc = new Compiler(_analyzed);

                cg.InitializeRule(tc, top);

                foreach (VariableReference vr in top.References.Values) {
                    vr.CreateSlot(cg, top);
                }

                if (_test != null) {
                    cg.EmitBranchFalse(_test, ifFalse);
                }

                // Now do the generation
                cg.EmitExpression(_target);

                // free any temps now that we're done generating
                // TODO: Keep temp slots aside sot that they can be freed
                //if (_temps != null) {
                //    foreach (Variable vr in _temps) {
                //        cg.FreeLocalTmp(vr.Slot);
                //    }
                //}
            }
        }

        [Confined]
        public override string/*!*/ ToString() {
            return string.Format("StandardRule({0})", _target);
        }

        public override Type ReturnType {
            get {
                return typeof(T).GetMethod("Invoke").ReturnType;
            }
        }                   

        #region Factory Methods

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")] // TODO: fix
        public static StandardRule<T> Simple(ActionBinder binder, MethodBase target, params Type[] types) {
            StandardRule<T> ret = new StandardRule<T>();
            BindingTarget bindingTarget = MethodBinder.MakeBinder(binder, target.Name, new MethodBase[] { target }).MakeBindingTarget(CallType.None, types);

            ret.MakeTest(types);
            ret.Target = ret.MakeReturn(binder, bindingTarget.MakeExpression(ret, ret.Parameters));
            return ret;
        }        
        
        #endregion


        /// <summary>
        /// Returns a TemplatedRuleBuilder which can be used to replace data.  See TemplatedRuleBuilder
        /// for more information.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public TemplatedRuleBuilder<T> GetTemplateBuilder() {
            if (_test == null || _target == null) throw new InvalidOperationException();
            if (_templateData == null) throw new InvalidOperationException("no template arguments created");

            return new TemplatedRuleBuilder<T>(this);
        }
    }
}
 
