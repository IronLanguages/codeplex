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

namespace Microsoft.Scripting.Actions {
    public class CreateInstanceAction : CallAction, IEquatable<CreateInstanceAction> {
        private static CreateInstanceAction _simple = new CreateInstanceAction(null);

        private CreateInstanceAction(ArgumentInfo[] args)
            : base(args) {
        }

        public static new CreateInstanceAction Make(string s) {
            return new CreateInstanceAction(ArgumentInfo.ParseAll(s));
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

        public static new CreateInstanceAction Simple {
            get {
                return _simple;
            }
        }

        public override bool Equals(object obj) {
            return Equals(obj as CreateInstanceAction);
        }

        public bool Equals(CreateInstanceAction other) {
            return other != null && ParameterString == other.ParameterString;
        }

        public override int GetHashCode() {
            return ArgumentInfo.GetHashCode(Kind, ArgumentInfos);
        }

        public override ActionKind Kind {
            get {
                return ActionKind.CreateInstance;
            }
        }
    }
}
