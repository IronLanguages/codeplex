/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.ComInterop;

namespace ComTest {
    class TestGetMember : GetMemberBinder {

        private readonly bool _delayComEval;
        private TestGetMember(string name, bool forceComEval)
            : base(name, false) {

            _delayComEval = forceComEval;
        }

        private static readonly Dictionary<string, TestGetMember> _bindersNoEval = new Dictionary<string, TestGetMember>();
        private static readonly Dictionary<string, TestGetMember> _bindersForceEval = new Dictionary<string, TestGetMember>();

        internal static TestGetMember Create(string name) {
            return Create(name, true);
        }
        internal static TestGetMember Create(string name, bool forceComEval) {
            var binders = forceComEval ? _bindersForceEval : _bindersNoEval;
            lock (binders) {
                TestGetMember result;
                if (binders.TryGetValue(name, out result)) {
                    return result;
                }
                return binders[name] = new TestGetMember(name, forceComEval);
            }
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject self, DynamicMetaObject onBindingError) {
            DynamicMetaObject com;
            if (ComBinder.TryBindGetMember(this, self, out com, _delayComEval)) {
                return com;
            }

            self = new DynamicMetaObject(
                Expression.Convert(self.Expression, self.LimitType),
                BindingRestrictions.GetTypeRestriction(self.Expression, self.LimitType)
            );

            MemberInfo[] mis = self.LimitType.GetMember(Name);
            if (mis.Length == 0) {
                Type[] ifaces = self.LimitType.GetInterfaces();
                for (int i = 0; i < ifaces.Length; i++) {
                    mis = ifaces[i].GetMember(Name);
                    if (mis.Length > 0) {
                        break;
                    }
                }
            }

            if (mis.Length > 0) {
                MemberInfo mi = mis[0];

                switch (mi.MemberType) {

                    case MemberTypes.Property:
                        return new DynamicMetaObject(
                            Expression.Property(self.Expression, (PropertyInfo)mi),
                            self.Restrictions
                        );
                    case MemberTypes.Field:
                        return new DynamicMetaObject(
                            Expression.Field(self.Expression, (FieldInfo)mi),
                            self.Restrictions
                        );
                    case MemberTypes.Method:
                        return new DynamicMetaObject(
                            Expression.New(typeof(CsMethod).GetConstructor(new Type[] { typeof(MethodInfo) }), Expression.Constant(mi)),
                            self.Restrictions
                        );
                }
            }

            return Utils.Error(typeof(InvalidOperationException), "No member " + Name, self.Restrictions);
        }
    }

    class TestSetMember : SetMemberBinder {
        private TestSetMember(string name)
            : base(name, false) {
        }

        private static readonly Dictionary<string, TestSetMember> _binders = new Dictionary<string, TestSetMember>();
        internal static TestSetMember Create(string name) {
            lock (_binders) {
                TestSetMember result;
                if (_binders.TryGetValue(name, out result)) {
                    return result;
                }
                return _binders[name] = new TestSetMember(name);
            }
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject self, DynamicMetaObject value, DynamicMetaObject onBindingError) {
            DynamicMetaObject com;
            if (ComBinder.TryBindSetMember(this, self, value, out com)) {
                return com;
            }

            self = new DynamicMetaObject(
                Expression.Convert(self.Expression, self.LimitType),
                BindingRestrictions.GetTypeRestriction(self.Expression, self.LimitType)
            );

            MemberInfo[] mis = self.LimitType.GetMember(Name);
            if (mis.Length == 0) {
                Type[] ifaces = self.LimitType.GetInterfaces();
                for (int i = 0; i < ifaces.Length; i++) {
                    mis = ifaces[i].GetMember(Name);
                    if (mis.Length > 0) {
                        break;
                    }
                }
            }

            if (mis.Length > 0) {
                MemberInfo mi = mis[0];

                switch (mi.MemberType) {
                    case MemberTypes.Property:
                        return new DynamicMetaObject(
                            Expression.Assign(Expression.Property(self.Expression, (PropertyInfo)mi), value.Expression),
                            self.Restrictions.Merge(value.Restrictions)
                        );
                    case MemberTypes.Field:
                        return new DynamicMetaObject(
                            Expression.Assign(Expression.Field(self.Expression, (FieldInfo)mi), value.Expression),
                            self.Restrictions.Merge(value.Restrictions)
                        );
                }
            }

            return Utils.Error(typeof(InvalidOperationException), "No member " + Name, self.Restrictions.Merge(value.Restrictions));
        }
    }

    class TestInvoke : InvokeBinder {
        private TestInvoke(Type siteType) :
            base(TypeToCallInfo(siteType)) {
        }

        private static readonly Dictionary<Type, TestInvoke> _binders = new Dictionary<Type, TestInvoke>();
        internal static TestInvoke Create(Type siteType) {
            lock (_binders) {
                TestInvoke result;
                if (_binders.TryGetValue(siteType, out result)) {
                    return result;
                }
                return _binders[siteType] = new TestInvoke(siteType);
            }
        }

        public static CallInfo TypeToCallInfo(Type dt) {
            var Invoke = dt.GetMethod("Invoke");

            ParameterInfo[] pis = Invoke.GetParameters();

            // skip CallSite and target arguments
            return new CallInfo(pis.Length - 2);
        }


        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
            DynamicMetaObject com;
            if (ComBinder.TryBindInvoke(this, target, args, out com)) {
                return com;
            }
            throw new NotImplementedException();
        }
    }

    class TestCall : InvokeMemberBinder {
        private TestCall(string name, Type siteType)
            // COM still supports the "missing" callinfo. We should probably
            // remove that, it's not supported in the interop anymore.
            // For now, we test the real thing languages will emit.
            //: base(name, false, new CallInfo(0)) {
            : base(name, false, TestInvoke.TypeToCallInfo(siteType)) {
        }

        private sealed class KeyComparer : IEqualityComparer<KeyValuePair<string, Type>>  {
            public bool Equals(KeyValuePair<string, Type> x, KeyValuePair<string, Type> y) {
                return x.Key == y.Key && x.Value == y.Value;
            }

            public int GetHashCode(KeyValuePair<string, Type> obj) {
                return obj.Key.GetHashCode() ^ obj.Value.GetHashCode();
            }
        }

        private static readonly Dictionary<KeyValuePair<string, Type>, TestCall> _binders = new Dictionary<KeyValuePair<string, Type>, TestCall>(new KeyComparer());
        internal static TestCall Create(string name, Type siteType) {
            lock (_binders) {
                var key = new KeyValuePair<string, Type>(name, siteType);
                TestCall call;
                if (_binders.TryGetValue(key, out call)) {
                    return call;
                }
                return _binders[key] = new TestCall(name, siteType);
            }
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
            DynamicMetaObject com;
            if (ComBinder.TryBindInvokeMember(this, target, args, out com)) {
                return com;
            }

            throw new NotImplementedException();
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
            throw new NotImplementedException();
        }
    }

    class TestSetIndex : SetIndexBinder {
        private TestSetIndex(Type siteType) :
            base(new CallInfo(TestInvoke.TypeToCallInfo(siteType).ArgumentCount - 1)) {
        }

        private static readonly Dictionary<Type, TestSetIndex> _binders = new Dictionary<Type, TestSetIndex>();
        internal static TestSetIndex Create(Type siteType) {
            lock (_binders) {
                TestSetIndex result;
                if (_binders.TryGetValue(siteType, out result)) {
                    return result;
                }
                return _binders[siteType] = new TestSetIndex(siteType);
            }
        }

        private static T[] RemoveLast<T>(T[] args) {
            Array.Resize(ref args, args.Length - 1);
            return args;
        }

        public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
            DynamicMetaObject com;
            if (ComBinder.TryBindSetIndex(this, target, indexes, value, out com)) {
                return com;
            }
			return new DynamicMetaObject(
				Expression.Throw(Expression.New(typeof(NotImplementedException)), typeof(object)),
				target.Restrictions
			);
        }
    }

    class TestGetIndex : GetIndexBinder {
        private TestGetIndex(Type siteType)
            : base(TestInvoke.TypeToCallInfo(siteType)) {
        }

        private static readonly Dictionary<Type, TestGetIndex> _binders = new Dictionary<Type, TestGetIndex>();
        internal static TestGetIndex Create(Type siteType) {
            lock (_binders) {
                TestGetIndex result;
                if (_binders.TryGetValue(siteType, out result)) {
                    return result;
                }
                return _binders[siteType] = new TestGetIndex(siteType);
            }
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion) {
            DynamicMetaObject com;
            if (ComBinder.TryBindGetIndex(this, target, indexes, out com)) {
                return com;
            }
            throw new NotImplementedException();
        }
    }

    class TestConvert : ConvertBinder {
        private TestConvert(Type to)
            : base(to, true) {
        }

        private static readonly Dictionary<Type, TestConvert> _binders = new Dictionary<Type, TestConvert>();
        internal static TestConvert Create(Type to) {
            lock (_binders) {
                TestConvert result;
                if (_binders.TryGetValue(to, out result)) {
                    return result;
                }
                return _binders[to] = new TestConvert(to);
            }
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            DynamicMetaObject com;
            if (ComBinder.TryConvert(this, target, out com)) {
                return com;
            }
            throw new NotImplementedException();
        }
    }


    public class CsMethod : IDynamicMetaObjectProvider {
        private readonly MethodInfo _mi;

        public CsMethod(MethodInfo mi) {
            _mi = mi;
        }

        public DynamicMetaObject GetMetaObject(Expression parameter) {
            return null;
        }
    }

    public class DelegateMetaObject : DynamicMetaObject {
        public DelegateMetaObject(Expression expr, BindingRestrictions restrictions, ComEventTarget target)
            : base(expr, restrictions, target) {
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder action, DynamicMetaObject[] args) {
            Type delType = ((ComEventTarget)Value).Target.GetType();

            Expression del = Expression.Convert(
                Expression.Field(Expression.Convert(Expression, typeof(ComEventTarget)), "Target"),
                delType
            );

            BindingRestrictions r = this.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            r.Merge(BindingRestrictions.Combine(args));
            r.Merge(BindingRestrictions.GetTypeRestriction(del, delType));

            Expression[] exprs = DynamicUtils.GetExpressions(args);
            ParameterInfo[] pis = delType.GetMethod("Invoke").GetParameters();
            for (int i = 0; i < exprs.Length; i++) {
                Expression arg = exprs[i];
                Type t = pis[i].ParameterType;
                exprs[i] = Expression.Convert(arg, t);
                r.Merge(BindingRestrictions.GetTypeRestriction(arg, t));
            }

            Expression invoke = Expression.Invoke(del, exprs);
            if (invoke.Type == typeof(void)) {
                invoke = Expression.Block(invoke, Expression.Constant(null));
            } else if (invoke.Type.IsValueType) { 
                invoke = Expression.Convert(invoke, typeof(object)); 
            }
            return new DynamicMetaObject(invoke, r);
        }
    }

    public class ComEventTarget : IDynamicMetaObjectProvider {
        internal ComEventTarget(Delegate target) {
            Target = target;
        }

        public readonly Delegate Target;

        public DynamicMetaObject GetMetaObject(Expression parameter) {
            return new DelegateMetaObject(parameter, BindingRestrictions.Empty, this);
        }
    }

    public class BinaryOpBinder : BinaryOperationBinder {
        public BinaryOpBinder(ExpressionType operation) :
            base(operation) {
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
            if (errorSuggestion != null) {
                return errorSuggestion;
            } else {
                return new DynamicMetaObject(
                    Expression.Throw(Expression.New(typeof(NotImplementedException)), typeof(object)),
                    target.Restrictions
                );
            }
        }
    }
}
