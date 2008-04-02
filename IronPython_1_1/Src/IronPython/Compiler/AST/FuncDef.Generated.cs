/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {
    public partial class FunctionDefinition {
        #region Generated FuncDef Code

        // *** BEGIN GENERATED CODE ***

        private void GetFunctionType(out Type ft, out Type tt) {
            if (flags == FunctionAttributes.None) {
                if (parameters.Length <= Ops.MaximumCallArgs) {
                    switch (parameters.Length) {
                        case 0: ft = typeof(Function0); tt = typeof(CallTarget0); break;
                        case 1: ft = typeof(Function1); tt = typeof(CallTarget1); break;
                        case 2: ft = typeof(Function2); tt = typeof(CallTarget2); break;
                        case 3: ft = typeof(Function3); tt = typeof(CallTarget3); break;
                        case 4: ft = typeof(Function4); tt = typeof(CallTarget4); break;
                        case 5: ft = typeof(Function5); tt = typeof(CallTarget5); break;
                        default: ft = typeof(FunctionN); tt = typeof(CallTargetN); break;
                    }
                } else {
                    ft = typeof(FunctionN); tt = typeof(CallTargetN);
                }
            } else {
                ft = typeof(FunctionX); tt = typeof(CallTargetN);
            }
        }

        // *** END GENERATED CODE ***

        #endregion

    }
}
