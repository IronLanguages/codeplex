/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using IronPython;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using Microsoft.IronPythonTools.Intellisense;
using Microsoft.IronPythonTools.Internal;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Intellisense;
using Microsoft.IronStudio.Library.Intellisense;
using Microsoft.PyAnalysis;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Library;
using Microsoft.Scripting.Runtime;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.IronPythonTools.Library {
    /// <summary>
    /// Performs centralized parsing and analysis of Python source code.
    /// </summary>
    [Export(typeof(IPythonAnalyzer))]
    public class PythonAnalyzer : IParser, IAnalyzer<IProjectEntry>, IPythonAnalyzer {
        private readonly ParseQueue _queue;
        private readonly AnalysisQueue<IProjectEntry> _analysisQueue;
        private readonly ScriptEngine _engine;
        private readonly IErrorProviderFactory _squiggleProvider;
        private readonly Dictionary<string, IProjectEntry> _projectFiles;
        private readonly ProjectState _analysisState;
        private bool _implicitProject = true;

        private static PythonOptions EmptyOptions = new PythonOptions();

        [ImportingConstructor]
        public PythonAnalyzer(IPythonRuntimeHost runtimeHost, IErrorProviderFactory errorProvider) {
            _engine = runtimeHost.ScriptEngine;
            _squiggleProvider = errorProvider;

            _queue = new ParseQueue(this);
            _analysisQueue = new AnalysisQueue<IProjectEntry>(this);
            _analysisState = new ProjectState(_engine);
            _projectFiles = new Dictionary<string, IProjectEntry>(StringComparer.OrdinalIgnoreCase);
        }

        public bool ImplicitProject {
            get {
                return _implicitProject;
            }
            set {
                _implicitProject = value;
            }
        }

        public IProjectEntry AnalyzeTextView(ITextView textView) {
            // Get an AnalysisItem for this file, creating one if necessary
            var res = textView.TextBuffer.Properties.GetOrCreateSingletonProperty<IProjectEntry>(() => {
                string path = textView.GetFilePath();
                if (path == null) {
                    return null;
                }

                IProjectEntry entry;
                if (!_projectFiles.TryGetValue(path, out entry)) {
                    var modName = PathToModuleName(path);

                    var initialSnapshot = textView.TextBuffer.CurrentSnapshot;

                    if (textView.TextBuffer.ContentType.IsOfType(PythonCoreConstants.ContentType)) {
                        entry = _analysisState.AddModule(
                            modName,
                            textView.GetFilePath(),
                            new SnapshotCookie(initialSnapshot)
                        );
                    } else if (textView.TextBuffer.ContentType.IsOfType("xaml")) {
                        entry = _analysisState.AddXamlFile(path);
                    } else {
                        return null;
                    }

                    _projectFiles[path] = entry;

                    if (ImplicitProject) {
                        AddImplicitFiles(Path.GetDirectoryName(Path.GetFullPath(path)));
                    }
                }
                
                return entry;
            });

            // kick off initial processing on the ITextWindow
            _queue.EnqueueBuffer(textView);

            return res;
        }

        public IProjectEntry AnalyzeFile(string path) {
            IProjectEntry item;
            if (!_projectFiles.TryGetValue(path, out item)) {
                if (path.EndsWith(".py", StringComparison.OrdinalIgnoreCase)) {
                    var modName = PathToModuleName(path);

                    item = _analysisState.AddModule(
                        modName,
                        path,
                        null
                    );
                } else if (path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase)) {
                    item = _analysisState.AddXamlFile(path);
                }

                if (item != null) {
                    _projectFiles[path] = item;
                }
            }

            _queue.EnqueueFile(path);

            return item;
        }

        private void AddImplicitFiles(string dir) {
            foreach (string filename in Directory.GetFiles(dir, "*.py")) {
                AnalyzeFile(filename);
            }

            foreach (string innerDir in Directory.GetDirectories(dir)) {
                if (File.Exists(Path.Combine(innerDir, "__init__.py"))) {
                    AddImplicitFiles(innerDir);
                }
            }
        }

        public IProjectEntry GetAnalysisFromFile(string path) {
            IProjectEntry res;
            if (_projectFiles.TryGetValue(path, out res)) {
                return res;
            }
            return null;
        }

        internal static string PathToModuleName(string path) {
            string moduleName;
            string dirName;
            
            if (path == null) {
                return String.Empty;
            } else if (path.EndsWith("__init__.py")) {
                moduleName = Path.GetFileName(Path.GetDirectoryName(path));
                dirName = Path.GetDirectoryName(path);
            } else {
                moduleName = Path.GetFileNameWithoutExtension(path);
                dirName = path;
            }
                
            while (dirName.Length != 0 && (dirName = Path.GetDirectoryName(dirName)).Length != 0 &&
                File.Exists(Path.Combine(dirName, "__init__.py"))) {
                moduleName = Path.GetFileName(dirName) + "." + moduleName;
            }

            return moduleName;
        }

        #region IParser Members

        public void Parse(TextContentProvider content) {

            ISnapshotTextContentProvider snapshotContent = content as ISnapshotTextContentProvider;
            if (snapshotContent != null) {
                ParseSnapshot(snapshotContent);
            } else {
                FileTextContentProvider fileContent = content as FileTextContentProvider;
                if (fileContent != null) {
                    ParseFile(fileContent);
                }
            }

        }

        private void ParseFile(FileTextContentProvider fileContent) {
            if (fileContent.Path.EndsWith(".py", StringComparison.OrdinalIgnoreCase)) {
                PythonAst ast;
                CollectingErrorSink errorSink;
                ParsePythonCode(fileContent, out ast, out errorSink);

                if (ast != null) {
                    IProjectEntry analysis;
                    IPythonProjectEntry pyAnalysis;
                    if (fileContent != null &&
                        _projectFiles.TryGetValue(fileContent.Path, out analysis) &&
                        (pyAnalysis = analysis as IPythonProjectEntry) != null) {

                        pyAnalysis.UpdateTree(ast, new FileCookie(fileContent.Path));
                        _analysisQueue.Enqueue(analysis, AnalysisPriority.Normal);
                    }
                }
            } else if (fileContent.Path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase)) {
                IProjectEntry analysis;
                XamlProjectEntry xamlProject;
                if (_projectFiles.TryGetValue(fileContent.Path, out analysis) && 
                    (xamlProject = analysis as XamlProjectEntry) != null) {
                    xamlProject.UpdateContent(fileContent.GetReader(), new FileCookie(fileContent.Path));
                    analysis.Analyze();
                }
            }
        }

        private void ParseSnapshot(ISnapshotTextContentProvider snapshotContent) {
            
            // queue analysis of the parsed tree at High Pri so the active buffer is quickly re-analyzed
            var snapshot = snapshotContent.Snapshot;

            if (snapshot.TextBuffer.ContentType.IsOfType(PythonCoreConstants.ContentType)) {
                PythonAst ast;
                CollectingErrorSink errorSink;
                ParsePythonCode((TextContentProvider)snapshotContent, out ast, out errorSink);
                if (ast != null) {
                    IPythonProjectEntry analysis;
                    if (snapshot.TextBuffer.TryGetPythonAnalysis(out analysis)) {
                        // only update the AST when we're error free, this way we don't remove
                        // a useful analysis with an incomplete and useless analysis.
                        if (errorSink.Errors.Count == 0) {
                            analysis.UpdateTree(ast, new SnapshotCookie(snapshot));
                            _analysisQueue.Enqueue(analysis, AnalysisPriority.High);
                        }

                        SimpleTagger<ErrorTag> squiggles = _squiggleProvider.GetErrorTagger(snapshot.TextBuffer);
                        squiggles.RemoveTagSpans(x => true);

                        // update squiggles for the live buffer
                        foreach (ErrorResult warning in errorSink.Warnings) {
                            var span = warning.Span;
                            var tspan = CreateSpan(snapshot, span);
                            squiggles.CreateTagSpan(tspan, new ErrorTag("Warning", warning.Message));
                        }

                        foreach (ErrorResult error in errorSink.Errors) {
                            var span = error.Span;
                            var tspan = CreateSpan(snapshot, span);
                            squiggles.CreateTagSpan(tspan, new ErrorTag("Error", error.Message));
                        }
                    }
                }
            } else if (snapshot.TextBuffer.ContentType.IsOfType("xaml")) {
                string path = snapshot.TextBuffer.GetFilePath();
                if (path != null) {
                    IProjectEntry analysis;
                    XamlProjectEntry xamlProject;
                    if (_projectFiles.TryGetValue(path, out analysis) &&
                        (xamlProject = analysis as XamlProjectEntry) != null) {
                        xamlProject.UpdateContent(((TextContentProvider)snapshotContent).GetReader(), new SnapshotCookie(snapshotContent.Snapshot));
                        analysis.Analyze();
                    }
                }

            }
        }

        private void ParsePythonCode(TextContentProvider content, out PythonAst ast, out CollectingErrorSink errorSink) {
            ast = null;
            errorSink = new CollectingErrorSink();

            // parse the tree
            var source = _engine.CreateScriptSource(content, "", SourceCodeKind.File);
            var compOptions = (PythonCompilerOptions)HostingHelpers.GetLanguageContext(_engine).GetCompilerOptions();
            var context = new CompilerContext(HostingHelpers.GetSourceUnit(source), compOptions, errorSink);
            //compOptions.Verbatim = true;
            using (var parser = MakeParser(context)) {
                if (parser != null) {
                    try {
                        ast = parser.ParseFile(false);
                    } catch (Exception e) {
                        Debug.Assert(false, String.Format("Failure in IronPython parser: {0}", e.ToString()));
                    }

                }
            }
        }

        private static Parser MakeParser(CompilerContext context) {
            for (int i = 0; i < 10; i++) {
                try {
                    return Parser.CreateParser(context, EmptyOptions);
                } catch (IOException) {
                    // file being copied, try again...
                    Thread.Sleep(100);
                }
            }
            return null;
        }

        private static ITrackingSpan CreateSpan(ITextSnapshot snapshot, SourceSpan span) {
            var tspan = snapshot.CreateTrackingSpan(
                new Span(
                    span.Start.Index,
                    Math.Min(span.End.Index - span.Start.Index, Math.Max(snapshot.Length - span.Start.Index, 0))
                ), 
                SpanTrackingMode.EdgeInclusive
            );
            return tspan;
        }

        #endregion

        #region IAnalyzer<AnalysisItem> Members

        public void Analyze(IProjectEntry content) {
            content.Analyze();
        }

        #endregion

        public bool IsAnalyzing {
            get {
                return _queue.IsParsing || _analysisQueue.IsAnalyzing;
            }
        }
    }

    /// <summary>
    /// Provides access to the python file analysis.
    /// </summary>
    public interface IPythonAnalyzer {
        IProjectEntry AnalyzeTextView(ITextView textView);
        IProjectEntry AnalyzeFile(string path);
        IProjectEntry GetAnalysisFromFile(string filename);
        bool IsAnalyzing { get; }
        bool ImplicitProject { get; set;  }
    }
}
