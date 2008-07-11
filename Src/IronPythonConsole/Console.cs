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
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;

internal sealed class PythonConsoleHost : ConsoleHost {

    protected override ScriptEngine/*!*/ CreateEngine() {
        return Runtime.GetEngine(typeof(PythonContext));
    }

    protected override CommandLine/*!*/ CreateCommandLine() {
        return new PythonCommandLine();
    }

    protected override OptionsParser/*!*/ CreateOptionsParser() {
        return new PythonOptionsParser(this.Options);
    }

    protected override string/*!*/ GetHelp() {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(PythonCommandLine.GetLogoDisplay());
        PrintLanguageHelp(sb);
        sb.AppendLine();

        return sb.ToString();
    }

    protected override void ParseHostOptions(string/*!*/[]/*!*/ args) {
        // Python doesn't want any of the DLR base options.
        foreach (string s in args) {
            Options.IgnoredArgs.Add(s);
        }
    }

    [STAThread]
    static int Main(string[] args) {
        return new PythonConsoleHost().Run(args);
    }
}