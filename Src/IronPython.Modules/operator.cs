/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;


[assembly: PythonModule("operator", typeof(IronPython.Modules.PythonOperator))]
namespace IronPython.Modules {
    [PythonType("operator")]
    public static class PythonOperator {
        
        [PythonType("attrgetter")]
        public class AttributeGetter {
            private readonly object name;
            public AttributeGetter(object name) {
                this.name = name;
            }

            public override string ToString() {
                return String.Format("<operator.attrgetter: {0}>", name == null ? "None" : name);
            }

            [SpecialName]
            public object Call(CodeContext context, object param) {
                string s = name as string;
                if (s == null) {
                    throw PythonOps.TypeError("attribute name must be string");
                }
                return PythonOps.GetBoundAttr(context, param, SymbolTable.StringToId(s));
            }
        }

        [PythonType("itemgetter")]
        public class ItemGetter {
            private readonly object item;
            public ItemGetter(object item) {
                this.item = item;
            }

            public override string ToString() {
                return String.Format("<operator.itemgetter: {0}>", item == null ? "None" : item);
            }

            [SpecialName]
            public object Call(object param) {
                try {
                    return PythonOps.GetIndex(param, item);
                } catch (IndexOutOfRangeException) {
                    throw;
                } catch (KeyNotFoundException) {
                    throw;
                } catch {
                    throw PythonOps.TypeError("invalid parameter for itemgetter");
                }
            }
        }

        [PythonName("lt")]
        public static object LessThan(object a, object b) {
            return PythonSites.LessThan(a, b);
        }
        [PythonName("le")]
        public static object LessThanOrEqual(object a, object b) {
            return PythonSites.LessThanOrEqual(a, b);
        }
        [PythonName("eq")]
        public static object Equal(object a, object b) {
            return PythonOps.Equal(a, b);
        }
        [PythonName("ne")]
        public static object NotEqual(object a, object b) {
            return PythonSites.NotEquals(a, b);
        }
        [PythonName("ge")]
        public static object GreaterThanOrEqual(object a, object b) {
            return PythonSites.GreaterThanOrEqual(a, b);
        }
        [PythonName("gt")]
        public static object GreaterThan(object a, object b) {
            return PythonSites.GreaterThan(a, b);
        }
        [PythonName("__lt__")]
        public static object OperatorLessThan(object a, object b) {
            return PythonSites.LessThan(a, b);
        }
        [PythonName("__le__")]
        public static object OperatorLessThanOrEqual(object a, object b) {
            return PythonSites.LessThanOrEqual(a, b);
        }
        [PythonName("__eq__")]
        public static object OperatorEqual(object a, object b) {
            return PythonOps.Equal(a, b);
        }
        [PythonName("__ne__")]
        public static object OperatorNotEqual(object a, object b) {
            return PythonSites.NotEquals(a, b);
        }
        [PythonName("__ge__")]
        public static object OperatorGreaterThanOrEqual(object a, object b) {
            return PythonSites.GreaterThanOrEqual(a, b);
        }
        [PythonName("__gt__")]
        public static object OperatorGreaterThan(object a, object b) {
            return PythonSites.GreaterThan(a, b);
        }
        [PythonName("not_")]
        public static object OperatorNot(object o) {
            return PythonOps.Not(o);
        }
        [PythonName("__not__")]
        public static object OperatorNotDoubleUnderscore(object o) {
            return PythonOps.Not(o);
        }
        [PythonName("truth")]
        public static bool Truth(object o) {
            return PythonOps.IsTrue(o);
        }

        [PythonName("is_")]
        public static object Is(object a, object b) {
            return PythonOps.Is(a, b);
        }
        [PythonName("is_not")]
        public static object IsNot(object a, object b) {
            return PythonOps.IsNot(a, b);
        }
        [PythonName("abs")]
        public static object Abs(CodeContext context, object o) {
            return Builtin.abs(context, o);
        }
        [PythonName("__abs__")]
        public static object OperatorAbs(CodeContext context, object o) {
            return Builtin.abs(context, o);
        }
        [PythonName("add")]
        public static object Add(object a, object b) {
            return PythonSites.Add(a, b);
        }
        [PythonName("__add__")]
        public static object OperatorAdd(object a, object b) {
            return PythonSites.Add(a, b);
        }
        [PythonName("and_")]
        public static object And(object a, object b) {
            return PythonSites.BitwiseAnd(a, b);
        }
        [PythonName("__and__")]
        public static object OperatorAnd(object a, object b) {
            return PythonSites.BitwiseAnd(a, b);
        }
        [PythonName("div")]
        public static object Div(object a, object b) {
            return PythonSites.Divide(a, b);
        }
        [PythonName("__div__")]
        public static object OperatorDiv(object a, object b) {
            return PythonSites.Divide(a, b);
        }
        [PythonName("floordiv")]
        public static object FloorDiv(object a, object b) {
            return PythonSites.FloorDivide(a, b);
        }
        [PythonName("__floordiv__")]
        public static object OperatorFloorDiv(object a, object b) {
            return PythonSites.FloorDivide(a, b);
        }
        [PythonName("inv")]
        public static object Inv(object o) {
            return PythonOps.OnesComplement(o);
        }
        [PythonName("invert")]
        public static object Invert(object o) {
            return PythonOps.OnesComplement(o);
        }
        [PythonName("__inv__")]
        public static object OperatorInv(object o) {
            return PythonOps.OnesComplement(o);
        }
        [PythonName("__invert__")]
        public static object OperatorInvert(object o) {
            return PythonOps.OnesComplement(o);
        }
        [PythonName("lshift")]
        public static object LeftShift(object a, object b) {
            return PythonSites.LeftShift(a, b);
        }
        [PythonName("__lshift__")]
        public static object OperatorLeftShift(object a, object b) {
            return PythonSites.LeftShift(a, b);
        }
        [PythonName("mod")]
        public static object Mod(object a, object b) {
            return PythonSites.Mod(a, b);
        }
        [PythonName("__mod__")]
        public static object OperatorMod(object a, object b) {
            return PythonSites.Mod(a, b);
        }
        [PythonName("mul")]
        public static object Multiply(object a, object b) {
            return PythonSites.Multiply(a, b);
        }
        [PythonName("__mul__")]
        public static object OperatorMultiply(object a, object b) {
            return PythonSites.Multiply(a, b);
        }
        [PythonName("neg")]
        public static object Negate(object o) {
            return PythonOps.Negate(o);
        }
        [PythonName("__neg__")]
        public static object OperatorNegate(object o) {
            return PythonOps.Negate(o);
        }
        [PythonName("or_")]
        public static object Or(object a, object b) {
            return PythonSites.BitwiseOr(a, b);
        }
        [PythonName("__or__")]
        public static object OperatorOr(object a, object b) {
            return PythonSites.BitwiseOr(a, b);
        }
        [PythonName("pos")]
        public static object Plus(object o) {
            return PythonOps.Plus(o);
        }
        [PythonName("__pos__")]
        public static object OperatorPlus(object o) {
            return PythonOps.Plus(o);
        }
        [PythonName("pow")]
        public static object Power(object a, object b) {
            return PythonSites.Power(a, b);
        }
        [PythonName("__pow__")]
        public static object OperatorPower(object a, object b) {
            return PythonSites.Power(a, b);
        }
        [PythonName("rshift")]
        public static object RightShift(object a, object b) {
            return PythonSites.RightShift(a, b);
        }
        [PythonName("__rshift__")]
        public static object OperatorRightShift(object a, object b) {
            return PythonSites.RightShift(a, b);
        }
        [PythonName("sub")]
        public static object Subtract(object a, object b) {
            return PythonSites.Subtract(a, b);
        }
        [PythonName("__sub__")]
        public static object OperatorSubtract(object a, object b) {
            return PythonSites.Subtract(a, b);
        }
        [PythonName("truediv")]
        public static object TrueDivide(object a, object b) {
            return PythonSites.TrueDivide(a, b);
        }
        [PythonName("__truediv__")]
        public static object OperatorTrueDivide(object a, object b) {
            return PythonSites.TrueDivide(a, b);
        }
        [PythonName("xor")]
        public static object Xor(object a, object b) {
            return PythonSites.Xor(a, b);
        }
        [PythonName("__xor__")]
        public static object OperatorXor(object a, object b) {
            return PythonSites.Xor(a, b);
        }
        [PythonName("concat")]
        public static object Concat(object a, object b) {
            return PythonSites.Add(a, b);
        }
        [PythonName("__concat__")]
        public static object OperatorConcat(object a, object b) {
            return PythonSites.Add(a, b);
        }
        [PythonName("contains")]
        public static object Contains(object a, object b) {
            return PythonOps.In(b, a);
        }
        [PythonName("__contains__")]
        public static object OperatorContains(object a, object b) {
            return PythonOps.In(a, b);
        }

        [PythonName("countOf")]
        public static object CountOf(object a, object b) {
            System.Collections.IEnumerator e = PythonOps.GetEnumerator(a);
            int count = 0;
            while (e.MoveNext()) {
                if (PythonOps.Equals(e.Current, b)) {
                    count++;
                }
            }
            return count;
        }
        [PythonName("delitem")]
        public static void DelIndex(object a, object b) {
            PythonOps.DelIndex(a, b);
        }
        [PythonName("__delitem__")]
        public static void OperatorDelItem(object a, object b) {
            PythonOps.DelIndex(a, b);
        }
        [PythonName("delslice")]
        public static void DelIndex(object a, object b, object c) {
            PythonOps.DelIndex(a, MakeSlice(b, c));
        }

        [PythonName("__delslice__")]
        public static void OperatorDelIndex(object a, object b, object c) {
            PythonOps.DelIndex(a, MakeSlice(b, c));
        }
        [PythonName("getitem")]
        public static object GetItem(object a, object b) {
            return PythonOps.GetIndex(a, b);
        }
        [PythonName("__getitem__")]
        public static object OperatorGetItem(object a, object b) {
            return PythonOps.GetIndex(a, b);
        }
        [PythonName("getslice")]
        public static object GetSlice(object a, object b, object c) {
            return PythonOps.GetIndex(a, MakeSlice(b, c));
        }

        [PythonName("__getslice__")]
        public static object OperatorGetSlice(object a, object b, object c) {
            return PythonOps.GetIndex(a, MakeSlice(b, c));
        }

        [PythonName("indexOf")]
        public static object IndexOf(object a, object b) {
            System.Collections.IEnumerator e = PythonOps.GetEnumerator(a);
            int index = 0;
            while (e.MoveNext()) {
                if (PythonOps.Equals(e.Current, b)) {
                    return index;
                }
                index++;
            }
            throw PythonOps.ValueError("object not in sequence");
        }

        [PythonName("repeat")]
        public static object Repeat(CodeContext context, object a, object b) {
            try {
                PythonOps.GetEnumerator(a);
            } catch {
                throw PythonOps.TypeError("object can't be repeated");
            }
            try {
                Int32Ops.Make(context, b);
            } catch {
                throw PythonOps.TypeError("integer required");
            }
            return PythonSites.Multiply(a, b);
        }

        [PythonName("__repeat__")]
        public static object OperatorRepeat(CodeContext context, object a, object b) {
            return Repeat(context, a, b);
        }
        [PythonName("sequenceIncludes")]
        public static object SequenceIncludes(object a, object b) {
            return Contains(a, b);
        }

        [PythonName("setitem")]
        public static void SetIndex(object a, object b, object c) {
            PythonOps.SetIndex(a, b, c);
        }
        [PythonName("__setitem__")]
        public static void OperatorSetIndex(object a, object b, object c) {
            PythonOps.SetIndex(a, b, c);
        }
        [PythonName("setslice")]
        public static void SetIndex(object a, object b, object c, object v) {
            PythonOps.SetIndex(a, MakeSlice(b, c), v);
        }

        [PythonName("__setslice__")]
        public static void OperatorSetSlice(object a, object b, object c, object v) {
            PythonOps.SetIndex(a, MakeSlice(b, c), v);
        }
        [PythonName("isCallable")]
        public static bool IsCallable(object o) {
            return PythonOps.IsCallable(o);
        }

        [PythonName("isMappingType")]
        public static object IsMappingType(CodeContext context, object o) {
            return PythonOps.IsMappingType(context, o);
        }

        [PythonName("isNumberType")]
        public static bool IsNumberType(object o) {
            return o is int ||
                o is long ||
                o is double ||
                o is float ||
                o is short ||
                o is uint ||
                o is ulong ||
                o is ushort ||
                o is decimal ||
                o is BigInteger ||
                o is Complex64 ||
                o is byte;
        }

        [PythonName("isSequenceType")]
        public static bool IsSequenceType(object o) {
            return 
                   o is System.Collections.ICollection ||
                   o is System.Collections.IEnumerable ||
                   o is System.Collections.IEnumerator ||
                   o is System.Collections.IList;
        }

        private static int SliceToInt(object o) {
            int i;
            if (Converter.TryConvertToInt32(o, out i)) return i;
            throw PythonOps.TypeError("integer expected");
        }

        private static object MakeSlice(object a, object b) {
            return new Slice(SliceToInt(a), SliceToInt(b), null);
        }
    }
}