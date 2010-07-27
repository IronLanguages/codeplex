using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.PyAnalysis.Values {
    interface IVariableDefContainer {
        IEnumerable<VariableDef> GetDefinitions(string name);
    }
}
