/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Scripting.Utils;
using Microsoft.Contracts;

namespace System.Linq.Expressions {
    public enum ArgumentType {
        Positional,
        Named
    }

    public abstract class Argument {
        private readonly ArgumentType _type;

        internal Argument(ArgumentType type) {
            _type = type;
        }

        public ArgumentType ArgumentType {
            get { return _type; }
        }
    }

    public sealed class PositionalArgument : Argument {
        private readonly int _position;

        internal PositionalArgument(int position)
            : base(ArgumentType.Positional) {
            _position = position;
        }

        public int Position {
            get { return _position; }
        }

        [Confined]
        public override bool Equals(object obj) {
            PositionalArgument arg = obj as PositionalArgument;
            return arg != null && arg._position == _position;
        }

        [Confined]
        public override int GetHashCode() {
            return _position;
        }
    }

    public sealed class NamedArgument : Argument {
        private readonly string _name;

        internal NamedArgument(string name)
            : base(ArgumentType.Named) {
            _name = name;
        }

        public string Name {
            get { return _name; }
        }

        [Confined]
        public override bool Equals(object obj) {
            NamedArgument arg = obj as NamedArgument;
            return arg != null && arg._name == _name;
        }

        [Confined]
        public override int GetHashCode() {
            return _name.GetHashCode();
        }
    }

    public partial class Expression {
        public static PositionalArgument PositionalArg(int position) {
            ContractUtils.Requires(position >= 0, "position", "must be >= 0");
            return new PositionalArgument(position);
        }
        public static NamedArgument NamedArg(string name) {
            // TODO: should we allow the empty string?
            ContractUtils.Requires(!string.IsNullOrEmpty(name), "name");
            return new NamedArgument(name);
        }
    }
}
