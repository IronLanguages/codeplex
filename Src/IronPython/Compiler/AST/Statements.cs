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

    public class TryStatement : Statement {
        private Location header;
        private readonly Statement body;
        private readonly TryStatementHandler[] handlers;
        private readonly Statement elseStmt;
        private bool yieldInExcept = false;
        private List<YieldTarget> yieldTargets = new List<YieldTarget>();

        public TryStatement(Statement body, TryStatementHandler[] handlers, Statement else_) {
            this.body = body; this.handlers = handlers; this.elseStmt = else_;
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

        public bool YieldInExcept {
            get { return yieldInExcept; }
            set { yieldInExcept = value; }
        }

        public IList<YieldTarget> YieldTargets {
            get { return yieldTargets; }
        }

        public void AddYieldTarget(YieldTarget target) {
            yieldTargets.Add(target);
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
                    cg.MarkLabel(yt.TopBranchTarget);
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
                    cg.Emit(OpCodes.Beq, yt.TryBranchTarget);
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                body.Walk(walker);
                foreach (TryStatementHandler handler in handlers) handler.Walk(walker);
                if (elseStmt != null) elseStmt.Walk(walker);
            }
            walker.PostWalk(this);
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

    public class TryFinallyStatement : Statement {
        private Location header;
        private readonly Statement body;
        private readonly Statement finallyStmt;
        private List<YieldTarget> yieldTargets = new List<YieldTarget>();

        public TryFinallyStatement(Statement body, Statement finally_) {
            this.body = body; this.finallyStmt = finally_;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public Statement Body {
            get { return body; }
        }

        public Statement FinallyStmt {
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
                    cg.MarkLabel(yt.TopBranchTarget);
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
                    cg.Emit(OpCodes.Beq, yt.TryBranchTarget);
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                body.Walk(walker);
                finallyStmt.Walk(walker);
            }
            walker.PostWalk(this);
        }

        public void AddYieldTarget(YieldTarget target) {
            yieldTargets.Add(target);
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
            if (IronPython.Hosting.PythonEngine.options.DebugMode) {
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
