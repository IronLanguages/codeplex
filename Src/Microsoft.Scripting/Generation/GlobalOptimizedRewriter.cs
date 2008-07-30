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
using System.Linq.Expressions.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Generation {

    internal abstract class GlobalOptimizedRewriter : GlobalRewriter {
        private readonly Dictionary<GlobalVariableExpression, Expression> _mapToExpression = new Dictionary<GlobalVariableExpression, Expression>();
        private readonly Dictionary<string, GlobalVariableExpression> _globalNames = new Dictionary<string, GlobalVariableExpression>();

        protected abstract Expression MakeWrapper(GlobalVariableExpression variable);

        protected override Expression RewriteGet(GlobalVariableExpression node) {
            return Expression.ConvertHelper(MapToExpression(node), node.Type);
        }

        protected override Expression RewriteSet(AssignmentExpression node) {
            GlobalVariableExpression lvalue = (GlobalVariableExpression)node.Expression;

            return Expression.ConvertHelper(
                Expression.Assign(
                    MapToExpression(lvalue),
                    Expression.ConvertHelper(VisitNode(node.Value), typeof(object)),
                    node.Annotations
                ),
                node.Type
            );
        }

        protected Expression MapToExpression(GlobalVariableExpression variable) {
            Expression result;
            if (_mapToExpression.TryGetValue(variable, out result)) {
                return result;
            }

            EnsureUniqueName(_globalNames, variable);

            result = Expression.Property(
                MakeWrapper(variable),
                typeof(ModuleGlobalWrapper).GetProperty("CurrentValue"),
                variable.Annotations
            );

            return _mapToExpression[variable] = result;
        }

        // TODO: Do we really need this optimization?
        // it adds complexity
        #region SymbolId rewrite support

        // TypeGen, possibly null
        protected TypeGen TypeGen { get; set; }

        // If TypeGen is non-null, we rewrite SymbolIds to static field accesses
        private readonly Dictionary<SymbolId, FieldBuilder> _indirectSymbolIds = new Dictionary<SymbolId, FieldBuilder>();

        protected override Expression VisitExtension(Expression node) {
            var symbol = node as SymbolConstantExpression;
            if (symbol != null && TypeGen != null) {
                return GetSymbolExpression(symbol.Value);
            }
            return base.VisitExtension(node);
        }

        protected void EmitSymbolId(ILGen cg, SymbolId id) {
            cg.Emit(OpCodes.Ldsfld, GetSymbolField(id));
        }

        private Expression GetSymbolExpression(SymbolId id) {
            return Expression.Field(null, GetSymbolField(id));
        }

        private FieldInfo GetSymbolField(SymbolId id) {
            Debug.Assert(TypeGen != null);

            if (id == SymbolId.Empty) {
                return typeof(SymbolId).GetField("Empty");
            }
            FieldBuilder value;
            if (!_indirectSymbolIds.TryGetValue(id, out value)) {
                // create field, emit fix-up...

                value = TypeGen.AddStaticField(typeof(SymbolId), FieldAttributes.Public, "symbol_" + SymbolTable.IdToString(id));
                ILGen init = TypeGen.TypeInitializer;
                if (_indirectSymbolIds.Count == 0) {
                    init.EmitType(TypeGen.TypeBuilder);
                    init.EmitCall(typeof(RuntimeHelpers), "InitializeSymbols");
                }
                _indirectSymbolIds[id] = value;
            }
            return value;
        }

        #endregion
    }
}
