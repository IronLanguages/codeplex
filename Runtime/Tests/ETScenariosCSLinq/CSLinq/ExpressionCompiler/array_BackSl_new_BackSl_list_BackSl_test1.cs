#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 2302
			namespace Scenario2302{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "byteq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression byteq_NewArrayList__() {
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
				            success = check_byteq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byteq_NewArrayList() {
				        byte?[][] vals = new byte?[][] {
				            new byte?[] {  },
				            new byte?[] { 0 },
				            new byte?[] { 0, 1, byte.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                byte? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(byte?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_byteq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("byteq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_byteq_NewArrayList(byte?[] val, Expression[] exprs) {
				        Expression<Func<byte?[]>> e =
				            Expression.Lambda<Func<byte?[]>>(
				                Expression.NewArrayInit(typeof(byte?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<byte?[]> f = e.Compile();
				        byte?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("byteq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("byteq_NewArrayList failed");
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
			
			//-------- Scenario 2303
			namespace Scenario2303{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ushortq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ushortq_NewArrayList__() {
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
				            success = check_ushortq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ushortq_NewArrayList() {
				        ushort?[][] vals = new ushort?[][] {
				            new ushort?[] {  },
				            new ushort?[] { 0 },
				            new ushort?[] { 0, 1, ushort.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                ushort? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(ushort?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_ushortq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("ushortq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushortq_NewArrayList(ushort?[] val, Expression[] exprs) {
				        Expression<Func<ushort?[]>> e =
				            Expression.Lambda<Func<ushort?[]>>(
				                Expression.NewArrayInit(typeof(ushort?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ushort?[]> f = e.Compile();
				        ushort?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("ushortq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("ushortq_NewArrayList failed");
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
			
			//-------- Scenario 2304
			namespace Scenario2304{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "uintq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression uintq_NewArrayList__() {
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
				            success = check_uintq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_uintq_NewArrayList() {
				        uint?[][] vals = new uint?[][] {
				            new uint?[] {  },
				            new uint?[] { 0 },
				            new uint?[] { 0, 1, uint.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                uint? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(uint?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_uintq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("uintq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_uintq_NewArrayList(uint?[] val, Expression[] exprs) {
				        Expression<Func<uint?[]>> e =
				            Expression.Lambda<Func<uint?[]>>(
				                Expression.NewArrayInit(typeof(uint?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<uint?[]> f = e.Compile();
				        uint?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("uintq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("uintq_NewArrayList failed");
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
			
			//-------- Scenario 2305
			namespace Scenario2305{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "ulongq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression ulongq_NewArrayList__() {
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
				            success = check_ulongq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_ulongq_NewArrayList() {
				        ulong?[][] vals = new ulong?[][] {
				            new ulong?[] {  },
				            new ulong?[] { 0 },
				            new ulong?[] { 0, 1, ulong.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                ulong? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(ulong?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_ulongq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("ulongq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulongq_NewArrayList(ulong?[] val, Expression[] exprs) {
				        Expression<Func<ulong?[]>> e =
				            Expression.Lambda<Func<ulong?[]>>(
				                Expression.NewArrayInit(typeof(ulong?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<ulong?[]> f = e.Compile();
				        ulong?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("ulongq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("ulongq_NewArrayList failed");
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
			
			//-------- Scenario 2306
			namespace Scenario2306{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "sbyteq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression sbyteq_NewArrayList__() {
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
				            success = check_sbyteq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_sbyteq_NewArrayList() {
				        sbyte?[][] vals = new sbyte?[][] {
				            new sbyte?[] {  },
				            new sbyte?[] { 0 },
				            new sbyte?[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                sbyte? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(sbyte?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_sbyteq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("sbyteq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyteq_NewArrayList(sbyte?[] val, Expression[] exprs) {
				        Expression<Func<sbyte?[]>> e =
				            Expression.Lambda<Func<sbyte?[]>>(
				                Expression.NewArrayInit(typeof(sbyte?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<sbyte?[]> f = e.Compile();
				        sbyte?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("sbyteq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("sbyteq_NewArrayList failed");
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
			
			//-------- Scenario 2307
			namespace Scenario2307{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "shortq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression shortq_NewArrayList__() {
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
				            success = check_shortq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_shortq_NewArrayList() {
				        short?[][] vals = new short?[][] {
				            new short?[] {  },
				            new short?[] { 0 },
				            new short?[] { 0, 1, -1, short.MinValue, short.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                short? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(short?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_shortq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("shortq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_shortq_NewArrayList(short?[] val, Expression[] exprs) {
				        Expression<Func<short?[]>> e =
				            Expression.Lambda<Func<short?[]>>(
				                Expression.NewArrayInit(typeof(short?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<short?[]> f = e.Compile();
				        short?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("shortq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("shortq_NewArrayList failed");
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
			
			//-------- Scenario 2308
			namespace Scenario2308{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "intq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression intq_NewArrayList__() {
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
				            success = check_intq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_intq_NewArrayList() {
				        int?[][] vals = new int?[][] {
				            new int?[] {  },
				            new int?[] { 0 },
				            new int?[] { 0, 1, -1, int.MinValue, int.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                int? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(int?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_intq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("intq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_intq_NewArrayList(int?[] val, Expression[] exprs) {
				        Expression<Func<int?[]>> e =
				            Expression.Lambda<Func<int?[]>>(
				                Expression.NewArrayInit(typeof(int?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<int?[]> f = e.Compile();
				        int?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("intq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("intq_NewArrayList failed");
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
			
			//-------- Scenario 2309
			namespace Scenario2309{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "longq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression longq_NewArrayList__() {
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
				            success = check_longq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_longq_NewArrayList() {
				        long?[][] vals = new long?[][] {
				            new long?[] {  },
				            new long?[] { 0 },
				            new long?[] { 0, 1, -1, long.MinValue, long.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                long? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(long?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_longq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("longq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_longq_NewArrayList(long?[] val, Expression[] exprs) {
				        Expression<Func<long?[]>> e =
				            Expression.Lambda<Func<long?[]>>(
				                Expression.NewArrayInit(typeof(long?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<long?[]> f = e.Compile();
				        long?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("longq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("longq_NewArrayList failed");
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
			
			//-------- Scenario 2310
			namespace Scenario2310{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "floatq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression floatq_NewArrayList__() {
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
				            success = check_floatq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_floatq_NewArrayList() {
				        float?[][] vals = new float?[][] {
				            new float?[] {  },
				            new float?[] { 0 },
				            new float?[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                float? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(float?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_floatq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("floatq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_floatq_NewArrayList(float?[] val, Expression[] exprs) {
				        Expression<Func<float?[]>> e =
				            Expression.Lambda<Func<float?[]>>(
				                Expression.NewArrayInit(typeof(float?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<float?[]> f = e.Compile();
				        float?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("floatq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("floatq_NewArrayList failed");
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
			
			//-------- Scenario 2311
			namespace Scenario2311{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "doubleq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression doubleq_NewArrayList__() {
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
				            success = check_doubleq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_doubleq_NewArrayList() {
				        double?[][] vals = new double?[][] {
				            new double?[] {  },
				            new double?[] { 0 },
				            new double?[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                double? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(double?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_doubleq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("doubleq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_doubleq_NewArrayList(double?[] val, Expression[] exprs) {
				        Expression<Func<double?[]>> e =
				            Expression.Lambda<Func<double?[]>>(
				                Expression.NewArrayInit(typeof(double?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<double?[]> f = e.Compile();
				        double?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("doubleq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("doubleq_NewArrayList failed");
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
			
			//-------- Scenario 2312
			namespace Scenario2312{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "decimalq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression decimalq_NewArrayList__() {
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
				            success = check_decimalq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_decimalq_NewArrayList() {
				        decimal?[][] vals = new decimal?[][] {
				            new decimal?[] {  },
				            new decimal?[] { decimal.Zero },
				            new decimal?[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                decimal? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(decimal?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_decimalq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("decimalq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimalq_NewArrayList(decimal?[] val, Expression[] exprs) {
				        Expression<Func<decimal?[]>> e =
				            Expression.Lambda<Func<decimal?[]>>(
				                Expression.NewArrayInit(typeof(decimal?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<decimal?[]> f = e.Compile();
				        decimal?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("decimalq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("decimalq_NewArrayList failed");
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
			
			//-------- Scenario 2313
			namespace Scenario2313{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "charq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression charq_NewArrayList__() {
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
				            success = check_charq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_charq_NewArrayList() {
				        char?[][] vals = new char?[][] {
				            new char?[] {  },
				            new char?[] { '\0' },
				            new char?[] { '\0', '\b', 'A', '\uffff' }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                char? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(char?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_charq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("charq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_charq_NewArrayList(char?[] val, Expression[] exprs) {
				        Expression<Func<char?[]>> e =
				            Expression.Lambda<Func<char?[]>>(
				                Expression.NewArrayInit(typeof(char?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<char?[]> f = e.Compile();
				        char?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("charq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("charq_NewArrayList failed");
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
			
			//-------- Scenario 2314
			namespace Scenario2314{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "boolq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression boolq_NewArrayList__() {
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
				            success = check_boolq_NewArrayList();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_boolq_NewArrayList() {
				        bool?[][] vals = new bool?[][] {
				            new bool?[] {  },
				            new bool?[] { true },
				            new bool?[] { true, false }
				        };
				        Expression[][] exprs = new Expression[vals.Length][];
				        for (int i = 0; i < vals.Length; i++) {
				            exprs[i] = new Expression[vals[i].Length];
				            for (int j = 0; j < vals[i].Length; j++) {
				                bool? val = vals[i][j];
				                exprs[i][j] = Expression.Constant(val, typeof(bool?));
				            }
				        }
				
				        for (int i = 0; i < vals.Length; i++) {
				            if (!check_boolq_NewArrayList(vals[i], exprs[i])) {
				                Console.WriteLine("boolq_NewArrayList failed");
				                return false;
				            }
				        }
				        return true;
				    }
				
				    static bool check_boolq_NewArrayList(bool?[] val, Expression[] exprs) {
				        Expression<Func<bool?[]>> e =
				            Expression.Lambda<Func<bool?[]>>(
				                Expression.NewArrayInit(typeof(bool?), exprs),
				                new System.Collections.Generic.List<ParameterExpression>());
				        Func<bool?[]> f = e.Compile();
				        bool?[] result = f();
				        if (result.Length != val.Length) {
				            Console.WriteLine("boolq_NewArrayList failed");
				            return false;
				        }
				        for (int i = 0; i < result.Length; i++) {
				            if (!object.Equals(result[i], val[i])) {
				                Console.WriteLine("boolq_NewArrayList failed");
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
				
				//-------- Scenario 2315
				namespace Scenario2315{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Sq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Sq_NewArrayList__() {
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
					            success = check_Sq_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Sq_NewArrayList() {
					        S?[][] vals = new S?[][] {
					            new S?[] {  },
					            new S?[] { default(S) },
					            new S?[] { default(S), new S() }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                S? val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(S?));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Sq_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("Sq_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Sq_NewArrayList(S?[] val, Expression[] exprs) {
					        Expression<Func<S?[]>> e =
					            Expression.Lambda<Func<S?[]>>(
					                Expression.NewArrayInit(typeof(S?), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<S?[]> f = e.Compile();
					        S?[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Sq_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Sq_NewArrayList failed");
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
			
			public struct Scs : IEquatable<Scs>
			{
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
				
				//-------- Scenario 2316
				namespace Scenario2316{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Spq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Spq_NewArrayList__() {
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
					            success = check_Spq_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Spq_NewArrayList() {
					        Sp?[][] vals = new Sp?[][] {
					            new Sp?[] {  },
					            new Sp?[] { default(Sp) },
					            new Sp?[] { default(Sp), new Sp(), new Sp(5,5.0) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Sp? val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Sp?));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Spq_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("Spq_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Spq_NewArrayList(Sp?[] val, Expression[] exprs) {
					        Expression<Func<Sp?[]>> e =
					            Expression.Lambda<Func<Sp?[]>>(
					                Expression.NewArrayInit(typeof(Sp?), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp?[]> f = e.Compile();
					        Sp?[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Spq_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Spq_NewArrayList failed");
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
			
			public struct Scs : IEquatable<Scs>
			{
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
				
				//-------- Scenario 2317
				namespace Scenario2317{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Ssq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Ssq_NewArrayList__() {
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
					            success = check_Ssq_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Ssq_NewArrayList() {
					        Ss?[][] vals = new Ss?[][] {
					            new Ss?[] {  },
					            new Ss?[] { default(Ss) },
					            new Ss?[] { default(Ss), new Ss(), new Ss(new S()) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Ss? val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Ss?));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Ssq_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("Ssq_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Ssq_NewArrayList(Ss?[] val, Expression[] exprs) {
					        Expression<Func<Ss?[]>> e =
					            Expression.Lambda<Func<Ss?[]>>(
					                Expression.NewArrayInit(typeof(Ss?), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ss?[]> f = e.Compile();
					        Ss?[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Ssq_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Ssq_NewArrayList failed");
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
			
			public struct Scs : IEquatable<Scs>
			{
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
				
				//-------- Scenario 2318
				namespace Scenario2318{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Scq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Scq_NewArrayList__() {
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
					            success = check_Scq_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Scq_NewArrayList() {
					        Sc?[][] vals = new Sc?[][] {
					            new Sc?[] {  },
					            new Sc?[] { default(Sc) },
					            new Sc?[] { default(Sc), new Sc(), new Sc(null) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Sc? val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Sc?));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Scq_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("Scq_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Scq_NewArrayList(Sc?[] val, Expression[] exprs) {
					        Expression<Func<Sc?[]>> e =
					            Expression.Lambda<Func<Sc?[]>>(
					                Expression.NewArrayInit(typeof(Sc?), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sc?[]> f = e.Compile();
					        Sc?[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Scq_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Scq_NewArrayList failed");
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
			
			public struct Scs : IEquatable<Scs>
			{
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
				
				//-------- Scenario 2319
				namespace Scenario2319{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Scsq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Scsq_NewArrayList__() {
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
					            success = check_Scsq_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Scsq_NewArrayList() {
					        Scs?[][] vals = new Scs?[][] {
					            new Scs?[] {  },
					            new Scs?[] { default(Scs) },
					            new Scs?[] { default(Scs), new Scs(), new Scs(null,new S()) }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Scs? val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Scs?));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Scsq_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("Scsq_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Scsq_NewArrayList(Scs?[] val, Expression[] exprs) {
					        Expression<Func<Scs?[]>> e =
					            Expression.Lambda<Func<Scs?[]>>(
					                Expression.NewArrayInit(typeof(Scs?), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs?[]> f = e.Compile();
					        Scs?[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Scsq_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Scsq_NewArrayList failed");
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
			
			public struct Scs : IEquatable<Scs>
			{
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
				
				//-------- Scenario 2320
				namespace Scenario2320{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tsq_NewArrayList_S___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tsq_NewArrayList_S___() {
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
					            success = check_Tsq_NewArrayList<S>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tsq_NewArrayList<Ts>() where Ts : struct {
					        Ts?[][] vals = new Ts?[][] {
					            new Ts?[] {  },
					            new Ts?[] { default(Ts) },
					            new Ts?[] { default(Ts), new Ts() }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Ts? val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Ts?));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Tsq_NewArrayList<Ts>(vals[i], exprs[i])) {
					                Console.WriteLine("Tsq_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tsq_NewArrayList<Ts>(Ts?[] val, Expression[] exprs) where Ts : struct {
					        Expression<Func<Ts?[]>> e =
					            Expression.Lambda<Func<Ts?[]>>(
					                Expression.NewArrayInit(typeof(Ts?), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?[]> f = e.Compile();
					        Ts?[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Tsq_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Tsq_NewArrayList failed");
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
			
			public struct Scs : IEquatable<Scs>
			{
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
				
				//-------- Scenario 2321
				namespace Scenario2321{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tsq_NewArrayList_Scs___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tsq_NewArrayList_Scs___() {
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
					            success = check_Tsq_NewArrayList<Scs>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tsq_NewArrayList<Ts>() where Ts : struct {
					        Ts?[][] vals = new Ts?[][] {
					            new Ts?[] {  },
					            new Ts?[] { default(Ts) },
					            new Ts?[] { default(Ts), new Ts() }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Ts? val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Ts?));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Tsq_NewArrayList<Ts>(vals[i], exprs[i])) {
					                Console.WriteLine("Tsq_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tsq_NewArrayList<Ts>(Ts?[] val, Expression[] exprs) where Ts : struct {
					        Expression<Func<Ts?[]>> e =
					            Expression.Lambda<Func<Ts?[]>>(
					                Expression.NewArrayInit(typeof(Ts?), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?[]> f = e.Compile();
					        Ts?[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Tsq_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Tsq_NewArrayList failed");
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
			
			public struct Scs : IEquatable<Scs>
			{
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
				
				//-------- Scenario 2322
				namespace Scenario2322{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Tsq_NewArrayList_E___", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Tsq_NewArrayList_E___() {
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
					            success = check_Tsq_NewArrayList<E>();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Tsq_NewArrayList<Ts>() where Ts : struct {
					        Ts?[][] vals = new Ts?[][] {
					            new Ts?[] {  },
					            new Ts?[] { default(Ts) },
					            new Ts?[] { default(Ts), new Ts() }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                Ts? val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(Ts?));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Tsq_NewArrayList<Ts>(vals[i], exprs[i])) {
					                Console.WriteLine("Tsq_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Tsq_NewArrayList<Ts>(Ts?[] val, Expression[] exprs) where Ts : struct {
					        Expression<Func<Ts?[]>> e =
					            Expression.Lambda<Func<Ts?[]>>(
					                Expression.NewArrayInit(typeof(Ts?), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<Ts?[]> f = e.Compile();
					        Ts?[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Tsq_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Tsq_NewArrayList failed");
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
			
			public struct Scs : IEquatable<Scs>
			{
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
				
				//-------- Scenario 2323
				namespace Scenario2323{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Eq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Eq_NewArrayList__() {
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
					            success = check_Eq_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Eq_NewArrayList() {
					        E?[][] vals = new E?[][] {
					            new E?[] {  },
					            new E?[] { (E) 0 },
					            new E?[] { (E) 0, E.A, E.B, (E) int.MaxValue, (E) int.MinValue }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                E? val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(E?));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Eq_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("Eq_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Eq_NewArrayList(E?[] val, Expression[] exprs) {
					        Expression<Func<E?[]>> e =
					            Expression.Lambda<Func<E?[]>>(
					                Expression.NewArrayInit(typeof(E?), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<E?[]> f = e.Compile();
					        E?[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Eq_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Eq_NewArrayList failed");
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
			
			public struct Scs : IEquatable<Scs>
			{
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
				
				//-------- Scenario 2324
				namespace Scenario2324{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Elq_NewArrayList__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression Elq_NewArrayList__() {
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
					            success = check_Elq_NewArrayList();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_Elq_NewArrayList() {
					        El?[][] vals = new El?[][] {
					            new El?[] {  },
					            new El?[] { (El) 0 },
					            new El?[] { (El) 0, El.A, El.B, (El) long.MaxValue, (El) long.MinValue }
					        };
					        Expression[][] exprs = new Expression[vals.Length][];
					        for (int i = 0; i < vals.Length; i++) {
					            exprs[i] = new Expression[vals[i].Length];
					            for (int j = 0; j < vals[i].Length; j++) {
					                El? val = vals[i][j];
					                exprs[i][j] = Expression.Constant(val, typeof(El?));
					            }
					        }
					
					        for (int i = 0; i < vals.Length; i++) {
					            if (!check_Elq_NewArrayList(vals[i], exprs[i])) {
					                Console.WriteLine("Elq_NewArrayList failed");
					                return false;
					            }
					        }
					        return true;
					    }
					
					    static bool check_Elq_NewArrayList(El?[] val, Expression[] exprs) {
					        Expression<Func<El?[]>> e =
					            Expression.Lambda<Func<El?[]>>(
					                Expression.NewArrayInit(typeof(El?), exprs),
					                new System.Collections.Generic.List<ParameterExpression>());
					        Func<El?[]> f = e.Compile();
					        El?[] result = f();
					        if (result.Length != val.Length) {
					            Console.WriteLine("Elq_NewArrayList failed");
					            return false;
					        }
					        for (int i = 0; i < result.Length; i++) {
					            if (!object.Equals(result[i], val[i])) {
					                Console.WriteLine("Elq_NewArrayList failed");
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
			
			public struct Scs : IEquatable<Scs>
			{
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
