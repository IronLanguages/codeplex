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
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {

    public delegate void EmitArrayHelper(int index);

    public class ILGen {
        private readonly ILGenerator _ilg;
        private readonly TypeGen _tg;
        private readonly List<Slot> _freeSlots = new List<Slot>();

        #region Constructor

        public ILGen(ILGenerator ilg) :
            this(ilg, null) {
        }

        internal ILGen(ILGenerator ilg, TypeGen tg) {
            CodeContract.RequiresNotNull(ilg, "ilg");

            _ilg = ilg;
            _tg = tg;
        }

        #endregion

        #region ILGenerator Methods

        /// <summary>
        /// Begins a catch block.
        /// </summary>
        public virtual void BeginCatchBlock(Type exceptionType) {
            _ilg.BeginCatchBlock(exceptionType);
        }

        /// <summary>
        /// Begins an exception block for a filtered exception.
        /// </summary>
        public virtual void BeginExceptFilterBlock() {
            _ilg.BeginExceptionBlock();
        }

        /// <summary>
        /// Begins an exception block for a non-filtered exception.
        /// </summary>
        /// <returns></returns>
        public virtual Label BeginExceptionBlock() {
            return _ilg.BeginExceptionBlock();
        }

        /// <summary>
        /// Begins an exception fault block
        /// </summary>
        public virtual void BeginFaultBlock() {
            _ilg.BeginFaultBlock();
        }

        /// <summary>
        /// Begins a finally block
        /// </summary>
        public virtual void BeginFinallyBlock() {
            _ilg.BeginFinallyBlock();
        }

        /// <summary>
        /// Ends an exception block.
        /// </summary>
        public virtual void EndExceptionBlock() {
            _ilg.EndExceptionBlock();
        }

        /// <summary>
        /// Begins a lexical scope.
        /// </summary>
        public virtual void BeginScope() {
            _ilg.BeginScope();
        }

        /// <summary>
        /// Ends a lexical scope.
        /// </summary>
        public virtual void EndScope() {
            _ilg.EndScope();
        }

        /// <summary>
        /// Declares a local variable of the specified type.
        /// </summary>
        public virtual LocalBuilder DeclareLocal(Type localType) {
            return _ilg.DeclareLocal(localType);
        }

        /// <summary>
        /// Declares a local variable of the specified type, optionally
        /// pinning the object referred to by the variable.
        /// </summary>
        public virtual LocalBuilder DeclareLocal(Type localType, bool pinned) {
            return _ilg.DeclareLocal(localType, pinned);
        }

        /// <summary>
        /// Declares a new label.
        /// </summary>
        public virtual Label DefineLabel() {
            return _ilg.DefineLabel();
        }

        /// <summary>
        /// Marks the label at the current position.
        /// </summary>
        public virtual void MarkLabel(Label loc) {
            _ilg.MarkLabel(loc);
        }

        /// <summary>
        /// Emits an instruction.
        /// </summary>
        public virtual void Emit(OpCode opcode) {
            _ilg.Emit(opcode);
        }

        /// <summary>
        /// Emits an instruction with a byte argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, byte arg) {
            _ilg.Emit(opcode, arg);
        }

        /// <summary>
        /// Emits an instruction with the metadata token for the specified contructor.
        /// </summary>
        public virtual void Emit(OpCode opcode, ConstructorInfo con) {
            _ilg.Emit(opcode, con);
        }

        /// <summary>
        /// Emits an instruction with a double argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, double arg) {
            _ilg.Emit(opcode, arg);
        }

        /// <summary>
        /// Emits an instruction with the metadata token for the specified field.
        /// </summary>
        public virtual void Emit(OpCode opcode, FieldInfo field) {
            _ilg.Emit(opcode, field);
        }

        /// <summary>
        /// Emits an instruction with a float argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, float arg) {
            _ilg.Emit(opcode, arg);
        }

        /// <summary>
        /// Emits an instruction with an int argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, int arg) {
            _ilg.Emit(opcode, arg);
        }

        /// <summary>
        /// Emits an instruction with a label argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, Label label) {
            _ilg.Emit(opcode, label);
        }

        /// <summary>
        /// Emits an instruction with multiple target labels (switch).
        /// </summary>
        public virtual void Emit(OpCode opcode, Label[] labels) {
            _ilg.Emit(opcode, labels);
        }

        /// <summary>
        /// Emits an instruction with a reference to a local variable.
        /// </summary>
        public virtual void Emit(OpCode opcode, LocalBuilder local) {
            _ilg.Emit(opcode, local);
        }

        /// <summary>
        /// Emits an instruction with a long argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, long arg) {
            _ilg.Emit(opcode, arg);
        }

        /// <summary>
        /// Emits an instruction with the metadata token for a specified method.
        /// </summary>
        public virtual void Emit(OpCode opcode, MethodInfo meth) {
            _ilg.Emit(opcode, meth);
        }

        /// <summary>
        /// Emits an instruction with a signed byte argument.
        /// </summary>
        [CLSCompliant(false)]
        public virtual void Emit(OpCode opcode, sbyte arg) {
            _ilg.Emit(opcode, arg);
        }

        /// <summary>
        /// Emits an instruction with a short argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, short arg) {
            _ilg.Emit(opcode, arg);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Emits an instruction with a signature token.
        /// </summary>
        public virtual void Emit(OpCode opcode, SignatureHelper signature) {
            _ilg.Emit(opcode, signature);
        }
#endif

        /// <summary>
        /// Emits an instruction with a string argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, string str) {
            _ilg.Emit(opcode, str);
        }

        /// <summary>
        /// Emits an instruction with the metadata token for a specified type argument.
        /// </summary>
        public virtual void Emit(OpCode opcode, Type cls) {
            _ilg.Emit(opcode, cls);
        }

        /// <summary>
        /// Emits a call or a virtual call to the varargs method.
        /// </summary>
        public virtual void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes) {
            _ilg.EmitCall(opcode, methodInfo, optionalParameterTypes);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Emits an unmanaged indirect call instruction.
        /// </summary>
        public virtual void EmitCalli(OpCode opcode, CallingConvention unmanagedCallConv, Type returnType, Type[] parameterTypes) {
            _ilg.EmitCalli(opcode, unmanagedCallConv, returnType, parameterTypes);
        }

        /// <summary>
        /// Emits a managed indirect call instruction.
        /// </summary>
        public virtual void EmitCalli(OpCode opcode, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes) {
            _ilg.EmitCalli(opcode, callingConvention, returnType, parameterTypes, optionalParameterTypes);
        }
#endif

        /// <summary>
        /// Marks a sequence point.
        /// </summary>
        public virtual void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn) {
            _ilg.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
        }

        /// <summary>
        /// Specifies the namespace to be used in evaluating locals and watches for the
        ///     current active lexical scope.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")] // TODO: fix
        public virtual void UsingNamespace(string usingNamespace) {
            _ilg.UsingNamespace(usingNamespace);
        }

        #endregion

        #region Simple Macros

        [Conditional("DEBUG")]
        internal void EmitDebugWriteLine(string message) {
            EmitString(message);
            EmitCall(typeof(Debug), "WriteLine", new Type[] { typeof(string) });
        }

        #endregion

        #region Instruction helpers

        public void EmitLoadArg(int index) {
            CodeContract.Requires(index >= 0, "index");

            switch (index) {
                case 0:
                    Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index <= Byte.MaxValue) {
                        Emit(OpCodes.Ldarg_S, (byte)index);
                    } else {
                        this.Emit(OpCodes.Ldarg, index);
                    }
                    break;
            }
        }

        public void EmitLoadArgAddress(int index) {
            CodeContract.Requires(index >= 0, "index");

            if (index <= Byte.MaxValue) {
                Emit(OpCodes.Ldarga_S, index);
            } else {
                this.Emit(OpCodes.Ldarga, index);
            }
        }

        /// <summary>
        /// Emits a Ldind* instruction for the appropriate type
        /// </summary>
        public void EmitLoadValueIndirect(Type type) {
            CodeContract.RequiresNotNull(type, "type");

            if (type.IsValueType) {
                if (type == typeof(int)) {
                    Emit(OpCodes.Ldind_I4);
                } else if (type == typeof(uint)) {
                    Emit(OpCodes.Ldind_U4);
                } else if (type == typeof(short)) {
                    Emit(OpCodes.Ldind_I2);
                } else if (type == typeof(ushort)) {
                    Emit(OpCodes.Ldind_U2);
                } else if (type == typeof(long) || type == typeof(ulong)) {
                    Emit(OpCodes.Ldind_I8);
                } else if (type == typeof(char)) {
                    Emit(OpCodes.Ldind_I2);
                } else if (type == typeof(bool)) {
                    Emit(OpCodes.Ldind_I1);
                } else if (type == typeof(float)) {
                    Emit(OpCodes.Ldind_R4);
                } else if (type == typeof(double)) {
                    Emit(OpCodes.Ldind_R8);
                } else {
                    Emit(OpCodes.Ldobj, type);
                }
            } else {
                Emit(OpCodes.Ldind_Ref);
            }
        }


        /// <summary>
        /// Emits a Stind* instruction for the appropriate type.
        /// </summary>
        public void EmitStoreValueIndirect(Type type) {
            CodeContract.RequiresNotNull(type, "type");

            if (type.IsValueType) {
                if (type == typeof(int)) {
                    Emit(OpCodes.Stind_I4);
                } else if (type == typeof(short)) {
                    Emit(OpCodes.Stind_I2);
                } else if (type == typeof(long) || type == typeof(ulong)) {
                    Emit(OpCodes.Stind_I8);
                } else if (type == typeof(char)) {
                    Emit(OpCodes.Stind_I2);
                } else if (type == typeof(bool)) {
                    Emit(OpCodes.Stind_I1);
                } else if (type == typeof(float)) {
                    Emit(OpCodes.Stind_R4);
                } else if (type == typeof(double)) {
                    Emit(OpCodes.Stind_R8);
                } else {
                    Emit(OpCodes.Stobj, type);
                }
            } else {
                Emit(OpCodes.Stind_Ref);
            }
        }

        /// <summary>
        /// Emits the Ldelem* instruction for the appropriate type
        /// </summary>
        /// <param name="type"></param>
        public void EmitLoadElement(Type type) {
            CodeContract.RequiresNotNull(type, "type");

            if (type.IsValueType) {
                if (type == typeof(System.SByte)) {
                    Emit(OpCodes.Ldelem_I1);
                } else if (type == typeof(System.Int16)) {
                    Emit(OpCodes.Ldelem_I2);
                } else if (type == typeof(System.Int32)) {
                    Emit(OpCodes.Ldelem_I4);
                } else if (type == typeof(System.Int64) || type == typeof(System.UInt64)) {
                    Emit(OpCodes.Ldelem_I8);
                } else if (type == typeof(System.Single)) {
                    Emit(OpCodes.Ldelem_R4);
                } else if (type == typeof(System.Double)) {
                    Emit(OpCodes.Ldelem_R8);
                } else if (type == typeof(System.Byte)) {
                    Emit(OpCodes.Ldelem_U1);
                } else if (type == typeof(System.UInt16)) {
                    Emit(OpCodes.Ldelem_U2);
                } else if (type == typeof(System.UInt32)) {
                    Emit(OpCodes.Ldelem_U4);
                } else {
                    Emit(OpCodes.Ldelem, type);
                }
            } else {
                Emit(OpCodes.Ldelem_Ref);
            }
        }

        /// <summary>
        /// Emits a Stelem* instruction for the appropriate type.
        /// </summary>
        public void EmitStoreElement(Type type) {
            CodeContract.RequiresNotNull(type, "type");

            if (type.IsValueType) {
                if (type == typeof(int) || type == typeof(uint)) {
                    Emit(OpCodes.Stelem_I4);
                } else if (type == typeof(short) || type == typeof(ushort)) {
                    Emit(OpCodes.Stelem_I2);
                } else if (type == typeof(long) || type == typeof(ulong)) {
                    Emit(OpCodes.Stelem_I8);
                } else if (type == typeof(char)) {
                    Emit(OpCodes.Stelem_I2);
                } else if (type == typeof(bool)) {
                    Emit(OpCodes.Stelem_I4);
                } else if (type == typeof(float)) {
                    Emit(OpCodes.Stelem_R4);
                } else if (type == typeof(double)) {
                    Emit(OpCodes.Stelem_R8);
                } else {
                    Emit(OpCodes.Stelem, type);
                }
            } else {
                Emit(OpCodes.Stelem_Ref);
            }
        }

        public void EmitType(Type type) {
            CodeContract.RequiresNotNull(type, "type");

            if (!(type is TypeBuilder) && !type.IsGenericParameter && !type.IsVisible) {
                throw new InvalidOperationException("Cannot emit type");
            }

            Emit(OpCodes.Ldtoken, type);
            EmitCall(typeof(Type), "GetTypeFromHandle");
        }

        public void EmitUnbox(Type type) {
            CodeContract.RequiresNotNull(type, "type");
            Emit(OpCodes.Unbox_Any, type);
        }

        #endregion

        #region Fields, properties and methods

        public void EmitPropertyGet(Type type, string name) {
            CodeContract.RequiresNotNull(type, "type");
            CodeContract.RequiresNotNull(name, "name");

            PropertyInfo pi = type.GetProperty(name);
            CodeContract.Requires(pi != null, "name", "Property doesn't exist on the provided type");

            EmitPropertyGet(pi);
        }

        public void EmitPropertyGet(PropertyInfo pi) {
            CodeContract.RequiresNotNull(pi, "pi");

            if (!pi.CanRead) {
                throw new InvalidOperationException(Resources.CantReadProperty);
            }

            EmitCall(pi.GetGetMethod());
        }

        public void EmitPropertySet(Type type, string name) {
            CodeContract.RequiresNotNull(type, "type");
            CodeContract.RequiresNotNull(name, "name");

            PropertyInfo pi = type.GetProperty(name);
            CodeContract.Requires(pi != null, "name", "Property doesn't exist on the provided type");

            EmitPropertySet(pi);
        }

        public void EmitPropertySet(PropertyInfo pi) {
            CodeContract.RequiresNotNull(pi, "pi");

            if (!pi.CanWrite) {
                throw new InvalidOperationException(Resources.CantWriteProperty);
            }

            EmitCall(pi.GetSetMethod());
        }

        public void EmitFieldAddress(FieldInfo fi) {
            CodeContract.RequiresNotNull(fi, "fi");

            if (fi.IsStatic) {
                Emit(OpCodes.Ldsflda, fi);
            } else {
                Emit(OpCodes.Ldflda, fi);
            }
        }

        public void EmitFieldGet(Type type, String name) {
            CodeContract.RequiresNotNull(type, "type");
            CodeContract.RequiresNotNull(name, "name");

            FieldInfo fi = type.GetField(name);
            CodeContract.Requires(fi != null, "name", "Field doesn't exist on provided type");
            EmitFieldGet(fi);
        }

        public void EmitFieldSet(Type type, String name) {
            CodeContract.RequiresNotNull(type, "type");
            CodeContract.RequiresNotNull(name, "name");

            FieldInfo fi = type.GetField(name);
            CodeContract.Requires(fi != null, "name", "Field doesn't exist on provided type");
            EmitFieldSet(fi);
        }

        public void EmitFieldGet(FieldInfo fi) {
            CodeContract.RequiresNotNull(fi, "fi");

            if (fi.IsStatic) {
                Emit(OpCodes.Ldsfld, fi);
            } else {
                Emit(OpCodes.Ldfld, fi);
            }
        }

        public void EmitFieldSet(FieldInfo fi) {
            CodeContract.RequiresNotNull(fi, "fi");

            if (fi.IsStatic) {
                Emit(OpCodes.Stsfld, fi);
            } else {
                Emit(OpCodes.Stfld, fi);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        public void EmitNew(ConstructorInfo ci) {
            CodeContract.RequiresNotNull(ci, "ci");

            if (ci.DeclaringType.ContainsGenericParameters) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.IllegalNew_GenericParams, ci.DeclaringType));
            }

            Emit(OpCodes.Newobj, ci);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        public void EmitNew(Type type, Type[] paramTypes) {
            CodeContract.RequiresNotNull(type, "type");
            CodeContract.RequiresNotNull(paramTypes, "paramTypes");

            ConstructorInfo ci = type.GetConstructor(paramTypes);
            CodeContract.Requires(ci != null, "type", "Type doesn't have constructor with a given signature");
            EmitNew(ci);
        }

        public void EmitCall(MethodInfo mi) {
            CodeContract.RequiresNotNull(mi, "mi");

            if (mi.IsVirtual && !mi.DeclaringType.IsValueType) {
                Emit(OpCodes.Callvirt, mi);
            } else {
                Emit(OpCodes.Call, mi);
            }
        }

        public void EmitCall(Type type, String name) {
            CodeContract.RequiresNotNull(type, "type");
            CodeContract.RequiresNotNull(name, "name");

            if (!type.IsVisible) {
                throw new ArgumentException(String.Format(Resources.TypeMustBeVisible, type.FullName));
            }

            MethodInfo mi = type.GetMethod(name);
            CodeContract.Requires(mi != null, "type", "Type doesn't have a method with a given name.");

            EmitCall(mi);
        }

        public void EmitCall(Type type, String name, Type[] paramTypes) {
            CodeContract.RequiresNotNull(type, "type");
            CodeContract.RequiresNotNull(name, "name");
            CodeContract.RequiresNotNull(paramTypes, "paramTypes");

            MethodInfo mi = type.GetMethod(name, paramTypes);
            CodeContract.Requires(mi != null, "type", "Type doesn't have a method with a given name and signature.");

            EmitCall(mi);
        }

        #endregion

        #region Constants

        public void EmitNull() {
            Emit(OpCodes.Ldnull);
        }

        public void EmitString(string value) {
            CodeContract.RequiresNotNull(value, "value");
            Emit(OpCodes.Ldstr, value);
        }

        public void EmitBoolean(bool value) {
            if (value) {
                Emit(OpCodes.Ldc_I4_1);
            } else {
                Emit(OpCodes.Ldc_I4_0);
            }
        }

        public void EmitChar(char value) {
            EmitInt(value);
            Emit(OpCodes.Conv_U2);
        }

        public void EmitByte(byte value) {
            EmitInt(value);
            Emit(OpCodes.Conv_U1);
        }

        [CLSCompliant(false)]
        public void EmitSByte(sbyte value) {
            EmitInt(value);
            Emit(OpCodes.Conv_I1);
        }

        public void EmitShort(short value) {
            EmitInt(value);
            Emit(OpCodes.Conv_I2);
        }

        [CLSCompliant(false)]
        public void EmitUShort(ushort value) {
            EmitInt(value);
            Emit(OpCodes.Conv_U2);
        }

        public void EmitInt(int value) {
            OpCode c;
            switch (value) {
                case -1:
                    c = OpCodes.Ldc_I4_M1;
                    break;
                case 0:
                    c = OpCodes.Ldc_I4_0;
                    break;
                case 1:
                    c = OpCodes.Ldc_I4_1;
                    break;
                case 2:
                    c = OpCodes.Ldc_I4_2;
                    break;
                case 3:
                    c = OpCodes.Ldc_I4_3;
                    break;
                case 4:
                    c = OpCodes.Ldc_I4_4;
                    break;
                case 5:
                    c = OpCodes.Ldc_I4_5;
                    break;
                case 6:
                    c = OpCodes.Ldc_I4_6;
                    break;
                case 7:
                    c = OpCodes.Ldc_I4_7;
                    break;
                case 8:
                    c = OpCodes.Ldc_I4_8;
                    break;
                default:
                    if (value >= -128 && value <= 127) {
                        Emit(OpCodes.Ldc_I4_S, (byte)value);
                    } else {
                        Emit(OpCodes.Ldc_I4, value);
                    }
                    return;
            }
            Emit(c);
        }

        [CLSCompliant(false)]
        public void EmitUInt(uint value) {
            EmitInt((int)value);
            Emit(OpCodes.Conv_U4);
        }

        public void EmitLong(long value) {
            Emit(OpCodes.Ldc_I8, value);
        }

        [CLSCompliant(false)]
        public void EmitULong(ulong value) {
            Emit(OpCodes.Ldc_I8, (long)value);
            Emit(OpCodes.Conv_U8);
        }

        public void EmitDouble(double value) {
            Emit(OpCodes.Ldc_R8, value);
        }

        public void EmitSingle(float value) {
            Emit(OpCodes.Ldc_R4, value);
        }

        private void EmitSimpleConstant(object value) {
            if (!TryEmitConstant(value)) {
                throw new ArgumentException(String.Format("Cannot emit constant {0} ({1})", value, value.GetType()));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public bool TryEmitConstant(object value) {
            string strVal;
            string[] sa;
            MethodInfo methodInfo;
            Type type;

            if (value == null) {
                EmitNull();
            } else if (value is int) {
                EmitInt((int)value);
            } else if (value is double) {
                EmitDouble((double)value);
            } else if (value is float) {
                EmitSingle((float)value);
            } else if (value is long) {
                EmitLong((long)value);
            } else if ((strVal = value as string) != null) {
                EmitString(strVal);
            } else if (value is bool) {
                EmitBoolean((bool)value);
            } else if ((sa = value as string[]) != null) {
                EmitArray(sa);
            } else if (value is Missing) {
                Emit(OpCodes.Ldsfld, typeof(Missing).GetField("Value"));
            } else if (value.GetType().IsEnum) {
                EmitEnum(value);
            } else if (value is uint) {
                EmitUInt((uint)value);
            } else if (value is char) {
                EmitChar((char)value);
            } else if (value is byte) {
                EmitByte((byte)value);
            } else if (value is sbyte) {
                EmitSByte((sbyte)value);
            } else if (value is short) {
                EmitShort((short)value);
            } else if (value is ushort) {
                EmitUShort((ushort)value);
            } else if (value is ulong) {
                EmitULong((ulong)value);
            } else if (value is decimal) {
                EmitDecimal((decimal)value);
            } else if ((type = value as Type) != null) {
                EmitType(type);
            } else if (value is RuntimeTypeHandle) {
                RuntimeTypeHandle rth = (RuntimeTypeHandle)value;
                if (!rth.Equals(default(RuntimeTypeHandle))) {
                    Emit(OpCodes.Ldtoken, Type.GetTypeFromHandle((RuntimeTypeHandle)value));
                } else {
                    return false; //EmitConstant(new RuntimeConstant(value));
                }
            } else if ((methodInfo = value as MethodInfo) != null) {
                Emit(OpCodes.Ldtoken, methodInfo);
                EmitCall(typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) }));
            } else if (value is RuntimeMethodHandle) {
                RuntimeMethodHandle rmh = (RuntimeMethodHandle)value;
                if (rmh != default(RuntimeMethodHandle)) {
                    Emit(OpCodes.Ldtoken, (MethodInfo)MethodBase.GetMethodFromHandle((RuntimeMethodHandle)value));
                } else {
                    return false; //EmitConstant(new RuntimeConstant(value));
                }
            } else {
                return false; // EmitConstant(new RuntimeConstant(value));
            }

            return true;
        }

        internal void EmitConstant(object value) {
            Debug.Assert(!(value is CompilerConstant));

            Type type;
            if (value is SymbolId) {
                EmitSymbolId((SymbolId)value);
            } else if ((type = value as Type) != null) {
                EmitType(type);
            } else {
                EmitSimpleConstant(value);
            }
        }

        #endregion

        #region Conversions

        public void EmitImplicitCast(Type from, Type to) {
            if (!TryEmitCast(from, to, true)) {
                throw new ArgumentException(String.Format("No implicit cast from {0} to {1}", from, to));
            }
        }

        public void EmitExplicitCast(Type from, Type to) {
            if (!TryEmitCast(from, to, false)) {
                throw new ArgumentException(String.Format("No explicit cast from {0} to {1}", from, to));
            }
        }

        public bool TryEmitImplicitCast(Type from, Type to) {
            return TryEmitCast(from, to, true);
        }

        public bool TryEmitExplicitCast(Type from, Type to) {
            return TryEmitCast(from, to, false);
        }

        private bool TryEmitCast(Type from, Type to, bool implicitOnly) {
            CodeContract.RequiresNotNull(from, "from");
            CodeContract.RequiresNotNull(to, "to");

            // No cast necessary if identical types
            if (from == to) {
                return true;
            }

            if (to.IsAssignableFrom(from)) {
                // T -> Nullable<T>
                if (TypeUtils.IsNullableType(to)) {
                    Type nonNullableTo = TypeUtils.GetNonNullableType(to);
                    if (TryEmitCast(from, nonNullableTo, true)) {
                        EmitNew(to.GetConstructor(new Type[] { nonNullableTo }));
                    } else {
                        return false;
                    }
                }

                if (from.IsValueType) {
                    if (to == typeof(object)) {
                        EmitBoxing(from);
                        return true;
                    }
                }

                if (to.IsInterface) {
                    Emit(OpCodes.Box, from);
                    return true;
                }

                if (from.IsEnum && to == typeof(Enum)) {
                    Emit(OpCodes.Box, from);
                    return true;
                }

                // They are assignable and reference types.
                return true;
            }

            if (to == typeof(void)) {
                Emit(OpCodes.Pop);
                return true;
            }

            if (to.IsValueType && from == typeof(object)) {
                if (implicitOnly) {
                    return false;
                }
                Emit(OpCodes.Unbox_Any, to);
                return true;
            }

            if (to.IsValueType != from.IsValueType) {
                return false;
            }

            if (!to.IsValueType) {
                if (implicitOnly) {
                    return false;
                }
                if (!to.IsVisible) {
                    throw new ArgumentException(String.Format(Resources.TypeMustBeVisible, to.FullName));
                }
                Emit(OpCodes.Castclass, to);
                return true;
            }

            if (to.IsEnum) {
                to = Enum.GetUnderlyingType(to);
            }
            if (from.IsEnum) {
                from = Enum.GetUnderlyingType(from);
            }

            if (to == from) {
                return true;
            }

            if (EmitNumericCast(from, to, implicitOnly)) {
                return true;
            }

            return false;
        }

        public bool EmitNumericCast(Type from, Type to, bool implicitOnly) {
            TypeCode fc = Type.GetTypeCode(from);
            TypeCode tc = Type.GetTypeCode(to);
            int fx, fy, tx, ty;

            if (!TypeUtils.GetNumericConversionOrder(fc, out fx, out fy) ||
                !TypeUtils.GetNumericConversionOrder(tc, out tx, out ty)) {
                // numeric <-> non-numeric
                return false;
            }

            bool isImplicit = TypeUtils.IsImplicitlyConvertible(fx, fy, tx, ty);

            if (implicitOnly && !isImplicit) {
                return false;
            }

            // IL conversion instruction also needed for floating point -> integer:
            if (!isImplicit || ty == 2 || tx == 2) {
                switch (tc) {
                    case TypeCode.SByte:
                        Emit(OpCodes.Conv_I1);
                        break;
                    case TypeCode.Int16:
                        Emit(OpCodes.Conv_I2);
                        break;
                    case TypeCode.Int32:
                        Emit(OpCodes.Conv_I4);
                        break;
                    case TypeCode.Int64:
                        Emit(OpCodes.Conv_I8);
                        break;
                    case TypeCode.Byte:
                        Emit(OpCodes.Conv_U1);
                        break;
                    case TypeCode.UInt16:
                        Emit(OpCodes.Conv_U1);
                        break;
                    case TypeCode.UInt32:
                        Emit(OpCodes.Conv_U2);
                        break;
                    case TypeCode.UInt64:
                        Emit(OpCodes.Conv_U4);
                        break;
                    case TypeCode.Single:
                        Emit(OpCodes.Conv_R4);
                        break;
                    case TypeCode.Double:
                        Emit(OpCodes.Conv_R8);
                        break;
                    default:
                        throw Assert.Unreachable;
                }
            }

            return true;
        }

        /// <summary>
        /// Boxes the value of the stack. No-op for reference types.
        /// </summary>
        public void EmitBoxing(Type type) {
            CodeContract.RequiresNotNull(type, "type");

            if (type.IsValueType) {
                if (type == typeof(void)) {
                    Emit(OpCodes.Ldnull);
                } else {
                    Emit(OpCodes.Box, type);
                }
            }
        }

        #endregion

        #region Arrays

        /// <summary>
        /// Emits an array of constant values provided in the given list.
        /// The array is strongly typed.
        /// </summary>
        public void EmitArray<T>(IList<T> items) {
            CodeContract.RequiresNotNull(items, "items");

            EmitInt(items.Count);
            Emit(OpCodes.Newarr, typeof(T));
            for (int i = 0; i < items.Count; i++) {
                Emit(OpCodes.Dup);
                EmitInt(i);
                EmitSimpleConstant(items[i]);
                EmitStoreElement(typeof(T));
            }
        }

        /// <summary>
        /// Emits an array of values of count size.  The items are emitted via the callback
        /// which is provided with the current item index to emit.
        /// </summary>
        public void EmitArray(Type elementType, int count, EmitArrayHelper emit) {
            CodeContract.RequiresNotNull(elementType, "elementType");
            CodeContract.RequiresNotNull(emit, "emit");
            CodeContract.Requires(count >= 0, "count", "Count must be non-negative.");

            EmitInt(count);
            Emit(OpCodes.Newarr, elementType);
            for (int i = 0; i < count; i++) {
                Emit(OpCodes.Dup);
                EmitInt(i);

                emit(i);

                EmitStoreElement(elementType);
            }
        }

        /// <summary>
        /// Emits an array construction code.  
        /// The code assumes that bounds for all dimensions
        /// are already emitted.
        /// </summary>
        public void EmitArray(Type arrayType) {
            CodeContract.RequiresNotNull(arrayType, "arrayType");
            CodeContract.Requires(arrayType.IsArray, "arrayType", "arryaType must be an array type");

            int rank = arrayType.GetArrayRank();
            if (rank == 1) {
                Emit(OpCodes.Newarr, arrayType.GetElementType());
            } else {
                Type[] types = new Type[rank];
                for (int i = 0; i < rank; i++) {
                    types[i] = typeof(int);
                }
                EmitNew(arrayType, types);
            }
        }

        #endregion

        #region Support for emitting constants

        public void EmitEnum(object value) {
            CodeContract.Requires(value != null, "value");
            CodeContract.Requires(value.GetType().IsEnum, "value");

            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32:
                    EmitInt((int)value);
                    break;
                case TypeCode.Int64:
                    EmitLong((long)value);
                    break;
                case TypeCode.Int16:
                    EmitShort((short)value);
                    break;
                case TypeCode.UInt32:
                    EmitUInt((uint)value);
                    break;
                case TypeCode.UInt64:
                    EmitULong((ulong)value);
                    break;
                case TypeCode.SByte:
                    EmitSByte((sbyte)value);
                    break;
                case TypeCode.UInt16:
                    EmitUShort((ushort)value);
                    break;
                case TypeCode.Byte:
                    EmitByte((byte)value);
                    break;
                default:
                    throw new NotImplementedException(String.Format(CultureInfo.CurrentCulture, Resources.NotImplemented_EnumEmit, value.GetType(), value));
            }
        }

        public void EmitDecimal(decimal value) {
            if (Decimal.Truncate(value) == value) {
                if (Int32.MinValue <= value && value <= Int32.MaxValue) {
                    int intValue = Decimal.ToInt32(value);
                    EmitInt(intValue);
                    EmitNew(typeof(Decimal).GetConstructor(new Type[] { typeof(int) }));
                } else if (Int64.MinValue <= value && value <= Int64.MaxValue) {
                    long longValue = Decimal.ToInt64(value);
                    EmitLong(longValue);
                    EmitNew(typeof(Decimal).GetConstructor(new Type[] { typeof(long) }));
                } else {
                    EmitDecimalBits(value);
                }
            } else {
                EmitDecimalBits(value);
            }
        }

        private void EmitDecimalBits(decimal value) {
            int[] bits = Decimal.GetBits(value);
            EmitInt(bits[0]);
            EmitInt(bits[1]);
            EmitInt(bits[2]);
            EmitBoolean((bits[3] & 0x80000000) != 0);
            EmitByte((byte)(bits[3] >> 16));
            EmitNew(typeof(decimal).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte) }));
        }

        public void EmitMissingValue(Type type) {
            LocalBuilder lb;

            switch (Type.GetTypeCode(type)) {
                default:
                case TypeCode.Object:
                    if (type == typeof(object)) {
                        // parameter of type object receives the actual Missing value
                        Emit(OpCodes.Ldsfld, typeof(Missing).GetField("Value"));
                    } else if (!type.IsValueType) {
                        // reference type
                        EmitNull();
                    } else if (type.IsSealed && !type.IsEnum) {
                        lb = DeclareLocal(type);
                        Emit(OpCodes.Ldloca, lb);
                        Emit(OpCodes.Initobj, type);
                        Emit(OpCodes.Ldloc, lb);
                    } else {
                        throw new ArgumentException("No default value for a given type");
                    }
                    break;

                case TypeCode.Empty:
                case TypeCode.DBNull:
                    EmitNull();
                    break;

                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    EmitInt(0);
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    EmitLong(0);
                    break;

                case TypeCode.Single:
                    EmitSingle(default(Single));
                    break;

                case TypeCode.Double:
                    Emit(OpCodes.Ldc_R8, default(Double));
                    break;

                case TypeCode.Decimal:
                    EmitFieldGet(typeof(Decimal).GetField("Zero"));
                    break;

                case TypeCode.DateTime:
                    lb = DeclareLocal(typeof(DateTime));
                    Emit(OpCodes.Ldloca, lb);
                    Emit(OpCodes.Initobj, typeof(DateTime));
                    Emit(OpCodes.Ldloc, lb);
                    break;

                case TypeCode.String:
                    EmitNull();
                    break;
            }
        }

        internal void EmitUninitialized() {
            EmitFieldGet(typeof(Microsoft.Scripting.Runtime.Uninitialized), "Instance");
        }

        #endregion

        #region Tuples

        // TOOD: Should the tuple support be here?

        public void EmitTuple(Type tupleType, int count, EmitArrayHelper emit) {
            EmitTuple(tupleType, 0, count, emit);
        }

        private void EmitTuple(Type tupleType, int start, int end, EmitArrayHelper emit) {
            int size = end - start;

            if (size > Tuple.MaxSize) {
                int multiplier = 1;
                while (size > Tuple.MaxSize) {
                    size = (size + Tuple.MaxSize - 1) / Tuple.MaxSize;
                    multiplier *= Tuple.MaxSize;
                }
                for (int i = 0; i < size; i++) {
                    int newStart = start + (i * multiplier);
                    int newEnd = System.Math.Min(end, start + ((i + 1) * multiplier));

                    PropertyInfo pi = tupleType.GetProperty("Item" + String.Format("{0:D3}", i));
                    Debug.Assert(pi != null);
                    EmitTuple(pi.PropertyType, newStart, newEnd, emit);
                }
            } else {
                for (int i = start; i < end; i++) {
                    emit(i);
                }
            }

            // fill in emptys with null.
            Type[] genArgs = tupleType.GetGenericArguments();
            for (int i = size; i < genArgs.Length; i++) {
                EmitNull();
            }

            EmitTupleNew(tupleType);
        }

        private void EmitTupleNew(Type tupleType) {
            ConstructorInfo[] cis = tupleType.GetConstructors();
            foreach (ConstructorInfo ci in cis) {
                if (ci.GetParameters().Length != 0) {
                    EmitNew(ci);
                    break;
                }
            }
        }
        #endregion

        #region LocalTemps

        internal Slot GetLocalTmp(Type type) {
            CodeContract.RequiresNotNull(type, "type");

            for (int i = 0; i < _freeSlots.Count; i++) {
                Slot slot = _freeSlots[i];
                if (slot.Type == type) {
                    _freeSlots.RemoveAt(i);
                    return slot;
                }
            }

            return new LocalSlot(DeclareLocal(type), this);
        }

        internal void FreeLocalTmp(Slot slot) {
            if (slot != null) {
                Debug.Assert(!_freeSlots.Contains(slot));
                _freeSlots.Add(slot);
            }
        }

        #endregion

        #region SynbolId

        /// <summary>
        /// Emits a symbol id.  
        /// </summary>
        internal void EmitSymbolId(SymbolId id) {
            if (_tg == null) {
                EmitInt(id.Id);
                EmitNew(typeof(SymbolId), new Type[] { typeof(int) });
            } else {
                _tg.EmitIndirectedSymbol(this, id);
            }
        }

        #endregion

    }
}
