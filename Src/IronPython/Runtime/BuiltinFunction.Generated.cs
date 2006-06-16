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

namespace IronPython.Runtime.Calls {
    public partial class BuiltinFunction {
        #region Generated BuiltinFunction targets

        // *** BEGIN GENERATED CODE ***

        public override object Call(ICallerContext context) {
            return OptimizedTarget.Call(context);
        }
        public override object Call(ICallerContext context, object arg0) {
            return OptimizedTarget.Call(context, arg0);
        }
        public override object CallInstance(ICallerContext context, object arg0) {
            return OptimizedTarget.CallInstance(context, arg0);
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            return OptimizedTarget.Call(context, arg0, arg1);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1) {
            return OptimizedTarget.CallInstance(context, arg0, arg1);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            return OptimizedTarget.Call(context, arg0, arg1, arg2);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2) {
            return OptimizedTarget.CallInstance(context, arg0, arg1, arg2);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            return OptimizedTarget.Call(context, arg0, arg1, arg2, arg3);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            return OptimizedTarget.CallInstance(context, arg0, arg1, arg2, arg3);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return OptimizedTarget.Call(context, arg0, arg1, arg2, arg3, arg4);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return OptimizedTarget.CallInstance(context, arg0, arg1, arg2, arg3, arg4);
        }

        // *** END GENERATED CODE ***

        #endregion
    }

    public partial class BoundBuiltinFunction {
        #region Generated BoundBuiltinFunction targets

        // *** BEGIN GENERATED CODE ***

        public override object Call(ICallerContext context) {
            return target.OptimizedTarget.CallInstance(context, instance);
        }
        public override object Call(ICallerContext context, object arg0) {
            return target.OptimizedTarget.CallInstance(context, instance, arg0);
        }
        public override object CallInstance(ICallerContext context, object arg0) {
            return target.OptimizedTarget.CallInstance(context, instance, arg0);
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            return target.OptimizedTarget.CallInstance(context, instance, arg0, arg1);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1) {
            return target.OptimizedTarget.CallInstance(context, instance, arg0, arg1);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            return target.OptimizedTarget.CallInstance(context, instance, arg0, arg1, arg2);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2) {
            return target.OptimizedTarget.CallInstance(context, instance, arg0, arg1, arg2);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            return target.OptimizedTarget.CallInstance(context, instance, arg0, arg1, arg2, arg3);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            return target.OptimizedTarget.CallInstance(context, instance, arg0, arg1, arg2, arg3);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return target.OptimizedTarget.CallInstance(context, instance, arg0, arg1, arg2, arg3, arg4);
        }
        public override object CallInstance(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return target.OptimizedTarget.CallInstance(context, instance, arg0, arg1, arg2, arg3, arg4);
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
