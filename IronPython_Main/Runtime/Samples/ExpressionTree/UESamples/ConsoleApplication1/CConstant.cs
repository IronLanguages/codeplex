using System;
using System.Collections.Generic;
using Microsoft.Scripting.Ast;
using System.Text;

namespace Samples {
    class CConstant {
        //Expression.Constant(Object)
        static public void Constant1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a Constant value.
            Expression MyConstant = Expression.Constant(
                                        5.5
                                    );

            //Should print "5.5".
            Console.WriteLine(Expression.Lambda<Func<double>>(MyConstant).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<double>>(MyConstant).Compile().Invoke() != 5.5) throw new Exception("");
        }

        //Expression.Constant(Object, Type)
        static public void Constant2() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This Expression represents a Constant value.
            //The type can explicitly be given. This can be used, for example, for defining constants of a nullable type.
            Expression MyConstant = Expression.Constant(
                                        5.5,
                                        typeof(double?)
                                    );

            //Should print "5.5".
            Console.WriteLine(Expression.Lambda<Func<double?>>(MyConstant).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<double?>>(MyConstant).Compile().Invoke() != 5.5) throw new Exception("");
        }
    }
}
