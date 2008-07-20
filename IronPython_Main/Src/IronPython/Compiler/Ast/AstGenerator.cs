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
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Compilers;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    internal class AstGenerator {
        private readonly LambdaBuilder _block;                 // the DLR lambda that we are building
        private List<MSAst.VariableExpression> _temps;               // temporary variables allocated against the lambda so we can re-use them
        private readonly CompilerContext _context;                   // compiler context (source unit, etc...) that we are compiling against
        private readonly bool _print;                                // true if we should print expression statements
        private readonly Stack<MSAst.LabelTarget> _loopStack;        // the current stack of loops labels for break/continue
        private MSAst.VariableExpression _lineNoVar, _lineNoUpdated; // the variable used for storing current line # and if we need to store to it
        private int? _curLine;                                       // tracks what the current line we've emitted at code-gen time
        private readonly bool _generator;                            // true if we're transforming for a generator functino
        private MSAst.ParameterExpression _generatorParameter;       // the extra parameter receiving the instance of PythonGenerator

        private AstGenerator(MSAst.Annotations annotations, string name, bool generator, bool print) {
            _loopStack = new Stack<MSAst.LabelTarget>();
            _print = print;
            _generator = generator;

            _block = AstUtils.Lambda(typeof(object), name, annotations);
        }

        internal AstGenerator(AstGenerator parent, MSAst.Annotations annotations, string name, bool generator, bool print)
            : this(annotations, name, generator, print) {
            Assert.NotNull(parent);
            _context = parent.Context;
        }

        internal AstGenerator(CompilerContext context, MSAst.Annotations annotations, string name, bool generator, bool print)
            : this(annotations, name, generator, print) {
            Assert.NotNull(context);
            _context = context;
        }

        public bool Optimize {
            get { return PythonContext.PythonOptions.Optimize; }
        }

        public bool StripDocStrings {
            get { return PythonContext.PythonOptions.StripDocStrings; }
        }

        public bool DebugMode {
            get { return _context.SourceUnit.LanguageContext.DomainManager.Configuration.DebugMode; }
        }

        public PythonDivisionOptions DivisionOptions {
            get {
                return PythonContext.PythonOptions.DivisionOptions;
            }
        }

        private PythonContext/*!*/ PythonContext {
            get {
                return ((PythonContext)_context.SourceUnit.LanguageContext);
            }
        }

        public LambdaBuilder Block {
            get { return _block; }
        }

        public CompilerContext Context {
            get { return _context; }
        }

        public ActionBinder Binder {
            get { return _context.SourceUnit.LanguageContext.Binder; }
        }

        public bool PrintExpressions {
            get { return _print; }
        }

        internal bool IsGenerator {
            get { return _generator; }
        }

        internal MSAst.ParameterExpression GeneratorParameter {
            get { return _generatorParameter; }
        }

        public MSAst.LabelTarget EnterLoop() {
            MSAst.LabelTarget label = Ast.Label();
            _loopStack.Push(label);
            return label;
        }

        public void ExitLoop() {
            _loopStack.Pop();
        }

        public bool InLoop {
            get { return _loopStack.Count > 0; }
        }

        public MSAst.LabelTarget LoopLabel {
            get { return _loopStack.Peek(); }
        }

        public void AddError(string message, SourceSpan span) {
            // TODO: error code
            _context.Errors.Add(_context.SourceUnit, message, span, -1, Severity.Error);
        }

        public MSAst.VariableExpression MakeTemp(SymbolId name, Type type) {
            if (_temps != null) {
                foreach (MSAst.VariableExpression temp in _temps) {
                    if (temp.Type == type) {
                        _temps.Remove(temp);
                        return temp;
                    }
                }
            }
            return _block.HiddenVariable(type, SymbolTable.IdToString(name));
        }


        public MSAst.VariableExpression MakeTempExpression(string name) {
            return MakeTempExpression(name, typeof(object));
        }

        public MSAst.VariableExpression MakeTempExpression(string name, Type type) {
            return MakeTemp(SymbolTable.StringToId(name), type);
        }

        public void FreeTemp(MSAst.VariableExpression temp) {
            if (IsGenerator) {
                return;
            }

            if (_temps == null) {
                _temps = new List<MSAst.VariableExpression>();
            }
            _temps.Add(temp);
        }

        internal static MSAst.Expression MakeAssignment(MSAst.VariableExpression variable, MSAst.Expression right) {
            return MakeAssignment(variable, right, SourceSpan.None);
        }

        internal static MSAst.Expression MakeAssignment(MSAst.VariableExpression variable, MSAst.Expression right, SourceSpan span) {
            return AstUtils.Assign(variable, Ast.Convert(right, variable.Type), span);
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

        internal MSAst.Expression DynamicConvertIfNeeded(MSAst.Expression expression, Type type) {
            Debug.Assert(expression != null);
            // Do we need conversion?
            if (!CanAssign(type, expression.Type)) {
                // Add conversion step to the AST
                expression = AstUtils.ConvertTo(Binder, type, ConversionResultKind.ExplicitCast, Ast.CodeContext(), expression);
            }
            return expression;
        }

        internal static bool CanAssign(Type to, Type from) {
            return to.IsAssignableFrom(from) && (to.IsValueType == from.IsValueType);
        }

        public string GetDocumentation(Statement stmt) {
            if (StripDocStrings) {
                return null;
            }

            return stmt.Documentation;
        }

        #region Dynamic stack trace support

        /// <summary>
        /// A temporary variable to track the current line number
        /// </summary>
        internal MSAst.VariableExpression LineNumberExpression {
            get {
                if (_lineNoVar == null) {
                    _lineNoVar = _block.HiddenVariable(typeof(int), "$lineNo");
                }

                return _lineNoVar;
            }
        }

        /// <summary>
        /// A temporary variable to track if the current line number has been emitted via the fault update block.
        /// 
        /// For example consider:
        /// 
        /// try:
        ///     raise Exception()
        /// except Exception, e:
        ///     # do something here
        ///     raise
        ///     
        /// At "do something here" we need to have already emitted the line number, when we re-raise we shouldn't add it 
        /// again.  If we handled the exception then we should have set the bool back to false.
        /// 
        /// We also sometimes directly check _lineNoUpdated to avoid creating this unless we have nested exceptions.
        /// </summary>
        internal MSAst.VariableExpression LineNumberUpdated {
            get {
                if (_lineNoUpdated == null) {
                    _lineNoUpdated = _block.HiddenVariable(typeof(bool), "$lineUpdated");
                }

                return _lineNoUpdated;
            }
        }

        /// <summary>
        /// Wraps the body of a statement which should result in a frame being available during
        /// exception handling.  This ensures the line number is updated as the stack is unwound.
        /// </summary>
        internal MSAst.Expression WrapScopeStatements(MSAst.Expression body) {
            if (_lineNoVar == null) {
                // we have nothing that can throw, so don't emit the fault block at all
                return body;
            }

            return Ast.Try(
                body
            ).Fault(
                GetUpdateTrackbackExpression()
            );
        }

        /// <summary>
        /// Gets the expression for updating the dynamic stack trace at runtime when an
        /// exception is thrown.
        /// </summary>
        internal MSAst.Expression GetUpdateTrackbackExpression() {
            if (_lineNoUpdated == null) {
                return Ast.Call(
                    typeof(ExceptionHelpers).GetMethod("UpdateStackTrace"),
                    Ast.CodeContext(),
                    Ast.Call(typeof(MethodBase).GetMethod("GetCurrentMethod")),
                    Ast.Constant(_block.Name),
                    Ast.Constant(Context.SourceUnit.Path ?? "<string>"),
                    LineNumberExpression
                );
            }

            return GetLineNumberUpdateExpression();
        }

        internal MSAst.Expression GetLineNumberUpdateExpression() {
            return GetLineNumberUpdateExpression(true);
        }

        /// <summary>
        /// Gets the expression for the actual updating of the line number for stack traces to be available
        /// </summary>
        internal MSAst.Expression GetLineNumberUpdateExpression(bool preventAdditionalAdds) {
            return Ast.Block(
                Ast.If(
                    Ast.Not(
                        LineNumberUpdated
                    ),
                    AstUtils.Call(
                        typeof(ExceptionHelpers).GetMethod("UpdateStackTrace"), 
                        SourceSpan.None, Ast.CodeContext(), 
                        Ast.Call(typeof(MethodBase).GetMethod("GetCurrentMethod")), 
                        Ast.Constant(_block.Name), 
                        Ast.Constant(Context.SourceUnit.Path ?? "<string>"), 
                        LineNumberExpression
                    )
                ),
                AstUtils.Assign(
                    LineNumberUpdated, 
                    Ast.Constant(preventAdditionalAdds), 
                    SourceSpan.None
                )
            );
        }

        #endregion

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
                return TransformWithLineNumberUpdate(from);                
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

                to[i] = TransformWithLineNumberUpdate(from[i]);
            }
            return to;
        }

        private MSAst.Expression TransformWithLineNumberUpdate(Statement fromStmt) {
            // add line number tracking when the line changes...  First we see if the
            // line number changes and then we transform the body.  This prevents the body
            // from updating the line info first.
            bool updateLine = false;

            if (fromStmt.CanThrow &&        // don't need to update line tracking for statements that can't throw
                ((_curLine.HasValue && fromStmt.Start.IsValid && _curLine.Value != fromStmt.Start.Line) ||  // don't need to update unless line has changed
                (!_curLine.HasValue && fromStmt.Start.IsValid))) {  // do need to update if we don't yet have a valid line

                _curLine = fromStmt.Start.Line;
                updateLine = true;
            }

            MSAst.Expression toExpr = fromStmt.Transform(this);

            if (toExpr != null && updateLine) {
                toExpr = Ast.Block(
                    Ast.Assign(
                        LineNumberExpression,
                        Ast.Constant(fromStmt.Start.Line)
                    ),
                    toExpr
                );
            }

            return toExpr;
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
                for (int i = 0; i < types.Length; i++) {
                    if (i > 0) sb.Append(", ");
                    sb.Append(types[i].Name);
                }
                sb.Append(")");
                Debug.Assert(mi != null, "Missing Python helper: " + name + sb.ToString());
            }
#endif
            return mi;
        }

        internal void CreateGeneratorParameter() {
            Debug.Assert(IsGenerator);
            Debug.Assert(_generatorParameter == null);
            _generatorParameter = Block.CreateHiddenParameter("$generator", typeof(PythonGenerator));
        }
    }
}
