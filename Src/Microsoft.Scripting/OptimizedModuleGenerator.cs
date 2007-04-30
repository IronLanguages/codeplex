/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.IO;
using System.Threading;
using System.Security.Permissions;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Creates the code for an optimized module.  
    /// </summary>
    abstract class OptimizedModuleGenerator {
        private ScriptCode[] _scriptCodes;
        private Dictionary<LanguageContext, ScopeAllocator> _allocators = new Dictionary<LanguageContext, ScopeAllocator>();

        protected OptimizedModuleGenerator(params ScriptCode[] scriptCodes) {
            _scriptCodes = scriptCodes;
        }

        public static Scope GenerateOptimizedCode(params ScriptCode[] scriptCodes) {
            if (scriptCodes == null) throw new ArgumentNullException("scriptCodes");
            if (scriptCodes.Length == 0) throw new ArgumentException("scriptCodes", "must have at least one ScriptCode");
            if (scriptCodes.Length != 1) throw new ArgumentException("scriptCodes", "only one ScriptCode currently supported");

            // Silverlight: can't access SecurityPermission
            if (!ScriptDomainManager.Options.GenerateModulesAsSnippets) {
#if !SILVERLIGHT
                try {
                    // CreateStaticCodeGenerator requires ReflectionEmit (in CLR V2) or UnmanagedCode (in CLR V2 SP1) permission.
                    // If we are running in partial-trust, fall through to generated dynamic code.
                    // TODO: Symbol information requires unmanaged code permission.  Move the error
                    // handling lower and just don't dump PDBs.
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

#endif
                    return new StaticFieldModuleGenerator(scriptCodes).Generate();
#if !SILVERLIGHT
                } catch (SecurityException) {
                }
#endif
            }

            return new TupleModuleGenerator(scriptCodes).Generate();
        }

        /// <summary>
        /// Creates the methods and optimized Scope's which get associated with each ScriptCode.
        /// </summary>
        private Scope Generate() {
            List<CodeGen> cgs = GenerateScriptMethods();
            List<Scope> scopes = GenerateScriptScopes();

            Debug.Assert(cgs.Count == scopes.Count);
            Debug.Assert(scopes.Count == _scriptCodes.Length);

            List<CallTargetWithContext0> targets = new List<CallTargetWithContext0>();
            foreach (CodeGen cg in cgs) {
                targets.Add((CallTargetWithContext0)cg.CreateDelegate(typeof(CallTargetWithContext0)));
            }

            // TODO: clean this up after clarifying dynamic site initialization logic
            for (int i = 0; i < _scriptCodes.Length; i++) {
                CodeContext cc = new CodeContext(scopes[i], _scriptCodes[i].LanguageContext);
                Microsoft.Scripting.Actions.DynamicSiteHelpers.InitializeFields(cc, cgs[i].MethodInfo.DeclaringType);
            }

            // everything succeeded, commit the results
            for (int i = 0; i < _scriptCodes.Length; i++) {
                ScriptCode sc = _scriptCodes[i];

                sc.OptimizedTarget = targets[i];
                sc.OptimizedScope = scopes[i];
            }

            return scopes[0];
        }

        private List<Scope> GenerateScriptScopes() {
            List<Scope> scopes = new List<Scope>(_scriptCodes.Length);
            foreach (ScriptCode sc in _scriptCodes) {
                // Force creation of names used in other script codes into all optimized dictionaries
                ScopeAllocator allocator = _allocators[sc.LanguageContext];
                IAttributesCollection iac = CreateLanguageDictionary(sc.LanguageContext, allocator);
                Scope scope = new Scope(iac);

                IModuleDictionaryInitialization ici = iac as IModuleDictionaryInitialization;
                if (ici != null) {
                    ici.InitializeModuleDictionary(new CodeContext(scope, sc.LanguageContext));
                }

                scopes.Add(scope);
            }
            return scopes;
        }

        private List<CodeGen> GenerateScriptMethods() {
            List<CodeGen> cgs = new List<CodeGen>(_scriptCodes.Length);
            foreach (ScriptCode sc in _scriptCodes) {
                ScopeAllocator sa = CreateStorageAllocator(sc);
                CodeGen cg = CreateCodeGen(sc);
                cg.Allocator = sa;

                // every module can hand it's environment to anything embedded in it.
                cg.EnvironmentSlot = new EnvironmentSlot(new PropertySlot(cg.ContextSlot, typeof(CodeContext).GetProperty("Locals")));
                cg.Context = sc.CompilerContext;
                sc.CodeBlock.EmitFunctionImplementation(cg);

                cg.Finish();

                cgs.Add(cg);
            }
            return cgs;
        }

        private ScopeAllocator CreateStorageAllocator(ScriptCode scriptCode) {
            ScopeAllocator allocator;
            if (!_allocators.TryGetValue(scriptCode.LanguageContext, out allocator)) {
                SlotFactory sf = CreateSlotFactory(scriptCode);
                ModuleGlobalFactory mgf = new ModuleGlobalFactory(sf);
                GlobalFieldAllocator gfa = new GlobalFieldAllocator(mgf);

                // Locals and globals are allocated from the same namespace for optimized modules
                ScopeAllocator global = new ScopeAllocator(null, gfa);
                allocator = new ScopeAllocator(global, gfa);

                _allocators[scriptCode.LanguageContext] = allocator;
            }

            return allocator;
        }

        #region Protected Members

        protected abstract CodeGen CreateCodeGen(ScriptCode scriptCode);
        protected abstract IAttributesCollection CreateLanguageDictionary(LanguageContext context, ScopeAllocator allocator);
        protected abstract SlotFactory CreateSlotFactory(ScriptCode scriptCode);

        #endregion
    }

    class TupleModuleGenerator : OptimizedModuleGenerator {
        private Dictionary<LanguageContext, TupleSlotFactory> _languages = new Dictionary<LanguageContext, TupleSlotFactory>();

        public TupleModuleGenerator(params ScriptCode[] scriptCodes)
            : base(scriptCodes) {
        }

        private class LanguageInfo {
            public StaticFieldSlotFactory SlotFactory;
            public TypeGen TypeGen;

            public LanguageInfo(StaticFieldSlotFactory slotFactory, TypeGen tg) {
                TypeGen = tg;
                SlotFactory = slotFactory;
            }
        }

        #region Abstract overrides

        protected override SlotFactory CreateSlotFactory(ScriptCode scriptCode) {
            return _languages[scriptCode.LanguageContext] = new TupleSlotFactory(typeof(ModuleGlobalDictionary<>));
        }

        protected override IAttributesCollection CreateLanguageDictionary(LanguageContext context, ScopeAllocator allocator) {
            TupleSlotFactory tsf = _languages[context];
            object tuple = tsf.CreateTupleInstance();

            // TODO: Force all dictionaries to share same object data (for multi-module)

            IAttributesCollection res = (IAttributesCollection)Activator.CreateInstance(
                tsf.DictionaryType.MakeGenericType(tsf.TupleType),
                tuple,
                tsf.Names);

            return res;
        }

        protected override CodeGen CreateCodeGen(ScriptCode scriptCode) {
            return CompilerHelpers.CreateDynamicCodeGenerator(scriptCode.CompilerContext);
        }

        #endregion
    }

    class StaticFieldModuleGenerator : OptimizedModuleGenerator {
        private static int _Counter;
        private Dictionary<LanguageContext, LanguageInfo> _languages = new Dictionary<LanguageContext, LanguageInfo>();

        private class LanguageInfo {
            public StaticFieldSlotFactory SlotFactory;
            public TypeGen TypeGen;

            public LanguageInfo(StaticFieldSlotFactory slotFactory, TypeGen tg) {
                TypeGen = tg;
                SlotFactory = slotFactory;
            }
        }

        public StaticFieldModuleGenerator(params ScriptCode[] scriptCodes)
            : base(scriptCodes) {
        }

        #region Abstract overrides

        protected override SlotFactory CreateSlotFactory(ScriptCode scriptCode) {
            AssemblyGen ag = CreateModuleAssembly(scriptCode);

            TypeGen tg = GenerateModuleGlobalsType(scriptCode.CompilerContext.SourceUnit.Name, ag);
            StaticFieldSlotFactory factory = new StaticFieldSlotFactory(tg);

            _languages[scriptCode.LanguageContext] = new LanguageInfo(factory, tg);

            return factory;
        }

        protected override IAttributesCollection CreateLanguageDictionary(LanguageContext context, ScopeAllocator allocator) {
            LanguageInfo li = _languages[context];

            // TODO: Force all dictionaries to share same object data (for multi-module)

            GlobalFieldAllocator gfa = allocator.LocalAllocator as GlobalFieldAllocator;
            if (gfa != null) {
                Dictionary<SymbolId, Slot> fields = gfa.SlotFactory.Fields;

                BuildDictionary(li, fields);

                Type t = li.TypeGen.FinishType();
                li.TypeGen.AssemblyGen.DumpAndLoad();

                return (IAttributesCollection)Activator.CreateInstance(t);
            } else {
                throw new InvalidOperationException("invalid allocator");
            }
        }

        protected override CodeGen CreateCodeGen(ScriptCode scriptCode) {
            LanguageInfo li = _languages[scriptCode.LanguageContext];

            return li.TypeGen.DefineMethod(CompilerHelpers.PublicStatic,
                "Initialize",
                typeof(object),
                new Type[] { typeof(CodeContext) },
                null);
        }

        #endregion

        /// <summary>
        /// Creates a new assembly for generating a module, ensuring a unique filename like "filename.N.exe" for the generated assembly
        /// </summary>
        private AssemblyGen CreateModuleAssembly(ScriptCode scriptCode) {
            //scriptCode.CompilerContext.Options
            AssemblyGenAttributes genAttrs = ScriptDomainManager.Options.AssemblyGenAttributes;

            if (scriptCode.SourceUnit.IsVisibleToDebugger)
                genAttrs |= AssemblyGenAttributes.EmitDebugInfo;
            
            if (ScriptDomainManager.Options.DebugCodeGeneration)
                genAttrs |= AssemblyGenAttributes.DisableOptimizations;

            string outDir, fileName;
            GetCompiledSourceUnitAssemblyLocation(scriptCode.CompilerContext.SourceUnit, out outDir, out fileName);

            AssemblyGen ag;
            string ext = ".exe";
            // Try to create a file called "filename.<cnt>.exe", ensuring that the filename does not clash with an existing file
            int cnt = 0;
            for (; ; ) {
                try {
                    ag = new AssemblyGen(scriptCode.CompilerContext.SourceUnit.Name, outDir, fileName + ext, genAttrs);
                    break;
                } catch (IOException) {
                    // If a file already exits with the same name, try the next name in the sequence.
                    ext = "_" + cnt.ToString() + ext;
                }
            }

            ag.SetSourceUnit(scriptCode.CompilerContext.SourceUnit);

            return ag;
        }

        private static void GetCompiledSourceUnitAssemblyLocation(SourceUnit sourceUnit, out string outDir, out string fileName) {
            SourceFileUnit file_unit = sourceUnit as SourceFileUnit;

            if (file_unit != null) {
                string fullPath = ScriptDomainManager.CurrentManager.Host.NormalizePath(file_unit.Path);
                outDir = ScriptDomainManager.Options.BinariesDirectory == null ? Path.GetDirectoryName(fullPath) : ScriptDomainManager.Options.BinariesDirectory;
                fileName = Path.GetFileNameWithoutExtension(fullPath);
            } else {
                outDir = null;
                fileName = Utils.ToValidFileName(sourceUnit.Name);
            }
        }

        private static TypeGen GenerateModuleGlobalsType(string moduleName, AssemblyGen ag) {
            TypeGen tg = ag.DefinePublicType(moduleName + "$mod_" + Interlocked.Increment(ref _Counter).ToString(), typeof(CustomSymbolDictionary));
            tg.AddCodeContextField();
            tg.DefaultConstructor = tg.TypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            return tg;
        }

        private void BuildDictionary(LanguageInfo li, Dictionary<SymbolId, Slot> fields) {
            MakeGetMethod(li, fields);
            MakeSetMethod(li, fields);
            CodeGen keysMethod = MakeRawKeysMethod(li, fields);

            MakeInitialization(keysMethod, li, fields);
        }

        private static void MakeInitialization(CodeGen keysMethod, LanguageInfo li, Dictionary<SymbolId, Slot> fields) {
            li.TypeGen.TypeBuilder.AddInterfaceImplementation(typeof(IModuleDictionaryInitialization));
            CodeGen cg = li.TypeGen.DefineExplicitInterfaceImplementation(typeof(IModuleDictionaryInitialization).GetMethod("InitializeModuleDictionary"));

            Label ok = cg.DefineLabel();
            cg.ContextSlot.EmitGet(cg);
            cg.EmitNull();
            cg.Emit(OpCodes.Ceq);
            cg.Emit(OpCodes.Brtrue_S, ok);
            cg.EmitNew(typeof(InvalidOperationException), new Type[0]);
            cg.Emit(OpCodes.Throw);
            cg.MarkLabel(ok);

            // MyModuleDictType.ContextSlot = arg0
            cg.EmitArgGet(0);
            cg.ContextSlot.EmitSet(cg);

            foreach (KeyValuePair<SymbolId, Slot> kv in fields) {
                Slot slot = kv.Value;
                ModuleGlobalSlot builtin = slot as ModuleGlobalSlot;

                Debug.Assert(builtin != null);

                cg.EmitCodeContext();
                cg.EmitSymbolId(kv.Key);
                builtin.EmitWrapperAddr(cg);
                cg.EmitCall(typeof(RuntimeHelpers), "InitializeModuleField");
            }

            cg.EmitReturn();
            cg.Finish();
        }

        //
        // This generates a method like the following:
        //
        //  TryGetExtraValue(int name, object out value) {
        //      if (name1 == name) {
        //          value = type.name1Slot.RawValue;
        //          return value != Uninitialized.Instance;
        //      }
        //      if (name2 == name) {
        //          value = type.name2Slot.RawValue;
        //          return value != Uninitialized.Instance;
        //      }
        //      ...
        //      return false
        //  }

        private void MakeGetMethod(LanguageInfo li, Dictionary<SymbolId, Slot> fields) {
            CodeGen cg = li.TypeGen.DefineMethodOverride(typeof(CustomSymbolDictionary).GetMethod("TryGetExtraValue", BindingFlags.NonPublic | BindingFlags.Instance));
            foreach (KeyValuePair<SymbolId, Slot> kv in fields) {
                SymbolId name = kv.Key;
                Slot slot = kv.Value;

                cg.EmitSymbolId(name);
                cg.EmitArgGet(0);
                cg.EmitCall(typeof(SymbolId), "op_Equality");

                Label next = cg.DefineLabel();
                cg.Emit(OpCodes.Brfalse_S, next);

                cg.EmitArgGet(1);

                ModuleGlobalSlot builtin = slot as ModuleGlobalSlot;
                Debug.Assert(builtin != null);
                builtin.EmitGetRaw(cg);
                cg.Emit(OpCodes.Stind_Ref);

                builtin.EmitGetRaw(cg);
                cg.EmitUninitialized();
                cg.Emit(OpCodes.Ceq);
                cg.Emit(OpCodes.Not);
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

        private void MakeSetMethod(LanguageInfo li, Dictionary<SymbolId, Slot> fields) {
            CodeGen cg = li.TypeGen.DefineMethodOverride(typeof(CustomSymbolDictionary).GetMethod("TrySetExtraValue", BindingFlags.NonPublic | BindingFlags.Instance));
            Slot valueSlot = cg.GetArgumentSlot(1);
            foreach (KeyValuePair<SymbolId, Slot> kv in fields) {
                SymbolId name = kv.Key;
                Slot slot = kv.Value;

                cg.EmitSymbolId(name);
                cg.EmitArgGet(0);
                cg.EmitCall(typeof(SymbolId), "op_Equality");

                Label next = cg.DefineLabel();
                cg.Emit(OpCodes.Brfalse_S, next);

                slot.EmitSet(cg, valueSlot);
                cg.EmitInt(1);
                cg.EmitReturn();
                cg.MarkLabel(next);
            }
            cg.EmitInt(0);
            cg.EmitReturn();
            cg.Finish();
        }

        private CodeGen MakeRawKeysMethod(LanguageInfo li, Dictionary<SymbolId, Slot> fields) {
            Slot rawKeysCache = li.TypeGen.AddStaticField(typeof(SymbolId[]), "ExtraKeysCache");
            CodeGen init = li.TypeGen.TypeInitializer;

            init.EmitInt(fields.Count);
            init.Emit(OpCodes.Newarr, typeof(SymbolId));

            int current = 0;
            foreach (KeyValuePair<SymbolId, Slot> kv in fields) {
                Debug.Assert(current < fields.Count);
                init.Emit(OpCodes.Dup);
                init.EmitInt(current++);
                init.Emit(OpCodes.Ldelema, typeof(SymbolId));
                init.EmitSymbolIdId(kv.Key);
                init.Emit(OpCodes.Call, typeof(SymbolId).GetConstructor(new Type[] { typeof(int) }));
            }

            rawKeysCache.EmitSet(init);

            CodeGen cg = li.TypeGen.DefineMethodOverride(typeof(CustomSymbolDictionary).GetMethod("GetExtraKeys", BindingFlags.Public | BindingFlags.Instance));
            rawKeysCache.EmitGet(cg);
            cg.EmitReturn();
            cg.Finish();

            return cg;
        }
    }

}
