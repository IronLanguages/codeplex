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


        public static CodeContext DefaultTrueDivision {
            get {
                Debug.Assert(_defaultTrueDivision != null);
                return _defaultTrueDivision;
            }
        }

        internal static void CreateContexts(PythonEngine engine) {
            _default = CreateDefaultContext(engine);
            _defaultCLS = CreateDefaultCLSContext(engine);
            _defaultTrueDivision = CreateTrueDivisionContext(engine);
        }

        private static CodeContext CreateDefaultContext(PythonEngine engine) {
            ScriptScope globalMod = ScriptDomainManager.CurrentManager.CreateModule("__builtin__", new Scope(new SymbolDictionary()));
            return new CodeContext(globalMod.Scope, new PythonContext(engine, false), new PythonModuleContext(globalMod));
        }

        private static CodeContext CreateTrueDivisionContext(PythonEngine engine) {
            ScriptScope globalMod = ScriptDomainManager.CurrentManager.CreateModule("__builtin__", new Scope(new SymbolDictionary()));
            return new CodeContext(globalMod.Scope, new PythonContext(engine, true), new PythonModuleContext(globalMod));
        }

        private static CodeContext CreateDefaultCLSContext(PythonEngine engine) {
            ScriptScope globalMod = ScriptDomainManager.CurrentManager.CreateModule("__builtin__", new Scope(new SymbolDictionary()));

            PythonModuleContext moduleContext = new PythonModuleContext(globalMod);
            moduleContext.ShowCls = true;

            return new CodeContext(globalMod.Scope, new PythonContext(engine, false), moduleContext);
        }
    }
}
