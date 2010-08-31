#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 108
			namespace Scenario108{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byteArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        byte[][] svals1 = new byte[][] { null, new byte[0], new byte[] { 0, 1, byte.MaxValue }, new byte[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_byteArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("byteArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteArray_Conditional(bool condition, byte[] val0, byte[] val1) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(byte[])),
				                    Expression.Constant(val1, typeof(byte[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte[]> f = e.Compile();
				
				        byte[] fResult = default(byte[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        byte[] csResult = default(byte[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;
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
			
			//-------- Scenario 109
			namespace Scenario109{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushortArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        ushort[][] svals1 = new ushort[][] { null, new ushort[0], new ushort[] { 0, 1, ushort.MaxValue }, new ushort[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_ushortArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("ushortArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortArray_Conditional(bool condition, ushort[] val0, ushort[] val1) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(ushort[])),
				                    Expression.Constant(val1, typeof(ushort[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort[]> f = e.Compile();
				
				        ushort[] fResult = default(ushort[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ushort[] csResult = default(ushort[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
			
			//-------- Scenario 110
			namespace Scenario110{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uintArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        uint[][] svals1 = new uint[][] { null, new uint[0], new uint[] { 0, 1, uint.MaxValue }, new uint[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_uintArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("uintArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintArray_Conditional(bool condition, uint[] val0, uint[] val1) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(uint[])),
				                    Expression.Constant(val1, typeof(uint[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint[]> f = e.Compile();
				
				        uint[] fResult = default(uint[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        uint[] csResult = default(uint[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
			
			//-------- Scenario 111
			namespace Scenario111{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulongArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        ulong[][] svals1 = new ulong[][] { null, new ulong[0], new ulong[] { 0, 1, ulong.MaxValue }, new ulong[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_ulongArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("ulongArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongArray_Conditional(bool condition, ulong[] val0, ulong[] val1) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(ulong[])),
				                    Expression.Constant(val1, typeof(ulong[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong[]> f = e.Compile();
				
				        ulong[] fResult = default(ulong[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ulong[] csResult = default(ulong[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
			
			//-------- Scenario 112
			namespace Scenario112{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyteArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        sbyte[][] svals1 = new sbyte[][] { null, new sbyte[0], new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }, new sbyte[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_sbyteArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("sbyteArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteArray_Conditional(bool condition, sbyte[] val0, sbyte[] val1) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(sbyte[])),
				                    Expression.Constant(val1, typeof(sbyte[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte[]> f = e.Compile();
				
				        sbyte[] fResult = default(sbyte[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        sbyte[] csResult = default(sbyte[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
			
			//-------- Scenario 113
			namespace Scenario113{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_shortArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        short[][] svals1 = new short[][] { null, new short[0], new short[] { 0, 1, -1, short.MinValue, short.MaxValue }, new short[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_shortArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("shortArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortArray_Conditional(bool condition, short[] val0, short[] val1) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(short[])),
				                    Expression.Constant(val1, typeof(short[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short[]> f = e.Compile();
				
				        short[] fResult = default(short[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        short[] csResult = default(short[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
			
			//-------- Scenario 114
			namespace Scenario114{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_intArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        int[][] svals1 = new int[][] { null, new int[0], new int[] { 0, 1, -1, int.MinValue, int.MaxValue }, new int[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_intArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("intArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_intArray_Conditional(bool condition, int[] val0, int[] val1) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(int[])),
				                    Expression.Constant(val1, typeof(int[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int[]> f = e.Compile();
				
				        int[] fResult = default(int[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        int[] csResult = default(int[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
			
			//-------- Scenario 115
			namespace Scenario115{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_longArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        long[][] svals1 = new long[][] { null, new long[0], new long[] { 0, 1, -1, long.MinValue, long.MaxValue }, new long[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_longArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("longArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_longArray_Conditional(bool condition, long[] val0, long[] val1) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(long[])),
				                    Expression.Constant(val1, typeof(long[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long[]> f = e.Compile();
				
				        long[] fResult = default(long[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        long[] csResult = default(long[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
			
			//-------- Scenario 116
			namespace Scenario116{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "floatArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression floatArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_floatArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_floatArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        float[][] svals1 = new float[][] { null, new float[0], new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity }, new float[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_floatArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("floatArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_floatArray_Conditional(bool condition, float[] val0, float[] val1) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(float[])),
				                    Expression.Constant(val1, typeof(float[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float[]> f = e.Compile();
				
				        float[] fResult = default(float[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        float[] csResult = default(float[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
			
			//-------- Scenario 117
			namespace Scenario117{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "doubleArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression doubleArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_doubleArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_doubleArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        double[][] svals1 = new double[][] { null, new double[0], new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity }, new double[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_doubleArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("doubleArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_doubleArray_Conditional(bool condition, double[] val0, double[] val1) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(double[])),
				                    Expression.Constant(val1, typeof(double[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double[]> f = e.Compile();
				
				        double[] fResult = default(double[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        double[] csResult = default(double[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
			
			//-------- Scenario 118
			namespace Scenario118{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "decimalArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression decimalArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_decimalArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_decimalArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        decimal[][] svals1 = new decimal[][] { null, new decimal[0], new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue }, new decimal[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_decimalArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("decimalArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimalArray_Conditional(bool condition, decimal[] val0, decimal[] val1) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(decimal[])),
				                    Expression.Constant(val1, typeof(decimal[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal[]> f = e.Compile();
				
				        decimal[] fResult = default(decimal[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        decimal[] csResult = default(decimal[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
			
			//-------- Scenario 119
			namespace Scenario119{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "charArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression charArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_charArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_charArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        char[][] svals1 = new char[][] { null, new char[0], new char[] { '\0', '\b', 'A', '\uffff' }, new char[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_charArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("charArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_charArray_Conditional(bool condition, char[] val0, char[] val1) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(char[])),
				                    Expression.Constant(val1, typeof(char[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char[]> f = e.Compile();
				
				        char[] fResult = default(char[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        char[] csResult = default(char[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
			
			//-------- Scenario 120
			namespace Scenario120{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "boolArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression boolArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_boolArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_boolArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        bool[][] svals1 = new bool[][] { null, new bool[0], new bool[] { true, false }, new bool[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_boolArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("boolArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_boolArray_Conditional(bool condition, bool[] val0, bool[] val1) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(bool[])),
				                    Expression.Constant(val1, typeof(bool[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool[]> f = e.Compile();
				
				        bool[] fResult = default(bool[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        bool[] csResult = default(bool[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
				
				//-------- Scenario 121
				namespace Scenario121{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression SArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_SArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_SArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        S[][] svals1 = new S[][] { null, new S[0], new S[] { default(S), new S() }, new S[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_SArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("SArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_SArray_Conditional(bool condition, S[] val0, S[] val1) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(S[])),
					                    Expression.Constant(val1, typeof(S[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S[]> f = e.Compile();
					
					        S[] fResult = default(S[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        S[] csResult = default(S[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 122
				namespace Scenario122{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SpArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression SpArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_SpArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_SpArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        Sp[][] svals1 = new Sp[][] { null, new Sp[0], new Sp[] { default(Sp), new Sp(), new Sp(5,5.0) }, new Sp[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_SpArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("SpArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_SpArray_Conditional(bool condition, Sp[] val0, Sp[] val1) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Sp[])),
					                    Expression.Constant(val1, typeof(Sp[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp[]> f = e.Compile();
					
					        Sp[] fResult = default(Sp[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sp[] csResult = default(Sp[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 123
				namespace Scenario123{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SsArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression SsArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_SsArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_SsArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        Ss[][] svals1 = new Ss[][] { null, new Ss[0], new Ss[] { default(Ss), new Ss(), new Ss(new S()) }, new Ss[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_SsArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("SsArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_SsArray_Conditional(bool condition, Ss[] val0, Ss[] val1) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Ss[])),
					                    Expression.Constant(val1, typeof(Ss[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss[]> f = e.Compile();
					
					        Ss[] fResult = default(Ss[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ss[] csResult = default(Ss[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 124
				namespace Scenario124{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ScArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ScArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ScArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ScArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        Sc[][] svals1 = new Sc[][] { null, new Sc[0], new Sc[] { default(Sc), new Sc(), new Sc(null) }, new Sc[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_ScArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("ScArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ScArray_Conditional(bool condition, Sc[] val0, Sc[] val1) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Sc[])),
					                    Expression.Constant(val1, typeof(Sc[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc[]> f = e.Compile();
					
					        Sc[] fResult = default(Sc[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sc[] csResult = default(Sc[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 125
				namespace Scenario125{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ScsArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ScsArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ScsArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ScsArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        Scs[][] svals1 = new Scs[][] { null, new Scs[0], new Scs[] { default(Scs), new Scs(), new Scs(null,new S()) }, new Scs[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_ScsArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("ScsArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ScsArray_Conditional(bool condition, Scs[] val0, Scs[] val1) {
					        Expression<Func<Scs[]>> e =
					            Expression.Lambda<Func<Scs[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Scs[])),
					                    Expression.Constant(val1, typeof(Scs[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs[]> f = e.Compile();
					
					        Scs[] fResult = default(Scs[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Scs[] csResult = default(Scs[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 126
				namespace Scenario126{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TsArray_Conditional_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TsArray_Conditional_S___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TsArray_Conditional<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TsArray_Conditional<Ts>() where Ts : struct {
					        bool[] svals0 = new bool[] { false, true };
					        Ts[][] svals1 = new Ts[][] { null, new Ts[0], new Ts[] { default(Ts), new Ts() }, new Ts[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TsArray_Conditional<Ts>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TsArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TsArray_Conditional<Ts>(bool condition, Ts[] val0, Ts[] val1) where Ts : struct {
					        Expression<Func<Ts[]>> e =
					            Expression.Lambda<Func<Ts[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Ts[])),
					                    Expression.Constant(val1, typeof(Ts[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts[]> f = e.Compile();
					
					        Ts[] fResult = default(Ts[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ts[] csResult = default(Ts[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 127
				namespace Scenario127{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TsArray_Conditional_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TsArray_Conditional_Scs___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TsArray_Conditional<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TsArray_Conditional<Ts>() where Ts : struct {
					        bool[] svals0 = new bool[] { false, true };
					        Ts[][] svals1 = new Ts[][] { null, new Ts[0], new Ts[] { default(Ts), new Ts() }, new Ts[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TsArray_Conditional<Ts>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TsArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TsArray_Conditional<Ts>(bool condition, Ts[] val0, Ts[] val1) where Ts : struct {
					        Expression<Func<Ts[]>> e =
					            Expression.Lambda<Func<Ts[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Ts[])),
					                    Expression.Constant(val1, typeof(Ts[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts[]> f = e.Compile();
					
					        Ts[] fResult = default(Ts[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ts[] csResult = default(Ts[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 128
				namespace Scenario128{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TsArray_Conditional_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TsArray_Conditional_E___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TsArray_Conditional<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TsArray_Conditional<Ts>() where Ts : struct {
					        bool[] svals0 = new bool[] { false, true };
					        Ts[][] svals1 = new Ts[][] { null, new Ts[0], new Ts[] { default(Ts), new Ts() }, new Ts[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TsArray_Conditional<Ts>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TsArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TsArray_Conditional<Ts>(bool condition, Ts[] val0, Ts[] val1) where Ts : struct {
					        Expression<Func<Ts[]>> e =
					            Expression.Lambda<Func<Ts[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Ts[])),
					                    Expression.Constant(val1, typeof(Ts[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts[]> f = e.Compile();
					
					        Ts[] fResult = default(Ts[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ts[] csResult = default(Ts[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 129
				namespace Scenario129{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "EArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression EArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_EArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_EArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        E[][] svals1 = new E[][] { null, new E[0], new E[] { (E) 0, E.A, E.B, (E) int.MaxValue, (E) int.MinValue }, new E[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_EArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("EArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_EArray_Conditional(bool condition, E[] val0, E[] val1) {
					        Expression<Func<E[]>> e =
					            Expression.Lambda<Func<E[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(E[])),
					                    Expression.Constant(val1, typeof(E[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<E[]> f = e.Compile();
					
					        E[] fResult = default(E[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        E[] csResult = default(E[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 130
				namespace Scenario130{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ElArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ElArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ElArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ElArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        El[][] svals1 = new El[][] { null, new El[0], new El[] { (El) 0, El.A, El.B, (El) long.MaxValue, (El) long.MinValue }, new El[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_ElArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("ElArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ElArray_Conditional(bool condition, El[] val0, El[] val1) {
					        Expression<Func<El[]>> e =
					            Expression.Lambda<Func<El[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(El[])),
					                    Expression.Constant(val1, typeof(El[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<El[]> f = e.Compile();
					
					        El[] fResult = default(El[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        El[] csResult = default(El[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
			
			//-------- Scenario 131
			namespace Scenario131{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "stringArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression stringArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_stringArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_stringArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        string[][] svals1 = new string[][] { null, new string[0], new string[] { null, "", "a", "foo" }, new string[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_stringArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("stringArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_stringArray_Conditional(bool condition, string[] val0, string[] val1) {
				        Expression<Func<string[]>> e =
				            Expression.Lambda<Func<string[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(string[])),
				                    Expression.Constant(val1, typeof(string[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<string[]> f = e.Compile();
				
				        string[] fResult = default(string[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        string[] csResult = default(string[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
				
				//-------- Scenario 132
				namespace Scenario132{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "objectArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression objectArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_objectArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_objectArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        object[][] svals1 = new object[][] { null, new object[0], new object[] { null, new object() }, new object[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_objectArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("objectArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_objectArray_Conditional(bool condition, object[] val0, object[] val1) {
					        Expression<Func<object[]>> e =
					            Expression.Lambda<Func<object[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(object[])),
					                    Expression.Constant(val1, typeof(object[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<object[]> f = e.Compile();
					
					        object[] fResult = default(object[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        object[] csResult = default(object[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 133
				namespace Scenario133{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "CArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression CArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_CArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_CArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        C[][] svals1 = new C[][] { null, new C[0], new C[] { null, new C() }, new C[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_CArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("CArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_CArray_Conditional(bool condition, C[] val0, C[] val1) {
					        Expression<Func<C[]>> e =
					            Expression.Lambda<Func<C[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(C[])),
					                    Expression.Constant(val1, typeof(C[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<C[]> f = e.Compile();
					
					        C[] fResult = default(C[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        C[] csResult = default(C[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 134
				namespace Scenario134{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TArray_Conditional_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TArray_Conditional_object___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TArray_Conditional<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TArray_Conditional<T>() {
					        bool[] svals0 = new bool[] { false, true };
					        T[][] svals1 = new T[][] { null, new T[0], new T[] { default(T) }, new T[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TArray_Conditional<T>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TArray_Conditional<T>(bool condition, T[] val0, T[] val1) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(T[])),
					                    Expression.Constant(val1, typeof(T[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
					
					        T[] fResult = default(T[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        T[] csResult = default(T[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 135
				namespace Scenario135{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TArray_Conditional_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TArray_Conditional_C___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TArray_Conditional<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TArray_Conditional<T>() {
					        bool[] svals0 = new bool[] { false, true };
					        T[][] svals1 = new T[][] { null, new T[0], new T[] { default(T) }, new T[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TArray_Conditional<T>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TArray_Conditional<T>(bool condition, T[] val0, T[] val1) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(T[])),
					                    Expression.Constant(val1, typeof(T[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
					
					        T[] fResult = default(T[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        T[] csResult = default(T[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 136
				namespace Scenario136{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TArray_Conditional_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TArray_Conditional_S___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TArray_Conditional<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TArray_Conditional<T>() {
					        bool[] svals0 = new bool[] { false, true };
					        T[][] svals1 = new T[][] { null, new T[0], new T[] { default(T) }, new T[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TArray_Conditional<T>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TArray_Conditional<T>(bool condition, T[] val0, T[] val1) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(T[])),
					                    Expression.Constant(val1, typeof(T[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
					
					        T[] fResult = default(T[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        T[] csResult = default(T[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 137
				namespace Scenario137{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TArray_Conditional_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TArray_Conditional_Scs___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TArray_Conditional<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TArray_Conditional<T>() {
					        bool[] svals0 = new bool[] { false, true };
					        T[][] svals1 = new T[][] { null, new T[0], new T[] { default(T) }, new T[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TArray_Conditional<T>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TArray_Conditional<T>(bool condition, T[] val0, T[] val1) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(T[])),
					                    Expression.Constant(val1, typeof(T[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
					
					        T[] fResult = default(T[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        T[] csResult = default(T[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 138
				namespace Scenario138{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TArray_Conditional_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TArray_Conditional_E___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TArray_Conditional<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TArray_Conditional<T>() {
					        bool[] svals0 = new bool[] { false, true };
					        T[][] svals1 = new T[][] { null, new T[0], new T[] { default(T) }, new T[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TArray_Conditional<T>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TArray_Conditional<T>(bool condition, T[] val0, T[] val1) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(T[])),
					                    Expression.Constant(val1, typeof(T[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
					
					        T[] fResult = default(T[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        T[] csResult = default(T[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 139
				namespace Scenario139{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TcArray_Conditional_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TcArray_Conditional_object___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TcArray_Conditional<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TcArray_Conditional<Tc>() where Tc : class {
					        bool[] svals0 = new bool[] { false, true };
					        Tc[][] svals1 = new Tc[][] { null, new Tc[0], new Tc[] { null, default(Tc) }, new Tc[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TcArray_Conditional<Tc>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TcArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TcArray_Conditional<Tc>(bool condition, Tc[] val0, Tc[] val1) where Tc : class {
					        Expression<Func<Tc[]>> e =
					            Expression.Lambda<Func<Tc[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Tc[])),
					                    Expression.Constant(val1, typeof(Tc[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc[]> f = e.Compile();
					
					        Tc[] fResult = default(Tc[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Tc[] csResult = default(Tc[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 140
				namespace Scenario140{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TcArray_Conditional_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TcArray_Conditional_C___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TcArray_Conditional<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TcArray_Conditional<Tc>() where Tc : class {
					        bool[] svals0 = new bool[] { false, true };
					        Tc[][] svals1 = new Tc[][] { null, new Tc[0], new Tc[] { null, default(Tc) }, new Tc[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TcArray_Conditional<Tc>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TcArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TcArray_Conditional<Tc>(bool condition, Tc[] val0, Tc[] val1) where Tc : class {
					        Expression<Func<Tc[]>> e =
					            Expression.Lambda<Func<Tc[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Tc[])),
					                    Expression.Constant(val1, typeof(Tc[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc[]> f = e.Compile();
					
					        Tc[] fResult = default(Tc[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Tc[] csResult = default(Tc[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 141
				namespace Scenario141{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TcnArray_Conditional_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TcnArray_Conditional_object___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TcnArray_Conditional<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TcnArray_Conditional<Tcn>() where Tcn : class, new() {
					        bool[] svals0 = new bool[] { false, true };
					        Tcn[][] svals1 = new Tcn[][] { null, new Tcn[0], new Tcn[] { null, default(Tcn), new Tcn() }, new Tcn[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TcnArray_Conditional<Tcn>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TcnArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TcnArray_Conditional<Tcn>(bool condition, Tcn[] val0, Tcn[] val1) where Tcn : class, new() {
					        Expression<Func<Tcn[]>> e =
					            Expression.Lambda<Func<Tcn[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Tcn[])),
					                    Expression.Constant(val1, typeof(Tcn[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn[]> f = e.Compile();
					
					        Tcn[] fResult = default(Tcn[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Tcn[] csResult = default(Tcn[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 142
				namespace Scenario142{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TcnArray_Conditional_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TcnArray_Conditional_C___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TcnArray_Conditional<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TcnArray_Conditional<Tcn>() where Tcn : class, new() {
					        bool[] svals0 = new bool[] { false, true };
					        Tcn[][] svals1 = new Tcn[][] { null, new Tcn[0], new Tcn[] { null, default(Tcn), new Tcn() }, new Tcn[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TcnArray_Conditional<Tcn>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TcnArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TcnArray_Conditional<Tcn>(bool condition, Tcn[] val0, Tcn[] val1) where Tcn : class, new() {
					        Expression<Func<Tcn[]>> e =
					            Expression.Lambda<Func<Tcn[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Tcn[])),
					                    Expression.Constant(val1, typeof(Tcn[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn[]> f = e.Compile();
					
					        Tcn[] fResult = default(Tcn[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Tcn[] csResult = default(Tcn[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 143
				namespace Scenario143{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TCArray_Conditional_C_a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TCArray_Conditional_C_a__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TCArray_Conditional<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TCArray_Conditional<TC>() where TC : C {
					        bool[] svals0 = new bool[] { false, true };
					        TC[][] svals1 = new TC[][] { null, new TC[0], new TC[] { null, default(TC), (TC) new C() }, new TC[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TCArray_Conditional<TC>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TCArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TCArray_Conditional<TC>(bool condition, TC[] val0, TC[] val1) where TC : C {
					        Expression<Func<TC[]>> e =
					            Expression.Lambda<Func<TC[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(TC[])),
					                    Expression.Constant(val1, typeof(TC[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<TC[]> f = e.Compile();
					
					        TC[] fResult = default(TC[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        TC[] csResult = default(TC[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 144
				namespace Scenario144{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TCnArray_Conditional_C_a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TCnArray_Conditional_C_a__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TCnArray_Conditional<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TCnArray_Conditional<TCn>() where TCn : C, new() {
					        bool[] svals0 = new bool[] { false, true };
					        TCn[][] svals1 = new TCn[][] { null, new TCn[0], new TCn[] { null, default(TCn), new TCn(), (TCn) new C() }, new TCn[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TCnArray_Conditional<TCn>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TCnArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TCnArray_Conditional<TCn>(bool condition, TCn[] val0, TCn[] val1) where TCn : C, new() {
					        Expression<Func<TCn[]>> e =
					            Expression.Lambda<Func<TCn[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(TCn[])),
					                    Expression.Constant(val1, typeof(TCn[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<TCn[]> f = e.Compile();
					
					        TCn[] fResult = default(TCn[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        TCn[] csResult = default(TCn[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
			
			//-------- Scenario 145
			namespace Scenario145{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DelegateArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression DelegateArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_DelegateArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_DelegateArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        Delegate[][] svals1 = new Delegate[][] { null, new Delegate[0], new Delegate[] { null, (Func<object>) delegate() { return null; } }, new Delegate[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_DelegateArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("DelegateArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_DelegateArray_Conditional(bool condition, Delegate[] val0, Delegate[] val1) {
				        Expression<Func<Delegate[]>> e =
				            Expression.Lambda<Func<Delegate[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(Delegate[])),
				                    Expression.Constant(val1, typeof(Delegate[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<Delegate[]> f = e.Compile();
				
				        Delegate[] fResult = default(Delegate[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        Delegate[] csResult = default(Delegate[]);
				        Exception csEx = null;
				        try {
				            csResult = condition ? val0 : val1;;
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
				
				//-------- Scenario 146
				namespace Scenario146{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Func_objectArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Func_objectArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Func_objectArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Func_objectArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        Func<object>[][] svals1 = new Func<object>[][] { null, new Func<object>[0], new Func<object>[] { null, delegate() { return null; } }, new Func<object>[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_Func_objectArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("Func_objectArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_Func_objectArray_Conditional(bool condition, Func<object>[] val0, Func<object>[] val1) {
					        Expression<Func<Func<object>[]>> e =
					            Expression.Lambda<Func<Func<object>[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Func<object>[])),
					                    Expression.Constant(val1, typeof(Func<object>[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<object>[]> f = e.Compile();
					
					        Func<object>[] fResult = default(Func<object>[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Func<object>[] csResult = default(Func<object>[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
				
				//-------- Scenario 147
				namespace Scenario147{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IEquatable_CArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression IEquatable_CArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_IEquatable_CArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_IEquatable_CArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        IEquatable<C>[][] svals1 = new IEquatable<C>[][] { null, new IEquatable<C>[0], new IEquatable<C>[] { null, new C() }, new IEquatable<C>[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_IEquatable_CArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("IEquatable_CArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_IEquatable_CArray_Conditional(bool condition, IEquatable<C>[] val0, IEquatable<C>[] val1) {
					        Expression<Func<IEquatable<C>[]>> e =
					            Expression.Lambda<Func<IEquatable<C>[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(IEquatable<C>[])),
					                    Expression.Constant(val1, typeof(IEquatable<C>[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<C>[]> f = e.Compile();
					
					        IEquatable<C>[] fResult = default(IEquatable<C>[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        IEquatable<C>[] csResult = default(IEquatable<C>[]);
					        Exception csEx = null;
					        try {
					            csResult = condition ? val0 : val1;;
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
				
			
			
			
			public class C : IEquatable<C> {
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
