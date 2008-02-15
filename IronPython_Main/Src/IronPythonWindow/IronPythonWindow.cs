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
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using System.Windows.Forms;

internal sealed class PythonWindowsConsoleHost : ConsoleHost {

    protected override void Initialize() {
        base.Initialize();
        this.Options.ScriptEngine = Environment.GetEngineByFileExtension("py");
        // TODO: this.Options.NoConsole = true;
    }
    
    [STAThread]
    static int Main(string[] args) {
        if (args.Length == 0) {
            new PythonWindowsConsoleHost().PrintHelp();
            return 1;
        }

        return new PythonWindowsConsoleHost().Run(args);
    }

    protected override void PrintHelp() {
        MessageBox.Show(GetHelp(), "IronPython Window Console Help");
    }
}