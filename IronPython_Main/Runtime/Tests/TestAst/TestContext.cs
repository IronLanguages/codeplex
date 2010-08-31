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

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

using TestAst.Runtime;

namespace TestAst {
    class TestContext : LanguageContext {
        internal static TestContext _TestContext;
        private readonly ActionBinder _binder;

        internal TestTypeFlag _RunTestTypes;
        public enum TestTypeFlag {
            Positive = 1,
            Negative = 2
        }

        public TestTypeFlag RunTestTypes {
            get{
                return _RunTestTypes;
            }
            set{
                _RunTestTypes = value;
            }
        }

        public ActionBinder Binder {
            get {
                return _binder;
            }
        }

        private readonly TestEngineOptions _options;

        public TestContext(ScriptDomainManager manager, IDictionary<string, object> options) 
            : base(manager) {
            _options = new TestEngineOptions(options);
            manager.LoadAssembly(typeof(TestContext).Assembly);

            _binder = new DefaultActionBinder();
            _TestContext = this;
        }

        public override LanguageOptions Options {
            get { return _options; }
        }

        internal TestEngineOptions TestOptions {
            get { return _options; }
        }

#if !SILVERLIGHT
        public override SourceUnit GenerateSourceCode(System.CodeDom.CodeObject codeDom, string id, SourceCodeKind kind) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }
#endif

        public override ScriptCode CompileSourceCode(SourceUnit/*!*/ sourceUnit, CompilerOptions/*!*/ options, ErrorSink/*!*/ errorSink) {
            //We aren't actually running a file here.  It's just that the default
            //command line parser believes we are.  Actually we're specifying test
            //scenario names on the command line.  Those scenarios are defined in
            //TestScenarios.cs.
            
            TestScope scope = new TestScope("TestAst",null); //@TODO - Move this into the TestScenarios constructor along with the push/pop maintenance methods now in TestScope
            TestScenarios tests = new TestScenarios(this);

            LambdaExpression ast = scope.FinishScope(tests.Generate(this.RunTestTypes));
            return new LegacyScriptCode(ast, sourceUnit);
        }

        public override CompilerOptions GetCompilerOptions() {            
            return new CompilerOptions();
        }
    }
}
