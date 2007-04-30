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
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Internal.Ast;

namespace Microsoft.Scripting {

    [Flags]
    public enum InteractiveCodeProperties {
        None = 0,
        IsInvalid = 1,
        IsIncompleteToken = 2,
        IsIncompleteStatement = 4,
        IsEmpty = 16
    }

    public static partial class InteractiveCodePropertiesEnum {

        // TODO: extension method
        public static bool IsValidAndComplete(InteractiveCodeProperties props, bool allowIncompleteStatement) {
            return
                (props & InteractiveCodeProperties.IsInvalid) == 0 &&
                (props & InteractiveCodeProperties.IsIncompleteToken) == 0 &&
                (allowIncompleteStatement || (props & InteractiveCodeProperties.IsIncompleteStatement) == 0);
        }
    }

    // TODO: Nessie specific SourceUnit?

    [Serializable]
    public abstract class SourceUnit {

        private bool _isDebuggable;
        private IScriptEngine _engine;

        [NonSerialized]
        private ScriptEngine _localEngine;
        
        
        private string _name;
        private bool _disableLineFeedLineSeparator;
        KeyValuePair<int, int>[] _lineMap;
        KeyValuePair<int, string>[] _fileMap;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public IScriptEngine Engine {
            get { return _engine; }
            set {
                if (value == null) throw new ArgumentNullException("value");
                _engine = value; 
            }
        }

        public bool IsVisibleToDebugger {
            get { return _isDebuggable; }
            set { _isDebuggable = value; }
        }

        protected ScriptEngine LocalEngine {
            get {
                if (_localEngine == null) {
                    _localEngine = RemoteWrapper.TryGetLocal<ScriptEngine>(_engine);
                    // TODO: switch the domain automatically (that would need to change return value of Compile etc)? load engine to the current domain? 
                    if (_localEngine == null) {
                        throw new InvalidOperationException("Cannot use the source unit in this domain");
                    }
                }
                return _localEngine;
            }
        }

        protected SourceUnit(IScriptEngine engine, string name) {
            if (engine == null) throw new ArgumentNullException("engine");

            _engine = engine;
            _name = name ?? "<string>"; // TODO
        }

        public abstract string SymbolDocumentName { get; }
        public abstract SourceUnitReader GetReader();

        public bool DisableLineFeedLineSeparator {
            get { return _disableLineFeedLineSeparator; }
            set { _disableLineFeedLineSeparator = value; }
        }

        public string[] GetCodeLines(bool skipFirstLine) {
            List<string> result = new List<string>();

            using (SourceUnitReader reader = GetReader()) {
                for (; ; ) {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    result.Add(line);
                }
            }

            return result.ToArray();
        }

        public string GetCode() {
            using (SourceUnitReader reader = GetReader()) {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Compiles the source unit using the associated engine.
        /// </summary>
        public CompiledCode Compile() {
            return Compile(null, null);
        }


        /// <summary>
        /// Compiles the source unit using the associated engine.
        /// <c>options</c> can be <c>null</c>.
        /// </summary>
        public CompiledCode Compile(CompilerOptions options) {
            return Compile(options, null);
        }

        /// <summary>
        /// Compiles the source unit using the associated engine.
        /// Associates a new language context with the compiled code.
        /// The compilation is performed in the AppDomain of the engine associated with the unit.
        /// Use <c>ScriptEnvironmnent.CompileSourceUnit</c> to compile units in different domain.
        /// <c>options</c> can be <c>null</c>.
        /// <c>errroSink</c> can be <c>null</c>.
        /// </summary>
        public CompiledCode Compile(CompilerOptions options, ErrorSink errorSink) {
            if (options == null) options = LocalEngine.GetDefaultCompilerOptions();
            if (errorSink == null) errorSink = LocalEngine.GetDefaultErrorSink();

            CompilerContext compiler_context = new CompilerContext(this, options, errorSink);
            
            CodeBlock block = Parse(compiler_context);
            ScriptCode code = LocalEngine.GetLanguageContext(options).CompileAst(compiler_context, block);

            return new CompiledCode(code);
        }

        /// <summary>
        /// Compiles the source file unit into an optimized module.
        /// </summary>
        public ScriptModule CompileToModule() {
            return CompileToModule(null, null);
        }

        /// <summary>
        /// Compiles the source file unit into an optimized module.
        /// </summary>
        /// <c>options</c> can be <c>null</c>.
        /// <c>errorSink</c> can be <c>null</c>.
        public ScriptModule CompileToModule(CompilerOptions options, ErrorSink errorSink) {
            return ScriptDomainManager.CurrentManager.CreateModule(Name, ScriptCode.FromCompiledCode(this.Compile(options, errorSink)));
        }

        #region Line/File mapping
        public SourceSpan MapLine(SourceSpan span) {
            return new SourceSpan(MapLine(span.Start), MapLine(span.End));
        }

        public SourceLocation MapLine(SourceLocation loc) {
            return new SourceLocation(loc.Index, MapLine(loc.Line), loc.Column);
        }

        public int MapLine(int line) {
            if (null != _lineMap) {
                int match = BinarySearch(_lineMap, line);
                int delta = line - _lineMap[match].Key;
                return _lineMap[match].Value + delta;
            }

            return line;
        }

        public string GetSymbolDocument(SourceLocation loc) {
            return GetSymbolDocument(loc.Line);
        }

        public string GetSymbolDocument(int line) {
            if (null != _fileMap) {
                int match = BinarySearch(_fileMap, line);
                return _fileMap[match].Value;
            }

            return SymbolDocumentName;
        }

        private int BinarySearch<T>(KeyValuePair<int, T>[] array, int line) {
            int match = Array.BinarySearch(array, new KeyValuePair<int, T>(line, default(T)), new KeyComparer<T>());
            if (match < 0) {
                // If we couldn't find an exact match for this line number, get the nearest
                // matching line number less than this one
                match = ~match - 1;

                // If our index = -1, it means that this line is before any line numbers that
                // we know about. If that's the case, use the first entry in the list
                if (match == -1) {
                    match = 0;
                }
            }
            return match;
        }

        public void SetLineMapping(KeyValuePair<int, int>[] lineMap) {
            // implementation detail: so we don't always have to check for null and empty
            _lineMap = (lineMap.Length == 0) ? null : lineMap;
        }

        public void SetDocumentMapping(KeyValuePair<int, string>[] fileMap) {
            _fileMap = (fileMap.Length == 0) ? null : fileMap;
        }

        class KeyComparer<T1> : IComparer<KeyValuePair<int, T1>> {
            public int Compare(KeyValuePair<int, T1> x, KeyValuePair<int, T1> y) {
                return x.Key - y.Key;
            }
        }

        #endregion

        protected abstract CodeBlock Parse(CompilerContext compilerContext);

    }

    /// <summary>
    /// Represents a source code written in a specified language. 
    /// The code parsing starts by default from the "statement" non-terminal 
    /// (the language defines what that non-terminal precisely means) 
    /// and ends when the entire unit is parsed. The derived source units can override this behavior.
    /// (TODO: better intruduce another source unit and make this abstract).
    /// </summary>
    [Serializable]
    public class SourceCodeUnit : SourceUnit {

        private string _code;

        public override string SymbolDocumentName {
            get { throw new InvalidOperationException(Resources.Unit_NotDebuggerVisible); }
        }

        protected string Code {
            get { return _code; }
            set { _code = value; }
        }

        public SourceCodeUnit(IScriptEngine engine, string code)
            : this(engine, code, null) {
        }

        public SourceCodeUnit(IScriptEngine engine, string code, string name)
            : base(engine, name) {
            if (code == null) throw new ArgumentNullException("code");

            _code = code;
        }

        public override SourceUnitReader GetReader() {
            return new SourceUnitReader(this, new StringReader(_code), Encoding.Unicode);
        }

        protected override CodeBlock Parse(CompilerContext compilerContext) {
            Debug.Assert(compilerContext != null);
            return LocalEngine.Compiler.ParseFile(compilerContext);            
        }

        // TODO:
        internal CompiledCode CompileInteractive(ScriptModule module, CompilerOptions options, ErrorSink errorSink, bool allowIncomplete, out InteractiveCodeProperties properties) {
            CompilerContext compilerContext;
            CodeBlock ast = ParseInteractive(options, errorSink, allowIncomplete, out properties, out compilerContext);
            return (ast != null) ? new CompiledCode(LocalEngine.GetLanguageContext(module).CompileAst(compilerContext, ast)) : null;
        }

        // TODO:
        public InteractiveCodeProperties GetInteractiveCodeProperties(CompilerOptions options, ErrorSink errorSink) {
            InteractiveCodeProperties result;
            CompilerContext compilerContext;
            ParseInteractive(options, errorSink, true, out result, out compilerContext);
            return result;
        }

        internal CodeBlock ParseInteractive(CompilerOptions options, ErrorSink errorSink, bool allowIncomplete, 
            out InteractiveCodeProperties properties, out CompilerContext compilerContext) {

            if (options == null) options = LocalEngine.GetDefaultCompilerOptions();
            if (errorSink == null) errorSink = LocalEngine.GetDefaultErrorSink();

            compilerContext = new CompilerContext(this, options, errorSink);
            return LocalEngine.Compiler.ParseInteractiveCode(compilerContext, allowIncomplete, out properties);
        }
    }

    /// <summary>
    /// Represents source code of an expression written in a specified language.
    /// The code parsing starts from the "expression" non-terminal
    /// (the language defines what that non-terminal precisely means)  and only a single expression is read.
    /// </summary>
    [Serializable]
    public class ExpressionSourceCode : SourceCodeUnit {
        public ExpressionSourceCode(IScriptEngine engine, string code)
            : base(engine, code) {

        }
        public ExpressionSourceCode(IScriptEngine engine, string code, string name)
            : base(engine, code, name) {

        }

        protected override CodeBlock Parse(CompilerContext compilerContext) {
            return LocalEngine.Compiler.ParseExpressionCode(compilerContext);
        }
    }

    /// <summary>
    /// Represents source code of an statement written in a specified language.
    /// The code parsing starts from the "statement" non-terminal 
    /// (the language defines what that non-terminal precisely means) and only a single statement is read.
    /// </summary>
    [Serializable]
    public class StatementSourceCode : SourceCodeUnit {
        public StatementSourceCode(IScriptEngine engine, string code)
            : base(engine, code) {

        }
        
        public StatementSourceCode(IScriptEngine engine, string code, string name)
            : base(engine, code, name) {

        }

        protected override CodeBlock Parse(CompilerContext compilerContext) {
            return LocalEngine.Compiler.ParseStatementCode(compilerContext);
        }
    }

    /// <summary>
    /// Represents a source code that is logically a content of a file (the file needn't exist though).
    /// The code parsing starts from the initial state of the parser.
    /// </summary>
    [Serializable]
    public class SourceFileUnit : SourceUnit {

        // TODO: enforce normalized path?
        private readonly string _path;
        private readonly string _textualContent;
        private readonly byte[] _binaryContent;
        private readonly Encoding _defaultEncoding;

        public string Path {
            get { return _path; }
        }

        public override string SymbolDocumentName {
            get { return _path; }
        }

        #region Construction

        public SourceFileUnit(IScriptEngine engine, string path, string name, string content)
            : base(engine, name) {
            if (path == null) throw new ArgumentNullException("path");
            if (content == null) throw new ArgumentNullException("content");

            _path = path;
            _textualContent = content;
            _binaryContent = null;
            _defaultEncoding = null;
            IsVisibleToDebugger = true;
        }

        public SourceFileUnit(IScriptEngine engine, string path, string name, byte[] content, Encoding encoding)
            : base(engine, name) {
            if (path == null) throw new ArgumentNullException("path");
            if (content == null) throw new ArgumentNullException("content");
            if (encoding == null) throw new ArgumentNullException("encoding");

            _path = path;
            _textualContent = null;
            _binaryContent = content;
            _defaultEncoding = encoding;
            IsVisibleToDebugger = true;
        }

        public SourceFileUnit(IScriptEngine engine, string path, Encoding encoding)
            : this(engine, path, System.IO.Path.GetFileNameWithoutExtension(path), encoding) {
        }
        
        public SourceFileUnit(IScriptEngine engine, string path, string name, Encoding encoding)
            : base(engine, name) {
            if (path == null) throw new ArgumentNullException("path");
            if (encoding == null) throw new ArgumentNullException("encoding");

            _path = path;
            _textualContent = null;
            _binaryContent = null;
            _defaultEncoding = encoding;
            IsVisibleToDebugger = true;
        }

        #endregion

        #region Reader

        public sealed override SourceUnitReader GetReader() {
            Debug.Assert(_textualContent != null || ((_binaryContent != null || _path != null) && _defaultEncoding != null));

            if (_textualContent != null) {
                return new SourceUnitReader(this, new StringReader(_textualContent), Encoding.Unicode);
            }

            Stream stream;
            TextReader reader;
            Encoding encoding = _defaultEncoding;

            if (_binaryContent != null) {
                stream = new MemoryStream(_binaryContent);
            } else {
                stream = OpenStream();
            }

            if (stream != null) {
                reader = Engine.GetSourceReader(stream, ref encoding);
            } else
                reader = OpenReader();

            if (reader == null) {
                throw new InvalidImplementationException();
            }

            return new SourceUnitReader(this, reader, encoding);
        }

        protected virtual Stream OpenStream() {
            return ScriptDomainManager.CurrentManager.PAL.OpenInputFileStream(_path);
        }

        protected virtual TextReader OpenReader() {
            return null;

        }

        #endregion

        protected override CodeBlock Parse(CompilerContext compilerContext) {
            Debug.Assert(compilerContext != null);
            return LocalEngine.Compiler.ParseFile(compilerContext);          
        }

        public override bool Equals(object obj) {
            SourceFileUnit sfu = obj as SourceFileUnit;
            if (sfu != null) {
                return _path == sfu.Path && _defaultEncoding == sfu._defaultEncoding;
            }
            return false;
        }

        public override int GetHashCode() {
            return _path.GetHashCode() ^ ((_defaultEncoding != null) ? _defaultEncoding.GetHashCode() : 0);
        }
    }
}
