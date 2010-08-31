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
        
    public class Property {
        private static Expr TE(Type t, string Message) {
            ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(String) });
            Expression Ex = Expr.New(ci, Expr.Constant(Message));
            return Ex;
        }

        public class GenericValue<T> {
            public T Value { get; set; }
            public static bool operator ==(GenericValue<T> x, GenericValue<T> y) {
                return x.Value.Equals(y.Value);
            }
            public static bool operator !=(GenericValue<T> x, GenericValue<T> y) {
                return x.Value.Equals(y.Value);
            }
            public GenericValue(T x) {
                Value = x;
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return 1;
            }
        }

        public class TestClass {
            public string result;
            public TestClass() {
                result += "new TestClass()";
            }
            public virtual int Foo {
                get {
                    result += "get_Foo";
                    return 123;
                }
                set {
                    result += "set_Foo(" + Convert.ToString(value) + ")";
                }
            }

            public virtual GenericValue<int> Val {
                get {
                    result += "get_GenericValue";
                    return new GenericValue<int>(3);
                }
                set {
                    result += "set_GenericValue";
                }
            }

            public int this[int index] {
                get {
                    result += "get_index";
                    return index;
                }
                set {
                    result += "set_index(" + System.Convert.ToString(value) + ")";
                }
            }

            public int TestMethod() { return 1; }
            public int ReadOnlyProp { get { return 1; } }
            public int WriteOnlyProp { set { result += "WriteOnlyProp(" + System.Convert.ToString(value) + ")"; } }
            public int VisibilityProp {
                get {
                    return 1;
                }
                private set {
                    result += "VisibilityProp(" + System.Convert.ToString(value) + ")";
                }
            }
            public static string StaticProp { get; set; }
            private int PrivateProp { get; set; }
        }

        public void TestFunc(Action f) {
            f();
        }

        public void TestExpr(Expression<Func<int>> e) {
            e.Compile()();
        }

        static public int TestByRef(ref int i) {
            return i = i + 1;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 1", new string[] { "positive", "property", "assignread", "Pri1" })]
        public static Expr Property1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            ParameterExpression Result = Expr.Variable(typeof(string), "");

            ParameterExpression cl = Expr.Variable(typeof(TestClass), "");

            Expressions.Add(Expr.Assign(cl, Expr.New(typeof(TestClass))));

            MemberExpression Foo = Expr.Property(cl, typeof(TestClass).GetProperty("Foo"));
            Expressions.Add(Expr.Call(typeof(Property).GetMethod("TestByRef"), Foo));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()get_Fooset_Foo(124)"), Expr.Field(cl, typeof(TestClass).GetField("result")), "Property 1"));

            var tree = Expr.Block(new[] { cl }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Call a parameterized property with this factory
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 2", new string[] { "positive", "property", "assignread", "Pri1" })]
        public static Expr Property2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ParameterExpression PropResult = Expr.Variable(typeof(GenericValue<int>), "PropResult");

            Expressions.Add(Expr.Assign(Obj, Expr.Constant(new TestClass())));

            PropertyInfo pi = typeof(TestClass).GetProperty("Val");
            Expressions.Add(Expr.Assign(PropResult, Expr.Property(Obj, pi, new List<Expr>() { })));

            Expr Result = Expr.Constant(new GenericValue<int>(3));
            Expressions.Add(EU.GenAreEqual(Result, PropResult, "Property 1"));

            var tree = Expr.Block(new[] { Obj, PropResult }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass an incorrect number of parameters
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 3", new string[] { "negative", "property", "assignread", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Property3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");

            Expressions.Add(Expr.Assign(Obj, Expr.Constant(new TestClass())));

            PropertyInfo pi = typeof(TestClass).GetProperty("Foo");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Property(Obj, pi, new List<Expr>() { Expr.Constant(1) });
            }));

            return Expr.Empty();
        }

        // Pass parameters of incorrect types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 4", new string[] { "negative", "property", "assignread", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Property4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");

            Expressions.Add(Expr.Assign(Obj, Expr.Constant(new TestClass())));

            PropertyInfo pi = typeof(TestClass).GetProperty("Item");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Property(Obj, pi, new List<Expression>() { Expr.Constant("1") });
            }));

            return Expr.Empty();
        }

        // Call a parameterized property with this factory
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 5", new string[] { "positive", "property", "assignread", "Pri1" })]
        public static Expr Property5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ParameterExpression PropResult = Expr.Variable(typeof(GenericValue<int>), "PropResult");

            Expressions.Add(Expr.Assign(Obj, Expr.Constant(new TestClass())));

            PropertyInfo pi = typeof(TestClass).GetProperty("Val");
            Expressions.Add(Expr.Assign(PropResult, Expr.Property(Obj, pi, new Expression[] { })));

            Expr Result = Expr.Constant(new GenericValue<int>(3));
            Expressions.Add(EU.GenAreEqual(Result, PropResult, "Property 1"));

            var tree = Expr.Block(new[] { Obj, PropResult }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass an incorrect number of parameters
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 6", new string[] { "negative", "property", "assignread", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Property6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("Item");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Property(Obj, pi, new Expression[] { Expr.Constant(1), Expr.Constant(2) });
            }));

            return Expr.Empty();
        }

        // Pass parameters of incorrect types
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 7", new string[] { "negative", "property", "assignread", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Property7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("Item");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Property(Obj, pi, new Expression[] { Expr.Constant("1") });
            }));

            return Expr.Empty();
        }

        // Pass a method info that isn't a property accessor
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 8", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            Expressions.Add(Expr.Assign(Obj, Expr.Constant(new TestClass())));

            MethodInfo mi = typeof(TestClass).GetMethod("TestMethod");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(1), Expr.Property(Obj, mi), "Property 1");
            }));

            return Expr.Empty();
        }

        // Pass null to the propertyAccessor
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 9", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Property9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            Expressions.Add(Expr.Assign(Obj, Expr.Constant(new TestClass())));

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(1), Expr.Property(Obj, (MethodInfo)null), "Property 1");
            }));

            return Expr.Empty();
        }

        // Assign to read only property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 10", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            MethodInfo mi = typeof(TestClass).GetMethod("get_ReadOnlyProp");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, mi), Expr.Constant(3));
            }));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Property(Obj, mi), "Property 1"));

            return Expr.Empty();
        }

        // Read from write only property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 11", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            MethodInfo mi = typeof(TestClass).GetMethod("set_WriteOnlyProp");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(3), Expr.Property(Obj, mi), "Property 1");
            }));

            return Expr.Empty();
        }

        // Different visibility for getter and setter, assign and read
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 12", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Property12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            MethodInfo gmi = typeof(TestClass).GetMethod("get_VisibilityProp");
            MethodInfo smi = typeof(TestClass).GetMethod("set_VisibilityProp");

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Expr.Property(Obj, gmi), "Property 1"));
            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, smi), Expr.Constant(4));
            }));

            FieldInfo fi = typeof(TestClass).GetField("result");
            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()VisibilityProp(4)"), Expr.Field(Obj, fi), "Property 2"));

            return Expr.Block(new[] { Obj }, Expressions);
        }

        // Pass null to property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 13", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Property13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            Expressions.Add(Expr.Assign(Obj, Expr.Constant(new TestClass())));

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(1), Expr.Property(Obj, (PropertyInfo)null), "Property 1");
            }));

            return Expr.Empty();
        }

        // Read the property value using this factory 
        // Assign to the property using this factory
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 14", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("Foo");
            Expressions.Add(Expr.Assign(Expr.Property(Obj, pi), Expr.Constant(3)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(123), Expr.Property(Obj, pi), "Property 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()set_Foo(3)get_Foo"), Expr.Field(Obj, typeof(TestClass), "result"), "Property 2"));

            var tree = Expr.Block(new[] { Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass a non-public property info. Access works because of RestrictedMemberAccess
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 15", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("VisibilityProp");

            Expressions.Add(Expr.Assign(Expr.Property(Obj, pi), Expr.Constant(4)));

            FieldInfo fi = typeof(TestClass).GetField("result");
            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()VisibilityProp(4)"), Expr.Field(Obj, fi), "Property 2"));

            var tree = Expr.Block(new[] { Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

//Seems Exception doesn't have this property in Silverlight
#if !SILVERLIGHT
        // Pass an invalid propertyInfo (non accessible)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 55", new string[] { "negative", "property", "assignread", "Pri2", "PartialTrustOnly" }, Exception = typeof(MethodAccessException))]
        public static Expr Property55(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var tree = Expression.Property(
                Expression.Constant(new Exception()),
                typeof(Exception).GetProperty("IsTransient", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            );
            if (Environment.Version.Major >= 4)
                V.ValidateException<MethodAccessException>(tree, false);
            else
                V.ValidateException<MethodAccessException>(tree, true);
            return tree;
        }
#endif

        // Pass an invalid propertyInfo (from a different type)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 16", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(string).GetProperty("Length");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, pi), Expr.Constant(4));
            }));

            FieldInfo fi = typeof(TestClass).GetField("result");
            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()VisibilityProp(4)"), Expr.Field(Obj, fi), "Property 2"));

            return Expr.Empty();
        }

        // Assign to read only property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 17", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("ReadOnlyProp");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, pi), Expr.Constant(3));
            }));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Property(Obj, pi), "Property 1"));

            return Expr.Empty();
        }

        // Read from write only property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 18", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("WriteOnlyProp");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(3), Expr.Property(Obj, pi), "Property 1");
            }));

            return Expr.Empty();
        }

        // Different visibility for getter and setter, assign and read
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 19", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("VisibilityProp");

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Expr.Property(Obj, pi), "Property 1"));
            Expressions.Add(Expr.Assign(Expr.Property(Obj, pi), Expr.Constant(4)));

            FieldInfo fi = typeof(TestClass).GetField("result");
            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()VisibilityProp(4)"), Expr.Field(Obj, fi), "Property 2"));

            var tree = Expr.Block(new[] { Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 20", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Property20(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Assign(Expr.Property((Expression)null, "Foo"), Expr.Constant(3));
            }));

            return Expr.Block(new[] { Obj }, Expressions);
        }

        // Pass a static property to propertyName
        // Regression for Dev10 Bug 555936
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 21", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property21(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, "StaticProp"), Expr.Constant("Test"));
            }));

            return Expr.Empty();
        }

        // Pass an instance property to propertyName
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 22", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property22(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(Expr.Assign(Expr.Property(Obj, "Foo"), Expr.Constant(3)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(123), Expr.Property(Obj, "Foo"), "Property 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()set_Foo(3)get_Foo"), Expr.Field(Obj, typeof(TestClass), "result"), "Property 1"));

            var tree = Expr.Block(new[] { Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass an empty string to propertyName
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 23", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property23(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, ""), Expr.Constant(3));
            }));

            return Expr.Empty();
        }

        // Pass null to propertyName
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 24", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Property24(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(EU.Throws<System.ArgumentNullException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, (string)null), Expr.Constant(3));
            }));

            return Expr.Block(new[] { Obj }, Expressions);
        }

        // Pass a non existing propertyName
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 25", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property25(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, "NoProp"), Expr.Constant(3));
            }));

            return Expr.Empty();
        }

        // Pass a non accessible propertyName
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 26", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property26(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(Expr.Assign(Expr.Property(Obj, "PrivateProp"), Expr.Constant(3)));

            var tree = Expr.Block(new[] { Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Call a property with improper casing
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 27", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property27(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(Expr.Assign(Expr.Property(Obj, "foo"), Expr.Constant(3)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(123), Expr.Property(Obj, "foo"), "Property 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()set_Foo(3)get_Foo"), Expr.Field(Obj, typeof(TestClass), "result"), "Property 1"));

            var tree = Expr.Block(new[] { Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class DerivedTestClass : TestClass {
            public DerivedTestClass() {
                result += "new DerivedTestClass()";
            }

            public override int Foo {
                get {
                    result += "get_DerivedFoo";
                    return 123;
                }
                set {
                    result += "set_DerivedFoo(" + System.Convert.ToString(value) + ")";
                }
            }
            public new GenericValue<int> Val {
                get {
                    return new GenericValue<int>(6);
                }
                set {
                    result += "set_DerivedGenericValue";
                }
            }
        }

        // Call an overloaded property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 28", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property28(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(DerivedTestClass), "Obj");
            ConstructorInfo ci = typeof(DerivedTestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(Expr.Assign(Expr.Property(Obj, "Foo"), Expr.Constant(3)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(123), Expr.Property(Obj, "Foo"), "Property 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()new DerivedTestClass()set_DerivedFoo(3)get_DerivedFoo"), Expr.Field(Obj, typeof(TestClass), "result"), "Property 1"));

            var tree = Expr.Block(new[] { Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Assign to read only property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 29", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property29(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, "ReadOnlyProp"), Expr.Constant(3));
            }));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Property(Obj, "ReadOnlyProp"), "Property 1"));

            return Expr.Empty();
        }

        // Read from write only property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 30", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property30(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(3), Expr.Property(Obj, "WriteOnlyProp"), "Property 1");
            }));

            return Expr.Empty();
        }

        // Different visibility for getter and setter, assign and read
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 31", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property31(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(1), Expr.Property(Obj, "VisibilityProp"), "Property 1"));
            Expressions.Add(Expr.Assign(Expr.Property(Obj, "VisibilityProp"), Expr.Constant(4)));

            FieldInfo fi = typeof(TestClass).GetField("result");
            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()VisibilityProp(4)"), Expr.Field(Obj, fi), "Property 2"));

            var tree = Expr.Block(new[] { Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 32", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property32(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("Item");
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.Assign(Expr.Property(Obj, pi, (Expression[])null), Expr.Constant(3)));
            });

            return Expr.Empty();
        }

        // Pass an empty array to arguments for a property with no arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 33", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property33(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("Foo");
            Expressions.Add(Expr.Assign(Expr.Property(Obj, pi, new Expression[] { }), Expr.Constant(3)));

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }

        // Pass an array with a null element
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 34", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Property34(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("Item");
            EU.Throws<System.ArgumentNullException>(() =>
            {
                Expressions.Add(Expr.Assign(Expr.Property(Obj, pi, new Expression[] { null }), Expr.Constant(3)));
            });

            return Expr.Empty();
        }

        public class IndexTestClass1 {
            public int this[int val] {
                get {
                    return 123;
                }
            }
        }

        // Assign to read only property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 35", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property35(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(IndexTestClass1), "Obj");
            ConstructorInfo ci = typeof(IndexTestClass1).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(IndexTestClass1).GetProperty("Item");
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.Assign(Expr.Property(Obj, pi, new Expression[] { Expr.Constant(0) }), Expr.Constant(3)));
            });

            Expressions.Add(EU.GenAreEqual(Expr.Constant(123), Expr.Property(Obj, "Item"), "Property 1"));

            return Expr.Empty();
        }

        public class IndexTestClass2 {
            public string _result;
            public int this[int val] {
                set {
                    _result += "setIndex(" + System.Convert.ToString(val) + ")";
                }
            }
        }

        // Read from write only property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 36", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property36(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(IndexTestClass2), "Obj");
            ConstructorInfo ci = typeof(IndexTestClass2).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(IndexTestClass2).GetProperty("Item");

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(123), Expr.Property(Obj, pi, new Expression[] { Expr.Constant(0) }), "Property 1");
            }));

            return Expr.Empty();
        }

        public class IndexTestClass3 {
            public string _result;
            public int this[int val] {
                get {
                    return 123;
                }
                private set {
                    _result += "setIndex(" + System.Convert.ToString(val) + ")";
                }
            }
        }

        // Different visibility for getter and setter, assign and read
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 37", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property37(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(IndexTestClass3), "Obj");
            ConstructorInfo ci = typeof(IndexTestClass3).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(IndexTestClass3).GetProperty("Item");

            Expressions.Add(EU.GenAreEqual(Expr.Constant(123), Expr.Property(Obj, pi, new Expression[] { Expr.Constant(0) }), "Property 1"));
            Expressions.Add(Expr.Assign(Expr.Property(Obj, pi, new Expression[] { Expr.Constant(0) }), Expr.Constant(4)));

            FieldInfo fi = typeof(IndexTestClass3).GetField("_result");
            Expressions.Add(EU.GenAreEqual(Expr.Constant("setIndex(0)"), Expr.Field(Obj, fi), "Property 2"));

            var tree = Expr.Block(new[] { Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass null to arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 38", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property38(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("Item");
            EU.Throws<System.ArgumentException>(() =>
            {
                Expressions.Add(Expr.Assign(Expr.Property(Obj, pi, (List<Expr>)null), Expr.Constant(3)));
            });

            return Expr.Empty();
        }

        // Pass an ienumerable implementing object with no elements to arguments for a property with no arguments
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 39", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property39(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("Foo");
            Expressions.Add(Expr.Assign(Expr.Property(Obj, pi, new List<Expr>() { }), Expr.Constant(3)));

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }

        // Pass an ienumerable implementing object with null elements
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 40", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentNullException))]
        public static Expr Property40(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            PropertyInfo pi = typeof(TestClass).GetProperty("Item");
            EU.Throws<System.ArgumentNullException>(() =>
            {
                Expressions.Add(Expr.Assign(Expr.Property(Obj, pi, new List<Expr>() { null }), Expr.Constant(3)));
            });

            return Expr.Empty();
        }

        // Read the property value using this factory 
        // Assign to the property using this factory
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 41", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property41(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(Expr.Assign(Expr.Property(Obj, typeof(TestClass), "Foo"), Expr.Constant(3)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(123), Expr.Property(Obj, typeof(TestClass), "Foo"), "Property 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()set_Foo(3)get_Foo"), Expr.Field(Obj, typeof(TestClass), "result"), "Property 2"));

            var tree = Expr.Block(new[] { Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // empty ("") string
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 42", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property42(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, typeof(TestClass), ""), Expr.Constant(3));
            }));

            return Expr.Empty();
        }

        // Miscased string
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 43", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property43(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(Expr.Assign(Expr.Property(Obj, typeof(TestClass), "foo"), Expr.Constant(3)));

            Expressions.Add(EU.GenAreEqual(Expr.Constant(123), Expr.Property(Obj, typeof(TestClass), "foo"), "Property 1"));
            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()set_Foo(3)get_Foo"), Expr.Field(Obj, typeof(TestClass), "result"), "Property 1"));

            var tree = Expr.Block(new[] { Obj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Use a type of a different type than the expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 44", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property44(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(Expr.Assign(Expr.Property(Obj, typeof(TestClass), "Foo"), Expr.Constant(3)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                EU.GenAreEqual(Expr.Constant(123), Expr.Property(Obj, typeof(string), "Foo"), "Property 1");
            }));

            return Expr.Block(new[] { Obj }, Expressions);
        }

        // Use a derived type from the expression for type, use propertyname for a base type's property  
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 45", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property45(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, typeof(DerivedTestClass), "Foo"), Expr.Constant(3));
            }));

            return Expr.Empty();
        }

        // Pass an open generic type to type
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 46", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property46(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(GenericValue<int>), "Obj");
            ConstructorInfo ci = typeof(GenericValue<int>).GetConstructor(new Type[] { typeof(int) });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci, new Expression[] { Expr.Constant(1) })));
            
            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, typeof(GenericValue<>), "Value"), Expr.Constant(1));
            }));

            return Expr.Empty();
        }

        // Name corresponds to a field, try to call as a parameterless property
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 47", new string[] { "negative", "property", "assignread", "Pri2" }, Exception = typeof(ArgumentException))]
        public static Expr Property47(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(EU.Throws<System.ArgumentException>(() =>
            {
                Expr.Assign(Expr.Property(Obj, typeof(TestClass), "result"), Expr.Constant(3));
            }));

            return Expr.Empty();
        }

        // Access a property that is hidden by a member in the derived class
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 48", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property48(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression Obj = Expr.Variable(typeof(TestClass), "Obj");
            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj, Expr.New(ci)));

            Expressions.Add(Expr.Assign(Expr.Property(Obj, typeof(TestClass), "Val"), Expr.Constant(new GenericValue<int>(3))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()set_GenericValue"), Expr.Field(Obj, typeof(TestClass), "result"), "Property 1"));


            ParameterExpression Obj2 = Expr.Variable(typeof(DerivedTestClass), "Obj2");
            ConstructorInfo ci2 = typeof(DerivedTestClass).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(Obj2, Expr.New(ci2)));

            Expressions.Add(Expr.Assign(Expr.Property(Obj2, typeof(DerivedTestClass), "Val"), Expr.Constant(new GenericValue<int>(4))));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("new TestClass()new DerivedTestClass()set_DerivedGenericValue"), Expr.Field(Obj2, typeof(DerivedTestClass), "result"), "Property 1"));

            var tree = Expr.Block(new[] { Obj, Obj2 }, Expressions);
            V.Validate(tree);
            return tree;
        }

        public class IndexTestClass4<T> {
            public string _result;
            public T this[IEnumerable<int> val] {
                get {
                    _result += "getIndex";
                    return default(T);
                }
                private set {
                    _result += "setIndex(" + System.Convert.ToString(val) + ")";
                }
            }
        }

        // Pass types of arguments that are reference convertible to property's
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 49", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property49(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(IndexTestClass4<int>), "TestObj");
            ConstructorInfo ci = typeof(IndexTestClass4<int>).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci)));

            PropertyInfo pi = typeof(IndexTestClass4<int>).GetProperty("Item");
            Expressions.Add(Expr.Property(TestObj, pi, new Expression[] { Expr.Constant(new List<int>() { 1, 2, 3 }) }));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("getIndex"), Expr.Field(TestObj, "_result"), "Property 1"));

            var tree = Expr.Block(new[] { TestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        // Pass types of arguments that are reference convertible to property's
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 50", new string[] { "positive", "property", "assignread", "Pri2" })]
        public static Expr Property50(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(IndexTestClass4<int>), "TestObj");
            ConstructorInfo ci = typeof(IndexTestClass4<int>).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci)));

            PropertyInfo pi = typeof(IndexTestClass4<int>).GetProperty("Item");
            Expressions.Add(Expr.Property(TestObj, pi, new List<Expression>() { Expr.Constant(new List<int>() { 1, 2, 3 }) }));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("getIndex"), Expr.Field(TestObj, "_result"), "Property 1"));

            var tree = Expr.Block(new[] { TestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }

        //null to expression
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 51", new string[] { "negative", "property", "assignread", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Property51(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<ArgumentNullException>(() => { Expr.Property((Expression)null, "", new Expression[] { }); });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 52", new string[] { "negative", "property", "assignread", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr Property52(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<ArgumentNullException>(() => { Expr.Property(Expr.Constant(1), (string)null, new Expression[] { }); });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 53", new string[] { "negative", "property", "assignread", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Property53(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<ArgumentException>(() => { Expr.Property(Expr.Constant(new IndexTestClass3()), "Item", (Expression[])null); });

            return Expr.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Property 54", new string[] { "positive", "property", "assignread", "Pri1" })]
        public static Expr Property54(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ParameterExpression TestObj = Expr.Variable(typeof(IndexTestClass4<int>), "TestObj");
            ConstructorInfo ci = typeof(IndexTestClass4<int>).GetConstructor(new Type[] { });
            Expressions.Add(Expr.Assign(TestObj, Expr.New(ci)));

            Expressions.Add(Expr.Property(TestObj, "Item", new Expression[] { Expr.Constant(new List<int>() { 1, 2, 3 }) }));

            Expressions.Add(EU.GenAreEqual(Expr.Constant("getIndex"), Expr.Field(TestObj, "_result"), "Property 1"));

            var tree = Expr.Block(new[] { TestObj }, Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
