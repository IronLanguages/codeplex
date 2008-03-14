/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.ComponentModel;

// TODO: Move this to a separate namespace to hide it from ordinary hosts?
namespace Microsoft.Scripting.Hosting {

    /// <summary>
    /// Advanced APIs for HAPI providers. These methods should not be used by hosts. 
    /// They are provided for other hosting API implementers that would like to leverage existing HAPI and 
    /// extend it with language specific functionality, for example. 
    /// 
    /// Providers or something like that and move this class there to hide it from the ordinary hosts. 
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HostingHelpers {
        public static ScriptDomainManager/*!*/ GetDomainManager(ScriptRuntime/*!*/ runtime) {
            Contract.RequiresNotNull(runtime, "runtime");
            return runtime.DomainManager;
        }

        public static LanguageContext/*!*/ GetLanguageContext(ScriptEngine/*!*/ engine) {
            Contract.RequiresNotNull(engine, "engine");
            return engine.LanguageContext;
        }
    }
}
