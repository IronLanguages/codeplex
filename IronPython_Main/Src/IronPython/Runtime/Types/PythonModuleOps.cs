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

using IronPython.Hosting;
using IronPython.Compiler;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

[assembly: PythonExtensionTypeAttribute(typeof(ScriptScope), typeof(PythonModuleOps))]
namespace IronPython.Runtime.Types {
    /// <summary>
    /// Represents functionality that is exposed on PythonModule's but not exposed on the common ScriptModule
    /// class.
    /// </summary>
    public static class PythonModuleOps {
        private static object PythonCreated = new object();

        #region Public Python API Surface

        [StaticExtensionMethod("__new__")]
        public static ScriptScope MakeModule(CodeContext context, PythonType cls, params object[] args\u00F8) {
            if (cls.IsSubclassOf(TypeCache.Module)) {
                ScriptScope module = PythonEngine.CurrentEngine.MakePythonModule("?");
                
                // TODO: should be null
                module.Scope.Clear();

                SetPythonCreated(module);

                return module;
            }
            throw PythonOps.TypeError("{0} is not a subtype of module", cls.Name);
        }

        [StaticExtensionMethod("__new__")]
        public static ScriptScope MakeModule(CodeContext context, PythonType cls, [ParamDictionary] PythonDictionary kwDict\u00F8, params object[] args\u00F8) {
            return MakeModule(context, cls, args\u00F8);
        }

        [PythonName("__init__")]
        public static void Initialize(ScriptScope module, string name) {
            Initialize(module, name, null);
        }

        [PythonName("__init__")]
        public static void Initialize(ScriptScope module, string name, string documentation) {
            module.ModuleName = name;

            if (documentation != null) {
                module.Scope.SetName(Symbols.Doc, documentation);
            }
        }

        [PythonName("__repr__")]
        [SpecialName]
        public static string ToCodeString(ScriptScope module) {
            return ToString(module);
        }

        [PythonName("__str__")]
        [SpecialName]
        public static string ToString(ScriptScope module) {
            if (Get__file__(module) == null) {
                return String.Format("<module '{0}' (built-in)>", module.ModuleName);
            }
            return String.Format("<module '{0}' from '{1}'>", module.ModuleName, Get__file__(module));
        }

        [PropertyMethod, PythonName("__name__")]
        public static object Get__name__(ScriptScope module) {
            object res;
            if (module.Scope.TryLookupName(DefaultContext.Default.LanguageContext, Symbols.Name, out res)) {
                return res;
            }

            return module.ModuleName;
        }

        [PropertyMethod, PythonName("__name__")]
        public static void Set__name__(ScriptScope module, object value) {
            module.Scope.SetName(Symbols.Name, value);

            string strVal = value as string;
            if (strVal != null) {
                module.ModuleName = strVal;
            }
        }

        [PropertyMethod, PythonName("__dict__")]
        public static IAttributesCollection Get__dict__(ScriptScope module) {
            return new GlobalsDictionary(module.Scope);
        }

        [PropertyMethod, PythonName("__dict__")]
        public static IAttributesCollection Set__dict__(ScriptScope module, object value) {
            throw PythonOps.TypeError("readonly attribute");
        }

        [PropertyMethod, PythonName("__dict__")]
        public static IAttributesCollection Delete__dict__(ScriptScope module) {
            throw PythonOps.TypeError("can't set attributes of built-in/extension type 'module'");
        }
        
        [PropertyMethod, PythonName("__file__")]
        public static string Get__file__(ScriptScope module) {
            object res;
            if (!module.Scope.TryLookupName(DefaultContext.Default.LanguageContext, Symbols.File, out res)) {
                return module.FileName;
            }
            return res as string;
        }

        [PropertyMethod, PythonName("__file__")]
        public static void Set__file__(ScriptScope module, string value) {
            module.Scope.SetName(Symbols.File, module.FileName = value);
        }

        public static void SetPythonCreated(ScriptScope module) {
            PythonModuleContext moduleContext = (PythonModuleContext)DefaultContext.Default.LanguageContext.EnsureModuleContext(module);
            moduleContext.IsPythonCreatedModule = true;
        }

        #endregion

        // TODO: Should be internal on PythonOps:

        public static ScriptCode CompileFlowTrueDivision(SourceUnit codeUnit, LanguageContext context) {
            // flow TrueDivision and bind to the current module, use default error sink:
            return context.CompileSourceCode(codeUnit);
        }

        /// <summary>
        /// Creates a new PythonModule putting the members defined on type into it. 
        /// 
        /// Used for __builtins__ and the built-in modules (e.g. nt, re, etc...)
        /// </summary>
        internal static ScriptScope MakePythonModule(string name, Type type) {
            Contract.RequiresNotNull(type, "type");

            // TODO: hack to enable __builtin__ reloading:
            //return MakePythonModule(name, new Scope(MakeModuleDictionary(type)), ModuleOptions.None);

            // import __builtin__
            // del __builtin__.pow
            // reload(__builtin__)
            // __builtin__.pow

            // creates an empty module:
            
            ScriptScope module = ScriptDomainManager.CurrentManager.CompileModule(name, 
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

            // we could take the easy way out and build a PythonType for the module
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

                dict[SymbolTable.StringToId(strName)] = DynamicHelpers.GetPythonTypeFromType(t);
            }

            return dict;
        }
    }
}
