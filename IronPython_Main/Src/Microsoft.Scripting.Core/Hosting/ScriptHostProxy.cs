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

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Provides hosting to DLR. Forwards DLR requests to the ScriptHost. 
    /// </summary>
    internal sealed class ScriptHostProxy : DynamicRuntimeHostingProvider {
        private readonly ScriptHost/*!*/ _host;

        public ScriptHostProxy(ScriptHost/*!*/ host) {
            Assert.NotNull(host);
            _host = host;
        }

        public override PlatformAdaptationLayer/*!*/ PlatformAdaptationLayer {
            get { return _host.PlatformAdaptationLayer; }
        }

        public override SourceUnit TryGetSourceFileUnit(LanguageContext/*!*/ language, string/*!*/ path, Encoding/*!*/ encoding, SourceCodeKind kind) {
            ScriptSource result = _host.TryGetSourceFile(_host.Runtime.GetEngine(language), path, encoding, kind);
            return (result != null) ? result.SourceUnit : null;
        }

        public override SourceUnit/*!*/ ResolveSourceFileUnit(string/*!*/ name) {
            ScriptSource result = _host.ResolveSourceFile(name);
            return (result != null) ? result.SourceUnit : null;
        }
    }
}
