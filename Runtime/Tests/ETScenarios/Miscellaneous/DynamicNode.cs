#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class DynamicNode {

        public class MyBinder : CallSiteBinder {
            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                string Result = "";
                foreach (var arg in args) {
                    Result += arg.ToString();
                }

                return Expr.Return(returnLabel, Expr.Constant("Success" + Result));
            }
        }

        // Pass a self implemented binder, one through four arguments, a valid return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DynamicNode 1", new string[] { "positive", "dynamicnode", "miscellaneous", "Pri1" })]
        public static Expr DynamicNode1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            CallSiteBinder myBinder = new MyBinder();

            // for comparing ToString'd results of doubles on non ENU languages
            var d = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            Expressions.Add(Expr.Assign(Result, Expr.Dynamic(myBinder, typeof(string), Expr.Constant(1))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Success1"), Result, "DynamicNode 1"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Result, Expr.Dynamic(myBinder, typeof(string), Expr.Constant(1), Expr.Constant("Test"))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Success1Test"), Result, "DynamicNode 2"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Result, Expr.Dynamic(myBinder, typeof(string), Expr.Constant(1), Expr.Constant("Test"), Expr.Constant(2.1))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Success1Test2" + d + "1"), Result, "DynamicNode 3"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Result, Expr.Dynamic(myBinder, typeof(string), Expr.Constant(1), Expr.Constant("Test"), Expr.Constant(2.1), Expr.Constant(3))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Success1Test2" + d + "13"), Result, "DynamicNode 4"));

            // different overload
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Result, Expr.Dynamic(myBinder, typeof(string), new Expression[] { Expr.Constant(1) })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Success1"), Result, "DynamicNode 5"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Result, Expr.Dynamic(myBinder, typeof(string), new Expression[] { Expr.Constant(1), Expr.Constant("Test") })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Success1Test"), Result, "DynamicNode 6"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Result, Expr.Dynamic(myBinder, typeof(string), new Expression[] { Expr.Constant(1), Expr.Constant("Test"), Expr.Constant(2.1) })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Success1Test2" + d + "1"), Result, "DynamicNode 7"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Result, Expr.Dynamic(myBinder, typeof(string), new Expression[] { Expr.Constant(1), Expr.Constant("Test"), Expr.Constant(2.1), Expr.Constant(3) })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Success1Test2" + d + "13"), Result, "DynamicNode 8"));

            // different overload
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Result, Expr.Dynamic(myBinder, typeof(string), new List<Expression>() { Expr.Constant(1) })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Success1"), Result, "DynamicNode 9"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Result, Expr.Dynamic(myBinder, typeof(string), new List<Expression>() { Expr.Constant(1), Expr.Constant("Test") })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Success1Test"), Result, "DynamicNode 10"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Result, Expr.Dynamic(myBinder, typeof(string), new List<Expression>() { Expr.Constant(1), Expr.Constant("Test"), Expr.Constant(2.1) })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Success1Test2" + d + "1"), Result, "DynamicNode 11"));

            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));
            Expressions.Add(Expr.Assign(Result, Expr.Dynamic(myBinder, typeof(string), new List<Expression>() { Expr.Constant(1), Expr.Constant("Test"), Expr.Constant(2.1), Expr.Constant(3) })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("Success1Test2" + d + "13"), Result, "DynamicNode 12"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class MyVoidBinder : CallSiteBinder {
            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                if (args[0].GetType() != typeof(Int32))
                    throw new NotSupportedException();
                return Expr.Return(returnLabel);
            }
        }

        // Pass System.void to return type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DynamicNode 2", new string[] { "positive", "dynamicnode", "miscellaneous", "Pri1" })]
        public static Expr DynamicNode2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(string), "Result");
            CallSiteBinder myBinder = new MyVoidBinder();

            Expressions.Add(
                Expr.TryCatch(
                    EU.BlockVoid(Expr.Dynamic(myBinder, typeof(void), Expr.Constant(2.0))),
                    Expr.Catch(typeof(NotSupportedException), EU.BlockVoid(EU.ConcatEquals(Result, Expr.Constant("NonInt"))))
                )
            );
            Expressions.Add(EU.GenAreEqual(Expr.Constant("NonInt"), Result, "DynamicNode 1"));

            // different overload
            Expressions.Add(Expr.Assign(Result, Expr.Constant("")));

            Expressions.Add(
                Expr.TryCatch(
                    EU.BlockVoid(Expr.Dynamic(myBinder, typeof(void), new Expression[] { Expr.Constant(2.0) })),
                    Expr.Catch(typeof(NotSupportedException), EU.BlockVoid(EU.ConcatEquals(Result, Expr.Constant("NonInt"))))
                )
            );
            Expressions.Add(EU.GenAreEqual(Expr.Constant("NonInt"), Result, "DynamicNode 2"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        class testDynamic : CallSiteBinder {
            public override int GetHashCode() {
                return 1;
            }

            public override bool Equals(object obj) {
                return obj != null && obj.GetHashCode() == 1;
            }

            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                return null;
            }
        }

        // Test      : Pass null to binder
        // Expected  : Exception
        // Notes     : This appears to have been a miscellaneous dynamic node test.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DynamicNode 3", new string[] { "positive", "dynamicnode", "miscellaneous", "Pri1" })]
        public static Expr DynamicNode3(EU.IValidator V) {

            try {
                Expr.Dynamic(null, typeof(int));
                throw new Exception("No exception thrown.");
            } catch (ArgumentNullException) {
            } catch (ArgumentException) {
            } catch (Exception ex) {
                throw new Exception("1 Incorrect exception thrown: " + ex.GetType().Name + Environment.NewLine + ex.Message);
            }

            try {
                Expr.Dynamic(null, typeof(int), Expr.Empty());
                throw new Exception("No exception thrown.");
            } catch (ArgumentNullException) {
            } catch (ArgumentException) {
            } catch (Exception ex) {
                throw new Exception("2 Incorrect exception thrown: " + ex.GetType().Name + Environment.NewLine + ex.Message);
            }

            try {
                Expr.Dynamic(null, typeof(int), new Expr[] { null });
                throw new Exception("No exception thrown.");
            } catch (ArgumentNullException) {
            } catch (ArgumentException) {
            } catch (Exception ex) {
                throw new Exception("3 Incorrect exception thrown: " + ex.GetType().Name + Environment.NewLine + ex.Message);
            }

            try {
                Expr.Dynamic(new testDynamic(), null);
                throw new Exception("No exception thrown.");
            } catch (ArgumentNullException) {
            } catch (ArgumentException) {
            } catch (Exception ex) {
                throw new Exception("4 Incorrect exception thrown: " + ex.GetType().Name + Environment.NewLine + ex.Message);
            }

            try {
                Expr.Dynamic(new testDynamic(), typeof(int), new Expr[] { null });
                throw new Exception("No exception thrown.");
            } catch (ArgumentNullException) {
            } catch (ArgumentException) {
            } catch (Exception ex) {
                throw new Exception("6 Incorrect exception thrown: " + ex.GetType().Name + Environment.NewLine + ex.Message);
            }

            return Expr.Empty();
        }
    }
}
