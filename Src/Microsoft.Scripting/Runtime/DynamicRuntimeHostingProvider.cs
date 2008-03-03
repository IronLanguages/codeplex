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

namespace Microsoft.Scripting.Runtime {
    [Serializable]
    public abstract class DynamicRuntimeHostingProvider {
        public abstract PlatformAdaptationLayer/*!*/ PlatformAdaptationLayer { get; }
        public abstract IList<string/*!*/>/*!*/ SourceUnitResolutionPath { get; }
        public abstract SourceUnit TryGetSourceFileUnit(LanguageContext/*!*/ langauge, string/*!*/ path, Encoding/*!*/ encoding, SourceCodeKind kind);
        public abstract SourceUnit ResolveSourceFileUnit(string name);
    }
}
