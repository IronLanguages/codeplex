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
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Threading;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Generation {

    /// <summary>
    /// Serializes constants and dynamic sites so the code can be saved to disk
    /// </summary>
    internal sealed class ToDiskRewriter : ExpressionVisitor {
        private static int _uniqueNameId;
        private List<Expression> _constants;
        private ParameterExpression _constantPool;
        private Dictionary<Type, Type> _delegateTypes;
        private int _depth;
        private readonly TypeGen _typeGen;

        internal ToDiskRewriter(TypeGen typeGen) {
            _typeGen = typeGen;
        }

        public LambdaExpression RewriteLambda(LambdaExpression lambda) {
            return (LambdaExpression)Visit(lambda);
        }

        protected override Expression VisitLambda<T>(Expression<T> node) {
            _depth++;
            try {

                // Visit the lambda first, so we walk the tree and find any
                // constants we need to rewrite.
                node = (Expression<T>)base.VisitLambda(node);

                if (_depth != 1) {
                    return node;
                }

                var body = node.Body;

                if (_constants != null) {
                    // Rewrite the constants, they can contain embedded
                    // CodeContextExpressions
                    for (int i = 0; i < _constants.Count; i++) {
                        _constants[i] = Visit(_constants[i]);
                    }

                    // Add the consant pool variable to the top lambda
                    body = AstUtils.AddScopedVariable(
                        body,
                        _constantPool,
                        Expression.NewArrayInit(typeof(object), _constants)
                    );
                }

                // Rewrite the lambda
                return Expression.Lambda<T>(
                    body,
                    node.Name + "$" + Interlocked.Increment(ref _uniqueNameId),
                    node.Parameters
                );

            } finally {
                _depth--;
            }
        }

        protected override Expression VisitExtension(Expression node) {
            if (node.NodeType == ExpressionType.Dynamic) {
                // the node was dynamic, the dynamic nodes were removed,
                // we now need to rewrite any call sites.
                return VisitDynamic((DynamicExpression)node);
            }

            return base.VisitExtension(node);
        }

        protected override Expression VisitConstant(ConstantExpression node) {
            var site = node.Value as CallSite;
            if (site != null) {
                return RewriteCallSite(site);
            }

            var exprSerializable = node.Value as IExpressionSerializable;
            if (exprSerializable != null) {
                return Visit(exprSerializable.CreateExpression());
            }

            var symbols = node.Value as SymbolId[];
            if (symbols != null) {
                return Expression.NewArrayInit(
                     typeof(SymbolId),
                     new ReadOnlyCollection<Expression>(
                         symbols.Map(s => SymbolConstantExpression.GetExpression(s))
                     )
                 );
            }

            return base.VisitConstant(node);
        }

        // If the DynamicExpression uses a transient (in-memory) type for its
        // delegate, we need to replace it with a new delegate type that can be
        // saved to disk
        protected override Expression VisitDynamic(DynamicExpression node) {
            Type delegateType;
            if (RewriteDelegate(node.DelegateType, out delegateType)) {
                node = Expression.MakeDynamic(delegateType, node.Binder, node.Arguments);
            }

            // Reduce dynamic expression so that the lambda can be emitted as a non-dynamic method.
            return Visit(CompilerHelpers.Reduce(node));
        }

        private bool RewriteDelegate(Type delegateType, out Type newDelegateType) {
            if (!ShouldRewriteDelegate(delegateType)) {
                newDelegateType = null;
                return false;
            }

            if (_delegateTypes == null) {
                _delegateTypes = new Dictionary<Type, Type>();
            }

            // TODO: should caching move to AssemblyGen?
            if (!_delegateTypes.TryGetValue(delegateType, out newDelegateType)) {
                MethodInfo invoke = delegateType.GetMethod("Invoke");

                newDelegateType = _typeGen.AssemblyGen.MakeDelegateType(
                    delegateType.Name,
                    invoke.GetParameters().Map(p => p.ParameterType),
                    invoke.ReturnType
                );

                _delegateTypes[delegateType] = newDelegateType;
            }

            return true;
        }

        private bool ShouldRewriteDelegate(Type delegateType) {
            // We need to replace a transient delegateType with one stored in
            // the assembly we're saving to disk.
            //
            // One complication:
            // SaveAssemblies mode prevents us from detecting the module as
            // transient. If that option is turned on, always replace delegates
            // that live in another AssemblyBuilder

            var module = delegateType.Module as ModuleBuilder;
            if (module == null) {
                return false;
            }

            if (module.IsTransient()) {
                return true;
            }

            if (Snippets.Shared.SaveSnippets && module.Assembly != _typeGen.AssemblyGen.AssemblyBuilder) {
                return true;
            }

            return false;
        }

        private Expression RewriteCallSite(CallSite site) {
            IExpressionSerializable serializer = site.Binder as IExpressionSerializable;
            if (serializer == null) {
                throw Error.GenNonSerializableBinder();
            }

            // add the initialization code that we'll generate later into the outermost
            // lambda and then return an index into the array we'll be creating.
            if (_constantPool == null) {
                _constantPool = Expression.Variable(typeof(object[]), "$constantPool");
                _constants = new List<Expression>();
            }

            Type siteType = site.GetType();

            _constants.Add(Expression.Call(siteType.GetMethod("Create"), serializer.CreateExpression()));

            // rewrite the node...
            return Visit(
                AstUtils.Convert(
                    Expression.ArrayAccess(_constantPool, AstUtils.Constant(_constants.Count - 1)),
                    siteType
                )
            );
        }
    }
}
