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
using System.Reflection.Emit;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Compiler.Generation;
using IronPython.Compiler;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Hosting;

namespace IronPython.Runtime.Types {
    public class DelegateSignatureInfo {
        Type retType;
        ParameterInfo[] pis;
        Action<Exception> handler;

        public DelegateSignatureInfo(Type returnType, ParameterInfo[] parameterInfos, Action<Exception> exceptionHandler) {
            pis = parameterInfos;
            retType = returnType;
            handler = exceptionHandler;
        }

        public override bool Equals(object obj) {
            DelegateSignatureInfo dsi = obj as DelegateSignatureInfo;
            if (dsi == null) return false;

            if (dsi.pis.Length != pis.Length) return false;
            if (retType != dsi.retType) return false;

            for (int i = 0; i < pis.Length; i++) {
                if (dsi.pis[i] != pis[i]) return false;
            }

            if (dsi.handler != handler) return false;

            return true;
        }

        public override int GetHashCode() {
            int hashCode = 5331;

            for (int i = 0; i < pis.Length; i++) {
                hashCode ^= pis[i].GetHashCode();
            }
            hashCode ^= retType.GetHashCode();
            if (handler != null) {
                hashCode ^= handler.GetHashCode();
            }
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

        public MethodInfo CreateNewDelegate() {
            PerfTrack.NoteEvent(PerfTrack.Categories.DelegateCreate, ToString());

            Type[] delegateParams = new Type[pis.Length + 1];
            delegateParams[0] = typeof(object);
            for (int i = 0; i < pis.Length; i++) {
                delegateParams[i + 1] = pis[i].ParameterType;
            }

            CodeGen cg = DefineDelegateMethod(delegateParams);

            NewTypeMaker.EmitCallFromClrToPython(cg, cg.GetArgumentSlot(0), 1, handler);

            return cg.CreateDelegateMethodInfo();
        }

        private CodeGen DefineDelegateMethod(Type[] delegateParams) {
            AssemblyGen snippets = ScriptDomainManager.CurrentManager.Snippets.Assembly;
            //TODO maybe use staticData for closure target
            CodeGen cg = snippets.DefineMethod(ToString(), retType, delegateParams, null);
            cg.Binder = PythonBinder.Default;
            return cg;
        }
    }
}
