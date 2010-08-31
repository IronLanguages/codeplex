#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 2157
			namespace Scenario2157{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And__3", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression And__3() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_bool_And();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_bool_And() {
				        bool[] svals = new bool[] { true, false };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_bool_And(svals[i], svals[j])) {
				                    Console.WriteLine("bool_And failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_bool_And(bool val0, bool val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.And(Expression.Constant(val0, typeof(bool)),
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
				            csResult = (bool) (val0 & val1);
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
			
			//-------- Scenario 2158
			namespace Scenario2158{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or__3", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Or__3() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_bool_Or();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_bool_Or() {
				        bool[] svals = new bool[] { true, false };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_bool_Or(svals[i], svals[j])) {
				                    Console.WriteLine("bool_Or failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_bool_Or(bool val0, bool val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Or(Expression.Constant(val0, typeof(bool)),
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
				            csResult = (bool) (val0 | val1);
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
			
			//-------- Scenario 2159
			namespace Scenario2159{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression AndAlso__2() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_bool_AndAlso();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_bool_AndAlso() {
				        bool[] svals = new bool[] { true, false };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_bool_AndAlso(svals[i], svals[j])) {
				                    Console.WriteLine("bool_AndAlso failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_bool_AndAlso(bool val0, bool val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.AndAlso(Expression.Constant(val0, typeof(bool)),
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
				            csResult = (bool) (val0 && val1);
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
			
			//-------- Scenario 2160
			namespace Scenario2160{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression OrElse__2() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_bool_OrElse();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_bool_OrElse() {
				        bool[] svals = new bool[] { true, false };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_bool_OrElse(svals[i], svals[j])) {
				                    Console.WriteLine("bool_OrElse failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_bool_OrElse(bool val0, bool val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.OrElse(Expression.Constant(val0, typeof(bool)),
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
				            csResult = (bool) (val0 || val1);
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
	
}
