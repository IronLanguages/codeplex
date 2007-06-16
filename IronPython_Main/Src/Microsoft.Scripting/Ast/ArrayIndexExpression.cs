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

using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class ArrayIndexExpression : Expression {
        private readonly Expression _array;
        private readonly Expression _index;
        private readonly Type _elementType;

        public ArrayIndexExpression(Expression array, Expression index)
            : this(array, index, SourceSpan.None) {
        }

        public ArrayIndexExpression(Expression array, Expression index, SourceSpan span)
            : base(span) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }
            if (index == null) {
                throw new ArgumentNullException("index");
            }

            Type arrayType = array.ExpressionType;
            if (!arrayType.IsArray) {
                throw new NotSupportedException("Expression type of the array must be array (Type.IsArray)!");
            }

            _array = array;
            _index = index;
            _elementType = arrayType.GetElementType();
        }

        public Expression Array {
            get { return _array; }
        }

        public Expression Index {
            get { return _index; }
        }

        public override Type ExpressionType {
            get {
                return _elementType;
            }
        }

        public override void Emit(CodeGen cg) {
            // Emit the array reference
            _array.Emit(cg);
            // Emit the index (as integer)
            _index.EmitAs(cg, typeof(int));
            // Load the array element
            cg.EmitLoadElement(_elementType);
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
