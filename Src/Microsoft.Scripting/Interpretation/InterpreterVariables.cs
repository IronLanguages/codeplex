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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Interpretation {

    /// <summary>
    /// An ILocalVariables implementation for the interpreter
    /// 
    /// TODO: This isn't quite correct, because it doesn't implement the
    /// LocalScopeExpression.IsClosure feature that only exposes variables that
    /// would otherwise be lifted. To implement it correctly would require a
    /// full variable binding pass, something the interpreter doesn't need
    /// today. The only thing that this breaks is Python's func_closure
    /// </summary>
    internal sealed class InterpreterVariables : ILocalVariables {
        private readonly InterpreterState _state;
        private readonly ReadOnlyCollection<Expression> _vars;
        private ReadOnlyCollection<string> _names;

        internal InterpreterVariables(InterpreterState state, LocalScopeExpression node) {
            _state = state;
            _vars = node.Variables;
        }

        #region ILocalVariables Members

        public ReadOnlyCollection<string> Names {
            get {
                if (_names == null) {
                    string[] names = new string[_vars.Count];
                    for (int i = 0; i < _vars.Count; i++) {
                        if (_vars[i].NodeType == ExpressionType.Variable) {
                            names[i] = ((VariableExpression)_vars[i]).Name;
                        } else {
                            names[i] = ((ParameterExpression)_vars[i]).Name;
                        }
                    }
                    _names = new ReadOnlyCollection<string>(names);
                }
                return _names;
            }
        }

        #endregion

        #region IList<object> Members

        public int IndexOf(object item) {
            int i = 0;
            foreach (object element in this) {
                if (object.Equals(element, item)) {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public void Insert(int index, object item) {
            throw new NotSupportedException("variables cannot be added/removed");
        }

        public void RemoveAt(int index) {
            throw new NotSupportedException("variables cannot be added/removed");
        }

        public object this[int index] {
            get {
                ContractUtils.RequiresArrayIndex(this, index, "index");
                return _state.GetValue(_vars[index]);
            }
            set {
                ContractUtils.RequiresArrayIndex(this, index, "index");
                _state.SetValue(_vars[index], value);
            }
        }

        #endregion

        #region ICollection<object> Members

        public void Add(object item) {
            throw new NotSupportedException("variables cannot be added/removed");
        }

        public void Clear() {
            throw new NotSupportedException("variables cannot be added/removed");
        }

        public bool Contains(object item) {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(object[] array, int arrayIndex) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresArrayRange(array, arrayIndex, Count, "arrayIndex", "array");

            foreach (object value in this) {
                array[arrayIndex++] = value;
            }
        }

        public int Count {
            get { return _vars.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(object item) {
            throw new NotSupportedException("variables cannot be added/removed");
        }

        #endregion

        #region IEnumerable<object> Members

        public IEnumerator<object> GetEnumerator() {
            foreach (Expression v in _vars) {
                yield return _state.GetValue(v);
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }
}
