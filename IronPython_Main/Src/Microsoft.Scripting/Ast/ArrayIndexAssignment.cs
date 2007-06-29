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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class ArrayIndexAssignment : Expression{
        private readonly Expression _array;
        private readonly Expression _index;
        private readonly Expression _value;
        private readonly Type _elementType;

        internal ArrayIndexAssignment(SourceSpan span, Expression array, Expression index, Expression value)
            : base(span) {
            if (array == null) throw new ArgumentNullException("array");
            if (index == null) throw new ArgumentNullException("index");
            if (value == null) throw new ArgumentNullException("value");
            Type arrayType = array.ExpressionType;
            if (!arrayType.IsArray) {
                throw new NotSupportedException("Expression type of the array must be array (Type.IsArray)!");
            }

            _array = array;
            _index = index;
            _value = value;
            _elementType = arrayType.GetElementType();
        }

        public Expression Array {
            get { return _array; }
        }

        public Expression Index {
            get { return _index; }
        }

        public Expression Value {
            get { return _value; }
        }

        public override Type ExpressionType {
            get {
                return _elementType;
            }
        }

        public override object Evaluate(CodeContext context) {
            object value = _value.Evaluate(context); // evaluate the value first
            object[] array = (object[])_array.Evaluate(context);
            int index = (int)context.LanguageContext.Binder.Convert(_index.Evaluate(context), typeof(int));
            array[index] = value;
            return null; //??? does Emit leave behind a value on the stack? should we return value instead?
        }

        public override void Emit(CodeGen cg) {
            _value.EmitAs(cg, _elementType);

            // Save the expression value - order of evaluation is different than that of the Stelem* instruction
            Slot temp = cg.GetLocalTmp(_elementType);
            temp.EmitSet(cg);

            // Emit the array reference
            _array.Emit(cg);
            // Emit the index (as integer)
            _index.EmitAs(cg, typeof(int));
            // Emit the value
            temp.EmitGet(cg);
            // Store it in the array
            cg.EmitStoreElement(_elementType);
            temp.EmitGet(cg);
            cg.FreeLocalTmp(temp);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _array.Walk(walker);
                _index.Walk(walker);
                _value.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        public static ArrayIndexAssignment AssignItem(SourceSpan span, Expression array, Expression index, Expression value) {
            return new ArrayIndexAssignment(span, array, index, value);
        }
    }
}
