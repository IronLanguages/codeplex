using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CGoto {
        //Goto(LabelTarget)
        static public void Goto1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We need to create a label target to use with the goto.
            LabelTarget MyLabelTarget = Expression.Label();

            //This expression represents an execution jump to a specific label.
            Expression MyGoto = Expression.Goto(
                MyLabelTarget
            );

            //Goto can be used to move execution to a desired place.
            //In this example, we skip over the decrement using the goto.
            var Count = Expression.Parameter(typeof(int));
            var MyBlock = Expression.Block(
                new ParameterExpression[]{Count},
                Expression.Assign(Count, Expression.Constant(0)),
                Expression.PreIncrementAssign(Count),
                MyGoto,
                Expression.PreDecrementAssign(Count),
                Expression.Label(MyLabelTarget ),
                Count
            );

            //The end result should be 1:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyBlock).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyBlock).Compile().Invoke() != 1) throw new Exception();
        }

        //Goto(LabelTarget, Expression)
        static public void Goto2() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We need to create a label target to use with the goto.
            LabelTarget MyLabelTarget = Expression.Label(typeof(int));

            //This expression represents an execution jump to a specific label, with an associated value.
            Expression MyGoto = Expression.Goto(
                MyLabelTarget,
                Expression.Constant(3)
            );

            //Goto can be used to move execution to a desired place. It can also carry a value.
            //In this example, we skip over the throw.
            var MyBlock = Expression.Block(
                MyGoto,
                Expression.Throw(Expression.Constant(new Exception())),
                Expression.Label(
                    MyLabelTarget,
                    Expression.Constant(0)
                )
            );

            //The end result should be 3:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyBlock).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyBlock).Compile().Invoke() != 3) throw new Exception();
        }

        //Goto(LabelTarget, Expression, Type)
        static public void Goto3() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We need to create a label target to use with the goto.
            LabelTarget MyLabelTarget = Expression.Label(typeof(int));

            //This expression represents an execution jump to a specific label, with an associated value.
            //It also has an explicit type, in case it is used in an expression that demands a particular type.
            Expression MyGoto = Expression.Goto(
                MyLabelTarget,
                Expression.Constant(3),
                typeof(short)
            );

            //Goto can be used to move execution to a desired place. It can also carry a value.
            //In this example, we skip over the throw.
            var MyBlock = Expression.Block(
                Expression.Condition(
                    Expression.Constant(false),
                    Expression.Constant((short) 1),
                    MyGoto
                ),
                Expression.Throw(Expression.Constant(new Exception())),
                Expression.Label(
                    MyLabelTarget,
                    Expression.Constant(0)
                )
            );

            //The end result should be 3:
            Console.WriteLine(Expression.Lambda<Func<int>>(MyBlock).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(MyBlock).Compile().Invoke() != 3) throw new Exception();
        }


        //Goto(LabelTarget, Type)
        static public void Goto4() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We need to create a label target to use with the goto.
            LabelTarget MyLabelTarget = Expression.Label();

            //This expression represents an execution jump to a specific label.
            //It also has an explicit type, in case it is used in an expression that demands a particular type.
            Expression MyGoto = Expression.Goto(
                MyLabelTarget,
                typeof(short)
            );

            //Goto can be used to move execution to a desired place. It can also carry a value.
            //In this example, we skip over the throw.
            var MyBlock = Expression.Block(
                Expression.Condition(
                    Expression.Constant(false),
                    Expression.Constant((short) 1),
                    MyGoto
                ),
                Expression.Throw(Expression.Constant(new Exception())),
                Expression.Label(MyLabelTarget),
                Expression.Constant("Got Here!")
            );

            //The end result should be "Got Here!":
            Console.WriteLine(Expression.Lambda<Func<string>>(MyBlock).Compile().Invoke());
            //</Snippet1>

            //validate sample.
            if ((Expression.Lambda<Func<string>>(MyBlock).Compile().Invoke()).CompareTo("Got Here!") != 0) throw new Exception();
        }
    }
}
