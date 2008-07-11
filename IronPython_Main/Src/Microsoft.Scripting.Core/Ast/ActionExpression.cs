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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Utils;

namespace System.Linq.Expressions {
    // TODO: remove or rename
    public sealed class ActionExpression : Expression {
        private readonly ReadOnlyCollection<Expression>/*!*/ _arguments;

        internal ActionExpression(Annotations annotations, CallSiteBinder/*!*/ binder, ReadOnlyCollection<Expression>/*!*/ arguments, Type/*!*/ result)
            : base(annotations, ExpressionType.ActionExpression, result, binder) {
            _arguments = arguments;
        }

        public CallSiteBinder Action {
            get { return BindingInfo; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }
    }

    public partial class Expression {
        // TODO: remove or rename
        public static ActionExpression ActionExpression(CallSiteBinder action, Type resultType, params Expression[] arguments) {
            return ActionExpression(action, resultType, Annotations.Empty, arguments);
        }
        // TODO: remove or rename
        public static ActionExpression ActionExpression(CallSiteBinder action, Type resultType, IEnumerable<Expression> arguments) {
            return ActionExpression(action, resultType, Annotations.Empty, arguments);
        }
        // TODO: remove or rename
        public static ActionExpression ActionExpression(CallSiteBinder action, Type resultType, Annotations annotations, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(action, "action");
            ContractUtils.RequiresNotNull(resultType, "result");

            ReadOnlyCollection<Expression> args = CollectionUtils.ToReadOnlyCollection(arguments);
            ContractUtils.RequiresNotNullItems(args, "arguments");

            return new ActionExpression(annotations, action, args, resultType);
        }
    }
}
