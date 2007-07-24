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

    public class InvokeMemberAction : MemberAction {
        private readonly SymbolId _memberName;
        private readonly InvokeMemberActionFlags _flags;
        private readonly ArgumentInfo[] _argumentInfos;

        public bool ReturnNonCallable { get { return (_flags & InvokeMemberActionFlags.ReturnNonCallable) != 0; } }
        public bool HasExplicitTarget { get { return (_flags & InvokeMemberActionFlags.IsCallWithThis) != 0; } }
        public SymbolId MemberName { get { return _memberName; } }
        public ArgumentInfo[] ArgumentInfos { get { return _argumentInfos; } }

        public InvokeMemberAction(SymbolId memberName, InvokeMemberActionFlags flags, ArgumentInfo[] argumentKinds)
            : base(MakeName(memberName, flags, argumentKinds)) {
            Utils.Assert.NotNull(argumentKinds);

            _memberName = memberName;
            _flags = flags;
            _argumentInfos = argumentKinds;
        }

        private static SymbolId MakeName(SymbolId memberName, InvokeMemberActionFlags flags, ArgumentInfo[] argumentKinds) {
            return SymbolTable.StringToId(String.Concat(
                (flags & InvokeMemberActionFlags.ReturnNonCallable) != 0 ? "t" : "f",
                (flags & InvokeMemberActionFlags.IsCallWithThis) != 0 ? "t" : "f",
                "-",
                SymbolTable.IdToString(memberName), 
                "-",
                ArgumentInfo.ToParameterString(argumentKinds)));
        }

        public static InvokeMemberAction Make(string s) {
            Utils.Assert.NotNull(s);
            const int FlagCount = 2;

            int lastDash = s.LastIndexOf('-');

            // s ~ [t|f]{FlagCount}-name-ArgumentKind(,ArgumentKind)*
            Debug.Assert(s.Length > FlagCount && s[FlagCount] == '-' && lastDash != -1);

            return new InvokeMemberAction(
                SymbolTable.StringToId(s.Substring(FlagCount + 1, lastDash - FlagCount - 1)),
                (s[0] == 't' ? InvokeMemberActionFlags.ReturnNonCallable : InvokeMemberActionFlags.None) |
                (s[1] == 't' ? InvokeMemberActionFlags.IsCallWithThis : InvokeMemberActionFlags.None),
                ArgumentInfo.ParseAll(s.Substring(lastDash + 1))
            );
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
    }
}
