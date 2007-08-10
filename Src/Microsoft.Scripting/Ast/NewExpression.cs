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
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Reflection;
using System.Reflection.Emit;

using System.Diagnostics;

using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class NewExpression : Expression {
        readonly ConstructorInfo _constructor;
        readonly ReadOnlyCollection<Expression> _arguments;

        internal NewExpression(SourceSpan span, ConstructorInfo constructor, IList<Expression> arguments)
            : base(span) {
            _constructor = constructor;
            _arguments = new ReadOnlyCollection<Expression>(arguments);
        }

        public ConstructorInfo Constructor {
            get { return _constructor; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }

        public override Type ExpressionType {
            get {
                return _constructor.DeclaringType;
            }
        }

        public override void Emit(CodeGen cg) {
            ParameterInfo[] pis = _constructor.GetParameters();
            Debug.Assert(pis.Length == _arguments.Count);
            for (int i=0; i < pis.Length; i++) {
                _arguments[i].EmitAs(cg, pis[i].ParameterType);
            }
            cg.EmitNew(_constructor);
        }

        protected override object DoEvaluate(CodeContext context) {
            object[] args = new object[_arguments.Count];
            for (int i = 0; i < _arguments.Count; i++) {
                args[i] = _arguments[i].Evaluate(context);
            }
            try {
                return _constructor.Invoke(args);
            } catch (TargetInvocationException e) {
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                if (_arguments != null) {
                    foreach (Expression e in _arguments) {
                        e.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments) {
            return New(SourceSpan.None, constructor, arguments);
        }

        public static NewExpression New(SourceSpan span, ConstructorInfo constructor, params Expression[] arguments) {
            return new NewExpression(span, constructor, arguments);
        }
    }
}
