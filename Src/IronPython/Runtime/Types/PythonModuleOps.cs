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
    }
}
