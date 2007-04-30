/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Diagnostics;
using Microsoft.Scripting.Internal.Ast;

namespace Microsoft.Scripting.Internal.Ast {
    #region Generated Scripting AST Walker

    // *** BEGIN GENERATED CODE ***

    /// <summary>
    /// Walker class - The Scripting AST Walker (default result is true)
    /// </summary>
    public class Walker {
        // ActionExpression
        public virtual bool Walk(ActionExpression node) { return true; }
        public virtual void PostWalk(ActionExpression node) { }

        // AndExpression
        public virtual bool Walk(AndExpression node) { return true; }
        public virtual void PostWalk(AndExpression node) { }

        // ArrayIndexAssignment
        public virtual bool Walk(ArrayIndexAssignment node) { return true; }
        public virtual void PostWalk(ArrayIndexAssignment node) { }

        // ArrayIndexExpression
        public virtual bool Walk(ArrayIndexExpression node) { return true; }
        public virtual void PostWalk(ArrayIndexExpression node) { }

        // BinaryExpression
        public virtual bool Walk(BinaryExpression node) { return true; }
        public virtual void PostWalk(BinaryExpression node) { }

        // BoundAssignment
        public virtual bool Walk(BoundAssignment node) { return true; }
        public virtual void PostWalk(BoundAssignment node) { }

        // BoundExpression
        public virtual bool Walk(BoundExpression node) { return true; }
        public virtual void PostWalk(BoundExpression node) { }

        // CallExpression
        public virtual bool Walk(CallExpression node) { return true; }
        public virtual void PostWalk(CallExpression node) { }

        // CallWithThisExpression
        public virtual bool Walk(CallWithThisExpression node) { return true; }
        public virtual void PostWalk(CallWithThisExpression node) { }

        // CodeBlockExpression
        public virtual bool Walk(CodeBlockExpression node) { return true; }
        public virtual void PostWalk(CodeBlockExpression node) { }

        // CodeContextExpression
        public virtual bool Walk(CodeContextExpression node) { return true; }
        public virtual void PostWalk(CodeContextExpression node) { }

        // CommaExpression
        public virtual bool Walk(CommaExpression node) { return true; }
        public virtual void PostWalk(CommaExpression node) { }

        // ConditionalExpression
        public virtual bool Walk(ConditionalExpression node) { return true; }
        public virtual void PostWalk(ConditionalExpression node) { }

        // ConstantExpression
        public virtual bool Walk(ConstantExpression node) { return true; }
        public virtual void PostWalk(ConstantExpression node) { }

        // DeleteDynamicMemberExpression
        public virtual bool Walk(DeleteDynamicMemberExpression node) { return true; }
        public virtual void PostWalk(DeleteDynamicMemberExpression node) { }

        // DeleteIndexExpression
        public virtual bool Walk(DeleteIndexExpression node) { return true; }
        public virtual void PostWalk(DeleteIndexExpression node) { }

        // DynamicMemberAssignment
        public virtual bool Walk(DynamicMemberAssignment node) { return true; }
        public virtual void PostWalk(DynamicMemberAssignment node) { }

        // DynamicMemberExpression
        public virtual bool Walk(DynamicMemberExpression node) { return true; }
        public virtual void PostWalk(DynamicMemberExpression node) { }

        // DynamicNewExpression
        public virtual bool Walk(DynamicNewExpression node) { return true; }
        public virtual void PostWalk(DynamicNewExpression node) { }

        // EnvironmentExpression
        public virtual bool Walk(EnvironmentExpression node) { return true; }
        public virtual void PostWalk(EnvironmentExpression node) { }

        // IndexAssignment
        public virtual bool Walk(IndexAssignment node) { return true; }
        public virtual void PostWalk(IndexAssignment node) { }

        // IndexExpression
        public virtual bool Walk(IndexExpression node) { return true; }
        public virtual void PostWalk(IndexExpression node) { }

        // MemberAssignment
        public virtual bool Walk(MemberAssignment node) { return true; }
        public virtual void PostWalk(MemberAssignment node) { }

        // MemberExpression
        public virtual bool Walk(MemberExpression node) { return true; }
        public virtual void PostWalk(MemberExpression node) { }

        // MethodCallExpression
        public virtual bool Walk(MethodCallExpression node) { return true; }
        public virtual void PostWalk(MethodCallExpression node) { }

        // NewArrayExpression
        public virtual bool Walk(NewArrayExpression node) { return true; }
        public virtual void PostWalk(NewArrayExpression node) { }

        // NewExpression
        public virtual bool Walk(NewExpression node) { return true; }
        public virtual void PostWalk(NewExpression node) { }

        // OrExpression
        public virtual bool Walk(OrExpression node) { return true; }
        public virtual void PostWalk(OrExpression node) { }

        // ParamsExpression
        public virtual bool Walk(ParamsExpression node) { return true; }
        public virtual void PostWalk(ParamsExpression node) { }

        // ParenthesisExpression
        public virtual bool Walk(ParenthesisExpression node) { return true; }
        public virtual void PostWalk(ParenthesisExpression node) { }

        // ShortCircuitExpression
        public virtual bool Walk(ShortCircuitExpression node) { return true; }
        public virtual void PostWalk(ShortCircuitExpression node) { }

        // StaticUnaryExpression
        public virtual bool Walk(StaticUnaryExpression node) { return true; }
        public virtual void PostWalk(StaticUnaryExpression node) { }

        // ThrowExpression
        public virtual bool Walk(ThrowExpression node) { return true; }
        public virtual void PostWalk(ThrowExpression node) { }

        // TypeBinaryExpression
        public virtual bool Walk(TypeBinaryExpression node) { return true; }
        public virtual void PostWalk(TypeBinaryExpression node) { }

        // VoidExpression
        public virtual bool Walk(VoidExpression node) { return true; }
        public virtual void PostWalk(VoidExpression node) { }

        // BlockStatement
        public virtual bool Walk(BlockStatement node) { return true; }
        public virtual void PostWalk(BlockStatement node) { }

        // BreakStatement
        public virtual bool Walk(BreakStatement node) { return true; }
        public virtual void PostWalk(BreakStatement node) { }

        // ContinueStatement
        public virtual bool Walk(ContinueStatement node) { return true; }
        public virtual void PostWalk(ContinueStatement node) { }

        // DebugStatement
        public virtual bool Walk(DebugStatement node) { return true; }
        public virtual void PostWalk(DebugStatement node) { }

        // DelStatement
        public virtual bool Walk(DelStatement node) { return true; }
        public virtual void PostWalk(DelStatement node) { }

        // DoStatement
        public virtual bool Walk(DoStatement node) { return true; }
        public virtual void PostWalk(DoStatement node) { }

        // EmptyStatement
        public virtual bool Walk(EmptyStatement node) { return true; }
        public virtual void PostWalk(EmptyStatement node) { }

        // ExpressionStatement
        public virtual bool Walk(ExpressionStatement node) { return true; }
        public virtual void PostWalk(ExpressionStatement node) { }

        // IfStatement
        public virtual bool Walk(IfStatement node) { return true; }
        public virtual void PostWalk(IfStatement node) { }

        // LabeledStatement
        public virtual bool Walk(LabeledStatement node) { return true; }
        public virtual void PostWalk(LabeledStatement node) { }

        // LoopStatement
        public virtual bool Walk(LoopStatement node) { return true; }
        public virtual void PostWalk(LoopStatement node) { }

        // ReturnStatement
        public virtual bool Walk(ReturnStatement node) { return true; }
        public virtual void PostWalk(ReturnStatement node) { }

        // ScopeStatement
        public virtual bool Walk(ScopeStatement node) { return true; }
        public virtual void PostWalk(ScopeStatement node) { }

        // SwitchStatement
        public virtual bool Walk(SwitchStatement node) { return true; }
        public virtual void PostWalk(SwitchStatement node) { }

        // TryFinallyStatement
        public virtual bool Walk(TryFinallyStatement node) { return true; }
        public virtual void PostWalk(TryFinallyStatement node) { }

        // TryStatement
        public virtual bool Walk(TryStatement node) { return true; }
        public virtual void PostWalk(TryStatement node) { }

        // YieldStatement
        public virtual bool Walk(YieldStatement node) { return true; }
        public virtual void PostWalk(YieldStatement node) { }

        // Arg
        public virtual bool Walk(Arg node) { return true; }
        public virtual void PostWalk(Arg node) { }

        // CodeBlock
        public virtual bool Walk(CodeBlock node) { return true; }
        public virtual void PostWalk(CodeBlock node) { }

        // GeneratorCodeBlock
        public virtual bool Walk(GeneratorCodeBlock node) { return true; }
        public virtual void PostWalk(GeneratorCodeBlock node) { }

        // IfStatementTest
        public virtual bool Walk(IfStatementTest node) { return true; }
        public virtual void PostWalk(IfStatementTest node) { }

        // TryStatementHandler
        public virtual bool Walk(TryStatementHandler node) { return true; }
        public virtual void PostWalk(TryStatementHandler node) { }
    }


    /// <summary>
    /// WalkerNonRecursive class - The Scripting AST Walker (default result is false)
    /// </summary>
    public class WalkerNonRecursive : Walker {
        // ActionExpression
        public override bool Walk(ActionExpression node) { return false; }
        public override void PostWalk(ActionExpression node) { }

        // AndExpression
        public override bool Walk(AndExpression node) { return false; }
        public override void PostWalk(AndExpression node) { }

        // ArrayIndexAssignment
        public override bool Walk(ArrayIndexAssignment node) { return false; }
        public override void PostWalk(ArrayIndexAssignment node) { }

        // ArrayIndexExpression
        public override bool Walk(ArrayIndexExpression node) { return false; }
        public override void PostWalk(ArrayIndexExpression node) { }

        // BinaryExpression
        public override bool Walk(BinaryExpression node) { return false; }
        public override void PostWalk(BinaryExpression node) { }

        // BoundAssignment
        public override bool Walk(BoundAssignment node) { return false; }
        public override void PostWalk(BoundAssignment node) { }

        // BoundExpression
        public override bool Walk(BoundExpression node) { return false; }
        public override void PostWalk(BoundExpression node) { }

        // CallExpression
        public override bool Walk(CallExpression node) { return false; }
        public override void PostWalk(CallExpression node) { }

        // CallWithThisExpression
        public override bool Walk(CallWithThisExpression node) { return false; }
        public override void PostWalk(CallWithThisExpression node) { }

        // CodeBlockExpression
        public override bool Walk(CodeBlockExpression node) { return false; }
        public override void PostWalk(CodeBlockExpression node) { }

        // CodeContextExpression
        public override bool Walk(CodeContextExpression node) { return false; }
        public override void PostWalk(CodeContextExpression node) { }

        // CommaExpression
        public override bool Walk(CommaExpression node) { return false; }
        public override void PostWalk(CommaExpression node) { }

        // ConditionalExpression
        public override bool Walk(ConditionalExpression node) { return false; }
        public override void PostWalk(ConditionalExpression node) { }

        // ConstantExpression
        public override bool Walk(ConstantExpression node) { return false; }
        public override void PostWalk(ConstantExpression node) { }

        // DeleteDynamicMemberExpression
        public override bool Walk(DeleteDynamicMemberExpression node) { return false; }
        public override void PostWalk(DeleteDynamicMemberExpression node) { }

        // DeleteIndexExpression
        public override bool Walk(DeleteIndexExpression node) { return false; }
        public override void PostWalk(DeleteIndexExpression node) { }

        // DynamicMemberAssignment
        public override bool Walk(DynamicMemberAssignment node) { return false; }
        public override void PostWalk(DynamicMemberAssignment node) { }

        // DynamicMemberExpression
        public override bool Walk(DynamicMemberExpression node) { return false; }
        public override void PostWalk(DynamicMemberExpression node) { }

        // DynamicNewExpression
        public override bool Walk(DynamicNewExpression node) { return false; }
        public override void PostWalk(DynamicNewExpression node) { }

        // EnvironmentExpression
        public override bool Walk(EnvironmentExpression node) { return false; }
        public override void PostWalk(EnvironmentExpression node) { }

        // IndexAssignment
        public override bool Walk(IndexAssignment node) { return false; }
        public override void PostWalk(IndexAssignment node) { }

        // IndexExpression
        public override bool Walk(IndexExpression node) { return false; }
        public override void PostWalk(IndexExpression node) { }

        // MemberAssignment
        public override bool Walk(MemberAssignment node) { return false; }
        public override void PostWalk(MemberAssignment node) { }

        // MemberExpression
        public override bool Walk(MemberExpression node) { return false; }
        public override void PostWalk(MemberExpression node) { }

        // MethodCallExpression
        public override bool Walk(MethodCallExpression node) { return false; }
        public override void PostWalk(MethodCallExpression node) { }

        // NewArrayExpression
        public override bool Walk(NewArrayExpression node) { return false; }
        public override void PostWalk(NewArrayExpression node) { }

        // NewExpression
        public override bool Walk(NewExpression node) { return false; }
        public override void PostWalk(NewExpression node) { }

        // OrExpression
        public override bool Walk(OrExpression node) { return false; }
        public override void PostWalk(OrExpression node) { }

        // ParamsExpression
        public override bool Walk(ParamsExpression node) { return false; }
        public override void PostWalk(ParamsExpression node) { }

        // ParenthesisExpression
        public override bool Walk(ParenthesisExpression node) { return false; }
        public override void PostWalk(ParenthesisExpression node) { }

        // ShortCircuitExpression
        public override bool Walk(ShortCircuitExpression node) { return false; }
        public override void PostWalk(ShortCircuitExpression node) { }

        // StaticUnaryExpression
        public override bool Walk(StaticUnaryExpression node) { return false; }
        public override void PostWalk(StaticUnaryExpression node) { }

        // ThrowExpression
        public override bool Walk(ThrowExpression node) { return false; }
        public override void PostWalk(ThrowExpression node) { }

        // TypeBinaryExpression
        public override bool Walk(TypeBinaryExpression node) { return false; }
        public override void PostWalk(TypeBinaryExpression node) { }

        // VoidExpression
        public override bool Walk(VoidExpression node) { return false; }
        public override void PostWalk(VoidExpression node) { }

        // BlockStatement
        public override bool Walk(BlockStatement node) { return false; }
        public override void PostWalk(BlockStatement node) { }

        // BreakStatement
        public override bool Walk(BreakStatement node) { return false; }
        public override void PostWalk(BreakStatement node) { }

        // ContinueStatement
        public override bool Walk(ContinueStatement node) { return false; }
        public override void PostWalk(ContinueStatement node) { }

        // DebugStatement
        public override bool Walk(DebugStatement node) { return false; }
        public override void PostWalk(DebugStatement node) { }

        // DelStatement
        public override bool Walk(DelStatement node) { return false; }
        public override void PostWalk(DelStatement node) { }

        // DoStatement
        public override bool Walk(DoStatement node) { return false; }
        public override void PostWalk(DoStatement node) { }

        // EmptyStatement
        public override bool Walk(EmptyStatement node) { return false; }
        public override void PostWalk(EmptyStatement node) { }

        // ExpressionStatement
        public override bool Walk(ExpressionStatement node) { return false; }
        public override void PostWalk(ExpressionStatement node) { }

        // IfStatement
        public override bool Walk(IfStatement node) { return false; }
        public override void PostWalk(IfStatement node) { }

        // LabeledStatement
        public override bool Walk(LabeledStatement node) { return false; }
        public override void PostWalk(LabeledStatement node) { }

        // LoopStatement
        public override bool Walk(LoopStatement node) { return false; }
        public override void PostWalk(LoopStatement node) { }

        // ReturnStatement
        public override bool Walk(ReturnStatement node) { return false; }
        public override void PostWalk(ReturnStatement node) { }

        // ScopeStatement
        public override bool Walk(ScopeStatement node) { return false; }
        public override void PostWalk(ScopeStatement node) { }

        // SwitchStatement
        public override bool Walk(SwitchStatement node) { return false; }
        public override void PostWalk(SwitchStatement node) { }

        // TryFinallyStatement
        public override bool Walk(TryFinallyStatement node) { return false; }
        public override void PostWalk(TryFinallyStatement node) { }

        // TryStatement
        public override bool Walk(TryStatement node) { return false; }
        public override void PostWalk(TryStatement node) { }

        // YieldStatement
        public override bool Walk(YieldStatement node) { return false; }
        public override void PostWalk(YieldStatement node) { }

        // Arg
        public override bool Walk(Arg node) { return false; }
        public override void PostWalk(Arg node) { }

        // CodeBlock
        public override bool Walk(CodeBlock node) { return false; }
        public override void PostWalk(CodeBlock node) { }

        // GeneratorCodeBlock
        public override bool Walk(GeneratorCodeBlock node) { return false; }
        public override void PostWalk(GeneratorCodeBlock node) { }

        // IfStatementTest
        public override bool Walk(IfStatementTest node) { return false; }
        public override void PostWalk(IfStatementTest node) { }

        // TryStatementHandler
        public override bool Walk(TryStatementHandler node) { return false; }
        public override void PostWalk(TryStatementHandler node) { }
    }

    // *** END GENERATED CODE ***

    #endregion
}
