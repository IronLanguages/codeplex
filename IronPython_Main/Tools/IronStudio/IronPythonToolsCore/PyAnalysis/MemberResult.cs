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
using System.Text;
using Microsoft.PyAnalysis.Values;

namespace Microsoft.PyAnalysis {
    public struct MemberResult {
        private readonly string _name;
        private string _completion;
        private readonly IEnumerable<Namespace> _vars;
        private readonly ObjectType? _type;

        internal MemberResult(string name, IEnumerable<Namespace> vars) {
            _name = _completion = name;
            _vars = vars;
            _type = null;
        }

        internal MemberResult(string name, string completion, IEnumerable<Namespace> vars) {
            _name = name;
            _vars = vars;
            _completion = completion;
            _type = null;
        }


        internal MemberResult(string name, ObjectType type) {
            _name = name;
            _type = type;
            _completion = _name;
            _vars = Empty;
        }

        private static Namespace[] Empty = new Namespace[0];

        public string Name {
            get { return _name; }
        }

        public string Completion {
            get { return _completion; }
        }

        public ObjectType MemberType {
            get {
                return _type ?? GetMemberType();
            }
        }

        private ObjectType GetMemberType() {
            ObjectType result = ObjectType.Unknown;
            foreach (var ns in _vars) {
                var nsType = ns.NamespaceType;
                if (result == ObjectType.Unknown) {
                    result = nsType;
                } else if (result != nsType) {
                    return ObjectType.Multiple;
                }
            }
            return result;
        }
#if FALSE
        public LocationInfo[] Definitions {
            get {
                var result = new List<LocationInfo>();
                foreach (var ns in _vars) {
                    if (ns.Location != null) {
                        result.Add(ns.Location);
                    }
                }
                return result.ToArray();
            }
        }
#endif
        internal IEnumerable<Namespace> Namespaces {
            get {
                return _vars;
            }
        }

        public string ToolTip {
            get {
                var doc = new StringBuilder();
                foreach (var ns in _vars) {
                    doc.Append(ns.Documentation);
                }
                return Utils.CleanDocumentation(doc.ToString());
            }
        }
    }
}
