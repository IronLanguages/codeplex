/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Collections;
using System.Threading;

using IronMath;

namespace IronPython.Runtime {

    public static class BoolOps {
        internal static ReflectedType BoolType;

        public static ReflectedType MakeDynamicType(ReflectedType type) {
            if (BoolType == null) {
                OpsReflectedType ret = new OpsReflectedType("bool", typeof(bool), typeof(BoolOps), null, new CallTarget1(FastNew)); 
                if (Interlocked.CompareExchange<ReflectedType>(ref BoolType, ret, null) == null)
                    return ret;
            }

            return BoolType;
        }

        private static object FastNew(object value) {
            if(value is bool) return Ops.Bool2Object((bool)value);
            return Ops.Bool2Object(Ops.IsTrue(value));
        }

        [PythonName("__add__")]
        public static object Add(bool x, object other)
        {
            if (other is bool) {
                return (x ? 1 : 0) + ((bool)other ? 1 : 0);
            } else {
                return IntOps.Add(x ? 1 : 0, other);
            }
        }

        [PythonName("__sub__")]
        public static object Subtract(bool x, object other) {
            if (other is bool) {
                return (x ? 1 : 0) - ((bool)other ? 1 : 0);
            } else {
                return IntOps.Subtract(x ? 1 : 0, other);
            }
        }

        [PythonName("__pow__")]
        public static object Power(bool x, object other) {
            if (other is bool) {
                return IntOps.Power(x ? 1 : 0, (bool)other ? 1 : 0);
            } else {
                return IntOps.Power(x ? 1 : 0, other);
            }
        }

        [PythonName("__mul__")]
        public static object Multiply(bool x, object other) {
            if (other is bool) {
                return (x ? 1 : 0) * ((bool)other ? 1 : 0);
            } else {
                return IntOps.Multiply(x ? 1 : 0, other);
            }
        }

        [PythonName("__flordiv__")]
        public static object FloorDivide(bool x, object other) {
            if (other is bool) {
                return IntOps.FloorDivide(x ? 1 : 0, (bool)other ? 1 : 0);
            } else {
                return IntOps.FloorDivide(x ? 1 : 0, other);
            }
        }

        [PythonName("__divmod__")]
        public static object Divide(bool x, object other) {
            if (other is bool) {
                return IntOps.Divide(x ? 1 : 0, (bool)other ? 1 : 0);
            } else {
                return IntOps.Divide(x ? 1 : 0, other);
            }
        }

        [PythonName("__div__")]
        public static object TrueDivide(bool x, object other) {
            return IntOps.TrueDivide(x ? 1 : 0, other);
        }

        [PythonName("__mod__")]
        public static object Mod(bool x, object other) {
            if (other is bool) {
                return IntOps.Mod(x ? 1 : 0, (bool)other ? 1 : 0);
            } else {
                return IntOps.Mod(x ? 1 : 0, other);
            }
        }

        [PythonName("__lshift__")]
        public static object LeftShift(bool x, object other) {
            if (other is bool) {
                return IntOps.LeftShift(x ? 1 : 0, (bool)other ? 1 : 0);
            } else {
                return IntOps.LeftShift(x ? 1 : 0, other);
            }

        }

        [PythonName("__rshift__")]
        public static object RightShift(bool x, object other) {
            if (other is bool) {
                return IntOps.RightShift(x ? 1 : 0, (bool)other ? 1 : 0);
            } else {
                return IntOps.RightShift(x ? 1 : 0, other);
            }
        }

        [PythonName("__xor__")]
        public static object Xor(bool x, object other) {
            if (other is bool) {
                return Ops.Bool2Object(x ^ (bool)other);
            }

            return (IntOps.Xor(x ? 1 : 0, other));
        }
       
        [PythonName("__and__")]
        public static object BitwiseAnd(bool x, object other) {
            if (other is bool) {
                return Ops.Bool2Object(x & (bool)other);
            }
            return IntOps.BitwiseAnd(x ? 1 : 0, other);
        }

        [PythonName("__or__")]
        public static object BitwiseOr(bool x, object other) {
            if (other is bool) {
                return Ops.Bool2Object(x | (bool)other);
            }

            return IntOps.BitwiseOr(x ? 1 : 0, other);
        }

        [PythonName("__new__")]
        public static object Make(object cls, object o) {
            return Ops.Bool2Object(Ops.IsTrue(o));
        }

        [PythonName("__cmp__")]
        public static object Compare(bool x, object y) {
            if (y == null) return 1;

            int res;
            if (y is bool) res = ((x) ? 1 : 0) - (((bool)y) ? 1 : 0);
            else {
                Conversion conv;
                int iVal = Converter.TryConvertToInt32(y, out conv);
                if (conv == Conversion.None) return Ops.NotImplemented;
                res = ((x) ? 1 : 0) - Converter.ConvertToInt32(y);
            }

            return res >= 1 ? 1 : res <= -1 ? -1 : 0;
        }

        public static object Equals(bool x, object other) {
            // common case is bool vs bool
            if (other is bool)  return Ops.Bool2Object(x == (bool)other);

            // otherwise convert other to a bool, and compare
            Conversion conv;
            int otherInt = Converter.TryConvertToInt32(other, out conv);
            if (conv < Conversion.Truncation) {
                int myint = x ? 1 : 0;
                return Ops.Bool2Object(myint == otherInt);
            } else if (conv == Conversion.Truncation) {
                // if we truncated the value (eg 1.5) we wouldn't be equal
                // to True, but if we had something like 1.0 we didn't lose
                // any precision and we're equal
                if (EqualsTruncation(x, other)) return true;
            }
            return Ops.NotImplemented;
        }

        public static bool EqualsRetBool(bool x, object other) {
            // common case is bool vs bool
            if (other is bool) return x == (bool)other;

            // otherwise convert other to a bool, and compare
            Conversion conv;
            int otherInt = Converter.TryConvertToInt32(other, out conv);
            if (conv < Conversion.Truncation) {
                int myint = x ? 1 : 0;
                return myint == otherInt;
            } else if (conv == Conversion.Truncation) {
                // if we truncated the value (eg 1.5) we wouldn't be equal
                // to True, but if we had something like 1.0 we didn't lose
                // any precision and we're equal
                if (EqualsTruncation(x, other)) return true;
            }
            return Ops.DynamicEqualRetBool(x, other);             
        }

        private static bool EqualsTruncation(bool x, object other) {
            if (other is double) {
                double dblOth = (double)other;

                if (x && dblOth == 1.0) return true;
                else if (!x && dblOth == 0.0) return true;
            } else if (other is float) {
                float fltOth = (float)other;

                if (x && fltOth == 1.0) return true;
                else if (!x && fltOth == 0.0) return true;
            } else if (other is decimal) {
                decimal decOth = (decimal)other;

                if (x && decOth == 1.0m) return true;
                else if (!x && decOth == 0.0m) return true;
            }
            return false;
        }
    }    
}
