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
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;

using IronPython.Runtime.Operations;
using IronPython.Compiler;
using IronPython.Modules;
using IronPython.Runtime.Calls;
using IronMath;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// OpsReflectedType is the Python representation of primitive CLI valuetypes,
    /// and some Python valuetypes. This allows the engine to present a uniform
    /// view of types, including features not supported by the CLI like
    /// inheriting from a valuetype.
    /// </summary>
    [PythonType(typeof(DynamicType))]
    public class OpsReflectedType : ReflectedType {
        // Maps the OpsType back to the corresponding ReflectedType.
        internal static Dictionary<Type, ReflectedType> OpsTypeToType = new Dictionary<Type, ReflectedType>(10);

        // This is the type to extend for inheriting from the current ReflectedType.
        protected Type extensibleType;

        // This is the type that implements various functionality on behalf of the
        // current ReflectedType
        protected Type opsType;
        private CallTarget1 optCtor;
        private CallTarget2 optCtor2;

        public OpsReflectedType(string name, Type baseType, Type opsType, Type extensibleType, CallTarget1 optimizedCtor)
            : this(name, baseType, opsType, extensibleType) {
            optCtor = optimizedCtor;
        }

        public OpsReflectedType(string name, Type baseType, Type opsType, Type extensibleType, CallTarget2 optimizedCtor)
            : this(name, baseType, opsType, extensibleType) {
            optCtor2 = optimizedCtor;
        }

        public OpsReflectedType(string name, Type baseType, Type opsType, Type extensibleType)
            : base(baseType) {
            this.extensibleType = extensibleType;
            this.__name__ = name;
            this.opsType = opsType;

            lock (OpsTypeToType) {
                // All the array types (byte[], int[]) etc map to ArrayOps. However, we map it only to System.Array.
                Debug.Assert(!OpsTypeToType.ContainsKey(opsType) ||
                             (opsType == typeof(ArrayOps) && OpsTypeToType[opsType] == TypeCache.Array));
                if (!OpsTypeToType.ContainsKey(opsType))
                    OpsTypeToType[opsType] = this;
            }
        }

        public override Type GetTypeToExtend() {
            return extensibleType;
        }

        public override bool IsPythonType {
            get {
                return true;
            }
        }

        public override object Call(ICallerContext context, object[] args) {
            if (optCtor != null && args.Length == 1) return optCtor(args[0]);
            if (optCtor2 != null && args.Length == 1) return optCtor2(context, args[0]);

            PerfTrack.NoteEvent(PerfTrack.Categories.Methods, "TypeInvoke " + this.__name__ + args.Length);
            return base.Call(context, args);
        }

        /// <summary>
        /// Find the methods implemented by opsType, and expose them from 
        /// the current ReflectedType
        /// </summary>
        protected override void AddOps() {
            foreach (MethodInfo mi in opsType.GetMethods()) {
                AddReflectedUnboundMethod(mi);
            }
        }

        private void AddReflectedUnboundMethod(MethodInfo mi) {
            if (!mi.IsStatic) return;

            if (Options.Python25 == false) {
                object[] attribute = mi.GetCustomAttributes(typeof(PythonVersionAttribute), false);
                if (attribute.Length > 0) {
                    PythonVersionAttribute attr = attribute[0] as PythonVersionAttribute;
                    if (attr != null && attr.version == ReflectionUtil.pythonVersion25) return;
                }
            }

            string name;
            NameType nt = NameConverter.TryGetName(this, mi, out name);
            if (nt == NameType.None) return;

            FunctionType funcType = FunctionType.Method;
            if (name == "__new__" || mi.IsDefined(typeof(StaticOpsMethodAttribute), false)) funcType = FunctionType.Function;
            if (mi.DeclaringType == typeof(ArrayOps)) funcType |= FunctionType.SkipThisCheck;
            if (nt == NameType.PythonMethod) funcType |= FunctionType.PythonVisible;


            RemoveNonOps(SymbolTable.StringToId(name));

            // store Python version
            StoreMethod(name, mi, funcType | FunctionType.OpsFunction);

            // store CLR version, if different and we don't have a clash (if we do
            // have a clash our version is still available under the python name)
            if (name != mi.Name && !ContainsNonOps(SymbolTable.StringToId(mi.Name))) {
                StoreMethod(mi.Name, mi, (funcType & ~FunctionType.PythonVisible)| FunctionType.OpsFunction);
            }
        }

        private bool ContainsNonOps(SymbolId name) {
            object value;
            if (dict.TryGetValue(name, out value)) {
                BuiltinFunction rum = value as BuiltinFunction;
                BuiltinMethodDescriptor bimd;
                if (rum != null) {
                    if ((rum.FunctionType & FunctionType.OpsFunction) != 0) return false;
                } else if ((bimd = value as BuiltinMethodDescriptor) != null) {
                    if ((bimd.template.FunctionType & FunctionType.OpsFunction) != 0) return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Ops functions overwrite non-ops functions.  Therefore we remove the
        /// non-ops version if both exist before inserting the new Ops method.
        /// </summary>
        private void RemoveNonOps(SymbolId name) {
            if (ContainsNonOps(name)) {
                dict.Remove(name);
            }
        }

        public override bool IsSubclassOf(object other) {
            //!!! This is an ugly special case to handle the fact the Python bool extends Python int
            if (type == typeof(bool)) {
                ReflectedType rt = other as ReflectedType;
                if (rt == null) return false;
                return rt.type == typeof(bool) || rt.type == typeof(int) || rt.type == typeof(object);
            }
            return base.IsSubclassOf(other);
        }
    }

    public class ReflectedArrayType : OpsReflectedType {
        public ReflectedArrayType(string name, Type arrayType)
            : base(name, arrayType, typeof(ArrayOps) , arrayType) {
        }

        public override object this[object index] {
            get {
                Type[] types = GetTypesFromTuple(index);
                if (types.Length != 1) throw Ops.TypeError("expected single type");

                return Ops.GetDynamicTypeFromType(types[0].MakeArrayType());
            }
        }

        public override object Call(ICallerContext context, object[] args) {
            if (args.Length != 1) throw Ops.TypeError("array expects one and only 1 argument");
            if (this.type == typeof(Array)) throw Ops.TypeError("general array type is not callable");

            return ArrayOps.CreateArray(this.type.GetElementType(), args[0]);
        }
    }

}
