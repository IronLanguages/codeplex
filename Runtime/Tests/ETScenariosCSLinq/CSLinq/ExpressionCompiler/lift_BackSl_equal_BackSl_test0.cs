#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 282
			namespace Scenario282{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression And__2() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_boolq_And();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_boolq_And()
				    {
				        bool?[] svals = new bool?[] { null, true, false };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_boolq_And(svals[i], svals[j]))
				                {
				                    Console.WriteLine("boolq_And failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_boolq_And(bool? val0, bool? val1) {
				        ParameterExpression p0 = Expression.Parameter(typeof(bool), "p0");
				        ParameterExpression p1 = Expression.Parameter(typeof(bool), "p1");
				        Expression<Func<bool?>> e = Expression.Lambda<Func<bool?>>(
				                            Expression.And(
				                                Expression.Constant(val0, typeof(bool?)), 
				                                Expression.Constant(val1, typeof(bool?)),
				                                typeof(Test).GetMethod("And")
				                                ), 
				                        new ParameterExpression[]{});
				
				        Func<bool?> f = e.Compile();
				
				        bool? fResult = default(bool);
				        Exception fEx = null;
				        try {
				            fResult = f();
				        }
				        catch (Exception ex) {
				            fEx = ex;
				        }
				
				        bool? csResult = (val0) & (val1);
				        bool result = object.Equals(fResult, csResult);
				        return result;
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 283
			namespace Scenario283{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Or__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Or__2() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_boolq_Or();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_boolq_Or()
				    {
				        bool?[] svals = new bool?[] { null, true, false };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_boolq_Or(svals[i], svals[j]))
				                {
				                    Console.WriteLine("boolq_Or failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_boolq_Or(bool? val0, bool? val1)
				    {
				        ParameterExpression p0 = Expression.Parameter(typeof(bool), "p0");
				        ParameterExpression p1 = Expression.Parameter(typeof(bool), "p1");
				        Expression<Func<bool?>> e = Expression.Lambda<Func<bool?>>(
				                            Expression.Or(
				                                Expression.Constant(val0, typeof(bool?)),
				                                Expression.Constant(val1, typeof(bool?)),
				                                typeof(Test).GetMethod("Or")
				                                ),
				                        new ParameterExpression[] { });
				
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
				        }
				
				        bool? csResult = (val0) | (val1);
				        bool result = object.Equals(fResult, csResult);
				        return result;
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 284
			namespace Scenario284{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "AndAlso__1", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression AndAlso__1() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_boolq_AndAlso();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_boolq_AndAlso()
				    {
				        bool?[] svals = new bool?[] { null, true, false };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_boolq_AndAlso(svals[i], svals[j]))
				                {
				                    Console.WriteLine("boolq_AndAlso failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_boolq_AndAlso(bool? val0, bool? val1)
				    {
				        ParameterExpression p0 = Expression.Parameter(typeof(bool), "p0");
				        ParameterExpression p1 = Expression.Parameter(typeof(bool), "p1");
				        Expression<Func<bool?>> e = Expression.Lambda<Func<bool?>>(
				                            Expression.AndAlso(
				                                Expression.Constant(val0, typeof(bool?)),
				                                Expression.Constant(val1, typeof(bool?)),
				                                typeof(Test).GetMethod("AndAlso")
				                                ),
				                        new ParameterExpression[] { });
				
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
				        }
				
				        bool? csResult = (val0) == false ? false : (val0) & (val1);
				        bool result = object.Equals(fResult, csResult);
				        return result;
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 285
			namespace Scenario285{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "OrElse__1", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression OrElse__1() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_boolq_OrElse();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_boolq_OrElse()
				    {
				        bool?[] svals = new bool?[] { null, true, false };
				        for (int i = 0; i < svals.Length; i++)
				        {
				            for (int j = 0; j < svals.Length; j++)
				            {
				                if (!check_boolq_OrElse(svals[i], svals[j]))
				                {
				                    Console.WriteLine("boolq_OrElse failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_boolq_OrElse(bool? val0, bool? val1)
				    {
				        ParameterExpression p0 = Expression.Parameter(typeof(bool), "p0");
				        ParameterExpression p1 = Expression.Parameter(typeof(bool), "p1");
				        Expression<Func<bool?>> e = Expression.Lambda<Func<bool?>>(
				                            Expression.OrElse(
				                                Expression.Constant(val0, typeof(bool?)),
				                                Expression.Constant(val1, typeof(bool?)),
				                                typeof(Test).GetMethod("OrElse")
				                                ),
				                        new ParameterExpression[] { });
				
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
				        }
				
				        bool? csResult = (val0) == true ? true : (val0) | (val1);
				        bool result = object.Equals(fResult, csResult);
				        return result;
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
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
