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
    public class BoundExpression : Expression {
        private readonly VariableReference _vr;
        private bool _defined;

        public BoundExpression(VariableReference vr)
            : this(vr, SourceSpan.None) {
        }

        public BoundExpression(VariableReference vr, SourceSpan span)
            : base(span) {
            if (vr == null) {
                throw new ArgumentNullException("vr");
            }
            _vr = vr;
        }

        public VariableReference Reference {
            get { return _vr; }
        }

        public SymbolId Name {
            get { return _vr.Name; }
        }

        public bool IsDefined {
            get { return _defined; }
            set { _defined = value; }
        }

        public override Type ExpressionType {
            get { return _vr.Type; }
        }

        public override string ToString() {
            return "BoundExpression : " + SymbolTable.IdToString(_vr.Name);
        }

        public override object Evaluate(CodeContext context) {
            return context.Scope.LookupName(_vr.Name);
            //return RuntimeHelpers.LookupName(context, _vr.Name);
        }

        public override void Emit(CodeGen cg) {
            EmitAs(cg, typeof(object));
        }
        
        public override void EmitAddress(CodeGen cg, Type asType) {
            if (asType == ExpressionType) {
                _vr.Slot.EmitGetAddr(cg);
            } else {
                base.EmitAddress(cg, asType);
            }
        }

        public override void EmitAs(CodeGen cg, Type asType) {
            // Do not emit CheckInitialized for variables that are defined, or for temp variables.
            // Only emit CheckInitialized for variables of type object
            bool check = !_defined && !(_vr.Variable != null && _vr.Variable.IsTemporary) && ExpressionType == typeof(object);
            cg.EmitGet(_vr.Slot, _vr.Name, check);
            cg.EmitConvert(ExpressionType, asType);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }

        public static BoundExpression Defined(VariableReference vr) {
            BoundExpression ret = new BoundExpression(vr);
            ret.IsDefined = true;
            return ret;
        }
    }
}
