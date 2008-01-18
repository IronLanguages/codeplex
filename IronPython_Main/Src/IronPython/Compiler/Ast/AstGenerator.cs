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
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;
using MSAst = Microsoft.Scripting.Ast;

using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Ast;

    internal class AstGenerator {
        private readonly MSAst.CodeBlock _block;
        private List<MSAst.Variable> _temps;
        private readonly CompilerContext _context;
        private readonly bool _print;
        private int _loopDepth = 0;

        public AstGenerator(MSAst.CodeBlock block, CompilerContext context)
            : this(block, context, false) {
        }

        public AstGenerator(MSAst.CodeBlock block, CompilerContext context, bool print) {
            Assert.NotNull(context);

            _block = block;
            _context = context;
            _print = print;
        }

        public MSAst.CodeBlock Block {
            get { return _block; }
        }

        public CompilerContext Context {
            get { return _context; }
        }

        public bool PrintExpressions {
            get { return _print; }
        }

        public void EnterLoop() {
            _loopDepth++;
        }
        public void ExitLoop() {
            _loopDepth--;
            Debug.Assert(_loopDepth >= 0);
        }

        public bool InLoop {
            get { return _loopDepth > 0; }
        }

        internal static MSAst.CodeBlock TransformAst(CompilerContext context, PythonAst ast) {
            return new AstGenerator(null, context).Transform(ast);
        }

        public void AddError(string message, SourceSpan span) {
            _context.AddError(message, span.Start, span.End, Severity.Error);
        }

        public MSAst.Variable MakeTemp(SymbolId name, Type type) {
            if (_temps != null) {
                foreach (MSAst.Variable temp in _temps) {
                    if (temp.Type == type) {
                        _temps.Remove(temp);
                        return temp;
                    }
                }
            }
            return _block.CreateTemporaryVariable(name, type);
        }


        public MSAst.BoundExpression MakeTempExpression(string name) {
            return MakeTempExpression(name, typeof(object));
        }

        public MSAst.BoundExpression MakeTempExpression(string name, Type type) {
            return Ast.Read(MakeTemp(SymbolTable.StringToId(name), type));
        }

        public void FreeTemp(MSAst.BoundExpression be) {
            FreeTemp(be.Variable);
        }

        public void FreeTemp(MSAst.Variable temp) {
            if (this.Block is MSAst.GeneratorCodeBlock) {
                return;
            }
            if (_temps == null) {
                _temps = new List<MSAst.Variable>();
            }
            _temps.Add(temp);
        }

        internal static MSAst.Expression MakeAssignment(MSAst.Variable variable, MSAst.Expression right) {
            return MakeAssignment(variable, right, SourceSpan.None);
        }

        internal static MSAst.Expression MakeAssignment(MSAst.Variable variable, MSAst.Expression right, SourceSpan span) {
            return Ast.Statement(
                span,
                Ast.Assign(variable, Ast.Convert(right, variable.Type))
            );
        }

        internal static MSAst.Expression ConvertIfNeeded(MSAst.Expression expression, Type type) {
            Debug.Assert(expression != null);
            // Do we need conversion?
            if (!CanAssign(type, expression.Type)) {
                // Add conversion step to the AST
                expression = Ast.Convert(expression, type);
            }
            return expression;
        }

        internal static MSAst.Expression DynamicConvertIfNeeded(MSAst.Expression expression, Type type) {
            Debug.Assert(expression != null);
            // Do we need conversion?
            if (!CanAssign(type, expression.Type)) {
                // Add conversion step to the AST
                expression = Ast.Action.ConvertTo(type, ConversionResultKind.ExplicitCast, expression);
            }
            return expression;
        }

        internal static bool CanAssign(Type to, Type from) {
            return to.IsAssignableFrom(from) && (to.IsValueType == from.IsValueType);
        }

        #region Utility methods

        public MSAst.Expression Transform(Expression from) {
            return Transform(from, typeof(object));
        }

        public MSAst.Expression Transform(Expression from, Type type) {
            if (from != null) {
                return from.Transform(this, type);
            }
            return null;
        }

        public MSAst.Expression TransformAsObject(Expression from) {
            return TransformAndConvert(from, typeof(object));
        }

        public MSAst.Expression TransformAndConvert(Expression from, Type type) {
            if (from != null) {
                MSAst.Expression transformed = from.Transform(this, type);
                transformed = ConvertIfNeeded(transformed, type);
                return transformed;
            }
            return null;
        }

        internal MSAst.Expression TransformOrConstantNull(Expression expression, Type type) {
            if (expression == null) {
                return Ast.Null(type);
            } else {
                return ConvertIfNeeded(expression.Transform(this, type), type);
            }
        }

        public MSAst.Expression TransformAndDynamicConvert(Expression from, Type type) {
            if (from != null) {
                MSAst.Expression transformed = from.Transform(this, type);
                transformed = DynamicConvertIfNeeded(transformed, type);
                return transformed;
            }
            return null;
        }

        public MSAst.Expression Transform(Statement from) {
            if (from == null) {
                return null;
            } else {
                MSAst.Expression expression = from.Transform(this);
                if (expression.Type != typeof(void)) {
                    expression = Ast.Convert(expression, typeof(void));
                }
                return expression;
            }
        }

        internal MSAst.CodeBlock Transform(PythonAst from) {
            if (from == null) {
                return null;
            } else {
                MSAst.CodeBlock ret = from.TransformToAst(this, _context);
                Debug.Assert(_loopDepth == 0);
                return ret;
            }
        }

        internal MSAst.Expression[] Transform(Expression[] expressions) {
            return Transform(expressions, typeof(object));
        }

        internal MSAst.Expression[] Transform(Expression[] expressions, Type type) {
            Debug.Assert(expressions != null);
            MSAst.Expression[] to = new MSAst.Expression[expressions.Length];
            for (int i = 0; i < expressions.Length; i++) {
                Debug.Assert(expressions[i] != null);
                to[i] = Transform(expressions[i], type);
            }
            return to;
        }

        internal MSAst.Expression[] TransformAndConvert(Expression[] expressions, Type type) {
            Debug.Assert(expressions != null);
            MSAst.Expression[] to = new MSAst.Expression[expressions.Length];
            for (int i = 0; i < expressions.Length; i++) {
                Debug.Assert(expressions[i] != null);
                to[i] = TransformAndConvert(expressions[i], type);
            }
            return to;
        }

        internal MSAst.Expression[] Transform(Statement[] from) {
            Debug.Assert(from != null);
            MSAst.Expression[] to = new MSAst.Expression[from.Length];
            for (int i = 0; i < from.Length; i++) {
                Debug.Assert(from[i] != null);
                to[i] = from[i].Transform(this);
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
