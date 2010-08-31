#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 310
			namespace Scenario310{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_Lambda() {
				        foreach (byte val in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_Lambda(val)) {
				                Console.WriteLine("byte_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_Lambda(byte val) {
				        ParameterExpression p = Expression.Parameter(typeof(byte), "p");
				        Expression<Func<byte>> e1 = Expression.Lambda<Func<byte>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<byte, byte>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(byte)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte> f1 = e1.Compile();
				
				        Expression<Func<byte, Func<byte>>> e2 = Expression.Lambda<Func<byte, Func<byte>>>(
				            Expression.Lambda<Func<byte>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<byte, Func<byte>> f2 = e2.Compile();
				
				        Expression<Func<Func<byte, byte>>> e3 = Expression.Lambda<Func<Func<byte, byte>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<byte, byte>>>(
				                    Expression.Lambda<Func<byte, byte>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte, byte> f3 = e3.Compile()();
				
				        Expression<Func<Func<byte, byte>>> e4 = Expression.Lambda<Func<Func<byte, byte>>>(
				            Expression.Lambda<Func<byte, byte>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<byte, byte>> f4 = e4.Compile();
				
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
			
			//-------- Scenario 311
			namespace Scenario311{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_Lambda() {
				        foreach (ushort val in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_Lambda(val)) {
				                Console.WriteLine("ushort_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_Lambda(ushort val) {
				        ParameterExpression p = Expression.Parameter(typeof(ushort), "p");
				        Expression<Func<ushort>> e1 = Expression.Lambda<Func<ushort>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<ushort, ushort>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(ushort)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort> f1 = e1.Compile();
				
				        Expression<Func<ushort, Func<ushort>>> e2 = Expression.Lambda<Func<ushort, Func<ushort>>>(
				            Expression.Lambda<Func<ushort>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<ushort, Func<ushort>> f2 = e2.Compile();
				
				        Expression<Func<Func<ushort, ushort>>> e3 = Expression.Lambda<Func<Func<ushort, ushort>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<ushort, ushort>>>(
				                    Expression.Lambda<Func<ushort, ushort>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort, ushort> f3 = e3.Compile()();
				
				        Expression<Func<Func<ushort, ushort>>> e4 = Expression.Lambda<Func<Func<ushort, ushort>>>(
				            Expression.Lambda<Func<ushort, ushort>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<ushort, ushort>> f4 = e4.Compile();
				
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
			
			//-------- Scenario 312
			namespace Scenario312{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_Lambda() {
				        foreach (uint val in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_Lambda(val)) {
				                Console.WriteLine("uint_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_Lambda(uint val) {
				        ParameterExpression p = Expression.Parameter(typeof(uint), "p");
				        Expression<Func<uint>> e1 = Expression.Lambda<Func<uint>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<uint, uint>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(uint)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint> f1 = e1.Compile();
				
				        Expression<Func<uint, Func<uint>>> e2 = Expression.Lambda<Func<uint, Func<uint>>>(
				            Expression.Lambda<Func<uint>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<uint, Func<uint>> f2 = e2.Compile();
				
				        Expression<Func<Func<uint, uint>>> e3 = Expression.Lambda<Func<Func<uint, uint>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<uint, uint>>>(
				                    Expression.Lambda<Func<uint, uint>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint, uint> f3 = e3.Compile()();
				
				        Expression<Func<Func<uint, uint>>> e4 = Expression.Lambda<Func<Func<uint, uint>>>(
				            Expression.Lambda<Func<uint, uint>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<uint, uint>> f4 = e4.Compile();
				
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
			
			//-------- Scenario 313
			namespace Scenario313{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_Lambda() {
				        foreach (ulong val in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_Lambda(val)) {
				                Console.WriteLine("ulong_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_Lambda(ulong val) {
				        ParameterExpression p = Expression.Parameter(typeof(ulong), "p");
				        Expression<Func<ulong>> e1 = Expression.Lambda<Func<ulong>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<ulong, ulong>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(ulong)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong> f1 = e1.Compile();
				
				        Expression<Func<ulong, Func<ulong>>> e2 = Expression.Lambda<Func<ulong, Func<ulong>>>(
				            Expression.Lambda<Func<ulong>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<ulong, Func<ulong>> f2 = e2.Compile();
				
				        Expression<Func<Func<ulong, ulong>>> e3 = Expression.Lambda<Func<Func<ulong, ulong>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<ulong, ulong>>>(
				                    Expression.Lambda<Func<ulong, ulong>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong, ulong> f3 = e3.Compile()();
				
				        Expression<Func<Func<ulong, ulong>>> e4 = Expression.Lambda<Func<Func<ulong, ulong>>>(
				            Expression.Lambda<Func<ulong, ulong>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<ulong, ulong>> f4 = e4.Compile();
				
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
			
			//-------- Scenario 314
			namespace Scenario314{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_Lambda() {
				        foreach (sbyte val in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_Lambda(val)) {
				                Console.WriteLine("sbyte_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_Lambda(sbyte val) {
				        ParameterExpression p = Expression.Parameter(typeof(sbyte), "p");
				        Expression<Func<sbyte>> e1 = Expression.Lambda<Func<sbyte>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<sbyte, sbyte>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(sbyte)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte> f1 = e1.Compile();
				
				        Expression<Func<sbyte, Func<sbyte>>> e2 = Expression.Lambda<Func<sbyte, Func<sbyte>>>(
				            Expression.Lambda<Func<sbyte>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<sbyte, Func<sbyte>> f2 = e2.Compile();
				
				        Expression<Func<Func<sbyte, sbyte>>> e3 = Expression.Lambda<Func<Func<sbyte, sbyte>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<sbyte, sbyte>>>(
				                    Expression.Lambda<Func<sbyte, sbyte>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte, sbyte> f3 = e3.Compile()();
				
				        Expression<Func<Func<sbyte, sbyte>>> e4 = Expression.Lambda<Func<Func<sbyte, sbyte>>>(
				            Expression.Lambda<Func<sbyte, sbyte>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<sbyte, sbyte>> f4 = e4.Compile();
				
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
			
			//-------- Scenario 315
			namespace Scenario315{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_Lambda() {
				        foreach (short val in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_Lambda(val)) {
				                Console.WriteLine("short_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_Lambda(short val) {
				        ParameterExpression p = Expression.Parameter(typeof(short), "p");
				        Expression<Func<short>> e1 = Expression.Lambda<Func<short>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<short, short>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(short)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<short> f1 = e1.Compile();
				
				        Expression<Func<short, Func<short>>> e2 = Expression.Lambda<Func<short, Func<short>>>(
				            Expression.Lambda<Func<short>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<short, Func<short>> f2 = e2.Compile();
				
				        Expression<Func<Func<short, short>>> e3 = Expression.Lambda<Func<Func<short, short>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<short, short>>>(
				                    Expression.Lambda<Func<short, short>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<short, short> f3 = e3.Compile()();
				
				        Expression<Func<Func<short, short>>> e4 = Expression.Lambda<Func<Func<short, short>>>(
				            Expression.Lambda<Func<short, short>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<short, short>> f4 = e4.Compile();
				
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
			
			//-------- Scenario 316
			namespace Scenario316{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_Lambda() {
				        foreach (int val in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_Lambda(val)) {
				                Console.WriteLine("int_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_Lambda(int val) {
				        ParameterExpression p = Expression.Parameter(typeof(int), "p");
				        Expression<Func<int>> e1 = Expression.Lambda<Func<int>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<int, int>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(int)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f1 = e1.Compile();
				
				        Expression<Func<int, Func<int>>> e2 = Expression.Lambda<Func<int, Func<int>>>(
				            Expression.Lambda<Func<int>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<int, Func<int>> f2 = e2.Compile();
				
				        Expression<Func<Func<int, int>>> e3 = Expression.Lambda<Func<Func<int, int>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<int, int>>>(
				                    Expression.Lambda<Func<int, int>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<int, int> f3 = e3.Compile()();
				
				        Expression<Func<Func<int, int>>> e4 = Expression.Lambda<Func<Func<int, int>>>(
				            Expression.Lambda<Func<int, int>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<int, int>> f4 = e4.Compile();
				
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
			
			//-------- Scenario 317
			namespace Scenario317{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_Lambda() {
				        foreach (long val in new long[] { 0, 1, -1, long.MinValue, long.MaxValue }) {
				            if (!check_long_Lambda(val)) {
				                Console.WriteLine("long_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_Lambda(long val) {
				        ParameterExpression p = Expression.Parameter(typeof(long), "p");
				        Expression<Func<long>> e1 = Expression.Lambda<Func<long>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<long, long>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(long)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<long> f1 = e1.Compile();
				
				        Expression<Func<long, Func<long>>> e2 = Expression.Lambda<Func<long, Func<long>>>(
				            Expression.Lambda<Func<long>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<long, Func<long>> f2 = e2.Compile();
				
				        Expression<Func<Func<long, long>>> e3 = Expression.Lambda<Func<Func<long, long>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<long, long>>>(
				                    Expression.Lambda<Func<long, long>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<long, long> f3 = e3.Compile()();
				
				        Expression<Func<Func<long, long>>> e4 = Expression.Lambda<Func<Func<long, long>>>(
				            Expression.Lambda<Func<long, long>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<long, long>> f4 = e4.Compile();
				
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
			
			//-------- Scenario 318
			namespace Scenario318{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "float_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression float_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_float_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_float_Lambda() {
				        foreach (float val in new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN }) {
				            if (!check_float_Lambda(val)) {
				                Console.WriteLine("float_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_Lambda(float val) {
				        ParameterExpression p = Expression.Parameter(typeof(float), "p");
				        Expression<Func<float>> e1 = Expression.Lambda<Func<float>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<float, float>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(float)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<float> f1 = e1.Compile();
				
				        Expression<Func<float, Func<float>>> e2 = Expression.Lambda<Func<float, Func<float>>>(
				            Expression.Lambda<Func<float>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<float, Func<float>> f2 = e2.Compile();
				
				        Expression<Func<Func<float, float>>> e3 = Expression.Lambda<Func<Func<float, float>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<float, float>>>(
				                    Expression.Lambda<Func<float, float>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<float, float> f3 = e3.Compile()();
				
				        Expression<Func<Func<float, float>>> e4 = Expression.Lambda<Func<Func<float, float>>>(
				            Expression.Lambda<Func<float, float>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<float, float>> f4 = e4.Compile();
				
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
			
			//-------- Scenario 319
			namespace Scenario319{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "double_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression double_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_double_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_double_Lambda() {
				        foreach (double val in new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN }) {
				            if (!check_double_Lambda(val)) {
				                Console.WriteLine("double_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_Lambda(double val) {
				        ParameterExpression p = Expression.Parameter(typeof(double), "p");
				        Expression<Func<double>> e1 = Expression.Lambda<Func<double>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<double, double>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(double)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<double> f1 = e1.Compile();
				
				        Expression<Func<double, Func<double>>> e2 = Expression.Lambda<Func<double, Func<double>>>(
				            Expression.Lambda<Func<double>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<double, Func<double>> f2 = e2.Compile();
				
				        Expression<Func<Func<double, double>>> e3 = Expression.Lambda<Func<Func<double, double>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<double, double>>>(
				                    Expression.Lambda<Func<double, double>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<double, double> f3 = e3.Compile()();
				
				        Expression<Func<Func<double, double>>> e4 = Expression.Lambda<Func<Func<double, double>>>(
				            Expression.Lambda<Func<double, double>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<double, double>> f4 = e4.Compile();
				
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
			
			//-------- Scenario 320
			namespace Scenario320{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "decimal_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression decimal_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_decimal_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_decimal_Lambda() {
				        foreach (decimal val in new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue }) {
				            if (!check_decimal_Lambda(val)) {
				                Console.WriteLine("decimal_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_Lambda(decimal val) {
				        ParameterExpression p = Expression.Parameter(typeof(decimal), "p");
				        Expression<Func<decimal>> e1 = Expression.Lambda<Func<decimal>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<decimal, decimal>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(decimal)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal> f1 = e1.Compile();
				
				        Expression<Func<decimal, Func<decimal>>> e2 = Expression.Lambda<Func<decimal, Func<decimal>>>(
				            Expression.Lambda<Func<decimal>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<decimal, Func<decimal>> f2 = e2.Compile();
				
				        Expression<Func<Func<decimal, decimal>>> e3 = Expression.Lambda<Func<Func<decimal, decimal>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<decimal, decimal>>>(
				                    Expression.Lambda<Func<decimal, decimal>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal, decimal> f3 = e3.Compile()();
				
				        Expression<Func<Func<decimal, decimal>>> e4 = Expression.Lambda<Func<Func<decimal, decimal>>>(
				            Expression.Lambda<Func<decimal, decimal>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<decimal, decimal>> f4 = e4.Compile();
				
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
			
			//-------- Scenario 321
			namespace Scenario321{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "char_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression char_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_char_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_char_Lambda() {
				        foreach (char val in new char[] { '\0', '\b', 'A', '\uffff' }) {
				            if (!check_char_Lambda(val)) {
				                Console.WriteLine("char_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_Lambda(char val) {
				        ParameterExpression p = Expression.Parameter(typeof(char), "p");
				        Expression<Func<char>> e1 = Expression.Lambda<Func<char>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<char, char>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(char)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<char> f1 = e1.Compile();
				
				        Expression<Func<char, Func<char>>> e2 = Expression.Lambda<Func<char, Func<char>>>(
				            Expression.Lambda<Func<char>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<char, Func<char>> f2 = e2.Compile();
				
				        Expression<Func<Func<char, char>>> e3 = Expression.Lambda<Func<Func<char, char>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<char, char>>>(
				                    Expression.Lambda<Func<char, char>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<char, char> f3 = e3.Compile()();
				
				        Expression<Func<Func<char, char>>> e4 = Expression.Lambda<Func<Func<char, char>>>(
				            Expression.Lambda<Func<char, char>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<char, char>> f4 = e4.Compile();
				
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
			
			//-------- Scenario 322
			namespace Scenario322{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "bool_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression bool_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_bool_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_bool_Lambda() {
				        foreach (bool val in new bool[] { true, false }) {
				            if (!check_bool_Lambda(val)) {
				                Console.WriteLine("bool_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_bool_Lambda(bool val) {
				        ParameterExpression p = Expression.Parameter(typeof(bool), "p");
				        Expression<Func<bool>> e1 = Expression.Lambda<Func<bool>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<bool, bool>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(bool)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool> f1 = e1.Compile();
				
				        Expression<Func<bool, Func<bool>>> e2 = Expression.Lambda<Func<bool, Func<bool>>>(
				            Expression.Lambda<Func<bool>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<bool, Func<bool>> f2 = e2.Compile();
				
				        Expression<Func<Func<bool, bool>>> e3 = Expression.Lambda<Func<Func<bool, bool>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<bool, bool>>>(
				                    Expression.Lambda<Func<bool, bool>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool, bool> f3 = e3.Compile()();
				
				        Expression<Func<Func<bool, bool>>> e4 = Expression.Lambda<Func<Func<bool, bool>>>(
				            Expression.Lambda<Func<bool, bool>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<bool, bool>> f4 = e4.Compile();
				
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
				
				//-------- Scenario 323
				namespace Scenario323{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "S_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression S_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_S_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_S_Lambda() {
					        foreach (S val in new S[] { default(S), new S() }) {
					            if (!check_S_Lambda(val)) {
					                Console.WriteLine("S_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_S_Lambda(S val) {
					        ParameterExpression p = Expression.Parameter(typeof(S), "p");
					        Expression<Func<S>> e1 = Expression.Lambda<Func<S>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<S, S>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(S)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<S> f1 = e1.Compile();
					
					        Expression<Func<S, Func<S>>> e2 = Expression.Lambda<Func<S, Func<S>>>(
					            Expression.Lambda<Func<S>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<S, Func<S>> f2 = e2.Compile();
					
					        Expression<Func<Func<S, S>>> e3 = Expression.Lambda<Func<Func<S, S>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<S, S>>>(
					                    Expression.Lambda<Func<S, S>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<S, S> f3 = e3.Compile()();
					
					        Expression<Func<Func<S, S>>> e4 = Expression.Lambda<Func<Func<S, S>>>(
					            Expression.Lambda<Func<S, S>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<S, S>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 324
				namespace Scenario324{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Sp_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Sp_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Sp_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Sp_Lambda() {
					        foreach (Sp val in new Sp[] { default(Sp), new Sp(), new Sp(5,5.0) }) {
					            if (!check_Sp_Lambda(val)) {
					                Console.WriteLine("Sp_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Sp_Lambda(Sp val) {
					        ParameterExpression p = Expression.Parameter(typeof(Sp), "p");
					        Expression<Func<Sp>> e1 = Expression.Lambda<Func<Sp>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Sp, Sp>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Sp)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp> f1 = e1.Compile();
					
					        Expression<Func<Sp, Func<Sp>>> e2 = Expression.Lambda<Func<Sp, Func<Sp>>>(
					            Expression.Lambda<Func<Sp>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Sp, Func<Sp>> f2 = e2.Compile();
					
					        Expression<Func<Func<Sp, Sp>>> e3 = Expression.Lambda<Func<Func<Sp, Sp>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Sp, Sp>>>(
					                    Expression.Lambda<Func<Sp, Sp>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp, Sp> f3 = e3.Compile()();
					
					        Expression<Func<Func<Sp, Sp>>> e4 = Expression.Lambda<Func<Func<Sp, Sp>>>(
					            Expression.Lambda<Func<Sp, Sp>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Sp, Sp>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 325
				namespace Scenario325{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ss_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ss_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ss_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ss_Lambda() {
					        foreach (Ss val in new Ss[] { default(Ss), new Ss(), new Ss(new S()) }) {
					            if (!check_Ss_Lambda(val)) {
					                Console.WriteLine("Ss_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ss_Lambda(Ss val) {
					        ParameterExpression p = Expression.Parameter(typeof(Ss), "p");
					        Expression<Func<Ss>> e1 = Expression.Lambda<Func<Ss>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Ss, Ss>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Ss)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss> f1 = e1.Compile();
					
					        Expression<Func<Ss, Func<Ss>>> e2 = Expression.Lambda<Func<Ss, Func<Ss>>>(
					            Expression.Lambda<Func<Ss>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Ss, Func<Ss>> f2 = e2.Compile();
					
					        Expression<Func<Func<Ss, Ss>>> e3 = Expression.Lambda<Func<Func<Ss, Ss>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Ss, Ss>>>(
					                    Expression.Lambda<Func<Ss, Ss>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss, Ss> f3 = e3.Compile()();
					
					        Expression<Func<Func<Ss, Ss>>> e4 = Expression.Lambda<Func<Func<Ss, Ss>>>(
					            Expression.Lambda<Func<Ss, Ss>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Ss, Ss>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 326
				namespace Scenario326{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Sc_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Sc_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Sc_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Sc_Lambda() {
					        foreach (Sc val in new Sc[] { default(Sc), new Sc(), new Sc(null) }) {
					            if (!check_Sc_Lambda(val)) {
					                Console.WriteLine("Sc_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Sc_Lambda(Sc val) {
					        ParameterExpression p = Expression.Parameter(typeof(Sc), "p");
					        Expression<Func<Sc>> e1 = Expression.Lambda<Func<Sc>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Sc, Sc>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Sc)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc> f1 = e1.Compile();
					
					        Expression<Func<Sc, Func<Sc>>> e2 = Expression.Lambda<Func<Sc, Func<Sc>>>(
					            Expression.Lambda<Func<Sc>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Sc, Func<Sc>> f2 = e2.Compile();
					
					        Expression<Func<Func<Sc, Sc>>> e3 = Expression.Lambda<Func<Func<Sc, Sc>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Sc, Sc>>>(
					                    Expression.Lambda<Func<Sc, Sc>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc, Sc> f3 = e3.Compile()();
					
					        Expression<Func<Func<Sc, Sc>>> e4 = Expression.Lambda<Func<Func<Sc, Sc>>>(
					            Expression.Lambda<Func<Sc, Sc>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Sc, Sc>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 327
				namespace Scenario327{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Scs_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Scs_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Scs_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Scs_Lambda() {
					        foreach (Scs val in new Scs[] { default(Scs), new Scs(), new Scs(null,new S()) }) {
					            if (!check_Scs_Lambda(val)) {
					                Console.WriteLine("Scs_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Scs_Lambda(Scs val) {
					        ParameterExpression p = Expression.Parameter(typeof(Scs), "p");
					        Expression<Func<Scs>> e1 = Expression.Lambda<Func<Scs>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Scs, Scs>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Scs)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs> f1 = e1.Compile();
					
					        Expression<Func<Scs, Func<Scs>>> e2 = Expression.Lambda<Func<Scs, Func<Scs>>>(
					            Expression.Lambda<Func<Scs>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Scs, Func<Scs>> f2 = e2.Compile();
					
					        Expression<Func<Func<Scs, Scs>>> e3 = Expression.Lambda<Func<Func<Scs, Scs>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Scs, Scs>>>(
					                    Expression.Lambda<Func<Scs, Scs>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs, Scs> f3 = e3.Compile()();
					
					        Expression<Func<Func<Scs, Scs>>> e4 = Expression.Lambda<Func<Func<Scs, Scs>>>(
					            Expression.Lambda<Func<Scs, Scs>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Scs, Scs>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 328
				namespace Scenario328{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ts_Lambda_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ts_Lambda_S___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ts_Lambda<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ts_Lambda<Ts>() where Ts : struct {
					        foreach (Ts val in new Ts[] { default(Ts), new Ts() }) {
					            if (!check_Ts_Lambda<Ts>(val)) {
					                Console.WriteLine("Ts_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ts_Lambda<Ts>(Ts val) where Ts : struct {
					        ParameterExpression p = Expression.Parameter(typeof(Ts), "p");
					        Expression<Func<Ts>> e1 = Expression.Lambda<Func<Ts>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Ts, Ts>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Ts)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts> f1 = e1.Compile();
					
					        Expression<Func<Ts, Func<Ts>>> e2 = Expression.Lambda<Func<Ts, Func<Ts>>>(
					            Expression.Lambda<Func<Ts>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Ts, Func<Ts>> f2 = e2.Compile();
					
					        Expression<Func<Func<Ts, Ts>>> e3 = Expression.Lambda<Func<Func<Ts, Ts>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Ts, Ts>>>(
					                    Expression.Lambda<Func<Ts, Ts>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts, Ts> f3 = e3.Compile()();
					
					        Expression<Func<Func<Ts, Ts>>> e4 = Expression.Lambda<Func<Func<Ts, Ts>>>(
					            Expression.Lambda<Func<Ts, Ts>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Ts, Ts>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 329
				namespace Scenario329{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ts_Lambda_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ts_Lambda_Scs___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ts_Lambda<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ts_Lambda<Ts>() where Ts : struct {
					        foreach (Ts val in new Ts[] { default(Ts), new Ts() }) {
					            if (!check_Ts_Lambda<Ts>(val)) {
					                Console.WriteLine("Ts_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ts_Lambda<Ts>(Ts val) where Ts : struct {
					        ParameterExpression p = Expression.Parameter(typeof(Ts), "p");
					        Expression<Func<Ts>> e1 = Expression.Lambda<Func<Ts>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Ts, Ts>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Ts)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts> f1 = e1.Compile();
					
					        Expression<Func<Ts, Func<Ts>>> e2 = Expression.Lambda<Func<Ts, Func<Ts>>>(
					            Expression.Lambda<Func<Ts>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Ts, Func<Ts>> f2 = e2.Compile();
					
					        Expression<Func<Func<Ts, Ts>>> e3 = Expression.Lambda<Func<Func<Ts, Ts>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Ts, Ts>>>(
					                    Expression.Lambda<Func<Ts, Ts>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts, Ts> f3 = e3.Compile()();
					
					        Expression<Func<Func<Ts, Ts>>> e4 = Expression.Lambda<Func<Func<Ts, Ts>>>(
					            Expression.Lambda<Func<Ts, Ts>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Ts, Ts>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 330
				namespace Scenario330{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ts_Lambda_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ts_Lambda_E___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ts_Lambda<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ts_Lambda<Ts>() where Ts : struct {
					        foreach (Ts val in new Ts[] { default(Ts), new Ts() }) {
					            if (!check_Ts_Lambda<Ts>(val)) {
					                Console.WriteLine("Ts_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ts_Lambda<Ts>(Ts val) where Ts : struct {
					        ParameterExpression p = Expression.Parameter(typeof(Ts), "p");
					        Expression<Func<Ts>> e1 = Expression.Lambda<Func<Ts>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Ts, Ts>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Ts)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts> f1 = e1.Compile();
					
					        Expression<Func<Ts, Func<Ts>>> e2 = Expression.Lambda<Func<Ts, Func<Ts>>>(
					            Expression.Lambda<Func<Ts>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Ts, Func<Ts>> f2 = e2.Compile();
					
					        Expression<Func<Func<Ts, Ts>>> e3 = Expression.Lambda<Func<Func<Ts, Ts>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Ts, Ts>>>(
					                    Expression.Lambda<Func<Ts, Ts>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts, Ts> f3 = e3.Compile()();
					
					        Expression<Func<Func<Ts, Ts>>> e4 = Expression.Lambda<Func<Func<Ts, Ts>>>(
					            Expression.Lambda<Func<Ts, Ts>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Ts, Ts>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 331
				namespace Scenario331{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "E_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression E_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_E_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_E_Lambda() {
					        foreach (E val in new E[] { (E) 0, E.A, E.B, (E) int.MaxValue, (E) int.MinValue }) {
					            if (!check_E_Lambda(val)) {
					                Console.WriteLine("E_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_E_Lambda(E val) {
					        ParameterExpression p = Expression.Parameter(typeof(E), "p");
					        Expression<Func<E>> e1 = Expression.Lambda<Func<E>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<E, E>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(E)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<E> f1 = e1.Compile();
					
					        Expression<Func<E, Func<E>>> e2 = Expression.Lambda<Func<E, Func<E>>>(
					            Expression.Lambda<Func<E>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<E, Func<E>> f2 = e2.Compile();
					
					        Expression<Func<Func<E, E>>> e3 = Expression.Lambda<Func<Func<E, E>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<E, E>>>(
					                    Expression.Lambda<Func<E, E>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<E, E> f3 = e3.Compile()();
					
					        Expression<Func<Func<E, E>>> e4 = Expression.Lambda<Func<Func<E, E>>>(
					            Expression.Lambda<Func<E, E>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<E, E>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 332
				namespace Scenario332{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "El_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression El_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_El_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_El_Lambda() {
					        foreach (El val in new El[] { (El) 0, El.A, El.B, (El) long.MaxValue, (El) long.MinValue }) {
					            if (!check_El_Lambda(val)) {
					                Console.WriteLine("El_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_El_Lambda(El val) {
					        ParameterExpression p = Expression.Parameter(typeof(El), "p");
					        Expression<Func<El>> e1 = Expression.Lambda<Func<El>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<El, El>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(El)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<El> f1 = e1.Compile();
					
					        Expression<Func<El, Func<El>>> e2 = Expression.Lambda<Func<El, Func<El>>>(
					            Expression.Lambda<Func<El>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<El, Func<El>> f2 = e2.Compile();
					
					        Expression<Func<Func<El, El>>> e3 = Expression.Lambda<Func<Func<El, El>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<El, El>>>(
					                    Expression.Lambda<Func<El, El>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<El, El> f3 = e3.Compile()();
					
					        Expression<Func<Func<El, El>>> e4 = Expression.Lambda<Func<Func<El, El>>>(
					            Expression.Lambda<Func<El, El>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<El, El>> f4 = e4.Compile();
					
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
			
			//-------- Scenario 333
			namespace Scenario333{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "string_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression string_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_string_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_string_Lambda() {
				        foreach (string val in new string[] { null, "", "a", "foo" }) {
				            if (!check_string_Lambda(val)) {
				                Console.WriteLine("string_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_string_Lambda(string val) {
				        ParameterExpression p = Expression.Parameter(typeof(string), "p");
				        Expression<Func<string>> e1 = Expression.Lambda<Func<string>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<string, string>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(string)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<string> f1 = e1.Compile();
				
				        Expression<Func<string, Func<string>>> e2 = Expression.Lambda<Func<string, Func<string>>>(
				            Expression.Lambda<Func<string>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<string, Func<string>> f2 = e2.Compile();
				
				        Expression<Func<Func<string, string>>> e3 = Expression.Lambda<Func<Func<string, string>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<string, string>>>(
				                    Expression.Lambda<Func<string, string>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<string, string> f3 = e3.Compile()();
				
				        Expression<Func<Func<string, string>>> e4 = Expression.Lambda<Func<Func<string, string>>>(
				            Expression.Lambda<Func<string, string>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<string, string>> f4 = e4.Compile();
				
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
				
				//-------- Scenario 334
				namespace Scenario334{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "object_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression object_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_object_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_object_Lambda() {
					        foreach (object val in new object[] { null, new object(), new C(), new D(3) }) {
					            if (!check_object_Lambda(val)) {
					                Console.WriteLine("object_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_object_Lambda(object val) {
					        ParameterExpression p = Expression.Parameter(typeof(object), "p");
					        Expression<Func<object>> e1 = Expression.Lambda<Func<object>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<object, object>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(object)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<object> f1 = e1.Compile();
					
					        Expression<Func<object, Func<object>>> e2 = Expression.Lambda<Func<object, Func<object>>>(
					            Expression.Lambda<Func<object>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<object, Func<object>> f2 = e2.Compile();
					
					        Expression<Func<Func<object, object>>> e3 = Expression.Lambda<Func<Func<object, object>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<object, object>>>(
					                    Expression.Lambda<Func<object, object>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<object, object> f3 = e3.Compile()();
					
					        Expression<Func<Func<object, object>>> e4 = Expression.Lambda<Func<Func<object, object>>>(
					            Expression.Lambda<Func<object, object>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<object, object>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 335
				namespace Scenario335{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "C_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression C_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_C_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_C_Lambda() {
					        foreach (C val in new C[] { null, new C(), new D(), new D(0), new D(5) }) {
					            if (!check_C_Lambda(val)) {
					                Console.WriteLine("C_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_C_Lambda(C val) {
					        ParameterExpression p = Expression.Parameter(typeof(C), "p");
					        Expression<Func<C>> e1 = Expression.Lambda<Func<C>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<C, C>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(C)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<C> f1 = e1.Compile();
					
					        Expression<Func<C, Func<C>>> e2 = Expression.Lambda<Func<C, Func<C>>>(
					            Expression.Lambda<Func<C>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<C, Func<C>> f2 = e2.Compile();
					
					        Expression<Func<Func<C, C>>> e3 = Expression.Lambda<Func<Func<C, C>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<C, C>>>(
					                    Expression.Lambda<Func<C, C>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<C, C> f3 = e3.Compile()();
					
					        Expression<Func<Func<C, C>>> e4 = Expression.Lambda<Func<Func<C, C>>>(
					            Expression.Lambda<Func<C, C>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<C, C>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 336
				namespace Scenario336{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "D_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression D_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_D_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_D_Lambda() {
					        foreach (D val in new D[] { null, new D(), new D(0), new D(5) }) {
					            if (!check_D_Lambda(val)) {
					                Console.WriteLine("D_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_D_Lambda(D val) {
					        ParameterExpression p = Expression.Parameter(typeof(D), "p");
					        Expression<Func<D>> e1 = Expression.Lambda<Func<D>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<D, D>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(D)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<D> f1 = e1.Compile();
					
					        Expression<Func<D, Func<D>>> e2 = Expression.Lambda<Func<D, Func<D>>>(
					            Expression.Lambda<Func<D>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<D, Func<D>> f2 = e2.Compile();
					
					        Expression<Func<Func<D, D>>> e3 = Expression.Lambda<Func<Func<D, D>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<D, D>>>(
					                    Expression.Lambda<Func<D, D>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<D, D> f3 = e3.Compile()();
					
					        Expression<Func<Func<D, D>>> e4 = Expression.Lambda<Func<Func<D, D>>>(
					            Expression.Lambda<Func<D, D>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<D, D>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 337
				namespace Scenario337{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_Lambda_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_Lambda_object___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_T_Lambda<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_Lambda<T>() {
					        foreach (T val in new T[] { default(T) }) {
					            if (!check_T_Lambda<T>(val)) {
					                Console.WriteLine("T_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_T_Lambda<T>(T val) {
					        ParameterExpression p = Expression.Parameter(typeof(T), "p");
					        Expression<Func<T>> e1 = Expression.Lambda<Func<T>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(T)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<T> f1 = e1.Compile();
					
					        Expression<Func<T, Func<T>>> e2 = Expression.Lambda<Func<T, Func<T>>>(
					            Expression.Lambda<Func<T>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<T, Func<T>> f2 = e2.Compile();
					
					        Expression<Func<Func<T, T>>> e3 = Expression.Lambda<Func<Func<T, T>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<T, T>>>(
					                    Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<T, T> f3 = e3.Compile()();
					
					        Expression<Func<Func<T, T>>> e4 = Expression.Lambda<Func<Func<T, T>>>(
					            Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<T, T>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 338
				namespace Scenario338{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_Lambda_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_Lambda_C___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_T_Lambda<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_Lambda<T>() {
					        foreach (T val in new T[] { default(T) }) {
					            if (!check_T_Lambda<T>(val)) {
					                Console.WriteLine("T_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_T_Lambda<T>(T val) {
					        ParameterExpression p = Expression.Parameter(typeof(T), "p");
					        Expression<Func<T>> e1 = Expression.Lambda<Func<T>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(T)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<T> f1 = e1.Compile();
					
					        Expression<Func<T, Func<T>>> e2 = Expression.Lambda<Func<T, Func<T>>>(
					            Expression.Lambda<Func<T>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<T, Func<T>> f2 = e2.Compile();
					
					        Expression<Func<Func<T, T>>> e3 = Expression.Lambda<Func<Func<T, T>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<T, T>>>(
					                    Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<T, T> f3 = e3.Compile()();
					
					        Expression<Func<Func<T, T>>> e4 = Expression.Lambda<Func<Func<T, T>>>(
					            Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<T, T>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 339
				namespace Scenario339{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_Lambda_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_Lambda_S___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_T_Lambda<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_Lambda<T>() {
					        foreach (T val in new T[] { default(T) }) {
					            if (!check_T_Lambda<T>(val)) {
					                Console.WriteLine("T_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_T_Lambda<T>(T val) {
					        ParameterExpression p = Expression.Parameter(typeof(T), "p");
					        Expression<Func<T>> e1 = Expression.Lambda<Func<T>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(T)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<T> f1 = e1.Compile();
					
					        Expression<Func<T, Func<T>>> e2 = Expression.Lambda<Func<T, Func<T>>>(
					            Expression.Lambda<Func<T>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<T, Func<T>> f2 = e2.Compile();
					
					        Expression<Func<Func<T, T>>> e3 = Expression.Lambda<Func<Func<T, T>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<T, T>>>(
					                    Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<T, T> f3 = e3.Compile()();
					
					        Expression<Func<Func<T, T>>> e4 = Expression.Lambda<Func<Func<T, T>>>(
					            Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<T, T>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 340
				namespace Scenario340{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_Lambda_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_Lambda_Scs___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_T_Lambda<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_Lambda<T>() {
					        foreach (T val in new T[] { default(T) }) {
					            if (!check_T_Lambda<T>(val)) {
					                Console.WriteLine("T_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_T_Lambda<T>(T val) {
					        ParameterExpression p = Expression.Parameter(typeof(T), "p");
					        Expression<Func<T>> e1 = Expression.Lambda<Func<T>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(T)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<T> f1 = e1.Compile();
					
					        Expression<Func<T, Func<T>>> e2 = Expression.Lambda<Func<T, Func<T>>>(
					            Expression.Lambda<Func<T>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<T, Func<T>> f2 = e2.Compile();
					
					        Expression<Func<Func<T, T>>> e3 = Expression.Lambda<Func<Func<T, T>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<T, T>>>(
					                    Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<T, T> f3 = e3.Compile()();
					
					        Expression<Func<Func<T, T>>> e4 = Expression.Lambda<Func<Func<T, T>>>(
					            Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<T, T>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 341
				namespace Scenario341{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_Lambda_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_Lambda_E___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_T_Lambda<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_Lambda<T>() {
					        foreach (T val in new T[] { default(T) }) {
					            if (!check_T_Lambda<T>(val)) {
					                Console.WriteLine("T_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_T_Lambda<T>(T val) {
					        ParameterExpression p = Expression.Parameter(typeof(T), "p");
					        Expression<Func<T>> e1 = Expression.Lambda<Func<T>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(T)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<T> f1 = e1.Compile();
					
					        Expression<Func<T, Func<T>>> e2 = Expression.Lambda<Func<T, Func<T>>>(
					            Expression.Lambda<Func<T>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<T, Func<T>> f2 = e2.Compile();
					
					        Expression<Func<Func<T, T>>> e3 = Expression.Lambda<Func<Func<T, T>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<T, T>>>(
					                    Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<T, T> f3 = e3.Compile()();
					
					        Expression<Func<Func<T, T>>> e4 = Expression.Lambda<Func<Func<T, T>>>(
					            Expression.Lambda<Func<T, T>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<T, T>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 342
				namespace Scenario342{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tc_Lambda_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tc_Lambda_object___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tc_Lambda<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tc_Lambda<Tc>() where Tc : class {
					        foreach (Tc val in new Tc[] { null, default(Tc) }) {
					            if (!check_Tc_Lambda<Tc>(val)) {
					                Console.WriteLine("Tc_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tc_Lambda<Tc>(Tc val) where Tc : class {
					        ParameterExpression p = Expression.Parameter(typeof(Tc), "p");
					        Expression<Func<Tc>> e1 = Expression.Lambda<Func<Tc>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Tc, Tc>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Tc)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc> f1 = e1.Compile();
					
					        Expression<Func<Tc, Func<Tc>>> e2 = Expression.Lambda<Func<Tc, Func<Tc>>>(
					            Expression.Lambda<Func<Tc>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Tc, Func<Tc>> f2 = e2.Compile();
					
					        Expression<Func<Func<Tc, Tc>>> e3 = Expression.Lambda<Func<Func<Tc, Tc>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Tc, Tc>>>(
					                    Expression.Lambda<Func<Tc, Tc>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc, Tc> f3 = e3.Compile()();
					
					        Expression<Func<Func<Tc, Tc>>> e4 = Expression.Lambda<Func<Func<Tc, Tc>>>(
					            Expression.Lambda<Func<Tc, Tc>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Tc, Tc>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 343
				namespace Scenario343{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tc_Lambda_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tc_Lambda_C___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tc_Lambda<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tc_Lambda<Tc>() where Tc : class {
					        foreach (Tc val in new Tc[] { null, default(Tc) }) {
					            if (!check_Tc_Lambda<Tc>(val)) {
					                Console.WriteLine("Tc_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tc_Lambda<Tc>(Tc val) where Tc : class {
					        ParameterExpression p = Expression.Parameter(typeof(Tc), "p");
					        Expression<Func<Tc>> e1 = Expression.Lambda<Func<Tc>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Tc, Tc>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Tc)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc> f1 = e1.Compile();
					
					        Expression<Func<Tc, Func<Tc>>> e2 = Expression.Lambda<Func<Tc, Func<Tc>>>(
					            Expression.Lambda<Func<Tc>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Tc, Func<Tc>> f2 = e2.Compile();
					
					        Expression<Func<Func<Tc, Tc>>> e3 = Expression.Lambda<Func<Func<Tc, Tc>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Tc, Tc>>>(
					                    Expression.Lambda<Func<Tc, Tc>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc, Tc> f3 = e3.Compile()();
					
					        Expression<Func<Func<Tc, Tc>>> e4 = Expression.Lambda<Func<Func<Tc, Tc>>>(
					            Expression.Lambda<Func<Tc, Tc>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Tc, Tc>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 344
				namespace Scenario344{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tcn_Lambda_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tcn_Lambda_object___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tcn_Lambda<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tcn_Lambda<Tcn>() where Tcn : class, new() {
					        foreach (Tcn val in new Tcn[] { null, default(Tcn), new Tcn() }) {
					            if (!check_Tcn_Lambda<Tcn>(val)) {
					                Console.WriteLine("Tcn_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tcn_Lambda<Tcn>(Tcn val) where Tcn : class, new() {
					        ParameterExpression p = Expression.Parameter(typeof(Tcn), "p");
					        Expression<Func<Tcn>> e1 = Expression.Lambda<Func<Tcn>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Tcn, Tcn>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Tcn)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn> f1 = e1.Compile();
					
					        Expression<Func<Tcn, Func<Tcn>>> e2 = Expression.Lambda<Func<Tcn, Func<Tcn>>>(
					            Expression.Lambda<Func<Tcn>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Tcn, Func<Tcn>> f2 = e2.Compile();
					
					        Expression<Func<Func<Tcn, Tcn>>> e3 = Expression.Lambda<Func<Func<Tcn, Tcn>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Tcn, Tcn>>>(
					                    Expression.Lambda<Func<Tcn, Tcn>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn, Tcn> f3 = e3.Compile()();
					
					        Expression<Func<Func<Tcn, Tcn>>> e4 = Expression.Lambda<Func<Func<Tcn, Tcn>>>(
					            Expression.Lambda<Func<Tcn, Tcn>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Tcn, Tcn>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 345
				namespace Scenario345{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tcn_Lambda_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tcn_Lambda_C___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tcn_Lambda<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tcn_Lambda<Tcn>() where Tcn : class, new() {
					        foreach (Tcn val in new Tcn[] { null, default(Tcn), new Tcn() }) {
					            if (!check_Tcn_Lambda<Tcn>(val)) {
					                Console.WriteLine("Tcn_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tcn_Lambda<Tcn>(Tcn val) where Tcn : class, new() {
					        ParameterExpression p = Expression.Parameter(typeof(Tcn), "p");
					        Expression<Func<Tcn>> e1 = Expression.Lambda<Func<Tcn>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Tcn, Tcn>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Tcn)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn> f1 = e1.Compile();
					
					        Expression<Func<Tcn, Func<Tcn>>> e2 = Expression.Lambda<Func<Tcn, Func<Tcn>>>(
					            Expression.Lambda<Func<Tcn>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Tcn, Func<Tcn>> f2 = e2.Compile();
					
					        Expression<Func<Func<Tcn, Tcn>>> e3 = Expression.Lambda<Func<Func<Tcn, Tcn>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Tcn, Tcn>>>(
					                    Expression.Lambda<Func<Tcn, Tcn>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn, Tcn> f3 = e3.Compile()();
					
					        Expression<Func<Func<Tcn, Tcn>>> e4 = Expression.Lambda<Func<Func<Tcn, Tcn>>>(
					            Expression.Lambda<Func<Tcn, Tcn>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Tcn, Tcn>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 346
				namespace Scenario346{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TC_Lambda_C_a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TC_Lambda_C_a__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TC_Lambda<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TC_Lambda<TC>() where TC : C {
					        foreach (TC val in new TC[] { null, default(TC), (TC) new C() }) {
					            if (!check_TC_Lambda<TC>(val)) {
					                Console.WriteLine("TC_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_TC_Lambda<TC>(TC val) where TC : C {
					        ParameterExpression p = Expression.Parameter(typeof(TC), "p");
					        Expression<Func<TC>> e1 = Expression.Lambda<Func<TC>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<TC, TC>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(TC)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<TC> f1 = e1.Compile();
					
					        Expression<Func<TC, Func<TC>>> e2 = Expression.Lambda<Func<TC, Func<TC>>>(
					            Expression.Lambda<Func<TC>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<TC, Func<TC>> f2 = e2.Compile();
					
					        Expression<Func<Func<TC, TC>>> e3 = Expression.Lambda<Func<Func<TC, TC>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<TC, TC>>>(
					                    Expression.Lambda<Func<TC, TC>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<TC, TC> f3 = e3.Compile()();
					
					        Expression<Func<Func<TC, TC>>> e4 = Expression.Lambda<Func<Func<TC, TC>>>(
					            Expression.Lambda<Func<TC, TC>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<TC, TC>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 347
				namespace Scenario347{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TCn_Lambda_C_a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TCn_Lambda_C_a__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TCn_Lambda<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TCn_Lambda<TCn>() where TCn : C, new() {
					        foreach (TCn val in new TCn[] { null, default(TCn), new TCn(), (TCn) new C() }) {
					            if (!check_TCn_Lambda<TCn>(val)) {
					                Console.WriteLine("TCn_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_TCn_Lambda<TCn>(TCn val) where TCn : C, new() {
					        ParameterExpression p = Expression.Parameter(typeof(TCn), "p");
					        Expression<Func<TCn>> e1 = Expression.Lambda<Func<TCn>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<TCn, TCn>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(TCn)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<TCn> f1 = e1.Compile();
					
					        Expression<Func<TCn, Func<TCn>>> e2 = Expression.Lambda<Func<TCn, Func<TCn>>>(
					            Expression.Lambda<Func<TCn>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<TCn, Func<TCn>> f2 = e2.Compile();
					
					        Expression<Func<Func<TCn, TCn>>> e3 = Expression.Lambda<Func<Func<TCn, TCn>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<TCn, TCn>>>(
					                    Expression.Lambda<Func<TCn, TCn>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<TCn, TCn> f3 = e3.Compile()();
					
					        Expression<Func<Func<TCn, TCn>>> e4 = Expression.Lambda<Func<Func<TCn, TCn>>>(
					            Expression.Lambda<Func<TCn, TCn>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<TCn, TCn>> f4 = e4.Compile();
					
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
			
			//-------- Scenario 348
			namespace Scenario348{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Delegate_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Delegate_Lambda__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_Delegate_Lambda();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_Delegate_Lambda() {
				        foreach (Delegate val in new Delegate[] { null, (Func<object>) delegate() { return null; }, (Func<int, int>) delegate(int i) { return i+1; }, (Action<object>) delegate { } }) {
				            if (!check_Delegate_Lambda(val)) {
				                Console.WriteLine("Delegate_Lambda failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_Delegate_Lambda(Delegate val) {
				        ParameterExpression p = Expression.Parameter(typeof(Delegate), "p");
				        Expression<Func<Delegate>> e1 = Expression.Lambda<Func<Delegate>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Delegate, Delegate>>(p, new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(Delegate)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Delegate> f1 = e1.Compile();
				
				        Expression<Func<Delegate, Func<Delegate>>> e2 = Expression.Lambda<Func<Delegate, Func<Delegate>>>(
				            Expression.Lambda<Func<Delegate>>(p, new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<Delegate, Func<Delegate>> f2 = e2.Compile();
				
				        Expression<Func<Func<Delegate, Delegate>>> e3 = Expression.Lambda<Func<Func<Delegate, Delegate>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<Delegate, Delegate>>>(
				                    Expression.Lambda<Func<Delegate, Delegate>>(p, new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Delegate, Delegate> f3 = e3.Compile()();
				
				        Expression<Func<Func<Delegate, Delegate>>> e4 = Expression.Lambda<Func<Func<Delegate, Delegate>>>(
				            Expression.Lambda<Func<Delegate, Delegate>>(p, new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<Delegate, Delegate>> f4 = e4.Compile();
				
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
				
				//-------- Scenario 349
				namespace Scenario349{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Func_object_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Func_object_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Func_object_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Func_object_Lambda() {
					        foreach (Func<object> val in new Func<object>[] { null, (Func<object>) delegate() { return null; } }) {
					            if (!check_Func_object_Lambda(val)) {
					                Console.WriteLine("Func_object_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Func_object_Lambda(Func<object> val) {
					        ParameterExpression p = Expression.Parameter(typeof(Func<object>), "p");
					        Expression<Func<Func<object>>> e1 = Expression.Lambda<Func<Func<object>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<object>, Func<object>>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(Func<object>)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<object>> f1 = e1.Compile();
					
					        Expression<Func<Func<object>, Func<Func<object>>>> e2 = Expression.Lambda<Func<Func<object>, Func<Func<object>>>>(
					            Expression.Lambda<Func<Func<object>>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<Func<object>, Func<Func<object>>> f2 = e2.Compile();
					
					        Expression<Func<Func<Func<object>, Func<object>>>> e3 = Expression.Lambda<Func<Func<Func<object>, Func<object>>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<Func<object>, Func<object>>>>(
					                    Expression.Lambda<Func<Func<object>, Func<object>>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<object>, Func<object>> f3 = e3.Compile()();
					
					        Expression<Func<Func<Func<object>, Func<object>>>> e4 = Expression.Lambda<Func<Func<Func<object>, Func<object>>>>(
					            Expression.Lambda<Func<Func<object>, Func<object>>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<Func<object>, Func<object>>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 350
				namespace Scenario350{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IEquatable_C_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression IEquatable_C_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_IEquatable_C_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_IEquatable_C_Lambda() {
					        foreach (IEquatable<C> val in new IEquatable<C>[] { null, new C(), new D(), new D(0), new D(5) }) {
					            if (!check_IEquatable_C_Lambda(val)) {
					                Console.WriteLine("IEquatable_C_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_IEquatable_C_Lambda(IEquatable<C> val) {
					        ParameterExpression p = Expression.Parameter(typeof(IEquatable<C>), "p");
					        Expression<Func<IEquatable<C>>> e1 = Expression.Lambda<Func<IEquatable<C>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<IEquatable<C>, IEquatable<C>>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(IEquatable<C>)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<C>> f1 = e1.Compile();
					
					        Expression<Func<IEquatable<C>, Func<IEquatable<C>>>> e2 = Expression.Lambda<Func<IEquatable<C>, Func<IEquatable<C>>>>(
					            Expression.Lambda<Func<IEquatable<C>>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<IEquatable<C>, Func<IEquatable<C>>> f2 = e2.Compile();
					
					        Expression<Func<Func<IEquatable<C>, IEquatable<C>>>> e3 = Expression.Lambda<Func<Func<IEquatable<C>, IEquatable<C>>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<IEquatable<C>, IEquatable<C>>>>(
					                    Expression.Lambda<Func<IEquatable<C>, IEquatable<C>>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<C>, IEquatable<C>> f3 = e3.Compile()();
					
					        Expression<Func<Func<IEquatable<C>, IEquatable<C>>>> e4 = Expression.Lambda<Func<Func<IEquatable<C>, IEquatable<C>>>>(
					            Expression.Lambda<Func<IEquatable<C>, IEquatable<C>>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<IEquatable<C>, IEquatable<C>>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 351
				namespace Scenario351{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IEquatable_D_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression IEquatable_D_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_IEquatable_D_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_IEquatable_D_Lambda() {
					        foreach (IEquatable<D> val in new IEquatable<D>[] { null, new D(), new D(0), new D(5) }) {
					            if (!check_IEquatable_D_Lambda(val)) {
					                Console.WriteLine("IEquatable_D_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_IEquatable_D_Lambda(IEquatable<D> val) {
					        ParameterExpression p = Expression.Parameter(typeof(IEquatable<D>), "p");
					        Expression<Func<IEquatable<D>>> e1 = Expression.Lambda<Func<IEquatable<D>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<IEquatable<D>, IEquatable<D>>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(IEquatable<D>)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<D>> f1 = e1.Compile();
					
					        Expression<Func<IEquatable<D>, Func<IEquatable<D>>>> e2 = Expression.Lambda<Func<IEquatable<D>, Func<IEquatable<D>>>>(
					            Expression.Lambda<Func<IEquatable<D>>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<IEquatable<D>, Func<IEquatable<D>>> f2 = e2.Compile();
					
					        Expression<Func<Func<IEquatable<D>, IEquatable<D>>>> e3 = Expression.Lambda<Func<Func<IEquatable<D>, IEquatable<D>>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<IEquatable<D>, IEquatable<D>>>>(
					                    Expression.Lambda<Func<IEquatable<D>, IEquatable<D>>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<D>, IEquatable<D>> f3 = e3.Compile()();
					
					        Expression<Func<Func<IEquatable<D>, IEquatable<D>>>> e4 = Expression.Lambda<Func<Func<IEquatable<D>, IEquatable<D>>>>(
					            Expression.Lambda<Func<IEquatable<D>, IEquatable<D>>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<IEquatable<D>, IEquatable<D>>> f4 = e4.Compile();
					
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
				
				//-------- Scenario 352
				namespace Scenario352{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "I_Lambda__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression I_Lambda__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_I_Lambda();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_I_Lambda() {
					        foreach (I val in new I[] { null, new C(), new D(), new D(0), new D(5) }) {
					            if (!check_I_Lambda(val)) {
					                Console.WriteLine("I_Lambda failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_I_Lambda(I val) {
					        ParameterExpression p = Expression.Parameter(typeof(I), "p");
					        Expression<Func<I>> e1 = Expression.Lambda<Func<I>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<I, I>>(p, new ParameterExpression[] { p }),
					                new Expression[] { Expression.Constant(val, typeof(I)) }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<I> f1 = e1.Compile();
					
					        Expression<Func<I, Func<I>>> e2 = Expression.Lambda<Func<I, Func<I>>>(
					            Expression.Lambda<Func<I>>(p, new System.Collections.Generic.List<ParameterExpression>()),
					            new ParameterExpression[] { p });
					        Func<I, Func<I>> f2 = e2.Compile();
					
					        Expression<Func<Func<I, I>>> e3 = Expression.Lambda<Func<Func<I, I>>>(
					            Expression.Invoke(
					                Expression.Lambda<Func<Func<I, I>>>(
					                    Expression.Lambda<Func<I, I>>(p, new ParameterExpression[] { p }),
					                    new System.Collections.Generic.List<ParameterExpression>()),
					                new System.Collections.Generic.List<Expression>()),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<I, I> f3 = e3.Compile()();
					
					        Expression<Func<Func<I, I>>> e4 = Expression.Lambda<Func<Func<I, I>>>(
					            Expression.Lambda<Func<I, I>>(p, new ParameterExpression[] { p }),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Func<I, I>> f4 = e4.Compile();
					
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
