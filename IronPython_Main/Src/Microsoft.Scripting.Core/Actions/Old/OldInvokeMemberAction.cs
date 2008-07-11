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

using System.Linq.Expressions;
using System.Scripting.Utils;
using Microsoft.Contracts;

namespace System.Scripting.Actions {
    // TODO: rename to match MethodCallExpression
    public class OldInvokeMemberAction : OldMemberAction, IEquatable<OldInvokeMemberAction>, IExpressionSerializable {
        private readonly CallSignature _signature;

        public CallSignature Signature { get { return _signature; } }

        protected OldInvokeMemberAction(ActionBinder binder, SymbolId memberName, CallSignature signature)
            : base(binder, memberName) {
            _signature = signature;
        }

        public static OldInvokeMemberAction Make(ActionBinder binder, string memberName, CallSignature signature) {
            return Make(binder, SymbolTable.StringToId(memberName), signature);
        }

        public static OldInvokeMemberAction Make(ActionBinder binder, SymbolId memberName, CallSignature signature) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return new OldInvokeMemberAction(binder, memberName, signature);
        }

        public override DynamicActionKind Kind {
            get { return DynamicActionKind.InvokeMember; }
        }

        [Confined]
        public override bool Equals(object obj) {
            return Equals(obj as OldInvokeMemberAction);
        }

        [Confined]
        public override int GetHashCode() {
            return _signature.GetHashCode();
        }

        [Confined]
        public override string/*!*/ ToString() {
            return base.ToString() + _signature.ToString();
        }

        public Expression CreateExpression() {
            return Expression.Call(
                typeof(OldInvokeMemberAction).GetMethod("Make", new Type[] { typeof(ActionBinder), typeof(SymbolId), typeof(CallSignature) }),
                CreateActionBinderReadExpression(),
                Expression.Constant(Name),
                Signature.CreateExpression()
            );            
        }

        #region IEquatable<OldInvokeMemberAction> Members

        [StateIndependent]
        public bool Equals(OldInvokeMemberAction other) {
            if (other == null) return false;
            return Name == other.Name && _signature.Equals(other._signature);
        }

        #endregion
    }
}
