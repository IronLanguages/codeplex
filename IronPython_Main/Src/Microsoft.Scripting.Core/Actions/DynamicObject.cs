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
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Binders {
    /// <summary>
    /// Provides a simple class that can be inherited from to create an object with dynamic behavior
    /// at runtime.  Subclasses can override the various binder methods (GetMember, SetMember, Call, etc...)
    /// to provide custom behavior that will be invoked at runtime.  
    /// 
    /// If a method is not overridden then the Dynamic object does not directly support that behavior and 
    /// the call site will determine how the binder should be performed.
    /// </summary>
    public class DynamicObject : IDynamicObject {

        /// <summary>
        /// Enables derived types to create a new instance of Dynamic.  Dynamic instances cannot be
        /// directly instantiated because they have no implementation of dynamic behavior.
        /// </summary>
        protected DynamicObject() {
        }

        #region Public Virtual APIs

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of getting a member.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object GetMember(GetMemberBinder binder) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of setting a member.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual void SetMember(SetMemberBinder binder, object value) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of deleting a member.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual bool DeleteMember(DeleteMemberBinder binder) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of calling a member
        /// in the expando.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object InvokeMember(InvokeMemberBinder binder, object[] args) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of converting the
        /// Dynamic object to another type.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object Convert(ConvertBinder binder) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of creating an instance
        /// of the Dynamic object.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object CreateInstance(CreateInstanceBinder binder, object[] args) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of invoking the
        /// Dynamic object.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object Invoke(InvokeBinder binder, object[] args) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing a binary operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object BinaryOperation(BinaryOperationBinder binder, object arg) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing a unary operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object UnaryOperation(UnaryOperationBinder binder) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing a get index operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object GetIndex(GetIndexBinder binder, object[] args) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing a set index operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object SetIndex(SetIndexBinder binder, object[] indexes, object value) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing a delete index operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object DeleteIndex(DeleteIndexBinder binder, object[] indexes) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing an operation on member "a.b (op)=c" operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object BinaryOperationOnMember(BinaryOperationOnMemberBinder binder, object value) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing an operation on index "a[i,j,k] (op)= c" operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object BinaryOperationOnIndex(BinaryOperationOnIndexBinder binder, object[] indexes, object value) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing an operation on member "a.b (op)" operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object UnaryOperationOnMember(UnaryOperationOnMemberBinder binder) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing an operation on index "a[i,j,k] (op)" operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual object UnaryOperationOnIndex(UnaryOperationOnIndexBinder binder, object[] indexes) {
            throw new NotSupportedException();
        }

        #endregion

        #region MetaDynamic

        private sealed class MetaDynamic : MetaObject {

            internal MetaDynamic(Expression expression, DynamicObject value)
                : base(expression, Restrictions.Empty, value) {
            }

            public override MetaObject BindGetMember(GetMemberBinder binder) {
                if (IsOverridden("GetMember")) {
                    return CallMethodUnary(binder, "GetMember");
                }

                return base.BindGetMember(binder);
            }

            public override MetaObject BindSetMember(SetMemberBinder binder, MetaObject value) {
                if (IsOverridden("SetMember")) {
                    return CallMethodBinary(binder, value, "SetMember");
                }

                return base.BindSetMember(binder, value);
            }

            public override MetaObject BindDeleteMember(DeleteMemberBinder binder) {
                if (IsOverridden("DeleteMember")) {
                    return CallMethodUnary(binder, "DeleteMember");
                }

                return base.BindDeleteMember(binder);
            }

            public override MetaObject BindConvert(ConvertBinder binder) {
                if (IsOverridden("Convert")) {
                    return CallMethodUnary(binder, "Convert");
                }

                return base.BindConvert(binder);
            }

            public override MetaObject BindInvokeMember(InvokeMemberBinder binder, MetaObject[] args) {
                if (IsOverridden("InvokeMember")) {
                    return CallMethodNAry(binder, args, "InvokeMember");
                }

                return base.BindInvokeMember(binder, args);
            }

            public override MetaObject BindCreateInstance(CreateInstanceBinder binder, MetaObject[] args) {
                if (IsOverridden("CreateInstance")) {
                    return CallMethodNAry(binder, args, "CreateInstance");
                }

                return base.BindCreateInstance(binder, args);
            }

            public override MetaObject BindInvoke(InvokeBinder binder, MetaObject[] args) {
                if (IsOverridden("Invoke")) {
                    return CallMethodNAry(binder, args, "Invoke");
                }

                return base.BindInvoke(binder, args);
            }

            public override MetaObject BindBinaryOperation(BinaryOperationBinder binder, MetaObject arg) {
                if (IsOverridden("BinaryOperation")) {
                    return CallMethodBinary(binder, arg, "BinaryOperation");
                }

                return base.BindBinaryOperation(binder, arg);
            }

            public override MetaObject BindUnaryOperation(UnaryOperationBinder binder) {
                if (IsOverridden("UnaryOperation")) {
                    return CallMethodUnary(binder, "UnaryOperation");
                }

                return base.BindUnaryOperation(binder);
            }

            public override MetaObject BindGetIndex(GetIndexBinder binder, MetaObject[] indexes) {
                if (IsOverridden("GetIndex")) {
                    return CallMethodNAry(binder, indexes, "GetIndex");
                }

                return base.BindGetIndex(binder, indexes);
            }

            public override MetaObject BindSetIndex(SetIndexBinder binder, MetaObject[] indexes, MetaObject value) {
                if (IsOverridden("SetIndex")) {
                    return CallMethodNAry(binder, indexes, value, "SetIndex");
                }

                return base.BindSetIndex(binder, indexes, value);
            }

            public override MetaObject BindDeleteIndex(DeleteIndexBinder binder, MetaObject[] indexes) {
                if (IsOverridden("DeleteIndex")) {
                    return CallMethodNAry(binder, indexes, "DeleteIndex");
                }

                return base.BindDeleteIndex(binder, indexes);
            }

            public override MetaObject BindBinaryOperationOnMember(BinaryOperationOnMemberBinder binder, MetaObject value) {
                if (IsOverridden("BinaryOperationOnMember")) {
                    return CallMethodBinary(binder, value, "BinaryOperationOnMember");
                }

                return base.BindBinaryOperationOnMember(binder, value);
            }

            public override MetaObject BindBinaryOperationOnIndex(BinaryOperationOnIndexBinder binder, MetaObject[] indexes, MetaObject value) {
                if (IsOverridden("BinaryOperationOnIndex")) {
                    return CallMethodNAry(binder, indexes, value, "BinaryOperationOnIndex");
                }

                return base.BindBinaryOperationOnIndex(binder, indexes, value);
            }


            public override MetaObject BindUnaryOperationOnMember(UnaryOperationOnMemberBinder binder) {
                if (IsOverridden("UnaryOperationOnMember")) {
                    return CallMethodUnary(binder, "UnaryOperationOnMember");
                }

                return base.BindUnaryOperationOnMember(binder);
            }

            public override MetaObject BindUnaryOperationOnIndex(UnaryOperationOnIndexBinder binder, MetaObject[] indexes) {
                if (IsOverridden("UnaryOperationOnIndex")) {
                    return CallMethodNAry(binder, indexes, "UnaryOperationOnIndex");
                }

                return base.BindUnaryOperationOnIndex(binder, indexes);
            }

            /// <summary>
            /// Helper method for generating a MetaObject which calls a specific method on Dynamic
            /// w/o any additional parameters.
            /// </summary>
            private MetaObject CallMethodUnary(MetaObjectBinder binder, string methodName) {
                return new MetaObject(
                    Expression.Call(
                        GetLimitedSelf(),
                        typeof(DynamicObject).GetMethod(methodName),
                        Expression.Constant(binder)
                    ),
                    GetRestrictions()
                );
            }

            /// <summary>
            /// Helper method for generating a MetaObject which calls a specific method declared on
            /// Dynamic w/ one additional parameter.
            /// </summary>
            private MetaObject CallMethodBinary(MetaObjectBinder binder, MetaObject arg, string name) {
                return new MetaObject(
                    Expression.Call(
                        GetLimitedSelf(),
                        typeof(DynamicObject).GetMethod(name),
                        Expression.Constant(binder),
                        arg.Expression
                    ),
                    GetRestrictions()
                );
            }

            /// <summary>
            /// Helper method for generating a MetaObject which calls a specific method on Dynamic w/ the
            /// meta object array as the params.
            /// </summary>
            private MetaObject CallMethodNAry(MetaObjectBinder binder, MetaObject[] args, string method) {
                return new MetaObject(
                    Expression.Call(
                        GetLimitedSelf(),
                        typeof(DynamicObject).GetMethod(method),
                        Expression.Constant(binder),
                        CreateArrayForArgs(args)
                    ),
                    GetRestrictions()
                );
            }

            private MetaObject CallMethodNAry(MetaObjectBinder binder, MetaObject[] args, MetaObject value, string method) {
                return new MetaObject(
                    Expression.Call(
                        GetLimitedSelf(),
                        typeof(DynamicObject).GetMethod(method),
                        Expression.Constant(binder),
                        CreateArrayForArgs(args),
                        Helpers.Convert(value.Expression, typeof(object))
                    ),
                    GetRestrictions()
                );
            }

            /// <summary>
            /// Returns the parameters for a call to one of our helpers.  It adds the MetaAction
            /// first and packs the parameters into an object array.
            /// </summary>
            private static Expression CreateArrayForArgs(MetaObject[] args) {
                Expression[] paramArgs = MetaObject.GetExpressions(args);

                for (int i = 0; i < paramArgs.Length; i++) {
                    paramArgs[i] = Helpers.Convert(args[i].Expression, typeof(object));
                }

                return Expression.NewArrayInit(typeof(object), paramArgs);
            }

            /// <summary>
            /// Checks if the derived type has overridden the specified method.  If there is no
            /// implementation for the method provided then Dynamic falls back to the base class
            /// behavior which lets the call site determine how the binder is performed.
            /// </summary>
            private bool IsOverridden(string method) {
                var methods = Value.GetType().GetMember(method, MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance);

                foreach (MethodInfo mi in methods) {
                    if (mi.DeclaringType != typeof(DynamicObject) && mi.GetBaseDefinition().DeclaringType == typeof(DynamicObject)) {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Returns a Restrictions object which includes our current restrictions merged
            /// with a restriction limiting our type
            /// </summary>
            private Restrictions GetRestrictions() {
                Debug.Assert(Restrictions == Restrictions.Empty, "We don't merge, restrictions are always empty");

                return Restrictions.GetTypeRestriction(Expression, LimitType);
            }

            /// <summary>
            /// Returns our Expression converted to our known LimitType
            /// </summary>
            private Expression GetLimitedSelf() {
                return Helpers.Convert(
                    Expression,
                    LimitType
                );
            }

            private new DynamicObject Value {
                get {
                    return (DynamicObject)base.Value;
                }
            }
        }

        #endregion

        #region IDynamicObject Members

        /// <summary>
        /// The provided MetaObject will dispatch to the Dynamic virtual methods.
        /// The object can be encapsulated inside of another MetaObject to
        /// provide custom behavior for individual actions.
        /// </summary>
        MetaObject IDynamicObject.GetMetaObject(Expression parameter) {
            return new MetaDynamic(parameter, this);
        }

        #endregion
    }
}
