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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Interpreter {
    /// <summary>
    /// Interpreter partial class. This part contains interpretation code for lambdas.
    /// </summary>
    internal static partial class Interpreter {
        private static object DoExecute(CodeContext context, LambdaExpression lambda) {
            context.Scope.SourceLocation = lambda.Start;
            object ret = Interpreter.Interpret(context, lambda.Body);

            ControlFlow cf = ret as ControlFlow;
            if (cf != null) {
                return cf.Value;
            } else {
                return null;
            }
        }

        private static object Execute(CodeContext context, LambdaExpression lambda) {
            try {
                return DoExecute(context, lambda);
            } catch (Exception e) {
#if !SILVERLIGHT
                MethodBase method = MethodBase.GetCurrentMethod();
#else
                MethodBase method = null;
#endif
                SourceUnit sourceUnit = context.ModuleContext.CompilerContext.SourceUnit;
                int line = context.Scope.SourceLocation.Line;

                ExceptionHelpers.UpdateStackTrace(context, method, lambda.Name, sourceUnit.GetSymbolDocument(line), sourceUnit.MapLine(line));
                ExceptionHelpers.AssociateDynamicStackFrames(e);
                throw ExceptionHelpers.UpdateForRethrow(e);
            }
        }

        /// <summary>
        /// Called by the code:LambdaInvoker.Invoke from the delegate generated below by
        /// code:GetDelegateForInterpreter.
        /// 
        /// This method must repackage arguments to match the lambdas signature, which
        /// may mean repackaging the parameter arrays.
        /// 
        /// Input are two arrays - regular arguments passed into the generated delegate,
        /// and (if the delegate had params array), the parameter array, separately.
        /// </summary>
        internal static object InterpretLambda(CodeContext/*!*/ parent, LambdaExpression/*!*/ lambda, object[]/*!*/ args, object[] array) {
            Contract.Requires(parent != null, "parent");
            Contract.Requires(lambda != null, "lambda");
            Contract.Requires(args != null, "args");        // was allocated by the generated delegate

            CodeContext child = RuntimeHelpers.CreateNestedCodeContext(new SymbolDictionary(), parent, lambda.IsVisible);

            // First load from args.
            object[] input = args;
            int inputIndex = 0;

            //
            // Populate all parameters ...
            //
            for (int i = 0; i < lambda.Parameters.Count; i++) {
                //
                // Need to switch to the other array?
                //
                if (input != null && inputIndex >= input.Length) {
                    //
                    // If still loading from args, we can switch to array, otherwise we are done.
                    //
                    input = ((object)input == (object)args) ? array : null;
                    inputIndex = 0;
                }

                object value = (input != null && inputIndex < input.Length) ? input[inputIndex++] : Uninitialized.Instance;
                RuntimeHelpers.SetName(child, VariableInfo.GetName(lambda.Parameters[i]), value);
            }

            return Execute(child, lambda);
        }

        private static int _Interpreted;

        private static Dictionary<LambdaExpression, MethodInfo> _Delegates;

        /// <summary>
        /// Gets the delegate associated with the LambdaExpression.
        /// Either it uses cached MethodInfo and creates delegate from it, or it will generate
        /// completely new dynamic method, store it in a cache and use it to create the delegate.
        /// </summary>
        private static Delegate GetDelegateForInterpreter(CodeContext context, LambdaExpression node) {
            if (_Delegates == null) {
                Interlocked.CompareExchange<Dictionary<LambdaExpression, MethodInfo>>(
                    ref _Delegates,
                    new Dictionary<LambdaExpression, MethodInfo>(),
                    null
                );
            }

            bool found = false;
            MethodInfo method = null;

            //
            // LOCK to find the MethodInfo
            //

            lock (_Delegates) {
                found = _Delegates.TryGetValue(node, out method);
            }

            if (! found || method == null) {
                method = CreateDelegateForInterpreter(node, node.Type);

                //
                // LOCK to store the MethodInfo
                // (and maybe find one added while we were creating new delegate, in which case
                // throw away the new one and use the one from the cache.
                //

                lock (_Delegates) {
                    MethodInfo conflict;
                    if (!_Delegates.TryGetValue(node, out conflict)) {
                        _Delegates.Add(node, method);
                    } else {
                        method = conflict;
                    }
                }
            }

            return ReflectionUtils.CreateDelegate(method, node.Type, new LambdaInvoker(node, context));
        }

        /// <summary>
        /// The core of the interpreter, calling back onto itself via delegates.
        /// </summary>
        private static MethodInfo CreateDelegateForInterpreter(LambdaExpression lambda, Type type) {
            Debug.Assert(type != typeof(Delegate) && typeof(Delegate).IsAssignableFrom(type));

            //
            // Get the desired signature
            //
            MethodInfo invoke = type.GetMethod("Invoke");
            ParameterInfo[] parameters = invoke.GetParameters();

            string name = "Interpreted_" + lambda.Name + "_" + Interlocked.Increment(ref _Interpreted);

            Type [] signature = CreateInterpreterSignature(parameters);
            DynamicILGen il = Snippets.Shared.CreateDynamicMethod(name, invoke.ReturnType, signature, false);

            // Collect all arguments received by the delegate into an array
            // and pass them to the Interpreter along with the LambdaInvoker

            // LambdaInvoker
            il.EmitLoadArg(0);

            //
            // If the delegate takes parameter array, pass it separately
            //

            bool array = parameters.Length > 0 && CompilerHelpers.IsParamArray(parameters[parameters.Length - 1]);

            int count = parameters.Length;
            if (array) count--;

            // Create the array
            il.EmitInt(count);
            il.Emit(OpCodes.Newarr, typeof(object));
            for (int i = 0; i < count; i++) {
                il.Emit(OpCodes.Dup);
                il.EmitInt(i);
                il.EmitLoadArg(i + 1);
                EmitExplicitCast(il, parameters[i].ParameterType, typeof(object));
                il.EmitStoreElement(typeof(object));
            }

            if (array) {
                il.EmitLoadArg(parameters.Length);
                EmitExplicitCast(il, parameters[parameters.Length - 1].ParameterType, typeof(object[]));
            } else {
                il.EmitNull();
            }

            // Call back to interpreter
            il.EmitCall(typeof(LambdaInvoker).GetMethod("Invoke"));

            // Cast back to the delegate return type
            EmitExplicitCast(il, typeof(object), invoke.ReturnType);

            // And return whatever the result was.
            il.Emit(OpCodes.Ret);

            //
            // We are done (for now), finish the MethodInfo
            //
            return il.Finish();
        }

        private static void EmitExplicitCast(ILGen il, Type from, Type to) {
            if (!il.TryEmitExplicitCast(from, to)) {
                throw new ArgumentException(String.Format("Cannot cast from '{0}' to '{1}'", from, to));
            }
        }

        private static Type[] CreateInterpreterSignature(ParameterInfo[] parameters) {
            Type[] signature = new Type[parameters.Length + 1];

            // First one is always LambdaInvoker.
            signature[0] = typeof(LambdaInvoker);

            // The rest is copied from the parameter infos.
            for (int i = 0; i < parameters.Length; i++) {
                signature[i + 1] = parameters[i].ParameterType;
            }

            return signature;
        }
    }
}
