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
using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Scripting.Interpretation")]

namespace Microsoft.Scripting.Interpretation {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class Interpreter {
        #region Entry points

        public static object TopLevelExecute(InterpretedScriptCode scriptCode, params object[] args) {
            ContractUtils.RequiresNotNull(scriptCode, "scriptCode");
            InterpreterState state = InterpreterState.CreateForTopLambda(scriptCode, scriptCode.Code, args);
            return DoExecute(state, scriptCode.Code);
        }

        internal static object Evaluate(InterpreterState state, Expression expression) {
            object result = Interpret(state, expression);

            if (result is ControlFlow) {
                throw new InvalidOperationException("Invalid expression");
            }

            return result;
        }

        internal static object Execute(InterpreterState state, Expression expression) {
            ControlFlow result = Interpret(state, expression) as ControlFlow;

            if (result != null && result.Kind == ControlFlowKind.Return) {
                return result.Value;
            }

            return null;
        }

        internal static object ExecuteGenerator(InterpreterState state, Expression expression) {
            return Interpret(state, expression);
        }

        #endregion

        /// <summary>
        /// Evaluates expression and checks it for ControlFlow. If it is control flow, returns true,
        /// otherwise returns false.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="node"></param>
        /// <param name="result">Result of the evaluation</param>
        /// <returns>true if control flow, false if not</returns>
        private static bool InterpretAndCheckFlow(InterpreterState state, Expression node, out object result) {
            result = Interpret(state, node);

            return result != ControlFlow.NextForYield && result is ControlFlow;
        }

        /// <summary>
        /// Evaluates an expression and checks to see if the ControlFlow is NextForYield.  If it is then we are currently
        /// searching for the next yield and we need to execute any additional nodes in a larger compound node.
        /// </summary>
        private static bool InterpretAndCheckYield(InterpreterState state, Expression target, out object res) {
            res = Interpret(state, target);
            if (res != ControlFlow.NextForYield) {
                return true;
            }
            return false;
        }

        // Individual expressions and statements

        private static object InterpretConstantExpression(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            ConstantExpression node = (ConstantExpression)expr;
            return node.Value;
        }

        private static object InterpretConditionalExpression(InterpreterState state, Expression expr) {
            ConditionalExpression node = (ConditionalExpression)expr;
            SetSourceLocation(state, node);
            object test;

            if (InterpretAndCheckFlow(state, node.Test, out test)) {
                return test;
            }

            if (test == ControlFlow.NextForYield || (bool)test) {
                if (InterpretAndCheckYield(state, node.IfTrue, out test)) {
                    return test;
                }
            }

            return Interpret(state, node.IfFalse);
        }

        private static bool IsInputParameter(ParameterInfo pi) {
            return !pi.IsOut || (pi.Attributes & ParameterAttributes.In) != 0;
        }

        private static object InvokeMethod(MethodInfo method, object instance, params object[] parameters) {
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

        private static object InterpretInvocationExpression(InterpreterState state, Expression expr) {
            InvocationExpression node = (InvocationExpression)expr;

            // TODO: this should have the same semantics of the compiler
            // in particular, it doesn't handle the case where the left hand
            // side returns a lambda that we need to interpret
            return InterpretMethodCallExpression(state, Expression.Call(node.Expression, node.Expression.Type.GetMethod("Invoke"), ArrayUtils.ToArray(node.Arguments)));
        }

        private static object InterpretIndexExpression(InterpreterState state, Expression expr) {
            var node = (IndexExpression)expr;

            if (node.Indexer != null) {
                return InterpretMethodCallExpression(
                    state,
                    Expression.Call(node.Object, node.Indexer.GetGetMethod(true), node.Arguments)
                );
            }

            if (node.Arguments.Count != 1) {
                var get = node.Object.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
                return InterpretMethodCallExpression(
                    state,
                    Expression.Call(node.Object, get, node.Arguments)
                );
            }

            object array, index;

            if (InterpretAndCheckFlow(state, node.Object, out array)) {
                return array;
            }
            if (InterpretAndCheckFlow(state, node.Arguments[0], out index)) {
                return index;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            return ((Array)array).GetValue((int)index);
        }

        private static object InterpretMethodCallExpression(InterpreterState state, Expression expr) {
            MethodCallExpression node = (MethodCallExpression)expr;
            SetSourceLocation(state, node);

            object instance = null;
            // Evaluate the instance first (if the method is non-static)
            if (!node.Method.IsStatic) {
                if (InterpretAndCheckFlow(state, node.Object, out instance)) {
                    return instance;
                }
            }

            var parameterInfos = node.Method.GetParameters();

            object[] parameters;
            if (!state.TryGetStackState(expr, out parameters)) {
                parameters = new object[parameterInfos.Length];
            }

            Debug.Assert(parameters.Length == parameterInfos.Length);

            int lastByRefParamIndex = -1;
            var paramAddrs = new EvaluationAddress[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++) {
                ParameterInfo info = parameterInfos[i];

                if (info.ParameterType.IsByRef) {
                    lastByRefParamIndex = i;
                    paramAddrs[i] = EvaluateAddress(state, node.Arguments[i]);

                    object value = paramAddrs[i].GetValue(state, !IsInputParameter(info));
                    if (IsInputParameter(info)) {
                        if (value != ControlFlow.NextForYield) {
                            // implict cast?
                            parameters[i] = Cast.Explicit(value, info.ParameterType.GetElementType());
                        }
                    }
                } else if (IsInputParameter(info)) {
                    Expression arg = node.Arguments[i];
                    object argValue = null;
                    if (arg != null) {
                        if (InterpretAndCheckFlow(state, arg, out argValue)) {
                            if (state.CurrentYield != null) {
                                state.SaveStackState(node, parameters);
                            }

                            return argValue;
                        }
                    }

                    if (argValue != ControlFlow.NextForYield) {
                        parameters[i] = argValue;
                    }
                }
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            try {
                object res;
                try {
                    // Call the method                    
                    res = InvokeMethod(node.Method, instance, parameters);                   
                } finally {
                    // expose by-ref args
                    for (int i = 0; i <= lastByRefParamIndex; i++) {
                        if (parameterInfos[i].ParameterType.IsByRef) {
                            paramAddrs[i].AssignValue(state, parameters[i]);
                        }
                    }
                }

                // back propagate instance on value types if the instance supports it.
                if (node.Method.DeclaringType != null && node.Method.DeclaringType.IsValueType && !node.Method.IsStatic) {
                    EvaluateAssign(state, node.Object, instance);
                }

                return res;
            } catch (TargetInvocationException e) {
                // Unwrap the real (inner) exception and raise it
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        private static object InterpretAndAlsoBinaryExpression(InterpreterState state, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;
            object ret;
            if (InterpretAndCheckFlow(state, node.Left, out ret)) {
                return ret;
            }

            if (ret == ControlFlow.NextForYield || (bool)ret) {
                return Interpret(state, node.Right);
            }

            return ret;
        }

        private static object InterpretOrElseBinaryExpression(InterpreterState state, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;
            object ret;
            if (InterpretAndCheckFlow(state, node.Left, out ret)) {
                return ret;
            }

            if (ret == ControlFlow.NextForYield || !(bool)ret) {
                return Interpret(state, node.Right);
            }

            return ret;
        }

        // TODO: support conversion lambda
        private static object InterpretCoalesceBinaryExpression(InterpreterState state, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;

            object ret;
            if (InterpretAndCheckFlow(state, node.Left, out ret)) {
                return ret;
            }

            if (ret == ControlFlow.NextForYield || ret == null) {
                return Interpret(state, node.Right);
            }

            return ret;
        }

        private static object InterpretBinaryExpression(InterpreterState state, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;

            object left, right;

            if (InterpretAndCheckFlow(state, node.Left, out left)) {
                return left;
            }
            if (InterpretAndCheckFlow(state, node.Right, out right)) {
                return right;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            if (node.Method != null) {
                return node.Method.Invoke(null, new object[] { left, right });
            } else {
                return EvaluateBinaryOperator(node.NodeType, left, right);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static object EvaluateBinaryOperator(ExpressionType nodeType, object l, object r) {
            switch (nodeType) {
                case ExpressionType.ArrayIndex:
                    Array array = (Array)l;
                    int index = (int)r;
                    return array.GetValue(index);

                case ExpressionType.GreaterThan:
                    return RuntimeHelpers.BooleanToObject(((IComparable)l).CompareTo(r) > 0);
                case ExpressionType.LessThan:
                    return RuntimeHelpers.BooleanToObject(((IComparable)l).CompareTo(r) < 0);
                case ExpressionType.GreaterThanOrEqual:
                    return RuntimeHelpers.BooleanToObject(((IComparable)l).CompareTo(r) >= 0);
                case ExpressionType.LessThanOrEqual:
                    return RuntimeHelpers.BooleanToObject(((IComparable)l).CompareTo(r) <= 0);
                case ExpressionType.Equal:
                    return RuntimeHelpers.BooleanToObject(TestEquals(l, r));

                case ExpressionType.NotEqual:
                    return RuntimeHelpers.BooleanToObject(!TestEquals(l, r));

                case ExpressionType.Multiply:
                    return EvalMultiply(l, r);
                case ExpressionType.Add:
                    return EvalAdd(l, r);
                case ExpressionType.Subtract:
                    return EvalSub(l, r);
                case ExpressionType.Divide:
                    return EvalDiv(l, r);
                case ExpressionType.Modulo:
                    return EvalMod(l, r);
                case ExpressionType.And:
                    return EvalAnd(l, r);
                case ExpressionType.Or:
                    return EvalOr(l, r);
                case ExpressionType.ExclusiveOr:
                    return EvalXor(l, r);
                case ExpressionType.AddChecked:
                    return EvalAddChecked(l, r);
                case ExpressionType.MultiplyChecked:
                    return EvalMultiplyChecked(l, r);
                case ExpressionType.SubtractChecked:
                    return EvalSubChecked(l, r);
                case ExpressionType.Power:
                    return EvalPower(l, r);

                default:
                    throw new NotImplementedException(nodeType.ToString());
            }
        }

        private static object EvalMultiply(object l, object r) {
            if (l is int) return RuntimeHelpers.Int32ToObject((int)l * (int)r);
            if (l is uint) return (uint)l * (uint)r;
            if (l is short) return (short)((short)l * (short)r);
            if (l is ushort) return (ushort)((ushort)l * (ushort)r);
            if (l is long) return (long)l * (long)r;
            if (l is ulong) return (ulong)l * (ulong)r;
            if (l is float) return (float)l * (float)r;
            if (l is double) return (double)l * (double)r;
            throw new InvalidOperationException("multiply: {0} " + CompilerHelpers.GetType(l).Name);
        }
        private static object EvalMultiplyChecked(object l, object r) {
            if (l is int) return RuntimeHelpers.Int32ToObject(checked((int)l * (int)r));
            if (l is uint) return checked((uint)l * (uint)r);
            if (l is short) return checked((short)((short)l * (short)r));
            if (l is ushort) return checked((ushort)((ushort)l * (ushort)r));
            if (l is long) return checked((long)l * (long)r);
            if (l is ulong) return checked((ulong)l * (ulong)r);
            if (l is float) return checked((float)l * (float)r);
            if (l is double) return checked((double)l * (double)r);
            throw new InvalidOperationException("multiply: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalAdd(object l, object r) {
            if (l is int) return RuntimeHelpers.Int32ToObject((int)l + (int)r);
            if (l is uint) return (uint)l + (uint)r;
            if (l is short) return (short)((short)l + (short)r);
            if (l is ushort) return (ushort)((ushort)l + (ushort)r);
            if (l is long) return (long)l + (long)r;
            if (l is ulong) return (ulong)l + (ulong)r;
            if (l is float) return (float)l + (float)r;
            if (l is double) return (double)l + (double)r;
            throw new InvalidOperationException("add: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalAddChecked(object l, object r) {
            if (l is int) return RuntimeHelpers.Int32ToObject(checked((int)l + (int)r));
            if (l is uint) return checked((uint)l + (uint)r);
            if (l is short) return checked((short)((short)l + (short)r));
            if (l is ushort) return checked((ushort)((ushort)l + (ushort)r));
            if (l is long) return checked((long)l + (long)r);
            if (l is ulong) return checked((ulong)l + (ulong)r);
            if (l is float) return checked((float)l + (float)r);
            if (l is double) return checked((double)l + (double)r);
            throw new InvalidOperationException("add: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalSub(object l, object r) {
            if (l is int) return RuntimeHelpers.Int32ToObject((int)l - (int)r);
            if (l is uint) return (uint)l - (uint)r;
            if (l is short) return (short)((short)l - (short)r);
            if (l is ushort) return (ushort)((ushort)l - (ushort)r);
            if (l is long) return (long)l - (long)r;
            if (l is ulong) return (ulong)l - (ulong)r;
            if (l is float) return (float)l - (float)r;
            if (l is double) return (double)l - (double)r;
            throw new InvalidOperationException("sub: {0} " + CompilerHelpers.GetType(l).Name);
        }
        private static object EvalSubChecked(object l, object r) {
            if (l is int) return RuntimeHelpers.Int32ToObject(checked((int)l - (int)r));
            if (l is uint) return checked((uint)l - (uint)r);
            if (l is short) return checked((short)((short)l - (short)r));
            if (l is ushort) return checked((ushort)((ushort)l - (ushort)r));
            if (l is long) return checked((long)l - (long)r);
            if (l is ulong) return checked((ulong)l - (ulong)r);
            if (l is float) return checked((float)l - (float)r);
            if (l is double) return checked((double)l - (double)r);
            throw new InvalidOperationException("sub: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalMod(object l, object r) {
            if (l is int) return RuntimeHelpers.Int32ToObject((int)l % (int)r);
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
            if (l is int) return RuntimeHelpers.Int32ToObject((int)l / (int)r);
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
            if (l is int) return RuntimeHelpers.Int32ToObject((int)l & (int)r);
            if (l is uint) return (uint)l & (uint)r;
            if (l is short) return (short)((short)l & (short)r);
            if (l is ushort) return (ushort)((ushort)l & (ushort)r);
            if (l is long) return (long)l & (long)r;
            if (l is ulong) return (ulong)l & (ulong)r;
            throw new InvalidOperationException("and: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalOr(object l, object r) {
            if (l is int) return RuntimeHelpers.Int32ToObject((int)l | (int)r);
            if (l is uint) return (uint)l | (uint)r;
            if (l is short) return (short)((short)l | (short)r);
            if (l is ushort) return (ushort)((ushort)l | (ushort)r);
            if (l is long) return (long)l | (long)r;
            if (l is ulong) return (ulong)l | (ulong)r;
            throw new InvalidOperationException("or: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalXor(object l, object r) {
            if (l is int) return RuntimeHelpers.Int32ToObject((int)l ^ (int)r);
            if (l is uint) return (uint)l ^ (uint)r;
            if (l is short) return (short)((short)l ^ (short)r);
            if (l is ushort) return (ushort)((ushort)l ^ (ushort)r);
            if (l is long) return (long)l ^ (long)r;
            if (l is ulong) return (ulong)l ^ (ulong)r;
            throw new InvalidOperationException("xor: {0} " + CompilerHelpers.GetType(l).Name);
        }

        private static object EvalPower(object l, object r) {
            return System.Math.Pow((double)l, (double)r);
        }

        private static object EvalCoalesce(object l, object r) {
            return l ?? r;
        }


        private static bool TestEquals(object l, object r) {
            // We don't need to go through the same type checks as the emit case,
            // since we know we're always dealing with boxed objects.

            return Object.Equals(l, r);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        private static object InterpretQuoteUnaryExpression(InterpreterState state, Expression expr) {
            // TODO: should we do all the fancy tree rewrite stuff here?
            return ((UnaryExpression)expr).Operand;
        }

        private static object InterpretUnboxUnaryExpression(InterpreterState state, Expression expr) {
            UnaryExpression node = (UnaryExpression)expr;

            object value;
            if (InterpretAndCheckFlow(state, node.Operand, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            if (value != null && node.Type == value.GetType()) {
                return value;
            }

            throw new InvalidCastException(string.Format("cannot unbox value to type '{0}'", node.Type));
        }

        private static object InterpretConvertUnaryExpression(InterpreterState state, Expression expr) {
            UnaryExpression node = (UnaryExpression)expr;

            object value;
            if (InterpretAndCheckFlow(state, node.Operand, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            if (node.Type == typeof(void)) {
                return null;
            }

            // TODO: distinguish between Convert and ConvertChecked
            // TODO: semantics should match compiler
            return Cast.Explicit(value, node.Type);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static object InterpretUnaryExpression(InterpreterState state, Expression expr) {
            UnaryExpression node = (UnaryExpression)expr;

            object value;
            if (InterpretAndCheckFlow(state, node.Operand, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            switch (node.NodeType) {
                case ExpressionType.TypeAs:
                    if (value != null && expr.Type.IsAssignableFrom(value.GetType())) {
                        return value;
                    } else {
                        return null;
                    }

                case ExpressionType.Not:
                    if (value is bool) return (bool)value ? RuntimeHelpers.False : RuntimeHelpers.True;
                    if (value is int) return RuntimeHelpers.Int32ToObject((int)~(int)value);
                    if (value is long) return (long)~(long)value;
                    if (value is short) return (short)~(short)value;
                    if (value is uint) return (uint)~(uint)value;
                    if (value is ulong) return (ulong)~(ulong)value;
                    if (value is ushort) return (ushort)~(ushort)value;
                    if (value is byte) return (byte)~(byte)value;
                    if (value is sbyte) return (sbyte)~(sbyte)value;
                    throw new InvalidOperationException("can't perform unary not on type " + CompilerHelpers.GetType(value).Name);

                case ExpressionType.Negate:
                    if (value is int) return RuntimeHelpers.Int32ToObject((int)(-(int)value));
                    if (value is long) return (long)(-(long)value);
                    if (value is short) return (short)(-(short)value);
                    if (value is float) return -(float)value;
                    if (value is double) return -(double)value;
                    throw new InvalidOperationException("can't negate type " + CompilerHelpers.GetType(value).Name);

                case ExpressionType.UnaryPlus:
                    if (value is int) return RuntimeHelpers.Int32ToObject((int)+(int)value);
                    if (value is long) return (long)+(long)value;
                    if (value is short) return (short)+(short)value;
                    if (value is uint) return (uint)+(uint)value;
                    if (value is ulong) return (ulong)+(ulong)value;
                    if (value is ushort) return (ushort)+(ushort)value;
                    if (value is byte) return (byte)+(byte)value;
                    if (value is sbyte) return (sbyte)+(sbyte)value;
                    throw new InvalidOperationException("can't perform unary plus on type " + CompilerHelpers.GetType(value).Name);

                case ExpressionType.NegateChecked:
                    if (value is int) return RuntimeHelpers.Int32ToObject(checked((int)(-(int)value)));
                    if (value is long) return checked((long)(-(long)value));
                    if (value is short) return checked((short)(-(short)value));
                    if (value is float) return checked(-(float)value);
                    if (value is double) return checked(-(double)value);
                    throw new InvalidOperationException("can't negate type " + CompilerHelpers.GetType(value).Name);

                case ExpressionType.ArrayLength:
                    System.Array arr = (System.Array)value;
                    return arr.Length;

                default:
                    throw new NotImplementedException();
            }
        }

        private static object InterpretLocalScopeExpression(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            LocalScopeExpression node = (LocalScopeExpression)expr;
            return new InterpreterVariables(state, node);
        }

        private static object InterpretNewExpression(InterpreterState state, Expression expr) {
            NewExpression node = (NewExpression)expr;

            object[] args = new object[node.Arguments.Count];
            for (int i = 0; i < node.Arguments.Count; i++) {
                object argValue;
                if (InterpretAndCheckFlow(state, node.Arguments[i], out argValue)) {
                    return argValue;
                }
                args[i] = argValue;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            try {
                return node.Constructor.Invoke(args);
            } catch (TargetInvocationException e) {
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        private static object InterpretListInitExpression(InterpreterState state, Expression expr) {
            throw new NotImplementedException("InterpretListInitExpression");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        private static object InterpretMemberInitExpression(InterpreterState state, Expression expr) {
            throw new NotImplementedException("InterpretMemberInitExpression");
        }

        private static object InterpretTypeBinaryExpression(InterpreterState state, Expression expr) {
            TypeBinaryExpression node = (TypeBinaryExpression)expr;
            object value;
            if (InterpretAndCheckFlow(state, node.Expression, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            return RuntimeHelpers.BooleanToObject(
                node.TypeOperand.IsInstanceOfType(value)
            );
        }

        private static object InterpretDynamicExpression(InterpreterState state, Expression expr) {
            DynamicExpression node = (DynamicExpression)expr;
            SetSourceLocation(state, node);
            var arguments = node.Arguments;

            object[] args;
            if (!state.TryGetStackState(node, out args)) {
                args = new object[arguments.Count];
            }

            for (int i = 0, n = arguments.Count; i < n; i++) {
                object argValue;
                if (InterpretAndCheckFlow(state, arguments[i], out argValue)) {
                    if (state.CurrentYield != null) {
                        state.SaveStackState(node, args);
                    }

                    return argValue;
                }
                if (argValue != ControlFlow.NextForYield) {
                    args[i] = argValue;
                }
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            var callSite = GetCallSite(state, node);
            MatchCallerTarget caller = MatchCaller.GetCaller(callSite.GetType().GetGenericArguments()[0]);
            return caller(callSite, args);
        }

        private static CallSite GetCallSite(InterpreterState state, DynamicExpression node) {
            CallSite callSite;
            var callSites = state.LambdaState.ScriptCode.CallSites;

            // TODO: better locking
            lock (callSites) {
                if (!callSites.TryGetValue(node, out callSite)) {
                    ReflectedCaller factory = GetCallSiteFactory(node);
                    callSite = (CallSite)factory.Invoke(node.Binder);
                    callSites.Add(node, callSite);
                }
            }

            return callSite;
        }

        // The ReflectiveCaller cache
        private static readonly Dictionary<ValueArray<Type>, ReflectedCaller> _executeSites = new Dictionary<ValueArray<Type>, ReflectedCaller>();
        
        private static ReflectedCaller GetCallSiteFactory(DynamicExpression node) {
            var arguments = node.Arguments;
            Type[] types = CompilerHelpers.GetSiteTypes(arguments, node.Type);

            // TODO: remove CodeContext special case:
            int i = (arguments.Count > 0 && typeof(CodeContext).IsAssignableFrom(arguments[0].Type)) ? 1 : 0;
            for (; i < arguments.Count; i++) {
                types[i] = typeof(object);
            }

            ReflectedCaller rc;
            lock (_executeSites) {
                ValueArray<Type> array = new ValueArray<Type>(types);
                if (!_executeSites.TryGetValue(array, out rc)) {
                    Type delegateType = DynamicSiteHelpers.MakeCallSiteDelegate(types);
                    MethodInfo target = typeof(InterpreterHelpers).GetMethod("CreateSite").MakeGenericMethod(delegateType);
                    _executeSites[array] = rc = ReflectedCaller.Create(target);
                }
            }
            return rc;
        }

        private static object InterpretIndexAssignment(InterpreterState state, AssignmentExpression node) {
            var index = (IndexExpression)node.Expression;

            object instance, value;
            var args = new object[index.Arguments.Count];

            if (InterpretAndCheckFlow(state, index.Object, out instance)) {
                return instance;
            }

            for (int i = 0; i < index.Arguments.Count; i++) {
                object arg;
                if (InterpretAndCheckFlow(state, index.Arguments[i], out arg)) {
                    return arg;
                }
                args[i] = arg;
            }

            if (InterpretAndCheckFlow(state, node.Value, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            if (index.Indexer != null) {
                // For indexed properties, just call the setter
                InvokeMethod(index.Indexer.GetSetMethod(true), instance, args);
            } else if (index.Arguments.Count != 1) {
                // Multidimensional arrays, call set
                var set = index.Object.Type.GetMethod("Set", BindingFlags.Public | BindingFlags.Instance);
                InvokeMethod(set, instance, args);
            } else {
                ((Array)instance).SetValue(value, (int)args[0]);
            }

            return value;
        }

        private static object InterpretVariableAssignment(InterpreterState state, Expression expr) {
            AssignmentExpression node = (AssignmentExpression)expr;
            SetSourceLocation(state, expr);
            object value;
            if (InterpretAndCheckFlow(state, node.Value, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            EvaluateAssignVariable(state, node.Expression, value);
            return value;
        }

        private static object InterpretAssignmentExpression(InterpreterState state, Expression expr) {
            AssignmentExpression node = (AssignmentExpression)expr;
            switch (node.Expression.NodeType) {
                case ExpressionType.Index:
                    return InterpretIndexAssignment(state, node);
                case ExpressionType.MemberAccess:
                    return InterpretMemberAssignment(state, node);
                case ExpressionType.Parameter:
                case ExpressionType.Variable:
                case ExpressionType.Extension:
                    return InterpretVariableAssignment(state, node);
                default:
                    throw new InvalidOperationException("Invalid lvalue for assignment: " + node.Expression.NodeType);
            }
        }

        private static object InterpretVariableExpression(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            return state.GetValue(expr);
        }

        private static object InterpretParameterExpression(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            return state.GetValue(expr);
        }

        private static object InterpretLambdaExpression(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            LambdaExpression node = (LambdaExpression)expr;
            return GetDelegateForInterpreter(state, node);
        }

        private static object InterpretMemberAssignment(InterpreterState state, AssignmentExpression node) {
            MemberExpression lvalue = (MemberExpression)node.Expression;

            object target = null, value;
            if (lvalue.Expression != null) {
                if (InterpretAndCheckFlow(state, lvalue.Expression, out target)) {
                    return target;
                }
            }
            if (InterpretAndCheckFlow(state, node.Value, out value)) {
                return value;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            switch (lvalue.Member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = (FieldInfo)lvalue.Member;
                    field.SetValue(target, value);
                    break;
                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)lvalue.Member;
                    property.SetValue(target, value, null);
                    break;
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }
            return value;
        }

        private static object InterpretMemberExpression(InterpreterState state, Expression expr) {
            MemberExpression node = (MemberExpression)expr;

            object self = null;
            if (node.Expression != null) {
                if (InterpretAndCheckFlow(state, node.Expression, out self)) {
                    return self;
                }
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            switch (node.Member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = (FieldInfo)node.Member;
                    return field.GetValue(self);
                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)node.Member;
                    return property.GetValue(self, ArrayUtils.EmptyObjects);
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }

            throw new InvalidOperationException();
        }

        private static object InterpretNewArrayExpression(InterpreterState state, Expression expr) {
            NewArrayExpression node = (NewArrayExpression)expr;
            ConstructorInfo constructor;

            if (node.NodeType == ExpressionType.NewArrayBounds) {
                int rank = node.Type.GetArrayRank();
                Type[] types = new Type[rank];
                object[] bounds = new object[rank];
                for (int i = 0; i < rank; i++) {
                    types[i] = typeof(int);
                    object value;
                    if (InterpretAndCheckFlow(state, node.Expressions[i], out value)) {
                        return value;
                    }
                    bounds[i] = value;
                }

                if (state.CurrentYield != null) {
                    return ControlFlow.NextForYield;
                }

                constructor = expr.Type.GetConstructor(types);
                return constructor.Invoke(bounds);
            } else {
                // this must be ExpressionType.NewArrayInit
                object[] values;
                if (!state.TryGetStackState(node, out values)) {
                    values = new object[node.Expressions.Count];
                }

                for (int i = 0; i < node.Expressions.Count; i++) {
                    object value;
                    if (InterpretAndCheckFlow(state, node.Expressions[i], out value)) {
                        if (state.CurrentYield != null) {
                            // yield w/ expressions on the stack, we need to save the currently 
                            // evaluated nodes for when we come back.
                            state.SaveStackState(node, values);
                        }

                        return value;
                    }

                    if (value != ControlFlow.NextForYield) {
                        values[i] = value;
                    }
                }

                if (state.CurrentYield != null) {
                    // we were just walking looking for yields, this has no result.
                    return ControlFlow.NextForYield;
                }

                if (node.Type != typeof(object[])) {
                    constructor = expr.Type.GetConstructor(new Type[] { typeof(int) });
                    Array contents = (Array)constructor.Invoke(new object[] { node.Expressions.Count });
                    // value arrays cannot be cast to object arrays

                    for (int i = 0; i < node.Expressions.Count; i++) {
                        contents.SetValue(values[i], i);
                    }
                    return contents;
                }

                return values;
            }
        }

        private static object InterpretBlock(InterpreterState state, Expression expr) {
            Block node = (Block)expr;
            SetSourceLocation(state, node);

            object result = ControlFlow.NextStatement;
            for (int index = 0; index < node.Expressions.Count; index++) {
                Expression current = node.Expressions[index];

                object val;
                if (InterpretAndCheckFlow(state, current, out val)) {
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

        private static object InterpretBreakStatement(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            BreakStatement node = (BreakStatement)expr;
            SetSourceLocation(state, node);
            return ControlFlow.Break;
        }

        private static object InterpretContinueStatement(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            ContinueStatement node = (ContinueStatement)expr;
            SetSourceLocation(state, node);
            return ControlFlow.Continue;
        }

        private static object InterpretDoStatement(InterpreterState state, Expression expr) {
            DoStatement node = (DoStatement)expr;
            SetSourceLocation(state, node);

            for (; ; ) {
                ControlFlow cf;

                object ret = Interpret(state, node.Body);

                if ((cf = ret as ControlFlow) != null) {
                    if (cf == ControlFlow.Break) {
                        break;
                    } else if (cf.Kind == ControlFlowKind.Return) {
                        return ret;
                    }
                }

                ret = Interpret(state, node.Test);
                if ((cf = ret as ControlFlow) != null) {
                    if (cf == ControlFlow.Break) {
                        break;
                    } else if (cf.Kind == ControlFlowKind.Return) {
                        return ret;
                    }
                }

                // Check the condition
                if (ret != ControlFlow.NextForYield && !(bool)ret) {
                    break;
                }
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            return ControlFlow.NextStatement;
        }

        private static object InterpretEmptyStatement(InterpreterState state, Expression expr) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            EmptyStatement node = (EmptyStatement)expr;
            SetSourceLocation(state, node);
            return ControlFlow.NextStatement;
        }

        /// <summary>
        /// Labeled statement makes break/continue go to the end of the contained expression.
        /// </summary>
        private static object InterpretLabeledStatement(InterpreterState state, Expression expr) {
            LabeledStatement node = (LabeledStatement)expr;
            SetSourceLocation(state, node);

            object res = Interpret(state, expr);
            if (res == ControlFlow.Break || res == ControlFlow.Continue) {
                return ControlFlow.NextStatement;
            }

            return res;
        }

        private static object InterpretLoopStatement(InterpreterState state, Expression expr) {
            LoopStatement node = (LoopStatement)expr;
            SetSourceLocation(state, node);

            for (; ; ) {
                ControlFlow cf;

                if (node.Test != null) {
                    object test = Interpret(state, node.Test);
                    if ((cf = test as ControlFlow) != null) {
                        if (cf == ControlFlow.Break) {
                            // Break out of the loop and execute next statement outside
                            return ControlFlow.NextStatement;
                        } else if (cf.Kind == ControlFlowKind.Return) {
                            return test;
                        }
                    }

                    // Test is false, break the loop
                    if (test != ControlFlow.NextForYield && !(bool)test) {
                        break;
                    }
                }

                object body = Interpret(state, node.Body);
                if ((cf = body as ControlFlow) != null) {
                    if (cf == ControlFlow.Break) {
                        // Break out of the loop and execute next statement outside
                        return ControlFlow.NextStatement;
                    } else if (cf.Kind == ControlFlowKind.Return) {
                        return body;
                    }
                }

                if (node.Increment != null) {
                    object increment = Interpret(state, node.Increment);
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
                return Interpret(state, node.ElseStatement);
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            return ControlFlow.NextStatement;
        }

        private static object InterpretReturnStatement(InterpreterState state, Expression expr) {
            ReturnStatement node = (ReturnStatement)expr;
            SetSourceLocation(state, node);
            object value = null;
            if (node.Expression != null) {
                value = Interpret(state, node.Expression);
                ControlFlow cf = value as ControlFlow;
                if (cf != null) {
                    // propagate
                    return cf;
                }
            }

            return ControlFlow.Return(value);
        }

        private static object InterpretScopeExpression(InterpreterState state, Expression expr) {
            ScopeExpression node = (ScopeExpression)expr;
            SetSourceLocation(state, node);

            // restore scope if we yielded
            InterpreterState child;
            if (!state.TryGetStackState(node, out child)) {
                // otherwise, create a new nested scope
                child = state.CreateForScope(node);
            }

            object result = Interpret(child, node.Body);

            if (state.CurrentYield != null) {
                // save scope if yielding so we can restore it
                state.SaveStackState(node, child);
            }

            return result;
        }

        private static object InterpretSwitchStatement(InterpreterState state, Expression expr) {
            // TODO: yield aware switch
            SwitchStatement node = (SwitchStatement)expr;
            SetSourceLocation(state, node);

            object testValue;
            if (InterpretAndCheckFlow(state, node.TestValue, out testValue)) {
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
                object result = Interpret(state, sc.Body);

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

        private static object InterpretThrowStatement(InterpreterState state, Expression expr) {
            ThrowStatement node = (ThrowStatement)expr;
            Exception ex;

            if (node.Value == null) {
                ex = LastEvalException;
            } else {
                object exception;
                if (InterpretAndCheckFlow(state, node.Exception, out exception)) {
                    return exception;
                }

                ex = (Exception)exception;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            throw (Exception)ex;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses")]
        private static object InterpretTryStatement(InterpreterState state, Expression expr) {
            // TODO: Yield aware
            TryStatement node = (TryStatement)expr;
            bool rethrow = false, catchFaulted = false;
            Exception savedExc = null;
            object ret = ControlFlow.NextStatement;

            try {
                if (!InterpretAndCheckFlow(state, node.Body, out ret)) {
                    ret = ControlFlow.NextStatement;
                }
            } catch (Exception exc) {
                rethrow = true;
                savedExc = exc;
                if (node.Handlers != null) {
                    PushEvalException(exc);
                    try {
                        ret = ControlFlowKind.NextStatement;
                        foreach (CatchBlock handler in node.Handlers) {
                            if (handler.Test.IsInstanceOfType(exc)) {
                                if (handler.Variable != null) {
                                    EvaluateAssignVariable(state, handler.Variable, exc);
                                }

                                if (handler.Filter != null) {
                                    object filterResult;
                                    if (InterpretAndCheckFlow(state, handler.Filter, out filterResult)) {
                                        ret = filterResult;
                                        break;
                                    } else if (!((bool)filterResult)) {
                                        // handler doesn't apply, check next handler.
                                        continue;
                                    }
                                }

                                rethrow = false;
                                catchFaulted = true;
                                object body;
                                if (InterpretAndCheckFlow(state, handler.Body, out body)) {
                                    ret = body;
                                }
                                catchFaulted = false;
                                break;
                            }
                        }
                    } finally {
                        PopEvalException();
                    }
                }
            } finally {
                if (node.Finally != null || ((rethrow || catchFaulted) && node.Fault != null)) {
                    Expression faultOrFinally = node.Finally ?? node.Fault;

                    object result;
                    if (InterpretAndCheckFlow(state, faultOrFinally, out result) &&
                        result != ControlFlow.NextStatement) {
                        ret = result;
                        rethrow = false;
                    }
                }
                if (rethrow) {
                    throw ExceptionHelpers.UpdateForRethrow(savedExc);
                }
            }

            return ret;
        }

        private static object InterpretYieldStatement(InterpreterState state, Expression node) {
            YieldStatement yield = (YieldStatement)node;

            if (state.CurrentYield == yield) {
                // we've just advanced past the current yield, start executing code again.
                state.CurrentYield = null;
                return ControlFlow.NextStatement;
            }

            object res;
            if (InterpretAndCheckFlow(state, yield.Expression, out res) && res != ControlFlow.NextStatement) {
                // yield contains flow control.
                return res;
            }

            if (state.CurrentYield == null) {
                // we are the yield, we just ran our code, and now we
                // need to return the result.
                state.CurrentYield = yield;

                return ControlFlow.Return(res);
            }

            return ControlFlow.NextForYield;
        }

        private static object InterpretExtensionExpression(InterpreterState state, Expression expr) {
            return Interpret(state, expr.ReduceToKnown());
        }

        #endregion

        internal static object EvaluateAssign(InterpreterState state, Expression node, object value) {
            switch (node.NodeType) {
                case ExpressionType.Variable:
                case ExpressionType.Parameter:
                case ExpressionType.Extension:
                    return EvaluateAssignVariable(state, node, value);
                case ExpressionType.Assign:
                    return EvaluateAssign(state, (AssignmentExpression)node, value);
                case ExpressionType.MemberAccess:
                    return EvaluateAssign(state, (MemberExpression)node, value);
                default:
                    return value;
            }
        }

        private static object EvaluateAssign(InterpreterState state, AssignmentExpression node, object value) {
            return EvaluateAssignVariable(state, node.Expression, value);
        }

        private static object EvaluateAssignVariable(InterpreterState state, Expression var, object value) {
            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
            }

            state.SetValue(var, value);
            return value;
        }

        private static object EvaluateAssign(InterpreterState state, MemberExpression node, object value) {
            object self = null;
            if (InterpretAndCheckFlow(state, node.Expression, out self)) {
                return self;
            }

            if (state.CurrentYield != null) {
                return ControlFlow.NextForYield;
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


        private static EvaluationAddress EvaluateAddress(InterpreterState state, Expression node) {
            switch (node.NodeType) {
                case ExpressionType.Variable:
                case ExpressionType.Parameter:
                    return new VariableAddress(node);
                case ExpressionType.Block:
                    return EvaluateAddress(state, (Block)node);
                case ExpressionType.Conditional:
                    return EvaluateAddress(state, (ConditionalExpression)node);
                default:
                    return new EvaluationAddress(node);
            }
        }


        private static EvaluationAddress EvaluateAddress(InterpreterState state, Block node) {
            if (node.Type == typeof(void)) {
                throw new NotSupportedException("Address of block without value");
            }

            List<EvaluationAddress> addresses = new List<EvaluationAddress>();
            foreach (Expression current in node.Expressions) {
                addresses.Add(EvaluateAddress(state, current));
            }
            return new CommaAddress(node, addresses);
        }

        private static EvaluationAddress EvaluateAddress(InterpreterState state, ConditionalExpression node) {
            object test = (bool)Interpret(state, node.Test);

            if ((bool)test) {
                return EvaluateAddress(state, node.IfTrue);
            } else {
                return EvaluateAddress(state, node.IfFalse);
            }
        }

        private static void SetSourceLocation(InterpreterState state, Expression node) {
            if (state.CurrentYield == null) {
                SourceLocation curLocation = node.Annotations.Get<SourceSpan>().Start;
                if (curLocation != SourceLocation.Invalid && curLocation != SourceLocation.None) {
                    state.CurrentLocation = curLocation;
                }
            }
        }
    }
}
