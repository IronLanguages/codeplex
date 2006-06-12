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
using System.Text;
using System.Globalization;
using System.Collections;
using System.Diagnostics;
using IronMath;

namespace IronPython.Runtime {
    /// <summary>
    /// StringFormatter provides Python's % style string formatting services.
    /// </summary>
    public class StringFormatter {

        // This is a ThreadStatic since so that formatting operations on one thread do not interfere with other threads
        [ThreadStatic]
        private static NumberFormatInfo numberFormatInfoForThread;
        private static NumberFormatInfo nfi {
            get {
                if (numberFormatInfoForThread == null) {
                    NumberFormatInfo numberFormatInfo = new CultureInfo("en-US", false).NumberFormat;
                    // The CLI formats as "Infinity", but CPython formats differently
                    numberFormatInfo.PositiveInfinitySymbol = "1.#INF";
                    numberFormatInfo.NegativeInfinitySymbol = "-1.#INF";
                    numberFormatInfo.NaNSymbol = "-1.#IND";

                    numberFormatInfoForThread = numberFormatInfo;
                }
                return numberFormatInfoForThread;
            }
        }

        #region Instance members
        const int UnspecifiedPrecision = -1; // Use the default precision

        private object data;
        private int dataIndex;

        private string str;
        private int index;
        private char curCh;

        // The options for formatting the current formatting specifier in the format string
        internal FormatSettings opts;
        // Should ddd.0 be displayed as "ddd" or "ddd.0". "'%g' % ddd.0" needs "ddd", but str(ddd.0) needs "ddd.0"
        internal bool TrailingZeroAfterWholeFloat = false;

        private StringBuilder buf;
        #endregion

        #region Constructors
        public StringFormatter(string str, object data) {
            this.str = str;
            this.data = data;
        }
        #endregion

        #region Public API Surface
        public string Format() {
            index = 0;
            buf = new StringBuilder(str.Length * 2);
            int modIndex;

            while ((modIndex = str.IndexOf('%', index)) != -1) {
                buf.Append(str, index, modIndex - index);
                index = modIndex + 1;
                DoFormatCode();
            }
            buf.Append(str, index, str.Length - index);

            CheckDataUsed();

            return buf.ToString();
        }
        #endregion

        #region Private APIs

        private void DoFormatCode() {
            // we already pulled the first %
            curCh = str[index++];

            if (curCh == '%') {
                // Escaped '%' character using "%%". Just print it and we are done
                buf.Append('%');
                return;
            }

            string key = ReadMappingKey();

            opts = new FormatSettings();

            ReadConversionFlags();

            ReadMinimumFieldWidth();

            ReadPrecision();

            ReadLengthModifier();

            // use the key (or lack thereof) to get the value
            object value;
            if (key == null) {
                value = GetData(dataIndex++);
            } else {
                value = GetKey(key);
            }
            opts.Value = value;

            WriteConversion();
        }

        private string ReadMappingKey() {
            string key = null;
            if (curCh == '(') {
                int start = index;
                int end = str.IndexOf(')', start);
                key = str.Substring(start, end - start);
                index = end + 1;
                curCh = str[index++];
            }
            return key;
        }

        private void ReadConversionFlags() {
            bool fFoundConversion;
            do {
                fFoundConversion = true;
                switch (curCh) {
                    case '#': opts.AltForm = true; break;
                    case '-': opts.LeftAdj = true; opts.ZeroPad = false; break;
                    case '0': if (!opts.LeftAdj) opts.ZeroPad = true; break;
                    case '+': opts.SignChar = true; opts.Space = false; break;
                    case ' ': if (!opts.SignChar) opts.Space = true; break;
                    default: fFoundConversion = false; break;
                }
                if (fFoundConversion) curCh = str[index++];
            } while (fFoundConversion);
        }

        private int ReadNumberOrStar() {
            return ReadNumberOrStar(0);
        }
        private int ReadNumberOrStar(int noValSpecified) {
            int res = noValSpecified;
            if (curCh == '*') {
                if (!(data is Tuple)) { throw Ops.TypeError("* requires a tuple for values"); }
                curCh = str[index++];
                res = Converter.ConvertToInt32(GetData(dataIndex++));
            } else {
                if (Char.IsDigit(curCh)) {
                    res = 0;
                    while (Char.IsDigit(curCh) && index < this.str.Length) {
                        res = res * 10 + ((int)(curCh - '0'));
                        curCh = str[index++];
                    }
                }
            }
            return res;
        }

        private void ReadMinimumFieldWidth() {
            opts.FieldWidth = ReadNumberOrStar();
            if (opts.FieldWidth == Int32.MaxValue) throw Ops.MemoryError("not enough memory for field width");
        }

        private void ReadPrecision() {
            if (curCh == '.') {
                curCh = str[index++];
                // possibility: "8.f", "8.0f", or "8.2f"
                opts.Precision = ReadNumberOrStar();
                if (opts.Precision > 116) throw Ops.OverflowError("formatted integer is too long (precision too large?)");
            } else {
                opts.Precision = UnspecifiedPrecision;
            }
        }

        private void ReadLengthModifier() {
            switch (curCh) {
                // ignored, not necessary for Python
                case 'h':
                case 'l':
                case 'L':
                    curCh = str[index++];
                    break;
            }
        }

        private void WriteConversion() {
            // conversion type (required)
            switch (curCh) {
                // signed integer decimal
                case 'd':
                case 'i': AppendInt(); return;
                // unsigned octal
                case 'o': AppendOctal(); return;
                // unsigned decimal
                case 'u': AppendInt(); return;
                // unsigned hexidecimal
                case 'x': AppendHex(curCh); return;
                case 'X': AppendHex(curCh); return;
                // floating point exponential format 
                case 'e':
                case 'E':
                // floating point decimal
                case 'f':
                case 'F': AppendFloat(curCh); return;
                // Same as "e" if exponent is less than -4 or more than precision, "f" otherwise.
                case 'G':
                case 'g': AppendFloat(curCh); return;
                // single character (int or single char str)
                case 'c': AppendChar(); return;
                // string (repr() version)
                case 'r': AppendRepr(); return;
                // string (str() version)
                case 's': AppendString(); return;
                default:
                    if (curCh > 0xff)
                        throw Ops.ValueError("unsupported format character '{0}' (0x{1:X}) at index {2}", '?', (int)curCh, index - 1);
                    else
                        throw Ops.ValueError("unsupported format character '{0}' (0x{1:X}) at index {2}", curCh, (int)curCh, index - 1);
            }
        }

        private object GetData(int index) {
            Tuple dt = data as Tuple;
            if (dt != null) {
                if (index < dt.Count) {
                    return dt[index];
                }
            } else {
                if (index == 0) {
                    return data;
                }
            }

            throw Ops.TypeError("not enough arguments for format string");
        }

        private object GetKey(string key) {
            IMapping map = data as IMapping;
            if (map == null) throw Ops.TypeError("format requires a mapping");

            object res = map.GetValue(key, this);
            if (res != this) return res;

            throw Ops.KeyError(key);
        }

        private object GetIntegerValue(out bool fPos) {
            object val;
            Conversion conv;

            int intVal = Converter.TryConvertToInt32(opts.Value, out conv);

            if (conv > Conversion.Implicit) {
                BigInteger bigInt = Converter.TryConvertToBigInteger(opts.Value, out conv);
                if (conv == Conversion.None) throw Ops.TypeError("int argument required");

                val = bigInt;
                fPos = bigInt >= 0;
            } else {
                val = intVal;
                fPos = intVal >= 0;
            }

            return val;
        }

        private void AppendChar() {
            char val = Converter.ConvertToChar(opts.Value);
            if (opts.FieldWidth > 1) {
                if (!opts.LeftAdj) {
                    buf.Append(' ', opts.FieldWidth - 1);
                }
                buf.Append(val);
                if (opts.LeftAdj) {
                    buf.Append(' ', opts.FieldWidth - 1);
                }
            } else {
                buf.Append(val);
            }
        }

        private void CheckDataUsed() {
            if (!(data is IMapping)) {
                if ((!(data is Tuple) && dataIndex != 1) ||
                    (data is Tuple && dataIndex != ((Tuple)data).Count)) {
                    throw Ops.TypeError("not all arguments converted during string formatting");
                }
            }
        }

        private void AppendInt() {
            bool fPos;
            object val = GetIntegerValue(out fPos);

            if (opts.LeftAdj) {
                AppendLeftAdj(val, fPos, 'D');
            } else if (opts.ZeroPad) {
                AppendZeroPad(val, fPos, 'D');
            } else {
                AppendNumeric(val, fPos, 'D');
            }
        }

        private static readonly char[] zero = new char[] { '0' };

        // Return the new type char to use
        // opts.Precision will be set to the nubmer of digits to display after the decimal point
        private char AdjustForG(char type, double v) {
            if (type != 'G' && type != 'g')
                return type;
            if (Double.IsNaN(v) || Double.IsInfinity(v))
                return type;

            double absV = Math.Abs(v);

            if ((v != 0.0) && // 0.0 should not be displayed as scientific notation
                absV < 1e-4 || // Values less than 0.0001 will need scientific notation
                absV >= Math.Pow(10, opts.Precision)) { // Values bigger than 1e<precision> will need scientific notation

                // One digit is displayed before the decimal point. Hence, we need one fewer than the precision after the decimal point
                int fractionDigitsRequired = (opts.Precision - 1);
                string expForm = absV.ToString("E" + fractionDigitsRequired);
                string mantissa = expForm.Substring(0, expForm.IndexOf('E')).TrimEnd(zero);

                // We do -2 to ignore the digit before the decimal point and the decimal point itself
                Debug.Assert(mantissa[1] == '.');
                opts.Precision = mantissa.Length - 2;

                type = (type == 'G') ? 'E' : 'e';
            } else {
                // "0.000ddddd" is allowed when the precision is 5. The 3 leading zeros are not counted
                int numberDecimalDigits = opts.Precision;
                if (absV < 1e-3) numberDecimalDigits += 3;
                else if (absV < 1e-2) numberDecimalDigits += 2;
                else if (absV < 1e-1) numberDecimalDigits += 1;

                string fixedPointForm = absV.ToString("F" + numberDecimalDigits).TrimEnd(zero);
                string fraction = fixedPointForm.Substring(fixedPointForm.IndexOf('.') + 1);
                if (absV < 1.0) {
                    opts.Precision = fraction.Length;
                } else {
                    int digitsBeforeDecimalPoint = 1 + (int)Math.Log10(absV);
                    opts.Precision = Math.Min(opts.Precision - digitsBeforeDecimalPoint, fraction.Length);
                }

                type = (type == 'G') ? 'F' : 'f';
            }

            return type;
        }

        private void AppendFloat(char type) {
            Conversion conv;
            double v = Converter.TryConvertToDouble(opts.Value, out conv);
            if (conv == Conversion.None) throw Ops.TypeError("float argument required");

            // scientific exponential format 
            Debug.Assert(type == 'E' || type == 'e' ||
                // floating point decimal
                         type == 'F' || type == 'f' ||
                // Same as "e" if exponent is less than -4 or more than precision, "f" otherwise.
                         type == 'G' || type == 'g');

            bool forceDot = false;
            // update our precision first...
            if (opts.Precision != UnspecifiedPrecision) {
                if (opts.Precision == 0 && opts.AltForm) forceDot = true;
                if (opts.Precision > 50)
                    opts.Precision = 50;
            } else {
                // alternate form (#) specified, set precision to zero...
                if (opts.AltForm) {
                    opts.Precision = 0;
                    forceDot = true;
                } else opts.Precision = 6;
            }

            type = AdjustForG(type, v);
            nfi.NumberDecimalDigits = opts.Precision;

            // then append
            if (opts.LeftAdj) {
                AppendLeftAdj(v, v >= 0, type);
            } else if (opts.ZeroPad) {
                AppendZeroPadFloat(v, type);
            } else {
                AppendNumeric(v, v >= 0, type);
            }
            if (v < 0 && v > -1 && buf[0] != '-') {
                FixupFloatMinus();
            }

            if (forceDot) {
                FixupAltFormDot();
            }
        }

        private void FixupAltFormDot() {
            buf.Append('.');
            if (opts.FieldWidth != 0) {
                // try and remove the extra character we're adding.
                for (int i = 0; i < buf.Length; i++) {
                    if (buf[i] == ' ' || buf[i] == '0') {
                        buf.Remove(i, 1);
                        break;
                    } else if (buf[i] != '-' && buf[i] != '+') {
                        break;
                    }
                }
            }
        }

        private void FixupFloatMinus() {
            // Python always appends a - even if precision is 0 and the value would appear to be zero.
            bool fNeedMinus = true;
            for (int i = 0; i < buf.Length; i++) {
                if (buf[i] != '.' && buf[i] != '0' && buf[i] != ' ') {
                    fNeedMinus = false;
                    break;
                }
            }

            if (fNeedMinus) {
                if (opts.FieldWidth != 0) {
                    // trim us back down to the correct field width...
                    if (buf[buf.Length - 1] == ' ') {
                        buf.Insert(0, '-');
                        buf.Remove(buf.Length - 1, 1);
                    } else {
                        int index = 0;
                        while (buf[index] == ' ') index++;
                        if (index > 0) index--;
                        buf[index] = '-';
                    }
                } else {
                    buf.Insert(0, '-');
                }
            }
        }

        private void AppendZeroPad(object val, bool fPos, char format) {
            if (fPos && (opts.SignChar || opts.Space)) {
                // produce [' '|'+']0000digits
                // first get 0 padded number to field width
                string res = String.Format(nfi, "{0:" + format + opts.FieldWidth.ToString() + "}", val);

                char signOrSpace = opts.SignChar ? '+' : ' ';
                // then if we ended up with a leading zero replace it, otherwise
                // append the space / + to the front.
                if (res[0] == '0' && res.Length > 1) {
                    res = signOrSpace + res.Substring(1);
                } else {
                    res = signOrSpace + res;
                }
                buf.Append(res);
            } else {
                string res = String.Format(nfi, "{0:" + format + opts.FieldWidth.ToString() + "}", val);

                // Difference: 
                //   System.String.Format("{0:D3}", -1)      '-001'
                //   "%03d" % -1                             '-01'

                if (res[0] == '-') {
                    // negative
                    buf.Append("-");
                    if (res[1] != '0') {
                        buf.Append(res.Substring(1));
                    } else {
                        buf.Append(res.Substring(2));
                    }
                } else {
                    // positive
                    buf.Append(res);
                }
            }
        }

        private void AppendZeroPadFloat(double val, char format) {
            if (val >= 0) {
                StringBuilder res = new StringBuilder(val.ToString(format.ToString(), nfi));
                if (res.Length < opts.FieldWidth) {
                    res.Insert(0, new string('0', opts.FieldWidth - res.Length));
                }
                if (opts.SignChar || opts.Space) {
                    char signOrSpace = opts.SignChar ? '+' : ' ';
                    // then if we ended up with a leading zero replace it, otherwise
                    // append the space / + to the front.
                    if (res[0] == '0' && res[1] != '.') {
                        res[0] = signOrSpace;
                    } else {
                        res.Insert(0, signOrSpace);
                    }
                }
                buf.Append(res);
            } else {
                StringBuilder res = new StringBuilder(val.ToString(format.ToString(), nfi));
                if (res.Length < opts.FieldWidth) {
                    res.Insert(1, new string('0', opts.FieldWidth - res.Length));
                }
                buf.Append(res);
            }
        }

        private void AppendNumeric(object val, bool fPos, char format) {
            if (fPos && (opts.SignChar || opts.Space)) {
                string strval = (opts.SignChar ? "+" : " ") + String.Format(nfi, "{0:" + format.ToString() + "}", val);
                if (strval.Length < opts.FieldWidth) {
                    buf.Append(' ', opts.FieldWidth - strval.Length);
                }
                buf.Append(strval);
            } else if (opts.Precision == UnspecifiedPrecision) {
                buf.AppendFormat(nfi, "{0," + opts.FieldWidth.ToString() + ":" + format + "}", val);
            } else if (opts.Precision < 100) {
                //CLR formatting has a maximum precision of 100.
                buf.AppendFormat(nfi, "{0," + opts.FieldWidth.ToString() + ":" + format + opts.Precision.ToString() + "}", val);
            } else {
                StringBuilder res = new StringBuilder();
                res.AppendFormat("{0:" + format + "}", val);
                if (res.Length < opts.Precision) {
                    res.Insert(0, new String('0', opts.Precision - res.Length));
                }
                if (res.Length < opts.FieldWidth) {
                    res.Insert(0, new String(' ', opts.FieldWidth - res.Length));
                }
                buf.Append(res.ToString());
            }

            // If AdjustForG() sets opts.Precision == 0, it means that no significant digits should be displayed after
            // the decimal point. ie. 123.4 should be displayed as "123", not "123.4". However, we might still need a 
            // decorative ".0". ie. to display "123.0"
            if (TrailingZeroAfterWholeFloat && (format == 'f' || format == 'F') && opts.Precision == 0)
                buf.Append(".0");
        }

        private void AppendLeftAdj(object val, bool fPos, char type) {
            string str = String.Format(nfi, "{0:" + type.ToString() + "}", val);
            if (fPos) {
                if (opts.SignChar) str = '+' + str;
                else if (opts.Space) str = ' ' + str;
            }
            buf.Append(str);
            if (str.Length < opts.FieldWidth) buf.Append(' ', opts.FieldWidth - str.Length);
        }

        private static bool NeedsAltForm(char format, char last) {
            if (format == 'X' || format == 'x') return true;

            if (last == '0') return false;
            return true;
        }

        private static string GetAltFormPrefixForRadix(char format, int radix) {
            switch (radix) {
                case 8: return "0";
                case 16: return format + "0";
            }
            return "";
        }

        /// <summary>
        /// AppendBase appends an integer at the specified radix doing all the
        /// special forms for Python.  We have a copy & paste version of this
        /// for BigInteger below that should be kept in sync.
        /// </summary>
        private void AppendBase(char format, int radix) {
            bool fPos;
            object intVal = GetIntegerValue(out fPos);
            if (intVal is BigInteger) {
                AppendBaseBigInt(intVal as BigInteger, format, radix);
                return;
            }
            int origVal = (int)intVal;
            int val = origVal;
            if (val < 0) {
                val *= -1;

                // if negative number, the leading space has no impact
                opts.Space = false;
            }
            // we build up the number backwards inside a string builder,
            // and after we've finished building this up we append the
            // string to our output buffer backwards.

            // convert value to string 
            StringBuilder str = new StringBuilder();
            if (val == 0) str.Append('0');
            while (val != 0) {
                int digit = val % radix;
                if (digit < 10) str.Append((char)((digit) + '0'));
                else if (Char.IsLower(format)) str.Append((char)((digit - 10) + 'a'));
                else str.Append((char)((digit - 10) + 'A'));

                val /= radix;
            }

            // pad out for additional precision
            if (str.Length < opts.Precision) {
                int len = opts.Precision - str.Length;
                str.Append('0', len);
            }

            // pad result to minimum field width
            if (opts.FieldWidth != 0) {
                int signLen = (origVal < 0 || opts.SignChar) ? 1 : 0;
                int spaceLen = opts.Space ? 1 : 0;
                int len = opts.FieldWidth - (str.Length + signLen + spaceLen);

                if (len > 0) {
                    // we account for the size of the alternate form, if we'll end up adding it.
                    if (opts.AltForm && NeedsAltForm(format, (!opts.LeftAdj && opts.ZeroPad) ? '0' : str[str.Length - 1])) {
                        len -= GetAltFormPrefixForRadix(format, radix).Length;
                    }

                    if (len > 0) {
                        // and finally append the right form
                        if (opts.LeftAdj) {
                            str.Insert(0, " ", len);
                        } else {
                            if (opts.ZeroPad) {
                                str.Append('0', len);
                            } else {
                                buf.Append(' ', len);
                            }
                        }
                    }
                }
            }

            // append the alternate form
            if (opts.AltForm && NeedsAltForm(format, str[str.Length - 1]))
                str.Append(GetAltFormPrefixForRadix(format, radix));


            // add any sign if necessary
            if (origVal < 0) {
                buf.Append('-');
            } else if (opts.SignChar) {
                buf.Append('+');
            } else if (opts.Space) {
                buf.Append(' ');
            }

            // append the final value
            for (int i = str.Length - 1; i >= 0; i--) {
                buf.Append(str[i]);
            }
        }

        /// <summary>
        /// BigInteger version of AppendBase.  Should be kept in sync w/ AppendBase
        /// </summary>
        private void AppendBaseBigInt(BigInteger origVal, char format, int radix) {
            BigInteger val = origVal;
            if (val < 0) val *= -1;

            // convert value to octal
            StringBuilder str = new StringBuilder();
            if (val == 0) str.Append('0');
            while (val != 0) {
                int digit = (int)(val % radix);
                if (digit < 10) str.Append((char)((digit) + '0'));
                else if (Char.IsLower(format)) str.Append((char)((digit - 10) + 'a'));
                else str.Append((char)((digit - 10) + 'A'));

                val /= radix;
            }

            // pad out for additional precision
            if (str.Length < opts.Precision) {
                int len = opts.Precision - str.Length;
                str.Append('0', len);
            }

            // pad result to minimum field width
            if (opts.FieldWidth != 0) {
                int signLen = (origVal < 0 || opts.SignChar) ? 1 : 0;
                int len = opts.FieldWidth - (str.Length + signLen);
                if (len > 0) {
                    // we account for the size of the alternate form, if we'll end up adding it.
                    if (opts.AltForm && NeedsAltForm(format, (!opts.LeftAdj && opts.ZeroPad) ? '0' : str[str.Length - 1])) {
                        len -= GetAltFormPrefixForRadix(format, radix).Length;
                    }

                    if (len > 0) {
                        // and finally append the right form
                        if (opts.LeftAdj) {
                            str.Insert(0, " ", len);
                        } else {
                            if (opts.ZeroPad) {
                                str.Append('0', len);
                            } else {
                                buf.Append(' ', len);
                            }
                        }
                    }
                }
            }

            // append the alternate form
            if (opts.AltForm && NeedsAltForm(format, str[str.Length - 1]))
                str.Append(GetAltFormPrefixForRadix(format, radix));


            // add any sign if necessary
            if (origVal < 0) {
                buf.Append('-');
            } else if (opts.SignChar) {
                buf.Append('+');
            } else if (opts.Space) {
                buf.Append(' ');
            }

            // append the final value
            for (int i = str.Length - 1; i >= 0; i--) {
                buf.Append(str[i]);
            }
        }

        private void AppendHex(char format) {
            AppendBase(format, 16);
        }

        private void AppendOctal() {
            AppendBase('o', 8);
        }

        private void AppendString() {
            string s = Ops.ToString(opts.Value);
            if (s == null) s = "None";
            AppendString(s);
        }

        private void AppendRepr() {
            AppendString(Ops.StringRepr(opts.Value));
        }

        private void AppendString(string s) {
            if (opts.Precision != UnspecifiedPrecision && s.Length > opts.Precision) s = s.Substring(0, opts.Precision);
            if (!opts.LeftAdj && opts.FieldWidth > s.Length) {
                buf.Append(' ', opts.FieldWidth - s.Length);
            }
            buf.Append(s);
            if (opts.LeftAdj && opts.FieldWidth > s.Length) {
                buf.Append(' ', opts.FieldWidth - s.Length);
            }
        }

        #endregion

        #region Private data structures

        // The conversion specifier format is as follows:
        //   % (mappingKey) conversionFlags fieldWidth . precision lengthModifier conversionType
        // where:
        //   mappingKey - value to be formatted
        //   conversionFlags - # 0 - + <space>
        //   lengthModifier - h, l, and L. Ignored by Python
        //   conversionType - d i o u x X e E f F g G c r s %
        // Ex:
        //   %(varName)#4o - Display "varName" as octal and prepend with leading 0 if necessary, for a total of atleast 4 characters

        [Flags]
        internal enum FormatOptions {
            ZeroPad = 0x01, // Use zero-padding to fit FieldWidth
            LeftAdj = 0x02, // Use left-adjustment to fit FieldWidth. Overrides ZeroPad
            AltForm = 0x04, // Add a leading 0 if necessary for octal, or add a leading 0x or 0X for hex
            Space = 0x08, // Leave a white-space
            SignChar = 0x10 // Force usage of a sign char even if the value is positive
        }

        internal struct FormatSettings {

            #region FormatOptions property accessors

            public bool ZeroPad {
                get {
                    return ((Options & FormatOptions.ZeroPad) != 0);
                }
                set {
                    if (value) {
                        Options |= FormatOptions.ZeroPad;
                    } else {
                        Options &= (~FormatOptions.ZeroPad);
                    }
                }
            }
            public bool LeftAdj {
                get {
                    return ((Options & FormatOptions.LeftAdj) != 0);
                }
                set {
                    if (value) {
                        Options |= FormatOptions.LeftAdj;
                    } else {
                        Options &= (~FormatOptions.LeftAdj);
                    }
                }
            }
            public bool AltForm {
                get {
                    return ((Options & FormatOptions.AltForm) != 0);
                }
                set {
                    if (value) {
                        Options |= FormatOptions.AltForm;
                    } else {
                        Options &= (~FormatOptions.AltForm);
                    }
                }
            }
            public bool Space {
                get {
                    return ((Options & FormatOptions.Space) != 0);
                }
                set {
                    if (value) {
                        Options |= FormatOptions.Space;
                    } else {
                        Options &= (~FormatOptions.Space);
                    }
                }
            }
            public bool SignChar {
                get {
                    return ((Options & FormatOptions.SignChar) != 0);
                }
                set {
                    if (value) {
                        Options |= FormatOptions.SignChar;
                    } else {
                        Options &= (~FormatOptions.SignChar);
                    }
                }
            }
            #endregion

            internal FormatOptions Options;

            // Minimum number of characters that the entire formatted string should occupy.
            // Smaller results will be left-padded with white-space or zeros depending on Options
            internal int FieldWidth;

            // Number of significant digits to display, before and after the decimal point.
            // For floats, it gets adjusted to the number of digits to display after the decimal point since
            // that is the value required by StringBuilder.AppendFormat.
            // For clarity, we should break this up into the two values - the precision specified by the
            // format string, and the value to be passed in to StringBuilder.AppendFormat
            internal int Precision;

            internal object Value;
        }
        #endregion
    }
}
