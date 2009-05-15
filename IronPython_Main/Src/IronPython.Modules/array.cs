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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using SpecialName = System.Runtime.CompilerServices.SpecialNameAttribute;

[assembly: PythonModule("array", typeof(IronPython.Modules.ArrayModule))]
namespace IronPython.Modules {
    public static class ArrayModule {
        public static readonly PythonType/*!*/ ArrayType = DynamicHelpers.GetPythonTypeFromType(typeof(PythonArray));
        public static readonly PythonType/*!*/ array = ArrayType;

        [PythonType("array")]
        public class PythonArray : IPythonArray, IValueEquality, IEnumerable, IWeakReferenceable, ICollection, ICodeFormattable {
            private ArrayData _data;
            private char _typeCode;
            private WeakRefTracker _tracker;

            public PythonArray(string type, [Optional]object initializer) {
                if (type == null || type.Length != 1) throw PythonOps.TypeError("expected character, got {0}", DynamicHelpers.GetPythonType(type));

                _typeCode = type[0];
                _data = CreateData(_typeCode);

                if (initializer != Type.Missing) extend(initializer);
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

            [SpecialName]
            public PythonArray InPlaceAdd(PythonArray other) {
                if (typecode != other.typecode) throw PythonOps.TypeError("cannot add different typecodes");

                if (other._data.Length != 0) {
                    extend(other);
                }

                return this;
            }

            [SpecialName]
            public static PythonArray operator +(PythonArray self, PythonArray other) {
                if (self.typecode != other.typecode) throw PythonOps.TypeError("cannot add different typecodes");

                PythonArray res = new PythonArray(self.typecode, Type.Missing);
                foreach (object o in self) {
                    res.append(o);
                }

                foreach (object o in other) {
                    res.append(o);
                }

                return res;
            }

            [SpecialName]
            public PythonArray InPlaceMultiply(int value) {
                if (value <= 0) {
                    _data.Clear();
                } else {
                    List myData = tolist();

                    for (int i = 0; i < (value - 1); i++) {
                        extend(myData);
                    }
                }
                return this;
            }

            public static PythonArray operator *(PythonArray array, int value) {
                if ((BigInteger)value * array.__len__() * array.itemsize > SysModule.maxsize) {
                    throw PythonOps.MemoryError("");
                }
                PythonArray data = new PythonArray(array.typecode, Type.Missing);
                for (int i = 0; i < value; i++) {
                    data.extend(array);
                }
                return data;
            }

            public static PythonArray operator *(PythonArray array, BigInteger value) {
                if (value * array.__len__() * array.itemsize > SysModule.maxsize) {
                    throw PythonOps.MemoryError("");
                }
                int intValue;
                if (!value.AsInt32(out intValue)) {
                    throw PythonOps.OverflowError("cannot fit 'long' into an index-sized integer");
                }
                return array * intValue;
            }

            public static PythonArray operator *(int value, PythonArray array) {
                if ((BigInteger)value * array.__len__() * array.itemsize > SysModule.maxsize) {
                    throw PythonOps.MemoryError("");
                }
                PythonArray data = new PythonArray(array.typecode, Type.Missing);
                for (int i = 0; i < value; i++) {
                    data.extend(array);
                }
                return data;
            }

            public static PythonArray operator *(BigInteger value, PythonArray array) {
                if (value * array.__len__() * array.itemsize > SysModule.maxsize) {
                    throw PythonOps.MemoryError("");
                }
                int intValue;
                if (!value.AsInt32(out intValue)) {
                    throw PythonOps.OverflowError("cannot fit 'long' into an index-sized integer");
                }
                return intValue * array;
            }

            public void append(object iterable) {
                _data.Append(iterable);
            }

            internal IntPtr GetArrayAddress() {
                return _data.GetAddress();
            }

            public PythonTuple buffer_info() {
                return PythonTuple.MakeTuple(
                    _data.GetAddress().ToPython(),
                    _data.Length
                );
            }

            public void byteswap() {
                Stream s = ToStream();
                byte[] bytes = new byte[s.Length];
                s.Read(bytes, 0, bytes.Length);

                byte[] tmp = new byte[itemsize];
                for (int i = 0; i < bytes.Length; i += itemsize) {
                    for (int j = 0; j < itemsize; j++) {
                        tmp[j] = bytes[i + j];
                    }
                    for (int j = 0; j < itemsize; j++) {
                        bytes[i + j] = tmp[itemsize - (j + 1)];
                    }
                }
                _data.Clear();
                MemoryStream ms = new MemoryStream(bytes);
                FromStream(ms);
            }

            public int count(object x) {
                if (x == null) return 0;

                return _data.Count(x);
            }

            public void extend(object iterable) {
                PythonArray pa = iterable as PythonArray;
                if (pa != null && typecode != pa.typecode) {
                    throw PythonOps.TypeError("cannot extend with different typecode");
                }

                string buf = iterable as string;
                if (buf != null && _typeCode != 'u') {
                    fromstring(buf);
                    return;
                }

                IEnumerator ie = PythonOps.GetEnumerator(iterable);
                while (ie.MoveNext()) {
                    append(ie.Current);
                }
            }

            public void fromlist(object iterable) {
                IEnumerator ie = PythonOps.GetEnumerator(iterable);

                List<object> items = new List<object>();
                while (ie.MoveNext()) {
                    if (!_data.CanStore(ie.Current)) {
                        throw PythonOps.TypeError("expected {0}, got {1}",
                            DynamicHelpers.GetPythonTypeFromType(_data.StorageType).Name,
                            DynamicHelpers.GetPythonType(ie.Current).Name);
                    }
                    items.Add(ie.Current);
                }

                extend(items);
            }

            public void fromfile(PythonFile f, int n) {
                int bytesNeeded = n * itemsize;
                string bytes = f.read(bytesNeeded);
                if (bytes.Length < bytesNeeded) throw PythonOps.EofError("file not large enough");

                fromstring(bytes);
            }

            public void fromstring(string s) {
                if ((s.Length % itemsize) != 0) throw PythonOps.ValueError("string length not a multiple of itemsize");
                byte[] bytes = new byte[s.Length];
                for (int i = 0; i < bytes.Length; i++) {
                    bytes[i] = (byte)s[i];
                }
                MemoryStream ms = new MemoryStream(bytes);

                FromStream(ms);
            }

            public void fromunicode(CodeContext/*!*/ context, string s) {
                if (s == null) throw PythonOps.TypeError("expected string");
                if (s.Length == 0) throw PythonOps.ValueError("empty string");
                if ((s.Length % itemsize) != 0) throw PythonOps.ValueError("string length not a multiple of itemsize");
                MemoryStream ms = new MemoryStream(PythonContext.GetContext(context).DefaultEncoding.GetBytes(s));

                FromStream(ms);
            }

            public int index(object x) {
                if (x == null) throw PythonOps.ValueError("got None, expected value");

                int res = _data.Index(x);
                if (res == -1) throw PythonOps.ValueError("x not found");
                return res;
            }

            public void insert(int i, object x) {
                if (i > _data.Length) i = _data.Length;
                if (i < 0) i = _data.Length + i;
                if (i < 0) i = 0;

                _data.Insert(i, x);
            }

            public int itemsize {
                get {
                    switch (_typeCode) {
                        case 'c': // char
                        case 'b': // signed byte
                        case 'B': // unsigned byte
                        case 'x': // pad byte
                        case 's': // null-terminated string
                        case 'p': // Pascal string
                        case 'u': // unicode char
                            return 1;
                        case 'h': // signed short
                        case 'H': // unsigned short
                            return 2;
                        case 'i': // signed int
                        case 'I': // unsigned int
                        case 'l': // signed long
                        case 'L': // unsigned long
                        case 'f': // float
                            return 4;
                        case 'P': // pointer
                            return IntPtr.Size;
                        case 'q': // signed long long
                        case 'Q': // unsigned long long
                        case 'd': // double
                            return 8;
                        default:
                            return 0;
                    }
                }
            }

            public object pop() {
                return pop(-1);
            }

            public object pop(int i) {
                i = PythonOps.FixIndex(i, _data.Length);
                object res = _data.GetData(i);
                _data.RemoveAt(i);
                return res;
            }

            public void remove(object value) {
                if (value == null) throw PythonOps.ValueError("got None, expected value");

                _data.Remove(value);
            }

            public void reverse() {
                for (int index = 0; index < _data.Length / 2; index++) {
                    int left = index, right = _data.Length - (index + 1);

                    Debug.Assert(left != right);
                    _data.Swap(left, right);
                }
            }

            public virtual object this[int index] {
                get {
                    object val = _data.GetData(PythonOps.FixIndex(index, _data.Length));
                    switch (_typeCode) {
                        case 'b': return (int)(sbyte)val;
                        case 'B': return (int)(byte)val;
                        case 'c':
                        case 'u': return new string((char)val, 1);
                        case 'h': return (int)(short)val;
                        case 'H': return (int)(ushort)val;
                        case 'l': return BigInteger.Create((int)val);
                        case 'i': return val;
                        case 'L': return BigInteger.Create((uint)val);
                        case 'I':
                            uint tmp = (uint)val;
                            if (tmp <= Int32.MaxValue) {
                                return (int)(uint)val;
                            }

                            return (BigInteger)tmp;
                        case 'f': return (double)(float)val;
                        case 'd': return val;
                        default:
                            throw PythonOps.ValueError("Bad type code (expected one of 'c', 'b', 'B', 'u', 'H', 'h', 'i', 'I', 'l', 'L', 'f', 'd')");
                    }
                }
                set {
                    _data.SetData(PythonOps.FixIndex(index, _data.Length), value);
                }
            }

            public void __delitem__(int index) {
                _data.RemoveAt(PythonOps.FixIndex(index, _data.Length));
            }

            public void __delitem__(Slice slice) {
                if (slice == null) throw PythonOps.TypeError("expected Slice, got None");

                int start, stop, step;
                // slice is sealed, indices can't be user code...
                slice.indices(_data.Length, out start, out stop, out step);

                if (step > 0 && (start >= stop)) return;
                if (step < 0 && (start <= stop)) return;

                if (step == 1) {
                    int i = start;
                    for (int j = stop; j < _data.Length; j++, i++) {
                        _data.SetData(i, _data.GetData(j));
                    }
                    for (i = 0; i < stop - start; i++) {
                        _data.RemoveAt(_data.Length - 1);
                    }
                    return;
                }
                if (step == -1) {
                    int i = stop + 1;
                    for (int j = start + 1; j < _data.Length; j++, i++) {
                        _data.SetData(i, _data.GetData(j));
                    }
                    for (i = 0; i < stop - start; i++) {
                        _data.RemoveAt(_data.Length - 1);
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
                        _data.SetData(curr++, _data.GetData(move));
                    } else
                        skip += step;
                    move++;
                }
                while (stop < _data.Length) {
                    _data.SetData(curr++, _data.GetData(stop++));
                }
                while (_data.Length > curr) {
                    _data.RemoveAt(_data.Length - 1);
                }
            }

            public object this[Slice index] {
                get {
                    if (index == null) throw PythonOps.TypeError("expected Slice, got None");

                    int start, stop, step;
                    index.indices(_data.Length, out start, out stop, out step);

                    PythonArray pa = new PythonArray(new string(_typeCode, 1), Type.Missing);
                    if (step < 0) {
                        for (int i = start; i > stop; i += step) {
                            pa._data.Append(_data.GetData(i));
                        }
                    } else {
                        for (int i = start; i < stop; i += step) {
                            pa._data.Append(_data.GetData(i));
                        }
                    }
                    return pa;
                }
                set {
                    if (index == null) throw PythonOps.TypeError("expected Slice, got None");

                    PythonArray pa = value as PythonArray;
                    if (pa != null && pa._typeCode != _typeCode) {
                        throw PythonOps.TypeError("bad array type");
                    }

                    if (index.step != null) {
                        if (Object.ReferenceEquals(value, this)) value = this.tolist();

                        index.DoSliceAssign(SliceAssign, _data.Length, value);
                    } else {
                        int start, stop, step;
                        index.indices(_data.Length, out start, out stop, out step);
                        if (stop < start) {
                            stop = start;
                        }

                        // replace between start & stop w/ values
                        IEnumerator ie = PythonOps.GetEnumerator(value);

                        ArrayData newData = CreateData(_typeCode);
                        for (int i = 0; i < start; i++) {
                            newData.Append(_data.GetData(i));
                        }

                        while (ie.MoveNext()) {
                            newData.Append(ie.Current);
                        }

                        for (int i = stop; i < _data.Length; i++) {
                            newData.Append(_data.GetData(i));
                        }

                        _data = newData;
                    }
                }
            }

            public object __getslice__(object start, object stop) {
                return this[new Slice(start, stop)];
            }

            public void __setslice__(object start, object stop, object value) {
                this[new Slice(start, stop)] = value;
            }

            public PythonTuple __reduce__() {
                return PythonOps.MakeTuple(
                    DynamicHelpers.GetPythonType(this),
                    PythonOps.MakeTuple(
                        typecode,
                        ToByteArray().MakeString()
                    ),
                    null
                );
            }

            public PythonArray __copy__() {
                return new PythonArray(typecode, this);
            }

            public PythonArray __deepcopy__() {
                // we only have simple data so this is the same as a copy
                return __copy__();
            }

            public PythonTuple __reduce_ex__(int version) {
                return __reduce__();
            }

            public PythonTuple __reduce_ex__() {
                return __reduce__();
            }

            private void SliceAssign(int index, object value) {
                _data.SetData(index, value);
            }

            public void tofile(PythonFile f) {
                f.write(tostring());
            }

            public List tolist() {
                List res = new List();
                for (int i = 0; i < _data.Length; i++) {
                    res.AddNoLock(_data.GetData(i));
                }
                return res;
            }

            public string tostring() {
                Stream s = ToStream();
                byte[] bytes = new byte[s.Length];
                s.Read(bytes, 0, (int)s.Length);

                StringBuilder res = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) {
                    res.Append((char)bytes[i]);
                }
                return res.ToString();
            }

            public string tounicode(CodeContext/*!*/ context) {
                if (_typeCode != 'u') throw PythonOps.ValueError("only 'u' arrays can be converted to unicode");

                Stream s = ToStream();
                byte[] bytes = new byte[s.Length];
                s.Read(bytes, 0, (int)s.Length);

                return PythonContext.GetContext(context).DefaultEncoding.GetString(bytes, 0, bytes.Length);
            }

            public void write(PythonFile f) {
                tofile(f);
            }

            public string/*!*/ typecode {
                get { return ScriptingRuntimeHelpers.CharToString(_typeCode); }
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
                public abstract IntPtr GetAddress();
            }

            private MemoryStream ToStream() {
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                for (int i = 0; i < _data.Length; i++) {
                    switch (_typeCode) {
                        case 'c': bw.Write((byte)(char)_data.GetData(i)); break;
                        case 'b': bw.Write((sbyte)_data.GetData(i)); break;
                        case 'B': bw.Write((byte)_data.GetData(i)); break;
                        case 'u': bw.Write((char)_data.GetData(i)); break;
                        case 'h': bw.Write((short)_data.GetData(i)); break;
                        case 'H': bw.Write((ushort)_data.GetData(i)); break;
                        case 'l':
                        case 'i': bw.Write((int)_data.GetData(i)); break;
                        case 'L':
                        case 'I': bw.Write((uint)_data.GetData(i)); break;
                        case 'f': bw.Write((float)_data.GetData(i)); break;
                        case 'd': bw.Write((double)_data.GetData(i)); break;
                    }
                }
                ms.Seek(0, SeekOrigin.Begin);
                return ms;                
            }

            internal byte[] ToByteArray() {
                return ToStream().ToArray();
            }

            internal void Clear() {
                _data = CreateData(_typeCode);
            }

            internal void FromStream(Stream ms) {
                BinaryReader br = new BinaryReader(ms);

                for (int i = 0; i < ms.Length / itemsize; i++) {
                    object value;
                    switch (_typeCode) {
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
                    _data.Append(value);
                }
            }

            // a version of FromStream that overwrites starting at 'index'
            internal void FromStream(int index, Stream ms) {
                BinaryReader br = new BinaryReader(ms);

                for (int i = index; i < ms.Length / itemsize + index; i++) {
                    object value;
                    switch (_typeCode) {
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
                    _data.SetData(i, value);
                }
            }

            private class ArrayData<T> : ArrayData {
                private T[] _data;
                private int _count;
                private GCHandle? _dataHandle;

                public ArrayData() {
                    _data = new T[8];
                    GC.SuppressFinalize(this);
                }

                ~ArrayData() {
                    Debug.Assert(_dataHandle.HasValue);
                    _dataHandle.Value.Free();
                }

                public override object GetData(int index) {
                    return _data[index];
                }

                public override void SetData(int index, object value) {
                    _data[index] = GetValue(value);
                }

                private static T GetValue(object value) {
                    if (!(value is T)) {
                        object newVal;
                        if (!Converter.TryConvert(value, typeof(T), out newVal)) {
                            if (value != null && typeof(T).IsPrimitive && typeof(T) != typeof(char))
                                throw PythonOps.OverflowError("couldn't convert {1} to {0}",
                                    DynamicHelpers.GetPythonTypeFromType(typeof(T)).Name,
                                    DynamicHelpers.GetPythonType(value).Name);
                            throw PythonOps.TypeError("expected {0}, got {1}",
                                DynamicHelpers.GetPythonTypeFromType(typeof(T)).Name,
                                DynamicHelpers.GetPythonType(value).Name);
                        }
                        value = newVal;
                    }
                    return (T)value;
                }

                public override void Append(object value) {
                    EnsureSize(_count + 1);
                    _data[_count++] = GetValue(value);
                }

                private void EnsureSize(int size) {
                    if (_data.Length < size) {
                        Array.Resize(ref _data, _data.Length * 2);
                        if (_dataHandle != null) {
                            _dataHandle.Value.Free();
                            _dataHandle = null;
                            GC.SuppressFinalize(this);
                        }
                    }
                }

                public override int Count(object value) {
                    T other = GetValue(value);

                    int count = 0;
                    for (int i = 0; i < _count; i++) {
                        if (_data[i].Equals(other)) {
                            count++;
                        }
                    }
                    return count;
                }

                public override void Insert(int index, object value) {
                    EnsureSize(_count + 1);
                    if (index < _count) {
                        Array.Copy(_data, index, _data, index + 1, _count - index);
                    }
                    _data[index] = GetValue(value);
                    _count++;
                }

                public override int Index(object value) {
                    T other = GetValue(value);

                    for (int i = 0; i < _count; i++) {
                        if (_data[i].Equals(other)) return i;
                    }
                    return -1;
                }

                public override void Remove(object value) {
                    T other = GetValue(value);

                    for (int i = 0; i < _count; i++) {
                        if (_data[i].Equals(other)) {
                            RemoveAt(i);
                            return;
                        }
                    }
                    throw PythonOps.ValueError("couldn't find value to remove");
                }

                public override void RemoveAt(int index) {
                    _count--;
                    if (index < _count) {
                        Array.Copy(_data, index + 1, _data, index, _count - index);
                    }
                }

                public override void Swap(int x, int y) {
                    T temp = _data[x];
                    _data[x] = _data[y];
                    _data[y] = temp;
                }

                public override int Length {
                    get {
                        return _count;
                    }
                }

                public override void Clear() {
                    _count = 0;
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

                public override IntPtr GetAddress() {
                    // slightly evil to pin our data array but it's only used in rare
                    // interop cases.  If this becomes a problem we can move the allocation
                    // onto the unmanaged heap if we have full trust via a different subclass
                    // of ArrayData.
                    if (!_dataHandle.HasValue) {
                        _dataHandle = GCHandle.Alloc(_data, GCHandleType.Pinned);
                        GC.ReRegisterForFinalize(this);
                    }
                    return _dataHandle.Value.AddrOfPinnedObject();
                }
            }

            #region IValueEquality Members

            int IValueEquality.GetValueHashCode() {
                throw PythonOps.TypeError("unhashable type");
            }

            bool IValueEquality.ValueEquals(object other) {
                PythonArray pa = other as PythonArray;
                if (pa == null) return false;

                if (_data.Length != pa._data.Length) return false;
                for (int i = 0; i < _data.Length; i++) {
                    if (!_data.GetData(i).Equals(pa._data.GetData(i))) {
                        return false;
                    }
                }

                return true;
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator() {
                for (int i = 0; i < _data.Length; i++) {
                    yield return _data.GetData(i);
                }
            }

            #endregion

            #region ICodeFormattable Members

            public virtual string/*!*/ __repr__(CodeContext/*!*/ context) {
                string res = "array('" + typecode.ToString() + "'";
                if (_data.Length == 0) {
                    return res + ")";
                }

                StringBuilder sb = new StringBuilder(res);
                if (_typeCode == 'c') {
                    sb.Append(", '");
                    for (int i = 0; i < _data.Length; i++) {
                        sb.Append((char)_data.GetData(i));
                    }
                    sb.Append("')");
                } else {
                    sb.Append(", [");
                    for (int i = 0; i < _data.Length; i++) {
                        if (i > 0) {
                            sb.Append(", ");
                        }
                        sb.Append(PythonOps.Repr(context, this[i]).ToString());
                    }
                    sb.Append("])");
                }

                return sb.ToString();
            }

            #endregion

            #region IWeakReferenceable Members

            WeakRefTracker IWeakReferenceable.GetWeakRef() {
                return _tracker;
            }

            bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
                _tracker = value;
                return true;
            }

            void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
                _tracker = value;
            }

            #endregion

            #region IPythonContainer Members

            public int __len__() {
                return _data.Length;
            }

            public bool __contains__(object value) {
                return _data.Index(value) != -1;
            }

            #endregion

            #region IRichComparable Members

            private bool TryCompare(object other, out int res) {
                PythonArray pa = other as PythonArray;
                if (pa == null || pa.typecode != typecode) {
                    res = 0;
                    return false;
                }

                if (pa._data.Length != _data.Length) {
                    res = _data.Length - pa._data.Length;
                } else {
                    res = 0;
                    for (int i = 0; i < pa._data.Length && res == 0; i++) {
                        res = PythonOps.Compare(_data.GetData(i), pa._data.GetData(i));
                    }
                }

                return true;
            }

            [return: MaybeNotImplemented]
            public object __cmp__(object other) {
                int res;
                if (!TryCompare(other, out res)) {
                    return NotImplementedType.Value;
                }

                return ScriptingRuntimeHelpers.Int32ToObject(res);
            }

            [return: MaybeNotImplemented]
            public static object operator >(PythonArray self, object other) {
                int res;
                if (!self.TryCompare(other, out res)) {
                    return NotImplementedType.Value;
                }

                return ScriptingRuntimeHelpers.BooleanToObject(res > 0);
            }

            [return: MaybeNotImplemented]
            public static object operator <(PythonArray self, object other) {
                int res;
                if (!self.TryCompare(other, out res)) {
                    return NotImplementedType.Value;
                }

                return ScriptingRuntimeHelpers.BooleanToObject(res < 0);
            }

            [return: MaybeNotImplemented]
            public static object operator >=(PythonArray self, object other) {
                int res;
                if (!self.TryCompare(other, out res)) {
                    return NotImplementedType.Value;
                }

                return ScriptingRuntimeHelpers.BooleanToObject(res >= 0);
            }

            [return: MaybeNotImplemented]
            public static object operator <=(PythonArray self, object other) {
                int res;
                if (!self.TryCompare(other, out res)) {
                    return NotImplementedType.Value;
                }

                return ScriptingRuntimeHelpers.BooleanToObject(res <= 0);
            }

            #endregion

            #region ISequence Members

            object ISequence.__getslice__(int start, int stop) {
                return this[new Slice(start, stop)];
            }

            #endregion

            #region ICollection Members

            void ICollection.CopyTo(Array array, int index) {
                throw new NotImplementedException();
            }

            int ICollection.Count {
                get { return __len__(); }
            }

            bool ICollection.IsSynchronized {
                get { return false; }
            }

            object ICollection.SyncRoot {
                get { return this; }
            }

            #endregion
        }
    }
}
