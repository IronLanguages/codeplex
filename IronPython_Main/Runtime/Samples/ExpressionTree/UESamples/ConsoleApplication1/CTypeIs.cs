using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Ast;

namespace Samples {
    class CTypeIs {
        //TypeIs(Expression, Type)
        public static void TypeIsSample() {
            //<Snippet5>
            // Defines a TypeBinaryExpression representing a subtype test of the given value and type.
            // This is similar to C#'s 'is' operator but differs slightly.
            // This check will return true because DivideByZeroException is a subtype of Exception.
            TypeBinaryExpression Check1 = Expression.TypeIs(Expression.Constant(new DivideByZeroException()), typeof(Exception));
            // This check will return false because Exception is not a subtype of Exception.
            TypeBinaryExpression Check2 = Expression.TypeIs(Expression.Constant(new Exception()), typeof(DivideByZeroException));
            // This check will return true because the types match exactly.
            TypeBinaryExpression Check3 = Expression.TypeIs(Expression.Constant(new DivideByZeroException()), typeof(DivideByZeroException));

            Console.WriteLine(Expression.Lambda<Func<bool>>(Check1).Compile().Invoke());
            Console.WriteLine(Expression.Lambda<Func<bool>>(Check2).Compile().Invoke());
            Console.WriteLine(Expression.Lambda<Func<bool>>(Check3).Compile().Invoke());
            //</Snippet5>

            // validate sample
            if (Expression.Lambda<Func<bool>>(Check1).Compile().Invoke() != true ||
                Expression.Lambda<Func<bool>>(Check2).Compile().Invoke() != false ||
                Expression.Lambda<Func<bool>>(Check3).Compile().Invoke() != true)
                throw new Exception("TypeIsSample failed");
        }
    }
}
