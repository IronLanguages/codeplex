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
using Microsoft.Scripting;

namespace Microsoft.Scripting {
    public partial class BuiltinFunction {
        #region Generated BuiltinFunction targets

        // *** BEGIN GENERATED CODE ***

        public override object Call(CodeContext context) {
            return OptimizedTarget.Call(context);
        }
        public override object Call(CodeContext context, object arg0) {
            return OptimizedTarget.Call(context, arg0);
        }
        public override object CallInstance(CodeContext context, object arg0) {
            return OptimizedTarget.CallInstance(context, arg0);
        }
        public override object Call(CodeContext context, object arg0, object arg1) {
            return OptimizedTarget.Call(context, arg0, arg1);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1) {
            return OptimizedTarget.CallInstance(context, arg0, arg1);
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2) {
            return OptimizedTarget.Call(context, arg0, arg1, arg2);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2) {
            return OptimizedTarget.CallInstance(context, arg0, arg1, arg2);
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            return OptimizedTarget.Call(context, arg0, arg1, arg2, arg3);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            return OptimizedTarget.CallInstance(context, arg0, arg1, arg2, arg3);
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return OptimizedTarget.Call(context, arg0, arg1, arg2, arg3, arg4);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return OptimizedTarget.CallInstance(context, arg0, arg1, arg2, arg3, arg4);
        }

        // *** END GENERATED CODE ***

        #endregion
    }

    public partial class BoundBuiltinFunction {
        #region Generated BoundBuiltinFunction targets

        // *** BEGIN GENERATED CODE ***

        public override object Call(CodeContext context) {
            return _target.OptimizedTarget.CallInstance(context, _instance);
        }
        public override object Call(CodeContext context, object arg0) {
            return _target.OptimizedTarget.CallInstance(context, _instance, arg0);
        }
        public override object CallInstance(CodeContext context, object arg0) {
            return _target.OptimizedTarget.CallInstance(context, _instance, arg0);
        }
        public override object Call(CodeContext context, object arg0, object arg1) {
            return _target.OptimizedTarget.CallInstance(context, _instance, arg0, arg1);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1) {
            return _target.OptimizedTarget.CallInstance(context, _instance, arg0, arg1);
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2) {
            return _target.OptimizedTarget.CallInstance(context, _instance, arg0, arg1, arg2);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2) {
            return _target.OptimizedTarget.CallInstance(context, _instance, arg0, arg1, arg2);
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            return _target.OptimizedTarget.CallInstance(context, _instance, arg0, arg1, arg2, arg3);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            return _target.OptimizedTarget.CallInstance(context, _instance, arg0, arg1, arg2, arg3);
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return _target.OptimizedTarget.CallInstance(context, _instance, arg0, arg1, arg2, arg3, arg4);
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return _target.OptimizedTarget.CallInstance(context, _instance, arg0, arg1, arg2, arg3, arg4);
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
