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
using IronPython.Runtime.Types;
using Microsoft.PyAnalysis.Interpreter;

namespace Microsoft.PyAnalysis.Values {
    /// <summary>
    /// Provides a built-in function whose analysis we deeply understand.  Created with a custom delegate which
    /// allows custom behavior rather than the typical behavior of returning the return type of the function.
    /// 
    /// This is used for clr.AddReference* and calls to range() both of which we want to be customized in different
    /// ways.
    /// </summary>
    class SpecializedBuiltinFunction : BuiltinFunctionInfo {
        private readonly Func<CallExpression, AnalysisUnit, ISet<Namespace>[], ISet<Namespace>> _call;

        public SpecializedBuiltinFunction(ProjectState state, BuiltinFunction function, Func<CallExpression, AnalysisUnit, ISet<Namespace>[], ISet<Namespace>> call)
            : base(function, state) {
            _call = call;
        }

        public override ISet<Namespace> Call(Node node, AnalysisUnit unit, ISet<Namespace>[] args, string[] keywordArgNames) {
            return _call((CallExpression)node, unit, args) ?? base.Call(node, unit, args, keywordArgNames);
        }
    }
}
