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


    //-------- Scenario 669
    namespace Scenario669
    {

        namespace Negate
        {
            public class Test
            {
                // expression = null
                public static int Test1()
                {
                    ConstantExpression ce = Expression.Constant(10);

                    try
                    {
                        ce = null;
                        UnaryExpression result = Expression.Negate(ce);
                        return 1;
                    }
                    catch (ArgumentNullException ane)
                    {
                        if (ane.CompareParamName("expression")) return 0;
                        return 1;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate1__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate1__()
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

    //-------- Scenario 670
    namespace Scenario670
    {

        namespace Negate
        {
            public class Test
            {
                // Test for functionality: Argument is of type Arithmetic (int)
                public static int Test2()
                {
                    ConstantExpression ce = Expression.Constant(10);
                    Type exp_type = ce.Type;
                    ExpressionType exp_et = ExpressionType.Negate;
                    string exp_str = "-" + ce.ToString();
                    Expression exp_operand = ce;

                    UnaryExpression result = Expression.Negate(ce);

                    return Verification.VerifyUnaryParms(result, exp_et, exp_type, exp_operand, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate2__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate2__()
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

    //-------- Scenario 671
    namespace Scenario671
    {

        namespace Negate
        {
            public class Test
            {
                // Test for functionality: Argument is NOT of type numeric
                public static int Test3a()
                {
                    ConstantExpression ce = Expression.Constant("Test");

                    try
                    {
                        UnaryExpression result = Expression.Negate(ce);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate3a__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate3a__()
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
                    return Test3a();
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

    //-------- Scenario 672
    namespace Scenario672
    {

        namespace Negate
        {
            public class Test
            {
                // Test for functionality: Argument is of type numeric (byte) => (Test to verify IsArithmetic is called and not IsNumeric)
                public static int Test3b()
                {
                    ConstantExpression ce = Expression.Constant((byte)10);

                    try
                    {
                        UnaryExpression result = Expression.Negate(ce);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate3b__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate3b__()
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
                    return Test3b();
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

    //-------- Scenario 673
    namespace Scenario673
    {

        namespace Negate
        {
            public class Test
            {
                // Test for functionality: Argument is of type numeric (UINT16) => (test to verify IsUnsigned)
                public static int Test3c()
                {
                    ConstantExpression ce = Expression.Constant((UInt16)10);

                    try
                    {
                        UnaryExpression result = Expression.Negate(ce);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate3c__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate3c__()
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
                    return Test3c();
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

    //-------- Scenario 674
    namespace Scenario674
    {

        namespace Negate
        {
            public class Test
            {
                // Test for functionality: Argument is of type numeric (UINT32) => (test to verify IsUnsigned)
                public static int Test3d()
                {
                    ConstantExpression ce = Expression.Constant((UInt32)10);

                    try
                    {
                        UnaryExpression result = Expression.Negate(ce);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate3d__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate3d__()
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
                    return Test3d();
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

    //-------- Scenario 675
    namespace Scenario675
    {

        namespace Negate
        {
            public class Test
            {
                // Test for functionality: Argument is of type numeric (UINT64) => (test to verify IsUnsigned)
                public static int Test3e()
                {
                    ConstantExpression ce = Expression.Constant((UInt64)10);

                    try
                    {
                        UnaryExpression result = Expression.Negate(ce);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate3e__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate3e__()
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
                    return Test3e();
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

    //-------- Scenario 676
    namespace Scenario676
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-1: Test for functionality: Argument is of type user-defined type
                public static int Test4()
                {
                    Complex ctest = new Complex();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex));
                    Type exp_type = ce.Type;
                    ExpressionType exp_et = ExpressionType.Negate;
                    string exp_str = "-" + ce.ToString();
                    Expression exp_operand = ce;
                    MethodInfo exp_mi = typeof(Complex).GetMethod("op_UnaryNegation");

                    UnaryExpression result = Expression.Negate(ce);

                    return Verification.VerifyUnaryParms(result, exp_et, exp_type, exp_operand, exp_str, exp_mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate4__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate4__()
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 677
    namespace Scenario677
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-1: Test for functionality: Argument is of type user-defined type, and methodinfo not defined for the operator
                public static int Test5()
                {
                    Complex_Noop ctest = new Complex_Noop();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex_Noop));

                    try
                    {
                        UnaryExpression result = Expression.Negate(ce);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate5__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate5__()
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 678
    namespace Scenario678
    {

        namespace Negate
        {
            public class Test
            {
                // Test for functionality: Argument is of type nullable numeric
                public static int Test6()
                {
                    ConstantExpression ce = Expression.Constant(10, typeof(int?));
                    Type exp_type = ce.Type;
                    ExpressionType exp_et = ExpressionType.Negate;
                    string exp_str = "-" + ce.ToString();
                    Expression exp_operand = ce;

                    UnaryExpression result = Expression.Negate(ce);

                    return Verification.VerifyUnaryParms(result, exp_et, exp_type, exp_operand, exp_str, null, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate6__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate6__()
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

    //-------- Scenario 679
    namespace Scenario679
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-1: Test for functionality: Argument is of type nullable user-defined type
                public static int Test7()
                {
                    Complex? ctest = new Complex?();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex?));
                    Type exp_type = ce.Type;
                    ExpressionType exp_et = ExpressionType.Negate;
                    string exp_str = "-" + ce.ToString();
                    Expression exp_operand = ce;
                    MethodInfo exp_mi = typeof(Complex).GetMethod("op_UnaryNegation");

                    UnaryExpression result = Expression.Negate(ce);

                    return Verification.VerifyUnaryParms(result, exp_et, exp_type, exp_operand, exp_str, exp_mi, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate7__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate7__()
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 680
    namespace Scenario680
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-1: Test for functionality: User-defined type (passing Complex2? with return type Complex2)
                public static int Test8()
                {
                    Complex2 ctest = new Complex2();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex2));

                    try
                    {
                        UnaryExpression result = Expression.Negate(ce);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate8__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate8__()
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 681
    namespace Scenario681
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-1: Test for functionality: User-defined type (passing Complex3 with return type Complex3?)
                public static int Test9()
                {
                    Complex3 ctest = new Complex3();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex3));

                    try
                    {
                        UnaryExpression result = Expression.Negate(ce);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate9__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate9__()
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 682
    namespace Scenario682
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-1: Test for functionality: User-defined type (passing Complex4? when Complex4 is expected with return type Complex4?)
                public static int Test10()
                {
                    Complex4? ctest = new Complex4?();

                    try
                    {
                        ConstantExpression ce = Expression.Constant(ctest, typeof(Complex4?));
                        UnaryExpression result = Expression.Negate(ce);
                        return 1;
                    }
                    catch (InvalidOperationException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate10__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate10__()
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 683
    namespace Scenario683
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-2: Passing "null" methodinfo for user-defined type
                public static int Test11a()
                {
                    Complex ctest = new Complex();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex));
                    Type exp_type = ce.Type;
                    ExpressionType exp_et = ExpressionType.Negate;
                    string exp_str = "-" + ce.ToString();
                    Expression exp_operand = ce;
                    MethodInfo exp_mi = typeof(Complex).GetMethod("op_UnaryNegation");

                    UnaryExpression result = Expression.Negate(ce, null);

                    return Verification.VerifyUnaryParms(result, exp_et, exp_type, exp_operand, exp_str, exp_mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate11a__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate11a__()
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
                    return Test11a();
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 684
    namespace Scenario684
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-2: Expression is null and MethodInfo is non-null
                public static int Test11b()
                {
                    Complex ctest = new Complex();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex));
                    MethodInfo exp_mi = typeof(Complex).GetMethod("Meth4");

                    try
                    {
                        ce = null;
                        UnaryExpression result = Expression.Negate(ce, exp_mi);
                        return 1;
                    }
                    catch (ArgumentNullException ane)
                    {
                        if (!ane.CompareParamName("expression")) return 1;
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate11b__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate11b__()
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
                    return Test11b();
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 685
    namespace Scenario685
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-2: Passing non-static methodinfo
                public static int Test12()
                {
                    Complex ctest = new Complex();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex));
                    MethodInfo exp_mi = typeof(Complex).GetMethod("Meth1");

                    try
                    {
                        UnaryExpression result = Expression.Negate(ce, exp_mi);
                        return 1;
                    }
                    catch (ArgumentException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate12__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate12__()
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 686
    namespace Scenario686
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-2: Passing methodinfo with void return type
                public static int Test13()
                {
                    Complex ctest = new Complex();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex));
                    MethodInfo exp_mi = typeof(Complex).GetMethod("Meth2");

                    try
                    {
                        UnaryExpression result = Expression.Negate(ce, exp_mi);
                        return 1;
                    }
                    catch (ArgumentException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate13__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate13__()
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 687
    namespace Scenario687
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-2: Passing methodinfo with more than one parameter
                public static int Test14()
                {
                    Complex ctest = new Complex();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex));
                    MethodInfo exp_mi = typeof(Complex).GetMethod("Meth3");

                    try
                    {
                        UnaryExpression result = Expression.Negate(ce, exp_mi);
                        return 1;
                    }
                    catch (ArgumentException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate14__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate14__()
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 688
    namespace Scenario688
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-2: Passing methodinfo for user-defined type
                public static int Test15()
                {
                    Complex ctest = new Complex();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex));
                    Type exp_type = ce.Type;
                    ExpressionType exp_et = ExpressionType.Negate;
                    string exp_str = "-" + ce.ToString();
                    Expression exp_operand = ce;
                    MethodInfo exp_mi = typeof(Complex).GetMethod("Meth4");

                    UnaryExpression result = Expression.Negate(ce, exp_mi);

                    return Verification.VerifyUnaryParms(result, exp_et, exp_type, exp_operand, exp_str, exp_mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate15__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate15__()
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 689
    namespace Scenario689
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-2: Passing methodinfo for user-defined type with nullable parameter
                public static int Test16()
                {
                    Complex? ctest = new Complex?();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex?));
                    Type exp_type = typeof(Complex?);
                    ExpressionType exp_et = ExpressionType.Negate;
                    string exp_str = "-" + ce.ToString();
                    Expression exp_operand = ce;
                    MethodInfo exp_mi = typeof(Complex).GetMethod("Meth4");

                    UnaryExpression result = Expression.Negate(ce, exp_mi);

                    return Verification.VerifyUnaryParms(result, exp_et, exp_type, exp_operand, exp_str, exp_mi, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate16__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate16__()
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

    //-------- Scenario 690
    namespace Scenario690
    {

        namespace Negate
        {
            public class Test
            {
                // Overload-2: Passing a methodinfo for user-defined type with nullable parameter (with a int return type)
                public static int Test17()
                {
                    Complex? ctest = new Complex?();
                    ConstantExpression ce = Expression.Constant(ctest, typeof(Complex?));
                    Type exp_type = typeof(int?);
                    ExpressionType exp_et = ExpressionType.Negate;
                    string exp_str = "-" + ce.ToString();
                    Expression exp_operand = ce;
                    MethodInfo exp_mi = typeof(Complex).GetMethod("Meth5");

                    UnaryExpression result = Expression.Negate(ce, exp_mi);

                    return Verification.VerifyUnaryParms(result, exp_et, exp_type, exp_operand, exp_str, exp_mi, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Negate17__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression Negate17__()
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



        namespace Negate
        {
            // Declaring a user-defined type with negate operator
            struct Complex
            {
                private int x;
                private int y;

                public static Complex operator -(Complex c)
                {
                    Complex temp = new Complex();
                    temp.x = -c.x;
                    temp.y = -c.y;
                    return temp;
                }

                public Complex Meth1(Complex c)
                {
                    return new Complex();
                }

                public static void Meth2(Complex c)
                {
                }

                public static Complex Meth3(Complex c, Complex d)
                {
                    return new Complex();
                }

                public static Complex Meth4(Complex c)
                {
                    return new Complex();
                }

                public static int Meth5(Complex c)
                {
                    return 5;
                }
            }

            // Declaring a user-defined type without negate operator
            struct Complex_Noop
            {
                private int x;
                private int y;
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param)
            struct Complex2
            {
                public static Complex2 operator -(Complex2? c)
                {
                    return new Complex2();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable param and return)
            struct Complex3
            {
                public static Complex3? operator -(Complex3? c)
                {
                    return new Complex3?();
                }
            }

            // Declaring a user-defined type with overloaded "-" operator (with nullable return)
            struct Complex4
            {
                public static Complex4? operator -(Complex4 c)
                {
                    return new Complex4?();
                }
            }

        }

    }

}
