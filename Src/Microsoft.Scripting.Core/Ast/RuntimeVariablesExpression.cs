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
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    /// <summary>
    /// An expression that provides runtime read/write access to variables.
    /// Needed to implement "eval" in dynamic languages.
    /// Evaluates to an instance of ILocalVariables at run time.
    /// </summary>
    public sealed class RuntimeVariablesExpression : Expression {
        private readonly ReadOnlyCollection<ParameterExpression> _variables;

        internal RuntimeVariablesExpression(
            Annotations annotations,
            ReadOnlyCollection<ParameterExpression> variables)
            : base(annotations) {
            _variables = variables;
        }

        protected override Type GetExpressionType() {
            return typeof(IRuntimeVariables);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.LocalScope;
        }

        /// <summary>
        /// The variables or parameters to provide access to
        /// </summary>
        public ReadOnlyCollection<ParameterExpression> Variables {
            get { return _variables; }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitRuntimeVariables(this);
        }
    }

    public partial class Expression {

        // TODO: rename to RuntimeVariables

        public static RuntimeVariablesExpression AllVariables(params ParameterExpression[] variables) {
            return AllVariables(Annotations.Empty, (IEnumerable<ParameterExpression>)variables);
        }
        public static RuntimeVariablesExpression AllVariables(IEnumerable<ParameterExpression> variables) {
            return AllVariables(Annotations.Empty, variables);
        }
        public static RuntimeVariablesExpression AllVariables(Annotations annotations, params ParameterExpression[] variables) {
            return AllVariables(annotations, (IEnumerable<ParameterExpression>)variables);
        }
        public static RuntimeVariablesExpression AllVariables(Annotations annotations, IEnumerable<ParameterExpression> variables) {
            ContractUtils.RequiresNotNull(variables, "variables");

            var vars = variables.ToReadOnly();
            for (int i = 0; i < vars.Count; i++) {
                Expression v = vars[i];
                if (v == null) {
                    throw new ArgumentNullException("variables[" + i + "]");
                }
            }

            return new RuntimeVariablesExpression(annotations, vars);
        }
    }
}
