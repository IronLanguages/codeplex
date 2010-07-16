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

using Microsoft.IronPythonTools.Intellisense;
using Microsoft.PyAnalysis;
using Microsoft.Scripting.Hosting;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.IronPythonTools.Internal {
    static class Extensions {
        internal static StandardGlyphGroup ToGlyphGroup(this ObjectType objectType) {
            StandardGlyphGroup group;
            switch (objectType) {
                case ObjectType.Class: group = StandardGlyphGroup.GlyphGroupClass; break;
                case ObjectType.Delegate: group = StandardGlyphGroup.GlyphGroupDelegate; break;
                case ObjectType.Enum: group = StandardGlyphGroup.GlyphGroupEnum; break;
                case ObjectType.Namespace: group = StandardGlyphGroup.GlyphGroupNamespace; break;
                case ObjectType.Multiple: group = StandardGlyphGroup.GlyphGroupOverload; break;
                case ObjectType.Field: group = StandardGlyphGroup.GlyphGroupField; break;
                case ObjectType.Module: group = StandardGlyphGroup.GlyphGroupModule; break;
                case ObjectType.Property: group = StandardGlyphGroup.GlyphGroupProperty; break;
                case ObjectType.Instance: group = StandardGlyphGroup.GlyphGroupVariable; break;
                case ObjectType.Constant: group = StandardGlyphGroup.GlyphGroupConstant; break;
                case ObjectType.EnumMember: group = StandardGlyphGroup.GlyphGroupEnumMember; break;
                case ObjectType.Event: group = StandardGlyphGroup.GlyphGroupEvent; break;
                case ObjectType.Function:
                case ObjectType.Method:
                default:
                    group = StandardGlyphGroup.GlyphGroupMethod;
                    break;
            }
            return group;
        }

        internal static bool TryGetAnalysis(this ITextBuffer buffer, out IProjectEntry analysis) {
            return buffer.Properties.TryGetProperty<IProjectEntry>(typeof(IProjectEntry), out analysis);
        }

        internal static bool TryGetPythonAnalysis(this ITextBuffer buffer, out IPythonProjectEntry analysis) {
            IProjectEntry entry;
            if (buffer.TryGetAnalysis(out entry) && (analysis = entry as IPythonProjectEntry) != null) {
                return true;
            }
            analysis = null;
            return false;
        }

        internal static IProjectEntry GetAnalysis(this ITextBuffer buffer) {
            IProjectEntry res;
            buffer.TryGetAnalysis(out res);
            return res;
        }

        internal static IPythonProjectEntry GetPythonAnalysis(this ITextBuffer buffer) {
            IPythonProjectEntry res;
            buffer.TryGetPythonAnalysis(out res);
            return res;
        }

        internal static string GetFilePath(this ITextView textView) {
            return textView.TextBuffer.GetFilePath();
        }

        internal static string GetFilePath(this ITextBuffer textBuffer) {
            ITextDocument textDocument;
            if (textBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDocument)) {
                return textDocument.FilePath;
            } else {
                return null;
            }
        }

        internal static ITrackingSpan CreateTrackingSpan(this IIntellisenseSession session, ITextBuffer buffer) {
            var triggerPoint = session.GetTriggerPoint(buffer);
            var position = session.GetTriggerPoint(buffer).GetPosition(session.TextView.TextSnapshot);

            var snapshot = buffer.CurrentSnapshot;
            if (position == snapshot.Length) {
                return snapshot.CreateTrackingSpan(position, 0, SpanTrackingMode.EdgeInclusive);
            } else {
                return snapshot.CreateTrackingSpan(position, 1, SpanTrackingMode.EdgeInclusive);
            }
        }

        internal static ITrackingSpan CreateTrackingSpan0(this IIntellisenseSession session, ITextBuffer buffer) {
            var triggerPoint = session.GetTriggerPoint(buffer);
            var position = session.GetTriggerPoint(buffer).GetPosition(session.TextView.TextSnapshot);

            var snapshot = buffer.CurrentSnapshot;
            return snapshot.CreateTrackingSpan(position, 0, SpanTrackingMode.EdgeInclusive);
        }
    }
}
