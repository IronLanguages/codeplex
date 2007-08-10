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

        internal UnboundExpression(SourceSpan span, SymbolId name)
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

        protected override object DoEvaluate(CodeContext context) {
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

    /// <summary>
    /// Factory methods
    /// </summary>
    public static partial class Ast {
        public static UnboundExpression Read(SymbolId name) {
            return Read(SourceSpan.None, name);
        }

        public static UnboundExpression Read(SourceSpan span, SymbolId name) {
            if (name.IsInvalid || name.IsEmpty) {
                throw new ArgumentException("Invalid or empty name is not allowed");
            }

            return new UnboundExpression(span, name);
        }
    }
}
