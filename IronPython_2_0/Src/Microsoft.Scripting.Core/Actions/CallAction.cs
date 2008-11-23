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


using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Actions {
    public abstract class CallAction : StandardAction {
        private readonly string _name;
        private readonly bool _ignoreCase;
        private readonly ReadOnlyCollection<Argument> _arguments;

        protected CallAction(string name, bool ignoreCase, IEnumerable<Argument> arguments)
            : base(StandardActionKind.Call) {
            _name = name;
            _ignoreCase = ignoreCase;
            _arguments = arguments.ToReadOnly();
        }

        protected CallAction(string name, bool ignoreCase, params Argument[] arguments)
            : this(name, ignoreCase, (IEnumerable<Argument>)arguments) {
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

        public ReadOnlyCollection<Argument> Arguments {
            get {
                return _arguments;
            }
        }

        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");
            
            return target.Call(this, args);
        }

        public MetaObject Fallback(MetaObject target, MetaObject[] args) {
            return Fallback(target, args, null);
        }

        public abstract MetaObject Fallback(MetaObject target, MetaObject[] args, MetaObject onBindingError);
        public abstract MetaObject FallbackInvoke(MetaObject target, MetaObject[] args, MetaObject onBindingError);

        [Confined]
        public override bool Equals(object obj) {
            CallAction ca = obj as CallAction;
            return ca != null && ca._name == _name && ca._ignoreCase == _ignoreCase && ca._arguments.ListEquals(_arguments);
        }

        [Confined]
        public override int GetHashCode() {
            return ((int)Kind << 28 ^ _name.GetHashCode() ^ (_ignoreCase ? 0x8000000 : 0) ^ _arguments.ListHashCode());
        }
    }
}
