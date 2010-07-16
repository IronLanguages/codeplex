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
    class BuiltinModuleScope : InterpreterScope {

        public BuiltinModuleScope(BuiltinModule moduleInfo)
            : base(moduleInfo) {
        }

        public override VariableDef GetVariable(string name, AnalysisUnit unit) {
            ISet<Namespace> res;

            if (Module.VariableDict.TryGetValue(name, out res)) {
                // TODO: We should cache these definitions
                var def = new VariableDef();
                def.AddTypes(res, unit);
                return def;
            }
            return null;
        }

        public BuiltinModule Module { get { return Namespace as BuiltinModule; } }

        public override string Name {
            get { return Module.Name; }
        }
    }
}
