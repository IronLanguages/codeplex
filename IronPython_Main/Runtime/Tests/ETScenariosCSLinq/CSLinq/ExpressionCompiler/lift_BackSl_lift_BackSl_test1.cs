#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 264
			namespace Scenario264{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AddChecked__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression AddChecked__() {
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
				            success = check_uintq_AddChecked() &
				                check_intq_AddChecked() &
				                check_ulongq_AddChecked() &
				                check_longq_AddChecked() &
				                check_byteq_AddChecked() &
				                check_sbyteq_AddChecked() &
				                check_shortq_AddChecked() &
				                check_ushortq_AddChecked() &
				                check_floatq_AddChecked() &
				                check_doubleq_AddChecked() &
				                check_decimalq_AddChecked();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_AddChecked()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_AddChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_AddChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static uint AddChecked_uint(uint val0, uint val1)
				    {
				        return (uint)checked(val0 + val1);
				    }
				
				    static bool check_uintq_AddChecked(uint? val0, uint? val1)
				    {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.AddChecked(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    typeof(Test).GetMethod("AddChecked_uint")
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
				                csResult = (uint?)checked(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_AddChecked()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_AddChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_AddChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static int AddChecked_int(int val0, int val1)
				    {
				        return (int)checked(val0 + val1);
				    }
				
				    static bool check_intq_AddChecked(int? val0, int? val1)
				    {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.AddChecked(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    typeof(Test).GetMethod("AddChecked_int")
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
				                csResult = (int?)checked(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_AddChecked()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_AddChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_AddChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ulong AddChecked_ulong(ulong val0, ulong val1)
				    {
				        return (ulong)checked(val0 + val1);
				    }
				
				    static bool check_ulongq_AddChecked(ulong? val0, ulong? val1)
				    {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.AddChecked(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    typeof(Test).GetMethod("AddChecked_ulong")
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
				                csResult = (ulong?)checked(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_AddChecked()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_AddChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_AddChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static long AddChecked_long(long val0, long val1)
				    {
				        return (long)checked(val0 + val1);
				    }
				
				    static bool check_longq_AddChecked(long? val0, long? val1)
				    {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.AddChecked(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    typeof(Test).GetMethod("AddChecked_long")
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
				                csResult = (long?)checked(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_byteq_AddChecked()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_AddChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_AddChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static byte AddChecked_byte(byte val0, byte val1)
				    {
				        return (byte)checked(val0 + val1);
				    }
				
				    static bool check_byteq_AddChecked(byte? val0, byte? val1)
				    {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.AddChecked(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    typeof(Test).GetMethod("AddChecked_byte")
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
				                csResult = (byte?)checked(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_AddChecked()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_AddChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_AddChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static sbyte AddChecked_sbyte(sbyte val0, sbyte val1)
				    {
				        return (sbyte)checked(val0 + val1);
				    }
				
				    static bool check_sbyteq_AddChecked(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.AddChecked(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    typeof(Test).GetMethod("AddChecked_sbyte")
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
				                csResult = (sbyte?)checked(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_AddChecked()
				    {
				        short?[] svals = new short?[] { null, 0, 1, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_AddChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_AddChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static short AddChecked_short(short val0, short val1)
				    {
				        return (short)checked(val0 + val1);
				    }
				
				    static bool check_shortq_AddChecked(short? val0, short? val1)
				    {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.AddChecked(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    typeof(Test).GetMethod("AddChecked_short")
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
				                csResult = (short?)checked(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_AddChecked()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_AddChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_AddChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ushort AddChecked_ushort(ushort val0, ushort val1)
				    {
				        return (ushort)checked(val0 + val1);
				    }
				
				    static bool check_ushortq_AddChecked(ushort? val0, ushort? val1)
				    {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.AddChecked(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    typeof(Test).GetMethod("AddChecked_ushort")
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
				                csResult = (ushort?)checked(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_AddChecked()
				    {
				        float?[] svals = new float?[] { null, 0, 1, float.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_AddChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_AddChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static float AddChecked_float(float val0, float val1)
				    {
				        return (float)checked(val0 + val1);
				    }
				
				    static bool check_floatq_AddChecked(float? val0, float? val1)
				    {
				        Expression<Func<float?>> e =
				            Expression.Lambda<Func<float?>>(
				                Expression.AddChecked(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    typeof(Test).GetMethod("AddChecked_float")
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
				                csResult = (float?)checked(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_AddChecked()
				    {
				        double?[] svals = new double?[] { null, 0, 1, double.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_AddChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_AddChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static double AddChecked_double(double val0, double val1)
				    {
				        return (double)checked(val0 + val1);
				    }
				
				    static bool check_doubleq_AddChecked(double? val0, double? val1)
				    {
				        Expression<Func<double?>> e =
				            Expression.Lambda<Func<double?>>(
				                Expression.AddChecked(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    typeof(Test).GetMethod("AddChecked_double")
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
				                csResult = (double?)checked(val0 + val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_AddChecked()
				    {
				        decimal?[] svals = new decimal?[] { null, 0, 1, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_AddChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_AddChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static decimal AddChecked_decimal(decimal val0, decimal val1)
				    {
				        return (decimal)checked(val0 + val1);
				    }
				
				    static bool check_decimalq_AddChecked(decimal? val0, decimal? val1)
				    {
				        Expression<Func<decimal?>> e =
				            Expression.Lambda<Func<decimal?>>(
				                Expression.AddChecked(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    typeof(Test).GetMethod("AddChecked_decimal")
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
				                csResult = (decimal?)checked(val0 + val1);
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
			
			//-------- Scenario 265
			namespace Scenario265{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SubtractChecked__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression SubtractChecked__() {
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
				            success = check_uintq_SubtractChecked() &
				                check_intq_SubtractChecked() &
				                check_ulongq_SubtractChecked() &
				                check_longq_SubtractChecked() &
				                check_byteq_SubtractChecked() &
				                check_sbyteq_SubtractChecked() &
				                check_shortq_SubtractChecked() &
				                check_ushortq_SubtractChecked() &
				                check_floatq_SubtractChecked() &
				                check_doubleq_SubtractChecked() &
				                check_decimalq_SubtractChecked();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_SubtractChecked()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_SubtractChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_SubtractChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static uint SubtractChecked_uint(uint val0, uint val1)
				    {
				        return (uint)checked(val0 - val1);
				    }
				
				    static bool check_uintq_SubtractChecked(uint? val0, uint? val1)
				    {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.SubtractChecked(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    typeof(Test).GetMethod("SubtractChecked_uint")
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
				                csResult = (uint?)checked(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_SubtractChecked()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_SubtractChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_SubtractChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static int SubtractChecked_int(int val0, int val1)
				    {
				        return (int)checked(val0 - val1);
				    }
				
				    static bool check_intq_SubtractChecked(int? val0, int? val1)
				    {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.SubtractChecked(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    typeof(Test).GetMethod("SubtractChecked_int")
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
				                csResult = (int?)checked(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_SubtractChecked()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_SubtractChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_SubtractChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ulong SubtractChecked_ulong(ulong val0, ulong val1)
				    {
				        return (ulong)checked(val0 - val1);
				    }
				
				    static bool check_ulongq_SubtractChecked(ulong? val0, ulong? val1)
				    {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.SubtractChecked(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    typeof(Test).GetMethod("SubtractChecked_ulong")
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
				                csResult = (ulong?)checked(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_SubtractChecked()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_SubtractChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_SubtractChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static long SubtractChecked_long(long val0, long val1)
				    {
				        return (long)checked(val0 - val1);
				    }
				
				    static bool check_longq_SubtractChecked(long? val0, long? val1)
				    {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.SubtractChecked(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    typeof(Test).GetMethod("SubtractChecked_long")
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
				                csResult = (long?)checked(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_byteq_SubtractChecked()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_SubtractChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_SubtractChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static byte SubtractChecked_byte(byte val0, byte val1)
				    {
				        return (byte)checked(val0 - val1);
				    }
				
				    static bool check_byteq_SubtractChecked(byte? val0, byte? val1)
				    {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.SubtractChecked(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    typeof(Test).GetMethod("SubtractChecked_byte")
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
				                csResult = (byte?)checked(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_SubtractChecked()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_SubtractChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_SubtractChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static sbyte SubtractChecked_sbyte(sbyte val0, sbyte val1)
				    {
				        return (sbyte)checked(val0 - val1);
				    }
				
				    static bool check_sbyteq_SubtractChecked(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.SubtractChecked(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    typeof(Test).GetMethod("SubtractChecked_sbyte")
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
				                csResult = (sbyte?)checked(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_SubtractChecked()
				    {
				        short?[] svals = new short?[] { null, 0, 1, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_SubtractChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_SubtractChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static short SubtractChecked_short(short val0, short val1)
				    {
				        return (short)checked(val0 - val1);
				    }
				
				    static bool check_shortq_SubtractChecked(short? val0, short? val1)
				    {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.SubtractChecked(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    typeof(Test).GetMethod("SubtractChecked_short")
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
				                csResult = (short?)checked(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_SubtractChecked()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_SubtractChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_SubtractChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ushort SubtractChecked_ushort(ushort val0, ushort val1)
				    {
				        return (ushort)checked(val0 - val1);
				    }
				
				    static bool check_ushortq_SubtractChecked(ushort? val0, ushort? val1)
				    {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.SubtractChecked(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    typeof(Test).GetMethod("SubtractChecked_ushort")
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
				                csResult = (ushort?)checked(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_SubtractChecked()
				    {
				        float?[] svals = new float?[] { null, 0, 1, float.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_SubtractChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_SubtractChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static float SubtractChecked_float(float val0, float val1)
				    {
				        return (float)checked(val0 - val1);
				    }
				
				    static bool check_floatq_SubtractChecked(float? val0, float? val1)
				    {
				        Expression<Func<float?>> e =
				            Expression.Lambda<Func<float?>>(
				                Expression.SubtractChecked(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    typeof(Test).GetMethod("SubtractChecked_float")
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
				                csResult = (float?)checked(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_SubtractChecked()
				    {
				        double?[] svals = new double?[] { null, 0, 1, double.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_SubtractChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_SubtractChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static double SubtractChecked_double(double val0, double val1)
				    {
				        return (double)checked(val0 - val1);
				    }
				
				    static bool check_doubleq_SubtractChecked(double? val0, double? val1)
				    {
				        Expression<Func<double?>> e =
				            Expression.Lambda<Func<double?>>(
				                Expression.SubtractChecked(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    typeof(Test).GetMethod("SubtractChecked_double")
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
				                csResult = (double?)checked(val0 - val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_SubtractChecked()
				    {
				        decimal?[] svals = new decimal?[] { null, 0, 1, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_SubtractChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_SubtractChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static decimal SubtractChecked_decimal(decimal val0, decimal val1)
				    {
				        return (decimal)checked(val0 - val1);
				    }
				
				    static bool check_decimalq_SubtractChecked(decimal? val0, decimal? val1)
				    {
				        Expression<Func<decimal?>> e =
				            Expression.Lambda<Func<decimal?>>(
				                Expression.SubtractChecked(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    typeof(Test).GetMethod("SubtractChecked_decimal")
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
				                csResult = (decimal?)checked(val0 - val1);
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
			
			//-------- Scenario 266
			namespace Scenario266{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MultiplyChecked__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression MultiplyChecked__() {
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
				            success = check_uintq_MultiplyChecked() &
				                check_intq_MultiplyChecked() &
				                check_ulongq_MultiplyChecked() &
				                check_longq_MultiplyChecked() &
				                check_byteq_MultiplyChecked() &
				                check_sbyteq_MultiplyChecked() &
				                check_shortq_MultiplyChecked() &
				                check_ushortq_MultiplyChecked() &
				                check_floatq_MultiplyChecked() &
				                check_doubleq_MultiplyChecked() &
				                check_decimalq_MultiplyChecked();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_MultiplyChecked()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_MultiplyChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_MultiplyChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static uint MultiplyChecked_uint(uint val0, uint val1)
				    {
				        return (uint)checked(val0 * val1);
				    }
				
				    static bool check_uintq_MultiplyChecked(uint? val0, uint? val1)
				    {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.MultiplyChecked(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    typeof(Test).GetMethod("MultiplyChecked_uint")
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
				                csResult = (uint?)checked(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_MultiplyChecked()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_MultiplyChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_MultiplyChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static int MultiplyChecked_int(int val0, int val1)
				    {
				        return (int)checked(val0 * val1);
				    }
				
				    static bool check_intq_MultiplyChecked(int? val0, int? val1)
				    {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.MultiplyChecked(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    typeof(Test).GetMethod("MultiplyChecked_int")
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
				                csResult = (int?)checked(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_MultiplyChecked()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_MultiplyChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_MultiplyChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ulong MultiplyChecked_ulong(ulong val0, ulong val1)
				    {
				        return (ulong)checked(val0 * val1);
				    }
				
				    static bool check_ulongq_MultiplyChecked(ulong? val0, ulong? val1)
				    {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.MultiplyChecked(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    typeof(Test).GetMethod("MultiplyChecked_ulong")
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
				                csResult = (ulong?)checked(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_MultiplyChecked()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_MultiplyChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_MultiplyChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static long MultiplyChecked_long(long val0, long val1)
				    {
				        return (long)checked(val0 * val1);
				    }
				
				    static bool check_longq_MultiplyChecked(long? val0, long? val1)
				    {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.MultiplyChecked(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    typeof(Test).GetMethod("MultiplyChecked_long")
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
				                csResult = (long?)checked(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_byteq_MultiplyChecked()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_MultiplyChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_MultiplyChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static byte MultiplyChecked_byte(byte val0, byte val1)
				    {
				        return (byte)checked(val0 * val1);
				    }
				
				    static bool check_byteq_MultiplyChecked(byte? val0, byte? val1)
				    {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.MultiplyChecked(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    typeof(Test).GetMethod("MultiplyChecked_byte")
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
				                csResult = (byte?)checked(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_MultiplyChecked()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_MultiplyChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_MultiplyChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static sbyte MultiplyChecked_sbyte(sbyte val0, sbyte val1)
				    {
				        return (sbyte)checked(val0 * val1);
				    }
				
				    static bool check_sbyteq_MultiplyChecked(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.MultiplyChecked(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    typeof(Test).GetMethod("MultiplyChecked_sbyte")
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
				                csResult = (sbyte?)checked(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_MultiplyChecked()
				    {
				        short?[] svals = new short?[] { null, 0, 1, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_MultiplyChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_MultiplyChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static short MultiplyChecked_short(short val0, short val1)
				    {
				        return (short)checked(val0 * val1);
				    }
				
				    static bool check_shortq_MultiplyChecked(short? val0, short? val1)
				    {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.MultiplyChecked(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    typeof(Test).GetMethod("MultiplyChecked_short")
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
				                csResult = (short?)checked(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_MultiplyChecked()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_MultiplyChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_MultiplyChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ushort MultiplyChecked_ushort(ushort val0, ushort val1)
				    {
				        return (ushort)checked(val0 * val1);
				    }
				
				    static bool check_ushortq_MultiplyChecked(ushort? val0, ushort? val1)
				    {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.MultiplyChecked(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    typeof(Test).GetMethod("MultiplyChecked_ushort")
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
				                csResult = (ushort?)checked(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_floatq_MultiplyChecked()
				    {
				        float?[] svals = new float?[] { null, 0, 1, float.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_floatq_MultiplyChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("floatq_MultiplyChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static float MultiplyChecked_float(float val0, float val1)
				    {
				        return (float)checked(val0 * val1);
				    }
				
				    static bool check_floatq_MultiplyChecked(float? val0, float? val1)
				    {
				        Expression<Func<float?>> e =
				            Expression.Lambda<Func<float?>>(
				                Expression.MultiplyChecked(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float?)),
				                    typeof(Test).GetMethod("MultiplyChecked_float")
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
				                csResult = (float?)checked(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_doubleq_MultiplyChecked()
				    {
				        double?[] svals = new double?[] { null, 0, 1, double.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_doubleq_MultiplyChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("doubleq_MultiplyChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static double MultiplyChecked_double(double val0, double val1)
				    {
				        return (double)checked(val0 * val1);
				    }
				
				    static bool check_doubleq_MultiplyChecked(double? val0, double? val1)
				    {
				        Expression<Func<double?>> e =
				            Expression.Lambda<Func<double?>>(
				                Expression.MultiplyChecked(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double?)),
				                    typeof(Test).GetMethod("MultiplyChecked_double")
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
				                csResult = (double?)checked(val0 * val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_decimalq_MultiplyChecked()
				    {
				        decimal?[] svals = new decimal?[] { null, 0, 1, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_decimalq_MultiplyChecked(svals[i], svals[j]))
				                {
				                    Console.WriteLine("decimalq_MultiplyChecked failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static decimal MultiplyChecked_decimal(decimal val0, decimal val1)
				    {
				        return (decimal)checked(val0 * val1);
				    }
				
				    static bool check_decimalq_MultiplyChecked(decimal? val0, decimal? val1)
				    {
				        Expression<Func<decimal?>> e =
				            Expression.Lambda<Func<decimal?>>(
				                Expression.MultiplyChecked(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal?)),
				                    typeof(Test).GetMethod("MultiplyChecked_decimal")
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
				                csResult = (decimal?)checked(val0 * val1);
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
