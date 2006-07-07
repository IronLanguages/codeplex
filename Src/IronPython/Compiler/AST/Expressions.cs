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

namespace IronPython.Compiler.Ast {
    /// <summary>
    /// Summary description for Expr.
    /// </summary>
    public abstract class Expression : Node {
        internal virtual object Evaluate(NameEnvironment environment) {
            throw new NotImplementedException("Evaluate: " + this);
        }

        internal virtual void Assign(object val, NameEnvironment environment) {
            throw new NotImplementedException("Assign: " + this);
        }

        internal abstract void Emit(CodeGen cg);

        internal virtual void EmitSet(CodeGen cg) {
            cg.Context.AddError("can't assign to " + this.GetType().Name, this);
        }

        internal virtual void EmitDel(CodeGen cg) {
            cg.Context.AddError("can't perform deletion", this);
        }

        internal static object[] Evaluate(IList<Expression> items, NameEnvironment environment) {
            object[] ret = new object[items.Count];
            for (int i = 0; i < items.Count; i++) {
                ret[i] = items[i].Evaluate(environment);
            }
            return ret;
        }
    }

    public class ErrorExpression : Expression {
        public override void Walk(IAstWalker walker) {
        }

        internal override void Emit(CodeGen cg) {
            cg.Context.AddError("can't generate error expression", this);
        }
    }

    public class CallExpression : Expression {
        private readonly Expression target;
        private readonly Arg[] args;
        private bool hasArgsTuple, hasKeywordDict;
        private int keywordCount, extraArgs;

        public CallExpression(Expression target, Arg[] args, bool hasArgsTuple, bool hasKeywordDictionary, int keywordCount, int extraArgs) {
            this.target = target;
            this.args = args;
            this.hasArgsTuple = hasArgsTuple;
            this.hasKeywordDict = hasKeywordDictionary;
            this.keywordCount = keywordCount;
            this.extraArgs = extraArgs;
        }

        public IList<Arg> Args {
            get { return args; }
        }

        public Expression Target {
            get { return target; }
        }

        public bool MightNeedLocalsDictionary() {
            NameExpression nameExpr = target as NameExpression;
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

        internal override object Evaluate(NameEnvironment environment) {
            object callee = target.Evaluate(environment);

            object[] cargs = new object[args.Length];
            int index = 0;
            foreach (Arg arg in args) {
                if (arg.Name != SymbolTable.Empty) throw new NotImplementedException("keywords");
                cargs[index++] = arg.Expression.Evaluate(environment);
            }

            switch (cargs.Length) {
                case 0: return Ops.Call(callee);
                default: return Ops.Call(callee, cargs);
            }
        }

        internal override void Emit(CodeGen cg) {
            Label done = new Label();
            bool emitDone = false;

            Expression[] exprs = new Expression[args.Length - extraArgs];
            Expression argsTuple = null, keywordDict = null;
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
                    foreach (Expression e in exprs) {
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                target.Walk(walker);
                foreach (Arg arg in args) arg.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class Arg : Node {
        private readonly SymbolId name;
        private readonly Expression expr;

        public Arg(Expression expression) : this(SymbolTable.Empty, expression) { }

        public Arg(SymbolId name, Expression expression) {
            this.name = name;
            this.expr = expression;
        }

        public override string ToString() {
            return base.ToString() + ":" + name.ToString();
        }

        public SymbolId Name {
            get { return name; }
        }

        public Expression Expression {
            get { return expr; }
        } 

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                expr.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class FieldExpression : Expression {
        private readonly Expression target;
        private readonly SymbolId name;

        public FieldExpression(Expression target, SymbolId name) {
            this.target = target;
            this.name = name;
        }

        public override string ToString() {
            return base.ToString() + ":" + name.ToString();
        }

        public Expression Target {
            get { return target; }
        }

        public SymbolId Name {
            get { return name; }
        }

        internal override object Evaluate(NameEnvironment environment) {
            object t = target.Evaluate(environment);
            return Ops.GetAttr(environment.Globals, t, SymbolTable.StringToId(name.GetString()));
        }

        internal override void Assign(object val, NameEnvironment environment) {
            object t = target.Evaluate(environment);
            Ops.SetAttr(environment.Globals, t, SymbolTable.StringToId(name.GetString()), val);
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                target.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class IndexExpression : Expression {
        private readonly Expression target;
        private readonly Expression index;

        public IndexExpression(Expression target, Expression index) {
            this.target = target;
            this.index = index;
        }

        public Expression Target {
            get { return target; }
        }

        public Expression Index {
            get { return index; }
        }

        internal override object Evaluate(NameEnvironment environment) {
            object t = target.Evaluate(environment);
            object i = index.Evaluate(environment);
            return Ops.GetIndex(t, i);
        }

        internal override void Assign(object val, NameEnvironment environment) {
            object t = target.Evaluate(environment);
            object i = index.Evaluate(environment);
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                target.Walk(walker);
                index.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public abstract class SequenceExpression : Expression {
        private readonly Expression[] items;

        protected SequenceExpression(params Expression[] items) { this.items = items; }
        protected abstract string EmptySequenceString { get; }

        public IList<Expression> Items {
            get { return items; }
        } 

        internal override void Assign(object val, NameEnvironment environment) {
            // Disallow "[] = l", "[], a = l, l", "[[]] = [l]", etc
            if (items.Length == 0) {
                throw Ops.SyntaxError("can't assign to " + EmptySequenceString, "<unknown>",
                    Start.Line, Start.Column, null, 0, IronPython.Hosting.Severity.Error);
            }

            IEnumerator ie = Ops.GetEnumerator(val);

            int leftCount = items.Length;
            object[] values = new object[leftCount];

            int rightCount = Ops.GetEnumeratorValues(ie, ref values);
            if (leftCount != rightCount)
                throw Ops.ValueErrorForUnpackMismatch(leftCount, rightCount);

            for (int i = 0; i < leftCount; i++)
                items[i].Assign(values[i], environment);
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
            cg.EmitCall(typeof(Ops), "GetEnumeratorForUnpack");
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
            foreach (Expression expr in items) {
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
            foreach (Expression expr in items) {
                expr.EmitDel(cg);
            }
        }
    }

    public class TupleExpression : SequenceExpression {
        private bool expandable;

        public TupleExpression(params Expression[] items)
            : this(false, items) {
        }
        public TupleExpression(bool expandable, params Expression[] items)
            : base(items) {
            this.expandable = expandable;
        }

        protected override string EmptySequenceString { get { return "()"; } }

        internal override object Evaluate(NameEnvironment environment) {
            return Ops.MakeTuple(Evaluate(Items, environment));
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitObjectArray(Items);
            cg.EmitCall(typeof(Ops), expandable ? "MakeExpandableTuple" : "MakeTuple", new Type[] { typeof(object[]) });
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                foreach (Expression e in Items) e.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class ListExpression : SequenceExpression {
        public ListExpression(params Expression[] items) : base(items) { }

        protected override string EmptySequenceString { get { return "[]"; } }

        internal override object Evaluate(NameEnvironment environment) {
            return Ops.MakeList(Evaluate(Items, environment));
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitObjectArray(Items);
            cg.EmitCall(typeof(Ops), "MakeList", new Type[] { typeof(object[]) });
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                foreach (Expression e in Items) e.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class DictionaryExpression : Expression {
        private readonly SliceExpression[] items;

        public DictionaryExpression(params SliceExpression[] items) { this.items = items; }

        public IList<SliceExpression> Items {
            get { return items; }
        }

        internal override object Evaluate(NameEnvironment environment) {
            IDictionary<object, object> dict = Ops.MakeDict(items.Length);
            foreach (SliceExpression s in items) {
                dict[s.SliceStart.Evaluate(environment)] = s.SliceStop.Evaluate(environment);
            }
            return dict;
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitInt(items.Length);
            cg.EmitCall(typeof(Ops), "MakeDict");
            foreach (SliceExpression s in items) {
                cg.Emit(OpCodes.Dup);
                s.SliceStart.Emit(cg);
                s.SliceStop.Emit(cg);
                cg.EmitCall(typeof(Dict).GetProperty("Item").GetSetMethod());
            }
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                foreach (SliceExpression e in items) e.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class SliceExpression : Expression {
        private readonly Expression sliceStart, sliceStop, sliceStep;

        public SliceExpression(Expression start, Expression stop, Expression step) {
            this.sliceStart = start;
            this.sliceStop = stop;
            this.sliceStep = step;
        }


        public Expression SliceStep {
            get { return sliceStep; }
        }

        public Expression SliceStop {
            get { return sliceStop; }
        }

        public Expression SliceStart {
            get { return sliceStart; }
        } 

        internal override object Evaluate(NameEnvironment environment) {
            object e1 = sliceStart.Evaluate(environment);
            object e2 = sliceStop.Evaluate(environment);
            object e3 = sliceStep.Evaluate(environment);
            return Ops.MakeSlice(e1, e2, e3);
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitExprOrNone(sliceStart);
            cg.EmitExprOrNone(sliceStop);
            cg.EmitExprOrNone(sliceStep);
            cg.EmitCall(typeof(Ops), "MakeSlice");
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                if (sliceStart != null) sliceStart.Walk(walker);
                if (sliceStop != null) sliceStop.Walk(walker);
                if (sliceStep != null) sliceStep.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class BackQuoteExpression : Expression {
        private readonly Expression expr;

        public BackQuoteExpression(Expression expression) { this.expr = expression; }

        public Expression Expression {
            get { return expr; }
        }

        internal override object Evaluate(NameEnvironment environment) {
            return Ops.Repr(expr.Evaluate(environment));
        }

        internal override void Emit(CodeGen cg) {
            expr.Emit(cg);
            cg.EmitCall(typeof(Ops), "Repr");
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                expr.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class ParenthesisExpression : Expression {
        private readonly Expression expr;

        public ParenthesisExpression(Expression expression) { this.expr = expression; }

        public Expression Expression {
            get { return expr; }
        }

        internal override object Evaluate(NameEnvironment environment) {
            return expr.Evaluate(environment);
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

        internal override void Assign(object val, NameEnvironment environment) {
            expr.Assign(val, environment);
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                expr.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class ConstantExpression : Expression {
        private readonly object value;

        public ConstantExpression(object value) {
            this.value = value;
        }

        public object Value {
            get { return this.value; }
        } 

        internal override object Evaluate(NameEnvironment environment) {
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                ;
            }
            walker.PostWalk(this);
        }
    }

    public class NameExpression : Expression {
        private readonly SymbolId name;
        private bool defined;

        public NameExpression(SymbolId name) { this.name = name; }

        public override string ToString() {
            return base.ToString() + ":" + name.ToString();
        }

        public SymbolId Name {
            get { return name; }
        }

        public bool IsDefined {
            get { return defined; }
            set { defined = value; }
        }

        internal override object Evaluate(NameEnvironment environment) {
            return environment.Get(name.GetString());
        }

        internal override void Assign(object val, NameEnvironment environment) {
            environment.Set(name.GetString(), val);
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                ;
            }
            walker.PostWalk(this);
        }
    }

    public class AndExpression : Expression {
        private readonly Expression left, right;

        public AndExpression(Expression left, Expression right) {
            this.left = left;
            this.right = right;
            this.Start = left.Start;
            this.End = right.End;
        }

        public Expression Right {
            get { return right; }
        }

        public Expression Left {
            get { return left; }
        }         

        internal override object Evaluate(NameEnvironment environment) {
            object ret = left.Evaluate(environment);
            if (Ops.IsTrue(ret)) return right.Evaluate(environment);
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                left.Walk(walker);
                right.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class OrExpression : Expression {
        private readonly Expression left, right;

        public OrExpression(Expression left, Expression right) {
            this.left = left; this.right = right;
            this.Start = left.Start; this.End = right.End;
        }

        public Expression Right {
            get { return right; }
        }

        public Expression Left {
            get { return left; }
        } 
      
        internal override object Evaluate(NameEnvironment environment) {
            object ret = left.Evaluate(environment);
            if (!Ops.IsTrue(ret)) return right.Evaluate(environment);
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                left.Walk(walker);
                right.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class UnaryExpression : Expression {
        private readonly Expression expr;
        private readonly UnaryOperator op;

        public UnaryExpression(UnaryOperator op, Expression expression) {
            this.op = op; this.expr = expression;
            this.End = expression.End;
        }

        public Expression Expression {
            get { return expr; }
        }

        public UnaryOperator Operator {
            get { return op; }
        }

        internal override object Evaluate(NameEnvironment environment) {
            return op.Evaluate(expr.Evaluate(environment));
        }

        internal override void Emit(CodeGen cg) {
            expr.Emit(cg);
            cg.EmitCall(op.Target.Method);
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                expr.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class BinaryExpression : Expression {
        private readonly Expression left, right;
        private readonly BinaryOperator op;

        public BinaryExpression(BinaryOperator op, Expression left, Expression right) {
            this.op = op; this.left = left; this.right = right;
            this.Start = left.Start; this.End = right.End;
        }

        public Expression Right {
            get { return right; }
        }

        public Expression Left {
            get { return left; }
        }

        public BinaryOperator Operator {
            get { return op; }
        } 

        internal override object Evaluate(NameEnvironment environment) {
            object l = left.Evaluate(environment);
            object r = right.Evaluate(environment);

            return op.Evaluate(l, r);
        }

        internal override void Emit(CodeGen cg) {
            left.Emit(cg);
            if (IsComparison() && IsComparison(right)) {
                FinishCompare(cg);
            } else {
                right.Emit(cg);
                op.Emit(cg);
            }
        }

        protected bool IsComparison() {
            return op.IsComparison;
        }

        public static bool IsComparison(Expression expression) {
            BinaryExpression be = expression as BinaryExpression;
            return be != null && be.IsComparison();
        }

        internal void FinishCompare(CodeGen cg) {
            BinaryExpression bright = (BinaryExpression)right;

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

            if (IsComparison(bright.right)) {
                bright.FinishCompare(cg);
            } else {
                bright.right.Emit(cg);
                cg.EmitCall(bright.op.Target.Method);
            }

            retTmp.EmitSet(cg);
            cg.MarkLabel(end);
            retTmp.EmitGet(cg);
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                left.Walk(walker);
                right.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class LambdaExpression : Expression {
        private readonly FunctionDefinition func;

        public LambdaExpression(FunctionDefinition function) {
            this.func = function;
        }

        public FunctionDefinition Function {
            get { return func; }
        }

        internal override object Evaluate(NameEnvironment environment) {
            return func.MakeFunction(environment);
        }

        internal override void Emit(CodeGen cg) {
            func.Emit(cg);
            cg.EmitGet(func.Name, false);
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                func.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public abstract class ListComprehensionIterator : Node {
    }

    public class ListComprehensionFor : ListComprehensionIterator {
        private readonly Expression lhs, list;

        public ListComprehensionFor(Expression lhs, Expression list) {
            this.lhs = lhs; this.list = list;
        }

        public Expression List {
            get { return list; }
        }

        public Expression Left {
            get { return lhs; }
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                lhs.Walk(walker);
                list.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class ListComprehensionIf : ListComprehensionIterator {
        private readonly Expression test;

        public ListComprehensionIf(Expression test) {
            this.test = test;
        }

        public Expression Test {
            get { return test; }
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                test.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class ListComprehension : Expression {
        private readonly Expression item;
        private readonly ListComprehensionIterator[] iters;

        public ListComprehension(Expression item, ListComprehensionIterator[] citers) {
            this.item = item; this.iters = citers;
        }

        public Expression Item {
            get { return item; }
        }

        public IList<ListComprehensionIterator> Iterators {
            get { return iters; }
        }

        internal override void Emit(CodeGen cg) {
            Slot list = cg.GetLocalTmp(typeof(List));
            cg.EmitCall(typeof(Ops), "MakeList", Type.EmptyTypes);
            list.EmitSet(cg);

            // first loop: how many For; initialize labels/slots
            int iFors = 0;
            foreach (ListComprehensionIterator iter in iters) {
                if (iter is ListComprehensionFor) iFors++;
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
            foreach (ListComprehensionIterator iter in iters) {
                if (iter is ListComprehensionFor) {
                    ListComprehensionFor cfor = iter as ListComprehensionFor;
                    cfor.List.Emit(cg);
                    cg.EmitCall(typeof(Ops), "GetEnumeratorForIteration");
                    enumerators[iFors].EmitSet(cg);

                    cg.MarkLabel(continueTargets[iFors]);

                    enumerators[iFors].EmitGet(cg);
                    cg.EmitCall(typeof(IEnumerator), "MoveNext", Type.EmptyTypes);
                    cg.Emit(OpCodes.Brfalse, exitTargets[jIters]);

                    enumerators[iFors].EmitGet(cg);
                    cg.EmitCall(typeof(IEnumerator).GetProperty("Current").GetGetMethod());

                    cfor.Left.EmitSet(cg);
                    iFors++;
                } else if (iter is ListComprehensionIf) {
                    ListComprehensionIf cif = iter as ListComprehensionIf;

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
                ListComprehensionIterator iter = iters[jIters];
                if (iter is ListComprehensionFor) {
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                foreach (ListComprehensionIterator iter in iters) iter.Walk(walker);

                item.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class GeneratorExpression : Expression {
        private readonly FunctionDefinition func;
        private readonly CallExpression call;

        public GeneratorExpression(FunctionDefinition function, CallExpression call) {
            this.func = function;
            this.call = call;
        }

        public FunctionDefinition Function {
            get { return func; }
        }

        public CallExpression Call {
            get { return call; }
        }

        internal override void Emit(CodeGen cg) {
            func.Emit(cg);
            call.Emit(cg);
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                func.Walk(walker);
                call.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public class ConditionalExpression : Expression {
        private readonly Expression testExpr;
        private readonly Expression trueExpr;
        private readonly Expression falseExpr;


        public ConditionalExpression(Expression testExpression, Expression trueExpression, Expression falseExpression) {
            this.testExpr = testExpression;
            this.trueExpr = trueExpression;
            this.falseExpr = falseExpression;
        }

        public Expression FalseExpression {
            get { return falseExpr; }
        }

        public Expression Test {
            get { return testExpr; }
        }

        public Expression TrueExpression {
            get { return trueExpr; }
        }

        internal override object Evaluate(NameEnvironment environment) {
            object ret = testExpr.Evaluate(environment);
            if (Ops.IsTrue(ret))
                return trueExpr.Evaluate(environment);
            else
                return falseExpr.Evaluate(environment);
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

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                testExpr.Walk(walker);
                trueExpr.Walk(walker);
                falseExpr.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

}
