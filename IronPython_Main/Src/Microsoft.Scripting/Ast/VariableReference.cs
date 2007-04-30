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
    /// <summary>
    /// Reference is representation of the use(s) of the variable (_variable).
    /// It is used to resolve closures and create actual slots for the code generation using
    /// the information stored in the Binding.
    /// </summary>
    public class VariableReference {
        private readonly SymbolId _name;
        private Variable _variable;
        private Slot _slot;

        private Type _knownType;

        public VariableReference(SymbolId name) {
            _name = name;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public Type Type {
            get {
                // Variables dynamically resolved at runtime can be of any time,
                // therefore typeof(object)
                return _variable != null ? _variable.Type : typeof(object);
            }
        }

        /// <summary>
        /// The referenced variable. For references dynamically resolved at runtime,
        /// the variable is null.
        /// </summary>
        public Variable Variable {
            get { return _variable; }
            set { _variable = value; }
        }

        public Type KnownType {
            get { return _knownType; }
            set { _knownType = value; }
        }

        public Slot Slot {
            get { return _slot; }
        }

        public void CreateSlot(CodeGen cg) {
            if (_variable == null) {
                // Unbound variable, lookup dynamically at runtime
                _slot = new DynamicLookupSlot(_name);
            } else {
                _slot = _variable.CreateSlot(cg);
            }
        }

        public static Expression[] ReferencesToExpressions(VariableReference[] parameters) {
            Expression[] exprs = new Expression[parameters.Length];
            for (int i = 0; i < exprs.Length; i++) {
                exprs[i] = BoundExpression.Defined(parameters[i]);
            }
            return exprs;
        }
    }
}
