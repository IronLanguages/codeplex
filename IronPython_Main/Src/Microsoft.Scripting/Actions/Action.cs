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
        InvokeMember,

        Call,
        CreateInstance
    }

    public abstract class Action {
        public abstract ActionKind Kind { get; }

        // This is an experimental simple serialization/de-serialization scheme
        public abstract string ParameterString { get; }
        private static ulong counter;
        public static string MakeName(Action action) {
            return action.Kind.ToString() + "-" + action.ParameterString + "-" + (counter++).ToString();
        }

        public static Action ParseName(string name) {
            int separator = name.IndexOf('-');
            Debug.Assert(separator != -1, "Action name is bad: " + name);

            ActionKind kind = (ActionKind)Enum.Parse(typeof(ActionKind), name.Substring(0, separator), false);
            int lastSeparator = name.LastIndexOf('-');
            string param = name.Substring(separator + 1, lastSeparator - separator - 1);

            switch (kind) {
                case ActionKind.DoOperation:
                    return DoOperationAction.Make(param);
                case ActionKind.ConvertTo:
                    return ConvertToAction.Make(param);
                case ActionKind.GetMember:
                    return GetMemberAction.Make(param);
                case ActionKind.SetMember:
                    return SetMemberAction.Make(param);
                case ActionKind.InvokeMember:
                    return InvokeMemberAction.Make(param);
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
