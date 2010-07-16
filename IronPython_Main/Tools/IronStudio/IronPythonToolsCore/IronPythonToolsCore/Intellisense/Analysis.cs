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
using Microsoft.IronPythonTools.Internal;
using Microsoft.IronStudio.Repl;
using Microsoft.PyAnalysis;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.IronPythonTools.Intellisense {
    public static class Analysis {
        private static Stopwatch _stopwatch = MakeStopWatch();

        /// <summary>
        /// Gets a ExpressionAnalysis for the expression at the provided span.  If the span is in
        /// part of an identifier then the expression is extended to complete the identifier.
        /// </summary>
        public static ExpressionAnalysis AnalyzeExpression(ITextSnapshot snapshot, ITextBuffer buffer, ITrackingSpan span) {
            ReverseExpressionParser parser = new ReverseExpressionParser(snapshot, buffer, span);

            var loc = parser.Span.GetSpan(parser.Snapshot.Version);
            var exprRange = parser.GetExpressionRange();
            if (exprRange == null) {
                return new ExpressionAnalysis("", new VariableResult[0], null);
            }

            // extend right for any partial expression the user is hovering on, for example:
            // "x.Baz" where the user is hovering over the B in baz we want the complete
            // expression.
            var text = exprRange.Value.GetText();
            var endingLine = exprRange.Value.End.GetContainingLine();
            if (endingLine.End.Position - exprRange.Value.End.Position < 0) {
                return new ExpressionAnalysis("", new VariableResult[0], null);
            }
            var endText = snapshot.GetText(exprRange.Value.End.Position, endingLine.End.Position - exprRange.Value.End.Position);
            bool allChars = true;
            for (int i = 0; i < endText.Length; i++) {
                if (!Char.IsLetterOrDigit(endText[i]) && endText[i] != '_') {
                    text += endText.Substring(0, i);
                    allChars = false;
                    break;
                }
            }
            if (allChars) {
                text += endText;
            }

            var applicableSpan = parser.Snapshot.CreateTrackingSpan(
                exprRange.Value.Span,
                SpanTrackingMode.EdgeExclusive
            );

            IProjectEntry analysisItem;
            if (buffer.TryGetAnalysis(out analysisItem)) {
                var analysis = ((IPythonProjectEntry)analysisItem).CurrentAnalysis;
                if (analysis != null && text.Length > 0) {

                    var lineNo = parser.Snapshot.GetLineNumberFromPosition(loc.Start);
                    return new ExpressionAnalysis(
                        text, 
                        analysis.GetVariablesFromExpression(text, lineNo + 1), 
                        applicableSpan);
                }
            }

            return new ExpressionAnalysis(text, new VariableResult[0], applicableSpan);
        }

        /// <summary>
        /// Gets a CompletionContext providing a list of possible members the user can dot through.
        /// </summary>
        public static CompletionList GetMembers(ITextSnapshot snapshot, ITextBuffer buffer, ITrackingSpan span, bool intersectMembers = true, bool hideAdvancedMembers = false) {
            ReverseExpressionParser parser = new ReverseExpressionParser(snapshot, buffer, span);

            var loc = parser.Span.GetSpan(parser.Snapshot.Version);
            var line = parser.Snapshot.GetLineFromPosition(loc.Start);
            var lineStart = line.Start;

            var textLen = loc.End - lineStart.Position;
            if (textLen <= 0) {
                // Ctrl-Space on an empty line, we just want to get global vars
                return new NormalCompletionList(String.Empty, loc.Start, parser.Snapshot, parser.Span, parser.Buffer, 0);
            }

            return TrySpecialCompletions(parser, loc) ??
                   GetNormalCompletionContext(parser, loc, intersectMembers, hideAdvancedMembers);
        }

        /// <summary>
        /// Gets a list of signatuers available for the expression at the provided location in the snapshot.
        /// </summary>
        public static SignatureAnalysis GetSignatures(ITextSnapshot snapshot, ITextBuffer buffer, ITrackingSpan span) {
            ReverseExpressionParser parser = new ReverseExpressionParser(snapshot, buffer, span);

            var loc = parser.Span.GetSpan(parser.Snapshot.Version);

            int paramIndex;
            SnapshotPoint? sigStart;
            var exprRange = parser.GetExpressionRange(1, out paramIndex, out sigStart);
            if (exprRange == null || sigStart == null) {
                return new SignatureAnalysis("", 0, new ISignature[0]);
            }

            Debug.Assert(sigStart != null);
            var text = new SnapshotSpan(exprRange.Value.Snapshot, new Span(exprRange.Value.Start, sigStart.Value.Position - exprRange.Value.Start)).GetText();
            //var text = exprRange.Value.GetText();
            var applicableSpan = parser.Snapshot.CreateTrackingSpan(exprRange.Value.Span, SpanTrackingMode.EdgeInclusive);

            var liveSigs = TryGetLiveSignatures(snapshot, paramIndex, text, applicableSpan);
            if (liveSigs != null) {
                return liveSigs;
            }

            var start = Stopwatch.ElapsedMilliseconds;

            var analysisItem = buffer.GetAnalysis();
            if (analysisItem != null) {
                var analysis = ((IPythonProjectEntry)analysisItem).CurrentAnalysis;
                if (analysis != null) {

                    var lineNo = parser.Snapshot.GetLineNumberFromPosition(loc.Start);

                    IOverloadResult[] sigs = analysis.GetSignaturesFromExpression(text, lineNo + 1);
                    var end = Stopwatch.ElapsedMilliseconds;

                    if (/*Logging &&*/ (end - start) > CompletionList.TooMuchTime) {
                        Trace.WriteLine(String.Format("{0} lookup time {1} for {2} signatures", text, end - start, sigs.Length));
                    }

                    var result = new ISignature[sigs.Length];
                    for (int i = 0; i < sigs.Length; i++) {
                        result[i] = new PythonSignature(applicableSpan, sigs[i], paramIndex);
                    }

                    return new SignatureAnalysis(
                        text,
                        paramIndex,
                        result
                    );
                }
            }
            return new SignatureAnalysis(text, paramIndex, new ISignature[0]);
        }

        internal static Stopwatch Stopwatch {
            get {
                return _stopwatch;
            }
        }

        #region Implementation Details

        private static SignatureAnalysis TryGetLiveSignatures(ITextSnapshot snapshot, int paramIndex, string text, ITrackingSpan applicableSpan) {
            IReplEvaluator eval;
            IDlrEvaluator dlrEval;
            if (snapshot.TextBuffer.Properties.TryGetProperty<IReplEvaluator>(typeof(IReplEvaluator), out eval) &&
                (dlrEval = eval as IDlrEvaluator) != null) {
                if (text.EndsWith("(")) {
                    text = text.Substring(0, text.Length - 1);
                }
                var liveSigs = dlrEval.GetSignatureDocumentation(text);

                if (liveSigs != null && liveSigs.Count > 0) {
                    return new SignatureAnalysis(text, paramIndex, GetLiveSignatures(text, liveSigs, paramIndex, applicableSpan));
                }
            }
            return null;
        }

        private static ISignature[] GetLiveSignatures(string text, ICollection<OverloadDoc> liveSigs, int paramIndex, ITrackingSpan span) {
            ISignature[] res = new ISignature[liveSigs.Count];
            int i = 0;
            foreach (var sig in liveSigs) {
                var parameters = new ParameterResult[sig.Parameters.Count];
                int j = 0;
                foreach (var param in sig.Parameters) {
                    parameters[j++] = new ParameterResult(param.Name);
                }

                res[i++] = new PythonSignature(
                    span,
                    new LiveOverloadResult(text, sig.Documentation, parameters),
                    paramIndex
                );
            }
            return res;
        }

        class LiveOverloadResult : IOverloadResult {
            private readonly string _name, _doc;
            private readonly ParameterResult[] _parameters;

            public LiveOverloadResult(string name, string documentation, ParameterResult[] parameters) {
                _name = name;
                _doc = documentation;
                _parameters = parameters;
            }

            #region IOverloadResult Members

            public string Name {
                get { return _name; }
            }

            public string Documentation {
                get { return _doc; }
            }

            public ParameterResult[] Parameters {
                get { return _parameters; }
            }

            #endregion
        }

        private static CompletionList TrySpecialCompletions(ReverseExpressionParser parser, Span loc) {
            if (parser.Tokens.Count > 0) {
                // Check for context-sensitive intellisense
                var lastClass = parser.Tokens[parser.Tokens.Count - 1];

                if (lastClass.ClassificationType == parser.Classifier.Provider.Comment) {
                    // No completions in comments
                    return CompletionList.EmptyCompletionContext;
                } else if (lastClass.ClassificationType == parser.Classifier.Provider.StringLiteral) {
                    // String completion
                    return new StringLiteralCompletionList(lastClass.Span.GetText(), loc.Start, parser.Span, parser.Buffer);
                }

                // Import completions
                var first = parser.Tokens[0];
                if (CompletionList.IsKeyword(first, "import")) {
                    return ImportCompletionList.Make(first, lastClass, loc, parser.Snapshot, parser.Span, parser.Buffer, IsSpaceCompletion(parser, loc));
                } else if (CompletionList.IsKeyword(first, "from")) {
                    return FromImportCompletionList.Make(parser.Tokens, first, loc, parser.Snapshot, parser.Span, parser.Buffer, IsSpaceCompletion(parser, loc));
                }
                return null;
            }

            return CompletionList.EmptyCompletionContext;
        }

        private static CompletionList GetNormalCompletionContext(ReverseExpressionParser parser, Span loc, bool intersectMembers = true, bool hideAdvancedMembers = false) {
            var exprRange = parser.GetExpressionRange();
            if (exprRange == null) {
                return CompletionList.EmptyCompletionContext;
            }
            var text = exprRange.Value.GetText();

            var applicableSpan = parser.Snapshot.CreateTrackingSpan(
                exprRange.Value.Span,
                SpanTrackingMode.EdgeExclusive
            );

            return new NormalCompletionList(
                text,
                loc.Start,
                //parser.Snapshot.GetLineNumberFromPosition(loc.Start),
                parser.Snapshot,
                applicableSpan,
                parser.Buffer,
                -1,
                intersectMembers,
                hideAdvancedMembers
            );
        }

        private static bool IsSpaceCompletion(ReverseExpressionParser parser, Span loc) {
            var keySpan = new SnapshotSpan(parser.Snapshot, loc.Start - 1, 1);
            return (keySpan.GetText() == " ");
        }

        private static Stopwatch MakeStopWatch() {
            var res = new Stopwatch();
            res.Start();
            return res;
        }

        #endregion
    }
}
