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
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting;

namespace Microsoft.Scripting.Hosting {
    public abstract class ScriptCompiler {

        private readonly ScriptEngine _engine;
        public ScriptEngine Engine { get { return _engine; } }

        protected ScriptCompiler(ScriptEngine engine) {
            if (engine == null) throw new ArgumentNullException("engine");
            _engine = engine;
        }

        #region Public API Surface

        public abstract CodeBlock ParseInteractiveCode(CompilerContext cc, bool allowIncomplete, out InteractiveCodeProperties properties);

        // Parses a list of statements.
        // TODO: rename 
        public abstract CodeBlock ParseFile(CompilerContext cc);

        // Parses a single statement.
        public abstract CodeBlock ParseStatementCode(CompilerContext cc);

        // Parses a code containing an expression and creates a block returning the result of the expression.
        public abstract CodeBlock ParseExpressionCode(CompilerContext cc);

#if !SILVERLIGHT
        // Convert a CodeDom to source code, and output the generated code and the line number mappings (if any)
        public abstract SourceUnit ParseCodeDom(System.CodeDom.CodeObject codeDom);
#endif

        #endregion
    }
}
