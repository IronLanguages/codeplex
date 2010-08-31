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


    //-------- Scenario 486
    namespace Scenario486
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is Add
                public static int Test1()
                {
                    ConstantExpression left = Expression.Constant(10, typeof(int));
                    ConstantExpression right = Expression.Constant(12, typeof(int));

                    ExpressionType exp_et = ExpressionType.Add;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.Add, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary1__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary1__()
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

    //-------- Scenario 487
    namespace Scenario487
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is AddChecked
                public static int Test2()
                {
                    ConstantExpression left = Expression.Constant(99, typeof(int?));
                    ConstantExpression right = Expression.Constant(18, typeof(int?));

                    ExpressionType exp_et = ExpressionType.AddChecked;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "+" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.AddChecked, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary2__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary2__()
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

    //-------- Scenario 488
    namespace Scenario488
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is Subtract
                public static int Test3()
                {
                    ConstantExpression left = Expression.Constant((short)5, typeof(short));
                    ConstantExpression right = Expression.Constant((short)10, typeof(short));

                    ExpressionType exp_et = ExpressionType.Subtract;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "-" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.Subtract, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary3__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary3__()
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

    //-------- Scenario 489
    namespace Scenario489
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is SubtractChecked
                public static int Test4()
                {
                    ConstantExpression left = Expression.Constant(99L, typeof(long));
                    ConstantExpression right = Expression.Constant(18L, typeof(long));

                    ExpressionType exp_et = ExpressionType.SubtractChecked;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "-" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.SubtractChecked, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary4__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary4__()
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

    //-------- Scenario 490
    namespace Scenario490
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is Multiply
                public static int Test5()
                {
                    ConstantExpression left = Expression.Constant(10.3m, typeof(decimal?));
                    ConstantExpression right = Expression.Constant(2.2m, typeof(decimal?));

                    ExpressionType exp_et = ExpressionType.Multiply;
                    Type exp_type = left.Type;
                    MethodInfo mi = typeof(decimal).GetMethod("op_Multiply", new Type[] { typeof(decimal), typeof(decimal) });
                    string exp_str = "(" + left.ToString() + " " + "*" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.Multiply, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary5__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary5__()
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

    //-------- Scenario 491
    namespace Scenario491
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is MultiplyChecked
                public static int Test6()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int));
                    ConstantExpression right = Expression.Constant(60, typeof(int));

                    ExpressionType exp_et = ExpressionType.MultiplyChecked;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "*" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.MultiplyChecked, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary6__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary6__()
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

    //-------- Scenario 492
    namespace Scenario492
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is Divide
                public static int Test7()
                {
                    ConstantExpression left = Expression.Constant(10L, typeof(long));
                    ConstantExpression right = Expression.Constant(12L, typeof(long));

                    ExpressionType exp_et = ExpressionType.Divide;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "/" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.Divide, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary7__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary7__()
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

    //-------- Scenario 493
    namespace Scenario493
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is Modulo
                public static int Test8()
                {
                    ConstantExpression left = Expression.Constant(10.3m, typeof(decimal));
                    ConstantExpression right = Expression.Constant(2.2m, typeof(decimal));

                    ExpressionType exp_et = ExpressionType.Modulo;
                    Type exp_type = left.Type;
                    MethodInfo mi = typeof(decimal).GetMethod("op_Modulus", new Type[] { typeof(decimal), typeof(decimal) });
                    string exp_str = "(" + left.ToString() + " " + "%" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.Modulo, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary8__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary8__()
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

    //-------- Scenario 494
    namespace Scenario494
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is And
                public static int Test9()
                {
                    ConstantExpression left = Expression.Constant(9, typeof(int?));
                    ConstantExpression right = Expression.Constant(18, typeof(int?));

                    ExpressionType exp_et = ExpressionType.And;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "&" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.And, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary9__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary9__()
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

    //-------- Scenario 495
    namespace Scenario495
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is AndAlso
                public static int Test10()
                {
                    ConstantExpression left = Expression.Constant(true, typeof(bool));
                    ConstantExpression right = Expression.Constant(false, typeof(bool));

                    ExpressionType exp_et = ExpressionType.AndAlso;
                    Type exp_type = left.Type;
#if SILVERLIGHT3
                    string exp_str = "(" + left.ToString() + " " + "&&" + " " + right.ToString() + ")";
#else
                    string exp_str = "(" + left.ToString() + " " + "AndAlso" + " " + right.ToString() + ")";
#endif

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.AndAlso, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary10__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary10__()
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

    //-------- Scenario 496
    namespace Scenario496
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is Or
                public static int Test11()
                {
                    ConstantExpression left = Expression.Constant(false, typeof(bool?));
                    ConstantExpression right = Expression.Constant(true, typeof(bool?));

                    ExpressionType exp_et = ExpressionType.Or;
                    Type exp_type = typeof(bool?);
                    string exp_str = "(" + left.ToString() + " " + "Or" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.Or, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary11__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary11__()
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

    //-------- Scenario 497
    namespace Scenario497
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is OrElse
                public static int Test12()
                {
                    ConstantExpression left = Expression.Constant(true, typeof(bool));
                    ConstantExpression right = Expression.Constant(false, typeof(bool));

                    ExpressionType exp_et = ExpressionType.OrElse;
                    Type exp_type = left.Type;
#if SILVERLIGHT3
                    string exp_str = "(" + left.ToString() + " " + "||" + " " + right.ToString() + ")";
#else
                    string exp_str = "(" + left.ToString() + " " + "OrElse" + " " + right.ToString() + ")";
#endif

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.OrElse, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary12__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary12__()
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

    //-------- Scenario 498
    namespace Scenario498
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is LessThan
                public static int Test13()
                {
                    ConstantExpression left = Expression.Constant((uint)10, typeof(uint));
                    ConstantExpression right = Expression.Constant((uint)18, typeof(uint));

                    ExpressionType exp_et = ExpressionType.LessThan;
                    Type exp_type = typeof(bool);
                    string exp_str = "(" + left.ToString() + " " + "<" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.LessThan, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary13__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary13__()
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


    }

    //-------- Scenario 499
    namespace Scenario499
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is LessThanOrEqual
                public static int Test14()
                {
                    ConstantExpression left = Expression.Constant(39.9m, typeof(decimal));
                    ConstantExpression right = Expression.Constant(84.54m, typeof(decimal));

                    ExpressionType exp_et = ExpressionType.LessThanOrEqual;
                    Type exp_type = typeof(bool);
                    MethodInfo mi = typeof(decimal).GetMethod("op_LessThanOrEqual", new Type[] { typeof(decimal), typeof(decimal) });
                    string exp_str = "(" + left.ToString() + " " + "<=" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.LessThanOrEqual, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, mi, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary14__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary14__()
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


    }

    //-------- Scenario 500
    namespace Scenario500
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is GreaterThan
                public static int Test15()
                {
                    ConstantExpression left = Expression.Constant((short)10, typeof(short));
                    ConstantExpression right = Expression.Constant((short)18, typeof(short));

                    ExpressionType exp_et = ExpressionType.GreaterThan;
                    Type exp_type = typeof(bool);
                    string exp_str = "(" + left.ToString() + " " + ">" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.GreaterThan, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary15__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary15__()
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


    }

    //-------- Scenario 501
    namespace Scenario501
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is GreaterThanOrEqual
                public static int Test16()
                {
                    ConstantExpression left = Expression.Constant((byte)1, typeof(byte));
                    ConstantExpression right = Expression.Constant((byte)8, typeof(byte));

                    ExpressionType exp_et = ExpressionType.GreaterThanOrEqual;
                    Type exp_type = typeof(bool);
                    string exp_str = "(" + left.ToString() + " " + ">=" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary16__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary16__()
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


    }

    //-------- Scenario 502
    namespace Scenario502
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is Equal
                public static int Test17()
                {
                    ConstantExpression left = Expression.Constant(1, typeof(int));
                    ConstantExpression right = Expression.Constant(100, typeof(int));

                    ExpressionType exp_et = ExpressionType.Equal;
                    Type exp_type = typeof(bool);
#if SILVERLIGHT3
                    string exp_str = "(" + left.ToString() + " " + "=" + " " + right.ToString() + ")";
#else
                    string exp_str = "(" + left.ToString() + " " + "==" + " " + right.ToString() + ")";
#endif

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.Equal, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary17__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary17__()
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


    }

    //-------- Scenario 503
    namespace Scenario503
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is NotEqual
                public static int Test18()
                {
                    ConstantExpression left = Expression.Constant(true, typeof(bool));
                    ConstantExpression right = Expression.Constant(false, typeof(bool));

                    ExpressionType exp_et = ExpressionType.NotEqual;
                    Type exp_type = typeof(bool);
                    string exp_str = "(" + left.ToString() + " " + "!=" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.NotEqual, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary18__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary18__()
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


    }

    //-------- Scenario 504
    namespace Scenario504
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is ExclusiveOr
                public static int Test19()
                {
                    ConstantExpression left = Expression.Constant(false, typeof(bool?));
                    ConstantExpression right = Expression.Constant(true, typeof(bool?));

                    ExpressionType exp_et = ExpressionType.ExclusiveOr;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "^" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.ExclusiveOr, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary19__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary19__()
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


    }

    //-------- Scenario 505
    namespace Scenario505
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is Coalesce
                public static int Test20()
                {
                    ConstantExpression left = Expression.Constant(null, typeof(int?));
                    ConstantExpression right = Expression.Constant(9, typeof(int));

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = right.Type;
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.Coalesce, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary20__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary20__()
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


    }

    //-------- Scenario 506
    namespace Scenario506
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is ArrayIndex
                public static int Test21()
                {
                    int[] elements = { 9, 45, -4, 0, 10, 5 };
                    ConstantExpression array = Expression.Constant(elements, elements.GetType());
                    ConstantExpression index = Expression.Constant(5, typeof(int));

                    ExpressionType exp_et = ExpressionType.ArrayIndex;
                    Type exp_type = array.Type.GetElementType();
                    string exp_str = array.ToString() + "[" + index.ToString() + "]";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.ArrayIndex, array, index);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, array, index, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary21__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary21__()
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


    }

    //-------- Scenario 507
    namespace Scenario507
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is RightShift
                public static int Test22()
                {
                    ConstantExpression left = Expression.Constant(9999L, typeof(long?));
                    ConstantExpression right = Expression.Constant((int)45, typeof(int));

                    ExpressionType exp_et = ExpressionType.RightShift;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + ">>" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.RightShift, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, true, true);


                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary22__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary22__()
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


    }

    //-------- Scenario 508
    namespace Scenario508
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is LeftShift
                public static int Test23()
                {
                    ConstantExpression left = Expression.Constant((short)9, typeof(short));
                    ConstantExpression right = Expression.Constant(45, typeof(int));

                    ExpressionType exp_et = ExpressionType.LeftShift;
                    Type exp_type = left.Type;
                    string exp_str = "(" + left.ToString() + " " + "<<" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.LeftShift, left, right);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary23__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary23__()
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


    }

    //-------- Scenario 509
    namespace Scenario509
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Expression Type is NOT a Binary Expression
                public static int Test24()
                {
                    ConstantExpression left = Expression.Constant((short)9, typeof(short));
                    ConstantExpression right = Expression.Constant(45, typeof(int));

                    try
                    {
                        BinaryExpression result = Expression.MakeBinary(ExpressionType.Negate, left, right);
                        return 1;
                    }
                    catch (ArgumentException)
                    {
                        return 0;
                    }
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary24__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary24__()
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


    }

    //-------- Scenario 510
    namespace Scenario510
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Tests around the following MakeBinary Overload
                // public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method)
                public static int Test25a()
                {
                    ConstantExpression left = Expression.Constant(new TS1?(), typeof(TS1?));
                    ConstantExpression right = Expression.Constant(new TS1?(), typeof(TS1?));

                    MethodInfo method = typeof(TS1).GetMethod("Meth1");
                    ExpressionType exp_et = ExpressionType.LessThan;
                    Type exp_type = typeof(bool?);
                    string exp_str = "(" + left.ToString() + " " + "<" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.LessThan, left, right, true, method);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, method, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary25a__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary25a__()
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
                    return Test25a();
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



        namespace MakeBinary
        {
            // Helper struct for testing
            struct TS1
            {
                public static bool Meth1(TS1 t1, TS1 t2)
                {
                    return true;
                }
            }
        }

    }

    //-------- Scenario 511
    namespace Scenario511
    {

        namespace MakeBinary
        {
            public class Test
            {
                public static int Test25b()
                {
                    ConstantExpression left = Expression.Constant(new TS1?(), typeof(TS1?));
                    ConstantExpression right = Expression.Constant(new TS1?(), typeof(TS1?));

                    MethodInfo method = typeof(TS1).GetMethod("Meth1");
                    ExpressionType exp_et = ExpressionType.LessThanOrEqual;
                    Type exp_type = typeof(bool?);
                    string exp_str = "(" + left.ToString() + " " + "<=" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.LessThanOrEqual, left, right, true, method);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, method, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary25b__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary25b__()
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
                    return Test25b();
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



        namespace MakeBinary
        {
            // Helper struct for testing
            struct TS1
            {
                public static bool Meth1(TS1 t1, TS1 t2)
                {
                    return true;
                }
            }
        }

    }

    //-------- Scenario 512
    namespace Scenario512
    {

        namespace MakeBinary
        {
            public class Test
            {
                public static int Test25c()
                {
                    ConstantExpression left = Expression.Constant(new TS1?(), typeof(TS1?));
                    ConstantExpression right = Expression.Constant(new TS1?(), typeof(TS1?));

                    MethodInfo method = typeof(TS1).GetMethod("Meth1");
                    ExpressionType exp_et = ExpressionType.GreaterThan;
                    Type exp_type = typeof(bool?);
                    string exp_str = "(" + left.ToString() + " " + ">" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.GreaterThan, left, right, true, method);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, method, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary25c__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary25c__()
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
                    return Test25c();
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



        namespace MakeBinary
        {
            // Helper struct for testing
            struct TS1
            {
                public static bool Meth1(TS1 t1, TS1 t2)
                {
                    return true;
                }
            }
        }

    }

    //-------- Scenario 513
    namespace Scenario513
    {

        namespace MakeBinary
        {
            public class Test
            {
                public static int Test25d()
                {
                    ConstantExpression left = Expression.Constant(new TS1?(), typeof(TS1?));
                    ConstantExpression right = Expression.Constant(new TS1?(), typeof(TS1?));

                    MethodInfo method = typeof(TS1).GetMethod("Meth1");
                    ExpressionType exp_et = ExpressionType.GreaterThanOrEqual;
                    Type exp_type = typeof(bool?);
                    string exp_str = "(" + left.ToString() + " " + ">=" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, left, right, true, method);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, method, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary25d__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary25d__()
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
                    return Test25d();
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



        namespace MakeBinary
        {
            // Helper struct for testing
            struct TS1
            {
                public static bool Meth1(TS1 t1, TS1 t2)
                {
                    return true;
                }
            }
        }

    }

    //-------- Scenario 514
    namespace Scenario514
    {

        namespace MakeBinary
        {
            public class Test
            {
                public static int Test25e()
                {
                    ConstantExpression left = Expression.Constant(new TS1?(), typeof(TS1?));
                    ConstantExpression right = Expression.Constant(new TS1?(), typeof(TS1?));

                    MethodInfo method = typeof(TS1).GetMethod("Meth1");
                    ExpressionType exp_et = ExpressionType.Equal;
                    Type exp_type = typeof(bool?);
#if SILVERLIGHT3
                    string exp_str = "(" + left.ToString() + " " + "=" + " " + right.ToString() + ")";
#else
                    string exp_str = "(" + left.ToString() + " " + "==" + " " + right.ToString() + ")";
#endif

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.Equal, left, right, true, method);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, method, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary25e__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary25e__()
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
                    return Test25e();
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



        namespace MakeBinary
        {
            // Helper struct for testing
            struct TS1
            {
                public static bool Meth1(TS1 t1, TS1 t2)
                {
                    return true;
                }
            }
        }

    }

    //-------- Scenario 515
    namespace Scenario515
    {

        namespace MakeBinary
        {
            public class Test
            {
                public static int Test25f()
                {
                    ConstantExpression left = Expression.Constant(new TS1?(), typeof(TS1?));
                    ConstantExpression right = Expression.Constant(new TS1?(), typeof(TS1?));

                    MethodInfo method = typeof(TS1).GetMethod("Meth1");
                    ExpressionType exp_et = ExpressionType.NotEqual;
                    Type exp_type = typeof(bool?);
                    string exp_str = "(" + left.ToString() + " " + "!=" + " " + right.ToString() + ")";

                    BinaryExpression result = Expression.MakeBinary(ExpressionType.NotEqual, left, right, true, method);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, method, true, true);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary25f__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary25f__()
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
                    return Test25f();
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



        namespace MakeBinary
        {
            // Helper struct for testing
            struct TS1
            {
                public static bool Meth1(TS1 t1, TS1 t2)
                {
                    return true;
                }
            }
        }

    }

    //-------- Scenario 516
    namespace Scenario516
    {

        namespace MakeBinary
        {
            public class Test
            {
                // Tests around the following MakeBinary Overload
                // public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method, LambdaExpression conversion)

                // Only Coalesce operator uses the parameter "conversion"
                public static int Test26a()
                {
                    ConstantExpression left = Expression.Constant(40, typeof(int?));
                    ConstantExpression right = Expression.Constant(36L, typeof(long));

                    ParameterExpression param1 = Expression.Parameter(typeof(int), "intVarParm1");
                    ConstantExpression body = Expression.Constant(40L, typeof(long));
                    LambdaExpression conversion = Expression.Lambda(body, new ParameterExpression[] { param1 });
                    MethodInfo method = typeof(TS1).GetMethod("Meth1");

                    ExpressionType exp_et = ExpressionType.Coalesce;
                    Type exp_type = right.Type;
                    string exp_str = "(" + left.ToString() + " " + "??" + " " + right.ToString() + ")";


                    BinaryExpression result = Expression.MakeBinary(ExpressionType.Coalesce, left, right, true, method, conversion);

                    return Verification.VerifyBinaryParms(result, exp_et, exp_type, left, right, exp_str, null, conversion, false, false);
                }


                [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "MakeBinary26a__", new string[] { "positive", "cslinq", "FullTrustOnly", "Pri1" })]
                public static Expression MakeBinary26a__()
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
                    return Test26a();
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



        namespace MakeBinary
        {
            // Helper struct for testing
            struct TS1
            {
                public static bool Meth1(TS1 t1, TS1 t2)
                {
                    return true;
                }
            }
        }

    }

}
