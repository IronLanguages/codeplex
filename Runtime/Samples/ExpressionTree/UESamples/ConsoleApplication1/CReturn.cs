using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CReturn {
        // Return(LabelTarget)
        public static void ReturnSample1() {
            //<Snippet1>
            // This defines a label with void type that can be jumped to with a GotoExpression.
            LabelTarget returnTarget = Expression.Label();

            BlockExpression MyBlock =
                Expression.Block(
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Returning")),
                    // This creates a GotoExpression representing a return statement with no value.
                    // It will jump to a LabelExpression that was initialized with the same LabelTarget as the GotoExpression.
                    // The types of the GotoExpression, LabelExpression, and LabelTarget must match.
                    Expression.Return(returnTarget),
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("OtherWork")),
                    Expression.Label(returnTarget)
                );

            // On execution the program prints Returning but does not print OtherWork because 
            // the return expression jumped past the second call to Console.WriteLine.
            Expression.Lambda<Action>(MyBlock).Compile().Invoke();
            //</Snippet1>
        }

        // Return(LabelTarget, Expression)
        public static void ReturnSample2() {
            //<Snippet2>
            // This defines a label with type string.
            // LabelExpressions using this LabelTarget must provide a default value of type string
            // and GotoExpressions that jump to those LabelExpressions must provide a string to pass to the LabelExpression.
            LabelTarget returnTarget = Expression.Label(typeof(string));

            BlockExpression MyBlock =
                Expression.Block(
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Returning")),
                    // This creates a GotoExpression representing a return statement with a value. 
                    // It will jump to a LabelExpression that was initialized with the same LabelTarget as the GotoExpression.
                    // The types of the GotoExpression, LabelExpression, and LabelTarget must match.
                    Expression.Return(returnTarget, Expression.Constant("Return")),
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("OtherWork")),
                    Expression.Label(returnTarget, Expression.Constant("DefaultValue"))
                );

            // On execution the program prints Returning but does not print OtherWork because 
            // the return expression jumped past the second call to Console.WriteLine.
            // The value of the expression is "Return" because control flow reached the LabelExpression at the end of
            // the block via the GotoExpression that passed "Return" to the label. Had control flow reached the Label
            // at the end of the block without a jump the block's value would be "DefaultValue"
            Console.WriteLine(Expression.Lambda<Func<string>>(MyBlock).Compile().Invoke());
            //</Snippet2>

            // validate sample
            if (Expression.Lambda<Func<string>>(MyBlock).Compile().Invoke() != "Return")
                throw new Exception("ReturnSample2 failed");
        }

        // Return(LabelTarget, Expression, Type)
        public static void ReturnSample3() {
            //<Snippet3>
            // This defines a label with type int.
            // LabelExpressions using this LabelTarget must provide a default value of type int
            // and GotoExpressions that jump to those LabelExpressions must provide an int to pass to the LabelExpression.
            LabelTarget returnTarget = Expression.Label(typeof(int));

            BlockExpression MyBlock =
                Expression.Block(
                    Expression.Condition(
                        Expression.Constant(true),
                        Expression.Block(
                            Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Returning")),
                            // This creates a GotoExpression representing a return statement with a value. 
                            // It will jump to a LabelExpression that was initialized with the same LabelTarget as the GotoExpression.
                            // The types of the GotoExpression, LabelExpression, and LabelTarget must match.
                            // The Type parameter sets the GotoExpression.Type property to typeof(string).
                            // Without this parameter an exception would be thrown when the Compile method is called 
                            // on this tree because the Type property of the last expression in each branch would not match.
                            Expression.Return(returnTarget, Expression.Constant(1), typeof(string))
                        ),
                        Expression.Constant("Fail")
                    ),
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("OtherWork")),
                    Expression.Label(returnTarget, Expression.Constant(-1))
                );

            // On execution the program prints Returning but does not print OtherWork because 
            // the return expression jumped past the second call to Console.WriteLine.
            // The value of the expression is 1 because control flow reached the LabelExpression at the end of
            // the block via the GotoExpression that passed 1 to the label. Had control flow reached the Label
            // at the end of the block without a jump the block's value would be -1
            Console.WriteLine(Expression.Lambda<Func<int>>(MyBlock).Compile().Invoke());
            //</Snippet33>

            // validate sample
            if (Expression.Lambda<Func<int>>(MyBlock).Compile().Invoke() != 1)
                throw new Exception("ReturnSample3 failed");
        }

        // Return(LabelTarget, Type)
        public static void ReturnSample4() {
            //<Snippet4>
            // This defines a label with void type that can be jumped to with a GotoExpression.
            LabelTarget returnTarget = Expression.Label(typeof(void));

            BlockExpression MyBlock =
                Expression.Block(
                    Expression.Condition(
                        Expression.Constant(true),
                        Expression.Block(
                            Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("Returning")),
                            // This creates a GotoExpression representing a return statement with a value. 
                            // It will jump to a LabelExpression that was initialized with the same LabelTarget as the GotoExpression.
                            // The types of the GotoExpression, LabelExpression, and LabelTarget must match.
                            // The Type parameter sets the GotoExpression.Type property to typeof(string).
                            // Without this parameter an exception would be thrown when the Compile method is called 
                            // on this tree because the Type property of the last expression in each branch would not match.
                            Expression.Return(returnTarget, typeof(string))
                        ),
                        Expression.Constant("Fail")
                    ),
                    Expression.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("OtherWork")),
                    Expression.Label(returnTarget)
                );

            // On execution the program prints Returning but does not print OtherWork because 
            // the return expression jumped past the second call to Console.WriteLine.
            Expression.Lambda<Action>(MyBlock).Compile().Invoke();
            //</Snippet4>
        }
    }
}
