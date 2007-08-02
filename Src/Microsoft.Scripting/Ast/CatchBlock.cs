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
using System.Diagnostics;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class CatchBlock : Node {
        private readonly SourceLocation _header;
        private readonly Type _test;
        private readonly Variable _var;
        private readonly Statement _body;

        private VariableReference _ref;

        internal CatchBlock(SourceSpan span, SourceLocation header, Type test, Variable target, Statement body)
            : base(span) {
            if (body == null) throw new ArgumentNullException("body");

            _test = test;
            _var = target;
            _body = body;
            _header = header;
        }

        public SourceLocation Header {
            get { return _header; }
        }

        public Variable Variable {
            get { return _var; }
        }

        public Type Test {
            get { return _test; }
        }

        public Statement Body {
            get { return _body; }
        }

        internal VariableReference Ref {
            get { return _ref; }
            set {
                Debug.Assert(value.Variable == _var);
                Debug.Assert(_ref == null);
                _ref = value;
            }
        }

        internal Slot Slot {
            get { return _ref.Slot; }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _body.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        public static CatchBlock Catch(Type test, Statement body) {
            return Catch(SourceSpan.None, SourceLocation.None, test, null, body);
        }
        public static CatchBlock Catch(Type test, Variable target, Statement body) {
            return Catch(SourceSpan.None, SourceLocation.None, test, target, body);
        }
        public static CatchBlock Catch(SourceSpan span, SourceLocation header, Type test, Variable target, Statement body) {
            return new CatchBlock(span, header, test, target, body);
        }
    }
}