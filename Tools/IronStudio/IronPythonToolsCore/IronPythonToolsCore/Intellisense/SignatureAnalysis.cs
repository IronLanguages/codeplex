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
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.IronPythonTools.Intellisense {
    public class SignatureAnalysis {
        private readonly string _text;
        private readonly int _paramIndex;
        private readonly ISignature[] _signatures;

        public SignatureAnalysis(string text, int paramIndex, IList<ISignature> signatures) {
            _text = text;
            _paramIndex = paramIndex;
            _signatures = new ISignature[signatures.Count];
            signatures.CopyTo(_signatures, 0);
            Array.Sort(_signatures, (x, y) => x.Parameters.Count - y.Parameters.Count);
        }

        public string Text {
            get {
                return _text;
            }
        }

        public int ParameterIndex {
            get {
                return _paramIndex;
            }
        }

        public IList<ISignature> Signatures {
            get {
                return _signatures;
            }
        }
    }
}
