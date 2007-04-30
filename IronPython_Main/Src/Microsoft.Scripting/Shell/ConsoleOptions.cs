/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting.Shell {
    public class ConsoleOptions {
        private string _command;
        private string _filename;
        private bool _printVersionAndExit;
        private int _autoIndentSize = 4;
        private string[] _remainingArgs;
        private bool _introspection;

        // TODO: prop
        public bool AutoIndent = false;
        public bool HandleExceptions = true;
        public bool TabCompletion = false;
        public bool ColorfulConsole = false;
        public bool PrintUsageAndExit = false;
        
        /// <summary>
        /// Literal script command given using -c option
        /// </summary>
        public string Command {
            get { return _command; }
            set { _command = value; }
        }

        /// <summary>
        /// Filename to execute passed on the command line options.
        /// </summary>
        public string FileName {
            get { return _filename; }
            set { _filename = value; }
        }

        /// <summary>
        /// Only print the version of the script interpreter and exit
        /// </summary>
        public bool PrintVersionAndExit {
            get { return _printVersionAndExit; }
            set { _printVersionAndExit = value; }
        }

        public int AutoIndentSize {
            get { return _autoIndentSize; }
            set { _autoIndentSize = value; }
        }

        public string[] RemainingArgs {
            get { return _remainingArgs; }
            set { _remainingArgs = value; }
        }

        public bool Introspection {
            get { return _introspection; }
            set { _introspection = value; }
        }
    }
}
