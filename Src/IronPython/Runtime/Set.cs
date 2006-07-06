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
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System.Text;


using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {

    /// <summary>
    /// Common interface shared by both Set and FrozenSet
    /// </summary>
    internal interface ISet : IEnumerable {
        int GetLength();

        // return Ops.FALSE or Ops.TRUE
        object Contains(object value);
        object IsSubset(object set);
        object IsSuperset(object set);

        // private methods used for operations between set types.
        ISet PrivDifference(IEnumerable set);
        ISet PrivIntersection(IEnumerable set);
        ISet PrivSymmetricDifference(IEnumerable set);
        ISet PrivUnion(IEnumerable set);
        void PrivAdd(object adding);
        void PrivRemove(object removing);
        void PrivFreeze();

        void SetData(Dict data);
        void SetData(IEnumerator set);
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
                setTypeStr = Ops.GetDynamicType(set).__name__.ToString();
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(setTypeStr);
            sb.Append("([");
            string comma = "";
            foreach (object o in items) {
                sb.Append(comma);
                sb.Append(Ops.Repr(o));
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
                UserType ut = Ops.GetDynamicType(setObj) as UserType;
                Debug.Assert(ut != null);

                ISet set = ut.Call() as ISet;
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
                UserType ut = Ops.GetDynamicType(setObj) as UserType;
                Debug.Assert(ut != null);
                ISet res = ut.Call(new object[] { }) as ISet;

                Debug.Assert(res != null);
                res.SetData(set.GetEnumerator());
                return res;
            }
        }

        public static ISet Intersection(ISet x, object y) {
            ISet res = SetHelpers.MakeSet(x);

            IEnumerator ie = Ops.GetEnumerator(y);
            while (ie.MoveNext()) {
                if (x.Contains(ie.Current) == Ops.TRUE)
                    res.PrivAdd(ie.Current);
            }
            res.PrivFreeze();
            return res;
        }

        public static ISet Difference(ISet x, object y) {
            ISet res = SetHelpers.MakeSet(x, x) as ISet;
            Debug.Assert(res != null);

            IEnumerator ie = Ops.GetEnumerator(y);
            while (ie.MoveNext()) {
                if (res.Contains(ie.Current) == Ops.TRUE) {
                    res.PrivRemove(ie.Current);
                }
            }
            res.PrivFreeze();
            return res;
        }

        public static ISet SymmetricDifference(ISet x, object y) {
            SetCollection otherSet = new SetCollection(Ops.GetEnumerator(y));       //make a set to deal w/ dups in the enumerator
            ISet res = SetHelpers.MakeSet(x, x) as ISet;
            Debug.Assert(res != null);

            foreach (object o in otherSet) {
                if (res.Contains(o) == Ops.TRUE) {
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
            IEnumerator ie = Ops.GetEnumerator(y);
            while (ie.MoveNext()) {
                set.PrivAdd(ie.Current);
            }
            set.PrivFreeze();
            return set;
        }

        public static object IsSubset(ISet x, object y) {
            SetCollection set = new SetCollection(Ops.GetEnumerator(y));
            foreach (object o in x) {
                if (set.Contains(o) == Ops.FALSE) {
                    return Ops.FALSE;
                }
            }
            return Ops.TRUE;
        }
    }

    /// <summary>
    /// Mutable set class
    /// </summary>
    [PythonType("set")]
    public class SetCollection : ISet, IRichComparable, IRichEquality {
        Dict items;

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
            items = new Dict();
        }

        internal SetCollection(object setData) {
            Init(setData);
        }

        internal SetCollection(IEnumerator setData) {
            items = new Dict();
            while (setData.MoveNext()) {
                Add(setData.Current);
            }
        }

        internal SetCollection(Dict setData) {
            items = setData;
        }

        #endregion

        #region ISet
        [PythonName("__len__")]
        public int GetLength() {
            return items.Count;
        }

        [PythonName("__contains__")]
        public object Contains(object o) {
            // promote sets to FrozenSet's for contains checks (so we get a hash code)
            o = SetHelpers.GetHashableSetIfSet(o);

            Ops.Hash(o);    // make sure we have a hashable item
            return Ops.Bool2Object(items.ContainsKey(o));
        }
        [PythonName("issubset")]
        public object IsSubset(object set) {
            return SetHelpers.IsSubset(this, set);
        }
        [PythonName("issuperset")]
        public object IsSuperset(object set) {
            return GreaterThanOrEqual(new SetCollection(Ops.GetEnumerator(set)));
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

        void ISet.SetData(Dict data) {
            items = data;
        }

        void ISet.SetData(IEnumerator set) {
            items = new Dict();
            while (set.MoveNext()) {
                items[set.Current] = set.Current;
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
            object[] keys = new object[items.Keys.Count];
            items.Keys.CopyTo(keys, 0);

            return Tuple.MakeTuple(new object[] { 
                TypeCache.Set, 
                Tuple.MakeTuple(new List(keys)), 
                null });
        }

        [PythonName("__reduce_ex__")]
        public Tuple ReduceEx(object proto) {
            return Reduce();
        }

        private void Init(params object[] o) {
            if (o.Length > 1) throw Ops.TypeError("set expected at most 1 arguments, got {0}", o.Length);

            items = new Dict();
            if (o.Length != 0) {
                IEnumerator setData = Ops.GetEnumerator(o[0]);
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
            IEnumerator ie = Ops.GetEnumerator(s);
            while (ie.MoveNext()) {
                object o = ie.Current;
                if (Contains(o) == Ops.FALSE) {
                    Add(o);
                }
            }
        }

        [PythonName("add")]
        public void Add(object o) {
            Ops.Hash(o);// make sure we're hashable
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
            SetCollection set = new SetCollection(Ops.GetEnumerator(s));
            foreach (object o in set) {
                if (Contains(o) == Ops.TRUE) {
                    Remove(o);
                }
            }
        }

        [PythonName("symmetric_difference_update")]
        public void SymmetricDifferenceUpdate(object s) {
            SetCollection set = new SetCollection(Ops.GetEnumerator(s));
            foreach (object o in set) {
                if (Contains(o) == Ops.TRUE) {
                    Remove(o);
                } else {
                    Add(o);
                }
            }
        }

        [PythonName("remove")]
        public void Remove(object o) {
            o = SetHelpers.GetHashableSetIfSet(o);

            Ops.Hash(o);
            if (!items.ContainsKey(o)) throw Ops.KeyError("{0}", o.ToString());

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
            throw Ops.KeyError("pop from an empty set");
        }

        [PythonName("clear")]
        public void Clear() {
            items.Clear();
        }
        #endregion

        #region Operators

        [PythonName("__iand__")]
        public SetCollection InPlaceAnd(object s) {
            ISet set = s as ISet; 
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for &=: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            IntersectionUpdate(set);
            return this;
        }

        [PythonName("__ior__")]
        public SetCollection InPlaceOr(object s) {
            ISet set = s as ISet; 
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for |=: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            Update(set);
            return this;
        }

        [PythonName("__isub__")]
        public SetCollection InPlaceSubtract(object s) {
            ISet set = s as ISet;
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for -=: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            DifferenceUpdate(set);
            return this;
        }

        [PythonName("__ixor__")]
        public SetCollection InPlaceXor(object s) {
            ISet set = s as ISet;
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for ^=: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            SymmetricDifferenceUpdate(set);
            return this;
        }

        [PythonName("__cmp__")]
        public int Compare(object o) {
            throw Ops.TypeError("cannot compare sets using cmp()");
        }

        [PythonName("__and__")]
        public object OperatorAnd(object s) {
            ISet set = s as ISet;
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for &: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            if (set.GetLength() < GetLength()) {
                return Intersection(set);
            }

            SetCollection setc = s as SetCollection;
            if (setc != null) {
                return setc.Intersection(this);
            }

            return SetHelpers.MakeSet(this, set.PrivIntersection(this));
        }

        [PythonName("__ror__")]
        public object OperatorOr(object s) {
            ISet set = s as ISet;
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for |: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            if (set.GetLength() < GetLength()) {
                return Union(set);
            }

            SetCollection setc = set as SetCollection;
            if (setc != null) {
                return setc.Union(this);
            }

            return SetHelpers.MakeSet(this, set.PrivUnion(this));
        }

        [PythonName("__rsub__")]
        public object ReverseSubtract(object s) {
            ISet set = s as ISet;
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for -: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            SetCollection setc = s as SetCollection;
            if (setc != null) {
                return setc.Difference(this);
            }

            return SetHelpers.MakeSet(this, set.PrivDifference(this));
        }

        [PythonName("__rxor__")]
        public object ReverseXor(object s) {
            ISet set = s as ISet; 
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for ^: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            if (set.GetLength() < GetLength()) {
                return SymmetricDifference(set);
            }
            SetCollection setc = s as SetCollection;
            if (setc != null) {
                return setc.SymmetricDifference(this);
            }

            return SetHelpers.MakeSet(this, set.PrivSymmetricDifference(this));
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator() {
            return items.Keys.GetEnumerator();
        }

        #endregion

        #region Object overrides
        [PythonName("__str__")]
        public override string ToString() {
            return (SetHelpers.SetToString(this, this.items.Keys));
        }

        #endregion

        #region IRichComparable
        object IRichComparable.CompareTo(object other) {
            throw Ops.TypeError("cannot compare sets using cmp()");
        }

        [PythonName("__gt__")]
        public object GreaterThan(object other) {
            ISet s = other as ISet;
            if (s == null) throw Ops.TypeError("can only compare to a set");

            if (s.GetLength() >= GetLength()) return false;

            foreach (object o in s) {
                if (Contains(o) == Ops.FALSE) return false;
            }
            return true;
        }

        [PythonName("__lt__")]
        public object LessThan(object other) {
            ISet s = other as ISet;
            if (s == null) throw Ops.TypeError("can only compare to a set");

            if (s.GetLength() <= GetLength()) return false;

            foreach (object o in this) {
                if (s.Contains(o) == Ops.FALSE) {
                    return false;
                }
            }
            return true;
        }
        [PythonName("__ge__")]
        public object GreaterThanOrEqual(object other) {
            return ((bool)GreaterThan(other)) || ((bool)RichEquals(other));
        }

        [PythonName("__le__")]
        public object LessThanOrEqual(object other) {
            return ((bool)LessThan(other)) || ((bool)RichEquals(other));
        }

        [PythonName("__ne__")]
        public bool NotEquals(object other) {
            return !((bool)RichEquals(other));
        }

        #endregion

        #region IRichEquality Members
        [PythonName("__hash__")]
        public object RichGetHashCode() {
            throw Ops.TypeError("set objects are unhashable");
        }

        [PythonName("__eq__")]
        public object RichEquals(object other) {
            ISet set = other as ISet;
            if (set != null) {
                if (set.GetLength() != GetLength()) return Ops.FALSE;
                return Ops.Bool2Object(set.IsSubset(this) == Ops.TRUE && this.IsSubset(set) == Ops.TRUE);
            }
            return Ops.FALSE;
        }

        [PythonName("__ne__")]
        public object RichNotEquals(object other) {
            return Ops.Not(RichEquals(other));
        }

        #endregion
    }


    /// <summary>
    /// Non-mutable set class
    /// </summary>
    [PythonType("frozenset")]
    public class FrozenSetCollection : ISet, IRichComparable, IRichEquality {
        Dict items;
        int hashCode;
#if DEBUG
        int returnedHc;
#endif

        #region Set Construction
        [PythonName("__new__")]
        public static FrozenSetCollection NewInst(object cls) {
            if (cls == TypeCache.FrozenSet) {
                FrozenSetCollection fs = new FrozenSetCollection();
                return fs;
            } else {
                object res = ((DynamicType)cls).ctor.Call(cls);
                FrozenSetCollection fs = res as FrozenSetCollection;
                if (fs == null) throw Ops.TypeError("{0} is not a subclass of frozenset", res);
                return fs;
            }
        }

        [PythonName("__new__")]
        public static FrozenSetCollection NewInst(object cls, object setData) {
            if (cls == TypeCache.FrozenSet) {
                FrozenSetCollection fs = setData as FrozenSetCollection;
                if (fs != null) {
                    // constructing frozen set from set, we return the original frozen set.
                    return fs;
                }

                fs = new FrozenSetCollection(Ops.GetEnumerator(setData));
                return fs;
            } else {
                object res = ((DynamicType)cls).ctor.Call(cls, setData);
                FrozenSetCollection fs = res as FrozenSetCollection;
                if (fs == null) throw Ops.TypeError("{0} is not a subclass of frozenset", res);

                return fs;
            }
        }

        internal static FrozenSetCollection Make(object setData) {
            FrozenSetCollection fs = setData as FrozenSetCollection;
            if (fs != null) {
                // constructing frozen set from set, we return the original frozen set.
                return fs;
            }

            fs = new FrozenSetCollection(Ops.GetEnumerator(setData));
            return fs;
        }


        public FrozenSetCollection() {
            items = new Dict();
            // hash code is 0 for empty set
            CalculateHashCode();
        }

        internal FrozenSetCollection(object setData)
            : this(Ops.GetEnumerator(setData)) {
            CalculateHashCode();
        }

        protected FrozenSetCollection(IEnumerator setData) {
            items = new Dict();
            while (setData.MoveNext()) {
                object o = setData.Current;
                if (!items.ContainsKey(o)) {
                    items.Add(o, o);
                }
            }
            CalculateHashCode();
        }

        internal FrozenSetCollection(Dict dict) {
            items = dict;
            CalculateHashCode();
        }
        #endregion

        #region ISet
        [PythonName("len")]
        public int GetLength() {
            return items.Count;
        }

        [PythonName("__contains__")]
        public object Contains(object o) {
            // promote sets to FrozenSet's for contains checks (so we get a hash code)
            o = SetHelpers.GetHashableSetIfSet(o);

            Ops.Hash(o);// make sure we have a hashable item
            return Ops.Bool2Object(items.ContainsKey(o));
        }

        [PythonName("issubset")]
        public object IsSubset(object set) {
            return SetHelpers.IsSubset(this, set);
        }

        [PythonName("issuperset")]
        public object IsSuperset(object set) {
            return GreaterThanOrEqual(new FrozenSetCollection(Ops.GetEnumerator(set)));
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
            Ops.Hash(adding);// make sure we're hashable
            if (!items.ContainsKey(adding)) {
                items.Add(adding, adding);
            }
        }

        void ISet.PrivRemove(object removing) {
            Ops.Hash(removing);// make sure we're hashable
            items.Remove(removing);
        }

        void ISet.SetData(Dict data) {
            items = data;
        }

        void ISet.SetData(IEnumerator set) {
            items = new Dict();
            while (set.MoveNext()) {
                items[set.Current] = set.Current;
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

        [PythonName("__init__")]
        public void Init(params object[] o) {
            // nop
        }

        [PythonName("__cmp__")]
        public int Compare(object o) {
            throw Ops.TypeError("cannot compare sets using cmp()");
        }

        [PythonName("__len__")]
        public int OperatorLength() {
            return GetLength();
        }

        [PythonName("__reduce__")]
        public Tuple Reduce() {
            object[] keys = new object[items.Keys.Count];
            items.Keys.CopyTo(keys, 0);

            return Tuple.MakeTuple(new object[] { 
                Ops.GetDynamicTypeFromType(typeof(SetCollection)), 
                Tuple.MakeTuple(new List(keys)), 
                null });
        }

        [PythonName("__reduce_ex__")]
        public Tuple ReduceEx(object param) {
            return Reduce();
        }

        #endregion

        #region Operators

        [PythonName("__and__")]
        public object OperatorAnd(object s) {
            ISet set = s as ISet;
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for &: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            if (set.GetLength() < GetLength()) {
                return Intersection(s);
            }

            FrozenSetCollection fs = s as FrozenSetCollection;
            if (fs != null) {
                return fs.Intersection(this);
            }

            ISet newset = SetHelpers.MakeSet(this, set.PrivIntersection(this));
            newset.PrivFreeze();
            return newset;
        }

        [PythonName("__ior__")]
        public object OperatorInPlaceOr(object s) {
            ISet set = s as ISet; 
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for |=: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            if (set.GetLength() < GetLength()) {
                return Union(set);
            }

            FrozenSetCollection fs = s as FrozenSetCollection;
            if (fs != null) {
                return fs.Union(this);
            }

            ISet newset = SetHelpers.MakeSet(this, set.PrivUnion(this));
            newset.PrivFreeze();
            return newset;
        }

        [PythonName("__isub__")]
        public object OperatorInPlaceSub(object s) {
            ISet set = s as ISet; 
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for -=: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            return Difference(set);
        }

        [PythonName("__ixor__")]
        public object OperatorInPlaceXor(object s) {
            ISet set = s as ISet; 
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for ^=: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            if (set.GetLength() < GetLength()) {
                return SymmetricDifference(s);
            }

            FrozenSetCollection fs = s as FrozenSetCollection;
            if (fs != null) {
                return fs.SymmetricDifference(this);
            }

            ISet newset = SetHelpers.MakeSet(this, set.PrivSymmetricDifference(this));
            newset.PrivFreeze();
            return newset;
        }

        [PythonName("__ror__")]
        public object OperatorReverseOr(object s) {
            ISet set = s as ISet; 
            if (set == null) throw Ops.TypeError("unsupported operand type(s) for |: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            if (set.GetLength() < GetLength()) {
                return Union(s);
            }
            FrozenSetCollection fs = s as FrozenSetCollection;
            if (fs != null) {
                return fs.Union(this);
            }
            ISet newset = SetHelpers.MakeSet(this, set.PrivUnion(this));
            newset.PrivFreeze();
            return newset;
        }

        [PythonName("__rsub__")]
        public object OperatorReverseSubtract(object s) {
            ISet set = s as ISet; 
            if (s == null) throw Ops.TypeError("unsupported operand type(s) for -: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));
            
            FrozenSetCollection fs = s as FrozenSetCollection;
            if (fs != null) {
                return fs.Difference(this);
            }

            ISet newset = SetHelpers.MakeSet(this, set.PrivDifference(this));
            newset.PrivFreeze();
            return newset;
        }

        [PythonName("__rxor__")]
        public object OperatorReverseXor(object s) {
            ISet set = s as ISet; 
            if (s == null) throw Ops.TypeError("unsupported operand type(s) for ^: {0} and {1}", Ops.StringRepr(Ops.GetDynamicType(s)), Ops.StringRepr(Ops.GetDynamicType(this)));

            if (set.GetLength() < GetLength()) {
                return SymmetricDifference(s);
            }
            FrozenSetCollection fs = s as FrozenSetCollection;
            if (fs != null) {
                return fs.SymmetricDifference(this);
            }
            ISet newset = SetHelpers.MakeSet(this, set.PrivSymmetricDifference(this));
            newset.PrivFreeze();
            return newset;
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

            // first get a sorted list of all our hash codes and their respective counts.
            SortedList<int, int> hcList = new SortedList<int, int>();
            foreach (object o in items.Keys) {
                int curHc = Ops.Hash(o);
                if (hcList.ContainsKey(curHc)) {
                    hcList[curHc] = hcList[curHc] + 1;
                } else {
                    hcList[curHc] = 1;
                }
            }

            // then calculate hash code based upon that & cache the result.
            foreach (int curHc in hcList.Keys) {
                for (int i = 0; i < hcList[curHc]; i++) {
                    hashCode = (hashCode << 5) ^ (hashCode >> 26) ^ (curHc);
                }
            }
        }

        #region Object Overrides
        [PythonName("__str__")]
        public override string ToString() {
            return (SetHelpers.SetToString(this, this.items.Keys));
        }
        #endregion

        #region IRichComparable
        object  IRichComparable.CompareTo(object other) {
            throw Ops.TypeError("cannot compare sets using cmp()");
        }

        [PythonName("__gt__")]
        public object GreaterThan(object other) {
            ISet s = other as ISet;
            if (s == null) throw Ops.TypeError("can only compare to a set");

            if (s.GetLength() >= GetLength()) return false;

            foreach (object o in s) {
                if (Contains(o) == Ops.FALSE) return false;
            }
            return true;
        }

        [PythonName("__lt__")]
        public object LessThan(object other) {
            ISet s = other as ISet;
            if (s == null) throw Ops.TypeError("can only compare to a set");

            if (s.GetLength() <= GetLength()) return false;

            foreach (object o in this) {
                if (s.Contains(o) == Ops.FALSE) {
                    return false;
                }
            }
            return true;
        }

        [PythonName("__ge__")]
        public object GreaterThanOrEqual(object other) {
            return ((bool)GreaterThan(other)) || ((bool)RichEquals(other));
        }

        [PythonName("__le__")]
        public object LessThanOrEqual(object other) {
            return ((bool)LessThan(other)) || ((bool)RichEquals(other));
        }

        [PythonName("__ne__")]
        public bool NotEquals(object other) {
            return !((bool)RichEquals(other));
        }

        #endregion


        #region IRichEquality Members

        [PythonName("__hash__")]
        public object RichGetHashCode() {
#if DEBUG
            // make sure we never change the hashcode we hand out in debug builds.
            // if we do then it means we somehow called PrivAdd/PrivRemove after
            // already using the hash code.
            Debug.Assert(returnedHc == hashCode || returnedHc == 0);
            returnedHc = hashCode;
#endif
            return hashCode;
        }

        [PythonName("__eq__")]
        public object RichEquals(object other) {
            ISet set = other as ISet;
            if (set != null) {
                if (set.GetLength() != GetLength()) return Ops.FALSE;
                return Ops.Bool2Object(set.IsSubset(this) == Ops.TRUE && this.IsSubset(set) == Ops.TRUE);
            }
            return Ops.FALSE;
        }

        [PythonName("__ne__")]
        public object RichNotEquals(object other) {
            return Ops.Not(RichEquals(other));
        }

        #endregion
    }

}
