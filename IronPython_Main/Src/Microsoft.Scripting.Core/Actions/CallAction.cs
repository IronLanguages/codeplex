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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Scripting.Utils;
using Microsoft.Contracts;

namespace System.Scripting.Actions {
    public abstract class CallAction : StandardAction {
        private readonly string _name;
        private readonly bool _caseInsensitive;
        private readonly ReadOnlyCollection<Argument> _arguments;

        protected CallAction(string name, bool caseInsensitive, IEnumerable<Argument> arguments)
            : base(StandardActionKind.Call) {
            _name = name;
            _caseInsensitive = caseInsensitive;
            _arguments = CollectionUtils.ToReadOnlyCollection(arguments);
        }

        protected CallAction(string name, bool caseInsensitive, params Argument[] arguments)
            : this(name, caseInsensitive, (IEnumerable<Argument>)arguments) {
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

        public ReadOnlyCollection<Argument> Arguments {
            get {
                return _arguments;
            }
        }

        public sealed override MetaObject Bind(MetaObject[] args) {
            ContractUtils.RequiresNotNullItems(args, "args");
            ContractUtils.Requires(args.Length > 0);
            return args[0].Call(this, args);
        }

        [Confined]
        public override bool Equals(object obj) {
            CallAction ca = obj as CallAction;
            return ca != null && ca._name == _name && ca._caseInsensitive == _caseInsensitive && CollectionUtils.Equal(ca._arguments, _arguments);
        }

        [Confined]
        public override int GetHashCode() {
            return ((int)Kind << 28 ^ _name.GetHashCode() ^ (_caseInsensitive ? 0x8000000 : 0) ^ CollectionUtils.GetHashCode(_arguments));
        }
    }
}
