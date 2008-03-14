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
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Microsoft.Contracts;
using System.Security.Permissions;
using Microsoft.Scripting.Runtime;
using System.Threading;
using System.Runtime.Remoting;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Hosting counterpart for <see cref="SourceUnit"/>.
    /// </summary>
    public sealed class ScriptSource
#if !SILVERLIGHT
        : MarshalByRefObject 
#endif
    {
        private readonly ScriptEngine/*!*/ _engine;
        private readonly SourceUnit/*!*/ _unit;

        // TODO: make internal as soon as SyntaxErrorException gets fixed
        public SourceUnit/*!*/ SourceUnit { 
            get { return _unit; } 
        } 

        /// <summary>
        /// Identification of the source unit. Assigned by the host. 
        /// The format and semantics is host dependent (could be a path on file system or URL).
        /// <c>null</c> for anonymous script source.
        /// Cannot be an empty string.
        /// </summary>
        public string Path {
            get { return _unit.Path; }
        }

        public bool HasPath {
            get { return _unit.HasPath; }
        }

        public SourceCodeKind Kind {
            get { return _unit.Kind; }
        }

        public ScriptEngine/*!*/ Engine {
            get { return _engine; }
        }

        public SourceCodeProperties? CodeProperties {
            get { return _unit.CodeProperties; }
            set { _unit.CodeProperties = value; } 
        }

        internal ScriptSource(ScriptEngine/*!*/ engine, SourceUnit/*!*/ sourceUnit) {
            Assert.NotNull(engine, sourceUnit);
            _unit = sourceUnit;
            _engine = engine;
        }

        [Confined]
        public override string/*!*/ ToString() {
            return _unit.ToString();
        }

        #region Compilation and Execution

        /// <summary>
        /// Compile the ScriptSource into CompileCode object that can be executed 
        /// repeatedly in its default scope or in other scopes without having to recompile the code.
        /// </summary>
        /// <exception cref="SyntaxErrorException">Code cannot be compiled.</exception>
        public CompiledCode/*!*/ Compile() {
            return CompileInternal(null, null);
        }

        /// <remarks>
        /// Errors are reported to the specified listener. 
        /// Returns <c>null</c> if the parser cannot compile the code due to errors.
        /// </remarks>
        public CompiledCode Compile(ErrorListener/*!*/ errorListener) {
            Contract.RequiresNotNull(errorListener, "errorListener");

            return CompileInternal(null, errorListener);
        }

        /// <remarks>
        /// Errors are reported to the specified listener. 
        /// Returns <c>null</c> if the parser cannot compile the code due to error(s).
        /// </remarks>
        public CompiledCode Compile(CompilerOptions/*!*/ compilerOptions) {
            Contract.RequiresNotNull(compilerOptions, "compilerOptions");

            return CompileInternal(compilerOptions, null);
        }

        /// <remarks>
        /// Errors are reported to the specified listener. 
        /// Returns <c>null</c> if the parser cannot compile the code due to error(s).
        /// </remarks>
        public CompiledCode Compile(CompilerOptions/*!*/ compilerOptions, ErrorListener/*!*/ errorListener) {
            Contract.RequiresNotNull(errorListener, "errorListener");
            Contract.RequiresNotNull(compilerOptions, "compilerOptions");

            return CompileInternal(compilerOptions, errorListener);
        }

        private CompiledCode CompileInternal(CompilerOptions compilerOptions, ErrorListener errorListener) {
            ErrorSink errorSink = new ErrorListenerProxySink(this, errorListener);
            ScriptCode code = _unit.Compile(compilerOptions ?? _unit.LanguageContext.GetCompilerOptions(), errorSink);

            return (code != null) ? new CompiledCode(_engine, code) : null;
        }

        /// <summary>
        /// Execute the ScriptScope in a new scope.
        /// </summary>
        /// <returns></returns>
        public object Execute() {
            return _unit.Execute();
        }
        
        /// <summary>
        /// Execute the ScriptScope.
        /// Returns an object that is the resulting value of running the code.  
        /// 
        /// When the ScriptSource is a file or statement, the engine decides what is 
        /// an appropriate value to return.  Some languages return the value produced 
        /// by the last expression or statement, but languages that are not expression 
        /// based may return null.
        /// </summary>
        /// <exception cref="SyntaxErrorException">Code cannot be compiled.</exception>
        public object Execute(ScriptScope/*!*/ scope) {
            Contract.RequiresNotNull(scope, "scope");

            return _unit.Execute(scope.Scope, ErrorSink.Default);
        }

        /// <remarks>
        /// Converts the result of execution to specified type using language specific conversions.
        /// </remarks>
        public T Execute<T>(ScriptScope/*!*/ scope) {
            return _engine.Operations.ConvertTo<T>(Execute(scope));
        }

#if !SILVERLIGHT
        /// <summary>
        /// Execute the code in the specified ScriptScope and return a result.
        /// 
        /// ExecuteAndGetAsHandle returns an ObjectHandle wrapping the resulting value 
        /// of running the code.  
        /// 
        /// When the ScriptSource is a file or statement, the engine decides what is 
        /// an appropriate value to return.  Some languages return the value produced 
        /// by the last expression or statement, but languages that are not expression 
        /// based may return null.
        /// </summary>
        public ObjectHandle ExecuteAndGetAsHandle(ScriptScope/*!*/ scope) {
            return new ObjectHandle(Execute(scope));
        }
#endif

        /// <summary>
        /// Runs a specified code as if it was a program launched from OS command shell. 
        /// and returns a process exit code indicating the success or error condition 
        /// of executing the code.
        /// 
        /// Exact behavior depends on the language. Some languages have a dedicated "exit" exception that 
        /// carries the exit code, in which case the exception is cought and the exit code is returned.
        /// The default behavior returns the result of program's execution converted to an integer 
        /// using a langauge specific conversion.
        /// </summary>
        /// <exception cref="SyntaxErrorException">Code cannot be compiled.</exception>
        public int ExecuteProgram() {
            return _unit.LanguageContext.ExecuteProgram(_unit);
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public SourceCodeProperties GetCodeProperties() {
            return _unit.GetCodeProperties();
        }

        public SourceCodeProperties GetCodeProperties(CompilerOptions/*!*/ options) {
            return _unit.GetCodeProperties(options);
        }

        // TODO: remove
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public SourceUnitReader/*!*/ GetReader() {
            return _unit.GetReader();
        }
        
        /// <summary>
        /// Reads specified range of lines (or less) from the source unit. 
        /// Line numbers starts with 1.
        /// </summary>
        public string/*!*/[]/*!*/ GetCodeLines(int start, int count) {
            return _unit.GetCodeLines(start, count);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetCodeLine(int line) {
            return _unit.GetCodeLine(line);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string/*!*/ GetCode() {
            return _unit.GetCode();
        }

        public SourceSpan MapLine(SourceSpan span) {
            return _unit.MapLine(span);
        }

        public SourceLocation MapLine(SourceLocation location) {
            return _unit.MapLine(location);
        }

        public int MapLine(int line) {
            return _unit.MapLine(line);
        }

        /// <summary>
        /// Returns null if unknown/undefined.
        /// </summary>
        public string GetSymbolDocument(int line) {
            return _unit.GetSymbolDocument(line);
        }

#if !SILVERLIGHT
        // TODO: Figure out what is the right lifetime
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}
