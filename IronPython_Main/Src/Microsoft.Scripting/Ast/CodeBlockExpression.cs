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

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Text;
using System.Threading;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// An expression that will return a reference to a block of code.
    /// Currently these references are created by emitting a delegate of the 
    /// requested type.
    /// </summary>
    public class CodeBlockExpression : Expression {
        private CodeBlock _block;
        private bool _forceWrapperMethod;

        public CodeBlockExpression(CodeBlock block, bool forceWrapperMethod)
            : base(SourceSpan.None) {
            this._block = block;
            this._forceWrapperMethod = forceWrapperMethod;
        }

        public CodeBlock Block {
            get { return _block; }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _block.Walk(walker);
            }
            walker.PostWalk(this);
        }

        public override Type ExpressionType {
            get {
                return typeof(Delegate);
            }
        }

        public override object Evaluate(CodeContext context) {
            return new CallTargetWithContextN(_block.Execute);
        }

        public override void Emit(CodeGen cg) {
            _block.EmitDelegate(cg, _forceWrapperMethod);
        }
    }
}
