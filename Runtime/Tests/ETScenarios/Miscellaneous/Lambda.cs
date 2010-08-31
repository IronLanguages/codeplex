#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MsSc = System.Dynamic;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Lambda {

        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }



        // Test     : Pass parameters with the same name
        // Expected : Exception
        // Note     : Legal for LinqV1?
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Pass_Parameters_With_Same_Name", new string[] { "positive", "Lambda", "miscellaneous", "Pri1" })]
        public static Expr Pass_Parameters_With_Same_Name(EU.IValidator V) {
            Expression exp = Expr.Multiply(Expr.Constant(2, typeof(int)), Expr.Constant(2, typeof(int)));
            ParameterExpression v1 = Expression.Variable(typeof(int), "foo");
            ParameterExpression v2 = Expression.Variable(typeof(int), "foo");
            ParameterExpression[] args = new ParameterExpression[] { v1, v2 };
            // from test plan: "According to John, they might check this at compilation time rather than factory call time."

            Expression exp2 = Expr.Invoke(Expr.Lambda(exp, args), new Expression[] { Expr.Constant(1), Expr.Constant(1) });
            var tree = EU.GenAreEqual(Expr.Constant(4, typeof(int)), exp2);
            V.Validate(tree);
            return tree;

        }

        // Test     : Pass parameters that differ only by case
        // Expected : Should work
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Pass_Parameters_With_Different_Case", new string[] { "positive", "Lambda", "miscellaneous", "Pri1" })]
        public static Expr Pass_Parameters_With_Different_Case(EU.IValidator V) {
            // Create simple expression tree
            Expression exp = Expr.Multiply(Expr.Constant(2, typeof(int)), Expr.Constant(2, typeof(int)));

            // Create parameters
            ParameterExpression v1 = Expression.Variable(typeof(int), "Foo");
            ParameterExpression v2 = Expression.Variable(typeof(int), "foo");
            ParameterExpression[] args = new ParameterExpression[] { v1, v2 };

            Expression lambdaTestExp = Expr.Lambda(exp, args);

            // Shouldn't this fail here - during Invoke
            Expression exp2 = Expr.Invoke(lambdaTestExp, new Expression[] { Expr.Constant(1), Expr.Constant(1) });
            var tree = EU.GenAreEqual(Expr.Constant(4, typeof(int)), exp2);
            V.Validate(tree);
            return tree;
        }

        // Test     : Pass repeated parameters
        // Expected : Exception
        // Note     : Exception is not thrown in the factory call so return the ET
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Pass_Repeated_Parameters", new string[] { "negative", "Lambda", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Pass_Repeated_Parameters(EU.IValidator V) {
            // Create simple expression tree
            Expression exp = Expr.Multiply(Expr.Constant(2, typeof(int)), Expr.Constant(2, typeof(int)));

            // Create parameters
            ParameterExpression v = Expression.Variable(typeof(int), "Foo");

            ParameterExpression[] args = new ParameterExpression[] { v, v };

            Expression lambdaTestExp = EU.Throws<System.ArgumentException>(() => { Expr.Lambda(exp, args); });
            //var tree = Expr.Invoke(lambdaTestExp, new Expression[] { Expr.Constant(1), Expr.Constant(1) });

            return Expr.Empty();
        }

        // Test      : Pass null to delegateType
        // Expected  : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Pass_Null_To_DelegateType", new string[] { "negative", "Lambda", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Pass_Null_To_DelegateType(EU.IValidator V) {
            // Create simple expression tree
            Expression exp = Expr.Multiply(Expr.Constant(2, typeof(int)), Expr.Constant(2, typeof(int)));

            // Create parameters
            ParameterExpression v1 = Expression.Variable(typeof(int), "foo");
            ParameterExpression v2 = Expression.Variable(typeof(int), "bar");

            ParameterExpression[] args = new ParameterExpression[] { v1, v2 };

            var lambdaTestExp = EU.Throws<ArgumentNullException>(() => { Expr.Lambda((Type)null, exp, args); });

            return Expression.Empty();
        }

        // Test      : Pass a non delegate to TDelegate	
        // Expected  : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Pass_Non_Delegate_To_TDelegate", new string[] { "negative", "Lambda", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Pass_Non_Delegate_To_TDelegate(EU.IValidator V) {
            // Create simple expression tree
            Expression exp = Expr.Multiply(Expr.Constant(2, typeof(int)), Expr.Constant(2, typeof(int)));

            // Create parameters
            ParameterExpression v1 = Expression.Variable(typeof(int), "foo");
            ParameterExpression v2 = Expression.Variable(typeof(int), "bar");

            ParameterExpression[] args = new ParameterExpression[] { v1, v2 };

            var lambdaTestExp = EU.Throws<ArgumentException>(() => { Expr.Lambda<int>(exp, args); });

            return Expression.Empty();
        }

        // Test      : Pass less parameters than TDelegate defines
        // Expected  : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Pass_Less_Parameters_Than_TDelegate_Defines", new string[] { "negative", "Lambda", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Pass_Less_Parameters_Than_TDelegate_Defines(EU.IValidator V) {
            // Create simple expression tree
            Expression exp = Expr.Multiply(Expr.Constant(2, typeof(int)), Expr.Constant(2, typeof(int)));

            // Create parameters
            ParameterExpression p1 = Expression.Variable(typeof(int), "foo");

            ParameterExpression[] args = new ParameterExpression[] { p1 };

            Expression lambdaTestExp = EU.Throws<ArgumentException>(() => { Expr.Lambda<Func<int, int, int>>(exp, args); });

            return Expression.Empty();
        }

        // Test      : Pass more parameters than TDelegate defines
        // Expected  : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Pass_More_Parameters_Than_TDelegate_Defines", new string[] { "negative", "Lambda", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Pass_More_Parameters_Than_TDelegate_Defines(EU.IValidator V) {
            // Create simple expression tree
            Expression exp = Expr.Multiply(Expr.Constant(2, typeof(int)), Expr.Constant(2, typeof(int)));

            // Create parameters
            ParameterExpression p1 = Expression.Variable(typeof(int), "foo");
            ParameterExpression p2 = Expression.Variable(typeof(int), "bar");

            ParameterExpression[] args = new ParameterExpression[] { p1, p2 };

            Expression lambdaTestExp = EU.Throws<ArgumentException>(() => { Expr.Lambda<Func<int, int>>(exp, args); });

            return Expression.Empty();
        }

        public class CBase {
            protected int _x;

            public CBase() {
            }
            public CBase(int n) {
                _x = n;
            }
            public static int operator *(CBase a, CBase b) {
                return (a._x * b._x);
            }
        }

        public class CChild : CBase {

            public CChild(int n) {
                _x = n;
            }

        }


        // Test      : Pass Parameters With Reference Conversion To TDelegates
        // Expected  : Works
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Pass_Parameters_With_Reference_Conversion_To_TDelegates", new string[] { "positive", "Lambda", "miscellaneous", "Pri1" })]
        public static Expr Pass_Parameters_With_Reference_Conversion_To_TDelegates(EU.IValidator V) {
            // Create parameters
            ParameterExpression p1 = Expression.Variable(typeof(CBase), "x");
            ParameterExpression p2 = Expression.Variable(typeof(CBase), "y");

            // Create simple expression tree - where it will only work if CChild is converted th CBase
            Expression exp = Expr.Multiply(p1, p2);

            ParameterExpression[] args = new ParameterExpression[] { p1, p2 };

            // Args are of base type and TDelegate Parameters are of derived type thus conversion takes place at factory level
            Expression<Func<CChild, CChild, int>> lambdaTestExp = Expr.Lambda<Func<CChild, CChild, int>>(exp, args);

            Expression exp2 = Expr.Invoke(lambdaTestExp, new Expression[] { 
                                                                Expr.Constant(new CChild(2), typeof(CChild)), 
                                                                Expr.Constant(new CChild(3), typeof(CChild)) });
            var tree = Expression.Empty();
            V.Validate(tree);
            return tree;


        }

        // Test      : Pass Parameters With Non Reference Conversion To TDelegate's
        // Expected  : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Pass_Parameters_With_Non_Reference_Conversion_To_TDelegates", new string[] { "negative", "Lambda", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Pass_Parameters_With_Non_Reference_Conversion_To_TDelegates(EU.IValidator V) {

            // Create trivial expression tree
            Expression exp = Expr.Empty();

            // Create parameters with "Non Ref Conversion Type"
            ParameterExpression[] args = new ParameterExpression[] { Expression.Variable(typeof(short), "x") };

            // Use Action<> in order to ignore the return type and focus on lambda parameter and TDelegate's
            var lambdaTestExp = EU.Throws<ArgumentException>(() =>
            {
                Expr.Lambda<Action<int>>(exp, args);
            });
            //Expression exp2 = Expr.Invoke(lambdaTestExp, new Expression[] { Expr.Constant(3, typeof(short)) });

            // Should throw before this
            return Expression.Empty();
        }


        // Test      : Nest three lambdas. Have each lambda use a parent lambda's parameter
        // Expected  : Should work	
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Nest_Three_Lambdas", new string[] { "positive", "Lambda", "miscellaneous", "Pri1" })]
        public static Expr Nest_Three_Lambdas(EU.IValidator V) {

            // Create some parameters
            ParameterExpression p1 = Expression.Variable(typeof(int), "x");
            ParameterExpression p2 = Expression.Parameter(typeof(int), "y");
            ParameterExpression p3 = Expression.Parameter(typeof(int), "z");

            Expression lambdaTestExp1 = Expr.Lambda(
                                                Expr.Invoke(
                                                        Expr.Lambda(
                                                            Expr.Invoke(
                                                                Expr.Lambda(
                                                                            Expr.Multiply(p2, p3),
                                                                            new ParameterExpression[] { p3 }),
                                                                new Expression[] { p2 }),
                                                             new ParameterExpression[] { p2 }),
                                                         new Expression[] { p1 }),
                                                 new ParameterExpression[] { p1 });

            var tree = EU.GenAreEqual(Expr.Constant(4, typeof(int)), Expr.Invoke(lambdaTestExp1, new Expression[] { Expr.Constant(2) }));
            V.Validate(tree);
            return tree;
        }

        // Test     : Define a nested lambda with a parameter with the same name as a parent's parameter name. 
        //            Use said parameter
        // Expected : Should work, refer to both parameters in body
        // Todo     : Clean up/simplify
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Defined_Nested_Lambda_Same_Name_Parameter", new string[] { "positive", "Lambda", "miscellaneous", "Pri1" })]
        public static Expr Defined_Nested_Lambda_Same_Name_Parameter(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Create parameters
            ParameterExpression p1 = Expression.Parameter(typeof(int), "x");
            ParameterExpression p2 = Expression.Variable(typeof(int), "y");
            ParameterExpression p3 = Expression.Parameter(typeof(int), "x");


            Expression lambdaTestExp1 = Expr.Lambda(
                                                Expr.Invoke(
                                                        Expr.Lambda(
                                                            Expr.Invoke(
                                                                Expr.Lambda(
                                                                            Expr.Multiply(p2, p3),
                                                                            new ParameterExpression[] { p3 }),
                                                                new Expression[] { p2 }),
                                                             new ParameterExpression[] { p2 }),
                                                         new Expression[] { p1 }),
                                                 new ParameterExpression[] { p1 });

            Expression exp = Expr.Invoke(lambdaTestExp1, new Expression[] { Expr.Constant(2) });


            Expressions.Add(EU.GenAreEqual(Expr.Constant(4, typeof(int)), exp));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Test     : Define a nested lambda that uses variables defined in an enclosing lambda. 
        //            Have the nesting lambda return the nested lambda and invoke it outside the 
        //            nesting lambda.
        // Expected : Validate value of variable as seen by nested lambda
        // Note     : Psuedo Code Example
        //            
        //            Param x = expr.param(int, Name1);
        //            Param y = expr.param(int, Name1);
        //            NestedLambda = Lambda(Block(x = x + 1 ; Return(x)), x);
        //            NestingLambda = Lambda(Block(x = 2 ; var y = NestedLambda(5); genareeequal(x == 2); genareequal(y==6));
        //            NestingLambda.Invoke().
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Define_Nested_Lambda_Invoke_From_Nesting_Lambda", new string[] { "positive", "Lambda", "miscellaneous", "Pri1" })]
        public static Expr Define_Nested_Lambda_Invoke_From_Nesting_Lambda(EU.IValidator V) {

            // See 'Defined_Nested_Lambda_Same_Name_Parameter' Did I cover this in the previous test?
            List<Expression> ExprsNested = new List<Expression>();

            ParameterExpression x = Expression.Variable(typeof(int), "x");
            ParameterExpression y = Expression.Variable(typeof(int), "y");

            ExprsNested.Add(Expr.Assign(x, Expr.Add(x, Expr.Constant(1, typeof(int)))));
            ExprsNested.Add(x);
            LambdaExpression NestedLambda = Expr.Lambda<Func<int, int>>(Expr.Block(ExprsNested), new ParameterExpression[] { x });

            LambdaExpression NestingLambda = Expr.Lambda(
                                                        Expr.Block(new ParameterExpression[] { x, y },
                                                                   new[]{ 
                                                                    Expr.Assign(x, Expr.Constant(2, typeof(int))), 
                                                                    //var y = NestedLambda(5)
                                                                    y,
                                                                    Expr.Assign(y, 
                                                                        Expr.Constant(NestedLambda.Compile().DynamicInvoke(
                                                                                                                new object[] { 5 }), 
                                                                                                                typeof(int))),
                                                                    //genareeequal(x == 2)
                                                                    EU.GenAreEqual(Expr.Constant(2, typeof(int)), x),
                                                                    // genareequal(y==6)
                                                                    EU.GenAreEqual(Expr.Constant(6, typeof(int)), y)}),
                                                                    new ParameterExpression[] { });
            var tree = Expr.Block(new[] { x, y }, NestingLambda);
            V.Validate(tree);
            return tree;
        }

        // Test     : Define a nested lambda that uses variables defined in an enclosing lambda. Have the nesting 
        //            lambda execute the nested lambda.
        // Expected : Validate value of variables after lambda execution
        // Note     : Use delegate?
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Defined_Nested_Using_Variables_From_Enclosing_Lambda", new string[] { "positive", "Lambda", "miscellaneous", "Pri1" })]
        public static Expr Defined_Nested_Using_Variables_From_Enclosing_Lambda(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            // Create a variable that is passed in at the parent lambda
            ParameterExpression v0 = Expression.Variable(typeof(int), "x");

            ParameterExpression[] emptyArgs = new ParameterExpression[] { };

            // Create block for nested lambda
            Expression exprNestedLambda = Expr.Assign(v0, Expr.Add(v0, Expr.Constant(1)));

            // Create lambda for above block
            Expression<Func<int>> childLambda = Expr.Lambda<Func<int>>(exprNestedLambda, emptyArgs);

            // Create block for nesting lambda
            Expressions.Add(Expr.Assign(v0, Expr.Constant(1)));

            // Add call to Invoke of nested lambda from nesting lambda
            Expressions.Add(Expr.Invoke(childLambda, emptyArgs));
            // Return variable to check for sideo-effects
            Expressions.Add(v0);

            // Parent lambda
            Expression<Func<int, int>> parentLambda = Expr.Lambda<Func<int, int>>(Expr.Block(Expressions),
                                                                                  new ParameterExpression[] { v0 });
            // Invoke nesting lambda - value initalize and then is ignored
            Expression exprTest = Expr.Invoke(parentLambda, new Expression[] { Expr.Constant(0) });

            // Compare 
            var tree = Expr.Block(EU.GenAreEqual(Expr.Constant(2), exprTest));
            V.Validate(tree);
            return tree;
        }


        // Test     : Pass null to delegateType
        // Expected : Exception
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Generator_Pass_Null_To_DelegateType", new string[] { "negative", "Lambda", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Generator_Pass_Null_To_DelegateType(EU.IValidator V) {

            LabelTarget label = Expr.Label(typeof(int));

            Expression exp = Expr.Multiply(Expr.Constant(2, typeof(int)), Expr.Constant(2, typeof(int)));

            ParameterExpression v1 = Expression.Variable(typeof(int), "foo");
            ParameterExpression v2 = Expression.Variable(typeof(int), "foo");

            ParameterExpression[] args = new ParameterExpression[] { v1, v2 };

            // Expect to throw an exception here
            var Gen = EU.Throws<ArgumentException>(() => { AstUtils.GeneratorLambda((Type)null, label, exp, args); });

            return Expr.Empty();
        }


        //closures and scopes. New scope for each variable created check closures are independent.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Lambda 100", new string[] { "positive", "Lambda", "loop", "scope", "miscellaneous", "Pri1" })]
        public static Expr Lambda100(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();

            ParameterExpression Result = Expr.Parameter(typeof(string), "");
            ParameterExpression Var1 = Expr.Parameter(typeof(int), "var1");
            //lambda that assigns to a variable
            var Lambda1 = Expr.Lambda<Action>(Expr.Assign(Var1, Expr.Add(Var1, Expr.Constant(1))));

            //lambda that prints variable
            var Lambda2 = Expr.Lambda<Action>(
                EU.ConcatEquals(
                    Result,
                    Expr.Call(
                        Var1,
                        typeof(int).GetMethod("ToString", new Type[] { })
                    )
                )
            );


            var lambdas = Expr.Parameter(typeof(Action[]), "");
            var indexer = Expr.Parameter(typeof(int), "");

            Expressions.Add(Expr.Assign(lambdas, Expr.Constant(new Action[4])));

            Expressions.Add(Expr.Assign(indexer, Expr.Constant(0)));

            var lt1 = Expr.Label();

            var Loop1Body = Expr.Block(new[] { Var1 }, Expr.Assign(Expr.ArrayAccess(lambdas, indexer), Lambda1),
                                    Expr.Assign(indexer, Expr.Add(indexer, Expr.Constant(1))),
                                    Expr.Assign(Expr.ArrayAccess(lambdas, indexer), Lambda2),
                                    Expr.Assign(indexer, Expr.Add(indexer, Expr.Constant(1))),
                                    Expr.Condition(Expr.Equal(indexer, Expr.Constant(4)), Expr.Break(lt1), Expr.Empty()));

            var loop1 = Expr.Loop(Loop1Body,
                                    lt1);

            Expressions.Add(loop1);

            Expressions.Add(Expr.Assign(indexer, Expr.Constant(0)));

            var lt2 = Expr.Label();

            var Loop2Body = Expr.Block(Expr.Invoke(Expr.ArrayAccess(lambdas, indexer)),
                                    Expr.Assign(indexer, Expr.Add(indexer, Expr.Constant(1))),
                                    Expr.Invoke(Expr.ArrayAccess(lambdas, indexer)),
                                    Expr.Assign(indexer, Expr.Add(indexer, Expr.Constant(1))),
                                    Expr.Condition(Expr.Equal(indexer, Expr.Constant(4)), Expr.Break(lt2), Expr.Empty()));


            var loop2 = Expr.Loop(Loop2Body, lt2);

            Expressions.Add(loop2);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("11"), Result));

            var tree = Expr.Block(new[] { indexer, lambdas, Result }, Expressions);
            V.Validate(tree);
            return tree;
        }


        //closures and scopes. New scope for each variable created check closures are not independent.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Lambda 101", new string[] { "positive", "Lambda", "loop", "scope", "miscellaneous", "Pri1" })]
        public static Expr Lambda101(EU.IValidator V) {
            List<Expr> Expressions = new List<Expr>();



            ParameterExpression Result = Expr.Parameter(typeof(string), "");
            ParameterExpression Var1 = Expr.Parameter(typeof(int), "var1");
            //lambda that assigns to a variable
            var Lambda1 = Expr.Lambda<Action>(Expr.Assign(Var1, Expr.Add(Var1, Expr.Constant(1))));

            //lambda that prints variable
            var Lambda2 = Expr.Lambda<Action>(
                EU.ConcatEquals(
                    Result,
                    Expr.Call(
                        Var1,
                        typeof(int).GetMethod("ToString", new Type[] { })
                    )
                )
            );


            var lambdas = Expr.Parameter(typeof(Action[]), "");
            var indexer = Expr.Parameter(typeof(int), "");

            Expressions.Add(Expr.Assign(lambdas, Expr.Constant(new Action[4])));

            Expressions.Add(Expr.Assign(indexer, Expr.Constant(0)));

            var lt1 = Expr.Label();

            var Loop1Body = Expr.Block(Expr.Assign(Expr.ArrayAccess(lambdas, indexer), Lambda1),
                                    Expr.Assign(indexer, Expr.Add(indexer, Expr.Constant(1))),
                                    Expr.Assign(Expr.ArrayAccess(lambdas, indexer), Lambda2),
                                    Expr.Assign(indexer, Expr.Add(indexer, Expr.Constant(1))),
                                    Expr.Condition(Expr.Equal(indexer, Expr.Constant(4)), Expr.Break(lt1), Expr.Empty()));

            var loop1 = Expr.Loop(Loop1Body,
                                    lt1);

            Expressions.Add(loop1);

            Expressions.Add(Expr.Assign(indexer, Expr.Constant(0)));

            var lt2 = Expr.Label();

            var Loop2Body = Expr.Block(Expr.Invoke(Expr.ArrayAccess(lambdas, indexer)),
                                    Expr.Assign(indexer, Expr.Add(indexer, Expr.Constant(1))),
                                    Expr.Invoke(Expr.ArrayAccess(lambdas, indexer)),
                                    Expr.Assign(indexer, Expr.Add(indexer, Expr.Constant(1))),
                                    Expr.Condition(Expr.Equal(indexer, Expr.Constant(4)), Expr.Break(lt2), Expr.Empty()));


            var loop2 = Expr.Loop(Loop2Body, lt2);

            Expressions.Add(loop2);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("12"), Result));

            var tree = Expr.Block(new[] { indexer, lambdas, Result, Var1 }, Expressions);
            V.Validate(tree);
            return tree;
        }


        interface I {
            int X { get; }
            int SetX();
        }

        struct S : I {
            public int X { get; private set; }

            public int SetX() {
                X = 7;
                return 0;
            }
        }

        //Regress 575396
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Lambda 102", new string[] { "negative", "Lambda", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Lambda102(EU.IValidator V) {
            Type t = typeof(System.ValueType);
            MethodInfo m = t.GetMethod("GetTypeCode");
            ParameterExpression p = Expression.Parameter(typeof(S), "s");
            var e =
                EU.Throws<System.ArgumentNullException>(() =>
                {
                    Expression.Lambda<Func<S, TypeCode>>(
                        Expression.Call(p, m), p);
                });				    

            return Expr.Empty();
        }

        //name of created methods.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Lambda 103", new string[] { "positive", "Lambda", "miscellaneous", "Pri1" })]
        public static Expr Lambda103(EU.IValidator V) {
            String DefaultName = "lambda_method";
            if (Expr.Lambda(Expr.Constant("test1"), new ParameterExpression[] { }).Compile().Method.Name.CompareTo(DefaultName) != 0)
                throw new Exception("Lambda has unexpected method value 1");


            if (Expr.Lambda(Expr.Constant("test1"), (IEnumerable<ParameterExpression>)new ParameterExpression[] { }).Compile().Method.Name.CompareTo(DefaultName) != 0)
                throw new Exception("Lambda has unexpected method value 2");

            if (Expr.Lambda(Expr.Constant("test1"), true, new ParameterExpression[] { }).Compile().Method.Name.CompareTo(DefaultName) != 0)
                throw new Exception("Lambda has unexpected method value 3");

            if (Expr.Lambda(Expr.Constant("test1"), true, (IEnumerable<ParameterExpression>)new ParameterExpression[] { }).Compile().Method.Name.CompareTo(DefaultName) != 0)
                throw new Exception("Lambda has unexpected method value 4");

            if (Expr.Lambda(Expr.Constant("test1"), "123", (IEnumerable<ParameterExpression>)new ParameterExpression[] { }).Compile().Method.Name.CompareTo("123") != 0)
                throw new Exception("Lambda has unexpected method value 5");

            if (Expr.Lambda(typeof(Action), Expr.Constant("test1"), new ParameterExpression[] { }).Compile().Method.Name.CompareTo(DefaultName) != 0)
                throw new Exception("Lambda has unexpected method value 6");

            if (Expr.Lambda(typeof(Func<String>), Expr.Constant("test1"), (IEnumerable<ParameterExpression>)new ParameterExpression[] { }).Compile().Method.Name.CompareTo(DefaultName) != 0)
                throw new Exception("Lambda has unexpected method value 7");

            if (Expr.Lambda(Expr.Constant("test1"), "456", true, (IEnumerable<ParameterExpression>)new ParameterExpression[] { }).Compile().Method.Name.CompareTo("456") != 0)
                throw new Exception("Lambda has unexpected method value 8");

            if (Expr.Lambda(typeof(Action), Expr.Constant("test1"), true, new ParameterExpression[] { }).Compile().Method.Name.CompareTo(DefaultName) != 0)
                throw new Exception("Lambda has unexpected method value 9");

            if (Expr.Lambda(typeof(Action), Expr.Constant("test1"), false, (IEnumerable<ParameterExpression>)new ParameterExpression[] { }).Compile().Method.Name.CompareTo(DefaultName) != 0)
                throw new Exception("Lambda has unexpected method value 10");

            if (Expr.Lambda(typeof(Action), Expr.Constant("test1"), "", (IEnumerable<ParameterExpression>)new ParameterExpression[] { }).Compile().Method.Name.CompareTo("") != 0)
                throw new Exception("Lambda has unexpected method value 11");

            if (Expr.Lambda(typeof(Action), Expr.Constant("test1"), "$%^!@#&*()_+\\|]}[{;:'\"<>,./?", true, (IEnumerable<ParameterExpression>)new ParameterExpression[] { }).Compile().Method.Name.CompareTo("$%^!@#&*()_+\\|]}[{;:'\"<>,./?") != 0)
                throw new Exception("Lambda has unexpected method value 12");

            return Expr.Empty();
        }

//Exception does not contain a "TargetSite" field/property in Silverlight
#if !SILVERLIGHT
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Lambda 104", new string[] { "positive", "Lambda", "miscellaneous", "Pri1" })]
        public static Expr Lambda104(EU.IValidator V) {
            String DefaultName = "lambda_method";
            try {
                Expr.Lambda(Expr.Call(Expr.Lambda(Expr.Throw(Expr.Constant(new Exception())), new ParameterExpression[] { }), "Invoke", null)).Compile().DynamicInvoke();
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo(DefaultName) != 0)
                    throw new Exception("Lambda has unexpected method value 1");
            }

            try {
                Expr.Lambda(Expr.Call(Expr.Lambda(Expr.Throw(Expr.Constant(new Exception())), (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)).Compile().DynamicInvoke();
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo(DefaultName) != 0)
                    throw new Exception("Lambda has unexpected method value 2");
            }

            try {
                Expr.Lambda(Expr.Call(Expr.Lambda(Expr.Throw(Expr.Constant(new Exception())), true, new ParameterExpression[] { }), "Invoke", null)).Compile().DynamicInvoke();
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo(DefaultName) != 0)
                    throw new Exception("Lambda has unexpected method value 3");
            }

            try {
                Expr.Lambda(Expr.Call(Expr.Lambda(Expr.Throw(Expr.Constant(new Exception())), true, (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)).Compile().DynamicInvoke();
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo(DefaultName) != 0)
                    throw new Exception("Lambda has unexpected method value 4");
            }

            try {
                Expr.Lambda(Expr.Call(Expr.Lambda(Expr.Throw(Expr.Constant(new Exception())), "123", (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)).Compile().DynamicInvoke();
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo("123") != 0)
                    throw new Exception("Lambda has unexpected method value 5");
            }

            try {
                Expr.Lambda(Expr.Call(Expr.Lambda(typeof(Action), Expr.Throw(Expr.Constant(new Exception())), new ParameterExpression[] { }), "Invoke", null)).Compile().DynamicInvoke();
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo(DefaultName) != 0)
                    throw new Exception("Lambda has unexpected method value 6");
            }

            try {
                Expr.Lambda(Expr.Call(Expr.Lambda(typeof(Action), Expr.Throw(Expr.Constant(new Exception())), (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)).Compile().DynamicInvoke();
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo(DefaultName) != 0)
                    throw new Exception("Lambda has unexpected method value 7");
            }

            try {
                Expr.Lambda(Expr.Call(Expr.Lambda(Expr.Throw(Expr.Constant(new Exception())), "456", true, (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)).Compile().DynamicInvoke();
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo("456") != 0)
                    throw new Exception("Lambda has unexpected method value 8");
            }

            try {
                Expr.Lambda(Expr.Call(Expr.Lambda(typeof(Action), Expr.Throw(Expr.Constant(new Exception())), true, new ParameterExpression[] { }), "Invoke", null)).Compile().DynamicInvoke();
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo(DefaultName) != 0)
                    throw new Exception("Lambda has unexpected method value 9");
            }

            try {
                Expr.Lambda(Expr.Call(Expr.Lambda(typeof(Action), Expr.Throw(Expr.Constant(new Exception())), false, (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)).Compile().DynamicInvoke();
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo(DefaultName) != 0)
                    throw new Exception("Lambda has unexpected method value 10");
            }

            try {
                Expr.Lambda(Expr.Call(Expr.Lambda(typeof(Action), Expr.Throw(Expr.Constant(new Exception())), "", (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)).Compile().DynamicInvoke();
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo("") != 0)
                    throw new Exception("Lambda has unexpected method value 11");
            }

            try {
                Expr.Lambda(Expr.Call(Expr.Lambda(typeof(Action), Expr.Throw(Expr.Constant(new Exception())), "$%^!@#&*()_+\\|]}[{;:'\"<>,./?", true, (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)).Compile().DynamicInvoke();
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo("$%^!@#&*()_+\\|]}[{;:'\"<>,./?") != 0)
                    throw new Exception("Lambda has unexpected method value 12");
            }

            return Expr.Empty(); ;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Disabled, "Lambda 105", new string[] { "positive", "Lambda", "miscellaneous", "Pri1" })]
        public static Expr Lambda105(EU.IValidator V) {
            String StartDefaultName = "<ExpressionCompilerImplementationDetails>{";
            String EndDefaultName = "}lambda_method";
            var Exception = Expr.New(typeof(Exception).GetConstructor(new Type[] { typeof(String) }), Expr.Constant("Hello!"));
            try {
                EU.CompileAsMethodAndRun(Expr.Lambda(Expr.Call(Expr.Lambda(Expr.Throw(Exception), new ParameterExpression[] { }), "Invoke", null)));
            } catch (Exception ex) {
                if (!(ex.InnerException.TargetSite.Name.StartsWith(StartDefaultName) && ex.InnerException.TargetSite.Name.EndsWith(EndDefaultName)))
                    throw new Exception("Lambda has unexpected method value 1");
            }


            try {
                EU.CompileAsMethodAndRun(Expr.Lambda(Expr.Call(Expr.Lambda(Expr.Throw(Exception), (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)));
            } catch (Exception ex) {
                if (!(ex.InnerException.TargetSite.Name.StartsWith(StartDefaultName) && ex.InnerException.TargetSite.Name.EndsWith(EndDefaultName)))
                    throw new Exception("Lambda has unexpected method value 2");
            }

            try {
                EU.CompileAsMethodAndRun(Expr.Lambda(Expr.Call(Expr.Lambda(Expr.Throw(Exception), true, new ParameterExpression[] { }), "Invoke", null)));
            } catch (Exception ex) {
                if (!(ex.InnerException.TargetSite.Name.StartsWith(StartDefaultName) && ex.InnerException.TargetSite.Name.EndsWith(EndDefaultName)))
                    throw new Exception("Lambda has unexpected method value 3");
            }

            try {
                EU.CompileAsMethodAndRun(Expr.Lambda(Expr.Call(Expr.Lambda(Expr.Throw(Exception), true, (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)));
            } catch (Exception ex) {
                if (!(ex.InnerException.TargetSite.Name.StartsWith(StartDefaultName) && ex.InnerException.TargetSite.Name.EndsWith(EndDefaultName)))
                    throw new Exception("Lambda has unexpected method value 4");
            }

            try {
                EU.CompileAsMethodAndRun(Expr.Lambda(Expr.Call(Expr.Lambda(Expr.Throw(Exception), "123", (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)));
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo("123") != 0)
                    throw new Exception("Lambda has unexpected method value 5");
            }

            try {
                EU.CompileAsMethodAndRun(Expr.Lambda(Expr.Call(Expr.Lambda(typeof(Action), Expr.Throw(Exception), new ParameterExpression[] { }), "Invoke", null)));
            } catch (Exception ex) {
                if (!(ex.InnerException.TargetSite.Name.StartsWith(StartDefaultName) && ex.InnerException.TargetSite.Name.EndsWith(EndDefaultName)))
                    throw new Exception("Lambda has unexpected method value 6");
            }

            try {
                EU.CompileAsMethodAndRun(Expr.Lambda(Expr.Call(Expr.Lambda(typeof(Action), Expr.Throw(Exception), (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)));
            } catch (Exception ex) {
                if (!(ex.InnerException.TargetSite.Name.StartsWith(StartDefaultName) && ex.InnerException.TargetSite.Name.EndsWith(EndDefaultName)))
                    throw new Exception("Lambda has unexpected method value 7");
            }

            try {
                EU.CompileAsMethodAndRun(Expr.Lambda(Expr.Call(Expr.Lambda(Expr.Throw(Exception), "456", true, (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)));
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo("456") != 0)
                    throw new Exception("Lambda has unexpected method value 8");
            }

            try {
                EU.CompileAsMethodAndRun(Expr.Lambda(Expr.Call(Expr.Lambda(typeof(Action), Expr.Throw(Exception), true, new ParameterExpression[] { }), "Invoke", null)));
            } catch (Exception ex) {
                if (!(ex.InnerException.TargetSite.Name.StartsWith(StartDefaultName) && ex.InnerException.TargetSite.Name.EndsWith(EndDefaultName)))
                    throw new Exception("Lambda has unexpected method value 9");
            }

            try {
                EU.CompileAsMethodAndRun(Expr.Lambda(Expr.Call(Expr.Lambda(typeof(Action), Expr.Throw(Exception), false, (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)));
            } catch (Exception ex) {
                if (!(ex.InnerException.TargetSite.Name.StartsWith(StartDefaultName) && ex.InnerException.TargetSite.Name.EndsWith(EndDefaultName)))
                    throw new Exception("Lambda has unexpected method value 10");
            }

            try {
                EU.CompileAsMethodAndRun(Expr.Lambda(Expr.Call(Expr.Lambda(typeof(Action), Expr.Throw(Exception), "", (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)));
            } catch (Exception ex) {
                if (!(ex.InnerException.TargetSite.Name.StartsWith(StartDefaultName) && ex.InnerException.TargetSite.Name.EndsWith(EndDefaultName)))
                    throw new Exception("Lambda has unexpected method value 11");
            }

            try {
                EU.CompileAsMethodAndRun(Expr.Lambda(Expr.Call(Expr.Lambda(typeof(Action), Expr.Throw(Exception), "$%^!@#&*()_+\\|]}[{;:'\"<>,./?", true, (IEnumerable<ParameterExpression>)new ParameterExpression[] { }), "Invoke", null)));
            } catch (Exception ex) {
                if (ex.InnerException.TargetSite.Name.CompareTo("$%^!@#&*()_+\\|]}[{;:'\"<>,./?") != 0)
                    throw new Exception("Lambda has unexpected method value 12");
            }

            return Expr.Empty();
        }
#endif

        private static int _NonVoidReturningLambda_Test_Value = 0;
        public static int Do() { return (_NonVoidReturningLambda_Test_Value++); }

#if !CLR2 // TODO: inline expression trees
        // Regression test fro @bug 463188
        // Test:    Verify that non void returning lambda does not "destabilize the runtime"
        // Expected: Should work.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "NonVoidReturningLambda", new string[] { "positive", "Lambda", "miscellaneous", "Pri1" }, Priority = 1)]
        public static Expr NonVoidReturningLambda(EU.IValidator V) {
            _NonVoidReturningLambda_Test_Value = 0;
            Expression<Action> compiled = () => new Action(() => Do())();
            compiled.Compile()();

            // Make sure there is no exception or assertion thrown and the method is called correctly.
            var tree = EU.GenAreEqual(Expr.Constant(1), Expr.Constant(_NonVoidReturningLambda_Test_Value, typeof(int)));
            V.Validate(tree);
            return tree;
        }

//Doesn't compile on Silverlight 4.  Should it?  cannot convert from 'System.Action<object>' to 'System.Action<string>'
#if !SILVERLIGHT
        // Regression for Dev10 814111
        // Testing contravariance is carried through ETs (won't compile pre 4.0)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Lambda 106", new string[] { "positive", "Lambda", "miscellaneous", "Pri1" }, Priority = 1)]
        public static Expr Lambda106(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var action = (Action<object>)Console.WriteLine;
            var a = Expression.Parameter(typeof(Action<string>), "arg");
            var b = Expression.TypeIs(a, typeof(Action<object>));

            var lm = Expression.Lambda<Func<Action<string>, bool>>(b, a);
            var res = Expr.Constant(lm.Compile()(action));

            var tree = Expr.Block(EU.GenAreEqual(res, Expr.Constant(true)));
            V.Validate(tree);
            return tree;
        }
#endif
#endif
    }
}


