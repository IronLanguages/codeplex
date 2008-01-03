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

namespace IronPython.Runtime.Calls {
    public static class DefaultContext {

        public static CodeContext _default;
        public static CodeContext _defaultCLS;
        public static CodeContext _defaultTrueDivision;
        
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

        public static CodeContext DefaultCLS {
            get {
                Debug.Assert(_defaultCLS != null);
                return _defaultCLS;
            }
        }

        internal static void CreateContexts(LanguageContext context) {
            _default = CreateDefaultContext(context);
            _defaultCLS = CreateDefaultCLSContext(context);
        }

        private static CodeContext CreateDefaultContext(LanguageContext context) {
            ScriptScope globalMod = ScriptDomainManager.CurrentManager.CreateModule("__builtin__", new Scope(new SymbolDictionary()));
            return new CodeContext(globalMod.Scope, context, new PythonModuleContext(globalMod));
        }

        private static CodeContext CreateDefaultCLSContext(LanguageContext context) {
            ScriptScope globalMod = ScriptDomainManager.CurrentManager.CreateModule("__builtin__", new Scope(new SymbolDictionary()));

            PythonModuleContext moduleContext = new PythonModuleContext(globalMod);
            moduleContext.ShowCls = true;

            return new CodeContext(globalMod.Scope, context, moduleContext);
        }
    }
}
