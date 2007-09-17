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

using Microsoft.Scripting;
using Microsoft.Scripting.Types;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Custom dictionary use for old class instances so commonly used
    /// items can be accessed quickly w/o requiring dictionary access.
    /// 
    /// Keys are only added to the dictionary, once added they are never
    /// removed.
    /// 
    /// TODO Merge this with TupleDictionary
    /// </summary>
    [PythonType(typeof(PythonDictionary))]
    public class CustomOldClassDictionary : CustomSymbolDictionary {
        private int _keyVersion;
        private SymbolId[] _extraKeys;
        private object[] _values;

        public CustomOldClassDictionary(SymbolId[] extraKeys, int keyVersion) {
            _extraKeys = extraKeys;
            _keyVersion = keyVersion;
            _values = new object[extraKeys.Length];
            for (int i = 0; i < _values.Length; i++) {
                _values[i] = Uninitialized.Instance;
            }
        }

        public int KeyVersion {
            get {
                return _keyVersion;
            }
        }

        public override SymbolId[] GetExtraKeys() {
            return _extraKeys;
        }

        public int FindKey(SymbolId key) {
            for (int i = 0; i < _extraKeys.Length; i++) {
                if (_extraKeys[i] == key) {
                    return i;
                }
            }
            return -1;
        }

        public object GetExtraValue(int index) {
            return _values[index];
        }

        public object GetValueHelper(int index, object oldInstance) {
            object ret = _values[index];
            if (ret != Uninitialized.Instance) return ret;
            //TODO this should go to a faster path since we know it's not in the dict
            return ((OldInstance)oldInstance).GetBoundMember(null, _extraKeys[index]);
        }

        public void SetExtraValue(int index, object value) {
            _values[index] = value;
        }

        protected override bool TrySetExtraValue(SymbolId keyId, object value) {
            int key = keyId.Id;
            for (int i = 0; i < _extraKeys.Length; i++) {
                // see if we already have a key (once keys are assigned
                // they never change) that matches this ID.
                if (_extraKeys[i].Id == key) {
                    _values[i] = value;
                    return true;
                }
            }
            return false;
        }

        protected override bool TryGetExtraValue(SymbolId keyId, out object value) {
            int key = keyId.Id;
            for (int i = 0; i < _extraKeys.Length; i++) {
                if (_extraKeys[i].Id == key) {
                    value = _values[i];
                    return true;
                }
            }
            value = null;
            return false;
        }

        [PythonClassMethod("fromkeys")]
        public static object FromKeys(CodeContext context, DynamicType cls, object seq) {
            return PythonDictionary.FromKeys(context, cls, seq, null);
        }

        [PythonClassMethod("fromkeys")]
        public static object FromKeys(CodeContext context, DynamicType cls, object seq, object value) {
            return PythonDictionary.FromKeys(context, cls, seq, value);
        }

        public override string ToString() {
            return DictionaryOps.__str__(this);
        }
    }
}
