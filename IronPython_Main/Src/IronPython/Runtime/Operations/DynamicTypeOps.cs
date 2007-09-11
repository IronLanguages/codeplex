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
using System.Diagnostics;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Compiler;


[assembly:PythonExtensionType(typeof(DynamicType), typeof(DynamicTypeOps), DerivationType=typeof(ExtensibleType))]
namespace IronPython.Runtime.Operations {
    public static class DynamicTypeOps {
        private static object _DefaultNewInst;

        [PythonName("__bases__")]
        public static readonly DynamicTypeSlot BasesSlot = new DynamicTypeBasesSlot();
        [PythonName("__dict__")]
        public static readonly DynamicTypeSlot DictSlot = new DynamicTypeDictSlot();
        [PythonName("__name__")]
        public static readonly DynamicTypeSlot NameSlot = new DynamicTypeNameSlot();
        [PythonName("__mro__")]
        public static readonly DynamicTypeSlot MroSlot = new DynamicTypeMroSlot();
        [OperatorSlot]
        public static readonly DynamicTypeSlot Call = new DynamicTypeOps.TypeCaller();

        [StaticExtensionMethod("__new__")]
        public static object Make(CodeContext context, object cls, string name, Tuple bases, IAttributesCollection dict) {
            if (name == null) {
                throw PythonOps.TypeError("type() argument 1 must be string, not None");
            }
            if (bases == null) {
                throw PythonOps.TypeError("type() argument 2 must be tuple, not None");
            }
            if (dict == null) {
                throw PythonOps.TypeError("TypeError: type() argument 3 must be dict, not None");
            }

            if (!dict.ContainsKey(Symbols.Module)) {
                object modName;
                if (context.Scope.TryLookupName(context.LanguageContext, Symbols.Name, out modName)) {
                    dict[Symbols.Module] = modName;
                }
            }

            DynamicType meta = cls as DynamicType;
            foreach (object dt in bases) {
                DynamicType metaCls = DynamicHelpers.GetDynamicType(dt);

                if (metaCls == TypeCache.OldClass) continue;

                if (meta.IsSubclassOf(metaCls)) continue;

                if (metaCls.IsSubclassOf(meta)) {
                    meta = metaCls;
                    continue;
                }
                throw PythonOps.TypeError("metaclass conflict {0} and {1}", metaCls.Name, meta.Name);
            }

            if (meta != TypeCache.OldInstance && meta != TypeCache.DynamicType) {
                object newFunc = PythonOps.GetBoundAttr(context, meta, Symbols.NewInst);

                if (meta != cls) {
                    if (_DefaultNewInst == null)
                        _DefaultNewInst = PythonOps.GetBoundAttr(context, TypeCache.DynamicType, Symbols.NewInst);

                    // the user has a custom __new__ which picked the wrong meta class, call __new__ again
                    if (newFunc != _DefaultNewInst)
                        return PythonCalls.Call(newFunc, meta, name, bases, dict);
                }

                // we have the right user __new__, call our ctor method which will do the actual
                // creation.                   
                return meta.CreateInstance(context, name, bases, dict);
            }

            // no custom user type for __new__
            return UserTypeBuilder.Build(context, name, bases, dict);
        }

        [StaticExtensionMethod("__new__")]
        public static object Make(CodeContext context, object cls, object o) {
            return DynamicHelpers.GetDynamicType(o);
        }

        [SpecialName]
        public static bool Equals(DynamicType self, DynamicType other) {
            if (self == null) {
                return other == null;
            } else if (other == null) {
                return false;
            }

            return object.ReferenceEquals(self.CanonicalDynamicType, other.CanonicalDynamicType);
        }

        [PythonName("mro")]
        public static object GetMethodResolutionOrder(object self) {
            throw PythonOps.NotImplementedError("mro is not implemented on type, use __mro__ instead");
        }

        [PythonName("__subclasses__")]
        public static object GetSubClasses(DynamicType self) {
            List ret = new List();
            IList<WeakReference> subtypes = self.SubTypes;
            if (subtypes != null) {
                foreach (WeakReference wr in subtypes) {
                    if (wr.IsAlive) ret.AddNoLock(wr.Target);
                }
            }
            return ret;
        }

        [SpecialName, PythonName("__repr__")]
        public static string Repr(CodeContext context, DynamicType self) {
            string name = GetName(self);

            if (self.IsSystemType) {
                return string.Format("<type '{0}'>", name);
            } else {
                DynamicTypeSlot dts;
                string module = "unknown";
                object modObj;
                if (self.TryLookupSlot(context, Symbols.Module, out dts) &&
                    dts.TryGetValue(context, self, self, out modObj)) {
                    module = modObj as string;
                }
                return string.Format("<class '{0}.{1}'>", module, name);
            }
        }

        [SpecialName, PythonName("__str__")]
        public static string PythonToString(CodeContext context, DynamicType self) {
            return Repr(context, self);
        }

        [PropertyMethod, PythonName("__module__")]
        public static string GetModule(DynamicType self) {
            if (IsRuntimeAssembly(self.UnderlyingSystemType.Assembly) || PythonTypeCustomizer.IsPythonType(self.UnderlyingSystemType)) {
                string moduleName = null;
                Type curType = self.UnderlyingSystemType;
                while (curType != null) {
                    SystemState.Instance.BuiltinModuleNames.TryGetValue(curType, out moduleName);
                    curType = curType.DeclaringType;

                    if (moduleName != null) return moduleName;
                }
                return "__builtin__";
            }

            return self.UnderlyingSystemType.Namespace + " in " + self.UnderlyingSystemType.Assembly.FullName;
        }

        /// <summary>
        /// Helper slot for performing calls on types.    This class is here to both speed
        /// things up (hitting the ICallable* fast paths) and ensure correctness.  W/o this
        /// we hit issues w/ FastCallable unwrapping the object[] arrays if a user explicitly
        /// passses one from Python code.        
        /// </summary>
        internal class TypeCaller : DynamicTypeSlot, ICallableWithCodeContext, IFancyCallable {
            private DynamicType _type;
            public TypeCaller() {
            }

            public TypeCaller(DynamicType type) {
                _type = type;
            }

            #region ICallableWithCodeContext Members

            public object Call(CodeContext context, object[] args) {
                if (_type == null) {
                    return CallWithoutType(context, args);
                }
                return DynamicTypeOps.CallWorker(context, _type, args ?? ArrayUtils.EmptyObjects);
            }

            private static object CallWithoutType(CodeContext context, object[] args) {
                if (args == null || args.Length == 0)
                    throw PythonOps.TypeError("type.__call__ needs an argument");

                DynamicType dt = args[0] as DynamicType;
                if (dt == null) {
                    throw PythonOps.TypeError("type.__call__ requires a type object but received an {0}",
                        PythonOps.StringRepr(DynamicHelpers.GetDynamicType(args[0])));
                }

                return DynamicTypeOps.CallWorker(context, dt, ArrayUtils.RemoveFirst(args));
            }

            #endregion

            public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
                value = new TypeCaller((DynamicType)instance);
                return true;
            }

            #region IFancyCallable Members

            public object Call(CodeContext context, object[] args, string[] names) {
                return DynamicTypeOps.CallWorker(context, _type, new KwCallInfo(args, names));
            }

            #endregion
        }

        internal static object CallParams(CodeContext context, DynamicType cls, params object[] args\u03c4) {
            if (args\u03c4 == null) args\u03c4 = ArrayUtils.EmptyObjects;

            return CallWorker(context, cls, args\u03c4);
        }

        internal static object CallWorker(CodeContext context, DynamicType dt, object[] args) {
            object newObject = PythonOps.CallWithContext(context, GetTypeNew(context, dt), ArrayUtils.Insert<object>(dt, args));

            if (ShouldInvokeInit(dt, DynamicHelpers.GetDynamicType(newObject), args.Length)) {
                PythonOps.CallWithContext(context, GetInitMethod(context, dt, newObject), args);

                AddFinalizer(context, dt, newObject);
            }

            return newObject;
        }

        private static object CallWorker(CodeContext context, DynamicType dt, KwCallInfo args) {
            object[] clsArgs = ArrayUtils.Insert<object>(dt, args.Arguments);
            object newObject = PythonOps.CallWithKeywordArgs(context,
                GetTypeNew(context, dt),
                clsArgs,
                args.Names);

            if (newObject == null) return null;

            if (ShouldInvokeInit(dt, DynamicHelpers.GetDynamicType(newObject), args.Arguments.Length)) {
                PythonOps.CallWithKeywordArgs(context, GetInitMethod(context, dt, newObject), args.Arguments, args.Names);

                AddFinalizer(context, dt, newObject);
            }

            return newObject;
        }

        /// <summary>
        /// Looks up __init__ avoiding calls to __getattribute__ and handling both
        /// new-style and old-style classes in the MRO.
        /// </summary>
        private static object GetInitMethod(CodeContext context, DynamicType dt, object newObject) {
            // __init__ is never searched for w/ __getattribute__
            for (int i = 0; i < dt.ResolutionOrder.Count; i++) {
                DynamicMixin cdt = dt.ResolutionOrder[i];
                DynamicTypeSlot dts;
                object value;
                if (Mro.IsOldStyle(cdt)) {
                    OldClass oc = PythonOps.ToPythonType(cdt) as OldClass;

                    if (oc != null && oc.TryGetBoundCustomMember(context, Symbols.Init, out value)) {
                        return oc.GetOldStyleDescriptor(context, value, newObject, oc);
                    }
                    // fall through to new-style only case.  We might accidently
                    // detect old-style if the user imports a IronPython.NewTypes
                    // type.
                }

                if (cdt.TryLookupSlot(context, Symbols.Init, out dts) &&
                    dts.TryGetValue(context, newObject, dt, out value)) {
                    return value;
                }

            }
            return null;
        }


        private static void AddFinalizer(CodeContext context, DynamicType dt, object newObject) {
            // check if object has finalizer...
            DynamicTypeSlot dummy;
            if (dt.TryResolveSlot(context, Symbols.Unassign, out dummy)) {
                IWeakReferenceable iwr = newObject as IWeakReferenceable;
                Debug.Assert(iwr != null);

                InstanceFinalizer nif = new InstanceFinalizer(newObject);
                iwr.SetFinalizer(new WeakRefTracker(nif, nif));
            }
        }

        private static object GetTypeNew(CodeContext context, DynamicType dt) {
            DynamicTypeSlot dts;

            if (!dt.TryResolveSlot(context, Symbols.NewInst, out dts)) {
                throw PythonOps.TypeError("cannot create instances of {0}", dt.Name);
            }

            object newInst;
            bool res = dts.TryGetValue(context, dt, dt, out newInst);
            Debug.Assert(res);

            return newInst;
        }

        private static bool IsRuntimeAssembly(Assembly assembly) {
            if (assembly == typeof(PythonOps).Assembly || // IronPython.dll
                assembly == typeof(Microsoft.Scripting.Math.BigInteger).Assembly) { // Microsoft.Scripting.dll
                return true;
            }

            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            if (assemblyName.Name.Equals("IronPython.Modules")) { // IronPython.Modules.dll
                return true;
            }

            return false;
        }

        private static bool ShouldInvokeInit(DynamicType cls, DynamicType newObjectType, int argCnt) {
            // don't run __init__ if it's not a subclass of ourselves,
            // or if this is the user doing type(x), or if it's a standard
            // .NET type which doesn't have an __init__ method (this is a perf optimization)
            return (!cls.IsSystemType || cls.GetContextTag(PythonContext.Id) != null) &&
                newObjectType.IsSubclassOf(cls) &&                
                (cls != TypeCache.DynamicType || argCnt > 1);
        }

        internal static string GetName(Type type) {
            string name;
            if (!PythonTypeCustomizer.SystemTypes.TryGetValue(type, out name) &&
                NameConverter.TryGetName(type, out name) == NameType.None) {
                name = type.Name;
            }
            return name;
        }

        internal static string GetName(DynamicType dt) {
            string name;
            if (dt.IsSystemType) {
                return GetName(dt.UnderlyingSystemType);
            } else {
                name = dt.Name;
            }

            return name;
        }

        internal static string GetName(object o) {
            return GetName(DynamicHelpers.GetDynamicType(o));
        }

        // TODO remove this method as we move from DynamicType to Type
        internal static DynamicType[] ObjectTypes(object[] args) {
            DynamicType[] types = new DynamicType[args.Length];
            for (int i = 0; i < args.Length; i++) {
                types[i] = DynamicHelpers.GetDynamicType(args[i]);
            }
            return types;
        }

        internal static Type[] ConvertToTypes(DynamicType[] dynamicTypes) {
            Type[] types = new Type[dynamicTypes.Length];
            for (int i = 0; i < dynamicTypes.Length; i++) {
                types[i] = ConvertToType(dynamicTypes[i]);
            }
            return types;
        }

        private static Type ConvertToType(DynamicType dynamicType) {
            if (dynamicType.IsNull) {
                return None.Type;
            } else {
                return dynamicType.UnderlyingSystemType;
            }
        }
    }
}
