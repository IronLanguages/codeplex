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
using System.IO;
using System.Scripting;
using System.Scripting.Utils;
using System.Threading;
using System.Windows;
using System.Windows.Resources;
using System.Xml;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting.Silverlight {

    /// <summary>
    /// The entry point for dynamic language applications
    /// It is a static class that exists to bootstrap the DLR, and start running the application
    /// Also contains helper APIs. These can be accessed by using:
    /// 
    ///   System.Windows.Application.Current
    ///   
    /// ... which returns the global instance of DynamicApplication
    /// </summary>
    public class DynamicApplication : Application {

        #region public properties

        /// <summary>
        /// Returns the entry point file
        /// </summary>
        public string EntryPoint {
            get { return _entryPoint; }
        }

        /// <summary>
        /// Determines if we emit optimized code, and whether turn on debugging features
        /// </summary>
        public bool Debug {
            get { return _debug; }
            set { _debug = value; }
        }

        /// <summary>
        /// Returns the "initParams" argument passed to the Silverlight control
        /// (otherwise would be inaccessible because the DLR host consumes them)
        /// </summary>
        public IDictionary<string, string> InitParams {
            get { return _initParams; }
        }

        /// <summary>
        /// Returns the instance of the DynamicApplication.
        /// Importantly, this method works if not on the UI thread, unlike
        /// Application.Current
        /// </summary>
        public static new DynamicApplication Current {
            get { return _Current; }
        }
        
        /// <summary>
        /// Indicates whether we report unhandled errors to the HTML page
        /// </summary>
        public bool ReportUnhandledErrors {
            get { return _reportErrors; }
            set {
                if (value != _reportErrors) {
                    _reportErrors = value;
                    if (_reportErrors) {
                        Application.Current.UnhandledException += OnUnhandledException;
                    } else {
                        Application.Current.UnhandledException -= OnUnhandledException;
                    }
                }
            }
        }

        /// <summary>
        /// Indicates what HTML element errors should be reported into.
        /// </summary>
        public string ErrorTargetID {
            get { return _errorTargetID; }
            set { _errorTargetID = value; }
        }

        #endregion

        #region public helper APIs

        // these are instance methods so you can do Application.Current.TheMethod(...)

        public DependencyObject LoadRootVisual(UIElement root, Uri uri) {
            Application.LoadComponent(root, uri);
            RootVisual = root;
            return root;
        }

        public DependencyObject LoadRootVisual(UIElement root, string uri) {
            return LoadRootVisual(root, MakeUri(uri));
        }

        public void LoadComponent(object component, string uri) {
            LoadComponent(component, MakeUri(uri));
        }

        /// <summary>
        /// Makes a Uri object that is relative to the location of the source file itself
        /// </summary>
        public Uri MakeUri(string relativeUri) {
            // Get the source file location so we can make the URI relative to the executing source file
            string baseUri = Path.GetDirectoryName(_entryPoint);
            if (baseUri != "") baseUri += "/";
            return new Uri(baseUri + relativeUri, UriKind.Relative);
        }

        #endregion

        #region implementation 

        private string _entryPoint;
        private bool _debug;
        private bool _exceptionDetail;
        private bool _reportErrors;
        private string _errorTargetID;
        private IDictionary<string, string> _initParams;

        private static int _UIThreadId;

        // we need to store this because we can't access Application.Current
        // if we're not on the UI thread
        private static volatile DynamicApplication _Current;

        private const string _DefaultEntryPoint = "app";
        private const string _LanguagesConfigFile = "languages.config";
        private ScriptRuntime _env;

        internal static bool InUIThread {
            get { return _UIThreadId == Thread.CurrentThread.ManagedThreadId; }
        }

        internal bool ExceptionDetail {
            get { return _exceptionDetail; }
        }

        /// <summary>
        /// Called by Silverlight host when it instantiates our application
        /// </summary>
        public DynamicApplication() {
            if (_Current != null) {
                throw new Exception("Only one instance of DynamicApplication can be created");
            }

            _Current = this;
            _UIThreadId = Thread.CurrentThread.ManagedThreadId;

            Startup += new StartupEventHandler(DynamicApplication_Startup);
        }

        #region XAP downloading APIs

        internal static string DownloadContents(string relativePath) {
            return DownloadContents(new Uri(NormalizePath(relativePath), UriKind.Relative));
        }

        internal static string DownloadContents(Uri relativeUri) {
            Stream stream = Download(relativeUri);
            if (stream == null) {
                return null;
            }

            string result;
            using (StreamReader sr = new StreamReader(stream)) {
                result = sr.ReadToEnd();
            }
            return result;
        }

        internal static Stream Download(string relativePath) {
            return Download(new Uri(NormalizePath(relativePath), UriKind.Relative));
        }

        internal static Stream Download(Uri relativeUri) {
            StreamResourceInfo sri = Application.GetResourceStream(relativeUri);
            return (sri != null) ? sri.Stream : null;
        }

        internal static string NormalizePath(string path) {
            // files are stored in the XAP using forward slashes
            return path.Replace(Path.DirectorySeparatorChar, '/');
        }

        #endregion


        void DynamicApplication_Startup(object sender, StartupEventArgs e) {
            // Turn error reporting on while we parse initParams.
            // (Otherwise, we would silently fail if the initParams has an error)
            ReportUnhandledErrors = true;

            ParseArguments(e.InitParams);
            ScriptRuntimeSetup setup = ParseConfigurationFile();

            InitializeDLR(setup);

            StartMainProgram();
        }

        private void InitializeDLR(ScriptRuntimeSetup setup) {
            setup.HostType = typeof(BrowserScriptHost);
            _env = ScriptRuntime.Create(setup);

            _env.LoadAssembly(GetType().Assembly); // to expose our helper APIs

            // Add default references to Silverlight platform DLLs
            // (Currently we auto reference CoreCLR, UI controls, browser interop, and networking stack.)
            foreach (string name in new string[] { "mscorlib", "System", "System.Windows", "System.Windows.Browser", "System.Net" }) {
                _env.LoadAssembly(BrowserPAL.PAL.LoadAssembly(name));
            }
        }

        private void StartMainProgram() {
            string code = DownloadEntryPoint();

            _env.GlobalOptions.DebugMode = _debug;

            ScriptEngine engine = _env.GetEngineByFileExtension(Path.GetExtension(_entryPoint));

            ScriptSource sourceCode = engine.CreateScriptSourceFromString(code, _entryPoint, SourceCodeKind.File);
            SourceCache.Add(sourceCode);

            // Create a new script module & execute the code.
            // It's important to use optimized scopes,
            // which are ~4x faster on benchmarks that make heavy use of top-level functions/variables.
            sourceCode.Compile(new ErrorFormatter.Sink()).Execute();
        }

        private string DownloadEntryPoint() {
            string code = null;

            if (_entryPoint == null) {
                // try default entry point name w/ all extensions
                
                foreach (string ext in _env.GetRegisteredFileExtensions()) {
                    string file = _DefaultEntryPoint + ext;
                    string contents = DownloadContents(file);
                    if (contents != null) {
                        if (_entryPoint != null) {
                            throw new ApplicationException(string.Format("Application can only have one entry point, but found two: {0}, {1}", _entryPoint, file));
                        }
                        _entryPoint = file;
                        code = contents;
                    }
                }

                if (code == null) {
                    throw new ApplicationException(string.Format("Application must have an entry point called {0}.*, where * is the language's extension", _DefaultEntryPoint));
                }
                return code;                
            }

            // if name was supplied just download it
            code = DownloadContents(_entryPoint);
            if (code == null) {
                throw new ApplicationException(string.Format("Could not find the entry point file {0} in the XAP", _entryPoint));
            }
            return code;
        }

        #endregion

        #region Error handling

        private void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs args) {
            args.Handled = true;
            ErrorFormatter.DisplayError(_errorTargetID, args.ExceptionObject);
        }

        #endregion

        #region arguments and configuration file processing

        private void ParseArguments(IDictionary<string, string> args) {
            // save off the initParams (otherwise user code couldn't access it)
            // also, normalize initParams because otherwise it preserves whitespace, which is not very useful
            _initParams = new Dictionary<string, string>(args.Count);
            foreach (KeyValuePair<string, string> pair in args) {
                _initParams[pair.Key.Trim()] = pair.Value.Trim();
            }

            _initParams.TryGetValue("start", out _entryPoint);

            string debug;
            if (_initParams.TryGetValue("debug", out debug)) {
                if (!bool.TryParse(debug, out _debug)) {
                    throw new ArgumentException("You must set 'debug' to 'true' or 'false', for example: initParams: \"..., debug=true\"");
                }
            }

            string exceptionDetail;
            if (_initParams.TryGetValue("exceptionDetail", out exceptionDetail)) {
                if (!bool.TryParse(exceptionDetail, out _exceptionDetail)) {
                    throw new ArgumentException("You must set 'exceptionDetail' to 'true' or 'false', for example: initParams: \"..., exceptionDetail=true\"");
                }
            }

            string reportErrorsDiv;
            if (_initParams.TryGetValue("reportErrors", out reportErrorsDiv)) {
                _errorTargetID = reportErrorsDiv;
                ReportUnhandledErrors = true;
            } else {
                // if reportErrors is unspecified, set to false
                ReportUnhandledErrors = false;
            }
        }

        private ScriptRuntimeSetup ParseConfigurationFile() {
            ScriptRuntimeSetup result = new ScriptRuntimeSetup(true);
            Stream configFile = Download(_LanguagesConfigFile);
            if (configFile == null) {
                return result;
            }

            List<LanguageProviderSetup> langs = new List<LanguageProviderSetup>();
            try {
                XmlReader reader = XmlReader.Create(configFile);
                reader.MoveToContent();
                if (!reader.IsStartElement("Languages")) {
                    throw new ConfigFileException("expected 'Configuration' root element");
                }

                while (reader.Read()) {
                    if (reader.NodeType != XmlNodeType.Element || reader.Name != "Language") {
                        continue;
                    }
                    string context = null, assembly = null, exts = null;
                    while (reader.MoveToNextAttribute()) {
                        switch (reader.Name) {
                            case "languageContext":
                                context = reader.Value;
                                break;
                            case "assembly":
                                assembly = reader.Value;
                                break;
                            case "extensions":
                                exts = reader.Value;
                                break;
                        }
                    }

                    if (context == null || assembly == null || exts == null) {
                        throw new ConfigFileException("expected 'Language' element to have attributes 'languageContext', 'assembly', 'extensions'");
                    }

                    langs.Add(new LanguageProviderSetup(context, assembly, exts.Split(',')));
                }
            } catch (ConfigFileException cfe) {
                throw cfe;
            } catch (Exception ex) {
                throw new ConfigFileException(ex.Message, ex);
            }

            if (langs.Count > 0) {
                result.LanguageProviders = ArrayUtils.AppendRange(result.LanguageProviders, langs);
            }

            return result;
        }

        // an exception parsing the host configuration file
        public class ConfigFileException : Exception {
            public ConfigFileException(string msg)
                : this(msg, null) {
            }
            public ConfigFileException(string msg, Exception inner)
                : base("Invalid configuration file " + _LanguagesConfigFile + ": " + msg, inner) {
            }

        }

        #endregion

        public ScriptRuntime Environment {
            get {
                return _env;
            }
        }
    }
}
