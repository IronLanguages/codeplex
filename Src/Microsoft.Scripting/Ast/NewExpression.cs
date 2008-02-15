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
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class NewExpression : Expression {
        private readonly ConstructorInfo/*!*/ _constructor;
        private readonly ReadOnlyCollection<Expression>/*!*/ _arguments;

        internal NewExpression(ConstructorInfo/*!*/ constructor, ReadOnlyCollection<Expression>/*!*/ arguments)
            : base(AstNodeType.New, constructor.DeclaringType) {
            _constructor = constructor;
            _arguments = arguments;
        }

        public ConstructorInfo/*!*/ Constructor {
            get { return _constructor; }
        }

        public ReadOnlyCollection<Expression>/*!*/ Arguments {
            get { return _arguments; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static NewExpression New(ConstructorInfo/*!*/ constructor, params Expression/*!*/[]/*!*/ arguments) {
            return New(constructor, (IList<Expression>)arguments);
        }

        public static NewExpression New(ConstructorInfo/*!*/ constructor, IList<Expression/*!*/>/*!*/ arguments) {
            Contract.RequiresNotNull(constructor, "constructor");
            Contract.RequiresNotNullItems(arguments, "arguments");
            Contract.Requires(!constructor.DeclaringType.ContainsGenericParameters, "constructor", "Cannot instantiate an open generic type");

            ParameterInfo[] parameters = constructor.GetParameters();
            ValidateCallArguments(parameters, arguments);

            return new NewExpression(constructor, CollectionUtils.ToReadOnlyCollection(arguments));
        }

        public static NewExpression SimpleNewHelper(ConstructorInfo/*!*/ constructor, params Expression/*!*/[]/*!*/ arguments) {
            Contract.RequiresNotNull(constructor, "constructor");
            Contract.RequiresNotNullItems(arguments, "arguments");

            ParameterInfo[] parameters = constructor.GetParameters();
            Contract.Requires(arguments.Length == parameters.Length, "arguments", "Incorrect number of arguments");

            return New(constructor, ArgumentConvertHelper(arguments, parameters));
        }
    }
}
