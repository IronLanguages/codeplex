using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CBreak {
        //Expression.Break(LabelTarget)
        static public void Break1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We need to identify a label that will be used by the break statement and the loop. 
            //This allows a break to go to the end of an enclosing loop if needed.
            LabelTarget Mylabel = Expression.Label();

            //This Expression represents a break to the end of a specified loop, identified by a label
            Expression MyBreak = Expression.Break(Mylabel);

            //A Break statement can appear within a loop statement
            Expression ABreakUse = Expression.Loop(
                MyBreak,
                Mylabel
            );
           
            //Without the break, this loop would loop forever.
            Expression.Lambda<Action>(ABreakUse).Compile().Invoke();

            //</Snippet1>
        }

        //Expression.Break(LabelTarget, Expresssion)
        static public void Break2() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We need to identify a label that will be used by the break statement and the loop. 
            //This allows a break to go to the end of an enclosing loop if needed.
            LabelTarget Mylabel = Expression.Label(typeof(int));

            //This Expression represents a break to the end of a specified loop, identified by a label
            //The value that the loop should return in case this break is used is also specified.
            Expression MyBreak = Expression.Break(Mylabel, Expression.Constant(5));

            //A Break statement should appear within a loop statement
            Expression ABreakUse = Expression.Loop(
                Expression.Block(
                    MyBreak,
                    Expression.Constant(1)
                ),
                Mylabel
            );

            //Without the break, this loop would loop forever. The value of the loop when it breaks is 5
            Console.WriteLine(Expression.Lambda<Func<int>>(ABreakUse).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(ABreakUse).Compile().Invoke() != 5) throw new Exception();
        }

        //Expression.Break(LabelTarget, Type)
        static public void Break3()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We need to identify a label that will be used by the break statement and the loop. 
            //This allows a break to go to the end of an enclosing loop if needed.
            LabelTarget Mylabel = Expression.Label();

            //This Expression represents a break to the end of a specified loop, identified by a label
            //It also has an explicit type, so it can be used in expressions that expect their arguments to have a certain type
            Expression MyBreak = Expression.Break(Mylabel, typeof(int));

            //A Break statement should appear within a loop statement
            Expression ABreakUse = Expression.Loop(
                Expression.Condition(
                    Expression.Constant(false),
                    Expression.Constant(1),
                    MyBreak,
                    typeof(int)
                ),
                Mylabel
            );

            //Without the break, this loop would loop forever.
            Expression.Lambda<Action>(ABreakUse).Compile().Invoke();

            //</Snippet1>
        }

        //Expression.Break(LabelTarget, Expression, Type)
        static public void Break4()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We need to identify a label that will be used by the break statement and the loop. 
            //This allows a break to go to the end of an enclosing loop if needed.
            LabelTarget Mylabel = Expression.Label(typeof(int));

            //This Expression represents a break to the end of a specified loop, identified by a label
            //The value that the loop should return in case this break is used is also specified.
            //It also has an explicit type, so it can be used in expressions that expect their arguments to have a certain type
            Expression MyBreak = Expression.Break(Mylabel, Expression.Constant(5), typeof(int));

            //A Break statement should appear within a loop statement
            Expression ABreakUse = Expression.Loop(
                Expression.Condition(
                    Expression.Constant(true),
                    MyBreak,
                    Expression.Constant(1),
                    typeof(int)
                ),
                Mylabel
            );

            //Without the break, this loop would loop forever. The value of the loop when it breaks is 5
            Console.WriteLine(Expression.Lambda<Func<int>>(ABreakUse).Compile().Invoke());

            //</Snippet1>

            //validate sample.
            if (Expression.Lambda<Func<int>>(ABreakUse).Compile().Invoke() != 5) throw new Exception();
        }

    }
}
