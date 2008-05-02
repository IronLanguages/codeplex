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
using System.Diagnostics;
using System.Reflection;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;

using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Expression;

    // New in Pep342 for Python 2.5. Yield is an expression with a return value.
    //    x = yield z
    // The return value (x) is provided by calling Generator.Send()
    public class YieldExpression : Expression { 
        private readonly Expression _expression;

        public YieldExpression(Expression expression) {
            _expression = expression;
        }

        public Expression Expression {
            get { return _expression; }
        }

        // Generate AST statement to call $gen.CheckThrowable() on the Python Generator.
        // This needs to be injected at any yield suspension points, mainly:
        // - at the start of the generator body
        // - after each yield statement.
        static internal MSAst.Expression CreateCheckThrowExpression(AstGenerator ag, SourceSpan span) {
            if (!ag.IsGenerator) {
                // This can fail if yield is used outside of a function body. 
                // Normally, we'd like the parser to catch this and just assert there. But yield could be in practically any expression,
                // and the parser can't catch all cases. 

                // Consider using ag.AddError(). However, consumers expect Expression transforms to be non-null, so if we don't throw,
                // we'd still need to return something. 
                throw PythonOps.SyntaxError(IronPython.Resources.MisplacedYield, ag.Context.SourceUnit, span, IronPython.Hosting.ErrorCodes.SyntaxError);
            }

            Type tGenerator = typeof(IronPython.Runtime.PythonGenerator);

            MSAst.Expression instance = Ast.GeneratorInstanceExpression(tGenerator);
            Debug.Assert(instance.Type == tGenerator);

            MSAst.Expression s2 = Ast.Call(
                typeof(PythonOps).GetMethod("GeneratorCheckThrowableAndReturnSendValue"),
                instance
            );
            return s2;
        }

        internal override Microsoft.Scripting.Ast.Expression Transform(AstGenerator ag, Type type) {
            // (yield z) becomes:
            // .comma (1) {
            //    .void ( .yield_statement (_expression) ),
            //    $gen.CheckThrowable() // <-- has return result from send            
            //  }

            return Ast.Comma(
                Ast.Yield(Span, ag.Transform(_expression)),
                CreateCheckThrowExpression(ag, this.Span) // emits ($gen.CheckThrowable())
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_expression != null) {
                    _expression.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        public override string NodeName {
            get {
                return "yield expression";
            }
        }
    }
}
