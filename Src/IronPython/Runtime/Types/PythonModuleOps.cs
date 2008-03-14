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
using Microsoft.Scripting.Runtime;

[assembly: PythonExtensionTypeAttribute(typeof(Scope), typeof(PythonModuleOps))]
namespace IronPython.Runtime.Types {
    /// <summary>
    /// Represents functionality that is exposed on PythonModule's but not exposed on the common ScriptModule
    /// class.
    /// </summary>
    public static class PythonModuleOps {

        #region Public Python API Surface

        [StaticExtensionMethod]
        public static Scope/*!*/ __new__(CodeContext/*!*/ context, PythonType/*!*/ cls, params object[]/*!*/ args\u00F8) {
            if (cls.IsSubclassOf(TypeCache.Module)) {
                PythonModule module = PythonContext.GetContext(context).CreateModule("?");
                
                // TODO: should be null
                module.Scope.Clear();

                return module.Scope;
            }
            throw PythonOps.TypeError("{0} is not a subtype of module", cls.Name);
        }

        [StaticExtensionMethod]
        public static Scope/*!*/ __new__(CodeContext/*!*/ context, PythonType/*!*/ cls, [ParamDictionary]PythonDictionary kwDict\u00F8, params object[]/*!*/ args\u00F8) {
            return __new__(context, cls, args\u00F8);
        }

        public static void __init__(Scope/*!*/ scope, string name) {
            __init__(scope, name, null);
        }

        public static void __init__(Scope/*!*/ scope, string name, string documentation) {
            DefaultContext.DefaultPythonContext.EnsurePythonModule(scope).SetName(name);

            if (documentation != null) {
                scope.SetName(Symbols.Doc, documentation);
            }
        }

        public static string/*!*/ __repr__(Scope/*!*/ scope) {
            return __str__(scope);
        }

        public static string/*!*/ __str__(Scope/*!*/ scope) {
            PythonModule module = DefaultContext.DefaultPythonContext.EnsurePythonModule(scope);
            string file = module.GetFile() as string;
            string name = module.GetName() as string ?? "?";

            if (file == null) {
                return String.Format("<module '{0}' (built-in)>", name);
            }
            return String.Format("<module '{0}' from '{1}'>", name, file);
        }

        [PropertyMethod]
        public static IAttributesCollection/*!*/ Get__dict__(Scope/*!*/ scope) {
            return new PythonDictionary(new GlobalScopeDictionaryStorage(scope));
        }

        [PropertyMethod]
        public static IAttributesCollection Set__dict__(Scope/*!*/ scope, object value) {
            throw PythonOps.TypeError("readonly attribute");
        }

        [PropertyMethod]
        public static IAttributesCollection Delete__dict__(Scope/*!*/ scope) {
            throw PythonOps.TypeError("can't set attributes of built-in/extension type 'module'");
        }
        
        #endregion

        internal static void PopulateModuleDictionary(PythonContext/*!*/ context, IAttributesCollection/*!*/ dict, Type/*!*/ type) {
            Assert.NotNull(dict, type);

            // we could take the easy way out and build a PythonType for the module
            // and then copy but that causes a bunch of overhead we don't need.  
            // Instead we simply extract the items we need out for the modules and
            // put them into the dictionary.  Note for fields & propreties we publish
            // the value, not the property - modules aren't descriptor aware.
            foreach (FieldInfo fi in type.GetFields()) {
                if (!fi.IsStatic) continue;

                dict[SymbolTable.StringToId(fi.Name)] = fi.GetValue(null);
            }

            // if we're replacing an existing built-in function we need to first
            // place a new one instead of just adding the pre-existing overloads back 
            // in.
            Dictionary<SymbolId, object> cleared = new Dictionary<SymbolId,object>();
            MethodInfo reloadMethod = null;
            foreach (MethodInfo mi in type.GetMethods()) {
                if (!mi.IsStatic) continue;
                if (mi.Name == Importer.ModuleReloadMethod) {
                    // there can be only one reload method
                    Debug.Assert(reloadMethod == null); 
                    reloadMethod = mi;
                    continue; // hidden Python method for reloading modules.
                }

                string strName = mi.Name;
                NameType nt = NameConverter.GetNameFromMethod(TypeCache.Object, mi, NameType.Method, ref strName);
                if (nt == NameType.None) continue;

                SymbolId name = SymbolTable.StringToId(strName);

                object val, dummy;
                if (dict.TryGetValue(name, out val) && cleared.TryGetValue(name, out dummy)) {                    
                    BuiltinFunction bf = val as BuiltinFunction;
                    Debug.Assert(bf != null);
                    bf.AddMethod(mi);
                } else {
                    cleared[name] = null;
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

            if (reloadMethod != null) {
                reloadMethod.Invoke(null, new object[] { context, dict });
            }
        }
    }
}
