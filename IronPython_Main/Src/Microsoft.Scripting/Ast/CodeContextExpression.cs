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
    public class CodeContextExpression : Expression {

        public CodeContextExpression() {
        }

        public CodeContextExpression(SourceSpan span)
            : base(span) {
        }

        public override Type ExpressionType {
            get {
                return typeof(CodeContext);
            }
        }

        public override object Evaluate(CodeContext context) {
            return context;
        }

        public override void EmitAs(CodeGen cg, Type asType) {
            cg.EmitCodeContext();
            cg.EmitConvert(ExpressionType, asType);
        }

        public override void Emit(CodeGen cg) {
            EmitAs(cg, typeof(object));
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }
    }
}
