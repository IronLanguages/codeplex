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


using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions.Compiler {

    partial class LambdaCompiler {

        #region Conditional

        //CONFORMING
        private void EmitConditionalExpression(Expression expr) {
            ConditionalExpression node = (ConditionalExpression)expr;
            Debug.Assert(node.Test.Type == typeof(bool) && node.IfTrue.Type == node.IfFalse.Type);

            Label labFalse = _ilg.DefineLabel();
            EmitExpressionAndBranch(false, node.Test, labFalse);
            EmitExpression(node.IfTrue);

            if (Significant(node.IfFalse)) {
                Label labEnd = _ilg.DefineLabel();
                _ilg.Emit(OpCodes.Br, labEnd);
                _ilg.MarkLabel(labFalse);
                EmitExpression(node.IfFalse);
                _ilg.MarkLabel(labEnd);
            } else {
                _ilg.MarkLabel(labFalse);
            }
        }

        /// <summary>
        /// Expression is significant if:
        ///   * it is not an empty expression
        /// == or ==
        ///   * it is an empty expression, and 
        ///   * it has a valid span, and
        ///   * we are emitting debug symbols
        /// </summary>
        private static bool Significant(Expression node) {
            var empty = node as DefaultExpression;
            if (empty == null || empty.Type != typeof(void)) {
                // non-empty expression is significant
                return true;
            }

            // Not a significant expression
            return false;
        }

        #endregion

        #region Coalesce

        //CONFORMING
        private void EmitCoalesceBinaryExpression(Expression expr) {
            BinaryExpression b = (BinaryExpression)expr;
            Debug.Assert(b.Method == null);

            if (TypeUtils.IsNullableType(b.Left.Type)) {
                EmitNullableCoalesce(b);
            } else if (b.Left.Type.IsValueType) {
                throw Error.CoalesceUsedOnNonNullType();
            } else if (b.Conversion != null) {
                EmitLambdaReferenceCoalesce(b);
            } else {
                EmitReferenceCoalesceWithoutConversion(b);
            }
        }

        //CONFORMING
        private void EmitNullableCoalesce(BinaryExpression b) {
            LocalBuilder loc = GetLocal(b.Left.Type);
            Label labIfNull = _ilg.DefineLabel();
            Label labEnd = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Stloc, loc);
            _ilg.Emit(OpCodes.Ldloca, loc);
            _ilg.EmitHasValue(b.Left.Type);
            _ilg.Emit(OpCodes.Brfalse, labIfNull);

            Type nnLeftType = TypeUtils.GetNonNullableType(b.Left.Type);
            if (b.Method != null) {
                ParameterInfo[] parameters = b.Method.GetParametersCached();
                Debug.Assert(b.Method.IsStatic);
                Debug.Assert(parameters.Length == 1);
                Debug.Assert(parameters[0].ParameterType.IsAssignableFrom(b.Left.Type) ||
                             parameters[0].ParameterType.IsAssignableFrom(nnLeftType));
                if (!parameters[0].ParameterType.IsAssignableFrom(b.Left.Type)) {
                    _ilg.Emit(OpCodes.Ldloca, loc);
                    _ilg.EmitGetValueOrDefault(b.Left.Type);
                } else
                    _ilg.Emit(OpCodes.Ldloc, loc);
                _ilg.Emit(OpCodes.Call, b.Method);
            } else if (b.Conversion != null) {
                Debug.Assert(b.Conversion.Parameters.Count == 1);
                ParameterExpression p = b.Conversion.Parameters[0];
                Debug.Assert(p.Type.IsAssignableFrom(b.Left.Type) ||
                             p.Type.IsAssignableFrom(nnLeftType));

                // emit the delegate instance
                EmitLambdaExpression(b.Conversion);

                // emit argument
                if (!p.Type.IsAssignableFrom(b.Left.Type)) {
                    _ilg.Emit(OpCodes.Ldloca, loc);
                    _ilg.EmitGetValueOrDefault(b.Left.Type);
                } else {
                    _ilg.Emit(OpCodes.Ldloc, loc);
                }

                // emit call to invoke
                _ilg.Emit(OpCodes.Callvirt, b.Conversion.Type.GetMethod("Invoke"));

            } else if (b.Type != nnLeftType) {
                _ilg.Emit(OpCodes.Ldloca, loc);
                _ilg.EmitGetValueOrDefault(b.Left.Type);
                _ilg.EmitConvertToType(nnLeftType, b.Type, true);
            } else {
                _ilg.Emit(OpCodes.Ldloca, loc);
                _ilg.EmitGetValueOrDefault(b.Left.Type);
            }
            FreeLocal(loc);

            _ilg.Emit(OpCodes.Br, labEnd);
            _ilg.MarkLabel(labIfNull);
            EmitExpression(b.Right);
            if (b.Right.Type != b.Type) {
                _ilg.EmitConvertToType(b.Right.Type, b.Type, true);
            }
            _ilg.MarkLabel(labEnd);
        }

        //CONFORMING
        private void EmitLambdaReferenceCoalesce(BinaryExpression b) {
            LocalBuilder loc = GetLocal(b.Left.Type);
            Label labEnd = _ilg.DefineLabel();
            Label labNotNull = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Dup);
            _ilg.Emit(OpCodes.Stloc, loc);
            _ilg.Emit(OpCodes.Ldnull);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brfalse, labNotNull);
            EmitExpression(b.Right);
            _ilg.Emit(OpCodes.Br, labEnd);

            // if not null, call conversion
            _ilg.MarkLabel(labNotNull);
            Debug.Assert(b.Conversion.Parameters.Count == 1);
            ParameterExpression p = b.Conversion.Parameters[0];

            // emit the delegate instance
            EmitLambdaExpression(b.Conversion);

            // emit argument
            _ilg.Emit(OpCodes.Ldloc, loc);
            FreeLocal(loc);

            // emit call to invoke
            _ilg.Emit(OpCodes.Callvirt, b.Conversion.Type.GetMethod("Invoke"));

            _ilg.MarkLabel(labEnd);
        }

        //CONFORMING
        private void EmitReferenceCoalesceWithoutConversion(BinaryExpression b) {
            Label labEnd = _ilg.DefineLabel();
            Label labCast = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Dup);
            _ilg.Emit(OpCodes.Ldnull);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brfalse, labCast);
            _ilg.Emit(OpCodes.Pop);
            EmitExpression(b.Right);
            if (b.Right.Type != b.Type) {
                if (b.Right.Type.IsValueType) {
                    _ilg.Emit(OpCodes.Box, b.Right.Type);
                }
                _ilg.Emit(OpCodes.Castclass, b.Type);
            }
            _ilg.Emit(OpCodes.Br_S, labEnd);
            _ilg.MarkLabel(labCast);
            if (b.Left.Type != b.Type) {
                Debug.Assert(!b.Left.Type.IsValueType);
                _ilg.Emit(OpCodes.Castclass, b.Type);
            }
            _ilg.MarkLabel(labEnd);
        }

        #endregion

        #region AndAlso

        private void EmitLiftedAndAlso(BinaryExpression b) {
            Type type = typeof(bool?);
            Label labComputeRight = _ilg.DefineLabel();
            Label labReturnFalse = _ilg.DefineLabel();
            Label labReturnNull = _ilg.DefineLabel();
            Label labReturnValue = _ilg.DefineLabel();
            Label labExit = _ilg.DefineLabel();
            LocalBuilder locLeft = GetLocal(type);
            LocalBuilder locRight = GetLocal(type);
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Stloc, locLeft);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse, labComputeRight);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitGetValueOrDefault(type);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brtrue, labReturnFalse);
            // compute right
            _ilg.MarkLabel(labComputeRight);
            EmitExpression(b.Right);
            _ilg.Emit(OpCodes.Stloc, locRight);
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse_S, labReturnNull);
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitGetValueOrDefault(type);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brtrue_S, labReturnFalse);
            // check left for null again
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse, labReturnNull);
            // return true
            _ilg.Emit(OpCodes.Ldc_I4_1);
            _ilg.Emit(OpCodes.Br_S, labReturnValue);
            // return false
            _ilg.MarkLabel(labReturnFalse);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Br_S, labReturnValue);
            _ilg.MarkLabel(labReturnValue);
            ConstructorInfo ci = type.GetConstructor(new Type[] { typeof(bool) });
            _ilg.Emit(OpCodes.Newobj, ci);
            _ilg.Emit(OpCodes.Stloc, locLeft);
            _ilg.Emit(OpCodes.Br, labExit);
            // return null
            _ilg.MarkLabel(labReturnNull);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.Emit(OpCodes.Initobj, type);
            _ilg.MarkLabel(labExit);
            _ilg.Emit(OpCodes.Ldloc, locLeft);
            FreeLocal(locLeft);
            FreeLocal(locRight);
        }

        private void EmitMethodAndAlso(BinaryExpression b) {
            Label labEnd = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Dup);
            MethodInfo opFalse = TypeUtils.GetBooleanOperator(b.Method.DeclaringType, "op_False");
            Debug.Assert(opFalse != null, "factory should check that the method exists");
            _ilg.Emit(OpCodes.Call, opFalse);
            _ilg.Emit(OpCodes.Brtrue, labEnd);

            //store the value of the left value before emitting b.Right to empty the evaluation stack
            LocalBuilder locLeft = GetLocal(b.Left.Type);
            _ilg.Emit(OpCodes.Stloc, locLeft);

            EmitExpression(b.Right);
            //store the right value to local
            LocalBuilder locRight = GetLocal(b.Right.Type);
            _ilg.Emit(OpCodes.Stloc, locRight);

            Debug.Assert(b.Method.IsStatic);
            _ilg.Emit(OpCodes.Ldloc, locLeft);
            _ilg.Emit(OpCodes.Ldloc, locRight);
            _ilg.Emit(OpCodes.Call, b.Method);
            FreeLocal(locLeft);
            FreeLocal(locRight);
            _ilg.MarkLabel(labEnd);
        }

        private void EmitUnliftedAndAlso(BinaryExpression b) {
            Label @else = _ilg.DefineLabel();
            Label end = _ilg.DefineLabel();
            EmitExpressionAndBranch(false, b.Left, @else);
            EmitExpression(b.Right);
            _ilg.Emit(OpCodes.Br, end);
            _ilg.MarkLabel(@else);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.MarkLabel(end);
        }

        private void EmitAndAlsoBinaryExpression(Expression expr) {
            BinaryExpression b = (BinaryExpression)expr;

            if (b.Method != null && !b.IsLiftedLogical) {
                EmitMethodAndAlso(b);
            } else if (b.Left.Type == typeof(bool?)) {
                EmitLiftedAndAlso(b);
            } else if (b.IsLiftedLogical) {
                EmitExpression(b.ReduceUserdefinedLifted());
            } else {
                EmitUnliftedAndAlso(b);
            }
        }

        #endregion

        #region OrElse

        private void EmitLiftedOrElse(BinaryExpression b) {
            Type type = typeof(bool?);
            Label labComputeRight = _ilg.DefineLabel();
            Label labReturnTrue = _ilg.DefineLabel();
            Label labReturnNull = _ilg.DefineLabel();
            Label labReturnValue = _ilg.DefineLabel();
            Label labExit = _ilg.DefineLabel();
            LocalBuilder locLeft = GetLocal(type);
            LocalBuilder locRight = GetLocal(type);
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Stloc, locLeft);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse, labComputeRight);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitGetValueOrDefault(type);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brfalse, labReturnTrue);
            // compute right
            _ilg.MarkLabel(labComputeRight);
            EmitExpression(b.Right);
            _ilg.Emit(OpCodes.Stloc, locRight);
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse_S, labReturnNull);
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitGetValueOrDefault(type);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brfalse_S, labReturnTrue);
            // check left for null again
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse, labReturnNull);
            // return false
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Br_S, labReturnValue);
            // return true
            _ilg.MarkLabel(labReturnTrue);
            _ilg.Emit(OpCodes.Ldc_I4_1);
            _ilg.Emit(OpCodes.Br_S, labReturnValue);
            _ilg.MarkLabel(labReturnValue);
            ConstructorInfo ci = type.GetConstructor(new Type[] { typeof(bool) });
            _ilg.Emit(OpCodes.Newobj, ci);
            _ilg.Emit(OpCodes.Stloc, locLeft);
            _ilg.Emit(OpCodes.Br, labExit);
            // return null
            _ilg.MarkLabel(labReturnNull);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.Emit(OpCodes.Initobj, type);
            _ilg.MarkLabel(labExit);
            _ilg.Emit(OpCodes.Ldloc, locLeft);
            FreeLocal(locLeft);
            FreeLocal(locRight);
        }

        private void EmitUnliftedOrElse(BinaryExpression b) {
            Label @else = _ilg.DefineLabel();
            Label end = _ilg.DefineLabel();
            EmitExpressionAndBranch(false, b.Left, @else);
            _ilg.Emit(OpCodes.Ldc_I4_1);
            _ilg.Emit(OpCodes.Br, end);
            _ilg.MarkLabel(@else);
            EmitExpression(b.Right);
            _ilg.MarkLabel(end);
        }

        private void EmitMethodOrElse(BinaryExpression b) {
            Label labEnd = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Dup);
            MethodInfo opTrue = TypeUtils.GetBooleanOperator(b.Method.DeclaringType, "op_True");
            Debug.Assert(opTrue != null, "factory should check that the method exists");
            _ilg.Emit(OpCodes.Call, opTrue);
            _ilg.Emit(OpCodes.Brtrue, labEnd);

            //store the value of the left value before emitting b.Right to empty the evaluation stack
            LocalBuilder locLeft = GetLocal(b.Left.Type);
            _ilg.Emit(OpCodes.Stloc, locLeft);

            EmitExpression(b.Right);
            //store the right value to local
            LocalBuilder locRight = GetLocal(b.Right.Type);
            _ilg.Emit(OpCodes.Stloc, locRight);
            
            Debug.Assert(b.Method.IsStatic);
            _ilg.Emit(OpCodes.Ldloc, locLeft);
            _ilg.Emit(OpCodes.Ldloc, locRight);
            _ilg.Emit(OpCodes.Call, b.Method);
            FreeLocal(locLeft);
            FreeLocal(locRight);
            _ilg.MarkLabel(labEnd);
        }

        private void EmitOrElseBinaryExpression(Expression expr) {
            BinaryExpression b = (BinaryExpression)expr;

            if (b.Method != null && !b.IsLiftedLogical) {
                EmitMethodOrElse(b);
            } else if (b.Left.Type == typeof(bool?)) {
                EmitLiftedOrElse(b);
            } else if (b.IsLiftedLogical) {
                EmitExpression(b.ReduceUserdefinedLifted());
            } else {
                EmitUnliftedOrElse(b);
            }
        }

        #endregion

        #region Optimized branching

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void EmitExpressionAndBranch(bool branchValue, Expression node, Label label) {
            if (node.Type == typeof(bool)) {
                switch (node.NodeType) {
                    case ExpressionType.AndAlso:
                    case ExpressionType.OrElse:
                        EmitBranchLogical(branchValue, (BinaryExpression)node, label);
                        return;
                    case ExpressionType.Block:
                        EmitBranchBlock(branchValue, (BlockExpression)node, label);
                        return;
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                        EmitBranchComparison(branchValue, (BinaryExpression)node, label);
                        return;
                }
            }
            EmitExpression(node);
            EmitBranchOp(branchValue, label);
        }

        private void EmitBranchOp(bool branch, Label label) {
            _ilg.Emit(branch ? OpCodes.Brtrue : OpCodes.Brfalse, label);
        }

        private void EmitBranchComparison(bool branch, BinaryExpression node, Label label) {
            Debug.Assert(node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual);
            Debug.Assert(!node.IsLiftedToNull);

            // To share code paths, we want to treat NotEqual as an inverted Equal
            bool branchWhenEqual = branch == (node.NodeType == ExpressionType.Equal);

            if (node.Method != null) {
                EmitBinaryMethod(node);
                // EmitBinaryMethod takes into account the Equal/NotEqual
                // node kind, so use the original branch value
                EmitBranchOp(branch, label);
            } else if (ConstantCheck.IsNull(node.Left)) {
                if (TypeUtils.IsNullableType(node.Right.Type)) {
                    EmitAddress(node.Right, node.Right.Type);
                    _ilg.EmitHasValue(node.Right.Type);
                } else {
                    Debug.Assert(!node.Right.Type.IsValueType);
                    EmitExpression(node.Right);
                }
                EmitBranchOp(!branchWhenEqual, label);
            } else if (ConstantCheck.IsNull(node.Right)) {
                if (TypeUtils.IsNullableType(node.Left.Type)) {
                    EmitAddress(node.Left, node.Left.Type);
                    _ilg.EmitHasValue(node.Left.Type);
                } else {
                    Debug.Assert(!node.Left.Type.IsValueType);
                    EmitExpression(node.Left);
                }
                EmitBranchOp(!branchWhenEqual, label);
            } else if (TypeUtils.IsNullableType(node.Left.Type) || TypeUtils.IsNullableType(node.Right.Type)) {
                EmitBinaryExpression(node);
                // EmitBinaryExpression takes into account the Equal/NotEqual
                // node kind, so use the original branch value
                EmitBranchOp(branch, label);
            } else {
                EmitExpression(node.Left);
                EmitExpression(node.Right);
                if (branchWhenEqual) {
                    _ilg.Emit(OpCodes.Beq, label);
                } else {
                    _ilg.Emit(OpCodes.Ceq);
                    _ilg.Emit(OpCodes.Brfalse, label);
                }
            }
        }

        private void EmitBranchLogical(bool branch, BinaryExpression node, Label label) {
            Debug.Assert(node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse);
            Debug.Assert(!node.IsLiftedToNull);

            if (node.Method != null || node.IsLifted) {
                EmitExpression(node);
                EmitBranchOp(branch, label);
                return;
            }
            
            
            bool isAnd = node.NodeType == ExpressionType.AndAlso;

            // To share code, we make the following substitutions:
            //     if (!(left || right)) branch value
            // becomes:
            //     if (!left && !right) branch value
            // and:
            //     if (!(left && right)) branch value
            // becomes:
            //     if (!left || !right) branch value

            if (branch == isAnd) {
                EmitBranchAnd(branch, (BinaryExpression)node, label);
            } else {
                EmitBranchOr(branch, (BinaryExpression)node, label);
            }
        }

        // Generates optimized AndAlso with branch == true
        // or optimized OrElse with branch == false
        private void EmitBranchAnd(bool branch, BinaryExpression node, Label label) {
            // if (left AND right) branch label

            if (!ConstantCheck.IsConstant(node.Left, !branch) && !ConstantCheck.IsConstant(node.Right, !branch)) {
                if (ConstantCheck.IsConstant(node.Left, branch)) {
                    EmitExpressionAndBranch(branch, node.Right, label);
                } else if (ConstantCheck.IsConstant(node.Right, branch)) {
                    EmitExpressionAndBranch(branch, node.Left, label);
                } else {
                    // if (left) then 
                    //   if (right) branch label
                    // endif

                    Label endif = _ilg.DefineLabel();
                    EmitExpressionAndBranch(!branch, node.Left, endif);
                    EmitExpressionAndBranch(branch, node.Right, label);
                    _ilg.MarkLabel(endif);
                }
            }            
        }

        // Generates optimized OrElse with branch == true
        // or optimized AndAlso with branch == false
        private void EmitBranchOr(bool branch, BinaryExpression node, Label label) {
            // if (left OR right) branch label

            if (ConstantCheck.IsConstant(node.Left, branch)) {
                _ilg.Emit(OpCodes.Br, label);
            } else {
                if (!ConstantCheck.IsConstant(node.Left, !branch)) {
                    EmitExpressionAndBranch(branch, node.Left, label);
                }

                if (ConstantCheck.IsConstant(node.Right, branch)) {
                    _ilg.Emit(OpCodes.Br, label);
                } else if (!ConstantCheck.IsConstant(node.Right, !branch)) {
                    EmitExpressionAndBranch(branch, node.Right, label);
                }
            }
        }

        private void EmitBranchBlock(bool branch, BlockExpression node, Label label) {
            EnterScope(node);

            int count = node.ExpressionCount;
            for (int i = 0; i < count - 1; i++) {
                EmitExpressionAsVoid(node.GetExpression(i));
            }
            EmitExpressionAndBranch(branch, node.GetExpression(count - 1), label);
            
            ExitScope(node);
        }

        #endregion
    }
}
