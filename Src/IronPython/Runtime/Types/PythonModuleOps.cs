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
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Types;

using IronPython.Hosting;
using IronPython.Compiler;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

[assembly: PythonExtensionTypeAttribute(typeof(ScriptModule), typeof(PythonModuleOps))]
namespace IronPython.Runtime.Types {
    /// <summary>
    /// Represents functionality that is exposed on PythonModule's but not exposed on the common ScriptModule
    /// class.
    /// </summary>
    public static class PythonModuleOps {
        private static object PythonCreated = new object();

        #region Public Python API Surface

        [StaticExtensionMethod("__new__")]
        public static ScriptModule MakeModule(CodeContext context, DynamicType cls, params object[] args\u00F8) {
            if (cls.IsSubclassOf(TypeCache.Module)) {
                ScriptModule module = PythonEngine.CurrentEngine.MakePythonModule("?");
                
                // TODO: should be null
                module.Scope.Clear();

                SetPythonCreated(module);

                return module;
            }
            throw PythonOps.TypeError("{0} is not a subtype of module", cls.Name);
        }

        [StaticExtensionMethod("__new__")]
        public static ScriptModule MakeModule(CodeContext context, DynamicType cls, [ParamDictionary] PythonDictionary kwDict\u00F8, params object[] args\u00F8) {
            return MakeModule(context, cls, args\u00F8);
        }

        [PythonName("__init__")]
        public static void Initialize(ScriptModule module, string name) {
            Initialize(module, name, null);
        }

        [PythonName("__init__")]
        public static void Initialize(ScriptModule module, string name, string documentation) {
            module.ModuleName = name;

            if (documentation != null) {
                module.Scope.SetName(Symbols.Doc, documentation);
            }
        }

        [PythonName("__repr__")]
        [SpecialName]
        public static string ToCodeString(ScriptModule module) {
            return ToString(module);
        }

        [PythonName("__str__")]
        [SpecialName]
        public static string ToString(ScriptModule module) {
            if (GetFileName(module) == null) {
                if (module.InnerModule != null) {
                    ReflectedPackage rp = module.InnerModule as ReflectedPackage;
                    if (rp != null) {
                        if (rp.PackageAssemblies.Count != 1) {
                            return String.Format("<module '{0}' (CLS module, {1} assemblies loaded)>", module.ModuleName, rp.PackageAssemblies.Count);
                        } 
                        return String.Format("<module '{0}' (CLS module from {1})>", module.ModuleName, rp.PackageAssemblies[0].FullName);
                    } 
                    return String.Format("<module '{0}' (CLS module)>", module.ModuleName);                    
                } 
                return String.Format("<module '{0}' (built-in)>", module.ModuleName);
            }
            return String.Format("<module '{0}' from '{1}'>", module.ModuleName, GetFileName(module));
        }

        [PropertyMethod, PythonName("__name__")]
        public static object GetName(ScriptModule module) {
            object res;
            if (module.Scope.TryLookupName(DefaultContext.Default.LanguageContext, Symbols.Name, out res)) {
                return res;
            }

            return module.ModuleName;
        }

        [PropertyMethod, PythonName("__name__")]
        public static void SetName(ScriptModule module, object value) {
            module.Scope.SetName(Symbols.Name, value);

            string strVal = value as string;
            if (strVal != null) {
                module.ModuleName = strVal;
            }
        }

        [PropertyMethod, PythonName("__dict__")]
        public static IAttributesCollection GetDictionary(ScriptModule module) {
            if (module.PackageImported) {
                // TODO: Remove bad cast
                return (IAttributesCollection)module.InnerModule.GetCustomMemberDictionary(DefaultContext.Default);                
            }

            return new GlobalsDictionary(module.Scope);
        }

        [PropertyMethod, PythonName("__dict__")]
        public static IAttributesCollection SetDictionary(ScriptModule module, object value) {
            throw PythonOps.TypeError("readonly attribute");
        }

        [PropertyMethod, PythonName("__dict__")]
        public static IAttributesCollection DeleteDictionary(ScriptModule module) {
            throw PythonOps.TypeError("can't set attributes of built-in/extension type 'module'");
        }
        
        [PropertyMethod, PythonName("__file__")]
        public static string GetFileName(ScriptModule module) {
            object res;
            if (!module.Scope.TryLookupName(DefaultContext.Default.LanguageContext, Symbols.File, out res)) {
                return module.FileName;
            }
            return res as string;
        }

        [PropertyMethod, PythonName("__file__")]
        public static void SetFileName(ScriptModule module, string value) {
            module.Scope.SetName(Symbols.File, module.FileName = value);
        }

        internal static string GetReloadFilename(ScriptModule module, SourceFileUnit sourceUnit) {
            PythonModuleContext moduleContext = (PythonModuleContext)DefaultContext.Default.LanguageContext.GetModuleContext(module);

            //!!! TODO
            if (sourceUnit == null || moduleContext != null && moduleContext.IsPythonCreatedModule) {
                // We created the module and it only contains Python code. If the user changes
                // __file__ we'll reload from that file.  
                return GetFileName(module);
            }

            // multi-language scenario and we can re-load the file.
            return sourceUnit.Path;
        }

        public static void CheckReloadable(ScriptModule module) {
            PythonModuleContext moduleContext = (PythonModuleContext)DefaultContext.Default.LanguageContext.GetModuleContext(module);

            // only check for Python requirements of reloading on modules created from Python.code.
            if (moduleContext != null && moduleContext.IsPythonCreatedModule) {
                if (!module.Scope.ContainsName(DefaultContext.Default.LanguageContext, Symbols.Name))
                    throw PythonOps.SystemError("nameless module");

                if (!SystemState.Instance.modules.ContainsKey(module.Scope.LookupName(DefaultContext.Default.LanguageContext, Symbols.Name))) {
                    throw PythonOps.ImportError("module {0} not in sys.modules", module.Scope.LookupName(DefaultContext.Default.LanguageContext, Symbols.Name));
                }
            }
        }

        public static void SetPythonCreated(ScriptModule module) {
            PythonModuleContext moduleContext = (PythonModuleContext)DefaultContext.Default.LanguageContext.EnsureModuleContext(module);
            moduleContext.IsPythonCreatedModule = true;
        }

        #endregion

        // TODO: Should be internal on PythonOps:

        public static ScriptCode CompileFlowTrueDivision(SourceUnit codeUnit, LanguageContext context) {
            // flow TrueDivision and bind to the current module, use default error sink:
            ScriptCode result = ScriptCode.FromCompiledCode(codeUnit.Compile(context.GetCompilerOptions()));
            // obsolete: result.LanguageContext.ModuleContext = context.ModuleContext;
            return result;
        }

        /// <summary>
        /// Creates a new PythonModule putting the members defined on type into it. 
        /// 
        /// Used for __builtins__ and the built-in modules (e.g. nt, re, etc...)
        /// </summary>
        internal static ScriptModule MakePythonModule(string name, Type type) {
            if (type == null) throw new ArgumentNullException("type");

            // TODO: hack to enable __builtin__ reloading:
            //return MakePythonModule(name, new Scope(MakeModuleDictionary(type)), ModuleOptions.None);

            // import __builtin__
            // del __builtin__.pow
            // reload(__builtin__)
            // __builtin__.pow

            // creates an empty module:
            
            ScriptModule module = ScriptDomainManager.CurrentManager.CompileModule(name, 
                ScriptModuleKind.Default,
                new Scope(MakeModuleDictionary(type)), 
                null, 
                null);

            PythonModuleContext moduleContext = (PythonModuleContext)DefaultContext.Default.LanguageContext.EnsureModuleContext(module);
            moduleContext.IsPythonCreatedModule = true;

            return module;
        }

        private static IAttributesCollection MakeModuleDictionary(Type type) {
            IAttributesCollection dict = new SymbolDictionary();

            // we could take the easy way out and build a DynamicType for the module
            // and then copy but that causes a bunch of overhead we don't need.  
            // Instead we simply extract the items we need out for the modules and
            // put them into the dictionary.  Note for fields & propreties we publish
            // the value, not the property - modules aren't descriptor aware.
            foreach (FieldInfo fi in type.GetFields()) {
                if (!fi.IsStatic) continue;

                dict[SymbolTable.StringToId(fi.Name)] = fi.GetValue(null);
            }

            foreach (MethodInfo mi in type.GetMethods()) {
                if (!mi.IsStatic) continue;

                string strName = mi.Name;
                NameType nt = NameConverter.GetNameFromMethod(TypeCache.Object, mi, NameType.Method, ref strName);
                if (nt == NameType.None) continue;

                SymbolId name = SymbolTable.StringToId(strName);

                object val;
                if (dict.TryGetValue(name, out val)) {
                    BuiltinFunction bf = val as BuiltinFunction;
                    Debug.Assert(bf != null);
                    bf.AddMethod(mi);
                } else {
                    dict[name] = BuiltinFunction.MakeMethod(strName,
                        mi,
                        nt == NameType.PythonMethod ? FunctionType.Function | FunctionType.AlwaysVisible : FunctionType.Function);
                }
            }

            foreach (PropertyInfo pi in type.GetProperties()) {
                string strName = pi.Name;
                NameType nt = NameConverter.GetNameFromMethod(TypeCache.Object,
                    pi.GetGetMethod() ?? pi.GetSetMethod(),
                    NameType.Method,
                    ref strName);

                if (nt == NameType.None) continue;

                dict[SymbolTable.StringToId(strName)] = pi.GetValue(null, ArrayUtils.EmptyObjects);
            }

            foreach (Type t in type.GetNestedTypes(BindingFlags.Public)) {
                string strName;
                NameType nt = NameConverter.TryGetName(t, out strName);

                if (nt == NameType.None) continue;

                dict[SymbolTable.StringToId(strName)] = DynamicHelpers.GetDynamicTypeFromType(t);
            }

            return dict;
        }
    }
}
