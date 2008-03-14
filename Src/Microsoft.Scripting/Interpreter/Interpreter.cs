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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal static partial class Interpreter {

        #region Entry points

        internal static object TopLevelExecute(LambdaExpression lambda, CodeContext context) {
            return Execute(context, lambda);
        }

        internal static object Evaluate(CodeContext context, Expression expression) {
            object result = Interpret(context, expression);

            if (result is ControlFlow) {
                throw new InvalidOperationException("Invalid expression");
            }

            return result;
        }

        internal static object Execute(CodeContext context, Expression expression) {
            ControlFlow result = Interpret(context, expression) as ControlFlow;

            if (result != null && result.Kind == ControlFlowKind.Return) {
                return result.Value;
            }

            return null;
        }

        #endregion

        private static object Interpret(CodeContext context, Expression expr) {
            Contract.RequiresNotNull(context, "context");
            Contract.RequiresNotNull(expr, "expr");

            int kind = (int)expr.NodeType;
            Debug.Assert(kind < _Interpreters.Length);

            return _Interpreters[kind](context, expr);
        }


        /// <summary>
        /// Evaluates expression and checks it for ControlFlow. If it is control flow, returns true,
        /// otherwise returns false.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="node"></param>
        /// <param name="result">Result of the evaluation</param>
        /// <returns>true if control flow, false if not</returns>
        private static bool InterpretAndCheckFlow(CodeContext context, Expression node, out object result) {
            result = Interpret(context, node);
            return result is ControlFlow;
        }

        // Individual expressions and statements

        private static object InterpretConstantExpression(CodeContext context, Expression expr) {
            ConstantExpression node = (ConstantExpression)expr;
            CompilerConstant cc = node.Value as CompilerConstant;
            if (cc != null) {
                return cc.Create(); // TODO: Only create once?
            }

            return node.Value;
        }

        private static object InterpretConditionalExpression(CodeContext context, Expression expr) {
            ConditionalExpression node = (ConditionalExpression)expr;
            object test;

            if (InterpretAndCheckFlow(context, node.Test, out test)) {
                return test;
            }

            if ((bool)test) {
                return Interpret(context, node.IfTrue);
            } else {
                return Interpret(context, node.IfFalse);
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

        private static object InterpretMethodCallExpression(CodeContext context, Expression expr) {
            MethodCallExpression node = (MethodCallExpression)expr;
            object instance = null;
            // Evaluate the instance first (if the method is non-static)
            if (!node.Method.IsStatic) {
                if (InterpretAndCheckFlow(context, node.Instance, out instance)) {
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
                            if (InterpretAndCheckFlow(context, arg, out argValue)) {
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
                    } else if (node.Type == typeof(int)) {
                        res = RuntimeHelpers.Int32ToObject((int)res);
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

        private static object InterpretAndAlsoBinaryExpression(CodeContext context, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;
            object ret;
            if (InterpretAndCheckFlow(context, node.Left, out ret)) {
                return ret;
            }
            return ((bool)ret) ? Interpret(context, node.Right) : ret;
        }

        private static object InterpretOrElseBinaryExpression(CodeContext context, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;
            object ret;
            if (InterpretAndCheckFlow(context, node.Left, out ret)) {
                return ret;
            }
            return ((bool)ret) ? ret : Interpret(context, node.Right);
        }

        private static object InterpretBinaryExpression(CodeContext context, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;
            object left, right;

            if (InterpretAndCheckFlow(context, node.Left, out left)) {
                return left;
            }
            if (InterpretAndCheckFlow(context, node.Right, out right)) {
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

        private static bool TestEquals(object l, object r) {
            // We don't need to go through the same type checks as the emit case,
            // since we know we're always dealing with boxed objects.

            return Object.Equals(l, r);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static object InterpretUnaryExpression(CodeContext context, Expression expr) {
            UnaryExpression node = (UnaryExpression)expr;
            object value;
            if (InterpretAndCheckFlow(context, node.Operand, out value)) {
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
                    if (value is int) return RuntimeHelpers.Int32ToObject((int)~(int)value);
                    if (value is long) return (long)~(long)value;
                    if (value is short) return (short)~(short)value;
                    if (value is uint) return (uint)~(uint)value;
                    if (value is ulong) return (ulong)~(ulong)value;
                    if (value is ushort) return (ushort)~(ushort)value;
                    if (value is byte) return (byte)~(byte)value;
                    if (value is sbyte) return (sbyte)~(sbyte)value;
                    throw new InvalidOperationException("can't perform unary not on type " + CompilerHelpers.GetType(value).Name);

                case AstNodeType.Negate:
                    if (value is int) return RuntimeHelpers.Int32ToObject((int)(-(int)value));
                    if (value is long) return (long)(-(long)value);
                    if (value is short) return (short)(-(short)value);
                    if (value is float) return -(float)value;
                    if (value is double) return -(double)value;
                    throw new InvalidOperationException("can't negate type " + CompilerHelpers.GetType(value).Name);

                default:
                    throw new NotImplementedException();
            }
        }

        private static object InterpretIntrinsicExpression(CodeContext context, Expression expr) {
            switch (expr.NodeType) {
                case AstNodeType.CodeContextExpression:
                    return context;
                case AstNodeType.GeneratorIntrinsic:
                case AstNodeType.EnvironmentExpression:
                    throw new NotImplementedException();
                default:
                    throw new InvalidOperationException();
            }
        }

        private static object InterpretNewExpression(CodeContext context, Expression expr) {
            NewExpression node = (NewExpression)expr;
            object[] args = new object[node.Arguments.Count];
            for (int i = 0; i < node.Arguments.Count; i++) {
                object argValue;
                if (InterpretAndCheckFlow(context, node.Arguments[i], out argValue)) {
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

        private static object InterpretTypeBinaryExpression(CodeContext context, Expression expr) {
            TypeBinaryExpression node = (TypeBinaryExpression)expr;
            object value;
            if (InterpretAndCheckFlow(context, node.Expression, out value)) {
                return value;
            }
            return RuntimeHelpers.BooleanToObject(
                node.TypeOperand.IsInstanceOfType(value)
            );
        }

        private static object InterpretActionExpression(CodeContext context, Expression expr) {
            ActionExpression node = (ActionExpression)expr;
            object[] args = new object[node.Arguments.Count];
            for (int i = 0; i < node.Arguments.Count; i++) {
                object argValue;
                if (InterpretAndCheckFlow(context, node.Arguments[i], out argValue)) {
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

        private static object InterpretArrayIndexAssignment(CodeContext context, Expression expr) {
            ArrayIndexAssignment node = (ArrayIndexAssignment)expr;
            object value, array, index;

            // evaluate the value first
            if (InterpretAndCheckFlow(context, node.Value, out value)) {
                return value;
            }
            if (InterpretAndCheckFlow(context, node.Array, out array)) {
                return array;
            }
            if (InterpretAndCheckFlow(context, node.Index, out index)) {
                return index;
            }
            ((Array)array).SetValue(value, (int)index);
            return value;
        }

        private static object InterpretBoundAssignment(CodeContext context, Expression expr) {
            BoundAssignment node = (BoundAssignment)expr;
            object value;
            if (InterpretAndCheckFlow(context, node.Value, out value)) {
                return value;
            }
            EvaluateAssignVariable(context, node.Variable, value);
            return value;
        }

        private static object InterpretBoundExpression(CodeContext context, Expression expr) {
            BoundExpression node = (BoundExpression)expr;
            object ret;
            switch (node.Variable.Kind) {
                case VariableKind.Temporary:
                    if (!context.Scope.TemporaryStorage.TryGetValue(node.Variable, out ret)) {
                        throw context.LanguageContext.MissingName(node.Variable.Name);
                    } else {
                        return ret;
                    }
                case VariableKind.Parameter:
                    // This is sort of ugly: parameter variables can be stored either as locals or as temporaries (in case of $argn).
                    if (!context.Scope.TemporaryStorage.TryGetValue(node.Variable, out ret) || ret == Uninitialized.Instance) {
                        return RuntimeHelpers.LookupName(context, node.Variable.Name);
                    } else {
                        return ret;
                    }
                case VariableKind.Global:
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

        private static object InterpretCodeBlockExpression(CodeContext context, Expression expr) {
            CodeBlockExpression node = (CodeBlockExpression)expr;
            return GetDelegateForInterpreter(context, node);
        }

        private static object EvaluateCodeContext(CodeContext context) {
            return context;
        }

        private static object InterpretDeleteUnboundExpression(CodeContext context, Expression expr) {
            DeleteUnboundExpression node = (DeleteUnboundExpression)expr;
            return RuntimeHelpers.RemoveName(context, node.Name);
        }

        private static object InterpretMemberAssignment(CodeContext context, Expression expr) {
            MemberAssignment node = (MemberAssignment)expr;
            object target = null, value;
            if (node.Expression != null) {
                if (InterpretAndCheckFlow(context, node.Expression, out target)) {
                    return target;
                }
            }
            if (InterpretAndCheckFlow(context, node.Value, out value)) {
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

        private static object InterpretMemberExpression(CodeContext context, Expression expr) {
            MemberExpression node = (MemberExpression)expr;
            object self = null;
            if (node.Expression != null) {
                if (InterpretAndCheckFlow(context, node.Expression, out self)) {
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

        private static object InterpretNewArrayExpression(CodeContext context, Expression expr) {
            NewArrayExpression node = (NewArrayExpression)expr;
            ConstructorInfo constructor;

            if (node.NodeType == AstNodeType.NewArrayBounds) {
                Debug.Assert(false, "debug me");
                int rank = node.Type.GetArrayRank();
                Type[] types = new Type[rank];
                object[] bounds = new object[rank];
                for (int i = 0; i < rank; i++) {
                    types[i] = typeof(int);
                    object value;
                    if (InterpretAndCheckFlow(context, node.Expressions[i], out value)) {
                        return value;
                    }
                    bounds[i] = value;
                }
                constructor = expr.Type.GetConstructor(types);
                return constructor.Invoke(bounds);
            } else {
                // this must be AstNodeType.NewArrayExpression
                constructor = expr.Type.GetConstructor(new Type[] { typeof(int) });

                if (node.Type.GetElementType().IsValueType) {
                    // value arrays cannot be cast to object arrays
                    object contents = (object)constructor.Invoke(new object[] { node.Expressions.Count });
                    MethodInfo setter = node.Type.GetMethod("Set");
                    for (int i = 0; i < node.Expressions.Count; i++) {
                        object value;
                        if (InterpretAndCheckFlow(context, node.Expressions[i], out value)) {
                            return value;
                        }
                        setter.Invoke(contents, new object[] { i, value });
                    }
                    return contents;
                } else {
                    object[] contents = (object[])constructor.Invoke(new object[] { node.Expressions.Count });
                    for (int i = 0; i < node.Expressions.Count; i++) {
                        object value;
                        if (InterpretAndCheckFlow(context, node.Expressions[i], out value)) {
                            return value;
                        }
                        contents[i] = value;
                    }
                    return contents;
                }
            }
        }

        private static object InterpretUnboundAssignment(CodeContext context, Expression expr) {
            UnboundAssignment node = (UnboundAssignment)expr;
            object value;
            if (InterpretAndCheckFlow(context, node.Value, out value)) {
                return value;
            }
            RuntimeHelpers.SetName(context, node.Name, value);
            return value;
        }

        private static object InterpretUnboundExpression(CodeContext context, Expression expr) {
            UnboundExpression node = (UnboundExpression)expr;
            return RuntimeHelpers.LookupName(context, node.Name);
        }

        private static object InterpretBlock(CodeContext context, Expression expr) {
            Block node = (Block)expr;
            context.Scope.SourceLocation = node.Start;

            object result = ControlFlow.NextStatement;
            for (int index = 0; index < node.Expressions.Count; index++) {
                Expression current = node.Expressions[index];

                object val;
                if (InterpretAndCheckFlow(context, current, out val)) {
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

        private static object InterpretBreakStatement(CodeContext context, Expression expr) {
            BreakStatement node = (BreakStatement)expr;
            context.Scope.SourceLocation = node.Start;
            return ControlFlow.Break;
        }

        private static object InterpretContinueStatement(CodeContext context, Expression expr) {
            ContinueStatement node = (ContinueStatement)expr;
            context.Scope.SourceLocation = node.Start;
            return ControlFlow.Continue;
        }

        // TODO: Should not be name-based.
        private static object InterpretDeleteStatement(CodeContext context, Expression expr) {
            DeleteStatement node = (DeleteStatement)expr;
            context.Scope.SourceLocation = node.Start;
            switch (node.Variable.Kind) {
                case VariableKind.Temporary:
                    context.Scope.TemporaryStorage.Remove(node.Variable);
                    break;
                case VariableKind.Global:
                    RuntimeHelpers.RemoveGlobalName(context, node.Variable.Name);
                    break;
                default:
                    RuntimeHelpers.RemoveName(context, node.Variable.Name);
                    break;
            }

            return ControlFlow.NextStatement;
        }

        private static object InterpretDoStatement(CodeContext context, Expression expr) {
            DoStatement node = (DoStatement)expr;
            context.Scope.SourceLocation = node.Start;

            for (; ; ) {
                ControlFlow cf;

                object ret = Interpret(context, node.Body);

                if ((cf = ret as ControlFlow) != null) {
                    if (cf == ControlFlow.Break) {
                        break;
                    } else if (cf.Kind == ControlFlowKind.Return) {
                        return ret;
                    }
                }

                ret = Interpret(context, node.Test);
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

        private static object InterpretEmptyStatement(CodeContext context, Expression expr) {
            EmptyStatement node = (EmptyStatement)expr;
            context.Scope.SourceLocation = node.Start;
            return ControlFlow.NextStatement;
        }

        private static object InterpretExpressionStatement(CodeContext context, Expression expr) {
            ExpressionStatement node = (ExpressionStatement)expr;
            context.Scope.SourceLocation = node.Start;
            object value;
            if (InterpretAndCheckFlow(context, node.Expression, out value)) {
                return value;
            }
            return ControlFlow.NextStatement;
        }

        private static object InterpretLabeledStatement(CodeContext context, Expression expr) {
            LabeledStatement node = (LabeledStatement)expr;
            context.Scope.SourceLocation = node.Start;
            throw new NotImplementedException();
        }

        private static object InterpretLoopStatement(CodeContext context, Expression expr) {
            LoopStatement node = (LoopStatement)expr;
            context.Scope.SourceLocation = node.Start;

            for (; ; ) {
                ControlFlow cf;

                if (node.Test != null) {
                    object test = Interpret(context, node.Test);
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

                object body = Interpret(context, node.Body);
                if ((cf = body as ControlFlow) != null) {
                    if (cf == ControlFlow.Break) {
                        // Break out of the loop and execute next statement outside
                        return ControlFlow.NextStatement;
                    } else if (cf.Kind == ControlFlowKind.Return) {
                        return body;
                    }
                }

                if (node.Increment != null) {
                    object increment = Interpret(context, node.Increment);
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
                return Interpret(context, node.ElseStatement);
            }

            return ControlFlow.NextStatement;
        }

        private static object InterpretReturnStatement(CodeContext context, Expression expr) {
            ReturnStatement node = (ReturnStatement)expr;
            context.Scope.SourceLocation = node.Start;
            object value = null;
            if (node.Expression != null) {
                value = Interpret(context, node.Expression);
                ControlFlow cf = value as ControlFlow;
                if (cf != null) {
                    // propagate
                    return cf;
                }
            }

            return ControlFlow.Return(value);
        }

        private static object InterpretScopeStatement(CodeContext context, Expression expr) {
            ScopeStatement node = (ScopeStatement)expr;
            context.Scope.SourceLocation = node.Start;
            CodeContext scopeContext;
            ControlFlow cf;

            // TODO: should work with LocalScope
            if (node.Scope != null) {
                object scope = Interpret(context, node.Scope);
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
            if (InterpretAndCheckFlow(scopeContext, node.Body, out body)) {
                return body;
            }

            // Eat the value and return "next"
            return ControlFlow.NextStatement;
        }

        private static object InterpretSwitchStatement(CodeContext context, Expression expr) {
            SwitchStatement node = (SwitchStatement)expr;
            context.Scope.SourceLocation = node.Start;

            object testValue;
            if (InterpretAndCheckFlow(context, node.TestValue, out testValue)) {
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
                object result = Interpret(context, sc.Body);

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

        private static object InterpretThrowStatement(CodeContext context, Expression expr) {
            ThrowStatement node = (ThrowStatement)expr;
            if (node.Value == null) {
                throw LastEvalException;
            } else {
                object exception;
                if (InterpretAndCheckFlow(context, node.Exception, out exception)) {
                    return exception;
                }

                throw (Exception)exception;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses")]
        private static object InterpretTryStatement(CodeContext context, Expression expr) {
            TryStatement node = (TryStatement)expr;
            bool rethrow = false;
            Exception savedExc = null;
            object ret = ControlFlow.NextStatement;

            try {
                if (InterpretAndCheckFlow(context, node.Body, out ret)) {
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
                                if (InterpretAndCheckFlow(context, handler.Body, out body)) {
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
                    object result;
                    if (InterpretAndCheckFlow(context, node.FinallyStatement, out result) &&
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

        private static object InterpretYieldStatement(CodeContext context, Expression node) {
            throw new NotImplementedException();
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
                case VariableKind.Temporary:
                    context.Scope.TemporaryStorage[var] = value;
                    break;
                case VariableKind.Global:
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
            if (InterpretAndCheckFlow(context, node.Expression, out self)) {
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
            foreach (Expression current in node.Expressions) {
                addresses.Add(EvaluateAddress(context, current));
            }
            return new CommaAddress(node, addresses);
        }

        private static EvaluationAddress EvaluateAddress(CodeContext context, ConditionalExpression node) {
            object test = (bool)Interpret(context, node.Test);

            if ((bool)test) {
                return EvaluateAddress(context, node.IfTrue);
            } else {
                return EvaluateAddress(context, node.IfFalse);
            }
        }
    }
}
