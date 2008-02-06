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

using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Ast {
    static partial class Interpreter {

        private delegate object InterpretDelegate(CodeContext context, Expression expression);

        private static InterpretDelegate[] _Interpreters = new InterpretDelegate[] {
            #region Generated Ast Interpreter

            // *** BEGIN GENERATED CODE ***

            InterpretBinaryExpression,                                  //    Add
                                                                        // ** AddChecked
            InterpretBinaryExpression,                                  //    And
            InterpretAndAlsoBinaryExpression,                           //    AndAlso
                                                                        // ** ArrayLength
            InterpretBinaryExpression,                                  //    ArrayIndex
            InterpretMethodCallExpression,                              //    Call
                                                                        // ** Coalesce
            InterpretConditionalExpression,                             //    Conditional
            InterpretConstantExpression,                                //    Constant
            InterpretUnaryExpression,                                   //    Convert
                                                                        // ** ConvertChecked
            InterpretBinaryExpression,                                  //    Divide
            InterpretBinaryExpression,                                  //    Equal
            InterpretBinaryExpression,                                  //    ExclusiveOr
            InterpretBinaryExpression,                                  //    GreaterThan
            InterpretBinaryExpression,                                  //    GreaterThanOrEqual
                                                                        // ** Invoke
                                                                        // ** Lambda
            InterpretBinaryExpression,                                  //    LeftShift
            InterpretBinaryExpression,                                  //    LessThan
            InterpretBinaryExpression,                                  //    LessThanOrEqual
                                                                        // ** ListInit
                                                                        // ** MemberAccess
                                                                        // ** MemberInit
            InterpretBinaryExpression,                                  //    Modulo
            InterpretBinaryExpression,                                  //    Multiply
                                                                        // ** MultiplyChecked
            InterpretUnaryExpression,                                   //    Negate
                                                                        // ** UnaryPlus
                                                                        // ** NegateChecked
            InterpretNewExpression,                                     //    New
                                                                        // ** NewArrayInit
                                                                        // ** NewArrayBounds
            InterpretUnaryExpression,                                   //    Not
            InterpretBinaryExpression,                                  //    NotEqual
            InterpretBinaryExpression,                                  //    Or
            InterpretOrElseBinaryExpression,                            //    OrElse
                                                                        // ** Parameter
                                                                        // ** Power
                                                                        // ** Quote
            InterpretBinaryExpression,                                  //    RightShift
            InterpretBinaryExpression,                                  //    Subtract
                                                                        // ** SubtractChecked
                                                                        // ** TypeAs
            InterpretTypeBinaryExpression,                              //    TypeIs
            InterpretActionExpression,                                  //    ActionExpression
            InterpretArrayIndexAssignment,                              //    ArrayIndexAssignment
            InterpretBlock,                                             //    Block
            InterpretBoundAssignment,                                   //    BoundAssignment
            InterpretBoundExpression,                                   //    BoundExpression
            InterpretBreakStatement,                                    //    BreakStatement
            InterpretCodeBlockExpression,                               //    CodeBlockExpression
            InterpretIntrinsicExpression,                               //    CodeContextExpression
            InterpretIntrinsicExpression,                               //    GeneratorIntrinsic
            InterpretContinueStatement,                                 //    ContinueStatement
            InterpretDeleteStatement,                                   //    DeleteStatement
            InterpretDeleteUnboundExpression,                           //    DeleteUnboundExpression
            InterpretDoStatement,                                       //    DoStatement
            InterpretEmptyStatement,                                    //    EmptyStatement
            InterpretIntrinsicExpression,                               //    EnvironmentExpression
            InterpretExpressionStatement,                               //    ExpressionStatement
            InterpretLabeledStatement,                                  //    LabeledStatement
            InterpretLoopStatement,                                     //    LoopStatement
            InterpretMemberAssignment,                                  //    MemberAssignment
            InterpretMemberExpression,                                  //    MemberExpression
            InterpretNewArrayExpression,                                //    NewArrayExpression
            InterpretUnaryExpression,                                   //    OnesComplement
            InterpretIntrinsicExpression,                               //    ParamsExpression
            InterpretReturnStatement,                                   //    ReturnStatement
            InterpretScopeStatement,                                    //    ScopeStatement
            InterpretSwitchStatement,                                   //    SwitchStatement
            InterpretThrowStatement,                                    //    ThrowStatement
            InterpretTryStatement,                                      //    TryStatement
            InterpretUnboundAssignment,                                 //    UnboundAssignment
            InterpretUnboundExpression,                                 //    UnboundExpression
            InterpretYieldStatement,                                    //    YieldStatement

            // *** END GENERATED CODE ***

            #endregion
        };
    }
}
