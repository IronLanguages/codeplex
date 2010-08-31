#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System;
namespace ExpressionTreeNodes
{


    //-------- Scenario 878
    namespace Scenario878
    {

        namespace MemberAssign
        {
            public class HelperClass
            {
                public static int Meth1(int m, int n) { return 1; }
            }

            public class Test
            {
                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "program__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression program__()
                {
                    if (Main() != 0)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        return Expression.Constant(0);
                    }
                }
                public static int Main()
                {
                    MethodInfo mi1 = typeof(HelperClass).GetMethod("Meth1");
                    ParameterExpression pe1 = Expression.Parameter(typeof(int), "a");
                    ParameterExpression pe2 = Expression.Parameter(typeof(int), "b");
                    ParameterExpression pe3 = Expression.Parameter(typeof(int), "c");

                    ParameterExpression[] expr = new ParameterExpression[] { pe1, pe2 };

                    // Create a Method Call Node with the "expr" containing the Arguments
                    MethodCallExpression mce1 = Expression.Call(mi1, expr);

                    // Verify if the "Arguments" in MethodCallNode have appropriate data
                    if (mce1.Arguments[0] != pe1) return 1;
                    if (mce1.Arguments[1] != pe2) return 1;

                    // Update the 1st element in "expr" which was passed as "arguments" to Expression.Call function
                    expr[0] = pe3;

                    // Verify the "Arguments" in MethodCallNode still have the same original data
                    // This verifies that the "Arguments" which are of type "ReadOnlyCollection" does a copy of the data and
                    // does not has a reference
                    if (mce1.Arguments[0] != pe1) return 1;
                    if (mce1.Arguments[1] != pe2) return 1;

                    return 0;
                }
            }
        }

        // </Code>

    }

}
