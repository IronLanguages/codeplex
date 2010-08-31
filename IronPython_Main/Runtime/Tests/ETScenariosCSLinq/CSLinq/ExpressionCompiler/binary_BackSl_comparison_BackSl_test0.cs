#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 2165
			namespace Scenario2165{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Equal__4", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression Equal__4() {
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
				            success = check_byte_bool_Equal() &
				                check_sbyte_bool_Equal() &
				                check_ushort_bool_Equal() &
				                check_short_bool_Equal() &
				                check_uint_bool_Equal() &
				                check_int_bool_Equal() &
				                check_ulong_bool_Equal() &
				                check_long_bool_Equal() &
				                check_float_bool_Equal() &
				                check_double_bool_Equal() &
				                check_decimal_bool_Equal() &
				                check_char_bool_Equal();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_bool_Equal() {
				        byte[] svals = new byte[] { 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_byte_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("byte_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_bool_Equal(byte val0, byte val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(Expression.Constant(val0, typeof(byte)),
				                    Expression.Constant(val1, typeof(byte))),
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
				            csResult = (bool) (val0 == val1);
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
				
				    static bool check_sbyte_bool_Equal() {
				        sbyte[] svals = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_sbyte_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("sbyte_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_bool_Equal(sbyte val0, sbyte val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(Expression.Constant(val0, typeof(sbyte)),
				                    Expression.Constant(val1, typeof(sbyte))),
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
				            csResult = (bool) (val0 == val1);
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
				
				    static bool check_ushort_bool_Equal() {
				        ushort[] svals = new ushort[] { 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_ushort_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("ushort_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_bool_Equal(ushort val0, ushort val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(Expression.Constant(val0, typeof(ushort)),
				                    Expression.Constant(val1, typeof(ushort))),
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
				            csResult = (bool) (val0 == val1);
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
				
				    static bool check_short_bool_Equal() {
				        short[] svals = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_short_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("short_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_bool_Equal(short val0, short val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(Expression.Constant(val0, typeof(short)),
				                    Expression.Constant(val1, typeof(short))),
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
				            csResult = (bool) (val0 == val1);
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
				
				    static bool check_uint_bool_Equal() {
				        uint[] svals = new uint[] { 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_uint_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("uint_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_bool_Equal(uint val0, uint val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(Expression.Constant(val0, typeof(uint)),
				                    Expression.Constant(val1, typeof(uint))),
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
				            csResult = (bool) (val0 == val1);
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
				
				    static bool check_int_bool_Equal() {
				        int[] svals = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_int_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("int_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_bool_Equal(int val0, int val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(Expression.Constant(val0, typeof(int)),
				                    Expression.Constant(val1, typeof(int))),
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
				            csResult = (bool) (val0 == val1);
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
				
				    static bool check_ulong_bool_Equal() {
				        ulong[] svals = new ulong[] { 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_ulong_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("ulong_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_bool_Equal(ulong val0, ulong val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(Expression.Constant(val0, typeof(ulong)),
				                    Expression.Constant(val1, typeof(ulong))),
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
				            csResult = (bool) (val0 == val1);
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
				
				    static bool check_long_bool_Equal() {
				        long[] svals = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_long_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("long_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_bool_Equal(long val0, long val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(Expression.Constant(val0, typeof(long)),
				                    Expression.Constant(val1, typeof(long))),
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
				            csResult = (bool) (val0 == val1);
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
				
				    static bool check_float_bool_Equal() {
				        float[] svals = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_float_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("float_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_bool_Equal(float val0, float val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(Expression.Constant(val0, typeof(float)),
				                    Expression.Constant(val1, typeof(float))),
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
				            csResult = (bool) (val0 == val1);
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
				
				    static bool check_double_bool_Equal() {
				        double[] svals = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_double_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("double_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_bool_Equal(double val0, double val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(Expression.Constant(val0, typeof(double)),
				                    Expression.Constant(val1, typeof(double))),
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
				            csResult = (bool) (val0 == val1);
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
				
				    static bool check_decimal_bool_Equal() {
				        decimal[] svals = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_decimal_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("decimal_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_bool_Equal(decimal val0, decimal val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(Expression.Constant(val0, typeof(decimal)),
				                    Expression.Constant(val1, typeof(decimal))),
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
				            csResult = (bool) (val0 == val1);
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
				
				    static bool check_char_bool_Equal() {
				        char[] svals = new char[] { '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_char_bool_Equal(svals[i], svals[j])) {
				                    Console.WriteLine("char_bool_Equal failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_bool_Equal(char val0, char val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.Equal(Expression.Constant(val0, typeof(char)),
				                    Expression.Constant(val1, typeof(char))),
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
				            csResult = (bool) (val0 == val1);
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
			
			//-------- Scenario 2166
			namespace Scenario2166{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NotEqual__4", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression NotEqual__4() {
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
				            success = check_byte_bool_NotEqual() &
				                check_sbyte_bool_NotEqual() &
				                check_ushort_bool_NotEqual() &
				                check_short_bool_NotEqual() &
				                check_uint_bool_NotEqual() &
				                check_int_bool_NotEqual() &
				                check_ulong_bool_NotEqual() &
				                check_long_bool_NotEqual() &
				                check_float_bool_NotEqual() &
				                check_double_bool_NotEqual() &
				                check_decimal_bool_NotEqual() &
				                check_char_bool_NotEqual();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_bool_NotEqual() {
				        byte[] svals = new byte[] { 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_byte_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("byte_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_bool_NotEqual(byte val0, byte val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(Expression.Constant(val0, typeof(byte)),
				                    Expression.Constant(val1, typeof(byte))),
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
				            csResult = (bool) (val0 != val1);
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
				
				    static bool check_sbyte_bool_NotEqual() {
				        sbyte[] svals = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_sbyte_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("sbyte_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_bool_NotEqual(sbyte val0, sbyte val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(Expression.Constant(val0, typeof(sbyte)),
				                    Expression.Constant(val1, typeof(sbyte))),
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
				            csResult = (bool) (val0 != val1);
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
				
				    static bool check_ushort_bool_NotEqual() {
				        ushort[] svals = new ushort[] { 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_ushort_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("ushort_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_bool_NotEqual(ushort val0, ushort val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(Expression.Constant(val0, typeof(ushort)),
				                    Expression.Constant(val1, typeof(ushort))),
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
				            csResult = (bool) (val0 != val1);
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
				
				    static bool check_short_bool_NotEqual() {
				        short[] svals = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_short_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("short_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_bool_NotEqual(short val0, short val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(Expression.Constant(val0, typeof(short)),
				                    Expression.Constant(val1, typeof(short))),
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
				            csResult = (bool) (val0 != val1);
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
				
				    static bool check_uint_bool_NotEqual() {
				        uint[] svals = new uint[] { 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_uint_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("uint_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_bool_NotEqual(uint val0, uint val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(Expression.Constant(val0, typeof(uint)),
				                    Expression.Constant(val1, typeof(uint))),
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
				            csResult = (bool) (val0 != val1);
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
				
				    static bool check_int_bool_NotEqual() {
				        int[] svals = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_int_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("int_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_bool_NotEqual(int val0, int val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(Expression.Constant(val0, typeof(int)),
				                    Expression.Constant(val1, typeof(int))),
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
				            csResult = (bool) (val0 != val1);
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
				
				    static bool check_ulong_bool_NotEqual() {
				        ulong[] svals = new ulong[] { 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_ulong_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("ulong_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_bool_NotEqual(ulong val0, ulong val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(Expression.Constant(val0, typeof(ulong)),
				                    Expression.Constant(val1, typeof(ulong))),
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
				            csResult = (bool) (val0 != val1);
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
				
				    static bool check_long_bool_NotEqual() {
				        long[] svals = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_long_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("long_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_bool_NotEqual(long val0, long val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(Expression.Constant(val0, typeof(long)),
				                    Expression.Constant(val1, typeof(long))),
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
				            csResult = (bool) (val0 != val1);
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
				
				    static bool check_float_bool_NotEqual() {
				        float[] svals = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_float_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("float_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_bool_NotEqual(float val0, float val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(Expression.Constant(val0, typeof(float)),
				                    Expression.Constant(val1, typeof(float))),
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
				            csResult = (bool) (val0 != val1);
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
				
				    static bool check_double_bool_NotEqual() {
				        double[] svals = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_double_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("double_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_bool_NotEqual(double val0, double val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(Expression.Constant(val0, typeof(double)),
				                    Expression.Constant(val1, typeof(double))),
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
				            csResult = (bool) (val0 != val1);
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
				
				    static bool check_decimal_bool_NotEqual() {
				        decimal[] svals = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_decimal_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("decimal_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_bool_NotEqual(decimal val0, decimal val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(Expression.Constant(val0, typeof(decimal)),
				                    Expression.Constant(val1, typeof(decimal))),
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
				            csResult = (bool) (val0 != val1);
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
				
				    static bool check_char_bool_NotEqual() {
				        char[] svals = new char[] { '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_char_bool_NotEqual(svals[i], svals[j])) {
				                    Console.WriteLine("char_bool_NotEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_bool_NotEqual(char val0, char val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.NotEqual(Expression.Constant(val0, typeof(char)),
				                    Expression.Constant(val1, typeof(char))),
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
				            csResult = (bool) (val0 != val1);
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
			
			//-------- Scenario 2167
			namespace Scenario2167{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThanOrEqual__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression GreaterThanOrEqual__2() {
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
				            success = check_byte_bool_GreaterThanOrEqual() &
				                check_sbyte_bool_GreaterThanOrEqual() &
				                check_ushort_bool_GreaterThanOrEqual() &
				                check_short_bool_GreaterThanOrEqual() &
				                check_uint_bool_GreaterThanOrEqual() &
				                check_int_bool_GreaterThanOrEqual() &
				                check_ulong_bool_GreaterThanOrEqual() &
				                check_long_bool_GreaterThanOrEqual() &
				                check_float_bool_GreaterThanOrEqual() &
				                check_double_bool_GreaterThanOrEqual() &
				                check_decimal_bool_GreaterThanOrEqual() &
				                check_char_bool_GreaterThanOrEqual();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_bool_GreaterThanOrEqual() {
				        byte[] svals = new byte[] { 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_byte_bool_GreaterThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("byte_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_bool_GreaterThanOrEqual(byte val0, byte val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(Expression.Constant(val0, typeof(byte)),
				                    Expression.Constant(val1, typeof(byte))),
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
				            csResult = (bool) (val0 >= val1);
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
				
				    static bool check_sbyte_bool_GreaterThanOrEqual() {
				        sbyte[] svals = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_sbyte_bool_GreaterThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("sbyte_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_bool_GreaterThanOrEqual(sbyte val0, sbyte val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(Expression.Constant(val0, typeof(sbyte)),
				                    Expression.Constant(val1, typeof(sbyte))),
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
				            csResult = (bool) (val0 >= val1);
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
				
				    static bool check_ushort_bool_GreaterThanOrEqual() {
				        ushort[] svals = new ushort[] { 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_ushort_bool_GreaterThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("ushort_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_bool_GreaterThanOrEqual(ushort val0, ushort val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(Expression.Constant(val0, typeof(ushort)),
				                    Expression.Constant(val1, typeof(ushort))),
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
				            csResult = (bool) (val0 >= val1);
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
				
				    static bool check_short_bool_GreaterThanOrEqual() {
				        short[] svals = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_short_bool_GreaterThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("short_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_bool_GreaterThanOrEqual(short val0, short val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(Expression.Constant(val0, typeof(short)),
				                    Expression.Constant(val1, typeof(short))),
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
				            csResult = (bool) (val0 >= val1);
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
				
				    static bool check_uint_bool_GreaterThanOrEqual() {
				        uint[] svals = new uint[] { 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_uint_bool_GreaterThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("uint_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_bool_GreaterThanOrEqual(uint val0, uint val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(Expression.Constant(val0, typeof(uint)),
				                    Expression.Constant(val1, typeof(uint))),
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
				            csResult = (bool) (val0 >= val1);
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
				
				    static bool check_int_bool_GreaterThanOrEqual() {
				        int[] svals = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_int_bool_GreaterThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("int_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_bool_GreaterThanOrEqual(int val0, int val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(Expression.Constant(val0, typeof(int)),
				                    Expression.Constant(val1, typeof(int))),
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
				            csResult = (bool) (val0 >= val1);
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
				
				    static bool check_ulong_bool_GreaterThanOrEqual() {
				        ulong[] svals = new ulong[] { 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_ulong_bool_GreaterThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("ulong_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_bool_GreaterThanOrEqual(ulong val0, ulong val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(Expression.Constant(val0, typeof(ulong)),
				                    Expression.Constant(val1, typeof(ulong))),
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
				            csResult = (bool) (val0 >= val1);
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
				
				    static bool check_long_bool_GreaterThanOrEqual() {
				        long[] svals = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_long_bool_GreaterThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("long_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_bool_GreaterThanOrEqual(long val0, long val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(Expression.Constant(val0, typeof(long)),
				                    Expression.Constant(val1, typeof(long))),
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
				            csResult = (bool) (val0 >= val1);
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
				
				    static bool check_float_bool_GreaterThanOrEqual() {
				        float[] svals = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_float_bool_GreaterThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("float_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_bool_GreaterThanOrEqual(float val0, float val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(Expression.Constant(val0, typeof(float)),
				                    Expression.Constant(val1, typeof(float))),
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
				            csResult = (bool) (val0 >= val1);
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
				
				    static bool check_double_bool_GreaterThanOrEqual() {
				        double[] svals = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_double_bool_GreaterThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("double_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_bool_GreaterThanOrEqual(double val0, double val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(Expression.Constant(val0, typeof(double)),
				                    Expression.Constant(val1, typeof(double))),
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
				            csResult = (bool) (val0 >= val1);
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
				
				    static bool check_decimal_bool_GreaterThanOrEqual() {
				        decimal[] svals = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_decimal_bool_GreaterThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("decimal_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_bool_GreaterThanOrEqual(decimal val0, decimal val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(Expression.Constant(val0, typeof(decimal)),
				                    Expression.Constant(val1, typeof(decimal))),
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
				            csResult = (bool) (val0 >= val1);
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
				
				    static bool check_char_bool_GreaterThanOrEqual() {
				        char[] svals = new char[] { '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_char_bool_GreaterThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("char_bool_GreaterThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_bool_GreaterThanOrEqual(char val0, char val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThanOrEqual(Expression.Constant(val0, typeof(char)),
				                    Expression.Constant(val1, typeof(char))),
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
				            csResult = (bool) (val0 >= val1);
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
			
			//-------- Scenario 2168
			namespace Scenario2168{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GreaterThan__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression GreaterThan__2() {
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
				            success = check_byte_bool_GreaterThan() &
				                check_sbyte_bool_GreaterThan() &
				                check_ushort_bool_GreaterThan() &
				                check_short_bool_GreaterThan() &
				                check_uint_bool_GreaterThan() &
				                check_int_bool_GreaterThan() &
				                check_ulong_bool_GreaterThan() &
				                check_long_bool_GreaterThan() &
				                check_float_bool_GreaterThan() &
				                check_double_bool_GreaterThan() &
				                check_decimal_bool_GreaterThan() &
				                check_char_bool_GreaterThan();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_bool_GreaterThan() {
				        byte[] svals = new byte[] { 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_byte_bool_GreaterThan(svals[i], svals[j])) {
				                    Console.WriteLine("byte_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_bool_GreaterThan(byte val0, byte val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(Expression.Constant(val0, typeof(byte)),
				                    Expression.Constant(val1, typeof(byte))),
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
				            csResult = (bool) (val0 > val1);
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
				
				    static bool check_sbyte_bool_GreaterThan() {
				        sbyte[] svals = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_sbyte_bool_GreaterThan(svals[i], svals[j])) {
				                    Console.WriteLine("sbyte_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_bool_GreaterThan(sbyte val0, sbyte val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(Expression.Constant(val0, typeof(sbyte)),
				                    Expression.Constant(val1, typeof(sbyte))),
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
				            csResult = (bool) (val0 > val1);
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
				
				    static bool check_ushort_bool_GreaterThan() {
				        ushort[] svals = new ushort[] { 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_ushort_bool_GreaterThan(svals[i], svals[j])) {
				                    Console.WriteLine("ushort_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_bool_GreaterThan(ushort val0, ushort val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(Expression.Constant(val0, typeof(ushort)),
				                    Expression.Constant(val1, typeof(ushort))),
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
				            csResult = (bool) (val0 > val1);
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
				
				    static bool check_short_bool_GreaterThan() {
				        short[] svals = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_short_bool_GreaterThan(svals[i], svals[j])) {
				                    Console.WriteLine("short_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_bool_GreaterThan(short val0, short val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(Expression.Constant(val0, typeof(short)),
				                    Expression.Constant(val1, typeof(short))),
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
				            csResult = (bool) (val0 > val1);
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
				
				    static bool check_uint_bool_GreaterThan() {
				        uint[] svals = new uint[] { 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_uint_bool_GreaterThan(svals[i], svals[j])) {
				                    Console.WriteLine("uint_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_bool_GreaterThan(uint val0, uint val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(Expression.Constant(val0, typeof(uint)),
				                    Expression.Constant(val1, typeof(uint))),
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
				            csResult = (bool) (val0 > val1);
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
				
				    static bool check_int_bool_GreaterThan() {
				        int[] svals = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_int_bool_GreaterThan(svals[i], svals[j])) {
				                    Console.WriteLine("int_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_bool_GreaterThan(int val0, int val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(Expression.Constant(val0, typeof(int)),
				                    Expression.Constant(val1, typeof(int))),
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
				            csResult = (bool) (val0 > val1);
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
				
				    static bool check_ulong_bool_GreaterThan() {
				        ulong[] svals = new ulong[] { 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_ulong_bool_GreaterThan(svals[i], svals[j])) {
				                    Console.WriteLine("ulong_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_bool_GreaterThan(ulong val0, ulong val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(Expression.Constant(val0, typeof(ulong)),
				                    Expression.Constant(val1, typeof(ulong))),
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
				            csResult = (bool) (val0 > val1);
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
				
				    static bool check_long_bool_GreaterThan() {
				        long[] svals = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_long_bool_GreaterThan(svals[i], svals[j])) {
				                    Console.WriteLine("long_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_bool_GreaterThan(long val0, long val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(Expression.Constant(val0, typeof(long)),
				                    Expression.Constant(val1, typeof(long))),
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
				            csResult = (bool) (val0 > val1);
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
				
				    static bool check_float_bool_GreaterThan() {
				        float[] svals = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_float_bool_GreaterThan(svals[i], svals[j])) {
				                    Console.WriteLine("float_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_bool_GreaterThan(float val0, float val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(Expression.Constant(val0, typeof(float)),
				                    Expression.Constant(val1, typeof(float))),
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
				            csResult = (bool) (val0 > val1);
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
				
				    static bool check_double_bool_GreaterThan() {
				        double[] svals = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_double_bool_GreaterThan(svals[i], svals[j])) {
				                    Console.WriteLine("double_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_bool_GreaterThan(double val0, double val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(Expression.Constant(val0, typeof(double)),
				                    Expression.Constant(val1, typeof(double))),
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
				            csResult = (bool) (val0 > val1);
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
				
				    static bool check_decimal_bool_GreaterThan() {
				        decimal[] svals = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_decimal_bool_GreaterThan(svals[i], svals[j])) {
				                    Console.WriteLine("decimal_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_bool_GreaterThan(decimal val0, decimal val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(Expression.Constant(val0, typeof(decimal)),
				                    Expression.Constant(val1, typeof(decimal))),
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
				            csResult = (bool) (val0 > val1);
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
				
				    static bool check_char_bool_GreaterThan() {
				        char[] svals = new char[] { '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_char_bool_GreaterThan(svals[i], svals[j])) {
				                    Console.WriteLine("char_bool_GreaterThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_bool_GreaterThan(char val0, char val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.GreaterThan(Expression.Constant(val0, typeof(char)),
				                    Expression.Constant(val1, typeof(char))),
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
				            csResult = (bool) (val0 > val1);
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
			
			//-------- Scenario 2169
			namespace Scenario2169{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThan__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression LessThan__2() {
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
				            success = check_byte_bool_LessThan() &
				                check_sbyte_bool_LessThan() &
				                check_ushort_bool_LessThan() &
				                check_short_bool_LessThan() &
				                check_uint_bool_LessThan() &
				                check_int_bool_LessThan() &
				                check_ulong_bool_LessThan() &
				                check_long_bool_LessThan() &
				                check_float_bool_LessThan() &
				                check_double_bool_LessThan() &
				                check_decimal_bool_LessThan() &
				                check_char_bool_LessThan();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_bool_LessThan() {
				        byte[] svals = new byte[] { 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_byte_bool_LessThan(svals[i], svals[j])) {
				                    Console.WriteLine("byte_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_bool_LessThan(byte val0, byte val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(Expression.Constant(val0, typeof(byte)),
				                    Expression.Constant(val1, typeof(byte))),
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
				            csResult = (bool) (val0 < val1);
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
				
				    static bool check_sbyte_bool_LessThan() {
				        sbyte[] svals = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_sbyte_bool_LessThan(svals[i], svals[j])) {
				                    Console.WriteLine("sbyte_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_bool_LessThan(sbyte val0, sbyte val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(Expression.Constant(val0, typeof(sbyte)),
				                    Expression.Constant(val1, typeof(sbyte))),
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
				            csResult = (bool) (val0 < val1);
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
				
				    static bool check_ushort_bool_LessThan() {
				        ushort[] svals = new ushort[] { 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_ushort_bool_LessThan(svals[i], svals[j])) {
				                    Console.WriteLine("ushort_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_bool_LessThan(ushort val0, ushort val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(Expression.Constant(val0, typeof(ushort)),
				                    Expression.Constant(val1, typeof(ushort))),
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
				            csResult = (bool) (val0 < val1);
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
				
				    static bool check_short_bool_LessThan() {
				        short[] svals = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_short_bool_LessThan(svals[i], svals[j])) {
				                    Console.WriteLine("short_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_bool_LessThan(short val0, short val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(Expression.Constant(val0, typeof(short)),
				                    Expression.Constant(val1, typeof(short))),
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
				            csResult = (bool) (val0 < val1);
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
				
				    static bool check_uint_bool_LessThan() {
				        uint[] svals = new uint[] { 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_uint_bool_LessThan(svals[i], svals[j])) {
				                    Console.WriteLine("uint_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_bool_LessThan(uint val0, uint val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(Expression.Constant(val0, typeof(uint)),
				                    Expression.Constant(val1, typeof(uint))),
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
				            csResult = (bool) (val0 < val1);
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
				
				    static bool check_int_bool_LessThan() {
				        int[] svals = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_int_bool_LessThan(svals[i], svals[j])) {
				                    Console.WriteLine("int_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_bool_LessThan(int val0, int val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(Expression.Constant(val0, typeof(int)),
				                    Expression.Constant(val1, typeof(int))),
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
				            csResult = (bool) (val0 < val1);
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
				
				    static bool check_ulong_bool_LessThan() {
				        ulong[] svals = new ulong[] { 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_ulong_bool_LessThan(svals[i], svals[j])) {
				                    Console.WriteLine("ulong_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_bool_LessThan(ulong val0, ulong val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(Expression.Constant(val0, typeof(ulong)),
				                    Expression.Constant(val1, typeof(ulong))),
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
				            csResult = (bool) (val0 < val1);
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
				
				    static bool check_long_bool_LessThan() {
				        long[] svals = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_long_bool_LessThan(svals[i], svals[j])) {
				                    Console.WriteLine("long_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_bool_LessThan(long val0, long val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(Expression.Constant(val0, typeof(long)),
				                    Expression.Constant(val1, typeof(long))),
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
				            csResult = (bool) (val0 < val1);
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
				
				    static bool check_float_bool_LessThan() {
				        float[] svals = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_float_bool_LessThan(svals[i], svals[j])) {
				                    Console.WriteLine("float_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_bool_LessThan(float val0, float val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(Expression.Constant(val0, typeof(float)),
				                    Expression.Constant(val1, typeof(float))),
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
				            csResult = (bool) (val0 < val1);
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
				
				    static bool check_double_bool_LessThan() {
				        double[] svals = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_double_bool_LessThan(svals[i], svals[j])) {
				                    Console.WriteLine("double_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_bool_LessThan(double val0, double val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(Expression.Constant(val0, typeof(double)),
				                    Expression.Constant(val1, typeof(double))),
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
				            csResult = (bool) (val0 < val1);
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
				
				    static bool check_decimal_bool_LessThan() {
				        decimal[] svals = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_decimal_bool_LessThan(svals[i], svals[j])) {
				                    Console.WriteLine("decimal_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_bool_LessThan(decimal val0, decimal val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(Expression.Constant(val0, typeof(decimal)),
				                    Expression.Constant(val1, typeof(decimal))),
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
				            csResult = (bool) (val0 < val1);
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
				
				    static bool check_char_bool_LessThan() {
				        char[] svals = new char[] { '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_char_bool_LessThan(svals[i], svals[j])) {
				                    Console.WriteLine("char_bool_LessThan failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_bool_LessThan(char val0, char val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThan(Expression.Constant(val0, typeof(char)),
				                    Expression.Constant(val1, typeof(char))),
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
				            csResult = (bool) (val0 < val1);
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
			
			//-------- Scenario 2170
			namespace Scenario2170{
				
				public class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "LessThanOrEqual__2", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression LessThanOrEqual__2() {
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
				            success = check_byte_bool_LessThanOrEqual() &
				                check_sbyte_bool_LessThanOrEqual() &
				                check_ushort_bool_LessThanOrEqual() &
				                check_short_bool_LessThanOrEqual() &
				                check_uint_bool_LessThanOrEqual() &
				                check_int_bool_LessThanOrEqual() &
				                check_ulong_bool_LessThanOrEqual() &
				                check_long_bool_LessThanOrEqual() &
				                check_float_bool_LessThanOrEqual() &
				                check_double_bool_LessThanOrEqual() &
				                check_decimal_bool_LessThanOrEqual() &
				                check_char_bool_LessThanOrEqual();
				        }
				        finally
				        {
				            Ext.StopCapture();
				        }
				        return success ? 0 : 1;
				    }
				
				    static bool check_byte_bool_LessThanOrEqual() {
				        byte[] svals = new byte[] { 0, 1, byte.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_byte_bool_LessThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("byte_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_byte_bool_LessThanOrEqual(byte val0, byte val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(Expression.Constant(val0, typeof(byte)),
				                    Expression.Constant(val1, typeof(byte))),
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
				            csResult = (bool) (val0 <= val1);
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
				
				    static bool check_sbyte_bool_LessThanOrEqual() {
				        sbyte[] svals = new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_sbyte_bool_LessThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("sbyte_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_sbyte_bool_LessThanOrEqual(sbyte val0, sbyte val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(Expression.Constant(val0, typeof(sbyte)),
				                    Expression.Constant(val1, typeof(sbyte))),
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
				            csResult = (bool) (val0 <= val1);
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
				
				    static bool check_ushort_bool_LessThanOrEqual() {
				        ushort[] svals = new ushort[] { 0, 1, ushort.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_ushort_bool_LessThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("ushort_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ushort_bool_LessThanOrEqual(ushort val0, ushort val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(Expression.Constant(val0, typeof(ushort)),
				                    Expression.Constant(val1, typeof(ushort))),
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
				            csResult = (bool) (val0 <= val1);
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
				
				    static bool check_short_bool_LessThanOrEqual() {
				        short[] svals = new short[] { 0, 1, -1, short.MinValue, short.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_short_bool_LessThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("short_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_short_bool_LessThanOrEqual(short val0, short val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(Expression.Constant(val0, typeof(short)),
				                    Expression.Constant(val1, typeof(short))),
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
				            csResult = (bool) (val0 <= val1);
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
				
				    static bool check_uint_bool_LessThanOrEqual() {
				        uint[] svals = new uint[] { 0, 1, uint.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_uint_bool_LessThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("uint_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_uint_bool_LessThanOrEqual(uint val0, uint val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(Expression.Constant(val0, typeof(uint)),
				                    Expression.Constant(val1, typeof(uint))),
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
				            csResult = (bool) (val0 <= val1);
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
				
				    static bool check_int_bool_LessThanOrEqual() {
				        int[] svals = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_int_bool_LessThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("int_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_int_bool_LessThanOrEqual(int val0, int val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(Expression.Constant(val0, typeof(int)),
				                    Expression.Constant(val1, typeof(int))),
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
				            csResult = (bool) (val0 <= val1);
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
				
				    static bool check_ulong_bool_LessThanOrEqual() {
				        ulong[] svals = new ulong[] { 0, 1, ulong.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_ulong_bool_LessThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("ulong_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_ulong_bool_LessThanOrEqual(ulong val0, ulong val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(Expression.Constant(val0, typeof(ulong)),
				                    Expression.Constant(val1, typeof(ulong))),
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
				            csResult = (bool) (val0 <= val1);
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
				
				    static bool check_long_bool_LessThanOrEqual() {
				        long[] svals = new long[] { 0, 1, -1, long.MinValue, long.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_long_bool_LessThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("long_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_long_bool_LessThanOrEqual(long val0, long val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(Expression.Constant(val0, typeof(long)),
				                    Expression.Constant(val1, typeof(long))),
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
				            csResult = (bool) (val0 <= val1);
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
				
				    static bool check_float_bool_LessThanOrEqual() {
				        float[] svals = new float[] { 0, 1, -1, float.MinValue, float.MaxValue, float.Epsilon, float.NegativeInfinity, float.PositiveInfinity, float.NaN };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_float_bool_LessThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("float_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_float_bool_LessThanOrEqual(float val0, float val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(Expression.Constant(val0, typeof(float)),
				                    Expression.Constant(val1, typeof(float))),
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
				            csResult = (bool) (val0 <= val1);
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
				
				    static bool check_double_bool_LessThanOrEqual() {
				        double[] svals = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_double_bool_LessThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("double_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_double_bool_LessThanOrEqual(double val0, double val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(Expression.Constant(val0, typeof(double)),
				                    Expression.Constant(val1, typeof(double))),
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
				            csResult = (bool) (val0 <= val1);
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
				
				    static bool check_decimal_bool_LessThanOrEqual() {
				        decimal[] svals = new decimal[] { decimal.Zero, decimal.One, decimal.MinusOne, decimal.MinValue, decimal.MaxValue };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_decimal_bool_LessThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("decimal_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_decimal_bool_LessThanOrEqual(decimal val0, decimal val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(Expression.Constant(val0, typeof(decimal)),
				                    Expression.Constant(val1, typeof(decimal))),
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
				            csResult = (bool) (val0 <= val1);
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
				
				    static bool check_char_bool_LessThanOrEqual() {
				        char[] svals = new char[] { '\0', '\b', 'A', '\uffff' };
				        for (int i = 0; i < svals.Length; i++) {
				            for (int j = 0; j < svals.Length; j++) {
				                if (!check_char_bool_LessThanOrEqual(svals[i], svals[j])) {
				                    Console.WriteLine("char_bool_LessThanOrEqual failed");
				                    return false;
				                }
				            }
				        }
				        return true;
				    }
				
				    static bool check_char_bool_LessThanOrEqual(char val0, char val1) {
				        Expression<Func<bool>> e =
				            Expression.Lambda<Func<bool>>(
				                Expression.LessThanOrEqual(Expression.Constant(val0, typeof(char)),
				                    Expression.Constant(val1, typeof(char))),
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
				            csResult = (bool) (val0 <= val1);
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
