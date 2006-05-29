/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using IronPython.Compiler;


namespace IronPython.Compiler {

    #region Generated AST Walker

    // *** BEGIN GENERATED CODE ***

    /// <summary>
    /// IAstWalker interface
    /// </summary>
    public interface IAstWalker {
        bool Walk(AndExpr node);
        void PostWalk(AndExpr node);

        bool Walk(BackquoteExpr node);
        void PostWalk(BackquoteExpr node);

        bool Walk(BinaryExpr node);
        void PostWalk(BinaryExpr node);

        bool Walk(CallExpr node);
        void PostWalk(CallExpr node);

        bool Walk(ConstantExpr node);
        void PostWalk(ConstantExpr node);

        bool Walk(DictExpr node);
        void PostWalk(DictExpr node);

        bool Walk(ErrorExpr node);
        void PostWalk(ErrorExpr node);

        bool Walk(FieldExpr node);
        void PostWalk(FieldExpr node);

        bool Walk(GenExpr node);
        void PostWalk(GenExpr node);

        bool Walk(IndexExpr node);
        void PostWalk(IndexExpr node);

        bool Walk(LambdaExpr node);
        void PostWalk(LambdaExpr node);

        bool Walk(ListComp node);
        void PostWalk(ListComp node);

        bool Walk(ListExpr node);
        void PostWalk(ListExpr node);

        bool Walk(NameExpr node);
        void PostWalk(NameExpr node);

        bool Walk(OrExpr node);
        void PostWalk(OrExpr node);

        bool Walk(ParenExpr node);
        void PostWalk(ParenExpr node);

        bool Walk(SliceExpr node);
        void PostWalk(SliceExpr node);

        bool Walk(TupleExpr node);
        void PostWalk(TupleExpr node);

        bool Walk(UnaryExpr node);
        void PostWalk(UnaryExpr node);

        bool Walk(AssertStmt node);
        void PostWalk(AssertStmt node);

        bool Walk(AssignStmt node);
        void PostWalk(AssignStmt node);

        bool Walk(AugAssignStmt node);
        void PostWalk(AugAssignStmt node);

        bool Walk(BreakStmt node);
        void PostWalk(BreakStmt node);

        bool Walk(ClassDef node);
        void PostWalk(ClassDef node);

        bool Walk(ContinueStmt node);
        void PostWalk(ContinueStmt node);

        bool Walk(DelStmt node);
        void PostWalk(DelStmt node);

        bool Walk(ExecStmt node);
        void PostWalk(ExecStmt node);

        bool Walk(ExprStmt node);
        void PostWalk(ExprStmt node);

        bool Walk(ForStmt node);
        void PostWalk(ForStmt node);

        bool Walk(FromImportStmt node);
        void PostWalk(FromImportStmt node);

        bool Walk(FuncDef node);
        void PostWalk(FuncDef node);

        bool Walk(GlobalStmt node);
        void PostWalk(GlobalStmt node);

        bool Walk(GlobalSuite node);
        void PostWalk(GlobalSuite node);

        bool Walk(IfStmt node);
        void PostWalk(IfStmt node);

        bool Walk(ImportStmt node);
        void PostWalk(ImportStmt node);

        bool Walk(PassStmt node);
        void PostWalk(PassStmt node);

        bool Walk(PrintStmt node);
        void PostWalk(PrintStmt node);

        bool Walk(RaiseStmt node);
        void PostWalk(RaiseStmt node);

        bool Walk(ReturnStmt node);
        void PostWalk(ReturnStmt node);

        bool Walk(SuiteStmt node);
        void PostWalk(SuiteStmt node);

        bool Walk(TryFinallyStmt node);
        void PostWalk(TryFinallyStmt node);

        bool Walk(TryStmt node);
        void PostWalk(TryStmt node);

        bool Walk(WhileStmt node);
        void PostWalk(WhileStmt node);

        bool Walk(YieldStmt node);
        void PostWalk(YieldStmt node);

        bool Walk(Arg node);
        void PostWalk(Arg node);

        bool Walk(DottedName node);
        void PostWalk(DottedName node);

        bool Walk(IfStmtTest node);
        void PostWalk(IfStmtTest node);

        bool Walk(ListCompFor node);
        void PostWalk(ListCompFor node);

        bool Walk(ListCompIf node);
        void PostWalk(ListCompIf node);

        bool Walk(TryStmtHandler node);
        void PostWalk(TryStmtHandler node);
    }


    /// <summary>
    /// AstWalker abstract class - the powerful walker (default result is true)
    /// </summary>
    public abstract class AstWalker : IAstWalker {
        // AndExpr
        public virtual bool Walk(AndExpr node) { return true; }
        public virtual void PostWalk(AndExpr node) { }

        // BackquoteExpr
        public virtual bool Walk(BackquoteExpr node) { return true; }
        public virtual void PostWalk(BackquoteExpr node) { }

        // BinaryExpr
        public virtual bool Walk(BinaryExpr node) { return true; }
        public virtual void PostWalk(BinaryExpr node) { }

        // CallExpr
        public virtual bool Walk(CallExpr node) { return true; }
        public virtual void PostWalk(CallExpr node) { }

        // ConstantExpr
        public virtual bool Walk(ConstantExpr node) { return true; }
        public virtual void PostWalk(ConstantExpr node) { }

        // DictExpr
        public virtual bool Walk(DictExpr node) { return true; }
        public virtual void PostWalk(DictExpr node) { }

        // ErrorExpr
        public virtual bool Walk(ErrorExpr node) { return true; }
        public virtual void PostWalk(ErrorExpr node) { }

        // FieldExpr
        public virtual bool Walk(FieldExpr node) { return true; }
        public virtual void PostWalk(FieldExpr node) { }

        // GenExpr
        public virtual bool Walk(GenExpr node) { return true; }
        public virtual void PostWalk(GenExpr node) { }

        // IndexExpr
        public virtual bool Walk(IndexExpr node) { return true; }
        public virtual void PostWalk(IndexExpr node) { }

        // LambdaExpr
        public virtual bool Walk(LambdaExpr node) { return true; }
        public virtual void PostWalk(LambdaExpr node) { }

        // ListComp
        public virtual bool Walk(ListComp node) { return true; }
        public virtual void PostWalk(ListComp node) { }

        // ListExpr
        public virtual bool Walk(ListExpr node) { return true; }
        public virtual void PostWalk(ListExpr node) { }

        // NameExpr
        public virtual bool Walk(NameExpr node) { return true; }
        public virtual void PostWalk(NameExpr node) { }

        // OrExpr
        public virtual bool Walk(OrExpr node) { return true; }
        public virtual void PostWalk(OrExpr node) { }

        // ParenExpr
        public virtual bool Walk(ParenExpr node) { return true; }
        public virtual void PostWalk(ParenExpr node) { }

        // SliceExpr
        public virtual bool Walk(SliceExpr node) { return true; }
        public virtual void PostWalk(SliceExpr node) { }

        // TupleExpr
        public virtual bool Walk(TupleExpr node) { return true; }
        public virtual void PostWalk(TupleExpr node) { }

        // UnaryExpr
        public virtual bool Walk(UnaryExpr node) { return true; }
        public virtual void PostWalk(UnaryExpr node) { }

        // AssertStmt
        public virtual bool Walk(AssertStmt node) { return true; }
        public virtual void PostWalk(AssertStmt node) { }

        // AssignStmt
        public virtual bool Walk(AssignStmt node) { return true; }
        public virtual void PostWalk(AssignStmt node) { }

        // AugAssignStmt
        public virtual bool Walk(AugAssignStmt node) { return true; }
        public virtual void PostWalk(AugAssignStmt node) { }

        // BreakStmt
        public virtual bool Walk(BreakStmt node) { return true; }
        public virtual void PostWalk(BreakStmt node) { }

        // ClassDef
        public virtual bool Walk(ClassDef node) { return true; }
        public virtual void PostWalk(ClassDef node) { }

        // ContinueStmt
        public virtual bool Walk(ContinueStmt node) { return true; }
        public virtual void PostWalk(ContinueStmt node) { }

        // DelStmt
        public virtual bool Walk(DelStmt node) { return true; }
        public virtual void PostWalk(DelStmt node) { }

        // ExecStmt
        public virtual bool Walk(ExecStmt node) { return true; }
        public virtual void PostWalk(ExecStmt node) { }

        // ExprStmt
        public virtual bool Walk(ExprStmt node) { return true; }
        public virtual void PostWalk(ExprStmt node) { }

        // ForStmt
        public virtual bool Walk(ForStmt node) { return true; }
        public virtual void PostWalk(ForStmt node) { }

        // FromImportStmt
        public virtual bool Walk(FromImportStmt node) { return true; }
        public virtual void PostWalk(FromImportStmt node) { }

        // FuncDef
        public virtual bool Walk(FuncDef node) { return true; }
        public virtual void PostWalk(FuncDef node) { }

        // GlobalStmt
        public virtual bool Walk(GlobalStmt node) { return true; }
        public virtual void PostWalk(GlobalStmt node) { }

        // GlobalSuite
        public virtual bool Walk(GlobalSuite node) { return true; }
        public virtual void PostWalk(GlobalSuite node) { }

        // IfStmt
        public virtual bool Walk(IfStmt node) { return true; }
        public virtual void PostWalk(IfStmt node) { }

        // ImportStmt
        public virtual bool Walk(ImportStmt node) { return true; }
        public virtual void PostWalk(ImportStmt node) { }

        // PassStmt
        public virtual bool Walk(PassStmt node) { return true; }
        public virtual void PostWalk(PassStmt node) { }

        // PrintStmt
        public virtual bool Walk(PrintStmt node) { return true; }
        public virtual void PostWalk(PrintStmt node) { }

        // RaiseStmt
        public virtual bool Walk(RaiseStmt node) { return true; }
        public virtual void PostWalk(RaiseStmt node) { }

        // ReturnStmt
        public virtual bool Walk(ReturnStmt node) { return true; }
        public virtual void PostWalk(ReturnStmt node) { }

        // SuiteStmt
        public virtual bool Walk(SuiteStmt node) { return true; }
        public virtual void PostWalk(SuiteStmt node) { }

        // TryFinallyStmt
        public virtual bool Walk(TryFinallyStmt node) { return true; }
        public virtual void PostWalk(TryFinallyStmt node) { }

        // TryStmt
        public virtual bool Walk(TryStmt node) { return true; }
        public virtual void PostWalk(TryStmt node) { }

        // WhileStmt
        public virtual bool Walk(WhileStmt node) { return true; }
        public virtual void PostWalk(WhileStmt node) { }

        // YieldStmt
        public virtual bool Walk(YieldStmt node) { return true; }
        public virtual void PostWalk(YieldStmt node) { }

        // Arg
        public virtual bool Walk(Arg node) { return true; }
        public virtual void PostWalk(Arg node) { }

        // DottedName
        public virtual bool Walk(DottedName node) { return true; }
        public virtual void PostWalk(DottedName node) { }

        // IfStmtTest
        public virtual bool Walk(IfStmtTest node) { return true; }
        public virtual void PostWalk(IfStmtTest node) { }

        // ListCompFor
        public virtual bool Walk(ListCompFor node) { return true; }
        public virtual void PostWalk(ListCompFor node) { }

        // ListCompIf
        public virtual bool Walk(ListCompIf node) { return true; }
        public virtual void PostWalk(ListCompIf node) { }

        // TryStmtHandler
        public virtual bool Walk(TryStmtHandler node) { return true; }
        public virtual void PostWalk(TryStmtHandler node) { }
    }


    /// <summary>
    /// AstWalkerNonRecursive abstract class - the powerful walker (default result is false)
    /// </summary>
    public abstract class AstWalkerNonRecursive : IAstWalker {
        // AndExpr
        public virtual bool Walk(AndExpr node) { return false; }
        public virtual void PostWalk(AndExpr node) { }

        // BackquoteExpr
        public virtual bool Walk(BackquoteExpr node) { return false; }
        public virtual void PostWalk(BackquoteExpr node) { }

        // BinaryExpr
        public virtual bool Walk(BinaryExpr node) { return false; }
        public virtual void PostWalk(BinaryExpr node) { }

        // CallExpr
        public virtual bool Walk(CallExpr node) { return false; }
        public virtual void PostWalk(CallExpr node) { }

        // ConstantExpr
        public virtual bool Walk(ConstantExpr node) { return false; }
        public virtual void PostWalk(ConstantExpr node) { }

        // DictExpr
        public virtual bool Walk(DictExpr node) { return false; }
        public virtual void PostWalk(DictExpr node) { }

        // ErrorExpr
        public virtual bool Walk(ErrorExpr node) { return false; }
        public virtual void PostWalk(ErrorExpr node) { }

        // FieldExpr
        public virtual bool Walk(FieldExpr node) { return false; }
        public virtual void PostWalk(FieldExpr node) { }

        // GenExpr
        public virtual bool Walk(GenExpr node) { return false; }
        public virtual void PostWalk(GenExpr node) { }

        // IndexExpr
        public virtual bool Walk(IndexExpr node) { return false; }
        public virtual void PostWalk(IndexExpr node) { }

        // LambdaExpr
        public virtual bool Walk(LambdaExpr node) { return false; }
        public virtual void PostWalk(LambdaExpr node) { }

        // ListComp
        public virtual bool Walk(ListComp node) { return false; }
        public virtual void PostWalk(ListComp node) { }

        // ListExpr
        public virtual bool Walk(ListExpr node) { return false; }
        public virtual void PostWalk(ListExpr node) { }

        // NameExpr
        public virtual bool Walk(NameExpr node) { return false; }
        public virtual void PostWalk(NameExpr node) { }

        // OrExpr
        public virtual bool Walk(OrExpr node) { return false; }
        public virtual void PostWalk(OrExpr node) { }

        // ParenExpr
        public virtual bool Walk(ParenExpr node) { return false; }
        public virtual void PostWalk(ParenExpr node) { }

        // SliceExpr
        public virtual bool Walk(SliceExpr node) { return false; }
        public virtual void PostWalk(SliceExpr node) { }

        // TupleExpr
        public virtual bool Walk(TupleExpr node) { return false; }
        public virtual void PostWalk(TupleExpr node) { }

        // UnaryExpr
        public virtual bool Walk(UnaryExpr node) { return false; }
        public virtual void PostWalk(UnaryExpr node) { }

        // AssertStmt
        public virtual bool Walk(AssertStmt node) { return false; }
        public virtual void PostWalk(AssertStmt node) { }

        // AssignStmt
        public virtual bool Walk(AssignStmt node) { return false; }
        public virtual void PostWalk(AssignStmt node) { }

        // AugAssignStmt
        public virtual bool Walk(AugAssignStmt node) { return false; }
        public virtual void PostWalk(AugAssignStmt node) { }

        // BreakStmt
        public virtual bool Walk(BreakStmt node) { return false; }
        public virtual void PostWalk(BreakStmt node) { }

        // ClassDef
        public virtual bool Walk(ClassDef node) { return false; }
        public virtual void PostWalk(ClassDef node) { }

        // ContinueStmt
        public virtual bool Walk(ContinueStmt node) { return false; }
        public virtual void PostWalk(ContinueStmt node) { }

        // DelStmt
        public virtual bool Walk(DelStmt node) { return false; }
        public virtual void PostWalk(DelStmt node) { }

        // ExecStmt
        public virtual bool Walk(ExecStmt node) { return false; }
        public virtual void PostWalk(ExecStmt node) { }

        // ExprStmt
        public virtual bool Walk(ExprStmt node) { return false; }
        public virtual void PostWalk(ExprStmt node) { }

        // ForStmt
        public virtual bool Walk(ForStmt node) { return false; }
        public virtual void PostWalk(ForStmt node) { }

        // FromImportStmt
        public virtual bool Walk(FromImportStmt node) { return false; }
        public virtual void PostWalk(FromImportStmt node) { }

        // FuncDef
        public virtual bool Walk(FuncDef node) { return false; }
        public virtual void PostWalk(FuncDef node) { }

        // GlobalStmt
        public virtual bool Walk(GlobalStmt node) { return false; }
        public virtual void PostWalk(GlobalStmt node) { }

        // GlobalSuite
        public virtual bool Walk(GlobalSuite node) { return false; }
        public virtual void PostWalk(GlobalSuite node) { }

        // IfStmt
        public virtual bool Walk(IfStmt node) { return false; }
        public virtual void PostWalk(IfStmt node) { }

        // ImportStmt
        public virtual bool Walk(ImportStmt node) { return false; }
        public virtual void PostWalk(ImportStmt node) { }

        // PassStmt
        public virtual bool Walk(PassStmt node) { return false; }
        public virtual void PostWalk(PassStmt node) { }

        // PrintStmt
        public virtual bool Walk(PrintStmt node) { return false; }
        public virtual void PostWalk(PrintStmt node) { }

        // RaiseStmt
        public virtual bool Walk(RaiseStmt node) { return false; }
        public virtual void PostWalk(RaiseStmt node) { }

        // ReturnStmt
        public virtual bool Walk(ReturnStmt node) { return false; }
        public virtual void PostWalk(ReturnStmt node) { }

        // SuiteStmt
        public virtual bool Walk(SuiteStmt node) { return false; }
        public virtual void PostWalk(SuiteStmt node) { }

        // TryFinallyStmt
        public virtual bool Walk(TryFinallyStmt node) { return false; }
        public virtual void PostWalk(TryFinallyStmt node) { }

        // TryStmt
        public virtual bool Walk(TryStmt node) { return false; }
        public virtual void PostWalk(TryStmt node) { }

        // WhileStmt
        public virtual bool Walk(WhileStmt node) { return false; }
        public virtual void PostWalk(WhileStmt node) { }

        // YieldStmt
        public virtual bool Walk(YieldStmt node) { return false; }
        public virtual void PostWalk(YieldStmt node) { }

        // Arg
        public virtual bool Walk(Arg node) { return false; }
        public virtual void PostWalk(Arg node) { }

        // DottedName
        public virtual bool Walk(DottedName node) { return false; }
        public virtual void PostWalk(DottedName node) { }

        // IfStmtTest
        public virtual bool Walk(IfStmtTest node) { return false; }
        public virtual void PostWalk(IfStmtTest node) { }

        // ListCompFor
        public virtual bool Walk(ListCompFor node) { return false; }
        public virtual void PostWalk(ListCompFor node) { }

        // ListCompIf
        public virtual bool Walk(ListCompIf node) { return false; }
        public virtual void PostWalk(ListCompIf node) { }

        // TryStmtHandler
        public virtual bool Walk(TryStmtHandler node) { return false; }
        public virtual void PostWalk(TryStmtHandler node) { }
    }

    // *** END GENERATED CODE ***

    #endregion
}
