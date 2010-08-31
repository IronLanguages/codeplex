#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class VisitorChanges {
        class MyVisitor : ExpressionVisitor {

        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 1", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges1(EU.IValidator V) {
            var c1 = Expression.Constant(2);
            var c2 = Expression.Constant(2);
            var parm1 = Expression.Parameter(typeof(int));
            var BE = Expression.Add(c1, c2);
            var Conv = BE.Conversion;

            var res = BE.Update(Expression.Constant(1), Conv, c2);

            if (res == BE) throw new Exception("Update should have created a different instance");

            res = BE.Update(c1, Conv, Expression.Constant(2));

            if (res == BE) throw new Exception("Update should have created a different instance");


            res = BE.Update(c1, Expression.Lambda<Func<int, int>>(parm1, parm1), c2);

            if (res == BE) throw new Exception("Update should have created a different instance");

            res = BE.Update(c1, Conv, c2);

            if (res != BE) throw new Exception("Update should have returned the same instance");


            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 2", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges2(EU.IValidator V) {
            var c1 = Expression.Constant(2);
            var c2 = Expression.Constant(2);
            var parm1 = Expression.Parameter(typeof(int));
            var BE = Expression.Block(new ParameterExpression[] { parm1 }, c1, c2);

            var res = BE.Update(new ParameterExpression[] { parm1 }, new Expression[] { Expression.Constant(1), c2 });

            if (res == BE) throw new Exception("Update should have created a different instance");

            res = BE.Update(new ParameterExpression[] { parm1 }, new Expression[] { c1, Expression.Constant(2) });

            if (res == BE) throw new Exception("Update should have created a different instance");


            res = BE.Update(new ParameterExpression[] { Expression.Parameter(typeof(int)) }, new Expression[] { c1, c2 });

            if (res == BE) throw new Exception("Update should have created a different instance");



            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 3", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges3(EU.IValidator V) {
            var c1 = Expression.Constant(1);
            var type = Expression.Parameter(typeof(Exception));
            var Filter = Expression.Constant(true);

            var CBE = Expression.Catch(type, c1, Filter);




            var res = CBE.Update(Expression.Parameter(typeof(Exception)), Filter, c1);

            if (res == CBE) throw new Exception("Update should have created a different instance");

            res = CBE.Update(type, Filter, Expression.Constant(1));

            if (res == CBE) throw new Exception("Update should have created a different instance");


            res = CBE.Update(type, Expression.Constant(true), c1);

            if (res == CBE) throw new Exception("Update should have created a different instance");



            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 4", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges4(EU.IValidator V) {
            var c1 = Expression.Constant(true);
            var c2 = Expression.Constant(1);
            var c3 = Expression.Constant(2);


            var CE = Expression.Condition(c1, c2, c3);




            var res = CE.Update(Expression.Constant(true), c2, c3);

            if (res == CE) throw new Exception("Update should have created a different instance");

            res = CE.Update(c1, Expression.Constant(1), c3);

            if (res == CE) throw new Exception("Update should have created a different instance");


            res = CE.Update(c1, c2, Expression.Constant(2));

            if (res == CE) throw new Exception("Update should have created a different instance");



            return Expression.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 5", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges5(EU.IValidator V) {
            var c1 = Expression.Constant(1);
            var c2 = Expression.Constant(2);
            var List = new List<Expression>();
            List.Add(c1);
            List.Add(c2);


            var binder = new ETScenarios.Miscellaneous.DynamicNode.MyBinder();

            var DE = Expression.Dynamic(binder, typeof(DateTime), List);



            var res = DE.Update(List);

            if (res == DE) throw new Exception("Update should have created a different instance");


            res = DE.Update(DE.Arguments);

            if (res != DE) throw new Exception("Update should have returned the same instance");



            return Expression.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 6", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges6(EU.IValidator V) {
            var c1 = Expression.Constant(1);
            var c2 = Expression.Constant(2);
            var List = new List<Expression>();
            List.Add(c1);

            var EIE = Expression.ElementInit(typeof(List<int>).GetMethod("Add"), List);


            var res = EIE.Update(List);

            if (res == EIE) throw new Exception("Update should have created a different instance");


            res = EIE.Update(EIE.Arguments);

            if (res != EIE) throw new Exception("Update should have returned the same instance");



            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 7", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges7(EU.IValidator V) {
            var lb = Expression.Label();
            var c1 = Expression.Constant(1);

            var GE = Expression.Goto(lb, c1);


            var res = GE.Update(Expression.Label(), c1);

            if (res == GE) throw new Exception("Update should have created a different instance");


            res = GE.Update(lb, Expression.Constant(1));

            if (res == GE) throw new Exception("Update should have created a different instance");


            res = GE.Update(lb, c1);

            if (res != GE) throw new Exception("Update should have returned the same instance");



            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 8", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges8(EU.IValidator V) {
            var arr = Expression.Constant(new int[] { 1, 2, 3 });
            var c1 = Expression.Constant(1);

            var IE = Expression.ArrayAccess(arr, c1);


            var res = IE.Update(Expression.Constant(new int[] { 1, 2, 3 }), IE.Arguments);

            if (res == IE) throw new Exception("Update should have created a different instance");


            res = IE.Update(arr, new List<Expression>() { c1 });

            if (res == IE) throw new Exception("Update should have created a different instance");


            res = IE.Update(arr, IE.Arguments);

            if (res != IE) throw new Exception("Update should have returned the same instance");



            return Expression.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 9", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges9(EU.IValidator V) {
            var p = Expression.Parameter(typeof(int));
            var lmb = Expression.Lambda(Expression.Empty(), p);
            var l1 = new Expression[] { Expression.Constant(1) };

            var IE = Expression.Invoke(lmb, l1);


            var res = IE.Update(Expression.Lambda(Expression.Empty(), p), IE.Arguments);

            if (res == IE) throw new Exception("Update should have created a different instance");


            res = IE.Update(lmb, l1);

            if (res == IE) throw new Exception("Update should have created a different instance");


            res = IE.Update(lmb, IE.Arguments);

            if (res != IE) throw new Exception("Update should have returned the same instance");



            return Expression.Empty();
        }



        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 10", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges10(EU.IValidator V) {
            var label = Expression.Label();
            var c1 = Expression.Constant(1);

            var LE = Expression.Label(label, c1);


            var res = LE.Update(Expression.Label(), c1);

            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(label, Expression.Constant(1));

            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(label, c1);

            if (res != LE) throw new Exception("Update should have returned the same instance");



            return Expression.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 11", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges11(EU.IValidator V) {
            var Body = Expression.Empty();
            var parms = new ParameterExpression[] { Expression.Parameter(typeof(int)) };

            var LE = Expression.Lambda<Action<int>>(Body, parms);


            var res = LE.Update(Expression.Empty(), LE.Parameters);

            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(Body, parms);

            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(Body, LE.Parameters);

            if (res != LE) throw new Exception("Update should have returned the same instance");



            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 12", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges12(EU.IValidator V) {
            var List = Expression.New(typeof(List<int>).GetConstructor(new Type[] { }));
            var parms = new List<ElementInit> { Expression.ElementInit(typeof(List<int>).GetMethod("Add"), Expression.Constant(1)) };

            var LE = Expression.ListInit(List, parms);


            var res = LE.Update(Expression.New(typeof(List<int>).GetConstructor(new Type[] { })), LE.Initializers);

            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(List, parms);

            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(List, LE.Initializers);

            if (res != LE) throw new Exception("Update should have returned the same instance");



            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 13", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges13(EU.IValidator V) {
            var l1 = Expression.Label();
            var l2 = Expression.Label();
            var Body = Expression.Empty();

            var LE = Expression.Loop(Body, l1, l2);


            var res = LE.Update(Expr.Label(), l2, Body);

            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(l1, Expression.Label(), Body);

            if (res == LE) throw new Exception("Update should have created a different instance");



            res = LE.Update(l1, l2, Expression.Empty());

            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(l1, l2, Body);

            if (res != LE) throw new Exception("Update should have returned the same instance");



            return Expression.Empty();
        }

        private class c1 {
            public int x;
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 14", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges14(EU.IValidator V) {
            var member = typeof(c1).GetMember("x")[0];
            var value = Expression.Constant(1);


            var LE = Expression.Bind(member, value);

            var res = LE.Update(Expression.Constant(1));

            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(value);

            if (res != LE) throw new Exception("Update should have returned the same instance");



            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 15", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges15(EU.IValidator V) {
            var expression = Expression.Constant(new c1());
            var field = typeof(c1).GetField("x");


            var LE = Expression.Field(expression, field);

            var res = LE.Update(Expression.Constant(new c1()));

            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(expression);

            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }




        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 16", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges16(EU.IValidator V) {
            var New = Expression.New(typeof(c1).GetConstructor(new Type[] { }));
            var MemberBinding = Expression.Bind(typeof(c1).GetMember("x")[0], Expression.Constant(1));
            var ListMemberBinding = new List<MemberBinding>();
            ListMemberBinding.Add(MemberBinding);

            var LE = Expression.MemberInit(New, ListMemberBinding);

            var res = LE.Update(Expression.New(typeof(c1).GetConstructor(new Type[] { })), LE.Bindings);

            if (res == LE) throw new Exception("Update should have created a different instance");

            res = LE.Update(New, ListMemberBinding);

            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(New, LE.Bindings);

            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }


        class c2 {
            public List<int> x;
        }



        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 17", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges17(EU.IValidator V) {
            var Member = typeof(c2).GetMember("x")[0];

            var ElementInit = Expression.ElementInit(typeof(List<int>).GetMethod("Add"), Expression.Constant(1));

            var LE = Expression.ListBind(Member, ElementInit);


            var res = LE.Update(new List<ElementInit>() { ElementInit });
            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(LE.Initializers);
            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }

        class c3 {
            public c1 y;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 18", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges18(EU.IValidator V) {

            var member = typeof(c3).GetMember("y")[0];
            var bindings = Expression.Bind(typeof(c1).GetField("x"), Expression.Constant(1));

            var LE = Expression.MemberBind(member, bindings);


            var res = LE.Update(new List<MemberBinding>() { bindings });
            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(LE.Bindings);
            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 19", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges19(EU.IValidator V) {

            var inst = Expression.Constant(new List<int>());
            var method = typeof(List<int>).GetMethod("Add");
            var args = new List<Expression> { Expression.Constant(1) };

            var LE = Expression.Call(inst, method, args);


            var res = LE.Update(Expression.Constant(new List<int>()), LE.Arguments);
            if (res == LE) throw new Exception("Update should have created a different instance");

            res = LE.Update(inst, args);
            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(inst, LE.Arguments);
            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 20", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges20(EU.IValidator V) {

            var type = typeof(int);
            var bounds = new List<Expression>() { Expression.Constant(1) };

            var LE = Expression.NewArrayBounds(type, bounds);


            var res = LE.Update(bounds);
            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(LE.Expressions);
            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 21", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges21(EU.IValidator V) {

            var args = new List<Expression>() { };

            var LE = Expression.New(typeof(List<int>).GetConstructor(new Type[] { }), args);


            var res = LE.Update(args);
            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(LE.Arguments);
            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 22", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges22(EU.IValidator V) {
            var p = new List<ParameterExpression>() { Expression.Parameter(typeof(int)) };

            var LE = Expression.RuntimeVariables(p);


            var res = LE.Update(p);
            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(LE.Variables);
            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }



        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 23", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges23(EU.IValidator V) {
            var value = Expression.Constant(1);
            var cases = new SwitchCase[] { Expression.SwitchCase(Expression.Empty(), Expression.Constant(1)) };
            var def = Expression.Empty();

            var LE = Expression.Switch(value, def, cases);



            var res = LE.Update(Expression.Constant(1), LE.Cases, def);
            if (res == LE) throw new Exception("Update should have created a different instance");

            res = LE.Update(value, cases, def);
            if (res == LE) throw new Exception("Update should have created a different instance");

            res = LE.Update(value, LE.Cases, Expression.Empty());
            if (res == LE) throw new Exception("Update should have created a different instance");



            res = LE.Update(value, LE.Cases, def);
            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 24", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges24(EU.IValidator V) {
            var values = new Expression[] { Expression.Constant(1) };
            var body = Expression.Empty();

            var LE = Expression.SwitchCase(body, values);



            var res = LE.Update(values, body);
            if (res == LE) throw new Exception("Update should have created a different instance");

            res = LE.Update(LE.TestValues, Expression.Empty());
            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(LE.TestValues, body);
            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 25", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges25(EU.IValidator V) {

            var body = Expression.Empty();
            var fin = Expression.Empty();

            var handlers = new CatchBlock[] { Expression.Catch(typeof(int), Expression.Empty()) };

            var LE = Expression.TryCatchFinally(body, fin, handlers);



            var res = LE.Update(Expression.Empty(), LE.Handlers, fin, null);
            if (res == LE) throw new Exception("Update should have created a different instance");

            res = LE.Update(body, handlers, fin, null);
            if (res == LE) throw new Exception("Update should have created a different instance");

            res = LE.Update(body, LE.Handlers, Expression.Empty(), null);
            if (res == LE) throw new Exception("Update should have created a different instance");

            try {
                LE.Update(body, LE.Handlers, fin, Expression.Empty());
                throw new Exception("Update should have caused an exception since both finally and fault are defined");
            } catch {
            }



            res = LE.Update(body, LE.Handlers, fin, null);
            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 26", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges26(EU.IValidator V) {

            var value = Expression.Constant(new c1());
            var type = typeof(c1);

            var LE = Expression.TypeIs(value, type);



            var res = LE.Update(Expression.Constant(new c1()));
            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(value);
            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "VisitorChanges 27", new string[] { "positive", "comma", "visitor", "Pri1" })]
        public static Expr VisitorChanges27(EU.IValidator V) {

            var value = Expression.Constant(1);

            var LE = Expression.UnaryPlus(value);



            var res = LE.Update(Expression.Constant(1));
            if (res == LE) throw new Exception("Update should have created a different instance");


            res = LE.Update(value);
            if (res != LE) throw new Exception("Update should have returned the same instance");

            return Expression.Empty();
        }

    }


}


