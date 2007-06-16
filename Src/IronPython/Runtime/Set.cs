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
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System.Text;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

using Microsoft.Scripting;

namespace IronPython.Runtime {

    /// <summary>
    /// Common interface shared by both Set and FrozenSet
    /// </summary>
    public interface ISet : IEnumerable, IEnumerable<object> {
        int GetLength();

        bool Contains(object value);
        bool IsSubset(object set);
        bool IsSuperset(CodeContext context, object set);

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
                setTypeStr = DynamicTypeOps.GetName(set);
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
                return FrozenSetCollection.Make(asSet.GetEnumerator());
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
                DynamicType dt = DynamicHelpers.GetDynamicType(setObj);

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
                DynamicType dt = DynamicHelpers.GetDynamicType(setObj);

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
                if (x.Contains(ie.Current))
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
                if (res.Contains(ie.Current)) {
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
                if (res.Contains(o)) {
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
                if (!set.Contains(o)) {
                    return false;
                }
            }
            return true;
        }

        public static Tuple Reduce(PythonDictionary items, DynamicType type) {
            object[] keys = new object[items.Keys.Count];
            items.Keys.CopyTo(keys, 0);
            return Tuple.MakeTuple(type, Tuple.MakeTuple(new List(keys)), null);
        }
    }

    /// <summary>
    /// Mutable set class
    /// </summary>
    [PythonType("set")]
    public class SetCollection : ISet, IValueEquality, ICodeFormattable {
        PythonDictionary items;

        #region Set contruction

        [PythonName("__init__")]
        public void Initialize() {
            Clear();
        }

        [PythonName("__init__")]
        public void Initialize(object setData) {
            Clear();
            Update(setData);
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
                Add(setData.Current);
            }
        }

        internal SetCollection(PythonDictionary setData) {
            items = setData;
        }

        #endregion

        #region ISet
        [OperatorMethod, PythonName("__len__")]
        public int GetLength() {
            return items.Count;
        }

        [OperatorMethod, PythonName("__contains__")]
        public bool Contains(object value) {
            // promote sets to FrozenSet's for contains checks (so we get a hash code)
            value = SetHelpers.GetHashableSetIfSet(value);

            PythonOps.Hash(value);    // make sure we have a hashable item
            return items.ContainsKey(value);
        }
        [PythonName("issubset")]
        public bool IsSubset(object set) {
            return SetHelpers.IsSubset(this, set);
        }
        [PythonName("issuperset")]
        public bool IsSuperset(CodeContext context, object set) {
            return this >= new SetCollection(PythonOps.GetEnumerator(set));
        }

        ISet ISet.PrivDifference(IEnumerable set) {
            return (ISet)Difference(set);
        }
        ISet ISet.PrivIntersection(IEnumerable set) {
            return (ISet)Intersection(set);
        }
        ISet ISet.PrivSymmetricDifference(IEnumerable set) {
            return (ISet)SymmetricDifference(set);
        }
        ISet ISet.PrivUnion(IEnumerable set) {
            return (ISet)Union(set);
        }
        void ISet.PrivAdd(object adding) {
            Add(adding);
        }
        void ISet.PrivRemove(object removing) {
            Remove(removing);
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

        [PythonName("union")]
        public object Union(object s) {
            return SetHelpers.Union(this, s);
        }

        [PythonName("intersection")]
        public object Intersection(object s) {
            return SetHelpers.Intersection(this, s);
        }

        [PythonName("difference")]
        public object Difference(object s) {
            return SetHelpers.Difference(this, s);
        }

        [PythonName("symmetric_difference")]
        public object SymmetricDifference(object s) {
            return SetHelpers.SymmetricDifference(this, s);
        }

        [PythonName("copy")]
        public SetCollection Copy() {
            return new SetCollection(this.GetEnumerator());
        }

        [PythonName("__reduce__")]
        public Tuple Reduce() {
            return SetHelpers.Reduce(items, DynamicHelpers.GetDynamicTypeFromType(typeof(SetCollection)));
        }

        private void Init(params object[] o) {
            if (o.Length > 1) throw PythonOps.TypeError("set expected at most 1 arguments, got {0}", o.Length);

            items = new PythonDictionary();
            if (o.Length != 0) {
                IEnumerator setData = PythonOps.GetEnumerator(o[0]);
                while (setData.MoveNext()) {
                    Add(setData.Current);
                }
            }
        }

        #endregion

        #region Mutating Members
        /// <summary>
        /// Appends one IEnumerable to an existing set
        /// </summary>
        /// <param name="s"></param>
        [PythonName("update")]
        public void Update(object s) {
            IEnumerator ie = PythonOps.GetEnumerator(s);
            while (ie.MoveNext()) {
                object o = ie.Current;
                if (!Contains(o)) {
                    Add(o);
                }
            }
        }

        [PythonName("add")]
        public void Add(object o) {
            PythonOps.Hash(o);// make sure we're hashable
            if (!items.ContainsKey(o)) {
                items.Add(o, o);
            }
        }

        [PythonName("intersection_update")]
        public void IntersectionUpdate(object s) {
            SetCollection set = Intersection(s) as SetCollection;
            items = set.items;
        }

        [PythonName("difference_update")]
        public void DifferenceUpdate(object s) {
            SetCollection set = new SetCollection(PythonOps.GetEnumerator(s));
            foreach (object o in set) {
                if (Contains(o)) {
                    Remove(o);
                }
            }
        }

        [PythonName("symmetric_difference_update")]
        public void SymmetricDifferenceUpdate(object s) {
            SetCollection set = new SetCollection(PythonOps.GetEnumerator(s));
            foreach (object o in set) {
                if (Contains(o)) {
                    Remove(o);
                } else {
                    Add(o);
                }
            }
        }

        [PythonName("remove")]
        public void Remove(object o) {
            o = SetHelpers.GetHashableSetIfSet(o);

            PythonOps.Hash(o);
            if (!items.ContainsKey(o)) throw PythonOps.KeyError(o);

            items.Remove(o);
        }

        [PythonName("discard")]
        public void Discard(object o) {
            o = SetHelpers.GetHashableSetIfSet(o);

            items.Remove(o);
        }

        [PythonName("pop")]
        public object Pop() {
            foreach (object o in items.Keys) {
                items.Remove(o);
                return o;
            }
            throw PythonOps.KeyError("pop from an empty set");
        }

        [PythonName("clear")]
        public void Clear() {
            items.Clear();
        }
        #endregion

        #region Operators

        [OperatorMethod, PythonName("__iand__")]
        public SetCollection InPlaceAnd(object s) {
            ISet set = s as ISet;
            if (set == null) throw PythonOps.TypeError("unsupported operand type(s) for &=: {0} and {1}", PythonOps.StringRepr(DynamicTypeOps.GetName(s)), PythonOps.StringRepr(DynamicTypeOps.GetName(this)));

            IntersectionUpdate(set);
            return this;
        }

        [OperatorMethod, PythonName("__ior__")]
        public SetCollection InPlaceOr(object s) {
            ISet set = s as ISet;
            if (set == null) throw PythonOps.TypeError("unsupported operand type(s) for |=: {0} and {1}", PythonOps.StringRepr(DynamicTypeOps.GetName(s)), PythonOps.StringRepr(DynamicTypeOps.GetName(this)));

            Update(set);
            return this;
        }

        [OperatorMethod, PythonName("__isub__")]
        public SetCollection InPlaceSubtract(object s) {
            ISet set = s as ISet;
            if (set == null) throw PythonOps.TypeError("unsupported operand type(s) for -=: {0} and {1}", PythonOps.StringRepr(DynamicTypeOps.GetName(s)), PythonOps.StringRepr(DynamicTypeOps.GetName(this)));

            DifferenceUpdate(set);
            return this;
        }

        [OperatorMethod, PythonName("__ixor__")]
        public SetCollection InPlaceXor(object s) {
            ISet set = s as ISet;
            if (set == null) throw PythonOps.TypeError("unsupported operand type(s) for ^=: {0} and {1}", PythonOps.StringRepr(DynamicTypeOps.GetName(s)), PythonOps.StringRepr(DynamicTypeOps.GetName(this)));

            SymmetricDifferenceUpdate(set);
            return this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o"), OperatorMethod, PythonName("__cmp__")]
        public int Compare(object o) {
            throw PythonOps.TypeError("cannot compare sets using cmp()");
        }

        public static object BitwiseAnd(SetCollection x, ISet y) {
            if (y.GetLength() < x.GetLength()) {
                return x.Intersection(y);
            }

            SetCollection setc = y as SetCollection;
            if (setc != null) {
                return setc.Intersection(x);
            }

            return SetHelpers.MakeSet(x, y.PrivIntersection(x));
        }

        public static object operator &(SetCollection x, ISet y) {
            return BitwiseAnd(x, y);
        }


        public static object BitwiseOr(SetCollection x, ISet y) {
            if (y.GetLength() < x.GetLength()) {
                return x.Union(y);
            }

            SetCollection setc = y as SetCollection;
            if (setc != null) {
                return setc.Union(x);
            }

            return SetHelpers.MakeSet(x, y.PrivUnion(x));
        }

        public static object operator |(SetCollection x, ISet y) {
            return BitwiseOr(x, y);
        }

        public static object Xor(SetCollection x, ISet y) {
            if (y.GetLength() < x.GetLength()) {
                return x.SymmetricDifference(y);
            }

            SetCollection setc = y as SetCollection;
            if (setc != null) {
                return setc.SymmetricDifference(x);
            }

            return SetHelpers.MakeSet(x, y.PrivSymmetricDifference(x));
        }

        public static object operator ^(SetCollection x, ISet y) {
            return Xor(x, y);
        }

        public static object Subtract(SetCollection x, ISet y) {
            return x.Difference(y);
        }

        public static object operator -(SetCollection x, ISet y) {
            return x.Difference(y);
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator() {
            return items.Keys.GetEnumerator();
        }

        #endregion

        #region Object overrides
        public override string ToString() {
            return (SetHelpers.SetToString(this, this.items.Keys));
        }

        #endregion

        #region IRichComparable

        public static bool operator >(SetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) throw PythonOps.TypeError("can only compare to a set");

            if (s.GetLength() >= self.GetLength()) return false;

            foreach (object o in s) {
                if (!self.Contains(o)) return false;
            }
            return true;
        }

        public static bool operator <(SetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) throw PythonOps.TypeError("can only compare to a set");

            if (s.GetLength() <= self.GetLength()) return false;

            foreach (object o in self) {
                if (!s.Contains(o)) {
                    return false;
                }
            }
            return true;
        }

        public static bool operator >=(SetCollection self, object other) {
            return self > other || self.ValueEquals(other);
        }

        public static bool operator <=(SetCollection self, object other) {
            return self < other || self.ValueEquals(other);
        }

        #endregion

        #region Custom Rich Equality 
        
        // default conversion of protocol methods only allow's our specific type for equality,
        // sets can do __eq__ / __ne__ against any type though

        [OperatorMethod, PythonName("__eq__")]
        public virtual bool RichEquals(object other) {
            return ValueEquals(other);
        }

        [OperatorMethod, PythonName("__ne__")]
        public virtual bool RichNotEquals(object other) {
            return !ValueEquals(other);
        }
        
        #endregion

        #region IValueEquality Members

        public int GetValueHashCode() {
            throw PythonOps.TypeError("set objects are unhashable");
        }

        public bool ValueEquals(object other) {
            ISet set = other as ISet;
            if (set != null) {
                if (set.GetLength() != GetLength()) return false;
                return set.IsSubset(this) && this.IsSubset(set);
            }
            return false;
        }

        public bool ValueNotEquals(object other) {
            return !ValueEquals(other);
        }

        #endregion

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return items.Keys.GetEnumerator();
        }

        #endregion

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            return ToString();
        }

        #endregion
    }


    /// <summary>
    /// Non-mutable set class
    /// </summary>
    [PythonType("frozenset")]
    public class FrozenSetCollection : ISet, IValueEquality, ICodeFormattable {
        internal static FrozenSetCollection EMPTY = new FrozenSetCollection();

        PythonDictionary items;
        int hashCode;
#if DEBUG
        int returnedHc;
#endif

        #region Set Construction
        [StaticExtensionMethod("__new__")]
        public static FrozenSetCollection NewInst(CodeContext context, object cls) {
            if (cls == TypeCache.FrozenSet) {
                return EMPTY;
            } else {
                DynamicType dt = cls as DynamicType;
                object res = dt.CreateInstance(context);
                FrozenSetCollection fs = res as FrozenSetCollection;
                if (fs == null) throw PythonOps.TypeError("{0} is not a subclass of frozenset", res);
                return fs;
            }
        }

        [StaticExtensionMethod("__new__")]
        public static FrozenSetCollection NewInst(CodeContext context, object cls, object setData) {
            if (cls == TypeCache.FrozenSet) {
                FrozenSetCollection fs = setData as FrozenSetCollection;
                if (fs != null) {
                    // constructing frozen set from set, we return the original frozen set.
                    return fs;
                }

                fs = FrozenSetCollection.Make(setData);
                return fs;
            } else {
                object res = ((DynamicType)cls).CreateInstance(context, setData);
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

            if (items.Count == 0) {
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
                if (!items.ContainsKey(o)) {
                    items.Add(o, o);
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
        [OperatorMethod, PythonName("len")]
        public int GetLength() {
            return items.Count;
        }

        [OperatorMethod, PythonName("__contains__")]
        public bool Contains(object value) {
            // promote sets to FrozenSet's for contains checks (so we get a hash code)
            value = SetHelpers.GetHashableSetIfSet(value);

            PythonOps.Hash(value);// make sure we have a hashable item
            return items.ContainsKey(value);
        }

        [PythonName("issubset")]
        public bool IsSubset(object set) {
            return SetHelpers.IsSubset(this, set);
        }

        [PythonName("issuperset")]
        public bool IsSuperset(CodeContext context, object set) {
            return this >= FrozenSetCollection.Make(set);
        }

        ISet ISet.PrivDifference(IEnumerable set) {
            return (ISet)Difference(set);
        }
        ISet ISet.PrivIntersection(IEnumerable set) {
            return (ISet)Intersection(set);
        }
        ISet ISet.PrivSymmetricDifference(IEnumerable set) {
            return (ISet)SymmetricDifference(set);
        }
        ISet ISet.PrivUnion(IEnumerable set) {
            return (ISet)Union(set);
        }

        void ISet.PrivAdd(object adding) {
            PythonOps.Hash(adding);// make sure we're hashable
            if (!items.ContainsKey(adding)) {
                items.Add(adding, adding);
            }
        }

        void ISet.PrivRemove(object removing) {
            PythonOps.Hash(removing);// make sure we're hashable
            items.Remove(removing);
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

        [PythonName("union")]
        public object Union(object s) {
            return SetHelpers.Union(this, s);
        }

        [PythonName("intersection")]
        public object Intersection(object s) {
            return (SetHelpers.Intersection(this, s));
        }

        [PythonName("difference")]
        public object Difference(object s) {
            return SetHelpers.Difference(this, s);
        }

        [PythonName("symmetric_difference")]
        public object SymmetricDifference(object s) {
            return SetHelpers.SymmetricDifference(this, s);
        }

        [PythonName("copy")]
        public object Copy() {
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o"), PythonName("__init__")]
        public void Init(params object[] o) {
            // nop
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o"), OperatorMethod, PythonName("__cmp__")]
        public int Compare(object o) {
            throw PythonOps.TypeError("cannot compare sets using cmp()");
        }

        [OperatorMethod, PythonName("__len__")]
        public int OperatorLength() {
            return GetLength();
        }

        [PythonName("__reduce__")]
        public Tuple Reduce() {
            return SetHelpers.Reduce(items, DynamicHelpers.GetDynamicTypeFromType(typeof(SetCollection)));
        }

        #endregion

        #region Operators

        public static object BitwiseAnd(FrozenSetCollection x, ISet y) {
            if (y.GetLength() < x.GetLength()) {
                return x.Intersection(y);
            }

            FrozenSetCollection setc = y as FrozenSetCollection;
            if (setc != null) {
                return setc.Intersection(x);
            }

            ISet newset = SetHelpers.MakeSet(x, y.PrivIntersection(x));
            newset.PrivFreeze();
            return newset;
        }

        public static object operator &(FrozenSetCollection x, ISet y) {
            return BitwiseAnd(x, y);
        }


        public static object BitwiseOr(FrozenSetCollection x, ISet y) {
            if (y.GetLength() < x.GetLength()) {
                return x.Union(y);
            }

            FrozenSetCollection setc = y as FrozenSetCollection;
            if (setc != null) {
                return setc.Union(x);
            }

            ISet newset = SetHelpers.MakeSet(x, y.PrivUnion(x));
            newset.PrivFreeze();
            return newset;
        }

        public static object operator |(FrozenSetCollection x, ISet y) {
            return BitwiseOr(x, y);
        }

        public static object Xor(FrozenSetCollection x, ISet y) {
            if (y.GetLength() < x.GetLength()) {
                return x.SymmetricDifference(y);
            }

            FrozenSetCollection setc = y as FrozenSetCollection;
            if (setc != null) {
                return setc.SymmetricDifference(x);
            }

            ISet newset = SetHelpers.MakeSet(x, y.PrivSymmetricDifference(x));
            newset.PrivFreeze();
            return newset;
        }

        public static object operator ^(FrozenSetCollection x, ISet y) {
            return Xor(x, y);
        }


        public static object Subtract(FrozenSetCollection x, ISet y) {
            return x.Difference(y);
        }

        public static object operator -(FrozenSetCollection x, ISet y) {
            return x.Difference(y);
        }


        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator() {
            return items.Keys.GetEnumerator();
        }

        #endregion

        void CalculateHashCode() {
            // hash code needs be stable across collections (even if keys are
            // added in different order) and needs to be fairly collision free.
            hashCode = 6551;

            int[] hash_codes = new int[items.Keys.Count];

            int i = 0;
            foreach (object o in items.Keys) {
                hash_codes[i++] = PythonOps.Hash(o);
            }

            Array.Sort(hash_codes);

            for (int j = 0; j < hash_codes.Length; j++) {
                hashCode = (hashCode << 5) ^ (hashCode >> 26) ^ (hash_codes[j]);
            }
        }

        #region Object Overrides
        public override string ToString() {
            return (SetHelpers.SetToString(this, this.items.Keys));
        }
        #endregion

        #region IRichComparable

        public static bool operator >(FrozenSetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) throw PythonOps.TypeError("can only compare to a set");

            if (s.GetLength() >= self.GetLength()) return false;

            foreach (object o in s) {
                if (!self.Contains(o)) return false;
            }
            return true;
        }

        public static bool operator <(FrozenSetCollection self, object other) {
            ISet s = other as ISet;
            if (s == null) throw PythonOps.TypeError("can only compare to a set");

            if (s.GetLength() <= self.GetLength()) return false;

            foreach (object o in self) {
                if (!s.Contains(o)) {
                    return false;
                }
            }
            return true;
        }

        public static bool operator >=(FrozenSetCollection self, object other) {
            return self > other  || self.ValueEquals(other);
        }

        public static bool operator <=(FrozenSetCollection self, object other) {
            return self < other  || self.ValueEquals(other);
        }

        #endregion

        #region Custom Rich Equality

        #endregion

        #region IValueEquality Members

        public int GetValueHashCode() {
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
        // sets can do __eq__ / __ne__ against any type though.  Decorating w/ the PythonName
        // is enough to supress the generation in ReflectedTypeBuilder.

        [OperatorMethod, PythonName("__eq__")]
        public bool ValueEquals(object other) {
            ISet set = other as ISet;
            if (set != null) {
                if (set.GetLength() != GetLength()) return false;
                return set.IsSubset(this) && this.IsSubset(set);
            }
            return false;
        }

        [OperatorMethod, PythonName("__ne__")]
        public bool ValueNotEquals(object other) {
            return !ValueEquals(other);
        }

        #endregion

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return items.Keys.GetEnumerator();
        }

        #endregion

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            return ToString();
        }

        #endregion
    }

}
