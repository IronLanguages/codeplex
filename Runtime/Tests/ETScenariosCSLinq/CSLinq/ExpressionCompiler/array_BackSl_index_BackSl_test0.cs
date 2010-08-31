#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 3124
			namespace Scenario3124{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_ArrayIndex__() {
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
				            success = check_byte_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_ArrayIndex()
				    {
				        return checkEx_byte_ArrayIndex(null, -1) &
				            checkEx_byte_ArrayIndex(null, 0) &
				            checkEx_byte_ArrayIndex(null, 1) &
				
				            check_byte_ArrayIndex(genArrbyte_ArrayIndex(0)) &
				            check_byte_ArrayIndex(genArrbyte_ArrayIndex(1)) &
				            check_byte_ArrayIndex(genArrbyte_ArrayIndex(5));
				    }
				
				    static byte[] genArrbyte_ArrayIndex(int size)
				    {
				        byte[] vals = new byte[] { 0, 1, byte.MaxValue };
				        byte[] result = new byte[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_byte_ArrayIndex(byte[] val)
				    {
				        bool success = checkEx_byte_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_byte_ArrayIndex(val, 0);
				        }
				        success &= checkEx_byte_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_byte_ArrayIndex(byte[] val, int index)
				    {
				        try
				        {
				            check_byte_ArrayIndex(val, index);
				            Console.WriteLine("byte_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_byte_ArrayIndex(byte[] val, int index)
				    {
				        Expression<Func<byte>> e =
				            Expression.Lambda<Func<byte>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(byte[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3125
			namespace Scenario3125{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_ArrayIndex__() {
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
				            success = check_ushort_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_ArrayIndex()
				    {
				        return checkEx_ushort_ArrayIndex(null, -1) &
				            checkEx_ushort_ArrayIndex(null, 0) &
				            checkEx_ushort_ArrayIndex(null, 1) &
				
				            check_ushort_ArrayIndex(genArrushort_ArrayIndex(0)) &
				            check_ushort_ArrayIndex(genArrushort_ArrayIndex(1)) &
				            check_ushort_ArrayIndex(genArrushort_ArrayIndex(5));
				    }
				
				    static ushort[] genArrushort_ArrayIndex(int size)
				    {
				        ushort[] vals = new ushort[] { 0, 1, ushort.MaxValue };
				        ushort[] result = new ushort[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_ushort_ArrayIndex(ushort[] val)
				    {
				        bool success = checkEx_ushort_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_ushort_ArrayIndex(val, 0);
				        }
				        success &= checkEx_ushort_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_ushort_ArrayIndex(ushort[] val, int index)
				    {
				        try
				        {
				            check_ushort_ArrayIndex(val, index);
				            Console.WriteLine("ushort_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_ushort_ArrayIndex(ushort[] val, int index)
				    {
				        Expression<Func<ushort>> e =
				            Expression.Lambda<Func<ushort>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(ushort[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3126
			namespace Scenario3126{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_ArrayIndex__() {
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
				            success = check_uint_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_ArrayIndex()
				    {
				        return checkEx_uint_ArrayIndex(null, -1) &
				            checkEx_uint_ArrayIndex(null, 0) &
				            checkEx_uint_ArrayIndex(null, 1) &
				
				            check_uint_ArrayIndex(genArruint_ArrayIndex(0)) &
				            check_uint_ArrayIndex(genArruint_ArrayIndex(1)) &
				            check_uint_ArrayIndex(genArruint_ArrayIndex(5));
				    }
				
				    static uint[] genArruint_ArrayIndex(int size)
				    {
				        uint[] vals = new uint[] { 0, 1, uint.MaxValue };
				        uint[] result = new uint[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_uint_ArrayIndex(uint[] val)
				    {
				        bool success = checkEx_uint_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_uint_ArrayIndex(val, 0);
				        }
				        success &= checkEx_uint_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_uint_ArrayIndex(uint[] val, int index)
				    {
				        try
				        {
				            check_uint_ArrayIndex(val, index);
				            Console.WriteLine("uint_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_uint_ArrayIndex(uint[] val, int index)
				    {
				        Expression<Func<uint>> e =
				            Expression.Lambda<Func<uint>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(uint[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3127
			namespace Scenario3127{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_ArrayIndex__() {
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
				            success = check_ulong_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_ArrayIndex()
				    {
				        return checkEx_ulong_ArrayIndex(null, -1) &
				            checkEx_ulong_ArrayIndex(null, 0) &
				            checkEx_ulong_ArrayIndex(null, 1) &
				
				            check_ulong_ArrayIndex(genArrulong_ArrayIndex(0)) &
				            check_ulong_ArrayIndex(genArrulong_ArrayIndex(1)) &
				            check_ulong_ArrayIndex(genArrulong_ArrayIndex(5));
				    }
				
				    static ulong[] genArrulong_ArrayIndex(int size)
				    {
				        ulong[] vals = new ulong[] { 0, 1, ulong.MaxValue };
				        ulong[] result = new ulong[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_ulong_ArrayIndex(ulong[] val)
				    {
				        bool success = checkEx_ulong_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_ulong_ArrayIndex(val, 0);
				        }
				        success &= checkEx_ulong_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_ulong_ArrayIndex(ulong[] val, int index)
				    {
				        try
				        {
				            check_ulong_ArrayIndex(val, index);
				            Console.WriteLine("ulong_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_ulong_ArrayIndex(ulong[] val, int index)
				    {
				        Expression<Func<ulong>> e =
				            Expression.Lambda<Func<ulong>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(ulong[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3128
			namespace Scenario3128{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_ArrayIndex__() {
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
				            success = check_sbyte_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_ArrayIndex()
				    {
				        return checkEx_sbyte_ArrayIndex(null, -1) &
				            checkEx_sbyte_ArrayIndex(null, 0) &
				            checkEx_sbyte_ArrayIndex(null, 1) &
				
				            check_sbyte_ArrayIndex(genArrsbyte_ArrayIndex(0)) &
				            check_sbyte_ArrayIndex(genArrsbyte_ArrayIndex(1)) &
				            check_sbyte_ArrayIndex(genArrsbyte_ArrayIndex(5));
				    }
				
				    static sbyte[] genArrsbyte_ArrayIndex(int size)
				    {
				        sbyte[] vals = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        sbyte[] result = new sbyte[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_sbyte_ArrayIndex(sbyte[] val)
				    {
				        bool success = checkEx_sbyte_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_sbyte_ArrayIndex(val, 0);
				        }
				        success &= checkEx_sbyte_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_sbyte_ArrayIndex(sbyte[] val, int index)
				    {
				        try
				        {
				            check_sbyte_ArrayIndex(val, index);
				            Console.WriteLine("sbyte_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_sbyte_ArrayIndex(sbyte[] val, int index)
				    {
				        Expression<Func<sbyte>> e =
				            Expression.Lambda<Func<sbyte>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(sbyte[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3129
			namespace Scenario3129{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_ArrayIndex__() {
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
				            success = check_short_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_ArrayIndex()
				    {
				        return checkEx_short_ArrayIndex(null, -1) &
				            checkEx_short_ArrayIndex(null, 0) &
				            checkEx_short_ArrayIndex(null, 1) &
				
				            check_short_ArrayIndex(genArrshort_ArrayIndex(0)) &
				            check_short_ArrayIndex(genArrshort_ArrayIndex(1)) &
				            check_short_ArrayIndex(genArrshort_ArrayIndex(5));
				    }
				
				    static short[] genArrshort_ArrayIndex(int size)
				    {
				        short[] vals = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        short[] result = new short[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_short_ArrayIndex(short[] val)
				    {
				        bool success = checkEx_short_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_short_ArrayIndex(val, 0);
				        }
				        success &= checkEx_short_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_short_ArrayIndex(short[] val, int index)
				    {
				        try
				        {
				            check_short_ArrayIndex(val, index);
				            Console.WriteLine("short_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_short_ArrayIndex(short[] val, int index)
				    {
				        Expression<Func<short>> e =
				            Expression.Lambda<Func<short>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(short[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3130
			namespace Scenario3130{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_ArrayIndex__() {
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
				            success = check_int_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_ArrayIndex()
				    {
				        return checkEx_int_ArrayIndex(null, -1) &
				            checkEx_int_ArrayIndex(null, 0) &
				            checkEx_int_ArrayIndex(null, 1) &
				
				            check_int_ArrayIndex(genArrint_ArrayIndex(0)) &
				            check_int_ArrayIndex(genArrint_ArrayIndex(1)) &
				            check_int_ArrayIndex(genArrint_ArrayIndex(5));
				    }
				
				    static int[] genArrint_ArrayIndex(int size)
				    {
				        int[] vals = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        int[] result = new int[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_int_ArrayIndex(int[] val)
				    {
				        bool success = checkEx_int_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_int_ArrayIndex(val, 0);
				        }
				        success &= checkEx_int_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_int_ArrayIndex(int[] val, int index)
				    {
				        try
				        {
				            check_int_ArrayIndex(val, index);
				            Console.WriteLine("int_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_int_ArrayIndex(int[] val, int index)
				    {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(int[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3131
			namespace Scenario3131{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_ArrayIndex__() {
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
				            success = check_long_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_ArrayIndex()
				    {
				        return checkEx_long_ArrayIndex(null, -1) &
				            checkEx_long_ArrayIndex(null, 0) &
				            checkEx_long_ArrayIndex(null, 1) &
				
				            check_long_ArrayIndex(genArrlong_ArrayIndex(0)) &
				            check_long_ArrayIndex(genArrlong_ArrayIndex(1)) &
				            check_long_ArrayIndex(genArrlong_ArrayIndex(5));
				    }
				
				    static long[] genArrlong_ArrayIndex(int size)
				    {
				        long[] vals = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        long[] result = new long[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_long_ArrayIndex(long[] val)
				    {
				        bool success = checkEx_long_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_long_ArrayIndex(val, 0);
				        }
				        success &= checkEx_long_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_long_ArrayIndex(long[] val, int index)
				    {
				        try
				        {
				            check_long_ArrayIndex(val, index);
				            Console.WriteLine("long_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_long_ArrayIndex(long[] val, int index)
				    {
				        Expression<Func<long>> e =
				            Expression.Lambda<Func<long>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(long[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3132
			namespace Scenario3132{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "float_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression float_ArrayIndex__() {
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
				            success = check_float_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_float_ArrayIndex()
				    {
				        return checkEx_float_ArrayIndex(null, -1) &
				            checkEx_float_ArrayIndex(null, 0) &
				            checkEx_float_ArrayIndex(null, 1) &
				
				            check_float_ArrayIndex(genArrfloat_ArrayIndex(0)) &
				            check_float_ArrayIndex(genArrfloat_ArrayIndex(1)) &
				            check_float_ArrayIndex(genArrfloat_ArrayIndex(5));
				    }
				
				    static float[] genArrfloat_ArrayIndex(int size)
				    {
				        float[] vals = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        float[] result = new float[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_float_ArrayIndex(float[] val)
				    {
				        bool success = checkEx_float_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_float_ArrayIndex(val, 0);
				        }
				        success &= checkEx_float_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_float_ArrayIndex(float[] val, int index)
				    {
				        try
				        {
				            check_float_ArrayIndex(val, index);
				            Console.WriteLine("float_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_float_ArrayIndex(float[] val, int index)
				    {
				        Expression<Func<float>> e =
				            Expression.Lambda<Func<float>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(float[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3133
			namespace Scenario3133{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "double_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression double_ArrayIndex__() {
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
				            success = check_double_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_double_ArrayIndex()
				    {
				        return checkEx_double_ArrayIndex(null, -1) &
				            checkEx_double_ArrayIndex(null, 0) &
				            checkEx_double_ArrayIndex(null, 1) &
				
				            check_double_ArrayIndex(genArrdouble_ArrayIndex(0)) &
				            check_double_ArrayIndex(genArrdouble_ArrayIndex(1)) &
				            check_double_ArrayIndex(genArrdouble_ArrayIndex(5));
				    }
				
				    static double[] genArrdouble_ArrayIndex(int size)
				    {
				        double[] vals = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        double[] result = new double[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_double_ArrayIndex(double[] val)
				    {
				        bool success = checkEx_double_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_double_ArrayIndex(val, 0);
				        }
				        success &= checkEx_double_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_double_ArrayIndex(double[] val, int index)
				    {
				        try
				        {
				            check_double_ArrayIndex(val, index);
				            Console.WriteLine("double_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_double_ArrayIndex(double[] val, int index)
				    {
				        Expression<Func<double>> e =
				            Expression.Lambda<Func<double>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(double[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3134
			namespace Scenario3134{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "decimal_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression decimal_ArrayIndex__() {
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
				            success = check_decimal_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_decimal_ArrayIndex()
				    {
				        return checkEx_decimal_ArrayIndex(null, -1) &
				            checkEx_decimal_ArrayIndex(null, 0) &
				            checkEx_decimal_ArrayIndex(null, 1) &
				
				            check_decimal_ArrayIndex(genArrdecimal_ArrayIndex(0)) &
				            check_decimal_ArrayIndex(genArrdecimal_ArrayIndex(1)) &
				            check_decimal_ArrayIndex(genArrdecimal_ArrayIndex(5));
				    }
				
				    static decimal[] genArrdecimal_ArrayIndex(int size)
				    {
				        decimal[] vals = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        decimal[] result = new decimal[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_decimal_ArrayIndex(decimal[] val)
				    {
				        bool success = checkEx_decimal_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_decimal_ArrayIndex(val, 0);
				        }
				        success &= checkEx_decimal_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_decimal_ArrayIndex(decimal[] val, int index)
				    {
				        try
				        {
				            check_decimal_ArrayIndex(val, index);
				            Console.WriteLine("decimal_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_decimal_ArrayIndex(decimal[] val, int index)
				    {
				        Expression<Func<decimal>> e =
				            Expression.Lambda<Func<decimal>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(decimal[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3135
			namespace Scenario3135{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "char_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression char_ArrayIndex__() {
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
				            success = check_char_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_char_ArrayIndex()
				    {
				        return checkEx_char_ArrayIndex(null, -1) &
				            checkEx_char_ArrayIndex(null, 0) &
				            checkEx_char_ArrayIndex(null, 1) &
				
				            check_char_ArrayIndex(genArrchar_ArrayIndex(0)) &
				            check_char_ArrayIndex(genArrchar_ArrayIndex(1)) &
				            check_char_ArrayIndex(genArrchar_ArrayIndex(5));
				    }
				
				    static char[] genArrchar_ArrayIndex(int size)
				    {
				        char[] vals = new char[] { '\0', '\b', 'A', '\uffff' };
				        char[] result = new char[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_char_ArrayIndex(char[] val)
				    {
				        bool success = checkEx_char_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_char_ArrayIndex(val, 0);
				        }
				        success &= checkEx_char_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_char_ArrayIndex(char[] val, int index)
				    {
				        try
				        {
				            check_char_ArrayIndex(val, index);
				            Console.WriteLine("char_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_char_ArrayIndex(char[] val, int index)
				    {
				        Expression<Func<char>> e =
				            Expression.Lambda<Func<char>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(char[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3136
			namespace Scenario3136{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "bool_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression bool_ArrayIndex__() {
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
				            success = check_bool_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_bool_ArrayIndex()
				    {
				        return checkEx_bool_ArrayIndex(null, -1) &
				            checkEx_bool_ArrayIndex(null, 0) &
				            checkEx_bool_ArrayIndex(null, 1) &
				
				            check_bool_ArrayIndex(genArrbool_ArrayIndex(0)) &
				            check_bool_ArrayIndex(genArrbool_ArrayIndex(1)) &
				            check_bool_ArrayIndex(genArrbool_ArrayIndex(5));
				    }
				
				    static bool[] genArrbool_ArrayIndex(int size)
				    {
				        bool[] vals = new bool[] { true, false };
				        bool[] result = new bool[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_bool_ArrayIndex(bool[] val)
				    {
				        bool success = checkEx_bool_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_bool_ArrayIndex(val, 0);
				        }
				        success &= checkEx_bool_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_bool_ArrayIndex(bool[] val, int index)
				    {
				        try
				        {
				            check_bool_ArrayIndex(val, index);
				            Console.WriteLine("bool_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_bool_ArrayIndex(bool[] val, int index)
				    {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(bool[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
				
				//-------- Scenario 3137
				namespace Scenario3137{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "S_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression S_ArrayIndex__() {
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
					            success = check_S_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_S_ArrayIndex()
					    {
					        return checkEx_S_ArrayIndex(null, -1) &
					            checkEx_S_ArrayIndex(null, 0) &
					            checkEx_S_ArrayIndex(null, 1) &
					
					            check_S_ArrayIndex(genArrS_ArrayIndex(0)) &
					            check_S_ArrayIndex(genArrS_ArrayIndex(1)) &
					            check_S_ArrayIndex(genArrS_ArrayIndex(5));
					    }
					
					    static S[] genArrS_ArrayIndex(int size)
					    {
					        S[] vals = new S[] { default(S), new S() };
					        S[] result = new S[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_S_ArrayIndex(S[] val)
					    {
					        bool success = checkEx_S_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_S_ArrayIndex(val, 0);
					        }
					        success &= checkEx_S_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_S_ArrayIndex(S[] val, int index)
					    {
					        try
					        {
					            check_S_ArrayIndex(val, index);
					            Console.WriteLine("S_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_S_ArrayIndex(S[] val, int index)
					    {
					        Expression<Func<S>> e =
					            Expression.Lambda<Func<S>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(S[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3138
				namespace Scenario3138{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Sp_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Sp_ArrayIndex__() {
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
					            success = check_Sp_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Sp_ArrayIndex()
					    {
					        return checkEx_Sp_ArrayIndex(null, -1) &
					            checkEx_Sp_ArrayIndex(null, 0) &
					            checkEx_Sp_ArrayIndex(null, 1) &
					
					            check_Sp_ArrayIndex(genArrSp_ArrayIndex(0)) &
					            check_Sp_ArrayIndex(genArrSp_ArrayIndex(1)) &
					            check_Sp_ArrayIndex(genArrSp_ArrayIndex(5));
					    }
					
					    static Sp[] genArrSp_ArrayIndex(int size)
					    {
					        Sp[] vals = new Sp[] { default(Sp), new Sp(), new Sp(5, 5.0) };
					        Sp[] result = new Sp[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Sp_ArrayIndex(Sp[] val)
					    {
					        bool success = checkEx_Sp_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_Sp_ArrayIndex(val, 0);
					        }
					        success &= checkEx_Sp_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Sp_ArrayIndex(Sp[] val, int index)
					    {
					        try
					        {
					            check_Sp_ArrayIndex(val, index);
					            Console.WriteLine("Sp_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_Sp_ArrayIndex(Sp[] val, int index)
					    {
					        Expression<Func<Sp>> e =
					            Expression.Lambda<Func<Sp>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Sp[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3139
				namespace Scenario3139{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ss_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ss_ArrayIndex__() {
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
					            success = check_Ss_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ss_ArrayIndex()
					    {
					        return checkEx_Ss_ArrayIndex(null, -1) &
					            checkEx_Ss_ArrayIndex(null, 0) &
					            checkEx_Ss_ArrayIndex(null, 1) &
					
					            check_Ss_ArrayIndex(genArrSs_ArrayIndex(0)) &
					            check_Ss_ArrayIndex(genArrSs_ArrayIndex(1)) &
					            check_Ss_ArrayIndex(genArrSs_ArrayIndex(5));
					    }
					
					    static Ss[] genArrSs_ArrayIndex(int size)
					    {
					        Ss[] vals = new Ss[] { default(Ss), new Ss(), new Ss(new S()) };
					        Ss[] result = new Ss[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Ss_ArrayIndex(Ss[] val)
					    {
					        bool success = checkEx_Ss_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_Ss_ArrayIndex(val, 0);
					        }
					        success &= checkEx_Ss_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Ss_ArrayIndex(Ss[] val, int index)
					    {
					        try
					        {
					            check_Ss_ArrayIndex(val, index);
					            Console.WriteLine("Ss_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_Ss_ArrayIndex(Ss[] val, int index)
					    {
					        Expression<Func<Ss>> e =
					            Expression.Lambda<Func<Ss>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Ss[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3140
				namespace Scenario3140{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Sc_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Sc_ArrayIndex__() {
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
					            success = check_Sc_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Sc_ArrayIndex()
					    {
					        return checkEx_Sc_ArrayIndex(null, -1) &
					            checkEx_Sc_ArrayIndex(null, 0) &
					            checkEx_Sc_ArrayIndex(null, 1) &
					
					            check_Sc_ArrayIndex(genArrSc_ArrayIndex(0)) &
					            check_Sc_ArrayIndex(genArrSc_ArrayIndex(1)) &
					            check_Sc_ArrayIndex(genArrSc_ArrayIndex(5));
					    }
					
					    static Sc[] genArrSc_ArrayIndex(int size)
					    {
					        Sc[] vals = new Sc[] { default(Sc), new Sc(), new Sc(null) };
					        Sc[] result = new Sc[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Sc_ArrayIndex(Sc[] val)
					    {
					        bool success = checkEx_Sc_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_Sc_ArrayIndex(val, 0);
					        }
					        success &= checkEx_Sc_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Sc_ArrayIndex(Sc[] val, int index)
					    {
					        try
					        {
					            check_Sc_ArrayIndex(val, index);
					            Console.WriteLine("Sc_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_Sc_ArrayIndex(Sc[] val, int index)
					    {
					        Expression<Func<Sc>> e =
					            Expression.Lambda<Func<Sc>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Sc[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3141
				namespace Scenario3141{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Scs_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Scs_ArrayIndex__() {
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
					            success = check_Scs_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Scs_ArrayIndex()
					    {
					        return checkEx_Scs_ArrayIndex(null, -1) &
					            checkEx_Scs_ArrayIndex(null, 0) &
					            checkEx_Scs_ArrayIndex(null, 1) &
					
					            check_Scs_ArrayIndex(genArrScs_ArrayIndex(0)) &
					            check_Scs_ArrayIndex(genArrScs_ArrayIndex(1)) &
					            check_Scs_ArrayIndex(genArrScs_ArrayIndex(5));
					    }
					
					    static Scs[] genArrScs_ArrayIndex(int size)
					    {
					        Scs[] vals = new Scs[] { default(Scs), new Scs(), new Scs(null, new S()) };
					        Scs[] result = new Scs[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Scs_ArrayIndex(Scs[] val)
					    {
					        bool success = checkEx_Scs_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_Scs_ArrayIndex(val, 0);
					        }
					        success &= checkEx_Scs_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Scs_ArrayIndex(Scs[] val, int index)
					    {
					        try
					        {
					            check_Scs_ArrayIndex(val, index);
					            Console.WriteLine("Scs_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_Scs_ArrayIndex(Scs[] val, int index)
					    {
					        Expression<Func<Scs>> e =
					            Expression.Lambda<Func<Scs>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Scs[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3142
				namespace Scenario3142{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ts_ArrayIndex_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ts_ArrayIndex_S___() {
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
					            success = check_Ts_ArrayIndex<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ts_ArrayIndex<Ts>() where Ts : struct
					    {
					        return checkEx_Ts_ArrayIndex<Ts>(null, -1) &
					            checkEx_Ts_ArrayIndex<Ts>(null, 0) &
					            checkEx_Ts_ArrayIndex<Ts>(null, 1) &
					
					            check_Ts_ArrayIndex<Ts>(genArrTs_ArrayIndex<Ts>(0)) &
					            check_Ts_ArrayIndex<Ts>(genArrTs_ArrayIndex<Ts>(1)) &
					            check_Ts_ArrayIndex<Ts>(genArrTs_ArrayIndex<Ts>(5));
					    }
					
					    static Ts[] genArrTs_ArrayIndex<Ts>(int size) where Ts : struct
					    {
					        Ts[] vals = new Ts[] { default(Ts), new Ts() };
					        Ts[] result = new Ts[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Ts_ArrayIndex<Ts>(Ts[] val) where Ts : struct
					    {
					        bool success = checkEx_Ts_ArrayIndex<Ts>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_Ts_ArrayIndex<Ts>(val, 0);
					        }
					        success &= checkEx_Ts_ArrayIndex<Ts>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Ts_ArrayIndex<Ts>(Ts[] val, int index) where Ts : struct
					    {
					        try
					        {
					            check_Ts_ArrayIndex<Ts>(val, index);
					            Console.WriteLine("Ts_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_Ts_ArrayIndex<Ts>(Ts[] val, int index) where Ts : struct
					    {
					        Expression<Func<Ts>> e =
					            Expression.Lambda<Func<Ts>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Ts[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3143
				namespace Scenario3143{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ts_ArrayIndex_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ts_ArrayIndex_Scs___() {
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
					            success = check_Ts_ArrayIndex<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ts_ArrayIndex<Ts>() where Ts : struct
					    {
					        return checkEx_Ts_ArrayIndex<Ts>(null, -1) &
					            checkEx_Ts_ArrayIndex<Ts>(null, 0) &
					            checkEx_Ts_ArrayIndex<Ts>(null, 1) &
					
					            check_Ts_ArrayIndex<Ts>(genArrTs_ArrayIndex<Ts>(0)) &
					            check_Ts_ArrayIndex<Ts>(genArrTs_ArrayIndex<Ts>(1)) &
					            check_Ts_ArrayIndex<Ts>(genArrTs_ArrayIndex<Ts>(5));
					    }
					
					    static Ts[] genArrTs_ArrayIndex<Ts>(int size) where Ts : struct
					    {
					        Ts[] vals = new Ts[] { default(Ts), new Ts() };
					        Ts[] result = new Ts[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Ts_ArrayIndex<Ts>(Ts[] val) where Ts : struct
					    {
					        bool success = checkEx_Ts_ArrayIndex<Ts>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_Ts_ArrayIndex<Ts>(val, 0);
					        }
					        success &= checkEx_Ts_ArrayIndex<Ts>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Ts_ArrayIndex<Ts>(Ts[] val, int index) where Ts : struct
					    {
					        try
					        {
					            check_Ts_ArrayIndex<Ts>(val, index);
					            Console.WriteLine("Ts_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_Ts_ArrayIndex<Ts>(Ts[] val, int index) where Ts : struct
					    {
					        Expression<Func<Ts>> e =
					            Expression.Lambda<Func<Ts>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Ts[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3144
				namespace Scenario3144{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ts_ArrayIndex_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ts_ArrayIndex_E___() {
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
					            success = check_Ts_ArrayIndex<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ts_ArrayIndex<Ts>() where Ts : struct
					    {
					        return checkEx_Ts_ArrayIndex<Ts>(null, -1) &
					            checkEx_Ts_ArrayIndex<Ts>(null, 0) &
					            checkEx_Ts_ArrayIndex<Ts>(null, 1) &
					
					            check_Ts_ArrayIndex<Ts>(genArrTs_ArrayIndex<Ts>(0)) &
					            check_Ts_ArrayIndex<Ts>(genArrTs_ArrayIndex<Ts>(1)) &
					            check_Ts_ArrayIndex<Ts>(genArrTs_ArrayIndex<Ts>(5));
					    }
					
					    static Ts[] genArrTs_ArrayIndex<Ts>(int size) where Ts : struct
					    {
					        Ts[] vals = new Ts[] { default(Ts), new Ts() };
					        Ts[] result = new Ts[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Ts_ArrayIndex<Ts>(Ts[] val) where Ts : struct
					    {
					        bool success = checkEx_Ts_ArrayIndex<Ts>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_Ts_ArrayIndex<Ts>(val, 0);
					        }
					        success &= checkEx_Ts_ArrayIndex<Ts>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Ts_ArrayIndex<Ts>(Ts[] val, int index) where Ts : struct
					    {
					        try
					        {
					            check_Ts_ArrayIndex<Ts>(val, index);
					            Console.WriteLine("Ts_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_Ts_ArrayIndex<Ts>(Ts[] val, int index) where Ts : struct
					    {
					        Expression<Func<Ts>> e =
					            Expression.Lambda<Func<Ts>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Ts[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3145
				namespace Scenario3145{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "E_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression E_ArrayIndex__() {
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
					            success = check_E_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_E_ArrayIndex()
					    {
					        return checkEx_E_ArrayIndex(null, -1) &
					            checkEx_E_ArrayIndex(null, 0) &
					            checkEx_E_ArrayIndex(null, 1) &
					
					            check_E_ArrayIndex(genArrE_ArrayIndex(0)) &
					            check_E_ArrayIndex(genArrE_ArrayIndex(1)) &
					            check_E_ArrayIndex(genArrE_ArrayIndex(5));
					    }
					
					    static E[] genArrE_ArrayIndex(int size)
					    {
					        E[] vals = new E[] { (E)0, E.A, E.B, (E)int.MaxValue, (E)int.MinValue };
					        E[] result = new E[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_E_ArrayIndex(E[] val)
					    {
					        bool success = checkEx_E_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_E_ArrayIndex(val, 0);
					        }
					        success &= checkEx_E_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_E_ArrayIndex(E[] val, int index)
					    {
					        try
					        {
					            check_E_ArrayIndex(val, index);
					            Console.WriteLine("E_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_E_ArrayIndex(E[] val, int index)
					    {
					        Expression<Func<E>> e =
					            Expression.Lambda<Func<E>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(E[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<E> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3146
				namespace Scenario3146{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "El_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression El_ArrayIndex__() {
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
					            success = check_El_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_El_ArrayIndex()
					    {
					        return checkEx_El_ArrayIndex(null, -1) &
					            checkEx_El_ArrayIndex(null, 0) &
					            checkEx_El_ArrayIndex(null, 1) &
					
					            check_El_ArrayIndex(genArrEl_ArrayIndex(0)) &
					            check_El_ArrayIndex(genArrEl_ArrayIndex(1)) &
					            check_El_ArrayIndex(genArrEl_ArrayIndex(5));
					    }
					
					    static El[] genArrEl_ArrayIndex(int size)
					    {
					        El[] vals = new El[] { (El)0, El.A, El.B, (El)long.MaxValue, (El)long.MinValue };
					        El[] result = new El[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_El_ArrayIndex(El[] val)
					    {
					        bool success = checkEx_El_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_El_ArrayIndex(val, 0);
					        }
					        success &= checkEx_El_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_El_ArrayIndex(El[] val, int index)
					    {
					        try
					        {
					            check_El_ArrayIndex(val, index);
					            Console.WriteLine("El_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_El_ArrayIndex(El[] val, int index)
					    {
					        Expression<Func<El>> e =
					            Expression.Lambda<Func<El>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(El[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<El> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
			
			//-------- Scenario 3147
			namespace Scenario3147{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "string_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression string_ArrayIndex__() {
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
				            success = check_string_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_string_ArrayIndex()
				    {
				        return checkEx_string_ArrayIndex(null, -1) &
				            checkEx_string_ArrayIndex(null, 0) &
				            checkEx_string_ArrayIndex(null, 1) &
				
				            check_string_ArrayIndex(genArrstring_ArrayIndex(0)) &
				            check_string_ArrayIndex(genArrstring_ArrayIndex(1)) &
				            check_string_ArrayIndex(genArrstring_ArrayIndex(5));
				    }
				
				    static string[] genArrstring_ArrayIndex(int size)
				    {
				        string[] vals = new string[] { null, "", "a", "foo" };
				        string[] result = new string[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_string_ArrayIndex(string[] val)
				    {
				        bool success = checkEx_string_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_string_ArrayIndex(val, 0);
				        }
				        success &= checkEx_string_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_string_ArrayIndex(string[] val, int index)
				    {
				        try
				        {
				            check_string_ArrayIndex(val, index);
				            Console.WriteLine("string_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_string_ArrayIndex(string[] val, int index)
				    {
				        Expression<Func<string>> e =
				            Expression.Lambda<Func<string>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(string[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<string> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
				
				//-------- Scenario 3148
				namespace Scenario3148{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "object_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression object_ArrayIndex__() {
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
					            success = check_object_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_object_ArrayIndex()
					    {
					        return checkEx_object_ArrayIndex(null, -1) &
					            checkEx_object_ArrayIndex(null, 0) &
					            checkEx_object_ArrayIndex(null, 1) &
					
					            check_object_ArrayIndex(genArrobject_ArrayIndex(0)) &
					            check_object_ArrayIndex(genArrobject_ArrayIndex(1)) &
					            check_object_ArrayIndex(genArrobject_ArrayIndex(5));
					    }
					
					    static object[] genArrobject_ArrayIndex(int size)
					    {
					        object[] vals = new object[] { null, new object(), new C(), new D(3) };
					        object[] result = new object[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_object_ArrayIndex(object[] val)
					    {
					        bool success = checkEx_object_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_object_ArrayIndex(val, 0);
					        }
					        success &= checkEx_object_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_object_ArrayIndex(object[] val, int index)
					    {
					        try
					        {
					            check_object_ArrayIndex(val, index);
					            Console.WriteLine("object_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_object_ArrayIndex(object[] val, int index)
					    {
					        Expression<Func<object>> e =
					            Expression.Lambda<Func<object>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(object[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<object> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3149
				namespace Scenario3149{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "C_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression C_ArrayIndex__() {
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
					            success = check_C_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_C_ArrayIndex()
					    {
					        return checkEx_C_ArrayIndex(null, -1) &
					            checkEx_C_ArrayIndex(null, 0) &
					            checkEx_C_ArrayIndex(null, 1) &
					
					            check_C_ArrayIndex(genArrC_ArrayIndex(0)) &
					            check_C_ArrayIndex(genArrC_ArrayIndex(1)) &
					            check_C_ArrayIndex(genArrC_ArrayIndex(5));
					    }
					
					    static C[] genArrC_ArrayIndex(int size)
					    {
					        C[] vals = new C[] { null, new C(), new D(), new D(0), new D(5) };
					        C[] result = new C[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_C_ArrayIndex(C[] val)
					    {
					        bool success = checkEx_C_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_C_ArrayIndex(val, 0);
					        }
					        success &= checkEx_C_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_C_ArrayIndex(C[] val, int index)
					    {
					        try
					        {
					            check_C_ArrayIndex(val, index);
					            Console.WriteLine("C_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_C_ArrayIndex(C[] val, int index)
					    {
					        Expression<Func<C>> e =
					            Expression.Lambda<Func<C>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(C[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<C> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3150
				namespace Scenario3150{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "D_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression D_ArrayIndex__() {
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
					            success = check_D_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_D_ArrayIndex()
					    {
					        return checkEx_D_ArrayIndex(null, -1) &
					            checkEx_D_ArrayIndex(null, 0) &
					            checkEx_D_ArrayIndex(null, 1) &
					
					            check_D_ArrayIndex(genArrD_ArrayIndex(0)) &
					            check_D_ArrayIndex(genArrD_ArrayIndex(1)) &
					            check_D_ArrayIndex(genArrD_ArrayIndex(5));
					    }
					
					    static D[] genArrD_ArrayIndex(int size)
					    {
					        D[] vals = new D[] { null, new D(), new D(0), new D(5) };
					        D[] result = new D[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_D_ArrayIndex(D[] val)
					    {
					        bool success = checkEx_D_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_D_ArrayIndex(val, 0);
					        }
					        success &= checkEx_D_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_D_ArrayIndex(D[] val, int index)
					    {
					        try
					        {
					            check_D_ArrayIndex(val, index);
					            Console.WriteLine("D_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_D_ArrayIndex(D[] val, int index)
					    {
					        Expression<Func<D>> e =
					            Expression.Lambda<Func<D>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(D[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<D> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3151
				namespace Scenario3151{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_ArrayIndex_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_ArrayIndex_object___() {
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
					            success = check_T_ArrayIndex<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_ArrayIndex<T>()
					    {
					        return checkEx_T_ArrayIndex<T>(null, -1) &
					            checkEx_T_ArrayIndex<T>(null, 0) &
					            checkEx_T_ArrayIndex<T>(null, 1) &
					
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(0)) &
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(1)) &
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(5));
					    }
					
					    static T[] genArrT_ArrayIndex<T>(int size)
					    {
					        T[] vals = new T[] { default(T) };
					        T[] result = new T[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_T_ArrayIndex<T>(T[] val)
					    {
					        bool success = checkEx_T_ArrayIndex<T>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_T_ArrayIndex<T>(val, 0);
					        }
					        success &= checkEx_T_ArrayIndex<T>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_T_ArrayIndex<T>(T[] val, int index)
					    {
					        try
					        {
					            check_T_ArrayIndex<T>(val, index);
					            Console.WriteLine("T_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_T_ArrayIndex<T>(T[] val, int index)
					    {
					        Expression<Func<T>> e =
					            Expression.Lambda<Func<T>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(T[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3152
				namespace Scenario3152{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_ArrayIndex_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_ArrayIndex_C___() {
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
					            success = check_T_ArrayIndex<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_ArrayIndex<T>()
					    {
					        return checkEx_T_ArrayIndex<T>(null, -1) &
					            checkEx_T_ArrayIndex<T>(null, 0) &
					            checkEx_T_ArrayIndex<T>(null, 1) &
					
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(0)) &
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(1)) &
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(5));
					    }
					
					    static T[] genArrT_ArrayIndex<T>(int size)
					    {
					        T[] vals = new T[] { default(T) };
					        T[] result = new T[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_T_ArrayIndex<T>(T[] val)
					    {
					        bool success = checkEx_T_ArrayIndex<T>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_T_ArrayIndex<T>(val, 0);
					        }
					        success &= checkEx_T_ArrayIndex<T>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_T_ArrayIndex<T>(T[] val, int index)
					    {
					        try
					        {
					            check_T_ArrayIndex<T>(val, index);
					            Console.WriteLine("T_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_T_ArrayIndex<T>(T[] val, int index)
					    {
					        Expression<Func<T>> e =
					            Expression.Lambda<Func<T>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(T[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3153
				namespace Scenario3153{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_ArrayIndex_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_ArrayIndex_S___() {
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
					            success = check_T_ArrayIndex<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_ArrayIndex<T>()
					    {
					        return checkEx_T_ArrayIndex<T>(null, -1) &
					            checkEx_T_ArrayIndex<T>(null, 0) &
					            checkEx_T_ArrayIndex<T>(null, 1) &
					
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(0)) &
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(1)) &
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(5));
					    }
					
					    static T[] genArrT_ArrayIndex<T>(int size)
					    {
					        T[] vals = new T[] { default(T) };
					        T[] result = new T[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_T_ArrayIndex<T>(T[] val)
					    {
					        bool success = checkEx_T_ArrayIndex<T>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_T_ArrayIndex<T>(val, 0);
					        }
					        success &= checkEx_T_ArrayIndex<T>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_T_ArrayIndex<T>(T[] val, int index)
					    {
					        try
					        {
					            check_T_ArrayIndex<T>(val, index);
					            Console.WriteLine("T_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_T_ArrayIndex<T>(T[] val, int index)
					    {
					        Expression<Func<T>> e =
					            Expression.Lambda<Func<T>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(T[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3154
				namespace Scenario3154{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_ArrayIndex_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_ArrayIndex_Scs___() {
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
					            success = check_T_ArrayIndex<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_ArrayIndex<T>()
					    {
					        return checkEx_T_ArrayIndex<T>(null, -1) &
					            checkEx_T_ArrayIndex<T>(null, 0) &
					            checkEx_T_ArrayIndex<T>(null, 1) &
					
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(0)) &
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(1)) &
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(5));
					    }
					
					    static T[] genArrT_ArrayIndex<T>(int size)
					    {
					        T[] vals = new T[] { default(T) };
					        T[] result = new T[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_T_ArrayIndex<T>(T[] val)
					    {
					        bool success = checkEx_T_ArrayIndex<T>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_T_ArrayIndex<T>(val, 0);
					        }
					        success &= checkEx_T_ArrayIndex<T>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_T_ArrayIndex<T>(T[] val, int index)
					    {
					        try
					        {
					            check_T_ArrayIndex<T>(val, index);
					            Console.WriteLine("T_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_T_ArrayIndex<T>(T[] val, int index)
					    {
					        Expression<Func<T>> e =
					            Expression.Lambda<Func<T>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(T[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3155
				namespace Scenario3155{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_ArrayIndex_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_ArrayIndex_E___() {
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
					            success = check_T_ArrayIndex<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_ArrayIndex<T>()
					    {
					        return checkEx_T_ArrayIndex<T>(null, -1) &
					            checkEx_T_ArrayIndex<T>(null, 0) &
					            checkEx_T_ArrayIndex<T>(null, 1) &
					
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(0)) &
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(1)) &
					            check_T_ArrayIndex<T>(genArrT_ArrayIndex<T>(5));
					    }
					
					    static T[] genArrT_ArrayIndex<T>(int size)
					    {
					        T[] vals = new T[] { default(T) };
					        T[] result = new T[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_T_ArrayIndex<T>(T[] val)
					    {
					        bool success = checkEx_T_ArrayIndex<T>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_T_ArrayIndex<T>(val, 0);
					        }
					        success &= checkEx_T_ArrayIndex<T>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_T_ArrayIndex<T>(T[] val, int index)
					    {
					        try
					        {
					            check_T_ArrayIndex<T>(val, index);
					            Console.WriteLine("T_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_T_ArrayIndex<T>(T[] val, int index)
					    {
					        Expression<Func<T>> e =
					            Expression.Lambda<Func<T>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(T[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3156
				namespace Scenario3156{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tc_ArrayIndex_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tc_ArrayIndex_object___() {
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
					            success = check_Tc_ArrayIndex<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tc_ArrayIndex<Tc>() where Tc : class
					    {
					        return checkEx_Tc_ArrayIndex<Tc>(null, -1) &
					            checkEx_Tc_ArrayIndex<Tc>(null, 0) &
					            checkEx_Tc_ArrayIndex<Tc>(null, 1) &
					
					            check_Tc_ArrayIndex<Tc>(genArrTc_ArrayIndex<Tc>(0)) &
					            check_Tc_ArrayIndex<Tc>(genArrTc_ArrayIndex<Tc>(1)) &
					            check_Tc_ArrayIndex<Tc>(genArrTc_ArrayIndex<Tc>(5));
					    }
					
					    static Tc[] genArrTc_ArrayIndex<Tc>(int size) where Tc : class
					    {
					        Tc[] vals = new Tc[] { null, default(Tc) };
					        Tc[] result = new Tc[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Tc_ArrayIndex<Tc>(Tc[] val) where Tc : class
					    {
					        bool success = checkEx_Tc_ArrayIndex<Tc>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_Tc_ArrayIndex<Tc>(val, 0);
					        }
					        success &= checkEx_Tc_ArrayIndex<Tc>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Tc_ArrayIndex<Tc>(Tc[] val, int index) where Tc : class
					    {
					        try
					        {
					            check_Tc_ArrayIndex<Tc>(val, index);
					            Console.WriteLine("Tc_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_Tc_ArrayIndex<Tc>(Tc[] val, int index) where Tc : class
					    {
					        Expression<Func<Tc>> e =
					            Expression.Lambda<Func<Tc>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Tc[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3157
				namespace Scenario3157{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tc_ArrayIndex_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tc_ArrayIndex_C___() {
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
					            success = check_Tc_ArrayIndex<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tc_ArrayIndex<Tc>() where Tc : class
					    {
					        return checkEx_Tc_ArrayIndex<Tc>(null, -1) &
					            checkEx_Tc_ArrayIndex<Tc>(null, 0) &
					            checkEx_Tc_ArrayIndex<Tc>(null, 1) &
					
					            check_Tc_ArrayIndex<Tc>(genArrTc_ArrayIndex<Tc>(0)) &
					            check_Tc_ArrayIndex<Tc>(genArrTc_ArrayIndex<Tc>(1)) &
					            check_Tc_ArrayIndex<Tc>(genArrTc_ArrayIndex<Tc>(5));
					    }
					
					    static Tc[] genArrTc_ArrayIndex<Tc>(int size) where Tc : class
					    {
					        Tc[] vals = new Tc[] { null, default(Tc) };
					        Tc[] result = new Tc[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Tc_ArrayIndex<Tc>(Tc[] val) where Tc : class
					    {
					        bool success = checkEx_Tc_ArrayIndex<Tc>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_Tc_ArrayIndex<Tc>(val, 0);
					        }
					        success &= checkEx_Tc_ArrayIndex<Tc>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Tc_ArrayIndex<Tc>(Tc[] val, int index) where Tc : class
					    {
					        try
					        {
					            check_Tc_ArrayIndex<Tc>(val, index);
					            Console.WriteLine("Tc_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_Tc_ArrayIndex<Tc>(Tc[] val, int index) where Tc : class
					    {
					        Expression<Func<Tc>> e =
					            Expression.Lambda<Func<Tc>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Tc[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3158
				namespace Scenario3158{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tcn_ArrayIndex_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tcn_ArrayIndex_object___() {
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
					            success = check_Tcn_ArrayIndex<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tcn_ArrayIndex<Tcn>() where Tcn : class, new()
					    {
					        return checkEx_Tcn_ArrayIndex<Tcn>(null, -1) &
					            checkEx_Tcn_ArrayIndex<Tcn>(null, 0) &
					            checkEx_Tcn_ArrayIndex<Tcn>(null, 1) &
					
					            check_Tcn_ArrayIndex<Tcn>(genArrTcn_ArrayIndex<Tcn>(0)) &
					            check_Tcn_ArrayIndex<Tcn>(genArrTcn_ArrayIndex<Tcn>(1)) &
					            check_Tcn_ArrayIndex<Tcn>(genArrTcn_ArrayIndex<Tcn>(5));
					    }
					
					    static Tcn[] genArrTcn_ArrayIndex<Tcn>(int size) where Tcn : class, new()
					    {
					        Tcn[] vals = new Tcn[] { null, default(Tcn), new Tcn() };
					        Tcn[] result = new Tcn[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Tcn_ArrayIndex<Tcn>(Tcn[] val) where Tcn : class, new()
					    {
					        bool success = checkEx_Tcn_ArrayIndex<Tcn>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_Tcn_ArrayIndex<Tcn>(val, 0);
					        }
					        success &= checkEx_Tcn_ArrayIndex<Tcn>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Tcn_ArrayIndex<Tcn>(Tcn[] val, int index) where Tcn : class, new()
					    {
					        try
					        {
					            check_Tcn_ArrayIndex<Tcn>(val, index);
					            Console.WriteLine("Tcn_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_Tcn_ArrayIndex<Tcn>(Tcn[] val, int index) where Tcn : class, new()
					    {
					        Expression<Func<Tcn>> e =
					            Expression.Lambda<Func<Tcn>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Tcn[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3159
				namespace Scenario3159{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tcn_ArrayIndex_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tcn_ArrayIndex_C___() {
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
					            success = check_Tcn_ArrayIndex<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tcn_ArrayIndex<Tcn>() where Tcn : class, new()
					    {
					        return checkEx_Tcn_ArrayIndex<Tcn>(null, -1) &
					            checkEx_Tcn_ArrayIndex<Tcn>(null, 0) &
					            checkEx_Tcn_ArrayIndex<Tcn>(null, 1) &
					
					            check_Tcn_ArrayIndex<Tcn>(genArrTcn_ArrayIndex<Tcn>(0)) &
					            check_Tcn_ArrayIndex<Tcn>(genArrTcn_ArrayIndex<Tcn>(1)) &
					            check_Tcn_ArrayIndex<Tcn>(genArrTcn_ArrayIndex<Tcn>(5));
					    }
					
					    static Tcn[] genArrTcn_ArrayIndex<Tcn>(int size) where Tcn : class, new()
					    {
					        Tcn[] vals = new Tcn[] { null, default(Tcn), new Tcn() };
					        Tcn[] result = new Tcn[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Tcn_ArrayIndex<Tcn>(Tcn[] val) where Tcn : class, new()
					    {
					        bool success = checkEx_Tcn_ArrayIndex<Tcn>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_Tcn_ArrayIndex<Tcn>(val, 0);
					        }
					        success &= checkEx_Tcn_ArrayIndex<Tcn>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Tcn_ArrayIndex<Tcn>(Tcn[] val, int index) where Tcn : class, new()
					    {
					        try
					        {
					            check_Tcn_ArrayIndex<Tcn>(val, index);
					            Console.WriteLine("Tcn_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_Tcn_ArrayIndex<Tcn>(Tcn[] val, int index) where Tcn : class, new()
					    {
					        Expression<Func<Tcn>> e =
					            Expression.Lambda<Func<Tcn>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Tcn[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3160
				namespace Scenario3160{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TC_ArrayIndex_C_a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TC_ArrayIndex_C_a__() {
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
					            success = check_TC_ArrayIndex<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TC_ArrayIndex<TC>() where TC : C
					    {
					        return checkEx_TC_ArrayIndex<TC>(null, -1) &
					            checkEx_TC_ArrayIndex<TC>(null, 0) &
					            checkEx_TC_ArrayIndex<TC>(null, 1) &
					
					            check_TC_ArrayIndex<TC>(genArrTC_ArrayIndex<TC>(0)) &
					            check_TC_ArrayIndex<TC>(genArrTC_ArrayIndex<TC>(1)) &
					            check_TC_ArrayIndex<TC>(genArrTC_ArrayIndex<TC>(5));
					    }
					
					    static TC[] genArrTC_ArrayIndex<TC>(int size) where TC : C
					    {
					        TC[] vals = new TC[] { null, default(TC), (TC)new C() };
					        TC[] result = new TC[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TC_ArrayIndex<TC>(TC[] val) where TC : C
					    {
					        bool success = checkEx_TC_ArrayIndex<TC>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_TC_ArrayIndex<TC>(val, 0);
					        }
					        success &= checkEx_TC_ArrayIndex<TC>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TC_ArrayIndex<TC>(TC[] val, int index) where TC : C
					    {
					        try
					        {
					            check_TC_ArrayIndex<TC>(val, index);
					            Console.WriteLine("TC_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_TC_ArrayIndex<TC>(TC[] val, int index) where TC : C
					    {
					        Expression<Func<TC>> e =
					            Expression.Lambda<Func<TC>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(TC[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<TC> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3161
				namespace Scenario3161{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TCn_ArrayIndex_C_a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TCn_ArrayIndex_C_a__() {
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
					            success = check_TCn_ArrayIndex<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TCn_ArrayIndex<TCn>() where TCn : C, new()
					    {
					        return checkEx_TCn_ArrayIndex<TCn>(null, -1) &
					            checkEx_TCn_ArrayIndex<TCn>(null, 0) &
					            checkEx_TCn_ArrayIndex<TCn>(null, 1) &
					
					            check_TCn_ArrayIndex<TCn>(genArrTCn_ArrayIndex<TCn>(0)) &
					            check_TCn_ArrayIndex<TCn>(genArrTCn_ArrayIndex<TCn>(1)) &
					            check_TCn_ArrayIndex<TCn>(genArrTCn_ArrayIndex<TCn>(5));
					    }
					
					    static TCn[] genArrTCn_ArrayIndex<TCn>(int size) where TCn : C, new()
					    {
					        TCn[] vals = new TCn[] { null, default(TCn), new TCn(), (TCn)new C() };
					        TCn[] result = new TCn[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TCn_ArrayIndex<TCn>(TCn[] val) where TCn : C, new()
					    {
					        bool success = checkEx_TCn_ArrayIndex<TCn>(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_TCn_ArrayIndex<TCn>(val, 0);
					        }
					        success &= checkEx_TCn_ArrayIndex<TCn>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TCn_ArrayIndex<TCn>(TCn[] val, int index) where TCn : C, new()
					    {
					        try
					        {
					            check_TCn_ArrayIndex<TCn>(val, index);
					            Console.WriteLine("TCn_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_TCn_ArrayIndex<TCn>(TCn[] val, int index) where TCn : C, new()
					    {
					        Expression<Func<TCn>> e =
					            Expression.Lambda<Func<TCn>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(TCn[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<TCn> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
			
			//-------- Scenario 3162
			namespace Scenario3162{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Delegate_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Delegate_ArrayIndex__() {
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
				            success = check_Delegate_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_Delegate_ArrayIndex()
				    {
				        return checkEx_Delegate_ArrayIndex(null, -1) &
				            checkEx_Delegate_ArrayIndex(null, 0) &
				            checkEx_Delegate_ArrayIndex(null, 1) &
				
				            check_Delegate_ArrayIndex(genArrDelegate_ArrayIndex(0)) &
				            check_Delegate_ArrayIndex(genArrDelegate_ArrayIndex(1)) &
				            check_Delegate_ArrayIndex(genArrDelegate_ArrayIndex(5));
				    }
				
				    static Delegate[] genArrDelegate_ArrayIndex(int size)
				    {
				        Delegate[] vals = new Delegate[] { null, (Func<object>)delegate() { return null; }, (Func<int, int>)delegate(int i) { return i + 1; }, (Action<object>)delegate { } };
				        Delegate[] result = new Delegate[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_Delegate_ArrayIndex(Delegate[] val)
				    {
				        bool success = checkEx_Delegate_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_Delegate_ArrayIndex(val, 0);
				        }
				        success &= checkEx_Delegate_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_Delegate_ArrayIndex(Delegate[] val, int index)
				    {
				        try
				        {
				            check_Delegate_ArrayIndex(val, index);
				            Console.WriteLine("Delegate_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_Delegate_ArrayIndex(Delegate[] val, int index)
				    {
				        Expression<Func<Delegate>> e =
				            Expression.Lambda<Func<Delegate>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(Delegate[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<Delegate> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
			
			//-------- Scenario 3163
			namespace Scenario3163{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Func_object_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Func_object_ArrayIndex__() {
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
				            success = check_Func_object_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_Func_object_ArrayIndex()
				    {
				        return checkEx_Func_object_ArrayIndex(null, -1) &
				            checkEx_Func_object_ArrayIndex(null, 0) &
				            checkEx_Func_object_ArrayIndex(null, 1) &
				
				            check_Func_object_ArrayIndex(genArrFunc_object_ArrayIndex(0)) &
				            check_Func_object_ArrayIndex(genArrFunc_object_ArrayIndex(1)) &
				            check_Func_object_ArrayIndex(genArrFunc_object_ArrayIndex(5));
				    }
				
				    static Func<object>[] genArrFunc_object_ArrayIndex(int size)
				    {
				        Func<object>[] vals = new Func<object>[] { null, (Func<object>)delegate() { return null; } };
				        Func<object>[] result = new Func<object>[size];
				        for (int i = 0; i < size; i++)
				        {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_Func_object_ArrayIndex(Func<object>[] val)
				    {
				        bool success = checkEx_Func_object_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++)
				        {
				            success &= check_Func_object_ArrayIndex(val, 0);
				        }
				        success &= checkEx_Func_object_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_Func_object_ArrayIndex(Func<object>[] val, int index)
				    {
				        try
				        {
				            check_Func_object_ArrayIndex(val, index);
				            Console.WriteLine("Func_object_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch
				        {
				            return true;
				        }
				    }
				
				    static bool check_Func_object_ArrayIndex(Func<object>[] val, int index)
				    {
				        Expression<Func<Func<object>>> e =
				            Expression.Lambda<Func<Func<object>>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(Func<object>[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<object>> f = e.Compile();
				        return object.Equals(f(), val[index]);
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
				
				//-------- Scenario 3164
				namespace Scenario3164{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IEquatable_C_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression IEquatable_C_ArrayIndex__() {
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
					            success = check_IEquatable_C_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_IEquatable_C_ArrayIndex()
					    {
					        return checkEx_IEquatable_C_ArrayIndex(null, -1) &
					            checkEx_IEquatable_C_ArrayIndex(null, 0) &
					            checkEx_IEquatable_C_ArrayIndex(null, 1) &
					
					            check_IEquatable_C_ArrayIndex(genArrIEquatable_C_ArrayIndex(0)) &
					            check_IEquatable_C_ArrayIndex(genArrIEquatable_C_ArrayIndex(1)) &
					            check_IEquatable_C_ArrayIndex(genArrIEquatable_C_ArrayIndex(5));
					    }
					
					    static IEquatable<C>[] genArrIEquatable_C_ArrayIndex(int size)
					    {
					        IEquatable<C>[] vals = new IEquatable<C>[] { null, new C(), new D(), new D(0), new D(5) };
					        IEquatable<C>[] result = new IEquatable<C>[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_IEquatable_C_ArrayIndex(IEquatable<C>[] val)
					    {
					        bool success = checkEx_IEquatable_C_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_IEquatable_C_ArrayIndex(val, 0);
					        }
					        success &= checkEx_IEquatable_C_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_IEquatable_C_ArrayIndex(IEquatable<C>[] val, int index)
					    {
					        try
					        {
					            check_IEquatable_C_ArrayIndex(val, index);
					            Console.WriteLine("IEquatable_C_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_IEquatable_C_ArrayIndex(IEquatable<C>[] val, int index)
					    {
					        Expression<Func<IEquatable<C>>> e =
					            Expression.Lambda<Func<IEquatable<C>>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(IEquatable<C>[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<C>> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3165
				namespace Scenario3165{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IEquatable_D_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression IEquatable_D_ArrayIndex__() {
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
					            success = check_IEquatable_D_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_IEquatable_D_ArrayIndex()
					    {
					        return checkEx_IEquatable_D_ArrayIndex(null, -1) &
					            checkEx_IEquatable_D_ArrayIndex(null, 0) &
					            checkEx_IEquatable_D_ArrayIndex(null, 1) &
					
					            check_IEquatable_D_ArrayIndex(genArrIEquatable_D_ArrayIndex(0)) &
					            check_IEquatable_D_ArrayIndex(genArrIEquatable_D_ArrayIndex(1)) &
					            check_IEquatable_D_ArrayIndex(genArrIEquatable_D_ArrayIndex(5));
					    }
					
					    static IEquatable<D>[] genArrIEquatable_D_ArrayIndex(int size)
					    {
					        IEquatable<D>[] vals = new IEquatable<D>[] { null, new D(), new D(0), new D(5) };
					        IEquatable<D>[] result = new IEquatable<D>[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_IEquatable_D_ArrayIndex(IEquatable<D>[] val)
					    {
					        bool success = checkEx_IEquatable_D_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_IEquatable_D_ArrayIndex(val, 0);
					        }
					        success &= checkEx_IEquatable_D_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_IEquatable_D_ArrayIndex(IEquatable<D>[] val, int index)
					    {
					        try
					        {
					            check_IEquatable_D_ArrayIndex(val, index);
					            Console.WriteLine("IEquatable_D_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_IEquatable_D_ArrayIndex(IEquatable<D>[] val, int index)
					    {
					        Expression<Func<IEquatable<D>>> e =
					            Expression.Lambda<Func<IEquatable<D>>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(IEquatable<D>[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<D>> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3166
				namespace Scenario3166{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "I_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression I_ArrayIndex__() {
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
					            success = check_I_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_I_ArrayIndex()
					    {
					        return checkEx_I_ArrayIndex(null, -1) &
					            checkEx_I_ArrayIndex(null, 0) &
					            checkEx_I_ArrayIndex(null, 1) &
					
					            check_I_ArrayIndex(genArrI_ArrayIndex(0)) &
					            check_I_ArrayIndex(genArrI_ArrayIndex(1)) &
					            check_I_ArrayIndex(genArrI_ArrayIndex(5));
					    }
					
					    static I[] genArrI_ArrayIndex(int size)
					    {
					        I[] vals = new I[] { null, new C(), new D(), new D(0), new D(5) };
					        I[] result = new I[size];
					        for (int i = 0; i < size; i++)
					        {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_I_ArrayIndex(I[] val)
					    {
					        bool success = checkEx_I_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++)
					        {
					            success &= check_I_ArrayIndex(val, 0);
					        }
					        success &= checkEx_I_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_I_ArrayIndex(I[] val, int index)
					    {
					        try
					        {
					            check_I_ArrayIndex(val, index);
					            Console.WriteLine("I_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch
					        {
					            return true;
					        }
					    }
					
					    static bool check_I_ArrayIndex(I[] val, int index)
					    {
					        Expression<Func<I>> e =
					            Expression.Lambda<Func<I>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(I[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<I> f = e.Compile();
					        return object.Equals(f(), val[index]);
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
				
			
			
			
			public interface I
			{
			    void M();
			}
			
			public  class C : IEquatable<C>, I
			{
			    void I.M()
			    {
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is C && Equals((C)o);
			    }
			
			    public bool Equals(C c)
			    {
			        return c != null;
			    }
			
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D>
			{
			    public int Val;
			    public D()
			    {
			    }
			    public D(int val)
			    {
			        Val = val;
			    }
			
			    public override bool Equals(object o)
			    {
			        return o is D && Equals((D)o);
			    }
			
			    public bool Equals(D d)
			    {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode()
			    {
			        return Val;
			    }
			}
			
			public enum E
			{
			    A = 1, B = 2
			}
			
			public enum El : long
			{
			    A, B, C
			}
			
			public struct S : IEquatable<S>
			{
			    public override bool Equals(object o)
			    {
			        return (o is S) && Equals((S)o);
			    }
			    public bool Equals(S other)
			    {
			        return true;
			    }
			    public override int GetHashCode()
			    {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp>
			{
			    public Sp(int i, double d)
			    {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sp) && Equals((Sp)o);
			    }
			    public bool Equals(Sp other)
			    {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode()
			    {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss>
			{
			    public Ss(S s)
			    {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Ss) && Equals((Ss)o);
			    }
			    public bool Equals(Ss other)
			    {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc>
			{
			    public Sc(string s)
			    {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o)
			    {
			        return (o is Sc) && Equals((Sc)o);
			    }
			    public bool Equals(Sc other)
			    {
			        return other.S == S;
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs>
			{
			    public Scs(string s, S val)
			    {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o)
			    {
			        return (o is Scs) && Equals((Scs)o);
			    }
			    public bool Equals(Scs other)
			    {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode()
			    {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
	
}
