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


    //-------- Scenario 139
    namespace Scenario139
    {

        namespace Coalesce
        {
            public class Test
            {
                // left = null
                public static int Test1()
                {
                    ConstantExpression left = Expression.Constant(10, typeof(int));
                    ConstantExpression right = Expression.Constant(45, typeof(int));

                    try
                    {
                        left = null;
                        BinaryExpression result = Expression.Coalesce(left, right);
                        return 1;
                    }
                    catch (ArgumentNullException ane)
                    {
                        if (ane.CompareParamName("left")) return 0;
                        return 1;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce1__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce1__()
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

    //-------- Scenario 140
    namespace Scenario140
    {

        namespace Coalesce
        {
            public class Test
            {
                // right = null
                public static int Test2()
                {
                    ConstantExpression left = Expression.Constant(10, typeof(int));
                    ConstantExpression right = Expression.Constant(45, typeof(int));

                    try
                    {
                        right = null;
                        BinaryExpression result = Expression.Coalesce(left, right);
                        return 1;
                    }
                    catch (ArgumentNullException ane)
                    {
                        if (ane.CompareParamName("right")) return 0;
                        return 1;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce2__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce2__()
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

    //-------- Scenario 141
    namespace Scenario141
    {

        namespace Coalesce
        {
            public class Test
            {
                // left and right are of different types: NOT implicitly convertible
                public static int Test3()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant("Test", typeof(string));

                    try
                    {
                        BinaryExpression result = Expression.Coalesce(left, right);
                        return 1;
                    }
                    catch (ArgumentException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce3__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce3__()
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


    }

    //-------- Scenario 142
    namespace Scenario142
    {

        namespace Coalesce
        {
            public class Test
            {
                // Test for functionality: left is a nullable type and right is non-nullable type
                public static int Test4()
                {
                    ConstantExpression left = Expression.Constant(null, typeof(int?));
                    ConstantExpression right = Expression.Constant(9, typeof(int));

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = right.Type;
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Coalesce(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, (MethodInfo)null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce4__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce4__()
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

    //-------- Scenario 143
    namespace Scenario143
    {

        namespace Coalesce
        {
            public class Test
            {
                // left is a non-nullable type and right is nullable type
                public static int Test5()
                {
                    ConstantExpression left = Expression.Constant(3.5, typeof(double));
                    ConstantExpression right = Expression.Constant(null, typeof(double?));

                    try
                    {
                        BinaryExpression result = Expression.Coalesce(left, right);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce5__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce5__()
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

    //-------- Scenario 144
    namespace Scenario144
    {

        namespace Coalesce
        {
            public class Test
            {
                // Test for functionality: left is a nullable type and right is nullable type
                // Verify IsLifted and IsLiftedToNull are false. Test Case for DDB: 35132.
                public static int Test6()
                {
                    ConstantExpression left = Expression.Constant(null, typeof(int?));
                    ConstantExpression right = Expression.Constant(9, typeof(int?));

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = right.Type;
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Coalesce(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, (MethodInfo)null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce6__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce6__()
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

    //-------- Scenario 145
    namespace Scenario145
    {

        namespace Coalesce
        {
            public class Test
            {
                // Test for functionality: An Implicit conversion exists from left type to right type, result type is right type
                // Test Case for DDB: 37798.
                public static int Test7()
                {
                    Foo f1 = new Foo();
                    Bar b1 = new Bar();
                    ConstantExpression left = Expression.Constant(b1, typeof(Bar));
                    ConstantExpression right = Expression.Constant(f1, typeof(Foo));

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = typeof(Foo);
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Coalesce(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, (MethodInfo)null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce7__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce7__()
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



        namespace Coalesce
        {
            public class Foo
            {
            }

            public class Bar : Foo
            {
            }
        }

    }

    //-------- Scenario 146
    namespace Scenario146
    {

        namespace Coalesce
        {
            public class Test
            {
                // Test for functionality: An Implicit conversion exists from right type to left type, result type is left type
                // Test Case for DDB: 37798.
                public static int Test8()
                {
                    Foo f1 = new Foo();
                    Bar b1 = new Bar();
                    ConstantExpression left = Expression.Constant(f1, typeof(Foo));
                    ConstantExpression right = Expression.Constant(b1, typeof(Bar));

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = typeof(Foo);
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Coalesce(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, (MethodInfo)null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce8__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce8__()
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



        namespace Coalesce
        {
            public class Foo
            {
            }

            public class Bar : Foo
            {
            }
        }

    }

    //-------- Scenario 147
    namespace Scenario147
    {

        namespace Coalesce
        {
            public class Test
            {
                // right type is implicitly convertible to left type when nullability is discarded
                // Test Case for DDB: 37241
                public static int Test9()
                {
                    ConstantExpression left = Expression.Constant(40L, typeof(long?));
                    ConstantExpression right = Expression.Constant(36, typeof(int));

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = typeof(long);
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Coalesce(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, (MethodInfo)null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce9__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce9__()
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


    }

    //-------- Scenario 148
    namespace Scenario148
    {

        namespace Coalesce
        {
            public class Test
            {
                // right type is implicitly convertible to left type
                // Test Case for DDB: 37241
                public static int Test10()
                {
                    ConstantExpression left = Expression.Constant(40L, typeof(long?));
                    ConstantExpression right = Expression.Constant(36, typeof(int?));

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = typeof(long?);
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Coalesce(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, (MethodInfo)null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce10__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce10__()
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


    }

    //-------- Scenario 149
    namespace Scenario149
    {

        namespace Coalesce
        {
            public class Test
            {

                // left type is implicitly convertible to right type
                // Test Case for DDB: 37241
                public static int Test11()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant(36L, typeof(long?));

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = typeof(long?);
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Coalesce(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, (MethodInfo)null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce11__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce11__()
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


    }

    //-------- Scenario 150
    namespace Scenario150
    {

        namespace Coalesce
        {
            public class Test
            {
                // left type without nullability is implicitly convertible to right type
                // Test Case for DDB: 37241
                public static int Test12()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant(36L, typeof(long));

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = typeof(long);
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Coalesce(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, (MethodInfo)null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce12__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce12__()
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


    }

    //-------- Scenario 151
    namespace Scenario151
    {

        namespace Coalesce
        {
            public class Test
            {
                // Overload-2: left = null
                public static int Test13a()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant(36L, typeof(long));

                    ParameterExpression param1 = Expression.Parameter(typeof(int), "intVarParm");
                    ConstantExpression body = Expression.Constant(40L, typeof(long));
                    LambdaExpression conversion = Expression.Lambda(body, new ParameterExpression[] { param1 });

                    try
                    {
                        left = null;
                        BinaryExpression result = Expression.Coalesce(left, right, conversion);
                        return 1;
                    }
                    catch (ArgumentNullException ane)
                    {
                        if (!ane.CompareParamName("left")) return 1;
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13a__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13a__()
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
                    return Test13a();
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

    //-------- Scenario 152
    namespace Scenario152
    {

        namespace Coalesce
        {
            public class Test
            {
                // Overload-2: right = null
                public static int Test13b()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant(36L, typeof(long));

                    ParameterExpression param1 = Expression.Parameter(typeof(int), "intVarParm");
                    ConstantExpression body = Expression.Constant(40L, typeof(long));
                    LambdaExpression conversion = Expression.Lambda(body, new ParameterExpression[] { param1 });

                    try
                    {
                        right = null;
                        BinaryExpression result = Expression.Coalesce(left, right, conversion);
                        return 1;
                    }
                    catch (ArgumentNullException ane)
                    {
                        if (!ane.CompareParamName("right")) return 1;
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13b__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13b__()
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
                    return Test13b();
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

    //-------- Scenario 153
    namespace Scenario153
    {

        namespace Coalesce
        {
            public class Test
            {
                // Overload-2: conversion = null
                public static int Test13c()
                {
                    ConstantExpression left = Expression.Constant(null, typeof(int?));
                    ConstantExpression right = Expression.Constant(9, typeof(int));

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = right.Type;
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Coalesce(left, right, (LambdaExpression)null);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, (MethodInfo)null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13c__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13c__()
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
                    return Test13c();
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

    //-------- Scenario 154
    namespace Scenario154
    {

        namespace Coalesce
        {
            public class Test
            {
                // Overload-2: left is non-nullable value type
                public static int Test13d()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int));
                    ConstantExpression right = Expression.Constant(36L, typeof(long));

                    ParameterExpression param1 = Expression.Parameter(typeof(int), "intVarParm");
                    ConstantExpression body = Expression.Constant(40L, typeof(long));
                    LambdaExpression conversion = Expression.Lambda(body, new ParameterExpression[] { param1 });

                    try
                    {
                        BinaryExpression result = Expression.Coalesce(left, right, conversion);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13d__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13d__()
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
                    return Test13d();
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

    //-------- Scenario 155
    namespace Scenario155
    {

        namespace Coalesce
        {
            public class Test
            {
                // Helper method returning void for Test13e
                public static void HelperMeth1()
                {
                }

                // Overload-2: LambdaExpression convertion returns void
                public static int Test13e()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant(36L, typeof(long));

                    MethodCallExpression body = Expression.Call(typeof(Test).GetMethod("HelperMeth1"), (Expression[])null);
                    LambdaExpression conversion = Expression.Lambda<Action>(body, new ParameterExpression[] { });

                    try
                    {
                        BinaryExpression result = Expression.Coalesce(left, right, conversion);
                        return 1;
                    }
                    catch (ArgumentException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13e__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13e__()
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
                    return Test13e();
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

    //-------- Scenario 156
    namespace Scenario156
    {

        namespace Coalesce
        {
            public class Test
            {
                // Overload-2: LambdaExpression conversion has zero parameters (not equal to one)
                public static int Test13f()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant(36L, typeof(long));

                    ConstantExpression body = Expression.Constant(40L, typeof(long));
                    LambdaExpression conversion = Expression.Lambda(body, new ParameterExpression[] { });

                    try
                    {
                        BinaryExpression result = Expression.Coalesce(left, right, conversion);
                        return 1;
                    }
                    catch (ArgumentException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13f__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13f__()
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
                    return Test13f();
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

    //-------- Scenario 157
    namespace Scenario157
    {

        namespace Coalesce
        {
            public class Test
            {
                // Overload-2: LambdaExpression conversion has two parameters (not equal to one)
                public static int Test13g()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant(36L, typeof(long));

                    ParameterExpression param1 = Expression.Parameter(typeof(int), "intVarParm1");
                    ParameterExpression param2 = Expression.Parameter(typeof(int), "intVarParm2");
                    ConstantExpression body = Expression.Constant(40L, typeof(long));
                    LambdaExpression conversion = Expression.Lambda(body, new ParameterExpression[] { param1, param2 });

                    try
                    {
                        BinaryExpression result = Expression.Coalesce(left, right, conversion);
                        return 1;
                    }
                    catch (ArgumentException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13g__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13g__()
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
                    return Test13g();
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

    //-------- Scenario 158
    namespace Scenario158
    {

        namespace Coalesce
        {
            public class Test
            {
                // Overload-2:Right type does not match the return type of the Lambda
                public static int Test13h()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant(36L, typeof(long));

                    ParameterExpression param1 = Expression.Parameter(typeof(int), "intVarParm1");
                    ConstantExpression body = Expression.Constant(40L, typeof(long?));
                    LambdaExpression conversion = Expression.Lambda(body, new ParameterExpression[] { param1 });

                    try
                    {
                        BinaryExpression result = Expression.Coalesce(left, right, conversion);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13h__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13h__()
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
                    return Test13h();
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

    //-------- Scenario 159
    namespace Scenario159
    {

        namespace Coalesce
        {
            public class Test
            {
                // Overload-2: parameter is not assignable to both nullable and non-nullable left.Type
                public static int Test13i()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant(36L, typeof(long));

                    ParameterExpression param1 = Expression.Parameter(typeof(long?), "intVarParm1");
                    ConstantExpression body = Expression.Constant(40L, typeof(long));
                    LambdaExpression conversion = Expression.Lambda(body, new ParameterExpression[] { param1 });

                    try
                    {
                        BinaryExpression result = Expression.Coalesce(left, right, conversion);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13i__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13i__()
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
                    return Test13i();
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

    //-------- Scenario 160
    namespace Scenario160
    {

        namespace Coalesce
        {
            public class Test
            {
                // Overload-2: parameter is assignable to the nullable left.Type
                public static int Test13j()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant(36L, typeof(long));

                    ParameterExpression param1 = Expression.Parameter(typeof(int?), "intVarParm1");
                    ConstantExpression body = Expression.Constant(40L, typeof(long));
                    LambdaExpression conversion = Expression.Lambda(body, new ParameterExpression[] { param1 });

                    BinaryExpression result = Expression.Coalesce(left, right, conversion);

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = right.Type;
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, conversion, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13j__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13j__()
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
                    return Test13j();
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

    //-------- Scenario 161
    namespace Scenario161
    {

        namespace Coalesce
        {
            public class Test
            {
                // Overload-2: parameter is assignable to the non-nullable left.Type
                public static int Test13k()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant(36L, typeof(long));

                    ParameterExpression param1 = Expression.Parameter(typeof(int), "intVarParm1");
                    ConstantExpression body = Expression.Constant(40L, typeof(long));
                    LambdaExpression conversion = Expression.Lambda(body, new ParameterExpression[] { param1 });

                    BinaryExpression result = Expression.Coalesce(left, right, conversion);

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = right.Type;
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, conversion, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13k__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13k__()
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
                    return Test13k();
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

    //-------- Scenario 162
    namespace Scenario162
    {

        namespace Coalesce
        {
            public class Test
            {
                // Overload-2: arguments are not of value types
                public static int Test13l()
                {

                    Foo f1 = new Foo();
                    Bar b1 = new Bar();

                    ConstantExpression left = Expression.Constant(b1, typeof(Bar));
                    ConstantExpression right = Expression.Constant(f1, typeof(Foo));

                    ParameterExpression param1 = Expression.Parameter(typeof(Foo), "intVarParm1");
                    ConstantExpression body = Expression.Constant(f1, typeof(Foo));
                    LambdaExpression conversion = Expression.Lambda(body, new ParameterExpression[] { param1 });

                    BinaryExpression result = Expression.Coalesce(left, right, conversion);

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = right.Type;
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, conversion, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13l__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13l__()
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
                    return Test13l();
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



        namespace Coalesce
        {
            public class Foo
            {
            }

            public class Bar : Foo
            {
            }
        }

    }

    //-------- Scenario 163
    namespace Scenario163
    {

        namespace Coalesce
        {
            public class Test
            {
                // Overload-2: coalesce operator between class and value type
                public static int Test13m()
                {

                    Foo f1 = new Foo();
                    Bar b1 = new Bar();

                    ConstantExpression left = Expression.Constant(b1, typeof(Bar));
                    ConstantExpression right = Expression.Constant(10, typeof(int));

                    ParameterExpression param1 = Expression.Parameter(typeof(Foo), "intVarParm1");
                    ConstantExpression body = Expression.Constant(45, typeof(int));
                    LambdaExpression conversion = Expression.Lambda(body, new ParameterExpression[] { param1 });

                    BinaryExpression result = Expression.Coalesce(left, right, conversion);

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = right.Type;
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, conversion, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Coalesce13m__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Coalesce13m__()
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
                    return Test13m();
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



        namespace Coalesce
        {
            public class Foo
            {
            }

            public class Bar : Foo
            {
            }
        }

    }

}
