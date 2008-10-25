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
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Scripting.Utils;
using System.Text;
using System.Reflection;

namespace Microsoft.Linq.Expressions {
    /// <summary>
    /// Expression is the base type for all nodes in Expression Trees
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public abstract partial class Expression {
        private readonly Annotations _annotations;        
        // protected ctors are part of API surface area

        // LinqV1 ctor
        [Obsolete("use a different constructor that does not take ExpressionType.  Then override GetExpressionType and GetNodeKind to provide the values that would be specified to this constructor.")]
        protected Expression(ExpressionType nodeType, Type type) {
            // Can't enforce anything that V1 didn't
            _annotations = Annotate(new ExtensionInfo(nodeType, type));
        }

        protected Expression(Annotations annotations) {
            _annotations = annotations ?? Annotations.Empty;
        }

        protected Expression() {
            _annotations = Annotations.Empty;
        }
        
        //CONFORMING
        public ExpressionType NodeType {
            get { return GetNodeKind(); }
        }

        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get { return GetExpressionType(); }
        }
        
        public Annotations Annotations {
            get {
                return _annotations;
            }
        }

        /// <summary>
        /// Indicates that the node can be reduced to a simpler node. If this 
        /// returns true, Reduce() can be called to produce the reduced form.
        /// </summary>
        public virtual bool CanReduce {
            get { return false; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual ExpressionType GetNodeKind() {
            return _annotations.Get<ExtensionInfo>().NodeType;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual Type GetExpressionType() {
            return _annotations.Get<ExtensionInfo>().Type;
        }

        /// <summary>
        /// Reduces this node to a simpler expression. If CanReduce returns
        /// true, this should return a valid expression. This method is
        /// allowed to return another node which itself must be reduced.
        /// </summary>
        /// <returns>the reduced expression</returns>
        public virtual Expression Reduce() {
            ContractUtils.Requires(!CanReduce, "this", Strings.ReducibleMustOverrideReduce);
            return this;
        }

        /// <summary>
        /// Override this to provide logic to walk the node's children. A
        /// typical implementation will call visitor.Visit on each of its
        /// children, and if any of them change, should return a new copy of
        /// itself with the modified children.
        /// 
        /// The default implementation will reduce the node and then walk it
        /// This will throw an exception if the node is not reducible
        /// </summary>
        protected internal virtual Expression VisitChildren(ExpressionTreeVisitor visitor) {
            ContractUtils.Requires(CanReduce, "this", Strings.MustBeReducible);
            return visitor.Visit(ReduceExtensions());
        }

        // Visitor pattern: this is the method that dispatches back to the visitor
        // NOTE: this is unlike the Visit method, which provides a hook for
        // derived classes to extend the visitor framework to be able to walk
        // themselves
        internal virtual Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitExtension(this);
        }

        /// <summary>
        /// Reduces this node to a simpler expression. If CanReduce returns
        /// true, this should return a valid expression. This method is
        /// allowed to return another node which itself must be reduced.
        /// 
        /// Unlike Reduce, this method checks that the reduced node satisfies
        /// certain invaraints.
        /// </summary>
        /// <returns>the reduced expression</returns>
        public Expression ReduceAndCheck() {
            ContractUtils.Requires(CanReduce, "this", Strings.MustBeReducible);

            var newNode = Reduce();

            // 1. Reduction must return a new, non-null node
            // 2. Reduction must return a new node whose result type can be assigned to the type of the original node
            ContractUtils.Requires(newNode != null && newNode != this, "this", Strings.MustReduceToDifferent);
            ContractUtils.Requires(TypeUtils.AreReferenceAssignable(Type, newNode.Type), "this", Strings.ReducedNotCompatible);
            return newNode;
        }

        /// <summary>
        /// Reduces the expression to a known node type (i.e. not an Extension node)
        /// or simply returns the expression if it is already a known type.
        /// </summary>
        /// <returns>the reduced expression</returns>
        public Expression ReduceExtensions() {
            var node = this;
            while (node.NodeType == ExpressionType.Extension) {
                node = node.ReduceAndCheck();
            }
            return node;
        }

        //CONFORMING
        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            this.BuildString(builder);
            return builder.ToString();
        }

        //CONFORMING
        internal virtual void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");
            builder.Append("[");
            builder.Append(GetNodeKind().ToString());
            builder.Append("]");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private string Dump {
            get {
                using (System.IO.StringWriter writer = new System.IO.StringWriter(CultureInfo.CurrentCulture)) {
                    ExpressionWriter.Dump(this, GetType().Name, writer);
                    return writer.ToString();
                }
            }
        }

        internal static void RequiresCanRead(Expression expression, string paramName) {
            if (expression == null) {
                throw new ArgumentNullException(paramName);
            }

            // validate that we can read the node
            switch (expression.NodeType) {
                case ExpressionType.Index:
                    IndexExpression index = (IndexExpression)expression;
                    if (index.Indexer != null && !index.Indexer.CanRead) {
                        throw new ArgumentException(Strings.ExpressionMustBeReadable, paramName);
                    }
                    break;
                case ExpressionType.MemberAccess:
                    MemberExpression member = (MemberExpression)expression;
                    MemberInfo memberInfo = member.Member;
                    if (memberInfo.MemberType == MemberTypes.Property) {
                        PropertyInfo prop = (PropertyInfo)memberInfo;
                        if (!prop.CanRead) {
                            throw new ArgumentException(Strings.ExpressionMustBeReadable, paramName);
                        }
                    }
                    break;
            }
        }

        internal static void RequiresCanRead(IEnumerable<Expression> items, string paramName) {
            if (items != null) {
                // this is called a lot, avoid allocating an enumerator if we can...
                IList<Expression> listItems = items as IList<Expression>;
                if (listItems != null) {
                    for (int i = 0; i < listItems.Count; i++) {
                        RequiresCanRead(listItems[i], paramName);
                    }
                    return;
                }

                foreach (var i in items) {
                    RequiresCanRead(i, paramName);
                }
            }
        }
        internal static void RequiresCanWrite(Expression expression, string paramName) {
            if (expression == null) {
                throw new ArgumentNullException(paramName);
            }

            bool canWrite = false;
            switch (expression.NodeType) {
                case ExpressionType.Index:
                    IndexExpression index = (IndexExpression)expression;
                    if (index.Indexer != null) {
                        canWrite = index.Indexer.CanWrite;
                    } else {
                        canWrite = true;
                    }
                    break;
                case ExpressionType.MemberAccess:
                    MemberExpression member = (MemberExpression)expression;
                    switch (member.Member.MemberType) {
                        case MemberTypes.Property:
                            PropertyInfo prop = (PropertyInfo)member.Member;
                            canWrite = prop.CanWrite;
                            break;
                        case MemberTypes.Field:
                            FieldInfo field = (FieldInfo)member.Member;
                            canWrite = !(field.IsInitOnly || field.IsLiteral);
                            break;
                    }
                    break;
                case ExpressionType.Parameter:
                case ExpressionType.ArrayIndex:
                    canWrite = true;
                    break;
            }

            if (!canWrite) {
                throw new ArgumentException(Strings.ExpressionMustBeWriteable, paramName);
            }
        }

        struct ExtensionInfo {
            public ExtensionInfo(ExpressionType nodeType, Type type) {
                NodeType = nodeType;
                Type = type;
            }

            internal readonly ExpressionType NodeType;
            internal readonly Type Type;
        }
    }
}
