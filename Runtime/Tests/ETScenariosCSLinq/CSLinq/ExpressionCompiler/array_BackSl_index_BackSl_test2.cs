#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 3190
			namespace Scenario3190{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteArray_ArrayIndex__() {
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
				            success = check_byteArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteArray_ArrayIndex() {
				        return checkEx_byteArray_ArrayIndex(null, -1) &
				            checkEx_byteArray_ArrayIndex(null, 0) &
				            checkEx_byteArray_ArrayIndex(null, 1) &
				
				            check_byteArray_ArrayIndex(genArrbyteArray_ArrayIndex(0)) &
				            check_byteArray_ArrayIndex(genArrbyteArray_ArrayIndex(1)) &
				            check_byteArray_ArrayIndex(genArrbyteArray_ArrayIndex(5));
				    }
				
				    static byte[][] genArrbyteArray_ArrayIndex(int size) {
				        byte[][] vals = new byte[][] { null, new byte[0], new byte[] { 0, 1, byte.MaxValue }, new byte[100] };
				        byte[][] result = new byte[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_byteArray_ArrayIndex(byte[][] val) {
				        bool success = checkEx_byteArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_byteArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_byteArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_byteArray_ArrayIndex(byte[][] val, int index) {
				        try {
				            check_byteArray_ArrayIndex(val, index);
				            Console.WriteLine("byteArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_byteArray_ArrayIndex(byte[][] val, int index) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(byte[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte[]> f = e.Compile();
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
			
			//-------- Scenario 3191
			namespace Scenario3191{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortArray_ArrayIndex__() {
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
				            success = check_ushortArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortArray_ArrayIndex() {
				        return checkEx_ushortArray_ArrayIndex(null, -1) &
				            checkEx_ushortArray_ArrayIndex(null, 0) &
				            checkEx_ushortArray_ArrayIndex(null, 1) &
				
				            check_ushortArray_ArrayIndex(genArrushortArray_ArrayIndex(0)) &
				            check_ushortArray_ArrayIndex(genArrushortArray_ArrayIndex(1)) &
				            check_ushortArray_ArrayIndex(genArrushortArray_ArrayIndex(5));
				    }
				
				    static ushort[][] genArrushortArray_ArrayIndex(int size) {
				        ushort[][] vals = new ushort[][] { null, new ushort[0], new ushort[] { 0, 1, ushort.MaxValue }, new ushort[100] };
				        ushort[][] result = new ushort[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_ushortArray_ArrayIndex(ushort[][] val) {
				        bool success = checkEx_ushortArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_ushortArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_ushortArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_ushortArray_ArrayIndex(ushort[][] val, int index) {
				        try {
				            check_ushortArray_ArrayIndex(val, index);
				            Console.WriteLine("ushortArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_ushortArray_ArrayIndex(ushort[][] val, int index) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(ushort[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort[]> f = e.Compile();
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
			
			//-------- Scenario 3192
			namespace Scenario3192{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintArray_ArrayIndex__() {
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
				            success = check_uintArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintArray_ArrayIndex() {
				        return checkEx_uintArray_ArrayIndex(null, -1) &
				            checkEx_uintArray_ArrayIndex(null, 0) &
				            checkEx_uintArray_ArrayIndex(null, 1) &
				
				            check_uintArray_ArrayIndex(genArruintArray_ArrayIndex(0)) &
				            check_uintArray_ArrayIndex(genArruintArray_ArrayIndex(1)) &
				            check_uintArray_ArrayIndex(genArruintArray_ArrayIndex(5));
				    }
				
				    static uint[][] genArruintArray_ArrayIndex(int size) {
				        uint[][] vals = new uint[][] { null, new uint[0], new uint[] { 0, 1, uint.MaxValue }, new uint[100] };
				        uint[][] result = new uint[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_uintArray_ArrayIndex(uint[][] val) {
				        bool success = checkEx_uintArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_uintArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_uintArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_uintArray_ArrayIndex(uint[][] val, int index) {
				        try {
				            check_uintArray_ArrayIndex(val, index);
				            Console.WriteLine("uintArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_uintArray_ArrayIndex(uint[][] val, int index) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(uint[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint[]> f = e.Compile();
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
			
			//-------- Scenario 3193
			namespace Scenario3193{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongArray_ArrayIndex__() {
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
				            success = check_ulongArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongArray_ArrayIndex() {
				        return checkEx_ulongArray_ArrayIndex(null, -1) &
				            checkEx_ulongArray_ArrayIndex(null, 0) &
				            checkEx_ulongArray_ArrayIndex(null, 1) &
				
				            check_ulongArray_ArrayIndex(genArrulongArray_ArrayIndex(0)) &
				            check_ulongArray_ArrayIndex(genArrulongArray_ArrayIndex(1)) &
				            check_ulongArray_ArrayIndex(genArrulongArray_ArrayIndex(5));
				    }
				
				    static ulong[][] genArrulongArray_ArrayIndex(int size) {
				        ulong[][] vals = new ulong[][] { null, new ulong[0], new ulong[] { 0, 1, ulong.MaxValue }, new ulong[100] };
				        ulong[][] result = new ulong[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_ulongArray_ArrayIndex(ulong[][] val) {
				        bool success = checkEx_ulongArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_ulongArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_ulongArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_ulongArray_ArrayIndex(ulong[][] val, int index) {
				        try {
				            check_ulongArray_ArrayIndex(val, index);
				            Console.WriteLine("ulongArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_ulongArray_ArrayIndex(ulong[][] val, int index) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(ulong[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong[]> f = e.Compile();
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
			
			//-------- Scenario 3194
			namespace Scenario3194{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteArray_ArrayIndex__() {
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
				            success = check_sbyteArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteArray_ArrayIndex() {
				        return checkEx_sbyteArray_ArrayIndex(null, -1) &
				            checkEx_sbyteArray_ArrayIndex(null, 0) &
				            checkEx_sbyteArray_ArrayIndex(null, 1) &
				
				            check_sbyteArray_ArrayIndex(genArrsbyteArray_ArrayIndex(0)) &
				            check_sbyteArray_ArrayIndex(genArrsbyteArray_ArrayIndex(1)) &
				            check_sbyteArray_ArrayIndex(genArrsbyteArray_ArrayIndex(5));
				    }
				
				    static sbyte[][] genArrsbyteArray_ArrayIndex(int size) {
				        sbyte[][] vals = new sbyte[][] { null, new sbyte[0], new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }, new sbyte[100] };
				        sbyte[][] result = new sbyte[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_sbyteArray_ArrayIndex(sbyte[][] val) {
				        bool success = checkEx_sbyteArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_sbyteArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_sbyteArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_sbyteArray_ArrayIndex(sbyte[][] val, int index) {
				        try {
				            check_sbyteArray_ArrayIndex(val, index);
				            Console.WriteLine("sbyteArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_sbyteArray_ArrayIndex(sbyte[][] val, int index) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(sbyte[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte[]> f = e.Compile();
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
			
			//-------- Scenario 3195
			namespace Scenario3195{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortArray_ArrayIndex__() {
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
				            success = check_shortArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortArray_ArrayIndex() {
				        return checkEx_shortArray_ArrayIndex(null, -1) &
				            checkEx_shortArray_ArrayIndex(null, 0) &
				            checkEx_shortArray_ArrayIndex(null, 1) &
				
				            check_shortArray_ArrayIndex(genArrshortArray_ArrayIndex(0)) &
				            check_shortArray_ArrayIndex(genArrshortArray_ArrayIndex(1)) &
				            check_shortArray_ArrayIndex(genArrshortArray_ArrayIndex(5));
				    }
				
				    static short[][] genArrshortArray_ArrayIndex(int size) {
				        short[][] vals = new short[][] { null, new short[0], new short[] { 0, 1, -1, short.MinValue, short.MaxValue }, new short[100] };
				        short[][] result = new short[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_shortArray_ArrayIndex(short[][] val) {
				        bool success = checkEx_shortArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_shortArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_shortArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_shortArray_ArrayIndex(short[][] val, int index) {
				        try {
				            check_shortArray_ArrayIndex(val, index);
				            Console.WriteLine("shortArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_shortArray_ArrayIndex(short[][] val, int index) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(short[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short[]> f = e.Compile();
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
			
			//-------- Scenario 3196
			namespace Scenario3196{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intArray_ArrayIndex__() {
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
				            success = check_intArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intArray_ArrayIndex() {
				        return checkEx_intArray_ArrayIndex(null, -1) &
				            checkEx_intArray_ArrayIndex(null, 0) &
				            checkEx_intArray_ArrayIndex(null, 1) &
				
				            check_intArray_ArrayIndex(genArrintArray_ArrayIndex(0)) &
				            check_intArray_ArrayIndex(genArrintArray_ArrayIndex(1)) &
				            check_intArray_ArrayIndex(genArrintArray_ArrayIndex(5));
				    }
				
				    static int[][] genArrintArray_ArrayIndex(int size) {
				        int[][] vals = new int[][] { null, new int[0], new int[] { 0, 1, -1, int.MinValue, int.MaxValue }, new int[100] };
				        int[][] result = new int[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_intArray_ArrayIndex(int[][] val) {
				        bool success = checkEx_intArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_intArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_intArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_intArray_ArrayIndex(int[][] val, int index) {
				        try {
				            check_intArray_ArrayIndex(val, index);
				            Console.WriteLine("intArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_intArray_ArrayIndex(int[][] val, int index) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(int[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int[]> f = e.Compile();
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
			
			//-------- Scenario 3197
			namespace Scenario3197{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longArray_ArrayIndex__() {
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
				            success = check_longArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longArray_ArrayIndex() {
				        return checkEx_longArray_ArrayIndex(null, -1) &
				            checkEx_longArray_ArrayIndex(null, 0) &
				            checkEx_longArray_ArrayIndex(null, 1) &
				
				            check_longArray_ArrayIndex(genArrlongArray_ArrayIndex(0)) &
				            check_longArray_ArrayIndex(genArrlongArray_ArrayIndex(1)) &
				            check_longArray_ArrayIndex(genArrlongArray_ArrayIndex(5));
				    }
				
				    static long[][] genArrlongArray_ArrayIndex(int size) {
				        long[][] vals = new long[][] { null, new long[0], new long[] { 0, 1, -1, long.MinValue, long.MaxValue }, new long[100] };
				        long[][] result = new long[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_longArray_ArrayIndex(long[][] val) {
				        bool success = checkEx_longArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_longArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_longArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_longArray_ArrayIndex(long[][] val, int index) {
				        try {
				            check_longArray_ArrayIndex(val, index);
				            Console.WriteLine("longArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_longArray_ArrayIndex(long[][] val, int index) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(long[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long[]> f = e.Compile();
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
			
			//-------- Scenario 3198
			namespace Scenario3198{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "floatArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression floatArray_ArrayIndex__() {
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
				            success = check_floatArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_floatArray_ArrayIndex() {
				        return checkEx_floatArray_ArrayIndex(null, -1) &
				            checkEx_floatArray_ArrayIndex(null, 0) &
				            checkEx_floatArray_ArrayIndex(null, 1) &
				
				            check_floatArray_ArrayIndex(genArrfloatArray_ArrayIndex(0)) &
				            check_floatArray_ArrayIndex(genArrfloatArray_ArrayIndex(1)) &
				            check_floatArray_ArrayIndex(genArrfloatArray_ArrayIndex(5));
				    }
				
				    static float[][] genArrfloatArray_ArrayIndex(int size) {
				        float[][] vals = new float[][] { null, new float[0], new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN }, new float[100] };
				        float[][] result = new float[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_floatArray_ArrayIndex(float[][] val) {
				        bool success = checkEx_floatArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_floatArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_floatArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_floatArray_ArrayIndex(float[][] val, int index) {
				        try {
				            check_floatArray_ArrayIndex(val, index);
				            Console.WriteLine("floatArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_floatArray_ArrayIndex(float[][] val, int index) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(float[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float[]> f = e.Compile();
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
			
			//-------- Scenario 3199
			namespace Scenario3199{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "doubleArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression doubleArray_ArrayIndex__() {
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
				            success = check_doubleArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_doubleArray_ArrayIndex() {
				        return checkEx_doubleArray_ArrayIndex(null, -1) &
				            checkEx_doubleArray_ArrayIndex(null, 0) &
				            checkEx_doubleArray_ArrayIndex(null, 1) &
				
				            check_doubleArray_ArrayIndex(genArrdoubleArray_ArrayIndex(0)) &
				            check_doubleArray_ArrayIndex(genArrdoubleArray_ArrayIndex(1)) &
				            check_doubleArray_ArrayIndex(genArrdoubleArray_ArrayIndex(5));
				    }
				
				    static double[][] genArrdoubleArray_ArrayIndex(int size) {
				        double[][] vals = new double[][] { null, new double[0], new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN }, new double[100] };
				        double[][] result = new double[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_doubleArray_ArrayIndex(double[][] val) {
				        bool success = checkEx_doubleArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_doubleArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_doubleArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_doubleArray_ArrayIndex(double[][] val, int index) {
				        try {
				            check_doubleArray_ArrayIndex(val, index);
				            Console.WriteLine("doubleArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_doubleArray_ArrayIndex(double[][] val, int index) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(double[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double[]> f = e.Compile();
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
			
			//-------- Scenario 3200
			namespace Scenario3200{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "decimalArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression decimalArray_ArrayIndex__() {
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
				            success = check_decimalArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_decimalArray_ArrayIndex() {
				        return checkEx_decimalArray_ArrayIndex(null, -1) &
				            checkEx_decimalArray_ArrayIndex(null, 0) &
				            checkEx_decimalArray_ArrayIndex(null, 1) &
				
				            check_decimalArray_ArrayIndex(genArrdecimalArray_ArrayIndex(0)) &
				            check_decimalArray_ArrayIndex(genArrdecimalArray_ArrayIndex(1)) &
				            check_decimalArray_ArrayIndex(genArrdecimalArray_ArrayIndex(5));
				    }
				
				    static decimal[][] genArrdecimalArray_ArrayIndex(int size) {
				        decimal[][] vals = new decimal[][] { null, new decimal[0], new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue }, new decimal[100] };
				        decimal[][] result = new decimal[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_decimalArray_ArrayIndex(decimal[][] val) {
				        bool success = checkEx_decimalArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_decimalArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_decimalArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_decimalArray_ArrayIndex(decimal[][] val, int index) {
				        try {
				            check_decimalArray_ArrayIndex(val, index);
				            Console.WriteLine("decimalArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_decimalArray_ArrayIndex(decimal[][] val, int index) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(decimal[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal[]> f = e.Compile();
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
			
			//-------- Scenario 3201
			namespace Scenario3201{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "charArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression charArray_ArrayIndex__() {
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
				            success = check_charArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_charArray_ArrayIndex() {
				        return checkEx_charArray_ArrayIndex(null, -1) &
				            checkEx_charArray_ArrayIndex(null, 0) &
				            checkEx_charArray_ArrayIndex(null, 1) &
				
				            check_charArray_ArrayIndex(genArrcharArray_ArrayIndex(0)) &
				            check_charArray_ArrayIndex(genArrcharArray_ArrayIndex(1)) &
				            check_charArray_ArrayIndex(genArrcharArray_ArrayIndex(5));
				    }
				
				    static char[][] genArrcharArray_ArrayIndex(int size) {
				        char[][] vals = new char[][] { null, new char[0], new char[] { '\0', '\b', 'A', '\uffff' }, new char[100] };
				        char[][] result = new char[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_charArray_ArrayIndex(char[][] val) {
				        bool success = checkEx_charArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_charArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_charArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_charArray_ArrayIndex(char[][] val, int index) {
				        try {
				            check_charArray_ArrayIndex(val, index);
				            Console.WriteLine("charArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_charArray_ArrayIndex(char[][] val, int index) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(char[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char[]> f = e.Compile();
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
			
			//-------- Scenario 3202
			namespace Scenario3202{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "boolArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression boolArray_ArrayIndex__() {
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
				            success = check_boolArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_boolArray_ArrayIndex() {
				        return checkEx_boolArray_ArrayIndex(null, -1) &
				            checkEx_boolArray_ArrayIndex(null, 0) &
				            checkEx_boolArray_ArrayIndex(null, 1) &
				
				            check_boolArray_ArrayIndex(genArrboolArray_ArrayIndex(0)) &
				            check_boolArray_ArrayIndex(genArrboolArray_ArrayIndex(1)) &
				            check_boolArray_ArrayIndex(genArrboolArray_ArrayIndex(5));
				    }
				
				    static bool[][] genArrboolArray_ArrayIndex(int size) {
				        bool[][] vals = new bool[][] { null, new bool[0], new bool[] { true, false }, new bool[100] };
				        bool[][] result = new bool[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_boolArray_ArrayIndex(bool[][] val) {
				        bool success = checkEx_boolArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_boolArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_boolArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_boolArray_ArrayIndex(bool[][] val, int index) {
				        try {
				            check_boolArray_ArrayIndex(val, index);
				            Console.WriteLine("boolArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_boolArray_ArrayIndex(bool[][] val, int index) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(bool[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool[]> f = e.Compile();
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
				
				//-------- Scenario 3203
				namespace Scenario3203{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression SArray_ArrayIndex__() {
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
					            success = check_SArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_SArray_ArrayIndex() {
					        return checkEx_SArray_ArrayIndex(null, -1) &
					            checkEx_SArray_ArrayIndex(null, 0) &
					            checkEx_SArray_ArrayIndex(null, 1) &
					
					            check_SArray_ArrayIndex(genArrSArray_ArrayIndex(0)) &
					            check_SArray_ArrayIndex(genArrSArray_ArrayIndex(1)) &
					            check_SArray_ArrayIndex(genArrSArray_ArrayIndex(5));
					    }
					
					    static S[][] genArrSArray_ArrayIndex(int size) {
					        S[][] vals = new S[][] { null, new S[0], new S[] { default(S), new S() }, new S[100] };
					        S[][] result = new S[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_SArray_ArrayIndex(S[][] val) {
					        bool success = checkEx_SArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_SArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_SArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_SArray_ArrayIndex(S[][] val, int index) {
					        try {
					            check_SArray_ArrayIndex(val, index);
					            Console.WriteLine("SArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_SArray_ArrayIndex(S[][] val, int index) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(S[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3204
				namespace Scenario3204{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SpArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression SpArray_ArrayIndex__() {
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
					            success = check_SpArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_SpArray_ArrayIndex() {
					        return checkEx_SpArray_ArrayIndex(null, -1) &
					            checkEx_SpArray_ArrayIndex(null, 0) &
					            checkEx_SpArray_ArrayIndex(null, 1) &
					
					            check_SpArray_ArrayIndex(genArrSpArray_ArrayIndex(0)) &
					            check_SpArray_ArrayIndex(genArrSpArray_ArrayIndex(1)) &
					            check_SpArray_ArrayIndex(genArrSpArray_ArrayIndex(5));
					    }
					
					    static Sp[][] genArrSpArray_ArrayIndex(int size) {
					        Sp[][] vals = new Sp[][] { null, new Sp[0], new Sp[] { default(Sp), new Sp(), new Sp(5,5.0) }, new Sp[100] };
					        Sp[][] result = new Sp[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_SpArray_ArrayIndex(Sp[][] val) {
					        bool success = checkEx_SpArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_SpArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_SpArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_SpArray_ArrayIndex(Sp[][] val, int index) {
					        try {
					            check_SpArray_ArrayIndex(val, index);
					            Console.WriteLine("SpArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_SpArray_ArrayIndex(Sp[][] val, int index) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Sp[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3205
				namespace Scenario3205{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SsArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression SsArray_ArrayIndex__() {
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
					            success = check_SsArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_SsArray_ArrayIndex() {
					        return checkEx_SsArray_ArrayIndex(null, -1) &
					            checkEx_SsArray_ArrayIndex(null, 0) &
					            checkEx_SsArray_ArrayIndex(null, 1) &
					
					            check_SsArray_ArrayIndex(genArrSsArray_ArrayIndex(0)) &
					            check_SsArray_ArrayIndex(genArrSsArray_ArrayIndex(1)) &
					            check_SsArray_ArrayIndex(genArrSsArray_ArrayIndex(5));
					    }
					
					    static Ss[][] genArrSsArray_ArrayIndex(int size) {
					        Ss[][] vals = new Ss[][] { null, new Ss[0], new Ss[] { default(Ss), new Ss(), new Ss(new S()) }, new Ss[100] };
					        Ss[][] result = new Ss[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_SsArray_ArrayIndex(Ss[][] val) {
					        bool success = checkEx_SsArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_SsArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_SsArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_SsArray_ArrayIndex(Ss[][] val, int index) {
					        try {
					            check_SsArray_ArrayIndex(val, index);
					            Console.WriteLine("SsArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_SsArray_ArrayIndex(Ss[][] val, int index) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Ss[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3206
				namespace Scenario3206{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ScArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ScArray_ArrayIndex__() {
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
					            success = check_ScArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ScArray_ArrayIndex() {
					        return checkEx_ScArray_ArrayIndex(null, -1) &
					            checkEx_ScArray_ArrayIndex(null, 0) &
					            checkEx_ScArray_ArrayIndex(null, 1) &
					
					            check_ScArray_ArrayIndex(genArrScArray_ArrayIndex(0)) &
					            check_ScArray_ArrayIndex(genArrScArray_ArrayIndex(1)) &
					            check_ScArray_ArrayIndex(genArrScArray_ArrayIndex(5));
					    }
					
					    static Sc[][] genArrScArray_ArrayIndex(int size) {
					        Sc[][] vals = new Sc[][] { null, new Sc[0], new Sc[] { default(Sc), new Sc(), new Sc(null) }, new Sc[100] };
					        Sc[][] result = new Sc[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_ScArray_ArrayIndex(Sc[][] val) {
					        bool success = checkEx_ScArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_ScArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_ScArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_ScArray_ArrayIndex(Sc[][] val, int index) {
					        try {
					            check_ScArray_ArrayIndex(val, index);
					            Console.WriteLine("ScArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_ScArray_ArrayIndex(Sc[][] val, int index) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Sc[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3207
				namespace Scenario3207{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ScsArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ScsArray_ArrayIndex__() {
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
					            success = check_ScsArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ScsArray_ArrayIndex() {
					        return checkEx_ScsArray_ArrayIndex(null, -1) &
					            checkEx_ScsArray_ArrayIndex(null, 0) &
					            checkEx_ScsArray_ArrayIndex(null, 1) &
					
					            check_ScsArray_ArrayIndex(genArrScsArray_ArrayIndex(0)) &
					            check_ScsArray_ArrayIndex(genArrScsArray_ArrayIndex(1)) &
					            check_ScsArray_ArrayIndex(genArrScsArray_ArrayIndex(5));
					    }
					
					    static Scs[][] genArrScsArray_ArrayIndex(int size) {
					        Scs[][] vals = new Scs[][] { null, new Scs[0], new Scs[] { default(Scs), new Scs(), new Scs(null,new S()) }, new Scs[100] };
					        Scs[][] result = new Scs[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_ScsArray_ArrayIndex(Scs[][] val) {
					        bool success = checkEx_ScsArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_ScsArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_ScsArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_ScsArray_ArrayIndex(Scs[][] val, int index) {
					        try {
					            check_ScsArray_ArrayIndex(val, index);
					            Console.WriteLine("ScsArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_ScsArray_ArrayIndex(Scs[][] val, int index) {
					        Expression<Func<Scs[]>> e =
					            Expression.Lambda<Func<Scs[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Scs[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3208
				namespace Scenario3208{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TsArray_ArrayIndex_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TsArray_ArrayIndex_S___() {
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
					            success = check_TsArray_ArrayIndex<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TsArray_ArrayIndex<Ts>() where Ts : struct {
					        return checkEx_TsArray_ArrayIndex<Ts>(null, -1) &
					            checkEx_TsArray_ArrayIndex<Ts>(null, 0) &
					            checkEx_TsArray_ArrayIndex<Ts>(null, 1) &
					
					            check_TsArray_ArrayIndex<Ts>(genArrTsArray_ArrayIndex<Ts>(0)) &
					            check_TsArray_ArrayIndex<Ts>(genArrTsArray_ArrayIndex<Ts>(1)) &
					            check_TsArray_ArrayIndex<Ts>(genArrTsArray_ArrayIndex<Ts>(5));
					    }
					
					    static Ts[][] genArrTsArray_ArrayIndex<Ts>(int size) where Ts : struct {
					        Ts[][] vals = new Ts[][] { null, new Ts[0], new Ts[] { default(Ts), new Ts() }, new Ts[100] };
					        Ts[][] result = new Ts[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TsArray_ArrayIndex<Ts>(Ts[][] val) where Ts : struct {
					        bool success = checkEx_TsArray_ArrayIndex<Ts>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TsArray_ArrayIndex<Ts>(val, 0);
					        }
					        success &= checkEx_TsArray_ArrayIndex<Ts>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TsArray_ArrayIndex<Ts>(Ts[][] val, int index) where Ts : struct {
					        try {
					            check_TsArray_ArrayIndex<Ts>(val, index);
					            Console.WriteLine("TsArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TsArray_ArrayIndex<Ts>(Ts[][] val, int index) where Ts : struct {
					        Expression<Func<Ts[]>> e =
					            Expression.Lambda<Func<Ts[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Ts[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3209
				namespace Scenario3209{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TsArray_ArrayIndex_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TsArray_ArrayIndex_Scs___() {
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
					            success = check_TsArray_ArrayIndex<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TsArray_ArrayIndex<Ts>() where Ts : struct {
					        return checkEx_TsArray_ArrayIndex<Ts>(null, -1) &
					            checkEx_TsArray_ArrayIndex<Ts>(null, 0) &
					            checkEx_TsArray_ArrayIndex<Ts>(null, 1) &
					
					            check_TsArray_ArrayIndex<Ts>(genArrTsArray_ArrayIndex<Ts>(0)) &
					            check_TsArray_ArrayIndex<Ts>(genArrTsArray_ArrayIndex<Ts>(1)) &
					            check_TsArray_ArrayIndex<Ts>(genArrTsArray_ArrayIndex<Ts>(5));
					    }
					
					    static Ts[][] genArrTsArray_ArrayIndex<Ts>(int size) where Ts : struct {
					        Ts[][] vals = new Ts[][] { null, new Ts[0], new Ts[] { default(Ts), new Ts() }, new Ts[100] };
					        Ts[][] result = new Ts[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TsArray_ArrayIndex<Ts>(Ts[][] val) where Ts : struct {
					        bool success = checkEx_TsArray_ArrayIndex<Ts>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TsArray_ArrayIndex<Ts>(val, 0);
					        }
					        success &= checkEx_TsArray_ArrayIndex<Ts>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TsArray_ArrayIndex<Ts>(Ts[][] val, int index) where Ts : struct {
					        try {
					            check_TsArray_ArrayIndex<Ts>(val, index);
					            Console.WriteLine("TsArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TsArray_ArrayIndex<Ts>(Ts[][] val, int index) where Ts : struct {
					        Expression<Func<Ts[]>> e =
					            Expression.Lambda<Func<Ts[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Ts[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3210
				namespace Scenario3210{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TsArray_ArrayIndex_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TsArray_ArrayIndex_E___() {
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
					            success = check_TsArray_ArrayIndex<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TsArray_ArrayIndex<Ts>() where Ts : struct {
					        return checkEx_TsArray_ArrayIndex<Ts>(null, -1) &
					            checkEx_TsArray_ArrayIndex<Ts>(null, 0) &
					            checkEx_TsArray_ArrayIndex<Ts>(null, 1) &
					
					            check_TsArray_ArrayIndex<Ts>(genArrTsArray_ArrayIndex<Ts>(0)) &
					            check_TsArray_ArrayIndex<Ts>(genArrTsArray_ArrayIndex<Ts>(1)) &
					            check_TsArray_ArrayIndex<Ts>(genArrTsArray_ArrayIndex<Ts>(5));
					    }
					
					    static Ts[][] genArrTsArray_ArrayIndex<Ts>(int size) where Ts : struct {
					        Ts[][] vals = new Ts[][] { null, new Ts[0], new Ts[] { default(Ts), new Ts() }, new Ts[100] };
					        Ts[][] result = new Ts[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TsArray_ArrayIndex<Ts>(Ts[][] val) where Ts : struct {
					        bool success = checkEx_TsArray_ArrayIndex<Ts>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TsArray_ArrayIndex<Ts>(val, 0);
					        }
					        success &= checkEx_TsArray_ArrayIndex<Ts>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TsArray_ArrayIndex<Ts>(Ts[][] val, int index) where Ts : struct {
					        try {
					            check_TsArray_ArrayIndex<Ts>(val, index);
					            Console.WriteLine("TsArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TsArray_ArrayIndex<Ts>(Ts[][] val, int index) where Ts : struct {
					        Expression<Func<Ts[]>> e =
					            Expression.Lambda<Func<Ts[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Ts[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3211
				namespace Scenario3211{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "EArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression EArray_ArrayIndex__() {
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
					            success = check_EArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_EArray_ArrayIndex() {
					        return checkEx_EArray_ArrayIndex(null, -1) &
					            checkEx_EArray_ArrayIndex(null, 0) &
					            checkEx_EArray_ArrayIndex(null, 1) &
					
					            check_EArray_ArrayIndex(genArrEArray_ArrayIndex(0)) &
					            check_EArray_ArrayIndex(genArrEArray_ArrayIndex(1)) &
					            check_EArray_ArrayIndex(genArrEArray_ArrayIndex(5));
					    }
					
					    static E[][] genArrEArray_ArrayIndex(int size) {
					        E[][] vals = new E[][] { null, new E[0], new E[] { (E) 0, E.A, E.B, (E) int.MaxValue, (E) int.MinValue }, new E[100] };
					        E[][] result = new E[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_EArray_ArrayIndex(E[][] val) {
					        bool success = checkEx_EArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_EArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_EArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_EArray_ArrayIndex(E[][] val, int index) {
					        try {
					            check_EArray_ArrayIndex(val, index);
					            Console.WriteLine("EArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_EArray_ArrayIndex(E[][] val, int index) {
					        Expression<Func<E[]>> e =
					            Expression.Lambda<Func<E[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(E[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<E[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3212
				namespace Scenario3212{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ElArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ElArray_ArrayIndex__() {
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
					            success = check_ElArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ElArray_ArrayIndex() {
					        return checkEx_ElArray_ArrayIndex(null, -1) &
					            checkEx_ElArray_ArrayIndex(null, 0) &
					            checkEx_ElArray_ArrayIndex(null, 1) &
					
					            check_ElArray_ArrayIndex(genArrElArray_ArrayIndex(0)) &
					            check_ElArray_ArrayIndex(genArrElArray_ArrayIndex(1)) &
					            check_ElArray_ArrayIndex(genArrElArray_ArrayIndex(5));
					    }
					
					    static El[][] genArrElArray_ArrayIndex(int size) {
					        El[][] vals = new El[][] { null, new El[0], new El[] { (El) 0, El.A, El.B, (El) long.MaxValue, (El) long.MinValue }, new El[100] };
					        El[][] result = new El[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_ElArray_ArrayIndex(El[][] val) {
					        bool success = checkEx_ElArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_ElArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_ElArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_ElArray_ArrayIndex(El[][] val, int index) {
					        try {
					            check_ElArray_ArrayIndex(val, index);
					            Console.WriteLine("ElArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_ElArray_ArrayIndex(El[][] val, int index) {
					        Expression<Func<El[]>> e =
					            Expression.Lambda<Func<El[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(El[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<El[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
			
			//-------- Scenario 3213
			namespace Scenario3213{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "stringArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression stringArray_ArrayIndex__() {
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
				            success = check_stringArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_stringArray_ArrayIndex() {
				        return checkEx_stringArray_ArrayIndex(null, -1) &
				            checkEx_stringArray_ArrayIndex(null, 0) &
				            checkEx_stringArray_ArrayIndex(null, 1) &
				
				            check_stringArray_ArrayIndex(genArrstringArray_ArrayIndex(0)) &
				            check_stringArray_ArrayIndex(genArrstringArray_ArrayIndex(1)) &
				            check_stringArray_ArrayIndex(genArrstringArray_ArrayIndex(5));
				    }
				
				    static string[][] genArrstringArray_ArrayIndex(int size) {
				        string[][] vals = new string[][] { null, new string[0], new string[] { null, "", "a", "foo" }, new string[100] };
				        string[][] result = new string[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_stringArray_ArrayIndex(string[][] val) {
				        bool success = checkEx_stringArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_stringArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_stringArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_stringArray_ArrayIndex(string[][] val, int index) {
				        try {
				            check_stringArray_ArrayIndex(val, index);
				            Console.WriteLine("stringArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_stringArray_ArrayIndex(string[][] val, int index) {
				        Expression<Func<string[]>> e =
				            Expression.Lambda<Func<string[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(string[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<string[]> f = e.Compile();
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
				
				//-------- Scenario 3214
				namespace Scenario3214{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "objectArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression objectArray_ArrayIndex__() {
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
					            success = check_objectArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_objectArray_ArrayIndex() {
					        return checkEx_objectArray_ArrayIndex(null, -1) &
					            checkEx_objectArray_ArrayIndex(null, 0) &
					            checkEx_objectArray_ArrayIndex(null, 1) &
					
					            check_objectArray_ArrayIndex(genArrobjectArray_ArrayIndex(0)) &
					            check_objectArray_ArrayIndex(genArrobjectArray_ArrayIndex(1)) &
					            check_objectArray_ArrayIndex(genArrobjectArray_ArrayIndex(5));
					    }
					
					    static object[][] genArrobjectArray_ArrayIndex(int size) {
					        object[][] vals = new object[][] { null, new object[0], new object[] { null, new object(), new C(), new D(3) }, new object[100] };
					        object[][] result = new object[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_objectArray_ArrayIndex(object[][] val) {
					        bool success = checkEx_objectArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_objectArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_objectArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_objectArray_ArrayIndex(object[][] val, int index) {
					        try {
					            check_objectArray_ArrayIndex(val, index);
					            Console.WriteLine("objectArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_objectArray_ArrayIndex(object[][] val, int index) {
					        Expression<Func<object[]>> e =
					            Expression.Lambda<Func<object[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(object[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<object[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3215
				namespace Scenario3215{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "CArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression CArray_ArrayIndex__() {
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
					            success = check_CArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_CArray_ArrayIndex() {
					        return checkEx_CArray_ArrayIndex(null, -1) &
					            checkEx_CArray_ArrayIndex(null, 0) &
					            checkEx_CArray_ArrayIndex(null, 1) &
					
					            check_CArray_ArrayIndex(genArrCArray_ArrayIndex(0)) &
					            check_CArray_ArrayIndex(genArrCArray_ArrayIndex(1)) &
					            check_CArray_ArrayIndex(genArrCArray_ArrayIndex(5));
					    }
					
					    static C[][] genArrCArray_ArrayIndex(int size) {
					        C[][] vals = new C[][] { null, new C[0], new C[] { null, new C(), new D(), new D(0), new D(5) }, new C[100] };
					        C[][] result = new C[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_CArray_ArrayIndex(C[][] val) {
					        bool success = checkEx_CArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_CArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_CArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_CArray_ArrayIndex(C[][] val, int index) {
					        try {
					            check_CArray_ArrayIndex(val, index);
					            Console.WriteLine("CArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_CArray_ArrayIndex(C[][] val, int index) {
					        Expression<Func<C[]>> e =
					            Expression.Lambda<Func<C[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(C[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<C[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3216
				namespace Scenario3216{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression DArray_ArrayIndex__() {
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
					            success = check_DArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_DArray_ArrayIndex() {
					        return checkEx_DArray_ArrayIndex(null, -1) &
					            checkEx_DArray_ArrayIndex(null, 0) &
					            checkEx_DArray_ArrayIndex(null, 1) &
					
					            check_DArray_ArrayIndex(genArrDArray_ArrayIndex(0)) &
					            check_DArray_ArrayIndex(genArrDArray_ArrayIndex(1)) &
					            check_DArray_ArrayIndex(genArrDArray_ArrayIndex(5));
					    }
					
					    static D[][] genArrDArray_ArrayIndex(int size) {
					        D[][] vals = new D[][] { null, new D[0], new D[] { null, new D(), new D(0), new D(5) }, new D[100] };
					        D[][] result = new D[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_DArray_ArrayIndex(D[][] val) {
					        bool success = checkEx_DArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_DArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_DArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_DArray_ArrayIndex(D[][] val, int index) {
					        try {
					            check_DArray_ArrayIndex(val, index);
					            Console.WriteLine("DArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_DArray_ArrayIndex(D[][] val, int index) {
					        Expression<Func<D[]>> e =
					            Expression.Lambda<Func<D[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(D[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<D[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3217
				namespace Scenario3217{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TArray_ArrayIndex_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TArray_ArrayIndex_object___() {
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
					            success = check_TArray_ArrayIndex<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TArray_ArrayIndex<T>() {
					        return checkEx_TArray_ArrayIndex<T>(null, -1) &
					            checkEx_TArray_ArrayIndex<T>(null, 0) &
					            checkEx_TArray_ArrayIndex<T>(null, 1) &
					
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(0)) &
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(1)) &
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(5));
					    }
					
					    static T[][] genArrTArray_ArrayIndex<T>(int size) {
					        T[][] vals = new T[][] { null, new T[0], new T[] { default(T) }, new T[100] };
					        T[][] result = new T[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TArray_ArrayIndex<T>(T[][] val) {
					        bool success = checkEx_TArray_ArrayIndex<T>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TArray_ArrayIndex<T>(val, 0);
					        }
					        success &= checkEx_TArray_ArrayIndex<T>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TArray_ArrayIndex<T>(T[][] val, int index) {
					        try {
					            check_TArray_ArrayIndex<T>(val, index);
					            Console.WriteLine("TArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TArray_ArrayIndex<T>(T[][] val, int index) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(T[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3218
				namespace Scenario3218{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TArray_ArrayIndex_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TArray_ArrayIndex_C___() {
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
					            success = check_TArray_ArrayIndex<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TArray_ArrayIndex<T>() {
					        return checkEx_TArray_ArrayIndex<T>(null, -1) &
					            checkEx_TArray_ArrayIndex<T>(null, 0) &
					            checkEx_TArray_ArrayIndex<T>(null, 1) &
					
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(0)) &
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(1)) &
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(5));
					    }
					
					    static T[][] genArrTArray_ArrayIndex<T>(int size) {
					        T[][] vals = new T[][] { null, new T[0], new T[] { default(T) }, new T[100] };
					        T[][] result = new T[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TArray_ArrayIndex<T>(T[][] val) {
					        bool success = checkEx_TArray_ArrayIndex<T>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TArray_ArrayIndex<T>(val, 0);
					        }
					        success &= checkEx_TArray_ArrayIndex<T>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TArray_ArrayIndex<T>(T[][] val, int index) {
					        try {
					            check_TArray_ArrayIndex<T>(val, index);
					            Console.WriteLine("TArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TArray_ArrayIndex<T>(T[][] val, int index) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(T[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3219
				namespace Scenario3219{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TArray_ArrayIndex_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TArray_ArrayIndex_S___() {
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
					            success = check_TArray_ArrayIndex<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TArray_ArrayIndex<T>() {
					        return checkEx_TArray_ArrayIndex<T>(null, -1) &
					            checkEx_TArray_ArrayIndex<T>(null, 0) &
					            checkEx_TArray_ArrayIndex<T>(null, 1) &
					
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(0)) &
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(1)) &
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(5));
					    }
					
					    static T[][] genArrTArray_ArrayIndex<T>(int size) {
					        T[][] vals = new T[][] { null, new T[0], new T[] { default(T) }, new T[100] };
					        T[][] result = new T[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TArray_ArrayIndex<T>(T[][] val) {
					        bool success = checkEx_TArray_ArrayIndex<T>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TArray_ArrayIndex<T>(val, 0);
					        }
					        success &= checkEx_TArray_ArrayIndex<T>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TArray_ArrayIndex<T>(T[][] val, int index) {
					        try {
					            check_TArray_ArrayIndex<T>(val, index);
					            Console.WriteLine("TArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TArray_ArrayIndex<T>(T[][] val, int index) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(T[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3220
				namespace Scenario3220{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TArray_ArrayIndex_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TArray_ArrayIndex_Scs___() {
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
					            success = check_TArray_ArrayIndex<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TArray_ArrayIndex<T>() {
					        return checkEx_TArray_ArrayIndex<T>(null, -1) &
					            checkEx_TArray_ArrayIndex<T>(null, 0) &
					            checkEx_TArray_ArrayIndex<T>(null, 1) &
					
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(0)) &
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(1)) &
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(5));
					    }
					
					    static T[][] genArrTArray_ArrayIndex<T>(int size) {
					        T[][] vals = new T[][] { null, new T[0], new T[] { default(T) }, new T[100] };
					        T[][] result = new T[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TArray_ArrayIndex<T>(T[][] val) {
					        bool success = checkEx_TArray_ArrayIndex<T>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TArray_ArrayIndex<T>(val, 0);
					        }
					        success &= checkEx_TArray_ArrayIndex<T>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TArray_ArrayIndex<T>(T[][] val, int index) {
					        try {
					            check_TArray_ArrayIndex<T>(val, index);
					            Console.WriteLine("TArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TArray_ArrayIndex<T>(T[][] val, int index) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(T[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3221
				namespace Scenario3221{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TArray_ArrayIndex_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TArray_ArrayIndex_E___() {
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
					            success = check_TArray_ArrayIndex<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TArray_ArrayIndex<T>() {
					        return checkEx_TArray_ArrayIndex<T>(null, -1) &
					            checkEx_TArray_ArrayIndex<T>(null, 0) &
					            checkEx_TArray_ArrayIndex<T>(null, 1) &
					
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(0)) &
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(1)) &
					            check_TArray_ArrayIndex<T>(genArrTArray_ArrayIndex<T>(5));
					    }
					
					    static T[][] genArrTArray_ArrayIndex<T>(int size) {
					        T[][] vals = new T[][] { null, new T[0], new T[] { default(T) }, new T[100] };
					        T[][] result = new T[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TArray_ArrayIndex<T>(T[][] val) {
					        bool success = checkEx_TArray_ArrayIndex<T>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TArray_ArrayIndex<T>(val, 0);
					        }
					        success &= checkEx_TArray_ArrayIndex<T>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TArray_ArrayIndex<T>(T[][] val, int index) {
					        try {
					            check_TArray_ArrayIndex<T>(val, index);
					            Console.WriteLine("TArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TArray_ArrayIndex<T>(T[][] val, int index) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(T[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3222
				namespace Scenario3222{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TcArray_ArrayIndex_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TcArray_ArrayIndex_object___() {
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
					            success = check_TcArray_ArrayIndex<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TcArray_ArrayIndex<Tc>() where Tc : class {
					        return checkEx_TcArray_ArrayIndex<Tc>(null, -1) &
					            checkEx_TcArray_ArrayIndex<Tc>(null, 0) &
					            checkEx_TcArray_ArrayIndex<Tc>(null, 1) &
					
					            check_TcArray_ArrayIndex<Tc>(genArrTcArray_ArrayIndex<Tc>(0)) &
					            check_TcArray_ArrayIndex<Tc>(genArrTcArray_ArrayIndex<Tc>(1)) &
					            check_TcArray_ArrayIndex<Tc>(genArrTcArray_ArrayIndex<Tc>(5));
					    }
					
					    static Tc[][] genArrTcArray_ArrayIndex<Tc>(int size) where Tc : class {
					        Tc[][] vals = new Tc[][] { null, new Tc[0], new Tc[] { null, default(Tc) }, new Tc[100] };
					        Tc[][] result = new Tc[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TcArray_ArrayIndex<Tc>(Tc[][] val) where Tc : class {
					        bool success = checkEx_TcArray_ArrayIndex<Tc>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TcArray_ArrayIndex<Tc>(val, 0);
					        }
					        success &= checkEx_TcArray_ArrayIndex<Tc>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TcArray_ArrayIndex<Tc>(Tc[][] val, int index) where Tc : class {
					        try {
					            check_TcArray_ArrayIndex<Tc>(val, index);
					            Console.WriteLine("TcArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TcArray_ArrayIndex<Tc>(Tc[][] val, int index) where Tc : class {
					        Expression<Func<Tc[]>> e =
					            Expression.Lambda<Func<Tc[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Tc[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3223
				namespace Scenario3223{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TcArray_ArrayIndex_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TcArray_ArrayIndex_C___() {
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
					            success = check_TcArray_ArrayIndex<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TcArray_ArrayIndex<Tc>() where Tc : class {
					        return checkEx_TcArray_ArrayIndex<Tc>(null, -1) &
					            checkEx_TcArray_ArrayIndex<Tc>(null, 0) &
					            checkEx_TcArray_ArrayIndex<Tc>(null, 1) &
					
					            check_TcArray_ArrayIndex<Tc>(genArrTcArray_ArrayIndex<Tc>(0)) &
					            check_TcArray_ArrayIndex<Tc>(genArrTcArray_ArrayIndex<Tc>(1)) &
					            check_TcArray_ArrayIndex<Tc>(genArrTcArray_ArrayIndex<Tc>(5));
					    }
					
					    static Tc[][] genArrTcArray_ArrayIndex<Tc>(int size) where Tc : class {
					        Tc[][] vals = new Tc[][] { null, new Tc[0], new Tc[] { null, default(Tc) }, new Tc[100] };
					        Tc[][] result = new Tc[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TcArray_ArrayIndex<Tc>(Tc[][] val) where Tc : class {
					        bool success = checkEx_TcArray_ArrayIndex<Tc>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TcArray_ArrayIndex<Tc>(val, 0);
					        }
					        success &= checkEx_TcArray_ArrayIndex<Tc>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TcArray_ArrayIndex<Tc>(Tc[][] val, int index) where Tc : class {
					        try {
					            check_TcArray_ArrayIndex<Tc>(val, index);
					            Console.WriteLine("TcArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TcArray_ArrayIndex<Tc>(Tc[][] val, int index) where Tc : class {
					        Expression<Func<Tc[]>> e =
					            Expression.Lambda<Func<Tc[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Tc[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3224
				namespace Scenario3224{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TcnArray_ArrayIndex_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TcnArray_ArrayIndex_object___() {
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
					            success = check_TcnArray_ArrayIndex<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TcnArray_ArrayIndex<Tcn>() where Tcn : class, new() {
					        return checkEx_TcnArray_ArrayIndex<Tcn>(null, -1) &
					            checkEx_TcnArray_ArrayIndex<Tcn>(null, 0) &
					            checkEx_TcnArray_ArrayIndex<Tcn>(null, 1) &
					
					            check_TcnArray_ArrayIndex<Tcn>(genArrTcnArray_ArrayIndex<Tcn>(0)) &
					            check_TcnArray_ArrayIndex<Tcn>(genArrTcnArray_ArrayIndex<Tcn>(1)) &
					            check_TcnArray_ArrayIndex<Tcn>(genArrTcnArray_ArrayIndex<Tcn>(5));
					    }
					
					    static Tcn[][] genArrTcnArray_ArrayIndex<Tcn>(int size) where Tcn : class, new() {
					        Tcn[][] vals = new Tcn[][] { null, new Tcn[0], new Tcn[] { null, default(Tcn), new Tcn() }, new Tcn[100] };
					        Tcn[][] result = new Tcn[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TcnArray_ArrayIndex<Tcn>(Tcn[][] val) where Tcn : class, new() {
					        bool success = checkEx_TcnArray_ArrayIndex<Tcn>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TcnArray_ArrayIndex<Tcn>(val, 0);
					        }
					        success &= checkEx_TcnArray_ArrayIndex<Tcn>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TcnArray_ArrayIndex<Tcn>(Tcn[][] val, int index) where Tcn : class, new() {
					        try {
					            check_TcnArray_ArrayIndex<Tcn>(val, index);
					            Console.WriteLine("TcnArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TcnArray_ArrayIndex<Tcn>(Tcn[][] val, int index) where Tcn : class, new() {
					        Expression<Func<Tcn[]>> e =
					            Expression.Lambda<Func<Tcn[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Tcn[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3225
				namespace Scenario3225{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TcnArray_ArrayIndex_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TcnArray_ArrayIndex_C___() {
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
					            success = check_TcnArray_ArrayIndex<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TcnArray_ArrayIndex<Tcn>() where Tcn : class, new() {
					        return checkEx_TcnArray_ArrayIndex<Tcn>(null, -1) &
					            checkEx_TcnArray_ArrayIndex<Tcn>(null, 0) &
					            checkEx_TcnArray_ArrayIndex<Tcn>(null, 1) &
					
					            check_TcnArray_ArrayIndex<Tcn>(genArrTcnArray_ArrayIndex<Tcn>(0)) &
					            check_TcnArray_ArrayIndex<Tcn>(genArrTcnArray_ArrayIndex<Tcn>(1)) &
					            check_TcnArray_ArrayIndex<Tcn>(genArrTcnArray_ArrayIndex<Tcn>(5));
					    }
					
					    static Tcn[][] genArrTcnArray_ArrayIndex<Tcn>(int size) where Tcn : class, new() {
					        Tcn[][] vals = new Tcn[][] { null, new Tcn[0], new Tcn[] { null, default(Tcn), new Tcn() }, new Tcn[100] };
					        Tcn[][] result = new Tcn[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TcnArray_ArrayIndex<Tcn>(Tcn[][] val) where Tcn : class, new() {
					        bool success = checkEx_TcnArray_ArrayIndex<Tcn>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TcnArray_ArrayIndex<Tcn>(val, 0);
					        }
					        success &= checkEx_TcnArray_ArrayIndex<Tcn>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TcnArray_ArrayIndex<Tcn>(Tcn[][] val, int index) where Tcn : class, new() {
					        try {
					            check_TcnArray_ArrayIndex<Tcn>(val, index);
					            Console.WriteLine("TcnArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TcnArray_ArrayIndex<Tcn>(Tcn[][] val, int index) where Tcn : class, new() {
					        Expression<Func<Tcn[]>> e =
					            Expression.Lambda<Func<Tcn[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Tcn[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3226
				namespace Scenario3226{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TCArray_ArrayIndex_C_a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TCArray_ArrayIndex_C_a__() {
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
					            success = check_TCArray_ArrayIndex<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TCArray_ArrayIndex<TC>() where TC : C {
					        return checkEx_TCArray_ArrayIndex<TC>(null, -1) &
					            checkEx_TCArray_ArrayIndex<TC>(null, 0) &
					            checkEx_TCArray_ArrayIndex<TC>(null, 1) &
					
					            check_TCArray_ArrayIndex<TC>(genArrTCArray_ArrayIndex<TC>(0)) &
					            check_TCArray_ArrayIndex<TC>(genArrTCArray_ArrayIndex<TC>(1)) &
					            check_TCArray_ArrayIndex<TC>(genArrTCArray_ArrayIndex<TC>(5));
					    }
					
					    static TC[][] genArrTCArray_ArrayIndex<TC>(int size) where TC : C {
					        TC[][] vals = new TC[][] { null, new TC[0], new TC[] { null, default(TC), (TC) new C() }, new TC[100] };
					        TC[][] result = new TC[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TCArray_ArrayIndex<TC>(TC[][] val) where TC : C {
					        bool success = checkEx_TCArray_ArrayIndex<TC>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TCArray_ArrayIndex<TC>(val, 0);
					        }
					        success &= checkEx_TCArray_ArrayIndex<TC>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TCArray_ArrayIndex<TC>(TC[][] val, int index) where TC : C {
					        try {
					            check_TCArray_ArrayIndex<TC>(val, index);
					            Console.WriteLine("TCArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TCArray_ArrayIndex<TC>(TC[][] val, int index) where TC : C {
					        Expression<Func<TC[]>> e =
					            Expression.Lambda<Func<TC[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(TC[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<TC[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3227
				namespace Scenario3227{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TCnArray_ArrayIndex_C_a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TCnArray_ArrayIndex_C_a__() {
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
					            success = check_TCnArray_ArrayIndex<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TCnArray_ArrayIndex<TCn>() where TCn : C, new() {
					        return checkEx_TCnArray_ArrayIndex<TCn>(null, -1) &
					            checkEx_TCnArray_ArrayIndex<TCn>(null, 0) &
					            checkEx_TCnArray_ArrayIndex<TCn>(null, 1) &
					
					            check_TCnArray_ArrayIndex<TCn>(genArrTCnArray_ArrayIndex<TCn>(0)) &
					            check_TCnArray_ArrayIndex<TCn>(genArrTCnArray_ArrayIndex<TCn>(1)) &
					            check_TCnArray_ArrayIndex<TCn>(genArrTCnArray_ArrayIndex<TCn>(5));
					    }
					
					    static TCn[][] genArrTCnArray_ArrayIndex<TCn>(int size) where TCn : C, new() {
					        TCn[][] vals = new TCn[][] { null, new TCn[0], new TCn[] { null, default(TCn), new TCn(), (TCn) new C() }, new TCn[100] };
					        TCn[][] result = new TCn[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_TCnArray_ArrayIndex<TCn>(TCn[][] val) where TCn : C, new() {
					        bool success = checkEx_TCnArray_ArrayIndex<TCn>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_TCnArray_ArrayIndex<TCn>(val, 0);
					        }
					        success &= checkEx_TCnArray_ArrayIndex<TCn>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_TCnArray_ArrayIndex<TCn>(TCn[][] val, int index) where TCn : C, new() {
					        try {
					            check_TCnArray_ArrayIndex<TCn>(val, index);
					            Console.WriteLine("TCnArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_TCnArray_ArrayIndex<TCn>(TCn[][] val, int index) where TCn : C, new() {
					        Expression<Func<TCn[]>> e =
					            Expression.Lambda<Func<TCn[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(TCn[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<TCn[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
			
			//-------- Scenario 3228
			namespace Scenario3228{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DelegateArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression DelegateArray_ArrayIndex__() {
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
				            success = check_DelegateArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_DelegateArray_ArrayIndex() {
				        return checkEx_DelegateArray_ArrayIndex(null, -1) &
				            checkEx_DelegateArray_ArrayIndex(null, 0) &
				            checkEx_DelegateArray_ArrayIndex(null, 1) &
				
				            check_DelegateArray_ArrayIndex(genArrDelegateArray_ArrayIndex(0)) &
				            check_DelegateArray_ArrayIndex(genArrDelegateArray_ArrayIndex(1)) &
				            check_DelegateArray_ArrayIndex(genArrDelegateArray_ArrayIndex(5));
				    }
				
				    static Delegate[][] genArrDelegateArray_ArrayIndex(int size) {
				        Delegate[][] vals = new Delegate[][] { null, new Delegate[0], new Delegate[] { null, (Func<object>) delegate() { return null; }, (Func<int, int>) delegate(int i) { return i+1; }, (Action<object>) delegate { } }, new Delegate[100] };
				        Delegate[][] result = new Delegate[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_DelegateArray_ArrayIndex(Delegate[][] val) {
				        bool success = checkEx_DelegateArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_DelegateArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_DelegateArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_DelegateArray_ArrayIndex(Delegate[][] val, int index) {
				        try {
				            check_DelegateArray_ArrayIndex(val, index);
				            Console.WriteLine("DelegateArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_DelegateArray_ArrayIndex(Delegate[][] val, int index) {
				        Expression<Func<Delegate[]>> e =
				            Expression.Lambda<Func<Delegate[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(Delegate[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<Delegate[]> f = e.Compile();
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
			
			//-------- Scenario 3229
			namespace Scenario3229{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Func_objectArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Func_objectArray_ArrayIndex__() {
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
				            success = check_Func_objectArray_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_Func_objectArray_ArrayIndex() {
				        return checkEx_Func_objectArray_ArrayIndex(null, -1) &
				            checkEx_Func_objectArray_ArrayIndex(null, 0) &
				            checkEx_Func_objectArray_ArrayIndex(null, 1) &
				
				            check_Func_objectArray_ArrayIndex(genArrFunc_objectArray_ArrayIndex(0)) &
				            check_Func_objectArray_ArrayIndex(genArrFunc_objectArray_ArrayIndex(1)) &
				            check_Func_objectArray_ArrayIndex(genArrFunc_objectArray_ArrayIndex(5));
				    }
				
				    static Func<object>[][] genArrFunc_objectArray_ArrayIndex(int size) {
				        Func<object>[][] vals = new Func<object>[][] { null, new Func<object>[0], new Func<object>[] { null, (Func<object>) delegate() { return null; } }, new Func<object>[100] };
				        Func<object>[][] result = new Func<object>[size][];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_Func_objectArray_ArrayIndex(Func<object>[][] val) {
				        bool success = checkEx_Func_objectArray_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_Func_objectArray_ArrayIndex(val, 0);
				        }
				        success &= checkEx_Func_objectArray_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_Func_objectArray_ArrayIndex(Func<object>[][] val, int index) {
				        try {
				            check_Func_objectArray_ArrayIndex(val, index);
				            Console.WriteLine("Func_objectArray_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_Func_objectArray_ArrayIndex(Func<object>[][] val, int index) {
				        Expression<Func<Func<object>[]>> e =
				            Expression.Lambda<Func<Func<object>[]>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(Func<object>[][])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<object>[]> f = e.Compile();
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
				
				//-------- Scenario 3230
				namespace Scenario3230{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IEquatable_CArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression IEquatable_CArray_ArrayIndex__() {
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
					            success = check_IEquatable_CArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_IEquatable_CArray_ArrayIndex() {
					        return checkEx_IEquatable_CArray_ArrayIndex(null, -1) &
					            checkEx_IEquatable_CArray_ArrayIndex(null, 0) &
					            checkEx_IEquatable_CArray_ArrayIndex(null, 1) &
					
					            check_IEquatable_CArray_ArrayIndex(genArrIEquatable_CArray_ArrayIndex(0)) &
					            check_IEquatable_CArray_ArrayIndex(genArrIEquatable_CArray_ArrayIndex(1)) &
					            check_IEquatable_CArray_ArrayIndex(genArrIEquatable_CArray_ArrayIndex(5));
					    }
					
					    static IEquatable<C>[][] genArrIEquatable_CArray_ArrayIndex(int size) {
					        IEquatable<C>[][] vals = new IEquatable<C>[][] { null, new IEquatable<C>[0], new IEquatable<C>[] { null, new C(), new D(), new D(0), new D(5) }, new IEquatable<C>[100] };
					        IEquatable<C>[][] result = new IEquatable<C>[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_IEquatable_CArray_ArrayIndex(IEquatable<C>[][] val) {
					        bool success = checkEx_IEquatable_CArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_IEquatable_CArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_IEquatable_CArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_IEquatable_CArray_ArrayIndex(IEquatable<C>[][] val, int index) {
					        try {
					            check_IEquatable_CArray_ArrayIndex(val, index);
					            Console.WriteLine("IEquatable_CArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_IEquatable_CArray_ArrayIndex(IEquatable<C>[][] val, int index) {
					        Expression<Func<IEquatable<C>[]>> e =
					            Expression.Lambda<Func<IEquatable<C>[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(IEquatable<C>[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<C>[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3231
				namespace Scenario3231{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IEquatable_DArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression IEquatable_DArray_ArrayIndex__() {
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
					            success = check_IEquatable_DArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_IEquatable_DArray_ArrayIndex() {
					        return checkEx_IEquatable_DArray_ArrayIndex(null, -1) &
					            checkEx_IEquatable_DArray_ArrayIndex(null, 0) &
					            checkEx_IEquatable_DArray_ArrayIndex(null, 1) &
					
					            check_IEquatable_DArray_ArrayIndex(genArrIEquatable_DArray_ArrayIndex(0)) &
					            check_IEquatable_DArray_ArrayIndex(genArrIEquatable_DArray_ArrayIndex(1)) &
					            check_IEquatable_DArray_ArrayIndex(genArrIEquatable_DArray_ArrayIndex(5));
					    }
					
					    static IEquatable<D>[][] genArrIEquatable_DArray_ArrayIndex(int size) {
					        IEquatable<D>[][] vals = new IEquatable<D>[][] { null, new IEquatable<D>[0], new IEquatable<D>[] { null, new D(), new D(0), new D(5) }, new IEquatable<D>[100] };
					        IEquatable<D>[][] result = new IEquatable<D>[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_IEquatable_DArray_ArrayIndex(IEquatable<D>[][] val) {
					        bool success = checkEx_IEquatable_DArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_IEquatable_DArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_IEquatable_DArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_IEquatable_DArray_ArrayIndex(IEquatable<D>[][] val, int index) {
					        try {
					            check_IEquatable_DArray_ArrayIndex(val, index);
					            Console.WriteLine("IEquatable_DArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_IEquatable_DArray_ArrayIndex(IEquatable<D>[][] val, int index) {
					        Expression<Func<IEquatable<D>[]>> e =
					            Expression.Lambda<Func<IEquatable<D>[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(IEquatable<D>[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<D>[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
				
				//-------- Scenario 3232
				namespace Scenario3232{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IArray_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression IArray_ArrayIndex__() {
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
					            success = check_IArray_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_IArray_ArrayIndex() {
					        return checkEx_IArray_ArrayIndex(null, -1) &
					            checkEx_IArray_ArrayIndex(null, 0) &
					            checkEx_IArray_ArrayIndex(null, 1) &
					
					            check_IArray_ArrayIndex(genArrIArray_ArrayIndex(0)) &
					            check_IArray_ArrayIndex(genArrIArray_ArrayIndex(1)) &
					            check_IArray_ArrayIndex(genArrIArray_ArrayIndex(5));
					    }
					
					    static I[][] genArrIArray_ArrayIndex(int size) {
					        I[][] vals = new I[][] { null, new I[0], new I[] { null, new C(), new D(), new D(0), new D(5) }, new I[100] };
					        I[][] result = new I[size][];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_IArray_ArrayIndex(I[][] val) {
					        bool success = checkEx_IArray_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_IArray_ArrayIndex(val, 0);
					        }
					        success &= checkEx_IArray_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_IArray_ArrayIndex(I[][] val, int index) {
					        try {
					            check_IArray_ArrayIndex(val, index);
					            Console.WriteLine("IArray_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_IArray_ArrayIndex(I[][] val, int index) {
					        Expression<Func<I[]>> e =
					            Expression.Lambda<Func<I[]>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(I[][])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<I[]> f = e.Compile();
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
				
			
			
			
			public interface I {
			  void M();
			}
			
			public  class C : IEquatable<C>, I {
			    void I.M() {
			    }
			
			    public override bool Equals(object o) {
			        return o is C && Equals((C) o);
			    }
			
			    public bool Equals(C c) {
			        return c != null;
			    }
			
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public  class D : C, IEquatable<D> {
			    public int Val;
			    public D() {
			    }
			    public D(int val) {
			        Val = val;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val;
			    }
			
			    public override int GetHashCode() {
			        return Val;
			    }
			}
			
			public enum E {
			  A=1, B=2
			}
			
			public enum El : long {
			  A, B, C
			}
			
			public struct S : IEquatable<S> {
			    public override bool Equals(object o) {
			        return (o is S) && Equals((S) o);
			    }
			    public bool Equals(S other) {
			        return true;
			    }
			    public override int GetHashCode() {
			        return 0;
			    }
			}
			
			public struct Sp : IEquatable<Sp> {
			    public Sp(int i, double d) {
			        I = i;
			        D = d;
			    }
			
			    public int I;
			    public double D;
			
			    public override bool Equals(object o) {
			        return (o is Sp) && Equals((Sp) o);
			    }
			    public bool Equals(Sp other) {
			        return other.I == I && other.D == D;
			    }
			    public override int GetHashCode() {
			        return I.GetHashCode() ^ D.GetHashCode();
			    }
			}
			
			public struct Ss : IEquatable<Ss> {
			    public Ss(S s) {
			        Val = s;
			    }
			
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Ss) && Equals((Ss) o);
			    }
			    public bool Equals(Ss other) {
			        return other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return Val.GetHashCode();
			    }
			}
			
			public struct Sc : IEquatable<Sc> {
			    public Sc(string s) {
			        S = s;
			    }
			
			    public string S;
			
			    public override bool Equals(object o) {
			        return (o is Sc) && Equals((Sc) o);
			    }
			    public bool Equals(Sc other) {
			        return other.S == S;
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode();
			    }
			}
			
			public struct Scs : IEquatable<Scs> {
			    public Scs(string s, S val) {
			        S = s;
			        Val = val;
			    }
			
			    public string S;
			    public S Val;
			
			    public override bool Equals(object o) {
			        return (o is Scs) && Equals((Scs) o);
			    }
			    public bool Equals(Scs other) {
			        return other.S == S && other.Val.Equals(Val);
			    }
			    public override int GetHashCode() {
			        return S.GetHashCode() ^ Val.GetHashCode();
			    }
			}
			
		
	}
	
}
