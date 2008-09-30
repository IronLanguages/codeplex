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
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Generation {

    /// <summary>
    /// Rewrites globals to elements on a closed over array
    /// </summary>
    internal class GlobalArrayRewriter : GlobalOptimizedRewriter {

        // This starts as a List<string>, but becomes readonly when we're finished allocating
        private IList<string> _names = new List<string>();
        private VariableExpression _array;

        internal ReadOnlyCollection<string> Names {
            get {
                ReadOnlyCollection<string> names = _names as ReadOnlyCollection<string>;
                if (names == null) {
                    // can't add any more items after this
                    _names = names = new ReadOnlyCollection<string>(_names);
                }
                return names;
            }
        }

        protected override Expression Visit(LambdaExpression node) {
            // Only run this for the top-level lambda
            if (_array == null) {
                _array = Expression.Variable(typeof(ModuleGlobalWrapper[]), "$globals");
                Expression body = AddScopedVariable(
                    node.Body,
                    _array,
                    Expression.Call(typeof(RuntimeHelpers).GetMethod("GetGlobalArray"), Context)
                );
                Debug.Assert(node.NodeType == ExpressionType.Lambda);
                node = Expression.Lambda(
                    node.Type,
                    body,
                    node.Name,
                    node.Annotations,
                    node.Parameters
                );
            }

            return base.Visit(node);
        }

        protected override Expression MakeWrapper(GlobalVariableExpression variable) {
            Debug.Assert(!_names.IsReadOnly);
            int index = _names.Count;
            _names.Add(variable.Name);
            return Expression.ArrayAccess(_array, Expression.Constant(index));
        }

        internal IAttributesCollection CreateDictionary() {
            return new GlobalsDictionary(SymbolTable.StringsToIds(Names));
        }
    }
}

namespace Microsoft.Scripting.Runtime {
    public static partial class RuntimeHelpers {
        // TODO: remove and get the array some other way
        public static ModuleGlobalWrapper[] GetGlobalArray(CodeContext context) {
            return ((GlobalsDictionary)context.GlobalScope.Dict).Data;
        }
    }
}
