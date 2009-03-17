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
using System; using Microsoft;


using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    /// <summary>
    /// Defines a scope where variables are defined. The compiler will
    /// automatically close over these variables if they're referenced in a
    /// nested LambdaExpession
    /// </summary>
    public sealed class ScopeExpression : Expression {
        private readonly string _name;
        private readonly Expression _body;
        private readonly ReadOnlyCollection<ParameterExpression> _variables;

        internal ScopeExpression(
            Expression body,
            string name,
            Annotations annotations,
            ReadOnlyCollection<ParameterExpression> variables)
            : base(ExpressionType.Scope, body.Type, annotations) {

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
        public ReadOnlyCollection<ParameterExpression> Variables {
            get { return _variables; }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitScope(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        public static ScopeExpression Scope(Expression body, params ParameterExpression[] variables) {
            return Scope(body, null, Annotations.Empty, (IEnumerable<ParameterExpression>)variables);
        }

        public static ScopeExpression Scope(Expression body, IEnumerable<ParameterExpression> variables) {
            return Scope(body, null, Annotations.Empty, variables);
        }

        public static ScopeExpression Scope(Expression body, string name, params ParameterExpression[] variables) {
            return Scope(body, name, Annotations.Empty, (IEnumerable<ParameterExpression>)variables);
        }

        public static ScopeExpression Scope(Expression body, string name, IEnumerable<ParameterExpression> variables) {
            return Scope(body, name, Annotations.Empty, variables);
        }

        public static ScopeExpression Scope(Expression body, string name, Annotations annotations, params ParameterExpression[] variables) {
            return Scope(body, name, annotations, (IEnumerable<ParameterExpression>)variables);
        }

        public static ScopeExpression Scope(Expression body, string name, Annotations annotations, IEnumerable<ParameterExpression> variables) {
            RequiresCanRead(body, "body");

            var varList = variables.ToReadOnly();
            ContractUtils.RequiresNotNullItems(varList, "variables");
            Expression.RequireVariablesNotByRef(varList, "variables");

            return new ScopeExpression(body, name, annotations, varList);
        }
    }
}