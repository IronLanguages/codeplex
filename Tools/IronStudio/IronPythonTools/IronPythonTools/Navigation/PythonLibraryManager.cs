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
using System.Runtime.InteropServices;
using Microsoft.IronStudio.Navigation;
using Microsoft.PyAnalysis;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.IronPythonTools.Navigation {

    /// <summary>
    /// This interface defines the service that finds IronPython files inside a hierarchy
    /// and builds the informations to expose to the class view or object browser.
    /// </summary>
    [Guid(PythonConstants.LibraryManagerServiceGuid)]
    public interface IPythonLibraryManager : ILibraryManager {        
    }

    /// <summary>
    /// Implementation of the service that builds the information to expose to the symbols
    /// navigation tools (class view or object browser) from the Python files inside a
    /// hierarchy.
    /// </summary>
    [Guid(PythonConstants.LibraryManagerGuid)]
    internal class PythonLibraryManager : LibraryManager, IPythonLibraryManager {
        private readonly IronPythonToolsPackage/*!*/ _package;

        public PythonLibraryManager(IronPythonToolsPackage/*!*/ package)
            : base(package) {
            _package = package;
        }

        protected override LibraryNode CreateLibraryNode(IScopeNode subItem, string namePrefix, IVsHierarchy hierarchy, uint itemid) {
            return new PythonLibraryNode(subItem, namePrefix, hierarchy, itemid);            
        }

        protected override void OnNewFile(LibraryTask task) {
            IProjectEntry item;
            if (task.TextBuffer != null) {
                item = task.TextBuffer.GetAnalysis();
            } else {
                item = IronPythonToolsPackage.Instance.Analyzer.AnalyzeFile(task.FileName);
            }

            IPythonProjectEntry pyCode;
            if (item != null && (pyCode = item as IPythonProjectEntry) != null) {
                // We subscribe to OnNewAnalysis here instead of OnNewParseTree so that 
                // in the future we can use the analysis to include type information in the
                // object browser (for example we could include base type information with
                // links elsewhere in the object browser).
                pyCode.OnNewAnalysis += (sender, args) => {
                    FileParsed(task, new AstScopeNode(pyCode.Tree, pyCode));
                };
            }
        }
    }
}
