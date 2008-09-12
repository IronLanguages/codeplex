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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    public abstract class DeleteMemberAction : StandardAction {
        private readonly string _name;
        private readonly bool _ignoreCase;

        protected DeleteMemberAction(string name, bool ignoreCase)
            : base(StandardActionKind.DeleteMember) {
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

        public sealed override MetaObject Bind(MetaObject[] args) {
            ContractUtils.RequiresNotNullItems(args, "args");
            ContractUtils.Requires(args.Length > 0);
            return args[0].DeleteMember(this, args);
        }

        public override int GetHashCode() {
            return ((int)Kind << 28) ^ _name.GetHashCode() ^ (_ignoreCase ? 0x8000000 : 0); ;
        }

        public override bool Equals(object obj) {
            DeleteMemberAction dma = obj as DeleteMemberAction;
            return dma != null && dma._name == _name && dma._ignoreCase == _ignoreCase;
        }
    }
}
