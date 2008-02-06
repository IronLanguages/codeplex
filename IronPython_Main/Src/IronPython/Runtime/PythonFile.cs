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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using System.Diagnostics;

namespace IronPython.Runtime {

    #region Readers

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

        protected Encoding _encoding;

        public Encoding Encoding { get { return _encoding; } }
        public abstract TextReader TextReader { get; }

        public PythonStreamReader(Encoding encoding) {
            _encoding = encoding;
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

        private readonly Stream/*!*/ _stream;
        public override TextReader TextReader { get { return null; } }

        // Buffer size (in bytes) used when reading until the end of the stream.
        private const int BufferSize = 4096;
        private byte[] _buffer;

        public PythonBinaryReader(Stream/*!*/ stream)
            : base(null) {
            Assert.NotNull(stream);
            _stream = stream;
        }

        // Read at most size characters (bytes in this case) and return the result as a string.
        public override String Read(int size) {
            byte[] data;
            if (size <= BufferSize) {
                if (_buffer == null)
                    _buffer = new byte[BufferSize];
                data = _buffer;
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
            if (_buffer == null)
                _buffer = new byte[BufferSize];
            while (true) {
                int count = _stream.Read(_buffer, 0, BufferSize);
                if (count == 0)
                    break;
                sb.Append(PackDataIntoString(_buffer, count));
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

    internal abstract class PythonTextReader : PythonStreamReader {
        
        // We read the stream through a StreamReader to take advantage of stream buffering and encoding to
        // translate incoming bytes into characters.  This requires us to keep control of our own position.
        protected readonly TextReader/*!*/ _reader;
        protected long _position;

        public override TextReader TextReader { get { return _reader; } }

        public override long Position {
            get {
                return _position;
            }
            internal set {
                _position = value;
            }
        }

        public PythonTextReader(TextReader/*!*/ reader, Encoding/*!*/ encoding, long position)
            : base(encoding) {
            _reader = reader;
            _position = position;
        }

        // Discard any data we may have buffered based on the current stream position. Called after seeking in
        // the stream.
        public override void DiscardBufferedData() {
            StreamReader streamReader = _reader as StreamReader;
            if (streamReader != null) {
                streamReader.DiscardBufferedData();
            }
        }
    }
    
    // Read data as text with lines terminated with '\r\n' (the Windows convention). Such terminators will be
    // translated to '\n' in the strings returned.
    internal class PythonTextCRLFReader : PythonTextReader {

        // We read the stream through a StreamReader to take advantage of stream buffering and encoding to
        // translate incoming bytes into characters.  This requires us to keep track of our own position.
        private char[] _buffer = new char[20];
        private int _bufPos, _bufLen;

        public PythonTextCRLFReader(TextReader/*!*/ reader, Encoding/*!*/ encoding, long position)
            : base(reader, encoding, position) {
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
            _bufLen = _reader.Read(_buffer, 0, _buffer.Length);
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
            if (sb.Length == 0) {
                return String.Empty;
            }
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

        // Discard any data we may have buffered based on the current stream position. Called after seeking in
        // the stream.
        public override void DiscardBufferedData() {
            _bufPos = _bufLen = 0;
            base.DiscardBufferedData();
        }
    }

    // Read data as text with lines terminated with '\r' (the Macintosh convention). Such terminators will be
    // translated to '\n' in the strings returned.
    internal class PythonTextCRReader : PythonTextReader {

        public PythonTextCRReader(TextReader/*!*/ reader, Encoding/*!*/ encoding, long position)
            : base(reader, encoding, position) {
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
    }

    // Read data as text with lines terminated with '\n' (the Unix convention).
    internal class PythonTextLFReader : PythonTextReader {

        public PythonTextLFReader(TextReader/*!*/ reader, Encoding/*!*/ encoding, long position)
            : base(reader, encoding, position) {
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
    }

    // Read data as text with lines terminated with any of '\n', '\r' or '\r\n'. Such terminators will be
    // translated to '\n' in the strings returned. This class also records whcih of these have been seen so
    // far in the stream to support python semantics (see the Terminators property).
    internal class PythonUniversalReader : PythonTextReader {

        // Symbols for the different styles of newline terminator we might have seen in this stream so far.
        public enum TerminatorStyles {
            None = 0x0,
            CrLf = 0x1,  // '\r\n'
            Cr = 0x2,  // '\r'
            Lf = 0x4   // '\n'
        }

        // We read the stream through a StreamReader to take advantage of stream buffering and encoding to
        // translate incoming bytes into characters.  This requires that we keep track of our own position.
        private TerminatorStyles _terminators;

        public PythonUniversalReader(TextReader/*!*/ reader, Encoding/*!*/ encoding, long position)
            : base(reader, encoding, position) {
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

        // PythonUniversalReader specific property that returns a bitmask of all the newline termination
        // styles seen in the stream so far.
        public TerminatorStyles Terminators { get { return _terminators; } }
    }

    #endregion

    #region Writers

    // The abstract writer API.
    internal abstract class PythonStreamWriter {

        protected Encoding _encoding;

        public Encoding Encoding { get { return _encoding; } }
        public abstract TextWriter TextWriter { get; } 
        
        public PythonStreamWriter(Encoding encoding) {
            _encoding = encoding;
        }

        // Write the data in the input string to the output stream, converting line terminators ('\n') into
        // the output format as necessary.  Returns the number of bytes written
        public abstract int Write(String/*!*/ data);

        // Flush any buffered data to the file.
        public abstract void Flush();
    }

    // Write binary data embedded in the low-order byte of each string character to the output stream with no
    // other translation.
    internal class PythonBinaryWriter : PythonStreamWriter {
        private Stream/*!*/ _stream;

        public override TextWriter TextWriter { get { return null; } }
        
        public PythonBinaryWriter(Stream/*!*/ stream)
            : base(null) {
            _stream = stream;
        }

        // Write the data in the input string to the output stream. No newline conversion is performed.
        public override int Write(string/*!*/ data) {
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

    // Write data with '\r', '\n' or '\r\n' line termination.
    internal class PythonTextWriter : PythonStreamWriter {

        // We write the stream through a StreamWriter to take advantage of stream buffering and encoding to
        // translate outgoing characters into bytes.
        private TextWriter/*!*/ _writer;
        private readonly string _eoln;

        public override TextWriter TextWriter { get { return _writer; } }
        
        public PythonTextWriter(TextWriter/*!*/ writer, string eoln)
            : base(writer.Encoding) {
            _writer = writer;
            _eoln = eoln;
        }

        // Write the data in the input string to the output stream, converting line terminators ('\n') into
        // _eoln as necessary.
        public override int Write(string/*!*/ data) {
            if (_eoln != null) {
                data = data.Replace("\n", _eoln);
            }
            _writer.Write(data);
            return data.Length;
        }

        // Flush any buffered data to the file.
        public override void Flush() {
            _writer.Flush();
        }
    }

    #endregion

    #region File Manager

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

    #endregion

    [PythonType("file")]
    public class PythonFile : IDisposable, ICodeFormattable, IEnumerable<string>, IEnumerable, IWeakReferenceable {
        private ConsoleStreamType _consoleStreamType;
        private SharedIO _io;   // null for non-console
        private Stream _stream; // null for console
        private string _mode;
        private string _name;
        private PythonFileMode _fileMode;

        private PythonStreamReader _reader;
        private PythonStreamWriter _writer;
        private bool _isOpen;
        private Nullable<long> _reseekPosition; // always null for console
        private WeakRefTracker _weakref;

        public bool softspace;

        public bool IsConsole {
            get {
                return _stream == null;
            }
        }

        public PythonFile() {
        }
        
        internal static PythonFile/*!*/ Create(CodeContext/*!*/ context, Stream/*!*/ stream, string/*!*/ name, string/*!*/ mode) {
            return Create(stream, PythonContext.GetContext(context).DefaultEncoding, name, mode);
        }

        internal static PythonFile/*!*/ Create(Stream/*!*/ stream, Encoding/*!*/ encoding, string/*!*/ name, string/*!*/ mode) {
            PythonFile res = new PythonFile();
            res.Initialize(stream, encoding, name, mode);
            return res;
        }

        internal static PythonFile/*!*/ CreateConsole(SharedIO/*!*/ io, ConsoleStreamType type, string/*!*/ name) {
            PythonFile res = new PythonFile();
            res.InitializeConsole(io, type, name);
            return res;
        }

        internal static PythonFile/*!*/ Create(CodeContext/*!*/ context, Stream/*!*/ stream, string/*!*/ name, string/*!*/ mode, bool weakMapping) {
            PythonFile res = new PythonFile();
            res.InternalInitialize(stream, PythonContext.GetContext(context).DefaultEncoding, name, mode, weakMapping);
            return res;
        }

#if !SILVERLIGHT // finalizers not supported
        ~PythonFile() {
            Dispose(false);
        }
#endif

        #region Python initialization

        //
        // Here are the mode rules for IronPython "file":
        //          (r|a|w|rU|U|Ur) [ [+][b|t] | [b|t][+] ]
        // 
        // Seems C-Python allows "b|t" at the beginning too.
        // 
        [PythonName("__init__")]
        public void Initialize(CodeContext/*!*/ context, string name, [DefaultParameterValue("r")]string mode, [DefaultParameterValue(-1)]int bufsize) {
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

                Initialize(stream, PythonContext.GetContext(context).DefaultEncoding, name, inMode);
                this._isOpen = true;
            } catch (UnauthorizedAccessException e) {
                throw new IOException(e.Message, e);
            }
        }

        [PythonName("__init__")]
        public void Initialize(CodeContext/*!*/ context, [NotNull]Stream/*!*/ stream) {
            Contract.RequiresNotNull(stream, "stream");

            string mode;
            if (stream.CanRead && stream.CanWrite) mode = "w+";
            else if (stream.CanWrite) mode = "w";
            else mode = "r";

            Initialize(stream, PythonContext.GetContext(context).DefaultEncoding, mode);
        }

        [PythonName("__init__")]
        public void Initialize(CodeContext/*!*/ context, [NotNull]Stream/*!*/ stream, string mode) {
            Initialize(stream, PythonContext.GetContext(context).DefaultEncoding, mode);
        }

        [PythonName("__init__")]
        public void Initialize([NotNull]Stream/*!*/ stream, Encoding encoding, string mode) {
            InternalInitialize(stream, encoding, mode, true);
        }

        [PythonName("__init__")]
        public void Initialize([NotNull]Stream/*!*/ stream, [NotNull]Encoding/*!*/ encoding, string name, string mode) {
            Contract.RequiresNotNull(stream, "stream");
            Contract.RequiresNotNull(encoding, "encoding");

            InternalInitialize(stream, encoding, name, mode, true);
        }

        private PythonTextReader/*!*/ CreateTextReader(TextReader/*!*/ reader, Encoding/*!*/ encoding, long initPosition) {
            switch (_fileMode) {
                case PythonFileMode.TextCrLf:
                    return new PythonTextCRLFReader(reader, encoding, initPosition);

                case PythonFileMode.TextCr:
                    return new PythonTextCRReader(reader, encoding, initPosition);

                case PythonFileMode.TextLf:
                    return new PythonTextLFReader(reader, encoding, initPosition);

                case PythonFileMode.UniversalNewline:
                    return new PythonUniversalReader(reader, encoding, initPosition);
            }

            throw Assert.Unreachable;
        }

        private PythonTextReader/*!*/ CreateConsoleReader(SharedIO/*!*/ io) {
            Encoding encoding;
            return CreateTextReader(io.GetReader(out encoding), encoding, 0);
        }

        private PythonTextWriter/*!*/ CreateTextWriter(TextWriter/*!*/ writer) {
            switch (_fileMode) {
                case PythonFileMode.TextCrLf:
                    return new PythonTextWriter(writer, "\r\n");

                case PythonFileMode.TextCr:
                    return new PythonTextWriter(writer, "\r");

                case PythonFileMode.TextLf:
                    return new PythonTextWriter(writer, null);
            }

            throw Assert.Unreachable;
        }

        internal void InternalInitialize(Stream/*!*/ stream, Encoding/*!*/ encoding, string/*!*/ mode, bool weakMapping) {
            Assert.NotNull(stream, encoding, mode);

            _stream = stream;
            _mode = mode;
            _isOpen = true;
            _io = null;
            _fileMode = MapFileMode(mode);

            if (stream.CanRead) {
                if (_fileMode == PythonFileMode.Binary) {
                    _reader = new PythonBinaryReader(stream);
                } else {
                    long initPosition = (stream.CanSeek) ? stream.Position : 0;
                    _reader = CreateTextReader(new StreamReader(stream, encoding), encoding, initPosition);
                }
            }

            if (stream.CanWrite) {
                if (_fileMode == PythonFileMode.Binary) {
                    _writer = new PythonBinaryWriter(stream);
                } else {
                    _writer = CreateTextWriter(new StreamWriter(stream, encoding));
                }
            }

#if !SILVERLIGHT
            // only possible if the user provides us w/ the stream directly
            FileStream fs = stream as FileStream;
            if (fs != null) {
                _name = fs.Name;
            } else {
                _name = "nul";
            }
#else
            _name = "stream";
#endif

            if (weakMapping) {
                PythonFileManager.AddToWeakMapping(this);
            }
        }

        internal void InitializeConsole(SharedIO/*!*/ io, ConsoleStreamType type, string/*!*/ name) {
            _consoleStreamType = type;
            _io = io;
            _stream = null;
            _mode = (type == ConsoleStreamType.Input) ? "r" : "w";
            _isOpen = true;
            _fileMode = MapFileMode(_mode);
            _name = name;

            if (type == ConsoleStreamType.Input) {
                _reader = CreateConsoleReader(io);
            } else {
                _writer = CreateTextWriter(io.GetWriter(type));
            }

            PythonFileManager.AddToWeakMapping(this);
        }

        internal void InternalInitialize(Stream stream, Encoding encoding, string name, string mode, bool weakMapping) {
            InternalInitialize(stream, encoding, mode, weakMapping);
            _name = name;
        }

        #endregion

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

        public Encoding Encoding {
            get {
                return (_reader != null) ? _reader.Encoding : (_writer != null) ? _writer.Encoding : null;
            }
        }            

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_isOpen) return;

            if (disposing) {
                Flush();
                if (!IsConsole) {
                    _stream.Close();
                }
            }

            _isOpen = false;
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
                return !_isOpen;
            }
        }

        void ThrowIfClosed() {
            if (!_isOpen)
                throw PythonOps.ValueError("I/O operation on closed file");
        }

        [PythonName("flush")]
        public void Flush() {
            ThrowIfClosed();
            if (_writer != null) {
                _writer.Flush();
                if (!IsConsole) {
                    _stream.Flush();
                }
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
                return _name;
            }
        }

        [PythonName("read")]
        public string Read() {
            return Read(-1);
        }

        [PythonName("read")]
        public string Read(int size) {
            PythonStreamReader reader = GetReader();
            if (size < 0) {
                return reader.ReadToEnd();
            } else {
                return reader.Read(size);
            }
        }

        [PythonName("readline")]
        public string ReadLine() {
            return GetReader().ReadLine();
        }

        [PythonName("readline")]
        public string ReadLine(int size) {
            return GetReader().ReadLine(size);
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
            if (_mode == "a") return;    // nop when seeking on stream's opened for append.

            ThrowIfClosed();

            if (IsConsole || !_stream.CanSeek) {
                throw PythonOps.IOError("Can not seek on file " + _name);
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
            long newPos = _stream.Seek(offset, origin);
            if (_reader != null) {
                _reader.DiscardBufferedData();
                _reader.Position = newPos;
            }
        }

        [PythonName("tell")]
        public object Tell() {
            if (IsConsole) {
                throw PythonOps.IOError("Bad file descriptor");
            }

            long l = (_reader != null) ? _reader.Position : _stream.Position;
            if (l <= Int32.MaxValue) return l;
            return Microsoft.Scripting.Math.BigInteger.Create(l);
        }

        [PythonName("write")]
        public void Write(string s) {
            PythonStreamWriter writer = GetWriter();
            int bytesWritten = _writer.Write(s);
            if (IsConsole) {
                Flush();
            } else if (_reader != null && _stream.CanSeek) {
                _reader.Position += bytesWritten;
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
                if (_reader == null || !(_reader is PythonUniversalReader))
                    return null;

                PythonUniversalReader.TerminatorStyles styles = ((PythonUniversalReader)_reader).Terminators;
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
            if (_mode == "a+") {
                Debug.Assert(!IsConsole);
                _reseekPosition = _stream.Position;
            }
        }

        // called before each read operation
        private PythonStreamReader/*!*/ GetReader() {
            ThrowIfClosed();
            if (_reader == null) {
                throw PythonOps.IOError("Can not read from " + _name);
            }

            if (IsConsole) {
                // update reader if redirected:
                SharedIO io = ScriptDomainManager.CurrentManager.SharedIO;

                if (!ReferenceEquals(io.InputReader, _reader.TextReader)) {
                    _reader = CreateConsoleReader(io);
                }
            }

            return _reader;
        }

        // called before each write operation
        private PythonStreamWriter/*!*/ GetWriter() {
            ThrowIfClosed();

            if (_writer == null) {
                throw PythonOps.IOError("Can not write to " + _name);
            }

            if (IsConsole) {
                // update writer if redirected:
                TextWriter currentWriter = ScriptDomainManager.CurrentManager.SharedIO.GetWriter(_consoleStreamType);

                if (!ReferenceEquals(currentWriter, _writer.TextWriter)) {
                    _writer.Flush();
                    _writer = CreateTextWriter(currentWriter);
                }

            } else if (_reseekPosition != null) {
                _stream.Seek(_reseekPosition.Value, SeekOrigin.Begin);
                _reader.Position = _reseekPosition.Value;
                _reseekPosition = null;
            }

            return _writer;
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
            return IsConsole;
        }

        [PythonName("__enter__")]
        public object Enter() {
            ThrowIfClosed();
            return this;
        }

        [PythonName("__exit__")]
        public void Exit(params object[] excinfo) {
            Close();
        }

        public override string ToString() {
            return string.Format("<{0} file '{1}', mode '{2}' at 0x{3:X8}>",
                _isOpen ? "open" : "closed",
                _name ?? "<uninitialized file>",
                _mode ?? "<uninitialized file>",
                this.GetHashCode()
                );
        }

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            return ToString();
        }

        #endregion

        #region IEnumerable<string> Members

        IEnumerator<string> IEnumerable<string>.GetEnumerator() {
            for (; ; ) {
                string line = ReadLine();
                if (line == "") yield break;
                yield return line;
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            for (; ; ) {
                string line = ReadLine();
                if (line == "") yield break;
                yield return line;
            }
        }

        #endregion

        #region IWeakReferenceable Members

        public WeakRefTracker GetWeakRef() {
            return _weakref;
        }

        public bool SetWeakRef(WeakRefTracker value) {
            _weakref = value;
            return true;
        }

        public void SetFinalizer(WeakRefTracker value) {
            SetWeakRef(value);
        }

        #endregion
    }
}
