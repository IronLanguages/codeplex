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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class VoidExpression : Expression {
        private Statement _statement;

        internal VoidExpression(SourceSpan span, Statement statement)
            : base(span) {
            if (statement == null) {
                throw new ArgumentNullException("statement");
            }
            _statement = statement;
        }

        public override Type ExpressionType {
            get {
                return typeof(void);
            }
        }

        public Statement Statement {
            get { return _statement; }
        }

        public override void Emit(CodeGen cg) {
            _statement.Emit(cg);
        }

        public override object Evaluate(CodeContext context) {
            _statement.Execute(context);
            return null;
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _statement.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    /// <summary>
    /// Factory methods
    /// </summary>
    public static partial class Ast {
        public static VoidExpression Void(Statement statement) {
            return Void(SourceSpan.None, statement);
        }

        public static VoidExpression Void(SourceSpan span, Statement statement) {
            return new VoidExpression(span, statement);
        }
    }
}