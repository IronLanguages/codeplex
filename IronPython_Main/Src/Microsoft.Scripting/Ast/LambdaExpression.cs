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
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using System.Collections.ObjectModel;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// This captures a block of code that should correspond to a .NET method body.  It takes
    /// input through parameters and is expected to be fully bound.  This code can then be
    /// generated in a variety of ways.  The variables can be kept as .NET locals or in a
    /// 1st class environment object. This is the primary unit used for passing around
    /// AST's in the DLR.
    /// </summary>
    public partial class LambdaExpression {
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;
        private readonly Type _returnType; 
        private readonly string _name;
        private Expression _body;

        private readonly ReadOnlyCollection<Variable> _parameters;
        private readonly List<Variable> _variables;

        // TODO: Evaluate necessity...
        #region Flags

        private readonly bool _isGlobal;
        private readonly bool _visibleScope;

        private readonly bool _emitLocalDictionary;
        private readonly bool _parameterArray;

        #endregion

        internal LambdaExpression(SourceSpan span, string name, Type returnType, Expression body, ReadOnlyCollection<Variable> parameters,
                           List<Variable> variables, bool global, bool visible, bool dictionary, bool parameterArray){

            Assert.NotNull(returnType);
            _start = span.Start;
            _end = span.End;

            _name = name;
            _returnType = returnType;
            _body = body;

            _parameters = parameters;
            _variables = variables;

            _isGlobal = global;
            _visibleScope = visible;
            _emitLocalDictionary = dictionary;
            _parameterArray = parameterArray;
        }

        public SourceLocation Start {
            get { return _start; }
        }

        public SourceLocation End {
            get { return _end; }
        }

        public SourceSpan Span {
            get {
                return new SourceSpan(_start, _end);
            }
        }

        public Type ReturnType {
            get { return _returnType; }
        }

        public ReadOnlyCollection<Variable> Parameters {
            get { return _parameters; }
        }

        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// True to force a function to have an environment and have all of its locals lifted
        /// into this environment.  This provides access to local variables via a dictionary but
        /// comes with the performance penality of not using the real stack for locals.
        /// </summary>
        public bool EmitLocalDictionary {
            get {
                // When custom frames are turned on, we emit dictionaries everywhere
                return ScriptDomainManager.Options.Frames || _emitLocalDictionary;
            }
        }

        public bool IsGlobal {
            get { return _isGlobal; }
        }

        internal bool ParameterArray {
            get { return _parameterArray; }
        }

        internal bool IsVisible {
            get { return _visibleScope; }
        }

        public Expression Body {
            get { return _body; }

            internal set { _body = value; }
        }

        public List<Variable> Variables {
            get { return _variables; }
        }

        internal Variable CreateTemporaryVariable(SymbolId name, Type type) {
            Variable variable = Variable.Temporary(name, type);
            variable.Lambda = this;
            _variables.Add(variable);
            return variable;
        }
    }

    public static partial class Ast {
        public static LambdaExpression GlobalLambda(string name, Expression body, Variable[] parameters, Variable[] variables) {
            return Lambda(SourceSpan.None, name, typeof(object), body, parameters, variables, true, true, false, false);
        }

        public static LambdaExpression Lambda(string name, Type returnType, Expression body, Variable[] parameters, Variable[] variables) {
            return Lambda(SourceSpan.None, name, returnType, body, parameters, variables);
        }

        public static LambdaExpression Lambda(SourceSpan span, string name, Type returnType, Expression body, Variable[] parameters, Variable[] variables) {
            return Lambda(span, name, returnType, body, parameters, variables, false, true, false, false);
        }

        public static LambdaExpression Lambda(SourceSpan span, string name, Type returnType, Expression body, Variable[] parameters, Variable[] variables,
                                          bool global, bool visible, bool dictionary, bool parameterArray) {
            Contract.RequiresNotNull(name, "name");
            Contract.RequiresNotNull(returnType, "returnType");
            Contract.RequiresNotNull(body, "body");
            Contract.RequiresNotNullItems(parameters, "parameters");
            Contract.RequiresNotNullItems(variables, "variables");

            LambdaExpression lambda = new LambdaExpression(span, name, returnType, body, CollectionUtils.ToReadOnlyCollection(parameters),
                                            new List<Variable>(variables), global, visible, dictionary, parameterArray);

            // TODO: Remove when variable no longer has block.
            SetBlock(parameters, lambda);
            SetBlock(variables, lambda);

            return lambda;
        }

        // TODO: Remove when variable no longer has block.
        private static void SetBlock(Variable[] variables, LambdaExpression lambda) {
            for (int i = 0; i < variables.Length; i++) {
                Contract.Requires(variables[i].Lambda == null, "variables");
                variables[i].Lambda = lambda;
            }
        }

        /// <summary>
        /// Extracts the signature of the LambdaExpression as an array of Types
        /// </summary>
        internal static Type[] GetLambdaSignature(LambdaExpression lambda) {
            Debug.Assert(lambda != null);

            ReadOnlyCollection<Variable> parameters = lambda.Parameters;
            Type[] result = new Type[parameters.Count];
            for (int i = 0; i < parameters.Count; i++) {
                result[i] = parameters[i].Type;
            }
            return result;
        }
    }
}
