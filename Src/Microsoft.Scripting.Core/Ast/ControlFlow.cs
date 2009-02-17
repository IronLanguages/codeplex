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


namespace Microsoft.Scripting.Ast {
    // Singleton objects of this enum type are used as return values from Statement.Execute() to handle control flow.
    enum ControlFlowKind {
        NextStatement,
        Break,
        Continue,
        Return
    };

    sealed class ControlFlow {
        private readonly ControlFlowKind _kind;
        private readonly object _value;          // for returns

        private ControlFlow(ControlFlowKind kind)
            : this(kind, null) {
        }

        private ControlFlow(ControlFlowKind kind, object value) {
            _kind = kind;
            _value = value;
        }

        internal ControlFlowKind Kind {
            get { return _kind; }
        }

        internal object Value {
            get { return _value; }
        }

        internal static ControlFlow Return(object value) {
            return new ControlFlow(ControlFlowKind.Return, value);
        }

        // Hold on to one instance for each member of the ControlFlow enumeration to avoid unnecessary boxing
        internal static readonly ControlFlow NextStatement = new ControlFlow(ControlFlowKind.NextStatement);
        internal static readonly ControlFlow Break = new ControlFlow(ControlFlowKind.Break);
        internal static readonly ControlFlow Continue = new ControlFlow(ControlFlowKind.Continue);
    }
}
