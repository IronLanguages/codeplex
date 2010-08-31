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
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;
using IronPython.Hosting;
using IronRuby;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace PerfHost {
    public class Program {
        public static int RebindVersion;
        private static TestUtil m_util = new TestUtil();
        private static ComTypeLibInfo agentServerTypeLib;
        private static object agentServerObjects, agentServer;

        class TestInfo {
            public Action<long> Action;
            public long Iterations;

            public TestInfo(Action<long> action, long iterations) {
                Action = action;
                Iterations = iterations;
            }
        }

        static void Main(string[] p_args) {
            RunTests(p_args);
        }

        private static void RunTests(string[] p_args) {
            var testList = new[] {
                new TestInfo(CodeGenRuntimeVarsIntoType, 20000000),
                new TestInfo(CodeGenRuntimeVarsIntoDynamicMethod, 20000000),
                new TestInfo(CodeGenRuntimeVarsIntoType2, 20000000),
                new TestInfo(CodeGenRuntimeVarsIntoDynamicMethod2, 300000),

                new TestInfo(ComObjectSetItem, 200000),
                new TestInfo(ComObjectGetItem, 200000),
                new TestInfo(ComTypeLibGetEnumType, 100000000),
                new TestInfo(ComEnumDescGetValue, 100000000),
                new TestInfo(ComTypeLibGetClass, 100000000),
                new TestInfo(ComClassDescCreate, 100000),           
                //new TestInfo(ComObjectInvokeRetVoid, 1000000),
                new TestInfo(ComObjectGetPropertyString, 1000000),
                new TestInfo(ComObjectSetPropertyString, 1000000),
                //new TestInfo(ComObjectInvokeRetComObject, 100000),
                new TestInfo(ComObjectGetPropertyInt, 1000000),
                new TestInfo(ComObjectSetPropertyInt, 1000000),
                //new TestInfo(ComTypeLibDescCreateFromGuid, 20000),
                //new TestInfo(ComGetAgentServerObjects, 200000000),
                //new TestInfo(ComGetAgentServerObjectsNewSite, 1000000),
                //new TestInfo(ComGetAgentServer, 100000000),
                //new TestInfo(ComGetAgentServerNewSite, 1000000),
                //new TestInfo(ComCreateAgentServer, 200),
                //new TestInfo(ComCreateAgentServerNewSite, 200),

                new TestInfo(MetaRebindSameSite, 30000),
                new TestInfo(MetaRebindSameSite8, 30000),
                new TestInfo(MetaRebindSameSite16, 30000),
                new TestInfo(MetaNullSameSite, 50000000),
                new TestInfo(MetaTrueSameSite, 50000000),
                new TestInfo(MetaRebindNewSite, 3000),
                new TestInfo(MetaRebindNewSite8, 3000),
                new TestInfo(MetaRebindNewSite16, 3000),
                new TestInfo(MetaNullNewSite, 1000000),
                new TestInfo(MetaTrueNewSite, 1000000),
                new TestInfo(MetaBigCalls, 2), 

                new TestInfo(CallSiteStaticCreation, 8),
                new TestInfo(CallSiteCreation, 100000000),
                new TestInfo(CallSiteCreationAndHold, 10000000),
                new TestInfo(CallSiteCreationPrimitive, 40000000),
                new TestInfo(CallSiteCreationAndHoldPrimitive, 10000000),

                //new TestInfo(ParseAndTransformPython, 3),
                //new TestInfo(ParseAndTransformRuby, 3),
                new TestInfo(ParseSimpleRuby, 30000), 
                new TestInfo(ParseSimplePython, 30000),

                new TestInfo(Convert1, 300),
                new TestInfo(Convert2, 500),

                new TestInfo(Lambda1, 1000),
                new TestInfo(Lambda2, 20000),
                new TestInfo(Lambda3, 10000),
                new TestInfo(Lambda4, 80000),
                new TestInfo(Lambda5, 100000),
                new TestInfo(Lambda6, 100000),
                new TestInfo(Lambda7, 30000000),

                new TestInfo(UnaryPlus1, 500),

                new TestInfo(MethodCall1, 100000),
                new TestInfo(MethodCall2, 5000),
                new TestInfo(MethodCall3, 300),
                new TestInfo(MethodCall4, 100000),
                
                
                new TestInfo(FieldAccess1, 50000),
                new TestInfo(FieldAccess2, 15000),
                new TestInfo(FieldAccess3, 10000),
                new TestInfo(FieldAccess4, 40000),

                new TestInfo(PropertyAccess1, 40000),
                new TestInfo(PropertyAccess2, 15000),
                new TestInfo(PropertyAccess3, 3000),
                new TestInfo(PropertyAccess4, 20000),
                
                new TestInfo(Misc1, 50000),
                new TestInfo(Misc2, 200),
                new TestInfo(Misc3, 1500),
            };

            if (p_args.Length == 1 && p_args[0] == "RUN") {
                RunAll(testList);
            } else if (p_args.Length == 1 && p_args[0] == "FASTRUN") {
                // Smoke test: cut iterations by a factor of 100, so we can
                // verify functionality of the tests
                foreach (TestInfo t in testList) {
                    t.Iterations /= 100;
                    if (t.Iterations == 0) {
                        t.Iterations = 1;
                    }
                }

                RunAll(testList);
            } else {
                RunInLab(p_args, testList);
            }
        }

        private static void RunAll(TestInfo[] testList) {
            // run one dynamic site test initially to spin things up...
            ComTypeLibGetEnumType(1);

            Stopwatch sw = new Stopwatch();
            Stopwatch total = new Stopwatch();
            total.Start();
            foreach (var test in testList) {
                // then run w/ the proper number of iterations
                sw.Start();

                test.Action(test.Iterations);

                sw.Stop();

                Console.WriteLine("{0,-40} {1,-5}ms, {2,5} ms/iter", test.Action.Method.Name, sw.ElapsedMilliseconds, (double)sw.ElapsedMilliseconds / (double)test.Iterations);
                sw.Reset();
            }
            total.Stop();
            Console.WriteLine();
            Console.WriteLine("Total time: {0}ms", total.ElapsedMilliseconds);
        }

        private static void RunInLab(string[] p_args, TestInfo[] testList) {
            foreach (var test in testList) {
                m_util.AddTest(test.Action.Method.Name);
            }

            string[] args = m_util.ParseArgs(p_args);	// parses out extra args

            long iterations = m_util.GetIterations();	// gets -iters arg
            string testName = m_util.GetTestName();	    // gets –test arg

            if (iterations == 0) {
                Console.WriteLine("To run all tests pass RUN as the only argument.");
                Console.WriteLine("To perform a quick functionality test, pass FASTRUN.");

                m_util.DumpTestNames();
                return;
            }

            foreach (var test in testList) {
                if (test.Action.Method.Name == testName) {
                    // run any prep code in the test once...
                    test.Action(0);

                    // then run w/ the proper number of iterations
                    m_util.StartTimer(testName);

                    test.Action(iterations);

                    m_util.StopTimer();

                    m_util.DumpResult();

                    break;
                }
            }
        }

        #region COM Tests

        private static object adodbCommand = Activator.CreateInstance(System.Type.GetTypeFromProgID("ADODB.Command"));
        private static object adodbConnection = Activator.CreateInstance(System.Type.GetTypeFromProgID("ADODB.Connection"));
        private static ComTypeLibInfo adoTypeLibInfo = ComTypeLibDesc.CreateFromGuid(new System.Guid("{00000200-0000-0010-8000-00AA006D2EA4}"));

        public static void ComTypeLibGetEnumType(long iters) {
            var site1 = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("ADODB"));
            object typeLibDesc = site1.Target(site1, adoTypeLibInfo);

            var site = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("CommandTypeEnum"));
            for (long i = 0; i < iters; i++) {
                site.Target(site, typeLibDesc);
            }
        }

        public static void ComTypeLibGetClass(long iters) {
            var site1 = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("ADODB"));
            object typeLibDesc = site1.Target(site1, adoTypeLibInfo);

            var site = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("Recordset"));
            for (long i = 0; i < iters; i++) {
                site.Target(site, typeLibDesc);
            }
        }

        public static void ComEnumDescGetValue(long iters) {
            var site1 = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("ADODB"));
            object typeLibDesc = site1.Target(site1, adoTypeLibInfo);

            var site2 = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("CommandTypeEnum"));
            object enumType = site2.Target(site2, typeLibDesc);

            var site = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("adCmdFile"));
            for (long i = 0; i < iters; i++) {
                site.Target(site, enumType);
            }
        }

        public static void ComClassDescCreate(long iters) {
            var site1 = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("ADODB"));
            object typeLibDesc = site1.Target(site1, adoTypeLibInfo);

            var site2 = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("Parameter"));
            object cls = site2.Target(site2, typeLibDesc);

            var site = CallSite<Func<CallSite, object, object>>.Create(new MyCreateInstanceBinder());

            for (long i = 0; i < iters; i++) {
                site.Target(site, cls);
            }
        }

        public static void ComObjectSetItem(long iters) {
            var site1 = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("ADODB"));
            object typeLibDesc = site1.Target(site1, adoTypeLibInfo);

            var site2 = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("Connection"));
            object cls = site2.Target(site2, typeLibDesc);

            var site3 = CallSite<Func<CallSite, object, object>>.Create(new MyCreateInstanceBinder());
            object connection = site3.Target(site3, cls);

            var site4 = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("Properties"));
            object properties = site4.Target(site4, connection);

            var site = CallSite<Func<CallSite, object, string, int, object>>.Create(new MySetIndexBinder());

            for (long i = 0; i < iters; i++) {
                site.Target(site, properties, "Prompt", 1);
            }
        }


        public static void ComObjectGetItem(long iters) {
            var site1 = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("ADODB"));
            object typeLibDesc = site1.Target(site1, adoTypeLibInfo);

            var site2 = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("Connection"));
            object cls = site2.Target(site2, typeLibDesc);

            var site3 = CallSite<Func<CallSite, object, object>>.Create(new MyCreateInstanceBinder());
            object connection = site3.Target(site3, cls);

            var site4 = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("Properties"));
            object properties = site4.Target(site4, connection);

            var site = CallSite<Func<CallSite, object, string, object>>.Create(new MyGetIndexBinder());

            for (long i = 0; i < iters; i++) {
                site.Target(site, properties, "Prompt");
            }
        }


        public static void ComObjectGetPropertyInt(long iters) {
            var site = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("CommandTimeout"));

            for (long i = 0; i < iters; i++) {
                site.Target(site, adodbCommand);
            }
        }

        public static void ComObjectSetPropertyInt(long iters) {
            var site = CallSite<Func<CallSite, object, int, object>>.Create(new MySetMemberBinder("CommandTimeout"));

            for (long i = 0; i < iters; i++) {
                site.Target(site, adodbCommand, 40);
            }
        }

        public static void ComObjectGetPropertyString(long iters) {
            var gmsite = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("Name"));
            var setSite = CallSite<Func<CallSite, object, string, object>>.Create(new MySetMemberBinder("Name"));

            setSite.Target(setSite, adodbCommand, "Foo");

            for (long i = 0; i < iters; i++) {
                gmsite.Target(gmsite, adodbCommand);
            }
        }

        public static void ComObjectSetPropertyString(long iters) {
            var setSite = CallSite<Func<CallSite, object, string, object>>.Create(new MySetMemberBinder("Provider"));

            for (long i = 0; i < iters; i++) {
                setSite.Target(setSite, adodbConnection, "Foo");
            }
        }

        public static void ComObjectInvokeRetComObject(long iters) {
            var gmsite = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("CreateParameter"));
            var invokesite = CallSite<Func<CallSite, object, object>>.Create(new MyInvokeBinder());

            object createParamMethod = gmsite.Target(gmsite, adodbCommand);

            for (long i = 0; i < iters; i++) {
                invokesite.Target(invokesite, createParamMethod);
            }
        }

        public static void ComObjectInvokeRetVoid(long iters) {
            var gmsite = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("Cancel"));
            var invokesite = CallSite<Func<CallSite, object, object>>.Create(new MyInvokeBinder());

            object createParamMethod = gmsite.Target(gmsite, adodbCommand);

            for (long i = 0; i < iters; i++) {
                invokesite.Target(invokesite, createParamMethod);
            }
        }

        public static void ComTypeLibDescCreateFromGuid(long iters) {
            Guid g = new Guid("A7B93C73-7B81-11D0-AC5F-00C04FD97575");
            for (long i = 0; i < iters; i++) {
                agentServerTypeLib = ComTypeLibDesc.CreateFromGuid(g);
            }
        }

        public static void ComGetAgentServerObjects(long iters) {
            ComTypeLibDescCreateFromGuid(1); // make sure we have the type lib

            var site = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("AgentServerObjects"));

            for (long i = 0; i < iters; i++) {
                agentServerObjects = site.Target(site, agentServerTypeLib);
            }
        }

        public static void ComGetAgentServerObjectsNewSite(long iters) {
            ComTypeLibDescCreateFromGuid(1); // make sure we have the type lib

            for (long i = 0; i < iters; i++) {
                var site = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("AgentServerObjects"));
                agentServerObjects = site.Target(site, agentServerTypeLib);
            }
        }

        public static void ComGetAgentServer(long iters) {
            ComGetAgentServerObjects(1);    // make sure we have agentServerObjects

            var site = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("AgentServer"));

            for (long i = 0; i < iters; i++) {
                agentServer = site.Target(site, agentServerObjects);
            }
        }

        public static void ComGetAgentServerNewSite(long iters) {
            ComGetAgentServerObjects(1);    // make sure we have agentServerObjects

            for (long i = 0; i < iters; i++) {
                var site = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder("AgentServer"));
                agentServer = site.Target(site, agentServerObjects);
            }
        }

        public static void ComCreateAgentServer(long iters) {
            ComGetAgentServerNewSite(1);    // make sure we have agentServer

            var site = CallSite<Func<CallSite, object, object>>.Create(new MyCreateInstanceBinder());

            for (long i = 0; i < iters; i++) {
                site.Target(site, agentServer);
            }
        }

        public static void ComCreateAgentServerNewSite(long iters) {
            ComGetAgentServerNewSite(1);    // make sure we have agentServer

            for (long i = 0; i < iters; i++) {
                var site = CallSite<Func<CallSite, object, object>>.Create(new MyCreateInstanceBinder());
                site.Target(site, agentServer);
            }
        }

        #endregion

        #region MetaObject tests

        public static void MetaRebindSameSite(long iters) {
            var site = CallSite<Func<CallSite, object, object>>.Create(new ForceRebindBinder());

            for (long i = 0; i < iters; i++) {
                site.Target(site, null);
                RebindVersion++;
            }
        }

        public static void MetaRebindSameSite8(long iters) {
            var site = CallSite<Func<CallSite, object, object, object, object, object, object, object, object>>.Create(new ForceRebindBinder());

            for (long i = 0; i < iters; i++) {
                site.Target(site, null, null, null, null, null, null, null);
                RebindVersion++;
            }
        }

        public static void MetaRebindSameSite16(long iters) {
            var site = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>.Create(new ForceRebindBinder());

            for (long i = 0; i < iters; i++) {
                site.Target(site, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
                RebindVersion++;
            }
        }

        public static void MetaNullSameSite(long iters) {
            var site = CallSite<Func<CallSite, object, object>>.Create(new NullBinder());

            for (long i = 0; i < iters; i++) {
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
            }
        }

        public static void MetaTrueSameSite(long iters) {
            var site = CallSite<Func<CallSite, object, object>>.Create(new TrueBinder());

            for (long i = 0; i < iters; i++) {
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
                site.Target(site, null);
            }
        }

        public static void MetaRebindNewSite(long iters) {
            var binder = new ForceRebindBinder();

            for (long i = 0; i < iters; i++) {
                var site = CallSite<Func<CallSite, object, object>>.Create(binder);
                site.Target(site, null);
                RebindVersion++;
            }
        }

        public static void MetaRebindNewSite8(long iters) {
            var binder = new ForceRebindBinder();

            for (long i = 0; i < iters; i++) {
                var site = CallSite<Func<CallSite, object, object, object, object, object, object, object, object>>.Create(binder);
                site.Target(site, null, null, null, null, null, null, null);
                RebindVersion++;
            }
        }

        public static void MetaRebindNewSite16(long iters) {
            var binder = new ForceRebindBinder();

            for (long i = 0; i < iters; i++) {
                var site = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>.Create(binder);
                site.Target(site, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
                RebindVersion++;
            }
        }

        public static void MetaNullNewSite(long iters) {
            var binder = new NullBinder();

            for (long i = 0; i < iters; i++) {
                var site = CallSite<Func<CallSite, object, object>>.Create(binder);
                site.Target(site, null);
            }
        }

        public static void MetaTrueNewSite(long iters) {
            var binder = new TrueBinder();

            for (long i = 0; i < iters; i++) {
                var site = CallSite<Func<CallSite, object, object>>.Create(binder);
                site.Target(site, null);
            }
        }

        public static void MetaBigCalls(long iters) {
            Func<object[], int> sum = items => {
                int total = 0;
                foreach (object o in items) {
                    if (o != null) total++;
                }
                return total;
            };

            var args = new object[5002];
            var types = new Type[args.Length + 1];
            Type objType = typeof(object);
            for (int i = 2, n = types.Length - 1; i < n; i++ ) {
                types[i] = objType;
            }
            types[0] = typeof(CallSite);
            types[1] = typeof(object);
            types[types.Length - 1] = typeof(int);
            var delegateType = Expression.GetDelegateType(types);

            var binder = new ParamsDelegateBinder();
            for (long i = 0; i < iters; i++) {
                var site = CallSite.Create(delegateType, new ParamsDelegateBinder());
                var target = site.GetType().GetField("Target").GetValue(site) as Delegate;
                args[0] = site;
                args[1] = sum;
                var result = target.DynamicInvoke(args);
                if ((int)result != 0) {
                    throw new InvalidOperationException("test failed");
                }
            }
        }

        #endregion

        #region Call Site Creation Tests

        private static void CallSiteStaticCreation(long iters) {
            for (long i = 0; i < iters; i++) {
                switch (i) {
                    default:
                    case 0: MakeNext<object>(); break;
                    case 1: MakeNext<int>(); break;
                    case 2: MakeNext<bool>(); break;
                    case 3: MakeNext<string>(); break;
                    case 4: MakeNext<object[]>(); break;
                    case 5: MakeNext<Stopwatch>(); break;
                    case 6: MakeNext<List<object>>(); break;
                    case 7: MakeNext<Dictionary<List<object>, object>>(); break;
                    case 8: MakeNext<List<string>>(); break;
                    case 9: MakeNext<Program>(); break;
                    case 10: MakeNext<NullBinder>(); break;
                    case 11: MakeNext<TrueBinder>(); break;
                    case 12: MakeNext<MyGetMemberBinder>(); break;
                    case 13: MakeNext<MySetMemberBinder>(); break;
                    case 14: MakeNext<MySetIndexBinder>(); break;
                    case 15: MakeNext<MyInvokeBinder>(); break;
                    case 16: MakeNext<MyCreateInstanceBinder>(); break;
                    case 17: MakeNext<GetMemberBinder>(); break;
                    case 18: MakeNext<SetMemberBinder>(); break;
                    case 19: MakeNext<DeleteMemberBinder>(); break;
                    case 20: MakeNext<InvokeBinder>(); break;
                    case 21: MakeNext<InvokeMemberBinder>(); break;
                    case 22: MakeNext<CallSite>(); break;
                    case 23: MakeNext<BinaryExpression>(); break;
                    case 24: MakeNext<UnaryExpression>(); break;
                    case 25: MakeNext<MethodCallExpression>(); break;
                    case 26: MakeNext<string[]>(); break;
                    case 27: MakeNext<Activator>(); break;
                    case 28: MakeNext<DynamicExpression>(); break;
                    case 29: MakeNext<CallSiteBinder>(); break;
                    case 30: MakeNext<Action>(); break;
                    case 31: MakeNext<MyGetIndexBinder>(); break;
                }
            }
        }

        private static void MakeNext<T1>() {
            for (long i = 0; i < 8; i++) {
                switch (i) {
                    case 0: MakeNext<T1, object>(); break;
                    case 1: MakeNext<T1, int>(); break;
                    case 2: MakeNext<T1, bool>(); break;
                    case 3: MakeNext<T1, string>(); break;
                    case 4: MakeNext<T1, object[]>(); break;
                    case 5: MakeNext<T1, Stopwatch>(); break;
                    case 6: MakeNext<T1, List<object>>(); break;
                    case 7: MakeNext<T1, Dictionary<List<object>, object>>(); break;
                }
            }
        }

        private static void MakeNext<T1, T2>() {
            for (long i = 0; i < 8; i++) {
                switch (i) {
                    case 0: MakeNext<T1, T2, object>(); break;
                    case 1: MakeNext<T1, T2, int>(); break;
                    case 2: MakeNext<T1, T2, bool>(); break;
                    case 3: MakeNext<T1, T2, string>(); break;
                    case 4: MakeNext<T1, T2, object[]>(); break;
                    case 5: MakeNext<T1, T2, Stopwatch>(); break;
                    case 6: MakeNext<T1, T2, List<object>>(); break;
                    case 7: MakeNext<T1, T2, Dictionary<List<object>, object>>(); break;
                }
            }
        }

        private static void MakeNext<T1, T2, T3>() {
            for (long i = 0; i < 8; i++) {
                switch (i) {
                    case 0: MakeNext<T1, T2, T3, object>(); break;
                    case 1: MakeNext<T1, T2, T3, int>(); break;
                    case 2: MakeNext<T1, T2, T3, bool>(); break;
                    case 3: MakeNext<T1, T2, T3, string>(); break;
                    case 4: MakeNext<T1, T2, T3, object[]>(); break;
                    case 5: MakeNext<T1, T2, T3, Stopwatch>(); break;
                    case 6: MakeNext<T1, T2, T3, List<object>>(); break;
                    case 7: MakeNext<T1, T2, T3, Dictionary<List<object>, object>>(); break;
                }
            }
        }

        private static void MakeNext<T1, T2, T3, T4>() {
            CallSite<Func<CallSite, T1, T2, T3, T4>>.Create(null);
        }

        private static void CallSiteCreation(long iters) {
            for (long i = 0; i < iters; i++) {
                CallSite<Func<CallSite, object, object, object>>.Create(null);
            }
        }

        private static void CallSiteCreationAndHold(long iters) {
            List<CallSite> sites = new List<CallSite>();

            for (long i = 0; i < iters; i++) {
                sites.Add(CallSite<Func<CallSite, object, object, object>>.Create(null));
            }
        }

        private static void CallSiteCreationPrimitive(long iters) {
            for (long i = 0; i < iters; i++) {
                CallSite<Func<CallSite, int, int, int>>.Create(null);
            }
        }

        private static void CallSiteCreationAndHoldPrimitive(long iters) {
            List<CallSite> sites = new List<CallSite>();

            for (long i = 0; i < iters; i++) {
                sites.Add(CallSite<Func<CallSite, int, int, int>>.Create(null));
            }
        }

        #endregion

        #region Compilation Tests

        private static void ParseAndTransformPython(long iters) {
            ScriptEngine pyEngine = Python.CreateEngine();
            string dlr_root = Environment.GetEnvironmentVariable("DLR_ROOT");

            for (long i = 0; i < iters; i++) {
                pyEngine.CreateScriptSourceFromFile(Path.Combine(dlr_root, @"External.LCA_RESTRICTED\Languages\IronPython\27\Lib\decimal.py")).GetCodeProperties();
                pyEngine.CreateScriptSourceFromFile(Path.Combine(dlr_root, @"External.LCA_RESTRICTED\Languages\IronPython\27\Lib\doctest.py")).GetCodeProperties();
                pyEngine.CreateScriptSourceFromFile(Path.Combine(dlr_root, @"External.LCA_RESTRICTED\Languages\IronPython\27\Lib\pydoc.py")).GetCodeProperties();
                pyEngine.CreateScriptSourceFromFile(Path.Combine(dlr_root, @"External.LCA_RESTRICTED\Languages\IronPython\27\Lib\difflib.py")).GetCodeProperties();
                pyEngine.CreateScriptSourceFromFile(Path.Combine(dlr_root, @"External.LCA_RESTRICTED\Languages\IronPython\27\Lib\mailbox.py")).GetCodeProperties();
            }
        }

        private static void ParseAndTransformRuby(long iters) {
            ScriptEngine rbEngine = Ruby.GetEngine(Ruby.CreateRuntime());
            string dlr_root = Environment.GetEnvironmentVariable("DLR_ROOT");

            for (long i = 0; i < iters; i++) {
                rbEngine.CreateScriptSourceFromFile(Path.Combine(dlr_root, @"External.LCA_RESTRICTED\Languages\Ruby\ruby-1.9.0-0\lib\ruby\1.9.0\tk.rb")).GetCodeProperties();
                rbEngine.CreateScriptSourceFromFile(Path.Combine(dlr_root, @"External.LCA_RESTRICTED\Languages\Ruby\ruby-1.9.0-0\lib\ruby\1.9.0\multi-tk.rb")).GetCodeProperties();
                rbEngine.CreateScriptSourceFromFile(Path.Combine(dlr_root, @"External.LCA_RESTRICTED\Languages\Ruby\ruby-1.9.0-0\lib\ruby\1.9.0\cgi.rb")).GetCodeProperties();
                rbEngine.CreateScriptSourceFromFile(Path.Combine(dlr_root, @"External.LCA_RESTRICTED\Languages\Ruby\ruby-1.9.0-0\lib\ruby\1.9.0\csv.rb")).GetCodeProperties();
                rbEngine.CreateScriptSourceFromFile(Path.Combine(dlr_root, @"External.LCA_RESTRICTED\Languages\Ruby\ruby-1.9.0-0\lib\ruby\1.9.0\rake.rb")).GetCodeProperties();
            }
        }

        private static void ParseSimpleRuby(long iters) {
            ScriptEngine rbEngine = Ruby.GetEngine(Ruby.CreateRuntime());
            for (long i = 0; i < iters; i++) {
                rbEngine.CreateScriptSourceFromString("1").GetCodeProperties();
            }
        }

        private static void ParseSimplePython(long iters) {
            ScriptEngine pyEngine = Python.CreateEngine();

            for (long i = 0; i < iters; i++) {
                pyEngine.CreateScriptSourceFromString("1").GetCodeProperties();
            }
        }

        #endregion

        #region CodeGen Tests

        public static void CodeGenRuntimeVarsIntoType(long iterations) {
            var lambda = MakeSimpleClosure();

            var func = CompileIntoType(lambda);

            for (int i = 0; i < iterations; i++) {
                func();
            }
        }

        public static void CodeGenRuntimeVarsIntoDynamicMethod(long iterations) {
            var lambda = MakeSimpleClosure();

            var func = lambda.Compile();

            for (int i = 0; i < iterations; i++) {
                func();
            }
        }

        public static void CodeGenRuntimeVarsIntoType2(long iterations) {
            var lambda = MakeNestedClosure();

            var func = CompileIntoType(lambda);

            for (int i = 0; i < iterations; i++) {
                func();
            }
        }

        public static void CodeGenRuntimeVarsIntoDynamicMethod2(long iterations) {
            var lambda = MakeNestedClosure();

            var func = CompileLambda(lambda);

            for (int i = 0; i < iterations; i++) {
                func();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Func<IRuntimeVariables> CompileLambda(Expression<Func<IRuntimeVariables>> lambda) {
            return lambda.Compile();
        }

        private static Func<IRuntimeVariables> CompileIntoType(Expression<Func<IRuntimeVariables>> lambda) {
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("foo"), AssemblyBuilderAccess.Run);
            ModuleBuilder mb = ab.DefineDynamicModule("foo");
            TypeBuilder tb = mb.DefineType("myType");

            MethodBuilder method = tb.DefineMethod(lambda.Name ?? "lambda_method", MethodAttributes.Public | MethodAttributes.Static);
            lambda.CompileToMethod(method);

            Type t = tb.CreateType();

            var func = (Func<IRuntimeVariables>)Delegate.CreateDelegate(typeof(Func<IRuntimeVariables>), t.GetMethod(method.Name));
            return func;
        }

        private static Expression<Func<IRuntimeVariables>> MakeSimpleClosure() {
            ParameterExpression var1 = Expression.Variable(typeof(int), "foo");
            ParameterExpression var2 = Expression.Variable(typeof(int), "bar");
            ParameterExpression var3 = Expression.Variable(typeof(int), "baz");

            var lambda = Expression.Lambda<Func<IRuntimeVariables>>(
                Expression.Block(
                    new ParameterExpression[] {
                        var1,
                        var2,
                        var3
                    },
                    Expression.Assign(var1, AstUtils.Constant(42)),
                    Expression.Assign(var2, AstUtils.Constant(23)),
                    Expression.Assign(var3, AstUtils.Constant(5)),
                    Expression.RuntimeVariables(
                        var1, var2, var3
                    )
                )
            );
            return lambda;
        }

        private static Expression<Func<IRuntimeVariables>> MakeNestedClosure() {
            ParameterExpression var1 = Expression.Variable(typeof(int), "foo");
            ParameterExpression var2 = Expression.Variable(typeof(int), "bar");
            ParameterExpression var3 = Expression.Variable(typeof(int), "baz");
            ParameterExpression var4 = Expression.Variable(typeof(int), "foo2");
            ParameterExpression var5 = Expression.Variable(typeof(int), "bar2");
            ParameterExpression var6 = Expression.Variable(typeof(int), "baz2");

            var lambda = Expression.Lambda<Func<IRuntimeVariables>>(
                Expression.Block(
                    new ParameterExpression[] {
                        var1,
                        var2,
                        var3
                    },
                    Expression.Assign(var1, AstUtils.Constant(42)),
                    Expression.Assign(var2, AstUtils.Constant(23)),
                    Expression.Assign(var3, AstUtils.Constant(5)),

                    Expression.Invoke(
                        Expression.Lambda(
                            Expression.Block(
                                new ParameterExpression[] {
                                    var4,
                                    var5,
                                    var6
                                },
                                Expression.Assign(var4, AstUtils.Constant(42)),
                                Expression.Assign(var5, AstUtils.Constant(23)),
                                Expression.Assign(var6, AstUtils.Constant(5)),
                                Expression.RuntimeVariables(
                                    var1, var2, var3, var4, var5, var6
                                )
                            )
                        )
                    )
                )
            );
            return lambda;
        }

        #endregion

        #region Binders

        class MyGetMemberBinder : GetMemberBinder {
            public MyGetMemberBinder(string name)
                : base(name, false) {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
                DynamicMetaObject com;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindGetMember(this, target, out com, true)) {
                    return com;
                }
                return errorSuggestion ?? new DynamicMetaObject(
                    Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(object)),
                    target.Restrictions
                );
            }
        }

        class MySetMemberBinder : SetMemberBinder {
            public MySetMemberBinder(string name)
                : base(name, false) {
            }

            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
                DynamicMetaObject com;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindSetMember(this, target, value, out com)) {
                    return com;
                }

                return errorSuggestion ?? new DynamicMetaObject(
                    Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(object)),
                    target.Restrictions.Merge(value.Restrictions)
                );
            }
        }

        class MyCreateInstanceBinder : CreateInstanceBinder {
            public MyCreateInstanceBinder()
                : base(new CallInfo(0)) {
            }

            public override DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
                return errorSuggestion ?? new DynamicMetaObject(
                    Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(object)),
                    target.Restrictions.Merge(BindingRestrictions.Combine(args))
                );
            }
        }

        class MyInvokeBinder : InvokeBinder {
            public MyInvokeBinder()
                : base(new CallInfo(0)) {
            }

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
                return errorSuggestion ?? new DynamicMetaObject(
                    Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(object)),
                    target.Restrictions.Merge(BindingRestrictions.Combine(args))
                );
            }
        }

        class MySetIndexBinder : SetIndexBinder {
            public MySetIndexBinder()
                : base(new CallInfo(0)) {
            }

            public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
                DynamicMetaObject com;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindSetIndex(this, target, indexes, value, out com)) {
                    return com;
                }

                return errorSuggestion ?? new DynamicMetaObject(
                    Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(object)),
                    target.Restrictions.Merge(BindingRestrictions.Combine(indexes)).Merge(value.Restrictions)
                );
            }
        }

        class MyGetIndexBinder : GetIndexBinder {
            public MyGetIndexBinder()
                : base(new CallInfo(0)) {
            }

            public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion) {
                DynamicMetaObject com;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindGetIndex(this, target, indexes, out com)) {
                    return com;
                }

                return errorSuggestion ?? new DynamicMetaObject(
                    Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(object)),
                    target.Restrictions.Merge(BindingRestrictions.Combine(indexes))
                );
            }
        }

        /// <summary>
        /// Forces a rebind on every single access so that we can measure the rebinding
        /// infatructure.
        /// </summary>
        class ForceRebindBinder : DynamicMetaObjectBinder {
            public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
                return new DynamicMetaObject(
                    Expression.Condition(
                        Expression.Equal(
                            Expression.Field(
                                null,
                                typeof(Program).GetField("RebindVersion")
                            ),
                            AstUtils.Constant(RebindVersion)
                        ),
                        AstUtils.Constant(null),
                        GetUpdateExpression(typeof(object))
                    ),
                    BindingRestrictions.Empty
                );
            }
        }

        class NullBinder : DynamicMetaObjectBinder {
            public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
                return new DynamicMetaObject(
                    AstUtils.Constant(null),
                    BindingRestrictions.Empty
                );
            }
        }

        class TrueBinder : DynamicMetaObjectBinder {
            public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
                return new DynamicMetaObject(
                    AstUtils.Constant(null),
                    BindingRestrictions.GetExpressionRestriction(AstUtils.Constant(true))
                );
            }
        }

        class ParamsDelegateBinder : DynamicMetaObjectBinder {
            public override Type ReturnType {
                get { return typeof(int); }
            }
            public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
                var restrictions = BindingRestrictions.GetTypeRestriction(target.Expression, target.RuntimeType);
                var inits = new Expression[args.Length];
                
                for (int i = 0; i < args.Length; i++) {
                    var o = args[i];
                    var r = o.Value == null ?
                        BindingRestrictions.GetInstanceRestriction(o.Expression, null) :
                        BindingRestrictions.GetTypeRestriction(o.Expression, o.RuntimeType);

                    restrictions = restrictions.Merge(r);
                    inits[i] = o.Expression;
                }

                return new DynamicMetaObject(
                    Expression.Invoke(
                        Expression.Convert(target.Expression, target.RuntimeType),
                        Expression.NewArrayInit(inits[0].Type, inits)
                    ),
                    restrictions
                );
            }
        }

        #endregion

        #region ET scenarios

        #region Conversions

        public static class Delegates {
            public static Action Del1 = Expression.Lambda<Action>(
                    RepeatExpression(1000, Expression.Condition(
                        Expression.Convert(
                            Expression.GreaterThan(
                                Expression.Add(
                                    Expression.Convert(
                                        Expression.Convert(
                                            Expression.Convert(
                                                Expression.Convert(
                                                    Expression.Convert(
                                                        Expression.Convert(
                                                            Expression.Convert(
                                                                Expression.Convert(
                                                                    Expression.Convert(
                                                                        Expression.Convert(
                                                                            Expression.Convert(
                                                                                Expression.Convert(
                                                                                    Expression.Convert(
                                                                                        Expression.Convert(
                                                                                            Expression.Convert(
                                                                                                Expression.Convert(
                                                                                                    Expression.Convert(
                                                                                                        Expression.Convert(
                                                                                                            Expression.Convert(
                                                                                                                Expression.Convert(
                                                                                                                    Expression.Convert(
                                                                                                                        Expression.Convert(
                                                                                                                            Expression.Convert(
                                                                                                                                Expression.Convert(
                                                                                                                                    AstUtils.Constant(5),
                                                                                                                                    typeof(object)
                                                                                                                                ),
                                                                                                                                typeof(int)
                                                                                                                            ),
                                                                                                                            typeof(object)
                                                                                                                        ),
                                                                                                                        typeof(int)
                                                                                                                    ),
                                                                                                                    typeof(object)
                                                                                                                ),
                                                                                                                typeof(int)
                                                                                                            ),
                                                                                                            typeof(object)
                                                                                                        ),
                                                                                                        typeof(int)
                                                                                                    ),
                                                                                                    typeof(object)
                                                                                                ),
                                                                                                typeof(int)
                                                                                            ),
                                                                                            typeof(object)
                                                                                        ),
                                                                                        typeof(int)
                                                                                    ),
                                                                                    typeof(object)
                                                                                ),
                                                                                typeof(int)
                                                                            ),
                                                                            typeof(object)
                                                                        ),
                                                                        typeof(int)
                                                                    ),
                                                                    typeof(object)
                                                                ),
                                                                typeof(int)
                                                            ),
                                                            typeof(object)
                                                        ),
                                                        typeof(int)
                                                    ),
                                                    typeof(object)
                                                ),
                                                typeof(int)
                                            ),
                                            typeof(object)
                                        ),
                                        typeof(int)
                                    ),
                                    Expression.Convert(
                                        Expression.Convert(
                                            AstUtils.Constant(-10),
                                            typeof(object)
                                        ),
                                        typeof(int)
                                    )
                                ),
                                AstUtils.Constant(0)
                            ),
                            typeof(bool)
                        ),
                        Expression.Convert(AstUtils.Constant("yes"), typeof(object)),
                        Expression.Convert(AstUtils.Constant("no"), typeof(object))
                    ), 100)
                ).Compile();
        }

        //boxing/unboxing
        public static void Convert1(long iters) {
            for (long i = 0; i < iters; i++) {
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
                Delegates.Del1();
            }
        }

        private static Action Convert2Var1 = (Action)Expression.Lambda(
            RepeatExpression(100000000, Expression.Convert(
                Expression.Convert(
                    Expression.Convert(
                        Expression.Convert(
                            Expression.Convert(
                                Expression.Convert(
                                    Expression.Convert(
                                        Expression.Convert(
                                            Expression.Convert(
                                                Expression.Convert(
                                                    Expression.Convert(
                                                        Expression.Convert(
                                                            AstUtils.Constant(1),
                                                            typeof(int)
                                                        ),
                                                        typeof(int)
                                                    ),
                                                    typeof(int)
                                                ),
                                                typeof(int)
                                            ),
                                            typeof(int)
                                        ),
                                        typeof(int)
                                    ),
                                    typeof(int)
                                ),
                                typeof(int)
                            ),
                            typeof(int)
                        ),
                        typeof(int)
                    ),
                    typeof(int)
                ),
                typeof(int)
            ), 100)
        ).Compile();
        //same type conversions
        public static void Convert2(long iters) {
            for (long i = 0; i < iters; i++) {
                Convert2Var1.Invoke();
            }
        }

        #endregion

        #region lambdas
        //Test empty lambda compilation
        private static LambdaExpression Lambda1Var1 = Expression.Lambda(AstUtils.Constant(null));
        public static void Lambda1(long iters) {
            for (long i = 0; i < iters; i++) {
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
                Lambda1Var1.Compile();
            }
        }


        //Lambda invocation
        private static Action Lambda2Var1 = (Action)Expression.Lambda<Action>(AstUtils.Constant(null)).Compile();
        public static void Lambda2(long iters) {
            for (long i = 0; i < iters; i++) {
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
                Lambda2Var1.Invoke();
            }
        }

        //Lambda Factories
        public static void Lambda3(long iters) {
            Expression Empty = AstUtils.Constant(null);
            for (long i = 0; i < iters; i++) {
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
                Expression.Lambda(Empty);
            }
        }

        //lambda factories
        public static void Lambda4(long iters) {
            Expression Empty = AstUtils.Constant(null);
            ParameterExpression Param1 = Expression.Parameter(typeof(int), "");
            ParameterExpression Param2 = Expression.Parameter(typeof(int), "");
            for (long i = 0; i < iters; i++) {
                Expression.Lambda(Empty, Param1, Param2);
            }
        }

        //lambda factories
        public static void Lambda5(long iters) {
            Expression Empty = AstUtils.Constant(null);
            ParameterExpression Param1 = Expression.Parameter(typeof(int), "");
            ParameterExpression Param2 = Expression.Parameter(typeof(int), "");
            for (long i = 0; i < iters; i++) {
                Expression.Lambda(typeof(Action<int, int>), Empty, new ParameterExpression[] { Param1, Param2 });
                Expression.Lambda(typeof(Action<int, int>), Empty, new ParameterExpression[] { Param1, Param2 });
            }
        }

        //lambda factories
        public static void Lambda6(long iters) {
            Expression Empty = AstUtils.Constant(null);
            ParameterExpression Param1 = Expression.Parameter(typeof(int), "");
            ParameterExpression Param2 = Expression.Parameter(typeof(int), "");
            for (long i = 0; i < iters; i++) {
                Expression.Lambda<Action<int, int>>(Empty, Param1, Param2);
                Expression.Lambda<Action<int, int>>(Empty, Param1, Param2);
            }
        }

        public static ParameterExpression Lambda7Parm1 = Expression.Parameter(typeof(int), "");
        public static ParameterExpression Lambda7Parm2 = Expression.Parameter(typeof(int), "");
        public static LambdaExpression Lambda7Var1 = Expression.Lambda<Func<int>>(
            Expression.Invoke(
                Expression.Lambda(AstUtils.Constant(1), Lambda7Parm1, Lambda7Parm2),
                AstUtils.Constant(1),
                AstUtils.Constant(2)
            )
        );
        public static Func<int> Lambda7Var2 = (Func<int>)Lambda7Var1.Compile();

        public static void Lambda7(long iters) {
            for (long i = 0; i < iters; i++) {
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
                Lambda7Var2.Invoke();
            }
        }


        #endregion

        #region Operators

        private static Action UnaryPlus1Var1 = (Action)Expression.Lambda(
            RepeatExpression(100000000, Expression.UnaryPlus(
                Expression.UnaryPlus(
                    Expression.UnaryPlus(
                        Expression.UnaryPlus(
                            Expression.UnaryPlus(
                                Expression.UnaryPlus(
                                    Expression.UnaryPlus(
                                        Expression.UnaryPlus(
                                            Expression.UnaryPlus(
                                                Expression.UnaryPlus(
                                                    Expression.UnaryPlus(
                                                        Expression.UnaryPlus(
                                                            Expression.UnaryPlus(
                                                                Expression.UnaryPlus(
                                                                    Expression.UnaryPlus(
                                                                        Expression.UnaryPlus(
                                                                            Expression.UnaryPlus(
                                                                                Expression.UnaryPlus(
                                                                                    Expression.UnaryPlus(
                                                                                        Expression.UnaryPlus(
                                                                                            Expression.UnaryPlus(
                                                                                                Expression.UnaryPlus(
                                                                                                    Expression.UnaryPlus(
                                                                                                        Expression.UnaryPlus(
                                                                                                            AstUtils.Constant(5)
                                                                                                        )
                                                                                                    )
                                                                                                )
                                                                                            )
                                                                                        )
                                                                                    )
                                                                                )
                                                                            )
                                                                        )
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            ), 100)
        ).Compile();
        //Unary plus repeatedly
        public static void UnaryPlus1(long iters) {
            for (long i = 0; i < iters; i++) {
                UnaryPlus1Var1.Invoke();
            }
        }
        #endregion

        #region CLR access
        //method call
        public static MethodInfo MethodCall1Var1 = typeof(int).GetMethod("GetType");
        //Factory 1
        public static void MethodCall1(long iters) {
            Expression Const1 = AstUtils.Constant(1);
            for (long i = 0; i < iters; i++) {
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
                Expression.Call(Const1, MethodCall1Var1);
            }
        }

        //Factory 2
        public static void MethodCall2(long iters) {
            Expression Const1 = AstUtils.Constant(1);
            Type[] TypeArgs = new Type[] { };
            for (long i = 0; i < iters; i++) {
                Expression.Call(Const1, "GetType", TypeArgs);
                Expression.Call(Const1, "GetType", TypeArgs);
                Expression.Call(Const1, "GetType", TypeArgs);
                Expression.Call(Const1, "GetType", TypeArgs);
                Expression.Call(Const1, "GetType", TypeArgs);
                Expression.Call(Const1, "GetType", TypeArgs);
                Expression.Call(Const1, "GetType", TypeArgs);
                Expression.Call(Const1, "GetType", TypeArgs);
                Expression.Call(Const1, "GetType", TypeArgs);
                Expression.Call(Const1, "GetType", TypeArgs);
            }
        }

        //Compilation
        public static void MethodCall3(long iters) {
            Expression Const1 = AstUtils.Constant(1);
            LambdaExpression expr = Expression.Lambda(Expression.Call(Const1, "GetType", new Type[] { }));
            for (long i = 0; i < iters; i++) {
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
                expr.Compile();
            }
        }

        //Execution
        public static Action MethodCall4Var1 = Expression.Lambda<Action>(Expression.Call(AstUtils.Constant(1), "GetType", new Type[] { })).Compile();
        public static void MethodCall4(long iters) {
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 300; j++) {
                    MethodCall4Var1.Invoke();
                }
            }
        }

        //field access
        //factories
        public static void FieldAccess1(long iters) {
            FieldInfo Field = typeof(int).GetField("MaxValue");
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 500; j++) {
                    Expression.Field(null, Field);
                }
            }
        }

        public class c_FA2 {
            public int x;
        }
        public static Expression FieldAccess2Var1 = AstUtils.Constant(new c_FA2());
        public static void FieldAccess2(long iters) {
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 100; j++) {
                    Expression.Field(FieldAccess2Var1, "x");
                }
            }
        }

        //compilation
        public static LambdaExpression FieldAccess3Var1 = Expression.Lambda<Action>(
            Expression.Field(null, typeof(int).GetField("MaxValue"))
        );
        public static void FieldAccess3(long iters) {
            for (long i = 0; i < iters; i++) {
                FieldAccess3Var1.Compile();
            }
        }

        //execution
        public static Action FieldAccess4Var1 = (Action)FieldAccess3Var1.Compile();
        public static void FieldAccess4(long iters) {
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 10000; j++) {
                    FieldAccess4Var1.Invoke();
                }
            }
        }

        //property access
        //factories
        public static void PropertyAccess1(long iters) {
            PropertyInfo Property = typeof(DateTime).GetProperty("Now");
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 500; j++) {
                    Expression.Property(null, Property);
                }
            }
        }

        public class c_PA2 {
            public int x {
                get {
                    return 1;
                }
                set {
                }
            }
        }
        public static Expression PropertyAccess2Var1 = AstUtils.Constant(new c_PA2());
        public static void PropertyAccess2(long iters) {
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 100; j++) {
                    Expression.Property(PropertyAccess2Var1, "x");
                }
            }
        }

        //compilation
        public static LambdaExpression PropertyAccess3Var1 = Expression.Lambda<Action>(
            Expression.Property(AstUtils.Constant(new c_PA2()), typeof(c_PA2).GetProperty("x"))
        );
        public static void PropertyAccess3(long iters) {
            for (long i = 0; i < iters; i++) {
                PropertyAccess3Var1.Compile();
            }
        }

        //execution
        public static Action PropertyAccess4Var1 = (Action)PropertyAccess3Var1.Compile();
        public static void PropertyAccess4(long iters) {
            for (long i = 0; i < iters; i++) {
                for (int j = 0; j < 10000; j++) {
                    PropertyAccess4Var1.Invoke();
                }
            }
        }
        #endregion

        #region Misc
        //Construction of sample tree containing basic operations.
        public static void Misc1(long iters) {
            for (long i = 0; i < iters; i++) {
                Expression.Lambda(
                    Expression.Condition(
                        Expression.NotEqual(
                            AstUtils.Constant(1.1),
                            AstUtils.Constant(1.2)
                        ),
                        AstUtils.Constant(5),
                        AstUtils.Constant(6)
                    ),
                    Expression.Parameter(typeof(int), ""),
                    Expression.Parameter(typeof(int), "")
                );
            }
        }

        private static ParameterExpression Misc2Param1 = Expression.Parameter(typeof(int), "");
        private static ParameterExpression Misc2Param2 = Expression.Parameter(typeof(int), "");
        private static LambdaExpression Misc2Var1 = Expression.Lambda(
                    RepeatExpression(1000000, Expression.Condition(
                        Expression.NotEqual(
                            Misc2Param1,
                            Misc2Param2
                        ),
                        AstUtils.Constant(5),
                        AstUtils.Constant(6)
                    ), 100),
                    Misc2Param1,
                    Misc2Param2
                );
        //Compilation of sample tree
        public static void Misc2(long iters) {
            for (long i = 0; i < iters; i++) {
                Misc2Var1.Compile();
            }
        }

        private static Action<int, int> Misc3Var1 = (Action<int, int>)Misc2Var1.Compile();
        //Execution of sample tree
        public static void Misc3(long iters) {
            for (long i = 0; i < iters; i++) {
                Misc3Var1.Invoke(1, 2);
            }
        }


        #endregion

        #region ETUtils

        /// <summary>
        /// This method wraps an expression on a loop.
        /// </summary>
        /// <param name="iters"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public static Expression RepeatExpression(long iters, Expression Body) {
            return RepeatExpression(iters, Body, 1);
        }

        /// <summary>
        /// This method wraps an expression on a loop.
        /// It attempts to duplicate the expression multiple times inside the loop to minimize the actual 
        /// loop time costs. It can therefore affect compilation time.
        /// This can also break code which contains labels or any other expression that should be unique.
        /// </summary>
        /// <param name="iters"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public static Expression RepeatExpression(long iters, Expression Body, long DuplicationConstant) {
            long MainIterations;
            long LeftOvers;

            MainIterations = iters / DuplicationConstant;
            LeftOvers = iters - MainIterations * DuplicationConstant;

            ParameterExpression Counter = Expression.Parameter(typeof(long), "Counter");
            var exprs = new List<Expression>();

            var Test1 = Expression.LessThan(Counter, AstUtils.Constant(MainIterations));
            LabelTarget Exit1 = Expression.Label();
            BlockExpression Body1;

            var exprs1 = new List<Expression>();

            exprs1.Add(Expression.Condition(Test1, AstUtils.Empty(), Expression.Break(Exit1)));
            exprs1.Add(Expression.AddAssign(Counter, AstUtils.Constant((long)1)));
            for (int y = 0; y < DuplicationConstant; y++) {
                exprs1.Add(Body);
            }



            Body1 = Expression.Block(exprs1);

            exprs.Add(Expression.Assign(Counter, AstUtils.Constant((long)0)));
            exprs.Add(Expression.Loop(Body1, Exit1));

            var Test2 = Expression.LessThan(Counter, AstUtils.Constant(LeftOvers));
            LabelTarget Exit2 = Expression.Label();
            BlockExpression Body2;

            var exprs2 = new List<Expression>();

            exprs2.Add(Expression.Condition(Test2, AstUtils.Empty(), Expression.Break(Exit2)));
            exprs2.Add(Expression.AddAssign(Counter, AstUtils.Constant((long)1)));
            exprs2.Add(Body);



            Body2 = Expression.Block(exprs2);

            exprs.Add(Expression.Assign(Counter, AstUtils.Constant((long)0)));
            exprs.Add(Expression.Loop(Body2, Exit2));

            return Expression.Block(new[] { Counter }, exprs);
        }
        #endregion

        #endregion
    }
}
