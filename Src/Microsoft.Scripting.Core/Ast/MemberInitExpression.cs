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
using System.Text;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    // TODO: should support annotations
    public sealed class MemberInitExpression : Expression {
        NewExpression _newExpression;
        ReadOnlyCollection<MemberBinding> _bindings;
        internal MemberInitExpression(NewExpression newExpression, ReadOnlyCollection<MemberBinding> bindings)
            : base(ExpressionType.MemberInit, newExpression.Type, null) {
            _newExpression = newExpression;
            _bindings = bindings;
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
    }

    public partial class Expression {
        //CONFORMING
        public static MemberInitExpression MemberInit(NewExpression newExpression, params MemberBinding[] bindings) {
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(bindings, "bindings");
            return MemberInit(newExpression, bindings.ToReadOnly());
        }
        //CONFORMING
        public static MemberInitExpression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings) {
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(bindings, "bindings");
            ReadOnlyCollection<MemberBinding> roBindings = bindings.ToReadOnly();
            ValidateMemberInitArgs(newExpression.Type, roBindings);
            return new MemberInitExpression(newExpression, roBindings);
        }
    }
}