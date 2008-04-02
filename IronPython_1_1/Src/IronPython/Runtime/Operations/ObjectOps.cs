/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

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
        /// Return a dict that maps slot names to slot values, but only include slots that have been assigned to.
        /// Looks up slots in base types as well as the current type.
        /// 
        /// Sort-of Python equivalent (doesn't look up base slots, while the real code does):
        ///   return dict([(slot, getattr(self, slot)) for slot in type(self).__slots__ if hasattr(self, slot)])
        /// 
        /// Return null if the object has no __slots__, or empty dict if it has __slots__ but none are initialized.
        /// </summary>
        private static Dict GetInitializedSlotValues(object obj) {
            Dict initializedSlotValues = new Dict();
            Tuple mro = Ops.GetDynamicType(obj).MethodResolutionOrder;
            object slots;
            object slotValue;
            foreach (object type in mro) {
                if (Ops.TryGetAttr(type, SymbolTable.Slots, out slots)) {
                    List<string> slotNames = IronPython.Compiler.Generation.NewTypeMaker.SlotsToList(slots);
                    foreach (string slotName in slotNames) {
                        if (slotName == "__dict__") continue;
                        // don't reassign same-named slots from types earlier in the MRO
                        if (initializedSlotValues.ContainsKey(slotName)) continue;
                        if (Ops.TryGetAttr(obj, SymbolTable.StringToId(slotName), out slotValue)) {
                            initializedSlotValues[slotName] = slotValue;
                        }
                    }
                }
            }
            if (initializedSlotValues.Count == 0) return null;
            return initializedSlotValues;
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

            object func = PythonCopyReg.PythonReconstructor;

            object funcArgs = Tuple.MakeTuple(
                myType,
                closestNonPythonBase,
                TypeCache.Object == closestNonPythonBase ? null : Ops.Call(closestNonPythonBase, self)
            );

            object state;
            if (hasGetState) {
                state = Ops.Call(getState);
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

            func = PythonCopyReg.PythonNewObject;

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
                funcArgs = new object[] { myType };
            }

            if (!Ops.TryInvokeSpecialMethod(self, SymbolTable.GetState, out state)) {
                object dict;
                if (!Ops.TryGetAttr(self, SymbolTable.Dict, out dict)) {
                    dict = null;
                }

                Dict initializedSlotValues = GetInitializedSlotValues(self);
                if (initializedSlotValues != null && initializedSlotValues.Count == 0) {
                    initializedSlotValues = null;
                }

                if (dict == null && initializedSlotValues == null) state = null;
                else if (dict != null && initializedSlotValues == null) state = dict;
                else if (dict != null && initializedSlotValues != null) state = Tuple.MakeTuple(dict, initializedSlotValues);
                else   /*dict == null && initializedSlotValues != null*/ state = Tuple.MakeTuple(null, initializedSlotValues);
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
