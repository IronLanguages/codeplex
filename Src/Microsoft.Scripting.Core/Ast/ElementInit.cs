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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Scripting.Utils;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class ElementInit {
        private MethodInfo _addMethod;
        private ReadOnlyCollection<Expression> _arguments;

        internal ElementInit(MethodInfo addMethod, ReadOnlyCollection<Expression> arguments) {
            _addMethod = addMethod;
            _arguments = arguments;
        }
        public MethodInfo AddMethod {
            get { return _addMethod; }
        }
        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }
        internal void BuildString(StringBuilder builder) {
            builder.Append(AddMethod);
            builder.Append("(");
            bool first = true;
            foreach (Expression argument in _arguments) {
                if (first) {
                    first = false;
                } else {
                    builder.Append(",");
                }
                argument.BuildString(builder);
            }
            builder.Append(")");
        }
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            BuildString(sb);
            return sb.ToString();
        }
    }


    public partial class Expression {
        //CONFORMING
        public static ElementInit ElementInit(MethodInfo addMethod, params Expression[] arguments) {
            return ElementInit(addMethod, arguments as IEnumerable<Expression>);
        }
        //CONFORMING
        public static ElementInit ElementInit(MethodInfo addMethod, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(addMethod, "addMethod");
            RequiresCanRead(arguments, "arguments");
            ValidateElementInitAddMethodInfo(addMethod);
            ReadOnlyCollection<Expression> argumentsRO = arguments.ToReadOnly();
            ValidateArgumentTypes(addMethod, ref argumentsRO);
            return new ElementInit(addMethod, argumentsRO);
        }

        //CONFORMING
        private static void ValidateElementInitAddMethodInfo(MethodInfo addMethod) {
            ValidateMethodInfo(addMethod);
            if (addMethod.GetParameters().Length == 0) {
                throw Error.ElementInitializerMethodWithZeroArgs();
            }
            if (!addMethod.Name.Equals("Add", StringComparison.OrdinalIgnoreCase)) {
                throw Error.ElementInitializerMethodNotAdd();
            }
            if (addMethod.IsStatic) {
                throw Error.ElementInitializerMethodStatic();
            }
            foreach (ParameterInfo pi in addMethod.GetParameters()) {
                if (pi.ParameterType.IsByRef) {
                    throw Error.ElementInitializerMethodNoRefOutParam(pi.Name, addMethod.Name);
                }
            }
        }
    }
}