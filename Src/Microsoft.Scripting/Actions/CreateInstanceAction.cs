/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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

namespace Microsoft.Scripting.Actions {
    public class CreateInstanceAction : CallAction, IEquatable<CreateInstanceAction> {
        private static CreateInstanceAction _simple = new CreateInstanceAction(null);

        private CreateInstanceAction(ArgumentInfo[] args)
            : base(args) {
        }

        public static new CreateInstanceAction Make(params ArgumentInfo[] args) {
            if (args == null) return Simple;

            for (int i = 0; i < args.Length; i++) {
                if (args[i].Kind != ArgumentKind.Simple) {
                    return new CreateInstanceAction(args);
                }
            }

            return CreateInstanceAction.Simple;
        }

        public static new CreateInstanceAction Make(params Arg[] args) {
            ArgumentInfo[] argkind = new ArgumentInfo[args.Length];
            bool nonSimple = false;
            for (int i = 0; i < args.Length; i++) {
                argkind[i] = args[i].Info;
                if (args[i].Kind != ArgumentKind.Simple) nonSimple = true;
            }

            if (nonSimple) {
                return new CreateInstanceAction(argkind);
            }

            return CreateInstanceAction.Simple;
        }

        public static new CreateInstanceAction Simple {
            get {
                return _simple;
            }
        }

        public override bool Equals(object obj) {
            return Equals(obj as CreateInstanceAction);
        }

        public bool Equals(CreateInstanceAction other) {
            if (other != null) {
                return ArgumentInfo.ArrayEquals(ArgumentInfos, other.ArgumentInfos);
            }
            return false;
        }

        public override int GetHashCode() {
            return ArgumentInfo.GetHashCode(Kind, ArgumentInfos);
        }

        public override DynamicActionKind Kind {
            get {
                return DynamicActionKind.CreateInstance;
            }
        }
    }
}
