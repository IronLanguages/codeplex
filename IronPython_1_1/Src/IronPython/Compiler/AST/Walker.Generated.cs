/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using IronPython.Compiler;


namespace IronPython.Compiler.Ast {

    #region Generated AST Walker

    // *** BEGIN GENERATED CODE ***

    /// <summary>
    /// IAstWalker interface
    /// </summary>
    public interface IAstWalker {
        bool Walk(AndExpression node);
        void PostWalk(AndExpression node);

        bool Walk(BackQuoteExpression node);
        void PostWalk(BackQuoteExpression node);

        bool Walk(BinaryExpression node);
        void PostWalk(BinaryExpression node);

        bool Walk(CallExpression node);
        void PostWalk(CallExpression node);

        bool Walk(ConditionalExpression node);
        void PostWalk(ConditionalExpression node);

        bool Walk(ConstantExpression node);
        void PostWalk(ConstantExpression node);

        bool Walk(DictionaryExpression node);
        void PostWalk(DictionaryExpression node);

        bool Walk(ErrorExpression node);
        void PostWalk(ErrorExpression node);

        bool Walk(FieldExpression node);
        void PostWalk(FieldExpression node);

        bool Walk(GeneratorExpression node);
        void PostWalk(GeneratorExpression node);

        bool Walk(IndexExpression node);
        void PostWalk(IndexExpression node);

        bool Walk(LambdaExpression node);
        void PostWalk(LambdaExpression node);

        bool Walk(ListComprehension node);
        void PostWalk(ListComprehension node);

        bool Walk(ListExpression node);
        void PostWalk(ListExpression node);

        bool Walk(NameExpression node);
        void PostWalk(NameExpression node);

        bool Walk(OrExpression node);
        void PostWalk(OrExpression node);

        bool Walk(ParenthesisExpression node);
        void PostWalk(ParenthesisExpression node);

        bool Walk(SliceExpression node);
        void PostWalk(SliceExpression node);

        bool Walk(TupleExpression node);
        void PostWalk(TupleExpression node);

        bool Walk(UnaryExpression node);
        void PostWalk(UnaryExpression node);

        bool Walk(AssertStatement node);
        void PostWalk(AssertStatement node);

        bool Walk(AssignStatement node);
        void PostWalk(AssignStatement node);

        bool Walk(AugAssignStatement node);
        void PostWalk(AugAssignStatement node);

        bool Walk(BreakStatement node);
        void PostWalk(BreakStatement node);

        bool Walk(ClassDefinition node);
        void PostWalk(ClassDefinition node);

        bool Walk(ContinueStatement node);
        void PostWalk(ContinueStatement node);

        bool Walk(DelStatement node);
        void PostWalk(DelStatement node);

        bool Walk(ExecStatement node);
        void PostWalk(ExecStatement node);

        bool Walk(ExpressionStatement node);
        void PostWalk(ExpressionStatement node);

        bool Walk(ForStatement node);
        void PostWalk(ForStatement node);

        bool Walk(FromImportStatement node);
        void PostWalk(FromImportStatement node);

        bool Walk(FunctionDefinition node);
        void PostWalk(FunctionDefinition node);

        bool Walk(GlobalStatement node);
        void PostWalk(GlobalStatement node);

        bool Walk(GlobalSuite node);
        void PostWalk(GlobalSuite node);

        bool Walk(IfStatement node);
        void PostWalk(IfStatement node);

        bool Walk(ImportStatement node);
        void PostWalk(ImportStatement node);

        bool Walk(PassStatement node);
        void PostWalk(PassStatement node);

        bool Walk(PrintStatement node);
        void PostWalk(PrintStatement node);

        bool Walk(RaiseStatement node);
        void PostWalk(RaiseStatement node);

        bool Walk(ReturnStatement node);
        void PostWalk(ReturnStatement node);

        bool Walk(SuiteStatement node);
        void PostWalk(SuiteStatement node);

        bool Walk(TryFinallyStatement node);
        void PostWalk(TryFinallyStatement node);

        bool Walk(TryStatement node);
        void PostWalk(TryStatement node);

        bool Walk(WhileStatement node);
        void PostWalk(WhileStatement node);

        bool Walk(WithStatement node);
        void PostWalk(WithStatement node);

        bool Walk(YieldStatement node);
        void PostWalk(YieldStatement node);

        bool Walk(Arg node);
        void PostWalk(Arg node);

        bool Walk(DottedName node);
        void PostWalk(DottedName node);

        bool Walk(IfStatementTest node);
        void PostWalk(IfStatementTest node);

        bool Walk(ListComprehensionFor node);
        void PostWalk(ListComprehensionFor node);

        bool Walk(ListComprehensionIf node);
        void PostWalk(ListComprehensionIf node);

        bool Walk(TryStatementHandler node);
        void PostWalk(TryStatementHandler node);
    }


    /// <summary>
    /// AstWalker abstract class - the powerful walker (default result is true)
    /// </summary>
    public abstract class AstWalker : IAstWalker {
        // AndExpression
        public virtual bool Walk(AndExpression node) { return true; }
        public virtual void PostWalk(AndExpression node) { }

        // BackQuoteExpression
        public virtual bool Walk(BackQuoteExpression node) { return true; }
        public virtual void PostWalk(BackQuoteExpression node) { }

        // BinaryExpression
        public virtual bool Walk(BinaryExpression node) { return true; }
        public virtual void PostWalk(BinaryExpression node) { }

        // CallExpression
        public virtual bool Walk(CallExpression node) { return true; }
        public virtual void PostWalk(CallExpression node) { }

        // ConditionalExpression
        public virtual bool Walk(ConditionalExpression node) { return true; }
        public virtual void PostWalk(ConditionalExpression node) { }

        // ConstantExpression
        public virtual bool Walk(ConstantExpression node) { return true; }
        public virtual void PostWalk(ConstantExpression node) { }

        // DictionaryExpression
        public virtual bool Walk(DictionaryExpression node) { return true; }
        public virtual void PostWalk(DictionaryExpression node) { }

        // ErrorExpression
        public virtual bool Walk(ErrorExpression node) { return true; }
        public virtual void PostWalk(ErrorExpression node) { }

        // FieldExpression
        public virtual bool Walk(FieldExpression node) { return true; }
        public virtual void PostWalk(FieldExpression node) { }

        // GeneratorExpression
        public virtual bool Walk(GeneratorExpression node) { return true; }
        public virtual void PostWalk(GeneratorExpression node) { }

        // IndexExpression
        public virtual bool Walk(IndexExpression node) { return true; }
        public virtual void PostWalk(IndexExpression node) { }

        // LambdaExpression
        public virtual bool Walk(LambdaExpression node) { return true; }
        public virtual void PostWalk(LambdaExpression node) { }

        // ListComprehension
        public virtual bool Walk(ListComprehension node) { return true; }
        public virtual void PostWalk(ListComprehension node) { }

        // ListExpression
        public virtual bool Walk(ListExpression node) { return true; }
        public virtual void PostWalk(ListExpression node) { }

        // NameExpression
        public virtual bool Walk(NameExpression node) { return true; }
        public virtual void PostWalk(NameExpression node) { }

        // OrExpression
        public virtual bool Walk(OrExpression node) { return true; }
        public virtual void PostWalk(OrExpression node) { }

        // ParenthesisExpression
        public virtual bool Walk(ParenthesisExpression node) { return true; }
        public virtual void PostWalk(ParenthesisExpression node) { }

        // SliceExpression
        public virtual bool Walk(SliceExpression node) { return true; }
        public virtual void PostWalk(SliceExpression node) { }

        // TupleExpression
        public virtual bool Walk(TupleExpression node) { return true; }
        public virtual void PostWalk(TupleExpression node) { }

        // UnaryExpression
        public virtual bool Walk(UnaryExpression node) { return true; }
        public virtual void PostWalk(UnaryExpression node) { }

        // AssertStatement
        public virtual bool Walk(AssertStatement node) { return true; }
        public virtual void PostWalk(AssertStatement node) { }

        // AssignStatement
        public virtual bool Walk(AssignStatement node) { return true; }
        public virtual void PostWalk(AssignStatement node) { }

        // AugAssignStatement
        public virtual bool Walk(AugAssignStatement node) { return true; }
        public virtual void PostWalk(AugAssignStatement node) { }

        // BreakStatement
        public virtual bool Walk(BreakStatement node) { return true; }
        public virtual void PostWalk(BreakStatement node) { }

        // ClassDefinition
        public virtual bool Walk(ClassDefinition node) { return true; }
        public virtual void PostWalk(ClassDefinition node) { }

        // ContinueStatement
        public virtual bool Walk(ContinueStatement node) { return true; }
        public virtual void PostWalk(ContinueStatement node) { }

        // DelStatement
        public virtual bool Walk(DelStatement node) { return true; }
        public virtual void PostWalk(DelStatement node) { }

        // ExecStatement
        public virtual bool Walk(ExecStatement node) { return true; }
        public virtual void PostWalk(ExecStatement node) { }

        // ExpressionStatement
        public virtual bool Walk(ExpressionStatement node) { return true; }
        public virtual void PostWalk(ExpressionStatement node) { }

        // ForStatement
        public virtual bool Walk(ForStatement node) { return true; }
        public virtual void PostWalk(ForStatement node) { }

        // FromImportStatement
        public virtual bool Walk(FromImportStatement node) { return true; }
        public virtual void PostWalk(FromImportStatement node) { }

        // FunctionDefinition
        public virtual bool Walk(FunctionDefinition node) { return true; }
        public virtual void PostWalk(FunctionDefinition node) { }

        // GlobalStatement
        public virtual bool Walk(GlobalStatement node) { return true; }
        public virtual void PostWalk(GlobalStatement node) { }

        // GlobalSuite
        public virtual bool Walk(GlobalSuite node) { return true; }
        public virtual void PostWalk(GlobalSuite node) { }

        // IfStatement
        public virtual bool Walk(IfStatement node) { return true; }
        public virtual void PostWalk(IfStatement node) { }

        // ImportStatement
        public virtual bool Walk(ImportStatement node) { return true; }
        public virtual void PostWalk(ImportStatement node) { }

        // PassStatement
        public virtual bool Walk(PassStatement node) { return true; }
        public virtual void PostWalk(PassStatement node) { }

        // PrintStatement
        public virtual bool Walk(PrintStatement node) { return true; }
        public virtual void PostWalk(PrintStatement node) { }

        // RaiseStatement
        public virtual bool Walk(RaiseStatement node) { return true; }
        public virtual void PostWalk(RaiseStatement node) { }

        // ReturnStatement
        public virtual bool Walk(ReturnStatement node) { return true; }
        public virtual void PostWalk(ReturnStatement node) { }

        // SuiteStatement
        public virtual bool Walk(SuiteStatement node) { return true; }
        public virtual void PostWalk(SuiteStatement node) { }

        // TryFinallyStatement
        public virtual bool Walk(TryFinallyStatement node) { return true; }
        public virtual void PostWalk(TryFinallyStatement node) { }

        // TryStatement
        public virtual bool Walk(TryStatement node) { return true; }
        public virtual void PostWalk(TryStatement node) { }

        // WhileStatement
        public virtual bool Walk(WhileStatement node) { return true; }
        public virtual void PostWalk(WhileStatement node) { }

        // WithStatement
        public virtual bool Walk(WithStatement node) { return true; }
        public virtual void PostWalk(WithStatement node) { }

        // YieldStatement
        public virtual bool Walk(YieldStatement node) { return true; }
        public virtual void PostWalk(YieldStatement node) { }

        // Arg
        public virtual bool Walk(Arg node) { return true; }
        public virtual void PostWalk(Arg node) { }

        // DottedName
        public virtual bool Walk(DottedName node) { return true; }
        public virtual void PostWalk(DottedName node) { }

        // IfStatementTest
        public virtual bool Walk(IfStatementTest node) { return true; }
        public virtual void PostWalk(IfStatementTest node) { }

        // ListComprehensionFor
        public virtual bool Walk(ListComprehensionFor node) { return true; }
        public virtual void PostWalk(ListComprehensionFor node) { }

        // ListComprehensionIf
        public virtual bool Walk(ListComprehensionIf node) { return true; }
        public virtual void PostWalk(ListComprehensionIf node) { }

        // TryStatementHandler
        public virtual bool Walk(TryStatementHandler node) { return true; }
        public virtual void PostWalk(TryStatementHandler node) { }
    }


    /// <summary>
    /// AstWalkerNonRecursive abstract class - the powerful walker (default result is false)
    /// </summary>
    public abstract class AstWalkerNonRecursive : IAstWalker {
        // AndExpression
        public virtual bool Walk(AndExpression node) { return false; }
        public virtual void PostWalk(AndExpression node) { }

        // BackQuoteExpression
        public virtual bool Walk(BackQuoteExpression node) { return false; }
        public virtual void PostWalk(BackQuoteExpression node) { }

        // BinaryExpression
        public virtual bool Walk(BinaryExpression node) { return false; }
        public virtual void PostWalk(BinaryExpression node) { }

        // CallExpression
        public virtual bool Walk(CallExpression node) { return false; }
        public virtual void PostWalk(CallExpression node) { }

        // ConditionalExpression
        public virtual bool Walk(ConditionalExpression node) { return false; }
        public virtual void PostWalk(ConditionalExpression node) { }

        // ConstantExpression
        public virtual bool Walk(ConstantExpression node) { return false; }
        public virtual void PostWalk(ConstantExpression node) { }

        // DictionaryExpression
        public virtual bool Walk(DictionaryExpression node) { return false; }
        public virtual void PostWalk(DictionaryExpression node) { }

        // ErrorExpression
        public virtual bool Walk(ErrorExpression node) { return false; }
        public virtual void PostWalk(ErrorExpression node) { }

        // FieldExpression
        public virtual bool Walk(FieldExpression node) { return false; }
        public virtual void PostWalk(FieldExpression node) { }

        // GeneratorExpression
        public virtual bool Walk(GeneratorExpression node) { return false; }
        public virtual void PostWalk(GeneratorExpression node) { }

        // IndexExpression
        public virtual bool Walk(IndexExpression node) { return false; }
        public virtual void PostWalk(IndexExpression node) { }

        // LambdaExpression
        public virtual bool Walk(LambdaExpression node) { return false; }
        public virtual void PostWalk(LambdaExpression node) { }

        // ListComprehension
        public virtual bool Walk(ListComprehension node) { return false; }
        public virtual void PostWalk(ListComprehension node) { }

        // ListExpression
        public virtual bool Walk(ListExpression node) { return false; }
        public virtual void PostWalk(ListExpression node) { }

        // NameExpression
        public virtual bool Walk(NameExpression node) { return false; }
        public virtual void PostWalk(NameExpression node) { }

        // OrExpression
        public virtual bool Walk(OrExpression node) { return false; }
        public virtual void PostWalk(OrExpression node) { }

        // ParenthesisExpression
        public virtual bool Walk(ParenthesisExpression node) { return false; }
        public virtual void PostWalk(ParenthesisExpression node) { }

        // SliceExpression
        public virtual bool Walk(SliceExpression node) { return false; }
        public virtual void PostWalk(SliceExpression node) { }

        // TupleExpression
        public virtual bool Walk(TupleExpression node) { return false; }
        public virtual void PostWalk(TupleExpression node) { }

        // UnaryExpression
        public virtual bool Walk(UnaryExpression node) { return false; }
        public virtual void PostWalk(UnaryExpression node) { }

        // AssertStatement
        public virtual bool Walk(AssertStatement node) { return false; }
        public virtual void PostWalk(AssertStatement node) { }

        // AssignStatement
        public virtual bool Walk(AssignStatement node) { return false; }
        public virtual void PostWalk(AssignStatement node) { }

        // AugAssignStatement
        public virtual bool Walk(AugAssignStatement node) { return false; }
        public virtual void PostWalk(AugAssignStatement node) { }

        // BreakStatement
        public virtual bool Walk(BreakStatement node) { return false; }
        public virtual void PostWalk(BreakStatement node) { }

        // ClassDefinition
        public virtual bool Walk(ClassDefinition node) { return false; }
        public virtual void PostWalk(ClassDefinition node) { }

        // ContinueStatement
        public virtual bool Walk(ContinueStatement node) { return false; }
        public virtual void PostWalk(ContinueStatement node) { }

        // DelStatement
        public virtual bool Walk(DelStatement node) { return false; }
        public virtual void PostWalk(DelStatement node) { }

        // ExecStatement
        public virtual bool Walk(ExecStatement node) { return false; }
        public virtual void PostWalk(ExecStatement node) { }

        // ExpressionStatement
        public virtual bool Walk(ExpressionStatement node) { return false; }
        public virtual void PostWalk(ExpressionStatement node) { }

        // ForStatement
        public virtual bool Walk(ForStatement node) { return false; }
        public virtual void PostWalk(ForStatement node) { }

        // FromImportStatement
        public virtual bool Walk(FromImportStatement node) { return false; }
        public virtual void PostWalk(FromImportStatement node) { }

        // FunctionDefinition
        public virtual bool Walk(FunctionDefinition node) { return false; }
        public virtual void PostWalk(FunctionDefinition node) { }

        // GlobalStatement
        public virtual bool Walk(GlobalStatement node) { return false; }
        public virtual void PostWalk(GlobalStatement node) { }

        // GlobalSuite
        public virtual bool Walk(GlobalSuite node) { return false; }
        public virtual void PostWalk(GlobalSuite node) { }

        // IfStatement
        public virtual bool Walk(IfStatement node) { return false; }
        public virtual void PostWalk(IfStatement node) { }

        // ImportStatement
        public virtual bool Walk(ImportStatement node) { return false; }
        public virtual void PostWalk(ImportStatement node) { }

        // PassStatement
        public virtual bool Walk(PassStatement node) { return false; }
        public virtual void PostWalk(PassStatement node) { }

        // PrintStatement
        public virtual bool Walk(PrintStatement node) { return false; }
        public virtual void PostWalk(PrintStatement node) { }

        // RaiseStatement
        public virtual bool Walk(RaiseStatement node) { return false; }
        public virtual void PostWalk(RaiseStatement node) { }

        // ReturnStatement
        public virtual bool Walk(ReturnStatement node) { return false; }
        public virtual void PostWalk(ReturnStatement node) { }

        // SuiteStatement
        public virtual bool Walk(SuiteStatement node) { return false; }
        public virtual void PostWalk(SuiteStatement node) { }

        // TryFinallyStatement
        public virtual bool Walk(TryFinallyStatement node) { return false; }
        public virtual void PostWalk(TryFinallyStatement node) { }

        // TryStatement
        public virtual bool Walk(TryStatement node) { return false; }
        public virtual void PostWalk(TryStatement node) { }

        // WhileStatement
        public virtual bool Walk(WhileStatement node) { return false; }
        public virtual void PostWalk(WhileStatement node) { }

        // WithStatement
        public virtual bool Walk(WithStatement node) { return false; }
        public virtual void PostWalk(WithStatement node) { }

        // YieldStatement
        public virtual bool Walk(YieldStatement node) { return false; }
        public virtual void PostWalk(YieldStatement node) { }

        // Arg
        public virtual bool Walk(Arg node) { return false; }
        public virtual void PostWalk(Arg node) { }

        // DottedName
        public virtual bool Walk(DottedName node) { return false; }
        public virtual void PostWalk(DottedName node) { }

        // IfStatementTest
        public virtual bool Walk(IfStatementTest node) { return false; }
        public virtual void PostWalk(IfStatementTest node) { }

        // ListComprehensionFor
        public virtual bool Walk(ListComprehensionFor node) { return false; }
        public virtual void PostWalk(ListComprehensionFor node) { }

        // ListComprehensionIf
        public virtual bool Walk(ListComprehensionIf node) { return false; }
        public virtual void PostWalk(ListComprehensionIf node) { }

        // TryStatementHandler
        public virtual bool Walk(TryStatementHandler node) { return false; }
        public virtual void PostWalk(TryStatementHandler node) { }
    }

    // *** END GENERATED CODE ***

    #endregion
}
