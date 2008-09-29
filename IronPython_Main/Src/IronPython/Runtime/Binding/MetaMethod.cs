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
using System.Collections;
using System.Collections.Generic;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Actions;

using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;

using Ast = Microsoft.Linq.Expressions.Expression;

namespace IronPython.Runtime.Binding {

    class MetaMethod : MetaPythonObject, IPythonInvokable {
        public MetaMethod(Expression/*!*/ expression, Restrictions/*!*/ restrictions, Method/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region IPythonInvokable Members

        public MetaObject/*!*/ Invoke(InvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, args);
        }

        #endregion

        #region MetaObject Overrides

        public override MetaObject/*!*/ Call(CallAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            return BindingHelpers.GenericCall(action, args);
        }

        public override MetaObject/*!*/ Invoke(InvokeAction/*!*/ callAction, params MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(callAction, args);
        }

        public override MetaObject Convert(ConvertAction action, MetaObject[] args) {
            if (action.ToType.IsSubclassOf(typeof(Delegate))) {
                return MakeDelegateTarget(action, action.ToType, Restrict(typeof(Method)));
            }

            return base.Convert(action, args);
        }

        #endregion

        #region Invoke Implementation

        private MetaObject InvokeWorker(MetaAction/*!*/ callAction, MetaObject/*!*/[] args) {
            args = ArrayUtils.RemoveFirst(args);

            CallSignature signature = BindingHelpers.GetCallSignature(callAction);
            MetaObject self = Restrict(typeof(Method));
            Restrictions restrictions = self.Restrictions;

            MetaObject func = GetMetaFunction(self);
            MetaObject call;

            if (Value.im_self == null) {
                // restrict to null self (Method is immutable so this is an invariant test)
                restrictions = restrictions.Merge(
                    Restrictions.ExpressionRestriction(
                        Ast.Equal(
                            GetSelfExpression(self),
                            Ast.Null()
                        )
                    )
                );

                if (args.Length == 0) {
                    // this is an error, we pass null which will throw the normal error
                    call = new MetaObject(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("MethodCheckSelf"),
                            self.Expression,
                            Ast.Null()
                        ),
                        restrictions
                    );
                } else {
                    // this may or may not be an error
                    call = new MetaObject(
                        Ast.Comma(
                            MakeCheckSelf(signature, args),
                            Ast.Dynamic(
                                new InvokeBinder(
                                    BinderState.GetBinderState(callAction),
                                    BindingHelpers.GetCallSignature(callAction)
                                ),
                                typeof(object),
                                ArrayUtils.Insert(BinderState.GetCodeContext(callAction), MetaObject.GetExpressions(ArrayUtils.Insert(func, args)))
                            )
                        ),
                        Restrictions.Empty
                    );
                    /*call = func.Invoke(callAction, ArrayUtils.Insert(func, args));
                    call =  new MetaObject(
                        Ast.Comma(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("MethodCheckSelf"),
                                self.Expression,
                                args[0].Expression
                            ),
                            call.Expression
                        ),
                        call.Restrictions                        
                    );*/
                }
            } else {
                // restrict to non-null self (Method is immutable so this is an invariant test)
                restrictions = restrictions.Merge(
                    Restrictions.ExpressionRestriction(
                        Ast.NotEqual(
                            GetSelfExpression(self),
                            Ast.Null()
                        )
                    )
                );

                MetaObject im_self = GetMetaSelf(self);
                MetaObject[] newArgs = ArrayUtils.Insert(func, im_self, args);
                CallSignature newSig = new CallSignature(ArrayUtils.Insert(new ArgumentInfo(ArgumentKind.Simple), signature.GetArgumentInfos()));


                call = new MetaObject(
                    Ast.Dynamic(
                        new InvokeBinder(
                            BinderState.GetBinderState(callAction),
                            newSig
                        ),
                        typeof(object),
                        ArrayUtils.Insert(BinderState.GetCodeContext(callAction), MetaObject.GetExpressions(newArgs))
                    ),
                    Restrictions.Empty
                );

                /*
                call = func.Invoke(
                    new CallBinder(
                        BinderState.GetBinderState(callAction),
                        newSig
                    ),
                    newArgs
                );*/
            }

            if (call.HasValue) {
                return new MetaObject(
                    call.Expression,
                    restrictions.Merge(call.Restrictions),
                    call.Value
                );
            } else {
                return new MetaObject(
                    call.Expression,
                    restrictions.Merge(call.Restrictions)
                );
            }
        }

        #endregion

        #region Helpers

        private MetaObject GetMetaSelf(MetaObject/*!*/ self) {
            MetaObject func;

            IDynamicObject ido = Value.im_self as IDynamicObject;
            if (ido != null) {
                func = ido.GetMetaObject(GetSelfExpression(self));
            } else if (Value.im_self == null) {
                func = new MetaObject(
                    GetSelfExpression(self),
                    Restrictions.Empty);
            } else {
                func = new MetaObject(
                    GetSelfExpression(self),
                    Restrictions.Empty,
                    Value.im_self
                );
            }

            return func;
        }
        
        private MetaObject/*!*/ GetMetaFunction(MetaObject/*!*/ self) {
            MetaObject func;
            IDynamicObject ido = Value.im_func as IDynamicObject;
            if (ido != null) {
                func = ido.GetMetaObject(GetFunctionExpression(self));
            } else {
                func = new MetaObject(
                    GetFunctionExpression(self),
                    Restrictions.Empty
                );
            }
            return func;
        }

        private static MemberExpression GetFunctionExpression(MetaObject self) {
            return Ast.Property(
                self.Expression,
                typeof(Method).GetProperty("im_func")
            );
        }

        private static MemberExpression GetSelfExpression(MetaObject self) {
            return Ast.Property(
                self.Expression,
                typeof(Method).GetProperty("im_self")
            );
        }

        public new Method/*!*/ Value {
            get {
                return (Method)base.Value;
            }
        }

        private Expression/*!*/ MakeCheckSelf(CallSignature signature, MetaObject/*!*/[]/*!*/ args) {
            ArgumentKind firstArgKind = signature.GetArgumentKind(0);

            Expression res;
            if (firstArgKind == ArgumentKind.Simple || firstArgKind == ArgumentKind.Instance) {
                res = CheckSelf(Ast.ConvertHelper(Expression, typeof(Method)), args[0].Expression);
            } else if (firstArgKind != ArgumentKind.List) {
                res = CheckSelf(Ast.ConvertHelper(Expression, typeof(Method)), Ast.Null());
            } else {
                // list, check arg[0] and then return original list.  If not a list,
                // or we have no items, then check against null & throw.
                res = CheckSelf(
                    Ast.ConvertHelper(Expression, typeof(Method)),
                    Ast.Condition(
                        Ast.AndAlso(
                            Ast.TypeIs(args[0].Expression, typeof(IList<object>)),
                            Ast.NotEqual(
                                Ast.Property(
                                    Ast.Convert(args[0].Expression, typeof(ICollection)),
                                    typeof(ICollection).GetProperty("Count")
                                ),
                                Ast.Constant(0)
                            )
                        ),
                        Ast.Call(
                            Ast.Convert(args[0].Expression, typeof(IList<object>)),
                            typeof(IList<object>).GetMethod("get_Item"),
                            Ast.Constant(0)
                        ),
                        Ast.Null()
                    )
                );
            }

            return res;
        }

        private Expression/*!*/ CheckSelf(Expression/*!*/ method, Expression/*!*/ inst) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MethodCheckSelf"),
                method,
                Ast.ConvertHelper(inst, typeof(object))
            );
        }

        #endregion
    }
}
