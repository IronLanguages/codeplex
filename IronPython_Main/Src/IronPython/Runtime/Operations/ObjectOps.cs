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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;


[assembly: PythonExtensionType(typeof(object), typeof(ObjectOps))]
namespace IronPython.Runtime.Operations {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using IronPython.Runtime.Exceptions;

    public static class ObjectOps {
        // Types for which the pickle module has built-in support (from PEP 307 case 2)
        private static Dictionary<PythonType, object> nativelyPickleableTypes = null;

        [PropertyMethod, PythonName("__class__")]
        public static PythonType Get__class__(object self) {
            return DynamicHelpers.GetPythonType(self);
        }

        [PropertyMethod, PythonName("__class__")]
        public static void Set__class__(CodeContext context, object self, object value) {
            if (!new PythonTypeTypeSlot().TrySetValue(context, self, DynamicHelpers.GetPythonType(self), value)) {
                throw PythonOps.TypeError("__class__ assignment can only be performed on user defined types");
            }
        }

        [SpecialName]
        public static string __repr__(object self) {
            return String.Format("<{0} object at {1}>",
                PythonTypeOps.GetName(DynamicHelpers.GetPythonType(self)),
                PythonOps.HexId(self));
        }

        [GetAttributeAction]
        public static object __getattribute__(CodeContext context, object self, string name) {
            return PythonOps.ObjectGetAttribute(context, self, SymbolTable.StringToId(name));
        }

        public static void __delattr__(CodeContext context, object self, string name) {
            PythonOps.ObjectDeleteAttribute(context, self, SymbolTable.StringToId(name));
        }

        public static void __setattr__(CodeContext context, object self, string name, object value) {
            PythonOps.ObjectSetAttribute(context, self, SymbolTable.StringToId(name), value);
        }

        [SpecialName]
        public static string __str__(object o) {
            ICodeFormattable icf = o as ICodeFormattable;
            if (icf != null) return icf.ToCodeString(DefaultContext.Default);

            return __repr__(o);
        }

        [SpecialName]
        public static int __hash__(object self) {
            if (self == null) return NoneTypeOps.HashCode;
            return self.GetHashCode();
        }

        #region Pickle helpers

        // This is a dynamically-initialized property rather than a statically-initialized field
        // to avoid a bootstrapping dependency loop
        static Dictionary<PythonType, object> NativelyPickleableTypes {
            get {
                if (nativelyPickleableTypes == null) {
                    nativelyPickleableTypes = new Dictionary<PythonType, object>();
                    nativelyPickleableTypes.Add(TypeCache.None, null);
                    nativelyPickleableTypes.Add(DynamicHelpers.GetPythonTypeFromType(typeof(bool)), null);
                    nativelyPickleableTypes.Add(DynamicHelpers.GetPythonTypeFromType(typeof(int)), null);
                    nativelyPickleableTypes.Add(DynamicHelpers.GetPythonTypeFromType(typeof(double)), null);
                    nativelyPickleableTypes.Add(DynamicHelpers.GetPythonTypeFromType(typeof(Complex64)), null);
                    nativelyPickleableTypes.Add(DynamicHelpers.GetPythonTypeFromType(typeof(string)), null);
                    nativelyPickleableTypes.Add(DynamicHelpers.GetPythonTypeFromType(typeof(PythonTuple)), null);
                    nativelyPickleableTypes.Add(DynamicHelpers.GetPythonTypeFromType(typeof(List)), null);
                    nativelyPickleableTypes.Add(DynamicHelpers.GetPythonTypeFromType(typeof(PythonDictionary)), null);
                    nativelyPickleableTypes.Add(DynamicHelpers.GetPythonTypeFromType(typeof(OldInstance)), null);
                    nativelyPickleableTypes.Add(DynamicHelpers.GetPythonTypeFromType(typeof(OldClass)), null);
                    nativelyPickleableTypes.Add(DynamicHelpers.GetPythonTypeFromType(typeof(PythonFunction)), null);
                    nativelyPickleableTypes.Add(DynamicHelpers.GetPythonTypeFromType(typeof(BuiltinFunction)), null);
                }
                return nativelyPickleableTypes;
            }
        }

        public static object __reduce__(CodeContext context, object self) {
            return __reduce_ex__(context, self, 0);
        }

        public static object __reduce_ex__(CodeContext context, object self, object protocol) {
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
            IList<PythonType> mro = DynamicHelpers.GetPythonType(obj).ResolutionOrder;
            object slots;
            object slotValue;
            foreach (object type in mro) {
                if (PythonOps.TryGetBoundAttr(type, Symbols.Slots, out slots)) {
                    List<string> slotNames = IronPython.Compiler.Generation.NewTypeMaker.SlotsToList(slots);
                    foreach (string slotName in slotNames) {
                        if (slotName == "__dict__") continue;
                        // don't reassign same-named slots from types earlier in the MRO
                        if (initializedSlotValues.ContainsKey(slotName)) continue;
                        if (PythonOps.TryGetBoundAttr(obj, SymbolTable.StringToId(slotName), out slotValue)) {
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
        private static PythonTuple ReduceProtocol0(CodeContext context, object self) {
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

            object func = PythonOps.PythonReconstructor;

            object funcArgs = PythonTuple.MakeTuple(
                myType,
                closestNonPythonBase,
                TypeCache.Object == closestNonPythonBase ? null : PythonCalls.Call(closestNonPythonBase, self)
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
        private static PythonTuple ReduceProtocol2(CodeContext context, object self) {
            PythonType myType = DynamicHelpers.GetPythonType(self);

            object func, state, listIterator, dictIterator;
            object[] funcArgs;

            func = PythonOps.NewObject;

            object getNewArgsCallable;
            if (PythonOps.TryGetBoundAttr(context, myType, Symbols.GetNewArgs, out getNewArgsCallable)) {
                // TypeError will bubble up if __getnewargs__ isn't callable
                PythonTuple newArgs = PythonOps.CallWithContext(context, getNewArgsCallable, self) as PythonTuple;
                if (newArgs == null) {
                    throw PythonOps.TypeError("__getnewargs__ should return a tuple");
                }
                funcArgs = new object[1 + newArgs.Count];
                funcArgs[0] = myType;
                for (int i = 0; i < newArgs.Count; i++) funcArgs[i + 1] = newArgs[i];
            } else {
                funcArgs = new object[] { myType };
            }

            if(!PythonOps.TryInvokeOperator(context,
                    Operators.GetState,
                    self,
                    out state)) {
                object dict;
                if (!PythonOps.TryGetBoundAttr(context, self, Symbols.Dict, out dict)) {
                    dict = null;
                }

                PythonDictionary initializedSlotValues = GetInitializedSlotValues(self);
                if (initializedSlotValues != null && initializedSlotValues.Count == 0) {
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
                dictIterator = PythonOps.InvokeWithContext(context, self, Symbols.IterItems, ArrayUtils.EmptyObjects);
            }

            return PythonTuple.MakeTuple(func, PythonTuple.MakeTuple(funcArgs), state, listIterator, dictIterator);
        }

        #endregion

        /// <summary>
        /// Provides fast access for object.__getattribute__ by inlining the attribute lookup.
        /// </summary>
        class GetAttributeActionAttribute : ActionOnCallAttribute {
            public override StandardRule<T> GetRule<T>(CodeContext callerContext, object[] args) {
                switch (args.Length) {
                    // someObj.__getattribute__(name)
                    case 2: return MakeRule<T>(callerContext, args, ((BoundBuiltinFunction)args[0]).__self__, args[1]);
                    // object.__getattribute__(object, name)                        
                    case 3: return MakeRule<T>(callerContext, args, args[1], args[2]);
                }
                return null;
            }

            private StandardRule<T> MakeRule<T>(CodeContext context, object[] args, object self, object attribute) {
                string strAttr = attribute as string;
                if (strAttr == null) return null;
                if (self is ICustomMembers || self is IPythonObject) return null;

                PythonType selfType = DynamicHelpers.GetPythonType(self);

                return MakeGetMemberRule<T>(context, strAttr, selfType, args) ?? CreateCallRule<T>(context, args, strAttr);
            }

            private StandardRule<T> MakeGetMemberRule<T>(CodeContext context, string strAttr, PythonType selfType, object[] args) {
                PythonTypeSlot dts;
                if (selfType.TryResolveSlot(context, SymbolTable.StringToId(strAttr), out dts)) {
                    PythonGetMemberBinderHelper<T> helper = new PythonGetMemberBinderHelper<T>(context, GetMemberAction.Make(strAttr), args);                    
                    
                    if (helper.TryMakeGetMemberRule(selfType, dts, GetTargetObject(helper.InternalRule, args))) {
                        helper.InternalRule.Test = MakeTest(args, strAttr, helper.InternalRule);
                        return helper.InternalRule;
                    }
                }
                return null;
            }

            private StandardRule<T> CreateCallRule<T>(CodeContext context, object[] args, string strAttr) {
                StandardRule<T> res = new StandardRule<T>();
                res.Test = MakeTest<T>(args, strAttr, res);
                res.Target = res.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        typeof(ObjectOps).GetMethod("__getattribute__"),
                        Ast.CodeContext(),
                        Ast.ConvertHelper(GetTargetObject<T>(res, args), typeof(object)),
                        Ast.Constant(strAttr, typeof(string))
                    )
                );
                return res;
            }

            private static Expression MakeTest<T>(object[] args, string strAttr, StandardRule<T> res) {
                // test is object types + test on the parameter being looked up.  The input name is
                // either an object or a string so we call the appropriaet overload on string.
                return Ast.AndAlso(
                    PythonBinderHelper.MakeTestForTypes(res, PythonTypeOps.ObjectTypes(args), 0),
                    Ast.Call(
                        Ast.Constant(strAttr, typeof(string)),
                        typeof(string).GetMethod("Equals", new Type[] { res.Parameters[res.ParameterCount - 1].Type }),
                        res.Parameters[res.ParameterCount - 1]
                    )
                );
            }

            private Expression GetTargetObject<T>(StandardRule<T> rule, object[] args) {
                if (args.Length == 2) {
                    return Ast.ReadProperty(
                        Ast.ConvertHelper(rule.Parameters[0], typeof(BoundBuiltinFunction)),
                        typeof(BoundBuiltinFunction).GetProperty("__self__")
                    );
                } else {
                    Debug.Assert(args.Length == 3);
                    return rule.Parameters[1];
                }
            }
        }

    }
}
