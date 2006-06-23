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

namespace IronPython.Compiler {
    static class CompilerHelpers {
        public static MethodAttributes PublicStatic = MethodAttributes.Public | MethodAttributes.Static;

        public static T[] MakeRepeatedArray<T>(T item, int count) {
            T[] ret = new T[count];
            for (int i = 0; i < count; i++) ret[i] = item;
            return ret;
        }

        public static Type[] GetTypes(ParameterInfo[] parameterInfos) {
            Type[] ret = new Type[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++) ret[i] = parameterInfos[i].ParameterType;
            return ret;
        }

        public static Type GetReturnType(MethodBase mi) {
            if (mi.IsConstructor) return mi.DeclaringType;
            else return ((MethodInfo)mi).ReturnType;
        }

        public static int GetStaticNumberOfArgs(MethodBase method) {
            if (IsStatic(method)) return method.GetParameters().Length;

            return method.GetParameters().Length + 1;
        }

        public static bool IsParamsMethod(MethodBase method) {
            ParameterInfo[] pis = method.GetParameters();
            return pis.Length > 0 && (pis[pis.Length-1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0);

        }

        public static bool IsStatic(MethodBase mi) {
            return mi.IsConstructor || mi.IsStatic;
        }
    }
}
