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

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class NewExpression : Expression {
        private readonly ConstructorInfo _constructor;
        private readonly ReadOnlyCollection<Expression>/*!*/ _arguments;

        internal NewExpression(Type type, ConstructorInfo constructor, ReadOnlyCollection<Expression>/*!*/ arguments, CreateInstanceAction bindingInfo)
            : base(Annotations.Empty, AstNodeType.New, type, bindingInfo) {
            if (IsBound) {
                RequiresBoundItems(arguments, "arguments");
            }
            _constructor = constructor;
            _arguments = arguments;
        }

        public ConstructorInfo Constructor {
            get { return _constructor; }
        }

        public ReadOnlyCollection<Expression>/*!*/ Arguments {
            get { return _arguments; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        public static NewExpression New(ConstructorInfo/*!*/ constructor, params Expression/*!*/[]/*!*/ arguments) {
            return New(constructor, (IList<Expression>)arguments);
        }

        public static NewExpression New(ConstructorInfo/*!*/ constructor, IList<Expression/*!*/>/*!*/ arguments) {
            ContractUtils.RequiresNotNull(constructor, "constructor");
            ContractUtils.RequiresNotNullItems(arguments, "arguments");
            ContractUtils.Requires(!constructor.DeclaringType.ContainsGenericParameters, "constructor", "Cannot instantiate an open generic type");

            ParameterInfo[] parameters = constructor.GetParameters();
            ValidateCallArguments(parameters, arguments);

            return new NewExpression(constructor.DeclaringType, constructor, CollectionUtils.ToReadOnlyCollection(arguments), null);
        }

        public static NewExpression New(Type type) {
            ContractUtils.RequiresNotNull(type, "type");

            ReadOnlyCollection<Expression> noArgs = CollectionUtils.ToReadOnlyCollection<Expression>(new Expression[0]);

            if (type.IsValueType) {
                return new NewExpression(type, null, noArgs, null);
            } else {
                ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
                ContractUtils.Requires(ci != null, "type", "type must have a parameterless constructor");
                return new NewExpression(type, ci, noArgs, null);
            }
        }

        public static NewExpression New(Type result, CreateInstanceAction bindingInfo, params Expression/*!*/[]/*!*/ arguments) {
            return New(result, bindingInfo, (IList<Expression>)arguments);
        }

        public static NewExpression New(Type result, CreateInstanceAction bindingInfo, IList<Expression/*!*/>/*!*/ arguments) {
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");
            ContractUtils.RequiresNotNullItems(arguments, "arguments");
            return new NewExpression(result, null, CollectionUtils.ToReadOnlyCollection(arguments), bindingInfo);
        }

        public static NewExpression SimpleNewHelper(ConstructorInfo/*!*/ constructor, params Expression/*!*/[]/*!*/ arguments) {
            ContractUtils.RequiresNotNull(constructor, "constructor");
            ContractUtils.RequiresNotNullItems(arguments, "arguments");

            ParameterInfo[] parameters = constructor.GetParameters();
            ContractUtils.Requires(arguments.Length == parameters.Length, "arguments", "Incorrect number of arguments");

            return New(constructor, ArgumentConvertHelper(arguments, parameters));
        }
    }
}
