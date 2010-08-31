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

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class FuncAction {
//Bug 87580
#if !SILVERLIGHT
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "FuncAction 1", new string[] { "positive", "Func", "Action", "miscellaneous", "Pri1" })]
        public static Expr FuncAction1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();


            //not entirely sure just using environment version will work, so keeping these around for future reference.
            /*var Assembly = typeof(Expression).Assembly.FullName;
            var pos = Assembly.IndexOf("Version=") + 8;
            Assembly = Assembly.Substring(pos);
            var version = Assembly.Substring(0, Assembly.IndexOf("."));*/
            //typeof(Expression).Assembly.FullName.Contains("Microsoft.Scripting.Core")

            //Console.WriteLine(System.Environment.Version.Major);
            //Console.WriteLine(typeof(Expression).Assembly.FullName);


            if (System.Environment.Version.Major >= 4) {
                for (int i = 0; i < 15; i++) {
                    var parms = new List<Type>();
                    for (int j = 0; j < i; j++) {
                        parms.Add(typeof(DateTime));
                    }

                    Expressions.Add(EU.GenAreEqual(Expr.Constant(true),
                         Expr.Constant(ValidateAction(Expression.GetActionType(parms.ToArray()))), "Action with " + i + "parameters"));
                    if (i > 0)
                        Expressions.Add(EU.GenAreEqual(Expr.Constant(true),
                             Expr.Constant(ValidateFunc(Expression.GetFuncType(parms.ToArray()))), "Func with " + i + "parameters"));
                }
            } else {
                Expressions.Add(Expr.Empty());
            }

            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }
#endif

        static bool ValidateFunc(Type arg) {
            var args = arg.GetGenericTypeDefinition().GetGenericArguments();

            int i;
            for (i = 0; i < args.Length - 1; i++) {
                if ((args[i].GenericParameterAttributes & System.Reflection.GenericParameterAttributes.Contravariant) == 0)
                    return false;
            }
            if ((args[i].GenericParameterAttributes & System.Reflection.GenericParameterAttributes.Covariant) == 0)
                return false;

            return true;
        }

        static bool ValidateAction(Type arg) {
            Type[] args;
            if (arg.IsGenericType) {
                args = arg.GetGenericTypeDefinition().GetGenericArguments();
            } else {
                args = new Type[] { };
            }


            int i;
            for (i = 0; i < args.Length; i++) {
                if ((args[i].GenericParameterAttributes & System.Reflection.GenericParameterAttributes.Contravariant) == 0)
                    return false;
            }

            return true;

        }

        static NullReferenceException FuncAction2_1(Exception arg1, Exception arg2, Exception arg3, Exception arg4, Exception arg5, Exception arg6) { return null; }
        static void FuncAction2_2(Exception arg1, Exception arg2, Exception arg3, Exception arg4, Exception arg5, Exception arg6, Exception arg7) { }
//InvalidFilterCriteriaException is inaccessible on Silverlight and InsufficientMemoryException does not exist
#if !SILVERLIGHT
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "FuncAction 2", new string[] { "positive", "Func", "Action", "miscellaneous", "Pri1" })]
        public static Expr FuncAction2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            if (System.Environment.Version.Major >= 4) {
                Expressions.Add(Expr.Convert(
                                     Expr.Constant(new Func<Exception, Exception, Exception, Exception, Exception, Exception, Exception>(FuncAction2_1)),
                                     typeof(Func<AccessViolationException, OverflowException, InvalidCastException, InvalidFilterCriteriaException, InsufficientMemoryException, IndexOutOfRangeException, Exception>)
                                 ));
                Expressions.Add(Expr.Convert(
                                     Expr.Constant(new Action<Exception, Exception, Exception, Exception, Exception, Exception, Exception>(FuncAction2_2)),
                                     typeof(Action<AccessViolationException, OverflowException, InvalidCastException, InvalidFilterCriteriaException, InsufficientMemoryException, IndexOutOfRangeException, NullReferenceException>)
                                 ));
            } else {
                Expressions.Add(Expr.Empty());
            }
            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "FuncAction 3", new string[] { "positive", "Func", "Action", "miscellaneous", "Pri1" })]
        public static Expr FuncAction3(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            if (System.Environment.Version.Major >= 4) {
                Expressions.Add(Expr.Condition(
                                     Expr.Constant(true),
                                     Expr.Constant(new Func<Exception, Exception, Exception, Exception, Exception, Exception, Exception>(FuncAction2_1)),
                                     Expr.Constant(new Func<Exception, Exception, Exception, Exception, Exception, Exception, Exception>(FuncAction2_1)),
                                     typeof(Func<AccessViolationException, OverflowException, InvalidCastException, InvalidFilterCriteriaException, InsufficientMemoryException, IndexOutOfRangeException, Exception>)
                                 ));
                Expressions.Add(Expr.Condition(
                                     Expr.Constant(true),
                                     Expr.Constant(new Action<Exception, Exception, Exception, Exception, Exception, Exception, Exception>(FuncAction2_2)),
                                     Expr.Constant(new Action<Exception, Exception, Exception, Exception, Exception, Exception, Exception>(FuncAction2_2)),
                                     typeof(Action<AccessViolationException, OverflowException, InvalidCastException, InvalidFilterCriteriaException, InsufficientMemoryException, IndexOutOfRangeException, NullReferenceException>)
                                 ));
            } else {
                Expressions.Add(Expr.Empty());
            }
            var tree = Expr.Block(Expressions);
            V.Validate(tree);
            return tree;
        }
#endif

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GetActionGetFunc1", new string[] { "positive", "TryGetActionType", "TryGetFuncType", "Pri1" }, Priority = 1)]
        public static Expr GetActionGetFunc1(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            for (int i = 0; i < 30; i++) {
                Type da, df;
                var ba = Expression.TryGetActionType(GetTypes(i), out da);
                var bf = Expression.TryGetFuncType(GetTypes(i + 1), out df);

                if (i < 17) {
                    if (ba != true) throw new Exception("TryGetActionType result for iteration " + i);
                    if (bf != true) throw new Exception("TryGetFuncType result for iteration " + i);

                    if (da == null) throw new Exception("TryGetActionType output delegate for iteration " + i);
                    if (df == null) throw new Exception("TryGetFuncType output delegate for iteration " + i);
                } else {
                    if (ba != false) throw new Exception("TryGetActionType result for iteration " + i);
                    if (bf != false) throw new Exception("TryGetFuncType result for iteration " + i);

                    if (da != null) throw new Exception("TryGetActionType output delegate for iteration " + i);
                    if (df != null) throw new Exception("TryGetFuncType output delegate for iteration " + i);
                }
            }
            var tree = Expression.Empty();
            V.Validate(tree);
            return tree;
        }

        public static Type[] GetTypes(int n) {
            var t = new Type[n];
            while (--n >= 0) {
                t[n] = typeof(string);
            }
            return t;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "GetActionGetFunc2", new string[] { "positive", "TryGetActionType", "TryGetFuncType", "Pri1" }, Priority = 1)]
        public static Expr GetActionGetFunc2(EU.IValidator V) {
            List<Expression> Expressions = new List<Expression>();

            Type da, df;
            try {
                Expression.TryGetActionType((Type[])null, out da);
                throw new Exception("Expected exception on TryGetActionType");
            } catch (ArgumentNullException) {
            }

            try {
                Expression.TryGetFuncType((Type[])null, out df);
                throw new Exception("Expected exception on TryGetFuncType");
            } catch (ArgumentNullException) {
            }

            var tree = Expression.Empty();
            V.Validate(tree);
            return tree;
        }
    }
}
