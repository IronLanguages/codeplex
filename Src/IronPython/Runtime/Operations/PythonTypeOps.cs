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
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using IronPython.Compiler.Generation;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations {
    internal static class PythonTypeOps {
        [MultiRuntimeAware]
        internal static object _DefaultNewInst;
        private static readonly Dictionary<FieldInfo, ReflectedField> _fieldCache = new Dictionary<FieldInfo, ReflectedField>();
        private static readonly Dictionary<BuiltinFunction, BuiltinMethodDescriptor> _methodCache = new Dictionary<BuiltinFunction, BuiltinMethodDescriptor>();
        private static readonly Dictionary<BuiltinFunction, ClassMethodDescriptor> _classMethodCache = new Dictionary<BuiltinFunction, ClassMethodDescriptor>();
        internal static readonly Dictionary<BuiltinFunctionKey, BuiltinFunction> _functions = new Dictionary<BuiltinFunctionKey, BuiltinFunction>();
        private static readonly Dictionary<ReflectionCache.MethodBaseCache, ConstructorFunction> _ctors = new Dictionary<ReflectionCache.MethodBaseCache, ConstructorFunction>();
        private static readonly Dictionary<EventTracker, ReflectedEvent> _eventCache = new Dictionary<EventTracker, ReflectedEvent>();
        private static readonly Dictionary<PropertyTracker, ReflectedGetterSetter> _propertyCache = new Dictionary<PropertyTracker, ReflectedGetterSetter>();
        private static readonly Dictionary<Type, TypePrepender.PrependerState> _prependerState = new Dictionary<Type, TypePrepender.PrependerState>();

        internal static PythonTuple MroToPython(IList<PythonType> types) {
            List<object> res = new List<object>(types.Count);
            foreach (PythonType dt in types) {
                if (dt.UnderlyingSystemType == typeof(ValueType)) continue; // hide value type

                if(dt.OldClass != null) {
                    res.Add(dt.OldClass);
                } else {
                    res.Add(dt);
                }
            }

            return PythonTuple.Make(res);
        }

        internal static string GetModuleName(CodeContext/*!*/ context, Type type) {
            if (IsRuntimeAssembly(type.Assembly) || PythonBinder.IsPythonType(type)) {
                Type curType = type;
                while (curType != null) {
                    string moduleName;
                    if (PythonContext.GetContext(context).BuiltinModuleNames.TryGetValue(curType, out moduleName)) {
                        return moduleName;
                    }

                    curType = curType.DeclaringType;
                }
                return "__builtin__";
            }

            return type.Namespace + " in " + type.Assembly.FullName;
        }

        internal static object CallParams(CodeContext/*!*/ context, PythonType cls, params object[] args\u03c4) {
            if (args\u03c4 == null) args\u03c4 = ArrayUtils.EmptyObjects;

            return CallWorker(context, cls, args\u03c4);
        }

        internal static object CallWorker(CodeContext/*!*/ context, PythonType dt, object[] args) {
            object newObject = PythonOps.CallWithContext(context, GetTypeNew(context, dt), ArrayUtils.Insert<object>(dt, args));

            if (ShouldInvokeInit(dt, DynamicHelpers.GetPythonType(newObject), args.Length)) {
                PythonOps.CallWithContext(context, GetInitMethod(context, dt, newObject), args);

                AddFinalizer(context, dt, newObject);
            }

            return newObject;
        }

        internal static object CallWorker(CodeContext/*!*/ context, PythonType dt, IAttributesCollection kwArgs, object[] args) {
            object[] allArgs = ArrayOps.CopyArray(args, kwArgs.Count + args.Length);
            string[] argNames = new string[kwArgs.Count];

            int i = args.Length;
            foreach (KeyValuePair<SymbolId, object> kvp in kwArgs.SymbolAttributes) {
                allArgs[i] = kvp.Value;
                argNames[i++] = SymbolTable.IdToString(kvp.Key);
            }

            return CallWorker(context, dt, new KwCallInfo(allArgs, argNames));        }

        internal static object CallWorker(CodeContext/*!*/ context, PythonType dt, KwCallInfo args) {
            object[] clsArgs = ArrayUtils.Insert<object>(dt, args.Arguments);
            object newObject = PythonOps.CallWithKeywordArgs(context,
                GetTypeNew(context, dt),
                clsArgs,
                args.Names);

            if (newObject == null) return null;

            if (ShouldInvokeInit(dt, DynamicHelpers.GetPythonType(newObject), args.Arguments.Length)) {
                PythonOps.CallWithKeywordArgs(context, GetInitMethod(context, dt, newObject), args.Arguments, args.Names);

                AddFinalizer(context, dt, newObject);
            }

            return newObject;
        }

        /// <summary>
        /// Looks up __init__ avoiding calls to __getattribute__ and handling both
        /// new-style and old-style classes in the MRO.
        /// </summary>
        private static object GetInitMethod(CodeContext/*!*/ context, PythonType dt, object newObject) {
            // __init__ is never searched for w/ __getattribute__
            for (int i = 0; i < dt.ResolutionOrder.Count; i++) {
                PythonType cdt = dt.ResolutionOrder[i];
                PythonTypeSlot dts;
                object value;
                if (cdt.IsOldClass) {
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


        private static void AddFinalizer(CodeContext/*!*/ context, PythonType dt, object newObject) {
            // check if object has finalizer...
            PythonTypeSlot dummy;
            if (dt.TryResolveSlot(context, Symbols.Unassign, out dummy)) {
                IWeakReferenceable iwr = newObject as IWeakReferenceable;
                Debug.Assert(iwr != null);

                InstanceFinalizer nif = new InstanceFinalizer(newObject);
                iwr.SetFinalizer(new WeakRefTracker(nif, nif));
            }
        }

        private static object GetTypeNew(CodeContext/*!*/ context, PythonType dt) {
            PythonTypeSlot dts;

            if (!dt.TryResolveSlot(context, Symbols.NewInst, out dts)) {
                throw PythonOps.TypeError("cannot create instances of {0}", dt.Name);
            }

            object newInst;
            bool res = dts.TryGetValue(context, dt, dt, out newInst);
            Debug.Assert(res);

            return newInst;
        }

        internal static bool IsRuntimeAssembly(Assembly assembly) {
            if (assembly == typeof(PythonOps).Assembly || // IronPython.dll
                assembly == typeof(Microsoft.Scripting.Math.BigInteger).Assembly || // Microsoft.Scripting.dll
                assembly == typeof(SymbolId).Assembly) {  // Microsoft.Scripting.Core.dll
                return true;
            }

            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            if (assemblyName.Name.Equals("IronPython.Modules")) { // IronPython.Modules.dll
                return true;
            }

            return false;
        }

        private static bool ShouldInvokeInit(PythonType cls, PythonType newObjectType, int argCnt) {
            // don't run __init__ if it's not a subclass of ourselves,
            // or if this is the user doing type(x), or if it's a standard
            // .NET type which doesn't have an __init__ method (this is a perf optimization)
            return (!cls.IsSystemType || cls.IsPythonType) &&
                newObjectType.IsSubclassOf(cls) &&                
                (cls != TypeCache.PythonType || argCnt > 1);
        }

        internal static string GetName(object o) {
            return DynamicHelpers.GetPythonType(o).Name;
        }

        internal static PythonType[] ObjectTypes(object[] args) {
            PythonType[] types = new PythonType[args.Length];
            for (int i = 0; i < args.Length; i++) {
                types[i] = DynamicHelpers.GetPythonType(args[i]);
            }
            return types;
        }

        internal static Type[] ConvertToTypes(PythonType[] pythonTypes) {
            Type[] types = new Type[pythonTypes.Length];
            for (int i = 0; i < pythonTypes.Length; i++) {
                types[i] = ConvertToType(pythonTypes[i]);
            }
            return types;
        }

        private static Type ConvertToType(PythonType pythonType) {
            if (pythonType.IsNull) {
                return None.Type;
            } else {
                return pythonType.UnderlyingSystemType;
            }
        }

        internal static TrackerTypes GetMemberType(MemberGroup members) {
            TrackerTypes memberType = TrackerTypes.All;
            for (int i = 0; i < members.Count; i++) {
                MemberTracker mi = members[i];
                if (mi.MemberType != memberType) {
                    if (memberType != TrackerTypes.All) {
                        return TrackerTypes.All;
                    }
                    memberType = mi.MemberType;
                }
            }

            return memberType;
        }


        internal static PythonTypeSlot/*!*/ GetSlot(MemberGroup group, string name, bool privateBinding) {
            if (group.Count == 0) {
                return null;
            }

            group = FilterNewSlots(group);
            TrackerTypes tt = GetMemberType(group);
            switch(tt) {
                case TrackerTypes.Method:
                    List<MemberInfo> mems = new List<MemberInfo>();
                    foreach (MemberTracker mt in group) {
                        mems.Add(((MethodTracker)mt).Method);
                    }
                    return GetFinalSlotForFunction(GetBuiltinFunction(group[0].DeclaringType, group[0].Name, name, null, mems.ToArray()));
                
                case TrackerTypes.Field:
                    return GetReflectedField(((FieldTracker)group[0]).Field);
                
                case TrackerTypes.Property:
                    return GetReflectedProperty((PropertyTracker)group[0], privateBinding);       
                
                case TrackerTypes.Event:
                    return GetReflectedEvent(((EventTracker)group[0]));
                
                case TrackerTypes.Type:
                    TypeTracker type = (TypeTracker)group[0];
                    for (int i = 1; i < group.Count; i++) {
                        type = TypeGroup.UpdateTypeEntity(type, (TypeTracker)group[i]);
                    }
                    
                    if (type is TypeGroup) {
                        return new PythonTypeValueSlot(type);
                    }

                    return new PythonTypeValueSlot(DynamicHelpers.GetPythonTypeFromType(type.Type));
                
                case TrackerTypes.Constructor:
                    return GetConstructor(group[0].DeclaringType, privateBinding);
                
                case TrackerTypes.Custom:
                    return ((PythonCustomTracker)group[0]).GetSlot();
                
                default:
                    // if we have a new slot in the derived class filter out the 
                    // members from the base class.
                    throw new InvalidOperationException(String.Format("Bad member type {0} on {1}.{2}", tt.ToString(), group[0].DeclaringType, name));
            }
        }

        internal static MemberGroup FilterNewSlots(MemberGroup group) {
            if (GetMemberType(group) == TrackerTypes.All) {
                Type declType = group[0].DeclaringType;
                for (int i = 1; i < group.Count; i++) {
                    if (group[i].DeclaringType != declType) {
                        if (group[i].DeclaringType.IsSubclassOf(declType)) {
                            declType = group[i].DeclaringType;
                        }
                    }
                }
                List<MemberTracker> trackers = new List<MemberTracker>();

                for (int i = 0; i < group.Count; i++) {
                    if (group[i].DeclaringType == declType) {
                        trackers.Add(group[i]);
                    }
                }

                if (trackers.Count != group.Count) {
                    return new MemberGroup(trackers.ToArray());
                }
            }

            return group;
        }

        private static BuiltinFunction GetConstructor(Type t, bool privateBinding) {
            BuiltinFunction ctorFunc = InstanceOps.NonDefaultNewInst;
            MethodBase[] ctors = CompilerHelpers.GetConstructors(t, privateBinding);

            return GetConstructor(t, ctorFunc, ctors);
        }

        internal static bool IsDefaultNew(MethodBase[] targets) {
            if (targets.Length == 1) {
                ParameterInfo[] pis = targets[0].GetParameters();
                if (pis.Length == 0) {
                    return true;
                }

                if (pis.Length == 1 && pis[0].ParameterType == typeof(CodeContext)) {
                    return true;
                }
            }

            return false;
        }

        internal static BuiltinFunction GetConstructorFunction(Type type, string name) {
            BuiltinFunction reflectedCtors = null;
            bool hasDefaultConstructor = false;

            foreach (ConstructorInfo ci in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)) {
                if (ci.IsPublic) {
                    if (ci.GetParameters().Length == 0) hasDefaultConstructor = true;
                    reflectedCtors = BuiltinFunction.MakeOrAdd(reflectedCtors, name, ci, type, FunctionType.Function);
                }
            }
            
            if (type.IsValueType && !hasDefaultConstructor && type != typeof(void)) {
                try {
                    MethodInfo mi = typeof(BinderOps).GetMethod("CreateInstance", Type.EmptyTypes).MakeGenericMethod(type);

                    reflectedCtors = BuiltinFunction.MakeOrAdd(reflectedCtors, name, mi, type, FunctionType.Function);
                } catch (BadImageFormatException) {
                    // certain types (e.g. ArgIterator) won't survive the above call.
                    // we won't let you create instances of these types.
                }
            }
            return reflectedCtors;
        }

        internal static ReflectedEvent GetReflectedEvent(EventTracker tracker) {
            ReflectedEvent res;
            lock (_eventCache) {
                if (!_eventCache.TryGetValue(tracker, out res)) {
                    if (PythonBinder.IsExtendedType(tracker.DeclaringType)) {
                        _eventCache[tracker] = res = new ReflectedEvent(tracker.Event, false);
                    } else {
                        _eventCache[tracker] = res = new ReflectedEvent(tracker.Event, true);
                    }
                }
            }
            return res;
        }

        internal static PythonTypeSlot/*!*/ GetFinalSlotForFunction(BuiltinFunction/*!*/ func) {
            if ((func.FunctionType & FunctionType.Method) != 0) {
                BuiltinMethodDescriptor desc;
                lock (_methodCache) {
                    if (!_methodCache.TryGetValue(func, out desc)) {
                        _methodCache[func] = desc = new BuiltinMethodDescriptor(func);
                    }

                    return desc;
                }
            }

            if (func.Targets[0].IsDefined(typeof(PythonClassMethodAttribute), true)) {
                lock (_classMethodCache) {
                    ClassMethodDescriptor desc;
                    if (!_classMethodCache.TryGetValue(func, out desc)) {
                        _classMethodCache[func] = desc = new ClassMethodDescriptor(func);
                    }

                    return desc;
                }
            }

            return func;
        }

        internal static BuiltinFunction/*!*/ GetBuiltinFunction(Type/*!*/ type, string/*!*/ name, MemberInfo/*!*/[]/*!*/ mems) {
            return GetBuiltinFunction(type, name, null, mems);
        }

        internal struct BuiltinFunctionKey {
            Type DeclaringType;
            ReflectionCache.MethodBaseCache Cache;
            FunctionType FunctionType;

            public BuiltinFunctionKey(Type declaringType, ReflectionCache.MethodBaseCache cache, FunctionType funcType) {
                Cache = cache;
                FunctionType = funcType;
                DeclaringType = declaringType;
            }
        }

        public static MethodBase[] GetNonBaseHelperMethodInfos(MemberInfo[] members) {
            List<MethodBase> res = new List<MethodBase>();
            foreach (MemberInfo mi in members) {
                MethodBase mb = mi as MethodBase;
                if (mb != null && !mb.Name.StartsWith(NewTypeMaker.BaseMethodPrefix)) {
                    res.Add(mb);
                }
            }

            return res.ToArray();
        }

        internal static BuiltinFunction/*!*/ GetBuiltinFunction(Type/*!*/ type, string/*!*/ name, FunctionType? funcType, params MemberInfo/*!*/[]/*!*/ mems) {
            return GetBuiltinFunction(type, name, name, funcType, mems);
        }

        /// <summary>
        /// Gets a builtin function for the given declaring type and member infos.
        /// 
        /// Given the same inputs this always returns the same object ensuring there's only 1 builtinfunction
        /// for each .NET method.
        /// 
        /// This method takes both a cacheName and a pythonName.  The cache name is the real method name.  The pythonName
        /// is the name of the method as exposed to Python.
        /// </summary>
        internal static BuiltinFunction/*!*/ GetBuiltinFunction(Type/*!*/ type, string/*!*/ cacheName, string/*!*/ pythonName, FunctionType? funcType, params MemberInfo/*!*/[]/*!*/ mems) {
            BuiltinFunction res = null;
            
            MethodBase[] bases = GetNonBaseHelperMethodInfos(mems);

            // get the base most declaring type, first sort the list so that
            // the most derived class is at the beginning.
            Array.Sort<MethodBase>(bases, delegate(MethodBase x, MethodBase y) {
                if (x.DeclaringType.IsSubclassOf(y.DeclaringType)) {
                    return -1;
                } else if (y.DeclaringType.IsSubclassOf(x.DeclaringType)) {
                    return 1;
                }
                return 0;
            });

            // then if the provided type is a subclass of the most derived type
            // then our declaring type is the methods declaring type.
            foreach (MethodBase mb in bases) {
                // skip extension methods
                if (mb.DeclaringType.IsAssignableFrom(type)) {
                    
                    if (type == mb.DeclaringType || type.IsSubclassOf(mb.DeclaringType)) {
                        type = mb.DeclaringType;
                        break;
                    }
                }
            }

            if (mems.Length != 0) {
                FunctionType ft = funcType ?? GetMethodFunctionType(type, bases);
                BuiltinFunctionKey cache = new BuiltinFunctionKey(type, new ReflectionCache.MethodBaseCache(cacheName, bases), ft);

                lock (_functions) {
                    if (!_functions.TryGetValue(cache, out res)) {
                        _functions[cache] = res = BuiltinFunction.MakeMethod(pythonName, ReflectionUtils.GetMethodInfos(mems), type, ft);
                    }
                }
            }

            return res;
        }

        internal static ConstructorFunction GetConstructor(Type type, BuiltinFunction realTarget, params MethodBase[] mems) {
            ConstructorFunction res = null;

            if (mems.Length != 0) {
                ReflectionCache.MethodBaseCache cache = new ReflectionCache.MethodBaseCache("__new__", mems);
                lock (_ctors) {
                    if (!_ctors.TryGetValue(cache, out res)) {
                        _ctors[cache] = res = new ConstructorFunction(realTarget, mems);
                    }
                }
            }

            return res;
        }

        internal static FunctionType GetMethodFunctionType(Type type, MethodBase[] methods) {
            FunctionType ft = FunctionType.None;
            foreach (MethodInfo mi in methods) {
                if (mi.IsStatic && mi.IsSpecialName) {

                    ParameterInfo[] pis = mi.GetParameters();
                    if ((pis.Length == 2 && (pis[0].ParameterType != typeof(CodeContext))) || 
                        (pis.Length == 3 && pis[0].ParameterType == typeof(CodeContext))) {
                        ft |= FunctionType.BinaryOperator;

                        if (pis[pis.Length - 2].ParameterType != type && pis[pis.Length - 1].ParameterType == type) {
                            ft |= FunctionType.ReversedOperator;
                        }
                    }
                }

                if (IsStaticFunction(type, mi)) {
                    ft |= FunctionType.Function;
                } else {
                    ft |= FunctionType.Method;
                }
            }

            if (PythonBinder.IsPythonType(type)) {
                bool alwaysVisible = true;
                // only show methods defined outside of the system types (object, string)
                foreach (MethodInfo mi in methods) {
                    if (PythonBinder.IsExtendedType(mi.DeclaringType) ||
                        PythonBinder.IsExtendedType(mi.GetBaseDefinition().DeclaringType) || 
                        mi.IsDefined(typeof(PythonHiddenAttribute), false)) {
                        alwaysVisible = false;
                        break;
                    }
                }

                if (alwaysVisible) {
                    ft |= FunctionType.AlwaysVisible;
                }
            } else if (typeof(IPythonObject).IsAssignableFrom(type)) {
                // check if this is a virtual override helper, if so we
                // may need to filter it out.
                bool alwaysVisible = true;
                foreach (MethodInfo mi in methods) {
                    MethodInfo baseDef = mi.GetBaseDefinition();
                    if (PythonBinder.IsExtendedType(mi.DeclaringType)) {
                        alwaysVisible = false;
                        break;
                    }
                }

                if (alwaysVisible) {
                    ft |= FunctionType.AlwaysVisible;
                }
            } else {
                ft |= FunctionType.AlwaysVisible;
            }

            return ft;
        }

        /// <summary>
        /// a function is static if it's a static .NET method and it's defined on the type or is an extension method 
        /// with StaticExtensionMethod decoration.
        /// </summary>
        private static bool IsStaticFunction(Type type, MethodInfo mi) {            
            return mi.IsStatic &&       // method must be truly static
                !mi.IsDefined(typeof(WrapperDescriptorAttribute), false) &&     // wrapper descriptors are instance methods
                (mi.DeclaringType.IsAssignableFrom(type) || mi.IsDefined(typeof(StaticExtensionMethodAttribute), false)); // or it's not an extension method or it's a static extension method
        }

        internal static ReflectedField GetReflectedField(FieldInfo info) {
            ReflectedField res;

            NameType nt = NameType.Field;
            if (!PythonBinder.IsExtendedType(info.DeclaringType) && 
                !info.IsDefined(typeof(PythonHiddenAttribute), false)) {
                nt |= NameType.PythonField;
            }

            lock (_fieldCache) {
                if (!_fieldCache.TryGetValue(info, out res)) {
                    _fieldCache[info] = res = new ReflectedField(info, nt);
                }
            }

            return res;
        }

        internal static string GetDocumentation(Type type) {
            // Python documentation
            object[] docAttr = type.GetCustomAttributes(typeof(DocumentationAttribute), false);
            if (docAttr != null && docAttr.Length > 0) {
                return ((DocumentationAttribute)docAttr[0]).Documentation;
            }

            if (type == typeof(None)) return null;

            // Auto Doc (XML or otherwise)
            string autoDoc = DocBuilder.CreateAutoDoc(type);
            if (autoDoc == null) {
                autoDoc = String.Empty;
            } else {
                autoDoc += Environment.NewLine + Environment.NewLine;
            }

            // Simple generated helpbased on ctor, if available.
            ConstructorInfo[] cis = type.GetConstructors();
            foreach (ConstructorInfo ci in cis) {
                autoDoc += FixCtorDoc(type, DocBuilder.CreateAutoDoc(ci)) + Environment.NewLine;
            }

            return autoDoc;
        }

        private static string FixCtorDoc(Type type, string autoDoc) {
            return autoDoc.Replace("__new__(cls)", DynamicHelpers.GetPythonTypeFromType(type).Name + "()").
                            Replace("__new__(cls, ", DynamicHelpers.GetPythonTypeFromType(type).Name + "(");
        }

        internal static ReflectedGetterSetter GetReflectedProperty(PropertyTracker pt, bool privateBinding) {
            ReflectedGetterSetter rp;
            lock (_propertyCache) {
                if (_propertyCache.TryGetValue(pt, out rp)) {
                    return rp;
                }

                NameType nt = NameType.PythonProperty;
                MethodInfo getter = FilterProtectedGetterOrSetter(pt.GetGetMethod(true), privateBinding);
                MethodInfo setter = FilterProtectedGetterOrSetter(pt.GetSetMethod(true), privateBinding);

                if ((getter != null && getter.IsDefined(typeof(PythonHiddenAttribute), true)) ||
                    setter != null && setter.IsDefined(typeof(PythonHiddenAttribute), true)) {
                    nt = NameType.Property;
                }

                ExtensionPropertyTracker ept = pt as ExtensionPropertyTracker;
                if (ept != null) {
                    rp = new ReflectedExtensionProperty(new ExtensionPropertyInfo(pt.DeclaringType,
                        getter ?? setter), nt);
                } else {
                    ReflectedPropertyTracker rpt = pt as ReflectedPropertyTracker;
                    Debug.Assert(rpt != null);

                    if (PythonBinder.IsExtendedType(pt.DeclaringType) ||
                        rpt.Property.IsDefined(typeof(PythonHiddenAttribute), true)) {
                        nt = NameType.Property;
                    }

                    NewTypeMaker.PropertyOverrideInfo overrideInfo;
                    if (pt.GetIndexParameters().Length > 0) {
                        rp = new ReflectedIndexer(((ReflectedPropertyTracker)pt).Property, NameType.Property);
                    } else if(NewTypeMaker._overriddenProperties.TryGetValue(rpt.Property, out overrideInfo)) {
                        List<MethodInfo> getters = CopyOrNewList(overrideInfo.Getters);
                        List<MethodInfo> setters = CopyOrNewList(overrideInfo.Setters);
                        if (getter != null) {
                            getters.Add(getter);
                        }
                        if (setter != null) {
                            setters.Add(setter);
                        }
                        rp = new ReflectedProperty(rpt.Property, getters.ToArray(), setters.ToArray(), nt);
                    } else {
                        rp = new ReflectedProperty(rpt.Property, getter, setter, nt);
                    }
                }

                _propertyCache[pt] = rp;

                return rp;
            }            
        }

        private static List<MethodInfo> CopyOrNewList(List<MethodInfo> getters) {
            if (getters != null) {
                getters = new List<MethodInfo>(getters);
            } else {
                getters = new List<MethodInfo>(1);
            }
            return getters;
        }

        private static MethodInfo FilterProtectedGetterOrSetter(MethodInfo info, bool privateBinding) {
            if (info != null) {
                if (privateBinding || info.IsPublic) {
                    return info;
                }

                if (info.IsFamily || info.IsFamilyOrAssembly) {
                    return info;
                }
            }

            return null;
        }

        internal static bool TryInvokeUnaryOperator(CodeContext context, object o, SymbolId si, out object value) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Temporary, "UnaryOp " + CompilerHelpers.GetType(o).Name + " " + SymbolTable.IdToString(si));

            PythonTypeSlot pts;
            PythonType pt = DynamicHelpers.GetPythonType(o);
            object callable;

            if (pt.TryResolveMixedSlot(context, si, out pts) &&
                pts.TryGetBoundValue(context, o, pt, out callable)) {
                value = PythonCalls.Call(callable);
                return true;
            }

            value = null;
            return false;
        }

        internal static bool TryInvokeBinaryOperator(CodeContext context, object o, object arg1, SymbolId si, out object value) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Temporary, "BinaryOp " + CompilerHelpers.GetType(o).Name + " " + SymbolTable.IdToString(si));

            PythonTypeSlot pts;
            PythonType pt = DynamicHelpers.GetPythonType(o);
            object callable;
            if (pt.TryResolveMixedSlot(context, si, out pts) &&
                pts.TryGetBoundValue(context, o, pt, out callable)) {
                value = PythonCalls.Call(callable, arg1);
                return true;
            }

            value = null;
            return false;
        }

        internal static bool TryInvokeTernaryOperator(CodeContext context, object o, object arg1, object arg2, SymbolId si, out object value) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Temporary, "TernaryOp " + CompilerHelpers.GetType(o).Name + " " + SymbolTable.IdToString(si));

            PythonTypeSlot pts;
            PythonType pt = DynamicHelpers.GetPythonType(o);
            object callable;
            if (pt.TryResolveMixedSlot(context, si, out pts) &&
                pts.TryGetBoundValue(context, o, pt, out callable)) {
                value = PythonCalls.Call(callable, arg1, arg2);
                return true;
            }

            value = null;
            return false;
        }

        internal static TypePrepender.PrependerState GetPrependerState(Type t) {
            TypePrepender.PrependerState prependerState;
            lock (_prependerState) {
                if (!_prependerState.TryGetValue(t, out prependerState)) {
                    _prependerState[t] = prependerState =
                        new TypePrepender.PrependerState(BuiltinFunction.MakeMethod(t.Name, t.GetConstructors(), t, FunctionType.Function));
                }
            }
            return prependerState;
        }

        /// <summary>
        /// If we have only interfaces, we'll need to insert object's base
        /// </summary>
        internal static PythonTuple EnsureBaseType(PythonTuple bases) {
            foreach (object baseClass in bases) {
                if (baseClass is OldClass) continue;

                PythonType dt = baseClass as PythonType;

                if (!dt.UnderlyingSystemType.IsInterface)
                    return bases;
            }

            // We found only interfaces. We need do add System.Object to the bases
            return new PythonTuple(bases, TypeCache.Object);
        }

        internal static Type GetFinalSystemType(Type type) {
            while (typeof(IPythonObject).IsAssignableFrom(type) && !type.IsDefined(typeof(DynamicBaseTypeAttribute), false)) {
                type = type.BaseType;
            }
            return type;
        }
    }
}
