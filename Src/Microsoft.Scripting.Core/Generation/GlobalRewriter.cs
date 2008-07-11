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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace System.Scripting.Generation {

    /// <summary>
    /// Rewrites known extension nodes into primitive ones:
    ///   * GlobalVariableExpression
    ///   * CodeContextExpression
    ///   * CodeContextScopeExpression
    ///   
    /// TODO: move to Microsoft.Scripting, make internal
    /// 
    /// TODO: remove all of the functionality related to CodeContext, once
    /// Python and JS fix their scope implementations to and the CodeContext*
    /// nodes go away. All that should be left is global support, and even that
    /// can go once OptimizedModules moves into Python.
    /// </summary>
    public abstract class GlobalRewriter : DynamicNodeRewriter {
        private Expression _context;

        // Rewrite entry point
        public LambdaExpression/*!*/ RewriteLambda(LambdaExpression/*!*/ lambda) {
            Debug.Assert(_context == null);
            Debug.Assert(lambda.Parameters.Count == 0);

            // Fix up the top-level lambda to have a scope and language parameters
            ParameterExpression scopeParameter = Expression.Parameter(typeof(Scope), "$scope");
            ParameterExpression languageParameter = Expression.Parameter(typeof(LanguageContext), "$language");
            VariableExpression contextVariable = Expression.Variable(typeof(CodeContext), "$globalContext");

            _context = contextVariable;
            lambda = (LambdaExpression)Visit(lambda);

            lambda = Expression.Lambda<DlrMainCallTarget>(
                AddScopedVariable(
                    lambda.Body,
                    contextVariable,
                    Expression.Call(typeof(RuntimeHelpers).GetMethod("CreateTopLevelCodeContext"), scopeParameter, languageParameter)
                ),
                lambda.Name,
                lambda.Annotations,
                new ParameterExpression[] { scopeParameter, languageParameter }
            );

            return lambda;
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

            return base.VisitExtension(node);
        }

        protected override Expression Visit(AssignmentExpression node) {
            Expression lvalue = node.Expression;

            GlobalVariableExpression global = lvalue as GlobalVariableExpression;
            if (global != null) {
                return RewriteSet(node);
            }

            return base.Visit(node);
        }

        #endregion

        #region CodeContext support

        protected Expression Context {
            get { return _context; }
        }

        private Expression Rewrite(CodeContextScopeExpression ccs) {
            Expression saved = _context;
            VariableExpression nested = Expression.Variable(typeof(CodeContext), "$frame");

            // rewrite body with nested context
            _context = nested;
            Expression body = VisitNode(ccs.Body);
            _context = saved;

            // wrap the body in a scope that initializes the nested context
            return AddScopedVariable(body, nested, VisitNode(ccs.NewContext));
        }

        #endregion

        protected static void EnsureUniqueName(IDictionary<string, GlobalVariableExpression> varsByName, GlobalVariableExpression node) {
            GlobalVariableExpression n2;
            if (varsByName.TryGetValue(node.Name, out n2)) {
                if (node == n2) {
                    return;
                }
                throw new ArgumentException("global/top-level local variable names must be unique");
            }

            varsByName.Add(node.Name, node);
        }
    }
}

namespace System.Scripting.Runtime {
    public static partial class RuntimeHelpers {
        // emitted by GlobalRewriter
        // TODO: Python and JScript should do this
        public static CodeContext/*!*/ CreateTopLevelCodeContext(Scope/*!*/ scope, LanguageContext/*!*/ context) {
            context.EnsureScopeExtension(scope.ModuleScope);
            return new CodeContext(scope, context);
        }
    }
}
