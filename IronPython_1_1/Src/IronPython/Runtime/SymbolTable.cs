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
    public struct SymbolId : ISerializable, IComparable, IEquatable<SymbolId> {
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
            if (field == null)
                throw IronPython.Runtime.Operations.Ops.TypeError("attribute name must be string");

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

        public static string IdToString(SymbolId id) {
            PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "BackwardsFieldTableLookup " + ids[id.Id] == null ? "(null)" : ids[id.Id]);
            return ids[id.Id];
        }

        public static string[] IdsToStrings(IList<SymbolId> ids) {
            string[] ret = new string[ids.Count];
            for (int i = 0; i < ids.Count; i++) {
                if (ids[i] == Empty) ret[i] = null;
                else ret[i] = ids[i].GetString();
            }
            return ret;
        }

        static void PublishWellKnownSymbol(string name, SymbolId expectedSymbol) {
            SymbolId symbolId = StringToId(name);
            Debug.Assert(symbolId == expectedSymbol);
        }
    }

    /// <summary>
    /// Presents a flat enumerable view of multiple dictionaries
    /// </summary>
    class DictionaryUnionEnumerator : CheckedDictionaryEnumerator {
        IList<IDictionaryEnumerator> enums;
        int current = 0;

        public DictionaryUnionEnumerator(IList<IDictionaryEnumerator> enums) {
            this.enums = enums;
        }

        protected override object GetKey() {
            return enums[current].Key;
        }

        protected override object GetValue() {
            return enums[current].Value;
        }

        protected override bool DoMoveNext() {
            // Have we already walked over all the enumerators in the list?
            if (current == enums.Count)
                return false;

            // Are there any more entries in the current enumerator?
            if (enums[current].MoveNext())
                return true;

            // Move to the next enumerator in the list
            current++;

            // Make sure that the next enumerator is ready to be used
            return DoMoveNext();
        }

        protected override void DoReset() {
            for (int i = 0; i < enums.Count; i++) {
                enums[i].Reset();
            }
            current = 0;
        }

    }

    /// <summary>
    /// Exposes a IDictionary<SymbolId, object> as a IDictionary<object, object>
    /// </summary>
    class TransformDictEnum : CheckedDictionaryEnumerator {
        IEnumerator<KeyValuePair<SymbolId, object>> backing;

        public TransformDictEnum(IDictionary<SymbolId, object> backing) {
            this.backing = backing.GetEnumerator();
        }

        protected override object GetKey() {
            return SymbolTable.IdToString(backing.Current.Key);
        }

        protected override object GetValue() {
            return backing.Current.Value;
        }

        protected override bool DoMoveNext() {
            bool result = backing.MoveNext();
            if (result && backing.Current.Key == SymbolTable.ObjectKeys) {
                result = MoveNext();
            }
            return result;
        }

        protected override void DoReset() {
            backing.Reset();
        }
    }

}
