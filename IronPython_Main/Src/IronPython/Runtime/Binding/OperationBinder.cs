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
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Actions;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;

    class OperationBinder : OperationAction, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;

        public OperationBinder(BinderState/*!*/ state, string/*!*/ operation)
            : base(operation) {
            _state = state;
        }

        public override MetaObject/*!*/ Fallback(MetaObject/*!*/[]/*!*/ args, MetaObject onBindingError) {
            return PythonProtocol.Operation(this, args);
        }

        public override object HashCookie {
            get { return this; }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            OperationBinder ob = obj as OperationBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && base.Equals(obj);
        }

        public BinderState/*!*/ Binder {
            get {
                return _state;
            }
        }

        public override string ToString() {
            return "Python " + Operation;
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeOperationAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Expression.Constant(Operation)
            );
        }

        #endregion
    }
}
