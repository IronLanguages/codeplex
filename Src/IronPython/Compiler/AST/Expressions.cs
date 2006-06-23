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
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using System.CodeDom;
using System.Text;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.CodeDom;
using IronPython.Compiler.Generation;

namespace IronPython.Compiler.AST {
    /// <summary>
    /// Summary description for Expr.
    /// </summary>
    public abstract class Expr : Node {
        internal virtual object Evaluate(NameEnv env) {
            throw new NotImplementedException("Evaluate: " + this);
        }

        internal virtual void Assign(object val, NameEnv env) {
            throw new NotImplementedException("Assign: " + this);
        }

        internal abstract void Emit(CodeGen cg);

        internal virtual void EmitSet(CodeGen cg) {
            cg.Context.AddError("can't assign to " + this.GetType().Name, this);
        }

        internal virtual void EmitDel(CodeGen cg) {
            cg.Context.AddError("can't perform deletion", this);
        }

        internal static object[] Evaluate(Expr[] items, NameEnv env) {
            object[] ret = new object[items.Length];
            for (int i = 0; i < items.Length; i++) {
                ret[i] = items[i].Evaluate(env);
            }
            return ret;
        }
    }

    public class ErrorExpr : Expr {
        public override void Walk(IAstWalker w) {
        }

        internal override void Emit(CodeGen cg) {
            cg.Context.AddError("can't generate error expression", this);
        }
    }

    public class CallExpr : Expr {
        private readonly Expr target;
        private readonly Arg[] args;
        private bool hasArgsTuple, hasKeywordDict;
        private int keywordCount, extraArgs;

        public CallExpr(Expr target, Arg[] args, bool hasArgsTuple, bool hasKeywordDict, int keywordCount, int extraArgs) {
            this.target = target;
            this.args = args;
            this.hasArgsTuple = hasArgsTuple;
            this.hasKeywordDict = hasKeywordDict;
            this.keywordCount = keywordCount;
            this.extraArgs = extraArgs;
        }

        public IList<Arg> Args {
            get { return args; }
        }

        public Expr Target {
            get { return target; }
        }

        public bool MightNeedLocalsDictionary() {
            NameExpr nameExpr = target as NameExpr;
            if (nameExpr == null) return false;

            if (args.Length == 0) {
                if (nameExpr.Name == SymbolTable.Locals) return true;
                if (nameExpr.Name == SymbolTable.Vars) return true;
                if (nameExpr.Name == SymbolTable.Dir) return true;
                return false;
            } else {
                if (nameExpr.Name == SymbolTable.Eval) return true;
            }
            return false;
        }

        internal override void EmitDel(CodeGen cg) {
            cg.Context.AddError("can't delete function call", this);
        }

        internal override object Evaluate(NameEnv env) {
            object callee = target.Evaluate(env);

            object[] cargs = new object[args.Length];
            int index = 0;
            foreach (Arg arg in args) {
                if (arg.Name != SymbolTable.Empty) throw new NotImplementedException("keywords");
                cargs[index++] = arg.Expression.Evaluate(env);
            }

            switch (cargs.Length) {
                case 0: return Ops.Call(callee);
                default: return Ops.Call(callee, cargs);
            }
        }

        internal override void Emit(CodeGen cg) {
            Label done = new Label();
            bool emitDone = false;

            Expr[] exprs = new Expr[args.Length - extraArgs];
            Expr argsTuple = null, keywordDict = null;
            string[] keywordNames = new string[keywordCount];
            int index = 0, keywordIndex = 0;
            foreach (Arg arg in args) {
                if (arg.Name == SymbolTable.Star) {
                    argsTuple = arg.Expression; continue;
                } else if (arg.Name == SymbolTable.StarStar) {
                    keywordDict = arg.Expression; continue;
                } else if (arg.Name != SymbolTable.Empty) {
                    keywordNames[keywordIndex++] = arg.Name.GetString();
                }
                exprs[index++] = arg.Expression;
            }

            if (hasKeywordDict || (hasArgsTuple && keywordCount > 0)) {
                cg.EmitCallerContext();
                target.Emit(cg);
                cg.EmitObjectArray(exprs);
                cg.EmitStringArray(keywordNames);
                cg.EmitExprOrNone(argsTuple);
                cg.EmitExprOrNone(keywordDict);
                cg.EmitCall(typeof(Ops), "CallWithArgsTupleAndKeywordDictAndContext",
                    new Type[] { typeof(ICallerContext), typeof(object), typeof(object[]), typeof(string[]),
							   typeof(object), typeof(object)});
            } else if (hasArgsTuple) {
                cg.EmitCallerContext();
                target.Emit(cg);
                cg.EmitObjectArray(exprs);
                cg.EmitExprOrNone(argsTuple);
                cg.EmitCall(typeof(Ops), "CallWithArgsTupleAndContext",
                    new Type[] { typeof(ICallerContext), typeof(object), typeof(object[]), typeof(object) });
            } else if (keywordCount > 0) {
                cg.EmitCallerContext();
                target.Emit(cg);
                cg.EmitObjectArray(exprs);
                cg.EmitStringArray(keywordNames);
                cg.EmitCall(typeof(Ops), "Call",
                    new Type[] { typeof(ICallerContext), typeof(object), typeof(object[]), typeof(string[]) });
            } else {
                cg.EmitCallerContext();
                target.Emit(cg);
                if (args.Length <= Ops.MaximumCallArgs) {
                    Type[] argTypes = new Type[args.Length + 2];
                    int i = 0;
                    argTypes[i++] = typeof(ICallerContext);
                    argTypes[i++] = typeof(object);
                    foreach (Expr e in exprs) {
                        e.Emit(cg);
                        argTypes[i++] = typeof(object);
                    }
                    cg.EmitCall(typeof(Ops), "CallWithContext", argTypes);
                } else {
                    cg.EmitObjectArray(exprs);
                    cg.EmitCall(typeof(Ops), "CallWithContext",
                        new Type[] { typeof(ICallerContext), typeof(object), typeof(object[]) });
                }
            }

            if (emitDone) {
                cg.MarkLabel(done);
            }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                target.Walk(w);
                foreach (Arg arg in args) arg.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class Arg : Node {
        private readonly SymbolId name;
        private readonly Expr expr;

        public Arg(Expr expr) : this(SymbolTable.Empty, expr) { }

        public Arg(SymbolId name, Expr expr) {
            this.name = name;
            this.expr = expr;
        }

        public SymbolId Name {
            get { return name; }
        }

        public Expr Expression {
            get { return expr; }
        } 

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                expr.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class FieldExpr : Expr {
        private readonly Expr target;
        private readonly SymbolId name;

        public FieldExpr(Expr target, SymbolId name) {
            this.target = target;
            this.name = name;
        }

        public Expr Target {
            get { return target; }
        }

        public SymbolId Name {
            get { return name; }
        }

        internal override object Evaluate(NameEnv env) {
            object t = target.Evaluate(env);
            return Ops.GetAttr(env.globals, t, SymbolTable.StringToId(name.GetString()));
        }

        internal override void Assign(object val, NameEnv env) {
            object t = target.Evaluate(env);
            Ops.SetAttr(env.globals, t, SymbolTable.StringToId(name.GetString()), val);
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitCallerContext();
            target.Emit(cg);

            cg.EmitSymbolId(name);

            cg.EmitCall(typeof(Ops), "GetAttr"); //, new Type[] { typeof(object), typeof(SymbolId) });
        }

        internal override void EmitSet(CodeGen cg) {
            target.Emit(cg);
            cg.EmitSymbolId(name);
            cg.EmitCallerContext();
            cg.EmitCall(typeof(Ops), "SetAttrStackHelper");
        }

        internal override void EmitDel(CodeGen cg) {
            cg.EmitCallerContext();
            target.Emit(cg);
            cg.EmitSymbolId(name);
            cg.EmitCall(typeof(Ops), "DelAttr");
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                target.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class IndexExpr : Expr {
        private readonly Expr target;
        private readonly Expr index;

        public IndexExpr(Expr target, Expr index) {
            this.target = target;
            this.index = index;
        }

        public Expr Target {
            get { return target; }
        }

        public Expr Index {
            get { return index; }
        }

        internal override object Evaluate(NameEnv env) {
            object t = target.Evaluate(env);
            object i = index.Evaluate(env);
            return Ops.GetIndex(t, i);
        }

        internal override void Assign(object val, NameEnv env) {
            object t = target.Evaluate(env);
            object i = index.Evaluate(env);
            Ops.SetIndex(t, i, val);
        }

        internal override void Emit(CodeGen cg) {
            target.Emit(cg);
            index.Emit(cg);
            cg.EmitCall(typeof(Ops), "GetIndex");
        }


        internal override void EmitSet(CodeGen cg) {
            target.Emit(cg);
            index.Emit(cg);
            cg.EmitCall(typeof(Ops), "SetIndexStackHelper");
        }

        internal override void EmitDel(CodeGen cg) {
            target.Emit(cg);
            index.Emit(cg);
            cg.EmitCall(typeof(Ops), "DelIndex");
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                target.Walk(w);
                index.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public abstract class SequenceExpr : Expr {
        private readonly Expr[] items;

        protected SequenceExpr(params Expr[] items) { this.items = items; }
        protected abstract string EmptySequenceString { get; }

        public Expr[] Items {
            get { return items; }
        } 

        internal override void Assign(object val, NameEnv env) {
            // Disallow "[] = l", "[], a = l, l", "[[]] = [l]", etc
            if (items.Length == 0) {
                throw Ops.SyntaxError("can't assign to " + EmptySequenceString, "<unknown>",
                    Start.line, Start.column, null, 0, IronPython.Hosting.Severity.Error);
            }

            IEnumerator ie = Ops.GetEnumerator(val);

            int leftCount = items.Length;
            object[] values = new object[leftCount];

            int rightCount = Ops.GetEnumeratorValues(ie, ref values);
            if (leftCount != rightCount)
                throw Ops.ValueErrorForUnpackMismatch(leftCount, rightCount);

            for (int i = 0; i < leftCount; i++)
                items[i].Assign(values[i], env);
        }

        internal override void EmitSet(CodeGen cg) {
            // Disallow "[] = l", "[], a = l, l", "[[]] = [l]", etc
            if (items.Length == 0) {
                cg.Context.AddError("can't assign to " + EmptySequenceString, this);
                return;
            }

            // int leftCount = items.Length;
            Slot leftCount = cg.GetLocalTmp(typeof(int));
            cg.EmitInt(items.Length);
            leftCount.EmitSet(cg);

            // object[] values = new object[leftCount]; 
            Slot values = cg.GetLocalTmp(typeof(object[]));
            leftCount.EmitGet(cg);
            cg.Emit(OpCodes.Newarr, typeof(object));
            values.EmitSet(cg);

            // ie = Ops.GetEnumerator(<value on stack>)
            Slot ie = cg.GetLocalTmp(typeof(IEnumerator));
            cg.EmitCall(typeof(Ops), "GetEnumerator");
            ie.EmitSet(cg);

            // int rightCount = Ops.GetEnumeratorValues(ie, ref values);
            Slot rightCount = cg.GetLocalTmp(typeof(int));

            ie.EmitGet(cg);
            values.EmitGetAddr(cg);
            cg.EmitCall(typeof(Ops), "GetEnumeratorValues");
            rightCount.EmitSet(cg);

            // if (leftCount != rightCount)
            //      throw Ops.ValueErrorForUnpackMismatch(leftCount, rightCount);
            Label equalSizes = cg.DefineLabel();

            leftCount.EmitGet(cg);
            rightCount.EmitGet(cg);
            cg.Emit(OpCodes.Ceq);
            cg.Emit(OpCodes.Brtrue_S, equalSizes);

            leftCount.EmitGet(cg);
            rightCount.EmitGet(cg);
            cg.EmitCall(typeof(Ops).GetMethod("ValueErrorForUnpackMismatch"));
            cg.Emit(OpCodes.Throw);

            cg.MarkLabel(equalSizes);

            // for (int i = 0; i < leftCount; i++)
            //     items[i].Assign(values[i], env);

            int i = 0;
            foreach (Expr expr in items) {
                values.EmitGet(cg);
                cg.EmitInt(i++);
                cg.Emit(OpCodes.Ldelem_Ref);
                expr.EmitSet(cg);
            }

            cg.FreeLocalTmp(leftCount);
            cg.FreeLocalTmp(rightCount);
            cg.FreeLocalTmp(values);
            cg.FreeLocalTmp(ie);
        }

        internal override void EmitDel(CodeGen cg) {
            foreach (Expr expr in items) {
                expr.EmitDel(cg);
            }
        }
    }


    public class TupleExpr : SequenceExpr {
        private bool expandable;

        public TupleExpr(params Expr[] items)
            : this(false, items) {
        }
        public TupleExpr(bool expandable, params Expr[] items)
            : base(items) {
            this.expandable = expandable;
        }

        protected override string EmptySequenceString { get { return "()"; } }

        internal override object Evaluate(NameEnv env) {
            return Ops.MakeTuple(Evaluate(Items, env));
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitObjectArray(Items);
            cg.EmitCall(typeof(Ops), expandable ? "MakeExpandableTuple" : "MakeTuple", new Type[] { typeof(object[]) });
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (Expr e in Items) e.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class ListExpr : SequenceExpr {
        public ListExpr(params Expr[] items) : base(items) { }

        protected override string EmptySequenceString { get { return "[]"; } }

        internal override object Evaluate(NameEnv env) {
            return Ops.MakeList(Evaluate(Items, env));
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitObjectArray(Items);
            cg.EmitCall(typeof(Ops), "MakeList", new Type[] { typeof(object[]) });
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (Expr e in Items) e.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class DictExpr : Expr {
        private readonly SliceExpr[] items;

        public DictExpr(params SliceExpr[] items) { this.items = items; }

        public SliceExpr[] Items {
            get { return items; }
        }

        internal override object Evaluate(NameEnv env) {
            IDictionary<object, object> dict = Ops.MakeDict(items.Length);
            foreach (SliceExpr s in items) {
                dict[s.SliceStart.Evaluate(env)] = s.SliceStop.Evaluate(env);
            }
            return dict;
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitInt(items.Length);
            cg.EmitCall(typeof(Ops), "MakeDict");
            foreach (SliceExpr s in items) {
                cg.Emit(OpCodes.Dup);
                s.SliceStart.Emit(cg);
                s.SliceStop.Emit(cg);
                cg.EmitCall(typeof(Dict).GetProperty("Item").GetSetMethod());
            }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (SliceExpr e in items) e.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class SliceExpr : Expr {
        private readonly Expr sliceStart, sliceStop, sliceStep;

        public SliceExpr(Expr start, Expr stop, Expr step) {
            this.sliceStart = start;
            this.sliceStop = stop;
            this.sliceStep = step;
        }


        public Expr SliceStep {
            get { return sliceStep; }
        }

        public Expr SliceStop {
            get { return sliceStop; }
        }

        public Expr SliceStart {
            get { return sliceStart; }
        } 

        internal override object Evaluate(NameEnv env) {
            object e1 = sliceStart.Evaluate(env);
            object e2 = sliceStop.Evaluate(env);
            object e3 = sliceStep.Evaluate(env);
            return Ops.MakeSlice(e1, e2, e3);
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitExprOrNone(sliceStart);
            cg.EmitExprOrNone(sliceStop);
            cg.EmitExprOrNone(sliceStep);
            cg.EmitCall(typeof(Ops), "MakeSlice");
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                if (sliceStart != null) sliceStart.Walk(w);
                if (sliceStop != null) sliceStop.Walk(w);
                if (sliceStep != null) sliceStep.Walk(w);
            }
            w.PostWalk(this);
        }
    }


    public class BackquoteExpr : Expr {
        private readonly Expr expr;

        public BackquoteExpr(Expr expr) { this.expr = expr; }

        public Expr Expression {
            get { return expr; }
        }

        internal override object Evaluate(NameEnv env) {
            return Ops.Repr(expr.Evaluate(env));
        }

        internal override void Emit(CodeGen cg) {
            expr.Emit(cg);
            cg.EmitCall(typeof(Ops), "Repr");
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                expr.Walk(w);
            }
            w.PostWalk(this);
        }
    }
    public class ParenExpr : Expr {
        private readonly Expr expr;

        public ParenExpr(Expr expr) { this.expr = expr; }

        public Expr Expression {
            get { return expr; }
        }

        internal override object Evaluate(NameEnv env) {
            return expr.Evaluate(env);
        }

        internal override void Emit(CodeGen cg) {
            expr.Emit(cg);
        }

        internal override void EmitDel(CodeGen cg) {
            expr.EmitDel(cg);
        }

        internal override void EmitSet(CodeGen cg) {
            expr.EmitSet(cg);
        }

        internal override void Assign(object val, NameEnv env) {
            expr.Assign(val, env);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                expr.Walk(w);
            }
            w.PostWalk(this);
        }
    }


    public class ConstantExpr : Expr {
        private readonly object value;

        public ConstantExpr(object value) {
            this.value = value;
        }

        public object Value {
            get { return this.value; }
        } 

        internal override object Evaluate(NameEnv env) {
            return value;
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitConstant(value);
        }

        internal override void EmitSet(CodeGen cg) {
            if (value == null) {
                cg.Context.AddError("assignment to None", this);
            }

            cg.Context.AddError("can't assign to literal", this);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                ;
            }
            w.PostWalk(this);
        }
    }

    public class NameExpr : Expr {
        private readonly SymbolId name;
        private bool defined;

        public NameExpr(SymbolId name) { this.name = name; }

        public SymbolId Name {
            get { return name; }
        }

        public bool IsDefined {
            get { return defined; }
            set { defined = value; }
        }

        internal override object Evaluate(NameEnv env) {
            return env.Get(name.GetString());
        }

        internal override void Assign(object val, NameEnv env) {
            env.Set(name.GetString(), val);
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitGet(name, !defined);
        }

        internal override void EmitSet(CodeGen cg) {
            cg.EmitSet(name);
        }

        internal override void EmitDel(CodeGen cg) {
            cg.EmitDel(name, !defined);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                ;
            }
            w.PostWalk(this);
        }
    }


    public class AndExpr : Expr {
        private readonly Expr left, right;

        public AndExpr(Expr left, Expr right) {
            this.left = left;
            this.right = right;
            this.Start = left.Start;
            this.End = right.End;
        }

        public Expr Right {
            get { return right; }
        }

        public Expr Left {
            get { return left; }
        }         

        internal override object Evaluate(NameEnv env) {
            object ret = left.Evaluate(env);
            if (Ops.IsTrue(ret)) return right.Evaluate(env);
            else return ret;
        }

        internal override void Emit(CodeGen cg) {
            left.Emit(cg);
            cg.Emit(OpCodes.Dup);
            cg.EmitCall(typeof(Ops), "IsTrue");
            //cg.emitNonzero(left);
            Label l = cg.DefineLabel();
            cg.Emit(OpCodes.Brfalse, l);
            cg.Emit(OpCodes.Pop);
            right.Emit(cg);
            cg.MarkLabel(l);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                left.Walk(w);
                right.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class OrExpr : Expr {
        private readonly Expr left, right;

        public OrExpr(Expr left, Expr right) {
            this.left = left; this.right = right;
            this.Start = left.Start; this.End = right.End;
        }

        public Expr Right {
            get { return right; }
        }

        public Expr Left {
            get { return left; }
        } 
      
        internal override object Evaluate(NameEnv env) {
            object ret = left.Evaluate(env);
            if (!Ops.IsTrue(ret)) return right.Evaluate(env);
            else return ret;
        }

        internal override void Emit(CodeGen cg) {
            left.Emit(cg);
            cg.Emit(OpCodes.Dup);
            cg.EmitCall(typeof(Ops), "IsTrue");
            //cg.emitNonzero(left);
            Label l = cg.DefineLabel();
            cg.Emit(OpCodes.Brtrue, l);
            cg.Emit(OpCodes.Pop);
            right.Emit(cg);
            cg.MarkLabel(l);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                left.Walk(w);
                right.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class UnaryExpr : Expr {
        private readonly Expr expr;
        private readonly UnaryOperator op;

        public UnaryExpr(UnaryOperator op, Expr expr) {
            this.op = op; this.expr = expr;
            this.End = expr.End;
        }

        public Expr Expression {
            get { return expr; }
        }

        public UnaryOperator Operator {
            get { return op; }
        }

        internal override object Evaluate(NameEnv env) {
            return op.Evaluate(expr.Evaluate(env));
        }

        internal override void Emit(CodeGen cg) {
            expr.Emit(cg);
            cg.EmitCall(op.Target.Method);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                expr.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class BinaryExpr : Expr {
        private readonly Expr left, right;
        private readonly BinaryOperator op;

        public BinaryExpr(BinaryOperator op, Expr left, Expr right) {
            this.op = op; this.left = left; this.right = right;
            this.Start = left.Start; this.End = right.End;
        }

        public Expr Right {
            get { return right; }
        }

        public Expr Left {
            get { return left; }
        }

        public BinaryOperator Operator {
            get { return op; }
        } 

        internal override object Evaluate(NameEnv env) {
            object l = left.Evaluate(env);
            object r = right.Evaluate(env);

            return op.Evaluate(l, r);
        }

        internal override void Emit(CodeGen cg) {
            left.Emit(cg);
            if (IsComparision() && IsComparision(right)) {
                FinishCompare(cg);
            } else {
                right.Emit(cg);
                op.Emit(cg);
            }
        }

        protected bool IsComparision() {
            return op.IsComparision();
        }

        public static bool IsComparision(Expr e) {
            BinaryExpr be = e as BinaryExpr;
            return be != null && be.IsComparision();
        }

        internal void FinishCompare(CodeGen cg) {
            BinaryExpr bright = (BinaryExpr)right;

            Slot valTmp = cg.GetLocalTmp(typeof(object));
            Slot retTmp = cg.GetLocalTmp(typeof(object));
            bright.left.Emit(cg);
            cg.Emit(OpCodes.Dup);
            valTmp.EmitSet(cg);

            cg.EmitCall(op.Target.Method);
            cg.Emit(OpCodes.Dup);
            retTmp.EmitSet(cg);
            cg.EmitTestTrue();

            Label end = cg.DefineLabel();
            cg.Emit(OpCodes.Brfalse, end);

            valTmp.EmitGet(cg);

            if (IsComparision(bright.right)) {
                bright.FinishCompare(cg);
            } else {
                bright.right.Emit(cg);
                cg.EmitCall(bright.op.Target.Method);
            }

            retTmp.EmitSet(cg);
            cg.MarkLabel(end);
            retTmp.EmitGet(cg);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                left.Walk(w);
                right.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class LambdaExpr : Expr {
        private readonly FuncDef func;

        public LambdaExpr(FuncDef func) {
            this.func = func;
        }

        public FuncDef Function {
            get { return func; }
        }

        internal override object Evaluate(NameEnv env) {
            return func.MakeFunction(env);
        }

        internal override void Emit(CodeGen cg) {
            func.Emit(cg);
            cg.EmitGet(func.Name, false);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                func.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public abstract class ListCompIter : Node {
    }

    public class ListCompFor : ListCompIter {
        private readonly Expr lhs, list;

        public ListCompFor(Expr lhs, Expr list) {
            this.lhs = lhs; this.list = list;
        }

        public Expr List {
            get { return list; }
        }

        public Expr Left {
            get { return lhs; }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                lhs.Walk(w);
                list.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class ListCompIf : ListCompIter {
        private readonly Expr test;

        public ListCompIf(Expr test) {
            this.test = test;
        }

        public Expr Test {
            get { return test; }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                test.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class ListComp : Expr {
        private readonly Expr item;
        private readonly ListCompIter[] iters;

        public ListComp(Expr item, ListCompIter[] citers) {
            this.item = item; this.iters = citers;
        }

        public Expr Item {
            get { return item; }
        }

        public IList<ListCompIter> Iterators {
            get { return iters; }
        }

        internal override void Emit(CodeGen cg) {
            Slot list = cg.GetLocalTmp(typeof(List));
            cg.EmitCall(typeof(Ops), "MakeList", Type.EmptyTypes);
            list.EmitSet(cg);

            // first loop: how many For; initialize labels/slots
            int iFors = 0;
            foreach (ListCompIter iter in iters) {
                if (iter is ListCompFor) iFors++;
            }

            Label[] continueTargets = new Label[iFors];
            Slot[] enumerators = new Slot[iFors];
            int jIters = iters.Length;
            Label[] exitTargets = new Label[jIters];

            for (int i = 0; i < iFors; i++) {
                continueTargets[i] = cg.DefineLabel();
                enumerators[i] = cg.GetLocalTmp(typeof(IEnumerator));
            }
            for (int i = 0; i < jIters; i++) {
                exitTargets[i] = cg.DefineLabel();
            }

            // second loop: before emiting item
            iFors = jIters = 0;
            foreach (ListCompIter iter in iters) {
                if (iter is ListCompFor) {
                    ListCompFor cfor = iter as ListCompFor;
                    cfor.List.Emit(cg);
                    cg.EmitCall(typeof(Ops), "GetEnumerator");
                    enumerators[iFors].EmitSet(cg);

                    cg.MarkLabel(continueTargets[iFors]);

                    enumerators[iFors].EmitGet(cg);
                    cg.EmitCall(typeof(IEnumerator), "MoveNext", Type.EmptyTypes);
                    cg.Emit(OpCodes.Brfalse, exitTargets[jIters]);

                    enumerators[iFors].EmitGet(cg);
                    cg.EmitCall(typeof(IEnumerator).GetProperty("Current").GetGetMethod());

                    cfor.Left.EmitSet(cg);
                    iFors++;
                } else if (iter is ListCompIf) {
                    ListCompIf cif = iter as ListCompIf;

                    cg.EmitTestTrue(cif.Test);
                    cg.Emit(OpCodes.Brfalse, exitTargets[jIters]);
                }

                jIters++;
            }

            // append the item
            list.EmitGet(cg);
            this.item.Emit(cg);
            cg.EmitCall(typeof(List), "Append");

            // third loop: in reverse order
            iFors = continueTargets.Length - 1;
            jIters = iters.Length - 1;
            while (jIters >= 0) {
                ListCompIter iter = iters[jIters];
                if (iter is ListCompFor) {
                    cg.Emit(OpCodes.Br, continueTargets[iFors]);
                    cg.FreeLocalTmp(enumerators[iFors]);
                    iFors--;
                }

                cg.MarkLabel(exitTargets[jIters]);
                jIters--;
            }

            list.EmitGet(cg);
            cg.FreeLocalTmp(list);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (ListCompIter iter in iters) iter.Walk(w);

                item.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class GenExpr : Expr {
        private readonly FuncDef func;
        private readonly CallExpr call;

        public GenExpr(FuncDef func, CallExpr call) {
            this.func = func;
            this.call = call;
        }

        public FuncDef Function {
            get { return func; }
        }

        public CallExpr Call {
            get { return call; }
        }

        internal override void Emit(CodeGen cg) {
            func.Emit(cg);
            call.Emit(cg);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                func.Walk(w);
                call.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class CondExpr : Expr {
        private readonly Expr testExpr;
        private readonly Expr trueExpr;
        private readonly Expr falseExpr;


        public CondExpr(Expr testExpr, Expr trueExpr, Expr falseExpr) {
            this.testExpr = testExpr;
            this.trueExpr = trueExpr;
            this.falseExpr = falseExpr;
        }

        public Expr FalseExpression {
            get { return falseExpr; }
        }

        public Expr Test {
            get { return testExpr; }
        }

        public Expr TrueExpression {
            get { return trueExpr; }
        }

        internal override object Evaluate(NameEnv env) {
            object ret = testExpr.Evaluate(env);
            if (Ops.IsTrue(ret))
                return trueExpr.Evaluate(env);
            else
                return falseExpr.Evaluate(env);
        }

        internal override void Emit(CodeGen cg) {
            Label eoi = cg.DefineLabel();
            Label next = cg.DefineLabel();
            cg.EmitTestTrue(testExpr);
            cg.Emit(OpCodes.Brfalse, next);
            trueExpr.Emit(cg);
            cg.Emit(OpCodes.Br, eoi);
            cg.MarkLabel(next);
            falseExpr.Emit(cg);
            cg.MarkLabel(eoi);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                testExpr.Walk(w);
                trueExpr.Walk(w);
                falseExpr.Walk(w);
            }
            w.PostWalk(this);
        }
    }

}
