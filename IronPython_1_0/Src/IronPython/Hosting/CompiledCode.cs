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

using IronPython.Runtime;

namespace IronPython.Hosting {

    internal delegate object CompiledCodeDelegate(ModuleScope moduleScope);

    /// <summary>
    /// CompiledCode represents code that executes in the context of a ModuleScope. This is typically
    /// code executed using the Hosting APIs, from the interactive console, code compiled with the "eval" keyword, etc
    /// Also see CompiledModule which represents code of an entire module
    /// </summary>
    public class CompiledCode {
        private CompiledCodeDelegate code;
        private string name;
        private List<object> staticData;
        internal PythonEngine engine;

        internal CompiledCode(string name, CompiledCodeDelegate code, List<object> staticData) {
            this.name = name;
            this.code = code;
            this.staticData = staticData;
        }

        internal object Run(ModuleScope moduleScope) {
            moduleScope = (ModuleScope)moduleScope.Clone();
            moduleScope.staticData = staticData;
            return code(moduleScope);
        }

        public override string ToString() {
            return string.Format("<code {0}>", name);
        }

        public void Execute() {
            Execute(engine.DefaultModule, null);
        }

        public void Execute(EngineModule engineModule) {
            Execute(engineModule, null);
        }

        /// <summary>
        /// This can throw any exceptions raised by the code. If PythonSystemExitException is thrown, the host should 
        /// interpret that in a way that is appropriate for the host.
        /// </summary>
        public void Execute(EngineModule engineModule, IDictionary<string, object> locals) {
            engine.EnsureValidModule(engineModule);

            ModuleScope moduleScope = engineModule.GetModuleScope(locals);
            Run(moduleScope);
        }

    }
}