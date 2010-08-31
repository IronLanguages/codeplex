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
using System.Runtime.CompilerServices;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {
    internal class TestVisitor : ExpressionVisitor {
        protected override Expression VisitLambda<T>(Expression<T> node) {
            return Expression.Lambda<T>(node.Body, node.Name, node.TailCall, node.Parameters);
        }
    }

    public static partial class Scenarios {
        private class VisitAndConvertTest : ExpressionVisitor {
            protected override Expression VisitParameter(ParameterExpression node) {
                return Expression.Default(node.Type);
            }
        }

        public static void Negative_VisitAndConvert(EU.IValidator V) {
            var visitor = new VisitAndConvertTest();
            var x = Expression.Parameter(typeof(Exception), "x");

            Expression test = Expression.Block(new[] { x }, Expression.Empty());
            EU.Throws<InvalidOperationException>(() => visitor.Visit(test));

            test = Expression.TryCatch(Expression.Empty(), Expression.Catch(x, Expression.Empty()));
            EU.Throws<InvalidOperationException>(() => visitor.Visit(test));
        }

        private class VisitChangeChildType : ExpressionVisitor {
            internal Type ChangeToType;

            protected override Expression VisitParameter(ParameterExpression node) {
                return Expression.Parameter(ChangeToType, node.Name);
            }
        }

        public static void Negative_VisitChangeChildType(EU.IValidator V) {
            var visitor = new VisitChangeChildType { ChangeToType = typeof(double) };
            var x = Expression.Parameter(typeof(int), "x");

            Expression test = Expression.Add(x, x);
            EU.Throws<InvalidOperationException>(() => visitor.Visit(test));

            test = Expression.UnaryPlus(x);
            EU.Throws<InvalidOperationException>(() => visitor.Visit(test));
        }

#if !SILVERLIGHT
        public static void Positive_VisitChangeChildType(EU.IValidator V) {
            var visitor = new VisitChangeChildType { ChangeToType = typeof(string) };
            var x = Expression.Parameter(typeof(ICloneable), "x");

            visitor.Visit(Expression.TypeAs(x, typeof(object)));
            visitor.Visit(Expression.Convert(x, typeof(object)));
            visitor.Visit(Expression.TypeEqual(x, typeof(object)));
            visitor.Visit(Expression.TypeIs(x, typeof(object)));
            visitor.Visit(Expression.Equal(x, Expression.Constant("hello")));
            visitor.Visit(Expression.NotEqual(x, Expression.Constant(null, typeof(IEnumerable<char>))));
        }
#endif

        public static void Positive_VisitPrivateDelegate(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var e = Expression.Lambda<PrivateDelegate>(
                Expression.Add(
                    Expression.Constant(123),
                    Expression.Block(
                        Expression.TryFinally(
                            Expression.Assign(x, Expression.Add(x, Expression.Constant(111))),
                            Expression.Assign(x, Expression.Add(x, Expression.Constant(222)))
                        ),
                        x
                    )
                ),
                x
            );

            var e2 = (Expression<PrivateDelegate>)new TestVisitor().Visit(e);

            EU.Equal(e == e2, false); // should have rewritten

            V.Validate(e2, f =>
            {
                EU.Equal(f(0), 456);
                EU.Equal(f(321), 777);
            });
        }

        internal class InvokeVisitor : ExpressionVisitor {
            protected override Expression VisitConstant(ConstantExpression node) {
                if (node.Type == typeof(Func<int, int, int>)) {
                    // override only the target, but not args.
                    return Expression.Constant((Func<int, int, int>)Math.Max);
                }
                return node;
            }
        }

        public static void Positive_VisitInvocation(EU.IValidator V) {
            var lambda = Expression.Lambda<Func<int>>(
                Expression.Invoke(
                    Expression.Constant((Func<int, int, int>)Math.Max),
                    Expression.Constant(2),
                    Expression.Constant(5)
                )
            );

            lambda = (Expression<Func<int>>)new InvokeVisitor().Visit(lambda);
            V.Validate(lambda, r =>
            {
                // Max(2, 5) == 5
                EU.Equal(r(), 5);
            });
        }

        internal class IndexerVisitor : ExpressionVisitor {
            protected override Expression VisitConstant(ConstantExpression node) {
                if (node.Type == typeof(Dictionary<string, int>)) {
                    // override only the target, but not args.
                    var dict = new Dictionary<string, int>();
                    dict["Hello"] = 5;
                    return Expression.Constant(dict);
                }
                return node;
            }
        }

        public static void Positive_VisitIndex(EU.IValidator V) {
            var lambda = Expression.Lambda<Func<int>>(
                Expression.Property(
                    Expression.Constant(null, typeof(Dictionary<string, int>)),
                    typeof(Dictionary<string, int>).GetProperty("Item"),
                    Expression.Constant("Hello")
                )
            );

            lambda = (Expression<Func<int>>)new IndexerVisitor().Visit(lambda);
            V.Validate(lambda, r =>
            {
                EU.Equal(r(), 5);
            });
        }

        internal class ChangeTypeVisitor : ExpressionVisitor {
            Expression _change;
            internal ChangeTypeVisitor(Expression change) {
                _change = change;
            }
            protected override Expression VisitConstant(ConstantExpression node) {
                if ((object)node == (object)_change) {
                    return Expression.Constant("int");
                }
                return node;
            }
        }

        internal class VisitorTestBinder : CallSiteBinder {
            public override Expression Bind(object[] args, System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                throw new NotImplementedException();
            }
        }

        private static void CheckBadRewriteException(Expression[] args, int max, Expression visit) {
            for (int i = 0; i < max; i++) {
                EU.Throws<ArgumentException>(
                    delegate {
                        new ChangeTypeVisitor(args[i]).Visit(visit);
                    }
                );
            }
        }

        public static void Negative_VisitChangeTypeDynamic(EU.IValidator V) {
            var args = new Expression[] {
                Expression.Constant(1),
                Expression.Constant(2),
                Expression.Constant(3),
                Expression.Constant(4),
                Expression.Constant(5),
                Expression.Constant(6)
            };
            var binder = new VisitorTestBinder();

            foreach (var t in new[] { typeof(void), typeof(object) }) {
                CheckBadRewriteException(args, 1, Expression.Dynamic(binder, t, args[0]));
                CheckBadRewriteException(args, 2, Expression.Dynamic(binder, t, args[0], args[1]));
                CheckBadRewriteException(args, 3, Expression.Dynamic(binder, t, args[0], args[1], args[2]));
                CheckBadRewriteException(args, 4, Expression.Dynamic(binder, t, args[0], args[1], args[2], args[3]));
                CheckBadRewriteException(args, 5, Expression.Dynamic(binder, t, args[0], args[1], args[2], args[3], args[4]));
                CheckBadRewriteException(args, 6, Expression.Dynamic(binder, t, args));
            }
        }

        public class VisitChangeTypeMethodCallTest {
            public static void StaticVoid0() { }
            public static void StaticVoid1(int a1) { }
            public static void StaticVoid2(int a1, int a2) { }
            public static void StaticVoid3(int a1, int a2, int a3) { }
            public static void StaticVoid4(int a1, int a2, int a3, int a4) { }
            public static void StaticVoid5(int a1, int a2, int a3, int a4, int a5) { }
            public static void StaticVoid6(int a1, int a2, int a3, int a4, int a5, int a6) { }

            public void InstanceVoid0() { }
            public void InstanceVoid1(int a1) { }
            public void InstanceVoid2(int a1, int a2) { }
            public void InstanceVoid3(int a1, int a2, int a3) { }
            public void InstanceVoid4(int a1, int a2, int a3, int a4) { }
            public void InstanceVoid5(int a1, int a2, int a3, int a4, int a5) { }
            public void InstanceVoid6(int a1, int a2, int a3, int a4, int a5, int a6) { }

            public static int StaticInt0() { return 0; }
            public static int StaticInt1(int a1) { return 0; }
            public static int StaticInt2(int a1, int a2) { return 0; }
            public static int StaticInt3(int a1, int a2, int a3) { return 0; }
            public static int StaticInt4(int a1, int a2, int a3, int a4) { return 0; }
            public static int StaticInt5(int a1, int a2, int a3, int a4, int a5) { return 0; }
            public static int StaticInt6(int a1, int a2, int a3, int a4, int a5, int a6) { return 0; }

            public int InstanceInt0() { return 0; }
            public int InstanceInt1(int a1) { return 0; }
            public int InstanceInt2(int a1, int a2) { return 0; }
            public int InstanceInt3(int a1, int a2, int a3) { return 0; }
            public int InstanceInt4(int a1, int a2, int a3, int a4) { return 0; }
            public int InstanceInt5(int a1, int a2, int a3, int a4, int a5) { return 0; }
            public int InstanceInt6(int a1, int a2, int a3, int a4, int a5, int a6) { return 0; }
        }

        public static void Negative_VisitChangeTypeMethodCall(EU.IValidator V) {
            var args = new Expression[] {
                Expression.Constant(1),
                Expression.Constant(2),
                Expression.Constant(3),
                Expression.Constant(4),
                Expression.Constant(5),
                Expression.Constant(6)
            };
            Expression instance = Expression.Constant(null, typeof(VisitChangeTypeMethodCallTest));
            var argswi = new Expression[args.Length + 1];
            argswi[0] = instance;
            Array.Copy(args, 0, argswi, 1, args.Length);
            Func<string, MethodInfo> gm = typeof(VisitChangeTypeMethodCallTest).GetMethod;

            foreach (var type in new[] { "Int", "Void" }) {
                // Static
                CheckBadRewriteException(args, 1, Expression.Call(gm("Static" + type + 1), args[0]));
                CheckBadRewriteException(args, 2, Expression.Call(gm("Static" + type + 2), args[0], args[1]));
                CheckBadRewriteException(args, 3, Expression.Call(gm("Static" + type + 3), args[0], args[1], args[2]));
                CheckBadRewriteException(args, 4, Expression.Call(gm("Static" + type + 4), args[0], args[1], args[2], args[3]));
                CheckBadRewriteException(args, 5, Expression.Call(gm("Static" + type + 5), args[0], args[1], args[2], args[3], args[4]));
                CheckBadRewriteException(args, 6, Expression.Call(gm("Static" + type + 6), args[0], args[1], args[2], args[3], args[4], args[5]));

                // Instance
                CheckBadRewriteException(argswi, 1, Expression.Call(instance, gm("Instance" + type + 0)));
                CheckBadRewriteException(argswi, 2, Expression.Call(instance, gm("Instance" + type + 1), args[0]));
                CheckBadRewriteException(argswi, 3, Expression.Call(instance, gm("Instance" + type + 2), args[0], args[1]));
                CheckBadRewriteException(argswi, 4, Expression.Call(instance, gm("Instance" + type + 3), args[0], args[1], args[2]));
                CheckBadRewriteException(argswi, 5, Expression.Call(instance, gm("Instance" + type + 4), args[0], args[1], args[2], args[3]));
                CheckBadRewriteException(argswi, 6, Expression.Call(instance, gm("Instance" + type + 5), args[0], args[1], args[2], args[3], args[4]));
                CheckBadRewriteException(argswi, 7, Expression.Call(instance, gm("Instance" + type + 6), args[0], args[1], args[2], args[3], args[4], args[5]));
            }
        }

        public static void Positive_VisitChangeTypeConditionalWithVoid(EU.IValidator V) {
            var conditional = Expression.Condition(
                Expression.Constant(true),
                Expression.Constant(1),
                Expression.Constant(2),
                typeof(void)
            );

            // Rewrite and run
            var e1 = Expression.Lambda<Action>(
                new ChangeTypeVisitor(conditional.IfTrue).Visit(conditional)
            );
            V.Validate(e1);

            var e2 = Expression.Lambda<Action>(
                new ChangeTypeVisitor(conditional.IfFalse).Visit(conditional)
            );
            V.Validate(e2);
        }

        public static void Positive_VisitChangeTypeBlockWithVoid(EU.IValidator V) {
            var one = Expression.Constant(1);
            var two = Expression.Constant(1);
#if SILVERLIGHT
            var ht = Expression.Constant(new System.Collections.Generic.Dictionary<Object,Object>());
#else
            var ht = Expression.Constant(new System.Collections.Hashtable());
#endif
            var block = Expression.Block(
                typeof(void),
                Expression.Constant(true),
                one,
                two,
                ht
            );

            Validate_Positive_VisitChangeTypeBlockWithVoid(
                new ChangeTypeVisitor(one).Visit(block) as BlockExpression, V
            );

            Validate_Positive_VisitChangeTypeBlockWithVoid(
                new ChangeTypeVisitor(two).Visit(block) as BlockExpression, V
            );

            Validate_Positive_VisitChangeTypeBlockWithVoid(
                new ChangeTypeVisitor(ht).Visit(block) as BlockExpression, V
            );
        }

        private static void Validate_Positive_VisitChangeTypeBlockWithVoid(BlockExpression block, EU.IValidator V) {
            if (block == null) {
                throw new InvalidOperationException("Expected block");
            }

            if (block.Expressions[block.Expressions.Count - 1].Type == block.Type) {
                throw new InvalidOperationException("Expected different types on rewrite");
            }

            V.Validate(block);
        }

        public sealed class ReducingVisitor : ExpressionVisitor {
            protected override Expression VisitExtension(Expression node) {
                return Visit(node.ReduceExtensions());
            }
        }

        public static void Positive_TypeEqual_Visit(EU.IValidator V) {
            var x = Expression.Parameter(typeof(Exception), "x");
            var e = Expression.Lambda<Func<Exception, bool>>(
                new ReducingVisitor().Visit(
                    Expression.TypeEqual(new TransparentReducible(x), typeof(Exception))
                ),
                x
            );
            
            V.Validate(e, f =>
            {
                EU.Equal(f(new Exception()), true);
                EU.Equal(f(new NotImplementedException()), false);
            });
        }

        class AllNodeVisitor : ExpressionVisitor {
            public List<ExpressionType> Kinds = new List<ExpressionType>();
            public override Expression Visit(Expression node) {
                Kinds.Add(node.NodeType);
                return base.Visit(node);
            }
        }

        public static void Positive_VisitAllNodes(EU.IValidator V) {
            var visitor = new AllNodeVisitor();
            visitor.Visit(
                Expression.Add(
                    Expression.Subtract(
                        Expression.Constant(1),
                        Expression.Parameter(typeof(int), "x")
                    ),
                    Expression.Not(Expression.Default(typeof(int)))
                )
            );
            EU.ArrayEqual(visitor.Kinds.ToArray(), new[] {
                ExpressionType.Add,
                ExpressionType.Subtract,
                ExpressionType.Constant,
                ExpressionType.Parameter,
                ExpressionType.Not,
                ExpressionType.Default
            });
        }

        class ReplaceParameterVisitor : ExpressionVisitor {
            private readonly Dictionary<ParameterExpression, ParameterExpression> _map = new Dictionary<ParameterExpression, ParameterExpression>();

            protected override Expression VisitParameter(ParameterExpression node) {
                ParameterExpression result;
                if (_map.TryGetValue(node, out result)) {
                    return result;
                }
                return _map[node] = Expression.Parameter(node.IsByRef ? node.Type.MakeByRefType() : node.Type, node.Name);
            }
        }

        public static void Positive_VisitLambdaTailCall(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var lambda = Expression.Lambda<Func<int, int>>(Expression.Add(x, x), false, x);
            var tailLambda = Expression.Lambda<Func<int, int>>(Expression.Add(x, x), true, x);

            var visit = new ReplaceParameterVisitor();
            var newLambda = (Expression<Func<int, int>>)visit.Visit(lambda);
            var newTailLambda = (Expression<Func<int, int>>)visit.Visit(tailLambda);

            EU.ReferenceNotEqual(newLambda, lambda);
            EU.ReferenceNotEqual(newTailLambda, tailLambda);
            EU.Equal(newLambda.TailCall, false);
            EU.Equal(newTailLambda.TailCall, true);
        }

        public static void Negative_UpdateBlock(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(string), "y");
            var node = Expression.Block(new[] { x, y }, new[] { x, y });
            var node2 = node.Update(node.Variables, node.Expressions);
            EU.ReferenceEqual(node, node2);
            node2 = node.Update(node.Variables, new ReadOnlyCollectionBuilder<Expression>(node.Expressions));
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);
            node2 = node.Update(new ReadOnlyCollectionBuilder<ParameterExpression>(node.Variables), node.Expressions);
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);
            node2 = node.Update(new[] { x, y }, new[] { x, y });
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);


            // Some negative tests. Ultimately these just test the factory
            EU.Throws<ArgumentException>(() => node.Update(new[] { x, y }, new[] { y, x }));
            EU.Throws<ArgumentNullException>(() => node.Update(new[] { x }, null));
            EU.Throws<ArgumentNullException>(() => node.Update(new[] { x, null }, new[] { x, y }));
            EU.Throws<ArgumentNullException>(() => node.Update(new[] { x, y }, new[] { null, y }));
            EU.Throws<ArgumentException>(() => node.Update(new[] { x }, new Expression[0]));
            EU.Throws<ArgumentException>(() => node.Update(new[] { x, x }, new[] { x, y }));
        }

        public static void Negative_UpdateDynamic(EU.IValidator V) {
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(string), "y");
            var node = Expression.Dynamic(new TestCreateInstance(), typeof(object), new[] { x, y });
            var node2 = node.Update(node.Arguments);
            EU.ReferenceEqual(node, node2);
            node2 = node.Update(new ReadOnlyCollectionBuilder<Expression>(node.Arguments));
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);
            node2 = node.Update(new[] { x, y });
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);

            // Some negative tests. Ultimately these just test the factory
            EU.Throws<ArgumentException>(() => node.Update(new[] { y, x }));
            EU.Throws<ArgumentException>(() => node.Update(null));
            EU.Throws<ArgumentNullException>(() => node.Update(new[] { null, y }));
            EU.Throws<ArgumentException>(() => node.Update(new Expression[0]));
        }

        public static void Negative_UpdateInvocation(EU.IValidator V) {
            var x = Expression.Parameter(typeof(Func<string, int, double>), "x");
            var y = Expression.Parameter(typeof(string), "y");
            var z = Expression.Parameter(typeof(int), "z");
            var node = Expression.Invoke(x, new[] { y, z });
            var node2 = node.Update(node.Expression, node.Arguments);
            EU.ReferenceEqual(node, node2);
            node2 = node.Update(x, new ReadOnlyCollectionBuilder<Expression>(node.Arguments));
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);
            EU.ReferenceEqual(node.Expression, node2.Expression);
            node2 = node.Update(x, new[] { y, z });
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);
            node2 = node.Update(Expression.Parameter(x.Type, "x2"), node.Arguments);
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);
            EU.ReferenceEqual(node.Arguments, node2.Arguments);

            // Some negative tests. Ultimately these just test the factory
            EU.Throws<ArgumentException>(() => node.Update(x, new[] { z, y }));
            EU.Throws<ArgumentNullException>(() => node.Update(null, new[] { x, y }));
            EU.Throws<ArgumentNullException>(() => node.Update(x, new[] { null, y }));
            EU.Throws<InvalidOperationException>(() => node.Update(x, new Expression[0]));
            EU.Throws<InvalidOperationException>(() => node.Update(x, null));
        }

        public static void Negative_UpdateMethodCall(EU.IValidator V) {
            var x = Expression.Parameter(typeof(Func<string, int, double>), "x");
            var y = Expression.Parameter(typeof(string), "y");
            var z = Expression.Parameter(typeof(int), "z");
            var node = Expression.Call(x, x.Type.GetMethod("Invoke"), new[] { y, z });
            var node2 = node.Update(node.Object, node.Arguments);
            EU.ReferenceEqual(node, node2);
            node2 = node.Update(x, new ReadOnlyCollectionBuilder<Expression>(node.Arguments));
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);
            EU.ReferenceEqual(node.Object, node2.Object);
            node2 = node.Update(x, new[] { y, z });
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);
            node2 = node.Update(Expression.Parameter(x.Type, "x2"), node.Arguments);
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);
            EU.ReferenceEqual(node.Arguments, node2.Arguments);

            // Some negative tests. Ultimately these just test the factory
            EU.Throws<ArgumentException>(() => node.Update(x, new[] { z, y }));
            EU.Throws<ArgumentException>(() => node.Update(null, new[] { x, y }));
            EU.Throws<ArgumentNullException>(() => node.Update(x, new[] { null, y }));
            EU.Throws<ArgumentException>(() => node.Update(x, new Expression[0]));
            EU.Throws<ArgumentException>(() => node.Update(x, null));
        }

        public class MyIndexer {
            public double this[string x, int y] {
                get { throw new NotImplementedException("MyIndexer"); }
            }
        }

        public static void Negative_UpdateIndex(EU.IValidator V) {
            var x = Expression.Parameter(typeof(MyIndexer), "x");
            var y = Expression.Parameter(typeof(string), "y");
            var z = Expression.Parameter(typeof(int), "z");
            var node = Expression.Property(x, x.Type.GetProperty("Item"), new[] { y, z });
            var node2 = node.Update(node.Object, node.Arguments);
            EU.ReferenceEqual(node, node2);
            node2 = node.Update(x, new ReadOnlyCollectionBuilder<Expression>(node.Arguments));
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);
            EU.ReferenceEqual(node.Object, node2.Object);
            node2 = node.Update(x, new[] { y, z });
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);
            node2 = node.Update(Expression.Parameter(x.Type, "x2"), node.Arguments);
            EU.ReferenceNotEqual(node, node2);
            EU.ReferenceEqual(node.Type, node2.Type);
            EU.ReferenceEqual(node.Arguments, node2.Arguments);

            // Some negative tests. Ultimately these just test the factory
            EU.Throws<ArgumentException>(() => node.Update(x, new[] { z, y }));
            EU.Throws<ArgumentException>(() => node.Update(null, new[] { x, y }));
            EU.Throws<ArgumentNullException>(() => node.Update(x, new[] { null, y }));
            EU.Throws<ArgumentException>(() => node.Update(x, new Expression[0]));
            EU.Throws<ArgumentException>(() => node.Update(x, null));
        }
    }
}
#endif
