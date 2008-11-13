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

namespace Microsoft.Scripting.Binders {
    public abstract class SetIndexBinder : MetaObjectBinder {
        private readonly ReadOnlyCollection<ArgumentInfo> _arguments;

        protected SetIndexBinder(params ArgumentInfo[] arguments)
            : this((IEnumerable<ArgumentInfo>)arguments) {
        }

        protected SetIndexBinder(IEnumerable<ArgumentInfo> arguments) {
            _arguments = arguments.ToReadOnly();
        }

        public ReadOnlyCollection<ArgumentInfo> Arguments {
            get { return _arguments; }
        }

        [Confined]
        public override bool Equals(object obj) {
            SetIndexBinder ia = obj as SetIndexBinder;
            return ia != null && ia._arguments.ListEquals(_arguments);
        }

        [Confined]
        public override int GetHashCode() {
            return SetMemberBinderHash ^ _arguments.ListHashCode();
        }

        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");

            return target.BindSetIndex(this, args);
        }

        public MetaObject FallbackSetIndex(MetaObject target, MetaObject[] args) {
            return FallbackSetIndex(target, args, null);
        }

        public abstract MetaObject FallbackSetIndex(MetaObject target, MetaObject[] args, MetaObject errorSuggestion);
    }
}
