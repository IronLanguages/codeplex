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

using System; using Microsoft;
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;

    class PythonDeleteSliceBinder : DynamicMetaObjectBinder, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;

        public PythonDeleteSliceBinder(BinderState/*!*/ state) {
            _state = state;
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            return PythonProtocol.Index(this, PythonIndexType.DeleteSlice, ArrayUtils.Insert(target, args));
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            PythonDeleteSliceBinder ob = obj as PythonDeleteSliceBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && base.Equals(obj);
        }

        #region IPythonSite Members

        public BinderState/*!*/ Binder {
            get { return _state; }
        }

        #endregion

        #region IExpressionSerializable Members

        public Expression/*!*/ CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeDeleteSliceBinder"),
                BindingHelpers.CreateBinderStateExpression()
            );
        }

        #endregion
    }
}
