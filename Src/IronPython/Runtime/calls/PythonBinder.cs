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
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

#if !SILVERLIGHT

using Microsoft.Scripting.Actions.ComDispatch;

#endif

namespace IronPython.Runtime.Calls {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using IronPython.Compiler.Generation;

    public class PythonBinder : ActionBinder {
        private PythonContext/*!*/ _context;
        [MultiRuntimeAware]
        private static Dictionary<string, string[]> _memberMapping;
        [MultiRuntimeAware]
        private static Dictionary<Type, Type> _extTypes = new Dictionary<Type, Type>();

        public PythonBinder(PythonContext/*!*/ pythonContext, CodeContext context)
            : base(context) {
            Contract.RequiresNotNull(pythonContext, "pythonContext");

            _context = pythonContext;
        }

        private StandardRule<T> MakeRuleWorker<T>(CodeContext/*!*/ context, DynamicAction/*!*/ action, object[]/*!*/ args) {
            switch (action.Kind) {
                case DynamicActionKind.DoOperation:
                    return new PythonDoOperationBinderHelper<T>(context, (DoOperationAction)action).MakeRule(args);
                case DynamicActionKind.GetMember:
                    return new PythonGetMemberBinderHelper<T>(context, (GetMemberAction)action, args).MakeRule();
                case DynamicActionKind.SetMember:
                    return new SetMemberBinderHelper<T>(context, (SetMemberAction)action, args).MakeNewRule();
                case DynamicActionKind.Call:
                    // if call fails Python will try and create an instance as it treats these two operations as the same.
                    StandardRule<T> rule = new PythonCallBinderHelper<T>(context, (CallAction)action, args).MakeRule();
                    if (rule == null) {
                        rule = base.MakeRule<T>(context, action, args);

                        if (rule.IsError) {
                            // try CreateInstance...
                            CreateInstanceAction createAct = PythonCallBinderHelper<T>.MakeCreateInstanceAction((CallAction)action);
                            StandardRule<T> newRule = GetRule<T>(context, createAct, args);
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

        protected override StandardRule<T> MakeRule<T>(CodeContext/*!*/ context, DynamicAction/*!*/ action, object[]/*!*/ args) {
            return MakeRuleWorker<T>(context, action, args) ?? base.MakeRule<T>(context, action, args);
        }

        public override Expression/*!*/ ConvertExpression(Expression/*!*/ expr, Type/*!*/ toType) {
            Contract.RequiresNotNull(expr, "expr");
            Contract.RequiresNotNull(toType, "toType");

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
                ConvertToAction.Make(visType, visType == typeof(char) ? ConversionResultKind.ImplicitCast : ConversionResultKind.ExplicitCast),
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
            Contract.RequiresNotNull(tracker, "tracker");
            Contract.RequiresNotNull(parameters, "parameters");
            Contract.RequiresNotNullItems(parameters, "parameters");

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

        public override MemberGroup GetMember(DynamicAction action, Type type, string name) {
            // Python type customization:
            switch (name) {
                case "__str__":
#if !SILVERLIGHT
                    if (!ComObject.Is__ComObject(type)) {
#else
                    if (true) {
#endif
                        MethodInfo tostr = type.GetMethod("ToString", Type.EmptyTypes);
                        if (tostr != null && tostr.DeclaringType != typeof(object)) {
                            return GetInstanceOpsMethod(type, "ToStringMethod");
                        }
                    }
                    break;
                case "__repr__":
#if !SILVERLIGHT
                    if (!ComObject.Is__ComObject(type)) {                    
#else
                    if (true) {
#endif
                        if (!typeof(ICodeFormattable).IsAssignableFrom(type) || type.IsInterface) {
                            // __repr__ for normal .NET types is special, if we have a real __repr__ we'll call it below
                                return GetInstanceOpsMethod(type, "FancyRepr");
                        }  
                    }
                    break;
                case "__init__":
                    // non-default init would have been handled by the Python binder.
                    return GetInstanceOpsMethod(type, "DefaultInit", "DefaultInitKW");
                case "__new__":
                    return new MemberGroup(type.GetConstructors());
                case "next":
                    if (typeof(IEnumerator).IsAssignableFrom(type)) {
                        return GetInstanceOpsMethod(type, "NextMethod");
                    }
                    break;
                case "__get__":
                    if (typeof(PythonTypeSlot).IsAssignableFrom(type)) {
                        return GetInstanceOpsMethod(type, "GetMethod");
                    }
                    break;
            }


            // normal binding
            MemberGroup res = base.GetMember(action, type, name);
            if (res.Count > 0) {
                lock (NewTypeMaker._overriddenMethods) {
                    Dictionary<string, List<MethodInfo>> methods;
                    if (NewTypeMaker._overriddenMethods.TryGetValue(type, out methods)) {
                        List<MethodInfo> methodList;
                        if (methods.TryGetValue(name, out methodList)) {
                            List<MemberTracker> members = new List<MemberTracker>(res.Count + methodList.Count);
                            members.AddRange(res);
                            foreach (MethodInfo mi in methodList) {
                                members.Add(MemberTracker.FromMemberInfo(mi));
                            }
                            res = new MemberGroup(members.ToArray());
                        }
                    }
                }
                return res;
            }

            if (type.IsInterface) {
                foreach (Type t in type.GetInterfaces()) {
                    res = GetMember(action, t, name);
                    if (res.Count > 0) {
                        return res;
                    }
                }
            }
            
            // try mapping __*__ methods to .NET method names
            OperatorMapping opMap;
            if (PythonExtensionTypeAttribute._pythonOperatorTable.TryGetValue(
                SymbolTable.StringToId(name),
                out opMap)) {

                if (IsUnfilterOperator(type, opMap)) {
                    OperatorInfo opInfo = OperatorInfo.GetOperatorInfo(opMap.Operator);
                    if (opInfo != null) {
                        res = base.GetMember(action, type, opInfo.Name);
                        if (res.Count > 0) {
                            return res;
                        }

                        res = base.GetMember(action, type, opInfo.AlternateName);
                        if (res.Count > 0) {
                            return res;
                        }
                    }
                }
            }

            if (_context.DomainManager.GlobalOptions.PrivateBinding) {
                // in private binding mode Python exposes private members under a mangled name.
                string header = "_" + type.Name + "__";
                if (name.StartsWith(header)) {
                    string memberName = name.Substring(header.Length);
                    const BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

                    res = new MemberGroup(type.GetMember(memberName, bf));
                    if (res.Count > 0) {
                        return FilterFieldAndEvent(res);
                    }

                    res = new MemberGroup(type.GetMember(memberName, BindingFlags.FlattenHierarchy | bf));
                    if (res.Count > 0) {
                        return FilterFieldAndEvent(res);
                    }
                }
            }

            // Python exposes protected members as public            
            res = new MemberGroup(ArrayUtils.FindAll(type.GetMember(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic), ProtectedOnly));
            if (res.Count > 0) {
                return res;
            }

            // try alternate mapping to support backwards compatibility of calling extension methods.
            EnsureMemberMapping();
            string[] newNames;
            if (_memberMapping.TryGetValue(name, out newNames)) {
                List<MemberTracker> oldRes = new List<MemberTracker>();
                foreach (string newName in newNames) {
                    oldRes.AddRange(base.GetMember(action, type, newName));
                }

                return new MemberGroup(oldRes.ToArray());
            }

            return res;
        }

        private static bool IsUnfilterOperator(Type type, OperatorMapping opMap) {
            if (type == typeof(int)) {
                // Python doesn't define __eq__ on int, .NET does
                return opMap.Operator != Operators.Equals;
            } else if (type == typeof(BigInteger)) {
                switch(opMap.Operator) {
                    // Python's big int only defines __cmp__
                    case Operators.LessThan:
                    case Operators.GreaterThan:
                    case Operators.Equals:
                    case Operators.NotEquals:
                    case Operators.LessThanOrEqual:
                    case Operators.GreaterThanOrEqual:
                        return false;
                }
            }

            return true;
        }

        public override ErrorInfo MakeEventValidation(StandardRule rule, MemberGroup members) {
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

        public override Expression MakeReadOnlyMemberError<T>(StandardRule<T> rule, Type type, string name) {
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

        public override Expression MakeUndeletableMemberError<T>(StandardRule<T> rule, Type type, string name) {
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

        private bool ProtectedOnly(MemberInfo input) {
            switch (input.MemberType) {
                case MemberTypes.Method:
                    return ((MethodInfo)input).IsFamily || ((MethodInfo)input).IsFamilyOrAssembly;
                case MemberTypes.Property:
                    MethodInfo mi = ((PropertyInfo)input).GetGetMethod(true);
                    if (mi != null) return ProtectedOnly(mi);
                    return false;
                case MemberTypes.Field:
                    return ((FieldInfo)input).IsFamily || ((FieldInfo)input).IsFamilyOrAssembly;
                default:
                    return false;
            }
        }

        /// <summary>
        /// When private binding is enabled we can have a collision between the private Event
        /// and private field backing the event.  We filter this out and favor the event.
        /// 
        /// This matches the v1.0 behavior of private binding.
        /// </summary>
        private MemberGroup FilterFieldAndEvent(MemberGroup members) {
            TrackerTypes mt = TrackerTypes.None;
            foreach (MemberTracker mi in members) {
                mt |= mi.MemberType;
            }

            if (mt == (TrackerTypes.Event | TrackerTypes.Field)) {
                List<MemberTracker> res = new List<MemberTracker>();
                foreach (MemberTracker mi in members) {
                    if (mi.MemberType == TrackerTypes.Event) {
                        res.Add(mi);
                    }
                }
                return new MemberGroup(res.ToArray());
            }
            return members;
        }

        private void EnsureMemberMapping() {
            if (_memberMapping != null) return;

            Dictionary<string, string[]> res = new Dictionary<string, string[]>();

            /* common object ops */
            AddMapping(res, "GetAttribute", "__getattribute__");
            AddMapping(res, "DelAttrMethod", "__delattr__");
            AddMapping(res, "SetAttrMethod", "__setattr__");
            AddMapping(res, "PythonToString", "__str__");
            AddMapping(res, "Hash", "__hash__");
            AddMapping(res, "Reduce", "__reduce__", "__reduce_ex__");
            AddMapping(res, "CodeRepresentation", "__repr__");

            AddMapping(res, "Add", "append");
            AddMapping(res, "Contains", "__contains__");
            AddMapping(res, "CompareTo", "__cmp__");
            AddMapping(res, "DelIndex", "__delitem__");
            AddMapping(res, "GetEnumerator", "__iter__");
            AddMapping(res, "Length", "__len__");
            AddMapping(res, "Clear", "clear");
            AddMapping(res, "Clone", "copy");
            AddMapping(res, "GetIndex", "get");
            AddMapping(res, "HasKey", "has_key");
            AddMapping(res, "Insert", "insert");
            AddMapping(res, "Items", "items");
            AddMapping(res, "IterItems", "iteritems");
            AddMapping(res, "IterKeys", "iterkeys");
            AddMapping(res, "IterValues", "itervalues");
            AddMapping(res, "Keys", "keys");
            AddMapping(res, "Pop", "pop");
            AddMapping(res, "PopItem", "popitem");
            AddMapping(res, "Remove", "remove");
            AddMapping(res, "RemoveAt", "pop");
            AddMapping(res, "SetDefault", "setdefault");
            AddMapping(res, "ToFloat", "__float__");
            AddMapping(res, "Values", "values");
            AddMapping(res, "Update", "update");

            Interlocked.Exchange(ref _memberMapping, res);
        }

        private void AddMapping(Dictionary<string, string[]> res, string name, params string[] names) {
            res[name] = names;
        }

        #endregion

        protected override IList<Type> GetExtensionTypes(Type t) {
            // Ensure that the type is initialized. If ReflectedTypeBuilder.RegisterAlternateBuilder was used,
            // the alternate builder will get a chance to initialize the type.
            DynamicHelpers.GetPythonTypeFromType(t);

            List<Type> list = new List<Type>();

            // Python includes the types themselves so we can use extension properties w/ CodeContext
            list.Add(t);

            Type res;
            if (_extTypes.TryGetValue(t, out res)) {
                list.Add(res);
            }

            list.AddRange(base.GetExtensionTypes(t));

            return list;
        }

        internal static void RegisterType(Type extended, Type extension) {
            _extTypes[extended] = extension;
        }

        public override Expression ReturnMemberTracker(Type type, MemberTracker memberTracker) {
            switch (memberTracker.MemberType) {
                case TrackerTypes.TypeGroup:
                    return Ast.RuntimeConstant(memberTracker);
                case TrackerTypes.Type:
                    return ReturnTypeTracker((TypeTracker)memberTracker);
                case TrackerTypes.Bound:
                    return ReturnBoundTracker((BoundMemberTracker)memberTracker);
                case TrackerTypes.Property:
                    return ReturnPropertyTracker((PropertyTracker)memberTracker);
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
                    ConstructorFunction cf = PythonTypeOps.GetConstructor(type, InstanceOps.NonDefaultNewInst, type.GetConstructors());
                    return Ast.RuntimeConstant(cf);

            }

            return base.ReturnMemberTracker(type, memberTracker);
        }

        private Expression ReturnFieldTracker(FieldTracker fieldTracker) {
            return Ast.RuntimeConstant(PythonTypeOps.GetReflectedField(fieldTracker.Field));
        }

        private Expression ReturnMethodGroup(MethodGroup methodGroup) {
            return Ast.RuntimeConstant(PythonTypeOps.GetMethodIfMethod(GetBuiltinFunction(methodGroup)));
        }

        private Expression ReturnBoundTracker(BoundMemberTracker boundMemberTracker) {
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

        private BuiltinFunction GetBuiltinFunction(MethodGroup mg) {
            return PythonTypeOps.GetBuiltinFunction(
                mg.DeclaringType,
                mg.Methods[0].Name,
                (mg.ContainsInstance ? FunctionType.Method : FunctionType.None) |
                (mg.ContainsStatic ? FunctionType.Function : FunctionType.None),
                mg.GetMethodBases()
            );            
        }

        private Expression ReturnPropertyTracker(PropertyTracker propertyTracker) {
            if (propertyTracker.GetIndexParameters().Length > 0) {
                return Ast.RuntimeConstant(new ReflectedIndexer(((ReflectedPropertyTracker)propertyTracker).Property, NameType.Property));
            }

            ReflectedPropertyTracker rpt = propertyTracker as ReflectedPropertyTracker;
            if (rpt != null) {
                return Ast.RuntimeConstant(new ReflectedProperty(rpt.Property,
                    IncludePropertyMethod(rpt.GetGetMethod(true)),
                    IncludePropertyMethod(rpt.GetSetMethod(true)),
                    NameType.Property));
            }

            return Ast.RuntimeConstant(new ReflectedExtensionProperty(
                new ExtensionPropertyInfo(
                    propertyTracker.DeclaringType,
                    IncludePropertyMethod(propertyTracker.GetGetMethod(true)) ??
                        IncludePropertyMethod(propertyTracker.GetSetMethod(true))
                ),
                NameType.Property)
            );
        }

        private MethodInfo IncludePropertyMethod(MethodInfo method) {
            if (_context.DomainManager.GlobalOptions.PrivateBinding) return method;

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
    }
}
