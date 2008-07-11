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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Runtime;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Math;

[assembly: PythonModule("operator", typeof(IronPython.Modules.PythonOperator))]
namespace IronPython.Modules {
    public static class PythonOperator {
        private static DynamicSite<object, object, bool> _InSite;

        public class attrgetter {
            private readonly object[] _names;
            public attrgetter(params object[] attrs) {
                if (attrs.Length == 0) throw PythonOps.TypeError("attrgetter expected 1 arguments, got 0");

                this._names = attrs;
            }

            [SpecialName]
            public object Call(CodeContext context, object param) {
                if (_names.Length == 1) {
                    return GetOneAttr(context, param, _names[0]);
                }

                object[] res = new object[_names.Length];
                for (int i = 0; i < _names.Length; i++) {
                    res[i] = GetOneAttr(context, param, _names[i]);
                }
                return PythonTuple.MakeTuple(res);
            }

            private static object GetOneAttr(CodeContext context, object param, object val) {
                string s = val as string;
                if (s == null) {
                    throw PythonOps.TypeError("attribute name must be string");
                }
                return PythonOps.GetBoundAttr(context, param, SymbolTable.StringToId(s));
            }
        }

        public class itemgetter {
            private readonly object _item;
            public itemgetter(object item) {
                this._item = item;
            }

            public override string ToString() {
                return String.Format("<operator.itemgetter: {0}>", _item == null ? "None" : _item);
            }

            [SpecialName]
            public object Call(object param) {
                try {
                    return PythonOps.GetIndex(param, _item);
                } catch (IndexOutOfRangeException) {
                    throw;
                } catch (KeyNotFoundException) {
                    throw;
                } catch {
                    throw PythonOps.TypeError("invalid parameter for itemgetter");
                }
            }
        }

        public static object lt(object a, object b) {
            return PythonSites.LessThan(a, b);
        }

        public static object le(object a, object b) {
            return PythonSites.LessThanOrEqual(a, b);
        }

        public static object eq(object a, object b) {
            return PythonOps.Equal(a, b);
        }

        public static object ne(object a, object b) {
            return PythonSites.NotEquals(a, b);
        }

        public static object ge(object a, object b) {
            return PythonSites.GreaterThanOrEqual(a, b);
        }

        public static object gt(object a, object b) {
            return PythonSites.GreaterThan(a, b);
        }

        public static object __lt__(object a, object b) {
            return PythonSites.LessThan(a, b);
        }

        public static object __le__(object a, object b) {
            return PythonSites.LessThanOrEqual(a, b);
        }

        public static object __eq__(object a, object b) {
            return PythonOps.Equal(a, b);
        }

        public static object __ne__(object a, object b) {
            return PythonSites.NotEquals(a, b);
        }

        public static object __ge__(object a, object b) {
            return PythonSites.GreaterThanOrEqual(a, b);
        }

        public static object __gt__(object a, object b) {
            return PythonSites.GreaterThan(a, b);
        }

        public static object not_(object o) {
            return PythonOps.Not(o);
        }

        public static object __not__(object o) {
            return PythonOps.Not(o);
        }

        public static bool truth(object o) {
            return PythonOps.IsTrue(o);
        }

        public static object is_(object a, object b) {
            return PythonOps.Is(a, b);
        }

        public static object is_not(object a, object b) {
            return PythonOps.IsNot(a, b);
        }

        public static object abs(CodeContext context, object o) {
            return Builtin.abs(context, o);
        }

        public static object __abs__(CodeContext context, object o) {
            return Builtin.abs(context, o);
        }

        public static object add(object a, object b) {
            return PythonSites.Add(a, b);
        }

        public static object __add__(object a, object b) {
            return PythonSites.Add(a, b);
        }

        public static object and_(object a, object b) {
            return PythonSites.BitwiseAnd(a, b);
        }

        public static object __and__(object a, object b) {
            return PythonSites.BitwiseAnd(a, b);
        }

        public static object div(object a, object b) {
            return PythonSites.Divide(a, b);
        }

        public static object __div__(object a, object b) {
            return PythonSites.Divide(a, b);
        }

        public static object floordiv(object a, object b) {
            return PythonSites.FloorDivide(a, b);
        }

        public static object __floordiv__(object a, object b) {
            return PythonSites.FloorDivide(a, b);
        }

        public static object inv(object o) {
            return PythonOps.OnesComplement(o);
        }

        public static object invert(object o) {
            return PythonOps.OnesComplement(o);
        }

        public static object __inv__(object o) {
            return PythonOps.OnesComplement(o);
        }

        public static object __invert__(object o) {
            return PythonOps.OnesComplement(o);
        }

        public static object lshift(object a, object b) {
            return PythonSites.LeftShift(a, b);
        }

        public static object __lshift__(object a, object b) {
            return PythonSites.LeftShift(a, b);
        }

        public static object mod(object a, object b) {
            return PythonSites.Mod(a, b);
        }

        public static object __mod__(object a, object b) {
            return PythonSites.Mod(a, b);
        }

        public static object mul(object a, object b) {
            return PythonSites.Multiply(a, b);
        }

        public static object __mul__(object a, object b) {
            return PythonSites.Multiply(a, b);
        }

        public static object neg(object o) {
            return PythonOps.Negate(o);
        }

        public static object __neg__(object o) {
            return PythonOps.Negate(o);
        }

        public static object or_(object a, object b) {
            return PythonSites.BitwiseOr(a, b);
        }

        public static object __or__(object a, object b) {
            return PythonSites.BitwiseOr(a, b);
        }

        public static object pos(object o) {
            return PythonOps.Plus(o);
        }

        public static object __pos__(object o) {
            return PythonOps.Plus(o);
        }

        public static object pow(object a, object b) {
            return PythonSites.Power(a, b);
        }

        public static object __pow__(object a, object b) {
            return PythonSites.Power(a, b);
        }

        public static object rshift(object a, object b) {
            return PythonSites.RightShift(a, b);
        }

        public static object __rshift__(object a, object b) {
            return PythonSites.RightShift(a, b);
        }

        public static object sub(object a, object b) {
            return PythonSites.Subtract(a, b);
        }

        public static object __sub__(object a, object b) {
            return PythonSites.Subtract(a, b);
        }

        public static object truediv(object a, object b) {
            return PythonSites.TrueDivide(a, b);
        }

        public static object __truediv__(object a, object b) {
            return PythonSites.TrueDivide(a, b);
        }

        public static object xor(object a, object b) {
            return PythonSites.ExclusiveOr(a, b);
        }

        public static object __xor__(object a, object b) {
            return PythonSites.ExclusiveOr(a, b);
        }

        public static object concat(object a, object b) {
            TestBothSequence(a, b);

            return PythonSites.Add(a, b);
        }

        public static object __concat__(object a, object b) {
            TestBothSequence(a, b);

            return PythonSites.Add(a, b);
        }

        public static bool contains(object a, object b) {
            if (!_InSite.IsInitialized) {
                _InSite.EnsureInitialized(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.Contains));
            }

            return _InSite.Invoke(DefaultContext.Default, b, a);
        }

        public static bool __contains__(object a, object b) {
            return contains(a, b);
        }

        public static int countOf(object a, object b) {
            System.Collections.IEnumerator e = PythonOps.GetEnumerator(a);
            int count = 0;
            while (e.MoveNext()) {
                if (PythonOps.Equals(e.Current, b)) {
                    count++;
                }
            }
            return count;
        }

        public static void delitem(object a, object b) {
            PythonOps.DelIndex(a, b);
        }

        public static void __delitem__(object a, object b) {
            PythonOps.DelIndex(a, b);
        }

        public static void delslice(object a, object b, object c) {
            PythonOps.DelIndex(a, MakeSlice(b, c));
        }

        public static void __delslice__(object a, object b, object c) {
            PythonOps.DelIndex(a, MakeSlice(b, c));
        }

        public static object getitem(object a, object b) {
            return PythonOps.GetIndex(a, b);
        }

        public static object __getitem__(object a, object b) {
            return PythonOps.GetIndex(a, b);
        }

        public static object getslice(object a, object b, object c) {
            return PythonOps.GetIndex(a, MakeSlice(b, c));
        }

        public static object __getslice__(object a, object b, object c) {
            return PythonOps.GetIndex(a, MakeSlice(b, c));
        }

        public static int indexOf(object a, object b) {
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

        public static object repeat(CodeContext context, object a, object b) {
            try {
                PythonOps.GetEnumerator(a);
            } catch {
                throw PythonOps.TypeError("object can't be repeated");
            }
            try {
                Int32Ops.__new__(context, b);
            } catch {
                throw PythonOps.TypeError("integer required");
            }
            return PythonSites.Multiply(a, b);
        }

        public static object __repeat__(CodeContext context, object a, object b) {
            return repeat(context, a, b);
        }

        public static object sequenceIncludes(object a, object b) {
            return contains(a, b);
        }

        public static void setitem(object a, object b, object c) {
            PythonOps.SetIndex(a, b, c);
        }

        public static void __setitem__(object a, object b, object c) {
            PythonOps.SetIndex(a, b, c);
        }

        public static void setslice(object a, object b, object c, object v) {
            PythonOps.SetIndex(a, MakeSlice(b, c), v);
        }

        public static void __setslice__(object a, object b, object c, object v) {
            PythonOps.SetIndex(a, MakeSlice(b, c), v);
        }

        public static bool isCallable(object o) {
            return PythonOps.IsCallable(o);
        }

        public static object isMappingType(CodeContext context, object o) {
            return PythonOps.IsMappingType(context, o);
        }

        public static bool isNumberType(object o) {
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

        public static bool isSequenceType(object o) {
            return
                   o is System.Collections.ICollection ||
                   o is System.Collections.IEnumerable ||
                   o is System.Collections.IEnumerator ||
                   o is System.Collections.IList ||
                   PythonOps.HasAttr(DefaultContext.Default, o, Symbols.GetItem);
        }

        private static int SliceToInt(object o) {
            int i;
            if (Converter.TryConvertToInt32(o, out i)) return i;
            throw PythonOps.TypeError("integer expected");
        }

        private static object MakeSlice(object a, object b) {
            return new Slice(SliceToInt(a), SliceToInt(b), null);
        }

        private static readonly DynamicSite<object, object, object> _iadd =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlaceAdd));

        private static readonly DynamicSite<object, object, object> _iand =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlaceBitwiseAnd));

        private static readonly DynamicSite<object, object, object> _idiv =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlaceDivide));

        private static readonly DynamicSite<object, object, object> _ilshift =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlaceLeftShift));

        private static readonly DynamicSite<object, object, object> _imod =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlaceMod));

        private static readonly DynamicSite<object, object, object> _imul =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlaceMultiply));

        private static readonly DynamicSite<object, object, object> _ior =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlaceBitwiseOr));

        private static readonly DynamicSite<object, object, object> _ipow =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlacePower));

        private static readonly DynamicSite<object, object, object> _irshift =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlaceRightShift));

        private static readonly DynamicSite<object, object, object> _isub =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlaceSubtract));

        private static readonly DynamicSite<object, object, object> _itruediv =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlaceTrueDivide));

        private static readonly DynamicSite<object, object, object> _ifloordiv =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlaceFloorDivide));

        private static readonly DynamicSite<object, object, object> _ixor =
            DynamicSite<object, object, object>.Create(OldDoOperationAction.Make(DefaultContext.DefaultPythonBinder, Operators.InPlaceExclusiveOr));

        public static object iadd(object a, object b) {
            return _iadd.Invoke(DefaultContext.Default, a, b);
        }

        public static object iand(object a, object b) {
            return _iand.Invoke(DefaultContext.Default, a, b);
        }

        public static object idiv(object a, object b) {
            return _idiv.Invoke(DefaultContext.Default, a, b);
        }

        public static object ifloordiv(object a, object b) {
            return _ifloordiv.Invoke(DefaultContext.Default, a, b);
        }

        public static object ilshift(object a, object b) {
            return _ilshift.Invoke(DefaultContext.Default, a, b);
        }

        public static object imod(object a, object b) {
            return _imod.Invoke(DefaultContext.Default, a, b);
        }

        public static object imul(object a, object b) {
            return _imul.Invoke(DefaultContext.Default, a, b);
        }

        public static object ior(object a, object b) {
            return _ior.Invoke(DefaultContext.Default, a, b);
        }

        public static object ipow(object a, object b) {
            return _ipow.Invoke(DefaultContext.Default, a, b);
        }

        public static object irshift(object a, object b) {
            return _irshift.Invoke(DefaultContext.Default, a, b);
        }

        public static object isub(object a, object b) {
            return _isub.Invoke(DefaultContext.Default, a, b);
        }

        public static object itruediv(object a, object b) {
            return _itruediv.Invoke(DefaultContext.Default, a, b);
        }

        public static object ixor(object a, object b) {
            return _ixor.Invoke(DefaultContext.Default, a, b);
        }

        public static object iconcat(object a, object b) {
            TestBothSequence(a, b);

            return _iadd.Invoke(DefaultContext.Default, a, b);
        }

        public static object irepeat(object a, object b) {
            if (!isSequenceType(a)) {
                throw PythonOps.TypeError("'{0}' object cannot be repeated", PythonTypeOps.GetName(a));
            }

            try {
                Int32Ops.__new__(DefaultContext.Default, b);
            } catch {
                throw PythonOps.TypeError("integer required");
            }

            return _imul.Invoke(DefaultContext.Default, a, b);
        }

        public static object __iadd__(object a, object b) {
            return iadd(a, b);
        }

        public static object __iand__(object a, object b) {
            return iand(a, b);
        }

        public static object __idiv__(object a, object b) {
            return idiv(a, b);
        }

        public static object __ifloordiv__(object a, object b) {
            return ifloordiv(a, b);
        }

        public static object __ilshift__(object a, object b) {
            return ilshift(a, b);
        }

        public static object __imod__(object a, object b) {
            return imod(a, b);
        }

        public static object __imul__(object a, object b) {
            return imul(a, b);
        }

        public static object __ior__(object a, object b) {
            return ior(a, b);
        }

        public static object __ipow__(object a, object b) {
            return ipow(a, b);
        }

        public static object __irshift__(object a, object b) {
            return irshift(a, b);
        }

        public static object __isub__(object a, object b) {
            return isub(a, b);
        }

        public static object __itruediv__(object a, object b) {
            return itruediv(a, b);
        }

        public static object __ixor__(object a, object b) {
            return ixor(a, b);
        }

        public static object __iconcat__(object a, object b) {
            return iconcat(a, b);
        }

        public static object __irepeat__(object a, object b) {
            return irepeat(a, b);
        }

        public static object index(object a) {
            return __index__(a);
        }

        public static int __index__(object a) {
            return Converter.ConvertToIndex(a);
        }

        private static void TestBothSequence(object a, object b) {
            if (!isSequenceType(a)) {
                throw PythonOps.TypeError("'{0}' object cannot be concatenated", PythonTypeOps.GetName(a));
            } else if (!isSequenceType(b)) {
                throw PythonOps.TypeError("cannot concatenate '{0}' and '{1} objects", PythonTypeOps.GetName(a), PythonTypeOps.GetName(b));
            }
        }
    }
}
