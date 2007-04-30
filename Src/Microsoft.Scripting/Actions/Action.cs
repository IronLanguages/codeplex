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
using System.Diagnostics;

namespace Microsoft.Scripting.Actions {
    public enum ActionKind {
        DoOperation,
        ConvertTo,

        GetMember,
        SetMember,
        DeleteMember,

        Call
        //Call and CreateInstance variants TBD
    }

    public abstract class Action {
        public abstract ActionKind Kind { get; }

        // This is an experimental simple serialization/de-serialization scheme
        public abstract string ParameterString { get; }
        private static ulong counter = 0;
        public static string MakeName(Action action) {
            return string.Format("{0}-{1}-{2}", action.Kind, action.ParameterString, counter++);
        }

        public static Action ParseName(string name) {
            Debug.Assert(name.IndexOf('-') != -1, "Action name is bad: " + name);
            ActionKind kind = (ActionKind)typeof(ActionKind).GetField(name.Substring(0, name.IndexOf('-'))).GetValue(null);
            string param = name.Substring(name.IndexOf('-')+1);
            param = param.Substring(0, param.LastIndexOf('-'));
            switch (kind) {
                case ActionKind.DoOperation:
                    return DoOperationAction.Make(param);
                case ActionKind.ConvertTo:
                    return ConvertToAction.Make(param);
                case ActionKind.GetMember:
                    return GetMemberAction.Make(param);
                case ActionKind.SetMember:
                    return SetMemberAction.Make(param);
                case ActionKind.Call:
                    return CallAction.Make(param);
                default:
                    throw new NotImplementedException(name);
            }
        }

        public override string ToString() {
            return string.Format("{0}Action({1})", this.Kind, this.ParameterString);
        }
    }
}
