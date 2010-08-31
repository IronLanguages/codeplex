using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CVariable {
        // Variable(Type, String)
        public static void VariableSample() {
            //<Snippet1>
            // This expression defines a ParameterExpression representing a local variable .
            // The variable name in the second argument is for debugging convenience only, the compiler ignores it.
            ParameterExpression MyVariable = Expression.Variable(typeof(int), "MyVar");

            Expression Tree =
                // The first argument for the Block describes the local variables in scope for this Expression.
                // Without the Block defining the scoped variables executing the LambdaExpression below would throw an exception.
                Expression.Block(
                    new[] { MyVariable },
                    Expression.Assign(MyVariable, Expression.Constant(1))
                );

            Console.WriteLine(Expression.Lambda<Func<int>>(Tree).Compile().Invoke());
            //</Snippet1>

            // validate sample
            if (Expression.Lambda<Func<int>>(Tree).Compile().Invoke() != 1)
                throw new Exception("VariableSample failed");
        }

        //<Snippet2>
        public delegate int TestDelegate(ref int arg1);

        // Parameter(Type, String)
        public static void ParameterSample() {
            //<Snippet1>
            // This expression defines a ParameterExpression.
            // Unlike Expression.Variable the Parameter factory allows the creation of ByRef parameters.
            // The variable name in the second argument is for debugging convenience only, the compiler ignores it.
            ParameterExpression MyVariable = Expression.Parameter(typeof(int).MakeByRefType(), "MyVar2");

            var MyLambda = Expression.Lambda<TestDelegate>(
                    Expression.Assign(MyVariable, Expression.Add(MyVariable, Expression.Constant(2))),
                    new ParameterExpression[] { MyVariable }
                );

            var MyFunc = MyLambda.Compile();
            int x = 0;
            
            MyFunc(ref x);
            MyFunc(ref x);

            // x is now equal to 4
            Console.WriteLine(x);
            //</Snippet1>

            // validate sample
            if (x != 4)
                throw new Exception("ParameterSample failed");
        }
    }
}
