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
    public partial class CodeBlock {
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;
        private readonly Type _returnType;
        private readonly string _name;
        private CodeBlock _parent;
        private Expression _body;

        private readonly ReadOnlyCollection<Variable> _parameters;
        private readonly List<Variable> _variables;

        // TODO: Evaluate necessity...
        #region Flags

        private readonly bool _isGlobal;
        private readonly bool _visibleScope;

        // TODO: Make readonly
        private bool _emitLocalDictionary;
        private bool _parameterArray;

        #endregion

        internal CodeBlock(SourceSpan span, string name, Type returnType, ReadOnlyCollection<Variable> parameters, List<Variable> variables, bool global, bool visible) {
            Assert.NotNull(returnType);
            _start = span.Start;
            _end = span.End;

            _name = name;
            _returnType = returnType;

            _parameters = parameters;
            _variables = variables;

            _isGlobal = global;
            _visibleScope = visible;
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
            set {
                _emitLocalDictionary = value;
            }
        }

        public bool IsGlobal {
            get { return _isGlobal; }
        }

        internal bool ParameterArray {
            get { return _parameterArray; }
            set { _parameterArray = value; }
        }

        public CodeBlock Parent {
            get { return _parent; }
            set { _parent = value; }
        }

        internal bool IsVisible {
            get { return _visibleScope; }
        }

        public Expression Body {
            get { return _body; }
            set { _body = value; }
        }

        public List<Variable> Variables {
            get { return _variables; }
        }

        internal Variable CreateTemporaryVariable(SymbolId name, Type type) {
            Variable variable = Variable.Temporary(name, type);
            variable.Block = this;
            _variables.Add(variable);
            return variable;
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
        public static CodeBlock GlobalCodeBlock(string name, Variable[] parameters, Variable[] variables) {
            return CodeBlock(SourceSpan.None, name, typeof(object), parameters, variables, true, true);
        }

        public static CodeBlock CodeBlock(string name, Type returnType, Variable[] parameters, Variable[] variables) {
            return CodeBlock(SourceSpan.None, name, returnType, parameters, variables);
        }

        public static CodeBlock CodeBlock(SourceSpan span, string name, Type returnType, Variable[] parameters, Variable[] variables) {
            return CodeBlock(span, name, returnType, parameters, variables, false, true);
        }

        public static CodeBlock CodeBlock(SourceSpan span, string name, Type returnType, Variable[] parameters, Variable[] variables, bool global, bool visible) {
            Contract.RequiresNotNull(name, "name");
            Contract.RequiresNotNull(returnType, "returnType");
            Contract.RequiresNotNullItems(parameters, "parameters");
            Contract.RequiresNotNullItems(variables, "variables");

            CodeBlock block = new CodeBlock(span, name, returnType, CollectionUtils.ToReadOnlyCollection(parameters), new List<Variable>(variables), global, visible);

            // TODO: Remove when variable no longer has block.
            SetBlock(parameters, block);
            SetBlock(variables, block);

            return block;
        }

        // TODO: Remove when variable no longer has block.
        private static void SetBlock(Variable[] variables, CodeBlock block) {
            for (int i = 0; i < variables.Length; i++) {
                Contract.Requires(variables[i].Block == null, "variables");
                variables[i].Block = block;
            }
        }
    }
}
