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
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell; 

namespace ToyScript {
    class ToyConsole : ConsoleHost {
        protected override ScriptEngine/*!*/ CreateEngine() {
            Runtime.LoadAssembly(typeof(string).Assembly);
            return Runtime.GetEngine(typeof(ToyLanguageContext));
        }

        protected override CommandLine CreateCommandLine() {
            return new ToyCommandLine();
        }

        [STAThread]
        static int Main(string[] args) {
            return new ToyConsole().Run(args);
        }
    }
}
