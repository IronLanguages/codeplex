#if !CLR2 // inline linq expressions

using System;
#if SILVERLIGHT
using System.Linq.Expressions;
#endif
namespace ExpressionCompiler { 
	
		
		//-------- Scenario 192
		namespace Scenario192{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit001__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit001__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        int rez = Foo<S>();
			        rez = rez - 1;
			
			        if (rez < 0)
			            rez = 1;
			
			        return rez;
			    }
			
			    static int Foo<T>() where T : I, new()
			    {
			        Expression<Func<int, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    int X { get; set; }
			}
			
			struct S : I
			{
			    public int X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 193
		namespace Scenario193{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit002__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit002__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        int rez = Foo<S>();
			        rez = rez - 1;
			
			        if (rez < 0)
			            rez = 1;
			
			        return rez;
			    }
			
			    static int Foo<T>() where T : I, new()
			    {
			        Expression<Func<int, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    int X { get; set; }
			}
			
			public class S : I
			{
			    public int X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 194
		namespace Scenario194{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit003__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit003__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        int rez = Foo<S>();
			        rez = rez - 1;
			
			        if (rez < 0)
			            rez = 1;
			
			        return rez;
			    }
			
			    static int Foo<T>() where T : I, new()
			    {
			        Expression<Func<int, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    int X { get; set; }
			}
			
			interface I2
			{
			    string X { get; set; }
			}
			struct S : I, I2
			{
			    public int X { get; set; }
			
			    string I2.X { get; set; }
			
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 195
		namespace Scenario195{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit004__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit004__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        int rez = Foo<S>();
			        rez = rez - 1;
			
			        if (rez < 0)
			            rez = 1;
			
			        return rez;
			    }
			
			    static int Foo<T>() where T : I, new()
			    {
			        Expression<Func<int, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    int X { get; set; }
			}
			
			interface I2
			{
			    string X { get; set; }
			}
			public class S : I, I2
			{
			    public int X { get; set; }
			
			    string I2.X { get; set; }
			
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 196
		namespace Scenario196{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit005__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit005__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        int rez = Foo<S>();
			        rez = rez - 1;
			
			        if (rez < 0)
			            rez = 1;
			
			        return rez;
			    }
			
			    static int Foo<T>() where T : S, new()
			    {
			        Expression<Func<int, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    int X { get; set; }
			}
			
			public class S : I
			{
			    public int X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 197
		namespace Scenario197{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit006__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit006__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        int rez = Foo<S>();
			        rez = rez - 1;
			
			        if (rez < 0)
			            rez = 1;
			
			        return rez;
			    }
			
			    static int Foo<T>() where T : I, new()
			    {
			        Expression<Func<int, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    int X { get; set; }
			}
			
			interface I2:I
			{
			    string X2 { get; set; }
			}
			public class S : I2
			{
			    public string X2 { get; set; }
			    public int X { get; set; }
			
			}
			//</Code>
		
	}
		
		//-------- Scenario 198
		namespace Scenario198{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_byte__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_byte__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        byte rez = Foo<S>();
			
			        if (rez == 0)
			            return 1;
			        else
			            return 0;
			    }
			
			    static byte Foo<T>() where T : I, new()
			    {
			        Expression<Func<byte, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    byte X { get; set; }
			}
			
			struct S : I
			{
			    public byte X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 199
		namespace Scenario199{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_byte_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_byte_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        byte rez = Foo<S>();
			
			        if (rez == 0)
			            return 1;
			        else 
			            return 0;
			    }
			
			    static byte Foo<T>() where T : I, new()
			    {
			        Expression<Func<byte, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    byte X { get; set; }
			}
			
			public class S : I
			{
			    public byte X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 200
		namespace Scenario200{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_datetime__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_datetime__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        DateTime rez = Foo<S>();
			
			        int output = 1;
			        if ((rez.Year == 2008 && rez.Month == 1 && rez.Day == 1))
			            output--;
			
			        return output;
			    }
			
			    static DateTime Foo<T>() where T : I, new()
			    {
			        Expression<Func<DateTime, T>> e = x => new T { X = x };
			        return e.Compile()(new DateTime(2008, 1, 1)).X;
			    }
			}
			
			interface I
			{
			    DateTime X { get; set; }
			}
			
			struct S : I
			{
			    public DateTime X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 201
		namespace Scenario201{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_datetime_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_datetime_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        DateTime rez = Foo<S>();
			
			
			        int output = 1;
			        if ((rez.Year == 2008 && rez.Month == 1 && rez.Day == 1))
			            output--;
			
			        return output;
			    }
			
			    static DateTime Foo<T>() where T : I, new()
			    {
			        Expression<Func<DateTime, T>> e = x => new T { X = x };
			        return e.Compile()(new DateTime(2008, 1, 1)).X;
			    }
			}
			
			interface I
			{
			    DateTime X { get; set; }
			}
			
			public class S : I
			{
			    public DateTime X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 202
		namespace Scenario202{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_decimal__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_decimal__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        Decimal rez = Foo<S>();
			        rez--;
			
			        if (rez < 0)
			            rez = 1;
			
			        return (int)rez;
			    }
			
			    static Decimal Foo<T>() where T : I, new()
			    {
			        Expression<Func<Decimal, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    Decimal X { get; set; }
			}
			
			struct S : I
			{
			    public Decimal X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 203
		namespace Scenario203{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_decimal_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_decimal_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        Decimal rez = Foo<S>();
			        rez--;
			
			        if (rez < 0)
			            rez = 1;
			
			        return (int)rez;
			    }
			
			    static Decimal Foo<T>() where T : I, new()
			    {
			        Expression<Func<Decimal, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    Decimal X { get; set; }
			}
			
			public class S : I
			{
			    public Decimal X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 204
		namespace Scenario204{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_float__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_float__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        float rez = Foo<S>();
			        rez --;
			
			        if (rez < 0)
			            rez = 1;
			
			        return (int)rez;
			    }
			
			    static float Foo<T>() where T : I, new()
			    {
			        Expression<Func<float, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    float X { get; set; }
			}
			
			struct S : I
			{
			    public float X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 205
		namespace Scenario205{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_float_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_float_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        float rez = Foo<S>();
			        rez --;
			
			        if (rez < 0)
			            rez = 1;
			
			        return (int)rez;
			    }
			
			    static float Foo<T>() where T : I, new()
			    {
			        Expression<Func<float, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    float X { get; set; }
			}
			
			public class S : I
			{
			    public float X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 206
		namespace Scenario206{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_guid__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_guid__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        Guid test = new Guid();
			        Guid rez = Foo<S>();
			
			        if (rez.ToString() == test.ToString())
			            return 1;
			
			        return 0;
			    }
			
			    static Guid Foo<T>() where T : I, new()
			    {
			        Expression<Func<Guid, T>> e = x => new T { X = x };
			        return e.Compile()(Guid.NewGuid()).X;
			    }
			}
			
			interface I
			{
			    Guid X { get; set; }
			}
			
			struct S : I
			{
			    public Guid X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 207
		namespace Scenario207{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_guid_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_guid_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        Guid test = new Guid();
			        Guid rez = Foo<S>();
			
			        if (rez.ToString() == test.ToString())
			            return 1;
			
			        return 0;
			    }
			
			    static Guid Foo<T>() where T : I, new()
			    {
			        Expression<Func<Guid, T>> e = x => new T { X = x };
			        return e.Compile()(Guid.NewGuid()).X;
			    }
			}
			
			interface I
			{
			    Guid X { get; set; }
			}
			
			public class S : I
			{
			    public Guid X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 208
		namespace Scenario208{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_long__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_long__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        long rez = Foo<S>();
			        rez --;
			
			        if (rez < 0)
			            rez = 1;
			
			        return (int)rez;
			    }
			
			    static long Foo<T>() where T : I, new()
			    {
			        Expression<Func<long, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    long X { get; set; }
			}
			
			struct S : I
			{
			    public long X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 209
		namespace Scenario209{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_long_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_long_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        long rez = Foo<S>();
			        rez --;
			
			        if (rez < 0)
			            rez = 1;
			
			        return (int)rez;
			    }
			
			    static long Foo<T>() where T : I, new()
			    {
			        Expression<Func<long, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    long X { get; set; }
			}
			
			public class S : I
			{
			    public long X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 210
		namespace Scenario210{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_byte__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_byte__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        byte? rez = Foo<S>();
			
			        if (rez.Value == 0)
			            return 1;
			        else
			            return 0;
			    }
			
			    static byte? Foo<T>() where T : I, new()
			    {
			        Expression<Func<byte?, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    byte? X { get; set; }
			}
			
			struct S : I
			{
			    public byte? X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 211
		namespace Scenario211{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_byte_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_byte_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        byte? rez = Foo<S>();
			
			        if (rez.Value == 0)
			            return 1;
			        else
			            return 0;
			    }
			
			    static byte? Foo<T>() where T : I, new()
			    {
			        Expression<Func<byte?, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    byte? X { get; set; }
			}
			
			public class S : I
			{
			    public byte? X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 212
		namespace Scenario212{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_datetime__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_datetime__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        DateTime? rez = Foo<S>();
			
			
			        int output = 1;
			        if ((rez.Value.Year == 2008 && rez.Value.Month == 1 && rez.Value.Day == 1))
			            output--;
			
			        return output;
			    }
			
			    static DateTime? Foo<T>() where T : I, new()
			    {
			        Expression<Func<DateTime?, T>> e = x => new T { X = x };
			        return e.Compile()(new DateTime(2008, 1, 1)).X;
			    }
			}
			
			interface I
			{
			    DateTime? X { get; set; }
			}
			
			struct S : I
			{
			    public DateTime? X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 213
		namespace Scenario213{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_datetime_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_datetime_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        DateTime? rez = Foo<S>();
			
			
			        int output = 1;
			        if ((rez.Value.Year == 2008 && rez.Value.Month == 1 && rez.Value.Day == 1))
			            output--;
			
			        return output;
			    }
			
			    static DateTime? Foo<T>() where T : I, new()
			    {
			        Expression<Func<DateTime?, T>> e = x => new T { X = x };
			        return e.Compile()(new DateTime(2008, 1, 1)).X;
			    }
			}
			
			interface I
			{
			    DateTime? X { get; set; }
			}
			
			public class S : I
			{
			    public DateTime? X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 214
		namespace Scenario214{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_decimal__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_decimal__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        Decimal? rez = Foo<S>();
			        rez--;
			
			        if (rez < 0)
			            rez = 1;
			
			        return (int)rez;
			    }
			
			    static Decimal? Foo<T>() where T : I, new()
			    {
			        Expression<Func<Decimal?, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    Decimal? X { get; set; }
			}
			
			struct S : I
			{
			    public Decimal? X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 215
		namespace Scenario215{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_decimal_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_decimal_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        Decimal? rez = Foo<S>();
			        rez--;
			
			        if (rez < 0)
			            rez = 1;
			
			        return (int)rez;
			    }
			
			    static Decimal? Foo<T>() where T : I, new()
			    {
			        Expression<Func<Decimal?, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    Decimal? X { get; set; }
			}
			
			public class S : I
			{
			    public Decimal? X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 216
		namespace Scenario216{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_float__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_float__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        float? rez = Foo<S>();
			        rez--;
			
			        if (rez < 0)
			            rez = 1;
			
			        return (int)rez.Value;
			    }
			
			    static float? Foo<T>() where T : I, new()
			    {
			        Expression<Func<float?, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    float? X { get; set; }
			}
			
			struct S : I
			{
			    public float? X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 217
		namespace Scenario217{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_float_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_float_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        float? rez = Foo<S>();
			        rez--;
			
			        if (rez < 0)
			            rez = 1;
			
			        return (int)rez.Value;
			    }
			
			    static float? Foo<T>() where T : I, new()
			    {
			        Expression<Func<float?, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    float? X { get; set; }
			}
			
			public class S : I
			{
			    public float? X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 218
		namespace Scenario218{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_guid__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_guid__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        Guid test = new Guid();
			        Guid rez = Foo<S>();
			
			        if (rez.ToString() == test.ToString())
			            return 1;
			
			        return 0;
			    }
			
			    static Guid Foo<T>() where T : I, new()
			    {
			        Expression<Func<Guid, T>> e = x => new T { X = x };
			        return e.Compile()(Guid.NewGuid()).X;
			    }
			}
			
			interface I
			{
			    Guid X { get; set; }
			}
			
			struct S : I
			{
			    public Guid X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 219
		namespace Scenario219{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_guid_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_guid_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        Guid test = new Guid();
			        Guid rez = Foo<S>();
			
			        if (rez.ToString() == test.ToString())
			            return 1;
			
			        return 0;
			    }
			
			    static Guid Foo<T>() where T : I, new()
			    {
			        Expression<Func<Guid, T>> e = x => new T { X = x };
			        return e.Compile()(Guid.NewGuid()).X;
			    }
			}
			
			interface I
			{
			    Guid X { get; set; }
			}
			
			public class S : I
			{
			    public Guid X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 220
		namespace Scenario220{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_int__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_int__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        int? rez = Foo<S>();
			        rez--;
			
			        if (rez < 0)
			            rez = 1;
			
			        return rez.Value;
			    }
			
			    static int? Foo<T>() where T : I, new()
			    {
			        Expression<Func<int?, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    int? X { get; set; }
			}
			
			struct S : I
			{
			    public int? X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 221
		namespace Scenario221{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_int_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_int_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        int? rez = Foo<S>();
			        rez--;
			
			        if (rez < 0)
			            rez = 1;
			
			        return rez.Value;
			    }
			
			    static int? Foo<T>() where T : I, new()
			    {
			        Expression<Func<int?, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    int? X { get; set; }
			}
			
			public class S : I
			{
			    public int? X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 222
		namespace Scenario222{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_long__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_long__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        long? rez = Foo<S>();
			        rez--;
			
			        if (rez < 0)
			            rez = 1;
			
			        return (int)rez.Value;
			    }
			
			    static long? Foo<T>() where T : I, new()
			    {
			        Expression<Func<long?, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    long? X { get; set; }
			}
			
			struct S : I
			{
			    public long? X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 223
		namespace Scenario223{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_nullable_long_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_nullable_long_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        long? rez = Foo<S>();
			        rez--;
			
			        if (rez < 0)
			            rez = 1;
			
			        return (int)rez.Value;
			    }
			
			    static long? Foo<T>() where T : I, new()
			    {
			        Expression<Func<long?, T>> e = x => new T { X = x };
			        return e.Compile()(1).X;
			    }
			}
			
			interface I
			{
			    long? X { get; set; }
			}
			
			public class S : I
			{
			    public long? X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 224
		namespace Scenario224{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_string__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_string__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        string val = Foo<S>();
			
			        int rez = int.Parse(val);
			        rez--;
			
			        if (rez < 0)
			            rez = 1;
			
			        return rez;
			    }
			
			    static string Foo<T>() where T : I, new()
			    {
			        Expression<Func<string, T>> e = x => new T { X = x };
			        return e.Compile()("1").X;
			    }
			}
			
			interface I
			{
			    string X { get; set; }
			}
			
			struct S : I
			{
			    public string X { get; set; }
			}
			
			//</Code>
		
	}
		
		//-------- Scenario 225
		namespace Scenario225{
			
			public class Program
			{
		    [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "expressions_objectinit_string_class__", new string[] { "positive", "cslinq", "FullTrustOnly","Pri1" })]
		    public static Expression expressions_objectinit_string_class__() {
		       if(Main() != 0 ) {
		           throw new Exception();
		       } else { 
		           return Expression.Constant(0);
		       }
		    }
			public     static int Main()
			    {
			        string val = Foo<S>();
			
			        int rez = int.Parse(val);
			        rez--;
			
			        if (rez < 0)
			            rez = 1;
			
			        return rez;
			    }
			
			    static string Foo<T>() where T : I, new()
			    {
			        Expression<Func<string, T>> e = x => new T { X = x };
			        return e.Compile()("1").X;
			    }
			}
			
			interface I
			{
			    string X { get; set; }
			}
			
			public class S : I
			{
			    public string X { get; set; }
			}
			
			//</Code>
		
	}
	
}

#endif
