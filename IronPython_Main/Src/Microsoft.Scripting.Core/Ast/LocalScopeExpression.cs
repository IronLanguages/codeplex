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
    /// An expression that provides run-time read/write access to variables in
    /// the local scope. Needed to implement "eval" in dynamic languages.
    /// Evaluates to an instance of ILocalVariables at run time.
    /// 
    /// TODO: rename !!!
    /// </summary>
    public sealed class LocalScopeExpression : Expression {
        private readonly bool _isClosure;
        private readonly ReadOnlyCollection<Expression> _variables;

        internal LocalScopeExpression(
            Annotations annotations,
            bool isClosure,
            ReadOnlyCollection<Expression> variables)
            : base(annotations, ExpressionType.LocalScope, typeof(ILocalVariables)) {

            _isClosure = isClosure;
            _variables = variables;
        }

        /// <summary>
        /// If true, if this will cause all variables it references to be
        /// hoisted to the lambda's closure.
        /// 
        /// If false, only variables that are closed over already (because
        /// they are referenced in a nested lambda) will be returned.
        /// 
        /// TODO: Can this be removed? Currently it exists for perf with
        /// CodeContext, but that is probably not needed (language is better,
        /// than us for figuring out the smallest set of variables to hoist)
        /// Also, Python's func_closure needs this. Again can be fixed without
        /// needing this feature. (Once it's removed, it will always have the
        /// IsClosure == true behavior)
        /// </summary>
        public bool IsClosure {
            get { return _isClosure; }
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

        // TODO: rename or remove !!!
        public static LocalScopeExpression LiftedVariables(params Expression[] variables) {
            return LiftedVariables(Annotations.Empty, (IEnumerable<Expression>)variables);
        }
        // TODO: rename or remove !!!
        public static LocalScopeExpression LiftedVariables(IEnumerable<Expression> variables) {
            return LiftedVariables(Annotations.Empty, variables);
        }
        // TODO: rename or remove !!!
        public static LocalScopeExpression LiftedVariables(Annotations annotations, params Expression[] variables) {
            return LiftedVariables(annotations, (IEnumerable<Expression>)variables);
        }
        // TODO: rename or remove !!!
        public static LocalScopeExpression LiftedVariables(Annotations annotations, IEnumerable<Expression> variables) {
            ContractUtils.RequiresNotNull(annotations, "annotations");
            ContractUtils.RequiresNotNull(variables, "variables");

            ReadOnlyCollection<Expression> vars = CollectionUtils.ToReadOnlyCollection(variables);
            for (int i = 0; i < vars.Count; i++) {
                Expression v = vars[i];
                if (v == null) {
                    throw ExceptionUtils.MakeArgumentItemNullException(i, "variables");
                }
                ExpressionType kind = vars[i].NodeType;
                if (kind != ExpressionType.Variable && kind != ExpressionType.Parameter) {
                    throw new ArgumentException("elements must be variables or parameters", "variables");
                }
            }

            return new LocalScopeExpression(annotations, false, vars);
        }

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

            ReadOnlyCollection<Expression> vars = CollectionUtils.ToReadOnlyCollection(variables);
            for (int i = 0; i < vars.Count; i++) {
                Expression v = vars[i];
                if (v == null) {
                    throw ExceptionUtils.MakeArgumentItemNullException(i, "variables");
                }
                ExpressionType kind = vars[i].NodeType;
                if (kind != ExpressionType.Variable && kind != ExpressionType.Parameter) {
                    throw new ArgumentException("elements must be variables or parameters", "variables");
                }
            }

            return new LocalScopeExpression(annotations, true, vars);
        }
    }
}
