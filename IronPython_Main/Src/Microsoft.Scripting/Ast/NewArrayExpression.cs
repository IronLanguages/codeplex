/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class NewArrayExpression : Expression {
        private IList<Expression> _expressions;
        private Type _type;

        /// <summary>
        /// Creates a new array expression of the specified type from the provided initializers.
        /// </summary>
        /// <param name="type">The type of the array (e.g. object[]).</param>
        /// <param name="initializers">The expressions used to create the array elements.</param>
        public static NewArrayExpression NewArrayInit(Type type, IEnumerable<Expression> initializers) {
            return new NewArrayExpression(type, new List<Expression>(initializers));
        }

        /// <summary>
        /// Creates a new array expression of the specified type from the provided initializers.
        /// </summary>
        /// <param name="type">The type of the array (e.g. object[]).</param>
        /// <param name="initializers">The expressions used to create the array elements.</param>
        public static NewArrayExpression NewArrayInit(Type type, params Expression[] initializers) {
            return new NewArrayExpression(type, new List<Expression>(initializers));
        }

        private NewArrayExpression(Type type, IList<Expression> expressions) {
            if (expressions == null) throw new ArgumentNullException("expressions");
            if (type == null) throw new ArgumentNullException("type");
            for (int i = 0; i < expressions.Count; i++) {
                if (expressions[i] == null) {
                    Debug.Assert(false);
                    throw new ArgumentNullException("expressions[" + i.ToString() + "]"); 
                }
            }

            _type = type;
            _expressions = expressions;
        }

        public IList<Expression> Expressions {
            get { return _expressions; }
        }

        public override Type ExpressionType {
            get {
                return _type;
            }
        }

        public override void EmitAs(CodeGen cg, Type asType) {
            cg.EmitArrayFromExpressions(_type.GetElementType(), _expressions);
            cg.EmitConvert(ExpressionType, asType);
        }

        public override void Emit(CodeGen cg) {
            cg.EmitArrayFromExpressions(_type.GetElementType(), _expressions);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                foreach (Expression expr in _expressions) {
                    expr.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
