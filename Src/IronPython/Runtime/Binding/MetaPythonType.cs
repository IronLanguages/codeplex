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
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Utils;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {

    partial class MetaPythonType : MetaPythonObject {
        public MetaPythonType(Expression/*!*/ expression, Restrictions/*!*/ restrictions, PythonType/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        /*public override MetaObject Create(CreateAction create, params MetaObject[] args) {
            return Call(create, args);
        }*/

        public override MetaObject Convert(ConvertAction/*!*/ conversion, MetaObject/*!*/[]/*!*/ args) {
            if (conversion.ToType.IsSubclassOf(typeof(Delegate))) {
                return MakeDelegateTarget(conversion, conversion.ToType, Restrict(Value.GetType()));
            }
            return conversion.Fallback(args);
        }


        public new PythonType/*!*/ Value {
            get {
                return (PythonType)base.Value;
            }
        }
    }
}
