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

using Microsoft.PyAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;


namespace Microsoft.IronPythonTools.Intellisense {
    internal class PythonParameter : IParameter {
        private readonly ISignature _signature;
        private readonly ParameterResult _param;
        private readonly Span _locus;

        public PythonParameter(ISignature signature, ParameterResult param, Span locus) {
            _signature = signature;
            _param = param;
            _locus = locus;
        }

        public string Documentation {
            get { return _param.Documentation; }
        }

        public Span Locus {
            get { return _locus; }
        }

        public string Name {
            get { return _param.Name; }
        }

        public ISignature Signature {
            get { return _signature; }
        }

        public Span PrettyPrintedLocus {
            get { return Locus; }
        }
    }
}
