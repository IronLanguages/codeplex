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
    public class CallAction : Action {
        private static readonly CallAction _simple = new CallAction();
        private readonly ArgumentKind[] _argumentKinds;

        protected CallAction() {
        }

        protected CallAction(ArgumentKind[] args) {
            _argumentKinds = args;
        }

        public static CallAction Make(params Arg[] args) {
            if (args == null) return Simple;

            ArgumentKind[] argkind = new ArgumentKind[args.Length];
            bool nonSimple = false;
            for (int i = 0; i < args.Length; i++) {
                argkind[i] = Transform(args[i]);
                if (args[i].Kind != Arg.ArgumentKind.Simple) nonSimple = true;
            }

            if (nonSimple) {
                return new CallAction(argkind);
            }

            return CallAction.Simple;
        }


        public static CallAction Make(params ArgumentKind[] args) {
            if (args == null) return Simple;

            for (int i = 0; i < args.Length; i++) {
                if (args[i] != ArgumentKind.Simple) {
                    return new CallAction(args);
                }
            }

            return CallAction.Simple;
        }

        private static ArgumentKind Transform(Arg arg) {
            switch(arg.Kind) {
                case Arg.ArgumentKind.Dictionary: return new ArgumentKind(false, false, true, arg.Name);
                case Arg.ArgumentKind.List: return new ArgumentKind(false, true, false, arg.Name);
                case Arg.ArgumentKind.Named: return new ArgumentKind(false, false, false, arg.Name);
                case Arg.ArgumentKind.Simple: return new ArgumentKind(false, false, false, arg.Name);
                case Arg.ArgumentKind.Instance: return new ArgumentKind(true, false, false, arg.Name);
                default: throw new InvalidOperationException();
            }
        }

        public static CallAction Make(string s) {
            if (s == "Simple") return Simple;

            return new CallAction(ArgumentKind.ParseAll(s));
        }

        public static CallAction Simple {
            get {
                return _simple;
            }
        }

        public ArgumentKind[] ArgumentKinds {
            get { return _argumentKinds; }
        }

        public bool IsSimple {
            get {
                return _argumentKinds == null;
            }
        }

        public override ActionKind Kind {
            get { return ActionKind.Call; }
        }

        public override string ParameterString {
            get {
                return ArgumentKind.ToParameterString(_argumentKinds);
            }
        }

        public override bool Equals(object obj) {
            CallAction other = obj as CallAction;
            if (other == null) return false;

            if (other.GetType() != GetType()) return false;

            if (_argumentKinds == null) {
                return other.ArgumentKinds == null;
            } else if (other.ArgumentKinds == null) {
                return false;
            }

            if (_argumentKinds.Length != other.ArgumentKinds.Length) return false;

            for (int i = 0; i < _argumentKinds.Length; i++) {
                if (!_argumentKinds[i].Equals(other.ArgumentKinds[i])) return false;
            }
            return true;
        }

        public override int GetHashCode() {
            return ArgumentKind.GetHashCode(Kind, _argumentKinds);
        }
    }
    /// <summary>
    /// TODO: Just use Arg objects directly?
    /// </summary>
    public class ArgumentKind {
        private bool _isThis;
        private bool _expandList;
        private bool _expandDictionary;

        private SymbolId _name;

        public static readonly ArgumentKind Simple = new ArgumentKind(false, false, false, SymbolId.Empty);

        public ArgumentKind(bool isThis, bool expandList, bool expandDictionary, SymbolId name) {
            this._isThis = isThis;
            this._expandList = expandList;
            this._expandDictionary = expandDictionary;
            this._name = name;
        }

        public bool IsThis { get { return _isThis; } }
        public bool ExpandList { get { return _expandList; } }
        public bool ExpandDictionary { get { return _expandDictionary; } }
        public SymbolId Name { get { return _name; } }

        public override bool Equals(object obj) {
            ArgumentKind o = obj as ArgumentKind;
            if (o == null) return false;

            return _isThis == o._isThis && 
                _expandList == o._expandList && 
                _expandDictionary == o._expandDictionary &&
                _name == o._name;
        }

        public override int GetHashCode() {
            int ret = _name.GetHashCode();
            if (_isThis) ret ^= 0x10000000;
            if (_expandList) ret ^= 0x20000000;
            if (_expandDictionary) ret ^= 0x40000000;
            return ret;
        }

        public bool IsSimple {
            get {
                return Equals(Simple);
            }
        }

        public string ParameterString {
            get {
                StringBuilder b = new StringBuilder();
                if (_isThis) b.Append('!');
                if (_expandList) b.Append('@');
                if (_expandDictionary) b.Append('#');
                if (_name != SymbolId.Empty) b.Append(SymbolTable.IdToString(_name));
                return b.ToString();
            }
        }

        public static ArgumentKind[] ParseAll(string s) {
            string[] pieces = s.Split(',');
            ArgumentKind[] kinds = new ArgumentKind[pieces.Length];
            for (int i = 0; i < kinds.Length; i++) {
                kinds[i] = ArgumentKind.Parse(pieces[i]);
            }
            return kinds;
        }

        public static string ToParameterString(ArgumentKind[] kinds) {
            if (kinds == null) return "Simple";

            StringBuilder b = new StringBuilder();
            for (int i = 0; i < kinds.Length; i++) {
                if (i > 0) b.Append(',');
                b.Append(kinds[i].ParameterString);
            }
            return b.ToString();
        }

        public static int GetHashCode(ActionKind kind, ArgumentKind[] kinds) {
            int h = 6551;
            if (kinds != null) {
                foreach (ArgumentKind k in kinds) {
                    h ^= (h << 5) ^ k.GetHashCode();
                }
            }
            return (int)kind << 28 ^ h;
        }

        public static ArgumentKind Parse(string s) {
            if (s.Length == 0) return Simple;
            ArgumentKind ret = new ArgumentKind(false, false, false, SymbolId.Empty);
            int i = 0;
            if (s[i] == '!') {
                i += 1;
                ret._isThis = true;
                if (i >= s.Length) return ret;
            }
            if (s[i] == '@') {
                i += 1;
                ret._expandList = true;
                if (i >= s.Length) return ret;
            }
            if (s[i] == '#') {
                i += 1;
                ret._expandDictionary = true;
                if (i >= s.Length) return ret;
            }
            ret._name = SymbolTable.StringToId(s.Substring(i));
            return ret;
        }
    }
}


