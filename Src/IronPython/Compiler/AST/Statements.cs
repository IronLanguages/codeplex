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

using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using System.CodeDom;

using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.CodeDom;

using IronPython.Compiler.Generation;

namespace IronPython.Compiler.AST {
    public abstract class Stmt : Node {
        public static readonly object NextStmt = new object();

        public virtual object Execute(NameEnv env) {
            throw new NotImplementedException("execute: " + this);
        }

        internal abstract void Emit(CodeGen cg);

        public virtual string GetDocString() {
            return null;
        }
    }

    public class SuiteStmt : Stmt {
        private readonly Stmt[] stmts;

        public IList<Stmt> Statements {
            get { return stmts; }
        } 

        public SuiteStmt(Stmt[] stmts) {
            this.stmts = stmts;
        }

        public override object Execute(NameEnv env) {
            object ret = Stmt.NextStmt;
            foreach (Stmt stmt in stmts) {
                ret = stmt.Execute(env);
                if (ret != Stmt.NextStmt) break;
            }
            return ret;
        }
       
        internal override void Emit(CodeGen cg) {
            // Should emit nop for the colon?
            foreach (Stmt stmt in stmts) {
                stmt.Emit(cg);
            }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (Stmt stmt in stmts) stmt.Walk(w);
            }
            w.PostWalk(this);
        }

        public override string GetDocString() {
            if (stmts.Length > 0 && stmts[0] is ExprStmt) {
                ExprStmt es = (ExprStmt)stmts[0];
                if (es.Expression is ConstantExpr) {
                    object val = ((ConstantExpr)es.Expression).Value;
                    if (val is string && !Options.StripDocStrings) return (string)val;
                }
            }
            return null;
        }
    }

    public class IfStmt : Stmt {
        private readonly IfStmtTest[] tests;
        private readonly Stmt elseStmt;

        public IfStmt(IfStmtTest[] tests, Stmt else_) {
            this.tests = tests; this.elseStmt = else_;
        }

        public IList<IfStmtTest> Tests {
            get { return tests; }
        }

        public Stmt ElseStatement {
            get { return elseStmt; }
        }

        public override object Execute(NameEnv env) {
            foreach (IfStmtTest t in tests) {
                object val = t.Test.Evaluate(env);
                if (Ops.IsTrue(val)) {
                    return t.Body.Execute(env);
                }
            }
            if (elseStmt != null) {
                return elseStmt.Execute(env);
            }
            return NextStmt;
        }

        internal override void Emit(CodeGen cg) {
            Label eoi = cg.DefineLabel();
            foreach (IfStmtTest t in tests) {
                Label next = cg.DefineLabel();
                cg.EmitPosition(t.Start, t.Header);
                cg.EmitTestTrue(t.Test);
                cg.Emit(OpCodes.Brfalse, next);
                t.Body.Emit(cg);
                // optimize no else case
                cg.Emit(OpCodes.Br, eoi);
                cg.MarkLabel(next);
            }
            if (elseStmt != null) {
                elseStmt.Emit(cg);
            }
            cg.MarkLabel(eoi);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (IfStmtTest t in tests) t.Walk(w);
                if (elseStmt != null) elseStmt.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class IfStmtTest : Node {
        private Location header;
        private readonly Expr test;
        private Stmt body;

        public IfStmtTest(Expr test, Stmt body) {
            this.test = test; this.body = body;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }
        
        public Expr Test {
            get { return test; }
        }

        public Stmt Body {
            get { return body; }
            set { body = value; }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                test.Walk(w);
                body.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class WhileStmt : Stmt {
        private Location header;
        private readonly Expr test;
        private readonly Stmt body;
        private readonly Stmt elseStmt;

        public WhileStmt(Expr test, Stmt body, Stmt else_) {
            this.test = test; this.body = body; this.elseStmt = else_;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public Expr Test {
            get { return test; }
        }

        public Stmt Body {
            get { return body; }
        }

        public Stmt ElseStatement {
            get { return elseStmt; }
        } 

        public override object Execute(NameEnv env) {
            object ret = NextStmt;
            while (Ops.IsTrue(test.Evaluate(env))) {
                ret = body.Execute(env);
                if (ret != NextStmt) break;
            }
            return ret;
            //			if (else_ != null) {
            //				else_.exec(env);
            //			}
        }

        internal override void Emit(CodeGen cg) {
            Label eol = cg.DefineLabel();
            Label breakTarget = cg.DefineLabel();
            Label continueTarget = cg.DefineLabel();

            cg.MarkLabel(continueTarget);

            cg.EmitPosition(Start, header);
            cg.EmitTestTrue(test);
            cg.Emit(OpCodes.Brfalse, eol);

            cg.PushTargets(breakTarget, continueTarget);

            body.Emit(cg);

            cg.Emit(OpCodes.Br, continueTarget);

            cg.PopTargets();

            cg.MarkLabel(eol);
            if (elseStmt != null) {
                elseStmt.Emit(cg);
            }
            cg.MarkLabel(breakTarget);
        }
        
        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                test.Walk(w);
                body.Walk(w);
                if (elseStmt != null) elseStmt.Walk(w);
            }
            w.PostWalk(this);
        }
        public void SetLoc(Location start, Location header, Location end) {
            this.Start = start;
            this.header = header;
            this.End = end;
        }
    }

    public class ForStmt : Stmt {
        private Location header;
        private readonly Expr lhs;
        private Expr list;
        private Stmt body;
        private readonly Stmt elseStmt;

        public ForStmt(Expr lhs, Expr list, Stmt body, Stmt else_) {
            this.lhs = lhs; this.list = list;
            this.body = body; this.elseStmt = else_;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public Expr Left {
            get { return lhs; }
        }

        public Expr List {
            get { return list; }
            set { list = value; }
        }

        public Stmt Body {
            get { return body; }
            set { body = value; }
        }

        public Stmt ElseStatement {
            get { return elseStmt; }
        }

        public override object Execute(NameEnv env) {
            object ret = Stmt.NextStmt;

            IEnumerator i = Ops.GetEnumerator(list.Evaluate(env));

            while (i.MoveNext()) {
                lhs.Assign(i.Current, env);
                ret = body.Execute(env);
                if (ret != NextStmt) return ret;
            }

            return ret;
            //			if (else_ != null) {
            //				else_.exec(env);
            //			}
        }

        internal override void Emit(CodeGen cg) {
            Label eol = cg.DefineLabel();
            Label breakTarget = cg.DefineLabel();
            Label continueTarget = cg.DefineLabel();

            cg.EmitPosition(Start, header);

            list.Emit(cg);
            cg.EmitCall(typeof(Ops), "GetEnumerator");

            Slot iter;
            if (cg.IsGenerator()) {
                iter = cg.Names.GetTempSlot("iter", typeof(IEnumerator));
            } else {
                iter = cg.GetLocalTmp(typeof(IEnumerator));
            }

            iter.EmitSet(cg);

            cg.MarkLabel(continueTarget);
            iter.EmitGet(cg);
            cg.EmitCall(typeof(IEnumerator), "MoveNext");

            cg.Emit(OpCodes.Brfalse, eol);

            cg.PushTargets(breakTarget, continueTarget);

            iter.EmitGet(cg);
            cg.EmitCall(typeof(IEnumerator).GetProperty("Current").GetGetMethod());
            lhs.EmitSet(cg);

            body.Emit(cg);

            cg.Emit(OpCodes.Br, continueTarget);

            cg.PopTargets();

            cg.MarkLabel(eol);
            if (elseStmt != null) {
                elseStmt.Emit(cg);
            }
            cg.MarkLabel(breakTarget);

            if (!cg.IsGenerator()) {
                cg.FreeLocalTmp(iter);
            }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                lhs.Walk(w);
                list.Walk(w);
                body.Walk(w);
                if (elseStmt != null) elseStmt.Walk(w);
            }
            w.PostWalk(this);
        }

        // For uses one local slot for the iter variable
        public int LocalSlots {
            get {
                return 1;
            }
        }
    }

    public class TryStmt : Stmt {
        private Location header;
        private readonly Stmt body;
        private readonly TryStmtHandler[] handlers;
        private readonly Stmt elseStmt;
        private bool yieldInExcept = false;
        private List<YieldTarget> yieldTargets = new List<YieldTarget>();

        public TryStmt(Stmt body, TryStmtHandler[] handlers, Stmt else_) {
            this.body = body; this.handlers = handlers; this.elseStmt = else_;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public Stmt Body {
            get { return body; }
        }

        public IList<TryStmtHandler> Handlers {
            get { return handlers; }
        }

        public Stmt ElseStatement {
            get { return elseStmt; }
        }

        public bool YieldInExcept {
            get { return yieldInExcept; }
            set { yieldInExcept = value; }
        }

        public IList<YieldTarget> YieldTargets {
            get { return yieldTargets; }
        }

        public void AddYieldTarget(YieldTarget t) {
            yieldTargets.Add(t);
        }

        internal override void Emit(CodeGen cg) {
            Slot choiceVar = null;
            cg.EmitPosition(Start, header);

            if (yieldTargets.Count > 0) {
                Label startOfBlock = cg.DefineLabel();
                choiceVar = cg.GetLocalTmp(typeof(int));
                cg.EmitInt(-1);
                choiceVar.EmitSet(cg);
                cg.Emit(OpCodes.Br, startOfBlock);

                int index = 0;
                foreach (YieldTarget yt in yieldTargets) {
                    cg.MarkLabel(yt.topBranchTarget);
                    cg.EmitInt(index++);
                    choiceVar.EmitSet(cg);
                    cg.Emit(OpCodes.Br, startOfBlock);
                }

                cg.MarkLabel(startOfBlock);
            }

            Label afterCatch = new Label();
            Label afterElse = cg.DefineLabel();

            cg.PushTryBlock();
            cg.BeginExceptionBlock();

            if (yieldTargets.Count > 0) {
                int index = 0;
                foreach (YieldTarget yt in yieldTargets) {
                    choiceVar.EmitGet(cg);
                    cg.EmitInt(index);
                    cg.Emit(OpCodes.Beq, yt.tryBranchTarget);
                    index++;
                }
                cg.FreeLocalTmp(choiceVar);
            }

            body.Emit(cg);
            if (yieldInExcept) {
                afterCatch = cg.DefineLabel();
                cg.Emit(OpCodes.Leave, afterCatch);
            }
            cg.BeginCatchBlock(typeof(Exception));
            // Extract state from the carrier exception
            cg.EmitCallerContext();
            cg.EmitCall(typeof(Ops), "ExtractException");
            Slot pyExc = cg.GetLocalTmp(typeof(object));
            Slot tmpExc = cg.GetLocalTmp(typeof(object));
            pyExc.EmitSet(cg);
            if (yieldInExcept) {
                cg.EndExceptionBlock();
                cg.PopTargets();
            }

            foreach (TryStmtHandler handler in handlers) {
                cg.EmitPosition(handler.Start, handler.Header);
                Label next = cg.DefineLabel();
                if (handler.Test != null) {
                    pyExc.EmitGet(cg);
                    handler.Test.Emit(cg);
                    cg.EmitCall(typeof(Ops), "CheckException");
                    if (handler.Target != null) {
                        tmpExc.EmitSet(cg);
                        tmpExc.EmitGet(cg);
                    }
                    cg.EmitPythonNone();
                    cg.Emit(OpCodes.Ceq);
                    cg.Emit(OpCodes.Brtrue, next);
                }

                if (handler.Target != null) {
                    tmpExc.EmitGet(cg);
                    handler.Target.EmitSet(cg);
                }

                cg.PushExceptionBlock(Targets.TargetBlockType.Catch, null);

                handler.Body.Emit(cg);
                cg.EmitCallerContext();
                cg.EmitCall(typeof(Ops), "ClearException");

                cg.PopTargets();

                if (yieldInExcept) {
                    cg.Emit(OpCodes.Br, afterElse);
                } else {
                    cg.Emit(OpCodes.Leave, afterElse);
                }
                cg.MarkLabel(next);
            }

            cg.FreeLocalTmp(tmpExc);
            if (yieldInExcept) {
                pyExc.EmitGet(cg);
                cg.Emit(OpCodes.Throw);
                cg.MarkLabel(afterCatch);
            } else {
                cg.Emit(OpCodes.Rethrow);
                cg.EndExceptionBlock();
                cg.PopTargets();
            }

            if (elseStmt != null) {
                elseStmt.Emit(cg);
            }
            cg.MarkLabel(afterElse);

            cg.FreeLocalTmp(pyExc);

            yieldTargets.Clear();
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                body.Walk(w);
                foreach (TryStmtHandler handler in handlers) handler.Walk(w);
                if (elseStmt != null) elseStmt.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class TryStmtHandler : Node {
        private Location header;
        private readonly Expr test, target;
        private readonly Stmt body;

        public TryStmtHandler(Expr test, Expr target, Stmt body) {
            this.test = test; this.target = target; this.body = body;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public Expr Target {
            get { return target; }
        }

        public Expr Test {
            get { return test; }
        }

        public Stmt Body {
            get { return body; }
        } 

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                if (test != null) test.Walk(w);
                if (target != null) target.Walk(w);
                body.Walk(w);
            }
            w.PostWalk(this);
        }
    }


    public class TryFinallyStmt : Stmt {
        private Location header;
        private readonly Stmt body;
        private readonly Stmt finallyStmt;
        private List<YieldTarget> yieldTargets = new List<YieldTarget>();

        public TryFinallyStmt(Stmt body, Stmt finally_) {
            this.body = body; this.finallyStmt = finally_;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public Stmt Body {
            get { return body; }
        }

        public Stmt FinallyStmt {
            get { return finallyStmt; }
        }

        public IList<YieldTarget> YieldTargets {
            get { return yieldTargets; }
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, header);
            Slot choiceVar = null;
            Slot returnVar = null;
            Label endOfTry = new Label();

            if (yieldTargets.Count > 0) {
                Label startOfBlock = cg.DefineLabel();
                choiceVar = cg.GetLocalTmp(typeof(int));
                returnVar = cg.GetLocalTmp(typeof(bool));
                cg.EmitInt(0);
                returnVar.EmitSet(cg);
                cg.EmitInt(-1);
                choiceVar.EmitSet(cg);
                cg.Emit(OpCodes.Br, startOfBlock);

                int index = 0;
                foreach (YieldTarget yt in yieldTargets) {
                    cg.MarkLabel(yt.topBranchTarget);
                    cg.EmitInt(index++);
                    choiceVar.EmitSet(cg);
                    cg.Emit(OpCodes.Br, startOfBlock);
                }

                cg.MarkLabel(startOfBlock);
            }

            cg.PushTryBlock();
            cg.BeginExceptionBlock();

            if (yieldTargets.Count > 0) {
                endOfTry = cg.DefineLabel();
                for (int index = 0; index < yieldTargets.Count; index ++) {
                    choiceVar.EmitGet(cg);
                    cg.EmitInt(index);
                    cg.Emit(OpCodes.Beq, endOfTry);
                }
            }

            body.Emit(cg);

            if (yieldTargets.Count > 0) {
                cg.MarkLabel(endOfTry);
            }

            cg.PopTargets();
            cg.PushFinallyBlock(returnVar);
            cg.BeginFinallyBlock();

            if (yieldTargets.Count > 0) {
                int index = 0;
                foreach (YieldTarget yt in yieldTargets) {
                    choiceVar.EmitGet(cg);
                    cg.EmitInt(index++);
                    cg.Emit(OpCodes.Beq, yt.tryBranchTarget);
                }
            }

            finallyStmt.Emit(cg);

            cg.EndExceptionBlock();
            cg.PopTargets();

            if (yieldTargets.Count > 0) {
                Label noReturn = cg.DefineLabel();
                returnVar.EmitGet(cg);
                cg.Emit(OpCodes.Brfalse_S, noReturn);
                cg.Emit(OpCodes.Ldc_I4_1);
                cg.EmitReturn();
                cg.MarkLabel(noReturn);
            }

            yieldTargets.Clear();
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                body.Walk(w);
                finallyStmt.Walk(w);
            }
            w.PostWalk(this);
        }

        public void AddYieldTarget(YieldTarget yt) {
            yieldTargets.Add(yt);
        }
    }

    public class ExprStmt : Stmt {
        private readonly Expr expr;

        public ExprStmt(Expr expr) { this.expr = expr; }

        public Expr Expression {
            get { return expr; }
        }

        public override object Execute(NameEnv env) {
            expr.Evaluate(env);
            return NextStmt;
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            if (cg.printExprStmts) {
                // emit & print expression.
                cg.EmitSystemState();
                expr.Emit(cg);

                Label noSet = cg.DefineLabel();
                // only set _ if the last expression is not None.
                cg.Emit(OpCodes.Dup);
                cg.Emit(OpCodes.Ldnull);
                cg.Emit(OpCodes.Beq, noSet);
                cg.Emit(OpCodes.Dup);
                cg.EmitSet(SymbolTable.Underscore);
                cg.MarkLabel(noSet);

                // finally emit the call to print the value.
                cg.EmitCall(typeof(Ops), "PrintNotNoneRepr", new Type[] { typeof(SystemState), typeof(object) });
            } else {
                // expression needs to be emitted incase it has side-effects.
                expr.Emit(cg);
                cg.Emit(OpCodes.Pop);
            }            
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                expr.Walk(w);
            }
            w.PostWalk(this);
        }

        public override string GetDocString() {
            ConstantExpr ce = expr as ConstantExpr;
            if (ce != null) {
                return ce.Value as string;
            }
            return null;
        }
    }

    public class AssignStmt : Stmt {
        // lhs.Length is 1 for simple assignments like "x = 1"
        // lhs.Lenght will be 3 for "x = y = z = 1"
        private readonly Expr[] lhs;
        private readonly Expr rhs;

        public AssignStmt(Expr[] lhs, Expr rhs) { 
            this.lhs = lhs;
            this.rhs = rhs; 
        }

        public IList<Expr> Left {
            get { return lhs; }
        }

        public Expr Right {
            get { return rhs; }
        } 
        
        public override object Execute(NameEnv env) {
            object v = rhs.Evaluate(env);
            foreach (Expr e in lhs) {
                e.Assign(v, env);
            }
            return NextStmt;
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            rhs.Emit(cg);
            for (int i = 0; i < lhs.Length; i++) {
                if (i < lhs.Length - 1) cg.Emit(OpCodes.Dup);
                lhs[i].EmitSet(cg);
            }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (Expr e in lhs) e.Walk(w);
                rhs.Walk(w);
            }
            w.PostWalk(this);
        }

    }

    public class AugAssignStmt : Stmt {
        private readonly BinaryOperator op;
        private readonly Expr lhs;
        private readonly Expr rhs;

        public AugAssignStmt(BinaryOperator op, Expr lhs, Expr rhs) {
            this.op = op; this.lhs = lhs; this.rhs = rhs;
        }

        public BinaryOperator Operator {
            get { return op; }
        }

        public Expr Left {
            get { return lhs; }
        }

        public Expr Right {
            get { return rhs; }
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);

            // if lhs is a complex expression (eg foo[x] or foo.bar)
            // then it's EmitSet needs to do the right thing.
            lhs.Emit(cg);  
            rhs.Emit(cg);

            op.EmitInPlace(cg);
            lhs.EmitSet(cg);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                lhs.Walk(w);
                rhs.Walk(w);
            }
            w.PostWalk(this);
        }

    }


    public class PrintStmt : Stmt {
        private readonly Expr dest;
        private readonly Expr[] exprs;
        private readonly bool trailingComma;

        public PrintStmt(Expr dest, Expr[] exprs, bool trailingComma) {
            this.dest = dest; this.exprs = exprs;
            this.trailingComma = trailingComma;
        }

        public Expr Destination {
            get { return dest; }
        }

        public IList<Expr> Expressions {
            get { return exprs; }
        }

        public bool TrailingComma {
            get { return trailingComma; }
        }

        public override object Execute(NameEnv env) {
            Console.Out.Write("print> ");
            foreach (Expr e in exprs) {
                object val = e.Evaluate(env);
                Ops.PrintComma(env.globals.SystemState, val);
            }
            if (!trailingComma) Ops.PrintNewline(env.globals.SystemState);

            return NextStmt;
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            string suffix = "";
            if (dest != null) suffix = "WithDest";
            if (exprs.Length == 0) {
                cg.EmitSystemState();
                if (dest != null) dest.Emit(cg);
                cg.EmitCall(typeof(Ops), "PrintNewline" + suffix);
            } else {
                Slot destSlot = null;
                if (dest != null) {
                    destSlot = cg.GetLocalTmp(typeof(object));
                    dest.Emit(cg);
                    destSlot.EmitSet(cg);
                }
                for (int i = 0; i < exprs.Length; i++) {
                    cg.EmitSystemState();
                    if (dest != null) {
                        Debug.Assert(destSlot != null);
                        destSlot.EmitGet(cg);
                    }
                    exprs[i].Emit(cg);
                    if (i < exprs.Length - 1 || trailingComma) cg.EmitCall(typeof(Ops), "PrintComma" + suffix);
                    else cg.EmitCall(typeof(Ops), "Print" + suffix);
                }
                if (destSlot != null) {
                    cg.FreeLocalTmp(destSlot);
                }
            }
        }
        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                if (dest != null) dest.Walk(w);
                foreach (Expr e in exprs) e.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class DottedName : Node {
        private readonly SymbolId[] names;

        public DottedName(SymbolId[] names) { this.names = names; }

        public IList<SymbolId> Names {
            get { return names; }
        }

        public string MakeString() {
            StringBuilder ret = new StringBuilder(names[0].GetString());
            for (int i = 1; i < names.Length; i++) {
                ret.Append('.');
                ret.Append(names[i].GetString());
            }
            return ret.ToString();
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                ;
            }
            w.PostWalk(this);
        }
    }


    public class ImportStmt : Stmt {
        private readonly DottedName[] names;
        private readonly SymbolId[] asNames;

        public ImportStmt(DottedName[] names, SymbolId[] asNames) {
            this.names = names;
            this.asNames = asNames;
        }

        public IList<DottedName> Names {
            get { return names; }
        }

        public IList<SymbolId> AsNames {
            get { return asNames; }
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);

            for (int i = 0; i < names.Length; i++) {
                DottedName name = names[i];
                cg.EmitModuleInstance();
                cg.EmitString(name.MakeString());
                if (asNames[i] == SymbolTable.Empty) {
                    cg.EmitCall(typeof(Ops), "Import");
                    cg.EmitSet(name.Names[0]);
                } else {
                    cg.EmitString(asNames[i].GetString());
                    cg.EmitCall(typeof(Ops), "ImportAs");
                    cg.EmitSet(asNames[i]);
                }
            }
        }

        public override void Walk(IAstWalker w) {
            w.Walk(this);
            w.PostWalk(this);
        }
    }

    public class FromImportStmt : Stmt {
        private static readonly SymbolId[] star = new SymbolId[1];
        private readonly DottedName root;
        private readonly SymbolId[] names;
        private readonly SymbolId[] asNames;
        private readonly bool fromFuture;

        public FromImportStmt(DottedName root, SymbolId[] names, SymbolId[] asNames)
            : this(root, names, asNames, false) {
        }

        public static SymbolId[] Star {
            get { return FromImportStmt.star; }
        }

        public DottedName Root {
            get { return root; }
        }

        public IList<SymbolId> Names {
            get { return names; }
        }

        public IList<SymbolId> AsNames {
            get { return asNames; }
        }

        public FromImportStmt(DottedName root, SymbolId[] names, SymbolId[] asNames, bool fromFuture) {
            this.root = root;
            this.names = names;
            this.asNames = asNames;
            this.fromFuture = fromFuture;
        }

        public override object Execute(NameEnv env) {
            Ops.ImportFrom(env.globals, root.MakeString(), SymbolTable.IdsToStrings(names));

            return NextStmt;
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);

            cg.EmitModuleInstance();
            cg.EmitString(root.MakeString());
            if (names == star) {
                cg.EmitCall(typeof(Ops), "ImportStar");
            } else {
                Slot fromObj = cg.GetLocalTmp(typeof(object));
                cg.EmitStringArray(SymbolTable.IdsToStrings(names));

                if (asNames != null) {
                    cg.EmitStringArray(SymbolTable.IdsToStrings(asNames));
                    cg.EmitCall(typeof(Ops), "ImportFromAs");
                } else {
                    cg.EmitCall(typeof(Ops), "ImportFrom");
                }

                fromObj.EmitSet(cg);

                for (int i = 0; i < names.Length; i++) {
                    cg.EmitCallerContext();
                    fromObj.EmitGet(cg);
                    cg.EmitString(names[i].GetString());
                    cg.EmitCall(typeof(Ops), "ImportOneFrom");

                    SymbolId asName;
                    if (i < asNames.Length && asNames[i] != SymbolTable.Empty)
                        asName = asNames[i];
                    else
                        asName = names[i];

                    cg.EmitSet(asName);
                }
            }
        }

        public override void Walk(IAstWalker w) {
            w.Walk(this);
            w.PostWalk(this);
        }

        internal bool IsFromFuture {
            get {
                return fromFuture;
            }
        }
    }

    public class GlobalStmt : Stmt {
        private readonly SymbolId[] names;

        public GlobalStmt(SymbolId[] names) {
            this.names = names;
        }

        public IList<SymbolId> Names {
            get { return names; }
        }

        public override object Execute(NameEnv env) {
            return NextStmt;
        }

        internal override void Emit(CodeGen cg) {
        }

        public override void Walk(IAstWalker w) {
            w.Walk(this);
            w.PostWalk(this);
        }
    }

    public class DelStmt : Stmt {
        private readonly Expr[] exprs;

        public DelStmt(Expr[] exprs) {
            this.exprs = exprs;
        }

        public IList<Expr> Expressions {
            get { return exprs; }
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            foreach (Expr expr in exprs) {
                expr.EmitDel(cg);
            }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (Expr e in exprs) e.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class RaiseStmt : Stmt {
        private readonly Expr type, value, traceback;

        public RaiseStmt(Expr type, Expr _value, Expr traceback) {
            this.type = type; this.value = _value; this.traceback = traceback;
        }

        public Expr Traceback {
            get { return traceback; }
        }

        public Expr Value {
            get { return this.value; }
        }


        public Expr ExceptionType {
            get { return type; }
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            if (type == null && value == null && traceback == null) {
                cg.EmitCall(typeof(Ops), "Raise", Type.EmptyTypes);
                return;
            } else {
                cg.EmitExprOrNone(type);
                cg.EmitExprOrNone(value);
                cg.EmitExprOrNone(traceback);
                cg.EmitCall(typeof(Ops), "Raise", new Type[] { typeof(object), typeof(object), typeof(object) });
            }
        }
        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                if (type != null) type.Walk(w);
                if (value != null) value.Walk(w);
                if (traceback != null) traceback.Walk(w);
            }
            w.PostWalk(this);
        }

    }

    public class AssertStmt : Stmt {
        private readonly Expr test, message;

        public AssertStmt(Expr test, Expr message) {
            this.test = test;
            this.message = message;
        }

        public Expr Message {
            get { return message; }
        }

        public Expr Test {
            get { return test; }
        }

        internal override void Emit(CodeGen cg) {
            if (IronPython.Hosting.PythonEngine.options.DebugMode) {
                cg.EmitPosition(Start, End);
                cg.EmitTestTrue(test);
                Label endLabel = cg.DefineLabel();
                cg.Emit(OpCodes.Brtrue, endLabel);
                cg.EmitExprOrNone(message);
                cg.EmitCastFromObject(typeof(string));
                cg.EmitCall(typeof(Ops), "AssertionError", new Type[] { typeof(string) });
                cg.Emit(OpCodes.Throw);
                cg.MarkLabel(endLabel);
            }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                test.Walk(w);
                if (message != null) message.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class ExecStmt : Stmt {
        private readonly Expr code, locals, globals;

        public ExecStmt(Expr code, Expr locals, Expr globals) {
            this.code = code;
            this.locals = locals;
            this.globals = globals;
        }

        public Expr Globals {
            get { return globals; }
        }

        public Expr Locals {
            get { return locals; }
        }

        public Expr Code {
            get { return code; }
        }

        public bool NeedsLocalsDictionary() {
            return globals == null && locals == null;
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            cg.EmitCallerContext();
            code.Emit(cg);
            if (locals == null && globals == null) {
                // pass in the current module's globals.
                cg.EmitCall(typeof(Ops), "Exec", new Type[] {
                        typeof(ICallerContext),
                        typeof(object),                        
                    });
            } else {
                // We must have globals now (locals is last and may be absent)
                Debug.Assert(globals != null);
                globals.Emit(cg);
                cg.EmitCastFromObject(typeof(IAttributesDictionary));
                if (locals != null) {
                    locals.Emit(cg);        // emit locals
                } else {
                    cg.Emit(OpCodes.Dup);   // use globals
                }
                cg.EmitCall(typeof(Ops), "Exec", new Type[] {
                        typeof(ICallerContext),
                        typeof(object),
                        typeof(IAttributesDictionary),
                        typeof(object)
                    });
            }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                code.Walk(w);
                if (locals != null) locals.Walk(w);
                if (globals != null) globals.Walk(w);
            }
            w.PostWalk(this);
        }

    }

    public class ReturnStmt : Stmt {
        private readonly Expr expr;

        public ReturnStmt(Expr expr) {
            this.expr = expr;
        }

        public Expr Expression {
            get { return expr; }
        }

        public override object Execute(NameEnv env) {
            return expr.Evaluate(env);
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            cg.EmitReturn(expr);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                if (expr != null) expr.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class YieldStmt : Stmt {
        private readonly Expr expr;
        private readonly int index;
        private Label label;

        public YieldStmt(Expr expr, int index) {
            this.expr = expr;
            this.index = index;
        }

        public Expr Expression {
            get { return expr; }
        }

        public int Index {
            get { return index; }
        }

        public Label Label {
            get { return label; }
            set { label = value; }
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            cg.EmitYield(expr, index, label);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                expr.Walk(w);
            }
            w.PostWalk(this);
        }
    }


    public class PassStmt : Stmt {
        public PassStmt() { }

        public override object Execute(NameEnv env) {
            return NextStmt;
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                ;
            }
            w.PostWalk(this);
        }

    }
    public class BreakStmt : Stmt {
        public BreakStmt() { }

        internal override void Emit(CodeGen cg) {
            if (!cg.InLoop()) {
                cg.Context.AddError("'break' not properly in loop", this);
                return;
            }
            cg.EmitPosition(Start, End);
            cg.EmitBreak();
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                ;
            }
            w.PostWalk(this);
        }

    }
    public class ContinueStmt : Stmt {
        public ContinueStmt() { }

        internal override void Emit(CodeGen cg) {
            if (!cg.InLoop()) {
                cg.Context.AddError("'continue' not properly in loop", this);
                return;
            }
            cg.EmitPosition(Start, End);
            cg.EmitContinue();
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                ;
            }
            w.PostWalk(this);
        }
    }
}
