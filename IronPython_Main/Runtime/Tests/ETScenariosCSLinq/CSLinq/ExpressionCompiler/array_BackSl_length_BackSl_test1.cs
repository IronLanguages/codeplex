#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 3058
			namespace Scenario3058{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_ArrayLength__() {
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
				            success = check_byteq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_ArrayLength() {
				        return checkEx_byteq_ArrayLength(null) &
				            check_byteq_ArrayLength(genArrbyteq_ArrayLength(0)) &
				            check_byteq_ArrayLength(genArrbyteq_ArrayLength(1)) &
				            check_byteq_ArrayLength(genArrbyteq_ArrayLength(5));
				    }
				
				    static byte?[] genArrbyteq_ArrayLength(int size) {
				        byte?[] vals = new byte?[] { 0, 1, byte.MaxValue };
				        byte?[] result = new byte?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_byteq_ArrayLength(byte?[] val) {
				        try {
				            check_byteq_ArrayLength(val);
				            Console.WriteLine("byteq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_byteq_ArrayLength(byte?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(byte?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
			
			//-------- Scenario 3059
			namespace Scenario3059{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_ArrayLength__() {
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
				            success = check_ushortq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_ArrayLength() {
				        return checkEx_ushortq_ArrayLength(null) &
				            check_ushortq_ArrayLength(genArrushortq_ArrayLength(0)) &
				            check_ushortq_ArrayLength(genArrushortq_ArrayLength(1)) &
				            check_ushortq_ArrayLength(genArrushortq_ArrayLength(5));
				    }
				
				    static ushort?[] genArrushortq_ArrayLength(int size) {
				        ushort?[] vals = new ushort?[] { 0, 1, ushort.MaxValue };
				        ushort?[] result = new ushort?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_ushortq_ArrayLength(ushort?[] val) {
				        try {
				            check_ushortq_ArrayLength(val);
				            Console.WriteLine("ushortq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_ushortq_ArrayLength(ushort?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(ushort?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
			
			//-------- Scenario 3060
			namespace Scenario3060{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_ArrayLength__() {
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
				            success = check_uintq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_ArrayLength() {
				        return checkEx_uintq_ArrayLength(null) &
				            check_uintq_ArrayLength(genArruintq_ArrayLength(0)) &
				            check_uintq_ArrayLength(genArruintq_ArrayLength(1)) &
				            check_uintq_ArrayLength(genArruintq_ArrayLength(5));
				    }
				
				    static uint?[] genArruintq_ArrayLength(int size) {
				        uint?[] vals = new uint?[] { 0, 1, uint.MaxValue };
				        uint?[] result = new uint?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_uintq_ArrayLength(uint?[] val) {
				        try {
				            check_uintq_ArrayLength(val);
				            Console.WriteLine("uintq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_uintq_ArrayLength(uint?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(uint?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
			
			//-------- Scenario 3061
			namespace Scenario3061{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_ArrayLength__() {
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
				            success = check_ulongq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_ArrayLength() {
				        return checkEx_ulongq_ArrayLength(null) &
				            check_ulongq_ArrayLength(genArrulongq_ArrayLength(0)) &
				            check_ulongq_ArrayLength(genArrulongq_ArrayLength(1)) &
				            check_ulongq_ArrayLength(genArrulongq_ArrayLength(5));
				    }
				
				    static ulong?[] genArrulongq_ArrayLength(int size) {
				        ulong?[] vals = new ulong?[] { 0, 1, ulong.MaxValue };
				        ulong?[] result = new ulong?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_ulongq_ArrayLength(ulong?[] val) {
				        try {
				            check_ulongq_ArrayLength(val);
				            Console.WriteLine("ulongq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_ulongq_ArrayLength(ulong?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(ulong?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
			
			//-------- Scenario 3062
			namespace Scenario3062{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_ArrayLength__() {
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
				            success = check_sbyteq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_ArrayLength() {
				        return checkEx_sbyteq_ArrayLength(null) &
				            check_sbyteq_ArrayLength(genArrsbyteq_ArrayLength(0)) &
				            check_sbyteq_ArrayLength(genArrsbyteq_ArrayLength(1)) &
				            check_sbyteq_ArrayLength(genArrsbyteq_ArrayLength(5));
				    }
				
				    static sbyte?[] genArrsbyteq_ArrayLength(int size) {
				        sbyte?[] vals = new sbyte?[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        sbyte?[] result = new sbyte?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_sbyteq_ArrayLength(sbyte?[] val) {
				        try {
				            check_sbyteq_ArrayLength(val);
				            Console.WriteLine("sbyteq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_sbyteq_ArrayLength(sbyte?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(sbyte?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
			
			//-------- Scenario 3063
			namespace Scenario3063{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_ArrayLength__() {
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
				            success = check_shortq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_ArrayLength() {
				        return checkEx_shortq_ArrayLength(null) &
				            check_shortq_ArrayLength(genArrshortq_ArrayLength(0)) &
				            check_shortq_ArrayLength(genArrshortq_ArrayLength(1)) &
				            check_shortq_ArrayLength(genArrshortq_ArrayLength(5));
				    }
				
				    static short?[] genArrshortq_ArrayLength(int size) {
				        short?[] vals = new short?[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        short?[] result = new short?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_shortq_ArrayLength(short?[] val) {
				        try {
				            check_shortq_ArrayLength(val);
				            Console.WriteLine("shortq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_shortq_ArrayLength(short?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(short?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
			
			//-------- Scenario 3064
			namespace Scenario3064{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_ArrayLength__() {
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
				            success = check_intq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_ArrayLength() {
				        return checkEx_intq_ArrayLength(null) &
				            check_intq_ArrayLength(genArrintq_ArrayLength(0)) &
				            check_intq_ArrayLength(genArrintq_ArrayLength(1)) &
				            check_intq_ArrayLength(genArrintq_ArrayLength(5));
				    }
				
				    static int?[] genArrintq_ArrayLength(int size) {
				        int?[] vals = new int?[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        int?[] result = new int?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_intq_ArrayLength(int?[] val) {
				        try {
				            check_intq_ArrayLength(val);
				            Console.WriteLine("intq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_intq_ArrayLength(int?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(int?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
			
			//-------- Scenario 3065
			namespace Scenario3065{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_ArrayLength__() {
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
				            success = check_longq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_ArrayLength() {
				        return checkEx_longq_ArrayLength(null) &
				            check_longq_ArrayLength(genArrlongq_ArrayLength(0)) &
				            check_longq_ArrayLength(genArrlongq_ArrayLength(1)) &
				            check_longq_ArrayLength(genArrlongq_ArrayLength(5));
				    }
				
				    static long?[] genArrlongq_ArrayLength(int size) {
				        long?[] vals = new long?[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        long?[] result = new long?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_longq_ArrayLength(long?[] val) {
				        try {
				            check_longq_ArrayLength(val);
				            Console.WriteLine("longq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_longq_ArrayLength(long?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(long?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
			
			//-------- Scenario 3066
			namespace Scenario3066{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "floatq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression floatq_ArrayLength__() {
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
				            success = check_floatq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_floatq_ArrayLength() {
				        return checkEx_floatq_ArrayLength(null) &
				            check_floatq_ArrayLength(genArrfloatq_ArrayLength(0)) &
				            check_floatq_ArrayLength(genArrfloatq_ArrayLength(1)) &
				            check_floatq_ArrayLength(genArrfloatq_ArrayLength(5));
				    }
				
				    static float?[] genArrfloatq_ArrayLength(int size) {
				        float?[] vals = new float?[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        float?[] result = new float?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_floatq_ArrayLength(float?[] val) {
				        try {
				            check_floatq_ArrayLength(val);
				            Console.WriteLine("floatq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_floatq_ArrayLength(float?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(float?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
			
			//-------- Scenario 3067
			namespace Scenario3067{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "doubleq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression doubleq_ArrayLength__() {
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
				            success = check_doubleq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_doubleq_ArrayLength() {
				        return checkEx_doubleq_ArrayLength(null) &
				            check_doubleq_ArrayLength(genArrdoubleq_ArrayLength(0)) &
				            check_doubleq_ArrayLength(genArrdoubleq_ArrayLength(1)) &
				            check_doubleq_ArrayLength(genArrdoubleq_ArrayLength(5));
				    }
				
				    static double?[] genArrdoubleq_ArrayLength(int size) {
				        double?[] vals = new double?[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        double?[] result = new double?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_doubleq_ArrayLength(double?[] val) {
				        try {
				            check_doubleq_ArrayLength(val);
				            Console.WriteLine("doubleq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_doubleq_ArrayLength(double?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(double?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
			
			//-------- Scenario 3068
			namespace Scenario3068{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "decimalq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression decimalq_ArrayLength__() {
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
				            success = check_decimalq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_decimalq_ArrayLength() {
				        return checkEx_decimalq_ArrayLength(null) &
				            check_decimalq_ArrayLength(genArrdecimalq_ArrayLength(0)) &
				            check_decimalq_ArrayLength(genArrdecimalq_ArrayLength(1)) &
				            check_decimalq_ArrayLength(genArrdecimalq_ArrayLength(5));
				    }
				
				    static decimal?[] genArrdecimalq_ArrayLength(int size) {
				        decimal?[] vals = new decimal?[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        decimal?[] result = new decimal?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_decimalq_ArrayLength(decimal?[] val) {
				        try {
				            check_decimalq_ArrayLength(val);
				            Console.WriteLine("decimalq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_decimalq_ArrayLength(decimal?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(decimal?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
			
			//-------- Scenario 3069
			namespace Scenario3069{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "charq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression charq_ArrayLength__() {
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
				            success = check_charq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_charq_ArrayLength() {
				        return checkEx_charq_ArrayLength(null) &
				            check_charq_ArrayLength(genArrcharq_ArrayLength(0)) &
				            check_charq_ArrayLength(genArrcharq_ArrayLength(1)) &
				            check_charq_ArrayLength(genArrcharq_ArrayLength(5));
				    }
				
				    static char?[] genArrcharq_ArrayLength(int size) {
				        char?[] vals = new char?[] { '\0', '\b', 'A', '\uffff' };
				        char?[] result = new char?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_charq_ArrayLength(char?[] val) {
				        try {
				            check_charq_ArrayLength(val);
				            Console.WriteLine("charq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_charq_ArrayLength(char?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(char?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
			
			//-------- Scenario 3070
			namespace Scenario3070{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "boolq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression boolq_ArrayLength__() {
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
				            success = check_boolq_ArrayLength();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_boolq_ArrayLength() {
				        return checkEx_boolq_ArrayLength(null) &
				            check_boolq_ArrayLength(genArrboolq_ArrayLength(0)) &
				            check_boolq_ArrayLength(genArrboolq_ArrayLength(1)) &
				            check_boolq_ArrayLength(genArrboolq_ArrayLength(5));
				    }
				
				    static bool?[] genArrboolq_ArrayLength(int size) {
				        bool?[] vals = new bool?[] { true, false };
				        bool?[] result = new bool?[size];
				        for (int i = 0; i < size; i++) {
				            result[i] = vals[i % vals.Length];
				        }
				        return result;
				    }
				
				    static bool checkEx_boolq_ArrayLength(bool?[] val) {
				        try {
				            check_boolq_ArrayLength(val);
				            Console.WriteLine("boolq_ArrayLength failed");
				            return false;
				        }
				        catch {
				            return true;
				        }
				    }
				
				    static bool check_boolq_ArrayLength(bool?[] val) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.ArrayLength(Expression.Constant(val, typeof(bool?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				        return f() == val.Length;
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
				
				//-------- Scenario 3071
				namespace Scenario3071{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Sq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Sq_ArrayLength__() {
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
					            success = check_Sq_ArrayLength();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Sq_ArrayLength() {
					        return checkEx_Sq_ArrayLength(null) &
					            check_Sq_ArrayLength(genArrSq_ArrayLength(0)) &
					            check_Sq_ArrayLength(genArrSq_ArrayLength(1)) &
					            check_Sq_ArrayLength(genArrSq_ArrayLength(5));
					    }
					
					    static S?[] genArrSq_ArrayLength(int size) {
					        S?[] vals = new S?[] { default(S), new S() };
					        S?[] result = new S?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool checkEx_Sq_ArrayLength(S?[] val) {
					        try {
					            check_Sq_ArrayLength(val);
					            Console.WriteLine("Sq_ArrayLength failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Sq_ArrayLength(S?[] val) {
					        Expression<Func<int>> e =
					            Expression.Lambda<Func<int>>(
					                Expression.ArrayLength(Expression.Constant(val, typeof(S?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<int> f = e.Compile();
					        return f() == val.Length;
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
				
				//-------- Scenario 3072
				namespace Scenario3072{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Spq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Spq_ArrayLength__() {
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
					            success = check_Spq_ArrayLength();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Spq_ArrayLength() {
					        return checkEx_Spq_ArrayLength(null) &
					            check_Spq_ArrayLength(genArrSpq_ArrayLength(0)) &
					            check_Spq_ArrayLength(genArrSpq_ArrayLength(1)) &
					            check_Spq_ArrayLength(genArrSpq_ArrayLength(5));
					    }
					
					    static Sp?[] genArrSpq_ArrayLength(int size) {
					        Sp?[] vals = new Sp?[] { default(Sp), new Sp(), new Sp(5,5.0) };
					        Sp?[] result = new Sp?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool checkEx_Spq_ArrayLength(Sp?[] val) {
					        try {
					            check_Spq_ArrayLength(val);
					            Console.WriteLine("Spq_ArrayLength failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Spq_ArrayLength(Sp?[] val) {
					        Expression<Func<int>> e =
					            Expression.Lambda<Func<int>>(
					                Expression.ArrayLength(Expression.Constant(val, typeof(Sp?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<int> f = e.Compile();
					        return f() == val.Length;
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
				
				//-------- Scenario 3073
				namespace Scenario3073{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ssq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ssq_ArrayLength__() {
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
					            success = check_Ssq_ArrayLength();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ssq_ArrayLength() {
					        return checkEx_Ssq_ArrayLength(null) &
					            check_Ssq_ArrayLength(genArrSsq_ArrayLength(0)) &
					            check_Ssq_ArrayLength(genArrSsq_ArrayLength(1)) &
					            check_Ssq_ArrayLength(genArrSsq_ArrayLength(5));
					    }
					
					    static Ss?[] genArrSsq_ArrayLength(int size) {
					        Ss?[] vals = new Ss?[] { default(Ss), new Ss(), new Ss(new S()) };
					        Ss?[] result = new Ss?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool checkEx_Ssq_ArrayLength(Ss?[] val) {
					        try {
					            check_Ssq_ArrayLength(val);
					            Console.WriteLine("Ssq_ArrayLength failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Ssq_ArrayLength(Ss?[] val) {
					        Expression<Func<int>> e =
					            Expression.Lambda<Func<int>>(
					                Expression.ArrayLength(Expression.Constant(val, typeof(Ss?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<int> f = e.Compile();
					        return f() == val.Length;
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
				
				//-------- Scenario 3074
				namespace Scenario3074{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Scq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Scq_ArrayLength__() {
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
					            success = check_Scq_ArrayLength();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Scq_ArrayLength() {
					        return checkEx_Scq_ArrayLength(null) &
					            check_Scq_ArrayLength(genArrScq_ArrayLength(0)) &
					            check_Scq_ArrayLength(genArrScq_ArrayLength(1)) &
					            check_Scq_ArrayLength(genArrScq_ArrayLength(5));
					    }
					
					    static Sc?[] genArrScq_ArrayLength(int size) {
					        Sc?[] vals = new Sc?[] { default(Sc), new Sc(), new Sc(null) };
					        Sc?[] result = new Sc?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool checkEx_Scq_ArrayLength(Sc?[] val) {
					        try {
					            check_Scq_ArrayLength(val);
					            Console.WriteLine("Scq_ArrayLength failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Scq_ArrayLength(Sc?[] val) {
					        Expression<Func<int>> e =
					            Expression.Lambda<Func<int>>(
					                Expression.ArrayLength(Expression.Constant(val, typeof(Sc?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<int> f = e.Compile();
					        return f() == val.Length;
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
				
				//-------- Scenario 3075
				namespace Scenario3075{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Scsq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Scsq_ArrayLength__() {
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
					            success = check_Scsq_ArrayLength();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Scsq_ArrayLength() {
					        return checkEx_Scsq_ArrayLength(null) &
					            check_Scsq_ArrayLength(genArrScsq_ArrayLength(0)) &
					            check_Scsq_ArrayLength(genArrScsq_ArrayLength(1)) &
					            check_Scsq_ArrayLength(genArrScsq_ArrayLength(5));
					    }
					
					    static Scs?[] genArrScsq_ArrayLength(int size) {
					        Scs?[] vals = new Scs?[] { default(Scs), new Scs(), new Scs(null,new S()) };
					        Scs?[] result = new Scs?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool checkEx_Scsq_ArrayLength(Scs?[] val) {
					        try {
					            check_Scsq_ArrayLength(val);
					            Console.WriteLine("Scsq_ArrayLength failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Scsq_ArrayLength(Scs?[] val) {
					        Expression<Func<int>> e =
					            Expression.Lambda<Func<int>>(
					                Expression.ArrayLength(Expression.Constant(val, typeof(Scs?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<int> f = e.Compile();
					        return f() == val.Length;
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
				
				//-------- Scenario 3076
				namespace Scenario3076{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tsq_ArrayLength_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tsq_ArrayLength_S___() {
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
					            success = check_Tsq_ArrayLength<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tsq_ArrayLength<Ts>() where Ts : struct {
					        return checkEx_Tsq_ArrayLength<Ts>(null) &
					            check_Tsq_ArrayLength<Ts>(genArrTsq_ArrayLength<Ts>(0)) &
					            check_Tsq_ArrayLength<Ts>(genArrTsq_ArrayLength<Ts>(1)) &
					            check_Tsq_ArrayLength<Ts>(genArrTsq_ArrayLength<Ts>(5));
					    }
					
					    static Ts?[] genArrTsq_ArrayLength<Ts>(int size) where Ts : struct {
					        Ts?[] vals = new Ts?[] { default(Ts), new Ts() };
					        Ts?[] result = new Ts?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool checkEx_Tsq_ArrayLength<Ts>(Ts?[] val) where Ts : struct {
					        try {
					            check_Tsq_ArrayLength<Ts>(val);
					            Console.WriteLine("Tsq_ArrayLength failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Tsq_ArrayLength<Ts>(Ts?[] val) where Ts : struct {
					        Expression<Func<int>> e =
					            Expression.Lambda<Func<int>>(
					                Expression.ArrayLength(Expression.Constant(val, typeof(Ts?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<int> f = e.Compile();
					        return f() == val.Length;
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
				
				//-------- Scenario 3077
				namespace Scenario3077{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tsq_ArrayLength_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tsq_ArrayLength_Scs___() {
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
					            success = check_Tsq_ArrayLength<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tsq_ArrayLength<Ts>() where Ts : struct {
					        return checkEx_Tsq_ArrayLength<Ts>(null) &
					            check_Tsq_ArrayLength<Ts>(genArrTsq_ArrayLength<Ts>(0)) &
					            check_Tsq_ArrayLength<Ts>(genArrTsq_ArrayLength<Ts>(1)) &
					            check_Tsq_ArrayLength<Ts>(genArrTsq_ArrayLength<Ts>(5));
					    }
					
					    static Ts?[] genArrTsq_ArrayLength<Ts>(int size) where Ts : struct {
					        Ts?[] vals = new Ts?[] { default(Ts), new Ts() };
					        Ts?[] result = new Ts?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool checkEx_Tsq_ArrayLength<Ts>(Ts?[] val) where Ts : struct {
					        try {
					            check_Tsq_ArrayLength<Ts>(val);
					            Console.WriteLine("Tsq_ArrayLength failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Tsq_ArrayLength<Ts>(Ts?[] val) where Ts : struct {
					        Expression<Func<int>> e =
					            Expression.Lambda<Func<int>>(
					                Expression.ArrayLength(Expression.Constant(val, typeof(Ts?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<int> f = e.Compile();
					        return f() == val.Length;
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
				
				//-------- Scenario 3078
				namespace Scenario3078{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tsq_ArrayLength_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tsq_ArrayLength_E___() {
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
					            success = check_Tsq_ArrayLength<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tsq_ArrayLength<Ts>() where Ts : struct {
					        return checkEx_Tsq_ArrayLength<Ts>(null) &
					            check_Tsq_ArrayLength<Ts>(genArrTsq_ArrayLength<Ts>(0)) &
					            check_Tsq_ArrayLength<Ts>(genArrTsq_ArrayLength<Ts>(1)) &
					            check_Tsq_ArrayLength<Ts>(genArrTsq_ArrayLength<Ts>(5));
					    }
					
					    static Ts?[] genArrTsq_ArrayLength<Ts>(int size) where Ts : struct {
					        Ts?[] vals = new Ts?[] { default(Ts), new Ts() };
					        Ts?[] result = new Ts?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool checkEx_Tsq_ArrayLength<Ts>(Ts?[] val) where Ts : struct {
					        try {
					            check_Tsq_ArrayLength<Ts>(val);
					            Console.WriteLine("Tsq_ArrayLength failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Tsq_ArrayLength<Ts>(Ts?[] val) where Ts : struct {
					        Expression<Func<int>> e =
					            Expression.Lambda<Func<int>>(
					                Expression.ArrayLength(Expression.Constant(val, typeof(Ts?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<int> f = e.Compile();
					        return f() == val.Length;
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
				
				//-------- Scenario 3079
				namespace Scenario3079{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Eq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Eq_ArrayLength__() {
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
					            success = check_Eq_ArrayLength();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Eq_ArrayLength() {
					        return checkEx_Eq_ArrayLength(null) &
					            check_Eq_ArrayLength(genArrEq_ArrayLength(0)) &
					            check_Eq_ArrayLength(genArrEq_ArrayLength(1)) &
					            check_Eq_ArrayLength(genArrEq_ArrayLength(5));
					    }
					
					    static E?[] genArrEq_ArrayLength(int size) {
					        E?[] vals = new E?[] { (E) 0, E.A, E.B, (E) int.MaxValue, (E) int.MinValue };
					        E?[] result = new E?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool checkEx_Eq_ArrayLength(E?[] val) {
					        try {
					            check_Eq_ArrayLength(val);
					            Console.WriteLine("Eq_ArrayLength failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Eq_ArrayLength(E?[] val) {
					        Expression<Func<int>> e =
					            Expression.Lambda<Func<int>>(
					                Expression.ArrayLength(Expression.Constant(val, typeof(E?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<int> f = e.Compile();
					        return f() == val.Length;
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
				
				//-------- Scenario 3080
				namespace Scenario3080{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Elq_ArrayLength__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Elq_ArrayLength__() {
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
					            success = check_Elq_ArrayLength();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Elq_ArrayLength() {
					        return checkEx_Elq_ArrayLength(null) &
					            check_Elq_ArrayLength(genArrElq_ArrayLength(0)) &
					            check_Elq_ArrayLength(genArrElq_ArrayLength(1)) &
					            check_Elq_ArrayLength(genArrElq_ArrayLength(5));
					    }
					
					    static El?[] genArrElq_ArrayLength(int size) {
					        El?[] vals = new El?[] { (El) 0, El.A, El.B, (El) long.MaxValue, (El) long.MinValue };
					        El?[] result = new El?[size];
					        for (int i = 0; i < size; i++) {
					            result[i] = vals[i % vals.Length];
					        }
					        return result;
					    }
					
					    static bool checkEx_Elq_ArrayLength(El?[] val) {
					        try {
					            check_Elq_ArrayLength(val);
					            Console.WriteLine("Elq_ArrayLength failed");
					            return false;
					        }
					        catch {
					            return true;
					        }
					    }
					
					    static bool check_Elq_ArrayLength(El?[] val) {
					        Expression<Func<int>> e =
					            Expression.Lambda<Func<int>>(
					                Expression.ArrayLength(Expression.Constant(val, typeof(El?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<int> f = e.Compile();
					        return f() == val.Length;
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
