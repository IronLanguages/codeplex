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

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Provides a simple class that can be inherited from to create an object with dynamic behavior
    /// at runtime.  Subclasses can override the various action methods (GetMember, SetMember, Call, etc...)
    /// to provide custom behavior that will be invoked at runtime.  
    /// 
    /// If a method is not overridden then the Dynamic object does not directly support that behavior and 
    /// the call site will determine how the action should be performed.
    /// </summary>
    public class Dynamic : IDynamicObject {

        /// <summary>
        /// Enables derived types to create a new instance of Dynamic.  Dynamic instances cannot be
        /// directly instantiated because they have no implementation of dynamic behavior.
        /// </summary>
        protected Dynamic() {
        }

        #region Public Virtual APIs

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of getting a member.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        public virtual object GetMember(GetMemberAction action) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of setting a member.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        public virtual void SetMember(SetMemberAction action, object value) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of deleting a member.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        public virtual bool DeleteMember(DeleteMemberAction action) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of calling a member
        /// in the expando.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Call")]
        public virtual object Call(CallAction action, params object[] args) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of converting the
        /// Dynamic object to another type.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        public virtual object Convert(ConvertAction action) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of creating an instance
        /// of the Dynamic object.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        public virtual object Create(CreateAction action, params object[] args) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of invoking the
        /// Dynamic object.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        public virtual object Invoke(InvokeAction action, params object[] args) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of performing
        /// the operation.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        public virtual object Operation(OperationAction action, params object[] args) {
            throw new NotSupportedException();
        }

        #endregion

        #region MetaDynamic

        private sealed class MetaDynamic : MetaObject {

            internal MetaDynamic(Expression expression, Dynamic value)
                : base(expression, Restrictions.Empty, value) {
            }

            public override MetaObject GetMember(GetMemberAction action) {
                if (IsOverridden("GetMember")) {
                    return CallMethodUnary(action, "GetMember");
                }

                return base.GetMember(action);
            }

            public override MetaObject SetMember(SetMemberAction action, MetaObject value) {
                if (IsOverridden("SetMember")) {
                    return CallMethodBinary(action, value, "SetMember");
                }

                return base.SetMember(action, value);
            }

            public override MetaObject DeleteMember(DeleteMemberAction action) {
                if (IsOverridden("DeleteMember")) {
                    return CallMethodUnary(action, "DeleteMember");
                }

                return base.DeleteMember(action);
            }

            public override MetaObject Convert(ConvertAction action) {
                if (IsOverridden("Convert")) {
                    return CallMethodUnary(action, "Convert");
                }

                return base.Convert(action);
            }

            public override MetaObject Call(CallAction action, MetaObject[] args) {
                if (IsOverridden("Call")) {
                    return CallMethodNAry(action, args, "Call");
                }

                return base.Call(action, args);
            }
            
            public override MetaObject Create(CreateAction action, MetaObject[] args) {
                if (IsOverridden("Create")) {
                    return CallMethodNAry(action, args, "Create");
                }

                return base.Create(action, args);
            }

            public override MetaObject Invoke(InvokeAction action, MetaObject[] args) {
                if (IsOverridden("Invoke")) {
                    return CallMethodNAry(action, args, "Invoke");
                }

                return base.Invoke(action, args);
            }

            public override MetaObject Operation(OperationAction action, MetaObject[] args) {
                if (IsOverridden("Operation")) {
                    return CallMethodNAry(action, args, "Operation");
                }

                return base.Operation(action, args);
            }

            /// <summary>
            /// Helper method for generating a MetaObject which calls a specific method on Dynamic
            /// w/o any additional parameters.
            /// </summary>
            private MetaObject CallMethodUnary(MetaAction action, string methodName) {
                return new MetaObject(
                    Expression.Call(
                        GetLimitedSelf(),
                        typeof(Dynamic).GetMethod(methodName),
                        Expression.Constant(action)
                    ),
                    GetRestrictions()
                );
            }

            /// <summary>
            /// Helper method for generating a MetaObject which calls a specific method declared on
            /// Dynamic w/ one additional parameter.
            /// </summary>
            private MetaObject CallMethodBinary(SetMemberAction action, MetaObject arg, string name) {
                return new MetaObject(
                    Expression.Call(
                        GetLimitedSelf(),
                        typeof(Dynamic).GetMethod(name),
                        Expression.Constant(action),
                        arg.Expression
                    ),
                    GetRestrictions()
                );
            }

            /// <summary>
            /// Helper method for generating a MetaObject which calls a specific method on Dynamic w/ the
            /// meta object array as the params.
            /// </summary>
            private MetaObject CallMethodNAry(MetaAction action, MetaObject[] args, string method) {
                return new MetaObject(
                    Expression.Call(
                        GetLimitedSelf(),
                        typeof(Dynamic).GetMethod(method),
                        GetActionToArgs(action, args)
                    ),
                    GetRestrictions()
                );
            }

            /// <summary>
            /// Returns the parameters for a call to one of our helpers.  It adds the MetaAction
            /// first and packs the parameters into an object array.
            /// </summary>
            private static Expression[] GetActionToArgs(MetaAction action, MetaObject[] args) {
                Expression[] paramArgs = MetaObject.GetExpressions(args);

                for (int i = 0; i < paramArgs.Length; i++) {
                    paramArgs[i] = Expression.ConvertHelper(args[i].Expression, typeof(object));
                }
                
                return new Expression[] { 
                    Expression.Constant(action), 
                    Expression.NewArrayInit(typeof(object), paramArgs)
                };
            }

            /// <summary>
            /// Checks if the derived type has overridden the specified method.  If there is no
            /// implementation for the method provided then Dynamic falls back to the base class
            /// behavior which lets the call site determine how the action is performed.
            /// </summary>
            private bool IsOverridden(string method) {
                var methods = Value.GetType().GetMember(method, MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance);

                foreach (MethodInfo mi in methods) {
                    if (mi.DeclaringType != typeof(Dynamic) && mi.GetBaseDefinition().DeclaringType == typeof(Dynamic)) { 
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

                return Restrictions.TypeRestriction(Expression, LimitType);
            }

            /// <summary>
            /// Returns our Expression converted to our known LimitType
            /// </summary>
            private Expression GetLimitedSelf() {
                return Expression.ConvertHelper(
                    Expression,
                    LimitType
                );
            }
            
            private new Dynamic Value {
                get {
                    return (Dynamic)base.Value;
                }
            }
        }

        #endregion

        #region IDynamicObject Members

        /// <summary>
        /// Can be overridden in the derived class.  The provided
        /// MetaObject will dispatch to the Dynamic virtual methods.  The
        /// object can be encapsulated inside of another MetaObject to
        /// provide custom behavior for individual actions.
        /// </summary>
        public virtual MetaObject GetMetaObject(Expression parameter) {
            return new MetaDynamic(parameter, this);
        }

        #endregion
    }
}
