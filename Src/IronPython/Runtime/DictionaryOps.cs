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
        public static bool Contains([StaticThis]IDictionary<object, object> self, object value) {
            return self.ContainsKey(value);
        }

        [OperatorMethod, PythonName("__cmp__")]
        [return: MaybeNotImplemented]
        public static object CompareTo(IDictionary<object, object> self, object other) {
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
            object res = CompareTo(self, other);
            if (res == PythonOps.NotImplemented) return res;

            return ((int)res) >= 0;
        }

        [return: MaybeNotImplemented]
        [OperatorMethod]
        public static object GreaterThan(IDictionary<object, object> self, object other) {
            object res = CompareTo(self, other);
            if (res == PythonOps.NotImplemented) return res;

            return ((int)res) > 0;
        }

        [OperatorMethod, PythonName("__delitem__")]
        public static void DelIndex(IDictionary<object, object> self, object key) {
            if (!self.Remove(key)) {
                throw PythonOps.KeyError(key);
            }
        }

        [PythonName("__iter__")]
        public static IEnumerator GetEnumerator(IDictionary<object, object> self) {
            return new DictionaryKeyEnumerator(self);
        }

        [return: MaybeNotImplemented]
        [OperatorMethod]
        public static object LessThanOrEqual(IDictionary<object, object> self, object other) {
            object res = CompareTo(self, other);
            if (res == PythonOps.NotImplemented) return res;

            return ((int)res) <= 0;
        }

        [OperatorMethod, PythonName("__len__")]
        public static int Length(IDictionary<object, object> self) {
            return self.Count;
        }

        [return: MaybeNotImplemented]
        [OperatorMethod]
        public static object LessThan(IDictionary<object, object> self, object other) {
            object res = CompareTo(self, other);
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

        [OperatorMethod, PythonName("__repr__")]
        public static string ToCodeString(IDictionary<object, object> self) {
            return ToString(self);
        }

        [OperatorMethod, PythonName("__str__")]
        public static string ToString(IDictionary<object, object> self) {
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

        [PythonName("clear")]
        public static void Clear(IDictionary<object, object> self) {
            self.Clear();
        }

        [PythonName("copy")]
        public static object Clone(IDictionary<object, object> self) {
            return new PythonDictionary(new Dictionary<object, object>(self));
        }

        [PythonName("get")]
        public static object GetIndex(IDictionary<object, object> self, object key) {
            return GetIndex(self, key, null);
        }

        [PythonName("get")]
        public static object GetIndex(IDictionary<object, object> self, object key, object defaultValue) {
            object ret;
            if (self.TryGetValue(key, out ret)) return ret;
            return defaultValue;
        }

        [PythonName("has_key")]
        public static bool HasKey(IDictionary<object, object> self, object key) {
            return self.ContainsKey(key);
        }

        [PythonName("items")]
        public static List Items(IDictionary<object, object> self) {
            List ret = List.MakeEmptyList(self.Count);
            foreach (KeyValuePair<object, object> kv in self) {
                ret.AddNoLock(Tuple.MakeTuple(kv.Key, kv.Value));
            }
            return ret;
        }

        [PythonName("iteritems")]
        public static IEnumerator IterItems(IDictionary<object, object> self) {
            return Items(self).GetEnumerator();
        }

        [PythonName("iterkeys")]
        public static IEnumerator IterKeys(IDictionary<object, object> self) {
            return Keys(self).GetEnumerator();
        }

        [PythonName("itervalues")]
        public static IEnumerator IterValues(IDictionary<object, object> self) {
            return Values(self).GetEnumerator();
        }

        [PythonName("keys")]
        public static List Keys(IDictionary<object, object> self) {
            List l = List.Make(self.Keys);
            for (int i = 0; i < l.Count; i++) {
                if (l[i] == nullObject) {
                    l[i] = DictionaryOps.ObjToNull(l[i]);
                    break;
                }
            }
            return l;
        }

        [PythonName("pop")]
        public static object Pop(IDictionary<object, object> self, object key) {
            //??? perf won't match expected Python perf
            object ret;
            if (self.TryGetValue(key, out ret)) {
                self.Remove(key);
                return ret;
            } else {
                throw PythonOps.KeyError(key);
            }
        }

        [PythonName("pop")]
        public static object Pop(IDictionary<object, object> self, object key, object defaultValue) {
            //??? perf won't match expected Python perf
            object ret;
            if (self.TryGetValue(key, out ret)) {
                self.Remove(key);
                return ret;
            } else {
                return defaultValue;
            }
        }

        [PythonName("popitem")]
        public static Tuple PopItem(IDictionary<object, object> self) {
            IEnumerator<KeyValuePair<object, object>> ie = self.GetEnumerator();
            if (ie.MoveNext()) {
                object key = ie.Current.Key;
                object val = ie.Current.Value;
                self.Remove(key);
                return Tuple.MakeTuple(key, val);
            }
            throw PythonOps.KeyError("dictionary is empty");
        }

        [PythonName("setdefault")]
        public static object SetDefault(IDictionary<object, object> self, object key) {
            return SetDefault(self, key, null);
        }

        [PythonName("setdefault")]
        public static object SetDefault(IDictionary<object, object> self, object key, object defaultValue) {
            object ret;
            if (self.TryGetValue(key, out ret)) return ret;
            self[key] = defaultValue;
            return defaultValue;
        }

        [PythonName("values")]
        public static List Values(IDictionary<object, object> self) {
            return List.Make(self.Values);
        }

        [PythonName("update")]
        public static void Update(IDictionary<object, object> self) {
        }

        [PythonName("update")]
        public static void Update(IDictionary<object, object> self, object b) {
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

        public static bool TryGetValueVirtual(CodeContext context, IMapping self, object key, ref object DefaultGetItem, out object value) {
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

        public static bool AddKeyValue(IDictionary<object, object> self, object o) {
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

        public static object NullToObj(object o) {
            if (o == null) return nullObject;
            return o;
        }

        public static object ObjToNull(object o) {
            if (o == nullObject) return null;
            return o;
        }

        public static int CompareTo(IDictionary<object, object> left, IDictionary<object, object> right) {
            int lcnt = left.Count;
            int rcnt = right.Count;

            if (lcnt != rcnt) return lcnt > rcnt ? 1 : -1;

            List ritems = DictionaryOps.Items(right);
            return CompareToWorker(left, ritems);
        }

        public static int CompareToWorker(IDictionary<object, object> left, List ritems) {
            List litems = DictionaryOps.Items(left);

            litems.Sort();
            ritems.Sort();

            return litems.CompareToWorker(ritems);
        }

        public static bool EqualsHelper(IDictionary<object, object> self, object other) {
            IDictionary<object, object> oth = other as IDictionary<object, object>;
            if (oth == null) return false;

            if (oth.Count != self.Count) return false;

            // we cannot call Compare here and compare against zero because Python defines
            // value equality as working even if the keys/values are unordered.
            List myKeys = Keys(self);

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
