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

#if !SILVERLIGHT

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Scripting.Actions;

namespace System.Scripting.Com {
    class ComInvokeAction : InvokeAction {
        public override object HashCookie {
            get { return this; }
        }

        internal ComInvokeAction(params Argument[] arguments)
            : base(arguments) {
        }

        internal ComInvokeAction(IEnumerable<Argument> arguments)
            : base(arguments) {
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            return base.Equals(obj as ComInvokeAction);
        }

        public override MetaObject Fallback(MetaObject[] args, MetaObject onBindingError) {
            if (onBindingError == null) {
                onBindingError =
                    new MetaObject(
                        Expression.Throw(
                            Expression.New(
                                typeof(NotSupportedException).GetConstructor(new Type[] { typeof(string) }),
                                Expression.Constant("Cannot perform call")
                            )
                        ),
                        Restrictions.Combine(args)
                    );
            }

            return onBindingError;
        }
    }
}

#endif
