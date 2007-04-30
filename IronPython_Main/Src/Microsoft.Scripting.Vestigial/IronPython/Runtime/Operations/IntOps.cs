/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Threading;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Compiler;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Internal;

namespace IronPython.Runtime.Operations {

    public class ExtensibleInt : Extensible<int> {
        public ExtensibleInt() : base() { }
        public ExtensibleInt(int v) : base(v) { }

        [PythonName("__cmp__")]
        [return: MaybeNotImplemented]
        public virtual object Compare(CodeContext context, object obj) {
            return Int32Ops.Compare(context, Value, obj);
        }
    }


    public static partial class Int32Ops {
        private static object FastNew(object o) {
            Extensible<BigInteger> el;

            if (o is string) return Make(null, (string)o, 10);
            if (o is double) return DoubleOps.ToInteger((double)o);
            if (o is int) return o;
            if (o is BigInteger) return o;
            if ((el = o as Extensible<BigInteger>) != null) return el.Value;
            if (o is float) return DoubleOps.ToInteger((double)(float)o);

            if (o is Complex64) throw Ops.TypeError("can't convert complex to int; use int(abs(z))");

            if (o is Byte) return (Int32)(Byte)o;
            if (o is SByte) return (Int32)(SByte)o;
            if (o is Int16) return (Int32)(Int16)o;
            if (o is Int64) {
                Int64 val = (Int64)o;
                if (Int32.MinValue <= val && val <= Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            }
            if (o is UInt16) return (Int32)(UInt16)o;

            if (o is UInt32) {
                UInt32 val = (UInt32)o;
                if (val <= Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            }
            if (o is UInt64) {
                UInt64 val = (UInt64)o;
                if (val <= Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            }

            if (o is Decimal) {
                Decimal val = (Decimal)o;
                if (Int32.MinValue <= val && val <= Int32.MaxValue) {
                    return (Int32)val;
                } else {
                    return BigInteger.Create(val);
                }
            }

            if (o is Enum) {
                return Converter.CastEnumToInt32(o);
            }

            object newValue;
            if(Ops.TryInvokeOperator(DefaultContext.Default,
                Operators.ConvertToInt32,
                o,
                out newValue)) {
                // Convert resulting object to the desired type
                if (newValue is int) return newValue;
                if (newValue is BigInteger) return newValue;
                if (newValue is Extensible<BigInteger>)return ((Extensible<BigInteger>)newValue).Value;
                if (newValue is Extensible<int>) return ((Extensible<int>)newValue).Value;

                throw Ops.TypeError("__int__ returned non-int");
            }

            return Converter.ConvertToInt32(o);
        }

        public static object Make(CodeContext context, object o) {
            return Make(context, TypeCache.Int32, o);
        }

        private static void ValidateType(DynamicType cls) {
            if (cls == TypeCache.Boolean)
                throw Ops.TypeError("int.__new__(bool) is not safe, use bool.__new__()");
        }

        [StaticOpsMethod("__new__")]
        public static object Make(DynamicType cls, string s, int radix) {
            ValidateType(cls);

            //try {
                return LiteralParser.ParseIntegerSign(s, radix);
            /*} catch (ArgumentException e) {
                throw Runtime.Exceptions.ExceptionConverter.UpdateForRethrow(Ops.ValueError(e.Message));
            }*/
        }

        [StaticOpsMethod("__new__")]
        public static object Make(CodeContext context, DynamicType cls, object x) {
            if (cls == TypeCache.Int32)  return FastNew(x); // TODO: Call site?

            ValidateType(cls);

            // derived int creation...
            return cls.CreateInstance(context, x);
        }

        // "int()" calls ReflectedType.Call(), which calls "Activator.CreateInstance" and return directly.
        // this is for derived int creation or direct calls to __new__...
        [StaticOpsMethod("__new__")]
        public static object Make(CodeContext context, DynamicType cls) {
            if (cls == TypeCache.Int32) return 0;

            return cls.CreateInstance(context);
        }

        #region Binary Operators
        [OperatorMethod]
        public static object FloorDivide(int x, int y) {
            if (y == -1 && x == Int32.MinValue) {
                return -BigInteger.Create(Int32.MinValue);
            }
            return RuntimeHelpers.Int32ToObject(FloorDivideImpl(x, y));
        }

        public static int FloorDivideImpl(int x, int y) {
            int q = x / y;

            if (x >= 0) {
                if (y > 0) return q;
                else if (x % y == 0) return q;
                else return q - 1;
            } else {
                if (y > 0) {
                    if (x % y == 0) return q;
                    else return q - 1;
                } else return q;
            }
        }

        [OperatorMethod]
        public static int Mod(int x, int y) {
            if (y == -1) return 0;
            int r = x % y;

            if (x >= 0) {
                if (y > 0) return r;
                else if (r == 0) return 0;
                else return r + y;
            } else {
                if (y > 0) {
                    if (r == 0) return r;
                    else return r + y;
                } else return r;
            }
        }

        [OperatorMethod]
        public static object Power(int x, BigInteger power, BigInteger qmod) {
            return BigIntegerOps.Power((BigInteger)x, power, qmod);
        }


        [OperatorMethod]
        public static object Power(int x, int power, int? qmod) {
            if (qmod == null) return Power(x, power);
            int mod = (int)qmod;

            if (power < 0) throw Ops.TypeError("power", power, "power must be >= 0");

            if (mod == 0) {
                throw Ops.ZeroDivisionError();
            }

            long result = 1;
            if (power > 0) {
                long factor = x;
                while (power != 0) {
                    if ((power & 1) != 0) result = result * factor % mod;
                    factor = factor * factor % mod; //???
                    power >>= 1;
                }
            }

            if (result >= 0) {
                if (mod < 0) return (int)(result + mod);
            } else {
                if (mod > 0) return (int)(result + mod);
            }
            return (int)result;
        }

        [OperatorMethod]
        public static object Power(int x, int power) {
            if (power == 0) return 1;
            if (power < 0) {
                if (x == 0)
                    throw Ops.ZeroDivisionError("0.0 cannot be raised to a negative power");
                return DoubleOps.Power(x, power);
            }
            int factor = x;
            int result = 1;
            int savePower = power;
            try {
                checked {
                    while (power != 0) {  //??? this loop has redundant checks for exit condition
                        if ((power & 1) != 0) result = result * factor;
                        if (power == 1) break;
                        factor = factor * factor;
                        power >>= 1;
                    }
                    return result;
                }
            } catch (OverflowException) {
                return BigIntegerOps.Power(BigInteger.Create(x), savePower);
            }
        }


        [OperatorMethod]
        public static object LeftShift(int x, int y) {
            if (y < 0) {
                throw Ops.ValueError("negative shift count");
            }
            if (y > 31 ||
                (x > 0 && x > (Int32.MaxValue >> y)) ||
                (x < 0 && x < (Int32.MinValue >> y))) {
                return Int64Ops.LeftShift((long)x, y);
            }
            return RuntimeHelpers.Int32ToObject(x << y);
        }

        [OperatorMethod]
        public static int RightShift(int x, int y) {
            if (y < 0) {
                throw Ops.ValueError("negative shift count");
            }
            if (y > 31) {
                return x >= 0 ? 0 : -1;
            }

            int q;

            if (x >= 0) {
                q = x >> y;
            } else {
                q = (x + ((1 << y) - 1)) >> y;
                int r = x - (q << y);
                if (r != 0) q--;
            }

            return q;
        }

        #endregion

        [PythonName("__divmod__")]
        public static object DivMod(int x, int y) {
            return Tuple.MakeTuple(Divide(x, y), Mod(x, y));
        }

        [PythonName("__divmod__")]
        public static object DivMod(int x, object y) {
            return Ops.NotImplemented;
        }


        #region Unary Operators
        [PythonName("__oct__")]
        public static string Oct(int x) {
            if (x == 0) {
                return "0";
            } else if (x > 0) {
                return "0" + BigInteger.Create(x).ToString(8);
            } else {
                return "-0" + BigInteger.Create(-x).ToString(8);
            }
        }

        [PythonName("__hex__")]
        public static string Hex(int x) {
            if (x < 0) {
                return "-0x" + (-x).ToString("x");
            } else {
                return "0x" + x.ToString("x");
            }
        }

        #endregion

        [PythonName("__getnewargs__")]
        public static object GetNewArgs(CodeContext context, int self) {
            return Tuple.MakeTuple(Int32Ops.Make(context, TypeCache.Int32, self));
        }

        private static object Compare(int x, int y) {
            return RuntimeHelpers.Int32ToObject(x == y ? 0 : x < y ? -1 : +1);
        }

        [PythonName("__cmp__")]
        [return: MaybeNotImplemented]
        public static object Compare(CodeContext context, int self, object obj) {
            if (obj == null) return RuntimeHelpers.Int32ToObject(1);

            int otherInt;

            if (obj is int) {
                otherInt = (int)obj;
            } else if (obj is ExtensibleInt) {
                otherInt = ((ExtensibleInt)obj).Value;
            } else if (obj is bool) {
                otherInt = ((bool)obj) ? 1 : 0;
            } else if (obj is double) {
                // compare as double to avoid truncation issues
                return DoubleOps.Compare(context, (double)self, (double)obj);
            } else if (obj is Extensible<double>) {
                // compare as double to avoid truncation issues
                return DoubleOps.Compare(context, (double)self, ((Extensible<double>)obj).Value);
            } else if (obj is Decimal) {
                return DoubleOps.Compare(context, (double)self, (double)(decimal)obj);
            } else {
                if (!Converter.TryConvertToInt32(obj, out otherInt)) {
                    object res;
                    if(Ops.GetDynamicType(obj).TryInvokeBinaryOperator(context,
                        Operators.Coerce,
                        obj,
                        self, 
                        out res)) {
                        if (res != Ops.NotImplemented && !(res is OldInstance)) {
                            return Ops.Compare(context, ((Tuple)res)[1], ((Tuple)res)[0]);
                        }
                    }
                    return Ops.NotImplemented;
                }
            }

            return Compare(self, otherInt);
        }

        internal static object ReverseDivMod(int x, int y) {
            return DivMod(y, x);
        }


    }
}
