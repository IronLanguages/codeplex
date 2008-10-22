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

namespace Microsoft.Linq.Expressions {

    // TODO: probably should not have Annotations, since it's part of TryExpression
    // They can either go there on on the body
    public sealed class CatchBlock {
        private readonly Annotations _annotations;
        private readonly Type _test;
        private readonly ParameterExpression _var;
        private readonly Expression _body;
        private readonly Expression _filter;

        internal CatchBlock(Annotations annotations, Type test, ParameterExpression target, Expression body, Expression filter) {
            _test = test;
            _var = target;
            _body = body;
            _annotations = annotations;
            _filter = filter;
        }

        public Annotations Annotations {
            get { return _annotations; }
        }

        public ParameterExpression Variable {
            get { return _var; }
        }

        public Type Test {
            get { return _test; }
        }

        public Expression Body {
            get { return _body; }
        }

        public Expression Filter {
            get {
                return _filter;
            }
        }
    }

    public partial class Expression {
        public static CatchBlock Catch(Type type, Expression body) {
            return Catch(type, null, body, null, Annotations.Empty);
        }

        public static CatchBlock Catch(Type type, ParameterExpression target, Expression body) {
            return Catch(type, target, body, null, Annotations.Empty);
        }

        public static CatchBlock Catch(Type type, ParameterExpression target, Expression body, Expression filter) {
            return Catch(type, target, body, filter, Annotations.Empty);
        }

        public static CatchBlock Catch(Type type, ParameterExpression target, Expression body, Expression filter, Annotations annotations) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(target == null || TypeUtils.AreReferenceAssignable(target.Type, type), "target");
            Expression.RequireVariableNotByRef(target, "target");
            RequiresCanRead(body, "body");
            if (filter != null) {
                RequiresCanRead(filter, "filter");
                ContractUtils.Requires(filter.Type == typeof(bool), Strings.ArgumentMustBeBoolean);
            }

            return new CatchBlock(annotations, type, target, body, filter);
        }
    }
}
