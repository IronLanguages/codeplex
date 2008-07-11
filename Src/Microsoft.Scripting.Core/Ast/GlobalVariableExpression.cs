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

using System.Diagnostics;

namespace System.Linq.Expressions {

    /// <summary>
    /// Represents a variable accessed from the host's scope
    /// Note: this node isn't reducible; it needs a tree rewrite to work
    /// See GlobalsRewriter
    /// 
    /// TODO: move to Microsoft.Scripting !!!
    /// </summary>
    public sealed class GlobalVariableExpression : Expression {
        private readonly string _name;
        private readonly bool _local;

        internal GlobalVariableExpression(Annotations annotations, Type type, string name, bool local)
            : base(annotations, ExpressionType.Extension, type) {
            Debug.Assert(type != typeof(void));

            _name = name;
            _local = local;
        }

        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// If using dynamic lookup, indicates that the variable should be
        /// looked up in the innermost Scope rather than the top level scope
        /// 
        /// TODO: Python specific, can it be removed?
        /// </summary>
        public bool IsLocal {
            get { return _local; }
        }

        // TODO: Remove? Useful for debugging
        public override string ToString() {
            return string.Format("Global {0} {1}", Type.Name, _name);
        }
    }

    /// <summary>
    /// TODO: move to Microsoft.Scripting !!!
    /// </summary>
    public partial class Expression {
        public static GlobalVariableExpression Global(Type type, string name) {
            return Global(type, name, false, Annotations.Empty);
        }

        public static GlobalVariableExpression Global(Type type, string name, bool local) {
            return Global(type, name, local, Annotations.Empty);
        }

        public static GlobalVariableExpression Global(Type type, string name, bool local, Annotations annotations) {
            return new GlobalVariableExpression(annotations, GetNonVoidType(type), name, local);
        }
    }
}
