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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// LambdaInfo is a data structure in which Compiler keeps information related co compiling
    /// LambdaExpression. Within the tree the parent/child relationship is expressed implicitly through
    /// the tree structure. Because compiler needs to walk the parent chain (for example to resolve
    /// closures), the parent link is kept here as a reference to the parent's LambdaInfo.
    /// code:LambdaExpression.
    /// </summary>
    class LambdaInfo {
        /// <summary>
        /// The LambdaExpression to which this info belongs.
        /// </summary>
        private readonly LambdaExpression _lambda;

        /// <summary>
        /// The lambda info of the parent lambda. Parent is lexically enclosing lambda.
        /// </summary>
        private readonly LambdaInfo _parent;

        /// <summary>
        /// Variables defined in this lambda.
        /// </summary>
        private readonly Dictionary<Expression, VariableInfo> _variables = new Dictionary<Expression, VariableInfo>();

        /// <summary>
        /// Variables referenced from this lambda.
        /// </summary>
        private readonly Dictionary<Expression, Slot> _referenceSlots = new Dictionary<Expression, Slot>();

        /// <summary>
        /// The factory to create the environment (if the lambda has environment)
        /// </summary>
        private EnvironmentAllocator _environmentAllocator;

        /// <summary>
        /// If this lambda has an environment and is a closure, this points at
        /// the parent's environmnet
        /// </summary>
        private Storage _parentEnvironmentStorage;

        /// <summary>
        /// The lambda is a closure (it references variables outside its own scope)
        /// </summary>
        private bool _isClosure;

        /// <summary>
        /// The lambda has environment
        /// Either its variables are referenced from nested scopes, or the lambda
        /// is a generator, or outputs its locals into an enviroment altogether
        /// </summary>
        private bool _hasEnvironment;

        /// <summary>
        /// Slots to access outer scopes. For now this is dictionary, even though list would be better.
        /// as soon as ScopeId goes away, make this list or something better.
        /// </summary>
        private readonly Dictionary<LambdaExpression, Slot> _closureAccess = new Dictionary<LambdaExpression, Slot>();

        private StorageAllocator _globalAllocator;
        private StorageAllocator _localAllocator;

        internal LambdaInfo(LambdaExpression lambda, LambdaInfo parent) {
            _lambda = lambda;
            _parent = parent;
        }

        internal LambdaExpression Lambda {
            get { return _lambda; }
        }

        internal LambdaInfo Parent {
            get { return _parent; }
        }

        internal Dictionary<Expression, Slot> ReferenceSlots {
            get { return _referenceSlots; }
        }

        internal Dictionary<Expression, VariableInfo> Variables {
            get { return _variables; }
        }

        internal EnvironmentAllocator EnvironmentAllocator {
            get { return _environmentAllocator; }
            set { _environmentAllocator = value; }
        }

        internal Storage ParentEnvironmentStorage {
            get { return _parentEnvironmentStorage; }
            set { _parentEnvironmentStorage = value; }
        }

        /// <summary>
        /// The method refers to a variable in one of its parents lexical context and will need an environment
        /// flown into it.  A function which is a closure does not necessarily contain an Environment unless
        /// it contains additional closures or uses language features which require lifting all locals to
        /// an environment.
        /// </summary>
        internal bool IsClosure {
            get { return _isClosure; }
            set { _isClosure = value; }
        }

        /// <summary>
        /// Scopes with environments will have some locals stored within a dictionary (FunctionEnvironment).  If
        /// we are also a closure an environment is flown into the method and our environment will point to the
        /// parent environment.  Ultimately this will enable our children to get at our or our parents envs.
        /// 
        /// Upon entering a function with an environment a new CodeContext will be allocated with a new
        /// FunctionEnviroment as its locals.  In the case of a generator this new CodeContext and environment
        /// is allocated in the function called to create the Generator, not the function that implements the
        /// Generator body.
        /// 
        /// The environment is provided as the Locals of a CodeContext or in the case of a Generator 
        /// as the parentEnvironment field.
        /// </summary>
        internal bool HasEnvironment {
            get { return _hasEnvironment; }
            set { _hasEnvironment = value; }
        }

        internal Dictionary<LambdaExpression, Slot> ClosureAccess {
            get { return _closureAccess; }
        }

        internal StorageAllocator LocalAllocator {
            get { return _localAllocator; }
        }

        internal StorageAllocator GlobalAllocator {
            get { return _globalAllocator; }
        }

        /// <summary>
        /// Gets the compiler state corresponding to the variable/parameter
        /// Searches parent LambdaInfos if the variable is not defined in this lambda
        /// </summary>
        internal VariableInfo GetVariableInfo(Expression variable) {
            Debug.Assert(variable is VariableExpression || variable is ParameterExpression);

            VariableInfo result;
            LambdaInfo definingLambda = this;
            while (!definingLambda.Variables.TryGetValue(variable, out result)) {
                definingLambda = definingLambda.Parent;
                if (definingLambda == null) {
                    throw new ArgumentException("Could not resolve Variable " + VariableInfo.GetName(variable));
                }
            }
            return result;
        }

        internal void SetAllocators(StorageAllocator global, StorageAllocator local) {
            _localAllocator = local;
            _globalAllocator = global;
        }
    }
}
