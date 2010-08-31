#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System.Reflection;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System;
namespace ExpressionTreeNodes
{


    //-------- Scenario 40
    namespace Scenario40
    {

        namespace And
        {
            public class Test
            {
                // left = null
                public static int Test1()
                {
                    ConstantExpression left = Expression.Constant(10);
                    ConstantExpression right = Expression.Constant(9);

                    try
                    {
                        left = null;
                        BinaryExpression result = Expression.And(left, right);
                        return 1;
                    }
                    catch (ArgumentNullException ane)
                    {
                        if (ane.CompareParamName("left")) return 0;
                        return 1;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And1__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And1__()
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
                    return Test1();
                }
            }
        }



        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }


    }

    //-------- Scenario 41
    namespace Scenario41
    {

        namespace And
        {
            public class Test
            {
                // right = null
                public static int Test2()
                {
                    ConstantExpression left = Expression.Constant(10);
                    ConstantExpression right = Expression.Constant(9);

                    try
                    {
                        right = null;
                        BinaryExpression result = Expression.And(left, right);
                        return 1;
                    }
                    catch (ArgumentNullException ane)
                    {
                        if (ane.CompareParamName("right")) return 0;
                        return 1;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And2__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And2__()
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
                    return Test2();
                }
            }
        }



        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }


    }

    //-------- Scenario 42
    namespace Scenario42
    {

        namespace And
        {
            public class Test
            {
                // Test for functionality: type is of enum
                public static int Test3()
                {
                    ConstantExpression left = Expression.Constant(color.Red, typeof(color));
                    ConstantExpression right = Expression.Constant(color.Red, typeof(color));

                    try
                    {
                        BinaryExpression result = Expression.And(left, right);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And3__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And3__()
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
                    return Test3();
                }
            }
        }


        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
				        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
				        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
				        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
				        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }



        namespace And
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator &(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }

                public static TC1 Meth1(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            public enum color { Red, Green, Blue }
        }

    }

    //-------- Scenario 43
    namespace Scenario43
    {

        namespace And
        {
            public class Test
            {
                // Test for functionality: type is string (non-numeric)
                public static int Test4()
                {
                    ConstantExpression left = Expression.Constant("Test", typeof(string));
                    ConstantExpression right = Expression.Constant("Test", typeof(string));

                    try
                    {
                        BinaryExpression result = Expression.And(left, right);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And4__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And4__()
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
                    return Test4();
                }
            }
        }


        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }


    }

    //-------- Scenario 44
    namespace Scenario44
    {

        namespace And
        {
            public class Test
            {
                // Test for functionality: left and right types do NOT match
                public static int Test5()
                {
                    ConstantExpression left = Expression.Constant(10, typeof(int));
                    ConstantExpression right = Expression.Constant(12L, typeof(long));

                    try
                    {
                        BinaryExpression result = Expression.And(left, right);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And5__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And5__()
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
                    return Test5();
                }
            }
        }


        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }


    }

    //-------- Scenario 45
    namespace Scenario45
    {

        namespace And
        {
            public class Test
            {
                // Test for functionality: Test for bool
                public static int Test6()
                {
                    ConstantExpression left = Expression.Constant(true, typeof(bool));
                    ConstantExpression right = Expression.Constant(false, typeof(bool));

                    ExpressionType exp_et = ExpressionType.And;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "And" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.And(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And6__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And6__()
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
                    return Test6();
                }
            }
        }


        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }


    }

    //-------- Scenario 46
    namespace Scenario46
    {

        namespace And
        {
            public class Test
            {
                // Test for functionality: Test for bool?
                public static int Test7()
                {
                    ConstantExpression left = Expression.Constant(false, typeof(bool?));
                    ConstantExpression right = Expression.Constant(true, typeof(bool?));

                    ExpressionType exp_et = ExpressionType.And;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "And" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.And(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And7__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And7__()
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
                    return Test7();
                }
            }
        }


        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }


    }

    //-------- Scenario 47
    namespace Scenario47
    {

        namespace And
        {
            public class Test
            {
                // Test for functionality: Type is int?
                public static int Test8()
                {
                    ConstantExpression left = Expression.Constant(9, typeof(int?));
                    ConstantExpression right = Expression.Constant(18, typeof(int?));

                    ExpressionType exp_et = ExpressionType.And;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "&" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.And(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And8__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And8__()
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
                    return Test8();
                }
            }
        }


        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
			        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
			        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }


    }

    //-------- Scenario 48
    namespace Scenario48
    {

        namespace And
        {
            public class Test
            {
                // Overload-1: Test for functionality: Test methodinfo data is picked up for user-defined type
                public static int Test9()
                {
                    TC1 tc1 = new TC1();
                    ConstantExpression left = Expression.Constant(tc1, typeof(TC1));
                    ConstantExpression right = Expression.Constant(tc1, typeof(TC1));
                    MethodInfo mi = typeof(TC1).GetMethod("op_BitwiseAnd");

                    ExpressionType exp_et = ExpressionType.And;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "&" + " " + right.ToString() + ")";

                    BinaryExpression result = (BinaryExpression)Expression.And(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And9__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And9__()
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
                    return Test9();
                }
            }
        }


        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
				        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
				        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
				        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
				        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }



        namespace And
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator &(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }

                public static TC1 Meth1(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            public enum color { Red, Green, Blue }
        }

    }

    //-------- Scenario 49
    namespace Scenario49
    {

        namespace And
        {
            public class Test
            {
                // Overload-1: Test for functionality: Test methodinfo data is picked up for user-defined nullable type
                public static int Test10()
                {
                    TC1 tc1 = new TC1();
                    ConstantExpression left = Expression.Constant(tc1, typeof(TC1?));
                    ConstantExpression right = Expression.Constant(tc1, typeof(TC1?));
                    MethodInfo mi = typeof(TC1).GetMethod("op_BitwiseAnd");

                    ExpressionType exp_et = ExpressionType.And;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "&" + " " + right.ToString() + ")";

                    BinaryExpression result = (BinaryExpression)Expression.And(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And10__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And10__()
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
                    return Test10();
                }
            }
        }


        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
				        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
				        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
				        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
				        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }



        namespace And
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator &(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }

                public static TC1 Meth1(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            public enum color { Red, Green, Blue }
        }

    }

    //-------- Scenario 50
    namespace Scenario50
    {

        namespace And
        {
            public class Test
            {
                // Overload-2: Test for functionality: Test methodinfo data is picked up for user-defined type when MethodInfo is null
                public static int Test11()
                {
                    TC1 tc1 = new TC1();
                    ConstantExpression left = Expression.Constant(tc1, typeof(TC1));
                    ConstantExpression right = Expression.Constant(tc1, typeof(TC1));
                    MethodInfo mi = typeof(TC1).GetMethod("op_BitwiseAnd");

                    ExpressionType exp_et = ExpressionType.And;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "&" + " " + right.ToString() + ")";

                    BinaryExpression result = (BinaryExpression)Expression.And(left, right, null);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And11__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And11__()
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
                    return Test11();
                }
            }
        }


        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
				        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
				        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
				        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
				        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }



        namespace And
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator &(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }

                public static TC1 Meth1(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            public enum color { Red, Green, Blue }
        }

    }

    //-------- Scenario 51
    namespace Scenario51
    {

        namespace And
        {
            public class Test
            {
                // Overload-2: Test for functionality: Test methodinfo data is picked up for user-defined type when MethodInfo is  not-null
                public static int Test12()
                {
                    TC1 tc1 = new TC1();
                    ConstantExpression left = Expression.Constant(tc1, typeof(TC1));
                    ConstantExpression right = Expression.Constant(tc1, typeof(TC1));
                    MethodInfo mi = typeof(TC1).GetMethod("Meth1");

                    ExpressionType exp_et = ExpressionType.And;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "&" + " " + right.ToString() + ")";

                    BinaryExpression result = (BinaryExpression)Expression.And(left, right, mi);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And12__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And12__()
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
                    return Test12();
                }
            }
        }


        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
				        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
				        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
				        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
				        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }



        namespace And
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator &(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }

                public static TC1 Meth1(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            public enum color { Red, Green, Blue }
        }

    }

    //-------- Scenario 52
    namespace Scenario52
    {

        namespace And
        {
            public class Test
            {
                // Overload-2: Test for functionality: Test methodinfo data is picked up for user-defined type when MethodInfo is  not-null and type is nullable
                public static int Test13()
                {
                    TC1 tc1 = new TC1();
                    ConstantExpression left = Expression.Constant(tc1, typeof(TC1?));
                    ConstantExpression right = Expression.Constant(tc1, typeof(TC1?));
                    MethodInfo mi = typeof(TC1).GetMethod("Meth1");

                    ExpressionType exp_et = ExpressionType.And;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "&" + " " + right.ToString() + ")";

                    BinaryExpression result = (BinaryExpression)Expression.And(left, right, mi);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "And13__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression And13__()
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
                    return Test13();
                }
            }
        }


        public static class Extension
        {
            public static bool CompareParamName(this ArgumentException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
				        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
				        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }

            public static bool CompareParamName(this ArgumentNullException ex, string expected)
            {
#if SILVERLIGHT
#if SLRESOURCES
				        return ex.Message.Substring(ex.Message.LastIndexOf(": ") + 2) == expected;
#else
				        return true;
#endif
#else
                return ex.ParamName == expected;
#endif
            }
        }

        public static class Verification
        {
            public static int VerifyConstParms(ConstantExpression result, ExpressionType et, Type type, object val, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (val == null)
                {
                    if (result.Value != null) return 1;
                }
                else
                {
                    if (!(result.Value.Equals(val))) return 1;
                }
                if (!(result.ToString() == str)) return 1;
                return 0;
            }

            public static int VerifyUnaryParms(UnaryExpression result, ExpressionType et, Type type, Expression operand, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (operand == null)
                {
                    if (result.Operand != null) return 1;
                }
                else
                {
                    if (!(result.Operand.Equals(operand))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null)
                    {
                        Console.WriteLine("left was null");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Left.Equals(left)))
                    {
                        Console.WriteLine("left was different");
                        return 1;
                    }
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion != null) return 1;

                return 0;
            }

            public static int VerifyBinaryParms(BinaryExpression result, ExpressionType et, Type type, Expression left, Expression right, string str, MethodInfo mi, LambdaExpression conv_expr, bool islifted, bool isliftedtonull)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (left == null)
                {
                    if (result.Left != null) return 1;
                }
                else
                {
                    if (!(result.Left.Equals(left))) return 1;
                }

                if (right == null)
                {
                    if (result.Right != null) return 1;
                }
                else
                {
                    if (!(result.Right.Equals(right))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                if (result.Method != mi) return 1;
                if (result.IsLifted != islifted) return 1;
                if (result.IsLiftedToNull != isliftedtonull) return 1;

                if (result.Conversion.ToString() != conv_expr.ToString()) return 1;

                return 0;
            }

            public static int VerifyMethodCallParms(MethodCallExpression result, ExpressionType et, Type type, MethodInfo method, Expression obj, string str, params Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Method == method)) return 1;

                if (obj == null)
                {
                    if (result.Object != null)
                    {
                        Console.WriteLine("Expected object to be null.");
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Object.Equals(obj)))
                    {
                        Console.WriteLine("Object on which call is made is different from the result.");
                        return 1;
                    }
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length) return 1;

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i]) return 1;
                }

                return 0;
            }

            public static int VerifyTypeBinaryParms(TypeBinaryExpression result, ExpressionType et, Type type, Expression expr, Type typeop, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    Console.WriteLine("expr was null.");
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr)))
                    {
                        return 1;
                    }
                }

                if (!(result.TypeOperand == typeop)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }

            public static int VerifyConditionalParms(ConditionalExpression result, ExpressionType et, Type type, Expression test, Expression ifTrue, Expression ifFalse, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (test == null)
                {
                    if (result.Test != null) return 1;
                }
                else
                {
                    if (!(result.Test.Equals(test))) return 1;
                }

                if (ifTrue == null)
                {
                    if (result.IfTrue != null) return 1;
                }
                else
                {
                    if (!(result.IfTrue.Equals(ifTrue))) return 1;
                }

                if (ifFalse == null)
                {
                    if (result.IfFalse != null) return 1;
                }
                else
                {
                    if (!(result.IfFalse.Equals(ifFalse))) return 1;
                }

                if (!(result.ToString() == str)) return 1;

                return 0;
            }

            public static int VerifyMemberParms(MemberExpression result, ExpressionType et, Type type, Expression exp_expr, MemberInfo exp_member, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (exp_expr == null)
                {
                    if (result.Expression != null)
                    {
                        Console.WriteLine("Expression is not null: " + result.Expression.ToString());
                        return 1;
                    }
                }
                else
                {
                    if (!(result.Expression.Equals(exp_expr)))
                    {
                        Console.WriteLine("Unexpected Expression: " + result.Expression.ToString());
                        return 1;
                    }
                }

                if (!(result.Member == exp_member))
                {
                    Console.WriteLine("Unexpected result member: " + result.Member.ToString());
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments, params MemberInfo[] members)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Unexpected constructor");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (arguments == null)
                {
                    if (result.Arguments.Count != 0)
                    {
                        Console.WriteLine("More than one argument supplied: " + result.Arguments.Count);
                        return 1;
                    }
                }
                else
                {
                    if (result.Arguments.Count != arguments.Length)
                    {
                        Console.WriteLine("Different number of arguments obtained: " + result.Arguments.Count);
                        return 1;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (result.Arguments[i] != arguments[i])
                        {
                            Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                            return 1;
                        }
                    }
                }

                if (result.Members == null)
                {
                    Console.WriteLine("result.Members was null");
                    return 1;
                }

                if (members == null)
                {
                    if (result.Members.Count != 0)
                    {
                        Console.WriteLine("Got more than zero members");
                        return 1;
                    }
                }
                else
                {
                    if (result.Members.Count != members.Length)
                    {
                        Console.WriteLine("Got an unexpected number of members: " + result.Members.Count);
                        return 1;
                    }

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (result.Members[i] != members[i])
                        {
                            Console.WriteLine("Member " + i.ToString() + " is different than expected: Expected " + members[i].ToString() + " and got " + result.Members[i].ToString());
                            return 1;
                        }
                    }
                }

                return 0;
            }

            public static int VerifyNewParms(NewExpression result, ExpressionType et, Type type, ConstructorInfo constructor, string str, Expression[] arguments)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.Constructor == constructor))
                {
                    Console.WriteLine("Constructor is different from expected.");
                    return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != arguments.Length)
                {
                    Console.WriteLine("Different number of arguments: " + result.Arguments.Count);
                    return 1;
                }

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (result.Arguments[i] != arguments[i])
                    {
                        Console.WriteLine("Argument " + i.ToString() + " is different than expected: Expected " + arguments[i].ToString() + " and got " + result.Arguments[i].ToString());
                        return 1;
                    }
                }

                if (result.Members != null)
                {
                    Console.WriteLine("result.Members isn't null");
                    return 1;
                }

                return 0;
            }

            public static int VerifyListInitParms(ListInitExpression result, ExpressionType et, Type type, NewExpression newExpression, string str, params ElementInit[] initializers)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (!(result.NewExpression == newExpression)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }

                return 0;
            }

            public static int VerifyElementInitParms(ElementInit result, MethodInfo exp_mi, string str, params Expression[] args)
            {
                if (result.AddMethod != exp_mi) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (result.Arguments[i] != args[i]) return 1;
                }

                return 0;
            }

            public static int VerifyMemberAssignmentParms(MemberAssignment result, MemberBindingType bt, Expression expr, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;
                if (!(result.Expression == expr)) return 1;
                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyMemberListBindParms(MemberListBinding result, MemberBindingType bt, ElementInit[] initializers, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Initializers.Count != initializers.Length) return 1;

                for (int i = 0; i < initializers.Length; i++)
                {
                    if (result.Initializers[i].AddMethod != initializers[i].AddMethod) return 1;

                    if (result.Initializers[i].Arguments.Count != initializers[i].Arguments.Count) return 1;

                    for (int j = 0; j < result.Initializers[i].Arguments.Count; j++)
                    {
                        if (result.Initializers[i].Arguments[j] != initializers[i].Arguments[j]) return 1;
                    }
                }


                return 0;
            }

            public static int VerifyMemberMemberBindParms(MemberMemberBinding result, MemberBindingType bt, MemberBinding[] bindings, MemberInfo member, string str)
            {
                if (!(result.BindingType == bt)) return 1;

                if (!(result.Member.Equals(member))) return 1;
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyMemberInitParms(MemberInitExpression result, ExpressionType et, Type type, NewExpression newExpr, MemberBinding[] bindings, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.NewExpression.Equals(newExpr))) return 1;
                if (!(result.ToString() == str)) return 1;

                if (result.Bindings.Count != bindings.Length) return 1;

                for (int i = 0; i < bindings.Length; i++)
                {
                    if (!(result.Bindings[i].Equals(bindings[i]))) return 1;
                }

                return 0;
            }

            public static int VerifyNewArrayParms(NewArrayExpression result, ExpressionType et, Type type, Expression[] expr, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (result.Expressions.Count != expr.Length) return 1;

                for (int i = 0; i < expr.Length; i++)
                {
                    if (!(result.Expressions[i].Equals(expr[i]))) return 1;
                }
                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyParameterParms(ParameterExpression result, ExpressionType et, Type type, string name, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }
                if (!(result.Name == name)) return 1;

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyLambdaParms(LambdaExpression result, ExpressionType et, Type type, Expression expr, ParameterExpression[] parms, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Body != null) return 1;
                }
                else
                {
                    if (!(result.Body.Equals(expr))) return 1;
                }

                if (result.Parameters.Count != parms.Length) return 1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (!(result.Parameters[i].Equals(parms[i]))) return 1;
                }

                if (!(result.ToString() == str))
                {
                    Console.WriteLine("Unexpected result: " + result.ToString() + " - Expected: " + str);
                    return 1;
                }

                return 0;
            }

            public static int VerifyInvokeParms(InvocationExpression result, ExpressionType et, Type type, Expression expr, Expression[] args, string str)
            {
                if (!(result.NodeType == et))
                {
                    Console.WriteLine("Unexpected result node type: " + result.NodeType.ToString());
                    return 1;
                }
                if (!(result.Type == type))
                {
                    Console.WriteLine("Unexpected result type: " + result.Type.ToString());
                    return 1;
                }

                if (expr == null)
                {
                    if (result.Expression != null) return 1;
                }
                else
                {
                    if (!(result.Expression.Equals(expr))) return 1;
                }

                if (result.Arguments.Count != args.Length) return 1;

                for (int i = 0; i < args.Length; i++)
                {
                    if (!(result.Arguments[i].Equals(args[i])))
                    {
                        return 1;
                    }
                }

                if (result.ToString() != str)
                {
                    Console.WriteLine("Expected: " + str + " Actual: " + result.ToString());
                    return 1;
                }

                return 0;
            }
        }



        namespace And
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator &(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }

                public static TC1 Meth1(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            public enum color { Red, Green, Blue }
        }

    }

}
