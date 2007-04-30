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
using System.Diagnostics;
using System.Threading;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Internal;

[assembly: PythonExtensionType(typeof(object), typeof(ObjectOps))]
namespace IronPython.Runtime.Operations {
    public static class ObjectOps {
        // Types for which the pickle module has built-in support (from PEP 307 case 2)
        private static Dictionary<DynamicType, object> nativelyPickleableTypes = null;

        [PythonName("__class__")]
        public static readonly DynamicTypeSlot Class = new DynamicTypeTypeSlot();
        [PythonName("__module__")]
        public static readonly DynamicTypeSlot Module = new DynamicTypeValueSlot("__builtin__");

        [OperatorMethod, PythonName("__repr__")]
        public static string CodeRepresentation(object self) {
            return String.Format("<{0} object at {1}>",
                DynamicTypeOps.GetName(Ops.GetDynamicType(self)),
                Ops.HexId(self));
        }

        [PythonName("__getattribute__")]
        public static object GetAttribute(CodeContext context, object self, string name) {
            return Ops.ObjectGetAttribute(context, self, SymbolTable.StringToId(name));
        }

        [PythonName("__delattr__")]
        public static void DelAttrMethod(CodeContext context, object self, string name) {
            Ops.ObjectDeleteAttribute(context, self, SymbolTable.StringToId(name));
        }

        [PythonName("__setattr__")]
        public static void SetAttrMethod(CodeContext context, object self, string name, object value) {
            Ops.ObjectSetAttribute(context, self, SymbolTable.StringToId(name), value);
        }

        [OperatorMethod, PythonName("__str__")]
        public static string PythonToString(object o) {
            return CodeRepresentation(o);
        }

        [OperatorMethod, PythonName("__hash__")]
        public static int Hash(object self) {
            if (self == null) return NoneTypeOps.HashCode;
            return self.GetHashCode();
        }

        #region Pickle helpers

        // This is a dynamically-initialized property rather than a statically-initialized field
        // to avoid a bootstrapping dependency loop
        static Dictionary<DynamicType, object> NativelyPickleableTypes {
            get {
                if (nativelyPickleableTypes == null) {
                    nativelyPickleableTypes = new Dictionary<DynamicType, object>();
                    nativelyPickleableTypes.Add(TypeCache.None, null);
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(bool)), null);
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(int)), null);
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(double)), null);
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(Complex64)), null);
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(string)), null);
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(Tuple)), null);
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(List)), null);
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(PythonDictionary)), null);
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(OldInstance)), null);
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(OldClass)), null);
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(PythonFunction)), null);
                    nativelyPickleableTypes.Add(Ops.GetDynamicTypeFromType(typeof(BuiltinFunction)), null);
                }
                return nativelyPickleableTypes;
            }
        }

        [PythonName("__reduce__")]
        public static object Reduce(CodeContext context, object self) {
            return Reduce(context, self, 0);
        }

        [PythonName("__reduce_ex__")]
        public static object Reduce(CodeContext context, object self, object protocol) {
            object objectReduce = Ops.GetBoundAttr(context, Ops.GetDynamicTypeFromType(typeof(object)), Symbols.Reduce);
            object myReduce;
            if (Ops.TryGetBoundAttr(context, Ops.GetDynamicType(self), Symbols.Reduce, out myReduce)) {
                if (!Ops.IsRetBool(myReduce, objectReduce)) {
                    // A derived class overrode __reduce__ but not __reduce_ex__, so call
                    // specialized __reduce__ instead of generic __reduce_ex__.
                    // (see the "The __reduce_ex__ API" section of PEP 307)
                    return Ops.CallWithContext(context, myReduce, self);
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
        private static PythonDictionary GetInitializedSlotValues(object obj) {
            PythonDictionary initializedSlotValues = new PythonDictionary();
            IList<DynamicMixin> mro = Ops.GetDynamicType(obj).ResolutionOrder;
            object slots;
            object slotValue;
            foreach (object type in mro) {
                if (Ops.TryGetBoundAttr(type, Symbols.Slots, out slots)) {
                    List<string> slotNames = IronPython.Compiler.Generation.NewTypeMaker.SlotsToList(slots);
                    foreach (string slotName in slotNames) {
                        if (slotName == "__dict__") continue;
                        // don't reassign same-named slots from types earlier in the MRO
                        if (initializedSlotValues.ContainsKey(slotName)) continue;
                        if (Ops.TryGetBoundAttr(obj, SymbolTable.StringToId(slotName), out slotValue)) {
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
        private static Tuple ReduceProtocol0(CodeContext context, object self) {
            // CPython implements this in copy_reg._reduce_ex

            DynamicType myType = Ops.GetDynamicType(self); // PEP 307 calls this "D"
            ThrowIfNativelyPickable(myType);

            object getState;
            bool hasGetState = Ops.TryGetBoundAttr(context, self, Symbols.GetState, out getState);

            object slots;
            if (Ops.TryGetBoundAttr(context, myType, Symbols.Slots, out slots) && Ops.Length(slots) > 0 && !hasGetState) {
                // ??? does this work with superclass slots?
                throw Ops.TypeError("a class that defines __slots__ without defining __getstate__ cannot be pickled with protocols 0 or 1");
            }

            DynamicType closestNonPythonBase = FindClosestNonPythonBase(myType); // PEP 307 calls this "B"

            object func = Ops.PythonReconstructor;

            object funcArgs = Tuple.MakeTuple(
                myType,
                closestNonPythonBase,
                TypeCache.Object == closestNonPythonBase ? null : PythonCalls.Call(closestNonPythonBase, self)
            );

            object state;
            if (hasGetState) {
                state = Ops.CallWithContext(context, getState);
            } else {
                Ops.TryGetBoundAttr(context, self, Symbols.Dict, out state);
            }
            if (!Ops.IsTrue(state)) state = null;

            return Tuple.MakeTuple(func, funcArgs, state);
        }

        private static void ThrowIfNativelyPickable(DynamicType type) {
            if (NativelyPickleableTypes.ContainsKey(type)) {
                throw Ops.TypeError("can't pickle {0} objects", type.Name);
            }
        }

        /// <summary>
        /// Returns the closest base class (in terms of MRO) that isn't defined in Python code
        /// </summary>
        private static DynamicType FindClosestNonPythonBase(DynamicType type) {
            foreach (DynamicType pythonBase in type.ResolutionOrder) {
                if (pythonBase.IsSystemType) {
                    return pythonBase;
                }
            }
            throw Ops.TypeError("can't pickle {0} instance: no non-Python bases found", type.Name);
        }

        /// <summary>
        /// Implements the default __reduce_ex__ method as specified by PEP 307 case 3 (new-style instance, protocol 2)
        /// </summary>
        private static Tuple ReduceProtocol2(CodeContext context, object self) {
            DynamicType myType = Ops.GetDynamicType(self);

            object func, state, listIterator, dictIterator;
            object[] funcArgs;

            func = Ops.NewObject;

            object getNewArgsCallable;
            if (Ops.TryGetBoundAttr(context, myType, Symbols.GetNewArgs, out getNewArgsCallable)) {
                // TypeError will bubble up if __getnewargs__ isn't callable
                Tuple newArgs = Ops.CallWithContext(context, getNewArgsCallable, self) as Tuple;
                if (newArgs == null) {
                    throw Ops.TypeError("__getnewargs__ should return a tuple");
                }
                funcArgs = new object[1 + newArgs.Count];
                funcArgs[0] = myType;
                for (int i = 0; i < newArgs.Count; i++) funcArgs[i + 1] = newArgs[i];
            } else {
                funcArgs = new object[] { myType };
            }

            if(!Ops.TryInvokeOperator(context,
                    Operators.GetState,
                    self,
                    out state)) {
                object dict;
                if (!Ops.TryGetBoundAttr(context, self, Symbols.Dict, out dict)) {
                    dict = null;
                }

                PythonDictionary initializedSlotValues = GetInitializedSlotValues(self);
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
            if (self is PythonDictionary || self is IAttributesCollection) {
                dictIterator = Ops.InvokeWithContext(context, self, Symbols.IterItems, Ops.EmptyObjectArray);
            }

            return Tuple.MakeTuple(func, Tuple.MakeTuple(funcArgs), state, listIterator, dictIterator);
        }

        #endregion

    }
}
