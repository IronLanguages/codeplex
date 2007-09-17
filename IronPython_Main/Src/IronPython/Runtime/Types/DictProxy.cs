/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Types;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;


namespace IronPython.Runtime.Types {
    [PythonType("dictproxy")]
    public class DictProxy : IMapping, IEnumerable {
        private DynamicMixin _dt;
        
        public DictProxy(DynamicMixin dt) {
            Debug.Assert(dt != null);
            _dt = dt;
        }

        public object GetIndex(CodeContext context, object index) {
            string strIndex = index as string;
            if (strIndex != null) {
                DynamicTypeSlot dts;
                if (_dt.TryLookupSlot(context, SymbolTable.StringToId(strIndex), out dts)) {
                    object res;
                    DynamicTypeUserDescriptorSlot uds = dts as DynamicTypeUserDescriptorSlot;
                    if (uds != null) {
                        return uds.Value;
                    }

                    if ((dts is DynamicTypeValueSlot) && dts.TryGetValue(context, null, _dt, out res)) {
                        return res;
                    }
                    return dts;
                }
            }

            throw PythonOps.KeyError(index.ToString());
        }

        public override bool Equals(object obj) {
            DictProxy proxy = obj as DictProxy;
            if (proxy == null) return false;

            return proxy._dt == _dt;
        }

        public override int GetHashCode() {
            return ~_dt.GetHashCode();
        }

        #region IMapping Members

        public object GetValue(object key) {
            return GetIndex(DefaultContext.Default, key);
        }

        public object GetValue(object key, object defaultValue) {
            object res;
            if (TryGetValue(key, out res))
                return res;
            return defaultValue;
        }

        public bool TryGetValue(object key, out object value) {
            string strIndex = key as string;
            if (strIndex != null) {
                DynamicTypeSlot dts;
                if (_dt.TryLookupSlot(DefaultContext.Default, SymbolTable.StringToId(strIndex), out dts)) {
                    DynamicTypeUserDescriptorSlot uds = dts as DynamicTypeUserDescriptorSlot;
                    if (uds != null) {
                        value = uds.Value;
                        return true;
                    }

                    value = dts;
                    return true;
                }
            }

            value = null;
            return false;
        }

        public object this[object key] {
            get {
                return GetIndex(DefaultContext.Default, key);
            }
            set {
                throw PythonOps.TypeError("cannot assign to dictproxy");
            }
        }

        public bool DeleteItem(object key) {
            throw PythonOps.TypeError("cannot delete from dictproxy");
        }

        #endregion

        #region IPythonContainer Members

        public int GetLength() {
            return _dt.GetMemberNames(DefaultContext.Default).Count;
        }

        [PythonName("__contains__")]
        public bool ContainsValue(object value) {
            return ContainsKey(value);
        }

        #endregion

        [PythonName("has_key")]
        public bool ContainsKey(object key) {
            object dummy;
            return TryGetValue(key, out dummy);
        }

        [PythonName("keys")]
        public object GetKeys(CodeContext context) {
            return new List(_dt.GetMemberDictionary(context).Keys);
        }

        [PythonName("values")]
        public object GetValues(CodeContext context) {
            List res = new List();
            foreach(KeyValuePair<object, object> kvp in _dt.GetMemberDictionary(context)){
                DynamicTypeUserDescriptorSlot dts = kvp.Value as DynamicTypeUserDescriptorSlot;

                if (dts != null) {
                    res.AddNoLock(dts.Value);
                } else {
                    res.AddNoLock(kvp.Value);
                }
            }

            return res;
        }        

        #region IEnumerable Members

        [PythonName("__iter__")]
        public System.Collections.IEnumerator GetEnumerator() {
            return DictionaryOps.iterkeys(_dt.GetMemberDictionary(DefaultContext.Default).AsObjectKeyedDictionary());
        }

        #endregion
    }
}
