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
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {

    /// <summary>
    /// Serializes constants and dynamic sites so the code can be saved to disk
    /// </summary>
    internal sealed class ToDiskRewriter : GlobalArrayRewriter {
        private List<Expression> _constants;
        private VariableExpression _constantPool;
        private Dictionary<Type, Type> _delegateTypes;
        private int _depth;

        internal ToDiskRewriter(TypeGen typeGen) {
            TypeGen = typeGen;
        }

        protected override Expression Visit(LambdaExpression node) {
            _depth++;
            try {

                // Visit the lambda first, so we walk the tree and find any
                // constants we need to rewrite.
                node = (LambdaExpression)base.Visit(node);

                // Only rewrite if we have constants and this is the top lambda
                if (_constants == null || _depth != 1) {
                    return node;
                }

                // Rewrite the constants, they can contain embedded
                // CodeContextExpressions
                for (int i = 0; i < _constants.Count; i++) {
                    _constants[i] = VisitNode(_constants[i]);
                }

                // Add the consant pool variable to the top lambda
                Expression body = AddScopedVariable(
                    node.Body,
                    _constantPool,
                    Expression.NewArrayInit(typeof(object), _constants)
                );

                // Rewrite the lambda
                Debug.Assert(node.NodeType == ExpressionType.Lambda);
                return Expression.Lambda(
                    node.Type,
                    body,
                    node.Name,
                    node.Annotations,
                    node.Parameters
                );

            } finally {
                _depth--;
            }
        }

        protected override Expression VisitExtension(Expression node) {
            Expression res = base.VisitExtension(node);

            if (node.NodeType == ExpressionType.Dynamic) {
                // the node was dynamic, the dynamic nodes were removed,
                // we now need to rewrite any call sites.
                return Visit((DynamicExpression)res);
            }

            return res;
        }

        protected override Expression Visit(ConstantExpression node) {
            CallSite site = node.Value as CallSite;
            if (site != null) {
                return RewriteCallSite(site);
            }

            IExpressionSerializable exprSerializable = node.Value as IExpressionSerializable;
            if (exprSerializable != null) {
                return VisitNode(exprSerializable.CreateExpression());
            }

            return base.Visit(node);
        }

        // If the DynamicExpression uses a transient (in-memory) type for its
        // delegate, we need to replace it with a new delegate type that can be
        // saved to disk
        protected override Expression Visit(DynamicExpression node) {
            Type delegateType;
            if (RewriteDelegate(node.DelegateType, out delegateType)) {
                node = Expression.MakeDynamic(delegateType, node.Binder, node.Annotations, node.Arguments);
            }
            return base.Visit(node);
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

                newDelegateType = TypeGen.AssemblyGen.MakeDelegateType(
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

            if (Snippets.Shared.SaveSnippets && module.Assembly != TypeGen.AssemblyGen.AssemblyBuilder) {
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
            return VisitNode(
                Expression.ConvertHelper(
                    Expression.ArrayAccess(_constantPool, Expression.Constant(_constants.Count - 1)),
                    siteType
                )
            );
        }
    }
}
