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

using System.Diagnostics;
using System.Scripting.Actions;
using System.Scripting.Runtime;
using System.Threading;

namespace IronPython.Runtime.Calls {
    public static class DefaultContext {
        [MultiRuntimeAware]
        public static CodeContext _default;
        [MultiRuntimeAware]
        public static CodeContext _defaultCLS;
        [MultiRuntimeAware]
        public static ActionBinder _defaultBinder;
        
        public static ContextId Id {
            get {
                return Default.LanguageContext.ContextId;
            }
        }
        
        public static CodeContext Default {
            get {
                Debug.Assert(_default != null);
                return _default;
            }
        }

        public static ActionBinder DefaultPythonBinder {
            get {
                return _defaultBinder;
            }
        }

        public static PythonContext DefaultPythonContext {
            get {
                Debug.Assert(_default != null);
                return (PythonContext)_default.LanguageContext;
            }
        }

        public static CodeContext DefaultCLS {
            get {
                Debug.Assert(_defaultCLS != null);
                return _defaultCLS;
            }
        }

        internal static void CreateContexts(ScriptDomainManager manager, PythonContext/*!*/ context) {
            Interlocked.CompareExchange<CodeContext>(ref _default, CreateDefaultContext(context), null);
            Interlocked.CompareExchange<CodeContext>(ref _defaultCLS, CreateDefaultCLSContext(context), null);
            Interlocked.CompareExchange<ActionBinder>(ref _defaultBinder, new PythonBinder(manager, context, _default), null);
        }

        private static CodeContext/*!*/ CreateDefaultContext(PythonContext/*!*/ context) {
            PythonModule globalMod = context.CreateModule(ModuleOptions.NoBuiltins);
            return new CodeContext(globalMod.Scope, context);
        }


        private static CodeContext/*!*/ CreateDefaultCLSContext(PythonContext/*!*/ context) {
            PythonModule globalMod = context.CreateModule(ModuleOptions.ShowClsMethods | ModuleOptions.NoBuiltins);
            return new CodeContext(globalMod.Scope, context);
        }
    }
}
