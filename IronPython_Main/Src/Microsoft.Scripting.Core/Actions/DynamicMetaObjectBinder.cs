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
using Microsoft.Scripting.Utils;
using Microsoft.Linq.Expressions;
using Microsoft.Linq.Expressions.Compiler;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Runtime.Remoting;

namespace Microsoft.Scripting {
    /// <summary>
    /// The dynamic call site binder that participates in the <see cref="DynamicMetaObject"/> binding protocol.
    /// </summary>
    /// <remarks>
    /// The <see cref="CallSiteBinder"/> performs the binding of the dynamic operation using the runtime values
    /// as input. On the other hand, the <see cref="DynamicMetaObjectBinder"/> participates in the <see cref="DynamicMetaObject"/>
    /// binding protocol.
    /// </remarks>
    public abstract class DynamicMetaObjectBinder : CallSiteBinder {

        #region Standard Binder Kinds

        internal const int OperationBinderHash = 0x4000000;
        internal const int UnaryOperationBinderHash = 0x8000000;
        internal const int BinaryOperationBinderHash = 0xc000000;
        internal const int GetMemberBinderHash = 0x10000000;
        internal const int SetMemberBinderHash = 0x14000000;
        internal const int DeleteMemberBinderHash = 0x18000000;
        internal const int GetIndexBinderHash = 0x1c000000;
        internal const int SetIndexBinderHash = 0x20000000;
        internal const int DeleteIndexBinderHash = 0x24000000;
        internal const int InvokeMemberBinderHash = 0x28000000;
        internal const int ConvertBinderHash = 0x2c000000;
        internal const int CreateInstanceBinderHash = 0x30000000;
        internal const int InvokeBinderHash = 0x34000000;
        internal const int BinaryOperationOnMemberBinderHash = 0x38000000;
        internal const int BinaryOperationOnIndexBinderHash = 0x3c000000;
        internal const int UnaryOperationOnMemberBinderHash = 0x40000000;
        internal const int UnaryOperationOnIndexBinderHash = 0x44000000;

        #endregion

        #region Public APIs

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicMetaObjectBinder"/> class.
        /// </summary>
        protected DynamicMetaObjectBinder() {
        }

        /// <summary>
        /// Performs the runtime binding of the dynamic operation on a set of arguments.
        /// </summary>
        /// <param name="args">An array of arguments to the dynamic operation.</param>
        /// <param name="parameters">The array of <see cref="ParameterExpression"/> instances that represent the parameters of the call site in the binding process.</param>
        /// <param name="returnLabel">A LabelTarget used to return the result of the dynamic binding.</param>
        /// <returns>
        /// An Expression that performs tests on the dynamic operation arguments, and
        /// performs the dynamic operation if hte tests are valid. If the tests fail on
        /// subsequent occurrences of the dynamic operation, Bind will be called again
        /// to produce a new <see cref="Expression"/> for the new argument types.
        /// </returns>
        public sealed override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
            if (args.Length == 0) {
                throw new InvalidOperationException();
            }

            DynamicMetaObject target = DynamicMetaObject.Create(args[0], parameters[0]);
            DynamicMetaObject[] metaArgs = CreateArgumentMetaObjects(args, parameters);

            DynamicMetaObject binding = Bind(target, metaArgs);

            if (binding == null) {
                throw Error.BindingCannotBeNull();
            }

            Expression body = binding.Expression;
            BindingRestrictions restrictions = binding.Restrictions;

            // if the target is IDO, standard binders ask it to bind the rule so we may have a target-specific binding. 
            // it makes sense to restrict on the target's type in such cases.
            // ideally IDO metaobjects should do this, but they often miss that type of "this" is significant.
            if (IsStandardBinder && args[0] as IDynamicMetaObjectProvider != null) {
                VerifyRestrictions(restrictions);
            }

            restrictions = AddRemoteObjectRestrictions(restrictions, args, parameters);

            // Add the return
            if (body.NodeType != ExpressionType.Goto) {
                body = Expression.Return(returnLabel, Convert(body, returnLabel.Type));
            }

            // Finally, add restrictions
            if (restrictions != BindingRestrictions.Empty) {
                body = Expression.IfThen(restrictions.ToExpression(), body);
            }

            return body;
        }

        private static void VerifyRestrictions(BindingRestrictions restrictions) {
            if (restrictions.Equals(BindingRestrictions.Empty)) {
                throw new InvalidOperationException();
            }
        }

        private static DynamicMetaObject[] CreateArgumentMetaObjects(object[] args, ReadOnlyCollection<ParameterExpression> parameters) {
            DynamicMetaObject[] mos;
            if (args.Length != 1) {
                mos = new DynamicMetaObject[args.Length - 1];
                for (int i = 1; i < args.Length; i++) {
                    mos[i - 1] = DynamicMetaObject.Create(args[i], parameters[i]);
                }
            } else {
                mos = DynamicMetaObject.EmptyMetaObjects;
            }
            return mos;
        }

        private static BindingRestrictions AddRemoteObjectRestrictions(BindingRestrictions restrictions, object[] args, ReadOnlyCollection<ParameterExpression> parameters) {
#if !SILVERLIGHT

            for (int i = 0; i < parameters.Count; i++) {
                var expr = parameters[i];
                var value = args[i] as MarshalByRefObject;

                // special case for MBR objects.
                // when MBR objects are remoted they can have different conversion behavior
                // so bindings created for local and remote objects should not be mixed.
                if (value != null && !IsComObject(value)) {
                    BindingRestrictions remotedRestriction;
                    if (RemotingServices.IsObjectOutOfAppDomain(value)) {
                        remotedRestriction = BindingRestrictions.GetExpressionRestriction(
                            Expression.AndAlso(
                                Expression.NotEqual(expr, Expression.Constant(null)),
                                Expression.Call(
                                    typeof(RemotingServices).GetMethod("IsObjectOutOfAppDomain"),
                                    expr
                                )
                            )
                        );
                    } else {
                        remotedRestriction = BindingRestrictions.GetExpressionRestriction(
                            Expression.AndAlso(
                                Expression.NotEqual(expr, Expression.Constant(null)),
                                Expression.Not(
                                    Expression.Call(
                                        typeof(RemotingServices).GetMethod("IsObjectOutOfAppDomain"),
                                        expr
                                    )
                                )
                            )
                        );
                    }
                    restrictions = restrictions.Merge(remotedRestriction);
                }
            }

#endif
            return restrictions;
        }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the dynamic operation.
        /// </summary>
        /// <param name="target">The target of the dynamic operation.</param>
        /// <param name="args">An array of arguments of the dynamic operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public abstract DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args);

        /// <summary>
        /// Gets an expression that will cause the binding to be updated. It
        /// indicates that the expression's binding is no longer valid.
        /// This is typically used when the "version" of a dynamic object has
        /// changed.
        /// </summary>
        /// <param name="type">The <see cref="Expression.Type">Type</see> property of the resulting expression; any type is allowed.</param>
        /// <returns>The update expression.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public Expression GetUpdateExpression(Type type) {
            return Expression.Goto(CallSiteBinder.UpdateLabel, type);
        }

        /// <summary>
        /// Defers the binding of the operation until later time when the runtime values of all dynamic operation arguments have been computed.
        /// </summary>
        /// <param name="target">The target of the dynamic operation.</param>
        /// <param name="args">An array of arguments of the dynamic operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public DynamicMetaObject Defer(DynamicMetaObject target, params DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");

            if (args == null) {
                return MakeDeferred(target.Restrictions, target);
            } else {
                return MakeDeferred(
                    target.Restrictions.Merge(BindingRestrictions.Combine(args)),
                    args.AddFirst(target)
                );
            }
        }

        /// <summary>
        /// Defers the binding of the operation until later time when the runtime values of all dynamic operation arguments have been computed.
        /// </summary>
        /// <param name="args">An array of arguments of the dynamic operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public DynamicMetaObject Defer(params DynamicMetaObject[] args) {
            return MakeDeferred(BindingRestrictions.Combine(args), args);
        }

        private DynamicMetaObject MakeDeferred(BindingRestrictions rs, params DynamicMetaObject[] args) {
            var exprs = DynamicMetaObject.GetExpressions(args);

            Type delegateType = DelegateHelpers.MakeDeferredSiteDelegate(args, typeof(object));

            // Because we know the arguments match the delegate type (we just created the argument types)
            // we go directly to DynamicExpression.Make to avoid a bunch of unnecessary argument validation
            return new DynamicMetaObject(
                DynamicExpression.Make(typeof(object), delegateType, this, new TrueReadOnlyCollection<Expression>(exprs)),
                rs
            );
        }

        #endregion

        /// <summary>
        /// Converts the result of a dynamic operation to the given type.
        /// Void converts to default(T). This will not find a userdefined
        /// conversion method, and its error is specific to converting the
        /// result of a dynamic operation.
        /// </summary>
        internal static Expression Convert(Expression expression, Type type) {
            if (expression.Type == type) {
                return expression;
            }

            // Result type can be void but non-void expression.
            if (type == typeof(void)) {
                return Expression.Block(typeof(void), expression);
            }

            // Dynamic operations can convert void -> default(T)
            if (expression.Type == typeof(void)) {
                return Expression.Block(expression, Expression.Default(type));
            }

            // If we don't have a valid primtive conversion, it's an error.
            if (!TypeUtils.HasIdentityPrimitiveOrNullableConversion(expression.Type, type) &&
                !TypeUtils.HasReferenceConversion(expression.Type, type)) {
                throw Error.CannotConvertDynamicResult(expression.Type, type);
            }

            return Expression.Convert(expression, type);
        }

        // used to detect standard MetaObjectBinders.
        internal virtual bool IsStandardBinder {
            get {
                return false;
            }
        }

#if !SILVERLIGHT
        private static readonly Type ComObjectType = typeof(object).Assembly.GetType("System.__ComObject");
        private static bool IsComObject(object obj) {
            // we can't use System.Runtime.InteropServices.Marshal.IsComObject(obj) since it doesn't work in partial trust
            return obj != null && ComObjectType.IsAssignableFrom(obj.GetType());
        }
#endif

    }
}
