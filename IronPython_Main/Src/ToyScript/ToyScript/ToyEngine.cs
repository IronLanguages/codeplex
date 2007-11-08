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

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;

using ToyScript.Runtime;

namespace ToyScript {
    sealed class ToyEngine : ScriptEngine {
        private ToyBinder _binder;

        static ToyEngine() {
            RuntimeHelpers.RegisterAssembly(typeof(ToyEngine).Assembly);
        }

        public ToyEngine(LanguageProvider provider, EngineOptions engineOptions)
            : base(provider, engineOptions, new ToyLanguageContext()) {

            ((ToyLanguageContext)LanguageContext).ToyEngine = this;

            _binder = new ToyBinder(new CodeContext(new Scope(), LanguageContext, new ModuleContext(null)));
        }

        public override ActionBinder DefaultBinder {
            get { return _binder; }
        }

        protected override LanguageContext GetLanguageContext(ScriptModule module) {
            return LanguageContext;
        }

        protected override LanguageContext GetLanguageContext(CompilerOptions compilerOptions) {
            return LanguageContext;
        }
    }
}
