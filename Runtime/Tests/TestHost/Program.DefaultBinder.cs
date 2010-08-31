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
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Dynamic;
using System.Text;
using System.Threading;
using IronPython.Compiler;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Hosting.Providers;

#if SILVERLIGHT
using Microsoft.Silverlight.TestHostCritical;
#endif

namespace TestHost {
    public partial class Tests {
        #region GetMember Tests

        class DefaultGetMemberBinder : GetMemberBinder {
            private readonly DefaultBinder _binder = new DefaultBinder();
            public DefaultGetMemberBinder(string name)
                : base(name, false) {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
                var res = _binder.GetMember(Name, target);
                if (res.LimitType.IsValueType) {
                    res = new DynamicMetaObject(Expression.Convert(res.Expression, typeof(object)), res.Restrictions);
                }
                return res;
            }
        }

        [Scenario]
        public static void Scenario_DefaultBinder_GetMember() {
            var binder = new DefaultBinder();

            CallSite<Func<CallSite, object, object>> site = CallSite<Func<CallSite, object, object>>.Create(new DefaultGetMemberBinder("Count"));
            var list = new List<object>();
            AreEqual(site.Target(site, list), 0);

            list.Add(1);
            AreEqual(site.Target(site, list), 1);

            var dict = new Dictionary<object, object>();
            AreEqual(site.Target(site, dict), 0);

            dict["foo"] = "bar";            
            AreEqual(site.Target(site, dict), 1);
        }

        #endregion

        #region SetMember Tests

        class DefaultSetMemberBinder : SetMemberBinder {
            private readonly DefaultBinder _binder = new DefaultBinder();
            public DefaultSetMemberBinder(string name)
                : base(name, false) {
            }

            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
                return _binder.SetMember(Name, target, value);
            }
        }

        [Scenario]
        public static void Scenario_DefaultBinder_SetMember() {
            var binder = new DefaultBinder();

            CallSite<Func<CallSite, object, object, object>> site = CallSite<Func<CallSite, object, object, object>>.Create(new DefaultSetMemberBinder("Capacity"));
            var list = new ArrayList();
            AreEqual(site.Target(site, list, 42), 42);
            AreEqual(list.Capacity, 42);
        }

        #endregion

        #region Binary Operation Tests

        class DefaultBinaryOperationBinder : BinaryOperationBinder {
            private readonly DefaultBinder _binder = new DefaultBinder();
            public DefaultBinaryOperationBinder(ExpressionType operation)
                : base(operation) {
            }

            public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
                var res = _binder.DoOperation(Operation, target, arg);
                if (res.LimitType.IsValueType) {
                    res = new DynamicMetaObject(Expression.Convert(res.Expression, typeof(object)), res.Restrictions);
                }
                return res;
            }
        }

        [Scenario]
        public static void Scenario_DefaultBinder_DoOperation() {
            var binder = new DefaultBinder();

            CallSite<Func<CallSite, object, object, object>> site = CallSite<Func<CallSite, object, object, object>>.Create(new DefaultBinaryOperationBinder(ExpressionType.Add));
            AreEqual(site.Target(site, 2, 3), 5);
            AreEqual(site.Target(site, 2.0, 3.0), 5.0);
            AreEqual(site.Target(site, new Decimal(2), new Decimal(3)), new Decimal(5));
        }

        #endregion

        #region Unary Operation Tests

        class DefaultUnaryOperationBinder : UnaryOperationBinder {
            private readonly DefaultBinder _binder = new DefaultBinder();
            public DefaultUnaryOperationBinder(ExpressionType operation)
                : base(operation) {
            }

            public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
                var res = _binder.DoOperation(Operation, target);
                if (res.LimitType.IsValueType) {
                    res = new DynamicMetaObject(Expression.Convert(res.Expression, typeof(object)), res.Restrictions);
                }
                return res;
            }
        }

        [Scenario]
        public static void Scenario_DefaultBinder_DoOperation_Unary() {
            var binder = new DefaultBinder();

            CallSite<Func<CallSite, object, object>> site = CallSite<Func<CallSite, object, object>>.Create(new DefaultUnaryOperationBinder(ExpressionType.Negate));
            // TODO: Need to support primitives for unary negation
            //AreEqual(site.Target(site, 2), -2);
            //AreEqual(site.Target(site, 2.0), -2.0);
            AreEqual(site.Target(site, new Decimal(2)), new Decimal(-2));
        }

        #endregion

        #region Invoke Tests

        class DefaultInvokeBinder : InvokeBinder {
            private readonly DefaultBinder _binder = new DefaultBinder();
            public DefaultInvokeBinder(CallInfo callInfo)
                : base(callInfo) {                
            }

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
                var res = _binder.Call(new CallSignature(CallInfo.ArgumentCount), target, args);
                if (res.LimitType == typeof(void)) {
                    res = new DynamicMetaObject(Expression.Block(res.Expression, Expression.Constant(null)), res.Restrictions);
                } else if (res.LimitType.IsValueType) {
                    res = new DynamicMetaObject(Expression.Convert(res.Expression, typeof(object)), res.Restrictions);
                }
                return res;
            }
        }

        [Scenario]
        public static void Scenario_DefaultBinder_Invoke() {
            var binder = new DefaultBinder();

            // invoke a delegate
            CallSite<Func<CallSite, object, object, object>> site = CallSite<Func<CallSite, object, object, object>>.Create(new DefaultInvokeBinder(new CallInfo(1)));
            object value = null;
            Action<object> actionDlg = (x) => {
                value = x;
            };

            AreEqual(site.Target(site, actionDlg, 42), null);
            AreEqual(value, 42);

            Func<object, object> funcDlg = (x) => x;
            AreEqual(site.Target(site, funcDlg, 42), 42);

            Func<int, int> funcDlg2 = (x) => x;
            AreEqual(site.Target(site, funcDlg2, 42), 42);

            // get a method and invoke it
            CallSite<Func<CallSite, object, object>> getsite = CallSite<Func<CallSite, object, object>>.Create(new DefaultGetMemberBinder("Add"));
            var list = new List<object>();
            var add = getsite.Target(getsite, list);

            site.Target(site, add, 42);
            AreEqual(list.Count, 1);

            // Get an overloaded method and invoke it
            CallSite<Func<CallSite, object, object>> getGetValueSite = CallSite<Func<CallSite, object, object>>.Create(new DefaultGetMemberBinder("GetValue"));
            object[] array = new object[] { 2, 3, 4 };
            var getValue = getGetValueSite.Target(getGetValueSite, array);
            
            site = CallSite<Func<CallSite, object, object, object>>.Create(new DefaultInvokeBinder(new CallInfo(1)));
            AreEqual(site.Target(site, getValue, 1), 3);
        }

        #endregion
    }
}
