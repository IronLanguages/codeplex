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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Actions;
using Microsoft.Linq.Expressions;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;

    /// <summary>
    /// Builds up a series of conditionals when the False clause isn't yet known.  We can
    /// keep appending conditions and if true's.  Each subsequent true branch becomes the
    /// false branch of the previous condition and body.  Finally a non-conditional terminating
    /// branch must be added.
    /// </summary>
    class ConditionalBuilder {
        private readonly MetaObjectBinder/*!*/ _action;
        private readonly List<Expression/*!*/>/*!*/ _conditions = new List<Expression>();
        private readonly List<Expression/*!*/>/*!*/ _bodies = new List<Expression>();
        private readonly List<ParameterExpression/*!*/>/*!*/ _variables = new List<ParameterExpression>();
        private Expression _body;
        private bool _testCoercionRecursionCheck;
        private Restrictions/*!*/ _restrictions = Restrictions.Empty;
        private ParameterExpression _compareRetBool;

        public ConditionalBuilder(MetaObjectBinder/*!*/ action) {
            _action = action;
        }

        /// <summary>
        /// Adds a new conditional and body.  The first call this becomes the top-level
        /// conditional, subsequent calls will have it added as false statement of the
        /// previous conditional.
        /// </summary>
        public void AddCondition(Expression/*!*/ condition, Expression/*!*/ body) {
            _conditions.Add(condition);
            _bodies.Add(body);
        }

        /// <summary>
        /// Adds the non-conditional terminating node.
        /// </summary>
        public void FinishCondition(Expression/*!*/ body) {
            if (_body != null) throw new InvalidOperationException();

            for (int i = _bodies.Count - 1; i >= 0; i--) {
                Type t = _bodies[i].Type;
                Type otherType = body.Type;

                t = BindingHelpers.GetCompatibleType(t, otherType);

                body = Ast.Condition(
                    _conditions[i],
                    Ast.ConvertHelper(_bodies[i], t),
                    Ast.ConvertHelper(body, t)
                );
            }

            _body = Ast.Scope(
                body,
                _variables
            );
        }

        public ParameterExpression CompareRetBool {
            get {
                if (_compareRetBool == null) {
                    _compareRetBool = Expression.Variable(typeof(bool), "compareRetBool");
                    AddVariable(_compareRetBool);
                }

                return _compareRetBool;
            }
        }

        public Restrictions Restrictions {
            get {
                return _restrictions;
            }
            set {
                _restrictions = value;
            }
        }

        public bool TestCoercionRecursionCheck {
            get {
                return _testCoercionRecursionCheck;
            }
            set {
                _testCoercionRecursionCheck = value;
            }
        }

        public MetaObjectBinder/*!*/ Action {
            get {
                return _action;
            }
        }

        /// <summary>
        /// Returns true if no conditions have been added
        /// </summary>
        public bool NoConditions {
            get {
                return _conditions.Count == 0;
            }
        }

        /// <summary>
        /// Returns true if a final, non-conditional, body has been added.
        /// </summary>
        public bool IsFinal {
            get {
                return _body != null;
            }
        }

        /// <summary>
        /// Gets the resulting meta object for the full body.  FinishCondition
        /// must have been called.
        /// </summary>
        public MetaObject/*!*/ GetMetaObject(params MetaObject/*!*/[]/*!*/ types) {
            if (_body == null) {
                throw new InvalidOperationException("FinishCondition not called before GetMetaObject");
            }

            return new MetaObject(
                _body,
                Restrictions.Combine(types)
            );
        }

        /// <summary>
        /// Adds a variable which will be scoped at the level of the final expression.
        /// </summary>
        public void AddVariable(ParameterExpression/*!*/ var) {
            if (_body != null) {
                throw new InvalidOperationException("Variables must be added before calling FinishCondition");
            }

            _variables.Add(var);
        }
    }

}
