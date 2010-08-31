#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class DebugInfo {
        // pass all sorts of expressions to debug info.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 1", new string[] { "positive", "debuginfo", "miscellaneous", "Pri1" })]
        public static Expr DebugInfo1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("x");

            foreach (Expr x in EU.GetAllExpressions()) {
                Expressions.Add(EU.AddDebugInfo(x, sd, 1, 1, 2, 3));
            }

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }

        // pass null to filename
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 2", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr DebugInfo2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<ArgumentNullException>(() => { Expr.SymbolDocument(null); });

            return Expr.Empty();
        }

        // pass empty string to filename
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 3", new string[] { "positive", "debuginfo", "miscellaneous", "Pri1" })]
        public static Expr DebugInfo3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr.SymbolDocument("");

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }

        // pass a non existing file to filename
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 4", new string[] { "positive", "debuginfo", "miscellaneous", "Pri1" })]
        public static Expr DebugInfo4(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Expr.SymbolDocument("testxxx.xyz");

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }

        // pass a string longer than maxpath to filename
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 5", new string[] { "positive", "debuginfo", "miscellaneous", "Pri1" })]
        public static Expr DebugInfo5(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var t = new System.Text.StringBuilder();
            for (int i = 0; i < 500; i++) { t.Append("X"); }

            Expr.SymbolDocument(t.ToString());

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }


        // pass null to body
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 6", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr DebugInfo6(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("");
            EU.Throws<System.ArgumentNullException>(() =>
            {
                EU.AddDebugInfo(null, sd, 1, 1, 2, 3);
            });

            return Expr.Empty();
        }

        // pass null to document
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 7", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentNullException))]
        public static Expr DebugInfo7(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            EU.Throws<ArgumentNullException>(() => { EU.AddDebugInfo(Expr.Empty(), null, 1, 1, 2, 3); });

            return Expr.Empty();
        }

        // pass a negative number to startline
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 8", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentOutOfRangeException))]
        public static Expr DebugInfo8(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("");
            EU.Throws<System.ArgumentOutOfRangeException>(() =>
            {
                EU.AddDebugInfo(Expr.Empty(), sd, -1, 1, 2, 3);
            });

            return Expr.Empty();
        }

        // pass a negative number to startcolumn
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 9", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentOutOfRangeException))]
        public static Expr DebugInfo9(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("");
            EU.Throws<System.ArgumentOutOfRangeException>(() =>
            {
                EU.AddDebugInfo(Expr.Empty(), sd, 1, -1, 2, 3);
            });

            return Expr.Empty();
        }

        // pass a negative number to endline
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 10", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentOutOfRangeException))]
        public static Expr DebugInfo10(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("");
            EU.Throws<System.ArgumentOutOfRangeException>(() =>
            {
                EU.AddDebugInfo(Expr.Empty(), sd, 1, 1, -2, 3);
            });

            return Expr.Empty();
        }

        // pass a negative number to endcolumn
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 11", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentOutOfRangeException))]
        public static Expr DebugInfo11(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("");
            EU.Throws<System.ArgumentOutOfRangeException>(() =>
            {
                EU.AddDebugInfo(Expr.Empty(), sd, 1, 1, 2, -3);
            });

            return Expr.Empty();
        }


        // pass zero to startline
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 12", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentOutOfRangeException))]
        public static Expr DebugInfo12(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("");
            EU.Throws<System.ArgumentOutOfRangeException>(() =>
            {
                EU.AddDebugInfo(Expr.Empty(), sd, 0, 1, 2, 3);
            });

            return Expr.Empty();
        }

        // pass zero to startcolumn
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 13", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentOutOfRangeException))]
        public static Expr DebugInfo13(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("");
            EU.Throws<System.ArgumentOutOfRangeException>(() =>
            {
                EU.AddDebugInfo(Expr.Empty(), sd, 1, 0, 2, 3);
            });

            return Expr.Empty();
        }

        // pass zero to endline
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 14", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentOutOfRangeException))]
        public static Expr DebugInfo14(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("");
            EU.Throws<System.ArgumentOutOfRangeException>(() =>
            {
                EU.AddDebugInfo(Expr.Empty(), sd, 1, 1, 0, 3);
            });

            return Expr.Empty();
        }

        // pass zero to endcolumn
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 15", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentOutOfRangeException))]
        public static Expr DebugInfo15(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("");
            EU.Throws<System.ArgumentOutOfRangeException>(() =>
            {
                EU.AddDebugInfo(Expr.Empty(), sd, 1, 1, 2, 0);
            });

            return Expr.Empty();
        }

        // pass endline smaller than startline
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 16", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr DebugInfo16(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("");
            EU.Throws<System.ArgumentException>(() =>
            {
                EU.AddDebugInfo(Expr.Empty(), sd, 5, 1, 2, 1);
            });

            return Expr.Empty();
        }

        // pass endcolumn smaller than startcolumn
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 17", new string[] { "negative", "debuginfo", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr DebugInfo17(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("");
            EU.Throws<System.ArgumentException>(() =>
            {
                EU.AddDebugInfo(Expr.Empty(), sd, 1, 5, 1, 1);
            });

            return Expr.Empty();
        }

        // pass same numbers for all positions
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 18", new string[] { "positive", "debuginfo", "miscellaneous", "Pri1" })]
        public static Expr DebugInfo18(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            var sd = Expr.SymbolDocument("");
            EU.AddDebugInfo(Expr.Empty(), sd, 1, 1, 1, 1);

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }


        // pass same numbers for all positions
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "DebugInfo 19", new string[] { "positive", "debuginfo", "miscellaneous", "Pri1" })]
        public static Expr DebugInfo19(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();
            var g1 = Guid.NewGuid();
            var g2 = Guid.NewGuid();
            var g3 = Guid.NewGuid();
            var txtGuid = new Guid(0x5a869d0b, 0x6611, 0x11d3, 0xbd, 0x2a, 0, 0, 0xf8, 8, 0x49, 0xbd);

            var sd1 = Expression.SymbolDocument("blahville");
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd1.FileName), Expression.Constant("blahville")));
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd1.Language), Expression.Constant(Guid.Empty)));
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd1.LanguageVendor), Expression.Constant(Guid.Empty)));
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd1.DocumentType), Expression.Constant(txtGuid)));

            var sd2 = Expression.SymbolDocument("blahville", g1);
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd2.FileName), Expression.Constant("blahville")));
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd2.Language), Expression.Constant(g1)));
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd2.LanguageVendor), Expression.Constant(Guid.Empty)));
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd2.DocumentType), Expression.Constant(txtGuid)));

            var sd3 = Expression.SymbolDocument("blahville", g1, g2);
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd3.FileName), Expression.Constant("blahville")));
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd3.Language), Expression.Constant(g1)));
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd3.LanguageVendor), Expression.Constant(g2)));
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd3.DocumentType), Expression.Constant(txtGuid)));

            var sd4 = Expression.SymbolDocument("blahville", g1, g2, g3);
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd4.FileName), Expression.Constant("blahville")));
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd4.Language), Expression.Constant(g1)));
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd4.LanguageVendor), Expression.Constant(g2)));
            Expressions.Add(EU.GenAreEqual(Expression.Constant(sd4.DocumentType), Expression.Constant(g3)));

            var tree = EU.BlockVoid(Expressions);
            V.Validate(tree);
            return tree;
        }
    }
}
