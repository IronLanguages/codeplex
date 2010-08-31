using System;
using System.Collections.Generic;
using Microsoft.Scripting.Ast;
using System.Text;

namespace Samples {
    class CNegate {
        //Expression.Negate(Expression)
        static public void NegateSample1() {
            //<Snippet1>
            //This Expression represents a Negate operation.
            Expression MyNegate = Expression.Negate(Expression.Constant(5));

            //Should print -5.
            Console.WriteLine(Expression.Lambda<Func<int>>(MyNegate).Compile().Invoke());

            //</Snippet1>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyNegate).Compile().Invoke() != -5) throw new Exception("");
        }

        //<Snippet2>
        public static int NegateMethod(int arg) {
            return -(arg + 2);
        }
        //</Snippet2>

        //Expression.Negate(Expression, MethodInfo)
        static public void NegateSample2() {
            //<Snippet2>
            //This Expression represents a user defined Negate operation; It will use the specified user defined operation.
            Expression MyNegate =
                Expression.Negate(
                    Expression.Constant(5),
                    ((Func<int, int>)NegateMethod).Method
                );

            //Should print -7
            Console.WriteLine(Expression.Lambda<Func<int>>(MyNegate).Compile().Invoke());

            //</Snippet2>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyNegate).Compile().Invoke() != -7) throw new Exception("");
        }

        //Expression.NegateChecked(Expression)
        static public void NegateCheckedSample1() {
            //<Snippet3>
            //This Expression represents a NegateChecked operation.
            Expression MyNegateChecked = Expression.NegateChecked(Expression.Constant(Int32.MinValue));

            //An exception should happen:
            try {
                Expression.Lambda<Func<int>>(MyNegateChecked).Compile().Invoke();
            } catch (OverflowException) {
                Console.WriteLine("Expected exception thrown");
            }
            //</Snippet3>

            //validate sample.
            try {
                Expression.Lambda<Func<int>>(MyNegateChecked).Compile().Invoke();
                throw new Exception("Expected Overflow Exception, no exception thrown.");
            } catch (OverflowException) {
            }
        }

        //<Snippet4>
        public static int NegateCheckedMethod(int arg) {
            return (arg == Int32.MinValue) ? 0 : -arg;
        }
        //</Snippet4>

        //Expression.NegateChecked(Expression, MethodInfo)
        static public void NegateCheckedSample2() {
            //<Snippet4>
            //This Expression represents a user defined NegateChecked operation. It will use the specified user defined operation.
            Expression MyNegateChecked =
                Expression.NegateChecked(
                    Expression.Constant(Int32.MinValue),
                    ((Func<int, int>)NegateCheckedMethod).Method
                );

            //Should print 0
            Console.WriteLine(Expression.Lambda<Func<int>>(MyNegateChecked).Compile().Invoke());

            //</Snippet4>

            //Validate sample
            if (Expression.Lambda<Func<int>>(MyNegateChecked).Compile().Invoke() != 0) throw new Exception("");
        }
    }
}
