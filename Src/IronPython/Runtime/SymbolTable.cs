/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

using System.Diagnostics;
using System.Threading;

namespace IronPython.Runtime {

    [Serializable]
    public struct SymbolId : ISerializable, IComparable {
        public int Id;

        public SymbolId(int value) {
            Id = value;
        }

        public override bool Equals(object obj) {
            if (!(obj is SymbolId)) return false;
            SymbolId other = (SymbolId)obj;
            return Id == other.Id;
        }

        public bool Equals(SymbolId other) {
            return Id == other.Id;
        }

        public override int GetHashCode() {
            return Id;
        }

        public override string ToString() {
            return SymbolTable.IdToString(this);
        }

        public static explicit operator SymbolId(string s) {
            return SymbolTable.StringToId(s);
        }

        public static bool operator ==(SymbolId a, SymbolId b) {
            return a.Id == b.Id;
        }

        public static bool operator !=(SymbolId a, SymbolId b) {
            return a.Id != b.Id;
        }

        public string GetString() {
            return SymbolTable.IdToString(this);
        }

        #region Cross-Domain/Process Serialization Support

        // When leaving a context we serialize out our ID as a name
        // rather than a raw ID.  When we enter a new context we 
        // consult it's FieldTable to get the ID of the symbol name in
        // the new context.

        public SymbolId(SerializationInfo info, StreamingContext context) {
            Id = SymbolTable.StringToId(info.GetString("symbolName")).Id;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("symbolName", SymbolTable.IdToString(this));
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj) {
            if (!(obj is SymbolId)) return -1;

            SymbolId other = (SymbolId)obj;
            return Id - other.Id;
        }

        #endregion
    }

    public static partial class SymbolTable {
        static object lockObj = new object();

        static Dictionary<string, int> idToFieldTable = new Dictionary<string, int>();
        static List<string> ids;

        public const int EmptyId = 0;
        /// <summary>SymbolId for null string</summary>
        public static readonly SymbolId Empty = new SymbolId(EmptyId);

        public const int InvalidId = -1;
        public static readonly SymbolId Invalid = new SymbolId(InvalidId);

        public const int ObjectKeysId = -2;
        public static readonly SymbolId ObjectKeys = new SymbolId(ObjectKeysId);

        static SymbolTable() {
            ids = new List<string>(LastWellKnownId);
            ids.Add(null);          // initialize the null string
            Initialize();           // add the rest of them
            Debug.Assert(ids.Count == LastWellKnownId, "Possible duplicate in the symbol table initialization");
        }

        public static SymbolId StringToId(string field) {
            if (field == null) {
                return Empty;
            } else {
                PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "FieldTable " + field.ToString());

                int res;
                lock (lockObj) {
                    if (!idToFieldTable.TryGetValue(field, out res)) {
                        // register new id...
                        res = ids.Count;
                        // Console.WriteLine("Registering {0} as {1}", field, res);
                        ids.Add(field);
                        idToFieldTable[field] = res;
                    }
                }
                return new SymbolId(res);
            }
        }

        public static string IdToString(SymbolId id) {
            PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "BackwardsFieldTableLookup " + ids[id.Id] == null ? "(null)" : ids[id.Id]);
            return ids[id.Id];
        }

        public static string[] IdsToStrings(SymbolId[] ids) {
            string[] ret = new string[ids.Length];
            for (int i = 0; i < ids.Length; i++) {
                if (ids[i] == Empty) ret[i] = null;
                else ret[i] = ids[i].GetString();
            }
            return ret;
        }
    }

    class DictionaryUnionEnumerator : IDictionaryEnumerator {
        IList<IDictionaryEnumerator> enums;
        int current;

        public DictionaryUnionEnumerator(IList<IDictionaryEnumerator> enums) {
            this.enums = enums;
        }

        #region IDictionaryEnumerator Members

        public DictionaryEntry Entry {
            get { return enums[current].Entry; }
        }

        public object Key {
            get { return enums[current].Key; }
        }

        public object Value {
            get { return enums[current].Value; }
        }

        #endregion

        #region IEnumerator Members

        public object Current {
            get { return enums[current].Current; }
        }

        public bool MoveNext() {
            if (current == enums.Count) return false;

            if (!enums[current].MoveNext()) {
                current++;
                if (current == enums.Count) return false;
            }
            return true;
        }

        public void Reset() {
            current = 0;
            for (int i = 0; i < enums.Count; i++) {
                enums[i].Reset();
            }
        }

        #endregion
    }

    class TransformDictEnum : IDictionaryEnumerator {
        IEnumerator<KeyValuePair<SymbolId, object>> backing;
        public TransformDictEnum(IDictionary<SymbolId, object> backing) {
            this.backing = backing.GetEnumerator();
        }

        #region IDictionaryEnumerator Members

        public DictionaryEntry Entry {
            get { return new DictionaryEntry(SymbolTable.IdToString(backing.Current.Key), backing.Current.Value); }
        }

        public object Key {
            get { return SymbolTable.IdToString(backing.Current.Key); }
        }

        public object Value {
            get { return backing.Current.Key; }
        }

        #endregion

        #region IEnumerator Members

        public object Current {
            get { return Entry; }
        }

        public bool MoveNext() {
            bool result = backing.MoveNext();
            if (result && backing.Current.Key == SymbolTable.ObjectKeys) {
                result = MoveNext();
            }
            return result;
        }

        public void Reset() {
            backing.Reset();
        }

        #endregion
    }

}
