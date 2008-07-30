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

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Utils;

namespace System.Linq.Expressions {
    // TODO: debug builds only!
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal sealed class ExpressionWriter : ExpressionTreeVisitor {
        [Flags]
        private enum Flow {
            None,
            Space,
            NewLine,

            Break = 0x8000      // newline if column > MaxColumn
        };

        private struct LambdaId {
            private readonly LambdaExpression _lambda;
            private readonly int _id;

            internal LambdaId(LambdaExpression lambda, int id) {
                _lambda = lambda;
                _id = id;
            }

            internal LambdaExpression Lambda {
                get { return _lambda; }
            }
            internal int Id {
                get { return _id; }
            }
        }

        private struct Alignment {
            private readonly Expression _expression;
            private readonly int _depth;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal Alignment(Expression expression, int depth) {
                _expression = expression;
                _depth = depth;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal Expression Statement {
                get { return _expression; }
            }
            internal int Depth {
                get { return _depth; }
            }
        }

        private const int Tab = 4;
        private const int MaxColumn = 80;

        private TextWriter _out;
        private int _column;

        private Queue<LambdaId> _lambdaIds;
        private int _blockid;
        private Stack<Alignment> _stack = new Stack<Alignment>();
        private int _delta;
        private Flow _flow;

        private ExpressionWriter(TextWriter file) {
            _out = file;
        }

        private int Base {
            get {
                return _stack.Count > 0 ? _stack.Peek().Depth : 0;
            }
        }

        private int Delta {
            get { return _delta; }
        }

        private int Depth {
            get { return Base + Delta; }
        }

        private void Indent() {
            _delta += Tab;
        }
        private void Dedent() {
            _delta -= Tab;
        }

        private void NewLine() {
            _flow = Flow.NewLine;
        }

#if !SILVERLIGHT
        private static ConsoleColor GetAstColor() {
            if (Console.BackgroundColor == ConsoleColor.White) {
                return ConsoleColor.DarkCyan;
            } else {
                return ConsoleColor.Cyan;
            }
        }
#endif

        /// <summary>
        /// Write out the given AST (only if ShowTrees or DumpTrees is enabled)
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Dump(Expression expression, string description) {
            Debug.Assert(expression != null);

            if (GlobalDlrOptions.ShowTrees) {
#if !SILVERLIGHT
                ConsoleColor color = Console.ForegroundColor;
                try {
                    Console.ForegroundColor = GetAstColor();
#endif
                    Dump(expression, description, System.Console.Out);
#if !SILVERLIGHT
                } finally {
                    Console.ForegroundColor = color;
                }
#endif
            } else if (GlobalDlrOptions.DumpTrees) {
                StreamWriter sw = new StreamWriter(GetFilePath(description), true);
                using (sw) {
                    Dump(expression, description, sw);
                }
            }
        }

        /// <summary>
        /// Write out the given rule's AST (only if ShowRules is enabled)
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Dump<T>(Rule<T> rule) where T : class {
            if (GlobalDlrOptions.ShowRules) {
#if !SILVERLIGHT
                ConsoleColor color = Console.ForegroundColor;
                try {
                    Console.ForegroundColor = GetAstColor();
#endif
                    ExpressionWriter.Dump(rule.Binding, "Rule", System.Console.Out);
#if !SILVERLIGHT
                } finally {
                    Console.ForegroundColor = color;
                }
#endif
            }
        }

        /// <summary>
        /// Write out the given AST
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Dump(Expression node, string descr, TextWriter writer) {
            Debug.Assert(node != null);
            Debug.Assert(descr != null);
            Debug.Assert(writer != null);

            ExpressionWriter dv = new ExpressionWriter(writer);
            dv.DoDump(node, descr);
        }

        private static string GetFilePath(string path) {
            Debug.Assert(path != null);

#if !SILVERLIGHT // GetInvalidFileNameChars does not exist in CoreCLR
            char[] invalid = System.IO.Path.GetInvalidFileNameChars();
            foreach (char ch in invalid) {
                path = path.Replace(ch, '_');
            }
#endif
            return path + ".ast";
        }

        private void DoDump(Expression node, string description) {
            WritePrologue(description);

            VisitNode(node);
            WriteLine();

            WriteLambdas();
            WriteLine();
        }

        private void WritePrologue(string name) {
            WriteLine("//");
            WriteLine("// AST: {0}", name);
            WriteLine("//");
            WriteLine();
        }

        private void WriteLambdas() {
            Debug.Assert(_stack.Count == 0);

            while (_lambdaIds != null && _lambdaIds.Count > 0) {
                LambdaId b = _lambdaIds.Dequeue();
                WriteLine();
                WriteLine("//");
                WriteLine("// LAMBDA: {0}({1})", b.Lambda.Name, b.Id);
                WriteLine("//");
                DumpLambda(b.Lambda);
                WriteLine();

                Debug.Assert(_stack.Count == 0);
            }
        }

        private int Enqueue(LambdaExpression lambda) {
            if (_lambdaIds == null) {
                _lambdaIds = new Queue<LambdaId>();
            }
            _lambdaIds.Enqueue(new LambdaId(lambda, ++_blockid));
            return _blockid;
        }

        #region The printing code

        private void Out(string s) {
            Out(Flow.None, s, Flow.None);
        }

        private void Out(Flow before, string s) {
            Out(before, s, Flow.None);
        }

        private void Out(string s, Flow after) {
            Out(Flow.None, s, after);
        }

        private void Out(Flow before, string s, Flow after) {
            switch (GetFlow(before)) {
                case Flow.None:
                    break;
                case Flow.Space:
                    Write(" ");
                    break;
                case Flow.NewLine:
                    WriteLine();
                    Write(new String(' ', Depth));
                    break;
            }
            Write(s);
            _flow = after;
        }

        private void WriteLine() {
            _out.WriteLine();
            _column = 0;
        }
        private void WriteLine(string s) {
            _out.WriteLine(s);
            _column = 0;
        }
        private void WriteLine(string format, object arg0) {
            string s = String.Format(format, arg0);
            WriteLine(s);
        }
        private void WriteLine(string format, object arg0, object arg1) {
            string s = String.Format(format, arg0, arg1);
            WriteLine(s);
        }
        private void Write(string s) {
            _out.Write(s);
            _column += s.Length;
        }

        private Flow GetFlow(Flow flow) {
            Flow last;

            last = CheckBreak(_flow);
            flow = CheckBreak(flow);

            // Get the biggest flow that is requested None < Space < NewLine
            return (Flow)System.Math.Max((int)last, (int)flow);
        }

        private Flow CheckBreak(Flow flow) {
            if ((flow & Flow.Break) != 0) {
                if (_column > (MaxColumn + Depth)) {
                    flow = Flow.NewLine;
                } else {
                    flow &= ~Flow.Break;
                }
            }
            return flow;
        }

        #endregion

        #region The AST Output

        // More proper would be to make this a virtual method on Action
        private static string FormatBinder(CallSiteBinder binder) {
            var action = binder as StandardAction;
            if (action == null) {
                return "CallSiteBinder (" + binder.ToString() + ")";
            }

            switch (action.Kind) {
                case StandardActionKind.Call:
                    return "Call " + ((CallAction)action).Name;
                case StandardActionKind.Convert:
                    return "Convert " + ((ConvertAction)action).ToType;
                case StandardActionKind.Create:
                    return "Create";
                case StandardActionKind.DeleteMember:
                    return "DeleteMember " + ((DeleteMemberAction)action).Name;
                case StandardActionKind.GetMember:
                    return "GetMember " + ((GetMemberAction)action).Name;
                case StandardActionKind.Invoke:
                    return "Invoke";
                case StandardActionKind.Operation:
                    return "Operation " + ((OperationAction)action).Operation;
                case StandardActionKind.SetMember:
                    return "SetMember " + ((SetMemberAction)action).Name;
                default: throw Assert.Unreachable;
            }
        }

        protected override Expression Visit(ActionExpression node) {
            return WriteCallSite(node, node.Arguments);
        }

        private Expression WriteCallSite(Expression node, params Expression[] arguments) {
            return WriteCallSite(node, (IEnumerable<Expression>)arguments);
        }

        private Expression WriteCallSite(Expression node, IEnumerable<Expression> arguments) {
            Out(".site", Flow.Space);

            Out("(");
            Out(node.Type.Name);
            Out(")", Flow.Space);

            Out(FormatBinder(node.BindingInfo));
            Out("( // " + node.BindingInfo.ToString());
            Indent();
            NewLine();
            foreach (Expression arg in arguments) {
                VisitNode(arg);
                NewLine();
            }
            Dedent();
            Out(")");
            return node;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected override Expression Visit(BinaryExpression node) {
            if (node.IsDynamic) {
                return WriteCallSite(node, node.Left, node.Right);
            }

            if (node.NodeType == ExpressionType.ArrayIndex) {
                VisitNode(node.Left);
                Out("[");
                VisitNode(node.Right);
                Out("]");
            } else {
                string op;
                bool isChecked = false;
                switch (node.NodeType) {
                    case ExpressionType.Equal: op = "=="; break;
                    case ExpressionType.NotEqual: op = "!="; break;
                    case ExpressionType.AndAlso: op = "&&"; break;
                    case ExpressionType.OrElse: op = "||"; break;
                    case ExpressionType.GreaterThan: op = ">"; break;
                    case ExpressionType.LessThan: op = "<"; break;
                    case ExpressionType.GreaterThanOrEqual: op = ">="; break;
                    case ExpressionType.LessThanOrEqual: op = "<="; break;
                    case ExpressionType.Add: op = "+"; break;
                    case ExpressionType.AddChecked: op = "+"; isChecked = true; break;
                    case ExpressionType.Subtract: op = "-"; break;
                    case ExpressionType.SubtractChecked: op = "-"; isChecked = true; break;
                    case ExpressionType.Divide: op = "/"; break;
                    case ExpressionType.Modulo: op = "%"; break;
                    case ExpressionType.Multiply: op = "*"; break;
                    case ExpressionType.MultiplyChecked: op = "*"; isChecked = true; break;
                    case ExpressionType.LeftShift: op = "<<"; break;
                    case ExpressionType.RightShift: op = ">>"; break;
                    case ExpressionType.And: op = "&"; break;
                    case ExpressionType.Or: op = "|"; break;
                    case ExpressionType.ExclusiveOr: op = "^"; break;
                    case ExpressionType.Power: op = "**"; break;
                    //TODO: need to handle conversion lambda
                    case ExpressionType.Coalesce: op = "??"; break;

                    default:
                        throw new InvalidOperationException();
                }
                if (isChecked) {
                    Out(Flow.Break, "checked(", Flow.None);
                } else {
                    Out(Flow.Break, "(", Flow.None);
                }
                VisitNode(node.Left);
                Out(Flow.Space, op, Flow.Space | Flow.Break);
                VisitNode(node.Right);
                Out(Flow.None, ")", Flow.Break);
            }
            return node;
        }

        protected override Expression Visit(AssignmentExpression node) {
            if (node.IsDynamic) {
                if (node.Expression.NodeType == ExpressionType.MemberAccess) {
                    MemberExpression getMember = (MemberExpression)node.Expression;
                    return WriteCallSite(node, getMember.Expression, node.Value);
                } else {
                    BinaryExpression arrayIndex = (BinaryExpression)node.Expression;
                    return WriteCallSite(node, arrayIndex.Left, arrayIndex.Right, node.Value);
                }
            }

            VisitNode(node.Expression);
            Out(" = ");
            VisitNode(node.Value);
            return node;
        }

        protected override Expression Visit(VariableExpression node) {
            Out("(.var", Flow.Space);
            Out(node.Name ?? "");
            Out(")");
            return node;
        }

        protected override Expression Visit(ParameterExpression node) {
            Out("(.arg", Flow.Space);
            Out(node.Name ?? "");
            Out(")");
            return node;
        }

        protected override Expression Visit(LambdaExpression node) {
            int id = Enqueue(node);
            Out(
                String.Format(
                    "{0} ({1} {2} #{3})",
                    node.NodeType == ExpressionType.Lambda ? ".lambda" : ".generator",
                    node.Name,
                    node.Type,
                    id
                )
            );
            return node;
        }

        protected override Expression Visit(ConditionalExpression node) {
            Out(".if (", Flow.Break);
            VisitNode(node.Test);
            Out(" ) {", Flow.Break);
            VisitNode(node.IfTrue);
            Out(Flow.Break, "} .else {", Flow.Break);
            VisitNode(node.IfFalse);
            Out("}", Flow.Break);
            return node;
        }

        private static string Constant(object value) {
            if (value == null) {
                return ".null";
            }

            ITemplatedValue itv = value as ITemplatedValue;
            if (itv != null) {
                return ".template" + itv.Index.ToString() + " (" + itv.ObjectValue.ToString() + ")";
            }

            Type t;
            if ((t = value as Type) != null) {
                return "((Type)" + t.Name + ")";
            }
            string s;
            if ((s = value as string) != null) {
                return "\"" + s + "\"";
            }
            if (value is int || value is double) {
                return String.Format("{0:G}", value);
            }
            return "(" + value.GetType().Name + ")" + value.ToString();
        }

        protected override Expression Visit(ConstantExpression node) {
            Out(Constant(node.Value));
            return node;
        }

        protected override Expression Visit(LocalScopeExpression node) {
            Out(".localScope (", Flow.NewLine);
            Indent();
            foreach (Expression v in node.Variables) {
                Out(Flow.NewLine, "");
                VisitNode(v);
            }
            Dedent();
            Out(Flow.NewLine, ")", Flow.NewLine);
            return node;
        }

        // Prints ".instanceField" or "declaringType.staticField"
        private void OutMember(Expression instance, MemberInfo member) {
            if (instance != null) {
                VisitNode(instance);
                Out("." + member.Name);
            } else {
                // For static members, include the type name
                Out(member.DeclaringType.Name + "." + member.Name);
            }
        }

        protected override Expression Visit(MemberExpression node) {
            if (node.IsDynamic) {
                return WriteCallSite(node, node.Expression);
            }
            OutMember(node.Expression, node.Member);
            return node;
        }

        protected override Expression Visit(InvocationExpression node) {
            if (node.IsDynamic) {
                return WriteCallSite(node, node.Arguments.AddFirst(node.Expression));
            }

            Out("(");
            VisitNode(node.Expression);
            Out(").Invoke(");
            if (node.Arguments != null && node.Arguments.Count > 0) {
                NewLine(); Indent();
                foreach (Expression e in node.Arguments) {
                    VisitNode(e);
                    Out(",", Flow.NewLine);
                }
                Dedent();
            }
            Out(")");
            return node;
        }

        protected override Expression Visit(MethodCallExpression node) {
            if (node.IsDynamic) {
                return WriteCallSite(node, node.Arguments.AddFirst(node.Object));
            }

            if (node.Object != null) {
                Out("(");
                VisitNode(node.Object);
                Out(").");
            }
            if (node.Method.ReflectedType != null) {
                Out("(" + node.Method.ReflectedType.Name + "." + node.Method.Name + ")(");
            } else {
                Out("(" + node.Method.Name + ")(");
            }
            if (node.Arguments != null && node.Arguments.Count > 0) {
                NewLine(); Indent();
                foreach (Expression e in node.Arguments) {
                    VisitNode(e);
                    Out(",", Flow.NewLine);
                }
                Dedent();
            }
            Out(")");
            return node;
        }

        protected override Expression Visit(NewArrayExpression node) {
            if (node.NodeType == ExpressionType.NewArrayBounds) {
                // .new MyType[expr1, expr2]
                Out(".new " + node.Type.GetElementType().Name + "[");
                if (node.Expressions != null && node.Expressions.Count > 0) {
                    NewLine(); Indent();
                    foreach (Expression e in node.Expressions) {
                        VisitNode(e);
                        Out(",", Flow.NewLine);
                    }
                    Dedent();
                }
                Out("]");

            } else {
                // .new MyType = {expr1, expr2}
                Out(".new " + node.Type.Name + " = {");
                if (node.Expressions != null && node.Expressions.Count > 0) {
                    NewLine(); Indent();
                    foreach (Expression e in node.Expressions) {
                        VisitNode(e);
                        Out(",", Flow.NewLine);
                    }
                    Dedent();
                }
                Out("}");
            }
            return node;
        }

        protected override Expression Visit(NewExpression node) {
            if (node.IsDynamic) {
                return WriteCallSite(node, node.Arguments);
            }

            Out(".new " + node.Type.Name + "(");
            if (node.Arguments != null && node.Arguments.Count > 0) {
                NewLine(); Indent();
                foreach (Expression e in node.Arguments) {
                    VisitNode(e);
                    Out(",", Flow.NewLine);
                }
                Dedent();
            }
            Out(")");
            return node;
        }

        protected override Expression Visit(TypeBinaryExpression node) {
            VisitNode(node.Expression);
            Out(Flow.Space, ".is", Flow.Space);
            Out(node.TypeOperand.Name);
            return node;
        }

        protected override Expression Visit(UnaryExpression node) {
            if (node.IsDynamic) {
                return WriteCallSite(node, node.Operand);
            }

            switch (node.NodeType) {
                case ExpressionType.Convert:
                    Out("(" + node.Type.Name + ")");
                    break;
                case ExpressionType.ConvertChecked:
                    Out("checked((" + node.Type.Name + ")");
                    break;
                case ExpressionType.TypeAs:
                    break;
                case ExpressionType.Not:
                    Out(node.Type == typeof(bool) ? "!" : "~");
                    break;
                case ExpressionType.Negate:
                    Out("-");
                    break;
                case ExpressionType.NegateChecked:
                    Out("checked(-");
                    break;
                case ExpressionType.UnaryPlus:
                    Out("+");
                    break;
                case ExpressionType.OnesComplement:
                    Out("~");
                    break;
                case ExpressionType.ArrayLength:
                    break;
                case ExpressionType.Quote:
                    Out("'");
                    break;
            }

            VisitNode(node.Operand);

            switch (node.NodeType) {
                case ExpressionType.ConvertChecked:
                    Out(")");
                    break;
                case ExpressionType.TypeAs:
                    Out(Flow.Space, "as", Flow.Space | Flow.Break);
                    Out(node.Type.Name);
                    break;
                case ExpressionType.NegateChecked:
                    Out(")");
                    break;
                case ExpressionType.ArrayLength:
                    Out(".Length");
                    break;
            }
            return node;
        }

        protected override Expression Visit(Block node) {
            Out(node.Type != typeof(void) ? ".comma {" : "{");
            NewLine(); Indent();
            foreach (Expression s in node.Expressions) {
                VisitNode(s);
                NewLine();
            }
            Dedent();
            Out("}", Flow.NewLine);
            return node;
        }

        protected override Expression Visit(BreakStatement node) {
            Out(".break ");
            DumpLabel(node.Target);
            Out(";", Flow.NewLine);
            return node;
        }

        protected override Expression Visit(ContinueStatement node) {
            Out(".continue ");
            DumpLabel(node.Target);
            Out(";", Flow.NewLine);
            return node;
        }

        protected override Expression Visit(DeleteExpression node) {
            Debug.Assert(node.IsDynamic && node.Expression.NodeType == ExpressionType.MemberAccess);
            return WriteCallSite(node, ((MemberExpression)node.Expression).Expression);
        }

        protected override Expression Visit(DoStatement node) {
            Out(".do ");
            if (node.Label != null) {
                DumpLabel(node.Label);
                Out(" ");
            }
            Out("{", Flow.NewLine);
            Indent();
            VisitNode(node.Body);
            Dedent();
            Out(Flow.NewLine, "} .while (");
            VisitNode(node.Test);
            Out(");");
            return node;
        }

        protected override Expression Visit(EmptyStatement node) {
            Out("/*empty*/;", Flow.NewLine);
            return node;
        }

        protected override Expression Visit(LabeledStatement node) {
            Out(".labeled ");
            DumpLabel(node.Label);
            Out(" {", Flow.NewLine);
            Indent();
            VisitNode(node.Statement);
            Dedent();
            Out(Flow.NewLine, "}");
            return node;
        }

        protected override Expression Visit(LoopStatement node) {
            Out(".for ");
            if (node.Label != null) {
                DumpLabel(node.Label);
                Out(" ");
            }
            Out("(; ");
            VisitNode(node.Test);
            Out("; ");
            VisitNode(node.Increment);
            Out(") {", Flow.NewLine);
            Indent();
            VisitNode(node.Body);
            Dedent();
            Out(Flow.NewLine, "}"); return node;
        }

        protected override Expression Visit(ReturnStatement node) {
            Out(".return", Flow.Space);
            VisitNode(node.Expression);
            Out(";", Flow.NewLine);
            return node;
        }

        protected override Expression Visit(ScopeExpression node) {
            Out(".scope", Flow.Space);
            if (node.Name != null) {
                Out(node.Name, Flow.Space);
            }
            Out("(", Flow.NewLine);
            Indent();
            foreach (VariableExpression v in node.Variables) {
                Out(Flow.NewLine, "");
                Out(v.Type.ToString(), Flow.Space);
                Out(v.Name ?? "");
            }
            Dedent();
            Out(Flow.NewLine, ") {", Flow.NewLine);
            Indent();
            VisitNode(node.Body);
            Dedent();
            Out("}", Flow.NewLine);
            return node;
        }

        protected override SwitchCase Visit(SwitchCase node) {
            if (node.IsDefault) {
                Out(".default");
            } else {
                Out(".case " + node.Value);
            }
            Out(":", Flow.NewLine);
            Indent(); Indent();
            VisitNode(node.Body);
            Dedent(); Dedent();
            NewLine();
            return node;
        }

        protected override Expression Visit(SwitchStatement node) {
            Out(".switch ");
            if (node.Label != null) {
                DumpLabel(node.Label);
                Out(" ");
            }
            Out("(");
            VisitNode(node.TestValue);
            Out(") {", Flow.NewLine);
            VisitNodes(node.Cases, Visit);
            Out("}", Flow.NewLine);
            return node;
        }

        protected override Expression Visit(ThrowStatement node) {
            Out(Flow.NewLine, ".throw (");
            VisitNode(node.Exception);
            Out(")", Flow.NewLine);
            return node;
        }

        protected override CatchBlock Visit(CatchBlock node) {
            Out("} .catch ( " + node.Test.Name);
            if (node.Variable != null) {
                Out(Flow.Space, node.Variable.Name ?? "");
            }
            if (node.Filter != null) {
                Out(") if (", Flow.Break);
                VisitNode(node.Filter);
            }
            Out(") {", Flow.NewLine);
            Indent();
            VisitNode(node.Body);
            Dedent();
            return node;
        }

        protected override Expression Visit(TryStatement node) {
            Out(".try {", Flow.NewLine);
            Indent();
            VisitNode(node.Body);
            Dedent();
            VisitNodes(node.Handlers, Visit);
            if (node.Finally != null) {
                Out("} .finally {", Flow.NewLine);
                Indent();
                VisitNode(node.Finally);
                Dedent();
            } else if (node.Fault != null) {
                Out("} .fault {", Flow.NewLine);
                Indent();
                VisitNode(node.Fault);
                Dedent();
            }

            Out("}", Flow.NewLine);
            return node;
        }

        protected override Expression Visit(YieldStatement node) {
            Out(".yield ");
            VisitNode(node.Expression);
            Out(";", Flow.NewLine);
            return node;
        }

        protected override Expression Visit(IndexedPropertyExpression node) {
            if (node.IsDynamic) {
                return WriteCallSite(node, node.Arguments.AddFirst(node.Object));
            }

            if (node.Object != null) {
                Out("(");
                VisitNode(node.Object);
                Out(").");
            }

            Out("(.property");

            if (node.GetMethod != null) {
                Out(" get=" + node.GetMethod.ReflectedType.Name + "." + node.GetMethod.Name);
            }
            if (node.SetMethod != null) {
                Out(" set=" + node.SetMethod.ReflectedType.Name + "." + node.SetMethod.Name);
            }
            Out(")(");

            if (node.Arguments != null && node.Arguments.Count > 0) {
                NewLine(); Indent();
                foreach (Expression e in node.Arguments) {
                    VisitNode(e);
                    Out(",", Flow.NewLine);
                }
                Dedent();
            }
            Out(")");
            return node;
        }

        protected override Expression VisitExtension(Expression node) {
            Out(".extension (");
            Out(node.Type.Name);
            Out(")", Flow.Space);

            // print the known node (best we can do)
            if (node.IsReducible) {
                VisitNode(node.ReduceToKnown());
            }

            Out(";", Flow.NewLine);
            return node;
        }

        private static string GetLambdaInfo(LambdaExpression lambda) {
            string info = lambda.NodeType == ExpressionType.Generator ? ".generator " : ".lambda ";

            info += String.Format("{0} {1} (", lambda.ReturnType, lambda.Name);
            info += ")";
            return info;
        }

        private void DumpLabel(LabelTarget target) {
            if (string.IsNullOrEmpty(target.Name)) {
                Out(string.Format("(.label 0x{0:x8})", target.GetHashCode()));
            } else {
                Out(string.Format("(.label '{0}')", target.Name));
            }
        }

        private void DumpLambda(LambdaExpression node) {
            Out(GetLambdaInfo(node));
            Out("(");
            Indent();
            foreach (ParameterExpression p in node.Parameters) {
                Out(Flow.NewLine, "");
                Out(p.Type.ToString(), Flow.Space);
                Out(p.Name ?? "");
            }
            Dedent();
            Out(Flow.NewLine, ") {", Flow.NewLine);
            Indent();
            VisitNode(node.Body);
            Dedent();
            Out(Flow.NewLine, "}");
        }

        #endregion
    }
}
