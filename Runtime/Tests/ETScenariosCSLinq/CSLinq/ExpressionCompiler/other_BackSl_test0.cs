#if !CLR2 // inline linq expressions
using System.Linq.Expressions;

using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Collections.Generic;
using System;
namespace ExpressionCompiler { 
	
			
			//-------- Scenario 169
			namespace Scenario169{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01a__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB63612();
				 return success ? 0 : 1;
				
				}
				
				  public static bool DDB63612()
				    {
				        ParameterExpression i = Expression.Parameter(typeof(int?), "i");
				        var e = Expression.Lambda<Func<int?, C>>(Expression.Convert(i, typeof(C), typeof(C).GetMethod("op_Implicit")), i);
				        // e = i => (C) i;
				        var f = e.Compile();
				        try
				        {
				            f(null);
				            return false;
				        }
				        catch (Exception ex)
				        {
				            bool res = ex.GetType().Equals(typeof(InvalidOperationException));
				            return res;
				        }
				    }
				
				}
			
		
	}
			
			//-------- Scenario 170
			namespace Scenario170{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01b__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01b__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB72413();
				 return success ? 0 : 1;
				
				}
				
				  
				    public static bool DDB72413()
				    {
				        int? x = 10;
				        int? y = 2;
				        ParameterExpression p1 = Expression.Parameter(typeof(int?), "x");
				        ParameterExpression p2 = Expression.Parameter(typeof(int?), "y");
				        Expression<Func<int?, int?, bool?>> e = Expression.Lambda<Func<int?, int?, bool?>>(
				            Expression.GreaterThan(p1, p2, true, null), new ParameterExpression[] { p1, p2 });
				        //Expression.Convert(Expression.GreaterThan(p1, p2, true, null), typeof(bool?)), new ParameterExpression[] { p1, p2 });
				        var f = e.Compile();
				        var r = f(x, y);
				        return r.Value;
				    }
				}
			
		
	}
			
			//-------- Scenario 171
			namespace Scenario171{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01c__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01c__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB78784();
				 return success ? 0 : 1;
				
				}
				
				    public static bool DDB78784()
				    {
				        int? n = 10;
				
				        Expression<Func<bool?>> e = Expression.Lambda<Func<bool?>>(
				            Expression.NotEqual(
				                Expression.Constant(n, typeof(int?)),
				                Expression.Convert(Expression.Constant(null, typeof(Object)), typeof(int?)),
				                true,
				                null),
				            null);
				        var f = e.Compile();
				        var r = f();
				        return r == null;
				    }
				}
			
		
	}
			
			//-------- Scenario 172
			namespace Scenario172{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01d__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01d__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB82012();
				 return success ? 0 : 1;
				
				}
				 public struct A
				    {
				        public int x;
				        public static implicit operator A(Decimal y)
				        {
				            return new A();
				        }
				    }
				
				    public static bool DDB82012()
				    {
				        A? a = new A();
				        var t = typeof(A);
				        var mi = t.GetMethod("op_Implicit");
				        var td = typeof(decimal);
				        var mid = td.GetMethod("op_Implicit", new Type[] { typeof(int) });
				        byte? b = 10;
				        ParameterExpression p = Expression.Parameter(typeof(byte?), "X");
				        Expression<Func<A?>> e =
				            Expression.Lambda<Func<A?>>(
				            Expression.Coalesce(
				            Expression.Constant(b, typeof(byte?)),
				            Expression.Constant(a, typeof(A?)),
				            Expression.Lambda<Func<byte?, A?>>(
				                Expression.Convert(
				                    Expression.Convert(
				                        Expression.Convert(
				                            p,
				                            typeof(int?)
				                        ),
				                        typeof(decimal?),
				                        mid
				                    ),
				                    typeof(A?),
				                    mi
				                ),
				                new ParameterExpression[] { p })
				                ),
				                null);
				
				        var f = e.Compile();
				        var r = f();
				        return r.Value.x == 0;
				    }
				  
				}
			
		
	}
			
			//-------- Scenario 173
			namespace Scenario173{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01e__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01e__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB94699();
				 return success ? 0 : 1;
				
				}
				  public static int? i;
				
				    public static bool DDB94699()
				    {
				        Expression<Func<string>> expr = () => i.ToString();
				        Func<string> f = expr.Compile();
				        var r = f();
				        return r == "";
				    }
				
				  
				}
			
		
	}
			
			//-------- Scenario 174
			namespace Scenario174{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01f__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01f__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB94699a();
				 return success ? 0 : 1;
				
				}
				  public static int? i;
				
				  public static bool DDB94699a()
				    {
				        Expression<Func<string>> expr = Expression.Lambda<Func<string>>(
				            Expression.Call(
				                Expression.Field(null, typeof(C).GetField("i")), typeof(int?).GetMethod("ToString")));
				        Func<string> f = expr.Compile();
				        return f() == "";
				    }
				
				  
				}
			
		
	}
			
			//-------- Scenario 175
			namespace Scenario175{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01g__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01g__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB94699b();
				 return success ? 0 : 1;
				
				}
				  public static int? i;
				
				   public static bool DDB94699b()
				    {
				        Expression<Func<string>> expr = Expression.Lambda<Func<string>>(
				 Expression.Call(
				     Expression.Field(null, typeof(Test).GetField("i")),
				     typeof(object).GetMethod("ToString")));
				        Func<string> f = expr.Compile();
				        return f() == "";
				    }
				
				  
				}
			
		
	}
			
			//-------- Scenario 176
			namespace Scenario176{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01h__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01h__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB94699c();
				 return success ? 0 : 1;
				
				}
				  public static int? i;
				
				    public static bool DDB94699c()
				    {
				        Expression<Func<string>> e = Expression.Lambda<Func<string>>(Expression.Call(
				           Expression.Field(null, typeof(C).GetField("i")), typeof(ValueType).GetMethod("ToString")));
				        Func<string> f = e.Compile();
				        return f() == "";
				    }
				
				  
				}
			
		
	}
			
			//-------- Scenario 177
			namespace Scenario177{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01i__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01i__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB71350();
				 return success ? 0 : 1;
				
				}
				     static int p = 1;
				
				    public static int P { get { return p; } set { p = value; } }
				
				    public static int M(ref int i)
				    {
				        return i = 0;
				    }
				
				    static bool DDB71350()
				    {
				        Expression<Func<int>> e =
				            Expression.Lambda<Func<int>>(
				                Expression.Call(typeof(Test).GetMethod("M"), new[] { Expression.Property(null, typeof(Test).GetProperty("P")) }),
				                null);
				        int result = (e.Compile())();
				        return result == 0;
				    }
				
				  
				}
			
		
	}
			
			//-------- Scenario 178
			namespace Scenario178{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01j__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01j__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB43375();
				 return success ? 0 : 1;
				
				}
				    
				 static bool DDB43375()
				    {
				        Expression<Action> e = Expression.Lambda<Action>(Expression.Constant(0, typeof(int)));
				        Action f = e.Compile();
				        f();
				        return true;
				    }
				
				  
				}
			
		
	}
			
			//-------- Scenario 180
			namespace Scenario180{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01l__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01l__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB124819();
				 return success ? 0 : 1;
				
				}
				
				    static bool DDB124819()
				    {
				        Expression<Func<float>> e =
				            Expression.Lambda<Func<Single>>(Expression.AddChecked(Expression.Constant(1.5F, typeof(float)), (Expression.Constant(1.5F, typeof(float)))));
				        Func<float> f = e.Compile();
				        float x = f();
				        return x == 3.0f;
				    }
				  
				}
			
		
	}
			
			//-------- Scenario 181
			namespace Scenario181{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01m__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01m__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB128358();
				 return success ? 0 : 1;
				
				}
				public static int Inc(ref int i)
				    {
				        ++i;
				        return i;
				    }
				    public static int Seq(int a, int b)
				    {
				        return b;
				    }
				
				    static bool DDB128358()
				    {
				        Func<int, Func<int>[]> i1 = a => new Func<int>[] {
				            (Func<int>)(()=>Inc(ref a)),
				            (Func<int>)(()=>Seq(Inc(ref a), Inc(ref a)))};
				        Func<int>[] fs1 = i1(100);
				        Func<int> first1 = fs1[0];
				        Func<int> second1 = fs1[1];
				        var l0 = new[] { 101, 102, 104, 106, 107, 108 };
				        var l1 = new [] {
				                            first1(),
				                            first1(),
				                            second1(),
				                            second1(),
				                            first1(),
				                            first1()
				                        };
				
				        // Expression trees, no quotes -- OK
				        Expression<Func<int, Func<int>[]>> expr4 = a => new Func<int>[] {
				            (Func<int>)(()=>Inc(ref a)),
				            (Func<int>)(()=>Seq(Inc(ref a), Inc(ref a)))};
				        Func<int, Func<int>[]> i4 = expr4.Compile();
				        Func<int>[] fs4 = i4(100);
				        Func<int> first4 = fs4[0];
				        Func<int> second4 = fs4[1];
				        var l2 = new [] {
				                            first4(),
				                            first4(),
				                            second4(),
				                            second4(),
				                            first4(),
				                            first4()
				                        };
				
				        // Quoted expression trees -- BAD
				        Expression<Func<int, Expression<Func<int>>[]>> expr8 = a => new Expression<Func<int>>[] {
				            (Expression<Func<int>>)(()=>Inc(ref a)),
				            (Expression<Func<int>>)(()=>Seq(Inc(ref a), Inc(ref a)))};
				        Func<int, Expression<Func<int>>[]> i8 = expr8.Compile();
				        Expression<Func<int>>[] fs8 = i8(100);
				        Func<int> first8 = fs8[0].Compile();
				        Func<int> second8 = fs8[1].Compile();
				        var l3 = new [] {
				                            first8(),
				                            first8(),
				                            second8(),
				                            second8(),
				                            first8(),
				                            first8()
				                        };
				
				        bool ok = true;
				        for (int i = 0; i < l0.Length; i++)
				        {
				            ok = ok && (l0[i] == l1[i]) && (l1[i] == l2[i]) && (l2[i] == l3[i]);
				        }
				        return ok;
				    }
				}
			
		
	}
			
			//-------- Scenario 182
			namespace Scenario182{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01n__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01n__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB127108();
				 return success ? 0 : 1;
				
				}
				
				static bool DDB127108()
				    {
				        var l0 = new[] { true, false, true };
				        var l1 = new bool[3];
				        Expression<Func<bool, Func<bool>>> expr = cond => () => cond;
				        var identity = expr.Compile();
				        var _true = identity(true);
				        l1[0] = _true();
				        var _false = identity(false);
				        l1[1] = _false();
				        l1[2] = _true();
				        bool ok = true;
				        for (int i = 0; i < l0.Length; i++)
				        {
				            ok = ok && l0[i] == l1[i];
				        }
				        return ok;
				    }
				
				}
			
		
	}
			
			//-------- Scenario 183
			namespace Scenario183{
				public  class C
				{
				    public static int? i;
				
				    public static implicit operator C(int i)
				    {
				        return new C();
				    }
				}
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other01o__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other01o__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				  bool success =
				            DDB139510();
				 return success ? 0 : 1;
				
				}
				  public static bool DDB139510()
				    {
				        try
				        {
				            Expression<Func<DayOfWeek>> e =
				                    Expression.Lambda<Func<DayOfWeek>>(
				                        Expression.Field(null, typeof(DayOfWeek).GetField("Sunday"))
				                    );
				
				            Func<DayOfWeek> f = e.Compile();  // throws NotSupportedException
				            DayOfWeek result = f();
				        }
				        catch (NotSupportedException)
				        {
				
				            return false;
				
				        }
				
				        return true;
				    }
				
				}
			
		
	}
	
}
#endif
