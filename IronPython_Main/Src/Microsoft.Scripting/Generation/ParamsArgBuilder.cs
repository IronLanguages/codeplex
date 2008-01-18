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

using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    using Microsoft.Scripting.Utils;
    using Ast = Microsoft.Scripting.Ast.Ast;

    class ParamsArgBuilder : ArgBuilder {
        private int _start;
        private int _count;
        private Type _elementType;
        public ParamsArgBuilder(int start, int count, Type elementType) {
            Contract.RequiresNotNull(elementType, "elementType");
            if (start < 0) throw new ArgumentOutOfRangeException("start");
            if (count < 0) throw new ArgumentOutOfRangeException("count");

            this._start = start;
            this._count = count;
            this._elementType = elementType;
        }

        public override int Priority {
            get { return 4; }
        }

        public override object Build(CodeContext context, object[] args) {
            Array paramsArray = Array.CreateInstance(_elementType, _count);
            for (int i = 0; i < _count; i++) {
                paramsArray.SetValue(context.LanguageContext.Binder.Convert(args[i + _start], _elementType), i);
            }
            return paramsArray;
        }

        internal override Expression ToExpression(MethodBinderContext context, Expression[] parameters) {
            Expression[] elems = new Expression[_count];
            for (int i = 0; i < _count; i++) {
                elems[i] = context.ConvertExpression(parameters[_start + i], _elementType);
            }

            return Ast.NewArray(_elementType.MakeArrayType(), elems);
        }


        public override Type Type {
            get {
                return _elementType.MakeArrayType();
            }
        }
    }
}
