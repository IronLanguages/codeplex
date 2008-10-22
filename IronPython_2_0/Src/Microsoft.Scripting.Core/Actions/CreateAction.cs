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
    public abstract class CreateAction : StandardAction {
        private readonly ReadOnlyCollection<Argument> _arguments;

        protected CreateAction(IEnumerable<Argument> arguments)
            : base(StandardActionKind.Create) {
            _arguments = arguments.ToReadOnly();
        }

        protected CreateAction(params Argument[] arguments)
            : this((IEnumerable<Argument>)arguments) {
        }

        public ReadOnlyCollection<Argument> Arguments {
            get {
                return _arguments;
            }
        }
        
        public MetaObject Fallback(MetaObject target, MetaObject[] args) {
            return Fallback(target, args, null);
        }

        public abstract MetaObject Fallback(MetaObject target, MetaObject[] args, MetaObject onBindingError);
        
        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");
            
            return target.Create(this, args);
        }

        [Confined]
        public override bool Equals(object obj) {
            CreateAction ca = obj as CreateAction;
            return ca != null && ca._arguments.ListEquals(_arguments);
        }

        [Confined]
        public override int GetHashCode() {
            return ((int)Kind << 28 ^ _arguments.ListHashCode());
        }
    }
}
