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
using System.Diagnostics;
using System.Threading;

using IronPython.Modules;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Operations {
    static class ObjectOps {

        static ReflectedType ObjectType;
        // Types for which the pickle module has built-in support (from PEP 307 case 2)
        private static SetCollection nativelyPickleableTypes = null;

        public static ReflectedType MakeDynamicType() {
            if (ObjectType == null) {
                ReflectedType res = new OpsReflectedType("object", typeof(object), typeof(ObjectOps), typeof(object));
                if (Interlocked.CompareExchange<ReflectedType>(ref ObjectType, res, null) == null) {
                    return res;
                }
            }
            return ObjectType;
        }

        #region Pickle helpers

        // This is a dynamically-initialized property rather than a statically-initialized field
        // to avoid a bootstrapping dependency loop
        static SetCollection NativelyPickleableTypes {
            get {
                if (nativelyPickleableTypes == null) {
                    nativelyPickleableTypes = new SetCollection();
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(void)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(bool)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(int)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(double)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(IronMath.Complex64)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(string)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(Tuple)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(List)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(Dict)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(OldInstance)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(OldClass)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(PythonFunction)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(BuiltinFunction)));
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(UserType)));
                }
                return nativelyPickleableTypes;
            }
        }

        [PythonName("__reduce__")]
        public static object Reduce(ICallerContext context, object self) {
            return Reduce(context, self, 0);
        }

        [PythonName("__reduce_ex__")]
        public static object Reduce(ICallerContext context, object self, object protocol) {
            object objectReduce = Ops.GetAttr(DefaultContext.Default, Ops.GetDynamicTypeFromType(typeof(object)), SymbolTable.Reduce);
            object myReduce;
            if (Ops.TryGetAttr(DefaultContext.Default, Ops.GetDynamicType(self), SymbolTable.Reduce, out myReduce)) {
                if (!Ops.IsRetBool(myReduce, objectReduce)) {
                    // A derived class overrode __reduce__ but not __reduce_ex__, so call
                    // specialized __reduce__ instead of generic __reduce_ex__.
                    // (see the "The __reduce_ex__ API" section of PEP 307)
                    return Ops.Call(myReduce, self);
                }
            }

            if (Converter.ConvertToInt32(protocol) < 2) {
                return ReduceProtocol0(context, self);
            } else {
                return ReduceProtocol2(context, self);
            }
        }

        /// <summary>
        /// Implements the default __reduce_ex__ method as specified by PEP 307 case 2 (new-style instance, protocol 0 or 1)
        /// </summary>
        private static Tuple ReduceProtocol0(ICallerContext context, object self) {
            // CPython implements this in copy_reg._reduce_ex

            DynamicType myType = Ops.GetDynamicType(self); // PEP 307 calls this "D"
            ThrowIfNativelyPickable(myType);

            object getState;
            bool hasGetState = Ops.TryGetAttr(self, SymbolTable.GetState, out getState);

            object slots;
            if (Ops.TryGetAttr(myType, SymbolTable.Slots, out slots) && Ops.Length(slots) > 0 && !hasGetState) {
                // ??? does this work with superclass slots?
                throw Ops.TypeError("a class that defines __slots__ without defining __getstate__ cannot be pickled with protocols 0 or 1");
            }

            DynamicType closestNonPythonBase = FindClosestNonPythonBase(myType); // PEP 307 calls this "B"

            if (closestNonPythonBase != TypeCache.Object) {
                if (!NonPythonTypeIsPickleable(closestNonPythonBase)) {
                    throw Ops.TypeErrorForBadInstance("can't pickle {0} instance (non-default __reduce__ needed)", self);
                }
            }

            object func = PythonCopyReg._reconstructor;

            object funcArgs = Tuple.MakeTuple(
                myType,
                closestNonPythonBase,
                TypeCache.Object == closestNonPythonBase ? null : Ops.Call(closestNonPythonBase, self)
            );

            object state;
            if (hasGetState) {
                state = Ops.Call(getState, self);
            } else {
                Ops.TryGetAttr(self, SymbolTable.Dict, out state);
            }
            if (!Ops.IsTrue(state)) state = null;

            return Tuple.MakeTuple(func, funcArgs, state);
        }

        private static void ThrowIfNativelyPickable(DynamicType type) {
            if (Ops.TRUE == NativelyPickleableTypes.Contains(type)) {
                throw Ops.TypeError("can't pickle {0} objects", type.Name);
            }
        }

        /// <summary>
        /// Returns true if the class is picklable. From PEP 307:
        ///     "Unless B is the class 'object', instances of class B must be
        ///     picklable, either by having built-in support (as defined in the
        ///     above three bullet points), or by having a non-default
        ///     __reduce__ implementation."
        /// </summary>
        private static bool NonPythonTypeIsPickleable(DynamicType type) {
            if (Ops.TRUE == NativelyPickleableTypes.Contains(type)) return true;

            object reduceFunction = PythonCopyReg.FindReduceFunction(type);

            return reduceFunction != null &&
                reduceFunction != Ops.GetAttr(DefaultContext.Default, TypeCache.Object, SymbolTable.Reduce) &&
                reduceFunction != Ops.GetAttr(DefaultContext.Default, TypeCache.Object, SymbolTable.ReduceEx);
        }

        /// <summary>
        /// Returns the closest base class (in terms of MRO) that isn't defined in Python code
        /// </summary>
        private static DynamicType FindClosestNonPythonBase(DynamicType type) {
            foreach (object pythonBase in type.MethodResolutionOrder) {
                if (pythonBase is ReflectedType) {
                    return (DynamicType)pythonBase;
                }
            }
            throw Ops.TypeError("can't pickle {0} instance: no non-Python bases found", type.Name);
        }

        /// <summary>
        /// Implements the default __reduce_ex__ method as specified by PEP 307 case 3 (new-style instance, protocol 2)
        /// </summary>
        private static Tuple ReduceProtocol2(ICallerContext context, object self) {
            DynamicType myType = Ops.GetDynamicType(self);

            object func, state, listIterator, dictIterator;
            object[] funcArgs;

            func = PythonCopyReg.__newobj__;

            object getNewArgsCallable;
            if (Ops.TryGetAttr(myType, SymbolTable.GetNewArgs, out getNewArgsCallable)) {
                // TypeError will bubble up if __getnewargs__ isn't callable
                Tuple newArgs = Ops.Call(getNewArgsCallable, self) as Tuple;
                if (newArgs == null) {
                    throw Ops.TypeError("__getnewargs__ should return a tuple");
                }
                funcArgs = new object[1 + newArgs.Count];
                funcArgs[0] = myType;
                for (int i = 0; i < newArgs.Count; i++) funcArgs[i + 1] = newArgs[i];
            } else {
                funcArgs = new object[]{myType};
            }
            
            if (!Ops.TryInvokeSpecialMethod(self, SymbolTable.GetState, out state)) {
                object dict;
                if (Ops.TryGetAttr(self, SymbolTable.Dict, out dict)) {
                    if (!(dict is IMapping && ((IMapping)dict).GetLength() > 0)) {
                        dict = null;
                    }
                } else {
                    dict = null;
                }

                Dict initializedSlotValues = PythonCopyReg.GetInitializedSlotValues(self);
                if (initializedSlotValues != null && initializedSlotValues.Count == 0) {
                    initializedSlotValues = null;
                }

                if      (dict == null && initializedSlotValues == null) state = null;
                else if (dict != null && initializedSlotValues == null) state = dict;
                else if (dict != null && initializedSlotValues != null) state = Tuple.MakeTuple(dict, initializedSlotValues);
                else   /*dict == null && initializedSlotValues != null*/state = Tuple.MakeTuple(null, initializedSlotValues);
            }

            listIterator = null;
            if (self is List) {
                listIterator = Ops.GetEnumerator(self);
            }

            dictIterator = null;
            if (self is Dict) {
                dictIterator = Ops.Invoke(self, SymbolTable.IterItems, Ops.EMPTY);
            }

            return Tuple.MakeTuple(func, Tuple.MakeTuple(funcArgs), state, listIterator, dictIterator);
        }

        #endregion

    }
}
