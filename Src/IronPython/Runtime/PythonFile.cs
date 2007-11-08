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
using System.Text;
using System.IO;

using Microsoft.Scripting;

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime {

    // The following set of classes is used to translate between pythonic file stream semantics and those of
    // the runtime and the underlying system.
    //
    // Python supports opening files in binary and text mode. Binary is fairly obvious: we want to preserve
    // the data as is, to the point where it should be possible to round-trip an arbitary binary file without
    // introducing corruptions.
    //
    // Text mode is more complex. Python further subdivides this class into the regular text mode where the
    // newline convention is defined by the underlying system, and universal newline mode where python will
    // treat '\n', '\r' and '\r\n' as equivalently terminating a line. In all these text modes reading from
    // the file will translate the associated newline format into '\n' and writing will convert '\n' back to
    // the original newline format.
    //
    // We want to support all these modes and also not tie ourselves to a particular platform. So although
    // Win32 always terminates lines with '\r\n' we want to support running on platforms where '\r' or '\n' is
    // the terminator as well. Further, we don't wish to bog down the performance of the implementation by
    // checking the newline semantics throughout the code. So instead we define abstract reader and writer
    // classes that roughly support the APIs and semantics that python needs and provide a set of
    // implementations of those classes that match the mode selected at runtime.
    //
    // The classes defined below have the following hierarchy:
    //
    //      PythonStreamReader          :: Abstract reader APIs
    //          PythonBinaryReader      :: Read binary data
    //          PythonTextCRLFReader    :: Read text data with lines terminated with '\r\n'
    //          PythonTextCRReader      :: Read text data with lines terminated with '\r'
    //          PythonTextLFReader      :: Read text data with lines terminated with '\n'
    //          PythonUniversalReader   :: Read text data with lines terminated with '\r\n', '\r' or '\n'
    //      PythonStreamWriter          :: Abstract writer APIs
    //          PythonBinaryWriter      :: Write binary data
    //          PythonTextCRLFWriter    :: Write text data with lines terminated with '\r\n'
    //          PythonTextCRWriter      :: Write text data with lines terminated with '\r'
    //          PythonTextLFWriter      :: Write text data with lines terminated with '\n'
    //
    // Note that there is no universal newline write mode since there's no reasonable way to define this.

    // The abstract reader API.
    internal abstract class PythonStreamReader {

        // All the readers use a stream as input and most (except the binary reader) use an encoding to define
        // the mapping from bytes into characters. So we factor that out in the base class.
        protected Stream _stream;
        protected Encoding _encoding;

        public Encoding Encoding { get { return _encoding; } }

        public PythonStreamReader(Stream stream, Encoding encoding) {
            this._stream = stream;
            this._encoding = encoding;
        }

        // Read at most size characters and return the result as a string.
        public abstract String Read(int size);

        // Read until the end of the stream and return the result as a single string.
        public abstract String ReadToEnd();

        // Read characters up to and including the mode defined newline (or until EOF, in which case the
        // string will not be newline terminated).
        public abstract String ReadLine();

        // Read characters up to and including the mode defined newline (or until EOF or the given size, in
        // which case the string will not be newline terminated).
        public abstract String ReadLine(int size);

        // Discard any data we may have buffered based on the current stream position. Called after seeking in
        // the stream.
        public abstract void DiscardBufferedData();

        public abstract long Position {
            get;
            internal set; // update position bookkeeping
        }
    }

    // Read data as binary. We encode binary data in the low order byte of each character of the strings
    // returned so there will be a X2 expansion in space required (but normal string indexing can be used to
    // inspect the data).
    internal class PythonBinaryReader : PythonStreamReader {

        // Buffer size (in bytes) used when reading until the end of the stream.
        private const int BufferSize = 4096;
        private byte[] buffer;

        public PythonBinaryReader(Stream stream)
            : base(stream, null) {
        }

        // Read at most size characters (bytes in this case) and return the result as a string.
        public override String Read(int size) {
            byte[] data;
            if (size <= BufferSize) {
                if (buffer == null)
                    buffer = new byte[BufferSize];
                data = buffer;
            } else
                data = new byte[size];
            int leftCount = size;
            int offset = 0;
            while (true) {
                int count = _stream.Read(data, offset, leftCount);
                if (count <= 0) break;
                leftCount -= count;
                if (leftCount <= 0) break;
                offset += count;
            }

            System.Diagnostics.Debug.Assert(leftCount >= 0);

            return PackDataIntoString(data, size - leftCount);
        }

        // Read until the end of the stream and return the result as a single string.
        public override String ReadToEnd() {
            StringBuilder sb = new StringBuilder();
            int totalcount = 0;
            if (buffer == null)
                buffer = new byte[BufferSize];
            while (true) {
                int count = _stream.Read(buffer, 0, BufferSize);
                if (count == 0)
                    break;
                sb.Append(PackDataIntoString(buffer, count));
                totalcount += count;
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Read characters up to and including a '\n' (or until EOF, in which case the string will not be
        // newline terminated).
        public override String ReadLine() {
            StringBuilder sb = new StringBuilder(80);
            while (true) {
                int b = _stream.ReadByte();
                if (b == -1)
                    break;
                sb.Append((char)b);
                if (b == '\n')
                    break;
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Read characters up to and including a '\n' (or until EOF or the given size, in which case the
        // string will not be newline terminated).
        public override String ReadLine(int size) {
            StringBuilder sb = new StringBuilder(80);
            while (size-- > 0) {
                int b = _stream.ReadByte();
                if (b == -1)
                    break;
                sb.Append((char)b);
                if (b == '\n')
                    break;
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Discard any data we may have buffered based on the current stream position. Called after seeking in
        // the stream.
        public override void DiscardBufferedData() {
            // No buffering is performed.
        }

        public override long Position {
            get {
                return _stream.Position;
            }
            internal set {
            }
        }

        // Convert a byte array into a string by casting each byte into a character.
        internal static String PackDataIntoString(byte[] data, int count) {
            StringBuilder sb = new StringBuilder(count);
            for (int i = 0; i < count; i++)
                sb.Append((char)data[i]);
            return sb.ToString();
        }

    }

    // Read data as text with lines terminated with '\r\n' (the Windows convention). Such terminators will be
    // translated to '\n' in the strings returned.
    internal class PythonTextCRLFReader : PythonStreamReader {

        // We read the stream through a StreamReader to take advantage of stream buffering and encoding to
        // translate incoming bytes into characters.  This requires us to keep track of our own position.
        private StreamReader _reader;
        private long _position;
        private char[] _buffer = new char[20];
        private int _bufPos, _bufLen;

        public PythonTextCRLFReader(Stream stream, Encoding encoding)
            : base(stream, encoding) {
            if (stream.CanSeek) _position = stream.Position;
            _reader = new StreamReader(stream, encoding);            
        }

        private int Read() {
            if (_bufPos >= _bufLen && ReadBuffer() == 0) {
                return -1;
            }

            _position++;
            return _buffer[_bufPos++];
        }

        private int Peek() {
            if (_bufPos >= _bufLen && ReadBuffer() == 0) {
                return -1;
            }

            return _buffer[_bufPos];
        }

        private int ReadBuffer() {
            _bufLen = _reader.ReadBlock(_buffer, 0, _buffer.Length);
            _bufPos = 0;
            return _bufLen;
        }        

        // Read at most size characters and return the result as a string.
        public override String Read(int size) {
            StringBuilder sb = new StringBuilder(size);
            while (size-- > 0) {
                int c = Read();
                if (c == -1)
                    break;
                if (c == '\r' && Peek() == '\n') {
                    c = Read();
                }
                sb.Append((char)c);
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Read until the end of the stream and return the result as a single string.
        public override String ReadToEnd() {
            StringBuilder sb = new StringBuilder();
            while (true) {
                int c = Read();
                if (c == -1)
                    break;
                if (c == '\r' && Peek() == '\n') {
                    c = Read();
                }
                sb.Append((char)c);
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Read characters up to and including a '\r\n', converted to '\n' (or until EOF, in which case the
        // string will not be newline terminated).
        public override String ReadLine() {
            return ReadLine(Int32.MaxValue);
        }

        // Read characters up to and including a '\r\n', converted to '\n' (or until EOF or the given size, in
        // which case the string will not be newline terminated).
        public override String ReadLine(int size) {
            StringBuilder sb = null;
            // start off w/ some text
            if (_bufPos >= _bufLen) ReadBuffer();
            if (_bufLen == 0) return String.Empty;

            int curIndex = _bufPos;
            int bytesWritten = 0;
            int lenAdj = 0;
            while (true) {
                if (curIndex >= _bufLen) {
                    // need more text...
                    if (sb == null) {
                        sb = new StringBuilder((curIndex - _bufPos) * 2);
                    }
                    sb.Append(_buffer, _bufPos, curIndex - _bufPos);
                    if (ReadBuffer() == 0) {
                        return sb.ToString();
                    }
                    curIndex = 0;
                }

                char c = _buffer[curIndex++];
                if (c == '\r') {
                    if (curIndex < _bufLen) {
                        if (_buffer[curIndex] == '\n') {
                            _position++;
                            c = _buffer[curIndex++];
                            lenAdj = 2;
                        }
                    } else if (_reader.Peek() == '\n') {
                        c = (char)_reader.Read();
                        lenAdj = 1;
                    }
                }
                _position++;
                if (c == '\n') {
                    break;
                }
                if (++bytesWritten >= size) break;
            }

            return FinishString(sb, curIndex, lenAdj);
        }

        private string FinishString(StringBuilder sb, int curIndex, int lenAdj) {
            int len = curIndex - _bufPos;
            int pos = _bufPos;
            _bufPos = curIndex;
            if (sb != null) {
                if (lenAdj != 0) {
                    sb.Append(_buffer, pos, len - lenAdj);
                    sb.Append('\n');
                } else {
                    sb.Append(_buffer, pos, len);
                }

                return sb.ToString();
            } else if (lenAdj != 0) {
                return new String(_buffer, pos, len - lenAdj) + "\n";
            } else {
                return new String(_buffer, pos, len);
            }
        }

        public override long Position {
            get {
                return _position;
            }
            internal set {
                _position = value;
            }
        }

        // Discard any data we may have buffered based on the current stream position. Called after seeking in
        // the stream.
        public override void DiscardBufferedData() {
            _bufPos = _bufLen = 0;
            _reader.DiscardBufferedData();
        }
    }

    // Read data as text with lines terminated with '\r' (the Macintosh convention). Such terminators will be
    // translated to '\n' in the strings returned.
    internal class PythonTextCRReader : PythonStreamReader {

        // We read the stream through a StreamReader to take advantage of stream buffering and encoding to
        // translate incoming bytes into characters.  This requires us to keep control of our own position.
        private StreamReader _reader;
        private long _position;

        public PythonTextCRReader(Stream stream, Encoding encoding)
            : base(stream, encoding) {
            if (stream.CanSeek) _position = stream.Position;
            _reader = new StreamReader(stream, encoding);
        }

        // Read at most size characters and return the result as a string.
        public override String Read(int size) {
            StringBuilder sb = new StringBuilder(size);
            while (size-- > 0) {
                int c = _reader.Read();
                if (c == -1)
                    break;
                _position++;
                if (c == '\r')
                    c = '\n';
                sb.Append((char)c);
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Read until the end of the stream and return the result as a single string.
        public override String ReadToEnd() {
            StringBuilder sb = new StringBuilder();
            while (true) {
                int c = _reader.Read();
                if (c == -1)
                    break;
                _position++;
                if (c == '\r')
                    c = '\n';
                sb.Append((char)c);
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Read characters up to and including a '\r', converted to '\n' (or until EOF, in which case the
        // string will not be newline terminated).
        public override String ReadLine() {
            StringBuilder sb = new StringBuilder(80);
            while (true) {
                int c = _reader.Read();
                if (c == -1)
                    break;
                _position++;
                if (c == '\r')
                    c = '\n';
                sb.Append((char)c);
                if (c == '\n')
                    break;
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Read characters up to and including a '\r', converted to '\n' (or until EOF or the given size, in
        // which case the string will not be newline terminated).
        public override String ReadLine(int size) {
            StringBuilder sb = new StringBuilder(80);
            while (size-- > 0) {
                int c = _reader.Read();
                if (c == -1)
                    break;
                _position++;
                if (c == '\r')
                    c = '\n';
                sb.Append((char)c);
                if (c == '\n')
                    break;
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        public override long Position {
            get {
                return _position;
            }
            internal set {
                _position = value;
            }
        }

        // Discard any data we may have buffered based on the current stream position. Called after seeking in
        // the stream.
        public override void DiscardBufferedData() {
            _reader.DiscardBufferedData();
        }
    }

    // Read data as text with lines terminated with '\n' (the Unix convention).
    internal class PythonTextLFReader : PythonStreamReader {

        // We read the stream through a StreamReader to take advantage of stream buffering and encoding to
        // translate incoming bytes into characters.  This requires us to keep track of our own position.
        private StreamReader _reader;
        private long _position;

        public PythonTextLFReader(Stream stream, Encoding encoding)
            : base(stream, encoding) {
            if (stream.CanSeek) _position = stream.Position;
            _reader = new StreamReader(stream, encoding);
        }

        // Read at most size characters and return the result as a string.
        public override String Read(int size) {
            StringBuilder sb = new StringBuilder(size);
            while (size-- > 0) {
                int c = _reader.Read();
                if (c == -1)
                    break;
                _position++;
                sb.Append((char)c);
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Read until the end of the stream and return the result as a single string.
        public override String ReadToEnd() {
            return _reader.ReadToEnd();
        }

        // Read characters up to and including a '\n' (or until EOF, in which case the string will not be
        // newline terminated).
        public override String ReadLine() {
            StringBuilder sb = new StringBuilder(80);
            while (true) {
                int c = _reader.Read();
                if (c == -1)
                    break;
                _position++;
                sb.Append((char)c);
                if (c == '\n')
                    break;
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Read characters up to and including a '\n' (or until EOF or the given size, in which case the
        // string will not be newline terminated).
        public override String ReadLine(int size) {
            StringBuilder sb = new StringBuilder(80);
            while (size-- > 0) {
                int c = _reader.Read();
                if (c == -1)
                    break;
                _position++;
                sb.Append((char)c);
                if (c == '\n')
                    break;
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        public override long Position {
            get {
                return _position;
            }
            internal set {
                _position = value;
            }
        }

        // Discard any data we may have buffered based on the current stream position. Called after seeking in
        // the stream.
        public override void DiscardBufferedData() {
            _reader.DiscardBufferedData();
        }
    }

    // Read data as text with lines terminated with any of '\n', '\r' or '\r\n'. Such terminators will be
    // translated to '\n' in the strings returned. This class also records whcih of these have been seen so
    // far in the stream to support python semantics (see the Terminators property).
    internal class PythonUniversalReader : PythonStreamReader {

        // Symbols for the different styles of newline terminator we might have seen in this stream so far.
        public enum TerminatorStyles {
            None = 0x0,
            CrLf = 0x1,  // '\r\n'
            Cr = 0x2,  // '\r'
            Lf = 0x4   // '\n'
        }

        // We read the stream through a StreamReader to take advantage of stream buffering and encoding to
        // translate incoming bytes into characters.  This requires that we keep track of our own position.
        private StreamReader _reader;
        private TerminatorStyles _terminators;
        private long _position;

        public PythonUniversalReader(Stream stream, Encoding encoding)
            : base(stream, encoding) {
            if (stream.CanSeek) _position = stream.Position;
            _reader = new StreamReader(stream, encoding);
            _terminators = TerminatorStyles.None;
        }

        // Private helper used to check for newlines and transform and record as necessary. Returns the
        // possibly translated character read.
        private int ReadChar() {
            int c = _reader.Read();
            if (c != -1) _position++;
            if (c == '\r' && _reader.Peek() == '\n') {
                c = _reader.Read();
                _position++;
                _terminators |= TerminatorStyles.CrLf;
            } else if (c == '\r') {
                c = '\n';
                _terminators |= TerminatorStyles.Cr;
            } else if (c == '\n') {
                _terminators |= TerminatorStyles.Lf;
            }
            return c;
        }

        // Read at most size characters and return the result as a string.
        public override String Read(int size) {
            StringBuilder sb = new StringBuilder(size);
            while (size-- > 0) {
                int c = ReadChar();
                if (c == -1)
                    break;
                sb.Append((char)c);
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Read until the end of the stream and return the result as a single string.
        public override String ReadToEnd() {
            StringBuilder sb = new StringBuilder();
            while (true) {
                int c = ReadChar();
                if (c == -1)
                    break;
                sb.Append((char)c);
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Read characters up to and including a '\r\n', '\r' or '\n' converted to '\n' (or until EOF, in
        // which case the string will not be newline terminated).
        public override String ReadLine() {
            StringBuilder sb = new StringBuilder(80);
            while (true) {
                int c = ReadChar();
                if (c == -1)
                    break;
                sb.Append((char)c);
                if (c == '\n')
                    break;
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Read characters up to and including a '\r\n', '\r' or '\n' converted to '\n' (or until EOF or the
        // given size, in which case the string will not be newline terminated).
        public override String ReadLine(int size) {
            StringBuilder sb = new StringBuilder(80);
            while (size-- > 0) {
                int c = ReadChar();
                if (c == -1)
                    break;
                sb.Append((char)c);
                if (c == '\n')
                    break;
            }
            if (sb.Length == 0)
                return String.Empty;
            return sb.ToString();
        }

        // Discard any data we may have buffered based on the current stream position. Called after seeking in
        // the stream.
        public override void DiscardBufferedData() {
            _reader.DiscardBufferedData();
        }

        public override long Position {
            get {
                return _position;
            }
            internal set {
                _position = value;
            }
        }

        // PythonUniversalReader specific property that returns a bitmask of all the newline termination
        // styles seen in the stream so far.
        public TerminatorStyles Terminators { get { return _terminators; } }
    }

    // The abstract writer API.
    internal abstract class PythonStreamWriter {

        // All the writers use a stream as output and most (except the binary writer) use an encoding to
        // define the mapping from characters into bytes. So we factor that out in the base class.
        protected Stream _stream;
        protected Encoding _encoding;

        public Encoding Encoding { get { return _encoding; } }
        
        public PythonStreamWriter(Stream stream, Encoding encoding) {
            this._stream = stream;
            this._encoding = encoding;
        }

        // Write the data in the input string to the output stream, converting line terminators ('\n') into
        // the output format as necessary.  Returns the number of bytes written
        public abstract int Write(String data);

        // Flush any buffered data to the file.
        public abstract void Flush();

        public void Seek(long index) {
            _stream.Seek(index, SeekOrigin.Begin);
        }
    }

    // Write binary data embedded in the low-order byte of each string character to the output stream with no
    // other translation.
    internal class PythonBinaryWriter : PythonStreamWriter {
        public PythonBinaryWriter(Stream stream)
            : base(stream, null) {
        }

        // Write the data in the input string to the output stream. No newline conversion is performed.
        public override int Write(String data) {
            int count = data.Length;
            for (int i = 0; i < count; i++)
                _stream.WriteByte((byte)data[i]);
            return count;
        }

        // Flush any buffered data to the file.
        public override void Flush() {
            _stream.Flush();
        }
    }

    // Write data with '\r\n' line termination.
    internal class PythonTextCRLFWriter : PythonStreamWriter {

        // We write the stream through a StreamWriter to take advantage of stream buffering and encoding to
        // translate outgoing characters into bytes.
        protected StreamWriter _writer;

        public PythonTextCRLFWriter(Stream stream, Encoding encoding)
            : base(stream, encoding) {
            _writer = new StreamWriter(stream, encoding);
        }

        // Write the data in the input string to the output stream, converting line terminators ('\n') into
        // '\r\n' as necessary.
        public override int Write(String data) {
            string writeData = data.Replace("\n", "\r\n");
            _writer.Write(writeData);
            return writeData.Length;
        }

        // Flush any buffered data to the file.
        public override void Flush() {
            _writer.Flush();
        }
    }

    // Write data with '\r' line termination.
    internal class PythonTextCRWriter : PythonStreamWriter {

        // We write the stream through a StreamWriter to take advantage of stream buffering and encoding to
        // translate outgoing characters into bytes.
        protected StreamWriter _writer;

        public PythonTextCRWriter(Stream stream, Encoding encoding)
            : base(stream, encoding) {
            _writer = new StreamWriter(stream, encoding);
        }

        // Write the data in the input string to the output stream, converting line terminators ('\n') into
        // '\r' as necessary.
        public override int Write(String data) {
            string writeData = data.Replace("\n", "\r");
            _writer.Write(writeData);
            return writeData.Length;
        }

        // Flush any buffered data to the file.
        public override void Flush() {
            _writer.Flush();
        }
    }

    // Write data with '\n' line termination.
    internal class PythonTextLFWriter : PythonStreamWriter {

        // We write the stream through a StreamWriter to take advantage of stream buffering and encoding to
        // translate outgoing characters into bytes.
        protected StreamWriter _writer;

        public PythonTextLFWriter(Stream stream, Encoding encoding)
            : base(stream, encoding) {
            _writer = new StreamWriter(stream, encoding);
        }

        // Write the data in the input string to the output stream. No conversion of newline terminators is
        // necessary.
        public override int Write(String data) {
            _writer.Write(data);
            return data.Length;
        }

        // Flush any buffered data to the file.
        public override void Flush() {
            _writer.Flush();
        }
    }

    internal static class PythonFileManager {
        static HybridMapping<PythonFile> mapping = new HybridMapping<PythonFile>();

        public static int AddToWeakMapping(PythonFile pf) {
            return mapping.WeakAdd(pf);
        }

        public static int AddToStrongMapping(PythonFile pf) {
            return mapping.StrongAdd(pf);
        }

        public static void Remove(PythonFile pf) {
            mapping.RemoveOnObject(pf);
        }

        public static PythonFile GetFileFromId(int id) {
            PythonFile pf = mapping.GetObjectFromId(id);

            if (pf != null) return pf;
            throw PythonOps.OSError("Bad file descriptor");
        }

        public static int GetIdFromFile(PythonFile pf) {
            return mapping.GetIdFromObject(pf);
        }
    }

    [PythonType("file")]
    public class PythonFile : IDisposable, ICodeFormattable {
        private bool _isConsole;

        // Enumeration of each stream mode.
        private enum PythonFileMode {
            Binary,
            TextCrLf,
            TextCr,
            TextLf,
            UniversalNewline
        }

        // Map a python mode string into a PythonFileMode.
        private static PythonFileMode MapFileMode(String mode) {
            // Assume "mode" is in reasonable good shape, since we checked it in "Make"
            if (mode.Contains("b"))
                return PythonFileMode.Binary;

            if (mode.Contains("U"))
                return PythonFileMode.UniversalNewline;

            // Must be platform specific text mode. Work out which line termination the platform
            // supports based on the value of Environment.NewLine.
            switch (Environment.NewLine) {
                case "\r\n":
                    return PythonFileMode.TextCrLf;
                case "\r":
                    return PythonFileMode.TextCr;
                case "\n":
                    return PythonFileMode.TextLf;
                default:
                    throw new NotImplementedException("Unsupported Environment.NewLine value");
            }
        }

        [StaticExtensionMethod("__new__")]
        public static PythonFile Make(CodeContext context, PythonType cls, string name) {
            return Make(context, cls, name, "r", -1);
        }

        [StaticExtensionMethod("__new__")]
        public static PythonFile Make(CodeContext context, PythonType cls, string name, string mode) {
            return Make(context, cls, name, mode, -1);
        }

        //
        // Here are the mode rules for IronPython "file":
        //          (r|a|w|rU|U|Ur) [ [+][b|t] | [b|t][+] ]
        // 
        // Seems C-Python allows "b|t" at the beginning too.
        // 
        [StaticExtensionMethod("__new__")]
        public static PythonFile Make(CodeContext context, PythonType cls, string name, string mode, int bufsize) {
            FileShare fshare = FileShare.ReadWrite;
            FileMode fmode;
            FileAccess faccess;
            string inMode = mode;

            if (name == null) {
                throw PythonOps.TypeError("file name must be string, found NoneType");
            }

            if (mode == null) {
                throw PythonOps.TypeError("mode must be string, not None");
            }

            if (mode == string.Empty) {
                throw PythonOps.ValueError("empty mode string");
            }


            bool seekEnd = false;
     
            int modeLen = mode.Length;
            char lastChar = mode[modeLen - 1];
            if (lastChar == 't' || lastChar == 'b')
                mode = mode.Substring(0, modeLen - 1);
            else if (lastChar == '+' && modeLen >= 2) {
                char secondToLastChar = mode[modeLen - 2];
                if (secondToLastChar == 't' || secondToLastChar == 'b') {
                    mode = mode.Substring(0, modeLen - 2) + "+";
                }
            }

            if (mode == "r" || mode == "rU" || mode == "U" || mode == "Ur") {
                fmode = FileMode.Open; faccess = FileAccess.Read;
            } else if (mode == "r+" || mode == "rU+" || mode == "U+" || mode == "Ur+") {
                fmode = FileMode.Open; faccess = FileAccess.ReadWrite; 
            } else if (mode == "w") {
                fmode = FileMode.Create; faccess = FileAccess.Write;
            } else if (mode == "w+") {
                fmode = FileMode.Create; faccess = FileAccess.ReadWrite;
            } else if (mode == "a") {
                fmode = FileMode.Append; faccess = FileAccess.Write;
            } else if (mode == "a+") {
                fmode = FileMode.OpenOrCreate; faccess = FileAccess.ReadWrite;
                seekEnd = true;
            } else {
                throw new NotImplementedException("bad mode: " + inMode);
            }

            try {
                Stream stream;
                if (Environment.OSVersion.Platform == PlatformID.Win32NT && name == "nul") {
                    stream = Stream.Null;
                } else if (bufsize <= 0) {
                    stream = ScriptDomainManager.CurrentManager.PAL.OpenInputFileStream(name, fmode, faccess, fshare);
                } else {
                    stream = ScriptDomainManager.CurrentManager.PAL.OpenInputFileStream(name, fmode, faccess, fshare, bufsize);
                }

                if (seekEnd) stream.Seek(0, SeekOrigin.End);

                if (cls == TypeCache.PythonFile) return new PythonFile(stream, SystemState.Instance.DefaultEncoding, name, inMode);

                return cls.CreateInstance(context, stream, SystemState.Instance.DefaultEncoding, name, inMode) as PythonFile;
            } catch (UnauthorizedAccessException e) {
                throw new IOException(e.Message, e);
            }
        }

        [StaticExtensionMethod("__new__")]
        public static PythonFile Make(CodeContext context, PythonType cls, Stream stream) {
            string mode;
            if (stream.CanRead && stream.CanWrite) mode = "w+";
            else if (stream.CanWrite) mode = "w";
            else mode = "r";

            if (cls == TypeCache.PythonFile) return new PythonFile(stream, SystemState.Instance.DefaultEncoding, mode);

            return cls.CreateInstance(context, stream, SystemState.Instance.DefaultEncoding, mode) as PythonFile;
        }

        [StaticExtensionMethod("__new__")]
        public static PythonFile Make(CodeContext context, PythonType cls, Stream stream, string mode) {
            if (cls == TypeCache.PythonFile) return new PythonFile(stream, SystemState.Instance.DefaultEncoding, mode);

            return cls.CreateInstance(context, stream, SystemState.Instance.DefaultEncoding, mode) as PythonFile;
        }

        private readonly Stream stream;
        private readonly string mode;
        private readonly string name;

        private readonly PythonStreamReader reader;
        private readonly PythonStreamWriter writer;
        private bool isclosed;
        private Nullable<long> reseekPosition;

        public bool softspace;

        public Encoding Encoding {
            get {
                return (reader != null) ? reader.Encoding : (writer != null) ? writer.Encoding : null;
            }
        }
        
        public PythonFile(Stream stream, Encoding encoding, string mode)
            : this(stream, encoding, mode, true) {
        }

        internal PythonFile(Stream stream, Encoding encoding, string mode, bool weakMapping) {
            this.stream = stream;
            this.mode = mode;

            PythonFileMode filemode = MapFileMode(mode);

            if (stream.CanRead) {
                switch (filemode) {
                    case PythonFileMode.Binary:
                        reader = new PythonBinaryReader(stream);
                        break;
                    case PythonFileMode.TextCrLf:
                        reader = new PythonTextCRLFReader(stream, encoding);
                        break;
                    case PythonFileMode.TextCr:
                        reader = new PythonTextCRReader(stream, encoding);
                        break;
                    case PythonFileMode.TextLf:
                        reader = new PythonTextLFReader(stream, encoding);
                        break;
                    case PythonFileMode.UniversalNewline:
                        reader = new PythonUniversalReader(stream, encoding);
                        break;
                }
            }
            if (stream.CanWrite) {
                switch (filemode) {
                    case PythonFileMode.Binary:
                        writer = new PythonBinaryWriter(stream);
                        break;
                    case PythonFileMode.TextCrLf:
                        writer = new PythonTextCRLFWriter(stream, encoding);
                        break;
                    case PythonFileMode.TextCr:
                        writer = new PythonTextCRWriter(stream, encoding);
                        break;
                    case PythonFileMode.TextLf:
                        writer = new PythonTextLFWriter(stream, encoding);
                        break;
                }
            }
#if !SILVERLIGHT
            // only possible if the user provides us w/ the stream directly
            FileStream fs = stream as FileStream;
            if (fs != null) {
                this.name = fs.Name;
            } else {
                this.name = "nul";
            }
#else
            this.name = "stream";
#endif

            if (weakMapping)
                PythonFileManager.AddToWeakMapping(this);
        }

        public PythonFile(Stream stream, Encoding encoding, string name, string mode)
            : this(stream, encoding, name, mode, true) {
        }

        internal PythonFile(Stream stream, Encoding encoding, string name, string mode, bool weakMapping)
            : this(stream, encoding, mode, weakMapping) {
            this.name = name;
        }

        ~PythonFile() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (isclosed) return;

            if (disposing) {
                Flush();
                stream.Close();
            }

            isclosed = true;
            PythonFileManager.Remove(this);
        }

        [PythonName("close")]
        public virtual object Close() {
            Dispose(true);
            GC.SuppressFinalize(this);
            return null;
        }

        [Documentation("True if the file is closed, False if the file is still open")]
        public bool IsClosed {
            [PythonName("closed")]
            get {
                return isclosed;
            }
        }

        void ThrowIfClosed() {
            if (isclosed)
                throw PythonOps.ValueError("I/O operation on closed file");
        }

        [PythonName("flush")]
        public void Flush() {
            ThrowIfClosed();
            if (writer != null) {
                writer.Flush();
                stream.Flush();
            }
        }

        [PythonName("fileno")]
        public int GetFileNumber() {
            ThrowIfClosed();
            return PythonFileManager.GetIdFromFile(this);
        }

        [Documentation("gets the name of the file")]
        public string FileName {
            [PythonName("name")]
            get {
                return name;
            }
        }

        [PythonName("read")]
        public string Read() {
            return Read(-1);
        }

        [PythonName("read")]
        public string Read(int size) {
            ThrowIfClosed();

            if (reader == null) {
                throw PythonOps.IOError("Can not read from " + this.name);
            }
            if (size < 0)
                return reader.ReadToEnd();
            else
                return reader.Read(size);
        }

        [PythonName("readline")]
        public string ReadLine() {
            ThrowIfClosed();

            if (reader == null) {
                throw PythonOps.IOError("Can not read from " + this.name);
            }
            return reader.ReadLine();
        }

        [PythonName("readline")]
        public string ReadLine(int size) {
            ThrowIfClosed();

            if (reader == null) {
                throw PythonOps.IOError("Can not read from " + this.name);
            }
            return reader.ReadLine(size);
        }

        [PythonName("readlines")]
        public List ReadLines() {
            List ret = new List();
            string line;
            for (; ; ) {
                line = ReadLine();
                if (line == "") break;
                ret.AddNoLock(line);
            }
            return ret;
        }

        [PythonName("readlines")]
        public List ReadLines(int sizehint) {
            List ret = new List();
            for (; ; ) {
                string line = ReadLine();
                if (line == "") break;
                ret.AddNoLock(line);
                if (line.Length >= sizehint) break;
                sizehint -= line.Length;
            }
            return ret;
        }

        [PythonName("seek")]
        public void Seek(long offset) {
            Seek(offset, 0);
        }

        [PythonName("seek")]
        public void Seek(long offset, int whence) {
            if (mode == "a") return;    // nop when seeking on stream's opened for append.

            ThrowIfClosed();

            if (!stream.CanSeek) {
                throw PythonOps.IOError("Can not seek on file " + name);
            }

            // flush before saving our position to ensure it's accurate.
            Flush();
            SavePositionPreSeek();

            SeekOrigin origin;
            switch (whence) {
                default:
                case 0:
                    origin = SeekOrigin.Begin;
                    break;
                case 1:
                    origin = SeekOrigin.Current;
                    break;
                case 2:
                    origin = SeekOrigin.End;
                    break;
            }
            long newPos = stream.Seek(offset, origin);
            if (reader != null) {
                reader.DiscardBufferedData();
                reader.Position = newPos;
            }
        }

        [PythonName("tell")]
        public object Tell() {
            long l = (reader != null) ? reader.Position : stream.Position;
            if (l <= Int32.MaxValue) return l;
            return Microsoft.Scripting.Math.BigInteger.Create(l);
        }

        [PythonName("write")]
        public void Write(string s) {
            ThrowIfClosed();

            if (writer != null) {
                ResetForWrite();

                int bytesWritten = writer.Write(s);
                if (reader != null && stream.CanSeek)
                    reader.Position += bytesWritten;
                if (IsConsole) {
                    Flush();
                } 
            } else {
                throw PythonOps.IOError("Can not write to " + this.name);
            }
        }

        [PythonName("writelines")]
        public void WriteLines(object o) {
            System.Collections.IEnumerator e = PythonOps.GetEnumerator(o);
            while (e.MoveNext()) {
                string line = e.Current as string;
                if (line == null) {
                    throw PythonOps.TypeError("writelines() argument must be a sequence of strings");
                }
                Write(line);
            }
        }

        [PythonName("xreadlines")]
        public PythonFile ReturnSelf() {
            return this;
        }

        public Object Newlines {
            [PythonName("newlines")]
            get {
                if (reader == null || !(reader is PythonUniversalReader))
                    return null;

                PythonUniversalReader.TerminatorStyles styles = ((PythonUniversalReader)reader).Terminators;
                switch (styles) {
                    case PythonUniversalReader.TerminatorStyles.None:
                        return null;
                    case PythonUniversalReader.TerminatorStyles.CrLf:
                        return "\r\n";
                    case PythonUniversalReader.TerminatorStyles.Cr:
                        return "\r";
                    case PythonUniversalReader.TerminatorStyles.Lf:
                        return "\n";
                    default:
                        System.Collections.Generic.List<String> styleStrings = new System.Collections.Generic.List<String>();
                        if ((styles & PythonUniversalReader.TerminatorStyles.CrLf) != 0)
                            styleStrings.Add("\r\n");
                        if ((styles & PythonUniversalReader.TerminatorStyles.Cr) != 0)
                            styleStrings.Add("\r");
                        if ((styles & PythonUniversalReader.TerminatorStyles.Lf) != 0)
                            styleStrings.Add("\n");
                        return new PythonTuple(styleStrings.ToArray());
                }
            }
        }

        private void SavePositionPreSeek() {
            if (mode == "a+") {
                reseekPosition = stream.Position;
            }
        }

        private void ResetForWrite() {
            if (reseekPosition != null) {
                writer.Seek(reseekPosition.Value);
                reader.Position = reseekPosition.Value;
                reseekPosition = null;
            }
        }

        [PythonName("next")]
        public object Next() {
            string line = ReadLine();
            if (line == "") {
                throw PythonOps.StopIteration();
            }
            return line;
        }

        [PythonName("__iter__")]
        public object GetIterator() {
            ThrowIfClosed();
            return this;
        }

        [PythonName("isatty")]
        public bool IsTty() {
            return _isConsole;
        }

        public bool IsConsole {
            get {
                return _isConsole;
            }
            set {
                _isConsole = value;
            }
        }

        public override string ToString() {
            return string.Format("<{0} file '{1}', mode '{2}' at 0x{3:X8}>",
                isclosed ? "closed" : "open",
                name,
                mode,
                this.GetHashCode()
                );
        }

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            return ToString();
        }

        #endregion
    }
}
