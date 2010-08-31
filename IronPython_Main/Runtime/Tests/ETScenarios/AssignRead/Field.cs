#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.AssignRead {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Field {
        public class TestClass {
            public int _x;
            public int _y;
            public int SomeProperty { get; set; }
            private string _privateField;
            internal double _internalField;
            public readonly int _ro;
            public TestClass(int val, string p, double i) {
                _x = val;
                _ro = val;
                _privateField = p;
                _internalField = i;
            }
            public TestClass(int val) {
                _x = val;
                _ro = val;
            }
            public static int staticField;

        }
        // Readonly field, attempt to assign into it. (create the expression in a constructor for the type that has the readonly field)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 1", new string[] { "negative", "field", "assignread", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Field1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");
            Expressions.Add(Expr.Assign(TestObj, Expr.Constant(new TestClass(1))));

            FieldInfo fi = typeof(TestClass).GetField("_ro", BindingFlags.Instance | BindingFlags.Public);
            Expr field = Expr.Field(TestObj, typeof(TestClass), "_ro");

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), field, "Field 1"));

            Expr OtherTestObj =
                EU.Throws<System.ArgumentException>(() =>
                {
                    Expr.New(
                        typeof(TestClass).GetConstructor(new Type[] { typeof(int) }),
                        Expr.Block(
                            Expr.Assign(Expr.Field(TestObj, fi), Expr.Constant(3)),
                            Expr.Constant(2)
                        )
                    );
                });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), OtherTestObj, "Field 2"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), field, "Field 3"));

            return Expr.Empty();
        }


        // Access a readonly field (assign)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 2", new string[] { "negative", "field", "assignread", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Field2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            FieldInfo fi = typeof(TestClass).GetField("_ro", BindingFlags.Instance | BindingFlags.Public);
            Expr field = Expr.Field(TestObj, typeof(TestClass), "_ro");

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), field, "Field 1"));
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Field(TestObj, fi), Expr.Constant(2));
            }));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Field(TestObj, fi), "Field 2"));

            return Expr.Empty();
        }

        // Assign to field using this factory
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 3", new string[] { "positive", "field", "assignread", "Pri2" })]
        public static Expr Field3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            FieldInfo fi = typeof(TestClass).GetField("_x");
            Expressions.Add(Expr.Assign(Expr.Field(TestObj, fi), Expr.Constant(2)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Field(TestObj, fi), "Field 1"));

            var tree = Expr.Block(new[] { TestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class OtherTestClass {
            public int _x;
            public OtherTestClass(int val) { _x = val; }
        }

        // Use a methodinfo from a different type than the expression (same name for field though)
        // Regression for Dev10 Bug 552888 
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 4", new string[] { "negative", "field", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Field4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            FieldInfo fi = typeof(OtherTestClass).GetField("_x");
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.Assign(Expr.Field(TestObj, fi), Expr.Constant(2)));
            });

            return Expr.Empty();
        }

        // Assign to field using this factory
        // Read from a field using this factory
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 5", new string[] { "positive", "field", "assignread", "Pri2" })]
        public static Expr Field5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            Expressions.Add(Expr.Assign(Expr.Field(TestObj, "_x"), Expr.Constant(2)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Field(TestObj, "_x"), "Field 1"));

            var tree = Expr.Block(new[] { TestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // empty ("") string
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 6", new string[] { "negative", "field", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Field6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Field(TestObj, ""), Expr.Constant(2));
            }));

            return Expr.Empty();
        }

        // miscased string
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 7", new string[] { "positive", "field", "assignread", "Pri2" })]
        public static Expr Field7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            Expressions.Add(Expr.Assign(Expr.Field(TestObj, "_X"), Expr.Constant(2)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Field(TestObj, "_X"), "Field 1"));

            var tree = Expr.Block(new[] { TestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // name corresponds to property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 8", new string[] { "negative", "field", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Field8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Field(TestObj, "SomeProperty"), Expr.Constant(2));
            }));

            return Expr.Empty();
        }

        // Assign to field using this factory
        // Read from a field using this factory
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 9", new string[] { "positive", "field", "assignread", "Pri2" })]
        public static Expr Field9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            Expressions.Add(Expr.Assign(Expr.Field(TestObj, typeof(TestClass), "_x"), Expr.Constant(2)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Field(TestObj, typeof(TestClass), "_x"), "Field 1"));

            var tree = Expr.Block(new[] { TestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // empty ("") string
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 10", new string[] { "negative", "field", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Field10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Field(TestObj, typeof(TestClass), ""), Expr.Constant(2));
            }));

            return Expr.Empty();
        }

        // miscased string
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 11", new string[] { "positive", "field", "assignread", "Pri2" })]
        public static Expr Field11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            Expressions.Add(Expr.Assign(Expr.Field(TestObj, typeof(TestClass), "_X"), Expr.Constant(2)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Field(TestObj, typeof(TestClass), "_X"), "Field 1"));

            var tree = Expr.Block(new[] { TestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // name corresponds to property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 12", new string[] { "negative", "field", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Field12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Field(TestObj, typeof(TestClass), "SomeProperty"), Expr.Constant(2));
            }));

            return Expr.Empty();
        }

        // Use a type of a different type than the expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 13", new string[] { "negative", "field", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Field13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Field(TestObj, typeof(int), "SomeProperty"), Expr.Constant(2));
            }));

            return Expr.Empty();
        }

        public class DerivedTestClass : TestClass {
            new public int _y;
            public int DerivedProp { get; set; }
            public DerivedTestClass(int val) : base(val) { DerivedProp = val; _y = val; }
        }

        // Use a derived type from the expression for type, use fieldname for a base field
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 14", new string[] { "positive", "field", "assignread", "Pri2" })]
        public static Expr Field14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(DerivedTestClass), "TestObj");

            ConstructorInfo ci = typeof(DerivedTestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            Expressions.Add(Expr.Assign(Expr.Field(TestObj, typeof(TestClass), "_x"), Expr.Constant(2)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(2), Expr.Field(TestObj, typeof(TestClass), "_x"), "Field 1"));

            var tree = Expr.Block(new[] { TestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class GenericClass<T> {
            public T _x;
            public GenericClass(T val) { _x = val; }
        }

        // Pass an open generic type to type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 15", new string[] { "negative", "field", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Field15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(GenericClass<int>), "TestObj");

            ConstructorInfo ci = typeof(GenericClass<int>).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Field(TestObj, typeof(GenericClass<>), "_x"), Expr.Constant(2));
            }));

            return Expr.Empty();
        }

        // Access a non public field (private)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 16", new string[] { "positive", "field", "assignread", "Pri2" })]
        public static Expr Field16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int), typeof(string), typeof(double) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1), Expr.Constant("Test"), Expr.Constant(2.1) })));

            Expressions.Add(Expr.Assign(Expr.Field(TestObj, typeof(TestClass), "_privateField"), Expr.Constant("New")));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("New"), Expr.Field(TestObj, typeof(TestClass), "_privateField"), "Field 1"));

            var tree = Expr.Block(new[] { TestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Access a non public field (internal)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 17", new string[] { "positive", "field", "assignread", "Pri2" })]
        public static Expr Field17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int), typeof(string), typeof(double) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(1), Expr.Constant("Test"), Expr.Constant(2.1) })));

            Expressions.Add(Expr.Assign(Expr.Field(TestObj, typeof(TestClass), "_internalField"), Expr.Constant(3.1)));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3.1), Expr.Field(TestObj, typeof(TestClass), "_internalField"), "Field 1"));

            var tree = Expr.Block(new[] { TestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Access a non public field from a platform assembly
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 22", new string[] { "negative", "field", "assignread", "Pri2", "PartialTrustOnly" }, Exception = typeof(FieldAccessException))]
        public static Expr Field22(EU.IValidator V) {
            var tree = Expr.Field(Expr.Constant(new List<int>()), typeof(List<int>).GetField("_size", BindingFlags.NonPublic | BindingFlags.Instance));
            if (Environment.Version.Major >= 4)
                V.ValidateException<FieldAccessException>(tree, false);
            else
                V.ValidateException<FieldAccessException>(tree, true);

            return tree;
        }

        // Readonly field, read from it
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 18", new string[] { "positive", "field", "assignread", "Pri2" })]
        public static Expr Field18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(3) })));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Field(TestObj, typeof(TestClass), "_ro"), "Field 1"));

            var tree = Expr.Block(new[] { TestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Attempt to access a field that is hidden by a field on a derived class
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 19", new string[] { "positive", "field", "assignread", "Pri2" })]
        public static Expr Field19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(3) })));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(0), Expr.Field(TestObj, typeof(TestClass), "_y"), "Field 1"));

            ParameterExpression OtherTestObj = Expr.Variable(typeof(DerivedTestClass), "OtherTestObj");
            ConstructorInfo ci2 = typeof(DerivedTestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(OtherTestObj, Expr.New(ci2, new Expression[] { Expr.Constant(4) })));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(4), Expr.Field(OtherTestObj, typeof(DerivedTestClass), "_y"), "Field 2"));

            var tree = Expr.Block(new[] { TestObj, OtherTestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Attempt to access a static field
        // Regression for Dev10 Bug 555936
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 20", new string[] { "negative", "field", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Field20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");

            ParameterExpression TestObj = Expr.Variable(typeof(TestClass), "TestObj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci, new Expression[] { Expr.Constant(3) })));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Result, Expr.Field(TestObj, "staticField"));
            }));

            return Expr.Empty();
        }

        // Attempt to access a static field
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Field 21", new string[] { "positive", "field", "assignread", "Pri2" })]
        public static Expr Field21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Result = Expr.Variable(typeof(int), "Result");
            Expressions.Add(Expr.Assign(Expr.Field(null, typeof(TestClass), "staticField"), Expr.Constant(1)));
            Expressions.Add(Expr.Assign(Result, Expr.Field(null, typeof(TestClass), "staticField")));
            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Result, "Field 1"));

            var tree = Expr.Block(new[] { Result }, Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
