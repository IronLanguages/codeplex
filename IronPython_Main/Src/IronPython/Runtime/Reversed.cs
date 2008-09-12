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
using System.Text;
using System.Collections;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    [PythonType("reversed")]
    public class ReversedEnumerator : IEnumerator {
        private readonly object _getItemMethod;
        private readonly int _savedIndex;
        private object _current;
        private int _index;

        protected ReversedEnumerator(int length, object getitem) {
            this._index = this._savedIndex = length;
            this._getItemMethod = getitem;
        }

        public static object __new__(CodeContext context, PythonType type, object o) {
            object reversed;
            if (PythonOps.TryGetBoundAttr(context, o, Symbols.Reversed, out reversed)) {
                return PythonCalls.Call(context, reversed);
            }

            object getitem;
            object len;
            if (!PythonOps.TryGetBoundAttr(o, Symbols.GetItem, out getitem) ||
                !PythonOps.TryGetBoundAttr(o, Symbols.Length, out len) ||
                o is PythonDictionary) {
                throw PythonOps.TypeError("argument to reversed() must be a sequence");
            }

            object length = PythonCalls.Call(context, len);
            if (!(length is int)) {
                throw PythonOps.ValueError("__len__ must return int");
            }

            if (type.UnderlyingSystemType == typeof(ReversedEnumerator)) {
                return new ReversedEnumerator((int)length, getitem);
            }

            return type.CreateInstance(context, length, getitem);
        }

        public int __length_hint__() { return _savedIndex; }

        public ReversedEnumerator/*!*/ __iter__() {
            return this;
        }

        #region IEnumerator implementation

        object IEnumerator.Current {
            get {
                return _current;
            }
        }

        bool IEnumerator.MoveNext() {
            if (_index > 0) {
                _index--;
                _current = PythonCalls.Call(_getItemMethod, _index);
                return true;
            } else return false;
        }

        void IEnumerator.Reset() {
            _index = _savedIndex;
        }

        #endregion
    }

}
