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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;

using System.Diagnostics;

using IronPython.Runtime.Calls;
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
    /// Calculates a C3 MRO as described in "The Python 2.3 Method Resolution Order"
    /// plus support for old-style classes.
    /// 
    /// We build up a list of our base classes MRO's plus our base classes themselves.
    /// We go through the list in order.  Look at the 1st class in the current list, and
    /// if it's not the non-first class in any other list then remove it from all the lists
    /// and append it to the mro.  Otherwise continue to the next list.  If all the classes at
    /// the start are no-good then the MRO is bad and we throw. 
    /// 
    /// For old-style classes if the old-style class is the only one in the list of bases add
    /// it as a depth-first old-style MRO, otherwise compute a new-style mro for all the classes 
    /// and use that.
    /// </summary>
    class Mro {
        public Mro() {
        }

        public static IList<DynamicMixin> Calculate(DynamicType startingType, IList<DynamicType> bases) {
            return Calculate(startingType, new List<DynamicType>(bases), false);
        }

        internal static bool IsOldStyle(DynamicMixin dt) {
            DynamicTypeSlot dummy;
            return dt != TypeCache.Object &&
                dt.TryLookupSlot(DefaultContext.Default, Symbols.Class, out dummy) &&
                dummy.GetType() == typeof(DynamicTypeValueSlot);   // not an old-style class if the user added __class__
        }

        /// <summary>
        /// </summary>
        public static IList<DynamicMixin> Calculate(DynamicType startingType, IList<DynamicType> baseTypes, bool forceNewStyle) {
            List<DynamicMixin> bases = new List<DynamicMixin>();
            foreach (DynamicType dt in baseTypes) bases.Add(dt);

            if (bases.Contains(startingType)) {
                throw Ops.TypeError("a __bases__ item causes an inheritance cycle ({0})", startingType.Name);
            }

            List<DynamicMixin> mro = new List<DynamicMixin>();
            mro.Add(startingType);

            if (bases.Count != 0) {
                List<IList<DynamicMixin>> mroList = new List<IList<DynamicMixin>>();
                // build up the list - it contains the MRO of all our
                // bases as well as the bases themselves in the order in
                // which they appear.
                int oldSytleCount = 0;
                foreach (DynamicMixin type in bases) {
                    if (IsOldStyle(type)) oldSytleCount++;
                }

                foreach (DynamicMixin dt in bases) {
                    if (!IsOldStyle(dt)) {
                        mroList.Add(TupleToList(dt.ResolutionOrder));
                    } else if (oldSytleCount == 1 && !forceNewStyle) {
                        mroList.Add(GetOldStyleMro(dt));
                    } else {
                        mroList.Add(GetNewStyleMro(dt));
                    }
                }

                mroList.Add(TupleToList(bases));

                int lastRemove = -1;
                for (; ; ) {
                    bool removed = false, sawNonZero = false;
                    // now that we have our list, look for good heads
                    for (int i = 0; i < mroList.Count; i++) {
                        if (mroList[i].Count == 0) continue;    // we've removed everything from this list.

                        sawNonZero = true;
                        DynamicMixin head = mroList[i][0];
                        // see if we're in the tail of any other lists...
                        bool inTail = false;
                        for (int j = 0; j < mroList.Count; j++) {
                            if (mroList[j].Count != 0 && !mroList[j][0].Equals(head) && mroList[j].Contains(head)) {
                                inTail = true;
                                break;
                            }
                        }

                        if (!inTail) {
                            lastRemove = i;
                            if (mro.Contains(head)) {
                                throw Ops.TypeError("a __bases__ item causes an inheritance cycle");
                            }
                            // add it to the linearization, and remove
                            // it from our lists
                            mro.Add(head);

                            for (int j = 0; j < mroList.Count; j++) {
                                mroList[j].Remove(head);
                            }
                            removed = true;
                            break;
                        }
                    }

                    if (!sawNonZero) break;

                    if (!removed) {
                        // we've iterated through the list once w/o removing anything
                        throw Ops.TypeError("invalid order for base classes: {0} {1}",
                            mroList[0][0],
                            mroList[1][0]);
                    }
                }
            }

            return mro;
        }

        private static IList<DynamicType> TupleToList(Tuple t) {
            List<DynamicType> innerList = new List<DynamicType>();
            foreach (DynamicType ipt in t) innerList.Add(ipt);
            return innerList;
        }

        private static IList<DynamicMixin> TupleToList(IList<DynamicMixin> t) {
            return new List<DynamicMixin>(t);
        }

        private static IList<DynamicMixin> GetOldStyleMro(DynamicMixin oldStyleType) {
            List<DynamicMixin> res = new List<DynamicMixin>();
            GetOldStyleMroWorker(oldStyleType, res);
            return res;
        }

        private static void GetOldStyleMroWorker(DynamicMixin curType, List<DynamicMixin> res) {
            DynamicType dt = curType as DynamicType;
            Debug.Assert(dt != null);

            if (!res.Contains(curType)) {
                res.Add(curType);

                foreach (DynamicType baseDt in dt.BaseTypes) {
                    GetOldStyleMroWorker(baseDt, res);
                }
            }
        }

        private static IList<DynamicMixin> GetNewStyleMro(DynamicMixin oldStyleType) {
            DynamicType dt = oldStyleType as DynamicType;
            Debug.Assert(dt != null);

            List<DynamicMixin> res = new List<DynamicMixin>();
            res.Add(oldStyleType);
            foreach (DynamicType baseDt in dt.BaseTypes) {
                res.AddRange(TupleToList(Calculate(baseDt, baseDt.BaseTypes, true)));
            }
            return res;
        }        
    }
}
