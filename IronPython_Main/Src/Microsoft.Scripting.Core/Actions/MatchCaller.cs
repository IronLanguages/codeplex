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
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    internal delegate object MatchCallerTarget<T>(T target, CallSite site, object[] args);

    /// <summary>
    /// MatchCaller allows to call match maker delegate with the signature (object, CallSite, object[])
    /// It is used by the call site cache lookup logic when searching for applicable rule.
    /// </summary>
    internal static partial class MatchCaller {
        private struct RefFixer {
            internal readonly LocalBuilder Temp;
            internal readonly int Index;

            internal RefFixer(LocalBuilder temp, int index) {
                Temp = temp;
                Index = index;
            }
        }

        private static readonly CacheDict<Type, Delegate> _Callers = new CacheDict<Type, Delegate>(200);

        internal static MatchCallerTarget<T> MakeCaller<T>() {
            Type target = typeof(T);
            Type[] args;
            MethodInfo invoke = target.GetMethod("Invoke");

            // TODO: faster way to test if target is a Func<...> or Action<...>
            if (target.IsGenericType && DynamicSiteHelpers.SimpleSignature(invoke, out args)) {
                MethodInfo method;
                if (invoke.ReturnType == typeof(void)) {
                    method = typeof(MatchCaller).GetMethod("CallVoid" + args.Length, BindingFlags.NonPublic | BindingFlags.Static);
                } else {
                    method = typeof(MatchCaller).GetMethod("Call" + (args.Length - 1), BindingFlags.NonPublic | BindingFlags.Static);
                }
                if (method != null) {
                    method = method.MakeGenericMethod(args);
                    if (method.GetParametersCached()[0].ParameterType == target) {
                        return method.CreateDelegate<MatchCallerTarget<T>>();
                    }
                }
            }

            return GetOrCreateCustomCaller<T>();
        }

        private static MatchCallerTarget<T> GetOrCreateCustomCaller<T>() {
            bool found;
            Delegate target = null;
            Type type = typeof(T);

            // LOCK to extract the weak reference with the updater DynamicMethod 
            lock (_Callers) {
                found = _Callers.TryGetValue(type, out target);
            }

            // No target? Build new one
            if (target == null) {
                target = CreateCustomCaller<T>();

                // Insert into dictionary
                lock (_Callers) {
                    _Callers[type] = target;
                }
            }

            return (MatchCallerTarget<T>)target;
        }

        /// <summary>
        /// Uses LCG to create method such as this:
        /// 
        /// object MatchCaller(ActualDelegateType target, CallSite site, object[] args) {
        ///      return (object)target(site, (T0)args[0], (T1)args[1], (T2)args[2], ...);
        /// }
        /// 
        /// inserting appropriate casts and boxings as needed.
        /// </summary>
        /// <returns>A MatchCallerTarget delegate.</returns>
        private static Delegate CreateCustomCaller<T>() {
            Type type = typeof(T);
            MethodInfo invoke = type.GetMethod("Invoke");
            ParameterInfo[] parameters = invoke.GetParametersCached();
            DynamicILGen il = DynamicSiteHelpers.CreateDynamicMethod(
                type.IsVisible,
                "_stub_MatchCaller",
                typeof(object),
                new[] { type, typeof(CallSite), typeof(object[]) }
            );

            List<RefFixer> fixers = null;

            // Emit delegate
            il.EmitLoadArg(0);
    
            // CallSite
            il.EmitLoadArg(1);

            // Arguments
            for (int i = 1; i < parameters.Length; i++) {
                il.EmitLoadArg(2);
                il.EmitInt(i - 1);
                il.Emit(OpCodes.Ldelem_Ref);
                Type pt = parameters[i].ParameterType;
                if (pt.IsByRef) {
                    RefFixer rf = new RefFixer(il.DeclareLocal(pt.GetElementType()), i - 1);
                    if (rf.Temp.LocalType.IsValueType) {
                        il.Emit(OpCodes.Unbox_Any, rf.Temp.LocalType);
                    } else if (rf.Temp.LocalType != typeof(object)) {
                        il.Emit(OpCodes.Castclass, rf.Temp.LocalType);
                    }
                    il.Emit(OpCodes.Stloc, rf.Temp);
                    il.Emit(OpCodes.Ldloca, rf.Temp);

                    if (fixers == null) {
                        fixers = new List<RefFixer>();
                    }
                    fixers.Add(rf);
                } else if (pt.IsValueType) {
                    il.Emit(OpCodes.Unbox_Any, pt);
                } else if (pt != typeof(object)) {
                    il.Emit(OpCodes.Castclass, pt);
                }
            }

            // Call the delegate
            il.Emit(OpCodes.Callvirt, invoke);

            // Propagate the ref parameters back into the array.
            if (fixers != null) {
                //
                // Only write-back if the rule matched. We check this by checking the
                // matchmaker directly (it is bound to the update delegate of the call site)
                //
                Type siteType = typeof(CallSite<>).MakeGenericType(type);
                Label nomatch = il.DefineLabel();

                il.EmitLoadArg(1);
                il.Emit(OpCodes.Castclass, siteType);
                il.Emit(OpCodes.Ldfld, siteType.GetField("Update"));
                il.EmitCall(typeof(RuntimeOps).GetMethod("RuleMatched"));
                il.Emit(OpCodes.Brfalse, nomatch);

                foreach (RefFixer rf in fixers) {
                    il.EmitLoadArg(2);
                    il.EmitInt(rf.Index);
                    il.Emit(OpCodes.Ldloc, rf.Temp);
                    if (rf.Temp.LocalType.IsValueType) {
                        il.Emit(OpCodes.Box, rf.Temp.LocalType);
                    }
                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.MarkLabel(nomatch);
            }

            // Return value
            if (invoke.ReturnType == typeof(void)) {
                il.Emit(OpCodes.Ldnull);
            } else if (invoke.ReturnType.IsValueType) {
                il.Emit(OpCodes.Box, invoke.ReturnType);
            }

            il.Emit(OpCodes.Ret);

            return il.CreateDelegate<MatchCallerTarget<T>>();
        }
    }
}

namespace Microsoft.Runtime.CompilerServices {
    public static partial class RuntimeOps {
        /// <summary>
        /// Called by generated code.
        /// </summary>
        [Obsolete("Do not call this method.")]
        public static bool RuleMatched(Delegate d) {
            //
            // The "Matchmaker" delegate is closed over a StrongBox<bool>,
            // which is updated should the rule not match. If the rule matched,
            // we detect it here and the ref arguments will get propagated to
            // the argument array by the MatchCaller
            //
            StrongBox<bool> mm;
            if (d != null && (mm = d.Target as StrongBox<bool>) != null) {
                return mm.Value;
            }

            throw Microsoft.Linq.Expressions.Error.BadDelegateData();
        }
    }
}
