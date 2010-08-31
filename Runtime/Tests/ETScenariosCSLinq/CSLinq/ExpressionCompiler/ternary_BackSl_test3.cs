#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 148
			namespace Scenario148{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byteqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        byte?[][] svals1 = new byte?[][] { null, new byte?[0], new byte?[] { 0, 1, byte.MaxValue }, new byte?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_byteqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("byteqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteqArray_Conditional(bool condition, byte?[] val0, byte?[] val1) {
				        Expression<Func<byte?[]>> e =
				            Expression.Lambda<Func<byte?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(byte?[])),
				                    Expression.Constant(val1, typeof(byte?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte?[]> f = e.Compile();
				
				        byte?[] fResult = default(byte?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        byte?[] csResult = default(byte?[]);
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
			
			//-------- Scenario 149
			namespace Scenario149{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushortqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        ushort?[][] svals1 = new ushort?[][] { null, new ushort?[0], new ushort?[] { 0, 1, ushort.MaxValue }, new ushort?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_ushortqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("ushortqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortqArray_Conditional(bool condition, ushort?[] val0, ushort?[] val1) {
				        Expression<Func<ushort?[]>> e =
				            Expression.Lambda<Func<ushort?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(ushort?[])),
				                    Expression.Constant(val1, typeof(ushort?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort?[]> f = e.Compile();
				
				        ushort?[] fResult = default(ushort?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ushort?[] csResult = default(ushort?[]);
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
			
			//-------- Scenario 150
			namespace Scenario150{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uintqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        uint?[][] svals1 = new uint?[][] { null, new uint?[0], new uint?[] { 0, 1, uint.MaxValue }, new uint?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_uintqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("uintqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintqArray_Conditional(bool condition, uint?[] val0, uint?[] val1) {
				        Expression<Func<uint?[]>> e =
				            Expression.Lambda<Func<uint?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(uint?[])),
				                    Expression.Constant(val1, typeof(uint?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint?[]> f = e.Compile();
				
				        uint?[] fResult = default(uint?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        uint?[] csResult = default(uint?[]);
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
			
			//-------- Scenario 151
			namespace Scenario151{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulongqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        ulong?[][] svals1 = new ulong?[][] { null, new ulong?[0], new ulong?[] { 0, 1, ulong.MaxValue }, new ulong?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_ulongqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("ulongqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongqArray_Conditional(bool condition, ulong?[] val0, ulong?[] val1) {
				        Expression<Func<ulong?[]>> e =
				            Expression.Lambda<Func<ulong?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(ulong?[])),
				                    Expression.Constant(val1, typeof(ulong?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong?[]> f = e.Compile();
				
				        ulong?[] fResult = default(ulong?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        ulong?[] csResult = default(ulong?[]);
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
			
			//-------- Scenario 152
			namespace Scenario152{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyteqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        sbyte?[][] svals1 = new sbyte?[][] { null, new sbyte?[0], new sbyte?[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }, new sbyte?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_sbyteqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("sbyteqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteqArray_Conditional(bool condition, sbyte?[] val0, sbyte?[] val1) {
				        Expression<Func<sbyte?[]>> e =
				            Expression.Lambda<Func<sbyte?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(sbyte?[])),
				                    Expression.Constant(val1, typeof(sbyte?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte?[]> f = e.Compile();
				
				        sbyte?[] fResult = default(sbyte?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        sbyte?[] csResult = default(sbyte?[]);
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
			
			//-------- Scenario 153
			namespace Scenario153{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_shortqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        short?[][] svals1 = new short?[][] { null, new short?[0], new short?[] { 0, 1, -1, short.MinValue, short.MaxValue }, new short?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_shortqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("shortqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortqArray_Conditional(bool condition, short?[] val0, short?[] val1) {
				        Expression<Func<short?[]>> e =
				            Expression.Lambda<Func<short?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(short?[])),
				                    Expression.Constant(val1, typeof(short?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short?[]> f = e.Compile();
				
				        short?[] fResult = default(short?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        short?[] csResult = default(short?[]);
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
			
			//-------- Scenario 154
			namespace Scenario154{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_intqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        int?[][] svals1 = new int?[][] { null, new int?[0], new int?[] { 0, 1, -1, int.MinValue, int.MaxValue }, new int?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_intqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("intqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_intqArray_Conditional(bool condition, int?[] val0, int?[] val1) {
				        Expression<Func<int?[]>> e =
				            Expression.Lambda<Func<int?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(int?[])),
				                    Expression.Constant(val1, typeof(int?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int?[]> f = e.Compile();
				
				        int?[] fResult = default(int?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        int?[] csResult = default(int?[]);
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
			
			//-------- Scenario 155
			namespace Scenario155{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_longqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        long?[][] svals1 = new long?[][] { null, new long?[0], new long?[] { 0, 1, -1, long.MinValue, long.MaxValue }, new long?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_longqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("longqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_longqArray_Conditional(bool condition, long?[] val0, long?[] val1) {
				        Expression<Func<long?[]>> e =
				            Expression.Lambda<Func<long?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(long?[])),
				                    Expression.Constant(val1, typeof(long?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long?[]> f = e.Compile();
				
				        long?[] fResult = default(long?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        long?[] csResult = default(long?[]);
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
			
			//-------- Scenario 156
			namespace Scenario156{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "floatqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression floatqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_floatqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_floatqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        float?[][] svals1 = new float?[][] { null, new float?[0], new float?[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity }, new float?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_floatqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("floatqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_floatqArray_Conditional(bool condition, float?[] val0, float?[] val1) {
				        Expression<Func<float?[]>> e =
				            Expression.Lambda<Func<float?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(float?[])),
				                    Expression.Constant(val1, typeof(float?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float?[]> f = e.Compile();
				
				        float?[] fResult = default(float?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        float?[] csResult = default(float?[]);
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
			
			//-------- Scenario 157
			namespace Scenario157{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "doubleqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression doubleqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_doubleqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_doubleqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        double?[][] svals1 = new double?[][] { null, new double?[0], new double?[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity }, new double?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_doubleqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("doubleqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_doubleqArray_Conditional(bool condition, double?[] val0, double?[] val1) {
				        Expression<Func<double?[]>> e =
				            Expression.Lambda<Func<double?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(double?[])),
				                    Expression.Constant(val1, typeof(double?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double?[]> f = e.Compile();
				
				        double?[] fResult = default(double?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        double?[] csResult = default(double?[]);
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
			
			//-------- Scenario 158
			namespace Scenario158{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "decimalqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression decimalqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_decimalqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_decimalqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        decimal?[][] svals1 = new decimal?[][] { null, new decimal?[0], new decimal?[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue }, new decimal?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_decimalqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("decimalqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimalqArray_Conditional(bool condition, decimal?[] val0, decimal?[] val1) {
				        Expression<Func<decimal?[]>> e =
				            Expression.Lambda<Func<decimal?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(decimal?[])),
				                    Expression.Constant(val1, typeof(decimal?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal?[]> f = e.Compile();
				
				        decimal?[] fResult = default(decimal?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        decimal?[] csResult = default(decimal?[]);
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
			
			//-------- Scenario 159
			namespace Scenario159{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "charqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression charqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_charqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_charqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        char?[][] svals1 = new char?[][] { null, new char?[0], new char?[] { '\0', '\b', 'A', '\uffff' }, new char?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_charqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("charqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_charqArray_Conditional(bool condition, char?[] val0, char?[] val1) {
				        Expression<Func<char?[]>> e =
				            Expression.Lambda<Func<char?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(char?[])),
				                    Expression.Constant(val1, typeof(char?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char?[]> f = e.Compile();
				
				        char?[] fResult = default(char?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        char?[] csResult = default(char?[]);
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
			
			//-------- Scenario 160
			namespace Scenario160{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "boolqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression boolqArray_Conditional__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_boolqArray_Conditional();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_boolqArray_Conditional() {
				        bool[] svals0 = new bool[] { false, true };
				        bool?[][] svals1 = new bool?[][] { null, new bool?[0], new bool?[] { true, false }, new bool?[100] };
				        for (int i = 0; i < svals0.Length; i++) {
				            for (int j = 0; j < svals1.Length; j++) {
				                for (int k = 0; k < svals1.Length; k++) {
				                    if (!check_boolqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
				                        Console.WriteLine("boolqArray_Conditional failed");
				                        return false;
				                    }
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_boolqArray_Conditional(bool condition, bool?[] val0, bool?[] val1) {
				        Expression<Func<bool?[]>> e =
				            Expression.Lambda<Func<bool?[]>>(
				                Expression.Condition(
				                    Expression.Constant(condition, typeof(bool)),
				                    Expression.Constant(val0, typeof(bool?[])),
				                    Expression.Constant(val1, typeof(bool?[]))),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool?[]> f = e.Compile();
				
				        bool?[] fResult = default(bool?[]);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        bool?[] csResult = default(bool?[]);
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
				
				//-------- Scenario 161
				namespace Scenario161{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression SqArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_SqArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_SqArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        S?[][] svals1 = new S?[][] { null, new S?[0], new S?[] { default(S), new S() }, new S?[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_SqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("SqArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_SqArray_Conditional(bool condition, S?[] val0, S?[] val1) {
					        Expression<Func<S?[]>> e =
					            Expression.Lambda<Func<S?[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(S?[])),
					                    Expression.Constant(val1, typeof(S?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S?[]> f = e.Compile();
					
					        S?[] fResult = default(S?[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        S?[] csResult = default(S?[]);
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
				
				//-------- Scenario 162
				namespace Scenario162{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SpqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression SpqArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_SpqArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_SpqArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        Sp?[][] svals1 = new Sp?[][] { null, new Sp?[0], new Sp?[] { default(Sp), new Sp(), new Sp(5,5.0) }, new Sp?[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_SpqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("SpqArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_SpqArray_Conditional(bool condition, Sp?[] val0, Sp?[] val1) {
					        Expression<Func<Sp?[]>> e =
					            Expression.Lambda<Func<Sp?[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Sp?[])),
					                    Expression.Constant(val1, typeof(Sp?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp?[]> f = e.Compile();
					
					        Sp?[] fResult = default(Sp?[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sp?[] csResult = default(Sp?[]);
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
				
				//-------- Scenario 163
				namespace Scenario163{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "SsqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression SsqArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_SsqArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_SsqArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        Ss?[][] svals1 = new Ss?[][] { null, new Ss?[0], new Ss?[] { default(Ss), new Ss(), new Ss(new S()) }, new Ss?[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_SsqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("SsqArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_SsqArray_Conditional(bool condition, Ss?[] val0, Ss?[] val1) {
					        Expression<Func<Ss?[]>> e =
					            Expression.Lambda<Func<Ss?[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Ss?[])),
					                    Expression.Constant(val1, typeof(Ss?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss?[]> f = e.Compile();
					
					        Ss?[] fResult = default(Ss?[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ss?[] csResult = default(Ss?[]);
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
				
				//-------- Scenario 164
				namespace Scenario164{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ScqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ScqArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ScqArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ScqArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        Sc?[][] svals1 = new Sc?[][] { null, new Sc?[0], new Sc?[] { default(Sc), new Sc(), new Sc(null) }, new Sc?[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_ScqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("ScqArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ScqArray_Conditional(bool condition, Sc?[] val0, Sc?[] val1) {
					        Expression<Func<Sc?[]>> e =
					            Expression.Lambda<Func<Sc?[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Sc?[])),
					                    Expression.Constant(val1, typeof(Sc?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc?[]> f = e.Compile();
					
					        Sc?[] fResult = default(Sc?[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Sc?[] csResult = default(Sc?[]);
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
				
				//-------- Scenario 165
				namespace Scenario165{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ScsqArray_Conditional__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression ScsqArray_Conditional__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_ScsqArray_Conditional();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_ScsqArray_Conditional() {
					        bool[] svals0 = new bool[] { false, true };
					        Scs?[][] svals1 = new Scs?[][] { null, new Scs?[0], new Scs?[] { default(Scs), new Scs(), new Scs(null,new S()) }, new Scs?[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_ScsqArray_Conditional(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("ScsqArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_ScsqArray_Conditional(bool condition, Scs?[] val0, Scs?[] val1) {
					        Expression<Func<Scs?[]>> e =
					            Expression.Lambda<Func<Scs?[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Scs?[])),
					                    Expression.Constant(val1, typeof(Scs?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs?[]> f = e.Compile();
					
					        Scs?[] fResult = default(Scs?[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Scs?[] csResult = default(Scs?[]);
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
				
				//-------- Scenario 166
				namespace Scenario166{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TsqArray_Conditional_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TsqArray_Conditional_S___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TsqArray_Conditional<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TsqArray_Conditional<Ts>() where Ts : struct {
					        bool[] svals0 = new bool[] { false, true };
					        Ts?[][] svals1 = new Ts?[][] { null, new Ts?[0], new Ts?[] { default(Ts), new Ts() }, new Ts?[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TsqArray_Conditional<Ts>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TsqArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TsqArray_Conditional<Ts>(bool condition, Ts?[] val0, Ts?[] val1) where Ts : struct {
					        Expression<Func<Ts?[]>> e =
					            Expression.Lambda<Func<Ts?[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Ts?[])),
					                    Expression.Constant(val1, typeof(Ts?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?[]> f = e.Compile();
					
					        Ts?[] fResult = default(Ts?[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ts?[] csResult = default(Ts?[]);
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
				
				//-------- Scenario 167
				namespace Scenario167{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TsqArray_Conditional_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TsqArray_Conditional_Scs___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TsqArray_Conditional<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TsqArray_Conditional<Ts>() where Ts : struct {
					        bool[] svals0 = new bool[] { false, true };
					        Ts?[][] svals1 = new Ts?[][] { null, new Ts?[0], new Ts?[] { default(Ts), new Ts() }, new Ts?[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TsqArray_Conditional<Ts>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TsqArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TsqArray_Conditional<Ts>(bool condition, Ts?[] val0, Ts?[] val1) where Ts : struct {
					        Expression<Func<Ts?[]>> e =
					            Expression.Lambda<Func<Ts?[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Ts?[])),
					                    Expression.Constant(val1, typeof(Ts?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?[]> f = e.Compile();
					
					        Ts?[] fResult = default(Ts?[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ts?[] csResult = default(Ts?[]);
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
				
				//-------- Scenario 168
				namespace Scenario168{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TsqArray_Conditional_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TsqArray_Conditional_E___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TsqArray_Conditional<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TsqArray_Conditional<Ts>() where Ts : struct {
					        bool[] svals0 = new bool[] { false, true };
					        Ts?[][] svals1 = new Ts?[][] { null, new Ts?[0], new Ts?[] { default(Ts), new Ts() }, new Ts?[100] };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                for (int k = 0; k < svals1.Length; k++) {
					                    if (!check_TsqArray_Conditional<Ts>(svals0[i], svals1[j], svals1[k])) {
					                        Console.WriteLine("TsqArray_Conditional failed");
					                        return false;
					                    }
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_TsqArray_Conditional<Ts>(bool condition, Ts?[] val0, Ts?[] val1) where Ts : struct {
					        Expression<Func<Ts?[]>> e =
					            Expression.Lambda<Func<Ts?[]>>(
					                Expression.Condition(
					                    Expression.Constant(condition, typeof(bool)),
					                    Expression.Constant(val0, typeof(Ts?[])),
					                    Expression.Constant(val1, typeof(Ts?[]))),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?[]> f = e.Compile();
					
					        Ts?[] fResult = default(Ts?[]);
					        Exception fEx = null;
					        try {
					            fResult = f();
					        }
					        catch (Exception ex) {
					            fEx = ex;
					        }
					
					        Ts?[] csResult = default(Ts?[]);
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
