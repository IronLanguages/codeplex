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

        public override VariableDef GetVariable(string name, AnalysisUnit unit) {
            Module.ModuleDefinition.AddDependency(unit);
            //Module.ModuleDependencies.AddDependency(Module, unit);

            return base.GetVariable(name, unit);
        }

        public override void SetVariable(string name, System.Collections.Generic.IEnumerable<Namespace> value, AnalysisUnit unit) {
            if (CreateVariableWorker(name).AddTypes(value, unit)) {
                Module.ModuleDefinition.EnqueueDependents();
                //Module.ModuleDependencies.EnqueueDependents(Module);
            }
        }

        public ModuleInfo Module { get { return Namespace as ModuleInfo; } }

        public override string Name {
            get { return Module.Name; }
        }
    }
}
