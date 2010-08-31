#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 3167
			namespace Scenario3167{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_ArrayIndex__() {
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
				            success = check_byteq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_ArrayIndex() {
				        return checkEx_byteq_ArrayIndex(null, -1) &
				            checkEx_byteq_ArrayIndex(null, 0) &
				            checkEx_byteq_ArrayIndex(null, 1) &
				
				            check_byteq_ArrayIndex(genArrbyteq_ArrayIndex(0)) &
				            check_byteq_ArrayIndex(genArrbyteq_ArrayIndex(1)) &
				            check_byteq_ArrayIndex(genArrbyteq_ArrayIndex(5));
				    }
				
				    static byte?[] genArrbyteq_ArrayIndex(int size) {
				        byte?[] vals = new byte?[] { 0, 1, byte.MaxValue };
				        byte?[] result = new byte?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_byteq_ArrayIndex(byte?[] val) {
				        bool success = checkEx_byteq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_byteq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_byteq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_byteq_ArrayIndex(byte?[] val, int index) {
				        try {
				            check_byteq_ArrayIndex(val, index);
				            Console.WriteLine("byteq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_byteq_ArrayIndex(byte?[] val, int index) {
				        Expression<Func<byte?>> e =
				            Expression.Lambda<Func<byte?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(byte?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte?> f = e.Compile();
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
			
			//-------- Scenario 3168
			namespace Scenario3168{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_ArrayIndex__() {
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
				            success = check_ushortq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_ArrayIndex() {
				        return checkEx_ushortq_ArrayIndex(null, -1) &
				            checkEx_ushortq_ArrayIndex(null, 0) &
				            checkEx_ushortq_ArrayIndex(null, 1) &
				
				            check_ushortq_ArrayIndex(genArrushortq_ArrayIndex(0)) &
				            check_ushortq_ArrayIndex(genArrushortq_ArrayIndex(1)) &
				            check_ushortq_ArrayIndex(genArrushortq_ArrayIndex(5));
				    }
				
				    static ushort?[] genArrushortq_ArrayIndex(int size) {
				        ushort?[] vals = new ushort?[] { 0, 1, ushort.MaxValue };
				        ushort?[] result = new ushort?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_ushortq_ArrayIndex(ushort?[] val) {
				        bool success = checkEx_ushortq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_ushortq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_ushortq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_ushortq_ArrayIndex(ushort?[] val, int index) {
				        try {
				            check_ushortq_ArrayIndex(val, index);
				            Console.WriteLine("ushortq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_ushortq_ArrayIndex(ushort?[] val, int index) {
				        Expression<Func<ushort?>> e =
				            Expression.Lambda<Func<ushort?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(ushort?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort?> f = e.Compile();
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
			
			//-------- Scenario 3169
			namespace Scenario3169{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_ArrayIndex__() {
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
				            success = check_uintq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_ArrayIndex() {
				        return checkEx_uintq_ArrayIndex(null, -1) &
				            checkEx_uintq_ArrayIndex(null, 0) &
				            checkEx_uintq_ArrayIndex(null, 1) &
				
				            check_uintq_ArrayIndex(genArruintq_ArrayIndex(0)) &
				            check_uintq_ArrayIndex(genArruintq_ArrayIndex(1)) &
				            check_uintq_ArrayIndex(genArruintq_ArrayIndex(5));
				    }
				
				    static uint?[] genArruintq_ArrayIndex(int size) {
				        uint?[] vals = new uint?[] { 0, 1, uint.MaxValue };
				        uint?[] result = new uint?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_uintq_ArrayIndex(uint?[] val) {
				        bool success = checkEx_uintq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_uintq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_uintq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_uintq_ArrayIndex(uint?[] val, int index) {
				        try {
				            check_uintq_ArrayIndex(val, index);
				            Console.WriteLine("uintq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_uintq_ArrayIndex(uint?[] val, int index) {
				        Expression<Func<uint?>> e =
				            Expression.Lambda<Func<uint?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(uint?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint?> f = e.Compile();
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
			
			//-------- Scenario 3170
			namespace Scenario3170{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_ArrayIndex__() {
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
				            success = check_ulongq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_ArrayIndex() {
				        return checkEx_ulongq_ArrayIndex(null, -1) &
				            checkEx_ulongq_ArrayIndex(null, 0) &
				            checkEx_ulongq_ArrayIndex(null, 1) &
				
				            check_ulongq_ArrayIndex(genArrulongq_ArrayIndex(0)) &
				            check_ulongq_ArrayIndex(genArrulongq_ArrayIndex(1)) &
				            check_ulongq_ArrayIndex(genArrulongq_ArrayIndex(5));
				    }
				
				    static ulong?[] genArrulongq_ArrayIndex(int size) {
				        ulong?[] vals = new ulong?[] { 0, 1, ulong.MaxValue };
				        ulong?[] result = new ulong?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_ulongq_ArrayIndex(ulong?[] val) {
				        bool success = checkEx_ulongq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_ulongq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_ulongq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_ulongq_ArrayIndex(ulong?[] val, int index) {
				        try {
				            check_ulongq_ArrayIndex(val, index);
				            Console.WriteLine("ulongq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_ulongq_ArrayIndex(ulong?[] val, int index) {
				        Expression<Func<ulong?>> e =
				            Expression.Lambda<Func<ulong?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(ulong?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong?> f = e.Compile();
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
			
			//-------- Scenario 3171
			namespace Scenario3171{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_ArrayIndex__() {
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
				            success = check_sbyteq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_ArrayIndex() {
				        return checkEx_sbyteq_ArrayIndex(null, -1) &
				            checkEx_sbyteq_ArrayIndex(null, 0) &
				            checkEx_sbyteq_ArrayIndex(null, 1) &
				
				            check_sbyteq_ArrayIndex(genArrsbyteq_ArrayIndex(0)) &
				            check_sbyteq_ArrayIndex(genArrsbyteq_ArrayIndex(1)) &
				            check_sbyteq_ArrayIndex(genArrsbyteq_ArrayIndex(5));
				    }
				
				    static sbyte?[] genArrsbyteq_ArrayIndex(int size) {
				        sbyte?[] vals = new sbyte?[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        sbyte?[] result = new sbyte?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_sbyteq_ArrayIndex(sbyte?[] val) {
				        bool success = checkEx_sbyteq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_sbyteq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_sbyteq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_sbyteq_ArrayIndex(sbyte?[] val, int index) {
				        try {
				            check_sbyteq_ArrayIndex(val, index);
				            Console.WriteLine("sbyteq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_sbyteq_ArrayIndex(sbyte?[] val, int index) {
				        Expression<Func<sbyte?>> e =
				            Expression.Lambda<Func<sbyte?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(sbyte?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte?> f = e.Compile();
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
			
			//-------- Scenario 3172
			namespace Scenario3172{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_ArrayIndex__() {
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
				            success = check_shortq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_ArrayIndex() {
				        return checkEx_shortq_ArrayIndex(null, -1) &
				            checkEx_shortq_ArrayIndex(null, 0) &
				            checkEx_shortq_ArrayIndex(null, 1) &
				
				            check_shortq_ArrayIndex(genArrshortq_ArrayIndex(0)) &
				            check_shortq_ArrayIndex(genArrshortq_ArrayIndex(1)) &
				            check_shortq_ArrayIndex(genArrshortq_ArrayIndex(5));
				    }
				
				    static short?[] genArrshortq_ArrayIndex(int size) {
				        short?[] vals = new short?[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        short?[] result = new short?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_shortq_ArrayIndex(short?[] val) {
				        bool success = checkEx_shortq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_shortq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_shortq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_shortq_ArrayIndex(short?[] val, int index) {
				        try {
				            check_shortq_ArrayIndex(val, index);
				            Console.WriteLine("shortq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_shortq_ArrayIndex(short?[] val, int index) {
				        Expression<Func<short?>> e =
				            Expression.Lambda<Func<short?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(short?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short?> f = e.Compile();
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
			
			//-------- Scenario 3173
			namespace Scenario3173{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_ArrayIndex__() {
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
				            success = check_intq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_ArrayIndex() {
				        return checkEx_intq_ArrayIndex(null, -1) &
				            checkEx_intq_ArrayIndex(null, 0) &
				            checkEx_intq_ArrayIndex(null, 1) &
				
				            check_intq_ArrayIndex(genArrintq_ArrayIndex(0)) &
				            check_intq_ArrayIndex(genArrintq_ArrayIndex(1)) &
				            check_intq_ArrayIndex(genArrintq_ArrayIndex(5));
				    }
				
				    static int?[] genArrintq_ArrayIndex(int size) {
				        int?[] vals = new int?[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        int?[] result = new int?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_intq_ArrayIndex(int?[] val) {
				        bool success = checkEx_intq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_intq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_intq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_intq_ArrayIndex(int?[] val, int index) {
				        try {
				            check_intq_ArrayIndex(val, index);
				            Console.WriteLine("intq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_intq_ArrayIndex(int?[] val, int index) {
				        Expression<Func<int?>> e =
				            Expression.Lambda<Func<int?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(int?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int?> f = e.Compile();
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
			
			//-------- Scenario 3174
			namespace Scenario3174{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_ArrayIndex__() {
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
				            success = check_longq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_ArrayIndex() {
				        return checkEx_longq_ArrayIndex(null, -1) &
				            checkEx_longq_ArrayIndex(null, 0) &
				            checkEx_longq_ArrayIndex(null, 1) &
				
				            check_longq_ArrayIndex(genArrlongq_ArrayIndex(0)) &
				            check_longq_ArrayIndex(genArrlongq_ArrayIndex(1)) &
				            check_longq_ArrayIndex(genArrlongq_ArrayIndex(5));
				    }
				
				    static long?[] genArrlongq_ArrayIndex(int size) {
				        long?[] vals = new long?[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        long?[] result = new long?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_longq_ArrayIndex(long?[] val) {
				        bool success = checkEx_longq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_longq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_longq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_longq_ArrayIndex(long?[] val, int index) {
				        try {
				            check_longq_ArrayIndex(val, index);
				            Console.WriteLine("longq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_longq_ArrayIndex(long?[] val, int index) {
				        Expression<Func<long?>> e =
				            Expression.Lambda<Func<long?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(long?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long?> f = e.Compile();
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
			
			//-------- Scenario 3175
			namespace Scenario3175{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "floatq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression floatq_ArrayIndex__() {
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
				            success = check_floatq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_floatq_ArrayIndex() {
				        return checkEx_floatq_ArrayIndex(null, -1) &
				            checkEx_floatq_ArrayIndex(null, 0) &
				            checkEx_floatq_ArrayIndex(null, 1) &
				
				            check_floatq_ArrayIndex(genArrfloatq_ArrayIndex(0)) &
				            check_floatq_ArrayIndex(genArrfloatq_ArrayIndex(1)) &
				            check_floatq_ArrayIndex(genArrfloatq_ArrayIndex(5));
				    }
				
				    static float?[] genArrfloatq_ArrayIndex(int size) {
				        float?[] vals = new float?[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        float?[] result = new float?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_floatq_ArrayIndex(float?[] val) {
				        bool success = checkEx_floatq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_floatq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_floatq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_floatq_ArrayIndex(float?[] val, int index) {
				        try {
				            check_floatq_ArrayIndex(val, index);
				            Console.WriteLine("floatq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_floatq_ArrayIndex(float?[] val, int index) {
				        Expression<Func<float?>> e =
				            Expression.Lambda<Func<float?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(float?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float?> f = e.Compile();
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
			
			//-------- Scenario 3176
			namespace Scenario3176{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "doubleq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression doubleq_ArrayIndex__() {
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
				            success = check_doubleq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_doubleq_ArrayIndex() {
				        return checkEx_doubleq_ArrayIndex(null, -1) &
				            checkEx_doubleq_ArrayIndex(null, 0) &
				            checkEx_doubleq_ArrayIndex(null, 1) &
				
				            check_doubleq_ArrayIndex(genArrdoubleq_ArrayIndex(0)) &
				            check_doubleq_ArrayIndex(genArrdoubleq_ArrayIndex(1)) &
				            check_doubleq_ArrayIndex(genArrdoubleq_ArrayIndex(5));
				    }
				
				    static double?[] genArrdoubleq_ArrayIndex(int size) {
				        double?[] vals = new double?[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        double?[] result = new double?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_doubleq_ArrayIndex(double?[] val) {
				        bool success = checkEx_doubleq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_doubleq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_doubleq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_doubleq_ArrayIndex(double?[] val, int index) {
				        try {
				            check_doubleq_ArrayIndex(val, index);
				            Console.WriteLine("doubleq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_doubleq_ArrayIndex(double?[] val, int index) {
				        Expression<Func<double?>> e =
				            Expression.Lambda<Func<double?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(double?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double?> f = e.Compile();
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
			
			//-------- Scenario 3177
			namespace Scenario3177{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "decimalq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression decimalq_ArrayIndex__() {
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
				            success = check_decimalq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_decimalq_ArrayIndex() {
				        return checkEx_decimalq_ArrayIndex(null, -1) &
				            checkEx_decimalq_ArrayIndex(null, 0) &
				            checkEx_decimalq_ArrayIndex(null, 1) &
				
				            check_decimalq_ArrayIndex(genArrdecimalq_ArrayIndex(0)) &
				            check_decimalq_ArrayIndex(genArrdecimalq_ArrayIndex(1)) &
				            check_decimalq_ArrayIndex(genArrdecimalq_ArrayIndex(5));
				    }
				
				    static decimal?[] genArrdecimalq_ArrayIndex(int size) {
				        decimal?[] vals = new decimal?[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        decimal?[] result = new decimal?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_decimalq_ArrayIndex(decimal?[] val) {
				        bool success = checkEx_decimalq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_decimalq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_decimalq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_decimalq_ArrayIndex(decimal?[] val, int index) {
				        try {
				            check_decimalq_ArrayIndex(val, index);
				            Console.WriteLine("decimalq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_decimalq_ArrayIndex(decimal?[] val, int index) {
				        Expression<Func<decimal?>> e =
				            Expression.Lambda<Func<decimal?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(decimal?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal?> f = e.Compile();
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
			
			//-------- Scenario 3178
			namespace Scenario3178{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "charq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression charq_ArrayIndex__() {
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
				            success = check_charq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_charq_ArrayIndex() {
				        return checkEx_charq_ArrayIndex(null, -1) &
				            checkEx_charq_ArrayIndex(null, 0) &
				            checkEx_charq_ArrayIndex(null, 1) &
				
				            check_charq_ArrayIndex(genArrcharq_ArrayIndex(0)) &
				            check_charq_ArrayIndex(genArrcharq_ArrayIndex(1)) &
				            check_charq_ArrayIndex(genArrcharq_ArrayIndex(5));
				    }
				
				    static char?[] genArrcharq_ArrayIndex(int size) {
				        char?[] vals = new char?[] { '\0', '\b', 'A', '\uffff' };
				        char?[] result = new char?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_charq_ArrayIndex(char?[] val) {
				        bool success = checkEx_charq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_charq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_charq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_charq_ArrayIndex(char?[] val, int index) {
				        try {
				            check_charq_ArrayIndex(val, index);
				            Console.WriteLine("charq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_charq_ArrayIndex(char?[] val, int index) {
				        Expression<Func<char?>> e =
				            Expression.Lambda<Func<char?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(char?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char?> f = e.Compile();
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
			
			//-------- Scenario 3179
			namespace Scenario3179{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "boolq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression boolq_ArrayIndex__() {
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
				            success = check_boolq_ArrayIndex();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_boolq_ArrayIndex() {
				        return checkEx_boolq_ArrayIndex(null, -1) &
				            checkEx_boolq_ArrayIndex(null, 0) &
				            checkEx_boolq_ArrayIndex(null, 1) &
				
				            check_boolq_ArrayIndex(genArrboolq_ArrayIndex(0)) &
				            check_boolq_ArrayIndex(genArrboolq_ArrayIndex(1)) &
				            check_boolq_ArrayIndex(genArrboolq_ArrayIndex(5));
				    }
				
				    static bool?[] genArrboolq_ArrayIndex(int size) {
				        bool?[] vals = new bool?[] { true, false };
				        bool?[] result = new bool?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool check_boolq_ArrayIndex(bool?[] val) {
				        bool success = checkEx_boolq_ArrayIndex(val, -1);
				        for (int i = 0; i < val.Length; i++) {
				            success &= check_boolq_ArrayIndex(val, 0);
				        }
				        success &= checkEx_boolq_ArrayIndex(val, val.Length);
				        return success;
				    }
				
				    static bool checkEx_boolq_ArrayIndex(bool?[] val, int index) {
				        try {
				            check_boolq_ArrayIndex(val, index);
				            Console.WriteLine("boolq_ArrayIndex[" + index + "] failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_boolq_ArrayIndex(bool?[] val, int index) {
				        Expression<Func<bool?>> e =
				            Expression.Lambda<Func<bool?>>(
				                Expression.ArrayIndex(Expression.Constant(val, typeof(bool?[])),
				                    Expression.Constant(index, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool?> f = e.Compile();
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
				
				//-------- Scenario 3180
				namespace Scenario3180{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Sq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Sq_ArrayIndex__() {
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
					            success = check_Sq_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Sq_ArrayIndex() {
					        return checkEx_Sq_ArrayIndex(null, -1) &
					            checkEx_Sq_ArrayIndex(null, 0) &
					            checkEx_Sq_ArrayIndex(null, 1) &
					
					            check_Sq_ArrayIndex(genArrSq_ArrayIndex(0)) &
					            check_Sq_ArrayIndex(genArrSq_ArrayIndex(1)) &
					            check_Sq_ArrayIndex(genArrSq_ArrayIndex(5));
					    }
					
					    static S?[] genArrSq_ArrayIndex(int size) {
					        S?[] vals = new S?[] { default(S), new S() };
					        S?[] result = new S?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Sq_ArrayIndex(S?[] val) {
					        bool success = checkEx_Sq_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_Sq_ArrayIndex(val, 0);
					        }
					        success &= checkEx_Sq_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Sq_ArrayIndex(S?[] val, int index) {
					        try {
					            check_Sq_ArrayIndex(val, index);
					            Console.WriteLine("Sq_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Sq_ArrayIndex(S?[] val, int index) {
					        Expression<Func<S?>> e =
					            Expression.Lambda<Func<S?>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(S?[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S?> f = e.Compile();
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
				
			
			
			
			interface I {
			  void M();
			}
			
			public class C : IEquatable<C>, I {
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
			
			public class D : C, IEquatable<D> {
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
				
				//-------- Scenario 3181
				namespace Scenario3181{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Spq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Spq_ArrayIndex__() {
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
					            success = check_Spq_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Spq_ArrayIndex() {
					        return checkEx_Spq_ArrayIndex(null, -1) &
					            checkEx_Spq_ArrayIndex(null, 0) &
					            checkEx_Spq_ArrayIndex(null, 1) &
					
					            check_Spq_ArrayIndex(genArrSpq_ArrayIndex(0)) &
					            check_Spq_ArrayIndex(genArrSpq_ArrayIndex(1)) &
					            check_Spq_ArrayIndex(genArrSpq_ArrayIndex(5));
					    }
					
					    static Sp?[] genArrSpq_ArrayIndex(int size) {
					        Sp?[] vals = new Sp?[] { default(Sp), new Sp(), new Sp(5,5.0) };
					        Sp?[] result = new Sp?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Spq_ArrayIndex(Sp?[] val) {
					        bool success = checkEx_Spq_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_Spq_ArrayIndex(val, 0);
					        }
					        success &= checkEx_Spq_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Spq_ArrayIndex(Sp?[] val, int index) {
					        try {
					            check_Spq_ArrayIndex(val, index);
					            Console.WriteLine("Spq_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Spq_ArrayIndex(Sp?[] val, int index) {
					        Expression<Func<Sp?>> e =
					            Expression.Lambda<Func<Sp?>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Sp?[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp?> f = e.Compile();
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
				
			
			
			
			interface I {
			  void M();
			}
			
			public class C : IEquatable<C>, I {
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
			
			public class D : C, IEquatable<D> {
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
				
				//-------- Scenario 3182
				namespace Scenario3182{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ssq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ssq_ArrayIndex__() {
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
					            success = check_Ssq_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ssq_ArrayIndex() {
					        return checkEx_Ssq_ArrayIndex(null, -1) &
					            checkEx_Ssq_ArrayIndex(null, 0) &
					            checkEx_Ssq_ArrayIndex(null, 1) &
					
					            check_Ssq_ArrayIndex(genArrSsq_ArrayIndex(0)) &
					            check_Ssq_ArrayIndex(genArrSsq_ArrayIndex(1)) &
					            check_Ssq_ArrayIndex(genArrSsq_ArrayIndex(5));
					    }
					
					    static Ss?[] genArrSsq_ArrayIndex(int size) {
					        Ss?[] vals = new Ss?[] { default(Ss), new Ss(), new Ss(new S()) };
					        Ss?[] result = new Ss?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Ssq_ArrayIndex(Ss?[] val) {
					        bool success = checkEx_Ssq_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_Ssq_ArrayIndex(val, 0);
					        }
					        success &= checkEx_Ssq_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Ssq_ArrayIndex(Ss?[] val, int index) {
					        try {
					            check_Ssq_ArrayIndex(val, index);
					            Console.WriteLine("Ssq_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Ssq_ArrayIndex(Ss?[] val, int index) {
					        Expression<Func<Ss?>> e =
					            Expression.Lambda<Func<Ss?>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Ss?[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss?> f = e.Compile();
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
				
			
			
			
			interface I {
			  void M();
			}
			
			public class C : IEquatable<C>, I {
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
			
			public class D : C, IEquatable<D> {
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
				
				//-------- Scenario 3183
				namespace Scenario3183{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Scq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Scq_ArrayIndex__() {
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
					            success = check_Scq_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Scq_ArrayIndex() {
					        return checkEx_Scq_ArrayIndex(null, -1) &
					            checkEx_Scq_ArrayIndex(null, 0) &
					            checkEx_Scq_ArrayIndex(null, 1) &
					
					            check_Scq_ArrayIndex(genArrScq_ArrayIndex(0)) &
					            check_Scq_ArrayIndex(genArrScq_ArrayIndex(1)) &
					            check_Scq_ArrayIndex(genArrScq_ArrayIndex(5));
					    }
					
					    static Sc?[] genArrScq_ArrayIndex(int size) {
					        Sc?[] vals = new Sc?[] { default(Sc), new Sc(), new Sc(null) };
					        Sc?[] result = new Sc?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Scq_ArrayIndex(Sc?[] val) {
					        bool success = checkEx_Scq_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_Scq_ArrayIndex(val, 0);
					        }
					        success &= checkEx_Scq_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Scq_ArrayIndex(Sc?[] val, int index) {
					        try {
					            check_Scq_ArrayIndex(val, index);
					            Console.WriteLine("Scq_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Scq_ArrayIndex(Sc?[] val, int index) {
					        Expression<Func<Sc?>> e =
					            Expression.Lambda<Func<Sc?>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Sc?[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc?> f = e.Compile();
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
				
			
			
			
			interface I {
			  void M();
			}
			
			public class C : IEquatable<C>, I {
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
			
			public class D : C, IEquatable<D> {
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
				
				//-------- Scenario 3184
				namespace Scenario3184{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Scsq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Scsq_ArrayIndex__() {
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
					            success = check_Scsq_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Scsq_ArrayIndex() {
					        return checkEx_Scsq_ArrayIndex(null, -1) &
					            checkEx_Scsq_ArrayIndex(null, 0) &
					            checkEx_Scsq_ArrayIndex(null, 1) &
					
					            check_Scsq_ArrayIndex(genArrScsq_ArrayIndex(0)) &
					            check_Scsq_ArrayIndex(genArrScsq_ArrayIndex(1)) &
					            check_Scsq_ArrayIndex(genArrScsq_ArrayIndex(5));
					    }
					
					    static Scs?[] genArrScsq_ArrayIndex(int size) {
					        Scs?[] vals = new Scs?[] { default(Scs), new Scs(), new Scs(null,new S()) };
					        Scs?[] result = new Scs?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Scsq_ArrayIndex(Scs?[] val) {
					        bool success = checkEx_Scsq_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_Scsq_ArrayIndex(val, 0);
					        }
					        success &= checkEx_Scsq_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Scsq_ArrayIndex(Scs?[] val, int index) {
					        try {
					            check_Scsq_ArrayIndex(val, index);
					            Console.WriteLine("Scsq_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Scsq_ArrayIndex(Scs?[] val, int index) {
					        Expression<Func<Scs?>> e =
					            Expression.Lambda<Func<Scs?>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Scs?[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs?> f = e.Compile();
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
				
			
			
			
			interface I {
			  void M();
			}
			
			public class C : IEquatable<C>, I {
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
			
			public class D : C, IEquatable<D> {
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
				
				//-------- Scenario 3185
				namespace Scenario3185{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tsq_ArrayIndex_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tsq_ArrayIndex_S___() {
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
					            success = check_Tsq_ArrayIndex<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tsq_ArrayIndex<Ts>() where Ts : struct {
					        return checkEx_Tsq_ArrayIndex<Ts>(null, -1) &
					            checkEx_Tsq_ArrayIndex<Ts>(null, 0) &
					            checkEx_Tsq_ArrayIndex<Ts>(null, 1) &
					
					            check_Tsq_ArrayIndex<Ts>(genArrTsq_ArrayIndex<Ts>(0)) &
					            check_Tsq_ArrayIndex<Ts>(genArrTsq_ArrayIndex<Ts>(1)) &
					            check_Tsq_ArrayIndex<Ts>(genArrTsq_ArrayIndex<Ts>(5));
					    }
					
					    static Ts?[] genArrTsq_ArrayIndex<Ts>(int size) where Ts : struct {
					        Ts?[] vals = new Ts?[] { default(Ts), new Ts() };
					        Ts?[] result = new Ts?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Tsq_ArrayIndex<Ts>(Ts?[] val) where Ts : struct {
					        bool success = checkEx_Tsq_ArrayIndex<Ts>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_Tsq_ArrayIndex<Ts>(val, 0);
					        }
					        success &= checkEx_Tsq_ArrayIndex<Ts>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Tsq_ArrayIndex<Ts>(Ts?[] val, int index) where Ts : struct {
					        try {
					            check_Tsq_ArrayIndex<Ts>(val, index);
					            Console.WriteLine("Tsq_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Tsq_ArrayIndex<Ts>(Ts?[] val, int index) where Ts : struct {
					        Expression<Func<Ts?>> e =
					            Expression.Lambda<Func<Ts?>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Ts?[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?> f = e.Compile();
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
				
			
			
			
			interface I {
			  void M();
			}
			
			public class C : IEquatable<C>, I {
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
			
			public class D : C, IEquatable<D> {
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
				
				//-------- Scenario 3186
				namespace Scenario3186{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tsq_ArrayIndex_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tsq_ArrayIndex_Scs___() {
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
					            success = check_Tsq_ArrayIndex<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tsq_ArrayIndex<Ts>() where Ts : struct {
					        return checkEx_Tsq_ArrayIndex<Ts>(null, -1) &
					            checkEx_Tsq_ArrayIndex<Ts>(null, 0) &
					            checkEx_Tsq_ArrayIndex<Ts>(null, 1) &
					
					            check_Tsq_ArrayIndex<Ts>(genArrTsq_ArrayIndex<Ts>(0)) &
					            check_Tsq_ArrayIndex<Ts>(genArrTsq_ArrayIndex<Ts>(1)) &
					            check_Tsq_ArrayIndex<Ts>(genArrTsq_ArrayIndex<Ts>(5));
					    }
					
					    static Ts?[] genArrTsq_ArrayIndex<Ts>(int size) where Ts : struct {
					        Ts?[] vals = new Ts?[] { default(Ts), new Ts() };
					        Ts?[] result = new Ts?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Tsq_ArrayIndex<Ts>(Ts?[] val) where Ts : struct {
					        bool success = checkEx_Tsq_ArrayIndex<Ts>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_Tsq_ArrayIndex<Ts>(val, 0);
					        }
					        success &= checkEx_Tsq_ArrayIndex<Ts>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Tsq_ArrayIndex<Ts>(Ts?[] val, int index) where Ts : struct {
					        try {
					            check_Tsq_ArrayIndex<Ts>(val, index);
					            Console.WriteLine("Tsq_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Tsq_ArrayIndex<Ts>(Ts?[] val, int index) where Ts : struct {
					        Expression<Func<Ts?>> e =
					            Expression.Lambda<Func<Ts?>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Ts?[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?> f = e.Compile();
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
				
			
			
			
			interface I {
			  void M();
			}
			
			public class C : IEquatable<C>, I {
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
			
			public class D : C, IEquatable<D> {
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
				
				//-------- Scenario 3187
				namespace Scenario3187{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tsq_ArrayIndex_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tsq_ArrayIndex_E___() {
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
					            success = check_Tsq_ArrayIndex<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tsq_ArrayIndex<Ts>() where Ts : struct {
					        return checkEx_Tsq_ArrayIndex<Ts>(null, -1) &
					            checkEx_Tsq_ArrayIndex<Ts>(null, 0) &
					            checkEx_Tsq_ArrayIndex<Ts>(null, 1) &
					
					            check_Tsq_ArrayIndex<Ts>(genArrTsq_ArrayIndex<Ts>(0)) &
					            check_Tsq_ArrayIndex<Ts>(genArrTsq_ArrayIndex<Ts>(1)) &
					            check_Tsq_ArrayIndex<Ts>(genArrTsq_ArrayIndex<Ts>(5));
					    }
					
					    static Ts?[] genArrTsq_ArrayIndex<Ts>(int size) where Ts : struct {
					        Ts?[] vals = new Ts?[] { default(Ts), new Ts() };
					        Ts?[] result = new Ts?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Tsq_ArrayIndex<Ts>(Ts?[] val) where Ts : struct {
					        bool success = checkEx_Tsq_ArrayIndex<Ts>(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_Tsq_ArrayIndex<Ts>(val, 0);
					        }
					        success &= checkEx_Tsq_ArrayIndex<Ts>(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Tsq_ArrayIndex<Ts>(Ts?[] val, int index) where Ts : struct {
					        try {
					            check_Tsq_ArrayIndex<Ts>(val, index);
					            Console.WriteLine("Tsq_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Tsq_ArrayIndex<Ts>(Ts?[] val, int index) where Ts : struct {
					        Expression<Func<Ts?>> e =
					            Expression.Lambda<Func<Ts?>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(Ts?[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?> f = e.Compile();
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
				
			
			
			
			interface I {
			  void M();
			}
			
			public class C : IEquatable<C>, I {
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
			
			public class D : C, IEquatable<D> {
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
				
				//-------- Scenario 3188
				namespace Scenario3188{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Eq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Eq_ArrayIndex__() {
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
					            success = check_Eq_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Eq_ArrayIndex() {
					        return checkEx_Eq_ArrayIndex(null, -1) &
					            checkEx_Eq_ArrayIndex(null, 0) &
					            checkEx_Eq_ArrayIndex(null, 1) &
					
					            check_Eq_ArrayIndex(genArrEq_ArrayIndex(0)) &
					            check_Eq_ArrayIndex(genArrEq_ArrayIndex(1)) &
					            check_Eq_ArrayIndex(genArrEq_ArrayIndex(5));
					    }
					
					    static E?[] genArrEq_ArrayIndex(int size) {
					        E?[] vals = new E?[] { (E) 0, E.A, E.B, (E) int.MaxValue, (E) int.MinValue };
					        E?[] result = new E?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Eq_ArrayIndex(E?[] val) {
					        bool success = checkEx_Eq_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_Eq_ArrayIndex(val, 0);
					        }
					        success &= checkEx_Eq_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Eq_ArrayIndex(E?[] val, int index) {
					        try {
					            check_Eq_ArrayIndex(val, index);
					            Console.WriteLine("Eq_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Eq_ArrayIndex(E?[] val, int index) {
					        Expression<Func<E?>> e =
					            Expression.Lambda<Func<E?>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(E?[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<E?> f = e.Compile();
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
				
			
			
			
			interface I {
			  void M();
			}
			
			public class C : IEquatable<C>, I {
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
			
			public class D : C, IEquatable<D> {
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
				
				//-------- Scenario 3189
				namespace Scenario3189{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Elq_ArrayIndex__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Elq_ArrayIndex__() {
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
					            success = check_Elq_ArrayIndex();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Elq_ArrayIndex() {
					        return checkEx_Elq_ArrayIndex(null, -1) &
					            checkEx_Elq_ArrayIndex(null, 0) &
					            checkEx_Elq_ArrayIndex(null, 1) &
					
					            check_Elq_ArrayIndex(genArrElq_ArrayIndex(0)) &
					            check_Elq_ArrayIndex(genArrElq_ArrayIndex(1)) &
					            check_Elq_ArrayIndex(genArrElq_ArrayIndex(5));
					    }
					
					    static El?[] genArrElq_ArrayIndex(int size) {
					        El?[] vals = new El?[] { (El) 0, El.A, El.B, (El) long.MaxValue, (El) long.MinValue };
					        El?[] result = new El?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool check_Elq_ArrayIndex(El?[] val) {
					        bool success = checkEx_Elq_ArrayIndex(val, -1);
					        for (int i = 0; i < val.Length; i++) {
					            success &= check_Elq_ArrayIndex(val, 0);
					        }
					        success &= checkEx_Elq_ArrayIndex(val, val.Length);
					        return success;
					    }
					
					    static bool checkEx_Elq_ArrayIndex(El?[] val, int index) {
					        try {
					            check_Elq_ArrayIndex(val, index);
					            Console.WriteLine("Elq_ArrayIndex[" + index + "] failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Elq_ArrayIndex(El?[] val, int index) {
					        Expression<Func<El?>> e =
					            Expression.Lambda<Func<El?>>(
					                Expression.ArrayIndex(Expression.Constant(val, typeof(El?[])),
					                    Expression.Constant(index, typeof(int))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<El?> f = e.Compile();
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
				
			
			
			
			interface I {
			  void M();
			}
			
			public class C : IEquatable<C>, I {
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
			
			public class D : C, IEquatable<D> {
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
