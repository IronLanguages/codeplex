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
using System.Collections.ObjectModel;
using System.Reflection;

using Microsoft.Contracts;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Rule Builder
    /// 
    /// Rule builder is produced by the action binders. The DLR finalizes them into Rules
    /// which are cached in the dynamic sites and provide means for fast dispatch to commonly
    /// invoked functionality.
    /// </summary>
    public abstract class SimpleRuleBuilder {
        /// <summary>
        /// List of compile time parameter types (extracted from the delegate type)
        /// </summary>
        private readonly ReadOnlyCollection<Type> _compile;

        /// <summary>
        /// List of runtime parameter types (extracted from the actual runtime values)
        /// </summary>
        private readonly ReadOnlyCollection<Type> _runtime;

        /// <summary>
        /// Representation of the parameters to the dynamic operation
        /// </summary>
        private readonly ReadOnlyCollection<ParameterExpression> _parameters;

        /// <summary>
        /// The array of parameters. This is wrapped in the ReadOnlyCollection above
        /// for public consumption.
        /// </summary>
        private readonly ParameterExpression[] _params;

        /// <summary>
        /// Return type of the rule
        /// </summary>
        private readonly Type _returnType;

        /// <summary>
        /// Test which determines whether this rule is applicable to the parameters
        /// </summary>
        private Expression _test;

        /// <summary>
        /// The actual operation to be performed on the parameters
        /// </summary>
        private Expression _target;

        /// <summary>
        /// 
        /// </summary>
        internal SimpleRuleBuilder(Type type, object[] values)
            : this(type, GetRuntimeTypes(values)) {
        }

        internal SimpleRuleBuilder(Type type, Type[] values) :
            this(type, CollectionUtils.ToReadOnlyCollection(values)) {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        private SimpleRuleBuilder(Type type, ReadOnlyCollection<Type> types) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(typeof(Delegate).IsAssignableFrom(type), "type");
            ContractUtils.RequiresNotNull(types, "types");

            // First parameter is the dynamic site so the first 'real' parameter
            const int FirstParameterIndex = 1;

            MethodInfo invoke = type.GetMethod("Invoke");
            ParameterInfo[] pis = invoke.GetParameters();
            ContractUtils.Requires(pis.Length > 0 && pis[0].ParameterType == typeof(CallSite), "type");

            int length = pis.Length - FirstParameterIndex;

            ContractUtils.Requires(types.Count == length, "types");

            ParameterExpression[] @params = new ParameterExpression[length];
            Type[] compile = new Type[length];

            for (int i = 0; i < length; i++) {
                Type pt = pis[i + FirstParameterIndex].ParameterType;
                @params[i] = Expression.Parameter(pt, "$arg" + i);
                compile[i] = pt;
            }

            _returnType = invoke.ReturnType;
            _compile = new ReadOnlyCollection<Type>(compile);
            _runtime = types;
            _params = @params;
            _parameters = new ReadOnlyCollection<ParameterExpression>(@params);
        }

        [Confined]
        public override string/*!*/ ToString() {
            return string.Format("SimpleRuleBuilder({0})", _target ?? (object)"(empty)");
        }

        /// <summary>
        /// Runtime types of the arguments actually passed in to the operation.
        /// </summary>
        public ReadOnlyCollection<Type> RuntimeTypes {
            get {
                return _runtime;
            }
        }

        /// <summary>
        /// Compile time types of the arguments (determined from the delegate type)
        /// </summary>
        public ReadOnlyCollection<Type> CompileTimeTypes {
            get {
                return _compile;
            }
        }

        /// <summary>
        /// Gets the logical parameters to the dynamic site in the form of Expressions.
        /// </summary>
        public ReadOnlyCollection<ParameterExpression> Parameters {
            get {
                return _parameters;
            }
        }

        internal ParameterExpression[] ParameterArray {
            get {
                return _params;
            }
        }

        /// <summary>
        /// Return type of the rule.
        /// </summary>
        public Type ReturnType {
            get {
                return _returnType;
            }
        }

        /// <summary>
        /// Test which determines whether this rule is applicable to the parameters.
        /// Test must be of type bool.
        /// </summary>
        public Expression Test {
            get {
                return _test;
            }
        }

        /// <summary>
        /// The actual operation to be performed on the parameters
        /// </summary>
        public Expression Target {
            get {
                return _target;
            }
        }

        /// <summary>
        /// Sets the Target to the value, plus adds conversion if necessary.
        /// </summary>
        /// <param name="value">Value to return from the rule</param>
        public void Return(Expression value) {
            ContractUtils.RequiresNotNull(value, "value");

            _target = Expression.Return(
                Expression.ConvertHelper(
                    value,
                    ReturnType
                )
            );
        }

        /// <summary>
        /// Creates Default test
        /// </summary>
        public void CreateRuntimeTypeTest() {
            _test = CreateTest(_parameters, _runtime);
        }

        /// <summary>
        /// Adds a condition to a test.
        /// </summary>
        /// <param name="test"></param>
        public void AddToTest(Expression test) {
            if (test != null) {
                ContractUtils.Requires(TypeUtils.IsBool(test.Type), "test");

                if (_test == null) {
                    _test = test;
                } else {
                    _test = Expression.AndAlso(_test, test);
                }
            }
        }

        /// <summary>
        /// Creates one type identity test 
        /// </summary>
        /// <param name="parameter">Expression representing the rule's parameter</param>
        /// <param name="rt">The runtime type of the value</param>
        /// <returns>Expression or null if no test necessary.</returns>
        private static Expression CreateOneTest(ParameterExpression parameter, Type rt) {
            Type ct = parameter.Type;

            if (ct == rt) {
                if (ct.IsValueType) {
                    // No test necessary for value types
                    return null;
                }
                if (ct.IsSealed) {
                    // Sealed type is easy, just check for null
                    return Expression.NotEqual(parameter, Expression.Null());
                }
            }

            if (rt == typeof(None)) {
                return Expression.Equal(parameter, Expression.Null(parameter.Type));
            }

            return Expression.AndAlso(
                Expression.NotEqual(parameter, Expression.Null()),
                Expression.Equal(
                    Expression.Call(
                        Expression.ConvertHelper(parameter, typeof(object)),
                        typeof(object).GetMethod("GetType")
                    ),
                    Expression.Constant(rt)
                )
            );
        }

        /// <summary>
        /// Creates standard type identity test.
        /// </summary>
        /// <param name="parameters">Parameter expressions representing the rule's parameters</param>
        /// <param name="runtime">Runtime types of the actual arguments</param>
        /// <returns></returns>
        private static Expression CreateTest(ReadOnlyCollection<ParameterExpression> parameters, ReadOnlyCollection<Type> runtime) {
            Expression test = null;

            for (int i = 0; i < parameters.Count; i++) {
                Expression one = CreateOneTest(parameters[i], runtime[i]);

                if (one != null) {
                    if (test == null) {
                        test = one;
                    } else {
                        test = Expression.AndAlso(test, one);
                    }
                }
            }

            if (test == null) {
                test = Expression.True();
            }

            return test;
        }

        private static ReadOnlyCollection<Type> GetRuntimeTypes(object[] values) {
            ContractUtils.RequiresNotNull(values, "values");
            int length = values.Length;

            Type[] types = new Type[length];
            for (int i = 0; i < length; i++) {
                object value = values[i];
                types[i] = value != null ? value.GetType() : typeof(None);
            }

            return new ReadOnlyCollection<Type>(types);
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
    /// The generic SimpleRuleBuilder which gets specialized via the delegate type
    /// of the site it serves.
    /// </summary>
    /// <typeparam name="T">Delegate type</typeparam>
    public class SimpleRuleBuilder<T> : SimpleRuleBuilder where T : class {
        public SimpleRuleBuilder(object[] values)
            : base(typeof(T), values) {
        }

        public SimpleRuleBuilder(Type[] types)
            : base(typeof(T), types) {
        }

        /// <summary>
        /// Completes the rule from the builder.
        /// </summary>
        /// <returns>New rule</returns>
        public Rule<T> CreateRule() {
            Expression test = Test;
            Expression target = Target;

            if (test == null) {
                test = Expression.True();
            }
            if (target == null) {
                throw new InvalidOperationException("Missing target");
            }

            return new Rule<T>(
                Expression.Condition(
                    test,
                    Expression.ConvertHelper(target, typeof(void)),
                    Expression.Empty()
                ),
                null,
                null,
                ParameterArray
            );
        }

        public Rule<T> ReturnAndCreate(Expression value) {
            Return(value);
            return CreateRule();
        }

        public Rule<T> DefaultTestReturnAndCreate(Expression value) {
            CreateRuntimeTypeTest();
            Return(value);
            return CreateRule();
        }
    }
}
