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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    partial class LambdaCompiler {
        /// <summary>
        /// Generates code for this expression in a value position.
        /// This method will leave the value of the expression
        /// on the top of the stack typed as Type.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal void EmitExpression(Expression node) {
            Debug.Assert(node != null);

            switch (node.NodeType) {
                case AstNodeType.AndAlso:
                    EmitBooleanOperator((BinaryExpression)node, true);
                    break;

                case AstNodeType.OrElse:
                    EmitBooleanOperator((BinaryExpression)node, false);
                    break;

                case AstNodeType.Add:
                case AstNodeType.And:
                case AstNodeType.ArrayIndex:
                case AstNodeType.Divide:
                case AstNodeType.Equal:
                case AstNodeType.ExclusiveOr:
                case AstNodeType.GreaterThan:
                case AstNodeType.GreaterThanOrEqual:
                case AstNodeType.LeftShift:
                case AstNodeType.LessThan:
                case AstNodeType.LessThanOrEqual:
                case AstNodeType.Modulo:
                case AstNodeType.Multiply:
                case AstNodeType.NotEqual:
                case AstNodeType.Or:
                case AstNodeType.RightShift:
                case AstNodeType.Subtract:
                    Emit((BinaryExpression)node);
                    break;

                case AstNodeType.Call:
                    Emit((MethodCallExpression)node);
                    break;

                case AstNodeType.Invoke:
                    Emit((InvocationExpression)node);
                    break;

                case AstNodeType.Conditional:
                    Emit((ConditionalExpression)node);
                    break;

                case AstNodeType.Constant:
                    Emit((ConstantExpression)node);
                    break;

                case AstNodeType.Convert:
                case AstNodeType.Negate:
                case AstNodeType.Not:
                case AstNodeType.OnesComplement:
                    Emit((UnaryExpression)node);
                    break;

                case AstNodeType.New:
                    Emit((NewExpression)node);
                    break;

                case AstNodeType.TypeIs:
                    Emit((TypeBinaryExpression)node);
                    break;

                case AstNodeType.ActionExpression:
                    Emit((ActionExpression)node);
                    break;

                case AstNodeType.Assign:
                    Emit((AssignmentExpression)node);
                    break;

                case AstNodeType.GlobalVariable:
                case AstNodeType.LocalVariable:
                case AstNodeType.TemporaryVariable:
                    Emit((VariableExpression)node);
                    break;

                case AstNodeType.Parameter:
                    Emit((ParameterExpression)node);
                    break;

                case AstNodeType.Lambda:
                case AstNodeType.Generator:
                    Emit((LambdaExpression)node);
                    break;

                case AstNodeType.CodeContextExpression:
                    EmitCodeContext();
                    break;

                case AstNodeType.GeneratorIntrinsic:
                    EmitGeneratorIntrinsic();
                    break;

                case AstNodeType.MemberExpression:
                    Emit((MemberExpression)node);
                    break;

                case AstNodeType.NewArrayExpression:
                case AstNodeType.NewArrayBounds:
                    Emit((NewArrayExpression)node);
                    break;

                case AstNodeType.Block:
                    Emit((Block)node);
                    break;

                case AstNodeType.BreakStatement:
                    Emit((BreakStatement)node);
                    break;

                case AstNodeType.ContinueStatement:
                    Emit((ContinueStatement)node);
                    break;

                case AstNodeType.DeleteStatement:
                    Emit((DeleteStatement)node);
                    break;

                case AstNodeType.DoStatement:
                    Emit((DoStatement)node);
                    break;

                case AstNodeType.EmptyStatement:
                    Emit((EmptyStatement)node);
                    break;

                case AstNodeType.LabeledStatement:
                    Emit((LabeledStatement)node);
                    break;

                case AstNodeType.LoopStatement:
                    Emit((LoopStatement)node);
                    break;

                case AstNodeType.ReturnStatement:
                    Emit((ReturnStatement)node);
                    break;

                case AstNodeType.ScopeStatement:
                    Emit((ScopeStatement)node);
                    break;

                case AstNodeType.SwitchStatement:
                    Emit((SwitchStatement)node);
                    break;

                case AstNodeType.ThrowStatement:
                    Emit((ThrowStatement)node);
                    break;

                case AstNodeType.TryStatement:
                    Emit((TryStatement)node);
                    break;

                case AstNodeType.YieldStatement:
                    Emit((YieldStatement)node);
                    break;

                case AstNodeType.Extension: // StackSpiller reduces Extension node type
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Generates the code for the expression, leaving it on
        /// the stack typed as object.
        /// </summary>
        private void EmitExpressionAsObject(Expression node) {
            EmitExpression(node);
            EmitBoxing(node.Type);
        }

        #region BinaryExpression

        private void Emit(BinaryExpression node) {
            Debug.Assert(node.NodeType != AstNodeType.AndAlso && node.NodeType != AstNodeType.OrElse);

            if (node.IsDynamic) {
                EmitCallSite(node, node.Left, node.Right);
                return;
            }

            if (NullableVsNull(node.Left, node.Right)) {
                EmitExpressionAddress(node.Left, node.Left.Type);

                GenerateNullableBinaryOperator(node.NodeType, node.Left.Type);
            } else if (NullableVsNull(node.Right, node.Left)) {
                // null vs Nullable<T>
                EmitExpressionAddress(node.Right, node.Right.Type);

                GenerateNullableBinaryOperator(node.NodeType, node.Right.Type);
            } else {
                EmitExpression(node.Left);
                EmitExpression(node.Right);

                if (node.Method != null) {
                    _ilg.EmitCall(node.Method);
                } else {
                    GenerateBinaryOperator(node.NodeType, node.Type);
                }
            }
        }

        private void GenerateNullableBinaryOperator(AstNodeType astNodeType, Type nullableType) {
            switch (astNodeType) {
                case AstNodeType.NotEqual:
                    _ilg.EmitPropertyGet(nullableType, "HasValue");
                    break;
                case AstNodeType.Equal:
                    _ilg.EmitPropertyGet(nullableType, "HasValue");
                    _ilg.EmitBoolean(false);
                    _ilg.Emit(OpCodes.Ceq);
                    break;
                default:
                    throw new InvalidOperationException(astNodeType.ToString());
            }
        }

        private static bool NullableVsNull(Expression nullable, Expression nullVal) {
            return TypeUtils.IsNullableType(nullable.Type) && ConstantCheck.IsConstant(nullVal, null);
        }

        private void EmitBooleanOperator(BinaryExpression node, bool isAnd) {
            Label otherwise = _ilg.DefineLabel();
            Label endif = _ilg.DefineLabel();

            // if (_left) 
            EmitBranchFalse(node.Left, otherwise);
            // then

            if (isAnd) {
                EmitExpression(node.Right);
            } else {
                _ilg.EmitInt(1);
            }

            _ilg.Emit(OpCodes.Br, endif);
            // otherwise
            _ilg.MarkLabel(otherwise);

            if (isAnd) {
                _ilg.EmitInt(0);
            } else {
                EmitExpression(node.Right);
            }

            // endif
            _ilg.MarkLabel(endif);
            return;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void GenerateBinaryOperator(AstNodeType nodeType, Type type) {
            switch (nodeType) {
                case AstNodeType.ArrayIndex:
                    _ilg.EmitLoadElement(type);
                    break;

                case AstNodeType.Equal:
                    _ilg.Emit(OpCodes.Ceq);
                    break;

                case AstNodeType.NotEqual:
                    _ilg.Emit(OpCodes.Ceq);
                    _ilg.EmitInt(0);
                    _ilg.Emit(OpCodes.Ceq);
                    break;

                case AstNodeType.GreaterThan:
                    _ilg.Emit(OpCodes.Cgt);
                    break;

                case AstNodeType.LessThan:
                    _ilg.Emit(OpCodes.Clt);
                    break;

                case AstNodeType.GreaterThanOrEqual:
                    _ilg.Emit(OpCodes.Clt);
                    _ilg.EmitInt(0);
                    _ilg.Emit(OpCodes.Ceq);
                    break;

                case AstNodeType.LessThanOrEqual:
                    _ilg.Emit(OpCodes.Cgt);
                    _ilg.EmitInt(0);
                    _ilg.Emit(OpCodes.Ceq);
                    break;
                case AstNodeType.Multiply:
                    _ilg.Emit(OpCodes.Mul);
                    break;
                case AstNodeType.Modulo:
                    _ilg.Emit(OpCodes.Rem);
                    break;
                case AstNodeType.Add:
                    _ilg.Emit(OpCodes.Add);
                    break;
                case AstNodeType.Subtract:
                    _ilg.Emit(OpCodes.Sub);
                    break;
                case AstNodeType.Divide:
                    _ilg.Emit(OpCodes.Div);
                    break;
                case AstNodeType.LeftShift:
                    _ilg.Emit(OpCodes.Shl);
                    break;
                case AstNodeType.RightShift:
                    _ilg.Emit(OpCodes.Shr);
                    break;
                case AstNodeType.And:
                    _ilg.Emit(OpCodes.And);
                    break;
                case AstNodeType.Or:
                    _ilg.Emit(OpCodes.Or);
                    break;
                case AstNodeType.ExclusiveOr:
                    _ilg.Emit(OpCodes.Xor);
                    break;
                default:
                    throw new InvalidOperationException(nodeType.ToString());
            }
        }

        #endregion

        #region InvocationExpression

        private void Emit(InvocationExpression node) {
            if (node.IsDynamic) {
                EmitCallSite(node, ArrayUtils.Insert(node.Expression, node.Arguments));
                return;
            }

            // TODO: need a smarter implementation here
            // (inlining, support quoted lambdas, etc)
            // see ExpressionCompiler in Linq

            Emit(Expression.Call(node.Expression, node.Expression.Type.GetMethod("Invoke"), node.Arguments));
        }

        #endregion

        #region MethodCallExpression

        private void Emit(MethodCallExpression node) {
            EmitPosition(node.Start, node.End);
            if (node.IsDynamic) {
                EmitCallSite(node, ArrayUtils.Insert(node.Instance, node.Arguments));
                return;
            }

            // Emit instance, if calling an instance method

            if (!node.Method.IsStatic) {
                Type type = node.Method.DeclaringType;

                if (type.IsValueType) {
                    EmitAddress(node.Instance, type);
                } else {
                    EmitExpression(node.Instance);
                }
            }

            ParameterInfo[] parameterInfos = node.Method.GetParameters();

            // Emit arguments
            Debug.Assert(node.Arguments.Count == parameterInfos.Length);
            for (int arg = 0; arg < parameterInfos.Length; arg++) {
                Expression argument = node.Arguments[arg];
                Type type = parameterInfos[arg].ParameterType;
                EmitArgument(argument, type);
            }

            // Emit the actual call
            _ilg.EmitCall(node.Method);
        }

        private void EmitArgument(Expression argument, Type type) {
            if (type.IsByRef) {
                EmitAddress(argument, type.GetElementType());
            } else {
                EmitExpression(argument);
            }
        }

        #endregion

        private void Emit(ConditionalExpression node) {
            EmitPosition(node.Start, node.End);
            Label eoi = _ilg.DefineLabel();
            Label next = _ilg.DefineLabel();
            EmitBranchFalse(node.Test, next);
            //Emit(OpCodes.Brfalse, next);
            EmitExpression(node.IfTrue);
            EmitSequencePointNone();
            _ilg.Emit(OpCodes.Br, eoi);
            _ilg.MarkLabel(next);
            EmitExpression(node.IfFalse);
            _ilg.MarkLabel(eoi);
        }

        private void Emit(ConstantExpression node) {
            EmitConstant(node.Value);
        }

        private void Emit(UnaryExpression node) {
            EmitPosition(node.Start, node.End);

            if (node.IsDynamic) {
                EmitCallSite(node, node.Operand);
                return;
            }

            EmitExpression(node.Operand);

            switch (node.NodeType) {
                case AstNodeType.Convert:
                    EmitCast(node.Operand.Type, node.Type);
                    break;

                case AstNodeType.Not:
                    if (node.Operand.Type == typeof(bool)) {
                        _ilg.Emit(OpCodes.Ldc_I4_0);
                        _ilg.Emit(OpCodes.Ceq);
                    } else {
                        _ilg.Emit(OpCodes.Not);
                    }
                    break;
                case AstNodeType.Negate:
                    _ilg.Emit(OpCodes.Neg);
                    break;
                case AstNodeType.OnesComplement:
                    _ilg.Emit(OpCodes.Not);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void Emit(NewExpression node) {
            if (node.IsDynamic) {
                EmitCallSite(node, node.Arguments);
                return;
            }

            ReadOnlyCollection<Expression> arguments = node.Arguments;
            for (int i = 0; i < arguments.Count; i++) {
                EmitExpression(arguments[i]);
            }
            if (node.Constructor != null) {
                _ilg.EmitNew(node.Constructor);
            } else {
                Debug.Assert(arguments.Count == 0, "Node with arguments must have a constructor.");
                Debug.Assert(node.Type.IsValueType, "Only value type may have constructor not set.");

                Slot temp = _ilg.GetLocalTmp(node.Type);
                temp.EmitGetAddr(_ilg);
                _ilg.Emit(OpCodes.Initobj, node.Type);
                temp.EmitGet(_ilg);
                _ilg.FreeLocalTmp(temp);
            }
        }

        private void Emit(TypeBinaryExpression node) {
            if (node.TypeOperand.IsAssignableFrom(node.Expression.Type)) {
                // if its always true just emit the bool
                EmitConstant(true);
                return;
            }

            EmitExpressionAsObject(node.Expression);
            _ilg.Emit(OpCodes.Isinst, node.TypeOperand);
            _ilg.Emit(OpCodes.Ldnull);
            _ilg.Emit(OpCodes.Cgt_Un);
        }

        #region dynamic expressions

        private void Emit(ActionExpression node) {
            EmitPosition(node.Start, node.End);
            EmitCallSite(node, node.Arguments);
        }

        private void EmitCallSite(Expression node, params Expression[] arguments) {
            EmitCallSite(node, (IList<Expression>)arguments);
        }

        private void EmitCallSite(Expression node, IList<Expression> arguments) {
            Slot site = CreateDynamicSite(node.BindingInfo, CompilerHelpers.GetSiteTypes(arguments, node.Type));
            Type siteType = site.Type;

            PropertyInfo target = siteType.GetProperty("Target");
            MethodInfo method = target.PropertyType.GetMethod("Invoke");

            Debug.Assert(!method.IsStatic);

            // Push site for the field load
            site.EmitGet(_ilg);

            // If we have slow slot, pull into local
            Slot temp = null;
            if (!(site is LocalSlot || site is StaticFieldSlot)) {
                site = temp = _ilg.GetLocalTmp(siteType);
                _ilg.Emit(OpCodes.Dup);
                temp.EmitSet(_ilg);
            }

            // Load the "Target" field of the site - the delegate to invoke
            _ilg.EmitPropertyGet(target);

            // Emit "this" - the site
            site.EmitGet(_ilg);

            // Free the temp
            if (temp != null) {
                _ilg.FreeLocalTmp(temp);
            }

            // Do not use these slots, the have been freed
            site = temp = null;

            ParameterInfo[] parameters = method.GetParameters();

            const int first = 2;

            // Emit code context
            EmitCodeContext();

            if (parameters.Length < arguments.Count + first) {
                // tuple parameters
                Debug.Assert(parameters.Length == first + 1);

                EmitTuple(
                    DynamicSiteHelpers.GetTupleTypeFromTarget(target.PropertyType),
                    arguments.Count,
                    delegate(int index) {
                        EmitExpression(arguments[index]);
                    }
                );
            } else {
                // Emit the arguments
                for (int arg = 0; arg < arguments.Count; arg++) {
                    Debug.Assert(parameters[arg + first].ParameterType == arguments[arg].Type);
                    EmitExpression(arguments[arg]);
                }
            }

            // Emit the site invoke by invoking the Target delegate
            _ilg.EmitCall(target.PropertyType.GetMethod("Invoke"));
        }

        #endregion

        private void EmitArrayIndexAssignment(AssignmentExpression node) {
            BinaryExpression arrayIndex = (BinaryExpression)node.Expression;

            if (node.IsDynamic) {
                EmitCallSite(node, arrayIndex.Left, arrayIndex.Right, node.Value);
                return;
            }

            EmitExpression(node.Value);

            // Save the expression value - order of evaluation is different than that of the Stelem* instruction
            Slot temp = _ilg.GetLocalTmp(node.Type);
            temp.EmitSet(_ilg);

            // Emit the array reference
            EmitExpression(arrayIndex.Left);
            // Emit the index (integer)
            EmitExpression(arrayIndex.Right);
            // Emit the value
            temp.EmitGet(_ilg);
            // Store it in the array
            _ilg.EmitStoreElement(node.Type);
            temp.EmitGet(_ilg);
            _ilg.FreeLocalTmp(temp);
        }

        private void EmitVariableAssignment(AssignmentExpression node) {
            EmitPosition(node.Start, node.End);
            if (TypeUtils.IsNullableType(node.Type)) {
                // Nullable<T> being assigned...
                if (ConstantCheck.IsConstant(node.Value, null)) {
                    _info.ReferenceSlots[node.Expression].EmitGetAddr(_ilg);
                    _ilg.Emit(OpCodes.Initobj, node.Type);
                    _info.ReferenceSlots[node.Expression].EmitGet(_ilg);
                    return;
                } else if (node.Type != node.Value.Type) {
                    throw new InvalidOperationException();
                }
                // fall through & emit the store from Nullable<T> -> Nullable<T>
            }
            EmitExpression(node.Value);
            _ilg.Emit(OpCodes.Dup);
            _info.ReferenceSlots[node.Expression].EmitSet(_ilg);
        }

        private void Emit(AssignmentExpression node) {
            switch (node.Expression.NodeType) {
                case AstNodeType.ArrayIndex:
                    EmitArrayIndexAssignment(node);
                    return;
                case AstNodeType.MemberExpression:
                    EmitMemberAssignment(node);
                    return;
                case AstNodeType.Parameter:
                case AstNodeType.LocalVariable:
                case AstNodeType.GlobalVariable:
                case AstNodeType.TemporaryVariable:
                    EmitVariableAssignment(node);
                    return;
                default:
                    throw new InvalidOperationException("Invalid lvalue for assignment: " + node.Expression.NodeType);
            }
        }

        private void Emit(VariableExpression node) {
            _info.ReferenceSlots[node].EmitGet(_ilg);
        }

        private void Emit(ParameterExpression node) {
            _info.ReferenceSlots[node].EmitGet(_ilg);
        }

        private void Emit(LambdaExpression node) {
            EmitDelegateConstruction(node, node.Type);
        }

        // Emit the generator intrinsic arg used in a GeneratorLambdaExpression.
        private void EmitGeneratorIntrinsic() {
            // This is coupled to the codegen in GeneratorLambdaExpression, 
            // which always uses the 1st arg.
            GetLambdaArgumentSlot(0).EmitGet(_ilg);
        }

        internal void EmitCodeContext() {
            if (ContextSlot == null) {
                throw new InvalidOperationException("ContextSlot not available.");
            }

            ContextSlot.EmitGet(_ilg);
        }

        private void EmitMemberAssignment(AssignmentExpression node) {
            MemberExpression lvalue = (MemberExpression)node.Expression;

            if (node.IsDynamic) {
                EmitCallSite(node, lvalue.Expression, node.Value);
                return;
            }

            // emit "this", if any
            EmitInstance(lvalue.Expression, lvalue.Member.DeclaringType);

            // emit value
            EmitExpression(node.Value);

            // save the value so we can return it
            _ilg.Emit(OpCodes.Dup);
            Slot temp = _ilg.GetLocalTmp(node.Type);
            temp.EmitSet(_ilg);

            switch (lvalue.Member.MemberType) {
                case MemberTypes.Field:
                    _ilg.EmitFieldSet((FieldInfo)lvalue.Member);
                    break;
                case MemberTypes.Property:
                    _ilg.EmitPropertySet((PropertyInfo)lvalue.Member);
                    break;
                default:
                    throw new InvalidOperationException("Invalid member type: " + lvalue.Member.MemberType);
            }

            temp.EmitGet(_ilg);
            _ilg.FreeLocalTmp(temp);
        }

        private void Emit(MemberExpression node) {
            if (node.IsDynamic) {
                EmitCallSite(node, node.Expression);
                return;
            }

            // emit "this", if any
            EmitInstance(node.Expression, node.Member.DeclaringType);

            switch (node.Member.MemberType) {
                case MemberTypes.Field:
                    _ilg.EmitFieldGet((FieldInfo)node.Member);
                    break;
                case MemberTypes.Property:
                    _ilg.EmitPropertyGet((PropertyInfo)node.Member);
                    break;
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }
        }

        private void EmitInstance(Expression instance, Type type) {
            if (instance != null) {
                if (type.IsValueType) {
                    EmitAddress(instance, type);
                } else {
                    EmitExpression(instance);
                }
            }
        }

        private void Emit(NewArrayExpression node) {
            if (node.NodeType == AstNodeType.NewArrayExpression) {
                _ilg.EmitArray(
                    node.Type.GetElementType(),
                    node.Expressions.Count,
                    delegate(int index) {
                        EmitExpression(node.Expressions[index]);
                    }
                );
            } else {
                ReadOnlyCollection<Expression> bounds = node.Expressions;
                for (int i = 0; i < bounds.Count; i++) {
                    EmitExpression(bounds[i]);
                }
                _ilg.EmitArray(node.Type);
            }
        }

        #region Expression helpers

        private void EmitExpressionAsObjectOrNull(Expression node) {
            if (node == null) {
                _ilg.Emit(OpCodes.Ldnull);
            } else {
                EmitExpressionAsObject(node);
            }
        }


        private void EmitExpressionAndPop(Expression node) {
            EmitExpression(node);
            if (node.Type != typeof(void)) {
                _ilg.Emit(OpCodes.Pop);
            }
        }

        #endregion
    }
}
