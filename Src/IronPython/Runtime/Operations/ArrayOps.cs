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
using System.Diagnostics;

using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Operations {
    [PythonType("array")]
    public static class ArrayOps {
        internal static object ArrayType;

        #region Python APIs

        [PythonName("__contains__")]
        public static bool Contains(object[] data, int size, object item) {
            for (int i = 0; i < size; i++) {
                if (Ops.EqualRetBool(data[i], item)) return true;
            }
            return false;
        }

        [PythonName("__add__")]
        public static Array Add(Array data1, Array data2) {
            if (data1.Rank > 1 || data2.Rank > 1) throw new NotImplementedException("can't add multidimensional arrays");

            Type type1 = data1.GetType();
            Type type2 = data2.GetType();
            Type type = (type1 == type2) ? type1.GetElementType() : typeof(object);

            Array ret = Array.CreateInstance(type, data1.Length + data2.Length);
            Array.Copy(data1, 0, ret, 0, data1.Length);
            Array.Copy(data2, 0, ret, data1.Length, data2.Length);
            return ret;
        }

        [PythonName("__call__")]
        public static object CreateArray(Type type, object items) {
            if (items is ISequence) {
                ISequence data = items as ISequence;
                Array res = Array.CreateInstance(type, data.GetLength());

                for (int i = 0; i < data.GetLength(); i++) {
                    res.SetValue(Ops.ConvertTo(data[i], type), i);
                }

                return res;
            } else {
                object lenFunc;
                if (!Ops.TryGetAttr(items, SymbolTable.Length, out lenFunc))
                    throw Ops.TypeErrorForBadInstance("expected object with __len__ function, got {0}", items);

                int len = Converter.ConvertToInt32(Ops.Call(lenFunc));

                Array res = Array.CreateInstance(type, len);

                IEnumerator ie = Ops.GetEnumerator(items);
                int i = 0;
                while (ie.MoveNext()) {
                    res.SetValue(Ops.ConvertTo(ie.Current, type), i++);
                }

                return res;
            }
        }

        /// <summary>
        /// Multiply two object[] arrays - slow version, we need to get the type, etc...
        /// </summary>
        [PythonName("__mul__")]
        public static Array Multiply(Array data, int count) {
            if (data.Rank > 1) throw new NotImplementedException("can't multiply multidimensional arrays");

            Type elemType = data.GetType().GetElementType();
            if (count <= 0) return Array.CreateInstance(elemType, 0);

            int newCount = data.Length * count;

            Array ret = Array.CreateInstance(elemType, newCount);
            Array.Copy(data, 0, ret, 0, data.Length);

            // this should be extremely fast for large count as it uses the same algoithim as efficient integer powers
            // ??? need to test to see how large count and n need to be for this to be fastest approach
            int block = data.Length;
            int pos = data.Length;
            while (pos < newCount) {
                Array.Copy(ret, 0, ret, pos, Math.Min(block, newCount - pos));
                pos += block;
                block *= 2;
            }
            return ret;
        }

        [PythonName("__getitem__")]
        public static object GetItem(Array data, int index) {
            return GetIndex(data, index); // data[Ops.FixIndex(index, data.Length)];
        }

        [PythonName("__getitem__")]
        public static object GetItem(Array data, Slice slice) {
            return GetSlice(data, data.Length, slice);
        }

        [PythonName("__setitem__")]
        public static void SetItem(Array data, int index, object value) {
            SetIndex(data, index, value);
        }

        [PythonName("__repr__")]
        public static string CodeRepresentation(Array a) {
            StringBuilder ret = new StringBuilder();
            ret.Append(a.GetType().FullName);
            ret.Append("(");
            switch (a.Rank) {
                case 1: {
                        for (int i = 0; i < a.Length; i++) {
                            if (i > 0) ret.Append(", ");
                            ret.Append(Ops.StringRepr(a.GetValue(i + a.GetLowerBound(0))));
                        }
                    }
                    break;
                case 2: {
                        int imax = a.GetLength(0);
                        int jmax = a.GetLength(1);
                        for (int i = 0; i < imax; i++) {
                            ret.Append("\n");
                            for (int j = 0; j < jmax; j++) {
                                if (j > 0) ret.Append(", ");
                                ret.Append(Ops.StringRepr(a.GetValue(i + a.GetLowerBound(0), j + a.GetLowerBound(1))));
                            }
                        }
                    }
                    break;
                default:
                    ret.Append(" Multi-dimensional array ");
                    break;
            }
            ret.Append(")");
            return ret.ToString();
        }

        #endregion

        #region Internal APIs

        internal static ReflectedType MakeDynamicType() {
            ReflectedType ret = new ReflectedArrayType("array", typeof(Array));
            ArrayType = ret;
            return ret;
        }

        /// <summary>
        /// Multiply two object[] arrays - internal version used for objects backed by arrays
        /// </summary>
        internal static object[] Multiply(object[] data, int size, int count) {
            int newCount = size * count;

            object[] ret = new object[newCount];
            if (count > 0) {
                Array.Copy(data, 0, ret, 0, size);

                // this should be extremely fast for large count as it uses the same algoithim as efficient integer powers
                // ??? need to test to see how large count and n need to be for this to be fastest approach
                int block = size;
                int pos = size;
                while (pos < newCount) {
                    Array.Copy(ret, 0, ret, pos, Math.Min(block, newCount - pos));
                    pos += block;
                    block *= 2;
                }
            }
            return ret;
        }

        /// <summary>
        /// Add two arrays - internal versions for objects backed by arrays
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="size1"></param>
        /// <param name="data2"></param>
        /// <param name="size2"></param>
        /// <returns></returns>
        internal static object[] Add(object[] data1, int size1, object[] data2, int size2) {
            object[] ret = new object[size1 + size2];
            Array.Copy(data1, 0, ret, 0, size1);
            Array.Copy(data2, 0, ret, size1, size2);
            return ret;
        }


        internal static Array GetSlice(Array data, int size, Slice slice) {
            if (data.Rank != 1) throw Ops.NotImplementedError("slice on multi-dimensional array");

            int start, stop, step;
            slice.indices(size, out start, out stop, out step);

            if ((step > 0 && start >= stop) || (step < 0 && start <= stop))
                return Ops.EMPTY;

            if (step == 1) {
                int n = stop - start;
                Array ret = Array.CreateInstance(data.GetType().GetElementType(), n);
                Array.Copy(data, start + data.GetLowerBound(0), ret, 0, n);
                return ret;
            } else {
                // could cause overflow (?)
                int n = step > 0 ? (stop - start + step - 1) / step : (stop - start + step + 1) / step;
                object[] ret = new object[n];
                int ri = 0;
                for (int i = 0, index = start; i < n; i++, index += step) {
                    ret[ri++] = data.GetValue(index + data.GetLowerBound(0));
                }
                return ret;
            }
        }

        internal static object GetIndex(Array a, object index) {
            int iindex;;
            if (Converter.TryConvertToInt32(index, out iindex)) {
                return a.GetValue(Ops.FixIndex(iindex, a.Length) + a.GetLowerBound(0));
            }

            Tuple ituple = index as Tuple;
            if (ituple != null && ituple.IsExpandable) {
                int[] indices = TupleToIndices(a, ituple);
                for (int i = 0; i < indices.Length; i++) indices[i] += a.GetLowerBound(i);

                return a.GetValue(indices);
            }

            Slice slice = index as Slice;
            if (slice != null) {
                return GetSlice(a, a.Length, slice);
            }

            throw Ops.TypeErrorForBadInstance("bad array index: {0}", index);
        }

        internal static void SetIndex(Array a, object index, object value) {
            Type t = a.GetType();
            Debug.Assert(t.HasElementType);

            Type elm = t.GetElementType();

            int iindex;
            if (Converter.TryConvertToInt32(index, out iindex)) {
                a.SetValue(Ops.ConvertTo(value, elm), Ops.FixIndex(iindex, a.Length) + a.GetLowerBound(0));
                return;
            }

            Tuple ituple = index as Tuple;
            if (ituple != null && ituple.IsExpandable) {
                if (a.Rank != ituple.Count) throw Ops.ValueError("bad dimensions for array, got {0} expected {1}", ituple.Count, a.Rank);

                int[] indices = TupleToIndices(a, ituple);
                for (int i = 0; i < indices.Length; i++) indices[i] += a.GetLowerBound(i);
                a.SetValue(value, indices);
                return;
            }

            Slice slice = index as Slice;
            if (slice != null) {
                if (a.Rank != 1) throw Ops.NotImplementedError("slice on multi-dimensional array");

                slice.DoSliceAssign(
                    delegate(int idx, object val) {
                        a.SetValue(Ops.ConvertTo(val, elm), idx + a.GetLowerBound(0));
                    },
                    a.Length,
                    value);
                return;
            }

            throw Ops.TypeErrorForBadInstance("bad type for array index: {0}", value);
        }

        #endregion

        #region Private helpers

        private static int[] TupleToIndices(Array a, Tuple tuple) {
            int[] indices = new int[tuple.Count];
            for (int i = 0; i < indices.Length; i++) {
                indices[i] = Ops.FixIndex(Converter.ConvertToInt32(tuple[i]), a.GetUpperBound(i) + 1);
            }
            return indices;
        }

        #endregion
    }
}
