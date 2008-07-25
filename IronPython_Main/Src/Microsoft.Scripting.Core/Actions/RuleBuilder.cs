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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using Microsoft.Contracts;

namespace System.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

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
        internal Expression _context;               // CodeContext, if any.
        internal Expression[] _parameters;          // the parameters which the rule is processing
        internal Expression[] _parametersMinusSite; // the parameters which the rule is processing minus the CallSite parameter
        internal Expression[] _allParameters;       // The parameters, including CodeContext, if any.
        internal List<Func<bool>> _validators;  // the list of validates which indicate when the rule is no longer valid
        private bool _error;                        // true if the rule represents an error
        internal List<VariableExpression> _temps;    // temporaries allocated by the rule

        // TODO revisit these fields and their uses when LambdaExpression moves down
        internal ParameterExpression[] _paramVariables;       // TODO: Remove me when we can refer to params as expressions

        internal RuleBuilder() { }

        /// <summary>
        /// An expression that should return true iff Target should be executed
        /// </summary>
        public Expression Test {
            get { return _test; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                ContractUtils.Requires(TypeUtils.IsBool(value.Type), "value", Strings.TypeOfTestMustBeBool);
                _test = value;
            }
        }

        /// <summary>
        /// The code to execute if the Test is true.
        /// </summary>
        public Expression Target {
            get { return _target; }
            set { _target = value; }
        }

        public Expression Context {
            get {
                return _context;
            }
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
        public void AddValidator(Func<bool> validator) {
            if (_validators == null) _validators = new List<Func<bool>>();
            _validators.Add(validator);
        }

        /// <summary>
        /// Gets the logical parameters to the dynamic site in the form of Expressions.
        /// </summary>
        public IList<Expression> Parameters {
            get {
                return _parametersMinusSite;
            }
        }

        public IList<Expression> AllParameters {
            get {
                return _allParameters;
            }
        }

        /// <summary>
        /// Allocates a temporary variable for use during the rule.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public VariableExpression GetTemporary(Type type, string name) {
            if (_temps == null) {
                _temps = new List<VariableExpression>();
            }
            VariableExpression t = Expression.Variable(type, name);
            _temps.Add(t);
            return t;
        }

        public Expression MakeReturn(ActionBinder binder, Expression expr) {
            // we create a temporary here so that ConvertExpression doesn't need to (because it has no way to declare locals).
            if (expr.Type != typeof(void)) {
                VariableExpression variable = GetTemporary(expr.Type, "$retVal");
                Expression conv = binder.ConvertExpression(variable, ReturnType, ConversionResultKind.ExplicitCast, Context);
                if (conv == variable) return Ast.Return(expr);

                return Ast.Return(Ast.Comma(Ast.Assign(variable, expr), conv));
            }
            return Ast.Return(binder.ConvertExpression(expr, ReturnType, ConversionResultKind.ExplicitCast, Context));
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
            set {
                _error = value;
            }
        }

        public void AddTest(Expression expression) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.Requires(TypeUtils.IsBool(expression.Type), "expression", Strings.TypeOfExpressionMustBeBool);

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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
                return _parameters.Length - 1;
            }
        }

        public Expression MakeTypeTestExpression(Type t, int param) {
            return MakeTypeTestExpression(t, Parameters[param]);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private string Dump {
            get {
                using (System.IO.StringWriter writer = new System.IO.StringWriter()) {
                    ExpressionWriter.Dump(Test, "Test", writer);
                    writer.WriteLine();
                    ExpressionWriter.Dump(Target, "Target", writer);
                    return writer.ToString();
                }
            }
        }
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
    public class RuleBuilder<T> : RuleBuilder where T : class {
        /// <summary>
        /// Completed rule
        /// </summary>
        private Rule<T> _rule;

        public RuleBuilder() {

            if (!typeof(Delegate).IsAssignableFrom(typeof(T))) {
                throw Error.TypeParameterIsNotDelegate(typeof(T));
            }

            ParameterInfo[] pis = typeof(T).GetMethod("Invoke").GetParameters();

            if (pis.Length == 0 || pis[0].ParameterType != typeof(CallSite)) {
                throw Error.FirstArgumentMustBeCallSite();
            }

            MakeParameters(pis);
        }

        private void MakeParameters(ParameterInfo[] pis) {
            // First argument is the dynamic site
            const int FirstParameterIndex = 1;

            Expression[] all = new Expression[pis.Length];
            ParameterExpression[] vars = new ParameterExpression[pis.Length];

            vars[0] = Ast.Parameter(typeof(CallSite), "callSite");
            for (int i = FirstParameterIndex; i < pis.Length; i++) {
                all[i] = vars[i] = Ast.Parameter(pis[i].ParameterType, "$arg" + i);
            }

            _paramVariables = vars;
            _allParameters = all;

            if (all.Length > 1 && typeof(CodeContext).IsAssignableFrom(all[1].Type)) {
                _context = all[1];
                _parameters = ArrayUtils.RemoveAt(all, 1);
            } else {
                _parameters = all;
            }

            _parametersMinusSite = ArrayUtils.RemoveFirst(_parameters);
        }

        [Confined]
        public override string ToString() {
            return string.Format("RuleBuilder({0})", _target);
        }

        public override Type ReturnType {
            get {
                return typeof(T).GetMethod("Invoke").ReturnType;
            }
        }

        public Rule<T> CreateRule() {
            if (_rule == null) {
                if (_test == null) {
                    throw Error.MissingTest();
                }
                if (_target == null) {
                    throw Error.MissingTarget();
                }

                _rule = new Rule<T>(
                    Expression.Scope(
                        Expression.Condition(
                            _test,
                            Ast.ConvertHelper(_target, typeof(void)),
                            Ast.Empty()
                        ),
                        "<rule>",
                        _temps != null ? _temps.ToArray() : new VariableExpression[0]
                    ),
                    RuleValidator.Create(_validators),
                    new ReadOnlyCollection<ParameterExpression>(_paramVariables)
                );
            }

            return _rule;
        }
    }
}
