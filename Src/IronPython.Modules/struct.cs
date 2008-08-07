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
using System.Collections.Generic;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Math;

[assembly: PythonModule("struct", typeof(IronPython.Modules.PythonStruct))]
namespace IronPython.Modules {
    public static class PythonStruct {

        #region Public API Surface

        public static string pack(string fmt, params object[] values) {
            int count = 1;
            int curObj = 0;
            StringBuilder res = new StringBuilder();
            bool fLittleEndian = BitConverter.IsLittleEndian;
            bool fStandardized = false;

            for (int i = 0; i < fmt.Length; i++) {
                if (!fStandardized) {
                    // In native mode, align to {size}-byte boundaries
                    int nativeSize = GetNativeSize(fmt[i]);
                    if (nativeSize > 0) {
                        int alignLength = Align(res.Length, nativeSize);
                        int padLength = alignLength - res.Length;
                        for (int j = 0; j < padLength; j++) {
                            res.Append('\0');
                        }
                    }
                }
                switch (fmt[i]) {
                    case 'x': // pad byte
                        for (int j = 0; j < count; j++) {
                            res.Append('\0');
                        }
                        count = 1;
                        break;
                    case 'c': // char
                        for (int j = 0; j < count; j++) res.Append(GetCharValue(curObj++, values));
                        count = 1;
                        break;
                    case 'b': // signed char
                        for (int j = 0; j < count; j++) res.Append((char)(byte)GetSByteValue(curObj++, values));
                        count = 1;
                        break;
                    case 'B': // unsigned char
                        for (int j = 0; j < count; j++) res.Append((char)GetByteValue(curObj++, values));
                        count = 1;
                        break;
                    case 'h': // short
                        for (int j = 0; j < count; j++) WriteShort(res, fLittleEndian, GetShortValue(curObj++, values));
                        count = 1;
                        break;
                    case 'H': // unsigned short
                        for (int j = 0; j < count; j++) WriteUShort(res, fLittleEndian, GetUShortValue(curObj++, values));
                        count = 1;
                        break;
                    case 'i': // int
                    case 'l': // long
                        for (int j = 0; j < count; j++) WriteInt(res, fLittleEndian, GetIntValue(curObj++, values));
                        count = 1;
                        break;
                    case 'I': // unsigned int
                    case 'L': // unsigned long
                        for (int j = 0; j < count; j++) WriteUInt(res, fLittleEndian, GetUIntValue(curObj++, values));
                        count = 1;
                        break;
                    case 'q': // long long
                        for (int j = 0; j < count; j++) WriteLong(res, fLittleEndian, GetLongValue(curObj++, values));
                        count = 1;
                        break;
                    case 'Q': // unsigned long long
                        for (int j = 0; j < count; j++) WriteULong(res, fLittleEndian, GetULongValue(curObj++, values));
                        count = 1;
                        break;
                    case 'f': // float                        
                        for (int j = 0; j < count; j++) WriteFloat(res, fLittleEndian, (float)GetDoubleValue(curObj++, values));
                        count = 1;
                        break;
                    case 'd': // double
                        for (int j = 0; j < count; j++) WriteDouble(res, fLittleEndian, GetDoubleValue(curObj++, values));
                        count = 1;
                        break;
                    case 's': // char[]
                        WriteString(res, count, GetStringValue(curObj++, values));
                        count = 1;
                        break;
                    case 'p': // char[]
                        WritePascalString(res, count - 1, GetStringValue(curObj++, values));
                        count = 1;
                        break;
                    case 'P': // void *
                        if (IntPtr.Size == 4) goto case 'I';
                        goto case 'Q';
                    case ' ':   // white space, ignore
                    case '\t':
                        break;
                    case '=': // native
                        if (i != 0) throw Error("unexpected byte order");
                        fStandardized = true;
                        break;
                    case '@': // native
                        if (i != 0) throw Error("unexpected byte order");
                        break;
                    case '<': // little endian
                        if (i != 0) throw Error("unexpected byte order");
                        fLittleEndian = true;
                        fStandardized = true;
                        break;
                    case '>': // big endian
                    case '!': // big endian
                        if (i != 0) throw Error("unexpected byte order");
                        fLittleEndian = false;
                        fStandardized = true;
                        break;
                    default:
                        if (Char.IsDigit(fmt[i])) {
                            count = 0;
                            while (Char.IsDigit(fmt[i])) {
                                count = count * 10 + (fmt[i] - '0');
                                i++;
                            }
                            if (Char.IsWhiteSpace(fmt[i])) Error("white space not allowed between count and format");
                            i--;
                            break;
                        }

                        throw Error("bad format string");
                }
            }

            if (curObj != values.Length) throw Error("not all arguments used");

            return res.ToString();
        }

        public static PythonTuple unpack(string fmt, string @string) {
            string data = @string;
            int count = 1;
            int curIndex = 0;
            List<object> res = new List<object>();
            bool fLittleEndian = BitConverter.IsLittleEndian;
            bool fStandardized = false;

            for (int i = 0; i < fmt.Length; i++) {
                if (!fStandardized) {
                    // In native mode, align to {size}-byte boundaries
                    int nativeSize = GetNativeSize(fmt[i]);
                    if (nativeSize > 0) {
                        curIndex = Align(curIndex, nativeSize);
                    }
                }
                switch (fmt[i]) {
                    case 'x': // pad byte
                        curIndex += count;
                        count = 1;
                        break;
                    case 'c': // char
                        for (int j = 0; j < count; j++) res.Add(CreateCharValue(ref curIndex, data).ToString());
                        count = 1;
                        break;
                    case 'b': // signed char
                        for (int j = 0; j < count; j++) res.Add((int)(sbyte)CreateCharValue(ref curIndex, data));
                        count = 1;
                        break;
                    case 'B': // unsigned char
                        for (int j = 0; j < count; j++) res.Add((int)CreateCharValue(ref curIndex, data));
                        count = 1;
                        break;
                    case 'h': // short
                        for (int j = 0; j < count; j++) res.Add((int)CreateShortValue(ref curIndex, fLittleEndian, data));
                        count = 1;
                        break;
                    case 'H': // unsigned short
                        for (int j = 0; j < count; j++) res.Add((int)CreateUShortValue(ref curIndex, fLittleEndian, data));
                        count = 1;
                        break;
                    case 'i': // int
                    case 'l': // long
                        for (int j = 0; j < count; j++) res.Add(CreateIntValue(ref curIndex, fLittleEndian, data));
                        count = 1;
                        break;
                    case 'I': // unsigned int
                    case 'L': // unsigned long
                        for (int j = 0; j < count; j++) {
                            res.Add(BigIntegerOps.__int__(BigInteger.Create(CreateUIntValue(ref curIndex, fLittleEndian, data))));
                        }
                        count = 1;
                        break;
                    case 'q': // long long
                        for (int j = 0; j < count; j++) {
                            res.Add(BigIntegerOps.__int__(BigInteger.Create(CreateLongValue(ref curIndex, fLittleEndian, data))));
                        }
                        count = 1;
                        break;
                    case 'Q': // unsigned long long
                        for (int j = 0; j < count; j++) {
                            res.Add(BigIntegerOps.__int__(BigInteger.Create(CreateULongValue(ref curIndex, fLittleEndian, data))));
                        }
                        count = 1;
                        break;
                    case 'f': // float                        
                        for (int j = 0; j < count; j++) res.Add((double)CreateFloatValue(ref curIndex, fLittleEndian, data));
                        count = 1;
                        break;
                    case 'd': // double
                        for (int j = 0; j < count; j++) res.Add(CreateDoubleValue(ref curIndex, fLittleEndian, data));
                        count = 1;
                        break;
                    case 's': // char[]
                        res.Add(CreateString(ref curIndex, count, data));
                        count = 1;
                        break;
                    case 'p': // char[]
                        res.Add(CreatePascalString(ref curIndex, count - 1, data));
                        count = 1;
                        break;
                    case 'P': // void *
                        if (IntPtr.Size == 4) goto case 'I';
                        goto case 'Q';
                    case ' ':   // white space, ignore
                    case '\t':
                        break;
                    case '=': // native
                        if (i != 0) Error("unexpected byte order");
                        fStandardized = true;
                        break;
                    case '@': // native
                        if (i != 0) Error("unexpected byte order");
                        break;
                    case '<': // little endian
                        if (i != 0) Error("unexpected byte order");
                        fLittleEndian = true;
                        fStandardized = true;
                        break;
                    case '>': // big endian
                    case '!': // big endian
                        if (i != 0) Error("unexpected byte order");
                        fLittleEndian = false;
                        fStandardized = true;
                        break;
                    default:
                        if (Char.IsDigit(fmt[i])) {
                            count = 0;
                            while (Char.IsDigit(fmt[i])) {
                                count = count * 10 + (fmt[i] - '0');
                                i++;
                            }
                            if (Char.IsWhiteSpace(fmt[i])) Error("white space not allowed between count and format");
                            i--;
                            break;
                        }

                        throw Error("bad format string");
                }
            }

            if (curIndex != data.Length) throw Error("not all data used");

            return new PythonTuple(res);
        }

        private static int Align(int length, int size) {
            return length + (size - 1) & ~(size - 1);
        }

        internal static int GetNativeSize(char c) {
            switch (c) {
                case 'c': // char
                case 'b': // signed byte
                case 'B': // unsigned byte
                case 'x': // pad byte
                case 's': // null-terminated string
                case 'p': // Pascal string
                case 'u': // unicode char (used by array module; TODO: fix)
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

        public static int calcsize(string fmt) {
            int len = 0;
            int count = 1;
            bool fStandardized = false;
            for (int i = 0; i < fmt.Length; i++) {
                int nativeSize = GetNativeSize(fmt[i]);
                if (nativeSize > 0) {
                    if (!fStandardized) {
                        len = Align(len, nativeSize); // In native mode, align to {size}-byte boundaries
                    }
                    len += (nativeSize * count);
                    count = 1;
                } else {
                    switch (fmt[i]) {
                        case ' ':
                        case '\t':
                            break;
                        case '@': // native
                            if (i != 0) PythonExceptions.CreateThrowable(error, "unexpected byte order");
                            break;
                        case '=': // native
                        case '<': // little endian
                        case '>': // big endian
                        case '!': // big endian
                            if (i != 0) PythonExceptions.CreateThrowable(error, "unexpected byte order");
                            fStandardized = true;
                            break;
                        default:
                            if (Char.IsDigit(fmt[i])) {
                                count = 0;
                                while (Char.IsDigit(fmt[i])) {
                                    count = count * 10 + (fmt[i] - '0');
                                    i++;
                                }
                                i--;
                                break;
                            }

                            throw Error("bad format string");
                    }
                }
            }
            return len;
        }

        public static PythonType error = PythonExceptions.CreateSubType(PythonExceptions.Exception, "error", "struct", "");

        #endregion

        #region Write Helpers

        private static void WriteShort(StringBuilder res, bool fLittleEndian, short val) {
            if (fLittleEndian) {
                res.Append((char)(val & 0xff));
                res.Append((char)((val >> 8) & 0xff));
            } else {
                res.Append((char)((val >> 8) & 0xff));
                res.Append((char)(val & 0xff));
            }
        }

        private static void WriteUShort(StringBuilder res, bool fLittleEndian, ushort val) {
            if (fLittleEndian) {
                res.Append((char)(val & 0xff));
                res.Append((char)((val >> 8) & 0xff));
            } else {
                res.Append((char)((val >> 8) & 0xff));
                res.Append((char)(val & 0xff));
            }
        }

        private static void WriteInt(StringBuilder res, bool fLittleEndian, int val) {
            if (fLittleEndian) {
                res.Append((char)(val & 0xff));
                res.Append((char)((val >> 8) & 0xff));
                res.Append((char)((val >> 16) & 0xff));
                res.Append((char)((val >> 24) & 0xff));
            } else {
                res.Append((char)((val >> 24) & 0xff));
                res.Append((char)((val >> 16) & 0xff));
                res.Append((char)((val >> 8) & 0xff));
                res.Append((char)(val & 0xff));
            }
        }

        private static void WriteUInt(StringBuilder res, bool fLittleEndian, uint val) {
            if (fLittleEndian) {
                res.Append((char)(val & 0xff));
                res.Append((char)((val >> 8) & 0xff));
                res.Append((char)((val >> 16) & 0xff));
                res.Append((char)((val >> 24) & 0xff));
            } else {
                res.Append((char)((val >> 24) & 0xff));
                res.Append((char)((val >> 16) & 0xff));
                res.Append((char)((val >> 8) & 0xff));
                res.Append((char)(val & 0xff));
            }
        }

        private static void WriteFloat(StringBuilder res, bool fLittleEndian, float val) {
            byte[] bytes = BitConverter.GetBytes(val);
            if (fLittleEndian) {
                res.Append((char)bytes[0]);
                res.Append((char)bytes[1]);
                res.Append((char)bytes[2]);
                res.Append((char)bytes[3]);
            } else {
                res.Append((char)bytes[3]);
                res.Append((char)bytes[2]);
                res.Append((char)bytes[1]);
                res.Append((char)bytes[0]);
            }
        }

        private static void WriteLong(StringBuilder res, bool fLittleEndian, long val) {
            if (fLittleEndian) {
                res.Append((char)(val & 0xff));
                res.Append((char)((val >> 8) & 0xff));
                res.Append((char)((val >> 16) & 0xff));
                res.Append((char)((val >> 24) & 0xff));
                res.Append((char)((val >> 32) & 0xff));
                res.Append((char)((val >> 40) & 0xff));
                res.Append((char)((val >> 48) & 0xff));
                res.Append((char)((val >> 56) & 0xff));
            } else {
                res.Append((char)((val >> 56) & 0xff));
                res.Append((char)((val >> 48) & 0xff));
                res.Append((char)((val >> 40) & 0xff));
                res.Append((char)((val >> 32) & 0xff));
                res.Append((char)((val >> 24) & 0xff));
                res.Append((char)((val >> 16) & 0xff));
                res.Append((char)((val >> 8) & 0xff));
                res.Append((char)(val & 0xff));
            }
        }

        private static void WriteULong(StringBuilder res, bool fLittleEndian, ulong val) {
            if (fLittleEndian) {
                res.Append((char)(val & 0xff));
                res.Append((char)((val >> 8) & 0xff));
                res.Append((char)((val >> 16) & 0xff));
                res.Append((char)((val >> 24) & 0xff));
                res.Append((char)((val >> 32) & 0xff));
                res.Append((char)((val >> 40) & 0xff));
                res.Append((char)((val >> 48) & 0xff));
                res.Append((char)((val >> 56) & 0xff));
            } else {
                res.Append((char)((val >> 56) & 0xff));
                res.Append((char)((val >> 48) & 0xff));
                res.Append((char)((val >> 40) & 0xff));
                res.Append((char)((val >> 32) & 0xff));
                res.Append((char)((val >> 24) & 0xff));
                res.Append((char)((val >> 16) & 0xff));
                res.Append((char)((val >> 8) & 0xff));
                res.Append((char)(val & 0xff));
            }
        }

        private static void WriteDouble(StringBuilder res, bool fLittleEndian, double val) {
            byte[] bytes = BitConverter.GetBytes(val);
            if (fLittleEndian) {
                res.Append((char)bytes[0]);
                res.Append((char)bytes[1]);
                res.Append((char)bytes[2]);
                res.Append((char)bytes[3]);
                res.Append((char)bytes[4]);
                res.Append((char)bytes[5]);
                res.Append((char)bytes[6]);
                res.Append((char)bytes[7]);
            } else {
                res.Append((char)bytes[7]);
                res.Append((char)bytes[6]);
                res.Append((char)bytes[5]);
                res.Append((char)bytes[4]);
                res.Append((char)bytes[3]);
                res.Append((char)bytes[2]);
                res.Append((char)bytes[1]);
                res.Append((char)bytes[0]);
            }
        }

        private static void WriteString(StringBuilder res, int len, string val) {
            for (int i = 0; i < val.Length && i < len; i++) {
                res.Append(val[i]);
            }
            for (int i = val.Length; i < len; i++) {
                res.Append('\0');
            }
        }

        private static void WritePascalString(StringBuilder res, int len, string val) {
            int lenByte = Math.Min(255, Math.Min(val.Length, len));
            res.Append((char)lenByte);

            for (int i = 0; i < val.Length && i < len; i++) {
                res.Append(val[i]);
            }
            for (int i = val.Length; i < len; i++) {
                res.Append('\0');
            }
        }
        #endregion

        #region Data getter helpers

        internal static char GetCharValue(int index, object[] args) {
            string val = GetValue(index, args) as string;
            if (val == null || val.Length != 1) throw Error("char format requires string of length 1");

            return val[0];
        }

        internal static sbyte GetSByteValue(int index, object[] args) {
            object val = GetValue(index, args);
            sbyte res;
            if (Converter.TryConvertToSByte(val, out res)) {
                return res;
            }
            throw Error("expected sbyte value got " + val.ToString());
        }

        internal static byte GetByteValue(int index, object[] args) {
            object val = GetValue(index, args);

            byte res;
            if (Converter.TryConvertToByte(val, out res)) return res;

            char cres;
            if (Converter.TryConvertToChar(val, out cres)) return (byte)cres;

            throw Error("expected byte value got " + val.ToString());
        }

        internal static short GetShortValue(int index, object[] args) {
            object val = GetValue(index, args);
            short res;
            if (Converter.TryConvertToInt16(val, out res)) return res;
            throw Error("expected short value");
        }

        internal static ushort GetUShortValue(int index, object[] args) {
            object val = GetValue(index, args);
            ushort res;
            if (Converter.TryConvertToUInt16(val, out res)) return res;
            throw Error("expected ushort value");
        }

        internal static int GetIntValue(int index, object[] args) {
            object val = GetValue(index, args);
            int res;
            if (Converter.TryConvertToInt32(val, out res)) return res;
            throw Error("expected int value");
        }

        internal static uint GetUIntValue(int index, object[] args) {
            object val = GetValue(index, args);
            uint res;
            if (Converter.TryConvertToUInt32(val, out res)) return res;
            throw Error("expected uint value");
        }

        internal static long GetLongValue(int index, object[] args) {
            object val = GetValue(index, args);
            long res;
            if (Converter.TryConvertToInt64(val, out res)) return res;
            throw Error("expected long value");
        }

        internal static ulong GetULongValue(int index, object[] args) {
            object val = GetValue(index, args);
            ulong res;
            if (Converter.TryConvertToUInt64(val, out res)) return res;
            throw Error("expected ulong value");
        }

        internal static double GetDoubleValue(int index, object[] args) {
            object val = GetValue(index, args);
            double res;
            if (Converter.TryConvertToDouble(val, out res)) return res;
            throw Error("expected double value");
        }

        internal static string GetStringValue(int index, object[] args) {
            object val = GetValue(index, args);
            string res;
            if (Converter.TryConvertToString(val, out res)) return res;
            throw Error("expected string value");
        }

        internal static object GetValue(int index, object[] args) {
            if (index >= args.Length) throw Error("not enough arguments");
            return args[index];
        }
        #endregion

        #region Data creater helpers

        internal static char CreateCharValue(ref int index, string data) {
            return ReadData(ref index, data);
        }

        internal static short CreateShortValue(ref int index, bool fLittleEndian, string data) {
            byte b1 = (byte)ReadData(ref index, data);
            byte b2 = (byte)ReadData(ref index, data);

            if (fLittleEndian) {
                return (short)((b2 << 8) | b1);
            } else {
                return (short)((b1 << 8) | b2);
            }
        }

        internal static ushort CreateUShortValue(ref int index, bool fLittleEndian, string data) {
            byte b1 = (byte)ReadData(ref index, data);
            byte b2 = (byte)ReadData(ref index, data);

            if (fLittleEndian) {
                return (ushort)((b2 << 8) | b1);
            } else {
                return (ushort)((b1 << 8) | b2);
            }
        }

        internal static float CreateFloatValue(ref int index, bool fLittleEndian, string data) {
            byte[] bytes = new byte[4];
            if (fLittleEndian) {
                bytes[0] = (byte)ReadData(ref index, data);
                bytes[1] = (byte)ReadData(ref index, data);
                bytes[2] = (byte)ReadData(ref index, data);
                bytes[3] = (byte)ReadData(ref index, data);
            } else {
                bytes[3] = (byte)ReadData(ref index, data);
                bytes[2] = (byte)ReadData(ref index, data);
                bytes[1] = (byte)ReadData(ref index, data);
                bytes[0] = (byte)ReadData(ref index, data);
            }
            return BitConverter.ToSingle(bytes, 0);
        }

        internal static int CreateIntValue(ref int index, bool fLittleEndian, string data) {
            byte b1 = (byte)ReadData(ref index, data);
            byte b2 = (byte)ReadData(ref index, data);
            byte b3 = (byte)ReadData(ref index, data);
            byte b4 = (byte)ReadData(ref index, data);

            if (fLittleEndian)
                return (int)((b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
            else
                return (int)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        internal static uint CreateUIntValue(ref int index, bool fLittleEndian, string data) {
            byte b1 = (byte)ReadData(ref index, data);
            byte b2 = (byte)ReadData(ref index, data);
            byte b3 = (byte)ReadData(ref index, data);
            byte b4 = (byte)ReadData(ref index, data);

            if (fLittleEndian)
                return (uint)((b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
            else
                return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        internal static long CreateLongValue(ref int index, bool fLittleEndian, string data) {
            long b1 = (byte)ReadData(ref index, data);
            long b2 = (byte)ReadData(ref index, data);
            long b3 = (byte)ReadData(ref index, data);
            long b4 = (byte)ReadData(ref index, data);
            long b5 = (byte)ReadData(ref index, data);
            long b6 = (byte)ReadData(ref index, data);
            long b7 = (byte)ReadData(ref index, data);
            long b8 = (byte)ReadData(ref index, data);

            if (fLittleEndian)
                return (long)((b8 << 56) | (b7 << 48) | (b6 << 40) | (b5 << 32) |
                                (b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
            else
                return (long)((b1 << 56) | (b2 << 48) | (b3 << 40) | (b4 << 32) |
                                (b5 << 24) | (b6 << 16) | (b7 << 8) | b8);
        }

        internal static ulong CreateULongValue(ref int index, bool fLittleEndian, string data) {
            ulong b1 = (byte)ReadData(ref index, data);
            ulong b2 = (byte)ReadData(ref index, data);
            ulong b3 = (byte)ReadData(ref index, data);
            ulong b4 = (byte)ReadData(ref index, data);
            ulong b5 = (byte)ReadData(ref index, data);
            ulong b6 = (byte)ReadData(ref index, data);
            ulong b7 = (byte)ReadData(ref index, data);
            ulong b8 = (byte)ReadData(ref index, data);
            if (fLittleEndian)
                return (ulong)((b8 << 56) | (b7 << 48) | (b6 << 40) | (b5 << 32) |
                                (b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
            else
                return (ulong)((b1 << 56) | (b2 << 48) | (b3 << 40) | (b4 << 32) |
                                (b5 << 24) | (b6 << 16) | (b7 << 8) | b8);
        }

        internal static double CreateDoubleValue(ref int index, bool fLittleEndian, string data) {
            byte[] bytes = new byte[8];
            if (fLittleEndian) {
                bytes[0] = (byte)ReadData(ref index, data);
                bytes[1] = (byte)ReadData(ref index, data);
                bytes[2] = (byte)ReadData(ref index, data);
                bytes[3] = (byte)ReadData(ref index, data);
                bytes[4] = (byte)ReadData(ref index, data);
                bytes[5] = (byte)ReadData(ref index, data);
                bytes[6] = (byte)ReadData(ref index, data);
                bytes[7] = (byte)ReadData(ref index, data);
            } else {
                bytes[7] = (byte)ReadData(ref index, data);
                bytes[6] = (byte)ReadData(ref index, data);
                bytes[5] = (byte)ReadData(ref index, data);
                bytes[4] = (byte)ReadData(ref index, data);
                bytes[3] = (byte)ReadData(ref index, data);
                bytes[2] = (byte)ReadData(ref index, data);
                bytes[1] = (byte)ReadData(ref index, data);
                bytes[0] = (byte)ReadData(ref index, data);
            }
            return BitConverter.ToDouble(bytes, 0);
        }

        internal static string CreateString(ref int index, int count, string data) {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < count; i++) {
                res.Append(ReadData(ref index, data));
            }
            return res.ToString();
        }


        internal static string CreatePascalString(ref int index, int count, string data) {
            int realLen = (int)ReadData(ref index, data);
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < realLen; i++) {
                res.Append(ReadData(ref index, data));
            }
            for (int i = realLen; i < count; i++) {
                // throw away null bytes
                ReadData(ref index, data);
            }
            return res.ToString();
        }

        private static char ReadData(ref int index, string data) {
            if (index >= data.Length) throw Error("not enough data while reading");

            return data[index++];
        }
        #endregion

        #region Misc. Private APIs

        private static Exception Error(string msg) {
            return PythonExceptions.CreateThrowable(error, msg);
        }

        #endregion
    }
}
