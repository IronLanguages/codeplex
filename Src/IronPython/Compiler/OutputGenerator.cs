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
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Threading;
using IronPython.Runtime;

namespace IronPython.Compiler {

    // IronPython has two units of compilation:
    // 1. Snippets - These are small pieces of code compiled for these cases:
    //    a. Interactive console expressions and statements
    //    b. exec statement, eval() built-in function
    //    c. Types generated by NewTypeMaker for instances of Python types
    //    d. others ...
    //    All snippets are created in the snippetAssembly. These are created using GenerateSnippet.
    // 2. Modules - Modules are compiled in one shot when they are imported.
    //    Every Python module is generated into its own assembly. These are created using GenerateModule.
    //
    // OutputGenerator manages both units of compilation.

    public static class OutputGenerator {
        private static int count = 0;
        private static AssemblyGen CreateNewSnippetAssembly() {
            string name = "snippets";
            int thisCnt = Interlocked.Increment(ref count);
            if (thisCnt > 0) name += thisCnt;
            return new AssemblyGen(name, Environment.CurrentDirectory, name + ".dll", Options.ILDebug);
        }

        // All snippets are created in this shared assembly
        private static AssemblyGen snippetAssembly = CreateNewSnippetAssembly();
        public static AssemblyGen Snippets {
            get {
                return snippetAssembly;
            }
        }

        public static void DumpSnippets() {
            if (Options.SaveAndReloadBinaries) {
                snippetAssembly.Dump();
            }
            snippetAssembly = CreateNewSnippetAssembly();
        }

        internal static FrameCode GenerateSnippet(CompilerContext context, Stmt body, bool printExprStmts) {
            return GenerateSnippet(context, body, context.SourceFile, printExprStmts);
        }

        public static FrameCode GenerateSnippet(CompilerContext context, Stmt body, string name, bool printExprStmts) {
            GlobalSuite gs = Binder.Bind(body, context);

            if (name.Length == 0) name = "<empty>"; // The empty string isn't a legal method name
            CodeGen cg;
            List<object> staticData = null;
            TypeGen tg = null;
            cg = snippetAssembly.DefineDynamicMethod(name, typeof(object), new Type[] { typeof(Frame) });
            staticData = new List<object>();
            cg.staticData = staticData;
            cg.doNotCacheConstants = true;
            cg.ModuleSlot = cg.GetLocalTmp(typeof(PythonModule));
            cg.ContextSlot = cg.GetArgumentSlot(0);
            cg.Names = CodeGen.CreateFrameNamespace(cg.ContextSlot);
            cg.Context = context;
            cg.printExprStmts = printExprStmts;
            if (printExprStmts) {
                cg.Names.EnsureLocalSlot(Name.Make("_"));
            }

            cg.ContextSlot.EmitGet(cg);
            cg.EmitFieldGet(typeof(Frame), "__module__");
            cg.ModuleSlot.EmitSet(cg);

            if (context.TrueDivision) {
                cg.ContextSlot.EmitGet(cg);
                cg.EmitInt(1);
                cg.EmitCall(typeof(ICallerContext), "set_TrueDivision");
            }

            gs.Emit(cg);

            if (!(body is ReturnStmt)) {
                cg.EmitPosition(Location.None, Location.None);
                cg.EmitReturn(null);
            }

            if (tg != null) tg.FinishType();

            FrameCode frameCode = new FrameCode(name,
                (FrameCodeDelegate)cg.CreateDelegate(typeof(FrameCodeDelegate)),
                staticData);
            return frameCode;
        }

        public static PythonModule GenerateModule(SystemState state, CompilerContext context, Stmt body, string moduleName) {
            if (Options.GenerateModulesAsSnippets) {
                return GenerateModuleAsSnippets(state, context, body, moduleName);
            }

            GlobalSuite gs = IronPython.Compiler.Binder.Bind(body, context);
            string suffix = "";
            int counter = 0;

            for (; ; ) {
                try {
                    return DoGenerateModule(state, context, gs, moduleName, context.SourceFile, suffix);
                } catch (System.IO.IOException) {
                    suffix = "_" + (++counter).ToString();
                }
            }
        }

        public static PythonModule GenerateModule(SystemState state, CompilerContext context, Stmt body, string moduleName, string outSuffix) {
            if (Options.GenerateModulesAsSnippets) {
                return GenerateModuleAsSnippets(state, context, body, moduleName);
            }

            GlobalSuite gs = IronPython.Compiler.Binder.Bind(body, context);
            return DoGenerateModule(state, context, gs, moduleName, context.SourceFile, outSuffix);
        }

        internal class SnippetModuleRunner {
            PythonModule module;
            Frame frame;
            Dict dict = new Dict();
            List<FrameCode> snippets = new List<FrameCode>();

            public PythonModule Module {
                get {
                    return module;
                }
            }

            public SnippetModuleRunner(string name, SystemState state) {
                module = new PythonModule(name, dict, state, this.Initialize);
                frame = new Frame(module);
            }

            public void AddSnippet(FrameCode fc) {
                snippets.Add(fc);
            }

            public void Initialize() {
                foreach (FrameCode fc in snippets) {
                    fc.Run(frame);
                }
            }
        }

        private static PythonModule GenerateModuleAsSnippets(SystemState state, CompilerContext context, Stmt body, string moduleName) {
            SuiteStmt suite = body as SuiteStmt;
            SnippetModuleRunner smr = new SnippetModuleRunner(moduleName, state);

            Debug.Assert(suite != null, "invalid statement");

            //  Convert document string into assignment
            if (suite.stmts.Length > 0) {
                ExprStmt es = suite.stmts[0] as ExprStmt;
                if (es != null) {
                    ConstantExpr ce = es.expr as ConstantExpr;
                    if (ce != null && ce.value is string) {
                        suite.stmts[0] = new AssignStmt(new Expr[] { new NameExpr(Name.Make("__doc__")) }, ce);
                    }
                }
            }

            foreach (Stmt stmt in suite.stmts) {
                // GenerateSnippet will do the binding
                smr.AddSnippet(GenerateSnippet(context, stmt, moduleName, true));
            }
            return smr.Module;
        }

        private static PythonModule DoGenerateModule(SystemState state, CompilerContext context, GlobalSuite gs, string moduleName, string sourceFileName, string outSuffix) {
            string fullPath;
            string outDir;
            string fileName;

            if (sourceFileName == "<stdin>") {
                fullPath = Environment.CurrentDirectory;
                outDir = Environment.CurrentDirectory;
                fileName = "__stdin__";
            } else {
                fullPath = Path.GetFullPath(sourceFileName);
                outDir = Options.BinariesDirectory == null? Path.GetDirectoryName(fullPath) : Options.BinariesDirectory;
                fileName = Path.GetFileNameWithoutExtension(sourceFileName);
            }

            AssemblyGen ag = new AssemblyGen(moduleName + outSuffix, outDir, fileName + outSuffix + ".exe", true);
            ag.SetPythonSourceFile(fullPath);


            TypeGen tg = GenerateModuleType(moduleName, ag);
            CodeGen cg = GenerateModuleInitialize(context, gs, tg);

            CodeGen main = GenerateModuleEntryPoint(tg, cg, moduleName, null);
            ag.SetEntryPoint(main.MethodInfo, PEFileKinds.ConsoleApplication);
            ag.AddPythonModuleAttribute(tg, moduleName);

            Type ret = tg.FinishType();
            Assembly assm = ag.DumpAndLoad();
            ret = assm.GetType(moduleName);

            CustomSymbolDict dict = (CustomSymbolDict)ret.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
            InitializeModule init = (InitializeModule)Delegate.CreateDelegate(
                typeof(InitializeModule), dict, "Initialize");

            PythonModule pmod = new PythonModule(moduleName, dict, state, init);

            pmod.InitializeBuiltins();
            return pmod;
        }

        internal delegate void CustomModuleInit(CodeGen cg);

        internal static CodeGen GenerateModuleInitialize(CompilerContext context, GlobalSuite gs, TypeGen tg) {
            return GenerateModuleInitialize(context, gs, tg, null);
        }

        internal static CodeGen GenerateModuleInitialize(CompilerContext context, GlobalSuite gs, TypeGen tg, CustomModuleInit customInit) {
            CodeGen ncg = tg.DefineUserHiddenMethod(MethodAttributes.Public, "Initialize", typeof(void), Type.EmptyTypes);
            ncg.Context = context;

            if (Options.StaticModules) {
                ncg.Names = CodeGen.CreateStaticFieldNamespace(tg);
            } else {
                throw new NotImplementedException(); //.names = new FieldNamespace(null, tg, new ModuleSlot(tg.myType));
            }

            if (context.TrueDivision) {
                ncg.ModuleSlot.EmitGet(ncg);
                ncg.EmitInt(1);
                ncg.EmitCall(typeof(ICallerContext), "set_TrueDivision");
            }

            // Add __doc__ and __name__
            ncg.Names.CreateGlobalSlot(Name.Make("__doc__"));
            ncg.Names.CreateGlobalSlot(Name.Make("__name__"));

            string doc = gs.GetDocString();
            ncg.EmitStringOrNull(doc);
            ncg.EmitSet(Name.Make("__doc__"));

            if (customInit != null) customInit(ncg);

            if (Options.StaticTypes) {
                TypeGen finished = UserTypeGenerator.DoStaticCompilation(gs, ncg);
            } else {
                gs.Emit(ncg);
            }

            ncg.EmitPosition(Location.None, Location.None);
            ncg.EmitReturn();

            FinishCustomDict(tg, ncg.Names);

            return ncg;
        }

        /// <summary>
        /// Generates a static entry point for stand-alone EXEs.  We just new up our module dstructure
        /// and then call into Ops to get running.
        /// </summary>
        internal static CodeGen GenerateModuleEntryPoint(TypeGen tg, CodeGen init, string moduleName, IList<string> referencedAssemblies) {
            CodeGen main = tg.DefineMethod(MethodAttributes.Static | MethodAttributes.Public, "Main", typeof(int), Type.EmptyTypes, new string[] { });
            main.SetCustomAttribute(new CustomAttributeBuilder(typeof(STAThreadAttribute).GetConstructor(Type.EmptyTypes), new object[0]));

            // leaves our module instance on the stack, we save it to create the delegate.
            Slot instance = new LocalSlot(main.DeclareLocal(tg.myType), main);            
            // notify the PythonEngine of the module.
            EmitModuleConstruction(tg, main, moduleName, instance, referencedAssemblies);
            
            main.Emit(OpCodes.Pop); // we don't care about the PythonModule.

            // Emit the delegate to the init method (init)
            main.EmitDelegate(init, typeof(InitializeModule), instance);    

            // Call ExecuteCompiled
            main.EmitCall(typeof(IronPython.Hosting.PythonEngine), "ExecuteCompiled");
            
            main.EmitReturn();

            return main;
        }

        /// <summary>
        /// Emits a call into the PythonModule to register this module, and then
        /// returns the resulting PythonModule 
        /// </summary>
        public static void EmitModuleConstruction(TypeGen tg, CodeGen main, string moduleName, Slot initialize, IList<string> referencedAssemblies) {
            // calling PythonModule InitializeModule(CustomDict compiled, string fullName)

            main.EmitNew(tg.DefaultConstructor); // Emit instance for the InitializeModule call (compiled)

            initialize.EmitSet(main);
            initialize.EmitGet(main);
            
            main.EmitString(moduleName);         // emit module name (fullName)

            // emit the references assemblies
            if (referencedAssemblies != null) {
                for (int i = 0; i < referencedAssemblies.Count; i++) {
                    if (referencedAssemblies[i].ToLower().EndsWith("\\ironpython.dll")) {
                        referencedAssemblies.RemoveAt(i);
                        i--;
                    } else {
                        if (referencedAssemblies[i].IndexOf(Path.DirectorySeparatorChar) != -1) {
                            referencedAssemblies[i] = referencedAssemblies[i].Substring(referencedAssemblies[i].LastIndexOf(Path.DirectorySeparatorChar) + 1);
                        }

                        if (referencedAssemblies[i].ToLower().EndsWith(".dll")) {
                            referencedAssemblies[i] = referencedAssemblies[i].Substring(0, referencedAssemblies[i].Length - 4);
                        }
                    }
                }
                main.EmitStringArray(referencedAssemblies);
            } else main.Emit(OpCodes.Ldnull);

            // Call InitializeModule
            main.EmitCall(typeof(IronPython.Hosting.PythonEngine), "InitializeModule");
        }

        internal static TypeGen GenerateModuleType(string moduleName, AssemblyGen ag) {
            TypeGen tg = ag.DefinePublicType(moduleName, typeof(CustomSymbolDict));
            tg.AddModuleField(typeof(PythonModule));
            tg.DefaultConstructor = tg.myType.DefineDefaultConstructor(MethodAttributes.Public);

            return tg;
        }

        private static void FinishCustomDict(TypeGen tg, Namespace ns) {
            DictBuilder db = new DictBuilder(tg, ns);
            db.AddCustomDictMethods();
        }
    }

    class DictBuilder {
        private TypeGen tg;
        private Namespace names;

        public DictBuilder(TypeGen tg, Namespace names) {
            this.tg = tg;
            this.names = names;
        }

        public void AddCustomDictMethods() {
            MakeGetMethod();
            MakeSetMethod();
            MakeRawKeysMethod();
        }

        //
        // This generates a method like the following:
        //
        //  TryGetExtraValue(int name, object out value) {
        //      if (name1 == name) {
        //          value = type.name1Slot;
        //          return true;
        //      }
        //      if (name2 == name) {
        //          value = type.name2Slot;
        //          return true;
        //      }
        //      ...
        //      return false
        //  }

        private void MakeGetMethod() {
            CodeGen cg = tg.DefineMethodOverride(typeof(CustomSymbolDict).GetMethod("TryGetExtraValue"));
            foreach (KeyValuePair<Name, Slot> entry in names.Slots) {
                cg.EmitSymbolIdInt(entry.Key.GetString());
                cg.EmitArgAddr(0);
                cg.EmitFieldGet(typeof(SymbolId), "Id");

                Label next = cg.DefineLabel();
                cg.Emit(OpCodes.Bne_Un, next);

                cg.EmitArgGet(1);

                // ugly special casing builtins this way, but they're special...
                // Builtins can be in one of two states:
                //      Not defined, but available internally to the module
                //      Defined, and exposed externally from the module.
                // In both cases we have static fields for these, and we're in the custom dict.
                // 
                // Therefore we keep track of which state they're in w/ a wrapper object,
                // and here we need to emit the real value, not the value that's availble
                // internally to the module.

                StaticFieldBuiltinSlot builtin = entry.Value as StaticFieldBuiltinSlot;
                if (builtin == null) {
                    entry.Value.EmitGet(cg);
                } else {
                    builtin.EmitGetRaw(cg);
                }

                cg.Emit(OpCodes.Stind_Ref);
                cg.EmitInt(1);
                cg.EmitReturn();
                cg.MarkLabel(next);
            }
            cg.EmitInt(0);
            cg.EmitReturn();
            cg.Finish();
        }

        // This generates a method like the following:
        //
        //  TrySetExtraValue(object name, object value) {
        //      if (name1 == name) {
        //          type.name1Slot = value;
        //          return 1;
        //      }
        //      if (name2 == name) {
        //          type.name2Slot = value;
        //          return 1;
        //      }
        //      ...
        //      return 0
        //  }

        private void MakeSetMethod() {
            CodeGen cg = tg.DefineMethodOverride(typeof(CustomSymbolDict).GetMethod("TrySetExtraValue"));
            Slot valueSlot = cg.GetArgumentSlot(1);
            foreach (KeyValuePair<Name, Slot> entry in names.Slots) {
                cg.EmitSymbolIdInt(entry.Key.GetString());
                cg.EmitArgAddr(0);
                cg.EmitFieldGet(typeof(SymbolId), "Id");

                Label next = cg.DefineLabel();
                cg.Emit(OpCodes.Bne_Un, next);

                entry.Value.EmitSet(cg, valueSlot);
                cg.EmitInt(1);
                cg.EmitReturn();
                cg.MarkLabel(next);
            }
            cg.EmitInt(0);
            cg.EmitReturn();
            cg.Finish();
        }

        private void MakeRawKeysMethod() {
            Slot rawKeysCache = tg.AddStaticField(typeof(SymbolId[]), "ExtraKeysCache");
            CodeGen init = tg.GetOrMakeInitializer();

            init.EmitSymbolIdArray(new List<Name>(names.Slots.Keys));

            rawKeysCache.EmitSet(init);

            CodeGen cg = tg.DefineMethodOverride(typeof(CustomSymbolDict).GetMethod("GetExtraKeys"));
            rawKeysCache.EmitGet(cg);
            cg.EmitReturn();
            cg.Finish();
        }
    }
}
