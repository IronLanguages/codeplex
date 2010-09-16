#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 294
			namespace Scenario294{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_Not__1", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_Not__1() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_Not();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_Not() {
				        foreach (byte val in new byte[] { 0, 1, byte.MaxValue }) {
				            if (!check_byte_Not(val)) {
				                Console.WriteLine("byte_Not failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_Not(byte val) {
				        ParameterExpression p = Expression.Parameter(typeof(byte), "p");
				        Expression<Func<byte>> e1 = Expression.Lambda<Func<byte>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<byte, byte>>(
				                    Expression.Not(p),
				                    new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(byte)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte> f1 = e1.Compile();
				
				        Expression<Func<byte, Func<byte>>> e2 = Expression.Lambda<Func<byte, Func<byte>>>(
				            Expression.Lambda<Func<byte>>(Expression.Not(p), new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<byte, Func<byte>> f2 = e2.Compile();
				
				        Expression<Func<Func<byte, byte>>> e3 = Expression.Lambda<Func<Func<byte, byte>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<byte, byte>>>(
				                    Expression.Lambda<Func<byte, byte>>(
				                        Expression.Not(p),
				                        new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte, byte> f3 = e3.Compile()();
				
				        Expression<Func<Func<byte, byte>>> e4 = Expression.Lambda<Func<Func<byte, byte>>>(
				            Expression.Lambda<Func<byte, byte>>(
				                Expression.Not(p),
				                    new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<byte, byte>> f4 = e4.Compile();
				
				        byte expected = (byte) ~val;
				
				        return object.Equals(f1(), expected) &&
				            object.Equals(f2(val)(), expected) &&
				            object.Equals(f3(val), expected) &&
				            object.Equals(f4()(val), expected);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 295
			namespace Scenario295{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_Not__1", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_Not__1() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_Not();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_Not() {
				        foreach (sbyte val in new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }) {
				            if (!check_sbyte_Not(val)) {
				                Console.WriteLine("sbyte_Not failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_Not(sbyte val) {
				        ParameterExpression p = Expression.Parameter(typeof(sbyte), "p");
				        Expression<Func<sbyte>> e1 = Expression.Lambda<Func<sbyte>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<sbyte, sbyte>>(
				                    Expression.Not(p),
				                    new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(sbyte)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte> f1 = e1.Compile();
				
				        Expression<Func<sbyte, Func<sbyte>>> e2 = Expression.Lambda<Func<sbyte, Func<sbyte>>>(
				            Expression.Lambda<Func<sbyte>>(Expression.Not(p), new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<sbyte, Func<sbyte>> f2 = e2.Compile();
				
				        Expression<Func<Func<sbyte, sbyte>>> e3 = Expression.Lambda<Func<Func<sbyte, sbyte>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<sbyte, sbyte>>>(
				                    Expression.Lambda<Func<sbyte, sbyte>>(
				                        Expression.Not(p),
				                        new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte, sbyte> f3 = e3.Compile()();
				
				        Expression<Func<Func<sbyte, sbyte>>> e4 = Expression.Lambda<Func<Func<sbyte, sbyte>>>(
				            Expression.Lambda<Func<sbyte, sbyte>>(
				                Expression.Not(p),
				                    new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<sbyte, sbyte>> f4 = e4.Compile();
				
				        sbyte expected = (sbyte) ~val;
				
				        return object.Equals(f1(), expected) &&
				            object.Equals(f2(val)(), expected) &&
				            object.Equals(f3(val), expected) &&
				            object.Equals(f4()(val), expected);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 296
			namespace Scenario296{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_Not__1", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_Not__1() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_Not();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_Not() {
				        foreach (ushort val in new ushort[] { 0, 1, ushort.MaxValue }) {
				            if (!check_ushort_Not(val)) {
				                Console.WriteLine("ushort_Not failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_Not(ushort val) {
				        ParameterExpression p = Expression.Parameter(typeof(ushort), "p");
				        Expression<Func<ushort>> e1 = Expression.Lambda<Func<ushort>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<ushort, ushort>>(
				                    Expression.Not(p),
				                    new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(ushort)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort> f1 = e1.Compile();
				
				        Expression<Func<ushort, Func<ushort>>> e2 = Expression.Lambda<Func<ushort, Func<ushort>>>(
				            Expression.Lambda<Func<ushort>>(Expression.Not(p), new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<ushort, Func<ushort>> f2 = e2.Compile();
				
				        Expression<Func<Func<ushort, ushort>>> e3 = Expression.Lambda<Func<Func<ushort, ushort>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<ushort, ushort>>>(
				                    Expression.Lambda<Func<ushort, ushort>>(
				                        Expression.Not(p),
				                        new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort, ushort> f3 = e3.Compile()();
				
				        Expression<Func<Func<ushort, ushort>>> e4 = Expression.Lambda<Func<Func<ushort, ushort>>>(
				            Expression.Lambda<Func<ushort, ushort>>(
				                Expression.Not(p),
				                    new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<ushort, ushort>> f4 = e4.Compile();
				
				        ushort expected = (ushort) ~val;
				
				        return object.Equals(f1(), expected) &&
				            object.Equals(f2(val)(), expected) &&
				            object.Equals(f3(val), expected) &&
				            object.Equals(f4()(val), expected);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 297
			namespace Scenario297{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_Not__1", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_Not__1() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_Not();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_Not() {
				        foreach (short val in new short[] { 0, 1, -1, short.MinValue, short.MaxValue }) {
				            if (!check_short_Not(val)) {
				                Console.WriteLine("short_Not failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_Not(short val) {
				        ParameterExpression p = Expression.Parameter(typeof(short), "p");
				        Expression<Func<short>> e1 = Expression.Lambda<Func<short>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<short, short>>(
				                    Expression.Not(p),
				                    new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(short)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<short> f1 = e1.Compile();
				
				        Expression<Func<short, Func<short>>> e2 = Expression.Lambda<Func<short, Func<short>>>(
				            Expression.Lambda<Func<short>>(Expression.Not(p), new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<short, Func<short>> f2 = e2.Compile();
				
				        Expression<Func<Func<short, short>>> e3 = Expression.Lambda<Func<Func<short, short>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<short, short>>>(
				                    Expression.Lambda<Func<short, short>>(
				                        Expression.Not(p),
				                        new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<short, short> f3 = e3.Compile()();
				
				        Expression<Func<Func<short, short>>> e4 = Expression.Lambda<Func<Func<short, short>>>(
				            Expression.Lambda<Func<short, short>>(
				                Expression.Not(p),
				                    new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<short, short>> f4 = e4.Compile();
				
				        short expected = (short) ~val;
				
				        return object.Equals(f1(), expected) &&
				            object.Equals(f2(val)(), expected) &&
				            object.Equals(f3(val), expected) &&
				            object.Equals(f4()(val), expected);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 298
			namespace Scenario298{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_Not__1", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_Not__1() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_Not();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_Not() {
				        foreach (uint val in new uint[] { 0, 1, uint.MaxValue }) {
				            if (!check_uint_Not(val)) {
				                Console.WriteLine("uint_Not failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_Not(uint val) {
				        ParameterExpression p = Expression.Parameter(typeof(uint), "p");
				        Expression<Func<uint>> e1 = Expression.Lambda<Func<uint>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<uint, uint>>(
				                    Expression.Not(p),
				                    new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(uint)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint> f1 = e1.Compile();
				
				        Expression<Func<uint, Func<uint>>> e2 = Expression.Lambda<Func<uint, Func<uint>>>(
				            Expression.Lambda<Func<uint>>(Expression.Not(p), new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<uint, Func<uint>> f2 = e2.Compile();
				
				        Expression<Func<Func<uint, uint>>> e3 = Expression.Lambda<Func<Func<uint, uint>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<uint, uint>>>(
				                    Expression.Lambda<Func<uint, uint>>(
				                        Expression.Not(p),
				                        new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint, uint> f3 = e3.Compile()();
				
				        Expression<Func<Func<uint, uint>>> e4 = Expression.Lambda<Func<Func<uint, uint>>>(
				            Expression.Lambda<Func<uint, uint>>(
				                Expression.Not(p),
				                    new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<uint, uint>> f4 = e4.Compile();
				
				        uint expected = (uint) ~val;
				
				        return object.Equals(f1(), expected) &&
				            object.Equals(f2(val)(), expected) &&
				            object.Equals(f3(val), expected) &&
				            object.Equals(f4()(val), expected);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 299
			namespace Scenario299{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_Not__1", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_Not__1() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_Not();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_Not() {
				        foreach (int val in new int[] { 0, 1, -1, int.MinValue, int.MaxValue }) {
				            if (!check_int_Not(val)) {
				                Console.WriteLine("int_Not failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_Not(int val) {
				        ParameterExpression p = Expression.Parameter(typeof(int), "p");
				        Expression<Func<int>> e1 = Expression.Lambda<Func<int>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<int, int>>(
				                    Expression.Not(p),
				                    new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(int)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<int> f1 = e1.Compile();
				
				        Expression<Func<int, Func<int>>> e2 = Expression.Lambda<Func<int, Func<int>>>(
				            Expression.Lambda<Func<int>>(Expression.Not(p), new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<int, Func<int>> f2 = e2.Compile();
				
				        Expression<Func<Func<int, int>>> e3 = Expression.Lambda<Func<Func<int, int>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<int, int>>>(
				                    Expression.Lambda<Func<int, int>>(
				                        Expression.Not(p),
				                        new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<int, int> f3 = e3.Compile()();
				
				        Expression<Func<Func<int, int>>> e4 = Expression.Lambda<Func<Func<int, int>>>(
				            Expression.Lambda<Func<int, int>>(
				                Expression.Not(p),
				                    new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<int, int>> f4 = e4.Compile();
				
				        int expected = (int) ~val;
				
				        return object.Equals(f1(), expected) &&
				            object.Equals(f2(val)(), expected) &&
				            object.Equals(f3(val), expected) &&
				            object.Equals(f4()(val), expected);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 300
			namespace Scenario300{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_Not__1", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_Not__1() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_Not();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_Not() {
				        foreach (ulong val in new ulong[] { 0, 1, ulong.MaxValue }) {
				            if (!check_ulong_Not(val)) {
				                Console.WriteLine("ulong_Not failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_Not(ulong val) {
				        ParameterExpression p = Expression.Parameter(typeof(ulong), "p");
				        Expression<Func<ulong>> e1 = Expression.Lambda<Func<ulong>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<ulong, ulong>>(
				                    Expression.Not(p),
				                    new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(ulong)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong> f1 = e1.Compile();
				
				        Expression<Func<ulong, Func<ulong>>> e2 = Expression.Lambda<Func<ulong, Func<ulong>>>(
				            Expression.Lambda<Func<ulong>>(Expression.Not(p), new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<ulong, Func<ulong>> f2 = e2.Compile();
				
				        Expression<Func<Func<ulong, ulong>>> e3 = Expression.Lambda<Func<Func<ulong, ulong>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<ulong, ulong>>>(
				                    Expression.Lambda<Func<ulong, ulong>>(
				                        Expression.Not(p),
				                        new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong, ulong> f3 = e3.Compile()();
				
				        Expression<Func<Func<ulong, ulong>>> e4 = Expression.Lambda<Func<Func<ulong, ulong>>>(
				            Expression.Lambda<Func<ulong, ulong>>(
				                Expression.Not(p),
				                    new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<ulong, ulong>> f4 = e4.Compile();
				
				        ulong expected = (ulong) ~val;
				
				        return object.Equals(f1(), expected) &&
				            object.Equals(f2(val)(), expected) &&
				            object.Equals(f3(val), expected) &&
				            object.Equals(f4()(val), expected);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static Type GetNonNullableType(Type type) {
			        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
			                type.GetGenericArguments()[0] :
			                type;
			    }
			}
			
		
	}
			
			//-------- Scenario 301
			namespace Scenario301{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_Not__1", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_Not__1() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_Not();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_Not() {
				        foreach (long val in new long[] { 0, 1, -1, long.MinValue, long.MaxValue }) {
				            if (!check_long_Not(val)) {
				                Console.WriteLine("long_Not failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_Not(long val) {
				        ParameterExpression p = Expression.Parameter(typeof(long), "p");
				        Expression<Func<long>> e1 = Expression.Lambda<Func<long>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<long, long>>(
				                    Expression.Not(p),
				                    new ParameterExpression[] { p }),
				                new Expression[] { Expression.Constant(val, typeof(long)) }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<long> f1 = e1.Compile();
				
				        Expression<Func<long, Func<long>>> e2 = Expression.Lambda<Func<long, Func<long>>>(
				            Expression.Lambda<Func<long>>(Expression.Not(p), new System.Collections.Generic.List<ParameterExpression>()),
				            new ParameterExpression[] { p });
				        Func<long, Func<long>> f2 = e2.Compile();
				
				        Expression<Func<Func<long, long>>> e3 = Expression.Lambda<Func<Func<long, long>>>(
				            Expression.Invoke(
				                Expression.Lambda<Func<Func<long, long>>>(
				                    Expression.Lambda<Func<long, long>>(
				                        Expression.Not(p),
				                        new ParameterExpression[] { p }),
				                    new System.Collections.Generic.List<ParameterExpression>()),
				                new System.Collections.Generic.List<Expression>()),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<long, long> f3 = e3.Compile()();
				
				        Expression<Func<Func<long, long>>> e4 = Expression.Lambda<Func<Func<long, long>>>(
				            Expression.Lambda<Func<long, long>>(
				                Expression.Not(p),
				                    new ParameterExpression[] { p }),
				            new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<long, long>> f4 = e4.Compile();
				
				        long expected = (long) ~val;
				
				        return object.Equals(f1(), expected) &&
				            object.Equals(f2(val)(), expected) &&
				            object.Equals(f3(val), expected) &&
				            object.Equals(f4()(val), expected);
				    }
				}
			
			
			public  static class Ext {
			    public static void StartCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StartCaptureToFile", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[] { "test.dll" });
			    }
			
			    public static void StopCapture() {
			//        MethodInfo m = Assembly.GetAssembly(typeof(Expression)).GetType("System.Linq.Expressions.ExpressionCompiler").GetMethod("StopCapture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			//        m.Invoke(null, new object[0]);
			    }
			
			    public static bool IsIntegralOrEnum(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Byte:
			            case TypeCode.SByte:
			            case TypeCode.Int16:
			            case TypeCode.Int32:
			            case TypeCode.Int64:
			            case TypeCode.UInt16:
			            case TypeCode.UInt32:
			            case TypeCode.UInt64:
			                return true;
			            default:
			                return false;
			        }
			    }
			
			    public static bool IsFloating(Type type) {
			        switch (Type.GetTypeCode(GetNonNullableType(type))) {
			            case TypeCode.Single:
			            case TypeCode.Double:
			                return true;
			            default:
			                return false;
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