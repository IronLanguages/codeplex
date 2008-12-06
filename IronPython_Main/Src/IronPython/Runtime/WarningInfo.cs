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

using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    class WarningInfo {
        private readonly string/*!*/ _message;
        private readonly PythonType/*!*/ _type;
        private readonly Expression _condition;

        public WarningInfo(PythonType/*!*/ type, string/*!*/ message) {
            _message = message;
            _type = type;
        }

        public WarningInfo(PythonType/*!*/ type, string/*!*/ message, Expression condition) {
            _message = message;
            _type = type;
            _condition = condition;
        }

        public DynamicMetaObject/*!*/ AddWarning(Expression/*!*/ codeContext, DynamicMetaObject/*!*/ result) {
            Expression warn = Expression.Call(
                typeof(PythonOps).GetMethod("Warn"),
                codeContext,
                Expression.Constant(_type),
                Expression.Constant(_message),
                Expression.Constant(ArrayUtils.EmptyObjects)
            );

            if (_condition != null) {
                warn = Expression.Condition(_condition, warn, Expression.Empty());
            }

            return new DynamicMetaObject(
                    Expression.Block(
                    warn,
                    result.Expression
                ),
                result.Restrictions
            );
        }
    }
}
