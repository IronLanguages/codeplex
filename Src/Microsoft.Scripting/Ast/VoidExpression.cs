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
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class VoidExpression : Expression {
        private Statement _statement;

        public VoidExpression(Statement statement) {
            if (statement == null) {
                throw new ArgumentNullException("statement");
            }
            _statement = statement;
            SetLoc(statement.Span);
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
            EmitAs(cg, typeof(object));
        }

        public override void EmitAs(CodeGen cg, Type asType) {
            if (asType != typeof(void)) {
                throw new NotSupportedException("VoidExpression can only be emitted as void");
            }

            _statement.Emit(cg);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _statement.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
