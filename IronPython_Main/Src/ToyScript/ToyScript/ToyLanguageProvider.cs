/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Helpers;
using Microsoft.Scripting.Shell;

namespace ToyScript {
    public sealed class ToyLanguageProvider : LanguageProvider {
        public ToyLanguageProvider(ScriptDomainManager manager) : base(manager) { }

        public override string LanguageDisplayName {
            get { return "ToyScript"; }
        }

        public override OptionsParser GetOptionsParser() {
            return new DefaultOptionsParser();
        }

        public override CommandLine GetCommandLine() {
            return new ToyCommandLine();
        }

        public override ScriptEngine GetEngine(EngineOptions options) {
            if (options == null) {
                options = new ToyEngineOptions();
            }
            return new ToyEngine(this, options);
        }
    }
}
