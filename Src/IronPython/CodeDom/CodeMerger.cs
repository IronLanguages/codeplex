/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using System.CodeDom;

namespace IronPython.CodeDom {
    /// <summary>
    /// CodeMeger provides merging services between a cached copy of the code
    /// and the pre-existing copy of the code.  The parser calls the merger to
    /// cache the existing text.  When the generator is called the generator
    /// will check if a merger exists, and if so, will use the generator for
    /// performing updates against the original text.  Finally the generator
    /// will write the updated text as a string to the output stream.
    /// </summary>
    class CodeMerger {
        string originalText;
        List<string> lines;
        int startLineCount, lineDelta;
#if DEBUG
        int lastLine;
#endif

        public static void CacheCode(CodeCompileUnit ccu, string code) {
            ccu.UserData["MergerCache"] = new CodeMerger(code);
        }

        public static CodeMerger GetCachedCode(CodeCompileUnit ccu) {
            return ccu.UserData["MergerCache"] as CodeMerger;
        }

        private CodeMerger(string codeString) {
            originalText = codeString;
        }

        /// <summary>
        /// Peforms a merge of the code.  Line/Columns are always in terms of the original code and
        /// index from 1.
        /// 
        /// Currently column parameters are ignored.
        /// </summary> 
        public void DoMerge(int startRow, int startCol, int endRow, int endCol, string text) {
            // input's index from 1, interally we index from zero.
            startRow--; startCol--;
            if (endRow != -1) { // end of stream
                endRow--; endCol--;
            }

#if DEBUG
            Debug.Assert(startRow >= lastLine, String.Format("Start line is less than or equal to last line start: {0} last: {1}",startRow,lastLine), text);
            lastLine = startRow;
#endif

            TextToLines();

            List<string> newLines = GetLines(text);

            // last \r\n should be removed (eg 'foo\r\n' should just be 'foo', but we 
            // can't ask Split in GetLines to remove blanks because we actually \
            // do want those if they're in the middle)
            if (newLines[newLines.Count - 1].Length == 0) newLines.RemoveAt(newLines.Count - 1);

            int lineRange;
            if (endRow != -1) {
                lineRange = endRow - startRow;
            } else {
                // to the end of the file
                lineRange = startLineCount - startRow;
            }

            Debug.Assert(lineRange >= 0, "line range is negative");
            Debug.Assert(startRow + lineDelta >= 0, "row & line delta is negative");
            Debug.Assert(startRow + lineDelta < lines.Count, "row + lineDelta is > # of lines");

            // we expect merges to come in-order (because we are walking
            // over the same CodeDOM tree in the same order).  Therefore
            // we progressively update a change-in-line offsets from the
            // original file as we append new data / remove existing data
            // from our CodeDOM tree.  This data will be reset when we finalize
            // the merge and return the resulting text.

            lines.RemoveRange(startRow+lineDelta, lineRange);
            lines.InsertRange(startRow+lineDelta, newLines);

            int newLineDelta = newLines.Count-lineRange;
            lineDelta += newLineDelta;
        }

        public int GetNewLine(int oldLine) {
            return oldLine + lineDelta;
        }

        public int LineDelta {
            get {
                return lineDelta;
            }
        }
        public string FinalizeMerge() {
            if (lines == null) return originalText;
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < lines.Count; i++) {
                res.AppendLine(lines[i]);
            }

            // and now we expect any new merges to be based on our
            // current offsets.
            startLineCount = lines.Count;
            lineDelta = 0;
#if DEBUG
            lastLine = 0;
#endif
            return res.ToString();
        }

        /// <summary>
        /// Converts our original text into a line based format
        /// that can be more quickly updated.
        /// </summary>
        private void TextToLines() {
            if (lines == null) {
                lines = GetLines(originalText);
                startLineCount = lines.Count;
                // no longer needed.
                originalText = null;
            }
        }

        private static List<string> GetLines(string text) {
            List<string> res;
            if (text.IndexOf("\r\n") != -1) {
                res = new List<string>(text.Split(new string[] { "\r\n" }, StringSplitOptions.None));
            } else if (text.IndexOf('\r') != -1) {
                res = new List<string>(text.Split('\r'));
            } else {
                res = new List<string>(text.Split('\n'));
            }
            return res;
        }

    }
}
