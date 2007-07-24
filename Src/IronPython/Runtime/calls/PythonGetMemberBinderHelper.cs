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

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

namespace IronPython.Runtime.Calls {
    class PythonGetMemberBinderHelper<T> : 
        BinderHelper<T, GetMemberAction> {

        public PythonGetMemberBinderHelper(CodeContext context, GetMemberAction action)
            : base(context, action) {
        }

        public StandardRule<T> MakeRule(object[] args) {
            // TODO: optimize other members
            if (CanOptimizeCall(args)) {
                DynamicType argType = DynamicHelpers.GetDynamicType(args[0]);

                // look up in the Dynamictype so that we can get our custom method names (e.g. string.startswith)
                if (argType.IsSystemType) {
                    DynamicTypeSlot dts;
                    if (argType.TryResolveSlot(Context, Action.Name, out dts)) {
                        BuiltinMethodDescriptor bmd = dts as BuiltinMethodDescriptor;
                        if (bmd != null) {
                            return MakeMethodRule(bmd.Template, argType.UnderlyingSystemType);
                        }
                    }
                }
            }

            return null;
        }

        private static bool CanOptimizeCall(object[] args) {
            return args[0] != null && !(args[0] is ICustomMembers);
        }
    }
}
