/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom.Compiler;

namespace Microsoft.Scripting.Runtime {
    [GeneratedCode("DLR", "2.0")]
    public static class CallTargets {
        #region Generated MaximumCallArgs

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_count from: generate_calls.py

        public const int MaximumCallArgs = 5;

        // *** END GENERATED CODE ***

        #endregion

        // argumentCount doesn't include context or this parameter
        public static Type GetTargetType(int argumentCount, bool needsThis) {
            if (needsThis) {
                switch (argumentCount) {
                    case 0: return typeof(CallTargetWithThis0);
                    case 1: return typeof(CallTargetWithThis1);
                    case 2: return typeof(CallTargetWithThis2);
                    case 3: return typeof(CallTargetWithThis3);
                    case 4: return typeof(CallTargetWithThis4);
                    case 5: return typeof(CallTargetWithThis5);
                }
            } else {
                switch (argumentCount) {
                    case 0: return typeof(CallTarget0);
                    case 1: return typeof(CallTarget1);
                    case 2: return typeof(CallTarget2);
                    case 3: return typeof(CallTarget3);
                    case 4: return typeof(CallTarget4);
                    case 5: return typeof(CallTarget5);
                }
            }
            throw new NotImplementedException();
        }
    }

    public delegate object CallTargetWithContext0(CodeContext context);
    public delegate object CallTargetN(params object[] args);
    public delegate object CallTargetWithContextN(CodeContext context, params object[] args);
    public delegate object CallTargetWithThisN(object instance, params object[] args);
    public delegate object CallTargetWithContextAndThisN(CodeContext context, object instance, params object[] args);

    #region Generated Call Targets

    // *** BEGIN GENERATED CODE ***
    // generated by function: call_targets from: generate_calls.py


    public delegate object CallTarget0();
    public delegate object CallTarget1(object arg0);
    public delegate object CallTarget2(object arg0, object arg1);
    public delegate object CallTarget3(object arg0, object arg1, object arg2);
    public delegate object CallTarget4(object arg0, object arg1, object arg2, object arg3);
    public delegate object CallTarget5(object arg0, object arg1, object arg2, object arg3, object arg4);


    // *** END GENERATED CODE ***

    #endregion

    #region Generated Call Targets With This

    // *** BEGIN GENERATED CODE ***
    // generated by function: calltargets_with_this from: generate_calls.py


    public delegate object CallTargetWithThis0(object instance);
    public delegate object CallTargetWithThis1(object instance, object arg0);
    public delegate object CallTargetWithThis2(object instance, object arg0, object arg1);
    public delegate object CallTargetWithThis3(object instance, object arg0, object arg1, object arg2);
    public delegate object CallTargetWithThis4(object instance, object arg0, object arg1, object arg2, object arg3);
    public delegate object CallTargetWithThis5(object instance, object arg0, object arg1, object arg2, object arg3, object arg4);


    // *** END GENERATED CODE ***

    #endregion
}
