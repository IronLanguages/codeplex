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
using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

using System.Diagnostics;
using IronPython.Runtime;


namespace IronPython.Compiler {
    /// <summary>
    /// A MethodTracker is like a MethodInfo except you can always access parameter information.
    /// 
    /// A MethodTracker can be constructed from either a baked MethodInfo or from an unbaked
    /// MethodBuilder or ConstructorInfo if the ParameterInfo is also supplied.
    /// </summary>
    public class MethodTracker {
        MethodBase mi;
        ParameterInfo[] sig;
        string name;

        public static MethodTracker[] GetTrackerArray(MethodBase[] infos) {
            MethodTracker[] res = new MethodTracker[infos.Length];
            for (int i = 0; i < res.Length; i++) res[i] = new MethodTracker(infos[i]);
            return res;
        }

        public MethodTracker(MethodBase info) {
            mi = info;
            sig = info.GetParameters();
            name = info.Name;
        }

        //public MethodTracker(ConstructorBuilder cb, ParameterInfo[] signature) {
        //    mi = cb;
        //    sig = signature;
        //    name = IronPython.Runtime.ReflectedType.MakeNewName;
        //}

        public MethodTracker(MethodBuilder mb, ParameterInfo[] signature) {
            mi = mb;
            sig = signature;
            name = mb.Name;
        }

        public MethodBase Method {
            get {
                return mi;
            }
        }

        public int SigLength {
            get {
                return sig.Length;
            }
        }

        public int DefaultCount {
            get {
                int defaultCount = 0;
                for (int i = sig.Length - 1; i >= 0; i--) {
                    if (sig[i].DefaultValue != DBNull.Value) {
                        defaultCount++;
                    } else {
                        break;
                    }
                }
                return defaultCount;
            }
        }

        public int StaticArgs {
            get {
                if (IsStatic) return SigLength;

                return SigLength + 1;
            }
        }

        public string Name {
            get {
                return name;
            }
        }
        public bool IsStatic {
            get {
                if (mi is ConstructorInfo) {
                    return true;
                } else if (mi.IsStatic) {
                    return true;
                }
                return false;
            }
        }

        public Type DeclaringType {
            get {
                return mi.DeclaringType;
            }
        }

        public Type GetParameterType(int index) {
            return sig[index].ParameterType;
        }

        public ParameterInfo[] GetParameters() {
            return sig;
        }
        
        internal static bool IsOutParameter(ParameterInfo pi) {
            return pi.IsOut && !pi.IsIn;
        }

        public int GetInArgCount() {
            int res = 0;
            ParameterInfo[] pis = GetParameters();
            for (int i = 0; i < pis.Length; i++) {
                if (!IsOutParameter(pis[i])) res++;
            }
            return res;
        }

        public bool HasOutParameters {
            get {
                ParameterInfo[] pis = GetParameters();
                for (int i = 0; i < pis.Length; i++) {
                    if (IsOutParameter(pis[i])) return true;
                }
                return false;
            }
        }

        public bool IsParamsMethod {
            get {
                return sig.Length > 0 && (sig[sig.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0);
            }
        }
    }

    class TrackingParamInfo : ParameterInfo {
        Type type;
        string name;

        public TrackingParamInfo(Type parameterType) {
            type = parameterType;
        }

        public TrackingParamInfo(Type parameterType, string parameterName) {
            type = parameterType;
            name = parameterName;
        }

        public override Type ParameterType {
            get {
                return type;
            }
        }

        public override string Name {
            get {
                if (name != null) return name;

                return base.Name;
            }
        }

        public override object[] GetCustomAttributes(bool inherit) {
            return new object[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
            return new object[0];
        }
    }

}
