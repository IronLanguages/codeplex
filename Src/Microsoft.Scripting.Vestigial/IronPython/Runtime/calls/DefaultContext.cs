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
using System.Diagnostics;
using IronPython.Compiler;
using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Actions;

namespace IronPython.Runtime.Calls {
    public static class DefaultContext {
        /// <summary> Standard Python context.  This is the ContextId we use to indicate calls from the Python context. </summary>
        // TODO: Move to PythonContext when DefaultContext is high enough in the stack        
        public static ContextId PythonContext = ContextId.RegisterContext(typeof(DefaultContext));

        public static CodeContext Default = CreateDefaultContext(null);
        public static CodeContext DefaultCLS = CreateDefaultCLSContext(null);
        public static CodeContext DefaultTrueDivision = CreateTrueDivisionContext(null);

        //public static CodeContext Default { get { if (_Default == null) _Default = CreateDefaultContext(SystemState.Instance.Engine); return _Default; } set { _Default = value; } }
        //public static CodeContext DefaultCLS { get { if (_DefaultCLS == null) _DefaultCLS = CreateDefaultCLSContext(SystemState.Instance.Engine); return _DefaultCLS; } set { _DefaultCLS = value; } }
        //public static CodeContext DefaultTrueDivision { get { if (_DefaultTrueDivision == null) _DefaultTrueDivision = CreateTrueDivisionContext(SystemState.Instance.Engine); return _DefaultTrueDivision; } set { _DefaultTrueDivision = value; } }

        //// TODO: 
        //public static void CreateDefaultContexts(ScriptEngine engine) {
        //    _Default = CreateDefaultContext(engine);
        //    _DefaultCLS = CreateDefaultCLSContext(engine);
        //    _DefaultTrueDivision = CreateTrueDivisionContext(engine);
        //}

        private static CodeContext CreateDefaultContext(ScriptEngine engine) {
            ScriptModule globalMod = ScriptDomainManager.CurrentManager.CreateModule("__builtin__", new Scope(new SymbolDictionary()));
            return new CodeContext(globalMod.Scope, new PythonContext(engine, new PythonModuleContext()));
        }

        private static CodeContext CreateTrueDivisionContext(ScriptEngine engine) {
            ScriptModule globalMod = ScriptDomainManager.CurrentManager.CreateModule("__builtin__", new Scope(new SymbolDictionary()));
            return new CodeContext(globalMod.Scope, new PythonContext(engine, new PythonModuleContext(), true));
        }

        private static CodeContext CreateDefaultCLSContext(ScriptEngine engine) {
            ScriptModule globalMod = ScriptDomainManager.CurrentManager.CreateModule("__builtin__", new Scope(new SymbolDictionary()));
            
            PythonModuleContext module_context = new PythonModuleContext();
            module_context.ShowCls = true;
            PythonContext ctx = new PythonContext(engine, module_context);

            return new CodeContext(globalMod.Scope, ctx);
        }
    }
}
