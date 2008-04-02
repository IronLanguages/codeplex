/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace IronPython.Runtime.Operations {
    public static class EnumOps {

        public static object Equal(object self, object other) {
            if (self is Enum) {
                if (other is Enum) {
                    Type selfType = self.GetType();
                    Type otherType = other.GetType();

                    if (selfType == otherType) {
                        Type underType = Enum.GetUnderlyingType(selfType);
                        if (underType == typeof(int)) {
                            return (int)self == (int)other;
                        } else if (underType == typeof(long)) {
                            return (long)self == (long)other;
                        } else if (underType == typeof(short)) {
                            return (short)self == (short)other;
                        } else if (underType == typeof(byte)) {
                            return (byte)self == (byte)other;
                        } else if (underType == typeof(sbyte)) {
                            return (sbyte)self == (sbyte)other;
                        } else if (underType == typeof(uint)) {
                            return (uint)self == (uint)other;
                        } else if (underType == typeof(ulong)) {
                            return (ulong)self == (ulong)other;
                        } else if (underType == typeof(ushort)) {
                            return (ushort)self == (ushort)other;
                        }
                    }
                } else if (other == null) return false;
            }

            throw Ops.ValueError("Equal cannot be applied to {0} and {1}", self.GetType(), other.GetType());
        }

        public static bool EqualRetBool(object self, object other) {
            if (self is Enum) {
                if (other is Enum) {
                    Type selfType = self.GetType();
                    Type otherType = other.GetType();

                    if (selfType == otherType) {
                        Type underType = Enum.GetUnderlyingType(selfType);
                        if (underType == typeof(int)) {
                            return (int)self == (int)other;
                        } else if (underType == typeof(long)) {
                            return (long)self == (long)other;
                        } else if (underType == typeof(short)) {
                            return (short)self == (short)other;
                        } else if (underType == typeof(byte)) {
                            return (byte)self == (byte)other;
                        } else if (underType == typeof(sbyte)) {
                            return (sbyte)self == (sbyte)other;
                        } else if (underType == typeof(uint)) {
                            return (uint)self == (uint)other;
                        } else if (underType == typeof(ulong)) {
                            return (ulong)self == (ulong)other;
                        } else if (underType == typeof(ushort)) {
                            return (ushort)self == (ushort)other;
                        }
                    }
                } else if (other == null) return false;
            }

            throw Ops.ValueError("Equal cannot be applied to {0} and {1}", self.GetType(), other.GetType());
        }

        public static object NotEqual(object self, object other) {
            return Ops.Not(Equal(self, other));
        }

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

            throw Ops.ValueError("bitwise or cannot be applied to {0} and {1}", self.GetType(), other.GetType());
        }

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

            throw Ops.ValueError("bitwise and cannot be applied to {0} and {1}", self.GetType(), other.GetType());
        }

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

            throw Ops.ValueError("bitwise xor cannot be applied to {0} and {1}", self.GetType(), other.GetType());
        }

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

            throw Ops.ValueError("one's complement cannot be applied to {0}", self.GetType());
        }
    }
}
