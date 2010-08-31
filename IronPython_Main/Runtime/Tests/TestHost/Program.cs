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
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
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
        private delegate void TestCase();

        private static bool hostedOnDLR = false;
        private static ScriptRuntime _env;

        public static ScriptRuntime CreateRuntime() {
            return new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration());
        }

        public static ScriptRuntime CreateRemoteRuntime() {
            return ScriptRuntime.CreateRemote(CreateDomain(), ScriptRuntimeSetup.ReadConfiguration());
        }

#if !SILVERLIGHT
        static int Main(string[] args) {
            List<string> largs = new List<string>(args);
            if (largs.Contains("/help") || largs.Contains("-?") || largs.Contains("/?") || largs.Contains("-help")) {
                Console.WriteLine("Run All Tests      : testhost");
                Console.WriteLine("Run Specific Tests : TestHost [test_to_run ...]");
                Console.WriteLine("List Tests         : TestHost /list");
            }

            if (largs.Contains("/list")) {
                ForEachTestCase(delegate(MethodInfo mi) {
                    Console.WriteLine(mi.Name);
                });
                return 1;
            }

            try {

                ScriptRuntimeSetup ses = ScriptRuntimeSetup.ReadConfiguration();
                ses.HostType = typeof(TestHost);
                ses.DebugMode = true;

                _env = new ScriptRuntime(ses);
                int executed = 0;

                List<Exception> errors = new List<Exception>();

                ForEachTestCase(delegate(MethodInfo mi) {
                    string name = mi.Name;
                    if (args.Length > 0 && !largs.Contains(name)) {
                        return;
                    }

                    Console.Write("Executing {0}", name);

                    try {
                        mi.Invoke(null, new object[0]);
                        Console.WriteLine();
                    } catch (TargetInvocationException tie) {
                        Exception e = tie.InnerException;
                        Console.WriteLine(" .... FAIL " + e.Message);
                        errors.Add(e);
                    }
                    executed++;
                });

                if (errors.Count > 0) {
                    Console.WriteLine();
                    foreach (Exception err in errors) {
                        Console.WriteLine(err);
                    }
                    return 1;
                }

                // return failure on bad filter (any real failures throw)
                return executed == 0 ? -1 : 0;
            } catch (Exception e) {
                Console.Error.WriteLine(e);
                return 2;
            }
        }
#else
        public static int DoIt() {

            //Have to play nice with the already-created ScriptRuntime when we run from Python...
            _env = ScriptRuntime.GetEnvironment();
            hostedOnDLR = _env != null;
            if (_env == null)
            {
                ScriptRuntimeSetup ses = new ScriptRuntimeSetup(true);
                ses.HostType = typeof(TestHost);
                ses.PALType = typeof(Microsoft.Scripting.Silverlight.SilverlightPAL);

                //_env = ScriptRuntime.Create(null);
                _env = ScriptRuntime.Create(ses);
            }
            int executed = 0;

            for (int i = 0; i < Cases.Length; i++) {
                string name = Cases[i].Method.Name;

                Console.WriteLine("Executing {0}", Cases[i].Method.Name);
                Cases[i]();
                executed++;
            }

            // return failure on bad filter (any real failures throw)
            return executed == 0 ? -1 : 0;
        }

        public static void Main(string[] args) {
        }
#endif
        private delegate void TestAction(MethodInfo mi);
        private static void ForEachTestCase(TestAction action) {
            foreach (MethodInfo mi in typeof(Tests).GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)) {
                object[] attribute = mi.GetCustomAttributes(typeof(ScenarioAttribute), false);
                if (attribute == null || attribute.Length == 0) {
                    continue;
                }

                // Do the action
                action(mi);
            }
        }

        [Scenario]
        private static void Scenario_SimpleInteractiveWindow() {
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();

            var options = new PythonCompilerOptions();
            options.Optimized = false;
            var code = engine.CreateScriptSourceFromString("def square(x): return x ** 2", SourceCodeKind.InteractiveCode);
            code.Compile(options).Execute(scope);

            code = engine.CreateScriptSourceFromString("sum(square(x) for x in range(10))");
            AreEqual(code.Compile(options).Execute(scope), 285);
        }

        [Scenario]
        private static void Scenario_IORedirection() {
            StringWriter consoleOut = new StringWriter();

            StringWriter s = new StringWriter();
            _env.IO.SetOutput(Stream.Null, s);
            ScriptScope scope = _env.CreateScope();
            try {
                PY.CreateScriptSourceFromString("print 1+1", SourceCodeKind.Statements).Execute(scope);
            } finally {
                _env.IO.RedirectToConsole();
            }
            AreEqual(s.ToString(), "2\r\n");

#if !SILVERLIGHT // console redirection needs full trust
            s = new StringWriter();
            TextWriter oldOut = Console.Out;
            Console.SetOut(s);
            try {
                PY.CreateScriptSourceFromString("print 2+2", SourceCodeKind.Statements).Execute(scope);
            } finally {
                Console.SetOut(oldOut);
            }

            AreEqual(s.ToString(), "4\r\n");
#endif

            // test API check:
            AssertOutput(delegate() {
                PY.CreateScriptSourceFromString("print 'foo'", SourceCodeKind.Statements).Execute(scope);
            }, "foo");
        }

        [Scenario]
        private static void Scenario_PythonTokenizer1() {
            Tokenizer tokenizer = new Tokenizer(ErrorSink.Null);
            List<Token> tokens;

            tokens = GetPythonTokens(tokenizer, "sys");

            Assert(tokens.Count == 1 &&
                tokens[0].Kind == IronPython.Compiler.TokenKind.Name);
        }

        private static List<Token> GetPythonTokens(Tokenizer tokenizer, string source) {
            tokenizer.Initialize(PythonContext.CreateSnippet(source, SourceCodeKind.Statements));
            List<Token> tokens = new List<Token>();
            Token token;
            while ((token = tokenizer.GetNextToken()) != Tokens.EndOfFileToken) {
                tokens.Add(token);
            }
            return tokens;
        }

        [Scenario]
        private static void Scenario_PythonCategorizer1() {
            TestCategorizer(PY, "sys", 3, new TokenInfo[] { 
                new TokenInfo(new SourceSpan(new SourceLocation(0,1,1), new SourceLocation(3,1,4)), TokenCategory.Identifier, TokenTriggers.None),
            });
        }

#if !SILVERLIGHT
        [Scenario]
        private static void Scenario_PythonImports1() {
            string dir = Path.GetTempPath();

            string aPath = dir + @"a.py";
            string bPath = dir + @"lib\b.py";
            string cPath = dir + @"lib\c.py";
            string initPath = dir + @"lib\__init__.py";

            PY.SetSearchPaths(new string[] { dir });

            try {
                Directory.CreateDirectory(dir + @"\lib");

                File.WriteAllText(aPath, "import lib.b");
                File.WriteAllText(bPath, "import c");
                File.WriteAllText(cPath, "");
                File.WriteAllText(initPath, "");

                ScriptScope sm = _env.CreateScope();
                PY.CreateScriptSourceFromFile(aPath).Execute(sm);

            } finally {
                File.Delete(aPath);
                File.Delete(bPath);
                File.Delete(cPath);
                File.Delete(initPath);
            }
        }
#endif

        [Scenario]
        private static void Scenario_PythonErrorSinkThrowsSyntaxErrorException() {
            ScriptScope module = _env.CreateScope();
            try {
                PY.CreateScriptSourceFromString("a = \"a broken string'", SourceCodeKind.Statements).Execute(module);
                throw new Exception("We should not reach here");
            } catch (SyntaxErrorException e) {
                AreEqual("EOL while scanning single-quoted string", e.Message);
            }
        }

        private static void TestCategorizer(ScriptEngine engine, string src, int charCount, params TokenInfo[] expected) {
            TokenCategorizer categorizer = engine.GetService<TokenCategorizer>();

            categorizer.Initialize(null, engine.CreateScriptSourceFromString("sys"), SourceLocation.MinValue);
            IEnumerable<TokenInfo> actual = categorizer.ReadTokens(3);

            int i = 0;
            foreach (TokenInfo info in actual) {
                Assert(i < expected.Length);
                Assert(info.Equals(expected[i]));
                i++;
            }
            Assert(i == expected.Length);
        }

        [Scenario]
        private static void Scenario_LanguageConfiguration() {
            ScriptRuntime runtime = CreateRuntime();

            foreach (var language in runtime.Setup.LanguageSetups) {
                ScriptEngine engine = runtime.GetEngineByTypeName(language.TypeName);

                Assert(language == engine.Setup);

                foreach (var ext in language.FileExtensions) {
                    Assert(engine == runtime.GetEngineByFileExtension(ext));
                }
                foreach (var name in language.Names) {
                    Assert(engine == runtime.GetEngine(name));
                }
            }
        }

        private static bool IsSubset(string[] subset, string[] superset) {
            Assert(subset != null && superset != null);

            foreach (string member in subset) {
                if (Array.IndexOf(superset, member) == -1) {
                    return false;
                }
            }

            return true;
        }

        [Scenario]
        private static void Scenario_Interactive() {
            ScriptScope module = _env.CreateScope();

            string[] input_output = new string[] {
                "\n\n\n\n\n\ninvalid=3;more_invalid=4;more_invalid",
                "\n\n\n\n\n\ninvalid=3;more_invalid=4;more_invalid\n\n\n\n",
                "\n\n\n\n\n\n\n\nvalid=3;more_valid=4;more_valid#print should be valid input\n\n\n\n",
                "\n\n\n\n\n\n700",
                "\n\n\n\n\n\n800\n\n\n\n\n\n",
                "x=1\ninvalid",
                "x=1\nmore_invalid",
                "\ncomplete_invalid",
                "valid=3\n;#more_valid=4;more_valid\n\n\n\n\n",
                "valid=3\n;#more_valid=4;more_valid\n\n\n\n\n"
            };

            foreach (string x in input_output) {
                try {
                    PY.CreateScriptSourceFromString(x, SourceCodeKind.InteractiveCode).Execute(module);
                    Assert(false, "Invalid Input " + x + "accepted");
                } catch (SyntaxErrorException) {
                    // ok
                } catch (Exception e) {
                    Assert(false, "Exception raised on input '" + x + "': " + e);
                }
            }
        }

        [Scenario]
        private static void Scenario_Interactive2() {
            var props = PY.CreateScriptSourceFromString("x = \"abc\\\n", SourceCodeKind.InteractiveCode).GetCodeProperties();
            Assert(props == ScriptCodeParseResult.IncompleteToken);
        }

        [Scenario]
        private static void Scenario_Interactive3() {
            ScriptScope scope = _env.CreateScope();
            RedirectOutput(TextWriter.Null, delegate() {
                PY.CreateScriptSourceFromString("1;2", SourceCodeKind.InteractiveCode).Execute(scope);
            });
        }

        [Scenario]
        private static void Scenario_ShowCls1() {
            ScriptScope scope = PY.Runtime.CreateScope();

            CompiledCode compiledCode = PY.CreateScriptSourceFromString("print 'foo;bar'.Split(';')", SourceCodeKind.SingleStatement).Compile();

            AssertExceptionThrown<MissingMemberException>(delegate() {
                compiledCode.Execute(scope);
            });

            CompiledCode compiledCode2 = PY.CreateScriptSourceFromString("import clr", SourceCodeKind.SingleStatement).Compile();
            compiledCode2.Execute(scope);

            AssertOutput(delegate() {
                compiledCode.Execute(scope);
            }, "Array[str](('foo', 'bar'))");
        }

        [Scenario]
        private static void Scenario_TrueDivision() {
            ScriptScope scope = PY.Runtime.CreateScope();
            PY.Runtime.Globals.SetVariable("__future__", scope);
            scope.SetVariable("division", 1);

            ScriptScope module1 = PY.Runtime.CreateScope();

            AssertOutput(delegate() {
                PY.CreateScriptSourceFromString("print 1/2", SourceCodeKind.Statements).Execute(module1);
            }, "0");
#if FALSE            
            // TD enabled code in TD disabled module is ok:
            PY.Compile(PY.CreateScriptSourceFromString("from __future__ import division", SourceCodeKind.Statements)).Execute(module1);
            AssertOutput(delegate() {
                PY.Compile(PY.CreateScriptSourceFromString("print 1/2", SourceCodeKind.Statements)).Execute(module1);
            }, "0.5");
#endif
        }

        const string _pythonFactorialFunction =
@"def fact(x):
    if (x == 1):
        return 1
    return x * fact(x - 1)
 
testNum = 5";

        const string _jsFactorialFunction =
@"function fact(x) {
    if (x == 1) {
        return 1;
    }
    return x * fact(x - 1);
}

testNum =5;";

        /// <summary>
        /// Define and execute a recursive factorial method in each language.
        /// 
        /// Default mode.
        /// </summary>
        [Scenario]
        private static void Scenario_Factorial1() {
            ScriptScope scope = _env.CreateScope();
            //Python
            PY.CreateScriptSourceFromString(_pythonFactorialFunction, SourceCodeKind.Statements).Execute(scope);
            AssertOutput(delegate() { PY.CreateScriptSourceFromString("print fact(5)", SourceCodeKind.Statements).Execute(scope); }, "120");
            scope.RemoveVariable("fact"); //Manually remove this to ensure it doesn't affect further tests
            AssertExceptionThrown<UnboundNameException>(delegate() { PY.CreateScriptSourceFromString("print fact(5)", SourceCodeKind.Statements).Execute(scope); });
        }

        /// <summary>
        /// New module from memory + module reloading. Python.
        /// </summary>
        [Scenario]
        private static void Scenario_Factorial2_PY() {
#if TODO //__name__ is not set correctly when a module is created by a host
            //Python
            SourceUnit sourceUnit = PY.CreateScriptSourceFromString(_pythonFactorialFunction, "test", SourceCodeKind.Statements);
            ScriptScope module = ScriptDomainManager.CurrentManager.CompileModule("ModuleTest", ScriptModuleKind.Default, null, null, null, sourceUnit);
            module.Execute();
            //PY.PublishModule(module);
            AssertOutput(delegate() { PY.Execute(module, PY.CreateScriptSourceFromString("print __name__", SourceCodeKind.Statements)); }, "ModuleTest");
            AssertOutput(delegate() { PY.Execute(module, PY.CreateScriptSourceFromString("print fact(testNum)", SourceCodeKind.Statements)); }, "120");
            PY.Execute(module, PY.CreateScriptSourceFromString("testNum = 7", SourceCodeKind.Statements));
            AssertOutput(delegate() { PY.Execute(module, PY.CreateScriptSourceFromString("print fact(testNum)", SourceCodeKind.Statements)); }, "5040");
            module.Reload();
            AssertOutput(delegate() { PY.Execute(module, PY.CreateScriptSourceFromString("print fact(testNum)", SourceCodeKind.Statements)); }, "120");
#endif
        }

        /// <summary>
        /// Covers ScriptEngine.CompileExpression for various basic expressions
        /// </summary>
        [Scenario]
        private static void Scenario_CompileExpression() {
            ScriptScope scope = _env.CreateScope();
            scope.SetVariable("modulevar", 5);

            //Compile and execution a normal expression
            CompiledCode e1 = PY.CreateScriptSourceFromString("2+2").Compile();
            AssertOutput(delegate() { e1.Execute(scope); }, "");

            //Compile an invalid expression
            AssertExceptionThrown<SyntaxErrorException>(delegate() {
                PY.CreateScriptSourceFromString("? 2+2").Compile();
            });

            //Compile an expression referencing an unbound variable, which generates a runtime error
            e1 = PY.CreateScriptSourceFromString("unbound + 2").Compile();
            AssertExceptionThrown<UnboundNameException>(delegate() {
                e1.Execute(scope);
            });

            //Compile an expression referencing a module bound variable
            e1 = PY.CreateScriptSourceFromString("modulevar+2").Compile();
            AssertOutput(delegate() { e1.Execute(scope); }, "");

            //@TODO - CompileExpression with meaningful module argument

            //@TODO - Note, JScript doesn't currently support CompileExpression, add tests when they do
        }

        /// <summary>
        /// Covers ScriptEngine.CompileStatement for various basic statements
        /// </summary>
        [Scenario]
        private static void Scenario_CompileStatement() {
            ScriptScope scope = _env.CreateScope();
            scope.SetVariable("modulevar", 5);

            //Compile and execute a normal statement
            CompiledCode e1 = PY.CreateScriptSourceFromString("print 2+2", SourceCodeKind.Statements).Compile();
            AssertOutput(delegate() { e1.Execute(scope); }, "4");

            //Compile an invalid statement
            AssertExceptionThrown<SyntaxErrorException>(delegate() {
                PY.CreateScriptSourceFromString("? 2+2", SourceCodeKind.Statements).Compile();
            });

            //Compile a statement referencing an unbound variable, which generates a runtime error
            e1 = PY.CreateScriptSourceFromString("print unbound+2", SourceCodeKind.Statements).Compile();
            AssertExceptionThrown<UnboundNameException>(delegate() {
                e1.Execute(scope);
            });

            //Compile a statement referencing a module bound variable
            e1 = PY.CreateScriptSourceFromString("print modulevar+2", SourceCodeKind.Statements).Compile();
            AssertOutput(delegate() { e1.Execute(scope); }, "7");

            //Bind a module variable in a statement and then reference it
            PY.CreateScriptSourceFromString("pythonvar='This is a python variable'", SourceCodeKind.Statements).Execute(scope);
            e1 = PY.CreateScriptSourceFromString("print pythonvar", SourceCodeKind.Statements).Compile();
            AssertOutput(delegate() { e1.Execute(scope); }, "This is a python variable");
            Assert(scope.ContainsVariable("pythonvar"), "Bound variable isn't visible in the module dict");

            //@TODO - CompileStatement with meaningful module argument

            //@TODO - JScript also doesn't support CompileStatement yet
        }

        /// <summary>
        /// Covers ScriptEngine.CompileCode for various basic code
        /// </summary>
        [Scenario]
        private static void Scenario_CompileCode() {
            ScriptScope scope = _env.CreateScope();
            scope.SetVariable("modulevar", 5);

            //Define methods in the module we'll use for code
            PY.CreateScriptSourceFromString(@"
def f(arg):
    print arg
", SourceCodeKind.Statements).Execute(scope);

            //Compile and execute valid code
            CompiledCode e1 = PY.CreateScriptSourceFromString("f('Hello World!')").Compile();
            AssertOutput(delegate() { e1.Execute(scope); }, "Hello World!");

            //Compile and execute code that gives an execution error
            e1 = PY.CreateScriptSourceFromString("f(unboundvar)").Compile();
            AssertExceptionThrown<UnboundNameException>(delegate() {
                e1.Execute(scope);
            });

            //Compile invalid code
            AssertExceptionThrown<SyntaxErrorException>(delegate() {
                PY.CreateScriptSourceFromString("f(?<this is bad syntax>?)").Compile();
            });

            //Bind a module variable in code and then reference it
            PY.CreateScriptSourceFromString("modulevar = 'This is a global from python'", SourceCodeKind.Statements).Execute(scope);
            e1 = PY.CreateScriptSourceFromString("f(modulevar)").Compile();
            AssertOutput(delegate() { e1.Execute(scope); }, "This is a global from python");
            Assert(scope.ContainsVariable("modulevar"), "Bound variable isn't visible in the module dict");
        }
#if !SILVERLIGHT

        #region Scenarion_CompileCodeDom

        public class Scenario_CompileCodeDom_Writer {
            StringBuilder _text = new StringBuilder();

            public void Write(object value) {
                _text.Append(value);
            }

            public string Text {
                get { return _text.ToString(); }
            }
        }

        [Scenario]
        public static void Scenario_CompileCodeDom() {
            CodeMemberMethod method;
            string result;

            method = GenerateTestCodeDom("myMethod0", new SourceLocation(72, 4, 17),
@"<%
a=1;
b=2;
%><b>Some piece of HTML</b><%=a+b%>");

            result = TestCodeDomMethod(PY, method);
            Assert(result == "<b>Some piece of HTML</b>3");

            method = GenerateTestCodeDom("myMethod1", new SourceLocation(50, 6, 1),
@"<%
for i in range(3):
    %>i=<%=i%> i*i=<%=i*i%><br />");

            result = TestCodeDomMethod(PY, method);
            Assert(result == "i=0 i*i=0<br />i=1 i*i=1<br />i=2 i*i=4<br />");

            method = GenerateTestCodeDom("myMethod1", new SourceLocation(0, 1, 1),
@"This is a test of line mapping.
A syntax error should occur on line 5. (due to a missing ':' on the previous line)
Note: line numbering starts from 1.
<%
for i in range(3)
    %>
i=<%=i%> i*i=<%=i*i%><br />");

            SyntaxErrorException ex = AssertExceptionThrown<SyntaxErrorException>(delegate() {
                TestCodeDomMethod(PY, method);
            });

            Assert(ex.Line == 5);
        }

        private static string TestCodeDomMethod(ScriptEngine engine, CodeMemberMethod method) {
            ScriptScope scope = _env.CreateScope();
            Scenario_CompileCodeDom_Writer writer = new Scenario_CompileCodeDom_Writer();
            CompiledCode code = engine.CreateScriptSource(method).Compile();
            code.Execute(scope);
            object methodVar = scope.GetVariable(method.Name);
            DynamicDelegateCreator dlgCreator = new DynamicDelegateCreator(HostingHelpers.GetLanguageContext(engine));
            Action<object> methodDelegate = (Action<object>)dlgCreator.GetDelegate(methodVar, typeof(Action<object>));
            methodDelegate(writer);
            return writer.Text;
        }

        private static CodeMemberMethod GenerateTestCodeDom(string methodName, SourceLocation loc, string code) {
            // *very* simple parser for parsing test cases
            // (so I don't have to generate CodeDoms by hand)

            CodeMemberMethod myMethod = new CodeMemberMethod();
            myMethod.Name = methodName;
            myMethod.Parameters.Add(new CodeParameterDeclarationExpression("object", "__writer"));
            SourceLocation methodStart = loc;

            string[] blocks = code.Split(new string[] { "<%", "%>" }, StringSplitOptions.None);

            bool literal = true;

            for (int i = 0; i < blocks.Length; ++i) {
                string block = blocks[i];

                bool expression = !literal && block.StartsWith("=");
                if (expression) {
                    loc = CountLinesCols(loc, "=");
                    block = block.Substring(1);
                }

                SourceLocation start = loc;
                loc = CountLinesCols(loc, block);
                loc = CountLinesCols(loc, "<%");

                CodeStatement stmt = null;
                if (expression) {
                    stmt = GenerateWriteExpressionStmt(new CodeSnippetExpression(block));
                } else if (literal) {
                    if (block != "") {
                        stmt = GenerateWriteExpressionStmt(new CodePrimitiveExpression(block));
                    }
                } else {
                    stmt = new CodeSnippetStatement(block);
                }

                if (stmt != null) {
                    stmt.LinePragma = new CodeLinePragma(methodName, start.Line);
                    myMethod.Statements.Add(stmt);
                }

                literal = !literal;
            }

            return myMethod;
        }

        private static SourceLocation CountLinesCols(SourceLocation loc, string block) {
            int index = loc.Index, line = loc.Line, col = loc.Column;

            index += block.Length;

            string[] lines = block.Split('\n');
            int last = lines.Length - 1;

            if (last > 0) {
                line += last;
                col = lines[last].Length + 1;
            } else {
                col += lines[0].Length;
            }

            return new SourceLocation(index, line, col);
        }


        private static CodeExpressionStatement GenerateWriteExpressionStmt(CodeExpression expression) {
            return new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("__writer"), "Write", expression));
        }
        #endregion
#endif

#if !SILVERLIGHT
        [Scenario]
        private static void Scenario_LocalSourceUnitCompilation() {
            TestSourceUnitCompilation(CreateRuntime());
        }

        [Scenario]
        private static void Scenario_RemoteSourceUnitCompilation() {
            TestSourceUnitCompilation(CreateRemoteRuntime());
        }


        private static void TestSourceUnitCompilation(ScriptRuntime env) {
            ScriptEngine engine = env.GetEngine("py");
            ScriptScope scope = env.CreateScope();

            ScriptSource unit = engine.CreateScriptSourceFromString("1+1", SourceCodeKind.Expression);
            CompiledCode code = unit.Compile();

            object o = code.Execute(scope);
            Assert(o is int && (int)o == 2);

            int oi = code.Execute<int>(scope);
            Assert(oi == 2);

            unit = engine.CreateScriptSourceFromString("dir()", SourceCodeKind.Expression);
            code = unit.Compile();

            object foo = code.Execute(scope);

            ObjectHandle handle = code.ExecuteAndWrap(scope);
            IList<string> members = engine.Operations.GetMemberNames(handle);
            Assert(members.IndexOf("sort") != -1, "Python dictionary should contain sort() method");
        }

#if FALSE
        [Scenario]
        private static void Scenario_LocalSourceUnitReload() {
            TestSourceUnitReload(ScriptRuntime.GetEnvironment());
        }

        [Scenario]
        private static void Scenario_RemoteSourceUnitReload() {
            TestSourceUnitReload(ScriptRuntime.Create(null, CreateDomain()));
        }

        private static void TestSourceUnitReload(ScriptRuntime env) {
            ScriptEngine engine = env.GetEngine("py");

            SourceUnit unit = engine.CreateScriptSourceFromString("x = 1+1", "foobarbaz", SourceCodeKind.Statements);
            SourceUnit unit2 = engine.CreateScriptSourceFromString("x = 2+2", "foobarbaz", SourceCodeKind.Statements);
            
            ScriptScope scope = engine.Runtime.CreateModule("foo");
            engine.Compile(unit).Execute(scope);

            object x;

            module.Execute();
            x = module.LookupVariable("x");
            Assert(x is int && (int)x == 2);

            module.Reload();
            x = module.LookupVariable("x");
            Assert(x is int && (int)x == 4);
        }
#endif

        [Scenario]
        private static void Scenario_InteractiveCode() {
            ScriptScope scope = _env.CreateScope();

            CompiledCode compiled_code;
            object result;

            RedirectOutput(TextWriter.Null, delegate() {
                compiled_code = PY.CreateScriptSourceFromString("1+1", SourceCodeKind.InteractiveCode).Compile();
                result = compiled_code.Execute(scope);
                Assert(result == null);

                compiled_code = PY.CreateScriptSourceFromString("print 'foo'", SourceCodeKind.InteractiveCode).Compile();
                result = compiled_code.Execute(scope);
                Assert(result == null);
            });
            AssertOutput(delegate() { PY.CreateScriptSourceFromString("1+1", SourceCodeKind.InteractiveCode).Execute(scope); }, "2");
        }

#endif

        [Scenario]
        private static void Scenario_LocalCodeSense() {
            ScriptScope scope = _env.CreateScope();
            ObjectOperations ops = PY.CreateOperations(scope);

            object obj = PY.CreateScriptSourceFromString("dir()", SourceCodeKind.Expression).Execute(scope);
            IList<string> members = ops.GetMemberNames(obj);
            Assert(members.IndexOf("sort") != -1, "Python dictionary should contain sort() method");

            object ftn;
            Assert(ops.TryGetMember(obj, "sort", out ftn));
            IList<string> signatures = ops.GetCallSignatures(ftn);

            // string object:
            members = ops.GetMemberNames("foo");
            Assert(members.IndexOf("islower") != -1, "CLR string should contain islower() method");
            Assert(members.IndexOf("Split") == -1, "CLR string should NOT contain Split() method when ShowCls == false");

            // importing CLR semantics:
            PY.CreateScriptSourceFromString("import clr", SourceCodeKind.SingleStatement).Execute(scope);

            members = ops.GetMemberNames("foo");
            Assert(members.IndexOf("islower") != -1, "CLR string should contain islower() method");
            // TODO: BUG: rule doesn't have test for ShowCls
            //Assert(members.IndexOf("Split") != -1, "CLR string should contain Split() method when ShowCls == true");
        }

#if !SILVERLIGHT
        [Scenario]
        private static void Scenario_LocalGetObjectMemberValue() {
            TestGetObjectMemberValue(CreateRuntime());
        }

        [Scenario]
        private static void Scenario_RemoteGetObjectMemberValue() {
            TestGetObjectMemberValue(CreateRemoteRuntime());
        }

        private static void TestGetObjectMemberValue(ScriptRuntime env) {
            ObjectHandle obj;
            ScriptScope module = env.CreateScope();
            ScriptEngine engine = env.GetEngine("py");

            engine.CreateScriptSourceFromString("import datetime", SourceCodeKind.Statements).Execute(module);

            obj = module.GetVariableHandle("datetime");
            Assert(obj != null);

            Assert(engine.Operations.TryGetMember(obj, "date", out obj));
            Assert(obj != null);

            Assert(engine.Operations.TryGetMember(obj, "replace", out obj));
            Assert(obj != null);

            Assert(engine.Operations.IsCallable(obj));

            IList<string> signatures = engine.Operations.GetCallSignatures(obj);
            CheckSignatures(signatures);

            // builtins/globals are not available in all scopes:
            Assert(!module.TryGetVariableHandle("dir", out obj));
        }
#endif
        private static void CheckSignatures(IList<string> signatures) {
            Assert(signatures != null && signatures.Count == 2);
            string sig1 = "date replace(self, dict dict)";
            string sig2 = "object replace(self)";
            Debug.Assert(signatures[0] == sig1 || signatures[1] == sig1);
            Debug.Assert(signatures[0] == sig2 || signatures[1] == sig2);
        }
#if !SILVERLIGHT

        [Scenario]
        private static void Scenario_LocalObjectCall() {
            TestObjectCall(CreateRuntime());
        }

        [Scenario]
        private static void Scenario_RemoteObjectCall() {
            TestObjectCall(CreateRemoteRuntime());
        }

        private static void TestObjectCall(ScriptRuntime env) {
            ScriptScope module = env.CreateScope();
            ScriptEngine engine = env.GetEngine("py");

            engine.CreateScriptSourceFromString("def foo(a,b):\n  return b\n", SourceCodeKind.Statements).Execute(module);
            ObjectHandle h_foo = module.GetVariableHandle("foo");
            Assert(engine.Operations.IsCallable(h_foo));

            // remote object with local arguments:
            ObjectHandle h_result = engine.Operations.Invoke(h_foo, 1, 2);
            Assert(h_result != null);

            object result = h_result.Unwrap();
            Assert(result is int && (int)result == 2);

            // create a dictionary and get another reference to it:
            ObjectHandle h_dir = engine.CreateScriptSourceFromString("dir()").ExecuteAndWrap(module);
            ObjectHandle h_dir1 = engine.Operations.Invoke(h_foo, 1, h_dir);

            // check that these references points to the same object:
            engine.CreateScriptSourceFromString("import System", SourceCodeKind.Statements).Execute(module);
            engine.CreateScriptSourceFromString("def RefEq(a,b):\n  return System.Object.ReferenceEquals(a,b)\n", SourceCodeKind.Statements).Execute(module);
            ObjectHandle h_re = module.GetVariableHandle("RefEq");
            Assert(engine.Operations.IsCallable(h_re));

            result = engine.Operations.Invoke(h_re, h_dir, h_dir).Unwrap();
            Assert(result is bool && (bool)result == true);
        }

        [Scenario]
        private static void Scenario_RemoteEvaluation() {
            ScriptRuntime env = CreateRemoteRuntime();
            ScriptEngine engine = env.GetEngine("py");
            ScriptScope scope = env.CreateScope();

            ObjectHandle obj = engine.CreateScriptSourceFromString("dir()").ExecuteAndWrap(scope);
            IList<string> members = engine.Operations.GetMemberNames(obj);
            Assert(members.IndexOf("sort") != -1, "Python dictionary should contain sort() method");

            object obj2 = engine.CreateScriptSourceFromString("1 + 1").Execute(scope);
            Assert(obj2 is int, "Evaluate()'d object shouldn't be wrapped.");
            members = engine.Operations.GetMemberNames(obj2);
            Assert(members.IndexOf("__cmp__") != -1, "Integer number should contain __cmp__ method");

            engine.CreateScriptSourceFromString("dir()").Execute(scope);
        }

        [Scenario]
        private static void Scenario_MultipleRemoteDomains() {
            ScriptRuntime env1 = CreateRemoteRuntime();
            ScriptRuntime env2 = CreateRemoteRuntime();


            // get LP in the evironment #1:
            ScriptEngine eng1 = env1.GetEngine("py");
            Assert(eng1 != null);

            // create an engine for the same language in the environment #2:
            ScriptEngine eng2 = env2.GetEngine("py");
            Assert(eng2 != null);

            ScriptScope scope1 = env1.CreateScope();
            ScriptScope scope2 = env2.CreateScope();
            eng1.CreateScriptSourceFromString("x = 1 + 1", SourceCodeKind.Statements).Execute(scope1);
            eng2.CreateScriptSourceFromString("y = 2 + 2", SourceCodeKind.Statements).Execute(scope2);

            object var_x = scope1.GetVariable("x");
            Assert(var_x is int && (int)var_x == 2);

            object var_y = scope2.GetVariable("y");
            Assert(var_y is int && (int)var_y == 4);
        }
#endif
        /// <summary>
        /// Add code coverage to ScriptEngine, mostly negative/exception cases
        /// </summary>
        [Scenario]
        private static void Coverage_ScriptEngine() {
            ScriptScope scope = _env.CreateScope();

            // TODO:
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                PY.CreateScriptSource((CodeMemberMethod)null);
            });
        }

#if !SILVERLIGHT
#if TODO // rewrite using Global Namespace
        [Scenario]
        public static void Scenario_RemoteModulePublishing() {
            ScriptRuntime env = ScriptRuntime.Create(null, CreateDomain());
            ScriptEngine engine = env.GetEngine("py");
            ScriptScope mod = env.CreateModule("foo");

            ScriptScope module = env.CreateModule("foo");
            module.FileName = "C:\\foo.py";

            CompiledCode code = engine.Compile(engine.CreateScriptSourceFromString("print 'foo'", @"C:\foo.py", SourceCodeKind.File));

            ScriptScope anonymous_module = env.CreateModule("bar-module",
                engine.Compile(engine.CreateScriptSourceFromString("print 'bar'", SourceCodeKind.Statements)),
                engine.Compile(engine.CreateScriptSourceFromString("print 'baz'", SourceCodeKind.Statements)));

            env.PublishModule(module);
            env.PublishModule(module, "my module");

            AssertExceptionThrown<ArgumentException>(delegate() {
                env.PublishModule(anonymous_module);
            });

            AssertExceptionThrown<ArgumentNullException>(delegate() {
                env.PublishModule(null);
            });

            AssertExceptionThrown<ArgumentNullException>(delegate() {
                env.PublishModule(module, null);
            });

            IDictionary<string, ScriptScope> modules = env.GetPublishedModules();

            Assert(modules != null && modules.Count >= 2);
            Assert(modules.ContainsKey("my module"));
            Assert(modules.ContainsKey(@"C:\foo.py"));
            Assert(modules["my module"] != null);
            Assert(modules[@"C:\foo.py"] != null);
        }
#endif
#endif

        [Scenario]
        public static void Scenario_Exceptions() {
            if (!hostedOnDLR) {
                //Disable this test when this assembly is hosted from Python; stack isn't really predictable.
                ScriptScope sm = _env.CreateScope();
                PY.CreateScriptSourceFromString("def pyf(): return abc", SourceCodeKind.Statements).Execute(sm);

                try {
                    object x = PY.Operations.Invoke(sm.GetVariable("pyf"));
                } catch (Exception e) {
                    DynamicStackFrame[] frames = PythonOps.GetDynamicStackFrames(e);
                    Assert(frames.Length == 1);
                }


                sm = _env.CreateScope();
                PY.CreateScriptSourceFromString("def pyf(): return abc\n\ndef pyf2(): return pyf()", SourceCodeKind.Statements).Execute(sm);

                try {
                    object x = PY.Operations.Invoke(sm.GetVariable("pyf2"));
                } catch (Exception e) {
                    DynamicStackFrame[] frames = PythonOps.GetDynamicStackFrames(e);
                    Assert(frames.Length == 2);
                }
            } else {
                Console.WriteLine("Skipping Scenario_Exceptions!");
            }
        }

        #region Hosting API Review Samples
#if TODO // TODO: rewrite according to the new HAPI usage scenarios

        const string Python = "py";
        const string JavaScript = "js";

        /// <summary>
        /// Level I. Execute a script code, evaluate an expression.
        /// Used concepts: Script.
        /// </summary>
        [Scenario] 
        public static void Scenario_ConvenienceAPI_RunCode() {
            Script.Execute(Python, "def add5(x): return x + 5");
            object result = Script.Evaluate(JavaScript, "add5(10)");

            Assert(result is double && (double)result == 15.0);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Level I. Execute a script file content in the context of the current module.
        /// Used concepts: Script, variables, default variable scope.
        /// </summary>
        [Scenario]
        public static void Scenario_ConvenienceAPI_RunFile1() {
            File.WriteAllText("my_python.py", "x = 'Hello'; print x");
            Script.ClearVariables();

            AssertOutput(delegate() {
                Script.ExecuteFileContent("my_python.py");
            }, "Hello");

            Assert(Script.VariableExists("x") == true);
        }

        /// <summary>
        /// Level I. Execute a script file in isolation from the current module.
        /// Used concepts: Script, variables, isolated variable scope.
        /// </summary>
        [Scenario]
        public static void Scenario_ConvenienceAPI_RunFile2() {
            File.WriteAllText("my_python.py", "x = 'Hello'; print x");
            Script.ClearVariables();

            AssertOutput(delegate() {

                Script.ExecuteFile("my_python.py");

            }, "Hello");

            Assert(Script.VariableExists("x") == false);
        }
#endif

        /// <summary>
        /// Level I. Set variables in the current module, execute Python and JavaScript code using these variables and retrieve results back.
        /// Used concepts: Script, variables.
        /// </summary>
        // TODO: [Scenario]
        public static void Scenario_ConvenienceAPI_Variables() {
            int x = 1;
            int y = 2;

            Script.SetVariable("x", x);
            Script.SetVariable("y", y);
            Script.Execute(Python, "py_z = x / y");
            Script.Execute(JavaScript, "js_z = x / y");
            object py_z = Script.GetVariable("py_z");
            object js_z = Script.GetVariable("js_z");

            Assert(py_z is int && (int)py_z == 0);
            Assert(js_z is double && (double)js_z == 0.5);
        }

        /// <summary>
        /// Level II. Compile a file into a module, get variables from the module's scope, call a function defined in the module.
        /// Used concepts: ScriptEngine, ScriptModule, module execution, variables, callable objects.
        /// </summary>
        [Scenario]
        public static void Scenario_ConvenienceAPI_Modules1() {
            ScriptEngine engine = Script.GetEngine(Python);

            File.WriteAllText("python_module.py", @"
def foo(): 
    global x  
    return 'Global x is ' + x
");

            ScriptScope module = engine.Runtime.CreateModule("python_module");
            CompiledCode code = engine.Compile(engine.CreateScriptSourceFromFile("python_module.py"));
            module.SetVariable("x", "Hello");
            code.Execute(module);

            object foo = module.LookupVariable("foo");
            object result = engine.Operations.Call(foo);

            Assert(result as string == "Global x is Hello");
        }
        /// <summary>
        /// Level II. Compile a code snippet to a module and call a function defined by the snippet.
        /// Used concepts: ScriptEngine, CompiledCode, ScriptModule, module execution, variables, callable objects.
        /// </summary>
        [Scenario]
        public static void Scenario_ConvenienceAPI_Modules2() {
            ScriptEngine engine = Script.GetEngine(Python);

            CompiledCode code = engine.Compile(engine.CreateScriptSourceFromString("def bar(): return 'bar'", SourceCodeKind.Statements));
            ScriptScope module = engine.Runtime.CreateModule("my_module");
            code.Execute(module);
            object result = engine.Operations.Call(module.LookupVariable("bar"));

            Assert(result as string == "bar");
        }

        /// <summary>
        /// Level II. Imports pre-compiled module.
        /// Used concepts: ScriptEngine, CompiledCode, ScriptModule, module execution, module publishing, evaluation.
        /// </summary>
        [Scenario]
        public static void Scenario_ConvenienceAPI_Import() {
            ScriptEngine engine = Script.GetEngine(Python);

            ScriptScope module = engine.Runtime.CreateModule("my_module");
            module.FileName = "my_module";
            CompiledCode code = engine.Compile(engine.CreateScriptSourceFromString("def bar(): return 'bar'", SourceCodeKind.Statements));

            // module should be initialized first:
            code.Execute(module);

            engine.Runtime.PublishModule(module);
            engine.Execute(module, engine.CreateScriptSourceFromString("import my_module", SourceCodeKind.Statements));
            object result = engine.Execute(module, engine.CreateScriptSourceFromString("my_module.bar()"));
            Assert(result as string == "bar");
        }

        /// <summary>
        /// Level II. Reflects over object members.
        /// Used concepts: ScriptEngine, variables, script object reflection.
        /// </summary>
        [Scenario]
        public static void Scenario_ConvenienceAPI_Introspection() {
            ScriptEngine engine = _env.GetEngine(Python);
            ScriptScope scope = _env.CreateScope();

            engine.Execute(scope, engine.CreateScriptSourceFromString("import datetime", ScriptSourceKind.Statements));

            object obj = engine.GetVariable(scope, "datetime");
            engine.Operations.TryGetMember(obj, "date", out obj);
            engine.Operations.TryGetMember(obj, "replace", out obj);
            engine.Operations.IsCallable(obj);

            IList<string> signatures = engine.Operations.GetCallSignatures(obj);
            CheckSignatures(signatures);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Level III. Remote objects introspection.
        /// Used concepts: ScriptRuntime, ScriptEngine, remote objects (ObjectHandle), script object reflection.
        /// </summary>
        // TODO: [Scenario]
        public static void Scenario_ConvenienceAPI_RemoteIntrospection() {
            File.WriteAllText("lib_module.py", "def bar(): return dir()");

            // creates a remote environment with a default setup:
            ScriptRuntime remoteEnvironment = ScriptRuntime.Create(null, AppDomain.CreateDomain("new domain"));

            // gets remote Python engine:
            ScriptEngine remoteEngine = remoteEnvironment.GetEngine(Python);
            ScriptScope remoteModule = remoteEnvironment.CreateScope();

            // get handle to a remotely created non-serializable object:
            remoteEngine.Execute(remoteModule, remoteEngine.CreateScriptSourceFromString("import lib_module", ScriptSourceKind.Statements));
            ObjectHandle handle = remoteEngine.ExecuteAndGetAsHandle(remoteModule, remoteEngine.CreateScriptSourceFromString("lib_module.bar()"));

            // get object member names:
            IList<string> members = remoteEngine.Operations.GetMemberNames(handle);

            Assert(members.IndexOf("append") != -1);
            Assert(members.IndexOf("count") != -1);
        }
#endif
        /// <summary>
        /// Level III. Module variables late binding.
        /// Used concepts: ScriptRuntime, advanced module creation API, CustomSymbolDictionary, SymbolId, execution against a module
        /// </summary>
        [Scenario]
        public static void Scenario_ConvenienceAPI_LateBinding() {
            ScriptEngine engine = Script.GetEngine(Python);

            ScriptScope scope = ScriptRuntime.GetEnvironment().CreateScope(new MyDictionary());
            engine.Execute(scope, engine.CreateScriptSourceFromString("x = Domain", ScriptSourceKind.Statements));
            object obj = engine.Execute(scope, engine.CreateScriptSourceFromString("x.FriendlyName"));

            Assert(obj as string == AppDomain.CurrentDomain.FriendlyName);
        }
        
#endif
        #endregion

        private class MyDictionary : CustomSymbolDictionary {

            private const string _domainSymbol = "Domain";

            public override string[] GetExtraKeys() {
                return new string[] { _domainSymbol };
            }

            protected override bool TrySetExtraValue(string key, object value) {
                //if (key == _domainSymbol) {
                //    throw new MemberAccessException("Domain variable is readonly");
                //}
                return false;
            }

            protected override bool TryGetExtraValue(string key, out object value) {
                if (key == _domainSymbol) {
                    value = AppDomain.CurrentDomain;
                    return true;
                }

                value = null;
                return false;
            }
        }

        [Scenario]
        public static void Coverage_CustomSymbolDictionary() {
            List<object> res = new List<object>();
            MyDictionary dict = new MyDictionary();
            MyDictionary dict2 = new MyDictionary();
            ScriptEngine engine = _env.GetEngine("py");

            AreEqual(dict.Contains(42), false);

            AssertExceptionThrown<NotImplementedException>(delegate() { dict.CopyTo(new int[] { 1, 2, 3 }, 0); });
            AssertExceptionThrown<ArgumentTypeException>(delegate() { dict.GetValueHashCode(); });
            Assert(dict.ValueEquals(dict));
            Assert(!dict.ValueEquals(new object()));
            Assert(dict.ValueEquals(dict2));
            AssertExceptionThrown<NotImplementedException>(delegate() { dict.Add(new KeyValuePair<object, object>("three", 3)); });
            AssertExceptionThrown<NotImplementedException>(delegate() { dict.Remove(new KeyValuePair<object, object>("three", 3)); });
            AssertExceptionThrown<NotImplementedException>(delegate() { dict.Contains(new KeyValuePair<object, object>("three", 3)); });
            AssertExceptionThrown<ArgumentOutOfRangeException>(delegate() { dict.CopyTo(new KeyValuePair<object, object>[] { }, 0); });
            Assert(!dict.IsReadOnly);
            dict["four"] = 4;
            dict["five"] = 5;
            Assert(dict.Contains("four"));
            AreEqual(4, dict["four"]);
            AreEqual(4, dict["four"]);
            AssertExceptionThrown<KeyNotFoundException>(delegate() { object o = dict["two"]; });
            dict.Clear();
            AssertExceptionThrown<KeyNotFoundException>(delegate() { object o = dict["two"]; });

            object key = new object();
            ((IDictionary<object, object>)dict).Add(key, 73);
            Assert(dict.Contains(key));
            AreEqual(dict[key], 73);

            ((IDictionary<object, object>)dict).Add("sixtytwo", 62);
            AreEqual(62, dict["sixtytwo"]);

            res = (List<object>)((IDictionary<object, object>)dict).Values;
            Assert(res.Contains(62));
            Assert(res.Contains(73));
            res = (List<object>)((IDictionary)dict).Values;

            ((IDictionary<object, object>)dict).Remove("sixtytwo");
            Assert(!dict.Contains("sixtytwo"));

            object key2 = new object();
            ((IDictionary)dict).Add(key2, 99);
            AreEqual(99, ((IDictionary)dict)[key2]);

            ((IDictionary<object, object>)dict).Add("sixtytwo", 62);
            Assert(dict.Contains(key2));
            Assert(dict.Contains("sixtytwo"));
            ((IDictionary)dict).Remove(key2);
            ((IDictionary)dict).Remove("sixtytwo");
            Assert(!dict.Contains(key2));
            Assert(!dict.Contains("sixtytwo"));

            ((IDictionary)dict)[key2] = 94;
            Assert(dict.Contains(key2));

            res = (List<object>)((IDictionary)dict).Keys;
            Assert(res.Contains(key2));

            object value;
            AreEqual(false, dict.TryGetValue("one", out value));
            AreEqual(null, value);

            AreEqual(false, dict.IsFixedSize);
            AreEqual(false, dict.IsReadOnly);
            AreEqual(true, dict.IsSynchronized);
            AreEqual(dict, dict.SyncRoot);
        }

        [Scenario]
        public static void Coverage_CodeGen() {
            // Hit all the bad argument cases that no real implementation should
            // First we have to get a Compiler object, this is one way...
            DynamicILGen dig = Snippets.Shared.CreateDynamicMethod(
                "__test_codegen__", typeof(Int32), new Type[] { typeof(string) }, false);

            //AssertExceptionThrown<InvalidOperationException>(delegate() { ActionBinder x = cg.Binder; });
            //AssertExceptionThrown<InvalidOperationException>(delegate() { cg.CheckAndPushTargets(null); });
            //AssertExceptionThrown<InvalidOperationException>(delegate() { cg.EmitBreak(); });
            //AssertExceptionThrown<InvalidOperationException>(delegate() { cg.EmitContinue(); });
            //AssertExceptionThrown<ArgumentNullException>(delegate() { cg.EmitConvert(null,typeof(System.String)); });
            //AssertExceptionThrown<ArgumentNullException>(delegate() { cg.EmitConvert(typeof(System.String),null); });
            //AssertExceptionThrown<ArgumentNullException>(delegate() { cg.EmitConvertFromObject(null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitBoxing(null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.DeclareLocal(null); });
            //AssertExceptionThrown<ArgumentNullException>(delegate() { cg.GetNamedLocal(null,"name"); });
            //AssertExceptionThrown<ArgumentNullException>(delegate() { cg.GetNamedLocal(typeof(System.String),null); });
            //AssertExceptionThrown<ArgumentNullException>(delegate() { cg.EmitGet(null,SymbolId.Empty,false); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitArray<int>(null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitArray(null, 3, new EmitArrayHelper(delegate(int x) { })); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitArray(typeof(String), 3, null); });
            AssertExceptionThrown<ArgumentException>(delegate() { dig.EmitArray(typeof(String), -1, new EmitArrayHelper(delegate(int x) { })); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitPropertyGet(typeof(String), null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitPropertyGet(null, "name"); });
            //#TODO - EmitPropertyGet on a readonly property
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitPropertySet(null, "name"); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitPropertySet(typeof(String), null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitPropertySet(null); });
            //#AssertExceptionThrown<ArgumentNullException>(delegate() { cg.EmitPropertySet(Type.GetType("Microsoft.Scripting.ScriptDomainManager").GetProperty("CurrentManager")); });;
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitFieldGet(null, "name"); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitFieldGet(typeof(String), null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitFieldGet(null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitFieldSet(null); });
            //AssertExceptionThrown<ArgumentNullException>(delegate() { cg.EmitNew(null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitNew(null, new Type[] { typeof(String) }); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitNew(typeof(String), null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitCall(null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitCall(null, "name"); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitCall(typeof(String), null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitCall(null, "name", new Type[] { typeof(String) }); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitCall(typeof(String), null, new Type[] { typeof(String) }); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitCall(typeof(String), "name", null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitType(null); });
            //AssertExceptionThrown<ArgumentNullException>(delegate() { cg.EmitLoadValueIndirect(null); });
            //AssertExceptionThrown<ArgumentNullException>(delegate() { cg.EmitStoreValueIndirect(null); });
            //AssertExceptionThrown<ArgumentNullException>(delegate() { cg.EmitLoadElement(null); });
            //AssertExceptionThrown<ArgumentNullException>(delegate() { cg.EmitStoreElement(null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitString(null); });
            AssertExceptionThrown<ArgumentNullException>(delegate() { dig.EmitUnbox(null); });
        }

        [Scenario]
        public static void Scenario_ThreadSafety_ModuleScriptCode() {
#if !SILVERLIGHT
            int threadCount = Environment.ProcessorCount * 3;
#else
            int threadCount = Microsoft.Silverlight.TestHostCritical.Environment.ProcessorCount * 3;
#endif
            CompiledCode code = PY.CreateScriptSourceFromString(
                "import clr\n\nprint str.Replace\nprint str.Replace\nprint str.Replace\n" +
                "print str.Replace\nprint str.Replace\nprint str.Replace\nprint str.Replace\n" +
                "print str.Replace\nprint str.Replace\nprint str.Replace\nprint str.Replace\n" +
                "print str.Replace\nprint str.Replace\nprint str.Replace\nprint str.Replace\n" +
                "print str.Replace\nprint str.Replace\nprint str.Replace\nprint str.Replace\n" +
                "print str.Replace\nprint str.Replace\nprint str.Replace\nprint str.Replace\n" +
                "print str.Replace\nprint str.Replace\nprint str.Replace\nprint str.Replace\n" +
                "print str.Replace\nprint str.Replace\nprint str.Replace\nprint str.Replace\n" +
                "print str.Replace\nprint str.Replace\nprint str.Replace\nprint str.Replace\n" +
                "print str.Replace\nprint str.Replace", SourceCodeKind.Statements).Compile();

            RedirectOutput(TextWriter.Null, delegate() {
                for (int loops = 0; loops < 10; loops++) {
                    Thread[] threads = new Thread[threadCount];
                    ScriptScope[] scopes = new ScriptScope[threadCount];

                    for (int i = 0; i < threadCount; i++) {
                        scopes[i] = _env.CreateScope();
                    }

                    for (int i = 0; i < threadCount; i++) {
                        ScriptScope scope = scopes[i];
                        threads[i] = new Thread(delegate() {
                            code.Execute(scope);
                        });
                    }

                    for (int i = 0; i < threads.Length; i++) {
                        threads[i].Start();
                    }

                    for (int i = 0; i < threads.Length; i++) {
                        threads[i].Join();
                    }
                }
            });
        }

        [Scenario]
        public static void Scenario_Contexts() {


            // TODO: globals are stored as locals in global code

            ScriptSource sourceCode = PY.CreateScriptSourceFromString(@"
def foo():
  global a
  x = 1
  y = 2
  print a

def bar():
  print 2

def on_click(sender, args):
    print 'goo'
    def g():
      print sender
    return g

a = b = 1
eval('a+b')
on_click(1,2)()
", "codebehindpy.py", SourceCodeKind.File);
            ScriptScope module = _env.CreateScope();
            CompiledCode code = sourceCode.Compile();

            RedirectOutput(TextWriter.Null, delegate() {
                code.Execute(module);
            });
        }

        [Scenario]
        public static void Scenario_ObjectOperations() {
            ObjectOpsTests(PY.Operations);
        }

        [Scenario]
        public static void Scenario_ObjectOperations_Create() {
            ObjectOpsTests(PY.CreateOperations());
        }

        public class HelperClass {
            private string _foo;
            private int _bar;

            public string Foo {
                get {
                    return _foo;
                }
                set {
                    _foo = value;
                }
            }

            public int Bar {
                get {
                    return _bar;
                }
                set {
                    _bar = value;
                }
            }
        }

        private static void ObjectOpsTests(ObjectOperations ops) {
            // mathematical operations
            ObjectOpsDoOperationTests(ops);

            // strongly typed DoOperation helpers directly accessed
            AreEqual(ops.DoOperation<int, int, object>(ExpressionType.Add, 2, 3), 5);
            AreEqual(ops.DoOperation<int, object>(ExpressionType.Negate, 2), -2);

            AreEqual(ops.IsCallable(new EventHandler(delegate(object sender, EventArgs e) { })), true);

            // get/set member
            HelperClass hc = new HelperClass();
            ops.SetMember(hc, "Foo", "abc");
            AreEqual(ops.GetMember(hc, "Foo"), "abc");

            ops.SetMember<string>(hc, "Foo", "def");
            AreEqual(ops.GetMember<string>(hc, "Foo"), "def");

            ops.SetMember(hc, "Bar", 2);
            AreEqual(ops.GetMember(hc, "Bar"), 2);

            ops.SetMember<int>(hc, "Bar", 3);
            AreEqual(ops.GetMember<int>(hc, "Bar"), 3);

            Assert(ops.ContainsMember("abc", "isalpha"));
            Assert(!ops.ContainsMember("abc", "does_not_exist"));

            IList<string> names = ops.GetMemberNames("abc");
            Assert(names.IndexOf("isalpha") != -1);

            AssertExceptionThrown<MissingMemberException>(delegate() { ops.RemoveMember("abc", "isalpha"); });

            ScriptScope mod = _env.CreateScope();
            PY.CreateScriptSourceFromString(@"
class foo(object):
    abc = 3
    bar = ['one', 'two', 'three']
# TODO (bug in Python?)
#    def __init__(self, x, y):
#        self.x = x
#        self.y = y
", SourceCodeKind.Statements).Execute(mod);

            object klass = mod.GetVariable("foo");

            // create instance of foo, verify members
            object foo = ops.CreateInstance(klass /*, 123, 444*/);
            Assert(ops.GetMember<int>(foo, "abc") == 3);
            //Assert(ops.GetMember<int>(foo, "x") == 123);
            //Assert(ops.GetMember<int>(foo, "y") == 444);

            ops.RemoveMember(klass, "abc"); // TODO: Python's remove member rule doesn't return the right true/false value.
            
            Assert(!ops.ContainsMember(klass, "abc"));

            IEnumerable<string> strEnum = ops.GetMember<IEnumerable<string>>(foo, "bar");
            List<string> list = new List<string>(strEnum);
            AreEqual(list.Count, 3);
            AreEqual(list[0], "one");
            AreEqual(list[1], "two");
            AreEqual(list[2], "three");

            // conversions
            BigInteger res;
            Assert(ops.TryConvertTo<BigInteger>(2, out res));
            AreEqual(res, BigInteger.Create(2));

            Assert(!ops.TryConvertTo<BigInteger>("2", out res));
            Assert(Object.ReferenceEquals(res, null));

            AreEqual(ops.ConvertTo<BigInteger>(2), BigInteger.Create(2));
            AssertExceptionThrown<ArgumentTypeException>(delegate() { ops.ConvertTo<BigInteger>("2"); });

            // non-generic conversion helpers
            object objres;
            Assert(ops.TryConvertTo(2, typeof(BigInteger), out objres));
            AreEqual(objres, BigInteger.Create(2));

            Assert(!ops.TryConvertTo("2", typeof(BigInteger), out objres));
            Assert(Object.ReferenceEquals(objres, null));

            AreEqual(ops.ConvertTo(2, typeof(BigInteger)), BigInteger.Create(2));
            AssertExceptionThrown<ArgumentTypeException>(delegate() { ops.ConvertTo("2", typeof(BigInteger)); });

            // exercise the caching / cache clearing...
            foreach (string attr in new string[] { "isalpha", "count", "decode", "encode", "endswith", "expandtabs", "find", "index", "isalnum", "isalpha", "isdecimal", "isdigit", "islower", "isnumeric", "isspace" }) {
                object value;
                Assert(ops.TryGetMember("abc", "isalpha", out value));
                Assert(value != null);
            }

            // force lots of accesses to test clearing the site cache
            for (int i = 0; i < 100; i++) {
                object value;
                Assert(!ops.TryGetMember("abc", "xyz" + i.ToString(), out value));
                Assert(value == null);
            }

            for (int i = 0; i < 100; i++) {
                // force a bunch of operations to be used multiple times...
                ObjectOpsDoOperationTests(ops);
            }

            // then force lots of new sites to be created...
            for (int i = 0; i < 100; i++) {
                object value;
                Assert(!ops.TryGetMember("abc", "abc" + i.ToString(), out value));
                Assert(value == null);
            }
        }

        private static void ObjectOpsDoOperationTests(ObjectOperations ops) {
            AreEqual(ops.Add(1, 2), 3);
            AreEqual(ops.Subtract(2, 1), 1);
            AreEqual(ops.Power(2, 4), 16);
            AreEqual(ops.Multiply(2, 3), 6);
            AreEqual(ops.Divide(6, 2), 3);
            AreEqual(ops.Modulo(3, 2), 1);
            AreEqual(ops.LeftShift(2, 2), 8);
            AreEqual(ops.RightShift(8, 2), 2);
            AreEqual(ops.BitwiseAnd(3, 2), 2);
            AreEqual(ops.BitwiseOr(2, 1), 3);
            AreEqual(ops.ExclusiveOr(2, 3), 1);
            AreEqual(ops.LessThan(5, 2), false);
            AreEqual(ops.LessThan(2, 5), true);
            AreEqual(ops.GreaterThan(5, 2), true);
            AreEqual(ops.GreaterThan(2, 5), false);
            AreEqual(ops.LessThanOrEqual(2, 2), true);
            AreEqual(ops.LessThanOrEqual(5, 2), false);
            AreEqual(ops.LessThanOrEqual(2, 5), true);
            AreEqual(ops.GreaterThanOrEqual(2, 2), true);
            AreEqual(ops.GreaterThanOrEqual(5, 2), true);
            AreEqual(ops.GreaterThanOrEqual(2, 5), false);
            AreEqual(ops.Equal(2, 2), true);
            AreEqual(ops.Equal(2, 3), false);
            AreEqual(ops.NotEqual(2, 2), false);
            AreEqual(ops.NotEqual(2, 3), true);

        }

        [Scenario]
        public static void Scenario_ScriptEngine_CreateScriptSource() {
            ScriptRuntime env = CreateRuntime();
            List<ScriptSource> sources = new List<ScriptSource>();
            ScriptScope scope = null;

            string dir = Path.GetTempPath();

            //@TODO - Other languages
            ScriptEngine pyEng = (ScriptEngine)env.GetEngine("python");

            //Script code to execute that will alter a scope in known ways
            string pyCode = @"
def increment(arg):
    local = arg + 1
    local2 = local
    del local2
    return local

global1 = increment(3)
global2 = global1
del global2";

            try {
                //Write our script out to files of various encodings and filenames
                File.WriteAllText(dir + "DefaultScriptSource.py", pyCode, Encoding.Default);
                File.WriteAllText(dir + "BigEndianUnicodeScriptSource.py", pyCode, Encoding.BigEndianUnicode);
                File.WriteAllText(dir + "UnicodeScriptSource.py", pyCode, Encoding.Unicode);
                File.WriteAllText(dir + "UTF32ScriptSource.py", pyCode, Encoding.UTF32);
                //File.WriteAllText(dir + "UTF7ScriptSource.py", pyCode, Encoding.UTF7);  The UTF7 source is written out oddly and seems invalid
                File.WriteAllText(dir + "UTF8ScriptSource.py", pyCode, Encoding.UTF8);
                File.WriteAllText(dir + "UnicodeScriptSource.xx", pyCode, Encoding.Unicode);    //Ensure there's no check of the extension

                //CreateScriptSourceFromFile positive cases
                //@TODO - FromFile with a Kind other than File
#if !SILVERLIGHT
                sources.Add(pyEng.CreateScriptSourceFromFile(dir + "DefaultScriptSource.py"));
#else
                sources.Add(pyEng.CreateScriptSourceFromFile(dir + "UTF8ScriptSource.py");
#endif
                sources.Add(pyEng.CreateScriptSourceFromFile(dir + "DefaultScriptSource.py", Encoding.Default));
                sources.Add(pyEng.CreateScriptSourceFromFile(dir + "BigEndianUnicodeScriptSource.py", Encoding.BigEndianUnicode));
                sources.Add(pyEng.CreateScriptSourceFromFile(dir + "UnicodeScriptSource.py", Encoding.Unicode));
                sources.Add(pyEng.CreateScriptSourceFromFile(dir + "UTF32ScriptSource.py", Encoding.UTF32));
                //sources.Add(pyEng.CreateScriptSourceFromFile(dir + "UTF7ScriptSource.py", Encoding.UTF7));
                sources.Add(pyEng.CreateScriptSourceFromFile(dir + "UTF8ScriptSource.py", Encoding.UTF8));
                sources.Add(pyEng.CreateScriptSourceFromFile(dir + "UnicodeScriptSource.xx", Encoding.Unicode));

                //CreateScriptSourceFromString positive cases
                sources.Add(pyEng.CreateScriptSourceFromString(pyCode, SourceCodeKind.Statements));
                sources.Add(pyEng.CreateScriptSourceFromString(pyCode, "customId", SourceCodeKind.Statements));

                //@TODO - CreateScriptSource(...) overloads

                foreach (ScriptSource src in sources) {
                    //Console.WriteLine("  Testing src: '{0}'", src.Id);

                    //Exec the code in a new scope
                    scope = env.CreateScope();
                    AreEqual(null, src.Execute(scope));
                    AreEqual(pyCode, src.GetCode());

                    //Verify the scope's contents after execution
                    Assert(!scope.ContainsVariable("local1"));
                    Assert(!scope.ContainsVariable("local2"));
                    Assert(!scope.ContainsVariable("global2"));
                    Assert(scope.ContainsVariable("increment"));
                    AreEqual(4, scope.GetVariable<int>("global1"));

                    //Add the identifier to the scope
                    scope.SetVariable("srcid", src.Path);

                    //And while we're here, verify ScriptEngine.GetScope()
                    /*if (src.Id != null) { @TODO - Doesn't work quite as expected, need some spec clarification
                        ScriptScope scope2 = pyEng.GetScope(src.Id);
                        Assert(scope2 != null, "ScriptEngine.GetScope failed to find an existing scope");
                        AreEqual(src.Id, scope2.GetVariable<string>("srcid"));
                    }*/

                    //Console.WriteLine("    Pass");
                }

                AssertExceptionThrown<ArgumentNullException>(delegate() {
                    pyEng.GetScope(null);
                });

            } finally {
                File.Delete(dir + "DefaultScriptSource.py");
                File.Delete(dir + "BigEndianUnicodeScriptSource.py");
                File.Delete(dir + "UnicodeScriptSource.py");
                File.Delete(dir + "UTF32ScriptSource.py");
                //File.Delete(dir + "UTF7ScriptSource.py");
                File.Delete(dir + "UTF8ScriptSource.py");
                File.Delete(dir + "UnicodeScriptSource.xx");
            }

            //Ensure that creating a ScriptSource doesn't trigger any parsing or execution of code
            //by constructing ScriptSource objects over an invalid code snippet.
            pyCode = "This is nonsense.";
            sources.Clear();

            try {
                File.WriteAllText(dir + "UnicodeScriptSource2.py", pyCode, Encoding.Unicode);
                sources.Add(pyEng.CreateScriptSourceFromFile(dir + "UnicodeScriptSource2.py", Encoding.Unicode));
                sources.Add(pyEng.CreateScriptSourceFromString(pyCode, "customId2", SourceCodeKind.Statements));

                foreach (ScriptSource src in sources) {
                    //Console.WriteLine("  Testing src: '{0}'", src.Id);

                    AreEqual(pyCode, src.GetCode());
                    scope = env.CreateScope();

                    AssertExceptionThrown<SyntaxErrorException>(delegate() {
                        src.Execute(scope);
                    });

                    //Console.WriteLine("    Pass");
                }
            } finally {
                File.Delete(dir + "UnicodeScriptSource2.py");
            }

            //Negative construction cases
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSourceFromFile(null);
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSourceFromFile(null, Encoding.Unicode);
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSourceFromFile("some_path.py", null);
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSourceFromFile(null, Encoding.Unicode, SourceCodeKind.File);
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSourceFromFile("some_path.py", null, SourceCodeKind.File);
            });

            // ScriptSource creation doesn't check whether the file exists, is readable, or the path is valid:
            File.Delete(dir + "non_existent_file.nop"); //Just to make absolutely sure
            Assert(pyEng.CreateScriptSourceFromFile(dir + "non_existent_file.nop") != null);
            Assert(pyEng.CreateScriptSourceFromFile("invalid?file|<name>") != null);
            Assert(pyEng.CreateScriptSourceFromFile(dir) != null);
            //SourceUnit src2 = pyEng.CreateScriptSourceFromFile(dir + "deny.txt");
            //pyEng.Execute(env.CreateScope(), src2);

            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSourceFromString(null);
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSourceFromString(null, SourceCodeKind.Statements);
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSourceFromString(null, "id");
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSourceFromString(null, "id", SourceCodeKind.Statements);
            });

            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSource(null);
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSource((CodeObject)null, SourceCodeKind.Statements);
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSource((CodeObject)null, "some_path.py");
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSource((StreamContentProvider)null, "some_path.py");
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSource((CodeObject)null, "some_path.py", SourceCodeKind.Statements);
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSource((StreamContentProvider)null, "some_path.py", Encoding.Unicode);
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSource((TextContentProvider)null, "some_path.py", SourceCodeKind.Statements);
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                pyEng.CreateScriptSource((StreamContentProvider)null, "some_path.py", Encoding.Unicode, SourceCodeKind.Statements);
            });
        }

        [Scenario]
        public static void Scenario_ScriptEngine_ScopeManipulation() {
            ScriptRuntime env = CreateRuntime();
            ScriptEngine pyEng = (ScriptEngine)env.GetEngine("python");

            ScriptScope scope = (ScriptScope)env.CreateScope();

            /////////////////////////////////////////////////////////////
            //SetVariable(ScriptScope scope, string, name, object value)

            //Negative argument cases
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                scope.SetVariable(null, 3);
            });

            //Scope bound to the current engine, new name
            scope = (ScriptScope)pyEng.CreateScope();
            Assert(!scope.ContainsVariable("var1"), "newly created scope is not empty?");
            scope.SetVariable("var1", 9);
            AreEqual(9, scope.GetVariable<int>("var1"));

            //Scope bound to the current engine, existing name, new value
            scope.SetVariable("var1", 12);
            AreEqual(12, scope.GetVariable<int>("var1"));

            ///////////////////////////////////////////////////////////////////
            //GetVariable(ScriptScope scope, string name)
            //TryGetVariable(ScriptScope scope, string name, out object value)

            object value;

            //Negative argument cases
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                scope.GetVariable(null);
            });
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                scope.TryGetVariable(null, out value);
            });

            //Non-existent name
            scope = (ScriptScope)pyEng.CreateScope();
            AssertExceptionThrown<MissingMemberException>(delegate() {
                scope.GetVariable("var1");
            });
            Assert(!scope.TryGetVariable("var1", out value));
            Assert(value == null);

            //Scope bound to this engine, existing name
            scope.SetVariable("var1", 16);
            AreEqual(16, (int)scope.GetVariable("var1"));
            Assert(scope.TryGetVariable("var1", out value));
            AreEqual(16, (int)value);

            //Scope bound to no engine, existing name
            scope = (ScriptScope)env.CreateScope();
            scope.SetVariable("var1", 19);
            AreEqual(19, (int)scope.GetVariable("var1"));
            Assert(scope.TryGetVariable("var1", out value));
            AreEqual(19, (int)value);

            //////////////////////////////////////////////
            //RemoveVariable(ScriptScope scope, string name)

            //Negative argument cases
            AssertExceptionThrown<ArgumentNullException>(delegate() {
                scope.RemoveVariable(null);
            });

            //Remove a non-existent name
            scope = (ScriptScope)pyEng.CreateScope();
            Assert(!scope.ContainsVariable("var1"));
            Assert(!scope.RemoveVariable("var1"));

            //Scope bound to this engine, existing name
            scope.SetVariable("var1", 32);
            Assert(scope.ContainsVariable("var1"));
            Assert(scope.RemoveVariable("var1"));
            Assert(!scope.ContainsVariable("var1"));

            //@TODO - A scenario where the variable cannot be removed

            //@BUGBUG - ScriptEngine.ClearVariables doesn't exist
            //@BUGBUG - ScriptEngine.GetVariableNames doesn't exist

            //@TODO - GetVariable<T>, TryGetVariable<T>, ObjectHandle overloads all around

        }

        [Scenario]
        public static void Scenario_Name_Resolution_Precedence() {
            ScriptRuntime env = CreateRuntime();
            ScriptScope scope = env.CreateScope();
            ScriptEngine eng = env.GetEngine("py");

            //Global
            env.Globals.SetVariable("variable", 1); //?

            //Scope
            scope.SetVariable("variable", 2);

            //Class + Closure + Local + Global
            string pyCode = @"
if variable!=2:
    raise Exception('1 - Lookup precendence mismatch, variable==%i, expected 2' % variable)

variable = 3

class C(object):
    def __init__(self):
        self.variable = 4
    def findlocal(self):
        variable = 5
        if variable!=5:
            raise Exception('2 - Lookup precendence mismatch, variable==%i, expected 5' % variable)
    #@TODO - Languages with an implicit 'this' could have a findinstance or findclass
    #@TODO - Closure as well
    def findglobal(self):
        if variable!=3:
            raise Exception('3 - Lookup precendence mismatch, variable==%i, expected 3' % variable)

c = C()
c.findlocal()
c.findglobal()
del variable

#if variable!=1:
#    raise Exception('4 - Lookup precendence mismatch, variable==%i, expected 1' % variable)
";

            ScriptSource src = eng.CreateScriptSourceFromString(pyCode, SourceCodeKind.Statements);
            src.Execute(scope);

            Assert(!scope.ContainsVariable("variable"));
            Assert(env.Globals.ContainsVariable("variable"));
            AreEqual(1, env.Globals.GetVariable<int>("variable"));
        }

        [Scenario]
        public static void Scenario_ScriptRuntime_Multiple() {
            List<ScriptRuntime> envs = new List<ScriptRuntime>();
            List<ScriptScope> scopes = new List<ScriptScope>();
            ScriptEngine eng = null;
            ScriptSource src = null;

            //Create several ScriptRuntime objects in this appdomain
            for (int i = 0; i < 20; i++) {
                envs.Add(CreateRuntime());
                scopes.Add(envs[i].CreateScope());
            }

            //For each one that we created, execute the same bit of code
            //ensuring that executing code in one runtime doesn't impact others
            for (int i = 0; i < envs.Count; i++) {
                Assert(!envs[i].Globals.ContainsVariable("globalvar"));
                envs[i].Globals.SetVariable("globalvar", i);
                eng = (ScriptEngine)envs[i].GetEngine("py");
                src = eng.CreateScriptSourceFromString(String.Format("scopevar = {0}", i), SourceCodeKind.Statements);
                Assert(!scopes[i].ContainsVariable("scopevar"));
                Assert(null == src.Execute(scopes[i]));
                AreEqual(i, envs[i].Globals.GetVariable<Int32>("globalvar"));
                AreEqual(i, scopes[i].GetVariable<Int32>("scopevar"));

                //Quick reference check to ensure we're actually getting different runtimes
                if (i > 0) {
                    Assert(envs[i - 1] != envs[i]);
                }
            }

            //Using scopes from other runtimes, same language
            eng = (ScriptEngine)envs[0].GetEngine("py");
            scopes[1] = envs[1].CreateScope();
            src = eng.CreateScriptSourceFromString("scopevar = 'ABC'", SourceCodeKind.Statements);
            src.Execute(scopes[1]);
            AreEqual("ABC", scopes[1].GetVariable<string>("scopevar"));

            //Using sourceunits from other runtimes, same language
            eng = (ScriptEngine)envs[1].GetEngine("py");
            scopes[1] = envs[1].CreateScope();
            Assert(!scopes[1].ContainsVariable("scopevar"));
            src.Execute(scopes[1]);
            AreEqual("ABC", scopes[1].GetVariable<string>("scopevar"));
        }

        #region test actions

        public class BindingException : Exception {
        }

        class TestGetMemberBinder : GetMemberBinder {
            internal TestGetMemberBinder(string name)
                : base(name, false) {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject self, DynamicMetaObject onBindingError) {
                return onBindingError ?? new DynamicMetaObject(Expression.Throw(Expression.New(typeof(BindingException)), typeof(object)), self.Restrictions);
            }
        }

        class TestSetMemberBinder : SetMemberBinder {
            internal TestSetMemberBinder(string name)
                : base(name, false) {
            }

            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject self, DynamicMetaObject value, DynamicMetaObject onBindingError) {
                return onBindingError ?? new DynamicMetaObject(Expression.Throw(Expression.New(typeof(BindingException)), typeof(object)), self.Restrictions.Merge(value.Restrictions));
            }
        }

        class TestDeleteMemberBinder : DeleteMemberBinder {
            internal TestDeleteMemberBinder(string name)
                : base(name, false) {
            }

            public override DynamicMetaObject FallbackDeleteMember(DynamicMetaObject self, DynamicMetaObject onBindingError) {
                return onBindingError ?? new DynamicMetaObject(Expression.Throw(Expression.New(typeof(BindingException))), self.Restrictions);
            }
        }

        class TestCallBinder : InvokeMemberBinder {
            internal TestCallBinder(string name)
                : base(name, false, new CallInfo(0)) {
            }

            public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
                return onBindingError ?? new DynamicMetaObject(Expression.Throw(Expression.New(typeof(BindingException)), typeof(object)), target.Restrictions.Merge(BindingRestrictions.Combine(args)));
            }

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
                return new TestInvokeBinder().FallbackInvoke(target, args, onBindingError);
            }
        }

        class TestInvokeBinder : InvokeBinder {
            public TestInvokeBinder()
                : base(new CallInfo(0)) {
            }

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject onBindingError) {
                if (target.NeedsDeferral()) {
                    var deferArgs = new DynamicMetaObject[args.Length + 1];
                    deferArgs[0] = target;
                    args.CopyTo(deferArgs, 1);
                    return Defer(deferArgs);
                }
                // Note: we could just return one expression with a conditional
                // but doing it this way more accurately simulates a normal
                // binder (produces seperate rules for error and success cases)
                if (target.RuntimeType.IsSubclassOf(typeof(Delegate))) {
                    var exprs = DynamicUtils.GetExpressions(args);
                    for (int i = 0, n = args.Length; i < n; i++) {
                        exprs[i] = Expression.Convert(exprs[i], typeof(object));
                    }

                    return new DynamicMetaObject(
                        Expression.Call(
                            Expression.Convert(target.Expression, typeof(Delegate)),
                            typeof(Delegate).GetMethod("DynamicInvoke"),
                            Expression.NewArrayInit(typeof(object), exprs)
                        ),
                        target.Restrictions.Merge(BindingRestrictions.Combine(args)).Merge(
                            BindingRestrictions.GetExpressionRestriction(Expression.TypeIs(target.Expression, typeof(Delegate)))
                        )
                    );
                }
                return onBindingError ?? new DynamicMetaObject(
                    Expression.Throw(Expression.New(typeof(BindingException)), typeof(object)),
                    target.Restrictions.Merge(BindingRestrictions.Combine(args)).Merge(
                        BindingRestrictions.GetExpressionRestriction(Expression.Not(Expression.TypeIs(target.Expression, typeof(Delegate))))
                    )
                );
            }
        }

        #endregion

        [Scenario]
        private static void Scenario_ScriptScopeIDynamicObject() {
            var scope = _env.CreateScope();

            var setSite = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMemberBinder("foo"));
            var getSite = CallSite<Func<CallSite, object, object>>.Create(new TestGetMemberBinder("foo"));
            var callSite = CallSite<Func<CallSite, object, int, int, object>>.Create(new TestCallBinder("foo"));
            var deleteSite = CallSite<Action<CallSite, object>>.Create(new TestDeleteMemberBinder("foo"));

            AssertExceptionThrown<BindingException>(() => getSite.Target(getSite, scope));
            AssertExceptionThrown<BindingException>(() => deleteSite.Target(deleteSite, scope));
            AssertExceptionThrown<BindingException>(() => callSite.Target(callSite, scope, 123, 444));

            setSite.Target(setSite, scope, "abc");
            Assert(getSite.Target(getSite, scope) == (object)"abc");
            AssertExceptionThrown<BindingException>(() => callSite.Target(callSite, scope, 123, 444));
            deleteSite.Target(callSite, scope);
            AssertExceptionThrown<BindingException>(() => getSite.Target(getSite, scope));

            Func<int, int, int> value = (x, y) => x + y;
            setSite.Target(setSite, scope, value);
            Assert(callSite.Target(callSite, scope, 123, 444).Equals(567));
            Assert(getSite.Target(getSite, scope) == (object)value);
            deleteSite.Target(callSite, scope);
            AssertExceptionThrown<BindingException>(() => getSite.Target(getSite, scope));
        }

        [Scenario]
        private static void Scenario_Expando() {
            var ops = PY.Operations;

            // Get/Set/Delete against an empty expando object, checking the transition from
            // empty to non-empty.
            ExpandoObject expando = new ExpandoObject();

            AssertExceptionThrown<MissingMemberException>(() => ops.GetMember(expando, "DoesNotExist"));
            AssertExceptionThrown<MissingMemberException>(() => ops.RemoveMember(expando, "DoesNotExist"));

            expando = new ExpandoObject();
            ops.SetMember(expando, "OtherDoesNotExist", 42);
            AreEqual(ops.GetMember(expando, "OtherDoesNotExist"), 42);

            // exercise growing the expando object alphabetically...
            expando = new ExpandoObject();
            for (int i = 0; i < 25; i++) {
                string name = "forward" + i.ToString("00");
                
                ops.SetMember(expando, name, i);
                AreEqual(ops.GetMember(expando, name), i);
            }

            // exercise growing the expando object reverse alphabetically...
            expando = new ExpandoObject();
            for (int i = 25; i >= 0; i--) {
                string name = "reverse" + i.ToString("00");
                
                ops.SetMember(expando, name, i);
                AreEqual(ops.GetMember(expando, name), i);
            }

            // exercise forward and then inserting in the middle...
            expando = new ExpandoObject();
            for (int i = 0; i < 25; i+=2) {
                string name = "forward2" + i.ToString("00");

                ops.SetMember(expando, name, i);
                AreEqual(ops.GetMember(expando, name), i);
            }

            for (int i = 1; i < 25; i += 2) {
                string name = "forward2" + i.ToString("00");

                ops.SetMember(expando, name, i);
                AreEqual(ops.GetMember(expando, name), i);
            }

            // exercise growing the expando object reverse alphabetically and
            // and inserting in the middle
            expando = new ExpandoObject();
            for (int i = 25; i >= 0; i-= 2) {
                string name = "reverse2" + i.ToString("00");

                ops.SetMember(expando, name, i);
                AreEqual(ops.GetMember(expando, name), i);
            }

            for (int i = 24; i >= 0; i -= 2) {
                string name = "reverse2" + i.ToString("00");

                ops.SetMember(expando, name, i);
                AreEqual(ops.GetMember(expando, name), i);
            }

            // exercise growing the expando object alphabetically and inserting
            // at the beginning and very end
            expando = new ExpandoObject();
            for (int i = 0; i < 25; i++) {
                string name = "startend" + i.ToString("00");

                ops.SetMember(expando, name, i);
                AreEqual(ops.GetMember(expando, name), i);
            }

            ops.SetMember(expando, "aaa", "aaa");
            ops.SetMember(expando, "zzz", "zzz");

            for (int i = 0; i < 25; i++) {
                string name = "startend" + i.ToString("00");
                AreEqual(ops.GetMember(expando, name), i);
            }
            AreEqual(ops.GetMember(expando, "aaa"), "aaa");
            AreEqual(ops.GetMember(expando, "zzz"), "zzz");

            // add/remove values
            expando = new ExpandoObject();
            ops.SetMember(expando, "addremove", 42);
            AreEqual(ops.GetMember(expando, "addremove"), 42);
            ops.RemoveMember(expando, "addremove");

            AssertExceptionThrown<MissingMemberException>(() => ops.GetMember(expando, "addremove"));
            AssertExceptionThrown<MissingMemberException>(() => ops.RemoveMember(expando, "addremove"));

            ops.SetMember(expando, "addremove", "abc");
            AreEqual(ops.GetMember(expando, "addremove"), "abc");

            // try to get before accessing, should be able to successfully access later
            AssertExceptionThrown<MissingMemberException>(() => ops.GetMember(expando, "AddRemoveNotYetAdded"));
            ops.SetMember(expando, "AddRemoveNotYetAdded", 42);
            AreEqual(ops.GetMember(expando, "AddRemoveNotYetAdded"), 42);

            // case insensitive access
            expando = new ExpandoObject();
            ops.SetMember(expando, "caseInsensitive", 42, true);
            // TODO: needs a case insensitive language
            //AreEqual(ops.GetMember(expando, "CaSeInSenSitIve", true), 42);
            //Set a member case-insensitively will overwrite the existing matching member
            //ops.SetMember(expando, "CaSeInSenSitIve", 23, true);
            //AreEqual(ops.GetMember(expando, "caseInsensitive", true), 23);
            //AreEqual(ops.GetMember(expando, "CaSeInSenSitIve", true), 23);
            //ops.SetMember(expando, "caseInsensitive", 42, true);
            //ops.SetMember(expando, "CaSeInSenSitIve", 23, false);
            //AssertExceptionThrown<AmbiguousMatchException>(() => ops.GetMember(expando, "caseInsensitive", true));
            //AssertExceptionThrown<AmbiguousMatchException>(() => ops.GetMember(expando, "CaSeInSenSitIve", true));
            //AssertExceptionThrown<AmbiguousMatchException>(() => ops.GetMember(expando, "CaSEInSenSitIve", true));
            //case sensitive deleting results in missing member exception.
            //AssertExceptionThrown<MissingMemberException>(() => ops.RemoveMember(expando, "caseInsensitivE"));
            //AssertExceptionThrown<AmbiguousMatchException>(() => ops.ContainsMember(expando, "caseInsensitive", true));
            //AssertExceptionThrown<AmbiguousMatchException>(() => ops.ContainsMember(expando, "CaSeInSenSitIve", true));
            //AssertExceptionThrown<AmbiguousMatchException>(() => ops.ContainsMember(expando, "caseInsensitivE", true));
            //AreEqual(ops.ContainsMember(expando, "caseInsensitivE", false), false);

            // verify some internal implementation details... we should get approrpiate class sharing
            ExpandoObject one = new ExpandoObject();
            ExpandoObject two = new ExpandoObject();

            AreEqual(GetExpandoClass(one), GetExpandoClass(two));

            // add a new field to both...
            ops.SetMember(one, "ShareTest01", 42);
            ops.SetMember(two, "ShareTest01", "abc");

            AreEqual(GetExpandoClass(one), GetExpandoClass(two));

            // and another....
            ops.SetMember(one, "ShareTest02", 42);
            ops.SetMember(two, "ShareTest02", "abc");

            AreEqual(GetExpandoClass(one), GetExpandoClass(two));

            // add another w/ space to insert in the middle
            ops.SetMember(one, "ShareTest04", 42);
            ops.SetMember(two, "ShareTest04", "abc");

            AreEqual(GetExpandoClass(one), GetExpandoClass(two));

            // insert at the beginning
            ops.SetMember(one, "AAShareTest", 42);
            ops.SetMember(two, "AAShareTest", "abc");

            AreEqual(GetExpandoClass(one), GetExpandoClass(two));

            // insert at the end
            ops.SetMember(one, "ZZShareTest", 42);
            ops.SetMember(two, "ZZShareTest", "abc");

            AreEqual(GetExpandoClass(one), GetExpandoClass(two));

            // insert in the middle
            ops.SetMember(one, "ShareTest03", 42);
            ops.SetMember(two, "ShareTest03", "abc");

            AreEqual(GetExpandoClass(one), GetExpandoClass(two));

            // GetMemberNames tests
            expando = new ExpandoObject();

            ops.SetMember(expando, "foo", 42);
            IList<string> names = ops.GetMemberNames(expando);
            AreEqual(names.Count, 16);
            AreEqual(names[names.Count - 1], "foo");

            ops.SetMember(expando, "bar", 42);
            names = ops.GetMemberNames(expando);
            AreEqual(names.Count, 17);
            AreEqual(names[names.Count - 1], "foo");
            AreEqual(names[names.Count - 2], "bar");

            // thread safety, multiple threads adding to the same object.
            // All values should be added
            expando = new ExpandoObject();

            Thread t1 = new Thread(() => ExpandoThreadAdder(ops, expando, "Thread1_"));
            Thread t2 = new Thread(() => ExpandoThreadAdder(ops, expando, "Thread2_"));
            Thread t3 = new Thread(() => ExpandoThreadAdder(ops, expando, "Thread3_"));
            Thread t4 = new Thread(() => ExpandoThreadAdder(ops, expando, "Thread4_"));

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
                    AreEqual(ops.GetMember(expando, "Thread" + (i+1) + "_" + j.ToString("0000")), j);
                }
            }

            t1 = new Thread(() => ExpandoThreadAdderRemover(ops, expando, "Thread1_"));
            t2 = new Thread(() => ExpandoThreadAdderRemover(ops, expando, "Thread2_"));
            t3 = new Thread(() => ExpandoThreadAdderRemover(ops, expando, "Thread3_"));
            t4 = new Thread(() => ExpandoThreadAdderRemover(ops, expando, "Thread4_"));

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
                    AreEqual(ops.ContainsMember(expando, "Thread" + (i + 1) + "_" + j.ToString("0000")), false);
                }
            }            
        }

        private static void ExpandoThreadAdder(ObjectOperations ops, ExpandoObject self, string name) {
            for (int i = 0; i < 1000; i++) {
                string setname = name + i.ToString("0000");
                ops.SetMember(self, setname, i);
            }
        }

        private static void ExpandoThreadAdderRemover(ObjectOperations ops, ExpandoObject self, string name) {
            for (int i = 0; i < 1000; i++) {
                string setname = name + i.ToString("0000");
                ops.SetMember(self, setname, i);
                ops.RemoveMember(self, setname);
            }
        }

        private static object GetExpandoClass(ExpandoObject one) {
            object val = one.GetType().GetField("_data", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(one);
            return val.GetType().GetField("Class", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(val);
        }

        #region Dynamic Subclasses

        public class EmptyDynamic : DynamicObject {
            private string _value = "Empty";

            public string Value {
                get {
                    return _value;
                }
                set {
                    _value = value;
                }
            }

            public static implicit operator int(EmptyDynamic self) {
                return 42;
            }

            public static string operator +(EmptyDynamic self, string value) {
                return self._value + value;
            }
        }

        class NormalDynamic : DynamicObject {
            private object _value = "def";

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
                result = ArrayUtils.Insert("InvokeMember", args);
                return true;
            }

            public override bool TryConvert(ConvertBinder binder, out object result) {
                result = "Hello";
                return true;
            }

            public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result) {
                result = ArrayUtils.Insert("CreateInstance", args);
                return true;
            }

            public override bool TryDeleteMember(DeleteMemberBinder binder) {
                if (binder.Name == "foo") {
                    return true;
                }
                return false;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result) {
                if (binder.Name == "foo") {
                    result = "abc";
                } else if (binder.Name == "bar") {
                    result = _value;
                } else {
                    result = "def";
                }
                return true;
            }

            public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
                result = ArrayUtils.Insert("Invoke", args);
                return true;
            }

            public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {
                result = new object[] { binder.Operation, arg };
                return true;
            }

            public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result) {
                result = binder.Operation;
                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value) {
                if (binder.Name == "foo") {
                    throw new InvalidOperationException();
                } else if (binder.Name == "bar") {
                    _value = value;
                }
                return true;
            }
        }

        public class BadDynamic1 : DynamicObject {
            public bool TryGetMember(DynamicMetaObjectBinder binder, out object result) {
                throw new InvalidOperationException("shouldn't be called");
            }
        }

        public class BadDynamic2 : DynamicObject {
            public bool TryGetMember(object binder, out object result) {
                throw new InvalidOperationException("shouldn't be called");
            }
            public bool TryGetMember(DynamicMetaObjectBinder binder, out object result) {
                throw new InvalidOperationException("shouldn't be called");
            }
        }

        public class BadDynamic3 : DynamicObject {

            public override bool TryGetMember(GetMemberBinder binder, out object result) {
                result = 42;
                return true;
            }

            public bool TryGetMember(DynamicMetaObjectBinder binder, out object result) {
                throw new InvalidOperationException("shouldn't be called");
            }
        }

        #endregion

        // Disabled: Python doesn't implement errorSuggestion on its Fallback methods
        // Moving this into AstTest where we can test it directly
        //[Scenario]
        private static void Scenario_Dynamic() {
            TestNormalDynamic();
            TestEmptyDynamic();
            TestBadDynamic();
        }

        /// <summary>
        /// Tests a Dynamic subclass that overrides nothing.  We should fallback 
        /// to the binder for each action so normal .NET interop should function.
        /// </summary>
        private static void TestEmptyDynamic() {
            var ops = PY.Operations;

            EmptyDynamic ed = new EmptyDynamic();

            // GetMember
            AreEqual(ops.GetMember(ed, "Value"), "Empty");
            AssertExceptionThrown<MissingMemberException>(() => ops.GetMember(ed, "DoesNotExist"));

            // SetMember
            ops.SetMember(ed, "Value", "baz");
            AreEqual(ops.GetMember(ed, "Value"), "baz");

            // DeleteMember
            AssertExceptionThrown<MissingMemberException>(() => ops.RemoveMember(ed, "Value"));
            AssertExceptionThrown<MissingMemberException>(() => ops.RemoveMember(ed, "bar"));

            // Convert
            AssertExceptionThrown<ArgumentTypeException>(() => ops.ConvertTo(ed, typeof(string)));
            AreEqual(ops.ConvertTo(ed, typeof(int)), 42);

            // Operation
            AssertExceptionThrown<ArgumentTypeException>(() => ops.Add(ed, 42));
            AreEqual(ops.Add(ed, "Foo"), "bazFoo");

            // Invoke
            AssertExceptionThrown<ArgumentTypeException>(() => ops.Invoke(ed, "abc"));   // ObjectOps.Call == Invoke

            // Create
            AssertExceptionThrown<ArgumentTypeException>(() => ops.CreateInstance(ed, "abc"));   

            // Call, not supported by ObjectOps or properly by any languages
            //ScriptSource src = RB.CreateScriptSourceFromString("ed.foo", SourceCodeKind.Expression);
            //ScriptScope scope = RB.CreateScope();
            //scope.SetVariable("ed", ed);
            //src.Execute(scope);
        }

        /// <summary>
        /// Test cases where the subclass has multiple or different methods
        /// </summary>
        private static void TestBadDynamic() {
            var ops = PY.Operations;

            BadDynamic1 bad1 = new BadDynamic1();
            BadDynamic2 bad2 = new BadDynamic2();
            BadDynamic3 bad3 = new BadDynamic3();

            AssertExceptionThrown<MissingMemberException>(() => ops.GetMember(bad1, "DoesNotExist"));
            AssertExceptionThrown<MissingMemberException>(() => ops.GetMember(bad2, "DoesNotExist"));
            AreEqual(ops.GetMember(bad3, "DoesNotExist"), 42);
        }

        /// <summary>
        /// Tests a Dynamic subclass that overrides everything.  We shouldn't fallback to the 
        /// action so we should always get our custom behavior.  The type does not need to be
        /// public because the virtual methods on the base class are public.
        /// </summary>
        private static void TestNormalDynamic() {
            var ops = PY.Operations;

            NormalDynamic nd = new NormalDynamic();
            // Invoke
            object[] res = (object[])ops.Invoke(nd, "abc");   // ObjectOps.Call == Invoke

            AreEqual(res[0], "Invoke");
            AreEqual(res[1], "abc");

            // GetMember
            AreEqual(ops.GetMember(nd, "foo"), "abc");
            AreEqual(ops.GetMember(nd, "bar"), "def");

            // DeleteMember
            ops.RemoveMember(nd, "foo");
            ops.RemoveMember(nd, "bar");

            // SetMember
            AssertExceptionThrown<InvalidOperationException>(() => ops.SetMember(nd, "foo", "baz"));
            ops.SetMember(nd, "bar", "abc");
            AreEqual(ops.GetMember(nd, "bar"), "abc");

            // CreateInstance
            res = (object[])ops.CreateInstance(nd, "foo");
            AreEqual(res[0], "CreateInstance");
            AreEqual(res[1], "foo");

            // BinaryOperation
            res = (object[])ops.DoOperation(ExpressionType.Add, nd, 2);
            AreEqual(res[0], ExpressionType.Add);
            AreEqual(res[1], 2);

            // UnaryOperation
            AreEqual(ops.DoOperation(ExpressionType.Negate, nd), ExpressionType.Negate);

            // Convert
            AreEqual(ops.ConvertTo(nd, typeof(string)), "Hello");

            // InvokeMember, not supported by ObjectOps or properly by any languages
            //ScriptSource src = RB.CreateScriptSourceFromString("nd.foo 'bar'", SourceCodeKind.Expression);
            //ScriptScope scope = RB.CreateScope();
            //scope.SetVariable("nd", nd);
            //res = (object[])src.Execute(scope);

            //AreEqual(res[0], "InvokeMember");
            //AreEqual(res[1], "bar");
        }

        #region Helper Code

        private static PythonCompilerOptions TrueDivisionOptions(bool value) {
            return new PythonCompilerOptions(value ? ModuleOptions.TrueDivision : ModuleOptions.None);
        }

        private static ScriptEngine PY { get { return _env.GetEngine("py"); } }
        private static ScriptEngine JS { get { return _env.GetEngine("js"); } }
        private static ScriptEngine RB { get { return _env.GetEngine("rb"); } }

        private static PythonContext PythonContext { get { return DefaultContext.DefaultPythonContext; } }

        private static ScriptEngine GetEngine(string id) {
            return _env.GetEngine(id);
        }

        private static int domainId = 0;
#if !SILVERLIGHT
        private static AppDomain CreateDomain() {
            return AppDomain.CreateDomain("RemoteScripts" + domainId++);
        }
#endif

        [Flags]
        enum OutputFlags {
            None = 0,
            Raw = 1
        }

        private static void AssertOutput(Action f, string expectedOutput) {
            AssertOutput(f, expectedOutput, OutputFlags.None);
        }

        private static void AssertOutput(Action f, string expectedOutput, OutputFlags flags) {
            StringBuilder builder = new StringBuilder();

            using (StringWriter output = new StringWriter(builder)) {
                RedirectOutput(output, f);
            }

            string actualOutput = builder.ToString();

            if ((flags & OutputFlags.Raw) == 0) {
                actualOutput = actualOutput.Trim();
                expectedOutput = expectedOutput.Trim();
            }

            Assert(actualOutput == expectedOutput, "Unexpected output: '" +
                builder.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t") + "'.");
        }

        private static void RedirectOutput(TextWriter output, Action f) {
            _env.IO.SetOutput(Stream.Null, output);
            _env.IO.SetErrorOutput(Stream.Null, output);

            try {
                f();
            } finally {
                _env.IO.RedirectToConsole();
            }
        }

        private static T AssertExceptionThrown<T>(Action f) where T : Exception {
            try {
                f();
            } catch (T ex) {
                return ex;
            } catch (Exception ex) {
                // For some unknown reason, the catch (T) above doesn't work in
                // the debugger
                T t = ex as T;
                if (t != null) {
                    return t;
                }
            }


            Assert(false, "Expecting exception '" + typeof(T) + "'.");
            return null;
        }

        /// <summary>
        /// Asserts two values are equal
        /// </summary>
        private static void AreEqual(object x, object y) {
            if (x == null && y == null) return;

            Assert(x != null && x.Equals(y), String.Format("values aren't equal: {0} and {1}", x, y));
        }

        /// <summary>
        /// Asserts two enumerables contain exactly the same values in the same order
        /// </summary>
        private static void ContentsAreEqual(IEnumerable x, IEnumerable y) {
            Assert(x != null && y != null, "neither x nor y can be null");
            IEnumerator xe = x.GetEnumerator();
            IEnumerator ye = y.GetEnumerator();
            int idx = 0;

            while (xe.MoveNext()) {
                Assert(ye.MoveNext(), "IEnumerables aren't equal: x is longer than y");
                Assert(xe.Current.Equals(ye.Current), String.Format("IEnumerables aren't equal at element {0}: {1} and {2}", idx, xe.Current, ye.Current));
                idx++;
            }
            Assert(!ye.MoveNext(), "IEnumerables aren't equal: y is longer than x");
        }

        /// <summary>
        /// Asserts an condition it true
        /// </summary>
        private static void Assert(bool condition, string msg) {
            if (!condition) throw new Exception(String.Format("Assertion failed: {0}", msg));
        }

        private static void Assert(bool condition) {
            if (!condition) throw new Exception("Assertion failed");
        }

        #endregion
    }
}
