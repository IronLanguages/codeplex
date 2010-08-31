/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace ComTest {

    [
    ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIDispatch),
    Guid("00020400-0000-0000-C000-000000000046")
    ]
    internal interface IDispatchForReflection {
    }


    public enum E1 : byte {
        a, b, c
    }


    public struct ConvertibleToDoubleStruct {
        public ConvertibleToDoubleStruct(double val) {
            value = val;
        }
        private readonly double value;
        public static implicit operator double(ConvertibleToDoubleStruct arg) {
            return arg.value;
        }
    }

    public struct ConvertibleToManyStruct {
        public ConvertibleToManyStruct(double val) {
            value = val;
        }
        private readonly double value;
        public static implicit operator double(ConvertibleToManyStruct arg) {
            return arg.value;
        }

        public static implicit operator int(ConvertibleToManyStruct arg) {
            return 1234;
        }

        public static implicit operator float(ConvertibleToManyStruct arg) {
            return 1234;
        }
        public static implicit operator decimal(ConvertibleToManyStruct arg) {
            return 1234;
        }
        public static implicit operator byte(ConvertibleToManyStruct arg) {
            return 123;
        }
    }

    public class ConvertibleToString {
        private string _value;
        public ConvertibleToString(string value) {
            _value = value;
        }
        public static implicit operator string(ConvertibleToString arg) {
            return arg._value;
        }

        public static implicit operator ConvertibleToString(string arg) {
            return new ConvertibleToString(arg);
        }

        public override string ToString() {
            return "CS_" + _value;
        }
    }

    public class ConvertibleToCW {
        private CurrencyWrapper _value;
        public ConvertibleToCW(CurrencyWrapper value) {
            _value = value;
        }
        public static implicit operator CurrencyWrapper(ConvertibleToCW arg) {
            return arg._value;
        }

        public static implicit operator ConvertibleToCW(CurrencyWrapper arg) {
            return new ConvertibleToCW(arg);
        }

        public override string ToString() {
            return "CW_" + _value.WrappedObject.ToString();
        }
    }

    public class MyIconvertible : IConvertible {
        object _value;
        private TypeCode _tc;

        public MyIconvertible(object value, TypeCode tc) {
            _value = value;
            _tc = tc;
        }

        TypeCode IConvertible.GetTypeCode() {
            return _tc;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider) {
            return this.ToIConvertible().ToBoolean(provider);
        }

        char IConvertible.ToChar(IFormatProvider provider) {
            return this.ToIConvertible().ToChar(provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider) {
            return this.ToIConvertible().ToSByte(provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider) {
            return this.ToIConvertible().ToByte(provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider) {
            return this.ToIConvertible().ToInt16(provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider) {
            return this.ToIConvertible().ToUInt16(provider);
        }

        private IConvertible ToIConvertible() {
            return this._value as IConvertible;
        }

        int IConvertible.ToInt32(IFormatProvider provider) {
            return this.ToIConvertible().ToInt32(provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider) {
            return this.ToIConvertible().ToUInt32(provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider) {
            return this.ToIConvertible().ToInt64(provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider) {
            return this.ToIConvertible().ToUInt64(provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider) {
            //Single doesnt have  COM equivalent. So let's use this to test 
            //throwing and catching an exception properly
            throw new NotImplementedException("ToSingle() is not implemented");
        }

        double IConvertible.ToDouble(IFormatProvider provider) {
            return this.ToIConvertible().ToDouble(provider);
        }

        Decimal IConvertible.ToDecimal(IFormatProvider provider) {
            return this.ToIConvertible().ToDecimal(provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider) {
            return this.ToIConvertible().ToDateTime(provider);
        }

        String IConvertible.ToString(IFormatProvider provider) {
            return this.ToIConvertible().ToString(provider);
        }

        Object IConvertible.ToType(Type conversionType, IFormatProvider provider) {
            return this.ToIConvertible().ToType(conversionType, provider);
        }

    }

}
