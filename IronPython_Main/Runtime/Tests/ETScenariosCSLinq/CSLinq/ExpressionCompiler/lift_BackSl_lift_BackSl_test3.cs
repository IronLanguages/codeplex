#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 271
			namespace Scenario271{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And__1", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression And__1() {
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
				            success = check_byteq_And() &
				                check_sbyteq_And() &
				                check_ushortq_And() &
				                check_shortq_And() &
				                check_uintq_And() &
				                check_intq_And() &
				                check_ulongq_And() &
				                check_longq_And();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_And()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_And(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_And failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static byte And_byte(byte val0, byte val1)
				    {
				        return (byte)(val0 & val1);
				    }
				
				    static bool check_byteq_And(byte? val0, byte? val1)
				    {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.And(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    typeof(Test).GetMethod("And_byte")
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
				                csResult = (byte?)(val0 & val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_And()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_And(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_And failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static sbyte And_sbyte(sbyte val0, sbyte val1)
				    {
				        return (sbyte)(val0 & val1);
				    }
				
				    static bool check_sbyteq_And(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.And(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    typeof(Test).GetMethod("And_sbyte")
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
				                csResult = (sbyte?)(val0 & val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_And()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_And(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_And failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ushort And_ushort(ushort val0, ushort val1)
				    {
				        return (ushort)(val0 & val1);
				    }
				
				    static bool check_ushortq_And(ushort? val0, ushort? val1)
				    {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.And(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    typeof(Test).GetMethod("And_ushort")
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
				                csResult = (ushort?)(val0 & val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_And()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_And(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_And failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static short And_short(short val0, short val1)
				    {
				        return (short)(val0 & val1);
				    }
				
				    static bool check_shortq_And(short? val0, short? val1)
				    {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.And(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    typeof(Test).GetMethod("And_short")
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
				                csResult = (short?)(val0 & val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_And()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_And(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_And failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static uint And_uint(uint val0, uint val1)
				    {
				        return (uint)(val0 & val1);
				    }
				
				    static bool check_uintq_And(uint? val0, uint? val1)
				    {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.And(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    typeof(Test).GetMethod("And_uint")
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
				                csResult = (uint?)(val0 & val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_And()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_And(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_And failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static int And_int(int val0, int val1)
				    {
				        return (int)(val0 & val1);
				    }
				
				    static bool check_intq_And(int? val0, int? val1)
				    {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.And(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    typeof(Test).GetMethod("And_int")
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
				                csResult = (int?)(val0 & val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_And()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_And(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_And failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ulong And_ulong(ulong val0, ulong val1)
				    {
				        return (ulong)(val0 & val1);
				    }
				
				    static bool check_ulongq_And(ulong? val0, ulong? val1)
				    {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.And(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    typeof(Test).GetMethod("And_ulong")
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
				                csResult = (ulong?)(val0 & val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_And()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_And(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_And failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static long And_long(long val0, long val1)
				    {
				        return (long)(val0 & val1);
				    }
				
				    static bool check_longq_And(long? val0, long? val1)
				    {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.And(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    typeof(Test).GetMethod("And_long")
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
				                csResult = (long?)(val0 & val1);
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
			
			//-------- Scenario 272
			namespace Scenario272{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or__1", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Or__1() {
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
				            success = check_byteq_Or() &
				                check_sbyteq_Or() &
				                check_ushortq_Or() &
				                check_shortq_Or() &
				                check_uintq_Or() &
				                check_intq_Or() &
				                check_ulongq_Or() &
				                check_longq_Or();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_Or()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_Or(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_Or failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static byte Or_byte(byte val0, byte val1)
				    {
				        return (byte)(val0 | val1);
				    }
				
				    static bool check_byteq_Or(byte? val0, byte? val1)
				    {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.Or(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    typeof(Test).GetMethod("Or_byte")
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
				                csResult = (byte?)(val0 | val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_Or()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_Or(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_Or failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static sbyte Or_sbyte(sbyte val0, sbyte val1)
				    {
				        return (sbyte)(val0 | val1);
				    }
				
				    static bool check_sbyteq_Or(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.Or(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    typeof(Test).GetMethod("Or_sbyte")
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
				                csResult = (sbyte?)(val0 | val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_Or()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_Or(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_Or failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ushort Or_ushort(ushort val0, ushort val1)
				    {
				        return (ushort)(val0 | val1);
				    }
				
				    static bool check_ushortq_Or(ushort? val0, ushort? val1)
				    {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.Or(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    typeof(Test).GetMethod("Or_ushort")
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
				                csResult = (ushort?)(val0 | val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_Or()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_Or(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_Or failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static short Or_short(short val0, short val1)
				    {
				        return (short)(val0 | val1);
				    }
				
				    static bool check_shortq_Or(short? val0, short? val1)
				    {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.Or(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    typeof(Test).GetMethod("Or_short")
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
				                csResult = (short?)(val0 | val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_Or()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_Or(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_Or failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static uint Or_uint(uint val0, uint val1)
				    {
				        return (uint)(val0 | val1);
				    }
				
				    static bool check_uintq_Or(uint? val0, uint? val1)
				    {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.Or(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    typeof(Test).GetMethod("Or_uint")
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
				                csResult = (uint?)(val0 | val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_Or()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_Or(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_Or failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static int Or_int(int val0, int val1)
				    {
				        return (int)(val0 | val1);
				    }
				
				    static bool check_intq_Or(int? val0, int? val1)
				    {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.Or(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    typeof(Test).GetMethod("Or_int")
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
				                csResult = (int?)(val0 | val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_Or()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_Or(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_Or failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ulong Or_ulong(ulong val0, ulong val1)
				    {
				        return (ulong)(val0 | val1);
				    }
				
				    static bool check_ulongq_Or(ulong? val0, ulong? val1)
				    {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.Or(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    typeof(Test).GetMethod("Or_ulong")
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
				                csResult = (ulong?)(val0 | val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_Or()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_Or(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_Or failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static long Or_long(long val0, long val1)
				    {
				        return (long)(val0 | val1);
				    }
				
				    static bool check_longq_Or(long? val0, long? val1)
				    {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.Or(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    typeof(Test).GetMethod("Or_long")
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
				                csResult = (long?)(val0 | val1);
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
			
			//-------- Scenario 273
			namespace Scenario273{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ExclusiveOr__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ExclusiveOr__() {
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
				            success = check_byteq_ExclusiveOr() &
				                check_sbyteq_ExclusiveOr() &
				                check_ushortq_ExclusiveOr() &
				                check_shortq_ExclusiveOr() &
				                check_uintq_ExclusiveOr() &
				                check_intq_ExclusiveOr() &
				                check_ulongq_ExclusiveOr() &
				                check_longq_ExclusiveOr();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_ExclusiveOr()
				    {
				        byte?[] svals = new byte?[] { null, 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_byteq_ExclusiveOr(svals[i], svals[j]))
				                {
				                    Console.WriteLine("byteq_ExclusiveOr failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static byte ExclusiveOr_byte(byte val0, byte val1)
				    {
				        return (byte)(val0 ^ val1);
				    }
				
				    static bool check_byteq_ExclusiveOr(byte? val0, byte? val1)
				    {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.ExclusiveOr(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte?)),
				                    typeof(Test).GetMethod("ExclusiveOr_byte")
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
				                csResult = (byte?)(val0 ^ val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_sbyteq_ExclusiveOr()
				    {
				        sbyte?[] svals = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_sbyteq_ExclusiveOr(svals[i], svals[j]))
				                {
				                    Console.WriteLine("sbyteq_ExclusiveOr failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static sbyte ExclusiveOr_sbyte(sbyte val0, sbyte val1)
				    {
				        return (sbyte)(val0 ^ val1);
				    }
				
				    static bool check_sbyteq_ExclusiveOr(sbyte? val0, sbyte? val1)
				    {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.ExclusiveOr(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte?)),
				                    typeof(Test).GetMethod("ExclusiveOr_sbyte")
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
				                csResult = (sbyte?)(val0 ^ val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ushortq_ExclusiveOr()
				    {
				        ushort?[] svals = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ushortq_ExclusiveOr(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ushortq_ExclusiveOr failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ushort ExclusiveOr_ushort(ushort val0, ushort val1)
				    {
				        return (ushort)(val0 ^ val1);
				    }
				
				    static bool check_ushortq_ExclusiveOr(ushort? val0, ushort? val1)
				    {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.ExclusiveOr(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort?)),
				                    typeof(Test).GetMethod("ExclusiveOr_ushort")
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
				                csResult = (ushort?)(val0 ^ val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_shortq_ExclusiveOr()
				    {
				        short?[] svals = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_shortq_ExclusiveOr(svals[i], svals[j]))
				                {
				                    Console.WriteLine("shortq_ExclusiveOr failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static short ExclusiveOr_short(short val0, short val1)
				    {
				        return (short)(val0 ^ val1);
				    }
				
				    static bool check_shortq_ExclusiveOr(short? val0, short? val1)
				    {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.ExclusiveOr(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short?)),
				                    typeof(Test).GetMethod("ExclusiveOr_short")
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
				                csResult = (short?)(val0 ^ val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_uintq_ExclusiveOr()
				    {
				        uint?[] svals = new uint?[] { null, 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_uintq_ExclusiveOr(svals[i], svals[j]))
				                {
				                    Console.WriteLine("uintq_ExclusiveOr failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static uint ExclusiveOr_uint(uint val0, uint val1)
				    {
				        return (uint)(val0 ^ val1);
				    }
				
				    static bool check_uintq_ExclusiveOr(uint? val0, uint? val1)
				    {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.ExclusiveOr(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint?)),
				                    typeof(Test).GetMethod("ExclusiveOr_uint")
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
				                csResult = (uint?)(val0 ^ val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_intq_ExclusiveOr()
				    {
				        int?[] svals = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_intq_ExclusiveOr(svals[i], svals[j]))
				                {
				                    Console.WriteLine("intq_ExclusiveOr failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static int ExclusiveOr_int(int val0, int val1)
				    {
				        return (int)(val0 ^ val1);
				    }
				
				    static bool check_intq_ExclusiveOr(int? val0, int? val1)
				    {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.ExclusiveOr(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int?)),
				                    typeof(Test).GetMethod("ExclusiveOr_int")
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
				                csResult = (int?)(val0 ^ val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_ulongq_ExclusiveOr()
				    {
				        ulong?[] svals = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_ulongq_ExclusiveOr(svals[i], svals[j]))
				                {
				                    Console.WriteLine("ulongq_ExclusiveOr failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static ulong ExclusiveOr_ulong(ulong val0, ulong val1)
				    {
				        return (ulong)(val0 ^ val1);
				    }
				
				    static bool check_ulongq_ExclusiveOr(ulong? val0, ulong? val1)
				    {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.ExclusiveOr(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong?)),
				                    typeof(Test).GetMethod("ExclusiveOr_ulong")
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
				                csResult = (ulong?)(val0 ^ val1);
				            }
				            catch (Exception ex)
				            {
				                return fEx != null && fEx.GetType().Equals(ex.GetType());
				            }
				        }
				        return fEx == null && object.Equals(fResult, csResult);
				    }
				
				    static bool check_longq_ExclusiveOr()
				    {
				        long?[] svals = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_longq_ExclusiveOr(svals[i], svals[j]))
				                {
				                    Console.WriteLine("longq_ExclusiveOr failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    public static long ExclusiveOr_long(long val0, long val1)
				    {
				        return (long)(val0 ^ val1);
				    }
				
				    static bool check_longq_ExclusiveOr(long? val0, long? val1)
				    {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.ExclusiveOr(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long?)),
				                    typeof(Test).GetMethod("ExclusiveOr_long")
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
				                csResult = (long?)(val0 ^ val1);
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
