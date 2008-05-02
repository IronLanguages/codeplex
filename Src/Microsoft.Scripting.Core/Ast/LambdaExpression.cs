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
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// This captures a block of code that should correspond to a .NET method body.  It takes
    /// input through parameters and is expected to be fully bound.  This code can then be
    /// generated in a variety of ways.  The variables can be kept as .NET locals or in a
    /// 1st class environment object. This is the primary unit used for passing around
    /// AST's in the DLR.
    /// </summary>
    public class LambdaExpression : Expression {
        private readonly Type _returnType;
        private readonly string _name;
        private readonly Expression _body;
        private readonly MethodInfo _scopeFactory;

        private readonly ReadOnlyCollection<ParameterExpression> _parameters;
        private readonly ReadOnlyCollection<VariableExpression> _variables;

        // TODO: Evaluate necessity...
        #region Flags

        private readonly bool _isGlobal;
        private readonly bool _visibleScope;

        private readonly bool _emitLocalDictionary;
        private readonly bool _parameterArray;

        #endregion

        internal LambdaExpression(Annotations annotations, AstNodeType nodeType, Type delegateType, string name, Type returnType, MethodInfo scopeFactory,
            Expression body, ReadOnlyCollection<ParameterExpression> parameters, ReadOnlyCollection<VariableExpression> variables,
            bool global, bool visible, bool dictionary, bool parameterArray)
            : base(annotations, nodeType, delegateType) {

            Assert.NotNull(returnType);

            _name = name;
            _returnType = returnType;
            _scopeFactory = scopeFactory;
            _body = body;

            _parameters = parameters;
            _variables = variables;

            _isGlobal = global;
            _visibleScope = visible;
            _emitLocalDictionary = dictionary;
            _parameterArray = parameterArray;
        }

        public Type ReturnType {
            get { return _returnType; }
        }

        public ReadOnlyCollection<ParameterExpression> Parameters {
            get { return _parameters; }
        }

        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// Factory method used for local scope instantiation.
        /// If null the default factory <see cref="Microsoft.Scripting.Runtime.RuntimeHelpers.CreateLocalScope{TTuple}"/> is used.
        /// The factory must have the same signature as <see cref="Microsoft.Scripting.Runtime.RuntimeHelpers.CreateLocalScope{TTuple}"/>.
        /// </summary>
        public MethodInfo ScopeFactory {
            get { return _scopeFactory; }
        }

        /// <summary>
        /// True to force a function to have an environment and have all of its locals lifted
        /// into this environment.  This provides access to local variables via a dictionary but
        /// comes with the performance penality of not using the real stack for locals.
        /// </summary>
        public bool EmitLocalDictionary {
            get { return _emitLocalDictionary; }
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
        }

        public ReadOnlyCollection<VariableExpression> Variables {
            get { return _variables; }
        }
    }

    public partial class Expression {
        public static LambdaExpression GlobalLambda(Type type, string name, Expression body, ParameterExpression[] parameters, VariableExpression[] variables) {
            return Lambda(SourceSpan.None, type, name, typeof(object), (MethodInfo)null, body, parameters, variables, true, true, false, false);
        }

        public static LambdaExpression Lambda(string name, Type returnType, Expression body, ParameterExpression[] parameters, VariableExpression[] variables) {
            return Lambda(SourceSpan.None, name, returnType, (MethodInfo)null, body, parameters, variables, false, true, false);
        }

        public static LambdaExpression Lambda(Type delegateType, string name, Type returnType, Expression body, ParameterExpression[] parameters, VariableExpression[] variables) {
            return Lambda(SourceSpan.None, delegateType, name, returnType, body, parameters, variables);
        }

        public static LambdaExpression Lambda(SourceSpan span, Type delegateType, string name, Type returnType, Expression body, ParameterExpression[] parameters, VariableExpression[] variables) {
            return Lambda(span, delegateType, name, returnType, (MethodInfo)null, body, parameters, variables, false, true, false, false);
        }

        public static LambdaExpression Lambda(
            SourceSpan span, string name, Type returnType, MethodInfo scopeFactory,
            Expression body, ParameterExpression[] parameters, VariableExpression[] variables,
            bool global, bool visible, bool dictionary) {

            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(returnType, "returnType");
            ContractUtils.RequiresNotNull(body, "body");
            ContractUtils.RequiresNotNullItems(parameters, "parameters");
            ContractUtils.RequiresNotNullItems(variables, "variables");

            ValidateScopeFactory(scopeFactory, "scopeFactory");
            
            Type delegateType = GetDelegateTypeForSignature(parameters, returnType);
            return new LambdaExpression(
                Annotate(span), AstNodeType.Lambda, delegateType, name, returnType, scopeFactory, 
                body, CollectionUtils.ToReadOnlyCollection(parameters), CollectionUtils.ToReadOnlyCollection(variables), 
                global, visible, dictionary, false);
        }

        public static LambdaExpression Lambda(
            SourceSpan span, Type delegateType, string name, Type returnType, MethodInfo scopeFactory,
            Expression body, ParameterExpression[] parameters, VariableExpression[] variables,
            bool global, bool visible, bool dictionary, bool parameterArray) {

            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(returnType, "returnType");
            ContractUtils.RequiresNotNull(body, "body");
            ContractUtils.RequiresNotNullItems(parameters, "parameters");
            ContractUtils.RequiresNotNullItems(variables, "variables");

            ValidateScopeFactory(scopeFactory, "scopeFactory");
            
            LambdaExpression lambda = new LambdaExpression(
                Annotate(span), AstNodeType.Lambda, delegateType, name, returnType, scopeFactory,
                body, CollectionUtils.ToReadOnlyCollection(parameters), CollectionUtils.ToReadOnlyCollection(variables), 
                global, visible, dictionary, parameterArray);

            ValidateDelegateType(lambda, delegateType);

            return lambda;
        }

        /// <summary>
        /// Extracts the signature of the LambdaExpression as an array of Types
        /// </summary>
        internal static Type[] GetLambdaSignature(LambdaExpression lambda) {
            Debug.Assert(lambda != null);

            ReadOnlyCollection<ParameterExpression> parameters = lambda.Parameters;
            Type[] result = new Type[parameters.Count];
            for (int i = 0; i < parameters.Count; i++) {
                result[i] = parameters[i].Type;
            }
            return result;
        }

        private static void ValidateScopeFactory(MethodInfo factory, string paramName) {
            ContractUtils.Requires(IsValidScopeFactory(factory), paramName, "Factory must have signature compatible with " + 
                "CodeContext Factory<TTuple>(TTuple, SymbolId[], CodeContext, bool) where TTuple : Tuple");
        }
    
        private static bool IsValidScopeFactory(MethodInfo factory) {
            if (factory == null) return true;
            if (!factory.IsGenericMethod) return false;

            Type[] typeParams = factory.GetGenericArguments();
            if (typeParams.Length != 1) return false;

            Type[] constraints = typeParams[0].GetGenericParameterConstraints();
            if (constraints.Length != 1 || constraints[0] != typeof(Tuple)) return false;

            if (!typeof(CodeContext).IsAssignableFrom(factory.ReturnType)) return false;

            ParameterInfo[] ps = factory.GetParameters();
            if (ps.Length != 4) return false;
            if (ps[0].ParameterType != typeParams[0]) return false;
            if (ps[1].ParameterType != typeof(SymbolId[])) return false;
            if (ps[2].ParameterType != typeof(CodeContext)) return false;
            if (ps[3].ParameterType != typeof(bool)) return false;

            return true;
        }

        /// <summary>
        /// Validates that the delegate type of the lambda
        /// matches the lambda itself.
        /// 
        /// * Return types of the lambda and the delegate must be identical.
        /// 
        /// * Without parameter array on the delegate type, the signatures must
        ///   match perfectly as to count and types of parameters.
        ///   
        /// * With parameter array on the delegate type, the common subset of
        ///   parameters must match
        /// </summary>
        private static void ValidateDelegateType(LambdaExpression lambda, Type delegateType) {
            ContractUtils.Requires(delegateType != typeof(Delegate), "type", "type must not be System.Delegate.");
            ContractUtils.Requires(TypeUtils.CanAssign(typeof(Delegate), delegateType), "type", "Incorrect delegate type.");

            MethodInfo mi = delegateType.GetMethod("Invoke");
            ContractUtils.RequiresNotNull(mi, "Delegate must have an 'Invoke' method");

            ContractUtils.Requires(mi.ReturnType == lambda.ReturnType, "type", "Delegate type doesn't match LambdaExpression");

            ParameterInfo[] infos = mi.GetParameters();
            ReadOnlyCollection<ParameterExpression> parameters = lambda.Parameters;

            if (infos.Length > 0 && CompilerHelpers.IsParamArray(infos[infos.Length - 1])) {
                ContractUtils.Requires(infos.Length - 1 <= parameters.Count, "Delegate and lambda parameter count mismatch");

                // Parameter array case. The lambda may have more parameters than delegate,
                // and can also have parameter array as its last parameter, however all of the
                // parameters upto delegate's parameter array (excluding) must be identical

                ValidateIdenticalParameters(infos, parameters, infos.Length - 1);
            } else {
                ContractUtils.Requires(infos.Length == parameters.Count, "Delegate and lambda parameter count mismatch");

                // No parameter array. The lambda must have identical signature to that of the
                // delegate, and it may not be marked as parameter array itself.
                ValidateIdenticalParameters(infos, parameters, infos.Length);

                ContractUtils.Requires(!lambda.ParameterArray, "lambda", "Parameter array delegate type required for parameter array lambda");
            }
        }

        private static void ValidateIdenticalParameters(ParameterInfo[] infos, ReadOnlyCollection<ParameterExpression> parameters, int count) {
            Debug.Assert(count <= infos.Length && count <= parameters.Count);
            while (count-- > 0) {
                ContractUtils.Requires(infos[count].ParameterType == parameters[count].Type, "type");
            }
        }

        private static Type GetDelegateTypeForSignature(IList<ParameterExpression> parameters, Type returnType) {
            ContractUtils.RequiresNotNull(returnType, "returnType");

            bool action = returnType == typeof(void);

            int paramCount = parameters == null ? 0 : parameters.Count;
            Type[] typeArgs = new Type[paramCount + (action ? 0 : 1)];
            for (int i = 0; i < paramCount; i++) {
                ContractUtils.RequiresNotNull(parameters[i], "parameters");
                typeArgs[i] = parameters[i].Type;
            }

            Type type;
            if (action)
                type = GetActionType(typeArgs);
            else {
                typeArgs[paramCount] = returnType;
                type = GetFuncType(typeArgs);
            }
            return type;
        }

        private static Type GetFuncType(params Type[] typeArgs) {
            ContractUtils.RequiresNotNull(typeArgs, "typeArgs");
            ContractUtils.Requires(typeArgs.Length > 0, "incorrect number of type arguments for Func.");

            Type funcType;

            switch (typeArgs.Length) {
                case 1:
                    funcType = typeof(Utils.Function<>).MakeGenericType(typeArgs);
                    break;
                case 2:
                    funcType = typeof(Utils.Function<,>).MakeGenericType(typeArgs);
                    break;
                case 3:
                    funcType = typeof(Utils.Function<,,>).MakeGenericType(typeArgs);
                    break;
                case 4:
                    funcType = typeof(Utils.Function<,,,>).MakeGenericType(typeArgs);
                    break;
                case 5:
                    funcType = typeof(Utils.Function<,,,,>).MakeGenericType(typeArgs);
                    break;
                default:
                    throw new ArgumentException("incorrect number of type arguments for Func.", "typeArgs"); ;
            }
            return funcType;
        }

        private static Type GetActionType(params Type[] typeArgs) {
            ContractUtils.RequiresNotNull(typeArgs, "typeArgs");

            Type actionType;

            switch (typeArgs.Length) {
                case 0:
                    actionType = typeof(Utils.Action);
                    break;
                case 1:
                    actionType = typeof(System.Action<>).MakeGenericType(typeArgs);
                    break;
                case 2:
                    actionType = typeof(Utils.Action<,>).MakeGenericType(typeArgs);
                    break;
                case 3:
                    actionType = typeof(Utils.Action<,,>).MakeGenericType(typeArgs);
                    break;
                case 4:
                    actionType = typeof(Utils.Action<,,,>).MakeGenericType(typeArgs);
                    break;
                default:
                    throw new ArgumentException("incorrect number of type arguments for Action.", "typeArgs"); ;
            }
            return actionType;
        }
    }
}
