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
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Hosting {
    public sealed class SourceUnit 
#if !SILVERLIGHT
        : MarshalByRefObject 
#endif
    {

        private readonly SourceCodeKind _kind;
        private readonly string _id;
        private readonly LanguageContext _context;

        private TextContentProvider _contentProvider;

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
        public string Id {
            get { return _id; }
        }

        // TODO: maybe we could disallow empty id to ensure there is a single invalid value only
        public bool HasPath {
            get { return !String.IsNullOrEmpty(_id); }
        }

        public SourceCodeKind Kind {
            get { return _kind; }
        }

        /// <summary>
        /// LanguageContext of the language of the unit.
        /// 
        /// TODO: Internal
        /// </summary>
        public LanguageContext LanguageContext {
            get { return _context; }
        }

        public Guid LanguageGuid {
            get {
                return _context.LanguageGuid;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public SourceCodeProperties GetCodeProperties() {
            _context.ParseSourceCode(new CompilerContext(this, null, new ErrorSink()));

            return _codeProperties ?? SourceCodeProperties.None;
        }

        public SourceCodeProperties? CodeProperties {
            get { return _codeProperties; }
            set { _codeProperties = value; } 
        }

        public bool DisableLineFeedLineSeparator {
            get { return _disableLineFeedLineSeparator; }
            set { _disableLineFeedLineSeparator = value; }
        }

        #region Construction

        private SourceUnit(LanguageContext context, TextContentProvider contentProvider, string id, SourceCodeKind kind) {
            Assert.NotNull(context, contentProvider);

            _context = context;
            _contentProvider = contentProvider;
            _kind = kind;
            _id = id;
        }

        // move factories to ScriptEngine/ScriptHost?

        internal static SourceUnit/*!*/ Create(LanguageContext context, TextContentProvider contentProvider, string id, SourceCodeKind kind) {
            Contract.RequiresNotNull(context, "context");
            Contract.RequiresNotNull(contentProvider, "contentProvider");

            return new SourceUnit(context, contentProvider, id, kind);
        }

        internal static SourceUnit CreateSnippet(LanguageContext context, string code) {
            return CreateSnippet(context, code, null, SourceCodeKind.Default);
        }

        internal static SourceUnit CreateSnippet(LanguageContext context, string code, SourceCodeKind kind) {
            return CreateSnippet(context, code, null, kind);
        }

        internal static SourceUnit CreateSnippet(LanguageContext context, string code, string id) {
            return CreateSnippet(context, code, id, SourceCodeKind.Default);
        }

        internal static SourceUnit CreateSnippet(LanguageContext context, string code, string id, SourceCodeKind kind) {
            Contract.RequiresNotNull(context, "context");
            Contract.RequiresNotNull(code, "code");

            return Create(context, new SourceStringContentProvider(code), id, kind);
        }

        /// <summary>
        /// Should be called by host only. TODO: move to the ScriptHost?
        /// </summary>
        public static SourceUnit CreateFileUnit(LanguageContext context, string path) {
            return CreateFileUnit(context, path, (Encoding)null);
        }

        public static SourceUnit CreateFileUnit(LanguageContext context, string path, Encoding encoding) {
            Contract.RequiresNotNull(context, "context");
            Contract.RequiresNotNull(path, "path");

            return CreateFileUnit(context, path, encoding, SourceCodeKind.File);
        }

        public static SourceUnit CreateFileUnit(LanguageContext context, string path, Encoding encoding, SourceCodeKind kind) {
            Contract.RequiresNotNull(context, "context");
            Contract.RequiresNotNull(path, "path");

            TextContentProvider provider = new EngineTextContentProvider(context, new FileStreamContentProvider(path), encoding ?? StringUtils.DefaultEncoding);
            return Create(context, provider, path, kind);
        }

        public static SourceUnit CreateFileUnit(LanguageContext context, string path, string content) {
            Contract.RequiresNotNull(context, "context");
            Contract.RequiresNotNull(path, "path");
            Contract.RequiresNotNull(content, "content");

            TextContentProvider provider = new SourceStringContentProvider(content);
            return Create(context, provider, path, SourceCodeKind.File);
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public SourceUnitReader GetReader() {
            return new SourceUnitReader(this, _contentProvider.GetReader());
        }

        public void SetContent(string content) {
            Contract.RequiresNotNull(content, "content");
            _contentProvider = new SourceStringContentProvider(content);
            ContentChanged();
        }

        private void ContentChanged() {
            _codeProperties = null;
            _lineMap = null;
            _fileMap = null;
        }
        
        /// <summary>
        /// Reads specified range of lines (or less) from the source unit. 
        /// Line numbers starts with 1.
        /// </summary>
        public string[] GetCodeLines(int start, int count) {
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
        public string GetCode() {
            using (SourceUnitReader reader = GetReader()) {
                return reader.ReadToEnd();
            }
        }

        [Confined]
        public override string/*!*/ ToString() {
            return _id;
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

            return _id;
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
    }
}
