/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Scripting;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using Microsoft.Scripting.Utils;

[assembly: PythonModule("_codecs", typeof(IronPython.Modules.PythonCodecs))]
namespace IronPython.Modules {

    [PythonType("_codecs")]
    public static class PythonCodecs {        
        internal const int EncoderIndex = 0;
        internal const int DecoderIndex = 1;
        internal const int StreamReaderIndex = 2;
        internal const int StreamWriterIndex = 3;

        private static PythonTuple DoDecode(Encoding encoding, object input, string errors) {
            return DoDecode(encoding, input, errors, false);
        }

        private static PythonTuple DoDecode(Encoding encoding, object input, string errors, bool fAlwaysThrow) {
            // input should be character buffer of some form...
            string res;
            if (Converter.TryConvertToString(input, out res)) {
                int preOffset = CheckPreamble(encoding, res);

                byte[] bytes = new byte[res.Length - preOffset];
                for (int i = 0; i < bytes.Length; i++) {
                    bytes[i] = (byte)res[i + preOffset];
                }

#if !SILVERLIGHT    // DecoderFallback
                encoding = (Encoding)encoding.Clone();

                ExceptionFallBack fallback = null;
                if (fAlwaysThrow) {
                    encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
                } else {
                    fallback = new ExceptionFallBack(bytes);
                    encoding.DecoderFallback = fallback;
                }
#endif
                string decoded = encoding.GetString(bytes, 0, bytes.Length);
                int badByteCount = 0;

#if !SILVERLIGHT    // DecoderFallback
                if (!fAlwaysThrow) {
                    byte[] badBytes = fallback.buffer.badBytes;
                    if (badBytes != null) {
                        badByteCount = badBytes.Length;
                    }
                }
#endif

                PythonTuple tuple = PythonTuple.MakeTuple(decoded, bytes.Length - badByteCount);
                return tuple;
            } else {
                throw PythonOps.TypeErrorForBadInstance("argument 1 must be string, got {0}", input);
            }
        }

        private static int CheckPreamble(Encoding enc, string buffer) {
            byte[] preamble = enc.GetPreamble();

            if (preamble.Length != 0 && buffer.Length >= preamble.Length) {
                bool hasPreamble = true;
                for (int i = 0; i < preamble.Length; i++) {
                    if (preamble[i] != (byte)buffer[i]) {
                        hasPreamble = false;
                        break;
                    }
                }
                if (hasPreamble) {
                    return preamble.Length;
                }
            }
            return 0;
        }

        private static PythonTuple DoEncode(Encoding encoding, object input, string errors) {
            return DoEncode(encoding, input, errors, false);
        }

        private static PythonTuple DoEncode(Encoding encoding, object input, string errors, bool includePreamble) {
            // input should be some Unicode object
            string res;
            if (Converter.TryConvertToString(input, out res)) {
                StringBuilder sb = new StringBuilder();

                encoding = (Encoding)encoding.Clone();

#if !SILVERLIGHT // EncoderFallback
                encoding.EncoderFallback = EncoderFallback.ExceptionFallback;
#endif

                if (includePreamble) {
                    byte[] preamble = encoding.GetPreamble();
                    for (int i = 0; i < preamble.Length; i++) {
                        sb.Append((char)preamble[i]);
                    }
                }

                byte[] bytes = encoding.GetBytes(res);
                for (int i = 0; i < bytes.Length; i++) {
                    sb.Append((char)bytes[i]);
                }
                return PythonTuple.MakeTuple(sb.ToString(), res.Length);
            }
            throw PythonOps.TypeErrorForBadInstance("cannot decode {0}", input);
        }

        #region ASCII Encoding
        [PythonName("ascii_decode")]
        public static object AsciiDecode(object input) {
            return AsciiDecode(input, "strict");
        }

        [PythonName("ascii_decode")]
        public static object AsciiDecode(object input, string errors) {
            return DoDecode(PythonAsciiEncoding.Instance, input, errors, true);
        }

        [PythonName("ascii_encode")]
        public static object AsciiEncode(object input) {
            return AsciiEncode(input, "strict");
        }

        [PythonName("ascii_encode")]
        public static object AsciiEncode(object input, string errors) {
            return DoEncode(PythonAsciiEncoding.Instance, input, errors);
        }

        #endregion

        [PythonName("charbuffer_encode")]
        public static object CharBufferEncode() {
            throw PythonOps.NotImplementedError("charbuffer_encode");
        }

        [PythonName("charmap_decode")]
        public static object CharMapDecode(string input, [Optional]string errors, [Optional]IDictionary<object, object> map) {
            if (input.Length == 0) return String.Empty;

            StringBuilder res = new StringBuilder();
            for (int i = 0; i < input.Length; i++) {
                object val;

                if (map == null) {
                    res.Append(input[i]);
                    continue;
                } 
                
                if (!map.TryGetValue((int)input[i], out val)) {
                    if (errors == "strict") throw PythonOps.LookupError("failed to find key in mapping");
                    continue;
                }

                res.Append((char)(int)val);
            }
            return PythonTuple.MakeTuple(res.ToString(), res.Length);
        }

        [PythonName("charmap_encode")]
        public static object CharMapEncode(string input, string errors, IDictionary<object, object> map) {
            return CharMapDecode(input, errors, map);
        }

        [PythonName("decode")]
        public static object Decode(CodeContext context, object obj) {
            PythonTuple t = Lookup(SystemState.Instance.GetDefaultEncoding());

            return PythonOps.GetIndex(PythonCalls.Call(t[DecoderIndex], obj, null), 0);
        }

        [PythonName("decode")]
        public static object Decode(CodeContext context, object obj, string encoding) {
            PythonTuple t = Lookup(encoding);

            return PythonOps.GetIndex(PythonCalls.Call(t[DecoderIndex], obj, null), 0);
        }

        [PythonName("decode")]
        public static object Decode(CodeContext context, object obj, string encoding, string errors) {
            PythonTuple t = Lookup(encoding);

            return PythonOps.GetIndex(PythonCalls.Call(t[DecoderIndex], obj, errors), 0);
        }

        [PythonName("encode")]
        public static object Encode(CodeContext context, object obj) {
            PythonTuple t = Lookup(SystemState.Instance.GetDefaultEncoding());

            return PythonOps.GetIndex(PythonCalls.Call(t[EncoderIndex], obj, null), 0);
        }

        [PythonName("encode")]
        public static object Encode(CodeContext context, object obj, string encoding) {
            PythonTuple t = Lookup(encoding);

            return PythonOps.GetIndex(PythonCalls.Call(t[EncoderIndex], obj, null), 0);
        }

        [PythonName("encode")]
        public static object Encode(CodeContext context, object obj, string encoding, string errors) {
            PythonTuple t = Lookup(encoding);

            return PythonOps.GetIndex(PythonCalls.Call(t[EncoderIndex], obj, errors), 0);
        }

        [PythonName("escape_decode")]
        public static object EscapeDecode(string text) {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < text.Length; i++) {

                if (text[i] == '\\') {
                    if (i == text.Length - 1) throw PythonOps.ValueError("\\ at end of string");

                    switch (text[++i]) {
                        case 'a': res.Append((char)0x07); break;
                        case 'b': res.Append((char)0x08); break;
                        case 't': res.Append('\t'); break;
                        case 'n': res.Append('\n'); break;
                        case 'r': res.Append('\r'); break;
                        case '\\': res.Append('\\'); break;
                        case 'f': res.Append((char)0x0c); break;
                        case 'v': res.Append((char)0x0b); break;
                        case 'x':
                            if (i >= text.Length - 2) throw PythonOps.ValueError("invalid character value");

                            res.Append(CharToInt(text[i]) * 16 + CharToInt(text[i + 1]));
                            i += 2;
                            break;
                        default:
                            res.Append("\\" + text[i - 1]);
                            break;
                    }
                } else {
                    res.Append(text[i]);
                }

            }
            return PythonTuple.MakeTuple(res.ToString(), text.Length);
        }

        static int CharToInt(char ch) {
            if (Char.IsDigit(ch)) {
                return ch - '0';
            }
            ch = Char.ToUpper(ch);
            if (ch >= 'A' && ch <= 'F') return ch - 'A' + 10;
            throw PythonOps.ValueError("invalid hexadecimal digit");
        }

        [PythonName("escape_encode")]
        public static object EscapeEncode(string text) {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < text.Length; i++) {
                switch (text[i]) {
                    case '\n': res.Append("\\n"); break;
                    case '\r': res.Append("\\r"); break;
                    case '\t': res.Append("\\t"); break;
                    case (char)0x07: res.Append("\\a"); break;
                    case (char)0x08: res.Append("\\b"); break;
                    case '\\': res.Append("\\\\"); break;
                    case (char)0x0c: res.Append("\\f"); break;
                    case (char)0x0b: res.Append("\\v"); break;
                    default:
                        res.Append(text[i]);
                        break;
                }
            }
            return res.ToString();
        }

        #region Latin-1 Functions

#if !SILVERLIGHT
        
        [PythonName("latin_1_decode")]
        public static object Latin1Decode(object input) {
            return Latin1Decode(input, "strict");
        }

        [PythonName("latin_1_decode")]
        public static object Latin1Decode(object input, string errors) {
            return DoDecode(Encoding.GetEncoding("iso-8859-1"), input, errors);
        }

        [PythonName("latin_1_encode")]
        public static object Latin1Encode(object input) {
            return Latin1Encode(input, "strict");
        }

        [PythonName("latin_1_encode")]
        public static object Latin1Encode(object input, string errors) {
            return DoEncode(Encoding.GetEncoding("iso-8859-1"), input, errors);
        }

#endif

        #endregion

        [PythonName("lookup")]
        public static PythonTuple Lookup(string encoding) {
            return PythonOps.LookupEncoding(encoding);
        }

#if !SILVERLIGHT
        [PythonName("lookup_error")]
        public static object LookupError(string name) {
            return PythonOps.LookupEncodingError(name);
        }
#endif

        #region MBCS Functions

        [PythonName("mbcs_decode")]
        public static object MbcsDecode() {
            throw PythonOps.NotImplementedError("mbcs_decode");
        }

        [PythonName("mbcs_encode")]
        public static object MbcsEncode() {
            throw PythonOps.NotImplementedError("mbcs_encode");
        }

        #endregion

        [PythonName("raw_unicode_escape_decode")]
        public static PythonTuple RawUnicodeEscapeDecode(CodeContext context, object input, [DefaultParameterValue("strict")]string errors) {
            return PythonTuple.MakeTuple(
                StringOps.Decode(context, Converter.ConvertToString(input), "raw-unicode-escape", errors),
                Builtin.len(input)
            );
        }

        [PythonName("raw_unicode_escape_encode")]
        public static PythonTuple RawUnicodeEscapeEncode(CodeContext context, object input, [DefaultParameterValue("strict")]string errors) {
            return PythonTuple.MakeTuple(
                StringOps.Encode(context, Converter.ConvertToString(input), "raw-unicode-escape", errors),
                Builtin.len(input)
            );
        }

        [PythonName("readbuffer_encode")]
        public static object ReadBufferEncode() {
            throw PythonOps.NotImplementedError("readbuffer_encode");
        }

        [PythonName("register")]
        public static void Register(object search_function) {
            PythonOps.RegisterEncoding(search_function);
        }

#if !SILVERLIGHT
        [PythonName("register_error")]
        public static void RegisterError(string name, object handler) {
            PythonOps.RegisterEncodingError(name, handler);
        }
#endif

        #region Unicode Escape Encoding

        [PythonName("unicode_escape_decode")]
        public static object UnicodeEscapeDecode() {
            throw PythonOps.NotImplementedError("unicode_escape_decode");
        }

        [PythonName("unicode_escape_encode")]
        public static object UnicodeEscapeEncode() {
            throw PythonOps.NotImplementedError("unicode_escape_encode");
        }

        [PythonName("unicode_internal_decode")]
        public static object UnicodeInternalDecode(object input, string errors) {
            return Utf16Decode(input, errors);
        }

        [PythonName("unicode_internal_encode")]
        public static object UnicodeInternalEncode(object input, string errors) {
            return Utf16LittleEndianEncode(input, errors);
        }

        #endregion

        #region Utf-16 Big Endian Functions

        [PythonName("utf_16_be_decode")]
        public static object Utf16BeDecode(object input) {
            return Utf16BeDecode(input, "strict");
        }

        [PythonName("utf_16_be_decode")]
        public static object Utf16BeDecode(object input, string errors) {
            return DoDecode(Encoding.BigEndianUnicode, input, errors);
        }

        [PythonName("utf_16_be_encode")]
        public static object Utf16BigEndianEncode(object input) {
            return Utf16BigEndianEncode(input, "strict");
        }

        [PythonName("utf_16_be_encode")]
        public static object Utf16BigEndianEncode(object input, string errors) {
            return DoEncode(Encoding.BigEndianUnicode, input, errors);
        }

        #endregion

        #region Utf-16 Functions

        [PythonName("utf_16_decode")]
        public static object Utf16Decode(object input) {
            return Utf16Decode(input, "strict");
        }

        [PythonName("utf_16_decode")]
        public static object Utf16Decode(object input, string errors) {
            return DoDecode(Encoding.Unicode, input, errors);
        }

        [PythonName("utf_16_encode")]
        public static object Utf16Encode(object input) {
            return Utf16Encode(input, "strict");
        }

        [PythonName("utf_16_encode")]
        public static object Utf16Encode(object input, string errors) {
            return DoEncode(Encoding.Unicode, input, errors, true);
        }

        #endregion

        [PythonName("utf_16_ex_decode")]
        public static object Utf16ExtDecode(object input, [Optional]string errors) {
            return Utf16ExtDecode(input, errors, null, null);
        }

        [PythonName("utf_16_ex_decode")]
        public static object Utf16ExtDecode(object input, string errors, object unknown1, object unknown2) {
            byte[] lePre = Encoding.Unicode.GetPreamble();
            byte[] bePre = Encoding.BigEndianUnicode.GetPreamble();

            string instr = Converter.ConvertToString(input);
            bool match = true;
            for (int i = 0; i < lePre.Length; i++) {
                if ((byte)instr[i] != lePre[i]) {
                    match = false;
                    break;
                }
            }
            if (match) {
                return PythonTuple.MakeTuple(String.Empty, lePre.Length, -1);
            }
            match = true;

            for (int i = 0; i < bePre.Length; i++) {
                if ((byte)instr[i] != bePre[i]) {
                    match = false;
                    break;
                }
            }

            if (match) {
                return PythonTuple.MakeTuple(String.Empty, bePre.Length, 1);
            }

            PythonTuple res = Utf16Decode(input, errors) as PythonTuple;
            return PythonTuple.MakeTuple(res[0], res[1], 0);
        }

        #region Utf-16 Le Functions
        [PythonName("utf_16_le_decode")]
        public static object Utf16LittleEndianDecode(object input) {
            return Utf16Decode(input, "strict");
        }

        [PythonName("utf_16_le_decode")]
        public static object Utf16LittleEndianDecode(object input, string errors) {
            return Utf16Decode(input, errors);
        }

        [PythonName("utf_16_le_encode")]
        public static object Utf16LittleEndianEncode(object input) {
            return Utf16LittleEndianEncode(input, "strict");
        }

        [PythonName("utf_16_le_encode")]
        public static object Utf16LittleEndianEncode(object input, string errors) {
            return DoEncode(Encoding.Unicode, input, errors);
        }
        #endregion

        #region Utf-7 Functions

#if !SILVERLIGHT
        [PythonName("utf_7_decode")]
        public static object Utf7Decode(object input) {
            return Utf7Decode(input, "strict");
        }

        [PythonName("utf_7_decode")]
        public static object Utf7Decode(object input, string errors) {
            return DoDecode(Encoding.UTF7, input, errors);
        }

        [PythonName("utf_7_encode")]
        public static object Utf7Encode(object input) {
            return Utf7Encode(input, "strict");
        }

        [PythonName("utf_7_encode")]
        public static object Utf7Encode(object input, string errors) {
            return DoEncode(Encoding.UTF7, input, errors);
        }
#endif

        #endregion

        #region Utf-8 Functions
        [PythonName("utf_8_decode")]
        public static object Utf8Decode(object input) {
            return Utf8Decode(input, "strict");
        }

        [PythonName("utf_8_decode")]
        public static object Utf8Decode(object input, string errors) {
            return DoDecode(Encoding.UTF8, input, errors);
        }

        [PythonName("utf_8_encode")]
        public static object Utf8Encode(object input) {
            return Utf8Encode(input, "strict");
        }

        [PythonName("utf_8_encode")]
        public static object Utf8Encode(object input, string errors) {
            return DoEncode(Encoding.UTF8, input, errors);
        }

        #endregion
    }

#if !SILVERLIGHT    // Encoding
    class ExceptionFallBack : DecoderFallback {
        internal ExceptionFallbackBuffer buffer;

        public ExceptionFallBack(byte[] bytes) {
            buffer = new ExceptionFallbackBuffer(bytes);
        }

        public override DecoderFallbackBuffer CreateFallbackBuffer() {
            return buffer;
        }

        public override int MaxCharCount {
            get { return 100; }
        }
    }

    class ExceptionFallbackBuffer : DecoderFallbackBuffer {
        internal byte[] badBytes;
        private byte[] inputBytes;
        public ExceptionFallbackBuffer(byte[] bytes) {
            inputBytes = bytes;
        }

        public override bool Fallback(byte[] bytesUnknown, int index) {
            if (index > 0 && index + bytesUnknown.Length != inputBytes.Length) {
                throw PythonOps.UnicodeDecodeError("failed to decode bytes at index {0}", index);
            }

            // just some bad bytes at the end
            badBytes = bytesUnknown;
            return false;
        }

        public override char GetNextChar() {
            return ' ';
        }

        public override bool MovePrevious() {
            return false;
        }

        public override int Remaining {
            get { return 0; }
        }
    }
#endif

}
