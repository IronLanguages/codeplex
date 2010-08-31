#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.ObjectInstantiation {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
        
    public class New {
        public class TestClass {
            public string Data { get; set; }
            public TestClass(object o) {
                Data = o.ToString();
            }
        }
        // Pass arguments of a derived type of the constructor arguments
        // Regression for Dev10 Bug 555277
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "New 1", new string[] { "positive", "new", "new", "Pri2" })]
        public static Expr New1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            ConstructorInfo ci = typeof(TestClass).GetConstructor(new Type[] { typeof(object) });
            Expression[] args = new Expression[] { Expr.Constant("ArgValue") };
            MemberInfo[] memInfo = typeof(TestClass).GetMember("Data");
            Expr Result = Expr.New(ci, args, memInfo);

            Expressions.Add(EU.GenAreEqual(Expr.Constant("ArgValue"), Expr.Property(Result, "Data")));

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        public class CMisc2 {
            public CMisc2(Exception member) {
            }
            public Exception member {
                get { return null; }
                set { }
            }
        }

        //regression for bug #467900
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "New 2", new string[] { "positive", "New", "Pri1", "regression" })]
        public static Expr New2(EU.IValidator V) {

            //create instance of CMisc2 with ArithmeticException. no error should occur.
            ConstructorInfo CM2New = typeof(CMisc2).GetConstructor(new Type[] { typeof(Exception) });
            MemberInfo CM2Member = typeof(CMisc2).GetMember("member")[0];
            Expr.New(CM2New, new Expression[] { Expr.Constant(new ArithmeticException()) }, CM2Member);

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }

        public class CMisc3 {
            public CMisc3(ArithmeticException member) {
            }

            public ArithmeticException member;
        }

        //regression for bug #467900
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "New 3", new string[] { "negative", "New", "Pri1", "regression" }, Exception = typeof(ArgumentException))]
        public static Expr New3(EU.IValidator V) {

            //create instance of CMisc# with Exception. An error should occur.
            ConstructorInfo CM3New = typeof(CMisc3).GetConstructor(new Type[] { typeof(ArithmeticException) });
            MemberInfo CM3Member = typeof(CMisc3).GetMember("member")[0];
            EU.Throws<ArgumentException>(() => { Expr.New(CM3New, new Expression[] { Expr.Constant(new Exception()) }, CM3Member); });

            return Expr.Empty();
        }
    }
}
