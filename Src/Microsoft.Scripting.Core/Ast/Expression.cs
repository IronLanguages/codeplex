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

namespace Microsoft.Linq.Expressions {
    /// <summary>
    /// Expression is the base type for all nodes in Expression Trees
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public abstract partial class Expression {
        // TODO: expose this to derived classes, so ctor doesn't take three booleans?
        [Flags]
        private enum NodeFlags : byte {
            None = 0,
            CanReduce = 1,
            CanRead = 2,
            CanWrite = 4,
        }

        // TODO: these two enums could be stored in one int32
        private readonly ExpressionType _nodeType;
        private readonly NodeFlags _flags;

        private readonly Type _type;
        private readonly Annotations _annotations;

        // protected ctors are part of API surface area

        // LinqV1 ctor
        [Obsolete("use a different constructor that does not take ExpressionType")]
        protected Expression(ExpressionType nodeType, Type type) {
            // Can't enforce anything that V1 didn't
            _nodeType = nodeType;
            _type = type;
            _flags = NodeFlags.CanRead;
        }

        // LinqV2: ctor for extension nodes
        protected Expression(Type type, bool canReduce, Annotations annotations)
            : this(ExpressionType.Extension, type, canReduce, annotations, true, false) {
        }

        // LinqV2: ctor for extension nodes with read/write flags
        protected Expression(Type type, bool canReduce, Annotations annotations, bool canRead, bool canWrite)
            : this(ExpressionType.Extension, type, canReduce, annotations, canRead, canWrite) {
        }

        // internal ctor -- not exposed API
        internal Expression(ExpressionType nodeType, Type type, Annotations annotations)
            : this(nodeType, type, false, annotations, true, false) {
        }

        // internal ctor -- not exposed API
        // but it is called from protected ctors, so we validate parameters
        // that could be passed from those
        internal Expression(ExpressionType nodeType, Type type, bool canReduce, Annotations annotations, bool canRead, bool canWrite) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(canRead || canWrite, "canRead", Strings.MustBeReadableOrWriteable);

            _annotations = annotations ?? Annotations.Empty;
            _nodeType = nodeType;
            _type = type;
            _flags = (canReduce ? NodeFlags.CanReduce : 0) | (canRead ? NodeFlags.CanRead : 0) | (canWrite ? NodeFlags.CanWrite : 0);
        }

        //CONFORMING
        public ExpressionType NodeType {
            get { return _nodeType; }
        }

        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get { return _type; }
        }

        public Annotations Annotations {
            get { return _annotations; }
        }

        /// <summary>
        /// Indicates that the node can be reduced to a simpler node. If this 
        /// returns true, Reduce() can be called to produce the reduced form.
        /// </summary>
        public bool CanReduce {
            get { return (_flags & NodeFlags.CanReduce) != 0; }
        }

        /// <summary>
        /// Indicates that the node can be read
        /// </summary>
        public bool CanRead {
            get { return (_flags & NodeFlags.CanRead) != 0; }
        }

        /// <summary>
        /// Indicates that the node can be written
        /// </summary>
        public bool CanWrite {
            get { return (_flags & NodeFlags.CanWrite) != 0; }
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
            // 3. Reduction must return a node that can be read/written to if the original node could
            ContractUtils.Requires(newNode != null && newNode != this, "this", Strings.MustReduceToDifferent);
            ContractUtils.Requires(TypeUtils.AreReferenceAssignable(Type, newNode.Type), "this", Strings.ReducedNotCompatible);
            ContractUtils.Requires(!CanRead || newNode.CanRead, "this", Strings.MustReduceToReadable);
            ContractUtils.Requires(!CanWrite || newNode.CanWrite, "this", Strings.MustReduceToWriteable);
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
            builder.Append(_nodeType.ToString());
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
            if (!expression.CanRead) {
                throw new ArgumentException(Strings.ExpressionMustBeReadable, paramName);
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
            if (!expression.CanWrite) {
                throw new ArgumentException(Strings.ExpressionMustBeWriteable, paramName);
            }
        }
    }
}
