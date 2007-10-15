using System;
using System.Collections.Generic;
using System.Text;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    /// <summary>
    /// Simple implementation of ASCII encoding/decoding.  The default instance (PythonAsciiEncoding.Instance) is
    /// setup to always convert even values outside of the ASCII range.  The EncoderFallback/DecoderFallbacks can
    /// be replaced with versions that will throw exceptions instead though.
    /// </summary>
    sealed class PythonAsciiEncoding : Encoding {
        internal static Encoding Instance = MakeNonThrowing();

        internal PythonAsciiEncoding()
            : base() {
        }

        internal static Encoding MakeNonThrowing() {
            // we need to Clone the new instance here so that the base class marks us as non-readonly
            Encoding enc = (Encoding)new PythonAsciiEncoding().Clone();
#if !SILVERLIGHT
            enc.DecoderFallback = new NonStrictDecoderFallback();
#endif
            enc.EncoderFallback = new NonStrictEncoderFallback();
            return enc;
        }

        public override int GetByteCount(char[] chars, int index, int count) {
            int byteCount = 0;
            int charEnd = index + count;
            while (index < charEnd) {
                char c = chars[index];
                if (c > 0x7f) {
                    EncoderFallbackBuffer efb = EncoderFallback.CreateFallbackBuffer();
                    if (efb.Fallback(c, index)) {
                        byteCount += efb.Remaining;
                    }
                } else {
                    byteCount++;
                }
                index++;
            }
            return byteCount;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) {
            int charEnd = charIndex + charCount;
            while (charIndex < charEnd) {
                char c = chars[charIndex];                
                if (c > 0x7f) {
                    EncoderFallbackBuffer efb = EncoderFallback.CreateFallbackBuffer();
                    if (efb.Fallback(c, charIndex)) {
                        while (efb.Remaining != 0) {
                            bytes[byteIndex++] = (byte)efb.GetNextChar();
                        }
                    }
                } else {
                    bytes[byteIndex++] = (byte)c;
                }
                charIndex++;
            }
            return charCount;
        }

        public override int GetCharCount(byte[] bytes, int index, int count) {
            return count;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
            int byteEnd = byteIndex + byteCount;
            while (byteIndex < byteEnd) {
                byte b = bytes[byteIndex];
                if (b > 0x7f) {
                    DecoderFallbackBuffer dfb = DecoderFallback.CreateFallbackBuffer();
                    if (dfb.Fallback(new byte[] { b }, byteIndex)) {
                        while (dfb.Remaining != 0) {
                            chars[charIndex++] = dfb.GetNextChar();
                        }
                    }
                } else {
                    chars[charIndex++] = (char)b;
                }
                byteIndex++;
            }
            return byteCount;
        }

        public override int GetMaxByteCount(int charCount) {
            return charCount * 4;
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

    class NonStrictEncoderFallback : EncoderFallback {
        public override EncoderFallbackBuffer CreateFallbackBuffer() {
            return new NonStrictEncoderFallbackBuffer();
        }
        
        public override int MaxCharCount {
            get { return 1; }
        }
    }

    class NonStrictEncoderFallbackBuffer : EncoderFallbackBuffer {
        private List<char> _buffer = new List<char>();
        private int _index;

        public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index) {
            throw PythonOps.UnicodeDecodeError("'ascii' codec can't encode character '\\u{0:X}{1:04X}' in position {1}", (int)charUnknownHigh, (int)charUnknownLow, index);
        }

        public override bool Fallback(char charUnknown, int index) {
            if (charUnknown > 0xff) {
                throw PythonOps.UnicodeDecodeError("'ascii' codec can't encode character '\\u{0:X}' in position {1}", (int)charUnknown, index);
            }

            _buffer.Add(charUnknown);
            return true;
        }
        
        public override char GetNextChar() {
            return _buffer[_index++];
        }

        public override bool MovePrevious() {
            if (_index > 0) {
                _index--;
                return true;
            }
            return false;
        }

        public override int Remaining {
            get { return _buffer.Count - _index; }
        }
    }

#if !SILVERLIGHT
    class NonStrictDecoderFallback : DecoderFallback {
        public override DecoderFallbackBuffer CreateFallbackBuffer() {
            return new NonStrictDecoderFallbackBuffer();
        }

        public override int MaxCharCount {
            get { return 1; }
        }
    }

    // no ctors on DecoderFallbackBuffer in Silverlight
    class NonStrictDecoderFallbackBuffer : DecoderFallbackBuffer {
        private List<byte> _bytes = new List<byte>();
        private int _index;

        public override bool Fallback(byte[] bytesUnknown, int index) {
            _bytes.AddRange(bytesUnknown);
            return true;
        }

        public override char GetNextChar() {
            return (char)_bytes[_index++];
        }

        public override bool MovePrevious() {
            if (_index > 0) {
                _index--;
                return true;
            }
            return false;
        }

        public override int Remaining {
            get { return _bytes.Count - _index; }
        }
    }
#endif
}
