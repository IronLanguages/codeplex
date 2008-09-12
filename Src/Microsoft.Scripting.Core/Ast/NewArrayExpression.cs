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
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;
using System.Text;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    public sealed class NewArrayExpression : Expression {
        private readonly ReadOnlyCollection<Expression> _expressions;

        internal NewArrayExpression(Annotations annotations, ExpressionType nodeType, Type type, ReadOnlyCollection<Expression> expressions)
            : base(nodeType, type, annotations) {
            _expressions = expressions;
        }


        public ReadOnlyCollection<Expression> Expressions {
            get { return _expressions; }
        }
        internal override void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");

            switch (this.NodeType) {
                case ExpressionType.NewArrayBounds:
                    builder.Append("new ");
                    builder.Append(this.Type.ToString());
                    builder.Append("(");
                    for (int i = 0, n = _expressions.Count; i < n; i++) {
                        if (i > 0) builder.Append(", ");
                        _expressions[i].BuildString(builder);
                    }
                    builder.Append(")");
                    break;
                case ExpressionType.NewArrayInit:
                    builder.Append("new ");
                    builder.Append("[] {");
                    for (int i = 0, n = _expressions.Count; i < n; i++) {
                        if (i > 0) builder.Append(", ");
                        _expressions[i].BuildString(builder);
                    }
                    builder.Append("}");
                    break;
            }
        }

    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {

        #region NewArrayInit

        //CONFORMING
        /// <summary>
        /// Creates a new array expression of the specified type from the provided initializers.
        /// </summary>
        /// <param name="type">A Type that represents the element type of the array.</param>
        /// <param name="initializers">The expressions used to create the array elements.</param>
        public static NewArrayExpression NewArrayInit(Type type, params Expression[] initializers) {
            return NewArrayInit(type, Annotations.Empty, (IEnumerable<Expression>)initializers);
        }

        public static NewArrayExpression NewArrayInit(Type type, Annotations annotations, params Expression[] initializers) {
            return NewArrayInit(type, annotations, (IEnumerable<Expression>)initializers);
        }

        /// <summary>
        /// Creates a new array expression of the specified type from the provided initializers.
        /// </summary>
        /// <param name="type">A Type that represents the element type of the array.</param>
        /// <param name="initializers">The expressions used to create the array elements.</param>
        public static NewArrayExpression NewArrayInit(Type type, IEnumerable<Expression> initializers) {
            return NewArrayInit(type, Annotations.Empty, initializers);
        }

        //CONFORMING
        public static NewArrayExpression NewArrayInit(Type type, Annotations annotations, IEnumerable<Expression> initializers) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            if (type.Equals(typeof(void))) {
                throw Error.ArgumentCannotBeOfTypeVoid();
            }

            ReadOnlyCollection<Expression> initializerList = initializers.ToReadOnly();

            Expression[] newList = null;
            for (int i = 0, n = initializerList.Count; i < n; i++) {
                Expression expr = initializerList[i];
                RequiresCanRead(expr, "initializers");

                if (!TypeUtils.AreReferenceAssignable(type, expr.Type)) {
                    if (TypeUtils.IsSameOrSubclass(typeof(Expression), type) && TypeUtils.AreAssignable(type, expr.GetType())) {
                        expr = Expression.Quote(expr);
                    } else {
                        throw Error.ExpressionTypeCannotInitializeArrayType(expr.Type, type);
                    }
                    if (newList == null) {
                        newList = new Expression[initializerList.Count];
                        for (int j = 0; j < i; j++) {
                            newList[j] = initializerList[j];
                        }
                    }
                }
                if (newList != null) {
                    newList[i] = expr;
                }
            }
            if (newList != null) {
                initializerList = new ReadOnlyCollection<Expression>(newList);
            }

            return new NewArrayExpression(annotations, ExpressionType.NewArrayInit, type.MakeArrayType(), initializerList);
        }

        #endregion

        #region NewArrayBounds

        //CONFORMING
        public static NewArrayExpression NewArrayBounds(Type type, params Expression[] bounds) {
            return NewArrayBounds(type, (IEnumerable<Expression>)bounds);
        }

        public static NewArrayExpression NewArrayBounds(Type type, IEnumerable<Expression> bounds) {
            return NewArrayBounds(type, Annotations.Empty, bounds);
        }
        //CONFORMING
        public static NewArrayExpression NewArrayBounds(Type type, Annotations annotations, IEnumerable<Expression> bounds) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(bounds, "bounds");

            if (type.Equals(typeof(void))) {
                throw Error.ArgumentCannotBeOfTypeVoid();
            }

            ReadOnlyCollection<Expression> boundsList = bounds.ToReadOnly();

            int dimensions = boundsList.Count;
            ContractUtils.Requires(dimensions > 0, "bounds", Strings.BoundsCannotBeLessThanOne);

            for (int i = 0; i < dimensions; i++) {
                Expression expr = boundsList[i];
                RequiresCanRead(expr, "bounds");
                if (!TypeUtils.IsInteger(expr.Type)) {
                    throw Error.ArgumentMustBeInteger();
                }
            }

            return new NewArrayExpression(annotations, ExpressionType.NewArrayBounds, type.MakeArrayType(dimensions), bounds.ToReadOnly());
        }

        #endregion

        public static NewArrayExpression NewArrayHelper(Type type, IEnumerable<Expression> initializers) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(initializers, "initializers");

            if (type.Equals(typeof(void))) {
                throw Error.ArgumentCannotBeOfTypeVoid();
            }

            ReadOnlyCollection<Expression> initializerList = initializers.ToReadOnly();

            Expression[] clone = null;
            for (int i = 0; i < initializerList.Count; i++) {
                Expression initializer = initializerList[i];
                RequiresCanRead(initializer, "initializers");

                if (!TypeUtils.AreReferenceAssignable(type, initializer.Type)) {
                    if (clone == null) {
                        clone = new Expression[initializerList.Count];
                        for (int j = 0; j < i; j++) {
                            clone[j] = initializerList[j];
                        }
                    }
                    if (TypeUtils.IsSameOrSubclass(typeof(Expression), type) && TypeUtils.AreAssignable(type, initializer.GetType())) {
                        initializer = Expression.Quote(initializer);
                    } else {
                        initializer = Expression.Convert(initializer, type);
                    }

                }
                if (clone != null) {
                    clone[i] = initializer;
                }
            }

            if (clone != null) {
                initializerList = new ReadOnlyCollection<Expression>(clone);
            }

            return new NewArrayExpression(Annotations.Empty, ExpressionType.NewArrayInit, type.MakeArrayType(), initializerList);
        }

    }
}
