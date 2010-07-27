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

using Microsoft.PyAnalysis.Values;

namespace Microsoft.PyAnalysis.Interpreter {
    class ModuleScope : InterpreterScope {

        public ModuleScope(ModuleInfo moduleInfo)
            : base(moduleInfo) {
        }

        public ModuleInfo Module { get { return Namespace as ModuleInfo; } }

        public override string Name {
            get { return Module.Name; }
        }
    }
}
