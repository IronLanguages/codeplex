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

#if !SILVERLIGHT // ComObject

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting.Actions;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Actions.ComDispatch {
    
    internal abstract class ComBinderHelper<T, TAction>
        where T : class 
        where TAction : OldDynamicAction {

        private readonly CodeContext _context;
        private readonly TAction _action;
        private readonly RuleBuilder<T> _rule;

        internal ComBinderHelper(CodeContext context, TAction action) {
            Assert.NotNull(context, action);

            _context = context;
            _action = action;
            _rule = new RuleBuilder<T>();
            _rule.Target = Expression.Empty();
        }

        protected CodeContext Context {
            get { return _context; }
        }

        protected TAction Action {
            get { return _action; }
        }

        protected ActionBinder Binder {
            get { return _context.LanguageContext.Binder; }
        }

        protected RuleBuilder<T> Rule {
            get { return _rule; }
        }

        /// <summary>
        /// There is no setter on Body.  Use AddToBody to extend it instead.
        /// </summary>
        protected Expression Body {
            get {
                return _rule.Target;
            }
        }

        protected Expression Test {
            get {
                return _rule.Test;
            }
            set {
                _rule.Test = value;
            }
        }

        protected static Expression MakeParamsTest(object paramArg, Expression listArg) {
            return Expression.AndAlso(
                Expression.TypeIs(listArg, typeof(ICollection<object>)),
                Expression.Equal(
                    Expression.Property(
                        Expression.Convert(listArg, typeof(ICollection<object>)),
                        typeof(ICollection<object>).GetProperty("Count")
                    ),
                    Expression.Constant(((IList<object>)paramArg).Count)
                )
            );
        }

        /// <summary>
        /// Use this method to extend the Body.  It will create BlockStatements as needed.
        /// </summary>
        /// <param name="expression"></param>
        protected void AddToBody(Expression expression) {
            if (_rule.Target.NodeType == ExpressionType.EmptyStatement) {
                _rule.Target = expression;
            } else {
                _rule.Target = Expression.Block(_rule.Target, expression);
            }
        }


        protected Expression MakeNecessaryTests(Type[] testTypes, IList<Expression> arguments) {
            Expression typeTest = Expression.Constant(true);

            if (testTypes != null) {
                for (int i = 0; i < testTypes.Length; i++) {
                    if (testTypes[i] != null) {
                        Debug.Assert(i < arguments.Count);
                        typeTest = Expression.AndAlso(typeTest, _rule.MakeTypeTest(testTypes[i], arguments[i]));
                    }
                }
            }

            return typeTest;
        }
    }
}

#endif
