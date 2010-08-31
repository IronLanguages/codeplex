#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 2465
			namespace Scenario2465{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_byte_NewArrayBounds__() {
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
				            success = check_byteq_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_byte_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_byte_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_byte_NewArrayBounds(byte? size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte[]> f = e.Compile();
				
				        byte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        byte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new byte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2466
			namespace Scenario2466{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_byte_NewArrayBounds__() {
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
				            success = check_sbyteq_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_byte_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_byte_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_byte_NewArrayBounds(sbyte? size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte[]> f = e.Compile();
				
				        byte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        byte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new byte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2467
			namespace Scenario2467{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_byte_NewArrayBounds__() {
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
				            success = check_ushortq_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_byte_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_byte_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_byte_NewArrayBounds(ushort? size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte[]> f = e.Compile();
				
				        byte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        byte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new byte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2468
			namespace Scenario2468{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_byte_NewArrayBounds__() {
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
				            success = check_shortq_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_byte_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_byte_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_byte_NewArrayBounds(short? size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte[]> f = e.Compile();
				
				        byte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        byte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new byte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2469
			namespace Scenario2469{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_byte_NewArrayBounds__() {
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
				            success = check_uintq_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_byte_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_byte_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_byte_NewArrayBounds(uint? size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte[]> f = e.Compile();
				
				        byte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        byte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new byte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2470
			namespace Scenario2470{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_byte_NewArrayBounds__() {
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
				            success = check_intq_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_byte_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_byte_NewArrayBounds(size)) {
				                Console.WriteLine("intq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_byte_NewArrayBounds(int? size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte[]> f = e.Compile();
				
				        byte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        byte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new byte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2471
			namespace Scenario2471{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_byte_NewArrayBounds__() {
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
				            success = check_ulongq_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_byte_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_byte_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_byte_NewArrayBounds(ulong? size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte[]> f = e.Compile();
				
				        byte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        byte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new byte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2472
			namespace Scenario2472{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_byte_NewArrayBounds__() {
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
				            success = check_longq_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_byte_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_byte_NewArrayBounds(size)) {
				                Console.WriteLine("longq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_byte_NewArrayBounds(long? size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte[]> f = e.Compile();
				
				        byte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        byte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new byte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2473
			namespace Scenario2473{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_ushort_NewArrayBounds__() {
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
				            success = check_byteq_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_ushort_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_ushort_NewArrayBounds(byte? size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort[]> f = e.Compile();
				
				        ushort[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ushort[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ushort[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2474
			namespace Scenario2474{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_ushort_NewArrayBounds__() {
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
				            success = check_sbyteq_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_ushort_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_ushort_NewArrayBounds(sbyte? size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort[]> f = e.Compile();
				
				        ushort[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ushort[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ushort[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2475
			namespace Scenario2475{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_ushort_NewArrayBounds__() {
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
				            success = check_ushortq_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_ushort_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_ushort_NewArrayBounds(ushort? size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort[]> f = e.Compile();
				
				        ushort[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ushort[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ushort[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2476
			namespace Scenario2476{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_ushort_NewArrayBounds__() {
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
				            success = check_shortq_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_ushort_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_ushort_NewArrayBounds(short? size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort[]> f = e.Compile();
				
				        ushort[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ushort[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ushort[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2477
			namespace Scenario2477{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_ushort_NewArrayBounds__() {
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
				            success = check_uintq_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_ushort_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_ushort_NewArrayBounds(uint? size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort[]> f = e.Compile();
				
				        ushort[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ushort[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ushort[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2478
			namespace Scenario2478{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_ushort_NewArrayBounds__() {
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
				            success = check_intq_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_ushort_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("intq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_ushort_NewArrayBounds(int? size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort[]> f = e.Compile();
				
				        ushort[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ushort[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ushort[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2479
			namespace Scenario2479{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_ushort_NewArrayBounds__() {
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
				            success = check_ulongq_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_ushort_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_ushort_NewArrayBounds(ulong? size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort[]> f = e.Compile();
				
				        ushort[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ushort[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ushort[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2480
			namespace Scenario2480{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_ushort_NewArrayBounds__() {
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
				            success = check_longq_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_ushort_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("longq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_ushort_NewArrayBounds(long? size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort[]> f = e.Compile();
				
				        ushort[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ushort[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ushort[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2481
			namespace Scenario2481{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_uint_NewArrayBounds__() {
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
				            success = check_byteq_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_uint_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_uint_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_uint_NewArrayBounds(byte? size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint[]> f = e.Compile();
				
				        uint[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        uint[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new uint[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2482
			namespace Scenario2482{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_uint_NewArrayBounds__() {
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
				            success = check_sbyteq_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_uint_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_uint_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_uint_NewArrayBounds(sbyte? size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint[]> f = e.Compile();
				
				        uint[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        uint[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new uint[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2483
			namespace Scenario2483{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_uint_NewArrayBounds__() {
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
				            success = check_ushortq_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_uint_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_uint_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_uint_NewArrayBounds(ushort? size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint[]> f = e.Compile();
				
				        uint[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        uint[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new uint[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2484
			namespace Scenario2484{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_uint_NewArrayBounds__() {
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
				            success = check_shortq_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_uint_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_uint_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_uint_NewArrayBounds(short? size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint[]> f = e.Compile();
				
				        uint[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        uint[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new uint[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2485
			namespace Scenario2485{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_uint_NewArrayBounds__() {
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
				            success = check_uintq_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_uint_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_uint_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_uint_NewArrayBounds(uint? size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint[]> f = e.Compile();
				
				        uint[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        uint[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new uint[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2486
			namespace Scenario2486{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_uint_NewArrayBounds__() {
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
				            success = check_intq_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_uint_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_uint_NewArrayBounds(size)) {
				                Console.WriteLine("intq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_uint_NewArrayBounds(int? size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint[]> f = e.Compile();
				
				        uint[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        uint[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new uint[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2487
			namespace Scenario2487{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_uint_NewArrayBounds__() {
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
				            success = check_ulongq_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_uint_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_uint_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_uint_NewArrayBounds(ulong? size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint[]> f = e.Compile();
				
				        uint[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        uint[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new uint[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2488
			namespace Scenario2488{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_uint_NewArrayBounds__() {
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
				            success = check_longq_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_uint_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_uint_NewArrayBounds(size)) {
				                Console.WriteLine("longq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_uint_NewArrayBounds(long? size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint[]> f = e.Compile();
				
				        uint[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        uint[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new uint[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2489
			namespace Scenario2489{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_ulong_NewArrayBounds__() {
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
				            success = check_byteq_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_ulong_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_ulong_NewArrayBounds(byte? size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong[]> f = e.Compile();
				
				        ulong[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ulong[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ulong[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2490
			namespace Scenario2490{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_ulong_NewArrayBounds__() {
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
				            success = check_sbyteq_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_ulong_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_ulong_NewArrayBounds(sbyte? size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong[]> f = e.Compile();
				
				        ulong[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ulong[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ulong[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2491
			namespace Scenario2491{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_ulong_NewArrayBounds__() {
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
				            success = check_ushortq_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_ulong_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_ulong_NewArrayBounds(ushort? size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong[]> f = e.Compile();
				
				        ulong[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ulong[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ulong[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2492
			namespace Scenario2492{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_ulong_NewArrayBounds__() {
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
				            success = check_shortq_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_ulong_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_ulong_NewArrayBounds(short? size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong[]> f = e.Compile();
				
				        ulong[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ulong[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ulong[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2493
			namespace Scenario2493{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_ulong_NewArrayBounds__() {
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
				            success = check_uintq_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_ulong_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_ulong_NewArrayBounds(uint? size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong[]> f = e.Compile();
				
				        ulong[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ulong[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ulong[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2494
			namespace Scenario2494{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_ulong_NewArrayBounds__() {
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
				            success = check_intq_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_ulong_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("intq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_ulong_NewArrayBounds(int? size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong[]> f = e.Compile();
				
				        ulong[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ulong[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ulong[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2495
			namespace Scenario2495{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_ulong_NewArrayBounds__() {
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
				            success = check_ulongq_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_ulong_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_ulong_NewArrayBounds(ulong? size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong[]> f = e.Compile();
				
				        ulong[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ulong[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ulong[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2496
			namespace Scenario2496{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_ulong_NewArrayBounds__() {
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
				            success = check_longq_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_ulong_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("longq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_ulong_NewArrayBounds(long? size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong[]> f = e.Compile();
				
				        ulong[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ulong[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new ulong[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2497
			namespace Scenario2497{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_sbyte_NewArrayBounds__() {
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
				            success = check_byteq_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_sbyte_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_sbyte_NewArrayBounds(byte? size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte[]> f = e.Compile();
				
				        sbyte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        sbyte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new sbyte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2498
			namespace Scenario2498{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_sbyte_NewArrayBounds__() {
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
				            success = check_sbyteq_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_sbyte_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_sbyte_NewArrayBounds(sbyte? size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte[]> f = e.Compile();
				
				        sbyte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        sbyte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new sbyte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2499
			namespace Scenario2499{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_sbyte_NewArrayBounds__() {
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
				            success = check_ushortq_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_sbyte_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_sbyte_NewArrayBounds(ushort? size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte[]> f = e.Compile();
				
				        sbyte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        sbyte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new sbyte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2500
			namespace Scenario2500{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_sbyte_NewArrayBounds__() {
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
				            success = check_shortq_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_sbyte_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_sbyte_NewArrayBounds(short? size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte[]> f = e.Compile();
				
				        sbyte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        sbyte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new sbyte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2501
			namespace Scenario2501{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_sbyte_NewArrayBounds__() {
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
				            success = check_uintq_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_sbyte_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_sbyte_NewArrayBounds(uint? size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte[]> f = e.Compile();
				
				        sbyte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        sbyte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new sbyte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2502
			namespace Scenario2502{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_sbyte_NewArrayBounds__() {
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
				            success = check_intq_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_sbyte_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("intq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_sbyte_NewArrayBounds(int? size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte[]> f = e.Compile();
				
				        sbyte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        sbyte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new sbyte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2503
			namespace Scenario2503{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_sbyte_NewArrayBounds__() {
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
				            success = check_ulongq_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_sbyte_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_sbyte_NewArrayBounds(ulong? size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte[]> f = e.Compile();
				
				        sbyte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        sbyte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new sbyte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2504
			namespace Scenario2504{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_sbyte_NewArrayBounds__() {
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
				            success = check_longq_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_sbyte_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("longq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_sbyte_NewArrayBounds(long? size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte[]> f = e.Compile();
				
				        sbyte[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        sbyte[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new sbyte[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2505
			namespace Scenario2505{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_short_NewArrayBounds__() {
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
				            success = check_byteq_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_short_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_short_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_short_NewArrayBounds(byte? size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short[]> f = e.Compile();
				
				        short[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        short[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new short[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2506
			namespace Scenario2506{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_short_NewArrayBounds__() {
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
				            success = check_sbyteq_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_short_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_short_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_short_NewArrayBounds(sbyte? size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short[]> f = e.Compile();
				
				        short[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        short[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new short[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2507
			namespace Scenario2507{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_short_NewArrayBounds__() {
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
				            success = check_ushortq_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_short_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_short_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_short_NewArrayBounds(ushort? size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short[]> f = e.Compile();
				
				        short[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        short[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new short[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2508
			namespace Scenario2508{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_short_NewArrayBounds__() {
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
				            success = check_shortq_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_short_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_short_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_short_NewArrayBounds(short? size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short[]> f = e.Compile();
				
				        short[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        short[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new short[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2509
			namespace Scenario2509{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_short_NewArrayBounds__() {
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
				            success = check_uintq_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_short_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_short_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_short_NewArrayBounds(uint? size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short[]> f = e.Compile();
				
				        short[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        short[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new short[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2510
			namespace Scenario2510{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_short_NewArrayBounds__() {
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
				            success = check_intq_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_short_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_short_NewArrayBounds(size)) {
				                Console.WriteLine("intq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_short_NewArrayBounds(int? size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short[]> f = e.Compile();
				
				        short[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        short[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new short[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2511
			namespace Scenario2511{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_short_NewArrayBounds__() {
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
				            success = check_ulongq_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_short_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_short_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_short_NewArrayBounds(ulong? size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short[]> f = e.Compile();
				
				        short[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        short[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new short[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2512
			namespace Scenario2512{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_short_NewArrayBounds__() {
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
				            success = check_longq_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_short_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_short_NewArrayBounds(size)) {
				                Console.WriteLine("longq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_short_NewArrayBounds(long? size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short[]> f = e.Compile();
				
				        short[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        short[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new short[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2513
			namespace Scenario2513{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_int_NewArrayBounds__() {
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
				            success = check_byteq_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_int_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_int_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_int_NewArrayBounds(byte? size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int[]> f = e.Compile();
				
				        int[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        int[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new int[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2514
			namespace Scenario2514{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_int_NewArrayBounds__() {
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
				            success = check_sbyteq_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_int_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_int_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_int_NewArrayBounds(sbyte? size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int[]> f = e.Compile();
				
				        int[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        int[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new int[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2515
			namespace Scenario2515{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_int_NewArrayBounds__() {
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
				            success = check_ushortq_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_int_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_int_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_int_NewArrayBounds(ushort? size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int[]> f = e.Compile();
				
				        int[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        int[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new int[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2516
			namespace Scenario2516{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_int_NewArrayBounds__() {
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
				            success = check_shortq_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_int_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_int_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_int_NewArrayBounds(short? size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int[]> f = e.Compile();
				
				        int[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        int[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new int[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2517
			namespace Scenario2517{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_int_NewArrayBounds__() {
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
				            success = check_uintq_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_int_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_int_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_int_NewArrayBounds(uint? size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int[]> f = e.Compile();
				
				        int[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        int[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new int[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2518
			namespace Scenario2518{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_int_NewArrayBounds__() {
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
				            success = check_intq_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_int_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_int_NewArrayBounds(size)) {
				                Console.WriteLine("intq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_int_NewArrayBounds(int? size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int[]> f = e.Compile();
				
				        int[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        int[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new int[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2519
			namespace Scenario2519{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_int_NewArrayBounds__() {
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
				            success = check_ulongq_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_int_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_int_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_int_NewArrayBounds(ulong? size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int[]> f = e.Compile();
				
				        int[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        int[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new int[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2520
			namespace Scenario2520{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_int_NewArrayBounds__() {
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
				            success = check_longq_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_int_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_int_NewArrayBounds(size)) {
				                Console.WriteLine("longq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_int_NewArrayBounds(long? size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int[]> f = e.Compile();
				
				        int[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        int[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new int[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2521
			namespace Scenario2521{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_long_NewArrayBounds__() {
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
				            success = check_byteq_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_long_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_long_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_long_NewArrayBounds(byte? size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long[]> f = e.Compile();
				
				        long[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        long[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new long[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2522
			namespace Scenario2522{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_long_NewArrayBounds__() {
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
				            success = check_sbyteq_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_long_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_long_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_long_NewArrayBounds(sbyte? size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long[]> f = e.Compile();
				
				        long[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        long[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new long[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2523
			namespace Scenario2523{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_long_NewArrayBounds__() {
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
				            success = check_ushortq_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_long_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_long_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_long_NewArrayBounds(ushort? size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long[]> f = e.Compile();
				
				        long[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        long[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new long[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2524
			namespace Scenario2524{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_long_NewArrayBounds__() {
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
				            success = check_shortq_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_long_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_long_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_long_NewArrayBounds(short? size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long[]> f = e.Compile();
				
				        long[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        long[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new long[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2525
			namespace Scenario2525{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_long_NewArrayBounds__() {
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
				            success = check_uintq_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_long_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_long_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_long_NewArrayBounds(uint? size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long[]> f = e.Compile();
				
				        long[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        long[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new long[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2526
			namespace Scenario2526{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_long_NewArrayBounds__() {
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
				            success = check_intq_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_long_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_long_NewArrayBounds(size)) {
				                Console.WriteLine("intq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_long_NewArrayBounds(int? size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long[]> f = e.Compile();
				
				        long[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        long[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new long[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2527
			namespace Scenario2527{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_long_NewArrayBounds__() {
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
				            success = check_ulongq_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_long_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_long_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_long_NewArrayBounds(ulong? size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long[]> f = e.Compile();
				
				        long[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        long[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new long[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2528
			namespace Scenario2528{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_long_NewArrayBounds__() {
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
				            success = check_longq_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_long_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_long_NewArrayBounds(size)) {
				                Console.WriteLine("longq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_long_NewArrayBounds(long? size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long[]> f = e.Compile();
				
				        long[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        long[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new long[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2529
			namespace Scenario2529{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_float_NewArrayBounds__() {
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
				            success = check_byteq_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_float_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_float_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_float_NewArrayBounds(byte? size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float[]> f = e.Compile();
				
				        float[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        float[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new float[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2530
			namespace Scenario2530{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_float_NewArrayBounds__() {
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
				            success = check_sbyteq_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_float_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_float_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_float_NewArrayBounds(sbyte? size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float[]> f = e.Compile();
				
				        float[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        float[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new float[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2531
			namespace Scenario2531{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_float_NewArrayBounds__() {
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
				            success = check_ushortq_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_float_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_float_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_float_NewArrayBounds(ushort? size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float[]> f = e.Compile();
				
				        float[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        float[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new float[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2532
			namespace Scenario2532{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_float_NewArrayBounds__() {
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
				            success = check_shortq_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_float_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_float_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_float_NewArrayBounds(short? size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float[]> f = e.Compile();
				
				        float[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        float[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new float[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2533
			namespace Scenario2533{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_float_NewArrayBounds__() {
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
				            success = check_uintq_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_float_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_float_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_float_NewArrayBounds(uint? size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float[]> f = e.Compile();
				
				        float[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        float[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new float[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2534
			namespace Scenario2534{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_float_NewArrayBounds__() {
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
				            success = check_intq_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_float_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_float_NewArrayBounds(size)) {
				                Console.WriteLine("intq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_float_NewArrayBounds(int? size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float[]> f = e.Compile();
				
				        float[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        float[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new float[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2535
			namespace Scenario2535{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_float_NewArrayBounds__() {
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
				            success = check_ulongq_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_float_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_float_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_float_NewArrayBounds(ulong? size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float[]> f = e.Compile();
				
				        float[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        float[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new float[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2536
			namespace Scenario2536{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_float_NewArrayBounds__() {
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
				            success = check_longq_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_float_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_float_NewArrayBounds(size)) {
				                Console.WriteLine("longq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_float_NewArrayBounds(long? size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float[]> f = e.Compile();
				
				        float[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        float[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new float[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2537
			namespace Scenario2537{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_double_NewArrayBounds__() {
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
				            success = check_byteq_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_double_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_double_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_double_NewArrayBounds(byte? size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double[]> f = e.Compile();
				
				        double[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        double[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new double[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2538
			namespace Scenario2538{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_double_NewArrayBounds__() {
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
				            success = check_sbyteq_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_double_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_double_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_double_NewArrayBounds(sbyte? size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double[]> f = e.Compile();
				
				        double[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        double[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new double[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2539
			namespace Scenario2539{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_double_NewArrayBounds__() {
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
				            success = check_ushortq_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_double_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_double_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_double_NewArrayBounds(ushort? size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double[]> f = e.Compile();
				
				        double[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        double[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new double[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2540
			namespace Scenario2540{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_double_NewArrayBounds__() {
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
				            success = check_shortq_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_double_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_double_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_double_NewArrayBounds(short? size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double[]> f = e.Compile();
				
				        double[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        double[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new double[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2541
			namespace Scenario2541{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_double_NewArrayBounds__() {
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
				            success = check_uintq_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_double_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_double_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_double_NewArrayBounds(uint? size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double[]> f = e.Compile();
				
				        double[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        double[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new double[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2542
			namespace Scenario2542{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_double_NewArrayBounds__() {
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
				            success = check_intq_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_double_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_double_NewArrayBounds(size)) {
				                Console.WriteLine("intq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_double_NewArrayBounds(int? size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double[]> f = e.Compile();
				
				        double[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        double[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new double[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2543
			namespace Scenario2543{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_double_NewArrayBounds__() {
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
				            success = check_ulongq_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_double_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_double_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_double_NewArrayBounds(ulong? size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double[]> f = e.Compile();
				
				        double[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        double[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new double[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2544
			namespace Scenario2544{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_double_NewArrayBounds__() {
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
				            success = check_longq_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_double_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_double_NewArrayBounds(size)) {
				                Console.WriteLine("longq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_double_NewArrayBounds(long? size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double[]> f = e.Compile();
				
				        double[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        double[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new double[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2545
			namespace Scenario2545{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_decimal_NewArrayBounds__() {
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
				            success = check_byteq_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_decimal_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_decimal_NewArrayBounds(byte? size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal[]> f = e.Compile();
				
				        decimal[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        decimal[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new decimal[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2546
			namespace Scenario2546{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_decimal_NewArrayBounds__() {
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
				            success = check_sbyteq_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_decimal_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_decimal_NewArrayBounds(sbyte? size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal[]> f = e.Compile();
				
				        decimal[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        decimal[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new decimal[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2547
			namespace Scenario2547{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_decimal_NewArrayBounds__() {
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
				            success = check_ushortq_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_decimal_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_decimal_NewArrayBounds(ushort? size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal[]> f = e.Compile();
				
				        decimal[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        decimal[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new decimal[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2548
			namespace Scenario2548{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_decimal_NewArrayBounds__() {
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
				            success = check_shortq_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_decimal_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_decimal_NewArrayBounds(short? size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal[]> f = e.Compile();
				
				        decimal[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        decimal[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new decimal[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2549
			namespace Scenario2549{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_decimal_NewArrayBounds__() {
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
				            success = check_uintq_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_decimal_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_decimal_NewArrayBounds(uint? size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal[]> f = e.Compile();
				
				        decimal[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        decimal[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new decimal[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2550
			namespace Scenario2550{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_decimal_NewArrayBounds__() {
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
				            success = check_intq_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_decimal_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("intq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_decimal_NewArrayBounds(int? size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal[]> f = e.Compile();
				
				        decimal[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        decimal[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new decimal[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2551
			namespace Scenario2551{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_decimal_NewArrayBounds__() {
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
				            success = check_ulongq_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_decimal_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_decimal_NewArrayBounds(ulong? size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal[]> f = e.Compile();
				
				        decimal[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        decimal[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new decimal[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2552
			namespace Scenario2552{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_decimal_NewArrayBounds__() {
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
				            success = check_longq_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_decimal_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("longq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_decimal_NewArrayBounds(long? size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal[]> f = e.Compile();
				
				        decimal[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        decimal[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new decimal[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2553
			namespace Scenario2553{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_char_NewArrayBounds__() {
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
				            success = check_byteq_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_char_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_char_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_char_NewArrayBounds(byte? size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char[]> f = e.Compile();
				
				        char[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        char[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new char[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2554
			namespace Scenario2554{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_char_NewArrayBounds__() {
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
				            success = check_sbyteq_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_char_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_char_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_char_NewArrayBounds(sbyte? size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char[]> f = e.Compile();
				
				        char[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        char[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new char[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2555
			namespace Scenario2555{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_char_NewArrayBounds__() {
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
				            success = check_ushortq_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_char_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_char_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_char_NewArrayBounds(ushort? size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char[]> f = e.Compile();
				
				        char[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        char[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new char[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2556
			namespace Scenario2556{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_char_NewArrayBounds__() {
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
				            success = check_shortq_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_char_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_char_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_char_NewArrayBounds(short? size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char[]> f = e.Compile();
				
				        char[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        char[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new char[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2557
			namespace Scenario2557{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_char_NewArrayBounds__() {
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
				            success = check_uintq_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_char_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_char_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_char_NewArrayBounds(uint? size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char[]> f = e.Compile();
				
				        char[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        char[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new char[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2558
			namespace Scenario2558{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_char_NewArrayBounds__() {
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
				            success = check_intq_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_char_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_char_NewArrayBounds(size)) {
				                Console.WriteLine("intq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_char_NewArrayBounds(int? size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char[]> f = e.Compile();
				
				        char[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        char[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new char[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2559
			namespace Scenario2559{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_char_NewArrayBounds__() {
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
				            success = check_ulongq_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_char_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_char_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_char_NewArrayBounds(ulong? size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char[]> f = e.Compile();
				
				        char[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        char[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new char[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2560
			namespace Scenario2560{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_char_NewArrayBounds__() {
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
				            success = check_longq_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_char_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_char_NewArrayBounds(size)) {
				                Console.WriteLine("longq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_char_NewArrayBounds(long? size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char[]> f = e.Compile();
				
				        char[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        char[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new char[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2561
			namespace Scenario2561{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_bool_NewArrayBounds__() {
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
				            success = check_byteq_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_bool_NewArrayBounds() {
				        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
				            if (!check_byteq_bool_NewArrayBounds(size)) {
				                Console.WriteLine("byteq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_bool_NewArrayBounds(byte? size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(byte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool[]> f = e.Compile();
				
				        bool[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        bool[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new bool[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("byteq_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byteq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2562
			namespace Scenario2562{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_bool_NewArrayBounds__() {
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
				            success = check_sbyteq_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_bool_NewArrayBounds() {
				        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_bool_NewArrayBounds(size)) {
				                Console.WriteLine("sbyteq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_bool_NewArrayBounds(sbyte? size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(sbyte?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool[]> f = e.Compile();
				
				        bool[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        bool[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new bool[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("sbyteq_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyteq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2563
			namespace Scenario2563{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_bool_NewArrayBounds__() {
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
				            success = check_ushortq_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_bool_NewArrayBounds() {
				        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_bool_NewArrayBounds(size)) {
				                Console.WriteLine("ushortq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_bool_NewArrayBounds(ushort? size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(ushort?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool[]> f = e.Compile();
				
				        bool[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        bool[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new bool[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ushortq_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushortq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2564
			namespace Scenario2564{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_bool_NewArrayBounds__() {
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
				            success = check_shortq_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_bool_NewArrayBounds() {
				        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_bool_NewArrayBounds(size)) {
				                Console.WriteLine("shortq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_bool_NewArrayBounds(short? size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(short?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool[]> f = e.Compile();
				
				        bool[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        bool[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new bool[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("shortq_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("shortq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2565
			namespace Scenario2565{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_bool_NewArrayBounds__() {
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
				            success = check_uintq_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_bool_NewArrayBounds() {
				        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
				            if (!check_uintq_bool_NewArrayBounds(size)) {
				                Console.WriteLine("uintq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_bool_NewArrayBounds(uint? size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(uint?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool[]> f = e.Compile();
				
				        bool[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        bool[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new bool[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("uintq_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uintq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2566
			namespace Scenario2566{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_bool_NewArrayBounds__() {
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
				            success = check_intq_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_bool_NewArrayBounds() {
				        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_bool_NewArrayBounds(size)) {
				                Console.WriteLine("intq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_bool_NewArrayBounds(int? size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(int?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool[]> f = e.Compile();
				
				        bool[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        bool[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new bool[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("intq_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("intq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2567
			namespace Scenario2567{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_bool_NewArrayBounds__() {
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
				            success = check_ulongq_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_bool_NewArrayBounds() {
				        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_bool_NewArrayBounds(size)) {
				                Console.WriteLine("ulongq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_bool_NewArrayBounds(ulong? size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(ulong?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool[]> f = e.Compile();
				
				        bool[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        bool[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new bool[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("ulongq_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulongq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
			
			//-------- Scenario 2568
			namespace Scenario2568{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_bool_NewArrayBounds__() {
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
				            success = check_longq_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_bool_NewArrayBounds() {
				        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_longq_bool_NewArrayBounds(size)) {
				                Console.WriteLine("longq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_bool_NewArrayBounds(long? size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(long?))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool[]> f = e.Compile();
				
				        bool[] result = null;
				        Exception fEx = null;
				        try {
				            result = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        bool[] expected = null;
				        Exception csEx = null;
				        try {
				            expected = new bool[(long) size];
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (csEx != null || fEx != null) {
				            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
				        }
				
				        if (result.Length != expected.Length) {
				            Console.WriteLine("longq_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("longq_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
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
				
				//-------- Scenario 2569
				namespace Scenario2569{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression byteq_S_NewArrayBounds__() {
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
					            success = check_byteq_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_byteq_S_NewArrayBounds() {
					        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
					            if (!check_byteq_S_NewArrayBounds(size)) {
					                Console.WriteLine("byteq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_byteq_S_NewArrayBounds(byte? size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(byte?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S[]> f = e.Compile();
					
					        S[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        S[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new S[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("byteq_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("byteq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2570
				namespace Scenario2570{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression sbyteq_S_NewArrayBounds__() {
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
					            success = check_sbyteq_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_sbyteq_S_NewArrayBounds() {
					        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
					            if (!check_sbyteq_S_NewArrayBounds(size)) {
					                Console.WriteLine("sbyteq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_sbyteq_S_NewArrayBounds(sbyte? size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(sbyte?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S[]> f = e.Compile();
					
					        S[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        S[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new S[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("sbyteq_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("sbyteq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2571
				namespace Scenario2571{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ushortq_S_NewArrayBounds__() {
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
					            success = check_ushortq_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushortq_S_NewArrayBounds() {
					        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
					            if (!check_ushortq_S_NewArrayBounds(size)) {
					                Console.WriteLine("ushortq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushortq_S_NewArrayBounds(ushort? size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(ushort?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S[]> f = e.Compile();
					
					        S[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        S[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new S[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("ushortq_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ushortq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2572
				namespace Scenario2572{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression shortq_S_NewArrayBounds__() {
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
					            success = check_shortq_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_shortq_S_NewArrayBounds() {
					        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
					            if (!check_shortq_S_NewArrayBounds(size)) {
					                Console.WriteLine("shortq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_shortq_S_NewArrayBounds(short? size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(short?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S[]> f = e.Compile();
					
					        S[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        S[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new S[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("shortq_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("shortq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2573
				namespace Scenario2573{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression uintq_S_NewArrayBounds__() {
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
					            success = check_uintq_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_uintq_S_NewArrayBounds() {
					        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
					            if (!check_uintq_S_NewArrayBounds(size)) {
					                Console.WriteLine("uintq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_uintq_S_NewArrayBounds(uint? size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(uint?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S[]> f = e.Compile();
					
					        S[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        S[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new S[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("uintq_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("uintq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2574
				namespace Scenario2574{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression intq_S_NewArrayBounds__() {
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
					            success = check_intq_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_intq_S_NewArrayBounds() {
					        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
					            if (!check_intq_S_NewArrayBounds(size)) {
					                Console.WriteLine("intq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_intq_S_NewArrayBounds(int? size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(int?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S[]> f = e.Compile();
					
					        S[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        S[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new S[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("intq_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("intq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2575
				namespace Scenario2575{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ulongq_S_NewArrayBounds__() {
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
					            success = check_ulongq_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ulongq_S_NewArrayBounds() {
					        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
					            if (!check_ulongq_S_NewArrayBounds(size)) {
					                Console.WriteLine("ulongq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulongq_S_NewArrayBounds(ulong? size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(ulong?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S[]> f = e.Compile();
					
					        S[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        S[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new S[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("ulongq_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ulongq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2576
				namespace Scenario2576{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression longq_S_NewArrayBounds__() {
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
					            success = check_longq_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_longq_S_NewArrayBounds() {
					        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
					            if (!check_longq_S_NewArrayBounds(size)) {
					                Console.WriteLine("longq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_longq_S_NewArrayBounds(long? size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(long?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S[]> f = e.Compile();
					
					        S[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        S[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new S[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("longq_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("longq_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2577
				namespace Scenario2577{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression byteq_Sp_NewArrayBounds__() {
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
					            success = check_byteq_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_byteq_Sp_NewArrayBounds() {
					        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
					            if (!check_byteq_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("byteq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_byteq_Sp_NewArrayBounds(byte? size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(byte?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp[]> f = e.Compile();
					
					        Sp[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sp[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sp[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("byteq_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("byteq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2578
				namespace Scenario2578{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression sbyteq_Sp_NewArrayBounds__() {
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
					            success = check_sbyteq_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_sbyteq_Sp_NewArrayBounds() {
					        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
					            if (!check_sbyteq_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("sbyteq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_sbyteq_Sp_NewArrayBounds(sbyte? size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(sbyte?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp[]> f = e.Compile();
					
					        Sp[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sp[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sp[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("sbyteq_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("sbyteq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2579
				namespace Scenario2579{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ushortq_Sp_NewArrayBounds__() {
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
					            success = check_ushortq_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushortq_Sp_NewArrayBounds() {
					        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
					            if (!check_ushortq_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("ushortq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushortq_Sp_NewArrayBounds(ushort? size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(ushort?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp[]> f = e.Compile();
					
					        Sp[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sp[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sp[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("ushortq_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ushortq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2580
				namespace Scenario2580{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression shortq_Sp_NewArrayBounds__() {
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
					            success = check_shortq_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_shortq_Sp_NewArrayBounds() {
					        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
					            if (!check_shortq_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("shortq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_shortq_Sp_NewArrayBounds(short? size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(short?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp[]> f = e.Compile();
					
					        Sp[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sp[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sp[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("shortq_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("shortq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2581
				namespace Scenario2581{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression uintq_Sp_NewArrayBounds__() {
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
					            success = check_uintq_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_uintq_Sp_NewArrayBounds() {
					        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
					            if (!check_uintq_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("uintq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_uintq_Sp_NewArrayBounds(uint? size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(uint?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp[]> f = e.Compile();
					
					        Sp[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sp[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sp[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("uintq_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("uintq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2582
				namespace Scenario2582{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression intq_Sp_NewArrayBounds__() {
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
					            success = check_intq_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_intq_Sp_NewArrayBounds() {
					        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
					            if (!check_intq_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("intq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_intq_Sp_NewArrayBounds(int? size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(int?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp[]> f = e.Compile();
					
					        Sp[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sp[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sp[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("intq_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("intq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2583
				namespace Scenario2583{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ulongq_Sp_NewArrayBounds__() {
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
					            success = check_ulongq_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ulongq_Sp_NewArrayBounds() {
					        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
					            if (!check_ulongq_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("ulongq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulongq_Sp_NewArrayBounds(ulong? size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(ulong?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp[]> f = e.Compile();
					
					        Sp[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sp[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sp[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("ulongq_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ulongq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2584
				namespace Scenario2584{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression longq_Sp_NewArrayBounds__() {
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
					            success = check_longq_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_longq_Sp_NewArrayBounds() {
					        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
					            if (!check_longq_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("longq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_longq_Sp_NewArrayBounds(long? size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(long?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp[]> f = e.Compile();
					
					        Sp[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sp[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sp[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("longq_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("longq_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2585
				namespace Scenario2585{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression byteq_Ss_NewArrayBounds__() {
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
					            success = check_byteq_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_byteq_Ss_NewArrayBounds() {
					        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
					            if (!check_byteq_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("byteq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_byteq_Ss_NewArrayBounds(byte? size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(byte?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss[]> f = e.Compile();
					
					        Ss[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ss[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Ss[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("byteq_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("byteq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2586
				namespace Scenario2586{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression sbyteq_Ss_NewArrayBounds__() {
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
					            success = check_sbyteq_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_sbyteq_Ss_NewArrayBounds() {
					        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
					            if (!check_sbyteq_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("sbyteq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_sbyteq_Ss_NewArrayBounds(sbyte? size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(sbyte?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss[]> f = e.Compile();
					
					        Ss[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ss[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Ss[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("sbyteq_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("sbyteq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2587
				namespace Scenario2587{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ushortq_Ss_NewArrayBounds__() {
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
					            success = check_ushortq_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushortq_Ss_NewArrayBounds() {
					        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
					            if (!check_ushortq_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("ushortq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushortq_Ss_NewArrayBounds(ushort? size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(ushort?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss[]> f = e.Compile();
					
					        Ss[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ss[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Ss[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("ushortq_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ushortq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2588
				namespace Scenario2588{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression shortq_Ss_NewArrayBounds__() {
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
					            success = check_shortq_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_shortq_Ss_NewArrayBounds() {
					        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
					            if (!check_shortq_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("shortq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_shortq_Ss_NewArrayBounds(short? size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(short?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss[]> f = e.Compile();
					
					        Ss[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ss[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Ss[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("shortq_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("shortq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2589
				namespace Scenario2589{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression uintq_Ss_NewArrayBounds__() {
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
					            success = check_uintq_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_uintq_Ss_NewArrayBounds() {
					        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
					            if (!check_uintq_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("uintq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_uintq_Ss_NewArrayBounds(uint? size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(uint?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss[]> f = e.Compile();
					
					        Ss[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ss[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Ss[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("uintq_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("uintq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2590
				namespace Scenario2590{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression intq_Ss_NewArrayBounds__() {
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
					            success = check_intq_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_intq_Ss_NewArrayBounds() {
					        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
					            if (!check_intq_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("intq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_intq_Ss_NewArrayBounds(int? size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(int?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss[]> f = e.Compile();
					
					        Ss[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ss[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Ss[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("intq_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("intq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2591
				namespace Scenario2591{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ulongq_Ss_NewArrayBounds__() {
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
					            success = check_ulongq_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ulongq_Ss_NewArrayBounds() {
					        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
					            if (!check_ulongq_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("ulongq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulongq_Ss_NewArrayBounds(ulong? size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(ulong?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss[]> f = e.Compile();
					
					        Ss[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ss[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Ss[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("ulongq_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ulongq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2592
				namespace Scenario2592{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression longq_Ss_NewArrayBounds__() {
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
					            success = check_longq_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_longq_Ss_NewArrayBounds() {
					        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
					            if (!check_longq_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("longq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_longq_Ss_NewArrayBounds(long? size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(long?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss[]> f = e.Compile();
					
					        Ss[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ss[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Ss[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("longq_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("longq_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2593
				namespace Scenario2593{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression byteq_Sc_NewArrayBounds__() {
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
					            success = check_byteq_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_byteq_Sc_NewArrayBounds() {
					        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
					            if (!check_byteq_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("byteq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_byteq_Sc_NewArrayBounds(byte? size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(byte?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc[]> f = e.Compile();
					
					        Sc[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sc[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sc[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("byteq_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("byteq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2594
				namespace Scenario2594{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression sbyteq_Sc_NewArrayBounds__() {
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
					            success = check_sbyteq_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_sbyteq_Sc_NewArrayBounds() {
					        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
					            if (!check_sbyteq_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("sbyteq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_sbyteq_Sc_NewArrayBounds(sbyte? size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(sbyte?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc[]> f = e.Compile();
					
					        Sc[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sc[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sc[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("sbyteq_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("sbyteq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2595
				namespace Scenario2595{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ushortq_Sc_NewArrayBounds__() {
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
					            success = check_ushortq_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushortq_Sc_NewArrayBounds() {
					        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
					            if (!check_ushortq_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("ushortq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushortq_Sc_NewArrayBounds(ushort? size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(ushort?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc[]> f = e.Compile();
					
					        Sc[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sc[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sc[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("ushortq_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ushortq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2596
				namespace Scenario2596{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression shortq_Sc_NewArrayBounds__() {
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
					            success = check_shortq_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_shortq_Sc_NewArrayBounds() {
					        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
					            if (!check_shortq_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("shortq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_shortq_Sc_NewArrayBounds(short? size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(short?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc[]> f = e.Compile();
					
					        Sc[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sc[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sc[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("shortq_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("shortq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2597
				namespace Scenario2597{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression uintq_Sc_NewArrayBounds__() {
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
					            success = check_uintq_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_uintq_Sc_NewArrayBounds() {
					        foreach (uint? size in new uint?[] { null, 0, 1, uint.MaxValue }) {
					            if (!check_uintq_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("uintq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_uintq_Sc_NewArrayBounds(uint? size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(uint?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc[]> f = e.Compile();
					
					        Sc[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sc[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sc[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("uintq_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("uintq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2598
				namespace Scenario2598{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression intq_Sc_NewArrayBounds__() {
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
					            success = check_intq_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_intq_Sc_NewArrayBounds() {
					        foreach (int? size in new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue }) {
					            if (!check_intq_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("intq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_intq_Sc_NewArrayBounds(int? size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(int?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc[]> f = e.Compile();
					
					        Sc[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sc[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sc[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("intq_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("intq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2599
				namespace Scenario2599{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ulongq_Sc_NewArrayBounds__() {
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
					            success = check_ulongq_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ulongq_Sc_NewArrayBounds() {
					        foreach (ulong? size in new ulong?[] { null, 0, 1, ulong.MaxValue }) {
					            if (!check_ulongq_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("ulongq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulongq_Sc_NewArrayBounds(ulong? size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(ulong?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc[]> f = e.Compile();
					
					        Sc[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sc[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sc[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("ulongq_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ulongq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2600
				namespace Scenario2600{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression longq_Sc_NewArrayBounds__() {
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
					            success = check_longq_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_longq_Sc_NewArrayBounds() {
					        foreach (long? size in new long?[] { null, 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
					            if (!check_longq_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("longq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_longq_Sc_NewArrayBounds(long? size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(long?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc[]> f = e.Compile();
					
					        Sc[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sc[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Sc[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("longq_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("longq_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2601
				namespace Scenario2601{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_Scs_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression byteq_Scs_NewArrayBounds__() {
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
					            success = check_byteq_Scs_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_byteq_Scs_NewArrayBounds() {
					        foreach (byte? size in new byte?[] { null, 0, 1, byte.MaxValue }) {
					            if (!check_byteq_Scs_NewArrayBounds(size)) {
					                Console.WriteLine("byteq_Scs_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_byteq_Scs_NewArrayBounds(byte? size) {
					        Expression<Func<Scs[]>> e =
					            Expression.Lambda<Func<Scs[]>>(
					                Expression.NewArrayBounds(typeof(Scs),
					                    Expression.Constant(size, typeof(byte?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs[]> f = e.Compile();
					
					        Scs[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Scs[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Scs[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("byteq_Scs_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("byteq_Scs_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2602
				namespace Scenario2602{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_Scs_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression sbyteq_Scs_NewArrayBounds__() {
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
					            success = check_sbyteq_Scs_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_sbyteq_Scs_NewArrayBounds() {
					        foreach (sbyte? size in new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
					            if (!check_sbyteq_Scs_NewArrayBounds(size)) {
					                Console.WriteLine("sbyteq_Scs_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_sbyteq_Scs_NewArrayBounds(sbyte? size) {
					        Expression<Func<Scs[]>> e =
					            Expression.Lambda<Func<Scs[]>>(
					                Expression.NewArrayBounds(typeof(Scs),
					                    Expression.Constant(size, typeof(sbyte?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs[]> f = e.Compile();
					
					        Scs[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Scs[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Scs[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("sbyteq_Scs_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("sbyteq_Scs_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2603
				namespace Scenario2603{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_Scs_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ushortq_Scs_NewArrayBounds__() {
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
					            success = check_ushortq_Scs_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushortq_Scs_NewArrayBounds() {
					        foreach (ushort? size in new ushort?[] { null, 0, 1, ushort.MaxValue }) {
					            if (!check_ushortq_Scs_NewArrayBounds(size)) {
					                Console.WriteLine("ushortq_Scs_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushortq_Scs_NewArrayBounds(ushort? size) {
					        Expression<Func<Scs[]>> e =
					            Expression.Lambda<Func<Scs[]>>(
					                Expression.NewArrayBounds(typeof(Scs),
					                    Expression.Constant(size, typeof(ushort?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs[]> f = e.Compile();
					
					        Scs[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Scs[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Scs[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("ushortq_Scs_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ushortq_Scs_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
				
				//-------- Scenario 2604
				namespace Scenario2604{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_Scs_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression shortq_Scs_NewArrayBounds__() {
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
					            success = check_shortq_Scs_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_shortq_Scs_NewArrayBounds() {
					        foreach (short? size in new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue }) {
					            if (!check_shortq_Scs_NewArrayBounds(size)) {
					                Console.WriteLine("shortq_Scs_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_shortq_Scs_NewArrayBounds(short? size) {
					        Expression<Func<Scs[]>> e =
					            Expression.Lambda<Func<Scs[]>>(
					                Expression.NewArrayBounds(typeof(Scs),
					                    Expression.Constant(size, typeof(short?))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs[]> f = e.Compile();
					
					        Scs[] result = null;
					        Exception fEx = null;
					        try {
					            result = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Scs[] expected = null;
					        Exception csEx = null;
					        try {
					            expected = new Scs[(long) size];
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (csEx != null || fEx != null) {
					            return csEx != null && fEx != null && csEx.GetType() == fEx.GetType();
					        }
					
					        if (result.Length != expected.Length) {
					            Console.WriteLine("shortq_Scs_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("shortq_Scs_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
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
