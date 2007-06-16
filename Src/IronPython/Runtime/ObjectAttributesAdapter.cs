/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime.Operations;
using Microsoft.Scripting;
namespace IronPython.Runtime {
    class ObjectAttributesAdapter  : IAttributesCollection {
        private object _backing;
        public ObjectAttributesAdapter(object backing) {
            _backing = backing;
        }

        #region IAttributesCollection Members

        public void Add(SymbolId name, object value) {
            PythonOps.SetIndex(_backing, SymbolTable.IdToString(name), value);
        }

        public bool TryGetValue(SymbolId name, out object value) {
            string nameStr = SymbolTable.IdToString(name);

            try {
                value = PythonOps.GetIndex(_backing, nameStr);
                return true;
            } catch (KeyNotFoundException) {
                // return false
            }
            value = null;
            return false;
        }

        public bool Remove(SymbolId name) {
            try {
                PythonOps.DelIndex(_backing, SymbolTable.IdToString(name));
                return true;
            } catch (KeyNotFoundException) {
                return false;
            }
        }

        public bool ContainsKey(SymbolId name) {
            throw new Exception("The method or operation is not implemented.");
        }

        public object this[SymbolId name] {
            get {
                object res;
                if (TryGetValue(name, out res)) return res;

                throw PythonOps.NameError(name);
            }
            set {
                Add(name, value);
            }
        }

        public IDictionary<SymbolId, object> SymbolAttributes {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void AddObjectKey(object name, object value) {
            PythonOps.SetIndex(_backing, name, value);
        }

        public bool TryGetObjectValue(object name, out object value) {
            try {
                value = PythonOps.GetIndex(_backing, name);
                return true;
            } catch (KeyNotFoundException) {
                // return false
            }
            value = null;
            return false;
        }

        public bool RemoveObjectKey(object name) {
            try {
                PythonOps.DelIndex(_backing, name);
                return true;
            } catch (KeyNotFoundException) {
                return false;
            }
        }

        public bool ContainsObjectKey(object name) {
            throw new Exception("The method or operation is not implemented.");
        }

        public IDictionary<object, object> AsObjectKeyedDictionary() {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count {
            get { return PythonOps.Length(_backing); }
        }

        public ICollection<object> Keys {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator() {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
