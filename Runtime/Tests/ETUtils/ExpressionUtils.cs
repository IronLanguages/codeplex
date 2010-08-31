#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using System.Security.Permissions;

#if !SILVERLIGHT
[assembly:System.Security.AllowPartiallyTrustedCallers]
#endif

namespace ETUtils {
    using Ast = Expression;

    public class ExpressionUtils {
#if !SILVERLIGHT3
        public static Expression GenAreEqual(Expression x, Expression y) {
            return GenAreEqual(x, y, String.Empty);
        }

        /// <summary>
        /// Generates AST nodes that assert two Expressions are equal
        /// </summary>
        public static Expression GenAreEqual(Expression Expected, Expression Actual, string note) {
            //if expr==y:
            //  pass
            //else:
            //  print "values are not equal:"
            //  print expr
            //  print y
            //  raise Exception("values are not equal")

            //Evaluate expressions only once.
            ParameterExpression TestLeft = Expression.Parameter(Expected.Type, "Expected");
            ParameterExpression TestRight = Expression.Parameter(Actual.Type, "Actual");

            var test =
                Expression.Invoke(
                    Expression.Lambda(
                        Expression.IfThenElse(
                            Expression.Equal(TestLeft, TestRight),
                            Expression.Empty(),
                            Expression.Block(
                                GenPrint(""),
                                GenPrint("Values are not equal: " + note),
                                GenPrintWithType(TestLeft),
                                GenPrintWithType(TestRight),
                                GenThrow("values are not equal", 1)
                        )
                    ),
                    // GenAreEqualRewriter uses this variable as a marker
                    new ParameterExpression[] { Expression.Variable(typeof(string), "GenAreEqualVar"), TestLeft, TestRight }
                ),
                new Expression[] { Expression.Constant("GenAreEqual"), Expected, Actual }
            );

            return test;
        }
#endif

        public static Expression GenPrint(string text) {
            return Ast.Call(
                typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                Ast.Constant(text)
            );
        }
        public static Expression GenPrint(Expression exp) {
            return Ast.Call(
                typeof(Console).GetMethod("WriteLine", new Type[] { typeof(object) }),
                Ast.Convert(exp, typeof(object))
            );
        }
#if !SILVERLIGHT3
        public static Expression GenPrintWithType(Expression exp) {
            return Ast.Block(
                Ast.Call(
                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(object) }),
                    Ast.Convert(exp, typeof(object))
                ),
                Ast.Call(
                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                    Ast.Constant("Type: " + exp.Type.Name)
                ),
                Ast.Call(
                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                    Ast.Constant("Expression.ToString(): " + exp.ToString() +"\n")
                ),
                Ast.Empty()
            );
        }
        public static Expression GenThrow(string reason, int skipframes) {
            return TestSpan.GetDebugInfoForFrame(
                Expression.Throw(
                    Ast.New(
                        typeof(Exception).GetConstructor(new Type[] { typeof(string) }),
                        Ast.Constant(reason)
                    )
                ),
                1 + skipframes
            );
        }

        public static Expression GenThrow(string reason) {
            return GenThrow(reason, 1);
        }

        public static Expression GenThrow<T>(string reason) where T : Exception {
            return TestSpan.GetDebugInfoForFrame(
                Expression.Throw(
                    Ast.New(
                        typeof(T).GetConstructor(new Type[] { typeof(string) }),
                        Ast.Constant(reason)
                    )
                ),
                1
            );
        }
#endif

        public static Expression GenConcat(string str1, string str2) {
            System.Reflection.MethodInfo Method = typeof(String).GetMethod("Concat", new Type[] { typeof(String), typeof(String) });
            return Ast.Call(Method, Ast.Constant(str1), Ast.Constant(str2));
        }

        public static Expression GenConcat(Expression var, string str) {
            System.Reflection.MethodInfo Method = typeof(String).GetMethod("Concat", new Type[] { typeof(String), typeof(String) });
            return Ast.Call(Method, var, Ast.Constant(str));
        }

#if !SILVERLIGHT3
        public static Expression ConcatEquals(Expression var, string str) {
            System.Reflection.MethodInfo Method = typeof(String).GetMethod("Concat", new Type[] { typeof(String), typeof(String) });
            return Ast.Assign(var, Ast.Call(Method, var, Ast.Constant(str)));
        }

        public static Expression ConcatEquals(Expression var, Expression str) {
            System.Reflection.MethodInfo Method = typeof(String).GetMethod("Concat", new Type[] { typeof(String), typeof(String) });
            return Ast.Assign(var, Ast.Call(Method, var, str));
        }
#endif

        public static Expression GetExprType(Expression expression) {
            MethodInfo mi = typeof(object).GetMethod("GetType", new Type[] { });
            return Expression.Call(expression, mi);
        }

#if !SILVERLIGHT3
        /// <summary>
        /// Checks whether an Expression's type is equal to the given type
        /// </summary>
        /// <param name="expr">The Expression whose type is being checked</param>
        /// <param name="t">Type to check expr against</param>
        /// <returns>Returns an EU.GenAreEqual expression to be evaluated in a LambdaExpression</returns>
        public static Expression ExprTypeCheck(Expression x, Type t) {
            return ExprTypeCheck(x, t, false);
        }

        /// <summary>
        /// Checks whether an Expression's type is equal to the given type
        /// </summary>
        /// <param name="expr">The Expression whose type is being checked</param>
        /// <param name="t">Type to check expr against</param>
        /// <param name="checkBaseTypes">True will check if any of expr's base types match t. False will only check expr.Type against t.</param>
        /// <returns>Returns an EU.GenAreEqual expression to be evaluated in a LambdaExpression</returns>
        public static Expression ExprTypeCheck(Expression expr, Type t, bool checkBaseTypes) {
            if (!checkBaseTypes)
                return GenAreEqual(Expression.Constant(expr.Type, typeof(Type)), Expression.Constant(t, typeof(Type)), "ExprValueTypeCheck");
            else {
                bool result = false;
                Type tmp = expr.Type;
                while (tmp != null) { // move up through the type hierarchy
                    if (tmp.Equals(t)) {
                        result = true;
                        break;
                    }
                    tmp = tmp.BaseType;
                }
                if (result == false) // just so a failure in GenAreEqual is properly descriptive
                    tmp = expr.Type;
                return GenAreEqual(Expression.Constant(tmp, typeof(Type)), Expression.Constant(t, typeof(Type)), "ExprValueTypeCheck on base types");
            }
        }

        public static BlockExpression BlockVoid(IEnumerable<ParameterExpression> vars, params Expression[] expressions) {
            return Expression.Block(vars, MakeVoid(expressions));
        }

        public static BlockExpression BlockVoid(params Expression[] expressions) {
            return Expression.Block(MakeVoid(expressions));
        }

        public static BlockExpression BlockVoid(IEnumerable<ParameterExpression> vars, IEnumerable<Expression> expressions) {
            return Expression.Block(vars, MakeVoid(expressions));
        }

        public static BlockExpression BlockVoid(IEnumerable<Expression> expressions) {
            return Expression.Block(MakeVoid(expressions));
        }

        private static List<Expression> MakeVoid(IEnumerable<Expression> expressions) {
            var exprList = new List<Expression>(expressions);
            if (exprList.Count == 0 || exprList[exprList.Count - 1].Type != typeof(void)) {
                exprList.Add(Expression.Empty());
            }
            return exprList;
        }

        private static Expression[] MakeVoid(Expression[] expressions) {
            if (expressions.Length == 0 || expressions[expressions.Length - 1].Type != typeof(void)) {
                var newExprs = new Expression[expressions.Length + 1];
                expressions.CopyTo(newExprs, 0);
                newExprs[expressions.Length] = Expression.Empty();
                expressions = newExprs;
            }
            return expressions;
        }
#endif

        public static ParameterExpression Param<T>(String Name) {
            return Expression.Parameter(typeof(T), Name);
        }

        public static ParameterExpression Param<T>() {
            return Expression.Parameter(typeof(T), "");
        }

#if !SILVERLIGHT3
        public static ParameterExpression ParamInit<T>(String Name, T Value, List<Expression> Expressions) {
            ParameterExpression Param = Expression.Parameter(typeof(T), Name);
            Expressions.Add(Expression.Assign(Param, Expression.Constant(Value, typeof(T))));

            return Param;
        }

        /// <summary> 
        /// Helper method for verifying results of generators. Moves through the generated list and performs equality checks against expected values from yield
        /// </summary>
        /// <param name="Expressions">List to add equality comparisons to</param>
        /// <param name="expectedValues">List of values expected to have been generated by yield</param>
        /// <param name="numTimes">Number of values to check for equality</param>
        /// <param name="enumerator">An IEnumerator inside an Expression.Constant</param>
        /// <param name="Result">typeof(bool) to check result of MoveNext calls on enumerator</param>
        /// <param name="Value">typeof(object) to check result of Current calls on enumerator</param>
        public static void Enumerate<T>(ref List<Expression> Expressions, IList<T> expectedValues, int numTimes, Expression enumerator, ref ParameterExpression Result, ref ParameterExpression Value) {
            MethodInfo MoveNext = typeof(IEnumerator).GetMethod("MoveNext");
            PropertyInfo Current = typeof(IEnumerator).GetProperty("Current");

            try {
                int i = 0;
                for (; i < numTimes; i++) {
                    Expressions.Add(Expression.Assign(Result, Expression.Call(enumerator, MoveNext)));
                    Expressions.Add(GenAreEqual(Result, Expression.Constant(true), "Yield " + i));
                    Expressions.Add(Expression.Assign(Value, Expression.Property(enumerator, Current)));
                    Expressions.Add(GenAreEqual(Expression.Unbox(Value, expectedValues[0].GetType()), Expression.Constant(expectedValues[i]), "Yield " + i));
                }

                // this should move past the last element of the list
                // technically the value of Current is undefined once MoveNext returns false but our implementation has it return the last valid element
                Expressions.Add(Expression.Assign(Result, Expression.Call(enumerator, MoveNext)));
                Expressions.Add(GenAreEqual(Result, Expression.Constant(false), "Yield last"));
                Expressions.Add(Expression.Assign(Value, Expression.Property(enumerator, Current)));
                Expressions.Add(GenAreEqual(Expression.Unbox(Value, expectedValues[0].GetType()), Expression.Constant(expectedValues[numTimes - 1]), "Yield last"));

                // make sure we can keep calling MoveNext and Current with correct results
                Expressions.Add(Expression.Assign(Result, Expression.Call(enumerator, MoveNext)));
                Expressions.Add(GenAreEqual(Result, Expression.Constant(false), "Yield past last"));
                Expressions.Add(Expression.Assign(Value, Expression.Property(enumerator, Current)));
                Expressions.Add(GenAreEqual(Expression.Unbox(Value, expectedValues[0].GetType()), Expression.Constant(expectedValues[numTimes - 1]), "Yield past last"));
            } catch (ArgumentException e) {
                throw new ArgumentException("ArgumentException inside helper method Enumerate", e);
            }
        }

        /// <summary>
        /// Support for non-local goto-with-value. Needs an explicit temp.
        /// </summary>
        public static Expression Goto(ParameterExpression temp, LabelTarget label, Expression value) {
            return Expression.Goto(label, Expression.Assign(temp, value));
        }

        /// <summary>
        /// Support for non-local goto-with-value. Needs an explicit temp.
        /// </summary>
        public static Expression Label(ParameterExpression temp, LabelTarget label, Expression value) {
            return Expression.Block(
                Expression.Label(label, Expression.Assign(temp, value)),
                temp
            );
        }

        public static Expression[] GetAllExpressions(){
            List<Expression> Ret = new List<Expression>();

            Ret.Add(Expression.Constant(0));

            Ret.Add(Expression.Add(Expression.Constant(0), Expression.Constant(1)));

            Ret.Add(Expression.Block(Expression.Empty()));

            Ret.Add(Expression.Lambda(Expression.Constant(1)));

            var sd = Expression.SymbolDocument("x");
            Ret.Add(AddDebugInfo(Expression.Constant(1),sd, 1,1,1,1));

            return Ret.ToArray();
        }

        public static SwitchCase SwitchCase(int value, Expression body) {
            return Expression.SwitchCase(AstUtils.Void(body), Expression.Constant(value));
        }

        public static Expression AddDebugInfo(Expression expression, SymbolDocumentInfo document, int startLine, int startColumn, int endLine, int endColumn){
            return AstUtils.AddDebugInfo(expression, document, startLine, startColumn, endLine, endColumn);
        }
#endif

        [Obsolete("tests should use Expression.Convert directly.")]
#if TRUESL3
        public static Microsoft.Scripting.Ast.Expression Convert(Microsoft.Scripting.Ast.Expression expression, Type type) {
#else
        public static Expression Convert(Expression expression, Type type) {
#endif
            if (type == typeof(void)) {
                return AstUtils.Void(expression);
            }
#if TRUESL3
            return Microsoft.Scripting.Ast.Expression.Convert(expression, type);
#else
            return Expression.Convert(expression, type);
#endif
        }


#if !SILVERLIGHT3
        public static T CompileAsMethodAndRun<T>(Expression<T> lambda, params object[] args) {
            var AssemblyBuilder = System.AppDomain.CurrentDomain.DefineDynamicAssembly(new System.Reflection.AssemblyName("MyTestAssembly"), System.Reflection.Emit.AssemblyBuilderAccess.Run);
            var ModuleBuilder = AssemblyBuilder.DefineDynamicModule("MyTestAssembly");
            var ClassBuilder = ModuleBuilder.DefineType("Class1", System.Reflection.TypeAttributes.Class | System.Reflection.TypeAttributes.Public, typeof(object));
            var MethodBuilder = ClassBuilder.DefineMethod("Test1", System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static);


            var mi = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) });
            lambda.CompileToMethod(MethodBuilder,System.Runtime.CompilerServices.DebugInfoGenerator.CreatePdbGenerator());

            ClassBuilder.CreateType();

            return (T) AssemblyBuilder.GetType("Class1").GetMethod("Test1", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(null, args);
        }

        public static void CompileAsMethodAndRun(LambdaExpression lambda, params object[] args) {
            var AssemblyBuilder = System.AppDomain.CurrentDomain.DefineDynamicAssembly(new System.Reflection.AssemblyName("MyTestAssembly"), System.Reflection.Emit.AssemblyBuilderAccess.Run);
            var ModuleBuilder = AssemblyBuilder.DefineDynamicModule("MyTestAssembly");
            var ClassBuilder = ModuleBuilder.DefineType("Class1", System.Reflection.TypeAttributes.Class | System.Reflection.TypeAttributes.Public, typeof(object));
            var MethodBuilder = ClassBuilder.DefineMethod("Test1", System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static);


            var mi = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) });
            lambda.CompileToMethod(MethodBuilder, System.Runtime.CompilerServices.DebugInfoGenerator.CreatePdbGenerator());

            ClassBuilder.CreateType();
            
            AssemblyBuilder.GetType("Class1").GetMethod("Test1", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(null, args);
        }
#endif

        public static void Equal<T>(Expression expression, IEquatable<T> expected) {
            Func<T> scenario = Expression.Lambda<Func<T>>(expression).Compile();

            T result = scenario();

            if (!expected.Equals(result)) {
                throw new InvalidOperationException("Test failed");
            }
        }

        // Remove risk of exception handlers or attributes elsewhere swallowing legitimate failures because of matching Exception types
        public class TestFailedException : Exception {
            public TestFailedException() : base() { }
            public TestFailedException(string message) : base(message) { }
            public TestFailedException(string message, Exception e) : base(message, e) { }
        }

        public static void Equal(object a, object b) {
            if (!object.Equals(a, b)) {
                throw new InvalidOperationException(string.Format("Expected '{0}' to equal '{1}'", a ?? "<null>", b ?? "<null>"));
            }
        }

        public static void ReferenceEqual(object a, object b) {
            if (a != b) {
                throw new InvalidOperationException(string.Format("Test failed: expected '{0}' to be reference equal to '{1}'", a ?? "<null>", b ?? "<null>"));
            }
        }

        public static void ReferenceNotEqual(object a, object b) {
            if (a == b) {
                throw new InvalidOperationException(string.Format("Test failed: expected '{0}' to be reference not equal to '{1}'", a ?? "<null>", b ?? "<null>"));
            }
        }

        public static void Assert(bool condition) {
            if (!condition) {
                throw new InvalidOperationException("Test failed");
            }
        }

        // Return an Expression because some ETScenarios expect it
        public static Expression Throws<T>(Action test) where T : Exception {
            try {
                test();
                throw new TestFailedException(string.Format("Expected exception '{0}'", typeof(T).Name));
            } catch (T) {
                return Expression.Constant(0);
            } catch (Exception e) {
                if (!(e is TestFailedException)) {
                    throw new TestFailedException(string.Format("Expected exception '{0}' but got:\n{1}", typeof(T).Name, e.ToString()), e);
                } else {
                    throw;
                }
            }
        }

        public static void ArrayEqual<T>(T[] actual, params T[] expected) {
            Equal(actual.Length, expected.Length);
            for (int i = 0; i < actual.Length; i++) {
                var x = actual[i] as T[];
                var y = expected[i] as T[];
                if (x != null && y != null) {
                    ArrayEqual(x, y);
                } else {
                    Equal(actual[i], expected[i]);
                }
            }
        }

        #region Test Rewriters
#if !SILVERLIGHT3
        public class ReduceRewriter : ExpressionVisitor {
            public List<string> Visited = new List<string>();

            public override Expression Visit(Expression node) {
                if (node == null)
                    return null;
                while (node.CanReduce) {
                    Visited.Add(node.NodeType.ToString());
                    var temp = node.ReduceAndCheck();
                    if (temp.GetType() == node.GetType()) { // might not be able to assign node = node.Reduce()
                        node = temp;
                    } else {
                        return temp;
                    }
                }
                return base.Visit(node);
            }
        }

        public class QuoteRewriter : TestRewriter {
            protected override Expression VisitLambda<T>(Expression<T> node) {
                return Expression.Lambda<T>(Expression.Quote(node), node.Parameters);
            }
        }


        // This rewriter effectively replaces all GenAreEqual calls in a test with Utils.Equals calls.
        // The result is a failing test with still be on the callstack when the failure is logged allowing for easier debugging.
        public class GenAreEqualRewriter : TestRewriter {
            ParameterExpression V = Expression.Variable(typeof(TestResults), "V");

            protected override Expression VisitLambda<T>(Expression<T> node) {
                if (node.Parameters.Count > 0 && node.Parameters[0].Name == "GenAreEqualVar") {
                    // Replace the tree created by GenAreEquals with a call to List.Add which adds the arguments to
                    // GenAreEquals to the list of Tuples that the test's TestResults object holds.
                    // This list will later be enumerated and checked with Utils.Equal rather than GenAreEqual doing validation during tree compilation/invocation.
                    var call =
                        Expression.Call(
                            V,
                            typeof(TestResults).GetMethod("Add"),
                            new Expression[] { 
                                Expression.New(
                                    typeof(Tuple<object, object>).GetConstructor(new Type[] { typeof(object), typeof(object), typeof(string) }), 
                                    new Expression[] { 
                                        Expression.Convert(node.Parameters[1], typeof(object)),
                                        Expression.Convert(node.Parameters[2], typeof(object)),
                                        node.Parameters[3]
                                    }
                                )
                            }
                        );

                    return Expression.Lambda(call, node.Parameters);
                }

                return base.VisitLambda<T>(node);
            }

            protected override Expression VisitBlock(BlockExpression node) {
                if (node.Variables.Count > 0 && node.Variables[0].Name == "TestResults") {
                    V = node.Variables[0]; // grab the tree's TestResults instance where actual/expected results will be stored.
                }
                return base.VisitBlock(node);
            }
        }


        /// <summary>
        /// Tuple for storing test results previously done via GenAreEquals. String parameter is for logging purposes.
        /// </summary>
        /// <typeparam name="Actual">The type of the Expression whose value is being checked for correctness.</typeparam>
        /// <typeparam name="Expected">The type of the value being compared against the given Expression.</typeparam>
        public class Tuple<Actual, Expected> {
            public Actual Item1 { get; set; }
            public Expected Item2 { get; set; }
            public string Item3 { get; set; }
            public Tuple(Actual a, Expected b, string c) {
                Item1 = a;
                Item2 = b;
                Item3 = c;
            }
        }

        // This class facilitates the use of the GenAreEqualRewriter.
        // Each test will create a tree with a single TestResults object whose Values field holds a Tuple for each GenAreEqual call the test makes.
        // Each Tuple simply holds the values that were passed to GenAreEqual so that they can be verified after tree execution.
        public class TestResults {
            public List<Tuple<object, object>> Values = new List<Tuple<object, object>>();

            public void Add(Tuple<object, object> val) {
                Values.Add(val);
            }
        }

        public class CompileAsMethodRewritter : TestRewriter {
            public bool Compilable = true;

            protected override Expression VisitConstant(ConstantExpression node) {
                // Can't save live objects, rewrite Exceptions in a nice way
                if ((node.Type == typeof(Exception) || node.Type.IsSubclassOf(typeof(Exception)))) {
                    if (node.Type.GetConstructor(new Type[] { typeof(string) }) != null) {

                        if (node.Value == null) {
                            return Expression.Convert(Expression.Constant(null), node.Type);
                        } else {
                            return Expression.New(
                                node.Type.GetConstructor(new Type[] { typeof(string) }),
                                Expression.Constant(((Exception)node.Value).Message)
                            );
                        }

                    } else {
                        return Expression.New(
                            node.Type.GetConstructor(new Type[] { })
                        );
                    }

                }

                if (node.Value != null) {
                    // Live reference types
                    if (node.Value.GetType().IsClass && (node.Value.GetType() != typeof(string))) {
                        Compilable = false;
                    }
                    // User defined structs
                    if (node.Type.BaseType != typeof(ValueType) && !node.Value.GetType().IsClass && node.Type != typeof(string)) {
                        //if (node.Value != null && node.Value.GetType().Assembly == typeof(ETScenarios.Conversions).Assembly) {
                        //note the node.Value.GetType to catch cases with nullable<userdefined struct>
                        Compilable = false;
                        return base.VisitConstant(node);
                    }

                    //boxed constants
                    if (node.Type != null && node.Type == typeof(object) && !node.Value.GetType().IsClass) {
                        return Expression.Convert(Expression.Constant(node.Value), node.Type);
                    }
                }

                //Guids
                if (node.Value is Guid) {
                    Compilable = false;
                    return base.VisitConstant(node);
                }

                //Generic types
                if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    //there had to be a couple of instrinsic common structures used :) - DateTime will not be supported, so we need
                    //to do something about it. In case other structures appear, we can add elseif's here and either created them
                    //or just mark compilable as false.
                    /*if(node.Value is DateTime){
                        return Expression.Convert(Expression.Call(null, typeof(DateTime).GetMethod("Parse", new Type[] { typeof(string) }), Expression.Constant(node.Value.ToString())), node.Type);
                    }else if(node.Value != null && node.Value.GetType() == typeof(Guid)){
                        Compilable = false;
                        return base.VisitConstant(node);
                    }else{
                        return Expression.Convert(Expression.Constant(node.Value), node.Type);
                    }*/

                    return Expression.Convert(VisitConstant(Expression.Constant(node.Value)), node.Type);
                }

                //DateTime
                if (node.Type == typeof(DateTime)) {
                    return Expression.Call(null, typeof(DateTime).GetMethod("Parse", new Type[] { typeof(string) }), Expression.Constant(node.Value.ToString()));
                }

                return base.VisitConstant(node);
            }
            protected override Expression VisitDynamic(DynamicExpression node) {
                //not allowed. 
                Compilable = false;
                return base.VisitDynamic(node);
            }

            protected override Expression VisitUnary(UnaryExpression node) {
                // Technically a Quote is allowed in CompileToMethod; however, any nontrivial Lambda (which is quoted) will be a live object (ex. () => 1)
                if (node.NodeType == ExpressionType.Quote) {
                    Compilable = false;
                }
                return base.VisitUnary(node);
            }
        }

        public class TestRewriter1 : StackSpillBase {

            protected override Expression VisitConstant(ConstantExpression node) {
                var c = Expression.Block(
                            Expression.TryCatch(
                                Expression.Empty(),
                                Expression.Catch(
                                    typeof(Exception),
                                    Expression.Empty()
                                )
                             ),
                             node
                        );

                return c;
            }

        }

        public class TestRewriter2 : StackSpillBase {

            protected override Expression VisitConstant(ConstantExpression node) {
                var c = Expression.Block(
                            Expression.TryCatch(
                                Expression.Empty(),
                                Expression.Catch(
                                    typeof(Exception),
                                    Expression.Empty()
                                )
                             ),
                             Expression.Call(Expression.Constant(1), typeof(int).GetMethod("GetType")),
                             Expression.Block(
                                Expression.TryCatch(
                                    Expression.Empty(),
                                    Expression.Catch(
                                        typeof(Exception),
                                        Expression.Empty()
                                    )
                                 ),
                                 node
                             )
                        );
                return c;
            }

        }

        public class StackSpillBase : TestRewriter {
            protected override Expression VisitBinary(BinaryExpression node) {

                if (node.NodeType == ExpressionType.Equal) {
                    return node;
                } else
                    return base.VisitBinary(node);
            }
        }

        public class LambdaInvoke : TestRewriter {
            protected override Expression VisitBlock(BlockExpression node) {
                //lambda that returns block's result

                //build parameters list
                ParameterExpression[] Params = (ParameterExpression[])Array.CreateInstance(typeof(ParameterExpression), node.Variables.Count);
                Expression[] Args = (Expression[])Array.CreateInstance(typeof(Expression), node.Variables.Count);
                for (int i = 0; i < node.Variables.Count; i++) {
                    Params[i] = node.Variables[i];
                    Args[i] = Expression.Default(Params[i].Type);
                }


                return Expression.Invoke(
                    Expression.Lambda(
                        Expression.Block(
                            node.Expressions
                        ),
                        Params
                    ),
                    Args
                );
            }
        }

        public static void TailCall() { TailCall1(); }
        public static void TailCall1() { }
        public static T TailCallRet<T>(T arg) { return TailCallRet1<T>(arg); }
        public static T TailCallRet1<T>(T arg) { return arg; }

        public class InjectCalls : TestRewriter {
            protected override Expression VisitBlock(BlockExpression node) {
                var Exprs = new System.Collections.Generic.List<Expression>();
                foreach (Expression x in node.Expressions) {
                    Exprs.Add(Expression.Call(null, typeof(AstUtils).GetMethod("TailCall")));
                    Exprs.Add(x);
                }
                if (node.Type == typeof(void)) {
                    Exprs.Add(Expression.Call(null, typeof(AstUtils).GetMethod("TailCall")));
                } else {
                    var last = Exprs[Exprs.Count - 1];
                    Exprs.RemoveAt(Exprs.Count - 1);
                    Exprs.Add(Expression.Call(typeof(AstUtils), "TailCallRet", new Type[] { last.Type }, last));
                }

                ParameterExpression[] Params = (ParameterExpression[])Array.CreateInstance(typeof(ParameterExpression), node.Variables.Count);
                Expression[] Args = (Expression[])Array.CreateInstance(typeof(Expression), node.Variables.Count);
                for (int i = 0; i < node.Variables.Count; i++) {
                    Params[i] = node.Variables[i];
                    Args[i] = Expression.Default(Params[i].Type);
                }


                return Expression.Invoke(
                    Expression.Lambda(
                        Expression.Block(
                            Exprs
                        ),
                        Params
                    ),
                    Args
                );
            }

            protected override Expression VisitConstant(ConstantExpression node) {
                return Expression.Call(typeof(AstUtils), "TailCallRet", new Type[] { node.Type }, node);
            }
        }

        public class TailCallOnRewritter : TestRewriter {
            protected override Expression VisitLambda<T>(Expression<T> node) {
                //Lambda with tailcall flag.
                return Expression.Lambda<T>(node.Body, node.Name, true, node.Parameters);
            }
        }

        public class TailCallOffRewritter : TestRewriter {
            protected override Expression VisitLambda<T>(Expression<T> node) {
                //Lambda with tailcall flag.
                return Expression.Lambda<T>(node.Body, node.Name, false, node.Parameters);
            }
        }

        public class RandomRewritter : TestRewriter {

            protected override Expression VisitBinary(BinaryExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitBlock(BlockExpression node) {
                return RandomExpression(node);
            }


            protected override CatchBlock VisitCatchBlock(CatchBlock node) {
                return RandomExpression(node);
            }


            protected override Expression VisitConditional(ConditionalExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitConstant(ConstantExpression node) {
                return RandomExpression(node);
            }

            protected override Expression VisitDebugInfo(DebugInfoExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitDefault(DefaultExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitDynamic(DynamicExpression node) {
                return RandomExpression(node);
            }


            protected override ElementInit VisitElementInit(ElementInit node) {
                return RandomExpression(node);
            }


            protected override Expression VisitExtension(Expression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitGoto(GotoExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitIndex(IndexExpression node) {
                return RandomExpression(node);
            }

            protected override Expression VisitInvocation(InvocationExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitLabel(LabelExpression node) {
                return RandomExpression(node);
            }


            protected override LabelTarget VisitLabelTarget(LabelTarget node) {
                return RandomExpression(node);
            }


            protected override Expression VisitLambda<T>(Expression<T> node) {
                return RandomExpression(node);
            }


            protected override Expression VisitListInit(ListInitExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitLoop(LoopExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitMember(MemberExpression node) {
                return RandomExpression(node);
            }


            protected override MemberAssignment VisitMemberAssignment(MemberAssignment node) {
                return RandomExpression(node);
            }


            protected override MemberBinding VisitMemberBinding(MemberBinding node) {
                return RandomExpression(node);
            }


            protected override Expression VisitMemberInit(MemberInitExpression node) {
                return RandomExpression(node);
            }


            protected override MemberListBinding VisitMemberListBinding(MemberListBinding node) {
                return RandomExpression(node);
            }


            protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node) {
                return RandomExpression(node);
            }


            protected override Expression VisitMethodCall(MethodCallExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitNew(NewExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitNewArray(NewArrayExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitParameter(ParameterExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitSwitch(SwitchExpression node) {
                return RandomExpression(node);
            }


            protected override SwitchCase VisitSwitchCase(SwitchCase node) {
                return RandomExpression(node);
            }


            protected override Expression VisitTry(TryExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitTypeBinary(TypeBinaryExpression node) {
                return RandomExpression(node);
            }


            protected override Expression VisitUnary(UnaryExpression node) {
                return RandomExpression(node);
            }

            static Random rnd = new Random();
            static Expression RandomExpression(Expression Original) {
                if (rnd.Next(0, 15) > 5) return Original;

                var Exprs = new System.Collections.Generic.List<Expression>();
                Exprs.Add(Expression.Constant(1));
                Exprs.Add(Expression.Add(Expression.Constant(1), Expression.Constant(2)));
                Exprs.Add(Expression.ArrayAccess(Expression.Constant(new int[] { 1, 2, 3 }), Expression.Constant(2)));
                Exprs.Add(Expression.ArrayIndex(Expression.Constant(new[] { 1, 2, 3 }), Expression.Constant(1)));

                return Exprs[rnd.Next(0, Exprs.Count)];
            }
            static CatchBlock RandomExpression(CatchBlock Original) {
                return Original;
            }
            static ElementInit RandomExpression(ElementInit Original) {
                return Original;
            }
            static LabelTarget RandomExpression(LabelTarget Original) {
                return Original;
            }
            static MemberAssignment RandomExpression(MemberAssignment Original) {
                return Original;
            }
            static MemberBinding RandomExpression(MemberBinding Original) {
                return Original;
            }
            static MemberListBinding RandomExpression(MemberListBinding Original) {
                return Original;
            }
            static MemberMemberBinding RandomExpression(MemberMemberBinding Original) {
                return Original;
            }
            static SwitchCase RandomExpression(SwitchCase Original) {
                return Original;
            }
        }

        /// <summary>
        /// This class should be the base class to any tree re-writers used in AstTest.
        /// It filters out some trees based on containing a parameter with a specific name 
        /// "dont_visit_node" for permanent exclusions
        /// "donotvisittemp" to disable a scenario while a bug is being fixed.
        /// </summary>
        public class TestRewriter : ExpressionVisitor {

            protected override Expression VisitBlock(BlockExpression node) {
                foreach (var v in node.Variables) {
                    if (v.Name != null && (
                        v.Name.ToLowerInvariant() == "dont_visit_node" /*||
                        v.Name.ToLowerInvariant() == "donotvisittemp"*/
                        )) return node;
                }
                return base.VisitBlock(node);
            }
        }

        internal static Expression Rewrite(Expression tree, String Rewriter) {
            if (Rewriter.ToLowerInvariant() == "testrewriter1") {
                var Comp = new TestRewriter1();
                return Comp.Visit(tree);
            }
            if (Rewriter.ToLowerInvariant() == "testrewriter2") {
                var Comp = new TestRewriter2();
                return Comp.Visit(tree);
            }

            if (Rewriter.ToLowerInvariant() == "lambdainvoke") {
                var Comp = new LambdaInvoke();
                return Comp.Visit(tree);
            }

            if (Rewriter.ToLowerInvariant() == "tailcallon") {
                //add more lambdas
                var calls = new InjectCalls();
                tree = calls.Visit(tree);
                var Comp = new TailCallOnRewritter();
                return Comp.Visit(tree);
            }

            if (Rewriter.ToLowerInvariant() == "tailcalloff") {
                var calls = new InjectCalls();
                tree = calls.Visit(tree);

                var Comp = new TailCallOffRewritter();
                return Comp.Visit(tree);
            }

            if (Rewriter.ToLowerInvariant() == "random") {
                var Comp = new RandomRewritter();
                return Comp.Visit(tree);
            }

            if (Rewriter.ToLowerInvariant() == "quote") {
                var Comp = new QuoteRewriter();
                return Comp.Visit(tree);
            }

            if (Rewriter.ToLowerInvariant() == "reduce") {
                var Comp = new ReduceRewriter();
                return Comp.Visit(tree);
            }

            return tree;
        }
#endif

        #endregion

        #region Validators
        // A Validator is responsible for verifying an Expression Tree executes with the expected results.
        public interface IValidator {
            /// <summary>
            /// For positive cases where the tree already contains validation logic (i.e., GenAreEqual).
            /// </summary>
            /// <param name="tree">An Expression Tree containing internal validation logic.</param>
            void Validate(Expression tree);

            /// <summary>
            /// For positive cases where the tree already contains validation logic (i.e., GenAreEqual).
            /// </summary>
            /// <param name="tree">An Expression Tree containing internal validation logic.</param>
            /// <param name="CompileToMethod">If true the Validator will use CompileToMethod to compile and execute this test.</param>
            void Validate(Expression tree, bool CompileToMethod);

            /// <summary>
            /// For positive cases of trees with no internal validation.
            /// External validation is provided in the form of an Action<Func<...>> or Action<Action<...>> 
            /// </summary>
            /// <typeparam name="T">The type of the validation delegate.</typeparam>
            /// <param name="tree">The Expression Tree to be validated.</param>
            /// <param name="validation">A delegate containing logic to validate the results of the tree's compilationg and execution.</param>
            void Validate<T>(Expression<T> tree, Action<T> validation);

            /// <summary>
            /// For negative cases which don't require arguments on tree invocation.
            /// </summary>
            /// <typeparam name="ExpectedException">The type of exception the tree is expected to throw.</typeparam>
            /// <param name="tree">The Expressino Tree to validate.</param>
            /// <param name="ThrowsOnCompile">Pass true if the tree should throw an Exception when compiled, false if the tree throws on invocation.</param>
            void ValidateException<ExpectedException>(Expression tree, bool ThrowsOnCompile) where ExpectedException : Exception;

            /// <summary>
            /// For negative cases which don't require arguments on tree invocation.
            /// </summary>
            /// <typeparam name="ExpectedException">The type of exception the tree is expected to throw.</typeparam>
            /// <param name="tree">The Expressino Tree to validate.</param>
            /// <param name="ThrowsOnCompile">Pass true if the tree should throw an Exception when compiled, false if the tree throws on invocation.</param>
            /// <param name="CompileToMethod">If true the Validator will use CompileToMethod to compile and execute this test.</param>
            void ValidateException<ExpectedException>(Expression tree, bool ThrowsOnCompile, bool CompileToMethod) where ExpectedException : Exception;
        }

#if !SILVERLIGHT3
        public class TestValidator : IValidator {
            private bool Rewrite = false;
            private string Rewriter;
            private bool CompileToMethod = false; // CompileAsMethodRewriter requires some extra work compared to others so we need to know if we're using it
            private bool VerifyAssembly = false;

            public TestValidator(string rewriter, bool compileToMethod, bool verifyAssembly) {
                if (rewriter != "") {
                    Rewrite = true;
                    this.Rewriter = rewriter;
                }
                this.CompileToMethod = compileToMethod;
                this.VerifyAssembly = verifyAssembly;
            }

            public void Validate(Expression tree) {
                Expression NewTree = PrepareTree(tree);

                var Lambda = Expression.Lambda<Func<TestResults>>(NewTree);
                Func<TestResults> f;

                if (this.CompileToMethod) {
                    f = TryCompileTreeAsMethod(NewTree);
                } else if (Rewrite) {
                    tree = ExpressionUtils.Rewrite(tree, Rewriter);
                    f = Lambda.Compile();
                } else {
                    f = Lambda.Compile();
                }

                // if the tree wasn't rewritten the TestResults' list will be empty and GenAreEqual will have validated it.
                // Otherwise we have a list of pairs of values to compare and a string to print in case of failure.
                TestResults Results = (TestResults)(f());
                foreach (var t in Results.Values) {
                    try {
                        Equal(t.Item1, t.Item2);
                    } catch (InvalidOperationException e) {
                        throw new TestFailedException(t.Item3, e);
                    }
                }
            }

            public void Validate(Expression tree, bool CompileToMethod) {
                // Little hacky: change global CompileToMethod flag, run the normal Validate (which will CTM as necessary), then restore CompileToMethod flag
                var tmp = this.CompileToMethod;
                try {
                    this.CompileToMethod = CompileToMethod;
                    Validate(tree);
                } finally {
                    this.CompileToMethod = tmp;
                }
            }

            // LambdaType is the Func<> or Action<> type of the LambdaExpression argument
            public void Validate<LambdaType>(Expression<LambdaType> tree, Action<LambdaType> validation) {
                Expression<LambdaType> NewTree = tree;
                if (Rewrite) {
                    NewTree = (Expression<LambdaType>)ExpressionUtils.Rewrite(tree, this.Rewriter);
                }
                validation(NewTree.Compile());
            }

            // Defines how the tree will be compiled and then defers validation to the helper method
            public void ValidateException<ExpectedException>(Expression tree, bool ThrowsOnCompile) where ExpectedException : Exception {
                Func<Expression, Func<TestResults>> CompilationExpression = 
                    (Expression treeToCompile) => { Func<TestResults> f = Expression.Lambda<Func<TestResults>>(treeToCompile).Compile(); return f; };
                ValidateExceptionHelper<ExpectedException>(tree, CompilationExpression, ThrowsOnCompile);
            }

            // Defines how the tree will be compiled and then defers validation to the helper method
            public void ValidateException<ExpectedException>(Expression tree, bool ThrowsOnCompile, bool CompileToMethod) where ExpectedException : Exception {
                Func<Expression, Func<TestResults>> CompilationExpression;
                if (CompileToMethod) {
                    CompilationExpression = (Expression treeToCompile) => { return TryCompileTreeAsMethod(treeToCompile); };
                } else {
                    CompilationExpression = (Expression treeToCompile) => { Func<TestResults> f = Expression.Lambda<Func<TestResults>>(treeToCompile).Compile(); return f; };
                }
                ValidateExceptionHelper<ExpectedException>(tree, CompilationExpression, ThrowsOnCompile);
            }

            // This does the real work for validating the correct Exception occurs in the correct place of a test tree.
            // compilationExpression holds the code to do the correct compilation based on previous arguments: .Compile() or .CompileToMethod()
            private void ValidateExceptionHelper<ExpectedException>(Expression tree, Func<Expression, Func<TestResults>> compilationExpression, bool ThrowsOnCompile) where ExpectedException : Exception {
                Expression NewTree = PrepareTree(tree);
                if (Rewrite) {
                    NewTree = ExpressionUtils.Rewrite(NewTree, Rewriter);
                }
                // if the tree wasn't rewritten the TestResults' list will be empty and GenAreEqual will have validated it.
                var Lambda = Expression.Lambda<Func<TestResults>>(NewTree);
                // Messy exception handling code because Throws<> can be used on it's own (as in dev scenarios).
                // This code will give more specific error messages.
                if (ThrowsOnCompile) {
                    try {
                        Throws<ExpectedException>(() => {
                            var f = compilationExpression(NewTree);
                        });
                    } catch (TestFailedException e) {
                        throw new TestFailedException(
                            string.Format("Expected {0} on tree compilation but none was thrown.", typeof(ExpectedException).Name),
                            (e.InnerException != null) ? e.InnerException : null
                        );
                    }
                } else {
                    try {
                        var f = compilationExpression(NewTree);
                        try {
                            Throws<ExpectedException>(() => f());
                        } catch (TestFailedException) {
                            throw new TestFailedException(string.Format("Expected {0} on tree execution but none was thrown.", typeof(ExpectedException).Name));
                        }
                    } catch (Exception e) {
                        if (!(e is TestFailedException)) {
                            throw new TestFailedException(string.Format("Expected {0} on tree execution but on tree compilation got:\n{1}\n{2}.", typeof(ExpectedException).Name, e.GetType().Name, e.Message), e);
                        } else {
                            throw;
                        }
                    }
                }
            }

            // Takes a tree and creates a Func<TestResults> with the tree compiled as a method.
            // If the tree cannot be compiled to method (ex. it contains live objects) this method falls back to regular compilation (which could still fail).
            public Func<TestResults> TryCompileTreeAsMethod(Expression tree) {
                var lambda = Expression.Lambda<Func<TestResults>>(tree);
                Func<TestResults> f = null;

                CompileAsMethodRewritter cm = new CompileAsMethodRewritter();
                cm.Compilable = true;
                lambda = Expression.Lambda<Func<TestResults>>(cm.Visit(tree));
                if (cm.Compilable) {
                    if (this.VerifyAssembly) {
                        f = CompileAsMethodUtils.CompileAsMethodWithSave<Func<TestResults>>(lambda);
                    } else {
                        f = CompileAsMethodUtils.CompileAsMethod<Func<TestResults>>(lambda);
                    }
                } else {
                    try {
                        f = lambda.Compile();
                    } catch (Exception e) {
                        throw new Exception("The Expression Tree cannot be compiled to a method. The fallback to regular compilation failed.", e);
                    }
                }

                return f;
            }

            // Wrap a test tree to return a TestResults object so the GenAreEqualRewriter can validate the results after tree invocation.
            private static Expression PrepareTree(Expression tree) {
                ParameterExpression TR = Expression.Variable(typeof(TestResults), "TestResults");
                
                Expression FinalTree =
                    Expression.Block(
                        new[] { TR },
                        Expression.Assign(TR, Expression.New(typeof(TestResults))),
                        tree,
                        TR
                    );

                return FinalTree;
            }

        }
#endif

        #endregion
    }   

    static class TestSpan {
        private static string _PrefixPath;

        private static string PrefixPath {
            get {
                if (_PrefixPath == null) {
                    try {
#if SILVERLIGHT
                        string setting = null;
#else
                        string setting = Environment.GetEnvironmentVariable("DLR_ROOT");
                        if (setting == null) {
                            setting = Environment.GetEnvironmentVariable("dlr_root");
                        }
#endif
                        if (setting == null) {
                            string path;
                            path = Assembly.GetExecutingAssembly().Location;
                            path = Path.GetDirectoryName(path);
                            int length = path.LastIndexOf('\\');
                            if (length > 0) {
                                length = path.LastIndexOf('\\', length - 1);
                            }
                            if (length > 0) {
                                setting = path.Substring(0, length);
                            }
                        }
                        _PrefixPath = Path.Combine(setting, "Runtime\\Tests\\TestAst\\");
                    } catch (SecurityException) {
                        _PrefixPath = "";
                    }
                }
                return _PrefixPath;
            }
        }

        /// <summary>
        /// Returns the full path to TestScenarios.Tests.cs
        /// </summary>
        public static string SourceFile {
            get {
                return PrefixPath + "TestScenarios.Tests.cs";
            }
        }

        /// <summary>
        /// Wraps an expression with DebugInfo pointing to the callers location or the location
        /// of any deeper frame in the current stack.
        /// </summary>
        /// <param name="index">0 for the current frame, 1 for the previous frame, etc.</param>
        /// <returns></returns>
        public static Expression GetDebugInfoForFrame(Expression var, int index) {
#if !SILVERLIGHT
            //Skip this frame too
            index = index + 1;
            StackTrace t = new StackTrace(true);
            if (index > t.FrameCount - 1)
                throw new ArgumentException("There aren't that many frames currently.");
            StackFrame frame = t.GetFrame(index);

            if (frame.GetFileColumnNumber() > 0 && frame.GetFileLineNumber() > 0){
                return ExpressionUtils.AddDebugInfo(
                    var,
                    Expression.SymbolDocument(SourceFile),
                    frame.GetFileLineNumber(),
                    frame.GetFileColumnNumber(),
                    frame.GetFileLineNumber(),
                    frame.GetFileColumnNumber()
                );
            } else {
                return var;
            }
#else
            return var;
#endif
            
        }


    }

    // Used for custom multiple negative tests
    public class NegativeTestCaseArgumentTypeException : ArgumentException {
        public NegativeTestCaseArgumentTypeException(String message)
            : base(message) { }
        public NegativeTestCaseArgumentTypeException(String message, Exception inner) : base(message, inner) { }
    }

    // The .DebugView property is not public in Dev10 so we need to get it through reflection
    public static class DebugViewExtensions {
        public static string DebugView(this Expression expression) {
            var prop = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return (string)prop.GetValue(expression, null);
        }
    }

    #region CompileAsMethodUtils
    public static class CompileAsMethodUtils {
        private static System.Reflection.Emit.AssemblyBuilder AB;
        private static System.Reflection.Emit.ModuleBuilder MB;
        private static System.Reflection.Emit.TypeBuilder TB = null;
        private const string AssemblyName = "CompileAsMethodAssembly";
        private static int TBCounter = 0;

#if !SILVERLIGHT3
        public static T CompileAsMethod<T>(this Expression<T> lambda) {

            if (TB == null) {
                AppDomain myDomain = AppDomain.CurrentDomain;
                AssemblyName asmName = new AssemblyName("CompileAsMethodAssembly");

#if SILVERLIGHT
                AB = myDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
#else
                AB = myDomain.DefineDynamicAssembly(asmName, System.Reflection.Emit.AssemblyBuilderAccess.Run, CompileAsMethodAttributes);
#endif
                MB = AB.DefineDynamicModule("MyModule");
            }

            TB = MB.DefineType("MyClass" + TBCounter.ToString(), TypeAttributes.Public | TypeAttributes.Class);

            var MethodName = "MyMethod";
            System.Reflection.Emit.MethodBuilder MtB = TB.DefineMethod(MethodName, MethodAttributes.Public | MethodAttributes.Static);
            TBCounter++;
            lambda.CompileToMethod(MtB);

            var f = (T)(object)Delegate.CreateDelegate(typeof(T), TB.CreateType().GetMethod(MethodName));
            return f;
        }

        //mark the assembly as transparent so it works in partial trust.
        private static CustomAttributeBuilder[] CompileAsMethodAttributes = new[] { 
            new CustomAttributeBuilder(typeof(SecurityTransparentAttribute).GetConstructor(Type.EmptyTypes), new object[0])
        };

         public static T CompileAsMethodWithSave<T>(this Expression<T> lambda) {
            if (TB == null) {
                AppDomain myDomain = AppDomain.CurrentDomain;
                AssemblyName asmName = new AssemblyName(AssemblyName);

#if SILVERLIGHT
                AB = myDomain.DefineDynamicAssembly(asmName, System.Reflection.Emit.AssemblyBuilderAccess.Run); 
#else
                AB = myDomain.DefineDynamicAssembly(asmName, System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave, CompileAsMethodAttributes);
#endif
                MB = AB.DefineDynamicModule("MyModule");
            }

            TB = MB.DefineType("MyClass" + TBCounter.ToString(), TypeAttributes.Public | TypeAttributes.Class);

            var MethodName = "MyMethod";
            System.Reflection.Emit.MethodBuilder MtB = TB.DefineMethod(MethodName, MethodAttributes.Public | MethodAttributes.Static);
            TBCounter++;
            lambda.CompileToMethod(MtB);

            var f = (T)(object)Delegate.CreateDelegate(typeof(T), TB.CreateType().GetMethod(MethodName));
            return (T)(object)f;
        }
#endif

        public static string SaveAssembly() {
            var fileName = AssemblyName + ".dll";
#if !SILVERLIGHT
            AB.Save(fileName);
#endif
            return fileName;
        }
    }
    #endregion
}
