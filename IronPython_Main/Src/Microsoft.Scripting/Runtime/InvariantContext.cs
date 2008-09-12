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

using System; using Microsoft;
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Singleton LanguageContext which represents a language-neutral LanguageContext
    /// </summary>
    internal sealed class InvariantContext : LanguageContext {
        // friend: ScriptDomainManager
        internal InvariantContext(ScriptDomainManager manager)
            : base(manager) {
            // TODO: use InvariantBinder
            Binder = new DefaultActionBinder(manager, Type.EmptyTypes);
        }

        public override bool CanCreateSourceCode {
            get { return false; }
        }

        protected internal override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink) {
            // invariant language doesn't have a grammar:
            throw new NotSupportedException();
        }
    }
}
