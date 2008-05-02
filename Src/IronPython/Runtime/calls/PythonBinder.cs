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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Calls {
    using Ast = Microsoft.Scripting.Ast.Expression;
    using System.Threading;
    using IronPython.Compiler.Generation;

    public class PythonBinder : ActionBinder {
        private PythonContext/*!*/ _context;
        private SlotCache/*!*/ _typeMembers = new SlotCache();
        private SlotCache/*!*/ _resolvedMembers = new SlotCache();

        [MultiRuntimeAware]
        private static Dictionary<Type, Type> _extTypes = new Dictionary<Type, Type>();
        private readonly GetMemberAction EmptyGetMemberAction;

        public PythonBinder(PythonContext/*!*/ pythonContext, CodeContext context)
            : base(context) {
            ContractUtils.RequiresNotNull(pythonContext, "pythonContext");

            EmptyGetMemberAction = GetMemberAction.Make(this, String.Empty);
            _context = pythonContext;
        }

        private RuleBuilder<T> MakeRuleWorker<T>(CodeContext/*!*/ context, DynamicAction/*!*/ action, object[]/*!*/ args) {
            switch (action.Kind) {
                case DynamicActionKind.DoOperation:
                    return new PythonDoOperationBinderHelper<T>(context, (DoOperationAction)action).MakeRule(args);
                case DynamicActionKind.GetMember:
                    return new PythonGetMemberBinderHelper<T>(context, (GetMemberAction)action, args).MakeRule();
                case DynamicActionKind.SetMember:
                    return new SetMemberBinderHelper<T>(context, (SetMemberAction)action, args).MakeNewRule();
                case DynamicActionKind.Call:
                    // if call fails Python will try and create an instance as it treats these two operations as the same.
                    RuleBuilder<T> rule = new PythonCallBinderHelper<T>(context, (CallAction)action, args).MakeRule();
                    if (rule == null) {
                        rule = base.MakeRule<T>(context, action, args);

                        if (rule.IsError) {
                            // try CreateInstance...
                            CreateInstanceAction createAct = PythonCallBinderHelper<T>.MakeCreateInstanceAction((CallAction)action);
                            RuleBuilder<T> newRule = MakeRule<T>(context, createAct, args);
                            if (!newRule.IsError) {
                                return newRule;
                            }
                        }
                    }
                    return rule;
                case DynamicActionKind.ConvertTo:
                    return new PythonConvertToBinderHelper<T>(context, (ConvertToAction)action, args).MakeRule();
                default:
                    return null;
            }
        }

        protected override RuleBuilder<T> MakeRule<T>(CodeContext/*!*/ context, DynamicAction/*!*/ action, object[]/*!*/ args) {
            RuleBuilder<T> rule = null;
            //
            // Try IDynamicObject
            //
            IDynamicObject ido = args[0] as IDynamicObject;
            if (ido != null) {
                rule = ido.GetRule<T>(action, context, args);
                if (rule != null) {
                    return rule;
                }
            }

            //
            // Try the Python rules
            //
            rule = MakeRuleWorker<T>(context, action, args);
            if (rule != null) {
                return rule;
            }

            //
            // Fall back on DLR rules
            //
            return base.MakeRule<T>(context, action, args);
        }

        public override Expression/*!*/ ConvertExpression(Expression/*!*/ expr, Type/*!*/ toType) {
            ContractUtils.RequiresNotNull(expr, "expr");
            ContractUtils.RequiresNotNull(toType, "toType");

            Type exprType = expr.Type;

            if (toType == typeof(object)) {
                if (exprType.IsValueType) {
                    return Ast.Convert(expr, toType);
                } else {
                    return expr;
                }
            }

            if (toType.IsAssignableFrom(exprType)) {
                return expr;
            }

            Type visType = CompilerHelpers.GetVisibleType(toType);
            return Ast.Action.ConvertTo(
                ConvertToAction.Make(this, visType, visType == typeof(char) ? ConversionResultKind.ImplicitCast : ConversionResultKind.ExplicitCast),
                expr);
        }

        internal static MethodInfo GetGenericConvertMethod(Type toType) {
            if (toType.IsValueType) {
                if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return typeof(Converter).GetMethod("ConvertToNullableType");
                } else {
                    return typeof(Converter).GetMethod("ConvertToValueType");
                }
            } else {
                return typeof(Converter).GetMethod("ConvertToReferenceType");
            }
        }


        internal static MethodInfo GetFastConvertMethod(Type toType) {
            if (toType == typeof(char)) {
                return typeof(Converter).GetMethod("ConvertToChar");
            } else if (toType == typeof(int)) {
                return typeof(Converter).GetMethod("ConvertToInt32");
            } else if (toType == typeof(string)) {
                return typeof(Converter).GetMethod("ConvertToString");
            } else if (toType == typeof(long)) {
                return typeof(Converter).GetMethod("ConvertToInt64");
            } else if (toType == typeof(double)) {
                return typeof(Converter).GetMethod("ConvertToDouble");
            } else if (toType == typeof(bool)) {
                return typeof(Converter).GetMethod("ConvertToBoolean");
            } else if (toType == typeof(BigInteger)) {
                return typeof(Converter).GetMethod("ConvertToBigInteger");
            } else if (toType == typeof(Complex64)) {
                return typeof(Converter).GetMethod("ConvertToComplex64");
            } else if (toType == typeof(IEnumerable)) {
                return typeof(Converter).GetMethod("ConvertToIEnumerable");
            } else if (toType == typeof(float)) {
                return typeof(Converter).GetMethod("ConvertToSingle");
            } else if (toType == typeof(byte)) {
                return typeof(Converter).GetMethod("ConvertToByte");
            } else if (toType == typeof(sbyte)) {
                return typeof(Converter).GetMethod("ConvertToSByte");
            } else if (toType == typeof(short)) {
                return typeof(Converter).GetMethod("ConvertToInt16");
            } else if (toType == typeof(uint)) {
                return typeof(Converter).GetMethod("ConvertToUInt32");
            } else if (toType == typeof(ulong)) {
                return typeof(Converter).GetMethod("ConvertToUInt64");
            } else if (toType == typeof(ushort)) {
                return typeof(Converter).GetMethod("ConvertToUInt16");
            } else if (toType == typeof(Type)) {
                return typeof(Converter).GetMethod("ConvertToType");
            } else {
                return null;
            }
        }

        public override object Convert(object obj, Type toType) {
            return Converter.Convert(obj, toType);
        }

        public override bool CanConvertFrom(Type fromType, Type toType, NarrowingLevel level) {
            return Converter.CanConvertFrom(fromType, toType, level);
        }

        public override bool PreferConvert(Type t1, Type t2) {
            return Converter.PreferConvert(t1, t2);
        }

        public override object GetByRefArray(object[] args) {
            return PythonTuple.MakeTuple(args);
        }

        public override ErrorInfo MakeConversionError(Type toType, Expression value) {
            return ErrorInfo.FromException(
                Ast.Call(
                    typeof(PythonOps).GetMethod("TypeErrorForTypeMismatch"),
                    Ast.Constant(PythonTypeOps.GetName(toType)),
                    Ast.ConvertHelper(value, typeof(object))
               )
            );
        }

        public override ErrorInfo MakeStaticAssignFromDerivedTypeError(Type accessingType, MemberTracker info, Expression assignedValue) {
            return MakeMissingMemberError(accessingType, info.Name);
        }
        
        public override ErrorInfo MakeStaticPropertyInstanceAccessError(PropertyTracker/*!*/ tracker, bool isAssignment, IList<Expression/*!*/>/*!*/ parameters) {
            ContractUtils.RequiresNotNull(tracker, "tracker");
            ContractUtils.RequiresNotNull(parameters, "parameters");
            ContractUtils.RequiresNotNullItems(parameters, "parameters");

            return ErrorInfo.FromException(
                Ast.Call(
                    typeof(PythonOps).GetMethod("StaticAssignmentFromInstanceError"),
                    Ast.RuntimeConstant(tracker),
                    Ast.Constant(isAssignment)
                )
            );
        }
        
        #region .NET member binding

        protected override string GetTypeName(Type t) {
            return PythonTypeOps.GetName(t);
        }

        private MemberGroup GetInstanceOpsMethod(Type extends, params string[] names) {
            MethodTracker[] trackers = new MethodTracker[names.Length];
            for (int i = 0; i < names.Length; i++) {
                trackers[i] = (MethodTracker)MemberTracker.FromMemberInfo(typeof(InstanceOps).GetMethod(names[i]), extends);
            }

            return new MemberGroup(trackers);
        }

        public override MemberGroup/*!*/ GetMember(DynamicAction action, Type type, string name) {
            // avoid looking in IPythonObject's.  They don't add interesting members that their
            // base types don't provide and will only slow us down.
            while (typeof(IPythonObject).IsAssignableFrom(type) && !type.IsDefined(typeof(DynamicBaseTypeAttribute), false)) {
                type = type.BaseType;
            }

            MemberGroup mg;
            if (!_resolvedMembers.TryGetCachedMember(type, name, action.Kind == DynamicActionKind.GetMember, out mg)) {
                mg = TypeInfo.GetMemberAll(
                    this,
                    action,
                    type,
                    name);

                _resolvedMembers.CacheSlot(type, name, PythonTypeOps.GetSlot(mg), mg);
            }

            return mg ?? MemberGroup.EmptyGroup;
        }

        public override ErrorInfo MakeEventValidation(RuleBuilder rule, MemberGroup members) {
            EventTracker ev = (EventTracker)members[0];

            return ErrorInfo.FromValueNoError(
               Ast.Call(
                   typeof(PythonOps).GetMethod("SlotTrySetValue"),
                   Ast.CodeContext(),
                   Ast.RuntimeConstant(PythonTypeOps.GetReflectedEvent(ev)),
                   Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                   Ast.Null(typeof(PythonType)),
                   Ast.ConvertHelper(rule.Parameters[1], typeof(object))
               )
            );
        }

        public override ErrorInfo MakeMissingMemberError(Type type, string name) {
            return ErrorInfo.FromException(
                Ast.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Constant(String.Format("'{0}' object has no attribute '{1}'", PythonTypeOps.GetName(DynamicHelpers.GetPythonTypeFromType(type)), name))
                )
            );
        }

        public override Expression MakeReadOnlyMemberError<T>(RuleBuilder<T> rule, Type type, string name) {
            return rule.MakeError(
                Ast.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Constant(
                        String.Format("attribute '{0}' of '{1}' object is read-only",
                            name,
                            PythonTypeOps.GetName(DynamicHelpers.GetPythonTypeFromType(type))
                        )
                    )
                )
            );
        }

        public override Expression MakeUndeletableMemberError<T>(RuleBuilder<T> rule, Type type, string name) {
            return rule.MakeError(
                Ast.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Constant(
                        String.Format("cannot delete attribute '{0}' of builtin type '{1}'",
                            name,
                            PythonTypeOps.GetName(DynamicHelpers.GetPythonTypeFromType(type))
                        )
                    )
                )
            );
        }

        #endregion

        internal IList<Type> GetExtensionTypesInternal(Type t) {
            List<Type> res = new List<Type>(base.GetExtensionTypes(t));

            Type extType;
            if (_extTypes.TryGetValue(t, out extType)) {
                res.Add(extType);
            }

            return res.ToArray();
        }

        protected override IList<Type> GetExtensionTypes(Type t) {
            List<Type> list = new List<Type>();

            // Python includes the types themselves so we can use extension properties w/ CodeContext
            list.Add(t);

            list.AddRange(base.GetExtensionTypes(t));

            Type extType;
            if (_extTypes.TryGetValue(t, out extType)) {
                list.Add(extType);
            }

            return list;
        }

        internal static void RegisterType(Type extended, Type extension) {
            _extTypes[extended] = extension;
        }

        public override Expression ReturnMemberTracker(Type type, MemberTracker memberTracker) {
            Expression res = ReturnMemberTracker(type, memberTracker, _context.DomainManager.GlobalOptions.PrivateBinding);
           
            return res ?? base.ReturnMemberTracker(type, memberTracker);
        }

        private static Expression ReturnMemberTracker(Type type, MemberTracker memberTracker, bool privateMembers) {
            switch (memberTracker.MemberType) {
                case TrackerTypes.TypeGroup:
                    return Ast.RuntimeConstant(memberTracker);
                case TrackerTypes.Type:
                    return ReturnTypeTracker((TypeTracker)memberTracker);
                case TrackerTypes.Bound:
                    return ReturnBoundTracker((BoundMemberTracker)memberTracker);
                case TrackerTypes.Property:
                    return ReturnPropertyTracker((PropertyTracker)memberTracker, privateMembers);
                case TrackerTypes.Event:
                    return Ast.Call(
                        typeof(PythonOps).GetMethod("MakeBoundEvent"),
                        Ast.RuntimeConstant(PythonTypeOps.GetReflectedEvent((EventTracker)memberTracker)),
                        Ast.Null(),
                        Ast.Constant(type)
                    );
                case TrackerTypes.Field:
                    return ReturnFieldTracker((FieldTracker)memberTracker);
                case TrackerTypes.MethodGroup:
                    return ReturnMethodGroup((MethodGroup)memberTracker);
                case TrackerTypes.Constructor:
                    MethodBase[] ctors = CompilerHelpers.GetConstructors(type);
                    object val;
                    if (PythonTypeOps.IsDefaultNew(ctors)) {
                        if (PythonTypeCustomizer.IsPythonType(type)) {
                            val = InstanceOps.New;
                        } else {
                            val = InstanceOps.NewCls;
                        }
                    } else {
                        val = PythonTypeOps.GetConstructor(type, InstanceOps.NonDefaultNewInst, CompilerHelpers.GetConstructors(type));
                    }

                    return Ast.RuntimeConstant(val);
                case TrackerTypes.Custom:
                    return Ast.RuntimeConstant(((PythonCustomTracker)memberTracker).GetSlot());
            }
            return null;
        }

        /// <summary>
        /// Gets the PythonBinder associated with tihs CodeContext
        /// </summary>
        public static PythonBinder/*!*/ GetBinder(CodeContext/*!*/ context) {
            return (PythonBinder)PythonContext.GetContext(context).Binder;
        }

        /// <summary>
        /// Performs .NET member resolution.  This looks within the given type and also
        /// includes any extension members.  Base classes and their extension members are 
        /// not searched.
        /// </summary>
        public bool TryLookupSlot(CodeContext/*!*/ context, PythonType/*!*/ type, SymbolId name, out PythonTypeSlot slot) {
            Debug.Assert(type.IsSystemType);

            string strName = SymbolTable.IdToString(name);
            Type curType = type.UnderlyingSystemType;

            if (!_typeMembers.TryGetCachedSlot(curType, strName, out slot)) {
                MemberGroup mg = TypeInfo.GetMember(
                    this,
                    GetMemberAction.Make(this, name),
                    curType,
                    strName);

                slot = PythonTypeOps.GetSlot(mg);

                _typeMembers.CacheSlot(curType, strName, slot, mg);
            }
            
            if (slot != null && slot.IsVisible(context, type)) {
                return true;
            }

            slot = null;
            return false;
        }

        /// <summary>
        /// Performs .NET member resolution.  This looks the type and any base types
        /// for members.  It also searches for extension members in the type and any base types.
        /// </summary>
        public bool TryResolveSlot(CodeContext/*!*/ context, PythonType/*!*/ type, PythonType/*!*/ owner, SymbolId name, out PythonTypeSlot slot) {
            Debug.Assert(type.IsSystemType);

            string strName = SymbolTable.IdToString(name);
            Type curType = type.UnderlyingSystemType;

            if (!_resolvedMembers.TryGetCachedSlot(curType, strName, out slot)) {
                MemberGroup mg = TypeInfo.GetMemberAll(
                    this,
                    GetMemberAction.Make(this, strName),
                    curType,
                    strName);

                slot = PythonTypeOps.GetSlot(mg);

                _resolvedMembers.CacheSlot(curType, strName, slot, mg);
            }

            if (slot != null && slot.IsVisible(context, owner)) {                
                return true;                
            }             

            slot = null;
            return false;
        }

        /// <summary>
        /// Gets the member names which are defined in this type and any extension members.
        /// 
        /// This search does not include members in any subtypes or their extension members.
        /// </summary>
        public void LookupMembers(CodeContext/*!*/ context, PythonType/*!*/ type, IAttributesCollection/*!*/ memberNames) {
            if (!_typeMembers.IsFullyCached(type.UnderlyingSystemType)) {
                Dictionary<string, KeyValuePair<PythonTypeSlot, MemberGroup>> members = new Dictionary<string, KeyValuePair<PythonTypeSlot, MemberGroup>>();

                foreach (ResolvedMember rm in TypeInfo.GetMembers(
                    this,
                    EmptyGetMemberAction,
                    type.UnderlyingSystemType)) {

                    if (!members.ContainsKey(rm.Name)) {
                        members[rm.Name] = new KeyValuePair<PythonTypeSlot, MemberGroup>(PythonTypeOps.GetSlot(rm.Member), rm.Member);
                    }
                }

                _typeMembers.CacheAll(type.UnderlyingSystemType, members);
            }

            foreach (KeyValuePair<string, PythonTypeSlot> kvp in _typeMembers.GetAllMembers(type.UnderlyingSystemType)) {
                PythonTypeSlot slot = kvp.Value;
                string name = kvp.Key;

                if (slot.IsVisible(context, type)) {
                    memberNames[SymbolTable.StringToId(name)] = slot;
                }
            }
        }

        /// <summary>
        /// Gets the member names which are defined in the type and any subtypes.  
        /// 
        /// This search includes members in the type and any subtypes as well as extension
        /// types of the type and its subtypes.
        /// </summary>
        public void ResolveMemberNames(CodeContext/*!*/ context, PythonType/*!*/ type, PythonType/*!*/ owner, Dictionary<string, string>/*!*/ memberNames) {
            if (!_resolvedMembers.IsFullyCached(type.UnderlyingSystemType)) {
                Dictionary<string, KeyValuePair<PythonTypeSlot, MemberGroup>> members = new Dictionary<string, KeyValuePair<PythonTypeSlot, MemberGroup>>();
                
                foreach (ResolvedMember rm in TypeInfo.GetMembersAll(
                    this,
                    EmptyGetMemberAction,
                    type.UnderlyingSystemType)) {

                    if (!members.ContainsKey(rm.Name)) {
                        members[rm.Name] = new KeyValuePair<PythonTypeSlot, MemberGroup>(PythonTypeOps.GetSlot(rm.Member), rm.Member);
                    }
                }

                _resolvedMembers.CacheAll(type.UnderlyingSystemType, members);
            }

            foreach (KeyValuePair<string, PythonTypeSlot> kvp in _resolvedMembers.GetAllMembers(type.UnderlyingSystemType)) {
                PythonTypeSlot slot = kvp.Value;
                string name = kvp.Key;

                if (slot.IsVisible(context, owner)) {
                    memberNames[name] = name;
                }
            }
        }

        private static Expression ReturnFieldTracker(FieldTracker fieldTracker) {
            return Ast.RuntimeConstant(PythonTypeOps.GetReflectedField(fieldTracker.Field));
        }

        private static Expression ReturnMethodGroup(MethodGroup methodGroup) {
            return Ast.RuntimeConstant(PythonTypeOps.GetFinalSlotForFunction(GetBuiltinFunction(methodGroup)));
        }

        private static Expression ReturnBoundTracker(BoundMemberTracker boundMemberTracker) {
            MemberTracker boundTo = boundMemberTracker.BoundTo;
            switch (boundTo.MemberType) {
                case TrackerTypes.Property:
                    PropertyTracker pt = (PropertyTracker)boundTo;
                    Debug.Assert(pt.GetIndexParameters().Length > 0);
                    return Ast.New(
                        typeof(ReflectedIndexer).GetConstructor(new Type[] { typeof(ReflectedIndexer), typeof(object) }),
                        Ast.RuntimeConstant(new ReflectedIndexer(((ReflectedPropertyTracker)pt).Property, NameType.Property)),
                        boundMemberTracker.Instance
                    );
                case TrackerTypes.Event:
                    return Ast.Call(
                        typeof(PythonOps).GetMethod("MakeBoundEvent"),
                        Ast.RuntimeConstant(PythonTypeOps.GetReflectedEvent((EventTracker)boundMemberTracker.BoundTo)),
                        boundMemberTracker.Instance,
                        Ast.Constant(boundMemberTracker.DeclaringType)
                    );
                case TrackerTypes.MethodGroup:
                    return Ast.Call(
                        typeof(PythonOps).GetMethod("MakeBoundBuiltinFunction"),
                        Ast.RuntimeConstant(GetBuiltinFunction((MethodGroup)boundTo)),
                        boundMemberTracker.Instance
                    );
            }
            throw new NotImplementedException();
        }

        private static BuiltinFunction GetBuiltinFunction(MethodGroup mg) {
            MethodBase[] methods = new MethodBase[mg.Methods.Count];
            for (int i = 0; i < mg.Methods.Count; i++) {
                methods[i] = mg.Methods[i].Method;
            }
            return PythonTypeOps.GetBuiltinFunction(
                mg.DeclaringType,
                mg.Methods[0].Name,
                (PythonTypeOps.GetMethodFunctionType(mg.DeclaringType, methods) & (~FunctionType.FunctionMethodMask)) |
                    (mg.ContainsInstance ? FunctionType.Method : FunctionType.None) |
                    (mg.ContainsStatic ? FunctionType.Function : FunctionType.None),
                mg.GetMethodBases()
            );            
        }

        private static Expression ReturnPropertyTracker(PropertyTracker propertyTracker, bool privateMembers) {
            return Ast.RuntimeConstant(PythonTypeOps.GetReflectedProperty(propertyTracker));
        }

        private static MethodInfo IncludePropertyMethod(MethodInfo method, bool privateMembers) {
            if (privateMembers) return method;

            if (method != null) {
                if (method.IsPrivate || (method.IsAssembly && !method.IsFamilyOrAssembly)) {
                    return null;
                }
            }

            // method is public, protected, or null
            return method;
        }

        private static Expression ReturnTypeTracker(TypeTracker memberTracker) {
            // all non-group types get exposed as PythonType's
            return Ast.RuntimeConstant(DynamicHelpers.GetPythonTypeFromType(memberTracker.Type));
        }

        protected override bool AllowKeywordArgumentSetting(MethodBase method) {
            return CompilerHelpers.IsConstructor(method) && !method.DeclaringType.IsDefined(typeof(PythonSystemTypeAttribute), true);
        }

        internal ScriptDomainManager/*!*/ DomainManager {
            get {
                return _context.DomainManager;
            }
        }

        /// <summary>
        /// Provides a cache from Type/name -> PythonTypeSlot and also allows access to
        /// all members (and remembering whether all members are cached).
        /// </summary>
        private class SlotCache {
            private Dictionary<Type/*!*/, SlotCacheInfo/*!*/> _cachedInfos;

            /// <summary>
            /// Writes to a cache the result of a type lookup.  Null values are allowed for the slots and they indicate that
            /// the value does not exist.
            /// </summary>
            public void CacheSlot(Type/*!*/ type, string/*!*/ name, PythonTypeSlot slot, MemberGroup/*!*/ memberGroup) {
                Debug.Assert(type != null); Debug.Assert(name != null);

                EnsureInfo();

                lock (_cachedInfos) {
                    SlotCacheInfo slots = GetSlotForType(type);

                    if (slots.ResolvedAll && slot == null && memberGroup.Count == 0) {
                        // nothing to cache, and we know we don't need to cache non-hits.
                        return;
                    }

                    slots.Members[name] = new KeyValuePair<PythonTypeSlot,MemberGroup>(slot, memberGroup);
                }
            }

            /// <summary>
            /// Looks up a cached type slot for the specified member and type.  This may return true and return a null slot - that indicates
            /// that a cached result for a member which doesn't exist has been stored.  Otherwise it returns true if a slot is found or
            /// false if it is not.
            /// </summary>
            public bool TryGetCachedSlot(Type/*!*/ type, string/*!*/ name, out PythonTypeSlot slot) {
                Debug.Assert(type != null); Debug.Assert(name != null);

                if (_cachedInfos != null) {
                    lock (_cachedInfos) {
                        SlotCacheInfo slots;
                        if (_cachedInfos.TryGetValue(type, out slots) && 
                            (slots.TryGetSlot(name, out slot) || slots.ResolvedAll)) {
                            return true;
                        }
                    }
                }

                slot = null;
                return false;
            }

            /// <summary>
            /// Looks up a cached member group for the specified member and type.  This may return true and return a null group - that indicates
            /// that a cached result for a member which doesn't exist has been stored.  Otherwise it returns true if a group is found or
            /// false if it is not.
            /// </summary>
            public bool TryGetCachedMember(Type/*!*/ type, string/*!*/ name, bool getMemberAction, out MemberGroup/*!*/ group) {
                Debug.Assert(type != null); Debug.Assert(name != null);

                if (_cachedInfos != null) {
                    lock (_cachedInfos) {
                        SlotCacheInfo slots;
                        if (_cachedInfos.TryGetValue(type, out slots) &&
                            (slots.TryGetMember(name, out group) || (getMemberAction && slots.ResolvedAll && !TypeInfo.IsBackwardsCompatabileName(name)))) {
                            return true;
                        }
                    }
                }

                group = MemberGroup.EmptyGroup;
                return false;
            }

            /// <summary>
            /// Checks to see if all members have been populated for the provided type.
            /// </summary>
            public bool IsFullyCached(Type/*!*/ type) {
                if (_cachedInfos != null) {
                    lock (_cachedInfos) {
                        SlotCacheInfo info;
                        if (_cachedInfos.TryGetValue(type, out info)) {
                            return info.ResolvedAll;
                        }
                    }
                }
                return false;
            }

            /// <summary>
            /// Populates the type with all the provided members and marks the type 
            /// as being fully cached.
            /// 
            /// The dictionary is used for the internal storage and should not be modified after
            /// providing it to the cache.
            /// </summary>
            public void CacheAll(Type/*!*/ type, Dictionary<string/*!*/, KeyValuePair<PythonTypeSlot/*!*/, MemberGroup/*!*/>> members) {
                Debug.Assert(type != null);

                EnsureInfo();

                lock (_cachedInfos) {
                    SlotCacheInfo slots = GetSlotForType(type);

                    slots.Members = members;
                    slots.ResolvedAll = true;
                }
            }

            /// <summary>
            /// Returns an enumerable object which provides access to all the members of the provided type.
            /// 
            /// The caller must check that the type is fully cached and populate the cache if it isn't before
            /// calling this method.
            /// </summary>
            public IEnumerable<KeyValuePair<string/*!*/, PythonTypeSlot/*!*/>>/*!*/ GetAllMembers(Type/*!*/ type) {
                Debug.Assert(type != null);

                SlotCacheInfo info = GetSlotForType(type);
                Debug.Assert(info.ResolvedAll);

                foreach (KeyValuePair<string, PythonTypeSlot> slot in info.GetAllSlots()) {
                    if (slot.Value != null) {
                        yield return slot;
                    }
                }
            }

            private SlotCacheInfo/*!*/ GetSlotForType(Type/*!*/ type) {
                SlotCacheInfo slots;
                if (!_cachedInfos.TryGetValue(type, out slots)) {
                    _cachedInfos[type] = slots = new SlotCacheInfo();
                }
                return slots;
            }

            private void EnsureInfo() {
                if (_cachedInfos == null) {
                    Interlocked.CompareExchange(ref _cachedInfos, new Dictionary<Type, SlotCacheInfo>(), null);
                }
            }

            private class SlotCacheInfo {
                public SlotCacheInfo() {
                    Members = new Dictionary<string/*!*/, KeyValuePair<PythonTypeSlot, MemberGroup/*!*/>>();
                }

                public void Add(string/*!*/ name, PythonTypeSlot slot, MemberGroup/*!*/ group) {
                    Debug.Assert(name != null);  Debug.Assert(group != null);

                    Members[name] = new KeyValuePair<PythonTypeSlot, MemberGroup>(slot, group);
                }

                public bool TryGetSlot(string/*!*/ name, out PythonTypeSlot slot) {
                    Debug.Assert(name != null);

                    KeyValuePair<PythonTypeSlot, MemberGroup> kvp;
                    if (Members.TryGetValue(name, out kvp)) {
                        slot = kvp.Key;
                        return true;
                    }

                    slot = null;
                    return false;
                }

                public bool TryGetMember(string/*!*/ name, out MemberGroup/*!*/ group) {
                    Debug.Assert(name != null);

                    KeyValuePair<PythonTypeSlot, MemberGroup> kvp;
                    if (Members.TryGetValue(name, out kvp)) {
                        group = kvp.Value;
                        return true;
                    }

                    group = MemberGroup.EmptyGroup;
                    return false;
                }

                public IEnumerable<KeyValuePair<string/*!*/, PythonTypeSlot>>/*!*/ GetAllSlots() {
                    foreach (KeyValuePair<string, KeyValuePair<PythonTypeSlot, MemberGroup>> kvp in Members) {
                        yield return new KeyValuePair<string, PythonTypeSlot>(kvp.Key, kvp.Value.Key);
                    }
                }

                public Dictionary<string/*!*/, KeyValuePair<PythonTypeSlot, MemberGroup/*!*/>>/*!*/ Members;
                public bool ResolvedAll;
            }
        }
    }
}
