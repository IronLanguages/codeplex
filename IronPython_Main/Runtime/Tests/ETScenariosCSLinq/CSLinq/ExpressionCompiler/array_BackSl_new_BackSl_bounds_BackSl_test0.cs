#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 2325
			namespace Scenario2325{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_NewArrayBounds(size)) {
				                Console.WriteLine("byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_NewArrayBounds(byte size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_NewArrayBounds failed");
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
			
			//-------- Scenario 2326
			namespace Scenario2326{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_byte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_byte_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_byte_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_byte_NewArrayBounds(sbyte size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_byte_NewArrayBounds failed");
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
			
			//-------- Scenario 2327
			namespace Scenario2327{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_byte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_byte_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_byte_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_byte_NewArrayBounds(ushort size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_byte_NewArrayBounds failed");
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
			
			//-------- Scenario 2328
			namespace Scenario2328{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_byte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_byte_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_byte_NewArrayBounds(size)) {
				                Console.WriteLine("short_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_byte_NewArrayBounds(short size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_byte_NewArrayBounds failed");
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
			
			//-------- Scenario 2329
			namespace Scenario2329{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_byte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_byte_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_byte_NewArrayBounds(size)) {
				                Console.WriteLine("uint_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_byte_NewArrayBounds(uint size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_byte_NewArrayBounds failed");
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
			
			//-------- Scenario 2330
			namespace Scenario2330{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_byte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_byte_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_byte_NewArrayBounds(size)) {
				                Console.WriteLine("int_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_byte_NewArrayBounds(int size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_byte_NewArrayBounds failed");
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
			
			//-------- Scenario 2331
			namespace Scenario2331{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_byte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_byte_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_byte_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_byte_NewArrayBounds(ulong size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_byte_NewArrayBounds failed");
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
			
			//-------- Scenario 2332
			namespace Scenario2332{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_byte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_byte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_byte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_byte_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_byte_NewArrayBounds(size)) {
				                Console.WriteLine("long_byte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_byte_NewArrayBounds(long size) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayBounds(typeof(byte),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_byte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_byte_NewArrayBounds failed");
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
			
			//-------- Scenario 2333
			namespace Scenario2333{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_ushort_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_ushort_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("byte_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_ushort_NewArrayBounds(byte size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_ushort_NewArrayBounds failed");
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
			
			//-------- Scenario 2334
			namespace Scenario2334{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_ushort_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_ushort_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_ushort_NewArrayBounds(sbyte size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_ushort_NewArrayBounds failed");
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
			
			//-------- Scenario 2335
			namespace Scenario2335{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_NewArrayBounds(ushort size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_NewArrayBounds failed");
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
			
			//-------- Scenario 2336
			namespace Scenario2336{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_ushort_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_ushort_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("short_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_ushort_NewArrayBounds(short size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_ushort_NewArrayBounds failed");
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
			
			//-------- Scenario 2337
			namespace Scenario2337{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_ushort_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_ushort_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("uint_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_ushort_NewArrayBounds(uint size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_ushort_NewArrayBounds failed");
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
			
			//-------- Scenario 2338
			namespace Scenario2338{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_ushort_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_ushort_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("int_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_ushort_NewArrayBounds(int size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_ushort_NewArrayBounds failed");
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
			
			//-------- Scenario 2339
			namespace Scenario2339{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_ushort_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_ushort_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_ushort_NewArrayBounds(ulong size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_ushort_NewArrayBounds failed");
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
			
			//-------- Scenario 2340
			namespace Scenario2340{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_ushort_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_ushort_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_ushort_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_ushort_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_ushort_NewArrayBounds(size)) {
				                Console.WriteLine("long_ushort_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_ushort_NewArrayBounds(long size) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayBounds(typeof(ushort),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_ushort_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_ushort_NewArrayBounds failed");
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
			
			//-------- Scenario 2341
			namespace Scenario2341{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_uint_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_uint_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_uint_NewArrayBounds(size)) {
				                Console.WriteLine("byte_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_uint_NewArrayBounds(byte size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_uint_NewArrayBounds failed");
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
			
			//-------- Scenario 2342
			namespace Scenario2342{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_uint_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_uint_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_uint_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_uint_NewArrayBounds(sbyte size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_uint_NewArrayBounds failed");
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
			
			//-------- Scenario 2343
			namespace Scenario2343{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_uint_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_uint_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_uint_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_uint_NewArrayBounds(ushort size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_uint_NewArrayBounds failed");
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
			
			//-------- Scenario 2344
			namespace Scenario2344{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_uint_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_uint_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_uint_NewArrayBounds(size)) {
				                Console.WriteLine("short_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_uint_NewArrayBounds(short size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_uint_NewArrayBounds failed");
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
			
			//-------- Scenario 2345
			namespace Scenario2345{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_NewArrayBounds(size)) {
				                Console.WriteLine("uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_NewArrayBounds(uint size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_NewArrayBounds failed");
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
			
			//-------- Scenario 2346
			namespace Scenario2346{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_uint_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_uint_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_uint_NewArrayBounds(size)) {
				                Console.WriteLine("int_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_uint_NewArrayBounds(int size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_uint_NewArrayBounds failed");
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
			
			//-------- Scenario 2347
			namespace Scenario2347{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_uint_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_uint_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_uint_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_uint_NewArrayBounds(ulong size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_uint_NewArrayBounds failed");
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
			
			//-------- Scenario 2348
			namespace Scenario2348{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_uint_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_uint_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_uint_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_uint_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_uint_NewArrayBounds(size)) {
				                Console.WriteLine("long_uint_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_uint_NewArrayBounds(long size) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayBounds(typeof(uint),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_uint_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_uint_NewArrayBounds failed");
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
			
			//-------- Scenario 2349
			namespace Scenario2349{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_ulong_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_ulong_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("byte_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_ulong_NewArrayBounds(byte size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_ulong_NewArrayBounds failed");
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
			
			//-------- Scenario 2350
			namespace Scenario2350{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_ulong_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_ulong_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_ulong_NewArrayBounds(sbyte size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_ulong_NewArrayBounds failed");
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
			
			//-------- Scenario 2351
			namespace Scenario2351{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_ulong_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_ulong_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_ulong_NewArrayBounds(ushort size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_ulong_NewArrayBounds failed");
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
			
			//-------- Scenario 2352
			namespace Scenario2352{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_ulong_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_ulong_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("short_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_ulong_NewArrayBounds(short size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_ulong_NewArrayBounds failed");
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
			
			//-------- Scenario 2353
			namespace Scenario2353{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_ulong_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_ulong_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("uint_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_ulong_NewArrayBounds(uint size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_ulong_NewArrayBounds failed");
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
			
			//-------- Scenario 2354
			namespace Scenario2354{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_ulong_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_ulong_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("int_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_ulong_NewArrayBounds(int size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_ulong_NewArrayBounds failed");
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
			
			//-------- Scenario 2355
			namespace Scenario2355{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_NewArrayBounds(ulong size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_NewArrayBounds failed");
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
			
			//-------- Scenario 2356
			namespace Scenario2356{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_ulong_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_ulong_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_ulong_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_ulong_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_ulong_NewArrayBounds(size)) {
				                Console.WriteLine("long_ulong_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_ulong_NewArrayBounds(long size) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayBounds(typeof(ulong),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_ulong_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_ulong_NewArrayBounds failed");
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
			
			//-------- Scenario 2357
			namespace Scenario2357{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_sbyte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_sbyte_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("byte_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_sbyte_NewArrayBounds(byte size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_sbyte_NewArrayBounds failed");
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
			
			//-------- Scenario 2358
			namespace Scenario2358{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_NewArrayBounds(sbyte size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_NewArrayBounds failed");
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
			
			//-------- Scenario 2359
			namespace Scenario2359{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_sbyte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_sbyte_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_sbyte_NewArrayBounds(ushort size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_sbyte_NewArrayBounds failed");
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
			
			//-------- Scenario 2360
			namespace Scenario2360{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_sbyte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_sbyte_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("short_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_sbyte_NewArrayBounds(short size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_sbyte_NewArrayBounds failed");
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
			
			//-------- Scenario 2361
			namespace Scenario2361{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_sbyte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_sbyte_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("uint_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_sbyte_NewArrayBounds(uint size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_sbyte_NewArrayBounds failed");
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
			
			//-------- Scenario 2362
			namespace Scenario2362{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_sbyte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_sbyte_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("int_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_sbyte_NewArrayBounds(int size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_sbyte_NewArrayBounds failed");
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
			
			//-------- Scenario 2363
			namespace Scenario2363{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_sbyte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_sbyte_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_sbyte_NewArrayBounds(ulong size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_sbyte_NewArrayBounds failed");
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
			
			//-------- Scenario 2364
			namespace Scenario2364{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_sbyte_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_sbyte_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_sbyte_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_sbyte_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_sbyte_NewArrayBounds(size)) {
				                Console.WriteLine("long_sbyte_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_sbyte_NewArrayBounds(long size) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayBounds(typeof(sbyte),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_sbyte_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_sbyte_NewArrayBounds failed");
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
			
			//-------- Scenario 2365
			namespace Scenario2365{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_short_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_short_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_short_NewArrayBounds(size)) {
				                Console.WriteLine("byte_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_short_NewArrayBounds(byte size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_short_NewArrayBounds failed");
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
			
			//-------- Scenario 2366
			namespace Scenario2366{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_short_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_short_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_short_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_short_NewArrayBounds(sbyte size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_short_NewArrayBounds failed");
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
			
			//-------- Scenario 2367
			namespace Scenario2367{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_short_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_short_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_short_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_short_NewArrayBounds(ushort size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_short_NewArrayBounds failed");
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
			
			//-------- Scenario 2368
			namespace Scenario2368{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_NewArrayBounds(size)) {
				                Console.WriteLine("short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_NewArrayBounds(short size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_NewArrayBounds failed");
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
			
			//-------- Scenario 2369
			namespace Scenario2369{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_short_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_short_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_short_NewArrayBounds(size)) {
				                Console.WriteLine("uint_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_short_NewArrayBounds(uint size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_short_NewArrayBounds failed");
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
			
			//-------- Scenario 2370
			namespace Scenario2370{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_short_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_short_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_short_NewArrayBounds(size)) {
				                Console.WriteLine("int_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_short_NewArrayBounds(int size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_short_NewArrayBounds failed");
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
			
			//-------- Scenario 2371
			namespace Scenario2371{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_short_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_short_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_short_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_short_NewArrayBounds(ulong size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_short_NewArrayBounds failed");
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
			
			//-------- Scenario 2372
			namespace Scenario2372{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_short_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_short_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_short_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_short_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_short_NewArrayBounds(size)) {
				                Console.WriteLine("long_short_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_short_NewArrayBounds(long size) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayBounds(typeof(short),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_short_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_short_NewArrayBounds failed");
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
			
			//-------- Scenario 2373
			namespace Scenario2373{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_int_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_int_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_int_NewArrayBounds(size)) {
				                Console.WriteLine("byte_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_int_NewArrayBounds(byte size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_int_NewArrayBounds failed");
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
			
			//-------- Scenario 2374
			namespace Scenario2374{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_int_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_int_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_int_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_int_NewArrayBounds(sbyte size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_int_NewArrayBounds failed");
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
			
			//-------- Scenario 2375
			namespace Scenario2375{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_int_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_int_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_int_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_int_NewArrayBounds(ushort size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_int_NewArrayBounds failed");
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
			
			//-------- Scenario 2376
			namespace Scenario2376{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_int_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_int_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_int_NewArrayBounds(size)) {
				                Console.WriteLine("short_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_int_NewArrayBounds(short size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_int_NewArrayBounds failed");
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
			
			//-------- Scenario 2377
			namespace Scenario2377{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_int_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_int_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_int_NewArrayBounds(size)) {
				                Console.WriteLine("uint_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_int_NewArrayBounds(uint size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_int_NewArrayBounds failed");
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
			
			//-------- Scenario 2378
			namespace Scenario2378{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_NewArrayBounds(size)) {
				                Console.WriteLine("int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_NewArrayBounds(int size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_NewArrayBounds failed");
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
			
			//-------- Scenario 2379
			namespace Scenario2379{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_int_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_int_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_int_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_int_NewArrayBounds(ulong size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_int_NewArrayBounds failed");
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
			
			//-------- Scenario 2380
			namespace Scenario2380{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_int_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_int_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_int_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_int_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_int_NewArrayBounds(size)) {
				                Console.WriteLine("long_int_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_int_NewArrayBounds(long size) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayBounds(typeof(int),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_int_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_int_NewArrayBounds failed");
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
			
			//-------- Scenario 2381
			namespace Scenario2381{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_long_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_long_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_long_NewArrayBounds(size)) {
				                Console.WriteLine("byte_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_long_NewArrayBounds(byte size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_long_NewArrayBounds failed");
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
			
			//-------- Scenario 2382
			namespace Scenario2382{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_long_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_long_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_long_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_long_NewArrayBounds(sbyte size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_long_NewArrayBounds failed");
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
			
			//-------- Scenario 2383
			namespace Scenario2383{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_long_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_long_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_long_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_long_NewArrayBounds(ushort size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_long_NewArrayBounds failed");
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
			
			//-------- Scenario 2384
			namespace Scenario2384{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_long_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_long_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_long_NewArrayBounds(size)) {
				                Console.WriteLine("short_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_long_NewArrayBounds(short size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_long_NewArrayBounds failed");
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
			
			//-------- Scenario 2385
			namespace Scenario2385{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_long_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_long_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_long_NewArrayBounds(size)) {
				                Console.WriteLine("uint_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_long_NewArrayBounds(uint size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_long_NewArrayBounds failed");
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
			
			//-------- Scenario 2386
			namespace Scenario2386{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_long_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_long_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_long_NewArrayBounds(size)) {
				                Console.WriteLine("int_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_long_NewArrayBounds(int size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_long_NewArrayBounds failed");
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
			
			//-------- Scenario 2387
			namespace Scenario2387{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_long_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_long_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_long_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_long_NewArrayBounds(ulong size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_long_NewArrayBounds failed");
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
			
			//-------- Scenario 2388
			namespace Scenario2388{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_NewArrayBounds(size)) {
				                Console.WriteLine("long_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_NewArrayBounds(long size) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayBounds(typeof(long),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_NewArrayBounds failed");
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
			
			//-------- Scenario 2389
			namespace Scenario2389{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_float_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_float_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_float_NewArrayBounds(size)) {
				                Console.WriteLine("byte_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_float_NewArrayBounds(byte size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_float_NewArrayBounds failed");
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
			
			//-------- Scenario 2390
			namespace Scenario2390{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_float_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_float_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_float_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_float_NewArrayBounds(sbyte size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_float_NewArrayBounds failed");
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
			
			//-------- Scenario 2391
			namespace Scenario2391{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_float_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_float_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_float_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_float_NewArrayBounds(ushort size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_float_NewArrayBounds failed");
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
			
			//-------- Scenario 2392
			namespace Scenario2392{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_float_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_float_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_float_NewArrayBounds(size)) {
				                Console.WriteLine("short_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_float_NewArrayBounds(short size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_float_NewArrayBounds failed");
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
			
			//-------- Scenario 2393
			namespace Scenario2393{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_float_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_float_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_float_NewArrayBounds(size)) {
				                Console.WriteLine("uint_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_float_NewArrayBounds(uint size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_float_NewArrayBounds failed");
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
			
			//-------- Scenario 2394
			namespace Scenario2394{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_float_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_float_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_float_NewArrayBounds(size)) {
				                Console.WriteLine("int_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_float_NewArrayBounds(int size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_float_NewArrayBounds failed");
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
			
			//-------- Scenario 2395
			namespace Scenario2395{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_float_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_float_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_float_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_float_NewArrayBounds(ulong size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_float_NewArrayBounds failed");
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
			
			//-------- Scenario 2396
			namespace Scenario2396{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_float_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_float_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_float_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_float_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_float_NewArrayBounds(size)) {
				                Console.WriteLine("long_float_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_float_NewArrayBounds(long size) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayBounds(typeof(float),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_float_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_float_NewArrayBounds failed");
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
			
			//-------- Scenario 2397
			namespace Scenario2397{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_double_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_double_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_double_NewArrayBounds(size)) {
				                Console.WriteLine("byte_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_double_NewArrayBounds(byte size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_double_NewArrayBounds failed");
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
			
			//-------- Scenario 2398
			namespace Scenario2398{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_double_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_double_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_double_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_double_NewArrayBounds(sbyte size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_double_NewArrayBounds failed");
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
			
			//-------- Scenario 2399
			namespace Scenario2399{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_double_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_double_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_double_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_double_NewArrayBounds(ushort size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_double_NewArrayBounds failed");
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
			
			//-------- Scenario 2400
			namespace Scenario2400{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_double_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_double_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_double_NewArrayBounds(size)) {
				                Console.WriteLine("short_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_double_NewArrayBounds(short size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_double_NewArrayBounds failed");
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
			
			//-------- Scenario 2401
			namespace Scenario2401{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_double_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_double_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_double_NewArrayBounds(size)) {
				                Console.WriteLine("uint_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_double_NewArrayBounds(uint size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_double_NewArrayBounds failed");
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
			
			//-------- Scenario 2402
			namespace Scenario2402{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_double_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_double_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_double_NewArrayBounds(size)) {
				                Console.WriteLine("int_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_double_NewArrayBounds(int size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_double_NewArrayBounds failed");
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
			
			//-------- Scenario 2403
			namespace Scenario2403{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_double_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_double_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_double_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_double_NewArrayBounds(ulong size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_double_NewArrayBounds failed");
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
			
			//-------- Scenario 2404
			namespace Scenario2404{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_double_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_double_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_double_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_double_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_double_NewArrayBounds(size)) {
				                Console.WriteLine("long_double_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_double_NewArrayBounds(long size) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayBounds(typeof(double),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_double_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_double_NewArrayBounds failed");
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
			
			//-------- Scenario 2405
			namespace Scenario2405{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_decimal_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_decimal_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("byte_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_decimal_NewArrayBounds(byte size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_decimal_NewArrayBounds failed");
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
			
			//-------- Scenario 2406
			namespace Scenario2406{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_decimal_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_decimal_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_decimal_NewArrayBounds(sbyte size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_decimal_NewArrayBounds failed");
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
			
			//-------- Scenario 2407
			namespace Scenario2407{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_decimal_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_decimal_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_decimal_NewArrayBounds(ushort size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_decimal_NewArrayBounds failed");
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
			
			//-------- Scenario 2408
			namespace Scenario2408{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_decimal_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_decimal_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("short_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_decimal_NewArrayBounds(short size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_decimal_NewArrayBounds failed");
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
			
			//-------- Scenario 2409
			namespace Scenario2409{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_decimal_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_decimal_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("uint_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_decimal_NewArrayBounds(uint size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_decimal_NewArrayBounds failed");
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
			
			//-------- Scenario 2410
			namespace Scenario2410{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_decimal_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_decimal_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("int_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_decimal_NewArrayBounds(int size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_decimal_NewArrayBounds failed");
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
			
			//-------- Scenario 2411
			namespace Scenario2411{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_decimal_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_decimal_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_decimal_NewArrayBounds(ulong size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_decimal_NewArrayBounds failed");
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
			
			//-------- Scenario 2412
			namespace Scenario2412{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_decimal_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_decimal_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_decimal_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_decimal_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_decimal_NewArrayBounds(size)) {
				                Console.WriteLine("long_decimal_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_decimal_NewArrayBounds(long size) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayBounds(typeof(decimal),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_decimal_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_decimal_NewArrayBounds failed");
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
			
			//-------- Scenario 2413
			namespace Scenario2413{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_char_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_char_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_char_NewArrayBounds(size)) {
				                Console.WriteLine("byte_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_char_NewArrayBounds(byte size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_char_NewArrayBounds failed");
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
			
			//-------- Scenario 2414
			namespace Scenario2414{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_char_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_char_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_char_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_char_NewArrayBounds(sbyte size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_char_NewArrayBounds failed");
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
			
			//-------- Scenario 2415
			namespace Scenario2415{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_char_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_char_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_char_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_char_NewArrayBounds(ushort size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_char_NewArrayBounds failed");
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
			
			//-------- Scenario 2416
			namespace Scenario2416{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_char_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_char_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_char_NewArrayBounds(size)) {
				                Console.WriteLine("short_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_char_NewArrayBounds(short size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_char_NewArrayBounds failed");
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
			
			//-------- Scenario 2417
			namespace Scenario2417{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_char_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_char_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_char_NewArrayBounds(size)) {
				                Console.WriteLine("uint_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_char_NewArrayBounds(uint size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_char_NewArrayBounds failed");
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
			
			//-------- Scenario 2418
			namespace Scenario2418{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_char_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_char_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_char_NewArrayBounds(size)) {
				                Console.WriteLine("int_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_char_NewArrayBounds(int size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_char_NewArrayBounds failed");
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
			
			//-------- Scenario 2419
			namespace Scenario2419{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_char_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_char_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_char_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_char_NewArrayBounds(ulong size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_char_NewArrayBounds failed");
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
			
			//-------- Scenario 2420
			namespace Scenario2420{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_char_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_char_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_char_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_char_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_char_NewArrayBounds(size)) {
				                Console.WriteLine("long_char_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_char_NewArrayBounds(long size) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayBounds(typeof(char),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_char_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_char_NewArrayBounds failed");
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
			
			//-------- Scenario 2421
			namespace Scenario2421{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_bool_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_bool_NewArrayBounds() {
				        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_bool_NewArrayBounds(size)) {
				                Console.WriteLine("byte_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_bool_NewArrayBounds(byte size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(byte))),
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
				            Console.WriteLine("byte_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("byte_bool_NewArrayBounds failed");
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
			
			//-------- Scenario 2422
			namespace Scenario2422{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_bool_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_bool_NewArrayBounds() {
				        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_bool_NewArrayBounds(size)) {
				                Console.WriteLine("sbyte_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_bool_NewArrayBounds(sbyte size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(sbyte))),
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
				            Console.WriteLine("sbyte_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("sbyte_bool_NewArrayBounds failed");
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
			
			//-------- Scenario 2423
			namespace Scenario2423{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_bool_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_bool_NewArrayBounds() {
				        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_bool_NewArrayBounds(size)) {
				                Console.WriteLine("ushort_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_bool_NewArrayBounds(ushort size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(ushort))),
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
				            Console.WriteLine("ushort_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ushort_bool_NewArrayBounds failed");
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
			
			//-------- Scenario 2424
			namespace Scenario2424{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_bool_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_bool_NewArrayBounds() {
				        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_bool_NewArrayBounds(size)) {
				                Console.WriteLine("short_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_bool_NewArrayBounds(short size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(short))),
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
				            Console.WriteLine("short_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("short_bool_NewArrayBounds failed");
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
			
			//-------- Scenario 2425
			namespace Scenario2425{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_bool_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_bool_NewArrayBounds() {
				        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_bool_NewArrayBounds(size)) {
				                Console.WriteLine("uint_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_bool_NewArrayBounds(uint size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(uint))),
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
				            Console.WriteLine("uint_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("uint_bool_NewArrayBounds failed");
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
			
			//-------- Scenario 2426
			namespace Scenario2426{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_bool_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_bool_NewArrayBounds() {
				        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_bool_NewArrayBounds(size)) {
				                Console.WriteLine("int_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_bool_NewArrayBounds(int size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(int))),
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
				            Console.WriteLine("int_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("int_bool_NewArrayBounds failed");
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
			
			//-------- Scenario 2427
			namespace Scenario2427{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_bool_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_bool_NewArrayBounds() {
				        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_bool_NewArrayBounds(size)) {
				                Console.WriteLine("ulong_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_bool_NewArrayBounds(ulong size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(ulong))),
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
				            Console.WriteLine("ulong_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("ulong_bool_NewArrayBounds failed");
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
			
			//-------- Scenario 2428
			namespace Scenario2428{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_bool_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_bool_NewArrayBounds__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_bool_NewArrayBounds();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_bool_NewArrayBounds() {
				        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
				            if (!check_long_bool_NewArrayBounds(size)) {
				                Console.WriteLine("long_bool_NewArrayBounds failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_bool_NewArrayBounds(long size) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayBounds(typeof(bool),
				                    Expression.Constant(size, typeof(long))),
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
				            Console.WriteLine("long_bool_NewArrayBounds failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], expected[i])) {
				                Console.WriteLine("long_bool_NewArrayBounds failed");
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
				
				//-------- Scenario 2429
				namespace Scenario2429{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression byte_S_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_byte_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_byte_S_NewArrayBounds() {
					        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
					            if (!check_byte_S_NewArrayBounds(size)) {
					                Console.WriteLine("byte_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_byte_S_NewArrayBounds(byte size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(byte))),
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
					            Console.WriteLine("byte_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("byte_S_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2430
				namespace Scenario2430{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression sbyte_S_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_sbyte_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_sbyte_S_NewArrayBounds() {
					        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
					            if (!check_sbyte_S_NewArrayBounds(size)) {
					                Console.WriteLine("sbyte_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_sbyte_S_NewArrayBounds(sbyte size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(sbyte))),
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
					            Console.WriteLine("sbyte_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("sbyte_S_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2431
				namespace Scenario2431{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ushort_S_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ushort_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushort_S_NewArrayBounds() {
					        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
					            if (!check_ushort_S_NewArrayBounds(size)) {
					                Console.WriteLine("ushort_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushort_S_NewArrayBounds(ushort size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(ushort))),
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
					            Console.WriteLine("ushort_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ushort_S_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2432
				namespace Scenario2432{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression short_S_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_short_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_short_S_NewArrayBounds() {
					        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
					            if (!check_short_S_NewArrayBounds(size)) {
					                Console.WriteLine("short_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_short_S_NewArrayBounds(short size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(short))),
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
					            Console.WriteLine("short_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("short_S_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2433
				namespace Scenario2433{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression uint_S_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_uint_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_uint_S_NewArrayBounds() {
					        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
					            if (!check_uint_S_NewArrayBounds(size)) {
					                Console.WriteLine("uint_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_uint_S_NewArrayBounds(uint size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(uint))),
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
					            Console.WriteLine("uint_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("uint_S_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2434
				namespace Scenario2434{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression int_S_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_int_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_int_S_NewArrayBounds() {
					        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
					            if (!check_int_S_NewArrayBounds(size)) {
					                Console.WriteLine("int_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_int_S_NewArrayBounds(int size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(int))),
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
					            Console.WriteLine("int_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("int_S_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2435
				namespace Scenario2435{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ulong_S_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ulong_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ulong_S_NewArrayBounds() {
					        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
					            if (!check_ulong_S_NewArrayBounds(size)) {
					                Console.WriteLine("ulong_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulong_S_NewArrayBounds(ulong size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(ulong))),
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
					            Console.WriteLine("ulong_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ulong_S_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2436
				namespace Scenario2436{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_S_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression long_S_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_long_S_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_long_S_NewArrayBounds() {
					        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
					            if (!check_long_S_NewArrayBounds(size)) {
					                Console.WriteLine("long_S_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_long_S_NewArrayBounds(long size) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayBounds(typeof(S),
					                    Expression.Constant(size, typeof(long))),
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
					            Console.WriteLine("long_S_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("long_S_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2437
				namespace Scenario2437{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression byte_Sp_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_byte_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_byte_Sp_NewArrayBounds() {
					        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
					            if (!check_byte_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("byte_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_byte_Sp_NewArrayBounds(byte size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(byte))),
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
					            Console.WriteLine("byte_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("byte_Sp_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2438
				namespace Scenario2438{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression sbyte_Sp_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_sbyte_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_sbyte_Sp_NewArrayBounds() {
					        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
					            if (!check_sbyte_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("sbyte_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_sbyte_Sp_NewArrayBounds(sbyte size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(sbyte))),
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
					            Console.WriteLine("sbyte_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("sbyte_Sp_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2439
				namespace Scenario2439{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ushort_Sp_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ushort_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushort_Sp_NewArrayBounds() {
					        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
					            if (!check_ushort_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("ushort_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushort_Sp_NewArrayBounds(ushort size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(ushort))),
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
					            Console.WriteLine("ushort_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ushort_Sp_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2440
				namespace Scenario2440{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression short_Sp_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_short_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_short_Sp_NewArrayBounds() {
					        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
					            if (!check_short_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("short_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_short_Sp_NewArrayBounds(short size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(short))),
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
					            Console.WriteLine("short_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("short_Sp_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2441
				namespace Scenario2441{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression uint_Sp_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_uint_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_uint_Sp_NewArrayBounds() {
					        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
					            if (!check_uint_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("uint_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_uint_Sp_NewArrayBounds(uint size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(uint))),
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
					            Console.WriteLine("uint_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("uint_Sp_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2442
				namespace Scenario2442{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression int_Sp_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_int_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_int_Sp_NewArrayBounds() {
					        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
					            if (!check_int_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("int_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_int_Sp_NewArrayBounds(int size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(int))),
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
					            Console.WriteLine("int_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("int_Sp_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2443
				namespace Scenario2443{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ulong_Sp_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ulong_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ulong_Sp_NewArrayBounds() {
					        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
					            if (!check_ulong_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("ulong_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulong_Sp_NewArrayBounds(ulong size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(ulong))),
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
					            Console.WriteLine("ulong_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ulong_Sp_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2444
				namespace Scenario2444{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_Sp_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression long_Sp_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_long_Sp_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_long_Sp_NewArrayBounds() {
					        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
					            if (!check_long_Sp_NewArrayBounds(size)) {
					                Console.WriteLine("long_Sp_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_long_Sp_NewArrayBounds(long size) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayBounds(typeof(Sp),
					                    Expression.Constant(size, typeof(long))),
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
					            Console.WriteLine("long_Sp_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("long_Sp_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2445
				namespace Scenario2445{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression byte_Ss_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_byte_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_byte_Ss_NewArrayBounds() {
					        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
					            if (!check_byte_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("byte_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_byte_Ss_NewArrayBounds(byte size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(byte))),
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
					            Console.WriteLine("byte_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("byte_Ss_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2446
				namespace Scenario2446{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression sbyte_Ss_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_sbyte_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_sbyte_Ss_NewArrayBounds() {
					        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
					            if (!check_sbyte_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("sbyte_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_sbyte_Ss_NewArrayBounds(sbyte size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(sbyte))),
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
					            Console.WriteLine("sbyte_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("sbyte_Ss_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2447
				namespace Scenario2447{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ushort_Ss_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ushort_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushort_Ss_NewArrayBounds() {
					        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
					            if (!check_ushort_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("ushort_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushort_Ss_NewArrayBounds(ushort size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(ushort))),
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
					            Console.WriteLine("ushort_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ushort_Ss_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2448
				namespace Scenario2448{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression short_Ss_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_short_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_short_Ss_NewArrayBounds() {
					        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
					            if (!check_short_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("short_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_short_Ss_NewArrayBounds(short size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(short))),
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
					            Console.WriteLine("short_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("short_Ss_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2449
				namespace Scenario2449{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression uint_Ss_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_uint_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_uint_Ss_NewArrayBounds() {
					        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
					            if (!check_uint_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("uint_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_uint_Ss_NewArrayBounds(uint size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(uint))),
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
					            Console.WriteLine("uint_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("uint_Ss_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2450
				namespace Scenario2450{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression int_Ss_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_int_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_int_Ss_NewArrayBounds() {
					        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
					            if (!check_int_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("int_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_int_Ss_NewArrayBounds(int size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(int))),
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
					            Console.WriteLine("int_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("int_Ss_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2451
				namespace Scenario2451{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ulong_Ss_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ulong_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ulong_Ss_NewArrayBounds() {
					        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
					            if (!check_ulong_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("ulong_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulong_Ss_NewArrayBounds(ulong size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(ulong))),
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
					            Console.WriteLine("ulong_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ulong_Ss_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2452
				namespace Scenario2452{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_Ss_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression long_Ss_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_long_Ss_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_long_Ss_NewArrayBounds() {
					        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
					            if (!check_long_Ss_NewArrayBounds(size)) {
					                Console.WriteLine("long_Ss_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_long_Ss_NewArrayBounds(long size) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayBounds(typeof(Ss),
					                    Expression.Constant(size, typeof(long))),
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
					            Console.WriteLine("long_Ss_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("long_Ss_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2453
				namespace Scenario2453{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression byte_Sc_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_byte_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_byte_Sc_NewArrayBounds() {
					        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
					            if (!check_byte_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("byte_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_byte_Sc_NewArrayBounds(byte size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(byte))),
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
					            Console.WriteLine("byte_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("byte_Sc_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2454
				namespace Scenario2454{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression sbyte_Sc_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_sbyte_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_sbyte_Sc_NewArrayBounds() {
					        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
					            if (!check_sbyte_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("sbyte_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_sbyte_Sc_NewArrayBounds(sbyte size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(sbyte))),
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
					            Console.WriteLine("sbyte_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("sbyte_Sc_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2455
				namespace Scenario2455{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ushort_Sc_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ushort_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushort_Sc_NewArrayBounds() {
					        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
					            if (!check_ushort_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("ushort_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushort_Sc_NewArrayBounds(ushort size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(ushort))),
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
					            Console.WriteLine("ushort_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ushort_Sc_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2456
				namespace Scenario2456{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression short_Sc_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_short_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_short_Sc_NewArrayBounds() {
					        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
					            if (!check_short_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("short_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_short_Sc_NewArrayBounds(short size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(short))),
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
					            Console.WriteLine("short_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("short_Sc_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2457
				namespace Scenario2457{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression uint_Sc_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_uint_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_uint_Sc_NewArrayBounds() {
					        foreach (uint size in new uint[] { 0, 1, uint.MaxValue }) {
					            if (!check_uint_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("uint_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_uint_Sc_NewArrayBounds(uint size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(uint))),
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
					            Console.WriteLine("uint_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("uint_Sc_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2458
				namespace Scenario2458{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression int_Sc_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_int_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_int_Sc_NewArrayBounds() {
					        foreach (int size in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
					            if (!check_int_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("int_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_int_Sc_NewArrayBounds(int size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(int))),
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
					            Console.WriteLine("int_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("int_Sc_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2459
				namespace Scenario2459{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ulong_Sc_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ulong_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ulong_Sc_NewArrayBounds() {
					        foreach (ulong size in new ulong[] { 0, 1, ulong.MaxValue }) {
					            if (!check_ulong_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("ulong_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ulong_Sc_NewArrayBounds(ulong size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(ulong))),
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
					            Console.WriteLine("ulong_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ulong_Sc_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2460
				namespace Scenario2460{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_Sc_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression long_Sc_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_long_Sc_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_long_Sc_NewArrayBounds() {
					        foreach (long size in new long[] { 0, 1, -1, (long)int.MinValue, long.MaxValue }) {
					            if (!check_long_Sc_NewArrayBounds(size)) {
					                Console.WriteLine("long_Sc_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_long_Sc_NewArrayBounds(long size) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayBounds(typeof(Sc),
					                    Expression.Constant(size, typeof(long))),
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
					            Console.WriteLine("long_Sc_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("long_Sc_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2461
				namespace Scenario2461{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_Scs_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression byte_Scs_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_byte_Scs_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_byte_Scs_NewArrayBounds() {
					        foreach (byte size in new byte[] { 0, 1, byte.MaxValue }) {
					            if (!check_byte_Scs_NewArrayBounds(size)) {
					                Console.WriteLine("byte_Scs_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_byte_Scs_NewArrayBounds(byte size) {
					        Expression<Func<Scs[]>> e =
					            Expression.Lambda<Func<Scs[]>>(
					                Expression.NewArrayBounds(typeof(Scs),
					                    Expression.Constant(size, typeof(byte))),
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
					            Console.WriteLine("byte_Scs_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("byte_Scs_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2462
				namespace Scenario2462{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_Scs_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression sbyte_Scs_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_sbyte_Scs_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_sbyte_Scs_NewArrayBounds() {
					        foreach (sbyte size in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
					            if (!check_sbyte_Scs_NewArrayBounds(size)) {
					                Console.WriteLine("sbyte_Scs_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_sbyte_Scs_NewArrayBounds(sbyte size) {
					        Expression<Func<Scs[]>> e =
					            Expression.Lambda<Func<Scs[]>>(
					                Expression.NewArrayBounds(typeof(Scs),
					                    Expression.Constant(size, typeof(sbyte))),
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
					            Console.WriteLine("sbyte_Scs_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("sbyte_Scs_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2463
				namespace Scenario2463{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_Scs_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ushort_Scs_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ushort_Scs_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ushort_Scs_NewArrayBounds() {
					        foreach (ushort size in new ushort[] { 0, 1, ushort.MaxValue }) {
					            if (!check_ushort_Scs_NewArrayBounds(size)) {
					                Console.WriteLine("ushort_Scs_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_ushort_Scs_NewArrayBounds(ushort size) {
					        Expression<Func<Scs[]>> e =
					            Expression.Lambda<Func<Scs[]>>(
					                Expression.NewArrayBounds(typeof(Scs),
					                    Expression.Constant(size, typeof(ushort))),
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
					            Console.WriteLine("ushort_Scs_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("ushort_Scs_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
				
				//-------- Scenario 2464
				namespace Scenario2464{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_Scs_NewArrayBounds__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression short_Scs_NewArrayBounds__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_short_Scs_NewArrayBounds();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_short_Scs_NewArrayBounds() {
					        foreach (short size in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
					            if (!check_short_Scs_NewArrayBounds(size)) {
					                Console.WriteLine("short_Scs_NewArrayBounds failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_short_Scs_NewArrayBounds(short size) {
					        Expression<Func<Scs[]>> e =
					            Expression.Lambda<Func<Scs[]>>(
					                Expression.NewArrayBounds(typeof(Scs),
					                    Expression.Constant(size, typeof(short))),
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
					            Console.WriteLine("short_Scs_NewArrayBounds failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], expected[i])) {
					                Console.WriteLine("short_Scs_NewArrayBounds failed");
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
			
			enum E {
			  A=1, B=2
			}
			
			enum El : long {
			  A, B, C
			}
			
			struct S : IEquatable<S> {
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
			
			struct Sp : IEquatable<Sp> {
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
			
			struct Ss : IEquatable<Ss> {
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
			
			struct Sc : IEquatable<Sc> {
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
			
			struct Scs : IEquatable<Scs> {
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
