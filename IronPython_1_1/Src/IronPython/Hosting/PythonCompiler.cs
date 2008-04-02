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
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

using IronPython.Compiler;
using IronPython.Compiler.Generation;
using IronPython.Compiler.Ast;
using IronPython.Modules;
using IronPython.Runtime;

namespace IronPython.Hosting {
    public class ResourceFile {
        private string name;
        private string file;
        private bool publicResource;

        public string Name {
            get { return name; }
            set { name = value; }
        }

        public string File {
            get { return file; }
            set { file = value; }
        }

        public bool PublicResource {
            get { return publicResource; }
            set { publicResource = value; }
        }

        public ResourceFile(string name, string file)
            : this(name, file, true) {
        }

        public ResourceFile(string name, string file, bool publicResource) {
            this.name = name;
            this.file = file;
            this.publicResource = publicResource;
        }
    }

    internal class PythonCompilerSink : CompilerSink {
        private CompilerSink sink;
        private int errors;
        private int warnings;
        private int messages;

        public PythonCompilerSink(CompilerSink sink) {
            this.sink = sink;
        }

        public int Errors {
            get { return errors; }
        }

        public override void AddError(string path, string message, string lineText, CodeSpan span, int errorCode, Severity severity) {
            if (severity >= Severity.Error) errors++;
            else if (severity >= Severity.Warning) warnings++;
            else messages++;

            if (sink != null) {
                sink.AddError(path, message, lineText, span, errorCode, severity);
            } else {
                throw new CompilerException(string.Format("{0}:{1} at {2} {3}:{4}-{5}:{6}", severity, message, path,
                                                  span.StartLine, span.StartColumn, span.EndLine, span.EndColumn));
            }
        }
    }


    public class PythonCompiler : IDisposable {
        private IList<string> sourceFiles;
        private SystemState state;

        public IList<string> SourceFiles {
            get { return sourceFiles; }
            set { sourceFiles = value; }
        }

        private string outputAssembly;

        public string OutputAssembly {
            get { return outputAssembly; }
            set { outputAssembly = value; }
        }

        private IList<string> referencedAssemblies = new List<string>();

        public IList<string> ReferencedAssemblies {
            get { return referencedAssemblies; }
            set { referencedAssemblies = value; }
        }

        private string mainFile;

        public string MainFile {
            get { return mainFile; }
            set { mainFile = value; }
        }

        private PEFileKinds targetKind = PEFileKinds.ConsoleApplication;

        public PEFileKinds TargetKind {
            get { return targetKind; }
            set { targetKind = value; }
        }

        private PortableExecutableKinds executable = PortableExecutableKinds.ILOnly;
        private ImageFileMachine machine = ImageFileMachine.I386;

        public PortableExecutableKinds ExecutableKind {
            get { return executable; }
            set { executable = value; }
        }

        public ImageFileMachine Machine {
            get { return machine; }
            set { machine = value; }
        }

        private bool includeDebugInformation = true;

        public bool IncludeDebugInformation {
            get { return includeDebugInformation; }
            set { includeDebugInformation = value; }
        }

        private bool staticTypes = false;

        public bool StaticTypes {
            get { return staticTypes; }
            set { staticTypes = value; }
        }

        private bool autoImportAll = false;
        public bool AutoImportAll {
            get { return autoImportAll; }
            set { autoImportAll = value; }
        }

        private AssemblyGen assemblyGen;

        private CompilerSink compilerSink;
        public CompilerSink CompilerSink {
            get { return compilerSink; }
            set { compilerSink = value; }
        }

        private IList<ResourceFile> resourceFiles;

        public IList<ResourceFile> ResourceFiles {
            get { return resourceFiles; }
            set { resourceFiles = value; }
        }

        public PythonCompiler(IList<string> sourceFiles, string outputAssembly)
            : this(sourceFiles, null, outputAssembly, null) {
        }

        public PythonCompiler(IList<string> sourceFiles, string outputAssembly, CompilerSink compilerSink)
            : this(sourceFiles, null, outputAssembly, compilerSink) {
        }

        public PythonCompiler(IList<string> sourceFiles, IList<ResourceFile> resourceFiles, string outputAssembly)
            : this(sourceFiles, resourceFiles, outputAssembly, null) {
        }

        public PythonCompiler(IList<string> sourceFiles, IList<ResourceFile> resourceFiles, string outputAssembly, CompilerSink compilerSink) {
            this.sourceFiles = sourceFiles;
            this.resourceFiles = resourceFiles;
            this.outputAssembly = outputAssembly;
            this.compilerSink = compilerSink;
            this.state = new SystemState();
        }

        public void Compile() {
            string fullPath = Path.GetFullPath(outputAssembly);
            string outDir = Path.GetDirectoryName(fullPath);
            string fileName = Path.GetFileName(outputAssembly);

            PythonCompilerSink sink = new PythonCompilerSink(compilerSink);

            assemblyGen = new AssemblyGen(
                Path.GetFileNameWithoutExtension(outputAssembly),
                outDir, fileName, includeDebugInformation, staticTypes, executable, machine
                );

            bool entryPointSet = false;

            // set default main file
            if (mainFile == null && sourceFiles.Count == 1 && targetKind != PEFileKinds.Dll) {
                mainFile = sourceFiles[0];
            }

            foreach (string sourceFile in sourceFiles) {
                bool createMainMethod = sourceFile == mainFile;
                CompilePythonModule(sourceFile, sink, createMainMethod);

                if (sink.Errors > 0) return;

                if (createMainMethod) {
                    entryPointSet = true;
                }
            }

            if (resourceFiles != null) {
                foreach (ResourceFile rf in resourceFiles) {
                    assemblyGen.AddResourceFile(rf.Name, rf.File, rf.PublicResource ? ResourceAttributes.Public : ResourceAttributes.Private);
                }
            }

            if (targetKind != PEFileKinds.Dll && !entryPointSet) {
                sink.AddError("", string.Format("Need an entry point for target kind {0}", targetKind), String.Empty, CodeSpan.Empty, -1, Severity.Error);
            }

            assemblyGen.Dump();
        }

        private static string GetModuleFromFilename(string filename) {
            string ext = Path.GetExtension(filename);
            if (ext.ToLower().EndsWith("py"))
                return Path.GetFileNameWithoutExtension(filename).Replace('.', '_');
            return Path.GetFileName(filename).Replace('.', '_');
        }

        private void CompilePythonModule(string fileName, PythonCompilerSink sink, bool createMain) {
            assemblyGen.SetPythonSourceFile(fileName);
            CompilerContext context = new CompilerContext(fileName, sink);
            Parser p = Parser.FromFile(state, context);
            Statement body = p.ParseFileInput();

            if (sink.Errors > 0) return;

            GlobalSuite gs = Compiler.Ast.Binder.Bind(body, context);
            string moduleName = GetModuleFromFilename(fileName);
            TypeGen tg = OutputGenerator.GenerateModuleType(moduleName, assemblyGen);
            CodeGen init = CompileModuleInit(context, gs, tg, moduleName);

            if (createMain) {
                CodeGen main = OutputGenerator.GenerateModuleEntryPoint(tg, init, moduleName, referencedAssemblies);
                assemblyGen.SetEntryPoint(main.MethodInfo, targetKind);
            }

            assemblyGen.AddPythonModuleAttribute(tg, moduleName);
            tg.FinishType();
        }

        private CodeGen CompileModuleInit(CompilerContext context, GlobalSuite gs, TypeGen tg, string moduleName) {
            CodeGen init;
            if (!AutoImportAll) {
                init = OutputGenerator.GenerateModuleInitialize(context, gs, tg);
            } else {
                // auto-import all compiled modules, useful for CodeDom scenarios.
                init = OutputGenerator.GenerateModuleInitialize(context, gs, tg, staticTypes, delegate(CodeGen cg) {

                    Location dummyLocation = new Location(1, 1);

                    for (int i = 0; i < sourceFiles.Count; i++) {
                        string otherModName = GetModuleFromFilename(sourceFiles[i]);
                        if (otherModName == moduleName) continue;

                        FromImportStatement stmt = new FromImportStatement(
                            new DottedName(new SymbolId[] { SymbolTable.StringToId(otherModName) }),
                            FromImportStatement.Star, null);
                        stmt.Start = dummyLocation;
                        stmt.End = dummyLocation;
                        stmt.Emit(cg);
                    }

                    // Import the first part of all namespaces in all referenced assemblies

                    // First, determine the set of unique such prefixes
                    Dictionary<string, object> nsPrefixes = new Dictionary<string, object>();
                    foreach (string name in ReferencedAssemblies) {
                        Assembly a = LoadAssembly(name);

                        foreach (Type t in a.GetTypes()) {
                            // We only care about public types
                            if (!t.IsPublic) continue;

                            // Ignore types that don't have a namespace
                            if (t.Namespace == null) continue;

                            string nsPrefix = t.Namespace.Split('.')[0];
                            nsPrefixes[nsPrefix] = null;
                        }
                    }

                    // Import all the uniquer prefixes we found
                    foreach (string nsPrefix in nsPrefixes.Keys) {
                        SymbolId symbolId = SymbolTable.StringToId(nsPrefix);
                        cg.Names.CreateGlobalSlot(symbolId);
                        DottedName dottedName = new DottedName(new SymbolId[] { symbolId });
                        ImportStatement importStmt = new ImportStatement(
                            new DottedName[] { dottedName },
                            new SymbolId[] { SymbolTable.Empty });
                        importStmt.Start = dummyLocation;
                        importStmt.End = dummyLocation;
                        importStmt.Emit(cg);
                    }
                });
            }
            return init;
        }

        // Load an assembly based on a string that may be a full path, and full assembly name,
        // or a partial assembly name.
        internal static Assembly LoadAssembly(string name) {
            // If it's an existing file, just call LoadFrom
            if (File.Exists(name))
                return Assembly.LoadFrom(name);

            // Get rid of the path
            name = Path.GetFileName(name);

            // Get rid of the .dll extension before treating it as an assembly name
            if (name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)) {
                name = Path.GetFileNameWithoutExtension(name);
            }

            try {
                return Assembly.Load(name);
            } catch {
#pragma warning disable
                return Assembly.LoadWithPartialName(name);
#pragma warning enable
            }
        }

        ~PythonCompiler() {
            Dispose(true);
        }

        #region IDisposable Members

        public void Dispose() {
            Dispose(false);
        }

        #endregion

        private void Dispose(bool finalizing) {
            // if we're finalizing we shouldn't access other managed objects, as
            // their finalizers may have already run            
            if (!finalizing) {
                state.Dispose();
            }
        }
    }
}
