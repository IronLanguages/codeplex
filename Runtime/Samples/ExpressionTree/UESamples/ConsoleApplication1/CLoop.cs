using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CLoop {

        //Expression.Loop(Expression)
        static public void Loop1() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;  

            //A simple loop loops forever, so we need to handle the jump out of the loop.
            //For that we'll create a label to jump out to.
            LabelTarget OutLabel = Expression.Label();

            //An index to condition the loop exit.
            ParameterExpression Index = Expression.Variable(typeof(int));


            //This Expression represents a loop.
            //Within the loop body, we'll increment a counter, and exit when it reaches 5.
            LoopExpression MyLoop = Expression.Loop(
                Expression.Block(
                    Expression.IfThen(
                        Expression.GreaterThanOrEqual(
                            Index,
                            Expression.Constant(5)
                        ),
                        Expression.Goto(OutLabel)
                    ),
                    Expression.PreIncrementAssign(Index)
                )
            );


            //To demonstrate the block use, we'll initialize the index variable,
            //run the loop, and check the index's value at the end (should be 5).
            var Main = Expression.Block(
                new ParameterExpression[] { Index },
                Expression.Assign(Index, Expression.Constant(0)),
                MyLoop,
                Expression.Label(OutLabel),
                Index
            );

            //Should print 5
            Console.WriteLine(Expression.Lambda<Func<int>>(Main).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(Main).Compile().Invoke() != 5) throw new Exception("");
        }

        //Expression.Loop(Expression, LabelTarget)
        static public void Loop2() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;   

            //A simple loop loops forever, so we need to handle the jump out of the loop.
            //For that we'll create a break label.
            LabelTarget BreakLabel = Expression.Label();

            //An index to condition the loop exit.
            ParameterExpression Index = Expression.Variable(typeof(int));


            //This Expression represents a loop.
            //Within the loop body, we'll increment a counter, and exit when it reaches 5.
            LoopExpression MyLoop = Expression.Loop(
                Expression.Block(
                    Expression.IfThen(
                        Expression.GreaterThanOrEqual(
                            Index,
                            Expression.Constant(5)
                        ),
                        Expression.Break(BreakLabel)
                    ),
                    Expression.PreIncrementAssign(Index)
                ),
                BreakLabel
            );


            //To demonstrate the block use, we'll initialize the index variable,
            //run the loop, and check the index's value at the end (should be 5).
            var Main = Expression.Block(
                new ParameterExpression[] { Index },
                Expression.Assign(Index, Expression.Constant(0)),
                MyLoop,
                Index
            );

            //Should print 5
            Console.WriteLine(Expression.Lambda<Func<int>>(Main).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(Main).Compile().Invoke() != 5) throw new Exception("");
        }



        //Expression.Loop(Expression, LabelTarget)
        static public void Loop3() {
            //<Snippet1>
            // add the following directive to your file
            // using Microsoft.Scripting.Ast;   

            //A simple loop loops forever, so we need to handle the jump out of the loop.
            //For that we'll create a break label.
            LabelTarget BreakLabel = Expression.Label();

            //We'll also create a continue label if we want to skip over a part of the loop.
            LabelTarget ContinueLabel = Expression.Label();

            //An index to condition the loop exit.
            ParameterExpression Index = Expression.Variable(typeof(int));


            //This Expression represents a loop.
            //Within the loop body, we'll increment a counter, and exit when it reaches 5.
            //For sample purposes, we continue over an increment and a decrement.
            LoopExpression MyLoop = Expression.Loop(
                Expression.Block(
                    Expression.IfThen(
                        Expression.GreaterThanOrEqual(
                            Index,
                            Expression.Constant(5)
                        ),
                        Expression.Break(BreakLabel)
                    ),
                    Expression.PreIncrementAssign(Index),
                    Expression.IfThen(
                        Expression.Equal(
                            Index,
                            Expression.Constant(3)
                        ),
                        Expression.Continue(ContinueLabel)
                    ),
                    Expression.PreIncrementAssign(Index),
                    Expression.PreDecrementAssign(Index)
                ),
                BreakLabel,
                ContinueLabel
            );


            //To demonstrate the block use, we'll initialize the index variable,
            //run the loop, and check the index's value at the end (should be 5).
            var Main = Expression.Block(
                new ParameterExpression[] { Index },
                Expression.Assign(Index, Expression.Constant(0)),
                MyLoop,
                Index
            );

            //Should print 5
            Console.WriteLine(Expression.Lambda<Func<int>>(Main).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(Main).Compile().Invoke() != 5) throw new Exception("");

        }

        /*
         * Note, these last examples do not use LoopExpression, but they apply to LoopExpression 
         * and show how LoopExpressions can be thought of as reducing to Labels and Goto's.
         * These examples illustrate an interesting property of ETs and how to create strong
         * lexical scoping for variables used within an iteration.  They also show an underlying
         * .NET issue with initializing variable storage locations vs. leaking through previous contents.
         */

        /*  Create a loop of the following format:  
             {
                var str;
                var count;
                Start:;
                {
                    var i;
                    (count += 1);
                    (i += 1);
                    (str = Concat(str, i.ToString(), "|"));
                };
                IIF((count < 10), goto Start, );
                str;
            }
         */
        private static Expression MakeLoop()
        {
            LabelTarget start = Expression.Label("Start");
            ParameterExpression i = Expression.Parameter(typeof(int), "i");
            ParameterExpression count = Expression.Parameter(typeof(int),
                                                             "count");
            ParameterExpression str = Expression.Parameter(typeof(String),
                                                           "str");
            return Expression.Block(new ParameterExpression[] { str, count },
                Expression.Label(start),
                Expression.Block(new ParameterExpression[] { i },
                    Expression.AddAssign(count, Expression.Constant(1)),
                    Expression.AddAssign(i, Expression.Constant(1)),
                    Expression.Assign(
                        str,
                        Expression.Call(
                            typeof(String)
                               .GetMethod(
                                   "Concat",
                                   new Type[] { typeof(String),
                                        typeof(String),
                                        typeof(String) }),
                            str,
                            Expression.Call(i, "ToString", Type.EmptyTypes),
                            Expression.Constant("|")
                            )
                        )
                    ),
                Expression.IfThen(
                    Expression.LessThan(count, Expression.Constant(10)),
                    Expression.Goto(start)
                    ),
                str
                );
        }

        /*  Create a loop of the following format:  
        {
                var str;
                var count;
                Start:;
                {
                    var i;
                    (count += 1);
                    (i += 1);
                    (str = Concat(str, i.ToString(), "|"));
                    () => i;
                };
                IIF((count < 10), goto Start, );
                str;
         }
         */
        private static Expression MakeLoopWithLamdba()
        {
            LabelTarget start = Expression.Label("Start");
            ParameterExpression i = Expression.Parameter(typeof(int), "i");
            ParameterExpression count = Expression.Parameter(typeof(int),
                                                             "count");
            ParameterExpression str = Expression.Parameter(typeof(String),
                                                           "str");
            return Expression.Block(new ParameterExpression[] { str, count },
                Expression.Label(start),
                Expression.Block(new ParameterExpression[] { i },
                    Expression.AddAssign(count, Expression.Constant(1)),
                    Expression.AddAssign(i, Expression.Constant(1)),
                    Expression.Assign(
                        str,
                        Expression.Call(
                            typeof(String)
                               .GetMethod(
                                   "Concat",
                                   new Type[] { typeof(String),
                                        typeof(String),
                                        typeof(String) }),
                            str,
                            Expression.Call(i, "ToString", Type.EmptyTypes),
                            Expression.Constant("|")
                            )
                        ),
                    Expression.Lambda(i)
                    ),
                Expression.IfThen(
                    Expression.LessThan(count, Expression.Constant(10)),
                    Expression.Goto(start)
                    ),
                str
                );
        }

        /*
         * The lambda creates a closure over "i", which create a unique lexical binding in each
         * iteration of the loop.  .NET leaks old values into the variable for each iteration
         * when there is no closure, so you get a string with one to ten in it.  
         * When there is a closure, .NET re-initializes the memory as it should, so you get a string
         * with all ones.
         * 
         * If you want to capture closures over unique bindings/values of "i" in an iteration,
         * ETs correctly compile with those semantics using Goto's or Loops
         * (when the Block is within the iteration bounds).  
         * However, you need to correctly initialize the loop variable that you close over.
         * The following MakeLoopWithLamdba2 is the same as MakeLoopWithLamdba except that it uses an
         * extra variable, "i_", that is outside of the Block that is inside the loop bounds.
         * The extra variable counts one to ten and is the initialization value for "i" 
         * each time the code enters the inner Block that is within the loop bounds.
         */
        private static Expression MakeLoopWithLamdba2()
        {
            LabelTarget start = Expression.Label("Start");
            ParameterExpression i_ = Expression.Parameter(typeof(int), "i_");
            ParameterExpression i = Expression.Parameter(typeof(int), "i");
            ParameterExpression count = Expression.Parameter(typeof(int),
                                                             "count");
            ParameterExpression str = Expression.Parameter(typeof(String),
                                                           "str");
            return Expression.Block(new ParameterExpression[] { str, count, i_ },
                Expression.Label(start),
                Expression.Block(new ParameterExpression[] { i },
                    Expression.Assign(i, i_),
                    Expression.AddAssign(count, Expression.Constant(1)),
                    Expression.AddAssign(i, Expression.Constant(1)),
                    Expression.Assign(
                        str,
                        Expression.Call(
                            typeof(String)
                                .GetMethod(
                                    "Concat",
                                    new Type[] { typeof(String),
                                         typeof(String),
                                         typeof(String) }),
                            str,
                            Expression.Call(i, "ToString",
                                            Type.EmptyTypes),
                            Expression.Constant("|")
                            )
                        ),
                    Expression.Lambda(i),
                    Expression.Assign(i_, i)
                    ),
                Expression.IfThen(
                    Expression.LessThan(count, Expression.Constant(10)),
                    Expression.Goto(start)
                    ),
                str
                );
        }

        static public void Loop4() {
            //Should print 1|2|3|4|5|6|7|8|9|10|
            Console.WriteLine(Expression.Lambda<Func<string>>(MakeLoop()).Compile()());

            //Validate
            if (Expression.Lambda<Func<string>>(MakeLoop()).Compile()() != "1|2|3|4|5|6|7|8|9|10|") throw new Exception("");

            //Should print 1|1|1|1|1|1|1|1|1|1|
            Console.WriteLine(Expression.Lambda<Func<string>>(MakeLoopWithLamdba()).Compile()());

            if (Expression.Lambda<Func<string>>(MakeLoopWithLamdba()).Compile()() != "1|1|1|1|1|1|1|1|1|1|") throw new Exception("");

            //Should print 1|2|3|4|5|6|7|8|9|10|
            Console.WriteLine(Expression.Lambda<Func<string>>(MakeLoopWithLamdba2()).Compile()());

            //Validate
            if (Expression.Lambda<Func<string>>(MakeLoopWithLamdba2()).Compile()() != "1|2|3|4|5|6|7|8|9|10|") throw new Exception("");

        }
    }
}
