/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Reflection.Emit;
using Microsoft.Scripting.Generation;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public enum BinaryOperators {
        Equal,
        NotEqual,
        AndAlso,
        OrElse,
        Add,
        Multiply,
    }

    public class BinaryExpression : Expression {
        private readonly Expression _left, _right;
        private readonly BinaryOperators _op;

        internal BinaryExpression(SourceSpan span, BinaryOperators op, Expression left, Expression right)
            : base(span) {
            Debug.Assert(left != null);
            Debug.Assert(right != null);

            _left = left;
            _right = right;
            _op = op;
        }

        public Expression Right {
            get { return _right; }
        }

        public Expression Left {
            get { return _left; }
        }

        public override Type ExpressionType {
            get {
                switch (_op) {
                    case BinaryOperators.Equal:
                    case BinaryOperators.NotEqual:
                    case BinaryOperators.AndAlso:
                    case BinaryOperators.OrElse:
                        return typeof(bool);

                    case BinaryOperators.Add:
                    case BinaryOperators.Multiply:
                        return typeof(int);
                    
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public BinaryOperators Operator {
            get { return _op; }
        }

        private bool EmitBranchTrue(CodeGen cg, BinaryOperators op, Label label) {
            switch (op) {
                case BinaryOperators.Equal:
                    if (_left.IsConstant(null)) {
                        _right.EmitAsObject(cg);
                        cg.Emit(OpCodes.Brfalse, label);
                    } else if (_right.IsConstant(null)) {
                        _left.EmitAsObject(cg);
                        cg.Emit(OpCodes.Brfalse, label);
                    } else {
                        _left.EmitAs(cg, GetEmitType());
                        _right.EmitAs(cg, GetEmitType());
                        cg.Emit(OpCodes.Beq, label);
                    }
                    return true;

                case BinaryOperators.NotEqual:
                    if (_left.IsConstant(null)) {
                        _right.EmitAsObject(cg);
                        cg.Emit(OpCodes.Brtrue, label);
                    } else if (_right.IsConstant(null)) {
                        _left.EmitAsObject(cg);
                        cg.Emit(OpCodes.Brtrue, label);
                    } else {
                        _left.EmitAs(cg, GetEmitType());
                        _right.EmitAs(cg, GetEmitType());
                        cg.Emit(OpCodes.Ceq);
                        cg.Emit(OpCodes.Brfalse, label);
                    }
                    return true;

                case BinaryOperators.AndAlso:
                    // if (left AND right) branch label
                    
                    // if (left) then 
                    //   if (right) branch label
                    // endif
                    Label endif = cg.DefineLabel();
                    _left.EmitBranchFalse(cg, endif);
                    _right.EmitBranchTrue(cg, label);
                    cg.MarkLabel(endif);
                    return true;

                case BinaryOperators.OrElse:
                    // if (left OR right) branch label

                    // if (left) then branch label endif
                    // if (right) then branch label endif
                    _left.EmitBranchTrue(cg, label);
                    _right.EmitBranchTrue(cg, label);
                    return true;

                case BinaryOperators.Add:
                case BinaryOperators.Multiply:
                    return false;
                   
                default:
                    throw Assert.Unreachable;
            }
        }

        private Type GetEmitType() {
            if (_op == BinaryOperators.Multiply) return typeof(int);

            return _left.ExpressionType == _right.ExpressionType ? _left.ExpressionType : typeof(object);
        }

        public override void EmitBranchFalse(CodeGen cg, Label label) {
            switch (_op) {
                case BinaryOperators.Equal:
                    EmitBranchTrue(cg, BinaryOperators.NotEqual, label);
                    break;

                case BinaryOperators.NotEqual:
                    EmitBranchTrue(cg, BinaryOperators.Equal, label);
                    break;

                case BinaryOperators.AndAlso:
                    // if NOT (left AND right) branch label

                    if (_left.IsConstant(false)) {
                        cg.Emit(OpCodes.Br, label);
                    } else {
                        if (!_left.IsConstant(true)) {
                            _left.EmitBranchFalse(cg, label);
                        }

                        if (_right.IsConstant(false)) {
                            cg.Emit(OpCodes.Br, label);
                        } else if (!_right.IsConstant(true)) {
                            _right.EmitBranchFalse(cg, label);
                        }
                    } 
                    break;

                case BinaryOperators.OrElse:
                    // if NOT left AND NOT right branch label

                    if (!_left.IsConstant(true) && !_right.IsConstant(true)) {
                        if (_left.IsConstant(false)) {
                            _right.EmitBranchFalse(cg, label);
                        } else if (_right.IsConstant(false)) {
                            _left.EmitBranchFalse(cg, label);
                        } else {
                            // if (NOT left) then 
                            //   if (NOT right) branch label
                            // endif
                            
                            Label endif = cg.DefineLabel();
                            _left.EmitBranchTrue(cg, endif);
                            _right.EmitBranchFalse(cg, label);
                            cg.MarkLabel(endif);
                        }
                    }
                    break;

                case BinaryOperators.Add:
                case BinaryOperators.Multiply:
                    base.EmitBranchFalse(cg, label);
                    return;

                default:
                    throw Assert.Unreachable;
            }
        }

        public override void EmitBranchTrue(CodeGen cg, Label label) {
            if (!EmitBranchTrue(cg, _op, label)) {
                base.EmitBranchTrue(cg, label);
            }
        }

        public override void Emit(CodeGen cg) {

            // TODO: code gen will be suboptimal for chained AndAlsos and AndAlso inside If
            if (_op == BinaryOperators.AndAlso || _op == BinaryOperators.OrElse) {
                EmitBooleanOperator(cg, _op == BinaryOperators.AndAlso);
                return;
            }

            _left.EmitAs(cg, GetEmitType());
            _right.EmitAs(cg, GetEmitType());

            switch (_op) {
                case BinaryOperators.Equal:
                    cg.Emit(OpCodes.Ceq);
                    break;

                case BinaryOperators.NotEqual:
                    cg.Emit(OpCodes.Ceq);
                    cg.EmitInt(0);
                    cg.Emit(OpCodes.Ceq);
                    break;

                case BinaryOperators.Multiply:
                    cg.Emit(OpCodes.Mul);
                    break;

                case BinaryOperators.Add:
                    cg.Emit(OpCodes.Add);
                    break;

                default:
                    throw Assert.Unreachable;
            }
        }

        private void EmitBooleanOperator(CodeGen cg, bool isAnd) {
            Label otherwise = cg.DefineLabel();
            Label endif = cg.DefineLabel();

            // if (_left) 
            _left.EmitBranchFalse(cg, otherwise);
            // then

            if (isAnd) {
                _right.EmitAs(cg, typeof(bool));
            } else {
                cg.EmitInt(1);
            }

            cg.Emit(OpCodes.Br, endif);
            // otherwise
            cg.MarkLabel(otherwise);

            if (isAnd) {
                cg.EmitInt(0);
            } else {
                _right.EmitAs(cg, typeof(bool));
            }

            // endif
            cg.MarkLabel(endif);
            return;
        }

        protected override object DoEvaluate(CodeContext context) {
            if (_op == BinaryOperators.AndAlso) {
                object ret = _left.Evaluate(context);
                return ((bool)ret) ? _right.Evaluate(context) : ret;
            } else if (_op == BinaryOperators.OrElse) {
                object ret = _left.Evaluate(context);
                return ((bool)ret) ? ret : _right.Evaluate(context);
            }

            object l = _left.Evaluate(context);
            object r = _right.Evaluate(context);
            switch (_op) {
                case BinaryOperators.Equal:
                    return RuntimeHelpers.BooleanToObject(TestEquals(l, r));

                case BinaryOperators.NotEqual:
                    return RuntimeHelpers.BooleanToObject(!TestEquals(l, r));

                case BinaryOperators.Multiply:
                    return (int)context.LanguageContext.Binder.Convert(l, typeof(int)) *
                            (int)context.LanguageContext.Binder.Convert(r, typeof(int));

                case BinaryOperators.Add:
                    return (int)context.LanguageContext.Binder.Convert(l, typeof(int)) +
                            (int)context.LanguageContext.Binder.Convert(r, typeof(int));

                default:
                    throw new NotImplementedException();
            }
        }

        private bool TestEquals(object l, object r) {
            // We don't need to go through the same type checks as the emit case,
            // since we know we're always dealing with boxed objects.
            return Object.Equals(l, r);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _left.Walk(walker);
                _right.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        public static BinaryExpression Equal(Expression left, Expression right) {
            return Binary(SourceSpan.None, BinaryOperators.Equal, left, right);
        }

        public static BinaryExpression NotEqual(Expression left, Expression right) {
            return Binary(SourceSpan.None, BinaryOperators.NotEqual, left, right);
        }

        #region Boolean Expressions

        public static Expression AndAlso(Expression left, Expression right) {
            return Binary(SourceSpan.None, BinaryOperators.AndAlso, left, right);
        }

        public static Expression AndAlso(SourceSpan span, Expression left, Expression right) {
            return Binary(span, BinaryOperators.AndAlso, left, right);
        }

        public static Expression OrElse(Expression left, Expression right) {
            return Binary(SourceSpan.None, BinaryOperators.OrElse, left, right);
        }

        public static Expression OrElse(SourceSpan span, Expression left, Expression right) {
            return Binary(span, BinaryOperators.OrElse, left, right);
        }

        #endregion

        #region Coalescing Expressions

        /// <summary>
        /// Null coalescing expression (LINQ).
        /// {result} ::= ((tmp = {_left}) == null) ? {right} : tmp
        /// '??' operator in C#.
        /// </summary>
        public static Expression Coalesce(CodeBlock currentBlock, Expression left, Expression right) {
            return CoalesceInternal(SourceSpan.None, currentBlock, left, right, null, false);
        }

        /// <summary>
        /// Null coalescing expression (LINQ).
        /// {result} ::= ((tmp = {_left}) == null) ? {right} : tmp
        /// '??' operator in C#.
        /// </summary>
        public static Expression Coalesce(SourceSpan span, CodeBlock currentBlock, Expression left, Expression right) {
            return CoalesceInternal(span, currentBlock, left, right, null, false);
        }

        /// <summary>
        /// True coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? {right} : tmp
        /// Generalized AND semantics.
        /// </summary>
        public static Expression CoalesceTrue(CodeBlock currentBlock, Expression left, Expression right, MethodInfo isTrue) {
            RequiresPredicate(isTrue);
            return CoalesceInternal(SourceSpan.None, currentBlock, left, right, isTrue, false);
        }

        /// <summary>
        /// True coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? {right} : tmp
        /// Generalized AND semantics.
        /// </summary>
        public static Expression CoalesceTrue(SourceSpan span, CodeBlock currentBlock, Expression left, Expression right, MethodInfo isTrue) {
            RequiresPredicate(isTrue);
            return CoalesceInternal(span, currentBlock, left, right, isTrue, false);
        }
        
        /// <summary>
        /// False coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? tmp : {right}
        /// Generalized OR semantics.
        /// </summary>
        public static Expression CoalesceFalse(CodeBlock currentBlock, Expression left, Expression right, MethodInfo isTrue) {
            RequiresPredicate(isTrue);
            return CoalesceInternal(SourceSpan.None, currentBlock, left, right, isTrue, true);
        }

        /// <summary>
        /// False coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? tmp : {right}
        /// Generalized OR semantics.
        /// </summary>
        public static Expression CoalesceFalse(SourceSpan span, CodeBlock currentBlock, Expression left, Expression right, MethodInfo isTrue) {
            RequiresPredicate(isTrue);
            return CoalesceInternal(span, currentBlock, left, right, isTrue, true);
        }

        private static Expression CoalesceInternal(SourceSpan span, CodeBlock currentBlock, Expression left, Expression right, MethodInfo isTrueOperator, bool isReverse) {
            if (currentBlock == null) throw new ArgumentNullException("currentBlock");
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");

            Variable tmp = currentBlock.CreateTemporaryVariable(SymbolTable.StringToId("tmp_left"), left.ExpressionType);

            Expression c;
            if (isTrueOperator != null) {
                // TODO: Optimization: take MethodInfo[] and choose an overload according to the type of the argument.
                c = Call(null, isTrueOperator, Assign(tmp, left));
            } else {
                c = Equal(Assign(tmp, left), Null());
            }

            Expression t, f;
            if (isReverse) {
                t = Read(tmp);
                f = right;
            } else {
                t = right;
                f = Read(tmp);
            }

            return Condition(span, c, t, f, true);
        }

        private static void RequiresPredicate(MethodInfo method) {
            if (method == null) throw new ArgumentNullException("method");
            Debug.Assert(ReflectionUtils.SignatureEquals(method, typeof(object), typeof(bool)));
            Debug.Assert(method.IsStatic && method.IsPublic);
        }

        #endregion

        /// <summary>
        /// Multiples two Int32 values.
        /// </summary>
        public static BinaryExpression Multiply(Expression left, Expression right) {
            if (left.ExpressionType != typeof(int) || right.ExpressionType != typeof(int)) {
                throw new NotSupportedException(String.Format("multiply only supports ints, got {0} {1}", left.ExpressionType.Name, right.ExpressionType.Name));
            }

            return Binary(SourceSpan.None, BinaryOperators.Multiply, left, right);
        }

        /// <summary>
        /// Adds two Int32 values.
        /// </summary>
        public static Expression Add(Expression left, Expression right) {
            if (left.ExpressionType != typeof(int) || right.ExpressionType != typeof(int)) {
                throw new NotSupportedException(String.Format("add only supports ints, got {0} {1}", left.ExpressionType.Name, right.ExpressionType.Name));
            }

            return Binary(SourceSpan.None, BinaryOperators.Add, left, right);
        }

        public static BinaryExpression Binary(SourceSpan span, BinaryOperators op, Expression left, Expression right) {
            if (left == null) {
                throw new ArgumentNullException("left");
            }
            if (right == null) {
                throw new ArgumentNullException("right");
            }
            return new BinaryExpression(span, op, left, right);
        }
    }
}
