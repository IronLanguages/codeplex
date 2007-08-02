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
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Summary description for Expr.
    /// </summary>
    public abstract class Expression : Node {

        protected Expression(SourceSpan span)
            : base(span) {
        }

        public virtual object Evaluate(CodeContext context) {
            throw new NotImplementedException(String.Format(CultureInfo.CurrentCulture, Resources.NotImplemented_Evaluate, this));
        }


        public virtual AbstractValue AbstractEvaluate(AbstractContext context) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates code for this expression in a value position.
        /// This method will leave the value of the expression
        /// on the top of the stack typed as ExpressionType.
        /// </summary>
        /// <param name="cg">Where to generate the code.</param>
        public abstract void Emit(CodeGen cg);

        /// <summary>
        /// Generates the code for the expression, leaving it on
        /// the stack typed as object.
        /// </summary>
        /// <param name="cg">Where to emit the code.</param>
        public void EmitAsObject(CodeGen cg) {
            this.Emit(cg);  // emit as ExpressionType
            cg.EmitBoxing(ExpressionType);
        }

        internal void EmitCast(CodeGen cg, Type asType) {
            this.Emit(cg);  // emit as ExpressionType
            cg.EmitCast(ExpressionType, asType);
        }

        /// <summary>
        /// Generates code for this expression in a value position.  This will leave
        /// the value of the expression on the top of the stack typed as asType.
        /// </summary>
        /// <param name="cg">Where to generate the code.</param>
        /// <param name="asType">The type to leave on top of the stack.</param>
        internal void EmitAs(CodeGen cg, Type asType) {
            this.Emit(cg);  // emit as ExpressionType
            cg.EmitConvert(ExpressionType, asType);
        }

        /// <summary>
        /// Generates this expression as a bool and then branches to label
        /// if the resulting bool is false.  This can be overriden by 
        /// comparison expressions to generate tighter code by using
        /// the opcodes that include both a test and a branch in one.
        /// </summary>
        /// <param name="cg">Where to generate the code.</param>
        /// <param name="label">Where to branch if this expression is false.</param>
        public virtual void EmitBranchFalse(CodeGen cg, Label label) {
            this.EmitAs(cg, typeof(bool));
            cg.Emit(OpCodes.Brfalse, label);
        }

        /// <summary>
        /// This is the dual of EmitBranchFalse.
        /// </summary>
        public virtual void EmitBranchTrue(CodeGen cg, Label label) {
            this.EmitAs(cg, typeof(bool));
            cg.Emit(OpCodes.Brtrue, label);
        }

        /// <summary>
        /// Tests to see if this expression is a constant with the given value.
        /// </summary>
        /// <param name="value">The constant value to check for.</param>
        public virtual bool IsConstant(object value) {
            return false;
        }

        public virtual void EmitAddress(CodeGen cg, Type asType) {
            EmitAs(cg, asType);
            Slot tmp = cg.GetLocalTmp(asType);
            tmp.EmitSet(cg);
            tmp.EmitGetAddr(cg);
        }

        public virtual Type ExpressionType {
            get { return typeof(object); }
        }

        public static object[] Evaluate(IList<Expression> items, CodeContext context) {
            if (items == null) throw new ArgumentNullException("items");
            if (context == null) throw new ArgumentNullException("environment");

            object[] ret = new object[items.Count];
            for (int i = 0; i < items.Count; i++) {
                ret[i] = items[i].Evaluate(context);
            }
            return ret;
        }
    }
}
