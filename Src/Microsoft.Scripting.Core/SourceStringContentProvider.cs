/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System; using Microsoft;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {

    [Serializable]
    public class SourceStringContentProvider : TextContentProvider {
        private readonly string _code;

        public SourceStringContentProvider(string code) {
            ContractUtils.RequiresNotNull(code, "code");

            _code = code;
        }

        public override TextReader GetReader() {
            return new StringReader(_code);
        }
    }
}
