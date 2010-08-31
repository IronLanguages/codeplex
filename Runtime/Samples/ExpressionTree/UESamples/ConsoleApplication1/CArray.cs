using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;


namespace Samples {
    class CArray {
        //ArrayAccess(Expression, Expression[])
        static public void ArrayAccess1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This variable will hold the array to be accessed
            ParameterExpression MyArray = Expression.Parameter(typeof(int[]), "MyArray");
            //This variable will hold the index we wish to access.
            //More indexes can be defined and used if the array is multidimensional.
            ParameterExpression Index = Expression.Parameter(typeof(int), "Index");
            //This variable will hold the value by which we want to increment the array element.
            ParameterExpression Value = Expression.Parameter(typeof(int), "Value");

            //This expression represents an array access.
            //It can be used for assigning to, or reading from, an array element's value.
            Expression MyArrayAccess = Expression.ArrayAccess(
                MyArray,
                Index
            );

            //This lambda will add a value provided to it to a specified index
            //and will return the new value.
            Expression<Func<int[], int, int, int>> MyLambda = Expression.Lambda<Func<int[], int, int, int>>(
                Expression.Block(
                    Expression.Assign(
                        MyArrayAccess,
                        Expression.Add(MyArrayAccess, Value)
                    ),
                    MyArrayAccess
                ),
                MyArray,
                Index,
                Value
            );

            //Value should be 7 (2 + 5)
            Console.WriteLine(MyLambda.Compile().Invoke(new int[]{1,2,3}, 1, 5));

            //</Snippet1>

            //validate sample.
            if (MyLambda.Compile().Invoke(new int[]{1,2,3}, 1, 5) != 7) throw new Exception();
            if (MyLambda.Compile().Invoke(new int[] { 1, 2, 3 }, 2, 0) != 3) throw new Exception();
        }

        //ArrayAccess(Expression, IEnumerable`1)
        static public void ArrayAccess2() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //This variable will hold the array to be accessed
            ParameterExpression MyArray = Expression.Parameter(typeof(int[]), "MyArray");
            //This variable will hold the index we wish to access.
            ParameterExpression Index = Expression.Parameter(typeof(int), "Index");
            //ArrayAccess can be used to index multiple dimensional arrays, so we 
            //need to provide an object that implements IEnumerable<Expression>.
            List<Expression> Indexes = new List<Expression>();
            Indexes.Add(Index);
            //This variable will hold the value by which we want to increment the array element.
            ParameterExpression Value = Expression.Parameter(typeof(int), "Value");

            //This expression represents an array access.
            //It can be used for assigning to, or reading from, an array element's value.
            Expression MyArrayAccess = Expression.ArrayAccess(
                MyArray,
                Indexes
            );

            //This lambda will add a value provided to it to a specified index
            //and will return the new value.
            Expression<Func<int[], int, int, int>> MyLambda = Expression.Lambda<Func<int[], int, int, int>>(
                Expression.Block(
                    Expression.Assign(
                        MyArrayAccess,
                        Expression.Add(MyArrayAccess, Value)
                    ),
                    MyArrayAccess
                ),
                MyArray,
                Index,
                Value
            );

            //Value should be 7 (2 + 5)
            Console.WriteLine(MyLambda.Compile().Invoke(new int[] { 1, 2, 3 }, 1, 5));

            //</Snippet1>

            //validate sample.
            if (MyLambda.Compile().Invoke(new int[] { 1, 2, 3 }, 1, 5) != 7) throw new Exception();
            if (MyLambda.Compile().Invoke(new int[] { 1, 2, 3 }, 2, 0) != 3) throw new Exception();
        }

        //ArrayIndex(Expression, Expression)
        static public void ArrayAccess3() {
            //<Snippet1>
            // add the following directive to your file:
            // using Microsoft.Scripting.Ast;  

            //This variable will hold the array to be indexed.
            ParameterExpression MyArray = Expression.Parameter(typeof(int[]), "MyArray");
            //This variable will hold the index we wish to access.
            ParameterExpression Index = Expression.Parameter(typeof(int), "Index");
            
            //This expression represents an array access.
            //It can be used for reading an array element's value.
            Expression MyArrayIndex = Expression.ArrayIndex(
                MyArray,
                Index
            );

            //This lambda will read the value of a specified index
            //and will return the new value.
            Expression<Func<int[], int, int>> MyLambda = Expression.Lambda<Func<int[], int, int>>(
                MyArrayIndex,
                MyArray,
                Index
            );

            //Value should be 2
            Console.WriteLine(MyLambda.Compile().Invoke(new int[] { 1, 2, 3 }, 1));

            //</Snippet1>

            //validate sample.
            if (MyLambda.Compile().Invoke(new int[] { 1, 2, 3 }, 1) != 2) throw new Exception();
            if (MyLambda.Compile().Invoke(new int[] { 1, 2, 3 }, 2) != 3) throw new Exception();
        }

        //ArrayIndex(Expression, Expression[])
        static public void ArrayAccess4() {
            //<Snippet1>
            // add the following directive to your file:
            // using Microsoft.Scripting.Ast;  

            //This variable will hold the array to be indexed.
            ParameterExpression MyArray = Expression.Parameter(typeof(int[,]), "MyArray");
            //These variable will hold the indexes we wish to access.
            ParameterExpression Index1 = Expression.Parameter(typeof(int), "Index 1");
            ParameterExpression Index2 = Expression.Parameter(typeof(int), "Index 2");

            //This expression represents an array access.
            //It can be used for reading an array element's value.
            Expression MyArrayIndex = Expression.ArrayIndex(
                MyArray,
                Index1,
                Index2
            );

            //This lambda will read the value of a specified index
            //and will return the new value.
            Expression<Func<int[,], int, int, int>> MyLambda = Expression.Lambda<Func<int[,], int, int, int>>(
                MyArrayIndex,
                MyArray,
                Index1,
                Index2
            );

            //Value should be 6
            Console.WriteLine(MyLambda.Compile().Invoke(new int[,] { {1, 2, 3} , {4, 5, 6}}, 1,2));

            //</Snippet1>

            //validate sample.
            if (MyLambda.Compile().Invoke(new int[,] { { 1, 2, 3 }, { 4, 5, 6 } }, 1, 1) != 5) throw new Exception();
            if (MyLambda.Compile().Invoke(new int[,] { { 1, 2, 3 }, { 4, 5, 6 } }, 0, 1) != 2) throw new Exception();
        }

        //ArrayIndex(Expression, IEnumerable`1)
        static public void ArrayAccess5() {
            //<Snippet1>
            // add the following directive to your file:
            // using Microsoft.Scripting.Ast;  

            //This variable will hold the array to be indexed.
            ParameterExpression MyArray = Expression.Parameter(typeof(int[,]), "MyArray");
            //These variable will hold the indexes we wish to access.
            ParameterExpression Index1 = Expression.Parameter(typeof(int), "Index 1");
            ParameterExpression Index2 = Expression.Parameter(typeof(int), "Index 2");
            //This overload requires an IEnumerable<Expression> to hold the indexes
            List<Expression> Indexes = new List<Expression>();
            Indexes.Add(Index1);
            Indexes.Add(Index2);

            //This expression represents an array access.
            //It can be used for reading an array element's value.
            Expression MyArrayIndex = Expression.ArrayIndex(
                MyArray,
                Indexes
            );

            //This lambda will read the value of a specified index
            //and will return the new value.
            Expression<Func<int[,], int, int, int>> MyLambda = Expression.Lambda<Func<int[,], int, int, int>>(
                MyArrayIndex,
                MyArray,
                Index1,
                Index2
            );

            //Value should be 6
            Console.WriteLine(MyLambda.Compile().Invoke(new int[,] { { 1, 2, 3 }, { 4, 5, 6 } }, 1, 2));

            //</Snippet1>

            //validate sample.
            if (MyLambda.Compile().Invoke(new int[,] { { 1, 2, 3 }, { 4, 5, 6 } }, 1, 1) != 5) throw new Exception();
            if (MyLambda.Compile().Invoke(new int[,] { { 1, 2, 3 }, { 4, 5, 6 } }, 0, 1) != 2) throw new Exception();
        }

        //ArrayLength(Expression)
        static public void ArrayAccess6() {
            //<Snippet1>
            // add the following directive to your file:
            // using Microsoft.Scripting.Ast;  

            //This variable will hold the array.
            ParameterExpression MyArray = Expression.Parameter(typeof(int[]), "MyArray");
            
            //This expression represents reading an array's length.
            Expression MyArrayLength = Expression.ArrayLength(
                MyArray
            );

            //This lambda will return the length of an array:
            Expression<Func<int[], int>> MyLambda = Expression.Lambda<Func<int[], int>>(
                MyArrayLength,
                MyArray
            );

            //Value should be 3
            Console.WriteLine(MyLambda.Compile().Invoke(new int[] { 1, 2, 3 }));

            //</Snippet1>

            //validate sample.
            if (MyLambda.Compile().Invoke(new int[] { 1, 2, 3 }) != 3) throw new Exception();

        }

        // NewArrayInit(Type, Expression[])
        public static void NewArrayInitSample1() {
            //<Snippet7>
            // This expression represents initializing a new array of the specified type with the provided initializers.
            Expression MyArray =
                Expression.NewArrayInit(
                    typeof(int),
                    new Expression[] { Expression.Constant(1), Expression.Constant(2), Expression.Constant(3) }
                );

            Expression MyArrayAccess = Expression.ArrayAccess(MyArray, Expression.Constant(2));

            // This will print the value of the 2nd element in the array.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyArrayAccess).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyArrayAccess).Compile().Invoke() != 3) throw new Exception();
        }

        // NewArrayInit(Type, IEnumerable'1)
        public static void NewArrayInitSample2() {
            //<Snippet8>
            // This expression represents initializing a new array of the specified type with the provided initializers.
            Expression MyArray =
                Expression.NewArrayInit(
                    typeof(int),
                    new List<Expression>() { Expression.Constant(1), Expression.Constant(2), Expression.Constant(3) }
                );

            Expression MyArrayAccess = Expression.ArrayAccess(MyArray, Expression.Constant(2));

            // This will print the value of the 2nd element in the array.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyArrayAccess).Compile().Invoke());
            //</Snippet8>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyArrayAccess).Compile().Invoke() != 3) throw new Exception();
        }

        // NewArrayBounds(Type, Expression[])
        public static void NewArrayBoundsSample1() {
            //<Snippet7>
            // This expression represents initializing a new multidimensional array of the specified rank and type;
            // in this case, a multidimensional array of integers with 2 rows and 3 columns.
            NewArrayExpression MyArray =
                Expression.NewArrayBounds(
                    typeof(int),
                    new Expression[] { Expression.Constant(2), Expression.Constant(3) }
                );

            Expression Expr =
                Expression.Block(
                    // This assigns the value of 5 to index [1,2] in the array.
                    Expression.Assign(
                        Expression.ArrayAccess(MyArray, new Expression[] { Expression.Constant(1), Expression.Constant(2) }),
                        Expression.Constant(5)
                    )
                );

            // The result of the assignment expression is 5
            Console.WriteLine(Expression.Lambda<Func<int>>(Expr).Compile().Invoke());
            //</Snippet7>

            //validate sample.
            if (Expression.Lambda<Func<int>>(Expr).Compile().Invoke() != 5) throw new Exception();
        }

        // NewArrayBounds(Type, Expression[])
        public static void NewArrayBoundsSample2() {
            //<Snippet8>
            // This expression represents initializing a new multidimensional array of the specified rank and type;
            // in this case, a multidimensional array of integers with 2 rows and 3 columns.
            NewArrayExpression MyArray =
                Expression.NewArrayBounds(
                    typeof(int),
                    new List<Expression>() { Expression.Constant(2), Expression.Constant(3) }
                );

            Expression Expr =
                Expression.Block(
                    // This assigns the value of 5 to index [1,2] in the array.
                    Expression.Assign(
                        Expression.ArrayAccess(MyArray, new Expression[] { Expression.Constant(1), Expression.Constant(2) }),
                        Expression.Constant(5)
                    )
                );

            // The result of the assignment expression is 5
            Console.WriteLine(Expression.Lambda<Func<int>>(Expr).Compile().Invoke());
            //</Snippet8>

            //validate sample.
            if (Expression.Lambda<Func<int>>(Expr).Compile().Invoke() != 5) throw new Exception();
        }
    }
}
