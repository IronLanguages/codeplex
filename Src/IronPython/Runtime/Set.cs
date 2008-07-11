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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Scripting;
using System.Scripting.Runtime;
using System.Text;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {

    /// <summary>
    /// Common interface shared by both Set and FrozenSet
    /// </summary>
    public interface ISet : IEnumerable, IEnumerable<object> {
        int __len__();

        bool __contains__(object value);
        bool issubset(object set);
        bool issuperset(CodeContext context, object set);

        // private methods used for operations between set types.
        ISet PrivDifference(IEnumerable set);
        ISet PrivIntersection(IEnumerable set);
        ISet PrivSymmetricDifference(IEnumerable set);
        ISet PrivUnion(IEnumerable set);
        void PrivAdd(object adding);
        void PrivRemove(object removing);
        void PrivFreeze();
        void SetData(IEnumerable set);
    }

    /// <summary>
    /// Contains common set functionality between set and forzenSet
    /// </summary>
    static class SetHelpers {
        public static string SetToString(object set, IEnumerable items) {
            string setTypeStr;
            Type setType = set.GetType();
            if (setType == typeof(SetCollection)) {
                setTypeStr = "set";
            } else if (setType == typeof(FrozenSetCollection)) {
                setTypeStr = "frozenset";
            } else {
                setTypeStr = PythonTypeOps.GetName(set);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(setTypeStr);
            sb.Append("([");
            string comma = "";
            foreach (object o in items) {
                sb.Append(comma);
                sb.Append(PythonOps.Repr(o));
                comma = ", ";
            }
            sb.Append("])");

            return sb.ToString();
        }

        /// <summary>
        /// Creates a set that can be hashable.  If the set is currently a FrozenSet the
        /// set is returned.  If the set is a normal Set then a FrozenSet is returned
        /// with its contents.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object GetHashableSetIfSet(object o) {
            SetCollection asSet = o as SetCollection;
            if (asSet != null) {
                return FrozenSetCollection.Make(((IEnumerable)asSet).GetEnumerator());
            }
            return o;
        }

        public static ISet MakeSet(object setObj) {
            Type t = setObj.GetType();
            if (t == typeof(SetCollection)) {
                return new SetCollection();
            } else if (t == typeof(FrozenSetCollection)) {
                return new FrozenSetCollection();
            } else {
                // subclass                
                PythonType dt = DynamicHelpers.GetPythonType(setObj);

                ISet set = PythonCalls.Call(dt) as ISet;
                Debug.Assert(set != null);

                return set;
            }
        }

        public static ISet MakeSet(object setObj, ISet set) {
            Type t = setObj.GetType();
            if (t == typeof(SetCollection)) {
                return new SetCollection(set);
            } else if (t == typeof(FrozenSetCollection)) {
                return new FrozenSetCollection(set);
            } else {
                // subclass                
                PythonType dt = DynamicHelpers.GetPythonType(setObj);

                ISet res = PythonCalls.Call(dt) as ISet;

                Debug.Assert(res != null);
                res.SetData(set);
                return res;
            }
        }

        public static ISet Intersection(ISet x, object y) {
            ISet res = SetHelpers.MakeSet(x);

            IEnumerator ie = PythonOps.GetEnumerator(y);
            while (ie.MoveNext()) {
                if (x.__contains__(ie.Current))
                    res.PrivAdd(ie.Current);
            }
            res.PrivFreeze();
            return res;
        }

        public static ISet Difference(ISet x, object y) {
            ISet res = SetHelpers.MakeSet(x, x) as ISet;
            Debug.Assert(res != null);

            IEnumerator ie = PythonOps.GetEnumerator(y);
            while (ie.MoveNext()) {
                if (res.__contains__(ie.Current)) {
                    res.PrivRemove(ie.Current);
                }
            }
            res.PrivFreeze();
            return res;
        }

        public static ISet SymmetricDifference(ISet x, object y) {
            SetCollection otherSet = new SetCollection(PythonOps.GetEnumerator(y));       //make a set to deal w/ dups in the enumerator
            ISet res = SetHelpers.MakeSet(x, x) as ISet;
            Debug.Assert(res != null);

            foreach (object o in otherSet) {
                if (res.__contains__(o)) {
                    res.PrivRemove(o);
                } else {
                    res.PrivAdd(o);
                }
            }
            res.PrivFreeze();
            return res;
        }

        public static ISet Union(ISet x, object y) {
            ISet set = SetHelpers.MakeSet(x, x);
            IEnumerator ie = PythonOps.GetEnumerator(y);
            while (ie.MoveNext()) {
                set.PrivAdd(ie.Current);
            }
            set.PrivFreeze();
            return set;
        }

        public static bool IsSubset(ISet x, object y) {
            SetCollection set = new SetCollection(PythonOps.GetEnumerator(y));
            foreach (object o in x) {
                if (!set.__contains__(o)) {
                    return false;
                }
            }
            return true;
        }

        public static PythonTuple Reduce(PythonDictionary items, PythonType type) {
            object[] keys = new object[items.keys().__len__()];
            ((IList)items.keys()).CopyTo(keys, 0);
            return PythonTuple.MakeTuple(type, PythonTuple.MakeTuple(List.FromArrayNoCopy(keys)), null);
        }
    }

    /// <summary>
    /// Mutable set class
    /// </summary>
    [PythonSystemType("set")]
    public class SetCollection : ISet, IValueEquality, ICodeFormattable, ICollection {
        PythonDictionary items;

        #region Set contruction

        public void __init__() {
            clear();
        }

        public void __init__(object setData) {
            clear();
            update(setData);
        }

        public SetCollection() {
            items = new PythonDictionary();
        }

        internal SetCollection(object setData) {
            Init(setData);
        }

        internal SetCollection(IEnumerator setData) {
            items = new PythonDictionary();
            while (setData.MoveNext()) {
                add(setData.Current);
            }
        }

        internal SetCollection(PythonDictionary setData) {
            items = setData;
        }

        #endregion

        #region ISet

        public int __len__() {
            return items.__len__();
        }

        public bool __contains__(object value) {
            // promote sets to FrozenSet's for contains checks (so we get a hash code)
            value = SetHelpers.GetHashableSetIfSet(value);

            PythonOps.Hash(value);    // make sure we have a hashable item
            return items.__contains__(value);
        }

        public bool issubset(object set) {
            return SetHelpers.IsSubset(this, set);
        }

        public bool issuperset(CodeContext context, object set) {
            return this >= new SetCollection(PythonOps.GetEnumerator(set));
        }

        ISet ISet.PrivDifference(IEnumerable set) {
            return (ISet)difference(set);
        }

        ISet ISet.PrivIntersection(IEnumerable set) {
            return (ISet)intersection(set);
        }

        ISet ISet.PrivSymmetricDifference(IEnumerable set) {
            return (ISet)symmetric_difference(set);
        }

        ISet ISet.PrivUnion(IEnumerable set) {
            return (ISet)union(set);
        }

        void ISet.PrivAdd(object adding) {
            add(adding);
        }

        void ISet.PrivRemove(object removing) {
            remove(removing);
        }

        void ISet.PrivFreeze() {
            // nop for non-frozen sets.
        }

        void ISet.SetData(IEnumerable set) {
            items = new PythonDictionary();
            foreach (object o in set) {
                items[o] = o;
            }
        }

        #endregion

        #region NonOperator Operations

        public object union(object s) {
            return SetHelpers.Union(this, s);
        }

        public object intersection(object s) {
            return SetHelpers.Intersection(this, s);
        }

        public object difference(object s) {
            return SetHelpers.Difference(this, s);
        }

        public object symmetric_difference(object s) {
            return SetHelpers.SymmetricDifference(this, s);
        }

        public SetCollection copy() {
            return new SetCollection(((IEnumerable)this).GetEnumerator());
        }

        public PythonTuple __reduce__() {
            return SetHelpers.Reduce(items, DynamicHelpers.GetPythonTypeFromType(typeof(SetCollection)));
        }

        private void Init(params object[] o) {
            if (o.Length > 1) throw PythonOps.TypeError("set expected at most 1 arguments, got {0}", o.Length);

            items = new PythonDictionary();
            if (o.Length != 0) {
                IEnumerator setData = PythonOps.GetEnumerator(o[0]);
                while (setData.MoveNext()) {
                    add(setData.Current);
                }
            }
        }

        #endregion

        #region Mutating Members

        /// <summary>
        /// Appends one IEnumerable to an existing set
        /// </summary>
        /// <param name="s"></param>
        public void update(object s) {
            IEnumerator ie = PythonOps.GetEnumerator(s);
            while (ie.MoveNext()) {
                object o = ie.Current;
                if (!__contains__(o)) {
                    add(o);
                }
            }
        }

        public void add(object o) {
            PythonOps.Hash(o);// make sure we're hashable
            if (!items.__contains__(o)) {
                items[o] = o;
            }
        }

        public void intersection_update(object s) {
            SetCollection set = intersection(s) as SetCollection;
            items = set.items;
        }

        public void difference_update(object s) {
            SetCollection set = new SetCollection(PythonOps.GetEnumerator(s));
            foreach (object o in set) {
                if (__contains__(o)) {
                    remove(o);
                }
            }
        }

        public void symmetric_difference_update(object s) {
            SetCollection set = new SetCollection(PythonOps.GetEnumerator(s));
            foreach (object o in set) {
                if (__contains__(o)) {
                    remove(o);
                } else {
                    add(o);
                }
            }
        }

        public void remove(object o) {
            o = SetHelpers.GetHashableSetIfSet(o);

            PythonOps.Hash(o);
            if (!items.__contains__(o)) throw PythonOps.KeyError(o);

            items.__delitem__(o);
        }

        public void discard(object o) {
            o = SetHelpers.GetHashableSetIfSet(o);

            ((IDictionary)items).Remove(o);
        }

        public object pop() {
            foreach (object o in items.keys()) {
                items.__delitem__(o);
                return o;
            }
            throw PythonOps.KeyError("pop from an empty set");
        }

        public void clear() {
            items.clear();
        }

        #endregion

        #region Operators

        [SpecialName]
        public SetCollection InPlaceAnd(object s) {
            ISet set = s as ISet;
            if (set == null) throw PythonOps.TypeError("unsupported operand type(s) for &=: {0} and {1}", PythonOps.StringRepr(PythonTypeOps.GetName(s)), PythonOps.StringRepr(PythonTypeOps.GetName(this)));

            intersection_update(set);
            return this;
        }

        [SpecialName]
        public SetCollection InPlaceOr(object s) {
            ISet set = s as ISet;
            if (set == null) throw PythonOps.TypeError("unsupported operand type(s) for |=: {0} and {1}", PythonOps.StringRepr(PythonTypeOps.GetName(s)), PythonOps.StringRepr(PythonTypeOps.GetName(this)));

            update(set);
            return this;
        }

        [SpecialName]
        public SetCollection InPlaceSubtract(object s) {
            ISet set = s as ISet;
            if (set == null) throw PythonOps.TypeError("unsupported operand type(s) for -=: {0} and {1}", PythonOps.StringRepr(PythonTypeOps.GetName(s)), PythonOps.StringRepr(PythonTypeOps.GetName(this)));

            difference_update(set);
            return this;
        }

        [SpecialName]
        public SetCollection InPlaceExclusiveOr(object s) {
            ISet set = s as ISet;
            if (set == null) throw PythonOps.TypeError("unsupported operand type(s) for ^=: {0} and {1}", PythonOps.StringRepr(PythonTypeOps.GetName(s)), PythonOps.StringRepr(PythonTypeOps.GetName(this)));

            symmetric_difference_update(set);
            return this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o"), SpecialName]
        public int Compare(object o) {
            throw PythonOps.TypeError("cannot compare sets using cmp()");
        }

        public int __cmp__(object o) {
            throw PythonOps.TypeError("cannot compare sets using cmp()");
        }

        public static object operator &(SetCollection x, ISet y) {
            if (y.__len__() < x.__len__()) {
                return x.intersection(y);
            }

            SetCollection setc = y as SetCollection;
            if (setc != null) {
                return setc.intersection(x);
            }

            return SetHelpers.MakeSet(x, y.PrivIntersection(x));
        }

        public static object operator |(SetCollection x, ISet y) {
            if (y.__len__() < x.__len__()) {
                return x.union(y);
            }

            SetCollection setc = y as SetCollection;
            if (setc != null) {
                return setc.union(x);
            }

            return SetHelpers.MakeSet(x, y.PrivUnion(x));
        }

        public static object operator ^(SetCollection x, ISet y) {
            if (y.__len__() < x.__len__()) {
                return x.symmetric_difference(y);
            }

            SetCollection setc = y as SetCollection;
            if (setc != null) {
                return setc.symmetric_difference(x);
            }

            return SetHelpers.MakeSet(x, y.PrivSymmetricDifference(x));
        }

        public static object operator -(SetCollection x, ISet y) {
            return x.difference(y);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            int count = this.items.__len__();

            foreach (object o in items.keys()) {
                if (count != this.items.__len__()) {
                    throw PythonOps.RuntimeError("set changed during iteration");
                }

                yield return o;
            }
        }

        #endregion

        #region IRichComparable

        public static bool operator >(SetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) throw PythonOps.TypeError("can only compare to a set");

            if (s.__len__() >= self.__len__()) return false;

            foreach (object o in s) {
                if (!self.__contains__(o)) return false;
            }
            return true;
        }

        public static bool operator <(SetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) throw PythonOps.TypeError("can only compare to a set");

            if (s.__len__() <= self.__len__()) return false;

            foreach (object o in self) {
                if (!s.__contains__(o)) {
                    return false;
                }
            }
            return true;
        }

        public static bool operator >=(SetCollection self, object other) {
            return self > other || ((IValueEquality)self).ValueEquals(other);
        }

        public static bool operator <=(SetCollection self, object other) {
            return self < other || ((IValueEquality)self).ValueEquals(other);
        }

        #endregion

        #region IValueEquality Members

        // default conversion of protocol methods only allow's our specific type for equality,
        // sets can do __eq__ / __ne__ against any type though.  That's why we have a seperate
        // __eq__ / __ne__ here.

        int IValueEquality.GetValueHashCode() {
            throw PythonOps.TypeError("set objects are unhashable");
        }

        bool IValueEquality.ValueEquals(object other) {
            ISet set = other as ISet;
            if (set != null) {
                if (set.__len__() != __len__()) return false;
                return set.issubset(this) && this.issubset(set);
            }
            return false;
        }

        public bool __eq__(object other) {
            return ((IValueEquality)this).ValueEquals(other);
        }

        public bool __ne__(object other) {
            return !((IValueEquality)this).ValueEquals(other);
        }

        #endregion

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            int count = this.items.__len__();

            foreach (object o in items.keys()) {
                if (count != this.items.__len__()) {
                    throw PythonOps.RuntimeError("set changed during iteration");
                }

                yield return o;
            }
        }

        #endregion        

        #region ICodeFormattable Members

        public virtual string/*!*/ __repr__(CodeContext/*!*/ context) {
            return SetHelpers.SetToString(this, this.items.keys());
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        int ICollection.Count {
            get { return this.items.__len__(); }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return this; }
        }

        #endregion      
    }


    /// <summary>
    /// Non-mutable set class
    /// </summary>
    [PythonSystemType("frozenset")]
    public class FrozenSetCollection : ISet, IValueEquality, ICodeFormattable, ICollection {
        internal static readonly FrozenSetCollection EMPTY = new FrozenSetCollection();

        PythonDictionary items;
        int hashCode;
#if DEBUG
        int returnedHc;
#endif

        #region Set Construction

        public static FrozenSetCollection __new__(CodeContext context, object cls) {
            if (cls == TypeCache.FrozenSet) {
                return EMPTY;
            } else {
                PythonType dt = cls as PythonType;
                object res = dt.CreateInstance(context);
                FrozenSetCollection fs = res as FrozenSetCollection;
                if (fs == null) throw PythonOps.TypeError("{0} is not a subclass of frozenset", res);
                return fs;
            }
        }

        public static FrozenSetCollection __new__(CodeContext context, object cls, object setData) {
            if (cls == TypeCache.FrozenSet) {
                FrozenSetCollection fs = setData as FrozenSetCollection;
                if (fs != null) {
                    // constructing frozen set from set, we return the original frozen set.
                    return fs;
                }

                fs = FrozenSetCollection.Make(setData);
                return fs;
            } else {
                object res = ((PythonType)cls).CreateInstance(context, setData);
                FrozenSetCollection fs = res as FrozenSetCollection;
                if (fs == null) throw PythonOps.TypeError("{0} is not a subclass of frozenset", res);

                return fs;
            }
        }

        internal static FrozenSetCollection Make(object setData) {
            FrozenSetCollection fs = setData as FrozenSetCollection;
            if (fs != null) {
                // constructing frozen set from set, we return the original frozen set.
                return fs;
            }

            PythonDictionary items = ListToDictionary(setData);

            if (items.__len__() == 0) {
                fs = EMPTY;
            } else {
                fs = new FrozenSetCollection(items);
            }

            return fs;
        }

        private static PythonDictionary ListToDictionary(object set) {
            IEnumerator setData = PythonOps.GetEnumerator(set);
            PythonDictionary items = new PythonDictionary();
            while (setData.MoveNext()) {
                object o = setData.Current;
                if (!items.__contains__(o)) {
                    items[o] = o;
                }
            }
            return items;
        }

        public FrozenSetCollection() {
            items = new PythonDictionary();
            // hash code is 0 for empty set
            CalculateHashCode();
        }

        private FrozenSetCollection(PythonDictionary set) {
            items = set;
            CalculateHashCode();
        }

        protected FrozenSetCollection(object set)
            : this(ListToDictionary(set)) {
        }

        internal FrozenSetCollection(ISet set)
            : this((object)set) {
        }

        #endregion

        #region ISet

        public int __len__() {
            return items.__len__();
        }

        public bool __contains__(object value) {
            // promote sets to FrozenSet's for contains checks (so we get a hash code)
            value = SetHelpers.GetHashableSetIfSet(value);

            PythonOps.Hash(value);// make sure we have a hashable item
            return items.__contains__(value);
        }

        public bool issubset(object set) {
            return SetHelpers.IsSubset(this, set);
        }

        public bool issuperset(CodeContext context, object set) {
            return this >= FrozenSetCollection.Make(set);
        }

        ISet ISet.PrivDifference(IEnumerable set) {
            return (ISet)difference(set);
        }

        ISet ISet.PrivIntersection(IEnumerable set) {
            return (ISet)intersection(set);
        }

        ISet ISet.PrivSymmetricDifference(IEnumerable set) {
            return (ISet)symmetric_difference(set);
        }

        ISet ISet.PrivUnion(IEnumerable set) {
            return (ISet)union(set);
        }

        void ISet.PrivAdd(object adding) {
            PythonOps.Hash(adding);// make sure we're hashable
            if (!items.__contains__(adding)) {
                items[adding] = adding;
            }
        }

        void ISet.PrivRemove(object removing) {
            PythonOps.Hash(removing);// make sure we're hashable
            items.__delitem__(removing);
        }

        void ISet.SetData(IEnumerable set) {
            items = new PythonDictionary();
            foreach (object o in set) {
                items[o] = o;
            }
            CalculateHashCode();
        }

        void ISet.PrivFreeze() {
            CalculateHashCode();
        }

        #endregion

        #region NonOperator operations

        public object union(object s) {
            return SetHelpers.Union(this, s);
        }

        public object intersection(object s) {
            return (SetHelpers.Intersection(this, s));
        }

        public object difference(object s) {
            return SetHelpers.Difference(this, s);
        }

        public object symmetric_difference(object s) {
            return SetHelpers.SymmetricDifference(this, s);
        }

        public object copy() {
            // Python behavior: If we're a non-derived frozen set, we return ourselves. 
            // If we're a derived frozen set we make a new set of our type that contains our
            // contents.
            if (this.GetType() == typeof(FrozenSetCollection)) {
                return (this);
            }
            ISet set = SetHelpers.MakeSet(this, this);
            set.PrivFreeze();
            return (set);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o")]
        public void __init__(params object[] o) {
            // nop
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o")]
        [SpecialName]
        public int Compare(object o) {
            throw PythonOps.TypeError("cannot compare sets using cmp()");
        }

        public int __cmp__(object o) {
            throw PythonOps.TypeError("cannot compare sets using cmp()");
        }

        public PythonTuple __reduce__() {
            return SetHelpers.Reduce(items, DynamicHelpers.GetPythonTypeFromType(typeof(SetCollection)));
        }

        #endregion

        #region Operators

        public static object operator &(FrozenSetCollection x, ISet y) {
            if (y.__len__() < x.__len__()) {
                return x.intersection(y);
            }

            FrozenSetCollection setc = y as FrozenSetCollection;
            if (setc != null) {
                return setc.intersection(x);
            }

            ISet newset = SetHelpers.MakeSet(x, y.PrivIntersection(x));
            newset.PrivFreeze();
            return newset;
        }

        public static object operator |(FrozenSetCollection x, ISet y) {
            if (y.__len__() < x.__len__()) {
                return x.union(y);
            }

            FrozenSetCollection setc = y as FrozenSetCollection;
            if (setc != null) {
                return setc.union(x);
            }

            ISet newset = SetHelpers.MakeSet(x, y.PrivUnion(x));
            newset.PrivFreeze();
            return newset;
        }

        public static object operator ^(FrozenSetCollection x, ISet y) {
            if (y.__len__() < x.__len__()) {
                return x.symmetric_difference(y);
            }

            FrozenSetCollection setc = y as FrozenSetCollection;
            if (setc != null) {
                return setc.symmetric_difference(x);
            }

            ISet newset = SetHelpers.MakeSet(x, y.PrivSymmetricDifference(x));
            newset.PrivFreeze();
            return newset;
        }

        public static object operator -(FrozenSetCollection x, ISet y) {
            return x.difference(y);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)items.keys()).GetEnumerator();
        }

        #endregion

        private void CalculateHashCode() {
            // hash code needs be stable across collections (even if keys are
            // added in different order) and needs to be fairly collision free.
            hashCode = 6551;

            int[] hash_codes = new int[items.keys().__len__()];

            int i = 0;
            foreach (object o in items.keys()) {
                hash_codes[i++] = PythonOps.Hash(o);
            }

            Array.Sort(hash_codes);

            int hash1 = 6551;
            int hash2 = hash1;

            for (i = 0; i < hash_codes.Length; i += 2) {
                hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ hash_codes[i];

                if (i == hash_codes.Length - 1) {
                    break;
                }
                hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ hash_codes[i + 1];
            }

            hashCode = hash1 + (hash2 * 1566083941);
        }

        #region IRichComparable

        public static bool operator >(FrozenSetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) throw PythonOps.TypeError("can only compare to a set");

            if (s.__len__() >= self.__len__()) return false;

            foreach (object o in s) {
                if (!self.__contains__(o)) return false;
            }
            return true;
        }

        public static bool operator <(FrozenSetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) throw PythonOps.TypeError("can only compare to a set");

            if (s.__len__() <= self.__len__()) return false;

            foreach (object o in self) {
                if (!s.__contains__(o)) {
                    return false;
                }
            }
            return true;
        }

        public static bool operator >=(FrozenSetCollection self, object other) {
            return self > other || ((IValueEquality)self).ValueEquals(other);
        }

        public static bool operator <=(FrozenSetCollection self, object other) {
            return self < other || ((IValueEquality)self).ValueEquals(other);
        }

        #endregion

        #region IValueEquality Members

        int IValueEquality.GetValueHashCode() {
#if DEBUG
            // make sure we never change the hashcode we hand out in debug builds.
            // if we do then it means we somehow called PrivAdd/PrivRemove after
            // already using the hash code.
            Debug.Assert(returnedHc == hashCode || returnedHc == 0);
            returnedHc = hashCode;
#endif
            return hashCode;
        }
        
        // default conversion of protocol methods only allow's our specific type for equality,
        // sets can do __eq__ / __ne__ against any type though.  That's why we have a seperate
        // __eq__ / __ne__ here.

        bool IValueEquality.ValueEquals(object other) {
            ISet set = other as ISet;
            if (set != null) {
                if (set.__len__() != __len__()) return false;
                return set.issubset(this) && this.issubset(set);
            }
            return false;
        }

        public bool __eq__(object other) {
            return ((IValueEquality)this).ValueEquals(other);
        }

        public bool __ne__(object other) {
            return !((IValueEquality)this).ValueEquals(other);
        }

        #endregion

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return ((ICollection<object>)items.keys()).GetEnumerator();
        }

        #endregion

        #region ICodeFormattable Members

        public virtual string/*!*/ __repr__(CodeContext/*!*/ context) {
            return SetHelpers.SetToString(this, this.items.keys());
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        int ICollection.Count {
            get { return items.__len__(); }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return this; }
        }

        #endregion
    }

}
