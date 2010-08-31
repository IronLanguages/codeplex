using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CLambda {
        //Expression.Lambda(Expression, IEnumerable<ParameterExpression>)
        static public void Lambda1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll define a parameter to use in the lambda
            ParameterExpression MyParam = Expression.Parameter(typeof(int));

            //This element defines a Lambda. It has an expression for the body, and a set of arguments.
            //The lambda's signature will be inferred from the arguments, and the body's type.
            LambdaExpression MyLambda = Expression.Lambda(
                Expression.Add(
                    MyParam,
                    Expression.Constant(1)
                ),
                new List<ParameterExpression>() { MyParam }
            );


            //The resulting value should be 2
            MyLambda.Compile().DynamicInvoke(1);
            //</Snippet1>

            //Validate sample
            if((int)MyLambda.Compile().DynamicInvoke(1)!=2) throw new Exception ();
        }

        //Expression.Lambda(Expression, ParameterExpression[])
        static public void Lambda2() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll define a parameter to use in the lambda
            ParameterExpression MyParam = Expression.Parameter(typeof(int));

            //This element defines a Lambda. It has an expression for the body, and a set of arguments.
            //The lambda's signature will be inferred from the arguments, and the body's type.
            LambdaExpression MyLambda = Expression.Lambda(
                Expression.Add(
                    MyParam,
                    Expression.Constant(1)
                ),
                new ParameterExpression[] { MyParam }
            );


            //The resulting value should be 2
            MyLambda.Compile().DynamicInvoke(1);
            //</Snippet1>

            //Validate sample
            if ((int)MyLambda.Compile().DynamicInvoke(1) != 2) throw new Exception();
        }

        //Expression.Lambda(Expression, String, IEnumerable<ParameterExpression>)
        static public void Lambda3() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll define a parameter to use in the lambda
            ParameterExpression MyParam = Expression.Parameter(typeof(int));

            //This element defines a Lambda. It has an expression for the body, and a set of arguments.
            //The lambda's signature will be inferred from the arguments, and the body's type.
            //The lambda is also given a name for debugging purposes.
            LambdaExpression MyLambda = Expression.Lambda(
                Expression.Add(
                    MyParam,
                    Expression.Constant(1)
                ),
                "This is my lambda",
                new List<ParameterExpression>() { MyParam }
            );


            //The resulting value should be 2
            MyLambda.Compile().DynamicInvoke(1);
            //</Snippet1>

            //Validate sample
            if ((int)MyLambda.Compile().DynamicInvoke(1) != 2) throw new Exception();
        }

        //Expression.Lambda(Type, Expression, IEnumerable<ParameterExpression>)
        static public void Lambda4() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll define a parameter to use in the lambda
            ParameterExpression MyParam = Expression.Parameter(typeof(int));

            //This element defines a Lambda. It has the signature's type, an expression for the body, 
            //and a set of arguments.
            LambdaExpression MyLambda = Expression.Lambda(
                typeof(Func<int, int>),
                Expression.Add(
                    MyParam,
                    Expression.Constant(1)
                ),
                new List<ParameterExpression>() { MyParam }
            );


            //The resulting value should be 2
            MyLambda.Compile().DynamicInvoke(1);
            //</Snippet1>

            //Validate sample
            if ((int)MyLambda.Compile().DynamicInvoke(1) != 2) throw new Exception();
        }

        //Expression.Lambda(Type, Expression, ParameterExpression[])
        static public void Lambda5() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll define a parameter to use in the lambda
            ParameterExpression MyParam = Expression.Parameter(typeof(int));

            //This element defines a Lambda. It has the signature's type, an expression for the body, 
            //and a set of arguments.
            LambdaExpression MyLambda = Expression.Lambda(
                typeof(Func<int, int>),
                Expression.Add(
                    MyParam,
                    Expression.Constant(1)
                ),
                new ParameterExpression[] { MyParam }
            );


            //The resulting value should be 2
            MyLambda.Compile().DynamicInvoke(1);
            //</Snippet1>

            //Validate sample
            if ((int)MyLambda.Compile().DynamicInvoke(1) != 2) throw new Exception();
        }

        //Expression.Lambda(Type, Expression, ParameterExpression[])
        static public void Lambda6() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll define a parameter to use in the lambda
            ParameterExpression MyParam = Expression.Parameter(typeof(int));

            //This element defines a Lambda. It has the signature's type, an expression for the body, 
            //and a set of arguments.
            //It defines a name for debugging purposes.
            LambdaExpression MyLambda = Expression.Lambda(
                typeof(Func<int, int>),
                Expression.Add(
                    MyParam,
                    Expression.Constant(1)
                ),
                "My lambda",
                new ParameterExpression[] { MyParam }
            );


            //The resulting value should be 2
            MyLambda.Compile().DynamicInvoke(1);
            //</Snippet1>

            //Validate sample
            if ((int)MyLambda.Compile().DynamicInvoke(1) != 2) throw new Exception();
        }

        //Expression.Lambda(Type, Expression, IEnumerable<ParameterExpression>)
        static public void Lambda7() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll define a parameter to use in the lambda
            ParameterExpression MyParam = Expression.Parameter(typeof(int));

            //This element defines a Lambda. It defines the lambda's signature, an expression for the body, 
            //and a set of arguments.
            LambdaExpression MyLambda = Expression.Lambda<Func<int, int>>(
                Expression.Add(
                    MyParam,
                    Expression.Constant(1)
                ),
                new List<ParameterExpression>() { MyParam }
            );


            //The resulting value should be 2
            MyLambda.Compile().DynamicInvoke(1);
            //</Snippet1>

            //Validate sample
            if ((int)MyLambda.Compile().DynamicInvoke(1) != 2) throw new Exception();
        }

        //Expression.Lambda(Type, Expression, ParameterExpression[])
        static public void Lambda8() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll define a parameter to use in the lambda
            ParameterExpression MyParam = Expression.Parameter(typeof(int));

            //This element defines a Lambda. It defines the lambda's signature, an expression for the body, 
            //and a set of arguments.
            LambdaExpression MyLambda = Expression.Lambda<Func<int, int>>(
                Expression.Add(
                    MyParam,
                    Expression.Constant(1)
                ),
                new ParameterExpression[] { MyParam }
            );


            //The resulting value should be 2
            MyLambda.Compile().DynamicInvoke(1);
            //</Snippet1>

            //Validate sample
            if ((int)MyLambda.Compile().DynamicInvoke(1) != 2) throw new Exception();
        }

        //Expression.Lambda(Type, Expression, ParameterExpression[])
        static public void Lambda9() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We'll define a parameter to use in the lambda
            ParameterExpression MyParam = Expression.Parameter(typeof(int));

            //This element defines a Lambda. It defines the lambda's signature, an expression for the body, 
            //and a set of arguments.
            //It defines a name for debugging purposes.
            LambdaExpression MyLambda = Expression.Lambda<Func<int, int>>(
                Expression.Add(
                    MyParam,
                    Expression.Constant(1)
                ),
                "My lambda",
                new ParameterExpression[] { MyParam }
            );


            //The resulting value should be 2
            MyLambda.Compile().DynamicInvoke(1);
            //</Snippet1>

            //Validate sample
            if ((int)MyLambda.Compile().DynamicInvoke(1) != 2) throw new Exception();
        }
    }
}
