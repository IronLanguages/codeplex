/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime;
using System.IO;

namespace IronPythonTest {
    class Common {
        public static string RuntimeDirectory;
        public static string ScriptTestDirectory;
        public static string InputTestDirectory;

        static Common() {
            RuntimeDirectory = Path.GetDirectoryName(typeof(PythonEngine).Assembly.Location);
            ScriptTestDirectory = Path.Combine(RuntimeDirectory, @"Src\Tests");
            InputTestDirectory = Path.Combine(ScriptTestDirectory, "Inputs");
        }
    }

    public delegate int IntIntDelegate(int arg);

    public class ClsPart {
        public int Field;
        int m_property;
        public int Property { get { return m_property; } set { m_property = value; } }
        public event IntIntDelegate Event;
        public int Method(int arg) {
            if (Event != null)
                return Event(arg);
            else
                return -1;
        }
    }

    public class EngineTest {

        PythonEngine standardEngine = new PythonEngine();

        public EngineTest() {
            // Load a script with all the utility functions that are required
            // standardEngine.ExecuteFile(InputTestDirectory + "\\EngineTests.py");
        }

        // Execute
        public void ScenarioExecute() {
            PythonEngine pe = new PythonEngine();

            ClsPart clsPart = new ClsPart();
            pe.SetVariable("clsPart", clsPart);

            // field: assign and get back
            pe.Execute("clsPart.Field = 100");
            pe.Execute("if 100 != clsPart.Field: raise AssertionError('test failed')");
            AreEqual(100, clsPart.Field);

            // property: assign and get back
            pe.Execute("clsPart.Property = clsPart.Field");
            pe.Execute("if 100 != clsPart.Property: raise AssertionError('test failed')");
            AreEqual(100, clsPart.Property);

            // method: Event not set yet
            pe.Execute("a = clsPart.Method(2)");
            pe.Execute("if -1 != a: raise AssertionError('test failed')");

            // method: add python func as event handler
            pe.Execute("def f(x) : return x * x");
            pe.Execute("clsPart.Event += f");
            pe.Execute("a = clsPart.Method(2)");
            pe.Execute("if 4 != a: raise AssertionError('test failed')");

            // ===============================================

            // reset the same variable with instance of the same type
            pe.SetVariable("clsPart", new ClsPart());
            pe.Execute("if 0 != clsPart.Field: raise AssertionError('test failed')");

            // add cls method as event handler
            pe.SetVariable("clsMethod", new IntIntDelegate(Negate));
            pe.Execute("clsPart.Event += clsMethod");
            pe.Execute("a = clsPart.Method(2)");
            pe.Execute("if -2 != a: raise AssertionError('test failed')");

            // ===============================================

            // reset the same variable with integer
            pe.SetVariable("clsPart", 1);
            pe.Execute("if 1 != clsPart: raise AssertionError('test failed')");
            AreEqual((int)pe.GetVariable("clsPart"), 1);
        }

        // No intereference between two engines
        public void ScenarioNoInterferenceBetweenTwoEngines() {
            PythonEngine pe1 = new PythonEngine();
            PythonEngine pe2 = new PythonEngine();

            pe1.Execute("a = 1");

            try {
                pe2.Execute("print a");
                throw new Exception("Scenario3");
            } catch (IronPython.Runtime.PythonNameError) { }
        }

        public void ScenarioEvaluateInAnonymousModuleScope() {
            Frame anonymousScope = new Frame();
            Frame anotherAnonymousScope = new Frame();
            standardEngine.Execute("x = 0");
            standardEngine.Execute("x = 1", anonymousScope, ExecutionOptions.Default);
            anotherAnonymousScope.SetGlobal((SymbolId)"x", 2);

            // Ensure that the default ModuleScope is not affected
            AreEqual(0, standardEngine.Evaluate<int>("x"));
            AreEqual(0, (int)standardEngine.DefaultModuleScope.GetGlobal((SymbolId)"x"));
            // Ensure that the anonymous scope has been updated as expected
            AreEqual(1, standardEngine.Evaluate<int>("x", anonymousScope, ExecutionOptions.Default));
            AreEqual(1, (int)anonymousScope.GetGlobal((SymbolId)"x"));
            // Ensure that other anonymous scopes are not affected
            AreEqual(2, standardEngine.Evaluate<int>("x", anotherAnonymousScope, ExecutionOptions.Default));
            AreEqual(2, (int)anotherAnonymousScope.GetGlobal((SymbolId)"x"));
        }

        public void ScenarioEvaluateInPublishedModuleScope() {
            Frame publishedScope = new Frame("published_scope_test");
            standardEngine.Execute("x = 0");
            standardEngine.Execute("x = 1", publishedScope, ExecutionOptions.Default);
            // Ensure that the default ModuleScope is not affected
            AreEqual(0, standardEngine.Evaluate<int>("x"));
            // Ensure that the published scope has been updated as expected
            AreEqual(1, standardEngine.Evaluate<int>("x", publishedScope, ExecutionOptions.Default));

            // Ensure that the published scope is accessible from other scopes using sys.modules
            standardEngine.Execute(@"
import sys
x_from_published_scope_test = sys.modules['published_scope_test'].x
");
            AreEqual(1, standardEngine.Evaluate<int>("x_from_published_scope_test"));
        }

        // Evaluate
        public void ScenarioEvaluate() {
            PythonEngine pe = new PythonEngine();

            AreEqual(10, (int)pe.Evaluate("4+6"));
            AreEqual(10, pe.Evaluate<int>("4+6"));

            AreEqual("abab", (string)pe.Evaluate("'ab' * 2"));
            AreEqual("abab", pe.Evaluate<string>("'ab' * 2"));

            ClsPart clsPart = new ClsPart();
            pe.SetVariable("clsPart", clsPart);
            AreEqual(clsPart, pe.Evaluate("clsPart") as ClsPart);
            AreEqual(clsPart, pe.Evaluate<ClsPart>("clsPart"));

            pe.Execute("clsPart.Field = 100");
            AreEqual(100, (int)pe.Evaluate("clsPart.Field"));
            AreEqual(100, pe.Evaluate<int>("clsPart.Field"));
        }

        // ExecuteFile
        public void ScenarioExecuteFile() {
            PythonEngine pe = new PythonEngine();

            string tempFile1 = Path.GetTempFileName();
            string tempFile2 = Path.GetTempFileName();

            try {
                using (StreamWriter sw = new StreamWriter(tempFile1)) {
                    sw.WriteLine("var1 = (10, 'z')");
                    sw.WriteLine("");
                    sw.WriteLine("clsPart.Field = 100");
                    sw.WriteLine("clsPart.Property = clsPart.Field * 5");
                    sw.WriteLine("clsPart.Event += (lambda x: x*x)");
                }

                ClsPart clsPart = new ClsPart();
                pe.SetVariable("clsPart", clsPart);
                pe.ExecuteFile(tempFile1);

                using (StreamWriter sw = new StreamWriter(tempFile2)) {
                    sw.WriteLine("if var1[0] != 10: raise AssertionError('test failed')");
                    sw.WriteLine("if var1[1] != 'z': raise AssertionError('test failed')");
                    sw.WriteLine("");
                    sw.WriteLine("if clsPart.Property != clsPart.Field * 5: raise AssertionError('test failed')");
                    sw.WriteLine("var2 = clsPart.Method(var1[0])");
                    sw.WriteLine("if var2 != 10 * 10: raise AssertionError('test failed')");
                }

                pe.ExecuteFile(tempFile2);
            } finally {
                File.Delete(tempFile1);
                File.Delete(tempFile2);
            }
        }

        // Bug: 542
        public void Scenario542() {
            PythonEngine pe = new PythonEngine();
            string tempFile1 = Path.GetTempFileName();

            try {
                using (StreamWriter sw = new StreamWriter(tempFile1)) {
                    sw.WriteLine("def M1(): return -1");
                    sw.WriteLine("def M2(): return +1");

                    sw.WriteLine("class C:");
                    sw.WriteLine("    def M1(self): return -1");
                    sw.WriteLine("    def M2(self): return +1");

                    sw.WriteLine("class C1:");
                    sw.WriteLine("    def M(): return -1");
                    sw.WriteLine("class C2:");
                    sw.WriteLine("    def M(): return +1");
                }

                pe.ExecuteFile(tempFile1);

                AreEqual(-1, pe.Evaluate<int>("M1()"));
                AreEqual(+1, pe.Evaluate<int>("M2()"));

                AreEqual(-1, (int)pe.Evaluate("M1()"));
                AreEqual(+1, (int)pe.Evaluate("M2()"));

                pe.Execute("if M1() != -1: raise AssertionError('test failed')");
                pe.Execute("if M2() != +1: raise AssertionError('test failed')");


                pe.Execute("c = C()");
                AreEqual(-1, pe.Evaluate<int>("c.M1()"));
                AreEqual(+1, pe.Evaluate<int>("c.M2()"));

                AreEqual(-1, (int)pe.Evaluate("c.M1()"));
                AreEqual(+1, (int)pe.Evaluate("c.M2()"));

                pe.Execute("if c.M1() != -1: raise AssertionError('test failed')");
                pe.Execute("if c.M2() != +1: raise AssertionError('test failed')");


                //AreEqual(-1, pe.Evaluate<int>("C1.M()"));
                //AreEqual(+1, pe.Evaluate<int>("C2.M()"));

                //AreEqual(-1, (int)pe.Evaluate("C1.M()"));
                //AreEqual(+1, (int)pe.Evaluate("C2.M()"));

                //pe.Execute("if C1.M() != -1: raise AssertionError('test failed')");
                //pe.Execute("if C2.M() != +1: raise AssertionError('test failed')");

            } finally {
                File.Delete(tempFile1);
            }
        }

        // Bug: 167 
        public void Scenario167() {
            PythonEngine pe = new PythonEngine();

            pe.Execute("a=1\r\nb=-1");
            AreEqual(1, pe.Evaluate<int>("a"));
            AreEqual(-1, pe.Evaluate<int>("b"));
        }

        // AddToPath
        public void ScenarioAddToPath() {
            PythonEngine pe = new PythonEngine();
            string ipc_dll = typeof(PythonEngine).Assembly.Location;
            string ipc_path = Path.GetDirectoryName(ipc_dll);
            pe.InitializeModules(ipc_path, ipc_path + "\\IronPythonConsole.exe", PythonEngine.VersionString);
            string tempFile1 = Path.GetTempFileName();

            try {
                File.WriteAllText(tempFile1, "from lib.assert_util import *");

                try {
                    pe.ExecuteFile(tempFile1);
                    throw new Exception("Scenario7");
                } catch (IronPython.Runtime.PythonImportError) { }

                pe.AddToPath(Common.ScriptTestDirectory);

                pe.ExecuteFile(tempFile1);
                pe.Execute("AreEqual(5, eval('2 + 3'))");
            } finally {
                File.Delete(tempFile1);
            }
        }

        // ExecutionOptions.EnableDebugging

        delegate void ThrowExceptionDelegate();
        static void ThrowException() {
            throw new Exception("Exception from ThrowException");
        }

        public void ScenarioEnableDebugging() {
            const string lineNumber = "raise.py:line 18";
            // Ensure that you do get good line numbers with ExecutionOptions.Default
            try {
                standardEngine.ExecuteFile(Common.InputTestDirectory + "\\raise.py", standardEngine.DefaultModuleScope, ExecutionOptions.EnableDebugging);
                throw new Exception("We should not get here");
            } catch (StringException e1) {
                if (!e1.StackTrace.Contains(lineNumber))
                    throw new Exception("Debugging is not enabled even though ExecutionOptions.EnableDebugging is specified");
            }

            // Ensure that you do not get good line numbers without ExecutionOptions.Default
            try {
                standardEngine.ExecuteFile(Common.InputTestDirectory + "\\raise.py", standardEngine.DefaultModuleScope, ExecutionOptions.Default);
                throw new Exception("We should not get here");
            } catch (StringException e2) {
                if (e2.StackTrace.Contains(lineNumber))
                    throw new Exception("Debugging is enabled even though ExecutionOptions.EnableDebugging is not specified");
            }
        }

        public void ScenarioExecuteFileOptimized() {
            PythonEngine pe = new PythonEngine();
            Frame moduleScope;
            pe.ExecuteFileOptimized(Common.InputTestDirectory + "\\simpleCommand.py", null, ExecutionOptions.Default, out moduleScope);
            AreEqual(1, pe.Evaluate<int>("x", moduleScope, ExecutionOptions.Default));

            // Ensure that we can set new globals in the scope, and execute further code
            moduleScope.SetGlobal((SymbolId)"y", 2);
            AreEqual(3, pe.Evaluate<int>("x+y", moduleScope, ExecutionOptions.Default));
        }

        public void ScenarioExecuteFileOptimized_ScriptThrows() {
            // We should be able to use the Frame even if an exception is thrown by the script
            PythonEngine pe = new PythonEngine();
            Frame moduleScope = null;
            try {
                pe.ExecuteFileOptimized(Common.InputTestDirectory + "\\raise.py", null, ExecutionOptions.Default, out moduleScope);
                throw new Exception("We should not reach here");
            } catch (StringException) {
                AreEqual(1, pe.Evaluate<int>("x", moduleScope, ExecutionOptions.Default));

                // Ensure that we can set new globals in the scope, and execute further code
                moduleScope.SetGlobal((SymbolId)"y", 2);
                AreEqual(3, pe.Evaluate<int>("x+y", moduleScope, ExecutionOptions.Default));
            }
        }

        public void ScenarioExecuteFileOptimized_ParserThrows() {
            // We should be able to use the Frame even if an exception is thrown before the script starts executing
            PythonEngine pe = new PythonEngine();
            Frame moduleScope = null;
            try {
                pe.ExecuteFileOptimized(Common.InputTestDirectory + "\\syntaxError.py", null, ExecutionOptions.Default, out moduleScope);
                throw new Exception("We should not reach here");
            } catch (PythonSyntaxError) {
                AreEqual((Frame)null, moduleScope);
            }
        }

        // Compile and Run
        public void ScenarioCompileAndRun() {
            PythonEngine pe = new PythonEngine();
            ClsPart clsPart = new ClsPart();

            pe.SetVariable("clsPart", clsPart);
            object code = pe.Compile("def f(): clsPart.Field += 10");
            pe.Execute(code);

            code = pe.Compile("f()");
            pe.Execute(code);
            AreEqual(10, clsPart.Field);
            pe.Execute(code);
            AreEqual(20, clsPart.Field);
            
        }

        public void Scenario10() {
            PythonEngine pe = new PythonEngine();
            // suppress output to the console when we do the execute...
            pe.Execute(@"
import sys
sys.stdout = file('testfile.tmp', 'w')
");
            object code = pe.Compile("45", ExecutionOptions.PrintExpressions);
            pe.Execute(code);

            code = pe.Compile("a = _");
            pe.Execute(code);

            code = pe.Compile("if a != 45: raise Exception");
            pe.Execute(code);


            pe.Execute(@"
sys.stdout.close()
import sys
sys.stdout = sys.__stdout__
import nt
nt.unlink('testfile.tmp')
");
        }

        public void Scenario12() {
            PythonEngine pe = new PythonEngine();

            string script = @"
class R(object):
    def __init__(self, a, b):
        self.a = a
        self.b = b
   
    def M(self):
        return self.a + self.b

    sum = property(M, None, None, None)

r = R(10, 100)
if r.sum != 110:
    raise AssertionError('Scenario 12 failed')
";
            object compiled = pe.Compile(script);
            pe.Execute(compiled);
        }

        // More to come: exception related...

        public static int Negate(int arg) { return -1 * arg; }

        static void AreEqual<T>(T expected, T actual) {
            if (expected == null && actual == null) return;

            if (!expected.Equals(actual)) {
                Console.WriteLine("{0} {1}", expected, actual);
                throw new Exception();
            }
        }
    }
}
