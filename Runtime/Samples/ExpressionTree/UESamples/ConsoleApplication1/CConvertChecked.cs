using System;
using System.Collections.Generic;
using Microsoft.Scripting.Ast;
using System.Text;

namespace Samples {
    class CConvertChecked {
        //Expression.ConvertChecked(Expression, Type)
        static public void ConvertChecked1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a ConvertChecked operation; It will attempt to find a conversion from the 
            //expression to the type specified. The conversion can overflow.
            Expression MyConvertChecked = Expression.ConvertChecked(
                                        Expression.Constant(5.5),
                                        typeof(Int16)
                                    );

            //Should print "Int16".
            Console.WriteLine(Expression.Lambda<Func<Type>>(Expression.Call(MyConvertChecked,typeof(object).GetMethod("GetType"))).Compile().Invoke().Name);

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<Type>>(Expression.Call(MyConvertChecked, typeof(object).GetMethod("GetType"))).Compile().Invoke() != typeof(Int16)) throw new Exception("");
        }

        //<Snippet2>
        public static Int16 ConvertCheckedMethod(double arg) {
            return (Int16)(arg + 2);
        }
        //</Snippet2>

        //Expression.ConvertChecked(Expression, Type, MethodInfo)
        static public void ConvertChecked2() {
            //<Snippet2>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a user defined ConvertChecked operation; It will use the specified user defined conversion.
            Expression MyConvertChecked = Expression.ConvertChecked(
                                        Expression.Constant(5.5),
                                        typeof(Int16),
                                        ((Func<double, Int16>) ConvertCheckedMethod).Method
                                    );

            //Should print:
            //7
            //"Int16".
            Console.WriteLine(Expression.Lambda<Func<Type>>(Expression.Call(MyConvertChecked, typeof(object).GetMethod("GetType"))).Compile().Invoke().Name);
            Console.WriteLine(Expression.Lambda<Func<Int16>>(MyConvertChecked).Compile().Invoke());

            //</Snippet2>

            //Validate sample
            if (Expression.Lambda<Func<Type>>(Expression.Call(MyConvertChecked, typeof(object).GetMethod("GetType"))).Compile().Invoke() != typeof(Int16)) throw new Exception("");
            if (Expression.Lambda<Func<Int16>>(MyConvertChecked).Compile().Invoke() != 7) throw new Exception("");
        }
    }
}
