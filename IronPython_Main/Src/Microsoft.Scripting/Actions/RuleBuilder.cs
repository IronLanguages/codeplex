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

using System; using Microsoft;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Contracts;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Linq.Expressions.Expression;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public VariableExpression GetTemporary(Type type, string name) {
            VariableExpression t = Expression.Variable(type, name);
            AddTemporary(t);
            return t;
        }

        public void AddTemporary(VariableExpression variable) {
            ContractUtils.RequiresNotNull(variable, "variable");
            if (_temps == null) {
                _temps = new List<VariableExpression>();
            }
            _temps.Add(variable);
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
                if (ConstantCheck.Check(test, true)) {
                    return nextTests;
                } else if (ConstantCheck.Check(nextTests, true)) {
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
                return _parameters.Length;
            }
        }

        public Expression MakeTypeTestExpression(Type t, int param) {
            return MakeTypeTestExpression(t, Parameters[param]);
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
            int count = pis.Length - 1;
            ParameterExpression[] vars = new ParameterExpression[count];

            for (int i = 0; i < count; i++) {
                // First argument is the dynamic site
                vars[i] = Ast.Parameter(pis[i + 1].ParameterType, "$arg" + i);
            }

            _paramVariables = vars;

            if (vars.Length > 0 && typeof(CodeContext).IsAssignableFrom(vars[0].Type)) {
                _context = vars[0];
                _parameters = ArrayUtils.RemoveAt(vars, 0);
            } else {
                _parameters = vars;
            }
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
                    new ReadOnlyCollection<ParameterExpression>(_paramVariables)
                );
            }

            return _rule;
        }
    }
}
