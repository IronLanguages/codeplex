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
        private IMergeDestination destination;
        private int startLineCount, lineDelta;
#if DEBUG
        private int lastLine;
#endif

        public static void CacheCode(CodeCompileUnit ccu, string code) {
            ccu.UserData["MergerCache"] = new CodeMerger(new SimpleMergeDestination(code));
        }

        public static void CacheCode(CodeCompileUnit ccu, IMergeDestination mergeDestination) {
            ccu.UserData["MergerCache"] = new CodeMerger(mergeDestination);
        }

        public static CodeMerger GetCachedCode(CodeCompileUnit ccu) {
            return ccu.UserData["MergerCache"] as CodeMerger;
        }

        private CodeMerger(IMergeDestination destination) {
            this.destination = destination;
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
            Debug.Assert(startRow >= lastLine, String.Format("Start line is less than or equal to last line start: {0} last: {1}", startRow, lastLine), text);
            lastLine = startRow;
#endif

            bool hasMerged = destination.HasMerged;
            int destLineCount = destination.LineCount;
            List<string> newLines = GetLines(text);
            if (!hasMerged) {
                startLineCount = destLineCount;
            }

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
            Debug.Assert(startRow + lineDelta < startLineCount, "row + lineDelta is > # of lines");

            // we expect merges to come in-order (because we are walking
            // over the same CodeDOM tree in the same order).  Therefore
            // we progressively update a change-in-line offsets from the
            // original file as we append new data / remove existing data
            // from our CodeDOM tree.  This data will be reset when we finalize
            // the merge and return the resulting text.

            destination.RemoveRange(startRow + lineDelta, lineRange);
            destination.InsertRange(startRow + lineDelta, newLines);

            int newLineDelta = newLines.Count - lineRange;
            lineDelta += newLineDelta;
        }

        public int LineDelta {
            get {
                return lineDelta;
            }
            set {
                lineDelta = value;
            }
        }

        public string FinalizeMerge() {
            string res = destination.FinalText;

            // and now we expect any new merges to be based on our
            // current offsets.
            startLineCount = startLineCount + lineDelta;
            lineDelta = 0;
#if DEBUG
            lastLine = 0;
#endif
            return res;
        }

        private int GetNewLine(int oldLine) {
            return oldLine + lineDelta;
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
        
        /// <summary>
        /// A simple merger that uses strings as it's backing store.
        /// </summary>
        class SimpleMergeDestination : IMergeDestination {
            private string originalText;
            private List<string> lines;

            public SimpleMergeDestination(string code) {
                originalText = code;
            }

            #region IMergeDestination Members

            public void InsertRange(int start, IList<string> newLines) {
                TextToLines();
                lines.InsertRange(start, newLines);
            }

            public void RemoveRange(int start, int count) {
                TextToLines();
                lines.RemoveRange(start, count);
            }

            public int LineCount {
                get {
                    TextToLines();
                    return lines.Count;
                }
            }            

            /// <summary>
            /// True if any updates have been applied to the original text, false other wise.
            /// </summary>
            public bool HasMerged {
                get { return lines != null; }
            }

            /// <summary>
            /// Returns the original text in the buffer.  May return null if any merges have been preformed.
            /// </summary>
            public string FinalText {
                get {
                    if (lines == null) return originalText;

                    StringBuilder res = new StringBuilder();
                    for (int i = 0; i < lines.Count; i++) {
                        res.AppendLine(lines[i]);
                    }
                    return res.ToString();
                }
            }

            #endregion

            /// <summary>
            /// Converts our original text into a line based format
            /// that can be more quickly updated.
            /// </summary>
            private void TextToLines() {
                if (lines == null) {
                    lines = GetLines(originalText);
                    // no longer needed.
                    originalText = null;
                }
            }

        }

    }

}
