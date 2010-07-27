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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronPythonTools.Intellisense {
    [Export(typeof(ISignatureHelpSourceProvider)), ContentType(PythonCoreConstants.ContentType), Order, Name("Python Signature Help Source")]
    class SignatureHelpSourceProvider : ISignatureHelpSourceProvider {
        [Import(typeof(IPythonAnalyzer))]
        internal IPythonAnalyzer _Analysis = null;

        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer) {
            return new SignatureHelpSource(this, textBuffer);
        }
    }
}
