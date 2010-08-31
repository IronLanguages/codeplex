/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace TestAst {
    using Ast = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using EU = ETUtils.ExpressionUtils;

    public partial class TestScenarios {
        private static Random rnd = null;
        private static int indent = 0;
        private const int TWEAKTHIS = 20;

        [TestAttribute(TestState.Disabled, "This test does random AST generation and is therefore non-deterministic.  For this reason we don't want to run it in daily dev runs.  Further, it is not completely finished.")]
        private static Expression Scenario_Random() {
            List<Expression> expressions = new List<Expression>();

            //Set up the seed for random generation.  If the AST_SEED
            //environment variable is set, use that value.  Otherwise
            //generate a new seed from the system clock.
            int seed;
            string env_seed = Environment.GetEnvironmentVariable("AST_SEED");
            if (env_seed == null) {
                seed = (int)(DateTime.Now.Ticks % Int32.MaxValue);
            } else {
                seed = Convert.ToInt32(env_seed);
            }
            Console.WriteLine("Randomly generating an AST from seed '{0}'", seed);

            rnd = new Random(seed);


            try {
                for (int i = 0; i < TWEAKTHIS; i++)
                    while (true) {
                        try {
                            expressions.Add(GetExpression());
                            break;
                        } catch (NotSupportedException e) {
                            Console.WriteLine("Failed to generate with chosen method, error was: {0}", e.ToString());
                        } catch (ArgumentException e) {
                            Console.WriteLine("Failed to generate with chosen method, error was: {0}", e.ToString());
                        } catch (TargetInvocationException e) {
                            if (e.InnerException is ArgumentNullException
                                || e.InnerException is ArgumentException
                                || e.InnerException is ArgumentOutOfRangeException
                                || e.InnerException is NotSupportedException)
                                Console.WriteLine("Failed to generate with chosen method, error was: {0}", e.InnerException.ToString());
                            else
                                throw;
                        }
                    }
            } finally {
                Console.WriteLine("Randomly generated an AST from seed '{0}'", seed);
            }

            Console.WriteLine("Finished generating a random AST, now going to execute it...");
            return EU.BlockVoid(expressions);
        }

        private static Expression GetExpression() {
            Debug.Assert(rnd != null);

            //Pick one statement factory method at random
            MethodInfo m = ExpressionMethods[rnd.Next(ExpressionMethods.Count)];
            for (int i = 0; i < indent; i++) Console.Write("  ");
            Console.WriteLine("{0}(", m.Name); //@TODO - Remove or make this more informative (what context is this in?  what parameters?)
            Expression retVal = (Expression)m.Invoke(null, GetParameters(m));
            for (int i = 0; i < indent; i++) Console.Write("  ");
            Console.WriteLine(")");
            return retVal;
        }

        private static object[] GetParameters(MethodInfo m) {
            //@TODO - Support: Variable, MethodInfo, ConstructorInfo, List<SwitchCase>, CatchBlock[], IfStatementTest[], Operators
            ParameterInfo[] pi = m.GetParameters();
            indent++;
            try {
                object[] args = new object[pi.Length];
                for (int i = 0; i < pi.Length; i++) {
                    Type pType = pi[i].ParameterType;
                    if (pType == typeof(SourceSpan))
                        args[i] = SourceSpan.None; //Using TestSpan here wouldn't really be all that useful
                    else if (pType == typeof(SourceLocation))
                        args[i] = SourceLocation.None;
                    else if (pType.IsSubclassOf(typeof(Expression)) || pType == typeof(Expression))
                        args[i] = GetExpression();
                    else if (pType.IsSubclassOf(typeof(Expression)) || pType == typeof(Expression))
                        args[i] = GetExpression();
                    else if (pType.IsArray && pType.GetElementType().IsSubclassOf(typeof(Expression)) || pType.GetElementType() == typeof(Expression)) {
                        List<Expression> subargs = new List<Expression>();
                        int count = rnd.Next(TWEAKTHIS);
                        for (int j = 0; j < count; j++) {
                            subargs.Add(GetExpression());
                        }
                        args[i] = subargs.ToArray();
                    } else if (pType == typeof(List<Expression>) || pType == typeof(IEnumerable<Expression>) || pType == typeof(IList<Expression>)) {
                        List<Expression> subargs = new List<Expression>();
                        int count = rnd.Next(TWEAKTHIS);
                        for (int j = 0; j < count; j++) {
                            subargs.Add(GetExpression());
                        }
                        args[i] = subargs;
                    } else if (pType == typeof(List<Expression>) || pType == typeof(IEnumerable<Expression>) || pType == typeof(IList<Expression>)) {
                        List<Expression> subargs = new List<Expression>();
                        int count = rnd.Next(TWEAKTHIS);
                        for (int j = 0; j < count; j++) {
                            subargs.Add(GetExpression());
                        }
                        args[i] = subargs;
                    } else if (pType == typeof(String)) {
                        //String is one of the tricky ones, to generate anything remotely
                        //valid we have to special case depending on the context in which
                        //the string value is needed.
                        //@TODO - More special cases are needed
                        if (m.Name == "Field" || m.Name == "AssignField")
                            args[i] = "PublicStaticField";
                        else if (m.Name == "Property" || m.Name == "AssignProperty")
                            args[i] = "PublicStaticProperty";
                        else
                            args[i] = "Hello World!";
                    } else if (pType == typeof(Int32))
                        args[i] = rnd.Next(); //@TODO - Doesn't work in many instances like indexexpressions where the size is constrained by context
                    else if (pType == typeof(MethodInfo)) {
                        args[i] = typeof(GeneratorTarget).GetMethod("PublicStaticMethodVoidVoid"); //@TODO - This probably won't work in a lot of scenarios
                    } else if (pType == typeof(Type))
                        args[i] = typeof(GeneratorTarget); //@TODO - This probably fails a lot too
                    else if (pType == typeof(Object))
                        args[i] = new GeneratorTarget(); //@TODO - again, not the best choice necessarily
                    else if (pType == typeof(FieldInfo))
                        args[i] = typeof(GeneratorTarget).GetField("PublicStaticField");
                    else if (pType == typeof(PropertyInfo))
                        args[i] = typeof(GeneratorTarget).GetProperty("PublicStaticProperty");
                    else if (pType == typeof(IfStatementTest[])) {
                        List<IfStatementTest> tests = new List<IfStatementTest>();
                        int count = rnd.Next(TWEAKTHIS);
                        for (int j = 0; j < count; j++)
                            tests.Add(AstUtils.IfCondition(GetExpression(), GetExpression()));
                        args[i] = tests.ToArray();
                    } else if (pType == typeof(CatchBlock[])) {
                        List<CatchBlock> cbs = new List<CatchBlock>();
                        int count = rnd.Next(TWEAKTHIS);
                        for (int j = 0; j < count; j++)
                            cbs.Add(Ast.Catch(typeof(GeneratorTarget), GetExpression())); //@TODO - Not the best type choice
                        args[i] = cbs.ToArray();
                    } else if (pType == typeof(ParameterExpression)) {
                        args[i] = TestScope.Current.GetOrMakeLocal("scenario_random_temp_TODO", typeof(Int32)); //@TODO - Likely won't work in all cases.  Actually, I'm not sure what happens if I try to assign, say, a string to this variable
                    } else
                        throw new NotSupportedException(String.Format("Unsupported parameter type '{0}'", pType));
                }
                return args;
            } finally {
                indent--;
            }
        }

        private static List<MethodInfo> _expressionMethods = null;
        private static List<MethodInfo> ExpressionMethods {
            get {
                if (_expressionMethods == null) {
                    //Get all methods and then filter only those that return a type of Expression
                    List<MethodInfo> allmethods = new List<MethodInfo>(typeof(Ast).GetMethods());
                    _expressionMethods = new List<MethodInfo>();
                    foreach (MethodInfo m in allmethods) {
                        if (m.ReturnParameter.ParameterType.IsSubclassOf(typeof(Expression)) ||
                            m.ReturnParameter.ParameterType == typeof(Expression)) {

                            if (m.ReturnParameter.ParameterType != typeof(UnaryExpression)
                                && m.ReturnParameter.ParameterType != typeof(GotoExpression)) {
                                _expressionMethods.Add(m);
                            }
                        }
                    }
                }
                return _expressionMethods;
            }
        }

        public class GeneratorTarget {
            public static int PublicStaticField;
            public static int PublicStaticProperty {
                get {
                    return PublicStaticField;
                }
                set {
                    PublicStaticField = value;
                }
            }
            public static void PublicStaticMethodVoidVoid() {
                Console.WriteLine("GeneratorTarget.PublicStaticMethodVoidVoid()");
            }
        }
    }
}
