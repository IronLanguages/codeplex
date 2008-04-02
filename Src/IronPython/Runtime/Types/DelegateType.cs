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
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Reflection.Emit;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Compiler.Generation;
using IronPython.Compiler;

namespace IronPython.Runtime.Types {
    public class ReflectedDelegateType : ReflectedType {
        private static BuiltinFunction DelegateNew = BuiltinFunction.MakeMethod("__new__", typeof(ReflectedDelegateType).GetMethod("MakeNew"), FunctionType.Function | FunctionType.PythonVisible);

        public static Delegate MakeNew(DynamicType type, object from) {
            return Ops.GetDelegate(from, ((ReflectedType)type).type);
        }

        private FastCallable invoker;

        public ReflectedDelegateType(Type delegateType)
            : base(delegateType) {
        }

        protected override void CreateNewMethod() {
            dict[SymbolTable.NewInst] = ctor = DelegateNew;
        }

        public override object CallOnInstance(object func, object[] args) {
            if (invoker == null) {
                CreateInvoker();
            }

            Debug.Assert(func is Delegate);

            return invoker.CallInstance(DefaultContext.Default, func, args);
        }

        private void CreateInvoker() {
            MethodInfo delegateInfo = type.GetMethod("Invoke");
            Debug.Assert(delegateInfo != null);
            invoker = MethodBinder.MakeFastCallable("invoke", delegateInfo, false);
        }
    }

    public class DelegateSignatureInfo {
        Type retType;
        ParameterInfo[] pis;

        public DelegateSignatureInfo(Type returnType, ParameterInfo[] parameterInfos) {
            pis = parameterInfos;
            retType = returnType;
        }

        public override bool Equals(object obj) {
            DelegateSignatureInfo dsi = obj as DelegateSignatureInfo;
            if (dsi == null) return false;

            if (dsi.pis.Length != pis.Length) return false;
            if (retType != dsi.retType) return false;

            for (int i = 0; i < pis.Length; i++) {
                if (dsi.pis[i] != pis[i]) return false;
            }

            return true;
        }

        public override int GetHashCode() {
            int hashCode = 5331;

            for (int i = 0; i < pis.Length; i++) {
                hashCode ^= pis[i].GetHashCode();
            }
            hashCode ^= retType.GetHashCode();

            return hashCode;
        }

        public override string ToString() {
            StringBuilder text = new StringBuilder();
            text.Append(retType.ToString());
            text.Append("(");
            for (int i = 0; i < pis.Length; i++) {
                if (i != 0) text.Append(", ");
                text.Append(pis[i].ParameterType.Name);
            }
            text.Append(")");
            return text.ToString();
        }

#if DEBUG
        private static int index;
#endif

        public MethodInfo CreateNewDelegate() {
            PerfTrack.NoteEvent(PerfTrack.Categories.DelegateCreate, ToString());

            Type[] delegateParams = new Type[pis.Length + 1];
            delegateParams[0] = typeof(object);
            for (int i = 0; i < pis.Length; i++) {
                delegateParams[i + 1] = pis[i].ParameterType;
            }

            CodeGen cg = DefineDelegateMethod(delegateParams);

            NewTypeMaker.EmitCallFromClrToPython(cg, cg.GetArgumentSlot(0), 1);

            return cg.CreateDelegateMethodInfo();
        }

        private CodeGen DefineDelegateMethod(Type[] delegateParams) {
            CodeGen cg;
#if DEBUG
            if (Options.SaveAndReloadBinaries) {
                TypeGen tg = OutputGenerator.Snippets.DefinePublicType("DelegateType" + ToString() + index++, typeof(object));
                cg = tg.DefineUserHiddenMethod(MethodAttributes.Public | MethodAttributes.Static,
                    "Invoke" + ToString(), retType, delegateParams);
            } else
#endif
                cg = OutputGenerator.Snippets.DefineDynamicMethod(ToString(),
                                                                          retType,
                                                                          delegateParams);
            return cg;
        }
    }
}
