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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    partial class LambdaCompiler {

        // TODO: need to remove the address-of support for some of the nodes
        // marked below, because 1) it doesn't make sense (there is no IL
        // equivalent), and/or 2) it's a breaking change from LinqV1
        //
        // We don't want to "ref" parameters to modify values of expressions
        // except where it would in IL: locals, args, fields, and array elements
        // (Unbox is an exception, it's intended to emit a ref to the orignal
        // boxed value)
        private void EmitAddress(Expression node, Type type) {
            Debug.Assert(node != null);

            switch (node.NodeType) {
                default:
                    EmitExpressionAddress(node, type);
                    break;

                case ExpressionType.ArrayIndex:
                    AddressOf((BinaryExpression)node, type);
                    break;

                // TODO: remove
                case ExpressionType.Conditional:
                    AddressOf((ConditionalExpression)node, type);
                    break;

                // TODO: remove
                case ExpressionType.Assign:
                    AddressOf((AssignmentExpression)node);
                    break;

                case ExpressionType.Variable:
                    AddressOf((VariableExpression)node, type);
                    break;

                case ExpressionType.Parameter:
                    AddressOf((ParameterExpression)node, type);
                    break;

                // TODO: remove
                case ExpressionType.Block:
                    AddressOf((Block)node, type);
                    break;

                case ExpressionType.MemberAccess:
                    AddressOf((MemberExpression)node, type);
                    break;

                case ExpressionType.Unbox:
                    AddressOf((UnaryExpression)node, type);
                    break;

                case ExpressionType.Call:
                    AddressOf((MethodCallExpression)node, type);
                    break;

                case ExpressionType.Index:
                    AddressOf((IndexExpression)node, type);
                    break;
            }
        }

        //CONFORMING
        private void AddressOf(BinaryExpression node, Type type) {
            Debug.Assert(node.NodeType == ExpressionType.ArrayIndex && node.Method == null);

            if (type == node.Type) {
                EmitExpression(node.Left);
                EmitExpression(node.Right);
                Type rightType = node.Right.Type;
                if (TypeUtils.IsNullableType(rightType)) {
                    LocalBuilder loc = _ilg.GetLocal(rightType);
                    _ilg.Emit(OpCodes.Stloc, loc);
                    _ilg.Emit(OpCodes.Ldloca, loc);
                    _ilg.EmitGetValue(rightType);
                    _ilg.FreeLocal(loc);
                }
                Type indexType = TypeUtils.GetNonNullableType(rightType);
                if (indexType != typeof(int)) {
                    _ilg.EmitConvertToType(indexType, typeof(int), true);
                }
                _ilg.Emit(OpCodes.Ldelema, node.Type);
            } else {
                EmitExpressionAddress(node, type);
            }
        }

        private void AddressOf(AssignmentExpression node) {
            ContractUtils.Requires(node.Expression is VariableExpression || node.Expression is ParameterExpression, "node");

            EmitExpression(node.Value);
            _scope.EmitSet(node.Expression);
            _scope.EmitAddressOf(node.Expression);
        }

        private void AddressOf(VariableExpression node, Type type) {
            if (type == node.Type) {
                _scope.EmitAddressOf(node);
            } else {
                EmitExpressionAddress(node, type);
            }
        }

        private void AddressOf(ParameterExpression node, Type type) {
            if (type == node.Type) {
                if (node.IsByRef) {
                    _scope.EmitGet(node);
                } else {
                    _scope.EmitAddressOf(node);
                }
            } else {
                EmitExpressionAddress(node, type);
            }
        }

        private void AddressOf(ConditionalExpression node, Type type) {
            Label eoi = _ilg.DefineLabel();
            Label next = _ilg.DefineLabel();
            EmitExpression(node.Test);
            _ilg.Emit(OpCodes.Brfalse, next);
            EmitAddress(node.IfTrue, type);
            _ilg.Emit(OpCodes.Br, eoi);
            _ilg.MarkLabel(next);
            EmitAddress(node.IfFalse, type);
            _ilg.MarkLabel(eoi);
        }

        private void AddressOf(Block node, Type type) {
            ReadOnlyCollection<Expression> expressions = node.Expressions;

            // TODO: Do something better
            if (node.Type == typeof(void)) {
                throw Error.AddressOfVoidBlock();
            }

            for (int index = 0; index < expressions.Count; index++) {
                Expression current = expressions[index];

                // Emit the expression
                if (index == expressions.Count - 1) {
                    EmitAddress(current, type);
                } else {
                    EmitExpressionAsVoid(current);
                }
            }
        }

        //CONFORMING
        private void AddressOf(MemberExpression node, Type type) {
            if (type == node.Type) {
                // emit "this", if any
                Type objectType = null;
                if (node.Expression != null) {
                    EmitInstance(node.Expression, objectType = node.Expression.Type);
                }
                EmitMemberAddress(node.Member, objectType);
            } else {
                EmitExpressionAddress(node, type);
            }
        }

        // assumes the instance is already on the stack
        private void EmitMemberAddress(MemberInfo member, Type objectType) {
            if (member.MemberType == MemberTypes.Field) {
                FieldInfo field = (FieldInfo)member;

                // Verifiable code may not take the address of an init-only field.
                // If we are asked to do so then get the value out of the field, stuff it
                // into a local of the same type, and then take the address of the local.
                // Typically this is what we want to do anyway; if we are saying
                // Foo.bar.ToString() for a static value-typed field bar then we don't need
                // the address of field bar to do the call.  The address of a local which
                // has the same value as bar is sufficient.

                // CONSIDER:
                // The C# compiler will not compile a lambda expression tree 
                // which writes to the address of an init-only field. But one could
                // probably use the expression tree API to build such an expression.
                // (When compiled, such an expression would fail silently.)  It might
                // be worth it to add checking to the expression tree API to ensure
                // that it is illegal to attempt to write to an init-only field,
                // the same way that it is illegal to write to a read-only property.
                // The same goes for literal fields.
                if (!field.IsLiteral && !field.IsInitOnly) {
                    _ilg.EmitFieldAddress(field);
                    return;
                }
            }

            EmitMemberGet(member, objectType);
            LocalBuilder temp = _ilg.GetLocal(GetMemberType(member));
            _ilg.Emit(OpCodes.Stloc, temp);
            _ilg.Emit(OpCodes.Ldloca, temp);
            _ilg.FreeLocal(temp);
        }

        //CONFORMING
        private void AddressOf(MethodCallExpression node, Type type) {
            // An array index of a multi-dimensional array is represented by a call to Array.Get,
            // rather than having its own array-access node. This means that when we are trying to
            // get the address of a member of a multi-dimensional array, we'll be trying to
            // get the address of a Get method, and it will fail to do so. Instead, detect
            // this situation and replace it with a call to the Address method.
            if (!node.Method.IsStatic &&
                node.Object.Type.IsArray &&
                node.Method == node.Object.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance)) {

                MethodInfo mi = node.Object.Type.GetMethod("Address", BindingFlags.Public | BindingFlags.Instance);

                EmitMethodCall(node.Object, mi, node.Arguments);
            } else {
                EmitExpressionAddress(node, type);
            }
        }

        private void AddressOf(IndexExpression node, Type type) {
            if (type != node.Type || node.Indexer != null) {
                EmitExpressionAddress(node, type);
                return;
            }

            if (node.Arguments.Count == 1) {
                EmitExpression(node.Object);
                EmitExpression(node.Arguments[0]);
                _ilg.Emit(OpCodes.Ldelema, node.Type);
            } else {
                var address = node.Object.Type.GetMethod("Address", BindingFlags.Public | BindingFlags.Instance);
                EmitMethodCall(node.Object, address, node.Arguments);
            }
        }

        private void AddressOf(UnaryExpression node, Type type) {
            Debug.Assert(node.NodeType == ExpressionType.Unbox);
            Debug.Assert(type.IsValueType && !TypeUtils.IsNullableType(type));

            // Unbox leaves a pointer to the boxed value on the stack
            EmitExpression(node.Operand);
            _ilg.Emit(OpCodes.Unbox, type);
        }

        private void EmitExpressionAddress(Expression node, Type type) {
            Debug.Assert(TypeUtils.CanAssign(type, node.Type));

            EmitExpression(node);
            LocalBuilder tmp = _ilg.GetLocal(type);
            _ilg.Emit(OpCodes.Stloc, tmp);
            _ilg.Emit(OpCodes.Ldloca, tmp);
            _ilg.FreeLocal(tmp);
        }


        // Emits the address of the expression, returning the write back if necessary
        //
        // For properties, we want to write back into the property if it's
        // passed byref.
        // 
        // Note: ExpressionCompiler thinks it needs to writeback
        // fields, but as far as I can tell, that code path is
        // unreachable because fields can always emit their address
        private WriteBack EmitAddressWriteBack(Expression node, Type type) {
            WriteBack result = null;
            if (type == node.Type) {
                switch (node.NodeType) {
                    case ExpressionType.MemberAccess:
                        result = AddressOfWriteBack((MemberExpression)node);
                        break;
                    case ExpressionType.Index:
                        result = AddressOfWriteBack((IndexExpression)node);
                        break;
                }
            }
            if (result == null) {
                EmitAddress(node, type);
            }
            return result;
        }

        private WriteBack AddressOfWriteBack(MemberExpression node) {
            if (!node.CanWrite || node.Member.MemberType != MemberTypes.Property) {
                return null;
            }

            // emit instance, if any
            LocalBuilder instanceLocal = null;
            Type instanceType = null;
            if (node.Expression != null) {
                EmitInstance(node.Expression, instanceType = node.Expression.Type);
                // store in local
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Stloc, instanceLocal = _ilg.GetLocal(instanceType));
            }

            PropertyInfo pi = (PropertyInfo)node.Member;

            // emit the get
            EmitCall(instanceType, pi.GetGetMethod(true));

            // emit the address of the value
            var valueLocal = _ilg.GetLocal(node.Type);
            _ilg.Emit(OpCodes.Stloc, valueLocal);
            _ilg.Emit(OpCodes.Ldloca, valueLocal);

            // Set the property after the method call
            // don't re-evaluate anything
            return delegate() {
                if (instanceLocal != null) {
                    _ilg.Emit(OpCodes.Ldloc, instanceLocal);
                    _ilg.FreeLocal(instanceLocal);
                }
                _ilg.Emit(OpCodes.Ldloc, valueLocal);
                _ilg.FreeLocal(valueLocal);
                EmitCall(instanceType, pi.GetSetMethod(true));
            };
        }

        private WriteBack AddressOfWriteBack(IndexExpression node) {
            if (!node.CanWrite || node.Indexer == null) {
                return null;
            }

            // emit instance, if any
            LocalBuilder instanceLocal = null;
            Type instanceType = null;
            if (node.Object != null) {
                EmitInstance(node.Object, instanceType = node.Object.Type);
                
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Stloc, instanceLocal = _ilg.GetLocal(instanceType));
            }

            // Emit indexes. We don't allow byref args, so no need to worry
            // about writebacks or EmitAddress
            List<LocalBuilder> args = new List<LocalBuilder>();
            foreach (var arg in node.Arguments) {
                EmitExpression(arg);

                var argLocal = _ilg.GetLocal(arg.Type);
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Stloc, argLocal);
                args.Add(argLocal);
            }

            // emit the get
            EmitGetIndexCall(node, instanceType);

            // emit the address of the value
            var valueLocal = _ilg.GetLocal(node.Type);
            _ilg.Emit(OpCodes.Stloc, valueLocal);
            _ilg.Emit(OpCodes.Ldloca, valueLocal);

            // Set the property after the method call
            // don't re-evaluate anything
            return delegate() {
                if (instanceLocal != null) {
                    _ilg.Emit(OpCodes.Ldloc, instanceLocal);
                    _ilg.FreeLocal(instanceLocal);
                }
                foreach (var arg in args) {
                    _ilg.Emit(OpCodes.Ldloc, arg);
                    _ilg.FreeLocal(arg);
                }
                _ilg.Emit(OpCodes.Ldloc, valueLocal);
                _ilg.FreeLocal(valueLocal);

                EmitSetIndexCall(node, instanceType);
            };
        }
    }
}
