#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 259
			namespace Scenario259{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Add__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byteq_Add() &
				                check_sbyteq_Add() &
				                check_ushortq_Add() &
				                check_shortq_Add() &
				                check_uintq_Add() &
				                check_intq_Add() &
				                check_ulongq_Add() &
				                check_longq_Add() &
				                check_floatq_Add() &
				                check_doubleq_Add() &
				                check_decimalq_Add() &
				                check_charq_Add();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_Add()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static byte Add_byte(byte val0, byte val1)
				    {
				        return (byte)(val0 + val1);
				    }
				
				    static bool check_byteq_Add(byte? val0, byte? val1)
				    {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.Add(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    typeof(Test).GetMethod("Add_byte")
				                ));
				
				        Func<byte?> f = e.Compile();
				
				        byte? fResult = default(byte);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        byte? csResult = default(byte);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (byte?)(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_Add()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static sbyte Add_sbyte(sbyte val0, sbyte val1)
				    {
				        return (sbyte)(val0 + val1);
				    }
				
				    static bool check_sbyteq_Add(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.Add(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    typeof(Test).GetMethod("Add_sbyte")
				                ));
				
				        Func<sbyte?> f = e.Compile();
				
				        sbyte? fResult = default(sbyte);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        sbyte? csResult = default(sbyte);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (sbyte?)(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_Add()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ushort Add_ushort(ushort val0, ushort val1)
				    {
				        return (ushort)(val0 + val1);
				    }
				
				    static bool check_ushortq_Add(ushort? val0, ushort? val1)
				    {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.Add(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    typeof(Test).GetMethod("Add_ushort")
				                ));
				
				        Func<ushort?> f = e.Compile();
				
				        ushort? fResult = default(ushort);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ushort? csResult = default(ushort);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (ushort?)(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_Add()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static short Add_short(short val0, short val1)
				    {
				        return (short)(val0 + val1);
				    }
				
				    static bool check_shortq_Add(short? val0, short? val1)
				    {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.Add(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    typeof(Test).GetMethod("Add_short")
				                ));
				
				        Func<short?> f = e.Compile();
				
				        short? fResult = default(short);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        short? csResult = default(short);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (short?)(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_Add()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static uint Add_uint(uint val0, uint val1)
				    {
				        return (uint)(val0 + val1);
				    }
				
				    static bool check_uintq_Add(uint? val0, uint? val1)
				    {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.Add(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    typeof(Test).GetMethod("Add_uint")
				                ));
				
				        Func<uint?> f = e.Compile();
				
				        uint? fResult = default(uint);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        uint? csResult = default(uint);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (uint?)(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_Add()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static int Add_int(int val0, int val1)
				    {
				        return (int)(val0 + val1);
				    }
				
				    static bool check_intq_Add(int? val0, int? val1)
				    {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.Add(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    typeof(Test).GetMethod("Add_int")
				                ));
				
				        Func<int?> f = e.Compile();
				
				        int? fResult = default(int);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        int? csResult = default(int);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (int?)(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_Add()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ulong Add_ulong(ulong val0, ulong val1)
				    {
				        return (ulong)(val0 + val1);
				    }
				
				    static bool check_ulongq_Add(ulong? val0, ulong? val1)
				    {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.Add(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    typeof(Test).GetMethod("Add_ulong")
				                ));
				
				        Func<ulong?> f = e.Compile();
				
				        ulong? fResult = default(ulong);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ulong? csResult = default(ulong);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (ulong?)(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_Add()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static long Add_long(long val0, long val1)
				    {
				        return (long)(val0 + val1);
				    }
				
				    static bool check_longq_Add(long? val0, long? val1)
				    {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.Add(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    typeof(Test).GetMethod("Add_long")
				                ));
				
				        Func<long?> f = e.Compile();
				
				        long? fResult = default(long);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        long? csResult = default(long);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (long?)(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_Add()
				    {
				        float?[] svals = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static float Add_float(float val0, float val1)
				    {
				        return (float)(val0 + val1);
				    }
				
				    static bool check_floatq_Add(float? val0, float? val1)
				    {
				        Expression<Func<float?>> e =
				            Expression.Lambda<Func<float?>>(
				                Expression.Add(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    typeof(Test).GetMethod("Add_float")
				                ));
				
				        Func<float?> f = e.Compile();
				
				        float? fResult = default(float);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        float? csResult = default(float);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (float?)(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_Add()
				    {
				        double?[] svals = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static double Add_double(double val0, double val1)
				    {
				        return (double)(val0 + val1);
				    }
				
				    static bool check_doubleq_Add(double? val0, double? val1)
				    {
				        Expression<Func<double?>> e =
				            Expression.Lambda<Func<double?>>(
				                Expression.Add(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    typeof(Test).GetMethod("Add_double")
				                ));
				
				        Func<double?> f = e.Compile();
				
				        double? fResult = default(double);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        double? csResult = default(double);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (double?)(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_Add()
				    {
				        decimal?[] svals = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static decimal Add_decimal(decimal val0, decimal val1)
				    {
				        return (decimal)(val0 + val1);
				    }
				
				    static bool check_decimalq_Add(decimal? val0, decimal? val1)
				    {
				        Expression<Func<decimal?>> e =
				            Expression.Lambda<Func<decimal?>>(
				                Expression.Add(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    typeof(Test).GetMethod("Add_decimal")
				                ));
				
				        Func<decimal?> f = e.Compile();
				
				        decimal? fResult = default(decimal);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        decimal? csResult = default(decimal);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (decimal?)(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_charq_Add()
				    {
				        char?[] svals = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_charq_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("charq_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static char Add_char(char val0, char val1)
				    {
				        return (char)(val0 + val1);
				    }
				
				    static bool check_charq_Add(char? val0, char? val1)
				    {
				        Expression<Func<char?>> e =
				            Expression.Lambda<Func<char?>>(
				                Expression.Add(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char?)),
				                    typeof(Test).GetMethod("Add_char")
				                ));
				
				        Func<char?> f = e.Compile();
				
				        char? fResult = default(char);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        char? csResult = default(char);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (char?)(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 260
			namespace Scenario260{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Subtract__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Subtract__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byteq_Subtract() &
				                check_sbyteq_Subtract() &
				                check_ushortq_Subtract() &
				                check_shortq_Subtract() &
				                check_uintq_Subtract() &
				                check_intq_Subtract() &
				                check_ulongq_Subtract() &
				                check_longq_Subtract() &
				                check_floatq_Subtract() &
				                check_doubleq_Subtract() &
				                check_decimalq_Subtract() &
				                check_charq_Subtract();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_Subtract()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static byte Subtract_byte(byte val0, byte val1)
				    {
				        return (byte)(val0 - val1);
				    }
				
				    static bool check_byteq_Subtract(byte? val0, byte? val1)
				    {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.Subtract(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    typeof(Test).GetMethod("Subtract_byte")
				                ));
				
				        Func<byte?> f = e.Compile();
				
				        byte? fResult = default(byte);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        byte? csResult = default(byte);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (byte?)(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_Subtract()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static sbyte Subtract_sbyte(sbyte val0, sbyte val1)
				    {
				        return (sbyte)(val0 - val1);
				    }
				
				    static bool check_sbyteq_Subtract(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.Subtract(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    typeof(Test).GetMethod("Subtract_sbyte")
				                ));
				
				        Func<sbyte?> f = e.Compile();
				
				        sbyte? fResult = default(sbyte);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        sbyte? csResult = default(sbyte);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (sbyte?)(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_Subtract()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ushort Subtract_ushort(ushort val0, ushort val1)
				    {
				        return (ushort)(val0 - val1);
				    }
				
				    static bool check_ushortq_Subtract(ushort? val0, ushort? val1)
				    {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.Subtract(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    typeof(Test).GetMethod("Subtract_ushort")
				                ));
				
				        Func<ushort?> f = e.Compile();
				
				        ushort? fResult = default(ushort);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ushort? csResult = default(ushort);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (ushort?)(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_Subtract()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static short Subtract_short(short val0, short val1)
				    {
				        return (short)(val0 - val1);
				    }
				
				    static bool check_shortq_Subtract(short? val0, short? val1)
				    {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.Subtract(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    typeof(Test).GetMethod("Subtract_short")
				                ));
				
				        Func<short?> f = e.Compile();
				
				        short? fResult = default(short);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        short? csResult = default(short);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (short?)(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_Subtract()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static uint Subtract_uint(uint val0, uint val1)
				    {
				        return (uint)(val0 - val1);
				    }
				
				    static bool check_uintq_Subtract(uint? val0, uint? val1)
				    {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.Subtract(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    typeof(Test).GetMethod("Subtract_uint")
				                ));
				
				        Func<uint?> f = e.Compile();
				
				        uint? fResult = default(uint);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        uint? csResult = default(uint);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (uint?)(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_Subtract()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static int Subtract_int(int val0, int val1)
				    {
				        return (int)(val0 - val1);
				    }
				
				    static bool check_intq_Subtract(int? val0, int? val1)
				    {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.Subtract(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    typeof(Test).GetMethod("Subtract_int")
				                ));
				
				        Func<int?> f = e.Compile();
				
				        int? fResult = default(int);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        int? csResult = default(int);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (int?)(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_Subtract()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ulong Subtract_ulong(ulong val0, ulong val1)
				    {
				        return (ulong)(val0 - val1);
				    }
				
				    static bool check_ulongq_Subtract(ulong? val0, ulong? val1)
				    {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.Subtract(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    typeof(Test).GetMethod("Subtract_ulong")
				                ));
				
				        Func<ulong?> f = e.Compile();
				
				        ulong? fResult = default(ulong);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ulong? csResult = default(ulong);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (ulong?)(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_Subtract()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static long Subtract_long(long val0, long val1)
				    {
				        return (long)(val0 - val1);
				    }
				
				    static bool check_longq_Subtract(long? val0, long? val1)
				    {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.Subtract(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    typeof(Test).GetMethod("Subtract_long")
				                ));
				
				        Func<long?> f = e.Compile();
				
				        long? fResult = default(long);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        long? csResult = default(long);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (long?)(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_Subtract()
				    {
				        float?[] svals = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static float Subtract_float(float val0, float val1)
				    {
				        return (float)(val0 - val1);
				    }
				
				    static bool check_floatq_Subtract(float? val0, float? val1)
				    {
				        Expression<Func<float?>> e =
				            Expression.Lambda<Func<float?>>(
				                Expression.Subtract(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    typeof(Test).GetMethod("Subtract_float")
				                ));
				
				        Func<float?> f = e.Compile();
				
				        float? fResult = default(float);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        float? csResult = default(float);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (float?)(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_Subtract()
				    {
				        double?[] svals = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static double Subtract_double(double val0, double val1)
				    {
				        return (double)(val0 - val1);
				    }
				
				    static bool check_doubleq_Subtract(double? val0, double? val1)
				    {
				        Expression<Func<double?>> e =
				            Expression.Lambda<Func<double?>>(
				                Expression.Subtract(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    typeof(Test).GetMethod("Subtract_double")
				                ));
				
				        Func<double?> f = e.Compile();
				
				        double? fResult = default(double);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        double? csResult = default(double);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (double?)(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_Subtract()
				    {
				        decimal?[] svals = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static decimal Subtract_decimal(decimal val0, decimal val1)
				    {
				        return (decimal)(val0 - val1);
				    }
				
				    static bool check_decimalq_Subtract(decimal? val0, decimal? val1)
				    {
				        Expression<Func<decimal?>> e =
				            Expression.Lambda<Func<decimal?>>(
				                Expression.Subtract(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    typeof(Test).GetMethod("Subtract_decimal")
				                ));
				
				        Func<decimal?> f = e.Compile();
				
				        decimal? fResult = default(decimal);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        decimal? csResult = default(decimal);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (decimal?)(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_charq_Subtract()
				    {
				        char?[] svals = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_charq_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("charq_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static char Subtract_char(char val0, char val1)
				    {
				        return (char)(val0 - val1);
				    }
				
				    static bool check_charq_Subtract(char? val0, char? val1)
				    {
				        Expression<Func<char?>> e =
				            Expression.Lambda<Func<char?>>(
				                Expression.Subtract(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char?)),
				                    typeof(Test).GetMethod("Subtract_char")
				                ));
				
				        Func<char?> f = e.Compile();
				
				        char? fResult = default(char);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        char? csResult = default(char);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (char?)(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 261
			namespace Scenario261{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Multiply__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byteq_Multiply() &
				                check_sbyteq_Multiply() &
				                check_ushortq_Multiply() &
				                check_shortq_Multiply() &
				                check_uintq_Multiply() &
				                check_intq_Multiply() &
				                check_ulongq_Multiply() &
				                check_longq_Multiply() &
				                check_floatq_Multiply() &
				                check_doubleq_Multiply() &
				                check_decimalq_Multiply() &
				                check_charq_Multiply();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_Multiply()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static byte Multiply_byte(byte val0, byte val1)
				    {
				        return (byte)(val0 * val1);
				    }
				
				    static bool check_byteq_Multiply(byte? val0, byte? val1)
				    {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.Multiply(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    typeof(Test).GetMethod("Multiply_byte")
				                ));
				
				        Func<byte?> f = e.Compile();
				
				        byte? fResult = default(byte);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        byte? csResult = default(byte);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (byte?)(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_Multiply()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static sbyte Multiply_sbyte(sbyte val0, sbyte val1)
				    {
				        return (sbyte)(val0 * val1);
				    }
				
				    static bool check_sbyteq_Multiply(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.Multiply(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    typeof(Test).GetMethod("Multiply_sbyte")
				                ));
				
				        Func<sbyte?> f = e.Compile();
				
				        sbyte? fResult = default(sbyte);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        sbyte? csResult = default(sbyte);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (sbyte?)(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_Multiply()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ushort Multiply_ushort(ushort val0, ushort val1)
				    {
				        return (ushort)(val0 * val1);
				    }
				
				    static bool check_ushortq_Multiply(ushort? val0, ushort? val1)
				    {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.Multiply(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    typeof(Test).GetMethod("Multiply_ushort")
				                ));
				
				        Func<ushort?> f = e.Compile();
				
				        ushort? fResult = default(ushort);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ushort? csResult = default(ushort);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (ushort?)(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_Multiply()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static short Multiply_short(short val0, short val1)
				    {
				        return (short)(val0 * val1);
				    }
				
				    static bool check_shortq_Multiply(short? val0, short? val1)
				    {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.Multiply(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    typeof(Test).GetMethod("Multiply_short")
				                ));
				
				        Func<short?> f = e.Compile();
				
				        short? fResult = default(short);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        short? csResult = default(short);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (short?)(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_Multiply()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static uint Multiply_uint(uint val0, uint val1)
				    {
				        return (uint)(val0 * val1);
				    }
				
				    static bool check_uintq_Multiply(uint? val0, uint? val1)
				    {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.Multiply(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    typeof(Test).GetMethod("Multiply_uint")
				                ));
				
				        Func<uint?> f = e.Compile();
				
				        uint? fResult = default(uint);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        uint? csResult = default(uint);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (uint?)(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_Multiply()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static int Multiply_int(int val0, int val1)
				    {
				        return (int)(val0 * val1);
				    }
				
				    static bool check_intq_Multiply(int? val0, int? val1)
				    {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.Multiply(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    typeof(Test).GetMethod("Multiply_int")
				                ));
				
				        Func<int?> f = e.Compile();
				
				        int? fResult = default(int);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        int? csResult = default(int);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (int?)(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_Multiply()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ulong Multiply_ulong(ulong val0, ulong val1)
				    {
				        return (ulong)(val0 * val1);
				    }
				
				    static bool check_ulongq_Multiply(ulong? val0, ulong? val1)
				    {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.Multiply(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    typeof(Test).GetMethod("Multiply_ulong")
				                ));
				
				        Func<ulong?> f = e.Compile();
				
				        ulong? fResult = default(ulong);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ulong? csResult = default(ulong);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (ulong?)(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_Multiply()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static long Multiply_long(long val0, long val1)
				    {
				        return (long)(val0 * val1);
				    }
				
				    static bool check_longq_Multiply(long? val0, long? val1)
				    {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.Multiply(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    typeof(Test).GetMethod("Multiply_long")
				                ));
				
				        Func<long?> f = e.Compile();
				
				        long? fResult = default(long);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        long? csResult = default(long);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (long?)(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_Multiply()
				    {
				        float?[] svals = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static float Multiply_float(float val0, float val1)
				    {
				        return (float)(val0 * val1);
				    }
				
				    static bool check_floatq_Multiply(float? val0, float? val1)
				    {
				        Expression<Func<float?>> e =
				            Expression.Lambda<Func<float?>>(
				                Expression.Multiply(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    typeof(Test).GetMethod("Multiply_float")
				                ));
				
				        Func<float?> f = e.Compile();
				
				        float? fResult = default(float);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        float? csResult = default(float);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (float?)(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_Multiply()
				    {
				        double?[] svals = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static double Multiply_double(double val0, double val1)
				    {
				        return (double)(val0 * val1);
				    }
				
				    static bool check_doubleq_Multiply(double? val0, double? val1)
				    {
				        Expression<Func<double?>> e =
				            Expression.Lambda<Func<double?>>(
				                Expression.Multiply(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    typeof(Test).GetMethod("Multiply_double")
				                ));
				
				        Func<double?> f = e.Compile();
				
				        double? fResult = default(double);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        double? csResult = default(double);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (double?)(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_Multiply()
				    {
				        decimal?[] svals = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static decimal Multiply_decimal(decimal val0, decimal val1)
				    {
				        return (decimal)(val0 * val1);
				    }
				
				    static bool check_decimalq_Multiply(decimal? val0, decimal? val1)
				    {
				        Expression<Func<decimal?>> e =
				            Expression.Lambda<Func<decimal?>>(
				                Expression.Multiply(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    typeof(Test).GetMethod("Multiply_decimal")
				                ));
				
				        Func<decimal?> f = e.Compile();
				
				        decimal? fResult = default(decimal);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        decimal? csResult = default(decimal);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (decimal?)(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_charq_Multiply()
				    {
				        char?[] svals = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_charq_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("charq_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static char Multiply_char(char val0, char val1)
				    {
				        return (char)(val0 * val1);
				    }
				
				    static bool check_charq_Multiply(char? val0, char? val1)
				    {
				        Expression<Func<char?>> e =
				            Expression.Lambda<Func<char?>>(
				                Expression.Multiply(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char?)),
				                    typeof(Test).GetMethod("Multiply_char")
				                ));
				
				        Func<char?> f = e.Compile();
				
				        char? fResult = default(char);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        char? csResult = default(char);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (char?)(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 262
			namespace Scenario262{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Divide__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byteq_Divide() &
				                check_sbyteq_Divide() &
				                check_ushortq_Divide() &
				                check_shortq_Divide() &
				                check_uintq_Divide() &
				                check_intq_Divide() &
				                check_ulongq_Divide() &
				                check_longq_Divide() &
				                check_floatq_Divide() &
				                check_doubleq_Divide() &
				                check_decimalq_Divide() &
				                check_charq_Divide();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_Divide()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static byte Divide_byte(byte val0, byte val1)
				    {
				        return (byte)(val0 / val1);
				    }
				
				    static bool check_byteq_Divide(byte? val0, byte? val1)
				    {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.Divide(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    typeof(Test).GetMethod("Divide_byte")
				                ));
				
				        Func<byte?> f = e.Compile();
				
				        byte? fResult = default(byte);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        byte? csResult = default(byte);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (byte?)(val0 / val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_Divide()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static sbyte Divide_sbyte(sbyte val0, sbyte val1)
				    {
				        return (sbyte)(val0 / val1);
				    }
				
				    static bool check_sbyteq_Divide(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.Divide(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    typeof(Test).GetMethod("Divide_sbyte")
				                ));
				
				        Func<sbyte?> f = e.Compile();
				
				        sbyte? fResult = default(sbyte);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        sbyte? csResult = default(sbyte);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (sbyte?)(val0 / val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_Divide()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ushort Divide_ushort(ushort val0, ushort val1)
				    {
				        return (ushort)(val0 / val1);
				    }
				
				    static bool check_ushortq_Divide(ushort? val0, ushort? val1)
				    {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.Divide(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    typeof(Test).GetMethod("Divide_ushort")
				                ));
				
				        Func<ushort?> f = e.Compile();
				
				        ushort? fResult = default(ushort);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ushort? csResult = default(ushort);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (ushort?)(val0 / val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_Divide()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static short Divide_short(short val0, short val1)
				    {
				        return (short)(val0 / val1);
				    }
				
				    static bool check_shortq_Divide(short? val0, short? val1)
				    {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.Divide(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    typeof(Test).GetMethod("Divide_short")
				                ));
				
				        Func<short?> f = e.Compile();
				
				        short? fResult = default(short);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        short? csResult = default(short);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (short?)(val0 / val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_Divide()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static uint Divide_uint(uint val0, uint val1)
				    {
				        return (uint)(val0 / val1);
				    }
				
				    static bool check_uintq_Divide(uint? val0, uint? val1)
				    {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.Divide(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    typeof(Test).GetMethod("Divide_uint")
				                ));
				
				        Func<uint?> f = e.Compile();
				
				        uint? fResult = default(uint);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        uint? csResult = default(uint);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (uint?)(val0 / val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_Divide()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static int Divide_int(int val0, int val1)
				    {
				        return (int)(val0 / val1);
				    }
				
				    static bool check_intq_Divide(int? val0, int? val1)
				    {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.Divide(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    typeof(Test).GetMethod("Divide_int")
				                ));
				
				        Func<int?> f = e.Compile();
				
				        int? fResult = default(int);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        int? csResult = default(int);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (int?)(val0 / val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_Divide()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ulong Divide_ulong(ulong val0, ulong val1)
				    {
				        return (ulong)(val0 / val1);
				    }
				
				    static bool check_ulongq_Divide(ulong? val0, ulong? val1)
				    {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.Divide(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    typeof(Test).GetMethod("Divide_ulong")
				                ));
				
				        Func<ulong?> f = e.Compile();
				
				        ulong? fResult = default(ulong);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ulong? csResult = default(ulong);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (ulong?)(val0 / val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_Divide()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static long Divide_long(long val0, long val1)
				    {
				        return (long)(val0 / val1);
				    }
				
				    static bool check_longq_Divide(long? val0, long? val1)
				    {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.Divide(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    typeof(Test).GetMethod("Divide_long")
				                ));
				
				        Func<long?> f = e.Compile();
				
				        long? fResult = default(long);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        long? csResult = default(long);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (long?)(val0 / val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_Divide()
				    {
				        float?[] svals = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static float Divide_float(float val0, float val1)
				    {
				        return (float)(val0 / val1);
				    }
				
				    static bool check_floatq_Divide(float? val0, float? val1)
				    {
				        Expression<Func<float?>> e =
				            Expression.Lambda<Func<float?>>(
				                Expression.Divide(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    typeof(Test).GetMethod("Divide_float")
				                ));
				
				        Func<float?> f = e.Compile();
				
				        float? fResult = default(float);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        float? csResult = default(float);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (float?)(val0 / val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_Divide()
				    {
				        double?[] svals = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static double Divide_double(double val0, double val1)
				    {
				        return (double)(val0 / val1);
				    }
				
				    static bool check_doubleq_Divide(double? val0, double? val1)
				    {
				        Expression<Func<double?>> e =
				            Expression.Lambda<Func<double?>>(
				                Expression.Divide(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    typeof(Test).GetMethod("Divide_double")
				                ));
				
				        Func<double?> f = e.Compile();
				
				        double? fResult = default(double);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        double? csResult = default(double);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (double?)(val0 / val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_Divide()
				    {
				        decimal?[] svals = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static decimal Divide_decimal(decimal val0, decimal val1)
				    {
				        return (decimal)(val0 / val1);
				    }
				
				    static bool check_decimalq_Divide(decimal? val0, decimal? val1)
				    {
				        Expression<Func<decimal?>> e =
				            Expression.Lambda<Func<decimal?>>(
				                Expression.Divide(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    typeof(Test).GetMethod("Divide_decimal")
				                ));
				
				        Func<decimal?> f = e.Compile();
				
				        decimal? fResult = default(decimal);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        decimal? csResult = default(decimal);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (decimal?)(val0 / val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_charq_Divide()
				    {
				        char?[] svals = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_charq_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("charq_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static char Divide_char(char val0, char val1)
				    {
				        return (char)(val0 / val1);
				    }
				
				    static bool check_charq_Divide(char? val0, char? val1)
				    {
				        Expression<Func<char?>> e =
				            Expression.Lambda<Func<char?>>(
				                Expression.Divide(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char?)),
				                    typeof(Test).GetMethod("Divide_char")
				                ));
				
				        Func<char?> f = e.Compile();
				
				        char? fResult = default(char);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        char? csResult = default(char);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (char?)(val0 / val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 263
			namespace Scenario263{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Modulo__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byteq_Modulo() &
				                check_sbyteq_Modulo() &
				                check_ushortq_Modulo() &
				                check_shortq_Modulo() &
				                check_uintq_Modulo() &
				                check_intq_Modulo() &
				                check_ulongq_Modulo() &
				                check_longq_Modulo() &
				                check_floatq_Modulo() &
				                check_doubleq_Modulo() &
				                check_decimalq_Modulo() &
				                check_charq_Modulo();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_Modulo()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static byte Modulo_byte(byte val0, byte val1)
				    {
				        return (byte)(val0 % val1);
				    }
				
				    static bool check_byteq_Modulo(byte? val0, byte? val1)
				    {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.Modulo(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    typeof(Test).GetMethod("Modulo_byte")
				                ));
				
				        Func<byte?> f = e.Compile();
				
				        byte? fResult = default(byte);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        byte? csResult = default(byte);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (byte?)(val0 % val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_Modulo()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static sbyte Modulo_sbyte(sbyte val0, sbyte val1)
				    {
				        return (sbyte)(val0 % val1);
				    }
				
				    static bool check_sbyteq_Modulo(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.Modulo(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    typeof(Test).GetMethod("Modulo_sbyte")
				                ));
				
				        Func<sbyte?> f = e.Compile();
				
				        sbyte? fResult = default(sbyte);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        sbyte? csResult = default(sbyte);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (sbyte?)(val0 % val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_Modulo()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ushort Modulo_ushort(ushort val0, ushort val1)
				    {
				        return (ushort)(val0 % val1);
				    }
				
				    static bool check_ushortq_Modulo(ushort? val0, ushort? val1)
				    {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.Modulo(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    typeof(Test).GetMethod("Modulo_ushort")
				                ));
				
				        Func<ushort?> f = e.Compile();
				
				        ushort? fResult = default(ushort);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ushort? csResult = default(ushort);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (ushort?)(val0 % val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_Modulo()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static short Modulo_short(short val0, short val1)
				    {
				        return (short)(val0 % val1);
				    }
				
				    static bool check_shortq_Modulo(short? val0, short? val1)
				    {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.Modulo(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    typeof(Test).GetMethod("Modulo_short")
				                ));
				
				        Func<short?> f = e.Compile();
				
				        short? fResult = default(short);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        short? csResult = default(short);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (short?)(val0 % val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_Modulo()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static uint Modulo_uint(uint val0, uint val1)
				    {
				        return (uint)(val0 % val1);
				    }
				
				    static bool check_uintq_Modulo(uint? val0, uint? val1)
				    {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.Modulo(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    typeof(Test).GetMethod("Modulo_uint")
				                ));
				
				        Func<uint?> f = e.Compile();
				
				        uint? fResult = default(uint);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        uint? csResult = default(uint);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (uint?)(val0 % val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_Modulo()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static int Modulo_int(int val0, int val1)
				    {
				        return (int)(val0 % val1);
				    }
				
				    static bool check_intq_Modulo(int? val0, int? val1)
				    {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.Modulo(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    typeof(Test).GetMethod("Modulo_int")
				                ));
				
				        Func<int?> f = e.Compile();
				
				        int? fResult = default(int);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        int? csResult = default(int);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (int?)(val0 % val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_Modulo()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ulong Modulo_ulong(ulong val0, ulong val1)
				    {
				        return (ulong)(val0 % val1);
				    }
				
				    static bool check_ulongq_Modulo(ulong? val0, ulong? val1)
				    {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.Modulo(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    typeof(Test).GetMethod("Modulo_ulong")
				                ));
				
				        Func<ulong?> f = e.Compile();
				
				        ulong? fResult = default(ulong);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ulong? csResult = default(ulong);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (ulong?)(val0 % val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_Modulo()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static long Modulo_long(long val0, long val1)
				    {
				        return (long)(val0 % val1);
				    }
				
				    static bool check_longq_Modulo(long? val0, long? val1)
				    {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.Modulo(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    typeof(Test).GetMethod("Modulo_long")
				                ));
				
				        Func<long?> f = e.Compile();
				
				        long? fResult = default(long);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        long? csResult = default(long);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (long?)(val0 % val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_Modulo()
				    {
				        float?[] svals = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static float Modulo_float(float val0, float val1)
				    {
				        return (float)(val0 % val1);
				    }
				
				    static bool check_floatq_Modulo(float? val0, float? val1)
				    {
				        Expression<Func<float?>> e =
				            Expression.Lambda<Func<float?>>(
				                Expression.Modulo(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    typeof(Test).GetMethod("Modulo_float")
				                ));
				
				        Func<float?> f = e.Compile();
				
				        float? fResult = default(float);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        float? csResult = default(float);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (float?)(val0 % val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_Modulo()
				    {
				        double?[] svals = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static double Modulo_double(double val0, double val1)
				    {
				        return (double)(val0 % val1);
				    }
				
				    static bool check_doubleq_Modulo(double? val0, double? val1)
				    {
				        Expression<Func<double?>> e =
				            Expression.Lambda<Func<double?>>(
				                Expression.Modulo(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    typeof(Test).GetMethod("Modulo_double")
				                ));
				
				        Func<double?> f = e.Compile();
				
				        double? fResult = default(double);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        double? csResult = default(double);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (double?)(val0 % val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_Modulo()
				    {
				        decimal?[] svals = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static decimal Modulo_decimal(decimal val0, decimal val1)
				    {
				        return (decimal)(val0 % val1);
				    }
				
				    static bool check_decimalq_Modulo(decimal? val0, decimal? val1)
				    {
				        Expression<Func<decimal?>> e =
				            Expression.Lambda<Func<decimal?>>(
				                Expression.Modulo(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    typeof(Test).GetMethod("Modulo_decimal")
				                ));
				
				        Func<decimal?> f = e.Compile();
				
				        decimal? fResult = default(decimal);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        decimal? csResult = default(decimal);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (decimal?)(val0 % val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_charq_Modulo()
				    {
				        char?[] svals = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_charq_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("charq_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static char Modulo_char(char val0, char val1)
				    {
				        return (char)(val0 % val1);
				    }
				
				    static bool check_charq_Modulo(char? val0, char? val1)
				    {
				        Expression<Func<char?>> e =
				            Expression.Lambda<Func<char?>>(
				                Expression.Modulo(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char?)),
				                    typeof(Test).GetMethod("Modulo_char")
				                ));
				
				        Func<char?> f = e.Compile();
				
				        char? fResult = default(char);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        char? csResult = default(char);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else
				        {
				            try
				            {
				                csResult = (char?)(val0 % val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
	
}
