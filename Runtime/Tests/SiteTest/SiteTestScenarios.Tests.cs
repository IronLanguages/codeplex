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
using System.Runtime.CompilerServices; //For the Assert/StringAssert/CollectionAssert classes
using System.Threading;
using SiteTest.Actions;

namespace SiteTest {
    partial class SiteTestScenarios {
        #region Test Scenarios.  Add new CallSite tests here.  They should be private instance methods taking no parameters.
        [Test("Basic sanity checks of dynamic sites")]
        private void Scenario_Basic() {
            //Create a simple rule
            RuleBuilder rule = (p, r) =>
                Expression.Condition(
                    Expression.Block(_log.GenLog(SiteLog.EventType.TestInvocation,"test"), Expression.Constant(true)),
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, p[0]),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Apply the rule to the binder
            _sitebinder.SetRules(rule);

            //Now create our dynamic site
            var site = CallSite<Func<CallSite, string, object>>.Create(_sitebinder);
            ClearRuleCache(site);

            //Verify the initial state of the site
            //Assert.AreSame(site.Target, site.Update);

            //Invoke the site for the first time and time it
            long time1 = _log.TimeScenario("Initial invocation", delegate() {
                site.Target(site, "first");
            });

            //Verify the site's state has changed
            //Assert.AreNotSame(site.Target, site.Update);

            //Invoke the site for the second time and time it
            long time2 = _log.TimeScenario("Secondary invocation", delegate() {
                site.Target(site, "second");
            });

            //Ensure the results match what we expected
            Assert.IsTrue(time1 > time2, "Secondary invocation of a dynamic site has become slower than the initial invocation.  This is generally bad.  Are the CallSite caches working?");
            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.ScenarioBegin),
                _log.CreateEvent(SiteLog.EventType.MakeRule),
                _log.CreateEvent(SiteLog.EventType.TestInvocation),
                _log.CreateEvent(SiteLog.EventType.TargetInvocation),
                _log.CreateEvent(SiteLog.EventType.ScenarioEnd),
                _log.CreateEvent(SiteLog.EventType.ScenarioBegin),
                _log.CreateEvent(SiteLog.EventType.TestInvocation),
                _log.CreateEvent(SiteLog.EventType.TargetInvocation),
                _log.CreateEvent(SiteLog.EventType.ScenarioEnd)), "Unexpected site invocation sequence");
        }

        [Test("Retrieve rules from the L2 cache")]
        private void Scenario_L2_Cache_Hits()
        {
            bool[] funcFlags = new bool[11];
            bool[] actionFlags = new bool[12];

            //Create a binder and fil the L2 cache with target delegates
            var binder = new SiteBinder();
            binder.ClearL2();
            binder.AddToL2((Func<CallSite, object>)delegate(CallSite cs) { funcFlags[0] = true; return new object(); });
            binder.AddToL2((Func<CallSite, object, object>)delegate(CallSite cs, object a) { funcFlags[1] = true; return new object(); });
            binder.AddToL2((Func<CallSite, object, object, object>)delegate(CallSite cs, object a, object b) { funcFlags[2] = true; return new object(); });
            binder.AddToL2((Func<CallSite, object, object, object, object>)delegate(CallSite cs, object a, object b, object c) { funcFlags[3] = true; return new object(); });
            binder.AddToL2((Func<CallSite, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d) { funcFlags[4] = true; return new object(); });
            binder.AddToL2((Func<CallSite, object, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e) { funcFlags[5] = true; return new object(); });
            binder.AddToL2((Func<CallSite, object, object, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e, object f) { funcFlags[6] = true; return new object(); });
            binder.AddToL2((Func<CallSite, object, object, object, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e, object f, object g) { funcFlags[7] = true; return new object(); });
            binder.AddToL2((Func<CallSite, object, object, object, object, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e, object f, object g, object h) { funcFlags[8] = true; return new object(); });
            binder.AddToL2((Func<CallSite, object, object, object, object, object, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e, object f, object g, object h, object i) { funcFlags[9] = true; return new object(); });
            binder.AddToL2((Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e, object f, object g, object h, object i, object j) { funcFlags[10] = true; return new object(); });

            binder.AddToL2((Action<CallSite, object>)delegate(CallSite cs, object a) { actionFlags[1] = true; });
            binder.AddToL2((Action<CallSite, object, object>)delegate(CallSite cs, object a, object b) { actionFlags[2] = true; });
            binder.AddToL2((Action<CallSite, object, object, object>)delegate(CallSite cs, object a, object b, object c) { actionFlags[3] = true; });
            binder.AddToL2((Action<CallSite, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d) { actionFlags[4] = true; });
            binder.AddToL2((Action<CallSite, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e) { actionFlags[5] = true; });
            binder.AddToL2((Action<CallSite, object, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e, object f) { actionFlags[6] = true; });
            binder.AddToL2((Action<CallSite, object, object, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e, object f, object g) { actionFlags[7] = true; });
            binder.AddToL2((Action<CallSite, object, object, object, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e, object f, object g, object h) { actionFlags[8] = true; });
            binder.AddToL2((Action<CallSite, object, object, object, object, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e, object f, object g, object h, object i) { actionFlags[9] = true; });
            binder.AddToL2((Action<CallSite, object, object, object, object, object, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e, object f, object g, object h, object i, object j) { actionFlags[10] = true; });
            binder.AddToL2((Action<CallSite, object, object, object, object, object, object, object, object, object, object, object>)delegate(CallSite cs, object a, object b, object c, object d, object e, object f, object g, object h, object i, object j, object k) { actionFlags[11] = true; });


            var site = CallSite<Func<CallSite, object>>.Create(binder);
            site.Target(site);
            Assert.IsTrue(funcFlags[0]);

            var site1 = CallSite<Func<CallSite, object, object>>.Create(binder);
            site1.Target(site1,null);
            Assert.IsTrue(funcFlags[1]);

            var site2 = CallSite<Func<CallSite, object, object, object>>.Create(binder);
            site2.Target(site2,null,null);
            Assert.IsTrue(funcFlags[2]);

            var site3 = CallSite<Func<CallSite, object, object, object, object>>.Create(binder);
            site3.Target(site3,null,null,null);
            Assert.IsTrue(funcFlags[3]);

            var site4 = CallSite<Func<CallSite, object, object, object, object, object>>.Create(binder);
            site4.Target(site4,null,null,null,null);
            Assert.IsTrue(funcFlags[4]);

            var site5 = CallSite<Func<CallSite, object, object, object, object, object, object>>.Create(binder);
            site5.Target(site5,null,null,null,null,null);
            Assert.IsTrue(funcFlags[5]);

            var site6 = CallSite<Func<CallSite, object, object, object, object, object, object, object>>.Create(binder);
            site6.Target(site6,null,null,null,null,null,null);
            Assert.IsTrue(funcFlags[6]);

            var site7 = CallSite<Func<CallSite, object, object, object, object, object, object, object, object>>.Create(binder);
            site7.Target(site7,null,null,null,null,null,null,null);
            Assert.IsTrue(funcFlags[7]);

            var site8 = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object>>.Create(binder);
            site8.Target(site8,null,null,null,null,null,null,null,null);
            Assert.IsTrue(funcFlags[8]);

            var site9 = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object>>.Create(binder);
            site9.Target(site9,null,null,null,null,null,null,null,null,null);
            Assert.IsTrue(funcFlags[9]);

            var site10 = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>>.Create(binder);
            site10.Target(site10,null,null,null,null,null,null,null,null,null,null);
            Assert.IsTrue(funcFlags[10]);


            var asite1 = CallSite<Action<CallSite, object>>.Create(binder);
            asite1.Target(asite1, null);
            Assert.IsTrue(actionFlags[1]);

            var asite2 = CallSite<Action<CallSite, object, object>>.Create(binder);
            asite2.Target(asite2, null, null);
            Assert.IsTrue(actionFlags[2]);

            var asite3 = CallSite<Action<CallSite, object, object, object>>.Create(binder);
            asite3.Target(asite3, null, null, null);
            Assert.IsTrue(actionFlags[3]);

            var asite4 = CallSite<Action<CallSite, object, object, object, object>>.Create(binder);
            asite4.Target(asite4, null, null, null, null);
            Assert.IsTrue(actionFlags[4]);

            var asite5 = CallSite<Action<CallSite, object, object, object, object, object>>.Create(binder);
            asite5.Target(asite5, null, null, null, null, null);
            Assert.IsTrue(actionFlags[5]);

            var asite6 = CallSite<Action<CallSite, object, object, object, object, object, object>>.Create(binder);
            asite6.Target(asite6, null, null, null, null, null, null);
            Assert.IsTrue(actionFlags[6]);

            var asite7 = CallSite<Action<CallSite, object, object, object, object, object, object, object>>.Create(binder);
            asite7.Target(asite7, null, null, null, null, null, null, null);
            Assert.IsTrue(actionFlags[7]);

            var asite8 = CallSite<Action<CallSite, object, object, object, object, object, object, object, object>>.Create(binder);
            asite8.Target(asite8, null, null, null, null, null, null, null, null);
            Assert.IsTrue(actionFlags[8]);

            var asite9 = CallSite<Action<CallSite, object, object, object, object, object, object, object, object, object>>.Create(binder);
            asite9.Target(asite9, null, null, null, null, null, null, null, null, null);
            Assert.IsTrue(actionFlags[9]);

            var asite10 = CallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object>>.Create(binder);
            asite10.Target(asite10, null, null, null, null, null, null, null, null, null, null);
            Assert.IsTrue(actionFlags[10]);

            var asite11 = CallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object, object>>.Create(binder);
            asite11.Target(asite11, null, null, null, null, null, null, null, null, null, null, null);
            Assert.IsTrue(actionFlags[11]);
        }
        
        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Func0()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Func<CallSite,  object>>.Create(_sitebinder);
            ClearRuleCache<Func<CallSite,  object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Func1() {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Func<CallSite, string, object>>.Create(_sitebinder);
            ClearRuleCache<Func<CallSite, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site,null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation,      "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation,    "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation,      "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation,      "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation,      "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation,    "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation,      "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation,    "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation,      "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation,      "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation,    "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation,      "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation,    "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation,      "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation,      "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation,    "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Func2()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Func<CallSite, string,string, object>>.Create(_sitebinder);
            ClearRuleCache<Func<CallSite, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null,null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Func3()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Func<CallSite, string,string,string ,object>>.Create(_sitebinder);
            ClearRuleCache<Func<CallSite, string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null,null,null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Func4()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Func<CallSite, string,string,string,string, object>>.Create(_sitebinder);
            ClearRuleCache<Func<CallSite, string, string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null,null,null,null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Func5()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Func<CallSite, string,string,string,string,string, object>>.Create(_sitebinder);
            ClearRuleCache<Func<CallSite, string,string,string,string,string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null,null,null,null,null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Func6()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Func<CallSite, string, string, string, string, string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Func<CallSite, string, string,string,string,string,string,object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null,null,null,null,null,null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Func7()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Func<CallSite, string,string,string,string,string,string,string, object>>.Create(_sitebinder);
            ClearRuleCache<Func<CallSite, string, string, string, string, string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null,null,null,null,null,null,null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Func8()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Func<CallSite, string,string, string, string, string, string, string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Func<CallSite, string,string, string, string, string, string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null, null, null, null, null, null, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Func9()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Func<CallSite, string,string,string, string, string, string, string, string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Func<CallSite, string, string, string, string,string,string, string, string, string,object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null, null, null, null, null, null,null,null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Func10()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Func<CallSite, string, string, string, string, string, string, string, string, string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Func<CallSite, string, string,string, string, string, string, string, string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null, null, null, null, null, null, null, null, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Sub1()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Action<CallSite,  object>>.Create(_sitebinder);
            ClearRuleCache<Action<CallSite,  object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Sub2()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Action<CallSite,  string, object>>.Create(_sitebinder);
            ClearRuleCache<Action<CallSite,  string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Sub3()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Action<CallSite,  string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Action<CallSite,  string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Sub4()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Action<CallSite,  string, string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Action<CallSite,  string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null, null, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_sub5()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Action<CallSite, string, string, string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Action<CallSite, string, string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null, null, null, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Sub6()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Action<CallSite,  string, string, string, string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Action<CallSite,  string, string, string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null, null, null, null, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Sub7()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Action<CallSite,  string, string, string, string, string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Action<CallSite,  string, string, string, string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null, null, null, null, null, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Sub8()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Action<CallSite,  string, string, string, string, string, string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Action<CallSite,  string, string, string, string, string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null, null, null, null, null, null, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Sub9()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Action<CallSite,  string, string, string, string, string, string, string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Action<CallSite,  string, string, string, string, string, string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null, null, null, null, null, null, null, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Sub10()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Action<CallSite,  string, string, string, string, string, string, string, string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Action<CallSite,  string, string, string, string, string, string, string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null, null, null, null, null, null, null, null, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        [Test("Create and verify a simple polymorphic site, where polymorphic means more than one rule becomes part of the level 0 codegen'd cache.")]
        private void Scenario_Polymorphic_Sub11()
        {
            int test1, test2;

            Expression test1Expr = RuleTestDispenser.Create(this, "test1", out test1);
            Expression test2Expr = RuleTestDispenser.Create(this, "test2", out test2);

            //Create the first rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    test1Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Now the second
            RuleBuilder rule2 = (p, r) =>
                Expression.Condition(
                    test2Expr,
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target2"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook them to the binder
            _sitebinder.SetRules(rule1, rule2);

            //Create a CallSite
            var site = CallSite<Action<CallSite, string,string, string, string, string, string, string, string, string, string, object>>.Create(_sitebinder);
            ClearRuleCache<Action<CallSite, string,string, string, string, string, string, string, string, string, string, object>>(site);

            //Invoke once, causing the first rule to be cached in the callsite
            site.Target(site, null, null, null, null, null, null, null, null, null, null, null);

            //Make the first rule's test fail and invoke again
            //This creates rule2 via MakeRule
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null, null, null);

            //Let the first rule's test pass again and invoke
            //The more recent rule, rule2, is checked first
            RuleTestDispenser.Test[test1] = true;
            site.Target(site, null, null, null, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //rule1 is found in the level 1 cache
            //and pulled into level 0
            RuleTestDispenser.Test[test2] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null, null, null);

            //Let the second rule succeed again
            //it should still be in level 0 cache
            RuleTestDispenser.Test[test2] = true;
            site.Target(site, null, null, null, null, null, null, null, null, null, null, null);

            //Make the second rule fail and invoke
            //At this point rule1 should be found
            //in the level 0 cache and this site has
            //become truly polymorphic
            RuleTestDispenser.Test[test1] = false;
            site.Target(site, null, null, null, null, null, null, null, null, null, null, null);

            Assert.IsTrue(_log.MatchesEventSequence(
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule1
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0 (codegen)
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 2 (rulecache)
                _log.CreateEvent(SiteLog.EventType.MakeRule),                       //Creates rule2
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in MakeRule<T>
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 works in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),    //rule11 passes in cache level 0
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target1"),
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test1"),   //rule1 fails in cache level 0
                _log.CreateEvent(SiteLog.EventType.TestInvocation, "test2"),   //rule2 passes in cache level 1
                _log.CreateEvent(SiteLog.EventType.TargetInvocation, "target2")
                ), "Unexpected site invocation sequence");
        }

        /*
        [Test("Create and verify a simple megamorphic site")]
        private void Scenario_Megamorphic() {
        }

        [Test("Create and verify a simple serially monomorphic site")]
        private void Scenario_Serially_Monomorphic() {
        }
        */

        [Test(TestState.Disabled,"Not quite ready - Create site and simultaneously invoke it on >10 threads")]
        private void Scenario_Threaded1() {
            int test1;

            //Create a rule
            RuleBuilder rule1 = (p, r) =>
                Expression.Condition(
                    RuleTestDispenser.Create(this, "test1", out test1),
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, "target1"),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Hook it to the binder
            //@TODO - This scenario should invoke MakeRule many times, ensuring that this bombs without enough rules
            _sitebinder.SetRules(rule1);

            //Create a CallSite and clear its RuleCache
            var site = CallSite<Func<CallSite, string, object>>.Create(_sitebinder);
            ClearRuleCache(site);

            //@TODO - Need an easy way to check the number of MakeRule invocations in the log for a scenario

            //Spin up N threads and prepare them all to invoke the site
            Thread[] threads = new Thread[20]; //@TODO - Tweak this count as necessary
            for (int i=0; i<threads.Length; i++){
                ThreadStart ts = new ThreadStart(delegate() {
                    try {
                        site.Target(site, null);
                    } catch (ApplicationException) {
                    }
                });
                threads[i] = new Thread(ts);
            }

            foreach (Thread t in threads) {
                t.Start();
            }

            foreach (Thread t in threads) {
                t.Join();
            }

            Console.WriteLine(_log);
        }

        [Test("Miscellaneous negative API testing of CallSite<T> and CallSiteBinder")]
        private void Scenario_Binder_Misc() {
            //Mostly quick negative API cases here

            // Test that returning a null binding fails            
            RuleBuilder nullRule = (p, r) => null;
            _sitebinder.SetRules(nullRule);

            var site = CallSite<Func<CallSite, string, object>>.Create(_sitebinder);
            ClearRuleCache(site);
            AssertExceptionThrown<InvalidOperationException>(() => site.Target.Invoke(site, "test"));
        }


        [Test("RuleBuilder.AddTest cases")]
        private void Scenario_RuleBuilder_AddTest() {
        }

        [Test("RuleBuilder Type Test helper cases")]
        private void Scenario_RuleBuilder_RuleBuilder_TypeTests() {
        }

        delegate object BigFunc(
            CallSite arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10,
            object arg11, object arg12, object arg13, object arg14, object arg15, object arg16, object arg17, object arg18, object arg19, object arg20,
            object arg21, object arg22, object arg23, object arg24, object arg25, object arg26, object arg27, object arg28, object arg29, object arg30,
            object arg31, object arg32, object arg33, object arg34, object arg35, object arg36, object arg37, object arg38, object arg39, object arg40,
            object arg41, object arg42, object arg43, object arg44, object arg45, object arg46, object arg47, object arg48, object arg49, object arg50,
            object arg51, object arg52, object arg53, object arg54, object arg55, object arg56, object arg57, object arg58, object arg59, object arg60,
            object arg61, object arg62, object arg63, object arg64, object arg65, object arg66, object arg67, object arg68, object arg69, object arg70,
            object arg71, object arg72, object arg73, object arg74, object arg75, object arg76, object arg77, object arg78, object arg79, object arg80,
            object arg81, object arg82, object arg83, object arg84, object arg85, object arg86, object arg87, object arg88, object arg89, object arg90,
            object arg91, object arg92, object arg93, object arg94, object arg95, object arg96, object arg97, object arg98, object arg99, object arg100,
            object arg101, object arg102, object arg103, object arg104, object arg105, object arg106, object arg107, object arg108, object arg109, object arg110,
            object arg111, object arg112, object arg113, object arg114, object arg115, object arg116, object arg117, object arg118, object arg119, object arg120,
            object arg121, object arg122, object arg123, object arg124, object arg125, object arg126, object arg127, object arg128, object arg129, object arg130,
            object arg131, object arg132, object arg133, object arg134, object arg135, object arg136, object arg137, object arg138, object arg139, object arg140);


        [Test("Test invocation of a callsite with >127 arguments")]
        private void Scenario_Many_Args() {
            //Create a simple rule
            RuleBuilder rule = (p, r) =>
                Expression.Condition(
                    Expression.Block(_log.GenLog(SiteLog.EventType.TestInvocation, "test"), Expression.Constant(true)),
                    Expression.Return(
                        r,
                        Expression.Convert(
                            _log.GenLog(SiteLog.EventType.TargetInvocation, Expression.Call(typeof(Convert).GetMethod("ToString",new Type[]{typeof(Int32)}), Expression.Convert(p[138], typeof(Int32)))),
                            typeof(object)
                        )
                    ),
                    Expression.Empty()
                );

            //Apply the rule to the binder
            _sitebinder.SetRules(rule);

            //Now create our dynamic site
            var site = CallSite<BigFunc>.Create(_sitebinder);
            ClearRuleCache(site);

            //Invoke it
            site.Target(site,
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
                49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71,
                72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94,
                95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113,
                114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131,
                132, 133, 134, 135, 136, 137, 138, 139);

            //Ensure the target was invoked correctly
            var lastEvent = _log[_log.Length-1];
            Assert.AreEqual(lastEvent.EType, SiteLog.EventType.TargetInvocation);
            Assert.AreEqual(lastEvent.Description, "139");
        }

        [Test("Regression test for 880953")]
        private void Regress880953() {
            //This test is only valid on .NET 4.5, which internally is still
            //versioned 4.0 with just a higher build number.  4.0 RTM was
            //build 30319.
            if (System.Environment.Version >= new Version(4, 0, 30322, 0)) {
                AssertExceptionThrown<ArgumentException>(() => CallSite.Create(typeof(MulticastDelegate), new TestInvokeBinder()),
                   "Type must be derived from System.Delegate");
                AssertExceptionThrown<ArgumentException>(() => CallSite<MulticastDelegate>.Create(new TestInvokeBinder()),
                    "Type must be derived from System.Delegate");
                AssertExceptionThrown<ArgumentException>(() => DynamicExpression.MakeDynamic(typeof(MulticastDelegate), new TestInvokeBinder(), Expression.Empty()),
                    "Type must be derived from System.Delegate");
                AssertExceptionThrown<ArgumentException>(() => DynamicExpression.MakeDynamic(typeof(MulticastDelegate), new TestInvokeBinder(), Expression.Empty(), Expression.Empty()),
                    "Type must be derived from System.Delegate");
                AssertExceptionThrown<ArgumentException>(() => DynamicExpression.MakeDynamic(typeof(MulticastDelegate), new TestInvokeBinder(), Expression.Empty(), Expression.Empty(), Expression.Empty()),
                    "Type must be derived from System.Delegate");
                AssertExceptionThrown<ArgumentException>(() => DynamicExpression.MakeDynamic(typeof(MulticastDelegate), new TestInvokeBinder(), Expression.Empty(), Expression.Empty(), Expression.Empty(), Expression.Empty()),
                    "Type must be derived from System.Delegate");
                AssertExceptionThrown<ArgumentException>(() => DynamicExpression.MakeDynamic(typeof(MulticastDelegate), new TestInvokeBinder(), Expression.Empty(), Expression.Empty(), Expression.Empty(), Expression.Empty(), Expression.Empty()),
                    "Type must be derived from System.Delegate");
                AssertExceptionThrown<ArgumentException>(() => DynamicExpression.MakeDynamic(typeof(MulticastDelegate), new TestInvokeBinder(), new Expression[] { Expression.Empty(), Expression.Empty() }),
                    "Type must be derived from System.Delegate");
                AssertExceptionThrown<ArgumentException>(() => Expression.Lambda<MulticastDelegate>(Expression.Empty()),
                    "Lambda type parameter must be derived from System.Delegate");
            }
        }
        #endregion
    }
}
