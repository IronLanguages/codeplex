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
        private ReadOnlyCollection<Expression> _expressions;

        // TODO: Remove!!!
        private readonly ConstructorInfo _constructor;

        internal NewArrayExpression(Type type, ReadOnlyCollection<Expression> expressions)
            : base(AstNodeType.NewArrayExpression, type) {
            _expressions = expressions;
            _constructor = type.GetConstructor(new Type[] { typeof(int) });
        }

        public ReadOnlyCollection<Expression> Expressions {
            get { return _expressions; }
        }

        internal ConstructorInfo Constructor {
            get { return _constructor; }
        } 
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
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
            Contract.RequiresNotNull(initializers, "initializers");
            Contract.RequiresNotNull(type, "type");
            Contract.Requires(type.IsArray, "type", "Not an array type");
            Contract.RequiresNotNullItems(initializers, "initializers");

            Type element = type.GetElementType();
            foreach (Expression expression in initializers) {
                Contract.Requires(TypeUtils.CanAssign(element, expression.Type), "initializers");
            }

            return new NewArrayExpression(type, CollectionUtils.ToReadOnlyCollection(initializers));
        }

        public static NewArrayExpression NewArrayHelper(Type type, IList<Expression> initializers) {
            Contract.RequiresNotNullItems(initializers, "initializers");
            Contract.RequiresNotNull(type, "type");
            Contract.Requires(type.IsArray, "type", "Not an array type");

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
                    initializer = Ast.Convert(initializer, element);
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
