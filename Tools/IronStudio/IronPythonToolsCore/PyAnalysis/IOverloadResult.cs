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

namespace Microsoft.PyAnalysis {
    public class ParameterResult {
        public string Name { get; private set; }
        public string Documentation { get; private set; }
        public string Type { get; private set; }
        public bool IsOptional { get; private set; }

        public ParameterResult(string name)
            : this(name, String.Empty, "object") {
        }
        public ParameterResult(string name, string doc)
            : this(name, doc, "object") {
        }
        public ParameterResult(string name, string doc, string type)
            : this(name, doc, type, false) {
        }
        public ParameterResult(string name, string doc, string type, bool isOptional) {
            Name = name;
            Documentation = Trim(doc);
            Type = type;
            IsOptional = isOptional;
        }

        private const int MaxDocLength = 1000;
        internal static string Trim(string doc) {
            if (doc != null && doc.Length > MaxDocLength) {
                return doc.Substring(0, MaxDocLength) + "...";
            }
            return doc;
        }
    }
    public interface IOverloadResult {
        string Name { get; }
        string Documentation { get; }
        ParameterResult[] Parameters { get; }
    }
}
