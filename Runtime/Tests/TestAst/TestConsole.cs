/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using Microsoft.Scripting.Generation; 
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;

namespace TestAst {
    class TestConsole : ConsoleHost {
        protected override Type Provider {
            get { return typeof(TestContext); }
        }

        protected override CommandLine/*!*/ CreateCommandLine() {
            return new TestCommandLine();
        }

        protected override OptionsParser/*!*/ CreateOptionsParser() {
            return new TestOptionsParser();
        }

        protected override ScriptRuntimeSetup/*!*/ CreateRuntimeSetup() {
            var setup = base.CreateRuntimeSetup();
            setup.DebugMode = true;
            Snippets.SetSaveAssemblies(true, Environment.CurrentDirectory);
            return setup;            
        }

        [STAThread]
        static int Main(string[] args) {
            var start = DateTime.Now;

            var ret = new TestConsole().Run(args);

            Console.WriteLine((DateTime.Now - start).ToString());

            return ret;
        }
    }
}
