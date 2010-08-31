#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 292
			namespace Scenario292{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal__3", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Equal__3() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_boolq_bool_Equal();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    public static bool Equal(bool val0, bool val1)
				    {
				        return val0 == val1;
				    }
				
				    public static bool NotEqual(bool val0, bool val1)
				    {
				        return val0 != val1;
				    }
				
				    static bool check_boolq_bool_Equal() {
				        bool?[] svals = new bool?[] { null, true };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_boolq_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("boolq_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_boolq_bool_Equal(bool? val0, bool? val1) {
				        Expression<Func<bool?>> e =
				            Expression.Lambda<Func<bool?>>(
				                    Expression.Equal(
				                        Expression.Constant(val0, typeof(bool?)),
				                        Expression.Constant(val1, typeof(bool?)),
				                        true,
				                        typeof(Test).GetMethod("Equal")
				                    ));
				
				        Func<bool?> f = e.Compile();
				
				        bool? fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool? csResult = default(bool);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else csResult = val0 == val1;
				
				        return object.Equals(fResult, csResult);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 293
			namespace Scenario293{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual__3", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression NotEqual__3() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_boolq_bool_NotEqual();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    public static bool Equal(bool val0, bool val1)
				    {
				        return val0 == val1;
				    }
				
				    public static bool NotEqual(bool val0, bool val1)
				    {
				        return val0 != val1;
				    }
				
				    static bool check_boolq_bool_NotEqual() {
				        bool?[] svals = new bool?[] { null, true };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_boolq_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("boolq_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_boolq_bool_NotEqual(bool? val0, bool? val1) {
				        Expression<Func<bool?>> e =
				            Expression.Lambda<Func<bool?>>(
				                    Expression.Equal(
				                        Expression.Constant(val0, typeof(bool?)),
				                        Expression.Constant(val1, typeof(bool?)),
				                        true,
				                        typeof(Test).GetMethod("NotEqual")
				                    ));
				
				        Func<bool?> f = e.Compile();
				
				        bool? fResult = default(bool);
				        Exception fEx = null;
				        try
				        {
				            fResult = f();
				        }
				        catch (Exception ex)
				        {
				            fEx = ex;
				            return false;
				        }
				
				        bool? csResult = default(bool);
				        if (val0 == null) csResult = null;
				        else if (val1 == null) csResult = null;
				        else csResult = val0 =! val1;
				
				        return object.Equals(fResult, csResult);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
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
