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
using System.Runtime.CompilerServices;

namespace IronPython.Runtime {

    static class IdDispenser {

        sealed class Wrapper {

            WeakReference weakReference;
            int hashCode;
            long id;

            public Wrapper(Object obj, long uniqueId) {
                weakReference = new WeakReference(obj, true);
                hashCode = RuntimeHelpers.GetHashCode(obj);
                id = uniqueId;
            }

            public long Id {
                get {
                    return id;
                }
            }

            public Object Target {
                get {
                    return weakReference.Target;
                }
            }

            public override int GetHashCode() {
                return hashCode;
            }
        }

        // WrapperComparer treats Wrapper as transparent envelope
        sealed class WrapperComparer : IEqualityComparer {

            bool IEqualityComparer.Equals(Object x, Object y) {

                Wrapper wx = x as Wrapper;
                if (wx != null)
                    x = wx.Target;

                Wrapper wy = y as Wrapper;
                if (wy != null)
                    y = wy.Target;

                return x == y;
            }

            int IEqualityComparer.GetHashCode(Object obj) {

                Wrapper wobj = obj as Wrapper;
                if (wobj != null)
                    return wobj.GetHashCode();

                return RuntimeHelpers.GetHashCode(obj);
            }
        }

        // The one and only comparer instance.
        static readonly IEqualityComparer comparer = new WrapperComparer();

        static Hashtable hashtable = new Hashtable(comparer);

        // The one and only global lock instance.
        static readonly Object synchObject = new Object();

        // We do not need to worry about duplicates that to using long for unique Id.
        // It takes more than 100 years to overflow long on year 2005 hardware.
        static long currentId = 42; // Last unique Id we have given out.

        // cleanupId and cleanupGC are used for efficient scheduling of hashtable cleanups
        static long cleanupId; // currentId at the time of last cleanup
        static int cleanupGC; // GC.CollectionCount(0) at the time of last cleanup

        // Go over the hashtable and remove empty entries
        static void Cleanup() {

            int liveCount = 0;
            int emptyCount = 0;

            foreach (Wrapper w in hashtable.Keys) {
                if (w.Target != null)
                    liveCount++;
                else
                    emptyCount++;
            }

            // Rehash the table if there is a significant number of empty slots
            if (emptyCount > liveCount / 4) {
                Hashtable newtable = new Hashtable(liveCount + liveCount / 4, 1.0f, comparer);

                foreach (Wrapper w in hashtable.Keys) {
                    if (w.Target != null)
                        newtable[w] = w;
                }

                hashtable = newtable;
            }
        }

        public static object GetObject(long id) {
            lock (synchObject) {
                foreach (Wrapper w in hashtable.Keys) {
                    if (w.Target != null) {
                        if (w.Id == id) return w.Target;
                    }
                }
                return null;
            }
        }

        public static long GetId(Object o) {

            if (o == null)
                return 0;

            // hashtable is thread safe for multiple readers. No need to take the lock for lookup.
            Object w = hashtable[o];
            if (w != null)
                return ((Wrapper)w).Id;

            lock (synchObject) {

                // Check that the object has not been added in the meantime.
                w = hashtable[o];
                if (w != null)
                    return ((Wrapper)w).Id;

                long uniqueId = checked(++currentId);

                long change = uniqueId - cleanupId;

                // Cleanup the table if it is a while since we have done it last time.
                // Take the size of the table into account.
                if (change > 1234 + hashtable.Count / 2) {
                    // It makes sense to do the cleanup only if a GC has happened in the meantime.
                    // WeakReferences can become zero only during the GC.
                    int currentGC = GC.CollectionCount(0);
                    if (currentGC != cleanupGC) {
                        Cleanup();

                        cleanupId = uniqueId;
                        cleanupGC = currentGC;
                    } else {
                        cleanupId += 1234;
                    }
                }

                w = new Wrapper(o, uniqueId);
                hashtable[w] = w;

                return uniqueId;
            }
        }
    }
}
