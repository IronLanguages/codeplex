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
using Microsoft.Linq.Expressions.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting.Utils;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// This class is used to look for matching rules in the caches
    /// by executing individual rules against the site whose fallback
    /// code delegates here.
    /// </summary>
    internal static partial class Matchmaker {
        private static Dictionary<Type, WeakReference> _Matchmakers;

        internal static T CreateMatchMakingDelegate<T>(StrongBox<bool> box) where T : class {
            Type target = typeof(T);
            Type[] args;

            MethodInfo invoke = target.GetMethod("Invoke");

            if (DynamicSiteHelpers.SimpleSignature(invoke, out args)) {
                MethodInfo method;
                if (invoke.ReturnType == typeof(void)) {
                    method = typeof(Matchmaker).GetMethod("MismatchVoid" + args.Length, BindingFlags.NonPublic | BindingFlags.Static);
                } else {
                    method = typeof(Matchmaker).GetMethod("Mismatch" + (args.Length - 1), BindingFlags.Static | BindingFlags.NonPublic);
                }

                if (method != null) {
                    method = method.MakeGenericMethod(args);
                    return (T)(object)Delegate.CreateDelegate(target, box, method);
                }
            }

            return GetOrCreateCustomMatchmakerDelegate<T>(invoke, box);
        }

        private static T GetOrCreateCustomMatchmakerDelegate<T>(MethodInfo invoke, StrongBox<bool> box) where T : class {
            if (_Matchmakers == null) {
                Interlocked.CompareExchange<Dictionary<Type, WeakReference>>(ref _Matchmakers, new Dictionary<Type, WeakReference>(), null);
            }

            bool found;
            WeakReference wr;

            // LOCK to extract the weak reference with the updater DynamicMethod 
            lock (_Matchmakers) {
                found = _Matchmakers.TryGetValue(typeof(T), out wr);
            }

            // Extract the DynamicMethod from the WeakReference, if any
            MethodInfo target = null;
            if (found && wr != null) {
                target = wr.Target as MethodInfo;
            }

            // No target? Build new one
            if (target == null) {
                target = CreateCustomMatchmakerDelegate<T>(invoke);

                // Insert into dictionary
                lock (_Matchmakers) {
                    _Matchmakers[typeof(T)] = new WeakReference(target);
                }
            }

            return target.CreateDelegate<T>(box);
        }

        private static MethodInfo CreateCustomMatchmakerDelegate<T>(MethodInfo invoke) where T : class {
            ParameterInfo[] parameters = invoke.GetParametersCached();
            Type[] signature = new Type[parameters.Length + 1];

            signature[0] = typeof(StrongBox<bool>);
            for (int arg = 0; arg < parameters.Length; arg++) {
                signature[arg + 1] = parameters[arg].ParameterType;
            }

            DynamicILGen il = Snippets.Shared.CreateDynamicMethod("Mismatch", invoke.ReturnType, signature, false);

            il.EmitLoadArg(0);
            il.EmitBoolean(false);
            il.EmitFieldSet(typeof(StrongBox<bool>).GetField("Value", BindingFlags.Instance | BindingFlags.Public));

            if (invoke.ReturnType != typeof(void)) {
                if (invoke.ReturnType.IsValueType) {
                    LocalBuilder ret = il.DeclareLocal(invoke.ReturnType);
                    il.Emit(OpCodes.Ldloca_S, ret);
                    il.Emit(OpCodes.Initobj, invoke.ReturnType);
                    il.Emit(OpCodes.Ldloc, ret);
                } else {
                    il.EmitNull();
                }
            }
            il.Emit(OpCodes.Ret);

            return il.Finish();
        }
    }
}