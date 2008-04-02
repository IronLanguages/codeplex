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

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Summary description for Expr.
    /// </summary>
    public abstract class Expression {
        private readonly AstNodeType _nodeType;
        private readonly Type /*!*/ _type;
        private readonly Annotations _annotations;

        protected Expression(AstNodeType nodeType, Type type) :
            this(Annotations.Empty, nodeType, type) {
            _nodeType = nodeType;
            _type = type;
        }

        protected Expression(Annotations annotations, AstNodeType nodeType, Type type) {
            _annotations = annotations;
            _nodeType = nodeType;
            _type = type;
        }

        public AstNodeType NodeType {
            get { return _nodeType; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get { return _type; }
        }

        //_annotations is never null
        public Annotations Annotations {
            get { return _annotations; }
        }

        internal SourceLocation Start {
            get { return _annotations.Get<SourceSpan>().Start; }
        }

        internal SourceLocation End {
            get { return _annotations.Get<SourceSpan>().End; }
        }

#if DEBUG
        public string Dump {
            get {
                using (System.IO.StringWriter writer = new System.IO.StringWriter()) {
                    AstWriter.Dump(this, GetType().Name, writer);
                    return writer.ToString();
                }
            }
        }
#endif
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")] // TODO: fix
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class Ast {
    }
}
