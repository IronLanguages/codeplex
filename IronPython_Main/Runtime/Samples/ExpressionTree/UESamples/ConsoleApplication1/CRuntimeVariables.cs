using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CRuntimeVariables {
        //RuntimeVariables(ParameterExpression[])
        static public void RuntimeVariables1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            ParameterExpression MyVariable = Expression.Variable(typeof(int));

            //This expression allows one to evaluate variables used in an expression tree
            //from outside of that expression tree.
            RuntimeVariablesExpression MyRuntimeVariables = Expression.RuntimeVariables(
                MyVariable
            );

            var MyBlock = Expression.Block(
                new ParameterExpression[] { MyVariable },
                Expression.Assign(MyVariable, Expression.Constant(43)),
                MyRuntimeVariables
            );


            //The end result should be 43:
            Console.WriteLine(Expression.Lambda<Func<IRuntimeVariables>>(MyBlock).Compile().Invoke()[0]);

            //validate sample.
            if ((int)Expression.Lambda<Func<IRuntimeVariables>>(MyBlock).Compile().Invoke()[0] != 43) throw new Exception();
        }



        //RuntimeVariables(IEnumerable<ParameterExpression>)
        static public void RuntimeVariables2() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            ParameterExpression MyVariable = Expression.Variable(typeof(int));

            //This expression allows one to evaluate variables used in an expression tree
            //from outside of that expression tree.
            RuntimeVariablesExpression MyRuntimeVariables = Expression.RuntimeVariables(
                new List<ParameterExpression>(){MyVariable}
            );

            var MyBlock = Expression.Block(
                new ParameterExpression[] { MyVariable },
                Expression.Assign(MyVariable, Expression.Constant(43)),
                MyRuntimeVariables
            );


            //The end result should be 43:
            Console.WriteLine(Expression.Lambda<Func<IRuntimeVariables>>(MyBlock).Compile().Invoke()[0]);

            //validate sample.
            if ((int)Expression.Lambda<Func<IRuntimeVariables>>(MyBlock).Compile().Invoke()[0] != 43) throw new Exception();
        }
    }
}
