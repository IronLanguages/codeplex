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

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class ThrowStatement : Expression, ISpan {
        private readonly Expression _val;
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        internal ThrowStatement(SourceLocation start, SourceLocation end, Expression value)
            : base(AstNodeType.ThrowStatement, typeof(void)) {
            _start = start;
            _end = end;
            _val = value;
        }

        public Expression Value {
            get {
                return _val;
            }
        }

        public Expression Exception {
            get {
                return _val;
            }
        }

        public SourceLocation Start {
            get { return _start; }
        }

        public SourceLocation End {
            get { return _end; }
        }
    }

    public static partial class Ast {
        public static ThrowStatement Rethrow() {
            return Throw(SourceSpan.None, null);
        }

        public static ThrowStatement Rethrow(SourceSpan span) {
            return Throw(span, null);
        }

        public static ThrowStatement Throw(Expression value) {
            return Throw(SourceSpan.None, value);
        }

        public static ThrowStatement Throw(SourceSpan span, Expression value) {
            if (value != null) {
                Contract.Requires(TypeUtils.CanAssign(typeof(Exception), value.Type));
            }
            return new ThrowStatement(span.Start, span.End, value);
        }
    }
}
