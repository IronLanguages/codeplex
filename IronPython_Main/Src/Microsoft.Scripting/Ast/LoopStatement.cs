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
using System.Reflection.Emit;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class LoopStatement : Statement {
        private readonly SourceLocation _header;
        private readonly Expression _test;
        private readonly Expression _increment;
        private readonly Statement _body;
        private readonly Statement _else;

        public LoopStatement(Expression test, Expression increment, Statement body, Statement else_) {
            _test = test;
            _increment = increment;
            _body = body;
            _else = else_;
        }

        public LoopStatement(Expression test, Expression increment, Statement body, Statement else_, SourceSpan span, SourceLocation header)
            : base(span) {
            _test = test;
            _increment = increment;
            _body = body;
            _else = else_;
            _header = header;
        }

        public SourceLocation Header {
            get { return _header; }
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression Increment {
            get { return _increment; }
        }

        public Statement Body {
            get { return _body; }
        }

        public Statement ElseStatement {
            get { return _else; }
        }

        public override object Execute(CodeContext context) {
            object ret = NextStatement;
            while (context.LanguageContext.IsTrue(_test.Evaluate(context))) {
                ret = _body.Execute(context);
                if (ret != NextStatement) break;

                if (_increment != null) {
                    _increment.Evaluate(context);
                }
            }            
            return ret;
        }

        public override void Emit(CodeGen cg) {
            Nullable<Label> firstTime = null;
            Label eol = cg.DefineLabel();
            Label breakTarget = cg.DefineLabel();
            Label continueTarget = cg.DefineLabel();

            if (_increment != null) {
                firstTime = cg.DefineLabel();
                cg.Emit(OpCodes.Br, firstTime.Value);
            }

            cg.MarkLabel(continueTarget);

            cg.EmitPosition(Start, _header);

            if (_increment != null) {
                _increment.EmitAs(cg, typeof(void));
                cg.MarkLabel(firstTime.Value);
            }
            
            cg.EmitTestTrue(_test);
            cg.Emit(OpCodes.Brfalse, eol);

            cg.PushTargets(breakTarget, continueTarget, this);

            _body.Emit(cg);
            
            cg.EmitPosition(Start, _header);
            cg.Emit(OpCodes.Br, continueTarget);

            cg.PopTargets();

            cg.MarkLabel(eol);
            if (_else != null) {
                _else.Emit(cg);
            }
            cg.MarkLabel(breakTarget);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _test.Walk(walker);
                if (_increment != null) _increment.Walk(walker);
                _body.Walk(walker);
                if (_else != null) _else.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
