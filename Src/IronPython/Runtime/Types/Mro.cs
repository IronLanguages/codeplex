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
        public Mro(){
        }

        public static Tuple Calculate(IPythonType startingType, Tuple bases) {
            return Calculate(startingType, bases, false);
        }

        /// <summary>
        /// </summary>
        public static Tuple Calculate(IPythonType startingType, Tuple bases, bool forceNewStyle) {
            if (bases.ContainsValue(startingType)) {
                throw Ops.TypeError("a __bases__ item causes an inheritance cycle");
            }

            List<IPythonType> mro = new List<IPythonType>();
            mro.Add(startingType);

            if (bases.Count != 0) {
                List<List<IPythonType>> mroList = new List<List<IPythonType>>();
                // build up the list - it contains the MRO of all our
                // bases as well as the bases themselves in the order in
                // which they appear.
                int oldSytleCount = 0;
                foreach (IPythonType type in bases) {
                    if (!(type is DynamicType))
                        oldSytleCount++;
                }

                foreach (IPythonType type in bases) {
                    DynamicType dt = type as DynamicType;
                    if (dt != null) {
                        mroList.Add(TupleToList(dt.MethodResolutionOrder));
                    } else if (oldSytleCount == 1 && !forceNewStyle) {
                        mroList.Add(GetOldStyleMro(type));
                    } else {
                        mroList.Add(GetNewStyleMro(type));
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
                        IPythonType head = mroList[i][0];
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

            return new Tuple(mro);
        }

        private static List<IPythonType> TupleToList(Tuple t) {
            List<IPythonType> innerList = new List<IPythonType>();
            foreach(IPythonType ipt in t) innerList.Add(ipt);
            return innerList;
        }

        private static List<IPythonType> GetOldStyleMro(IPythonType oldStyleType) {
            List<IPythonType> res = new List<IPythonType>();
            GetOldStyleMroWorker(oldStyleType, res);
            return res;
        }

        private static void GetOldStyleMroWorker(IPythonType curType, List<IPythonType> res) {
            if (!res.Contains(curType)) {
                res.Add(curType);
            
                foreach (IPythonType dt in curType.BaseClasses) {
                    GetOldStyleMroWorker(dt, res);
                }
            }
        }

        private static List<IPythonType> GetNewStyleMro(IPythonType oldStyleType) {
            List<IPythonType> res = new List<IPythonType>();
            res.Add(oldStyleType);
            foreach (IPythonType dt in oldStyleType.BaseClasses) {
                res.AddRange(TupleToList(Calculate(dt, dt.BaseClasses, true)));
            }
            return res;
        }

    }
}
