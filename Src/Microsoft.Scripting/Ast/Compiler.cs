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
using System.Reflection.Emit;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public partial class Compiler {
        private CodeGen _cg;

        private Compiler(CodeGen cg) {
            Debug.Assert(cg != null);
            _cg = cg;
        }

        public static void Emit(CodeGen cg, Statement node) {
            new Compiler(cg).EmitStatement(node);
        }

        internal static void Emit(CodeGen cg, Expression node) {
            new Compiler(cg).EmitExpression(node);
        }

        // TODO: REMOVE!!!
        internal static void EmitBranchFalse(CodeGen cg, Expression node, Label label) {
            new Compiler(cg).EmitBranchFalse(node, label);
        }

        // TODO: REMOVE!!!
        internal static void EmitExpressionAsObjectOrNull(CodeGen cg, Expression node) {
            new Compiler(cg).EmitExpressionAsObjectOrNull(node);
        }

        public void EmitExpressionAsObjectOrNull(Expression node) {
            if (node == null) {
                _cg.Emit(OpCodes.Ldnull);
            } else {
                EmitExpressionAsObject(node);
            }
        }

        // TODO: REMOVE !!!
        internal static void EmitAs(CodeGen cg, Expression node, Type type) {
            new Compiler(cg).EmitAs(node, type);
        }

        // TODO: REMOVE !!!
        /// <summary>
        /// Generates code for this expression in a value position.  This will leave
        /// the value of the expression on the top of the stack typed as asType.
        /// </summary>
        internal void EmitAs(Expression node, Type type) {
            EmitExpression(node);  // emit as Type
            if (type.IsValueType || !node.IsConstant(null)) {
                _cg.EmitConvert(node.Type, type);
            }
        }

        private void EmitExpressionAndPop(Expression node) {
            EmitExpression(node);
            if (node.Type != typeof(void)) {
                _cg.Emit(OpCodes.Pop);
            }
        }
    }
}
