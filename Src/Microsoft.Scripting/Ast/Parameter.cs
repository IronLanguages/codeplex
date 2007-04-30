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
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    /// <summary>
    /// Represents a formal parameter of the CodeBlock.
    /// </summary>
    public sealed class Parameter : Variable {
        private Parameter(CodeBlock block, SymbolId name, Type type, Expression defaultValue)
            : base(name, VariableKind.Parameter, block, type, defaultValue) {
        }

        #region Factory methods

        public static Parameter Create(CodeBlock block, SymbolId name) {
            return new Parameter(block, name, typeof(object), null);
        }

        public static Parameter Create(CodeBlock block, SymbolId name, Expression defaultValue) {
            return new Parameter(block, name, typeof(object), defaultValue);
        }

        #endregion
    }
}
