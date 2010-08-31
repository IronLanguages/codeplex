#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 2248
			namespace Scenario2248{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add__3", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Add__3() {
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
				            success = check_byte_Add() &
				                check_sbyte_Add() &
				                check_ushort_Add() &
				                check_short_Add() &
				                check_uint_Add() &
				                check_int_Add() &
				                check_ulong_Add() &
				                check_long_Add() &
				                check_float_Add() &
				                check_double_Add() &
				                check_decimal_Add() &
				                check_char_Add();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_Add()
				    {
				        byte[] svals = new byte[] { 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byte_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byte_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_Add(byte val0, byte val1)
				    {
				        try
				        {
				            Expression<Func<byte>> e =
				                Expression.Lambda<Func<byte>>(
				                    Expression.Add(Expression.Constant(val0, typeof(byte)),
				                        Expression.Constant(val1, typeof(byte))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
				    }
				
				    static bool check_sbyte_Add()
				    {
				        sbyte[] svals = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyte_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyte_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_Add(sbyte val0, sbyte val1)
				    {
				        try
				        {
				            Expression<Func<sbyte>> e =
				                Expression.Lambda<Func<sbyte>>(
				                    Expression.Add(Expression.Constant(val0, typeof(sbyte)),
				                        Expression.Constant(val1, typeof(sbyte))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
				    }
				
				    static bool check_ushort_Add()
				    {
				        ushort[] svals = new ushort[] { 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushort_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushort_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_Add(ushort val0, ushort val1)
				    {
				        Expression<Func<ushort>> e =
				            Expression.Lambda<Func<ushort>>(
				                Expression.Add(Expression.Constant(val0, typeof(ushort)),
				                    Expression.Constant(val1, typeof(ushort))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort> f = e.Compile();
				
				        ushort fResult = default(ushort);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ushort csResult = default(ushort);
				        Exception csEx = null;
				        try
				        {
				            csResult = (ushort)(val0 + val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_short_Add()
				    {
				        short[] svals = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_short_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("short_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_Add(short val0, short val1)
				    {
				        Expression<Func<short>> e =
				            Expression.Lambda<Func<short>>(
				                Expression.Add(Expression.Constant(val0, typeof(short)),
				                    Expression.Constant(val1, typeof(short))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short> f = e.Compile();
				
				        short fResult = default(short);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        short csResult = default(short);
				        Exception csEx = null;
				        try
				        {
				            csResult = (short)(val0 + val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_uint_Add()
				    {
				        uint[] svals = new uint[] { 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uint_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uint_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_Add(uint val0, uint val1)
				    {
				        Expression<Func<uint>> e =
				            Expression.Lambda<Func<uint>>(
				                Expression.Add(Expression.Constant(val0, typeof(uint)),
				                    Expression.Constant(val1, typeof(uint))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint> f = e.Compile();
				
				        uint fResult = default(uint);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        uint csResult = default(uint);
				        Exception csEx = null;
				        try
				        {
				            csResult = (uint)(val0 + val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_int_Add()
				    {
				        int[] svals = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_int_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("int_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_Add(int val0, int val1)
				    {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.Add(Expression.Constant(val0, typeof(int)),
				                    Expression.Constant(val1, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				
				        int fResult = default(int);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        int csResult = default(int);
				        Exception csEx = null;
				        try
				        {
				            csResult = (int)(val0 + val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_ulong_Add()
				    {
				        ulong[] svals = new ulong[] { 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulong_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulong_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_Add(ulong val0, ulong val1)
				    {
				        Expression<Func<ulong>> e =
				            Expression.Lambda<Func<ulong>>(
				                Expression.Add(Expression.Constant(val0, typeof(ulong)),
				                    Expression.Constant(val1, typeof(ulong))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong> f = e.Compile();
				
				        ulong fResult = default(ulong);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ulong csResult = default(ulong);
				        Exception csEx = null;
				        try
				        {
				            csResult = (ulong)(val0 + val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_long_Add()
				    {
				        long[] svals = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_long_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("long_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_Add(long val0, long val1)
				    {
				        Expression<Func<long>> e =
				            Expression.Lambda<Func<long>>(
				                Expression.Add(Expression.Constant(val0, typeof(long)),
				                    Expression.Constant(val1, typeof(long))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long> f = e.Compile();
				
				        long fResult = default(long);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        long csResult = default(long);
				        Exception csEx = null;
				        try
				        {
				            csResult = (long)(val0 + val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_float_Add()
				    {
				        float[] svals = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_float_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("float_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_Add(float val0, float val1)
				    {
				        Expression<Func<float>> e =
				            Expression.Lambda<Func<float>>(
				                Expression.Add(Expression.Constant(val0, typeof(float)),
				                    Expression.Constant(val1, typeof(float))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float> f = e.Compile();
				
				        float fResult = default(float);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        float csResult = default(float);
				        Exception csEx = null;
				        try
				        {
				            csResult = (float)(val0 + val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_double_Add()
				    {
				        double[] svals = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_double_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("double_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_Add(double val0, double val1)
				    {
				        Expression<Func<double>> e =
				            Expression.Lambda<Func<double>>(
				                Expression.Add(Expression.Constant(val0, typeof(double)),
				                    Expression.Constant(val1, typeof(double))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double> f = e.Compile();
				
				        double fResult = default(double);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        double csResult = default(double);
				        Exception csEx = null;
				        try
				        {
				            csResult = (double)(val0 + val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_decimal_Add()
				    {
				        decimal[] svals = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimal_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimal_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_Add(decimal val0, decimal val1)
				    {
				        Expression<Func<decimal>> e =
				            Expression.Lambda<Func<decimal>>(
				                Expression.Add(Expression.Constant(val0, typeof(decimal)),
				                    Expression.Constant(val1, typeof(decimal))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal> f = e.Compile();
				
				        decimal fResult = default(decimal);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        decimal csResult = default(decimal);
				        Exception csEx = null;
				        try
				        {
				            csResult = (decimal)(val0 + val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_char_Add()
				    {
				        char[] svals = new char[] { '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_char_Add(svals[i], svals[j]))
				                {
				                    Console.WriteLine("char_Add failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_Add(char val0, char val1)
				    {
				        try
				        {
				            Expression<Func<char>> e =
				                Expression.Lambda<Func<char>>(
				                    Expression.Add(Expression.Constant(val0, typeof(char)),
				                        Expression.Constant(val1, typeof(char))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
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
			
			//-------- Scenario 2249
			namespace Scenario2249{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Subtract__3", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Subtract__3() {
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
				            success = check_byte_Subtract() &
				                check_sbyte_Subtract() &
				                check_ushort_Subtract() &
				                check_short_Subtract() &
				                check_uint_Subtract() &
				                check_int_Subtract() &
				                check_ulong_Subtract() &
				                check_long_Subtract() &
				                check_float_Subtract() &
				                check_double_Subtract() &
				                check_decimal_Subtract() &
				                check_char_Subtract();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_Subtract()
				    {
				        byte[] svals = new byte[] { 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byte_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byte_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_Subtract(byte val0, byte val1)
				    {
				        try
				        {
				            Expression<Func<byte>> e =
				                Expression.Lambda<Func<byte>>(
				                    Expression.Subtract(Expression.Constant(val0, typeof(byte)),
				                        Expression.Constant(val1, typeof(byte))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
				    }
				
				    static bool check_sbyte_Subtract()
				    {
				        sbyte[] svals = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyte_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyte_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_Subtract(sbyte val0, sbyte val1)
				    {
				        try
				        {
				            Expression<Func<sbyte>> e =
				                Expression.Lambda<Func<sbyte>>(
				                    Expression.Subtract(Expression.Constant(val0, typeof(sbyte)),
				                        Expression.Constant(val1, typeof(sbyte))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
				    }
				
				    static bool check_ushort_Subtract()
				    {
				        ushort[] svals = new ushort[] { 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushort_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushort_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_Subtract(ushort val0, ushort val1)
				    {
				        Expression<Func<ushort>> e =
				            Expression.Lambda<Func<ushort>>(
				                Expression.Subtract(Expression.Constant(val0, typeof(ushort)),
				                    Expression.Constant(val1, typeof(ushort))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort> f = e.Compile();
				
				        ushort fResult = default(ushort);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ushort csResult = default(ushort);
				        Exception csEx = null;
				        try
				        {
				            csResult = (ushort)(val0 - val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_short_Subtract()
				    {
				        short[] svals = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_short_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("short_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_Subtract(short val0, short val1)
				    {
				        Expression<Func<short>> e =
				            Expression.Lambda<Func<short>>(
				                Expression.Subtract(Expression.Constant(val0, typeof(short)),
				                    Expression.Constant(val1, typeof(short))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short> f = e.Compile();
				
				        short fResult = default(short);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        short csResult = default(short);
				        Exception csEx = null;
				        try
				        {
				            csResult = (short)(val0 - val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_uint_Subtract()
				    {
				        uint[] svals = new uint[] { 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uint_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uint_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_Subtract(uint val0, uint val1)
				    {
				        Expression<Func<uint>> e =
				            Expression.Lambda<Func<uint>>(
				                Expression.Subtract(Expression.Constant(val0, typeof(uint)),
				                    Expression.Constant(val1, typeof(uint))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint> f = e.Compile();
				
				        uint fResult = default(uint);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        uint csResult = default(uint);
				        Exception csEx = null;
				        try
				        {
				            csResult = (uint)(val0 - val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_int_Subtract()
				    {
				        int[] svals = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_int_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("int_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_Subtract(int val0, int val1)
				    {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.Subtract(Expression.Constant(val0, typeof(int)),
				                    Expression.Constant(val1, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				
				        int fResult = default(int);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        int csResult = default(int);
				        Exception csEx = null;
				        try
				        {
				            csResult = (int)(val0 - val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_ulong_Subtract()
				    {
				        ulong[] svals = new ulong[] { 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulong_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulong_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_Subtract(ulong val0, ulong val1)
				    {
				        Expression<Func<ulong>> e =
				            Expression.Lambda<Func<ulong>>(
				                Expression.Subtract(Expression.Constant(val0, typeof(ulong)),
				                    Expression.Constant(val1, typeof(ulong))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong> f = e.Compile();
				
				        ulong fResult = default(ulong);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ulong csResult = default(ulong);
				        Exception csEx = null;
				        try
				        {
				            csResult = (ulong)(val0 - val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_long_Subtract()
				    {
				        long[] svals = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_long_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("long_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_Subtract(long val0, long val1)
				    {
				        Expression<Func<long>> e =
				            Expression.Lambda<Func<long>>(
				                Expression.Subtract(Expression.Constant(val0, typeof(long)),
				                    Expression.Constant(val1, typeof(long))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long> f = e.Compile();
				
				        long fResult = default(long);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        long csResult = default(long);
				        Exception csEx = null;
				        try
				        {
				            csResult = (long)(val0 - val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_float_Subtract()
				    {
				        float[] svals = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_float_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("float_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_Subtract(float val0, float val1)
				    {
				        Expression<Func<float>> e =
				            Expression.Lambda<Func<float>>(
				                Expression.Subtract(Expression.Constant(val0, typeof(float)),
				                    Expression.Constant(val1, typeof(float))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float> f = e.Compile();
				
				        float fResult = default(float);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        float csResult = default(float);
				        Exception csEx = null;
				        try
				        {
				            csResult = (float)(val0 - val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_double_Subtract()
				    {
				        double[] svals = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_double_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("double_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_Subtract(double val0, double val1)
				    {
				        Expression<Func<double>> e =
				            Expression.Lambda<Func<double>>(
				                Expression.Subtract(Expression.Constant(val0, typeof(double)),
				                    Expression.Constant(val1, typeof(double))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double> f = e.Compile();
				
				        double fResult = default(double);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        double csResult = default(double);
				        Exception csEx = null;
				        try
				        {
				            csResult = (double)(val0 - val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_decimal_Subtract()
				    {
				        decimal[] svals = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimal_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimal_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_Subtract(decimal val0, decimal val1)
				    {
				        Expression<Func<decimal>> e =
				            Expression.Lambda<Func<decimal>>(
				                Expression.Subtract(Expression.Constant(val0, typeof(decimal)),
				                    Expression.Constant(val1, typeof(decimal))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal> f = e.Compile();
				
				        decimal fResult = default(decimal);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        decimal csResult = default(decimal);
				        Exception csEx = null;
				        try
				        {
				            csResult = (decimal)(val0 - val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_char_Subtract()
				    {
				        char[] svals = new char[] { '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_char_Subtract(svals[i], svals[j]))
				                {
				                    Console.WriteLine("char_Subtract failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_Subtract(char val0, char val1)
				    {
				        try
				        {
				            Expression<Func<char>> e =
				                Expression.Lambda<Func<char>>(
				                    Expression.Subtract(Expression.Constant(val0, typeof(char)),
				                        Expression.Constant(val1, typeof(char))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
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
			
			//-------- Scenario 2250
			namespace Scenario2250{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Multiply__3", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Multiply__3() {
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
				            success = check_byte_Multiply() &
				                check_sbyte_Multiply() &
				                check_ushort_Multiply() &
				                check_short_Multiply() &
				                check_uint_Multiply() &
				                check_int_Multiply() &
				                check_ulong_Multiply() &
				                check_long_Multiply() &
				                check_float_Multiply() &
				                check_double_Multiply() &
				                check_decimal_Multiply() &
				                check_char_Multiply();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_Multiply()
				    {
				        byte[] svals = new byte[] { 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byte_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byte_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_Multiply(byte val0, byte val1)
				    {
				        try
				        {
				            Expression<Func<byte>> e =
				                Expression.Lambda<Func<byte>>(
				                    Expression.Multiply(Expression.Constant(val0, typeof(byte)),
				                        Expression.Constant(val1, typeof(byte))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
				    }
				
				    static bool check_sbyte_Multiply()
				    {
				        sbyte[] svals = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyte_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyte_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_Multiply(sbyte val0, sbyte val1)
				    {
				        try
				        {
				            Expression<Func<sbyte>> e =
				                Expression.Lambda<Func<sbyte>>(
				                    Expression.Multiply(Expression.Constant(val0, typeof(sbyte)),
				                        Expression.Constant(val1, typeof(sbyte))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
				    }
				
				    static bool check_ushort_Multiply()
				    {
				        ushort[] svals = new ushort[] { 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushort_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushort_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_Multiply(ushort val0, ushort val1)
				    {
				        Expression<Func<ushort>> e =
				            Expression.Lambda<Func<ushort>>(
				                Expression.Multiply(Expression.Constant(val0, typeof(ushort)),
				                    Expression.Constant(val1, typeof(ushort))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort> f = e.Compile();
				
				        ushort fResult = default(ushort);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ushort csResult = default(ushort);
				        Exception csEx = null;
				        try
				        {
				            csResult = (ushort)(val0 * val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_short_Multiply()
				    {
				        short[] svals = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_short_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("short_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_Multiply(short val0, short val1)
				    {
				        Expression<Func<short>> e =
				            Expression.Lambda<Func<short>>(
				                Expression.Multiply(Expression.Constant(val0, typeof(short)),
				                    Expression.Constant(val1, typeof(short))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short> f = e.Compile();
				
				        short fResult = default(short);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        short csResult = default(short);
				        Exception csEx = null;
				        try
				        {
				            csResult = (short)(val0 * val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_uint_Multiply()
				    {
				        uint[] svals = new uint[] { 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uint_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uint_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_Multiply(uint val0, uint val1)
				    {
				        Expression<Func<uint>> e =
				            Expression.Lambda<Func<uint>>(
				                Expression.Multiply(Expression.Constant(val0, typeof(uint)),
				                    Expression.Constant(val1, typeof(uint))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint> f = e.Compile();
				
				        uint fResult = default(uint);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        uint csResult = default(uint);
				        Exception csEx = null;
				        try
				        {
				            csResult = (uint)(val0 * val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_int_Multiply()
				    {
				        int[] svals = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_int_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("int_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_Multiply(int val0, int val1)
				    {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.Multiply(Expression.Constant(val0, typeof(int)),
				                    Expression.Constant(val1, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				
				        int fResult = default(int);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        int csResult = default(int);
				        Exception csEx = null;
				        try
				        {
				            csResult = (int)(val0 * val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_ulong_Multiply()
				    {
				        ulong[] svals = new ulong[] { 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulong_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulong_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_Multiply(ulong val0, ulong val1)
				    {
				        Expression<Func<ulong>> e =
				            Expression.Lambda<Func<ulong>>(
				                Expression.Multiply(Expression.Constant(val0, typeof(ulong)),
				                    Expression.Constant(val1, typeof(ulong))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong> f = e.Compile();
				
				        ulong fResult = default(ulong);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ulong csResult = default(ulong);
				        Exception csEx = null;
				        try
				        {
				            csResult = (ulong)(val0 * val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_long_Multiply()
				    {
				        long[] svals = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_long_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("long_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_Multiply(long val0, long val1)
				    {
				        Expression<Func<long>> e =
				            Expression.Lambda<Func<long>>(
				                Expression.Multiply(Expression.Constant(val0, typeof(long)),
				                    Expression.Constant(val1, typeof(long))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long> f = e.Compile();
				
				        long fResult = default(long);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        long csResult = default(long);
				        Exception csEx = null;
				        try
				        {
				            csResult = (long)(val0 * val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_float_Multiply()
				    {
				        float[] svals = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_float_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("float_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_Multiply(float val0, float val1)
				    {
				        Expression<Func<float>> e =
				            Expression.Lambda<Func<float>>(
				                Expression.Multiply(Expression.Constant(val0, typeof(float)),
				                    Expression.Constant(val1, typeof(float))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float> f = e.Compile();
				
				        float fResult = default(float);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        float csResult = default(float);
				        Exception csEx = null;
				        try
				        {
				            csResult = (float)(val0 * val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_double_Multiply()
				    {
				        double[] svals = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_double_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("double_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_Multiply(double val0, double val1)
				    {
				        Expression<Func<double>> e =
				            Expression.Lambda<Func<double>>(
				                Expression.Multiply(Expression.Constant(val0, typeof(double)),
				                    Expression.Constant(val1, typeof(double))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double> f = e.Compile();
				
				        double fResult = default(double);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        double csResult = default(double);
				        Exception csEx = null;
				        try
				        {
				            csResult = (double)(val0 * val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_decimal_Multiply()
				    {
				        decimal[] svals = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimal_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimal_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_Multiply(decimal val0, decimal val1)
				    {
				        Expression<Func<decimal>> e =
				            Expression.Lambda<Func<decimal>>(
				                Expression.Multiply(Expression.Constant(val0, typeof(decimal)),
				                    Expression.Constant(val1, typeof(decimal))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal> f = e.Compile();
				
				        decimal fResult = default(decimal);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        decimal csResult = default(decimal);
				        Exception csEx = null;
				        try
				        {
				            csResult = (decimal)(val0 * val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_char_Multiply()
				    {
				        char[] svals = new char[] { '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_char_Multiply(svals[i], svals[j]))
				                {
				                    Console.WriteLine("char_Multiply failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_Multiply(char val0, char val1)
				    {
				        try
				        {
				            Expression<Func<char>> e =
				                Expression.Lambda<Func<char>>(
				                    Expression.Multiply(Expression.Constant(val0, typeof(char)),
				                        Expression.Constant(val1, typeof(char))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
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
			
			//-------- Scenario 2251
			namespace Scenario2251{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Divide__3", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Divide__3() {
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
				            success = check_byte_Divide() &
				                check_sbyte_Divide() &
				                check_ushort_Divide() &
				                check_short_Divide() &
				                check_uint_Divide() &
				                check_int_Divide() &
				                check_ulong_Divide() &
				                check_long_Divide() &
				                check_float_Divide() &
				                check_double_Divide() &
				                check_decimal_Divide() &
				                check_char_Divide();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_Divide()
				    {
				        byte[] svals = new byte[] { 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byte_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byte_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_Divide(byte val0, byte val1)
				    {
				        try
				        {
				            Expression<Func<byte>> e =
				                Expression.Lambda<Func<byte>>(
				                    Expression.Divide(Expression.Constant(val0, typeof(byte)),
				                        Expression.Constant(val1, typeof(byte))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
				    }
				
				    static bool check_sbyte_Divide()
				    {
				        sbyte[] svals = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyte_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyte_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_Divide(sbyte val0, sbyte val1)
				    {
				        try
				        {
				            Expression<Func<sbyte>> e =
				                Expression.Lambda<Func<sbyte>>(
				                    Expression.Divide(Expression.Constant(val0, typeof(sbyte)),
				                        Expression.Constant(val1, typeof(sbyte))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
				    }
				
				    static bool check_ushort_Divide()
				    {
				        ushort[] svals = new ushort[] { 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushort_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushort_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_Divide(ushort val0, ushort val1)
				    {
				        Expression<Func<ushort>> e =
				            Expression.Lambda<Func<ushort>>(
				                Expression.Divide(Expression.Constant(val0, typeof(ushort)),
				                    Expression.Constant(val1, typeof(ushort))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort> f = e.Compile();
				
				        ushort fResult = default(ushort);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ushort csResult = default(ushort);
				        Exception csEx = null;
				        try
				        {
				            csResult = (ushort)(val0 / val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_short_Divide()
				    {
				        short[] svals = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_short_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("short_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_Divide(short val0, short val1)
				    {
				        Expression<Func<short>> e =
				            Expression.Lambda<Func<short>>(
				                Expression.Divide(Expression.Constant(val0, typeof(short)),
				                    Expression.Constant(val1, typeof(short))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short> f = e.Compile();
				
				        short fResult = default(short);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        short csResult = default(short);
				        Exception csEx = null;
				        try
				        {
				            csResult = (short)(val0 / val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_uint_Divide()
				    {
				        uint[] svals = new uint[] { 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uint_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uint_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_Divide(uint val0, uint val1)
				    {
				        Expression<Func<uint>> e =
				            Expression.Lambda<Func<uint>>(
				                Expression.Divide(Expression.Constant(val0, typeof(uint)),
				                    Expression.Constant(val1, typeof(uint))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint> f = e.Compile();
				
				        uint fResult = default(uint);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        uint csResult = default(uint);
				        Exception csEx = null;
				        try
				        {
				            csResult = (uint)(val0 / val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_int_Divide()
				    {
				        int[] svals = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_int_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("int_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_Divide(int val0, int val1)
				    {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.Divide(Expression.Constant(val0, typeof(int)),
				                    Expression.Constant(val1, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				
				        int fResult = default(int);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        int csResult = default(int);
				        Exception csEx = null;
				        try
				        {
				            csResult = (int)(val0 / val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_ulong_Divide()
				    {
				        ulong[] svals = new ulong[] { 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulong_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulong_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_Divide(ulong val0, ulong val1)
				    {
				        Expression<Func<ulong>> e =
				            Expression.Lambda<Func<ulong>>(
				                Expression.Divide(Expression.Constant(val0, typeof(ulong)),
				                    Expression.Constant(val1, typeof(ulong))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong> f = e.Compile();
				
				        ulong fResult = default(ulong);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ulong csResult = default(ulong);
				        Exception csEx = null;
				        try
				        {
				            csResult = (ulong)(val0 / val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_long_Divide()
				    {
				        long[] svals = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_long_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("long_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_Divide(long val0, long val1)
				    {
				        Expression<Func<long>> e =
				            Expression.Lambda<Func<long>>(
				                Expression.Divide(Expression.Constant(val0, typeof(long)),
				                    Expression.Constant(val1, typeof(long))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long> f = e.Compile();
				
				        long fResult = default(long);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        long csResult = default(long);
				        Exception csEx = null;
				        try
				        {
				            csResult = (long)(val0 / val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_float_Divide()
				    {
				        float[] svals = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_float_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("float_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_Divide(float val0, float val1)
				    {
				        Expression<Func<float>> e =
				            Expression.Lambda<Func<float>>(
				                Expression.Divide(Expression.Constant(val0, typeof(float)),
				                    Expression.Constant(val1, typeof(float))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float> f = e.Compile();
				
				        float fResult = default(float);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        float csResult = default(float);
				        Exception csEx = null;
				        try
				        {
				            csResult = (float)(val0 / val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_double_Divide()
				    {
				        double[] svals = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_double_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("double_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_Divide(double val0, double val1)
				    {
				        Expression<Func<double>> e =
				            Expression.Lambda<Func<double>>(
				                Expression.Divide(Expression.Constant(val0, typeof(double)),
				                    Expression.Constant(val1, typeof(double))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double> f = e.Compile();
				
				        double fResult = default(double);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        double csResult = default(double);
				        Exception csEx = null;
				        try
				        {
				            csResult = (double)(val0 / val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_decimal_Divide()
				    {
				        decimal[] svals = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimal_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimal_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_Divide(decimal val0, decimal val1)
				    {
				        Expression<Func<decimal>> e =
				            Expression.Lambda<Func<decimal>>(
				                Expression.Divide(Expression.Constant(val0, typeof(decimal)),
				                    Expression.Constant(val1, typeof(decimal))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal> f = e.Compile();
				
				        decimal fResult = default(decimal);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        decimal csResult = default(decimal);
				        Exception csEx = null;
				        try
				        {
				            csResult = (decimal)(val0 / val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_char_Divide()
				    {
				        char[] svals = new char[] { '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_char_Divide(svals[i], svals[j]))
				                {
				                    Console.WriteLine("char_Divide failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_Divide(char val0, char val1)
				    {
				        try
				        {
				            Expression<Func<char>> e =
				                Expression.Lambda<Func<char>>(
				                    Expression.Divide(Expression.Constant(val0, typeof(char)),
				                        Expression.Constant(val1, typeof(char))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
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
			
			//-------- Scenario 2252
			namespace Scenario2252{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Modulo__3", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Modulo__3() {
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
				            success = check_byte_Modulo() &
				                check_sbyte_Modulo() &
				                check_ushort_Modulo() &
				                check_short_Modulo() &
				                check_uint_Modulo() &
				                check_int_Modulo() &
				                check_ulong_Modulo() &
				                check_long_Modulo() &
				                check_float_Modulo() &
				                check_double_Modulo() &
				                check_decimal_Modulo() &
				                check_char_Modulo();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_Modulo()
				    {
				        byte[] svals = new byte[] { 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byte_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byte_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_Modulo(byte val0, byte val1)
				    {
				        try
				        {
				            Expression<Func<byte>> e =
				                Expression.Lambda<Func<byte>>(
				                    Expression.Modulo(Expression.Constant(val0, typeof(byte)),
				                        Expression.Constant(val1, typeof(byte))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
				    }
				
				    static bool check_sbyte_Modulo()
				    {
				        sbyte[] svals = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyte_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyte_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_Modulo(sbyte val0, sbyte val1)
				    {
				        try
				        {
				            Expression<Func<sbyte>> e =
				                Expression.Lambda<Func<sbyte>>(
				                    Expression.Modulo(Expression.Constant(val0, typeof(sbyte)),
				                        Expression.Constant(val1, typeof(sbyte))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
				    }
				
				    static bool check_ushort_Modulo()
				    {
				        ushort[] svals = new ushort[] { 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushort_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushort_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_Modulo(ushort val0, ushort val1)
				    {
				        Expression<Func<ushort>> e =
				            Expression.Lambda<Func<ushort>>(
				                Expression.Modulo(Expression.Constant(val0, typeof(ushort)),
				                    Expression.Constant(val1, typeof(ushort))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort> f = e.Compile();
				
				        ushort fResult = default(ushort);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ushort csResult = default(ushort);
				        Exception csEx = null;
				        try
				        {
				            csResult = (ushort)(val0 % val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_short_Modulo()
				    {
				        short[] svals = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_short_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("short_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_Modulo(short val0, short val1)
				    {
				        Expression<Func<short>> e =
				            Expression.Lambda<Func<short>>(
				                Expression.Modulo(Expression.Constant(val0, typeof(short)),
				                    Expression.Constant(val1, typeof(short))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short> f = e.Compile();
				
				        short fResult = default(short);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        short csResult = default(short);
				        Exception csEx = null;
				        try
				        {
				            csResult = (short)(val0 % val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_uint_Modulo()
				    {
				        uint[] svals = new uint[] { 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uint_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uint_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_Modulo(uint val0, uint val1)
				    {
				        Expression<Func<uint>> e =
				            Expression.Lambda<Func<uint>>(
				                Expression.Modulo(Expression.Constant(val0, typeof(uint)),
				                    Expression.Constant(val1, typeof(uint))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint> f = e.Compile();
				
				        uint fResult = default(uint);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        uint csResult = default(uint);
				        Exception csEx = null;
				        try
				        {
				            csResult = (uint)(val0 % val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_int_Modulo()
				    {
				        int[] svals = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_int_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("int_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_Modulo(int val0, int val1)
				    {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.Modulo(Expression.Constant(val0, typeof(int)),
				                    Expression.Constant(val1, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				
				        int fResult = default(int);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        int csResult = default(int);
				        Exception csEx = null;
				        try
				        {
				            csResult = (int)(val0 % val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_ulong_Modulo()
				    {
				        ulong[] svals = new ulong[] { 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulong_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulong_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_Modulo(ulong val0, ulong val1)
				    {
				        Expression<Func<ulong>> e =
				            Expression.Lambda<Func<ulong>>(
				                Expression.Modulo(Expression.Constant(val0, typeof(ulong)),
				                    Expression.Constant(val1, typeof(ulong))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong> f = e.Compile();
				
				        ulong fResult = default(ulong);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        ulong csResult = default(ulong);
				        Exception csEx = null;
				        try
				        {
				            csResult = (ulong)(val0 % val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_long_Modulo()
				    {
				        long[] svals = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_long_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("long_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_Modulo(long val0, long val1)
				    {
				        Expression<Func<long>> e =
				            Expression.Lambda<Func<long>>(
				                Expression.Modulo(Expression.Constant(val0, typeof(long)),
				                    Expression.Constant(val1, typeof(long))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long> f = e.Compile();
				
				        long fResult = default(long);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        long csResult = default(long);
				        Exception csEx = null;
				        try
				        {
				            csResult = (long)(val0 % val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_float_Modulo()
				    {
				        float[] svals = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_float_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("float_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_Modulo(float val0, float val1)
				    {
				        Expression<Func<float>> e =
				            Expression.Lambda<Func<float>>(
				                Expression.Modulo(Expression.Constant(val0, typeof(float)),
				                    Expression.Constant(val1, typeof(float))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float> f = e.Compile();
				
				        float fResult = default(float);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        float csResult = default(float);
				        Exception csEx = null;
				        try
				        {
				            csResult = (float)(val0 % val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_double_Modulo()
				    {
				        double[] svals = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_double_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("double_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_Modulo(double val0, double val1)
				    {
				        Expression<Func<double>> e =
				            Expression.Lambda<Func<double>>(
				                Expression.Modulo(Expression.Constant(val0, typeof(double)),
				                    Expression.Constant(val1, typeof(double))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double> f = e.Compile();
				
				        double fResult = default(double);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        double csResult = default(double);
				        Exception csEx = null;
				        try
				        {
				            csResult = (double)(val0 % val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_decimal_Modulo()
				    {
				        decimal[] svals = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimal_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimal_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_Modulo(decimal val0, decimal val1)
				    {
				        Expression<Func<decimal>> e =
				            Expression.Lambda<Func<decimal>>(
				                Expression.Modulo(Expression.Constant(val0, typeof(decimal)),
				                    Expression.Constant(val1, typeof(decimal))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal> f = e.Compile();
				
				        decimal fResult = default(decimal);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				        }
				
				        decimal csResult = default(decimal);
				        Exception csEx = null;
				        try
				        {
				            csResult = (decimal)(val0 % val1);
				        }
				        catch (Exception ex)
				        {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null)
				        {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else
				        {
				            return object.Equals(fResult, csResult);
				        }
				    }
				
				    static bool check_char_Modulo()
				    {
				        char[] svals = new char[] { '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_char_Modulo(svals[i], svals[j]))
				                {
				                    Console.WriteLine("char_Modulo failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_Modulo(char val0, char val1)
				    {
				        try
				        {
				            Expression<Func<char>> e =
				                Expression.Lambda<Func<char>>(
				                    Expression.Modulo(Expression.Constant(val0, typeof(char)),
				                        Expression.Constant(val1, typeof(char))),
				                    new System.Collections.Generic.List<ParameterExpression>());
				        }
				        catch (System.InvalidOperationException) { return true; }
				        return false;
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
