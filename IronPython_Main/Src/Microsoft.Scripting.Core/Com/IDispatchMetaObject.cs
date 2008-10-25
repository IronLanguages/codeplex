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
#if !SILVERLIGHT // ComObject

using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.ComInterop {

    internal sealed class IDispatchMetaObject : MetaObject {
        private readonly ComTypeDesc _wrapperType;
        private readonly IDispatchComObject _self;

        internal IDispatchMetaObject(Expression expression, ComTypeDesc wrapperType, IDispatchComObject self)
            : base(expression, Restrictions.Empty, self) {
            _wrapperType = wrapperType;
            _self = self;
        }

        public override MetaObject BindInvokeMemberl(InvokeMemberBinder action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");

            ComMethodDesc methodDesc;

            if (_wrapperType.Funcs.TryGetValue(action.Name, out methodDesc)) {
                return new ComInvokeBinder(
                    action.Arguments,
                    args,
                    IDispatchRestriction(),
                    Expression.Constant(methodDesc),
                    Expression.Property(
                        Expression.Convert(Expression, typeof(IDispatchComObject)),
                        typeof(IDispatchComObject).GetProperty("DispatchObject")
                    ),
                    methodDesc
                ).Invoke();
            }

            return action.FallbackInvokeMember(UnwrapSelf(), args);
        }

        public override MetaObject BindConvert(ConvertBinder action) {
            ContractUtils.RequiresNotNull(action, "action");

            if (action.Type.IsInterface) {
                Expression result =
                    Expression.Convert(
                        Expression.Property(
                            Expression.ConvertHelper(Expression, typeof(IDispatchComObject)),
                            typeof(ComObject).GetProperty("Obj")
                        ),
                        action.Type
                    );

                return new MetaObject(
                    result,
                    IDispatchRestriction()
                );
            }

            return base.BindConvert(action);
        }

        public override MetaObject BindGetMember(GetMemberBinder action) {
            ContractUtils.RequiresNotNull(action, "action");

            ComMethodDesc method;
            ComEventDesc @event;

            // 1. Try methods
            if (_self.TryGetMemberMethod(action.Name, out method)) {
                return BindGetMember(method);
            }

            // 2. Try events
            if (_self.TryGetMemberEvent(action.Name, out @event)) {
                return BindEvent(@event);
            }

            // 3. Try methods explicitly by name
            if (_self.TryGetMemberMethodExplicit(action.Name, out method)) {
                return BindGetMember(method);
            }

            // 4. Fallback
            return action.FallbackGetMember(UnwrapSelf());
        }

        private MetaObject BindGetMember(ComMethodDesc method) {
            Restrictions restrictions = IDispatchRestriction();
            Expression dispatch =
                Expression.Property(
                    Expression.ConvertHelper(Expression, typeof(IDispatchComObject)),
                    typeof(IDispatchComObject).GetProperty("DispatchObject")
                );

            if (method.DispId != ComDispIds.DISPID_NEWENUM && method.IsPropertyGet) {
                if (method.Parameters.Length == 0) {                    
                    return new ComInvokeBinder(
                        new ArgumentInfo[0],
                        MetaObject.EmptyMetaObjects,
                        restrictions,
                        Expression.Constant(method),
                        dispatch,
                        method
                    ).Invoke();
                }
            }

            return new MetaObject(
                Expression.Call(
                    typeof(ComRuntimeHelpers).GetMethod("CreateDispCallable"),
                    dispatch,
                    Expression.Constant(method)
                ),
                restrictions
            );
        }

        private MetaObject BindEvent(ComEventDesc @event) {
            // BoundDispEvent CreateComEvent(object rcw, Guid sourceIid, int dispid)
            Expression result =
                Expression.Call(
                    typeof(ComRuntimeHelpers).GetMethod("CreateComEvent"),
                    Expression.Property(
                        Expression.ConvertHelper(Expression, typeof(IDispatchComObject)),
                        typeof(ComObject).GetProperty("Obj")
                    ),
                    Expression.Constant(@event.sourceIID),
                    Expression.Constant(@event.dispid)
                );

            return new MetaObject(
                result,
                IDispatchRestriction()
            );
        }

        public override MetaObject BindOperation(OperationBinder action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");

            switch (action.Operation) {
                case "GetItem":
                    return IndexOperation(action, args, "TryGetGetItem");
                case "SetItem":
                    return IndexOperation(action, args, "TryGetSetItem");
                case "Documentation":
                    return DocumentationOperation(args);
                case "Equals":
                    return EqualsOperation(args);
                case "GetMemberNames":
                    return GetMemberNames(args);
                default:
                    return base.BindOperation(action, args);
            }
        }

        private MetaObject IndexOperation(OperationBinder action, MetaObject[] args, string method) {
            MetaObject fallback = action.FallbackOperation(UnwrapSelf(), args);

            ParameterExpression callable = Expression.Variable(typeof(DispCallable), "callable");

            Expression[] callArgs = new Expression[args.Length];
            for (int i = 0; i < callArgs.Length; i++) {
                callArgs[i] = args[i].Expression;
            }
            callArgs[0] = callable;

            Expression result = Expression.Comma(
                new ParameterExpression[] { callable },
                Expression.Condition(
                    Expression.Call(
                        Expression.Convert(Expression, typeof(IDispatchComObject)),
                        typeof(IDispatchComObject).GetMethod(method),
                        callable
                    ),
                    Expression.Dynamic(new ComInvokeAction(), typeof(object), callArgs),
                    Expression.ConvertHelper(fallback.Expression, typeof(object))
                )
            );

            return new MetaObject(
                result,
                Restrictions.Combine(args).Merge(IDispatchRestriction()).Merge(fallback.Restrictions)
            );
        }

        private MetaObject DocumentationOperation(MetaObject[] args) {
            Expression result =
                Expression.Property(
                    Expression.ConvertHelper(args[0].Expression, typeof(ComObject)),
                    typeof(ComObject).GetProperty("Documentation")
                );

            return new MetaObject(
                result,
                Restrictions.Combine(args).Merge(IDispatchRestriction())
            );
        }

        private MetaObject EqualsOperation(MetaObject[] args) {
            Expression result =
                Expression.Call(
                    Expression.ConvertHelper(args[0].Expression, typeof(ComObject)),
                    typeof(ComObject).GetMethod("Equals"),
                    Expression.ConvertHelper(args[1].Expression, typeof(object))
                );

            return new MetaObject(
                result,
                Restrictions.Combine(args).Merge(IDispatchRestriction())
            );
        }

        private MetaObject GetMemberNames(MetaObject[] args) {
            Expression result =
                Expression.Property(
                    Expression.ConvertHelper(args[0].Expression, typeof(ComObject)),
                    typeof(ComObject).GetProperty("MemberNames")
                );

            return new MetaObject(
                result,
                Restrictions.Combine(args).Merge(IDispatchRestriction())
            );
        }

        public override MetaObject BindSetMember(SetMemberBinder action, MetaObject value) {
            ContractUtils.RequiresNotNull(action, "action");

            return 
                // 1. Check for simple property put
                TryPropertyPut(action, value) ??

                // 2. Check for event handler hookup where the put is dropped
                TryEventHandlerNoop(action, value) ??

                // 3. Go back to language
                action.FallbackSetMember(UnwrapSelf(), value);
        }

        private MetaObject TryPropertyPut(SetMemberBinder action, MetaObject value) {
            ComMethodDesc method;
            if (_self.TryGetPropertySetter(action.Name, out method)) {
                Restrictions restrictions = IDispatchRestriction();
                Expression dispatch =
                    Expression.Property(
                        Expression.ConvertHelper(Expression, typeof(IDispatchComObject)),
                        typeof(IDispatchComObject).GetProperty("DispatchObject")
                    );

                return new ComInvokeBinder(
                    new ArgumentInfo[0],
                    new[] { value },
                    restrictions,
                    Expression.Constant(method),
                    dispatch,
                    method
                ).Invoke();
            }

            return null;
        }

        private MetaObject TryEventHandlerNoop(SetMemberBinder action, MetaObject value) {
            ComEventDesc @event;
            if (_self.TryGetEventHandler(action.Name, out @event) && value.LimitType == typeof(BoundDispEvent)) {
                // Drop the event property set.
                return new MetaObject(
                    Expression.Null(),
                    value.Restrictions.Merge(IDispatchRestriction()).Merge(Restrictions.GetTypeRestriction(value.Expression, typeof(BoundDispEvent)))
                );
            }

            return null;
        }

        private Restrictions IDispatchRestriction() {
            Expression @this = Expression;
            return Restrictions.GetTypeRestriction(
                @this, typeof(IDispatchComObject)
            ).Merge(
                Restrictions.GetExpressionRestriction(
                    Expression.Equal(
                        Expression.Property(
                            Expression.Convert(@this, typeof(IDispatchComObject)),
                            typeof(IDispatchComObject).GetProperty("ComTypeDesc")
                        ),
                        Expression.Constant(_wrapperType)
                    )
                )
            );
        }

        private MetaObject UnwrapSelf() {
            return new MetaObject(
                Expression.Property(
                    Expression.ConvertHelper(Expression, typeof(ComObject)),
                    typeof(ComObject).GetProperty("Obj")
                ),
                IDispatchRestriction(),
                _self.Obj
            );
        }
    }
}

#endif
