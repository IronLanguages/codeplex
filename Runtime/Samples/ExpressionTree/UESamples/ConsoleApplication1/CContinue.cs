using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CContinue {
        //Expression.Continue(LabelTarget)
        static public void Continue1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We need to identify a label that will be used by a Break statement and a loop. 
            //This allows a Break to go to the end of an enclosing loop if needed.
            LabelTarget MyBreakLabel = Expression.Label();

            //We need to identify a label that will be used by the Continue statement and the loop it refers to.
            //This allows a Continue to go to the next iteration of an enclosing loop if needed.
            LabelTarget MyContinueLabel = Expression.Label();

            //This expression represents a Continue to the next iteration of a specified loop, identified by a label
            Expression MyContinue = Expression.Continue(MyContinueLabel);

            //A variable that will trigger the loop exit.
            ParameterExpression count = Expression.Parameter(typeof(int));

            //A Continue statement can appear within a loop statement
            Expression AContinueUse = Expression.Loop(
                Expression.Block(
                    Expression.IfThen(
                        Expression.GreaterThan(count,Expression.Constant(5)),
                        Expression.Break(MyBreakLabel)
                    ),
                    Expression.PreIncrementAssign(count),
                    MyContinue,
                    Expression.PreDecrementAssign(count)
                ),
                MyBreakLabel,
                MyContinueLabel
            );
           
            //Without the Continue, this loop would loop forever.
            Expression.Lambda<Action<int>>(AContinueUse, count).Compile().Invoke(0);

            //</Snippet1>
        }


        //Expression.Continue(LabelTarget, Type)
        static public void Continue2()
        {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //We need to identify a label that will be used by a Break statement and a loop. 
            //This allows a Break to go to the end of an enclosing loop if needed.
            LabelTarget MyBreakLabel = Expression.Label();

            //We need to identify a label that will be used by the Continue statement and the loop it refers to.
            //This allows a Continue to go to the next iteration of an enclosing loop if needed.
            LabelTarget MyContinueLabel = Expression.Label();

            //This Expression represents a Continue to the end of a specified loop, indentified a label
            //It also has an explicit type, so it can be used in expressions that expect their arguments to have a certain type
            Expression MyContinue = Expression.Continue(MyContinueLabel, typeof(int));

            //A variable that will trigger the loop exit.
            ParameterExpression count = Expression.Parameter(typeof(int));

            //A Continue statement can appear within a loop statement
            Expression AContinueUse = Expression.Loop(
                Expression.Block(
                    Expression.IfThen(
                        Expression.GreaterThan(count, Expression.Constant(5)),
                        Expression.Break(MyBreakLabel)
                    ),
                    Expression.PreIncrementAssign(count),
                    Expression.Condition(
                        Expression.Constant(true),
                        MyContinue,
                        Expression.Constant(2)
                    ),
                    Expression.PreDecrementAssign(count)
                ),
                MyBreakLabel,
                MyContinueLabel
            );

            //Without the Continue, this loop would loop forever.
            Expression.Lambda<Action<int>>(AContinueUse, count).Compile().Invoke(0);

            //</Snippet1>
        }

        

    }
}
