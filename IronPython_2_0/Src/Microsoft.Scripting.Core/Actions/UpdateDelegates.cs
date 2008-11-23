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
using System.Diagnostics;
using Microsoft.Linq.Expressions.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting.Utils;
using System.Threading;

namespace Microsoft.Scripting.Actions {
    internal static partial class UpdateDelegates {
        private static Dictionary<Type, WeakReference> _Updaters;

        internal static T MakeUpdateDelegate<T>() where T : class {
            if (_Updaters == null) {
                Interlocked.CompareExchange<Dictionary<Type, WeakReference>>(ref _Updaters, new Dictionary<Type, WeakReference>(), null);
            }

            Type target = typeof(T);
            lock (_Updaters) {
                WeakReference wr;
                T ret = null;
                if (!_Updaters.TryGetValue(target, out wr) || ((ret = (T)wr.Target) == null)) {
                    Type[] args;
                    MethodInfo invoke = target.GetMethod("Invoke");

                    MethodInfo method = null;
                    if (DynamicSiteHelpers.SimpleSignature(invoke, out args)) {
                        if (invoke.ReturnType == typeof(void)) {
                            method = typeof(UpdateDelegates).GetMethod("UpdateVoid" + args.Length, BindingFlags.NonPublic | BindingFlags.Static);
                        } else {
                            method = typeof(UpdateDelegates).GetMethod("Update" + (args.Length - 1), BindingFlags.NonPublic | BindingFlags.Static);
                        }
                        if (method != null) {
                            ret = (T)(object)Delegate.CreateDelegate(target, method.MakeGenericMethod(args.AddFirst(target)));
                        }
                    }

                    if (method == null) {
                        ret = CreateCustomUpdateDelegate<T>(invoke);
                    }

                    Debug.Assert(ret != null);
                    _Updaters[target] = new WeakReference(ret);
                }
                return ret;
            }
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
