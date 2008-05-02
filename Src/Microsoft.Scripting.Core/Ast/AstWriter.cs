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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

#if DEBUG
namespace Microsoft.Scripting.Ast {
    partial class AstWriter {
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

            internal Alignment(Expression expression, int depth) {
                _expression = expression;
                _depth = depth;
            }

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

        private AstWriter(TextWriter file) {
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
        internal static void Dump(Expression/*!*/ expression, string description) {
            Debug.Assert(expression != null);

            if (ScriptDomainManager.Options.ShowTrees) {
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
            } else if (ScriptDomainManager.Options.DumpTrees) {
                StreamWriter sw = new StreamWriter(GetFilePath(description), true);
                using (sw) {
                    Dump(expression, description, sw);
                }
            }
        }

        /// <summary>
        /// Write out the given rule's AST (only if ShowRules is enabled)
        /// </summary>
        internal static void Dump<T>(Rule<T> rule) {
            if (ScriptDomainManager.Options.ShowRules) {
#if !SILVERLIGHT
                ConsoleColor color = Console.ForegroundColor;
                try {
                    Console.ForegroundColor = GetAstColor();
#endif
                    AstWriter.Dump(rule.Binding, "Rule", System.Console.Out);
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
        internal static void Dump(Expression/*!*/ node, string/*!*/ descr, TextWriter/*!*/ writer) {
            Debug.Assert(node != null);
            Debug.Assert(descr != null);
            Debug.Assert(writer != null);

            AstWriter dv = new AstWriter(writer);
            dv.DoDump(node, descr);
        }

        private static string GetFilePath(string/*!*/ path) {
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

            WalkNode(node);
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

        private void WalkNode(Expression node) {
            if (node == null) {
                return;
            }

            Debug.Assert((int)node.NodeType < _Writers.Length);
            _Writers[(int)node.NodeType](this, node);
        }

        // More proper would be to make this a virtual method on Action
        private static string FormatAction(DynamicAction action) {
            DoOperationAction doa;
            GetMemberAction gma;
            SetMemberAction sma;
            InvokeMemberAction ima;
            ConvertToAction cta;
            CallAction cla;

            if ((doa = action as DoOperationAction) != null) {
                return "Do " + doa.Operation.ToString();
            } else if ((gma = action as GetMemberAction) != null) {
                return "GetMember " + SymbolTable.IdToString(gma.Name);
            } else if ((sma = action as SetMemberAction) != null) {
                return "SetMember " + SymbolTable.IdToString(sma.Name);
            } else if ((ima = action as InvokeMemberAction) != null) {
                return "InvokeMember " + SymbolTable.IdToString(ima.Name);
            } else if ((cta = action as ConvertToAction) != null) {
                return "ConvertTo " + cta.ToType.ToString();
            } else if ((cla = action as CallAction) != null) {
                return "Call";
            } else {
                return "UnknownAction (" + action.Kind.ToString() + ")";
            }
        }

        // ActionExpression
        private static void WriteActionExpression(AstWriter aw, Expression expr) {
            ActionExpression node = (ActionExpression)expr;
            aw.WriteCallSite(node, node.Arguments);
        }

        private void WriteCallSite(Expression node, params Expression[] arguments) {
            WriteCallSite(node, (IEnumerable<Expression>)arguments);
        }

        private void WriteCallSite(Expression node, IEnumerable<Expression> arguments) {
            Out(".site", Flow.Space);

            Out("(");
            Out(node.Type.Name);
            Out(")", Flow.Space);

            Out(FormatAction(node.BindingInfo));
            Out("( // " + node.BindingInfo.ToString());
            Indent();
            NewLine();
            foreach (Expression arg in arguments) {
                WalkNode(arg);
                NewLine();
            }
            Dedent();
            Out(")");
        }
     
        // BinaryExpression
        private static void WriteBinaryExpression(AstWriter aw, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;
            if (node.IsDynamic) {
                aw.WriteCallSite(node, node.Left, node.Right);
                return;
            }

            if (node.NodeType == AstNodeType.ArrayIndex) {
                aw.WalkNode(node.Left);
                aw.Out("[");
                aw.WalkNode(node.Right);
                aw.Out("]");
            } else {
                string op;
                switch (node.NodeType) {
                    case AstNodeType.Equal: op = "=="; break;
                    case AstNodeType.NotEqual: op = "!="; break;
                    case AstNodeType.AndAlso: op = "&&"; break;
                    case AstNodeType.OrElse: op = "||"; break;
                    case AstNodeType.GreaterThan: op = ">"; break;
                    case AstNodeType.LessThan: op = "<"; break;
                    case AstNodeType.GreaterThanOrEqual: op = ">="; break;
                    case AstNodeType.LessThanOrEqual: op = "<="; break;
                    case AstNodeType.Add: op = "+"; break;
                    case AstNodeType.Subtract: op = "-"; break;
                    case AstNodeType.Divide: op = "/"; break;
                    case AstNodeType.Modulo: op = "%"; break;
                    case AstNodeType.Multiply: op = "*"; break;
                    case AstNodeType.LeftShift: op = "<<"; break;
                    case AstNodeType.RightShift: op = ">>"; break;
                    case AstNodeType.And: op = "&"; break;
                    case AstNodeType.Or: op = "|"; break;
                    case AstNodeType.ExclusiveOr: op = "^"; break;
                    default:
                        throw new InvalidOperationException();
                }
                aw.Out(Flow.Break, "(", Flow.None);
                aw.WalkNode(node.Left);
                aw.Out(Flow.Space, op, Flow.Space | Flow.Break);
                aw.WalkNode(node.Right);
                aw.Out(Flow.None, ")", Flow.Break);
            }
        }

        // WriteAssignmentExpression
        private static void WriteAssignmentExpression(AstWriter aw, Expression expr) {
            AssignmentExpression node = (AssignmentExpression)expr;
            if (node.IsDynamic) {
                if (node.Expression.NodeType == AstNodeType.MemberExpression) {
                    MemberExpression getMember = (MemberExpression)node.Expression;
                    aw.WriteCallSite(node, getMember.Expression, node.Value);
                } else {
                    BinaryExpression arrayIndex = (BinaryExpression)node.Expression;
                    aw.WriteCallSite(node, arrayIndex.Left, arrayIndex.Right, node.Value);
                }
                return;
            }

            aw.WalkNode(node.Expression);
            aw.Out(" = ");
            aw.WalkNode(node.Value);
        }

        // VariableExpression
        private static void WriteVariableExpression(AstWriter aw, Expression expr) {
            VariableExpression node = (VariableExpression)expr;
            aw.Out("(.");
            aw.Out(GetVariableKind(node), Flow.Space);
            aw.Out(SymbolTable.IdToString(node.Name));
            aw.Out(")");
        }

        private static string GetVariableKind(VariableExpression v) {
            switch (v.NodeType) {
                case AstNodeType.TemporaryVariable: return "temp";
                case AstNodeType.LocalVariable: return "local";
                case AstNodeType.GlobalVariable: return "global";
                default: throw new InvalidOperationException();
            }
        }

        // ParameterExpression
        private static void WriteParameterExpression(AstWriter aw, Expression expr) {
            ParameterExpression node = (ParameterExpression)expr;
            aw.Out("(.arg ");
            aw.Out(node.Name);
            aw.Out(")");
        }

        // LambdaExpression
        private static void WriteLambdaExpression(AstWriter aw, Expression expr) {
            LambdaExpression node = (LambdaExpression)expr;
            int id = aw.Enqueue(node);
            aw.Out(String.Format(".lambda ({0} {1} #{2})", node.Name, node.Type, id));
        }

        // GeneratorLambdaExpression
        private static void WriteGeneratorLambdaExpression(AstWriter aw, Expression expr) {
            GeneratorLambdaExpression node = (GeneratorLambdaExpression)expr;
            int id = aw.Enqueue(node);
            aw.Out(String.Format(".generator ({0} {1} #{2})", node.Name, node.Type, id));
        }

        // ConditionalExpression
        private static void WriteConditionalExpression(AstWriter aw, Expression expr) {
            ConditionalExpression node = (ConditionalExpression)expr;
            aw.Out(".if (", Flow.Break);
            aw.WalkNode(node.Test);
            aw.Out(" ) {", Flow.Break);
            aw.WalkNode(node.IfTrue);
            aw.Out(Flow.Break, "} .else {", Flow.Break);
            aw.WalkNode(node.IfFalse);
            aw.Out("}", Flow.Break);
        }

        private static string Constant(object value) {
            if (value == null) {
                return ".null";
            }

            CompilerConstant cc;
            if ((cc = value as CompilerConstant) != null) {
                value = cc.Create();
                if (value is ITemplatedValue) {
                    return ".template (" + ((ITemplatedValue)value).ObjectValue.ToString() + ")";
                }
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

        // ConstantExpression
        private static void WriteConstantExpression(AstWriter aw, Expression expr) {
            ConstantExpression node = (ConstantExpression)expr;
            aw.Out(Constant(node.Value));
        }

        // IntrinsicExpression
        private static void WriteIntrinsicExpression(AstWriter aw, Expression expr) {
            switch (expr.NodeType) {
                case AstNodeType.CodeContextExpression:
                    aw.Out(".context");
                    break;
                case AstNodeType.GeneratorIntrinsic:
                    aw.Out(".gen_intrinsic");
                    break;
            }
        }

        // Prints ".instanceField" or "declaringType.staticField"
        private void OutMember(Expression instance, MemberInfo member) {
            if (instance != null) {
                WalkNode(instance);
                Out("." + member.Name);
            } else {
                // For static members, include the type name
                Out(member.DeclaringType.Name + "." + member.Name);
            }
        }

        // MemberExpression
        private static void WriteMemberExpression(AstWriter aw, Expression expr) {
            MemberExpression node = (MemberExpression)expr;
            if (node.IsDynamic) {
                aw.WriteCallSite(node, node.Expression);
                return;
            }
            aw.OutMember(node.Expression, node.Member);
        }

        // InvocationExpression
        private static void WriteInvocationExpression(AstWriter aw, Expression expr) {
            InvocationExpression node = (InvocationExpression)expr;
            if (node.IsDynamic) {
                aw.WriteCallSite(node, ArrayUtils.Insert(node.Expression, node.Arguments));
                return;
            }

            aw.Out("(");
            aw.WalkNode(node.Expression);
            aw.Out(").Invoke(");
            if (node.Arguments != null && node.Arguments.Count > 0) {
                aw.NewLine(); aw.Indent();
                foreach (Expression e in node.Arguments) {
                    aw.WalkNode(e);
                    aw.Out(",", Flow.NewLine);
                }
                aw.Dedent();
            }
            aw.Out(")");
        }

        // MethodCallExpression
        private static void WriteMethodCallExpression(AstWriter aw, Expression expr) {
            MethodCallExpression node = (MethodCallExpression)expr;
            if (node.IsDynamic) {
                aw.WriteCallSite(node, ArrayUtils.Insert(node.Instance, node.Arguments));
                return;
            }

            if (node.Instance != null) {
                aw.Out("(");
                aw.WalkNode(node.Instance);
                aw.Out(").");
            }
            aw.Out("(" + node.Method.ReflectedType.Name + "." + node.Method.Name + ")(");
            if (node.Arguments != null && node.Arguments.Count > 0) {
                aw.NewLine(); aw.Indent();
                foreach (Expression e in node.Arguments) {
                    aw.WalkNode(e);
                    aw.Out(",", Flow.NewLine);
                }
                aw.Dedent();
            }
            aw.Out(")");
        }

        // NewArrayExpression
        private static void WriteNewArrayExpression(AstWriter aw, Expression expr) {
            NewArrayExpression node = (NewArrayExpression)expr;

            if (node.NodeType == AstNodeType.NewArrayBounds) {
                // .new MyType[expr1, expr2]
                aw.Out(".new " + node.Type.GetElementType().Name + "[");
                if (node.Expressions != null && node.Expressions.Count > 0) {
                    aw.NewLine(); aw.Indent();
                    foreach (Expression e in node.Expressions) {
                        aw.WalkNode(e);
                        aw.Out(",", Flow.NewLine);
                    }
                    aw.Dedent();
                }
                aw.Out("]");

            } else {
                // .new MyType = {expr1, expr2}
                aw.Out(".new " + node.Type.Name + " = {");
                if (node.Expressions != null && node.Expressions.Count > 0) {
                    aw.NewLine(); aw.Indent();
                    foreach (Expression e in node.Expressions) {
                        aw.WalkNode(e);
                        aw.Out(",", Flow.NewLine);
                    }
                    aw.Dedent();
                }
                aw.Out("}");
            }
        }

        // NewExpression
        private static void WriteNewExpression(AstWriter aw, Expression expr) {
            NewExpression node = (NewExpression)expr;
            if (node.IsDynamic) {
                aw.WriteCallSite(node, node.Arguments);
                return;
            }

            aw.Out(".new " + node.Type.Name + "(");
            if (node.Arguments != null && node.Arguments.Count > 0) {
                aw.NewLine(); aw.Indent();
                foreach (Expression e in node.Arguments) {
                    aw.WalkNode(e);
                    aw.Out(",", Flow.NewLine);
                }
                aw.Dedent();
            }
            aw.Out(")");
        }

        // TypeBinaryExpression
        private static void WriteTypeBinaryExpression(AstWriter aw, Expression expr) {
            TypeBinaryExpression node = (TypeBinaryExpression)expr;
            aw.WalkNode(node.Expression);
            aw.Out(Flow.Space, ".is", Flow.Space);
            aw.Out(node.TypeOperand.Name);
        }

        // UnaryExpression
        private static void WriteUnaryExpression(AstWriter aw, Expression expr) {
            UnaryExpression node = (UnaryExpression)expr;
            if (node.IsDynamic) {
                aw.WriteCallSite(node, node.Operand);
                return;
            }

            switch (node.NodeType) {
                case AstNodeType.Convert:
                    aw.Out("(" + node.Type.Name + ")");
                    break;
                case AstNodeType.Not:
                    aw.Out(node.Type == typeof(bool) ? "!" : "~");
                    break;
                case AstNodeType.Negate:
                    aw.Out("-");
                    break;
                case AstNodeType.OnesComplement:
                    aw.Out("~");
                    break;
            }

            aw.WalkNode(node.Operand);
        }

        // Block
        private static void WriteBlock(AstWriter aw, Expression expr) {
            Block node = (Block)expr;
            aw.Out(node.Type != typeof(void) ? ".comma {" : "{");
            aw.NewLine(); aw.Indent();
            foreach (Expression s in node.Expressions) {
                aw.WalkNode(s);
                aw.NewLine();
            }
            aw.Dedent();
            aw.Out("}", Flow.NewLine);
        }

        // BreakStatement
        private static void WriteBreakStatement(AstWriter aw, Expression expr) {
            BreakStatement node = (BreakStatement)expr;
            aw.Out(".break ");
            aw.DumpLabel(node.Target);
            aw.Out(";", Flow.NewLine);
        }

        // ContinueStatement
        private static void WriteContinueStatement(AstWriter aw, Expression expr) {
            ContinueStatement node = (ContinueStatement)expr;
            aw.Out(".continue ");
            aw.DumpLabel(node.Target);
            aw.Out(";", Flow.NewLine);
        }

        // DeleteStatement
        private static void WriteDeleteStatement(AstWriter aw, Expression expr) {
            DeleteStatement node = (DeleteStatement)expr;
            if (node.IsDynamic) {
                aw.WriteCallSite(node, node.Variable);
                return;
            }

            aw.Out(".del");
            aw.Out(Flow.Space, SymbolTable.IdToString(VariableInfo.GetName(node.Variable)));
            aw.NewLine();
        }

        // DoStatement
        private static void WriteDoStatement(AstWriter aw, Expression expr) {
            DoStatement node = (DoStatement)expr;
            aw.Out(".do ");
            if (node.Label != null) {
                aw.DumpLabel(node.Label);
                aw.Out(" ");
            }
            aw.Out("{", Flow.NewLine);
            aw.Indent();
            aw.WalkNode(node.Body);
            aw.Dedent();
            aw.Out(Flow.NewLine, "} .while (");
            aw.WalkNode(node.Test);
            aw.Out(");");
        }

        // EmptyStatement
        private static void WriteEmptyStatement(AstWriter aw, Expression expr) {
            EmptyStatement node = (EmptyStatement)expr;
            aw.Out("/*empty*/;", Flow.NewLine);
        }

        // LabeledStatement
        private static void WriteLabeledStatement(AstWriter aw, Expression expr) {
            LabeledStatement node = (LabeledStatement)expr;
            aw.Out(".labeled ");
            aw.DumpLabel(node.Label);
            aw.Out(" {", Flow.NewLine);
            aw.Indent();
            aw.WalkNode(node.Statement);
            aw.Dedent();
            aw.Out(Flow.NewLine, "}");
        }

        // LoopStatement
        private static void WriteLoopStatement(AstWriter aw, Expression expr) {
            LoopStatement node = (LoopStatement)expr;
            aw.Out(".for ");
            if (node.Label != null) {
                aw.DumpLabel(node.Label);
                aw.Out(" ");
            }
            aw.Out("(; ");
            aw.WalkNode(node.Test);
            aw.Out("; ");
            aw.WalkNode(node.Increment);
            aw.Out(") {", Flow.NewLine);
            aw.Indent();
            aw.WalkNode(node.Body);
            aw.Dedent();
            aw.Out(Flow.NewLine, "}");
        }

        // ReturnStatement
        private static void WriteReturnStatement(AstWriter aw, Expression expr) {
            ReturnStatement node = (ReturnStatement)expr;
            aw.Out(".return", Flow.Space);
            aw.WalkNode(node.Expression);
            aw.Out(";", Flow.NewLine);
        }

        // ScopeStatement
        private static void WriteScopeStatement(AstWriter aw, Expression expr) {
            ScopeStatement node = (ScopeStatement)expr;
            aw.Out(".scope {", Flow.NewLine);
            aw.Indent();
            aw.WalkNode(node.Body);
            aw.Dedent();
            aw.Out("}", Flow.NewLine);
        }

        // SwitchStatement
        private static void WriteSwitchStatement(AstWriter aw, Expression expr) {
            SwitchStatement node = (SwitchStatement)expr;
            aw.Out(".switch ");
            if (node.Label != null) {
                aw.DumpLabel(node.Label);
                aw.Out(" ");
            }
            aw.Out("(");
            aw.WalkNode(node.TestValue);
            aw.Out(") {", Flow.NewLine);
            foreach (SwitchCase sc in node.Cases) {
                if (sc.IsDefault) {
                    aw.Out(".default");
                } else {
                    aw.Out(".case " + sc.Value);
                }
                aw.Out(":", Flow.NewLine);
                aw.Indent(); aw.Indent();
                aw.WalkNode(sc.Body);
                aw.Dedent(); aw.Dedent();
                aw.NewLine();
            }
            aw.Out("}", Flow.NewLine);
        }

        // ThrowStatement
        private static void WriteThrowStatement(AstWriter aw, Expression expr) {
            ThrowStatement node = (ThrowStatement)expr;
            aw.Out(Flow.NewLine, ".throw (");
            aw.WalkNode(node.Exception);
            aw.Out(")", Flow.NewLine);
        }

        // TryStatement
        private static void WriteTryStatement(AstWriter aw, Expression expr) {
            TryStatement node = (TryStatement)expr;
            aw.Out(".try {", Flow.NewLine);
            aw.Indent();
            aw.WalkNode(node.Body);
            aw.Dedent();
            if (node.Handlers != null && node.Handlers.Count > 0) {
                foreach (CatchBlock cb in node.Handlers) {
                    aw.Out("} .catch ( " + cb.Test.Name);
                    if (cb.Variable != null) {
                        aw.Out(Flow.Space, SymbolTable.IdToString(cb.Variable.Name));
                    }
                    aw.Out(") {", Flow.NewLine);
                    aw.Indent();
                    aw.WalkNode(cb.Body);
                    aw.Dedent();
                }
            }
            if (node.FinallyStatement != null) {
                aw.Out("} .finally {", Flow.NewLine);
                aw.Indent();
                aw.WalkNode(node.FinallyStatement);
                aw.Dedent();
            }
            aw.Out("}", Flow.NewLine);
        }

        // YieldStatement
        private static void WriteYieldStatement(AstWriter aw, Expression expr) {
            YieldStatement node = (YieldStatement)expr;
            aw.Out(".yield ");
            aw.WalkNode(node.Expression);
            aw.Out(";", Flow.NewLine);
        }

        private static void WriteExtensionExpression(AstWriter aw, Expression expr) {
            aw.Out(".extension ");
            // print the known node (best we can do)
            aw.WalkNode(Expression.ReduceToKnown(expr));
            aw.Out(";", Flow.NewLine);
        }

        private static string GetLambdaInfo(LambdaExpression lambda) {
            string info = lambda.NodeType == AstNodeType.Generator ? ".generator ": ".lambda ";

            info += String.Format("{0} {1} (", lambda.ReturnType.Name, lambda.Name);
            if (lambda.IsGlobal) {
                info += " global,";
            }
            if (!lambda.IsVisible) {
                info += " hidden,";
            }
            if (lambda.ParameterArray) {
                info += " param array,";
            }
            if (lambda.EmitLocalDictionary) {
                info += " local dict";
            }
            info += ")";
            return info;
        }

        private void DumpVariable(Expression v) {
            string descr = String.Format("{2} {0} ({1})",
                SymbolTable.IdToString(VariableInfo.GetName(v)),
                v.NodeType.ToString().Replace("Variable", ""),
                v.Type.Name
            );
            Out(descr);
            NewLine();
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
                Out(Flow.NewLine, ".arg", Flow.Space);
                DumpVariable(p);
            }
            Dedent();
            Out(") {");
            Indent();
            Out(Flow.NewLine, "");
            WalkNode(node.Body);
            Dedent();
            Out(Flow.NewLine, "}");
        }

        #endregion
    }
}
#endif
