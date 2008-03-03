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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using System.Threading;

namespace IronPython.Runtime.Calls {
    public static class DefaultContext {
        [MultiRuntimeAware]
        public static CodeContext _default;
        [MultiRuntimeAware]
        public static CodeContext _defaultCLS;
        
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

        internal static void CreateContexts(PythonContext/*!*/ context) {
            Interlocked.CompareExchange(ref _default, CreateDefaultContext(context), null);
            Interlocked.CompareExchange(ref _defaultCLS, CreateDefaultCLSContext(context), null);
        }

        private static CodeContext/*!*/ CreateDefaultContext(PythonContext/*!*/ context) {
            PythonModule globalMod = context.CreateModule("__builtin__", ModuleOptions.NoBuiltins);
            return new CodeContext(globalMod.Scope, context, globalMod);
        }


        private static CodeContext/*!*/ CreateDefaultCLSContext(PythonContext/*!*/ context) {
            PythonModule globalMod = context.CreateModule("__builtin__", ModuleOptions.ShowClsMethods | ModuleOptions.NoBuiltins);
            return new CodeContext(globalMod.Scope, context, globalMod);
        }
    }
}
