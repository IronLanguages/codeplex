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

namespace Microsoft.Scripting.Actions {
    public class CreateInstanceAction : CallAction {
        private static CreateInstanceAction _simple = new CreateInstanceAction();

        private CreateInstanceAction() {
        }

        private CreateInstanceAction(ArgumentKind[] args)
            : base(args) {
        }

        public static new CreateInstanceAction Make(string s) {
            if (s == "Simple") return Simple;

            return new CreateInstanceAction(ArgumentKind.ParseAll(s));
        }

        public static new CreateInstanceAction Make(params ArgumentKind[] args) {
            if (args == null) return Simple;

            for (int i = 0; i < args.Length; i++) {
                if (args[i] != ArgumentKind.Simple) {
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
            CreateInstanceAction cia = obj as CreateInstanceAction;
            if (cia == null) return false;

            return ParameterString == cia.ParameterString;
        }

        public override int GetHashCode() {
            return ArgumentKind.GetHashCode(Kind, ArgumentKinds);
        }

        public override ActionKind Kind {
            get {
                return ActionKind.CreateInstance;
            }
        }
    }
}
