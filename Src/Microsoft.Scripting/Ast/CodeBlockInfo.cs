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

using System.Collections.Generic;

namespace Microsoft.Scripting.Ast {
    class CodeBlockInfo {
        private CodeBlock _block;
        private readonly Dictionary<Variable, VariableReference> _references = new Dictionary<Variable, VariableReference>();

        internal CodeBlockInfo(CodeBlock block) {
            _block = block;
        }

        internal CodeBlock CodeBlock {
            get { return _block; }
        }

        internal void Reference(Variable variable) {
            if (!_references.ContainsKey(variable)) {
                _references[variable] = new VariableReference(variable);
            }
        }

        internal void PublishReferences() {
            _block.References = _references;
        }

        internal void AddGeneratorTemps(int count) {
            _block.AddGeneratorTemps(count);
        }
    }
}
