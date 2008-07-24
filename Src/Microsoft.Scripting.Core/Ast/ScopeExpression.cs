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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Scripting.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// Defines a scope where variables are defined. The compiler will
    /// automatically close over these variables if they're referenced in a
    /// nested LambdaExpession
    /// </summary>
    public sealed class ScopeExpression : Expression {
        private readonly string _name;
        private readonly Expression _body;
        private readonly ReadOnlyCollection<VariableExpression> _variables;

        internal ScopeExpression(
            Expression body,
            string name,
            Annotations annotations,
            ReadOnlyCollection<VariableExpression> variables)
            : base(ExpressionType.Scope, body.Type, annotations, null) {

            _body = body;
            _name = name;
            _variables = variables;
        }

        /// <summary>
        /// The body of the scope
        /// </summary>
        public Expression Body {
            get { return _body; }
        }

        /// <summary>
        /// The friendly name of this scope. Can be null
        /// </summary>
        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// The variables in this scope
        /// </summary>
        public ReadOnlyCollection<VariableExpression> Variables {
            get { return _variables; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        public static ScopeExpression Scope(Expression body, params VariableExpression[] variables) {
            return Scope(body, null, Annotations.Empty, (IEnumerable<VariableExpression>)variables);
        }

        public static ScopeExpression Scope(Expression body, IEnumerable<VariableExpression> variables) {
            return Scope(body, null, Annotations.Empty, variables);
        }

        public static ScopeExpression Scope(Expression body, string name, params VariableExpression[] variables) {
            return Scope(body, name, Annotations.Empty, (IEnumerable<VariableExpression>)variables);
        }

        public static ScopeExpression Scope(Expression body, string name, IEnumerable<VariableExpression> variables) {
            return Scope(body, name, Annotations.Empty, variables);
        }

        public static ScopeExpression Scope(Expression body, string name, Annotations annotations, params VariableExpression[] variables) {
            return Scope(body, name, annotations, (IEnumerable<VariableExpression>)variables);
        }

        public static ScopeExpression Scope(Expression body, string name, Annotations annotations, IEnumerable<VariableExpression> variables) {
            RequiresCanRead(body, "body");

            var varList = variables.ToReadOnly();
            ContractUtils.RequiresNotNullItems(varList, "variables");

            return new ScopeExpression(body, name, annotations, varList);
        }
    }
}
