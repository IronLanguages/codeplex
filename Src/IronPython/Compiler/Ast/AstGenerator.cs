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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using IronPython.Runtime;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Internal.Generation;
using MSAst = Microsoft.Scripting.Internal.Ast;

namespace IronPython.Compiler.Ast {
    public class AstGenerator {
        private readonly MSAst.CodeBlock _block;
        private List<MSAst.VariableReference> _temps;
        private readonly CompilerContext _context;
        private readonly bool _print;

        public AstGenerator(MSAst.CodeBlock block, AstGenerator outer)
            : this(block, outer._context, false) {
        }

        public AstGenerator(MSAst.CodeBlock block, AstGenerator outer, bool print)
            : this(block, outer._context, print) {
        }

        public AstGenerator(MSAst.CodeBlock block, CompilerContext context, bool print) {
            _block = block;
            _context = context;
            _print = print;
        }

        public MSAst.CodeBlock Block {
            get { return _block; }
        } 

        public bool PrintExpressions {
            get { return _print; }
        }

        internal static MSAst.CodeBlock TransformAst(CompilerContext context, PythonAst past, bool print) {
            return new AstGenerator(null, context, print).Transform(past);
        }

        public void AddError(string message, SourceSpan span) {
            _context.AddError(message, span.Start, span.End, Severity.Error);
        }

        public MSAst.VariableReference MakeTemp(SymbolId name, Type type) {
            if (_temps != null) {
                foreach (MSAst.VariableReference temp in _temps) {
                    if (temp.Type == type) {
                        _temps.Remove(temp);
                        return temp;
                    }
                }
            }
            return _block.CreateTemporaryVariable(name, type);
        }

        public MSAst.VariableReference MakeGeneratorTemp(SymbolId name, Type type) {
            return _block.CreateGeneratorTempVariable(name, type);
        }

        public MSAst.BoundExpression MakeTempExpression(string name, SourceSpan span) {
            return MakeTempExpression(name, typeof(object), span);
        }

        public MSAst.BoundExpression MakeTempExpression(string name, Type type, SourceSpan span) {
            return new MSAst.BoundExpression(MakeTemp(SymbolTable.StringToId(name), type), span);
        }

        internal MSAst.BoundExpression MakeGeneratorTempExpression(string name, SourceSpan span) {
            return MakeGeneratorTempExpression(name, typeof(object), span);
        }

        internal MSAst.BoundExpression MakeGeneratorTempExpression(string name, Type type, SourceSpan span) {
            return new MSAst.BoundExpression(MakeGeneratorTemp(SymbolTable.StringToId(name), type), span);
        }

        public void FreeTemp(MSAst.BoundExpression be) {
            FreeTemp(be.Reference);
        }

        public void FreeTemp(MSAst.VariableReference temp) {
            if (_temps == null) {
                _temps = new List<MSAst.VariableReference>();
            }
            _temps.Add(temp);
        }

        internal static MSAst.Statement MakeAssignment(MSAst.VariableReference variable, MSAst.Expression right) {
            return MakeAssignment(variable, right, SourceSpan.None);
        }

        internal static MSAst.Statement MakeAssignment(MSAst.VariableReference variable, MSAst.Expression right, SourceSpan span) {
            return new MSAst.ExpressionStatement(
                new MSAst.BoundAssignment(variable, right, Operators.None, span),
                span
            );
        }

        #region Utility methods

        public MSAst.Expression Transform(Expression from) {
            return from != null ? from.Transform(this) : null;
        }

        public MSAst.Statement Transform(Statement from) {
            return from != null ? from.Transform(this) : null;
        }

        internal MSAst.CodeBlock Transform(PythonAst from) {
            return from != null ? from.TransformToAst(this, _context) : null;
        }

        internal MSAst.Expression TransformOrConstantNull(Expression expression) {
            if (expression == null) {
                return new MSAst.ConstantExpression(null);
            } else {
                return expression.Transform(this);
            }
        }

        internal MSAst.Expression[] Transform(Expression[] from) {
            if (from == null) {
                return null;
            }
            MSAst.Expression[] to = new MSAst.Expression[from.Length];
            for (int i = 0; i < from.Length; i++) {
                to[i] = from[i] != null ? from[i].Transform(this) : null;
            }
            return to;
        }

        internal MSAst.Statement[] Transform(Statement[] from) {
            if (from == null) {
                return null;
            }
            MSAst.Statement[] to = new MSAst.Statement[from.Length];
            for (int i = 0; i < from.Length; i++) {
                to[i] = from[i] != null ? from[i].Transform(this) : null;
            }
            return to;
        }

        internal MSAst.IfStatementTest[] Transform(IfStatementTest[] from) {
            if (from == null) {
                return null;
            }
            MSAst.IfStatementTest[] to = new MSAst.IfStatementTest[from.Length];
            for (int i = 0; i < from.Length; i++) {
                to[i] = from[i] != null ? from[i].Transform(this) : null;
            }
            return to;
        }

        internal MSAst.TryStatementHandler[] Transform(TryStatementHandler[] from) {
            if (from == null) {
                return null;
            }
            MSAst.TryStatementHandler[] to = new MSAst.TryStatementHandler[from.Length];
            for (int i = 0; i < from.Length; i++) {
                to[i] = from[i] != null ? from[i].Transform(this) : null;
            }
            return to;
        }

        internal MSAst.Arg[] Transform(Arg[] from) {
            if (from == null) {
                return null;
            }
            MSAst.Arg[] to = new MSAst.Arg[from.Length];
            for (int i = 0; i < from.Length; i++) {
                to[i] = from[i] != null ? from[i].Transform(this) : null;
            }
            return to;
        }

        #endregion

        /// <summary>
        /// Returns MethodInfo of the Python helper method given its name.
        /// </summary>
        /// <param name="name">Method name to find.</param>
        /// <returns></returns>
        internal static MethodInfo GetHelperMethod(string name) {
            MethodInfo mi = typeof(PythonOps).GetMethod(name);
            Debug.Assert(mi != null, "Missing Python helper: " + name);
            return mi;
        }

        /// <summary>
        /// Returns MethodInfo of the Python helper method given its name and signature.
        /// </summary>
        /// <param name="name">Name of the method to return</param>
        /// <param name="types">Parameter types</param>
        /// <returns></returns>
        internal static MethodInfo GetHelperMethod(string name, params Type[] types) {
            MethodInfo mi = typeof(PythonOps).GetMethod(name, types);
#if DEBUG
            if (mi == null) {
                System.Text.StringBuilder sb = new System.Text.StringBuilder("(");
                for (int i = 0; i < types.Length; i ++) {
                    if (i > 0) sb.Append(", ");
                    sb.Append(types[i].Name);
                }
                sb.Append(")");
                Debug.Assert(mi != null, "Missing Python helper: " + name + sb.ToString());
            }
#endif
            return mi;
        }
    }
}
