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
using System.Collections.Generic;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Utils;

namespace TestAst {
    class TestEngineOptions : LanguageOptions {
        private readonly string[]/*!*/ _arguments;
        private readonly string[] _skipTests;
        private readonly string[] _runTests;
        private readonly int[] _PriorityOfTests;

        public string[]/*!*/ Arguments {
            get { return _arguments; }
        }

        public string[] SkipTests {
            get { return _skipTests; }
        }

        public String[] RunTests {
            get { return _runTests; }
        }

        public int[] PriorityOfTests {
            get { return _PriorityOfTests; }
        }

        public TestEngineOptions(IDictionary<string, object> options) 
            : base(options) {

            _arguments = ArrayUtils.Copy(GetOption(options, "Arguments", ArrayUtils.EmptyStrings));
            _skipTests = GetOption(options, "SkipTests", new string[] { });
            _runTests = GetOption(options, "RunTests", new string[]{});
            string[] TempPri = GetOption(options, "PriorityOfTests", new string[] { "1" });

            _PriorityOfTests = (int[]) System.Array.CreateInstance(typeof(int), TempPri.Length);
            for(int i = 0; i < TempPri.Length; i++) {
                _PriorityOfTests[i] = Convert.ToInt32(TempPri[i]);
            }            
        }
    }

    class TestOptionsParser : OptionsParser<ConsoleOptions> {
        protected override void ParseArgument(string arg) {
            ContractUtils.RequiresNotNull(arg, "arg");

            //for commands on the form -tests:test1;test2;test3
            String subCommands = "";
            if(!arg.Contains("-X:") && arg.Contains(":")){
                subCommands = arg.Split(':')[1];
                arg = arg.Split(':')[0];
            }


            switch (arg) {
                case "-skip":
                    LanguageSetup.Options["SkipTests"] = subCommands.Split(';', ','); 
                    break;
                case "-tests":
                    LanguageSetup.Options["RunTests"] = subCommands.Split(';', ',');
                    break;
                case "-pri":
                    LanguageSetup.Options["PriorityOfTests"] = subCommands.Split(';',',');
                    break;
                default:
                    base.ParseArgument(arg);
                    if (ConsoleOptions.FileName != null) {
                        PushArgBack();
                        LanguageSetup.Options["Arguments"] = PopRemainingArgs();
                    }
                    break;
            }
        }

        public override void GetHelp(out string commandLine, out string[,] options, out string[,] environmentVariables, out string comments) {
            //@TODO - Ryan - alter this to match what you do with parseargument
            commandLine = "[options] [tests]";

            options = new string[,] {
                { "-skip", "Exclude the named tests" },
            };

            environmentVariables = new string[0, 0];

            comments = null;
        }
    }
}
