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
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Scripting;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using System.Runtime.InteropServices;

namespace IronPython.Runtime.Operations {
    /// <summary>
    /// InstanceOps contains methods that get added to CLS types depending on what
    /// methods and constructors they define. These have not been added directly to
    /// PythonType since they need to be added conditionally.
    /// 
    /// Possibilities include:
    /// 
    ///     __new__, one of 3 __new__ sets can be added:
    ///         DefaultNew - This is the __new__ used for a PythonType (list, dict, object, etc...) that
    ///             has only 1 default public constructor that takes no parameters.  These types are 
    ///             mutable types, and __new__ returns a new instance of the type, and __init__ can be used
    ///             to re-initialize the types.  This __new__ allows an unlimited number of arguments to
    ///             be passed if a non-default __init__ is also defined.
    ///
    ///         NonDefaultNew - This is used when a type has more than one constructor, or only has one
    ///             that takes more than zero parameters.  This __new__ does not allow an arbitrary # of
    ///             extra arguments.
    /// 
    ///         DefaultNewCls - This is the default new used for CLS types that have only a single ctor
    ///             w/ an arbitray number of arguments.  This constructor allows setting of properties
    ///             based upon an extra set of kw-args, e.g.: System.Windows.Forms.Button(Text='abc').  It
    ///             is only used on non-Python types.
    /// 
    ///     __init__:
    ///         For types that do not define __init__ we have an __init__ function that takes an
    ///         unlimited number of arguments and does nothing.  All types share the same reference
    ///         to 1 instance of this.
    /// 
    ///     next: Defined when a type is an enumerator to expose the Python iter protocol.
    /// 
    ///     call: Added for types that implement ICallable but don't define __call__
    /// 
    ///     repr: Added for types that override ToString
    /// 
    ///     get: added for types that implement IDescriptor
    /// </summary>
    public static class InstanceOps {
        internal static BuiltinFunction New = CreateFunction("__new__", "DefaultNew", "DefaultNewKW");
        internal static BuiltinFunction NewCls = CreateFunction("__new__", "DefaultNew", "DefaultNewClsKW");
        internal static BuiltinFunction OverloadedNew = CreateFunction("__new__", "OverloadedNewBasic", "OverloadedNewKW", "OverloadedNewClsKW");
        internal static BuiltinFunction NonDefaultNewInst = CreateNonDefaultNew();
        internal static object Init = CreateInitMethod();

        static InstanceOps() {
            // We create an OpsReflectedType so that the runtime can map back from the function to typeof(PythonType). 
            ExtensionTypeAttribute.RegisterType(typeof(object), typeof(InstanceOps), DynamicHelpers.GetDynamicTypeFromType(typeof(object)));
        }

        internal static BuiltinFunction CreateNonDefaultNew() {
            return CreateFunction("__new__", "NonDefaultNew", "NonDefaultNewKW", "NonDefaultNewKWNoParams");
        }

        public static object DefaultNew(CodeContext context, DynamicType type\u00F8, params object[] args\u00F8) {
            if (type\u00F8 == null) throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.StringRepr(DynamicHelpers.GetDynamicType(type\u00F8)));

            CheckInitArgs(context, null, args\u00F8, type\u00F8);

            return type\u00F8.CreateInstance(context, RuntimeHelpers.EmptyObjectArray);
        }

        public static object DefaultNewKW(CodeContext context, DynamicType type\u00F8, [ParamDictionary] IAttributesCollection kwargs\u00F8, params object[] args\u00F8) {
            if (type\u00F8 == null) throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.StringRepr(DynamicHelpers.GetDynamicType(type\u00F8)));

            CheckInitArgs(context, kwargs\u00F8, args\u00F8, type\u00F8);

            return type\u00F8.CreateInstance(context, RuntimeHelpers.EmptyObjectArray);
        }

        public static object DefaultNewClsKW(CodeContext context, DynamicType type\u00F8, [ParamDictionary] IAttributesCollection kwargs\u00F8, params object[] args\u00F8) {
            object res = DefaultNew(context, type\u00F8, args\u00F8);

            if (kwargs\u00F8.Count > 0) {
                foreach (KeyValuePair<object, object> kvp in (IDictionary<object, object>)kwargs\u00F8) {
                    PythonOps.SetAttr(context,
                        res,
                        SymbolTable.StringToId(kvp.Key.ToString()),
                        kvp.Value);
                }
            }
            return res;
        }

        public static object OverloadedNewBasic(CodeContext context, FastCallable overloads\u00F8, DynamicType type\u00F8, params object[] args\u00F8) {
            if (type\u00F8 == null) throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.StringRepr(DynamicHelpers.GetDynamicType(type\u00F8)));
            if (args\u00F8 == null) args\u00F8 = new object[1];
            return overloads\u00F8.Call(context, args\u00F8);
        }

        public static object OverloadedNewKW(CodeContext context, BuiltinFunction overloads\u00F8, DynamicType type\u00F8, [ParamDictionary] IAttributesCollection kwargs\u00F8) {
            if (type\u00F8 == null) throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.StringRepr(DynamicHelpers.GetDynamicType(type\u00F8)));

            object[] finalArgs;
            string[] names;
            TypeHelpers.GetKeywordArgs(kwargs\u00F8, RuntimeHelpers.EmptyObjectArray, out finalArgs, out names);

            return overloads\u00F8.CallHelper(context, finalArgs, names, null);
        }

        public static object OverloadedNewClsKW(CodeContext context, BuiltinFunction overloads\u00F8, DynamicType type\u00F8, [ParamDictionary] IAttributesCollection kwargs\u00F8, params object[] args\u00F8) {
            if (type\u00F8 == null) throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.StringRepr(DynamicHelpers.GetDynamicType(type\u00F8)));
            if (args\u00F8 == null) args\u00F8 = new object[1];

            object[] finalArgs;
            string[] names;
            TypeHelpers.GetKeywordArgs(kwargs\u00F8, args\u00F8, out finalArgs, out names);

            return overloads\u00F8.CallHelper(context, finalArgs, names, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "self"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args\u00F8"), PythonName("__init__")]
        public static void DefaultInit(CodeContext context, object self, params object[] args\u00F8) {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "self"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "kwargs\u00F8"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args\u00F8"), PythonName("__init__")]
        public static void DefaultInitKW(CodeContext context, object self, [ParamDictionary] IAttributesCollection kwargs\u00F8, params object[] args\u00F8) {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context"), StaticExtensionMethod("__new__")]
        public static object NonDefaultNew(CodeContext context, DynamicType type\u00F8, params object[] args\u00F8) {
            if (type\u00F8 == null) throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.StringRepr(DynamicHelpers.GetDynamicType(type\u00F8)));
            if (args\u00F8 == null) args\u00F8 = new object[1];
            return type\u00F8.CreateInstance(context, args\u00F8);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context"), StaticExtensionMethod("__new__")]
        public static object NonDefaultNewKW(CodeContext context, DynamicType type\u00F8, [ParamDictionary] IAttributesCollection kwargs\u00F8, params object[] args\u00F8) {
            if (type\u00F8 == null) throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.StringRepr(DynamicHelpers.GetDynamicType(type\u00F8)));
            if (args\u00F8 == null) args\u00F8 = new object[1];

            string []names;
            TypeHelpers.GetKeywordArgs(kwargs\u00F8, args\u00F8, out args\u00F8, out names);
            return type\u00F8.CreateInstance(context, args\u00F8, names);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context"), StaticExtensionMethod("__new__")]
        public static object NonDefaultNewKWNoParams(CodeContext context, DynamicType type\u00F8, [ParamDictionary] IAttributesCollection kwargs\u00F8) {
            if (type\u00F8 == null) throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.StringRepr(DynamicHelpers.GetDynamicType(type\u00F8)));

            string[] names;
            object[] args;
            TypeHelpers.GetKeywordArgs(kwargs\u00F8, RuntimeHelpers.EmptyObjectArray, out args, out names);
            return type\u00F8.CreateInstance(context, args, names);
        }

        public static object NextMethod(object self) {
            IEnumerator i = (IEnumerator)self;
            if (i.MoveNext()) return i.Current;
            throw PythonOps.StopIteration();
        }

        public static string SimpleRepr(object self) {
            return String.Format("<{0} object at {1}>",
                DynamicTypeOps.GetName(self),
                PythonOps.HexId(self));
        }

        public static string FancyRepr(object self) {
            DynamicType pt = (DynamicType)DynamicHelpers.GetDynamicType(self);
            // we can't call ToString on a UserType because we'll stack overflow, so
            // only do FancyRepr for reflected types.
            if (pt.IsSystemType) {
                string toStr = self.ToString();

                // get the type name to display (CLI name or Python name)
                Type type = pt.UnderlyingSystemType;
                string typeName = type.FullName;

                // Get the underlying .ToString() representation.  Truncate multiple
                // lines, and don't display it if it's object's default representation (type name)

                // skip initial empty lines:
                int i = 0;
                while (i < toStr.Length && (toStr[i] == '\r' || toStr[i] == '\n')) i++;

                // read the first non-empty line:
                int j = i;
                while (j < toStr.Length && toStr[j] != '\r' && toStr[j] != '\n') j++;

                // skip following empty lines:
                int k = j;
                while (k < toStr.Length && (toStr[k] == '\r' || toStr[k] == '\n')) k++;

                if (j > i) {
                    string first_non_empty_line = toStr.Substring(i, j - i);
                    bool has_multiple_non_empty_lines = k < toStr.Length;

                    return String.Format("<{0} object at {1} [{2}{3}]>",
                        typeName,
                        PythonOps.HexId(self),
                        first_non_empty_line,
                        has_multiple_non_empty_lines ? "..." : String.Empty);

                } else {
                    return String.Format("<{0} object at {1}>",
                             typeName,
                             PythonOps.HexId(self));
                }
            }
            return SimpleRepr(self);
        }

        public static object ReprHelper(CodeContext context, object self) {
            return ((ICodeFormattable)self).ToCodeString(context);
        }

        public static object ToStringMethod(object self) {
            return self.ToString();
        }

        // Value equality helpers:  These are the default implementation for classes that implement
        // IValueEquality.  We promote the ReflectedType to having these helper methods which will
        // automatically test the type and return NotImplemented for mixed comparisons.  For non-mixed
        // comparisons we have a fully optimized version which returns bool.

        public static bool ValueEqualsMethod<T>(T x, [NotNull]T y) 
            where T : IValueEquality {
            return x.ValueEquals(y);
        }

        public static bool ValueNotEqualsMethod<T>(T x, [NotNull]T y)
            where T : IValueEquality {
            return x.ValueNotEquals(y);
        }

        [return: MaybeNotImplemented]
        public static object ValueEqualsMethod<T>([NotNull]T x, object y) 
            where T : IValueEquality {
            if (!(y is T)) return PythonOps.NotImplemented;

            return RuntimeHelpers.BooleanToObject(x.ValueEquals(y));
        }

        [return: MaybeNotImplemented]
        public static object ValueNotEqualsMethod<T>([NotNull]T x, object y) 
            where T : IValueEquality {
            if (!(y is T)) return PythonOps.NotImplemented;

            return RuntimeHelpers.BooleanToObject(x.ValueNotEquals(y));
        }

        [return: MaybeNotImplemented]
        public static object ValueEqualsMethod<T>(object y, [NotNull]T x)
            where T : IValueEquality {
            if (!(y is T)) return PythonOps.NotImplemented;

            return RuntimeHelpers.BooleanToObject(x.ValueEquals(y));
        }

        [return: MaybeNotImplemented]
        public static object ValueNotEqualsMethod<T>(object y, [NotNull]T x)
            where T : IValueEquality {
            if (!(y is T)) return PythonOps.NotImplemented;

            return RuntimeHelpers.BooleanToObject(x.ValueNotEquals(y));
        }

        public static object ValueHashMethod(object x) {
            return ((IValueEquality)x).GetValueHashCode();
        }

        public static int GetHashCodeMethod(object x) {
            return x.GetHashCode();
        }

        public static bool EqualsMethod(object x, object y) {
            return x.Equals(y);
        }

        public static bool NotEqualsMethod(object x, object y) {
            return !x.Equals(y);
        }

        public static object GetMethod(CodeContext context, object self, object instance, [Optional]object typeContext) {
            DynamicTypeSlot dts = self as DynamicTypeSlot;
            DynamicType dt = typeContext as DynamicType;

            Debug.Assert(dts != null);

            object res;
            if (dts.TryGetValue(context, instance, dt, out res))
                return res;

            // context is hiding __get__
            throw PythonOps.AttributeErrorForMissingAttribute(dt == null ? "?" : dt.Name, Symbols.GetDescriptor);
        }

        public static object CallMethod(CodeContext context, object self, params object[] args\u00F8) {
            return ((ICallableWithCodeContext)self).Call(context, args\u00F8);
        }

        public static object CallMethod(CodeContext context, object self, [ParamDictionary] IAttributesCollection dict\u00F8, params object[] args\u00F8) {
            object[] allArgs = new object[dict\u00F8.Count + args\u00F8.Length];
            string[] names = new string[dict\u00F8.Count];

            Array.Copy(args\u00F8, allArgs, args\u00F8.Length);
            int i = 0;
            foreach(KeyValuePair<object, object> kvp in dict\u00F8) {
                allArgs[i + args\u00F8.Length] = kvp.Value;
                names[i++] = (string)kvp.Key;
            }

            return ((IFancyCallable)self).Call(context, allArgs, names);
        }

        private static void CheckInitArgs(CodeContext context, IAttributesCollection dict, object[] args, DynamicType pt) {
            DynamicTypeSlot dts;
            object initObj;
            if (((args != null && args.Length > 0) || (dict != null && dict.Count > 0)) &&
                (pt.TryResolveSlot(context, Symbols.Init, out dts)) && dts.TryGetValue(context, null, pt, out initObj) &&
                initObj == Init) {

                throw PythonOps.TypeError("default __new__ does not take parameters");
            }
        }

        private static object CreateInitMethod() {
            MethodBase mb1 = typeof(InstanceOps).GetMethod("DefaultInit");
            MethodBase mb2 = typeof(InstanceOps).GetMethod("DefaultInitKW");
            return BuiltinFunction.MakeMethod("__init__",
                new MethodBase[] { mb1, mb2 },
                FunctionType.Method | FunctionType.AlwaysVisible | FunctionType.SkipThisCheck | FunctionType.OpsFunction).GetDescriptor();
        }

        private static BuiltinFunction CreateFunction(string name, params string[] methodNames) {
            MethodBase[] methods = new MethodBase[methodNames.Length];
            for (int i = 0; i < methods.Length; i++) {
                methods[i] = typeof(InstanceOps).GetMethod(methodNames[i]);
            }
            return BuiltinFunction.MakeMethod(name, methods, FunctionType.Function | FunctionType.AlwaysVisible | FunctionType.OpsFunction);
        }
    }
}
