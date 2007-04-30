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

using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class ArrayIndexExpression : Expression {
        private readonly Expression _array;
        private readonly Expression _index;

        public ArrayIndexExpression(Expression array, Expression index)
            : this(array, index, SourceSpan.None) {
        }

        public ArrayIndexExpression(Expression array, Expression index, SourceSpan span)
            : base(span) {
            if (!array.ExpressionType.IsArray) {
                throw new NotSupportedException("Expression type of the array must be array (Type.IsArray)!");
            }
            _array = array;
            _index = index;
        }

        public Expression Array {
            get { return _array; }
        }

        public Expression Index {
            get { return _index; }
        }

        public override void Emit(CodeGen cg) {
            // Emit the array reference
            _array.Emit(cg);
            // Emit the index (as integer)
            _index.EmitAs(cg, typeof(int));
            Type arrayType = _array.ExpressionType;
            Debug.Assert(arrayType.IsArray, "Expression type must be array!");
            cg.EmitLoadElement(arrayType.IsArray ? arrayType.GetElementType() : typeof(object));
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _array.Walk(walker);
                _index.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
