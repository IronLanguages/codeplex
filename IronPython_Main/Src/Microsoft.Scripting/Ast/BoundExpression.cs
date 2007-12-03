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

using System;
using System.Diagnostics;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class BoundExpression : Expression {
        private readonly Variable /*!*/ _variable;
        private bool _defined;

        // Implementation detail
        private VariableReference _vr;

        internal BoundExpression(Variable /*!*/ variable)
            : base(AstNodeType.BoundExpression, variable.Type) {
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
            internal set { _defined = value; }
        }

        public override string ToString() {
            return "BoundExpression : " + SymbolTable.IdToString(Name);
        }

        public override AbstractValue AbstractEvaluate(AbstractContext context) {
            return context.Lookup(_variable);
        }
    }

    public static partial class Ast {
        public static BoundExpression Read(Variable variable) {
            Contract.RequiresNotNull(variable, "variable");
            return new BoundExpression(variable);
        }

        public static BoundExpression ReadDefined(Variable variable) {
            BoundExpression ret = Read(variable);
            ret.IsDefined = true;
            return ret;
        }
    }
}
