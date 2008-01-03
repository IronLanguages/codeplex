using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Actions {
    public class DeleteMemberAction : MemberAction, IEquatable<DeleteMemberAction> {
        private DeleteMemberAction(SymbolId name)
            : base(name) {
        }

        public static DeleteMemberAction Make(string name) {
            return new DeleteMemberAction(SymbolTable.StringToId(name));
        }

        public static DeleteMemberAction Make(SymbolId name) {
            return new DeleteMemberAction(name);
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