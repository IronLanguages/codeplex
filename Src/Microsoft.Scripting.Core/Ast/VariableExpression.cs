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

        internal VariableExpression(Type type, string name, Annotations annotations)
            : base(ExpressionType.Variable, type, false, annotations, true, true) {
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
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(type != typeof(void), "type", Strings.ArgumentCannotBeOfTypeVoid);
            ContractUtils.Requires(!type.IsByRef, "type", Strings.TypeMustNotBeByRef);
            return new VariableExpression(type, name, annotations);
        }
    }
}
