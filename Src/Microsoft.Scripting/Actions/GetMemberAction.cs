/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

namespace Microsoft.Scripting.Actions {
    public class GetMemberAction : MemberAction {
        public static GetMemberAction Make(string name) {
            return Make(SymbolTable.StringToId(name));
        }

        public static GetMemberAction Make(SymbolId name) {
            return new GetMemberAction(name);
        }

        private GetMemberAction(SymbolId name)
            : base(name) {
        }

        public override ActionKind Kind {
            get { return ActionKind.GetMember; }
        }
    }
}
