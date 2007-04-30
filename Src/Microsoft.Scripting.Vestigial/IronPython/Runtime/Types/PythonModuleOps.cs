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
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Internal.Ast;

using IronPython.Compiler;
using System.Reflection;
using Microsoft.Scripting.Hosting;
using IronPython.Runtime.Calls;
using IronPython.Hosting;
using System.Diagnostics;
using IronPython.Runtime.Types;

[assembly: PythonExtensionTypeAttribute(typeof(ScriptModule), typeof(PythonModuleOps))]
namespace IronPython.Runtime.Types {
    /// <summary>
    /// Represents functionality that is exposed on PythonModule's but not exposed on the common ScriptModule
    /// class.
    /// </summary>
    public static class PythonModuleOps {
        private static object PythonCreated = new object();

        #region Public Python API Surface

        [StaticOpsMethod("__new__")]
        public static ScriptModule MakeModule(CodeContext context, DynamicType cls, params object[] args\u00F8) {
            if (cls.IsSubclassOf(TypeCache.Module)) {
                ScriptModule module = SystemState.Instance.Engine.MakePythonModule("?");
                
                // TODO: should be null
                module.Scope.Clear();

                SetPythonCreated(module);

                return module;
            }
            throw Ops.TypeError("{0} is not a subtype of module", cls.Name);
        }

        [StaticOpsMethod("__new__")]
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
        [OperatorMethod]
        public static string ToCodeString(ScriptModule module) {
            return ToString(module);
        }

        [PythonName("__str__")]
        [OperatorMethod]
        public static string ToString(ScriptModule module) {
            if (GetFileName(module) == null) {
                if (module.InnerModule != null) {
                    ReflectedPackage rp = module.InnerModule as ReflectedPackage;
                    if (rp != null) {
                        if (rp._packageAssemblies.Count != 1) {
                            return String.Format("<module '{0}' (CLS module, {1} assemblies loaded)>", module.ModuleName, rp._packageAssemblies.Count);
                        } 
                        return String.Format("<module '{0}' (CLS module from {1})>", module.ModuleName, rp._packageAssemblies[0].FullName);
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
            throw Ops.TypeError("readonly attribute");
        }

        [PropertyMethod, PythonName("__dict__")]
        public static IAttributesCollection DeleteDictionary(ScriptModule module) {
            throw Ops.TypeError("can't set attributes of built-in/extension type 'module'");
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
            object dummy;
            //!!! TODO
            if (sourceUnit == null || module.TryGetLanguageData(PythonCreated, out dummy)) {
                // We created the module and it only contains Python code. If the user changes
                // __file__ we'll reload from that file.  
                return GetFileName(module);
            }

            // multi-language scenario and we can re-load the file.
            return sourceUnit.Path;
        }

        public static void CheckReloadable(ScriptModule module) {
            object dummy;
            // only check for Python requirements of reloading on modules created from Python.code.
            if (module.TryGetLanguageData(PythonCreated, out dummy)) {
                if (!module.Scope.ContainsName(DefaultContext.Default.LanguageContext, Symbols.Name))
                    throw Ops.SystemError("nameless module");

                if (!SystemState.Instance.modules.ContainsKey(module.Scope.LookupName(DefaultContext.Default.LanguageContext, Symbols.Name))) {
                    throw Ops.ImportError("module {0} not in sys.modules", module.Scope.LookupName(DefaultContext.Default.LanguageContext, Symbols.Name));
                }
            }
        }

        public static void SetPythonCreated(ScriptModule mod) {
            mod.SetLanguageData(PythonCreated, Ops.True);
        }

        #endregion

        // TODO: Should be internal on PythonOps:

        public static ScriptCode CompileFlowTrueDivision(SourceUnit codeUnit, LanguageContext context) {
            // flow TrueDivision and bind to the current module, use default error sink:
            ScriptCode result = ScriptCode.FromCompiledCode(codeUnit.Compile(context.GetCompilerOptions()));
            ((PythonContext)result.LanguageContext).ModuleContext = ((PythonContext)context).ModuleContext;
            return result;
        }
    }
}
