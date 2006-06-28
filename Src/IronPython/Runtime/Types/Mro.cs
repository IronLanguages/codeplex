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
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Calculates the method resolution order for a Python class
    /// the rules are:
    ///      If A is a subtype of B, then A has precedence (A >B)
    ///      If C appears before D in the list of bases then C > D
    ///      If E > F in one __mro__ then E > F in all __mro__'s for our subtype
    /// 
    /// class A(object): pass
    /// class B(object): pass
    /// class C(B): pass
    /// class N(A,B,C): pass         # illegal
    ///
    /// This is because:
    ///      C.__mro__ == (C, B, object)
    ///      N.__mro__ == (N, A, B, C, object)
    /// which would conflict, but:
    ///
    /// N(B,A) is ok  (N, B, a, object)
    /// N(C, B, A) is ok (N, C, B, A, object)
    ///
    /// To calculate this we build a graph that is based upon the first two
    /// properties: the base classes, and the order in which the base classes
    /// are ordered.  At the same time of building the graph we store the
    /// order the classes occur so that classes that come earlier in the
    /// precedence list but at the same depth from the newly defined type have
    /// lower values.  In the case of two classes that have the same weight
    /// we use this distance to disambiguate, and select the class that appeared
    /// earliest in the inheritance hierachy.
    /// 
    /// Once we've built the graph we recursively walk it, and if we detect
    /// any cycles the MRO is invalid.  The graph is stored using a dictionary
    /// of dynamic types mapping to the types that are less than it.  If there
    /// are no cycles we propagate the types in the lists up the graph so that
    /// the top most type contains all the types less than it.  
    /// 
    /// Next we need to make the graph prefer depth-first orders over breadth-first
    /// For this we take everything on the right side of the tree (siblings) and propagate
    /// it up the left side of the tree if the right side classes don't contain the 
    /// left side class we're propagating into. 
    /// 
    /// Finally we sort that list w/ the largest # of types winning, and that
    /// sorted order is our MRO.
    ///
    /// Thrown into the mix is a bunch of handling of old-style classes that get treated
    /// independently from new-style classes.  
    /// </summary>
    class Mro {
        Dictionary<DynamicType, MroRatingInfo> classes;

        public Mro() {
            classes = new Dictionary<DynamicType, MroRatingInfo>();
        }

        public Tuple Calculate(DynamicType startingType, Tuple bases) {
            // start w/ the provided bases, and build the graph that merges inheritance
            // and linear order as defined by the base classes
            int count = 1;

            GenerateMroGraph(startingType, bases, ref count);

            // then propagate all the classes reachable from one node into that node
            // so we get the overall weight of each node
            foreach (DynamicType dt in classes.Keys) {
                PropagateBases(dt);
            }

            PropagateDown(startingType);

            // get the sorted keys, based upon weight, ties decided by order
            KeyValuePair<DynamicType, MroRatingInfo>[] kvps = GetSortedRatings();
            DynamicType[] mro = new DynamicType[classes.Count];

            // and then we have our mro...
            for (int i = 0; i < mro.Length; i++) {
                mro[i] = kvps[i].Key;
            }

            return new Tuple(false, mro);
        }

        /// <summary>
        /// If it's not one of your children, and your children don't have it, propagate it down.
        /// </summary>
        /// <param name="classes"></param>
        /// <param name="parent"></param>
        /// <param name="baseType"></param>
        private void PropagateDown(DynamicType parent) {
            MroRatingInfo curInfo;
            if (!classes.TryGetValue(parent, out curInfo)) return;

            Tuple bases = parent.BaseClasses;
            bool foundOldClass = false;
            for (int i = 0; i < bases.Count; i++) {
                DynamicType baseType = (DynamicType)bases[i];

                if (bases.Count > 1 && baseType is OldClass) {
                    if (!foundOldClass) {
                        for (int j = i + 1; j < bases.Count; j++) {
                            if (bases[j] is OldClass) {
                                foundOldClass = true;
                                break;
                            }
                        }

                        if (!foundOldClass) {
                            EnsureOldClassLevel(parent, bases, i);
                            continue;
                        }
                    }
                }

                PropagateDown(baseType);
            }

            // new-style class, or old-style class being treated w/ new-style mro
            for (int i = 0; i < curInfo.Count; i++) {
                PropagateDownWorker(parent, curInfo[i]);
            }
        }


        /// <summary>
        /// Ensures that an old-class graph in the middle of a new-class has
        /// all of it's children at the same level and that the level is 1 less
        /// than the parents level.  This allows us to use the Order property
        /// to order the old classes
        /// </summary>
        private void EnsureOldClassLevel(DynamicType parent, Tuple bases, int oldClassIndex) {
            MroRatingInfo tempInfo = new MroRatingInfo();

            // first get all the sibling classes to the right of us, and
            // anything they refer to we will add to our weight.
            for (int i = oldClassIndex + 1; i < bases.Count; i++) {
                if (bases[i] is OldClass) continue;

                MroRatingInfo appending;
                if (!classes.TryGetValue((DynamicType)bases[i], out appending)) continue;

                if (!tempInfo.Contains((DynamicType)bases[i])) tempInfo.Add((DynamicType)bases[i]);

                for (int j = 0; j < appending.Count; j++) {
                    if (!tempInfo.Contains(appending[j]) && !(appending[j] is OldClass)) {
                        tempInfo.Add(appending[j]);
                    }
                }
            }

            foreach (DynamicType innerBaseType in parent.BaseClasses) {
                if (!(innerBaseType is OldClass)) continue;

                PropageteOldClass(innerBaseType, tempInfo);
            }
        }

        /// <summary>
        /// Propagates list of types to old-classes that are ordered using the order
        /// property.
        /// </summary>
        private void PropageteOldClass(DynamicType curType, MroRatingInfo propagating) {
            MroRatingInfo curInfo;
            if (!classes.TryGetValue(curType, out curInfo)) return;

            Debug.Assert(curInfo.OldClassOrdered);

            foreach (DynamicType dt in propagating) {
                if (dt != curType && !curInfo.Contains(dt)) curInfo.Add(dt);
            }

            foreach (DynamicType dt in curType.BaseClasses) {
                PropageteOldClass(dt, propagating);
            }
        }

        private void PropagateDownWorker(DynamicType parent, DynamicType propagating) {
            MroRatingInfo curInfosInfo = classes[propagating];

            foreach (DynamicType baseType in parent.BaseClasses) {
                MroRatingInfo childsInfo;

                if (propagating == baseType ||
                    curInfosInfo.Contains(baseType) ||
                    !classes.TryGetValue(baseType, out childsInfo) ||
                    childsInfo.OldClassOrdered) continue;

                // propagate the current type being propagated
                if (!childsInfo.Contains(propagating)) childsInfo.Add(propagating);

                // and then propagate anything that it references down as well.
                foreach (DynamicType dt in curInfosInfo) {
                    if (dt == baseType || ChildHasType(dt, baseType) || ChildHasType(parent, dt)) continue;

                    if (!childsInfo.Contains(dt)) childsInfo.Add(dt);
                }
                PropagateDownWorker(baseType, propagating);
            }
        }

        private bool ChildHasType(DynamicType parent, DynamicType child) {
            foreach (DynamicType baseType in parent.BaseClasses) {
                if (baseType == child) return true;

                if (!classes.ContainsKey(baseType)) continue;

                MroRatingInfo childsInfo = classes[baseType];

                if (childsInfo.Contains(child)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the keys sorted by their weight & ties settled by the order in which
        /// classes appear in the class hierarchy given the calculated graph
        /// </summary>
        private KeyValuePair<DynamicType, MroRatingInfo>[] GetSortedRatings() {
            KeyValuePair<DynamicType, MroRatingInfo>[] kvps = new KeyValuePair<DynamicType, MroRatingInfo>[classes.Count];
            ((ICollection<KeyValuePair<DynamicType, MroRatingInfo>>)classes).CopyTo(kvps, 0);

            // sort the array, so the largest lists are on top
            Array.Sort<KeyValuePair<DynamicType, MroRatingInfo>>(kvps,
                delegate(KeyValuePair<DynamicType, MroRatingInfo> x, KeyValuePair<DynamicType, MroRatingInfo> y) {
                    if (x.Key == y.Key) return 0;

                    int res = y.Value.Count - x.Value.Count;
                    if (res != 0) return res;

                    // two classes are of the same precedence, 
                    // we need to look at which one is left
                    // most - luckily we already stored that info
                    return x.Value.Order - y.Value.Order;
                });
            return kvps;
        }

        private void EnsureOldClassEntries(DynamicType parent, ref int count) {
            Debug.Assert(parent is OldClass);

            MroRatingInfo parentList;

            if (!classes.TryGetValue(parent, out parentList))
                parentList = classes[parent] = new MroRatingInfo();

            parentList.OldClassOrdered = true;
            // only set order once - if we see a type multiple times,
            // the earliest one is the most important.
            if (parentList.Order == 0) parentList.Order = count++;

            foreach (DynamicType baseType in parent.BaseClasses) {
                EnsureOldClassEntries(baseType, ref count);
            }
        }
        /// <summary>
        /// Updates our classes dictionary from a type's base classes, updating for both
        /// the inheritance hierachy as well as the order the types appear as bases.
        /// </summary>
        private void GenerateMroGraph(DynamicType parent, Tuple bases, ref int count) {
            MroRatingInfo parentList, innerList;
            if (!classes.TryGetValue(parent, out parentList))
                parentList = classes[parent] = new MroRatingInfo();

            // only set order once - if we see a type multiple times,
            // the earliest one is the most important.
            if (parentList.Order == 0) parentList.Order = count++;

            DynamicType prevType = null;
            bool foundOldClass = false;
            for (int i = 0; i < bases.Count; i++) {
                DynamicType baseType = (DynamicType)bases[i];

                if (bases.Count > 1 && baseType is OldClass) {
                    if (!foundOldClass) {
                        for (int j = i + 1; j < bases.Count; j++) {
                            if (bases[j] is OldClass) {
                                foundOldClass = true;
                                break;
                            }
                        }

                        if (!foundOldClass) {
                            // no other old-classes, let order sort it out - but make sure we have
                            // entries for them.
                            EnsureOldClassEntries(baseType, ref count);
                            continue;
                        }
                    }
                }

                // full new-style MRO, look at local ordering & hierarchal ordering
                if (!parentList.Contains(baseType) && !parentList.OldClassOrdered) parentList.Add(baseType);

                if (prevType != null) {
                    innerList = classes[prevType];

                    if (!innerList.Contains(baseType) && !innerList.OldClassOrdered) innerList.Add(baseType);
                }

                prevType = baseType;

                GenerateMroGraph(baseType, baseType.BaseClasses, ref count);
            }
        }

        private void PropagateBases(DynamicType dt) {
            MroRatingInfo innerInfo, mroInfo = classes[dt];
            mroInfo.Processing = true;

            // recurse down to the bottom of the tree
            foreach (DynamicType lesser in mroInfo) {
                DynamicType lesserType = lesser;

                if (classes[lesserType].Processing) throw Ops.TypeError("invalid order for base classes: {0} and {1}", dt.__name__, lesserType.__name__);

                PropagateBases(lesserType);
            }

            // then propagate the bases up the tree as we go.
            int startingCount = mroInfo.Count;
            for (int i = 0; i < startingCount; i++) {
                DynamicType lesser = mroInfo[i];

                if (!classes.TryGetValue(lesser, out innerInfo)) continue;

                foreach (DynamicType newList in innerInfo) {
                    if (!mroInfo.Contains(newList)) mroInfo.Add(newList);
                }
            }

            mroInfo.Processing = false;
        }

        private class MroRatingInfo : List<DynamicType> {
            public int Order;
            public bool Processing;
            public bool OldClassOrdered;

            public MroRatingInfo()
                : base() {
            }
            public override string ToString() {
                return String.Format("Count: {0} Order: {1} OldClass Ordered: {2}", Count, Order, OldClassOrdered);
            }
        }
    }
}
