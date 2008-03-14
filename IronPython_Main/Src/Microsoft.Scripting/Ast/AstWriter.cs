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
        /// Write out the given AST (only if ShowASTs or DumpASTs is enabled)
        /// </summary>
        internal static void Dump(LambdaExpression/*!*/ lambda) {
            Debug.Assert(lambda != null);

            if (ScriptDomainManager.Options.ShowASTs) {
#if !SILVERLIGHT
                ConsoleColor color = Console.ForegroundColor;
                try {
                    Console.ForegroundColor = GetAstColor();
#endif
                    Dump(lambda, System.Console.Out);
#if !SILVERLIGHT
                } finally {
                    Console.ForegroundColor = color;
                }
#endif
            } else if (ScriptDomainManager.Options.DumpASTs) {
                StreamWriter sw = new StreamWriter(GetFilePath(lambda.Name), true);
                using (sw) {
                    Dump(lambda, sw);
                }
            }
        }

        /// <summary>
        /// Write out the given rule's AST (only if ShowRules is enabled)
        /// </summary>
        internal static void Dump<T>(StandardRule<T> rule) {
            if (ScriptDomainManager.Options.ShowRules) {
#if !SILVERLIGHT
                ConsoleColor color = Console.ForegroundColor;
                try {
                    Console.ForegroundColor = GetAstColor();
#endif
                    AstWriter.Dump(rule.Test, "Rule.Test", System.Console.Out);
                    AstWriter.Dump(rule.Target, "Rule.Target", System.Console.Out);
#if !SILVERLIGHT
                } finally {
                    Console.ForegroundColor = color;
                }
#endif
            }
        }

        private static void Dump(LambdaExpression/*!*/ lambda, TextWriter/*!*/ writer) {
            Debug.Assert(lambda != null);
            Debug.Assert(writer != null);

            AstWriter dv = new AstWriter(writer);
            dv.DoDump(lambda);
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

        private void DoDump(LambdaExpression node) {
            WritePrologue(node.Name);

            WalkNode(node);

            WriteBlocks();
            WriteLine();
        }

        private void DoDump(Expression node, string name) {
            WritePrologue(name);

            WalkNode(node);

            WriteBlocks();
            WriteLine();
        }

        private void WritePrologue(string name) {
            WriteLine("//");
            WriteLine("// AST {0}", name);
            WriteLine("//");
            WriteLine();
        }

        private void WriteBlocks() {
            Debug.Assert(_stack.Count == 0);

            while (_lambdaIds != null && _lambdaIds.Count > 0) {
                LambdaId b = _lambdaIds.Dequeue();
                WriteLine();
                WriteLine("//");
                WriteLine("// LAMBDA: {0} ({1})", b.Lambda.Name, b.Id);
                WriteLine("//");
                WriteLine();

                WalkNode(b.Lambda);

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

        private void WalkNode(LambdaExpression node) {
            GeneratorCodeBlock gcb = node as GeneratorCodeBlock;
            if (gcb != null) {
                WriteGeneratorCodeBlock(gcb);
            } else {
                WriteLambda(node);
            }
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
                return "InvokeMember " + ima.Name;
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
            aw.Out(".action", Flow.Space);

            aw.Out("(");
            aw.Out(node.Type.Name);
            aw.Out(")", Flow.Space);

            aw.Out(FormatAction(node.Action));
            aw.Out("( // " + node.Action.ToString());
            aw.Indent();
            aw.NewLine();
            foreach (Expression arg in node.Arguments) {
                aw.WalkNode(arg);
                aw.NewLine();
            }
            aw.Dedent();
            aw.Out(")");
        }

        // ArrayIndexAssignment
        private static void WriteArrayIndexAssignment(AstWriter aw, Expression expr) {
            ArrayIndexAssignment node = (ArrayIndexAssignment)expr;
            aw.WalkNode(node.Array);
            aw.Out("[");
            aw.WalkNode(node.Index);
            aw.Out("] = ");
            aw.WalkNode(node.Value);
        }
      
        // BinaryExpression
        private static void WriteBinaryExpression(AstWriter aw, Expression expr) {
            BinaryExpression node = (BinaryExpression)expr;
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

        // BoundAssignment
        private static void WriteBoundAssignment(AstWriter aw, Expression expr) {
            BoundAssignment node = (BoundAssignment)expr;
            aw.Out("(.bound " + SymbolTable.IdToString(node.Variable.Name) + ") = ");
            aw.WalkNode(node.Value);
        }

        // BoundExpression
        private static void WriteBoundExpression(AstWriter aw, Expression expr) {
            BoundExpression node = (BoundExpression)expr;
            aw.Out("(.bound ");
            aw.Out(SymbolTable.IdToString(node.Name));
            aw.Out(")");
        }

        // CodeBlockExpression
        private static void WriteCodeBlockExpression(AstWriter aw, Expression expr) {
            CodeBlockExpression node = (CodeBlockExpression)expr;
            int id = aw.Enqueue(node.Block);
            aw.Out(String.Format(".block ({0} {1} #{2})", node.Block.Name, node.Type, id));
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

        // DeleteUnboundExpression
        private static void WriteDeleteUnboundExpression(AstWriter aw, Expression expr) {
            DeleteUnboundExpression node = (DeleteUnboundExpression)expr;
            aw.Out(String.Format(".delname({0})", SymbolTable.IdToString(node.Name)));
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
                case AstNodeType.EnvironmentExpression:
                    aw.Out(".env");
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

        // MemberAssignment
        private static void WriteMemberAssignment(AstWriter aw, Expression expr) {
            MemberAssignment node = (MemberAssignment)expr;
            aw.OutMember(node.Expression, node.Member);
            aw.Out(" = ");
            aw.WalkNode(node.Value);
        }

        // MemberExpression
        private static void WriteMemberExpression(AstWriter aw, Expression expr) {
            MemberExpression node = (MemberExpression)expr;
            aw.OutMember(node.Expression, node.Member);
        }

        // MethodCallExpression
        private static void WriteMethodCallExpression(AstWriter aw, Expression expr) {
            MethodCallExpression node = (MethodCallExpression)expr;
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

        // UnboundAssignment
        private static void WriteUnboundAssignment(AstWriter aw, Expression expr) {
            UnboundAssignment node = (UnboundAssignment)expr;
            aw.Out(SymbolTable.IdToString(node.Name));
            aw.Out(" := ");
            aw.WalkNode(node.Value);
        }

        // UnboundExpression
        private static void WriteUnboundExpression(AstWriter aw, Expression expr) {
            UnboundExpression node = (UnboundExpression)expr;
            aw.Out(".unbound " + SymbolTable.IdToString(node.Name));
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
            aw.Out(".break;", Flow.NewLine);
        }

        // ContinueStatement
        private static void WriteContinueStatement(AstWriter aw, Expression expr) {
            ContinueStatement node = (ContinueStatement)expr;
            aw.Out(".continue;", Flow.NewLine);
        }

        // DeleteStatement
        private static void WriteDeleteStatement(AstWriter aw, Expression expr) {
            DeleteStatement node = (DeleteStatement)expr;
            aw.Out(".del");
            if (node.Variable != null) {
                aw.Out(Flow.Space, SymbolTable.IdToString(node.Variable.Name));
            }
            aw.NewLine();
        }

        // DoStatement
        private static void WriteDoStatement(AstWriter aw, Expression expr) {
            DoStatement node = (DoStatement)expr;
            aw.Out(".do {", Flow.NewLine);
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

        // ExpressionStatement
        private static void WriteExpressionStatement(AstWriter aw, Expression expr) {
            ExpressionStatement node = (ExpressionStatement)expr;
            aw.WalkNode(node.Expression);
            aw.Out(";", Flow.NewLine);
        }

        // LabeledStatement
        private static void WriteLabeledStatement(AstWriter aw, Expression expr) {
            LabeledStatement node = (LabeledStatement)expr;
            aw.Out(".labeled {", Flow.NewLine);
            aw.Indent();
            aw.WalkNode(node.Statement);
            aw.Dedent();
            aw.Out(Flow.NewLine, "}");
        }

        // LoopStatement
        private static void WriteLoopStatement(AstWriter aw, Expression expr) {
            LoopStatement node = (LoopStatement)expr;
            aw.Out(".for (; ");
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
            aw.Out(".scope (");
            aw.WalkNode(node.Scope);
            aw.Out(") {", Flow.NewLine);
            aw.Indent();
            aw.WalkNode(node.Body);
            aw.Dedent();
            aw.Out("}", Flow.NewLine);
        }

        // SwitchStatement
        private static void WriteSwitchStatement(AstWriter aw, Expression expr) {
            SwitchStatement node = (SwitchStatement)expr;
            aw.Out(".switch (");
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

        private static string GetLambdaInfo(LambdaExpression lambda) {
            string info = String.Format("{0} {1} (", lambda.ReturnType.Name, lambda.Name);
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

        private void DumpVariable(Variable v) {
            string descr = String.Format("{2} {0} ({1}", SymbolTable.IdToString(v.Name), v.Kind.ToString(), v.Type.Name);
            if (v.Lift) {
                descr += ",Lift";
            }
            descr += ")";
            Out(descr);
            NewLine();
        }

        private void DumpBlock(LambdaExpression node) {
            Out(GetLambdaInfo(node));
            Out("(");
            Indent();
            foreach (Variable v in node.Parameters) {
                Out(Flow.NewLine, ".arg", Flow.Space);
                DumpVariable(v);
            }
            Dedent();
            Out(") {");
            Indent();
            foreach (Variable v in node.Variables) {
                Out(Flow.NewLine, ".var", Flow.Space);
                DumpVariable(v);
            }
            Out(Flow.NewLine, "", Flow.NewLine);
            WalkNode(node.Body);
            Dedent();
            Out("}");
        }

        // LambdaExpression
        private void WriteLambda(LambdaExpression node) {
            Out(".lambda", Flow.Space);
            DumpBlock(node);
        }

        // GeneratorCodeBlock
        private void WriteGeneratorCodeBlock(GeneratorCodeBlock node) {
            Out(".generator", Flow.Space);
            DumpBlock(node);
        }

        #endregion
    }
}
#endif
