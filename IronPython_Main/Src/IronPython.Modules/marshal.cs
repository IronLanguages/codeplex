/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Scripting.Runtime;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Math;

[assembly: PythonModule("marshal", typeof(IronPython.Modules.PythonMarshal))]
namespace IronPython.Modules {
    public static class PythonMarshal {

        #region Public marshal APIs
        public static void dump(object value, object file) {
            dump(value, file, null);
        }

        public static void dump(object value, object file, object version) {
            PythonFile pf = file as PythonFile;
            if (pf == null) throw PythonOps.TypeErrorForBadInstance("expected file, found '{0}'", file);

            pf.write(dumps(value, version));
        }

        public static object load(object file) {
            PythonFile pf = file as PythonFile;
            if (pf == null) throw PythonOps.TypeErrorForBadInstance("expected file, found '{0}'", file);

            return loads(pf.read());
        }

        public static object dumps(object value) {
            return dumps(value, null);
        }

        public static string dumps(object value, object version) {
            byte[] bytes = ObjectToBytes(value);
            StringBuilder sb = new StringBuilder(bytes.Length);
            for (int i = 0; i < bytes.Length; i++) {
                sb.Append((char)bytes[i]);
            }
            return sb.ToString();
        }

        public static object loads(string @string) {
            string strParam = @string;

            byte[] bytes = new byte[strParam.Length];
            for (int i = 0; i < strParam.Length; i++) {
                bytes[i] = (byte)strParam[i];
            }
            return BytesToObject(bytes);
        }

        public const int version = 1;
        #endregion

        #region Implementation details

        private static byte[] ObjectToBytes(object o) {
            MarshalWriter mw = new MarshalWriter();
            mw.WriteObject(o);
            return mw.GetBytes();
        }

        private static object BytesToObject(byte[] bytes) {
            MarshalReader mr = new MarshalReader(bytes);
            return mr.ReadObject();
        }

        /*********************************************************
         * Format 
         *  
         * Tuple: '(',int cnt,tuple items
         * List:  '[',int cnt, list items
         * Dict:  '{',key item, value item, '0' terminator
         * Int :  'i',4 bytes
         * float: 'f', 1 byte len, float in string
         * BigInt:  'l', int encodingSize
         *      if the value is negative then the size is negative
         *      and needs to be subtracted from int.MaxValue
         * 
         *      the bytes are encoded in 15 bit multiples, a size of
         *      0 represents a value of zero.
         * 
         * True: 'T'
         * False: 'F'
         * Float: 'f', str len, float in str
         * string: 't', int len, bytes  (ascii)
         * string: 'u', int len, bytes (unicode)
         * StopIteration: 'S'   
         * None: 'N'
         * Long: 'I' followed by 8 bytes (little endian 64-bit value)
         * complex: 'x', byte len, real str, byte len, imag str
         * Buffer (array.array too, but they don't round trip): 's', int len, buffer bytes
         * code: 'c', more stuff...
         * 
         */
        private class MarshalWriter {
            List<byte> _bytes;

            public MarshalWriter() {
                _bytes = new List<byte>();
            }

            public void WriteObject(object o) {
                List<object> infinite = PythonOps.GetReprInfinite();

                if (infinite.Contains(o)) throw PythonOps.ValueError("Marshaled data contains infinite cycle");

                int index = infinite.Count;
                infinite.Add(o);
                try {
                    if (o == null) _bytes.Add((byte)'N');
                    else if (o == RuntimeHelpers.True) _bytes.Add((byte)'T');
                    else if (o == RuntimeHelpers.False) _bytes.Add((byte)'F');
                    else if (o is string) WriteString(o as string);
                    else if (o is int) WriteInt((int)o);
                    else if (o is float) WriteFloat((float)o);
                    else if (o is double) WriteFloat((double)o);
                    else if (o is long) WriteLong((long)o);
                    else if (o is List) WriteList(o);
                    else if (o is PythonDictionary) WriteDict(o);
                    else if (o is PythonTuple) WriteTuple(o);
                    else if (o is SetCollection) WriteSet(o);
                    else if (o is FrozenSetCollection) WriteFrozenSet(o);
                    else if (o is BigInteger) WriteInteger((BigInteger)o);
                    else if (o is Complex64) WriteComplex((Complex64)o);
                    else if (o is PythonBuffer) WriteBuffer((PythonBuffer)o);
                    else if (o == PythonExceptions.StopIteration) WriteStopIteration();
                    else throw PythonOps.ValueError("unmarshallable object");
                } finally {
                    infinite.RemoveAt(index);
                }
            }

            private void WriteFloat(float f) {
                _bytes.Add((byte)'f');
                WriteFloatString(f);
            }

            private void WriteFloat(double f) {
                _bytes.Add((byte)'f');
                WriteDoubleString(f);
            }

            private void WriteFloatString(float f) {
                string s = f.ToString("G17");   // get maximum percision
                _bytes.Add((byte)s.Length);
                for (int i = 0; i < s.Length; i++) {
                    _bytes.Add((byte)s[i]);
                }
            }

            private void WriteDoubleString(double d) {
                string s = d.ToString("G17");  // get maximum percision
                _bytes.Add((byte)s.Length);
                for (int i = 0; i < s.Length; i++) {
                    _bytes.Add((byte)s[i]);
                }
            }

            private void WriteInteger(BigInteger val) {
                if (val == BigInteger.Zero) {
                    _bytes.Add((byte)'l');
                    for(int i = 0; i<4; i++) _bytes.Add(0);
                    return;
                }

                BigInteger mask = BigInteger.Create(short.MaxValue);
                uint startLen = (uint)val.Length;
                val = new BigInteger(val);

                _bytes.Add((byte)'l');
                uint byteLen = ((startLen * 32) + 14) / 15; // len is in 32-bit multiples, we want 15-bit multiples
                bool fNeg = false;
                if (val < 0) {
                    fNeg = true;
                    val *= -1;
                }

                if (val <= short.MaxValue)
                    byteLen = 1;
                else if (val < (1 << 30)) {
                    byteLen = 2;
                }

                // write out length
                if (fNeg) {
                    WriteUInt32(uint.MaxValue - byteLen + 1);
                } else {
                    WriteUInt32(byteLen);
                }

                // write out value (15 bits at a time)
                while (val != 0) {
                    BigInteger res = (val & mask);
                    uint writeVal = res.ToUInt32();
                    _bytes.Add((byte)((writeVal) & 0xff));
                    _bytes.Add((byte)((writeVal >> 8) & 0xff));
                    val = val >> 15;
                }
            }

            private void WriteBuffer(PythonBuffer b) {
                _bytes.Add((byte)'s');
                List<byte> newBytes = new List<byte>();
                for (int i = 0; i < b.Size; i++) {
                    if (b[i] is string) {
                        string str = b[i] as string;
                        byte[] utfBytes = Encoding.UTF8.GetBytes(str);
                        if (utfBytes.Length != str.Length) {
                            newBytes.AddRange(utfBytes);
                        } else {
                            byte[] strBytes = PythonAsciiEncoding.Instance.GetBytes(str);
                            newBytes.AddRange(strBytes);
                        }
                    } else {
                        newBytes.Add((byte)b[i]);
                    }
                }
                WriteInt32(newBytes.Count);
                _bytes.AddRange(newBytes);
            }

            private void WriteLong(long l) {
                _bytes.Add((byte)'I');

                for (int i = 0; i < 8; i++) {
                    _bytes.Add((byte)(l & 0xff));
                    l = l >> 8;
                }
            }

            private void WriteComplex(Complex64 val) {
                _bytes.Add((byte)'x');
                WriteDoubleString(val.Real);
                WriteDoubleString(val.Imag);
            }

            private void WriteStopIteration() {
                _bytes.Add((byte)'S');
            }

            private void WriteInt(int val) {
                _bytes.Add((byte)'i');
                WriteInt32(val);
            }

            private void WriteInt32(int val) {
                BitConverter.GetBytes(val);
                _bytes.Add((byte)(val & 0xff));
                _bytes.Add((byte)((val >> 8) & 0xff));
                _bytes.Add((byte)((val >> 16) & 0xff));
                _bytes.Add((byte)((val >> 24) & 0xff));
            }

            private void WriteUInt32(uint val) {
                _bytes.Add((byte)(val & 0xff));
                _bytes.Add((byte)((val >> 8) & 0xff));
                _bytes.Add((byte)((val >> 16) & 0xff));
                _bytes.Add((byte)((val >> 24) & 0xff));
            }

            private void WriteString(string s) {
                byte[] utfBytes = Encoding.UTF8.GetBytes(s);
                if (utfBytes.Length != s.Length) {
                    _bytes.Add((byte)'u');
                    WriteInt32(utfBytes.Length);
                    for (int i = 0; i < utfBytes.Length; i++) {
                        _bytes.Add(utfBytes[i]);
                    }
                } else {
                    byte[] strBytes = PythonAsciiEncoding.Instance.GetBytes(s);
                    _bytes.Add((byte)'t');
                    WriteInt32(strBytes.Length);
                    for (int i = 0; i < strBytes.Length; i++) {
                        _bytes.Add(strBytes[i]);
                    }
                }
            }

            private void WriteList(object o) {
                List l = o as List;
                _bytes.Add((byte)'[');
                WriteInt32(l.__len__());
                for (int i = 0; i < l.__len__(); i++) {
                    WriteObject(l[i]);
                }
            }

            private void WriteDict(object o) {
                PythonDictionary d = o as PythonDictionary;
                _bytes.Add((byte)'{');
                IEnumerator<KeyValuePair<object, object>> ie = ((IEnumerable<KeyValuePair<object, object>>)d).GetEnumerator();
                while (ie.MoveNext()) {
                    WriteObject(ie.Current.Key);
                    WriteObject(ie.Current.Value);
                }
                _bytes.Add((byte)'0');
            }

            private void WriteTuple(object o) {
                PythonTuple t = o as PythonTuple;
                _bytes.Add((byte)'(');
                WriteInt32(t.__len__());
                for (int i = 0; i < t.__len__(); i++) {
                    WriteObject(t[i]);
                }
            }

            private void WriteSet(object set) {
                SetCollection s = set as SetCollection;
                _bytes.Add((byte)'<');
                WriteInt32(s.__len__());
                foreach(object o in s) {
                    WriteObject(o);
                }
            }

            private void WriteFrozenSet(object set) {
                FrozenSetCollection s = set as FrozenSetCollection;
                _bytes.Add((byte)'>');
                WriteInt32(s.__len__());
                foreach (object o in s) {
                    WriteObject(o);
                }
            }

            public byte[] GetBytes() {
                return _bytes.ToArray();
            }
        }

        private class MarshalReader {
            byte[] _myBytes;
            int _curIndex;
            Stack<ProcStack> _stack;
            object _result;

            public MarshalReader(byte[] bytes) {
                _myBytes = bytes;
            }

            public object ReadObject() {
                while (_curIndex < _myBytes.Length) {
                    object res;
                    if (_myBytes[_curIndex] == '(') {
                        PushStack(StackType.Tuple);
                    } else if (_myBytes[_curIndex] == '[') {
                        PushStack(StackType.List);
                    } else if (_myBytes[_curIndex] == '{') {
                        PushStack(StackType.Dict);
                    } else if (_myBytes[_curIndex] == '<') {
                        PushStack(StackType.Set);
                    } else if (_myBytes[_curIndex] == '>') {
                        PushStack(StackType.FrozenSet);
                        /*} else if (myBytes[curIndex] == 'c') {*/
                    } else {
                        res = YieldSimple();
                        if (_stack == null) {
                            return res;
                        }

                        do {
                            res = UpdateStack(res);
                        } while (res != null && _stack.Count > 0);

                        if (_stack.Count == 0) {
                            break;
                        }
                        continue;
                    }

                    // handle empty lists/tuples...
                    if (_stack != null && _stack.Count > 0 && _stack.Peek().StackCount == 0) {
                        ProcStack ps = _stack.Pop();
                        res = ps.StackObj;

                        if (ps.StackType == StackType.Tuple) {
                            res = PythonTuple.Make(res);
                        } else if (ps.StackType == StackType.FrozenSet) {
                            res = FrozenSetCollection.Make(res);
                        }

                        if (_stack.Count > 0) {
                            // empty list/tuple
                            do {
                                res = UpdateStack(res);
                            } while (res != null && _stack.Count > 0);
                            if (_stack.Count == 0) break;
                        } else {
                            _result = res;
                            break;
                        }
                    }
                }

                return _result;
            }

            private void PushStack(StackType type) {
                ProcStack newStack = new ProcStack();
                newStack.StackType = type;
                _curIndex++;

                switch (type) {
                    case StackType.Dict:
                        newStack.StackObj = new PythonDictionary();

                        if (_curIndex == _myBytes.Length) throw PythonOps.EofError("EOF read where object expected");

                        if (_myBytes[_curIndex] == '0')
                            newStack.StackCount = 0;
                        else 
                            newStack.StackCount = -1;

                        break;
                    case StackType.List:
                        newStack.StackObj = new List();
                        newStack.StackCount = ReadInt32();
                        break;
                    case StackType.Tuple:
                        newStack.StackCount = ReadInt32();
                        newStack.StackObj = new List<object>(newStack.StackCount);
                        break;
                    case StackType.Set:
                        newStack.StackObj = new SetCollection();
                        newStack.StackCount = ReadInt32();
                        break;
                    case StackType.FrozenSet:
                        newStack.StackCount = ReadInt32();
                        newStack.StackObj = new List<object>(newStack.StackCount);
                        break;
                }

                if (_stack == null) _stack = new Stack<ProcStack>();

                _stack.Push(newStack);
            }

            private object UpdateStack(object res) {
                ProcStack curStack = _stack.Peek();
                switch (curStack.StackType) {
                    case StackType.Dict:
                        PythonDictionary od = curStack.StackObj as PythonDictionary;
                        if (curStack.HaveKey) {
                            od[curStack.Key] = res;
                            curStack.HaveKey = false;
                        } else {
                            curStack.HaveKey = true;
                            curStack.Key = res;
                        }

                        if (_curIndex == _myBytes.Length) throw PythonOps.EofError("EOF read where object expected");
                        if (_myBytes[_curIndex] == '0') {
                            _stack.Pop();
                            if (_stack.Count == 0) {
                                _result = od;
                            }
                            return od;
                        }
                        break;
                    case StackType.Tuple:
                        List<object> objs = curStack.StackObj as List<object>;
                        objs.Add(res);
                        curStack.StackCount--;
                        if (curStack.StackCount == 0) {
                            _stack.Pop();
                            object tuple = PythonTuple.Make(objs);
                            if (_stack.Count == 0) {
                                _result = tuple;
                            }
                            return tuple;
                        }
                        break;
                    case StackType.List:
                        List ol = curStack.StackObj as List;
                        ol.AddNoLock(res);
                        curStack.StackCount--;
                        if (curStack.StackCount == 0) {
                            _stack.Pop();
                            if (_stack.Count == 0) {
                                _result = ol;
                            }
                            return ol;
                        }
                        break;
                    case StackType.Set:
                        SetCollection os = curStack.StackObj as SetCollection;
                        os.add(res);
                        curStack.StackCount--;
                        if (curStack.StackCount == 0) {
                            _stack.Pop();
                            if (_stack.Count == 0) {
                                _result = os;
                            }
                            return os;
                        }
                        break;
                    case StackType.FrozenSet:
                        List<object> ofs = curStack.StackObj as List<object>;
                        ofs.Add(res);
                        curStack.StackCount--;
                        if (curStack.StackCount == 0) {
                            _stack.Pop();
                            object frozenSet = FrozenSetCollection.Make(ofs);
                            if (_stack.Count == 0) {
                                _result = frozenSet;
                            }
                            return frozenSet;
                        }
                        break;
                }
                return null;
            }

            private enum StackType {
                Tuple,
                Dict,
                List,
                Set,
                FrozenSet
            }

            private class ProcStack {
                public StackType StackType;
                public object StackObj;
                public int StackCount;
                public bool HaveKey;
                public object Key;
            }

            private object YieldSimple() {
                object res;
                switch ((char)_myBytes[_curIndex++]) {
                    // simple ops to be read in
                    case 'i': res = ReadInt(); break;
                    case 'l': res = ReadBigInteger(); break;
                    case 'T': res = RuntimeHelpers.True; break;
                    case 'F': res = RuntimeHelpers.False; break;
                    case 'f': res = ReadFloat(); break;
                    case 't': res = ReadAsciiString(); break;
                    case 'u': res = ReadUnicodeString(); break;
                    case 'S': res = PythonExceptions.StopIteration; break;
                    case 'N': res = null; break;
                    case 'x': res = ReadComplex(); break;
                    case 's': res = ReadBuffer(); break;
                    case 'I': res = ReadLong(); break;
                    default: throw PythonOps.ValueError("bad marshal data");
                }
                return res;
            }

            private int ReadInt32() {
                if (_curIndex + 3 >= _myBytes.Length) throw PythonOps.ValueError("bad marshal data");

                int res = _myBytes[_curIndex] |
                    (_myBytes[_curIndex + 1] << 8) |
                    (_myBytes[_curIndex + 2] << 16) |
                    (_myBytes[_curIndex + 3] << 24);

                _curIndex += 4;
                return res;
            }

            private double ReadFloatStr() {
                if (_curIndex >= _myBytes.Length) throw PythonOps.EofError("EOF read where object expected");


                int len = _myBytes[_curIndex];
                _curIndex++;
                if ((_curIndex + len) > _myBytes.Length) throw PythonOps.EofError("EOF read where object expected");

                string str = PythonAsciiEncoding.Instance.GetString(_myBytes, _curIndex, len);

                _curIndex += len;
                double res = 0;
#if !SILVERLIGHT        // Double.Parse
                double.TryParse(str, out res);
#else
                try { res = double.Parse(str); } catch { }
#endif
                return res;
            }

            object ReadInt() {
                // bytes not present are treated as being -1
                byte b1, b2, b3, b4;

                switch (_myBytes.Length - _curIndex) {
                    case 0: b1 = 255; b2 = 255; b3 = 255; b4 = 255; break;
                    case 1: b1 = _myBytes[_curIndex]; b2 = 255; b3 = 255; b4 = 255; break;
                    case 2: b1 = _myBytes[_curIndex]; b2 = _myBytes[_curIndex + 1]; b3 = 255; b4 = 255; break;
                    case 3: b1 = _myBytes[_curIndex]; b2 = _myBytes[_curIndex + 1]; b3 = _myBytes[_curIndex + 2]; b4 = 255; break;
                    default:
                        b1 = _myBytes[_curIndex];
                        b2 = _myBytes[_curIndex + 1];
                        b3 = _myBytes[_curIndex + 2];
                        b4 = _myBytes[_curIndex + 3];
                        break;
                }

                _curIndex += 4;
                byte[] bytes = new byte[] { b1, b2, b3, b4 };
                return RuntimeHelpers.Int32ToObject(BitConverter.ToInt32(bytes, 0));
                //return Ops.int2object(b1 | (b2 << 8) | (b3 << 16) | (b4 << 24));
            }

            object ReadFloat() {
                return ReadFloatStr();
            }

            private object ReadAsciiString() {
                int len = ReadInt32();
                if (len + _curIndex > _myBytes.Length) throw PythonOps.EofError("EOF read where object expected");

                string res = PythonAsciiEncoding.Instance.GetString(_myBytes, _curIndex, len);

                _curIndex += len;
                return res;
            }

            private object ReadUnicodeString() {
                int len = ReadInt32();
                if (len + _curIndex > _myBytes.Length) throw PythonOps.EofError("EOF read where object expected");

                string res = Encoding.UTF8.GetString(_myBytes, _curIndex, len);

                _curIndex += len;
                return res;
            }

            private object ReadComplex() {
                double real = ReadFloatStr();
                double imag = ReadFloatStr();

                return new Complex64(real, imag);
            }

            private object ReadBuffer() {
                int len = ReadInt32();

                if (len + _curIndex > _myBytes.Length) throw PythonOps.ValueError("bad marshal data");

                string res = Encoding.UTF8.GetString(_myBytes, _curIndex, len);

                _curIndex += len;

                return res;
            }

            private object ReadLong() {
                if (_curIndex + 8 > _myBytes.Length) throw PythonOps.ValueError("bad marshal data");

                long res = 0;
                for (int i = 0; i < 8; i++) {
                    res |= (((long)_myBytes[_curIndex++]) << (i * 8));
                }

                return res;
            }

            private object ReadBigInteger() {
                int encodingSize = ReadInt32();
                int sign = 1;
                if (encodingSize < 0) {
                    sign = -1;
                    encodingSize *= -1;
                }
                int len = encodingSize * 2;

                if (len + _curIndex > _myBytes.Length) throw PythonOps.ValueError("bad marshal data");

                // first read the values in shorts so we can work
                // with them as 15-bit bytes easier...
                short[] shData = new short[encodingSize];
                for (int i = 0; i < shData.Length; i++) {
                    shData[i] = (short)(_myBytes[_curIndex + i * 2] | (_myBytes[_curIndex + 1 + i * 2] << 8));
                }

                // then convert the short's into BigInteger's 32-bit 
                // format.
                uint[] numData = new uint[(shData.Length + 1) / 2];
                int bitWriteIndex = 0, shortIndex = 0, bitReadIndex = 0;
                while (shortIndex < shData.Length) {
                    short val = shData[shortIndex];
                    int shift = bitWriteIndex % 32;

                    if (bitReadIndex != 0) {
                        // we're read some bits, mask them off
                        // and adjust the shift.
                        int maskOff = ~((1 << bitReadIndex) - 1);
                        val = (short)(val & maskOff);
                        shift -= bitReadIndex;
                    }

                    // write the value into numData
                    if (shift < 0) {
                        numData[bitWriteIndex / 32] |= (uint)(val >> (shift * -1));
                    } else {
                        numData[bitWriteIndex / 32] |= (uint)(val << shift);
                    }

                    // and advance our indices
                    if ((bitWriteIndex % 32) <= 16) {
                        bitWriteIndex += (15 - bitReadIndex);
                        bitReadIndex = 0;
                        shortIndex++;
                    } else {
                        bitReadIndex = (32 - (bitWriteIndex % 32));
                        bitWriteIndex += bitReadIndex;
                    }
                }
                _curIndex += len;

                // and finally pass the data onto the big integer.
                return new BigInteger(sign, numData);
            }
        }

        #endregion
    }
}
