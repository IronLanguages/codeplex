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

namespace IronPython.Runtime.Calls {
    #region Generated Python Call Targets

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

    public delegate object CallTargetN(params object[] args);

    internal static class PythonCallTargets {
        internal static Type GetPythonTargetType(bool wrapper, int parameters) {
            if (!wrapper) {
                switch (parameters) {
                    #region Generated Python Call Target Switch

                    // *** BEGIN GENERATED CODE ***
                    // generated by function: gen_python_switch from: generate_calls.py

                    case 0: return typeof(CallTarget0);
                    case 1: return typeof(CallTarget1);
                    case 2: return typeof(CallTarget2);
                    case 3: return typeof(CallTarget3);
                    case 4: return typeof(CallTarget4);
                    case 5: return typeof(CallTarget5);

                    // *** END GENERATED CODE ***

                    #endregion
                }
            }

            return typeof(CallTargetN);
        }
    }
}
