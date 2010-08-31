#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 274
			namespace Scenario274{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Equal__() {
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
				            success = check_byteq_bool_Equal() &
				                check_sbyteq_bool_Equal() &
				                check_ushortq_bool_Equal() &
				                check_shortq_bool_Equal() &
				                check_uintq_bool_Equal() &
				                check_intq_bool_Equal() &
				                check_ulongq_bool_Equal() &
				                check_longq_bool_Equal() &
				                check_floatq_bool_Equal() &
				                check_doubleq_bool_Equal() &
				                check_decimalq_bool_Equal() &
				                check_charq_bool_Equal();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_bool_Equal()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_bool_Equal(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_bool_Equal(byte? val0, byte? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    false,
				                    typeof(Test).GetMethod("Equal_byte")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_bool_Equal()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_bool_Equal(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_bool_Equal(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    false,
				                    typeof(Test).GetMethod("Equal_sbyte")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_bool_Equal()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_bool_Equal(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_bool_Equal(ushort? val0, ushort? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    false,
				                    typeof(Test).GetMethod("Equal_ushort")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_bool_Equal()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_bool_Equal(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_bool_Equal(short? val0, short? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    false,
				                    typeof(Test).GetMethod("Equal_short")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_bool_Equal()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_bool_Equal(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_bool_Equal(uint? val0, uint? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    false,
				                    typeof(Test).GetMethod("Equal_uint")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_bool_Equal()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_bool_Equal(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_bool_Equal(int? val0, int? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    false,
				                    typeof(Test).GetMethod("Equal_int")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_bool_Equal()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_bool_Equal(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_bool_Equal(ulong? val0, ulong? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    false,
				                    typeof(Test).GetMethod("Equal_ulong")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_bool_Equal()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_bool_Equal(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_bool_Equal(long? val0, long? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    false,
				                    typeof(Test).GetMethod("Equal_long")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_bool_Equal()
				    {
				        float?[] svals = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_bool_Equal(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_floatq_bool_Equal(float? val0, float? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    false,
				                    typeof(Test).GetMethod("Equal_float")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_bool_Equal()
				    {
				        double?[] svals = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_bool_Equal(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_doubleq_bool_Equal(double? val0, double? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    false,
				                    typeof(Test).GetMethod("Equal_double")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_bool_Equal()
				    {
				        decimal?[] svals = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_bool_Equal(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimalq_bool_Equal(decimal? val0, decimal? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    false,
				                    typeof(Test).GetMethod("Equal_decimal")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_charq_bool_Equal()
				    {
				        char?[] svals = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_charq_bool_Equal(svals[i], svals[j]))
				                {
				                    Console.WriteLine("charq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_charq_bool_Equal(char? val0, char? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char?)),
				                    false,
				                    typeof(Test).GetMethod("Equal_char")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
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
			
			//-------- Scenario 275
			namespace Scenario275{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression NotEqual__() {
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
				            success = check_byteq_bool_NotEqual() &
				                check_sbyteq_bool_NotEqual() &
				                check_ushortq_bool_NotEqual() &
				                check_shortq_bool_NotEqual() &
				                check_uintq_bool_NotEqual() &
				                check_intq_bool_NotEqual() &
				                check_ulongq_bool_NotEqual() &
				                check_longq_bool_NotEqual() &
				                check_floatq_bool_NotEqual() &
				                check_doubleq_bool_NotEqual() &
				                check_decimalq_bool_NotEqual() &
				                check_charq_bool_NotEqual();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_bool_NotEqual()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_bool_NotEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_bool_NotEqual(byte? val0, byte? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    false,
				                    typeof(Test).GetMethod("NotEqual_byte")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 != val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_bool_NotEqual()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_bool_NotEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_bool_NotEqual(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    false,
				                    typeof(Test).GetMethod("NotEqual_sbyte")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 != val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_bool_NotEqual()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_bool_NotEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_bool_NotEqual(ushort? val0, ushort? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    false,
				                    typeof(Test).GetMethod("NotEqual_ushort")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 != val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_bool_NotEqual()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_bool_NotEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_bool_NotEqual(short? val0, short? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    false,
				                    typeof(Test).GetMethod("NotEqual_short")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 != val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_bool_NotEqual()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_bool_NotEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_bool_NotEqual(uint? val0, uint? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    false,
				                    typeof(Test).GetMethod("NotEqual_uint")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 != val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_bool_NotEqual()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_bool_NotEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_bool_NotEqual(int? val0, int? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    false,
				                    typeof(Test).GetMethod("NotEqual_int")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 != val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_bool_NotEqual()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_bool_NotEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_bool_NotEqual(ulong? val0, ulong? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    false,
				                    typeof(Test).GetMethod("NotEqual_ulong")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 != val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_bool_NotEqual()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_bool_NotEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_bool_NotEqual(long? val0, long? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    false,
				                    typeof(Test).GetMethod("NotEqual_long")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 != val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_bool_NotEqual()
				    {
				        float?[] svals = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_bool_NotEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_floatq_bool_NotEqual(float? val0, float? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    false,
				                    typeof(Test).GetMethod("NotEqual_float")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 != val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_bool_NotEqual()
				    {
				        double?[] svals = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_bool_NotEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_doubleq_bool_NotEqual(double? val0, double? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    false,
				                    typeof(Test).GetMethod("NotEqual_double")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 != val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_bool_NotEqual()
				    {
				        decimal?[] svals = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_bool_NotEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimalq_bool_NotEqual(decimal? val0, decimal? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    false,
				                    typeof(Test).GetMethod("NotEqual_decimal")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 != val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_charq_bool_NotEqual()
				    {
				        char?[] svals = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_charq_bool_NotEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("charq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_charq_bool_NotEqual(char? val0, char? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char?)),
				                    false,
				                    typeof(Test).GetMethod("NotEqual_char")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 != val1;
				
				        return object.Equals(fResult, csResult);
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
			
			//-------- Scenario 276
			namespace Scenario276{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThanOrEqual__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression GreaterThanOrEqual__() {
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
				            success = check_byteq_bool_GreaterThanOrEqual() &
				                check_sbyteq_bool_GreaterThanOrEqual() &
				                check_ushortq_bool_GreaterThanOrEqual() &
				                check_shortq_bool_GreaterThanOrEqual() &
				                check_uintq_bool_GreaterThanOrEqual() &
				                check_intq_bool_GreaterThanOrEqual() &
				                check_ulongq_bool_GreaterThanOrEqual() &
				                check_longq_bool_GreaterThanOrEqual() &
				                check_floatq_bool_GreaterThanOrEqual() &
				                check_doubleq_bool_GreaterThanOrEqual() &
				                check_decimalq_bool_GreaterThanOrEqual() &
				                check_charq_bool_GreaterThanOrEqual();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_bool_GreaterThanOrEqual()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_bool_GreaterThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_bool_GreaterThanOrEqual(byte? val0, byte? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThanOrEqual_byte")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 >= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_bool_GreaterThanOrEqual()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_bool_GreaterThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_bool_GreaterThanOrEqual(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThanOrEqual_sbyte")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 >= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_bool_GreaterThanOrEqual()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_bool_GreaterThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_bool_GreaterThanOrEqual(ushort? val0, ushort? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThanOrEqual_ushort")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 >= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_bool_GreaterThanOrEqual()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_bool_GreaterThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_bool_GreaterThanOrEqual(short? val0, short? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThanOrEqual_short")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 >= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_bool_GreaterThanOrEqual()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_bool_GreaterThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_bool_GreaterThanOrEqual(uint? val0, uint? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThanOrEqual_uint")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 >= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_bool_GreaterThanOrEqual()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_bool_GreaterThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_bool_GreaterThanOrEqual(int? val0, int? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThanOrEqual_int")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 >= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_bool_GreaterThanOrEqual()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_bool_GreaterThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_bool_GreaterThanOrEqual(ulong? val0, ulong? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThanOrEqual_ulong")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 >= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_bool_GreaterThanOrEqual()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_bool_GreaterThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_bool_GreaterThanOrEqual(long? val0, long? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThanOrEqual_long")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 >= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_bool_GreaterThanOrEqual()
				    {
				        float?[] svals = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_bool_GreaterThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_floatq_bool_GreaterThanOrEqual(float? val0, float? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThanOrEqual_float")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 >= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_bool_GreaterThanOrEqual()
				    {
				        double?[] svals = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_bool_GreaterThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_doubleq_bool_GreaterThanOrEqual(double? val0, double? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThanOrEqual_double")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 >= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_bool_GreaterThanOrEqual()
				    {
				        decimal?[] svals = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_bool_GreaterThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimalq_bool_GreaterThanOrEqual(decimal? val0, decimal? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThanOrEqual_decimal")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 >= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_charq_bool_GreaterThanOrEqual()
				    {
				        char?[] svals = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_charq_bool_GreaterThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("charq_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_charq_bool_GreaterThanOrEqual(char? val0, char? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThanOrEqual_char")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 >= val1;
				
				        return object.Equals(fResult, csResult);
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
			
			//-------- Scenario 277
			namespace Scenario277{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression GreaterThan__() {
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
				            success = check_byteq_bool_GreaterThan() &
				                check_sbyteq_bool_GreaterThan() &
				                check_ushortq_bool_GreaterThan() &
				                check_shortq_bool_GreaterThan() &
				                check_uintq_bool_GreaterThan() &
				                check_intq_bool_GreaterThan() &
				                check_ulongq_bool_GreaterThan() &
				                check_longq_bool_GreaterThan() &
				                check_floatq_bool_GreaterThan() &
				                check_doubleq_bool_GreaterThan() &
				                check_decimalq_bool_GreaterThan() &
				                check_charq_bool_GreaterThan();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_bool_GreaterThan()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_bool_GreaterThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_bool_GreaterThan(byte? val0, byte? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThan_byte")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 > val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_bool_GreaterThan()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_bool_GreaterThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_bool_GreaterThan(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThan_sbyte")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 > val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_bool_GreaterThan()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_bool_GreaterThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_bool_GreaterThan(ushort? val0, ushort? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThan_ushort")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 > val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_bool_GreaterThan()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_bool_GreaterThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_bool_GreaterThan(short? val0, short? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThan_short")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 > val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_bool_GreaterThan()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_bool_GreaterThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_bool_GreaterThan(uint? val0, uint? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThan_uint")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 > val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_bool_GreaterThan()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_bool_GreaterThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_bool_GreaterThan(int? val0, int? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThan_int")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 > val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_bool_GreaterThan()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_bool_GreaterThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_bool_GreaterThan(ulong? val0, ulong? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThan_ulong")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 > val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_bool_GreaterThan()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_bool_GreaterThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_bool_GreaterThan(long? val0, long? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThan_long")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 > val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_bool_GreaterThan()
				    {
				        float?[] svals = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_bool_GreaterThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_floatq_bool_GreaterThan(float? val0, float? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThan_float")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 > val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_bool_GreaterThan()
				    {
				        double?[] svals = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_bool_GreaterThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_doubleq_bool_GreaterThan(double? val0, double? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThan_double")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 > val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_bool_GreaterThan()
				    {
				        decimal?[] svals = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_bool_GreaterThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimalq_bool_GreaterThan(decimal? val0, decimal? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThan_decimal")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 > val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_charq_bool_GreaterThan()
				    {
				        char?[] svals = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_charq_bool_GreaterThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("charq_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_charq_bool_GreaterThan(char? val0, char? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char?)),
				                    false,
				                    typeof(Test).GetMethod("GreaterThan_char")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 > val1;
				
				        return object.Equals(fResult, csResult);
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
			
			//-------- Scenario 278
			namespace Scenario278{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThan__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression LessThan__() {
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
				            success = check_byteq_bool_LessThan() &
				                check_sbyteq_bool_LessThan() &
				                check_ushortq_bool_LessThan() &
				                check_shortq_bool_LessThan() &
				                check_uintq_bool_LessThan() &
				                check_intq_bool_LessThan() &
				                check_ulongq_bool_LessThan() &
				                check_longq_bool_LessThan() &
				                check_floatq_bool_LessThan() &
				                check_doubleq_bool_LessThan() &
				                check_decimalq_bool_LessThan() &
				                check_charq_bool_LessThan();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_bool_LessThan()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_bool_LessThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_bool_LessThan(byte? val0, byte? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    false,
				                    typeof(Test).GetMethod("LessThan_byte")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 < val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_bool_LessThan()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_bool_LessThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_bool_LessThan(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    false,
				                    typeof(Test).GetMethod("LessThan_sbyte")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 < val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_bool_LessThan()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_bool_LessThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_bool_LessThan(ushort? val0, ushort? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    false,
				                    typeof(Test).GetMethod("LessThan_ushort")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 < val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_bool_LessThan()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_bool_LessThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_bool_LessThan(short? val0, short? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    false,
				                    typeof(Test).GetMethod("LessThan_short")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 < val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_bool_LessThan()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_bool_LessThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_bool_LessThan(uint? val0, uint? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    false,
				                    typeof(Test).GetMethod("LessThan_uint")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 < val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_bool_LessThan()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_bool_LessThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_bool_LessThan(int? val0, int? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    false,
				                    typeof(Test).GetMethod("LessThan_int")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 < val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_bool_LessThan()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_bool_LessThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_bool_LessThan(ulong? val0, ulong? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    false,
				                    typeof(Test).GetMethod("LessThan_ulong")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 < val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_bool_LessThan()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_bool_LessThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_bool_LessThan(long? val0, long? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    false,
				                    typeof(Test).GetMethod("LessThan_long")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 < val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_bool_LessThan()
				    {
				        float?[] svals = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_bool_LessThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_floatq_bool_LessThan(float? val0, float? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    false,
				                    typeof(Test).GetMethod("LessThan_float")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 < val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_bool_LessThan()
				    {
				        double?[] svals = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_bool_LessThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_doubleq_bool_LessThan(double? val0, double? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    false,
				                    typeof(Test).GetMethod("LessThan_double")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 < val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_bool_LessThan()
				    {
				        decimal?[] svals = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_bool_LessThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimalq_bool_LessThan(decimal? val0, decimal? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    false,
				                    typeof(Test).GetMethod("LessThan_decimal")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 < val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_charq_bool_LessThan()
				    {
				        char?[] svals = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_charq_bool_LessThan(svals[i], svals[j]))
				                {
				                    Console.WriteLine("charq_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_charq_bool_LessThan(char? val0, char? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char?)),
				                    false,
				                    typeof(Test).GetMethod("LessThan_char")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 < val1;
				
				        return object.Equals(fResult, csResult);
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
			
			//-------- Scenario 279
			namespace Scenario279{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression LessThanOrEqual__() {
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
				            success = check_byteq_bool_LessThanOrEqual() &
				                check_sbyteq_bool_LessThanOrEqual() &
				                check_ushortq_bool_LessThanOrEqual() &
				                check_shortq_bool_LessThanOrEqual() &
				                check_uintq_bool_LessThanOrEqual() &
				                check_intq_bool_LessThanOrEqual() &
				                check_ulongq_bool_LessThanOrEqual() &
				                check_longq_bool_LessThanOrEqual() &
				                check_floatq_bool_LessThanOrEqual() &
				                check_doubleq_bool_LessThanOrEqual() &
				                check_decimalq_bool_LessThanOrEqual() &
				                check_charq_bool_LessThanOrEqual();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_bool_LessThanOrEqual()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_bool_LessThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_bool_LessThanOrEqual(byte? val0, byte? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    false,
				                    typeof(Test).GetMethod("LessThanOrEqual_byte")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 <= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_bool_LessThanOrEqual()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_bool_LessThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_bool_LessThanOrEqual(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    false,
				                    typeof(Test).GetMethod("LessThanOrEqual_sbyte")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 <= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_bool_LessThanOrEqual()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_bool_LessThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_bool_LessThanOrEqual(ushort? val0, ushort? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    false,
				                    typeof(Test).GetMethod("LessThanOrEqual_ushort")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 <= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_bool_LessThanOrEqual()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_bool_LessThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_bool_LessThanOrEqual(short? val0, short? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    false,
				                    typeof(Test).GetMethod("LessThanOrEqual_short")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 <= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_bool_LessThanOrEqual()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_bool_LessThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_bool_LessThanOrEqual(uint? val0, uint? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    false,
				                    typeof(Test).GetMethod("LessThanOrEqual_uint")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 <= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_bool_LessThanOrEqual()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_bool_LessThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_bool_LessThanOrEqual(int? val0, int? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    false,
				                    typeof(Test).GetMethod("LessThanOrEqual_int")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 <= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_bool_LessThanOrEqual()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_bool_LessThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_bool_LessThanOrEqual(ulong? val0, ulong? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    false,
				                    typeof(Test).GetMethod("LessThanOrEqual_ulong")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 <= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_bool_LessThanOrEqual()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_bool_LessThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_bool_LessThanOrEqual(long? val0, long? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    false,
				                    typeof(Test).GetMethod("LessThanOrEqual_long")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 <= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_bool_LessThanOrEqual()
				    {
				        float?[] svals = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_bool_LessThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_floatq_bool_LessThanOrEqual(float? val0, float? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    false,
				                    typeof(Test).GetMethod("LessThanOrEqual_float")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 <= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_bool_LessThanOrEqual()
				    {
				        double?[] svals = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_bool_LessThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_doubleq_bool_LessThanOrEqual(double? val0, double? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    false,
				                    typeof(Test).GetMethod("LessThanOrEqual_double")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 <= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_bool_LessThanOrEqual()
				    {
				        decimal?[] svals = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_bool_LessThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimalq_bool_LessThanOrEqual(decimal? val0, decimal? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    false,
				                    typeof(Test).GetMethod("LessThanOrEqual_decimal")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 <= val1;
				
				        return object.Equals(fResult, csResult);
				    }
				
				    static bool check_charq_bool_LessThanOrEqual()
				    {
				        char?[] svals = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_charq_bool_LessThanOrEqual(svals[i], svals[j]))
				                {
				                    Console.WriteLine("charq_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_charq_bool_LessThanOrEqual(char? val0, char? val1)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char?)),
				                    false,
				                    typeof(Test).GetMethod("LessThanOrEqual_char")
				                ));
				
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool csResult = val0 <= val1;
				
				        return object.Equals(fResult, csResult);
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
