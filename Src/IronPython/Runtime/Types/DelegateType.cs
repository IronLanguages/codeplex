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
        private FastCallable invoker;

        public ReflectedDelegateType(Type delegateType)
            : base(delegateType) {
        }

        public override object Call(object func, params object[] args) {
            if (invoker == null) {
                CreateInvoker();
            }
            
            // put delegate's Target object into args
            Delegate d = func as Delegate;
            Debug.Assert(d != null);

            return invoker.CallInstance(DefaultContext.Default, d, args);
        }

        private void CreateInvoker() {
            MethodInfo delegateInfo = type.GetMethod("Invoke");
            Debug.Assert(delegateInfo != null);
            invoker = MethodBinder.MakeFastCallable("invoke", delegateInfo, FunctionType.Method);
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
                if(dsi.pis[i] != pis[i]) return false;
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
            int inArgCount = 0;
            List<int> outArgs = null;
            for (int parameter = 0; parameter < pis.Length; parameter++) {
                delegateParams[parameter + 1] = pis[parameter].ParameterType;
                if (!pis[parameter].IsOut || pis[parameter].IsIn) {
                    inArgCount++;
                    if (pis[parameter].ParameterType.IsByRef) {
                        if (outArgs == null) outArgs = new List<int>();
                        outArgs.Add(parameter);
                    }
                } else {
                    if (outArgs == null) outArgs = new List<int>();
                    outArgs.Add(parameter);
                }                
            }

            CodeGen cg = DefineDelegateMethod(delegateParams);
            
            bool hasParams = (pis.Length > 0 && pis[pis.Length - 1].IsDefined(typeof(ParamArrayAttribute), false));
            if (inArgCount <= Ops.MaximumCallArgs && !hasParams) {  
                // simple case: no out-params, no params arrays, and
                // we have a small enough arguments we can just call Ops.Call(object func, object arg0, ...)

                // target function
                cg.EmitArgGet(0);   

                // arguments
                for (int parameter = 0; parameter < pis.Length; parameter++) {  
                    EmitDelegateParameter(cg, parameter);
                }
                
                // and do the call
                cg.EmitCall(typeof(Ops), "Call", CompilerHelpers.MakeRepeatedArray(typeof(object), inArgCount + 1));
            } else {
                // complex case: we either have params, out/by ref args, or 
                // have too many arguments so we need to call the params-array object

                // target function
                cg.EmitArgGet(0);                           

                //  Rest of arguments go in an object array - create the array
                cg.EmitObjectArray(hasParams ? inArgCount - 1 : inArgCount, delegate(int parameter) {
                    EmitDelegateParameter(cg, parameter);
                });

                // and do the call
                if (hasParams) {
                    // emit args tuple from last argument
                    cg.EmitArgGet(pis.Length);
                    cg.EmitCall(typeof(Ops), "MakeTuple");

                    cg.EmitCall(typeof(Ops), "CallWithArgsTuple");
                } else {
                    cg.EmitCall(typeof(Ops), "Call",
                        new Type[] { typeof(object), typeof(object[]) });
                }
            }

            // finally emit the return values
            if (outArgs == null) {
                cg.EmitCastFromObject(retType);
                cg.Emit(OpCodes.Ret);
            } else {
                EmitOutArgReturn(cg, outArgs);
            }            

            return cg.CreateDelegateMethodInfo();
        }

        private void EmitOutArgReturn(CodeGen cg, List<int> outArgs) {
            if (retType == typeof(void) && outArgs.Count == 1) {
                // void return type, single out arg, no tuple

                // save value in temp
                Slot tmpLocal = cg.GetLocalTmp(typeof(object));
                tmpLocal.EmitSet(cg);

                // ..., addr, val -> ...
                cg.EmitArgGet(outArgs[0] + 1);  // get arg addr 
                tmpLocal.EmitGet(cg);            // get value
                // cast value from object to correct type
                cg.EmitCastFromObject(pis[outArgs[0]].ParameterType.GetElementType());
                // store 
                cg.EmitStoreValueIndirect(pis[outArgs[0]].ParameterType.GetElementType());

                cg.Emit(OpCodes.Ret);

                cg.FreeLocalTmp(tmpLocal);
            } else {                
                cg.Emit(OpCodes.Castclass, typeof(Tuple));
                int start = retType == typeof(void) ? 0 : 1;
                Slot tmpLocal = cg.GetLocalTmp(typeof(object));

                for (int i = 0; i < outArgs.Count; i++) {
                    cg.Emit(OpCodes.Dup);                   // copy the tuple
                    cg.EmitInt(i + start);
                    cg.EmitCall(typeof(Tuple), "get_Item", new Type[] { typeof(int) }); // get the current 
                    tmpLocal.EmitSet(cg);

                    cg.EmitArgGet(outArgs[i] + 1);
                    tmpLocal.EmitGet(cg);
                    cg.EmitCastFromObject(pis[outArgs[i]].ParameterType.GetElementType());

                    cg.EmitStoreValueIndirect(pis[outArgs[i]].ParameterType.GetElementType());
                }

                // use the final tuple on the stack to get our return value.
                cg.EmitInt(0);
                cg.EmitCall(typeof(Tuple), "get_Item", new Type[] { typeof(int) }); // get the current 

                cg.EmitCastFromObject(retType);
                cg.Emit(OpCodes.Ret);
            }
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
        
        private void EmitDelegateParameter(CodeGen cg, int parameter) {
            ParameterInfo pi = pis[parameter];
            if (!pi.IsOut || pi.IsIn) {
                cg.EmitArgGet(parameter + 1);
                if (pi.ParameterType.IsByRef) {
                    cg.EmitLoadValueIndirect(pi.ParameterType.GetElementType());
                    cg.EmitCastToObject(pi.ParameterType.GetElementType());
                } else {
                    cg.EmitCastToObject(pi.ParameterType);
                }
            }
        }
    }
}
