/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.IO;
using System.Text;

namespace System.Scripting.Runtime {
    /// <summary>
    /// Internal class which binds a LanguageContext, StreamContentProvider, and Encoding together to produce
    /// a TextContentProvider which reads binary data with the correct language semantics.
    /// 
    /// TODO: currently duplicated in Microsoft.Scripting, remove this when LanguageContext moves up
    /// </summary>
    internal sealed class LanguageBoundTextContentProvider : TextContentProvider {
        private readonly LanguageContext _context;
        private readonly FileStreamContentProvider _stream;
        private readonly Encoding _encoding;

        public LanguageBoundTextContentProvider(LanguageContext context, FileStreamContentProvider stream, Encoding encoding) {
            _context = context;
            _stream = stream;
            _encoding = encoding;
        }

        public override TextReader GetReader() {
            return _context.GetSourceReader(_stream.GetStream(), _encoding);
        }
    }
}
