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
using System.Collections;
using System.IO;
using System.Text;
using IronPython.Runtime;

using IronPython.Runtime.Operations;
using Microsoft.Scripting;

[assembly: PythonModule("cStringIO", typeof(IronPython.Modules.PythonStringIO))]
namespace IronPython.Modules {
    class StringStream {
        private string _data;
        private int _position;
        private int _length;

        public StringStream(string data) {
            this._data = data;
            this._position = 0;
            this._length = data == null ? 0 : data.Length;
        }

        public bool EOF {
            get { return _position >= _length; }
        }

        public int Position {
            get { return _position; }
        }

        public string Data {
            get {
                return _data;
            }
            set {
                _data = value;
                if (_data == null) {
                    _length = _position = 0;
                } else {
                    _length = _data.Length;
                    if (_position > _length) {
                        _position = _length;
                    }
                }
            }
        }

        public string Prefix {
            get {
                return _data.Substring(0, _position);
            }
        }

        public int Read() {
            if (_position < _length) {
                return _data[_position++];
            } else {
                return -1;
            }
        }

        public string Read(int i) {
            if (_position + i > _length) {
                i = _length - _position;
            }
            string ret = _data.Substring(_position, i);
            _position += i;
            return ret;
        }

        public string ReadLine() {
            int i = _position;
            while (i < _length) {
                char c = _data[i];
                if (c == '\n' || c == '\r') {
                    i++;
                    if (c == '\r' && _position < _length && _data[i] == '\n') {
                        i++;
                    }
                    // preserve newline character like StringIO

                    string res = _data.Substring(_position, i - _position);
                    _position = i;
                    return res;
                }
                i++;
            }

            if (i > _position) {
                string res = _data.Substring(_position, i - _position);
                _position = i;
                return res;
            }

            return "";
        }

        public string ReadToEnd() {
            if (_position < _length) {
                string res = _data.Substring(_position);
                _position = _length;
                return res;
            } else return "";
        }

        public void Reset() {
            _position = 0;
        }

        public int Seek(int offset, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin:
                    _position = offset; break;
                case SeekOrigin.Current:
                    _position = _position + offset; break;
                case SeekOrigin.End:
                    _position = _length + offset; break;
                default:
                    throw new ArgumentException("origin");
            }
            return _position;
        }

        public void Truncate() {
            _data = _data.Substring(0, _position);
            _length = _data.Length;
        }

        public void Truncate(int size) {
            _data = _data.Substring(0, size);
            _position = size;
            _length = _data.Length;
        }

        internal void Write(string s) {
            string newData;
            int newPosition;
            if (_position > 0) {
                newData = _data.Substring(0, _position) + s;
            } else {
                newData = s;
            }
            newPosition = newData.Length;
            if (_position + s.Length < _length) {
                newData = newData + _data.Substring(_position + s.Length);
            }

            _data = newData;
            _position = newPosition;
            _length = _data.Length;
        }
    }

    [PythonType("cStringIO")]
    public static class PythonStringIO {
        public static object InputType = DynamicHelpers.GetDynamicType(typeof(StringI));
        public static object OutputType = DynamicHelpers.GetDynamicType(typeof(StringO));

        [PythonType("StringI")]
        public class StringI {
            private StringStream _sr;

            internal StringI(string data) {
                _sr = new StringStream(data);
            }

            [PythonName("close")]
            public void Close() {
                _sr = null;
            }

            public bool Closed {
                [PythonName("closed")]
                get {
                    return _sr == null;
                }
            }

            [PythonName("flush")]
            public void Flush() {
                ThrowIfClosed();
            }

            [PythonName("getvalue")]
            public string GetValue() {
                ThrowIfClosed();
                return _sr.Data;
            }

            [PythonName("getvalue")]
            public string GetValue(bool usePos) {
                return _sr.Prefix;
            }

            [PythonName("__iter__")]
            public object Iter() {
                return this;
            }

            [PythonName("next")]
            public string Next() {
                ThrowIfClosed();
                if (_sr.EOF) {
                    throw PythonOps.StopIteration();
                }
                return ReadLine();
            }

            [PythonName("read")]
            public string Read() {
                ThrowIfClosed();
                return _sr.ReadToEnd();
            }

            [PythonName("read")]
            public string Read(int s) {
                ThrowIfClosed();
                return _sr.Read(s);
            }

            [PythonName("readline")]
            public string ReadLine() {
                ThrowIfClosed();
                return _sr.ReadLine();
            }

            [PythonName("readlines")]
            public List ReadLines() {
                ThrowIfClosed();
                List list = List.Make();
                while (!_sr.EOF) {
                    list.AddNoLock(ReadLine());
                }
                return list;
            }

            [PythonName("readlines")]
            public List ReadLines(int size) {
                ThrowIfClosed();
                List list = List.Make();
                while (!_sr.EOF) {
                    string line = ReadLine();
                    list.AddNoLock(line);
                    if (line.Length >= size) break;
                    size -= line.Length;
                }
                return list;
            }

            [PythonName("reset")]
            public void Reset() {
                ThrowIfClosed();
                _sr.Reset();
            }

            [PythonName("seek")]
            public void Seek(int position) {
                Seek(position, 0);
            }

            [PythonName("seek")]
            public void Seek(int position, int mode) {
                ThrowIfClosed();
                SeekOrigin so;
                switch (mode) {
                    case 1: so = SeekOrigin.Current; break;
                    case 2: so = SeekOrigin.End; break;
                    default: so = SeekOrigin.Begin; break;
                }
                _sr.Seek(position, so);
            }

            [PythonName("tell")]
            public int Tell() {
                ThrowIfClosed();
                return _sr.Position;
            }

            [PythonName("truncate")]
            public void Truncate() {
                ThrowIfClosed();
                _sr.Truncate();
            }

            [PythonName("truncate")]
            public void Truncate(int size) {
                ThrowIfClosed();
                _sr.Truncate(size);
            }

            private void ThrowIfClosed() {
                if (Closed) {
                    throw PythonOps.ValueError("I/O operation on closed file");
                }
            }
        }

        [PythonType("StringO")]
        public class StringO {
            private StringWriter _sw = new StringWriter();
            private StringStream _sr = new StringStream("");
            private int _softspace;

            internal StringO() {
            }

            [PythonName("__iter__")]
            public object Iter() {
                return this;
            }

            [PythonName("close")]
            public void Close() {
                if (_sw != null) { _sw.Close(); _sw = null; }
                if (_sr != null) { _sr = null; }
            }

            public bool Closed {
                [PythonName("closed")]
                get {
                    return _sw == null || _sr == null;
                }
            }

            [PythonName("flush")]
            public void Flush() {
                FixStreams();
            }

            [PythonName("getvalue")]
            public string GetValue() {
                ThrowIfClosed();
                FixStreams();
                return _sr.Data;
            }

            [PythonName("getvalue")]
            public string GetValue(bool usePos) {
                ThrowIfClosed();
                FixStreams();
                return _sr.Prefix;
            }

            [PythonName("next")]
            public string Next() {
                ThrowIfClosed();
                FixStreams();
                if (_sr.EOF) {
                    throw PythonOps.StopIteration();
                }
                return ReadLine();
            }

            [PythonName("read")]
            public string Read() {
                ThrowIfClosed();
                FixStreams();
                return _sr.ReadToEnd();
            }

            [PythonName("read")]
            public string Read(int i) {
                ThrowIfClosed();
                FixStreams();
                return _sr.Read(i);
            }

            [PythonName("readline")]
            public string ReadLine() {
                ThrowIfClosed();
                FixStreams();
                return _sr.ReadLine();
            }

            [PythonName("readlines")]
            public List ReadLines() {
                ThrowIfClosed();
                List list = List.Make();
                while (!_sr.EOF) {
                    list.AddNoLock(ReadLine());
                }
                return list;
            }

            [PythonName("readlines")]
            public List ReadLines(int size) {
                ThrowIfClosed();
                List list = List.Make();
                while (!_sr.EOF) {
                    string line = ReadLine();
                    list.AddNoLock(line);
                    if (line.Length >= size) break;
                    size -= line.Length;
                }
                return list;
            }

            [PythonName("reset")]
            public void Reset() {
                ThrowIfClosed();
                FixStreams();
                _sr.Reset();
            }

            [PythonName("seek")]
            public void Seek(int position) {
                Seek(position, 0);
            }

            [PythonName("seek")]
            public void Seek(int offset, int origin) {
                ThrowIfClosed();
                FixStreams();
                SeekOrigin so;
                switch (origin) {
                    case 1: so = SeekOrigin.Current; break;
                    case 2: so = SeekOrigin.End; break;
                    default: so = SeekOrigin.Begin; break;
                }
                _sr.Seek(offset, so);
            }

            public int SoftSpace {
                [PythonName("softspace")]
                get { return _softspace; }
                [PythonName("softspace")]
                set { _softspace = value; }
            }

            [PythonName("tell")]
            public int Tell() {
                ThrowIfClosed();
                FixStreams();
                return _sr.Position;
            }

            [PythonName("truncate")]
            public void Truncate() {
                ThrowIfClosed();
                FixStreams();
                _sr.Truncate();
            }

            [PythonName("truncate")]
            public void Truncate(int size) {
                ThrowIfClosed();
                FixStreams();
                _sr.Truncate(size);
            }

            [PythonName("write")]
            public void Write(string s) {
                ThrowIfClosed();
                _sw.Write(s);
            }

            [PythonName("writelines")]
            public void WriteLines(object o) {
                ThrowIfClosed();
                IEnumerator e = PythonOps.GetEnumerator(o);
                while (e.MoveNext()) {
                    string s = e.Current as string;
                    if (s == null) {
                        throw PythonOps.ValueError("string expected");
                    }
                    Write(s);
                }
            }

            private void FixStreams() {
                if (_sr != null) {
                    StringBuilder sb = _sw.GetStringBuilder();
                    if (sb != null && sb.Length > 0) {
                        _sr.Write(sb.ToString());
                        sb.Length = 0;
                    }
                }
            }

            private void ThrowIfClosed() {
                if (Closed) {
                    throw PythonOps.ValueError("I/O operation on closed file");
                }
            }
        }

        [PythonName("StringIO")]
        public static object StringIO() {
            return new StringO();
        }

        [PythonName("StringIO")]
        public static object StringIO(string data) {
            return new StringI(data);
        }
    }
}
