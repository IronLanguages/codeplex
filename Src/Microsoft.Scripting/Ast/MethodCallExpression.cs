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

using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class MethodCallExpression : Expression {
        readonly MethodInfo _method;
        readonly Expression _instance;
        readonly ReadOnlyCollection<Expression> _arguments;

        public MethodCallExpression(MethodInfo method, Expression instance, IList<Expression> arguments) {
            VerifyArguments(method, arguments);

            _method = method;
            _instance = instance;
            _arguments = new ReadOnlyCollection<Expression>(arguments);
        }

        public MethodCallExpression(MethodInfo method, Expression instance, IList<Expression> arguments, SourceSpan span)
            : base(span) {
            VerifyArguments(method, arguments);
            
            _method = method;
            _instance = instance;
            _arguments = new ReadOnlyCollection<Expression>(arguments);
        }

        private static void VerifyArguments(MethodInfo method, IList<Expression> arguments) {
            if (arguments == null) throw new ArgumentNullException("arguments");
            if (method == null) throw new ArgumentNullException("method");
            for (int i = 0; i < arguments.Count; i++) {
                if (arguments[i] == null) {
                    Debug.Assert(false);
                    throw new ArgumentNullException("arguments[" + i.ToString() + "]");
                }
            }
        }

        public MethodInfo Method {
            get { return _method; }
        }

        public Expression Instance {
            get { return _instance; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }

        public override Type ExpressionType {
            get {
                return _method.ReturnType;
            }
        }

        public override object Evaluate(CodeContext context) {
            object instance = _instance != null ? _instance.Evaluate(context) : null;
            object[] parameters = new object[_arguments.Count];

            for (int i = 0; i < parameters.Length; i++) {
                parameters[i] = _arguments[i] != null ? _arguments[i].Evaluate(context) : null;
            }

            return _method.Invoke(instance, parameters);
        }

        public override void Emit(CodeGen cg) {
            EmitAs(cg, typeof(object));
        }

        public override void EmitAs(CodeGen cg, Type asType) {
            EmitCall(cg);
            cg.EmitConvert(ExpressionType, asType);
        }

        private void EmitCall(CodeGen cg) {
            // Emit instance, if calling an instance method
            Slot temp = null;
            if (!_method.IsStatic) {
                if (_instance == null) {
                    throw new InvalidOperationException("Cannot emit non-static call without instance");
                } else {
                    _instance.EmitAs(cg, _method.DeclaringType);

                    if (_method.DeclaringType.IsValueType) {
                        temp = cg.GetLocalTmp(_method.DeclaringType);
                        temp.EmitSet(cg);
                        temp.EmitGetAddr(cg);
                    }
                }
            }

            // Emit arguments
            ParameterInfo[] parameters = _method.GetParameters();
            if (parameters.Length > 0) {
                int current = 0;

                // Emit all but the last directly, the last may be param array
                while (current < parameters.Length - 1) {
                    EmitArgument(cg, parameters[current], current);
                    current++;
                }

                // Emit the last argument, possible a param array
                ParameterInfo last = parameters[parameters.Length - 1];
                if (CompilerHelpers.IsParamArray(last)) {
                    Debug.Assert(last.ParameterType.HasElementType);
                    Type elementType = last.ParameterType.GetElementType();

                    // There are arguments available for emit
                    int size = 0;
                    if (_arguments.Count > parameters.Length - 1) {
                        size = _arguments.Count - parameters.Length + 1;
                    }

                    if (size == 1 && _arguments[current].ExpressionType == last.ParameterType) {
                        _arguments[current].Emit(cg);
                    } else {
                        cg.EmitInt(size);
                        cg.Emit(OpCodes.Newarr, elementType);
                        for (int i = 0; i < size; i++) {
                            cg.Emit(OpCodes.Dup);
                            cg.EmitInt(i);
                            _arguments[current + i].EmitAs(cg, elementType);
                            cg.EmitStoreElement(elementType);
                        }
                    }
                } else {
                    EmitArgument(cg, parameters[parameters.Length - 1], parameters.Length - 1);
                }
            }

            // Emit the actual call
            cg.EmitCall(_method);

            if (temp != null) {
                cg.FreeLocalTmp(temp);
            }
        }

        private void EmitArgument(CodeGen cg, ParameterInfo param, int index) {
            if (index < _arguments.Count) {
                _arguments[index].EmitAs(cg, param.ParameterType);
            } else {
                object defaultValue = param.DefaultValue;
                if (defaultValue != DBNull.Value) {
                    cg.EmitConstant(defaultValue);
                    cg.EmitConvertFromObject(param.ParameterType);
                } else {
                    cg.Context.AddError(String.Format("No value provided for the call to {0}, argument {1}", _method, index), this);
                    cg.EmitMissingValue(param.ParameterType);
                }
            }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                if (_instance != null) {
                    _instance.Walk(walker);
                }
                if (_arguments != null) {
                    foreach (Expression e in _arguments) {
                        e.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }

        #region Factory methods

        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments) {
            return Call(SourceSpan.None, instance, method, arguments);
        }

        public static MethodCallExpression Call(SourceSpan span, Expression instance, MethodInfo method, params Expression[] arguments) {
            return new MethodCallExpression(method, instance, arguments, span);
        }

        #endregion 
    }
}
