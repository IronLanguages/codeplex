/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

using System.CodeDom;
using System.CodeDom.Compiler;

using IronPython.Runtime;
using IronPython.Hosting;

namespace IronPython.CodeDom {
    /// <summary>
    /// Compiler half of the Python CodeDom Generator.
    /// </summary>
    public partial class PythonGenerator {
        #region CodeCompiler overrides
        protected override string FileExtension {
            get { return "py"; }
        }

        protected override string CompilerName {
            get { return "IronPython"; }
        }

        protected override void ProcessCompilerOutputLine(CompilerResults results, string line) {
            // gets called from base classes FromFileBatch - which is never invoked because we override it
            throw new Exception("The method or operation is not implemented.");
        }

        protected override string CmdArgsFromParameters(CompilerParameters options) {
            // gets called from base classes FromFileBatch - which is never invoked because we override it
            throw new Exception("The method or operation is not implemented.");
        }

        protected override CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames) {
            return FromFileWorker(options, fileNames);
        }

        protected override CompilerResults FromFile(CompilerParameters options, string fileName) {
            return FromFileWorker(options, fileName);
        }

        protected override CompilerResults FromDom(CompilerParameters options, CodeCompileUnit e) {
            return FromDomWorker(options, e);
        }

        protected override CompilerResults FromDomBatch(CompilerParameters options, CodeCompileUnit[] ea) {
            return FromDomWorker(options, ea);
        }

        protected override CompilerResults FromSource(CompilerParameters options, string source) {
            return FromSourceWorker(options, source);
        }

        protected override CompilerResults FromSourceBatch(CompilerParameters options, string[] sources) {
            return FromSourceWorker(options, sources);
        }
        #endregion

        #region Private implementation details
        private CompilerResults FromFileWorker(CompilerParameters options, params string[] files) {
            CompilerResults res = new CompilerResults(options.TempFiles);

            PEFileKinds targetKind;
            if (options.OutputAssembly != null) {
                if (options.OutputAssembly.ToLower().EndsWith(".exe")) {
                    targetKind = PEFileKinds.WindowApplication;
                } else {
                    targetKind = options.GenerateExecutable ? PEFileKinds.WindowApplication : PEFileKinds.Dll;
                }
            } else {
                targetKind = PEFileKinds.WindowApplication;
            }

            AppDomain compileDomain = null;
            try {
                // The new domain needs to be set up with the same ApplicationBase and PrivateBinPath
                // as the current domain to make sure that the python assembly is loadable from there
                AppDomainSetup currentDomainSetup = AppDomain.CurrentDomain.SetupInformation;
                AppDomainSetup newDomainSetup = new AppDomainSetup();
                newDomainSetup.ApplicationBase = currentDomainSetup.ApplicationBase;
                newDomainSetup.PrivateBinPath = currentDomainSetup.PrivateBinPath;

                compileDomain = AppDomain.CreateDomain("compilation domain", null, newDomainSetup);
                RemoteCompiler rc = (RemoteCompiler)compileDomain.CreateInstanceAndUnwrap(
                    Assembly.GetExecutingAssembly().FullName,
                    "IronPython.CodeDom.RemoteCompiler");

                rc.Initialize(files, options.OutputAssembly, options.IncludeDebugInformation, options.ReferencedAssemblies, targetKind);

                rc.DoCompile();
                res.NativeCompilerReturnValue = rc.ErrorCount;
                for (int i = 0; i < rc.Errors.Count; i++) {
                    res.Errors.Add(rc.Errors[i]);
                }
                try {
                    if (options.GenerateInMemory)
                        res.CompiledAssembly = rc.CompiledAssembly;
                    else
                        res.PathToAssembly = options.OutputAssembly;
                } catch {
                }
            } finally {
                AppDomain.Unload(compileDomain);
            }

            return res;
        }

        private CompilerResults FromSourceWorker(CompilerParameters options, params string[] sources) {
            string[] tempFiles = new string[sources.Length];
            for (int i = 0; i < tempFiles.Length; i++) {
                tempFiles[i] = options.TempFiles.AddExtension("py", true);
                using (StreamWriter sw = new StreamWriter(tempFiles[i])) {
                    sw.Write(sources[i]);
                }
            }

            return FromFileWorker(options, tempFiles);
        }

        private CompilerResults FromDomWorker(CompilerParameters options, params CodeCompileUnit[] ea) {
            string[] tempFiles = new string[ea.Length];
            for (int i = 0; i < tempFiles.Length; i++) {
                tempFiles[i] = options.TempFiles.AddExtension("py", true);

                using (StreamWriter sw = new StreamWriter(tempFiles[i])) {
                    new PythonProvider().GenerateCodeFromCompileUnit(ea[i], sw, new CodeGeneratorOptions());
                }
            }

            return FromFileWorker(options, tempFiles);
        }

        #endregion
    }

    class CodeDomCompilerSink : CompilerSink {
        RemoteCompiler compResults;
        public CodeDomCompilerSink(RemoteCompiler results) {
            compResults = results;
        }

        public override void AddError(string path, string message, string lineText, int startLine, int startColumn, int endLine, int endColumn, int errorCode, Severity severity) {
            compResults.Errors.Add(new CompilerError(path, startLine, startColumn, errorCode.ToString(), message));
            throw new CompilerException();
        }
    }

    /// <summary>
    /// Used to prevent further processing after an error while compiling for CodeDom
    /// 
    /// This is the only exception we catch - all other exceptions propagate to our caller.
    /// </summary>
    class CompilerException : Exception {
        public CompilerException() {
        }

        public CompilerException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) {
        }
    }

    class RemoteCompiler : MarshalByRefObject {
        static RemoteCompiler instance;

        string[] files;
        string outAsm;
        bool debugInfo;
        StringCollection refs;
        PEFileKinds peKind;
        Assembly compAssm;
        int errorCnt;
        List<CompilerError> errors = new List<CompilerError>();

        public RemoteCompiler() {
            instance = this;
        }

        public void Initialize(string[] filenames, string outputAssembly, bool includeDebug, StringCollection references, PEFileKinds targetKind) {
            files = filenames;
            outAsm = outputAssembly;
            if (outAsm == null) outAsm = "assembly.dll";
            debugInfo = includeDebug;
            refs = references;
            peKind = targetKind;
        }

        public void DoCompile() {
            PythonCompiler pc = new PythonCompiler(files, outAsm, new CodeDomCompilerSink(this));
            pc.IncludeDebugInformation = debugInfo;
            pc.TargetKind = peKind;
            pc.AutoImportAll = true;
            pc.StaticTypes = true;

            foreach (string s in refs) {
                pc.ReferencedAssemblies.Add(s);
            }

            try {
                pc.Compile();

                try {
                    compAssm = Assembly.LoadFrom(outAsm);
                } catch {
                    errorCnt = 1;
                }
            } catch (CompilerException) {
                // errors occured during compilation
                errorCnt = Errors.Count;
//                Errors.Add(new CompilerError("unknown", 0, 0, "unknown", ex.ToString()));
            } catch (Exception ex) {
                errorCnt++;
                Errors.Add(new CompilerError("unknown", 0, 0, "unknown", ex.ToString()));
            }
        }

        public static RemoteCompiler Instance {
            get {
                return instance;
            }
        }

        public StringCollection References {
            get {
                return refs;
            }
        }

        public Assembly CompiledAssembly {
            get {
                return compAssm;
            }
        }

        public int ErrorCount {
            get {
                return errorCnt;
            }
        }

        public List<CompilerError> Errors {
            get {
                return errors;
            }
        }
    }
}
