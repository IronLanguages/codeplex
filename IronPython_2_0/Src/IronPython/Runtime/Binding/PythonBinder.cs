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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Microsoft.Scripting;
using Microsoft.Scripting.Com;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;

    public class PythonBinder : DefaultBinder {
        private PythonContext/*!*/ _context;
        private SlotCache/*!*/ _typeMembers = new SlotCache();
        private SlotCache/*!*/ _resolvedMembers = new SlotCache();
        private Dictionary<Type/*!*/, IList<Type/*!*/>/*!*/>/*!*/ _dlrExtensionTypes = MakeExtensionTypes();
        private readonly OldGetMemberAction EmptyGetMemberAction;

        [MultiRuntimeAware]
        private static readonly Dictionary<Type/*!*/, ExtensionTypeInfo/*!*/>/*!*/ _sysTypes = MakeSystemTypes();

        public PythonBinder(ScriptDomainManager manager, PythonContext/*!*/ pythonContext, CodeContext context)
            : base(manager) {
            ContractUtils.RequiresNotNull(pythonContext, "pythonContext");

            _context = pythonContext;
            if (context != null) {
                context.LanguageContext.DomainManager.AssemblyLoaded += new EventHandler<AssemblyLoadedEventArgs>(DomainManager_AssemblyLoaded);

                foreach (Assembly asm in pythonContext.DomainManager.GetLoadedAssemblyList()) {
                    DomainManager_AssemblyLoaded(this, new AssemblyLoadedEventArgs(asm));
                }
            }

            EmptyGetMemberAction = OldGetMemberAction.Make(this, String.Empty);
        }

        public override Expression/*!*/ ConvertExpression(Expression/*!*/ expr, Type/*!*/ toType, ConversionResultKind kind, Expression context) {
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

            return Binders.Convert(
                context,
                _context.DefaultBinderState,
                visType,
                visType == typeof(char) ? ConversionResultKind.ImplicitCast : kind,
                expr
            );
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

        public override bool CanConvertFrom(Type fromType, Type toType, bool toNotNullable, NarrowingLevel level) {
            return Converter.CanConvertFrom(fromType, toType, level);
        }

        public override Candidate PreferConvert(Type t1, Type t2) {
            return Converter.PreferConvert(t1, t2);
        }

        public override Expression GetByRefArrayExpression(Expression argumentArrayExpression) {
            return Ast.Call(typeof(PythonOps).GetMethod("MakeTuple"), argumentArrayExpression);
        }

        public override ErrorInfo MakeConversionError(Type toType, Expression value) {
            return ErrorInfo.FromException(
                Ast.Call(
                    typeof(PythonOps).GetMethod("TypeErrorForTypeMismatch"),
                    Ast.Constant(DynamicHelpers.GetPythonTypeFromType(toType).Name),
                    Ast.ConvertHelper(value, typeof(object))
               )
            );
        }

        public override ErrorInfo/*!*/ MakeNonPublicMemberGetError(Expression codeContext, MemberTracker member, Type type, Expression instance) {
            if (PrivateBinding) {
                return base.MakeNonPublicMemberGetError(codeContext, member, type, instance);
            }

            return ErrorInfo.FromValue(
                BindingHelpers.TypeErrorForProtectedMember(type, member.Name)
            );
        }

        public override ErrorInfo/*!*/ MakeStaticAssignFromDerivedTypeError(Type accessingType, MemberTracker info, Expression assignedValue, Expression context) {
            return MakeMissingMemberError(accessingType, info.Name);
        }

        public override ErrorInfo/*!*/ MakeStaticPropertyInstanceAccessError(PropertyTracker/*!*/ tracker, bool isAssignment, IList<Expression/*!*/>/*!*/ parameters) {
            ContractUtils.RequiresNotNull(tracker, "tracker");
            ContractUtils.RequiresNotNull(parameters, "parameters");
            ContractUtils.RequiresNotNullItems(parameters, "parameters");

            if (isAssignment) {
                return ErrorInfo.FromException(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("StaticAssignmentFromInstanceError"),
                        Ast.Constant(tracker),
                        Ast.Constant(isAssignment)
                    )
                );
            }

            return ErrorInfo.FromValue(
                Ast.Property(
                    null,
                    tracker.GetGetMethod(DomainManager.Configuration.PrivateBinding)
                )
            );
        }

        #region .NET member binding

        protected override string GetTypeName(Type t) {
            return DynamicHelpers.GetPythonTypeFromType(t).Name;
        }

        public override MemberGroup/*!*/ GetMember(OldDynamicAction action, Type type, string name) {
            MemberGroup mg;
            if (!_resolvedMembers.TryGetCachedMember(type, name, action.Kind == DynamicActionKind.GetMember, out mg)) {
                mg = TypeInfo.GetMemberAll(
                    this,
                    action,
                    type,
                    name);

                _resolvedMembers.CacheSlot(type, name, PythonTypeOps.GetSlot(mg, name, PrivateBinding), mg);
            }

            return mg ?? MemberGroup.EmptyGroup;
        }

        public override ErrorInfo/*!*/ MakeEventValidation(MemberGroup/*!*/ members, Expression eventObject, Expression/*!*/ value, Expression/*!*/ codeContext) {
            EventTracker ev = (EventTracker)members[0];

            return ErrorInfo.FromValueNoError(
               Ast.Call(
                   typeof(PythonOps).GetMethod("SlotTrySetValue"),
                   codeContext,
                   Ast.Constant(PythonTypeOps.GetReflectedEvent(ev)),
                   eventObject != null ? Ast.ConvertHelper(eventObject, typeof(object)) : Ast.Null(),
                   Ast.Null(typeof(PythonType)),
                   Ast.ConvertHelper(value, typeof(object))
               )
            );
        }

        public override ErrorInfo MakeEventValidation(RuleBuilder rule, MemberGroup members) {
            EventTracker ev = (EventTracker)members[0];

            return ErrorInfo.FromValueNoError(
               Ast.Call(
                   typeof(PythonOps).GetMethod("SlotTrySetValue"),
                   rule.Context,
                   Ast.Constant(PythonTypeOps.GetReflectedEvent(ev)),
                   Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                   Ast.Null(typeof(PythonType)),
                   Ast.ConvertHelper(rule.Parameters[1], typeof(object))
               )
            );
        }

        public override ErrorInfo MakeMissingMemberError(Type type, string name) {
            string typeName;
            if (typeof(TypeTracker).IsAssignableFrom(type)) {
                typeName = "type";
            } else {
                typeName = NameConverter.GetTypeName(type);
            }

            return ErrorInfo.FromException(
                Ast.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Constant(String.Format("'{0}' object has no attribute '{1}'", typeName, name))
                )
            );
        }

        /// <summary>
        /// Provides a way for the binder to provide a custom error message when lookup fails.  Just
        /// doing this for the time being until we get a more robust error return mechanism.
        /// </summary>
        public override ErrorInfo MakeReadOnlyMemberError(Type type, string name) {
            return ErrorInfo.FromException(
                Ast.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Constant(
                        String.Format("attribute '{0}' of '{1}' object is read-only",
                            name,
                            NameConverter.GetTypeName(type)
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Provides a way for the binder to provide a custom error message when lookup fails.  Just
        /// doing this for the time being until we get a more robust error return mechanism.
        /// </summary>
        public override ErrorInfo MakeUndeletableMemberError(Type type, string name) {
            return ErrorInfo.FromException(
                Ast.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Constant(
                        String.Format("cannot delete attribute '{0}' of builtin type '{1}'",
                            name,
                            NameConverter.GetTypeName(type)
                        )
                    )
                )
            );
        }

        #endregion

        internal IList<Type> GetExtensionTypesInternal(Type t) {
            List<Type> res = new List<Type>(base.GetExtensionTypes(t));

            AddExtensionTypes(t, res);

            return res.ToArray();
        }

        public override IList<Type> GetExtensionTypes(Type t) {
            List<Type> list = new List<Type>();

            // Python includes the types themselves so we can use extension properties w/ CodeContext
            list.Add(t);

            list.AddRange(base.GetExtensionTypes(t));

            AddExtensionTypes(t, list);

            return list;
        }

        private void AddExtensionTypes(Type t, List<Type> list) {
            ExtensionTypeInfo extType;
            if (_sysTypes.TryGetValue(t, out extType)) {
                list.Add(extType.ExtensionType);
            }

            IList<Type> userExtensions;
            lock (_dlrExtensionTypes) {
                if (_dlrExtensionTypes.TryGetValue(t, out userExtensions)) {
                    list.AddRange(userExtensions);
                }

                if (t.IsGenericType) {
                    // search for generic extensions, e.g. ListOfTOps<T> for List<T>,
                    // we then make a new generic type out of the extension type.
                    Type typeDef = t.GetGenericTypeDefinition();
                    Type[] args = t.GetGenericArguments();

                    if (_dlrExtensionTypes.TryGetValue(typeDef, out userExtensions)) {
                        foreach (Type genExtType in userExtensions) {
                            list.Add(genExtType.MakeGenericType(args));
                        }
                    }
                }
            }
        }

        public bool HasExtensionTypes(Type t) {
            return _dlrExtensionTypes.ContainsKey(t);
        }

        public override Expression ReturnMemberTracker(Type type, MemberTracker memberTracker) {
            Expression res = ReturnMemberTracker(type, memberTracker, PrivateBinding);

            return res ?? base.ReturnMemberTracker(type, memberTracker);
        }

        private static Expression ReturnMemberTracker(Type type, MemberTracker memberTracker, bool privateBinding) {
            switch (memberTracker.MemberType) {
                case TrackerTypes.TypeGroup:
                    return Ast.Constant(memberTracker);
                case TrackerTypes.Type:
                    return ReturnTypeTracker((TypeTracker)memberTracker);
                case TrackerTypes.Bound:
                    return ReturnBoundTracker((BoundMemberTracker)memberTracker, privateBinding);
                case TrackerTypes.Property:
                    return ReturnPropertyTracker((PropertyTracker)memberTracker, privateBinding);
                case TrackerTypes.Event:
                    return Ast.Call(
                        typeof(PythonOps).GetMethod("MakeBoundEvent"),
                        Ast.Constant(PythonTypeOps.GetReflectedEvent((EventTracker)memberTracker)),
                        Ast.Null(),
                        Ast.Constant(type)
                    );
                case TrackerTypes.Field:
                    return ReturnFieldTracker((FieldTracker)memberTracker);
                case TrackerTypes.MethodGroup:
                    return ReturnMethodGroup((MethodGroup)memberTracker);
                case TrackerTypes.Constructor:
                    MethodBase[] ctors = CompilerHelpers.GetConstructors(type, privateBinding, true);
                    object val;
                    if (PythonTypeOps.IsDefaultNew(ctors)) {
                        if (IsPythonType(type)) {
                            val = InstanceOps.New;
                        } else {
                            val = InstanceOps.NewCls;
                        }
                    } else {
                        val = PythonTypeOps.GetConstructor(type, InstanceOps.NonDefaultNewInst, ctors);
                    }

                    return Ast.Constant(val);
                case TrackerTypes.Custom:
                    return Ast.Constant(((PythonCustomTracker)memberTracker).GetSlot(), typeof(PythonTypeSlot));
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

            return TryLookupProtectedSlot(context, type, name, out slot);
        }

        /// <summary>
        /// Performs .NET member resolution.  This looks within the given type and also
        /// includes any extension members.  Base classes and their extension members are 
        /// not searched.
        /// 
        /// This version allows PythonType's for protected member resolution.  It shouldn't
        /// be called externally for other purposes.
        /// </summary>
        internal bool TryLookupProtectedSlot(CodeContext/*!*/ context, PythonType/*!*/ type, SymbolId name, out PythonTypeSlot slot) {
            string strName = SymbolTable.IdToString(name);
            Type curType = type.UnderlyingSystemType;

            if (!_typeMembers.TryGetCachedSlot(curType, strName, out slot)) {
                MemberGroup mg = TypeInfo.GetMember(
                    this,
                    OldGetMemberAction.Make(this, name),
                    curType,
                    strName);

                slot = PythonTypeOps.GetSlot(mg, SymbolTable.IdToString(name), PrivateBinding);

                _typeMembers.CacheSlot(curType, strName, slot, mg);
            }

            if (slot != null && (slot.IsAlwaysVisible || PythonOps.IsClsVisible(context))) {
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
                    OldGetMemberAction.Make(this, strName),
                    curType,
                    strName);

                slot = PythonTypeOps.GetSlot(mg, SymbolTable.IdToString(name), PrivateBinding);

                _resolvedMembers.CacheSlot(curType, strName, slot, mg);
            }

            if (slot != null && (slot.IsAlwaysVisible || PythonOps.IsClsVisible(context))) {
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
                        members[rm.Name] = new KeyValuePair<PythonTypeSlot, MemberGroup>(
                            PythonTypeOps.GetSlot(rm.Member, rm.Name, PrivateBinding), 
                            rm.Member
                        );
                    }
                }

                _typeMembers.CacheAll(type.UnderlyingSystemType, members);
            }

            foreach (KeyValuePair<string, PythonTypeSlot> kvp in _typeMembers.GetAllMembers(type.UnderlyingSystemType)) {
                PythonTypeSlot slot = kvp.Value;
                string name = kvp.Key;

                if (slot.IsAlwaysVisible || PythonOps.IsClsVisible(context)) {
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
                        members[rm.Name] = new KeyValuePair<PythonTypeSlot, MemberGroup>(
                            PythonTypeOps.GetSlot(rm.Member, rm.Name, PrivateBinding), 
                            rm.Member
                        );
                    }
                }

                _resolvedMembers.CacheAll(type.UnderlyingSystemType, members);
            }

            foreach (KeyValuePair<string, PythonTypeSlot> kvp in _resolvedMembers.GetAllMembers(type.UnderlyingSystemType)) {
                PythonTypeSlot slot = kvp.Value;
                string name = kvp.Key;

                if (slot.IsAlwaysVisible || PythonOps.IsClsVisible(context)) {
                    memberNames[name] = name;
                }
            }
        }

        private static Expression ReturnFieldTracker(FieldTracker fieldTracker) {
            return Ast.Constant(PythonTypeOps.GetReflectedField(fieldTracker.Field));
        }

        private static Expression ReturnMethodGroup(MethodGroup methodGroup) {
            return Ast.Constant(PythonTypeOps.GetFinalSlotForFunction(GetBuiltinFunction(methodGroup)));
        }

        private static Expression ReturnBoundTracker(BoundMemberTracker boundMemberTracker, bool privateBinding) {
            MemberTracker boundTo = boundMemberTracker.BoundTo;
            switch (boundTo.MemberType) {
                case TrackerTypes.Property:
                    PropertyTracker pt = (PropertyTracker)boundTo;
                    Debug.Assert(pt.GetIndexParameters().Length > 0);
                    return Ast.New(
                        typeof(ReflectedIndexer).GetConstructor(new Type[] { typeof(ReflectedIndexer), typeof(object) }),
                        Ast.Constant(new ReflectedIndexer(((ReflectedPropertyTracker)pt).Property, NameType.Property, privateBinding)),
                        boundMemberTracker.Instance
                    );
                case TrackerTypes.Event:
                    return Ast.Call(
                        typeof(PythonOps).GetMethod("MakeBoundEvent"),
                        Ast.Constant(PythonTypeOps.GetReflectedEvent((EventTracker)boundMemberTracker.BoundTo)),
                        boundMemberTracker.Instance,
                        Ast.Constant(boundMemberTracker.DeclaringType)
                    );
                case TrackerTypes.MethodGroup:
                    return Ast.Call(
                        typeof(PythonOps).GetMethod("MakeBoundBuiltinFunction"),
                        Ast.Constant(GetBuiltinFunction((MethodGroup)boundTo)),
                        Ast.ConvertHelper(
                            boundMemberTracker.Instance,
                            typeof(object)
                        )
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

        private static Expression ReturnPropertyTracker(PropertyTracker propertyTracker, bool privateBinding) {
            return Ast.Constant(PythonTypeOps.GetReflectedProperty(propertyTracker, null, privateBinding));
        }

        private static Expression ReturnTypeTracker(TypeTracker memberTracker) {
            // all non-group types get exposed as PythonType's
            return Ast.Constant(DynamicHelpers.GetPythonTypeFromType(memberTracker.Type));
        }

        protected override bool AllowKeywordArgumentSetting(MethodBase method) {
            return CompilerHelpers.IsConstructor(method) && !method.DeclaringType.IsDefined(typeof(PythonTypeAttribute), true);
        }

        internal ScriptDomainManager/*!*/ DomainManager {
            get {
                return _context.DomainManager;
            }
        }

        private class ExtensionTypeInfo {
            public Type ExtensionType;
            public string PythonName;

            public ExtensionTypeInfo(Type extensionType, string pythonName) {
                ExtensionType = extensionType;
                PythonName = pythonName;
            }
        }

        internal static void AssertNotExtensionType(Type t) {
            foreach (ExtensionTypeInfo typeinfo in _sysTypes.Values) {
                Debug.Assert(typeinfo.ExtensionType != t);
            }

            Debug.Assert(t != typeof(InstanceOps));
        }

        /// <summary>
        /// Creates the initial table of extension types.  These are standard extension that we apply
        /// to well known .NET types to make working with them better.  Being added to this table does
        /// not make a type a Python type though so that it's members are generally accessible w/o an
        /// import clr and their type is not re-named.
        /// </summary>
        private static Dictionary<Type/*!*/, IList<Type/*!*/>/*!*/>/*!*/ MakeExtensionTypes() {
            Dictionary<Type, IList<Type>> res = new Dictionary<Type, IList<Type>>();

            res[typeof(DBNull)] = new Type[] { typeof(DBNullOps) };
            res[typeof(List<>)] = new Type[] { typeof(ListOfTOps<>) };
            res[typeof(Dictionary<,>)] = new Type[] { typeof(DictionaryOfTOps<,>) };
            res[typeof(Array)] = new Type[] { typeof(ArrayOps) };
            res[typeof(Assembly)] = new Type[] { typeof(PythonAssemblyOps) };
            res[typeof(Enum)] = new Type[] { typeof(EnumOps) };
            res[typeof(Delegate)] = new Type[] { typeof(DelegateOps) };
            res[typeof(Byte)] = new Type[] { typeof(ByteOps) };
            res[typeof(SByte)] = new Type[] { typeof(SByteOps) };
            res[typeof(Int16)] = new Type[] { typeof(Int16Ops) };
            res[typeof(UInt16)] = new Type[] { typeof(UInt16Ops) };
            res[typeof(UInt32)] = new Type[] { typeof(UInt32Ops) };
            res[typeof(Int64)] = new Type[] { typeof(Int64Ops) };
            res[typeof(UInt64)] = new Type[] { typeof(UInt64Ops) };
            res[typeof(char)] = new Type[] { typeof(CharOps) };
            res[typeof(decimal)] = new Type[] { typeof(DecimalOps) };
            res[typeof(float)] = new Type[] { typeof(SingleOps) };

            return res;
        }

        /// <summary>
        /// Creates a table of standard .NET types which are also standard Python types.  These types have a standard
        /// set of extension types which are shared between all runtimes.
        /// </summary>
        private static Dictionary<Type/*!*/, ExtensionTypeInfo/*!*/>/*!*/ MakeSystemTypes() {
            Dictionary<Type/*!*/, ExtensionTypeInfo/*!*/> res = new Dictionary<Type, ExtensionTypeInfo>();

            // Native CLR types
            res[typeof(object)] = new ExtensionTypeInfo(typeof(ObjectOps), "object");
            res[typeof(string)] = new ExtensionTypeInfo(typeof(StringOps), "str");
            res[typeof(int)] = new ExtensionTypeInfo(typeof(Int32Ops), "int");
            res[typeof(bool)] = new ExtensionTypeInfo(typeof(BoolOps), "bool");
            res[typeof(double)] = new ExtensionTypeInfo(typeof(DoubleOps), "float");
            res[typeof(ValueType)] = new ExtensionTypeInfo(typeof(ValueType), "ValueType");   // just hiding it's methods in the inheritance hierarchy

            // MS.Math types
            res[typeof(BigInteger)] = new ExtensionTypeInfo(typeof(BigIntegerOps), "long");
            res[typeof(Complex64)] = new ExtensionTypeInfo(typeof(ComplexOps), "complex");

            // DLR types
            res[typeof(None)] = new ExtensionTypeInfo(typeof(NoneTypeOps), "NoneType");
            res[typeof(BaseSymbolDictionary)] = new ExtensionTypeInfo(typeof(DictionaryOps), "dict");
            res[typeof(IAttributesCollection)] = new ExtensionTypeInfo(typeof(DictionaryOps), "dict");
            res[typeof(NamespaceTracker)] = new ExtensionTypeInfo(typeof(ReflectedPackageOps), "namespace#");
            res[typeof(TypeGroup)] = new ExtensionTypeInfo(typeof(TypeGroupOps), "type-collision");
            res[typeof(TypeTracker)] = new ExtensionTypeInfo(typeof(TypeTrackerOps), "type-collision");
            res[typeof(Scope)] = new ExtensionTypeInfo(typeof(ScopeOps), "module");
            res[typeof(ScriptScope)] = new ExtensionTypeInfo(typeof(ScriptScopeOps), "module");
#if !SILVERLIGHT
            res[Type.GetType("System.__ComObject")] = new ExtensionTypeInfo(typeof(ComOps), "__ComObject");
#endif

            return res;
        }

        internal static string GetTypeNameInternal(Type t) {
            ExtensionTypeInfo extInfo;
            if (_sysTypes.TryGetValue(t, out extInfo)) {
                return extInfo.PythonName;
            }

            PythonTypeAttribute[] attrs = (PythonTypeAttribute[])t.GetCustomAttributes(typeof(PythonTypeAttribute), false);
            if (attrs.Length > 0 && attrs[0].Name != null) {
                return attrs[0].Name;
            }

            return t.Name;
        }

        public static bool IsExtendedType(Type/*!*/ t) {
            Debug.Assert(t != null);

            return _sysTypes.ContainsKey(t);
        }

        public static bool IsPythonType(Type/*!*/ t) {
            Debug.Assert(t != null);

            return _sysTypes.ContainsKey(t) || t.IsDefined(typeof(PythonTypeAttribute), false);
        }

        /// <summary>
        /// Event handler for when our domain manager has an assembly loaded by the user hosting the script
        /// runtime.  Here we can gather any information regarding extension methods.  
        /// 
        /// Currently DLR-style extension methods become immediately available w/o an explicit import step.
        /// </summary>
        private void DomainManager_AssemblyLoaded(object sender, AssemblyLoadedEventArgs e) {
            Assembly asm = e.Assembly;

            ExtensionTypeAttribute[] attrs = (ExtensionTypeAttribute[])asm.GetCustomAttributes(typeof(ExtensionTypeAttribute), true);

            if (attrs.Length > 0) {
                lock (_dlrExtensionTypes) {
                    foreach (ExtensionTypeAttribute attr in attrs) {
                        IList<Type> typeList;
                        if (!_dlrExtensionTypes.TryGetValue(attr.Extends, out typeList)) {
                            _dlrExtensionTypes[attr.Extends] = typeList = new List<Type>();
                        } else if (typeList.IsReadOnly) {
                            _dlrExtensionTypes[attr.Extends] = typeList = new List<Type>(typeList);
                        }

                        // don't add extension types twice even if we receive multiple assembly loads
                        if (!typeList.Contains(attr.ExtensionType)) {
                            typeList.Add(attr.ExtensionType);
                        }
                    }
                }
            }

#if !SILVERLIGHT // ComObject
            ComObjectWithTypeInfo.PublishComTypes(asm);
#endif

            // Add it to the references tuple if we
            // loaded a new assembly.
            ClrModule.ReferencesList rl = _context.ReferencedAssemblies;
            lock (rl) {
                rl.Add(asm);
            }

            // load any compiled code that has been cached...
            LoadScriptCode(_context, asm);

            // load any Python modules
            _context.LoadBuiltins(_context.Builtins, asm);

        }

        private static void LoadScriptCode(PythonContext/*!*/ pc, Assembly/*!*/ asm) {
            ScriptCode[] codes = ScriptCode.LoadFromAssembly(pc.DomainManager, asm);

            foreach (ScriptCode sc in codes) {
                pc.GetCompiledLoader().AddScriptCode(sc);
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

                    slots.Members[name] = new KeyValuePair<PythonTypeSlot, MemberGroup>(slot, memberGroup);
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
                            (slots.TryGetMember(name, out group) || (getMemberAction && slots.ResolvedAll))) {
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
                    Debug.Assert(name != null); Debug.Assert(group != null);

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
