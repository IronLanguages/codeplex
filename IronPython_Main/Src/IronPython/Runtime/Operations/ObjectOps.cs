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

using System; using Microsoft;
using System.Collections.Generic;
using Microsoft.Scripting;
using System.Threading;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations {

    /// <summary>
    /// Contains Python extension methods that are added to object
    /// </summary>
    public static class ObjectOps {
        /// <summary> Types for which the pickle module has built-in support (from PEP 307 case 2)  </summary>
        [MultiRuntimeAware]
        private static Dictionary<PythonType, object> _nativelyPickleableTypes;

        /// <summary>
        /// __class__, a custom slot so that it works for both objects and types.
        /// </summary>
        [SlotField]
        public static PythonTypeSlot __class__ = new PythonTypeTypeSlot();

        /// <summary>
        /// Removes an attribute from the provided member
        /// </summary>
        public static void __delattr__(CodeContext/*!*/ context, object self, string name) {
            PythonOps.ObjectDeleteAttribute(context, self, SymbolTable.StringToId(name));
        }

        /// <summary>
        /// Returns the hash code of the given object
        /// </summary>
        public static int __hash__(object self) {
            if (self == null) return NoneTypeOps.NoneHashCode;
            return self.GetHashCode();
        }

        /// <summary>
        /// Gets the specified attribute from the object without running any custom lookup behavior
        /// (__getattr__ and __getattribute__)
        /// </summary>
        public static object __getattribute__(CodeContext/*!*/ context, object self, string name) {
            return PythonOps.ObjectGetAttribute(context, self, SymbolTable.StringToId(name));
        }

        /// <summary>
        /// Initializes the object.  The base class does nothing.
        /// </summary>
        public static void __init__(object self, params object[] args) {
        }

        /// <summary>
        /// Initializes the object.  The base class does nothing.
        /// </summary>
        public static void __init__(object self, [ParamDictionary] IAttributesCollection kwargs, params object[] args) {
        }

        /// <summary>
        /// Creates a new instance of the type
        /// </summary>
        [StaticExtensionMethod]
        public static object __new__(CodeContext/*!*/ context, PythonType cls, params object[] args\u00F8) {
            if (cls == null) {
                throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.Repr(context, DynamicHelpers.GetPythonType(cls)));
            }

            InstanceOps.CheckInitArgs(context, null, args\u00F8, cls);

            return cls.CreateInstance(context);
        }

        /// <summary>
        /// Creates a new instance of the type
        /// </summary>
        [StaticExtensionMethod]
        public static object __new__(CodeContext/*!*/ context, PythonType cls, [ParamDictionary] IAttributesCollection kwargs\u00F8, params object[] args\u00F8) {
            if (cls == null) {
                throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.Repr(context, DynamicHelpers.GetPythonType(cls)));
            }

            InstanceOps.CheckInitArgs(context, kwargs\u00F8, args\u00F8, cls);

            return cls.CreateInstance(context);
        }

        /// <summary>
        /// Runs the pickle protocol
        /// </summary>
        public static object __reduce__(CodeContext/*!*/ context, object self) {
            return __reduce_ex__(context, self, 0);
        }

        /// <summary>
        /// Runs the pickle protocol
        /// </summary>
        public static object __reduce_ex__(CodeContext/*!*/ context, object self) {
            return __reduce_ex__(context, self, 0);
        }

        /// <summary>
        /// Runs the pickle protocol
        /// </summary>
        public static object __reduce_ex__(CodeContext/*!*/ context, object self, object protocol) {
            object objectReduce = PythonOps.GetBoundAttr(context, DynamicHelpers.GetPythonTypeFromType(typeof(object)), Symbols.Reduce);
            object myReduce;
            if (PythonOps.TryGetBoundAttr(context, DynamicHelpers.GetPythonType(self), Symbols.Reduce, out myReduce)) {
                if (!PythonOps.IsRetBool(myReduce, objectReduce)) {
                    // A derived class overrode __reduce__ but not __reduce_ex__, so call
                    // specialized __reduce__ instead of generic __reduce_ex__.
                    // (see the "The __reduce_ex__ API" section of PEP 307)
                    return PythonOps.CallWithContext(context, myReduce, self);
                }
            }

            if (PythonContext.GetContext(context).ConvertToInt32(protocol) < 2) {
                return ReduceProtocol0(context, self);
            } else {
                return ReduceProtocol2(context, self);
            }
        }

        /// <summary>
        /// Returns the code representation of the object.  The default implementation returns
        /// a string which consists of the type and a unique numerical identifier.
        /// </summary>
        public static string __repr__(object self) {
            return String.Format("<{0} object at {1}>",
                DynamicHelpers.GetPythonType(self).Name,
                PythonOps.HexId(self));
        }

        /// <summary>
        /// Sets an attribute on the object without running any custom object defined behavior.
        /// </summary>
        public static void __setattr__(CodeContext/*!*/ context, object self, string name, object value) {
            PythonOps.ObjectSetAttribute(context, self, SymbolTable.StringToId(name), value);
        }

        /// <summary>
        /// Returns a friendly string representation of the object. 
        /// </summary>
        public static string __str__(CodeContext/*!*/ context, object o) {
            return PythonOps.Repr(context, o);
        }

        #region Pickle helpers

        // This is a dynamically-initialized property rather than a statically-initialized field
        // to avoid a bootstrapping dependency loop
        private static Dictionary<PythonType, object> NativelyPickleableTypes {
            get {
                if (_nativelyPickleableTypes == null) {
                    Dictionary<PythonType, object> typeDict = new Dictionary<PythonType, object>();
                    typeDict.Add(TypeCache.Null, null);
                    typeDict.Add(DynamicHelpers.GetPythonTypeFromType(typeof(bool)), null);
                    typeDict.Add(DynamicHelpers.GetPythonTypeFromType(typeof(int)), null);
                    typeDict.Add(DynamicHelpers.GetPythonTypeFromType(typeof(double)), null);
                    typeDict.Add(DynamicHelpers.GetPythonTypeFromType(typeof(Complex64)), null);
                    typeDict.Add(DynamicHelpers.GetPythonTypeFromType(typeof(string)), null);
                    typeDict.Add(DynamicHelpers.GetPythonTypeFromType(typeof(PythonTuple)), null);
                    typeDict.Add(DynamicHelpers.GetPythonTypeFromType(typeof(List)), null);
                    typeDict.Add(DynamicHelpers.GetPythonTypeFromType(typeof(PythonDictionary)), null);
                    typeDict.Add(DynamicHelpers.GetPythonTypeFromType(typeof(OldInstance)), null);
                    typeDict.Add(DynamicHelpers.GetPythonTypeFromType(typeof(OldClass)), null);
                    typeDict.Add(DynamicHelpers.GetPythonTypeFromType(typeof(PythonFunction)), null);
                    typeDict.Add(DynamicHelpers.GetPythonTypeFromType(typeof(BuiltinFunction)), null);

                    // type dict needs to be ensured to be fully initialized before assigning back
                    Thread.MemoryBarrier();
                    _nativelyPickleableTypes = typeDict;
                }
                return _nativelyPickleableTypes;
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
            IList<PythonType> mro = DynamicHelpers.GetPythonType(obj).ResolutionOrder;
            object slots;
            object slotValue;
            foreach (object type in mro) {
                if (PythonOps.TryGetBoundAttr(type, Symbols.Slots, out slots)) {
                    List<string> slotNames = NewTypeMaker.SlotsToList(slots);
                    foreach (string slotName in slotNames) {
                        if (slotName == "__dict__") continue;
                        // don't reassign same-named slots from types earlier in the MRO
                        if (initializedSlotValues.__contains__(slotName)) continue;
                        if (PythonOps.TryGetBoundAttr(obj, SymbolTable.StringToId(slotName), out slotValue)) {
                            initializedSlotValues[slotName] = slotValue;
                        }
                    }
                }
            }
            if (initializedSlotValues.__len__() == 0) return null;
            return initializedSlotValues;
        }

        /// <summary>
        /// Implements the default __reduce_ex__ method as specified by PEP 307 case 2 (new-style instance, protocol 0 or 1)
        /// </summary>
        private static PythonTuple ReduceProtocol0(CodeContext/*!*/ context, object self) {
            // CPython implements this in copy_reg._reduce_ex

            PythonType myType = DynamicHelpers.GetPythonType(self); // PEP 307 calls this "D"
            ThrowIfNativelyPickable(myType);

            object getState;
            bool hasGetState = PythonOps.TryGetBoundAttr(context, self, Symbols.GetState, out getState);

            object slots;
            if (PythonOps.TryGetBoundAttr(context, myType, Symbols.Slots, out slots) && PythonOps.Length(slots) > 0 && !hasGetState) {
                // ??? does this work with superclass slots?
                throw PythonOps.TypeError("a class that defines __slots__ without defining __getstate__ cannot be pickled with protocols 0 or 1");
            }

            PythonType closestNonPythonBase = FindClosestNonPythonBase(myType); // PEP 307 calls this "B"

            object func = PythonContext.GetContext(context).PythonReconstructor;

            object funcArgs = PythonTuple.MakeTuple(
                myType,
                closestNonPythonBase,
                TypeCache.Object == closestNonPythonBase ? null : PythonCalls.Call(context, closestNonPythonBase, self)
            );

            object state;
            if (hasGetState) {
                state = PythonOps.CallWithContext(context, getState);
            } else {
                PythonOps.TryGetBoundAttr(context, self, Symbols.Dict, out state);
            }
            if (!PythonOps.IsTrue(state)) state = null;

            return PythonTuple.MakeTuple(func, funcArgs, state);
        }

        private static void ThrowIfNativelyPickable(PythonType type) {
            if (NativelyPickleableTypes.ContainsKey(type)) {
                throw PythonOps.TypeError("can't pickle {0} objects", type.Name);
            }
        }

        /// <summary>
        /// Returns the closest base class (in terms of MRO) that isn't defined in Python code
        /// </summary>
        private static PythonType FindClosestNonPythonBase(PythonType type) {
            foreach (PythonType pythonBase in type.ResolutionOrder) {
                if (pythonBase.IsSystemType) {
                    return pythonBase;
                }
            }
            throw PythonOps.TypeError("can't pickle {0} instance: no non-Python bases found", type.Name);
        }

        /// <summary>
        /// Implements the default __reduce_ex__ method as specified by PEP 307 case 3 (new-style instance, protocol 2)
        /// </summary>
        private static PythonTuple ReduceProtocol2(CodeContext/*!*/ context, object self) {
            PythonType myType = DynamicHelpers.GetPythonType(self);

            object func, state, listIterator, dictIterator;
            object[] funcArgs;

            func = PythonContext.GetContext(context).NewObject;

            object getNewArgsCallable;
            if (PythonOps.TryGetBoundAttr(context, myType, Symbols.GetNewArgs, out getNewArgsCallable)) {
                // TypeError will bubble up if __getnewargs__ isn't callable
                PythonTuple newArgs = PythonOps.CallWithContext(context, getNewArgsCallable, self) as PythonTuple;
                if (newArgs == null) {
                    throw PythonOps.TypeError("__getnewargs__ should return a tuple");
                }
                funcArgs = new object[1 + newArgs.__len__()];
                funcArgs[0] = myType;
                for (int i = 0; i < newArgs.__len__(); i++) funcArgs[i + 1] = newArgs[i];
            } else {
                funcArgs = new object[] { myType };
            }

            if (!PythonTypeOps.TryInvokeUnaryOperator(context,
                    self,
                    Symbols.GetState,
                    out state)) {
                object dict;
                if (!PythonOps.TryGetBoundAttr(context, self, Symbols.Dict, out dict)) {
                    dict = null;
                }

                PythonDictionary initializedSlotValues = GetInitializedSlotValues(self);
                if (initializedSlotValues != null && initializedSlotValues.__len__() == 0) {
                    initializedSlotValues = null;
                }

                if (dict == null && initializedSlotValues == null) state = null;
                else if (dict != null && initializedSlotValues == null) state = dict;
                else if (dict != null && initializedSlotValues != null) state = PythonTuple.MakeTuple(dict, initializedSlotValues);
                else   /*dict == null && initializedSlotValues != null*/ state = PythonTuple.MakeTuple(null, initializedSlotValues);
            }

            listIterator = null;
            if (self is List) {
                listIterator = PythonOps.GetEnumerator(self);
            }

            dictIterator = null;
            if (self is PythonDictionary || self is IAttributesCollection) {
                dictIterator = PythonOps.Invoke(context, self, Symbols.IterItems, ArrayUtils.EmptyObjects);
            }

            return PythonTuple.MakeTuple(func, PythonTuple.MakeTuple(funcArgs), state, listIterator, dictIterator);
        }

        #endregion        
    }
}
