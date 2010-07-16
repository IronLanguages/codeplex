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
using Microsoft.PyAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.IronPythonTools.Intellisense {
    public class ExpressionAnalysis {
        private readonly string _expr;
        private readonly IEnumerable<VariableResult> _vars;
        private readonly ITrackingSpan _span;
        
        public ExpressionAnalysis(string expression, IEnumerable<VariableResult> variables, ITrackingSpan span) {
            _expr = expression;
            _vars = variables;
            _span = span;
        }

        public string Expression {
            get {
                return _expr;
            }
        }

        public IEnumerable<VariableResult> Variables {
            get {
                return _vars;
            }
        }

        public ITrackingSpan Span {
            get {
                return _span;
            }
        }
    }
}
