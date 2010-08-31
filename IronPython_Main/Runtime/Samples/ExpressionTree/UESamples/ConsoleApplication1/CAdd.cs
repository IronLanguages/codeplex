using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CAdd {
        //Add(Expression, Expression)
        static public void Add1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This expression adds the values of its two arguments.
            //Both arguments need to be of the same type.
            Expression MyAdd = Expression.Add(
                Expression.Constant(1),
                Expression.Constant(1)
            );

            //The end result should be two:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyAdd).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyAdd).Compile().Invoke() != 2) throw new Exception();
        }

        //Add(Expression, Expression, MethodInfo)
        //<Snippet2>
        public static int Add(int arg1, Exception arg2) {
            return arg1 + Convert.ToInt32(arg2.Message);
        }
        //</Snippet2>


        public static void Add2() {
            //<Snippet2>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents an addition of two arguments using a user defined operator.
            //The parameters to the addition should be reference convertible to the MethodInfo's arguments
            Expression MyAdd = Expression.Add(
                Expression.Constant(1),
                Expression.Constant(new Exception("2")),
                ((Func<int, Exception, int>)Add).Method
            );

            //The end result should be three:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyAdd).Compile().Invoke());
            //</Snippet2>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyAdd).Compile().Invoke() != 3) throw new Exception();
        }


        //AddChecked(Expression, Expression)
        static public void AddChecked1() {
            //<Snippet3>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression adds the values of its two arguments.
            //Both arguments need to be of the same type.
            //If the result is larger than the type of the operation,
            //An OverflowException is thrown.
            Expression MyAdd = Expression.AddChecked(
                Expression.Constant(int.MaxValue),
                Expression.Constant(1)
            );

            //An exception should happen:
            try {
                Expression.Lambda<Func<int>>(MyAdd).Compile().Invoke();
            } catch (OverflowException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet3>

            //validate sample.
            try {
                Expression.Lambda<Func<int>>(MyAdd).Compile().Invoke();
                throw new Exception("Expected Overflow Exception, no exception thrown.");
            } catch (OverflowException) {
            }
        }

        //AddChecked(Expression, Expression, MethodInfo)
        //<Snippet4>
        public static int AddChecked(int arg1, Exception arg2) {
            int res = arg1 + Convert.ToInt32(arg2.Message);
            if (res < arg1 || res < Convert.ToInt32(arg2.Message)) throw new OverflowException();
            return res;
        }
        //</Snippet4>


        public static void AddChecked2() {
            //<Snippet4>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //This expression represents an addition of two arguments using a user defined operator.
            //The parameters to the addition should be reference convertible to the MethodInfo's arguments
            Expression MyAdd = Expression.AddChecked(
                Expression.Constant(int.MaxValue),
                Expression.Constant(new Exception("2")),
                ((Func<int, Exception, int>)AddChecked).Method
            );

            //An exception should happen:
            try {
                Expression.Lambda<Func<int>>(MyAdd).Compile().Invoke();
            } catch (OverflowException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet4>

            //validate sample.
            try {
                Expression.Lambda<Func<int>>(MyAdd).Compile().Invoke();
                throw new Exception("Expected Overflow Exception, no exception thrown.");
            } catch (OverflowException) {
            }
        }


        //AddAssign(Expression, Expression)
        public static void AddAssign1() {
            //<Snippet5>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //AddAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 1, then use it in an AddAssign expression.
            //both the AddAssign expression and the variable will have the value 2 after the
            //tree is executed.
            Expression MyAddAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.AddAssign(
                    Variable,
                    Expression.Constant(1)
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyAddAssign).Compile().Invoke());
            //</Snippet5>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyAddAssign).Compile().Invoke() != 2) throw new Exception();
        }

        //AddAssign(Expression, Expression, MethodInfo)
        //<Snippet6
        public static int AddAssign(int arg1, double arg2) {
            return arg1 + (int)arg2;
        }
        //</Snippet6>

        public static void AddAssign2() {
            //<Snippet6>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //AddAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 1, then use it in an AddAssign expression
            //with a user defined method.            
            Expression MyAddAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.AddAssign(
                    Variable,
                    Expression.Constant((double)1),
                    ((Func<int, double, int>)AddAssign).Method
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyAddAssign).Compile().Invoke());
            //</Snippet6>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyAddAssign).Compile().Invoke() != 2) throw new Exception();
        }

        //AddAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet7>
        public static double AddAssignDouble(int arg1, double arg2) {
            return (double)arg1 + arg2;
        }
        //</Snippet7>

        public static void AddAssign3() {
            //<Snippet7>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //AddAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of AddAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with 1, then use it in an AddAssign expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MyAddAssign = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.AddAssign(
                    Variable,
                    Expression.Constant((double)1),
                    ((Func<int, double, double>)AddAssignDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyAddAssign).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyAddAssign).Compile().Invoke() != 2) throw new Exception();
        }






        //AddAssignChecked(Expression, Expression)
        public static void AddAssignChecked1() {
            //<Snippet8>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //AddAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 1, then use it in an AddAssign expression.
            //both the AddAssign expression and the variable will have the value 2 after the
            //tree is executed.
            Expression MyAddAssignChecked = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.AddAssign(
                    Variable,
                    Expression.Constant(1)
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyAddAssignChecked).Compile().Invoke());
            //</Snippet8>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyAddAssignChecked).Compile().Invoke() != 2) throw new Exception();
        }

        //AddAssignChecked(Expression, Expression, MethodInfo)
        //<Snippet9
        public static int AddAssignChecked(int arg1, double arg2) {
            return arg1 + (int)arg2;
        }
        //</Snippet9>

        public static void AddAssignChecked2() {
            //<Snippet9>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //AddAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //Here we initialize the variable with 1, then use it in an AddAssign expression
            //with a user defined method.            
            Expression MyAddAssignChecked = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.AddAssignChecked(
                    Variable,
                    Expression.Constant((double)1),
                    ((Func<int, double, int>)AddAssignChecked).Method
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyAddAssignChecked).Compile().Invoke());
            //</Snippet9>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyAddAssignChecked).Compile().Invoke() != 2) throw new Exception();
        }

        //AddAssign(Expression, Expression, MethodInfo, LambdaExpression)
        //<Snippet10>
        public static double AddAssignCheckedDouble(int arg1, double arg2) {
            return (double)arg1 + arg2;
        }
        //</Snippet10>

        public static void AddAssignChecked3() {
            //<Snippet10>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;

            //AddAssign requires an assignable expression to be used as the left argument.
            ParameterExpression Variable = Expression.Variable(typeof(int), "Variable");

            //This overload of AddAssign also requires a conversion lambda. This is the 
            //Lambda's parameter
            ParameterExpression Param1 = Expression.Parameter(typeof(double), "Parameter 1");

            //Here we initialize the variable with 1, then use it in an AddAssign expression
            //with a user defined method.
            //Since the return type of the method is double, we need to provide a conversion
            //to the variable's type.
            Expression MyAddAssignChecked = Expression.Block(
                new ParameterExpression[] { Variable },
                Expression.Assign(Variable, Expression.Constant(1)),
                Expression.AddAssignChecked(
                    Variable,
                    Expression.Constant((double)1),
                    ((Func<int, double, double>)AddAssignDouble).Method,
                    Expression.Lambda<Func<double, int>>(
                        Expression.Convert(Param1, typeof(int)),
                        new ParameterExpression[] { Param1 }
                    )
                )
            );

            //The result should be two.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyAddAssignChecked).Compile().Invoke());
            //</Snippet10>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyAddAssignChecked).Compile().Invoke() != 2) throw new Exception();
        }

    }
}
