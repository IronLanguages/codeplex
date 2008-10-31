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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions.Compiler {
    internal static partial class DelegateHelpers {
        private static TypeInfo _DelegateCache = new TypeInfo();

        #region Generated Maximum Delegate Arity

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_max_delegate_arity from: generate_dynsites.py

        private const int MaximumArity = 17;

        // *** END GENERATED CODE ***

        #endregion

        private class TypeInfo {
            public Type DelegateType;
            public Dictionary<Type, TypeInfo> TypeChain;
        }

        /// <summary>
        /// Finds a delegate type for a CallSite using the types in the ReadOnlyCollection of Expression. 
        /// 
        /// We take the ROC of Expression explicitly to avoid allocating memory (an array of types) on
        /// lookup of delegate types.
        /// </summary>
        internal static Type MakeCallSiteDelegate(ReadOnlyCollection<Expression> types, Type returnType) {
            lock (_DelegateCache) {
                TypeInfo curTypeInfo = _DelegateCache;

                // CallSite
                curTypeInfo = NextTypeInfo(typeof(CallSite), curTypeInfo);

                // arguments
                for (int i = 0; i < types.Count; i++) {
                    curTypeInfo = NextTypeInfo(types[i].Type, curTypeInfo);
                }

                // return type
                curTypeInfo = NextTypeInfo(returnType, curTypeInfo);

                // see if we have the delegate already
                if (curTypeInfo.DelegateType == null) {
                    // nope, go ahead and create it and spend the
                    // cost of creating the array.
                    Type[] paramTypes = new Type[types.Count + 2];
                    paramTypes[0] = typeof(CallSite);
                    paramTypes[paramTypes.Length - 1] = returnType;
                    for (int i = 0; i < types.Count; i++) {
                        paramTypes[i + 1] = types[i].Type;
                    }

                    curTypeInfo.DelegateType = MakeDelegate(paramTypes);
                }

                return curTypeInfo.DelegateType;
            }
        }

        /// <summary>
        /// Finds a delegate type for a CallSite using the MetaObject array. 
        /// 
        /// We take the array of MetaObject explicitly to avoid allocating memory (an array of types) on
        /// lookup of delegate types.
        /// </summary>
        internal static Type MakeDeferredSiteDelegate(MetaObject[] args, Type returnType) {
            lock (_DelegateCache) {
                TypeInfo curTypeInfo = _DelegateCache;

                // CallSite
                curTypeInfo = NextTypeInfo(typeof(CallSite), curTypeInfo);

                // arguments
                for (int i = 0; i < args.Length; i++) {
                    MetaObject mo = args[i];
                    Type paramType = mo.Expression.Type;
                    if (mo.IsByRef) {
                        paramType = paramType.MakeByRefType();
                    }
                    curTypeInfo = NextTypeInfo(paramType, curTypeInfo);
                }

                // return type
                curTypeInfo = NextTypeInfo(returnType, curTypeInfo);

                // see if we have the delegate already
                if (curTypeInfo.DelegateType == null) {
                    // nope, go ahead and create it and spend the
                    // cost of creating the array.
                    Type[] paramTypes = new Type[args.Length + 2];
                    paramTypes[0] = typeof(CallSite);
                    paramTypes[paramTypes.Length - 1] = returnType;
                    for (int i = 0; i < args.Length; i++) {
                        MetaObject mo = args[i];
                        Type paramType = mo.Expression.Type;
                        if (mo.IsByRef) {
                            paramType = paramType.MakeByRefType();
                        }
                        paramTypes[i + 1] = paramType;
                    }

                    curTypeInfo.DelegateType = MakeDelegate(paramTypes);
                }

                return curTypeInfo.DelegateType;
            }
        }

        internal static Type MakeDeferredSiteDelegate(Type[] types, Type returnType) {
            lock (_DelegateCache) {
                TypeInfo curTypeInfo = _DelegateCache;

                // CallSite
                curTypeInfo = NextTypeInfo(typeof(CallSite), curTypeInfo);

                // arguments
                for (int i = 0; i < types.Length; i++) {
                    curTypeInfo = NextTypeInfo(types[i], curTypeInfo);
                }

                // return type
                curTypeInfo = NextTypeInfo(returnType, curTypeInfo);

                // see if we have the delegate already
                if (curTypeInfo.DelegateType == null) {
                    // nope, go ahead and create it and spend the
                    // cost of creating the array.
                    Type[] paramTypes = new Type[types.Length + 2];
                    paramTypes[0] = typeof(CallSite);
                    paramTypes[paramTypes.Length - 1] = returnType;
                    for (int i = 0; i < types.Length; i++) {
                        paramTypes[i + 1] = types[i];
                    }

                    curTypeInfo.DelegateType = MakeDelegate(paramTypes);
                }

                return curTypeInfo.DelegateType;
            }
        }


        private static TypeInfo NextTypeInfo(Type initialArg, TypeInfo curTypeInfo) {
            Type lookingUp = initialArg;
            TypeInfo nextTypeInfo;
            if (curTypeInfo.TypeChain == null) {
                curTypeInfo.TypeChain = new Dictionary<Type, TypeInfo>();
            }

            if (!curTypeInfo.TypeChain.TryGetValue(lookingUp, out nextTypeInfo)) {
                curTypeInfo.TypeChain[lookingUp] = nextTypeInfo = new TypeInfo();
            }
            return nextTypeInfo;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static Type MakeDelegate(Type[] types) {
            Debug.Assert(types != null && types.Length > 0);

            // Can only used predefined delegates if we have no byref types and
            // the arity is small enough to fit in Func<...> or Action<...>
            if (types.Length > MaximumArity || types.Any(t => t.IsByRef)) {
                return MakeCustomDelegate(types);
            }

            Type result;
            if (types[types.Length - 1] == typeof(void)) {
                result = GetActionType(types.RemoveLast());
            } else {
                result = GetFuncType(types);
            }
            Debug.Assert(result != null);
            return result;
        }

        internal static Type GetFuncType(Type[] types) {
            switch (types.Length) {
                #region Generated Delegate Func Types

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_delegate_func from: generate_dynsites.py

                case 1: return typeof(Func<>).MakeGenericType(types);
                case 2: return typeof(Func<,>).MakeGenericType(types);
                case 3: return typeof(Func<,,>).MakeGenericType(types);
                case 4: return typeof(Func<,,,>).MakeGenericType(types);
                case 5: return typeof(Func<,,,,>).MakeGenericType(types);
                case 6: return typeof(Func<,,,,,>).MakeGenericType(types);
                case 7: return typeof(Func<,,,,,,>).MakeGenericType(types);
                case 8: return typeof(Func<,,,,,,,>).MakeGenericType(types);
                case 9: return typeof(Func<,,,,,,,,>).MakeGenericType(types);
                case 10: return typeof(Func<,,,,,,,,,>).MakeGenericType(types);
                case 11: return typeof(Func<,,,,,,,,,,>).MakeGenericType(types);
                case 12: return typeof(Func<,,,,,,,,,,,>).MakeGenericType(types);
                case 13: return typeof(Func<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14: return typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15: return typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16: return typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 17: return typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(types);

                // *** END GENERATED CODE ***

                #endregion

                default: return null;
            }
        }

        internal static Type GetActionType(Type[] types) {
            switch (types.Length) {
                case 0: return typeof(Action);
                #region Generated Delegate Action Types

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_delegate_action from: generate_dynsites.py

                case 1: return typeof(Action<>).MakeGenericType(types);
                case 2: return typeof(Action<,>).MakeGenericType(types);
                case 3: return typeof(Action<,,>).MakeGenericType(types);
                case 4: return typeof(Action<,,,>).MakeGenericType(types);
                case 5: return typeof(Action<,,,,>).MakeGenericType(types);
                case 6: return typeof(Action<,,,,,>).MakeGenericType(types);
                case 7: return typeof(Action<,,,,,,>).MakeGenericType(types);
                case 8: return typeof(Action<,,,,,,,>).MakeGenericType(types);
                case 9: return typeof(Action<,,,,,,,,>).MakeGenericType(types);
                case 10: return typeof(Action<,,,,,,,,,>).MakeGenericType(types);
                case 11: return typeof(Action<,,,,,,,,,,>).MakeGenericType(types);
                case 12: return typeof(Action<,,,,,,,,,,,>).MakeGenericType(types);
                case 13: return typeof(Action<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14: return typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15: return typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16: return typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(types);

                // *** END GENERATED CODE ***

                #endregion

                default: return null;
            }
        }
    }
}
