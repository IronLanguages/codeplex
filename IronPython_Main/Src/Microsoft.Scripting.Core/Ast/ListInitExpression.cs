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
using System.Reflection;
using System.Text;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    // TODO: should support annotations
    public sealed class ListInitExpression : Expression {
        NewExpression _newExpression;
        ReadOnlyCollection<ElementInit> _initializers;
        internal ListInitExpression(NewExpression newExpression, ReadOnlyCollection<ElementInit> initializers)
            : base(ExpressionType.ListInit, newExpression.Type, null) {
            _newExpression = newExpression;
            _initializers = initializers;
        }
        public NewExpression NewExpression {
            get { return _newExpression; }
        }
        public ReadOnlyCollection<ElementInit> Initializers {
            get { return _initializers; }
        }
        internal override void BuildString(StringBuilder builder) {
            _newExpression.BuildString(builder);
            builder.Append(" {");
            for (int i = 0, n = _initializers.Count; i < n; i++) {
                if (i > 0) {
                    builder.Append(", ");
                }
                _initializers[i].BuildString(builder);
            }
            builder.Append("}");
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitListInit(this);
        }
    }


    public partial class Expression {
        //CONFORMING
        public static ListInitExpression ListInit(NewExpression newExpression, params Expression[] initializers) {
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListInit(newExpression, initializers as IEnumerable<Expression>);
        }
        //CONFORMING
        public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<Expression> initializers) {
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            ReadOnlyCollection<Expression> initializerlist = initializers.ToReadOnly();
            if (initializerlist.Count == 0) {
                throw Error.ListInitializerWithZeroMembers();
            }
            MethodInfo addMethod = FindMethod(newExpression.Type, "Add", null, new Expression[] { initializerlist[0] }, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return ListInit(newExpression, addMethod, initializers);
        }
        //CONFORMING
        public static ListInitExpression ListInit(NewExpression newExpression, MethodInfo addMethod, params Expression[] initializers) {
            if (addMethod == null) {
                return ListInit(newExpression, initializers as IEnumerable<Expression>);
            }
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListInit(newExpression, addMethod, initializers as IEnumerable<Expression>);
        }
        //CONFORMING
        public static ListInitExpression ListInit(NewExpression newExpression, MethodInfo addMethod, IEnumerable<Expression> initializers) {
            if (addMethod == null) {
                return ListInit(newExpression, initializers);
            }
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            ReadOnlyCollection<Expression> initializerlist = initializers.ToReadOnly();
            if (initializerlist.Count == 0) {
                throw Error.ListInitializerWithZeroMembers();
            }
            ElementInit[] initList = new ElementInit[initializerlist.Count];
            for (int i = 0; i < initializerlist.Count; i++) {
                initList[i] = ElementInit(addMethod, initializerlist[i]);
            }
            return ListInit(newExpression, new ReadOnlyCollection<ElementInit>(initList));
        }
        //CONFORMING
        public static ListInitExpression ListInit(NewExpression newExpression, params ElementInit[] initializers) {
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListInit(newExpression, initializers.ToReadOnly());
        }
        //CONFORMING
        public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<ElementInit> initializers) {
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            ReadOnlyCollection<ElementInit> initializerlist = initializers.ToReadOnly();
            if (initializerlist.Count == 0) {
                throw Error.ListInitializerWithZeroMembers();
            }
            ValidateListInitArgs(newExpression.Type, initializerlist);
            return new ListInitExpression(newExpression, initializerlist);
        }
    }
}