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
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

#if !SILVERLIGHT

namespace IronPython.Modules {
    /// <summary>
    /// Provides support for interop with native code from Python code.
    /// </summary>
    public static partial class CTypes {
        /// <summary>
        /// The meta class for ctypes unions.
        /// </summary>
        [PythonType, PythonHidden]
        public class UnionType : PythonType, INativeType {
            internal Field[] _fields;
            private int _size, _alignment;

            public UnionType(CodeContext/*!*/ context, string name, PythonTuple bases, IAttributesCollection members)
                : base(context, name, bases, members) {

                object fields;
                if (members.TryGetValue(SymbolTable.StringToId("_fields_"), out fields)) {
                    SetFields(fields);
                }
            }

            public new void __setattr__(CodeContext/*!*/ context, string name, object value) {
                if (name == "_fields_") {
                    lock (this) {
                        if (_fields != null) {
                            throw PythonOps.AttributeError("_fields_ is final");
                        }

                        SetFields(value);
                    }
                }

                base.__setattr__(context, name, value);
            }

            private UnionType(Type underlyingSystemType)
                : base(underlyingSystemType) {
            }

            /// <summary>
            /// Converts an object into a function call parameter.
            /// </summary>
            public object from_param(object obj) {
                return null;
            }

            internal static PythonType MakeSystemType(Type underlyingSystemType) {
                return PythonType.SetPythonType(underlyingSystemType, new UnionType(underlyingSystemType));
            }

            public static ArrayType/*!*/ operator *(UnionType type, int count) {
                return MakeArrayType(type, count);
            }

            public static ArrayType/*!*/ operator *(int count, UnionType type) {
                return MakeArrayType(type, count);
            }

            #region INativeType Members

            int INativeType.Size {
                get {
                    return _size;
                }
            }

            int INativeType.Alignment {
                get {
                    return _alignment;
                }
            }

            object INativeType.GetValue(MemoryHolder owner, object readingFrom, int offset, bool raw) {
                throw new NotImplementedException("union get value");
            }

            object INativeType.SetValue(MemoryHolder address, int offset, object value) {
                throw new NotImplementedException("union set value");
            }

            Type INativeType.GetNativeType() {
                return GetMarshalTypeFromSize(_size);
            }

            MarshalCleanup INativeType.EmitMarshalling(ILGenerator/*!*/ method, LocalOrArg argIndex, List<object>/*!*/ constantPool, int constantPoolArgument) {
                Type argumentType = argIndex.Type;
                argIndex.Emit(method);
                if (argumentType.IsValueType) {
                    method.Emit(OpCodes.Box, argumentType);
                }
                constantPool.Add(this);
                method.Emit(OpCodes.Ldarg, constantPoolArgument);
                method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
                method.Emit(OpCodes.Ldelem_Ref);
                method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CheckCDataType"));
                method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
                method.Emit(OpCodes.Ldobj, ((INativeType)this).GetNativeType());
                return null;
            }

            Type/*!*/ INativeType.GetPythonType() {
                return typeof(object);
            }

            void INativeType.EmitReverseMarshalling(ILGenerator method, LocalOrArg value, List<object> constantPool, int constantPoolArgument) {
                value.Emit(method);
                EmitCDataCreation(this, method, constantPool, constantPoolArgument);
            }

            #endregion

            private void SetFields(object fields) {
                lock (this) {
                    IList<object> list = GetFieldsList(fields);
                    IList<object> anonFields = StructType.GetAnonymousFields(this);

                    int size = 0, alignment = 1;
                    List<Field> allFields = new List<Field>();//GetBaseSizeAlignmentAndFields(out size, out alignment);
                    int? bitCount;
                    for (int fieldIndex = 0; fieldIndex < list.Count; fieldIndex++) {
                        object o = list[fieldIndex];
                        string fieldName;
                        INativeType cdata;
                        GetFieldInfo(this, o, out fieldName, out cdata, out bitCount);
                        alignment = Math.Max(alignment, cdata.Alignment);
                        size = Math.Max(size, cdata.Size);

                        Field newField = new Field(fieldName, cdata, 0, allFields.Count);
                        allFields.Add(newField);
                        AddSlot(SymbolTable.StringToId(fieldName), newField);

                        if (anonFields != null && anonFields.Contains(fieldName)) {
                            StructType.AddAnonymousFields(this, allFields, cdata, newField);
                        }
                    }

                    StructType.CheckAnonymousFields(allFields, anonFields);
                    
                    _fields = allFields.ToArray();
                    _size = PythonStruct.Align(size, alignment);
                    _alignment = alignment;
                }
            }

            internal void EnsureFinal() {
                if (_fields == null) {
                    SetFields(PythonTuple.EMPTY);
                }
            }
        }
    }
}

#endif
