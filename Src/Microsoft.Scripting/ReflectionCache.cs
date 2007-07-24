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
using System.Text;
using System.Reflection;

namespace Microsoft.Scripting {
    /// <summary>
    /// Provides a cache of reflection members.  Only one set of values is ever handed out per a 
    /// specific request.
    /// </summary>
    class ReflectionCache {
        private static Dictionary<MethodBaseCache, BuiltinFunction> _functions = new Dictionary<MethodBaseCache,BuiltinFunction>();

        /// <summary>
        /// Gets a singleton method group from the provided type.
        /// 
        /// The provided method group will be unique based upon the methods defined, not based upon the type/name
        /// combination.  In other words calling GetMethodGroup on a base type and a derived type that introduces
        /// no new methods under a given name will result in the same method group for both types.
        /// </summary>
        public static BuiltinFunction GetMethodGroup(Type type, string name) {
            if (type == null) throw new ArgumentNullException("type");
            if (name == null) throw new ArgumentNullException("name");

            BuiltinFunction res = null;

            MemberInfo[] mems = type.FindMembers(MemberTypes.Method,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.InvokeMethod,
                delegate(MemberInfo mem, object filterCritera) {
                    return mem.Name == name;
                },
                null);

            MethodBase[] bases = Utils.Array.ConvertAll<MemberInfo, MethodBase>(
                mems,
                delegate(MemberInfo inp) { return (MethodBase)inp; });

            if (mems.Length != 0) {
                MethodBaseCache cache = new MethodBaseCache(bases);
                lock (_functions) {
                    if (!_functions.TryGetValue(cache, out res)) {                       
                        _functions[cache] = res = BuiltinFunction.MakeMethod(
                            name,
                            bases,
                            GetMethodFunctionType(bases));
                    }
                }
            }                

            return res;
        }

        private static FunctionType GetMethodFunctionType(MethodBase[] methods) {
            FunctionType ft = FunctionType.None;
            foreach (MethodInfo mi in methods) {
                if (methods[0].IsStatic) ft |= FunctionType.Function;
                else ft |= FunctionType.Method;
            }

            return ft;
        }

        private class MethodBaseCache {
            private MethodBase[] _members;

            public MethodBaseCache(MethodBase [] members) {
                // sort by token so that the Equals / GetHashCode doesn't have
                // to line up members if reflection returns them in different orders.
                Array.Sort<MethodBase>(members, delegate(MethodBase x, MethodBase y) {
                    long res = x.MethodHandle.Value.ToInt64() - y.MethodHandle.Value.ToInt64();
                    if (res == 0) return 0;
                    if (res < 0) return -1;
                    return 1;
                });
                _members = members;
            }            

            public override bool Equals(object obj) {
                MethodBaseCache other = obj as MethodBaseCache;
                if (other == null || _members.Length != other._members.Length) return false;

                for (int i = 0; i < _members.Length; i++) {
                    if (_members[i].DeclaringType != other._members[i].DeclaringType ||
                        _members[i].MetadataToken != other._members[i].MetadataToken ||
                        _members[i].IsGenericMethod != other._members[i].IsGenericMethod) {
                        return false;
                    }

                    if (_members[i].IsGenericMethod) {
                        Type[] args = _members[i].GetGenericArguments();
                        Type[] otherArgs = other._members[i].GetGenericArguments();

                        if (args.Length != otherArgs.Length) {
                            return false;
                        }

                        for (int j = 0; j < args.Length; j++) {
                            if (args[j] != otherArgs[j]) return false;
                        }
                    }
                }

                return true;
            }

            public override int GetHashCode() {
                int res = 6551;
                foreach (MemberInfo mi in _members) {
                    res ^= res << 5 ^ mi.DeclaringType.GetHashCode() ^ mi.MetadataToken;

                }
                return res;
            }
        }
    }
}
