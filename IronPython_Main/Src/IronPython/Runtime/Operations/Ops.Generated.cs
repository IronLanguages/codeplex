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
using System.Collections.Generic;
using System.Text;

using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Exceptions;

namespace IronPython.Runtime.Operations {
    public static partial class Ops {
        #region Generated Call Ops

        // *** BEGIN GENERATED CODE ***

        public const int MaximumCallArgs = 5;

        public static object Call(object func) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(DefaultContext.Default);

            return Ops.Call(func, EMPTY);
        }

        public static object Call(object func, object arg0) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(DefaultContext.Default, arg0);

            return Ops.Call(func, new object[] { arg0 });
        }

        public static object Call(object func, object arg0, object arg1) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(DefaultContext.Default, arg0, arg1);

            return Ops.Call(func, new object[] { arg0, arg1 });
        }

        public static object Call(object func, object arg0, object arg1, object arg2) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(DefaultContext.Default, arg0, arg1, arg2);

            return Ops.Call(func, new object[] { arg0, arg1, arg2 });
        }

        public static object Call(object func, object arg0, object arg1, object arg2, object arg3) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(DefaultContext.Default, arg0, arg1, arg2, arg3);

            return Ops.Call(func, new object[] { arg0, arg1, arg2, arg3 });
        }

        public static object Call(object func, object arg0, object arg1, object arg2, object arg3, object arg4) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(DefaultContext.Default, arg0, arg1, arg2, arg3, arg4);

            return Ops.Call(func, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }

        public static object CallWithContext(ICallerContext context, object func) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context);

            return Ops.CallWithContext(context, func, EMPTY);
        }

        public static object CallWithContext(ICallerContext context, object func, object arg0) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context, arg0);

            return Ops.CallWithContext(context, func, new object[] { arg0 });
        }

        public static object CallWithContext(ICallerContext context, object func, object arg0, object arg1) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context, arg0, arg1);

            return Ops.CallWithContext(context, func, new object[] { arg0, arg1 });
        }

        public static object CallWithContext(ICallerContext context, object func, object arg0, object arg1, object arg2) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context, arg0, arg1, arg2);

            return Ops.CallWithContext(context, func, new object[] { arg0, arg1, arg2 });
        }

        public static object CallWithContext(ICallerContext context, object func, object arg0, object arg1, object arg2, object arg3) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context, arg0, arg1, arg2, arg3);

            return Ops.CallWithContext(context, func, new object[] { arg0, arg1, arg2, arg3 });
        }

        public static object CallWithContext(ICallerContext context, object func, object arg0, object arg1, object arg2, object arg3, object arg4) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context, arg0, arg1, arg2, arg3, arg4);

            return Ops.CallWithContext(context, func, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }


        // *** END GENERATED CODE ***

        #endregion

        #region Generated Binary Ops

        // *** BEGIN GENERATED CODE ***


        public static object Add(object x, object y) {
            object ret;
            INumber inum;
            string sx, sy;
            ExtensibleString es = null;

            if (x is int) {
                ret = IntOps.Add((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if ((sx = x as string) != null && ((sy = y as string) != null || (es = y as ExtensibleString) != null)) {
                if (sy != null) return sx + sy;
                return sx + es.Value;
            } else if (x is double) {
                ret = FloatOps.Add((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.Add((IronMath.Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Add((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Add((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if ((inum = x as INumber) != null) {
                ret = inum.Add(y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Add((bool)x, y);
                if (ret != NotImplemented) return ret;
            }


            ISequence seq = x as ISequence;
            if (seq != null) { return seq.AddSequence(y); }


            ret = GetDynamicType(x).Add(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReverseAdd(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return Add(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return Add(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for +: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlaceAdd(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.Add((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is double) {
                ret = FloatOps.Add((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Add((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Add((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.Add((Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is ExtensibleFloat) {
                ret = FloatOps.Add(((ExtensibleFloat)x).value, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Add((bool)x, y);
                if (ret != NotImplemented) return ret;
            }


            if (x is string && y is string) {
                return ((string)x) + ((string)y);
            }

            if (x is ReflectedEvent) {
                return ((ReflectedEvent)x).__iadd__(y);
            }


            DynamicType dt = GetDynamicType(x);
            ret = dt.InPlaceAdd(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlaceAdd(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlaceAdd(x, po.Target);

            return Add(x, y);
        }


        public static object Subtract(object x, object y) {
            object ret;
            INumber inum;

            if (x is int) {
                ret = IntOps.Subtract((int)x, y);
                if (ret != NotImplemented) return ret;

            } else if (x is double) {
                ret = FloatOps.Subtract((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.Subtract((IronMath.Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Subtract((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Subtract((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if ((inum = x as INumber) != null) {
                ret = inum.Subtract(y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Subtract((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            ret = GetDynamicType(x).Subtract(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReverseSubtract(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return Subtract(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return Subtract(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for -: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlaceSubtract(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.Subtract((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is double) {
                ret = FloatOps.Subtract((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Subtract((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Subtract((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.Subtract((Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is ExtensibleFloat) {
                ret = FloatOps.Subtract(((ExtensibleFloat)x).value, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Subtract((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            DynamicType dt = GetDynamicType(x);
            ret = dt.InPlaceSubtract(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlaceSubtract(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlaceSubtract(x, po.Target);

            return Subtract(x, y);
        }


        public static object Power(object x, object y) {
            object ret;
            INumber inum;

            if (x is int) {
                ret = IntOps.Power((int)x, y);
                if (ret != NotImplemented) return ret;

            } else if (x is double) {
                ret = FloatOps.Power((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.Power((IronMath.Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Power((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Power((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if ((inum = x as INumber) != null) {
                ret = inum.Power(y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Power((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            ret = GetDynamicType(x).Power(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReversePower(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return Power(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return Power(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for **: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlacePower(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.Power((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is double) {
                ret = FloatOps.Power((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Power((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Power((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.Power((Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is ExtensibleFloat) {
                ret = FloatOps.Power(((ExtensibleFloat)x).value, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Power((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            DynamicType dt = GetDynamicType(x);
            ret = dt.InPlacePower(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlacePower(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlacePower(x, po.Target);

            return Power(x, y);
        }


        public static object Multiply(object x, object y) {
            object ret;
            INumber inum;

            if (x is int) {
                ret = IntOps.Multiply((int)x, y);
                if (ret != NotImplemented) return ret;

            } else if (x is double) {
                ret = FloatOps.Multiply((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.Multiply((IronMath.Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Multiply((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Multiply((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if ((inum = x as INumber) != null) {
                ret = inum.Multiply(y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Multiply((bool)x, y);
                if (ret != NotImplemented) return ret;
            }


            if (x is ISequence) {
                return ((ISequence)x).MultiplySequence(y);
            } else if (y is ISequence) {
                return ((ISequence)y).MultiplySequence(x);
            }


            ret = GetDynamicType(x).Multiply(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReverseMultiply(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return Multiply(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return Multiply(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for *: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlaceMultiply(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.Multiply((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is double) {
                ret = FloatOps.Multiply((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Multiply((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Multiply((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.Multiply((Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is ExtensibleFloat) {
                ret = FloatOps.Multiply(((ExtensibleFloat)x).value, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Multiply((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            DynamicType dt = GetDynamicType(x);
            ret = dt.InPlaceMultiply(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlaceMultiply(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlaceMultiply(x, po.Target);

            return Multiply(x, y);
        }


        public static object FloorDivide(object x, object y) {
            object ret;
            INumber inum;

            if (x is int) {
                ret = IntOps.FloorDivide((int)x, y);
                if (ret != NotImplemented) return ret;

            } else if (x is double) {
                ret = FloatOps.FloorDivide((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.FloorDivide((IronMath.Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.FloorDivide((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.FloorDivide((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if ((inum = x as INumber) != null) {
                ret = inum.FloorDivide(y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.FloorDivide((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            ret = GetDynamicType(x).FloorDivide(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReverseFloorDivide(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return FloorDivide(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return FloorDivide(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for //: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlaceFloorDivide(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.FloorDivide((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is double) {
                ret = FloatOps.FloorDivide((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.FloorDivide((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.FloorDivide((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.FloorDivide((Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is ExtensibleFloat) {
                ret = FloatOps.FloorDivide(((ExtensibleFloat)x).value, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.FloorDivide((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            DynamicType dt = GetDynamicType(x);
            ret = dt.InPlaceFloorDivide(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlaceFloorDivide(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlaceFloorDivide(x, po.Target);

            return FloorDivide(x, y);
        }


        public static object Divide(object x, object y) {
            object ret;
            INumber inum;

            if (x is int) {
                ret = IntOps.Divide((int)x, y);
                if (ret != NotImplemented) return ret;

            } else if (x is double) {
                ret = FloatOps.Divide((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.Divide((IronMath.Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Divide((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Divide((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if ((inum = x as INumber) != null) {
                ret = inum.Divide(y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Divide((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            ret = GetDynamicType(x).Divide(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReverseDivide(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return Divide(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return Divide(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for /: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlaceDivide(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.Divide((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is double) {
                ret = FloatOps.Divide((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Divide((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Divide((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.Divide((Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is ExtensibleFloat) {
                ret = FloatOps.Divide(((ExtensibleFloat)x).value, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Divide((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            DynamicType dt = GetDynamicType(x);
            ret = dt.InPlaceDivide(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlaceDivide(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlaceDivide(x, po.Target);

            return Divide(x, y);
        }


        public static object TrueDivide(object x, object y) {
            object ret;
            INumber inum;

            if (x is int) {
                ret = IntOps.TrueDivide((int)x, y);
                if (ret != NotImplemented) return ret;

            } else if (x is double) {
                ret = FloatOps.TrueDivide((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.TrueDivide((IronMath.Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.TrueDivide((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.TrueDivide((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if ((inum = x as INumber) != null) {
                ret = inum.TrueDivide(y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.TrueDivide((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            ret = GetDynamicType(x).TrueDivide(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReverseTrueDivide(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return TrueDivide(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return TrueDivide(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for /: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlaceTrueDivide(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.TrueDivide((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is double) {
                ret = FloatOps.TrueDivide((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.TrueDivide((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.TrueDivide((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.TrueDivide((Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is ExtensibleFloat) {
                ret = FloatOps.TrueDivide(((ExtensibleFloat)x).value, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.TrueDivide((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            DynamicType dt = GetDynamicType(x);
            ret = dt.InPlaceTrueDivide(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlaceTrueDivide(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlaceTrueDivide(x, po.Target);

            return TrueDivide(x, y);
        }


        public static object Mod(object x, object y) {
            object ret;
            INumber inum;

            if (x is int) {
                ret = IntOps.Mod((int)x, y);
                if (ret != NotImplemented) return ret;

            } else if (x is double) {
                ret = FloatOps.Mod((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.Mod((IronMath.Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Mod((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Mod((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if ((inum = x as INumber) != null) {
                ret = inum.Mod(y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Mod((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            ret = GetDynamicType(x).Mod(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReverseMod(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return Mod(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return Mod(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for %: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlaceMod(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.Mod((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is double) {
                ret = FloatOps.Mod((double)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Mod((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Mod((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.Complex64) {
                ret = ComplexOps.Mod((Complex64)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is ExtensibleFloat) {
                ret = FloatOps.Mod(((ExtensibleFloat)x).value, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Mod((bool)x, y);
                if (ret != NotImplemented) return ret;
            }



            DynamicType dt = GetDynamicType(x);
            ret = dt.InPlaceMod(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlaceMod(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlaceMod(x, po.Target);

            return Mod(x, y);
        }


        public static object LeftShift(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.LeftShift((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.LeftShift((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.LeftShift((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.LeftShift((bool)x, y);
                if (ret != NotImplemented) return ret;
            }
            ret = GetDynamicType(x).LeftShift(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReverseLeftShift(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return LeftShift(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return LeftShift(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for <<: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlaceLeftShift(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.LeftShift((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.LeftShift((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.LeftShift((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.LeftShift((bool)x, y);
                if (ret != NotImplemented) return ret;
            }

            ret = GetDynamicType(x).InPlaceLeftShift(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlaceLeftShift(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlaceLeftShift(x, po.Target);

            return LeftShift(x, y);
        }


        public static object RightShift(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.RightShift((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.RightShift((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.RightShift((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.RightShift((bool)x, y);
                if (ret != NotImplemented) return ret;
            }
            ret = GetDynamicType(x).RightShift(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReverseRightShift(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return RightShift(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return RightShift(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for >>: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlaceRightShift(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.RightShift((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.RightShift((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.RightShift((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.RightShift((bool)x, y);
                if (ret != NotImplemented) return ret;
            }

            ret = GetDynamicType(x).InPlaceRightShift(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlaceRightShift(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlaceRightShift(x, po.Target);

            return RightShift(x, y);
        }


        public static object BitwiseAnd(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.BitwiseAnd((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.BitwiseAnd((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.BitwiseAnd((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.BitwiseAnd((bool)x, y);
                if (ret != NotImplemented) return ret;
            }
            ret = GetDynamicType(x).BitwiseAnd(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReverseBitwiseAnd(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return BitwiseAnd(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return BitwiseAnd(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for &: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlaceBitwiseAnd(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.BitwiseAnd((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.BitwiseAnd((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.BitwiseAnd((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.BitwiseAnd((bool)x, y);
                if (ret != NotImplemented) return ret;
            }

            ret = GetDynamicType(x).InPlaceBitwiseAnd(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlaceBitwiseAnd(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlaceBitwiseAnd(x, po.Target);

            return BitwiseAnd(x, y);
        }


        public static object BitwiseOr(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.BitwiseOr((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.BitwiseOr((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.BitwiseOr((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.BitwiseOr((bool)x, y);
                if (ret != NotImplemented) return ret;
            }
            ret = GetDynamicType(x).BitwiseOr(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReverseBitwiseOr(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return BitwiseOr(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return BitwiseOr(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for |: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlaceBitwiseOr(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.BitwiseOr((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.BitwiseOr((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.BitwiseOr((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.BitwiseOr((bool)x, y);
                if (ret != NotImplemented) return ret;
            }

            ret = GetDynamicType(x).InPlaceBitwiseOr(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlaceBitwiseOr(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlaceBitwiseOr(x, po.Target);

            return BitwiseOr(x, y);
        }


        public static object Xor(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.Xor((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Xor((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Xor((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Xor((bool)x, y);
                if (ret != NotImplemented) return ret;
            }
            ret = GetDynamicType(x).Xor(x, y);
            if (ret != NotImplemented) return ret;
            ret = GetDynamicType(y).ReverseXor(y, x);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return Xor(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return Xor(x, po.Target);

            throw Ops.TypeError("unsupported operand type(s) for ^: '{0}' and '{1}'",
                                GetDynamicType(x).__name__, GetDynamicType(y).__name__);
        }

        public static object InPlaceXor(object x, object y) {
            object ret;
            if (x is int) {
                ret = IntOps.Xor((int)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is long) {
                ret = Int64Ops.Xor((long)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is IronMath.BigInteger) {
                ret = LongOps.Xor((IronMath.BigInteger)x, y);
                if (ret != NotImplemented) return ret;
            } else if (x is bool) {
                ret = BoolOps.Xor((bool)x, y);
                if (ret != NotImplemented) return ret;
            }

            ret = GetDynamicType(x).InPlaceXor(x, y);
            if (ret != NotImplemented) return ret;

            IProxyObject po = x as IProxyObject;
            if (po != null) return InPlaceXor(po.Target, y);
            po = y as IProxyObject;
            if (po != null) return InPlaceXor(x, po.Target);

            return Xor(x, y);
        }


        public static object LessThan(object x, object y) {
            if (x is int) {
                if (y is int) {
                    return Ops.Bool2Object(((int)x) < ((int)y));
                } else if (y is double) {
                    return Ops.Bool2Object(((int)x) < ((double)y));
                } else if (y == null) {
                    return Ops.Bool2Object(1 < 0);
                }
            } else if (x is double) {
                if (y is int) {
                    return Ops.Bool2Object(((double)x) < ((int)y));
                } else if (y is double) {
                    return Ops.Bool2Object(((double)x) < ((double)y));
                } else if (y is ExtensibleFloat) {
                    return Ops.Bool2Object(((double)x) < ((ExtensibleFloat)y).value);
                } else if (y == null) {
                    return Ops.Bool2Object(1 < 0);
                } else {
                    BigInteger bi = y as BigInteger;
                    if (!Object.ReferenceEquals(bi, null)) {
                        BigInteger self = BigInteger.Create((double)x);
                        double dblSelf = (double)x;
                        if (self == bi) {
                            double mod = dblSelf % 1;
                            if (mod != 0) {
                                if (dblSelf > 0)
                                    return Ops.Bool2Object(1 < 0);
                                return Ops.Bool2Object(0 < 1);
                            }
                        }

                        return Ops.Bool2Object(self < bi);
                    }
                }
            } else if (x is bool) {
                if (y is bool) {
                    return Ops.Bool2Object((((bool)x) ? 1 : 0) < (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return Ops.Bool2Object(1 < 0);
                }
            } else if (x is BigInteger) {
                if (y is BigInteger) {
                    return Ops.Bool2Object(((BigInteger)x) < ((BigInteger)y));
                } else if (y is bool) {
                    return Ops.Bool2Object(((BigInteger)x) < (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return Ops.Bool2Object(1 < 0);
                } else if (y is double) {
                    double dbl = (double)y;
                    return Ops.Bool2Object((((int)FloatOps.Compare(dbl, x)) * -1) < 0);
                }
            } else if (x is short) {
                object res = IntOps.Compare((int)(short)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) < 0);
            } else if (x is ushort) {
                object res = IntOps.Compare((int)(ushort)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) < 0);
            } else if (x is byte) {
                object res = IntOps.Compare((int)(byte)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) < 0);
            } else if (x is sbyte) {
                object res = IntOps.Compare((int)(sbyte)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) < 0);
            } else if (x is ulong) {
                object res = Int64Ops.Compare((ulong)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) < 0);
            } else if (x is uint) {
                object res = Int64Ops.Compare((long)(uint)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) < 0);
            } else if (x is decimal) {
                object res = FloatOps.Compare((double)(decimal)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) < 0);
            } else if (x == null) {
                if (y == null) return Ops.Bool2Object(false);

                if (y.GetType().IsPrimitive || y is BigInteger) {
                    // built-in type that doesn't implement our comparable
                    // interfaces, being compared against null, go ahead
                    // and skip the rest of the checks.
                    return Ops.Bool2Object(0 < 1);
                }
            }

            if (x is string && y is string) {
                return Ops.Bool2Object(string.CompareOrdinal((string)x, (string)y) < 0);
            }

            object ret;
            IRichComparable pc1 = x as IRichComparable;
            IRichComparable pc2 = y as IRichComparable;
            if (pc1 != null)
                if ((ret = pc1.LessThan(y)) != Ops.NotImplemented) return ret;
            if (pc2 != null)
                if ((ret = pc2.GreaterThan(x)) != Ops.NotImplemented) return ret;
            if (pc1 != null)
                if ((ret = pc1.CompareTo(y)) != Ops.NotImplemented) return Ops.Bool2Object(Ops.CompareToZero(ret) < 0);
            if (pc2 != null)
                if ((ret = pc2.CompareTo(x)) != Ops.NotImplemented) return Ops.Bool2Object((-1 * Ops.CompareToZero(ret)) < 0);

            Type xType = (x == null) ? null : x.GetType(), yType = (y == null) ? null : y.GetType();

            IComparable c = x as IComparable;
            if (c != null && xType == yType) {
                return Ops.Bool2Object(c.CompareTo(y) < 0);
            }
            c = y as IComparable;
            if (c != null && xType == yType) {
                return Ops.Bool2Object(-1 * c.CompareTo(x) < 0);
            }

            DynamicType dt1 = GetDynamicType(x);
            DynamicType dt2 = GetDynamicType(y);
            if ((ret = dt1.LessThan(x, y)) != NotImplemented) return ret;
            if ((ret = dt2.GreaterThan(y, x)) != NotImplemented) return ret;
            if ((ret = dt1.CompareTo(x, y)) != Ops.NotImplemented) return Ops.Bool2Object(Ops.CompareToZero(ret) < 0);
            if ((ret = dt2.CompareTo(y, x)) != Ops.NotImplemented) return Ops.Bool2Object((-1 * Ops.CompareToZero(ret)) < 0);

            if (xType == yType) {
                return Ops.Bool2Object((IdDispenser.GetId(x) - IdDispenser.GetId(y)) < 0);
            } else {
                string xName = (xType == null) ? "!NoneType" : xType.Name, yName = (yType == null) ? "!NoneType" : yType.Name;

                return Ops.Bool2Object(string.CompareOrdinal(xName, yName) < 0);
            }
        }

        public static bool LessThanRetBool(object x, object y) {
            if (x is int) {
                if (y is int) {
                    return (((int)x) < ((int)y));
                } else if (y is double) {
                    return (((int)x) < ((double)y));
                } else if (y == null) {
                    return (1 < 0);
                }
            } else if (x is double) {
                if (y is int) {
                    return (((double)x) < ((int)y));
                } else if (y is double) {
                    return (((double)x) < ((double)y));
                } else if (y is ExtensibleFloat) {
                    return (((double)x) < ((ExtensibleFloat)y).value);
                } else if (y == null) {
                    return (1 < 0);
                } else {
                    BigInteger bi = y as BigInteger;
                    if (!Object.ReferenceEquals(bi, null)) {
                        BigInteger self = BigInteger.Create((double)x);
                        double dblSelf = (double)x;
                        if (self == bi) {
                            double mod = dblSelf % 1;
                            if (mod != 0) {
                                if (dblSelf > 0)
                                    return (1 < 0);
                                return (0 < 1);
                            }
                        }

                        return (self < bi);
                    }
                }
            } else if (x is bool) {
                if (y is bool) {
                    return ((((bool)x) ? 1 : 0) < (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return (1 < 0);
                }
            } else if (x is BigInteger) {
                if (y is BigInteger) {
                    return (((BigInteger)x) < ((BigInteger)y));
                } else if (y is bool) {
                    return (((BigInteger)x) < (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return (1 < 0);
                } else if (y is double) {
                    double dbl = (double)y;
                    return ((((int)FloatOps.Compare(dbl, x)) * -1) < 0);
                }
            } else if (x is short) {
                object res = IntOps.Compare((int)(short)x, y);
                if (res != Ops.NotImplemented) return (((int)res) < 0);
            } else if (x is ushort) {
                object res = IntOps.Compare((int)(ushort)x, y);
                if (res != Ops.NotImplemented) return (((int)res) < 0);
            } else if (x is byte) {
                object res = IntOps.Compare((int)(byte)x, y);
                if (res != Ops.NotImplemented) return (((int)res) < 0);
            } else if (x is sbyte) {
                object res = IntOps.Compare((int)(sbyte)x, y);
                if (res != Ops.NotImplemented) return (((int)res) < 0);
            } else if (x is ulong) {
                object res = Int64Ops.Compare((ulong)x, y);
                if (res != Ops.NotImplemented) return (((int)res) < 0);
            } else if (x is uint) {
                object res = Int64Ops.Compare((long)(uint)x, y);
                if (res != Ops.NotImplemented) return (((int)res) < 0);
            } else if (x is decimal) {
                object res = FloatOps.Compare((double)(decimal)x, y);
                if (res != Ops.NotImplemented) return (((int)res) < 0);
            } else if (x == null) {
                if (y == null) return (false);

                if (y.GetType().IsPrimitive || y is BigInteger) {
                    // built-in type that doesn't implement our comparable
                    // interfaces, being compared against null, go ahead
                    // and skip the rest of the checks.
                    return (0 < 1);
                }
            }

            if (x is string && y is string) {
                return (string.CompareOrdinal((string)x, (string)y) < 0);
            }

            object ret;
            IRichComparable pc1 = x as IRichComparable;
            IRichComparable pc2 = y as IRichComparable;
            if (pc1 != null)
                if ((ret = pc1.LessThan(y)) != Ops.NotImplemented) return Ops.IsTrue(ret);
            if (pc2 != null)
                if ((ret = pc2.GreaterThan(x)) != Ops.NotImplemented) return Ops.IsTrue(ret);
            if (pc1 != null)
                if ((ret = pc1.CompareTo(y)) != Ops.NotImplemented) return (Ops.CompareToZero(ret) < 0);
            if (pc2 != null)
                if ((ret = pc2.CompareTo(x)) != Ops.NotImplemented) return ((-1 * Ops.CompareToZero(ret)) < 0);

            Type xType = (x == null) ? null : x.GetType(), yType = (y == null) ? null : y.GetType();

            IComparable c = x as IComparable;
            if (c != null && xType == yType) {
                return (c.CompareTo(y) < 0);
            }
            c = y as IComparable;
            if (c != null && xType == yType) {
                return (-1 * c.CompareTo(x) < 0);
            }

            DynamicType dt1 = GetDynamicType(x);
            DynamicType dt2 = GetDynamicType(y);
            if ((ret = dt1.LessThan(x, y)) != NotImplemented) return Ops.IsTrue(ret);
            if ((ret = dt2.GreaterThan(y, x)) != NotImplemented) return Ops.IsTrue(ret);
            if ((ret = dt1.CompareTo(x, y)) != Ops.NotImplemented) return (Ops.CompareToZero(ret) < 0);
            if ((ret = dt2.CompareTo(y, x)) != Ops.NotImplemented) return ((-1 * Ops.CompareToZero(ret)) < 0);

            if (xType == yType) {
                return ((IdDispenser.GetId(x) - IdDispenser.GetId(y)) < 0);
            } else {
                string xName = (xType == null) ? "!NoneType" : xType.Name, yName = (yType == null) ? "!NoneType" : yType.Name;

                return (string.CompareOrdinal(xName, yName) < 0);
            }
        }

        public static object GreaterThan(object x, object y) {
            if (x is int) {
                if (y is int) {
                    return Ops.Bool2Object(((int)x) > ((int)y));
                } else if (y is double) {
                    return Ops.Bool2Object(((int)x) > ((double)y));
                } else if (y == null) {
                    return Ops.Bool2Object(1 > 0);
                }
            } else if (x is double) {
                if (y is int) {
                    return Ops.Bool2Object(((double)x) > ((int)y));
                } else if (y is double) {
                    return Ops.Bool2Object(((double)x) > ((double)y));
                } else if (y is ExtensibleFloat) {
                    return Ops.Bool2Object(((double)x) > ((ExtensibleFloat)y).value);
                } else if (y == null) {
                    return Ops.Bool2Object(1 > 0);
                } else {
                    BigInteger bi = y as BigInteger;
                    if (!Object.ReferenceEquals(bi, null)) {
                        BigInteger self = BigInteger.Create((double)x);
                        double dblSelf = (double)x;
                        if (self == bi) {
                            double mod = dblSelf % 1;
                            if (mod != 0) {
                                if (dblSelf > 0)
                                    return Ops.Bool2Object(1 > 0);
                                return Ops.Bool2Object(0 > 1);
                            }
                        }

                        return Ops.Bool2Object(self > bi);
                    }
                }
            } else if (x is bool) {
                if (y is bool) {
                    return Ops.Bool2Object((((bool)x) ? 1 : 0) > (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return Ops.Bool2Object(1 > 0);
                }
            } else if (x is BigInteger) {
                if (y is BigInteger) {
                    return Ops.Bool2Object(((BigInteger)x) > ((BigInteger)y));
                } else if (y is bool) {
                    return Ops.Bool2Object(((BigInteger)x) > (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return Ops.Bool2Object(1 > 0);
                } else if (y is double) {
                    double dbl = (double)y;
                    return Ops.Bool2Object((((int)FloatOps.Compare(dbl, x)) * -1) > 0);
                }
            } else if (x is short) {
                object res = IntOps.Compare((int)(short)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) > 0);
            } else if (x is ushort) {
                object res = IntOps.Compare((int)(ushort)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) > 0);
            } else if (x is byte) {
                object res = IntOps.Compare((int)(byte)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) > 0);
            } else if (x is sbyte) {
                object res = IntOps.Compare((int)(sbyte)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) > 0);
            } else if (x is ulong) {
                object res = Int64Ops.Compare((ulong)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) > 0);
            } else if (x is uint) {
                object res = Int64Ops.Compare((long)(uint)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) > 0);
            } else if (x is decimal) {
                object res = FloatOps.Compare((double)(decimal)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) > 0);
            } else if (x == null) {
                if (y == null) return Ops.Bool2Object(false);

                if (y.GetType().IsPrimitive || y is BigInteger) {
                    // built-in type that doesn't implement our comparable
                    // interfaces, being compared against null, go ahead
                    // and skip the rest of the checks.
                    return Ops.Bool2Object(0 > 1);
                }
            }

            if (x is string && y is string) {
                return Ops.Bool2Object(string.CompareOrdinal((string)x, (string)y) > 0);
            }

            object ret;
            IRichComparable pc1 = x as IRichComparable;
            IRichComparable pc2 = y as IRichComparable;
            if (pc1 != null)
                if ((ret = pc1.GreaterThan(y)) != Ops.NotImplemented) return ret;
            if (pc2 != null)
                if ((ret = pc2.LessThan(x)) != Ops.NotImplemented) return ret;
            if (pc1 != null)
                if ((ret = pc1.CompareTo(y)) != Ops.NotImplemented) return Ops.Bool2Object(Ops.CompareToZero(ret) > 0);
            if (pc2 != null)
                if ((ret = pc2.CompareTo(x)) != Ops.NotImplemented) return Ops.Bool2Object((-1 * Ops.CompareToZero(ret)) > 0);

            Type xType = (x == null) ? null : x.GetType(), yType = (y == null) ? null : y.GetType();

            IComparable c = x as IComparable;
            if (c != null && xType == yType) {
                return Ops.Bool2Object(c.CompareTo(y) > 0);
            }
            c = y as IComparable;
            if (c != null && xType == yType) {
                return Ops.Bool2Object(-1 * c.CompareTo(x) > 0);
            }

            DynamicType dt1 = GetDynamicType(x);
            DynamicType dt2 = GetDynamicType(y);
            if ((ret = dt1.GreaterThan(x, y)) != NotImplemented) return ret;
            if ((ret = dt2.LessThan(y, x)) != NotImplemented) return ret;
            if ((ret = dt1.CompareTo(x, y)) != Ops.NotImplemented) return Ops.Bool2Object(Ops.CompareToZero(ret) > 0);
            if ((ret = dt2.CompareTo(y, x)) != Ops.NotImplemented) return Ops.Bool2Object((-1 * Ops.CompareToZero(ret)) > 0);

            if (xType == yType) {
                return Ops.Bool2Object((IdDispenser.GetId(x) - IdDispenser.GetId(y)) > 0);
            } else {
                string xName = (xType == null) ? "!NoneType" : xType.Name, yName = (yType == null) ? "!NoneType" : yType.Name;

                return Ops.Bool2Object(string.CompareOrdinal(xName, yName) > 0);
            }
        }

        public static bool GreaterThanRetBool(object x, object y) {
            if (x is int) {
                if (y is int) {
                    return (((int)x) > ((int)y));
                } else if (y is double) {
                    return (((int)x) > ((double)y));
                } else if (y == null) {
                    return (1 > 0);
                }
            } else if (x is double) {
                if (y is int) {
                    return (((double)x) > ((int)y));
                } else if (y is double) {
                    return (((double)x) > ((double)y));
                } else if (y is ExtensibleFloat) {
                    return (((double)x) > ((ExtensibleFloat)y).value);
                } else if (y == null) {
                    return (1 > 0);
                } else {
                    BigInteger bi = y as BigInteger;
                    if (!Object.ReferenceEquals(bi, null)) {
                        BigInteger self = BigInteger.Create((double)x);
                        double dblSelf = (double)x;
                        if (self == bi) {
                            double mod = dblSelf % 1;
                            if (mod != 0) {
                                if (dblSelf > 0)
                                    return (1 > 0);
                                return (0 > 1);
                            }
                        }

                        return (self > bi);
                    }
                }
            } else if (x is bool) {
                if (y is bool) {
                    return ((((bool)x) ? 1 : 0) > (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return (1 > 0);
                }
            } else if (x is BigInteger) {
                if (y is BigInteger) {
                    return (((BigInteger)x) > ((BigInteger)y));
                } else if (y is bool) {
                    return (((BigInteger)x) > (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return (1 > 0);
                } else if (y is double) {
                    double dbl = (double)y;
                    return ((((int)FloatOps.Compare(dbl, x)) * -1) > 0);
                }
            } else if (x is short) {
                object res = IntOps.Compare((int)(short)x, y);
                if (res != Ops.NotImplemented) return (((int)res) > 0);
            } else if (x is ushort) {
                object res = IntOps.Compare((int)(ushort)x, y);
                if (res != Ops.NotImplemented) return (((int)res) > 0);
            } else if (x is byte) {
                object res = IntOps.Compare((int)(byte)x, y);
                if (res != Ops.NotImplemented) return (((int)res) > 0);
            } else if (x is sbyte) {
                object res = IntOps.Compare((int)(sbyte)x, y);
                if (res != Ops.NotImplemented) return (((int)res) > 0);
            } else if (x is ulong) {
                object res = Int64Ops.Compare((ulong)x, y);
                if (res != Ops.NotImplemented) return (((int)res) > 0);
            } else if (x is uint) {
                object res = Int64Ops.Compare((long)(uint)x, y);
                if (res != Ops.NotImplemented) return (((int)res) > 0);
            } else if (x is decimal) {
                object res = FloatOps.Compare((double)(decimal)x, y);
                if (res != Ops.NotImplemented) return (((int)res) > 0);
            } else if (x == null) {
                if (y == null) return (false);

                if (y.GetType().IsPrimitive || y is BigInteger) {
                    // built-in type that doesn't implement our comparable
                    // interfaces, being compared against null, go ahead
                    // and skip the rest of the checks.
                    return (0 > 1);
                }
            }

            if (x is string && y is string) {
                return (string.CompareOrdinal((string)x, (string)y) > 0);
            }

            object ret;
            IRichComparable pc1 = x as IRichComparable;
            IRichComparable pc2 = y as IRichComparable;
            if (pc1 != null)
                if ((ret = pc1.GreaterThan(y)) != Ops.NotImplemented) return Ops.IsTrue(ret);
            if (pc2 != null)
                if ((ret = pc2.LessThan(x)) != Ops.NotImplemented) return Ops.IsTrue(ret);
            if (pc1 != null)
                if ((ret = pc1.CompareTo(y)) != Ops.NotImplemented) return (Ops.CompareToZero(ret) > 0);
            if (pc2 != null)
                if ((ret = pc2.CompareTo(x)) != Ops.NotImplemented) return ((-1 * Ops.CompareToZero(ret)) > 0);

            Type xType = (x == null) ? null : x.GetType(), yType = (y == null) ? null : y.GetType();

            IComparable c = x as IComparable;
            if (c != null && xType == yType) {
                return (c.CompareTo(y) > 0);
            }
            c = y as IComparable;
            if (c != null && xType == yType) {
                return (-1 * c.CompareTo(x) > 0);
            }

            DynamicType dt1 = GetDynamicType(x);
            DynamicType dt2 = GetDynamicType(y);
            if ((ret = dt1.GreaterThan(x, y)) != NotImplemented) return Ops.IsTrue(ret);
            if ((ret = dt2.LessThan(y, x)) != NotImplemented) return Ops.IsTrue(ret);
            if ((ret = dt1.CompareTo(x, y)) != Ops.NotImplemented) return (Ops.CompareToZero(ret) > 0);
            if ((ret = dt2.CompareTo(y, x)) != Ops.NotImplemented) return ((-1 * Ops.CompareToZero(ret)) > 0);

            if (xType == yType) {
                return ((IdDispenser.GetId(x) - IdDispenser.GetId(y)) > 0);
            } else {
                string xName = (xType == null) ? "!NoneType" : xType.Name, yName = (yType == null) ? "!NoneType" : yType.Name;

                return (string.CompareOrdinal(xName, yName) > 0);
            }
        }

        public static object LessThanOrEqual(object x, object y) {
            if (x is int) {
                if (y is int) {
                    return Ops.Bool2Object(((int)x) <= ((int)y));
                } else if (y is double) {
                    return Ops.Bool2Object(((int)x) <= ((double)y));
                } else if (y == null) {
                    return Ops.Bool2Object(1 <= 0);
                }
            } else if (x is double) {
                if (y is int) {
                    return Ops.Bool2Object(((double)x) <= ((int)y));
                } else if (y is double) {
                    return Ops.Bool2Object(((double)x) <= ((double)y));
                } else if (y is ExtensibleFloat) {
                    return Ops.Bool2Object(((double)x) <= ((ExtensibleFloat)y).value);
                } else if (y == null) {
                    return Ops.Bool2Object(1 <= 0);
                } else {
                    BigInteger bi = y as BigInteger;
                    if (!Object.ReferenceEquals(bi, null)) {
                        BigInteger self = BigInteger.Create((double)x);
                        double dblSelf = (double)x;
                        if (self == bi) {
                            double mod = dblSelf % 1;
                            if (mod != 0) {
                                if (dblSelf > 0)
                                    return Ops.Bool2Object(1 <= 0);
                                return Ops.Bool2Object(0 <= 1);
                            }
                        }

                        return Ops.Bool2Object(self <= bi);
                    }
                }
            } else if (x is bool) {
                if (y is bool) {
                    return Ops.Bool2Object((((bool)x) ? 1 : 0) <= (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return Ops.Bool2Object(1 <= 0);
                }
            } else if (x is BigInteger) {
                if (y is BigInteger) {
                    return Ops.Bool2Object(((BigInteger)x) <= ((BigInteger)y));
                } else if (y is bool) {
                    return Ops.Bool2Object(((BigInteger)x) <= (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return Ops.Bool2Object(1 <= 0);
                } else if (y is double) {
                    double dbl = (double)y;
                    return Ops.Bool2Object((((int)FloatOps.Compare(dbl, x)) * -1) <= 0);
                }
            } else if (x is short) {
                object res = IntOps.Compare((int)(short)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) <= 0);
            } else if (x is ushort) {
                object res = IntOps.Compare((int)(ushort)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) <= 0);
            } else if (x is byte) {
                object res = IntOps.Compare((int)(byte)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) <= 0);
            } else if (x is sbyte) {
                object res = IntOps.Compare((int)(sbyte)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) <= 0);
            } else if (x is ulong) {
                object res = Int64Ops.Compare((ulong)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) <= 0);
            } else if (x is uint) {
                object res = Int64Ops.Compare((long)(uint)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) <= 0);
            } else if (x is decimal) {
                object res = FloatOps.Compare((double)(decimal)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) <= 0);
            } else if (x == null) {
                if (y == null) return Ops.Bool2Object(true);

                if (y.GetType().IsPrimitive || y is BigInteger) {
                    // built-in type that doesn't implement our comparable
                    // interfaces, being compared against null, go ahead
                    // and skip the rest of the checks.
                    return Ops.Bool2Object(0 <= 1);
                }
            }

            if (x is string && y is string) {
                return Ops.Bool2Object(string.CompareOrdinal((string)x, (string)y) <= 0);
            }

            object ret;
            IRichComparable pc1 = x as IRichComparable;
            IRichComparable pc2 = y as IRichComparable;
            if (pc1 != null)
                if ((ret = pc1.LessThanOrEqual(y)) != Ops.NotImplemented) return ret;
            if (pc2 != null)
                if ((ret = pc2.GreaterThanOrEqual(x)) != Ops.NotImplemented) return ret;
            if (pc1 != null)
                if ((ret = pc1.CompareTo(y)) != Ops.NotImplemented) return Ops.Bool2Object(Ops.CompareToZero(ret) <= 0);
            if (pc2 != null)
                if ((ret = pc2.CompareTo(x)) != Ops.NotImplemented) return Ops.Bool2Object((-1 * Ops.CompareToZero(ret)) <= 0);

            Type xType = (x == null) ? null : x.GetType(), yType = (y == null) ? null : y.GetType();

            IComparable c = x as IComparable;
            if (c != null && xType == yType) {
                return Ops.Bool2Object(c.CompareTo(y) <= 0);
            }
            c = y as IComparable;
            if (c != null && xType == yType) {
                return Ops.Bool2Object(-1 * c.CompareTo(x) <= 0);
            }

            DynamicType dt1 = GetDynamicType(x);
            DynamicType dt2 = GetDynamicType(y);
            if ((ret = dt1.LessThanOrEqual(x, y)) != NotImplemented) return ret;
            if ((ret = dt2.GreaterThanOrEqual(y, x)) != NotImplemented) return ret;
            if ((ret = dt1.CompareTo(x, y)) != Ops.NotImplemented) return Ops.Bool2Object(Ops.CompareToZero(ret) <= 0);
            if ((ret = dt2.CompareTo(y, x)) != Ops.NotImplemented) return Ops.Bool2Object((-1 * Ops.CompareToZero(ret)) <= 0);

            if (xType == yType) {
                return Ops.Bool2Object((IdDispenser.GetId(x) - IdDispenser.GetId(y)) <= 0);
            } else {
                string xName = (xType == null) ? "!NoneType" : xType.Name, yName = (yType == null) ? "!NoneType" : yType.Name;

                return Ops.Bool2Object(string.CompareOrdinal(xName, yName) <= 0);
            }
        }

        public static bool LessThanOrEqualRetBool(object x, object y) {
            if (x is int) {
                if (y is int) {
                    return (((int)x) <= ((int)y));
                } else if (y is double) {
                    return (((int)x) <= ((double)y));
                } else if (y == null) {
                    return (1 <= 0);
                }
            } else if (x is double) {
                if (y is int) {
                    return (((double)x) <= ((int)y));
                } else if (y is double) {
                    return (((double)x) <= ((double)y));
                } else if (y is ExtensibleFloat) {
                    return (((double)x) <= ((ExtensibleFloat)y).value);
                } else if (y == null) {
                    return (1 <= 0);
                } else {
                    BigInteger bi = y as BigInteger;
                    if (!Object.ReferenceEquals(bi, null)) {
                        BigInteger self = BigInteger.Create((double)x);
                        double dblSelf = (double)x;
                        if (self == bi) {
                            double mod = dblSelf % 1;
                            if (mod != 0) {
                                if (dblSelf > 0)
                                    return (1 <= 0);
                                return (0 <= 1);
                            }
                        }

                        return (self <= bi);
                    }
                }
            } else if (x is bool) {
                if (y is bool) {
                    return ((((bool)x) ? 1 : 0) <= (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return (1 <= 0);
                }
            } else if (x is BigInteger) {
                if (y is BigInteger) {
                    return (((BigInteger)x) <= ((BigInteger)y));
                } else if (y is bool) {
                    return (((BigInteger)x) <= (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return (1 <= 0);
                } else if (y is double) {
                    double dbl = (double)y;
                    return ((((int)FloatOps.Compare(dbl, x)) * -1) <= 0);
                }
            } else if (x is short) {
                object res = IntOps.Compare((int)(short)x, y);
                if (res != Ops.NotImplemented) return (((int)res) <= 0);
            } else if (x is ushort) {
                object res = IntOps.Compare((int)(ushort)x, y);
                if (res != Ops.NotImplemented) return (((int)res) <= 0);
            } else if (x is byte) {
                object res = IntOps.Compare((int)(byte)x, y);
                if (res != Ops.NotImplemented) return (((int)res) <= 0);
            } else if (x is sbyte) {
                object res = IntOps.Compare((int)(sbyte)x, y);
                if (res != Ops.NotImplemented) return (((int)res) <= 0);
            } else if (x is ulong) {
                object res = Int64Ops.Compare((ulong)x, y);
                if (res != Ops.NotImplemented) return (((int)res) <= 0);
            } else if (x is uint) {
                object res = Int64Ops.Compare((long)(uint)x, y);
                if (res != Ops.NotImplemented) return (((int)res) <= 0);
            } else if (x is decimal) {
                object res = FloatOps.Compare((double)(decimal)x, y);
                if (res != Ops.NotImplemented) return (((int)res) <= 0);
            } else if (x == null) {
                if (y == null) return (true);

                if (y.GetType().IsPrimitive || y is BigInteger) {
                    // built-in type that doesn't implement our comparable
                    // interfaces, being compared against null, go ahead
                    // and skip the rest of the checks.
                    return (0 <= 1);
                }
            }

            if (x is string && y is string) {
                return (string.CompareOrdinal((string)x, (string)y) <= 0);
            }

            object ret;
            IRichComparable pc1 = x as IRichComparable;
            IRichComparable pc2 = y as IRichComparable;
            if (pc1 != null)
                if ((ret = pc1.LessThanOrEqual(y)) != Ops.NotImplemented) return Ops.IsTrue(ret);
            if (pc2 != null)
                if ((ret = pc2.GreaterThanOrEqual(x)) != Ops.NotImplemented) return Ops.IsTrue(ret);
            if (pc1 != null)
                if ((ret = pc1.CompareTo(y)) != Ops.NotImplemented) return (Ops.CompareToZero(ret) <= 0);
            if (pc2 != null)
                if ((ret = pc2.CompareTo(x)) != Ops.NotImplemented) return ((-1 * Ops.CompareToZero(ret)) <= 0);

            Type xType = (x == null) ? null : x.GetType(), yType = (y == null) ? null : y.GetType();

            IComparable c = x as IComparable;
            if (c != null && xType == yType) {
                return (c.CompareTo(y) <= 0);
            }
            c = y as IComparable;
            if (c != null && xType == yType) {
                return (-1 * c.CompareTo(x) <= 0);
            }

            DynamicType dt1 = GetDynamicType(x);
            DynamicType dt2 = GetDynamicType(y);
            if ((ret = dt1.LessThanOrEqual(x, y)) != NotImplemented) return Ops.IsTrue(ret);
            if ((ret = dt2.GreaterThanOrEqual(y, x)) != NotImplemented) return Ops.IsTrue(ret);
            if ((ret = dt1.CompareTo(x, y)) != Ops.NotImplemented) return (Ops.CompareToZero(ret) <= 0);
            if ((ret = dt2.CompareTo(y, x)) != Ops.NotImplemented) return ((-1 * Ops.CompareToZero(ret)) <= 0);

            if (xType == yType) {
                return ((IdDispenser.GetId(x) - IdDispenser.GetId(y)) <= 0);
            } else {
                string xName = (xType == null) ? "!NoneType" : xType.Name, yName = (yType == null) ? "!NoneType" : yType.Name;

                return (string.CompareOrdinal(xName, yName) <= 0);
            }
        }

        public static object GreaterThanOrEqual(object x, object y) {
            if (x is int) {
                if (y is int) {
                    return Ops.Bool2Object(((int)x) >= ((int)y));
                } else if (y is double) {
                    return Ops.Bool2Object(((int)x) >= ((double)y));
                } else if (y == null) {
                    return Ops.Bool2Object(1 >= 0);
                }
            } else if (x is double) {
                if (y is int) {
                    return Ops.Bool2Object(((double)x) >= ((int)y));
                } else if (y is double) {
                    return Ops.Bool2Object(((double)x) >= ((double)y));
                } else if (y is ExtensibleFloat) {
                    return Ops.Bool2Object(((double)x) >= ((ExtensibleFloat)y).value);
                } else if (y == null) {
                    return Ops.Bool2Object(1 >= 0);
                } else {
                    BigInteger bi = y as BigInteger;
                    if (!Object.ReferenceEquals(bi, null)) {
                        BigInteger self = BigInteger.Create((double)x);
                        double dblSelf = (double)x;
                        if (self == bi) {
                            double mod = dblSelf % 1;
                            if (mod != 0) {
                                if (dblSelf > 0)
                                    return Ops.Bool2Object(1 >= 0);
                                return Ops.Bool2Object(0 >= 1);
                            }
                        }

                        return Ops.Bool2Object(self >= bi);
                    }
                }
            } else if (x is bool) {
                if (y is bool) {
                    return Ops.Bool2Object((((bool)x) ? 1 : 0) >= (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return Ops.Bool2Object(1 >= 0);
                }
            } else if (x is BigInteger) {
                if (y is BigInteger) {
                    return Ops.Bool2Object(((BigInteger)x) >= ((BigInteger)y));
                } else if (y is bool) {
                    return Ops.Bool2Object(((BigInteger)x) >= (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return Ops.Bool2Object(1 >= 0);
                } else if (y is double) {
                    double dbl = (double)y;
                    return Ops.Bool2Object((((int)FloatOps.Compare(dbl, x)) * -1) >= 0);
                }
            } else if (x is short) {
                object res = IntOps.Compare((int)(short)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) >= 0);
            } else if (x is ushort) {
                object res = IntOps.Compare((int)(ushort)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) >= 0);
            } else if (x is byte) {
                object res = IntOps.Compare((int)(byte)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) >= 0);
            } else if (x is sbyte) {
                object res = IntOps.Compare((int)(sbyte)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) >= 0);
            } else if (x is ulong) {
                object res = Int64Ops.Compare((ulong)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) >= 0);
            } else if (x is uint) {
                object res = Int64Ops.Compare((long)(uint)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) >= 0);
            } else if (x is decimal) {
                object res = FloatOps.Compare((double)(decimal)x, y);
                if (res != Ops.NotImplemented) return Ops.Bool2Object(((int)res) >= 0);
            } else if (x == null) {
                if (y == null) return Ops.Bool2Object(true);

                if (y.GetType().IsPrimitive || y is BigInteger) {
                    // built-in type that doesn't implement our comparable
                    // interfaces, being compared against null, go ahead
                    // and skip the rest of the checks.
                    return Ops.Bool2Object(0 >= 1);
                }
            }

            if (x is string && y is string) {
                return Ops.Bool2Object(string.CompareOrdinal((string)x, (string)y) >= 0);
            }

            object ret;
            IRichComparable pc1 = x as IRichComparable;
            IRichComparable pc2 = y as IRichComparable;
            if (pc1 != null)
                if ((ret = pc1.GreaterThanOrEqual(y)) != Ops.NotImplemented) return ret;
            if (pc2 != null)
                if ((ret = pc2.LessThanOrEqual(x)) != Ops.NotImplemented) return ret;
            if (pc1 != null)
                if ((ret = pc1.CompareTo(y)) != Ops.NotImplemented) return Ops.Bool2Object(Ops.CompareToZero(ret) >= 0);
            if (pc2 != null)
                if ((ret = pc2.CompareTo(x)) != Ops.NotImplemented) return Ops.Bool2Object((-1 * Ops.CompareToZero(ret)) >= 0);

            Type xType = (x == null) ? null : x.GetType(), yType = (y == null) ? null : y.GetType();

            IComparable c = x as IComparable;
            if (c != null && xType == yType) {
                return Ops.Bool2Object(c.CompareTo(y) >= 0);
            }
            c = y as IComparable;
            if (c != null && xType == yType) {
                return Ops.Bool2Object(-1 * c.CompareTo(x) >= 0);
            }

            DynamicType dt1 = GetDynamicType(x);
            DynamicType dt2 = GetDynamicType(y);
            if ((ret = dt1.GreaterThanOrEqual(x, y)) != NotImplemented) return ret;
            if ((ret = dt2.LessThanOrEqual(y, x)) != NotImplemented) return ret;
            if ((ret = dt1.CompareTo(x, y)) != Ops.NotImplemented) return Ops.Bool2Object(Ops.CompareToZero(ret) >= 0);
            if ((ret = dt2.CompareTo(y, x)) != Ops.NotImplemented) return Ops.Bool2Object((-1 * Ops.CompareToZero(ret)) >= 0);

            if (xType == yType) {
                return Ops.Bool2Object((IdDispenser.GetId(x) - IdDispenser.GetId(y)) >= 0);
            } else {
                string xName = (xType == null) ? "!NoneType" : xType.Name, yName = (yType == null) ? "!NoneType" : yType.Name;

                return Ops.Bool2Object(string.CompareOrdinal(xName, yName) >= 0);
            }
        }

        public static bool GreaterThanOrEqualRetBool(object x, object y) {
            if (x is int) {
                if (y is int) {
                    return (((int)x) >= ((int)y));
                } else if (y is double) {
                    return (((int)x) >= ((double)y));
                } else if (y == null) {
                    return (1 >= 0);
                }
            } else if (x is double) {
                if (y is int) {
                    return (((double)x) >= ((int)y));
                } else if (y is double) {
                    return (((double)x) >= ((double)y));
                } else if (y is ExtensibleFloat) {
                    return (((double)x) >= ((ExtensibleFloat)y).value);
                } else if (y == null) {
                    return (1 >= 0);
                } else {
                    BigInteger bi = y as BigInteger;
                    if (!Object.ReferenceEquals(bi, null)) {
                        BigInteger self = BigInteger.Create((double)x);
                        double dblSelf = (double)x;
                        if (self == bi) {
                            double mod = dblSelf % 1;
                            if (mod != 0) {
                                if (dblSelf > 0)
                                    return (1 >= 0);
                                return (0 >= 1);
                            }
                        }

                        return (self >= bi);
                    }
                }
            } else if (x is bool) {
                if (y is bool) {
                    return ((((bool)x) ? 1 : 0) >= (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return (1 >= 0);
                }
            } else if (x is BigInteger) {
                if (y is BigInteger) {
                    return (((BigInteger)x) >= ((BigInteger)y));
                } else if (y is bool) {
                    return (((BigInteger)x) >= (((bool)y) ? 1 : 0));
                } else if (y == null) {
                    return (1 >= 0);
                } else if (y is double) {
                    double dbl = (double)y;
                    return ((((int)FloatOps.Compare(dbl, x)) * -1) >= 0);
                }
            } else if (x is short) {
                object res = IntOps.Compare((int)(short)x, y);
                if (res != Ops.NotImplemented) return (((int)res) >= 0);
            } else if (x is ushort) {
                object res = IntOps.Compare((int)(ushort)x, y);
                if (res != Ops.NotImplemented) return (((int)res) >= 0);
            } else if (x is byte) {
                object res = IntOps.Compare((int)(byte)x, y);
                if (res != Ops.NotImplemented) return (((int)res) >= 0);
            } else if (x is sbyte) {
                object res = IntOps.Compare((int)(sbyte)x, y);
                if (res != Ops.NotImplemented) return (((int)res) >= 0);
            } else if (x is ulong) {
                object res = Int64Ops.Compare((ulong)x, y);
                if (res != Ops.NotImplemented) return (((int)res) >= 0);
            } else if (x is uint) {
                object res = Int64Ops.Compare((long)(uint)x, y);
                if (res != Ops.NotImplemented) return (((int)res) >= 0);
            } else if (x is decimal) {
                object res = FloatOps.Compare((double)(decimal)x, y);
                if (res != Ops.NotImplemented) return (((int)res) >= 0);
            } else if (x == null) {
                if (y == null) return (true);

                if (y.GetType().IsPrimitive || y is BigInteger) {
                    // built-in type that doesn't implement our comparable
                    // interfaces, being compared against null, go ahead
                    // and skip the rest of the checks.
                    return (0 >= 1);
                }
            }

            if (x is string && y is string) {
                return (string.CompareOrdinal((string)x, (string)y) >= 0);
            }

            object ret;
            IRichComparable pc1 = x as IRichComparable;
            IRichComparable pc2 = y as IRichComparable;
            if (pc1 != null)
                if ((ret = pc1.GreaterThanOrEqual(y)) != Ops.NotImplemented) return Ops.IsTrue(ret);
            if (pc2 != null)
                if ((ret = pc2.LessThanOrEqual(x)) != Ops.NotImplemented) return Ops.IsTrue(ret);
            if (pc1 != null)
                if ((ret = pc1.CompareTo(y)) != Ops.NotImplemented) return (Ops.CompareToZero(ret) >= 0);
            if (pc2 != null)
                if ((ret = pc2.CompareTo(x)) != Ops.NotImplemented) return ((-1 * Ops.CompareToZero(ret)) >= 0);

            Type xType = (x == null) ? null : x.GetType(), yType = (y == null) ? null : y.GetType();

            IComparable c = x as IComparable;
            if (c != null && xType == yType) {
                return (c.CompareTo(y) >= 0);
            }
            c = y as IComparable;
            if (c != null && xType == yType) {
                return (-1 * c.CompareTo(x) >= 0);
            }

            DynamicType dt1 = GetDynamicType(x);
            DynamicType dt2 = GetDynamicType(y);
            if ((ret = dt1.GreaterThanOrEqual(x, y)) != NotImplemented) return Ops.IsTrue(ret);
            if ((ret = dt2.LessThanOrEqual(y, x)) != NotImplemented) return Ops.IsTrue(ret);
            if ((ret = dt1.CompareTo(x, y)) != Ops.NotImplemented) return (Ops.CompareToZero(ret) >= 0);
            if ((ret = dt2.CompareTo(y, x)) != Ops.NotImplemented) return ((-1 * Ops.CompareToZero(ret)) >= 0);

            if (xType == yType) {
                return ((IdDispenser.GetId(x) - IdDispenser.GetId(y)) >= 0);
            } else {
                string xName = (xType == null) ? "!NoneType" : xType.Name, yName = (yType == null) ? "!NoneType" : yType.Name;

                return (string.CompareOrdinal(xName, yName) >= 0);
            }
        }

        // *** END GENERATED CODE ***

        #endregion

        #region Generated Exception Factories

        // *** BEGIN GENERATED CODE ***


        public static Exception ImportError(string format, params object[] args) {
            return new PythonImportErrorException(string.Format(format, args));
        }

        public static Exception RuntimeError(string format, params object[] args) {
            return new PythonRuntimeErrorException(string.Format(format, args));
        }

        public static Exception UnicodeTranslateError(string format, params object[] args) {
            return new PythonUnicodeTranslateErrorException(string.Format(format, args));
        }

        public static Exception PendingDeprecationWarning(string format, params object[] args) {
            return new PythonPendingDeprecationWarningException(string.Format(format, args));
        }

        public static Exception EnvironmentError(string format, params object[] args) {
            return new PythonEnvironmentErrorException(string.Format(format, args));
        }

        public static Exception LookupError(string format, params object[] args) {
            return new PythonLookupErrorException(string.Format(format, args));
        }

        public static Exception OSError(string format, params object[] args) {
            return new PythonOSErrorException(string.Format(format, args));
        }

        public static Exception DeprecationWarning(string format, params object[] args) {
            return new PythonDeprecationWarningException(string.Format(format, args));
        }

        public static Exception UnicodeError(string format, params object[] args) {
            return new PythonUnicodeErrorException(string.Format(format, args));
        }

        public static Exception FloatingPointError(string format, params object[] args) {
            return new PythonFloatingPointErrorException(string.Format(format, args));
        }

        public static Exception ReferenceError(string format, params object[] args) {
            return new PythonReferenceErrorException(string.Format(format, args));
        }

        public static Exception NameError(string format, params object[] args) {
            return new PythonNameErrorException(string.Format(format, args));
        }

        public static Exception OverflowWarning(string format, params object[] args) {
            return new PythonOverflowWarningException(string.Format(format, args));
        }

        public static Exception FutureWarning(string format, params object[] args) {
            return new PythonFutureWarningException(string.Format(format, args));
        }

        public static Exception AssertionError(string format, params object[] args) {
            return new PythonAssertionErrorException(string.Format(format, args));
        }

        public static Exception ImportWarning(string format, params object[] args) {
            return new PythonImportWarningException(string.Format(format, args));
        }

        public static Exception RuntimeWarning(string format, params object[] args) {
            return new PythonRuntimeWarningException(string.Format(format, args));
        }

        public static Exception KeyboardInterrupt(string format, params object[] args) {
            return new PythonKeyboardInterruptException(string.Format(format, args));
        }

        public static Exception UserWarning(string format, params object[] args) {
            return new PythonUserWarningException(string.Format(format, args));
        }

        public static Exception SyntaxWarning(string format, params object[] args) {
            return new PythonSyntaxWarningException(string.Format(format, args));
        }

        public static Exception UnboundLocalError(string format, params object[] args) {
            return new PythonUnboundLocalErrorException(string.Format(format, args));
        }

        public static Exception Warning(string format, params object[] args) {
            return new PythonWarningException(string.Format(format, args));
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
