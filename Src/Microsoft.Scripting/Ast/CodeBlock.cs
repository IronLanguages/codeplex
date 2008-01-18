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

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// This captures a block of code that should correspond to a .NET method body.  It takes
    /// input through parameters and is expected to be fully bound.  This code can then be
    /// generated in a variety of ways.  The variables can be kept as .NET locals or in a
    /// 1st class environment object.  This is the primary unit used for passing around
    /// AST's in the DLR.
    /// 
    /// TODO - This should probably not be a Node but that will require some substantial walker changes.
    /// </summary>
    public partial class CodeBlock {
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;
        private readonly Type _returnType;
        private readonly string _name;
        private CodeBlock _parent;
        private Expression _body;

        private readonly List<Variable> _parameters = new List<Variable>();
        private readonly List<Variable> _variables = new List<Variable>();

        #region Flags

        private bool _emitLocalDictionary;
        private bool _isGlobal;
        private bool _visibleScope = true;
        private bool _parameterArray;

        #endregion

        // TODO: Remove !!!
        private Expression _explicitCodeContextExpression;

        #region Compiler state - TODO: remove from here!

        private bool _isClosure;
        private bool _hasEnvironment;

        private Dictionary<Variable, VariableReference> _references;
        private EnvironmentFactory _environmentFactory;

        private int _generatorTemps;

        /// <summary>
        /// True, if the block is referenced by a declarative reference (CodeBlockExpression).
        /// </summary>
        private bool _declarativeReferenceExists;

        #endregion

        internal CodeBlock(SourceSpan span, string name, Type returnType) {
            Assert.NotNull(returnType);

            _name = name;
            _returnType = returnType;
            _start = span.Start;
            _end = span.End;
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

        public List<Variable> Parameters {
            get { return _parameters; }
        }

        public string Name {
            get { return _name; }
        }

        // TODO: Remove !!!
        public Expression ExplicitCodeContextExpression {
            get { return _explicitCodeContextExpression; }
            set { _explicitCodeContextExpression = value; }
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
            set {
                _emitLocalDictionary = value;
            }
        }

        public bool IsGlobal {
            get { return _isGlobal; }
            set { _isGlobal = value; }
        }

        public bool ParameterArray {
            get { return _parameterArray; }
            set { _parameterArray = value; }
        }

        public CodeBlock Parent {
            get { return _parent; }
            set { _parent = value; }
        }

        public bool IsVisible {
            get { return _visibleScope; }
            set { _visibleScope = value; }
        }
        public Expression Body {
            get { return _body; }
            set { _body = value; }
        }

        internal Dictionary<Variable, VariableReference> References {
            get { return _references; }
            set { _references = value; }
        }

        public List<Variable> Variables {
            get { return _variables; }
        }

        public Type EnvironmentType {
            get {
                Debug.Assert(_environmentFactory != null);
                return _environmentFactory.EnvironmentType;
            }
        }

        internal EnvironmentFactory EnvironmentFactory {
            get { return _environmentFactory; }
            set { _environmentFactory = value; }
        }

        protected internal int GeneratorTemps {
            get { return _generatorTemps; }
        }

        internal void DeclarativeReferenceAdded() {
            if (_declarativeReferenceExists) throw new InvalidOperationException("Block cannot be declared twice");
            _declarativeReferenceExists = true;
        }

        public Variable CreateParameter(SymbolId name, Type type) {
            Variable variable = Variable.Parameter(this, name, type);
            _parameters.Add(variable);
            return variable;
        }

        public Variable CreateParameter(SymbolId name, Type type, bool inParameterArray) {
            Variable variable = Variable.Parameter(this, name, type, inParameterArray);
            _parameters.Add(variable);
            return variable;
        }

        public Variable CreateVariable(SymbolId name, Variable.VariableKind kind, Type type) {
            return CreateVariable(name, kind, type, null);
        }

        public Variable CreateVariable(SymbolId name, Variable.VariableKind kind, Type type, Expression defaultValue) {
            Contract.Requires(kind != Variable.VariableKind.Parameter, "kind");

            Variable variable = Variable.Create(name, kind, this, type, defaultValue);
            _variables.Add(variable);
            return variable;
        }

        public Variable CreateLocalVariable(SymbolId name, Type type) {
            Variable variable = Variable.Local(name, this, type);
            _variables.Add(variable);
            return variable;
        }

        public Variable CreateTemporaryVariable(SymbolId name, Type type) {
            Variable variable = Variable.Temporary(name, this, type);
            _variables.Add(variable);
            return variable;
        }
        
        // TODO: Move away from here!!!
        internal void AddGeneratorTemps(int count) {
            _generatorTemps += count;
        }

        internal bool HasThis() {
            bool hasThis = false;
            for (int index = 0; index < _parameters.Count; index++) {
                if (!_parameters[index].InParameterArray) {
                    // Currently only one parameter can be out of parameter array
                    // TODO: Any number of parameters to be taken out of parameter array
                    Debug.Assert(hasThis == false);
                    Debug.Assert(index == 0);
                    hasThis = true;
                }
            }
            return hasThis;
        }
    }

    public static partial class Ast {
        public static CodeBlock CodeBlock(string name) {
            return CodeBlock(SourceSpan.None, name, typeof(object));
        }

        public static CodeBlock CodeBlock(string name, Type returnType) {
            return CodeBlock(SourceSpan.None, name, returnType);
        }

        public static CodeBlock CodeBlock(SourceSpan span, string name) {
            return CodeBlock(span, name, typeof(object));
        }

        public static CodeBlock CodeBlock(SymbolId name) {
            return CodeBlock(SourceSpan.None, SymbolTable.IdToString(name), typeof(object));
        }

        public static CodeBlock CodeBlock(SymbolId name, Type returnType) {
            return CodeBlock(SourceSpan.None, SymbolTable.IdToString(name), returnType);
        }

        public static CodeBlock CodeBlock(SourceSpan span, SymbolId name) {
            return CodeBlock(span, SymbolTable.IdToString(name), typeof(object));
        }

        public static CodeBlock CodeBlock(SourceSpan span, string name, Type returnType) {
            Contract.RequiresNotNull(name, "name");
            Contract.RequiresNotNull(returnType, "returnType");
            return new CodeBlock(span, name, returnType);
        }

        public static CodeBlock EventHandlerBlock(string name, EventInfo eventInfo) {
            Contract.RequiresNotNull(name, "name");
            Contract.RequiresNotNull(eventInfo, "eventInfo");

            ParameterInfo returnInfo;
            ParameterInfo[] parameterInfos;

            ReflectionUtils.GetDelegateSignature(eventInfo.EventHandlerType, out parameterInfos, out returnInfo);

            CodeBlock result = Ast.CodeBlock(name, returnInfo.ParameterType);
            for (int i = 0; i < parameterInfos.Length; i++) {
                result.Parameters.Add(Variable.Parameter(result, SymbolTable.StringToId("$" + i), parameterInfos[i].ParameterType));
            }

            return result;
        }
    }
}
