/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using Microsoft.Scripting.Shell;

namespace IronPython.Hosting {
    [CLSCompliant(true)]
    public /* TODO: sealed */ class PythonConsoleOptions : ConsoleOptions {

        private bool _ignoreEnvironmentVariables = false;
        private bool _importSite = true;

        public bool IgnoreEnvironmentVariables {
            get { return _ignoreEnvironmentVariables; }
            set { _ignoreEnvironmentVariables = value; }
        }

        public bool ImportSite {
            get { return _importSite; }
            set { _importSite = value; }
        }
    }
}

