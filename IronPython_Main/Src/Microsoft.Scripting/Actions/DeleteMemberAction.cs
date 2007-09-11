using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Scripting.Actions {
    public class DeleteMemberAction : MemberAction, IEquatable<DeleteMemberAction> {
        private DeleteMemberAction(SymbolId name)
            : base(name) {
        }

        public static DeleteMemberAction Make(SymbolId name) {
            return new DeleteMemberAction(name);
        }

        public override ActionKind Kind {
            get { return ActionKind.DeleteMember; }
        }

        #region IEquatable<DeleteMemberAction> Members

        public bool Equals(DeleteMemberAction other) {
            if (other == null) return false;

            return base.Equals(other);
        }

        #endregion

    }
}
