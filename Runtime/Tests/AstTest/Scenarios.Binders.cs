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

#if !SILVERLIGHT3
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Runtime.CompilerServices;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {
    public static partial class Scenarios {

        private class CallBinder : InvokeMemberBinder {
            public CallBinder(string name)
                : base(name, false, new CallInfo(0)) {
            }

            public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
                return new DynamicMetaObject(
                    Expression.Convert(
                        Expression.Call(
                            typeof(Scenarios).GetMethod(Name),
                            target.Expression,
                            args[0].Expression,
                            args[1].Expression
                        ),
                        typeof(object)
                    ),
                    GetTypeRestriction(target)
                        .Merge(GetTypeRestriction(args[0]))
                        .Merge(GetTypeRestriction(args[1]))
                );
            }

            internal static BindingRestrictions GetTypeRestriction(DynamicMetaObject obj) {
                if (obj.Value == null && obj.HasValue) {
                    return BindingRestrictions.GetInstanceRestriction(obj.Expression, null);
                } else {
                    return BindingRestrictions.GetTypeRestriction(obj.Expression, obj.LimitType);
                }
            }

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
                return new DynamicMetaObject(
                    Expression.Throw(
                        Expression.New(
                            typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) }),
                            Expression.Constant("Cannot invoke.")
                        )
                    ),
                    target.Restrictions.Merge(BindingRestrictions.Combine(args))
                );
            }
        }

        public delegate object CallByRefTest(CallSite site, ref int a, ref string b, out double c);

        public static void Positive_BinderWithRefSite(EU.IValidator V) {
            var site = CallSite<CallByRefTest>.Create(new CallBinder("CallByRef"));
            int a = 0;
            string b = null;
            double c = 0.0;

            int result = (int)site.Target(site, ref a, ref b, out c);

            EU.Equal(a, 17);
            EU.Equal(b, "Called");
            EU.Equal(c, Math.PI);
            EU.Equal(result, 7);
        }

        public delegate object CallWithNullable(CallSite site, int? a, ref int? b, int? c);

        public static void Positive_SimpleSiteNullable(EU.IValidator V) {
            var site = CallSite<CallWithNullable>.Create(new CallBinder("CallNullable"));

            int? a = 3;
            int? b = 7;
            int? c = null;

            int? result = (int?)site.Target(site, a, ref b, c);

            EU.Equal(a, 3);
            EU.Equal(b, null);
            EU.Equal(result, 10);
        }

        public static int? CallNullable(int? a, ref int? b, int? c) {
            int? result = a + b;
            b = c;
            return result;
        }

        public sealed class TestImplicitConvert {
            internal bool ConvertCalled;
            public static implicit operator string(TestImplicitConvert self) {
                self.ConvertCalled = true;
                return self.ToString();
            }
        }

        public sealed class TestInvalidConvertBinder : DynamicMetaObjectBinder {
            internal bool BindCalled;

            public override Type ReturnType {
                get { return typeof(string); }
            }

            public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
                BindCalled = true;
                return target;
            }
        }

        public static void Negative_BinderInvalidConversion(EU.IValidator V) {
            var binder = new TestInvalidConvertBinder();
            var site = CallSite<Func<CallSite, TestImplicitConvert, string>>.Create(binder);
            var testObj = new TestImplicitConvert();

            EU.Throws<InvalidCastException>(() => site.Target(site, testObj));

            EU.Equal(binder.BindCalled, true);
            EU.Equal(testObj.ConvertCalled, false);
        }

        private class Interceptee : CallSiteBinder {

            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                if (args[0] == null) {
                    return Expression.IfThen(
                        Expression.ReferenceEqual(parameters[0], Expression.Constant(null)),
                        Expression.Return(returnLabel, Expression.Constant("<null>"))
                    );
                }
                return Expression.IfThen(
                    Expression.TypeEqual(parameters[0], args[0].GetType()),
                    Expression.Return(returnLabel, Expression.Constant(args[0].GetType().Name))
                );
            }

            public override T BindDelegate<T>(CallSite<T> site, object[] args) {
                if (args[0] != null && args[0] is string) {
                    return (T)GetFunc();
                }
                return null;
            }

            private static object GetFunc() {
                return new Func<CallSite, object, object>(
                    (s, a0) => {
                        if (a0 != null && a0 is string) {
                            return "<string>";
                        }
                        return ((CallSite<Func<CallSite, object, object>>)s).Update(s, a0);
                    }
                );
            }
        }

        private class Interceptor : CallSiteBinder {
            internal CallSiteBinder Inner;

            internal readonly List<object> Bindings = new List<object>();

            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                Expression result = Inner.Bind(args, parameters, returnLabel);
                Bindings.Add(result);
                return result;
            }

            public override T BindDelegate<T>(CallSite<T> site, object[] args) {
                T result = Inner.BindDelegate<T>(site, args);
                if (result != null) {
                    Bindings.Add(result);
                }
                return result;
            }
        }

        public static void Positive_InterceptRule(EU.IValidator V) {
            var interceptor = new Interceptor { Inner = new Interceptee() };
            var site = CallSite<Func<CallSite, object, object>>.Create(interceptor);
            var bindings = interceptor.Bindings;

            EU.Equal(site.Target(site, null), "<null>");
            EU.Equal(bindings.Count, 1);
            EU.Equal(site.Target(site, "hello"), "<string>");
            EU.Equal(bindings.Count, 2);
            EU.Equal(site.Target(site, 123), "Int32");
            EU.Equal(bindings.Count, 3);
            EU.Equal(site.Target(site, null), "<null>");
            EU.Equal(site.Target(site, "bye"), "<string>");
            EU.Equal(bindings.Count, 3);
            EU.Equal(site.Target(site, 444.0), "Double");

            EU.Equal(bindings.Count, 4);
            EU.Assert(bindings[0] is ConditionalExpression);
            EU.Assert(bindings[1] is Delegate);
            EU.Assert(bindings[2] is ConditionalExpression);
            EU.Assert(bindings[3] is ConditionalExpression);
        }

        private static CallSite<Func<CallSite, object, object>> _GetFooSite = CallSite<Func<CallSite, object, object>>.Create(new TestGetMember("Foo"));

        public class DynamicWithEquals : IDynamicMetaObjectProvider {
            public object Foo { get; private set; }

            public DynamicWithEquals(object foo) {
                Foo = foo;
            }

            internal int EqualsCalled;

            public override bool Equals(object obj) {
                EqualsCalled++;
                return _GetFooSite.Target(_GetFooSite, this) == _GetFooSite.Target(_GetFooSite, obj);
            }

            public override int GetHashCode() {
                return 1;
            }

            #region IDynamicMetaObjectProvider Members

            public DynamicMetaObject GetMetaObject(Expression parameter) {
                return new Meta(parameter, this);
            }

            private class Meta : DynamicMetaObject {
                public Meta(Expression parameter, object value)
                    : base(parameter, BindingRestrictions.Empty, value) {
                }
                public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
                    if (binder.Name == "Foo") {
                        return new DynamicMetaObject(
                            Expression.Property(Expression.Convert(Expression, typeof(DynamicWithEquals)), binder.Name),
                            BindingRestrictions.GetExpressionRestriction(Expression.ReferenceEqual(Expression, Expression.Constant(Value)))
                        );
                    }
                    return base.BindGetMember(binder);
                }
            }

            #endregion
        }

        // Previously this test would StackOverflow because it called the user
        // equals method
        public static void Positive_TestDynamicWithEquals(EU.IValidator V) {
            var d1 = new DynamicWithEquals("hello");
            var d2 = new DynamicWithEquals("world");
            EU.Equal(_GetFooSite.Target(_GetFooSite, d1), "hello");
            EU.Equal(_GetFooSite.Target(_GetFooSite, d2), "world");
            EU.Equal(d1.EqualsCalled, 0);
            EU.Equal(d2.EqualsCalled, 0);
        }
    }
}
#endif
