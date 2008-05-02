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
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class NewArrayExpression : Expression {
        private readonly ReadOnlyCollection<Expression> _expressions;

        internal NewArrayExpression(AstNodeType nodeType, Type type, ReadOnlyCollection<Expression> expressions)
            : base(nodeType, type) {
            _expressions = expressions;
        }

        public ReadOnlyCollection<Expression> Expressions {
            get { return _expressions; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        /// <summary>
        /// Creates a new array expression of the specified type from the provided initializers.
        /// </summary>
        /// <param name="type">The type of the array (e.g. object[]).</param>
        /// <param name="initializers">The expressions used to create the array elements.</param>
        public static NewArrayExpression NewArray(Type type, params Expression[] initializers) {
            return NewArray(type, (IList<Expression>)initializers);
        }

        /// <summary>
        /// Creates a new array expression of the specified type from the provided initializers.
        /// </summary>
        /// <param name="type">The type of the array (e.g. object[]).</param>
        /// <param name="initializers">The expressions used to create the array elements.</param>
        public static NewArrayExpression NewArray(Type type, IList<Expression> initializers) {
            ContractUtils.RequiresNotNull(initializers, "initializers");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(type.IsArray, "type", "Not an array type");
            ContractUtils.RequiresNotNullItems(initializers, "initializers");

            Type element = type.GetElementType();
            foreach (Expression expression in initializers) {
                ContractUtils.Requires(TypeUtils.CanAssign(element, expression.Type), "initializers");
            }

            return new NewArrayExpression(AstNodeType.NewArrayExpression, type, CollectionUtils.ToReadOnlyCollection(initializers));
        }

        public static NewArrayExpression NewArrayBounds(Type type, params Expression[] bounds) {
            return NewArrayBounds(type, (IList<Expression>)bounds);
        }

        public static NewArrayExpression NewArrayBounds(Type type, IList<Expression> bounds) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(type.IsArray, "type", "Not an array type");
            ContractUtils.Requires(bounds.Count > 0, "bounds", "Bounds count cannot be less than 1");
            ContractUtils.Requires(type.GetArrayRank() == bounds.Count, "bounds", "Bounds count must match the rank");

            ReadOnlyCollection<Expression> boundsList = CollectionUtils.ToReadOnlyCollection(bounds);
            for (int i = 0, n = boundsList.Count; i < n; i++) {
                Expression e = boundsList[i];
                ContractUtils.RequiresNotNull(e, "bounds");
                ContractUtils.Requires(TypeUtils.CanAssign(typeof(int), e.Type), "bounds", "Bounds must be ints.");
            }
            return new NewArrayExpression(AstNodeType.NewArrayBounds, type, CollectionUtils.ToReadOnlyCollection(bounds));
        }
        
        public static NewArrayExpression NewArrayHelper(Type type, IList<Expression> initializers) {
            ContractUtils.RequiresNotNullItems(initializers, "initializers");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(type.IsArray, "type", "Not an array type");

            Type element = type.GetElementType();
            Expression[] clone = null;
            for (int i = 0; i < initializers.Count; i++) {
                Expression initializer = initializers[i];
                if (!TypeUtils.CanAssign(element, initializer.Type)) {
                    if (clone == null) {
                        clone = new Expression[initializers.Count];
                        for (int j = 0; j < i; j++) {
                            clone[j] = initializers[j];
                        }
                    }
                    initializer = Expression.Convert(initializer, element);
                }
                if (clone != null) {
                    clone[i] = initializer;
                }
            }

            if (clone != null) {
                initializers = clone;
            }
            return NewArray(type, initializers);
        }
    }
}
