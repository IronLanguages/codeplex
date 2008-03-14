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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Contracts;
using System.Security.Permissions;
using System.Threading;

namespace Microsoft.Scripting {
    public sealed class SourceUnit {
        private readonly SourceCodeKind _kind;
        private readonly string _path;
        private readonly LanguageContext/*!*/ _language;
        private readonly TextContentProvider/*!*/ _contentProvider;

        private bool _disableLineFeedLineSeparator;

        // content dependent properties:
        // SourceUnit is serializable => updated properties are not transmitted back to the host unless the unit is passed by-ref
        private SourceCodeProperties? _codeProperties;
        private KeyValuePair<int, int>[] _lineMap;
        private KeyValuePair<int, string>[] _fileMap;

        /// <summary>
        /// Identification of the source unit. Assigned by the host. 
        /// The format and semantics is host dependent (could be a path on file system or URL).
        /// Empty string for anonymous source units.
        /// </summary>
        public string Path {
            get { return _path; }
        }

        public bool HasPath {
            get { return _path != null; }
        }

        public SourceCodeKind Kind {
            get { return _kind; }
        }

        /// <summary>
        /// LanguageContext of the language of the unit.
        /// </summary>
        public LanguageContext/*!*/ LanguageContext {
            get { return _language; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public SourceCodeProperties GetCodeProperties() {
            return GetCodeProperties(_language.GetCompilerOptions());
        }

        public SourceCodeProperties GetCodeProperties(CompilerOptions/*!*/ options) {
            Contract.RequiresNotNull(options, "options");

            _language.ParseSourceCode(new CompilerContext(this, options, ErrorSink.Null));
            return _codeProperties ?? SourceCodeProperties.None;
        }

        public SourceCodeProperties? CodeProperties {
            get { return _codeProperties; }
            set { _codeProperties = value; }
        }

        // TODO: remove, used by Python only
        public bool DisableLineFeedLineSeparator {
            get { return _disableLineFeedLineSeparator; }
            set { _disableLineFeedLineSeparator = value; }
        }

        internal SourceUnit(LanguageContext/*!*/ context, TextContentProvider/*!*/ contentProvider, string path, SourceCodeKind kind) {
            Assert.NotNull(context, contentProvider);
            Debug.Assert(path == null || path.Length > 0);

            _language = context;
            _contentProvider = contentProvider;
            _kind = kind;
            _path = path;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public SourceUnitReader/*!*/ GetReader() {
            return new SourceUnitReader(this, _contentProvider.GetReader());
        }
        
        /// <summary>
        /// Reads specified range of lines (or less) from the source unit. 
        /// Line numbers starts with 1.
        /// </summary>
        public string/*!*/[]/*!*/ GetCodeLines(int start, int count) {
            Contract.Requires(start > 0, "start");
            Contract.Requires(count > 0, "count");

            List<string> result = new List<string>(count);

            using (SourceUnitReader reader = GetReader()) {
                reader.SeekLine(start);
                while (count > 0) {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    result.Add(line);
                    count--;
                }
            }

            return result.ToArray();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetCodeLine(int line) {
            string[] lines = GetCodeLines(line, 1);
            return (lines.Length > 0) ? lines[0] : null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string/*!*/ GetCode() {
            using (SourceUnitReader reader = GetReader()) {
                return reader.ReadToEnd();
            }
        }

        [Confined]
        public override string/*!*/ ToString() {
            return _path ?? String.Empty;
        }

        #region Line/File mapping

        public SourceSpan MapLine(SourceSpan span) {
            return new SourceSpan(MapLine(span.Start), MapLine(span.End));
        }

        public SourceLocation MapLine(SourceLocation loc) {
            return new SourceLocation(loc.Index, MapLine(loc.Line), loc.Column);
        }

        public int MapLine(int line) {
            if (_lineMap != null) {
                int match = BinarySearch(_lineMap, line);
                int delta = line - _lineMap[match].Key;
                return _lineMap[match].Value + delta;
            }

            return line;
        }

        /// <summary>
        /// Returns null if unknown/undefined.
        /// </summary>
        public string GetSymbolDocument(int line) {
            if (_fileMap != null) {
                int match = BinarySearch(_fileMap, line);
                return _fileMap[match].Value;
            }

            return _path;
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


        private class KeyComparer<T1> : IComparer<KeyValuePair<int, T1>> {
            public int Compare(KeyValuePair<int, T1> x, KeyValuePair<int, T1> y) {
                return x.Key - y.Key;
            }
        }

        #endregion

        #region Parsing, Compilation, Execution

        public LambdaExpression Parse(ErrorSink/*!*/ errorSink) {
            return Parse(_language.GetCompilerOptions(), errorSink);
        }

        public LambdaExpression Parse(CompilerOptions/*!*/ options, ErrorSink/*!*/ errorSink) {
            Contract.RequiresNotNull(errorSink, "errorSink");
            Contract.RequiresNotNull(options, "options");

            // TODO: ParseSourceCode can update CompilerContext.Options
            // TODO: Do we really need a compiler context here?
            CompilerContext context = new CompilerContext(this, options, errorSink);
            return _language.ParseSourceCode(context);
        }

        public ScriptCode Compile() {
            return Compile(ErrorSink.Default);
        }

        public ScriptCode Compile(ErrorSink/*!*/ errorSink) {
            return Compile(_language.GetCompilerOptions(), errorSink);
        }

        /// <summary>
        /// Errors are reported to the specified sink. 
        /// Returns <c>null</c> if the parser cannot compile the code due to error(s).
        /// </summary>
        public ScriptCode Compile(CompilerOptions/*!*/ options, ErrorSink/*!*/ errorSink) {
            Contract.RequiresNotNull(errorSink, "errorSink");
            Contract.RequiresNotNull(options, "options");

            LambdaExpression lambda = Parse(options, errorSink);

            // provided sink didn't throw an exception:
            if (lambda == null) {
                return null;
            }

            // TODO: Do we really need a compiler context here?
            CompilerContext context = new CompilerContext(this, options, errorSink);
            return new ScriptCode(lambda, _language, context);
        }

        public object Execute(Scope/*!*/ scope) {
            return Execute(scope, ErrorSink.Default);
        }

        public object Execute(Scope/*!*/ scope, ErrorSink/*!*/ errorSink) {
            Contract.RequiresNotNull(scope, "scope");
            
            ScriptCode compiledCode = Compile(_language.GetCompilerOptions(scope), errorSink);

            if (compiledCode == null) {
                throw new SyntaxErrorException();
            }

            return compiledCode.Run(scope);
        }

        /// <summary>
        /// Executes in an optimized scope.
        /// </summary>
        public object Execute() {
            ScriptCode compiledCode = Compile();
            return compiledCode.Run(compiledCode.MakeOptimizedScope());
        }
        
        /// <summary>
        /// Executes in an optimized scope.
        /// </summary>
        public object Execute(CompilerOptions/*!*/ options, ErrorSink/*!*/ errorSink) {
            ScriptCode compiledCode = Compile(options, errorSink);
            return compiledCode.Run(compiledCode.MakeOptimizedScope());
        }
        
        #endregion

        public void SetLineMapping(KeyValuePair<int, int>[] lineMap) {
            _lineMap = (lineMap == null || lineMap.Length == 0) ? null : lineMap;
        }

        public void SetDocumentMapping(KeyValuePair<int, string/*!*/>[] fileMap) {
            _fileMap = (fileMap == null || fileMap.Length == 0) ? null : fileMap;
        }
    }
}
