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

namespace Microsoft.Scripting.Com {

    internal sealed class IDispatchMetaObject : MetaObject {
        private readonly ComTypeDesc _wrapperType;
        private readonly IDispatchComObject _self;

        internal IDispatchMetaObject(Expression expression, ComTypeDesc wrapperType, IDispatchComObject self)
            : base(expression, Restrictions.Empty, self) {
            _wrapperType = wrapperType;
            _self = self;
        }

        public override MetaObject Call(CallAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");

            ComMethodDesc methodDesc;

            if (_wrapperType.Funcs.TryGetValue(action.Name, out methodDesc)) {
                return new InvokeBinder(
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

            return action.Fallback(Unwrap(args));
        }

        public override MetaObject Convert(ConvertAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");

            if (action.ToType.IsInterface) {
                Expression result =
                    Expression.Convert(
                        Expression.Property(
                            Expression.ConvertHelper(args[0].Expression, typeof(IDispatchComObject)),
                            typeof(ComObject).GetProperty("Obj")
                        ),
                        action.ToType
                    );

                return new MetaObject(
                    result,
                    Restrictions.Combine(args).Merge(IDispatchRestriction())
                );
            }

            return base.Convert(action, args);
        }

        public override MetaObject Create(CreateAction action, MetaObject[] args) {
            return base.Create(action, args);
        }

        public override MetaObject DeleteMember(DeleteMemberAction action, MetaObject[] args) {
            return base.DeleteMember(action, args);
        }

        public override MetaObject GetMember(GetMemberAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");

            ComMethodDesc method;
            ComEventDesc @event;

            // 1. Try methods
            if (_self.TryGetMemberMethod(action.Name, out method)) {
                return BindGetMember(method, args);
            }

            // 2. Try events
            if (_self.TryGetMemberEvent(action.Name, out @event)) {
                return BindEvent(@event, args);
            }

            // 3. Try methods explicitly by name
            if (_self.TryGetMemberMethodExplicit(action.Name, out method)) {
                return BindGetMember(method, args);
            }

            // 4. Fallback
            return action.Fallback(Unwrap(args));
        }

        private MetaObject BindGetMember(ComMethodDesc method, MetaObject[] args) {
            string helper;

            Restrictions restrictions = Restrictions.Combine(args).Merge(IDispatchRestriction());
            Expression dispatch =
                Expression.Property(
                    Expression.ConvertHelper(args[0].Expression, typeof(IDispatchComObject)),
                    typeof(IDispatchComObject).GetProperty("DispatchObject")
                );

            if (method.DispId == ComDispIds.DISPID_NEWENUM) {
                // says it's a property but it needs to be called
                helper = "CreateMethod";
            } else if (method.IsPropertyGet) {
                if (method.Parameters.Length == 0) {                    
                    return new InvokeBinder(
                        new Argument[0],
                        new MetaObject[] { args[0] },
                        restrictions,
                        Expression.Constant(method),
                        dispatch,
                        method
                    ).Invoke();
                }
                helper = "CreatePropertyGet";
            } else if (method.IsPropertyPut) {
                helper = "CreatePropertyPut";
            } else {
                helper = "CreateMethod";
            }

            Expression result =
                Expression.Call(
                    typeof(ComRuntimeHelpers).GetMethod(helper),
                    dispatch,
                    Expression.Constant(method)
                );

            return new MetaObject(result, restrictions);
        }

        private MetaObject BindEvent(ComEventDesc @event, MetaObject[] args) {
            // BoundDispEvent CreateComEvent(object rcw, Guid sourceIid, int dispid)
            Expression result =
                Expression.Call(
                    typeof(ComRuntimeHelpers).GetMethod("CreateComEvent"),
                    Expression.Property(
                        Expression.ConvertHelper(args[0].Expression, typeof(IDispatchComObject)),
                        typeof(ComObject).GetProperty("Obj")
                    ),
                    Expression.Constant(@event.sourceIID),
                    Expression.Constant(@event.dispid)
                );

            return new MetaObject(
                result,
                Restrictions.Combine(args).Merge(IDispatchRestriction())
            );
        }

        public override MetaObject Invoke(InvokeAction action, MetaObject[] args) {
            return base.Invoke(action, args);
        }

        public override MetaObject Operation(OperationAction action, MetaObject[] args) {
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
                    return base.Operation(action, args);
            }
        }

        private MetaObject IndexOperation(OperationAction action, MetaObject[] args, string method) {
            MetaObject fallback = action.Fallback(Unwrap(args));

            VariableExpression callable = Expression.Variable(typeof(DispCallable), "callable");

            Expression[] callArgs = new Expression[args.Length];
            for (int i = 0; i < callArgs.Length; i++) {
                callArgs[i] = args[i].Expression;
            }
            callArgs[0] = callable;

            Expression result = Expression.Scope(
                Expression.Condition(
                    Expression.Call(
                        Expression.Convert(Expression, typeof(IDispatchComObject)),
                        typeof(IDispatchComObject).GetMethod(method),
                        callable
                    ),
                    Expression.Dynamic(new ComInvokeAction(), typeof(object), callArgs),
                    Expression.ConvertHelper(fallback.Expression, typeof(object))
                ),
                callable
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

        public override MetaObject SetMember(SetMemberAction action, MetaObject[] args) {
            ContractUtils.RequiresNotNull(action, "action");

            return 
                // 1. Check for simple property put
                TryPropertyPut(action, args) ??

                // 2. Check for event handler hookup where the put is dropped
                TryEventHandlerNoop(action, args) ??

                // 3. Go back to language
                action.Fallback(Unwrap(args));
        }

        private MetaObject TryPropertyPut(SetMemberAction action, MetaObject[] args) {
            ComMethodDesc method;
            if (_self.TryGetPropertySetter(action.Name, out method) || _self.TryGetIDOfName(action.Name)) {
                Expression result = Expression.Call(
                    Expression.ConvertHelper(Expression, typeof(IDispatchComObject)),
                    typeof(IDispatchComObject).GetMethod("SetAttr"),
                    Expression.Constant(action.Name),
                    Expression.ConvertHelper(args[1].Expression, typeof(object))
                );

                return new MetaObject(
                     result,
                     Restrictions.Combine(args).Merge(IDispatchRestriction())
                 );
            }

            return null;
        }

        private MetaObject TryEventHandlerNoop(SetMemberAction action, MetaObject[] args) {
            ComEventDesc @event;
            if (_self.TryGetEventHandler(action.Name, out @event) && args[1].LimitType == typeof(BoundDispEvent)) {
                // Drop the event property set.

                return new MetaObject(
                    Expression.Null(),
                    Restrictions.Combine(args).Merge(IDispatchRestriction()).Merge(Restrictions.TypeRestriction(args[1].Expression, typeof(BoundDispEvent)))
                );
            }

            return null;
        }

        private Restrictions IDispatchRestriction() {
            Expression @this = Expression;
            return Restrictions.TypeRestriction(
                @this, typeof(IDispatchComObject)
            ).Merge(
                Restrictions.ExpressionRestriction(
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

        private MetaObject[] Unwrap(MetaObject[] args) {
            MetaObject[] unwrap = args.Copy();
            unwrap[0] = new MetaObject(
                Expression.Property(
                    Expression.ConvertHelper(Expression, typeof(ComObject)),
                    typeof(ComObject).GetProperty("Obj")
                ),
                IDispatchRestriction(),
                _self.Obj
            );

            return unwrap;
        }
    }
}

#endif
