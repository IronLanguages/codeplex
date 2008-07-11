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

using System.Diagnostics;
using System.Scripting.Utils;
using System.Text;

namespace System.Linq.Expressions {

    /// <summary>
    /// VariableExpression represents actual memory/dictionary location in the generated code.
    /// </summary>    
    public sealed class VariableExpression : Expression {
        private readonly string _name;

        internal VariableExpression(Annotations annotations, Type type, string name)
            : base(annotations, ExpressionType.Variable, type) {
            Debug.Assert(type != typeof(void));

            _name = name;
        }

        public string Name {
            get { return _name; }
        }

        internal override void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");
            builder.Append(_name ?? "<var>");
        }
    }

    public partial class Expression {
        public static VariableExpression Variable(Type type, string name) {
            return Variable(type, name, Annotations.Empty);
        }
        public static VariableExpression Variable(Type type, string name, Annotations annotations) {
            return new VariableExpression(annotations, GetNonVoidType(type), name);
        }

        // Converts typeof(void) to typeof(object), leaving other types unchanged.
        //
        // typeof(void) is allowed as the variable type to support this: 
        //
        // temp = CreateVariable(..., expression.Type, ...)
        // Expression.Assign(temp, expression)
        //
        // where expression.Type is void.
        private static Type GetNonVoidType(Type t) {
            return (t != typeof(void)) ? t : typeof(object);
        }

        // TODO: remove obsolete factories:
        [Obsolete("use Expression.Variable instead")]
        public static VariableExpression Local(Type type, string name) {
            return Variable(type, name, Annotations.Empty);
        }
        [Obsolete("use Expression.Variable instead")]
        public static VariableExpression Local(Type type, string name, Annotations annotations) {
            return Variable(type, name, annotations);
        }
        [Obsolete("use Expression.Variable instead")]
        public static VariableExpression Temporary(Type type, string name) {
            return Variable(type, name, Annotations.Empty);
        }
        [Obsolete("use Expression.Variable instead")]
        public static VariableExpression Temporary(Type type, string name, Annotations annotations) {
            return Variable(type, name, annotations);
        }
    }
}
