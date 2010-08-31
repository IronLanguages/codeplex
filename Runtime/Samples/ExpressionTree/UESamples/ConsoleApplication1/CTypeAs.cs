using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CTypeAs {
        // TypeAs(Expression, Type)
        public static void TypeAsSample() {
            //<Snippet6>
            // Defines a UnaryExpression representing an explicit reference or boxing conversion to the given type.
            // If the conversion is not possible the result is null.
            // Equivalent to the C# 'as' operator.
            UnaryExpression Result1 = Expression.TypeAs(Expression.Constant(new DivideByZeroException()), typeof(Exception));
            UnaryExpression Result2 = Expression.TypeAs(Expression.Constant("A String"), typeof(Exception));

            Expression Check1 =
                Expression.Condition(
                    Expression.NotEqual(Result1, Expression.Constant(null)),
                    Expression.Constant(true),
                    Expression.Constant(false)
                );

            Expression Check2 =
                Expression.Condition(
                    Expression.NotEqual(Result2, Expression.Constant(null)),
                    Expression.Constant(true),
                    Expression.Constant(false)
                );

            // The first check will return true because the DivideByZeroException could be legally converted to an Exception type.
            Console.WriteLine(Expression.Lambda<Func<bool>>(Check1).Compile().Invoke());
            // The second check will return false because a string cannot be converted to an Exception so the result was null.
            Console.WriteLine(Expression.Lambda<Func<bool>>(Check2).Compile().Invoke());
            //</Snippet6>

            // validate sample
            if (Expression.Lambda<Func<bool>>(Check1).Compile().Invoke() != true ||
                Expression.Lambda<Func<bool>>(Check2).Compile().Invoke() != false)
                throw new Exception("TypeAsSample failed");
        }
    }
}
