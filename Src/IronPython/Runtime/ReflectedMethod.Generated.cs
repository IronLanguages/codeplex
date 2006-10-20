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
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


using IronPython.Compiler;
using IronPython.Modules;
using IronMath;

namespace IronPython.Runtime {
    public partial class ReflectedMethodBase {
        #region Generated ReflectedMethod context targets

        // *** BEGIN GENERATED CODE ***

        public override object Call(ICallerContext context) {
            if (HasInstance) return MyFastCallable.CallInstance(context, Instance);
            else return MyFastCallable.Call(context);
        }
        public override object Call(ICallerContext context, object arg0) {
            if (HasInstance) return MyFastCallable.CallInstance(context, Instance, arg0);
            else return MyFastCallable.Call(context, arg0);
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            if (HasInstance) return MyFastCallable.CallInstance(context, Instance, arg0, arg1);
            else return MyFastCallable.Call(context, arg0, arg1);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            if (HasInstance) return MyFastCallable.CallInstance(context, Instance, arg0, arg1, arg2);
            else return MyFastCallable.Call(context, arg0, arg1, arg2);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            if (HasInstance) return MyFastCallable.CallInstance(context, Instance, arg0, arg1, arg2, arg3);
            else return MyFastCallable.Call(context, arg0, arg1, arg2, arg3);
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            if (HasInstance) return MyFastCallable.CallInstance(context, Instance, arg0, arg1, arg2, arg3, arg4);
            else return MyFastCallable.Call(context, arg0, arg1, arg2, arg3, arg4);
        }

        // *** END GENERATED CODE ***

        #endregion

        #region Generated ReflectedMethod IFastCallable

        // *** BEGIN GENERATED CODE ***

        public override object Call() {
            return Call((ICallerContext)null);
        }
        public override object Call(object arg0) {
            return Call((ICallerContext)null, arg0);
        }
        public override object Call(object arg0, object arg1) {
            return Call((ICallerContext)null, arg0, arg1);
        }
        public override object Call(object arg0, object arg1, object arg2) {
            return Call((ICallerContext)null, arg0, arg1, arg2);
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3) {
            return Call((ICallerContext)null, arg0, arg1, arg2, arg3);
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3, object arg4) {
            return Call((ICallerContext)null, arg0, arg1, arg2, arg3, arg4);
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}