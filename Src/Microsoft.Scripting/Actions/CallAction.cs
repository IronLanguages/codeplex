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
using Microsoft.Scripting.Internal.Ast;

namespace Microsoft.Scripting.Actions {
    public class CallAction : Action {
        private static CallAction _simple = new CallAction();
        private ArgumentKind[] _argumentKinds;

        private CallAction() {
        }

        private CallAction(ArgumentKind[] args) {
            _argumentKinds = args;
        }

        public static CallAction Make(params Arg[] args) {
            return Make(false, args);
        }

        public static CallAction Make(bool withThis, params Arg[] args) {
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

            string[] pieces = s.Split(',');
            ArgumentKind[] kinds = new ArgumentKind[pieces.Length];
            for (int i = 0; i < kinds.Length; i++) {
                kinds[i] = ArgumentKind.Parse(pieces[i]);
            }
            return new CallAction(kinds);
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
                if (_argumentKinds == null) return "Simple";

                StringBuilder b = new StringBuilder();
                for (int i = 0; i < _argumentKinds.Length; i++) {
                    if (i > 0) b.Append(',');
                    b.Append(_argumentKinds[i].ParameterString);
                }
                return b.ToString();
            }
        }

        public override bool Equals(object obj) {
            CallAction other = obj as CallAction;
            if (other == null) return false;
            if (IsSimple) {
                if (other.IsSimple) 
                    return true;
                return false;
            } else if(other.IsSimple) {
                return false;
            }
            
            if (other._argumentKinds.Length != this._argumentKinds.Length) {
                return false;
            }
            for (int i = 0; i < _argumentKinds.Length; i++) {
                if (!other._argumentKinds[i].Equals(this._argumentKinds[i])) {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode() {
            int h = 0;
            if (!IsSimple) {
                foreach (ArgumentKind kind in _argumentKinds) {
                    h ^= kind.GetHashCode();
                }
            }
            return (int)Kind << 28 ^ h;
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


