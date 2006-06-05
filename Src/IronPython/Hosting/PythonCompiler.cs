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
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

using IronPython.Compiler;
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

        public int Warnings {
            get { return warnings; }
        }

        public int Messages {
            get { return messages; }
        }

        public override void AddError(string path, string message, string lineText, int startLine, int startColumn, int endLine, int endColumn, int errorCode, Severity severity) {
            if(severity >= Severity.Error) errors++;
            else if(severity >= Severity.Warning) warnings++;
            else messages++;

            if (sink != null) {
                sink.AddError(path, message, lineText, startLine, startColumn, endLine, endColumn, errorCode, severity);
            } else {
                throw new Exception(string.Format("{0}:{1} at {2} {3}:{4}-{5}:{6}", severity, message, path, startLine, startColumn, endLine, endColumn));
            }
        }
    }


    public class PythonCompiler {
        private IList<string> sourceFiles;
        private SystemState state = new SystemState();

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

        private string mainFile = null;

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
                sink.AddError("", string.Format("Need an entry point for target kind {0}", targetKind), -1, Severity.Error);
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
            Stmt body = p.ParseFileInput();

            if (sink.Errors > 0) return;

            GlobalSuite gs = IronPython.Compiler.Binder.Bind(body, context);
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

                        FromImportStmt stmt = new FromImportStmt(
                            new DottedName(new SymbolId[] { SymbolTable.StringToId(otherModName) }),
                            FromImportStmt.Star, null);
                        stmt.start = dummyLocation;
                        stmt.end = dummyLocation;
                        stmt.Emit(cg);
                    }

                    // Import the first part of all namespaces in all referenced assemblies

                    // First, determine the set of unique such prefixes
                    Dictionary<string, object> nsPrefixes = new Dictionary<string, object>();
                    foreach (string assemblyPath in ReferencedAssemblies) {
                        Assembly a;
                        try {
                            a = Assembly.LoadFrom(assemblyPath);
                        } catch {
                            // Ignore assemblies that can't be loaded
                            continue;
                        }

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
                        ImportStmt importStmt = new ImportStmt(
                            new DottedName[] { dottedName },
                            new SymbolId[] { SymbolTable.Empty });
                        importStmt.start = dummyLocation;
                        importStmt.end = dummyLocation;
                        importStmt.Emit(cg);
                    }
                });
            }
            return init;
        }        
    }
}
