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
using IronPython.Runtime;
using System.Collections.Generic;

[assembly: PythonModule("operator", typeof(IronPython.Modules.PythonOperator))]
namespace IronPython.Modules {

    public class AttributeGetter {
        private readonly object name;
        public AttributeGetter(object name) {
            this.name = name;
        }

        public override string ToString() {
            return String.Format("<operator.attrgetter: {0}>", name == null ? "None" : name);
        }

        [PythonName("__call__")]
        public object Call(object param) {
            string s = name as string;
            if (s == null) {
                throw Ops.TypeError("attribute name must be string");
            }
            return Ops.GetAttr(DefaultContext.Default, param, SymbolTable.StringToId(s));
        }
    }

    public class ItemGetter {
        private readonly object item;
        public ItemGetter(object item) {
            this.item = item;
        }

        public override string ToString() {
            return String.Format("<operator.itemgetter: {0}>", item == null ? "None" : item);
        }

        [PythonName("__call__")]
        public object Call(object param) {
            try {
                return Ops.GetIndex(param, item);
            } catch (IndexOutOfRangeException) {
                throw;
            } catch (KeyNotFoundException) {
                throw;
            } catch {
                throw Ops.TypeError("invalid parameter for itemgetter");
            }
        }
    }

    public static class PythonOperator {
        [PythonName("lt")]
        public static object LessThan(object a, object b) {
            return Ops.LessThan(a, b);
        }
        [PythonName("le")]
        public static object LessThanOrEqual(object a, object b) {
            return Ops.LessThanOrEqual(a, b);
        }
        [PythonName("eq")]
        public static object Equal(object a, object b) {
            return Ops.Equal(a, b);
        }
        [PythonName("ne")]
        public static object NotEqual(object a, object b) {
            return Ops.NotEqual(a, b);
        }
        [PythonName("ge")]
        public static object GreaterThanOrEqual(object a, object b) {
            return Ops.GreaterThanOrEqual(a, b);
        }
        [PythonName("gt")]
        public static object GreaterThan(object a, object b) {
            return Ops.GreaterThan(a, b);
        }
        [PythonName("__lt__")]
        public static object OperatorLessThan(object a, object b) {
            return Ops.LessThan(a, b);
        }
        [PythonName("__le__")]
        public static object OperatorLessThanOrEqual(object a, object b) {
            return Ops.LessThanOrEqual(a, b);
        }
        [PythonName("__eq__")]
        public static object OperatorEqual(object a, object b) {
            return Ops.Equal(a, b);
        }
        [PythonName("__ne__")]
        public static object OperatorNotEqual(object a, object b) {
            return Ops.NotEqual(a, b);
        }
        [PythonName("__ge__")]
        public static object OperatorGreaterThanOrEqual(object a, object b) {
            return Ops.GreaterThanOrEqual(a, b);
        }
        [PythonName("__gt__")]
        public static object OperatorGreaterThan(object a, object b) {
            return Ops.GreaterThan(a, b);
        }
        [PythonName("not_")]
        public static object OperatorNot(object o) {
            return Ops.Not(o);
        }
        [PythonName("not__")]
        public static object OperatorNotDoubleUnderscore(object o) {
            return Ops.Not(o);
        }
        [PythonName("truth")]
        public static object Truth(object o) {
            return Ops.Bool2Object(Ops.IsTrue(o));
        }
        
        [PythonName("is_")]
        public static object Is(object a, object b) {
            return Ops.Is(a, b);
        }
        [PythonName("is_not")]
        public static object IsNot(object a, object b) {
            return Ops.IsNot(a, b);
        }
        [PythonName("abs")]
        public static object Abs(object o) {
            return Builtin.Abs(o);
        }
        [PythonName("__abs__")]
        public static object OperatorAbs(object o) {
            return Builtin.Abs(o);
        }
        [PythonName("add")]
        public static object Add(object a, object b) {
            return Ops.Add(a, b);
        }
        [PythonName("__add__")]
        public static object OperatorAdd(object a, object b) {
            return Ops.Add(a, b);
        }
        [PythonName("and_")]
        public static object And(object a, object b) {
            return Ops.BitwiseAnd(a, b);
        }
        [PythonName("__and__")]
        public static object OperatorAnd(object a, object b) {
            return Ops.BitwiseAnd(a, b);
        }
        [PythonName("div")]
        public static object Div(object a, object b) {
            return Ops.Divide(a, b);
        }
        [PythonName("__div__")]
        public static object OperatorDiv(object a, object b) {
            return Ops.Divide(a, b);
        }
        [PythonName("floordiv")]
        public static object FloorDiv(object a, object b) {
            return Ops.FloorDivide(a, b);
        }
        [PythonName("__floordiv__")]
        public static object OperatorFloorDiv(object a, object b) {
            return Ops.FloorDivide(a, b);
        }
        [PythonName("inv")]
        public static object Inv(object o) {
            return Ops.OnesComplement(o);
        }
        [PythonName("invert")]
        public static object Invert(object o) {
            return Ops.OnesComplement(o);
        }
        [PythonName("__inv__")]
        public static object OperatorInv(object o) {
            return Ops.OnesComplement(o);
        }
        [PythonName("__invert__")]
        public static object OperatorInvert(object o) {
            return Ops.OnesComplement(o);
        }
        [PythonName("lshift")]
        public static object LeftShift(object a, object b) {
            return Ops.LeftShift(a, b);
        }
        [PythonName("__lshift__")]
        public static object OperatorLeftShift(object a, object b) {
            return Ops.LeftShift(a, b);
        }
        [PythonName("mod")]
        public static object Mod(object a, object b) {
            return Ops.Mod(a, b);
        }
        [PythonName("__mod__")]
        public static object OperatorMod(object a, object b) {
            return Ops.Mod(a, b);
        }
        [PythonName("mul")]
        public static object Multiply(object a, object b) {
            return Ops.Multiply(a, b);
        }
        [PythonName("__mul__")]
        public static object OperatorMultiply(object a, object b) {
            return Ops.Multiply(a, b);
        }
        [PythonName("neg")]
        public static object Negate(object o) {
            return Ops.Negate(o);
        }
        [PythonName("__neg__")]
        public static object OperatorNegate(object o) {
            return Ops.Negate(o);
        }
        [PythonName("or_")]
        public static object Or(object a, object b) {
            return Ops.BitwiseOr(a, b);
        }
        [PythonName("__or__")]
        public static object OperatorOr(object a, object b) {
            return Ops.BitwiseOr(a, b);
        }
        [PythonName("pos")]
        public static object Plus(object o) {
            return Ops.Plus(o);
        }
        [PythonName("__pos__")]
        public static object OperatorPlus(object o) {
            return Ops.Plus(o);
        }
        [PythonName("pow")]
        public static object Power(object a, object b) {
            return Ops.Power(a, b);
        }
        [PythonName("__pow__")]
        public static object OperatorPower(object a, object b) {
            return Ops.Power(a, b);
        }
        [PythonName("rshift")]
        public static object RightShift(object a, object b) {
            return Ops.RightShift(a, b);
        }
        [PythonName("__rshift__")]
        public static object OperatorRightShift(object a, object b) {
            return Ops.RightShift(a, b);
        }
        [PythonName("sub")]
        public static object Subtract(object a, object b) {
            return Ops.Subtract(a, b);
        }
        [PythonName("__sub__")]
        public static object OperatorSubtract(object a, object b) {
            return Ops.Subtract(a, b);
        }
        [PythonName("truediv")]
        public static object TrueDivide(object a, object b) {
            if (a is int) {
                return IntOps.TrueDivide((int)a, b);
            } else if (a is long) {
                return Int64Ops.TrueDivide((long)a, b);
            } else if (a is IronMath.BigInteger) {
                return LongOps.TrueDivide((IronMath.BigInteger)a, b);
            }
            return Ops.Divide(a, b);
        }
        [PythonName("__truediv__")]
        public static object OperatorTrueDivide(object a, object b) {
            return Ops.Divide(a, b);
        }
        [PythonName("xor")]
        public static object Xor(object a, object b) {
            return Ops.Xor(a, b);
        }
        [PythonName("__xor__")]
        public static object OperatorXor(object a, object b) {
            return Ops.Xor(a, b);
        }
        [PythonName("concat")]
        public static object Concat(object a, object b) {
            return Ops.Add(a, b);
        }
        [PythonName("__concat__")]
        public static object OperatorConcat(object a, object b) {
            return Ops.Add(a, b);
        }
        [PythonName("contains")]
        public static object Contains(object a, object b) {
            return Ops.In(b, a);
        }
        [PythonName("__contains__")]
        public static object OperatorContains(object a, object b) {
            return Ops.In(a, b);
        }

        [PythonName("countOf")]
        public static object CountOf(object a, object b) {
            System.Collections.IEnumerator e = Ops.GetEnumerator(a);
            int count = 0;
            while (e.MoveNext()) {
                if (Ops.Equals(e.Current, b)) {
                    count++;
                }
            }
            return count;
        }
        [PythonName("delitem")]
        public static void DelIndex(object a, object b) {
            Ops.DelIndex(a, b);
        }
        [PythonName("__delitem__")]
        public static void OperatorDelItem(object a, object b) {
            Ops.DelIndex(a, b);
        }
        [PythonName("delslice")]
        public static void DelIndex(object a, object b, object c) {
            Ops.DelIndex(a, MakeSlice(b, c));
        }

        [PythonName("__delslice__")]
        public static void OperatorDelIndex(object a, object b, object c) {
            Ops.DelIndex(a, MakeSlice(b, c));
        }
        [PythonName("getitem")]
        public static object GetItem(object a, object b) {
            return Ops.GetIndex(a, b);
        }
        [PythonName("__getitem__")]
        public static object OperatorGetItem(object a, object b) {
            return Ops.GetIndex(a, b);
        }
        [PythonName("getslice")]
        public static object GetSlice(object a, object b, object c) {
            return Ops.GetIndex(a, MakeSlice(b, c));
        }

        [PythonName("__getslice__")]
        public static object OperatorGetSlice(object a, object b, object c) {
            return Ops.GetIndex(a, MakeSlice(b, c));
        }

        [PythonName("indexOf")]
        public static object IndexOf(object a, object b) {
            System.Collections.IEnumerator e = Ops.GetEnumerator(a);
            int index = 0;
            while (e.MoveNext()) {
                if (Ops.Equals(e.Current, b)) {
                    return index;
                }
                index++;
            }
            throw Ops.ValueError("object not in sequence");
        }

        [PythonName("repeat")]
        public static object Repeat(object a, object b) {
            try {
                Ops.GetEnumerator(a);
            } catch {
                throw Ops.TypeError("object can't be repeated");
            }
            try {
                IntOps.Make(b);
            } catch {
                throw Ops.TypeError("integer required");
            }
            return Ops.Multiply(a, b);
        }

        [PythonName("__repeat__")]
        public static object OperatorRepeat(object a, object b) {
            return Repeat(a, b);
        }
        [PythonName("sequenceIncludes")]
        public static object SequenceIncludes(object a, object b) {
            return Contains(a, b);
        }

        [PythonName("setitem")]
        public static void SetIndex(object a, object b, object c) {
            Ops.SetIndex(a, b, c);
        }
        public static void OperatorSetIndex(object a, object b, object c) {
            Ops.SetIndex(a, b, c);
        }
        [PythonName("setslice")]
        public static void SetIndex(object a, object b, object c, object v) {
            Ops.SetIndex(a, MakeSlice(b, c), v);
        }

        [PythonName("__setslice__")]
        public static void OperatorSetSlice(object a, object b, object c, object v) {
            Ops.SetIndex(a, MakeSlice(b, c), v);
        }
        [PythonName("isCallable")]
        public static object IsCallable(object o) {
            return Builtin.Callable(o);
        }

        [PythonName("isMappingType")]
        public static object IsMappingType(ICallerContext context, object o) {
            if (o is IMapping || o is Dict || o is IDictionary<object, object>) {
                if ((context.ContextFlags & CallerContextFlags.ShowCls) == 0) {
                    // in standard Python methods aren't mapping types, therefore
                    // if the user hasn't broken out of that box yet don't treat 
                    // them as mapping types.
                    if (o is ReflectedMethod) return Ops.FALSE;
                }
                return Ops.TRUE;
            }
            object getitem;
            if (Ops.TryGetAttr(o, SymbolTable.GetItem, out getitem)) {
                return Ops.TRUE;
            }
            return Ops.FALSE;
        }

        [PythonName("isNumberType")]
        public static object IsNumberType(object o) {
            return Ops.Bool2Object(
                o is int ||
                o is long ||
                o is double ||
                o is float ||
                o is short ||
                o is uint ||
                o is ulong ||
                o is ushort ||
                o is decimal ||
                o is IronMath.BigInteger ||
                o is IronMath.Complex64 ||
                o is byte);
        }

        [PythonName("isSequenceType")]
        public static object IsSequenceType(object o) {
            return Ops.Bool2Object(
                   o is System.Collections.ICollection ||
                   o is System.Collections.IEnumerable ||
                   o is System.Collections.IEnumerator ||
                   o is System.Collections.IList);
        }

        [PythonName("attrgetter")]
        public static object AttrGetter(object attr) {
            return new AttributeGetter(attr);
        }

        [PythonName("itemgetter")]
        public static object ItemGetter(object item) {
            return new ItemGetter(item);
        }

        private static int SliceToInt(object o) {
            Conversion c;
            int i = Converter.TryConvertToInt32(o, out c);
            if (c == Conversion.None) {
                throw Ops.TypeError("integer expected");
            }
            return i;
        }
        private static object MakeSlice(object a, object b) {
            return Ops.MakeSlice(SliceToInt(a), SliceToInt(b), null);
        }
    }
}