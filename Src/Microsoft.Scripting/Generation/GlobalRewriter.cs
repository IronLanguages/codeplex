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
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {

    /// <summary>
    /// Rewrites known extension nodes into primitive ones:
    ///   * GlobalVariableExpression
    ///   * CodeContextExpression
    ///   * CodeContextScopeExpression
    ///   
    /// TODO: remove all of the functionality related to CodeContext, once
    /// Python and JS fix their scope implementations to and the CodeContext*
    /// nodes go away. All that should be left is global support, and even that
    /// can go once OptimizedModules moves into Python.
    /// </summary>
    public abstract class GlobalRewriter : ExpressionTreeVisitor {
        private Expression _context;

        // Rewrite entry points
        public Expression<DlrMainCallTarget> RewriteLambda(LambdaExpression lambda) {
            return RewriteLambda(lambda, lambda.Name);
        }

        public Expression<DlrMainCallTarget> RewriteLambda(LambdaExpression lambda, string name) {
            Debug.Assert(_context == null);
            Debug.Assert(lambda.Parameters.Count == 0);

            // Fix up the top-level lambda to have a scope and language parameters
            ParameterExpression scopeParameter = Expression.Parameter(typeof(Scope), "$scope");
            ParameterExpression languageParameter = Expression.Parameter(typeof(LanguageContext), "$language");
            VariableExpression contextVariable = Expression.Variable(typeof(CodeContext), "$globalContext");

            _context = contextVariable;
            lambda = (LambdaExpression)VisitLambda(lambda);

            return Expression.Lambda<DlrMainCallTarget>(
                AddScopedVariable(
                    lambda.Body,
                    contextVariable,
                    Expression.Call(typeof(RuntimeHelpers).GetMethod("CreateTopLevelCodeContext"), scopeParameter, languageParameter)
                ),
                name,
                lambda.Annotations,
                new[] { scopeParameter, languageParameter }
            );
        }

        protected abstract Expression RewriteGet(GlobalVariableExpression node);
        protected abstract Expression RewriteSet(AssignmentExpression node);

        #region rewriter overrides

        protected override Expression VisitExtension(Expression node) {
            GlobalVariableExpression global = node as GlobalVariableExpression;
            if (global != null) {
                return RewriteGet(global);
            }

            CodeContextExpression cc = node as CodeContextExpression;
            if (cc != null) {
                return _context;
            }

            CodeContextScopeExpression ccs = node as CodeContextScopeExpression;
            if (ccs != null) {
                return Rewrite(ccs);
            }

            // Must remove extension nodes because they could contain
            // one of the above node types. See, e.g. DeleteUnboundExpression
            return Visit(node.ReduceExtensions());
        }

        protected override Expression VisitAssignment(AssignmentExpression node) {
            Expression lvalue = node.Expression;

            GlobalVariableExpression global = lvalue as GlobalVariableExpression;
            if (global != null) {
                return RewriteSet(node);
            }

            return base.VisitAssignment(node);
        }
        
        protected override Expression VisitDynamic(DynamicExpression node) {
            Type siteType = typeof(CallSite<>).MakeGenericType(node.DelegateType);

            // Rewite call site as constant
            var siteExpr = VisitConstant(Expression.Constant(DynamicSiteHelpers.MakeSite(node.Binder, siteType)));

            // Rewrite all of the arguments
            var args = Visit(node.Arguments);

            var siteVar = Expression.Variable(siteExpr.Type, "$site");

            // ($site = siteExpr).Target.Invoke($site, *args)
            return Expression.Scope(
                Expression.Call(
                    Expression.Field(
                        Expression.Assign(siteVar, siteExpr),
                        siteType.GetField("Target")
                    ),
                    node.DelegateType.GetMethod("Invoke"),
                    ArrayUtils.Insert(siteVar, args)
                ),
                siteVar
            );
        }

        #endregion

        #region CodeContext support

        protected Expression Context {
            get { return _context; }
            set { _context = value; }
        }

        private Expression Rewrite(CodeContextScopeExpression ccs) {
            Expression saved = _context;
            VariableExpression nested = Expression.Variable(typeof(CodeContext), "$frame");

            // rewrite body with nested context
            _context = nested;
            Expression body = Visit(ccs.Body);
            _context = saved;

            // wrap the body in a scope that initializes the nested context
            return AddScopedVariable(body, nested, Visit(ccs.NewContext));
        }

        #endregion

        #region helpers

        protected static void EnsureUniqueName(IDictionary<string, GlobalVariableExpression> varsByName, GlobalVariableExpression node) {
            GlobalVariableExpression n2;
            if (varsByName.TryGetValue(node.Name, out n2)) {
                if (node == n2) {
                    return;
                }
                throw Error.GlobalsMustBeUnique();
            }

            varsByName.Add(node.Name, node);
        }

        // Helper to add a variable to a scope
        protected static Expression AddScopedVariable(Expression body, VariableExpression variable, Expression variableInit) {
            List<VariableExpression> vars = new List<VariableExpression>();
            string name = null;
            Annotations annotations = Annotations.Empty;
            while (body.NodeType == ExpressionType.Scope) {
                ScopeExpression scope = (ScopeExpression)body;
                vars.AddRange(scope.Variables);
                name = scope.Name;
                annotations = scope.Annotations;
                body = scope.Body;
            }

            vars.Add(variable);

            return Expression.Scope(
                Expression.Comma(
                    Expression.Assign(variable, variableInit),
                    body
                ),
                name,
                annotations,
                vars
            );
        }

        #endregion
    }
}

namespace Microsoft.Scripting.Runtime {
    public static partial class RuntimeHelpers {
        // emitted by GlobalRewriter
        // TODO: Python and JScript should do this
        public static CodeContext CreateTopLevelCodeContext(Scope scope, LanguageContext context) {
            context.EnsureScopeExtension(scope.ModuleScope);
            return new CodeContext(scope, context);
        }
    }
}
