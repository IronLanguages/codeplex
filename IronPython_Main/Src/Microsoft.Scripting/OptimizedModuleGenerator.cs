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
using System.Diagnostics;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.IO;
using System.Threading;
using System.Security.Permissions;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Creates the code for an optimized module.  
    /// </summary>
    public abstract class OptimizedModuleGenerator {
        private readonly ScriptCode _scriptCode;
        private ScopeAllocator _allocator;
        private CodeContext _codeContext;

        protected OptimizedModuleGenerator(ScriptCode scriptCode) {
            Assert.NotNull(scriptCode);

            _scriptCode = scriptCode;
        }

        public static OptimizedModuleGenerator Create(ScriptCode scriptCode) {
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
                    return new StaticFieldModuleGenerator(scriptCode);
#if !SILVERLIGHT
                } catch (SecurityException) {
                }
#endif
            }

            return new TupleModuleGenerator(scriptCode);
        }

        /// <summary>
        /// Creates the methods and optimized Scope's which get associated with each ScriptCode.
        /// </summary>
        public Scope GenerateScope() {
            Compiler compiler = GenerateScriptMethod();
            Scope scope = GenerateScriptScope();

            CallTargetWithContext0 target;
            target = (CallTargetWithContext0)compiler.CreateDelegate(typeof(CallTargetWithContext0));

            // TODO: clean this up after clarifying dynamic site initialization logic
            Microsoft.Scripting.Actions.DynamicSiteHelpers.InitializeFields(_codeContext, compiler.Method.DeclaringType);

            // everything succeeded, commit the results
            _scriptCode.OptimizedTarget = target;
            _scriptCode.OptimizedScope = scope;

            return scope;
        }

        private Scope GenerateScriptScope() {
            // Force creation of names used in other script codes into all optimized dictionaries
            IAttributesCollection iac = CreateLanguageDictionary(_allocator);
            Scope scope = new Scope(_scriptCode.LanguageContext, iac);

            // module context is filled later:
            _codeContext = new CodeContext(scope, _scriptCode.LanguageContext);

            IModuleDictionaryInitialization ici = iac as IModuleDictionaryInitialization;
            if (ici != null) {
                ici.InitializeModuleDictionary(_codeContext);
            }

            return scope;
        }

        private Compiler GenerateScriptMethod() {
            _allocator = CreateStorageAllocator(_scriptCode);
            Compiler compiler = CreateCodeGen(_scriptCode);
            compiler.Allocator = _allocator;

            // every module can hand it's environment to anything embedded in it.
            compiler.EnvironmentSlot = new EnvironmentSlot(new PropertySlot(
                new PropertySlot(compiler.ContextSlot, 
                    typeof(CodeContext).GetProperty("Scope")),
                typeof(Scope).GetProperty("Dict"))
            );

            compiler.SetSource(_scriptCode.SourceUnit);
            compiler.GenerateCodeBlock(_scriptCode.CodeBlock);

            compiler.Finish();

            return compiler;
        }

        private ScopeAllocator CreateStorageAllocator(ScriptCode scriptCode) {
            SlotFactory sf = CreateSlotFactory(scriptCode);
            ModuleGlobalFactory mgf = new ModuleGlobalFactory(sf);
            GlobalFieldAllocator gfa = new GlobalFieldAllocator(mgf);

            // Locals and globals are allocated from the same namespace for optimized modules
            ScopeAllocator global = new ScopeAllocator(null, gfa);
            return  new ScopeAllocator(global, gfa);
        }

        #region Protected Members

        internal abstract Compiler/*!*/ CreateCodeGen(ScriptCode/*!*/ scriptCode);
        internal abstract IAttributesCollection/*!*/ CreateLanguageDictionary(ScopeAllocator/*!*/ allocator);
        internal abstract SlotFactory/*!*/ CreateSlotFactory(ScriptCode/*!*/ scriptCode);

        #endregion
    }

    class TupleModuleGenerator : OptimizedModuleGenerator {
        private TupleSlotFactory _slotFactory;

        public TupleModuleGenerator(ScriptCode scriptCode)
            : base(scriptCode) {
        }

        #region Abstract overrides

        internal override SlotFactory CreateSlotFactory(ScriptCode scriptCode) {
            return _slotFactory = new TupleSlotFactory(typeof(ModuleGlobalDictionary<>));
        }

        internal override IAttributesCollection/*!*/ CreateLanguageDictionary(ScopeAllocator/*!*/ allocator) {
            object tuple = _slotFactory.CreateTupleInstance();

            IAttributesCollection res = (IAttributesCollection)Activator.CreateInstance(
                _slotFactory.DictionaryType.MakeGenericType(_slotFactory.TupleType),
                tuple,
                _slotFactory.Names);

            return res;
        }

        internal override Compiler/*!*/ CreateCodeGen(ScriptCode/*!*/ scriptCode) {
            return CompilerHelpers.CreateDynamicCodeGenerator(scriptCode.SourceUnit);
        }

        #endregion
    }

    class StaticFieldModuleGenerator : OptimizedModuleGenerator {
        private static int _Counter;
        private TypeGen _typeGen;

        public StaticFieldModuleGenerator(ScriptCode scriptCode)
            : base(scriptCode) {
        }

        #region Abstract overrides

        internal override SlotFactory CreateSlotFactory(ScriptCode scriptCode) {
            _typeGen = GenerateModuleGlobalsType(scriptCode.SourceUnit);
            return new StaticFieldSlotFactory(_typeGen);
        }

        internal override IAttributesCollection CreateLanguageDictionary(ScopeAllocator/*!*/ allocator) {
            // TODO: Force all dictionaries to share same object data (for multi-module)

            GlobalFieldAllocator gfa = allocator.LocalAllocator as GlobalFieldAllocator;
            if (gfa != null) {
                Dictionary<SymbolId, Slot> fields = gfa.Fields;

                BuildDictionary(fields);

                Type t = _typeGen.FinishType();

                return (IAttributesCollection)Activator.CreateInstance(t);
            } else {
                throw new InvalidOperationException("invalid allocator");
            }
        }

        internal override Compiler/*!*/ CreateCodeGen(ScriptCode/*!*/ scriptCode) {
            Compiler compiler = _typeGen.DefineMethod("Initialize", typeof(object), new Type[] { typeof(CodeContext) }, null, null);
            compiler.SetSource(scriptCode.SourceUnit);
            return compiler;
        }

        #endregion


        private TypeGen GenerateModuleGlobalsType(SourceUnit sourceUnit) {
            TypeGen tg = ScriptDomainManager.CurrentManager.Snippets.DefineDebuggableType(
                "OptimizedScope$" + Interlocked.Increment(ref _Counter).ToString(), typeof(CustomSymbolDictionary), sourceUnit);
            tg.AddCodeContextField();
            return tg;
        }

        private void BuildDictionary(Dictionary<SymbolId, Slot> fields) {
            MakeGetMethod(fields);
            MakeSetMethod(fields);
            MakeRawKeysMethod(fields);
            MakeInitialization(fields);
        }

        private void MakeInitialization(Dictionary<SymbolId, Slot> fields) {
            _typeGen.TypeBuilder.AddInterfaceImplementation(typeof(IModuleDictionaryInitialization));
            Compiler cg = _typeGen.DefineExplicitInterfaceImplementation(typeof(IModuleDictionaryInitialization).GetMethod("InitializeModuleDictionary"));

            Label ok = cg.DefineLabel();
            cg.ContextSlot.EmitGet(cg);
            cg.Emit(OpCodes.Ldnull);
            cg.Emit(OpCodes.Ceq);
            cg.Emit(OpCodes.Brtrue_S, ok);
            cg.EmitNew(typeof(InvalidOperationException), ArrayUtils.EmptyTypes);
            cg.Emit(OpCodes.Throw);
            cg.MarkLabel(ok);

            // MyModuleDictType.ContextSlot = arg0
            cg.EmitArgGet(0);
            cg.ContextSlot.EmitSet(cg);

            foreach (KeyValuePair<SymbolId, Slot> kv in fields) {
                Slot slot = kv.Value;
                ModuleGlobalSlot builtin = slot as ModuleGlobalSlot;

                Debug.Assert(builtin != null);

                cg.EmitArgGet(0);
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

        private void MakeGetMethod(Dictionary<SymbolId, Slot> fields) {
            Compiler cg = _typeGen.DefineMethodOverride(typeof(CustomSymbolDictionary).GetMethod("TryGetExtraValue", BindingFlags.NonPublic | BindingFlags.Instance));
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

        private void MakeSetMethod(Dictionary<SymbolId, Slot> fields) {
            Compiler cg = _typeGen.DefineMethodOverride(typeof(CustomSymbolDictionary).GetMethod("TrySetExtraValue", BindingFlags.NonPublic | BindingFlags.Instance));
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

        private Compiler MakeRawKeysMethod(Dictionary<SymbolId, Slot> fields) {
            Slot rawKeysCache = _typeGen.AddStaticField(typeof(SymbolId[]), "ExtraKeysCache");
            Compiler init = _typeGen.TypeInitializer;

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

            Compiler cg = _typeGen.DefineMethodOverride(typeof(CustomSymbolDictionary).GetMethod("GetExtraKeys", BindingFlags.Public | BindingFlags.Instance));
            rawKeysCache.EmitGet(cg);
            cg.EmitReturn();
            cg.Finish();

            return cg;
        }
    }

}
