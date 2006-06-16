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
using System.Collections.Generic;
using System.Text;
using System.IO;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronMath;

[assembly: PythonModule("struct", typeof(IronPython.Modules.PythonStruct))]
namespace IronPython.Modules {
    [PythonType("struct")]
    public static class PythonStruct {

        #region Public API Surface 
        [PythonName("pack")]
        public static object Pack(string fmt, params object [] values) {
            int count = 1;
            int curObj = 0;
            StringBuilder res = new StringBuilder();
            bool fLittleEndian = BitConverter.IsLittleEndian;
            bool fStandardized = false;

            for (int i = 0; i < fmt.Length; i++) {
                switch (fmt[i]) {
                    case 'x': // pad byte
                        res.Append('\0');
                        break;
                    case 'c': // char
                        for(int j = 0; j<count; j++) res.Append(GetCharValue(curObj++, values));
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
                        WritePascalString(res, count-1, GetStringValue(curObj++, values));
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

            if (curObj != values.Length) throw Error("not all arguments used");

            if(fStandardized)
            return res.ToString();
            return res.ToString();
        }

        [PythonName("unpack")]
        public static Tuple Unpack(string fmt, string @string) {
            string data = @string;
            int count = 1;
            int curIndex = 0;
            List<object> res = new List<object>();
            bool fLittleEndian = BitConverter.IsLittleEndian;
            bool fStandardized = false;


            for (int i = 0; i < fmt.Length; i++) {
                switch (fmt[i]) {
                    case 'x': // pad byte
                        curIndex++;
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
                        for (int j = 0; j < count; j++) res.Add(BigInteger.Create(CreateUIntValue(ref curIndex, fLittleEndian, data)));
                        count = 1;
                        break;
                    case 'q': // long long
                        for (int j = 0; j < count; j++) res.Add(BigInteger.Create(CreateLongValue(ref curIndex, fLittleEndian, data)));                        
                        count = 1;
                        break;
                    case 'Q': // unsigned long long
                        for (int j = 0; j < count; j++) res.Add(BigInteger.Create(CreateULongValue(ref curIndex, fLittleEndian, data)));
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
                        res.Add(CreatePascalString(ref curIndex, count-1, data));
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
            if (fStandardized)
                return new Tuple(res);
            return new Tuple(res);
        }

        [PythonName("calcsize")]
        public static int CalculateSize(string fmt) {
            int len = 0;
            int count = 1;
            for (int i = 0; i < fmt.Length; i++) {
                switch (fmt[i]) {
                    case 'x': // pad byte
                    case 'c': // char
                    case 'b': // signed char
                    case 'B': // unsigned char
                    case 's': // char[]
                        len += (count);
                        count = 1;
                        break;
                    case 'h': // short
                    case 'H': // unsigned short
                        len += 2 * count;
                        count = 1;
                        break;
                    case 'i': // int
                    case 'I': // unsigned int
                    case 'l': // long
                    case 'L': // unsigned long
                        len += 4 * count;
                        count = 1;
                        break;
                    case 'q': // long long
                    case 'Q': // unsigned long long
                        len += 8 * count;
                        count = 1;
                        break;
                    case 'f': // float
                        len += 4 * count;
                        count = 1;
                        break;
                    case 'd': // double
                        len += 8 * count;
                        count = 1;
                        break;
                    case 'p': // char[]
                        len += count + 1;
                        break;
                    case 'P': // void *
                        len += IntPtr.Size;
                        break;
                    case ' ':
                    case '\t':
                        break;
                    case '=': // native
                    case '@': // native
                    case '<': // little endian
                    case '>': // big endian
                    case '!': // big endian
                        if (i != 0) ExceptionConverter.CreateThrowable(error, "unexpected byte order");
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
            return len;
        }

        public static object error = ExceptionConverter.CreatePythonException("error", "struct");

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
                res.Append((char) (val & 0xff));
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
                res.Append((char) (val & 0xff));
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
            int lenByte= Math.Min(255, Math.Min(val.Length, len));
            res.Append((char)lenByte);

            for (int i = 0; i < val.Length && i <len; i++) {
                res.Append(val[i]);
            }
            for (int i = val.Length; i < len; i++) {
                res.Append('\0');
            }
        }
        #endregion

        #region Data getter helpers
        private static char GetCharValue(int index, object[] args) {
            object val = GetValue(index, args);
            Conversion conv;
            char res = Converter.TryConvertToChar(val, out conv);
            if (conv == Conversion.None) throw Error("expected character value");
            return res;
        }

        private static sbyte GetSByteValue(int index, object[] args) {
            object val = GetValue(index, args);
            Conversion conv;
            sbyte res = Converter.TryConvertToSByte(val, out conv);
            if (conv == Conversion.None) throw Error("expected sbyte value got " + val.ToString());
            return res;
        }

        private static byte GetByteValue(int index, object[] args) {
            object val = GetValue(index, args);
            Conversion conv;
            byte res = Converter.TryConvertToByte(val, out conv);
            if (conv == Conversion.None) {
                char cres = Converter.TryConvertToChar(val, out conv);

                if(conv == Conversion.None) throw Error("expected byte value got " + val.ToString());

                return (byte)cres;
            }
            return res;
        }

        private static short GetShortValue(int index, object[] args) {
            object val = GetValue(index, args);
            Conversion conv;
            short res = Converter.TryConvertToInt16(val, out conv);
            if (conv == Conversion.None) throw Error("expected short value");
            return res;
        }

        private static ushort GetUShortValue(int index, object[] args) {
            object val = GetValue(index, args);
            Conversion conv;
            ushort res = Converter.TryConvertToUInt16(val, out conv);
            if (conv == Conversion.None) throw Error("expected ushort value");
            return res;
        }

        private static int GetIntValue(int index, object[] args) {
            object val = GetValue(index, args);
            Conversion conv;
            int res = Converter.TryConvertToInt32(val, out conv);
            if (conv == Conversion.None) throw Error("expected int value");
            return res;
        }

        private static uint GetUIntValue(int index, object[] args) {
            object val = GetValue(index, args);
            Conversion conv;
            uint res = Converter.TryConvertToUInt32(val, out conv);
            if (conv == Conversion.None) throw Error("expected uint value");
            return res;
        }

        private static long GetLongValue(int index, object[] args) {
            object val = GetValue(index, args);
            Conversion conv;
            long res = Converter.TryConvertToInt64(val, out conv);
            if (conv == Conversion.None) throw Error("expected long value");
            return res;
        }

        private static ulong GetULongValue(int index, object[] args) {
            object val = GetValue(index, args);
            Conversion conv;
            ulong res = Converter.TryConvertToUInt64(val, out conv);
            if (conv == Conversion.None) throw Error("expected ulong value");
            return res;
        }

        private static double GetDoubleValue(int index, object[] args) {
            object val = GetValue(index, args);
            Conversion conv;
            double res = Converter.TryConvertToDouble(val, out conv);
            if (conv == Conversion.None) throw Error("expected double value");
            return res;
        }

        private static string GetStringValue(int index, object[] args) {
            object val = GetValue(index, args);
            Conversion conv;
            string res = Converter.TryConvertToString(val, out conv);
            if (conv == Conversion.None) throw Error("expected string value");
            return res;            
        }

        private static object GetValue(int index, object[] args) {
            if (index >= args.Length) throw Error("not enough arguments");
            return args[index];
        }
        #endregion

        #region Data creater helpers

        private static char CreateCharValue(ref int index, string data) {
            return ReadData(ref index, data);
        }

        private static short CreateShortValue(ref int index, bool fLittleEndian, string data) {
            byte b1 = (byte)ReadData(ref index, data);
            byte b2 = (byte)ReadData(ref index, data);

            if (fLittleEndian) {
                return (short)((b2 << 8) | b1);
            } else {
                return (short)((b1 << 8) | b2);
            }
        }

        private static ushort CreateUShortValue(ref int index, bool fLittleEndian, string data) {
            byte b1 = (byte)ReadData(ref index, data);
            byte b2 = (byte)ReadData(ref index, data);

            if (fLittleEndian) {
                return (ushort)((b2 << 8) | b1);
            } else {
                return (ushort)((b1 << 8) | b2);
            }
        }

        private static float CreateFloatValue(ref int index, bool fLittleEndian, string data) {
            byte [] bytes = new byte[4];
            if(fLittleEndian){
                bytes[0] = (byte)ReadData(ref index ,data);
                bytes[1] = (byte)ReadData(ref index ,data);
                bytes[2] = (byte)ReadData(ref index ,data);
                bytes[3] = (byte)ReadData(ref index ,data);
            }else{
                bytes[3] = (byte)ReadData(ref index ,data);
                bytes[2] = (byte)ReadData(ref index ,data);
                bytes[1] = (byte)ReadData(ref index ,data);
                bytes[0] = (byte)ReadData(ref index ,data);
            }
            return BitConverter.ToSingle(bytes, 0);
        }

        private static int CreateIntValue(ref int index, bool fLittleEndian, string data) {
            byte b1 = (byte)ReadData(ref index, data);
            byte b2 = (byte)ReadData(ref index, data);
            byte b3 = (byte)ReadData(ref index, data);
            byte b4 = (byte)ReadData(ref index, data);

            if(fLittleEndian)
                return (int) ((b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
            else
                return (int)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        private static uint CreateUIntValue(ref int index, bool fLittleEndian, string data) {
            byte b1 = (byte)ReadData(ref index, data);
            byte b2 = (byte)ReadData(ref index, data);
            byte b3 = (byte)ReadData(ref index, data);
            byte b4 = (byte)ReadData(ref index, data);

            if (fLittleEndian)
                return (uint)((b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
            else
                return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        private static long CreateLongValue(ref int index, bool fLittleEndian, string data) {
            long b1 = (byte)ReadData(ref index, data);
            long b2 = (byte)ReadData(ref index, data);
            long b3 = (byte)ReadData(ref index, data);
            long b4 = (byte)ReadData(ref index, data);
            long b5 = (byte)ReadData(ref index, data);
            long b6 = (byte)ReadData(ref index, data);
            long b7 = (byte)ReadData(ref index, data);
            long b8 = (byte)ReadData(ref index, data);
            
            if(fLittleEndian)
                return (long)((b8 << 56) | (b7 << 48) | (b6 << 40) | (b5 << 32) |
                                (b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
            else
                return (long)((b1 << 56) | (b2 << 48) | (b3 << 40) | (b4 << 32) |
                                (b5 << 24) | (b6 << 16) | (b7 << 8) | b8);
        }

        private static ulong CreateULongValue(ref int index, bool fLittleEndian, string data) {
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

        private static double CreateDoubleValue(ref int index, bool fLittleEndian, string data) {
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

        private static string CreateString(ref int index, int count, string data) {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < count; i++) {
                res.Append(ReadData(ref index, data));
            }
            return res.ToString();
        }


        private static string CreatePascalString(ref int index, int count, string data) {
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
            return ExceptionConverter.CreateThrowable(error, msg);
        }

        #endregion
    }
}
