using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class COr {
        //Expression.Or(Expression, Expression)
        static public void OrSample1() {
            //<Snippet1>
            //This expression represents an OR operation on the values of its two arguments.
            //Both arguments need to be of the same type, either an integer type or boolean.
            Expression MyOr = Expression.Or(
                Expression.Constant(5),
                Expression.Constant(2)
            );

            //The end result should be seven:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyOr).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyOr).Compile().Invoke() != 7) throw new Exception();
        }

        //Expression.Or(Expression, Expression, MethodInfo)
        //<Snippet2>
        public static int Or(int arg1, Exception arg2) {
            return arg1 | Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>

        public static void OrSample2() {
            //<Snippet2>
            //This expression represents a boolean or bitwise OR operation of two arguments using a user defined operator.
            //The parameters to the OR should be reference convertible to the MethodInfo's arguments.
            Expression MyOr = Expression.Or(
                Expression.Constant(5),
                Expression.Constant(new Exception("2")),
                ((Func<int, Exception, int>)Or).Method
            );

            //The end result should be seven:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyOr).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyOr).Compile().Invoke() != 7) throw new Exception();
        }

        //Expression.OrElse(Expression, Expression)
        static public void OrElseSample1() {
            //<Snippet3>
            //This expression does an OR operation on the values of its two arguments, 
            //but will not evaluate the second if the first is false.
            //Both arguments need to be of boolean type.
            Expression MyOr = Expression.OrElse(
                Expression.Constant(true),
                Expression.Block(
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("OtherHalf")),
                    Expression.Constant(false)
                )
            );

            //The end result should be true.
            //Because the first argument already made MyOr true, the second argument is not evaluated and "OtherHalf" is not printed.
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyOr).Compile().Invoke());
            //</Snippet3>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyOr).Compile().Invoke() != true) throw new Exception();
        }

        //Expression.OrElse(Expression, Expression, MethodInfo)
        //<Snippet4>
        public class OrElseTest {
            public OrElseTest(bool Value) {
                this.Value = Value;
            }
            public bool Value;
            public static OrElseTest OrElse(OrElseTest arg1, OrElseTest arg2) {
                return new OrElseTest(arg1.Value || arg2.Value);
            }
            public static bool operator true(OrElseTest arg1) {
                return arg1.Value;
            }
            public static bool operator false(OrElseTest arg1) {
                return !arg1.Value;
            }
        }
        //</Snippet4>

        public static void OrElseSample2() {
            //<Snippet4>
            //This expression represents a boolean or bitwise OR operation of two arguments using a user defined operator.
            //The parameters to the OR operation should be reference convertible to the MethodInfo's arguments
            Expression MyOr = Expression.OrElse(
                Expression.Constant(new OrElseTest(true)),
                Expression.Constant(new OrElseTest(false)),
                ((Func<OrElseTest, OrElseTest, OrElseTest>)OrElseTest.OrElse).Method
            );

            //The end result should be true:
            Console.WriteLine(Expression.Lambda<Func<bool>>(Expression.Field(MyOr, "Value")).Compile().Invoke());
            //</Snippet4>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(Expression.Field(MyOr, "Value")).Compile().Invoke() != true) throw new Exception();
        }

        //Expression.OrAssign(Expression, Expression)
        public static void OrAssignSample1() {
            //<Snippet5>
            //OrAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(bool), "Variable");

            //Here we initialize the variable with true, then use it in an OrAssign expression.
            //both the OrAssign expression and the variable will have the value true after the
            //tree is executed.
            Expression MyOrAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(false)),
                Expression.OrAssign(
                    Variable,
                    Expression.Constant(true)
                )
            );

            //The result should be true.
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyOrAssign).Compile().Invoke());
            //</Snippet5>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyOrAssign).Compile().Invoke() != true) throw new Exception();
        }

        //Expression.OrAssign(Expression, Expression, MethodInfo)
        //<Snippet6
        public static bool OrAssign(bool arg1, bool arg2) {
            return arg1 || arg2;
        }
        //</Snippet6>

        public static void OrAssignSample2() {
            //<Snippet6>
            //OrAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(bool), "Variable");

            //Here we initialize the variable with true, then use it in an OrAssign expression
            //with a user defined method.
            Expression MyOrAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(false)),
                Expression.OrAssign(
                    Variable,
                    Expression.Constant(true),
                    ((Func<bool, bool, bool>)OrAssign).Method
                )
            );

            //The result value should be true.
            Console.WriteLine(Expression.Lambda<Func<bool>>(MyOrAssign).Compile().Invoke());
            //</Snippet6>

            //validate sample.
            if (Expression.Lambda<Func<bool>>(MyOrAssign).Compile().Invoke() != true) throw new Exception();
        }

        //Expression.OrAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet7>
        public static int OrAssignDouble(double arg1, double arg2) {
            return (int)(arg1 + arg2);
        }
        //</Snippet7>

        public static void OrAssignSample3() {
            //<Snippet7>
            //OrAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(double), "Variable");

            //This overload of OrAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(int), "Parameter 1");

            //Here we initialize the double variable with 1, then use it in an OrAssign expression
            //with a user defined method.
            //Since the return type of the method is int, we need to provide a conversion
            //to the variable's type.
            Expression MyOrAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant((double)1)),
                Expression.OrAssign(
                    Variable,
                    Expression.Constant((double)1),
                    ((Func<double, double, int>)OrAssignDouble).Method,
                    Expression.Lambda<Func<int, double>>(
                        Expression.Convert(Param1, typeof(double)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<double>>(MyOrAssign).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<double>>(MyOrAssign).Compile().Invoke() != 2) throw new Exception();
        }
    }
}
