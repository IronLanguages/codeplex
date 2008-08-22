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
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Microsoft.Scripting.Runtime;

using IronPython.Runtime;

namespace IronPythonTest {
    [Serializable]
    internal class StringDictionaryStorage : DictionaryStorage {
        private readonly IDictionary<string, object>/*!*/ _dict; // the underlying dictionary
        private Dictionary<object, object> _objDict;

        public StringDictionaryStorage(IDictionary<string, object>/*!*/ dict) {
            _dict = dict;
        }

        public StringDictionaryStorage(IDictionary<string, object> dict, Dictionary<object, object> objDict) {
            _dict = dict;
            _objDict = objDict;
        }

        public override void Add(object key, object value) {
            lock (this) {
                string strKey = key as string;
                if (strKey != null) {
                    _dict[strKey] = value;
                } else {
                    EnsureObjectDictionary();
                    _objDict[BaseSymbolDictionary.NullToObj(key)] = value;
                }
            }
        }

        public override bool Contains(object key) {
            lock (this) {
                string strKey = key as string;
                if (strKey != null) {
                    return _dict.ContainsKey(strKey);
                }

                if (_objDict != null) {
                    return _objDict.ContainsKey(BaseSymbolDictionary.NullToObj(key));
                }

                return false;
            }
        }

        public override bool Remove(object key) {
            lock (this) {
                string strKey = key as string;
                if (strKey != null) {
                    return _dict.Remove(strKey);
                }

                if (_objDict != null) {
                    return _objDict.Remove(BaseSymbolDictionary.NullToObj(key));
                }

                return false;
            }
        }

        public override bool TryGetValue(object key, out object value) {
            lock (this) {
                string strKey = key as string;
                if (strKey != null) {
                    return _dict.TryGetValue(strKey, out value);
                }

                if (_objDict != null) {
                    return _objDict.TryGetValue(BaseSymbolDictionary.NullToObj(key), out value);
                }

                value = null;
                return false;
            }
        }

        public override int Count {
            get {
                lock (this) {
                    int count = _dict.Count;
                    if (_objDict != null) {
                        count += _objDict.Count;
                    }
                    return count;
                }
            }
        }

        public override void Clear() {
            lock (this) {
                _dict.Clear();
                if (_objDict != null) {
                    _objDict.Clear();
                }
            }
        }

        public override List<KeyValuePair<object, object>> GetItems() {
            List<KeyValuePair<object, object>> res = new List<KeyValuePair<object, object>>();
            lock (this) {
                foreach (KeyValuePair<string, object> kvp in _dict) {
                    res.Add(new KeyValuePair<object, object>(kvp.Key, kvp.Value));
                }

                if (_objDict != null) {
                    foreach (KeyValuePair<object, object> kvp in _objDict) {
                        res.Add(kvp);
                    }
                }
            }
            return res;
        }
        
        public override DictionaryStorage Clone() {
            lock (this) {
                IDictionary<string, object> dict;
#if !SILVERLIGHT
                ICloneable cloneable = _dict as ICloneable;
                if (_dict != null) {
                    dict = (IDictionary<string, object>)cloneable.Clone();
                } else 
#endif
                {
                    dict = new Dictionary<string, object>(_dict);
                }

                Dictionary<object, object> objDict = null;
                if (_objDict != null) {
                    objDict = new Dictionary<object, object>(_objDict);
                }

                return new StringDictionaryStorage(dict, objDict);
            }
        }

        private void EnsureObjectDictionary() {
            if (_objDict == null) {
                Interlocked.CompareExchange<Dictionary<object, object>>(ref _objDict, new Dictionary<object, object>(), null);
            }            
        }
    }
}