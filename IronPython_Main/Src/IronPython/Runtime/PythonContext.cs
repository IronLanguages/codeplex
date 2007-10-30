/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
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

namespace IronPython.Runtime {
    public sealed class PythonContext : LanguageContext {
        /// <summary> Standard Python context.  This is the ContextId we use to indicate calls from the Python context. </summary>
        public static ContextId Id = ContextId.RegisterContext(typeof(PythonContext));

        private PythonEngine _engine;
        private readonly bool _trueDivision;

        /// <summary>
        /// Creates a new PythonContext not bound to Engine.
        /// </summary>
        internal PythonContext()
            : base() {
        }

        public PythonContext(PythonEngine engine, PythonCompilerOptions options) {
            Contract.RequiresNotNull(engine, "engine");
            Contract.RequiresNotNull(options, "options");

            _trueDivision = options.TrueDivision;
            _engine = engine;
        }

        public PythonContext(PythonEngine engine, bool trueDivision) {
            Contract.RequiresNotNull(engine, "engine");

            _engine = engine;
            _trueDivision = trueDivision;
        }

        /// <summary>
        /// True division is encoded into a compiled code that this context is associated with. Cannot be changed.
        /// </summary>
        public bool TrueDivision {
            get { return _trueDivision; }
        }

        public PythonEngine PythonEngine {
            get {
                return _engine;
            }
            // friend: PythonEngine
            internal set {
                Assert.NotNull(value);
                _engine = value;
            }
        }

        public override ScriptEngine Engine {
            get {
                return _engine;
            }
        }

        public override ContextId ContextId {
            get {
                return PythonContext.Id;
            }
        }

        public override CodeBlock ParseSourceCode(CompilerContext context) {
            Contract.RequiresNotNull(context, "context");

            PyAst.PythonAst ast;
            SourceCodeProperties properties = SourceCodeProperties.None;
            bool propertiesSet = false;
            int errorCode = 0;

            using (Parser parser = Parser.CreateParser(context, PythonEngine.Options)) {
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
        public override SourceUnit GenerateSourceCode(System.CodeDom.CodeObject codeDom) {
            return new PythonCodeDomCodeGen().GenerateCode((System.CodeDom.CodeMemberMethod)codeDom, _engine);
        }
#endif

        public override ModuleContext CreateModuleContext(ScriptModule module) {
            Contract.RequiresNotNull(module, "module");
            InitializeModuleScope(module);
            return new PythonModuleContext(module);
        }

        private void InitializeModuleScope(ScriptModule module) {
            // TODO: following should be context sensitive variables:

            // adds __builtin__ variable if this is not a __builtin__ module itself:             
            if (PythonEngine.SystemState.modules.ContainsKey("__builtin__")) {
                module.Scope.SetName(Symbols.Builtins, PythonEngine.SystemState.modules["__builtin__"]);
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
                module.Scope.SetName(Symbols.Path, List.MakeList(dir_path));
            }
        }


        public override void ModuleContextEntering(ModuleContext newContext) {
            if (newContext == null) return;

            PythonModuleContext newPythonContext = (PythonModuleContext)newContext;

            // code executed in the scope of module cannot disable TrueDivision:

            // TODO: doesn't work for evals (test_future.py) 
            //if (newPythonContext.TrueDivision && !_trueDivision) {
            //    throw new InvalidOperationException("Code cannot be executed in this module (TrueDivision cannot be disabled).");
            //}

            // flow the code's true division into the module if we have it set
            newPythonContext.TrueDivision |= _trueDivision;
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

            ScriptModule sm = builtins as ScriptModule;
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

        public override ScriptCode Reload(ScriptCode original, ScriptModule module) {
            PythonModuleContext moduleContext = (PythonModuleContext)GetModuleContext(module);
            if (moduleContext != null && moduleContext.IsPythonCreatedModule) {
                CheckReloadablePythonModule(module);

                // We created the module and it only contains Python code. If the user changes
                // __file__ we'll reload from that file. 
                string fileName = PythonModuleOps.Get__file__(module);
                
                // built-in module: TODO: explicitly mark builtin modules in module context?
                if (fileName == null) {
                    PythonEngine.Importer.ReloadBuiltin(module);
                    return original;
                }

                SourceUnit sourceUnit = ScriptDomainManager.CurrentManager.Host.TryGetSourceFileUnit(PythonEngine, fileName,
                    PythonEngine.SystemState.DefaultEncoding);

                if (sourceUnit == null) {
                    throw PythonOps.SystemError("module cannot be reloaded");
                }

                return CompileSourceCode(sourceUnit, PythonEngine.GetModuleCompilerOptions(module));
            } else {
                return base.Reload(original, module);
            }
        }

        public void CheckReloadable(ScriptModule module) {
            Contract.RequiresNotNull(module, "module");

            // only check for Python requirements of reloading on modules created from Python.code.
            PythonModuleContext moduleContext = (PythonModuleContext)GetModuleContext(module);
            if (moduleContext != null && moduleContext.IsPythonCreatedModule) {
                CheckReloadablePythonModule(module);
            }
        }

        private void CheckReloadablePythonModule(ScriptModule pythonCreatedModule) {
            Scope scope = pythonCreatedModule.Scope;

            if (!scope.ContainsName(this, Symbols.Name))
                throw PythonOps.SystemError("nameless module");

            object name = scope.LookupName(this, Symbols.Name);
            if (!PythonEngine.SystemState.modules.ContainsKey(name)) {
                throw PythonOps.ImportError("module {0} not in sys.modules", name);
            }
        }

        public override CompilerOptions GetCompilerOptions() {
            return new PythonCompilerOptions(TrueDivision);
        }

        public override Microsoft.Scripting.Actions.ActionBinder Binder {
            get {
                return _engine.DefaultBinder;
            }
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
    }
}
