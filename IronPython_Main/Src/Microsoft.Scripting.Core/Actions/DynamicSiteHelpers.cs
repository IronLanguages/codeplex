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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Scripting.Generation;
using System.Scripting.Utils;
using System.Threading;

namespace System.Scripting.Actions {
    public static partial class DynamicSiteHelpers {
        private static readonly Dictionary<Type, CreateSite> _siteCtors = new Dictionary<Type, CreateSite>();

        private delegate object CreateSite(CallSiteBinder binder);

        public static CallSite MakeSite(CallSiteBinder binder, Type siteType) {
            CreateSite ctor;
            lock (_siteCtors) {
                if (!_siteCtors.TryGetValue(siteType, out ctor)) {
                    _siteCtors[siteType] = ctor = (CreateSite)Delegate.CreateDelegate(typeof(CreateSite), siteType.GetMethod("Create"));
                }
            }

            return (CallSite)ctor(binder);
        }

        //
        // Initialization of dynamic sites stored in static fields 
        //

        public static void InitializeFields(Type type) {
            InitializeFields(type, false);
        }

        public static void InitializeFields(Type type, bool reusable) {
            if (type == null) return;

            const string slotStorageName = "#Constant";
            foreach (FieldInfo fi in type.GetFields()) {
                if (fi.Name.StartsWith(slotStorageName)) {
                    object value;
                    if (reusable) {
                        value = GlobalStaticFieldRewriter.GetConstantDataReusable(Int32.Parse(fi.Name.Substring(slotStorageName.Length)));
                    } else {
                        value = GlobalStaticFieldRewriter.GetConstantData(Int32.Parse(fi.Name.Substring(slotStorageName.Length)));
                    }
                    Debug.Assert(value != null);
                    fi.SetValue(null, value);
                }
            }
        }

        internal static bool SimpleSignature(MethodInfo invoke, out Type[] sig) {
            ParameterInfo[] pis = invoke.GetParameters();
            ContractUtils.Requires(pis.Length > 0 && pis[0].ParameterType == typeof(CallSite), "T");

            Type[] args = new Type[invoke.ReturnType != typeof(void) ? pis.Length : pis.Length - 1];
            bool supported = true;

            for (int i = 1; i < pis.Length; i++) {
                ParameterInfo pi = pis[i];
                if (CompilerHelpers.IsByRefParameter(pi)) {
                    supported = false;
                }
                args[i - 1] = pi.ParameterType;
            }
            if (invoke.ReturnType != typeof(void)) {
                args[args.Length - 1] = invoke.ReturnType;
            }
            sig = args;
            return supported;
        }

        private static Dictionary<Signature, Type> _DynamicSiteTargets;

        private class Signature {
            private readonly int _hash;
            private readonly Type[] _types;

            internal Signature(Type[] types) {
                _types = types;
                _hash = CalculateHash(types);
            }

            public override int GetHashCode() {
                return _hash;
            }

            public override bool Equals(object obj) {
                Signature sig = obj as Signature;
                if (sig == null) {
                    return false;
                }
                if (sig._hash != _hash) {
                    return false;
                }
                if (sig._types.Length != _types.Length) {
                    return false;
                }
                for (int i = 0; i < _types.Length; i++) {
                    if (sig._types[i] != _types[i]) {
                        return false;
                    }
                }

                return true;
            }

            private static int CalculateHash(Type[] types) {
                int hash = 0;
                for (int i = 0; i < types.Length; i++) {
                    hash ^= types[i].GetHashCode();
                }
                return hash;
            }
        }

        private static Type MakeBigSiteTargetType(Type[] types) {
            if (_DynamicSiteTargets == null) {
                Interlocked.CompareExchange<Dictionary<Signature, Type>>(ref _DynamicSiteTargets, new Dictionary<Signature, Type>(), null);
            }

            Signature sig = new Signature(ArrayUtils.Copy(types));

            bool found;
            Type type;

            //
            // LOCK to retrieve the delegate type, if any
            //

            lock (_DynamicSiteTargets) {
                found = _DynamicSiteTargets.TryGetValue(sig, out type);
            }

            if (!found && type != null) {
                return type;
            }

            //
            // Create new delegate type
            //

            type = MakeNewBigSiteTargetType(types);

            //
            // LOCK to insert new delegate into the cache. If we already have one (racing threads), use the one from the cache
            //

            lock (_DynamicSiteTargets) {
                Type conflict;
                if (_DynamicSiteTargets.TryGetValue(sig, out conflict) && conflict != null) {
                    type = conflict;
                } else {
                    _DynamicSiteTargets[sig] = type;
                }
            }

            return type;
        }

        private const MethodAttributes CtorAttributes = MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public;
        private const MethodImplAttributes ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed;
        private const MethodAttributes InvokeAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

        private static readonly Type[] _DelegateCtorSignature = new Type[] { typeof(object), typeof(IntPtr) };

        private static Type MakeNewBigSiteTargetType(Type[] types) {
            Type returnType = types[types.Length - 1];
            Type[] parameters = ArrayUtils.RotateRight(types, 1);
            parameters[0] = typeof(CallSite);

            TypeBuilder builder = Snippets.Shared.DefineDelegateType("CallSiteTarget" + types.Length);
            builder.DefineConstructor(CtorAttributes, CallingConventions.Standard, _DelegateCtorSignature).SetImplementationFlags(ImplAttributes);
            builder.DefineMethod("Invoke", InvokeAttributes, returnType, parameters).SetImplementationFlags(ImplAttributes);
            return builder.CreateType();
        }

        /// <summary>
        /// Dynamic code generation required by dynamic sites needs to be able to call the delegate by which the
        /// call site is parametrized. If the delegate type is visible, we can generate into assembly (if saving
        /// assemblies). With delegate types that are not visible we must generate LCG in order to skip visibility.
        /// </summary>
        internal static DynamicILGen CreateDynamicMethod(bool visible, string name, Type returnType, Type[] parameters) {
            if (visible) {
                return Snippets.Shared.CreateDynamicMethod(name, returnType, parameters, false);
            } else {
                DynamicMethod dm = ReflectionUtils.CreateDynamicMethod(name, returnType, parameters);
                return new DynamicILGenMethod(dm, dm.GetILGenerator());
            }
        }
    }
}
