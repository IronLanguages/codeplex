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
using System.Resources;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Threading;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Generation {
    class AssemblyGen {
        public static readonly Guid PythonLanguageGuid = new Guid("03ed4b80-d10b-442f-ad9a-47dae85b2051");
        public readonly AssemblyBuilder myAssembly;
        internal ModuleBuilder myModule;
        public ISymbolDocumentWriter sourceFile;
        private readonly string outFileName;
        private readonly string outDir;
        private readonly bool emitDebugInfo;
        private readonly bool staticTypes;
        private int index;
        private PortableExecutableKinds peKind;
        private ImageFileMachine machine;

        public AssemblyGen(string moduleName, string outDir, string outFile, bool emitDebugInfo)
            : this(moduleName, outDir, outFile, emitDebugInfo, false /*staticTypes*/,
                   PortableExecutableKinds.ILOnly, ImageFileMachine.I386) {
        }

        public AssemblyGen(string moduleName, string outDir, string outFile, bool emitDebugInfo,
            bool staticTypes, PortableExecutableKinds peKind, ImageFileMachine machine) {
            this.outFileName = outFile;
            this.outDir = outDir;
            this.emitDebugInfo = emitDebugInfo;
            this.staticTypes = staticTypes;
            this.peKind = peKind;
            this.machine = machine;

            AssemblyName asmname = new AssemblyName();

            AppDomain domain = System.Threading.Thread.GetDomain();
            asmname.Name = Path.GetFileNameWithoutExtension(outFileName);

            if (outFileName == null) {
                myAssembly = domain.DefineDynamicAssembly(
                                    asmname,
                                    AssemblyBuilderAccess.Run,
                                    outDir,
                                    null);
                myModule = myAssembly.DefineDynamicModule(moduleName);
            } else {
                myAssembly = domain.DefineDynamicAssembly(
                                    asmname,
                                    AssemblyBuilderAccess.RunAndSave,
                                    outDir,
                                    null);
                myModule = myAssembly.DefineDynamicModule(outFileName, outFileName, emitDebugInfo);

            }

            myAssembly.DefineVersionInfoResource();

            if (emitDebugInfo) SetDebuggableAttributes();
        }
        private void SetDebuggableAttributes() {
            Type[] argTypes = new Type[] { typeof(DebuggableAttribute.DebuggingModes) };
            Object[] argValues = new Object[] {
                DebuggableAttribute.DebuggingModes.Default |
                DebuggableAttribute.DebuggingModes.DisableOptimizations |
                DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints };

            myAssembly.SetCustomAttribute(new CustomAttributeBuilder(
               typeof(DebuggableAttribute).GetConstructor(argTypes), argValues)
               );

            myModule.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(DebuggableAttribute).GetConstructor(argTypes), argValues)
                );
        }
        public void SetPythonSourceFile(string sourceFileName) {
            if (emitDebugInfo) {
                sourceFile =
                    myModule.DefineDocument(sourceFileName,
                    PythonLanguageGuid, SymLanguageVendor.Microsoft, SymDocumentType.Text);
            }
        }
        public void AddResourceFile(string name, string file, ResourceAttributes attribute) {
            IResourceWriter rw = myModule.DefineResource(Path.GetFileName(file), name, attribute);

            string ext = Path.GetExtension(file).ToLower();
            if (ext == ".resources") {
                ResourceReader rr = new ResourceReader(file);
                using (rr) {
                    System.Collections.IDictionaryEnumerator de = rr.GetEnumerator();

                    while (de.MoveNext()) {
                        string key = de.Key as string;
                        rw.AddResource(key, de.Value);
                    }
                }
            } else {
                rw.AddResource(name, File.ReadAllBytes(file));
            }
        }
        public Assembly DumpAndLoad() {
            if (!Options.SaveAndReloadBinaries) {
                return myAssembly;
            }

            string fullPath = Path.Combine(outDir, outFileName);
            Dump();

            if (Options.ReadBinariesByteArray) {
                byte[] il = File.ReadAllBytes(fullPath);
                byte[] pdb = File.ReadAllBytes(Path.ChangeExtension(fullPath, "pdb"));
                return Assembly.Load(il, pdb);
            }

            return Assembly.LoadFile(fullPath);
        }
        public void Dump() {
            myAssembly.Save(outFileName, peKind, machine);
#if DEBUG
            if (IronPython.Hosting.PythonEngine.options != null && // This gets called from the PythonEngine constructor
                IronPython.Hosting.PythonEngine.options.EngineDebug) {
                PeVerifyThis();
            }
#endif
        }
#if DEBUG

        private const string peverify_exe = "peverify.exe";

        private static string FindPeverify() {
            string path = System.Environment.GetEnvironmentVariable("PATH");
            string[] dirs = path.Split(';');
            foreach (string dir in dirs) {
                string file = Path.Combine(dir, peverify_exe);
                if (File.Exists(file)) {
                    return file;
                }
            }
            return null;
        }

        private void PeVerifyThis() {
            string peverifyPath = FindPeverify();
            if (peverifyPath == null) {
                return;
            }

            int exitCode = 0;
            string strOut = null;
            string verifyFile = null;

            try {
                string pythonPath = new FileInfo(typeof(Ops).Assembly.Location).DirectoryName;

                string assemblyFile = Path.Combine(outDir, outFileName).ToLower();
                string assemblyName = Path.GetFileNameWithoutExtension(outFileName);
                string assemblyExtension = Path.GetExtension(outFileName);
                Random rnd = new System.Random();

                for (int i = 0; ; i++) {
                    string verifyName = string.Format("{0}_{1}_{2}{3}", assemblyName, i, rnd.Next(1, 100), assemblyExtension);
                    verifyName = Path.Combine(pythonPath, verifyName);

                    try {
                        File.Copy(assemblyFile, verifyName);
                        verifyFile = verifyName;
                        break;
                    } catch (IOException) {
                    }
                }

                ProcessStartInfo psi = new ProcessStartInfo(peverifyPath, verifyFile);
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                Process proc = Process.Start(psi);
                Thread thread = new Thread(
                    new ThreadStart(
                        delegate {
                            using (StreamReader sr = proc.StandardOutput) {
                                strOut = sr.ReadToEnd();
                            }
                        }
                        ));

                thread.Start();
                proc.WaitForExit();
                thread.Join();
                exitCode = proc.ExitCode;
                proc.Close();
            } catch {
                exitCode = 1;
            }

            if (exitCode != 0) {
                throw Ops.RuntimeError("Non-verifiable assembly generated: {0}:\nAssembly preserved as {1}\nError text:\n{2}\n",
                    outFileName, verifyFile, strOut == null ? "" : strOut);
            }

            if (verifyFile != null) {
                File.Delete(verifyFile);
            }
        }
#endif

        public TypeGen DefinePublicType(string name, Type parent) {
            TypeAttributes attrs = TypeAttributes.Public;
            if (staticTypes) attrs |= TypeAttributes.BeforeFieldInit;
            TypeBuilder tb = myModule.DefineType(name, attrs);
            tb.SetParent(parent);
            return new TypeGen(this, tb);
        }
        public CodeGen DefineDynamicMethod(string methodName, Type returnType, Type[] paramTypes) {
            CodeGen cg;
            if (Options.GenerateDynamicMethods) {
                DynamicMethod target = new DynamicMethod(methodName + "##" + index++, returnType, paramTypes, myModule);
                cg = new CodeGen(this, null, target, target.GetILGenerator(), paramTypes);
                //Console.WriteLine("----> {0} DynamicMethod", target.Name);
            } else {
                TypeGen tg = DefinePublicType("Type" + methodName + index++, typeof(object));
                cg = tg.DefineUserHiddenMethod(MethodAttributes.Public | MethodAttributes.Static,
                    "Handle", returnType, paramTypes);
            }
            return cg;
        }
        public void SetEntryPoint(MethodInfo mi, PEFileKinds kind) {
            myAssembly.SetEntryPoint(mi, kind);
        }

        public void AddPythonModuleAttribute(TypeGen tg, string moduleName) {
            myAssembly.SetCustomAttribute(new CustomAttributeBuilder(
               typeof(PythonModuleAttribute).GetConstructor(
               new Type[] { typeof(string), typeof(Type) }),
               new Object[] { moduleName, tg.myType }));
        }
    }
}

