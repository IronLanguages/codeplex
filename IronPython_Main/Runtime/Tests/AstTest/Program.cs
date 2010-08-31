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
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using ETUtils;
using Utils = AstTest.Utils;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {

    //
    // Quick note on the meaning of "Positive" "Negative" and "Slow" test
    // prefixes:
    //
    //   Positive: this test throws no exceptions while running. We want these
    //     to run first
    //
    //   Negative: this test intentionally throws an exception while running.
    //     These are run after positive tests so we don't see intentional
    //     exceptions when debugging positive test failures.
    //
    //   Slow: this test takes a while to run because it compiles lots of
    //     trees. For this reason, we want it to run last.
    //
    //   Disabled: the test is disabled. It can still be run manually
    //

    public static partial class Scenarios {
        public static bool FullTrust;


#if SILVERLIGHT
        public class ArgumentData {
#else
        public class ArgumentData : MarshalByRefObject {
#endif
            public Boolean CompileToMethod = false;
            public Boolean VerifyAssembly = false;
            public Boolean FullTrust = false;
            public Boolean NotExact = false;
            public Boolean Help = false;
            public List<string> Rewrite = new List<string>();
            public List<string> Testcases = new List<string>();

            public void Initialize(string[] args) {
                foreach (string s in args) {
                    String temp;
#if SILVERLIGHT3
                    temp = s.ToLower();
#else
                    temp = s.ToLowerInvariant();
#endif
                    if (temp.StartsWith("/") || temp.StartsWith("-")) {
                        //switch

                        //clear / or -
                        temp = temp.Substring(1);

                        if (temp.Equals("ne") || temp.Equals("notexact")) {
                            NotExact = true;
                        }

                        if (temp.Equals("ctm") || temp.Equals("compiletomethod")) {
                            CompileToMethod = true;
                        }

                        if (temp.Equals("va") || temp.Equals("verifyassembly")) {
                            CompileToMethod = true;
                            VerifyAssembly = true;
                            FullTrust = true;
                        }

                        if (temp.Equals("f") || temp.Equals("fulltrust")) {
                            FullTrust = true;
                        }

                        if (temp.Equals("?") || temp.Equals("help")) {
                            Help = true;
                        }

                        if (temp.StartsWith("r") || temp.StartsWith("rewrite")) {
                            //get rewriters
                            Rewrite.AddRange(Utils.GetArgumentValues(temp));
                        }

                    } else {
                        //testcase
                        Testcases.Add(temp);
                    }
                }
            }
        }

#if SILVERLIGHT
        public sealed class TestRunner {
#else
        public sealed class TestRunner : MarshalByRefObject {
#endif
            private string[] _arguments;
            public string[] Arguments {
                get {
                    return _arguments;
                }
                set {
                    _arguments = value;
                    Options.Initialize(value);
                }
            }
            public readonly ArgumentData Options = new ArgumentData();
            public int ExitCode { get; private set; }
            public int TestsRan { get; private set; }
            public TrustKind TrustMode { get; set; }
            public readonly List<Assembly> PreLoadedAssemblies = new List<Assembly>();

            public enum TrustKind {
                FullTrust,
                FullTrustOnly,
                PartialTrust,
                PartialTrustOnly
            }

            private Exception RunScenario(MethodInfo test, String Rewriter, bool CompileToMethod) {
                var attr = ETUtils.TestAttribute.GetAttribute(test);

                Exception error = null;
                try {
                    Expression body = null;
                    if (test.GetParameters().Length == 0) { // legacy LINQ tests, NoPia tests
                        test.Invoke(null, new object[] {});
                    } else {
#if SILVERLIGHT3
                        throw new Exception("Should not be here"); //@RYANOOB
#else
                        var V = new EU.TestValidator(Rewriter, Options.CompileToMethod, Options.VerifyAssembly);
                        body = (Expression)test.Invoke(null, new object[] { V }); // Validator will compile and invoke resulting tree
#endif
                    }

                    // check ToString and DebugView don't blow up
                    try {
                        if (body != null) {
                            ValidateExpression(body);
                        }
                    } catch (TargetInvocationException e) {
                        return e.InnerException;
                    }

                    return null;
                } catch (Exception e) {
                    if (e is TargetInvocationException) {
                        error = e.InnerException;
                    } else {
                        error = e;
                    }
                    // Rewriters may cause invalid trees, we'll ignore those failures so hopefully rewriters only fail with legitimate bugs
                    // A test that legitimately fails due to a NotSupportedException will throw a TestFailedException wrapping the NotSupportedException.
                    if (error is NotSupportedException && Rewriter != "") {
                        error = null;
                    }
                }

                return error;
            }

            private static Exception ValidateExpression(Expression expr) {
                //verifies that Expression.ToString() and DebugView do not blow up.
                try {
                    expr.ToString();
                    string s = expr.DebugView();
                } catch (Exception e) {
                    return e;
                }
                return null;
            }
            
            private sealed class TestComparer : IComparer<string> {
                private readonly string[] _prefixes;

                internal TestComparer(string[] prefixes) {
                    _prefixes = prefixes;
                }

                public int Compare(string x, string y) {
                    int px = MatchPrefix(x, _prefixes);
                    int py = MatchPrefix(y, _prefixes);
                    if (px != py) {
                        return px - py;
                    }
                    return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
                }
            }

            private static List<Type> GetTestTypes(List<Assembly> Assemblies)
            {
                var Types = new List<Type>();
#if !SILVERLIGHT3
                Types.AddRange(typeof(ETScenarios.Program).Assembly.GetTypes());
#endif
                Types.AddRange(typeof(ExpressionCompiler.Scenario0.Test).Assembly.GetTypes());

                foreach(Assembly a in Assemblies){
                    Types.AddRange(a.GetTypes());
                }
                return Types;
            }

            const int MaxName = 64;

            public void RunTests() {
                bool RunAll = !Utils.ArgumentsHaveTestcaseNames(Arguments);

                bool NotExact = Options.NotExact;
                bool CompileToMethod = Options.CompileToMethod;

                var Rewriters = new List<String>();
                Rewriters.AddRange(Options.Rewrite);

                if (Rewriters.Count == 0) Rewriters.Add(""); //no rewriter case.

#if SILVERLIGHT
                var tests = new Dictionary<string, MethodInfo>();
#else
                var tests = new SortedList<string, MethodInfo>(new TestComparer(Arguments));
#endif

                foreach (MethodInfo m in typeof(Scenarios).GetMethods(BindingFlags.Public | BindingFlags.Static)) {
                    string name = GetTestName(m);
                    if (((RunAll || Utils.InsensitiveStringInArray(name, Arguments) || NotExact) && MatchPrefix(name, Arguments) >= 0 && MatchPrefix(name, new[] { "Positive", "Negative" }) >= 0) || (RunAll && MatchPrefix(name, new[] { "Positive", "Negative" }) >= 0)) {
                        tests.Add(name, m);
                    }
                }

                var warnTypes = new List<Type>();
                foreach (Type t in GetTestTypes(PreLoadedAssemblies)) {
                    if (!t.IsPublic) {
                        foreach (MethodInfo m in t.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
                            if (ETUtils.TestAttribute.GetAttribute(m) != null) {
                                warnTypes.Add(t);
                                break;
                            }
                        }
                        continue;
                    }
                    foreach (MethodInfo m in t.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
                        var attr = ETUtils.TestAttribute.GetAttribute(m);
                        if (attr != null && attr.State == ETUtils.TestState.Enabled) {
                            string name = GetTestName(m);
                            if (RunAll || Utils.InsensitiveStringInArray(name, Arguments) || (NotExact && MatchPrefix(name, Arguments) >= 0)) {
                                tests.Add(name, m);
                            }
                        }
                    }
                }

                var errors = new List<KeyValuePair<string,Exception>>();
                int TestCount = 0;

                foreach (var pair in tests) {

                    var Attr = ETUtils.TestAttribute.GetAttribute(pair.Value);
                    if (Attr != null && Array.IndexOf(Attr.KeyWords, "PartialTrustOnly") >= 0 && (TrustMode == TestRunner.TrustKind.FullTrust || TrustMode == TestRunner.TrustKind.FullTrustOnly ))
                        continue;

                    if (Attr != null && Array.IndexOf(Attr.KeyWords, "FullTrustOnly") >= 0 && (TrustMode == TestRunner.TrustKind.PartialTrust || TrustMode == TestRunner.TrustKind.PartialTrustOnly))
                        continue;

                    // If mode is FullTrustOnly or PartialTrustOnly, we only run the tests that have that key
                    if (TrustMode == TestRunner.TrustKind.FullTrustOnly && ! (Attr != null && Array.IndexOf(Attr.KeyWords, "FullTrustOnly") >= 0))
                        continue;

                    if (TrustMode == TestRunner.TrustKind.PartialTrustOnly && !(Attr != null && Array.IndexOf(Attr.KeyWords, "PartialTrustOnly") >= 0))
                        continue;
                    
                    string name = pair.Key;
                    MethodInfo test = pair.Value;

                    Exception error;
                    foreach (string compl in Rewriters) {
                        String name1 = name + (compl != "" ? "+" + compl : "");
                        name1 = FormatName(name1);

                        Console.Write("Testing " + name1);

                        error = RunScenario(test, compl, CompileToMethod);
                        TestCount++;

                        LogPassFail(name1, error, errors);
                    }
                }

                if (Options.VerifyAssembly)
                {
                    try
                    {
                        Utils.VerifyAssembly();
                    }
                    catch (Exception ex)
                    {
                        LogPassFail("Peverify:", ex, errors);
                    }
                }

                foreach (var error in errors) {
                    Console.WriteLine();
                    Console.WriteLine("test " + error.Key + " threw:");
                    Console.WriteLine(error.Value);
                }

                foreach (var type in warnTypes) {
                    Console.WriteLine();
                    Console.WriteLine("ERROR: class {0} not public, no tests ran", type);
                }

                TestsRan = TestCount;
                ExitCode = errors.Count + warnTypes.Count;
            }

            private static string FormatName(string name) {
                if (name.Length > MaxName) {
                    name = name.Substring(0, MaxName - 3) + "...";
                }
                return name;
            }

            private static void LogPassFail(String name, Exception error, List<KeyValuePair<string, Exception>> errors) {
                Console.Write(new String(' ', 1 + MaxName - name.Length));
                if (error == null) {
                    Console.WriteLine("PASS");
                } else {
                    Console.WriteLine("FAIL");
                    errors.Add(new KeyValuePair<string, Exception>(name, error));
                }
            }

            private static int MatchPrefix(string name, string[] prefix) {
                for (int i = 0, n = prefix.Length; i < n; i++) {
                    if (name.StartsWith(prefix[i], StringComparison.InvariantCultureIgnoreCase)) {
                        return i;
                    }
                }
                return -1;
            }

            private static string GetTestName(MethodInfo test) {
                string name = test.Name;
                if (test.DeclaringType != typeof(Scenarios)) {
                    if (ETUtils.TestAttribute.IsTest(test)){
                        return ETUtils.TestAttribute.GetAttribute(test).Description;
                    }
                    if (name.StartsWith(test.DeclaringType.Name)) {
                        name = name.Substring(test.DeclaringType.Name.Length);
                    }
                    name = test.DeclaringType.FullName + "." + name;
                }
                return name;
            }

        }

        static void PrintArguments() {
            //Arguments: [/NE|/NotExact] [/F|/FullTrust] [/Rewrite:TestRewritter1,...] TestName TestName
            Console.WriteLine("Executes Expression Tree tests.");
            Console.WriteLine("Arguments:");
            Console.WriteLine("\t[/NE|/NotExact] [/F|/FullTrust] [/Rewrite:TestRewritter1,...] TestName TestName");
            Console.WriteLine("\t/NE or /NotExact - treat test names as prefixes. Will search all tests that start with testname. By default searches only for the exact name.");
            Console.WriteLine("\t/F or /FullTrust - Runs tests under full trust. By Default tests are run under partial trust. Fulltrust is always used if a code coverage build is detected.");
            Console.WriteLine("\t/CompileToMethod or /CTM - uses Lambda.CompileToMethod to compile trees instead of Lambda.Compile.");
            Console.WriteLine("\t/VerifyAssembly or /VA - Verifies that the IL generated for the expressions is valid. Implies /FullTrust and /CompileToMethod.");
            Console.WriteLine("\t/Rewrite:Rewrittername1,Rewrittername2 - modify tests by running each of the specified rewritters. A comma at the end of the list will also run the tests without modification.");
            Console.WriteLine("\tTestName - Runs only tests that match the name. For tests in AstTest's scenario class, the method name. For tests in ETScenarios, the description. If /NotExact is specified, all tests starting with TestName will be run.");
        }

        public static int Main(string[] args) {
            //If we're running under code coverage we require full permissions.
            //Some tests will not run properly, but that's a sacrifice we need to make.

#if SILVERLIGHT
            FullTrust = true;
#else
            //This environment variable is now set for these runs.
            if (!String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("THISISCOVERAGE")))
            {
                Console.WriteLine("Detected code coverage run - Running all scenarios in full trust");
                FullTrust = true;
            }
#endif

            var runner = new TestRunner { Arguments = args };

            if (runner.Options.Help) {
                PrintArguments();
                return 0;
            }

            if (args.Length > 0) {
                // Explicit option to run in full trust
                // Useful when running under the debugger, for visualizers, etc.
                if (runner.Options.FullTrust) {
                    FullTrust = true;
                }
            }

#if !SILVERLIGHT
            var s = new Stopwatch();
#endif

            int TestsRan = 0;
            int LoaderExitCode = 0;
#if !SILVERLIGHT
            try {
                if (Environment.Version.Major >= 4) {
                    MoveNoPiaFile("NoPiaTests.Dll");
                    MoveNoPiaFile("NoPiaPia.dll");
                    MoveNoPiaFile("NoPiaHelperClass.dll");
                    MoveNoPiaFile("NoPiaHelper2.dll");

                    var NoPiaStream = System.IO.File.OpenRead("NoPiaTests.Dll");
                    Byte[] NoPia = (byte[])Array.CreateInstance(typeof(byte), NoPiaStream.Length);
                    NoPiaStream.Read(NoPia, 0, (int)NoPiaStream.Length);

                    runner.PreLoadedAssemblies.Add(Assembly.Load(NoPia));
                }
            } catch (Exception ex) {
                Console.WriteLine("Failed to load Assemblies " + ex.Message);
                throw;
            }


            s.Start();

            //Currently, only two trust modes are availlable:
            // (default) Partial trust - runs all possible tests under partial trust, runs full trust only tests under full trust 
            // Fulltrust - runs all possible tests as full trust, doesn't run partial trust requiring tests
            //In the future we might have a partial trust only mode for environments where asttest can't be started under full trust
            //The reason why TrustMode has four settings is to allow us not to runt he same test multiple times. (under full and partial trust)
            if (!FullTrust) {
                //PartialTrust run

                var setup = AppDomain.CurrentDomain.SetupInformation;
                var permissions = new PermissionSet(PermissionState.None);
                permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));
                var domain = AppDomain.CreateDomain("Tests", null, setup, permissions);

                // Create the test runner in the remote domain
                var remoteRunner = (TestRunner)domain.CreateInstanceAndUnwrap(
                    typeof(TestRunner).Assembly.FullName, typeof(TestRunner).FullName
                );
                remoteRunner.TrustMode = TestRunner.TrustKind.PartialTrust;
                remoteRunner.Arguments = args;
                remoteRunner.RunTests();
                LoaderExitCode = remoteRunner.ExitCode;
                TestsRan += remoteRunner.TestsRan;

                runner.TrustMode = TestRunner.TrustKind.FullTrustOnly;
                runner.RunTests();
                LoaderExitCode += runner.ExitCode;
                TestsRan += runner.TestsRan;
            } else {
                //Fulltrust run
                runner.TrustMode = TestRunner.TrustKind.FullTrust;
#else
#if SILVERLIGHT3
		//Silverlight 3 can only run the legacy tests, which are all marked FullTrustOnly despite the fact that they work
		//on SL3.  The proper fix here would be to change the attribute on those tests if they really suppor partial trust
		//which would have the added benefit of running them on SL4 (non quirks mode), which we haven't done yet.
		//But that's a bigger change + verification pass, and I'm just trying to consolidate my source changes at the moment.
                runner.TrustMode = TestRunner.TrustKind.FullTrust; //.PartialTrust; //@RYANOOB - tests that are known to work only in "full trust" probably don't work in Silverlight
#else
                runner.TrustMode = TestRunner.TrustKind.PartialTrust; //@RYANOOB - tests that are known to work only in "full trust" probably don't work in Silverlight
#endif
#endif
                runner.RunTests();
                LoaderExitCode = runner.ExitCode;
                TestsRan += runner.TestsRan;
#if !SILVERLIGHT
            }
            s.Stop();
#endif

            string resultStr = String.Format("{0}Ran {1} tests{2}{3}", Environment.NewLine, TestsRan, Environment.NewLine,
                LoaderExitCode == 0 ? "All scenarios passed." : "There were " + LoaderExitCode + " failures.");
#if SILVERLIGHT
            System.Windows.MessageBox.Show(resultStr);
#else
            Console.WriteLine(resultStr);
#endif
            return LoaderExitCode ;
        }

        private static void MoveNoPiaFile(string file)
        {
            System.IO.File.Copy(System.IO.Path.Combine("NoPia", file), file, true);
            System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
        }
    }
}
