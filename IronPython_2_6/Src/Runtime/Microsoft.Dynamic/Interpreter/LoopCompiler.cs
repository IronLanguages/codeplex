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

#if !CLR2
using System.Linq.Expressions;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using LoopFunc = Func<object[], StrongBox<object>[], InterpretedFrame, int>;

    internal sealed class LoopCompiler : ExpressionVisitor {
        private struct LoopVariable {
            public ExpressionAccess Access;

            // a variable that holds on the strong box for closure variables:
            public ParameterExpression BoxStorage;

            public LoopVariable(ExpressionAccess access, ParameterExpression box) {
                Access = access;
                BoxStorage = box;
            }

            public override string ToString() {
                return Access.ToString() + " " + BoxStorage;
            }
        }

        private readonly ParameterExpression _frameDataVar;
        private readonly ParameterExpression _frameClosureVar;
        private readonly ParameterExpression _frameVar;
        private readonly LabelTarget _returnLabel;
        private readonly LocalVariables _locals;
        private readonly LoopExpression _loop;
        private ReadOnlyCollectionBuilder<ParameterExpression> _temps;
        private readonly Dictionary<ParameterExpression, LoopVariable> _loopVariables;

        private readonly Dictionary<LabelTarget, BranchLabel> _labelMapping;
        private readonly int _loopStartInstructionIndex;
        private readonly int _loopEndInstructionIndex;
        
        internal LoopCompiler(LoopExpression loop, Dictionary<LabelTarget, BranchLabel> labelMapping, LocalVariables locals,
            int loopStartInstructionIndex, int loopEndInstructionIndex) {
            _loop = loop;
            _locals = locals;
            _frameDataVar = Expression.Parameter(typeof(object[]));
            _frameClosureVar = Expression.Parameter(typeof(StrongBox<object>[]));
            _frameVar = Expression.Parameter(typeof(InterpretedFrame));
            _loopVariables = new Dictionary<ParameterExpression, LoopVariable>();
            _returnLabel = Expression.Label(typeof(int));
            _labelMapping = labelMapping;
            _loopStartInstructionIndex = loopStartInstructionIndex;
            _loopEndInstructionIndex = loopEndInstructionIndex;
        }

        internal LoopFunc CreateDelegate() {
            var loop = (LoopExpression)Visit(_loop);
            var body = new ReadOnlyCollectionBuilder<Expression>();
            var finallyClause = new ReadOnlyCollectionBuilder<Expression>();

            foreach (var variable in _loopVariables) {
                LocalVariable local = _locals[variable.Key];
                Expression elemRef = local.LoadFromArray(_frameDataVar, _frameClosureVar);

                if (local.InClosureOrBoxed) {
                    var box = variable.Value.BoxStorage;
                    Debug.Assert(box != null);
                    body.Add(Expression.Assign(box, elemRef));
                    AddTemp(box);
                } else {
                    // Always initialize the variable even if it is only written to.
                    // If a write-only variable is actually not assigned during execution of the loop we will still write some value back.
                    // This value must be the original value, which we assign at entry.
                    body.Add(Expression.Assign(variable.Key, AstUtils.Convert(elemRef, variable.Key.Type)));

                    if ((variable.Value.Access & ExpressionAccess.Write) != 0) {
                        finallyClause.Add(Expression.Assign(elemRef, AstUtils.Box(variable.Key)));
                    }

                    AddTemp(variable.Key);
                }
            }

            if (finallyClause.Count > 0) {
                body.Add(Expression.TryFinally(loop, Expression.Block(finallyClause)));
            } else {
                body.Add(loop);
            }

            body.Add(Expression.Label(_returnLabel, Expression.Constant(_loopEndInstructionIndex - _loopStartInstructionIndex)));

            var lambda = Expression.Lambda<LoopFunc>(
                _temps != null ? Expression.Block(_temps.ToReadOnlyCollection(), body) : Expression.Block(body),
                new[] { _frameDataVar, _frameClosureVar, _frameVar }
            );
            return lambda.Compile();
        }

        protected override Expression VisitExtension(Expression node) {
            // Reduce extensions before we visit them so that we operate on a plain DLR tree,
            // where we know relationships among the nodes (which nodes represent write context etc.).
            if (node.CanReduce) {
                return Visit(node.Reduce());
            }

            return base.VisitExtension(node);
        }

        #region Gotos

        protected override Expression VisitGoto(GotoExpression node) {
            BranchLabel label;

            var target = node.Target;
            var value = Visit(node.Value);
            
            // TODO: Is it possible for an inner reducible node of the loop to rely on nodes produced by reducing outer reducible nodes? 

            // Unknown label => must be within the loop:
            if (!_labelMapping.TryGetValue(target, out label)) {
                return node.Update(target, value);
            }

            // Known label within the loop:
            if (label.TargetIndex >= _loopStartInstructionIndex && label.TargetIndex < _loopEndInstructionIndex) {
                return node.Update(target, value);
            }

            return Expression.Return(_returnLabel, 
                (value != null) ?
                    Expression.Call(_frameVar, InterpretedFrame.GotoMethod, Expression.Constant(label.LabelIndex), AstUtils.Box(value)) :
                    Expression.Call(_frameVar, InterpretedFrame.VoidGotoMethod, Expression.Constant(label.LabelIndex)),
                node.Type
           );
        }

        #endregion

        #region Local Variables

        // Gather all outer variables accessed in the loop.
        // Determines which ones are read from and written to. 
        // We will consider a variable as "read" if it is read anywhere in the loop even though 
        // the first operation might actually always be "write". We could do better if we had CFG.

        protected override Expression VisitBlock(BlockExpression node) {
            var result = (BlockExpression)base.VisitBlock(node);

            // TODO: variable shadowing
            // block(x) { block(x) { ... x ... } block(x) { ... x ... } }

            // variables declared by the block are not outer variables:
            foreach (var local in result.Variables) {
                _loopVariables.Remove(local);
            }

            return result;
        }

        protected override Expression VisitLambda<T>(Expression<T> node) {
            var result = (Expression<T>)base.VisitLambda<T>(node);

            // TODO: variable shadowing
            // block(x) { lambda(x) { ... x ... } }

            // variables declared by the block are not outer variables:
            foreach (var local in result.Parameters) {
                _loopVariables.Remove(local);
            }

            return result;
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            // reduce compound assignments:
            if (node.CanReduce) {
                return Visit(node.Reduce());
            }
            Debug.Assert(!node.NodeType.IsReadWriteAssignment());

            var param = node.Left as ParameterExpression;
            if (param != null && node.NodeType == ExpressionType.Assign) {
                var left = VisitVariable(param, ExpressionAccess.Write);
                var right = Visit(node.Right);

                // left parameter is a boxed variable:
                if (left.Type != param.Type) {
                    Debug.Assert(left.Type == typeof(object));

                    Expression rightVar;
                    if (right.NodeType != ExpressionType.Parameter) {
                        // { left.Value = (object)(rightVar = right), rightVar }
                        rightVar = AddTemp(Expression.Parameter(right.Type));
                        right = Expression.Assign(rightVar, right);
                    } else {
                        // { left.Value = (object)right, right }
                        rightVar = right;
                    }

                    return Expression.Block(
                        node.Update(left, Expression.Convert(right, left.Type)),
                        rightVar
                    );
                } else {
                    return node.Update(left, right);
                }

            } else {
                return base.VisitBinary(node);
            }
        }

        protected override Expression VisitUnary(UnaryExpression node) {
            // reduce inplace increment/decrement:
            if (node.CanReduce) {
                return Visit(node.Reduce());
            }
            Debug.Assert(!node.NodeType.IsReadWriteAssignment());
            return base.VisitUnary(node);
        }

        // TODO: if we supported ref/out parameter we would need to override 
        // MethodCallExpression, VisitDynamic and VisitNew

        protected override Expression VisitParameter(ParameterExpression node) {
            return VisitVariable(node, ExpressionAccess.Read);
        }

        private Expression VisitVariable(ParameterExpression node, ExpressionAccess access) {
            ParameterExpression box;
            LoopVariable existing;
            LocalVariable loc;

            if (_loopVariables.TryGetValue(node, out existing)) {
                box = existing.BoxStorage;
                _loopVariables[node] = new LoopVariable(existing.Access | access, box);
            } else if (_locals.TryGetLocal(node, out loc)) {
                box = loc.InClosureOrBoxed ? Expression.Parameter(typeof(StrongBox<object>), node.Name) : null;
                _loopVariables[node] = new LoopVariable(access, box);
            } else {
                // node is a variable defined in a nested lambda -> skip
                return node;
            }

            if (box != null) {
                if ((access & ExpressionAccess.Write) != 0) {
                    // compound assignments were reduced:
                    Debug.Assert((access & ExpressionAccess.Read) == 0);

                    // box.Value = (object)rhs
                    return LightCompiler.Unbox(box);
                } else {
                    // (T)box.Value
                    return Expression.Convert(LightCompiler.Unbox(box), node.Type);
                }
            }
            
            return node;
        }

        private ParameterExpression AddTemp(ParameterExpression variable) {
            if (_temps == null) {
                _temps = new ReadOnlyCollectionBuilder<ParameterExpression>();
            }

            _temps.Add(variable);
            return variable;
        }

        #endregion
    }
}
