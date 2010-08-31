using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CMakeGoto {
        
        //MakeGoto(GotoExpressionKind, LabelTarget, Expression, Type)
        static public void MakeGoto1() {
            //<Snippet1>
            // Add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We need to create a label target to use with the MakeGoto.
            LabelTarget MyLabelTarget = Expression.Label(typeof(int));

            //This expression represents an execution jump to a specific label, with an associated value.
            //It also has an explicit type, in case it is used in an expression that demands a particular type.
            //It can be used to define gotos, continues, breaks and return statements.
            Expression MyMakeGoto = Expression.MakeGoto(
                GotoExpressionKind.Goto,
                MyLabelTarget,
                Expression.Constant(3),
                typeof(short)
            );

            //MakeGoto can be used to move execution to a desired place. It can also carry a value.
            //In this example, we skip over a throw.
            var MyBlock = Expression.Block(
                Expression.Condition(
                    Expression.Constant(false),
                    Expression.Constant((short) 1),
                    MyMakeGoto
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
    }
}
