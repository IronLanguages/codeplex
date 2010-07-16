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
using Microsoft.PyAnalysis.Interpreter;

namespace Microsoft.PyAnalysis.Values {
    /// <summary>
    /// contains information about dependencies.  Each DependencyInfo is 
    /// attached to a VariableRef in a dictionary keyed off of the ProjectEntry.
    /// 
    /// Module -> The Module this DependencyInfo object tracks.
    /// DependentUnits -> What needs to change if this VariableRef is updated.
    /// Types -> Types that this VariableRef has received from the Module.
    /// </summary>
    internal class DependencyInfo {
        public int Version { get; private set; }
        private TypeUnion _union;
        private HashSet<AnalysisUnit> _dependentUnits;

        public DependencyInfo() {
        }

        public DependencyInfo(int version) {
            Version = version;
            _dependentUnits = new HashSet<AnalysisUnit>();
            Types = new TypeUnion();
        }

        public TypeUnion Types {
            get {
                if (_union == null) {
                    _union = new TypeUnion();
                }
                return _union;
            }
            set {
                _union = value;
            }
        }

        public HashSet<AnalysisUnit> DependentUnits {
            get {
                if (_dependentUnits == null) {
                    _dependentUnits = new HashSet<AnalysisUnit>();
                }
                return _dependentUnits; 
            }
        }
    }
}
