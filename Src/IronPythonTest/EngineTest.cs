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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

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
    public delegate string RefStrDelegate(ref string arg);
    public delegate int RefIntDelegate(ref int arg);
    public delegate T GenericDelegate<T,U,V>(U arg1, V arg2);

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

        // Private members
#pragma warning disable 169
        // This field is accessed from the test
        private int privateField;
#pragma warning restore 169
        private int privateProperty { get { return m_property; } set { m_property = value; } }
        private event IntIntDelegate privateEvent;
        private int privateMethod(int arg) {
            if (privateEvent != null)
                return privateEvent(arg);
            else
                return -1;
        }
        private int privateStaticMethod() {
            return 100;
        }
    }

    internal class InternalClsPart {
#pragma warning disable 649
        // This field is accessed from the test
        internal int Field;
#pragma warning restore 649
        int m_property;
        internal int Property { get { return m_property; } set { m_property = value; } }
        internal event IntIntDelegate Event;
        internal int Method(int arg) {
            if (Event != null)
                return Event(arg);
            else
                return -1;
        }
    }

    public class EngineTest :MarshalByRefObject {

        PythonEngine standardEngine = new PythonEngine();

        public EngineTest() {
            // Load a script with all the utility functions that are required
            // standardEngine.ExecuteFile(InputTestDirectory + "\\EngineTests.py");
        }
        
        // Used to test exception thrown in another domain can be shown correctly.
        public void Run(string script) {
            standardEngine.Execute(script);
        }

        static readonly string clspartName = "clsPart";

        // Execute
        public void ScenarioExecute() {
            PythonEngine pe = new PythonEngine();

            ClsPart clsPart = new ClsPart();
            pe.Globals[clspartName] = clsPart;

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
            pe.Globals[clspartName] = new ClsPart();
            pe.Execute("if 0 != clsPart.Field: raise AssertionError('test failed')");

            // add cls method as event handler
            pe.Globals["clsMethod"] = new IntIntDelegate(Negate);
            pe.Execute("clsPart.Event += clsMethod");
            pe.Execute("a = clsPart.Method(2)");
            pe.Execute("if -2 != a: raise AssertionError('test failed')");

            // ===============================================

            // reset the same variable with integer
            pe.Globals[clspartName] = 1;
            pe.Execute("if 1 != clsPart: raise AssertionError('test failed')");
            AreEqual((int)pe.Globals[clspartName], 1);
        }

        // No intereference between two engines
        public void ScenarioNoInterferenceBetweenTwoEngines() {
            PythonEngine pe1 = new PythonEngine();
            PythonEngine pe2 = new PythonEngine();

            pe1.Execute("a = 1");

            try {
                pe2.Execute("print a");
                throw new Exception("Scenario3");
            } catch (IronPython.Runtime.Exceptions.PythonNameErrorException) { }
        }

        public void ScenarioEvaluateInAnonymousEngineModule() {
            EngineModule anonymousScope = standardEngine.CreateModule();
            EngineModule anotherAnonymousScope = standardEngine.CreateModule();
            standardEngine.Execute("x = 0");
            standardEngine.Execute("x = 1", anonymousScope);
            anotherAnonymousScope.Globals["x"] = 2;

            // Ensure that the default EngineModule is not affected
            AreEqual(0, standardEngine.EvaluateAs<int>("x"));
            AreEqual(0, (int)standardEngine.DefaultModule.Globals["x"]);
            // Ensure that the anonymous scope has been updated as expected
            AreEqual(1, standardEngine.EvaluateAs<int>("x", anonymousScope));
            AreEqual(1, (int)anonymousScope.Globals["x"]);
            // Ensure that other anonymous scopes are not affected
            AreEqual(2, standardEngine.EvaluateAs<int>("x", anotherAnonymousScope));
            AreEqual(2, (int)anotherAnonymousScope.Globals["x"]);
        }

        public void ScenarioEvaluateInPublishedEngineModule() {
            EngineModule publishedScope = standardEngine.CreateModule("published_scope_test", true);
            standardEngine.Execute("x = 0");
            standardEngine.Execute("x = 1", publishedScope);
            // Ensure that the default EngineModule is not affected
            AreEqual(0, standardEngine.EvaluateAs<int>("x"));
            // Ensure that the published scope has been updated as expected
            AreEqual(1, standardEngine.EvaluateAs<int>("x", publishedScope));

            // Ensure that the published scope is accessible from other scopes using sys.modules
            standardEngine.Execute(@"
import sys
x_from_published_scope_test = sys.modules['published_scope_test'].x
");
            AreEqual(1, standardEngine.EvaluateAs<int>("x_from_published_scope_test"));
        }


        class CustomDictionary : IDictionary<string, object> {
            // Make "customSymbol" always be accessible. This could have been accomplished just by
            // doing SetGlobal. However, this mechanism could be used for other extensibility
            // purposes like getting a callback whenever the symbol is read
            internal static readonly string customSymbol = "customSymbol";
            internal const int customSymbolValue = 100;

            Dictionary<string, object> dict = new Dictionary<string, object>();

            #region IDictionary<string,object> Members

            public void Add(string key, object value) {
                if (key.Equals(customSymbol))
                    throw new PythonNameErrorException("Cannot assign to customSymbol");
                dict.Add(key, value);
            }

            public bool ContainsKey(string key) {
                if (key.Equals(customSymbol))
                    return true;
                return dict.ContainsKey(key);
            }

            public ICollection<string> Keys {
                get { throw new NotImplementedException("The method or operation is not implemented."); }
            }

            public bool Remove(string key) {
                if (key.Equals(customSymbol))
                    throw new PythonNameErrorException("Cannot delete customSymbol");
                return dict.Remove(key);
            }

            public bool TryGetValue(string key, out object value) {
                if (key.Equals(customSymbol)) {
                    value = customSymbolValue;
                    return true;
                }

                return dict.TryGetValue(key, out value);
            }

            public ICollection<object> Values {
                get { throw new NotImplementedException("The method or operation is not implemented."); }
            }

            public object this[string key] {
                get {
                    if (key.Equals(customSymbol))
                        return customSymbolValue;
                    return dict[key];
                }
                set {
                    Add(key, value);
                }
            }

            #endregion

            #region ICollection<KeyValuePair<string,object>> Members

            public void Add(KeyValuePair<string, object> item) {
                throw new NotImplementedException("The method or operation is not implemented.");
            }

            public void Clear() {
                throw new NotImplementedException("The method or operation is not implemented.");
            }

            public bool Contains(KeyValuePair<string, object> item) {
                throw new NotImplementedException("The method or operation is not implemented.");
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
                throw new NotImplementedException("The method or operation is not implemented.");
            }

            public int Count {
                get { throw new NotImplementedException("The method or operation is not implemented."); }
            }

            public bool IsReadOnly {
                get { throw new NotImplementedException("The method or operation is not implemented."); }
            }

            public bool Remove(KeyValuePair<string, object> item) {
                throw new NotImplementedException("The method or operation is not implemented.");
            }

            #endregion

            #region IEnumerable<KeyValuePair<string,object>> Members

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
                throw new NotImplementedException("The method or operation is not implemented.");
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                throw new NotImplementedException("The method or operation is not implemented.");
            }

            #endregion
        }

        public void ScenarioCustomDictionary() {
            CustomDictionary customGlobals = new CustomDictionary();
            EngineModule customModule = standardEngine.CreateModule("customModule", customGlobals, true);

            // Evaluate
            AreEqual(standardEngine.EvaluateAs<int>("customSymbol + 1", customModule), CustomDictionary.customSymbolValue + 1);

            // Execute
            standardEngine.Execute("customSymbolPlusOne = customSymbol + 1", customModule);
            AreEqual(standardEngine.EvaluateAs<int>("customSymbolPlusOne", customModule), CustomDictionary.customSymbolValue + 1);
            AreEqual((int)customModule.Globals["customSymbolPlusOne"], CustomDictionary.customSymbolValue + 1);

            // Compile
            CompiledCode compiledCode = standardEngine.Compile("customSymbolPlusTwo = customSymbol + 2");
            compiledCode.Execute(customModule);
            AreEqual(standardEngine.EvaluateAs<int>("customSymbolPlusTwo", customModule), CustomDictionary.customSymbolValue + 2);
            AreEqual((int)customModule.Globals["customSymbolPlusTwo"], CustomDictionary.customSymbolValue + 2);

            // check overriding of Add
            try {
                standardEngine.Execute("customSymbol = 1", customModule);
                throw new Exception("We should not reach here");
            } catch (PythonNameErrorException) { }

            try {
                standardEngine.Execute(@"global customSymbol
customSymbol = 1", customModule);
                throw new Exception("We should not reach here");
            } catch (PythonNameErrorException) { }

            // check overriding of Remove
            try {
                standardEngine.Execute("del customSymbol", customModule);
                throw new Exception("We should not reach here");
            } catch (PythonNameErrorException) { }

            try {
                standardEngine.Execute(@"global customSymbol
del customSymbol", customModule);
                throw new Exception("We should not reach here");
            } catch (PythonNameErrorException) { }

            // vars()
            IDictionary vars = standardEngine.EvaluateAs<IDictionary>("vars()", customModule);
            AreEqual(true, vars.Contains("customSymbol"));
        }

        public void ScenarioModuleWithLocals() {
            // Ensure that the namespace is not already polluted
            if (standardEngine.Globals.ContainsKey("global_variable"))
                standardEngine.Globals.Remove("global_variable");
            if (standardEngine.Globals.ContainsKey("local_variable"))
                standardEngine.Globals.Remove("local_variable");

            // Simple use of locals
            Dictionary<string, object> locals = new Dictionary<string, object>();
            locals["local_variable"] = 100;
            AreEqual(100, standardEngine.EvaluateAs<int>("local_variable", standardEngine.DefaultModule, locals));

            // Ensure simple writes go to locals, not globals
            standardEngine.Execute("local_variable = 200", standardEngine.DefaultModule, locals);
            AreEqual(200, standardEngine.EvaluateAs<int>("local_variable", standardEngine.DefaultModule, locals));
            AreEqual(200, (int)locals["local_variable"]);
            if (standardEngine.Globals.ContainsKey("local_variable"))
                throw new Exception("local_variable is set in the Globals dictionary");
            try {
                standardEngine.Evaluate("local_variable", standardEngine.DefaultModule, null);
            } catch (PythonNameErrorException) { }

            // Ensure writes to globals go to globals, not locals
            standardEngine.Execute(@"global global_variable
global_variable = 300", standardEngine.DefaultModule, locals);
            AreEqual(300, standardEngine.EvaluateAs<int>("global_variable", standardEngine.DefaultModule, locals));
            AreEqual(300, (int)standardEngine.Globals["global_variable"]);
            if (locals.ContainsKey("global_variable"))
                throw new Exception("global_variable is set in the locals dictionary");

            // vars()
            IDictionary vars = standardEngine.EvaluateAs<IDictionary>("vars()", standardEngine.DefaultModule, locals);
            AreEqual(true, vars.Contains("local_variable"));
        }

        static long GetTotalMemory() {
            // Critical objects can take upto 3 GCs to be collected
            for (int i = 0; i < 3; i++) {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return GC.GetTotalMemory(true);
        }

        const string scenarioGCModuleName = "scenario_gc";

        void CreateModules(PythonEngine engine) {
            for (int scopeCount = 0; scopeCount < 100; scopeCount++) {
                EngineModule scope = engine.CreateModule(scenarioGCModuleName, true);
                scope.Globals["x"] = "Hello";
                engine.ExecuteFile(Common.InputTestDirectory + "\\simpleCommand.py", scope);
                AreEqual(engine.EvaluateAs<int>("x", scope), 1);
            }
        }

        public void ScenarioGC() {
            long initialMemory = GetTotalMemory();

            // Create multiple engines and scopes
            for (int engineCount = 0; engineCount < 100; engineCount++) {
                PythonEngine engine = new PythonEngine();
                CreateModules(engine);
            }

            // Create multiple scopes in an engine that is not collected
            CreateModules(standardEngine);
            standardEngine.Sys.modules.Remove(scenarioGCModuleName);

            long finalMemory = GetTotalMemory();
            long memoryUsed = finalMemory - initialMemory;
            const long memoryThreshold = 100000;
            if (memoryUsed > memoryThreshold)
                throw new Exception(String.Format("ScenarioGC used {0} bytes of memory. The threshold is {1} bytes", memoryUsed, memoryThreshold));
        }

        // Evaluate
        public void ScenarioEvaluate() {
            PythonEngine pe = new PythonEngine();

            AreEqual(10, (int)pe.Evaluate("4+6"));
            AreEqual(10, pe.EvaluateAs<int>("4+6"));

            AreEqual("abab", (string)pe.Evaluate("'ab' * 2"));
            AreEqual("abab", pe.EvaluateAs<string>("'ab' * 2"));

            ClsPart clsPart = new ClsPart();
            pe.Globals[clspartName] = clsPart;
            AreEqual(clsPart, pe.Evaluate("clsPart") as ClsPart);
            AreEqual(clsPart, pe.EvaluateAs<ClsPart>("clsPart"));

            pe.Execute("clsPart.Field = 100");
            AreEqual(100, (int)pe.Evaluate("clsPart.Field"));
            AreEqual(100, pe.EvaluateAs<int>("clsPart.Field"));

            // Ensure that we can get back a delegate to a Python method
            pe.Execute("def IntIntMethod(a): return a * 100");
            IntIntDelegate d = pe.EvaluateAs<IntIntDelegate>("IntIntMethod");
            AreEqual(d(2), 2 * 100);
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
                pe.Globals[clspartName] = clsPart;
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

                AreEqual(-1, pe.EvaluateAs<int>("M1()"));
                AreEqual(+1, pe.EvaluateAs<int>("M2()"));

                AreEqual(-1, (int)pe.Evaluate("M1()"));
                AreEqual(+1, (int)pe.Evaluate("M2()"));

                pe.Execute("if M1() != -1: raise AssertionError('test failed')");
                pe.Execute("if M2() != +1: raise AssertionError('test failed')");


                pe.Execute("c = C()");
                AreEqual(-1, pe.EvaluateAs<int>("c.M1()"));
                AreEqual(+1, pe.EvaluateAs<int>("c.M2()"));

                AreEqual(-1, (int)pe.Evaluate("c.M1()"));
                AreEqual(+1, (int)pe.Evaluate("c.M2()"));

                pe.Execute("if c.M1() != -1: raise AssertionError('test failed')");
                pe.Execute("if c.M2() != +1: raise AssertionError('test failed')");


                //AreEqual(-1, pe.EvaluateAs<int>("C1.M()"));
                //AreEqual(+1, pe.EvaluateAs<int>("C2.M()"));

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
            AreEqual(1, pe.EvaluateAs<int>("a"));
            AreEqual(-1, pe.EvaluateAs<int>("b"));
        }

        // AddToPath
        public void ScenarioAddToPath() {
            PythonEngine pe = new PythonEngine();
            string ipc_dll = typeof(PythonEngine).Assembly.Location;
            string ipc_path = Path.GetDirectoryName(ipc_dll);
            pe.InitializeModules(ipc_path, ipc_path + "\\ipy.exe", PythonEngine.VersionString);
            string tempFile1 = Path.GetTempFileName();

            try {
                File.WriteAllText(tempFile1, "from lib.assert_util import *");

                try {
                    pe.ExecuteFile(tempFile1);
                    throw new Exception("Scenario7");
                } catch (IronPython.Runtime.Exceptions.PythonImportErrorException) { }

                pe.AddToPath(Common.ScriptTestDirectory);

                pe.ExecuteFile(tempFile1);
                pe.Execute("AreEqual(5, eval('2 + 3'))");
            } finally {
                File.Delete(tempFile1);
            }
        }

        // Options.ClrDebuggingEnabled

        delegate void ThrowExceptionDelegate();
        static void ThrowException() {
            throw new Exception("Exception from ThrowException");
        }

        public void ScenarioClrDebuggingEnabled() {
            const string lineNumber1 = "raise.py:line 21";
            const string lineNumber2 = "raise.py:line 23";

            EngineOptions engineOptions = new EngineOptions();
            engineOptions.ClrDebuggingEnabled = true;
            PythonEngine debuggableEngine = new PythonEngine(engineOptions);

            // Ensure that you do get good line numbers with ExecutionOptions.Default
            try {
                debuggableEngine.ExecuteFile(Common.InputTestDirectory + "\\raise.py");
                throw new Exception("We should not get here");
            } catch (IronPython.Runtime.Exceptions.StringException e1) {
                if (!e1.StackTrace.Contains(lineNumber1) || !e1.StackTrace.Contains(lineNumber2))
                    throw new Exception("Debugging is not enabled even though Options.ClrDebuggingEnabled is specified");
            }

            // Ensure that you do not get good line numbers without ExecutionOptions.Default
            try {
                standardEngine.ExecuteFile(Common.InputTestDirectory + "\\raise.py");
                throw new Exception("We should not get here");
            } catch (StringException e2) {
                if (e2.StackTrace.Contains(lineNumber1) || e2.StackTrace.Contains(lineNumber2))
                    throw new Exception("Debugging is enabled even though Options.ClrDebuggingEnabled is not specified");
            }
        }

        public void ScenarioExecuteFileOptimized() {
            PythonEngine pe = new PythonEngine();
            OptimizedEngineModule engineModule = pe.CreateOptimizedModule(Common.InputTestDirectory + "\\simpleCommand.py", "__main__", false);
            engineModule.Execute();
            AreEqual(1, pe.EvaluateAs<int>("x", engineModule));

            // Ensure that we can set new globals in the scope, and execute further code
            engineModule.Globals["y"] = 2;
            AreEqual(3, pe.EvaluateAs<int>("x+y", engineModule));
        }

        public void ScenarioOptimizedModuleBeforeExecutingGlobalCode() {
            PythonEngine pe = new PythonEngine();
            OptimizedEngineModule engineModule = pe.CreateOptimizedModule(Common.InputTestDirectory + "\\uninitializedGlobal.py", "__main__", true);
            AreEqual(engineModule.Globals.ContainsKey("x"), false);
            AreEqual(engineModule.Globals.ContainsKey("y"), false);

            // Set a global variable before executing global code
            engineModule.Globals["x"] = 1;
            engineModule.Execute();

            // Ensure that the global code picked up the value of "x" that was pumped into the module
            AreEqual(101, pe.EvaluateAs<int>("x", engineModule));
            AreEqual(2, pe.EvaluateAs<int>("y", engineModule));
        }

        public void ScenarioCreateOptimizedModule_ScriptThrows() {
            // We should be able to use the EngineModule even if an exception is thrown by the script
            OptimizedEngineModule engineModule = standardEngine.CreateOptimizedModule(Common.InputTestDirectory + "\\raise.py", "__main__", true);
            try {
                engineModule.Execute();
                throw new Exception("We should not reach here");
            } catch (StringException) {
                AreEqual(1, standardEngine.EvaluateAs<int>("x", engineModule));

                // Ensure that we can set new globals in the scope, and execute further code
                engineModule.Globals["y"] = 2;
                AreEqual(3, standardEngine.EvaluateAs<int>("x+y", engineModule));
            }
        }

        // Compile and Run
        public void ScenarioCompileAndRun() {
            PythonEngine pe = new PythonEngine();
            ClsPart clsPart = new ClsPart();

            pe.Globals[clspartName] = clsPart;
            CompiledCode compiledCode = pe.Compile("def f(): clsPart.Field += 10");
            compiledCode.Execute();

            compiledCode = pe.Compile("f()");
            compiledCode.Execute();
            AreEqual(10, clsPart.Field);
            compiledCode.Execute();
            AreEqual(20, clsPart.Field);
        }

        public void ScenarioStreamRedirect() {
            MemoryStream stdout = new MemoryStream();
            MemoryStream stdin = new MemoryStream();
            MemoryStream stderr = new MemoryStream();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] buffer = new byte[15];

            PythonEngine pe = new PythonEngine();
            pe.SetStandardError(stderr);
            pe.SetStandardInput(stdin);
            pe.SetStandardOutput(stdout);
            pe.Execute("import sys");

            stdin.Write(encoding.GetBytes("This is stdout"), 0, 14);
            stdin.Position = 0;
            pe.Execute("output = sys.__stdin__.readline()");
            AreEqual("This is stdout",pe.EvaluateAs<string>("output"));
            pe.Execute("sys.__stdout__.write(output)");
            stdout.Flush();
            stdout.Position = 0;
            int len = stdout.Read(buffer, 0, 14);
            AreEqual(14, len);
            AreEqual("This is stdout",encoding.GetString(buffer,0,len));

            pe.Execute("sys.__stderr__.write(\"This is stderr\")");
            stderr.Flush();
            stderr.Position = 0;
            len = stderr.Read(buffer, 0, 14);
            AreEqual(14, len);
            AreEqual("This is stderr", encoding.GetString(buffer,0,len));        
        }

        public void ScenarioCreateMethodAndLambda() {
            PythonEngine pe = new PythonEngine();
            
            //Generic delegate types
            GenericDelegate<int,int,int> del = pe.CreateLambda<GenericDelegate<int,int,int>>("arg1+arg2");
            AreEqual(9, del(6, 3));
            del = pe.CreateMethod<GenericDelegate<int, int, int>>("return arg1+arg2");
            AreEqual(12, del(8, 4));

            //ByRef reference type arguments
            string arg = "Hello";
            RefStrDelegate rsdel = null;
            rsdel = pe.CreateLambda<RefStrDelegate>("'%s World!' % arg.Value");
            AreEqual("Hello World!", rsdel(ref arg));

            arg = "Hello";
            rsdel = pe.CreateMethod<RefStrDelegate>("arg.Value = '%s World!' % arg.Value\nreturn arg.Value");
            AreEqual("Hello World!", rsdel(ref arg));
            AreEqual("Hello World!", arg);

            //ByRef value type arguments
            int argi = 323;
            RefIntDelegate ridel = null;
            ridel = pe.CreateLambda<RefIntDelegate>("arg.Value+27");
            AreEqual(350, ridel(ref argi));

            argi = 323;
            ridel = pe.CreateMethod<RefIntDelegate>("arg.Value+=127\nreturn arg.Value+1");
            AreEqual(451, ridel(ref argi));
            AreEqual(450, argi);

            //Specify our own parameter names
            List<string> args = new List<string>(new string[] { "dir","secondarg"});
            del = pe.CreateLambda<GenericDelegate<int, int, int>>("dir+secondarg", args);
            AreEqual(6, del(3, 3));
            del = pe.CreateMethod<GenericDelegate<int, int, int>>("return dir+secondarg", args);
            AreEqual(6,del(3,3));
        }

        public void ScenarioCoverage() {
            PythonEngine pe = new PythonEngine();
            CompiledCode compiledCode = pe.Compile("arg1+arg2","MyNewSourceFile");
            AreEqual("<code MyNewSourceFile>", compiledCode.ToString());
        }

        public void ScenarioNullArguments() {
            PythonEngine pe = new PythonEngine();
            string tempFile1 = Path.GetTempFileName();

            try {
                pe.Compile(null, tempFile1);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }

            try {
                pe.CompileFile(null);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }

            try {
                pe.CreateLambda<IntIntDelegate>("x=2",null,null);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }

            try {
                pe.CreateMethod<IntIntDelegate>("x=2", null, null);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }

            try {
                pe.CreateModule(null, new Dictionary<string,object>(), false);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }

            try {
                pe.CreateModule("moduleName", null, false);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }

            try {
                pe.CreateOptimizedModule(null, "moduleName", false);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }

            try {
                pe.CreateOptimizedModule(tempFile1, null, false);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }

            try {
                pe = new PythonEngine(null);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }
        }

        private static void ExecuteAndVerify(PythonEngine pe, EngineModule module, IDictionary<string, object> locals, string text, string expectedText) {
            MemoryStream output = new MemoryStream();
            pe.SetStandardOutput(output);
            pe.Execute(text, module, locals);
            output.Flush();
            output.Position = 0;
            AreEqual((new StreamReader(output)).ReadToEnd(), expectedText);
        }

        public void ScenarioVariableScoping() {
            PythonEngine pe = new PythonEngine();

            string tempFile1 = Path.GetTempFileName();

            try {
                Dictionary<string, object> globals = new Dictionary<string, object>();
                Dictionary<string, object> locals = new Dictionary<string, object>();
                EngineModule module1 = pe.CreateModule("module1", globals, true);

                //Set variables in each scope
                globals["x"] = 1;
                File.WriteAllText(tempFile1, "y = 2");
                pe.ExecuteFile(tempFile1, module1, locals);
                locals["z"] = 3;

                //Query for a variable from each scope
                AreEqual((int)pe.Evaluate("x", module1, locals), 1);
                AreEqual((int)pe.Evaluate("y", module1, locals), 2);
                AreEqual((int)pe.Evaluate("z", module1, locals), 3);

                AreEqual(pe.EvaluateAs<int>("x", module1, locals), 1);
                AreEqual(pe.EvaluateAs<int>("y", module1, locals), 2);
                AreEqual(pe.EvaluateAs<int>("z", module1, locals), 3);

                ExecuteAndVerify(pe, module1, locals, "print x", "1" + Environment.NewLine);
                ExecuteAndVerify(pe, module1, locals, "print y", "2" + Environment.NewLine);
                ExecuteAndVerify(pe, module1, locals, "print z", "3" + Environment.NewLine);

                //Set conflicting variables in each scope and verify resolution
                module1.Globals["scope"] = "module";
                AreEqual("module", pe.EvaluateAs<string>("scope", module1, locals));
                locals["scope"] = "local";
                AreEqual("local", pe.EvaluateAs<string>("scope", module1, locals));
                pe.Execute("scope='also local'", module1, locals);
                AreEqual("also local", pe.EvaluateAs<string>("scope", module1, locals));
                AreEqual("also local", (string)locals["scope"]);

                //Validate that the module dict hasn't changed in an unexpected way
                AreEqual(false, module1.Globals.ContainsKey("y"));
                AreEqual(false, module1.Globals.ContainsKey("z"));

                //Remove the conflicting variables from each scope and verify resolution
                pe.Execute("del(scope)", module1, locals);
                AreEqual(false, locals.ContainsKey("scope"));
                AreEqual("module", pe.EvaluateAs<string>("scope", module1, locals));
                module1.Globals.Remove("scope");
                try {
                    AreEqual("module", pe.EvaluateAs<string>("scope", module1, locals));
                    Console.WriteLine("All references to scope have been removed, we should get an error when looking up this name");
                    throw new Exception();
                } catch (PythonNameErrorException) {
                    //Pass
                }

                //Change an existing variable in each scope
                locals["z"]  = 6;
                pe.Execute("y = 5", module1, locals);
                globals["x"] = 4;

                //Verify that the engine gets the updated values
                AreEqual(4, (int)pe.Evaluate("x", module1, locals));
                AreEqual(5, (int)pe.Evaluate("y", module1, locals));
                AreEqual(6, (int)pe.Evaluate("z", module1, locals));

                AreEqual(4, pe.EvaluateAs<int>("x", module1, locals));
                AreEqual(5, pe.EvaluateAs<int>("y", module1, locals));
                AreEqual(6, pe.EvaluateAs<int>("z", module1, locals));

                ExecuteAndVerify(pe, module1, locals, "print x", "4" + Environment.NewLine);
                ExecuteAndVerify(pe, module1, locals, "print y", "5" + Environment.NewLine);
                ExecuteAndVerify(pe, module1, locals, "print z", "6" + Environment.NewLine);

                //Hide a builtin from each scope and verify we can recover it
                globals["dir"] = "overriden in module";
                AreEqual("overriden in module",pe.EvaluateAs<string>("dir", module1, locals));
                globals.Remove("dir");
                AreEqual(true, pe.Evaluate("dir", module1, locals) is IronPython.Runtime.Calls.BuiltinFunction);

                locals["dir"] = "overriden in locals";
                AreEqual("overriden in locals", pe.EvaluateAs<string>("dir", module1, locals));
                locals.Remove("dir");
                AreEqual(true, pe.Evaluate("dir", module1, locals) is IronPython.Runtime.Calls.BuiltinFunction);
            } finally {
                File.Delete(tempFile1);
            }
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
            CompiledCode compiledCode = pe.Compile(script);
            compiledCode.Execute();
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
