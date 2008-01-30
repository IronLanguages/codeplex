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

#if DEBUG
namespace Microsoft.Scripting.Ast {
    class AstWriter {
        [Flags]
        private enum Flow {
            None,
            Space,
            NewLine,

            Break = 0x8000      // newline if column > MaxColumn
        };

        private struct CodeBlockId {
            private readonly CodeBlock _block;
            private readonly int _id;

            internal CodeBlockId(CodeBlock block, int id) {
                _block = block;
                _id = id;
            }

            internal CodeBlock CodeBlock {
                get { return _block; }
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

        private Queue<CodeBlockId> _blocks;
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
        internal static void Dump(CodeBlock/*!*/ block) {
            Debug.Assert(block != null);

            if (ScriptDomainManager.Options.ShowASTs) {
#if !SILVERLIGHT
                ConsoleColor color = Console.ForegroundColor;
                try {
                    Console.ForegroundColor = GetAstColor();
#endif
                    Dump(block, System.Console.Out);
#if !SILVERLIGHT
                } finally {
                    Console.ForegroundColor = color;
                }
#endif
            } else if (ScriptDomainManager.Options.DumpASTs) {
                StreamWriter sw = new StreamWriter(GetFilePath(block.Name), true);
                using (sw) {
                    Dump(block, sw);
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

        private static void Dump(CodeBlock/*!*/ block, TextWriter/*!*/ writer) {
            Debug.Assert(block != null);
            Debug.Assert(writer != null);

            AstWriter dv = new AstWriter(writer);
            dv.DoDump(block);
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

        private void DoDump(CodeBlock node) {
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

            while (_blocks != null && _blocks.Count > 0) {
                CodeBlockId b = _blocks.Dequeue();
                WriteLine();
                WriteLine("//");
                WriteLine("// CODE BLOCK: {0} ({1})", b.CodeBlock.Name, b.Id);
                WriteLine("//");
                WriteLine();

                WalkNode(b.CodeBlock);

                Debug.Assert(_stack.Count == 0);
            }
        }

        private int Enqueue(CodeBlock block) {
            if (_blocks == null) {
                _blocks = new Queue<CodeBlockId>();
            }
            _blocks.Enqueue(new CodeBlockId(block, ++_blockid));
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

            switch (node.NodeType) {
                case AstNodeType.Add:
                case AstNodeType.And:
                case AstNodeType.AndAlso:
                case AstNodeType.ArrayIndex:
                case AstNodeType.Divide:
                case AstNodeType.Equal:
                case AstNodeType.ExclusiveOr:
                case AstNodeType.GreaterThan:
                case AstNodeType.GreaterThanOrEqual:
                case AstNodeType.LeftShift:
                case AstNodeType.LessThan:
                case AstNodeType.LessThanOrEqual:
                case AstNodeType.Modulo:
                case AstNodeType.Multiply:
                case AstNodeType.NotEqual:
                case AstNodeType.Or:
                case AstNodeType.OrElse:
                case AstNodeType.RightShift:
                case AstNodeType.Subtract:
                    Dump((BinaryExpression)node);
                    break;
                case AstNodeType.Call:
                    Dump((MethodCallExpression)node);
                    break;
                case AstNodeType.Conditional:
                    Dump((ConditionalExpression)node);
                    break;
                case AstNodeType.Constant:
                    Dump((ConstantExpression)node);
                    break;
                case AstNodeType.Convert:
                case AstNodeType.Negate:
                case AstNodeType.Not:
                case AstNodeType.OnesComplement:
                    Dump((UnaryExpression)node);
                    break;
                case AstNodeType.New:
                    Dump((NewExpression)node);
                    break;
                case AstNodeType.TypeIs:
                    Dump((TypeBinaryExpression)node);
                    break;
                case AstNodeType.ActionExpression:
                    Dump((ActionExpression)node);
                    break;
                case AstNodeType.ArrayIndexAssignment:
                    Dump((ArrayIndexAssignment)node);
                    break;
                case AstNodeType.Block:
                    Dump((Block)node);
                    break;
                case AstNodeType.BoundAssignment:
                    Dump((BoundAssignment)node);
                    break;
                case AstNodeType.BoundExpression:
                    Dump((BoundExpression)node);
                    break;
                case AstNodeType.BreakStatement:
                    Dump((BreakStatement)node);
                    break;
                case AstNodeType.CodeBlockExpression:
                    Dump((CodeBlockExpression)node);
                    break;
                case AstNodeType.CodeContextExpression:
                    Out(".context");
                    break;
                case AstNodeType.GeneratorIntrinsic:
                    Out(".gen_intrinsic");
                    break;
                case AstNodeType.ContinueStatement:
                    Dump((ContinueStatement)node);
                    break;
                case AstNodeType.DeleteStatement:
                    Dump((DeleteStatement)node);
                    break;
                case AstNodeType.DeleteUnboundExpression:
                    Dump((DeleteUnboundExpression)node);
                    break;
                case AstNodeType.DoStatement:
                    Dump((DoStatement)node);
                    break;
                case AstNodeType.EmptyStatement:
                    Dump((EmptyStatement)node);
                    break;
                case AstNodeType.EnvironmentExpression:
                    Out(".env");
                    break;
                case AstNodeType.ExpressionStatement:
                    Dump((ExpressionStatement)node);
                    break;
                case AstNodeType.LabeledStatement:
                    Dump((LabeledStatement)node);
                    break;
                case AstNodeType.LoopStatement:
                    Dump((LoopStatement)node);
                    break;
                case AstNodeType.MemberAssignment:
                    Dump((MemberAssignment)node);
                    break;
                case AstNodeType.MemberExpression:
                    Dump((MemberExpression)node);
                    break;
                case AstNodeType.NewArrayExpression:
                    Dump((NewArrayExpression)node);
                    break;
                case AstNodeType.ParamsExpression:
                    Out(".params");
                    break;
                case AstNodeType.ReturnStatement:
                    Dump((ReturnStatement)node);
                    break;
                case AstNodeType.ScopeStatement:
                    Dump((ScopeStatement)node);
                    break;
                case AstNodeType.SwitchStatement:
                    Dump((SwitchStatement)node);
                    break;
                case AstNodeType.ThrowStatement:
                    Dump((ThrowStatement)node);
                    break;
                case AstNodeType.TryStatement:
                    Dump((TryStatement)node);
                    break;
                case AstNodeType.UnboundAssignment:
                    Dump((UnboundAssignment)node);
                    break;
                case AstNodeType.UnboundExpression:
                    Dump((UnboundExpression)node);
                    break;
                case AstNodeType.YieldStatement:
                    Dump((YieldStatement)node);
                    break;
                default:
                    throw new InvalidOperationException("Unexpected node type: " + node.NodeType.ToString());
            }
        }

        private void WalkNode(CodeBlock node) {
            GeneratorCodeBlock gcb = node as GeneratorCodeBlock;
            if (gcb != null) {
                DumpGeneratorCodeBlock(gcb);
            } else {
                DumpCodeBlock(node);
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
        private void Dump(ActionExpression node) {
            Out(".action", Flow.Space);
            
            Out("(");
            Out(node.Type.Name);
            Out(")", Flow.Space);

            Out(FormatAction(node.Action));
            Out("( // " + node.Action.ToString());
            Indent();
            NewLine();
            foreach (Expression arg in node.Arguments) {
                WalkNode(arg);
                NewLine();
            }
            Dedent();
            Out(")");
        }

        // ArrayIndexAssignment
        private void Dump(ArrayIndexAssignment node) {
            WalkNode(node.Array);
            Out("[");
            WalkNode(node.Index);
            Out("] = ");
            WalkNode(node.Value);
        }

        // BinaryExpression
        private void Dump(BinaryExpression node) {
            if (node.NodeType == AstNodeType.ArrayIndex) {
                WalkNode(node.Left);
                Out("[");
                WalkNode(node.Right);
                Out("]");
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
                Out(Flow.Break, "(", Flow.None);
                WalkNode(node.Left);
                Out(Flow.Space, op, Flow.Space | Flow.Break);
                WalkNode(node.Right);
                Out(Flow.None, ")", Flow.Break);
            }
        }

        // BoundAssignment
        private void Dump(BoundAssignment node) {
            Out("(.bound " + SymbolTable.IdToString(node.Variable.Name) + ") = ");
            WalkNode(node.Value);
        }

        // BoundExpression
        private void Dump(BoundExpression node) {
            Out("(.bound ");
            Out(SymbolTable.IdToString(node.Name));
            Out(")");
        }

        // CodeBlockExpression
        private void Dump(CodeBlockExpression node) {
            int id = Enqueue(node.Block);
            Out(String.Format(".block ({0} #{1}", node.Block.Name, id));
            Indent();
            bool nl = false;
            if (node.ForceWrapperMethod) { nl = true; Out(Flow.NewLine, "ForceWrapper"); }
            if (node.IsStronglyTyped) { nl = true; Out(Flow.NewLine, "StronglyTyped"); }
            Dedent();
            Out(nl ? Flow.NewLine : Flow.None, ")");
        }

        // ConditionalExpression
        private void Dump(ConditionalExpression node) {
            Out(".if (", Flow.Break);
            WalkNode(node.Test);
            Out(" ) {", Flow.Break);
            WalkNode(node.IfTrue);
            Out(Flow.Break, "} .else {", Flow.Break);
            WalkNode(node.IfFalse);
            Out("}", Flow.Break);
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
        private void Dump(ConstantExpression node) {
            Out(Constant(node.Value));
        }

        // DeleteUnboundExpression
        private void Dump(DeleteUnboundExpression node) {
            Out(String.Format(".delname({0})", SymbolTable.IdToString(node.Name)));
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
        private void Dump(MemberAssignment node) {
            OutMember(node.Expression, node.Member);
            Out(" = ");
            WalkNode(node.Value);
        }

        // MemberExpression
        private void Dump(MemberExpression node) {
            OutMember(node.Expression, node.Member);
        }

        // MethodCallExpression
        private void Dump(MethodCallExpression node) {
            if (node.Instance != null) {
                Out("(");
                WalkNode(node.Instance);
                Out(").");
            }
            Out("(" + node.Method.ReflectedType.Name + "." + node.Method.Name + ")(");
            if (node.Arguments != null && node.Arguments.Count > 0) {
                NewLine(); Indent();
                foreach (Expression e in node.Arguments) {
                    WalkNode(e);
                    Out(",", Flow.NewLine);
                }
                Dedent();
            }
            Out(")");
        }

        // NewArrayExpression
        private void Dump(NewArrayExpression node) {
            Out(".new " + node.Type.Name + " = {");
            if (node.Expressions != null && node.Expressions.Count > 0) {
                NewLine(); Indent();
                foreach (Expression e in node.Expressions) {
                    WalkNode(e);
                    Out(",", Flow.NewLine);
                }
                Dedent();
            }
            Out("}");
        }

        // NewExpression
        private void Dump(NewExpression node) {
            Out(".new " + node.Type.Name + "(");
            if (node.Arguments != null && node.Arguments.Count > 0) {
                NewLine(); Indent();
                foreach (Expression e in node.Arguments) {
                    WalkNode(e);
                    Out(",", Flow.NewLine);
                }
                Dedent();
            }
            Out(")");
        }

        // TypeBinaryExpression
        private void Dump(TypeBinaryExpression node) {
            WalkNode(node.Expression);
            Out(Flow.Space, ".is", Flow.Space);
            Out(node.TypeOperand.Name);
        }

        // UnaryExpression
        private void Dump(UnaryExpression node) {
            switch (node.NodeType) {
                case AstNodeType.Convert:
                    Out("(" + node.Type.Name + ")");
                    break;
                case AstNodeType.Not:
                    Out(node.Type == typeof(bool) ? "!" : "~");
                    break;
                case AstNodeType.Negate:
                    Out("-");
                    break;
                case AstNodeType.OnesComplement:
                    Out("~");
                    break;
            }

            WalkNode(node.Operand);
        }

        // UnboundAssignment
        private void Dump(UnboundAssignment node) {
            Out(SymbolTable.IdToString(node.Name));
            Out(" := ");
            WalkNode(node.Value);
        }

        // UnboundExpression
        private void Dump(UnboundExpression node) {
            Out(".unbound " + SymbolTable.IdToString(node.Name));
        }

        // Block
        private void Dump(Block node) {
            Out(node.Type != typeof(void) ? ".comma {" : "{");
            NewLine(); Indent();
            foreach (Expression s in node.Expressions) {
                WalkNode(s);
                NewLine();
            }
            Dedent();
            Out("}", Flow.NewLine);
        }

        // BreakStatement
        private void Dump(BreakStatement node) {
            Out(".break;", Flow.NewLine);
        }

        // ContinueStatement
        private void Dump(ContinueStatement node) {
            Out(".continue;", Flow.NewLine);
        }

        // DeleteStatement
        private void Dump(DeleteStatement node) {
            Out(".del");
            if (node.Variable != null) {
                Out(Flow.Space, SymbolTable.IdToString(node.Variable.Name));
            }
            NewLine();
        }

        // DoStatement
        private void Dump(DoStatement node) {
            Out(".do {", Flow.NewLine);
            Indent();
            WalkNode(node.Body);
            Dedent();
            Out(Flow.NewLine, "} .while (");
            WalkNode(node.Test);
            Out(");");
        }

        // EmptyStatement
        private void Dump(EmptyStatement node) {            
            Out("/*empty*/;", Flow.NewLine);
        }

        // ExpressionStatement
        private void Dump(ExpressionStatement node) {
            WalkNode(node.Expression);
            Out(";", Flow.NewLine);
        }

        // LabeledStatement
        private void Dump(LabeledStatement node) {
            Out(".labeled {", Flow.NewLine);
            Indent();
            WalkNode(node.Statement);
            Dedent();
            Out(Flow.NewLine, "}");
        }

        // LoopStatement
        private void Dump(LoopStatement node) {
            Out(".for (; ");
            WalkNode(node.Test);
            Out("; ");
            WalkNode(node.Increment);
            Out(") {", Flow.NewLine);
            Indent();
            WalkNode(node.Body);
            Dedent();
            Out(Flow.NewLine, "}");
        }

        // ReturnStatement
        private void Dump(ReturnStatement node) {
            Out(".return", Flow.Space);
            WalkNode(node.Expression);
            Out(";", Flow.NewLine);
        }

        // ScopeStatement
        private void Dump(ScopeStatement node) {
            Out(".scope (");
            WalkNode(node.Scope);
            Out(") {", Flow.NewLine);
            Indent();
            WalkNode(node.Body);
            Dedent();
            Out("}", Flow.NewLine);
        }

        // SwitchStatement
        private void Dump(SwitchStatement node) {
            Out(".switch (");
            WalkNode(node.TestValue);
            Out(") {", Flow.NewLine);
            foreach (SwitchCase sc in node.Cases) {
                if (sc.IsDefault) {
                    Out(".default");
                } else {
                    Out(".case " + sc.Value);
                }
                Out(":", Flow.NewLine);
                Indent(); Indent();
                WalkNode(sc.Body);
                Dedent(); Dedent();
                NewLine();
            }
            Out("}", Flow.NewLine);
        }

        // ThrowStatement
        private void Dump(ThrowStatement node) {
            Out(Flow.NewLine, ".throw (");
            WalkNode(node.Exception);
            Out(")", Flow.NewLine);
        }

        // TryStatement
        private void Dump(TryStatement node) {
            Out(".try {", Flow.NewLine);
            Indent();
            WalkNode(node.Body);
            Dedent();
            if (node.Handlers != null && node.Handlers.Count > 0) {
                foreach (CatchBlock cb in node.Handlers) {
                    Out("} .catch ( " + cb.Test.Name);
                    if (cb.Variable != null) {
                        Out(Flow.Space, SymbolTable.IdToString(cb.Variable.Name));
                    }
                    Out(") {", Flow.NewLine);
                    Indent();
                    WalkNode(cb.Body);
                    Dedent();
                }
            }
            if (node.FinallyStatement != null) {
                Out("} .finally {", Flow.NewLine);
                Indent();
                WalkNode(node.FinallyStatement);
                Dedent();
            }
            Out("}", Flow.NewLine);
        }

        // YieldStatement
        private void Dump(YieldStatement node) {
            Out(".yield ");
            WalkNode(node.Expression);
            Out(";", Flow.NewLine);
        }

        private static string GetCodeBlockInfo(CodeBlock block) {
            string info = String.Format("{0} {1} (", block.ReturnType.Name, block.Name);
            if (block.IsGlobal) {
                info += " global,";
            }
            if (!block.IsVisible) {
                info += " hidden,";
            }
            if (block.IsClosure) {
                info += " closure,";
            }
            if (block.ParameterArray) {
                info += " param array,";
            }
            if (block.HasEnvironment) {
                info += " environment,";
            }
            if (block.EmitLocalDictionary) {
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
            if (v.InParameterArray) {
                descr += ",InParameterArray";
            }
            descr += ")";
            Out(descr);
            NewLine();
        }

        private void DumpBlock(CodeBlock node) {
            Out(GetCodeBlockInfo(node));
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

        // CodeBlock
        private void DumpCodeBlock(CodeBlock node) {
            Out(".codeblock", Flow.Space);
            DumpBlock(node);
        }

        // GeneratorCodeBlock
        private void DumpGeneratorCodeBlock(GeneratorCodeBlock node) {
            Out(".generator", Flow.Space);
            DumpBlock(node);
        }

        #endregion
    }
}
#endif
