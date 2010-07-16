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

namespace Microsoft.PyAnalysis {
    public class VariableResult {
        private readonly Namespace _namespace;

        internal VariableResult(Namespace ns) {
            _namespace = ns;
        }

        public string Description {
            get { return _namespace.Description; }
        }

        public string ShortDescription {
            get {
                return _namespace.ShortDescription;
            }
        }

        public LocationInfo Location {
            get { return _namespace.Location; }
        }

        public ObjectType Type {
            get {
                return _namespace.NamespaceType;
            }
        }

        public IEnumerable<LocationInfo> References {
            get { return _namespace.References; }
        }
    }
}
