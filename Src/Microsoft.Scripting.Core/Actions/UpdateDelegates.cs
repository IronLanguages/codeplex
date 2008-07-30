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

using System.Collections.Generic;
using System.Linq.Expressions.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using System.Scripting.Utils;
using System.Threading;

namespace System.Scripting.Actions {
    public static partial class UpdateDelegates {

        private static Dictionary<Type, WeakReference> _Updaters;

        internal static T MakeUpdateDelegate<T>() where T : class {
            Type target = typeof(T);
            Type[] args;
            MethodInfo invoke = target.GetMethod("Invoke");

            if (DynamicSiteHelpers.SimpleSignature(invoke, out args)) {
                MethodInfo method;
                if (invoke.ReturnType == typeof(void)) {
                    method = typeof(UpdateDelegates).GetMethod("UpdateVoid" + args.Length);
                } else {
                    method = typeof(UpdateDelegates).GetMethod("Update" + (args.Length - 1));
                }
                if (method != null) {
                    return (T)(object)Delegate.CreateDelegate(target, method.MakeGenericMethod(args.AddFirst(target)));
                }
            }

            return GetOrCreateCustomUpdateDelegate<T>(invoke);
        }

        private static T GetOrCreateCustomUpdateDelegate<T>(MethodInfo invoke) where T : class {
            if (_Updaters == null) {
                Interlocked.CompareExchange<Dictionary<Type, WeakReference>>(ref _Updaters, new Dictionary<Type, WeakReference>(), null);
            }

            bool found;
            WeakReference wr;

            // LOCK to extract the weak reference with the updater DynamicMethod 
            lock (_Updaters) {
                found = _Updaters.TryGetValue(typeof(T), out wr);
            }

            // Extract the DynamicMethod from the WeakReference, if any
            object target = null;
            if (found && wr != null) {
                target = wr.Target;
            }

            // No target? Build new one
            if (target == null) {
                target = CreateCustomUpdateDelegate<T>(invoke);

                // Insert into dictionary
                lock (_Updaters) {
                    _Updaters[typeof(T)] = new WeakReference(target);
                }
            }

            return (T)target;
        }

        private static T CreateCustomUpdateDelegate<T>(MethodInfo invoke) where T : class {
            Type siteOfT = typeof(CallSite<T>);

            ParameterInfo[] parameters = invoke.GetParameters();
            Type[] signature = parameters.Map(p => p.ParameterType);

            DynamicILGen il = DynamicSiteHelpers.CreateDynamicMethod(siteOfT.IsVisible, "Update", invoke.ReturnType, signature);
            LocalBuilder array = il.DeclareLocal(typeof(object[]));

            il.EmitLoadArg(0);
            il.Emit(OpCodes.Castclass, siteOfT);
            il.EmitInt(signature.Length - 1);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, array);

            bool byref = false;

            for (int arg = 1; arg < signature.Length; arg++) {
                il.Emit(OpCodes.Ldloc_S, array);
                il.EmitInt(arg - 1);            // index into array
                il.EmitLoadArg(arg);            // argument

                Type type = signature[arg];

                if (type.IsByRef) {
                    byref = true;
                    type = type.GetElementType();
                    il.EmitLoadValueIndirect(type);
                }

                il.EmitBoxing(type);
                il.Emit(OpCodes.Stelem_Ref);
            }

            // CallSite<T>.UpdateAndExecute(array)
            il.Emit(OpCodes.Ldloc_S, array);

            // Only if no more instructions follow after call can we do tail call
            if (invoke.ReturnType == typeof(object) && !byref) {
                il.Emit(OpCodes.Tailcall);
            }
            il.Emit(OpCodes.Call, siteOfT.GetMethod("UpdateAndExecute", BindingFlags.Instance | BindingFlags.Public));
            if (invoke.ReturnType == typeof(void)) {
                il.Emit(OpCodes.Pop);
            } else if (invoke.ReturnType != typeof(object)) {
                il.Emit(OpCodes.Unbox_Any, invoke.ReturnType);
            }

            if (byref) {
                for (int arg = 1; arg < signature.Length; arg++) {
                    Type type = signature[arg];
                    if (type.IsByRef) {
                        type = type.GetElementType();

                        il.EmitLoadArg(arg);
                        il.Emit(OpCodes.Ldloc_S, array);
                        il.EmitInt(arg - 1);
                        il.Emit(OpCodes.Ldelem_Ref);
                        if (type.IsValueType) {
                            il.Emit(OpCodes.Unbox_Any, type);
                        } else if (type != typeof(object)) {
                            il.Emit(OpCodes.Castclass, type);
                        }
                        il.EmitStoreValueIndirect(type);
                    }
                }
            }

            il.Emit(OpCodes.Ret);

            return il.Finish().CreateDelegate<T>();
        }
    }
}
