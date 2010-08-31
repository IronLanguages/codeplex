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


    //-------- Scenario 0
    namespace Scenario0
    {

        namespace Add
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
                        BinaryExpression result = Expression.Add(left, right);
                        return 1;
                    }
                    catch (ArgumentNullException ane)
                    {
                        if (ane.CompareParamName("left")) return 0;
                        return 1;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add1__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add1__()
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

    //-------- Scenario 1
    namespace Scenario1
    {

        namespace Add
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
                        BinaryExpression result = Expression.Add(left, right);
                        return 1;
                    }
                    catch (ArgumentNullException ane)
                    {
                        if (ane.CompareParamName("right")) return 0;
                        return 1;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add2__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add2__()
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

    //-------- Scenario 2
    namespace Scenario2
    {

        namespace Add
        {
            public class Test
            {
                // Test for functionality: left is of type boolean (non-numeric)
                public static int Test3()
                {
                    ConstantExpression left = Expression.Constant(true, typeof(bool));
                    ConstantExpression right = Expression.Constant(9);

                    try
                    {
                        BinaryExpression result = Expression.Add(left, right);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add3__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add3__()
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

    //-------- Scenario 3
    namespace Scenario3
    {

        namespace Add
        {
            public class Test
            {
                // Test for functionality: right is of type string (non-numeric)
                public static int Test4a()
                {
                    ConstantExpression left = Expression.Constant(10, typeof(int));
                    ConstantExpression right = Expression.Constant("Test", typeof(string));

                    try
                    {
                        BinaryExpression result = Expression.Add(left, right);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add4a__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add4a__()
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
                    return Test4a();
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

    //-------- Scenario 4
    namespace Scenario4
    {

        namespace Add
        {
            public class Test
            {
                // Test for functionality: left and right is of same numeric type (byte) (verify that IsArithmetic is called instead of IsNumeric)
                public static int Test4b()
                {
                    ConstantExpression left = Expression.Constant((byte)10, typeof(byte));
                    ConstantExpression right = Expression.Constant((byte)10, typeof(byte));

                    try
                    {
                        BinaryExpression result = Expression.Add(left, right);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add4b__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add4b__()
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
                    return Test4b();
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

    //-------- Scenario 5
    namespace Scenario5
    {

        namespace Add
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
                        BinaryExpression result = Expression.Add(left, right);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add5__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add5__()
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

    //-------- Scenario 6
    namespace Scenario6
    {

        namespace Add
        {
            public class Test
            {
                // Test for functionality: Node has the right information
                public static int Test6()
                {
                    ConstantExpression left = Expression.Constant(10, typeof(int));
                    ConstantExpression right = Expression.Constant(12, typeof(int));

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Add(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add6__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add6__()
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

    //-------- Scenario 7
    namespace Scenario7
    {

        namespace Add
        {
            public class Test
            {
                // Test for functionality: Node has the right information; nullable type
                public static int Test7()
                {
                    ConstantExpression left = Expression.Constant(10, typeof(int?));
                    ConstantExpression right = Expression.Constant(12, typeof(int?));

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Add(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add7__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add7__()
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

    //-------- Scenario 8
    namespace Scenario8
    {

        namespace Add
        {
            public class Test
            {
                // Overload-1: User-Defined type: Node has the right information
                public static int Test8()
                {
                    TC1 tc1 = new TC1();
                    ConstantExpression left = Expression.Constant(tc1, typeof(TC1));
                    ConstantExpression right = Expression.Constant(tc1, typeof(TC1));
                    MethodInfo mi = typeof(TC1).GetMethod("op_Addition");

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Add(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add8__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add8__()
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 9
    namespace Scenario9
    {

        namespace Add
        {
            public class Test
            {
                // Overload-1: User-Defined type: Node has the right information: Nullable types
                public static int Test9()
                {
                    TC1 tc1 = new TC1();
                    ConstantExpression left = Expression.Constant(tc1, typeof(TC1?));
                    ConstantExpression right = Expression.Constant(tc1, typeof(TC1?));
                    MethodInfo mi = typeof(TC1).GetMethod("op_Addition");

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Add(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add9__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add9__()
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 10
    namespace Scenario10
    {

        namespace Add
        {
            public class Test
            {
                // Overload-1: User-Defined type: Operator not defined
                public static int Test10()
                {
                    TC2 tc2 = new TC2();
                    ConstantExpression left = Expression.Constant(tc2, typeof(TC2?));
                    ConstantExpression right = Expression.Constant(tc2, typeof(TC2?));

                    try
                    {
                        BinaryExpression result = Expression.Add(left, right);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add10__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add10__()
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 11
    namespace Scenario11
    {

        namespace Add
        {
            public class Test
            {
                // Overload-1: User-Defined type: multiple types
                public static int Test11()
                {
                    TC3 tc3 = new TC3();
                    TC2 tc2 = new TC2();
                    ConstantExpression left = Expression.Constant(tc3, typeof(TC3));
                    ConstantExpression right = Expression.Constant(tc2, typeof(TC2));
                    MethodInfo mi = typeof(TC3).GetMethod("op_Addition");

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Add(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add11__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add11__()
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 12
    namespace Scenario12
    {

        namespace Add
        {
            public class Test
            {
                // Overload-1: User-Defined type: multiple types: Nullable types
                public static int Test12()
                {
                    TC3 tc3 = new TC3();
                    TC2 tc2 = new TC2();
                    ConstantExpression left = Expression.Constant(tc3, typeof(TC3?));
                    ConstantExpression right = Expression.Constant(tc2, typeof(TC2?));
                    MethodInfo mi = typeof(TC3).GetMethod("op_Addition");

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Add(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add12__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add12__()
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 13
    namespace Scenario13
    {

        namespace Add
        {
            public class Test
            {
                // Overload-1: User-Defined type: multiple types (overloaded operator defined on right.Type)
                public static int Test13()
                {
                    TC3 tc3 = new TC3();
                    TC4 tc4 = new TC4();
                    ConstantExpression left = Expression.Constant(tc3, typeof(TC3));
                    ConstantExpression right = Expression.Constant(tc4, typeof(TC4));
                    MethodInfo mi = typeof(TC4).GetMethod("op_Addition");

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Add(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add13__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add13__()
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 14
    namespace Scenario14
    {

        namespace Add
        {
            public class Test
            {
                // Overload-1: User-Defined type: multiple types: Nullable types (overloaded operator defined on right.Type)
                public static int Test14()
                {
                    TC3 tc3 = new TC3();
                    TC4 tc4 = new TC4();
                    ConstantExpression left = Expression.Constant(tc3, typeof(TC3?));
                    ConstantExpression right = Expression.Constant(tc4, typeof(TC4?));
                    MethodInfo mi = typeof(TC4).GetMethod("op_Addition");

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Add(left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add14__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add14__()
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
                    return Test14();
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 15
    namespace Scenario15
    {

        namespace Add
        {
            public class Test
            {
                // Overload-2: User-Defined type: multiple types: Test for null as MethodInfo
                public static int Test15()
                {
                    TC3 tc3 = new TC3();
                    TC4 tc4 = new TC4();
                    ConstantExpression left = Expression.Constant(tc3, typeof(TC3));
                    ConstantExpression right = Expression.Constant(tc4, typeof(TC4));
                    MethodInfo mi = typeof(TC4).GetMethod("op_Addition");

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Add(left, right, null);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add15__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add15__()
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
                    return Test15();
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 16
    namespace Scenario16
    {

        namespace Add
        {
            public class Test
            {
                // Overload-2: User-Defined type: multiple types: Test for non-null as MethodInfo
                public static int Test16()
                {
                    TC2 tc2 = new TC2();
                    ConstantExpression left = Expression.Constant(tc2, typeof(TC2));
                    ConstantExpression right = Expression.Constant(tc2, typeof(TC2));
                    MethodInfo mi = typeof(TC2).GetMethod("Meth1");

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Add(left, right, mi);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add16__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add16__()
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
                    return Test16();
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 17
    namespace Scenario17
    {

        namespace Add
        {
            public class Test
            {
                // Overload-2: User-Defined type: void MethodInfo should throw an Exception
                public static int Test17()
                {
                    TC2 tc2 = new TC2();
                    ConstantExpression left = Expression.Constant(tc2, typeof(TC2));
                    ConstantExpression right = Expression.Constant(tc2, typeof(TC2));
                    MethodInfo mi = typeof(TC2).GetMethod("Meth2");

                    try
                    {
                        BinaryExpression result = Expression.Add(left, right, mi);
                        return 1;
                    }
                    catch (ArgumentException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add17__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add17__()
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
                    return Test17();
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 18
    namespace Scenario18
    {

        namespace Add
        {
            public class Test
            {
                // Overload-2: User-Defined type: non-static MethodInfo should throw an Exception
                public static int Test18()
                {
                    TC2 tc2 = new TC2();
                    ConstantExpression left = Expression.Constant(tc2, typeof(TC2));
                    ConstantExpression right = Expression.Constant(tc2, typeof(TC2));
                    MethodInfo mi = typeof(TC2).GetMethod("Meth3");

                    try
                    {
                        BinaryExpression result = Expression.Add(left, right, mi);
                        return 1;
                    }
                    catch (ArgumentException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add18__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add18__()
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
                    return Test18();
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 19
    namespace Scenario19
    {

        namespace Add
        {
            public class Test
            {
                // Overload-2: User-Defined type: MethodInfo with one argument
                public static int Test19()
                {
                    TC2 tc2 = new TC2();
                    ConstantExpression left = Expression.Constant(tc2, typeof(TC2));
                    ConstantExpression right = Expression.Constant(tc2, typeof(TC2));
                    MethodInfo mi = typeof(TC2).GetMethod("Meth4");

                    try
                    {
                        BinaryExpression result = Expression.Add(left, right, mi);
                        return 1;
                    }
                    catch (ArgumentException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add19__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add19__()
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
                    return Test19();
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 20
    namespace Scenario20
    {

        namespace Add
        {
            public class Test
            {
                // Overload-2: User-Defined type: MethodInfo with 3 arguments
                public static int Test20()
                {
                    TC2 tc2 = new TC2();
                    ConstantExpression left = Expression.Constant(tc2, typeof(TC2));
                    ConstantExpression right = Expression.Constant(tc2, typeof(TC2));
                    MethodInfo mi = typeof(TC2).GetMethod("Meth5");

                    try
                    {
                        BinaryExpression result = Expression.Add(left, right, mi);
                        return 1;
                    }
                    catch (ArgumentException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add20__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add20__()
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
                    return Test20();
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 21
    namespace Scenario21
    {

        namespace Add
        {
            public class Test
            {
                // Overload-2: User-Defined type: nullable types
                public static int Test21()
                {
                    TC2 tc2 = new TC2();
                    ConstantExpression left = Expression.Constant(tc2, typeof(TC2?));
                    ConstantExpression right = Expression.Constant(tc2, typeof(TC2?));
                    MethodInfo mi = typeof(TC2).GetMethod("Meth1");

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Add(left, right, mi);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add21__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add21__()
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
                    return Test21();
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 22
    namespace Scenario22
    {

        namespace Add
        {
            public class Test
            {
                // Overload-2: User-Defined type: (right.Type) One of the types non-nullable throws an Exception
                public static int Test22()
                {
                    TC2 tc2 = new TC2();
                    ConstantExpression left = Expression.Constant(tc2, typeof(TC2?));
                    ConstantExpression right = Expression.Constant(tc2, typeof(TC2));
                    MethodInfo mi = typeof(TC2).GetMethod("Meth1");

                    try
                    {
                        BinaryExpression result = Expression.Add(left, right, mi);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add22__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add22__()
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
                    return Test22();
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 23
    namespace Scenario23
    {

        namespace Add
        {
            public class Test
            {
                // Overload-2: User-Defined type: (left.Type) One of the types non-nullable throws an Exception
                public static int Test23()
                {
                    TC2 tc2 = new TC2();
                    ConstantExpression left = Expression.Constant(tc2, typeof(TC2));
                    ConstantExpression right = Expression.Constant(tc2, typeof(TC2?));
                    MethodInfo mi = typeof(TC2).GetMethod("Meth1");

                    try
                    {
                        BinaryExpression result = Expression.Add(left, right, mi);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add23__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add23__()
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
                    return Test23();
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 24
    namespace Scenario24
    {

        namespace Add
        {
            public class Test
            {
                // Overload-2: User-Defined type: passing nullable types to nullable arguments
                public static int Test24()
                {
                    TC5 tc5 = new TC5();
                    ConstantExpression left = Expression.Constant(tc5, typeof(TC5?));
                    ConstantExpression right = Expression.Constant(tc5, typeof(TC5?));
                    MethodInfo mi = typeof(TC5).GetMethod("Meth1");

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = typeof(TC5);
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.Add(left, right, mi);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add24__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add24__()
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
                    return Test24();
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

    //-------- Scenario 25
    namespace Scenario25
    {

        namespace Add
        {
            public class Test
            {
                // Overload-2: User-Defined type: passing non-nullable types to nullable arguments
                public static int Test25()
                {
                    TC5 tc5 = new TC5();
                    ConstantExpression left = Expression.Constant(tc5, typeof(TC5));
                    ConstantExpression right = Expression.Constant(tc5, typeof(TC5));
                    MethodInfo mi = typeof(TC5).GetMethod("Meth1");

                    try
                    {
                        BinaryExpression result = Expression.Add(left, right, mi);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Add25__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Add25__()
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
                    return Test25();
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



        namespace Add
        {
            // Helper struct for user-defined operator
            struct TC1
            {
                public static TC1 operator +(TC1 t1, TC1 t2)
                {
                    return new TC1();
                }
            }

            // Pre-Defined Binary Operator not defined
            struct TC2
            {
                public static TC2 Meth1(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static void Meth2(TC2 t1, TC2 t2)
                {
                }

                public TC2 Meth3(TC2 t1, TC2 t2)
                {
                    return new TC2();
                }

                public static TC2 Meth4(TC2 t1)
                {
                    return new TC2();
                }

                public static TC2 Meth5(TC2 t1, TC2 t2, TC2 t3)
                {
                    return new TC2();
                }
            }
            // Pre-Defined Binary opertor defined on different classes
            struct TC3
            {
                public static TC3 operator +(TC3 t3, TC2 t2)
                {
                    return new TC3();
                }
            }

            // Pre-Defined Binary opertor defined on different classes
            struct TC4
            {
                public static TC3 operator +(TC3 t3, TC4 t4)
                {
                    return new TC3();
                }
            }

            struct TC5
            {
                public static TC5 Meth1(TC5? t1, TC5? t2)
                {
                    return new TC5();
                }
            }
        }

    }

}
