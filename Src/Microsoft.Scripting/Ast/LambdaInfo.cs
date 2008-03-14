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
        private readonly Dictionary<VariableExpression, VariableInfo> _variables = new Dictionary<VariableExpression, VariableInfo>();

        /// <summary>
        /// Variables referenced from this lambda.
        /// </summary>
        private readonly Dictionary<VariableInfo, Slot> _slots = new Dictionary<VariableInfo, Slot>();

        /// <summary>
        /// Try statements in this lambda (if the lambda is generator)
        /// </summary>
        private Dictionary<TryStatement, TryStatementInfo> _tryInfos;

        /// <summary>
        /// Yield statements in this lambda (if the lambda has yields)
        /// </summary>
        private Dictionary<YieldStatement, YieldTarget> _yieldTargets;

        /// <summary>
        /// The factory to create the environment (if the lambda has environment)
        /// </summary>
        private EnvironmentFactory _environmentFactory;

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
        /// The count of generator temps required to generate the lambda
        /// (if the lambda is a generator)
        /// </summary>
        private int _generatorTemps;

        /// <summary>
        /// The top targets for the generator dispatch.
        /// (if the lambda is a generator)
        /// </summary>
        private IList<YieldTarget> _topTargets;

        internal LambdaInfo(LambdaExpression lambda, LambdaInfo parent) {
            _lambda = lambda;
            _parent = parent;

            if (lambda != null) {
                int index = 0;
                foreach (VariableExpression v in lambda.Parameters) {
                    _variables.Add(v, new VariableInfo(v, _lambda, index++));
                }
                foreach (VariableExpression v in lambda.Variables) {
                    _variables.Add(v, new VariableInfo(v, _lambda));
                }
            }
        }

        internal LambdaExpression Lambda {
            get { return _lambda; }
        }

        internal LambdaInfo Parent {
            get { return _parent; }
        }

        internal Dictionary<VariableInfo, Slot> Slots {
            get { return _slots; }
        }

        internal Dictionary<VariableExpression, VariableInfo> Variables {
            get { return _variables; }
        }

        internal EnvironmentFactory EnvironmentFactory {
            get { return _environmentFactory; }
            set { _environmentFactory = value; }
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

        protected internal int GeneratorTemps {
            get { return _generatorTemps; }
        }

        internal IList<YieldTarget> TopTargets {
            get { return _topTargets; }
        }

        /// <summary>
        /// Marks the variable as being referenced in this lambda
        /// </summary>
        internal void AddVariableReference(VariableExpression variable) {
            _slots[GetVariableInfo(variable)] = null;
        }

        internal void AddGeneratorTemps(int count) {
            _generatorTemps += count;
        }

        internal void PopulateGeneratorInfo(Dictionary<TryStatement, TryStatementInfo> tryInfos,
                                            Dictionary<YieldStatement, YieldTarget> yieldTargets,
                                            List<YieldTarget> topTargets,
                                            int temps) {
            _tryInfos = tryInfos;
            _yieldTargets = yieldTargets;
            _topTargets = topTargets;
            AddGeneratorTemps(temps);
        }

        internal TryStatementInfo TryGetTsi(TryStatement ts) {
            TryStatementInfo tsi;
            if (_tryInfos != null && _tryInfos.TryGetValue(ts, out tsi)) {
                return tsi;
            } else {
                return null;
            }
        }

        internal YieldTarget TryGetYieldTarget(YieldStatement ys) {
            YieldTarget yt;
            if (_yieldTargets != null && _yieldTargets.TryGetValue(ys, out yt)) {
                return yt;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Gets the compiler state corresponding to the Variable
        /// Searches parent LambdaInfos if the variable is not defined in this lambda
        /// </summary>
        internal VariableInfo GetVariableInfo(VariableExpression variable) {
            VariableInfo result;
            LambdaInfo definingLambda = this;
            while (!definingLambda.Variables.TryGetValue(variable, out result)) {
                definingLambda = definingLambda.Parent;
                if (definingLambda == null) {
                    throw new ArgumentException("Could not resolve Variable " + variable.Name);
                }
            }
            return result;
        }

        internal void CreateReferenceSlots(LambdaCompiler cg) {
            foreach (VariableInfo vi in new List<VariableInfo>(_slots.Keys)) {
                _slots[vi] = vi.CreateSlot(cg, this);
            }
        }

        internal Slot GetVariableSlot(VariableExpression variable) {
            return _slots[GetVariableInfo(variable)];
        }
    }
}
