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

using System;
using Microsoft.Contracts;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    public class DeleteMemberAction : MemberAction, IEquatable<DeleteMemberAction> {
        private DeleteMemberAction(ActionBinder binder, SymbolId name)
            : base(binder, name) {
        }

        public static DeleteMemberAction Make(ActionBinder binder, string name) {
            return Make(binder, SymbolTable.StringToId(name));
        }

        public static DeleteMemberAction Make(ActionBinder binder, SymbolId name) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return new DeleteMemberAction(binder, name);
        }

        public override DynamicActionKind Kind {
            get { return DynamicActionKind.DeleteMember; }
        }

        #region IEquatable<DeleteMemberAction> Members

        [StateIndependent]
        public bool Equals(DeleteMemberAction other) {
            if (other == null) return false;

            return base.Equals(other);
        }

        #endregion

    }
}
