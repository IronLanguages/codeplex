/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Scripting.Hosting;
using System.Diagnostics;

namespace Microsoft.Scripting {

    /// <summary>
    /// Couples a source unit with an open text reader. Remotable (TextReader is a MBRO).
    /// </summary>
    [Serializable]
    public sealed class SourceUnitReader : TextReader {

        private readonly TextReader _textReader;
        private readonly SourceUnit _sourceUnit;
        private readonly Encoding _encoding;

        public SourceUnit SourceUnit {
            get { return _sourceUnit; }
        }

        public Encoding Encoding {
            get { return _encoding; }
        }

        internal SourceUnitReader(SourceUnit sourceUnit, TextReader textReader, Encoding encoding) {
            Debug.Assert(sourceUnit != null && textReader != null && encoding != null);
            _textReader = textReader;
            _sourceUnit = sourceUnit;
            _encoding = encoding;
        }

        public override string ReadLine() {
            if (_sourceUnit.DisableLineFeedLineSeparator) {
                return Utils.ReadTo(_textReader, '\n');
            } else {
                return _textReader.ReadLine();
            }
        }

        public bool SeekLine(int line) {
            if (_sourceUnit.DisableLineFeedLineSeparator) {
                int current_line = 1;

                for (; ; ) {
                    if (!Utils.SeekTo(_textReader, '\n')) return false;
                    current_line++;
                    if (current_line == line) return true;
                }
            } else {
                return Utils.SeekLine(_textReader, line);
            }
        }

        public override string ReadToEnd() {
            return _textReader.ReadToEnd();
        }

        public override int Read(char[] buffer, int index, int count) {
            return _textReader.Read(buffer, index, count);
        }

        public override int Peek() {
            return _textReader.Peek();
        }

        public override int Read() {
            return _textReader.Read();
        }

        protected override void Dispose(bool disposing) {
            _textReader.Dispose();
        }
    }
}
