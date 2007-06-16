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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.Serialization;
using System.Reflection.Emit;
using System.Runtime.Remoting;

namespace Microsoft.Scripting {

    public delegate void Function();
    public delegate R Function<R>();
    public delegate R Function<T, R>(T arg);
    public delegate R Function<T1, T2, R>(T1 arg1, T2 arg2);
    
    /// <summary>
    /// Utilities implementing generic functionality. Contains also implementations of features not supported by Silverlight.
    /// </summary>
    public static class Utils {

        #region Debug

        public static class Assert {

            public static Exception Unreachable {
                get {
                    Debug.Assert(false, "Unreachable");
                    return null;
                }
            }

            [Conditional("DEBUG")]
            public static void NotNull(object var, params object[] vars) {
                Debug.Assert(var != null);
                foreach (object v in vars) {
                    Debug.Assert(v != null);
                }
            }

            [Conditional("DEBUG")]
            public static void Serializable(Type type) {
                Debug.Assert(type != null);

#if !SILVERLIGHT
                if (type.IsSerializable) return;
                if (type.IsSubclassOf(typeof(MarshalByRefObject))) return;
                if (typeof(ISerializable).IsAssignableFrom(type)) return;
                Debug.Assert(false, "Type " + type + " is not serializable");
#endif
            }

            [Conditional("DEBUG")]
            public static void NotNullItems<T>(IEnumerable<T> items) where T : class {
                Debug.Assert(items != null);
                foreach (object item in items) {
                    Debug.Assert(item != null);
                }
            }
        }

        #endregion

        #region Reflection

        public static class Reflection {

            // Generic type names have the arity (number of generic type paramters) appended at the end. 
            // For eg. the mangled name of System.List<T> is "List`1". This mangling is done to enable multiple 
            // generic types to exist as long as they have different arities.
            public const char GenericArityDelimiter = '`';

#if SILVERLIGHT
            public static readonly Type[] EmptyTypes = new Type[0];
#else
            public static readonly Type[] EmptyTypes = Type.EmptyTypes;
#endif

#if SILVERLIGHT
            public static bool IsNested(Type t) {
                return t.DeclaringType != null;
            }
#else
            public static bool IsNested(Type t) { return t.IsNested; }
#endif

#if DEBUG
            public static string FormatSignature(MethodBase method) {
                StringBuilder result = new StringBuilder();
                result.Append(method.DeclaringType.FullName);
                result.Append("::");
                result.Append(method.Name);
                result.Append("(");

                ParameterInfo[] ps = method.GetParameters();
                for (int i = 0; i < ps.Length; i++) {
                    if (i > 0) result.Append(", ");
                    result.Append(ps[i].ParameterType.Name);
                    if (!System.String.IsNullOrEmpty(ps[i].Name)) {
                        result.Append(" ");
                        result.Append(ps[i].Name);
                    }
                }

                result.Append(")");
                return result.ToString();
            }
#endif

            /// <exception cref="InvalidImplementationException">The type failed to instantiate.</exception>
            internal static T CreateInstance<T>(Type actualType, params object[] args) {
                Type type = typeof(T);

                Debug.Assert(type.IsAssignableFrom(actualType));
                
                try {
                    return (T)Activator.CreateInstance(actualType, args);
                } catch (TargetInvocationException e) {
                    throw new InvalidImplementationException(System.String.Format(Resources.InvalidCtorImplementation, actualType), e.InnerException);
                } catch (Exception e) {
                    throw new InvalidImplementationException(System.String.Format(Resources.InvalidCtorImplementation, actualType), e);
                }
            }
        }

        #endregion

        #region IO

        #region ASCII Encoding

        // Silverlight doesn't support ASCIIEncoding, so we have our own class
        // We still use System.Text.Encoding.ASCII for the non-Silverlight case
        public static Encoding AsciiEncoding {
            get { return _ascii; }
        }

#if !SILVERLIGHT
        static readonly Encoding _ascii = Encoding.ASCII;
#else
        static readonly Encoding _ascii = new AsciiEncodingImpl();

        // Simple implementation of ASCII encoding/decoding, meant to be
        // compatible with System.Text.ASCIIEncoding
        private class AsciiEncodingImpl : Encoding {

            internal AsciiEncodingImpl() : base(0x4e9f) { }

            public override int GetByteCount(char[] chars, int index, int count) {
                return count;
            }

            public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) {
                int charEnd = charIndex + charCount;
                while (charIndex < charEnd) {
                    byte b = (byte)chars[charIndex++];
                    bytes[byteIndex++] = (b <= 0x7f) ? b : (byte)'?';
                }
                return charCount;
            }

            public override int GetCharCount(byte[] bytes, int index, int count) {
                return count;
            }

            public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
                int byteEnd = byteIndex + byteCount;
                while (byteIndex < byteEnd) {
                    byte b = bytes[byteIndex++];
                    chars[charIndex++] = (b <= 0x7f) ? (char)b : '?';
                }
                return byteCount;
            }

            public override int GetMaxByteCount(int charCount) {
                return charCount;
            }

            public override int GetMaxCharCount(int byteCount) {
                return byteCount;
            }

            public override string WebName {
                get {
                    return "us-ascii";
                }
            }
        }
#endif

        #endregion

        #region Text Streams

        public abstract class TextStreamBase : Stream {

            private bool _buffered;

            protected TextStreamBase(bool buffered) {
                _buffered = buffered;
            }

            public abstract Encoding Encoding { get; }
            public abstract TextReader Reader { get; }
            public abstract TextWriter Writer { get; }

            public sealed override bool CanSeek {
                get { return false; }
            }

            public sealed override bool CanWrite {
                get { return Writer != null; }
            }

            public sealed override bool CanRead {
                get { return Reader != null; }
            }

            public sealed override void Flush() {
                if (!CanWrite) throw new InvalidOperationException();
                Writer.Flush();
            }

            public sealed override int Read(byte[] buffer, int offset, int count) {
                if (!CanRead) throw new InvalidOperationException();
                Array.CheckRange(buffer, offset, count, "buffer", "offset", "count");

                char[] char_buffer = new char[count];
                int real_count = Reader.Read(char_buffer, 0, count);
                return Encoding.GetBytes(char_buffer, 0, real_count, buffer, offset);
            }

            public sealed override void Write(byte[] buffer, int offset, int count) {
                Array.CheckRange(buffer, offset, count, "buffer", "offset", "count");
                char[] char_buffer = Encoding.GetChars(buffer, offset, count);
                Writer.Write(char_buffer, 0, char_buffer.Length);
                if (!_buffered) Writer.Flush();
            }

            #region Invalid Operations

            public sealed override long Length {
                get {
                    throw new InvalidOperationException();
                }
            }

            public sealed override long Position {
                get {
                    throw new InvalidOperationException();
                }
                set {
                    throw new InvalidOperationException();
                }
            }

            public sealed override long Seek(long offset, SeekOrigin origin) {
                throw new InvalidOperationException();
            }

            public sealed override void SetLength(long value) {
                throw new InvalidOperationException();
            }

            #endregion
        }

        public sealed class TextStream : TextStreamBase {

            private TextReader _reader;
            private TextWriter _writer;
            private Encoding _encoding;

            public override Encoding Encoding {
                get { return _encoding; }
            }

            public override TextReader Reader {
                get { return _reader; }
            }

            public override TextWriter Writer {
                get { return _writer; }
            }

            public TextStream(TextReader reader, Encoding encoding) 
                : base(true) {
                if (reader == null) throw new ArgumentNullException("reader");
                if (encoding == null) throw new ArgumentNullException("encoding");

                this._reader = reader;
                this._encoding = encoding;
            }

            public TextStream(TextWriter writer, Encoding encoding)
                : this(writer, encoding, true) {
            }

            public TextStream(TextWriter writer, Encoding encoding, bool buffered)
                : base(buffered) {
                if (writer == null) throw new ArgumentNullException("writer");
                if (encoding == null) throw new ArgumentNullException("encoding");

                this._writer = writer;
                this._encoding = encoding;
            }
        }

        public enum ConsoleStreamType {
            Input,
            Output,
            ErrorOutput,
        }
        
        public sealed class ConsoleStream : TextStreamBase {

            private ConsoleStreamType _type;
#if SILVERLIGHT
            // We need to return a version of the UTF8Encoding that doesn't emit the UT8
            // identifier. Otherwise we get garbage on the first line of the console output
            private static readonly Encoding _encoding = new UTF8Encoding(false);
#endif
            public override TextReader Reader {
                get { return (_type == ConsoleStreamType.Input) ? Console.In : null; }
            }

            public override TextWriter Writer {
                get { 
                    switch (_type) {
                        case ConsoleStreamType.Output: return Console.Out;
                        case ConsoleStreamType.ErrorOutput: return Console.Error; 
                    }
                    return null;
                }
            }

            public override Encoding Encoding {
                get { 
#if SILVERLIGHT
                    return _encoding;
#else
                    return CanRead ? Console.InputEncoding : Console.OutputEncoding;
#endif
                }
            }

            public ConsoleStream(ConsoleStreamType type)
                : this(type, true) {
            }

            public ConsoleStream(ConsoleStreamType type, bool buffered)
                : base(buffered) {
                _type = type;
            }
        }

        #endregion
        
        /// <summary>
        /// Seeks the first character of a specified line in the text stream.
        /// Assumes the reader is currently positioned just before the first character of the first line.
        /// Line numbers are counted starting from 1.
        /// Returns <c>true</c> if the line is found (the current position of the reader will be  
        /// character read from the reader will be the first one of the line - if there is any), <b>false</b> otherwise.
        /// </summary>
        public static bool SeekLine(TextReader reader, int line) {
            if (reader == null) throw new ArgumentNullException("reader");
            if (line < 1) throw new ArgumentOutOfRangeException("line");
            if (line == 1) return true;

            int current_line = 1;

            for(;;) {
                int c = reader.Read();

                if (c == '\r') {
                    if (reader.Peek() == '\n') {
                        reader.Read();
                    }

                    current_line++;
                    if (current_line == line) return true;

                } else if (c == '\n') {
                    current_line++;
                    if (current_line == line) return true;
                } else if (c == -1) {
                    return false;
                }
            }
        }

        /// <summary>
        /// Reads characters to a string until end position or a terminator is reached. 
        /// Doesn't include the terminator into the resulting string.
        /// Returns <c>null</c>, if the reader is at the end position.
        /// </summary>
        public static string ReadTo(TextReader reader, char terminator) {
            if (reader == null) throw new ArgumentNullException("reader");
            
            StringBuilder result = new StringBuilder();
            int ch;
            for (; ; ) {
                ch = reader.Read();

                if (ch == -1) break;
                if (ch == terminator) return result.ToString();

                result.Append((char)ch);
            }
            return (result.Length > 0) ? result.ToString() : null;
        }

        /// <summary>
        /// Reads characters until end position or a terminator is reached.
        /// Returns <c>true</c> if the character has been found (the reader is positioned right behind the character), 
        /// <c>false</c> otherwise.
        /// </summary>
        public static bool SeekTo(TextReader reader, char c) {
            if (reader == null) throw new ArgumentNullException("reader");

            for (; ; ) {
                int ch = reader.Read();
                if (ch == -1) return false;
                if (ch == c) return true;
            }
        }
        
        public static string ToValidFileName(string name) {
            if (System.String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            StringBuilder sb = new StringBuilder(name);
            foreach (char c in Path.GetInvalidPathChars())
                sb.Replace(c, '_');

            // GetInvalidNameChars not avaliable on Silverlight:
            sb.Replace('\\', '_').Replace(':', '_').Replace('*', '_').Replace('/', '_').Replace('?', '_');
            
            return sb.ToString();
        }

        #endregion

        #region Array

        public static class Array {

            public static readonly string[] EmptyStrings = new string[0];

            // copied from MSCORLIB
            public static T[] FindAll<T>(T[] array, Predicate<T> match) {
                if (array == null) {
                    throw new ArgumentNullException("array");
                }

                if (match == null) {
                    throw new ArgumentNullException("match");
                }

                List<T> list = new List<T>();
                for (int i = 0; i < array.Length; i++) {
                    if (match(array[i])) {
                        list.Add(array[i]);
                    }
                }
                return list.ToArray();
            }


            public static TElement[] FromList<TElement>(IList list) {
                TElement[] result = new TElement[list.Count];

                int i = 0;
                foreach (TElement element in list)
                    result[i++] = element;

                return result;
            }

            public static void PrintTable(TextWriter output, string[,] table) {
                if (output == null) throw new ArgumentNullException("output");
                if (table == null) throw new ArgumentNullException("table");

                int max_width = 0;
                for (int i = 0; i < table.GetLength(0); i++) {
                    if (table[i, 0].Length > max_width) {
                        max_width = table[i, 0].Length;
                    }
                }

                for (int i = 0; i < table.GetLength(0); i++) {
                    output.Write(" ");
                    output.Write(table[i, 0]);

                    for (int j = table[i, 0].Length; j < max_width + 1; j++) {
                        output.Write(' ');
                    }

                    output.WriteLine(table[i, 1]);
                }
            }

            /// <summary>
            /// Resizes an array to a speficied new size and copies a portion of the original array into its beginning.
            /// </summary>
            internal static void ResizeInternal(ref char[] array, int newSize, int start, int count) {
                Debug.Assert(array != null && newSize > 0 && count >= 0 && newSize >= count && start >= 0);

                char[] result = (newSize != array.Length) ? new char[newSize] : array;

                Buffer.BlockCopy(array, start * sizeof(char), result, 0, count * sizeof(char));

                array = result;
            }

            public static void CheckRange(byte[] array, int offset, int count, string arrayName, string offsetName, string countName) {
                if (array == null) throw new ArgumentNullException(arrayName);
                if (offset < 0 || offset > array.Length) throw new ArgumentOutOfRangeException(offsetName);
                if (count < 0 || count > array.Length - offset) throw new ArgumentOutOfRangeException(countName);
            }

            public static void CheckNonNullElements(object[] array, string arrayName) {
                if (array == null) throw new ArgumentNullException(arrayName);
                for (int i = 0; i < array.Length; i++) {
                    if (array[i] == null) {
                        throw new ArgumentNullException(System.String.Format("{0}[{1}]", arrayName, i));
                    }
                }
            }

            internal static T[] Copy<T>(T[] array) {
                return (array.Length > 0) ? (T[])array.Clone() : array; 
            }
        }

        #endregion

        #region String

        public static class String {

            public static string GetSuffix(string str, char separator, bool includeSeparator) {
                if (str == null) throw new ArgumentNullException("str");
                int last = str.LastIndexOf(separator);
                return (last != -1) ? str.Substring(includeSeparator ? last : last + 1) : null;
            }

            public static string GetLongestPrefix(string str, char separator, bool includeSeparator) {
                if (str == null) throw new ArgumentNullException("str");
                int last = str.LastIndexOf(separator);
                return (last != -1) ? str.Substring(0, (includeSeparator || last == 0) ? last : last - 1) : null;
            }

            public static int CountOf(string str, char c) {
                if (System.String.IsNullOrEmpty(str)) return 0;

                int result = 0;
                for (int i = 0; i < str.Length; i++) {
                    if (c == str[i]) {
                        result++;
                    }
                }
                return result;
            }

            public static string[] Split(string str, string separator, int maxComponents, StringSplitOptions options) {
#if SILVERLIGHT
                if (str == null) throw new ArgumentNullException("str");
                if (string.IsNullOrEmpty(separator)) throw new ArgumentNullException("separator");

                bool keep_empty = (options & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries;

                List<string> result = new List<string>(maxComponents == Int32.MaxValue ? 1 : maxComponents + 1);

                int i = 0;
                int next;
                while (maxComponents > 1 && i < str.Length && (next = str.IndexOf(separator, i)) != -1) {

                    if (next > i || keep_empty) {
                        result.Add(str.Substring(i, next - i));
                        maxComponents--;
                    }

                    i = next + separator.Length;
                }

                if (i < str.Length || keep_empty) {
                    result.Add(str.Substring(i));
                }

                return result.ToArray();
#else
                return str.Split(new string[] { separator }, maxComponents, options);
#endif
            }

            public static string[] Split(string str, char[] separators, int maxComponents, StringSplitOptions options) {
#if SILVERLIGHT
                if (str == null) throw new ArgumentNullException("str");
                if (separators == null) return SplitOnWhiteSpace(str, maxComponents, options);

                bool keep_empty = (options & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries;

                List<string> result = new List<string>(maxComponents == Int32.MaxValue ? 1 : maxComponents + 1);

                int i = 0;
                int next;
                while (maxComponents > 1 && i < str.Length && (next = str.IndexOfAny(separators, i)) != -1) {

                    if (next > i || keep_empty) {
                        result.Add(str.Substring(i, next - i));
                        maxComponents--;
                    }

                    i = next + 1;
                }

                if (i < str.Length || keep_empty) {
                    result.Add(str.Substring(i));
                }

                return result.ToArray();
#else
                return str.Split(separators, maxComponents, options);
#endif
            }

#if SILVERLIGHT
            public static string[] SplitOnWhiteSpace(string str, int maxComponents, StringSplitOptions options) {
                if (str == null) throw new ArgumentNullException("str");

                bool keep_empty = (options & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries;

                List<string> result = new List<string>(maxComponents == Int32.MaxValue ? 1 : maxComponents + 1);

                int i = 0;
                int next;
                while (maxComponents > 1 && i < str.Length && (next = IndexOfWhiteSpace(str, i)) != -1) {

                    if (next > i || keep_empty) {
                        result.Add(str.Substring(i, next - i));
                        maxComponents--;
                    }

                    i = next + 1;
                }

                if (i < str.Length || keep_empty) {
                    result.Add(str.Substring(i));
                }

                return result.ToArray();
            }

            public static int IndexOfWhiteSpace(string str, int start) {
                if (str == null) throw new ArgumentNullException("str");
                if (start < 0 || start > str.Length) throw new ArgumentOutOfRangeException("start");

                while (start < str.Length && !Char.IsWhiteSpace(str[start])) start++;

                return (start == str.Length) ? -1 : start;
            }
#endif

            /// <summary>
            /// Splits text and optionally indents first lines - breaks along words, not characters.
            /// </summary>
            public static string SplitWords(string text, bool indentFirst, int lineWidth) {
                if (text == null) throw new ArgumentNullException("text");

                const string indent = "    ";

                if (text.Length <= lineWidth || lineWidth <= 0) {
                    if (indentFirst) return indent + text;
                    return text;
                }

                StringBuilder res = new StringBuilder();
                int start = 0, len = lineWidth;
                while (start != text.Length) {
                    if (len >= lineWidth) {
                        // find last space to break on
                        while (len != 0 && !Char.IsWhiteSpace(text[start + len - 1]))
                            len--;
                    }

                    if (res.Length != 0) res.Append(' ');
                    if (indentFirst || res.Length != 0) res.Append(indent);

                    if (len == 0) {
                        int copying = System.Math.Min(lineWidth, text.Length - start);
                        res.Append(text, start, copying);
                        start += copying;
                    } else {
                        res.Append(text, start, len);
                        start += len;
                    }
                    res.AppendLine();
                    len = System.Math.Min(lineWidth, text.Length - start);
                }
                return res.ToString();
            }

            public static string AddSlashes(string str) {
                if (str == null) throw new ArgumentNullException("str");
                
                // TODO: optimize
                StringBuilder result = new StringBuilder(str.Length);
                for (int i = 0; i < str.Length; i++) {
                    switch (str[i]) {
                        case '\a': result.Append("\\a"); break;
                        case '\b': result.Append("\\b"); break;
                        case '\f': result.Append("\\f"); break;
                        case '\n': result.Append("\\n"); break;
                        case '\r': result.Append("\\r"); break;
                        case '\t': result.Append("\\t"); break;
                        case '\v': result.Append("\\v"); break;
                        default: result.Append(str[i]); break;
                    }
                }

                return result.ToString();
            }
        }

        #endregion

        #region Environment

        public static class Environment {

#if SILVERLIGHT
            private static UnhandledExceptionEventHandler unhandledExceptionEventHandler;
            private static string[] commandLineArgs;

            public class ExitProcessException : Exception {

                public int ExitCode { get { return exitCode; } }
                int exitCode;

                public ExitProcessException(int exitCode) {
                    this.exitCode = exitCode;
                }
            }

            public static void ExitProcess(int exitCode) {
                throw new ExitProcessException(exitCode);
            }
#else
            public static bool IsOrcas {
                get {
                    Type t = typeof(object).Assembly.GetType("System.DateTimeOffset", false);
                    return t != null;
                }
            }
#endif

            public delegate int MainRoutine();

            public static int RunMain(MainRoutine main, string[] args) {
                Debug.Assert(main != null && args != null);
#if SILVERLIGHT
                commandLineArgs = args;
                try {
                    return main();
                } catch (ExitProcessException e) {
                    // Environment.Exit:
                    return e.ExitCode;
                } catch (Exception e) {

                    // unhandled exceptions:
                    if (unhandledExceptionEventHandler != null)
                        unhandledExceptionEventHandler(null, new UnhandledExceptionEventArgs(e, true));
                    else
                        throw;

                    return 1;
                }
#else
                return main();
#endif
            }

            public static string[] GetCommandLineArgs() {
#if SILVERLIGHT
                return (string[])commandLineArgs.Clone();
#else
                return System.Environment.GetCommandLineArgs();
#endif
            }

            public static void AddUnhandledExceptionHandler(UnhandledExceptionEventHandler handler) {
#if SILVERLIGHT
                unhandledExceptionEventHandler += handler;
#else
                AppDomain.CurrentDomain.UnhandledException += handler;
#endif
            }


        }

        #endregion

        #region Parsing

        public static bool TryParseDouble(string s, NumberStyles style, NumberFormatInfo provider, out double result) {
#if SILVERLIGHT // Double.TryParse
            try {
                result = Double.Parse(s, style, provider);
                return true;
            } catch {
                result = 0.0;
                return false;
            }
#else
            return Double.TryParse(s, style, provider, out result);
#endif
        }

        public static bool TryParseInt32(string s, out int result) {
#if SILVERLIGHT // Int32.TryParse
            try {
                result = Int32.Parse(s);
                return true;
            } catch {
                result = 0;
                return false;
            }
#else
            return Int32.TryParse(s, out result);
#endif
        }

        public static bool TryParseDateTimeExact(string s, string format, DateTimeFormatInfo provider, DateTimeStyles style, out DateTime result) {
#if SILVERLIGHT // DateTime.ParseExact
            try {
                result = DateTime.ParseExact(s, format, provider, style);
                return true;
            } catch {
                result = DateTime.MinValue;
                return false;
            }
#else
            return DateTime.TryParseExact(s, format, provider, style, out result);
#endif
        }

        public static bool TryParseDate(string s, IFormatProvider provider, DateTimeStyles style, out DateTime result) {
#if SILVERLIGHT // DateTime.Parse
            try {
                result = DateTime.Parse(s, provider, style);
                return true;
            } catch {
                result = DateTime.MinValue;
                return false;
            }
#else
            return DateTime.TryParse(s, provider, style, out result);
#endif
        }

        #endregion

        #region Weak References

#if SILVERLIGHT
        public struct WeakHandle {

            private class SupressableWeakReference : WeakReference {
                public SupressableWeakReference(object target, bool trackResurrection)
                    : base(target, trackResurrection) {
                }
            }

            private SupressableWeakReference weakRef;

            public WeakHandle(object target, bool trackResurrection) {
                this.weakRef = new SupressableWeakReference(target, trackResurrection);
                GC.SuppressFinalize(this.weakRef);
            }

            public bool IsAlive { get { return weakRef != null && weakRef.IsAlive; } }
            public object Target { get { return weakRef != null ? weakRef.Target : null; } }
            
            public void Free() { 
                if (weakRef != null) {
                    GC.ReRegisterForFinalize(weakRef);
                    weakRef.Target = null;
                    weakRef = null;
                } 
            }
        }
#else
        public struct WeakHandle {

            private GCHandle weakRef;

            public WeakHandle(object target, bool trackResurrection) {
                this.weakRef = GCHandle.Alloc(target, trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
            }

            public bool IsAlive { get { return weakRef.IsAllocated; } }
            public object Target { get { return weakRef.Target; } }
            public void Free() { weakRef.Free(); }
        }
#endif

        #endregion

        #region Collections

        public static IEnumerator<KeyValuePair<KSuper, VSuper>> ToCovariantEnumerator<K, V, KSuper, VSuper>(IEnumerator<KeyValuePair<K, V>> enumerator)
            where K : KSuper
            where V : VSuper {

            if (enumerator == null) throw new ArgumentNullException("enumerator");

            while (enumerator.MoveNext()) {
                KeyValuePair<K, V> entry = enumerator.Current;
                yield return new KeyValuePair<KSuper, VSuper>((KSuper)entry.Key, (VSuper)entry.Value);
            }
        }

        public static IDictionaryEnumerator ToDictionaryEnumerator<K, V>(IEnumerator<KeyValuePair<K, V>> enumerator) {
            if (enumerator == null) throw new ArgumentNullException("enumerator");
            return new DictionaryEnumerator<K, V>(enumerator);
        }

        private sealed class DictionaryEnumerator<K, V> : IDictionaryEnumerator {
            IEnumerator<KeyValuePair<K, V>> enumerator;

            public DictionaryEnumerator(IEnumerator<KeyValuePair<K, V>> e) {
                enumerator = e;
            }

            #region IDictionaryEnumerator Members

            public DictionaryEntry Entry {
                get { 
                    KeyValuePair<K, V> entry = enumerator.Current;
                    return new DictionaryEntry(entry.Key, entry.Value); 
                }
            }

            public object Key {
                get { return enumerator.Current.Key; }
            }

            public object Value {
                get { return enumerator.Current.Value; }
            }

            #endregion

            #region IEnumerator Members

            public object Current {
                get { return Entry; }
            }

            public bool MoveNext() {
                return enumerator.MoveNext();
            }

            public void Reset() {
                enumerator.Reset();
            }

            #endregion
        }

        public static List<T> MakeList<T>(T item) {
            List<T> result = new List<T>();
            result.Add(item);
            return result;
        }

        #endregion

        #region Misc

#pragma warning disable 414 // Private field assigned but its value never used

        /// <summary>
        /// Volatile field. Written to when we need to do a memory barrier on Silverlight.
        /// </summary>
        private static volatile int _memoryBarrier;

        public static void MemoryBarrier() {
           _memoryBarrier = 1;
        }

#pragma warning restore 414

        public static ArgumentOutOfRangeException MakeArgumentOutOfRangeException(string paramName, object actualValue, string message) {
#if SILVERLIGHT // ArgumentOutOfRangeException ctor overload
            throw new ArgumentOutOfRangeException(paramName, string.Format("{0} (actual value is '{1}')", message, actualValue));
#else
            throw new ArgumentOutOfRangeException(paramName, actualValue, message);
#endif
        }

        public static bool IsRemote(object obj) {
#if !SILVERLIGHT
            return System.Runtime.Remoting.RemotingServices.IsObjectOutOfAppDomain(obj);
#else
            return false;
#endif
        }

#if SILVERLIGHT
        private static WeakHash<Exception, IDictionary> _exceptionData;
#endif
        public static IDictionary GetDataDictionary(Exception e) {
#if SILVERLIGHT
            if(_exceptionData == null) {
                Interlocked.CompareExchange(ref _exceptionData, new WeakHash<Exception, IDictionary>(), null);
            }

            lock(_exceptionData) {
                IDictionary res;
                if(_exceptionData.TryGetValue(e, out res)) return res;

                res = new Dictionary<object, object>();
                _exceptionData[e] = res;
                return res;
            }
#else
            return e.Data;
#endif
        }

#if !SILVERLIGHT
        internal static bool MakeHandle(bool valid, object obj, out IObjectHandle handle) {
            handle = valid ? new ObjectHandle(obj) : null;
            return valid;
        }
#endif
        #endregion
    }
}
