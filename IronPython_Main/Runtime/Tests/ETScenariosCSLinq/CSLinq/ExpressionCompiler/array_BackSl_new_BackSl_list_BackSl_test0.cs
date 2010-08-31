#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 2259
			namespace Scenario2259{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byte_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byte_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_byte_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_NewArrayList() {
				        byte[][] vals = new byte[][] {
				            new byte[] {  },
				            new byte[] { 0 },
				            new byte[] { 0, 1, byte.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                byte val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(byte));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_byte_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("byte_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_NewArrayList(byte[] val, Expression[] exprs) {
				        Expression<Func<byte[]>> e =
				            Expression.Lambda<Func<byte[]>>(
				                Expression.NewArrayInit(typeof(byte), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte[]> f = e.Compile();
				        byte[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("byte_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("byte_NewArrayList failed");
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
			
			//-------- Scenario 2260
			namespace Scenario2260{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushort_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushort_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ushort_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushort_NewArrayList() {
				        ushort[][] vals = new ushort[][] {
				            new ushort[] {  },
				            new ushort[] { 0 },
				            new ushort[] { 0, 1, ushort.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                ushort val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(ushort));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_ushort_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("ushort_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_NewArrayList(ushort[] val, Expression[] exprs) {
				        Expression<Func<ushort[]>> e =
				            Expression.Lambda<Func<ushort[]>>(
				                Expression.NewArrayInit(typeof(ushort), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort[]> f = e.Compile();
				        ushort[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("ushort_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("ushort_NewArrayList failed");
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
			
			//-------- Scenario 2261
			namespace Scenario2261{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uint_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uint_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_uint_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uint_NewArrayList() {
				        uint[][] vals = new uint[][] {
				            new uint[] {  },
				            new uint[] { 0 },
				            new uint[] { 0, 1, uint.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                uint val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(uint));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_uint_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("uint_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_NewArrayList(uint[] val, Expression[] exprs) {
				        Expression<Func<uint[]>> e =
				            Expression.Lambda<Func<uint[]>>(
				                Expression.NewArrayInit(typeof(uint), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint[]> f = e.Compile();
				        uint[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("uint_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("uint_NewArrayList failed");
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
			
			//-------- Scenario 2262
			namespace Scenario2262{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulong_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulong_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_ulong_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulong_NewArrayList() {
				        ulong[][] vals = new ulong[][] {
				            new ulong[] {  },
				            new ulong[] { 0 },
				            new ulong[] { 0, 1, ulong.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                ulong val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(ulong));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_ulong_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("ulong_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_NewArrayList(ulong[] val, Expression[] exprs) {
				        Expression<Func<ulong[]>> e =
				            Expression.Lambda<Func<ulong[]>>(
				                Expression.NewArrayInit(typeof(ulong), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong[]> f = e.Compile();
				        ulong[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("ulong_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("ulong_NewArrayList failed");
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
			
			//-------- Scenario 2263
			namespace Scenario2263{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyte_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyte_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_sbyte_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyte_NewArrayList() {
				        sbyte[][] vals = new sbyte[][] {
				            new sbyte[] {  },
				            new sbyte[] { 0 },
				            new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                sbyte val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(sbyte));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_sbyte_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("sbyte_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_NewArrayList(sbyte[] val, Expression[] exprs) {
				        Expression<Func<sbyte[]>> e =
				            Expression.Lambda<Func<sbyte[]>>(
				                Expression.NewArrayInit(typeof(sbyte), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte[]> f = e.Compile();
				        sbyte[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("sbyte_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("sbyte_NewArrayList failed");
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
			
			//-------- Scenario 2264
			namespace Scenario2264{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "short_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression short_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_short_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_short_NewArrayList() {
				        short[][] vals = new short[][] {
				            new short[] {  },
				            new short[] { 0 },
				            new short[] { 0, 1, -1, short.MinValue, short.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                short val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(short));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_short_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("short_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_NewArrayList(short[] val, Expression[] exprs) {
				        Expression<Func<short[]>> e =
				            Expression.Lambda<Func<short[]>>(
				                Expression.NewArrayInit(typeof(short), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short[]> f = e.Compile();
				        short[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("short_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("short_NewArrayList failed");
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
			
			//-------- Scenario 2265
			namespace Scenario2265{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression int_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_int_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_int_NewArrayList() {
				        int[][] vals = new int[][] {
				            new int[] {  },
				            new int[] { 0 },
				            new int[] { 0, 1, -1, int.MinValue, int.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                int val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(int));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_int_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("int_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_NewArrayList(int[] val, Expression[] exprs) {
				        Expression<Func<int[]>> e =
				            Expression.Lambda<Func<int[]>>(
				                Expression.NewArrayInit(typeof(int), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int[]> f = e.Compile();
				        int[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("int_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("int_NewArrayList failed");
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
			
			//-------- Scenario 2266
			namespace Scenario2266{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "long_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression long_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_long_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_long_NewArrayList() {
				        long[][] vals = new long[][] {
				            new long[] {  },
				            new long[] { 0 },
				            new long[] { 0, 1, -1, long.MinValue, long.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                long val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(long));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_long_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("long_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_NewArrayList(long[] val, Expression[] exprs) {
				        Expression<Func<long[]>> e =
				            Expression.Lambda<Func<long[]>>(
				                Expression.NewArrayInit(typeof(long), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long[]> f = e.Compile();
				        long[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("long_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("long_NewArrayList failed");
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
			
			//-------- Scenario 2267
			namespace Scenario2267{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "float_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression float_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_float_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_float_NewArrayList() {
				        float[][] vals = new float[][] {
				            new float[] {  },
				            new float[] { 0 },
				            new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                float val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(float));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_float_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("float_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_NewArrayList(float[] val, Expression[] exprs) {
				        Expression<Func<float[]>> e =
				            Expression.Lambda<Func<float[]>>(
				                Expression.NewArrayInit(typeof(float), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float[]> f = e.Compile();
				        float[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("float_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("float_NewArrayList failed");
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
			
			//-------- Scenario 2268
			namespace Scenario2268{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "double_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression double_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_double_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_double_NewArrayList() {
				        double[][] vals = new double[][] {
				            new double[] {  },
				            new double[] { 0 },
				            new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                double val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(double));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_double_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("double_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_NewArrayList(double[] val, Expression[] exprs) {
				        Expression<Func<double[]>> e =
				            Expression.Lambda<Func<double[]>>(
				                Expression.NewArrayInit(typeof(double), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double[]> f = e.Compile();
				        double[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("double_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("double_NewArrayList failed");
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
			
			//-------- Scenario 2269
			namespace Scenario2269{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "decimal_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression decimal_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_decimal_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_decimal_NewArrayList() {
				        decimal[][] vals = new decimal[][] {
				            new decimal[] {  },
				            new decimal[] { decimal.Zero },
				            new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                decimal val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(decimal));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_decimal_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("decimal_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_NewArrayList(decimal[] val, Expression[] exprs) {
				        Expression<Func<decimal[]>> e =
				            Expression.Lambda<Func<decimal[]>>(
				                Expression.NewArrayInit(typeof(decimal), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal[]> f = e.Compile();
				        decimal[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("decimal_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("decimal_NewArrayList failed");
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
			
			//-------- Scenario 2270
			namespace Scenario2270{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "char_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression char_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_char_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_char_NewArrayList() {
				        char[][] vals = new char[][] {
				            new char[] {  },
				            new char[] { '\0' },
				            new char[] { '\0', '\b', 'A', '\uffff' }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                char val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(char));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_char_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("char_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_NewArrayList(char[] val, Expression[] exprs) {
				        Expression<Func<char[]>> e =
				            Expression.Lambda<Func<char[]>>(
				                Expression.NewArrayInit(typeof(char), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char[]> f = e.Compile();
				        char[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("char_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("char_NewArrayList failed");
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
			
			//-------- Scenario 2271
			namespace Scenario2271{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "bool_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression bool_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_bool_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_bool_NewArrayList() {
				        bool[][] vals = new bool[][] {
				            new bool[] {  },
				            new bool[] { true },
				            new bool[] { true, false }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                bool val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(bool));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_bool_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("bool_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_bool_NewArrayList(bool[] val, Expression[] exprs) {
				        Expression<Func<bool[]>> e =
				            Expression.Lambda<Func<bool[]>>(
				                Expression.NewArrayInit(typeof(bool), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool[]> f = e.Compile();
				        bool[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("bool_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("bool_NewArrayList failed");
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
				
				//-------- Scenario 2272
				namespace Scenario2272{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "S_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression S_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_S_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_S_NewArrayList() {
					        S[][] vals = new S[][] {
					            new S[] {  },
					            new S[] { default(S) },
					            new S[] { default(S), new S() }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                S val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(S));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_S_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("S_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_S_NewArrayList(S[] val, Expression[] exprs) {
					        Expression<Func<S[]>> e =
					            Expression.Lambda<Func<S[]>>(
					                Expression.NewArrayInit(typeof(S), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S[]> f = e.Compile();
					        S[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("S_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("S_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2273
				namespace Scenario2273{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Sp_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Sp_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Sp_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Sp_NewArrayList() {
					        Sp[][] vals = new Sp[][] {
					            new Sp[] {  },
					            new Sp[] { default(Sp) },
					            new Sp[] { default(Sp), new Sp(), new Sp(5,5.0) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Sp val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Sp));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Sp_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("Sp_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Sp_NewArrayList(Sp[] val, Expression[] exprs) {
					        Expression<Func<Sp[]>> e =
					            Expression.Lambda<Func<Sp[]>>(
					                Expression.NewArrayInit(typeof(Sp), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp[]> f = e.Compile();
					        Sp[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Sp_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Sp_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2274
				namespace Scenario2274{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ss_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ss_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ss_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ss_NewArrayList() {
					        Ss[][] vals = new Ss[][] {
					            new Ss[] {  },
					            new Ss[] { default(Ss) },
					            new Ss[] { default(Ss), new Ss(), new Ss(new S()) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Ss val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Ss));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Ss_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("Ss_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ss_NewArrayList(Ss[] val, Expression[] exprs) {
					        Expression<Func<Ss[]>> e =
					            Expression.Lambda<Func<Ss[]>>(
					                Expression.NewArrayInit(typeof(Ss), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss[]> f = e.Compile();
					        Ss[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Ss_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Ss_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2275
				namespace Scenario2275{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Sc_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Sc_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Sc_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Sc_NewArrayList() {
					        Sc[][] vals = new Sc[][] {
					            new Sc[] {  },
					            new Sc[] { default(Sc) },
					            new Sc[] { default(Sc), new Sc(), new Sc(null) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Sc val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Sc));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Sc_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("Sc_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Sc_NewArrayList(Sc[] val, Expression[] exprs) {
					        Expression<Func<Sc[]>> e =
					            Expression.Lambda<Func<Sc[]>>(
					                Expression.NewArrayInit(typeof(Sc), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc[]> f = e.Compile();
					        Sc[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Sc_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Sc_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2276
				namespace Scenario2276{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Scs_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Scs_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Scs_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Scs_NewArrayList() {
					        Scs[][] vals = new Scs[][] {
					            new Scs[] {  },
					            new Scs[] { default(Scs) },
					            new Scs[] { default(Scs), new Scs(), new Scs(null,new S()) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Scs val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Scs));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Scs_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("Scs_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Scs_NewArrayList(Scs[] val, Expression[] exprs) {
					        Expression<Func<Scs[]>> e =
					            Expression.Lambda<Func<Scs[]>>(
					                Expression.NewArrayInit(typeof(Scs), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs[]> f = e.Compile();
					        Scs[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Scs_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Scs_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2277
				namespace Scenario2277{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ts_NewArrayList_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ts_NewArrayList_S___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ts_NewArrayList<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ts_NewArrayList<Ts>() where Ts : struct {
					        Ts[][] vals = new Ts[][] {
					            new Ts[] {  },
					            new Ts[] { default(Ts) },
					            new Ts[] { default(Ts), new Ts() }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Ts val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Ts));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Ts_NewArrayList<Ts>(vals[i], exprs[i])) {
					                Console.WriteLine("Ts_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ts_NewArrayList<Ts>(Ts[] val, Expression[] exprs) where Ts : struct {
					        Expression<Func<Ts[]>> e =
					            Expression.Lambda<Func<Ts[]>>(
					                Expression.NewArrayInit(typeof(Ts), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts[]> f = e.Compile();
					        Ts[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Ts_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Ts_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2278
				namespace Scenario2278{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ts_NewArrayList_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ts_NewArrayList_Scs___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ts_NewArrayList<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ts_NewArrayList<Ts>() where Ts : struct {
					        Ts[][] vals = new Ts[][] {
					            new Ts[] {  },
					            new Ts[] { default(Ts) },
					            new Ts[] { default(Ts), new Ts() }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Ts val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Ts));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Ts_NewArrayList<Ts>(vals[i], exprs[i])) {
					                Console.WriteLine("Ts_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ts_NewArrayList<Ts>(Ts[] val, Expression[] exprs) where Ts : struct {
					        Expression<Func<Ts[]>> e =
					            Expression.Lambda<Func<Ts[]>>(
					                Expression.NewArrayInit(typeof(Ts), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts[]> f = e.Compile();
					        Ts[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Ts_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Ts_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2279
				namespace Scenario2279{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ts_NewArrayList_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ts_NewArrayList_E___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Ts_NewArrayList<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ts_NewArrayList<Ts>() where Ts : struct {
					        Ts[][] vals = new Ts[][] {
					            new Ts[] {  },
					            new Ts[] { default(Ts) },
					            new Ts[] { default(Ts), new Ts() }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Ts val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Ts));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Ts_NewArrayList<Ts>(vals[i], exprs[i])) {
					                Console.WriteLine("Ts_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ts_NewArrayList<Ts>(Ts[] val, Expression[] exprs) where Ts : struct {
					        Expression<Func<Ts[]>> e =
					            Expression.Lambda<Func<Ts[]>>(
					                Expression.NewArrayInit(typeof(Ts), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts[]> f = e.Compile();
					        Ts[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Ts_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Ts_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2280
				namespace Scenario2280{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "E_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression E_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_E_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_E_NewArrayList() {
					        E[][] vals = new E[][] {
					            new E[] {  },
					            new E[] { (E) 0 },
					            new E[] { (E) 0, E.A, E.B, (E) int.MaxValue, (E) int.MinValue }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                E val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(E));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_E_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("E_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_E_NewArrayList(E[] val, Expression[] exprs) {
					        Expression<Func<E[]>> e =
					            Expression.Lambda<Func<E[]>>(
					                Expression.NewArrayInit(typeof(E), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<E[]> f = e.Compile();
					        E[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("E_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("E_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2281
				namespace Scenario2281{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "El_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression El_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_El_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_El_NewArrayList() {
					        El[][] vals = new El[][] {
					            new El[] {  },
					            new El[] { (El) 0 },
					            new El[] { (El) 0, El.A, El.B, (El) long.MaxValue, (El) long.MinValue }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                El val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(El));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_El_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("El_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_El_NewArrayList(El[] val, Expression[] exprs) {
					        Expression<Func<El[]>> e =
					            Expression.Lambda<Func<El[]>>(
					                Expression.NewArrayInit(typeof(El), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<El[]> f = e.Compile();
					        El[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("El_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("El_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
			
			//-------- Scenario 2282
			namespace Scenario2282{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "string_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression string_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_string_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_string_NewArrayList() {
				        string[][] vals = new string[][] {
				            new string[] {  },
				            new string[] { null },
				            new string[] { null, "", "a", "foo" }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                string val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(string));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_string_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("string_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_string_NewArrayList(string[] val, Expression[] exprs) {
				        Expression<Func<string[]>> e =
				            Expression.Lambda<Func<string[]>>(
				                Expression.NewArrayInit(typeof(string), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<string[]> f = e.Compile();
				        string[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("string_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("string_NewArrayList failed");
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
				
				//-------- Scenario 2283
				namespace Scenario2283{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "object_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression object_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_object_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_object_NewArrayList() {
					        object[][] vals = new object[][] {
					            new object[] {  },
					            new object[] { null },
					            new object[] { null, new object(), new C(), new D(3) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                object val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(object));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_object_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("object_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_object_NewArrayList(object[] val, Expression[] exprs) {
					        Expression<Func<object[]>> e =
					            Expression.Lambda<Func<object[]>>(
					                Expression.NewArrayInit(typeof(object), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<object[]> f = e.Compile();
					        object[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("object_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("object_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2284
				namespace Scenario2284{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "C_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression C_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_C_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_C_NewArrayList() {
					        C[][] vals = new C[][] {
					            new C[] {  },
					            new C[] { null },
					            new C[] { null, new C(), new D(), new D(0), new D(5) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                C val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(C));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_C_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("C_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_C_NewArrayList(C[] val, Expression[] exprs) {
					        Expression<Func<C[]>> e =
					            Expression.Lambda<Func<C[]>>(
					                Expression.NewArrayInit(typeof(C), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<C[]> f = e.Compile();
					        C[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("C_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("C_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2285
				namespace Scenario2285{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "D_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression D_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_D_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_D_NewArrayList() {
					        D[][] vals = new D[][] {
					            new D[] {  },
					            new D[] { null },
					            new D[] { null, new D(), new D(0), new D(5) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                D val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(D));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_D_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("D_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_D_NewArrayList(D[] val, Expression[] exprs) {
					        Expression<Func<D[]>> e =
					            Expression.Lambda<Func<D[]>>(
					                Expression.NewArrayInit(typeof(D), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<D[]> f = e.Compile();
					        D[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("D_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("D_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2286
				namespace Scenario2286{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_NewArrayList_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_NewArrayList_object___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_T_NewArrayList<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_NewArrayList<T>() {
					        T[][] vals = new T[][] {
					            new T[] {  },
					            new T[] { default(T) },
					            new T[] { default(T) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                T val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(T));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_T_NewArrayList<T>(vals[i], exprs[i])) {
					                Console.WriteLine("T_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_T_NewArrayList<T>(T[] val, Expression[] exprs) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.NewArrayInit(typeof(T), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
					        T[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("T_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("T_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2287
				namespace Scenario2287{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_NewArrayList_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_NewArrayList_C___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_T_NewArrayList<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_NewArrayList<T>() {
					        T[][] vals = new T[][] {
					            new T[] {  },
					            new T[] { default(T) },
					            new T[] { default(T) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                T val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(T));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_T_NewArrayList<T>(vals[i], exprs[i])) {
					                Console.WriteLine("T_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_T_NewArrayList<T>(T[] val, Expression[] exprs) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.NewArrayInit(typeof(T), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
					        T[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("T_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("T_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2288
				namespace Scenario2288{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_NewArrayList_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_NewArrayList_S___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_T_NewArrayList<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_NewArrayList<T>() {
					        T[][] vals = new T[][] {
					            new T[] {  },
					            new T[] { default(T) },
					            new T[] { default(T) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                T val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(T));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_T_NewArrayList<T>(vals[i], exprs[i])) {
					                Console.WriteLine("T_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_T_NewArrayList<T>(T[] val, Expression[] exprs) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.NewArrayInit(typeof(T), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
					        T[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("T_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("T_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2289
				namespace Scenario2289{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_NewArrayList_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_NewArrayList_Scs___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_T_NewArrayList<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_NewArrayList<T>() {
					        T[][] vals = new T[][] {
					            new T[] {  },
					            new T[] { default(T) },
					            new T[] { default(T) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                T val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(T));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_T_NewArrayList<T>(vals[i], exprs[i])) {
					                Console.WriteLine("T_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_T_NewArrayList<T>(T[] val, Expression[] exprs) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.NewArrayInit(typeof(T), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
					        T[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("T_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("T_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2290
				namespace Scenario2290{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "T_NewArrayList_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression T_NewArrayList_E___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_T_NewArrayList<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_T_NewArrayList<T>() {
					        T[][] vals = new T[][] {
					            new T[] {  },
					            new T[] { default(T) },
					            new T[] { default(T) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                T val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(T));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_T_NewArrayList<T>(vals[i], exprs[i])) {
					                Console.WriteLine("T_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_T_NewArrayList<T>(T[] val, Expression[] exprs) {
					        Expression<Func<T[]>> e =
					            Expression.Lambda<Func<T[]>>(
					                Expression.NewArrayInit(typeof(T), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<T[]> f = e.Compile();
					        T[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("T_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("T_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2291
				namespace Scenario2291{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tc_NewArrayList_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tc_NewArrayList_object___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tc_NewArrayList<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tc_NewArrayList<Tc>() where Tc : class {
					        Tc[][] vals = new Tc[][] {
					            new Tc[] {  },
					            new Tc[] { null },
					            new Tc[] { null, default(Tc) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Tc val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Tc));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Tc_NewArrayList<Tc>(vals[i], exprs[i])) {
					                Console.WriteLine("Tc_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tc_NewArrayList<Tc>(Tc[] val, Expression[] exprs) where Tc : class {
					        Expression<Func<Tc[]>> e =
					            Expression.Lambda<Func<Tc[]>>(
					                Expression.NewArrayInit(typeof(Tc), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc[]> f = e.Compile();
					        Tc[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Tc_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Tc_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2292
				namespace Scenario2292{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tc_NewArrayList_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tc_NewArrayList_C___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tc_NewArrayList<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tc_NewArrayList<Tc>() where Tc : class {
					        Tc[][] vals = new Tc[][] {
					            new Tc[] {  },
					            new Tc[] { null },
					            new Tc[] { null, default(Tc) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Tc val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Tc));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Tc_NewArrayList<Tc>(vals[i], exprs[i])) {
					                Console.WriteLine("Tc_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tc_NewArrayList<Tc>(Tc[] val, Expression[] exprs) where Tc : class {
					        Expression<Func<Tc[]>> e =
					            Expression.Lambda<Func<Tc[]>>(
					                Expression.NewArrayInit(typeof(Tc), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tc[]> f = e.Compile();
					        Tc[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Tc_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Tc_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2293
				namespace Scenario2293{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tcn_NewArrayList_object___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tcn_NewArrayList_object___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tcn_NewArrayList<object>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tcn_NewArrayList<Tcn>() where Tcn : class, new() {
					        Tcn[][] vals = new Tcn[][] {
					            new Tcn[] {  },
					            new Tcn[] { null },
					            new Tcn[] { null, default(Tcn), new Tcn() }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Tcn val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Tcn));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Tcn_NewArrayList<Tcn>(vals[i], exprs[i])) {
					                Console.WriteLine("Tcn_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tcn_NewArrayList<Tcn>(Tcn[] val, Expression[] exprs) where Tcn : class, new() {
					        Expression<Func<Tcn[]>> e =
					            Expression.Lambda<Func<Tcn[]>>(
					                Expression.NewArrayInit(typeof(Tcn), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn[]> f = e.Compile();
					        Tcn[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Tcn_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Tcn_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2294
				namespace Scenario2294{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tcn_NewArrayList_C___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tcn_NewArrayList_C___() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_Tcn_NewArrayList<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tcn_NewArrayList<Tcn>() where Tcn : class, new() {
					        Tcn[][] vals = new Tcn[][] {
					            new Tcn[] {  },
					            new Tcn[] { null },
					            new Tcn[] { null, default(Tcn), new Tcn() }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Tcn val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Tcn));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Tcn_NewArrayList<Tcn>(vals[i], exprs[i])) {
					                Console.WriteLine("Tcn_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tcn_NewArrayList<Tcn>(Tcn[] val, Expression[] exprs) where Tcn : class, new() {
					        Expression<Func<Tcn[]>> e =
					            Expression.Lambda<Func<Tcn[]>>(
					                Expression.NewArrayInit(typeof(Tcn), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Tcn[]> f = e.Compile();
					        Tcn[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Tcn_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Tcn_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2295
				namespace Scenario2295{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TC_NewArrayList_C_a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TC_NewArrayList_C_a__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TC_NewArrayList<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TC_NewArrayList<TC>() where TC : C {
					        TC[][] vals = new TC[][] {
					            new TC[] {  },
					            new TC[] { null },
					            new TC[] { null, default(TC), (TC) new C() }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                TC val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(TC));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_TC_NewArrayList<TC>(vals[i], exprs[i])) {
					                Console.WriteLine("TC_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_TC_NewArrayList<TC>(TC[] val, Expression[] exprs) where TC : C {
					        Expression<Func<TC[]>> e =
					            Expression.Lambda<Func<TC[]>>(
					                Expression.NewArrayInit(typeof(TC), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<TC[]> f = e.Compile();
					        TC[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("TC_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("TC_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2296
				namespace Scenario2296{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "TCn_NewArrayList_C_a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression TCn_NewArrayList_C_a__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_TCn_NewArrayList<C>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_TCn_NewArrayList<TCn>() where TCn : C, new() {
					        TCn[][] vals = new TCn[][] {
					            new TCn[] {  },
					            new TCn[] { null },
					            new TCn[] { null, default(TCn), new TCn(), (TCn) new C() }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                TCn val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(TCn));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_TCn_NewArrayList<TCn>(vals[i], exprs[i])) {
					                Console.WriteLine("TCn_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_TCn_NewArrayList<TCn>(TCn[] val, Expression[] exprs) where TCn : C, new() {
					        Expression<Func<TCn[]>> e =
					            Expression.Lambda<Func<TCn[]>>(
					                Expression.NewArrayInit(typeof(TCn), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<TCn[]> f = e.Compile();
					        TCn[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("TCn_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("TCn_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
			
			//-------- Scenario 2297
			namespace Scenario2297{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Delegate_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Delegate_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_Delegate_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_Delegate_NewArrayList() {
				        Delegate[][] vals = new Delegate[][] {
				            new Delegate[] {  },
				            new Delegate[] { null },
				            new Delegate[] { null, (Func<object>) delegate() { return null; }, (Func<int, int>) delegate(int i) { return i+1; }, (Action<object>) delegate { } }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                Delegate val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(Delegate));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_Delegate_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("Delegate_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_Delegate_NewArrayList(Delegate[] val, Expression[] exprs) {
				        Expression<Func<Delegate[]>> e =
				            Expression.Lambda<Func<Delegate[]>>(
				                Expression.NewArrayInit(typeof(Delegate), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<Delegate[]> f = e.Compile();
				        Delegate[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("Delegate_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("Delegate_NewArrayList failed");
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
			
			//-------- Scenario 2298
			namespace Scenario2298{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Func_object_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Func_object_NewArrayList__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public     static int Main()
				    {
				        Ext.StartCapture();
				        bool success = false;
				        try
				        {
				            success = check_Func_object_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_Func_object_NewArrayList() {
				        Func<object>[][] vals = new Func<object>[][] {
				            new Func<object>[] {  },
				            new Func<object>[] { null },
				            new Func<object>[] { null, (Func<object>) delegate() { return null; } }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                Func<object> val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(Func<object>));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_Func_object_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("Func_object_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_Func_object_NewArrayList(Func<object>[] val, Expression[] exprs) {
				        Expression<Func<Func<object>[]>> e =
				            Expression.Lambda<Func<Func<object>[]>>(
				                Expression.NewArrayInit(typeof(Func<object>), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<Func<object>[]> f = e.Compile();
				        Func<object>[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("Func_object_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("Func_object_NewArrayList failed");
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
				
				//-------- Scenario 2299
				namespace Scenario2299{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IEquatable_C_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression IEquatable_C_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_IEquatable_C_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_IEquatable_C_NewArrayList() {
					        IEquatable<C>[][] vals = new IEquatable<C>[][] {
					            new IEquatable<C>[] {  },
					            new IEquatable<C>[] { null },
					            new IEquatable<C>[] { null, new C(), new D(), new D(0), new D(5) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                IEquatable<C> val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(IEquatable<C>));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_IEquatable_C_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("IEquatable_C_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_IEquatable_C_NewArrayList(IEquatable<C>[] val, Expression[] exprs) {
					        Expression<Func<IEquatable<C>[]>> e =
					            Expression.Lambda<Func<IEquatable<C>[]>>(
					                Expression.NewArrayInit(typeof(IEquatable<C>), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<C>[]> f = e.Compile();
					        IEquatable<C>[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("IEquatable_C_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("IEquatable_C_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2300
				namespace Scenario2300{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "IEquatable_D_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression IEquatable_D_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_IEquatable_D_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_IEquatable_D_NewArrayList() {
					        IEquatable<D>[][] vals = new IEquatable<D>[][] {
					            new IEquatable<D>[] {  },
					            new IEquatable<D>[] { null },
					            new IEquatable<D>[] { null, new D(), new D(0), new D(5) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                IEquatable<D> val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(IEquatable<D>));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_IEquatable_D_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("IEquatable_D_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_IEquatable_D_NewArrayList(IEquatable<D>[] val, Expression[] exprs) {
					        Expression<Func<IEquatable<D>[]>> e =
					            Expression.Lambda<Func<IEquatable<D>[]>>(
					                Expression.NewArrayInit(typeof(IEquatable<D>), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<IEquatable<D>[]> f = e.Compile();
					        IEquatable<D>[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("IEquatable_D_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("IEquatable_D_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
				
				//-------- Scenario 2301
				namespace Scenario2301{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "I_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression I_NewArrayList__() {
				       if(Main() != 0 ) {
				           throw new Exception();
				       } else { 
				           return Expression.Constant(0);
				       }
				    }
					public     static int Main()
					    {
					        Ext.StartCapture();
					        bool success = false;
					        try
					        {
					            success = check_I_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_I_NewArrayList() {
					        I[][] vals = new I[][] {
					            new I[] {  },
					            new I[] { null },
					            new I[] { null, new C(), new D(), new D(0), new D(5) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                I val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(I));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_I_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("I_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_I_NewArrayList(I[] val, Expression[] exprs) {
					        Expression<Func<I[]>> e =
					            Expression.Lambda<Func<I[]>>(
					                Expression.NewArrayInit(typeof(I), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<I[]> f = e.Compile();
					        I[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("I_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("I_NewArrayList failed");
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
				
			
			
			
			public interface I
			{
			  void M();
			}
			
			public  class C : IEquatable<C>, I
			{
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
			
			public  class D : C, IEquatable<D>
			{
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
			
			public enum E
			{
			  A=1, B=2
			}
			
			public enum El : long
			{
			  A, B, C
			}
			
			public struct S : IEquatable<S>
			{
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
			
			public struct Sp : IEquatable<Sp>
			{
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
			
			public struct Ss : IEquatable<Ss>
			{
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
			
			public struct Sc : IEquatable<Sc>
			{
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
