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
using Microsoft.Contracts;

namespace Microsoft.Scripting.Actions {
    public abstract class ConvertAction : StandardAction {
        private readonly Type _type;
        private readonly bool _explicit;

        protected ConvertAction(Type type, bool @explicit)
            : base(StandardActionKind.Convert) {
            _type = type;
            _explicit = @explicit;
        }

        public Type ToType {
            get {
                return _type;
            }
        }

        public bool Explicit {
            get {
                return _explicit;
            }
        }

        public MetaObject Fallback(MetaObject self) {
            return Fallback(self, null);
        }

        public abstract MetaObject Fallback(MetaObject self, MetaObject onBindingError);
        
        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires(args.Length == 0);

            return target.Convert(this);
        }

        [Confined]
        public override bool Equals(object obj) {
            ConvertAction ca = obj as ConvertAction;
            return ca != null && ca._type == _type && ca._explicit == _explicit;
        }

        [Confined]
        public override int GetHashCode() {
            return ((int)Kind << 28) ^ _type.GetHashCode() ^ (_explicit ? 0x8000000 : 0);
        }
    }
}