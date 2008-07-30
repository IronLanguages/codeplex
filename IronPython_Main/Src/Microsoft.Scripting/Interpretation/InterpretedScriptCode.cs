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

using System.Linq.Expressions;
using System.Scripting;
using System.Scripting.Runtime;
using System.Collections.Generic;
using System.Scripting.Actions;
using System.Threading;

namespace Microsoft.Scripting.Interpretation {
    public class InterpretedScriptCode : ScriptCode {
        // call sites allocated for the tree:
        private Dictionary<Expression, CallSite> _callSites;

        internal Dictionary<Expression, CallSite> CallSites {
            get {
                if (_callSites == null) {
                    Interlocked.CompareExchange(ref _callSites, new Dictionary<Expression, CallSite>(), null);
                }

                return _callSites;
            }
        }

        public InterpretedScriptCode(LambdaExpression code, SourceUnit sourceUnit)
            : base(code, sourceUnit) {
        }

        public override void EnsureCompiled() {
            // nop
        }

        protected override object InvokeTarget(LambdaExpression code, Scope scope) {
            return Interpreter.TopLevelExecute(this, scope, LanguageContext);
        }
    }
}
