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

using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Scripting;
using System.Scripting.Utils;

namespace System.Linq.Expressions {

    partial class LambdaCompiler {

        #region Conditional

        // TODO: We could be a lot smarter about not double jumping for
        // combinations of AndAlso, OrElse, Equal, NotEqual, Block and
        // constants. In fact, we used to have much smarter code that did this
        // but it was removed as part of the compiler merge
        //CONFORMING
        private static void EmitConditionalExpression(LambdaCompiler lc, Expression expr) {
            ConditionalExpression node = (ConditionalExpression)expr;
            Debug.Assert(node.Test.Type == typeof(bool) && node.IfTrue.Type == node.IfFalse.Type);

            Label labFalse = lc._ilg.DefineLabel();
            lc.EmitExpression(node.Test);
            lc._ilg.Emit(OpCodes.Brfalse, labFalse);
            lc.EmitExpression(node.IfTrue);

            if (lc.Significant(node.IfFalse)) {
                Label labEnd = lc._ilg.DefineLabel();
                lc._ilg.Emit(OpCodes.Br, labEnd);
                lc._ilg.MarkLabel(labFalse);
                lc.EmitExpression(node.IfFalse);
                lc._ilg.MarkLabel(labEnd);
            } else {
                lc._ilg.MarkLabel(labFalse);
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
        private bool Significant(Expression node) {
            if (node.NodeType != ExpressionType.EmptyStatement) {
                // non-empty expression is significant
                return true;
            }

            if (_emitDebugSymbols) {
                SourceSpan span = node.Annotations.Get<SourceSpan>();
                if (span.IsValid) {
                    return true;
                }
                SourceLocation header = node.Annotations.Get<SourceLocation>();
                if (header.IsValid) {
                    return true;
                }
            }

            // Not a significant expression
            return false;
        }

        #endregion

        #region Coalesce

        //CONFORMING
        private static void EmitCoalesceBinaryExpression(LambdaCompiler lc, Expression expr) {
            BinaryExpression b = (BinaryExpression)expr;
            Debug.Assert(b.Method == null);

            if (TypeUtils.IsNullableType(b.Left.Type)) {
                lc.EmitNullableCoalesce(b);
            } else if (b.Left.Type.IsValueType) {
                throw Error.CoalesceUsedOnNonNullType();
            } else if (b.Conversion != null) {
                lc.EmitLambdaReferenceCoalesce(b);
            } else {
                lc.EmitReferenceCoalesceWithoutConversion(b);
            }
        }

        //CONFORMING
        private void EmitNullableCoalesce(BinaryExpression b) {
            LocalBuilder loc = _ilg.GetLocal(b.Left.Type);
            Label labIfNull = _ilg.DefineLabel();
            Label labEnd = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Stloc, loc);
            _ilg.Emit(OpCodes.Ldloca, loc);
            _ilg.EmitHasValue(b.Left.Type);
            _ilg.Emit(OpCodes.Brfalse, labIfNull);

            Type nnLeftType = TypeUtils.GetNonNullableType(b.Left.Type);
            if (b.Method != null) {
                ParameterInfo[] parameters = b.Method.GetParameters();
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
                EmitLambdaExpression(this, b.Conversion);

                // emit argument
                if (!p.Type.IsAssignableFrom(b.Left.Type)) {
                    _ilg.Emit(OpCodes.Ldloca, loc);
                    _ilg.EmitGetValueOrDefault(b.Left.Type);
                } else {
                    _ilg.Emit(OpCodes.Ldloc, loc);
                }

                // emit call to invoke
                _ilg.EmitCall(b.Conversion.Type.GetMethod("Invoke"));

            } else if (b.Type != nnLeftType) {
                _ilg.Emit(OpCodes.Ldloca, loc);
                _ilg.EmitGetValueOrDefault(b.Left.Type);
                _ilg.EmitConvertToType(nnLeftType, b.Type, true);
            } else {
                _ilg.Emit(OpCodes.Ldloca, loc);
                _ilg.EmitGetValueOrDefault(b.Left.Type);
            }
            _ilg.FreeLocal(loc);

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
            LocalBuilder loc = _ilg.GetLocal(b.Left.Type);
            Label labEnd = _ilg.DefineLabel();
            Label labNotNull = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Dup);
            _ilg.Emit(OpCodes.Stloc, loc);
            _ilg.Emit(OpCodes.Ldnull);
            _ilg.Emit(OpCodes.Ceq);
            _ilg.Emit(OpCodes.Brfalse, labNotNull);
            _ilg.Emit(OpCodes.Pop);
            EmitExpression(b.Right);
            _ilg.Emit(OpCodes.Br, labEnd);

            // if not null, call conversion
            _ilg.MarkLabel(labNotNull);
            Debug.Assert(b.Conversion.Parameters.Count == 1);
            ParameterExpression p = b.Conversion.Parameters[0];

            // emit the delegate instance
            EmitLambdaExpression(this, b.Conversion);

            // emit argument
            _ilg.Emit(OpCodes.Ldloc, loc);
            _ilg.FreeLocal(loc);

            // emit call to invoke
            _ilg.EmitCall(b.Conversion.Type.GetMethod("Invoke"));

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
                _ilg.Emit(OpCodes.Castclass, b.Type);
            }
            _ilg.Emit(OpCodes.Br_S, labEnd);
            _ilg.MarkLabel(labCast);
            if (b.Left.Type != b.Type) {
                _ilg.Emit(OpCodes.Castclass, b.Type);
            }
            _ilg.MarkLabel(labEnd);
        }

        #endregion

        #region AndAlso

        // for a userdefined type T which has Op_False defined and Lhs, Rhs are nullable L AndAlso R  is computed as
        // L.HasValue 
        //     ? (T.False(L.Value) 
        //         ? L 
        //         : (R.HasValue 
        //             ? (T?)(T.&(L.Value, R.Value)) 
        //             : R))
        //     : L
        private void EmitUserdefinedLiftedAndAlso(BinaryExpression b) {
            Type type = b.Left.Type;
            Type nnType = TypeUtils.GetNonNullableType(type);
            Label labReturnLeft = _ilg.DefineLabel();
            Label labReturnRight = _ilg.DefineLabel();
            Label labExit = _ilg.DefineLabel();

            LocalBuilder locLeft = _ilg.GetLocal(type);
            LocalBuilder locRight = _ilg.GetLocal(type);
            LocalBuilder locNNLeft = _ilg.GetLocal(nnType);
            LocalBuilder locNNRight = _ilg.GetLocal(nnType);

            // load left
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Stloc, locLeft);

            //check left
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse, labExit);

            //try false on left
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitGetValueOrDefault(type);
            Type[] types = new Type[] { nnType };
            MethodInfo opTrue = nnType.GetMethod("op_False",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
            _ilg.Emit(OpCodes.Call, opTrue);
            _ilg.Emit(OpCodes.Brtrue, labExit);

            //load right 
            EmitExpression(b.Right);
            _ilg.Emit(OpCodes.Stloc, locRight);

            // Check right
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse, labReturnRight);

            //Compute bitwise And
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitGetValueOrDefault(type);
            _ilg.Emit(OpCodes.Stloc, locNNLeft);
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitGetValueOrDefault(type);
            _ilg.Emit(OpCodes.Stloc, locNNRight);
            types = new Type[] { nnType, nnType };
            MethodInfo opAnd = nnType.GetMethod("op_BitwiseAnd",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
            _ilg.Emit(OpCodes.Ldloc, locNNLeft);
            _ilg.Emit(OpCodes.Ldloc, locNNRight);
            _ilg.Emit(OpCodes.Call, opAnd);
            if (opAnd.ReturnType != type)
                _ilg.EmitConvertToType(opAnd.ReturnType, type, true);
            _ilg.Emit(OpCodes.Stloc, locLeft);
            _ilg.Emit(OpCodes.Br, labExit);

            //return right
            _ilg.MarkLabel(labReturnRight);
            _ilg.Emit(OpCodes.Ldloc, locRight);
            _ilg.Emit(OpCodes.Stloc, locLeft);
            _ilg.MarkLabel(labExit);
            //return left
            _ilg.Emit(OpCodes.Ldloc, locLeft);

            _ilg.FreeLocal(locLeft);
            _ilg.FreeLocal(locRight);
            _ilg.FreeLocal(locNNLeft);
            _ilg.FreeLocal(locNNRight);
        }

        private void EmitLiftedAndAlso(BinaryExpression b) {
            Type type = typeof(bool?);
            Label labComputeRight = _ilg.DefineLabel();
            Label labReturnFalse = _ilg.DefineLabel();
            Label labReturnNull = _ilg.DefineLabel();
            Label labReturnValue = _ilg.DefineLabel();
            Label labExit = _ilg.DefineLabel();
            LocalBuilder locLeft = _ilg.GetLocal(type);
            LocalBuilder locRight = _ilg.GetLocal(type);
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
            _ilg.FreeLocal(locLeft);
            _ilg.FreeLocal(locRight);
        }

        private void EmitMethodAndAlso(BinaryExpression b) {
            Label labEnd = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Dup);
            Type type = b.Method.GetParameters()[0].ParameterType;
            Type[] types = new Type[] { type };
            MethodInfo opFalse = type.GetMethod("op_False",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
            _ilg.Emit(OpCodes.Call, opFalse);
            _ilg.Emit(OpCodes.Brtrue, labEnd);
            EmitExpression(b.Right);
            Debug.Assert(b.Method.IsStatic);
            _ilg.Emit(OpCodes.Call, b.Method);
            _ilg.MarkLabel(labEnd);
        }

        private void EmitUnliftedAndAlso(BinaryExpression b) {
            EmitExpression(b.Left);
            Label labEnd = _ilg.DefineLabel();
            _ilg.Emit(OpCodes.Dup);
            _ilg.Emit(OpCodes.Brfalse, labEnd);
            _ilg.Emit(OpCodes.Pop);
            EmitExpression(b.Right);
            _ilg.MarkLabel(labEnd);
        }

        private static void EmitAndAlsoBinaryExpression(LambdaCompiler lc, Expression expr) {
            BinaryExpression b = (BinaryExpression)expr;

            if (b.Method != null && !IsLiftedLogicalBinaryOperator(b.Left.Type, b.Right.Type, b.Method)) {
                lc.EmitMethodAndAlso(b);
            } else if (b.Left.Type == typeof(bool?)) {
                lc.EmitLiftedAndAlso(b);
            } else if (IsLiftedLogicalBinaryOperator(b.Left.Type, b.Right.Type, b.Method)) {
                lc.EmitUserdefinedLiftedAndAlso(b);
            } else {
                lc.EmitUnliftedAndAlso(b);
            }
        }

        #endregion

        #region OrElse

        // For a userdefined type T which has Op_True defined and Lhs, Rhs are nullable L OrElse R  is computed as
        // L.HasValue 
        //     ? (T.True(L.Value) 
        //         ? L 
        //         : (R.HasValue 
        //             ? (T?)(T.|(L.Value, R.Value)) 
        //             : R))
        //     : R
        private void EmitUserdefinedLiftedOrElse(BinaryExpression b) {
            Type type = b.Left.Type;
            Type nnType = TypeUtils.GetNonNullableType(type);
            Label labReturnLeft = _ilg.DefineLabel();
            Label labReturnRight = _ilg.DefineLabel();
            Label labExit = _ilg.DefineLabel();

            LocalBuilder locLeft = _ilg.GetLocal(type);
            LocalBuilder locRight = _ilg.GetLocal(type);
            LocalBuilder locNNLeft = _ilg.GetLocal(nnType);
            LocalBuilder locNNRight = _ilg.GetLocal(nnType);

            // Load left
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Stloc, locLeft);

            // Check left
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse, labReturnRight);
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitGetValueOrDefault(type);
            Type[] types = new Type[] { nnType };
            MethodInfo opTrue = nnType.GetMethod("op_True",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
            _ilg.Emit(OpCodes.Call, opTrue);
            _ilg.Emit(OpCodes.Brtrue, labExit);

            // Load right
            EmitExpression(b.Right);
            _ilg.Emit(OpCodes.Stloc, locRight);

            // Check right
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitHasValue(type);
            _ilg.Emit(OpCodes.Brfalse, labReturnRight);

            //Compute bitwise Or
            _ilg.Emit(OpCodes.Ldloca, locLeft);
            _ilg.EmitGetValueOrDefault(type);
            _ilg.Emit(OpCodes.Stloc, locNNLeft);
            _ilg.Emit(OpCodes.Ldloca, locRight);
            _ilg.EmitGetValueOrDefault(type);
            _ilg.Emit(OpCodes.Stloc, locNNRight);
            types = new Type[] { nnType, nnType };
            MethodInfo opAnd = nnType.GetMethod("op_BitwiseOr",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
            _ilg.Emit(OpCodes.Ldloc, locNNLeft);
            _ilg.Emit(OpCodes.Ldloc, locNNRight);
            _ilg.Emit(OpCodes.Call, opAnd);
            if (opAnd.ReturnType != type) {
                _ilg.EmitConvertToType(opAnd.ReturnType, type, true);
            }
            _ilg.Emit(OpCodes.Stloc, locLeft);
            _ilg.Emit(OpCodes.Br, labExit);
            //return right
            _ilg.MarkLabel(labReturnRight);
            _ilg.Emit(OpCodes.Ldloc, locRight);
            _ilg.Emit(OpCodes.Stloc, locLeft);
            _ilg.MarkLabel(labExit);
            //return left
            _ilg.Emit(OpCodes.Ldloc, locLeft);

            _ilg.FreeLocal(locNNLeft);
            _ilg.FreeLocal(locNNRight);
            _ilg.FreeLocal(locLeft);
            _ilg.FreeLocal(locRight);
        }

        private void EmitLiftedOrElse(BinaryExpression b) {
            Type type = typeof(bool?);
            Label labComputeRight = _ilg.DefineLabel();
            Label labReturnTrue = _ilg.DefineLabel();
            Label labReturnNull = _ilg.DefineLabel();
            Label labReturnValue = _ilg.DefineLabel();
            Label labExit = _ilg.DefineLabel();
            LocalBuilder locLeft = _ilg.GetLocal(type);
            LocalBuilder locRight = _ilg.GetLocal(type);
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
            _ilg.FreeLocal(locLeft);
            _ilg.FreeLocal(locRight);
        }

        private void EmitUnliftedOrElse(BinaryExpression b) {
            EmitExpression(b.Left);
            Label labEnd = _ilg.DefineLabel();
            _ilg.Emit(OpCodes.Dup);
            _ilg.Emit(OpCodes.Brtrue, labEnd);
            _ilg.Emit(OpCodes.Pop);
            EmitExpression(b.Right);
            _ilg.MarkLabel(labEnd);
        }

        private void EmitMethodOrElse(BinaryExpression b) {
            Label labEnd = _ilg.DefineLabel();
            EmitExpression(b.Left);
            _ilg.Emit(OpCodes.Dup);
            Type type = b.Method.GetParameters()[0].ParameterType;
            Type[] types = new Type[] { type };
            MethodInfo opTrue = type.GetMethod("op_True",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
            _ilg.Emit(OpCodes.Call, opTrue);
            _ilg.Emit(OpCodes.Brtrue, labEnd);
            EmitExpression(b.Right);
            Debug.Assert(b.Method.IsStatic);
            _ilg.Emit(OpCodes.Call, b.Method);
            _ilg.MarkLabel(labEnd);
        }

        private static void EmitOrElseBinaryExpression(LambdaCompiler lc, Expression expr) {
            BinaryExpression b = (BinaryExpression)expr;

            if (b.Method != null && !IsLiftedLogicalBinaryOperator(b.Left.Type, b.Right.Type, b.Method)) {
                lc.EmitMethodOrElse(b);
            } else if (b.Left.Type == typeof(bool?)) {
                lc.EmitLiftedOrElse(b);
            } else if (IsLiftedLogicalBinaryOperator(b.Left.Type, b.Right.Type, b.Method)) {
                lc.EmitUserdefinedLiftedOrElse(b);
            } else {
                lc.EmitUnliftedOrElse(b);
            }
        }

        #endregion

        private static bool IsLiftedLogicalBinaryOperator(Type left, Type right, MethodInfo method) {
            return right == left &&
                TypeUtils.IsNullableType(left) &&
                method != null &&
                method.ReturnType == TypeUtils.GetNonNullableType(left);
        }
    }
}
