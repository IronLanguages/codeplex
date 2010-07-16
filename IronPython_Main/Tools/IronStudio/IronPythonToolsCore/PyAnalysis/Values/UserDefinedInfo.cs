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
using IronPython.Compiler.Ast;
using Microsoft.PyAnalysis.Interpreter;

namespace Microsoft.PyAnalysis.Values {
    internal class UserDefinedInfo : Namespace {
        internal readonly AnalysisUnit _analysisUnit;
        private VariableDef _returnValue;
        private VariableDef[] _parameters;

        protected UserDefinedInfo(AnalysisUnit analysisUnit) {
            _analysisUnit = analysisUnit;
            _returnValue = new VariableDef();
        }

        public override VariableDef[] ParameterTypes {
            get { return _parameters; }
        }

        public VariableDef ReturnValue {
            get {
                return _returnValue;
            }
            set {
                _returnValue = value;
            }
        }

        public void SetParameters(VariableDef[] parameters) {
            _parameters = parameters;
        }
    }
}
