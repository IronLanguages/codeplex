#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 353
			namespace Scenario353{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byteq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_Lambda() {
				        foreach (byte? val in new byte?[] { 0, 1, byte.MaxValue }) {
				            if (!check_byteq_Lambda(val)) {
				                Console.WriteLine("byteq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_Lambda(byte? val) {
				        ParameterExpression p = Expression.Parameter(typeof(byte?), "p");
				        Expression<Func<byte?>> e1 = Expression.Lambda<Func<byte?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<byte?, byte?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(byte?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte?> f1 = e1.Compile();
				
				        Expression<Func<byte?, Func<byte?>>> e2 = Expression.Lambda<Func<byte?, Func<byte?>>>(
				            Expression.Lambda<Func<byte?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<byte?, Func<byte?>> f2 = e2.Compile();
				
				        Expression<Func<Func<byte?, byte?>>> e3 = Expression.Lambda<Func<Func<byte?, byte?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<byte?, byte?>>>(
				                    Expression.Lambda<Func<byte?, byte?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte?, byte?> f3 = e3.Compile()();
				
				        Expression<Func<Func<byte?, byte?>>> e4 = Expression.Lambda<Func<Func<byte?, byte?>>>(
				            Expression.Lambda<Func<byte?, byte?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<byte?, byte?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 354
			namespace Scenario354{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushortq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_Lambda() {
				        foreach (ushort? val in new ushort?[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushortq_Lambda(val)) {
				                Console.WriteLine("ushortq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_Lambda(ushort? val) {
				        ParameterExpression p = Expression.Parameter(typeof(ushort?), "p");
				        Expression<Func<ushort?>> e1 = Expression.Lambda<Func<ushort?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<ushort?, ushort?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(ushort?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort?> f1 = e1.Compile();
				
				        Expression<Func<ushort?, Func<ushort?>>> e2 = Expression.Lambda<Func<ushort?, Func<ushort?>>>(
				            Expression.Lambda<Func<ushort?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<ushort?, Func<ushort?>> f2 = e2.Compile();
				
				        Expression<Func<Func<ushort?, ushort?>>> e3 = Expression.Lambda<Func<Func<ushort?, ushort?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<ushort?, ushort?>>>(
				                    Expression.Lambda<Func<ushort?, ushort?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort?, ushort?> f3 = e3.Compile()();
				
				        Expression<Func<Func<ushort?, ushort?>>> e4 = Expression.Lambda<Func<Func<ushort?, ushort?>>>(
				            Expression.Lambda<Func<ushort?, ushort?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<ushort?, ushort?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 355
			namespace Scenario355{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uintq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_Lambda() {
				        foreach (uint? val in new uint?[] { 0, 1, uint.MaxValue }) {
				            if (!check_uintq_Lambda(val)) {
				                Console.WriteLine("uintq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_Lambda(uint? val) {
				        ParameterExpression p = Expression.Parameter(typeof(uint?), "p");
				        Expression<Func<uint?>> e1 = Expression.Lambda<Func<uint?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<uint?, uint?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(uint?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint?> f1 = e1.Compile();
				
				        Expression<Func<uint?, Func<uint?>>> e2 = Expression.Lambda<Func<uint?, Func<uint?>>>(
				            Expression.Lambda<Func<uint?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<uint?, Func<uint?>> f2 = e2.Compile();
				
				        Expression<Func<Func<uint?, uint?>>> e3 = Expression.Lambda<Func<Func<uint?, uint?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<uint?, uint?>>>(
				                    Expression.Lambda<Func<uint?, uint?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint?, uint?> f3 = e3.Compile()();
				
				        Expression<Func<Func<uint?, uint?>>> e4 = Expression.Lambda<Func<Func<uint?, uint?>>>(
				            Expression.Lambda<Func<uint?, uint?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<uint?, uint?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 356
			namespace Scenario356{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulongq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_Lambda() {
				        foreach (ulong? val in new ulong?[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulongq_Lambda(val)) {
				                Console.WriteLine("ulongq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_Lambda(ulong? val) {
				        ParameterExpression p = Expression.Parameter(typeof(ulong?), "p");
				        Expression<Func<ulong?>> e1 = Expression.Lambda<Func<ulong?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<ulong?, ulong?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(ulong?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong?> f1 = e1.Compile();
				
				        Expression<Func<ulong?, Func<ulong?>>> e2 = Expression.Lambda<Func<ulong?, Func<ulong?>>>(
				            Expression.Lambda<Func<ulong?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<ulong?, Func<ulong?>> f2 = e2.Compile();
				
				        Expression<Func<Func<ulong?, ulong?>>> e3 = Expression.Lambda<Func<Func<ulong?, ulong?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<ulong?, ulong?>>>(
				                    Expression.Lambda<Func<ulong?, ulong?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong?, ulong?> f3 = e3.Compile()();
				
				        Expression<Func<Func<ulong?, ulong?>>> e4 = Expression.Lambda<Func<Func<ulong?, ulong?>>>(
				            Expression.Lambda<Func<ulong?, ulong?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<ulong?, ulong?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 357
			namespace Scenario357{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyteq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_Lambda() {
				        foreach (sbyte? val in new sbyte?[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyteq_Lambda(val)) {
				                Console.WriteLine("sbyteq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_Lambda(sbyte? val) {
				        ParameterExpression p = Expression.Parameter(typeof(sbyte?), "p");
				        Expression<Func<sbyte?>> e1 = Expression.Lambda<Func<sbyte?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<sbyte?, sbyte?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(sbyte?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte?> f1 = e1.Compile();
				
				        Expression<Func<sbyte?, Func<sbyte?>>> e2 = Expression.Lambda<Func<sbyte?, Func<sbyte?>>>(
				            Expression.Lambda<Func<sbyte?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<sbyte?, Func<sbyte?>> f2 = e2.Compile();
				
				        Expression<Func<Func<sbyte?, sbyte?>>> e3 = Expression.Lambda<Func<Func<sbyte?, sbyte?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<sbyte?, sbyte?>>>(
				                    Expression.Lambda<Func<sbyte?, sbyte?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte?, sbyte?> f3 = e3.Compile()();
				
				        Expression<Func<Func<sbyte?, sbyte?>>> e4 = Expression.Lambda<Func<Func<sbyte?, sbyte?>>>(
				            Expression.Lambda<Func<sbyte?, sbyte?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<sbyte?, sbyte?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 358
			namespace Scenario358{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_shortq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_Lambda() {
				        foreach (short? val in new short?[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_shortq_Lambda(val)) {
				                Console.WriteLine("shortq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_Lambda(short? val) {
				        ParameterExpression p = Expression.Parameter(typeof(short?), "p");
				        Expression<Func<short?>> e1 = Expression.Lambda<Func<short?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<short?, short?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(short?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<short?> f1 = e1.Compile();
				
				        Expression<Func<short?, Func<short?>>> e2 = Expression.Lambda<Func<short?, Func<short?>>>(
				            Expression.Lambda<Func<short?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<short?, Func<short?>> f2 = e2.Compile();
				
				        Expression<Func<Func<short?, short?>>> e3 = Expression.Lambda<Func<Func<short?, short?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<short?, short?>>>(
				                    Expression.Lambda<Func<short?, short?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<short?, short?> f3 = e3.Compile()();
				
				        Expression<Func<Func<short?, short?>>> e4 = Expression.Lambda<Func<Func<short?, short?>>>(
				            Expression.Lambda<Func<short?, short?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<short?, short?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 359
			namespace Scenario359{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_intq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_Lambda() {
				        foreach (int? val in new int?[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_intq_Lambda(val)) {
				                Console.WriteLine("intq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_Lambda(int? val) {
				        ParameterExpression p = Expression.Parameter(typeof(int?), "p");
				        Expression<Func<int?>> e1 = Expression.Lambda<Func<int?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<int?, int?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(int?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<int?> f1 = e1.Compile();
				
				        Expression<Func<int?, Func<int?>>> e2 = Expression.Lambda<Func<int?, Func<int?>>>(
				            Expression.Lambda<Func<int?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<int?, Func<int?>> f2 = e2.Compile();
				
				        Expression<Func<Func<int?, int?>>> e3 = Expression.Lambda<Func<Func<int?, int?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<int?, int?>>>(
				                    Expression.Lambda<Func<int?, int?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<int?, int?> f3 = e3.Compile()();
				
				        Expression<Func<Func<int?, int?>>> e4 = Expression.Lambda<Func<Func<int?, int?>>>(
				            Expression.Lambda<Func<int?, int?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<int?, int?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 360
			namespace Scenario360{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_longq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_Lambda() {
				        foreach (long? val in new long?[] { 0, 1, -1, long.MinValue, long.MaxValue }) {
				            if (!check_longq_Lambda(val)) {
				                Console.WriteLine("longq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_Lambda(long? val) {
				        ParameterExpression p = Expression.Parameter(typeof(long?), "p");
				        Expression<Func<long?>> e1 = Expression.Lambda<Func<long?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<long?, long?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(long?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<long?> f1 = e1.Compile();
				
				        Expression<Func<long?, Func<long?>>> e2 = Expression.Lambda<Func<long?, Func<long?>>>(
				            Expression.Lambda<Func<long?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<long?, Func<long?>> f2 = e2.Compile();
				
				        Expression<Func<Func<long?, long?>>> e3 = Expression.Lambda<Func<Func<long?, long?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<long?, long?>>>(
				                    Expression.Lambda<Func<long?, long?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<long?, long?> f3 = e3.Compile()();
				
				        Expression<Func<Func<long?, long?>>> e4 = Expression.Lambda<Func<Func<long?, long?>>>(
				            Expression.Lambda<Func<long?, long?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<long?, long?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 361
			namespace Scenario361{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "floatq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression floatq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_floatq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_floatq_Lambda() {
				        foreach (float? val in new float?[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN }) {
				            if (!check_floatq_Lambda(val)) {
				                Console.WriteLine("floatq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_floatq_Lambda(float? val) {
				        ParameterExpression p = Expression.Parameter(typeof(float?), "p");
				        Expression<Func<float?>> e1 = Expression.Lambda<Func<float?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<float?, float?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(float?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<float?> f1 = e1.Compile();
				
				        Expression<Func<float?, Func<float?>>> e2 = Expression.Lambda<Func<float?, Func<float?>>>(
				            Expression.Lambda<Func<float?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<float?, Func<float?>> f2 = e2.Compile();
				
				        Expression<Func<Func<float?, float?>>> e3 = Expression.Lambda<Func<Func<float?, float?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<float?, float?>>>(
				                    Expression.Lambda<Func<float?, float?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<float?, float?> f3 = e3.Compile()();
				
				        Expression<Func<Func<float?, float?>>> e4 = Expression.Lambda<Func<Func<float?, float?>>>(
				            Expression.Lambda<Func<float?, float?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<float?, float?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 362
			namespace Scenario362{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "doubleq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression doubleq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_doubleq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_doubleq_Lambda() {
				        foreach (double? val in new double?[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN }) {
				            if (!check_doubleq_Lambda(val)) {
				                Console.WriteLine("doubleq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_doubleq_Lambda(double? val) {
				        ParameterExpression p = Expression.Parameter(typeof(double?), "p");
				        Expression<Func<double?>> e1 = Expression.Lambda<Func<double?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<double?, double?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(double?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<double?> f1 = e1.Compile();
				
				        Expression<Func<double?, Func<double?>>> e2 = Expression.Lambda<Func<double?, Func<double?>>>(
				            Expression.Lambda<Func<double?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<double?, Func<double?>> f2 = e2.Compile();
				
				        Expression<Func<Func<double?, double?>>> e3 = Expression.Lambda<Func<Func<double?, double?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<double?, double?>>>(
				                    Expression.Lambda<Func<double?, double?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<double?, double?> f3 = e3.Compile()();
				
				        Expression<Func<Func<double?, double?>>> e4 = Expression.Lambda<Func<Func<double?, double?>>>(
				            Expression.Lambda<Func<double?, double?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<double?, double?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 363
			namespace Scenario363{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "decimalq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression decimalq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_decimalq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_decimalq_Lambda() {
				        foreach (decimal? val in new decimal?[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue }) {
				            if (!check_decimalq_Lambda(val)) {
				                Console.WriteLine("decimalq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimalq_Lambda(decimal? val) {
				        ParameterExpression p = Expression.Parameter(typeof(decimal?), "p");
				        Expression<Func<decimal?>> e1 = Expression.Lambda<Func<decimal?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<decimal?, decimal?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(decimal?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal?> f1 = e1.Compile();
				
				        Expression<Func<decimal?, Func<decimal?>>> e2 = Expression.Lambda<Func<decimal?, Func<decimal?>>>(
				            Expression.Lambda<Func<decimal?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<decimal?, Func<decimal?>> f2 = e2.Compile();
				
				        Expression<Func<Func<decimal?, decimal?>>> e3 = Expression.Lambda<Func<Func<decimal?, decimal?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<decimal?, decimal?>>>(
				                    Expression.Lambda<Func<decimal?, decimal?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal?, decimal?> f3 = e3.Compile()();
				
				        Expression<Func<Func<decimal?, decimal?>>> e4 = Expression.Lambda<Func<Func<decimal?, decimal?>>>(
				            Expression.Lambda<Func<decimal?, decimal?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<decimal?, decimal?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 364
			namespace Scenario364{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "charq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression charq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_charq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_charq_Lambda() {
				        foreach (char? val in new char?[] { '\0', '\b', 'A', '\uffff' }) {
				            if (!check_charq_Lambda(val)) {
				                Console.WriteLine("charq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_charq_Lambda(char? val) {
				        ParameterExpression p = Expression.Parameter(typeof(char?), "p");
				        Expression<Func<char?>> e1 = Expression.Lambda<Func<char?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<char?, char?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(char?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<char?> f1 = e1.Compile();
				
				        Expression<Func<char?, Func<char?>>> e2 = Expression.Lambda<Func<char?, Func<char?>>>(
				            Expression.Lambda<Func<char?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<char?, Func<char?>> f2 = e2.Compile();
				
				        Expression<Func<Func<char?, char?>>> e3 = Expression.Lambda<Func<Func<char?, char?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<char?, char?>>>(
				                    Expression.Lambda<Func<char?, char?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<char?, char?> f3 = e3.Compile()();
				
				        Expression<Func<Func<char?, char?>>> e4 = Expression.Lambda<Func<Func<char?, char?>>>(
				            Expression.Lambda<Func<char?, char?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<char?, char?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 365
			namespace Scenario365{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "boolq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression boolq_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_boolq_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_boolq_Lambda() {
				        foreach (bool? val in new bool?[] { true, false }) {
				            if (!check_boolq_Lambda(val)) {
				                Console.WriteLine("boolq_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_boolq_Lambda(bool? val) {
				        ParameterExpression p = Expression.Parameter(typeof(bool?), "p");
				        Expression<Func<bool?>> e1 = Expression.Lambda<Func<bool?>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<bool?, bool?>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(bool?)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool?> f1 = e1.Compile();
				
				        Expression<Func<bool?, Func<bool?>>> e2 = Expression.Lambda<Func<bool?, Func<bool?>>>(
				            Expression.Lambda<Func<bool?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<bool?, Func<bool?>> f2 = e2.Compile();
				
				        Expression<Func<Func<bool?, bool?>>> e3 = Expression.Lambda<Func<Func<bool?, bool?>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<bool?, bool?>>>(
				                    Expression.Lambda<Func<bool?, bool?>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool?, bool?> f3 = e3.Compile()();
				
				        Expression<Func<Func<bool?, bool?>>> e4 = Expression.Lambda<Func<Func<bool?, bool?>>>(
				            Expression.Lambda<Func<bool?, bool?>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<bool?, bool?>> f4 = e4.Compile();
				
				        return object.Equals(f1(), val) &&
				            object.Equals(f2(val)(), val) &&
				            object.Equals(f3(val), val) &&
				            object.Equals(f4()(val), val);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
				
				//-------- Scenario 366
				namespace Scenario366{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Sq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Sq_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Sq_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Sq_Lambda() {
					        foreach (S? val in new S?[] { default(S), new S() }) {
					            if (!check_Sq_Lambda(val)) {
					                Console.WriteLine("Sq_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Sq_Lambda(S? val) {
					        ParameterExpression p = Expression.Parameter(typeof(S?), "p");
					        Expression<Func<S?>> e1 = Expression.Lambda<Func<S?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<S?, S?>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(S?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<S?> f1 = e1.Compile();
					
					        Expression<Func<S?, Func<S?>>> e2 = Expression.Lambda<Func<S?, Func<S?>>>(
					            Expression.Lambda<Func<S?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<S?, Func<S?>> f2 = e2.Compile();
					
					        Expression<Func<Func<S?, S?>>> e3 = Expression.Lambda<Func<Func<S?, S?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<S?, S?>>>(
					                    Expression.Lambda<Func<S?, S?>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<S?, S?> f3 = e3.Compile()();
					
					        Expression<Func<Func<S?, S?>>> e4 = Expression.Lambda<Func<Func<S?, S?>>>(
					            Expression.Lambda<Func<S?, S?>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<S?, S?>> f4 = e4.Compile();
					
					        return object.Equals(f1(), val) &&
					            object.Equals(f2(val)(), val) &&
					            object.Equals(f3(val), val) &&
					            object.Equals(f4()(val), val);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 367
				namespace Scenario367{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Spq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Spq_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Spq_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Spq_Lambda() {
					        foreach (Sp? val in new Sp?[] { default(Sp), new Sp(), new Sp(5,5.0) }) {
					            if (!check_Spq_Lambda(val)) {
					                Console.WriteLine("Spq_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Spq_Lambda(Sp? val) {
					        ParameterExpression p = Expression.Parameter(typeof(Sp?), "p");
					        Expression<Func<Sp?>> e1 = Expression.Lambda<Func<Sp?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Sp?, Sp?>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Sp?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp?> f1 = e1.Compile();
					
					        Expression<Func<Sp?, Func<Sp?>>> e2 = Expression.Lambda<Func<Sp?, Func<Sp?>>>(
					            Expression.Lambda<Func<Sp?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Sp?, Func<Sp?>> f2 = e2.Compile();
					
					        Expression<Func<Func<Sp?, Sp?>>> e3 = Expression.Lambda<Func<Func<Sp?, Sp?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Sp?, Sp?>>>(
					                    Expression.Lambda<Func<Sp?, Sp?>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp?, Sp?> f3 = e3.Compile()();
					
					        Expression<Func<Func<Sp?, Sp?>>> e4 = Expression.Lambda<Func<Func<Sp?, Sp?>>>(
					            Expression.Lambda<Func<Sp?, Sp?>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Sp?, Sp?>> f4 = e4.Compile();
					
					        return object.Equals(f1(), val) &&
					            object.Equals(f2(val)(), val) &&
					            object.Equals(f3(val), val) &&
					            object.Equals(f4()(val), val);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 368
				namespace Scenario368{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ssq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ssq_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ssq_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ssq_Lambda() {
					        foreach (Ss? val in new Ss?[] { default(Ss), new Ss(), new Ss(new S()) }) {
					            if (!check_Ssq_Lambda(val)) {
					                Console.WriteLine("Ssq_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ssq_Lambda(Ss? val) {
					        ParameterExpression p = Expression.Parameter(typeof(Ss?), "p");
					        Expression<Func<Ss?>> e1 = Expression.Lambda<Func<Ss?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Ss?, Ss?>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Ss?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss?> f1 = e1.Compile();
					
					        Expression<Func<Ss?, Func<Ss?>>> e2 = Expression.Lambda<Func<Ss?, Func<Ss?>>>(
					            Expression.Lambda<Func<Ss?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Ss?, Func<Ss?>> f2 = e2.Compile();
					
					        Expression<Func<Func<Ss?, Ss?>>> e3 = Expression.Lambda<Func<Func<Ss?, Ss?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Ss?, Ss?>>>(
					                    Expression.Lambda<Func<Ss?, Ss?>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss?, Ss?> f3 = e3.Compile()();
					
					        Expression<Func<Func<Ss?, Ss?>>> e4 = Expression.Lambda<Func<Func<Ss?, Ss?>>>(
					            Expression.Lambda<Func<Ss?, Ss?>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Ss?, Ss?>> f4 = e4.Compile();
					
					        return object.Equals(f1(), val) &&
					            object.Equals(f2(val)(), val) &&
					            object.Equals(f3(val), val) &&
					            object.Equals(f4()(val), val);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 369
				namespace Scenario369{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Scq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Scq_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Scq_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Scq_Lambda() {
					        foreach (Sc? val in new Sc?[] { default(Sc), new Sc(), new Sc(null) }) {
					            if (!check_Scq_Lambda(val)) {
					                Console.WriteLine("Scq_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Scq_Lambda(Sc? val) {
					        ParameterExpression p = Expression.Parameter(typeof(Sc?), "p");
					        Expression<Func<Sc?>> e1 = Expression.Lambda<Func<Sc?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Sc?, Sc?>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Sc?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc?> f1 = e1.Compile();
					
					        Expression<Func<Sc?, Func<Sc?>>> e2 = Expression.Lambda<Func<Sc?, Func<Sc?>>>(
					            Expression.Lambda<Func<Sc?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Sc?, Func<Sc?>> f2 = e2.Compile();
					
					        Expression<Func<Func<Sc?, Sc?>>> e3 = Expression.Lambda<Func<Func<Sc?, Sc?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Sc?, Sc?>>>(
					                    Expression.Lambda<Func<Sc?, Sc?>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc?, Sc?> f3 = e3.Compile()();
					
					        Expression<Func<Func<Sc?, Sc?>>> e4 = Expression.Lambda<Func<Func<Sc?, Sc?>>>(
					            Expression.Lambda<Func<Sc?, Sc?>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Sc?, Sc?>> f4 = e4.Compile();
					
					        return object.Equals(f1(), val) &&
					            object.Equals(f2(val)(), val) &&
					            object.Equals(f3(val), val) &&
					            object.Equals(f4()(val), val);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 370
				namespace Scenario370{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Scsq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Scsq_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Scsq_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Scsq_Lambda() {
					        foreach (Scs? val in new Scs?[] { default(Scs), new Scs(), new Scs(null,new S()) }) {
					            if (!check_Scsq_Lambda(val)) {
					                Console.WriteLine("Scsq_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Scsq_Lambda(Scs? val) {
					        ParameterExpression p = Expression.Parameter(typeof(Scs?), "p");
					        Expression<Func<Scs?>> e1 = Expression.Lambda<Func<Scs?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Scs?, Scs?>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Scs?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs?> f1 = e1.Compile();
					
					        Expression<Func<Scs?, Func<Scs?>>> e2 = Expression.Lambda<Func<Scs?, Func<Scs?>>>(
					            Expression.Lambda<Func<Scs?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Scs?, Func<Scs?>> f2 = e2.Compile();
					
					        Expression<Func<Func<Scs?, Scs?>>> e3 = Expression.Lambda<Func<Func<Scs?, Scs?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Scs?, Scs?>>>(
					                    Expression.Lambda<Func<Scs?, Scs?>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs?, Scs?> f3 = e3.Compile()();
					
					        Expression<Func<Func<Scs?, Scs?>>> e4 = Expression.Lambda<Func<Func<Scs?, Scs?>>>(
					            Expression.Lambda<Func<Scs?, Scs?>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Scs?, Scs?>> f4 = e4.Compile();
					
					        return object.Equals(f1(), val) &&
					            object.Equals(f2(val)(), val) &&
					            object.Equals(f3(val), val) &&
					            object.Equals(f4()(val), val);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 371
				namespace Scenario371{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tsq_Lambda_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tsq_Lambda_S___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tsq_Lambda<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tsq_Lambda<Ts>() where Ts : struct {
					        foreach (Ts? val in new Ts?[] { default(Ts), new Ts() }) {
					            if (!check_Tsq_Lambda<Ts>(val)) {
					                Console.WriteLine("Tsq_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tsq_Lambda<Ts>(Ts? val) where Ts : struct {
					        ParameterExpression p = Expression.Parameter(typeof(Ts?), "p");
					        Expression<Func<Ts?>> e1 = Expression.Lambda<Func<Ts?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Ts?, Ts?>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Ts?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?> f1 = e1.Compile();
					
					        Expression<Func<Ts?, Func<Ts?>>> e2 = Expression.Lambda<Func<Ts?, Func<Ts?>>>(
					            Expression.Lambda<Func<Ts?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Ts?, Func<Ts?>> f2 = e2.Compile();
					
					        Expression<Func<Func<Ts?, Ts?>>> e3 = Expression.Lambda<Func<Func<Ts?, Ts?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Ts?, Ts?>>>(
					                    Expression.Lambda<Func<Ts?, Ts?>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?, Ts?> f3 = e3.Compile()();
					
					        Expression<Func<Func<Ts?, Ts?>>> e4 = Expression.Lambda<Func<Func<Ts?, Ts?>>>(
					            Expression.Lambda<Func<Ts?, Ts?>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Ts?, Ts?>> f4 = e4.Compile();
					
					        return object.Equals(f1(), val) &&
					            object.Equals(f2(val)(), val) &&
					            object.Equals(f3(val), val) &&
					            object.Equals(f4()(val), val);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 372
				namespace Scenario372{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tsq_Lambda_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tsq_Lambda_Scs___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tsq_Lambda<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tsq_Lambda<Ts>() where Ts : struct {
					        foreach (Ts? val in new Ts?[] { default(Ts), new Ts() }) {
					            if (!check_Tsq_Lambda<Ts>(val)) {
					                Console.WriteLine("Tsq_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tsq_Lambda<Ts>(Ts? val) where Ts : struct {
					        ParameterExpression p = Expression.Parameter(typeof(Ts?), "p");
					        Expression<Func<Ts?>> e1 = Expression.Lambda<Func<Ts?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Ts?, Ts?>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Ts?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?> f1 = e1.Compile();
					
					        Expression<Func<Ts?, Func<Ts?>>> e2 = Expression.Lambda<Func<Ts?, Func<Ts?>>>(
					            Expression.Lambda<Func<Ts?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Ts?, Func<Ts?>> f2 = e2.Compile();
					
					        Expression<Func<Func<Ts?, Ts?>>> e3 = Expression.Lambda<Func<Func<Ts?, Ts?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Ts?, Ts?>>>(
					                    Expression.Lambda<Func<Ts?, Ts?>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?, Ts?> f3 = e3.Compile()();
					
					        Expression<Func<Func<Ts?, Ts?>>> e4 = Expression.Lambda<Func<Func<Ts?, Ts?>>>(
					            Expression.Lambda<Func<Ts?, Ts?>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Ts?, Ts?>> f4 = e4.Compile();
					
					        return object.Equals(f1(), val) &&
					            object.Equals(f2(val)(), val) &&
					            object.Equals(f3(val), val) &&
					            object.Equals(f4()(val), val);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 373
				namespace Scenario373{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tsq_Lambda_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tsq_Lambda_E___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tsq_Lambda<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tsq_Lambda<Ts>() where Ts : struct {
					        foreach (Ts? val in new Ts?[] { default(Ts), new Ts() }) {
					            if (!check_Tsq_Lambda<Ts>(val)) {
					                Console.WriteLine("Tsq_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tsq_Lambda<Ts>(Ts? val) where Ts : struct {
					        ParameterExpression p = Expression.Parameter(typeof(Ts?), "p");
					        Expression<Func<Ts?>> e1 = Expression.Lambda<Func<Ts?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Ts?, Ts?>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Ts?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?> f1 = e1.Compile();
					
					        Expression<Func<Ts?, Func<Ts?>>> e2 = Expression.Lambda<Func<Ts?, Func<Ts?>>>(
					            Expression.Lambda<Func<Ts?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Ts?, Func<Ts?>> f2 = e2.Compile();
					
					        Expression<Func<Func<Ts?, Ts?>>> e3 = Expression.Lambda<Func<Func<Ts?, Ts?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Ts?, Ts?>>>(
					                    Expression.Lambda<Func<Ts?, Ts?>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?, Ts?> f3 = e3.Compile()();
					
					        Expression<Func<Func<Ts?, Ts?>>> e4 = Expression.Lambda<Func<Func<Ts?, Ts?>>>(
					            Expression.Lambda<Func<Ts?, Ts?>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Ts?, Ts?>> f4 = e4.Compile();
					
					        return object.Equals(f1(), val) &&
					            object.Equals(f2(val)(), val) &&
					            object.Equals(f3(val), val) &&
					            object.Equals(f4()(val), val);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 374
				namespace Scenario374{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Eq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Eq_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Eq_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Eq_Lambda() {
					        foreach (E? val in new E?[] { (E) 0, E.A, E.B, (E) int.MaxValue, (E) int.MinValue }) {
					            if (!check_Eq_Lambda(val)) {
					                Console.WriteLine("Eq_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Eq_Lambda(E? val) {
					        ParameterExpression p = Expression.Parameter(typeof(E?), "p");
					        Expression<Func<E?>> e1 = Expression.Lambda<Func<E?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<E?, E?>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(E?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<E?> f1 = e1.Compile();
					
					        Expression<Func<E?, Func<E?>>> e2 = Expression.Lambda<Func<E?, Func<E?>>>(
					            Expression.Lambda<Func<E?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<E?, Func<E?>> f2 = e2.Compile();
					
					        Expression<Func<Func<E?, E?>>> e3 = Expression.Lambda<Func<Func<E?, E?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<E?, E?>>>(
					                    Expression.Lambda<Func<E?, E?>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<E?, E?> f3 = e3.Compile()();
					
					        Expression<Func<Func<E?, E?>>> e4 = Expression.Lambda<Func<Func<E?, E?>>>(
					            Expression.Lambda<Func<E?, E?>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<E?, E?>> f4 = e4.Compile();
					
					        return object.Equals(f1(), val) &&
					            object.Equals(f2(val)(), val) &&
					            object.Equals(f3(val), val) &&
					            object.Equals(f4()(val), val);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
				
				//-------- Scenario 375
				namespace Scenario375{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Elq_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Elq_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Elq_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Elq_Lambda() {
					        foreach (El? val in new El?[] { (El) 0, El.A, El.B, (El) long.MaxValue, (El) long.MinValue }) {
					            if (!check_Elq_Lambda(val)) {
					                Console.WriteLine("Elq_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Elq_Lambda(El? val) {
					        ParameterExpression p = Expression.Parameter(typeof(El?), "p");
					        Expression<Func<El?>> e1 = Expression.Lambda<Func<El?>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<El?, El?>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(El?)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<El?> f1 = e1.Compile();
					
					        Expression<Func<El?, Func<El?>>> e2 = Expression.Lambda<Func<El?, Func<El?>>>(
					            Expression.Lambda<Func<El?>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<El?, Func<El?>> f2 = e2.Compile();
					
					        Expression<Func<Func<El?, El?>>> e3 = Expression.Lambda<Func<Func<El?, El?>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<El?, El?>>>(
					                    Expression.Lambda<Func<El?, El?>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<El?, El?> f3 = e3.Compile()();
					
					        Expression<Func<Func<El?, El?>>> e4 = Expression.Lambda<Func<Func<El?, El?>>>(
					            Expression.Lambda<Func<El?, El?>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<El?, El?>> f4 = e4.Compile();
					
					        return object.Equals(f1(), val) &&
					            object.Equals(f2(val)(), val) &&
					            object.Equals(f3(val), val) &&
					            object.Equals(f4()(val), val);
					    }
					}
				
				
				public  static class Ext {
				    public static void StartCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[] { "test.dll" });
				    }
				
				    public static void StopCapture() {
				//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				//        m.Invoke(null, new object[0]);
				    }
				
				    public static bool IsIntegralOrEnum(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Byte:
				            case TypeCode.SByte:
				            case TypeCode.Int16:
				            case TypeCode.Int32:
				            case TypeCode.Int64:
				            case TypeCode.UInt16:
				            case TypeCode.UInt32:
				            case TypeCode.UInt64:
				                return true;
				            default:
				                return false;
				        }
				    }
				
				    public static bool IsFloating(Type type) {
				        switch (Type.GetTypeCode(GetNonNullableType(type))) {
				            case TypeCode.Single:
				            case TypeCode.Double:
				                return true;
				            default:
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
