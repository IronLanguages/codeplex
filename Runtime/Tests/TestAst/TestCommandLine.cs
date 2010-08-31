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
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting.Shell;

namespace TestAst {
    class TestCommandLine : CommandLine {
        public TestCommandLine() {
        }

        protected override int RunCommand(string command) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        /// <summary>
        /// Invoked when no arguments on the command line were parsed as file names
        /// by the default options parser.  In this case that means we want to run all
        /// of the available test scenarios and not enter some interactive console
        /// mode.
        /// </summary>
        /// <returns></returns>
        protected override int RunInteractive() {
            SourceUnit source = Language.CreateFileUnit(TestSpan.SourceFile);

            //Find and print disabled scenarios
            TestScenarios.PrintDisabled();

            ((TestContext)this.Language).RunTestTypes = TestContext.TestTypeFlag.Positive;
            source.ExecuteProgram();
            // Only try to save snippets if ExecuteProgram succeeds. Otherwise,
            // we'll get the wrong exception ("type was not completed")
            Snippets.SaveAndVerifyAssemblies();
            
            /*((TestContext)this.Language).RunTestTypes = TestContext.TestTypeFlag.Negative;
            source.ExecuteProgram();*/
            Snippets.SetSaveAssemblies(false, null);

            // These should also obey the testcase flags (skiptest, runtests, etc.)
            TestScenarios.RunNegative((TestEngineOptions)((TestContext)this.Language).Options);
            
            return 0;
        }
    }
}
