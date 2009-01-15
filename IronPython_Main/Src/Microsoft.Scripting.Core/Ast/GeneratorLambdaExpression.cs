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
using System.Diagnostics;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Generator lambda (lambda with yield statements).
    /// 
    /// To create the generator, the AST node requires 2 types.
    /// First is the type of the generator object. The code generation will emit code to instantiate
    /// this type using constructor GeneratorType(CodeContext, DelegateType)
    /// 
    /// The GeneratorType must inherit from Generator
    /// The second type is the Delegate type in the above constructor call.
    /// 
    /// The inner function of the generator will have the signature:
    /// bool GetNext(GeneratorType, out object value);
    /// </summary>
    public sealed class GeneratorLambdaExpression : LambdaExpression {
        /// <summary>
        /// The type of the generator instance.
        /// The LambdaExpression will emit code to create a new instance of this type, using constructor:
        /// GeneratorType(CodeContext context, Delegate next);
        /// </summary>
        private readonly Type _generator;
        /// <summary>
        /// The type of the delegate to produce the next element.
        /// </summary>
        private readonly Type _next;

        internal GeneratorLambdaExpression(Annotations annotations, Type lambdaType, string name, Type generator, Type next, MethodInfo scopeFactory,
            Expression body, ReadOnlyCollection<ParameterExpression> parameters, ReadOnlyCollection<VariableExpression> variables)
            : base(annotations, AstNodeType.Generator, lambdaType, name, typeof(object), scopeFactory, body, parameters, variables, false, true, false, false) {
            _generator = generator;
            _next = next;
        }

        public Type GeneratorType {
            get { return _generator; }
        }

        public Type DelegateType {
            get { return _next; }
        }
    }

    public partial class Expression {
        public static LambdaExpression Generator(SourceSpan span, Type delegateType, string name, Type generatorType, Type next, 
            MethodInfo scopeFactory, Expression body, ParameterExpression[] parameters, VariableExpression[] variables) {
            CodeContract.RequiresNotNull(name, "name");
            CodeContract.RequiresNotNull(delegateType, "delegateType");
            CodeContract.RequiresNotNull(generatorType, "generatorType");
            CodeContract.RequiresNotNull(next, "next");
            CodeContract.RequiresNotNull(body, "body");
            CodeContract.Requires(TypeUtils.CanAssign(typeof(Generator), generatorType), "generatorType", "The generator type must inherit from Generator");
            CodeContract.RequiresNotNullItems(parameters, "parameters");
            CodeContract.RequiresNotNullItems(variables, "variables");

            ValidateScopeFactory(scopeFactory, "scopeFactory");

            LambdaExpression lambda = new GeneratorLambdaExpression(
                Annotate(span), delegateType, name, generatorType, next, scopeFactory,
                body, CollectionUtils.ToReadOnlyCollection(parameters), CollectionUtils.ToReadOnlyCollection(variables)
            );

            ValidateDelegateType(lambda, delegateType);

            return lambda;
        }
    }
}
