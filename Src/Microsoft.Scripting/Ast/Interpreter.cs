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

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class Interpreter {

        #region Entry points

        public static object Evaluate(CodeContext context, Expression expression) {
            object result = EvaluateExpression(context, expression);

            if (result is ControlFlow) {
                throw new InvalidOperationException("Invalid expression");
            }

            return result;
        }

        public static object Execute(CodeContext context, Expression expression) {
            ControlFlow result = EvaluateExpression(context, expression) as ControlFlow;

            if (result != null && result.Kind == ControlFlowKind.Return) {
                return result.Value;
            }

            return null;
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static object EvaluateExpression(CodeContext context, Expression node) {
            Debug.Assert(node != null);

            switch (node.NodeType) {
                case AstNodeType.AndAlso:
                    return EvaluateAndAlso(context, (BinaryExpression)node);
                case AstNodeType.OrElse:
                    return EvaluateOrElse(context, (BinaryExpression)node);
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
                    return EvaluateBinary(context, (BinaryExpression)node);
                case AstNodeType.Call:
                    return EvaluateMethodCall(context, (MethodCallExpression)node);
                case AstNodeType.Conditional:
                    return EvaluateConditional(context, (ConditionalExpression)node);
                case AstNodeType.Constant:
                    return EvaluateConstant((ConstantExpression)node);
                case AstNodeType.Convert:
                case AstNodeType.Negate:
                case AstNodeType.Not:
                case AstNodeType.OnesComplement:
                    return EvaluateUnary(context, (UnaryExpression)node);
                case AstNodeType.New:
                    return EvaluateNew(context, (NewExpression)node);
                case AstNodeType.TypeIs:
                    return EvaluateTypeBinary(context, (TypeBinaryExpression)node);
                case AstNodeType.ActionExpression:
                    return EvaluateAction(context, (ActionExpression)node);
                case AstNodeType.ArrayIndexAssignment:
                    return EvaluateArrayIndex(context, (ArrayIndexAssignment)node);
                case AstNodeType.BoundAssignment:
                    return EvaluateBound(context, (BoundAssignment)node);
                case AstNodeType.BoundExpression:
                    return EvaluateBoundAssignment(context, (BoundExpression)node);
                case AstNodeType.CodeBlockExpression:
                    return EvaluateCodeBlock(context, (CodeBlockExpression)node);
                case AstNodeType.CodeContextExpression:
                    return EvaluateCodeContext(context);
                case AstNodeType.DeleteUnboundExpression:
                    return EvaluateDeleteUnbound(context, (DeleteUnboundExpression)node);
                case AstNodeType.EnvironmentExpression:
                    return NotImplemented();
                case AstNodeType.MemberAssignment:
                    return EvaluateMemberAssignment(context, (MemberAssignment)node);
                case AstNodeType.MemberExpression:
                    return EvaluateMember(context, (MemberExpression)node);
                case AstNodeType.NewArrayExpression:
                    return EvaluateNewArray(context, (NewArrayExpression)node);
                case AstNodeType.ParamsExpression:
                    return NotImplemented();
                case AstNodeType.UnboundAssignment:
                    return EvaluateUnboundAssignment(context, (UnboundAssignment)node);
                case AstNodeType.UnboundExpression:
                    return EvaluateUnbound(context, (UnboundExpression)node);

                // Statements
                case AstNodeType.Block:
                    return ExecuteBlock(context, (Block)node);
                case AstNodeType.BreakStatement:
                    return ExecuteBreak(context, (BreakStatement)node);
                case AstNodeType.ContinueStatement:
                    return ExecuteContinue(context, (ContinueStatement)node);
                case AstNodeType.DeleteStatement:
                    return ExecuteDelete(context, (DeleteStatement)node);
                case AstNodeType.DoStatement:
                    return ExecuteDo(context, (DoStatement)node);
                case AstNodeType.EmptyStatement:
                    return ExecuteEmpty(context, (EmptyStatement)node);
                case AstNodeType.ExpressionStatement:
                    return ExecuteExpression(context, (ExpressionStatement)node);
                case AstNodeType.LabeledStatement:
                    return ExecuteLabeled(context, (LabeledStatement)node);
                case AstNodeType.LoopStatement:
                    return ExecuteLoop(context, (LoopStatement)node);
                case AstNodeType.ReturnStatement:
                    return ExecuteReturn(context, (ReturnStatement)node);
                case AstNodeType.ScopeStatement:
                    return ExecuteScope(context, (ScopeStatement)node);
                case AstNodeType.SwitchStatement:
                    return ExecuteSwitch(context, (SwitchStatement)node);
                case AstNodeType.ThrowStatement:
                    return ExecuteThrow(context, (ThrowStatement)node);
                case AstNodeType.TryStatement:
                    return ExecuteTry(context, (TryStatement)node);
                case AstNodeType.YieldStatement:
                    return NotImplemented();
                default:
                    throw new InvalidOperationException();
            }
        }


        /// <summary>
        /// Evaluates expression and checks it for ControlFlow. If it is control flow, returns true,
        /// otherwise returns false.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="node"></param>
        /// <param name="result">Result of the evaluation</param>
        /// <returns>true if control flow, false if not</returns>
        private static bool EvaluateAndCheckFlow(CodeContext context, Expression node, out object result) {
            result = EvaluateExpression(context, node);
            return result is ControlFlow;
        }

        // Individual expressions and statements

        private static object EvaluateConstant(ConstantExpression node) {
            CompilerConstant cc = node.Value as CompilerConstant;
            if (cc != null) {
                return cc.Create(); // TODO: Only create once?
            }

            return node.Value;
        }

        private static object EvaluateConditional(CodeContext context, ConditionalExpression node) {
            object test;

            if (EvaluateAndCheckFlow(context, node.Test, out test)) {
                return test;
            }

            if ((bool)test) {
                return EvaluateExpression(context, node.IfTrue);
            } else {
                return EvaluateExpression(context, node.IfFalse);
            }
        }

        private static bool IsInputParameter(ParameterInfo pi) {
            return !pi.IsOut || (pi.Attributes & ParameterAttributes.In) != 0;
        }

        private static object InvokeMethod(MethodInfo method, object instance, object[] parameters) {
            // TODO: Cache !!!
            ReflectedCaller _caller = null;

            if (_caller == null) {
                _caller = ReflectedCaller.Create(method);
            }
            if (instance == null) {
                return _caller.Invoke(parameters);
            } else {
                return _caller.InvokeInstance(instance, parameters);
            }
        }

        private static object EvaluateMethodCall(CodeContext context, MethodCallExpression node) {
            object instance = null;
            // Evaluate the instance first (if the method is non-static)
            if (!node.Method.IsStatic) {
                if (EvaluateAndCheckFlow(context, node.Instance, out instance)) {
                    return instance;
                }
            }

            object[] parameters = new object[node.ParameterInfos.Length];
            EvaluationAddress[] paramAddrs = new EvaluationAddress[node.ParameterInfos.Length];
            if (node.ParameterInfos.Length > 0) {
                int last = parameters.Length;
                for (int i = 0; i < last; i++) {
                    ParameterInfo pi = node.ParameterInfos[i];

                    if (pi.ParameterType.IsByRef) {
                        paramAddrs[i] = EvaluateAddress(context, node.Arguments[i]);

                        object value = paramAddrs[i].GetValue(context, !IsInputParameter(node.ParameterInfos[i]));
                        if (IsInputParameter(node.ParameterInfos[i])) {
                            parameters[i] = context.LanguageContext.Binder.Convert(
                                value,
                                node.ParameterInfos[i].ParameterType.GetElementType()
                            );
                        }
                    } else if (IsInputParameter(node.ParameterInfos[i])) {
                        Expression arg = node.Arguments[i];
                        object argValue = null;
                        if (arg != null) {
                            if (EvaluateAndCheckFlow(context, arg, out argValue)) {
                                return argValue;
                            }
                        }
                        parameters[i] = argValue;
                    }
                }
            }

            try {
                object res;
                try {
                    // Call the method
                    res = InvokeMethod(node.Method, instance, parameters);

                    // Return the singleton True or False object
                    if (node.Type == typeof(Boolean)) {
                        res = RuntimeHelpers.BooleanToObject((bool)res);
                    }
                } finally {
                    // expose by-ref args
                    for (int i = 0; i < node.ParameterInfos.Length; i++) {
                        if (node.ParameterInfos[i].ParameterType.IsByRef) {
                            paramAddrs[i].AssignValue(context, parameters[i]);
                        }
                    }
                }

                // back propagate instance on value types if the instance supports it.
                if (node.Method.DeclaringType != null && node.Method.DeclaringType.IsValueType && !node.Method.IsStatic) {
                    EvaluateAssign(context, node.Instance, instance);
                }

                return res;
            } catch (TargetInvocationException e) {
                // Unwrap the real (inner) exception and raise it
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        private static object EvaluateAndAlso(CodeContext context, BinaryExpression node) {
            object ret;
            if (EvaluateAndCheckFlow(context, node.Left, out ret)) {
                return ret;
            }
            return ((bool)ret) ? EvaluateExpression(context, node.Right) : ret;
        }

        private static object EvaluateOrElse(CodeContext context, BinaryExpression node) {
            object ret;
            if (EvaluateAndCheckFlow(context, node.Left, out ret)) {
                return ret;
            }
            return ((bool)ret) ? ret : EvaluateExpression(context, node.Right);
        }

        private static object EvaluateBinary(CodeContext context, BinaryExpression node) {
            object left, right;

            if (EvaluateAndCheckFlow(context, node.Left, out left)) {
                return left;
            }
            if (EvaluateAndCheckFlow(context, node.Right, out right)) {
                return right;
            }

            if (node.Method != null) {
                return node.Method.Invoke(null, new object[] { left, right });
            } else {
                return EvaluateBinaryOperator(node.NodeType, left, right);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static object EvaluateBinaryOperator(AstNodeType nodeType, object l, object r) {
            switch (nodeType) {
                case AstNodeType.ArrayIndex:
                    Array array = (Array)l;
                    int index = (int)r;
                    return array.GetValue(index);

                case AstNodeType.GreaterThan:
                    return RuntimeHelpers.BooleanToObject(((IComparable)l).CompareTo(r) > 0);
                case AstNodeType.LessThan:
                    return RuntimeHelpers.BooleanToObject(((IComparable)l).CompareTo(r) < 0);
                case AstNodeType.GreaterThanOrEqual:
                    return RuntimeHelpers.BooleanToObject(((IComparable)l).CompareTo(r) >= 0);
                case AstNodeType.LessThanOrEqual:
                    return RuntimeHelpers.BooleanToObject(((IComparable)l).CompareTo(r) <= 0);
                case AstNodeType.Equal:
                    return RuntimeHelpers.BooleanToObject(TestEquals(l, r));

                case AstNodeType.NotEqual:
                    return RuntimeHelpers.BooleanToObject(!TestEquals(l, r));

                case AstNodeType.Multiply:
                    return EvalMultiply(l, r);
                case AstNodeType.Add:
                    return EvalAdd(l, r);
                case AstNodeType.Subtract:
                    return EvalSub(l, r);
                case AstNodeType.Divide:
                    return EvalDiv(l, r);
                case AstNodeType.Modulo:
                    return EvalMod(l, r);
                case AstNodeType.And:
                    return EvalAnd(l, r);
                case AstNodeType.Or:
                    return EvalOr(l, r);
                case AstNodeType.ExclusiveOr:
                    return EvalXor(l, r);
                default:
                    throw new NotImplementedException(nodeType.ToString());
            }
        }

        private static object EvalMultiply(object l, object r) {
            if (l is int) return (int)l * (int)r;
            if (l is uint) return (uint)l * (uint)r;
            if (l is short) return (short)((short)l * (short)r);
            if (l is ushort) return (ushort)((ushort)l * (ushort)r);
            if (l is long) return (long)l * (long)r;
            if (l is ulong) return (ulong)l * (ulong)r;
            if (l is float) return (float)l * (float)r;
            if (l is double) return (double)l * (double)r;
            throw new InvalidOperationException("multiply: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalAdd(object l, object r) {
            if (l is int) return (int)l + (int)r;
            if (l is uint) return (uint)l + (uint)r;
            if (l is short) return (short)((short)l + (short)r);
            if (l is ushort) return (ushort)((ushort)l + (ushort)r);
            if (l is long) return (long)l + (long)r;
            if (l is ulong) return (ulong)l + (ulong)r;
            if (l is float) return (float)l + (float)r;
            if (l is double) return (double)l + (double)r;
            throw new InvalidOperationException("add: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalSub(object l, object r) {
            if (l is int) return (int)l - (int)r;
            if (l is uint) return (uint)l - (uint)r;
            if (l is short) return (short)((short)l - (short)r);
            if (l is ushort) return (ushort)((ushort)l - (ushort)r);
            if (l is long) return (long)l - (long)r;
            if (l is ulong) return (ulong)l - (ulong)r;
            if (l is float) return (float)l - (float)r;
            if (l is double) return (double)l - (double)r;
            throw new InvalidOperationException("sub: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalMod(object l, object r) {
            if (l is int) return (int)l % (int)r;
            if (l is uint) return (uint)l % (uint)r;
            if (l is short) return (short)((short)l % (short)r);
            if (l is ushort) return (ushort)((ushort)l % (ushort)r);
            if (l is long) return (long)l % (long)r;
            if (l is ulong) return (ulong)l % (ulong)r;
            if (l is float) return (float)l % (float)r;
            if (l is double) return (double)l % (double)r;
            throw new InvalidOperationException("mod: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalDiv(object l, object r) {
            if (l is int) return (int)l / (int)r;
            if (l is uint) return (uint)l / (uint)r;
            if (l is short) return (short)((short)l / (short)r);
            if (l is ushort) return (ushort)((ushort)l / (ushort)r);
            if (l is long) return (long)l / (long)r;
            if (l is ulong) return (ulong)l / (ulong)r;
            if (l is float) return (float)l / (float)r;
            if (l is double) return (double)l / (double)r;
            throw new InvalidOperationException("div: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalAnd(object l, object r) {
            if (l is int) return (int)l & (int)r;
            if (l is uint) return (uint)l & (uint)r;
            if (l is short) return (short)((short)l & (short)r);
            if (l is ushort) return (ushort)((ushort)l & (ushort)r);
            if (l is long) return (long)l & (long)r;
            if (l is ulong) return (ulong)l & (ulong)r;
            throw new InvalidOperationException("and: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalOr(object l, object r) {
            if (l is int) return (int)l | (int)r;
            if (l is uint) return (uint)l | (uint)r;
            if (l is short) return (short)((short)l | (short)r);
            if (l is ushort) return (ushort)((ushort)l | (ushort)r);
            if (l is long) return (long)l | (long)r;
            if (l is ulong) return (ulong)l | (ulong)r;
            throw new InvalidOperationException("or: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalXor(object l, object r) {
            if (l is int) return (int)l ^ (int)r;
            if (l is uint) return (uint)l ^ (uint)r;
            if (l is short) return (short)((short)l ^ (short)r);
            if (l is ushort) return (ushort)((ushort)l ^ (ushort)r);
            if (l is long) return (long)l ^ (long)r;
            if (l is ulong) return (ulong)l ^ (ulong)r;
            throw new InvalidOperationException("xor: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static bool TestEquals(object l, object r) {
            // We don't need to go through the same type checks as the emit case,
            // since we know we're always dealing with boxed objects.

            return Object.Equals(l, r);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static object EvaluateUnary(CodeContext context, UnaryExpression node) {
            object value;
            if (EvaluateAndCheckFlow(context, node.Operand, out value)) {
                return value;
            }

            switch (node.NodeType) {
                case AstNodeType.Convert:
                    if (node.Type == typeof(void)) {
                        return null;
                    }
                    return Cast.Explicit(value, node.Type);

                case AstNodeType.Not:
                    if (value is bool) return (bool)value ? RuntimeHelpers.False : RuntimeHelpers.True;
                    if (value is int) return (int)~(int)value;
                    if (value is long) return (long)~(long)value;
                    if (value is short) return (short)~(short)value;
                    if (value is uint) return (uint)~(uint)value;
                    if (value is ulong) return (ulong)~(ulong)value;
                    if (value is ushort) return (ushort)~(ushort)value;
                    if (value is byte) return (byte)~(byte)value;
                    if (value is sbyte) return (sbyte)~(sbyte)value;
                    throw new InvalidOperationException("can't perform unary not on type " + CompilerHelpers.GetType(value).Name);

                case AstNodeType.Negate:
                    if (value is int) return (int)(-(int)value);
                    if (value is long) return (long)(-(long)value);
                    if (value is short) return (short)(-(short)value);
                    if (value is float) return -(float)value;
                    if (value is double) return -(double)value;
                    throw new InvalidOperationException("can't negate type " + CompilerHelpers.GetType(value).Name);

                default:
                    throw new NotImplementedException();
            }
        }

        private static object EvaluateNew(CodeContext context, NewExpression node) {
            object[] args = new object[node.Arguments.Count];
            for (int i = 0; i < node.Arguments.Count; i++) {
                object argValue;
                if (EvaluateAndCheckFlow(context, node.Arguments[i], out argValue)) {
                    return argValue;
                }
                args[i] = argValue;
            }
            try {
                return node.Constructor.Invoke(args);
            } catch (TargetInvocationException e) {
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        private static object EvaluateTypeBinary(CodeContext context, TypeBinaryExpression node) {
            object value;
            if (EvaluateAndCheckFlow(context, node.Expression, out value)) {
                return value;
            }
            return RuntimeHelpers.BooleanToObject(
                node.TypeOperand.IsInstanceOfType(value)
            );
        }

        private static object EvaluateAction(CodeContext context, ActionExpression node) {
            object[] args = new object[node.Arguments.Count];
            for (int i = 0; i < node.Arguments.Count; i++) {
                object argValue;
                if (EvaluateAndCheckFlow(context, node.Arguments[i], out argValue)) {
                    return argValue;
                }
                args[i] = argValue;
            }

            return context.LanguageContext.Binder.Execute(
                context,
                node.Action,
                CompilerHelpers.GetSiteTypes(node),
                args
            );
        }

        private static object EvaluateArrayIndex(CodeContext context, ArrayIndexAssignment node) {
            object value, array, index;

            // evaluate the value first
            if (EvaluateAndCheckFlow(context, node.Value, out value)) {
                return value;
            }
            if (EvaluateAndCheckFlow(context, node.Array, out array)) {
                return array;
            }
            if (EvaluateAndCheckFlow(context, node.Index, out index)) {
                return index;
            }
            ((Array)array).SetValue(value, (int)index);
            return value;
        }

        private static object EvaluateBound(CodeContext context, BoundAssignment node) {
            object value;
            if (EvaluateAndCheckFlow(context, node.Value, out value)) {
                return value;
            }
            EvaluateAssignVariable(context, node.Variable, value);
            return value;
        }

        private static object EvaluateBoundAssignment(CodeContext context, BoundExpression node) {
            object ret;
            switch (node.Variable.Kind) {
                case Variable.VariableKind.Temporary:
                    if (!context.Scope.TemporaryStorage.TryGetValue(node.Variable, out ret)) {
                        throw context.LanguageContext.MissingName(node.Variable.Name);
                    } else {
                        return ret;
                    }
                case Variable.VariableKind.Parameter:
                    // This is sort of ugly: parameter variables can be stored either as locals or as temporaries (in case of $argn).
                    if (!context.Scope.TemporaryStorage.TryGetValue(node.Variable, out ret) || ret == Uninitialized.Instance) {
                        return RuntimeHelpers.LookupName(context, node.Variable.Name);
                    } else {
                        return ret;
                    }
                case Variable.VariableKind.Global:
                    return RuntimeHelpers.LookupGlobalName(context, node.Variable.Name);
                default:
                    if (!context.LanguageContext.TryLookupName(context, node.Variable.Name, out ret)) {
                        throw context.LanguageContext.MissingName(node.Variable.Name);
                    } else if (ret == Uninitialized.Instance) {
                        RuntimeHelpers.ThrowUnboundLocalError(node.Variable.Name);
                        return null;
                    } else {
                        return ret;
                    }
            }
        }

        private static object EvaluateCodeBlock(CodeContext context, CodeBlockExpression node) {
            return GetDelegateForInterpreter(node.Block, context, node.DelegateType, node.ForceWrapperMethod);
        }

        private static object EvaluateCodeContext(CodeContext context) {
            return context;
        }

        private static object EvaluateDeleteUnbound(CodeContext context, DeleteUnboundExpression node) {
            return RuntimeHelpers.RemoveName(context, node.Name);
        }

        private static object EvaluateMemberAssignment(CodeContext context, MemberAssignment node) {
            object target = null, value;
            if (node.Expression != null) {
                if (EvaluateAndCheckFlow(context, node.Expression, out target)) {
                    return target;
                }
            }
            if (EvaluateAndCheckFlow(context, node.Value, out value)) {
                return value;
            }

            switch (node.Member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = (FieldInfo)node.Member;
                    field.SetValue(target, value);
                    break;
                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)node.Member;
                    property.SetValue(target, value, null);
                    break;
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }
            return null;
        }

        private static object EvaluateMember(CodeContext context, MemberExpression node) {
            object self = null;
            if (node.Expression != null) {
                if (EvaluateAndCheckFlow(context, node.Expression, out self)) {
                    return self;
                }
            }
            switch (node.Member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = (FieldInfo)node.Member;
                    return field.GetValue(self);
                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)node.Member;
                    return property.GetValue(self, Utils.ArrayUtils.EmptyObjects);
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }

            throw new InvalidOperationException();
        }

        private static object EvaluateNewArray(CodeContext context, NewArrayExpression node) {
            if (node.Type.GetElementType().IsValueType) {
                // value arrays cannot be cast to object arrays
                object contents = (object)node.Constructor.Invoke(new object[] { node.Expressions.Count });
                MethodInfo setter = node.Type.GetMethod("Set");
                for (int i = 0; i < node.Expressions.Count; i++) {
                    object value;
                    if (EvaluateAndCheckFlow(context, node.Expressions[i], out value)) {
                        return value;
                    }
                    setter.Invoke(contents, new object[] { i, value });
                }
                return contents;
            } else {
                object[] contents = (object[])node.Constructor.Invoke(new object[] { node.Expressions.Count });
                for (int i = 0; i < node.Expressions.Count; i++) {
                    object value;
                    if (EvaluateAndCheckFlow(context, node.Expressions[i], out value)) {
                        return value;
                    }
                    contents[i] = value;
                }
                return contents;
            }
        }

        private static object EvaluateUnboundAssignment(CodeContext context, UnboundAssignment node) {
            object value;
            if (EvaluateAndCheckFlow(context, node.Value, out value)) {
                return value;
            }
            RuntimeHelpers.SetName(context, node.Name, value);
            return value;
        }

        private static object EvaluateUnbound(CodeContext context, UnboundExpression node) {
            return RuntimeHelpers.LookupName(context, node.Name);
        }

        private static object ExecuteBlock(CodeContext context, Block node) {
            context.Scope.SourceLocation = node.Start;

            object result = ControlFlow.NextStatement;
            for (int index = 0; index < node.Expressions.Count; index++) {
                Expression current = node.Expressions[index];

                object val;
                if (EvaluateAndCheckFlow(context, current, out val)) {
                    if (val != ControlFlow.NextStatement) {
                        return val;
                    }
                }

                if (index == node.Expressions.Count - 1 && node.Type != typeof(void)) {
                    // Save the value at the designated index
                    result = val;
                }
            }
            return result;
        }

        private static object ExecuteBreak(CodeContext context, BreakStatement node) {
            context.Scope.SourceLocation = node.Start;
            return ControlFlow.Break;
        }

        private static object ExecuteContinue(CodeContext context, ContinueStatement node) {
            context.Scope.SourceLocation = node.Start;
            return ControlFlow.Continue;
        }

        // TODO: Should not be name-based.
        private static object ExecuteDelete(CodeContext context, DeleteStatement node) {
            context.Scope.SourceLocation = node.Start;
            switch (node.Variable.Kind) {
                case Variable.VariableKind.Temporary:
                    context.Scope.TemporaryStorage.Remove(node.Variable);
                    break;
                case Variable.VariableKind.Global:
                    RuntimeHelpers.RemoveGlobalName(context, node.Variable.Name);
                    break;
                default:
                    RuntimeHelpers.RemoveName(context, node.Variable.Name);
                    break;
            }

            return ControlFlow.NextStatement;
        }

        private static object ExecuteDo(CodeContext context, DoStatement node) {
            context.Scope.SourceLocation = node.Start;

            for (; ; ) {
                ControlFlow cf;

                object ret = EvaluateExpression(context, node.Body);

                if ((cf = ret as ControlFlow) != null) {
                    if (cf == ControlFlow.Break) {
                        break;
                    } else if (cf.Kind == ControlFlowKind.Return) {
                        return ret;
                    }
                }

                ret = EvaluateExpression(context, node.Test);
                if ((cf = ret as ControlFlow) != null) {
                    if (cf == ControlFlow.Break) {
                        break;
                    } else if (cf.Kind == ControlFlowKind.Return) {
                        return ret;
                    }
                }

                // Check the condition
                if (!(bool)ret) {
                    break;
                }
            }

            return ControlFlow.NextStatement;
        }

        private static object ExecuteEmpty(CodeContext context, EmptyStatement node) {
            context.Scope.SourceLocation = node.Start;
            return ControlFlow.NextStatement;
        }

        private static object ExecuteExpression(CodeContext context, ExpressionStatement node) {
            context.Scope.SourceLocation = node.Start;
            object value;
            if (EvaluateAndCheckFlow(context, node.Expression, out value)) {
                return value;
            }
            return ControlFlow.NextStatement;
        }

        private static object ExecuteLabeled(CodeContext context, LabeledStatement node) {
            context.Scope.SourceLocation = node.Start;
            throw new NotImplementedException();
        }

        private static object ExecuteLoop(CodeContext context, LoopStatement node) {
            context.Scope.SourceLocation = node.Start;

            for (; ; ) {
                ControlFlow cf;

                if (node.Test != null) {
                    object test = EvaluateExpression(context, node.Test);
                    if ((cf = test as ControlFlow) != null) {
                        if (cf == ControlFlow.Break) {
                            // Break out of the loop and execute next statement outside
                            return ControlFlow.NextStatement;
                        } else if (cf.Kind == ControlFlowKind.Return) {
                            return test;
                        }
                    }

                    // Test is false, break the loop
                    if (!(bool)test) {
                        break;
                    }
                }

                object body = EvaluateExpression(context, node.Body);
                if ((cf = body as ControlFlow) != null) {
                    if (cf == ControlFlow.Break) {
                        // Break out of the loop and execute next statement outside
                        return ControlFlow.NextStatement;
                    } else if (cf.Kind == ControlFlowKind.Return) {
                        return body;
                    }
                }

                if (node.Increment != null) {
                    object increment = EvaluateExpression(context, node.Increment);
                    if ((cf = increment as ControlFlow) != null) {
                        if (cf == ControlFlow.Break) {
                            // Break out of the loop and execute next statement outside
                            return ControlFlow.NextStatement;
                        } else if (cf.Kind == ControlFlowKind.Return) {
                            return increment;
                        }
                    }
                }
            }

            if (node.ElseStatement != null) {
                return EvaluateExpression(context, node.ElseStatement);
            }

            return ControlFlow.NextStatement;
        }

        private static object ExecuteReturn(CodeContext context, ReturnStatement node) {
            context.Scope.SourceLocation = node.Start;
            object value = null;
            if (node.Expression != null) {
                value = EvaluateExpression(context, node.Expression);
                ControlFlow cf = value as ControlFlow;
                if (cf != null) {
                    // propagate
                    return cf;
                }
            }

            return ControlFlow.Return(value);
        }

        private static object ExecuteScope(CodeContext context, ScopeStatement node) {
            context.Scope.SourceLocation = node.Start;
            CodeContext scopeContext;
            ControlFlow cf;

            // TODO: should work with LocalScope
            if (node.Scope != null) {
                object scope = EvaluateExpression(context, node.Scope);
                if ((cf = scope as ControlFlow) != null) {
                    if (cf.Kind == ControlFlowKind.Return) {
                        return cf;
                    }
                }
                scopeContext = RuntimeHelpers.CreateNestedCodeContext(scope as IAttributesCollection, context, true);
            } else {
                scopeContext = RuntimeHelpers.CreateCodeContext(context);
            }

            object body;
            if (EvaluateAndCheckFlow(scopeContext, node.Body, out body)) {
                return body;
            }

            // Eat the value and return "next"
            return ControlFlow.NextStatement;
        }

        private static object ExecuteSwitch(CodeContext context, SwitchStatement node) {
            context.Scope.SourceLocation = node.Start;

            object testValue;
            if (EvaluateAndCheckFlow(context, node.TestValue, out testValue)) {
                return testValue;
            }

            int test = (int)testValue;
            ReadOnlyCollection<SwitchCase> cases = node.Cases;
            int target = 0;
            while (target < cases.Count) {
                SwitchCase sc = cases[target];
                if (sc.IsDefault || sc.Value == test) {
                    break;
                }

                target++;
            }

            while (target < cases.Count) {
                SwitchCase sc = cases[target];
                object result = EvaluateExpression(context, sc.Body);

                ControlFlow cf = result as ControlFlow;
                if (cf == ControlFlow.Continue) {
                    return cf;
                } else if (cf == ControlFlow.Break) {
                    return ControlFlow.NextStatement;
                } else if (cf.Kind == ControlFlowKind.Return) {
                    return cf;
                }
                target++;
            }

            return ControlFlow.NextStatement;
        }

        #region Exceptions

        [ThreadStatic]
        private static List<Exception> _evalExceptions;

        private static void PopEvalException() {
            _evalExceptions.RemoveAt(_evalExceptions.Count - 1);
            if (_evalExceptions.Count == 0) _evalExceptions = null;
        }

        private static void PushEvalException(Exception exc) {
            if (_evalExceptions == null) _evalExceptions = new List<Exception>();
            _evalExceptions.Add(exc);
        }

        private static Exception LastEvalException {
            get {
                if (_evalExceptions == null || _evalExceptions.Count == 0) {
                    throw new InvalidOperationException("rethrow outside of catch block");
                }

                return _evalExceptions[_evalExceptions.Count - 1];
            }
        }

        private static object ExecuteThrow(CodeContext context, ThrowStatement node) {
            if (node.Value == null) {
                throw LastEvalException;
            } else {
                object exception;
                if (EvaluateAndCheckFlow(context, node.Exception, out exception)) {
                    return exception;
                }

                throw (Exception)exception;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses")]
        private static object ExecuteTry(CodeContext context, TryStatement node) {
            bool rethrow = false;
            Exception savedExc = null;
            object ret = ControlFlow.NextStatement;

            try {
                if (EvaluateAndCheckFlow(context, node.Body, out ret)) {
                    return ret;
                }
                ret = ControlFlow.NextStatement;
            } catch (Exception exc) {
                rethrow = true;
                savedExc = exc;
                if (node.Handlers != null) {
                    PushEvalException(exc);
                    try {
                        foreach (CatchBlock handler in node.Handlers) {
                            if (handler.Test.IsInstanceOfType(exc)) {
                                rethrow = false;
                                if (handler.Variable != null) {
                                    EvaluateAssignVariable(context, handler.Variable, exc);
                                }

                                object body;
                                if (EvaluateAndCheckFlow(context, handler.Body, out body)) {
                                    ret = body;
                                } else {
                                    ret = ControlFlowKind.NextStatement;
                                }
                                break;
                            }
                        }
                    } finally {
                        PopEvalException();
                    }
                }
            } finally {
                if (node.FinallyStatement != null) {
                    object finallyRet = EvaluateExpression(context, node.FinallyStatement);
                    if (finallyRet != ControlFlow.NextStatement) {
                        ret = finallyRet;
                        rethrow = false;
                    }
                }
                if (rethrow) {
                    throw ExceptionHelpers.UpdateForRethrow(savedExc);
                }
            }

            return ret;
        }

        #endregion

        internal static object EvaluateAssign(CodeContext context, Expression node, object value) {
            switch (node.NodeType) {
                case AstNodeType.BoundExpression:
                    return EvaluateAssign(context, (BoundExpression)node, value);
                case AstNodeType.BoundAssignment:
                    return EvaluateAssign(context, (BoundAssignment)node, value);
                case AstNodeType.MemberExpression:
                    return EvaluateAssign(context, (MemberExpression)node, value);
                default:
                    return value;
            }
        }

        private static object EvaluateAssign(CodeContext context, BoundExpression node, object value) {
            return EvaluateAssignVariable(context, node.Variable, value);
        }

        private static object EvaluateAssign(CodeContext context, BoundAssignment node, object value) {
            return EvaluateAssignVariable(context, node.Variable, value);
        }

        private static object EvaluateAssignVariable(CodeContext context, Variable var, object value) {
            switch (var.Kind) {
                case Variable.VariableKind.Temporary:
                    context.Scope.TemporaryStorage[var] = value;
                    break;
                case Variable.VariableKind.Global:
                    RuntimeHelpers.SetGlobalName(context, var.Name, value);
                    break;
                default:
                    RuntimeHelpers.SetName(context, var.Name, value);
                    break;
            }
            return value;
        }

        private static object EvaluateAssign(CodeContext context, MemberExpression node, object value) {
            object self = null;
            if (EvaluateAndCheckFlow(context, node.Expression, out self)) {
                return self;
            }
            switch (node.Member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = (FieldInfo)node.Member;
                    field.SetValue(self, value);
                    return value;
                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)node.Member;
                    property.SetValue(self, value, ArrayUtils.EmptyObjects);
                    return value;
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }

            throw new InvalidOperationException();
        }


        private static EvaluationAddress EvaluateAddress(CodeContext context, Expression node) {
            switch (node.NodeType) {
                case AstNodeType.BoundExpression:
                    return EvaluateAddress((BoundExpression)node);
                case AstNodeType.Block:
                    return EvaluateAddress(context, (Block)node);
                case AstNodeType.Conditional:
                    return EvaluateAddress(context, (ConditionalExpression)node);
                default:
                    return new EvaluationAddress(node);
            }
        }

        private static EvaluationAddress EvaluateAddress(BoundExpression node) {
            return new VariableAddress(node);
        }

        private static EvaluationAddress EvaluateAddress(CodeContext context, Block node) {
            if (node.Type == typeof(void)) {
                throw new NotSupportedException("Address of block without value");
            }

            List<EvaluationAddress> addresses = new List<EvaluationAddress>();
            foreach(Expression current in node.Expressions) {
                addresses.Add(EvaluateAddress(context, current));
            }
            return new CommaAddress(node, addresses);
        }

        private static EvaluationAddress EvaluateAddress(CodeContext context, ConditionalExpression node) {
            object test = (bool)Interpreter.EvaluateExpression(context, node.Test);

            if ((bool) test) {
                return EvaluateAddress(context, node.IfTrue);
            } else {
                return EvaluateAddress(context, node.IfFalse);
            }
        }

        private static object NotImplemented() {
            throw new NotImplementedException();
        }
    }
}
