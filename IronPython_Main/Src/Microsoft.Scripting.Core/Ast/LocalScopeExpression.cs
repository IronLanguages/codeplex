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
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// An expression that provides runtime read/write access to variables.
    /// Needed to implement "eval" in dynamic languages.
    /// Evaluates to an instance of ILocalVariables at run time.
    /// 
    /// TODO: rename !!!
    /// </summary>
    public sealed class LocalScopeExpression : Expression {
        private readonly ReadOnlyCollection<Expression> _variables;

        internal LocalScopeExpression(
            Annotations annotations,
            ReadOnlyCollection<Expression> variables)
            : base(annotations, ExpressionType.LocalScope, typeof(ILocalVariables)) {
            _variables = variables;
        }

        /// <summary>
        /// The variables or parameters to provide access to
        /// 
        /// TODO: should VariableExpressions and ParameterExpressions be two
        ///       seperate properties?
        /// </summary>
        public ReadOnlyCollection<Expression> Variables {
            get { return _variables; }
        }
    }

    public partial class Expression {

        // TODO: rename !!!
        //
        //   LocalScope
        //   ScopeAccess
        //   EvaluateVariables
        //   AccessVariables
        //   HoistVariables
        //   ...

        public static LocalScopeExpression AllVariables(params Expression[] variables) {
            return AllVariables(Annotations.Empty, (IEnumerable<Expression>)variables);
        }
        public static LocalScopeExpression AllVariables(IEnumerable<Expression> variables) {
            return AllVariables(Annotations.Empty, variables);
        }
        public static LocalScopeExpression AllVariables(Annotations annotations, params Expression[] variables) {
            return AllVariables(annotations, (IEnumerable<Expression>)variables);
        }
        public static LocalScopeExpression AllVariables(Annotations annotations, IEnumerable<Expression> variables) {
            ContractUtils.RequiresNotNull(annotations, "annotations");
            ContractUtils.RequiresNotNull(variables, "variables");

            ReadOnlyCollection<Expression> vars = variables.ToReadOnly();
            for (int i = 0; i < vars.Count; i++) {
                Expression v = vars[i];
                if (v == null) {
                    throw ExceptionUtils.MakeArgumentItemNullException(i, "variables");
                }
                ExpressionType kind = vars[i].NodeType;
                if (kind != ExpressionType.Variable && kind != ExpressionType.Parameter) {
                    throw Error.MustBeVariableOrParameter("variables");
                }
            }

            return new LocalScopeExpression(annotations, vars);
        }
    }
}
