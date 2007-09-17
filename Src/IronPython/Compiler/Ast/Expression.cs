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
using MSAst = Microsoft.Scripting.Ast;
using Operators = Microsoft.Scripting.Operators;

namespace IronPython.Compiler.Ast {
    public abstract class Expression : Node {
        internal abstract MSAst.Expression Transform(AstGenerator ag, Type type);

        internal virtual MSAst.Statement TransformSet(AstGenerator ag, MSAst.Expression right, Operators op) {
            ag.AddError("can't assign to " + GetType().Name, Span);
            return null;
        }

        internal virtual MSAst.Statement TransformDelete(AstGenerator ag) {
            ag.AddError("can't delete " + GetType().Name, Span);
            return null;
        }
    }
}
