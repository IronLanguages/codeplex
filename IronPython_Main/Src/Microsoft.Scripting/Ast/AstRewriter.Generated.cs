/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/


namespace Microsoft.Scripting.Ast {
    partial class AstRewriter {

        private delegate Expression Rewriter(AstRewriter ar, Expression expr, Stack stack);

        private static Rewriter[] _Rewriters = {

            #region Generated Ast Rewriter

            // *** BEGIN GENERATED CODE ***

            RewriteBinaryExpression,                                    //    Add
                                                                        // ** AddChecked
            RewriteBinaryExpression,                                    //    And
            RewriteLogicalBinaryExpression,                             //    AndAlso
                                                                        // ** ArrayLength
            RewriteBinaryExpression,                                    //    ArrayIndex
            RewriteMethodCallExpression,                                //    Call
                                                                        // ** Coalesce
            RewriteConditionalExpression,                               //    Conditional
            RewriteConstantExpression,                                  //    Constant
            RewriteUnaryExpression,                                     //    Convert
                                                                        // ** ConvertChecked
            RewriteBinaryExpression,                                    //    Divide
            RewriteBinaryExpression,                                    //    Equal
            RewriteBinaryExpression,                                    //    ExclusiveOr
            RewriteBinaryExpression,                                    //    GreaterThan
            RewriteBinaryExpression,                                    //    GreaterThanOrEqual
                                                                        // ** Invoke
                                                                        // ** Lambda
            RewriteBinaryExpression,                                    //    LeftShift
            RewriteBinaryExpression,                                    //    LessThan
            RewriteBinaryExpression,                                    //    LessThanOrEqual
                                                                        // ** ListInit
                                                                        // ** MemberAccess
                                                                        // ** MemberInit
            RewriteBinaryExpression,                                    //    Modulo
            RewriteBinaryExpression,                                    //    Multiply
                                                                        // ** MultiplyChecked
            RewriteUnaryExpression,                                     //    Negate
                                                                        // ** UnaryPlus
                                                                        // ** NegateChecked
            RewriteNewExpression,                                       //    New
                                                                        // ** NewArrayInit
                                                                        // ** NewArrayBounds
            RewriteUnaryExpression,                                     //    Not
            RewriteBinaryExpression,                                    //    NotEqual
            RewriteBinaryExpression,                                    //    Or
            RewriteLogicalBinaryExpression,                             //    OrElse
                                                                        // ** Parameter
                                                                        // ** Power
                                                                        // ** Quote
            RewriteBinaryExpression,                                    //    RightShift
            RewriteBinaryExpression,                                    //    Subtract
                                                                        // ** SubtractChecked
                                                                        // ** TypeAs
            RewriteTypeBinaryExpression,                                //    TypeIs
            RewriteActionExpression,                                    //    ActionExpression
            RewriteArrayIndexAssignment,                                //    ArrayIndexAssignment
            RewriteBlock,                                               //    Block
            RewriteBoundAssignment,                                     //    BoundAssignment
            RewriteBoundExpression,                                     //    BoundExpression
            RewriteBreakStatement,                                      //    BreakStatement
            RewriteCodeBlockExpression,                                 //    CodeBlockExpression
            RewriteIntrinsicExpression,                                 //    CodeContextExpression
            RewriteIntrinsicExpression,                                 //    GeneratorIntrinsic
            RewriteContinueStatement,                                   //    ContinueStatement
            RewriteDeleteStatement,                                     //    DeleteStatement
            RewriteDeleteUnboundExpression,                             //    DeleteUnboundExpression
            RewriteDoStatement,                                         //    DoStatement
            RewriteEmptyStatement,                                      //    EmptyStatement
            RewriteIntrinsicExpression,                                 //    EnvironmentExpression
            RewriteExpressionStatement,                                 //    ExpressionStatement
            RewriteLabeledStatement,                                    //    LabeledStatement
            RewriteLoopStatement,                                       //    LoopStatement
            RewriteMemberAssignment,                                    //    MemberAssignment
            RewriteMemberExpression,                                    //    MemberExpression
            RewriteNewArrayExpression,                                  //    NewArrayExpression
            RewriteUnaryExpression,                                     //    OnesComplement
            RewriteIntrinsicExpression,                                 //    ParamsExpression
            RewriteReturnStatement,                                     //    ReturnStatement
            RewriteScopeStatement,                                      //    ScopeStatement
            RewriteSwitchStatement,                                     //    SwitchStatement
            RewriteThrowStatement,                                      //    ThrowStatement
            RewriteTryStatement,                                        //    TryStatement
            RewriteUnboundAssignment,                                   //    UnboundAssignment
            RewriteUnboundExpression,                                   //    UnboundExpression
            RewriteYieldStatement,                                      //    YieldStatement

            // *** END GENERATED CODE ***

            #endregion
        };
    }
}

