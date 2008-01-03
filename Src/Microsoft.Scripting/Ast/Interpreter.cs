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

        // TODO: Make internal
        public static object[] Evaluate(CodeContext context, IList<Expression> items) {
            Contract.RequiresNotNullItems(items, "items");

            object[] ret = new object[items.Count];
            for (int i = 0; i < items.Count; i++) {
                ret[i] = EvaluateExpression(context, items[i]);
            }
            return ret;
        }

        // Hold on to one instance for each member of the ControlFlow enumeration to avoid unnecessary boxing
        internal static readonly object NextStatement = ControlFlow.NextStatement;
        internal static readonly object Break = ControlFlow.Break;
        internal static readonly object Continue = ControlFlow.Continue;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static object EvaluateExpression(CodeContext context, Expression node) {
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
                    return Evaluate(context, (BinaryExpression)node);
                case AstNodeType.Call:
                    return Evaluate(context, (MethodCallExpression)node);
                case AstNodeType.Conditional:
                    return Evaluate(context, (ConditionalExpression)node);
                case AstNodeType.Constant:
                    return Evaluate((ConstantExpression)node);
                case AstNodeType.Convert:
                case AstNodeType.Negate:
                case AstNodeType.Not:
                case AstNodeType.OnesComplement:
                    return Evaluate(context, (UnaryExpression)node);
                case AstNodeType.New:
                    return Evaluate(context, (NewExpression)node);
                case AstNodeType.TypeIs:
                    return Evaluate(context, (TypeBinaryExpression)node);
                case AstNodeType.ActionExpression:
                    return Evaluate(context, (ActionExpression)node);
                case AstNodeType.ArrayIndexAssignment:
                    return Evaluate(context, (ArrayIndexAssignment)node);
                case AstNodeType.BoundAssignment:
                    return Evaluate(context, (BoundAssignment)node);
                case AstNodeType.BoundExpression:
                    return Evaluate(context, (BoundExpression)node);
                case AstNodeType.CodeBlockExpression:
                    return Evaluate(context, (CodeBlockExpression)node);
                case AstNodeType.CodeContextExpression:
                    return Evaluate(context);
                case AstNodeType.CommaExpression:
                    return Evaluate(context, (CommaExpression)node);
                case AstNodeType.DeleteUnboundExpression:
                    return Evaluate(context, (DeleteUnboundExpression)node);
                case AstNodeType.EnvironmentExpression:
                    return NotImplemented();
                case AstNodeType.MemberAssignment:
                    return Evaluate(context, (MemberAssignment)node);
                case AstNodeType.MemberExpression:
                    return Evaluate(context, (MemberExpression)node);
                case AstNodeType.NewArrayExpression:
                    return Evaluate(context, (NewArrayExpression)node);
                case AstNodeType.ParamsExpression:
                    return NotImplemented();
                case AstNodeType.UnboundAssignment:
                    return Evaluate(context, (UnboundAssignment)node);
                case AstNodeType.UnboundExpression:
                    return Evaluate(context, (UnboundExpression)node);
                case AstNodeType.VoidExpression:
                    return Evaluate(context, (VoidExpression)node);
                default:
                    throw new InvalidOperationException();
            }
        }
        
        private static object Evaluate(ConstantExpression node) {
            CompilerConstant cc = node.Value as CompilerConstant;
            if (cc != null) {
                return cc.Create(); // TODO: Only create once?
            }

            return node.Value;
        }

        private static object Evaluate(CodeContext context, ConditionalExpression node) {
            object ret = EvaluateExpression(context, node.Test);
            if ((bool)ret) {
                return EvaluateExpression(context, node.IfTrue);
            } else {
                return EvaluateExpression(context, node.IfFalse);
            }
        }

        private static object EvaluateInstance(CodeContext context, Expression instance, Type type) {
            object res = EvaluateExpression(context, instance);

            // box "this" if it is a value type (in case _method tries to modify it)
            // -- this keeps the same semantics as Emit().
            if (type != null && type.IsValueType) {
                res = System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(res);
            }
            return res;
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

        private static object Evaluate(CodeContext context, MethodCallExpression node) {
            object instance = null;
            // Evaluate the instance first (if the method is non-static)
            if (!node.Method.IsStatic) {
                instance = EvaluateInstance(context, node.Instance, node.Method.DeclaringType);
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
                        parameters[i] = arg != null ? EvaluateExpression(context, arg) : null;
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
            object ret = EvaluateExpression(context, node.Left);
            return ((bool)ret) ? EvaluateExpression(context, node.Right) : ret;
        }
        private static object EvaluateOrElse(CodeContext context, BinaryExpression node) {
            object ret = EvaluateExpression(context, node.Left);
            return ((bool)ret) ? ret : EvaluateExpression(context, node.Right);
        }

        private static object Evaluate(CodeContext context, BinaryExpression node) {
            object l = EvaluateExpression(context, node.Left);
            object r = EvaluateExpression(context, node.Right);

            if (node.Method!= null) {
                return node.Method.Invoke(null, new object[] { l, r });
            } else {
                return EvaluateBinaryOperator(node.NodeType, l, r);
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
        private static object Evaluate(CodeContext context, UnaryExpression node) {
            object x = EvaluateExpression(context, node.Operand);
            switch (node.NodeType) {
                case AstNodeType.Convert:
                    return Cast.Explicit(x, node.Type);

                case AstNodeType.Not:
                    if (x is bool) return (bool)x ? RuntimeHelpers.False : RuntimeHelpers.True;
                    if (x is int) return (int)~(int)x;
                    if (x is long) return (long)~(long)x;
                    if (x is short) return (short)~(short)x;
                    if (x is uint) return (uint)~(uint)x;
                    if (x is ulong) return (ulong)~(ulong)x;
                    if (x is ushort) return (ushort)~(ushort)x;
                    if (x is byte) return (byte)~(byte)x;
                    if (x is sbyte) return (sbyte)~(sbyte)x;
                    throw new InvalidOperationException("can't perform unary not on type " + CompilerHelpers.GetType(x).Name);

                case AstNodeType.Negate:
                    if (x is int) return (int)(-(int)x);
                    if (x is long) return (long)(-(long)x);
                    if (x is short) return (short)(-(short)x);
                    if (x is float) return -(float)x;
                    if (x is double) return -(double)x;
                    throw new InvalidOperationException("can't negate type " + CompilerHelpers.GetType(x).Name);

                default:
                    throw new NotImplementedException();
            }
        }

        private static object Evaluate(CodeContext context, NewExpression node) {
            object[] args = new object[node.Arguments.Count];
            for (int i = 0; i < node.Arguments.Count; i++) {
                args[i] = EvaluateExpression(context, node.Arguments[i]);
            }
            try {
                return node.Constructor.Invoke(args);
            } catch (TargetInvocationException e) {
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        private static object Evaluate(CodeContext context, TypeBinaryExpression node) {
            return RuntimeHelpers.BooleanToObject(
                node.TypeOperand.IsInstanceOfType(EvaluateExpression(context, node.Expression))
            );
        }

        private static object Evaluate(CodeContext context, ActionExpression node) {
            return context.LanguageContext.Binder.Execute(
                context,
                node.Action,
                CompilerHelpers.GetSiteTypes(node),
                Evaluate(context, node.Arguments)
            );
        }

        private static object Evaluate(CodeContext context, ArrayIndexAssignment node) {
            object value = EvaluateExpression(context, node.Value); // evaluate the value first
            Array array = (Array)EvaluateExpression(context, node.Array);
            int index = (int)EvaluateExpression(context, node.Index);
            array.SetValue(value, index);
            return value;
        }

        private static object Evaluate(CodeContext context, BoundAssignment node) {
            object value = EvaluateExpression(context, node.Value);
            EvaluateAssignVariable(context, node.Variable, value);
            return value;
        }

        private static object Evaluate(CodeContext context, BoundExpression node) {
            object ret;
            switch (node.Variable.Kind) {
                case Variable.VariableKind.Temporary:
                case Variable.VariableKind.GeneratorTemporary:
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

        private static object Evaluate(CodeContext context, CodeBlockExpression node) {
            return GetDelegateForInterpreter(node.Block, context, node.DelegateType, node.ForceWrapperMethod);
        }

        private static object Evaluate(CodeContext context) {
            return context;
        }

        private static object Evaluate(CodeContext context, CommaExpression node) {
            object result = null;
            for (int index = 0; index < node.Expressions.Count; index++) {
                Expression current = node.Expressions[index];

                if (current != null) {
                    object val = EvaluateExpression(context, current);
                    if (index == node.ValueIndex) {
                        // Save the value at the designated index
                        result = val;
                    }
                }
            }
            return result;
        }

        private static object Evaluate(CodeContext context, DeleteUnboundExpression node) {
            return RuntimeHelpers.RemoveName(context, node.Name);
        }

        private static object Evaluate(CodeContext context, MemberAssignment node) {
            object target = node.Expression != null ? EvaluateExpression(context, node.Expression) : null;
            object value = EvaluateExpression(context, node.Value);

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

        private static object Evaluate(CodeContext context, MemberExpression node) {
            object self = node.Expression != null ? EvaluateExpression(context, node.Expression) : null;
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

        private static object Evaluate(CodeContext context, NewArrayExpression node) {
            if (node.Type.GetElementType().IsValueType) {
                // value arrays cannot be cast to object arrays
                object contents = (object)node.Constructor.Invoke(new object[] { node.Expressions.Count });
                MethodInfo setter = node.Type.GetMethod("Set");
                for (int i = 0; i < node.Expressions.Count; i++) {
                    setter.Invoke(
                        contents,
                        new object[] {
                            i,
                            EvaluateExpression(context, node.Expressions[i])
                        }
                    );
                }
                return contents;
            } else {
                object[] contents = (object[])node.Constructor.Invoke(new object[] { node.Expressions.Count });
                for (int i = 0; i < node.Expressions.Count; i++) {
                    contents[i] = EvaluateExpression(context, node.Expressions[i]);
                }
                return contents;
            }
        }

        private static object Evaluate(CodeContext context, UnboundAssignment node) {
            object value = EvaluateExpression(context, node.Value);
            RuntimeHelpers.SetName(context, node.Name, value);
            return value;
        }

        private static object Evaluate(CodeContext context, UnboundExpression node) {
            return RuntimeHelpers.LookupName(context, node.Name);
        }

        private static object Evaluate(CodeContext context, VoidExpression node) {
            object ret = ExecuteStatement(context, node.Statement);

            if (ret != NextStatement) {
                throw new ExpressionReturnException(ret);
            }

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static object ExecuteStatement(CodeContext context, Statement node) {
            context.Scope.SourceLocation = node.Start;
            try {
#if DEBUG
                ExpressionReturnException.CurrentDepth++;
#endif
                switch (node.NodeType) {
                    case AstNodeType.BlockStatement:
                        return Execute(context, (BlockStatement)node);
                    case AstNodeType.BreakStatement:
                        return ExecuteBreak();
                    case AstNodeType.ContinueStatement:
                        return ExecuteContinue();
                    case AstNodeType.DeleteStatement:
                        return Execute(context, (DeleteStatement)node);
                    case AstNodeType.DoStatement:
                        return Execute(context, (DoStatement)node);
                    case AstNodeType.EmptyStatement:
                        return ExecuteEmpty();
                    case AstNodeType.ExpressionStatement:
                        return Execute(context, (ExpressionStatement)node);
                    case AstNodeType.IfStatement:
                        return Execute(context, (IfStatement)node);
                    case AstNodeType.LabeledStatement:
                        return ExecuteLabeled();
                    case AstNodeType.LoopStatement:
                        return Execute(context, (LoopStatement)node);
                    case AstNodeType.ReturnStatement:
                        return Execute(context, (ReturnStatement)node);
                    case AstNodeType.ScopeStatement:
                        return Execute(context, (ScopeStatement)node);
                    case AstNodeType.SwitchStatement:
                        return Execute(context, (SwitchStatement)node);
                    case AstNodeType.ThrowStatement:
                        return Execute(context, (ThrowStatement)node);
                    case AstNodeType.TryStatement:
                        return Execute(context, (TryStatement)node);
                    case AstNodeType.YieldStatement:
                        return NotImplemented();
                    default:
                        throw new InvalidOperationException();
                }
            } catch (ExpressionReturnException ex) {
#if DEBUG
                Debug.Assert(ex.Depth == ExpressionReturnException.CurrentDepth);
#endif
                return ex.Value;
#if DEBUG
            } finally {
                ExpressionReturnException.CurrentDepth--;
#endif
            }
        }

        private static object Execute(CodeContext context, BlockStatement node) {
            object ret = NextStatement;
            foreach (Statement stmt in node.Statements) {
                ret = ExecuteStatement(context, stmt);
                if (ret != NextStatement) {
                    break;
                }
            }
            return ret;
        }

        private static object ExecuteBreak() {
            return Break;
        }

        private static object ExecuteContinue() {
            return Continue;
        }

        // TODO: Should not be name-based.
        private static object Execute(CodeContext context, DeleteStatement node) {
            switch (node.Variable.Kind) {
                case Variable.VariableKind.Temporary:
                case Variable.VariableKind.GeneratorTemporary:
                    context.Scope.TemporaryStorage.Remove(node.Variable);
                    break;
                case Variable.VariableKind.Global:
                    RuntimeHelpers.RemoveGlobalName(context, node.Variable.Name);
                    break;
                default:
                    RuntimeHelpers.RemoveName(context, node.Variable.Name);
                    break;
            }

            return NextStatement;
        }

        private static object Execute(CodeContext context, DoStatement node) {
            object ret = NextStatement;

            do {
                ret = ExecuteStatement(context, node.Body);
                if (ret == Break) {
                    break;
                } else if (!(ret is ControlFlow)) {
                    return ret;
                }
            } while ((bool)EvaluateExpression(context, node.Test));

            return NextStatement;
        }

        private static object ExecuteEmpty() {
            return NextStatement;
        }

        private static object Execute(CodeContext context, ExpressionStatement node) {
            EvaluateExpression(context, node.Expression);
            return NextStatement;
        }

        private static object Execute(CodeContext context, IfStatement node) {
            foreach (IfStatementTest t in node.Tests) {
                if ((bool)EvaluateExpression(context, t.Test)) {
                    return ExecuteStatement(context, t.Body);
                }
            }
            if (node.ElseStatement != null) {
                return ExecuteStatement(context, node.ElseStatement);
            }
            return NextStatement;
        }

        private static object ExecuteLabeled() {
            throw new NotImplementedException();
        }

        private static object Execute(CodeContext context, LoopStatement node) {
            object ret = NextStatement;
            while (node.Test == null || (bool)EvaluateExpression(context, node.Test)) {
                ret = ExecuteStatement(context, node.Body);
                if (ret == Break) {
                    return NextStatement;
                } else if (!(ret is ControlFlow)) {
                    return ret;
                }
                if (node.Increment != null) {
                    EvaluateExpression(context, node.Increment);
                }
            }
            if (node.ElseStatement != null) {
                return ExecuteStatement(context, node.ElseStatement);
            }
            return NextStatement;
        }

        private static object Execute(CodeContext context, ReturnStatement node) {
            if (node.Expression != null) {
                return EvaluateExpression(context, node.Expression);
            } else {
                return null;
            }
        }

        private static object Execute(CodeContext context, ScopeStatement node) {
            CodeContext scopeContext;
            // TODO: should work with LocalScope
            if (node.Scope != null) {
                IAttributesCollection scopeObject = EvaluateExpression(context, node.Scope) as IAttributesCollection;
                scopeContext = RuntimeHelpers.CreateNestedCodeContext(scopeObject, context, true);
            } else {
                scopeContext = RuntimeHelpers.CreateCodeContext(context);
            }
            ExecuteStatement(scopeContext, node.Body);
            return NextStatement;
        }

        private static object Execute(CodeContext context, SwitchStatement node) {
            int test = (int)EvaluateExpression(context, node.TestValue);

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
                object result = ExecuteStatement(context, sc.Body);
                if (result == Continue) {
                    return result;
                } else if (result == Break) {
                    return NextStatement;
                }
                target++;
            }
            return NextStatement;
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

        internal static Exception LastEvalException {
            get {
                if (_evalExceptions == null || _evalExceptions.Count == 0) {
                    throw new InvalidOperationException("rethrow outside of catch block");
                }

                return _evalExceptions[_evalExceptions.Count - 1];
            }
        }



        private static object Execute(CodeContext context, ThrowStatement node) {
            if (node.Value == null) {
                throw LastEvalException;
            } else {
                throw (Exception)EvaluateExpression(context, node.Value);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses")]
        private static object Execute(CodeContext context, TryStatement node) {
            bool rethrow = false;
            Exception savedExc = null;
            object ret = NextStatement;
            try {
                ret = ExecuteStatement(context, node.Body);
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
                                ret = ExecuteStatement(context, handler.Body);
                                break;
                            }
                        }
                    } finally {
                        PopEvalException();
                    }
                }
            } finally {
                if (node.FinallyStatement != null) {
                    object finallyRet = ExecuteStatement(context, node.FinallyStatement);
                    if (finallyRet != NextStatement) {
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

        internal static object EvaluateAssignVariable(CodeContext context, Variable var, object value) {
            switch (var.Kind) {
                case Variable.VariableKind.Temporary:
                case Variable.VariableKind.GeneratorTemporary:
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
            object self = node.Expression != null ? EvaluateExpression(context, node.Expression) : null;
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


        internal static EvaluationAddress EvaluateAddress(CodeContext context, Expression node) {
            switch (node.NodeType) {
                case AstNodeType.BoundExpression:
                    return EvaluateAddress((BoundExpression)node);
                case AstNodeType.CommaExpression:
                    return EvaluateAddress(context, (CommaExpression)node);
                case AstNodeType.Conditional:
                    return EvaluateAddress(context, (ConditionalExpression)node);
                default:
                    return new EvaluationAddress(node);
            }
        }

        private static EvaluationAddress EvaluateAddress(BoundExpression node) {
            return new VariableAddress(node);
        }

        private static EvaluationAddress EvaluateAddress(CodeContext context, CommaExpression node) {
            List<EvaluationAddress> addresses = new List<EvaluationAddress>();
            foreach(Expression current in node.Expressions) {
                if (current != null) {
                    addresses.Add(EvaluateAddress(context, current));
                } else {
                    addresses.Add(null);
                }
            }
            return new CommaAddress(node, addresses);
        }

        private static EvaluationAddress EvaluateAddress(CodeContext context, ConditionalExpression node) {
            bool test = (bool)Interpreter.EvaluateExpression(context, node.Test);

            if (test) {
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
