/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
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
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython;
using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

namespace IronPythonTest {
#if !SILVERLIGHT
    class Common {
        public static string RootDirectory;
        public static string RuntimeDirectory;
        public static string ScriptTestDirectory;
        public static string InputTestDirectory;

        static Common() {
            RuntimeDirectory = Path.GetDirectoryName(typeof(PythonContext).Assembly.Location);
            RootDirectory = Environment.GetEnvironmentVariable("MERLIN_ROOT");
            if (RootDirectory != null) {
                ScriptTestDirectory = Path.Combine(RootDirectory, "Languages\\IronPython\\Tests");
            } else {
                RootDirectory = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().GetFiles()[0].Name).Directory.FullName;
                ScriptTestDirectory = Path.Combine(RootDirectory, "Src\\Tests");
            }
            InputTestDirectory = Path.Combine(ScriptTestDirectory, "Inputs");
        }
    }
#endif

    public static class TestHelpers {
        public static LanguageContext GetContext(CodeContext context) {
            return context.LanguageContext;
        }
    }

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
        private static int privateStaticMethod() {
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

    public class EngineTest
#if !SILVERLIGHT // remoting not supported in Silverlight
        : MarshalByRefObject
#endif
    {

        private readonly IScriptEngine _pe;
        private readonly IScriptEnvironment _env;

        public EngineTest() {
            // Load a script with all the utility functions that are required
            // pe.ExecuteFile(InputTestDirectory + "\\EngineTests.py");
            _env = ScriptEnvironment.Create(null);
            _env.LoadAssembly(typeof(string).Assembly);
            _env.LoadAssembly(typeof(Debug).Assembly);
            _pe = _env.GetEngine("py");
        }

        // Used to test exception thrown in another domain can be shown correctly.
        public void Run(string script) {
            IScriptScope scope = _env.CreateScope();
            _pe.Execute(scope, _pe.CreateScriptSourceFromString(script, SourceCodeKind.File));
        }

        static readonly string clspartName = "clsPart";
        
        /// <summary>
        /// Asserts an condition it true
        /// </summary>
        private static void Assert(bool condition, string msg) {
            if (!condition) throw new Exception(String.Format("Assertion failed: {0}", msg));
        }

        private static void Assert(bool condition) {
            if (!condition) throw new Exception("Assertion failed");
        }

        private static T AssertExceptionThrown<T>(Action f) where T : Exception {
            try {
                f();
            } catch (T ex) {
                return ex;
            }

            Assert(false, "Expecting exception '" + typeof(T) + "'.");
            return null;
        }

        // Execute
        public void ScenarioExecute() {
            ClsPart clsPart = new ClsPart();

            IScriptScope scope = _env.CreateScope();

            scope.SetVariable(clspartName, clsPart);

            // field: assign and get back
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("clsPart.Field = 100", SourceCodeKind.Statements));
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("if 100 != clsPart.Field: raise AssertionError('test failed')", SourceCodeKind.Statements));
            AreEqual(100, clsPart.Field);

            // property: assign and get back
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("clsPart.Property = clsPart.Field", SourceCodeKind.Statements));
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("if 100 != clsPart.Property: raise AssertionError('test failed')", SourceCodeKind.Statements));
            AreEqual(100, clsPart.Property);

            // method: Event not set yet
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("a = clsPart.Method(2)", SourceCodeKind.Statements));
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("if -1 != a: raise AssertionError('test failed')", SourceCodeKind.Statements));

            // method: add python func as event handler
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("def f(x) : return x * x", SourceCodeKind.Statements));
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("clsPart.Event += f", SourceCodeKind.Statements));
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("a = clsPart.Method(2)", SourceCodeKind.Statements));
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("if 4 != a: raise AssertionError('test failed')", SourceCodeKind.Statements));

            // ===============================================

            // reset the same variable with instance of the same type
            scope.SetVariable(clspartName, new ClsPart());
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("if 0 != clsPart.Field: raise AssertionError('test failed')", SourceCodeKind.Statements));

            // add cls method as event handler
            scope.SetVariable("clsMethod", new IntIntDelegate(Negate));
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("clsPart.Event += clsMethod", SourceCodeKind.Statements));
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("a = clsPart.Method(2)", SourceCodeKind.Statements));
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("if -2 != a: raise AssertionError('test failed')", SourceCodeKind.Statements));

            // ===============================================

            // reset the same variable with integer
            scope.SetVariable(clspartName, 1);
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("if 1 != clsPart: raise AssertionError('test failed')", SourceCodeKind.Statements));
            AreEqual((int)scope.LookupVariable(clspartName), 1);

            AssertExceptionThrown<ArgumentNullException>(delegate() {
                _pe.Execute(null, null);
            });

            SourceUnit su = _pe.CreateScriptSourceFromString("");
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                _pe.Execute(null, su);
            });
        }

        public void ScenarioEvaluateInAnonymousEngineModule() {
            IScriptScope scope1 = _env.CreateScope();
            IScriptScope scope2 = _env.CreateScope();
            IScriptScope scope3 = _env.CreateScope();

            _pe.Execute(scope1, _pe.CreateScriptSourceFromString("x = 0", SourceCodeKind.Statements));
            _pe.Execute(scope2, _pe.CreateScriptSourceFromString("x = 1", SourceCodeKind.Statements));

            scope3.SetVariable("x", 2);

            AreEqual(0, _pe.Execute<int>(scope1, _pe.CreateScriptSourceFromString("x")));
            AreEqual(0, (int)scope1.LookupVariable("x"));

            AreEqual(1, _pe.Execute<int>(scope2, _pe.CreateScriptSourceFromString("x")));
            AreEqual(1, (int)scope2.LookupVariable("x"));

            AreEqual(2, _pe.Execute<int>(scope3, _pe.CreateScriptSourceFromString("x")));
            AreEqual(2, (int)scope3.LookupVariable("x"));
        }

        public void ScenarioEvaluateInPublishedEngineModule() {
            PythonContext pc = DefaultContext.DefaultPythonContext;

            PythonModule publishedModule = pc.CreateModule("published_context_test");
            PythonModule otherModule = pc.CreateModule("published_context_test");
            pc.PublishModule("published_context_test", publishedModule);

            pc.CompileSourceCode(_pe.CreateScriptSourceFromString("x = 0", SourceCodeKind.Statements)).Run(otherModule.Scope);
            pc.CompileSourceCode(_pe.CreateScriptSourceFromString("x = 1", SourceCodeKind.Statements)).Run(publishedModule.Scope);

            // Ensure that the default EngineModule is not affected
            AreEqual(0, (int)pc.CompileSourceCode(_pe.CreateScriptSourceFromString("x")).Run(otherModule.Scope));
            // Ensure that the published context has been updated as expected
            AreEqual(1, (int)pc.CompileSourceCode(_pe.CreateScriptSourceFromString("x")).Run(publishedModule.Scope));

            // Ensure that the published context is accessible from other contexts using sys.modules
            // TODO: do better:
            // pe.Import("sys", ScriptDomainManager.CurrentManager.DefaultModule);
            pc.CompileSourceCode(_pe.CreateScriptSourceFromString("from published_context_test import x", SourceCodeKind.Statements)).Run(otherModule.Scope);
            AreEqual(1, (int)pc.CompileSourceCode(_pe.CreateScriptSourceFromString("x")).Run(otherModule.Scope));
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
            IAttributesCollection customGlobals = new StringDictionaryAdapterDict(new CustomDictionary());
            
            IScriptScope customModule = _pe.Runtime.CreateScope(customGlobals);            

            // Evaluate
            AreEqual(_pe.Execute<int>(customModule, _pe.CreateScriptSourceFromString("customSymbol + 1")), CustomDictionary.customSymbolValue + 1);

            // Execute
            _pe.Execute(customModule, _pe.CreateScriptSourceFromString("customSymbolPlusOne = customSymbol + 1", SourceCodeKind.Statements));
            AreEqual(_pe.Execute<int>(customModule, _pe.CreateScriptSourceFromString("customSymbolPlusOne")), CustomDictionary.customSymbolValue + 1);
            AreEqual(_pe.GetVariable<int>(customModule, "customSymbolPlusOne"), CustomDictionary.customSymbolValue + 1);

            // Compile
            ICompiledCode compiledCode = _pe.Compile(_pe.CreateScriptSourceFromString("customSymbolPlusTwo = customSymbol + 2", SourceCodeKind.Statements));

            compiledCode.Execute(customModule);
            AreEqual(_pe.Execute<int>(customModule, _pe.CreateScriptSourceFromString("customSymbolPlusTwo")), CustomDictionary.customSymbolValue + 2);
            AreEqual(_pe.GetVariable<int>(customModule, "customSymbolPlusTwo"), CustomDictionary.customSymbolValue + 2);

            // check overriding of Add
            try {
                _pe.Execute(customModule, _pe.CreateScriptSourceFromString("customSymbol = 1", SourceCodeKind.Statements));
                throw new Exception("We should not reach here");
            } catch (UnboundNameException) { }

            try {
                _pe.Execute(customModule, _pe.CreateScriptSourceFromString(@"global customSymbol
customSymbol = 1", SourceCodeKind.Statements));
                throw new Exception("We should not reach here");
            } catch (UnboundNameException) { }

            // check overriding of Remove
            try {
                _pe.Execute(customModule, _pe.CreateScriptSourceFromString("del customSymbol", SourceCodeKind.Statements));
                throw new Exception("We should not reach here");
            } catch (UnboundNameException) { }

            try {
                _pe.Execute(customModule, _pe.CreateScriptSourceFromString(@"global customSymbol
del customSymbol", SourceCodeKind.Statements));
                throw new Exception("We should not reach here");
            } catch (UnboundNameException) { }

            // vars()
            IDictionary vars = _pe.Execute<IDictionary>(customModule, _pe.CreateScriptSourceFromString("vars()"));
            AreEqual(true, vars.Contains("customSymbol"));

            // Miscellaneous APIs
            //IntIntDelegate d = pe.CreateLambda<IntIntDelegate>("customSymbol + arg", customModule);
            //AreEqual(d(1), CustomDictionary.customSymbolValue + 1);
        }

        static long GetTotalMemory() {
            // Critical objects can take upto 3 GCs to be collected
            for (int i = 0; i < 3; i++) {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return GC.GetTotalMemory(true);
        }

#if !SILVERLIGHT
        public void ScenarioXGC() {
            long initialMemory = GetTotalMemory();

            // Create multiple scopes:
            for (int i = 0; i < 10000; i++) {
                IScriptScope scope = _pe.CreateScope();
                scope.SetVariable("x", "Hello");
                _pe.Execute(scope, _pe.CreateScriptSourceFromFile(Common.InputTestDirectory + "\\simpleCommand.py"));
                AreEqual(_pe.Execute<int>(scope, _pe.CreateScriptSourceFromString("x")), 1);
            }

            long finalMemory = GetTotalMemory();
            long memoryUsed = finalMemory - initialMemory;
            const long memoryThreshold = 100000;

            if (!_env.GlobalOptions.EmitsUncollectableCode) {
                if (memoryUsed > memoryThreshold)
                    throw new Exception(String.Format("ScenarioGC used {0} bytes of memory. The threshold is {1} bytes", memoryUsed, memoryThreshold));
            }
        }
#endif

        // Evaluate
        public void ScenarioEvaluate() {
            IScriptScope scope = _env.CreateScope();
            AreEqual(10, (int)_pe.Execute(scope, _pe.CreateScriptSourceFromString("4+6")));
            AreEqual(10, _pe.Execute<int>(scope, _pe.CreateScriptSourceFromString("4+6")));

            AreEqual("abab", (string)_pe.Execute(scope, _pe.CreateScriptSourceFromString("'ab' * 2")));
            AreEqual("abab", _pe.Execute<string>(scope, _pe.CreateScriptSourceFromString("'ab' * 2")));

            ClsPart clsPart = new ClsPart();
            scope.SetVariable(clspartName, clsPart);
            AreEqual(clsPart, _pe.Execute(scope, _pe.CreateScriptSourceFromString("clsPart")) as ClsPart);
            AreEqual(clsPart, _pe.Execute<ClsPart>(scope, _pe.CreateScriptSourceFromString("clsPart")));

            _pe.Execute(scope, _pe.CreateScriptSourceFromString("clsPart.Field = 100", SourceCodeKind.Statements));
            AreEqual(100, (int)_pe.Execute(scope, _pe.CreateScriptSourceFromString("clsPart.Field")));
            AreEqual(100, _pe.Execute<int>(scope, _pe.CreateScriptSourceFromString("clsPart.Field")));

            // Ensure that we can get back a delegate to a Python method
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("def IntIntMethod(a): return a * 100", SourceCodeKind.Statements));
            IntIntDelegate d = _pe.Execute<IntIntDelegate>(scope, _pe.CreateScriptSourceFromString("IntIntMethod"));
            AreEqual(d(2), 2 * 100);
        }

#if !SILVERLIGHT
        // ExecuteFile
        public void ScenarioExecuteFile() {
            SourceUnit tempFile1, tempFile2;

            IScriptScope scope = _env.CreateScope();

            using (StringWriter sw = new StringWriter()) {
                sw.WriteLine("var1 = (10, 'z')");
                sw.WriteLine("");
                sw.WriteLine("clsPart.Field = 100");
                sw.WriteLine("clsPart.Property = clsPart.Field * 5");
                sw.WriteLine("clsPart.Event += (lambda x: x*x)");

                tempFile1 = _pe.CreateScriptSourceFromString(sw.ToString(), SourceCodeKind.File);
            }

            ClsPart clsPart = new ClsPart();
            scope.SetVariable(clspartName, clsPart);
            _pe.Execute(scope, tempFile1);

            using (StringWriter sw = new StringWriter()) {
                sw.WriteLine("if var1[0] != 10: raise AssertionError('test failed')");
                sw.WriteLine("if var1[1] != 'z': raise AssertionError('test failed')");
                sw.WriteLine("");
                sw.WriteLine("if clsPart.Property != clsPart.Field * 5: raise AssertionError('test failed')");
                sw.WriteLine("var2 = clsPart.Method(var1[0])");
                sw.WriteLine("if var2 != 10 * 10: raise AssertionError('test failed')");

                tempFile2 = _pe.CreateScriptSourceFromString(sw.ToString(), SourceCodeKind.File);
            }

            _pe.Execute(scope, tempFile2); 
        }
#endif

#if !SILVERLIGHT
        // Bug: 542
        public void Scenario542() {
            SourceUnit tempFile1;

            IScriptScope scope = _env.CreateScope();

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

                tempFile1 = _pe.CreateScriptSourceFromString(sw.ToString(), SourceCodeKind.File);
            }

            _pe.Execute(scope, tempFile1);

            AreEqual(-1, _pe.Execute<int>(scope, _pe.CreateScriptSourceFromString("M1()")));
            AreEqual(+1, _pe.Execute<int>(scope, _pe.CreateScriptSourceFromString("M2()")));

            AreEqual(-1, (int)_pe.Execute(scope, _pe.CreateScriptSourceFromString("M1()")));
            AreEqual(+1, (int)_pe.Execute(scope, _pe.CreateScriptSourceFromString("M2()")));

            _pe.Execute(scope, _pe.CreateScriptSourceFromString("if M1() != -1: raise AssertionError('test failed')", SourceCodeKind.SingleStatement));
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("if M2() != +1: raise AssertionError('test failed')", SourceCodeKind.SingleStatement));


            _pe.Execute(scope, _pe.CreateScriptSourceFromString("c = C()", SourceCodeKind.SingleStatement));
            AreEqual(-1, _pe.Execute<int>(scope, _pe.CreateScriptSourceFromString("c.M1()")));
            AreEqual(+1, _pe.Execute<int>(scope, _pe.CreateScriptSourceFromString("c.M2()")));

            AreEqual(-1, (int)_pe.Execute(scope, _pe.CreateScriptSourceFromString("c.M1()")));
            AreEqual(+1, (int)_pe.Execute(scope, _pe.CreateScriptSourceFromString("c.M2()")));

            _pe.Execute(scope, _pe.CreateScriptSourceFromString("if c.M1() != -1: raise AssertionError('test failed')", SourceCodeKind.SingleStatement));
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("if c.M2() != +1: raise AssertionError('test failed')", SourceCodeKind.SingleStatement));


            //AreEqual(-1, pe.EvaluateAs<int>("C1.M()"));
            //AreEqual(+1, pe.EvaluateAs<int>("C2.M()"));

            //AreEqual(-1, (int)pe.Evaluate("C1.M()"));
            //AreEqual(+1, (int)pe.Evaluate("C2.M()"));

            //pe.Execute(pe.CreateScriptSourceFromString("if C1.M() != -1: raise AssertionError('test failed')");
            //pe.Execute(pe.CreateScriptSourceFromString("if C2.M() != +1: raise AssertionError('test failed')");
        }
#endif

        // Bug: 167 
        public void Scenario167() {
            IScriptScope scope = _env.CreateScope();
            _pe.Execute(scope, _pe.CreateScriptSourceFromString("a=1\r\nb=-1", SourceCodeKind.Statements));
            AreEqual(1, _pe.Execute<int>(scope, _pe.CreateScriptSourceFromString("a")));
            AreEqual(-1, _pe.Execute<int>(scope, _pe.CreateScriptSourceFromString("b")));
        }
#if !SILVERLIGHT
        // AddToPath

        public void ScenarioAddToPath() { // runs first to avoid path-order issues            
            string ipc_dll = typeof(PythonContext).Assembly.Location;
            string ipc_path = Path.GetDirectoryName(ipc_dll);
            //pe.InitializeModules(ipc_path, ipc_path + "\\ipy.exe", pe.VersionString);
            string tempFile1 = Path.GetTempFileName();

            try {
                File.WriteAllText(tempFile1, "from lib.does_not_exist import *");
                IScriptScope scope = _pe.Runtime.CreateScope();

                try {
                    _pe.Execute(scope, _pe.CreateScriptSourceFromFile(tempFile1));
                    throw new Exception("Scenario7");
                } catch (IronPython.Runtime.Exceptions.ImportException) { }

                File.WriteAllText(tempFile1, "from lib.assert_util import *");
                _pe.SetScriptSourceSearchPaths(new string[] { Common.ScriptTestDirectory });

                _pe.Execute(scope, _pe.CreateScriptSourceFromFile(tempFile1));
                _pe.Execute(scope, _pe.CreateScriptSourceFromString("AreEqual(5, eval('2 + 3'))", SourceCodeKind.Statements));
            } finally {
                File.Delete(tempFile1);
            }
        }

        // Options.DebugMode
#endif
        delegate void ThrowExceptionDelegate();
        static void ThrowException() {
            throw new Exception("Exception from ThrowException");
        }

#if !SILVERLIGHT
        public void ScenarioStackFrameLineInfo() {
            const string lineNumber = "raise.py:line";

            if (_pe.Options.InterpretedMode) {
                // Disable this test in interpreted mode, since in this case
                // the CLR stack traces do not include the python source file and line number.
                return;
            }

            bool oldDebug = ScriptDomainManager.Options.DebugMode;

            try {
                IScriptScope scope = _pe.Runtime.CreateScope();

                TestLineInfo(scope, lineNumber, false);
                TestLineInfo(scope, lineNumber, true);
                TestLineInfo(scope, lineNumber, false);

                // Ensure that all APIs work
                AreEqual(_pe.Execute<int>(scope, _pe.CreateScriptSourceFromString("x")), 1);
                //IntIntDelegate d = pe.CreateLambda<IntIntDelegate>("arg + x");
                //AreEqual(d(100), 101);
                //d = pe.CreateMethod<IntIntDelegate>("var = arg + x\nreturn var");
                //AreEqual(d(100), 101);
            } finally {
                ScriptDomainManager.Options.DebugMode = oldDebug;
            }
        }

        private void TestLineInfo(IScriptScope scope, string lineNumber, bool debuggable) {
            ScriptDomainManager.Options.DebugMode = debuggable;
            try {
                _pe.Execute(scope, _pe.CreateScriptSourceFromFile(Common.InputTestDirectory + "\\raise.py"));
                throw new Exception("We should not get here");
            } catch (StringException e2) {
                if (debuggable != e2.StackTrace.Contains(lineNumber))
                    throw new Exception("Debugging is enabled even though Options.DebugMode is not specified");
            }
        }

#endif

        // Compile and Run
        public void ScenarioCompileAndRun() {
            ClsPart clsPart = new ClsPart();

            IScriptScope scope = _env.CreateScope();

            scope.SetVariable(clspartName, clsPart);
            ICompiledCode compiledCode = _pe.Compile(_pe.CreateScriptSourceFromString("def f(): clsPart.Field += 10", SourceCodeKind.Statements));
            compiledCode.Execute(scope);

            compiledCode = _pe.Compile(_pe.CreateScriptSourceFromString("f()"));
            compiledCode.Execute(scope);
            AreEqual(10, clsPart.Field);
            compiledCode.Execute(scope);
            AreEqual(20, clsPart.Field);
        }

        public void ScenarioStreamRedirect() {
            MemoryStream stdout = new MemoryStream();
            MemoryStream stdin = new MemoryStream();
            MemoryStream stderr = new MemoryStream();
            Encoding encoding = Encoding.UTF8;

            byte[] buffer = new byte[50];

            _pe.Runtime.IO.SetInput(stdin, encoding);
            _pe.Runtime.IO.SetOutput(stdout, encoding);
            _pe.Runtime.IO.SetErrorOutput(stderr, encoding);
 
            const string str = "This is stdout";
            byte[] bytes = encoding.GetBytes(str);

            try {
                IScriptScope scope = _pe.Runtime.CreateScope();
                _pe.Execute(scope, _pe.CreateScriptSourceFromString("import sys", SourceCodeKind.Statements));

                stdin.Write(bytes, 0, bytes.Length);
                stdin.Position = 0;
                _pe.Execute(scope, _pe.CreateScriptSourceFromString("output = sys.__stdin__.readline()", SourceCodeKind.Statements));
                AreEqual(str, _pe.Execute<string>(scope, _pe.CreateScriptSourceFromString("output")));

                _pe.Execute(scope, _pe.CreateScriptSourceFromString("sys.__stdout__.write(output)", SourceCodeKind.Statements));
                stdout.Flush();
                stdout.Position = 0;

                // deals with BOM:
                using (StreamReader reader = new StreamReader(stdout, true)) {
                    string s = reader.ReadToEnd();
                    AreEqual(str, s);
                }

                _pe.Execute(scope, _pe.CreateScriptSourceFromString("sys.__stderr__.write(\"This is stderr\")", SourceCodeKind.Statements));

                stderr.Flush();
                stderr.Position = 0;
                
                // deals with BOM:
                using (StreamReader reader = new StreamReader(stderr, true)) {
                    string s = reader.ReadToEnd();
                    AreEqual("This is stderr", s);
                }
            } finally {
                _pe.Runtime.IO.RedirectToConsole();
            }
        }

        public void ScenarioNullArguments() {
            try {
                _pe.Compile((SourceUnit)null);
                throw new Exception();
            } catch (ArgumentNullException) {
                //Pass
            }            
        }
       
        public void Scenario12() {
            IScriptScope scope = _env.CreateScope();

            _pe.Execute(scope, _pe.CreateScriptSourceFromString(@"
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
", SourceCodeKind.Statements));
        }

// TODO: rewrite 
#if FALSE
        public void ScenarioTrueDivision1() {
            TestOldDivision(pe, DefaultModule);
            ScriptScope module = pe.CreateModule("anonymous", ModuleOptions.TrueDivision);
            TestNewDivision(pe, module);
        }

        public void ScenarioTrueDivision2() {
            TestOldDivision(pe, DefaultModule);
            ScriptScope module = pe.CreateModule("__future__", ModuleOptions.PublishModule);
            module.SetVariable("division", 1);
            pe.Execute(pe.CreateScriptSourceFromString("from __future__ import division", module));
            TestNewDivision(pe, module);
        }

        public void ScenarioTrueDivision3() {
            TestOldDivision(pe, DefaultModule);
            ScriptScope future = pe.CreateModule("__future__", ModuleOptions.PublishModule);
            future.SetVariable("division", 1);
            ScriptScope td = pe.CreateModule("truediv", ModuleOptions.None);
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

            PythonDivisionOptions old = PythonEngine.CurrentEngine.Options.DivisionOptions;

            try {
                PythonEngine.CurrentEngine.Options.DivisionOptions = PythonDivisionOptions.Old;
                ScriptScope module = pe.CreateModule("anonymous", ModuleOptions.TrueDivision);
                pe.Execute(pe.CreateScriptSourceFromString("import " + modName, module));
                int res = pe.EvaluateAs<int>(modName + ".result", module);
                AreEqual(res, 0);
            } finally {
                PythonEngine.CurrentEngine.Options.DivisionOptions = old;
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
                ScriptScope module = ScriptDomainManager.CurrentManager.CreateModule(modName);
                pe.Execute(pe.CreateScriptSourceFromString("import " + modName, module));
                double res = pe.EvaluateAs<double>(modName + ".result", module);
                AreEqual(res, 0.5);
                AreEqual((bool)((PythonContext)DefaultContext.Default.LanguageContext).TrueDivision, false);
            } finally {
                try {
                    System.IO.File.Delete(file);
                } catch { }
            }
        }
        public void ScenarioSystemStatePrefix() {
            AreEqual(IronPythonTest.Common.RuntimeDirectory, pe.SystemState.prefix);
        }
#endif

        private static void TestOldDivision(ScriptEngine pe, ScriptScope module) {
            pe.Execute(pe.CreateScriptSourceFromString("result = 1/2", module));
            AreEqual((int)module.Scope.LookupName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("result")), 0);
            AreEqual(pe.EvaluateAs<int>("1/2", module), 0);
            pe.Execute(pe.CreateScriptSourceFromString("exec 'result = 3/2'", module));
            AreEqual((int)module.Scope.LookupName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("result")), 1);
            AreEqual(pe.EvaluateAs<int>("eval('3/2')", module), 1);
        }

        private static void TestNewDivision(ScriptEngine pe, ScriptScope module) {
            pe.Execute(pe.CreateScriptSourceFromString("result = 1/2", module));
            AreEqual((double)module.Scope.LookupName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("result")), 0.5);
            AreEqual(pe.EvaluateAs<double>("1/2", module), 0.5);
            pe.Execute(pe.CreateScriptSourceFromString("exec 'result = 3/2'", module));
            AreEqual((double)module.Scope.LookupName(DefaultContext.Default.LanguageContext, SymbolTable.StringToId("result")), 1.5);
            AreEqual(pe.EvaluateAs<double>("eval('3/2')", module), 1.5);
        }
#endif
        // More to come: exception related...

        public static int Negate(int arg) { return -1 * arg; }

        static void AreEqual<T>(T expected, T actual) {
            if (expected == null && actual == null) return;

            if (!expected.Equals(actual)) {
                Console.WriteLine("Expected: {0} Got: {1} from {2}", expected, actual, new StackTrace(true));
                throw new Exception();
            }
        }
    }
}
