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
using IronPython.CodeDom;


namespace IronPython.Compiler {
    public abstract class Stmt : Node {
        public static readonly object NextStmt = new object();

        public virtual object Execute(NameEnv env) {
            throw new NotImplementedException("execute: " + this);
        }

        public virtual void Emit(CodeGen cg) {
            throw new NotImplementedException("Emit: " + this);
        }

        public virtual string GetDocString() {
            return null;
        }
    }

    public class SuiteStmt : Stmt {
        public readonly Stmt[] stmts;
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
       
        public override void Emit(CodeGen cg) {
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
                if (es.expr is ConstantExpr) {
                    object val = ((ConstantExpr)es.expr).value;
                    if (val is string && !Options.StripDocStrings) return (string)val;
                }
            }
            return null;
        }
    }

    public class IfStmt : Stmt {
        public readonly IfStmtTest[] tests;
        public readonly Stmt elseStmt;
        public IfStmt(IfStmtTest[] tests, Stmt else_) {
            this.tests = tests; this.elseStmt = else_;
        }

        public override object Execute(NameEnv env) {
            foreach (IfStmtTest t in tests) {
                object val = t.test.Evaluate(env);
                if (Ops.IsTrue(val)) {
                    return t.body.Execute(env);
                }
            }
            if (elseStmt != null) {
                return elseStmt.Execute(env);
            }
            return NextStmt;
        }

        public override void Emit(CodeGen cg) {
            Label eoi = cg.DefineLabel();
            foreach (IfStmtTest t in tests) {
                Label next = cg.DefineLabel();
                cg.EmitPosition(t.start, t.header);
                cg.EmitTestTrue(t.test);
                cg.Emit(OpCodes.Brfalse, next);
                t.body.Emit(cg);
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
        public Location header;
        public readonly Expr test;
        public Stmt body;
        public IfStmtTest(Expr test, Stmt body) {
            this.test = test; this.body = body;
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
        public Location header;
        public readonly Expr test;
        public readonly Stmt body;
        public readonly Stmt elseStmt;
        public WhileStmt(Expr test, Stmt body, Stmt else_) {
            this.test = test; this.body = body; this.elseStmt = else_;
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

        public override void Emit(CodeGen cg) {
            Label eol = cg.DefineLabel();
            Label breakTarget = cg.DefineLabel();
            Label continueTarget = cg.DefineLabel();

            cg.MarkLabel(continueTarget);

            cg.EmitPosition(start, header);
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
            this.start = start;
            this.header = header;
            this.end = end;
        }
    }

    public class ForStmt : Stmt {
        public Location header;
        public readonly Expr lhs;
        public Expr list;
        public Stmt body;
        public readonly Stmt elseStmt;
        public ForStmt(Expr lhs, Expr list, Stmt body, Stmt else_) {
            this.lhs = lhs; this.list = list;
            this.body = body; this.elseStmt = else_;
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

        public override void Emit(CodeGen cg) {
            Label eol = cg.DefineLabel();
            Label breakTarget = cg.DefineLabel();
            Label continueTarget = cg.DefineLabel();

            cg.EmitPosition(start, header);

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

            if (cg.IsGenerator()) {
                //!!! need to free my temp
            } else {
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
        public Location header;
        public readonly Stmt body;
        public readonly TryStmtHandler[] handlers;
        public readonly Stmt elseStmt;

        public bool yieldInExcept = false;
        public ArrayList yieldTargets = new ArrayList();
        public TryStmt(Stmt body, TryStmtHandler[] handlers, Stmt else_) {
            this.body = body; this.handlers = handlers; this.elseStmt = else_;
        }

        public void AddYieldTarget(YieldTarget t) {
            yieldTargets.Add(t);
        }

        //!!! need to evaluate break/continue through a try block
        public override void Emit(CodeGen cg) {
            Slot choiceVar = null;
            cg.EmitPosition(start, header);

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
                cg.EmitPosition(handler.start, handler.header);
                Label next = cg.DefineLabel();
                if (handler.test != null) {
                    pyExc.EmitGet(cg);
                    handler.test.Emit(cg);
                    cg.EmitCall(typeof(Ops), "CheckException");
                    if (handler.target != null) {
                        tmpExc.EmitSet(cg);
                        tmpExc.EmitGet(cg);
                    }
                    cg.EmitPythonNone();
                    cg.Emit(OpCodes.Ceq);
                    cg.Emit(OpCodes.Brtrue, next);
                }

                if (handler.target != null) {
                    tmpExc.EmitGet(cg);
                    handler.target.EmitSet(cg);
                }

                cg.PushExceptionBlock(Targets.TargetBlockType.Catch, null);

                handler.body.Emit(cg);
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
        public Location header;
        public readonly Expr test, target;
        public readonly Stmt body;

        public TryStmtHandler(Expr test, Expr target, Stmt body) {
            this.test = test; this.target = target; this.body = body;
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
        public Location header;
        public readonly Stmt body;
        public readonly Stmt finallyStmt;

        public ArrayList yieldTargets = new ArrayList();

        public TryFinallyStmt(Stmt body, Stmt finally_) {
            this.body = body; this.finallyStmt = finally_;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, header);
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
        public readonly Expr expr;
        public ExprStmt(Expr expr) { this.expr = expr; }

        public override object Execute(NameEnv env) {
            expr.Evaluate(env);
            //!!! print it if in the right env
            return NextStmt;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, end);
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
                cg.EmitSet(Name.Make("_"));
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
                return ce.value as string;
            }
            return null;
        }
    }

    public class AssignStmt : Stmt {
        // lhs.Length is 1 for simple assignments like "x = 1"
        // lhs.Lenght will be 3 for "x = y = z = 1"
        public readonly Expr[] lhs;

        public readonly Expr rhs;

        public AssignStmt(Expr[] lhs, Expr rhs) { 
            this.lhs = lhs;
            this.rhs = rhs; 
        }

        public override object Execute(NameEnv env) {
            object v = rhs.Evaluate(env);
            foreach (Expr e in lhs) {
                e.Assign(v, env);
            }
            return NextStmt;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, end);
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
        public readonly BinaryOperator op;
        public readonly Expr lhs;
        public readonly Expr rhs;
        public AugAssignStmt(BinaryOperator op, Expr lhs, Expr rhs) {
            this.op = op; this.lhs = lhs; this.rhs = rhs;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, end);

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
        public readonly Expr dest;
        public readonly Expr[] exprs;
        public readonly bool trailingComma;
        public PrintStmt(Expr dest, Expr[] exprs, bool trailingComma) {
            this.dest = dest; this.exprs = exprs;
            this.trailingComma = trailingComma;
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

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, end);
            string suffix = "";
            if (dest != null) suffix = "WithDest";
            if (exprs.Length == 0) {
                cg.EmitSystemState();
                if (dest != null) dest.Emit(cg);
                cg.EmitCall(typeof(Ops), "PrintNewline" + suffix);
            }

            for (int i = 0; i < exprs.Length; i++) {
                cg.EmitSystemState();
                if (dest != null) dest.Emit(cg); //!!! need to put in a temp
                exprs[i].Emit(cg);
                if (i < exprs.Length - 1 || trailingComma) cg.EmitCall(typeof(Ops), "PrintComma" + suffix);
                else cg.EmitCall(typeof(Ops), "Print" + suffix);
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
        public readonly Name[] names;
        public DottedName(Name[] names) { this.names = names; }

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
        public readonly DottedName[] names;
        public readonly Name[] asNames;
        public ImportStmt(DottedName[] names, Name[] asNames) {
            this.names = names;
            this.asNames = asNames;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, end);

            for (int i = 0; i < names.Length; i++) {
                DottedName name = names[i];
                cg.EmitModuleInstance();
                cg.EmitString(name.MakeString());
                if (asNames[i] == null) {
                    cg.EmitCall(typeof(Ops), "Import");
                    cg.EmitSet(name.names[0]);
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
        public static readonly Name[] Star = new Name[1];

        public readonly DottedName root;
        public readonly Name[] names;
        public readonly Name[] asNames;
        private readonly bool fromFuture;

        public FromImportStmt(DottedName root, Name[] names, Name[] asNames)
            : this(root, names, asNames, false) {
        }

        public FromImportStmt(DottedName root, Name[] names, Name[] asNames, bool fromFuture) {
            this.root = root;
            this.names = names;
            this.asNames = asNames;
            this.fromFuture = fromFuture;
        }

        public override object Execute(NameEnv env) {
            Ops.ImportFrom(env.globals, root.MakeString(), Name.ToStrings(names));

            return NextStmt;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, end);

            cg.EmitModuleInstance();
            cg.EmitString(root.MakeString());
            if (names == Star) {
                cg.EmitCall(typeof(Ops), "ImportStar"); //!!! this is tricky
            } else {
                Slot fromObj = cg.GetLocalTmp(typeof(object));
                cg.EmitStringArray(Name.ToStrings(names));

                if (asNames != null) {
                    cg.EmitStringArray(Name.ToStrings(asNames));
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

                    Name asName;
                    if (i < asNames.Length && asNames[i] != null)
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
        public readonly Name[] names;
        public GlobalStmt(Name[] names) {
            this.names = names;
        }

        public override object Execute(NameEnv env) {
            return NextStmt;
        }

        public override void Emit(CodeGen cg) {
        }

        public override void Walk(IAstWalker w) {
            w.Walk(this);
            w.PostWalk(this);
        }
    }

    public class DelStmt : Stmt {
        public readonly Expr[] exprs;
        public DelStmt(Expr[] exprs) {
            this.exprs = exprs;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, end);
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
        public readonly Expr type, value, traceback;
        public RaiseStmt(Expr type, Expr _value, Expr traceback) {
            this.type = type; this.value = _value; this.traceback = traceback;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, end);
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
        public readonly Expr test, message;
        public AssertStmt(Expr test, Expr message) {
            this.test = test;
            this.message = message;
        }

        public override void Emit(CodeGen cg) {
            if (IronPython.Hosting.PythonEngine.options.DebugMode) {
                cg.EmitPosition(start, end);
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
        public readonly Expr code, locals, globals;
        public ExecStmt(Expr code, Expr locals, Expr globals) {
            this.code = code;
            this.locals = locals;
            this.globals = globals;
        }

        public bool NeedsLocalsDictionary() {
            return globals == null && locals == null;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, end);
            cg.EmitCallerContext();
            code.Emit(cg);
            if (locals == null) {
                if (globals == null) {
                    // pass in the current module's globals.
                    cg.EmitCall(typeof(Ops), "Exec", new Type[] {
                        typeof(ICallerContext),
                        typeof(object),                        
                    });
                } else {
                    // user provided only globals, this gets used as both dictionarys.
                    globals.Emit(cg);
                    cg.EmitCastFromObject(typeof(IDictionary<object, object>));
                    globals.Emit(cg);
                    cg.EmitCastFromObject(typeof(IDictionary<object, object>));
                    cg.EmitCall(typeof(Ops), "Exec", new Type[] {
                        typeof(ICallerContext),
                        typeof(object),
                        typeof(System.Collections.Generic.IDictionary<object, object>),
                        typeof(System.Collections.Generic.IDictionary<object, object>)
                    });
                }
            } else {
                // locals is last so both are defined
                locals.Emit(cg);
                cg.EmitCastFromObject(typeof(System.Collections.Generic.IDictionary<object, object>));
                globals.Emit(cg);
                cg.EmitCastFromObject(typeof(System.Collections.Generic.IDictionary<object, object>));
                cg.EmitCall(typeof(Ops), "Exec", new Type[] {
                        typeof(ICallerContext),
                        typeof(object),
                        typeof(System.Collections.Generic.IDictionary<object, object>),
                        typeof(System.Collections.Generic.IDictionary<object, object>)
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
        public readonly Expr expr;
        public ReturnStmt(Expr expr) {
            this.expr = expr;
        }

        public override object Execute(NameEnv env) {
            return expr.Evaluate(env);
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, end);
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
        public readonly Expr expr;
        public readonly int index;
        public Label label;
        public YieldStmt(Expr expr, int index) {
            this.expr = expr;
            this.index = index;
        }


        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, end);
            cg.EmitYield(expr, index, label);
        }
/*
        public override CodeObject Generate() {
            //!!! expr, index
            return new CodeSnippetStatement("yield ");
        }*/

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

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, end);
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

        public override void Emit(CodeGen cg) {
            if (!cg.InLoop()) {
                cg.Context.AddError("'break' not properly in loop", this);
                return;
            }
            cg.EmitPosition(start, end);
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

        public override void Emit(CodeGen cg) {
            if (!cg.InLoop()) {
                cg.Context.AddError("'continue' not properly in loop", this);
                return;
            }
            cg.EmitPosition(start, end);
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
