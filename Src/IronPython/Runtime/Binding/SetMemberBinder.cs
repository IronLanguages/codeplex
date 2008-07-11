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
using System.Scripting;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Runtime;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    class SetMemberBinder : SetMemberAction, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;

        public SetMemberBinder(BinderState/*!*/ binder, string/*!*/ name)
            : base(name, false) {
            _state = binder;
        }

        public override MetaObject/*!*/ Fallback(MetaObject/*!*/[]/*!*/ args) {
            if (args[0].NeedsDeferral) {
                return Defer(args);
            }            

            return Binder.Binder.SetMember(Name, args[0], args[1], Ast.Constant(Binder.Context));
        }
        
        public BinderState/*!*/ Binder {
            get {
                return _state;
            }
        }

        public override object HashCookie {
            get { return this; }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            SetMemberBinder ob = obj as SetMemberBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && base.Equals(obj);
        }

        public override string ToString() {
            return "Python SetMember " + Name;
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeSetAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Ast.Constant(Name)
            );
        }

        #endregion
    }
}

