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
using System.Text;
using System.IO;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Hosting;

using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython;
using IronPython.Runtime.Calls;

namespace IronPythonTest {
#if !SILVERLIGHT
    class Common {
        public static string RootDirectory;
        public static string RuntimeDirectory;
        public static string ScriptTestDirectory;
        public static string InputTestDirectory;

        static Common() {
            RuntimeDirectory = Path.GetDirectoryName(typeof(PythonEngine).Assembly.Location);
            RootDirectory = Environment.GetEnvironmentVariable("MERLIN_ROOT");
            if (RootDirectory != null) {
                ScriptTestDirectory = Path.Combine(RootDirectory, "Languages\\IronPython\\Tests");
            } else {
                RootDirectory = System.Reflection.Assembly.GetEntryAssembly().GetFiles()[0].Name;
                ScriptTestDirectory = Path.Combine(RootDirectory, "..\\..\\..\\Src\\Tests");
            }
            InputTestDirectory = Path.Combine(ScriptTestDirectory, "Inputs");
        }
    }
#endif

    public delegate int IntIntDelegate(int arg);
    public delegate string RefStrDelegate(ref string arg);
    public delegate int RefIntDelegate(ref int arg);
    public delegate T GenericDelegate<T, U, V>(U arg1, V arg2);

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

    public class EngineTest : MarshalByRefObject {

        PythonEngine pe = PythonEngine.CurrentEngine;

        public EngineTest() {
            // Load a script with all the utility functions that are required
            // pe.ExecuteFile(InputTestDirectory + "\\EngineTests.py");
        }

        // Used to test exception thrown in another domain can be shown correctly.
        public void Run(string script) {
            pe.Execute(script);
        }

        static readonly string clspartName = "clsPart";

        private static ScriptModule DefaultModule {
            get {
                // TODO: should Python engine work with IScriptModule?
                return (ScriptModule)ScriptDomainManager.CurrentManager.Host.DefaultModule;
            }
        }

        // Execute
        public void ScenarioExecute() {
            ClsPart clsPart = new ClsPart();
            IScriptModule default_module = ScriptDomainManager.CurrentManager.Host.DefaultModule;

            DefaultModule.SetVariable(clspartName, clsPart);

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
            DefaultModule.SetVariable(clspartName, new ClsPart());
            pe.Execute("if 0 != clsPart.Field: raise AssertionError('test failed')");

            // add cls method as event handler
            DefaultModule.SetVariable("clsMethod", new IntIntDelegate(Negate));
            pe.Execute("clsPart.Event += clsMethod");
            pe.Execute("a = clsPart.Method(2)");
            pe.Execute("if -2 != a: raise AssertionError('test failed')");

            // ===============================================

            // reset the same variable with integer
            DefaultModule.SetVariable(clspartName, 1);
            pe.Execute("if 1 != clsPart: raise AssertionError('test failed')");
            AreEqual((int)DefaultModule.LookupVariable(clspartName), 1);
        }

        public void ScenarioEvaluateInAnonymousEngineModule() {
            ScriptModule anonymousModule = pe.CreateModule("", ModuleOptions.None);
            ScriptModule anotherAnonymousModule = pe.CreateModule("", ModuleOptions.None);
            pe.Execute("x = 0");
            pe.Execute("x = 1", anonymousModule);

            anotherAnonymousModule.SetVariable("x", 2);

            // Ensure that the default EngineModule is not affected
            AreEqual(0, pe.EvaluateAs<int>("x"));
            AreEqual(0, (int)ScriptDomainManager.CurrentManager.Host.DefaultModule.LookupVariable("x"));
            // Ensure that the anonymous context has been updated as expected
            AreEqual(1, pe.EvaluateAs<int>("x", anonymousModule));
            AreEqual(1, (int)anonymousModule.LookupVariable("x"));
            // Ensure that other anonymous contexts are not affected
            AreEqual(2, pe.EvaluateAs<int>("x", anotherAnonymousModule));
            AreEqual(2, (int)anotherAnonymousModule.LookupVariable("x"));
        }

        public void ScenarioEvaluateInPublishedEngineModule() {
            ScriptModule publishedModule = pe.CreateModule("published_context_test", ModuleOptions.PublishModule);
            pe.Execute("x = 0");
            pe.Execute("x = 1", publishedModule);
            // Ensure that the default EngineModule is not affected
            AreEqual(0, pe.EvaluateAs<int>("x"));
            // Ensure that the published context has been updated as expected
            AreEqual(1, pe.EvaluateAs<int>("x", publishedModule));

            // Ensure that the published context is accessible from other contexts using sys.modules
            // TODO: do better:
            // pe.Import("sys", ScriptDomainManager.CurrentManager.DefaultModule);
            pe.Execute("import sys", ScriptDomainManager.CurrentManager.Host.DefaultModule);
            pe.Execute("x_from_published_context_test = sys.modules['published_context_test'].x");
            AreEqual(1, pe.EvaluateAs<int>("x_from_published_context_test"));
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
                    throw new UnboundNameException("Cannot assign to customSymbol");
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
                    throw new UnboundNameException("Cannot delete customSymbol");
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
                    if (key.Equals(customSymbol))
                        throw new UnboundNameException("Cannot assign to customSymbol");
                    dict[key] = value;
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
            ScriptModule customModule = pe.CreateModule("customContext", customGlobals, ModuleOptions.PublishModule);
            PythonContext customContext = new PythonContext(pe, new PythonModuleContext());

            // Evaluate
            AreEqual(pe.EvaluateAs<int>("customSymbol + 1", customModule), CustomDictionary.customSymbolValue + 1);

            // Execute
            pe.Execute("customSymbolPlusOne = customSymbol + 1", customModule);
            AreEqual(pe.EvaluateAs<int>("customSymbolPlusOne", customModule), CustomDictionary.customSymbolValue + 1);
            AreEqual((int)customModule.Scope.LookupName(customContext, SymbolTable.StringToId("customSymbolPlusOne")), CustomDictionary.customSymbolValue + 1);

            // Compile
            ICompiledCode compiledCode = pe.CompileCode("customSymbolPlusTwo = customSymbol + 2");

            compiledCode.Execute(customModule);
            AreEqual(pe.EvaluateAs<int>("customSymbolPlusTwo", customModule), CustomDictionary.customSymbolValue + 2);
            AreEqual((int)customModule.Scope.LookupName(customContext, SymbolTable.StringToId("customSymbolPlusTwo")), CustomDictionary.customSymbolValue + 2);

            // check overriding of Add
            try {
                pe.Execute("customSymbol = 1", customModule);
                throw new Exception("We should not reach here");
            } catch (UnboundNameException) { }

            try {
                pe.Execute(@"global customSymbol
customSymbol = 1", customModule);
                throw new Exception("We should not reach here");
            } catch (UnboundNameException) { }

            // check overriding of Remove
            try {
                pe.Execute("del customSymbol", customModule);
                throw new Exception("We should not reach here");
            } catch (UnboundNameException) { }

            try {
                pe.Execute(@"global customSymbol
del customSymbol", customModule);
                throw new Exception("We should not reach here");
            } catch (UnboundNameException) { }

            // vars()
            IDictionary vars = pe.EvaluateAs<IDictionary>("vars()", customModule);
            AreEqual(true, vars.Contains("customSymbol"));

            // Miscellaneous APIs
            //IntIntDelegate d = pe.CreateLambda<IntIntDelegate>("customSymbol + arg", customModule);
            //AreEqual(d(1), CustomDictionary.customSymbolValue + 1);
        }

        public void ScenarioModuleWithLocals() {
            // Ensure that the namespace is not already polluted
            if (DefaultModule.VariableExists("global_variable"))
                DefaultModule.RemoveVariable("global_variable");
            if (DefaultModule.VariableExists("local_variable"))
                DefaultModule.RemoveVariable("local_variable");

            // Simple use of locals
            Dictionary<string, object> locals = new Dictionary<string, object>();
            locals["local_variable"] = 100;
            AreEqual(100, pe.EvaluateAs<int>("local_variable", DefaultModule, locals));

            // Ensure simple writes go to locals, not globals
            pe.Execute("local_variable = 200", DefaultModule, locals);
            AreEqual(200, pe.EvaluateAs<int>("local_variable", DefaultModule, locals));
            AreEqual(200, (int)locals["local_variable"]);
            if (DefaultModule.VariableExists("local_variable"))
                throw new Exception("local_variable is set in the Globals dictionary");
            try {
                pe.Evaluate("local_variable", DefaultModule, null);
            } catch (UnboundNameException) { }

            // Ensure writes to globals go to globals, not locals
            pe.Execute(@"global global_variable
global_variable = 300", DefaultModule, locals);
            AreEqual(300, pe.EvaluateAs<int>("global_variable", DefaultModule, locals));
            AreEqual(300, (int)DefaultModule.LookupVariable("global_variable"));
            if (locals.ContainsKey("global_variable"))
                throw new Exception("global_variable is set in the locals dictionary");

            // vars()
            IDictionary vars = pe.EvaluateAs<IDictionary>("vars()", DefaultModule, locals);
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

#if !SILVERLIGHT
        void CreateModules() {
            for (int contextCount = 0; contextCount < 100; contextCount++) {
                ScriptModule module = pe.CreateModule(scenarioGCModuleName, ModuleOptions.PublishModule);
                module.SetVariable("x", "Hello");
                pe.ExecuteFileContent(Common.InputTestDirectory + "\\simpleCommand.py", module);
                AreEqual(pe.EvaluateAs<int>("x", module), 1);
            }
        }

        public void ScenarioXGC() {
            long initialMemory = GetTotalMemory();

            // Create multiple engines and contexts
            for (int engineCount = 0; engineCount < 100; engineCount++) {
                CreateModules();
            }

            // Create multiple contexts in an engine that is not collected
            CreateModules();
            SystemState.Instance.modules.Remove(scenarioGCModuleName);

            long finalMemory = GetTotalMemory();
            long memoryUsed = finalMemory - initialMemory;
            const long memoryThreshold = 100000;
            if (memoryUsed > memoryThreshold)
                throw new Exception(String.Format("ScenarioGC used {0} bytes of memory. The threshold is {1} bytes", memoryUsed, memoryThreshold));
        }
#endif

        // Evaluate
        public void ScenarioEvaluate() {
            AreEqual(10, (int)pe.Evaluate("4+6"));
            AreEqual(10, pe.EvaluateAs<int>("4+6"));

            AreEqual("abab", (string)pe.Evaluate("'ab' * 2"));
            AreEqual("abab", pe.EvaluateAs<string>("'ab' * 2"));

            ClsPart clsPart = new ClsPart();
            DefaultModule.SetVariable(clspartName, clsPart);
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
            SourceUnit tempFile1, tempFile2;

            using (StringWriter sw = new StringWriter()) {
                sw.WriteLine("var1 = (10, 'z')");
                sw.WriteLine("");
                sw.WriteLine("clsPart.Field = 100");
                sw.WriteLine("clsPart.Property = clsPart.Field * 5");
                sw.WriteLine("clsPart.Event += (lambda x: x*x)");

                tempFile1 = new SourceFileUnit(pe, "", "", sw.ToString());
            }

            ClsPart clsPart = new ClsPart();
            DefaultModule.SetVariable(clspartName, clsPart);
            tempFile1.Compile().Execute();

            using (StringWriter sw = new StringWriter()) {
                sw.WriteLine("if var1[0] != 10: raise AssertionError('test failed')");
                sw.WriteLine("if var1[1] != 'z': raise AssertionError('test failed')");
                sw.WriteLine("");
                sw.WriteLine("if clsPart.Property != clsPart.Field * 5: raise AssertionError('test failed')");
                sw.WriteLine("var2 = clsPart.Method(var1[0])");
                sw.WriteLine("if var2 != 10 * 10: raise AssertionError('test failed')");

                tempFile2 = new SourceFileUnit(pe, "", "", sw.ToString());
            }

            tempFile2.Compile().Execute();
        }

        // Bug: 542
        public void Scenario542() {
            SourceUnit tempFile1;

            using (StringWriter sw = new StringWriter()) {
                sw.WriteLine("def M1(): return -1");
                sw.WriteLine("def M2(): return +1");

                sw.WriteLine("class C:");
                sw.WriteLine("    def M1(self): return -1");
                sw.WriteLine("    def M2(self): return +1");

                sw.WriteLine("class C1:");
                sw.WriteLine("    def M(): return -1");
                sw.WriteLine("class C2:");
                sw.WriteLine("    def M(): return +1");

                tempFile1 = new SourceFileUnit(pe, "", "", sw.ToString());
            }

            tempFile1.Compile().Execute();

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
        }

        // Bug: 167 
        public void Scenario167() {
            pe.Execute("a=1\r\nb=-1");
            AreEqual(1, pe.EvaluateAs<int>("a"));
            AreEqual(-1, pe.EvaluateAs<int>("b"));
        }
#if !SILVERLIGHT
        // AddToPath

        public void ScenarioAddToPath() { // runs first to avoid path-order issues
            string ipc_dll = typeof(PythonEngine).Assembly.Location;
            string ipc_path = Path.GetDirectoryName(ipc_dll);
            pe.InitializeModules(ipc_path, ipc_path + "\\ipy.exe", pe.VersionString);
            string tempFile1 = Path.GetTempFileName();

            try {
                File.WriteAllText(tempFile1, "from lib.does_not_exist import *");

                try {
                    pe.ExecuteFileContent(tempFile1);
                    throw new Exception("Scenario7");
                } catch (IronPython.Runtime.Exceptions.PythonImportErrorException) { }

                File.WriteAllText(tempFile1, "from lib.assert_util import *");

                pe.AddToPath(Common.ScriptTestDirectory);

                pe.ExecuteFileContent(tempFile1);
                pe.Execute("AreEqual(5, eval('2 + 3'))");
            } finally {
                File.Delete(tempFile1);
            }
        }

        // Options.ClrDebuggingEnabled
#endif
        delegate void ThrowExceptionDelegate();
        static void ThrowException() {
            throw new Exception("Exception from ThrowException");
        }

#if !SILVERLIGHT
        public void ScenarioClrDebuggingEnabled() {
            const string lineNumber = "raise.py:line";

            try {
                SystemState.Instance.Engine.Options.ClrDebuggingEnabled = true;
                try {
                    pe.ExecuteFileContent(Common.InputTestDirectory + "\\raise.py");
                    throw new Exception("We should not get here");
                } catch (IronPython.Runtime.Exceptions.StringException e1) {
                    if (!e1.StackTrace.Contains(lineNumber))
                        throw new Exception("Debugging is not enabled even though Options.ClrDebuggingEnabled is specified");
                }

                SystemState.Instance.Engine.Options.ClrDebuggingEnabled = false;
                try {
                    pe.ExecuteFileContent(Common.InputTestDirectory + "\\raise.py");
                    throw new Exception("We should not get here");
                } catch (StringException e2) {
                    if (e2.StackTrace.Contains(lineNumber))
                        throw new Exception("Debugging is enabled even though Options.ClrDebuggingEnabled is not specified");
                }

                // Ensure that all APIs work
                AreEqual(pe.EvaluateAs<int>("x"), 1);
                //IntIntDelegate d = pe.CreateLambda<IntIntDelegate>("arg + x");
                //AreEqual(d(100), 101);
                //d = pe.CreateMethod<IntIntDelegate>("var = arg + x\nreturn var");
                //AreEqual(d(100), 101);
            } finally {
                SystemState.Instance.Engine.Options.ClrDebuggingEnabled = false;
            }
        }

        public void ScenarioExecuteFileOptimized() {
            ScriptModule module = pe.CreateOptimizedModule(Common.InputTestDirectory + "\\simpleCommand.py", "__main__", false);
            module.Execute();

            AreEqual(1, pe.EvaluateAs<int>("x", module));

            // Ensure that we can set new globals in the context, and execute further code
            module.SetVariable("y", 2);
            AreEqual(3, pe.EvaluateAs<int>("x+y", module));
        }

        public void ScenarioOptimizedModuleBeforeExecutingGlobalCode() {
            ScriptModule module = pe.CreateOptimizedModule(Common.InputTestDirectory + "\\uninitializedGlobal.py", "__main__", true);

            AreEqual(module.Scope.ContainsName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("x")), false);
            AreEqual(module.Scope.ContainsName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("y")), false);

            // Set a global variable before executing global code
            module.SetVariable("x", 1);
            module.Execute();

            // Ensure that the global code picked up the value of "x" that was pumped into the module
            AreEqual(101, pe.EvaluateAs<int>("x", module));
            AreEqual(2, pe.EvaluateAs<int>("y", module));
        }

        public void ScenarioCreateOptimizedModule_ScriptThrows() {
            // We should be able to use the EngineModule even if an exception is thrown by the script
            ScriptModule module = pe.CreateOptimizedModule(Common.InputTestDirectory + "\\raise.py", "__main__", true);

            try {
                module.Execute();
                throw new Exception("We should not reach here");
            } catch (StringException) {
                AreEqual(1, pe.EvaluateAs<int>("x", module));

                // Ensure that we can set new globals in the context, and execute further code
                module.SetVariable("y", 2);
                AreEqual(3, pe.EvaluateAs<int>("x+y", module));
            }
        }
#endif
        // Compile and Run
        public void ScenarioCompileAndRun() {
            ClsPart clsPart = new ClsPart();

            DefaultModule.SetVariable(clspartName, clsPart);
            ICompiledCode compiledCode = pe.CompileCode("def f(): clsPart.Field += 10");
            compiledCode.Execute();

            compiledCode = pe.CompileCode("f()");
            compiledCode.Execute();
            AreEqual(10, clsPart.Field);
            compiledCode.Execute();
            AreEqual(20, clsPart.Field);
        }
#if FALSE
        public void ScenarioStreamRedirect() {
            MemoryStream stdout = new MemoryStream();
            MemoryStream stdin = new MemoryStream();
            MemoryStream stderr = new MemoryStream();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] buffer = new byte[15];

            pe.SetStandardError(stderr);
            pe.SetStandardInput(stdin);
            pe.SetStandardOutput(stdout);
            pe.Import("sys");

            stdin.Write(encoding.GetBytes("This is stdout"), 0, 14);
            stdin.Position = 0;
            pe.Execute("output = sys.__stdin__.readline()");
            AreEqual("This is stdout", pe.EvaluateAs<string>("output"));
            pe.Execute("sys.__stdout__.write(output)");
            stdout.Flush();
            stdout.Position = 0;
            int len = stdout.Read(buffer, 0, 14);
            AreEqual(14, len);
            AreEqual("This is stdout", encoding.GetString(buffer, 0, len));

            pe.Execute("sys.__stderr__.write(\"This is stderr\")");
            stderr.Flush();
            stderr.Position = 0;
            len = stderr.Read(buffer, 0, 14);
            AreEqual(14, len);
            AreEqual("This is stderr", encoding.GetString(buffer, 0, len));
        }
#endif
#if FALSE

        public void ScenarioCreateMethodAndLambda() {
            //Generic delegate types
            GenericDelegate<int, int, int> del = pe.CreateLambda<GenericDelegate<int, int, int>>("arg1+arg2");
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
            List<string> args = new List<string>(new string[] { "dir", "secondarg" });
            del = pe.CreateLambda<GenericDelegate<int, int, int>>("dir+secondarg", args);
            AreEqual(6, del(3, 3));
            del = pe.CreateMethod<GenericDelegate<int, int, int>>("return dir+secondarg", args);
            AreEqual(6, del(3, 3));
        }

        public void ScenarioCoverage() {
            ScriptCode compiledCode = ScriptDomainManager.CurrentManager.CompileCode(
                new SourceCodeUnit(pe, "arg1+arg2", "MyNewSourceFile"), pe.GetDefaultCompilerOptions(), new PythonErrorSink(true));
            AreEqual("ScriptCode MyNewSourceFile from Python", compiledCode.ToString());
        }

        public void ScenarioNullArguments() {
            try {
                pe.CompileCode((string)null);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }

            try {
                pe.CreateLambda<IntIntDelegate>("x=2", null, null);
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
                pe.CreateModule(null, new Dictionary<string, object>(), ModuleOptions.None);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }

            try {
                pe.CreateModule("moduleName", null, ModuleOptions.None);
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
                pe.CreateOptimizedModule("some_path", null, false);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }
        }
#endif

        private static void ExecuteAndVerify(PythonEngine pe, ScriptModule module, IDictionary<string, object> locals, string text, string expectedText) {
            MemoryStream stream = new MemoryStream();
            object old_out = SystemState.Instance.stdout;
            SystemState.Instance.stdout = new PythonFile(stream, Encoding.Default, "<stdout>", "w");
            try {
                pe.Execute(text, module, locals);
            } finally {
                SystemState.Instance.stdout = old_out;
            }

            byte[] array = stream.ToArray();
            AreEqual(Encoding.Default.GetString(array, 0, array.Length), expectedText);
        }

        public void ScenarioVariableScoping() {
            Dictionary<string, object> globals = new Dictionary<string, object>();
            Dictionary<string, object> locals = new Dictionary<string, object>();
            ScriptModule module1 = pe.CreateModule("module1", globals, ModuleOptions.PublishModule);

            //Set variables in each context
            globals["x"] = 1;

            SourceFileUnit tempFile1 = new SourceFileUnit(pe, "", "MyVirtualFile", "y = 2");
            pe.Execute(tempFile1, module1, locals);

            locals["z"] = 3;

            //Query for a variable from each context
            AreEqual((int)pe.Evaluate("x", module1, locals), 1);
            AreEqual((int)pe.Evaluate("y", module1, locals), 2);
            AreEqual((int)pe.Evaluate("z", module1, locals), 3);

            AreEqual(pe.EvaluateAs<int>("x", module1, locals), 1);
            AreEqual(pe.EvaluateAs<int>("y", module1, locals), 2);
            AreEqual(pe.EvaluateAs<int>("z", module1, locals), 3);

            ExecuteAndVerify(pe, module1, locals, "print x", "1" + Environment.NewLine);
            ExecuteAndVerify(pe, module1, locals, "print y", "2" + Environment.NewLine);
            ExecuteAndVerify(pe, module1, locals, "print z", "3" + Environment.NewLine);

            //Set conflicting variables in each context and verify resolution
            module1.SetVariable("context", "module");
            AreEqual("module", pe.EvaluateAs<string>("context", module1, locals));
            locals["context"] = "local";
            AreEqual("local", pe.EvaluateAs<string>("context", module1, locals));
            pe.Execute("context='also local'", module1, locals);
            AreEqual("also local", pe.EvaluateAs<string>("context", module1, locals));
            AreEqual("also local", (string)locals["context"]);

            //Validate that the module dict hasn't changed in an unexpected way
            AreEqual(false, module1.Scope.ContainsName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("y")));
            AreEqual(false, module1.Scope.ContainsName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("z")));

            //Remove the conflicting variables from each context and verify resolution
            pe.Execute("del(context)", module1, locals);
            AreEqual(false, locals.ContainsKey("context"));
            AreEqual("module", pe.EvaluateAs<string>("context", module1, locals));
            module1.Scope.GlobalScope.RemoveName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("context"));
            try {
                AreEqual("module", pe.EvaluateAs<string>("context", module1, locals));
                Console.WriteLine("All references to context have been removed, we should get an error when looking up this name");
                throw new Exception();
            } catch (UnboundNameException) {
                //Pass
            }

            //Change an existing variable in each context
            locals["z"] = 6;
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

            //Hide a builtin from each context and verify we can recover it
            globals["dir"] = "overriden in module";
            AreEqual("overriden in module", pe.EvaluateAs<string>("dir", module1, locals));
            globals.Remove("dir");
            AreEqual(true, pe.Evaluate("dir", module1, locals) is BuiltinFunction);

            locals["dir"] = "overriden in locals";
            AreEqual("overriden in locals", pe.EvaluateAs<string>("dir", module1, locals));
            locals.Remove("dir");
            AreEqual(true, pe.Evaluate("dir", module1, locals) is BuiltinFunction);
        }

        public void Scenario12() {

            pe.Execute(@"
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
");
        }

        public void ScenarioTrueDivision1() {
            TestOldDivision(pe, DefaultModule);
            ScriptModule module = pe.CreateModule("anonymous", ModuleOptions.TrueDivision);
            TestNewDivision(pe, module);
        }

        public void ScenarioTrueDivision2() {
            TestOldDivision(pe, DefaultModule);
            ScriptModule module = pe.CreateModule("__future__", ModuleOptions.PublishModule);
            module.SetVariable("division", 1);
            pe.Execute("from __future__ import division", module);
            TestNewDivision(pe, module);
        }

        public void ScenarioTrueDivision3() {
            TestOldDivision(pe, DefaultModule);
            ScriptModule future = pe.CreateModule("__future__", ModuleOptions.PublishModule);
            future.SetVariable("division", 1);
            ScriptModule td = pe.CreateModule("truediv", ModuleOptions.None);
            ScriptCode cc = ScriptCode.FromCompiledCode((CompiledCode)pe.CompileCode("from __future__ import division"));
            cc.Run(td);
            TestNewDivision(pe, td);  // we've polluted the DefaultModule by executing the code
        }
#if !SILVERLIGHT
        public void ScenarioTrueDivision4() {
            pe.AddToPath(Common.ScriptTestDirectory);

            string modName = GetTemporaryModuleName();
            string file = System.IO.Path.Combine(Common.ScriptTestDirectory, modName + ".py");
            System.IO.File.WriteAllText(file, "result = 1/2");

            DivisionOption old = ScriptDomainManager.Options.Division;

            try {
                ScriptDomainManager.Options.Division = DivisionOption.Old;
                ScriptModule module = pe.CreateModule("anonymous", ModuleOptions.TrueDivision);
                pe.Execute("import " + modName, module);
                int res = pe.EvaluateAs<int>(modName + ".result", module);
                AreEqual(res, 0);
            } finally {
                ScriptDomainManager.Options.Division = old;
                try {
                    System.IO.File.Delete(file);
                } catch { }
            }
        }

        private string GetTemporaryModuleName() {
            return "tempmod" + Path.GetRandomFileName().Replace('-', '_').Replace('.', '_');
        }

        public void ScenarioTrueDivision5() {
            pe.AddToPath(Common.ScriptTestDirectory);

            string modName = GetTemporaryModuleName();
            string file = System.IO.Path.Combine(Common.ScriptTestDirectory, modName + ".py");
            System.IO.File.WriteAllText(file, "from __future__ import division; result = 1/2");

            try {
                ScriptModule module = ScriptDomainManager.CurrentManager.CreateModule(modName);
                pe.Execute("import " + modName, module);
                double res = pe.EvaluateAs<double>(modName + ".result", module);
                AreEqual(res, 0.5);
                AreEqual((bool)((PythonContext)DefaultContext.Default.LanguageContext).TrueDivision, false);
            } finally {
                try {
                    System.IO.File.Delete(file);
                } catch { }
            }
        }
#endif
        private static void TestOldDivision(ScriptEngine pe, ScriptModule module) {
            pe.Execute("result = 1/2", module);
            AreEqual((int)module.Scope.LookupName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("result")), 0);
            AreEqual(pe.EvaluateAs<int>("1/2", module), 0);
            pe.Execute("exec 'result = 3/2'", module);
            AreEqual((int)module.Scope.LookupName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("result")), 1);
            AreEqual(pe.EvaluateAs<int>("eval('3/2')", module), 1);
        }

        private static void TestNewDivision(ScriptEngine pe, ScriptModule module) {
            pe.Execute("result = 1/2", module);
            AreEqual((double)module.Scope.LookupName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("result")), 0.5);
            AreEqual(pe.EvaluateAs<double>("1/2", module), 0.5);
            pe.Execute("exec 'result = 3/2'", module);
            AreEqual((double)module.Scope.LookupName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("result")), 1.5);
            AreEqual(pe.EvaluateAs<double>("eval('3/2')", module), 1.5);
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
