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

namespace Microsoft.Scripting.Ast {
    public class Arg : Node {
        public enum ArgumentKind {
            Simple,
            Named,
            List,
            Dictionary,
            Instance
        };

        private readonly SymbolId _name;
        private readonly Expression _expr;
        private readonly ArgumentKind _kind;

        private Arg(SymbolId name, Expression expression, ArgumentKind kind, SourceSpan span)
            : base(span) {
            _name = name;
            _expr = expression;
            _kind = kind;
        }

        public override string ToString() {
            return base.ToString() + ":" + SymbolTable.IdToString(_name);
        }

        public SymbolId Name {
            get { return _name; }
        }

        public Expression Expression {
            get { return _expr; }
        }

        public ArgumentKind Kind {
            get { return _kind; }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _expr.Walk(walker);
            }
            walker.PostWalk(this);
        }

        #region Factory methods

        public static Arg List(Expression expression) {
            return new Arg(SymbolId.Empty, expression, ArgumentKind.List, SourceSpan.None);
        }

        public static Arg List(Expression expression, SourceSpan span) {
            return new Arg(SymbolId.Empty, expression, ArgumentKind.List, span);
        }

        public static Arg Dictionary(Expression expression) {
            return new Arg(SymbolId.Empty, expression, ArgumentKind.Dictionary, SourceSpan.None);
        }

        public static Arg Dictionary(Expression expression, SourceSpan span) {
            return new Arg(SymbolId.Empty, expression, ArgumentKind.Dictionary, SourceSpan.None);
        }

        public static Arg Named(SymbolId name, Expression expression) {
            return new Arg(name, expression, ArgumentKind.Named, SourceSpan.None);
        }

        public static Arg Named(SymbolId name, Expression expression, SourceSpan span) {
            return new Arg(name, expression, ArgumentKind.Named, span);
        }

        public static Arg Simple(Expression expression) {
            return new Arg(SymbolId.Empty, expression, ArgumentKind.Simple, SourceSpan.None);
        }

        public static Arg Simple(Expression expression, SourceSpan span) {
            return new Arg(SymbolId.Empty, expression, ArgumentKind.Simple, span);
        }

        public static Arg Instance(Expression expression) {
            return new Arg(SymbolId.Empty, expression, ArgumentKind.Instance, SourceSpan.None);
        }

        public static Arg Instance(Expression expression, SourceSpan span) {
            return new Arg(SymbolId.Empty, expression, ArgumentKind.Instance, span);
        }

        #endregion
    }
}
