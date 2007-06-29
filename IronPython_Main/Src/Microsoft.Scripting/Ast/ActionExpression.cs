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
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class ActionExpression : Expression {
        private readonly IList<Expression> _arguments;
        private readonly Action _action;
        private readonly Type _result;

        internal ActionExpression(SourceSpan span, Action action, IList<Expression> arguments, Type result)
            : base(span) {
            if (action == null) throw new ArgumentNullException("action");
            if (arguments == null) throw new ArgumentNullException("arguments");
            if (result == null) throw new ArgumentNullException("result");

            _action = action;
            _arguments = arguments;
            _result = result;
        }

        public Action Action {
            get { return _action; }
        }

        public IList<Expression> Arguments {
            get { return _arguments; }
        }

        public override Type ExpressionType {
            get {
                return _result;
            }
        }

        public override object Evaluate(CodeContext context) {
            return context.LanguageContext.Binder.Execute(context, _action, Evaluate(_arguments, context));
        }

        public override AbstractValue AbstractEvaluate(AbstractContext context) {
            List<AbstractValue> values = new List<AbstractValue>();
            foreach (Expression arg in _arguments) {
                values.Add(arg.AbstractEvaluate(context));
            }

            return context.Binder.AbstractExecute(_action, values);
        }

        private Type[] GetSiteTypes() {
            Type[] ret = new Type[_arguments.Count + 1];
            for (int i = 0; i < _arguments.Count; i++) {
                ret[i] = _arguments[i].ExpressionType;
            }
            ret[_arguments.Count] = _result;
            return ret;
        }

        // Action expression is different in that it mutates its
        // ExpressionType based on the need of the outer codegen.
        // Therefore, unless asked explicitly, it will emit itself as object.
        public override void Emit(CodeGen cg) {
            bool fast;
            Slot site = cg.CreateDynamicSite(_action, GetSiteTypes(), out fast);
            MethodInfo method = site.Type.GetMethod("Invoke");

            Debug.Assert(!method.IsStatic);

            // Emit "this" - the site
            site.EmitGet(cg);
            ParameterInfo[] parameters = method.GetParameters();

            int first = 0;

            // Emit code context for unoptimized sites only
            if (!fast) {
                Debug.Assert(parameters[0].ParameterType == typeof(CodeContext));

                cg.EmitCodeContext();

                // skip the CodeContext parameter
                first = 1;
            }

            if (parameters.Length < _arguments.Count + first) {
                // tuple parameters
                Debug.Assert(parameters.Length == first + 1);

                CreateTupleInstance(cg, site.Type.GetGenericArguments()[0], 0, _arguments.Count);
            } else {
                // Emit the arguments
                for (int arg = 0; arg < _arguments.Count; arg++) {
                    Debug.Assert(parameters[arg + first].ParameterType == _arguments[arg].ExpressionType);
                    _arguments[arg].Emit(cg);
                }
            }


            // Emit the site invoke
            cg.EmitCall(site.Type, "Invoke");
        }

        private void CreateTupleInstance(CodeGen cg, Type tupleType, int start, int end) {
            int size = end - start;

            if (size > NewTuple.MaxSize) {
                int multiplier = 1;
                while (size > NewTuple.MaxSize) {
                    size = (size + NewTuple.MaxSize - 1) / NewTuple.MaxSize;
                    multiplier *= NewTuple.MaxSize;
                }
                for (int i = 0; i < size; i++) {
                    int newStart = start + (i * multiplier);
                    int newEnd = System.Math.Min(end, start + ((i + 1) * multiplier));

                    PropertyInfo pi = tupleType.GetProperty("Item" + String.Format("{0:D3}", i));
                    Debug.Assert(pi != null);
                    CreateTupleInstance(cg, pi.PropertyType, newStart, newEnd);
                }
            } else {
                for (int i = start; i < end; i++) {
                    _arguments[i].Emit(cg);
                }
            }

            // fill in emptys with null.
            Type[] genArgs = tupleType.GetGenericArguments();
            for (int i = size; i < genArgs.Length; i++) {
                cg.EmitNull();
            }
           
            EmitTupleNew(cg, tupleType);
        }

        private static void EmitTupleNew(CodeGen cg, Type tupleType) {
            ConstructorInfo[] cis = tupleType.GetConstructors();
            foreach (ConstructorInfo ci in cis) {
                if (ci.GetParameters().Length != 0) {
                    cg.EmitNew(ci);
                    break;
                }
            }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                foreach (Expression ex in _arguments) {
                    ex.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        public static class Action {
            /// <summary>
            /// Creates ActionExpression representing DoOperationAction.
            /// </summary>
            /// <param name="op">The operation to perform</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static ActionExpression Operator(Operators op, Type result, params Expression[] arguments) {
                return Operator(SourceSpan.None, op, result, arguments);
            }

            /// <summary>
            /// Creates ActionExpression representing DoOperationAction.
            /// </summary>
            /// <param name="span">SourceSpan to associate with the expression</param>
            /// <param name="op">The operation to perform</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static ActionExpression Operator(SourceSpan span, Operators op, Type result, params Expression[] arguments) {
                return new ActionExpression(span, DoOperationAction.Make(op), arguments, result);
            }

            /// <summary>
            /// Creates ActionExpression representing a GetMember action.
            /// </summary>
            /// <param name="name">The qualifier.</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static ActionExpression GetMember(SymbolId name, Type result, params Expression[] arguments) {
                return GetMember(SourceSpan.None, name, result, arguments);
            }

            /// <summary>
            /// Creates ActionExpression representing a GetMember action.
            /// </summary>
            /// <param name="span">SourceSpan to associate with the expression</param>
            /// <param name="name">The qualifier.</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static ActionExpression GetMember(SourceSpan span, SymbolId name, Type result, params Expression[] arguments) {
                return new ActionExpression(span, GetMemberAction.Make(name), arguments, result);
            }

            /// <summary>
            /// Creates ActionExpression representing a SetMember action.
            /// </summary>
            /// <param name="name">The qualifier.</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static ActionExpression SetMember(SymbolId name, Type result, params Expression[] arguments) {
                return SetMember(SourceSpan.None, name, result, arguments);
            }

            /// <summary>
            /// Creates ActionExpression representing a SetMember action.
            /// </summary>
            /// <param name="span">SourceSpan to associate with the expression</param>
            /// <param name="name">The qualifier.</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static ActionExpression SetMember(SourceSpan span, SymbolId name, Type result, params Expression[] arguments) {
                return new ActionExpression(span, SetMemberAction.Make(name), arguments, result);
            }

            /// <summary>
            /// Creates ActionExpression representing a Call action.
            /// </summary>
            /// <param name="action">The call action to perform.</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static ActionExpression Call(CallAction action, Type result, params Expression[] arguments) {
                return Call(SourceSpan.None, action, result, arguments);
            }

            /// <summary>
            /// Creates ActionExpression representing a Call action.
            /// </summary>
            /// <param name="span">SourceSpan to associate with the expression</param>
            /// <param name="action">The call action to perform.</param>
            /// <param name="result">Type of the result desired (The ActionExpression is strongly typed)</param>
            /// <param name="arguments">Array of arguments for the action expression</param>
            /// <returns>New instance of the ActionExpression</returns>
            public static ActionExpression Call(SourceSpan span, CallAction action, Type result, params Expression[] arguments) {
                return new ActionExpression(span, action, arguments, result);
            }
        }
    }
}
