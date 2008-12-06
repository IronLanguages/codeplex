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

namespace Microsoft.Scripting {
    public abstract class DeleteIndexBinder : DynamicMetaObjectBinder {
        private readonly ReadOnlyCollection<ArgumentInfo> _arguments;

        protected DeleteIndexBinder(params ArgumentInfo[] arguments)
            : this((IEnumerable<ArgumentInfo>)arguments) {
        }

        protected DeleteIndexBinder(IEnumerable<ArgumentInfo> arguments) {
            _arguments = arguments.ToReadOnly();
        }

        public ReadOnlyCollection<ArgumentInfo> Arguments {
            get { return _arguments; }
        }

        [Confined]
        public override bool Equals(object obj) {
            DeleteIndexBinder ia = obj as DeleteIndexBinder;
            return ia != null && ia._arguments.ListEquals(_arguments);
        }

        [Confined]
        public override int GetHashCode() {
            return DeleteIndexBinderHash ^ _arguments.ListHashCode();
        }

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");

            return target.BindDeleteIndex(this, args);
        }

        public DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] indexes) {
            return FallbackDeleteIndex(target, indexes, null);
        }

        public abstract DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion);
    }
}
