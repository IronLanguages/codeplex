/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

#if DEBUG
namespace Microsoft.Scripting.Ast {
    class AstWriter : Walker {

        private int _depth = 0;
        private TextWriter _outFile;

        private AstWriter() {

        }

        /// <summary>
        /// Write out the given AST (only if ShowASTs or DumpASTs is enabled)
        /// </summary>
        public static void Dump(Node node, CompilerContext context) {
            string descr = context.SourceUnit.Name;
            if (ScriptDomainManager.Options.ShowASTs) {
                AstWriter.ForceDump(node, descr, System.Console.Out);
            } else if (ScriptDomainManager.Options.DumpASTs) {
                AstWriter.ForceDump(node, descr, new StreamWriter(FixPath(descr) + ".ast", true));
            }
        }

        /// <summary>
        /// Write out the given rule's AST (only if ShowRules is enabled)
        /// </summary>
        public static void DumpRule<T>(StandardRule<T> rule) {
            if (ScriptDomainManager.Options.ShowRules) {
                AstWriter.ForceDump(rule.Test, rule.ToString() + ".Test", System.Console.Out);
                AstWriter.ForceDump(rule.Target, rule.ToString() + ".Target", System.Console.Out);
            }
        }

        /// <summary>
        /// Write out the given AST
        /// </summary>
        public static void ForceDump(Node node, string descr, TextWriter outFile) {
            if (node != null) {
                if (descr == null) descr = "<unknown>";
                AstWriter dv = new AstWriter();
                dv.DoDump(node, descr, outFile);
            }
        }

        private static string FixPath(string path) {
#if !SILVERLIGHT // GetInvalidFileNameChars does not exist in CoreCLR
            char[] invalid = System.IO.Path.GetInvalidFileNameChars();

            foreach (char ch in invalid) {
                path = path.Replace(ch, '_');
            }
#endif
            return path;
        }

        private void DoDump(Node node, string name, TextWriter outFile) {
            _outFile = outFile;
            _depth = 0;

            _outFile.WriteLine("# AST {0}", name);
            node.Walk(this);
            Debug.Assert(_depth == 0);
            _outFile.Close();
        }


        private void Push(string label) {
            Write(label);
            _depth++;
        }

        private void Pop() {
            _depth--;
            Debug.Assert(_depth >= 0);
        }
        public void Write(string s) {
            _outFile.WriteLine(new string(' ', _depth * 2) + s);
        }


        private void Child(Node node) {
            if (node != null) node.Walk(this);
        }

        // Unfortunately, overload resolution happens in our parent class, so we have to explicitly call this default case
        public bool DefaultWalk(Node node, string name) {
            Push(name);
            return (node != null);
        }

        // More proper would be to make this a virtual method on Action
        private string FormatAction(Action action) {
            DoOperationAction doa;
            GetMemberAction gma;
            SetMemberAction sma;
            ConvertToAction cta;
            CallAction cla;

            if ((doa = action as DoOperationAction) != null) {
                return "Do " + doa.Operation.ToString();
            } else if ((gma = action as GetMemberAction) != null) {
                return "GetMember " + SymbolTable.IdToString(gma.Name);
            } else if ((sma = action as SetMemberAction) != null) {
                return "SetMember " + SymbolTable.IdToString(sma.Name);
            } else if ((cta = action as ConvertToAction) != null) {
                return "ConvertTo " + cta.ToType.ToString();
            } else if ((cla = action as CallAction) != null) {
                return "Call";
            } else {
                return "UnknownAction (" + action.Kind.ToString() + ")";
            }
        }

        // ActionExpression
        public override bool Walk(ActionExpression node) {
            Push("<action> Kind: " + FormatAction(node.Action));
            return DefaultWalk(node, "<args>");
        }
        public override void PostWalk(ActionExpression node) {
            Pop();
            Pop();
        }

        // AndExpression
        public override bool Walk(AndExpression node) {
            return DefaultWalk(node, "<and>");
        }
        public override void PostWalk(AndExpression node) {
            Pop();
        }

        // ArrayIndexAssignment
        public override bool Walk(ArrayIndexAssignment node) {
            return DefaultWalk(node, "< [] = >");
        }
        public override void PostWalk(ArrayIndexAssignment node) {
            Pop();
        }

        // ArrayIndexExpression
        public override bool Walk(ArrayIndexExpression node) {
            return DefaultWalk(node, "< [] >");
        }
        public override void PostWalk(ArrayIndexExpression node) {
            Pop();
        }

        // BinaryExpression
        public override bool Walk(BinaryExpression node) {
            return DefaultWalk(node, "<binaryexpr> Op:" + node.Op.ToString());
        }
        public override void PostWalk(BinaryExpression node) {
            Pop();
        }

        // StaticUnaryExpression
        public override bool Walk(StaticUnaryExpression node) {
            return DefaultWalk(node, "<staticunaryexpr> Type:" + node.ExpressionType.ToString());
        }
        public override void PostWalk(StaticUnaryExpression node) {
            Pop();
        }

        // ConversionExpression
        public override bool Walk(ConversionExpression node) {
            return DefaultWalk(node, "<convert> Type:" + node.ExpressionType.ToString());
        }
        public override void PostWalk(ConversionExpression node) {
            Pop();
        }


        // BoundAssignment
        public override bool Walk(BoundAssignment node) {
            string descr = AssignmentOp(node.Operator);
            if (node.Variable != null)
                descr += " Target:" + SymbolTable.IdToString(node.Variable.Name);
            return DefaultWalk(node, descr);
        }
        public override void PostWalk(BoundAssignment node) {
            Pop();
        }

        // BoundExpression
        public override bool Walk(BoundExpression node) {
            return DefaultWalk(node, "BoundExpression: " + node.Name.ToString());
        }
        public override void PostWalk(BoundExpression node) {
            Pop();
        }

        // CallExpression
        public override bool Walk(CallExpression node) {
            Push("<call>");
            Child(node.Target);
            return DefaultWalk(node, "<args>");
        }
        public override void PostWalk(CallExpression node) {
            Pop();
            Pop();
        }

        // CallWithThisExpression
        public override bool Walk(CallWithThisExpression node) {
            return NotImplemented(node);
        }

        // CodeBlockExpression
        public override bool Walk(CodeBlockExpression node) {
            return DefaultWalk(node, "<codeblockexpr>");
        }
        public override void PostWalk(CodeBlockExpression node) {
            Pop();
        }

        // CodeContextExpression
        public override bool Walk(CodeContextExpression node) {
            return DefaultWalk(node, "<codecontextexpr>");
        }
        public override void PostWalk(CodeContextExpression node) {
            Pop();
        }

        // CommaExpression
        public override bool Walk(CommaExpression node) {
            return DefaultWalk(node, "<comma> Index:" + node.ValueIndex);
        }
        public override void PostWalk(CommaExpression node) {
            Pop();
        }

        // ConditionalExpression
        public override bool Walk(ConditionalExpression node) {
            return DefaultWalk(node, "<?:>");
        }
        public override void PostWalk(ConditionalExpression node) {
            Pop();
        }

        // ConstantExpression
        public override bool Walk(ConstantExpression node) {
            return DefaultWalk(node, node.Value == null ? "(null)" : node.Value.ToString());
        }
        public override void PostWalk(ConstantExpression node) {
            Pop();
        }

        // DynamicMemberAssignment
        public override bool Walk(DynamicMemberAssignment node) {
            return DefaultWalk(node, AssignmentOp(node.Op) + " Name:" + SymbolTable.IdToString(node.Name));
        }
        public override void PostWalk(DynamicMemberAssignment node) {
            Pop();
        }

        // DynamicMemberExpression
        public override bool Walk(DynamicMemberExpression node) {
            return DefaultWalk(node, "< . > Name:" + SymbolTable.IdToString(node.Name));
        }
        public override void PostWalk(DynamicMemberExpression node) {
            Pop();
        }

        // EnvironmentExpression
        public override bool Walk(EnvironmentExpression node) {
            return DefaultWalk(node, "<$env>");
        }
        public override void PostWalk(EnvironmentExpression node) {
            Pop();
        }

        // IndexAssignment
        public override bool Walk(IndexAssignment node) {
            return DefaultWalk(node, "< []= >: " + AssignmentOp(node.Op));
        }
        public override void PostWalk(IndexAssignment node) {
            Pop();
        }

        // IndexExpression
        public override bool Walk(IndexExpression node) {
            return DefaultWalk(node, "< [] >");
        }
        public override void PostWalk(IndexExpression node) {
            Pop();
        }

        // MemberExpression
        public override bool Walk(MemberExpression node) {
            return DefaultWalk(node, "<member> Name:" + node.Member.Name);
        }
        public override void PostWalk(MemberExpression node) {
            Pop();
        }

        // MethodCallExpression
        public override bool Walk(MethodCallExpression node) {
            Push("<methodcall> Name:" + node.Method.Name);
            Push("<instance>");
            Child(node.Instance);
            Pop();
            Push("<args>");
            foreach (Expression e in node.Arguments) {
                Child(e);
            }
            Pop();
            Pop();
            return false;
        }


        // NewArrayExpression
        public override bool Walk(NewArrayExpression node) {
            return DefaultWalk(node, "<newarray> Type:" + node.ExpressionType.Name);
        }
        public override void PostWalk(NewArrayExpression node) {
            Pop();
        }

        // NewExpression
        public override bool Walk(NewExpression node) {
            Push("<new> Name:" + node.Constructor.Name);
            Push("<args>");
            foreach (Expression e in node.Arguments) {
                Child(e);
            }
            Pop();
            Pop();
            return false;
        }


        // OrExpression
        public override bool Walk(OrExpression node) {
            return DefaultWalk(node, "<or>");
        }
        public override void PostWalk(OrExpression node) {
            Pop();
        }

        // ParamsExpression
        public override bool Walk(ParamsExpression node) {
            return DefaultWalk(node, "<$params$>");
        }
        public override void PostWalk(ParamsExpression node) {
            Pop();
        }

        // ParenthesisExpression
        public override bool Walk(ParenthesisExpression node) {
            return DefaultWalk(node, "<parens>");
        }
        public override void PostWalk(ParenthesisExpression node) {
            Pop();
        }

        // ShortCircuitExpression
        public override bool Walk(ShortCircuitExpression node) {
            return NotImplemented(node);
        }

        // VoidExpression
        public override bool Walk(VoidExpression node) {
            return DefaultWalk(node, "<void>");
        }
        public override void PostWalk(VoidExpression node) {
            Pop();
        }

        // BlockStatement
        public override bool Walk(BlockStatement node) {
            return DefaultWalk(node, "<block>");
        }
        public override void PostWalk(BlockStatement node) {
            Pop();
        }

        // BreakStatement
        public override bool Walk(BreakStatement node) {
            return DefaultWalk(node, "<break>");
        }
        public override void PostWalk(BreakStatement node) {
            Pop();
        }

        // ContinueStatement
        public override bool Walk(ContinueStatement node) {
            return DefaultWalk(node, "<continue>");
        }
        public override void PostWalk(ContinueStatement node) {
            Pop();
        }

        // DebugStatement
        public override bool Walk(DebugStatement node) {
            return DefaultWalk(node, "<trace> Marker:" + node.Marker);
        }
        public override void PostWalk(DebugStatement node) {
            Pop();
        }

        // DelStatement
        public override bool Walk(DelStatement node) {
            string descr = "<del>";
            if (node.Variable != null)
                descr += " Name:" + SymbolTable.IdToString(node.Variable.Name);
            return DefaultWalk(node, descr);
        }
        public override void PostWalk(DelStatement node) {
            Pop();
        }

        // DoStatement
        public override bool Walk(DoStatement node) {
            return DefaultWalk(node, "<do loop>");
        }
        public override void PostWalk(DoStatement node) {
            Pop();
        }

        // LoopStatement
        public override bool Walk(LoopStatement node) {
            return DefaultWalk(node, "<loop>");
        }
        public override void PostWalk(LoopStatement node) {
            Pop();
        }

        // EmptyStatement
        public override bool Walk(EmptyStatement node) {
            Push("<empty>");
            Pop();
            return false;
        }

        // ExpressionStatement
        public override bool Walk(ExpressionStatement node) {
            return DefaultWalk(node, "<exprstmt>");
        }
        public override void PostWalk(ExpressionStatement node) {
            Pop();
        }

        // IfStatement
        public override bool Walk(IfStatement node) {
            Push("<if>");
            foreach (IfStatementTest ist in node.Tests) {
                Child(ist);
            }
            Push("<else>");
            Child(node.ElseStatement);
            Pop();
            Pop();
            return false;
        }

        // LabeledStatement
        public override bool Walk(LabeledStatement node) {
            return DefaultWalk(node, "<label>");
        }
        public override void PostWalk(LabeledStatement node) {
            Pop();
        }

        // ReturnStatement
        public override bool Walk(ReturnStatement node) {
            return DefaultWalk(node, "<return>");
        }
        public override void PostWalk(ReturnStatement node) {
            Pop();
        }

        // SwitchStatement
        public override bool Walk(SwitchStatement node) {
            Push("<switch>");
            Push("<test>");
            Child(node.TestValue);
            Pop();
            foreach (SwitchCase sc in node.Cases) {
                DumpCase(sc);
            }
            Pop();
            return false;
        }

        private void DumpCase(SwitchCase sc) {
            Push("<case>");
            Child(sc.Value);
            Child(sc.Body);
            Pop();
        }

        // ThrowStatement
        public override bool Walk(ThrowExpression node) {
            return DefaultWalk(node, "<throw>");
        }
        public override void PostWalk(ThrowExpression node) {
            Pop();
        }

        // DynamicTryStatement
        public override bool Walk(DynamicTryStatement node) {
            Push("<try>");
            Child(node.Body);
            if (node.Handlers != null) {
                foreach (DynamicTryStatementHandler tsh in node.Handlers) {
                    Child(tsh);
                }
            }
            if (node.ElseStatement != null) {
                Push("<else>");
                Child(node.ElseStatement);
                Pop();
            }
            if (node.FinallyStatement != null) {
                Push("<finally>");
                Child(node.FinallyStatement);
                Pop();
            }
            Pop();
            return false;
        }

        // YieldStatement
        public override bool Walk(YieldStatement node) {
            return DefaultWalk(node, "<yield>");
        }
        public override void PostWalk(YieldStatement node) {
            Pop();
        }

        // Arg
        public override bool Walk(Arg node) {
            Push("<arg>");
            if (node.Name != SymbolId.Empty) {
                Push(SymbolTable.IdToString(node.Name));
                Pop();
            }
            Child(node.Expression);
            Pop();
            return false;
        }


        // CodeBlock
        public override bool Walk(CodeBlock node) {
            Push("<codeblock> Name:" + node.Name);
            Push("<variables>");
            foreach (Variable v in node.Variables) {
                DumpVariable(v);
            }
            Pop();
            Push("<parameters>");
            foreach (Variable v in node.Parameters) {
                DumpVariable(v);
            }
            Pop();
            Child(node.Body);
            Pop();
            return false;
        }

        private void DumpVariable(Variable v) {
            Push(SymbolTable.IdToString(v.Name));
            if (v.DefaultValue != null) {
                Child(v.DefaultValue);
            }
            Pop();
        }

        // GeneratorCodeBlock
        public override bool Walk(GeneratorCodeBlock node) {
            Push("<generator> Name:" + node.Name);
            Push("<parms>");
            foreach (Variable v in node.Parameters) {
                DumpVariable(v);
            }
            Pop();
            Child(node.Body);
            Pop();
            return false;
        }


        // IfStatementTest
        public override bool Walk(IfStatementTest node) {
            return DefaultWalk(node, "<cond>");
        }
        public override void PostWalk(IfStatementTest node) {
            Pop();
        }

        // DynamicTryStatementHandler
        public override bool Walk(DynamicTryStatementHandler node) {
            return DefaultWalk(node, "<catch>");
        }
        public override void PostWalk(DynamicTryStatementHandler node) {
            Pop();
        }

        private string AssignmentOp(Operators op) {
            return op == Operators.None ? "=" : Op(op);
        }

        private string Op(Operators op) {
            switch (op) {
                case Operators.Call: return "()";
                case Operators.CodeRepresentation: return "repr";
                case Operators.ConvertToString: return "str";
                case Operators.GetDescriptor: return "get";
                case Operators.SetDescriptor: return "set";
                case Operators.DeleteDescriptor: return "del";
                case Operators.Add: return "+";
                case Operators.Subtract: return "-";
                case Operators.Power: return "**";
                case Operators.Multiply: return "*";
                case Operators.FloorDivide: return "//";
                case Operators.Divide: return "/";
                case Operators.TrueDivide: return "/t";
                case Operators.Mod: return "%";
                case Operators.LeftShift: return "<<";
                case Operators.RightShift: return ">>";
                case Operators.BitwiseAnd: return "&";
                case Operators.BitwiseOr: return "|";
                case Operators.Xor: return "^";
                case Operators.LessThan: return "<";
                case Operators.GreaterThan: return ">";
                case Operators.LessThanOrEqual: return "<=";
                case Operators.GreaterThanOrEqual: return ">=";
                case Operators.Equal: return "=";
                case Operators.NotEqual: return "!=";
                case Operators.LessThanGreaterThan: return "<>";
                case Operators.InPlaceAdd: return "+=";
                case Operators.InPlaceSubtract: return "-=";
                case Operators.InPlacePower: return "**=";
                case Operators.InPlaceMultiply: return "*=";
                case Operators.InPlaceFloorDivide: return "//=";
                case Operators.InPlaceDivide: return "/=";
                case Operators.InPlaceTrueDivide: return "/t=";
                case Operators.InPlaceMod: return "%=";
                case Operators.InPlaceLeftShift: return "<<=";
                case Operators.InPlaceRightShift: return ">>=";
                case Operators.InPlaceBitwiseAnd: return "&=";
                case Operators.InPlaceBitwiseOr: return "|=";
                case Operators.InPlaceXor: return "^=";
                case Operators.ReverseAdd: return "r+";
                case Operators.ReverseSubtract: return "r-";
                case Operators.ReversePower: return "r**";
                case Operators.ReverseMultiply: return "r*";
                case Operators.ReverseFloorDivide: return "r//";
                case Operators.ReverseDivide: return "r/";
                case Operators.ReverseTrueDivide: return "rt/";
                case Operators.ReverseMod: return "r%";
                case Operators.ReverseLeftShift: return "r<<";
                case Operators.ReverseRightShift: return "r>>";
                case Operators.ReverseBitwiseAnd: return "r&";
                case Operators.ReverseBitwiseOr: return "r|";
                case Operators.ReverseXor: return "r^";
                case Operators.Contains: return "in";
                case Operators.GetItem: return "get[]";
                case Operators.SetItem: return "set[]";
                case Operators.DeleteItem: return "del[]";
                case Operators.Compare: return "cmp";
                case Operators.Positive: return "+";
                case Operators.Negate: return "-";
                case Operators.OnesComplement: return "~";
                case Operators.Length: return "len";
                case Operators.DivMod: return "/%";
                case Operators.ReverseDivMod: return "r/%";
                case Operators.MoveNext: return "next";
                case Operators.Coerce: return "coerce";
                case Operators.GetMember: return "getmember";
                case Operators.GetBoundMember: return "getbmember";
                case Operators.SetMember: return "setmember";
                case Operators.Unassign: return "unassign";
                case Operators.Missing: return "missing";
                case Operators.AbsoluteValue: return "abs";
                case Operators.ConvertToBigInteger: return "cvtbigint";
                case Operators.ConvertToComplex: return "cvtcomplex";
                case Operators.ConvertToDouble: return "cvtdouble";
                case Operators.ConvertToInt32: return "cvtint32";
                case Operators.ConvertToHex: return "cvthex";
                case Operators.ConvertToOctal: return "cvtoct";
                case Operators.ConvertToBoolean: return "cvtobool";
                case Operators.GetState: return "getstate";
                case Operators.ValueHash: return "#";
                case Operators.RightShiftUnsigned: return ">>>";
                case Operators.InPlaceRightShiftUnsigned: return ">>>=";
                case Operators.ReverseRightShiftUnsigned: return "r>>>=";

                default:
                    //Debug.Fail("unexpected op " + op.ToString());
                    return "unknown op " + op.ToString();
            }
        }

        private bool NotImplemented(Node node) {
            Push("NOT IMPLEMENTED: " + node.GetType().Name);
            Pop();
            return false;
        }
    }
}
#endif