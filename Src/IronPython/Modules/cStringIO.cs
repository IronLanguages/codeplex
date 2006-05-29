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
using System.IO;
using System.Text;
using IronPython.Runtime;

[assembly: PythonModule("cStringIO", typeof(IronPython.Modules.PythonStringIO))]
namespace IronPython.Modules {
    public class StringStream {
        private string data;
        private int position;
        private int length;

        public StringStream(string data) {
            this.data = data;
            this.position = 0;
            this.length = data == null ? 0 : data.Length;
        }

        public bool EOF {
            get { return position >= length; }
        }

        public int Position {
            get { return position; }
        }

        public string Data {
            get {
                return data;
            }
            set {
                data = value;
                if (data == null) {
                    length = position = 0;
                } else {
                    length = data.Length;
                    if (position > length) {
                        position = length;
                    }
                }
            }
        }

        public string Prefix {
            get {
                return data.Substring(0, position);
            }
        }

        public int Read() {
            if (position < length) {
                return data[position++];
            } else {
                return -1;
            }
        }

        public string Read(int i) {
            if (position + i > length) {
                i = length - position;
            }
            string ret = data.Substring(position, i);
            position += i;
            return ret;
        }

        public string ReadLine() {
            int i = position;
            while (i < length) {
                char c = data[i];
                if (c == '\n' || c == '\r') {
                    i++;
                    if (c == '\r' && position < length && data[i] == '\n') {
                        i++;
                    }
                    // preserve newline character like StringIO

                    string res = data.Substring(position, i - position);
                    position = i;
                    return res;
                }
                i++;
            }

            if (i > position) {
                string res = data.Substring(position, i - position);
                position = i;
                return res;
            }

            return "";
        }

        public string ReadToEnd() {
            if (position < length) {
                string res = data.Substring(position);
                position = length;
                return res;
            } else return "";
        }

        public void Reset() {
            position = 0;
        }

        public int Seek(int offset, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin:
                    position = offset; break;
                case SeekOrigin.Current:
                    position = position + offset; break;
                case SeekOrigin.End:
                    position = length + offset; break;
                default:
                    throw new ArgumentException("origin");
            }
            return position;
        }

        public void Truncate() {
            data = data.Substring(0, position);
            length = data.Length;
        }

        public void Truncate(int size) {
            data = data.Substring(0, size);
            position = size;
            length = data.Length;
        }

        internal void Write(string s) {
            string newData;
            int newPosition;
            if (position > 0) {
                newData = data.Substring(0, position) + s;
            } else {
                newData = s;
            }
            newPosition = newData.Length;
            if (position + s.Length < length) {
                newData = newData + data.Substring(position + s.Length);
            }

            data = newData;
            position = newPosition;
            length = data.Length;
        }
    }

    public class StringI {
        private StringStream sr;

        internal StringI(string data) {
            sr = new StringStream(data);
        }

        [PythonName("close")]
        public void Close() {
            sr = null;
        }

        public bool Closed {
            [PythonName("closed")]
            get {
                return sr == null;
            }
        }

        [PythonName("flush")]
        public void Flush() {
            ThrowIfClosed();
        }

        [PythonName("getvalue")]
        public string GetValue() {
            ThrowIfClosed();
            return sr.Data;
        }

        [PythonName("getvalue")]
        public string GetValue(bool usePos) {
            return sr.Prefix;
        }

        [PythonName("isatty")]
        public int IsAtty() {
            return 0;
        }

        [PythonName("__iter__")]
        public object Iter() {
            return this;
        }

        [PythonName("next")]
        public string Next() {
            ThrowIfClosed();
            if (sr.EOF) {
                throw Ops.StopIteration();
            }
            return ReadLine();
        }

        [PythonName("read")]
        public string Read() {
            ThrowIfClosed();
            return sr.ReadToEnd();
        }

        [PythonName("read")]
        public string Read(int s) {
            ThrowIfClosed();
            return sr.Read(s);
        }

        [PythonName("readline")]
        public string ReadLine() {
            ThrowIfClosed();
            return sr.ReadLine();
        }

        [PythonName("readlines")]
        public List ReadLines() {
            ThrowIfClosed();
            List list = List.Make();
            while (!sr.EOF) {
                list.AddNoLock(ReadLine());
            }
            return list;
        }

        [PythonName("readlines")]
        public List ReadLines(int size) {
            ThrowIfClosed();
            List list = List.Make();
            while (!sr.EOF) {
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
            sr.Reset();
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
            sr.Seek(position, so);
        }

        [PythonName("tell")]
        public int Tell() {
            ThrowIfClosed();
            return sr.Position;
        }

        [PythonName("truncate")]
        public void Truncate() {
            ThrowIfClosed();
            sr.Truncate();
        }

        [PythonName("truncate")]
        public void Truncate(int size) {
            ThrowIfClosed();
            sr.Truncate(size);
        }

        private void ThrowIfClosed() {
            if (Closed) {
                throw Ops.ValueError("I/O operation on closed file");
            }
        }
    }

    public class StringO {
        private StringWriter sw = new StringWriter();
        private StringStream sr = new StringStream("");
        private int softspace;

        internal StringO() {
        }

        [PythonName("__iter__")]
        public object Iter() {
            return this;
        }

        [PythonName("close")]
        public void Close() {
            if (sw != null) { sw.Close(); sw = null; }
            if (sr != null) { sr = null; }
        }

        public bool Closed {
            [PythonName("closed")]
            get {
                return sw == null || sr == null;
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
            return sr.Data;
        }

        [PythonName("getvalue")]
        public string GetValue(bool usePos) {
            ThrowIfClosed();
            FixStreams();
            return sr.Prefix;
        }

        [PythonName("isatty")]
        public int IsAtty() {
            return 0;
        }

        [PythonName("next")]
        public string Next() {
            ThrowIfClosed();
            FixStreams();
            if (sr.EOF) {
                throw Ops.StopIteration();
            }
            return ReadLine();
        }

        [PythonName("read")]
        public string Read() {
            ThrowIfClosed();
            FixStreams();
            return sr.ReadToEnd();
        }

        [PythonName("read")]
        public string Read(int i) {
            ThrowIfClosed();
            FixStreams();
            return sr.Read(i);
        }

        [PythonName("readline")]
        public string ReadLine() {
            ThrowIfClosed();
            FixStreams();
            return sr.ReadLine();
        }

        [PythonName("readlines")]
        public List ReadLines() {
            ThrowIfClosed();
            List list = List.Make();
            while (!sr.EOF) {
                list.AddNoLock(ReadLine());
            }
            return list;
        }

        [PythonName("readlines")]
        public List ReadLines(int size) {
            ThrowIfClosed();
            List list = List.Make();
            while(!sr.EOF) {
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
            sr.Reset();
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
            sr.Seek(offset, so);
        }

        public int SoftSpace {
            [PythonName("softspace")]
            get { return softspace; }
            [PythonName("softspace")]
            set { softspace = value; }
        }

        [PythonName("tell")]
        public int Tell() {
            ThrowIfClosed();
            FixStreams();
            return sr.Position;
        }

        [PythonName("truncate")]
        public void Truncate() {
            ThrowIfClosed();
            FixStreams();
            sr.Truncate();
        }

        [PythonName("truncate")]
        public void Truncate(int size) {
            ThrowIfClosed();
            FixStreams();
            sr.Truncate(size);
        }

        [PythonName("write")]
        public void Write(string s) {
            ThrowIfClosed();
            sw.Write(s);
        }

        [PythonName("writelines")]
        public void WriteLines(object o) {
            ThrowIfClosed();
            IEnumerator e = Ops.GetEnumerator(o);
            while (e.MoveNext()) {
                string s = e.Current as string;
                if (s == null) {
                    throw Ops.ValueError("string expected");
                }
                Write(s);
            }
        }

        private void FixStreams() {
            if (sr != null) {
                StringBuilder sb = sw.GetStringBuilder();
                if (sb != null && sb.Length > 0) {
                    sr.Write(sb.ToString());
                    sb.Length = 0;
                }
            }
        }

        private void ThrowIfClosed() {
            if (Closed) {
                throw Ops.ValueError("I/O operation on closed file");
            }
        }
    }

    public static class PythonStringIO {
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