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

#if !SILVERLIGHT // ComObject

using System.Linq.Expressions;
using System.Scripting.Actions;

namespace System.Scripting.Com {

    internal sealed class IDispatchMetaObject : MetaObject {
        private readonly ComTypeDesc _wrapperType;
        private readonly IDispatchComObject _self;

        internal IDispatchMetaObject(Expression expression, ComTypeDesc wrapperType, IDispatchComObject self)
            : base(expression, Restrictions.Empty, self) {
            _wrapperType = wrapperType;
            _self = self;
        }

        public override MetaObject Call(CallAction action, MetaObject[] args) {
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

            return action.Fallback(args);
        }

        public override MetaObject Convert(ConvertAction action, MetaObject[] args) {
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

        public override MetaObject Create(CreateAction action, MetaObject[] args) {
            return base.Create(action, args);
        }

        public override MetaObject DeleteMember(DeleteMemberAction action, MetaObject[] args) {
            return base.DeleteMember(action, args);
        }

        public override MetaObject GetMember(GetMemberAction action, MetaObject[] args) {
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
            return action.Fallback(args);
        }

        private MetaObject BindGetMember(ComMethodDesc method, MetaObject[] args) {
            string helper;

            Restrictions restrictions = Restrictions.Combine(args).Merge(IDispatchRestriction());
            Expression dispatch =
                Expression.Property(
                    Expression.ConvertHelper(args[0].Expression, typeof(IDispatchComObject)),
                    typeof(IDispatchComObject).GetProperty("DispatchObject")
                );

            if (method.IsPropertyGet) {
                if (method.Parameters.Length == 0 && args.Length == 1) {
                    return new InvokeBinder(
                        new Argument[0],
                        args,
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
            switch (action.Operation) {
                case "GetItem":
                    return IndexOperation(action, args, "TryGetGetItem");
                case "SetItem":
                    return IndexOperation(action, args, "TrySetGetItem");
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
            MetaObject fallback = action.Fallback(args);

            VariableExpression callable = Expression.Variable(typeof(DispCallable), "callable");

            Expression[] callArgs = new Expression[args.Length];
            for (int i = 0; i < callArgs.Length; i++) {
                callArgs[i] = args[i].Expression;
            }
            callArgs[0] = callable;

            Expression result = Expression.Condition(
                Expression.Call(
                    Expression.Convert(Expression, typeof(IDispatchComObject)),
                    typeof(IDispatchComObject).GetMethod(method),
                    callable
                ),
                Expression.ActionExpression(new ComInvokeAction(), typeof(object), callArgs),
                fallback.Expression
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
                Expression.Call(
                    Expression.ConvertHelper(args[0].Expression, typeof(ComObject)),
                    typeof(ComObject).GetMethod("GetMemberNames")
                );

            return new MetaObject(
                result,
                Restrictions.Combine(args).Merge(IDispatchRestriction())
            );
        }

        public override MetaObject SetMember(SetMemberAction action, MetaObject[] args) {
            MetaObject fallback = action.Fallback(args);

            VariableExpression exception = Expression.Variable(typeof(Exception), "exception");
            Expression result =
                Expression.Scope(
                    Expression.Condition(
                        Expression.Call(
                            Expression.Convert(Expression, typeof(IDispatchComObject)),
                            typeof(IDispatchComObject).GetMethod("TrySetAttr"),
                            Expression.Constant(action.Name),
                            args[1].Expression,
                            exception
                        ),
                        Expression.Null(),              // true
                        MakeObject(fallback.Expression) // false
                    ),
                    exception
                );

            return new MetaObject(
                result,
                Restrictions.Combine(args).Merge(IDispatchRestriction()).Merge(fallback.Restrictions)
            );
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

        private static Expression MakeObject(Expression e) {
            if (e.Type == typeof(void)) {
                return Expression.Comma(
                    e,
                    Expression.Null()
                );
            } else {
                return Expression.ConvertHelper(e, typeof(object));
            }
        }
    }
}

#endif
