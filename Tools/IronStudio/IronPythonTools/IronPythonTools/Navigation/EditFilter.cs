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
using System.Windows;
using Microsoft.IronPythonTools.Commands;
using Microsoft.IronPythonTools.Editor.Core;
using Microsoft.IronPythonTools.Intellisense;
using Microsoft.IronPythonTools.Navigation;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Core.Repl;
using Microsoft.IronStudio.Navigation;
using Microsoft.PyAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.IronPythonTools.Language {
    /// <summary>
    /// IOleCommandTarget implementation for interacting with various editor commands.  This enables
    /// wiring up most of our features to the VisualStudio editor.  We currently support:
    ///     Goto Definition
    ///     Find All References
    ///     Show Member List
    ///     Complete Word
    ///     Enable/Disable Outlining
    ///     Comment/Uncomment block
    /// 
    /// We also support IronPython specific commands via this class.  Currently these commands are
    /// added by updating our CommandTable class to contain a new command.  These commands also need
    /// to be registered in our .vsct file so that VS knows about them.
    /// </summary>
    class EditFilter : IOleCommandTarget {
        private readonly IWpfTextView _textView;
        private readonly IOleCommandTarget _next;
        private readonly IPythonAnalyzer _analyzer;

        public EditFilter(IPythonAnalyzer analyzer, IWpfTextView textView, IVsTextView vsTextView) {
            _textView = textView;
            _analyzer = analyzer;
            ErrorHandler.ThrowOnFailure(vsTextView.AddCommandFilter(this, out _next));
        }

        /// <summary>
        /// Implements Goto Definition.  Called when the user selects Goto Definition from the 
        /// context menu or hits the hotkey associated with Goto Definition.
        /// 
        /// If there is 1 and only one definition immediately navigates to it.  If there are
        /// no references displays a dialog box to the user.  Otherwise it opens the find
        /// symbols dialog with the list of results.
        /// </summary>
        private int GotoDefinition() {
            UpdateStatusForIncompleteAnalysis();

            var analysis = GetExpressionAnalysis();

            Dictionary<LocationInfo, SimpleLocationInfo> references, definitions, values;
            GetDefsRefsAndValues(analysis, out definitions, out references, out values);

            if ((values.Count + definitions.Count) == 1) {
                if (values.Count != 0) {
                    foreach (var location in values.Keys) {
                        location.GotoSource();
                        break;
                    }
                } else {
                    foreach (var location in definitions.Keys) {
                        location.GotoSource();
                        break;
                    }
                }
            } else if (values.Count + definitions.Count == 0) {
                if (references.Count != 0) {
                    ShowFindSymbolsDialog(analysis, new SymbolList("References", StandardGlyphGroup.GlyphReference, references.Values));
                } else {
                    MessageBox.Show(String.Format("Cannot go to definition \"{0}\"", analysis.Expression));
                }
            } else if (definitions.Count == 0) {
                ShowFindSymbolsDialog(analysis, new SymbolList("Values", StandardGlyphGroup.GlyphForwardType, values.Values));
            } else if (values.Count == 0) {
                ShowFindSymbolsDialog(analysis, new SymbolList("Definitions", StandardGlyphGroup.GlyphLibrary, definitions.Values));
            } else {
                ShowFindSymbolsDialog(analysis,
                    new SymbolList("Definitions", StandardGlyphGroup.GlyphLibrary, definitions.Values),
                    new SymbolList("Values", StandardGlyphGroup.GlyphForwardType, values.Values)
                );
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Implements Find All References.  Called when the user selects Find All References from
        /// the context menu or hits the hotkey associated with find all references.
        /// 
        /// Always opens the Find Symbol Results box to display the results.
        /// </summary>
        private int FindAllReferences() {
            UpdateStatusForIncompleteAnalysis();

            var analysis = GetExpressionAnalysis();

            Dictionary<LocationInfo, SimpleLocationInfo> references, definitions, values;
            GetDefsRefsAndValues(analysis, out definitions, out references, out values);

            ShowFindSymbolsDialog(analysis,
                new SymbolList("Definitions", StandardGlyphGroup.GlyphLibrary, definitions.Values),
                new SymbolList("Values", StandardGlyphGroup.GlyphForwardType, values.Values),
                new SymbolList("References", StandardGlyphGroup.GlyphReference, references.Values)
            );

            return VSConstants.S_OK;
        }

        private static void GetDefsRefsAndValues(ExpressionAnalysis provider, out Dictionary<LocationInfo, SimpleLocationInfo> definitions, out Dictionary<LocationInfo, SimpleLocationInfo> references, out Dictionary<LocationInfo, SimpleLocationInfo> values) {
            references = new Dictionary<LocationInfo, SimpleLocationInfo>();
            definitions = new Dictionary<LocationInfo, SimpleLocationInfo>();
            values = new Dictionary<LocationInfo,SimpleLocationInfo>();

            foreach (var v in provider.Variables) {
                switch (v.Type) {
                    case VariableType.Definition:
                        values.Remove(v.Location);
                        definitions[v.Location] = new SimpleLocationInfo(provider.Expression, v.Location, StandardGlyphGroup.GlyphGroupField);
                        break;
                    case VariableType.Reference:
                        references[v.Location] = new SimpleLocationInfo(provider.Expression, v.Location, StandardGlyphGroup.GlyphGroupField);
                        break;
                    case VariableType.Value:
                        if (!definitions.ContainsKey(v.Location)) {
                            values[v.Location] = new SimpleLocationInfo(provider.Expression, v.Location, StandardGlyphGroup.GlyphGroupField);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Opens the find symbols dialog with a list of results.  This is done by requesting
        /// that VS does a search against our library GUID.  Our library then responds to
        /// that request by extracting the prvoided symbol list out and using that for the
        /// search results.
        /// </summary>
        private static void ShowFindSymbolsDialog(ExpressionAnalysis provider, params SymbolList[] symbols) {
            // ensure our library is loaded so find all references will go to our library
            Package.GetGlobalService(typeof(IPythonLibraryManager));

            if (provider.Expression != "") {
                var findSym = (IVsFindSymbol)IronPythonToolsPackage.GetGlobalService(typeof(SVsObjectSearch));
                VSOBSEARCHCRITERIA2 searchCriteria = new VSOBSEARCHCRITERIA2();
                searchCriteria.eSrchType = VSOBSEARCHTYPE.SO_ENTIREWORD;
                searchCriteria.pIVsNavInfo = symbols.Length == 1 ? (IVsNavInfo)symbols[0] : (IVsNavInfo)new LocationCategory("Test", symbols);
                searchCriteria.grfOptions = (uint)_VSOBSEARCHOPTIONS2.VSOBSO_LISTREFERENCES;
                searchCriteria.szName = provider.Expression;

                Guid guid = Guid.Empty;
                //  new Guid("{a5a527ea-cf0a-4abf-b501-eafe6b3ba5c6}")
                ErrorHandler.ThrowOnFailure(findSym.DoSearch(new Guid(CommonConstants.LibraryGuid), new VSOBSEARCHCRITERIA2[] { searchCriteria }));
            } else {
                var statusBar = (IVsStatusbar)CommonPackage.GetGlobalService(typeof(SVsStatusbar));
                statusBar.SetText("The caret must be on valid expression to find all references.");
            }
        }

        private ExpressionAnalysis GetExpressionAnalysis() {
            var textView = _textView;
            var textBuffer = _textView.TextBuffer;
            var snapshot = textBuffer.CurrentSnapshot;
            int caretPos = _textView.Caret.Position.BufferPosition.Position;

            // foo(
            //    ^
            //    +---  Caret here
            //
            // We want to lookup foo, not foo(
            //
            ITrackingSpan span;
            if (caretPos != snapshot.Length) {
                string curChar = snapshot.GetText(caretPos, 1);
                if (!IsIdentifierChar(curChar[0]) && caretPos > 0) {
                    string prevChar = snapshot.GetText(caretPos - 1, 1);
                    if (IsIdentifierChar(prevChar[0])) {
                        caretPos--;
                    }
                }
                span = snapshot.CreateTrackingSpan(
                    caretPos,
                    1,
                    SpanTrackingMode.EdgeInclusive
                );
            } else {
                span = snapshot.CreateTrackingSpan(
                    caretPos,
                    0,
                    SpanTrackingMode.EdgeInclusive
                );
            }

            return _analyzer.AnalyzeExpression(snapshot, textBuffer, span);
        }

        class LocationCategory : SimpleObjectList<SymbolList>, IVsNavInfo, ICustomSearchListProvider {
            private readonly string _name;

            public LocationCategory(string name, params SymbolList[] locations) {
                _name = name;

                foreach (var location in locations) {
                    if (location.Children.Count > 0) {
                        Children.Add(location);
                    }
                }
            }
           
            public override uint CategoryField(LIB_CATEGORY lIB_CATEGORY) {
                return (uint)(_LIB_LISTTYPE.LLT_HIERARCHY | _LIB_LISTTYPE.LLT_MEMBERS | _LIB_LISTTYPE.LLT_PACKAGE);
            }

            #region IVsNavInfo Members

            public int EnumCanonicalNodes(out IVsEnumNavInfoNodes ppEnum) {
                ppEnum = new NodeEnumerator<SymbolList>(Children);
                return VSConstants.S_OK;
            }

            public int EnumPresentationNodes(uint dwFlags, out IVsEnumNavInfoNodes ppEnum) {
                ppEnum = new NodeEnumerator<SymbolList>(Children);
                return VSConstants.S_OK;
            }

            public int GetLibGuid(out Guid pGuid) {
                pGuid = Guid.Empty;
                return VSConstants.S_OK;
            }

            public int GetSymbolType(out uint pdwType) {
                pdwType = (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion

            #region ICustomSearchListProvider Members

            public IVsSimpleObjectList2 GetSearchList() {
                return this;
            }

            #endregion
        }

        class SimpleLocationInfo : SimpleObject, IVsNavInfoNode {
            private readonly LocationInfo _locationInfo;
            private readonly StandardGlyphGroup _glyphType;
            private readonly string _pathText, _lineText;

            public SimpleLocationInfo(string searchText, LocationInfo locInfo, StandardGlyphGroup glyphType) {
                _locationInfo = locInfo;
                _glyphType = glyphType;
                _pathText = GetSearchDisplayText();
                _lineText = _locationInfo.ProjectEntry.GetLine(_locationInfo.Line);
            }

            public override string Name {
                get {
                    return _locationInfo.FilePath;
                }
            }

            public override string GetTextRepresentation(VSTREETEXTOPTIONS options) {
                if (options == VSTREETEXTOPTIONS.TTO_DEFAULT) {
                    return _pathText + _lineText.Trim();
                }
                return String.Empty;
            }

            private string GetSearchDisplayText() {
                return String.Format("{0} - ({1}, {2}): ",
                    _locationInfo.FilePath,
                    _locationInfo.Line,
                    _locationInfo.Column);
            }

            public override string UniqueName {
                get {
                    return _locationInfo.FilePath;
                }
            }

            public override bool CanGoToSource {
                get {
                    return true;
                }
            }

            public override VSTREEDISPLAYDATA DisplayData {
                get {
                    var res = new VSTREEDISPLAYDATA();
                    res.Image = res.SelectedImage = (ushort)_glyphType;
                    res.State = (uint)_VSTREEDISPLAYSTATE.TDS_FORCESELECT;

                    // This code highlights the text but it gets the wrong region.  This should be re-enabled
                    // and highlight the correct region.

                    //res.ForceSelectStart = (ushort)(_pathText.Length + _locationInfo.Column - 1);
                    //res.ForceSelectLength = (ushort)_locationInfo.Length;
                    return res;
                }
            }

            public override void GotoSource(VSOBJGOTOSRCTYPE SrcType) {
                _locationInfo.GotoSource();
            }

            #region IVsNavInfoNode Members

            public int get_Name(out string pbstrName) {
                pbstrName = _locationInfo.FilePath;
                return VSConstants.S_OK;
            }

            public int get_Type(out uint pllt) {
                pllt = 16; // (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion
        }

        class SymbolList : SimpleObjectList<SimpleLocationInfo>, IVsNavInfo, IVsNavInfoNode, ICustomSearchListProvider, ISimpleObject {
            private readonly string _name;
            private readonly StandardGlyphGroup _glyphGroup;

            public SymbolList(string description, StandardGlyphGroup glyphGroup, IEnumerable<SimpleLocationInfo> locations) {
                _name = description;
                _glyphGroup = glyphGroup;
                Children.AddRange(locations);
            }

            public override uint CategoryField(LIB_CATEGORY lIB_CATEGORY) {
                return (uint)(_LIB_LISTTYPE.LLT_MEMBERS | _LIB_LISTTYPE.LLT_PACKAGE);
            }

            #region ISimpleObject Members

            public bool CanDelete {
                get { return false; }
            }

            public bool CanGoToSource {
                get { return false; }
            }

            public bool CanRename {
                get { return false; }
            }

            public string Name {
                get { return _name; }
            }

            public string UniqueName {
                get { return _name; }
            }

            public string GetTextRepresentation(VSTREETEXTOPTIONS options) {
                switch(options) {
                    case VSTREETEXTOPTIONS.TTO_DISPLAYTEXT:
                        return _name;
                }
                return null;
            }

            public string TooltipText {
                get { return null; }
            }

            public object BrowseObject {
                get { return null; }
            }

            public System.ComponentModel.Design.CommandID ContextMenuID {
                get { return null; }
            }

            public VSTREEDISPLAYDATA DisplayData {
                get { 
                    var res = new VSTREEDISPLAYDATA();
                    res.Image = res.SelectedImage = (ushort)_glyphGroup;
                    return res;
                }
            }

            public void Delete() {
            }

            public void DoDragDrop(OleDataObject dataObject, uint grfKeyState, uint pdwEffect) {
            }

            public void Rename(string pszNewName, uint grfFlags) {
            }

            public void GotoSource(VSOBJGOTOSRCTYPE SrcType) {
            }

            public void SourceItems(out IVsHierarchy ppHier, out uint pItemid, out uint pcItems) {
                ppHier = null;
                pItemid = 0;
                pcItems = 0;
            }

            public uint EnumClipboardFormats(_VSOBJCFFLAGS _VSOBJCFFLAGS, VSOBJCLIPFORMAT[] rgcfFormats) {
                return VSConstants.S_OK;
            }

            public void FillDescription(_VSOBJDESCOPTIONS _VSOBJDESCOPTIONS, IVsObjectBrowserDescription3 pobDesc) {
            }

            public IVsSimpleObjectList2 FilterView(uint ListType) {
                return this;
            }

            #endregion

            #region IVsNavInfo Members

            public int EnumCanonicalNodes(out IVsEnumNavInfoNodes ppEnum) {
                ppEnum = new NodeEnumerator<SimpleLocationInfo>(Children);
                return VSConstants.S_OK;
            }

            public int EnumPresentationNodes(uint dwFlags, out IVsEnumNavInfoNodes ppEnum) {
                ppEnum = new NodeEnumerator<SimpleLocationInfo>(Children);
                return VSConstants.S_OK;
            }

            public int GetLibGuid(out Guid pGuid) {
                pGuid = Guid.Empty;
                return VSConstants.S_OK;
            }

            public int GetSymbolType(out uint pdwType) {
                pdwType = (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion

            #region ICustomSearchListProvider Members

            public IVsSimpleObjectList2 GetSearchList() {
                return this;
            }

            #endregion

            #region IVsNavInfoNode Members

            public int get_Name(out string pbstrName) {
                pbstrName = "name";
                return VSConstants.S_OK;
            }

            public int get_Type(out uint pllt) {
                pllt = 16; // (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion
        }

        class NodeEnumerator<T> : IVsEnumNavInfoNodes where T : IVsNavInfoNode {
            private readonly List<T> _locations;
            private IEnumerator<T> _locationEnum;

            public NodeEnumerator(List<T> locations) {
                _locations = locations;
                Reset();
            }

            #region IVsEnumNavInfoNodes Members

            public int Clone(out IVsEnumNavInfoNodes ppEnum) {
                ppEnum = new NodeEnumerator<T>(_locations);
                return VSConstants.S_OK;
            }

            public int Next(uint celt, IVsNavInfoNode[] rgelt, out uint pceltFetched) {
                pceltFetched = 0;
                while (celt-- != 0 && _locationEnum.MoveNext()) {
                    rgelt[pceltFetched++] = _locationEnum.Current;
                }
                return VSConstants.S_OK;
            }

            public int Reset() {
                _locationEnum = _locations.GetEnumerator();
                return VSConstants.S_OK;
            }

            public int Skip(uint celt) {
                while (celt-- != 0) {
                    _locationEnum.MoveNext();
                }
                return VSConstants.S_OK;
            }

            #endregion
        }

        private static bool IsIdentifierChar(char curChar) {
            return Char.IsLetterOrDigit(curChar) || curChar == '_';
        }

        private void UpdateStatusForIncompleteAnalysis() {
            var statusBar = (IVsStatusbar)CommonPackage.GetGlobalService(typeof(SVsStatusbar));
            if (!IronPythonToolsPackage.Instance.Analyzer.IsAnalyzing) {
                statusBar.SetText("Python source analysis is not up to date");
            }
        }

        #region IOleCommandTarget Members

        /// <summary>
        /// Called from VS when we should handle a command or pass it on.
        /// </summary>
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            // preprocessing
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                switch ((VSConstants.VSStd97CmdID)nCmdID) {
                    case VSConstants.VSStd97CmdID.GotoDefn: return GotoDefinition();
                    case VSConstants.VSStd97CmdID.FindReferences: return FindAllReferences();
                        
                }
            } else if (pguidCmdGroup == CommonConstants.Std2KCmdGroupGuid) {
                OutliningTaggerProvider.OutliningTagger tagger;
                switch ((VSConstants.VSStd2KCmdID)nCmdID) {
                    case VSConstants.VSStd2KCmdID.RETURN:
                        if (IronPythonToolsPackage.Instance.LangPrefs.IndentMode == vsIndentStyle.vsIndentStyleSmart) {
                            // smart indent

                            AutoIndent.HandleReturn(_textView, (IClassifier)_textView.TextBuffer.Properties.GetProperty(typeof(IDlrClassifier)));
                            return VSConstants.S_OK;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.BACKSPACE:

                        if (IronPythonToolsPackage.Instance.LangPrefs.IndentMode == vsIndentStyle.vsIndentStyleSmart &&
                            _textView.Selection.IsEmpty) {
                            int indentSize = _textView.Options.GetIndentSize();
                            // smart dedent
                            var containingLine = _textView.Caret.Position.BufferPosition.GetContainingLine();
                            var curLineLine = containingLine.GetText();
                            
                            int lineOffset = _textView.Caret.Position.BufferPosition.Position - containingLine.Start.Position;
                            if (lineOffset >= indentSize) {
                                bool allSpaces = true;
                                for (int i = lineOffset - 1; i >= lineOffset - indentSize; i--) {
                                    if (curLineLine[i] != ' ') {
                                        allSpaces = false;
                                        break;
                                    }
                                }

                                if (allSpaces) {
                                    _textView.TextBuffer.Delete(new Span(_textView.Caret.Position.BufferPosition.Position - indentSize, indentSize));
                                    return VSConstants.S_OK;
                                }
                            }
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                    case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                        var controller = _textView.Properties.GetProperty<IntellisenseController>(typeof(IntellisenseController));
                        if (controller != null) {
                            controller.TriggerCompletionSession((VSConstants.VSStd2KCmdID)nCmdID == VSConstants.VSStd2KCmdID.COMPLETEWORD);
                            return VSConstants.S_OK;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
                        tagger = _textView.TextBuffer.GetOutliningTagger();
                        if (tagger != null) {
                            tagger.Disable();
                        }
                        // let VS get the event as well
                        break;
                    case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
                        tagger = _textView.TextBuffer.GetOutliningTagger();
                        if (tagger != null) {
                            tagger.Enable();
                        }
                        // let VS get the event as well
                        break;
                    case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        _textView.CommentBlock();
                        break;
                    case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        _textView.UncommentBlock();
                        break;
                }
            }

            return _next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        /// <summary>
        /// Called from VS to see what commands we support.  
        /// </summary>
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                for (int i = 0; i < cCmds; i++) {
                    switch ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd97CmdID.GotoDefn:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                        case VSConstants.VSStd97CmdID.FindReferences:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            } else if (pguidCmdGroup == GuidList.guidIronPythonToolsCmdSet) {
                for (int i = 0; i < cCmds; i++) {
                    foreach (var command in CommandTable.Commands) {
                        if (command.CommandId == prgCmds[i].cmdID) {
                            int? res = command.EditFilterQueryStatus(ref prgCmds[i], pCmdText);
                            if (res != null) {
                                return res.Value;
                            }
                        }
                    }
                }
            } else if (pguidCmdGroup == CommonConstants.Std2KCmdGroupGuid) {                
                OutliningTaggerProvider.OutliningTagger tagger;
                for (int i = 0; i < cCmds; i++) {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                        case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
                            tagger = _textView.TextBuffer.GetOutliningTagger();
                            if (tagger != null && tagger.Enabled) {
                                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            }
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
                            tagger = _textView.TextBuffer.GetOutliningTagger();
                            if (tagger != null && !tagger.Enabled) {
                                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            }
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }
            return _next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #endregion
    }
}
