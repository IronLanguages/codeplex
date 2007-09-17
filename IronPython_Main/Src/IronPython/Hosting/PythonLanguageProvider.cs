/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Hosting;

using IronPython.Compiler;

namespace IronPython.Hosting {
    public sealed class PythonLanguageProvider : LanguageProvider {

        public PythonLanguageProvider(ScriptDomainManager environment)
            : base(environment) {
        }

        public override string LanguageDisplayName {
            get { return "Python"; }
        }
        
        public override ScriptEngine GetEngine(EngineOptions options) {
            if (options != null && !(options is PythonEngineOptions)) throw new ArgumentException("options");
            return PythonEngine.Factory.GetInstance(this, (PythonEngineOptions)options);
        }

        public override CommandLine GetCommandLine() {
            return new PythonCommandLine();
        }

        public override OptionsParser GetOptionsParser() {
            return new PythonOptionsParser();
        }

        public override TokenCategorizer GetTokenCategorizer() {
            return new PythonTokenCategorizer();
        }
    }
}

