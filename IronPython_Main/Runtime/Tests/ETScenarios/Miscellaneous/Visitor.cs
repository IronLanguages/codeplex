#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ETScenarios.Miscellaneous {
    using EU = ETUtils.ExpressionUtils;
    using Expr = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class Visitor {

        public static int Foo(int arg1, int arg2) {
            return 0;
        }

        public delegate int Del(int arg1, int arg2);


        //Delegate constant replacement
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Visitor 1", new string[] { "positive", "Visitor", "miscellaneous", "Pri1" })]
        public static Expr Visitor1(EU.IValidator V) {
            Del d1 = Foo;
            Expression d = Expression.Constant(d1, typeof(Del));
            InvocationExpression i = Expression.Invoke(d, new Expression[] { Expression.Constant(1, typeof(int)), Expression.Constant(1, typeof(int)) });
            var v = new Visit();
            var tree = v.Visit(i);
            V.Validate(tree);
            return tree;
        }
        public class Visit : ExpressionVisitor {
            protected override Expression VisitConstant(ConstantExpression node) {
                if (node.Type == typeof(Del)) {
                    Del d1 = Foo;
                    Expression d = Expression.Constant(d1, typeof(Del));
                    return d;
                }
                return base.VisitConstant(node);
            }
        }

        //Visit all, do nothing
        public class VIsitAll : ExpressionVisitor {
            public List<String> Visited = new List<String>();

            //covered
            protected override Expression VisitBinary(BinaryExpression node) {
                Visited.Add("VisitBinary");
                return base.VisitBinary(node);
            }

            //covered
            protected override Expression VisitBlock(BlockExpression node) {
                Visited.Add("VisitBlock");
                return base.VisitBlock(node);
            }

            //covered
            protected override CatchBlock VisitCatchBlock(CatchBlock node) {
                Visited.Add("VisitCatchBlock");
                return base.VisitCatchBlock(node);
            }

            //covered
            protected override Expression VisitConditional(ConditionalExpression node) {
                Visited.Add("VisitConditional");
                return base.VisitConditional(node);
            }

            //covered
            protected override Expression VisitConstant(ConstantExpression node) {
                Visited.Add("VisitConstant");
                return base.VisitConstant(node);
            }

            protected override Expression VisitDebugInfo(DebugInfoExpression node) {
                Visited.Add("VisitDebugInfo");
                return base.VisitDebugInfo(node);
            }

            //covered
            protected override Expression VisitDefault(DefaultExpression node) {
                Visited.Add("VisitDefault");
                return base.VisitDefault(node);
            }

            //covered
            protected override Expression VisitDynamic(DynamicExpression node) {
                Visited.Add("VisitDynamic");
                return base.VisitDynamic(node);
            }

            //covered
            protected override ElementInit VisitElementInit(ElementInit node) {
                Visited.Add("VisitElementInit");
                return base.VisitElementInit(node);
            }

            //covered
            protected override Expression VisitExtension(Expression node) {
                Visited.Add("VisitExtension");
                return base.VisitExtension(node);
            }

            //covered
            protected override Expression VisitGoto(GotoExpression node) {
                Visited.Add("VisitGoto");
                return base.VisitGoto(node);
            }

            //covered
            protected override Expression VisitIndex(IndexExpression node) {
                Visited.Add("VisitIndex");
                return base.VisitIndex(node);
            }

            //covered
            protected override Expression VisitInvocation(InvocationExpression node) {
                Visited.Add("VisitInvocation");
                return base.VisitInvocation(node);
            }

            //covered
            protected override Expression VisitLabel(LabelExpression node) {
                Visited.Add("VisitLabel");
                return base.VisitLabel(node);
            }

            //covered
            protected override LabelTarget VisitLabelTarget(LabelTarget node) {
                Visited.Add("VisitLabelTarget");
                return base.VisitLabelTarget(node);
            }

            //covered
            protected override Expression VisitLambda<T>(Expression<T> node) {
                Visited.Add("VisitLambda<T>");
                return base.VisitLambda<T>(node);
            }

            //covered
            protected override Expression VisitListInit(ListInitExpression node) {
                Visited.Add("VisitListInit");
                return base.VisitListInit(node);
            }

            //covered
            protected override Expression VisitLoop(LoopExpression node) {
                Visited.Add("VisitLoop");
                return base.VisitLoop(node);
            }

            //covered
            protected override Expression VisitMember(MemberExpression node) {
                Visited.Add("VisitMember");
                return base.VisitMember(node);
            }

            //covered
            protected override MemberAssignment VisitMemberAssignment(MemberAssignment node) {
                Visited.Add("VisitMemberAssignment");
                return base.VisitMemberAssignment(node);
            }

            //covered
            protected override MemberBinding VisitMemberBinding(MemberBinding node) {
                Visited.Add("VisitMemberBinding");
                return base.VisitMemberBinding(node);
            }

            //covered
            protected override Expression VisitMemberInit(MemberInitExpression node) {
                Visited.Add("VisitMemberInit");
                return base.VisitMemberInit(node);
            }

            //covered
            protected override MemberListBinding VisitMemberListBinding(MemberListBinding node) {
                Visited.Add("VisitMemberListBinding");
                return base.VisitMemberListBinding(node);
            }

            //covered
            protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node) {
                Visited.Add("VisitMemberMemberBinding");
                return base.VisitMemberMemberBinding(node);
            }

            //covered
            protected override Expression VisitMethodCall(MethodCallExpression node) {
                Visited.Add("VisitMethodCall");
                return base.VisitMethodCall(node);
            }

            //covered
            protected override Expression VisitNew(NewExpression node) {
                Visited.Add("VisitNew");
                return base.VisitNew(node);
            }

            //covered
            protected override Expression VisitNewArray(NewArrayExpression node) {
                Visited.Add("VisitNewArray");
                return base.VisitNewArray(node);
            }

            //covered
            protected override Expression VisitParameter(ParameterExpression node) {
                Visited.Add("VisitParameter");
                return base.VisitParameter(node);
            }

            //covered
            protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
                Visited.Add("VisitRuntimeVariables");
                return base.VisitRuntimeVariables(node);
            }

            //covered
            protected override Expression VisitSwitch(SwitchExpression node) {
                Visited.Add("VisitSwitch");
                return base.VisitSwitch(node);
            }

            //covered
            protected override SwitchCase VisitSwitchCase(SwitchCase node) {
                Visited.Add("VisitSwitchCase");
                return base.VisitSwitchCase(node);
            }

            //covered
            protected override Expression VisitTry(TryExpression node) {
                Visited.Add("VisitTry");
                return base.VisitTry(node);
            }

            //covered
            protected override Expression VisitTypeBinary(TypeBinaryExpression node) {
                Visited.Add("VisitTypeBinary");
                return base.VisitTypeBinary(node);
            }

            //covered
            protected override Expression VisitUnary(UnaryExpression node) {
                Visited.Add("VisitUnary");
                return base.VisitUnary(node);
            }



        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Visitor 2", new string[] { "positive", "Visitor", "miscellaneous", "Pri1" })]
        public static Expr Visitor2(EU.IValidator V) {
            //Visit: binary, try, catch block, block, lambda<T>, parameter, Constant

            var visitor = new VIsitAll();

            //ten nodes total.
            var param = Expr.Parameter(typeof(int), "");
            var Exp = Expr.Lambda(
                    Expr.Block(
                        Expr.Add(Expr.Constant(1), Expr.Constant(2)),
                        Expr.TryCatch(
                            Expr.Constant(1),
                            new CatchBlock[]{
                                Expr.Catch(typeof(Exception),Expr.Constant(1))
                            }
                        )
                    ),
                    param
                );

            var NewExp = visitor.Visit(Exp);


            var exprs = new List<Expression>();

            exprs.Add(EU.GenAreEqual(Expr.Constant(10), Expr.Constant(visitor.Visited.Count), "Expected 10 nodes to be visited"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitBinary")), "visited Binary?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitTry")), "visited Try?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitCatchBlock")), "visited catch block?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitBlock")), "visited block?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitLambda<T>")), "visited lambda?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitParameter")), "visited parameter?"));

            var tree = Expr.Block(exprs);
            V.Validate(tree);
            return tree;
        }

        //condition default Dynamic ElementInit ListBind
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Visitor 3", new string[] { "positive", "Visitor", "miscellaneous", "Pri1" })]
        public static Expr Visitor3(EU.IValidator V) {
            //Visit: condition default Dynamic ElementInit ListInit constant block parameter new 

            var visitor = new VIsitAll();


            //13 nodes total.
            //var param = Expr.Parameter(typeof(int), "");
            var list = Expr.Parameter(typeof(List<int>), "");
            var Exp = Expr.Condition(
                    Expr.Default(typeof(bool)),
                    Expr.Dynamic(new DynamicNode.MyBinder(), typeof(int), Expr.Constant(1)),
                    Expr.Block(
                        new[] { list },
                        Expr.Assign(list, Expr.Constant(new List<int>())),
                        Expr.ListInit(
                            Expr.New(
                                typeof(List<int>).GetConstructor(new Type[] { })
                            ),
                            Expr.ElementInit(
                                            typeof(List<int>).GetMethod("Add"),
                                            Expression.Constant(1)
                            )
                        ),
                        Expr.Constant(1)
                    )
                );

            var NewExp = visitor.Visit(Exp);


            var exprs = new List<Expression>();

            exprs.Add(EU.GenAreEqual(Expr.Constant(14), Expr.Constant(visitor.Visited.Count), "Expected 14 nodes to be visited"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitConditional")), "visited Condition?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitDefault")), "visited Default?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitDynamic")), "visited Dynamic?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitElementInit")), "visited ElementInit?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitListInit")), "visited ListInit?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitBlock")), "visited Block?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitParameter")), "visited parameter?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitNew")), "visited new?"));

            var tree = Expr.Block(exprs);
            V.Validate(tree);
            return tree;
        }


        public class MyExtension : Expression {
            public override bool CanReduce {
                get {
                    return true;
                }
            }

            public override Expression Reduce() {
                return Expr.Constant(1);
            }

            public sealed override ExpressionType NodeType {
                get { return ExpressionType.Extension; }
            }

            public sealed override Type Type {
                get { return typeof(int); }
            }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Visitor 4", new string[] { "positive", "Visitor", "miscellaneous", "Pri1" })]
        public static Expr Visitor4(EU.IValidator V) {
            //Visit: Extension Goto Index Invocation Label LabelTarget block constant call

            var visitor = new VIsitAll();

            //fithteen nodes total (witht the reduced extension).
            var label = Expr.Label();
            var Exp = Expr.Block(
                    new MyExtension(),
                    Expr.Goto(label),
                    Expr.Label(label),
                    Expr.ArrayAccess(Expr.Constant(new int[] { 1, 2, 3 }), Expr.Constant(1)),
                    Expr.Call(Expr.Constant(1), typeof(int).GetMethod("GetType")),
                    Expr.Invoke(Expr.Lambda(Expr.Empty()))
                );

            var NewExp = visitor.Visit(Exp);


            var exprs = new List<Expression>();

            exprs.Add(EU.GenAreEqual(Expr.Constant(15), Expr.Constant(visitor.Visited.Count), "Expected 15 nodes to be visited"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitExtension")), "visited extension?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitGoto")), "visited goto?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitIndex")), "visited index?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitInvocation")), "visited invocation?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitLabel")), "visited label?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitLabelTarget")), "visited labeltarget?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitBlock")), "visited block?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitConstant")), "visited constant?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitMethodCall")), "visited call?"));

            var tree = Expr.Block(exprs);
            V.Validate(tree);
            return tree;
        }


        public class HasList {
            public static List<int> x;
        }
        public class HasProp {
            public HasProp Prop {
                set {
                }
                get {
                    return new HasProp();
                }
            }
        }
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Visitor 5", new string[] { "positive", "Visitor", "miscellaneous", "Pri1" })]
        public static Expr Visitor5(EU.IValidator V) {
            //Visit: Loop Member MemberAssignment MemberBinding MemberInit MemberListBinding MemberMemberBinding block constant

            var visitor = new VIsitAll();

            //16 visits total (MemberBinding is called for each of the derived classes)
            var Exp = Expr.Loop(
                    Expr.Block(
                        Expr.Field(null, typeof(int).GetField("MaxValue")),
                        Expr.MemberInit(
                            Expr.New(
                                typeof(HasProp).GetConstructor(new Type[] { })
                            ),
                            new MemberBinding[] { 
                                Expr.MemberBind(
                                    typeof(HasProp).GetMember("Prop")[0], 
                                    new MemberBinding[] { 
                                        Expr.Bind(
                                            typeof(HasProp).GetMember("Prop")[0],
                                            Expr.Constant(new HasProp())
                                        ) 
                                    }
                                )
                            }
                        ),
                        Expr.MemberInit(
                            Expr.New(typeof(HasList).GetConstructor(new Type[] { })),
                            new MemberBinding[]{
                                Expr.ListBind(typeof(HasList).GetMember("x")[0])
                            }
                        )
                    )
                );


            var NewExp = visitor.Visit(Exp);


            var exprs = new List<Expression>();

            exprs.Add(EU.GenAreEqual(Expr.Constant(16), Expr.Constant(visitor.Visited.Count), "Expected 16 nodes to be visited"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitLoop")), "visited loop?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitLabelTarget")), "visited loop?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitMember")), "visited Member?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitMemberAssignment")), "visited MemberAssignment?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitMemberBinding")), "visited MemberBinding?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitMemberInit")), "visited Memberinit?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitMemberListBinding")), "visited MemberListBinding?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitMemberMemberBinding")), "visited MemberMemberBinding?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitConstant")), "visited constant?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitBlock")), "visited block?"));

            var tree = Expr.Block(exprs);
            V.Validate(tree);
            return tree;
        }


        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Visitor 6", new string[] { "positive", "Visitor", "miscellaneous", "Pri1" })]
        public static Expr Visitor6(EU.IValidator V) {
            //Visit: NewArray RuntimeVariables Switch SwitchCase TypeBinary Unary

            var visitor = new VIsitAll();

            //13 visits total
            var Exp = Expr.NewArrayInit(
                            typeof(int),
                            Expr.Switch(
                                Expr.Constant(1),
                                Expr.Constant(1),
                                Expr.SwitchCase(Expr.Constant(1), Expr.Constant(1))
                            ),
                            Expr.Block(
                                Expr.TypeEqual(Expr.Constant(1), typeof(int)),
                                Expr.RuntimeVariables(
                                Expr.Parameter(typeof(int), "")
                                ),
                                Expr.Negate(Expr.Constant(1))
                            )
                        );


            var NewExp = visitor.Visit(Exp);


            var exprs = new List<Expression>();

            exprs.Add(EU.GenAreEqual(Expr.Constant(14), Expr.Constant(visitor.Visited.Count), "Expected 14 nodes to be visited"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitNewArray")), "visited newarray?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitRuntimeVariables")), "visited runtimevariables?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitSwitch")), "visited switch?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitSwitchCase")), "visited switchcase?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitTypeBinary")), "visited typebinary?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitUnary")), "visited unary?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitParameter")), "visited parameter?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitConstant")), "visited constant?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitBlock")), "visited block?"));

            var tree = Expr.Block(exprs);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Visitor 7", new string[] { "positive", "Visitor", "miscellaneous", "Pri1" })]
        public static Expr Visitor7(EU.IValidator V) {
            //Visit: all

            var visitor = new VIsitAll();
            var doc = Expr.SymbolDocument("doesn'texist");
            Expression Exp = Expr.Block(
                Expr.DebugInfo(doc, 1, 1, 2, 2),
                Expr.Constant(1)
            );
            var NewExp = visitor.Visit(Exp);

            var exprs = new List<Expression>();
            exprs.Add(EU.GenAreEqual(Expr.Constant(3), Expr.Constant(visitor.Visited.Count), "Expected 3 nodes to be visited"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitDebugInfo")), "visited debuginfo?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitConstant")), "visited constant?"));
            exprs.Add(EU.GenAreEqual(Expr.Constant(true), Expr.Constant(visitor.Visited.Contains("VisitBlock")), "visited block?"));

            var tree = Expr.Block(exprs);
            V.Validate(tree);
            return tree;
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Visitor 100", new string[] { "positive", "Visitor", "miscellaneous", "Pri1" })]
        public static Expr Visitor100(EU.IValidator V) {
            //Visit: all

            var visitor = new VIsitAll();

            //ten nodes total.
            var param = Expr.Parameter(typeof(int), "");
            Expression Exp = Expr.Lambda(
                    Expr.Block(
                        Expr.Add(Expr.Constant(1), Expr.Constant(2)),
                        Expr.TryCatch(
                            Expr.Constant(1),
                            new CatchBlock[]{
                                Expr.Catch(typeof(Exception),Expr.Constant(1))
                            }
                        )
                    ),
                    param
                );


            var NewExp = visitor.Visit(Exp);

            var list = Expr.Parameter(typeof(List<int>), "");
            Exp = Expr.Condition(
                    Expr.Default(typeof(bool)),
                    Expr.Dynamic(new DynamicNode.MyBinder(), typeof(int), Expr.Constant(1)),
                    Expr.Block(
                        new[] { list },
                        Expr.Assign(list, Expr.Constant(new List<int>())),
                        Expr.ListInit(
                            Expr.New(
                                typeof(List<int>).GetConstructor(new Type[] { })
                            ),
                            Expr.ElementInit(
                                            typeof(List<int>).GetMethod("Add"),
                                            Expression.Constant(1)
                            )
                        ),
                        Expr.Constant(1)
                    )
                );

            NewExp = visitor.Visit(Exp);


            var label = Expr.Label();
            Exp = Expr.Block(
                    new MyExtension(),
                    Expr.Goto(label),
                    Expr.Label(label),
                    Expr.ArrayAccess(Expr.Constant(new int[] { 1, 2, 3 }), Expr.Constant(1)),
                    Expr.Call(Expr.Constant(1), typeof(int).GetMethod("GetType")),
                    Expr.Invoke(Expr.Lambda(Expr.Empty()))
                );

            NewExp = visitor.Visit(Exp);

            Exp = Expr.Loop(
                    Expr.Block(
                        Expr.Field(null, typeof(int).GetField("MaxValue")),
                        Expr.MemberInit(
                            Expr.New(
                                typeof(HasProp).GetConstructor(new Type[] { })
                            ),
                            new MemberBinding[] { 
                                Expr.MemberBind(
                                    typeof(HasProp).GetMember("Prop")[0], 
                                    new MemberBinding[] { 
                                        Expr.Bind(
                                            typeof(HasProp).GetMember("Prop")[0],
                                            Expr.Constant(new HasProp())
                                        ) 
                                    }
                                )
                            }
                        ),
                        Expr.MemberInit(
                            Expr.New(typeof(HasList).GetConstructor(new Type[] { })),
                            new MemberBinding[]{
                                Expr.ListBind(typeof(HasList).GetMember("x")[0])
                            }
                        )
                    )
                );

            NewExp = visitor.Visit(Exp);

            Exp = Expr.NewArrayInit(
                            typeof(int),
                            Expr.Switch(
                                Expr.Constant(1),
                                Expr.Constant(1),
                                Expr.SwitchCase(Expr.Constant(1), Expr.Constant(1))
                            ),
                            Expr.Block(
                                Expr.TypeEqual(Expr.Constant(1), typeof(int)),
                                Expr.RuntimeVariables(
                                Expr.Parameter(typeof(int), "")
                                ),
                                Expr.Negate(Expr.Constant(1))
                            )
                        );

            NewExp = visitor.Visit(Exp);

            var doc = Expr.SymbolDocument("doesn'texist");
            Exp = Expr.Block(
                Expr.DebugInfo(doc, 1, 1, 2, 2),
                Expr.Constant(1)
            );
            NewExp = visitor.Visit(Exp);

            foreach (MethodInfo mi in typeof(ExpressionVisitor).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
                if (mi.IsVirtual && mi.Name.StartsWith("Visit") && mi.Name.Length > "Visit".Length) {
                    String name = mi.Name;
                    if (mi.IsGenericMethod) {
                        name += "<";
                        foreach (var T in mi.GetGenericArguments()) {
                            name += T.Name + ",";
                        }
                        name = name.Substring(0, name.Length - 1) + ">";
                    }
                    if (!visitor.Visited.Contains(name)) throw new Exception("Didn't visit method " + mi.Name);
                }
            }

            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }

        //Check that all existing expression derived classes have a visitor method.
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Visitor 101", new string[] { "positive", "Visitor", "miscellaneous", "Pri1" })]
        public static Expr Visitor101(EU.IValidator V) {
            var VisitMethods = new List<String>();
            foreach (MethodInfo mi in typeof(ExpressionVisitor).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
                if (mi.IsVirtual && mi.Name.StartsWith("Visit") && mi.Name.Length > "Visit".Length) {
                    String name = mi.Name;
                    if (mi.IsGenericMethod) {
                        name += "<";
                        foreach (var T in mi.GetGenericArguments()) {
                            name += T.Name + ",";
                        }
                        name = name.Substring(0, name.Length - 1) + ">";
                    }
                    VisitMethods.Add(name);
                }
            }

            foreach (Type t in typeof(Expression).Assembly.GetTypes()) {
                if (!t.IsPublic) continue;
                if (InheritsFromExpression(t)) {
                    String name = t.Name;
                    if (t.IsGenericType) {
                        name += "<";
                        foreach (var T in t.GetGenericArguments()) {
                            name += T.Name + ",";
                        }
                        name = name.Substring(0, name.Length - 1) + ">";
                    }
                    var visitname = "Visit" + name;
                    visitname = visitname.Substring(0, visitname.Length - "Expression".Length);
                    if (visitname.CompareTo("VisitLambda") == 0) visitname = "VisitLambda<T>";
                    if (!VisitMethods.Contains(visitname)) throw new Exception("Class " + name + " doesn't seem to have a visit method in ExpressionVisitor");
                }
            }
            var tree = Expr.Empty();
            V.Validate(tree);
            return tree;
        }

        private static bool InheritsFromExpression(Type t) {
            if (!t.IsClass) return false;

            while (!(t == typeof(Expression) || t == typeof(Expression<TargetException>).GetGenericTypeDefinition() || t == typeof(object)) && t.Name.Contains("Expression")) {
                if (t.GetMethod("Reduce", BindingFlags.DeclaredOnly | BindingFlags.Public) != null) return false;
                if (t.BaseType == typeof(Expression)) return true;
                t = t.BaseType;
            }
            return false;
        }

        //Unary: Rewrite a unary without a method info, add a methodinfo (Err)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Rewriter 1", new string[] { "negative", "Func", "Action", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Rewriter1(EU.IValidator V) {
            var r = new RewriterTest1();
            Expression exp = Expr.Not(Expr.Constant(1));
            exp = EU.Throws<InvalidOperationException>(() => { r.Visit(exp); });
            return exp;
        }

        public class RewriterTest1 : ExpressionVisitor {

            protected override Expression VisitConstant(ConstantExpression node) {
                //Change to object with OpNot
                return Expr.Constant(new HasNot());
            }
        }

        public class HasNot {
            public int value;
            static public HasNot operator !(HasNot original) {
                return new HasNot();
            }
        }

        //Unary: Rewrite a unary with a method info, remove methodinfo (Err)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Rewriter 2", new string[] { "positive", "Func", "Action", "miscellaneous", "Pri1" })]
        public static Expr Rewriter2(EU.IValidator V) {
            var r = new RewriterTest2();
            Expression exp = Expr.Not(Expr.Constant(new HasNot()));
            exp = r.Visit(exp);
            return exp;
        }

        public class RewriterTest2 : ExpressionVisitor {

            protected override Expression VisitConstant(ConstantExpression node) {
                return Expr.Constant(new HasNot2());
            }
        }

        public class HasNot2 : HasNot {
            static public HasNot2 operator !(HasNot2 original) {
                return new HasNot2();
            }
        }

        //Unary: Change a child from a reference type to a value type (err)

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Rewriter 3", new string[] { "negative", "Func", "Action", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Rewriter3(EU.IValidator V) {
            var r = new RewriterTest3();
            Expression exp = Expr.Not(Expr.Constant(new HasNot2()));
            EU.Throws<System.InvalidOperationException>(() => { exp = r.Visit(exp); });
            return Expr.Empty();
        }

        public class RewriterTest3 : ExpressionVisitor {

            protected override Expression VisitConstant(ConstantExpression node) {
                return Expr.Constant(1);
            }
        }

        //Unary: Change a child value type (err)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Rewriter 4", new string[] { "negative", "Func", "Action", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Rewriter4(EU.IValidator V) {
            var r = new RewriterTest4();
            Expression exp = Expr.Not(Expr.Constant((long)1));
            exp = EU.Throws<InvalidOperationException>(() => { r.Visit(exp); });
            return exp;
        }

        public class RewriterTest4 : ExpressionVisitor {

            protected override Expression VisitConstant(ConstantExpression node) {
                return Expr.Constant(1);
            }
        }

        //Binary: Change a childâ€™s ref value to a different non convertible ref value (Err)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Rewriter 5", new string[] { "negative", "Func", "Action", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Rewriter5(EU.IValidator V) {
            var r = new RewriterTest5();
            Expression exp = Expr.Modulo(Expr.Constant(new HasModulo1()), Expr.Constant(new HasModulo1()));
            EU.Throws<System.InvalidOperationException>(() => { exp = r.Visit(exp); });
            return Expr.Empty();
        }

        public class RewriterTest5 : ExpressionVisitor {

            protected override Expression VisitConstant(ConstantExpression node) {
                return Expr.Constant(new HasModulo2());
            }
        }

        public class HasModulo1 {
            public static HasModulo1 operator %(HasModulo1 left, HasModulo1 right) {
                return new HasModulo1();
            }
        }

        public class HasModulo2 {
        }


        //Binary: Change a child from a value type to a different value type (err)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Rewriter 6", new string[] { "negative", "Func", "Action", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Rewriter6(EU.IValidator V) {
            var r = new RewriterTest6();
            Expression exp = Expr.Divide(Expr.Constant(1), Expr.Constant(1));
            EU.Throws<System.InvalidOperationException>(() => { exp = r.Visit(exp); });
            return Expr.Empty();
        }

        public class RewriterTest6 : ExpressionVisitor {

            protected override Expression VisitConstant(ConstantExpression node) {
                return Expr.Constant((byte)1);
            }
        }


        //Binary: Change a child from a value type to a reference type(err)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Rewriter 7", new string[] { "negative", "Func", "Action", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Rewriter7(EU.IValidator V) {
            var r = new RewriterTest7();
            Expression exp = Expr.Divide(Expr.Constant(1), Expr.Constant(1));
            EU.Throws<System.InvalidOperationException>(() => { exp = r.Visit(exp); });
            return Expr.Empty();
        }

        public class RewriterTest7 : ExpressionVisitor {

            protected override Expression VisitConstant(ConstantExpression node) {
                return Expr.Constant(new HasModulo1());
            }
        }

        //Binary:  Rewrite an expression without a method info, add a methodinfo
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Rewriter 8", new string[] { "positive", "Func", "Action", "miscellaneous", "Pri1" })]
        public static Expr Rewriter8(EU.IValidator V) {
            var r = new RewriterTest8();
            Expression exp = Expr.Equal(Expr.Constant(new R8()), Expr.Constant(new R8()));
            exp = r.Visit(exp);
            var tree = exp;
            V.Validate(tree);
            return tree;
        }

        public class RewriterTest8 : ExpressionVisitor {

            protected override Expression VisitConstant(ConstantExpression node) {
                return Expr.Constant(new HasEqual());
            }
        }

        public class R8 {
        }

        public class HasEqual {
            public static bool operator ==(HasEqual Left, HasEqual Right) {
                return true;
            }

            public static bool operator !=(HasEqual Left, HasEqual Right) {
                return false;
            }
            public override bool Equals(object obj) {
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return base.GetHashCode();
            }
        }

        //Binary: Rewrite an expression with a methodinfo, change the children to a non convertible type (Err)
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Rewriter 9", new string[] { "negative", "Func", "Action", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Rewriter9(EU.IValidator V) {
            var r = new RewriterTest9();
            Expression exp = Expr.Equal(Expr.Constant(new HasEqual()), Expr.Constant(new HasEqual()));
            EU.Throws<System.InvalidOperationException>(() => { exp = r.Visit(exp); });
            return Expr.Empty();
        }

        public class RewriterTest9 : ExpressionVisitor {

            protected override Expression VisitConstant(ConstantExpression node) {
                return Expr.Constant(new R8());
            }
        }


        //switch: Rewrite an expression without a method info, add a methodinfo
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Rewriter 10", new string[] { "negative", "Func", "Action", "miscellaneous", "Pri1" }, Exception = typeof(InvalidOperationException))]
        public static Expr Rewriter10(EU.IValidator V) {
            var r = new RewriterTest10();
            Expression exp = Expr.Switch(
                typeof(int),
                Expr.Constant(new R8()),
                Expr.Constant(1),
                null,
                Expr.SwitchCase(Expr.Constant(1), Expr.Constant(new R8()))
                );
            exp = EU.Throws<InvalidOperationException>(() => { r.Visit(exp); });
            return exp;
        }

        public class RewriterTest10 : ExpressionVisitor {

            protected override Expression VisitConstant(ConstantExpression node) {
                if (node.Type == typeof(R8)) {
                    return Expr.Constant(new HasEqual());
                } else {
                    return node;
                }
            }
        }

        //switch: Rewrite an expression without a method info, add a methodinfo
        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Rewriter 11", new string[] { "negative", "Func", "Action", "miscellaneous", "Pri1" }, Exception = typeof(ArgumentException))]
        public static Expr Rewriter11(EU.IValidator V) {
            var r = new RewriterTest11();
            Expression exp = Expr.Switch(
                typeof(int),
                Expr.Constant(new HasEqual()),
                Expr.Constant(1),
                null,
                Expr.SwitchCase(Expr.Constant(1), Expr.Constant(new HasEqual()))
                );
            exp = EU.Throws<ArgumentException>(() => { r.Visit(exp); });
            return Expr.Empty();
        }

        public class RewriterTest11 : ExpressionVisitor {

            protected override Expression VisitConstant(ConstantExpression node) {
                if (node.Type == typeof(HasEqual))
                    return Expr.Constant(new R8());
                else
                    return node;
            }
        }

        class ExpressionVisitor1 : ExpressionVisitor {
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Visitor 12", new string[] { "negative", "Func", "Action", "miscellaneous", "Pri1" }, Exception = typeof(NullReferenceException))]
        public static Expr Visitor12(EU.IValidator V) {
            var Visit = new ExpressionVisitor1();

            EU.Throws<NullReferenceException>(() => { Visit.Visit(new ExtensionTest12()); });
            return Expr.Empty();
        }

        public class ExtensionTest12 : Expression {
            protected override Expression VisitChildren(ExpressionVisitor visitor) {
                return base.VisitChildren(null);
            }

            public override bool CanReduce {
                get {
                    return true;
                }
            }

            public override Expression Reduce() {
                return Expression.Constant(1);
            }

            public sealed override ExpressionType NodeType {
                get { return ExpressionType.Extension; }
            }

            public sealed override Type Type {
                get { return typeof(int); }
            }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Visitor 13", new string[] { "positive", "Func", "Action", "miscellaneous", "Pri1" })]
        public static Expr Visitor13(EU.IValidator V) {
            var Visit = new ExpressionVisitor1();
            // no tree to compile/validate. Just make sure this doesn't throw.
            Visit.Visit(new ExtensionTest13());
            return Expr.Empty();
        }

        public class ExtensionTest13 : Expression {
            private class BadVisitor : ExpressionVisitor {
                public override Expression Visit(Expression node) {
                    return null;
                }
            }

            protected override Expression VisitChildren(ExpressionVisitor visitor) {
                return base.VisitChildren(new BadVisitor());
            }

            public override bool CanReduce {
                get {
                    return true;
                }
            }

            public override Expression Reduce() {
                return Expression.Constant(1);
            }

            public sealed override ExpressionType NodeType {
                get { return ExpressionType.Extension; }
            }

            public sealed override Type Type {
                get { return typeof(int); }
            }
        }

        [ETUtils.TestAttribute(ETUtils.TestState.Enabled, "Visitor 14", new string[] { "positive", "Func", "Action", "miscellaneous", "Pri1" })]
        public static Expr Visitor14(EU.IValidator V) {
            var Visit = new ExpressionVisitor1();

            Visit.Visit(new ExtensionTest14());

            var tree = EU.GenAreEqual(Expr.Constant(true), Expr.Field(null, typeof(ExtensionTest14), "hit"));
            V.Validate(tree);
            return tree;
        }

        public class ExtensionTest14 : Expression {

            public static bool hit = false;
            protected override Expression VisitChildren(ExpressionVisitor visitor) {
                if (visitor != null) hit = true;
                return null;
            }

            public override bool CanReduce {
                get {
                    return true;
                }
            }

            public override Expression Reduce() {
                return Expression.Constant(1);
            }

            public sealed override ExpressionType NodeType {
                get { return ExpressionType.Extension; }
            }

            public sealed override Type Type {
                get { return typeof(int); }
            }
        }

    }
}
