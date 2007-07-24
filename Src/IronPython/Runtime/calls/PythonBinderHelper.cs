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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Calls {
    class PythonBinderHelper {
        public static Expression[] GetCollapsedIndexArguments<T>(DoOperationAction action, object[] args, StandardRule<T> rule) {
            int simpleArgCount = action.Operation == Operators.GetItem ? 2 : 3;

            Expression[] exprargs = new Expression[simpleArgCount];
            exprargs[0] = Ast.CodeContext();

            if (args.Length > simpleArgCount) {
                Expression[] tupleArgs = new Expression[args.Length - simpleArgCount + 1];
                for (int i = 0; i < tupleArgs.Length; i++) {
                    tupleArgs[i] = rule.Parameters[i + 1];
                }
                // multiple index arguments, pack into tuple.
                exprargs[1] = Ast.Call(null,
                    typeof(PythonOps).GetMethod("MakeTuple"),
                    tupleArgs);
            } else {
                // single index argument
                exprargs[1] = rule.Parameters[1];
            }

            if (action.Operation == Operators.SetItem) {
                exprargs[2] = rule.Parameters[rule.Parameters.Length - 1];
            }

            return exprargs;
        }


    }
}
