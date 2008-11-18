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
    public abstract class DeleteIndexAction : StandardAction {
        private readonly ReadOnlyCollection<Argument> _arguments;

        protected DeleteIndexAction(params Argument[] arguments)
            : this((IEnumerable<Argument>)arguments) {
        }

        protected DeleteIndexAction(IEnumerable<Argument> arguments)
            : base(StandardActionKind.DeleteIndex) {
            _arguments = arguments.ToReadOnly();
        }

        public ReadOnlyCollection<Argument> Arguments {
            get { return _arguments; }
        }

        [Confined]
        public override bool Equals(object obj) {
            DeleteIndexAction ia = obj as DeleteIndexAction;
            return ia != null && ia._arguments.ListEquals(_arguments);
        }

        [Confined]
        public override int GetHashCode() {
            return ((int)Kind << 28) ^ _arguments.ListHashCode();
        }

        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");

            return target.DeleteIndex(this, args);
        }

        public MetaObject Fallback(MetaObject target, MetaObject[] args) {
            return Fallback(target, args, null);
        }

        public abstract MetaObject Fallback(MetaObject target, MetaObject[] args, MetaObject onBindingError);
    }
}