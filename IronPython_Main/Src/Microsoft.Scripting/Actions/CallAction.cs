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
using System.Diagnostics;

namespace Microsoft.Scripting.Actions {
    public class CallAction : Action, IEquatable<CallAction> {
        private static readonly CallAction _simple = new CallAction(null);
        private readonly ArgumentInfo[] _argumentInfos;

        protected CallAction(ArgumentInfo[] args) {
            _argumentInfos = args;
        }

        public static CallAction Make(params Arg[] args) {
            if (args == null) return Simple;

            ArgumentInfo[] argkind = new ArgumentInfo[args.Length];
            bool nonSimple = false;
            for (int i = 0; i < args.Length; i++) {
                argkind[i] = args[i].Info;
                if (args[i].Kind != ArgumentKind.Simple) nonSimple = true;
            }

            if (nonSimple) {
                return new CallAction(argkind);
            }

            return CallAction.Simple;
        }


        public static CallAction Make(params ArgumentInfo[] args) {
            if (args == null) return Simple;

            for (int i = 0; i < args.Length; i++) {
                if (args[i].Kind != ArgumentKind.Simple) {
                    return new CallAction(args);
                }
            }

            return CallAction.Simple;
        }

        public static CallAction Simple {
            get {
                return _simple;
            }
        }

        public ArgumentInfo[] ArgumentInfos {
            get { return _argumentInfos; }
        }

        public bool IsSimple {
            get {
                return _argumentInfos == null;
            }
        }

        public override ActionKind Kind {
            get { return ActionKind.Call; }
        }

        public bool Equals(CallAction other) {
            if (other == null || other.GetType() != GetType()) return false;

            return ArgumentInfo.ArrayEquals(_argumentInfos, other._argumentInfos);
        }

        public override bool Equals(object obj) {
            return Equals(obj as CallAction);
        }

        public override int GetHashCode() {
            return ArgumentInfo.GetHashCode(Kind, _argumentInfos);
        }

        public override string ToString() {
            if (IsSimple) {
                return base.ToString() + " Simple";
            }
            return base.ToString() + ArgumentInfo.GetString(ArgumentInfos);
        }

        #region Helpers

        public int ArgumentCount {
            get {
                Utils.Assert.NotNull(_argumentInfos);
                return _argumentInfos.Length;
            }
        }

        // TODO: this is incorrect
        public bool IsParamsCall() {
            if (IsSimple) return false;

            foreach (ArgumentInfo info in _argumentInfos) {
                if (info.Kind == ArgumentKind.Named || info.Kind == ArgumentKind.Dictionary || info.Kind == ArgumentKind.Instance) {
                    return false;
                }
            }

            return _argumentInfos[_argumentInfos.Length - 1].Kind == ArgumentKind.List;
        }

        public ArgumentKind GetArgumentKind(int i) {
            Debug.Assert(i >= 0 && (_argumentInfos == null || i < _argumentInfos.Length));
            return _argumentInfos != null ? _argumentInfos[i].Kind : ArgumentKind.Simple;
        }

        public SymbolId GetArgumentName(int i) {
            Debug.Assert(i >= 0 && (_argumentInfos == null || i < _argumentInfos.Length));
            return _argumentInfos != null ? _argumentInfos[i].Name : SymbolId.Empty;
        }

        /// <summary>
        /// Gets the number of positional arguments the user provided at the call site.
        /// </summary>
        public int GetProvidedPositionalArgumentCount(int parameterCount) {
            if (IsSimple) return parameterCount - 1;

            int cnt = _argumentInfos.Length;
            for (int i = 0; i < _argumentInfos.Length; i++) {
                ArgumentKind kind = _argumentInfos[i].Kind;
                
                if (kind == ArgumentKind.Dictionary || kind == ArgumentKind.List || kind == ArgumentKind.Named) {
                    cnt--;
                }
            }

            return cnt;
        }

        public bool HasKeywordArgument() {
            if (IsSimple) return false;

            foreach (ArgumentInfo info in _argumentInfos) {
                if (info.Kind == ArgumentKind.Dictionary || info.Kind == ArgumentKind.Named) {
                    return true;
                }
            }
            return false;
        }

        public bool HasDictionaryArgument() {
            if (IsSimple) return false;

            foreach (ArgumentInfo info in _argumentInfos) {
                if (info.Kind == ArgumentKind.Dictionary) {
                    return true;
                }
            }
            return false;
        }

        public bool HasNamedArgument() {
            if (IsSimple) return false;

            foreach (ArgumentInfo info in _argumentInfos) {
                if (info.Kind == ArgumentKind.Named) {
                    return true;
                }
            }

            return false;
        }

        public SymbolId[] GetArgumentNames() {
            if (IsSimple) return SymbolId.EmptySymbols;

            List<SymbolId> res = new List<SymbolId>();
            foreach (ArgumentInfo info in _argumentInfos) {
                if (info.Name != SymbolId.Empty) {
                    res.Add(info.Name);
                }
            }

            return res.ToArray();
        }

        #endregion
    }

    /// <summary>
    /// TODO: Alternatively, it should be sufficient to remember indices for this, list, dict and block.
    /// </summary>
    public struct ArgumentInfo : IEquatable<ArgumentInfo> {
        private readonly ArgumentKind _kind;
        private readonly SymbolId _name;

        public static readonly ArgumentInfo Simple = new ArgumentInfo(ArgumentKind.Simple, SymbolId.Empty);

        public ArgumentKind Kind { get { return _kind; } }
        public SymbolId Name { get { return _name; } }

        public ArgumentInfo(SymbolId name) {
            _kind = ArgumentKind.Named;
            _name = name;
        }

        public ArgumentInfo(ArgumentKind kind) {
            _kind = kind;
            _name = SymbolId.Empty;
        }

        public ArgumentInfo(ArgumentKind kind, SymbolId name) {
            Debug.Assert((kind == ArgumentKind.Named) ^ (name == SymbolId.Empty));
            _kind = kind;
            _name = name;
        }

        public override bool Equals(object obj) {
            return obj is ArgumentInfo && Equals((ArgumentInfo)obj);
        }

        public bool Equals(ArgumentInfo other) {
            return _kind == other._kind && _name == other._name;
        }

        public override int GetHashCode() {
            return _name.GetHashCode() ^ (int)_kind << 7;
        }

        public bool IsSimple {
            get {
                return Equals(Simple);
            }
        }

        internal static string GetString(ArgumentInfo[] args) {
            StringBuilder sb = new StringBuilder("(");
            foreach (ArgumentInfo arg in args) {
                if (sb.Length != 0) {
                    sb.Append(", ");
                }
                sb.Append(arg.ToString());
            }
            sb.Append(")");
            return sb.ToString();
        }

        internal static bool ArrayEquals(ArgumentInfo[] self, ArgumentInfo[] other) {
            if (self == null) {
                return other == null;
            } else if (other == null) {
                return false;
            }

            if (self.Length != other.Length) return false;

            for (int i = 0; i < self.Length; i++) {
                if (!self[i].Equals(other[i])) return false;
            }
            return true;
        }

        internal static int GetHashCode(ActionKind kind, ArgumentInfo[] kinds) {
            int h = 6551;
            if (kinds != null) {
                foreach (ArgumentInfo k in kinds) {
                    h ^= (h << 5) ^ k.GetHashCode();
                }
            }
            return (int)kind << 28 ^ h;
        }
    }
}


