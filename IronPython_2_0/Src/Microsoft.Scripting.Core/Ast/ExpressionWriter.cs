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
using System; using Microsoft;


using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
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

        private const int Tab = 4;
        private const int MaxColumn = 80;

        private TextWriter _out;
        private int _column;

        private Queue<LambdaId> _lambdaIds;
        private int _blockid;
        private Stack<int> _stack = new Stack<int>();
        private int _delta;
        private Flow _flow;

        private ExpressionWriter(TextWriter file) {
            _out = file;
        }

        private int Base {
            get {
                return _stack.Count > 0 ? _stack.Peek() : 0;
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

            if (DebugOptions.ShowTrees) {
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
            } else if (DebugOptions.DumpTrees) {
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
        internal static void Dump<T>(Rule<T> rule, CallSiteBinder binder) where T : class {
            if (DebugOptions.ShowRules) {
#if !SILVERLIGHT
                ConsoleColor color = Console.ForegroundColor;
                try {
                    Console.ForegroundColor = GetAstColor();
#endif
                    ExpressionWriter.Dump(rule.Binding, "Rule for " + binder.ToString(), System.Console.Out);
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

            Visit(node);
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
            string s = String.Format(CultureInfo.CurrentCulture, format, arg0);
            WriteLine(s);
        }
        private void WriteLine(string format, object arg0, object arg1) {
            string s = String.Format(CultureInfo.CurrentCulture, format, arg0, arg1);
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

        protected internal override Expression VisitDynamic(DynamicExpression node) {
            Out(".site", Flow.Space);

            Out("(");
            Out(node.Type.Name);
            Out(")", Flow.Space);

            Out(FormatBinder(node.Binder));
            Out("( // " + node.Binder.ToString());
            Indent();
            NewLine();
            foreach (Expression arg in node.Arguments) {
                Visit(arg);
                NewLine();
            }
            Dedent();
            Out(")");
            return node;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected internal override Expression VisitBinary(BinaryExpression node) {
            if (node.NodeType == ExpressionType.ArrayIndex) {
                Visit(node.Left);
                Out("[");
                Visit(node.Right);
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
                Visit(node.Left);
                Out(Flow.Space, op, Flow.Space | Flow.Break);
                Visit(node.Right);
                Out(Flow.None, ")", Flow.Break);
            }
            return node;
        }

        protected internal override Expression VisitAssignment(AssignmentExpression node) {
            Visit(node.Expression);
            Out(" = ");
            Visit(node.Value);
            return node;
        }

        protected internal override Expression VisitParameter(ParameterExpression node) {
            Out(".prm", Flow.Space);
            Out(node.Name ?? "");
            Out(")");
            return node;
        }

        protected internal override Expression VisitLambda(LambdaExpression node) {
            int id = Enqueue(node);
            Out(
                String.Format(CultureInfo.CurrentCulture, 
                    "{0} ({1} {2} #{3})",
                    ".lambda",
                    node.Name,
                    node.Type,
                    id
                )
            );
            return node;
        }

        protected internal override Expression VisitConditional(ConditionalExpression node) {
            Out(".if (", Flow.Break);
            Visit(node.Test);
            Out(" ) {", Flow.Break);
            Visit(node.IfTrue);
            Out(Flow.Break, "} .else {", Flow.Break);
            Visit(node.IfFalse);
            Out("}", Flow.Break);
            return node;
        }

        private static string Constant(object value) {
            if (value == null) {
                return ".null";
            }

            ITemplatedValue itv = value as ITemplatedValue;
            if (itv != null) {
                return ".template" + itv.Index.ToString(CultureInfo.CurrentCulture) + " (" + itv.ObjectValue.ToString() + ")";
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
                return String.Format(CultureInfo.CurrentCulture, "{0:G}", value);
            }
            return "(" + value.GetType().Name + ")" + value.ToString();
        }

        protected internal override Expression VisitConstant(ConstantExpression node) {
            Out(Constant(node.Value));
            return node;
        }

        protected internal override Expression VisitRuntimeVariables(LocalScopeExpression node) {
            Out(".localScope (", Flow.NewLine);
            Indent();
            foreach (Expression v in node.Variables) {
                Out(Flow.NewLine, "");
                Visit(v);
            }
            Dedent();
            Out(Flow.NewLine, ")", Flow.NewLine);
            return node;
        }

        // Prints ".instanceField" or "declaringType.staticField"
        private void OutMember(Expression instance, MemberInfo member) {
            if (instance != null) {
                Visit(instance);
                Out("." + member.Name);
            } else {
                // For static members, include the type name
                Out(member.DeclaringType.Name + "." + member.Name);
            }
        }

        protected internal override Expression VisitMemberAccess(MemberExpression node) {
            OutMember(node.Expression, node.Member);
            return node;
        }

        protected internal override Expression VisitInvocation(InvocationExpression node) {
            Out("(");
            Visit(node.Expression);
            Out(").Invoke(");
            if (node.Arguments != null && node.Arguments.Count > 0) {
                NewLine(); Indent();
                foreach (Expression e in node.Arguments) {
                    Visit(e);
                    Out(",", Flow.NewLine);
                }
                Dedent();
            }
            Out(")");
            return node;
        }

        protected internal override Expression VisitMethodCall(MethodCallExpression node) {
            if (node.Object != null) {
                Out("(");
                Visit(node.Object);
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
                    Visit(e);
                    Out(",", Flow.NewLine);
                }
                Dedent();
            }
            Out(")");
            return node;
        }

        protected internal override Expression VisitNewArray(NewArrayExpression node) {
            if (node.NodeType == ExpressionType.NewArrayBounds) {
                // .new MyType[expr1, expr2]
                Out(".new " + node.Type.GetElementType().Name + "[");
                if (node.Expressions != null && node.Expressions.Count > 0) {
                    NewLine(); Indent();
                    foreach (Expression e in node.Expressions) {
                        Visit(e);
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
                        Visit(e);
                        Out(",", Flow.NewLine);
                    }
                    Dedent();
                }
                Out("}");
            }
            return node;
        }

        protected internal override Expression VisitNew(NewExpression node) {
            Out(".new " + node.Type.Name + "(");
            if (node.Arguments != null && node.Arguments.Count > 0) {
                NewLine(); Indent();
                foreach (Expression e in node.Arguments) {
                    Visit(e);
                    Out(",", Flow.NewLine);
                }
                Dedent();
            }
            Out(")");
            return node;
        }

        protected internal override Expression VisitTypeBinary(TypeBinaryExpression node) {
            Visit(node.Expression);
            Out(Flow.Space, ".is", Flow.Space);
            Out(node.TypeOperand.Name);
            return node;
        }

        protected internal override Expression VisitUnary(UnaryExpression node) {
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
                case ExpressionType.ArrayLength:
                    break;
                case ExpressionType.Quote:
                    Out("'");
                    break;
            }

            Visit(node.Operand);

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

        protected internal override Expression VisitBlock(Block node) {
            Out(node.Type != typeof(void) ? ".comma {" : "{");
            NewLine(); Indent();
            foreach (Expression s in node.Expressions) {
                Visit(s);
                NewLine();
            }
            Dedent();
            Out("}", Flow.NewLine);
            return node;
        }

        protected internal override Expression VisitDoWhile(DoStatement node) {
            Out(".do", Flow.Space);
            if (node.BreakLabel != null) {
                Out("break:");
                DumpLabel(node.BreakLabel);
                Out(Flow.Space, "");
            }
            if (node.ContinueLabel != null) {
                Out("continue:");
                DumpLabel(node.ContinueLabel);
                Out(Flow.Space, "");
            }
            Out("{", Flow.NewLine);
            Indent();
            Visit(node.Body);
            Dedent();
            Out(Flow.NewLine, "} .while (");
            Visit(node.Test);
            Out(");");
            return node;
        }

        protected internal override Expression VisitEmpty(EmptyStatement node) {
            Out("/*empty*/;", Flow.NewLine);
            return node;
        }

        protected internal override Expression VisitLabel(LabelExpression node) {
            Out(".label", Flow.Space);
            DumpLabel(node.Label);
            Out(Flow.Space, "(");
            Visit(node.DefaultValue);
            Out(")", Flow.Space);
            return node;
        }

        protected internal override Expression VisitGoto(GotoExpression node) {
            Out(".goto", Flow.Space);
            DumpLabel(node.Target);
            Out(Flow.Space, "(");
            Visit(node.Value);
            Out(")", Flow.Space);
            return node;
        }

        protected internal override Expression VisitLoop(LoopStatement node) {
            Out(".for", Flow.Space);
            if (node.BreakLabel != null) {
                Out("break:");
                DumpLabel(node.BreakLabel);
                Out(Flow.Space, "");
            }
            if (node.ContinueLabel != null) {
                Out("continue:");
                DumpLabel(node.ContinueLabel);
                Out(Flow.Space, "");
            }
            Out("(; ");
            Visit(node.Test);
            Out("; ");
            Visit(node.Increment);
            Out(") {", Flow.NewLine);
            Indent();
            Visit(node.Body);
            Dedent();
            Out(Flow.NewLine, "}"); return node;
        }

        protected internal override Expression VisitReturn(ReturnStatement node) {
            Out(".return", Flow.Space);
            Visit(node.Expression);
            Out(";", Flow.NewLine);
            return node;
        }

        protected internal override Expression VisitScope(ScopeExpression node) {
            Out(".scope", Flow.Space);
            if (node.Name != null) {
                Out(node.Name, Flow.Space);
            }
            Out("(", Flow.NewLine);
            Indent();
            foreach (ParameterExpression v in node.Variables) {
                Out(Flow.NewLine, "");
                Out(v.Type.ToString(), Flow.Space);
                Out(v.Name ?? "");
            }
            Dedent();
            Out(Flow.NewLine, ") {", Flow.NewLine);
            Indent();
            Visit(node.Body);
            Dedent();
            Out("}", Flow.NewLine);
            return node;
        }

        protected override SwitchCase VisitSwitchCase(SwitchCase node) {
            if (node.IsDefault) {
                Out(".default");
            } else {
                Out(".case " + node.Value);
            }
            Out(":", Flow.NewLine);
            Indent(); Indent();
            Visit(node.Body);
            Dedent(); Dedent();
            NewLine();
            return node;
        }

        protected internal override Expression VisitSwitch(SwitchStatement node) {
            Out(".switch ");
            if (node.Label != null) {
                DumpLabel(node.Label);
                Out(" ");
            }
            Out("(");
            Visit(node.TestValue);
            Out(") {", Flow.NewLine);
            Visit(node.Cases, VisitSwitchCase);
            Out("}", Flow.NewLine);
            return node;
        }

        protected internal override Expression VisitThrow(ThrowStatement node) {
            Out(Flow.NewLine, ".throw (");
            Visit(node.Value);
            Out(")", Flow.NewLine);
            return node;
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node) {
            Out("} .catch ( " + node.Test.Name);
            if (node.Variable != null) {
                Out(Flow.Space, node.Variable.Name ?? "");
            }
            if (node.Filter != null) {
                Out(") if (", Flow.Break);
                Visit(node.Filter);
            }
            Out(") {", Flow.NewLine);
            Indent();
            Visit(node.Body);
            Dedent();
            return node;
        }

        protected internal override Expression VisitTry(TryStatement node) {
            Out(".try {", Flow.NewLine);
            Indent();
            Visit(node.Body);
            Dedent();
            Visit(node.Handlers, VisitCatchBlock);
            if (node.Finally != null) {
                Out("} .finally {", Flow.NewLine);
                Indent();
                Visit(node.Finally);
                Dedent();
            } else if (node.Fault != null) {
                Out("} .fault {", Flow.NewLine);
                Indent();
                Visit(node.Fault);
                Dedent();
            }

            Out("}", Flow.NewLine);
            return node;
        }

        protected internal override Expression VisitIndex(IndexExpression node) {
            if (node.Indexer != null) {
                OutMember(node.Object, node.Indexer);
            } else {
                Visit(node.Object);
                Out(".");
            }

            Out("[");
            if (node.Arguments != null && node.Arguments.Count > 0) {
                NewLine(); Indent();
                foreach (Expression e in node.Arguments) {
                    Visit(e);
                    Out(",", Flow.NewLine);
                }
                Dedent();
            }
            Out("]");
            return node;
        }

        protected internal override Expression VisitExtension(Expression node) {
            Out(".extension", Flow.Space);

            Out(node.GetType().Name, Flow.Space);
            Out("(", Flow.Space);
            // walk it
            base.VisitExtension(node);
            Out(")", Flow.NewLine);
            return node;
        }

        private static string GetLambdaInfo(LambdaExpression lambda) {            
            return String.Format(CultureInfo.CurrentCulture, ".lambda {0} {1} ()", lambda.ReturnType, lambda.Name);
        }

        private void DumpLabel(LabelTarget target) {
            if (string.IsNullOrEmpty(target.Name)) {
                Out(String.Format(CultureInfo.CurrentCulture, "(.label 0x{0:x8})", target.GetHashCode()));
            } else {
                Out(String.Format(CultureInfo.CurrentCulture, "(.label '{0}')", target.Name));
            }
        }

        private void DumpLambda(LambdaExpression node) {
            Out(GetLambdaInfo(node));
            Out("(");
            Indent();
            foreach (ParameterExpression p in node.Parameters) {
                Out(Flow.NewLine, "");
                Out((p.IsByRef ? p.Type.MakeByRefType() : p.Type).ToString(), Flow.Space);
                Out(p.Name ?? "");
            }
            Dedent();
            Out(Flow.NewLine, ") {", Flow.NewLine);
            Indent();
            Visit(node.Body);
            Dedent();
            Out(Flow.NewLine, "}");
        }

        #endregion
    }
}
