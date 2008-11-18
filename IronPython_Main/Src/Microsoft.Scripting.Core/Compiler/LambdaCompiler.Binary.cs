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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions.Compiler {
    partial class LambdaCompiler {

        //CONFORMING
        private void EmitBinaryExpression(Expression expr) {
            BinaryExpression b = (BinaryExpression)expr;

            Debug.Assert(b.NodeType != ExpressionType.AndAlso && b.NodeType != ExpressionType.OrElse && b.NodeType != ExpressionType.Coalesce);

            if (b.Method != null) {
                EmitBinaryMethod(b);
                return;
            }

            // For EQ and NE, if there is a user-specified method, use it.
            // Otherwise implement the C# semantics that allow equality
            // comparisons on non-primitive nullable structs that don't
            // overload "=="
            if ((b.NodeType == ExpressionType.Equal || b.NodeType == ExpressionType.NotEqual) &&
                (b.Type == typeof(bool) || b.Type == typeof(bool?))) {

                // If we have x==null, x!=null, null==x or null!=x where x is
                // nullable but not null, then generate a call to x.HasValue.
                Debug.Assert(!b.IsLiftedToNull || b.Type == typeof(bool?));
                if (ConstantCheck.IsNull(b.Left) && !ConstantCheck.IsNull(b.Right) && TypeUtils.IsNullableType(b.Right.Type)) {
                    EmitNullEquality(b.NodeType, b.Right, b.IsLiftedToNull);
                    return;
                }
                if (ConstantCheck.IsNull(b.Right) && !ConstantCheck.IsNull(b.Left) && TypeUtils.IsNullableType(b.Left.Type)) {
                    EmitNullEquality(b.NodeType, b.Left, b.IsLiftedToNull);
                    return;
                }
            }

            // Otherwise generate it normally.
            EmitExpression(b.Left);
            EmitExpression(b.Right);

            EmitBinaryOperator(b.NodeType, b.Left.Type, b.Right.Type, b.Type, b.IsLiftedToNull);
        }

        //CONFORMING
        private void EmitNullEquality(ExpressionType op, Expression e, bool isLiftedToNull) {
            Debug.Assert(TypeUtils.IsNullableType(e.Type));
            Debug.Assert(op == ExpressionType.Equal || op == ExpressionType.NotEqual);
            // If we are lifted to null then just evaluate the expression for its side effects, discard,
            // and generate null.  If we are not lifted to null then generate a call to HasValue.
            if (isLiftedToNull) {
                EmitExpressionAsVoid(e);
                _ilg.EmitDefault(typeof(bool?));
            } else {
                EmitAddress(e, e.Type);
                _ilg.EmitHasValue(e.Type);
                if (op == ExpressionType.Equal) {
                    _ilg.Emit(OpCodes.Ldc_I4_0);
                    _ilg.Emit(OpCodes.Ceq);
                }
            }
        }

        //CONFORMING
        private void EmitBinaryMethod(BinaryExpression b) {
            if (b.IsLifted) {
                ParameterExpression p1 = Expression.Variable(TypeUtils.GetNonNullableType(b.Left.Type), null);
                ParameterExpression p2 = Expression.Variable(TypeUtils.GetNonNullableType(b.Right.Type), null);
                MethodCallExpression mc = Expression.Call(null, b.Method, p1, p2);
                Type resultType = null;
                if (b.IsLiftedToNull) {
                    resultType = TypeUtils.GetNullableType(mc.Type);
                } else {
                    switch (b.NodeType) {
                        case ExpressionType.Equal:
                        case ExpressionType.NotEqual:
                        case ExpressionType.LessThan:
                        case ExpressionType.LessThanOrEqual:
                        case ExpressionType.GreaterThan:
                        case ExpressionType.GreaterThanOrEqual:
                            if (mc.Type != typeof(bool)) {
                                throw Error.ArgumentMustBeBoolean();
                            }
                            resultType = typeof(bool);
                            break;
                        default:
                            resultType = TypeUtils.GetNullableType(mc.Type);
                            break;
                    }
                }
                IList<ParameterExpression> variables = new ParameterExpression[] { p1, p2 };
                IList<Expression> arguments = new Expression[] { b.Left, b.Right };
                ValidateLift(variables, arguments);
                EmitLift(b.NodeType, resultType, mc, variables, arguments);
            } else {
                EmitMethodCallExpression(Expression.Call(null, b.Method, b.Left, b.Right));
            }
        }

        //CONFORMING
        private void EmitBinaryOperator(ExpressionType op, Type leftType, Type rightType, Type resultType, bool liftedToNull) {
            bool leftIsNullable = TypeUtils.IsNullableType(leftType);
            bool rightIsNullable = TypeUtils.IsNullableType(rightType);
            switch (op) {
                case ExpressionType.ArrayIndex:
                    if (rightIsNullable) {
                        LocalBuilder loc = _ilg.GetLocal(rightType);
                        _ilg.Emit(OpCodes.Stloc, loc);
                        _ilg.Emit(OpCodes.Ldloca, loc);
                        _ilg.FreeLocal(loc);
                        _ilg.EmitGetValue(rightType);
                    }
                    Type indexType = TypeUtils.GetNonNullableType(rightType);
                    if (indexType != typeof(int)) {
                        _ilg.EmitConvertToType(indexType, typeof(int), true);
                    }
                    _ilg.EmitLoadElement(leftType.GetElementType());
                    return;
                case ExpressionType.Coalesce:
                    throw Error.UnexpectedCoalesceOperator();
            }

            if (leftIsNullable) {
                EmitLiftedBinaryOp(op, leftType, rightType, resultType, liftedToNull);
            } else {
                EmitUnliftedBinaryOp(op, leftType, rightType);
            }
        }

        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void EmitUnliftedBinaryOp(ExpressionType op, Type leftType, Type rightType) {
            Debug.Assert(!TypeUtils.IsNullableType(leftType));
            if (op == ExpressionType.Equal || op == ExpressionType.NotEqual) {
                EmitUnliftedEquality(op, leftType);
                return;
            }
            if (!leftType.IsPrimitive) {
                throw Error.OperatorNotImplementedForType(op, leftType);
            }
            switch (op) {
                case ExpressionType.Add:
                    _ilg.Emit(OpCodes.Add);
                    break;
                case ExpressionType.AddChecked: 
                    EmitOverflowHelper(leftType, rightType);
                    if (TypeUtils.IsFloatingPoint(leftType)) {
                        _ilg.Emit(OpCodes.Add);
                    } else if (TypeUtils.IsUnsigned(leftType)) {
                        _ilg.Emit(OpCodes.Add_Ovf_Un);
                    } else {
                        _ilg.Emit(OpCodes.Add_Ovf);
                    }
                    break;
                case ExpressionType.Subtract:
                    _ilg.Emit(OpCodes.Sub);
                    break;
                case ExpressionType.SubtractChecked:
                    EmitOverflowHelper(leftType, rightType);
                    if (TypeUtils.IsFloatingPoint(leftType)) {
                        _ilg.Emit(OpCodes.Sub);
                    } else if (TypeUtils.IsUnsigned(leftType)) {
                        _ilg.Emit(OpCodes.Sub_Ovf_Un);
                    } else {
                        _ilg.Emit(OpCodes.Sub_Ovf);
                    }
                    break;
                case ExpressionType.Multiply:
                    _ilg.Emit(OpCodes.Mul);
                    break;
                case ExpressionType.MultiplyChecked:
                    EmitOverflowHelper(leftType, rightType);
                    if (TypeUtils.IsFloatingPoint(leftType)) {
                        _ilg.Emit(OpCodes.Mul);
                    } else if (TypeUtils.IsUnsigned(leftType)) {
                        _ilg.Emit(OpCodes.Mul_Ovf_Un);
                    } else {
                        _ilg.Emit(OpCodes.Mul_Ovf);
                    }
                    break;
                case ExpressionType.Divide:
                    if (TypeUtils.IsUnsigned(leftType)) {
                        _ilg.Emit(OpCodes.Div_Un);
                    } else {
                        _ilg.Emit(OpCodes.Div);
                    }
                    break;
                case ExpressionType.Modulo:
                    if (TypeUtils.IsUnsigned(leftType)) {
                        _ilg.Emit(OpCodes.Rem_Un);
                    } else {
                        _ilg.Emit(OpCodes.Rem);
                    }
                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _ilg.Emit(OpCodes.And);
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    _ilg.Emit(OpCodes.Or);
                    break;
                case ExpressionType.LessThan:
                    if (TypeUtils.IsUnsigned(leftType)) {
                        _ilg.Emit(OpCodes.Clt_Un);
                    } else {
                        _ilg.Emit(OpCodes.Clt);
                    }
                    break;
                case ExpressionType.LessThanOrEqual: {
                        Label labFalse = _ilg.DefineLabel();
                        Label labEnd = _ilg.DefineLabel();
                        if (TypeUtils.IsUnsigned(leftType)) {
                            _ilg.Emit(OpCodes.Ble_Un_S, labFalse);
                        } else {
                            _ilg.Emit(OpCodes.Ble_S, labFalse);
                        }
                        _ilg.Emit(OpCodes.Ldc_I4_0);
                        _ilg.Emit(OpCodes.Br_S, labEnd);
                        _ilg.MarkLabel(labFalse);
                        _ilg.Emit(OpCodes.Ldc_I4_1);
                        _ilg.MarkLabel(labEnd);
                    }
                    break;
                case ExpressionType.GreaterThan:
                    if (TypeUtils.IsUnsigned(leftType)) {
                        _ilg.Emit(OpCodes.Cgt_Un);
                    } else {
                        _ilg.Emit(OpCodes.Cgt);
                    }
                    break;
                case ExpressionType.GreaterThanOrEqual: {
                        Label labFalse = _ilg.DefineLabel();
                        Label labEnd = _ilg.DefineLabel();
                        if (TypeUtils.IsUnsigned(leftType)) {
                            _ilg.Emit(OpCodes.Bge_Un_S, labFalse);
                        } else {
                            _ilg.Emit(OpCodes.Bge_S, labFalse);
                        }
                        _ilg.Emit(OpCodes.Ldc_I4_0);
                        _ilg.Emit(OpCodes.Br_S, labEnd);
                        _ilg.MarkLabel(labFalse);
                        _ilg.Emit(OpCodes.Ldc_I4_1);
                        _ilg.MarkLabel(labEnd);
                    }
                    break;
                case ExpressionType.ExclusiveOr:
                    _ilg.Emit(OpCodes.Xor);
                    break;
                case ExpressionType.LeftShift: {
                        Type shiftType = TypeUtils.GetNonNullableType(rightType);
                        if (shiftType != typeof(int)) {
                            _ilg.EmitConvertToType(shiftType, typeof(int), true);
                        }
                        _ilg.Emit(OpCodes.Shl);
                    }
                    break;
                case ExpressionType.RightShift: {
                        Type shiftType = TypeUtils.GetNonNullableType(rightType);
                        if (shiftType != typeof(int)) {
                            _ilg.EmitConvertToType(shiftType, typeof(int), true);
                        }
                        if (TypeUtils.IsUnsigned(leftType)) {
                            _ilg.Emit(OpCodes.Shr_Un);
                        } else {
                            _ilg.Emit(OpCodes.Shr);
                        }
                    }
                    break;
                default:
                    throw Error.UnhandledBinary(op);
            }
        }

        //this code is needed to make sure that we get overflow exception
        private void EmitOverflowHelper(Type leftType, Type rightType) {
            LocalBuilder left = _ilg.GetLocal(leftType);
            LocalBuilder right = _ilg.GetLocal(rightType);
            _ilg.Emit(OpCodes.Stloc, right);
            _ilg.Emit(OpCodes.Stloc, left);
            _ilg.Emit(OpCodes.Ldloc, left);
            _ilg.Emit(OpCodes.Ldloc, right);
            _ilg.FreeLocal(left);
            _ilg.FreeLocal(right);
        }

        //CONFORMING
        private void EmitUnliftedEquality(ExpressionType op, Type type) {
            Debug.Assert(op == ExpressionType.Equal || op == ExpressionType.NotEqual);
            if (!type.IsPrimitive && type.IsValueType && !type.IsEnum) {
                throw Error.OperatorNotImplementedForType(op, type);
            }
            _ilg.Emit(OpCodes.Ceq);
            if (op == ExpressionType.NotEqual) {
                _ilg.Emit(OpCodes.Ldc_I4_0);
                _ilg.Emit(OpCodes.Ceq);
            }
        }

        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void EmitLiftedBinaryOp(ExpressionType op, Type leftType, Type rightType, Type resultType, bool liftedToNull) {
            Debug.Assert(TypeUtils.IsNullableType(leftType));
            switch (op) {
                case ExpressionType.And:
                    if (leftType == typeof(bool?)) {
                        EmitLiftedBooleanAnd();
                    } else {
                        EmitLiftedBinaryArithmetic(op, leftType, rightType, resultType);
                    }
                    break;
                case ExpressionType.Or:
                    if (leftType == typeof(bool?)) {
                        EmitLiftedBooleanOr();
                    } else {
                        EmitLiftedBinaryArithmetic(op, leftType, rightType, resultType);
                    }
                    break;
                case ExpressionType.ExclusiveOr:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
                    EmitLiftedBinaryArithmetic(op, leftType, rightType, resultType);
                    break;
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    EmitLiftedRelational(op, leftType, rightType, resultType, liftedToNull);
                    break;
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                default:
                    throw Assert.Unreachable;
            }
        }

        //CONFORMING
        private void EmitLiftedRelational(ExpressionType op, Type leftType, Type rightType, Type resultType, bool liftedToNull) {
            Debug.Assert(TypeUtils.IsNullableType(leftType));

            Label shortCircuit = _ilg.DefineLabel();
            LocalBuilder locLeft = _ilg.GetLocal(leftType);
            LocalBuilder locRight = _ilg.GetLocal(rightType);

            // store values (reverse order since they are already on the stack)
            _ilg.Emit(OpCodes.Stloc, locRight);
            _ilg.Emit(OpCodes.Stloc, locLeft);

            if (op == ExpressionType.Equal) {
                // test for both null -> true
                _ilg.Emit(OpCodes.Ldloca, locLeft);
                _ilg.EmitHasValue(leftType);
                _ilg.Emit(OpCodes.Ldc_I4_0);
                _ilg.Emit(OpCodes.Ceq);
                _ilg.Emit(OpCodes.Ldloca, locRight);
                _ilg.EmitHasValue(rightType);
                _ilg.Emit(OpCodes.Ldc_I4_0);
                _ilg.Emit(OpCodes.Ceq);
                _ilg.Emit(OpCodes.And);
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Brtrue_S, shortCircuit);
                _ilg.Emit(OpCodes.Pop);

                // test for either is null -> false
                _ilg.Emit(OpCodes.Ldloca, locLeft);
                _ilg.EmitHasValue(leftType);
                _ilg.Emit(OpCodes.Ldloca, locRight);
                _ilg.EmitHasValue(rightType);
                _ilg.Emit(OpCodes.And);

                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Brfalse_S, shortCircuit);
                _ilg.Emit(OpCodes.Pop);
            } else if (op == ExpressionType.NotEqual) {
                // test for both null -> false
                _ilg.Emit(OpCodes.Ldloca, locLeft);
                _ilg.EmitHasValue(leftType);
                _ilg.Emit(OpCodes.Ldloca, locRight);
                _ilg.EmitHasValue(rightType);
                _ilg.Emit(OpCodes.Or);
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Brfalse_S, shortCircuit);
                _ilg.Emit(OpCodes.Pop);

                // test for either is null -> true
                _ilg.Emit(OpCodes.Ldloca, locLeft);
                _ilg.EmitHasValue(leftType);
                _ilg.Emit(OpCodes.Ldc_I4_0);
                _ilg.Emit(OpCodes.Ceq);
                _ilg.Emit(OpCodes.Ldloca, locRight);
                _ilg.EmitHasValue(rightType);
                _ilg.Emit(OpCodes.Ldc_I4_0);
                _ilg.Emit(OpCodes.Ceq);
                _ilg.Emit(OpCodes.Or);
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Brtrue_S, shortCircuit);
                _ilg.Emit(OpCodes.Pop);
            } else {
                // test for either is null -> false
                _ilg.Emit(OpCodes.Ldloca, locLeft);
                _ilg.EmitHasValue(leftType);
                _ilg.Emit(OpCodes.Ldloca, locRight);
                _ilg.EmitHasValue(rightType);
                _ilg.Emit(OpCodes.And);
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Brfalse_S, shortCircuit);
                _ilg.Emit(OpCodes.Pop);
            }

            // do op on values
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitGetValueOrDefault(leftType);
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitGetValueOrDefault(rightType);

            //RELEASING locLeft locRight
            _ilg.FreeLocal(locLeft);
            _ilg.FreeLocal(locRight);

            EmitBinaryOperator(
                op,
                TypeUtils.GetNonNullableType(leftType),
                TypeUtils.GetNonNullableType(rightType),
                TypeUtils.GetNonNullableType(resultType),
                false
            );

            if (!liftedToNull) {
                _ilg.MarkLabel(shortCircuit);
            }

            if (resultType != TypeUtils.GetNonNullableType(resultType)) {
                _ilg.EmitConvertToType(TypeUtils.GetNonNullableType(resultType), resultType, true);
            }

            if (liftedToNull) {
                Label labEnd = _ilg.DefineLabel();
                _ilg.Emit(OpCodes.Br, labEnd);
                _ilg.MarkLabel(shortCircuit);
                _ilg.Emit(OpCodes.Pop);
                _ilg.Emit(OpCodes.Ldnull);
                _ilg.Emit(OpCodes.Unbox_Any, resultType);
                _ilg.MarkLabel(labEnd);
            }
        }

        //CONFORMING
        private void EmitLiftedBinaryArithmetic(ExpressionType op, Type leftType, Type rightType, Type resultType) {
            bool leftIsNullable = TypeUtils.IsNullableType(leftType);
            bool rightIsNullable = TypeUtils.IsNullableType(rightType);

            Debug.Assert(leftIsNullable);

            Label labIfNull = _ilg.DefineLabel();
            Label labEnd = _ilg.DefineLabel();
            LocalBuilder locLeft = _ilg.GetLocal(leftType);
            LocalBuilder locRight = _ilg.GetLocal(rightType);
            LocalBuilder locResult = _ilg.GetLocal(resultType);

            // store values (reverse order since they are already on the stack)
            _ilg.Emit(OpCodes.Stloc, locRight);
            _ilg.Emit(OpCodes.Stloc, locLeft);

            // test for null
            if (rightIsNullable) {
                _ilg.Emit(OpCodes.Ldloca, locLeft);
                _ilg.EmitHasValue(leftType);
                _ilg.Emit(OpCodes.Ldloca, locRight);
                _ilg.EmitHasValue(rightType);
                _ilg.Emit(OpCodes.And);
                _ilg.Emit(OpCodes.Brfalse_S, labIfNull);
            } else {
                _ilg.Emit(OpCodes.Ldloca, locLeft);
                _ilg.EmitHasValue(leftType);
                _ilg.Emit(OpCodes.Brfalse_S, labIfNull);
            }

            // do op on values
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitGetValueOrDefault(leftType);

            if (rightIsNullable) {
                _ilg.Emit(OpCodes.Ldloca, locRight);
                _ilg.EmitGetValueOrDefault(rightType);
            } else {
                _ilg.Emit(OpCodes.Ldloc, locRight);
            }

            //RELEASING locLeft locRight
            _ilg.FreeLocal(locLeft);
            _ilg.FreeLocal(locRight);

            EmitBinaryOperator(op, TypeUtils.GetNonNullableType(leftType), TypeUtils.GetNonNullableType(rightType), TypeUtils.GetNonNullableType(resultType), false);

            // construct result type
            ConstructorInfo ci = resultType.GetConstructor(new Type[] { TypeUtils.GetNonNullableType(resultType) });
            _ilg.Emit(OpCodes.Newobj, ci);
            _ilg.Emit(OpCodes.Stloc, locResult);
            _ilg.Emit(OpCodes.Br_S, labEnd);

            // if null then create a default one
            _ilg.MarkLabel(labIfNull);
            _ilg.Emit(OpCodes.Ldloca, locResult);
            _ilg.Emit(OpCodes.Initobj, resultType);

            _ilg.MarkLabel(labEnd);

            _ilg.Emit(OpCodes.Ldloc, locResult);

            //RELEASING locResult
            _ilg.FreeLocal(locResult);
        }

        //CONFORMING
        private void EmitLiftedBooleanAnd() {
            Type type = typeof(bool?);
            Label labComputeRight = _ilg.DefineLabel();
            Label labReturnFalse = _ilg.DefineLabel();
            Label labReturnNull = _ilg.DefineLabel();
            Label labReturnValue = _ilg.DefineLabel();
            Label labExit = _ilg.DefineLabel();

            // store values (reverse order since they are already on the stack)
            LocalBuilder locLeft = _ilg.GetLocal(type);
            LocalBuilder locRight = _ilg.GetLocal(type);
            _ilg.Emit(OpCodes.Stloc, locRight);
            _ilg.Emit(OpCodes.Stloc, locLeft);

            // compute left
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
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse_S, labReturnNull);
            _ilg.Emit(OpCodes.Ldloca, locRight);

            //RELEASING locRight
            _ilg.FreeLocal(locRight);

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

            //RELEASING locLeft
            _ilg.FreeLocal(locLeft);
        }

        //CONFORMING
        private void EmitLiftedBooleanOr() {
            Type type = typeof(bool?);
            Label labComputeRight = _ilg.DefineLabel();
            Label labReturnTrue = _ilg.DefineLabel();
            Label labReturnNull = _ilg.DefineLabel();
            Label labReturnValue = _ilg.DefineLabel();
            Label labExit = _ilg.DefineLabel();

            // store values (reverse order since they are already on the stack)
            LocalBuilder locLeft = _ilg.GetLocal(type);
            LocalBuilder locRight = _ilg.GetLocal(type);
            _ilg.Emit(OpCodes.Stloc, locRight);
            _ilg.Emit(OpCodes.Stloc, locLeft);

            // compute left
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
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse_S, labReturnNull);
            _ilg.Emit(OpCodes.Ldloca, locRight);

            //RELEASING locRight
            _ilg.FreeLocal(locRight);

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

            //RELEASING locLeft
            _ilg.FreeLocal(locLeft);
        }
    }
}