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
using IronPython.Compiler.Ast;
using IronPython.Runtime.Types;
using Microsoft.PyAnalysis.Interpreter;

namespace Microsoft.PyAnalysis.Values {
    internal class RangeInfo : BuiltinInstanceInfo {
        public RangeInfo(PythonType seqType, ProjectState state)
            : base(state._listType) {
        }

        public override ISet<Namespace> GetEnumeratorTypes(Node node, AnalysisUnit unit) {
            return ProjectState._intType.SelfSet;
        }

        public override ISet<Namespace> GetIndex(Node node, AnalysisUnit unit, ISet<Namespace> index) {
            // TODO: Return correct index value if we have a constant
            /*int? constIndex = SequenceInfo.GetConstantIndex(index);

            if (constIndex != null && constIndex.Value < _indexTypes.Count) {
                // TODO: Warn if outside known index and no appends?
                return _indexTypes[constIndex.Value];
            }*/

            return ProjectState._intType.SelfSet;
        }
    }
}
