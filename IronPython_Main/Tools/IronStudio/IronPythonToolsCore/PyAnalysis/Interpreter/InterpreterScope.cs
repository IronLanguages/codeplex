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

using System;
using System.Collections.Generic;
using Microsoft.PyAnalysis.Values;

namespace Microsoft.PyAnalysis.Interpreter {
    abstract class InterpreterScope {
        private readonly Namespace _ns;
        private Dictionary<string, VariableDef> _variables = new Dictionary<string, VariableDef>();

        public InterpreterScope(Namespace ns) {
            _ns = ns;
        }

        public abstract string Name {
            get;
        }

        public virtual void SetVariable(string name, IEnumerable<Namespace> value, AnalysisUnit unit) {
            CreateVariable(name, unit).AddTypes(value, unit);
        }

        public virtual VariableDef GetVariable(string name, AnalysisUnit unit) {
            VariableDef res;
            if (_variables.TryGetValue(name, out res)) {
                return res;
            }
            return null;
        }

        public virtual VariableDef CreateVariable(string name, AnalysisUnit unit) {
            var res = GetVariable(name, unit);
            if (res == null) {
                _variables[name] = res = new VariableDef();
            }
            return res;
        }

        protected VariableDef CreateVariableWorker(string name) {
            VariableDef res;
            if (!_variables.TryGetValue(name, out res)) {
                _variables[name] = res = new VariableDef();
            }
            return res;
        }

        public virtual IDictionary<string, VariableDef> Variables {
            get {
                return _variables;
            }
        }

        public virtual bool VisibleToChildren {
            get {
                return true;
            }
        }

        public Namespace Namespace {
            get {
                return _ns;
            }
        }
    }
}
