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
    public class UnboundExpression : Expression {
        private readonly SymbolId _name;

        public UnboundExpression(SymbolId name)
            : this(name, SourceSpan.None) {
        }

        public UnboundExpression(SymbolId name, SourceSpan span)
            : base(span) {
            _name = name;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public override Type ExpressionType {
            get {
                return typeof(object);
            }
        }

        public override object Evaluate(CodeContext context) {
            return RuntimeHelpers.LookupName(context, _name);
        }

        public override void Emit(CodeGen cg) {
            // RuntimeHelpers.LookupName(CodeContext, name)
            cg.EmitCodeContext();
            cg.EmitSymbolId(_name);
            cg.EmitCall(typeof(RuntimeHelpers), "LookupName");
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }
    }
}
