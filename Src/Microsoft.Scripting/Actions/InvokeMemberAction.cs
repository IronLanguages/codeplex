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
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Actions {

    // TODO: Ruby specific flags?
    [Flags]
    public enum InvokeMemberActionFlags {
        None = 0,
        ReturnNonCallable = 1,  // ??
        IsCallWithThis = 2      // HasExplicitTarget
    }

    public class InvokeMemberAction : MemberAction, IEquatable<InvokeMemberAction> {
        private readonly InvokeMemberActionFlags _flags;
        private readonly ArgumentInfo[] _argumentInfos;

        public bool ReturnNonCallable { get { return (_flags & InvokeMemberActionFlags.ReturnNonCallable) != 0; } }
        public bool HasExplicitTarget { get { return (_flags & InvokeMemberActionFlags.IsCallWithThis) != 0; } }
        public ArgumentInfo[] ArgumentInfos { get { return _argumentInfos; } }

        public InvokeMemberAction(SymbolId memberName, InvokeMemberActionFlags flags, ArgumentInfo[] argumentKinds)
            : base(memberName) {
            Utils.Assert.NotNull(argumentKinds);

            _flags = flags;
            _argumentInfos = argumentKinds;
        }

        public override ActionKind Kind {
            get { return ActionKind.InvokeMember; }
        }

        public int IndexOfArgument(ArgumentKind kind) {
            for (int i = 0; i < _argumentInfos.Length; i++) {
                if (_argumentInfos[i].Kind == kind) {
                    return i;
                }
            }
            return -1;
        }

        public override bool Equals(object obj) {
            return Equals(obj as InvokeMemberAction);
        }

        public override int GetHashCode() {
            return ArgumentInfo.GetHashCode(Kind, _argumentInfos);
        }

        public override string ToString() {
            return base.ToString() + " " + _flags.ToString() + " " + ArgumentInfo.GetString(_argumentInfos);
        }


        #region IEquatable<InvokeMemberAction> Members

        public bool Equals(InvokeMemberAction other) {
            if (other == null) return false;

            return Name == other.Name &&
                _flags == other._flags &&
                ArgumentInfo.ArrayEquals(_argumentInfos, other._argumentInfos);
        }

        #endregion
    }
}
