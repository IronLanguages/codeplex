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
using System; using Microsoft;


using Microsoft.Scripting.Utils;
using System.Reflection;

namespace Microsoft.Scripting.Ast {
    public sealed class ScopeStatement : Expression {
        private readonly Expression/*!*/ _body;
        private readonly MethodInfo/*!*/ _factory;

        internal ScopeStatement(Annotations annotations, MethodInfo/*!*/ factory, Expression/*!*/ body)
            : base(annotations, AstNodeType.ScopeStatement, typeof(void)) {
            
            _factory = factory;
            _body = body;
        }

        public MethodInfo/*!*/ Factory {
            get {
                return _factory;
            }
        }

        public Expression/*!*/ Body {
            get {
                return _body;
            }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        public static ScopeStatement Scope(MethodInfo/*!*/ factory, Expression/*!*/ body) {
            return Scope(SourceSpan.None, factory, body);
        }
        
        public static ScopeStatement Scope(SourceSpan span, MethodInfo/*!*/ factory, Expression/*!*/ body) {
            return Scope(Annotate(span), factory, body);
        }

        public static ScopeStatement Scope(Annotations annotations, MethodInfo/*!*/ factory, Expression/*!*/ body) {
            CodeContract.RequiresNotNull(body, "body");

            // TODO: this requirement won't be necessary as soon as we have variables on scope:
            CodeContract.RequiresNotNull(factory, "factory");

            ValidateScopeFactory(factory, "factory");
            
            return new ScopeStatement(annotations, factory, body);
        }
    }
}
