using System;
using System.Collections.Generic;
using Microsoft.Scripting.Ast;
using System.Text;

namespace Samples {
    class CUnaryPlus {
        // UnaryPlus(Expression)
        public static void UnaryPlusSample() {
            //<Snippet3>
            // Defines a UnaryPlus operation using the given arithmetic value.
            UnaryExpression Plus = Expression.UnaryPlus(Expression.Constant(5));

            Console.WriteLine(Expression.Lambda<Func<int>>(Plus).Compile().Invoke());
            //</Snippet3>

            // validate sample
            if (Expression.Lambda<Func<int>>(Plus).Compile().Invoke() != 5)
                throw new Exception("UnaryPlusSample failed");
        }

        //UnaryPlus(Expression, MethodInfo)
        //<Snippet4>
        public static int MyPlus(int arg1) {
            return arg1 + 3;
        }
        //</Snippet4>

        public static void UnaryPlusWithMethodSample() {
            //<Snippet4_1>
            // Defines a UnaryPlus operation using the given arithmetic value.
            // The MethodInfo argument overrides the normal behavior of UnaryPlus.
            UnaryExpression Plus = Expression.UnaryPlus(Expression.Constant(5), ((Func<int, int>)MyPlus).Method);

            // The end result is 8 because MyPlus adds 3 to the operand of UnaryPlus.
            Console.WriteLine(Expression.Lambda<Func<int>>(Plus).Compile().Invoke());
            //</Snippet4_1>

            // validate sample
            if (Expression.Lambda<Func<int>>(Plus).Compile().Invoke() != 8)
                throw new Exception("UnaryPlusSample failed");
        }
    }
}
