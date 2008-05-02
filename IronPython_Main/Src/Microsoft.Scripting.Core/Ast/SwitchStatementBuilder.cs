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
using System.Collections.Generic;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class SwitchStatementBuilder {
        private readonly SourceSpan _span;
        private readonly SourceLocation _header;
        private Expression _test;
        private readonly List<SwitchCase> _cases = new List<SwitchCase>();
        private bool _default;
        private LabelTarget _label;

        internal SwitchStatementBuilder(SourceSpan span, SourceLocation header, LabelTarget label, Expression test) {
            _span = span;
            _header = header;
            _label = label;
            _test = test;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")] // TODO: fix
        public SwitchStatementBuilder Test(Expression test) {
            ContractUtils.RequiresNotNull(test, "test");
            _test = test;
            return this;
        }

        public SwitchStatementBuilder Default(Expression body) {
            return Default(SourceLocation.None, body);
        }

        public SwitchStatementBuilder Default(SourceLocation header, Expression body) {
            ContractUtils.Requires(_default == false, "body", "Already has default clause");
            _cases.Add(Expression.DefaultCase(header, body));
            _default = true;
            return this;
        }

        public SwitchStatementBuilder Case(int value, Expression body) {
            return Case(SourceLocation.None, value, body);
        }

        public SwitchStatementBuilder Case(SourceLocation header, int value, Expression body) {
            _cases.Add(Expression.SwitchCase(header, value, body));
            return this;
        }

        public Expression ToStatement() {
            ContractUtils.Requires(_test != null);
            return Expression.Switch(_span, _header, _label, _test, _cases.ToArray());
        }

        public static implicit operator Expression(SwitchStatementBuilder builder) {
            return builder.ToStatement();
        }
    }

    public partial class Expression {
        public static SwitchStatementBuilder Switch() {
            return Switch(SourceSpan.None, SourceLocation.None, null, null);
        }

        public static SwitchStatementBuilder Switch(LabelTarget label) {
            return Switch(SourceSpan.None, SourceLocation.None, label, null);
        }

        public static SwitchStatementBuilder Switch(SourceSpan span, SourceLocation header) {
            return new SwitchStatementBuilder(span, header, null, null);
        }

        public static SwitchStatementBuilder Switch(SourceSpan span, SourceLocation header, LabelTarget label) {
            return new SwitchStatementBuilder(span, header, label, null);
        }

        public static SwitchStatementBuilder Switch(Expression test) {
            return Switch(SourceSpan.None, SourceLocation.None, null, test);
        }

        public static SwitchStatementBuilder Switch(LabelTarget label, Expression test) {
            return Switch(SourceSpan.None, SourceLocation.None, label, test);
        }

        public static SwitchStatementBuilder Switch(SourceSpan span, SourceLocation header, Expression test) {
            return Switch(span, header, null, test);
        }

        public static SwitchStatementBuilder Switch(SourceSpan span, SourceLocation header, LabelTarget label, Expression test) {
            ContractUtils.RequiresNotNull(test, "test");
            return new SwitchStatementBuilder(span, header, label, test);
        }
    }
}
