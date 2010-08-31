using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CConvert {
        //Expression.Convert(Expression, Type)
        static public void Convert1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a Convert operation; It will attempt to find a conversion from the 
            //expression to the type specified.
            Expression MyConvert = Expression.Convert(
                                        Expression.Constant(5.5),
                                        typeof(Int16)
                                    );

            //Should print "Int16".
            Console.WriteLine(Expression.Lambda<Func<Type>>(Expression.Call(MyConvert,typeof(object).GetMethod("GetType"))).Compile().Invoke().Name);

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<Type>>(Expression.Call(MyConvert, typeof(object).GetMethod("GetType"))).Compile().Invoke() != typeof(Int16)) throw new Exception("");
        }

        //<Snippet2>
        public static Int16 ConvertMethod(double arg) {
            return (Int16)(arg + 2);
        }
        //</Snippet2>

        //Expression.Convert(Expression, Type, MethodInfo)
        static public void Convert2() {
            //<Snippet2>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a Convert operation; It will use the specified user defined conversion.
            Expression MyConvert = Expression.Convert(
                                        Expression.Constant(5.5),
                                        typeof(Int16),
                                        ((Func<double, Int16>) ConvertMethod).Method
                                    );

            //Should print:
            //7
            //"Int16".
            Console.WriteLine(Expression.Lambda<Func<Type>>(Expression.Call(MyConvert, typeof(object).GetMethod("GetType"))).Compile().Invoke().Name);
            Console.WriteLine(Expression.Lambda<Func<Int16>>(MyConvert).Compile().Invoke());

            //</Snippet2>

            //Validate sample
            if (Expression.Lambda<Func<Type>>(Expression.Call(MyConvert, typeof(object).GetMethod("GetType"))).Compile().Invoke() != typeof(Int16)) throw new Exception("");
            if (Expression.Lambda<Func<Int16>>(MyConvert).Compile().Invoke() != 7) throw new Exception("");
        }
    }
}
