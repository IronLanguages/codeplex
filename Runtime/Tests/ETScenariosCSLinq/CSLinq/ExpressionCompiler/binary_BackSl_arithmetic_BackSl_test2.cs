#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 2258
			namespace Scenario2258{
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Power__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Power__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        bool succeed =
				                check_byteq_Power()
				                && check_sbyteq_Power()
				                && check_ushortq_Power()
				                && check_shortq_Power()
				                && check_uintq_Power()
				                && check_intq_Power()
				                && check_ulongq_Power()
				                && check_longq_Power()
				                && check_floatq_Power()
				                && check_doubleq_Power()
				                && check_decimalq_Power()
				                && check_charq_Power();
				
				        return succeed ? 0 : 1;
				    }
				
				    static bool check_byteq_Power()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_Power(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_Power failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static byte Power_byte(byte val0, byte val1)
				    {
				        return (byte)Math.Pow((double)val0, (double)val1);
				    }
				
				    static bool check_byteq_Power(byte? val0, byte? val1)
				    {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.Power(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    typeof(Test).GetMethod("Power_byte")
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
				                if (val0 == null || val1 == null)
				                {
				                    csResult = null;
				                }
				                else
				                {
				                    csResult = (byte?)Math.Pow((double)val0, (double)val1);
				                }
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_Power()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_Power(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_Power failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static sbyte Power_sbyte(sbyte val0, sbyte val1)
				    {
				        return (sbyte)Math.Pow((double)val0, (double)val1);
				    }
				
				    static bool check_sbyteq_Power(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.Power(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    typeof(Test).GetMethod("Power_sbyte")
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
				                if (val0 == null || val1 == null)
				                {
				                    csResult = null;
				                }
				                else
				                {
				                    csResult = (sbyte?)Math.Pow((double)val0, (double)val1);
				                }
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_Power()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_Power(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_Power failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ushort Power_ushort(ushort val0, ushort val1)
				    {
				        return (ushort)Math.Pow((double)val0, (double)val1);
				    }
				
				    static bool check_ushortq_Power(ushort? val0, ushort? val1)
				    {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.Power(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    typeof(Test).GetMethod("Power_ushort")
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
				                if (val0 == null || val1 == null)
				                {
				                    csResult = null;
				                }
				                else
				                {
				                    csResult = (ushort?)Math.Pow((double)val0, (double)val1);
				                }
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_Power()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_Power(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_Power failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static short Power_short(short val0, short val1)
				    {
				        return (short)Math.Pow((double)val0, (double)val1);
				    }
				
				    static bool check_shortq_Power(short? val0, short? val1)
				    {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.Power(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    typeof(Test).GetMethod("Power_short")
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
				                if (val0 == null || val1 == null)
				                {
				                    csResult = null;
				                }
				                else
				                {
				                    csResult = (short?)Math.Pow((double)val0, (double)val1);
				                }
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_Power()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_Power(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_Power failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static uint Power_uint(uint val0, uint val1)
				    {
				        return (uint)Math.Pow((double)val0, (double)val1);
				    }
				
				    static bool check_uintq_Power(uint? val0, uint? val1)
				    {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.Power(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    typeof(Test).GetMethod("Power_uint")
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
				                if (val0 == null || val1 == null)
				                {
				                    csResult = null;
				                }
				                else
				                {
				                    csResult = (uint?)Math.Pow((double)val0, (double)val1);
				                }
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_Power()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_Power(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_Power failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static int Power_int(int val0, int val1)
				    {
				        return (int)Math.Pow((double)val0, (double)val1);
				    }
				
				    static bool check_intq_Power(int? val0, int? val1)
				    {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.Power(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    typeof(Test).GetMethod("Power_int")
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
				                if (val0 == null || val1 == null)
				                {
				                    csResult = null;
				                }
				                else
				                {
				                    csResult = (int?)Math.Pow((double)val0, (double)val1);
				                }
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_Power()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_Power(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_Power failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ulong Power_ulong(ulong val0, ulong val1)
				    {
				        return (ulong)Math.Pow((double)val0, (double)val1);
				    }
				
				    static bool check_ulongq_Power(ulong? val0, ulong? val1)
				    {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.Power(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    typeof(Test).GetMethod("Power_ulong")
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
				                if (val0 == null || val1 == null)
				                {
				                    csResult = null;
				                }
				                else
				                {
				                    csResult = (ulong?)Math.Pow((double)val0, (double)val1);
				                }
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_Power()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_Power(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_Power failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static long Power_long(long val0, long val1)
				    {
				        return (long)Math.Pow((double)val0, (double)val1);
				    }
				
				    static bool check_longq_Power(long? val0, long? val1)
				    {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.Power(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    typeof(Test).GetMethod("Power_long")
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
				                if (val0 == null || val1 == null)
				                {
				                    csResult = null;
				                }
				                else
				                {
				                    csResult = (long?)Math.Pow((double)val0, (double)val1);
				                }
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_Power()
				    {
				        float?[] svals = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_Power(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_Power failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static float Power_float(float val0, float val1)
				    {
				        return (float)Math.Pow((double)val0, (double)val1);
				    }
				
				    static bool check_floatq_Power(float? val0, float? val1)
				    {
				        Expression<Func<float?>> e =
				            Expression.Lambda<Func<float?>>(
				                Expression.Power(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    typeof(Test).GetMethod("Power_float")
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
				                if (val0 == null || val1 == null)
				                {
				                    csResult = null;
				                }
				                else
				                {
				                    csResult = (float?)Math.Pow((double)val0, (double)val1);
				                }
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_Power()
				    {
				        double?[] svals = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_Power(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_Power failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static double Power_double(double val0, double val1)
				    {
				        return (double)Math.Pow((double)val0, (double)val1);
				    }
				
				    static bool check_doubleq_Power(double? val0, double? val1)
				    {
				        Expression<Func<double?>> e =
				            Expression.Lambda<Func<double?>>(
				                Expression.Power(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    typeof(Test).GetMethod("Power_double")
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
				                if (val0 == null || val1 == null)
				                {
				                    csResult = null;
				                }
				                else
				                {
				                    csResult = (double?)Math.Pow((double)val0, (double)val1);
				                }
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_Power()
				    {
				        decimal?[] svals = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_Power(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_Power failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static decimal Power_decimal(decimal val0, decimal val1)
				    {
				        return (decimal)Math.Pow((double)val0, (double)val1);
				    }
				
				    static bool check_decimalq_Power(decimal? val0, decimal? val1)
				    {
				        Expression<Func<decimal?>> e =
				            Expression.Lambda<Func<decimal?>>(
				                Expression.Power(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    typeof(Test).GetMethod("Power_decimal")
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
				                if (val0 == null || val1 == null)
				                {
				                    csResult = null;
				                }
				                else
				                {
				                    csResult = (decimal?)Math.Pow((double)val0, (double)val1);
				                }
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_charq_Power()
				    {
				        char?[] svals = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_charq_Power(svals[i], svals[j]))
				                {
				                    Console.WriteLine("charq_Power failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static char Power_char(char val0, char val1)
				    {
				        return (char)Math.Pow((double)val0, (double)val1);
				    }
				
				    static bool check_charq_Power(char? val0, char? val1)
				    {
				        Expression<Func<char?>> e =
				            Expression.Lambda<Func<char?>>(
				                Expression.Power(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char?)),
				                    typeof(Test).GetMethod("Power_char")
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
				                if (val0 == null || val1 == null)
				                {
				                    csResult = null;
				                }
				                else
				                {
				                    csResult = (char?)Math.Pow((double)val0, (double)val1);
				                }
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
