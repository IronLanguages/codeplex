using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CBlock {
        //Expression.Block(Expression[])
        static public void Block1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  


            //This Expression represents a set of sequencial Expressions.
            //It has the value of the last Expression.
            Expression MyBlock = Expression.Block(
                Expression.Call(null, typeof(Console).GetMethod("Write",new Type[]{typeof(String)}),Expression.Constant("Hello")),
                Expression.Call(null, typeof(Console).GetMethod("Write",new Type[]{typeof(String)}),Expression.Constant(" beautiful")),
                Expression.Call(null, typeof(Console).GetMethod("Write",new Type[]{typeof(String)}),Expression.Constant(" and")),
                Expression.Call(null, typeof(Console).GetMethod("Write",new Type[]{typeof(String)}),Expression.Constant(" sunny")),
                Expression.Call(null, typeof(Console).GetMethod("Write",new Type[]{typeof(String)}),Expression.Constant(" World")),
                Expression.Call(null, typeof(Console).GetMethod("WriteLine",new Type[]{typeof(String)}),Expression.Constant("!")),
                Expression.Constant(42)
            );

            //Executing the block should result in a string being printed.
            Console.WriteLine(Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke());

            //</Snippet1>

            //validate snippet
            if (Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke() != 42) throw new Exception();
        }

        //Expression.Block(IEnumerable<Expression>)
        static public void Block2() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //A list that holds a set of instructions we want to run sequencially
            List<Expression> MyList = new List<Expression>();
            MyList.Add(Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant("Hello")));
            MyList.Add(Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" beautiful")));
            MyList.Add(Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" and")));
            MyList.Add(Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" sunny")));
            MyList.Add(Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" World")));
            MyList.Add(Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }), Expression.Constant("!")));
            MyList.Add(Expression.Constant(42));

            //This Expression represents a set of sequencial Expressions.
            //It has the value of the last Expression in the list.
            Expression MyBlock = Expression.Block(
                MyList
            );

            //Executing the block should result in a string being printed.
            Console.WriteLine(Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke());

            //</Snippet1>

            //validate snippet
            if (Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke() != 42) throw new Exception();
        }

        //Expression.Block(Expression,Expression)
        static public void Block3() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  


            //This Expression represents a set of sequencial Expressions.
            //It has the value of the last Expression.
            Expression MyBlock = Expression.Block(
                Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }), Expression.Constant("Hello World")),
                Expression.Constant(42)
            );

            //Executing the block should result in a string being printed.
            Console.WriteLine(Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke());

            //</Snippet1>

            //validate snippet
            if (Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke() != 42) throw new Exception();
        }

        //Expression.Block(Type, Expressions[])
        static public void Block4() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  


            //This Expression represents a set of sequencial Expressions.
            //The type of the block can be specified. 
            //If void, the value of the last expression is not returned as the block's value.
            Expression MyBlock = Expression.Block(
                typeof(void),
                Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant("Hello")),
                Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" beautiful")),
                Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" and")),
                Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" sunny")),
                Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" World")),
                Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }), Expression.Constant("!")),
                Expression.Constant(42)
            );

            //Executing the block should result in a string being printed.
            Expression.Lambda<Action>(
                MyBlock
            ).Compile().Invoke();

            //</Snippet1>

        }

        //Expression.Block(Type, IEnumerable<Expression>)
        static public void Block5() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //A list that holds a set of instructions we want to run sequencially
            List<Expression> MyList = new List<Expression>();
            MyList.Add(Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant("Hello")));
            MyList.Add(Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" beautiful")));
            MyList.Add(Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" and")));
            MyList.Add(Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" sunny")));
            MyList.Add(Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" World")));
            MyList.Add(Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }), Expression.Constant("!")));
            MyList.Add(Expression.Constant(42));

            //This Expression represents a set of sequencial Expressions.
            //The type of the block can be specified. 
            //If void, the value of the last expression is not returned as the block's value.
            Expression MyBlock = Expression.Block(
                typeof(void),
                MyList
            );

            //Executing the block should result in a string being printed.
            Expression.Lambda<Action>(
                MyBlock
            ).Compile().Invoke();

            //</Snippet1>

        }

        //Expression.Block(IEnumerable<ParameterExpression>, Expression[])
        static public void Block6() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  


            //This Expression represents a set of sequencial Expressions.
            //It has the value of the last Expression.
            //Block also defines the scope for variables.
            ParameterExpression var1 = Expression.Variable(typeof(int), "Var 1");
            Expression MyBlock = Expression.Block(
                new ParameterExpression[]{var1},
                Expression.AddAssign(var1,Expression.Constant(1)),
                Expression.AddAssign(var1,Expression.Constant(1)),
                Expression.AddAssign(var1,Expression.Constant(1)),
                Expression.AddAssign(var1,Expression.Constant(1)),
                Expression.AddAssign(var1,Expression.Constant(1)),
                var1
            );

            //Executing the block should result in var having the value 5
            Console.WriteLine(Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke());

            //</Snippet1>

            //validate snippet
            if (Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke() != 5) throw new Exception();
        }

        //Expression.Block(IEnumerable<ParameterExpression>, IEnumerable<Expression>)
        static public void Block7() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //First we'll create a list of Expressions we want to execute sequencially.
            ParameterExpression var1 = Expression.Variable(typeof(int), "Var 1");
            List<Expression> MyList = new List<Expression>{
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.AddAssign(var1, Expression.Constant(1)),
                var1
            };

            //This Expression represents a set of sequencial Expressions.
            //It has the value of the last Expression.
            //Block also defines the scope for variables.
            Expression MyBlock = Expression.Block(
                new ParameterExpression[] { var1 },
                MyList
            );

            //Executing the block should result in var having the value 5
            Console.WriteLine(Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke());

            //</Snippet1>

            //validate snippet
            if (Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke() != 5) throw new Exception();
        }

        //Expression.Block(Expression,Expression,Expression)
        static public void Block8() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  


            //This Expression represents a set of sequencial Expressions.
            //It has the value of the last Expression.
            Expression MyBlock = Expression.Block(
                Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant("Hello")),
                Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }), Expression.Constant(" World")),
                Expression.Constant(42)
            );

            //Executing the block should result in a string being printed.
            Console.WriteLine(Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke());

            //</Snippet1>

            //validate snippet
            if (Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke() != 42) throw new Exception();
        }

        //Expression.Block(Type,IEnumerable<ParameterExpression>, Expression[])
        static public void Block9() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  


            //This Expression represents a set of sequencial Expressions.
            //The resulting type can be defined. If void, The last expression is not returned as the block's value.
            //Block also defines the scope for variables.
            ParameterExpression var1 = Expression.Variable(typeof(int), "Var 1");
            Expression MyBlock = Expression.Block(typeof(void),
                new ParameterExpression[] { var1 },
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) }), var1)
            );

            //Executing the block should result in var having the value 5
            Expression.Lambda<Action>(
                MyBlock
            ).Compile().Invoke();

            //</Snippet1>
        }

        //Expression.Block(Type,IEnumerable<ParameterExpression>, IEnumerable<Expression>)
        static public void Block10() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //First we'll create a list of Expressions we want to execute sequencially.
            ParameterExpression var1 = Expression.Variable(typeof(int), "Var 1");
            List<Expression> MyList = new List<Expression>{
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.AddAssign(var1, Expression.Constant(1)),
                Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[]{typeof(int)}), var1)
            };

            //This Expression represents a set of sequencial Expressions.
            //The resulting type can be defined. If void, The last expression is not returned as the block's value.
            //Block also defines the scope for variables.
            Expression MyBlock = Expression.Block(
                typeof(void),
                new ParameterExpression[] { var1 },
                MyList
            );

            //Executing the block should result in var having the value 5
            Expression.Lambda<Action>(
                MyBlock
            ).Compile().Invoke();

            //</Snippet1>
        }

        //Expression.Block(Expression,Expression,Expression,Expression)
        static public void Block11() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  


            //This Expression represents a set of sequencial Expressions.
            //It has the value of the last Expression.
            Expression MyBlock = Expression.Block(
                Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant("Hello")),
                Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" World")),
                Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }), Expression.Constant("!")),
                Expression.Constant(42)
            );

            //Executing the block should result in a string being printed.
            Console.WriteLine(Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke());

            //</Snippet1>

            //validate snippet
            if (Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke() != 42) throw new Exception();
        }

        //Expression.Block(Expression,Expression,Expression,Expression,Expression)
        static public void Block12() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  


            //This Expression represents a set of sequencial Expressions.
            //It has the value of the last Expression.
            Expression MyBlock = Expression.Block(
                Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant("Hello")),
                Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" my")),
                Expression.Call(null, typeof(Console).GetMethod("Write", new Type[] { typeof(String) }), Expression.Constant(" World")),
                Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }), Expression.Constant("!")),
                Expression.Constant(42)
            );

            //Executing the block should result in a string being printed.
            Console.WriteLine(Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke());

            //</Snippet1>

            //validate snippet
            if (Expression.Lambda<Func<int>>(
                MyBlock
            ).Compile().Invoke() != 42) throw new Exception();
        }

    }
}
