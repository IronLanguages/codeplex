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
using Microsoft.Scripting;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;

    partial class MetaPythonType : MetaPythonObject {
        public MetaPythonType(Expression/*!*/ expression, Restrictions/*!*/ restrictions, PythonType/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        public override MetaObject BindCreateInstance(CreateInstanceBinder create, params MetaObject[] args) {
            return InvokeWorker(create, args, Ast.Constant(BinderState.GetBinderState(create).Context));
        }

        public override MetaObject BindConvert(ConvertBinder/*!*/ conversion) {
            if (conversion.Type.IsSubclassOf(typeof(Delegate))) {
                return MakeDelegateTarget(conversion, conversion.Type, Restrict(Value.GetType()));
            }
            return conversion.FallbackConvert(this);
        }


        public new PythonType/*!*/ Value {
            get {
                return (PythonType)base.Value;
            }
        }
    }
}
