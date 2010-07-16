/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.ComponentModel.Composition;

using Microsoft.IronStudio.Repl;
using Microsoft.IronPythonTools.Library.Repl;

namespace Microsoft.IronPythonTools.Repl {
    [Export(typeof(IReplCommand))]
    class SwitchModuleCommand : IReplCommand {
        #region IReplCommand Members

        public void Execute(IReplWindow window, string arguments) {
            var remoteEval = window.Evaluator as RemotePythonEvaluator;
            if(remoteEval != null) {
                remoteEval.SetScope(arguments);
            }
        }

        public string Description {
            get { return "Switches the current scope to the specified module name."; }
        }

        public string Command {
            get { return "mod"; }
        }

        public object ButtonContent {
            get {
                return null;
            }
        }

        #endregion
    }
}
