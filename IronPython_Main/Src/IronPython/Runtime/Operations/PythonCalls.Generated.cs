/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

using DefaultContext = IronPython.Runtime.Calls.DefaultContext;

namespace IronPython.Runtime.Operations {
    public static partial class PythonCalls {
        #region Generated Python Call Operations

        // *** BEGIN GENERATED CODE ***


        public static object Call(object func) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(DefaultContext.Default);

            return PythonCalls.Call(func, ArrayUtils.EmptyObjects);
        }

        public static object Call(object func, object arg0) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(DefaultContext.Default, arg0);

            return PythonCalls.Call(func, new object[] { arg0 });
        }

        public static object Call(object func, object arg0, object arg1) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(DefaultContext.Default, arg0, arg1);

            return PythonCalls.Call(func, new object[] { arg0, arg1 });
        }

        public static object Call(object func, object arg0, object arg1, object arg2) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(DefaultContext.Default, arg0, arg1, arg2);

            return PythonCalls.Call(func, new object[] { arg0, arg1, arg2 });
        }

        public static object Call(object func, object arg0, object arg1, object arg2, object arg3) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(DefaultContext.Default, arg0, arg1, arg2, arg3);

            return PythonCalls.Call(func, new object[] { arg0, arg1, arg2, arg3 });
        }

        public static object Call(object func, object arg0, object arg1, object arg2, object arg3, object arg4) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(DefaultContext.Default, arg0, arg1, arg2, arg3, arg4);

            return PythonCalls.Call(func, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
