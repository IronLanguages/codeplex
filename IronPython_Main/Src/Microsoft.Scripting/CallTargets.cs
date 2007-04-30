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

namespace Microsoft.Scripting {
    public static class CallTargets {
        #region Generated MaximumCallArgs

        // *** BEGIN GENERATED CODE ***

        public const int MaximumCallArgs = 5;

        // *** END GENERATED CODE ***

        #endregion

        public static Type GetTargetType(bool needsContext, int nargs) {
            if (needsContext) {
                switch (nargs) {
                    case 0: return typeof(CallTargetWithContext0);
                    case 1: return typeof(CallTargetWithContext1);
                    case 2: return typeof(CallTargetWithContext2);
                    case 3: return typeof(CallTargetWithContext3);
                    case 4: return typeof(CallTargetWithContext4);
                    case 5: return typeof(CallTargetWithContext5);
                }
            } else {
                switch (nargs) {
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


    public delegate object CallTargetN(params object[] args);
    public delegate object CallTargetWithContextN(CodeContext context, params object[] args);

    #region Generated Contextless CallTargets

    // *** BEGIN GENERATED CODE ***


    public delegate object CallTarget0();
    public delegate object CallTarget1(object arg0);
    public delegate object CallTarget2(object arg0, object arg1);
    public delegate object CallTarget3(object arg0, object arg1, object arg2);
    public delegate object CallTarget4(object arg0, object arg1, object arg2, object arg3);
    public delegate object CallTarget5(object arg0, object arg1, object arg2, object arg3, object arg4);


    // *** END GENERATED CODE ***

    #endregion

    #region Generated CallTargets

    // *** BEGIN GENERATED CODE ***


    public delegate object CallTargetWithContext0(CodeContext context);
    public delegate object CallTargetWithContext1(CodeContext context, object arg0);
    public delegate object CallTargetWithContext2(CodeContext context, object arg0, object arg1);
    public delegate object CallTargetWithContext3(CodeContext context, object arg0, object arg1, object arg2);
    public delegate object CallTargetWithContext4(CodeContext context, object arg0, object arg1, object arg2, object arg3);
    public delegate object CallTargetWithContext5(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4);


    // *** END GENERATED CODE ***

    #endregion



}
