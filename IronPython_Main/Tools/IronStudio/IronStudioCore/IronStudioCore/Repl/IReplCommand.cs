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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.IronStudio.Repl {
    /// <summary>
    /// Represents a command which can be run from a REPL window.
    /// 
    /// This interface is a MEF contract and can be implemented and exported to add commands to the REPL window.
    /// </summary>
    public interface IReplCommand {
        /// <summary>
        /// Runs the specified command with the given arguments.  The command may interact
        /// with the window using the provided interface.
        /// </summary>
        void Execute(IReplWindow window, string arguments);

        /// <summary>
        /// Gets a description of the REPL command which is displayed when the user asks for help.
        /// </summary>
        string Description {
            get;
        }

        /// <summary>
        /// Gets the text for the actual command.
        /// </summary>
        string Command {
            get;
        }

        /// <summary>
        /// Content to be placed in a toolbar button or null if should not be placed on a toolbar.
        /// </summary>
        object ButtonContent {
            get;
        }
    }
}
