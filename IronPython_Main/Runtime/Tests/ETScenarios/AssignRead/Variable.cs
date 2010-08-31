#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.AssignRead {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Variable {
        // Pass null to type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Variable 1", new string[] { "negative", "variable", "assignread", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Variable1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Type t = null;
            var Result = EU.Throws<ArgumentException>(() => { Expr.Variable(t, "Result"); });

            return Expr.Empty();
        }

        // Pass null to name
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Variable 2", new string[] { "positive", "variable", "assignread", "Pri2" })]
        public static Expr Variable2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(Int32), null);
            Expressions.Add(Expr.Assign(Result, Expr.Constant(2)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Result, "Variable 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use all types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Variable 3", new string[] { "positive", "variable", "assignread", "Pri2" })]
        public static Expr Variable3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Type[] nonNullableTypes = { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(Int16), typeof(UInt16), typeof(Int32), typeof(UInt32), typeof(long), typeof(ulong), typeof(decimal), typeof(Single), 
                             typeof(double), typeof(string), typeof(char) };

            Type[] nullableTypes = { typeof(Nullable<byte>), typeof(Nullable<sbyte>), typeof(Nullable<short>), typeof(Nullable<ushort>), typeof(Nullable<Int16>), typeof(Nullable<UInt16>),
                             typeof(Nullable<Int32>), typeof(Nullable<UInt32>), typeof(Nullable<long>), typeof(Nullable<ulong>), typeof(Nullable<decimal>), typeof(Nullable<Single>), typeof(Nullable<double>),
                             typeof(Nullable<char>) };

            for (int i = 0; i < nonNullableTypes.Length; i++) {
                Type t = nonNullableTypes[i];
                var x = Expr.Variable(t, t.GetType().ToString() + "Var" + i);
                Expr Res =
                    Expr.Block(
                        new[] { x },
#if SILVERLIGHT
                        Expr.Assign(x, Expr.Constant(System.Convert.ChangeType(2, t, null), t)),
                        EU.GenAreEqual(Expr.Constant(System.Convert.ChangeType(2, t, null), t), x, "Variable " + i)
#else
                        Expr.Assign(x, Expr.Constant(System.Convert.ChangeType(2, t), t)),
                        EU.GenAreEqual(Expr.Constant(System.Convert.ChangeType(2, t), t), x, "Variable " + i)
#endif
                    );
                Expressions.Add(Res);
            }

            for (int i = 0; i < nullableTypes.Length; i++) {
                Type t = nullableTypes[i];
                var x = Expr.Variable(t, t.GetType().ToString() + "Var" + (i + nonNullableTypes.Length));
                int? val = 2;

                LambdaExpression LM = Expr.Lambda(Expr.Convert(Expr.Constant(val), t));
                Object obj1 = LM.Compile().DynamicInvoke();

                Expr Res =
                    Expr.Block(
                        new[] { x },
                        Expr.Assign(x, Expr.Constant(obj1, t)),
                        EU.GenAreEqual(Expr.Constant(obj1, t), x, "Variable " + (i + nonNullableTypes.Length))
                    );
                Expressions.Add(Res);
            }

            ParameterExpression DateTimeVar = Expr.Variable(typeof(DateTime), "DateTimeVar");
            Expressions.Add(Expr.Assign(DateTimeVar, Expr.Constant(DateTime.Parse("1/1/2009"))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(DateTime.Parse("1/1/2009")), DateTimeVar, "Variable 0"));

            ParameterExpression DateTimeVar2 = Expr.Variable(typeof(DateTime?), "DateTimeVar2");
            Expressions.Add(Expr.Assign(DateTimeVar2, Expr.Constant(DateTime.Parse("1/1/2009"), typeof(DateTime?))));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(DateTime.Parse("1/1/2009"), typeof(DateTime?)), DateTimeVar2, "Variable 0"));

            var tree = Expr.Block(new[] { DateTimeVar, DateTimeVar2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass an open generic type to type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Variable 4", new string[] { "negative", "variable", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Variable4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var Result = Expr.Variable(typeof(List<>), "Result");
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Result, Expr.Constant(new List<int> { 1, 2, 3 }));
            }));

            return Expr.Empty();
        }

        // Try to create a variable of a ref type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Variable 5", new string[] { "negative", "variable", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Variable5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var Result = EU.Throws<ArgumentException>(() => { Expr.Variable(typeof(int).MakeByRefType(), "Result"); });

            return Expr.Empty();
        }

        // Regression for bug 575117
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Variable 6", new string[] { "positive", "variable", "assignread", "Pri1" })]
        public static Expr Variable6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var x = Expression.Variable(typeof(int), null);

            var y = Expression.Variable(typeof(int), null);

            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Expr.Invoke(Expression.Lambda<Func<int>>(
                Expression.Block(
                Expression.Block(new[] { x }, Expression.Assign(x, Expression.Constant(123))),
                Expression.Block(new[] { y }, y))))));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Regression for bug 639001
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Variable 7", new string[] { "positive", "variable", "assignread", "Pri1" })]
        public static Expr Variable7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var x = Expression.Variable(typeof(int));

            var y = Expression.Parameter(typeof(int));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Expr.Invoke(Expression.Lambda<Func<int>>(
                Expression.Block(
                Expression.Block(new[] { x }, Expression.Assign(x, Expression.Constant(123))),
                Expression.Block(new[] { y }, y))))));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use DBNull typed ParameterExpression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Variable 8", new string[] { "positive", "variable", "assignread", "Pri2" }, Priority = 2)]
        public static Expr Variable8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(DBNull), "Result");

            Expr Val =
                Expr.Condition(
                    Expr.Equal(Result, Expr.Constant(null)),
                    Expr.Constant(true),
                    Expr.Constant(false)
                );

            Expressions.Add(EU.GenAreEqual(Expr.Constant(true), Val, "Parameter Expression 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
