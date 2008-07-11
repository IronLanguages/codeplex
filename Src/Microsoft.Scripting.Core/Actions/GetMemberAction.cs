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

using Microsoft.Contracts;
using System.Scripting.Utils;

namespace System.Scripting.Actions {
    public abstract class GetMemberAction : StandardAction {
        private readonly string _name;
        private readonly bool _caseInsensitive;

        protected GetMemberAction(string name, bool caseInsensitive)
            : base(StandardActionKind.GetMember) {
            ContractUtils.RequiresNotNull(name, "name");

            _name = name;
            _caseInsensitive = caseInsensitive;
        }

        public string Name {
            get {
                return _name;
            }
        }

        public bool CaseInsensitive {
            get {
                return _caseInsensitive;
            }
        }

        public sealed override MetaObject Bind(params MetaObject[] args) {
            ContractUtils.RequiresNotNullItems(args, "args");
            ContractUtils.Requires(args.Length > 0);
            return args[0].GetMember(this, args);
        }

        [Confined]
        public override bool Equals(object obj) {
            GetMemberAction gma = obj as GetMemberAction;
            return gma != null && gma._name == _name && gma._caseInsensitive == _caseInsensitive;
        }

        [Confined]
        public override int GetHashCode() {
            return ((int)Kind << 28) ^ _name.GetHashCode() ^ (_caseInsensitive ? unchecked((int)0x80000000) : 0);
        }
    }
}
