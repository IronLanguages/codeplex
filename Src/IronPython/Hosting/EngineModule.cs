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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Globalization;

using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using IronPython.Compiler;

namespace IronPython.Hosting {
    /// <summary>
    /// This represents a module created by the host. The host can create multiple EngineModules.
    /// Code can then be executed in the EngineModule.
    /// </summary>
    public class EngineModule {
        IDictionary<string, object> globals;
        IAttributesDictionary globalsAdapter;
        ModuleScope defaultModuleScope;

        #region Public API
        public IDictionary<string, object> Globals { get { return globals; } }

        public PythonModule Module { get { return defaultModuleScope.Module; } }

        public string Name { get { return Module.ModuleName; } }

        public override string ToString() {
            string name = Name;
            if (name == String.Empty) name = "<Empty Name>";
            return base.ToString() + ":" + name;
        }
        #endregion

        internal IAttributesDictionary GlobalsAdapter { get { return globalsAdapter; } }

        internal ICallerContext CallerContext { get { return defaultModuleScope; } }

        internal ModuleScope GetModuleScope(IDictionary<string, object> locals) {
            if (locals == null) {
                // We can use defaultModuleScope in this case as we do not need to customize locals
                Debug.Assert(defaultModuleScope.Globals == defaultModuleScope.Locals);
                return defaultModuleScope;
            }
            ModuleScope scope = new ModuleScope(defaultModuleScope.Module, globalsAdapter, new StringDictionaryAdapterDict(locals));
            return scope;
        }

        internal EngineModule(ModuleScope scope) {
            defaultModuleScope = scope;
            globalsAdapter = scope.Globals;
            globals = new AttributesDictionaryAdapter(globalsAdapter);
        }

        internal EngineModule(string moduleName, IDictionary<string, object> globalsDict, SystemState systemState) {
            Debug.Assert(moduleName != null);
            globals = globalsDict;
            if (globals is IAttributesDictionary)
                globalsAdapter = globals as IAttributesDictionary;
            else
                globalsAdapter = new StringDictionaryAdapterDict(globalsDict);
            PythonModule pythonModule = new PythonModule(moduleName, globalsAdapter, systemState);
            defaultModuleScope = new ModuleScope(pythonModule);
        }
    }

    /// <summary>
    /// This represents a module whose global code is optimized. The restriction is that the user
    /// cannot specify a globals dictionary of her liking.
    /// Further code can be executed in the context of an OptimizedEngineModule. However, this
    /// will not be optimized.
    /// </summary>
    public class OptimizedEngineModule : EngineModule {
        bool globalCodeExecuted;

        internal OptimizedEngineModule(ModuleScope moduleScope)
            : base(moduleScope) {
            Debug.Assert(GlobalsAdapter is CompiledModule);
        }

        /// <summary>
        /// This executes the top-level global code of the module
        /// </summary>
        public void Execute() {
            if (globalCodeExecuted)
                throw new InvalidOperationException("Cannot execute global code multiple times");
            globalCodeExecuted = true;

            Module.Initialize();
        }
    }
}
