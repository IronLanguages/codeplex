#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 2181
			namespace Scenario2181{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_byteq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_byteq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_byteq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_byteq_Coalesce() {
				        byte?[] svals0 = new byte?[] { null, 0, 1, byte.MaxValue };
				        byte[] svals1 = new byte[] { 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_byte_byteq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("byte_byteq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_byteq_Coalesce(byte? val0, byte val1) {
				        Expression<Func<byte>> e =
				            Expression.Lambda<Func<byte>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(byte?)),
				                    Expression.Constant(val1, typeof(byte))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte> f = e.Compile();
				
				        byte fResult = default(byte);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        byte csResult = default(byte);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2182
			namespace Scenario2182{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_ushortq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_ushortq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_ushortq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_ushortq_Coalesce() {
				        ushort?[] svals0 = new ushort?[] { null, 0, 1, ushort.MaxValue };
				        ushort[] svals1 = new ushort[] { 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_ushort_ushortq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("ushort_ushortq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_ushortq_Coalesce(ushort? val0, ushort val1) {
				        Expression<Func<ushort>> e =
				            Expression.Lambda<Func<ushort>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(ushort?)),
				                    Expression.Constant(val1, typeof(ushort))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort> f = e.Compile();
				
				        ushort fResult = default(ushort);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ushort csResult = default(ushort);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2183
			namespace Scenario2183{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_uintq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_uintq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_uintq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_uintq_Coalesce() {
				        uint?[] svals0 = new uint?[] { null, 0, 1, uint.MaxValue };
				        uint[] svals1 = new uint[] { 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_uint_uintq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("uint_uintq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_uintq_Coalesce(uint? val0, uint val1) {
				        Expression<Func<uint>> e =
				            Expression.Lambda<Func<uint>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(uint?)),
				                    Expression.Constant(val1, typeof(uint))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint> f = e.Compile();
				
				        uint fResult = default(uint);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        uint csResult = default(uint);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2184
			namespace Scenario2184{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_ulongq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_ulongq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_ulongq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_ulongq_Coalesce() {
				        ulong?[] svals0 = new ulong?[] { null, 0, 1, ulong.MaxValue };
				        ulong[] svals1 = new ulong[] { 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_ulong_ulongq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("ulong_ulongq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_ulongq_Coalesce(ulong? val0, ulong val1) {
				        Expression<Func<ulong>> e =
				            Expression.Lambda<Func<ulong>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(ulong?)),
				                    Expression.Constant(val1, typeof(ulong))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong> f = e.Compile();
				
				        ulong fResult = default(ulong);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ulong csResult = default(ulong);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2185
			namespace Scenario2185{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_sbyteq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_sbyteq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_sbyteq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_sbyteq_Coalesce() {
				        sbyte?[] svals0 = new sbyte?[] { null, 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        sbyte[] svals1 = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_sbyte_sbyteq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("sbyte_sbyteq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_sbyteq_Coalesce(sbyte? val0, sbyte val1) {
				        Expression<Func<sbyte>> e =
				            Expression.Lambda<Func<sbyte>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(sbyte?)),
				                    Expression.Constant(val1, typeof(sbyte))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte> f = e.Compile();
				
				        sbyte fResult = default(sbyte);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        sbyte csResult = default(sbyte);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2186
			namespace Scenario2186{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_shortq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_shortq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_shortq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_shortq_Coalesce() {
				        short?[] svals0 = new short?[] { null, 0, 1, -1, short.MinValue, short.MaxValue };
				        short[] svals1 = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_short_shortq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("short_shortq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_shortq_Coalesce(short? val0, short val1) {
				        Expression<Func<short>> e =
				            Expression.Lambda<Func<short>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(short?)),
				                    Expression.Constant(val1, typeof(short))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short> f = e.Compile();
				
				        short fResult = default(short);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        short csResult = default(short);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2187
			namespace Scenario2187{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_intq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_intq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_intq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_intq_Coalesce() {
				        int?[] svals0 = new int?[] { null, 0, 1, -1, int.MinValue, int.MaxValue };
				        int[] svals1 = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_int_intq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("int_intq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_intq_Coalesce(int? val0, int val1) {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(int?)),
				                    Expression.Constant(val1, typeof(int))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f = e.Compile();
				
				        int fResult = default(int);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        int csResult = default(int);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2188
			namespace Scenario2188{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_longq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_longq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_longq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_longq_Coalesce() {
				        long?[] svals0 = new long?[] { null, 0, 1, -1, long.MinValue, long.MaxValue };
				        long[] svals1 = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_long_longq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("long_longq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_longq_Coalesce(long? val0, long val1) {
				        Expression<Func<long>> e =
				            Expression.Lambda<Func<long>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(long?)),
				                    Expression.Constant(val1, typeof(long))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long> f = e.Compile();
				
				        long fResult = default(long);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        long csResult = default(long);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2189
			namespace Scenario2189{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "float_floatq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression float_floatq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_float_floatq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_float_floatq_Coalesce() {
				        float?[] svals0 = new float?[] { null, 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        float[] svals1 = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_float_floatq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("float_floatq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_floatq_Coalesce(float? val0, float val1) {
				        Expression<Func<float>> e =
				            Expression.Lambda<Func<float>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(float?)),
				                    Expression.Constant(val1, typeof(float))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float> f = e.Compile();
				
				        float fResult = default(float);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        float csResult = default(float);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2190
			namespace Scenario2190{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "double_doubleq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression double_doubleq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_double_doubleq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_double_doubleq_Coalesce() {
				        double?[] svals0 = new double?[] { null, 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        double[] svals1 = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_double_doubleq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("double_doubleq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_doubleq_Coalesce(double? val0, double val1) {
				        Expression<Func<double>> e =
				            Expression.Lambda<Func<double>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(double?)),
				                    Expression.Constant(val1, typeof(double))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double> f = e.Compile();
				
				        double fResult = default(double);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        double csResult = default(double);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2191
			namespace Scenario2191{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "decimal_decimalq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression decimal_decimalq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_decimal_decimalq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_decimal_decimalq_Coalesce() {
				        decimal?[] svals0 = new decimal?[] { null, decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        decimal[] svals1 = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_decimal_decimalq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("decimal_decimalq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_decimalq_Coalesce(decimal? val0, decimal val1) {
				        Expression<Func<decimal>> e =
				            Expression.Lambda<Func<decimal>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(decimal?)),
				                    Expression.Constant(val1, typeof(decimal))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal> f = e.Compile();
				
				        decimal fResult = default(decimal);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        decimal csResult = default(decimal);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2192
			namespace Scenario2192{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "char_charq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression char_charq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_char_charq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_char_charq_Coalesce() {
				        char?[] svals0 = new char?[] { null, '\0', '\b', 'A', '\uffff' };
				        char[] svals1 = new char[] { '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_char_charq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("char_charq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_charq_Coalesce(char? val0, char val1) {
				        Expression<Func<char>> e =
				            Expression.Lambda<Func<char>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(char?)),
				                    Expression.Constant(val1, typeof(char))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char> f = e.Compile();
				
				        char fResult = default(char);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        char csResult = default(char);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2193
			namespace Scenario2193{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "bool_boolq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression bool_boolq_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_bool_boolq_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_bool_boolq_Coalesce() {
				        bool?[] svals0 = new bool?[] { null, true, false };
				        bool[] svals1 = new bool[] { true, false };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_bool_boolq_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("bool_boolq_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_bool_boolq_Coalesce(bool? val0, bool val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(bool?)),
				                    Expression.Constant(val1, typeof(bool))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool> f = e.Compile();
				
				        bool fResult = default(bool);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        bool csResult = default(bool);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
				
				//-------- Scenario 2194
				namespace Scenario2194{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "S_Sq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression S_Sq_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_S_Sq_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_S_Sq_Coalesce() {
					        S?[] svals0 = new S?[] { null, default(S), new S() };
					        S[] svals1 = new S[] { default(S), new S() };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_S_Sq_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("S_Sq_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_S_Sq_Coalesce(S? val0, S val1) {
					        Expression<Func<S>> e =
					            Expression.Lambda<Func<S>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(S?)),
					                    Expression.Constant(val1, typeof(S))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S> f = e.Compile();
					
					        S fResult = default(S);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        S csResult = default(S);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2195
				namespace Scenario2195{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Sp_Spq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Sp_Spq_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Sp_Spq_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Sp_Spq_Coalesce() {
					        Sp?[] svals0 = new Sp?[] { null, default(Sp), new Sp(), new Sp(5,5.0) };
					        Sp[] svals1 = new Sp[] { default(Sp), new Sp(), new Sp(5,5.0) };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_Sp_Spq_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("Sp_Spq_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_Sp_Spq_Coalesce(Sp? val0, Sp val1) {
					        Expression<Func<Sp>> e =
					            Expression.Lambda<Func<Sp>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(Sp?)),
					                    Expression.Constant(val1, typeof(Sp))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp> f = e.Compile();
					
					        Sp fResult = default(Sp);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sp csResult = default(Sp);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2196
				namespace Scenario2196{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ss_Ssq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ss_Ssq_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ss_Ssq_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ss_Ssq_Coalesce() {
					        Ss?[] svals0 = new Ss?[] { null, default(Ss), new Ss(), new Ss(new S()) };
					        Ss[] svals1 = new Ss[] { default(Ss), new Ss(), new Ss(new S()) };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_Ss_Ssq_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("Ss_Ssq_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ss_Ssq_Coalesce(Ss? val0, Ss val1) {
					        Expression<Func<Ss>> e =
					            Expression.Lambda<Func<Ss>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(Ss?)),
					                    Expression.Constant(val1, typeof(Ss))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss> f = e.Compile();
					
					        Ss fResult = default(Ss);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ss csResult = default(Ss);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2197
				namespace Scenario2197{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Sc_Scq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Sc_Scq_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Sc_Scq_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Sc_Scq_Coalesce() {
					        Sc?[] svals0 = new Sc?[] { null, default(Sc), new Sc(), new Sc(null) };
					        Sc[] svals1 = new Sc[] { default(Sc), new Sc(), new Sc(null) };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_Sc_Scq_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("Sc_Scq_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_Sc_Scq_Coalesce(Sc? val0, Sc val1) {
					        Expression<Func<Sc>> e =
					            Expression.Lambda<Func<Sc>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(Sc?)),
					                    Expression.Constant(val1, typeof(Sc))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc> f = e.Compile();
					
					        Sc fResult = default(Sc);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sc csResult = default(Sc);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2198
				namespace Scenario2198{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Scs_Scsq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Scs_Scsq_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Scs_Scsq_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Scs_Scsq_Coalesce() {
					        Scs?[] svals0 = new Scs?[] { null, default(Scs), new Scs(), new Scs(null,new S()) };
					        Scs[] svals1 = new Scs[] { default(Scs), new Scs(), new Scs(null,new S()) };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_Scs_Scsq_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("Scs_Scsq_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_Scs_Scsq_Coalesce(Scs? val0, Scs val1) {
					        Expression<Func<Scs>> e =
					            Expression.Lambda<Func<Scs>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(Scs?)),
					                    Expression.Constant(val1, typeof(Scs))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs> f = e.Compile();
					
					        Scs fResult = default(Scs);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Scs csResult = default(Scs);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2199
				namespace Scenario2199{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ts_Tsq_Coalesce_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ts_Tsq_Coalesce_S___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ts_Tsq_Coalesce<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ts_Tsq_Coalesce<Ts>() where Ts : struct {
					        Ts?[] svals0 = new Ts?[] { null, default(Ts), new Ts() };
					        Ts[] svals1 = new Ts[] { default(Ts), new Ts() };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_Ts_Tsq_Coalesce<Ts>(svals0[i], svals1[j])) {
					                    Console.WriteLine("Ts_Tsq_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ts_Tsq_Coalesce<Ts>(Ts? val0, Ts val1) where Ts : struct {
					        Expression<Func<Ts>> e =
					            Expression.Lambda<Func<Ts>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(Ts?)),
					                    Expression.Constant(val1, typeof(Ts))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts> f = e.Compile();
					
					        Ts fResult = default(Ts);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ts csResult = default(Ts);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2200
				namespace Scenario2200{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ts_Tsq_Coalesce_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ts_Tsq_Coalesce_Scs___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ts_Tsq_Coalesce<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ts_Tsq_Coalesce<Ts>() where Ts : struct {
					        Ts?[] svals0 = new Ts?[] { null, default(Ts), new Ts() };
					        Ts[] svals1 = new Ts[] { default(Ts), new Ts() };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_Ts_Tsq_Coalesce<Ts>(svals0[i], svals1[j])) {
					                    Console.WriteLine("Ts_Tsq_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ts_Tsq_Coalesce<Ts>(Ts? val0, Ts val1) where Ts : struct {
					        Expression<Func<Ts>> e =
					            Expression.Lambda<Func<Ts>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(Ts?)),
					                    Expression.Constant(val1, typeof(Ts))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts> f = e.Compile();
					
					        Ts fResult = default(Ts);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ts csResult = default(Ts);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2201
				namespace Scenario2201{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ts_Tsq_Coalesce_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ts_Tsq_Coalesce_E___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ts_Tsq_Coalesce<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ts_Tsq_Coalesce<Ts>() where Ts : struct {
					        Ts?[] svals0 = new Ts?[] { null, default(Ts), new Ts() };
					        Ts[] svals1 = new Ts[] { default(Ts), new Ts() };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_Ts_Tsq_Coalesce<Ts>(svals0[i], svals1[j])) {
					                    Console.WriteLine("Ts_Tsq_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ts_Tsq_Coalesce<Ts>(Ts? val0, Ts val1) where Ts : struct {
					        Expression<Func<Ts>> e =
					            Expression.Lambda<Func<Ts>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(Ts?)),
					                    Expression.Constant(val1, typeof(Ts))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts> f = e.Compile();
					
					        Ts fResult = default(Ts);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ts csResult = default(Ts);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2202
				namespace Scenario2202{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "E_Eq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression E_Eq_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_E_Eq_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_E_Eq_Coalesce() {
					        E?[] svals0 = new E?[] { null, (E) 0, E.A, E.B, (E) int.MaxValue, (E) int.MinValue };
					        E[] svals1 = new E[] { (E) 0, E.A, E.B, (E) int.MaxValue, (E) int.MinValue };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_E_Eq_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("E_Eq_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_E_Eq_Coalesce(E? val0, E val1) {
					        Expression<Func<E>> e =
					            Expression.Lambda<Func<E>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(E?)),
					                    Expression.Constant(val1, typeof(E))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<E> f = e.Compile();
					
					        E fResult = default(E);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        E csResult = default(E);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2203
				namespace Scenario2203{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "El_Elq_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression El_Elq_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_El_Elq_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_El_Elq_Coalesce() {
					        El?[] svals0 = new El?[] { null, (El) 0, El.A, El.B, (El) long.MaxValue, (El) long.MinValue };
					        El[] svals1 = new El[] { (El) 0, El.A, El.B, (El) long.MaxValue, (El) long.MinValue };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_El_Elq_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("El_Elq_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_El_Elq_Coalesce(El? val0, El val1) {
					        Expression<Func<El>> e =
					            Expression.Lambda<Func<El>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(El?)),
					                    Expression.Constant(val1, typeof(El))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<El> f = e.Compile();
					
					        El fResult = default(El);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        El csResult = default(El);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
			
			//-------- Scenario 2204
			namespace Scenario2204{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "string_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression string_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_string_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_string_Coalesce() {
				        string[] svals0 = new string[] { null, "", "a", "foo" };
				        string[] svals1 = new string[] { null, "", "a", "foo" };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_string_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("string_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_string_Coalesce(string val0, string val1) {
				        Expression<Func<string>> e =
				            Expression.Lambda<Func<string>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(string)),
				                    Expression.Constant(val1, typeof(string))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<string> f = e.Compile();
				
				        string fResult = default(string);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        string csResult = default(string);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
				
				//-------- Scenario 2205
				namespace Scenario2205{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "object_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression object_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_object_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_object_Coalesce() {
					        object[] svals0 = new object[] { null, new object(), new C(), new D(3) };
					        object[] svals1 = new object[] { null, new object(), new C(), new D(3) };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_object_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("object_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_object_Coalesce(object val0, object val1) {
					        Expression<Func<object>> e =
					            Expression.Lambda<Func<object>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(object)),
					                    Expression.Constant(val1, typeof(object))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<object> f = e.Compile();
					
					        object fResult = default(object);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        object csResult = default(object);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2206
				namespace Scenario2206{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "C_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression C_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_C_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_C_Coalesce() {
					        C[] svals0 = new C[] { null, new C(), new D(), new D(0), new D(5) };
					        C[] svals1 = new C[] { null, new C(), new D(), new D(0), new D(5) };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_C_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("C_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_C_Coalesce(C val0, C val1) {
					        Expression<Func<C>> e =
					            Expression.Lambda<Func<C>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(C)),
					                    Expression.Constant(val1, typeof(C))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<C> f = e.Compile();
					
					        C fResult = default(C);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        C csResult = default(C);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2207
				namespace Scenario2207{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "D_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression D_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_D_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_D_Coalesce() {
					        D[] svals0 = new D[] { null, new D(), new D(0), new D(5) };
					        D[] svals1 = new D[] { null, new D(), new D(0), new D(5) };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_D_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("D_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_D_Coalesce(D val0, D val1) {
					        Expression<Func<D>> e =
					            Expression.Lambda<Func<D>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(D)),
					                    Expression.Constant(val1, typeof(D))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<D> f = e.Compile();
					
					        D fResult = default(D);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        D csResult = default(D);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2208
				namespace Scenario2208{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tc_Coalesce_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tc_Coalesce_object___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tc_Coalesce<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tc_Coalesce<Tc>() where Tc : class {
					        Tc[] svals0 = new Tc[] { null, default(Tc) };
					        Tc[] svals1 = new Tc[] { null, default(Tc) };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_Tc_Coalesce<Tc>(svals0[i], svals1[j])) {
					                    Console.WriteLine("Tc_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tc_Coalesce<Tc>(Tc val0, Tc val1) where Tc : class {
					        Expression<Func<Tc>> e =
					            Expression.Lambda<Func<Tc>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(Tc)),
					                    Expression.Constant(val1, typeof(Tc))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc> f = e.Compile();
					
					        Tc fResult = default(Tc);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Tc csResult = default(Tc);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2209
				namespace Scenario2209{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tc_Coalesce_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tc_Coalesce_C___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tc_Coalesce<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tc_Coalesce<Tc>() where Tc : class {
					        Tc[] svals0 = new Tc[] { null, default(Tc) };
					        Tc[] svals1 = new Tc[] { null, default(Tc) };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_Tc_Coalesce<Tc>(svals0[i], svals1[j])) {
					                    Console.WriteLine("Tc_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tc_Coalesce<Tc>(Tc val0, Tc val1) where Tc : class {
					        Expression<Func<Tc>> e =
					            Expression.Lambda<Func<Tc>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(Tc)),
					                    Expression.Constant(val1, typeof(Tc))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc> f = e.Compile();
					
					        Tc fResult = default(Tc);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Tc csResult = default(Tc);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2210
				namespace Scenario2210{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tcn_Coalesce_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tcn_Coalesce_object___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tcn_Coalesce<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tcn_Coalesce<Tcn>() where Tcn : class, new() {
					        Tcn[] svals0 = new Tcn[] { null, default(Tcn), new Tcn() };
					        Tcn[] svals1 = new Tcn[] { null, default(Tcn), new Tcn() };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_Tcn_Coalesce<Tcn>(svals0[i], svals1[j])) {
					                    Console.WriteLine("Tcn_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tcn_Coalesce<Tcn>(Tcn val0, Tcn val1) where Tcn : class, new() {
					        Expression<Func<Tcn>> e =
					            Expression.Lambda<Func<Tcn>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(Tcn)),
					                    Expression.Constant(val1, typeof(Tcn))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn> f = e.Compile();
					
					        Tcn fResult = default(Tcn);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Tcn csResult = default(Tcn);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2211
				namespace Scenario2211{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tcn_Coalesce_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tcn_Coalesce_C___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tcn_Coalesce<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tcn_Coalesce<Tcn>() where Tcn : class, new() {
					        Tcn[] svals0 = new Tcn[] { null, default(Tcn), new Tcn() };
					        Tcn[] svals1 = new Tcn[] { null, default(Tcn), new Tcn() };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_Tcn_Coalesce<Tcn>(svals0[i], svals1[j])) {
					                    Console.WriteLine("Tcn_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tcn_Coalesce<Tcn>(Tcn val0, Tcn val1) where Tcn : class, new() {
					        Expression<Func<Tcn>> e =
					            Expression.Lambda<Func<Tcn>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(Tcn)),
					                    Expression.Constant(val1, typeof(Tcn))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn> f = e.Compile();
					
					        Tcn fResult = default(Tcn);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Tcn csResult = default(Tcn);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2212
				namespace Scenario2212{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TC_Coalesce_C_a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TC_Coalesce_C_a__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TC_Coalesce<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TC_Coalesce<TC>() where TC : C {
					        TC[] svals0 = new TC[] { null, default(TC), (TC) new C() };
					        TC[] svals1 = new TC[] { null, default(TC), (TC) new C() };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_TC_Coalesce<TC>(svals0[i], svals1[j])) {
					                    Console.WriteLine("TC_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TC_Coalesce<TC>(TC val0, TC val1) where TC : C {
					        Expression<Func<TC>> e =
					            Expression.Lambda<Func<TC>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(TC)),
					                    Expression.Constant(val1, typeof(TC))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<TC> f = e.Compile();
					
					        TC fResult = default(TC);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        TC csResult = default(TC);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2213
				namespace Scenario2213{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TCn_Coalesce_C_a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TCn_Coalesce_C_a__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TCn_Coalesce<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TCn_Coalesce<TCn>() where TCn : C, new() {
					        TCn[] svals0 = new TCn[] { null, default(TCn), new TCn(), (TCn) new C() };
					        TCn[] svals1 = new TCn[] { null, default(TCn), new TCn(), (TCn) new C() };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_TCn_Coalesce<TCn>(svals0[i], svals1[j])) {
					                    Console.WriteLine("TCn_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TCn_Coalesce<TCn>(TCn val0, TCn val1) where TCn : C, new() {
					        Expression<Func<TCn>> e =
					            Expression.Lambda<Func<TCn>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(TCn)),
					                    Expression.Constant(val1, typeof(TCn))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<TCn> f = e.Compile();
					
					        TCn fResult = default(TCn);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        TCn csResult = default(TCn);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
			
			//-------- Scenario 2214
			namespace Scenario2214{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Delegate_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Delegate_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_Delegate_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_Delegate_Coalesce() {
				        Delegate[] svals0 = new Delegate[] { null, (Func<object>) delegate() { return null; }, (Func<int, int>) delegate(int i) { return i+1; }, (Action<object>) delegate { } };
				        Delegate[] svals1 = new Delegate[] { null, (Func<object>) delegate() { return null; }, (Func<int, int>) delegate(int i) { return i+1; }, (Action<object>) delegate { } };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_Delegate_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("Delegate_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_Delegate_Coalesce(Delegate val0, Delegate val1) {
				        Expression<Func<Delegate>> e =
				            Expression.Lambda<Func<Delegate>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(Delegate)),
				                    Expression.Constant(val1, typeof(Delegate))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<Delegate> f = e.Compile();
				
				        Delegate fResult = default(Delegate);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        Delegate csResult = default(Delegate);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 2215
			namespace Scenario2215{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Func_object_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Func_object_Coalesce__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_Func_object_Coalesce();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_Func_object_Coalesce() {
				        Func<object>[] svals0 = new Func<object>[] { null, (Func<object>) delegate() { return null; } };
				        Func<object>[] svals1 = new Func<object>[] { null, (Func<object>) delegate() { return null; } };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                if (!check_Func_object_Coalesce(svals0[i], svals1[j])) {
				                    Console.WriteLine("Func_object_Coalesce failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_Func_object_Coalesce(Func<object> val0, Func<object> val1) {
				        Expression<Func<Func<object>>> e =
				            Expression.Lambda<Func<Func<object>>>(
				                Expression.Coalesce(
				                    Expression.Constant(val0, typeof(Func<object>)),
				                    Expression.Constant(val1, typeof(Func<object>))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<object>> f = e.Compile();
				
				        Func<object> fResult = default(Func<object>);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        Func<object> csResult = default(Func<object>);
				        Exception csEx = null;
				        try {
				            csResult = val0 ?? val1;
				        }
				        catch (Exception ex) {
				            csEx = ex;
				        }
				
				        if (fEx != null || csEx != null) {
				            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
				        }
				        else {
				            return object.Equals(fResult, csResult);
				        }
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
				
				//-------- Scenario 2216
				namespace Scenario2216{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IEquatable_C_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression IEquatable_C_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_IEquatable_C_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_IEquatable_C_Coalesce() {
					        IEquatable<C>[] svals0 = new IEquatable<C>[] { null, new C(), new D(), new D(0), new D(5) };
					        IEquatable<C>[] svals1 = new IEquatable<C>[] { null, new C(), new D(), new D(0), new D(5) };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_IEquatable_C_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("IEquatable_C_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_IEquatable_C_Coalesce(IEquatable<C> val0, IEquatable<C> val1) {
					        Expression<Func<IEquatable<C>>> e =
					            Expression.Lambda<Func<IEquatable<C>>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(IEquatable<C>)),
					                    Expression.Constant(val1, typeof(IEquatable<C>))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<C>> f = e.Compile();
					
					        IEquatable<C> fResult = default(IEquatable<C>);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        IEquatable<C> csResult = default(IEquatable<C>);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2217
				namespace Scenario2217{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IEquatable_D_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression IEquatable_D_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_IEquatable_D_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_IEquatable_D_Coalesce() {
					        IEquatable<D>[] svals0 = new IEquatable<D>[] { null, new D(), new D(0), new D(5) };
					        IEquatable<D>[] svals1 = new IEquatable<D>[] { null, new D(), new D(0), new D(5) };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_IEquatable_D_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("IEquatable_D_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_IEquatable_D_Coalesce(IEquatable<D> val0, IEquatable<D> val1) {
					        Expression<Func<IEquatable<D>>> e =
					            Expression.Lambda<Func<IEquatable<D>>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(IEquatable<D>)),
					                    Expression.Constant(val1, typeof(IEquatable<D>))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<D>> f = e.Compile();
					
					        IEquatable<D> fResult = default(IEquatable<D>);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        IEquatable<D> csResult = default(IEquatable<D>);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 2218
				namespace Scenario2218{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "I_Coalesce__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression I_Coalesce__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_I_Coalesce();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_I_Coalesce() {
					        I[] svals0 = new I[] { null, new C(), new D(), new D(0), new D(5) };
					        I[] svals1 = new I[] { null, new C(), new D(), new D(0), new D(5) };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_I_Coalesce(svals0[i], svals1[j])) {
					                    Console.WriteLine("I_Coalesce failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_I_Coalesce(I val0, I val1) {
					        Expression<Func<I>> e =
					            Expression.Lambda<Func<I>>(
					                Expression.Coalesce(
					                    Expression.Constant(val0, typeof(I)),
					                    Expression.Constant(val1, typeof(I))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<I> f = e.Compile();
					
					        I fResult = default(I);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        I csResult = default(I);
					        Exception csEx = null;
					        try {
					            csResult = val0 ?? val1;
					        }
					        catch (Exception ex) {
					            csEx = ex;
					        }
					
					        if (fEx != null || csEx != null) {
					            return fEx != null && csEx != null && fEx.GetType() == csEx.GetType();
					        }
					        else {
					            return object.Equals(fResult, csResult);
					        }
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
