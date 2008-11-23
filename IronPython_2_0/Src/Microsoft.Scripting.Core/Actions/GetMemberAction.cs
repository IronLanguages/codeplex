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


using Microsoft.Contracts;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    public abstract class GetMemberAction : StandardAction {
        private readonly string _name;
        private readonly bool _ignoreCase;

        protected GetMemberAction(string name, bool ignoreCase)
            : base(StandardActionKind.GetMember) {
            ContractUtils.RequiresNotNull(name, "name");

            _name = name;
            _ignoreCase = ignoreCase;
        }

        public string Name {
            get {
                return _name;
            }
        }

        public bool IgnoreCase {
            get {
                return _ignoreCase;
            }
        }

        public MetaObject Fallback(MetaObject self) {
            return Fallback(self, null);
        }

        public abstract MetaObject Fallback(MetaObject self, MetaObject onBindingError);        

        public sealed override MetaObject Bind(MetaObject target, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires(args.Length == 0);

            return target.GetMember(this);
        }

        [Confined]
        public override bool Equals(object obj) {
            GetMemberAction gma = obj as GetMemberAction;
            return gma != null && gma._name == _name && gma._ignoreCase == _ignoreCase;
        }

        [Confined]
        public override int GetHashCode() {
            return ((int)Kind << 28) ^ _name.GetHashCode() ^ (_ignoreCase ? unchecked((int)0x80000000) : 0);
        }
    }
}
