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
using System.Threading;
using System.Diagnostics;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;

    /// <summary>
    /// Rule Builder
    /// 
    /// Rule builder is produced by the action binders. The DLR finalizes them into Rules
    /// which are cached in the dynamic sites and provide means for fast dispatch to commonly
    /// invoked functionality.
    /// </summary>
    public abstract class RuleBuilder {
        internal Expression _test;                  // the test that determines if the rule is applicable for its parameters
        internal Expression _target;                // the target that executes if the rule is true
        internal Expression[] _parameters;          // the parameters which the rule is processing
        internal List<object> _templateData;        // the templated parameters for this rule 
        internal List<Function<bool>> _validators;  // the list of validates which indicate when the rule is no longer valid
        private bool _error;                        // true if the rule represents an error

        // TODO revisit these fields and their uses when LambdaExpression moves down
        internal ParameterExpression[] _paramVariables;       // TODO: Remove me when we can refer to params as expressions
        internal List<VariableExpression> _temps;             // TODO: Remove me when ASTs can have free-floating variables

        internal RuleBuilder() { }

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
        public VariableExpression GetTemporary(Type type, string name) {
            if (_temps == null) {
                _temps = new List<VariableExpression>();
            }
            VariableExpression ret = VariableExpression.Temporary(SymbolTable.StringToId(name), type);
            _temps.Add(ret);
            return ret;
        }

        public Expression MakeReturn(ActionBinder binder, Expression expr) {
            // we create a temporary here so that ConvertExpression doesn't need to (because it has no way to declare locals).
            if (expr.Type != typeof(void)) {
                VariableExpression variable = GetTemporary(expr.Type, "$retVal");
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

        public abstract Type ReturnType {
            get;
        }

        public void MakeTest(params Type[] types) {
            _test = MakeTestForTypes(types, 0);
        }

        /// <summary>
        /// Gets the logical parameters to the dynamic site in the form of Variables.
        /// </summary>
        internal ParameterExpression[] ParamVariables {
            get { return _paramVariables; }
        }

        /// <summary>
        ///  Gets the temporary variables allocated by this rule.
        /// </summary>
        internal VariableExpression[] TemporaryVariables {
            get {
                return _temps == null ? new VariableExpression[] { } : _temps.ToArray();
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
    /// In the current design, a RuleBuilder is also used to provide a mini binding scope for the
    /// parameters and temporary variables that might be needed by the Test and Target.  This will
    /// probably change in the future as we unify around the notion of Lambdas.
    /// </summary>
    /// <typeparam name="T">The type of delegate for the DynamicSites this rule may apply to.</typeparam>
    public class RuleBuilder<T> : RuleBuilder {
        private Rule<T> _rule;              // Completed rule

        public RuleBuilder() {
            int firstParameter = GetFirstParameterIndex();

            ParameterInfo[] pis = typeof(T).GetMethod("Invoke").GetParameters();
            if (!DynamicSiteHelpers.IsBigTarget(typeof(T))) {
                _parameters = new Expression[pis.Length - firstParameter];
                List<ParameterExpression> paramVars = new List<ParameterExpression>();
                for (int i = firstParameter; i < pis.Length; i++) {
                    ParameterExpression p = Ast.Parameter(pis[i].ParameterType, "$arg" + (i - firstParameter));
                    paramVars.Add(p);
                    _parameters[i - firstParameter] = p;
                }
                _paramVariables = paramVars.ToArray();
            } else {
                MakeTupleParameters(typeof(T).GetGenericArguments()[0]);
            }
        }

        private static int GetFirstParameterIndex() {
            return DynamicSiteHelpers.IsFastTarget(typeof(T)) ? 1 : 2;
        }

        private void MakeTupleParameters(Type tupleType) {
            int count = Tuple.GetSize(tupleType);

            ParameterExpression tupleVar = Ast.Parameter(tupleType, "$arg0");
            _paramVariables = new ParameterExpression[] { tupleVar };

            _parameters = new Expression[count];
            for (int i = 0; i < _parameters.Length; i++) {
                Expression tupleAccess = tupleVar;
                foreach (PropertyInfo pi in Tuple.GetAccessPath(tupleType, i)) {
                    tupleAccess = Ast.ReadProperty(tupleAccess, pi);
                }
                _parameters[i] = tupleAccess;
            }
        }

        [Confined]
        public override string/*!*/ ToString() {
            return string.Format("RuleBuilder({0})", _target);
        }

        public override Type ReturnType {
            get {
                return typeof(T).GetMethod("Invoke").ReturnType;
            }
        }

        #region Factory Methods

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")] // TODO: fix
        public static RuleBuilder<T> Simple(ActionBinder binder, MethodBase target, params Type[] types) {
            RuleBuilder<T> ret = new RuleBuilder<T>();
            BindingTarget bindingTarget = MethodBinder.MakeBinder(binder, target.Name, new MethodBase[] { target }).MakeBindingTarget(CallTypes.None, types);

            ret.MakeTest(types);
            ret.Target = ret.MakeReturn(binder, bindingTarget.MakeExpression(ret, ret.Parameters));
            return ret;
        }

        #endregion

        internal Rule<T> CreateRule() {
            if (_rule == null) {
                //
                // Extract the values of interest from the RuleBuilder. We do this for values that we access
                // more than once so that we get _some_ consistent state. Alternative would be to introduce
                // locking. Since we are doing validation on the rule (stack spiller, binder and code gen),
                // this seems sufficient for now.
                //
                List<Function<bool>> validators = _validators;
                List<object> template = _templateData;
                List<VariableExpression> temps = _temps;

                Rule<T> rule = new Rule<T>(
                    Ast.Condition(
                        _test,
                        Ast.ConvertHelper(_target, typeof(void)),
                        Ast.Empty()
                    ),
                    validators != null ? validators.ToArray() : null,
                    template != null ? template.ToArray() : null,
                    _paramVariables,
                    temps != null ? temps.ToArray() : null
                );

                rule = StackSpiller.AnalyzeRule<T>(rule);

                Interlocked.CompareExchange<Rule<T>>(ref _rule, rule, null);
            }

            return _rule;
        }

        /// <summary>
        /// Provides support for creating rules which can have runtime constants 
        /// replaced without recompiling the rule.
        /// 
        /// Template parameters can be added to a rule by calling 
        /// RuleBuilder.AddTemplatedConstant with a type and the value for the 
        /// current rule.  When the first templated rule is finished being constructed 
        /// calling RuleBuilder.GetTemplateBuilder returns a template builder which 
        /// can be used on future rules.
        /// 
        /// For future template requests the rule stil needs to be generated 
        /// (this is a current limitation due to needing to have a version
        /// of the AST w/ the correct constants at evaluation time).  Call 
        /// TempalatedRuleBuilder.MakeRuleFromTemplate with the new template parameters 
        /// (in the same order as AddTemplatedConstant was called) and the rule will be updated to 
        /// to enable code sharing.
        /// </summary>
        public void CopyTemplateToRule(RuleBuilder<T> builder) {
            Rule<T> from = CreateRule();
            Delegate target = (Delegate)(object)from.MonomorphicRuleSet.GetOrMakeTarget();
            Rule<T> to = builder.CreateRule();
            to.MonomorphicRuleSet.RawTarget = CloneDelegate(to.Template, target);
        }

        private T CloneDelegate(object[] newData, Delegate existingDelegate) {
            T dlg;
            DynamicMethod templateMethod = _rule.MonomorphicRuleSet.MonomorphicTemplate;
            if (templateMethod != null) {
                dlg = (T)(object)templateMethod.CreateDelegate(typeof(T), CloneData(existingDelegate.Target, newData));
            } else {
                dlg = (T)(object)Delegate.CreateDelegate(typeof(T), CloneData(existingDelegate.Target, newData), existingDelegate.Method);
            }
            return dlg;
        }

        /// <summary>
        /// Clones the delegate target to create new delegate around it.
        /// The delegates created by the compiler are closed over the instance of Closure class.
        /// </summary>
        private object CloneData(object data, params object[] newData) {
            Debug.Assert(data != null);

            Closure closure = data as Closure;
            if (closure != null) {
                return new Closure(closure.Context, CopyArray(newData, closure.Constants));
            }

            throw new InvalidOperationException("bad data bound to delegate");
        }

        private static object[] CopyArray(object[] newData, object[] oldData) {
            object[] res = new object[oldData.Length];
            for (int i = 0; i < oldData.Length; i++) {
                ITemplatedValue itv = oldData[i] as ITemplatedValue;
                if (itv == null) {
                    res[i] = oldData[i];
                    continue;
                }

                res[i] = itv.CopyWithNewValue(newData[itv.Index]);
            }
            return res;
        }
    }
}
