/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

namespace Microsoft.Scripting.Actions {
    public abstract class MemberAction : Action {
        private SymbolId _name;

        internal MemberAction(SymbolId name) {
            this._name = name;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public override bool Equals(object obj) {
            MemberAction other = obj as MemberAction;
            if (other == null) return false;
            return _name == other._name && Kind == other.Kind;
        }

        public override int GetHashCode() {
            return (int)Kind << 28 ^ _name.GetHashCode();
        }

        public override string ToString() {
            return base.ToString() + " " + SymbolTable.IdToString(_name);
        }
    }
}
