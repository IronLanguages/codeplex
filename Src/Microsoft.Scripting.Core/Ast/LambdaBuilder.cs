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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// The builder for creating the LambdaExpression and GeneratorLambdaExpression nodes
    /// Since the nodes require that parameters and variables are created
    /// before hand and then passed to the factories creating LambdaExpression or
    /// GeneratorLambdaExpression, this builder keeps track of the different pieces
    /// and at the end creates the LambdaExpression/GeneratorLambdaExpression.
    /// </summary>
    public class LambdaBuilder {
        private SourceSpan _span;
        private string _name;
        private Type _returnType;
        private List<VariableExpression> _locals;
        private List<ParameterExpression> _params;
        private ParameterExpression _paramsArray;
        private Expression _body;
        private bool _dictionary;
        private bool _global;
        private bool _visible = true;
        private bool _completed;

        internal LambdaBuilder(SourceSpan span, string name, Type returnType) {
            _span = span;
            _name = name;
            _returnType = returnType;
        }

        /// <summary>
        /// The source span of the lambda being built
        /// </summary>
        public SourceSpan Span {
            get {
                return _span;
            }
            set {
                _span = value;
            }
        }

        /// <summary>
        /// The name of the lambda.
        /// Currently anonymous/unnamed lambdas are not allowed.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
            set {
                Contract.RequiresNotNull(value, "value");
                _name = value;
            }
        }

        /// <summary>
        /// Return type of the lambda being created.
        /// </summary>
        public Type ReturnType {
            get {
                return _returnType;
            }
            set {
                Contract.RequiresNotNull(value, "value");
                _returnType = value;
            }
        }

        /// <summary>
        /// List of lambda's local variables for direct manipulation.
        /// </summary>
        public List<VariableExpression> Locals {
            get {
                if (_locals == null) {
                    _locals = new List<VariableExpression>();
                }
                return _locals;
            }
        }

        /// <summary>
        /// List of lambda's parameters for direct manipulation
        /// </summary>
        public List<ParameterExpression> Parameters {
            get {
                if (_params == null) {
                    _params = new List<ParameterExpression>();
                }
                return _params;
            }
        }

        /// <summary>
        /// The params array argument, if any.
        /// </summary>
        public ParameterExpression ParamsArray {
            get {
                return _paramsArray;
            }
        }

        /// <summary>
        /// The body of the lambda. This must be non-null.
        /// </summary>
        public Expression Body {
            get {
                return _body;
            }
            set {
                Contract.RequiresNotNull(value, "value");
                _body = value;
            }
        }

        /// <summary>
        /// The generated lambda should have dictionary of locals
        /// instead of allocating them directly on the CLR stack.
        /// </summary>
        public bool Dictionary {
            get {
                return _dictionary;
            }
            set {
                _dictionary = value;
            }
        }

        /// <summary>
        /// The resulting lambda should be marked as global.
        /// This should go away eventually.
        /// </summary>
        public bool Global {
            get {
                return _global;
            }
            set {
                _global = value;
            }
        }

        /// <summary>
        /// The scope is visible (default). Invisible if false.
        /// </summary>
        public bool Visible {
            get {
                return _visible;
            }
            set {
                _visible = value;
            }
        }

        /// <summary>
        /// Creates a parameter on the lambda with a given name and type.
        /// 
        /// Parameters maintain the order in which they are created,
        /// however custom ordering is possible via direct access to
        /// Parameters collection.
        /// </summary>
        public ParameterExpression CreateParameter(SymbolId name, Type type) {
            Contract.RequiresNotNull(type, "type");
            return Save(Ast.Parameter(type, SymbolTable.IdToString(name)));
        }

        /// <summary>
        /// Creates a parameter on the lambda with a given name and type.
        /// 
        /// Parameters maintain the order in which they are created,
        /// however custom ordering is possible via direct access to
        /// Parameters collection.
        /// </summary>
        public ParameterExpression CreateParameter(string name, Type type) {
            Contract.RequiresNotNull(type, "type");
            return Save(Ast.Parameter(type, name));
        }

        /// <summary>
        /// Creates a params array argument on the labmda.
        /// 
        /// The params array argument is added to the signature immediately. Before the lambda is
        /// created, the builder validates that it is still the last (since the caller can modify
        /// the order of parameters explicitly by maniuplating the parameter list)
        /// </summary>
        public ParameterExpression CreataParamsArray(SymbolId name, Type type) {
            Contract.RequiresNotNull(type, "type");
            Contract.Requires(type.IsArray, "type");
            Contract.Requires(type.GetArrayRank() == 1, "type");
            Contract.Requires(_paramsArray == null, "type", "Already have parameter array");

            return Save(_paramsArray = Ast.Parameter(type, SymbolTable.IdToString(name)));
        }

        /// <summary>
        /// Creates a global variable with specified name and type.
        /// </summary>
        public VariableExpression CreateGlobalVariable(SymbolId name, Type type) {
            return Save(VariableExpression.Global(name, type));
        }

        /// <summary>
        /// Creates a local variable with specified name and type.
        /// </summary>
        public VariableExpression CreateLocalVariable(SymbolId name, Type type) {
            return Save(VariableExpression.Local(name, type));
        }

        /// <summary>
        /// Creates a temporary variable with specified name and type.
        /// </summary>
        public VariableExpression CreateTemporaryVariable(SymbolId name, Type type) {
            return Save(VariableExpression.Temporary(name, type));
        }

        /// <summary>
        /// Adds the temporary variable to the list of variables maintained
        /// by the builder. This is useful in cases where the variable is
        /// created outside of the builder.
        /// </summary>
        public void AddTemp(VariableExpression temp) {
            Contract.RequiresNotNull(temp, "temp");
            Save(temp);
        }

        /// <summary>
        /// Creates the LambdaExpression from the builder.
        /// After this operation, the builder can no longer be used to create other instances.
        /// </summary>
        /// <param name="lambdaType">Desired type of the lambda. </param>
        /// <returns>New LambdaExpression instance.</returns>
        public LambdaExpression MakeLambda(Type lambdaType) {
            Validate();
            LambdaExpression lambda = Ast.Lambda(_span, lambdaType, _name, _returnType, _body, ToArray(_params), ToArray(_locals),
                                            _global, _visible, _dictionary, _paramsArray != null);

            // The builder is now completed
            _completed = true;

            return lambda;
        }

        /// <summary>
        /// Creates the LambdaExpression from the builder.
        /// After this operation, the builder can no longer be used to create other instances.
        /// </summary>
        /// <returns>New LambdaExpression instance.</returns>
        public LambdaExpression MakeLambda() {
            Contract.Requires(_paramsArray == null, "Paramarray lambdas require explicit delegate type");
            Validate();
            LambdaExpression lambda = Ast.Lambda(_span, _name, _returnType, _body, ToArray(_params), ToArray(_locals),
                                            _global, _visible, _dictionary);

            // The builder is now completed
            _completed = true;

            return lambda;
        }


        /// <summary>
        /// Creates the generator LambdaExpression from the builder.
        /// After this operation, the builder can no longer be used to create other instances.
        /// </summary>
        /// <returns>New LambdaExpression instance.</returns>
        public LambdaExpression MakeGenerator(Type lambdaType, Type generator, Type next) {
            Contract.RequiresNotNull(generator, "generator");
            Contract.RequiresNotNull(next, "next");

            Validate();
            LambdaExpression lambda = Ast.Generator(_span, lambdaType, _name, generator, next, _body, ToArray(_params), ToArray(_locals));

            // The builder is now completed
            _completed = true;

            return lambda;
        }

        /// <summary>
        /// Validates that the builder has enough information to create the lambda.
        /// </summary>
        private void Validate() {
            if (_completed) {
                throw new InvalidOperationException("The builder is closed");
            }
            if (_returnType == null) {
                throw new InvalidOperationException("Return type is missing");
            }
            if (_name == null) {
                throw new InvalidOperationException("Name is missing");
            }
            if (_body == null) {
                throw new InvalidOperationException("Body is missing");
            }

            if (_paramsArray != null &&
                (_params.Count == 0 || _params[_params.Count -1] != _paramsArray)) {
                throw new InvalidOperationException("The params array parameter is not last in the parameter list");
            }
        }

        private static T[] ToArray<T>(List<T> list) {
            return list != null ? list.ToArray() : new T[0];
        }

        /// <summary>
        /// Saves local/global/temporary variable into locals, creating the list if not yet created.
        /// </summary>
        private VariableExpression Save(VariableExpression var) {
            if (_locals == null) {
                _locals = new List<VariableExpression>();
            }
            _locals.Add(var);
            return var;
        }

        /// <summary>
        /// Saves parameter into parmaters, creating the list if not yet created.
        /// </summary>
        private ParameterExpression Save(ParameterExpression var) {
            if (_params == null) {
                _params = new List<ParameterExpression>();
            }
            _params.Add(var);
            return var;
        }
    }

    public static partial class Ast {
        /// <summary>
        /// Creates new instnace of the LambdaBuilder with specified name and a return type.
        /// </summary>
        /// <param name="name">Name for the lambda being built.</param>
        /// <param name="returnType">Return type of the lambda being built.</param>
        /// <returns>new LambdaBuilder instance</returns>
        public static LambdaBuilder Lambda(string name, Type returnType) {
            return Lambda(SourceSpan.None, name, returnType);
        }

        /// <summary>
        /// Creates new instance of the LambdaBuilder with specified name, return type and a source span.
        /// </summary>
        /// <param name="span">SourceSpan for the lambda being built.</param>
        /// <param name="name">Name of the lambda being built.</param>
        /// <param name="returnType">Return type of the lambda being built.</param>
        /// <returns>New instance of the </returns>
        public static LambdaBuilder Lambda(SourceSpan span, string name, Type returnType) {
            return new LambdaBuilder(span, name, returnType);
        }
    }
}
