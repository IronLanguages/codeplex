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
using Microsoft.IronPythonTools.Project;
using Microsoft.IronStudio;
using Microsoft.PyAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;

namespace Microsoft.IronPythonTools {
    static class Extensions {
        internal static void GotoSource(this LocationInfo location) {
            IronPythonToolsPackage.NavigateTo(
                location.FilePath,
                Guid.Empty,
                location.Line - 1,
                location.Column - 1);
        }

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

        internal static IProjectEntry GetAnalysis(this ITextBuffer buffer) {
            IProjectEntry res;
            buffer.Properties.TryGetProperty<IProjectEntry>(typeof(IProjectEntry), out res);
            return res;
        }

        internal static EnvDTE.Project GetProject(this IVsHierarchy hierarchy) {
            object project;

            ErrorHandler.ThrowOnFailure(
                hierarchy.GetProperty(
                    VSConstants.VSITEMID_ROOT,
                    (int)__VSHPROPID.VSHPROPID_ExtObject,
                    out project
                )
            );

            return (project as EnvDTE.Project);
        }

        internal static PythonProjectNode GetPythonProject(this EnvDTE.Project project) {
            return project.GetCommonProject() as PythonProjectNode;
        }
    }
}
