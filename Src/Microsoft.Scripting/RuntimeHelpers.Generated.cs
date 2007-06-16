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

namespace Microsoft.Scripting {
    public static partial class RuntimeHelpers {
        #region Generated Call Runtime Helpers

        // *** BEGIN GENERATED CODE ***

        public static object CallWithContext(CodeContext context, object func) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context);

            return RuntimeHelpers.CallWithContext(context, func, RuntimeHelpers.EmptyObjectArray);
        }

        public static object CallWithContext(CodeContext context, object func, object arg0) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context, arg0);

            return RuntimeHelpers.CallWithContext(context, func, new object[] { arg0 });
        }

        public static object CallWithContext(CodeContext context, object func, object arg0, object arg1) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context, arg0, arg1);

            return RuntimeHelpers.CallWithContext(context, func, new object[] { arg0, arg1 });
        }

        public static object CallWithContext(CodeContext context, object func, object arg0, object arg1, object arg2) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context, arg0, arg1, arg2);

            return RuntimeHelpers.CallWithContext(context, func, new object[] { arg0, arg1, arg2 });
        }

        public static object CallWithContext(CodeContext context, object func, object arg0, object arg1, object arg2, object arg3) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context, arg0, arg1, arg2, arg3);

            return RuntimeHelpers.CallWithContext(context, func, new object[] { arg0, arg1, arg2, arg3 });
        }

        public static object CallWithContext(CodeContext context, object func, object arg0, object arg1, object arg2, object arg3, object arg4) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context, arg0, arg1, arg2, arg3, arg4);

            return RuntimeHelpers.CallWithContext(context, func, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
