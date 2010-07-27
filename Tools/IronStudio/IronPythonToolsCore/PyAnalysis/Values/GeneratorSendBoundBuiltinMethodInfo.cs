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
    class GeneratorSendBoundBuiltinMethodInfo : BoundBuiltinMethodInfo {
        private readonly GeneratorInfo _generator;

        public GeneratorSendBoundBuiltinMethodInfo(GeneratorInfo generator, BuiltinMethodInfo method)
            : base(method) {
            _generator = generator;
        }

        public override ISet<Namespace> Call(Node node, AnalysisUnit unit, ISet<Namespace>[] args, string[] keywordArgNames) {
            if (args.Length == 1) {
                _generator.AddSend(node, unit, args[0]);
            }

            return _generator.Yields;
        }
    }
}
