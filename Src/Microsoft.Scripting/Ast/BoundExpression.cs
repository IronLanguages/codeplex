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
using System.Diagnostics;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class BoundExpression : Expression {
        private readonly Variable _variable;
        private bool _defined;

        // Implementation detail
        private VariableReference _vr;

        public BoundExpression(Variable variable)
            : this(variable, SourceSpan.None) {
        }

        public BoundExpression(Variable variable, SourceSpan span)
            : base(span) {
            if (variable == null) {
                throw new ArgumentNullException("variable");
            }
            _variable = variable;
        }

        public Variable Variable {
            get { return _variable; }
        }

        internal VariableReference Ref {
            get { return _vr; }
            set {
                Debug.Assert(value.Variable == _variable);
                // the _vr == value is true for DAGs
                Debug.Assert(_vr == null || _vr == value);
                _vr = value;
            }
        }

        public SymbolId Name {
            get { return _variable.Name; }
        }

        public bool IsDefined {
            get { return _defined; }
            set { _defined = value; }
        }

        public override Type ExpressionType {
            get { return _variable.Type; }
        }

        public override string ToString() {
            return "BoundExpression : " + SymbolTable.IdToString(Name);
        }

        public override object Evaluate(CodeContext context) {
            return context.LanguageContext.LookupName(context, Name);
        }

        public override AbstractValue AbstractEvaluate(AbstractContext context) {
            return context.Lookup(_variable);
        }

        public override void EmitAddress(CodeGen cg, Type asType) {
            if (asType == ExpressionType) {
                _vr.Slot.EmitGetAddr(cg);
            } else {
                base.EmitAddress(cg, asType);
            }
        }

        public override void Emit(CodeGen cg) {
            // Do not emit CheckInitialized for variables that are defined, or for temp variables.
            // Only emit CheckInitialized for variables of type object
            bool check = !_defined && !_variable.IsTemporary && _variable.Type == typeof(object);
            cg.EmitGet(_vr.Slot, Name, check);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }

        public static BoundExpression Defined(Variable variable) {
            BoundExpression ret = new BoundExpression(variable);
            ret.IsDefined = true;
            return ret;
        }
    }
}
