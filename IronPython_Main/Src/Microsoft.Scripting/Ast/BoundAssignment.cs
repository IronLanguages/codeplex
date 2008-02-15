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
using System.Reflection;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class BoundAssignment : Expression {
        private readonly Variable /*!*/ _variable;
        private readonly Expression /*!*/ _value;

        internal BoundAssignment(Variable /*!*/ variable, Expression /*!*/ value)
            : base(AstNodeType.BoundAssignment, variable.Type) {
            _variable = variable;
            _value = value;
        }

        public Variable Variable {
            get { return _variable; }
        }

        public Expression Value {
            get { return _value; }
        }
    }

    public static partial class Ast {
        /// <summary>
        /// Performs an assignment variable = value
        /// </summary>
        public static Expression Write(Variable variable, Variable value) {
            return Assign(variable, Ast.Read(value));
        }

        /// <summary>
        /// Performs an assignment variable = value
        /// </summary>
        public static Expression Write(Variable variable, Expression value) {
            return Assign(variable, value);
        }

        /// <summary>
        /// Performs an assignment variable.field = value
        /// </summary>
        public static Expression Write(Variable variable, FieldInfo field, Expression value) {
            return AssignField(Read(variable), field, value);
        }

        /// <summary>
        /// Performs an assignment variable.field = value
        /// </summary>
        public static Expression Write(Variable variable, FieldInfo field, Variable value) {
            return AssignField(Read(variable), field, Read(value));
        }

        /// <summary>
        /// Performs an assignment variable = right.field
        /// </summary>
        public static Expression Write(Variable variable, Variable right, FieldInfo field) {
            return Assign(variable, ReadField(Read(right), field));
        }

        /// <summary>
        /// Performs an assignment variable.leftField = right.rightField
        /// </summary>
        public static Expression Write(Variable variable, FieldInfo leftField, Variable right, FieldInfo rightField) {
            return AssignField(Read(variable), leftField, ReadField(Read(right), rightField));
        }

        /// <summary>
        /// Performs an assignment variable = value
        /// </summary>
        public static BoundAssignment Assign(Variable variable, Expression value) {
            Contract.RequiresNotNull(variable, "variable");
            Contract.RequiresNotNull(value, "value");
            Contract.Requires(TypeUtils.CanAssign(variable.Type, value));
            return new BoundAssignment(variable, value);
        }
    }
}
