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

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Helper class for emitting calls via the MethodBinder.
    /// </summary>
    class MethodBinderContext {
        private ActionBinder _actionBinder;
        private RuleBuilder _rule;

        internal MethodBinderContext(ActionBinder actionBinder, RuleBuilder rule) {
            _actionBinder = actionBinder;
            _rule = rule;
        }

        internal ActionBinder Binder {
            get {
                return _actionBinder;
            }
        }

        internal RuleBuilder Rule {
            get {
                return _rule;
            }
        }

        internal Expression ConvertExpression(Expression expr, Type type) {
            return _actionBinder.ConvertExpression(expr, type, _rule.Context);
        }

        internal VariableExpression GetTemporary(Type type, string name) {            
            return _rule.GetTemporary(type, name);
        }

        internal bool CanConvert(Type fromType, Type toType, NarrowingLevel level) {
            return _actionBinder.CanConvertFrom(fromType, toType, level);
        }
    }
}
