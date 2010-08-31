#if !CLR2 // inline linq expressions
using System.Linq.Expressions;
using System;

namespace ExpressionCompiler { 
	
		//-------- Scenario 3013
		namespace Scenario3013{
			
			public  class BaseClass
			{
			}
			
			public  class Test
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "test1__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression test1__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public      static int Main()
			    {
			        bool res = Test1();
			        return res ? 0 : 1;
			    }
			
			    public static bool Test1()
			    {
			        Expression<Func<object[]>> expr = () => (object[])new BaseClass[1];
			        expr.Compile();
			        return true;
			    }
			}
			
		
	}
		
		//-------- Scenario 3014
		namespace Scenario3014{
			
			public  class Test
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "test2__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression test2__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public      static int Main()
			    {
			        bool res = Test2();
			        return res ? 0 : 1;
			    }
			
			    public static bool Test2()
			    {
			        Expression<Func<int,object>> x = c => new double[c,c];
			        if (!x.ToString().Equals("c => new System.Double[,](c, c)"))
			        {
			            return false;
			        }
			        object y = x.Compile()(2);
			        if (!y.ToString().Equals("System.Double[,]"))
			        {
			            return false;        
			        }
			        return true;
			    }
			}
			
		
	}
	
}
#endif
