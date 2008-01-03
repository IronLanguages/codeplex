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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.IO;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Compiler;
using IronPython.Runtime.Exceptions;
using IronPython.Hosting;
using PyAst = IronPython.Compiler.Ast;
using IronPython.Compiler.Generation;
using System.Text;
using Microsoft.Scripting.Shell;

namespace IronPython.Runtime {
    public sealed class PythonContext : LanguageContext {
        internal static SystemState _systemState;    // should be instance for multi-engine support
        private static Importer _importer;          // should be instance for multi-engine support
        internal static PythonEngineOptions _engOptions = new PythonEngineOptions(); // should be instance for multi-engine support
        private static readonly PythonModuleContext _defaultModuleContext;
        
        private static readonly Guid PythonLanguageGuid = new Guid("03ed4b80-d10b-442f-ad9a-47dae85b2051");
        private static readonly Guid LanguageVendor_Microsoft = new Guid(-1723120188, -6423, 0x11d2, 0x90, 0x3f, 0, 0xc0, 0x4f, 0xa3, 2, 0xa1);
        internal static object _exceptionType;
        /// <summary> Standard Python context.  This is the ContextId we use to indicate calls from the Python context. </summary>

        private ScriptEngine _engine;
#if !SILVERLIGHT
        private static int _hookedAssemblyResolve;
#endif

        static PythonContext() {
            _defaultModuleContext = new PythonModuleContext(null);
        }

        /// <summary>
        /// Creates a new PythonContext not bound to Engine.
        /// </summary>
        public PythonContext(ScriptDomainManager manager)
            : base(manager) {

            // singletons:
            _importer = new Importer(this);

            DefaultContext.CreateContexts(this);

            Binder = new PythonBinder(DefaultContext.DefaultCLS);

            if (DefaultContext.Default.LanguageContext.Binder == null) {
                // hack to fix the default language context binder, there's an order of 
                // initialization issue w/ the binder & the default context.
                ((PythonContext)DefaultContext.Default.LanguageContext).Binder = Binder;
            }
            if (DefaultContext.DefaultCLS.LanguageContext.Binder == null) {
                // hack to fix the default language context binder, there's an order of 
                // initialization issue w/ the binder & the default context.
                ((PythonContext)DefaultContext.DefaultCLS.LanguageContext).Binder = Binder;
            }

            _systemState = new SystemState();

            _exceptionType = ExceptionConverter.GetPythonException("Exception");
            _systemState.Initialize();
#if SILVERLIGHT
            AddToPath(".");
#endif
            
            // sys.argv always includes at least one empty string.
            Debug.Assert(PythonOptions.Arguments != null);
            _systemState.argv = List.Make(PythonOptions.Arguments.Length == 0 ? new object[] { String.Empty } : PythonOptions.Arguments);

#if !SILVERLIGHT // AssemblyResolve
            try {
                if (Interlocked.Exchange(ref _hookedAssemblyResolve, 1) == 0) {
                    HookAssemblyResolve();
                }
            } catch (System.Security.SecurityException) {
                // We may not have SecurityPermissionFlag.ControlAppDomain. 
                // If so, we will not look up sys.path for module loads
            }
#endif
            // TODO:
            // SetModuleCodeContext(ScriptDomainManager.CurrentManager.Host.DefaultModule, _defaultModuleContext);
        }

        public ScriptEngine ScriptEngine {
            get {
                return _engine;
            }
            // friend: ScriptEngine
            internal set {
                Assert.NotNull(value);
                _engine = value;
            }
        }

        public override CodeBlock ParseSourceCode(CompilerContext context) {
            Contract.RequiresNotNull(context, "context");

            PyAst.PythonAst ast;
            SourceCodeProperties properties = SourceCodeProperties.None;
            bool propertiesSet = false;
            int errorCode = 0;

            using (Parser parser = Parser.CreateParser(context, PythonContext.GetPythonOptions(null))) {
                switch (context.SourceUnit.Kind) {
                    case SourceCodeKind.InteractiveCode:
                        ast = parser.ParseInteractiveCode(out properties);
                        propertiesSet = true;
                        break;

                    case SourceCodeKind.Expression:
                        ast = parser.ParseExpression();
                        break;

                    case SourceCodeKind.SingleStatement:
                        ast = parser.ParseSingleStatement();
                        break;

                    case SourceCodeKind.File:
                        ast = parser.ParseFile(true);
                        break;

                    default:
                    case SourceCodeKind.Statements:
                        ast = parser.ParseFile(false);
                        break;
                }

                errorCode = parser.ErrorCode;
            }

            if (!propertiesSet && errorCode != 0) {
                properties = SourceCodeProperties.IsInvalid;
            }

            context.SourceUnit.CodeProperties = properties;

            if (errorCode != 0 || properties == SourceCodeProperties.IsEmpty) {
                return null;
            }

            // TODO: remove when the module is generated by PythonAst.Transform:
            ((PythonCompilerOptions)context.Options).TrueDivision = ast.TrueDivision;

            PyAst.PythonNameBinder.BindAst(ast, context);

            return PyAst.AstGenerator.TransformAst(context, ast);
        }

        public override StreamReader GetSourceReader(Stream stream, Encoding encoding) {
            Contract.RequiresNotNull(stream, "stream");
            Contract.RequiresNotNull(encoding, "encoding");
            Contract.Requires(stream.CanSeek && stream.CanRead, "stream", "The stream must support seeking and reading");

            // we choose ASCII by default, if the file has a Unicode pheader though
            // we'll automatically get it as unicode.
            Encoding default_encoding = encoding;
            encoding = PythonAsciiEncoding.Instance;

            long start_position = stream.Position;

            StreamReader sr = new StreamReader(stream, PythonAsciiEncoding.Instance);            

            int bytesRead = 0;
            string line;
            line = ReadOneLine(sr, ref bytesRead);

            //string line = sr.ReadLine();
            bool gotEncoding = false;

            // magic encoding must be on line 1 or 2
            if (line != null && !(gotEncoding = Tokenizer.TryGetEncoding(default_encoding, line, ref encoding))) {
                line = ReadOneLine(sr, ref bytesRead);

                if (line != null) {
                    gotEncoding = Tokenizer.TryGetEncoding(default_encoding, line, ref encoding);
                }
            }

            if (gotEncoding && sr.CurrentEncoding != PythonAsciiEncoding.Instance && encoding != sr.CurrentEncoding) {
                // we have both a BOM & an encoding type, throw an error
                throw new IOException("file has both Unicode marker and PEP-263 file encoding");
            }

            if (encoding == null)
                throw new IOException("unknown encoding type");

            if (!gotEncoding) {
                // if we didn't get an encoding seek back to the beginning...
                stream.Seek(start_position, SeekOrigin.Begin);
            } else {
                // if we got an encoding seek to the # of bytes we read (so the StreamReader's
                // buffering doesn't throw us off)
                stream.Seek(bytesRead, SeekOrigin.Begin);
            }

            // re-read w/ the correct encoding type...
            return new StreamReader(stream, encoding);
        }

        /// <summary>
        /// Reads one line keeping track of the # of bytes read
        /// </summary>
        private static string ReadOneLine(StreamReader sr, ref int totalRead) {
            char[] buffer = new char[256];
            StringBuilder builder = null;
            
            int bytesRead = sr.Read(buffer, 0, buffer.Length);

            while (bytesRead > 0) {
                totalRead += bytesRead;

                bool foundEnd = false;
                for (int i = 0; i < bytesRead; i++) {
                    if (buffer[i] == '\r') {
                        if (i + 1 < bytesRead) {
                            if (buffer[i + 1] == '\n') {
                                totalRead -= (bytesRead - (i+2));   // skip cr/lf
                                sr.BaseStream.Seek(i + 1, SeekOrigin.Begin);
                                sr.DiscardBufferedData();
                                foundEnd = true;
                            }
                        } else {
                            totalRead -= (bytesRead - (i + 1)); // skip cr
                            sr.BaseStream.Seek(i, SeekOrigin.Begin);
                            sr.DiscardBufferedData();
                            foundEnd = true;
                        }
                    } else if (buffer[i] == '\n') {
                        totalRead -= (bytesRead - (i + 1)); // skip lf
                        sr.BaseStream.Seek(i + 1, SeekOrigin.Begin);
                        sr.DiscardBufferedData();
                        foundEnd = true;
                    }

                    if (foundEnd) {                        
                        if (builder != null) {
                            builder.Append(buffer, 0, i);
                            return builder.ToString();
                        }
                        return new string(buffer, 0, i);
                    }
                }

                if (builder == null) builder = new StringBuilder();
                builder.Append(buffer, 0, bytesRead);
                bytesRead = sr.Read(buffer, 0, buffer.Length);
            }

            // no string
            if (builder == null) {
                return null;
            }

            // no new-line
            return builder.ToString();
        }

#if !SILVERLIGHT
        // Convert a CodeDom to source code, and output the generated code and the line number mappings (if any)
        public override SourceUnit/*!*/ GenerateSourceCode(System.CodeDom.CodeObject codeDom, string id, SourceCodeKind kind) {
            return new PythonCodeDomCodeGen().GenerateCode((System.CodeDom.CodeMemberMethod)codeDom, this, id, kind);
        }
#endif

        public override ModuleContext CreateModuleContext(ScriptScope module) {
            Contract.RequiresNotNull(module, "module");
            InitializeModuleScope(module);
            return new PythonModuleContext(module);
        }

        private void InitializeModuleScope(ScriptScope module) {
            // TODO: following should be context sensitive variables:

            // adds __builtin__ variable if this is not a __builtin__ module itself:             
            if (PythonContext.GetSystemState(null).modules.ContainsKey("__builtin__")) {
                module.SetVariable("__builtins__", PythonContext.GetSystemState(null).modules["__builtin__"]);
            } else {
                Debug.Assert(module.ModuleName == "__builtin__");
            }

            // do not set names if null to make attribute getter pas thru:
            if (module.ModuleName != null) {
                PythonModuleOps.Set__name__(module, module.ModuleName);
            }

            if (module.FileName != null) {
                PythonModuleOps.Set__file__(module, module.FileName);
            }

            // If the filename is __init__.py then this is the initialization code
            // for a package and we need to set the __path__ variable appropriately
            if (module.FileName != null && Path.GetFileName(module.FileName) == "__init__.py") {
                string dirname = Path.GetDirectoryName(module.FileName);
                string dir_path = ScriptDomainManager.CurrentManager.Host.NormalizePath(dirname);
                module.SetVariable("__path__", List.MakeList(dir_path));
            }
        }


        public override void ModuleContextEntering(ModuleContext newContext) {
            if (newContext == null) return;
            Debug.Assert(newContext is PythonModuleContext);
            PythonModuleContext newPythonContext = (PythonModuleContext)newContext;
            

            // code executed in the scope of module cannot disable TrueDivision:

            // TODO: doesn't work for evals (test_future.py) 
            //if (newPythonContext.TrueDivision && !_trueDivision) {
            //    throw new InvalidOperationException("Code cannot be executed in this module (TrueDivision cannot be disabled).");
            //}

            // flow the code's true division into the module if we have it set
            newPythonContext.TrueDivision |= ((PythonCompilerOptions)newPythonContext.CompilerContext.Options).TrueDivision;
        }

        /// <summary>
        /// Python's global scope includes looking at built-ins.  First check built-ins, and if
        /// not there then fallback to any DLR globals.
        /// </summary>
        public override bool TryLookupGlobal(CodeContext context, SymbolId name, out object value) {
            object builtins;
            if (!context.Scope.ModuleScope.TryGetName(this, Symbols.Builtins, out builtins)) {
                value = null;
                return false;
            }

            ScriptScope sm = builtins as ScriptScope;
            if (sm == null) {
                value = null;
                return false;
            }

            if (sm.Scope.TryGetName(context.LanguageContext, name, out value)) return true;

            return base.TryLookupGlobal(context, name, out value);
        }

        protected override Exception MissingName(SymbolId name) {
            throw PythonOps.NameError(name);
        }

        public override ScriptCode Reload(ScriptCode original, ScriptScope module) {
            PythonModuleContext moduleContext = (PythonModuleContext)GetModuleContext(module);
            if (moduleContext != null && moduleContext.IsPythonCreatedModule) {
                CheckReloadablePythonModule(module);

                // We created the module and it only contains Python code. If the user changes
                // __file__ we'll reload from that file. 
                string fileName = PythonModuleOps.Get__file__(module);
                
                // built-in module: TODO: explicitly mark builtin modules in module context?
                if (fileName == null) {
                    PythonContext.GetImporter(null).ReloadBuiltin(module);
                    return original;
                }

                SourceUnit sourceUnit = ScriptDomainManager.CurrentManager.Host.TryGetSourceFileUnit(ScriptEngine, 
                    fileName,
                    _systemState.DefaultEncoding,
                    SourceCodeKind.File);

                if (sourceUnit == null) {
                    throw PythonOps.SystemError("module cannot be reloaded");
                }

                return CompileSourceCode(sourceUnit, GetModuleCompilerOptions(module));
            } else {
                return base.Reload(original, module);
            }
        }

        public void CheckReloadable(ScriptScope module) {
            Contract.RequiresNotNull(module, "module");

            // only check for Python requirements of reloading on modules created from Python.code.
            PythonModuleContext moduleContext = (PythonModuleContext)GetModuleContext(module);
            if (moduleContext != null && moduleContext.IsPythonCreatedModule) {
                CheckReloadablePythonModule(module);
            }
        }

        private void CheckReloadablePythonModule(ScriptScope pythonCreatedModule) {
            Scope scope = pythonCreatedModule.Scope;

            if (!scope.ContainsName(this, Symbols.Name))
                throw PythonOps.SystemError("nameless module");

            object name = scope.LookupName(this, Symbols.Name);
            if (!_systemState.modules.ContainsKey(name)) {
                throw PythonOps.ImportError("module {0} not in sys.modules", name);
            }
        }

        public override CompilerOptions GetCompilerOptions() {
            return new PythonCompilerOptions();
        }

        public override bool IsTrue(object obj) {
            return PythonOps.IsTrue(obj);
        }

        protected override ModuleGlobalCache GetModuleCache(SymbolId name) {
            ModuleGlobalCache res;
            if (!SystemState.Instance.TryGetModuleGlobalCache(name, out res)) {
                res = base.GetModuleCache(name);
            }

            return res;
        }

        public override object Call(CodeContext context, object function, object[] args) {
            return PythonOps.CallWithContext(context, function, args);
        }

        public override object CallWithThis(CodeContext context, object function, object instance, object[] args) {
            return PythonOps.CallWithContextAndThis(context, function, instance, args);
        }

        public override object CallWithArgsKeywordsTupleDict(CodeContext context, object func, object[] args, string[] names, object argsTuple, object kwDict) {
            return PythonOps.CallWithArgsTupleAndKeywordDictAndContext(context, func, args, names, argsTuple, kwDict);
        }

        public override object CallWithArgsTuple(CodeContext context, object func, object[] args, object argsTuple) {
            return PythonOps.CallWithArgsTupleAndContext(context, func, args, argsTuple);
        }

        public override object CallWithKeywordArgs(CodeContext context, object func, object[] args, string[] names) {
            return PythonOps.CallWithKeywordArgs(context, func, args, names);
        }

        public override bool EqualReturnBool(CodeContext context, object x, object y) {
            return PythonOps.EqualRetBool(x, y);
        }

        public override object GetNotImplemented(params MethodCandidate[] candidates) {
            return PythonOps.NotImplemented;
        }

        public override bool IsCallable(object obj, int argumentCount, out int min, out int max) {
            return PythonOps.IsCallable(obj, argumentCount, out min, out max);
        }

        public override Assembly LoadAssemblyFromFile(string file) {
#if !SILVERLIGHT
            // check all files in the path...
            IEnumerator ie = PythonOps.GetEnumerator(SystemState.Instance.path);
            while (ie.MoveNext()) {
                string str;
                if (Converter.TryConvertToString(ie.Current, out str)) {
                    string fullName = Path.Combine(str, file);
                    Assembly res;

                    if (TryLoadAssemblyFromFileWithPath(fullName, out res)) return res;
                    if (TryLoadAssemblyFromFileWithPath(fullName + ".EXE", out res)) return res;
                    if (TryLoadAssemblyFromFileWithPath(fullName + ".DLL", out res)) return res;
                }
            }
#endif
            return null;
        }

#if !SILVERLIGHT // AssemblyResolve, files, path
        private bool TryLoadAssemblyFromFileWithPath(string path, out Assembly res) {
            if (File.Exists(path)) {
                try {
                    res = Assembly.LoadFile(path);
                    if (res != null) return true;
                } catch { }
            }
            res = null;
            return false;
        }

        internal static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            AssemblyName an = new AssemblyName(args.Name);
            return DefaultContext.Default.LanguageContext.LoadAssemblyFromFile(an.Name);
        }
#endif

        public override string DisplayName {
            get {
                return "IronPython";
            }
        }

        public override Guid LanguageGuid {
            get {
                return PythonLanguageGuid;
            }
        }

        public override Version LanguageVersion {
            get {
                return new Version(2, 0);
            }
        }

        public override void SetScriptSourceSearchPaths(string[] paths) {
            _systemState.path = List.Make(paths);
        }
        
        public override TextWriter GetOutputWriter(bool isErrorOutput) {
            return new OutputWriter(this, isErrorOutput);
        }

        public override void Shutdown() {
            object callable;

            if (PythonOps.TryGetBoundAttr(_systemState, Symbols.SysExitFunc, out callable)) {
                PythonCalls.Call(callable);
            }
        }

        private static string GetInformationalVersion() {
            AssemblyInformationalVersionAttribute attribute = GetAssemblyAttribute<AssemblyInformationalVersionAttribute>();
            return attribute != null ? attribute.InformationalVersion : "";
        }

        private static string GetFileVersion() {
#if !SILVERLIGHT // file version
            AssemblyFileVersionAttribute attribute = GetAssemblyAttribute<AssemblyFileVersionAttribute>();
            return attribute != null ? attribute.Version : "";
#else
            return "1.0.0.0";
#endif
        }

        public override string FormatException(Exception exception) {
            SyntaxErrorException syntax_error = exception as SyntaxErrorException;
            if (syntax_error != null) {
                return FormatPythonSyntaxError(syntax_error);
            }

            object pythonEx = ExceptionConverter.ToPython(exception);

            string result = FormatStackTraces(exception) + FormatPythonException(pythonEx) + Environment.NewLine;

            if (Options.ShowClrExceptions) {
                result += FormatCLSException(exception);
            }

            return result;
        }

        internal static string FormatPythonSyntaxError(SyntaxErrorException e) {
            string sourceLine = e.GetCodeLine();

            return String.Format(
                "  File \"{1}\", line {2}{0}" +
                "    {3}{0}" +
                "    {4}^{0}" +
                "{5}: {6}{0}",
                Environment.NewLine,
                e.GetSymbolDocumentName(),
                e.Line > 0 ? e.Line.ToString() : "?",
                (sourceLine != null) ? sourceLine.Replace('\t', ' ') : null,
                new String(' ', e.Column != 0 ? e.Column - 1 : 0),
                GetPythonExceptionClassName(ExceptionConverter.ToPython(e)), e.Message);
        }

        private static string FormatCLSException(Exception e) {
            StringBuilder result = new StringBuilder();
            result.AppendLine("CLR Exception: ");
            while (e != null) {
                result.Append("    ");
                result.AppendLine(e.GetType().Name);
                if (!String.IsNullOrEmpty(e.Message)) {
                    result.AppendLine(": ");
                    result.AppendLine(e.Message);
                } else {
                    result.AppendLine();
                }

                e = e.InnerException;
            }

            return result.ToString();
        }

        internal static string FormatPythonException(object pythonException) {
            string result = "";

            // dump the python exception.
            if (pythonException != null) {
                string str = pythonException as string;
                if (str != null) {
                    result += str;
                } else if (pythonException is StringException) {
                    result += pythonException.ToString();
                } else {
                    result += GetPythonExceptionClassName(pythonException) + ": " + pythonException.ToString();
                }
            }

            return result;
        }

        private static string GetPythonExceptionClassName(object pythonException) {
            string className = "";
            object val;
            if (PythonOps.TryGetBoundAttr(pythonException, Symbols.Class, out val)) {
                if (PythonOps.TryGetBoundAttr(val, Symbols.Name, out val)) {
                    className = val.ToString();
                    if (PythonOps.TryGetBoundAttr(pythonException, Symbols.Module, out val)) {
                        string moduleName = val.ToString();
                        if (moduleName != ExceptionConverter.defaultExceptionModule) {
                            className = moduleName + "." + className;
                        }
                    }
                }
            }
            return className;
        }

#if SILVERLIGHT // stack trace
        private string FormatStackTraces(Exception e) {

            StringBuilder result = new StringBuilder();
            result.AppendLine("Traceback (most recent call last):");
            DynamicStackFrame[] dfs = RuntimeHelpers.GetDynamicStackFrames(e);
            for (int i = 0; i < dfs.Length; ++i) {
                DynamicStackFrame frame = dfs[i];
                result.AppendFormat("  at {0} in {1}, line {2}\n", frame.GetMethodName(), frame.GetFileName(), frame.GetFileLineNumber());
            }

            if (Options.ExceptionDetail) {
                result.AppendLine(e.Message);
            }
            
            return result.ToString();
        }
#else
        private string FormatStackTraces(Exception e) {
            bool printedHeader = false;

            return FormatStackTraces(e, ref printedHeader);
        }

        private string FormatStackTraces(Exception e, ref bool printedHeader) {
            return FormatStackTraces(e, null, ref printedHeader);
        }

        private string FormatStackTraces(Exception e, FilterStackFrame fsf, ref bool printedHeader) {
            string result = "";
            if (Options.ExceptionDetail) {
                if (!printedHeader) {
                    result = e.Message + Environment.NewLine;
                    printedHeader = true;
                }
                IList<System.Diagnostics.StackTrace> traces = ExceptionHelpers.GetExceptionStackTraces(e);

                if (traces != null) {
                    for (int i = 0; i < traces.Count; i++) {
                        for (int j = 0; j < traces[i].FrameCount; j++) {
                            StackFrame curFrame = traces[i].GetFrame(j);
                            if (fsf == null || fsf(curFrame))
                                result += curFrame.ToString() + Environment.NewLine;
                        }
                    }
                }

                if (e.StackTrace != null) result += e.StackTrace.ToString() + Environment.NewLine;
                if (e.InnerException != null) result += FormatStackTraces(e.InnerException, ref printedHeader);
            } else {
                result = FormatStackTraceNoDetail(e, fsf, ref printedHeader);
            }

            return result;
        }

        internal string FormatStackTraceNoDetail(Exception e, FilterStackFrame fsf, ref bool printedHeader) {
            string result = String.Empty;
            // dump inner most exception first, followed by outer most.
            if (e.InnerException != null) result += FormatStackTraceNoDetail(e.InnerException, fsf, ref printedHeader);

            if (!printedHeader) {
                result += "Traceback (most recent call last):" + Environment.NewLine;
                printedHeader = true;
            }
            List<DynamicStackFrame> dynamicFrames = new List<DynamicStackFrame>(RuntimeHelpers.GetDynamicStackFrames(e));
            IList<StackTrace> traces = ExceptionHelpers.GetExceptionStackTraces(e);
            if (traces != null && traces.Count > 0) {
                for (int i = traces.Count - 1; i >= 0; i--) {
                    result += FormatStackTrace(traces[i], dynamicFrames, fsf);
                }
            }
            result += FormatStackTrace(new StackTrace(e, true), dynamicFrames, fsf);

            //TODO: we would like to be able to assert this;
            // right now, we cannot, because we are not using dynamic frames for non-interpreted dynamic methods.
            // (we create the frames, but we do not consume them in FormatStackTrace.)
            //Debug.Assert(dynamicFrames.Count == 0);

            return result;
        }

        private string FormatStackTrace(StackTrace st, List<DynamicStackFrame> dynamicFrames, FilterStackFrame fsf) {
            string result = "";

            StackFrame[] frames = st.GetFrames();
            if (frames == null) return result;

            for (int i = frames.Length - 1; i >= 0; i--) {
                StackFrame frame = frames[i];
                MethodBase method = frame.GetMethod();
                Type parentType = method.DeclaringType;
                if (parentType != null) {
                    string typeName = parentType.FullName;
                    if (typeName == "Microsoft.Scripting.Ast.CodeBlock" && method.Name == "DoExecute") {
                        // Evaluated frame -- Replace with dynamic frame
                        Debug.Assert(dynamicFrames.Count > 0);
                        //if (dynamicFrames.Count == 0) continue;
                        result += FrameToString(dynamicFrames[dynamicFrames.Count - 1]) + Environment.NewLine;
                        dynamicFrames.RemoveAt(dynamicFrames.Count - 1);
                        continue;
                    }
                    if (typeName.StartsWith("IronPython.") ||
                        typeName.StartsWith("ReflectOpt.") ||
                        typeName.StartsWith("System.Reflection.") ||
                        typeName.StartsWith("System.Runtime") ||
                        typeName.StartsWith("Microsoft.Scripting") ||
                        typeName.StartsWith("IronPythonConsole.")) {
                        continue;
                    }
                }


                if (fsf != null && !fsf(frame)) continue;

                // TODO: also try to use dynamic frames for non-interpreted dynamic methods
                result += FrameToString(frame) + Environment.NewLine;
            }

            return result;
        }

        private string FrameToString(DynamicStackFrame frame) {
            return String.Format("  File {0}, line {1}, in {2}",
                frame.GetFileName(), frame.GetFileLineNumber(), frame.GetMethodName());
        }


        private string FrameToString(StackFrame frame) {
            if (frame.GetMethod().DeclaringType != null &&
                frame.GetMethod().DeclaringType.Assembly == ScriptDomainManager.CurrentManager.Snippets.Assembly.AssemblyBuilder) {
                string methodName;
                int dollar;

                if (frame.GetMethod().Name == "Run") methodName = "-toplevel-";
                else if ((dollar = frame.GetMethod().Name.IndexOf('$')) == -1) methodName = frame.GetMethod().Name;
                else methodName = frame.GetMethod().Name.Substring(0, dollar);

                return String.Format("  File {0}, line {1}, in {2}",
                    frame.GetFileName(),
                    frame.GetFileLineNumber(),
                    methodName);
            } else {
                string methodName;
                int dollar;

                if ((dollar = frame.GetMethod().Name.IndexOf('$')) == -1) methodName = frame.GetMethod().Name;
                else methodName = frame.GetMethod().Name.Substring(0, dollar);

                string filename = frame.GetFileName();
                string line = frame.GetFileLineNumber().ToString();
                if (String.IsNullOrEmpty(filename)) {
                    if (frame.GetMethod().DeclaringType != null) {
                        filename = frame.GetMethod().DeclaringType.Assembly.GetName().Name;
                        line = "unknown";
                    }
                }

                return String.Format("  File {0}, line {1}, in {2}",
                    filename,
                    line,
                    methodName);
            }
        }

        public delegate bool FilterStackFrame(StackFrame frame);

        private string FormatException(Exception exception, object pythonException, FilterStackFrame filter) {
            Debug.Assert(pythonException != null);
            Debug.Assert(exception != null);

            string result = string.Empty;
            bool printedHeader = false;
            result += FormatStackTraces(exception, filter, ref printedHeader);
            result += FormatPythonException(pythonException);
            if (Options.ShowClrExceptions) {
                result += FormatCLSException(exception);
            }

            return result;
        }
#endif

        private static T GetAssemblyAttribute<T>() where T : Attribute {
            Assembly asm = typeof(ScriptEngine).Assembly;
            object[] attributes = asm.GetCustomAttributes(typeof(T), false);
            if (attributes != null && attributes.Length > 0) {
                return (T)attributes[0];
            } else {
                Debug.Assert(false, String.Format("Cannot find attribute {0}", typeof(T).Name));
                return null;
            }
        }

        public static Importer GetImporter(CodeContext context) {
            // TODO: Multi-engine support
            return _importer;
        }

        public override ServiceType GetService<ServiceType>(params object[] args) {
            if (typeof(ServiceType) == typeof(CommandLine)) {
                return (ServiceType)(object)new PythonCommandLine();
            } else if (typeof(ServiceType) == typeof(OptionsParser)) {
                return (ServiceType)(object)new PythonOptionsParser(this);
            } else if (typeof(ServiceType) == typeof(ITokenCategorizer)) {
                return (ServiceType)(object)new PythonTokenCategorizer();
            }

            return base.GetService<ServiceType>(args);
        }

        public static SystemState GetSystemState(CodeContext context) {
            // TODO: Multi-engine support
            return _systemState;
        }

        public static PythonEngineOptions GetPythonOptions(CodeContext context) {
            return _engOptions;
        }

        public Importer Importer {
            get {
                return _importer;
            }
        }

        /// <summary>
        /// Should be instance method
        /// </summary>
        /// <param name="directory"></param>
        public static void AddToPath(string directory) {
            _systemState.path.Append(directory);
        }

        // scope can be null, should be instance method for multi-engine support
        public static ScriptScope MakePythonModule(string name, Scope scope, ModuleOptions options) {
            Contract.RequiresNotNull(name, "name");
            if (scope == null) scope = new Scope(new SymbolDictionary());

            ScriptScope module = ScriptDomainManager.CurrentManager.CreateModule(name, scope);

            PythonModuleContext moduleContext = (PythonModuleContext)DefaultContext.Default.LanguageContext.EnsureModuleContext(module);
            moduleContext.ShowCls = (options & ModuleOptions.ShowClsMethods) != 0;
            moduleContext.TrueDivision = (options & ModuleOptions.TrueDivision) != 0;
            moduleContext.IsPythonCreatedModule = true;

            if ((options & ModuleOptions.PublishModule) != 0) {
                _systemState.modules[module.ModuleName] = module;
            }

            return module;
        }

        /// <summary>
        /// Create a module with optimized code. The restriction is that the user cannot specify a globals 
        /// dictionary of her liking.
        /// </summary>
        public ScriptScope CreateOptimizedModule(string fileName, string moduleName, bool publishModule) {
            return CreateOptimizedModule(fileName, moduleName, publishModule, false);
        }

        public ScriptScope CreateOptimizedModule(string fileName, string moduleName, bool publishModule, bool skipFirstLine) {
            Contract.RequiresNotNull(fileName, "fileName");
            Contract.RequiresNotNull(moduleName, "moduleName");

            SourceUnit sourceUnit = CreateFileUnit(fileName, _systemState.DefaultEncoding);
            PythonCompilerOptions options = (PythonCompilerOptions)GetDefaultCompilerOptions();
            options.SkipFirstLine = skipFirstLine;
            ScriptScope module = ScriptDomainManager.CurrentManager.CompileModule(moduleName, ScriptModuleKind.Default, null, options, null, sourceUnit);

            if (publishModule) {
                _systemState.modules[moduleName] = module;
            }

            return module;
        }

        public override CompilerOptions GetDefaultCompilerOptions() {
            return new PythonCompilerOptions(PythonOptions.DivisionOptions == PythonDivisionOptions.New);
        }

        public override CompilerOptions GetModuleCompilerOptions(ScriptScope module) {
            Assert.NotNull(module);

            PythonCompilerOptions result = new PythonCompilerOptions();
            PythonModuleContext moduleContext = (PythonModuleContext)GetModuleContext(module);

            if (moduleContext != null) {
                result.TrueDivision = moduleContext.TrueDivision;
            } else {
                result.TrueDivision = PythonOptions.DivisionOptions == PythonDivisionOptions.New;
            }

            return result;
        }

        public override EngineOptions Options {
            get { return _engOptions; }
        }

        public PythonEngineOptions PythonOptions {
            get {
                return _engOptions;
            }
        }

        public override Guid VendorGuid {
            get {
                return LanguageVendor_Microsoft;
            }
        }

        public override ErrorSink GetCompilerErrorSink() {
            return new CompilerErrorSink();
        }

        public override void GetExceptionMessage(Exception exception, out string message, out string typeName) {
            object pythonEx = ExceptionConverter.ToPython(exception);

            message = FormatPythonException(ExceptionConverter.ToPython(exception));
            typeName = GetPythonExceptionClassName(pythonEx);
        }

#if !SILVERLIGHT
        /// <summary>
        /// We use Assembly.LoadFile to load assemblies from a path specified by the script (in LoadAssemblyFromFileWithPath).
        /// However, when the CLR loader tries to resolve any of assembly references, it will not be able to
        /// find the dependencies, unless we can hook into the CLR loader.
        /// </summary>
        private static void HookAssemblyResolve() {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(PythonContext.CurrentDomain_AssemblyResolve);
        }
#endif

    }
}
