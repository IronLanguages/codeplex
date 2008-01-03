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
using Microsoft.Scripting.Hosting;
using System.IO;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {
    
    /// <summary>
    /// 
    /// NOTE: Local hosting only.
    /// </summary>
    public static class Script {
        /// <exception cref="ArgumentNullException"><paramref name="languageId"/>, <paramref name="code"/></exception>
        /// <exception cref="ArgumentException">no language registered</exception>
        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language provider's implementation failed to instantiate.</exception>
        public static void Execute(string languageId, string code) {
            Contract.RequiresNotNull(languageId, "languageId");
            Contract.RequiresNotNull(code, "code");

            ScriptEngine eng = ScriptDomainManager.CurrentManager.GetEngine(languageId);
            eng.Execute(ScriptDomainManager.CurrentManager.CreateScope(null), eng.CreateScriptSourceFromString(code, SourceCodeKind.File));
        }
        /// <exception cref="ArgumentNullException"><paramref name="languageId"/>, <paramref name="code"/></exception>
        /// <exception cref="ArgumentException">no language registered</exception>
        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language provider's implementation failed to instantiate.</exception>
        public static object Evaluate(string languageId, string expression) {
            Contract.RequiresNotNull(languageId, "languageId");
            Contract.RequiresNotNull(expression, "expression");

            ScriptEngine eng = ScriptDomainManager.CurrentManager.GetEngine(languageId);
            return eng.Execute(ScriptDomainManager.CurrentManager.Host.DefaultScope, eng.CreateScriptSourceFromString(expression, SourceCodeKind.Expression));
        }

        // TODO: file IO exceptions
        /// <exception cref="ArgumentNullException"><paramref name="path"/></exception>
        /// <exception cref="ArgumentException">no language registered</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is not a valid path.</exception>
        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language provider's implementation failed to instantiate.</exception>
        public static void ExecuteFile(string path) {
            Contract.RequiresNotNull(path, "path");

            ScriptEngine eng = ScriptDomainManager.CurrentManager.GetEngineByFileExtension(Path.GetExtension(path));
            ScriptDomainManager.CurrentManager.ExecuteSourceUnit(eng.CreateScriptSourceFromFile(path));
        }

        // TODO: file IO exceptions
        /// <exception cref="ArgumentNullException"><paramref name="path"/></exception>
        /// <exception cref="ArgumentException">no language registered</exception>
        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language provider's implementation failed to instantiate.</exception>
        public static void ExecuteFileContent(string path) {
            Contract.RequiresNotNull(path, "path");

            ScriptEngine eng = ScriptDomainManager.CurrentManager.GetEngineByFileExtension(Path.GetExtension(path));
            eng.Compile(eng.CreateScriptSourceFromFile(path)).Execute(ScriptDomainManager.CurrentManager.Host.DefaultScope);
        }

        public static void SetVariable(string name, object value) {
            ScriptDomainManager.CurrentManager.Host.DefaultScope.SetVariable(name, value);  
        }

        public static object GetVariable(string name) {
            return ScriptDomainManager.CurrentManager.Host.DefaultScope.LookupVariable(name);  
        }

        public static bool VariableExists(string name) {
            return ScriptDomainManager.CurrentManager.Host.DefaultScope.VariableExists(name);
        }

        public static bool RemoveVariable(string name) {
            return ScriptDomainManager.CurrentManager.Host.DefaultScope.RemoveVariable(name);
        }

        public static void ClearVariables() {
            ScriptDomainManager.CurrentManager.Host.DefaultScope.ClearVariables();
        }
        
        /// <exception cref="ArgumentNullException"><paramref name="languageId"/></exception>
        /// <exception cref="ArgumentException">no language registered</exception>
        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language provider's implementation failed to instantiate.</exception>
        public static IScriptEngine GetEngine(string languageId) {
            Contract.RequiresNotNull(languageId, "languageId");

            return ScriptDomainManager.CurrentManager.GetEngine(languageId);
        }
    }
}
