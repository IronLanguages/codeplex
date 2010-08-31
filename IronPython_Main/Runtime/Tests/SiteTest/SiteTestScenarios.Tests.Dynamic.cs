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
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using SiteTest.Actions;
using System.IO;

namespace SiteTest {
    partial class SiteTestScenarios {
        [Test("Remoted IDO")]
        private void Scenario_Remoted() {

            #region CallSite for each action
            var setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("__code__"));
            var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("__code__"));
            #endregion


            // run locally
            object mbro = new MBRODynamicObject();
            Assert.AreEqual("MBRO_GetMember", getMember.Target(getMember, mbro));

            // run remotely
            var domain = AppDomain.CreateDomain("remote");
            mbro = domain.CreateInstanceAndUnwrap(typeof(MBRODynamicObject).Assembly.FullName, typeof(MBRODynamicObject).FullName);

            getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("__code__"));
            Assert.AreEqual("123", getMember.Target(getMember, mbro));

            // run in current domain
            domain = AppDomain.CurrentDomain;
            mbro = domain.CreateInstanceAndUnwrap(typeof(MBRODynamicObject).Assembly.FullName, typeof(MBRODynamicObject).FullName);

            getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("__code__"));
            Assert.AreEqual("MBRO_GetMember", getMember.Target(getMember, mbro));

            ClearRuleCache(getMember);

            // run remotely after clearing cache.
            domain = AppDomain.CreateDomain("remote");
            mbro = domain.CreateInstanceAndUnwrap(typeof(MBRODynamicObject).Assembly.FullName, typeof(MBRODynamicObject).FullName);

            getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("__code__"));
            Assert.AreEqual("123", getMember.Target(getMember, mbro));

            // run locally again
            mbro = new MBRODynamicObject();
            Assert.AreEqual("MBRO_GetMember", getMember.Target(getMember, mbro));

        }

#if CLR45
        [Test("Remoted IDO2")]
        private void Scenario_Remoted2() {

            #region CallSite for each action
            var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("Name"));
            #endregion

            object sdo = new SerializableDO();

            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(ms, sdo);
            ms.Seek(0, SeekOrigin.Begin);
            object sdo2 = formatter.Deserialize(ms);

            Assert.AreEqual("SerializableDO", getMember.Target(getMember, sdo2));
        }
#endif

        // Support for remoted COM is NYI

        //[Test(TestState.COM, "Remoted COM")]
        //private void Scenario_RemotedCom() {

        //    #region CallSite for each action
        //    var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("pLong"));
        //    #endregion

        //    // run locally
        //    MBRODynamicObject mbro = new MBRODynamicObject();
        //    Assert.AreEqual(0, getMember.Target(getMember, mbro.GetComObj()));

        //    // run remotely
        //    var domain = AppDomain.CreateDomain("remote");
        //    mbro = (MBRODynamicObject)domain.CreateInstanceAndUnwrap(typeof(MBRODynamicObject).Assembly.FullName, typeof(MBRODynamicObject).FullName);

        //    getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("pLong"));
        //    Assert.AreEqual("777", getMember.Target(getMember, mbro.GetComObj()));
        //    // need to clear after TestGetMemberBinder.
        //    ClearRuleCache(getMember);


        //    // run in current domain
        //    domain = AppDomain.CurrentDomain;
        //    mbro = (MBRODynamicObject)domain.CreateInstanceAndUnwrap(typeof(MBRODynamicObject).Assembly.FullName, typeof(MBRODynamicObject).FullName);

        //    getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("pLong"));
        //    Assert.AreEqual(0, getMember.Target(getMember, mbro.GetComObj()));
        //}


        #region Add new Dynamic (IDO Helper) tests here.
        /* Call
         * Convert
         * Create
         * DeleteIndex
         * DeleteMember
         * GetIndex
         * GetMember
         * Invoke
         * Operation
         * SetIndex
         * SetMember
         */
        [Test("Simple cases for a basic derivation Dynamic")]
        private void Scenario_DynamicIDO_Simple() {
            //Get a simple Dynamic IDO
            TestDynamicObject dyn = new TestDynamicObject();

            #region CallSite for each MetaAction on this IDO
            var call = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeMemberBinder("member"));
            var convert = CallSite<Func<CallSite, object, string>>.Create(new TestConvertBinder(typeof(String), true));
            var create = CallSite<Func<CallSite, object, object>>.Create(new TestCreateBinder());
            var deleteIndex = CallSite<Action<CallSite, object>>.Create(new TestDeleteIndexBinder());
            var deleteMember = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("member"));
            var getIndex = CallSite<Func<CallSite, object, object>>.Create(new TestGetIndexBinder());
            var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("member"));
            var invoke = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeBinder());
            var binaryOperation = CallSite<Func<CallSite, object, object, object>>.Create(new TestBinaryOperationBinder(ExpressionType.Add));
            var unaryOperation = CallSite<Func<CallSite, object, object>>.Create(new TestUnaryOperationBinder(ExpressionType.Increment));
            var setIndex = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetIndexBinder());
            var setIndex2 = CallSite<Func<CallSite, object, object, object, object>>.Create(new TestSetIndexBinder());
            var setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("member"));
            #endregion

            /* Invoke each CallSite.  We're using a basic derivation of Dynamic which applies no binding
             * logic of its own.  The default binding of Dynamic is to fallback to the supplied CallSiteBinder
             * every time.
             * 
             * We're also using fairly basic derivations of the basic StandardActions for our CallSiteBinders
             * that return a Rule that throws a special BindingException when invoked.  This is enough to
             * let us know that the basic binding mechanism worked from end-to-end.
             */
            AssertExceptionThrown<BindingException>(() => call.Target(call, dyn));
            AssertExceptionThrown<BindingException>(() => convert.Target(convert, dyn));
            AssertExceptionThrown<BindingException>(() => create.Target(create, dyn));
            AssertExceptionThrown<BindingException>(() => deleteIndex.Target(deleteIndex, dyn));
            AssertExceptionThrown<BindingException>(() => deleteMember.Target(deleteMember, dyn));
            AssertExceptionThrown<BindingException>(() => getIndex.Target(getIndex, dyn));
            AssertExceptionThrown<BindingException>(() => getMember.Target(getMember, dyn));
            AssertExceptionThrown<BindingException>(() => invoke.Target(invoke, dyn));
            AssertExceptionThrown<BindingException>(() => binaryOperation.Target(binaryOperation, dyn, 5));
            AssertExceptionThrown<BindingException>(() => unaryOperation.Target(unaryOperation, dyn));
            AssertExceptionThrown<ArgumentException>(() => setIndex.Target(setIndex, dyn, 3));
            AssertExceptionThrown<BindingException>(() => setIndex2.Target(setIndex2, dyn, 3, 10));
            AssertExceptionThrown<BindingException>(() => setMember.Target(setMember, dyn, 5));
        }

        [Test("Simple cases for a derivation DynamicObject that fails each operation")]
        private void Scenario_FailingDynamicObject() {
            //Get a simple Dynamic IDO
            TestDynamicObject3 dyn = new TestDynamicObject3();

            #region CallSite for each MetaAction on this IDO
            var call = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeMemberBinder("member"));
            var convert = CallSite<Func<CallSite, object, string>>.Create(new TestConvertBinder(typeof(String), true));
            var create = CallSite<Func<CallSite, object, object>>.Create(new TestCreateBinder());
            var deleteIndex = CallSite<Action<CallSite, object>>.Create(new TestDeleteIndexBinder());
            var deleteMember = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("member"));
            var getIndex = CallSite<Func<CallSite, object, object>>.Create(new TestGetIndexBinder());
            var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("member"));
            var invoke = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeBinder());
            var binaryOperation = CallSite<Func<CallSite, object, object, object>>.Create(new TestBinaryOperationBinder(ExpressionType.Add));
            var unaryOperation = CallSite<Func<CallSite, object, object>>.Create(new TestUnaryOperationBinder(ExpressionType.Increment));
            var setIndex = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetIndexBinder());
            var setIndex2 = CallSite<Func<CallSite, object, object, object, object>>.Create(new TestSetIndexBinder());
            var setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("member"));
            #endregion

            AssertExceptionThrown<BindingException>(() => call.Target(call, dyn));
            AssertExceptionThrown<BindingException>(() => convert.Target(convert, dyn));
            AssertExceptionThrown<BindingException>(() => create.Target(create, dyn));
            AssertExceptionThrown<BindingException>(() => deleteIndex.Target(deleteIndex, dyn));
            AssertExceptionThrown<BindingException>(() => deleteMember.Target(deleteMember, dyn));
            AssertExceptionThrown<BindingException>(() => getIndex.Target(getIndex, dyn));
            AssertExceptionThrown<BindingException>(() => getMember.Target(getMember, dyn));
            AssertExceptionThrown<BindingException>(() => invoke.Target(invoke, dyn));
            AssertExceptionThrown<BindingException>(() => binaryOperation.Target(binaryOperation, dyn, 5));
            AssertExceptionThrown<BindingException>(() => unaryOperation.Target(unaryOperation, dyn));
            AssertExceptionThrown<ArgumentException>(() => setIndex.Target(setIndex, dyn, 3));
            AssertExceptionThrown<BindingException>(() => setIndex2.Target(setIndex2, dyn, 3, 10));
            AssertExceptionThrown<BindingException>(() => setMember.Target(setMember, dyn, 5));
        }

        [Test("Simple cases for a basic Expando IDO")]
        private void Scenario_ExpandoObject() {
            //ExpandoObject is a real IDO implementing several actions
            var exp = new ExpandoObject();

            #region CallSite for each action
            var invokeMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestInvokeMemberBinder("member"));
            var deleteIndex = CallSite<Action<CallSite, object>>.Create(new TestDeleteIndexBinder());
            var deleteMember = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("member"));
            var deleteMemberUCase = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("MEMBER"));
            var setIndex = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetIndexBinder());
            var setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("member"));
            var setMemberUCase = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("MEMBER"));
            var setIndex2 = CallSite<Func<CallSite, object, object, object, object>>.Create(new TestSetIndexBinder());
            var convert = CallSite<Func<CallSite, object, string>>.Create(new TestConvertBinder(typeof(String), true));
            var create = CallSite<Func<CallSite, object, object>>.Create(new TestCreateBinder());
            var getIndex = CallSite<Func<CallSite, object, object>>.Create(new TestGetIndexBinder());
            var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("member"));
            var getMemberUCase = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("MEMBER"));
            var getMember2 = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("__code__")); //GetMember2 gets a member in the language binder.
            var invoke = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeBinder());
            var unaryOperationNeg = CallSite<Func<CallSite, object, object>>.Create(new TestUnaryOperationBinder(ExpressionType.Increment));
            var binaryOperationNeg = CallSite<Func<CallSite, object, object, object>>.Create(new TestBinaryOperationBinder(ExpressionType.Add));
            #endregion

            //Those actions implemented by ExpandoObject should succeed (SetMember, GetMember, etc).
            //The others should fall back to the CallSiteBinder and trigger a BindingException.

            Assert.AreEqual(getMember.Target(getMember2, exp), "123");

            //deleting a non-existing member results in falling back to the language's binder
            AssertExceptionThrown<BindingException>(() => deleteMember.Target(deleteMember, exp));

            //Actions implemented by ExpandoObject (GetMember, SetMember, DeleteMember)
            AssertExceptionThrown<BindingException>(() => getMember.Target(getMember, exp));
            AssertExceptionThrown<BindingException>(() => getMemberUCase.Target(getMemberUCase, exp));
            AssertExceptionThrown<BindingException>(() => deleteMember.Target(deleteMember, exp));

            setMember.Target(setMember, exp, 52);

            Assert.AreEqual(getMember.Target(getMember, exp), 52);
            AssertExceptionThrown<BindingException>(() => getMemberUCase.Target(getMemberUCase, exp));

            setMemberUCase.Target(setMemberUCase, exp, 9);

            Assert.AreEqual(getMember.Target(getMember, exp), 52);
            Assert.AreEqual(getMemberUCase.Target(getMemberUCase, exp), 9);

            deleteMember.Target(deleteMember, exp);

            AssertExceptionThrown<BindingException>(() => getMember.Target(getMember, exp));
            Assert.AreEqual(getMemberUCase.Target(getMemberUCase, exp), 9);

            deleteMemberUCase.Target(deleteMemberUCase, exp);

            AssertExceptionThrown<BindingException>(() => getMember.Target(getMember, exp));
            AssertExceptionThrown<BindingException>(() => getMemberUCase.Target(getMemberUCase, exp));

            //assign a delegate value to the member
            setMember.Target(setMember, exp, (Func<int, int>)(x => x + 1));

            //invokeMember should work since the member value is invokable now.
            Assert.AreEqual(invokeMember.Target(invokeMember, exp, 100), 101);

            setMember.Target(setMember, exp, 52);

            //Actions not implemented by ExpandoObject (which default to our basic MetaActions, which return rules throwing BindingErrors)
            //the member value is not invokable so invokeMember will result in exception
            AssertExceptionThrown<BindingException>(() => invokeMember.Target(invokeMember, exp, 100));
            AssertExceptionThrown<BindingException>(() => convert.Target(convert, exp));
            AssertExceptionThrown<BindingException>(() => create.Target(create, exp));
            AssertExceptionThrown<BindingException>(() => deleteIndex.Target(deleteIndex, exp));
            AssertExceptionThrown<BindingException>(() => getIndex.Target(getIndex, exp));
            AssertExceptionThrown<BindingException>(() => invoke.Target(invoke, exp));
            AssertExceptionThrown<BindingException>(() => binaryOperationNeg.Target(binaryOperationNeg, exp, 7));
            AssertExceptionThrown<BindingException>(() => unaryOperationNeg.Target(unaryOperationNeg, exp));
            AssertExceptionThrown<ArgumentException>(() => setIndex.Target(setIndex, exp, 3));
            AssertExceptionThrown<BindingException>(() => setIndex2.Target(setIndex2, exp, 3, 10));
        }

        [Test("Basic tests for INotifyPropertyChanged on ExpandoObject")]
        private void Scenario_ExpandoObject_NotifyPropertyChanged() {
            var expando = new ExpandoObject();
            var inpc = expando as INotifyPropertyChanged;
            var dict = expando as IDictionary<string, object>;

            string changed = "";
            PropertyChangedEventHandler handler = (s, e) => {
                Assert.AreEqual(expando, s);
                changed += e.PropertyName;
            };

            inpc.PropertyChanged += handler;

            Assert.AreEqual(changed, "");
            SetMember(expando, "foo", 123);
            Assert.AreEqual(changed, "foo"); changed = "";
            SetMemberIgnoreCase(expando, "FOO", 456);
            Assert.AreEqual(changed, "foo"); changed = "";
            DeleteMember(expando, "foo");
            Assert.AreEqual(changed, "foo"); changed = "";
            AssertExceptionThrown<BindingException>(() => DeleteMember(expando, "foo"));
            Assert.AreEqual(changed, "");
            SetMember(expando, "foo", 123);
            Assert.AreEqual(changed, "foo"); changed = "";
            SetMember(expando, "bar", 456);
            Assert.AreEqual(changed, "bar"); changed = "";
            SetMember(expando, "zed", 456);
            Assert.AreEqual(changed, "zed"); changed = "";
            SetMember(expando, "red", 789);
            Assert.AreEqual(changed, "red"); changed = "";
            DeleteMemberIgnoreCase(expando, "ZeD");
            Assert.AreEqual(changed, "zed"); changed = "";
            dict.Clear();
            Assert.AreEqual(changed, "foobarred"); changed = "";

            dict.Add("baz", "555");
            Assert.AreEqual(changed, "baz"); changed = "";
            dict["baz"] = "555";
            // We detect duplicates, so we don't fire it.
            // It would be okay to fire here, though.
            Assert.AreEqual(changed, "");

            dict["baz"] = "abc";
            Assert.AreEqual(changed, "baz"); changed = "";
            dict.Remove(new KeyValuePair<string, object>("baz", "zzz"));
            Assert.AreEqual(changed, "");
            dict.Remove(new KeyValuePair<string, object>("baz", "abc"));
            Assert.AreEqual(changed, "baz"); changed = "";
            dict["baz"] = "abc";
            dict.Remove("baz");
            Assert.AreEqual(changed, "bazbaz"); changed = "";

            // Add and remove handlers
            inpc.PropertyChanged += handler;
            dict["quux"] = 1;
            Assert.AreEqual(changed, "quuxquux"); changed = "";

            inpc.PropertyChanged -= handler;

            dict["quux"] = 2;
            Assert.AreEqual(changed, "quux"); changed = "";

            inpc.PropertyChanged -= handler;

            dict["quux"] = 3;
            Assert.AreEqual(changed, "");
        }

        private static void SetMember(ExpandoObject expando, string name, object value) {
            var site = CallSite<Action<CallSite, ExpandoObject, object>>.Create(new TestSetMemberBinder(name));
            site.Target(site, expando, value);
        }

        private static void SetMemberIgnoreCase(ExpandoObject expando, string name, object value) {
            var site = CallSite<Action<CallSite, ExpandoObject, object>>.Create(new TestSetMemberBinder(name, true));
            site.Target(site, expando, value);
        }

        private static void DeleteMember(ExpandoObject expando, string name) {
            var site = CallSite<Action<CallSite, ExpandoObject>>.Create(new TestDeleteMemberBinder(name));
            site.Target(site, expando);
        }

        private static void DeleteMemberIgnoreCase(ExpandoObject expando, string name) {
            var site = CallSite<Action<CallSite, ExpandoObject>>.Create(new TestDeleteMemberBinder(name, true));
            site.Target(site, expando);
        }

        private static object GetMember(ExpandoObject expando, string name) {
            var site = CallSite<Func<CallSite, ExpandoObject, object>>.Create(new TestGetMemberBinder(name));
            return site.Target(site, expando);
        }

        [Test("Test GetDynamicMemberNames for a basic Expando IDO")]
        private void Scenario_ExpandoObjectGetDynamicMemberNames() {
            var exp = new ExpandoObject();
            var setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("member1"));
            setMember.Target(setMember, exp, 10);

            var mo = ((IDynamicMetaObjectProvider)exp).GetMetaObject(Expression.Parameter(typeof(object), "arg0"));

            List<string> memberNames = new List<string>(mo.GetDynamicMemberNames());
            Assert.AreEqual(1, memberNames.Count);
            Assert.AreEqual("member1", memberNames[0]);

            setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("member2"));
            setMember.Target(setMember, exp, 20);
            setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("member3"));
            setMember.Target(setMember, exp, 30);

            memberNames = new List<string>(mo.GetDynamicMemberNames());
            Assert.AreEqual(3, memberNames.Count);

            var deleteMember = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("member1"));
            deleteMember.Target(deleteMember, exp);

            memberNames = new List<string>(mo.GetDynamicMemberNames());
            Assert.AreEqual(2, memberNames.Count);

            //Add the deleted member back. Since ExpandoObject didn't really deleted the member from the class 
            //(only the value got deleted), the added member should be at the original index.
            setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("member1"));
            setMember.Target(setMember, exp, 100);
            memberNames = new List<string>(mo.GetDynamicMemberNames());
            Assert.AreEqual(3, memberNames.Count);
        }

        #region Testing case-insensitive behavior of ExpandoObject

        [Test("Test ExpandoObject when used with case-insensitive get binders-1")]
        private void Senario_ExpandoObjectCaseInsensitiveGet1() {
            var exp = new ExpandoObject();
            ((IDictionary<string, object>)exp).Add("foo", 1);
            ((IDictionary<string, object>)exp).Add("FOO", 2);

            var getMemberIgnoreCase = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("FoO", true));
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase.Target(getMemberIgnoreCase, exp));

            ((IDictionary<string, object>)exp).Remove("foo");
            Assert.AreEqual(getMemberIgnoreCase.Target(getMemberIgnoreCase, exp), 2);

            ((IDictionary<string, object>)exp).Remove("FOO");
            AssertExceptionThrown<BindingException>(() => getMemberIgnoreCase.Target(getMemberIgnoreCase, exp));
        }

        [Test("Test ExpandoObject when used with case-insensitive set binders-1")]
        private void Senario_ExpandoObjectCaseInsensitiveSet1() {
            var exp = new ExpandoObject();
            ((IDictionary<string, object>)exp).Add("foo", 1);
            ((IDictionary<string, object>)exp).Add("FOO", 2);

            var setMemberIgnoreCase = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("FoO", true));
            AssertExceptionThrown<AmbiguousMatchException>(() => setMemberIgnoreCase.Target(setMemberIgnoreCase, exp, 100));

            ((IDictionary<string, object>)exp).Remove("foo");
            setMemberIgnoreCase.Target(setMemberIgnoreCase, exp, 100);
            Assert.AreEqual(((IDictionary<string, object>)exp)["FOO"], 100);

            ((IDictionary<string, object>)exp).Remove("FOO");
            setMemberIgnoreCase.Target(setMemberIgnoreCase, exp, 101);
            Assert.AreEqual(((IDictionary<string, object>)exp)["FoO"], 101);
        }

        [Test("Test ExpandoObject when used with case-insensitive delete binders-1")]
        private void Senario_ExpandoObjectCaseInsensitiveDelete1() {
            var exp = new ExpandoObject();
            ((IDictionary<string, object>)exp).Add("foo", 1);
            ((IDictionary<string, object>)exp).Add("FOO", 2);

            var deleteMemberIgnoreCase = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("FoO", true));

            AssertExceptionThrown<AmbiguousMatchException>(() => deleteMemberIgnoreCase.Target(deleteMemberIgnoreCase, exp));

            ((IDictionary<string, object>)exp).Remove("foo");
            Assert.IsTrue(((IDictionary<string, object>)exp).ContainsKey("FOO"));
            deleteMemberIgnoreCase.Target(deleteMemberIgnoreCase, exp);
            Assert.IsFalse(((IDictionary<string, object>)exp).ContainsKey("FOO"));

            AssertExceptionThrown<BindingException>(() => deleteMemberIgnoreCase.Target(deleteMemberIgnoreCase, exp));
        }


        [Test("Test ExpandoObject when used with case-insensitive get binders-2")]
        private void Senario_ExpandoObjectCaseInsensitiveGet2() {
            var exp = new ExpandoObject();
            ((IDictionary<string, object>)exp).Add("foo", 1);
            ((IDictionary<string, object>)exp).Add("FOO", 2);

            ((IDictionary<string, object>)exp).Remove("foo");
            ((IDictionary<string, object>)exp).Remove("FOO");

            var getMemberIgnoreCase = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("FoO", true));

            AssertExceptionThrown<BindingException>(() => getMemberIgnoreCase.Target(getMemberIgnoreCase, exp));

            ((IDictionary<string, object>)exp)["foo"] = 1;
            Assert.AreEqual(getMemberIgnoreCase.Target(getMemberIgnoreCase, exp), 1);

            ((IDictionary<string, object>)exp)["FOO"] = 2;
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase.Target(getMemberIgnoreCase, exp));
        }

        [Test("Test ExpandoObject when used with case-insensitive set binders-2")]
        private void Senario_ExpandoObjectCaseInsensitiveSet2() {
            var exp = new ExpandoObject();
            ((IDictionary<string, object>)exp).Add("foo", 1);
            ((IDictionary<string, object>)exp).Add("FOO", 2);

            ((IDictionary<string, object>)exp).Remove("foo");
            ((IDictionary<string, object>)exp).Remove("FOO");

            var setMemberIgnoreCase = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("FoO", true));

            Assert.IsFalse(((IDictionary<string, object>)exp).ContainsKey("foO"));
            setMemberIgnoreCase.Target(setMemberIgnoreCase, exp, 101);
            Assert.AreEqual(((IDictionary<string, object>)exp)["FoO"], 101);

            ((IDictionary<string, object>)exp)["foo"] = 1;
            AssertExceptionThrown<AmbiguousMatchException>(() => setMemberIgnoreCase.Target(setMemberIgnoreCase, exp, 100));
        }

        [Test("Test ExpandoObject when used with case-insensitive delete binders-2")]
        private void Senario_ExpandoObjectCaseInsensitiveDelete2() {
            var exp = new ExpandoObject();
            ((IDictionary<string, object>)exp).Add("foo", 1);
            ((IDictionary<string, object>)exp).Add("FOO", 2);

            ((IDictionary<string, object>)exp).Remove("foo");
            ((IDictionary<string, object>)exp).Remove("FOO");

            var deleteMemberIgnoreCase = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("FoO", true));

            AssertExceptionThrown<BindingException>(() => deleteMemberIgnoreCase.Target(deleteMemberIgnoreCase, exp));

            ((IDictionary<string, object>)exp)["foo"] = 1;
            Assert.IsTrue(((IDictionary<string, object>)exp).ContainsKey("foo"));
            deleteMemberIgnoreCase.Target(deleteMemberIgnoreCase, exp);
            Assert.IsFalse(((IDictionary<string, object>)exp).ContainsKey("foo"));

            ((IDictionary<string, object>)exp)["foo"] = 1;
            ((IDictionary<string, object>)exp)["FOO"] = 2;
            AssertExceptionThrown<AmbiguousMatchException>(() => deleteMemberIgnoreCase.Target(deleteMemberIgnoreCase, exp));
        }

        [Test("Test ExpandoObject when used with case-insensitive get binders-3")]
        private void Senario_ExpandoObjectCaseInsensitiveGet3() {
            var exp = new ExpandoObject();
            ((IDictionary<string, object>)exp).Add("foo", 1);
            ((IDictionary<string, object>)exp).Add("FOO", 2);

            ((IDictionary<string, object>)exp).Remove("FOO");

            var getMemberIgnoreCase = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("FoO", true));

            Assert.AreEqual(getMemberIgnoreCase.Target(getMemberIgnoreCase, exp), 1);

            ((IDictionary<string, object>)exp).Remove("foo");
            AssertExceptionThrown<BindingException>(() => getMemberIgnoreCase.Target(getMemberIgnoreCase, exp));

            ((IDictionary<string, object>)exp)["foo"] = 1;
            ((IDictionary<string, object>)exp)["FOO"] = 2;
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase.Target(getMemberIgnoreCase, exp));
        }

        [Test("Test ExpandoObject when used with case-insensitive set binders-3")]
        private void Senario_ExpandoObjectCaseInsensitiveSet3() {
            var exp = new ExpandoObject();
            ((IDictionary<string, object>)exp).Add("foo", 1);
            ((IDictionary<string, object>)exp).Add("FOO", 2);

            ((IDictionary<string, object>)exp).Remove("foo");

            var setMemberIgnoreCase = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("FoO", true));

            Assert.AreEqual(((IDictionary<string, object>)exp)["FOO"], 2);
            setMemberIgnoreCase.Target(setMemberIgnoreCase, exp, 101);
            Assert.AreEqual(((IDictionary<string, object>)exp)["FOO"], 101);

            ((IDictionary<string, object>)exp)["foo"] = 1;
            AssertExceptionThrown<AmbiguousMatchException>(() => setMemberIgnoreCase.Target(setMemberIgnoreCase, exp, 100));
        }

        [Test("Test ExpandoObject when used with case-insensitive delete binders-3")]
        private void Senario_ExpandoObjectCaseInsensitiveDelete3() {
            var exp = new ExpandoObject();
            ((IDictionary<string, object>)exp).Add("foo", 1);
            ((IDictionary<string, object>)exp).Add("FOO", 2);

            ((IDictionary<string, object>)exp).Remove("FOO");

            var deleteMemberIgnoreCase = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("FoO", true));

            Assert.IsTrue(((IDictionary<string, object>)exp).ContainsKey("foo"));
            deleteMemberIgnoreCase.Target(deleteMemberIgnoreCase, exp);
            Assert.IsFalse(((IDictionary<string, object>)exp).ContainsKey("foo"));

            AssertExceptionThrown<BindingException>(() => deleteMemberIgnoreCase.Target(deleteMemberIgnoreCase, exp));

            ((IDictionary<string, object>)exp)["foo"] = 1;
            ((IDictionary<string, object>)exp)["FOO"] = 2;
            AssertExceptionThrown<AmbiguousMatchException>(() => deleteMemberIgnoreCase.Target(deleteMemberIgnoreCase, exp));
        }

        [Test("Test ExpandoObject when used with binders that mix case sensitivity")]
        private void Scenario_ExpandoObjectMixCaseSensitivity() {
            var exp = new ExpandoObject();
            var setMember1 = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("Member"));
            var setMember2 = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("member"));
            var setMember3 = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("MEMBER"));
            var setMemberIgnoreCase1 = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("member", true));
            var setMemberIgnoreCase2 = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("MEMBER", true));
            var getMember1 = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("Member"));
            var getMember2 = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("member"));
            var getMemberIgnoreCase1 = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("member", true));
            var getMemberIgnoreCase2 = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("Member", true));
            var getMemberIgnoreCase3 = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("MEMBER", true));
            var deleteMember = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("member"));
            var deleteMemberIgnoreCase = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("member", true));


            //Verify that ignore case get can fall back correctly when no match
            var getMemberIgnoreCase = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("__cOdE__", true));
            Assert.AreEqual("123", getMemberIgnoreCase.Target(getMemberIgnoreCase, exp));

            AssertExceptionThrown<BindingException>(() => getMemberIgnoreCase1.Target(getMemberIgnoreCase1, exp));

            //exp.member = 1 (ignore case)
            setMemberIgnoreCase1.Target(setMemberIgnoreCase1, exp, 100);

            //If case insensitive, get the match if there is only one.
            //get exp.Member = 100, found the only match
            Assert.AreEqual(getMemberIgnoreCase1.Target(getMemberIgnoreCase1, exp), 100);
            Assert.AreEqual(getMemberIgnoreCase2.Target(getMemberIgnoreCase2, exp), 100);

            deleteMember.Target(deleteMember, exp);
            AssertExceptionThrown<BindingException>(() => getMemberIgnoreCase1.Target(getMemberIgnoreCase1, exp));
            ((IDictionary<string, object>)exp).Add("member", 1001);
            Assert.AreEqual(getMemberIgnoreCase1.Target(getMemberIgnoreCase1, exp), 1001);
            ((IDictionary<string, object>)exp)["member"] = 1;

            //exp.Member = 1 (case insensitive) overwrites the matching member
            setMemberIgnoreCase2.Target(setMemberIgnoreCase2, exp, 1);
            Assert.AreEqual(getMember2.Target(getMember2, exp), 1);
            Assert.AreEqual(getMemberIgnoreCase1.Target(getMemberIgnoreCase1, exp), 1);
            Assert.AreEqual(getMemberIgnoreCase2.Target(getMemberIgnoreCase2, exp), 1);

            //exp.Member = 2 (case sensitive)
            setMember1.Target(setMember1, exp, 2);

            //get exp.member (ignore case) results in AmbigousMatchException
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase1.Target(getMemberIgnoreCase1, exp));
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase2.Target(getMemberIgnoreCase2, exp));
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase3.Target(getMemberIgnoreCase3, exp));

            //Delete exp.member (case sensitive)
            deleteMember.Target(deleteMember, exp);
            Assert.AreEqual(getMember1.Target(getMember1, exp), 2);
            AssertExceptionThrown<BindingException>(() => getMember2.Target(getMember2, exp));
            Assert.AreEqual(getMemberIgnoreCase1.Target(getMemberIgnoreCase1, exp), 2);
            Assert.AreEqual(getMemberIgnoreCase2.Target(getMemberIgnoreCase1, exp), 2);

            //exp.member = 1 (case sensitive)
            setMember2.Target(setMember2, exp, 1);

            //exp.MEMBER = 3 (ignore case) results in AmbiguousMatchException
            AssertExceptionThrown<AmbiguousMatchException>(() => setMemberIgnoreCase2.Target(setMemberIgnoreCase2, exp, 3));

            //set exp.MEMBER = 3, case-sensitive
            setMember3.Target(setMember3, exp, 3);

            //get exp.MEMBER (ignore case) AmbiguousMatchException
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase3.Target(getMemberIgnoreCase3, exp));

            //Get exp.Member case sensitively
            Assert.AreEqual(getMember1.Target(getMember1, exp), 2);
            //Get exp.member case sensitively
            Assert.AreEqual(getMember2.Target(getMember2, exp), 1);

            //Delete exp.member (case sensitive)
            deleteMember.Target(deleteMember, exp);

            //Get exp.member (case sensitive) results in BindingException
            AssertExceptionThrown<BindingException>(() => getMember2.Target(getMember2, exp));
            //Get exp.Member case sensitively
            Assert.AreEqual(getMember1.Target(getMember1, exp), 2);

            //Get exp.member (case insensitive) results in AmbiguousMatchException
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase1.Target(getMemberIgnoreCase1, exp));
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase2.Target(getMemberIgnoreCase2, exp));
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase3.Target(getMemberIgnoreCase3, exp));

            //add the deleted expt.member back (case sensitive)
            setMember2.Target(setMember2, exp, 1);

            //get exp.member (case insensitive) results in AmbiguousMatchException
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase1.Target(getMemberIgnoreCase1, exp));
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase2.Target(getMemberIgnoreCase2, exp));

            //apply a case insensitive delete binder results in AmbigousMatchException
            AssertExceptionThrown<AmbiguousMatchException>(() => deleteMemberIgnoreCase.Target(deleteMemberIgnoreCase, exp));
            //Delete exp.member (case sensitive)
            deleteMember.Target(deleteMember, exp);

            //delete exp.member case insensitively results in AmbiguousMatchException
            AssertExceptionThrown<AmbiguousMatchException>(() => deleteMemberIgnoreCase.Target(deleteMemberIgnoreCase, exp));

            //Get exp.member case sensitively results in exception
            AssertExceptionThrown<BindingException>(() => getMember2.Target(getMember2, exp));
            //Set exp.member case insensitively results in AmbiguousMatchException
            AssertExceptionThrown<AmbiguousMatchException>(() => setMemberIgnoreCase2.Target(setMemberIgnoreCase2, exp, 100));
            //Get exp.member case insensitively results in AmbiguousMatchException
            AssertExceptionThrown<AmbiguousMatchException>(() => getMemberIgnoreCase1.Target(getMemberIgnoreCase1, exp));
        }

        #endregion

        [Test("Test the IDictionary<string, object> members of ExpandoObject")]
        private void Scenario_ExpandoObjectIDictionaryMembers() {
            var exp = (IDictionary<string, object>)(new ExpandoObject());

            Assert.IsFalse(exp.IsReadOnly);

            KeyValuePair<string, object> kv1 = new KeyValuePair<string, object>("a", 1);
            KeyValuePair<string, object> kv2 = new KeyValuePair<string, object>("b", 2);
            KeyValuePair<string, object> kv3 = new KeyValuePair<string, object>("c", 3);
            KeyValuePair<string, object> kv4 = new KeyValuePair<string, object>("c", 4);
            KeyValuePair<string, object> kv5 = new KeyValuePair<string, object>("d", 5);

            exp.Add("a", 1);
            exp.Add("b", 2);
            exp.Add(kv3);

            Assert.IsTrue(exp.Contains(kv1));
            Assert.IsTrue(exp.Contains(kv2));
            Assert.IsTrue(exp.Contains(kv3));
            Assert.IsFalse(exp.Contains(kv5));
            Assert.IsTrue(exp.ContainsKey("a"));
            Assert.IsTrue(exp.ContainsKey("b"));
            Assert.IsTrue(exp.ContainsKey("c"));

            Assert.IsTrue(exp.Remove(kv3));
            Assert.IsFalse(exp.ContainsKey("c"));
            Assert.IsFalse(exp.Remove(kv3));        //Try to remove non-existant
            exp.Add(kv3);

            Assert.IsFalse(exp.Remove(kv4));        //Remove KVP with same key, but different value
            Assert.AreEqual(3, exp.Count);          //Nothing should be removed

            //adding the same key causes exception
            AssertExceptionThrown<ArgumentException>(() => exp.Add("a", 100));

            Assert.AreEqual(3, exp.Count);
            List<string> keys = new List<string>(exp.Keys);
            Assert.AreEqual(keys[0], "a");
            List<object> values = new List<object>(exp.Values);
            Assert.AreEqual(values[2], 3);

            // Various valid/invalid cases with KeyCollection
            AssertExceptionThrown<NotSupportedException>(() => exp.Keys.Add("x"));
            AssertExceptionThrown<NotSupportedException>(() => exp.Keys.Clear());
            Assert.IsTrue(exp.Keys.Contains("a"));
            Assert.IsFalse(exp.Keys.Contains("blah"));
            Assert.IsTrue(exp.Keys.IsReadOnly);
            AssertExceptionThrown<NotSupportedException>(() => exp.Keys.Remove("HI"));
            Assert.IsTrue(exp.Keys.Contains("b"));
            Assert.IsFalse(exp.Keys.Contains("foo"));

            // Various valid/invalid cases with ValueCollection
            AssertExceptionThrown<NotSupportedException>(() => exp.Values.Add(2));
            AssertExceptionThrown<NotSupportedException>(() => exp.Values.Clear());
            Assert.IsTrue(exp.Values.Contains(2));
            Assert.IsFalse(exp.Values.Contains(-2));
            Assert.IsFalse(exp.Values.Contains(-2));
            Assert.IsTrue(exp.Values.IsReadOnly);
            AssertExceptionThrown<NotSupportedException>(() => exp.Values.Remove(1));
            Assert.IsFalse(exp.Values.Contains("foo"));

            //Iterate the Keys or Values collection and the expando changes will
            //cause InvalidOperationException
            AssertExceptionThrown<InvalidOperationException>(() => IterateAndModifyKeyCollection(exp));
            AssertExceptionThrown<InvalidOperationException>(() => IterateAndModifyValueCollection(exp));

            // Additional iterator cases
            IterateAndModifyKeyCollection2(exp);
            IterateAndModifyValueCollection2(exp);
            IterateAndModifyKeyCollection3(exp);
            IterateAndModifyValueCollection3(exp);
            IterateAndModifyKeyCollection4(exp);
            IterateAndModifyValueCollection4(exp);


            object value;
            Assert.IsTrue(exp.TryGetValue("b", out value));
            Assert.AreEqual(2, value);

            Assert.IsFalse(exp.TryGetValue("x", out value));

            KeyValuePair<string, object>[] arr = new KeyValuePair<string, object>[10];
            exp.CopyTo(arr, 0);
            Assert.AreEqual(arr[2].Value, 3);

            AssertExceptionThrown<ArgumentOutOfRangeException>(() => exp.CopyTo(arr, 8));

            foreach (var kv in exp) {
                Assert.IsTrue(kv.Value != null);
            }

            // Use non-generic collection
            var exp2 = (System.Collections.IEnumerable)(exp);
            int cnt = 0;
            foreach (var kv in exp2) {
                cnt++;
            }
            Assert.AreEqual(exp.Count, cnt);

            //iterate and modify the expando will cause exception
            AssertExceptionThrown<InvalidOperationException>(() => IterateAndModifyExpando(exp));

            Assert.IsTrue(exp.Remove("a"));

            Assert.IsFalse(exp.Remove("a"));    //the 2nd time will fail.
            //Assert.AreEqual(2, exp.Count);

            //add ["a", 100]
            exp.Add("a", 100);
            Assert.AreEqual(100, exp["a"]);
            exp["a"] = 1;
            Assert.AreEqual(1, exp["a"]);

            AssertExceptionThrown<KeyNotFoundException>(() => value = exp["e"]);
            exp["e"] = 5;
            Assert.AreEqual(exp["e"], 5);
            exp["e"] = 6;
            Assert.AreEqual(exp["e"], 6);
            Assert.AreEqual(exp.Remove(new KeyValuePair<string, object>("e", 5)), false);
            Assert.AreEqual(exp.Remove(new KeyValuePair<string, object>("e", 6)), true);
            AssertExceptionThrown<KeyNotFoundException>(() => value = exp["e"]);

            exp.Clear();

            Assert.AreEqual(0, exp.Count);
            Assert.AreEqual(0, exp.Values.Count);
            Assert.AreEqual(0, exp.Keys.Count);

            // Some additional cases around null
            var expnull = (IDictionary<string, object>)(new ExpandoObject());
            expnull.Add("a", null);
            AssertExceptionThrown<ArgumentNullException>(() => expnull.Add(null, "SDF"));
            KeyValuePair<string, object> kvnull = new KeyValuePair<string, object>();
            AssertExceptionThrown<ArgumentNullException>(() => expnull.Add(kvnull));
            Assert.IsFalse(expnull.Contains(kvnull));

            ExpandoObject expando = new ExpandoObject();
            exp = (IDictionary<string, object>)expando;
            // thread safety, multiple threads adding to the same object.
            // All values should be added
            Thread t1 = new Thread(() => ExpandoThreadAdder(expando, "Thread1_"));
            Thread t2 = new Thread(() => ExpandoThreadAdder(expando, "Thread2_"));
            Thread t3 = new Thread(() => ExpandoThreadAdder(expando, "Thread3_"));
            Thread t4 = new Thread(() => ExpandoThreadAdder(expando, "Thread4_"));

            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();

            t1.Join();
            t2.Join();
            t3.Join();
            t4.Join();

            // all values should be set
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 1000; j++) {
                    Assert.AreEqual(exp["Thread" + (i + 1) + "_" + j.ToString("0000")], j);
                }
            }

            t1 = new Thread(() => ExpandoThreadAdderRemover(expando, "Thread1_"));
            t2 = new Thread(() => ExpandoThreadAdderRemover(expando, "Thread2_"));
            t3 = new Thread(() => ExpandoThreadAdderRemover(expando, "Thread3_"));
            t4 = new Thread(() => ExpandoThreadAdderRemover(expando, "Thread4_"));

            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();

            t1.Join();
            t2.Join();
            t3.Join();
            t4.Join();

            // all values should have been set and removed
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 1000; j++) {
                    Assert.AreEqual(exp.ContainsKey("Thread" + (i + 1) + "_" + j.ToString("0000")), false);
                }
            }
        }

        [Test("Test the Contains member of ExpandoObject containing a null value")]
        private void Scenario_ExpandoObjectWithNullValue() {
            var exp = (IDictionary<string, object>)(new ExpandoObject());

            exp.Add("a", null);
            Assert.IsTrue(exp.Values.Contains(null));
            Assert.IsFalse(exp.Values.Contains(1));
        }

        private static void IterateAndModifyExpando(IDictionary<string, object> exp) {
            foreach (var k in exp) {
                exp.Add("d", 4);
                exp.Remove("d");
            }
        }

        private static void IterateAndModifyKeyCollection(IDictionary<string, object> exp) {
            foreach (var k in exp.Keys) {
                exp.Add("d", 4);
                exp.Remove("d");
            }
        }

        private static void IterateAndModifyValueCollection(IDictionary<string, object> exp) {
            foreach (var k in exp.Values) {
                exp.Add("d", 4);
                exp.Remove("d");
            }
        }

        private static void IterateAndModifyKeyCollection2(IDictionary<string, object> exp) {
            ICollection<string> k = exp.Keys;
            foreach (var kv in k) {
                exp.Add("blah", 5);
                int i = 0;
                AssertExceptionThrown<InvalidOperationException>(() => i = k.Count);
                return;
            }
        }

        private static void IterateAndModifyValueCollection2(IDictionary<string, object> exp) {
            ICollection<object> k = exp.Values;
            foreach (var kv in k) {
                exp.Add("blah2", 6);
                int i = 0;
                AssertExceptionThrown<InvalidOperationException>(() => i = k.Count);
                return;
            }
        }

        private static void IterateAndModifyKeyCollection3(IDictionary<string, object> exp) {
            ICollection<string> k = exp.Keys;
            foreach (var kv in k) {
                exp.Add("blah3", 5);
                AssertExceptionThrown<InvalidOperationException>(() => k.Contains("blah"));
                return;
            }
        }

        private static void IterateAndModifyValueCollection3(IDictionary<string, object> exp) {
            ICollection<object> k = exp.Values;
            foreach (var kv in k) {
                exp.Add("blah4", 6);
                AssertExceptionThrown<InvalidOperationException>(() => k.Contains("blah"));
                return;
            }
        }

        private static void IterateAndModifyKeyCollection4(IDictionary<string, object> exp) {
            ICollection<string> k = exp.Keys;
            string[] arr = new string[10];
            foreach (var kv in k) {
                exp.Add("blah5", 5);
                AssertExceptionThrown<InvalidOperationException>(() => k.CopyTo(arr, 0));
                return;
            }
        }

        private static void IterateAndModifyValueCollection4(IDictionary<string, object> exp) {
            ICollection<object> k = exp.Values;
            object[] arr = new object[10];
            foreach (var kv in k) {
                exp.Add("blah6", 6);
                AssertExceptionThrown<InvalidOperationException>(() => k.CopyTo(arr, 0));
                return;
            }
        }

        private static void ExpandoThreadAdder(ExpandoObject self, string name) {
            IDictionary<string, object> exp = (IDictionary<string, object>)self;
            for (int i = 0; i < 1000; i++) {
                string setname = name + i.ToString("0000");
                if (exp.ContainsKey(setname)) {
                    exp[setname] = i;
                } else {
                    exp.Add(setname, i);
                }
            }
        }

        private static void ExpandoThreadAdderRemover(ExpandoObject self, string name) {
            IDictionary<string, object> exp = (IDictionary<string, object>)self;
            for (int i = 0; i < 1000; i++) {
                string setname = name + i.ToString("0000");
                if (exp.ContainsKey(setname)) {
                    exp[setname] = i;
                } else {
                    exp.Add(setname, i);
                }
                exp.Remove(setname);
            }
        }

        [Test(TestState.COM, "Simple negative cases for the IDispatch COM IDO")]
        private void Scenario_IDispatch_Negative() {
            //Get a COM object that implements IDispatch
            Type comType = Type.GetTypeFromProgID("DlrComLibrary.DlrComServer");
            Assert.IsNotNull(comType, "Could not retrieve DlrComLibrary.DlrComServer.  Make sure you have registered DlrComLibrary.dll on this machine");
            var comObj = Activator.CreateInstance(comType);

            //var comMeta = MetaObject.ObjectToMetaObject(comObj, Expression.Constant(comObj));

            #region CallSite for each Action
            var call = CallSite<Func<CallSite, object, int, int, int, int, int, object>>.Create(new TestInvokeMemberBinder("NotThere"));
            var convert = CallSite<Func<CallSite, object, string>>.Create(new TestConvertBinder(typeof(String), true));
            var create = CallSite<Func<CallSite, object, object>>.Create(new TestCreateBinder());
            var deleteIndex = CallSite<Action<CallSite, object, int>>.Create(new TestDeleteIndexBinder());
            var deleteMember = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("NotThere"));
            var getIndex = CallSite<Func<CallSite, object, object, object>>.Create(new TestGetIndexBinder());
            var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("NotThere"));
            var invoke = CallSite<Func<CallSite, object, int, int, int, int, int, object>>.Create(new TestInvokeBinder());
            var binaryOperation = CallSite<Func<CallSite, object, object, object>>.Create(new TestBinaryOperationBinder(ExpressionType.Add));
            var unaryOperation = CallSite<Func<CallSite, object, object>>.Create(new TestUnaryOperationBinder(ExpressionType.Increment));
            var setIndex = CallSite<Func<CallSite, object, object, object, object>>.Create(new TestSetIndexBinder());
            var setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("NotThere"));
            #endregion

            //Perform each action, they should all fail and fall back to the given CallSiteBinder
            AssertExceptionThrown<BindingException>(() => call.Target(call, comObj, 1, 2, 3, 4, 5));
            AssertExceptionThrown<BindingException>(() => convert.Target(convert, comObj));
            AssertExceptionThrown<BindingException>(() => create.Target(create, comObj));
            AssertExceptionThrown<BindingException>(() => deleteIndex.Target(deleteIndex, comObj, 3));
            AssertExceptionThrown<BindingException>(() => deleteMember.Target(deleteMember, comObj));
            AssertExceptionThrown<MissingMemberException>(() => getIndex.Target(getIndex, comObj, 3)); //We always try to dispatch to the default property.
            AssertExceptionThrown<BindingException>(() => getMember.Target(getMember, comObj));
            AssertExceptionThrown<MissingMemberException>(() => invoke.Target(invoke, comObj, 5, 4, 3, 2, 1));
            AssertExceptionThrown<BindingException>(() => binaryOperation.Target(binaryOperation, comObj, 13));
            AssertExceptionThrown<BindingException>(() => unaryOperation.Target(unaryOperation, comObj));
            AssertExceptionThrown<MissingMemberException>(() => setIndex.Target(setIndex, comObj, 3, 4));
            AssertExceptionThrown<BindingException>(() => setMember.Target(setMember, comObj, null));
        }

        [Test(TestState.COM, "Simple positive cases for the IDispatch COM IDO")]
        private void Scenario_IDispatch_Positive() {
            /* Note: There are no positive cases for IDispatchComObject against these actions:
             * 
             * Create
             * BinaryOperation
             * UnaryOperation
             * Invoke
             * DeleteIndex
             * DeleteMember
             * BinaryOperationOnIndex
             * BinaryOperationOnMember
             * UnaryOperationOnIndex
             * UnaryOperationOnMember
             * 
             */

            //Get a COM object that implements IDispatch
            Type comType = Type.GetTypeFromProgID("DlrComLibrary.DlrComServer");
            Assert.IsNotNull(comType, "Could not retrieve DlrComLibrary.DlrComServer.  Make sure you have registered DlrComLibrary.dll on this machine");
            var dlrComServer = Activator.CreateInstance(comType);
            var propertyObj = Activator.CreateInstance(Type.GetTypeFromProgID("DlrComLibrary.Properties"));

            //var comMeta = MetaObject.ObjectToMetaObject(comObj, Expression.Constant(comObj));

            #region CallSite for each supported Action

            var call = CallSite<Func<CallSite, object, int, int, int, int, int, object>>.Create(new TestInvokeMemberBinder("SumArgs"));
            ClearRuleCache(call);

            //Why does this insist on returning object??           
            var getIndex = CallSite<Func<CallSite, object, short, object>>.Create(new TestGetIndexBinder());
            ClearRuleCache(getIndex);

            var convert = CallSite<Func<CallSite, object, IDynamicMetaObjectProvider>>.Create(new TestConvertBinder(typeof(IDynamicMetaObjectProvider), true));
            ClearRuleCache(convert);

            var getMember_Method = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("SumArgs"));
            ClearRuleCache(getMember_Method);

            var getMember_Event = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("SumArgs"));
            ClearRuleCache(getMember_Event);

            var getMember_Property = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("pLong"));
            ClearRuleCache(getMember_Property);

            var setIndex = CallSite<Func<CallSite, object, short, bool, object>>.Create(new TestSetIndexBinder());
            ClearRuleCache(setIndex);

            var setMember_Property = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("pLong"));
            ClearRuleCache(setMember_Property);

            var setMember_Event = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("SimpleMethod"));
            ClearRuleCache(setMember_Event);

            #endregion

            //Perform each action
            Assert.AreEqual(call.Target(call, dlrComServer, 1, 2, 3, 4, 5), 12345);

            Assert.AreEqual(true, getIndex.Target(getIndex, propertyObj, 0));
            setIndex.Target(setIndex, propertyObj, 0, false);
            Assert.AreEqual(false, getIndex.Target(getIndex, propertyObj, 0));

            var sumArgs = getMember_Method.Target(getMember_Method, dlrComServer); //Returns a new DispCallable IDO, which is invokable
            Assert.IsNotNull(sumArgs);
            Assert.AreEqual(0, getMember_Property.Target(getMember_Property, propertyObj));
            setMember_Property.Target(setMember_Property, propertyObj, 42);
            Assert.AreEqual(42, getMember_Property.Target(getMember_Property, propertyObj));

            //@TODO - getMember_Event, setMember_Event, convert


            //Repeat each action for dispcallable, which only supports Invoke
            #region Negative CallSites for dispcallable
            var create = CallSite<Func<CallSite, object, object>>.Create(new TestCreateBinder());
            var invoke = CallSite<Func<CallSite, object, int, int, int, int, int, object>>.Create(new TestInvokeBinder());
            ClearRuleCache(invoke);

            var deleteIndex = CallSite<Action<CallSite, object, int>>.Create(new TestDeleteIndexBinder());
            var deleteMember = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("NotThere"));
            var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("NotThere"));
            var setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("NotThere"));
            var binaryOperation = CallSite<Func<CallSite, object, object, object>>.Create(new TestBinaryOperationBinder(ExpressionType.Add));
            var unaryOperation = CallSite<Func<CallSite, object, object>>.Create(new TestUnaryOperationBinder(ExpressionType.Increment));
            var getIndex2 = CallSite<Func<CallSite, object, int, int, int, int, int, object>>.Create(new TestGetIndexBinder());
            #endregion

            Assert.AreEqual(invoke.Target(invoke, sumArgs, 5, 4, 3, 2, 1), 54321);
            Assert.AreEqual(getIndex2.Target(getIndex2, sumArgs, 5, 4, 3, 2, 1), 54321); 
            AssertExceptionThrown<MissingMemberException>(() => setIndex.Target(setIndex, sumArgs, 0, false));
            AssertExceptionThrown<BindingException>(() => call.Target(call, sumArgs, 6, 7, 8, 9, 10));
            AssertExceptionThrown<BindingException>(() => convert.Target(convert, sumArgs));
            AssertExceptionThrown<BindingException>(() => create.Target(create, sumArgs));
            AssertExceptionThrown<BindingException>(() => deleteIndex.Target(deleteIndex, sumArgs, 3));
            AssertExceptionThrown<BindingException>(() => deleteMember.Target(deleteMember, sumArgs));
            AssertExceptionThrown<BindingException>(() => getMember.Target(getMember, sumArgs));
            AssertExceptionThrown<BindingException>(() => binaryOperation.Target(binaryOperation, sumArgs, 13));
            AssertExceptionThrown<BindingException>(() => unaryOperation.Target(unaryOperation, sumArgs));
            AssertExceptionThrown<BindingException>(() => setMember.Target(setMember, sumArgs, null));
        }

        [Test(TestState.COM, "Simple cases for a basic generic COM IDO")]
        private void Scenario_GenericCOM_Simple() {
            //Get a COM object that does not implement IDispatch
            Type comType = Type.GetTypeFromProgID("DlrComLibrary.NonDispatch");
            Assert.IsNotNull(comType, "Could not retrieve DlrComLibrary.NonDispatch.  Make sure you have registered DlrComLibrary.dll on this machine");
            var comObj = Activator.CreateInstance(comType);

            //var comMeta = MetaObject.ObjectToMetaObject(comObj, Expression.Constant(comObj));

            #region CallSite for each Action
            var call = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeMemberBinder("SimpleMethod"));
            var convert = CallSite<Func<CallSite, object, string>>.Create(new TestConvertBinder(typeof(String), true));
            var create = CallSite<Func<CallSite, object, object>>.Create(new TestCreateBinder());
            var deleteIndex = CallSite<Action<CallSite, object, int>>.Create(new TestDeleteIndexBinder());
            var deleteMember = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("SimpleMethod"));
            var getIndex = CallSite<Func<CallSite, object, object, object>>.Create(new TestGetIndexBinder());
            var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("SimpleMethod"));
            var invoke = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeBinder());
            var binaryOperation = CallSite<Func<CallSite, object, object, object>>.Create(new TestBinaryOperationBinder(ExpressionType.Add));
            var unaryOperation = CallSite<Func<CallSite, object, object>>.Create(new TestUnaryOperationBinder(ExpressionType.Increment));
            var setIndex = CallSite<Func<CallSite, object, object, object, object>>.Create(new TestSetIndexBinder());
            var setMember = CallSite<Func<CallSite, object, long, object>>.Create(new TestSetMemberBinder("SimpleProperty"));
            #endregion

            //Perform each action, they should all fail and fall back to the given CallSiteBinder
            AssertExceptionThrown<BindingException>(() => call.Target(call, comObj));
            AssertExceptionThrown<BindingException>(() => convert.Target(convert, comObj));
            AssertExceptionThrown<BindingException>(() => create.Target(create, comObj));
            AssertExceptionThrown<BindingException>(() => deleteIndex.Target(deleteIndex, comObj, 3));
            AssertExceptionThrown<BindingException>(() => deleteMember.Target(deleteMember, comObj));
            AssertExceptionThrown<BindingException>(() => getIndex.Target(getIndex, comObj, 3));
            AssertExceptionThrown<BindingException>(() => getMember.Target(getMember, comObj));
            AssertExceptionThrown<BindingException>(() => invoke.Target(invoke, comObj));
            AssertExceptionThrown<BindingException>(() => binaryOperation.Target(binaryOperation, comObj, 13));
            AssertExceptionThrown<BindingException>(() => unaryOperation.Target(unaryOperation, comObj));
            AssertExceptionThrown<BindingException>(() => setIndex.Target(setIndex, comObj, 3, 4));
            AssertExceptionThrown<BindingException>(() => setMember.Target(setMember, comObj, 5));
        }

        [Test(TestState.COM, "Tests the COM binder against null arguments")]
        private void Scenario_COM_Nulls() {
            //Method calls
            Type comType = Type.GetTypeFromProgID("DlrComLibrary.ParamsInRetVal");
            Assert.IsNotNull(comType, "Could not retrieve DlrComLibrary.NonDispatch.  Make sure you have registered DlrComLibrary.dll on this machine");
            var comObj = Activator.CreateInstance(comType);

            var call_mIDispatch = CallSite<Func<CallSite, object, object, object>>.Create(new TestInvokeMemberBinder("mIDispatch"));
            var call_mVariant = CallSite<Func<CallSite, object, object, object>>.Create(new TestInvokeMemberBinder("mVariant"));
            var getMember_mIDispatch = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("mIDispatch"));
            var invoke = CallSite<Func<CallSite, object, object, object>>.Create(new TestInvokeBinder());

            //Assert.IsNull(call_mIDispatch.Target(call_mIDispatch, comObj, null));
            Assert.IsNull(call_mVariant.Target(call_mVariant, comObj, null));
            var mIDispatch = getMember_mIDispatch.Target(getMember_mIDispatch, comObj);
            //Assert.IsNull(invoke.Target(invoke, mIDispatch, null));

            //Property sets and gets
            comType = Type.GetTypeFromProgID("DlrComLibrary.Properties");
            comObj = Activator.CreateInstance(comType);

            var setMember_pBstr = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("pBstr"));
            var setMember_pVariant = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("pVariant"));
            var getMember_pBstr = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("pBstr"));
            var getMember_pVariant = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("pVariant"));

            setMember_pBstr.Target(setMember_pBstr, comObj, null);
            Assert.AreEqual(String.Empty, getMember_pBstr.Target(getMember_pBstr, comObj));
            setMember_pVariant.Target(setMember_pVariant, comObj, null);
            Assert.IsNull(getMember_pVariant.Target(getMember_pVariant, comObj));
        }

        [Test("Simple cases for instance restrictions")]
        private void Scenario_Restriction_Instance() {
        }

        [Test("Simple cases for type restrictions")]
        private void Scenario_Restriction_Type() {
        }

        [Test("Simple cases for expression restrictions")]
        private void Scenario_Restriction_Expression() {
        }

        [Test("Confirm that the DynamicObject MetaObject code invokes each overriden action")]
        private void Scenario_Dynamic_Overriden() {
            //Get a simple Dynamic IDO
            TestDynamicObject2 dyn = new TestDynamicObject2();

            #region CallSite for each MetaAction
            var call = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeMemberBinder("member"));
            var convert = CallSite<Func<CallSite, object, string>>.Create(new TestConvertBinder(typeof(String), true));
            var create = CallSite<Func<CallSite, object, object>>.Create(new TestCreateBinder());
            var deleteIndex = CallSite<Action<CallSite, object>>.Create(new TestDeleteIndexBinder());
            var deleteMember = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("member"));
            var getIndex = CallSite<Func<CallSite, object, object>>.Create(new TestGetIndexBinder());
            var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("member"));
            var invoke = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeBinder());
            var unaryOperation = CallSite<Func<CallSite, object, object>>.Create(new TestUnaryOperationBinder(ExpressionType.Increment));
            var binaryOperation = CallSite<Func<CallSite, object, object, object>>.Create(new TestBinaryOperationBinder(ExpressionType.Add));
            var setIndex = CallSite<Func<CallSite, object, object, object, object>>.Create(new TestSetIndexBinder());
            var setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("member"));
            #endregion

            //Invoke each CallSite
            Assert.AreEqual("InvokeMember", call.Target(call, dyn));
            Assert.AreEqual("Convert", convert.Target(convert, dyn));
            Assert.AreEqual("CreateInstance", create.Target(create, dyn));
            AssertExceptionThrown<BindingException>(() => deleteIndex.Target(deleteIndex, dyn));
            AssertExceptionThrown<BindingException>(() => deleteMember.Target(deleteMember, dyn));
            Assert.AreEqual("GetIndex", getIndex.Target(getIndex, dyn));
            Assert.AreEqual("GetMember", getMember.Target(getMember, dyn));
            Assert.AreEqual("Invoke", invoke.Target(invoke, dyn));
            Assert.AreEqual("BinaryOperation", binaryOperation.Target(binaryOperation, dyn, 5));
            Assert.AreEqual("UnaryOperation", unaryOperation.Target(unaryOperation, dyn));
            AssertExceptionThrown<BindingException>(() => setIndex.Target(setIndex, dyn, 3, 4));
            AssertExceptionThrown<BindingException>(() => setMember.Target(setMember, dyn, 5));
        }

        [Test("Simple tests of using standard .NET non-IDO types with the metaobjectbinders")]
        private void Scenario_GenericMO() {
            object[] vars = new object[] { "Hello world", 42 };

            #region CallSite for each MetaAction on this IDO
            var call = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeMemberBinder("member"));
            var convert = CallSite<Func<CallSite, object, string>>.Create(new TestConvertBinder(typeof(String), true));
            var create = CallSite<Func<CallSite, object, object>>.Create(new TestCreateBinder());
            var deleteIndex = CallSite<Action<CallSite, object>>.Create(new TestDeleteIndexBinder());
            var deleteMember = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("member"));
            var getIndex = CallSite<Func<CallSite, object, object>>.Create(new TestGetIndexBinder());
            var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("member"));
            var invoke = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeBinder());
            var binaryOperation = CallSite<Func<CallSite, object, object, object>>.Create(new TestBinaryOperationBinder(ExpressionType.Add));
            var unaryOperation = CallSite<Func<CallSite, object, object>>.Create(new TestUnaryOperationBinder(ExpressionType.Increment));
            var setIndex = CallSite<Func<CallSite, object, object, object, object>>.Create(new TestSetIndexBinder());
            var setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("member"));
            #endregion

            //Should all fallback to the callsitebinder
            foreach (object v in vars) {
                AssertExceptionThrown<BindingException>(() => call.Target(call, v));
                AssertExceptionThrown<BindingException>(() => convert.Target(convert, v));
                AssertExceptionThrown<BindingException>(() => create.Target(create, v));
                AssertExceptionThrown<BindingException>(() => deleteIndex.Target(deleteIndex, v));
                AssertExceptionThrown<BindingException>(() => deleteMember.Target(deleteMember, v));
                AssertExceptionThrown<BindingException>(() => getIndex.Target(getIndex, v));
                AssertExceptionThrown<BindingException>(() => getMember.Target(getMember, v));
                AssertExceptionThrown<BindingException>(() => invoke.Target(invoke, v));
                AssertExceptionThrown<BindingException>(() => binaryOperation.Target(binaryOperation, v, 5));
                AssertExceptionThrown<BindingException>(() => unaryOperation.Target(unaryOperation, v));
                AssertExceptionThrown<BindingException>(() => setIndex.Target(setIndex, v, 3, 4));
                AssertExceptionThrown<BindingException>(() => setMember.Target(setMember, v, 5));
            }
        }

        [Test("Confirm that the base virtual methods on DynamicObject all do not throw, but return false")]
        private void Scenario_Dynamic_Negative() {
            //Get a simple Dynamic IDO
            TestDynamicObject dyn = new TestDynamicObject();
            object result = null;

            Assert.IsFalse(dyn.TryBinaryOperation(new TestBinaryOperationBinder(ExpressionType.Add), null, out result));
            Assert.IsNull(result);
            Assert.IsFalse(dyn.TryConvert(new TestConvertBinder(typeof(String), true), out result));
            Assert.IsNull(result);
            Assert.IsFalse(dyn.TryCreateInstance(new TestCreateBinder(), null, out result));
            Assert.IsNull(result);
            Assert.IsFalse(dyn.TryDeleteIndex(new TestDeleteIndexBinder(), null));
            Assert.IsFalse(dyn.TryDeleteMember(new TestDeleteMemberBinder("member")));
            Assert.IsFalse(dyn.TryGetIndex(new TestGetIndexBinder(), null, out result));
            Assert.IsNull(result);
            Assert.IsFalse(dyn.TryGetMember(new TestGetMemberBinder("member"), out result));
            Assert.IsNull(result);
            Assert.IsFalse(dyn.TryInvoke(new TestInvokeBinder(), null, out result));
            Assert.IsNull(result);
            Assert.IsFalse(dyn.TryInvokeMember(new TestInvokeMemberBinder("member"), null, out result));
            Assert.IsNull(result);
            Assert.IsFalse(dyn.TrySetIndex(new TestSetIndexBinder(), null, null));
            Assert.IsFalse(dyn.TrySetMember(new TestSetMemberBinder("member"), null));
            Assert.IsFalse(dyn.TryUnaryOperation(new TestUnaryOperationBinder(ExpressionType.Increment), out result));
            Assert.IsNull(result);
        }
        #endregion



        [Test("Binder atomization")]
        private void Scenario_ManyBinders1() {
            #region CallSite for each action
            var setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("HaHa"));
            var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("HaHa"));
            #endregion

            // sanity check with expando
            ExpandoObject exp = new ExpandoObject();

            setMember.Target(setMember, exp, 52);
            Assert.AreEqual(52, getMember.Target(getMember, exp));


            var getMember42 = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("HaHa", false, 42));
            Assert.AreEqual(52, getMember42.Target(getMember42, exp));

            // create many shortlived binders of same identity
            for (int i = 0; i < 10000; i++) {
                var getMemberI = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("HaHa", false, 42));
                Assert.AreEqual(52, getMemberI.Target(getMemberI, exp));
            }

            Assert.AreEqual(52, getMember42.Target(getMember42, exp));
        }

        [Test("Binder life times")]
        private void Scenario_ManyBinders2() {
            #region CallSite for each action
            var setMember = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("HaHa"));
            var getMember = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("HaHa"));
            #endregion

            // sanity check with expando
            ExpandoObject exp = new ExpandoObject();

            setMember.Target(setMember, exp, 52);
            Assert.AreEqual(52, getMember.Target(getMember, exp));

            // create many shortlived binders of different indentity
            for (int i = 0; i < 10000; i++) {
                var getMemberI = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("HaHa", false, i));
                Assert.AreEqual(52, getMemberI.Target(getMemberI, exp));
            }
        }

        [Test("Passing IDMOP without restrictions to a standard binder causes InvalidOperationExceptions")]
        private void Scenario_InsufficientRestrictions() {
            var invoke = CallSite<Func<CallSite, string, string>>.Create(new TestBadGetMemberBinder("Something"));
            AssertExceptionThrown<InvalidOperationException>(() => invoke.Target(invoke, "hello"));
        }

        [Test("Ensure binding against delegates works")]
        private void Scenario_DelegateBindings() {
            //The string argument to this callsite tells DelegateBinder which 
            //method to bind to, Bind or Target1.  Target1 is a delegate, Bind
            //is the typical expression binding.  Further, binder exposes
            //two toggles to control whether the rules returned by each binding
            //pass or fail.
            var binder = new DelegateBinder();
            var site = CallSite<Func<CallSite, string, string>>.Create(binder);

            //Caches are empty, bind against the typical expression and ensure it worked
            binder.BindTest = true;
            binder.Target1Test = true;
            Assert.AreEqual("Bind", site.Target(site, "Bind"));

            //Should find the initial rule in the cache and not re-bind
            Assert.AreEqual("Bind", site.Target(site, "Target1"));

            //Make the expression fail and we re-bind to the delegate
            binder.BindTest = false;
            Assert.AreEqual("Target1", site.Target(site, "Target1"));

            //Create a new callsite to clear l0 and l1
            site = CallSite<Func<CallSite, string, string>>.Create(binder);

            //Set BindTest to pass, and we should find it in l2
            binder.BindTest = true;
            Assert.AreEqual("Bind", site.Target(site, "Target1"));

            //Reset the binder and all caches
            binder.BindTest = true;
            ClearRuleCache(site);
            site = CallSite<Func<CallSite, string, string>>.Create(binder);

            //Now bind to the delegate
            Assert.AreEqual("Target1", site.Target(site, "Target1"));

            //Create a new callsite to clear l0 and l1
            site = CallSite<Func<CallSite, string, string>>.Create(binder);

            //Ensure the delegate did not enter l2
            Assert.AreEqual("Bind", site.Target(site, "Bind"));

            //Reset all caches
            ClearRuleCache(site);
            site = CallSite<Func<CallSite, string, string>>.Create(binder);

            //Now bind to the delegate
            Assert.AreEqual("Target1", site.Target(site, "Target1"));

            //And again, the delegate is cached, we should not re-bind
            Assert.AreEqual("Target1", site.Target(site, "Bind"));

            //Make the delegate fail and we re-bind to the expression
            binder.Target1Test = false;
            Assert.AreEqual("Bind", site.Target(site, "Bind"));
        }

        [Test("Test invalid calls to base binder constructors.")]
        private void Scenario_TestBinderArgumentChecking() {
            // BinaryOperationBinder
            AssertExceptionThrown<ArgumentException>(delegate() { new TestBinaryOperationBinder(ExpressionType.Switch); });
            // ConvertBinder
            AssertExceptionThrown<ArgumentNullException>(delegate() { new TestConvertBinder(null, true); });
            // CreateInstanceBinder
            AssertExceptionThrown<ArgumentNullException>(delegate() { new TestCreateBinder(null); });
            // DeleteIndexBinder
            AssertExceptionThrown<ArgumentNullException>(delegate() { new TestDeleteIndexBinder(null); });
            // DeleteMemberBinder
            AssertExceptionThrown<ArgumentNullException>(delegate() { new TestDeleteMemberBinder(null, true); });
            // GetIndexBinder
            AssertExceptionThrown<ArgumentNullException>(delegate() { new TestGetIndexBinder(null); });
            // GetMemberBinder
            AssertExceptionThrown<ArgumentNullException>(delegate { new TestGetMemberBinder(null, true); });
            // InvokeBinder
            AssertExceptionThrown<ArgumentNullException>(delegate { new TestInvokeBinder(null); });
            // InvokeMemberBinder
            AssertExceptionThrown<ArgumentNullException>(delegate { new TestInvokeMemberBinder(null, true, new CallInfo(0)); });
            AssertExceptionThrown<ArgumentNullException>(delegate { new TestInvokeMemberBinder("Hello", true, null); });
            // SetIndexBinder
            AssertExceptionThrown<ArgumentNullException>(delegate { new TestSetIndexBinder(null); });
            // SetMemberBinder
            AssertExceptionThrown<ArgumentNullException>(delegate { new TestSetMemberBinder(null, true); });
            // UnaryOperationBinder
            AssertExceptionThrown<ArgumentException>(delegate { new TestUnaryOperationBinder(ExpressionType.Switch); });
        }

        private static DynamicMetaObject CreateDMO(string name, object value) {
            return new DynamicMetaObject(Expression.Parameter(typeof(object), name), BindingRestrictions.Empty, value);
        }

        private static DynamicMetaObject[] CreateDMOArray(int length) {
            var array = new DynamicMetaObject[length];
            for (int i = 0; i < length; i++) {
                array[i] = CreateDMO("arg" + i, "value" + i);
            }
            return array;
        }

        [Test("Test invalid calls to binder Bind methods.")]
        private void Scenario_TestBinderBindArgumentChecking() {
            var dmo = CreateDMO("Target", new object());

            var dmo_0 = new DynamicMetaObject[0];
            var dmo_1 = CreateDMOArray(1);
            var dmo_2 = CreateDMOArray(2);
            var dmo_5 = CreateDMOArray(5);

            var null_1 = new DynamicMetaObject[1];
            var null_2 = CreateDMOArray(2); null_2[1] = null;
            var null_5 = CreateDMOArray(5); null_5[4] = null;

            // BinaryOperationBinder
            BinaryOperationBinder binary = new TestBinaryOperationBinder(ExpressionType.Add);
            AssertExceptionThrown<ArgumentNullException>(delegate() { binary.Bind(null, dmo_1); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { binary.Bind(dmo, null); });
            AssertExceptionThrown<ArgumentException>(delegate() { binary.Bind(dmo, dmo_2); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { binary.Bind(dmo, null_1); });

            // ConvertBinder
            ConvertBinder convert = new TestConvertBinder(typeof(string), true);
            AssertExceptionThrown<ArgumentNullException>(delegate() { convert.Bind(null, dmo_0); });
            AssertExceptionThrown<ArgumentException>(delegate() { convert.Bind(dmo, dmo_1); });
            AssertExceptionThrown<ArgumentException>(delegate() { convert.Bind(dmo, null_5); });

            // CreateInstanceBinder
            CreateInstanceBinder create = new TestCreateBinder(new CallInfo(2));
            AssertExceptionThrown<ArgumentNullException>(delegate() { create.Bind(null, dmo_2); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { create.Bind(dmo, null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { create.Bind(dmo, null_5); });

            // DeleteIndexBinder
            DeleteIndexBinder di = new TestDeleteIndexBinder(new CallInfo(2));
            AssertExceptionThrown<ArgumentNullException>(delegate() { di.Bind(null, dmo_2); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { di.Bind(dmo, null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { di.Bind(dmo, null_2); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { di.Bind(dmo, null_5); });

            // DeleteMemberBinder
            DeleteMemberBinder dm = new TestDeleteMemberBinder("Member", true);
            AssertExceptionThrown<ArgumentNullException>(delegate() { dm.Bind(null, dmo_0); });
            AssertExceptionThrown<ArgumentException>(delegate() { dm.Bind(dmo, dmo_1); });

            // GetIndexBinder
            GetIndexBinder gi = new TestGetIndexBinder(new CallInfo(2));
            AssertExceptionThrown<ArgumentNullException>(delegate() { gi.Bind(null, dmo_2); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { gi.Bind(dmo, null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { gi.Bind(dmo, null_2); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { gi.Bind(dmo, null_5); });

            // GetMemberBinder
            GetMemberBinder gm = new TestGetMemberBinder("Member", true);
            AssertExceptionThrown<ArgumentNullException>(delegate { gm.Bind(null, dmo_0); });
            AssertExceptionThrown<ArgumentException>(delegate { gm.Bind(dmo, dmo_1); });
            AssertExceptionThrown<ArgumentException>(delegate { gm.Bind(dmo, dmo_5); });

            // InvokeBinder
            InvokeBinder invoke = new TestInvokeBinder(new CallInfo(2));
            AssertExceptionThrown<ArgumentNullException>(delegate { invoke.Bind(null, dmo_2); });
            AssertExceptionThrown<ArgumentNullException>(delegate { invoke.Bind(dmo, null); });
            AssertExceptionThrown<ArgumentNullException>(delegate { invoke.Bind(dmo, null_2); });
            AssertExceptionThrown<ArgumentNullException>(delegate { invoke.Bind(dmo, null_5); });

            // InvokeMemberBinder
            InvokeMemberBinder im = new TestInvokeMemberBinder("Hello", true, new CallInfo(2));
            AssertExceptionThrown<ArgumentNullException>(delegate { im.Bind(null, dmo_2); });
            AssertExceptionThrown<ArgumentNullException>(delegate { im.Bind(dmo, null); });
            AssertExceptionThrown<ArgumentNullException>(delegate { im.Bind(dmo, null_2); });
            AssertExceptionThrown<ArgumentNullException>(delegate { im.Bind(dmo, null_5); });

            // SetIndexBinder
            SetIndexBinder si = new TestSetIndexBinder(new CallInfo(2));
            var null_54 = CreateDMOArray(5); null_54[3] = null;
            AssertExceptionThrown<ArgumentNullException>(delegate { si.Bind(null, dmo_2); });
            AssertExceptionThrown<ArgumentException>(delegate { si.Bind(dmo, dmo_0); });
            AssertExceptionThrown<ArgumentNullException>(delegate { si.Bind(dmo, null); });
            AssertExceptionThrown<ArgumentNullException>(delegate { si.Bind(dmo, null_5); });
            AssertExceptionThrown<ArgumentNullException>(delegate { si.Bind(dmo, null_54); });

            // SetMemberBinder
            SetMemberBinder sm = new TestSetMemberBinder("Member", true);
            AssertExceptionThrown<ArgumentNullException>(delegate { sm.Bind(null, dmo_1); });
            AssertExceptionThrown<ArgumentException>(delegate { sm.Bind(dmo, dmo_2); });
            AssertExceptionThrown<ArgumentNullException>(delegate { sm.Bind(dmo, null); });
            AssertExceptionThrown<ArgumentNullException>(delegate { sm.Bind(dmo, null_1); });
            AssertExceptionThrown<ArgumentException>(delegate { sm.Bind(dmo, null_5); });

            // UnaryOperationBinder
            UnaryOperationBinder unary = new TestUnaryOperationBinder(ExpressionType.Negate);
            AssertExceptionThrown<ArgumentNullException>(delegate { unary.Bind(null, dmo_0); });
            AssertExceptionThrown<ArgumentException>(delegate { unary.Bind(dmo, dmo_2); });

            // DynamicMetaObjectBinder
            SetIndexBinder dmob = new TestSetIndexBinder(new CallInfo(2));
            AssertExceptionThrown<ArgumentOutOfRangeException>(delegate {
                dmob.Bind(new object[] { }, new System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression>(new ParameterExpression[] { Expression.Parameter(typeof(Int32)) }), Expression.Label());
            });
            AssertExceptionThrown<ArgumentOutOfRangeException>(delegate
            {
                dmob.Bind(new object[] { new object() }, new System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression>(new ParameterExpression[] { }), Expression.Label());
            });
            AssertExceptionThrown<ArgumentOutOfRangeException>(delegate
            {
                dmob.Bind(new object[] { new object(), new object() }, new System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression>(new ParameterExpression[] { Expression.Parameter(typeof(Int32)) }), Expression.Label());
            });
        }

        class Regress754079_DO : DynamicObject {
            public object useResult = null;
            public override bool TryConvert(ConvertBinder binder, out object result) {
                result = useResult;
                return true;
            }
        }

        [Test("Regression test for Dev10 bug 754079")]
        private void Regress754079() {
            //This test is only valid on .NET 4.5, which internally is still
            //versioned 4.0 with just a higher build number.  4.0 RTM was
            //build 30319.
            if (System.Environment.Version >= new Version(4, 0, 30322, 0)) {
                Regress754079_DO dynObj = new Regress754079_DO();

                // Converting null -> bool (negative)
                dynObj.useResult = null;
                var convert1 = CallSite<Func<CallSite, object, bool>>.Create(new TestConvertBinder(typeof(bool), true));
                AssertExceptionThrown<InvalidCastException>(() => convert1.Target(convert1, dynObj),
                    "The result type 'null' of the dynamic binding produced by the object with type 'SiteTest.SiteTestScenarios+Regress754079_DO' for the binder 'SiteTest.Actions.TestConvertBinder' is not compatible with the result type 'System.Boolean' expected by the call site.");

                // Converting object -> string (negative)
                dynObj.useResult = new object();
                var convert5 = CallSite<Func<CallSite, object, string>>.Create(new TestConvertBinder(typeof(string), true));
                AssertExceptionThrown<InvalidCastException>(() => convert5.Target(convert5, dynObj),
                    "The result type 'System.Object' of the dynamic binding produced by the object with type 'SiteTest.SiteTestScenarios+Regress754079_DO' for the binder 'SiteTest.Actions.TestConvertBinder' is not compatible with the result type 'System.String' expected by the call site.");

                // Converting object -> int (negative)
                dynObj.useResult = new object();
                var convert7 = CallSite<Func<CallSite, object, int>>.Create(new TestConvertBinder(typeof(int), true));
                AssertExceptionThrown<InvalidCastException>(() => convert7.Target(convert7, dynObj),
                    "The result type 'System.Object' of the dynamic binding produced by the object with type 'SiteTest.SiteTestScenarios+Regress754079_DO' for the binder 'SiteTest.Actions.TestConvertBinder' is not compatible with the result type 'System.Int32' expected by the call site.");

                // Converting float -> string (negative)
                dynObj.useResult = 3.1415968;
                var convert6 = CallSite<Func<CallSite, object, string>>.Create(new TestConvertBinder(typeof(string), true));
                AssertExceptionThrown<InvalidCastException>(() => convert6.Target(convert6, dynObj),
                    "The result type 'System.Double' of the dynamic binding produced by the object with type 'SiteTest.SiteTestScenarios+Regress754079_DO' for the binder 'SiteTest.Actions.TestConvertBinder' is not compatible with the result type 'System.String' expected by the call site.");

                // Converting TestAttribute -> Attribute (positive)
                dynObj.useResult = new TestAttribute();
                var convert2 = CallSite<Func<CallSite, object, Attribute>>.Create(new TestConvertBinder(typeof(Attribute), true));
                convert2.Target(convert2, dynObj);

                // Converting int -> IEquatable<int> (positive)
                dynObj.useResult = 1;
                var convert3 = CallSite<Func<CallSite, object, IEquatable<int>>>.Create(new TestConvertBinder(typeof(IEquatable<int>), true));
                convert3.Target(convert3, dynObj);

                // Converting ExpandoObject -> ExpandoObject (positive)
                dynObj.useResult = new ExpandoObject();
                var convert4 = CallSite<Func<CallSite, object, ExpandoObject>>.Create(new TestConvertBinder(typeof(ExpandoObject), true));
                convert4.Target(convert4, dynObj);
            }
        }
        delegate void RefDel012(CallSite site, object dynObj, ref int i, ref int j, ref int k);
        delegate void RefDel01_(CallSite site, object dynObj, ref int i, ref int j, int k);
        delegate void RefDel_12(CallSite site, object dynObj, int i, ref int j, ref int k);
        delegate void RefDel0_2(CallSite site, object dynObj, ref int i, int j, ref int k);
        delegate void RefDel0__(CallSite site, object dynObj, ref int i, int j, int k);
        delegate void RefDel_1_(CallSite site, object dynObj, int i, ref int j, int k);
        delegate void RefDel__2(CallSite site, object dynObj, int i, int j, ref int k);
        delegate void RefDel___(CallSite site, object dynObj, int i, int j, int k);

        [Test("Test byref arguments to DynamicObject invocation")]
        private void Scenario_ByRef() {
            //This test is only valid on .NET 4.5, which internally is still
            //versioned 4.0 with just a higher build number.  4.0 RTM was
            //build 30319.
            if (System.Environment.Version >= new Version(4, 0, 30322, 0)) {
                int i, j, k;
                ByRefDynamicObject dynObj = new ByRefDynamicObject();

                CallSiteBinder[] binders = {   new TestInvokeBinder(),
                                               new TestInvokeMemberBinder("Method"),
                                               new TestGetIndexBinder(),
                                               new TestSetIndexBinder(),
                                               new TestDeleteIndexBinder()
                                           };

                foreach (CallSiteBinder b in binders) {
                    var site0 = CallSite<RefDel012>.Create(b);
                    i = j = k = 2;
                    site0.Target(site0, dynObj, ref i, ref j, ref k);
                    Assert.AreEqual(42, i);
                    Assert.AreEqual(43, j);
                    if(!(b is TestSetIndexBinder))
                        Assert.AreEqual(44, k);

                    var site1 = CallSite<RefDel01_>.Create(b);
                    i = j = k = 2;
                    site1.Target(site1, dynObj, ref i, ref j, k);
                    Assert.AreEqual(42, i);
                    Assert.AreEqual(43, j);
                    Assert.AreEqual(2, k);

                    var site2 = CallSite<RefDel_12>.Create(b);
                    i = j = k = 2;
                    site2.Target(site2, dynObj, i, ref j, ref k);
                    Assert.AreEqual(2, i);
                    Assert.AreEqual(43, j);
                    if (!(b is TestSetIndexBinder))
                        Assert.AreEqual(44, k);

                    var site3 = CallSite<RefDel0_2>.Create(b);
                    i = j = k = 2;
                    site3.Target(site3, dynObj, ref i, j, ref k);
                    Assert.AreEqual(42, i);
                    Assert.AreEqual(2, j);
                    if (!(b is TestSetIndexBinder))
                        Assert.AreEqual(44, k);

                    var site4 = CallSite<RefDel0__>.Create(b);
                    i = j = k = 2;
                    site4.Target(site4, dynObj, ref i, j, k);
                    Assert.AreEqual(42, i);
                    Assert.AreEqual(2, j);
                    Assert.AreEqual(2, k);

                    var site5 = CallSite<RefDel_1_>.Create(b);
                    i = j = k = 2;
                    site5.Target(site5, dynObj, i, ref j, k);
                    Assert.AreEqual(2, i);
                    Assert.AreEqual(43, j);
                    Assert.AreEqual(2, k);

                    var site6 = CallSite<RefDel__2>.Create(b);
                    i = j = k = 2;
                    site6.Target(site6, dynObj, i, j, ref k);
                    Assert.AreEqual(2, i);
                    Assert.AreEqual(2, j);
                    if (!(b is TestSetIndexBinder))
                        Assert.AreEqual(44, k);

                    var site7 = CallSite<RefDel___>.Create(b);
                    i = j = k = 2;
                    site7.Target(site7, dynObj, i, j, k);
                    Assert.AreEqual(2, i);
                    Assert.AreEqual(2, j);
                    Assert.AreEqual(2, k);
                }
            }
        }
    }
}
