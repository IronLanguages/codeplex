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
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using System.Diagnostics;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting;

namespace Microsoft.Scripting.Internal.Ast {
    public class ScopeStatement : Statement {
        private Expression _scope;
        private Statement _body;

        public Expression Scope {
            get {
                return _scope;
            }
        }

        public Statement Body {
            get {
                return _body;
            }
        }

        public ScopeStatement(Expression scope, Statement body) {
            _scope = scope;
            _body = body;
        }

        public override object Execute(CodeContext context) {
            IAttributesCollection scopeObject = _scope.Evaluate(context) as IAttributesCollection;
            CodeContext scopeContext = RuntimeHelpers.CreateNestedCodeContext(context, scopeObject, true);
            _body.Execute(scopeContext);
            return NextStatement;
        }

        public override void Emit(CodeGen cg) {
            Slot tempContext = cg.ContextSlot;
            Slot newContext = cg.GetLocalTmp(typeof(CodeContext));
            
            cg.EmitCodeContext();   //CodeContext
            _scope.EmitAs(cg, typeof(IAttributesCollection));        //Locals dictionary
            cg.EmitInt(1);          //Visible = true
            cg.EmitCall(typeof(RuntimeHelpers), "CreateNestedCodeContext");

            newContext.EmitSet(cg);

            cg.ContextSlot = newContext;
            _body.Emit(cg);
            cg.ContextSlot = tempContext;
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _scope.Walk(walker);
                _body.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
