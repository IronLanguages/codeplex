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
using Microsoft.Scripting.Actions;

namespace IronPython.Runtime.Operations {
    public static partial class PythonCalls {
        #region Generated Python Call Operations

        // *** BEGIN GENERATED CODE ***

        private static FastDynamicSite<object, object> _callSite0 = RuntimeHelpers.CreateSimpleCallSite<object, object>(DefaultContext.Default);
        private static FastDynamicSite<object, object, object> _callSite1 = RuntimeHelpers.CreateSimpleCallSite<object, object, object>(DefaultContext.Default);
        private static FastDynamicSite<object, object, object, object> _callSite2 = RuntimeHelpers.CreateSimpleCallSite<object, object, object, object>(DefaultContext.Default);
        private static FastDynamicSite<object, object, object, object, object> _callSite3 = RuntimeHelpers.CreateSimpleCallSite<object, object, object, object, object>(DefaultContext.Default);
        private static FastDynamicSite<object, object, object, object, object, object> _callSite4 = RuntimeHelpers.CreateSimpleCallSite<object, object, object, object, object, object>(DefaultContext.Default);
        private static FastDynamicSite<object, object, object, object, object, object, object> _callSite5 = RuntimeHelpers.CreateSimpleCallSite<object, object, object, object, object, object, object>(DefaultContext.Default);

        public static object CallWithContext(CodeContext context, object func) {
            return _callSite0.Invoke(func);
        }

        public static object CallWithContext(CodeContext context, object func, object arg0) {
            return _callSite1.Invoke(func, arg0);
        }

        public static object CallWithContext(CodeContext context, object func, object arg0, object arg1) {
            return _callSite2.Invoke(func, arg0, arg1);
        }

        public static object CallWithContext(CodeContext context, object func, object arg0, object arg1, object arg2) {
            return _callSite3.Invoke(func, arg0, arg1, arg2);
        }

        public static object CallWithContext(CodeContext context, object func, object arg0, object arg1, object arg2, object arg3) {
            return _callSite4.Invoke(func, arg0, arg1, arg2, arg3);
        }

        public static object CallWithContext(CodeContext context, object func, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return _callSite5.Invoke(func, arg0, arg1, arg2, arg3, arg4);
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
