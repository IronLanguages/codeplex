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
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting {
    /// <summary>
    /// Singleton LanguageContext which represents a language-neutral LanguageContext
    /// </summary>
    public class InvariantContext : LanguageContext {
        public static InvariantContext Instance;
        public static CodeContext CodeContext;

        static InvariantContext() {
            Instance = new InvariantContext();
            ModuleContext moduleContext = new ModuleContext(null);
            moduleContext.ShowCls = true;
            CodeContext = new CodeContext(new Scope(new SymbolDictionary()), Instance, moduleContext);
        }
        
        private InvariantContext() {
        }

        public override ScriptEngine Engine {
            get {
                throw new InvalidOperationException("Engine property is not available on InvariantContext");
            }
        }
    }
}
