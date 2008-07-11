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

using System.Scripting.Utils;
using System.Text;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class ParameterExpression : Expression {
        private readonly string _name;
        private readonly bool _isByRef;

        internal ParameterExpression(Annotations annotations, Type type, string name, bool isByRef)
            : base(annotations, ExpressionType.Parameter, type) {
            _name = name;
            _isByRef = isByRef;
        }

        public string Name {
            get { return _name; }
        }

        public bool IsByRef {
            get {
                return _isByRef;
            }
        }

        internal override void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");
            builder.Append(_name ?? "<param>");
        }
    }

    public partial class Expression {
        public static ParameterExpression Parameter(Type type, string name) {
            return Parameter(type, name, Annotations.Empty);
        }

        //CONFORMING
        public static ParameterExpression Parameter(Type type, string name, Annotations annotations) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(!type.IsByRef, "type");

            if (type == typeof(void)) {
                throw Error.ArgumentCannotBeOfTypeVoid();
            }

            return new ParameterExpression(annotations, type, name, false);
        }

        public static ParameterExpression ByRefParameter(Type type, string name) {
            return ByRefParameter(type, name, Annotations.Empty);
        }

        public static ParameterExpression ByRefParameter(Type type, string name, Annotations annotations) {
            ContractUtils.RequiresNotNull(type, "type");

            if (type == typeof(void)) {
                throw Error.ArgumentCannotBeOfTypeVoid();
            }

            type = TypeUtils.NoRef(type);

            return new ParameterExpression(annotations, type, name, true);
        }
    }
}
