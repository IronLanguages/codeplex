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
    /// <summary>
    /// Represents the initialization of a list.
    /// </summary>
    public sealed class ElementInit : IArgumentProvider {
        private MethodInfo _addMethod;
        private ReadOnlyCollection<Expression> _arguments;

        internal ElementInit(MethodInfo addMethod, ReadOnlyCollection<Expression> arguments) {
            _addMethod = addMethod;
            _arguments = arguments;
        }
        /// <summary>
        /// Gets the <see cref="MethodInfo"/> used to add elements to the object.
        /// </summary>
        public MethodInfo AddMethod {
            get { return _addMethod; }
        }

        /// <summary>
        /// Gets the list of elements to be added to the object.
        /// </summary>
        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }

        Expression IArgumentProvider.GetArgument(int index) {
            return _arguments[index];
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return _arguments.Count;
            }
        }

        /// <summary>
        /// Creates a <see cref="String"/> representation of the node.
        /// </summary>
        /// <returns>A <see cref="String"/> representation of the node.</returns>
        public override string ToString() {
            return ExpressionStringBuilder.ElementInitBindingToString(this);
        }
    }


    public partial class Expression {
        //CONFORMING
        /// <summary>
        /// Creates an <see cref="Microsoft.Linq.Expressions.ElementInit">ElementInit</see> expression that represents the initialization of a list.
        /// </summary>
        /// <param name="addMethod">The <see cref="MethodInfo"/> for the list's Add method.</param>
        /// <param name="arguments">An array containing the Expressions to be used to initialize the list.</param>
        /// <returns>The created <see cref="Microsoft.Linq.Expressions.ElementInit">ElementInit</see> expression.</returns>
        public static ElementInit ElementInit(MethodInfo addMethod, params Expression[] arguments) {
            return ElementInit(addMethod, arguments as IEnumerable<Expression>);
        }

        //CONFORMING
        /// <summary>
        /// Creates an <see cref="Microsoft.Linq.Expressions.ElementInit">ElementInit</see> expression that represents the initialization of a list.
        /// </summary>
        /// <param name="addMethod">The <see cref="MethodInfo"/> for the list's Add method.</param>
        /// <param name="arguments">An <see cref="IEnumerable{T}"/> containing <see cref="Expression"/> elements to initialize the list.</param>
        /// <returns>The created <see cref="Microsoft.Linq.Expressions.ElementInit">ElementInit</see> expression.</returns>
        public static ElementInit ElementInit(MethodInfo addMethod, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(addMethod, "addMethod");
            ContractUtils.RequiresNotNull(arguments, "arguments");
            RequiresCanRead(arguments, "arguments");
            ValidateElementInitAddMethodInfo(addMethod);
            ReadOnlyCollection<Expression> argumentsRO = arguments.ToReadOnly();
            ValidateArgumentTypes(addMethod, ExpressionType.Call, ref argumentsRO);
            return new ElementInit(addMethod, argumentsRO);
        }

        //CONFORMING
        private static void ValidateElementInitAddMethodInfo(MethodInfo addMethod) {
            ValidateMethodInfo(addMethod);
            ParameterInfo[] pis = addMethod.GetParametersCached();
            if (pis.Length == 0) {
                throw Error.ElementInitializerMethodWithZeroArgs();
            }
            if (!addMethod.Name.Equals("Add", StringComparison.OrdinalIgnoreCase)) {
                throw Error.ElementInitializerMethodNotAdd();
            }
            if (addMethod.IsStatic) {
                throw Error.ElementInitializerMethodStatic();
            }
            foreach (ParameterInfo pi in pis) {
                if (pi.ParameterType.IsByRef) {
                    throw Error.ElementInitializerMethodNoRefOutParam(pi.Name, addMethod.Name);
                }
            }
        }
    }
}
