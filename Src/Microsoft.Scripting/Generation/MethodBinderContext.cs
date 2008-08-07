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
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Helper class for emitting calls via the MethodBinder.
    /// </summary>
    class MethodBinderContext {
        private readonly ActionBinder _actionBinder;
        private readonly Expression _context;
        private List<VariableExpression> _temps;

        internal MethodBinderContext(ActionBinder actionBinder, Expression codeContext) {
            Assert.NotNull(actionBinder, codeContext);

            _actionBinder = actionBinder;
            _context = codeContext;
        }

        internal ActionBinder Binder {
            get {
                return _actionBinder;
            }
        }

        internal Expression ContextExpression {
            get {
                return _context;
            }
        }

        internal Expression ConvertExpression(Expression expr, Type type) {
            Assert.NotNull(expr, type);

            return _actionBinder.ConvertExpression(expr, type, ConversionResultKind.ExplicitCast, ContextExpression);
        }

        internal VariableExpression GetTemporary(Type type, string name) {
            Assert.NotNull(type, name);

            if (_temps == null) {
                _temps = new List<VariableExpression>();
            }

            VariableExpression res = Expression.Variable(type, name);
            _temps.Add(res);
            return res;
        }

        internal bool CanConvert(Type fromType, Type toType, NarrowingLevel level) {
            Assert.NotNull(fromType, toType);

            return _actionBinder.CanConvertFrom(fromType, toType, level);
        }

        internal List<VariableExpression> Temps {
            get {
                return _temps;
            }
        }
    }
}
