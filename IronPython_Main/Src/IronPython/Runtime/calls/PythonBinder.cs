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

        public PythonBinder(CodeContext context)
            : base(context) {
        }

        private StandardRule<T> MakeRuleWorker<T>(CodeContext context, Action action, object[] args) {
            switch (action.Kind) {
                case ActionKind.DoOperation:
                    return new DoOperationBinderHelper<T>(this, context, (DoOperationAction)action).MakeRule(args);
                case ActionKind.GetMember:
                    return new PythonGetMemberBinderHelper<T>(context, (GetMemberAction)action).MakeRule(args);
                case ActionKind.SetMember:
                    return null;    // default implementation is good enough.
                case ActionKind.Call:
                    return new PythonCallBinderHelper<T>(context, (CallAction)action, args).MakeRule();
                default:
                    throw new NotImplementedException(action.ToString());
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

            BoundExpression be = expr as BoundExpression;
            if (be != null && be.Variable.KnownType != null) {
                if (toType.IsAssignableFrom(be.Variable.KnownType)) {
                    return Ast.Cast(expr, toType);
                }
            }

            // We used to have a special case for int -> double...
            if (exprType != typeof(object)) {
                expr = Ast.Cast(expr, typeof(object));
            }

            if (toType == typeof(object)) return expr;

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
            
            return Ast.Condition(
                Ast.TypeIs(
                    expr,
                    toType),
                Ast.Cast(
                    expr,
                    toType),
                Ast.Cast(
                    Ast.Call(
                        null, GetGenericConvertMethod(toType),
                        expr, Ast.Constant(toType.TypeHandle)
                    ),
                    toType
                )
            );
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


        #region .NET member binding

        public override MemberInfo[] GetMember(Type type, string name) {
            // Python type customization:
            switch (name) {
                case "__str__":
                    MethodInfo tostr = type.GetMethod("ToString", ReflectionUtils.EmptyTypes);
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
            MemberInfo[] res = base.GetMember(type, name);
            if (res.Length > 0) {
                return res;
            }
            
            if (ScriptDomainManager.Options.PrivateBinding) {
                // in private binding mode Python exposes private members under a mangled name.
                string header = "_" + type.Name + "__";
                if (name.StartsWith(header)) {
                    string memberName = name.Substring(header.Length);
                    const BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                    
                    res = type.GetMember(memberName, bf);
                    if (res.Length > 0) {
                        return FilterFieldAndEvent(res);
                    }
                    
                    res = type.GetMember(memberName, BindingFlags.FlattenHierarchy | bf);
                    if (res.Length > 0) {
                        return FilterFieldAndEvent(res);
                    }
                }
            }

            // Python exposes protected members as public            
            res = ArrayUtils.FindAll(type.GetMember(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic), ProtectedOnly);
            if (res.Length > 0) {
                return res;
            }

            // try alternate mapping to support backwards compatibility of calling extension methods.
            EnsureMemberMapping();
            string[] newNames;
            if (_memberMapping.TryGetValue(name, out newNames)) {
                List<MemberInfo> oldRes = new List<MemberInfo>();
                foreach (string newName in newNames) {
                    oldRes.AddRange(base.GetMember(type, newName));
                }
                return oldRes.ToArray();
            }

            return res;
        }

        public override Expression MakeMissingMemberError(Type type, string name) {
            return Ast.New(
                typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                Ast.Constant(String.Format("'{0}' object has no attribute '{1}'", DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(type)), name))
            );
        }

        public override Expression MakeReadOnlyMemberError(Type type, string name) {
            return Ast.New(
                typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                Ast.Constant(
                    String.Format("attribute '{0}' of '{1}' object is read-only", 
                        name,
                        DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(type))
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
        private MemberInfo[] FilterFieldAndEvent(MemberInfo []members) {
            MemberTypes mt = 0;
            foreach (MemberInfo mi in members) {
                mt |= mi.MemberType;
            }

            if (mt == (MemberTypes.Event | MemberTypes.Field)) {
                List<MemberInfo> res = new List<MemberInfo>();
                foreach (MemberInfo mi in members) {
                    if (mi.MemberType == MemberTypes.Event) {
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
    }
}
