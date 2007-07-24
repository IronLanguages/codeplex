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

namespace Microsoft.Scripting.Generation {
    public class ConstantPool {
        private List<object> _data;

        private List<Type> _types;
        private Slot _dataSlot;
        private CodeGen _cg;
        private static List<object> _staticData = new List<object>();
        private static object _nullVal = new object();
        private static int _lastCheck, _empties;

        public ConstantPool() {
            this._data = new List<object>();
            this._types = new List<Type>();
        }

        public Type SlotType {
            get { return typeof(object[]); }
        }

        public object[] Data {
            get { return _data.ToArray(); }
        }

        public Slot Slot {
            get { return _dataSlot; }
        }

        public int Count {
            get { return _data.Count; }
        }

        public void SetCodeGen(CodeGen cg, Slot dataSlot) {
            this._cg = cg;
            this._dataSlot = dataSlot;
        }

        public Slot AddData(object data) {
            return AddData(data, data == null ? typeof(object) : data.GetType());
        }

        public Slot AddData(object data, Type type) {
            Slot result;

            if (IsBound) {
                _data.Add(data);
                _types.Add(type);

                result = new IndexSlot(_dataSlot, _data.Count - 1);
                if (type != typeof(object)) {
                    // Use a CastSlot around an IndexSlot since we just want a cast and not a full conversion
                    result = new CastSlot(result, type);
                }
            } else {                
                int index = AddStaticData(data);
                
                result = _cg.TypeGen.AddStaticField(type, "#SlotStorage" + index.ToString());

            }
            return result;
        }

        /// <summary>
        /// True if the ConstantPool will be bound as the 1st argument to a delegate.  False
        /// if the constant pool storage will always go into static fields.
        /// </summary>
        public bool IsBound {
            get {
                return _dataSlot != null;
            }
        }

        public ConstantPool CopyData() {
            ConstantPool ret = new ConstantPool();
            ret._data = this._data;
            ret._types = this._types;
            return ret;
        }

        private static int AddStaticData(object data) {
            int index;
            lock (_staticData) {
                index = _staticData.Count;
                if (_empties != 0) {
                    for (int i = _lastCheck; i < _staticData.Count; i++) {
                        if (_staticData[i] == null) {
                            _staticData[i] = data == null ? _nullVal : data;
                            index = i;
                            _empties--;
                            break;
                        }
                    }
                }

                if (index == _staticData.Count) {
                    _lastCheck = 0;
                    _staticData.Add(data == null ? _nullVal : data);
                }
            }
            return index;
        }

        public static object GetConstantData(int index) {
            lock (_staticData) {
                object res = _staticData[index];
                _staticData[index] = null;
                _empties++;
                return res == _nullVal ? null : res;
            }
        }
    }
}
