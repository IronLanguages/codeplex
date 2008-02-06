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

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Calls {
    using Ast = Microsoft.Scripting.Ast.Ast;

    class PythonSetMemberBinderHelper<T> : SetMemberBinderHelper<T> {
        public PythonSetMemberBinderHelper(CodeContext context, SetMemberAction action, object[] args) :
            base(context, action, args) {
        }

        protected override StandardRule<T> MakeEventValidation(MemberGroup members) {
            EventTracker ev = (EventTracker)members[0];

            AddToBody(
               Rule.MakeReturn(Binder,
                   Ast.Call(
                       typeof(PythonOps).GetMethod("SlotTrySetValue"),
                       Ast.CodeContext(),
                       Ast.RuntimeConstant(PythonTypeOps.GetReflectedEvent(ev)),
                       Ast.ConvertHelper(Rule.Parameters[0], typeof(object)),
                       Ast.Null(typeof(PythonType)),
                       Ast.ConvertHelper(Rule.Parameters[1], typeof(object))
                   )
               )
            );
            return Rule;
        }
    }
}
