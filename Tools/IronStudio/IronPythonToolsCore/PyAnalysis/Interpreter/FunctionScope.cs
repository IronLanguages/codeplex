/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;
using Microsoft.PyAnalysis.Values;

namespace Microsoft.PyAnalysis.Interpreter {
    class FunctionScope : InterpreterScope {
        private IDictionary<string, VariableDef> _variables;
        private static Dictionary<string, VariableDef> EmptyVaribles = new Dictionary<string, VariableDef>();

        public FunctionScope(FunctionInfo functionInfo)
            : base(functionInfo) {
        }

        public FunctionInfo Function {
            get {
                return Namespace as FunctionInfo;
            }
        }

        public override string Name {
            get { return Function.FunctionDefinition.Name;  }
        }

        public override VariableDef GetVariable(string name, AnalysisUnit unit) {
            VariableDef res;
            if (_variables != null && _variables.TryGetValue(name, out res)) {
                return res;
            }

            return null;
        }
        
        public VariableDef DefineVariable(string name) {
            if (_variables == null) {
                _variables = new Dictionary<string, VariableDef>();
            }
            return _variables[name] = new VariableDef();
        }

        public override void SetVariable(string name, IEnumerable<Namespace> value, AnalysisUnit unit) {            
            if (_variables == null) {
                _variables = new Dictionary<string, VariableDef>();
            }
            VariableDef def;
            if (!_variables.TryGetValue(name, out def)) {
                _variables[name] = def = new VariableDef();
            }
            def.AddTypes(value, unit);
        }

        public override IDictionary<string, VariableDef> Variables {
            get {
                return _variables ?? EmptyVaribles;
            }
        }
    }
}
