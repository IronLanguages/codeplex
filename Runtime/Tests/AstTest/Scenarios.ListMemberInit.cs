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

#if !SILVERLIGHT3
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {

    public partial class Scenarios {

        private class ErrorClass1 {
            public static void Add(string a) {
            }
        }

        private class ErrorClass2 {
            public void Add(ref string a) {
            }
        }

        private class ErrorClass3 {

            public ErrorClass3() { }

            public void Add(ref string a) {
            }
        }

        private class ErrorClass4 {

            public ErrorClass4() { }

            public void Add(string a) {

            }
        }

        // Build the Expected Expression Tree (no spilling)
        public static void Positive_ListInit1(EU.IValidator V) {
            ConstructorInfo[] ci = typeof(List<int?>).GetConstructors();
            ConstantExpression ce1 = Expression.Constant(0, typeof(int));
            ConstantExpression ce2 = Expression.Constant(1, typeof(int));
            ConstantExpression ce3 = Expression.Constant(-10, typeof(int));
            ConstantExpression ce4 = Expression.Constant(45, typeof(int));
            UnaryExpression ue1 = Expression.Convert(ce1, typeof(int?));
            UnaryExpression ue2 = Expression.Convert(ce2, typeof(int?));
            UnaryExpression ue3 = Expression.Convert(ce3, typeof(int?));
            UnaryExpression ue4 = Expression.Convert(ce4, typeof(int?));
            NewExpression ne1 = Expression.New(ci[0]);
            ListInitExpression lie1 = Expression.ListInit(ne1, new Expression[] { ue1, ue2, ue3, ue4 });
            Expression<Func<List<int?>>> testExpr = Expression.Lambda<Func<List<int?>>>(lie1);

            V.Validate(testExpr, f =>
            {
                var testResult = (List<int?>)f();
                EU.Equal(testResult[0], 0);
                EU.Equal(testResult[3], 45);
            });
        }

        // Build the Expected Expression Tree (spilling)
        public static void Positive_ListInit2(EU.IValidator V) {
            var t = Expression.TryFinally(Expression.Constant(42), Expression.Empty());
            Expression comma = Expression.Block(t, Expression.Constant(47));

            ConstructorInfo[] ci = typeof(List<int?>).GetConstructors();
            ConstantExpression ce1 = Expression.Constant(0, typeof(int));
            ConstantExpression ce2 = Expression.Constant(1, typeof(int));
            ConstantExpression ce3 = Expression.Constant(-10, typeof(int));
            ConstantExpression ce4 = Expression.Constant(45, typeof(int));
            UnaryExpression ue1 = Expression.Convert(ce1, typeof(int?));
            UnaryExpression ue2 = Expression.Convert(ce2, typeof(int?));
            UnaryExpression ue3 = Expression.Convert(ce3, typeof(int?));
            UnaryExpression ue4 = Expression.Convert(comma, typeof(int?));  // <-- comma here
            NewExpression ne1 = Expression.New(ci[0]);
            ListInitExpression lie1 = Expression.ListInit(ne1, new Expression[] { ue1, ue2, ue3, ue4 });
            Expression<Func<List<int?>>> testExpr = Expression.Lambda<Func<List<int?>>>(lie1);

            V.Validate(testExpr, f =>
            {
                var testResult = (List<int?>)f();
                EU.Equal(testResult[0], 0);
                EU.Equal(testResult[3], 47);
            });
        }

        // Build the Expected Expression Tree (no spilling)
        public static void Positive_ListInit3(EU.IValidator V) {
            // Build the Expected Expression Tree
            ConstructorInfo[] ci = typeof(List<decimal?>).GetConstructors();
            ConstantExpression ce1 = Expression.Constant(9m, typeof(decimal));
            ConstantExpression ce2 = Expression.Constant(-45m, typeof(decimal));
            ConstantExpression ce3 = Expression.Constant(null, typeof(decimal?));
            UnaryExpression ue1 = Expression.Convert(ce1, typeof(decimal?));
            UnaryExpression ue2 = Expression.Convert(ce2, typeof(decimal?));
            NewExpression ne1 = Expression.New(ci[0]);
            ListInitExpression lie1 = Expression.ListInit(ne1, new Expression[] { ue1, ue2, ce3 });
            Expression<Func<List<decimal?>>> testExpr = Expression.Lambda<Func<List<decimal?>>>(lie1);

            V.Validate(testExpr, f =>
            {
                var testResult = (List<decimal?>)f();
                EU.Equal(testResult[0], 9m);
                EU.Equal(testResult[2], null);
            });
        }

        // Build the Expected Expression Tree (spilling)
        public static void Positive_ListInit4(EU.IValidator V) {
            var t = Expression.TryFinally(Expression.Constant(42), Expression.Empty());
            Expression comma = Expression.Block(t, Expression.Constant(-45m));

            ConstructorInfo[] ci = typeof(List<decimal?>).GetConstructors();
            ConstantExpression ce1 = Expression.Constant(9m, typeof(decimal));
            ConstantExpression ce3 = Expression.Constant(null, typeof(decimal?));
            UnaryExpression ue1 = Expression.Convert(ce1, typeof(decimal?));
            UnaryExpression ue2 = Expression.Convert(comma, typeof(decimal?));      // <-- comma here
            NewExpression ne1 = Expression.New(ci[0]);
            ListInitExpression lie1 = Expression.ListInit(ne1, new Expression[] { ue1, ue2, ce3 });
            Expression<Func<List<decimal?>>> testExpr = Expression.Lambda<Func<List<decimal?>>>(lie1);

            V.Validate(testExpr, f =>
            {
                var testResult = (List<decimal?>)f();
                EU.Equal(testResult[1], -45m);
                EU.Equal(testResult[2], null);
            });
        }

        public class Point {
            public int number;
            public string name;

            List<string> shapes = new List<string>();
            public List<string> Shapes { get { return shapes; } set { shapes = value; } }

            public List<string> employees = new List<string>();
        }

        // Build the Expected Expression Tree (no spilling)
        public static void Positive_ObjInit1(EU.IValidator V) {
            FieldInfo fi1 = typeof(Point).GetField("number");
            FieldInfo fi2 = typeof(Point).GetField("name");
            FieldInfo fi3 = typeof(Point).GetField("employees");
            ConstructorInfo[] cis = typeof(Point).GetConstructors();
            ConstantExpression ce1 = Expression.Constant(1, typeof(int));
            ConstantExpression ce2 = Expression.Constant("Test", typeof(string));
            ConstantExpression ce3 = Expression.Constant("string1", typeof(string));
            ConstantExpression ce4 = Expression.Constant("string2", typeof(string));
            MethodInfo mi1 = typeof(List<String>).GetMethod("Add", new Type[] { typeof(string) });
            ElementInit ei1 = Expression.ElementInit(mi1, ce3);
            ElementInit ei2 = Expression.ElementInit(mi1, ce4);
            MemberAssignment ma1 = Expression.Bind(fi1, ce1);
            MemberAssignment ma2 = Expression.Bind(fi2, ce2);
            MemberListBinding mlb1 = Expression.ListBind(fi3, new ElementInit[] { ei1, ei2 });
            NewExpression ne1 = Expression.New(cis[0]);
            MemberInitExpression mie1 = Expression.MemberInit(ne1, new MemberBinding[] { ma1, ma2, mlb1 });
            Expression<Func<Point>> testExpr = Expression.Lambda<Func<Point>>(mie1);

            V.Validate(testExpr, f =>
            {
                var testResult = (Point)f();
                EU.Equal(testResult.name, "Test");
                EU.Equal(testResult.employees[1], "string2");
            });
        }

        // Build the Expected Expression Tree (spill stack)
        public static void Positive_ObjInit2(EU.IValidator V) {
            var t = Expression.TryFinally(Expression.Constant(42), Expression.Empty());
            Expression comma = Expression.Block(t, Expression.Constant("string3"));

            FieldInfo fi1 = typeof(Point).GetField("number");
            FieldInfo fi2 = typeof(Point).GetField("name");
            FieldInfo fi3 = typeof(Point).GetField("employees");
            ConstructorInfo[] cis = typeof(Point).GetConstructors();
            ConstantExpression ce1 = Expression.Constant(1, typeof(int));
            ConstantExpression ce2 = Expression.Constant("Test", typeof(string));
            ConstantExpression ce3 = Expression.Constant("string1", typeof(string));
            ConstantExpression ce4 = Expression.Constant("string2", typeof(string));
            MethodInfo mi1 = typeof(List<String>).GetMethod("Add", new Type[] { typeof(string) });
            ElementInit ei1 = Expression.ElementInit(mi1, ce3);
            ElementInit ei2 = Expression.ElementInit(mi1, ce4);
            ElementInit ei3 = Expression.ElementInit(mi1, comma);   // <-- comma here
            MemberAssignment ma1 = Expression.Bind(fi1, ce1);
            MemberAssignment ma2 = Expression.Bind(fi2, ce2);
            MemberListBinding mlb1 = Expression.ListBind(fi3, new ElementInit[] { ei1, ei2, ei3 });
            NewExpression ne1 = Expression.New(cis[0]);
            MemberInitExpression mie1 = Expression.MemberInit(ne1, new MemberBinding[] { ma1, ma2, mlb1 });
            Expression<Func<Point>> testExpr = Expression.Lambda<Func<Point>>(mie1);

            V.Validate(testExpr, f =>
            {
                var testResult = (Point)f();
                EU.Equal(ei3.ToString(), "Void Add(System.String)({ ... })");    //Verify tostring()
                EU.Equal(testResult.name, "Test");
                EU.Equal(testResult.employees[1], "string2");
                EU.Equal(testResult.employees[2], "string3");
            });
        }

        public class Point4 {
            public int coord1;
            public int coord2;
            public int coord3;
            public int coord4;
        }
        public class Figure {
            public string shape;
            public Point4 points = new Point4();
        }

        //Build the Expected Expression Tree  (no spilling)
        public static void Positive_ObjInit3(EU.IValidator V) {
            FieldInfo fi1 = typeof(Figure).GetField("shape");
            FieldInfo fi2 = typeof(Figure).GetField("points");
            FieldInfo fi3 = typeof(Point4).GetField("coord1");
            FieldInfo fi4 = typeof(Point4).GetField("coord2");
            FieldInfo fi5 = typeof(Point4).GetField("coord3");
            FieldInfo fi6 = typeof(Point4).GetField("coord4");
            ConstructorInfo[] cis1 = typeof(Figure).GetConstructors();
            ConstantExpression ce1 = Expression.Constant(45, typeof(int));
            ConstantExpression ce2 = Expression.Constant(35, typeof(int));
            ConstantExpression ce3 = Expression.Constant(99, typeof(int));
            ConstantExpression ce4 = Expression.Constant(27, typeof(int));
            ConstantExpression ce5 = Expression.Constant("Square", typeof(string));
            MemberAssignment ma1 = Expression.Bind(fi3, ce1);
            MemberAssignment ma2 = Expression.Bind(fi4, ce2);
            MemberAssignment ma3 = Expression.Bind(fi5, ce3);
            MemberAssignment ma4 = Expression.Bind(fi6, ce4);
            MemberAssignment ma5 = Expression.Bind(fi1, ce5);
            NewExpression ne1 = Expression.New(cis1[0]);
            MemberMemberBinding mmb1 = Expression.MemberBind(fi2, new MemberBinding[] { ma1, ma2, ma3, ma4 });

            MemberInitExpression mie1 = Expression.MemberInit(ne1, new MemberBinding[] { ma5, mmb1 });
            Expression<Func<Figure>> testExpr = Expression.Lambda<Func<Figure>>(mie1);

            V.Validate(testExpr, f =>
            {
                var testResult = (Figure)f();
                EU.Equal(testResult.shape, "Square");
                EU.Equal(testResult.points.coord1, 45);
                EU.Equal(testResult.points.coord4, 27);
            });
        }

        //Build the Expected Expression Tree  (spill stack)
        public static void Positive_ObjInit4(EU.IValidator V) {
            var t = Expression.TryFinally(Expression.Constant(42), Expression.Empty());
            Expression comma = Expression.Block(t, Expression.Constant(333));

            FieldInfo fi1 = typeof(Figure).GetField("shape");
            FieldInfo fi2 = typeof(Figure).GetField("points");
            FieldInfo fi3 = typeof(Point4).GetField("coord1");
            FieldInfo fi4 = typeof(Point4).GetField("coord2");
            FieldInfo fi5 = typeof(Point4).GetField("coord3");
            FieldInfo fi6 = typeof(Point4).GetField("coord4");
            ConstructorInfo[] cis1 = typeof(Figure).GetConstructors();
            ConstantExpression ce1 = Expression.Constant(45, typeof(int));
            ConstantExpression ce2 = Expression.Constant(35, typeof(int));
            ConstantExpression ce3 = Expression.Constant(99, typeof(int));
            ConstantExpression ce5 = Expression.Constant("Square", typeof(string));
            MemberAssignment ma1 = Expression.Bind(fi3, ce1);
            MemberAssignment ma2 = Expression.Bind(fi4, ce2);
            MemberAssignment ma3 = Expression.Bind(fi5, ce3);
            MemberAssignment ma4 = Expression.Bind(fi6, comma); // <-- comma here
            MemberAssignment ma5 = Expression.Bind(fi1, ce5);
            NewExpression ne1 = Expression.New(cis1[0]);
            MemberMemberBinding mmb1 = Expression.MemberBind(fi2, new MemberBinding[] { ma1, ma2, ma3, ma4 });

            MemberInitExpression mie1 = Expression.MemberInit(ne1, new MemberBinding[] { ma5, mmb1 });
            Expression<Func<Figure>> testExpr = Expression.Lambda<Func<Figure>>(mie1);

            V.Validate(testExpr, f =>
            {
                var testResult = (Figure)f();
                EU.Equal(testResult.shape, "Square");
                EU.Equal(testResult.points.coord1, 45);
                EU.Equal(testResult.points.coord4, 333);
            });
        }

        public struct PointS {
            public int coord1;
            public int coord2;
            public int coord3;
            public int coord4;
        }
        public struct FigureS {
            public string shape;
            public PointS points;
        }

        //Build the Expected Expression Tree  (no spilling)
        public static void Positive_ObjInit5(EU.IValidator V) {
            FieldInfo fi1 = typeof(FigureS).GetField("shape");
            FieldInfo fi2 = typeof(FigureS).GetField("points");
            FieldInfo fi3 = typeof(PointS).GetField("coord1");
            FieldInfo fi4 = typeof(PointS).GetField("coord2");
            FieldInfo fi5 = typeof(PointS).GetField("coord3");
            FieldInfo fi6 = typeof(PointS).GetField("coord4");
            ConstantExpression ce1 = Expression.Constant(45, typeof(int));
            ConstantExpression ce2 = Expression.Constant(35, typeof(int));
            ConstantExpression ce3 = Expression.Constant(99, typeof(int));
            ConstantExpression ce4 = Expression.Constant(27, typeof(int));
            ConstantExpression ce5 = Expression.Constant("Square", typeof(string));
            MemberAssignment ma1 = Expression.Bind(fi3, ce1);
            MemberAssignment ma2 = Expression.Bind(fi4, ce2);
            MemberAssignment ma3 = Expression.Bind(fi5, ce3);
            MemberAssignment ma4 = Expression.Bind(fi6, ce4);
            MemberAssignment ma5 = Expression.Bind(fi1, ce5);
            NewExpression ne1 = Expression.New(typeof(FigureS));
            MemberMemberBinding mmb1 = Expression.MemberBind(fi2, new MemberBinding[] { ma1, ma2, ma3, ma4 });
            MemberInitExpression mie1 = Expression.MemberInit(ne1, new MemberBinding[] { ma5, mmb1 });
            Expression<Func<FigureS>> testExpr = Expression.Lambda<Func<FigureS>>(mie1);

            V.Validate(testExpr, f =>
            {
                var testResult = (FigureS)f();
                EU.Equal(testResult.shape, "Square");
                EU.Equal(testResult.points.coord1, 45);
                EU.Equal(testResult.points.coord4, 27);
            });
        }

        //Build the Expected Expression Tree  (spill stack)
        public static void Positive_ObjInit7(EU.IValidator V) {
            var t = Expression.TryFinally(Expression.Constant(42), Expression.Empty());
            Expression comma = Expression.Block(t, Expression.Constant(333));

            FieldInfo fi1 = typeof(FigureS).GetField("shape");
            FieldInfo fi2 = typeof(FigureS).GetField("points");
            FieldInfo fi3 = typeof(PointS).GetField("coord1");
            FieldInfo fi4 = typeof(PointS).GetField("coord2");
            FieldInfo fi5 = typeof(PointS).GetField("coord3");
            FieldInfo fi6 = typeof(PointS).GetField("coord4");
            ConstantExpression ce1 = Expression.Constant(45, typeof(int));
            ConstantExpression ce2 = Expression.Constant(35, typeof(int));
            ConstantExpression ce3 = Expression.Constant(99, typeof(int));
            ConstantExpression ce5 = Expression.Constant("Square", typeof(string));
            MemberAssignment ma1 = Expression.Bind(fi3, ce1);
            MemberAssignment ma2 = Expression.Bind(fi4, ce2);
            MemberAssignment ma3 = Expression.Bind(fi5, ce3);
            MemberAssignment ma4 = Expression.Bind(fi6, comma);         // <-- comma here
            MemberAssignment ma5 = Expression.Bind(fi1, ce5);
            NewExpression ne1 = Expression.New(typeof(FigureS));
            MemberMemberBinding mmb1 = Expression.MemberBind(fi2, new MemberBinding[] { ma1, ma2, ma3, ma4 });
            MemberInitExpression mie1 = Expression.MemberInit(ne1, new MemberBinding[] { ma5, mmb1 });
            Expression<Func<FigureS>> testExpr = Expression.Lambda<Func<FigureS>>(mie1);

            V.ValidateException<NotSupportedException>(testExpr, true);
            //Func<FigureS> testFunc = testExpr.Compile();
            //FigureS testResult = testFunc();

            //EU.Equal(testResult.shape, "Square");
            //EU.Equal(testResult.points.coord1, 45);
            //EU.Equal(testResult.points.coord4, 333);
        }

        public struct PointP {
            public int number;
            public string name;
            int age;

            public int Age { get { return age; } set { age = value; } }
        }

        //Build the Expected Expression Tree  (no spilling)
        public static void Positive_ObjInit8(EU.IValidator V) {
            FieldInfo fi1 = typeof(PointP).GetField("number");
            FieldInfo fi2 = typeof(PointP).GetField("name");
            PropertyInfo pi1 = typeof(PointP).GetProperty("Age");
            ConstantExpression ce1 = Expression.Constant(1, typeof(int));
            ConstantExpression ce2 = Expression.Constant("Test", typeof(string));
            ConstantExpression ce3 = Expression.Constant(95, typeof(int));
            MemberAssignment ma1 = Expression.Bind(fi1, ce1);
            MemberAssignment ma2 = Expression.Bind(fi2, ce2);
            MemberAssignment ma3 = Expression.Bind(pi1, ce3);
            NewExpression ne1 = Expression.New(typeof(PointP));
            MemberInitExpression mie1 = Expression.MemberInit(ne1, new MemberBinding[] { ma1, ma2, ma3 });
            Expression<Func<PointP>> testExpr = Expression.Lambda<Func<PointP>>(mie1);

            V.Validate(testExpr, f =>
            {
                var testResult = (PointP)f();
                EU.Equal(testResult.number, 1);
                EU.Equal(testResult.name, "Test");
                EU.Equal(testResult.Age, 95);
            });
        }

        // Quick ElementInit error cases
        public static void Negative_ObjInit1(EU.IValidator V) {

            ElementInit ei1;
            ConstantExpression ce3 = Expression.Constant("string1", typeof(string));

            MethodInfo mi1 = typeof(List<String>).GetMethod("ToArray", Type.EmptyTypes);

            // Method doesn't have parameter
            EU.Throws<ArgumentException>(() => ei1 = Expression.ElementInit(mi1, ce3));

            mi1 = typeof(List<String>).GetMethod("IndexOf", new Type[] { typeof(string) });
            // Non-Add method
            EU.Throws<ArgumentException>(() => ei1 = Expression.ElementInit(mi1, ce3));

            // Static method
            mi1 = typeof(ErrorClass1).GetMethod("Add", new Type[] { typeof(string) });
            EU.Throws<ArgumentException>(() => ei1 = Expression.ElementInit(mi1, ce3));

            // ByRef params to method
            mi1 = typeof(ErrorClass2).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            EU.Throws<ArgumentException>(() => ei1 = Expression.ElementInit(mi1, ce3));


            // Similar case to above, but with ListInit
            ConstructorInfo[] ci = typeof(ErrorClass3).GetConstructors();
            ConstantExpression ce1 = Expression.Constant("HI", typeof(string));
            NewExpression ne1 = Expression.New(ci[0]);
            EU.Throws<ArgumentException>(() => Expression.ListInit(ne1, new Expression[] { ce1 }));

            // List not enumerable
            ci = typeof(ErrorClass4).GetConstructors();
            ce1 = Expression.Constant("HI", typeof(string));
            ne1 = Expression.New(ci[0]);
            EU.Throws<ArgumentException>(() => Expression.ListInit(ne1, new Expression[] { ce1 }));
        }

        //Build the Expected Expression Tree  (spill stack)
        public static void Positive_ObjInit9(EU.IValidator V) {
            var t = Expression.TryFinally(Expression.Constant(42), Expression.Empty());
            Expression comma = Expression.Block(t, Expression.Constant(333));

            FieldInfo fi1 = typeof(PointP).GetField("number");
            FieldInfo fi2 = typeof(PointP).GetField("name");
            PropertyInfo pi1 = typeof(PointP).GetProperty("Age");
            ConstantExpression ce1 = Expression.Constant(1, typeof(int));
            ConstantExpression ce2 = Expression.Constant("Test", typeof(string));
            MemberAssignment ma1 = Expression.Bind(fi1, ce1);
            MemberAssignment ma2 = Expression.Bind(fi2, ce2);
            MemberAssignment ma3 = Expression.Bind(pi1, comma);     //  <--  comma here
            NewExpression ne1 = Expression.New(typeof(PointP));
            MemberInitExpression mie1 = Expression.MemberInit(ne1, new MemberBinding[] { ma1, ma2, ma3 });
            Expression<Func<PointP>> testExpr = Expression.Lambda<Func<PointP>>(mie1);

            V.ValidateException<NotSupportedException>(testExpr, true);
            //Func<PointP> testFunc = testExpr.Compile();
            //PointP testResult = testFunc();

            //EU.Equal(testResult.number, 1);
            //EU.Equal(testResult.name, "Test");
            //EU.Equal(testResult.Age, 333);
        }


        public class Circle {
            public Point Center { get; set; }

            public Circle() { MyList = new List<int>(); }
            public List<int> MyList { get; set; }
        }

        public static Expression Positive_ListBindZeroElements(EU.IValidator V) {
            var l2 = Expression.Lambda<Func<Circle>>(
                Expression.MemberInit(
                    Expression.New(typeof(Circle)),
                    new MemberBinding[] { 
                        Expression.ListBind(
                            typeof(Circle).GetMethod("get_MyList"),
                            new ElementInit[] {}
                        )
                    }
                )
            );

            V.Validate(l2);

            return l2;
        }
    }
}
#endif
