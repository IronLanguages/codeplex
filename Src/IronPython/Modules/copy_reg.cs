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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using IronMath;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonModule("copy_reg", typeof(IronPython.Modules.PythonCopyReg))]
namespace IronPython.Modules {
    [Documentation("Provides global reduction-function registration for pickling and copying objects.")]
    public static class PythonCopyReg {

        public static Dict dispatch_table = new Dict();
        public static Dict _extension_cache = new Dict();
        public static Dict _extension_registry = new Dict();
        public static Dict _inverted_registry = new Dict();

        public static BuiltinFunction pickle_complex;
        public static BuiltinFunction _reconstructor;
        public static BuiltinFunction __newobj__;

        static PythonCopyReg() {
            pickle_complex = BuiltinFunction.MakeMethod(
                "pickle_complex",
                typeof(PythonCopyReg).GetMethod("ReduceComplex"),
                FunctionType.Function | FunctionType.PythonVisible
            );

            _reconstructor = BuiltinFunction.MakeMethod(
                "_reconstructor",
                typeof(PythonCopyReg).GetMethod("Reconstructor"),
                FunctionType.Function | FunctionType.PythonVisible
            );

            __newobj__ = BuiltinFunction.MakeMethod(
                "__newobj__",
                typeof(PythonCopyReg).GetMethod("NewObject"),
                FunctionType.Function | FunctionType.PythonVisible
            );

            dispatch_table[TypeCache.Complex64] = pickle_complex;
        }

        #region Public API

        [Documentation("pickle(type, function[, constructor]) -> None\n\n"
            + "Associate function with type, indicating that function should be used to\n"
            + "\"reduce\" objects of the given type when pickling. function should behave as\n"
            + "specified by the \"Extended __reduce__ API\" section of PEP 307.\n"
            + "\n"
            + "Reduction functions registered by calling pickle() can be retrieved later\n"
            + "through copy_reg.dispatch_table[type].\n"
            + "\n"
            + "Note that calling pickle() will overwrite any previous association for the\n"
            + "given type.\n"
            + "\n"
            + "The constructor argument is ignored, and exists only for backwards\n"
            + "compatibility."
            )]
        [PythonName("pickle")]
        public static void RegisterReduceFunction(object type, object function, [DefaultParameterValue(null)] object constructor) {
            EnsureCallable(function, "reduction functions must be callable");
            Constructor(constructor);
            dispatch_table[type] = function;
        }

        [Documentation("constructor(object) -> None\n\n"
            + "Raise TypeError if object isn't callable. This function exists only for\n"
            + "backwards compatibility; for details, see\n"
            + "http://mail.python.org/pipermail/python-dev/2006-June/066831.html."
            )]
        [PythonName("constructor")]
        public static void Constructor(object callable) {
            EnsureCallable(callable, "constructors must be callable");
        }

        /// <summary>
        /// Throw TypeError with a specified message if object isn't callable.
        /// </summary>
        private static void EnsureCallable(object @object, string message) {
            if (!Ops.IsCallable(@object)) {
                throw Ops.TypeError(message);
            }
        }

        [Documentation("pickle_complex(complex_number) -> (<type 'complex'>, (real, imag))\n\n"
            + "Reduction function for pickling complex numbers.")]
        [PythonName("pickle_complex")]
        public static Tuple ReduceComplex(object complex) {
            return Tuple.MakeTuple(
                Ops.GetDynamicTypeFromType(typeof(Complex64)),
                Tuple.MakeTuple(
                    Ops.GetAttr(DefaultContext.Default, complex, SymbolTable.RealPart),
                    Ops.GetAttr(DefaultContext.Default, complex, SymbolTable.ImaginaryPart)
                )
            );
        }

        [PythonName("clear_extension_cache")]
        public static void ClearExtensionCache() {
            _extension_cache.Clear();
        }

        [Documentation("Register an extension code.")]
        [PythonName("add_extension")]
        public static void AddExtension(object key1, object key2, object value) {
            Tuple key = Tuple.MakeTuple(key1, key2);
            int code = GetCode(value);

            bool keyExists = _extension_registry.ContainsKey(key);
            bool codeExists = _inverted_registry.ContainsKey(code);

            if (!keyExists && !codeExists) {
                _extension_registry[key] = code;
                _inverted_registry[code] = key;
            } else if (keyExists && codeExists &&
                Ops.EqualRetBool(_extension_registry[key], code) &&
                Ops.EqualRetBool(_inverted_registry[code], key)
            ) {
                // nop
            } else {
                if (keyExists) {
                    throw Ops.ValueError("key {0} is already registered with code {1}", Ops.Repr(key), Ops.Repr(_extension_registry[key]));
                } else { // codeExists
                    throw Ops.ValueError("code {0} is already in use for key {1}", Ops.Repr(code), Ops.Repr(_inverted_registry[code]));
                }
            }
        }

        [Documentation("Unregister an extension code. (only for testing)")]
        [PythonName("remove_extension")]
        public static void RemoveExtension(object key1, object key2, object value) {
            Tuple key = Tuple.MakeTuple(key1, key2);
            int code = GetCode(value);

            object existingKey;
            object existingCode;

            if (_extension_registry.TryGetValue(key, out existingCode) &&
                _inverted_registry.TryGetValue(code, out existingKey) &&
                Ops.EqualRetBool(existingCode, code) &&
                Ops.EqualRetBool(existingKey, key)
            ) {
                _extension_registry.DeleteItem(key);
                _inverted_registry.DeleteItem(code);
            } else {
                throw Ops.ValueError("key {0} is not registered with code {1}", Ops.Repr(key), Ops.Repr(code));
            }
        }

        [Documentation("__newobj__(cls, *args) -> cls.__new__(cls, *args)\n\n"
            + "Helper function for unpickling. Creates a new object of a given class.\n"
            + "See PEP 307 section \"The __newobj__ unpickling function\" for details."
            )]
        [PythonName("__newobj__")]
        public static object NewObject(object cls, params object[] args) {
            object[] newArgs = new object[1 + args.Length];
            newArgs[0] = cls;
            for (int i = 0; i < args.Length; i++) newArgs[i + 1] = args[i];
            return Ops.Invoke(cls, SymbolTable.NewInst, newArgs);
        }

        [Documentation("_reconstructor(basetype, objtype, basestate) -> object\n\n"
            + "Helper function for unpickling. Creates and initializes a new object of a given\n"
            + "class. See PEP 307 section \"Case 2: pickling new-style class instances using\n"
            + "protocols 0 or 1\" for details."
            )]
        [PythonName("_reconstructor")]
        public static object Reconstructor(object objType, object baseType, object baseState) {
            object obj = Ops.Invoke(baseType, SymbolTable.NewInst, objType, baseState);
            Ops.Invoke(baseType, SymbolTable.Init, obj, baseState);
            return obj;
        }

        #endregion

        #region Internal methods (used by other runtime code)

        /// <summary>
        /// Return the first available of:
        ///  - copy_reg.dispatch_table[type]
        ///  - type.__reduce_ex__
        ///  - type.__reduce__
        /// 
        /// Return null if none of the preceding are available.
        /// </summary>
        internal static object FindReduceFunction(DynamicType type) {
            object func;

            if (dispatch_table.TryGetValue(type, out func)) return func;
            if (Ops.TryGetAttr(type, SymbolTable.ReduceEx, out func)) return func;
            if (Ops.TryGetAttr(type, SymbolTable.Reduce, out func)) return func;

            return null;
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
        internal static Dict GetInitializedSlotValues(object obj) {
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

        #endregion

        #region Private implementation

        /// <summary>
        /// Convert object to ushort, throwing ValueError on overflow.
        /// </summary>
        private static int GetCode(object value) {
            try {
                int intValue = Converter.ConvertToInt32(value);
                if (intValue > 0) return intValue;
                // fall through and throw below
            } catch (OverflowException) {
                // throw below
            }
            throw Ops.ValueError("code out of range");
        }

        #endregion

    }
}
