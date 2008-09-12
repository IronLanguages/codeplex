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
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

namespace IronPython.Runtime.Operations {
    public static class EnumOps {        
        [SpecialName]
        public static object BitwiseOr(object self, object other) {
            if (self is Enum && other is Enum) {
                Type selfType = self.GetType();
                Type otherType = other.GetType();

                if (selfType == otherType) {
                    Type underType = Enum.GetUnderlyingType(selfType);
                    if (underType == typeof(int)) {
                        return Enum.ToObject(selfType, (int)self | (int)other);
                    } else if (underType == typeof(long)) {
                        return Enum.ToObject(selfType, (long)self | (long)other);
                    } else if (underType == typeof(short)) {
                        return Enum.ToObject(selfType, (short)self | (short)other);
                    } else if (underType == typeof(byte)) {
                        return Enum.ToObject(selfType, (byte)self | (byte)other);
                    } else if (underType == typeof(sbyte)) {
                        return Enum.ToObject(selfType, (sbyte)self | (sbyte)other);
                    } else if (underType == typeof(uint)) {
                        return Enum.ToObject(selfType, (uint)self | (uint)other);
                    } else if (underType == typeof(ulong)) {
                        return Enum.ToObject(selfType, (ulong)self | (ulong)other);
                    } else if (underType == typeof(ushort)) {
                        return Enum.ToObject(selfType, (ushort)self | (ushort)other);
                    }
                }
            }

            throw PythonOps.ValueError("bitwise or cannot be applied to {0} and {1}", self.GetType(), other.GetType());
        }

        [SpecialName]
        public static object BitwiseAnd(object self, object other) {
            if (self is Enum && other is Enum) {
                Type selfType = self.GetType();
                Type otherType = other.GetType();

                if (selfType == otherType) {
                    Type underType = Enum.GetUnderlyingType(selfType);
                    if (underType == typeof(int)) {
                        return Enum.ToObject(selfType, (int)self & (int)other);
                    } else if (underType == typeof(long)) {
                        return Enum.ToObject(selfType, (long)self & (long)other);
                    } else if (underType == typeof(short)) {
                        return Enum.ToObject(selfType, (short)self & (short)other);
                    } else if (underType == typeof(byte)) {
                        return Enum.ToObject(selfType, (byte)self & (byte)other);
                    } else if (underType == typeof(sbyte)) {
                        return Enum.ToObject(selfType, (sbyte)self & (sbyte)other);
                    } else if (underType == typeof(uint)) {
                        return Enum.ToObject(selfType, (uint)self & (uint)other);
                    } else if (underType == typeof(ulong)) {
                        return Enum.ToObject(selfType, (ulong)self & (ulong)other);
                    } else if (underType == typeof(ushort)) {
                        return Enum.ToObject(selfType, (ushort)self & (ushort)other);
                    }
                }
            }

            throw PythonOps.ValueError("bitwise and cannot be applied to {0} and {1}", self.GetType(), other.GetType());
        }

        [SpecialName]
        public static object ExclusiveOr(object self, object other) {
            if (self is Enum && other is Enum) {
                Type selfType = self.GetType();
                Type otherType = other.GetType();

                if (selfType == otherType) {
                    Type underType = Enum.GetUnderlyingType(selfType);
                    if (underType == typeof(int)) {
                        return Enum.ToObject(selfType, (int)self ^ (int)other);
                    } else if (underType == typeof(long)) {
                        return Enum.ToObject(selfType, (long)self ^ (long)other);
                    } else if (underType == typeof(short)) {
                        return Enum.ToObject(selfType, (short)self ^ (short)other);
                    } else if (underType == typeof(byte)) {
                        return Enum.ToObject(selfType, (byte)self ^ (byte)other);
                    } else if (underType == typeof(sbyte)) {
                        return Enum.ToObject(selfType, (sbyte)self ^ (sbyte)other);
                    } else if (underType == typeof(uint)) {
                        return Enum.ToObject(selfType, (uint)self ^ (uint)other);
                    } else if (underType == typeof(ulong)) {
                        return Enum.ToObject(selfType, (ulong)self ^ (ulong)other);
                    } else if (underType == typeof(ushort)) {
                        return Enum.ToObject(selfType, (ushort)self ^ (ushort)other);
                    }
                }
            }

            throw PythonOps.ValueError("bitwise xor cannot be applied to {0} and {1}", self.GetType(), other.GetType());
        }

        [SpecialName]
        public static object OnesComplement(object self) {
            if (self is Enum) {
                Type selfType = self.GetType();
                Type underType = Enum.GetUnderlyingType(selfType);
                if (underType == typeof(int)) {
                    return Enum.ToObject(selfType, ~(int)self);
                } else if (underType == typeof(long)) {
                    return Enum.ToObject(selfType, ~(long)self);
                } else if (underType == typeof(short)) {
                    return Enum.ToObject(selfType, ~(short)self);
                } else if (underType == typeof(byte)) {
                    return Enum.ToObject(selfType, ~(byte)self);
                } else if (underType == typeof(sbyte)) {
                    return Enum.ToObject(selfType, ~(sbyte)self);
                } else if (underType == typeof(uint)) {
                    return Enum.ToObject(selfType, ~(uint)self);
                } else if (underType == typeof(ulong)) {
                    return Enum.ToObject(selfType, ~(ulong)self);
                } else if (underType == typeof(ushort)) {
                    return Enum.ToObject(selfType, ~(ushort)self);
                }
            }

            throw PythonOps.ValueError("one's complement cannot be applied to {0}", self.GetType());
        }

        public static bool __nonzero__(object self) {
            if (self is Enum) {
                Type selfType = self.GetType();
                Type underType = Enum.GetUnderlyingType(selfType);

                switch(Type.GetTypeCode(underType)) {
                    case TypeCode.Int16: return (short)self != 0;
                    case TypeCode.Int32: return (int)self != 0;
                    case TypeCode.Int64: return (long)self != 0;
                    case TypeCode.UInt16: return (ushort)self != 0;
                    case TypeCode.UInt32: return (uint)self != 0;
                    case TypeCode.UInt64: return ~(ulong)self != 0;
                    case TypeCode.Byte: return (byte)self != 0;
                    case TypeCode.SByte: return (sbyte)self != 0;
                }
            }

            throw PythonOps.ValueError("__nonzero__ cannot be applied to {0}", self.GetType());
        }
    }
}
