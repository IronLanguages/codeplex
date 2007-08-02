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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;

using Microsoft.Scripting;

[assembly: PythonExtensionTypeAttribute(typeof(BaseSymbolDictionary), typeof(DictionaryOps))]
namespace IronPython.Runtime {
    /// <summary>
    /// Provides both helpers for implementing Python dictionaries as well
    /// as providing public methods that should be exposed on all dictionary types.
    /// 
    /// Currently these are published on IDictionary&lt;object, object&gt;
    /// </summary>
    [PythonType("dict")]
    public static class DictionaryOps {
        static object nullObject = new object();

        #region Dictionary Public API Surface

        [OperatorMethod]
        public static bool __contains__([StaticThis]IDictionary<object, object> self, object value) {
            return self.ContainsKey(value);
        }

        [OperatorMethod]
        [return: MaybeNotImplemented]
        public static object __cmp__(IDictionary<object, object> self, object other) {
            IDictionary<object, object> oth = other as IDictionary<object, object>;
            // CompareTo is allowed to throw (string, int, etc... all do it if they don't get a matching type)
            if (oth == null) {
                object len, iteritems;
                if (!PythonOps.TryGetBoundAttr(DefaultContext.Default, other, Symbols.Length, out len) ||
                    !PythonOps.TryGetBoundAttr(DefaultContext.Default, other, SymbolTable.StringToId("iteritems"), out iteritems)) {
                    return PythonOps.NotImplemented;
                }

                // user-defined dictionary...
                int lcnt = self.Count;
                int rcnt = Converter.ConvertToInt32(PythonOps.CallWithContext(DefaultContext.Default, len));

                if (lcnt != rcnt) return lcnt > rcnt ? 1 : -1;

                return DictionaryOps.CompareToWorker(self, new List(PythonOps.CallWithContext(DefaultContext.Default, iteritems)));
            }

            CompareUtil.Push(self, oth);
            try {
                return DictionaryOps.CompareTo(self, oth);
            } finally {
                CompareUtil.Pop(self, oth);
            }
        }

        // Dictionary has an odd not-implemented check to support custom dictionaries and therefore
        // needs a custom __eq__ / __ne__ implementation.

        [return: MaybeNotImplemented]
        [OperatorMethod]
        public static object Equal(IDictionary<object, object> self, object other) {
            if (!(other is PythonDictionary || other is CustomSymbolDictionary || other is SymbolDictionary))
                return PythonOps.NotImplemented;

            return EqualsHelper(self, other);
        }

        [return: MaybeNotImplemented]
        [OperatorMethod]
        public static object GreaterThanOrEqual(IDictionary<object, object> self, object other) {
            object res = __cmp__(self, other);
            if (res == PythonOps.NotImplemented) return res;

            return ((int)res) >= 0;
        }

        [return: MaybeNotImplemented]
        [OperatorMethod]
        public static object GreaterThan(IDictionary<object, object> self, object other) {
            object res = __cmp__(self, other);
            if (res == PythonOps.NotImplemented) return res;

            return ((int)res) > 0;
        }

        [OperatorMethod]
        public static void __delitem__(IDictionary<object, object> self, object key) {
            if (!self.Remove(key)) {
                throw PythonOps.KeyError(key);
            }
        }

        public static IEnumerator __iter__(IDictionary<object, object> self) {
            return new DictionaryKeyEnumerator(self);
        }

        [return: MaybeNotImplemented]
        [OperatorMethod]
        public static object LessThanOrEqual(IDictionary<object, object> self, object other) {
            object res = __cmp__(self, other);
            if (res == PythonOps.NotImplemented) return res;

            return ((int)res) <= 0;
        }

        [OperatorMethod]
        public static int __len__(IDictionary<object, object> self) {
            return self.Count;
        }

        [return: MaybeNotImplemented]
        [OperatorMethod]
        public static object LessThan(IDictionary<object, object> self, object other) {
            object res = __cmp__(self, other);
            if (res == PythonOps.NotImplemented) return res;

            return ((int)res) < 0;
        }

        [return: MaybeNotImplemented]
        [OperatorMethod]
        public static object NotEqual(IDictionary<object, object> self, object other) {
            object res = Equal(self, other);
            if (res != PythonOps.NotImplemented) return PythonOps.Not(res);

            return res;
        }

        [OperatorMethod]
        public static string __repr__(IDictionary<object, object> self) {
            return __str__(self);
        }

        [OperatorMethod]
        public static string __str__(IDictionary<object, object> self) {
            StringBuilder buf = new StringBuilder();
            buf.Append("{");
            bool first = true;
            foreach (KeyValuePair<object, object> kv in self) {
                if (first) first = false;
                else buf.Append(", ");

                if (kv.Key == nullObject)
                    buf.Append("None");
                else
                    buf.Append(PythonOps.StringRepr(kv.Key));
                buf.Append(": ");

                // StringRepr enforces recursion for ICodeFormattable types, but
                // arbitrary dictionaries may not hit that.  We do the simple
                // recursive check here and let StringRepr handle the rest.
                if (Object.ReferenceEquals(kv.Value, self)) {
                    buf.Append("{...}");
                } else {
                    buf.Append(PythonOps.StringRepr(kv.Value));
                }
            }
            buf.Append("}");
            return buf.ToString();
        }

        public static void clear(IDictionary<object, object> self) {
            self.Clear();
        }

        public static object copy(IDictionary<object, object> self) {
            return new PythonDictionary(new Dictionary<object, object>(self));
        }

        public static object get(IDictionary<object, object> self, object key) {
            return get(self, key, null);
        }

        public static object get(IDictionary<object, object> self, object key, object defaultValue) {
            object ret;
            if (self.TryGetValue(key, out ret)) return ret;
            return defaultValue;
        }

        public static bool has_key(IDictionary<object, object> self, object key) {
            return self.ContainsKey(key);
        }

        public static List items(IDictionary<object, object> self) {
            List ret = List.MakeEmptyList(self.Count);
            foreach (KeyValuePair<object, object> kv in self) {
                ret.AddNoLock(Tuple.MakeTuple(kv.Key, kv.Value));
            }
            return ret;
        }

        public static IEnumerator iteritems(IDictionary<object, object> self) {
            return items(self).GetEnumerator();
        }

        public static IEnumerator iterkeys(IDictionary<object, object> self) {
            return keys(self).GetEnumerator();
        }

        public static IEnumerator itervalues(IDictionary<object, object> self) {
            return values(self).GetEnumerator();
        }

        public static List keys(IDictionary<object, object> self) {
            List l = List.Make(self.Keys);
            for (int i = 0; i < l.Count; i++) {
                if (l[i] == nullObject) {
                    l[i] = DictionaryOps.ObjToNull(l[i]);
                    break;
                }
            }
            return l;
        }

        public static object pop(IDictionary<object, object> self, object key) {
            //??? perf won't match expected Python perf
            object ret;
            if (self.TryGetValue(key, out ret)) {
                self.Remove(key);
                return ret;
            } else {
                throw PythonOps.KeyError(key);
            }
        }

        public static object pop(IDictionary<object, object> self, object key, object defaultValue) {
            //??? perf won't match expected Python perf
            object ret;
            if (self.TryGetValue(key, out ret)) {
                self.Remove(key);
                return ret;
            } else {
                return defaultValue;
            }
        }

        public static Tuple popitem(IDictionary<object, object> self) {
            IEnumerator<KeyValuePair<object, object>> ie = self.GetEnumerator();
            if (ie.MoveNext()) {
                object key = ie.Current.Key;
                object val = ie.Current.Value;
                self.Remove(key);
                return Tuple.MakeTuple(key, val);
            }
            throw PythonOps.KeyError("dictionary is empty");
        }

        public static object setdefault(IDictionary<object, object> self, object key) {
            return setdefault(self, key, null);
        }

        public static object setdefault(IDictionary<object, object> self, object key, object defaultValue) {
            object ret;
            if (self.TryGetValue(key, out ret)) return ret;
            self[key] = defaultValue;
            return defaultValue;
        }

        public static List values(IDictionary<object, object> self) {
            return List.Make(self.Values);
        }

        public static void update(IDictionary<object, object> self) {
        }

        public static void update(IDictionary<object, object> self, object b) {
            object keysFunc;
            IDictionary dict = b as IDictionary;
            if (dict != null) {
                IDictionaryEnumerator e = dict.GetEnumerator();
                while (e.MoveNext()) {
                    self[DictionaryOps.NullToObj(e.Key)] = e.Value;
                }
            } else if (PythonOps.TryGetBoundAttr(b, Symbols.Keys, out keysFunc)) {
                // user defined dictionary
                IEnumerator i = PythonOps.GetEnumerator(PythonCalls.Call(keysFunc));
                while (i.MoveNext()) {
                    self[DictionaryOps.NullToObj(i.Current)] = PythonOps.GetIndex(b, i.Current);
                }
            } else {
                // list of lists (key/value pairs), list of tuples,
                // tuple of tuples, etc...
                IEnumerator i = PythonOps.GetEnumerator(b);
                int index = 0;
                while (i.MoveNext()) {
                    if (!AddKeyValue(self, i.Current)) {
                        throw PythonOps.ValueError("dictionary update sequence element #{0} has bad length; 2 is required", index);
                    }
                    index++;
                }
            }
        }

        #endregion

        #region Dictionary Helper APIs

        internal static bool TryGetValueVirtual(CodeContext context, IMapping self, object key, ref object DefaultGetItem, out object value) {
            ISuperDynamicObject sdo = self as ISuperDynamicObject;
            if (sdo != null) {
                Debug.Assert(sdo != null);
                DynamicType myType = sdo.DynamicType;
                object ret;
                DynamicTypeSlot dts;

                if (DefaultGetItem == null) {
                    // lazy init our cached DefaultGetItem
                    TypeCache.Dict.TryLookupSlot(context, Symbols.GetItem, out dts);
                    bool res = dts.TryGetValue(context, self, TypeCache.Dict, out DefaultGetItem);
                    Debug.Assert(res);
                }

                // check and see if it's overriden
                if (myType.TryLookupSlot(context, Symbols.GetItem, out dts)) {
                    dts.TryGetValue(context, self, myType, out ret);

                    if (ret != DefaultGetItem) {
                        // subtype of dict that has overridden __getitem__
                        // we need to call the user's versions, and handle
                        // any exceptions.
                        try {
                            value = self[key];
                            return true;
                        } catch (KeyNotFoundException) {
                            value = null;
                            return false;
                        }
                    }
                }
            }

            value = null;
            return false;
        }

        internal static bool AddKeyValue(IDictionary<object, object> self, object o) {
            IEnumerator i = PythonOps.GetEnumerator(o); //c.GetEnumerator();
            if (i.MoveNext()) {
                object key = i.Current;
                if (i.MoveNext()) {
                    object value = i.Current;
                    self[key] = value;

                    return !i.MoveNext();
                }
            }
            return false;
        }

        internal static object NullToObj(object o) {
            if (o == null) return nullObject;
            return o;
        }

        internal static object ObjToNull(object o) {
            if (o == nullObject) return null;
            return o;
        }

        internal static int CompareTo(IDictionary<object, object> left, IDictionary<object, object> right) {
            int lcnt = left.Count;
            int rcnt = right.Count;

            if (lcnt != rcnt) return lcnt > rcnt ? 1 : -1;

            List ritems = DictionaryOps.items(right);
            return CompareToWorker(left, ritems);
        }

        internal static int CompareToWorker(IDictionary<object, object> left, List ritems) {
            List litems = DictionaryOps.items(left);

            litems.Sort();
            ritems.Sort();

            return litems.CompareToWorker(ritems);
        }

        internal static bool EqualsHelper(IDictionary<object, object> self, object other) {
            IDictionary<object, object> oth = other as IDictionary<object, object>;
            if (oth == null) return false;

            if (oth.Count != self.Count) return false;

            // we cannot call Compare here and compare against zero because Python defines
            // value equality as working even if the keys/values are unordered.
            List myKeys = keys(self);

            foreach (object o in myKeys) {
                object res;
                if (!oth.TryGetValue(o, out res)) return false;

                CompareUtil.Push(res);
                try {
                    if (!PythonOps.EqualRetBool(res, self[o])) return false;
                } finally {
                    CompareUtil.Pop(res);
                }
            }
            return true;
        }
        #endregion
    }
}