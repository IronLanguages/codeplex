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
using System.Collections.Generic;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// ForestRewriter rewrites trees which contain multiple lambdas.
    /// It will do so by identifying all lambdas and rewriting each
    /// of them using AstRewriter
    /// 
    /// TODO: Consider merging this pass with ClosureBinder, or extract
    /// all Lambdas into a top level AST node so we don't have to go
    /// looking for them each time we need them.
    /// </summary>
    class ForestRewriter : Walker {
        /// <summary>
        /// List of lambdas identified in the AST
        /// </summary>
        private List<LambdaExpression> _lambdas;
        private bool _rewrite = true;           // TODO: For now, rewrite always

        #region Forest rewriter entry point

        public static void Rewrite(LambdaExpression lambda) {
            ForestRewriter fr = new ForestRewriter();

            // Collect the lambdas that need rewriting
            fr.WalkNode(lambda);

            // If any do need rewriting, rewrite the lambdas now
            if (fr._lambdas != null) {
                foreach (LambdaExpression curLambda in fr._lambdas) {
                    AstRewriter.RewriteBlock(curLambda);
                }
            }
        }

        #endregion

        // Yield statement triggers rewrite
        protected internal override void PostWalk(YieldStatement node) {
            _rewrite = true;
        }

        // We must explicitly override all derived classes of LambdaExpression to ensure that
        // we bind against this class as opposed to our base class. 
        
        // If we don't override this, then we'd bind to base.Walk(LambdaExpression) instead. 
        protected internal override bool Walk(GeneratorCodeBlock node) {
            return CommonLambda(node);
        }

        protected internal override bool Walk(LambdaExpression node) {
            return CommonLambda(node);
        }

        bool CommonLambda(LambdaExpression node) {
            // Simple stack of flags
            bool backup = _rewrite;
            // Rewrite always for now
            //_rewrite = false;

            // Walk the lambda body
            WalkNode(node.Body);

            // Save it if it needs rewriting
            if (_rewrite) {
                if (_lambdas == null) {
                    _lambdas = new List<LambdaExpression>();
                }
                _lambdas.Add(node);
            }

            // Pop from the stack
            _rewrite = backup;

            // Do not walk the body anymore
            return false;
        }
    }
}
