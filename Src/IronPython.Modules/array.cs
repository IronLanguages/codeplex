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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using SpecialName = System.Runtime.CompilerServices.SpecialNameAttribute;

[assembly: PythonModule("array", typeof(IronPython.Modules.ArrayModule))]
namespace IronPython.Modules {
    public static class ArrayModule  {
        public static object ArrayType = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonArray));
        public static object array = ArrayType;

        [PythonType("array")]
        public class PythonArray : IPythonArray, IValueEquality, IEnumerable, IWeakReferenceable {
            private ArrayData data;
            private char typeCode;
            private WeakRefTracker tracker;

            public PythonArray(string type, [Optional]object initializer) {
                if (type == null || type.Length != 1) throw PythonOps.TypeError("expected character, got {0}", DynamicHelpers.GetDynamicType(type));

                typeCode = type[0];
                data = CreateData(typeCode);

                if (initializer != Type.Missing) Extend(initializer);
            }

            private static ArrayData CreateData(char typecode) {                
                ArrayData data;
                switch (typecode) {
                    case 'c': data = new ArrayData<char>(); break;
                    case 'b': data = new ArrayData<sbyte>(); break;
                    case 'B': data = new ArrayData<byte>(); break;
                    case 'u': data = new ArrayData<char>(); break;
                    case 'h': data = new ArrayData<short>(); break;
                    case 'H': data = new ArrayData<ushort>(); break;
                    case 'l':
                    case 'i': data = new ArrayData<int>(); break;
                    case 'L':
                    case 'I': data = new ArrayData<uint>(); break;
                    case 'f': data = new ArrayData<float>(); break;
                    case 'd': data = new ArrayData<double>(); break;
                    default:
                        throw PythonOps.ValueError("Bad type code (expected one of 'c', 'b', 'B', 'u', 'H', 'h', 'i', 'I', 'l', 'L', 'f', 'd')");
                }
                return data;
            }

            [SpecialName, PythonName("__iadd__")]
            public PythonArray InPlaceAdd(PythonArray other) {
                if (TypeCode != other.TypeCode) throw PythonOps.TypeError("cannot add different typecodes");

                if (other.data.Length != 0) {
                    Extend(other);
                }

                return this;
            }

            [SpecialName, PythonName("__add__")]
            public PythonArray Add(PythonArray other) {
                if (TypeCode != other.TypeCode) throw PythonOps.TypeError("cannot add different typecodes");

                PythonArray res = new PythonArray(new string(TypeCode, 1), Type.Missing);
                foreach (object o in this) {
                    res.Append(o);
                }
                
                foreach (object o in other) {
                    res.Append(o);
                }

                return res;
            }

            [SpecialName, PythonName("__imul__")]
            public PythonArray InPlaceMultiply(int value) {
                if (value <= 0) {
                    data.Clear();
                } else {
                    List myData = ToList();

                    for (int i = 0; i < (value - 1); i++) {
                        Extend(myData);
                    }
                }
                return this;
            }

            public static PythonArray operator *(PythonArray array, int value) {
                PythonArray data = new PythonArray(new string(array.TypeCode, 1), Type.Missing);
                for (int i = 0; i < value; i++) {
                    data.Extend(array);
                }
                return data;
            }

            public static PythonArray operator *(int value, PythonArray array) {
                PythonArray data = new PythonArray(new string(array.TypeCode, 1), Type.Missing);
                for (int i = 0; i < value; i++) {
                    data.Extend(array);
                }
                return data;
            }

            [PythonName("append")]
            public void Append(object iterable) {
                data.Append(iterable);
            }

            [PythonName("buffer_info")]
            public void BufferInfo() {
                throw PythonOps.NotImplementedError("buffer_info not implemented");
            }

            [PythonName("byteswap")]
            public void ByteSwap() {
                Stream s = ToStream();
                byte[] bytes = new byte[s.Length];
                s.Read(bytes, 0, bytes.Length);

                byte[] tmp = new byte[ItemSize];
                for (int i = 0; i < bytes.Length; i += ItemSize) {
                    for (int j = 0; j < ItemSize; j++) {
                        tmp[j] = bytes[i + j];
                    }
                    for (int j = 0; j < ItemSize; j++) {
                        bytes[i + j] = tmp[ItemSize - (j+1)];
                    }
                }
                data.Clear();
                MemoryStream ms = new MemoryStream(bytes);
                FromStream(ms);
            }

            [PythonName("count")]
            public int Count(object x) {
                if (x == null) return 0;

                return data.Count(x);
            }

            [PythonName("extend")]
            public void Extend(object iterable) {
                PythonArray pa = iterable as PythonArray;
                if (pa != null && TypeCode != pa.TypeCode) {
                    throw PythonOps.TypeError("cannot extend with different typecode");
                }

                IEnumerator ie = PythonOps.GetEnumerator(iterable);
                while (ie.MoveNext()) {
                    Append(ie.Current);
                }
            }

            [PythonName("fromlist")]
            public void FromList(object iterable) {
                IEnumerator ie = PythonOps.GetEnumerator(iterable);

                List<object> items = new List<object>();
                while (ie.MoveNext()) {
                    if (!data.CanStore(ie.Current)) {
                        throw PythonOps.TypeError("expected {0}, got {1}",
                            PythonOps.StringRepr(DynamicHelpers.GetDynamicTypeFromType(data.StorageType)),
                            PythonOps.StringRepr(DynamicHelpers.GetDynamicType(ie.Current)));
                    }
                    items.Add(ie.Current);
                }

                Extend(items);
            }

            [PythonName("fromfile")]
            public void FromFile(PythonFile f, int n) {
                int bytesNeeded = n * ItemSize;
                string bytes = f.Read(bytesNeeded);
                if (bytes.Length < bytesNeeded) throw PythonOps.EofError("file not large enough");

                FromString(bytes);
            }

            [PythonName("fromstring")]
            public void FromString(string s) {
                if ((s.Length % ItemSize) != 0) throw PythonOps.ValueError("string length not a multiple of itemsize");
                byte[] bytes = new byte[s.Length];
                for (int i = 0; i < bytes.Length; i++) {
                    bytes[i] = (byte)s[i];
                }
                MemoryStream ms = new MemoryStream(bytes);

                FromStream(ms);
            }

            [PythonName("fromunicode")]
            public void FromUnicode(string s) {
                if (s == null) throw PythonOps.TypeError("expected string");
                if (s.Length == 0) throw PythonOps.ValueError("empty string");
                if ((s.Length % ItemSize) != 0) throw PythonOps.ValueError("string length not a multiple of itemsize");
                MemoryStream ms = new MemoryStream(SystemState.Instance.DefaultEncoding.GetBytes(s));

                FromStream(ms);
            }

            [PythonName("index")]
            public int Index(object x) {
                if (x == null) throw PythonOps.ValueError("got None, expected value");

                int res = data.Index(x);
                if (res == -1) throw PythonOps.ValueError("x not found");
                return res;
            }

            [PythonName("insert")]
            public void Insert(int i, object x) {
                if (i > data.Length) i = data.Length;
                if (i < 0) i = data.Length + i;
                if (i < 0) i = 0;

                data.Insert(i, x);
            }

            public int ItemSize {
                [PythonName("itemsize")]
                get {
                    return Marshal.SizeOf(data.StorageType);
                }
            }

            [PythonName("pop")]
            public object Pop() {
                return Pop(-1);
            }

            [PythonName("pop")]
            public object Pop(int i) {
                i = PythonOps.FixIndex(i, data.Length);
                object res = data.GetData(i);
                data.RemoveAt(i);
                return res;
            }

            [PythonName("remove")]
            public void Remove(object value) {
                if (value == null) throw PythonOps.ValueError("got None, expected value");

                data.Remove(value);
            }

            [PythonName("reverse")]
            public void Reverse() {
                for (int index = 0; index < data.Length/2; index++) {
                    int left = index, right= data.Length-(index+1);

                    Debug.Assert(left != right);
                    data.Swap(left, right);                    
                }
            }

            public virtual object this[int index] {
                [PythonName("__getitem__")]
                get {
                    object val = data.GetData(PythonOps.FixIndex(index, data.Length));
                    switch (TypeCode) {
                        case 'b': return (int)(sbyte)val; 
                        case 'B': return (int)(byte)val; 
                        case 'c':
                        case 'u': return new string((char)val, 1);
                        case 'h': return (int)(short)val; 
                        case 'H': return (int)(ushort)val;
                        case 'l': return BigInteger.Create((int)val);
                        case 'i': return val;
                        case 'L': return BigInteger.Create((uint)val);
                        case 'I': return (int)(uint)val;
                        case 'f': return (double)(float)val;
                        case 'd': return val; 
                        default:
                            throw PythonOps.ValueError("Bad type code (expected one of 'c', 'b', 'B', 'u', 'H', 'h', 'i', 'I', 'l', 'L', 'f', 'd')");
                    }
                }
                [PythonName("__setitem__")]
                set {
                    data.SetData(PythonOps.FixIndex(index, data.Length), value);
                }
            }

            [SpecialName, PythonName("__delitem__")]
            public void DeleteItem(int index) {
                data.RemoveAt(PythonOps.FixIndex(index, data.Length));
            }
            [SpecialName, PythonName("__delitem__")]
            public void DeleteItem(Slice slice) {
                if (slice == null) throw PythonOps.TypeError("expected Slice, got None");

                int start, stop, step;
                // slice is sealed, indices can't be user code...
                slice.Indices(data.Length, out start, out stop, out step);

                if (step > 0 && (start >= stop)) return;
                if (step < 0 && (start <= stop)) return;

                if (step == 1) {
                    int i = start;
                    for (int j = stop; j < data.Length; j++, i++) {
                        data.SetData(i, data.GetData(j));
                    }
                    for (i = 0; i < stop - start; i++) {
                        data.RemoveAt(data.Length - 1);
                    }
                    return;
                }
                if (step == -1) {
                    int i = stop + 1;
                    for (int j = start + 1; j < data.Length; j++, i++) {
                        data.SetData(i, data.GetData(j));
                    }
                    for (i = 0; i < stop - start; i++) {
                        data.RemoveAt(data.Length - 1);
                    }
                    return;
                }

                if (step < 0) {
                    // find "start" we will skip in the 1,2,3,... order
                    int i = start;
                    while (i > stop) {
                        i += step;
                    }
                    i -= step;

                    // swap start/stop, make step positive
                    stop = start + 1;
                    start = i;
                    step = -step;
                }

                int curr, skip, move;
                // skip: the next position we should skip
                // curr: the next position we should fill in data
                // move: the next position we will check
                curr = skip = move = start;

                while (curr < stop && move < stop) {
                    if (move != skip) {
                        data.SetData(curr++, data.GetData(move));
                    } else
                        skip += step;
                    move++;
                }
                while (stop < data.Length) {
                    data.SetData(curr++, data.GetData(stop++));
                }
                while(data.Length > curr) {
                    data.RemoveAt(data.Length - 1);
                }
            }

            public object this[Slice index] {
                get {
                    if (index == null) throw PythonOps.TypeError("expected Slice, got None");

                    int start, stop, step;
                    index.Indices(data.Length, out start, out stop, out step);

                    PythonArray pa = new PythonArray(new string(typeCode, 1), Type.Missing);
                    if (step < 0) {
                        for (int i = start; i > stop; i += step) {
                            pa.data.Append(data.GetData(i));
                        }
                    } else {
                        for (int i = start; i < stop; i += step) {
                            pa.data.Append(data.GetData(i));
                        }
                    }
                    return pa;
                }
                set {
                    if (index == null) throw PythonOps.TypeError("expected Slice, got None");

                    PythonArray pa = value as PythonArray;
                    if (pa != null && pa.typeCode != typeCode) {
                        throw PythonOps.TypeError("bad array type");
                    }

                    if (index.Step != null) {
                        if (Object.ReferenceEquals(value, this)) value = this.ToList();

                        index.DoSliceAssign(SliceAssign, data.Length, value);
                    } else {
                        int start, stop, step;
                        index.Indices(data.Length, out start, out stop, out step);

                        // replace between start & stop w/ values
                        IEnumerator ie = PythonOps.GetEnumerator(value);

                        ArrayData newData = CreateData(TypeCode);
                        for (int i = 0; i < start; i++) {
                            newData.Append(data.GetData(i));
                        }

                        while (ie.MoveNext()) {
                            newData.Append(ie.Current);
                        }

                        for (int i = stop; i < data.Length; i++) {
                            newData.Append(data.GetData(i));
                        }

                        data = newData;
                    }
                }
            }

            [PythonName("__getslice__")]
            public object GetSlice(object start, object stop) {
                return this[new Slice(start, stop)];
            }

            [PythonName("__setslice__")]
            public void SetSlice(object start, object stop, object value) {
                this[new Slice(start, stop)] = value;
            }
            private void SliceAssign(int index, object value) {
                data.SetData(index, value);
            }

            [PythonName("tofile")]
            public void ToFile(PythonFile f) {
                f.Write(ConvertToString());                
            }

            [PythonName("tolist")]
            public List ToList() {
                List res = new List();
                for (int i = 0; i < data.Length; i++) {
                    res.AddNoLock(data.GetData(i));
                }
                return res;
            }

            [PythonName("tostring")]
            public string ConvertToString() {
                Stream s = ToStream();
                byte [] bytes = new byte[s.Length];
                s.Read(bytes, 0, (int)s.Length);

                StringBuilder res = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) {
                    res.Append((char)bytes[i]);
                }
                return res.ToString();
            }

            [PythonName("tounicode")]
            public string ToUnicode() {
                if (typeCode != 'u') throw PythonOps.ValueError("only 'u' arrays can be converted to unicode");

                Stream s = ToStream();
                byte[] bytes = new byte[s.Length];
                s.Read(bytes, 0, (int)s.Length);

                return SystemState.Instance.DefaultEncoding.GetString(bytes, 0, bytes.Length);
            }

            [PythonName("write")]
            public void Write(PythonFile f) {
                ToFile(f);
            }

            public char TypeCode {
                [PythonName("typecode")]
                get { return typeCode; }
            }

            private abstract class ArrayData {
                public abstract void SetData(int index, object value);
                public abstract object GetData(int index);
                public abstract void Append(object value);
                public abstract int Count(object value);
                public abstract bool CanStore(object value);
                public abstract Type StorageType { get; }
                public abstract int Index(object value);
                public abstract void Insert(int index, object value);
                public abstract void Remove(object value);
                public abstract void RemoveAt(int index);
                public abstract int Length { get; }
                public abstract void Swap(int x, int y);
                public abstract void Clear();
            }

            private Stream ToStream() {
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                for (int i = 0; i < data.Length; i++) {
                    switch (TypeCode) {
                        case 'c': bw.Write((byte)(char)data.GetData(i)); break;
                        case 'b': bw.Write((sbyte)data.GetData(i)); break;
                        case 'B': bw.Write((byte)data.GetData(i)); break;
                        case 'u': bw.Write((char)data.GetData(i)); break;
                        case 'h': bw.Write((short)data.GetData(i)); break;
                        case 'H': bw.Write((ushort)data.GetData(i)); break;
                        case 'l':
                        case 'i': bw.Write((int)data.GetData(i)); break;
                        case 'L':
                        case 'I': bw.Write((uint)data.GetData(i)); break;
                        case 'f': bw.Write((float)data.GetData(i)); break;
                        case 'd': bw.Write((double)data.GetData(i)); break;
                    }                    
                }
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }

            private void FromStream(Stream ms) {
                BinaryReader br = new BinaryReader(ms);

                for (int i = 0; i < ms.Length/ItemSize; i++) {
                    object value;
                    switch (TypeCode) {
                        case 'c': value = (char)br.ReadByte(); break;
                        case 'b': value = (sbyte)br.ReadByte(); break;
                        case 'B': value = br.ReadByte(); break;
                        case 'u': value = br.ReadChar(); break;
                        case 'h': value = br.ReadInt16(); break;
                        case 'H': value = br.ReadUInt16(); break;
                        case 'i': value = br.ReadInt32(); break;
                        case 'I': value = br.ReadUInt32(); break;                            
                        case 'l': value = br.ReadInt32(); break;
                        case 'L': value = br.ReadUInt32(); break;
                        case 'f': value = br.ReadSingle(); break;
                        case 'd': value = br.ReadDouble(); break;
                        default: throw new InvalidOperationException(); // should never happen
                    }
                    data.Append(value);
                }
            }

            private class ArrayData<T> : ArrayData {
                List<T> data;

                public ArrayData() {
                    data = new List<T>();
                }

                public override object GetData(int index) {
                    return data[index];
                }

                public override void SetData(int index, object value) {
                    data[index] = GetValue(value);
                }

                private static T GetValue(object value) {
                    if (!(value is T)) {
                        object newVal;
                        if (!Converter.TryConvert(value, typeof(T), out newVal)) {
                            if(value != null && typeof(T).IsPrimitive && typeof(T) != typeof(char))
                                throw PythonOps.OverflowError("couldn't convert {1} to {0}",
                                    PythonOps.StringRepr(DynamicHelpers.GetDynamicTypeFromType(typeof(T))),
                                    PythonOps.StringRepr(DynamicHelpers.GetDynamicType(value)));
                            throw PythonOps.TypeError("expected {0}, got {1}",
                                PythonOps.StringRepr(DynamicHelpers.GetDynamicTypeFromType(typeof(T))),
                                PythonOps.StringRepr(DynamicHelpers.GetDynamicType(value)));
                        }
                        value = newVal;
                    }
                    return (T)value;
                }

                public override void Append(object value) {
                    data.Add(GetValue(value));
                }

                public override int Count(object value) {
                    T other = GetValue(value);

                    int count = 0;
                    foreach (T item in data) {
                        if (item.Equals(other)) count++;
                    }
                    return count;
                }

                public override void Insert(int index, object value) {
                    data.Insert(index, GetValue(value));
                }

                public override int Index(object value) {
                    T other = GetValue(value);

                    for(int i = 0; i<data.Count; i++) {
                        if (data[i].Equals(other)) return i;
                    }
                    return -1;
                }

                public override void Remove(object value) {
                    T other = GetValue(value);

                    for (int i = 0; i < data.Count; i++) {
                        if (data[i].Equals(other)) {
                            RemoveAt(i);
                            return;
                        }
                    }
                    throw PythonOps.ValueError("couldn't find value to remove");
                }

                public override void RemoveAt(int index) {
                    data.RemoveAt(index);
                }

                public override void Swap(int x, int y) {
                    T temp = data[x];
                    data[x] = data[y];
                    data[y] = temp;
                }

                public override int Length {
                    get {
                        return data.Count;
                    }
                }

                public override void Clear() {
                    data.Clear();
                }

                public override bool CanStore(object value) {
                    object tmp;
                    if (!(value is T) && !Converter.TryConvert(value, typeof(T), out tmp))
                        return false;

                    return true;
                }                

                public override Type StorageType {
                    get { return typeof(T); }
                }
            }

            #region IValueEquality Members

            public int GetValueHashCode() {
                throw PythonOps.TypeError("unhashable type");
            }

            public bool ValueEquals(object other) {
                PythonArray pa = other as PythonArray;
                if (pa == null) return false;

                if (data.Length != pa.data.Length) return false;
                for (int i = 0; i < data.Length; i++) {
                    if (!data.GetData(i).Equals(pa.data.GetData(i))) {
                        return false;
                    }
                }

                return true;
            }

            public bool ValueNotEquals(object other) {
                return !ValueEquals(other);
            }

            #endregion

            #region IEnumerable Members

            public IEnumerator GetEnumerator() {
                for (int i = 0; i < data.Length; i++) {
                    yield return data.GetData(i);
                }
            }

            #endregion

            public override string ToString() {
                string res = "array('" + TypeCode.ToString() + "'";
                if (data.Length == 0) {
                    return res + ")";
                }
                StringBuilder sb = new StringBuilder(res);
                if (TypeCode == 'c') sb.Append(", '");
                else sb.Append(", [");
                
                for (int i = 0; i < data.Length; i++) {
                    if (TypeCode == 'c') sb.Append((char)data.GetData(i));
                    else {
                        sb.Append(PythonOps.Repr(this[i]).ToString());
                        sb.Append(", ");
                    }
                }

                if (TypeCode == 'c') sb.Append("')");
                else sb.Append("])");

                return sb.ToString();
            }

            #region IWeakReferenceable Members

            public WeakRefTracker GetWeakRef() {
                return tracker;
            }

            public bool SetWeakRef(WeakRefTracker value) {
                tracker = value;
                return true;
            }

            public void SetFinalizer(WeakRefTracker value) {
                tracker = value;
            }

            #endregion

            #region IPythonContainer Members

            public int GetLength() {
                return data.Length;
            }

            public bool ContainsValue(object value) {
                return data.Index(value) != -1;
            }

            #endregion

            #region IRichComparable Members

            [PythonName("__cmp__")]
            [return: MaybeNotImplemented]
            public object CompareTo(object other) {
                PythonArray pa = other as PythonArray;
                if (pa == null) return PythonOps.NotImplemented;
                if (pa.TypeCode != TypeCode) return PythonOps.NotImplemented;

                if (pa.data.Length != data.Length) {
                    return data.Length - pa.data.Length;
                }

                for (int i = 0; i < pa.data.Length; i++) {
                    int cmp = PythonOps.Compare(data.GetData(i), pa.data.GetData(i));
                    if (cmp != 0) return RuntimeHelpers.Int32ToObject(cmp);
                }

                return RuntimeHelpers.Int32ToObject(0);
            }

            public object GreaterThan(object other) {
                object res = CompareTo(other);
                if (res == PythonOps.NotImplemented) return res;

                return RuntimeHelpers.BooleanToObject((int)res > 0);
            }

            public object LessThan(object other) {
                object res = CompareTo(other);
                if (res == PythonOps.NotImplemented) return res;

                return RuntimeHelpers.BooleanToObject((int)res < 0);
            }

            public object GreaterThanOrEqual(object other) {
                object res = CompareTo(other);
                if (res == PythonOps.NotImplemented) return res;

                return RuntimeHelpers.BooleanToObject((int)res >= 0);
            }

            public object LessThanOrEqual(object other) {
                object res = CompareTo(other);
                if (res == PythonOps.NotImplemented) return res;

                return RuntimeHelpers.BooleanToObject((int)res <= 0);
            }

            #endregion

            #region ISequence Members

            public object AddSequence(object other) {
                PythonArray pa = other as PythonArray;
                if(pa != null) 
                    return Add(pa);

                throw PythonOps.TypeErrorForBadInstance("got {0}, expected array", other);
            }

            public object MultiplySequence(object count) {
                if (count is int) {
                    return this * (int)count;
                }

                throw PythonOps.TypeErrorForBadInstance("got {0}, expected int", count);
            }

            public object GetSlice(int start, int stop) {
                return this[new Slice(start, stop)];
            }

            #endregion
        }
    }
}
