#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System;
namespace ExpressionCompiler { 
	
				
				//-------- Scenario 256
				namespace Scenario256{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_double_Sp_New__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression int_double_Sp_New__() {
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
					            success = check_int_double_Sp_New();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_int_double_Sp_New() {
					        int[] svals0 = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
					        double[] svals1 = new double[] { 0, 1, -1, double.MinValue, double.MaxValue, double.Epsilon, double.NegativeInfinity, double.PositiveInfinity, double.NaN };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_int_double_Sp_New(svals0[i], svals1[j])) {
					                    Console.WriteLine("int_double_Sp_New failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_int_double_Sp_New(int val0, double val1) {
					        ConstructorInfo constructor = typeof(Sp).GetConstructor(new Type[] { typeof(int), typeof(double) });
					        Expression[] exprArgs = new Expression[] { Expression.Constant(val0, typeof(int)), Expression.Constant(val1, typeof(double)) };
					        Expression<Func<Sp>> e = Expression.Lambda<Func<Sp>>(
					            Expression.New(constructor, exprArgs),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Sp> f = e.Compile();
					
					        return object.Equals(f(), new Sp(val0, val1));
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
			    public string S;
			
			    public D() {
			    }
			    public D(int val) : this(val, "") {
			    }
			
			    public D(int val, string s) {
			        Val = val;
			        S = s;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val && d.S == S;
			    }
			
			    public override int GetHashCode() {
			        return Val ^ (S == null ? 0 : S.GetHashCode());
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
			        return other.I.Equals(I) && other.D.Equals(D);
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
				
				//-------- Scenario 257
				namespace Scenario257{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "string_S_Scs_New__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression string_S_Scs_New__() {
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
					            success = check_string_S_Scs_New();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_string_S_Scs_New() {
					        string[] svals0 = new string[] { null, "", "a", "foo" };
					        S[] svals1 = new S[] { default(S), new S() };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_string_S_Scs_New(svals0[i], svals1[j])) {
					                    Console.WriteLine("string_S_Scs_New failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_string_S_Scs_New(string val0, S val1) {
					        ConstructorInfo constructor = typeof(Scs).GetConstructor(new Type[] { typeof(string), typeof(S) });
					        Expression[] exprArgs = new Expression[] { Expression.Constant(val0, typeof(string)), Expression.Constant(val1, typeof(S)) };
					        Expression<Func<Scs>> e = Expression.Lambda<Func<Scs>>(
					            Expression.New(constructor, exprArgs),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<Scs> f = e.Compile();
					
					        return object.Equals(f(), new Scs(val0, val1));
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
			    public string S;
			
			    public D() {
			    }
			    public D(int val) : this(val, "") {
			    }
			
			    public D(int val, string s) {
			        Val = val;
			        S = s;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val && d.S == S;
			    }
			
			    public override int GetHashCode() {
			        return Val ^ (S == null ? 0 : S.GetHashCode());
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
			        return other.I.Equals(I) && other.D.Equals(D);
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
				
				//-------- Scenario 258
				namespace Scenario258{
					
					public class Test
					{
				    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "int_string_D_New__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
				    public static Expression int_string_D_New__() {
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
					            success = check_int_string_D_New();
					        }
					        finally
					        {
					            Ext.StopCapture();
					        }
					        return success ? 0 : 1;
					    }
					
					    static bool check_int_string_D_New() {
					        int[] svals0 = new int[] { 0, 1, -1, int.MinValue, int.MaxValue };
					        string[] svals1 = new string[] { null, "", "a", "foo" };
					        for (int i = 0; i < svals0.Length; i++) {
					            for (int j = 0; j < svals1.Length; j++) {
					                if (!check_int_string_D_New(svals0[i], svals1[j])) {
					                    Console.WriteLine("int_string_D_New failed");
					                    return false;
					                }
					            }
					        }
					        return true;
					    }
					
					    static bool check_int_string_D_New(int val0, string val1) {
					        ConstructorInfo constructor = typeof(D).GetConstructor(new Type[] { typeof(int), typeof(string) });
					        Expression[] exprArgs = new Expression[] { Expression.Constant(val0, typeof(int)), Expression.Constant(val1, typeof(string)) };
					        Expression<Func<D>> e = Expression.Lambda<Func<D>>(
					            Expression.New(constructor, exprArgs),
					            new System.Collections.Generic.List<ParameterExpression>());
					        Func<D> f = e.Compile();
					
					        return object.Equals(f(), new D(val0, val1));
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
			    public string S;
			
			    public D() {
			    }
			    public D(int val) : this(val, "") {
			    }
			
			    public D(int val, string s) {
			        Val = val;
			        S = s;
			    }
			
			    public override bool Equals(object o) {
			        return o is D && Equals((D) o);
			    }
			
			    public bool Equals(D d) {
			        return d != null && d.Val == Val && d.S == S;
			    }
			
			    public override int GetHashCode() {
			        return Val ^ (S == null ? 0 : S.GetHashCode());
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
			        return other.I.Equals(I) && other.D.Equals(D);
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
