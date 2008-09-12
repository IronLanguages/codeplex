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
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions.Compiler {
    partial class LambdaCompiler {
        /// <summary>
        /// Generates code for this expression in a value position.
        /// This method will leave the value of the expression
        /// on the top of the stack typed as Type.
        /// </summary>
        internal void EmitExpression(Expression node) {
            EmitExpression(node, true);
        }

        /// <summary>
        /// Emits an expression and discards the result.  For some nodes this emits
        /// more optimial code then EmitExpression/Pop
        /// </summary>
        private void EmitExpressionAsVoid(Expression node) {
            Debug.Assert(node != null);

            bool startEmitted = EmitExpressionStart(node);

            switch (node.NodeType) {
                case ExpressionType.Assign:
                    Emit((AssignmentExpression)node, EmitAs.Void);
                    break;
                case ExpressionType.Block:
                    Emit((Block)node, EmitAs.Void);
                    break;
                default:
                    EmitExpression(node, false);
                    if (node.Type != typeof(void)) {
                        _ilg.Emit(OpCodes.Pop);
                    }
                    break;
            }
            if (startEmitted) {
                EmitExpressionEnd();
            }
        }

        /// <summary>
        /// Generates the code for the expression, leaving it on
        /// the stack typed as object.
        /// </summary>
        private void EmitExpressionAsObject(Expression node) {
            EmitExpression(node);
            _ilg.EmitBoxing(node.Type);
        }

        #region DebugMarkers

        private bool EmitExpressionStart(Expression node) {
            if (!_emitDebugSymbols) {
                return false;
            }

            Annotations annotations = node.Annotations;
            SourceSpan span;
            SourceLocation header;

            if (annotations.TryGet<SourceSpan>(out span)) {
                if (annotations.TryGet<SourceLocation>(out header)) {
                    EmitPosition(span.Start, header);
                } else {
                    EmitPosition(span.Start, span.End);
                }
                return true;
            }
            return false;
        }

        private void EmitExpressionEnd() {
            EmitSequencePointNone();
        }

        #endregion

        #region InvocationExpression

        //CONFORMING
        private static void EmitInvocationExpression(LambdaCompiler lc, Expression expr) {
            InvocationExpression node = (InvocationExpression)expr;

            // Note: If node.Expression is a lambda, ExpressionCompiler inlines
            // the lambda here as an optimization. We don't, for various
            // reasons:
            //
            // * It's not necessarily optimal for large statement trees (JIT
            //   does better with small methods)
            // * We support returning from anywhere,
            // * The frame wouldn't show up in the stack trace,
            // * Possibly other subtle semantic differences
            //
            expr = node.Expression;
            if (typeof(LambdaExpression).IsAssignableFrom(expr.Type)) {
                // if the invoke target is a lambda expression tree, first compile it into a delegate
                expr = Expression.Call(expr, expr.Type.GetMethod("Compile", new Type[0]));
            }
            expr = Expression.Call(expr, expr.Type.GetMethod("Invoke"), node.Arguments);

            lc.EmitExpression(expr);
        }

        #endregion

        #region IndexExpression

        private static void EmitIndexExpression(LambdaCompiler lc, Expression expr) {
            var node = (IndexExpression)expr;

            // Emit instance, if calling an instance method
            Type objectType = null;
            if (node.Object != null) {
                lc.EmitInstance(node.Object, objectType = node.Object.Type);
            }

            // Emit indexes. We don't allow byref args, so no need to worry
            // about writebacks or EmitAddress
            foreach (var arg in node.Arguments) {
                lc.EmitExpression(arg);
            }

            lc.EmitGetIndexCall(node, objectType);
        }

        private void EmitIndexAssignment(AssignmentExpression node, EmitAs emitAs) {
            var index = (IndexExpression)node.Expression;

            // Emit instance, if calling an instance method
            Type objectType = null;
            if (index.Object != null) {
                EmitInstance(index.Object, objectType = index.Object.Type);
            }

            // Emit indexes. We don't allow byref args, so no need to worry
            // about writebacks or EmitAddress
            foreach (var arg in index.Arguments) {
                EmitExpression(arg);
            }

            // Emit value
            EmitExpression(node.Value);

            // Save the expression value, if needed
            LocalBuilder temp = null;
            if (emitAs != EmitAs.Void) {
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Stloc, temp = _ilg.GetLocal(node.Type));
            }

            EmitSetIndexCall(index, objectType);

            // Restore the value
            if (emitAs != EmitAs.Void) {
                _ilg.Emit(OpCodes.Ldloc, temp);
                _ilg.FreeLocal(temp);
            }
        }

        private void EmitGetIndexCall(IndexExpression node, Type objectType) {
            if (node.Indexer != null) {
                // For indexed properties, just call the getter
                var method = node.Indexer.GetGetMethod(true);
                EmitCall(objectType, method);
            } else if (node.Arguments.Count != 1) {
                // Multidimensional arrays, call get
                _ilg.Emit(OpCodes.Call, node.Object.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance));
            } else {
                // For one dimensional arrays, emit load
                _ilg.EmitLoadElement(node.Type);
            }
        }

        private void EmitSetIndexCall(IndexExpression node, Type objectType) {
            if (node.Indexer != null) {
                // For indexed properties, just call the setter
                var method = node.Indexer.GetSetMethod(true);
                EmitCall(objectType, method);
            } else if (node.Arguments.Count != 1) {
                // Multidimensional arrays, call set
                _ilg.Emit(OpCodes.Call, node.Object.Type.GetMethod("Set", BindingFlags.Public | BindingFlags.Instance));
            } else {
                // For one dimensional arrays, emit store
                _ilg.EmitStoreElement(node.Type);
            }
        }

        #endregion

        #region MethodCallExpression

        //CONFORMING
        private static void EmitMethodCallExpression(LambdaCompiler lc, Expression expr) {
            MethodCallExpression node = (MethodCallExpression)expr;

            lc.EmitMethodCall(node.Object, node.Method, node.Arguments);
        }

        //CONFORMING
        private void EmitMethodCall(Expression obj, MethodInfo method, ReadOnlyCollection<Expression> args) {
            // Emit instance, if calling an instance method
            Type objectType = null;
            if (!method.IsStatic) {
                EmitInstance(obj, objectType = obj.Type);
            }

            EmitMethodCall(method, args, objectType);
        }

        //CONFORMING
        // assumes 'object' of non-static call is already on stack
        private void EmitMethodCall(MethodInfo mi, ReadOnlyCollection<Expression> args, Type objectType) {

            // Emit arguments
            List<WriteBack> wb = EmitArguments(mi, args);

            // Emit the actual call
            OpCode callOp = UseVirtual(mi) ? OpCodes.Callvirt : OpCodes.Call;
            if (callOp == OpCodes.Callvirt && objectType.IsValueType) {
                // This automatically boxes value types if necessary.
                _ilg.Emit(OpCodes.Constrained, objectType);
            }
            if (mi.CallingConvention == CallingConventions.VarArgs) {
                _ilg.EmitCall(callOp, mi, args.Map(a => a.Type));
            } else {
                _ilg.Emit(callOp, mi);
            }

            // Emit writebacks for properties passed as "ref" arguments
            EmitWriteBack(wb);
        }

        private void EmitCall(Type objectType, MethodInfo method) {
            if (method.CallingConvention == CallingConventions.VarArgs) {
                throw Error.UnexpectedVarArgsCall(method.FormatSignature());
            }

            OpCode callOp = UseVirtual(method) ? OpCodes.Callvirt : OpCodes.Call;
            if (callOp == OpCodes.Callvirt && objectType.IsValueType) {
                _ilg.Emit(OpCodes.Constrained, objectType);
            }
            _ilg.Emit(callOp, method);
        }

        //CONFORMING
        // TODO: ILGen's EmitCall only does callvirt for the (virtual, ref)
        // case. We should probably fix it to work like UseVirtual, so ETs have
        // the same fail-fast behavior as C# when calling with a null instance
        private static bool UseVirtual(MethodInfo mi) {
            // There are two factors: is the method static, virtual or non-virtual instance?
            // And is the object ref or value?
            // The cases are:
            //
            // static, ref:     call
            // static, value:   call
            // virtual, ref:    callvirt
            // virtual, value:  call -- eg, double.ToString must be a non-virtual call to be verifiable.
            // instance, ref:   callvirt -- this looks wrong, but is verifiable and gives us a free null check.
            // instance, value: call
            //
            // We never need to generate a nonvirtual call to a virtual method on a reference type because
            // expression trees do not support "base.Foo()" style calling.
            // 
            // We could do an optimization here for the case where we know that the object is a non-null
            // reference type and the method is a non-virtual instance method.  For example, if we had
            // (new Foo()).Bar() for instance method Bar we don't need the null check so we could do a
            // call rather than a callvirt.  However that seems like it would not be a very big win for
            // most dynamically generated code scenarios, so let's not do that for now.

            if (mi.IsStatic) {
                return false;
            }
            if (mi.DeclaringType.IsValueType) {
                return false;
            }
            return true;
        }


        //CONFORMING
        private List<WriteBack> EmitArguments(MethodBase method, IList<Expression> args) {
            ParameterInfo[] pis = method.GetParameters();
            Debug.Assert(args.Count == pis.Length);

            var writeBacks = new List<WriteBack>();
            for (int i = 0, n = pis.Length; i < n; i++) {
                ParameterInfo parameter = pis[i];
                Expression argument = args[i];
                Type type = parameter.ParameterType;

                if (type.IsByRef) {
                    type = type.GetElementType();

                    WriteBack wb = EmitAddressWriteBack(argument, type);
                    if (wb != null) {
                        writeBacks.Add(wb);
                    }
                } else {
                    EmitExpression(argument);
                }
            }
            return writeBacks;
        }

        //CONFORMING
        private static void EmitWriteBack(IList<WriteBack> writeBacks) {
            foreach (WriteBack wb in writeBacks) {
                wb();
            }
        }

        #endregion

        //CONFORMING
        private static void EmitConstantExpression(LambdaCompiler lc, Expression expr) {
            ConstantExpression node = (ConstantExpression)expr;

            lc.EmitConstant(node.Value, node.Type);
        }

        //CONFORMING
        private void EmitConstant(object value, Type type) {
            // Try to emit the constant directly into IL
            if (_ilg.TryEmitConstant(value, type)) {
                return;
            }

            if (_boundConstants == null) {
                throw Error.RtConstRequiresBundDelegate();
            }

            type = TypeUtils.GetConstantType(type);

            int index;
            if (!_constantCache.TryGetValue(value, out index)) {
                index = _boundConstants.Count;
                _boundConstants.Add(value);
                _constantCache.Add(value, index);
            }

            // TODO: optimize to cache the constant in an IL local
            // (requires a tree walk before we start compiling)
            EmitClosureArgument();
            _ilg.Emit(OpCodes.Ldfld, typeof(Closure).GetField("Constants"));
            _ilg.EmitInt(index);
            _ilg.Emit(OpCodes.Ldelem_Ref);
            if (type.IsValueType) {
                _ilg.Emit(OpCodes.Unbox_Any, type);
            } else if (type != typeof(object)) {
                _ilg.Emit(OpCodes.Castclass, type);
            }
        }

        private static void EmitDynamicExpression(LambdaCompiler lc, Expression expr) {
            var node = (DynamicExpression)expr;

            Type siteType = typeof(CallSite<>).MakeGenericType(node.DelegateType);
            // TODO: is it worthwhile to lazy initialize CallSites?
            CallSite site = DynamicSiteHelpers.MakeSite(node.Binder, siteType);

            var invoke = node.DelegateType.GetMethod("Invoke");

            var siteVar = Expression.Variable(siteType, null);
            lc._scope.AddLocal(lc, siteVar);

            // site.Target.Invoke(site, args)
            lc.EmitConstant(site, siteType);
            lc._ilg.Emit(OpCodes.Dup);
            lc._scope.EmitSet(siteVar);
            lc._ilg.Emit(OpCodes.Ldfld, siteType.GetField("Target"));

            List<WriteBack> wb = lc.EmitArguments(invoke, node.Arguments.AddFirst(siteVar));
            lc._ilg.EmitCall(invoke);
            EmitWriteBack(wb);
        }

        //CONFORMING
        private static void EmitNewExpression(LambdaCompiler lc, Expression expr) {
            NewExpression node = (NewExpression)expr;

            if (node.Constructor != null) {
                List<WriteBack> wb = lc.EmitArguments(node.Constructor, node.Arguments);
                lc._ilg.Emit(OpCodes.Newobj, node.Constructor);
                EmitWriteBack(wb);
            } else {
                Debug.Assert(node.Arguments.Count == 0, "Node with arguments must have a constructor.");
                Debug.Assert(node.Type.IsValueType, "Only value type may have constructor not set.");
                LocalBuilder temp = lc._ilg.GetLocal(node.Type);
                lc._ilg.Emit(OpCodes.Ldloca, temp);
                lc._ilg.Emit(OpCodes.Initobj, node.Type);
                lc._ilg.Emit(OpCodes.Ldloc, temp);
                lc._ilg.FreeLocal(temp);
            }
        }

        //CONFORMING
        // TODO: There's plenty of room to optimize here, if we can determine
        // the answer statically. Be careful to use CLR semantics and not C#
        // semantics, however. The result must be identical to emitting isinst
        private static void EmitTypeBinaryExpression(LambdaCompiler lc, Expression expr) {
            TypeBinaryExpression node = (TypeBinaryExpression)expr;

            lc.EmitExpression(node.Expression);

            // Oddly enough, it is legal for an "is" expression to have a void-returning
            // method call on its left hand side.  In that case, always return false
            Type type = node.Expression.Type;
            if (type == typeof(void)) {
                lc._ilg.Emit(OpCodes.Ldc_I4_0);
                return;
            }

            if (type.IsValueType) {
                lc._ilg.Emit(OpCodes.Box, type);
            }
            lc._ilg.Emit(OpCodes.Isinst, node.TypeOperand);
            lc._ilg.Emit(OpCodes.Ldnull);
            lc._ilg.Emit(OpCodes.Cgt_Un);
        }

        private void EmitVariableAssignment(AssignmentExpression node, EmitAs emitAs) {
            Expression variable = node.Expression;

            if (TypeUtils.IsNullableType(node.Type)) {
                // Nullable<T> being assigned...
                if (ConstantCheck.IsConstant(node.Value, null)) {
                    _scope.EmitAddressOf(variable);
                    _ilg.Emit(OpCodes.Initobj, node.Type);
                    if (emitAs != EmitAs.Void) {
                        _scope.EmitGet(variable);
                    }
                    return;
                } else if (node.Type != node.Value.Type) {
                    throw new InvalidOperationException();
                }
                // fall through & emit the store from Nullable<T> -> Nullable<T>
            }
            EmitExpression(node.Value);
            if (emitAs != EmitAs.Void) {
                _ilg.Emit(OpCodes.Dup);
            }
            _scope.EmitSet(variable);
        }

        private static void EmitAssignmentExpression(LambdaCompiler lc, Expression expr) {
            lc.Emit((AssignmentExpression)expr, EmitAs.Default);
        }

        private void Emit(AssignmentExpression node, EmitAs emitAs) {
            switch (node.Expression.NodeType) {
                case ExpressionType.Index:
                    EmitIndexAssignment(node, emitAs);
                    return;
                case ExpressionType.MemberAccess:
                    EmitMemberAssignment(node, emitAs);
                    return;
                case ExpressionType.Parameter:
                case ExpressionType.Variable:
                    EmitVariableAssignment(node, emitAs);
                    return;
                default:
                    throw Error.InvalidLvalue(node.Expression.NodeType);
            }
        }

        private static void EmitVariableExpression(LambdaCompiler lc, Expression expr) {
            lc._scope.EmitGet(expr);
        }

        private static void EmitParameterExpression(LambdaCompiler lc, Expression expr) {
            ParameterExpression node = (ParameterExpression)expr;
            lc._scope.EmitGet(node);
            if (node.IsByRef) {
                lc._ilg.EmitLoadValueIndirect(node.Type);
            }
        }

        private static void EmitLambdaExpression(LambdaCompiler lc, Expression expr) {
            LambdaExpression node = (LambdaExpression)expr;
            lc.EmitDelegateConstruction(node, node.Type);
        }

        private static void EmitLocalScopeExpression(LambdaCompiler lc, Expression expr) {
            LocalScopeExpression node = (LocalScopeExpression)expr;
            lc._scope.EmitVariableAccess(lc, node.Variables);
        }

        private void EmitMemberAssignment(AssignmentExpression node, EmitAs emitAs) {
            MemberExpression lvalue = (MemberExpression)node.Expression;
            MemberInfo member = lvalue.Member;

            // emit "this", if any
            Type objectType = null;
            if (lvalue.Expression != null) {
                EmitInstance(lvalue.Expression, objectType = lvalue.Expression.Type);
            }

            // emit value
            EmitExpression(node.Value);

            LocalBuilder temp = null;
            if (emitAs != EmitAs.Void) {
                // save the value so we can return it
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Stloc, temp = _ilg.GetLocal(node.Type));
            }

            switch (member.MemberType) {
                case MemberTypes.Field:
                    _ilg.EmitFieldSet((FieldInfo)member);
                    break;
                case MemberTypes.Property:
                    EmitCall(objectType, ((PropertyInfo)member).GetSetMethod(true));
                    break;
                default:
                    throw Error.InvalidMemberType(member.MemberType);
            }

            if (emitAs != EmitAs.Void) {
                _ilg.Emit(OpCodes.Ldloc, temp);
                _ilg.FreeLocal(temp);
            }
        }

        //CONFORMING
        private static void EmitMemberExpression(LambdaCompiler lc, Expression expr) {
            MemberExpression node = (MemberExpression)expr;

            // emit "this", if any
            Type instanceType = null;
            if (node.Expression != null) {
                lc.EmitInstance(node.Expression, instanceType = node.Expression.Type);
            }

            lc.EmitMemberGet(node.Member, instanceType);
        }

        // assumes instance is already on the stack
        private void EmitMemberGet(MemberInfo member, Type objectType) {
            switch (member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo fi = (FieldInfo)member;
                    if (fi.IsLiteral) {
                        EmitConstant(fi.GetRawConstantValue(), fi.FieldType);
                    } else {
                        _ilg.EmitFieldGet(fi);
                    }
                    break;
                case MemberTypes.Property:
                    EmitCall(objectType, ((PropertyInfo)member).GetGetMethod(true));
                    break;
                default:
                    throw Assert.Unreachable;
            }
        }

        //CONFORMING
        private void EmitInstance(Expression instance, Type type) {
            if (instance != null) {
                if (type.IsValueType) {
                    EmitAddress(instance, type);
                } else {
                    EmitExpression(instance);
                }
            }
        }

        //CONFORMING
        private static void EmitNewArrayExpression(LambdaCompiler lc, Expression expr) {
            NewArrayExpression node = (NewArrayExpression)expr;

            if (node.NodeType == ExpressionType.NewArrayInit) {
                lc._ilg.EmitArray(
                    node.Type.GetElementType(),
                    node.Expressions.Count,
                    delegate(int index) {
                        lc.EmitExpression(node.Expressions[index]);
                    }
                );
            } else {
                ReadOnlyCollection<Expression> bounds = node.Expressions;
                for (int i = 0; i < bounds.Count; i++) {
                    Expression x = bounds[i];
                    lc.EmitExpression(x);
                    lc._ilg.EmitConvertToType(x.Type, typeof(int), true);
                }
                lc._ilg.EmitArray(node.Type);
            }
        }

        private static void EmitScopeExpression(LambdaCompiler lc, Expression expr) {
            ScopeExpression node = (ScopeExpression)expr;

            // If we merged the scope, just emit the body
            if (lc._scope.MergedScopes.Count > 0) {
                expr = lc._scope.MergedScopes.Dequeue();
                Debug.Assert(node == expr);

                lc.EmitExpression(node.Body);
                return;
            }

            // bind & push scope
            lc._scope = VariableBinder.Bind(lc._scope, expr);
            lc._scope.EnterScope(lc);

            // emit body
            lc.EmitExpression(node.Body);

            // pop scope
            lc._scope = lc._scope.Parent;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "lc")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "expr")]
        private static void EmitExtensionExpression(LambdaCompiler lc, Expression expr) {
            throw Error.ExtensionNotReduced();
        }

        #region ListInit, MemberInit

        private static void EmitListInitExpression(LambdaCompiler lc, Expression expr) {
            lc.EmitListInit((ListInitExpression)expr);
        }

        private static void EmitMemberInitExpression(LambdaCompiler lc, Expression expr) {
            lc.EmitMemberInit((MemberInitExpression)expr);
        }

        private void EmitBinding(MemberBinding binding, Type objectType) {
            switch (binding.BindingType) {
                case MemberBindingType.Assignment:
                    EmitMemberAssignment((MemberAssignment)binding, objectType);
                    break;
                case MemberBindingType.ListBinding:
                    EmitMemberListBinding((MemberListBinding)binding);
                    break;
                case MemberBindingType.MemberBinding:
                    EmitMemberMemberBinding((MemberMemberBinding)binding);
                    break;
                default:
                    throw Error.UnknownBindingType();
            }
        }

        private void EmitMemberAssignment(MemberAssignment binding, Type objectType) {
            EmitExpression(binding.Expression);
            FieldInfo fi = binding.Member as FieldInfo;
            if (fi != null) {
                _ilg.Emit(OpCodes.Stfld, fi);
            } else {
                PropertyInfo pi = binding.Member as PropertyInfo;
                if (pi != null) {
                    EmitCall(objectType, pi.GetSetMethod(true));
                } else {
                    throw Error.UnhandledBinding();
                }
            }
        }

        private void EmitMemberMemberBinding(MemberMemberBinding binding) {
            Type type = GetMemberType(binding.Member);
            if (binding.Member is PropertyInfo && type.IsValueType) {
                throw Error.CannotAutoInitializeValueTypeMemberThroughProperty(binding.Member);
            }
            if (type.IsValueType) {
                EmitMemberAddress(binding.Member, binding.Member.DeclaringType);
            } else {
                EmitMemberGet(binding.Member, binding.Member.DeclaringType);
            }
            if (binding.Bindings.Count == 0) {
                _ilg.Emit(OpCodes.Pop);
            } else {
                EmitMemberInit(binding.Bindings, false, type);
            }
        }

        private void EmitMemberListBinding(MemberListBinding binding) {
            Type type = GetMemberType(binding.Member);
            if (binding.Member is PropertyInfo && type.IsValueType) {
                throw Error.CannotAutoInitializeValueTypeElementThroughProperty(binding.Member);
            }
            if (type.IsValueType) {
                EmitMemberAddress(binding.Member, binding.Member.DeclaringType);
            } else {
                EmitMemberGet(binding.Member, binding.Member.DeclaringType);
            }
            EmitListInit(binding.Initializers, false, type);
        }

        private void EmitMemberInit(MemberInitExpression init) {
            EmitExpression(init.NewExpression);
            LocalBuilder loc = null;
            if (init.NewExpression.Type.IsValueType && init.Bindings.Count > 0) {
                loc = _ilg.DeclareLocal(init.NewExpression.Type);
                _ilg.Emit(OpCodes.Stloc, loc);
                _ilg.Emit(OpCodes.Ldloca, loc);
            }
            EmitMemberInit(init.Bindings, loc == null, init.NewExpression.Type);
            if (loc != null) {
                _ilg.Emit(OpCodes.Ldloc, loc);
            }
        }

        private void EmitMemberInit(ReadOnlyCollection<MemberBinding> bindings, bool keepOnStack, Type objectType) {
            for (int i = 0, n = bindings.Count; i < n; i++) {
                if (keepOnStack || i < n - 1) {
                    _ilg.Emit(OpCodes.Dup);
                }
                EmitBinding(bindings[i], objectType);
            }
        }

        private void EmitListInit(ListInitExpression init) {
            EmitExpression(init.NewExpression);
            LocalBuilder loc = null;
            if (init.NewExpression.Type.IsValueType) {
                loc = _ilg.DeclareLocal(init.NewExpression.Type);
                _ilg.Emit(OpCodes.Stloc, loc);
                _ilg.Emit(OpCodes.Ldloca, loc);
            }
            EmitListInit(init.Initializers, loc == null, init.NewExpression.Type);
            if (loc != null) {
                _ilg.Emit(OpCodes.Ldloc, loc);
            }
        }

        private void EmitListInit(ReadOnlyCollection<ElementInit> initializers, bool keepOnStack, Type objectType) {
            for (int i = 0, n = initializers.Count; i < n; i++) {
                if (keepOnStack || i < n - 1) {
                    _ilg.Emit(OpCodes.Dup);
                }
                EmitMethodCall(initializers[i].AddMethod, initializers[i].Arguments, objectType);

                // Aome add methods, ArrayList.Add for example, return non-void
                if (initializers[i].AddMethod.ReturnType != typeof(void)) {
                    _ilg.Emit(OpCodes.Pop);
                }
            }
        }

        private static Type GetMemberType(MemberInfo member) {
            FieldInfo fi = member as FieldInfo;
            if (fi != null) return fi.FieldType;
            PropertyInfo pi = member as PropertyInfo;
            if (pi != null) return pi.PropertyType;
            throw Error.MemberNotFieldOrProperty(member);
        }

        #endregion

        #region Expression helpers

        private void EmitExpressionAsObjectOrNull(Expression node) {
            if (node == null) {
                _ilg.Emit(OpCodes.Ldnull);
            } else {
                EmitExpressionAsObject(node);
            }
        }


        //CONFORMING
        internal static void ValidateLift(IList<VariableExpression> variables, IList<Expression> arguments) {
            System.Diagnostics.Debug.Assert(variables != null);
            System.Diagnostics.Debug.Assert(arguments != null);

            if (variables.Count != arguments.Count) {
                throw Error.IncorrectNumberOfIndexes();
            }
            for (int i = 0, n = variables.Count; i < n; i++) {
                if (!TypeUtils.AreReferenceAssignable(variables[i].Type, TypeUtils.GetNonNullableType(arguments[i].Type))) {
                    throw Error.ArgumentTypesMustMatch();
                }
            }
        }

        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void EmitLift(ExpressionType nodeType, Type resultType, MethodCallExpression mc, IList<VariableExpression> parameters, IList<Expression> arguments) {
            Debug.Assert(TypeUtils.GetNonNullableType(resultType) == TypeUtils.GetNonNullableType(mc.Type));
            ReadOnlyCollection<VariableExpression> paramList = new ReadOnlyCollection<VariableExpression>(parameters);
            ReadOnlyCollection<Expression> argList = new ReadOnlyCollection<Expression>(arguments);

            switch (nodeType) {
                default:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual: {
                        Label exit = _ilg.DefineLabel();
                        Label exitNull = _ilg.DefineLabel();
                        LocalBuilder anyNull = _ilg.DeclareLocal(typeof(bool));
                        for (int i = 0, n = paramList.Count; i < n; i++) {
                            VariableExpression v = paramList[i];
                            Expression arg = argList[i];
                            if (TypeUtils.IsNullableType(arg.Type)) {
                                _scope.AddLocal(this, v);
                                EmitAddress(arg, arg.Type);
                                _ilg.Emit(OpCodes.Dup);
                                _ilg.EmitHasValue(arg.Type);
                                _ilg.Emit(OpCodes.Ldc_I4_0);
                                _ilg.Emit(OpCodes.Ceq);
                                _ilg.Emit(OpCodes.Stloc, anyNull);
                                _ilg.EmitGetValueOrDefault(arg.Type);
                                _scope.EmitSet(v);
                            } else {
                                _scope.AddLocal(this, v);
                                EmitExpression(arg);
                                if (!arg.Type.IsValueType) {
                                    _ilg.Emit(OpCodes.Dup);
                                    _ilg.Emit(OpCodes.Ldnull);
                                    _ilg.Emit(OpCodes.Ceq);
                                    _ilg.Emit(OpCodes.Stloc, anyNull);
                                }
                                _scope.EmitSet(v);
                            }
                            _ilg.Emit(OpCodes.Ldloc, anyNull);
                            _ilg.Emit(OpCodes.Brtrue, exitNull);
                        }
                        EmitMethodCallExpression(this, mc);
                        if (TypeUtils.IsNullableType(resultType) && resultType != mc.Type) {
                            ConstructorInfo ci = resultType.GetConstructor(new Type[] { mc.Type });
                            _ilg.Emit(OpCodes.Newobj, ci);
                        }
                        _ilg.Emit(OpCodes.Br_S, exit);
                        _ilg.MarkLabel(exitNull);
                        if (resultType == TypeUtils.GetNullableType(mc.Type)) {
                            if (resultType.IsValueType) {
                                LocalBuilder result = _ilg.GetLocal(resultType);
                                _ilg.Emit(OpCodes.Ldloca, result);
                                _ilg.Emit(OpCodes.Initobj, resultType);
                                _ilg.Emit(OpCodes.Ldloc, result);
                                _ilg.FreeLocal(result);
                            } else {
                                _ilg.Emit(OpCodes.Ldnull);
                            }
                        } else {
                            switch (nodeType) {
                                case ExpressionType.LessThan:
                                case ExpressionType.LessThanOrEqual:
                                case ExpressionType.GreaterThan:
                                case ExpressionType.GreaterThanOrEqual:
                                    _ilg.Emit(OpCodes.Ldc_I4_0);
                                    break;
                                default:
                                    throw Error.UnknownLiftType(nodeType);
                            }
                        }
                        _ilg.MarkLabel(exit);
                        return;
                    }
                case ExpressionType.Equal:
                case ExpressionType.NotEqual: {
                        if (resultType == TypeUtils.GetNullableType(mc.Type)) {
                            goto default;
                        }
                        Label exit = _ilg.DefineLabel();
                        Label exitAllNull = _ilg.DefineLabel();
                        Label exitAnyNull = _ilg.DefineLabel();

                        LocalBuilder anyNull = _ilg.DeclareLocal(typeof(bool));
                        LocalBuilder allNull = _ilg.DeclareLocal(typeof(bool));
                        _ilg.Emit(OpCodes.Ldc_I4_0);
                        _ilg.Emit(OpCodes.Stloc, anyNull);
                        _ilg.Emit(OpCodes.Ldc_I4_1);
                        _ilg.Emit(OpCodes.Stloc, allNull);

                        for (int i = 0, n = paramList.Count; i < n; i++) {
                            VariableExpression v = paramList[i];
                            Expression arg = argList[i];
                            _scope.AddLocal(this, v);
                            if (TypeUtils.IsNullableType(arg.Type)) {
                                EmitAddress(arg, arg.Type);
                                _ilg.Emit(OpCodes.Dup);
                                _ilg.EmitHasValue(arg.Type);
                                _ilg.Emit(OpCodes.Ldc_I4_0);
                                _ilg.Emit(OpCodes.Ceq);
                                _ilg.Emit(OpCodes.Dup);
                                _ilg.Emit(OpCodes.Ldloc, anyNull);
                                _ilg.Emit(OpCodes.Or);
                                _ilg.Emit(OpCodes.Stloc, anyNull);
                                _ilg.Emit(OpCodes.Ldloc, allNull);
                                _ilg.Emit(OpCodes.And);
                                _ilg.Emit(OpCodes.Stloc, allNull);
                                _ilg.EmitGetValueOrDefault(arg.Type);
                            } else {
                                EmitExpression(arg);
                                if (!arg.Type.IsValueType) {
                                    _ilg.Emit(OpCodes.Dup);
                                    _ilg.Emit(OpCodes.Ldnull);
                                    _ilg.Emit(OpCodes.Ceq);
                                    _ilg.Emit(OpCodes.Dup);
                                    _ilg.Emit(OpCodes.Ldloc, anyNull);
                                    _ilg.Emit(OpCodes.Or);
                                    _ilg.Emit(OpCodes.Stloc, anyNull);
                                    _ilg.Emit(OpCodes.Ldloc, allNull);
                                    _ilg.Emit(OpCodes.And);
                                    _ilg.Emit(OpCodes.Stloc, allNull);
                                } else {
                                    _ilg.Emit(OpCodes.Ldc_I4_0);
                                    _ilg.Emit(OpCodes.Stloc, allNull);
                                }
                            }
                            _scope.EmitSet(v);
                        }
                        _ilg.Emit(OpCodes.Ldloc, allNull);
                        _ilg.Emit(OpCodes.Brtrue, exitAllNull);
                        _ilg.Emit(OpCodes.Ldloc, anyNull);
                        _ilg.Emit(OpCodes.Brtrue, exitAnyNull);

                        EmitMethodCallExpression(this, mc);
                        if (TypeUtils.IsNullableType(resultType) && resultType != mc.Type) {
                            ConstructorInfo ci = resultType.GetConstructor(new Type[] { mc.Type });
                            _ilg.Emit(OpCodes.Newobj, ci);
                        }
                        _ilg.Emit(OpCodes.Br_S, exit);

                        _ilg.MarkLabel(exitAllNull);
                        // TODO: emitting the bool as constant doesn't seem right
                        EmitConstant(nodeType == ExpressionType.Equal, typeof(bool));
                        _ilg.Emit(OpCodes.Br_S, exit);

                        _ilg.MarkLabel(exitAnyNull);
                        // TODO: emitting the bool as constant doesn't seem right
                        EmitConstant(nodeType == ExpressionType.NotEqual, typeof(bool));

                        _ilg.MarkLabel(exit);
                        return;
                    }
            }
        }

        #endregion

        enum EmitAs {
            Default,
            Void
        }
    }
}
