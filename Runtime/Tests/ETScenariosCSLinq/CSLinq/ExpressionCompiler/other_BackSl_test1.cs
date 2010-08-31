#if !CLR2 // inline linq expressions

using System.Security.Permissions;
using System.Text;
using System.Collections.Generic;
using System;
#if SILVERLIGHT
using System.Linq.Expressions;
#endif

namespace ExpressionCompiler { 
	
			
			//-------- Scenario 186
			namespace Scenario186{
				
				
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other02a__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other02a__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				        bool success =
				            DDB132062();
					return success ? 0 : 1;
				}
				
				 static int _DDB132062(out int x)
				    {
				        x = 2;
				        return 3;
				    }
				static bool DDB132062()
				    {
				        Expression<Func<int[,], int>> ex = x => _DDB132062(out x[0, 0]);
				        int[,] y = { { 1 } };
				        var z = ex.Compile()(y);
				        bool result = y[0, 0] == 2;
				        return result;
				    }
				}
			
		
	}
			
			//-------- Scenario 187
			namespace Scenario187{
				
				
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other02b__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other02b__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				        bool success =
				            DDB99393();
					return success ? 0 : 1;
				}
				
				public   class B
				    {
				        public bool value;
				        public B(bool value) { this.value = value; }
				        public static B operator &(B a1, B a2) { return new B(a1.value && a2.value); }
				        public static B operator |(B a1, B a2) { return new B(a1.value || a2.value); }
				        public static bool operator true(B a) { return a.value; }
				        public static bool operator false(B a) { return !a.value; }
				        public static B operator !(B a) { return new B(false); }
				        public override string ToString() { return value.ToString(); }
				    }
				
				    static bool DDB99393()
				    {
				        B b1 = new B(true);
				        Expression<Func<B>> e1 = () => b1 && !b1;
				        B r1 = e1.Compile()();
				        return r1.value == false;
				    }
				}
			
		
	}
			
			//-------- Scenario 188
			namespace Scenario188{
				
				
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other02c__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other02c__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				        bool success =
				            DDB99393a();
					return success ? 0 : 1;
				}
				
				public   class B
				    {
				        public bool value;
				        public B(bool value) { this.value = value; }
				        public static B operator &(B a1, B a2) { return new B(a1.value && a2.value); }
				        public static B operator |(B a1, B a2) { return new B(a1.value || a2.value); }
				        public static bool operator true(B a) { return a.value; }
				        public static bool operator false(B a) { return !a.value; }
				        public static B operator !(B a) { return new B(false); }
				        public override string ToString() { return value.ToString(); }
				    }
				
				   
				
				    static bool DDB99393a()
				    {
				        B b1 = new B(true);
				        Expression<Func<B>> e1 = () => b1 || !b1;
				        B r1 = e1.Compile()();
				        return r1.value == true;
				    }
				}
			
		
	}
			
			//-------- Scenario 189
			namespace Scenario189{
				
				
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other02d__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other02d__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				        bool success =
				            DDB114275();
					return success ? 0 : 1;
				}
				
				    static readonly int val = 0;
				
				    static bool DDB114275()
				    {
				        Expression<Func<string>> expr = () => val.ToString();
				        Func<string> f1 = expr.Compile();
				        string res = f1();
				        return res.Equals("0");
				    }
				
				}
			
		
	}
			
			//-------- Scenario 191
			namespace Scenario191{
				
				
				
				public  class Test
				{
			    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "other02f__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
			    public static Expression other02f__() {
			       if(Main() != 0 ) {
			           throw new Exception();
			       } else { 
			           return Expression.Constant(0);
			       }
			    }
				public      static int Main()
				    {
				        bool success =
				            DDB144293();
					return success ? 0 : 1;
				}
				
				      static void Foo(ref string s) { }
				
				    static bool DDB144293()
				    {
				        var arr = new string[] { null };
				        Expression<Action> ex = () => Foo(ref arr[0]);
				        ex.Compile()();
				        return true;
				    }
				
				
				}
			
		
	}
	
}
#endif
