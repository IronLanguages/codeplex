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

        [StaticExtensionMethod("__new__")]
        public static Scope/*!*/ MakeModule(CodeContext/*!*/ context, PythonType/*!*/ cls, params object[]/*!*/ args\u00F8) {
            if (cls.IsSubclassOf(TypeCache.Module)) {
                PythonModule module = PythonContext.GetContext(context).CreateModule("?");
                
                // TODO: should be null
                module.Scope.Clear();

                return module.Scope;
            }
            throw PythonOps.TypeError("{0} is not a subtype of module", cls.Name);
        }

        [StaticExtensionMethod("__new__")]
        public static Scope/*!*/ MakeModule(CodeContext/*!*/ context, PythonType/*!*/ cls, [ParamDictionary]PythonDictionary kwDict\u00F8, params object[]/*!*/ args\u00F8) {
            return MakeModule(context, cls, args\u00F8);
        }

        [PythonName("__init__")]
        public static void Initialize(Scope/*!*/ scope, string name) {
            Initialize(scope, name, null);
        }

        [PythonName("__init__")]
        public static void Initialize(Scope/*!*/ scope, string name, string documentation) {
            DefaultContext.DefaultPythonContext.EnsurePythonModule(scope).SetName(name);

            if (documentation != null) {
                scope.SetName(Symbols.Doc, documentation);
            }
        }

        [PythonName("__repr__")]
        [SpecialName]
        public static string/*!*/ ToCodeString(Scope/*!*/ scope) {
            return ToString(scope);
        }

        [PythonName("__str__")]
        [SpecialName]
        public static string/*!*/ ToString(Scope/*!*/ scope) {
            PythonModule module = DefaultContext.DefaultPythonContext.EnsurePythonModule(scope);
            string file = module.GetFile() as string;
            string name = module.GetName() as string ?? "?";

            if (file == null) {
                return String.Format("<module '{0}' (built-in)>", name);
            }
            return String.Format("<module '{0}' from '{1}'>", name, file);
        }

        [PropertyMethod, PythonName("__name__")]
        public static object Get__name__(Scope/*!*/ scope) {
            return DefaultContext.DefaultPythonContext.EnsurePythonModule(scope).GetName();
        }

        [PropertyMethod, PythonName("__name__")]
        public static void Set__name__(Scope/*!*/ scope, object value) {
            DefaultContext.DefaultPythonContext.EnsurePythonModule(scope).SetName(value);
        }

        [PropertyMethod, PythonName("__dict__")]
        public static IAttributesCollection/*!*/ Get__dict__(Scope/*!*/ scope) {
            return new GlobalsDictionary(scope);
        }

        [PropertyMethod, PythonName("__dict__")]
        public static IAttributesCollection Set__dict__(Scope/*!*/ scope, object value) {
            throw PythonOps.TypeError("readonly attribute");
        }

        [PropertyMethod, PythonName("__dict__")]
        public static IAttributesCollection Delete__dict__(Scope/*!*/ scope) {
            throw PythonOps.TypeError("can't set attributes of built-in/extension type 'module'");
        }
        
        [PropertyMethod, PythonName("__file__")]
        public static object Get__file__(Scope/*!*/ scope) {
            object file = DefaultContext.DefaultPythonContext.EnsurePythonModule(scope).GetFile();
            if (file == null) throw PythonOps.AttributeError("module has no __file__ attribute");
            return file;
        }

        [PropertyMethod, PythonName("__file__")]
        public static void Set__file__(Scope/*!*/ scope, object value) {
            DefaultContext.DefaultPythonContext.EnsurePythonModule(scope).SetFile(value);
        }

        #endregion

        internal static void PopulateModuleDictionary(IAttributesCollection/*!*/ dict, Type/*!*/ type) {
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
            foreach (MethodInfo mi in type.GetMethods()) {
                if (!mi.IsStatic) continue;

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
        }
    }
}
