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

namespace IronPython.Compiler.Ast {
    public abstract class Statement : Node {
        public static readonly object NextStatement = new object();

        public virtual object Execute(NameEnvironment environment) {
            throw new NotImplementedException("execute: " + this);
        }

        internal abstract void Emit(CodeGen cg);

        public virtual string Documentation {
            get {
                return null;
            }
        }
    }

    public class SuiteStatement : Statement {
        private readonly Statement[] stmts;

        public IList<Statement> Statements {
            get { return stmts; }
        }

        public SuiteStatement(Statement[] statements) {
            this.stmts = statements;
        }

        public override object Execute(NameEnvironment environment) {
            object ret = Statement.NextStatement;
            foreach (Statement stmt in stmts) {
                ret = stmt.Execute(environment);
                if (ret != Statement.NextStatement) break;
            }
            return ret;
        }

        internal override void Emit(CodeGen cg) {
            // Should emit nop for the colon?
            foreach (Statement stmt in stmts) {
                stmt.Emit(cg);
            }
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                foreach (Statement stmt in stmts) stmt.Walk(walker);
            }
            walker.PostWalk(this);
        }

        public override string Documentation {
            get {
                if (stmts.Length > 0 && stmts[0] is ExpressionStatement) {
                    ExpressionStatement es = (ExpressionStatement)stmts[0];
                    if (es.Expression is ConstantExpression) {
                        object val = ((ConstantExpression)es.Expression).Value;
                        if (val is string && !Options.StripDocStrings) return (string)val;
                    }
                }
                return null;
            }
        }
    }

    public class IfStatement : Statement {
        private readonly IfStatementTest[] tests;
        private readonly Statement elseStmt;

        public IfStatement(IfStatementTest[] tests, Statement else_) {
            this.tests = tests; this.elseStmt = else_;
        }

        public IList<IfStatementTest> Tests {
            get { return tests; }
        }

        public Statement ElseStatement {
            get { return elseStmt; }
        }

        public override object Execute(NameEnvironment environment) {
            foreach (IfStatementTest t in tests) {
                object val = t.Test.Evaluate(environment);
                if (Ops.IsTrue(val)) {
                    return t.Body.Execute(environment);
                }
            }
            if (elseStmt != null) {
                return elseStmt.Execute(environment);
            }
            return NextStatement;
        }

        internal override void Emit(CodeGen cg) {
            Label eoi = cg.DefineLabel();
            foreach (IfStatementTest t in tests) {
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                foreach (IfStatementTest t in tests) t.Walk(walker);
                if (elseStmt != null) elseStmt.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class IfStatementTest : Node {
        private Location header;
        private readonly Expression test;
        private Statement body;

        public IfStatementTest(Expression test, Statement body) {
            this.test = test; this.body = body;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public Expression Test {
            get { return test; }
        }

        public Statement Body {
            get { return body; }
            set { body = value; }
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                test.Walk(walker);
                body.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class WhileStatement : Statement {
        private Location header;
        private readonly Expression test;
        private readonly Statement body;
        private readonly Statement elseStmt;

        public WhileStatement(Expression test, Statement body, Statement else_) {
            this.test = test; this.body = body; this.elseStmt = else_;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public Expression Test {
            get { return test; }
        }

        public Statement Body {
            get { return body; }
        }

        public Statement ElseStatement {
            get { return elseStmt; }
        }

        public override object Execute(NameEnvironment environment) {
            object ret = NextStatement;
            while (Ops.IsTrue(test.Evaluate(environment))) {
                ret = body.Execute(environment);
                if (ret != NextStatement) break;
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                test.Walk(walker);
                body.Walk(walker);
                if (elseStmt != null) elseStmt.Walk(walker);
            }
            walker.PostWalk(this);
        }
        public void SetLoc(Location start, Location header, Location end) {
            this.Start = start;
            this.header = header;
            this.End = end;
        }
    }

    public class ForStatement : Statement {
        private Location header;
        private readonly Expression lhs;
        private Expression list;
        private Statement body;
        private readonly Statement elseStmt;

        public ForStatement(Expression lhs, Expression list, Statement body, Statement else_) {
            this.lhs = lhs; this.list = list;
            this.body = body; this.elseStmt = else_;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public Expression Left {
            get { return lhs; }
        }

        public Expression List {
            get { return list; }
            set { list = value; }
        }

        public Statement Body {
            get { return body; }
            set { body = value; }
        }

        public Statement ElseStatement {
            get { return elseStmt; }
        }

        public override object Execute(NameEnvironment environment) {
            object ret = Statement.NextStatement;

            IEnumerator i = Ops.GetEnumerator(list.Evaluate(environment));

            while (i.MoveNext()) {
                lhs.Assign(i.Current, environment);
                ret = body.Execute(environment);
                if (ret != NextStatement) return ret;
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
            cg.EmitCall(typeof(Ops), "GetEnumeratorForIteration");

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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                lhs.Walk(walker);
                list.Walk(walker);
                body.Walk(walker);
                if (elseStmt != null) elseStmt.Walk(walker);
            }
            walker.PostWalk(this);
        }

        // For uses one local slot for the iter variable
        internal static int LocalSlots {
            get {
                return 1;
            }
        }
    }

    public class WithStatement : Statement {
        private Location header;
        private Expression contextManager;
        private Expression var;
        private Statement body;
        private List<YieldTarget> yieldTargets;// = new List<YieldTarget>();

        public WithStatement(Expression contextManager, Expression var, Statement body) {
            this.contextManager = contextManager;
            this.var = var;
            this.body = body;
        }


        public Expression Variable {
            get { return var; }
        }

        public Expression ContextManager {
            get { return contextManager; }
        }

        public Statement Body {
            get { return body; }
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public IList<YieldTarget> YieldTargets {
            get { return yieldTargets; }
        }

        internal void AddYieldTarget(YieldTarget target) {
            if (yieldTargets == null)
                yieldTargets = new List<YieldTarget>();
            yieldTargets.Add(target);
        }

        // With uses 3 local slots 
        internal static int LocalSlots {
            get {
                return 3;
            }
        }


        // ***WITH STATEMENT CODE GENERATION ALGORITHM*** 
        //
        //GRAMMAR :=
        //with EXPR as VAR:
        //    BLOCK
        //
        //CODE GEN :=
        //
        //mgr = (EXPR)
        //exit = mgr.__exit__  # Not calling it yet
        //value = mgr.__enter__()
        //exc = True
        //isTryYielded = False
        //try:
        //
        //   VAR = value  # Only if "as VAR" is present
        //   BLOCK 
        //   // if yield happens in the Block, 
        //   // then isTryYielded is set to True by Yield's Code Gen
        //except:
        //   # The exceptional case is handled here
        //   exc = False
        //   if not exit(*sys.exc_info()):
        //        raise
        //   # The exception is consumed if exit() returns true
        //finally:
        //    # The normal and non-local-goto cases are handled here
        //    if  isTryYielded = False && exc == True :
        //        exit(None, None, None)



        internal override void Emit(CodeGen cg) {

            Slot exc = null;
            Slot isTryYielded = null;
            Slot exit = null;

            if (cg.IsGenerator()) {
                exc = cg.Names.GetTempSlot("with", typeof(object));
                isTryYielded = cg.Names.GetTempSlot("with", typeof(object));
                exit = cg.Names.GetTempSlot("with", typeof(object));
            } else {
                exc = cg.GetLocalTmp(typeof(object));
                isTryYielded = cg.GetLocalTmp(typeof(object));
                exit = cg.GetLocalTmp(typeof(object));
            }

            // mgr = (EXPR)
            Slot mgr = cg.GetLocalTmp(typeof(object));
            contextManager.Emit(cg);
            mgr.EmitSet(cg);

            // exit = mgr.__exit__ # not calling it yet
            cg.EmitCallerContext();
            mgr.EmitGet(cg);
            cg.EmitSymbolId("__exit__");
            cg.EmitCall(typeof(Ops), "GetAttr");
            exit.EmitSet(cg);

            mgr.EmitGet(cg);
            cg.FreeLocalTmp(mgr);
            cg.EmitSymbolId("__enter__");
            cg.EmitObjectArray(new Expression[0]);
            cg.EmitCall(typeof(Ops), "Invoke", new Type[] { typeof(object), typeof(SymbolId), typeof(object[]) });
            Slot value = cg.GetLocalTmp(typeof(object));
            value.EmitSet(cg);


            // exc = True
            cg.EmitConstantBoxed(true);
            exc.EmitSet(cg);

            Slot choiceVar = null;

            if (yieldTargets != null && yieldTargets.Count > 0) {
                Label startOfBlock = cg.DefineLabel();
                choiceVar = cg.GetLocalTmp(typeof(int));
                cg.EmitInt(-1);
                choiceVar.EmitSet(cg);
                cg.Emit(OpCodes.Br, startOfBlock);

                int index = 0;
                foreach (YieldTarget yt in yieldTargets) {
                    cg.MarkLabel(yt.TopBranchTarget);
                    cg.EmitInt(index++);
                    choiceVar.EmitSet(cg);
                    cg.Emit(OpCodes.Br, startOfBlock);
                }

                cg.MarkLabel(startOfBlock);
            }

            cg.EmitConstantBoxed(false);
            isTryYielded.EmitSet(cg);

            Label beforeFinally = cg.DefineLabel();

            cg.PushWithTryBlock(isTryYielded);
            cg.BeginExceptionBlock();

            if (yieldTargets != null && yieldTargets.Count > 0) {
                int index = 0;
                foreach (YieldTarget yt in yieldTargets) {
                    choiceVar.EmitGet(cg);
                    cg.EmitInt(index);
                    cg.Emit(OpCodes.Beq, yt.YieldContinuationTarget);
                    index++;
                }
                cg.FreeLocalTmp(choiceVar);
            }

            if (var != null) {
                value.EmitGet(cg);
                var.EmitSet(cg);
            }

            body.Emit(cg);

            EmitWithCatchBlock(cg, exc, exit);
            cg.PopTargets();
            EmitWithFinallyBlock(cg, exc, exit, isTryYielded);
            cg.EndExceptionBlock();
            if (yieldTargets != null)
                yieldTargets.Clear();

        }

        private void EmitWithCatchBlock(CodeGen cg, Slot exc, Slot exit) {
            cg.BeginCatchBlock(typeof(Exception));
            // Extract state from the carrier exception
            cg.EmitCallerContext();
            cg.EmitCall(typeof(Ops), "ExtractException",
                new Type[] { typeof(Exception), typeof(ICallerContext) });
            cg.Emit(OpCodes.Pop);

            // except body 
            cg.PushExceptionBlock(Targets.TargetBlockType.Catch, null, null);
            cg.EmitConstantBoxed(false);
            exc.EmitSet(cg);

            cg.EmitCallerContext();
            exit.EmitGet(cg);
            cg.EmitObjectArray(new Expression[0]);
            cg.EmitCallerContext();
            cg.EmitCall(typeof(Ops), "ExtractSysExcInfo");
            cg.EmitCall(typeof(Ops), "CallWithArgsTupleAndContext", new Type[] { typeof(ICallerContext), typeof(object), typeof(object[]), typeof(object) });

            Label afterRaise = cg.DefineLabel();

            cg.EmitTestTrue();
            cg.Emit(OpCodes.Brtrue, afterRaise);
            cg.EmitCall(typeof(Ops), "Raise", new Type[0]); //, new Type[] { typeof(object), typeof(SymbolId) });
            cg.MarkLabel(afterRaise);
            cg.EmitCallerContext();
            cg.EmitCall(typeof(Ops), "ClearException", new Type[] { typeof(ICallerContext) });
            cg.PopTargets();

        }

        private void EmitWithFinallyBlock(CodeGen cg, Slot exc, Slot exit, Slot isTryYielded) {
            // we are certain that Finally will never yield
            cg.PushFinallyBlock(null, null);
            cg.BeginFinallyBlock();

            //finally body
            Label endOfFinally = cg.DefineLabel();

            // isTryYielded == True ?
            isTryYielded.EmitGet(cg);
            cg.EmitTestTrue();
            cg.Emit(OpCodes.Brtrue, endOfFinally);

            // exc == False ?
            exc.EmitGet(cg);
            cg.EmitTestTrue();
            cg.Emit(OpCodes.Brfalse, endOfFinally);


            //exit(None, None, None)
            cg.EmitCallerContext();
            exit.EmitGet(cg);
            cg.Emit(OpCodes.Ldnull);
            cg.Emit(OpCodes.Ldnull);
            cg.Emit(OpCodes.Ldnull);
            cg.EmitCall(typeof(Ops), "CallWithContext", new Type[] { typeof(ICallerContext), typeof(object), typeof(object), typeof(object), typeof(object) });
            cg.Emit(OpCodes.Pop);

            cg.MarkLabel(endOfFinally);
            cg.PopTargets();

            // finally end

        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                contextManager.Walk(walker);
                if (var != null) var.Walk(walker);
                body.Walk(walker);
            }
            walker.PostWalk(this);
        }

    }

    public class TryStatement : Statement {
        class ControlFlowFinder : AstWalker {
            bool found;

            bool foundLoopControl;
            int loopCount = 0;

            public static bool FindControlFlow(Statement statement, out bool foundLoopControl) {
                // No return in null statement
                if (statement == null) {
                    foundLoopControl = false;
                    return false;
                }

                // find it now.
                ControlFlowFinder rf = new ControlFlowFinder();
                statement.Walk(rf);
                foundLoopControl = rf.foundLoopControl;
                return rf.found;
            }
            public override bool Walk(ReturnStatement node) {
                found = true;
                return true;
            }
            public override bool Walk(BreakStatement node) {
                if (loopCount == 0) {
                    found = true;
                    foundLoopControl = true;
                }
                return true;
            }
            public override bool Walk(ContinueStatement node) {
                if (loopCount == 0) {
                    found = true;
                    foundLoopControl = true;
                }
                return true;
            }

            public override bool Walk(ForStatement node) {
                loopCount++;
                return true;
            }

            public override void PostWalk(ForStatement node) {
                loopCount--;
            }

            public override bool Walk(WhileStatement node) {
                loopCount++;
                return true;
            }

            public override void PostWalk(WhileStatement node) {
                loopCount--;
            }
        }

        private Location header;
        private readonly Statement body;
        private readonly TryStatementHandler[] handlers;
        private readonly Statement elseStmt;
        private readonly Statement finallyStmt;
        private List<YieldTarget> tryYieldTargets = null;
        private List<YieldTarget> catchYieldTargets = null;
        private List<YieldTarget> finallyYieldTargets = null;
        private List<YieldTarget> elseYieldTargets = null;

        public TryStatement(Statement body, TryStatementHandler[] handlers, Statement elseSuite, Statement finallySuite) {
            this.body = body;
            this.handlers = handlers;
            this.elseStmt = elseSuite;
            this.finallyStmt = finallySuite;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public Statement Body {
            get { return body; }
        }

        public IList<TryStatementHandler> Handlers {
            get { return handlers; }
        }

        public Statement ElseStatement {
            get { return elseStmt; }
        }

        public Statement FinallyStatement {
            get { return finallyStmt; }
        }

        internal IList<YieldTarget> TryYieldTargets {
            get { return tryYieldTargets; }
        }

        internal void AddTryYieldTarget(YieldTarget target) {
            if (tryYieldTargets == null)
                tryYieldTargets = new List<YieldTarget>();
            tryYieldTargets.Add(target);
        }

        internal IList<YieldTarget> ElseYieldTargets {
            get { return elseYieldTargets; }
        }

        internal void AddElseYieldTarget(YieldTarget target) {
            if (elseYieldTargets == null)
                elseYieldTargets = new List<YieldTarget>();

            elseYieldTargets.Add(target);
        }

        internal IList<YieldTarget> CatchYieldTargets {
            get { return catchYieldTargets; }
        }

        internal void AddCatchYieldTarget(YieldTarget target) {
            if (catchYieldTargets == null)
                catchYieldTargets = new List<YieldTarget>();

            catchYieldTargets.Add(target);
        }

        internal IList<YieldTarget> FinallyYieldTargets {
            get { return finallyYieldTargets; }
        }

        internal void AddFinallyYieldTarget(YieldTarget target) {
            if (finallyYieldTargets == null)
                finallyYieldTargets = new List<YieldTarget>();

            finallyYieldTargets.Add(target);
        }

        private bool IsBlockYieldable(List<YieldTarget> block) {
            if (block != null && block.Count > 0)
                return true;
            return false;
        }

        // With uses 6 environmental slots 
        internal static int LocalSlots {
            get {
                // Binding looks for the LocalSlots even before the yield targets are counted,
                // Hence we need to set the, max posssible slots i.e 5 currently.
                // Though TryStatement requests 5 slots, during code gen it may use <= 5 based on what all block yield

                // There is scope to optimize here by getting the yield counts before binding. 
                // Once that is done uncomment the code code below, and use the instance of TryStatement in Walk(TryStatement node) in Binder.cs.

                //int ret = 0;
                //if (IsBlockYieldable(tryYieldTargets))
                //    ret++;
                //if (IsBlockYieldable(catchYieldTargets))
                //    ret += 2;
                //if (IsBlockYieldable(elseYieldTargets))
                //    ret++;
                //if (IsBlockYieldable(finallyYieldTargets))
                //    ret++; 
                return 6;
            }
        }

        private void EmitTopYieldTargetLabels(List<YieldTarget> yieldTargets, Slot choiceVar, CodeGen cg) {
            if (IsBlockYieldable(yieldTargets)) {
                Label label = cg.DefineLabel();
                cg.EmitInt(-1);
                choiceVar.EmitSet(cg);
                cg.Emit(OpCodes.Br, label);

                int index = 0;
                foreach (YieldTarget yt in yieldTargets) {
                    cg.MarkLabel(yt.TopBranchTarget);
                    cg.EmitInt(index++);
                    choiceVar.EmitSet(cg);
                    cg.Emit(OpCodes.Br, label);
                }
                cg.MarkLabel(label);
            }
        }

        private void EmitYieldDispatch(List<YieldTarget> yieldTargets, Slot isYielded, Slot choiceVar, CodeGen cg) {
            if (IsBlockYieldable(yieldTargets)) {
                cg.EmitFieldGet(typeof(Ops).GetField("FALSE"));
                isYielded.EmitSet(cg);

                int index = 0;
                foreach (YieldTarget yt in yieldTargets) {
                    choiceVar.EmitGet(cg);
                    cg.EmitInt(index);
                    cg.Emit(OpCodes.Beq, yt.YieldContinuationTarget);
                    index++;
                }
            }
        }

        private void EmitOnYieldBranchToLabel(List<YieldTarget> yieldTargets, Slot isYielded, Label label, CodeGen cg) {
            if (IsBlockYieldable(yieldTargets)) {
                isYielded.EmitGet(cg);
                cg.EmitUnbox(typeof(bool));
                cg.Emit(OpCodes.Brtrue, label);
            }
        }

        // codegen algorithm for unified try-catch-else-finally

        //    isTryYielded = false
        //    isCatchYielded = false
        //    isFinallyYielded = false
        //    isElseYielded = false
        //    Set up the labels for Try Yield Targets
        //    Set up the labels for Catch Yield Targets
        //    Set up the labels for Else Yield Targets
        //    Set up the labels for Finally Yield Targets
        //    returnVar = false
        //    isElseBlock = false

        //TRY:
        //    if isCatchYielded :
        //        rethow  storedException
        //    if finallyYielded :
        //        goto endOfTry
        //    if isElseYielded :
        //        goto beginElseBlock
        //    if isTryYielded:
        //        isTryYielded = false
        //        goto desired_label_in_TRY-BODY
        //    TRY-BODY
        //  beginElseBlock: # Note we are still under TRY
        //    isElseBlock = true
        //    if isElseYielded :
        //        isElseYielded  = false
        //        goto desired_label_in_ELSE-BODY
        //    ELSE-BODY
        //  endOfTry:
        //EXCEPT: # catches any exception
        //    if isElseBlock:
        //        rethrow
        //    pyExc = ExtractException()
        //    storedException = ExtractSysExcInfo()
        //    if pyExc == handler[0].Test :
        //        if isCatchYielded :
        //            isCatchYielded  = false
        //            goto desired_label_in_HANDLER-BODY
        //        HANDLER-BODY
        //        ClearException()
        //        Leave afterFinally
        //    elif pyExc == handler[1].Test :
        //        if isCatchYielded :
        //            isCatchYielded  = false
        //            goto desired_label_in_HANDLER-BODY
        //        HANDLER-BODY
        //        ClearException()
        //        Leave afterFinally
        //    .
        //    .
        //    .
        //    Rethrow
        //FINALLY:
        //    if (isTryYielded  or isCatchYielded  or isElseYielded ):
        //        goto endOfFinally
        //    if isFinallyYielded :
        //        isFinallyYielded  = false
        //        goto desired_label_in_FINALLY-BODY
        //    FINALLY-BODY
        //  endOfFinally:
        // #try-cathch-finally ends here
        // afterFinally:
        //    if not returnVar :
        //      goto noReturn
        //    if (finally may yield ):
        //          return 1
        //    else
        //          return appropriate_return_value
        //  noReturn:


        internal override void Emit(CodeGen cg) {
            // environmental slots
            Slot isTryYielded = null;
            Slot isCatchYielded = null;
            Slot isFinallyYielded = null;
            Slot isElseYielded = null;
            Slot storedException = null;

            // local slots
            Slot tryChoiceVar = null;
            Slot catchChoiceVar = null;
            Slot elseChoiceVar = null;
            Slot finallyChoiceVar = null;

            Slot flowControlVar = cg.GetLocalTmp(typeof(int));
            Slot isElseBlock = null;

            cg.EmitPosition(Start, header);

            if (IsBlockYieldable(tryYieldTargets)) {
                tryChoiceVar = cg.GetLocalTmp(typeof(int));
                isTryYielded = cg.Names.GetTempSlot("is", typeof(object));
                cg.EmitFieldGet(typeof(Ops).GetField("FALSE"));
                isTryYielded.EmitSet(cg);
            }

            if (IsBlockYieldable(catchYieldTargets)) {
                catchChoiceVar = cg.GetLocalTmp(typeof(int));
                storedException = cg.Names.GetTempSlot("exc", typeof(object));
                isCatchYielded = cg.Names.GetTempSlot("is", typeof(object));
                cg.EmitFieldGet(typeof(Ops).GetField("FALSE"));
                isCatchYielded.EmitSet(cg);
            }

            if (IsBlockYieldable(finallyYieldTargets)) {
                finallyChoiceVar = cg.GetLocalTmp(typeof(int));
                isFinallyYielded = cg.Names.GetTempSlot("is", typeof(object));
                cg.EmitFieldGet(typeof(Ops).GetField("FALSE"));
                isFinallyYielded.EmitSet(cg);
            }

            if (IsBlockYieldable(elseYieldTargets)) {
                elseChoiceVar = cg.GetLocalTmp(typeof(int));
                isElseYielded = cg.Names.GetTempSlot("is", typeof(object));
                cg.EmitFieldGet(typeof(Ops).GetField("FALSE"));
                isElseYielded.EmitSet(cg);
            }

            if (elseStmt != null) {
                isElseBlock = cg.GetLocalTmp(typeof(bool));
                cg.EmitInt(0);
                isElseBlock.EmitSet(cg);
            }

            Slot exception = null;
            bool foundLoopControl;
            bool returnInFinally = ControlFlowFinder.FindControlFlow(FinallyStatement, out foundLoopControl);

            if (IsBlockYieldable(finallyYieldTargets)) {
                exception = cg.Names.GetTempSlot("exception", typeof(Exception));
                cg.Emit(OpCodes.Ldnull);
                exception.EmitSet(cg);
            } else if (returnInFinally) {
                exception = cg.GetLocalTmp(typeof(Exception));
                cg.Emit(OpCodes.Ldnull);
                exception.EmitSet(cg);
            }

            EmitTopYieldTargetLabels(tryYieldTargets, tryChoiceVar, cg);
            EmitTopYieldTargetLabels(catchYieldTargets, catchChoiceVar, cg);
            EmitTopYieldTargetLabels(elseYieldTargets, elseChoiceVar, cg);
            EmitTopYieldTargetLabels(finallyYieldTargets, finallyChoiceVar, cg);


            cg.EmitInt(CodeGen.FinallyExitsNormally);
            flowControlVar.EmitSet(cg);

            Label afterFinally = cg.DefineLabel();

            cg.PushExceptionBlock(Targets.TargetBlockType.Try, flowControlVar, isTryYielded);
            cg.BeginExceptionBlock();

            // if catch yielded, rethow the storedException to be handled by Catch block
            if (IsBlockYieldable(catchYieldTargets)) {
                Label testFinally = cg.DefineLabel();
                isCatchYielded.EmitGet(cg);
                cg.EmitUnbox(typeof(bool));
                cg.Emit(OpCodes.Brfalse, testFinally);
                storedException.EmitGet(cg);
                cg.EmitCall(typeof(Ops), "Raise", new Type[] { typeof(object) });
                cg.MarkLabel(testFinally);
            }

            // if Finally yielded, Branch to the end of Try block
            Label endOfTry = cg.DefineLabel();
            EmitOnYieldBranchToLabel(finallyYieldTargets, isFinallyYielded, endOfTry, cg);

            Label beginElseBlock = cg.DefineLabel();
            if (IsBlockYieldable(elseYieldTargets)) {
                // isElseYielded ?
                Debug.Assert(isElseYielded != null);
                isElseYielded.EmitGet(cg);
                cg.EmitUnbox(typeof(bool));
                cg.Emit(OpCodes.Brtrue, beginElseBlock);
            }


            EmitYieldDispatch(tryYieldTargets, isTryYielded, tryChoiceVar, cg);

            body.Emit(cg);

            if (elseStmt != null) {
                if (IsBlockYieldable(elseYieldTargets)) {
                    cg.MarkLabel(beginElseBlock);
                }

                cg.PopTargets(Targets.TargetBlockType.Try);
                cg.PushExceptionBlock(Targets.TargetBlockType.Else, flowControlVar, isElseYielded);

                cg.EmitInt(1);
                isElseBlock.EmitSet(cg);

                EmitYieldDispatch(elseYieldTargets, isElseYielded, elseChoiceVar, cg);

                elseStmt.Emit(cg);
                cg.PopTargets(Targets.TargetBlockType.Else);
                cg.PushExceptionBlock(Targets.TargetBlockType.Try, flowControlVar, isTryYielded);

            }

            cg.MarkLabel(endOfTry);

            // get the exception if there is a yield / return  in finally
            if (IsBlockYieldable(finallyYieldTargets) || returnInFinally) {
                cg.BeginCatchBlock(typeof(Exception));
                exception.EmitSet(cg);
            }

            if (handlers != null) {
                cg.PushExceptionBlock(Targets.TargetBlockType.Catch, flowControlVar, isCatchYielded);
                if (IsBlockYieldable(finallyYieldTargets) || returnInFinally) {
                    exception.EmitGet(cg);
                } else {
                    cg.BeginCatchBlock(typeof(Exception));
                }


                // if in Catch block due to exception in else block -> just rethrow
                if (elseStmt != null) {
                    Label beginCatchBlock = cg.DefineLabel();
                    isElseBlock.EmitGet(cg);
                    cg.Emit(OpCodes.Brfalse, beginCatchBlock);
                    cg.Emit(OpCodes.Rethrow);
                    cg.MarkLabel(beginCatchBlock);
                }

                // Extract state from the carrier exception
                cg.EmitCallerContext();
                cg.EmitCall(typeof(Ops), "ExtractException",
                    new Type[] { typeof(Exception), typeof(ICallerContext) });
                Slot pyExc = cg.GetLocalTmp(typeof(object));
                Slot tmpExc = cg.GetLocalTmp(typeof(object));
                pyExc.EmitSet(cg);

                if (IsBlockYieldable(catchYieldTargets)) {
                    cg.EmitCallerContext();
                    cg.EmitCall(typeof(Ops), "ExtractSysExcInfo");
                    storedException.EmitSet(cg);
                }

                foreach (TryStatementHandler handler in handlers) {
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

                    if (IsBlockYieldable(finallyYieldTargets) || returnInFinally) {
                        cg.Emit(OpCodes.Ldnull);
                        exception.EmitSet(cg);
                    }

                    EmitYieldDispatch(catchYieldTargets, isCatchYielded, catchChoiceVar, cg);
                    handler.Body.Emit(cg);

                    cg.EmitCallerContext();
                    cg.EmitCall(typeof(Ops), "ClearException", new Type[] { typeof(ICallerContext) });

                    cg.Emit(OpCodes.Leave, afterFinally);
                    cg.MarkLabel(next);

                }

                cg.FreeLocalTmp(tmpExc);
                cg.FreeLocalTmp(pyExc);

                cg.Emit(OpCodes.Rethrow);
                cg.PopTargets(Targets.TargetBlockType.Catch);
            }

            if (finallyStmt != null) {
                cg.PushExceptionBlock(Targets.TargetBlockType.Finally, flowControlVar, isFinallyYielded);
                cg.BeginFinallyBlock();

                Label endOfFinally = cg.DefineLabel();

                // if try yielded
                EmitOnYieldBranchToLabel(tryYieldTargets, isTryYielded, endOfFinally, cg);
                // if catch yielded
                EmitOnYieldBranchToLabel(catchYieldTargets, isCatchYielded, endOfFinally, cg);
                //if else yielded
                EmitOnYieldBranchToLabel(elseYieldTargets, isElseYielded, endOfFinally, cg);

                EmitYieldDispatch(finallyYieldTargets, isFinallyYielded, finallyChoiceVar, cg);
                finallyStmt.Emit(cg);

                if (IsBlockYieldable(finallyYieldTargets) || returnInFinally) {
                    Label nothrow = cg.DefineLabel();
                    exception.EmitGet(cg);
                    cg.Emit(OpCodes.Dup);
                    cg.Emit(OpCodes.Brfalse_S, nothrow);
                    cg.Emit(OpCodes.Throw);
                    cg.MarkLabel(nothrow);
                    cg.Emit(OpCodes.Pop);
                }


                cg.MarkLabel(endOfFinally);
                cg.EndExceptionBlock();
                cg.PopTargets(Targets.TargetBlockType.Finally);
            } else {
                cg.EndExceptionBlock();
            }
            cg.PopTargets(Targets.TargetBlockType.Try);

            cg.MarkLabel(afterFinally);
            Label noReturn = cg.DefineLabel();
            flowControlVar.EmitGet(cg);
            cg.EmitInt(CodeGen.BranchForReturn);
            cg.Emit(OpCodes.Bne_Un, noReturn);

            if (cg.IsGenerator()) {
                // return true from the generator method
                cg.Emit(OpCodes.Ldc_I4_1);
                cg.EmitReturn();
            } else if (returnInFinally) {
                // return the actual value
                cg.EmitReturnValue();
                cg.EmitReturn();
            }

            cg.MarkLabel(noReturn);

            if (foundLoopControl) {
                noReturn = cg.DefineLabel();
                flowControlVar.EmitGet(cg);
                cg.EmitInt(CodeGen.BranchForBreak);
                cg.Emit(OpCodes.Bne_Un, noReturn);
                cg.EmitBreak();
                cg.MarkLabel(noReturn);

                noReturn = cg.DefineLabel();
                flowControlVar.EmitGet(cg);
                cg.EmitInt(CodeGen.BranchForContinue);
                cg.Emit(OpCodes.Bne_Un, noReturn);
                cg.EmitContinue();
                cg.MarkLabel(noReturn);
            }

            // clean up 
            if (IsBlockYieldable(tryYieldTargets)) {
                cg.FreeLocalTmp(tryChoiceVar);
                tryYieldTargets.Clear();
            }
            if (IsBlockYieldable(catchYieldTargets)) {
                cg.FreeLocalTmp(catchChoiceVar);
                catchYieldTargets.Clear();
            }
            if (IsBlockYieldable(finallyYieldTargets)) {
                cg.FreeLocalTmp(finallyChoiceVar);
                finallyYieldTargets.Clear();
            }
            if (IsBlockYieldable(elseYieldTargets)) {
                cg.FreeLocalTmp(elseChoiceVar);
                elseYieldTargets.Clear();
            }
            if (elseStmt != null) {
                cg.FreeLocalTmp(isElseBlock);
            }

#if DEBUG
            cg.EmitInt(-1);
            flowControlVar.EmitSet(cg);
#endif

            cg.FreeLocalTmp(flowControlVar);
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                body.Walk(walker);
                if (handlers != null)
                    foreach (TryStatementHandler handler in handlers)
                        handler.Walk(walker);

                if (elseStmt != null)
                    elseStmt.Walk(walker);

                if (finallyStmt != null)
                    finallyStmt.Walk(walker);

            }
            walker.PostWalk(this);
        }
    }

    public class TryFinallyStatement : Statement {
        internal override void Emit(CodeGen cg) {
            throw new InvalidOperationException("TryFinallyStatement is deprecated");
        }

        public override void Walk(IAstWalker walker) {            
        }
    }

    public class TryStatementHandler : Node {
        private Location header;
        private readonly Expression test, target;
        private readonly Statement body;

        public TryStatementHandler(Expression test, Expression target, Statement body) {
            this.test = test; this.target = target; this.body = body;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public Expression Target {
            get { return target; }
        }

        public Expression Test {
            get { return test; }
        }

        public Statement Body {
            get { return body; }
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                if (test != null) test.Walk(walker);
                if (target != null) target.Walk(walker);
                body.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }



    public class ExpressionStatement : Statement {
        private readonly Expression expr;

        public ExpressionStatement(Expression expression) { this.expr = expression; }

        public Expression Expression {
            get { return expr; }
        }

        public override object Execute(NameEnvironment environment) {
            expr.Evaluate(environment);
            return NextStatement;
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                expr.Walk(walker);
            }
            walker.PostWalk(this);
        }

        public override string Documentation {
            get {
                ConstantExpression ce = expr as ConstantExpression;
                if (ce != null) {
                    return ce.Value as string;
                }
                return null;
            }
        }
    }

    public class AssignStatement : Statement {
        // lhs.Length is 1 for simple assignments like "x = 1"
        // lhs.Lenght will be 3 for "x = y = z = 1"
        private readonly Expression[] lhs;
        private readonly Expression rhs;

        public AssignStatement(Expression[] leftSide, Expression rightSide) {
            this.lhs = leftSide;
            this.rhs = rightSide;
        }

        public IList<Expression> Left {
            get { return lhs; }
        }

        public Expression Right {
            get { return rhs; }
        }

        public override object Execute(NameEnvironment environment) {
            object v = rhs.Evaluate(environment);
            foreach (Expression e in lhs) {
                e.Assign(v, environment);
            }
            return NextStatement;
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            rhs.Emit(cg);
            for (int i = 0; i < lhs.Length; i++) {
                if (i < lhs.Length - 1) cg.Emit(OpCodes.Dup);
                lhs[i].EmitSet(cg);
            }
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                foreach (Expression e in lhs) e.Walk(walker);
                rhs.Walk(walker);
            }
            walker.PostWalk(this);
        }

    }

    public class AugAssignStatement : Statement {
        private readonly BinaryOperator op;
        private readonly Expression lhs;
        private readonly Expression rhs;

        public AugAssignStatement(BinaryOperator binaryOperator, Expression leftSide, Expression rightSide) {
            this.op = binaryOperator; this.lhs = leftSide; this.rhs = rightSide;
        }

        public BinaryOperator Operator {
            get { return op; }
        }

        public Expression Left {
            get { return lhs; }
        }

        public Expression Right {
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                lhs.Walk(walker);
                rhs.Walk(walker);
            }
            walker.PostWalk(this);
        }

    }

    public class PrintStatement : Statement {
        private readonly Expression dest;
        private readonly Expression[] exprs;
        private readonly bool trailingComma;

        public PrintStatement(Expression destination, Expression[] expressions, bool trailingComma) {
            this.dest = destination; this.exprs = expressions;
            this.trailingComma = trailingComma;
        }

        public Expression Destination {
            get { return dest; }
        }

        public IList<Expression> Expressions {
            get { return exprs; }
        }

        public bool TrailingComma {
            get { return trailingComma; }
        }

        public override object Execute(NameEnvironment environment) {
            Console.Out.Write("print> ");
            foreach (Expression e in exprs) {
                object val = e.Evaluate(environment);
                Ops.PrintComma(environment.Globals.SystemState, val);
            }
            if (!trailingComma) Ops.PrintNewline(environment.Globals.SystemState);

            return NextStatement;
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
        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                if (dest != null) dest.Walk(walker);
                foreach (Expression e in exprs) e.Walk(walker);
            }
            walker.PostWalk(this);
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                ;
            }
            walker.PostWalk(this);
        }
    }

    public class ImportStatement : Statement {
        private readonly DottedName[] names;
        private readonly SymbolId[] asNames;

        public ImportStatement(DottedName[] names, SymbolId[] asNames) {
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

        public override void Walk(IAstWalker walker) {
            walker.Walk(this);
            walker.PostWalk(this);
        }
    }

    public class FromImportStatement : Statement {
        private static readonly SymbolId[] star = new SymbolId[1];
        private readonly DottedName root;
        private readonly IList<SymbolId> names;
        private readonly IList<SymbolId> asNames;
        private readonly bool fromFuture;

        public FromImportStatement(DottedName root, IList<SymbolId> names, SymbolId[] asNames)
            : this(root, names, asNames, false) {
        }

        public static IList<SymbolId> Star {
            get { return FromImportStatement.star; }
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

        public FromImportStatement(DottedName root, IList<SymbolId> names, IList<SymbolId> asNames, bool fromFuture) {
            this.root = root;
            this.names = names;
            this.asNames = asNames;
            this.fromFuture = fromFuture;
        }

        public override object Execute(NameEnvironment environment) {
            Ops.ImportFrom(environment.Globals, root.MakeString(), SymbolTable.IdsToStrings(names));

            return NextStatement;
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

                for (int i = 0; i < names.Count; i++) {
                    cg.EmitCallerContext();
                    fromObj.EmitGet(cg);
                    cg.EmitString(names[i].GetString());
                    cg.EmitCall(typeof(Ops), "ImportOneFrom");

                    SymbolId asName;
                    if (i < asNames.Count && asNames[i] != SymbolTable.Empty)
                        asName = asNames[i];
                    else
                        asName = names[i];

                    cg.EmitSet(asName);
                }
            }
        }

        public override void Walk(IAstWalker walker) {
            walker.Walk(this);
            walker.PostWalk(this);
        }

        internal bool IsFromFuture {
            get {
                return fromFuture;
            }
        }
    }

    public class GlobalStatement : Statement {
        private readonly SymbolId[] names;

        public GlobalStatement(SymbolId[] names) {
            this.names = names;
        }

        public IList<SymbolId> Names {
            get { return names; }
        }

        public override object Execute(NameEnvironment environment) {
            return NextStatement;
        }

        internal override void Emit(CodeGen cg) {
        }

        public override void Walk(IAstWalker walker) {
            walker.Walk(this);
            walker.PostWalk(this);
        }
    }

    public class DelStatement : Statement {
        private readonly Expression[] exprs;

        public DelStatement(Expression[] expressions) {
            this.exprs = expressions;
        }

        public IList<Expression> Expressions {
            get { return exprs; }
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            foreach (Expression expr in exprs) {
                expr.EmitDel(cg);
            }
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                foreach (Expression e in exprs) e.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class RaiseStatement : Statement {
        private readonly Expression type, value, traceback;

        public RaiseStatement(Expression exceptionType, Expression exceptionValue, Expression traceBack) {
            this.type = exceptionType; this.value = exceptionValue; this.traceback = traceBack;
        }

        public Expression TraceBack {
            get { return traceback; }
        }

        public Expression Value {
            get { return this.value; }
        }


        public Expression ExceptionType {
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
        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                if (type != null) type.Walk(walker);
                if (value != null) value.Walk(walker);
                if (traceback != null) traceback.Walk(walker);
            }
            walker.PostWalk(this);
        }

    }

    public class AssertStatement : Statement {
        private readonly Expression test, message;

        public AssertStatement(Expression test, Expression message) {
            this.test = test;
            this.message = message;
        }

        public Expression Message {
            get { return message; }
        }

        public Expression Test {
            get { return test; }
        }

        internal override void Emit(CodeGen cg) {
            if (Options.DebugMode) {
                cg.EmitPosition(Start, End);
                cg.EmitTestTrue(test);
                Label endLabel = cg.DefineLabel();
                cg.Emit(OpCodes.Brtrue, endLabel);
                cg.EmitExprOrNone(message);
                cg.EmitConvertFromObject(typeof(string));
                cg.EmitCall(typeof(Ops), "AssertionError", new Type[] { typeof(string) });
                cg.Emit(OpCodes.Throw);
                cg.MarkLabel(endLabel);
            }
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                test.Walk(walker);
                if (message != null) message.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }



    public class ExecStatement : Statement {
        private readonly Expression code, locals, globals;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "2#globals")]
        public ExecStatement(Expression code, Expression locals, Expression globals) {
            this.code = code;
            this.locals = locals;
            this.globals = globals;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Globals")]
        public Expression Globals {
            get { return globals; }
        }

        public Expression Locals {
            get { return locals; }
        }

        public Expression Code {
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
                cg.EmitConvertFromObject(typeof(IAttributesDictionary));
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                code.Walk(walker);
                if (locals != null) locals.Walk(walker);
                if (globals != null) globals.Walk(walker);
            }
            walker.PostWalk(this);
        }

    }

    public class ReturnStatement : Statement {
        private readonly Expression expr;

        public ReturnStatement(Expression expression) {
            this.expr = expression;
        }

        public Expression Expression {
            get { return expr; }
        }

        public override object Execute(NameEnvironment environment) {
            return expr.Evaluate(environment);
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
            cg.EmitReturn(expr);
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                if (expr != null) expr.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class YieldStatement : Statement {
        private readonly Expression expr;
        private readonly int index;
        private Label label;

        public YieldStatement(Expression expression, int index) {
            this.expr = expression;
            this.index = index;
        }

        public Expression Expression {
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                expr.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class PassStatement : Statement {
        public PassStatement() { }

        public override object Execute(NameEnvironment environment) {
            return NextStatement;
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, End);
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                ;
            }
            walker.PostWalk(this);
        }

    }

    public class BreakStatement : Statement {
        public BreakStatement() { }

        internal override void Emit(CodeGen cg) {
            if (!cg.InLoop()) {
                cg.Context.AddError("'break' not properly in loop", this);
                return;
            }
            cg.EmitPosition(Start, End);
            cg.EmitBreak();
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                ;
            }
            walker.PostWalk(this);
        }

    }

    public class ContinueStatement : Statement {
        public ContinueStatement() { }

        internal override void Emit(CodeGen cg) {
            if (!cg.InLoop()) {
                cg.Context.AddError("'continue' not properly in loop", this);
                return;
            }
            cg.EmitPosition(Start, End);
            cg.EmitContinue();
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                ;
            }
            walker.PostWalk(this);
        }
    }
}
