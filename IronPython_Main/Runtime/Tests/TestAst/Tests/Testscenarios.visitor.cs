#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Collections.Generic;

namespace TestAst {

    public partial class TestScenarios {

        public class TestVisitor : ExpressionVisitor {
            public string res;
            public int count;

            //visited
            protected override Expression VisitParameter(ParameterExpression node) {
                res += "+parameter_" + node.Name;
                count += 1;
                return base.VisitParameter(node);
            }
           

            //visited
            protected override Expression VisitBinary(BinaryExpression node) {
                res += "+binary_";
                count += 1;
                return base.VisitBinary(node);
            }

            protected override Expression VisitBlock(BlockExpression node) {
                res += "+block_";
                count += 1;
                return base.VisitBlock(node);
            }

            protected override CatchBlock VisitCatchBlock(CatchBlock node) {
                res += "+catch_";
                count += 1;
                return base.VisitCatchBlock(node);
            }

            protected override Expression VisitConditional(ConditionalExpression node) {
                res += "+conditional_";
                count += 1;
                return base.VisitConditional(node);
            }

            //visited
            protected override Expression VisitConstant(ConstantExpression node) {
                res += "+constant_";
                count += 1;
                return base.VisitConstant(node);
            }

            protected override Expression VisitDebugInfo(DebugInfoExpression node) {
                res += "+debuginfo_";
                count += 1;
                return base.VisitDebugInfo(node);
            }
            
            protected override Expression VisitDynamic(DynamicExpression node) {
                res += "+dynamic_";
                count += 1;
                return base.VisitDynamic(node);
            }

            protected override ElementInit VisitElementInit(ElementInit initializer) {
                res += "+elementinit_";
                count += 1;
                return base.VisitElementInit(initializer);
            }

            protected override Expression VisitDefault(DefaultExpression node) {
                res += "+empty_";
                count += 1;
                return base.VisitDefault(node);
            }

            protected override Expression VisitExtension(Expression node) {
                res += "+extension_";
                count += 1;
                return base.VisitExtension(node);
            }

            protected override Expression VisitGoto(GotoExpression node) {
                res += "+goto_";
                count += 1;
                return base.VisitGoto(node);
            }

            protected override Expression VisitIndex(IndexExpression node) {
                res += "+index_";
                count += 1;
                return base.VisitIndex(node);
            }

            protected override Expression VisitInvocation(InvocationExpression node) {
                res += "+invocation_";
                count += 1;
                return base.VisitInvocation(node);
            }

            protected override Expression VisitLabel(LabelExpression node) {
                res += "+label_";
                count += 1;
                return base.VisitLabel(node);
            }

            protected override LabelTarget VisitLabelTarget(LabelTarget node) {
                res += "+labeltarget_";
                count += 1;
                return base.VisitLabelTarget(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node) {
                res += "+lambda_" + node.Name;
                count += 1;
                return base.VisitLambda<T>(node);
            }

            protected override Expression VisitListInit(ListInitExpression node) {
                res += "+listinit_";
                count += 1;
                return base.VisitListInit(node);
            }

            protected override Expression VisitLoop(LoopExpression node) {
                res += "+loop_";
                count += 1;
                return base.VisitLoop(node);
            }

            protected override Expression VisitMember(MemberExpression node) {
                res += "+memberaccess_";
                count += 1;
                return base.VisitMember(node);
            }

            protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment) {
                res += "+memberassignment_";
                count += 1;
                return base.VisitMemberAssignment(assignment);
            }

            protected override MemberBinding VisitMemberBinding(MemberBinding binding) {
                res += "+memberbinding_";
                count += 1;
                return base.VisitMemberBinding(binding);
            }

            protected override Expression VisitMemberInit(MemberInitExpression node) {
                res += "+memberinit_";
                count += 1;
                return base.VisitMemberInit(node);
            }

            protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding) {
                res += "+memberlistbinding_";
                count += 1;
                return base.VisitMemberListBinding(binding);
            }

            protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding) {
                res += "+membermemberbinding_";
                count += 1;
                return base.VisitMemberMemberBinding(binding);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node) {
                res += "+methodcall_";
                count += 1;
                return base.VisitMethodCall(node);
            }

            protected override Expression VisitNew(NewExpression node) {
                res += "+new_";
                count += 1;
                return base.VisitNew(node);
            }

            protected override Expression VisitNewArray(NewArrayExpression node) {
                res += "+newarray_";
                count += 1;
                return base.VisitNewArray(node);
            }

            protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
                res += "+runtimevariables_";
                count += 1;
                return base.VisitRuntimeVariables(node);
            }

            protected override Expression VisitSwitch(SwitchExpression node) {
                res += "+switch_";
                count += 1;
                return base.VisitSwitch(node);
            }

            protected override SwitchCase VisitSwitchCase(SwitchCase node) {
                res += "+switchcase_";
                count += 1;
                return base.VisitSwitchCase(node);
            }

            protected override Expression VisitTry(TryExpression node) {
                res += "+try_";
                count += 1;
                return base.VisitTry(node);
            }
           
            protected override Expression VisitTypeBinary(TypeBinaryExpression node) {
                res += "+typebinary_";
                count += 1;
                return base.VisitTypeBinary(node);
            }


            //visited
            protected override Expression VisitUnary(UnaryExpression node) {
                res += "+unary_";
                count += 1;
                return base.VisitUnary(node);
            }
        }

        [TestAttribute()]
        private static Expression Test_TreeVisitor1(TestScenarios ts) {
            TestVisitor visitor = new TestVisitor();

            var body = Expression.Block(
                                            new ParameterExpression[] { Expression.Parameter(typeof(int), "var1"), Expression.Parameter(typeof(int), "var2") },
                                            Expression.Empty()
                                        );

            var tree = Expression.Lambda(body, Expression.Parameter(typeof(int), "parm1"), Expression.Parameter(typeof(int), "parm2"), Expression.Parameter(typeof(int), "parm3"));

            visitor.Visit(tree);

            
            return ETUtils.ExpressionUtils.GenAreEqual(Expression.Constant("+lambda_lambda_method+block_+empty_+parameter_var1+parameter_var2+parameter_parm1+parameter_parm2+parameter_parm3"), Expression.Constant(visitor.res), "visiting lambda parameters");
        }

        [TestAttribute()]
        private static Expression Test_Visitor2(TestScenarios ts) {
            List<Expression > exprs = new List<Expression >();
            string res = "";

            // x = ~(-2) + 2

            ParameterExpression x = Expression.Parameter(typeof(int), "x");
            exprs.Add(Expression.Assign(x, Expression.Add(Expression.Negate(Expression.Constant(-2)),Expression.Constant(2))));
            res += "+binary_+parameter_x+binary_+unary_+constant_+constant_";


            exprs.Add(Expression.TypeIs(Expression.Constant(5), typeof(int)));
            res += "+typebinary_+constant_";

            //+block_+binary_+parameter_x+binary_+unary_+constant_+constant_+typebinary_+constant_+parameter_x
                       

            TestVisitor visitor = new TestVisitor();
            visitor.Visit(Expression.Block(new ParameterExpression[]{x},exprs.ToArray()));

            res = "+block_" + res + "+parameter_x";
            
            return ETUtils.ExpressionUtils.GenAreEqual(Expression.Constant(res), Expression.Constant(visitor.res), "visiting parameters");
        }
        
    }
}
