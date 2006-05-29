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
            return Call(context, new object[0]);
        }
        public override object Call(ICallerContext context, object arg0) {
            return Call(context, new object[]{ arg0});
        }
        public override object Call(ICallerContext context, object arg0, object arg1) {
            return Call(context, new object[]{ arg0, arg1});
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2) {
            return Call(context, new object[]{ arg0, arg1, arg2});
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
            return Call(context, new object[]{ arg0, arg1, arg2, arg3});
        }
        public override object Call(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return Call(context, new object[]{ arg0, arg1, arg2, arg3, arg4});
        }

        // *** END GENERATED CODE ***

        #endregion

        #region Generated ReflectedMethod IFastCallable

        // *** BEGIN GENERATED CODE ***

        public override object Call() {
            return Call(new object[]{ });
        }
        public override object Call(object arg0) {
            return Call(new object[]{ arg0});
        }
        public override object Call(object arg0, object arg1) {
            return Call(new object[]{ arg0, arg1});
        }
        public override object Call(object arg0, object arg1, object arg2) {
            return Call(new object[]{ arg0, arg1, arg2});
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3) {
            return Call(new object[]{ arg0, arg1, arg2, arg3});
        }
        public override object Call(object arg0, object arg1, object arg2, object arg3, object arg4) {
            return Call(new object[]{ arg0, arg1, arg2, arg3, arg4});
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}