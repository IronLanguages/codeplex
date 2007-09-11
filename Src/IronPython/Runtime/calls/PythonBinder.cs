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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;

using TypeCache = IronPython.Runtime.Types.TypeCache;

namespace IronPython.Runtime.Calls {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using System.Threading;
using IronPython.Runtime.Operations;
    using IronPython.Runtime.Types;
    using Microsoft.Scripting.Utils;

    public class PythonBinder : ActionBinder {
        private static Dictionary<string, string[]> _memberMapping;
        private static Dictionary<Type, Type> _extTypes = new Dictionary<Type,Type>();

        public PythonBinder(CodeContext context)
            : base(context) {
        }

        private StandardRule<T> MakeRuleWorker<T>(CodeContext context, Action action, object[] args) {
            switch (action.Kind) {
                case ActionKind.DoOperation:
                    return new DoOperationBinderHelper<T>(this, context, (DoOperationAction)action).MakeRule(args);
                case ActionKind.GetMember:
                    return new PythonGetMemberBinderHelper<T>(context, (GetMemberAction)action, args).MakeRule();
                case ActionKind.Call:
                    // if call fails Python will try and create an instance as it treats these two operations as the same.
                    StandardRule<T> rule = new PythonCallBinderHelper<T>(context, (CallAction)action, args).MakeRule();
                    if (rule == null) {
                        rule = base.MakeRule<T>(context, action, args);
                        // if we know we're callable we won't produce a rule - eventually this interface goes away.
                        if (rule.IsError && !(args[0] is ICallableWithCodeContext)) {
                            // try CreateInstance...
                            CreateInstanceAction createAct = PythonCallBinderHelper<T>.MakeCreateInstanceAction((CallAction)action);
                            StandardRule<T> newRule = GetRule<T>(context, createAct, args);
                            if (!newRule.IsError) {
                                return newRule;
                            }
                        }
                    }
                    return rule;
                default:
                    return null;
            }
        }

        protected override StandardRule<T> MakeRule<T>(CodeContext context, Action action, object[] args) {
            return MakeRuleWorker<T>(context, action, args) ?? base.MakeRule<T>(context, action, args);
        }

        public override Expression ConvertExpression(Expression expr, Type toType) {
            Type exprType = expr.ExpressionType;

            if (toType == typeof(object)) {
                if (exprType.IsValueType) {
                    return Ast.Cast(expr, toType);
                } else {
                    return expr;
                }
            }

            if (toType.IsAssignableFrom(exprType)) {
                return expr;
            }

            Type extensibleType = typeof(Extensible<>).MakeGenericType(toType);
            if (extensibleType.IsAssignableFrom(exprType)) {
                return Ast.ReadProperty(expr, extensibleType.GetProperty("Value"));
            }

            // We used to have a special case for int -> double...
            if (exprType != typeof(object) && exprType.IsValueType) {
                expr = Ast.Cast(expr, typeof(object));
            }

            MethodInfo fastConvertMethod = GetFastConvertMethod(toType);
            if (fastConvertMethod != null) {
                return Ast.Call(null, fastConvertMethod, expr);
            }

            if (typeof(Delegate).IsAssignableFrom(toType)) {
                return Ast.Cast(
                    Ast.Call(
                        null,
                        typeof(Converter).GetMethod("ConvertToDelegate"),
                        expr,
                        Ast.Constant(toType)
                    ),
                    toType
                );
            }

            Expression typeIs;
            Type visType = CompilerHelpers.GetVisibleType(toType);
            if (toType.IsVisible) {
                typeIs = Ast.TypeIs(expr, toType);
            } else {
                typeIs = Ast.Call(Ast.RuntimeConstant(toType), typeof(Type).GetMethod("IsInstanceOfType"), expr);
            }

            return Ast.Condition(
                typeIs,
                Ast.Cast(
                    expr,
                    visType),
                Ast.Cast(
                    Ast.Call(
                        null, GetGenericConvertMethod(visType),
                        expr, Ast.Constant(visType.TypeHandle)
                    ),
                    visType
                )
            );                       
        }

        public override Expression CheckExpression(Expression expr, Type toType) {
            if (toType == typeof(object) || toType.IsAssignableFrom(toType)) {
                return Ast.Constant(true);
            }

            return Ast.Call(null, typeof(Converter).GetMethod("CanConvert"), expr, Ast.Constant(toType));
        }

        private static MethodInfo GetGenericConvertMethod(Type toType) {
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


        private static MethodInfo GetFastConvertMethod(Type toType) {
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


        /// <summary>
        /// TODO Something like this method belongs on the Binder; however, it is probably
        /// something much more abstract.  This is just the first pass at removing this
        /// to get rid of the custom PythonCodeGen.
        /// </summary>
        public override void EmitConvertFromObject(CodeGen cg, Type toType) {
            if (toType == typeof(object)) return;

            MethodInfo fastConvertMethod = GetFastConvertMethod(toType);
            if (fastConvertMethod != null) {
                cg.EmitCall(fastConvertMethod);
                return;
            }

            if (toType == typeof(void)) {
                cg.Emit(OpCodes.Pop);
            } else if (typeof(Delegate).IsAssignableFrom(toType)) {
                cg.EmitType(toType);
                cg.EmitCall(typeof(Converter), "ConvertToDelegate");
                cg.Emit(OpCodes.Castclass, toType);
            } else {
                Label end = cg.DefineLabel();
                cg.Emit(OpCodes.Dup);
                cg.Emit(OpCodes.Isinst, toType);

                cg.Emit(OpCodes.Brtrue_S, end);
                cg.Emit(OpCodes.Ldtoken, toType);
                cg.EmitCall(GetGenericConvertMethod(toType));
                cg.MarkLabel(end);

                cg.Emit(OpCodes.Unbox_Any, toType); //??? this check may be redundant
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
            return Tuple.MakeTuple(args);
        }


        public override Statement MakeInvalidParametersError(MethodBinder binder, Action action, CallType callType, MethodBase[] targets, StandardRule rule, object[] args) {
            if (binder.IsBinaryOperator) {
                CallAction ca = action as CallAction;
                if (ca != null) {
                    int argsReceived = args.Length - 1 + GetParamsArgumentCountAdjust(ca, args);
                    if (ca.HasDictionaryArgument()) {
                        argsReceived--;
                    }

                    foreach (MethodBase mb in targets) {
                        ParameterInfo[] pis = mb.GetParameters();
                        int argsNeeded = pis.Length;
                        if (mb.IsStatic && callType == CallType.ImplicitInstance) {
                            argsNeeded--;
                        }

                        // only return NotImplemented if we match on # of args
                        if (argsNeeded == argsReceived || (CompilerHelpers.IsParamsMethod(mb) && argsNeeded <= argsReceived)) {
                            return rule.MakeReturn(this, Ast.ReadField(null, typeof(PythonOps).GetField("NotImplemented")));
                        }
                    }
                }
            }

            return base.MakeInvalidParametersError(binder, action, callType, targets, rule, args);
        }

        #region .NET member binding

        public override MemberGroup GetMember(Action action, Type type, string name) {
            // Python type customization:
            switch (name) {
                case "__str__":
                    MethodInfo tostr = type.GetMethod("ToString", ArrayUtils.EmptyTypes);
                    if (tostr != null && tostr.DeclaringType != typeof(object)) {
                        return new MemberInfo[] { typeof(InstanceOps).GetMethod("ToStringMethod") };
                    }
                    break;
                case "__repr__":
                    if (typeof(ICodeFormattable).IsAssignableFrom(type) && !type.IsInterface) {
                        return new MemberInfo[] { typeof(InstanceOps).GetMethod("ReprHelper") };
                    }
                    return new MemberInfo[] { typeof(InstanceOps).GetMethod("FancyRepr") };
                case "__init__":
                    // non-default init would have been handled by the Python binder.
                    return new MemberInfo[] { typeof(InstanceOps).GetMethod("DefaultInit"), typeof(InstanceOps).GetMethod("DefaultInitKW") };
                case "next":
                    if (typeof(IEnumerator).IsAssignableFrom(type)) {
                        return new MemberInfo[] { typeof(InstanceOps).GetMethod("NextMethod") };
                    }
                    break;
                case "__get__":
                    if (typeof(DynamicTypeSlot).IsAssignableFrom(type)) {
                        return new MemberInfo[] { typeof(InstanceOps).GetMethod("GetMethod") };
                    }
                    break;
            }


            // normal binding
            MemberGroup res = base.GetMember(action, type, name);
            if (res.Count > 0) {
                return res;
            }
            
            if (ScriptDomainManager.Options.PrivateBinding) {
                // in private binding mode Python exposes private members under a mangled name.
                string header = "_" + type.Name + "__";
                if (name.StartsWith(header)) {
                    string memberName = name.Substring(header.Length);
                    const BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                    
                    res = type.GetMember(memberName, bf);
                    if (res.Count > 0) {
                        return FilterFieldAndEvent(res);
                    }
                    
                    res = type.GetMember(memberName, BindingFlags.FlattenHierarchy | bf);
                    if (res.Count > 0) {
                        return FilterFieldAndEvent(res);
                    }
                }
            }

            // Python exposes protected members as public            
            res = ArrayUtils.FindAll(type.GetMember(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic), ProtectedOnly);
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
                
                return oldRes.ToArray();
            }

            return res;
        }

        public override Statement MakeMissingMemberError<T>(StandardRule<T> rule, Type type, string name) {
            return rule.MakeError(this,
                Ast.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Constant(String.Format("'{0}' object has no attribute '{1}'", DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(type)), name))
                )
            );
        }

        public override Statement MakeReadOnlyMemberError<T>(StandardRule<T> rule, Type type, string name) {
            return rule.MakeError(this,
                Ast.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Constant(
                        String.Format("attribute '{0}' of '{1}' object is read-only", 
                            name,
                            DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(type))
                        )
                    )
                )
            );
        }

        public override Statement MakeUndeletableMemberError<T>(StandardRule<T> rule, Type type, string name) {
            return rule.MakeError(this,
                Ast.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Constant(
                        String.Format("cannot delete attribute '{0}' of builtin type '{1}'",
                            name,
                            DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(type))
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
                    if(mi != null) return ProtectedOnly(mi);
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
                return res.ToArray();
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

            AddMapping(res, "Contains", "__contains__");
            AddMapping(res, "CompareTo", "__cmp__");
            AddMapping(res, "DelIndex", "__delitem__");
            AddMapping(res, "GetEnumerator", "__iter__");
            AddMapping(res, "Length", "__len__");
            AddMapping(res, "Clear", "clear");
            AddMapping(res, "Clone", "copy");
            AddMapping(res, "GetIndex", "get");
            AddMapping(res, "HasKey", "has_key");
            AddMapping(res, "Items", "items");
            AddMapping(res, "IterItems", "iteritems");
            AddMapping(res, "IterKeys", "iterkeys");
            AddMapping(res, "IterValues", "itervalues");
            AddMapping(res, "Keys", "keys");
            AddMapping(res, "Pop", "pop");
            AddMapping(res, "PopItem", "popitem");
            AddMapping(res, "SetDefault", "setdefault");
            AddMapping(res, "Values", "values");
            AddMapping(res, "Update", "update");

            Interlocked.Exchange(ref _memberMapping, res);
        }

        private void AddMapping(Dictionary<string, string[]> res, string name, params string[] names) {
            res[name] = names;
        }

        #endregion

        protected override IList<Type> GetExtensionTypes(Type t) {
            Type res;
            if (_extTypes.TryGetValue(t, out res)) {
                List<Type> list = new List<Type>();
                list.Add(res);
                list.AddRange(base.GetExtensionTypes(t));
                return list;
            }

            return base.GetExtensionTypes(t);
        }

        internal static void RegisterType(Type extended, Type extension) {
            _extTypes[extended] = extension;
        }

        protected override bool AllowKeywordArgumentConstruction(Type t) {
            return !PythonTypeCustomizer.IsPythonType(t);
        }

        protected override Expression ReturnMemberTracker(MemberTracker memberTracker) {
            switch (memberTracker.MemberType) {
                case TrackerTypes.TypeGroup:
                    return Ast.RuntimeConstant(memberTracker);
                case TrackerTypes.Type:
                    return ReturnTypeTracker((TypeTracker)memberTracker);
                case TrackerTypes.MemberGroup:
                    // member group
                    return ReturnMemberGroup((MemberGroup)memberTracker);
            }

            return base.ReturnMemberTracker(memberTracker);
        }

        private static Expression ReturnTypeTracker(TypeTracker memberTracker) {
            // all non-group types get exposed as DynamicType's
            return Ast.RuntimeConstant(DynamicHelpers.GetDynamicTypeFromType(memberTracker.Type));
        }

        private Expression ReturnMemberGroup(MemberGroup memberGroup) {
            TrackerTypes types = TrackerTypes.None;
            foreach (MemberTracker mt in memberGroup) {
                types |= mt.MemberType;
            }

            switch (types) {
                case TrackerTypes.Event:
                    return Ast.RuntimeConstant(ReflectionCache.GetReflectedEvent(((EventTracker)memberGroup[0]).Event));
                case TrackerTypes.Field:
                    return Ast.RuntimeConstant(ReflectionCache.GetReflectedField(((FieldTracker)memberGroup[0]).Field));
                case TrackerTypes.Property:
                    PropertyTracker pt = (PropertyTracker)memberGroup[0];
                    ReflectedPropertyTracker rpt = pt as ReflectedPropertyTracker;
                    if (rpt != null) {
                        if (rpt.Property.GetIndexParameters().Length > 0) {
                            return Ast.RuntimeConstant(ReflectionCache.GetReflectedIndexer(rpt.Property));
                        } else {
                            return Ast.RuntimeConstant(new ReflectedProperty(rpt.Property, rpt.GetGetMethod(), rpt.GetSetMethod(), NameType.Property));
                        }
                    }

                    throw new InvalidOperationException();
                default:
                    return base.ReturnMemberTracker(memberGroup);
            }

        }
    }
}
