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
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Silverlight {

    /// <summary>
    /// ScriptHost for use inside the browser
    /// Overrides certain operations to redirect to XAP or throw NotImplemented
    /// </summary>
    public sealed class BrowserScriptHost : ScriptHost {

        public BrowserScriptHost() {
        }

        protected override IList<string>/*!*/ SourceFileSearchPath {
            get {
                return new string[] { String.Empty };
            }
        }

        public override ScriptSource TryGetSourceFile(ScriptEngine/*!*/ engine, string/*!*/ path, Encoding/*!*/ encoding, SourceCodeKind kind) {
            ContractUtils.RequiresNotNull(path, "path");

            if (!DynamicApplication.InUIThread) {
                return null; // Application.GetResourceStream will throw if called from a non-UI thread
            }
            string code = DynamicApplication.DownloadContents(path);
            if (code != null) {
                ScriptSource src = engine.CreateScriptSourceFromString(code, path, kind);
                SourceCache.Add(src);
                return src;
            }
            return null;
        }

        public override PlatformAdaptationLayer/*!*/ PlatformAdaptationLayer {
            get {
                return BrowserPAL.PAL;
            }
        }

        protected override void EngineCreated(ScriptEngine/*!*/ engine) {
            engine.SetScriptSourceSearchPaths(new string[] { String.Empty });
        }
    }
}
