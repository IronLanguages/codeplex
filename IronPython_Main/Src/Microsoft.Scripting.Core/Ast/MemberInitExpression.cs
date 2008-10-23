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
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;
using System.Text;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    public sealed class MemberInitExpression : Expression {
        private readonly NewExpression _newExpression;
        private readonly ReadOnlyCollection<MemberBinding> _bindings;

        internal MemberInitExpression(NewExpression newExpression, ReadOnlyCollection<MemberBinding> bindings, Annotations annotations)
            : base(annotations) {
            _newExpression = newExpression;
            _bindings = bindings;
        }

        protected override Type GetExpressionType() {
            return _newExpression.Type;
        }

        internal override Expression.NodeFlags GetFlags() {
            return NodeFlags.CanReduce | NodeFlags.CanRead;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.MemberInit;
        }

        public NewExpression NewExpression {
            get { return _newExpression; }
        }
        public ReadOnlyCollection<MemberBinding> Bindings {
            get { return _bindings; }
        }
        internal override void BuildString(StringBuilder builder) {
            if (_newExpression.Arguments.Count == 0 &&
                _newExpression.Type.Name.Contains("<")) {
                // anonymous type constructor
                builder.Append("new");
            } else {
                _newExpression.BuildString(builder);
            }
            builder.Append(" {");
            for (int i = 0, n = _bindings.Count; i < n; i++) {
                MemberBinding b = _bindings[i];
                if (i > 0) {
                    builder.Append(", ");
                }
                b.BuildString(builder);
            }
            builder.Append("}");
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitMemberInit(this);
        }

        public override Expression Reduce() {
            return ReduceMemberInit(_newExpression, _bindings, true, Annotations);
        }

        internal static Expression ReduceMemberInit(Expression objExpression, ReadOnlyCollection<MemberBinding> bindings, bool keepOnStack, Annotations annotations) {
            var objVar = Expression.Variable(objExpression.Type, null);
            int count = bindings.Count;
            var block = new Expression[count + 1 + (keepOnStack ? 1 : 0)];
            block[0] = Expression.Assign(objVar, objExpression);
            for (int i = 0; i < count; i++) {
                block[i + 1] = ReduceMemberBinding(objVar, bindings[i]);
            }
            if (keepOnStack) {
                block[count + 1] = objVar;
                return Expression.Comma(annotations, new ReadOnlyCollection<Expression>(block));
            } else {
                return Expression.Block(annotations, new ReadOnlyCollection<Expression>(block));
            }
        }

        internal static Expression ReduceListInit(Expression listExpression, ReadOnlyCollection<ElementInit> initializers, bool keepOnStack, Annotations annotations) {
            var listVar = Expression.Variable(listExpression.Type, null);
            int count = initializers.Count;
            var block = new Expression[count + 1 + (keepOnStack ? 1 : 0)];
            block[0] = Expression.Assign(listVar, listExpression);
            for (int i = 0; i < count; i++) {
                ElementInit element = initializers[i];
                block[i + 1] = Expression.Call(listVar, element.AddMethod, element.Arguments);
            }
            if (keepOnStack) {
                block[count + 1] = listVar;
                return Expression.Comma(annotations, new ReadOnlyCollection<Expression>(block));
            } else {
                return Expression.Block(annotations, new ReadOnlyCollection<Expression>(block));
            }
        }

        internal static Expression ReduceMemberBinding(ParameterExpression objVar, MemberBinding binding) {
            MemberExpression member = Expression.MakeMemberAccess(objVar, binding.Member);
            switch (binding.BindingType) {
                case MemberBindingType.Assignment:
                    return Expression.Assign(member, ((MemberAssignment)binding).Expression);
                case MemberBindingType.ListBinding:
                    return ReduceListInit(member, ((MemberListBinding)binding).Initializers, false, null);
                case MemberBindingType.MemberBinding:
                    return ReduceMemberInit(member, ((MemberMemberBinding)binding).Bindings, false, null);
                default: throw Assert.Unreachable;
            }
        }
    }

    public partial class Expression {
        //CONFORMING
        public static MemberInitExpression MemberInit(NewExpression newExpression, params MemberBinding[] bindings) {
            return MemberInit(newExpression, null, (IEnumerable<MemberBinding>)bindings);
        }
        //CONFORMING
        public static MemberInitExpression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings) {
            return MemberInit(newExpression, null, bindings);
        }
        public static MemberInitExpression MemberInit(NewExpression newExpression, Annotations annotations, IEnumerable<MemberBinding> bindings) {
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ReadOnlyCollection<MemberBinding> roBindings = bindings.ToReadOnly();
            ValidateMemberInitArgs(newExpression.Type, roBindings);
            return new MemberInitExpression(newExpression, roBindings, annotations);
        }
    }
}