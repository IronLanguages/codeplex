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
using System.Collections;
using System.Reflection;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

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
        internal static object Init = CreateInitMethod();

        static InstanceOps() {
            // We create an OpsReflectedType so that the runtime can map back from the function to typeof(PythonType). 
            OpsReflectedType.OpsTypeToType[typeof(InstanceOps)] = new OpsReflectedType("InstanceOps", typeof(object), typeof(InstanceOps), null);
        }

        internal static BuiltinFunction CreateNonDefaultNew() {
            return CreateFunction("__new__", "NonDefaultNew", "NonDefaultNewKW", "NonDefaultNewKWNoParams");
        }

        [PythonName("__new__")]
        public static object DefaultNew(ICallerContext context, DynamicType type, params object[] args) {
            if (type == null) throw Ops.TypeError("__new__ expected type object, got {0}", Ops.StringRepr(Ops.GetDynamicType(type)));

            CheckInitArgs(context, null, args, type);

            return type.AllocateObject();
        }

        [PythonName("__new__")]
        public static object DefaultNewKW(ICallerContext context, DynamicType type, [ParamDict] Dict kwArgs, params object[] args) {
            if (type == null) throw Ops.TypeError("__new__ expected type object, got {0}", Ops.StringRepr(Ops.GetDynamicType(type)));

            CheckInitArgs(context, kwArgs, args, type);

            return type.AllocateObject();
        }

        [PythonName("__new__")]
        public static object DefaultNewClsKW(ICallerContext context, DynamicType type, [ParamDict] Dict kwDict, params object[] args) {
            object res = DefaultNew(context, type, args);

            if (kwDict.Count > 0) {
                foreach (KeyValuePair<object, object> kvp in (IDictionary<object, object>)kwDict) {
                    Ops.SetAttr(context,
                        res,
                        SymbolTable.StringToId(kvp.Key.ToString()),
                        kvp.Value);
                }
            }
            return res;
        }

        [PythonName("__new__")]
        public static object OverloadedNewBasic(ICallerContext context, BuiltinFunction overloads, DynamicType type, params object[] args) {
            if (type == null) throw Ops.TypeError("__new__ expected type object, got {0}", Ops.StringRepr(Ops.GetDynamicType(type)));
            if (args == null) args = new object[1];
            return overloads.Call(context, args);
        }

        [PythonName("__new__")]
        public static object OverloadedNewKW(ICallerContext context, BuiltinFunction overloads, DynamicType type, [ParamDict] Dict kwArgs) {
            if (type == null) throw Ops.TypeError("__new__ expected type object, got {0}", Ops.StringRepr(Ops.GetDynamicType(type)));

            object[] finalArgs;
            string[] names;
            DynamicType.GetKeywordArgs(kwArgs, Ops.EMPTY, out finalArgs, out names);


            return overloads.CallHelper(context, finalArgs, names, null);
        }

        [PythonName("__new__")]
        public static object OverloadedNewClsKW(ICallerContext context, BuiltinFunction overloads, DynamicType type, [ParamDict] Dict kwDict, params object[] args) {
            if (type == null) throw Ops.TypeError("__new__ expected type object, got {0}", Ops.StringRepr(Ops.GetDynamicType(type)));
            if (args == null) args = new object[1];

            object[] finalArgs;
            string[] names;
            DynamicType.GetKeywordArgs(kwDict, args, out finalArgs, out names);

            return overloads.CallHelper(context, finalArgs, names, null);
        }

        [PythonName("__init__")]
        public static void DefaultInit(ICallerContext context, object self, params object[] args) {
        }

        [PythonName("__init__")]
        public static void DefaultInitKW(ICallerContext context, object self, [ParamDict] Dict kwDict, params object[] args) {
        }

        [PythonName("__new__")]
        public static object NonDefaultNew(ICallerContext context, DynamicType type, params object[] args) {
            if (type == null) throw Ops.TypeError("__new__ expected type object, got {0}", Ops.StringRepr(Ops.GetDynamicType(type)));
            if (args == null) args = new object[1];
            return type.AllocateObject(args);
        }

        [PythonName("__new__")]
        public static object NonDefaultNewKW(ICallerContext context, DynamicType type, [ParamDict] Dict dict, params object[] args) {
            if (type == null) throw Ops.TypeError("__new__ expected type object, got {0}", Ops.StringRepr(Ops.GetDynamicType(type)));
            if (args == null) args = new object[1];
            return type.AllocateObject(dict, args);
        }

        [PythonName("__new__")]
        public static object NonDefaultNewKWNoParams(ICallerContext context, DynamicType type, [ParamDict] Dict dict) {
            if (type == null) throw Ops.TypeError("__new__ expected type object, got {0}", Ops.StringRepr(Ops.GetDynamicType(type)));

            return type.AllocateObject(dict, Ops.EMPTY);
        }

        public static object NextMethod(object self) {
            IEnumerator i = (IEnumerator)self;
            if (i.MoveNext()) return i.Current;
            throw Ops.StopIteration();
        }

        [PythonName("__repr__")]
        public static string SimpleRepr(object self) {
            return String.Format("<{0} object at {1}>",
                Ops.GetDynamicType(self).Name,
                Ops.HexId(self));
        }

        [PythonName("__repr__")]
        public static string FancyRepr(object self) {
            DynamicType pt = (DynamicType)Ops.GetDynamicType(self);
            // we can't call ToString on a UserType because we'll stack overflow, so
            // only do FancyRepr for reflected types.
            if (pt is ReflectedType) {
                string toStr = self.ToString();

                // get the type name to display (CLI name or Python name)
                Type type = pt.type;
                string typeName = type.FullName;

                // Get the underlying .ToString() representation.  Truncate multiple
                // lines, and don't display it if it's object's default representation (type name)

                string[] strForm = toStr.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                if (strForm.Length > 1) toStr = strForm[0] + "...";
                else if (strForm.Length != 0) toStr = strForm[0];
                else
                    return String.Format("<{0} object at {1}>",
                        typeName,
                        Ops.HexId(self));

                return String.Format("<{0} object at {1} [{2}]>",
                    typeName,
                    Ops.HexId(self),
                    toStr);
            }
            return SimpleRepr(self);
        }

        [PythonName("__repr__")]
        public static object ReprHelper(object self) {
            return ((ICodeFormattable)self).ToCodeString();
        }

        public static object ToStringMethod(object self) {
            return self.ToString();
        }

        public static object GetMethod(object self, object instance, object context) {
            return ((IDescriptor)self).GetAttribute(instance, context);
        }

        public static object CallMethod(object self, params object[] args) {
            return ((ICallable)self).Call(args);
        }

        private static void CheckInitArgs(ICallerContext context, Dict dict, object[] args, DynamicType pt) {
            object initMethod;
            if (((args != null && args.Length > 0) || (dict != null && dict.Count > 0)) &&
                (pt.TryGetSlot(context, SymbolTable.Init, out initMethod) ||
                pt.TryLookupSlotInBases(context, SymbolTable.Init, out initMethod)) &&
                initMethod == Init) {

                throw Ops.TypeError("default __new__ does not take parameters");
            }
        }

        private static object CreateInitMethod() {
            MethodBase mb1 = typeof(InstanceOps).GetMethod("DefaultInit");
            MethodBase mb2 = typeof(InstanceOps).GetMethod("DefaultInitKW");
            return BuiltinFunction.MakeMethod("__init__",
                new MethodBase[] { mb1, mb2 },
                FunctionType.Method | FunctionType.PythonVisible | FunctionType.SkipThisCheck | FunctionType.OpsFunction).GetDescriptor();
        }

        private static BuiltinFunction CreateFunction(string name, params string[] methodNames) {
            MethodBase[] methods = new MethodBase[methodNames.Length];
            for (int i = 0; i < methods.Length; i++) {
                methods[i] = typeof(InstanceOps).GetMethod(methodNames[i]);
            }
            return BuiltinFunction.MakeMethod(name, methods, FunctionType.Function | FunctionType.PythonVisible);
        }


    }
}
