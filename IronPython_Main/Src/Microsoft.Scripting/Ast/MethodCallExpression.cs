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
    public class MethodCallExpression : Expression {
        readonly MethodInfo _method;
        readonly Expression _instance;
        readonly ReadOnlyCollection<Expression> _arguments;
        readonly ParameterInfo[] _pi;

        internal MethodCallExpression(SourceSpan span, MethodInfo method, Expression instance, IList<Expression> arguments)
            : base(span) {

            VerifyArguments(method, arguments);

            _method = method;
            _instance = instance;
            _arguments = new ReadOnlyCollection<Expression>(arguments);
            _pi = _method.GetParameters();
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

        // Verify that _instance is non-null. Only call this on non-static methods.
        private void VerifyNonNull() {
            Debug.Assert(!Method.IsStatic);
            if (_instance == null) {
                throw new InvalidOperationException("Cannot emit non-static call without instance");
            }
        }

        public override object Evaluate(CodeContext context) {
            object instance = null;
            // Evaluate the instance first (if the method is non-static)
            if (!Method.IsStatic) {
                VerifyNonNull();
                instance = _instance.Evaluate(context);
            }

            // box "this" if it is a value type (in case _method tries to modify it)
            // -- this keeps the same semantics as Emit().
            if (_method.DeclaringType.IsValueType)
                instance = System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(instance);

            object[] parameters = new object[_pi.Length];
            if (_pi.Length > 0) {
                int last = parameters.Length - 1;
                for (int i = 0; i < last; i++) {
                    parameters[i] = _arguments[i] != null ? _arguments[i].Evaluate(context) : null;
                }
                
                // If the last parameter is a parameter array, throw the extra arguments into an array
                int extraArgs = _arguments.Count - last;
                if (CompilerHelpers.IsParamArray(_pi[last])) {
                    if (extraArgs == 1 && _arguments[last] != null
                        && _arguments[last].ExpressionType == _pi[last].ParameterType) {
                        // If the last argument is an array, copy it over directly
                        parameters[last] = _arguments[last].Evaluate(context);
                    } else {
                        object[] varargs = new object[_arguments.Count - last];
                        for (int i = last; i < _arguments.Count; i++) {
                            varargs[i - last] = _arguments[i] != null ? _arguments[i].Evaluate(context) : null;
                        }
                        parameters[last] = varargs;
                    }
                } else {
                    parameters[last] = _arguments[last] != null ? _arguments[last].Evaluate(context) : null;
                }
            }

            try {
                return _method.Invoke(instance, parameters);
            } catch (TargetInvocationException e) {
                // Unwrap the real (inner) exception and raise it
                throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
            }
        }

        public override void Emit(CodeGen cg) {
            // Emit instance, if calling an instance method
            Slot temp = null;
            if (!_method.IsStatic) {
                VerifyNonNull();

                _instance.EmitAs(cg, _method.DeclaringType);

                if (_method.DeclaringType.IsValueType) {
                    // _method expects a byref as "this", so unbox _instance and get a pointer
                    // to its location on the stack
                    // Note: this makes a copy, which is problematic if _method is a mutating method
                    temp = cg.GetLocalTmp(_method.DeclaringType);
                    temp.EmitSet(cg);
                    temp.EmitGetAddr(cg);
                }
            }

            // Emit arguments
            if (_pi.Length > 0) {
                int current = 0;

                // Emit all but the last directly, the last may be param array
                while (current < _pi.Length - 1) {
                    EmitArgument(cg, _pi[current], current);
                    current++;
                }

                // Emit the last argument, possible a param array
                ParameterInfo last = _pi[_pi.Length - 1];
                if (CompilerHelpers.IsParamArray(last)) {
                    Debug.Assert(last.ParameterType.HasElementType);
                    Type elementType = last.ParameterType.GetElementType();

                    // There are arguments available for emit
                    int size = 0;
                    if (_arguments.Count > _pi.Length - 1) {
                        size = _arguments.Count - _pi.Length + 1;
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
                    EmitArgument(cg, _pi[_pi.Length - 1], _pi.Length - 1);
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
                    cg.EmitConvert(defaultValue != null ? defaultValue.GetType() : typeof(object), param.ParameterType);
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
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments) {
            return Call(SourceSpan.None, instance, method, arguments);
        }

        public static MethodCallExpression Call(SourceSpan span, Expression instance, MethodInfo method, params Expression[] arguments) {
            return new MethodCallExpression(span, method, instance, arguments);
        }
    }
}
