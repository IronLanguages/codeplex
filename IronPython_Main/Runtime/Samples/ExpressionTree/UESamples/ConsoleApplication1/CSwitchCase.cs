using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CSwitchCase {
        //SwitchCase(Expression, IEnumerable<Expression>)
        public static void SwitchCaseSample() {
            //<Snippet13>
            //Add the following directive to your file
            //using Microsoft.Scripting.Ast;

            ConstantExpression switchValue = Expression.Constant(4);

            SwitchExpression MySwitch =
                Expression.Switch(
                    switchValue,
                    Expression.Constant("DefaultCase"),
                    new SwitchCase[] {
                        // This defines a SwitchCase which executes the Expressions in the first argument 
                        // when the switchValue matches any expressions in the second argument.
                        // SwitchCases are evaluated in the order they appear in the SwitchExpression.
                        Expression.SwitchCase(
                            Expression.Constant("Case1"),
                            Expression.Constant(1)
                        ),
                        // SwitchCases may have multiple test values associated with them as long as their types match.
                        // This SwitchCase will be matched if switchValue is equal to 3 or 4.
                        Expression.SwitchCase(
                            Expression.Constant("Case2"),
                            new List<Expression> { Expression.Constant(3), Expression.Constant(4) }
                        )
                    }
                );

            Console.WriteLine(Expression.Lambda<Func<string>>(MySwitch).Compile().Invoke());
            //</Snippet13>

            // validate sample
            if (Expression.Lambda<Func<string>>(MySwitch).Compile().Invoke() != "Case2")
                throw new Exception("SwitchCaseSample failed");
        }

        // SwitchCase(Expression, Expression[])
        public static void SwitchCaseSample2() {
            //<Snippet14>
            //Add the following directive to your file
            //using Microsoft.Scripting.Ast;

            ConstantExpression switchValue = Expression.Constant(1);

            SwitchExpression MySwitch =
                Expression.Switch(
                    switchValue,
                    Expression.Constant("DefaultCase"),
                    new SwitchCase[] {
                        // This defines a SwitchCase which executes the Expressions in the first argument 
                        // when the switchValue matches any expressions in the second argument.
                        // SwitchCases are evaluated in the order they appear in the SwitchExpression.
                        Expression.SwitchCase(
                            Expression.Constant("Case1"),
                            Expression.Constant(1)
                        ),
                        // SwitchCases may have multiple test values associated with them as long as their types match.
                        // This SwitchCase will be matched if switchValue is equal to 3 or 4.
                        Expression.SwitchCase(
                            Expression.Constant("Case2"),
                            new Expression[] { Expression.Constant(3), Expression.Constant(4) }
                        )
                    }
                );

            Console.WriteLine(Expression.Lambda<Func<string>>(MySwitch).Compile().Invoke());
            //</Snippet14>

            // validate sample
            if (Expression.Lambda<Func<string>>(MySwitch).Compile().Invoke() != "Case1")
                throw new Exception("SwitchCaseSample2 failed");
        }
    }
}
