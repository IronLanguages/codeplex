/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Hosting;

namespace IronPython.CodeDom {
    /// <summary>
    /// Compiler half of the Python CodeDom Generator.
    /// </summary>
    partial class PythonGenerator {
        #region CodeCompiler overrides
        protected override string FileExtension {
            get { return "py"; }
        }

        protected override string CompilerName {
            get { return "IronPython"; }
        }

        protected override void ProcessCompilerOutputLine(CompilerResults results, string line) {
            // gets called from base classes FromFileBatch - which is never invoked because we override it
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        protected override string CmdArgsFromParameters(CompilerParameters options) {
            // gets called from base classes FromFileBatch - which is never invoked because we override it
            throw new NotImplementedException("The method or operation is not implemented.");
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
        private static CompilerResults FromFileWorker(CompilerParameters options, params string[] files) {
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

            // The new domain needs to be set up with the same ApplicationBase and PrivateBinPath
            // as the current domain to make sure that the python assembly is loadable from there
            AppDomainSetup currentDomainSetup = AppDomain.CurrentDomain.SetupInformation;
            AppDomainSetup newDomainSetup = new AppDomainSetup();
            newDomainSetup.ApplicationBase = currentDomainSetup.ApplicationBase;
            newDomainSetup.PrivateBinPath = currentDomainSetup.PrivateBinPath;

            AppDomain compileDomain = null;
            try {
                compileDomain = AppDomain.CreateDomain("compilation domain", null, newDomainSetup);

                // This is a horrible hack: When running in a multi app domain scenario, where
                // IronPython has not been strong named, and exists on both sides of the app domain
                // boundary, we need some common type that both sides can agree on.  That common
                // type needs to live in the GAC on both sides.  So we abuse IReflect and use it
                // as our communication channel across the app domain boundary.
                IReflect rc = (IReflect)compileDomain.CreateInstanceFromAndUnwrap(
                    Assembly.GetExecutingAssembly().Location,
                    "IronPython.CodeDom.RemoteCompiler",
                    false,
                    BindingFlags.Public | BindingFlags.CreateInstance | BindingFlags.Instance,
                    null,
                    new object[] { files, options.OutputAssembly, options.IncludeDebugInformation, options.ReferencedAssemblies, targetKind, options.MainClass, options.EmbeddedResources},
                    null,
                    null,
                    null);

                //rc.Initialize(files, options.OutputAssembly, options.IncludeDebugInformation, options.ReferencedAssemblies, targetKind);

                InvokeCompiler(rc, "Compile");

                res.NativeCompilerReturnValue = (int)InvokeCompiler(rc, "get_ErrorCount");
                List<CompilerError> errors = (List<CompilerError>)(InvokeCompiler(rc, "get_Errors"));
                for (int i = 0; i < errors.Count; i++) {
                    res.Errors.Add(errors[i]);
                }
                try {
                    if (options.GenerateInMemory)
                        res.CompiledAssembly = (Assembly)InvokeCompiler(rc, "get_Assembly");
                    else
                        res.PathToAssembly = options.OutputAssembly;
                } catch {
                }
            } finally {
                if (compileDomain != null) AppDomain.Unload(compileDomain);
            }

            return res;
        }

        private static object InvokeCompiler(IReflect compiler, string api) {
            return compiler.InvokeMember(api, BindingFlags.Public, null, null, null, null, null, null);
        }

        private static CompilerResults FromSourceWorker(CompilerParameters options, params string[] sources) {
            string[] tempFiles = new string[sources.Length];
            for (int i = 0; i < tempFiles.Length; i++) {
                tempFiles[i] = options.TempFiles.AddExtension("py", true);
                using (StreamWriter sw = new StreamWriter(tempFiles[i])) {
                    sw.Write(sources[i]);
                }
            }

            return FromFileWorker(options, tempFiles);
        }

        private static CompilerResults FromDomWorker(CompilerParameters options, params CodeCompileUnit[] ea) {
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

        public override void AddError(string path, string message, string lineText, CodeSpan span, int errorCode, Severity severity) {
            string filename;
            int lineNumber;
            GetUpdatedLineInfo(path, span, out filename, out lineNumber);

            compResults.Errors.Add(new CompilerError(filename, 
                lineNumber, 
                span.StartColumn, 
                errorCode.ToString(), 
                message));
            throw new CompilerException();
        }


        private void GetUpdatedLineInfo(string path, CodeSpan span, out string filename, out int line) {
            string[] lines = File.ReadAllLines(path);

            filename = path;
            line = span.StartLine;

            // two passes: 1st search up for the external source, stopping at any EndExternal
            // source regions.  If we don't find it there, search down (stopping again at any
            // EndExternal blocks).  We do this because sometimes our line number information
            // isn't entirely what we'd want it to be.  We fix it in CodeDom today and will fix
            // the parser later.
            for (int pass = 0; pass < 2; pass++) {
                int lineDelta = 0;
                int extSrcOffset = 2; // minus 2 removes external source info
                int dir = -1;
                int startLine;

                switch(pass) {
                    case 0:
                        startLine = Math.Min(span.StartLine - 1, lines.Length - 1);
                        break;
                    case 1:
                        startLine = span.StartLine + 1;
                        dir = 1;
                        extSrcOffset = 0;
                        break;
                    default:
                        throw new InvalidOperationException();  // impossible
                }

                // get updated line info...
                for (int i = startLine; i >= 0 && i < lines.Length; i += dir) {
                    int extSrc;
                    if ((extSrc = lines[i].IndexOf("#ExternalSource")) != -1) {
                        filename = lines[i].Substring(extSrc + 17);
                        for (int j = 0; j < filename.Length; j++) {
                            if (filename[j] == '\\') {
                                if ((j + 1 < filename.Length) && filename[j] == '"')
                                    j++;
                            } else if (filename[j] == '"') {
                                filename = filename.Substring(0, j);
                                break;
                            }
                        }

                        Debug.Assert(lines[i].LastIndexOf(',') != -1);
                        string lineNo = lines[i].Substring(lines[i].LastIndexOf(',') + 1);
                        for (int j = 0; j < lineNo.Length; j++) {
                            if (!Char.IsDigit(lineNo[j])) {
                                line = Convert.ToInt32(lineNo.Substring(0, j)) + (lineDelta - extSrcOffset);
                            }
                        }
                        return;
                    } else if (lines[i].IndexOf("#End ExternalSource") != -1) {
                        break;
                    }
                    lineDelta++;
                }
            }
        }

    }

    /// <summary>
    /// Used to prevent further processing after an error while compiling for CodeDom
    /// 
    /// This is the only exception we catch - all other exceptions propagate to our caller.
    /// </summary>
    class CompilerException : Exception {
        public readonly Compiler.Ast.Node Node;
        public readonly string Filename;

        public CompilerException() {
        }

        public CompilerException(string msg, Compiler.Ast.Node node, string filename)
            : base(msg) {
            Node = node;
            Filename = filename;
        }

        public CompilerException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) {
        }
    }

    public class RemoteCompiler : MarshalByRefObject, IReflect {
        static RemoteCompiler instance;

        string[] files;
        string outAsm;
        bool debugInfo;
        StringCollection refs;
        PEFileKinds peKind;
        Assembly compAssm;
        int errorCnt;
		string mainClass;
        StringCollection resources;
        List<CompilerError> errors = new List<CompilerError>();

        public RemoteCompiler() {
        }

        public RemoteCompiler(string[] fileNames, string outputAssembly, bool includeDebug, StringCollection references, PEFileKinds targetKind, string mainFile, StringCollection resources) {
            instance = this;
            Initialize(fileNames, outputAssembly, includeDebug, references, targetKind, mainFile, resources);
        }

        public void Initialize(string[] fileNames, string outputAssembly, bool includeDebug, StringCollection references, PEFileKinds targetKind, string mainFile, StringCollection resources) {
            files = fileNames;
            outAsm = outputAssembly;
            if (outAsm == null) outAsm = "assembly.dll";
            debugInfo = includeDebug;
            refs = references;
            peKind = targetKind;
			mainClass = mainFile;
            this.resources = resources;
        }

        public void DoCompile() {
            PythonCompiler pc = new PythonCompiler(files, outAsm, new CodeDomCompilerSink(this));
            pc.IncludeDebugInformation = debugInfo;
            pc.TargetKind = peKind;
            pc.AutoImportAll = true;
            pc.StaticTypes = true;
			pc.MainFile = mainClass;
            List<ResourceFile> resource = new List<ResourceFile>();
            foreach(string s in resources) {
                resource.Add(new ResourceFile(Path.GetFileName(s), s));
            }
            pc.ResourceFiles = resource;

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
            } catch (CompilerException ex) {
                // errors occured during compilation
                if (ex.Node == null) {
                    Errors.Add(new CompilerError("unknown", 0, 0, "unknown", ex.Message));
                } else {
                    Errors.Add(new CompilerError(ex.Filename, ex.Node.Start.Line, ex.Node.Start.Line, "0", ex.Message));
                }
                errorCnt = Errors.Count;
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

        public IList<CompilerError> Errors {

            get {
                return errors;
            }
        }

        #region IReflect Members

        public FieldInfo GetField(string name, BindingFlags bindingAttr) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public FieldInfo[] GetFields(BindingFlags bindingAttr) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public MemberInfo[] GetMember(string name, BindingFlags bindingAttr) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public MemberInfo[] GetMembers(BindingFlags bindingAttr) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public MethodInfo[] GetMethods(BindingFlags bindingAttr) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public PropertyInfo[] GetProperties(BindingFlags bindingAttr) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters) {
            switch (name) {
                case "Compile": DoCompile(); break;
                case "get_ErrorCount": return ErrorCount;
                case "get_Errors": return Errors;
                case "get_Assembly": return CompiledAssembly;
            }
            return null;
        }

        public Type UnderlyingSystemType {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        #endregion
    }
}
