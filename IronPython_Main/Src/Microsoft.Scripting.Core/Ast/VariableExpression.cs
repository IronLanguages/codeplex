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
using System.Diagnostics;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// VariableExpression represents actual memory/dictionary location in the generated code.
    /// </summary>
    public sealed class VariableExpression : Expression {
        private readonly SymbolId _name;

        private VariableExpression(AstNodeType kind, SymbolId name, Type type)
            : base(kind, type) {
            Debug.Assert(type != typeof(void));
            Debug.Assert(
                kind == AstNodeType.LocalVariable ||
                kind == AstNodeType.TemporaryVariable ||
                kind == AstNodeType.GlobalVariable);
            _name = name;
        }

        public SymbolId Name {
            get { return _name; }
        }

        // TODO: move to Ast, fix parameter order to be type, name (matches Linq ParameterExpression)
        #region Factory methods

        public static VariableExpression Local(SymbolId name, Type type) {
            return new VariableExpression(AstNodeType.LocalVariable, name, GetNonVoidType(type));
        }

        public static VariableExpression Temporary(SymbolId name, Type type) {
            return new VariableExpression(AstNodeType.TemporaryVariable, name, GetNonVoidType(type));
        }

        public static VariableExpression Global(SymbolId name, Type type) {
            return new VariableExpression(AstNodeType.GlobalVariable, name, GetNonVoidType(type));
        }

        // Converts typeof(void) to typeof(object), leaving other types unchanged.
        //
        // typeof(void) is allowed as the variable type to support this: 
        //
        // temp = CreateVariable(..., expression.Type, ...)
        // Ast.Assign(temp, expression)
        //
        // where expression.Type is void.
        private static Type GetNonVoidType(Type t) {
            return (t != typeof(void)) ? t : typeof(object);
        }

        #endregion
    }

    public static partial class Ast {
        // TODO: Remove
        public static VariableExpression Read(VariableExpression variable) {
            Contract.RequiresNotNull(variable, "variable");
            return variable;
        }

        // TODO: Remove
        public static VariableExpression ReadDefined(VariableExpression variable) {
            return Read(variable);
        }
    }
}
