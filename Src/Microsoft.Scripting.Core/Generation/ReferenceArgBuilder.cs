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
using System.Diagnostics;

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Generation {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using System.Diagnostics;
    using System.Collections.Generic;

    /// <summary>
    /// An argument that the user wants to explicitly pass by-reference (with copy-in copy-out semantics).
    /// The user passes a StrongBox[T] object whose value will get updated when the call returns.
    /// </summary>
    class ReferenceArgBuilder : SimpleArgBuilder {
        private Type _elementType;
        private VariableExpression _tmp;

        public ReferenceArgBuilder(int index, Type parameterType)
            : base(index, parameterType) {
            Debug.Assert(parameterType.GetGenericTypeDefinition() == typeof(StrongBox<>));
            _elementType = parameterType.GetGenericArguments()[0];
        }

        public override int Priority {
            get { return 5; }
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters) {
            if (_tmp == null) {
                _tmp = context.GetTemporary(_elementType, "outParam");
            }

            // Ideally we'd pass in Ast.ReadField(parameters[Index], "Value") but due to
            // a bug in partial trust we can't access the generic field.

            // arg is boxType ? &_tmp : throw new ArgumentTypeException()
            //   IncorrectBoxType throws the exception to avoid stack imbalance issues.
            return Ast.Condition(
                Ast.TypeIs(parameters[Index], BoxType),
                Ast.Comma(
                    Ast.Assign(
                        _tmp,
                        Ast.Call(
                            typeof(BinderOps).GetMethod("GetBox").MakeGenericMethod(_elementType),
                            Ast.ConvertHelper(parameters[Index], typeof(StrongBox<>).MakeGenericType(_elementType))
                        )
                    ),
                    Ast.Read(_tmp)
                ),
                Ast.Call(
                    typeof(BinderOps).GetMethod("IncorrectBoxType").MakeGenericMethod(_elementType),
                    Ast.ConvertHelper(parameters[Index], typeof(object))
                )
            );
        }

        protected Type BoxType {
            get {
                return this.Type;
            }
        }

        internal Type ElementType {
            get {
                return _elementType;
            }
        }

        protected virtual Expression UpdatedValue() {
            return Ast.Read(_tmp);
        }

        internal override Expression UpdateFromReturn(MethodBinderContext context, IList<Expression> parameters) {
            return Ast.Call(
                typeof(BinderOps).GetMethod("UpdateBox").MakeGenericMethod(_elementType),
                Ast.Convert(parameters[Index], BoxType),
                UpdatedValue()
            );
        }

        public override object Build(CodeContext context, object[] args) {
            object arg = args[Index];

            if (arg == null) {
                throw RuntimeHelpers.SimpleTypeError("expected StrongBox, but found null");
            }
            Type argType = arg.GetType();
            if (!argType.IsGenericType || argType.GetGenericTypeDefinition() != typeof(StrongBox<>)) {
                throw RuntimeHelpers.SimpleTypeError("expected StrongBox<>");
            }
            if (argType.GetGenericArguments()[0] != _elementType) {
                throw RuntimeHelpers.SimpleTypeError(String.Format("Expected type {0}, got {1}", typeof(StrongBox<>).MakeGenericType(_elementType).FullName, CompilerHelpers.GetType(arg).FullName));
            }

            object value = ((IStrongBox)arg).Value;

            if (value == null) return null;
            return context.LanguageContext.Binder.Convert(value, _elementType);
        }

        public override void UpdateFromReturn(object callArg, object[] args) {
            ((IStrongBox)args[Index]).Value = callArg;
        }
    }
}
