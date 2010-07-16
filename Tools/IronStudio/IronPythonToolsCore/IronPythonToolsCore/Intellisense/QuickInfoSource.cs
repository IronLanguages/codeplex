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
using System.Diagnostics;
using System.Text;
using Microsoft.IronPythonTools.Internal;
using Microsoft.PyAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.IronPythonTools.Intellisense {
    internal class QuickInfoSource : IQuickInfoSource {
        private readonly ITextBuffer _textBuffer;

        public QuickInfoSource(ITextBuffer textBuffer) {
            _textBuffer = textBuffer;
        }

        #region IQuickInfoSource Members

        public void AugmentQuickInfoSession(IQuickInfoSession session, System.Collections.Generic.IList<object> quickInfoContent, out ITrackingSpan applicableToSpan) {
            var start = Analysis.Stopwatch.ElapsedMilliseconds;
            var textBuffer = session.TextView.TextBuffer;

            var vars = Analysis.AnalyzeExpression(
                textBuffer.CurrentSnapshot,
                textBuffer,
                session.CreateTrackingSpan(textBuffer)
            );

            applicableToSpan = vars.Span;
            if (String.IsNullOrEmpty(vars.Expression)) {
                return;
            }

            bool first = true;
            var result = new StringBuilder();            
            int count = 0;
            List<VariableResult> listVars = new List<VariableResult>(vars.Variables);
            HashSet<string> descriptions = new HashSet<string>();
            bool multiline = false;
            foreach (var v in listVars) {
                string description = null;
                if (listVars.Count == 1) {
                    if (v.Description != null) {
                        description = v.Description;
                    }
                } else {
                    if (v.ShortDescription != null) {
                        description = v.ShortDescription;
                    }
                }

                if (descriptions.Add(description)) {
                    if (first) {
                        first = false;
                    } else {
                        if (result.Length == 0 || result[result.Length - 1] != '\n') {
                            result.Append(", ");
                        } else {
                            multiline = true;
                        }
                    }
                    result.Append(description);
                    count++;
                }
            }

            if (multiline) {
                result.Insert(0, vars.Expression + ": " + Environment.NewLine);
            } else {
                result.Insert(0, vars.Expression + ": ");
            }
            var end = Analysis.Stopwatch.ElapsedMilliseconds;
            if (/*Logging &&*/ (end - start) > CompletionList.TooMuchTime) {
                Trace.WriteLine(String.Format("{0} lookup time {1} for {2} members", this, end - start, count));
            }

            quickInfoContent.Add(result.ToString());
        }

        #endregion

        public void Dispose() {
        }

    }
}
