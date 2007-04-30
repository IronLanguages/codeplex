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
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using IronPython.Compiler;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Hosting;

namespace IronPython.Runtime.Types {
    public class OpsReflectedTypeBuilder : ReflectedTypeBuilder {
        private Type _opsType;

        public OpsReflectedTypeBuilder() {
        }

        protected OpsReflectedTypeBuilder(Type opsType, Type extensibleType) {
            _opsType = opsType;
        }

        private OpsReflectedTypeBuilder(string name, Type opsType, Type extensibleType) {
            _opsType = opsType;
        }

        public static DynamicType Build(string name, Type baseType, Type opsType, Type extensibleType, CallTarget1 optimizedCtor) {
            OpsReflectedTypeBuilder rtb = new OpsReflectedTypeBuilder(opsType, extensibleType);
            DynamicType res = rtb.DoBuild(name, baseType, extensibleType, DefaultContext.PythonContext);
            ExtensionTypeAttribute.RegisterType(baseType, opsType, res);
            return res;
        }

        public static DynamicType Build(string name, Type baseType, Type opsType, Type extensibleType, CallTarget2 optimizedCtor) {
            OpsReflectedTypeBuilder rtb = new OpsReflectedTypeBuilder(opsType, extensibleType);
            DynamicType res = rtb.DoBuild(name, baseType, extensibleType, DefaultContext.PythonContext);
            ExtensionTypeAttribute.RegisterType(baseType, opsType, res);
            return res;
        }

        public static DynamicType Build(string name, Type baseType, Type opsType, Type extensibleType) {
            OpsReflectedTypeBuilder rtb = new OpsReflectedTypeBuilder(opsType, extensibleType);
            DynamicType res = rtb.DoBuild(name, baseType, extensibleType, DefaultContext.PythonContext);
            ExtensionTypeAttribute.RegisterType(baseType, opsType, res);
            return res;
        }        

        protected override bool IsPythonType {
            get {
                return true;
            }
        }

        protected override void AddOps() {
            Type curType = _opsType;
            if (_opsType != null) {
                do {
                    foreach (MethodInfo mi in curType.GetMethods()) {
                        AddReflectedUnboundMethod(mi);
                    }
                    curType = curType.BaseType;
                } while (curType != typeof(object));
            }

            SetValue(Symbols.Module, DefaultContext.PythonContext, "__builtin__");
        }

        private void AddReflectedUnboundMethod(MethodInfo mi) {
            if (!mi.IsStatic) return;

            if (mi.IsDefined(typeof(OperatorMethodAttribute), false)) {
                AddOperator(mi, FunctionType.Method | FunctionType.OpsFunction);
            }

            if (!ScriptDomainManager.Options.Python25 && PythonVersionAttribute.HasVersion25(mi)) return;

            string name;
            NameType nt = NameConverter.TryGetName(Builder.UnfinishedType, mi, out name);
            if (nt == NameType.None) return;

            FunctionType funcType = FunctionType.Method;
            if (name == "__new__" || mi.IsDefined(typeof(StaticOpsMethodAttribute), false)) funcType = FunctionType.Function;
            if (mi.DeclaringType == typeof(ArrayOps)) funcType |= FunctionType.SkipThisCheck;
            if (nt == NameType.PythonMethod) funcType |= FunctionType.AlwaysVisible;

            // skip ops-methods w/o a PythonName attribute

            if (name != mi.Name) {
                // store Python version
                StoreMethod(SymbolTable.StringToId(name), name, ContextId.Empty, mi, funcType | FunctionType.OpsFunction);

                // store CLR version, if different and we don't have a clash (if we do
                // have a clash our version is still available under the python name)
                if (!ContainsNonOps(SymbolTable.StringToId(mi.Name))) {
                    StoreMethod(SymbolTable.StringToId(mi.Name), mi.Name, ContextId.Empty, mi, (funcType & ~FunctionType.AlwaysVisible) | FunctionType.OpsFunction);
                }
            }
        }

        private bool ContainsNonOps(SymbolId name) {
            object value;
            if(TryGetValue(name, DefaultContext.PythonContext, out value)) {
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
                RemoveValue(name, DefaultContext.PythonContext);
            }
        }

        protected override DynamicTypeSlot StoreMethod(SymbolId methodId, string name, ContextId context, MethodInfo mi, FunctionType ft) {
            if ((ft & FunctionType.OpsFunction) != 0) RemoveNonOps(SymbolTable.StringToId(name));
            return base.StoreMethod(methodId, name, context, mi, ft);
        }
    }
}
