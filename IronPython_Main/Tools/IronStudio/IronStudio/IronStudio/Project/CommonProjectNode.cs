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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

//
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Scripting.Utils;
using Microsoft.Windows.Design.Host;

namespace Microsoft.IronStudio.Project {
    using Microsoft.IronStudio.Navigation;
    using VSConstants = Microsoft.VisualStudio.VSConstants;
    
    public enum CommonImageName {
        File = 0,
        Project = 1,
        SearchPathContainer,
        SearchPath,
        MissingSearchPath,
        StartupFile
    }

    public abstract class CommonProjectNode : ProjectNode, IVsProjectSpecificEditorMap2, IVsDeferredSaveProject {

        #region abstract methods

        public abstract Type GetProjectFactoryType();
        public abstract Type GetEditorFactoryType();
        public abstract string GetProjectName();
        public abstract string GetCodeFileExtension();
        public virtual CommonFileNode CreateCodeFileNode(ProjectElement item) {
            return new CommonFileNode(this, item);
        }
        public virtual CommonFileNode CreateNonCodeFileNode(ProjectElement item) {
            return new CommonNonCodeFileNode(this, item);
        }
        public abstract string GetFormatList();
        public abstract Type GetGeneralPropertyPageType();
        public abstract Type GetLibraryManagerType();
        public abstract string GetProjectFileExtension();

        #endregion

        #region fields

        private CommonProjectPackage/*!*/ _package;
        private Guid _mruPageGuid = new Guid(CommonConstants.AddReferenceMRUPageGuid);
        private VSLangProj.VSProject _vsProject = null;
        private static ImageList _imageList;
        private ProjectDocumentsListenerForStartupFileUpdates _projectDocListenerForStartupFileUpdates;
        private static int _imageOffset;
        private CommonSearchPathContainerNode _searchPathContainer;
        private string _projectDir;
        private bool _isRefreshing;
        private object _automationObject;

        #endregion

        #region Properties

        public new CommonProjectPackage/*!*/ Package {
            get { return _package; }
        }

        public static int ImageOffset {
            get { return _imageOffset; }
        }

        /// <summary>
        /// Get the VSProject corresponding to this project
        /// </summary>
        protected internal VSLangProj.VSProject VSProject {
            get {
                if (_vsProject == null)
                    _vsProject = new OAVSProject(this);
                return _vsProject;
            }
        }

        private IVsHierarchy InteropSafeHierarchy {
            get {
                IntPtr unknownPtr = Utilities.QueryInterfaceIUnknown(this);
                if (IntPtr.Zero == unknownPtr) {
                    return null;
                }
                IVsHierarchy hier = Marshal.GetObjectForIUnknown(unknownPtr) as IVsHierarchy;
                return hier;
            }
        }

        /// <summary>
        /// Returns project's directory name.
        /// </summary>
        public string ProjectDir {
            get { return _projectDir; }
        }

        /// <summary>
        /// Indicates whether the project is currently is busy refreshing its hierarchy.
        /// </summary>
        public bool IsRefreshing {
            get { return _isRefreshing; }
        }

        /// <summary>
        /// Language specific project images
        /// </summary>
        public static ImageList ImageList {
            get {
                return _imageList;
            }
            set {
                _imageList = value;
            }
        }
        #endregion

        #region ctor

        public CommonProjectNode(CommonProjectPackage/*!*/ package, ImageList/*!*/ imageList) {
            ContractUtils.RequiresNotNull(package, "package");
            ContractUtils.RequiresNotNull(imageList, "imageList");

            _package = package;
            CanFileNodesHaveChilds = true;
            OleServiceProvider.AddService(typeof(VSLangProj.VSProject), new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);
            SupportsProjectDesigner = true;
            _imageList = imageList;

            //Store the number of images in ProjectNode so we know the offset of the language icons.
            _imageOffset = ImageHandler.ImageList.Images.Count;
            foreach (Image img in ImageList.Images) {
                ImageHandler.AddImage(img);
            }

            InitializeCATIDs();
        }

        #endregion

        #region overridden properties

        /// <summary>
        /// Since we appended the language images to the base image list in the constructor,
        /// this should be the offset in the ImageList of the langauge project icon.
        /// </summary>
        public override int ImageIndex {
            get {
                return _imageOffset + (int)CommonImageName.Project;
            }
        }

        public override Guid ProjectGuid {
            get {
                return GetProjectFactoryType().GUID;
            }
        }
        public override string ProjectType {
            get {
                return GetProjectName();
            }
        }
        internal override object Object {
            get {
                return VSProject;
            }
        }
        #endregion

        #region overridden methods

        public override object GetAutomationObject() {
            if (_automationObject == null) {
                _automationObject = base.GetAutomationObject();
            }
            return _automationObject;
        }

        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result) {
            if (cmdGroup == CommonConstants.Std97CmdGroupGuid) {
                switch ((VSConstants.VSStd97CmdID)cmd) {
                    case VSConstants.VSStd97CmdID.BuildCtx:
                    case VSConstants.VSStd97CmdID.RebuildCtx:
                    case VSConstants.VSStd97CmdID.CleanCtx:
                        result = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                        return VSConstants.S_OK;
                }
            } else if (cmdGroup == GuidList.guidIronStudioCmdSet) {
                switch ((int)cmd) {
                    case CommonConstants.AddSearchPathCommandId:
                    case CommonConstants.StartWithoutDebuggingCmdId:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        protected override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            if (cmdGroup == GuidList.guidIronStudioCmdSet) {
                switch ((int)cmd) {
                    case CommonConstants.AddSearchPathCommandId:
                        AddSearchPath();
                        return VSConstants.S_OK;
                    case CommonConstants.StartWithoutDebuggingCmdId:
                        EnvDTE.Project automationObject = this.GetAutomationObject() as EnvDTE.Project;
                        string activeConfigName = Utilities.GetActiveConfigurationName(automationObject);
                        CommonProjectConfig config = new CommonProjectConfig(this, activeConfigName);
                        return config.DebugLaunch((uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_NoDebug);
                }
            }
            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        /// <summary>
        /// As we don't register files/folders in the project file, removing an item is a noop.
        /// </summary>
        public override int RemoveItem(uint reserved, uint itemId, out int result) {
            result = 1;
            return VSConstants.S_OK;
        }

        //No build for dynamic languages by default!
        public override MSBuildResult Build(uint vsopts, string config, IVsOutputWindowPane output, string target) {
            return MSBuildResult.Successful;
        }

        internal override void BuildAsync(uint vsopts, string config, IVsOutputWindowPane output, string target, Action<MSBuildResult, string> uiThreadCallback) {
            uiThreadCallback(MSBuildResult.Successful, target);
        }

        /// <summary>
        /// Overriding main project loading method to inject our hierarachy of nodes.
        /// </summary>
        protected override void Reload() {
            _projectDir = Path.GetDirectoryName(this.BaseURI.Uri.LocalPath);
            _searchPathContainer = new CommonSearchPathContainerNode(this);
            this.AddChild(_searchPathContainer);
            base.Reload();
            RefreshHierarchy();
            OnProjectPropertyChanged += new EventHandler<ProjectPropertyChangedArgs>(CommonProjectNode_OnProjectPropertyChanged);
        }

        protected override ReferenceContainerNode CreateReferenceContainerNode() {
            return new CommonReferenceContainerNode(this);
        }

        public override int GetGuidProperty(int propid, out Guid guid) {
            if ((__VSHPROPID)propid == __VSHPROPID.VSHPROPID_PreferredLanguageSID) {
                guid = new Guid("{EFB9A1D6-EA71-4F38-9BA7-368C33FCE8DC}");// GetLanguageServiceType().GUID;
            } else {
                return base.GetGuidProperty(propid, out guid);
            }
            return VSConstants.S_OK;
        }

        protected override bool IsItemTypeFileType(string type) {
            if (!base.IsItemTypeFileType(type)) {
                if (String.Compare(type, "Page", StringComparison.OrdinalIgnoreCase) == 0
                || String.Compare(type, "ApplicationDefinition", StringComparison.OrdinalIgnoreCase) == 0
                || String.Compare(type, "Resource", StringComparison.OrdinalIgnoreCase) == 0) {
                    return true;
                } else {
                    return false;
                }
            } else {
                //This is a well known item node type, so return true.
                return true;
            }
        }

        protected override NodeProperties CreatePropertiesObject() {
            return new CommonProjectNodeProperties(this);
        }

        public override int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider site) {
            base.SetSite(site);

            //Initialize a new object to track project document changes so that we can update the StartupFile Property accordingly
            _projectDocListenerForStartupFileUpdates = new ProjectDocumentsListenerForStartupFileUpdates((ServiceProvider)Site, this);
            _projectDocListenerForStartupFileUpdates.Init();

            return VSConstants.S_OK;
        }

        public override int Close() {
            if (null != _projectDocListenerForStartupFileUpdates) {
                _projectDocListenerForStartupFileUpdates.Dispose();
                _projectDocListenerForStartupFileUpdates = null;
            }
            if (null != Site) {
                LibraryManager libraryManager = Site.GetService(GetLibraryManagerType()) as LibraryManager;
                if (null != libraryManager) {
                    libraryManager.UnregisterHierarchy(InteropSafeHierarchy);
                }
            }

            return base.Close();
        }

        public override void Load(string filename, string location, string name, uint flags, ref Guid iidProject, out int canceled) {
            base.Load(filename, location, name, flags, ref iidProject, out canceled);
            LibraryManager libraryManager = Site.GetService(GetLibraryManagerType()) as LibraryManager;
            if (null != libraryManager) {
                libraryManager.RegisterHierarchy(InteropSafeHierarchy);
            }

            //If this is a WPFFlavor-ed project, then add a project-level DesignerContext service to provide
            //event handler generation (EventBindingProvider) for the XAML designer.
            this.OleServiceProvider.AddService(typeof(DesignerContext), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
        }

        /// <summary>
        /// Overriding to provide project general property page
        /// </summary>
        /// <returns></returns>
        protected override Guid[] GetConfigurationIndependentPropertyPages() {
            Guid[] result = new Guid[1];
            result[0] = GetGeneralPropertyPageType().GUID;
            return result;
        }

        /// <summary>
        /// Overriding to provide customization of files on add files.
        /// This will replace tokens in the file with actual value (namespace, class name,...)
        /// </summary>
        /// <param name="source">Full path to template file</param>
        /// <param name="target">Full path to destination file</param>
        public override void AddFileFromTemplate(string source, string target) {
            if (!System.IO.File.Exists(source))
                throw new FileNotFoundException(String.Format("Template file not found: {0}", source));

            // We assume that there is no token inside the file because the only
            // way to add a new element should be through the template wizard that
            // take care of expanding and replacing the tokens.
            // The only task to perform is to copy the source file in the
            // target location.
            string targetFolder = Path.GetDirectoryName(target);
            if (!Directory.Exists(targetFolder)) {
                Directory.CreateDirectory(targetFolder);
            }

            File.Copy(source, target);
        }

        /// <summary>
        /// Evaluates if a file is a current language code file based on is extension
        /// </summary>
        /// <param name="strFileName">The filename to be evaluated</param>
        /// <returns>true if is a code file</returns>
        public override bool IsCodeFile(string strFileName) {
            // We do not want to assert here, just return silently.
            if (String.IsNullOrEmpty(strFileName)) {
                return false;
            }
            return (String.Compare(Path.GetExtension(strFileName),
                GetCodeFileExtension(),
                StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Create a file node based on an msbuild item.
        /// </summary>
        /// <param name="item">The msbuild item to be analyzed</param>        
        public override FileNode CreateFileNode(ProjectElement item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            CommonFileNode newNode;
            if (string.Compare(GetCodeFileExtension(), Path.GetExtension(item.GetFullPathForElement()), StringComparison.OrdinalIgnoreCase) == 0) {
                newNode = CreateCodeFileNode(item);
            } else {
                newNode = CreateNonCodeFileNode(item);
            }
            string include = item.GetMetadata(ProjectFileConstants.Include);

            newNode.OleServiceProvider.AddService(typeof(EnvDTE.Project),
                new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);
            newNode.OleServiceProvider.AddService(typeof(EnvDTE.ProjectItem), newNode.ServiceCreator, false);
            if (!string.IsNullOrEmpty(include) && Path.GetExtension(include).Equals(".xaml", StringComparison.OrdinalIgnoreCase)) {
                //Create a DesignerContext for the XAML designer for this file
                newNode.OleServiceProvider.AddService(typeof(DesignerContext), newNode.ServiceCreator, false);
            }

            newNode.OleServiceProvider.AddService(typeof(VSLangProj.VSProject),
                new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);
            return newNode;
        }

        /// <summary>
        /// Create a file node based on absolute file name.
        /// </summary>
        public override FileNode CreateFileNode(string absFileName) {
            // Avoid adding files to the project multiple times.  Ultimately           
            // we should not use project items and instead should have virtual items.       
            string path = absFileName;
            if (absFileName.Length > ProjectDir.Length &&
                String.Compare(ProjectDir, 0, absFileName, 0, ProjectDir.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                path = absFileName.Substring(ProjectDir.Length);
                if (path.StartsWith("\\")) {
                    path = path.Substring(1);
                }
            }
            var prjItem = GetExistingItem(absFileName) ?? BuildProject.AddItem("None", path)[0];
            ProjectElement prjElem = new ProjectElement(this, prjItem, false);
            return CreateFileNode(prjElem);
        }

        protected Microsoft.Build.Evaluation.ProjectItem GetExistingItem(string absFileName) {
            Microsoft.Build.Evaluation.ProjectItem prjItem = null;
            foreach (var item in BuildProject.Items) {
                if (item.UnevaluatedInclude == absFileName) {
                    prjItem = item;
                    break;
                }
            }
            return prjItem;
        }

        public ProjectElement MakeProjectElement(string type, string path) {
            var item = BuildProject.AddItem(type, path)[0];
            return new ProjectElement(this, item, false);
        }
        
        public override int IsDirty(out int isDirty) {
            isDirty = 0;
            if (IsProjectFileDirty) {
                isDirty = 1;
                return VSConstants.S_OK;
            }

            isDirty = IsFlavorDirty();
            return VSConstants.S_OK;
        }

        public override DependentFileNode CreateDependentFileNode(ProjectElement item) {
            DependentFileNode node = base.CreateDependentFileNode(item);
            if (null != node) {
                string include = item.GetMetadata(ProjectFileConstants.Include);
                if (IsCodeFile(include)) {
                    node.OleServiceProvider.AddService(
                        typeof(SVSMDCodeDomProvider), new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);
                }
            }

            return node;
        }

        /// <summary>
        /// Creates the format list for the open file dialog
        /// </summary>
        /// <param name="formatlist">The formatlist to return</param>
        /// <returns>Success</returns>
        public override int GetFormatList(out string formatlist) {
            formatlist = GetFormatList();
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This overrides the base class method to show the VS 2005 style Add reference dialog. The ProjectNode implementation
        /// shows the VS 2003 style Add Reference dialog.
        /// </summary>
        /// <returns>S_OK if succeeded. Failure other wise</returns>
        public override int AddProjectReference() {
            IVsComponentSelectorDlg2 componentDialog;
            Guid guidEmpty = Guid.Empty;
            VSCOMPONENTSELECTORTABINIT[] tabInit = new VSCOMPONENTSELECTORTABINIT[1];
            string strBrowseLocations = Path.GetDirectoryName(BaseURI.Uri.LocalPath);

            //Add the Project page
            tabInit[0].dwSize = (uint)Marshal.SizeOf(typeof(VSCOMPONENTSELECTORTABINIT));
            // Tell the Add Reference dialog to call hierarchies GetProperty with the following
            // propID to enable filtering out ourself from the Project to Project reference
            tabInit[0].varTabInitInfo = (int)__VSHPROPID.VSHPROPID_ShowProjInSolutionPage;
            tabInit[0].guidTab = VSConstants.GUID_SolutionPage;

            uint pX = 0, pY = 0;

            componentDialog = GetService(typeof(SVsComponentSelectorDlg)) as IVsComponentSelectorDlg2;
            try {
                // call the container to open the add reference dialog.
                if (componentDialog != null) {
                    // Let the project know not to show itself in the Add Project Reference Dialog page
                    ShowProjectInSolutionPage = false;

                    // call the container to open the add reference dialog.
                    ErrorHandler.ThrowOnFailure(componentDialog.ComponentSelectorDlg2(
                        (System.UInt32)(__VSCOMPSELFLAGS.VSCOMSEL_MultiSelectMode | __VSCOMPSELFLAGS.VSCOMSEL_IgnoreMachineName),
                        (IVsComponentUser)this,
                        0,
                        null,
                DynamicProjectSR.GetString(Microsoft.VisualStudio.Project.SR.AddReferenceDialogTitle),   // Title
                        "VS.AddReference",						  // Help topic
                        ref pX,
                        ref pY,
                        (uint)tabInit.Length,
                        tabInit,
                        ref guidEmpty,
                        "*.dll",
                        ref strBrowseLocations));
                }
            } catch (COMException e) {
                Trace.WriteLine("Exception : " + e.Message);
                return e.ErrorCode;
            } finally {
                // Let the project know it can show itself in the Add Project Reference Dialog page
                ShowProjectInSolutionPage = true;
            }
            return VSConstants.S_OK;
        }

        protected override ConfigProvider CreateConfigProvider() {
            return new CommonConfigProvider(this);
        }
        #endregion

        #region Methods
             
        /// <summary>
        /// Main method for refreshing project hierarchy. It's called on project loading
        /// and each time the project property is changing.
        /// </summary>
        protected void RefreshHierarchy() {
            try {
                _isRefreshing = true;
                string projHome = GetProjectHomeDir();
                string workDir = GetWorkingDirectory();
                IList<string> searchPath = ParseSearchPath();

                //Refresh CWD node
                bool needCWD = !CommonUtils.AreTheSameDirectories(projHome, workDir);
                var cwdNode = FindImmediateChild<CurrentWorkingDirectoryNode>(_searchPathContainer);
                if (needCWD) {
                    if (cwdNode == null) {
                        //No cwd node yet
                        _searchPathContainer.AddChild(new CurrentWorkingDirectoryNode(this, workDir));
                    } else if (!CommonUtils.AreTheSameDirectories(cwdNode.Url, workDir)) {
                        //CWD has changed, recreate the node
                        cwdNode.Remove(false);
                        _searchPathContainer.AddChild(new CurrentWorkingDirectoryNode(this, workDir));
                    }
                } else {
                    //No need to show CWD, remove if exists
                    if (cwdNode != null) {
                        cwdNode.Remove(false);
                    }
                }

                //Refresh regular search path nodes

                //We need to update search path nodes according to the search path property.
                //It's quite expensive to remove all and build all nodes from scratch, 
                //so we are going to perform some smarter update.
                //We are looping over paths in the search path and if a corresponding node
                //exists, we only update its index (sort order), creating new node otherwise.
                //At the end all nodes that haven't been updated have to be removed - they are
                //not in the search path anymore.
                var searchPathNodes = new List<CommonSearchPathNode>();
                this.FindNodesOfType<CommonSearchPathNode>(searchPathNodes);
                bool[] updatedNodes = new bool[searchPathNodes.Count];
                int index;
                for (int i = 0; i < searchPath.Count; i++) {
                    string path = searchPath[i];
                    //ParseSearchPath() must resolve all paths
                    Debug.Assert(Path.IsPathRooted(path));
                    var node = FindSearchPathNodeByPath(searchPathNodes, path, out index);
                    bool alreadyShown = CommonUtils.AreTheSameDirectories(workDir, path) ||
                                        CommonUtils.AreTheSameDirectories(projHome, path);
                    if (!alreadyShown) {
                        if (node != null) {
                            //existing path, update index (sort order)
                            node.Index = i;
                            updatedNodes[index] = true;
                        } else {
                            //new path - create new node
                            _searchPathContainer.AddChild(new CommonSearchPathNode(this, path, i));
                        }
                    }
                }
                //Refresh nodes and remove non-updated ones
                for (int i = 0; i < searchPathNodes.Count; i++) {
                    if (!updatedNodes[i]) {
                        searchPathNodes[i].Remove();
                    }
                }
                // TODO: Port, fix me
                //_searchPathContainer.UpdateSortOrder();
                _searchPathContainer.OnInvalidateItems(this);
            } finally {
                _isRefreshing = false;
            }
        }

        /// <summary>
        /// Returns resolved value of the current working directory property.
        /// </summary>
        public string GetWorkingDirectory() {
            string workDir = this.ProjectMgr.GetProjectProperty(CommonConstants.WorkingDirectory, true);
            if (string.IsNullOrEmpty(workDir)) {
                //If empty - take project directory as working directory
                workDir = _projectDir;
            } else if (!Path.IsPathRooted(workDir)) {
                //If relative path - resolve it based on project home
                workDir = Path.Combine(_projectDir, workDir);
            }
            return CommonUtils.NormalizeDirectoryPath(workDir);
        }

        /// <summary>
        /// Returns resolved value of the project home directory property.
        /// </summary>
        internal string GetProjectHomeDir() {
            string projHome = this.ProjectMgr.GetProjectProperty(CommonConstants.ProjectHome, true);
            if (string.IsNullOrEmpty(projHome)) {
                //If empty - take project directory as project home
                projHome = _projectDir;
            } else if (!Path.IsPathRooted(projHome)) {
                //If relative path - resolve it based on project directory
                projHome = Path.Combine(_projectDir, projHome);
            }
            return CommonUtils.NormalizeDirectoryPath(projHome);
        }

        /// <summary>
        /// Returns resolved value of the startup file property.
        /// </summary>
        internal string GetStartupFile() {
            string startupFile = ProjectMgr.GetProjectProperty(CommonConstants.StartupFile, true);
            if (string.IsNullOrEmpty(startupFile)) {
                //No startup file is assigned
                return null;
            } else if (!Path.IsPathRooted(startupFile)) {
                //If relative path - resolve it based on project home
                return Path.Combine(GetProjectHomeDir(), startupFile);
            }
            return startupFile;
        }

        /// <summary>
        /// Whenever project property has changed - refresh project hierarachy.
        /// </summary>
        private void CommonProjectNode_OnProjectPropertyChanged(object sender, ProjectPropertyChangedArgs e) {
            RefreshHierarchy();
        }

        /// <summary>
        /// Returns first immediate child node (non-recursive) of a given type.
        /// </summary>
        private static T FindImmediateChild<T>(HierarchyNode parent)
            where T : HierarchyNode {
            for (HierarchyNode n = parent.FirstChild; n != null; n = n.NextSibling) {
                if (n is T) {
                    return (T)n;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds Search Path node by a given search path and returns it along with the node's index. 
        /// </summary>
        private CommonSearchPathNode FindSearchPathNodeByPath(IList<CommonSearchPathNode> nodes, string path, out int index) {
            index = 0;
            for (int j = 0; j < nodes.Count; j++) {
                if (CommonUtils.AreTheSameDirectories(nodes[j].Url, path)) {
                    index = j;
                    return nodes[j];
                }
            }
            return null;
        }

        /// <summary>
        /// Provide mapping from our browse objects and automation objects to our CATIDs
        /// </summary>
        private void InitializeCATIDs() {
            Type projectNodePropsType = typeof(CommonProjectNodeProperties);
            Type fileNodePropsType = typeof(CommonFileNodeProperties);
            // The following properties classes are specific to current language so we can use their GUIDs directly
            AddCATIDMapping(projectNodePropsType, projectNodePropsType.GUID);
            AddCATIDMapping(fileNodePropsType, fileNodePropsType.GUID);
            // The following is not language specific and as such we need a separate GUID
            AddCATIDMapping(typeof(FolderNodeProperties), new Guid(CommonConstants.FolderNodePropertiesGuid));
            // This one we use the same as language file nodes since both refer to files
            AddCATIDMapping(typeof(FileNodeProperties), fileNodePropsType.GUID);
            // Because our property page pass itself as the object to display in its grid, 
            // we need to make it have the same CATID
            // as the browse object of the project node so that filtering is possible.
            AddCATIDMapping(GetGeneralPropertyPageType(), projectNodePropsType.GUID);
            // We could also provide CATIDs for references and the references container node, if we wanted to.
        }

        /// <summary>
        /// Parses SearchPath property into a list of distinct absolute paths, preserving the order.
        /// </summary>
        private IList<string> ParseSearchPath() {
            string searchPath = this.ProjectMgr.GetProjectProperty(CommonConstants.SearchPath, true);
            List<string> parsedPaths = new List<string>();
            if (!string.IsNullOrEmpty(searchPath)) {
                foreach (string path in searchPath.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                    string resolvedPath = CommonUtils.NormalizeDirectoryPath(Path.Combine(_projectDir, path));
                    if (!parsedPaths.Contains(resolvedPath)) {
                        parsedPaths.Add(resolvedPath);
                    }
                }
            }
            return parsedPaths;
        }

        /// <summary>
        /// Saves list of paths back as SearchPath project property.
        /// </summary>
        private void SaveSearchPath(IList<string> value) {
            string valueStr = "";
            if (value != null && value.Count > 0) {
                valueStr = value.Aggregate((joined, path) => joined + ';' + path);
            }
            this.ProjectMgr.SetProjectProperty(CommonConstants.SearchPath, valueStr);
        }

        /// <summary>
        /// Adds new search path to the SearchPath project property.
        /// </summary>
        private void AddSearchPathEntry(string newpath) {
            if (newpath == null) {
                throw new ArgumentNullException("newpath");
            }
            IList<string> searchPath = ParseSearchPath();
            if (searchPath.Contains(newpath, StringComparer.CurrentCultureIgnoreCase)) {
                return;
            }
            searchPath.Add(newpath);
            SaveSearchPath(searchPath);
        }

        /// <summary>
        /// Removes a given path from the SearchPath property.
        /// </summary>
        internal void RemoveSearchPathEntry(string path) {
            IList<string> searchPath = ParseSearchPath();
            if (searchPath.Remove(path)) {
                SaveSearchPath(searchPath);
            }
        }

        /// <summary>
        /// Creates the services exposed by this project.
        /// </summary>
        private object CreateServices(Type serviceType) {
            object service = null;
            if (typeof(VSLangProj.VSProject) == serviceType) {
                service = VSProject;
            } else if (typeof(EnvDTE.Project) == serviceType) {
                service = GetAutomationObject();
            } else if (typeof(DesignerContext) == serviceType) {
                service = this.DesignerContext;
            }

            return service;
        }

        protected virtual internal Microsoft.Windows.Design.Host.DesignerContext DesignerContext {
            get {
                return null;
            }
        }

        /// <summary>
        /// Executes Add Search Path menu command.
        /// </summary>        
        internal int AddSearchPath() {
            // Get a reference to the UIShell.
            IVsUIShell uiShell = GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (null == uiShell) {
                return VSConstants.S_FALSE;
            }
            //Create a fill in a structure that defines Browse for folder dialog
            VSBROWSEINFOW[] browseInfo = new VSBROWSEINFOW[1];
            //Dialog title
            browseInfo[0].pwzDlgTitle = DynamicProjectSR.GetString(DynamicProjectSR.SelectFolderForSearchPath);
            //Initial directory - project directory
            browseInfo[0].pwzInitialDir = _projectDir;
            //Parent window
            uiShell.GetDialogOwnerHwnd(out browseInfo[0].hwndOwner);
            //Max path length
            browseInfo[0].nMaxDirName = NativeMethods.MAX_PATH;
            //This struct size
            browseInfo[0].lStructSize = (uint)Marshal.SizeOf(typeof(VSBROWSEINFOW));
            //Memory to write selected directory to.
            //Note: this one allocates unmanaged memory, which must be freed later
            IntPtr pDirName = Marshal.AllocCoTaskMem(NativeMethods.MAX_PATH);
            browseInfo[0].pwzDirName = pDirName;
            try {
                //Show the dialog
                int hr = uiShell.GetDirectoryViaBrowseDlg(browseInfo);
                if (hr == VSConstants.OLE_E_PROMPTSAVECANCELLED) {
                    //User cancelled the dialog
                    return VSConstants.S_OK;
                }
                //Check for any failures
                ErrorHandler.ThrowOnFailure(hr);
                //Get selected directory
                string dirName = Marshal.PtrToStringAuto(browseInfo[0].pwzDirName);
                AddSearchPathEntry(dirName);
            } finally {
                //Free allocated unmanaged memory
                if (pDirName != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(pDirName);
                }
            }
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsProjectSpecificEditorMap2 Members

        public int GetSpecificEditorProperty(string mkDocument, int propid, out object result) {
            // initialize output params
            result = null;

            //Validate input
            if (string.IsNullOrEmpty(mkDocument))
                throw new ArgumentException("Was null or empty", "mkDocument");

            // Make sure that the document moniker passed to us is part of this project
            // We also don't care if it is not a dynamic language file node
            uint itemid;
            ErrorHandler.ThrowOnFailure(ParseCanonicalName(mkDocument, out itemid));
            HierarchyNode hierNode = NodeFromItemId(itemid);
            if (hierNode == null || ((hierNode as CommonFileNode) == null))
                return VSConstants.E_NOTIMPL;

            switch (propid) {
                case (int)__VSPSEPROPID.VSPSEPROPID_UseGlobalEditorByDefault:
                    // we do not want to use global editor for form files
                    result = true;
                    break;
                //case (int)__VSPSEPROPID.VSPSEPROPID_ProjectDefaultEditorName:
                //    result = "Python Form Editor";
                //    break;
            }

            return VSConstants.S_OK;
        }

        public int GetSpecificEditorType(string mkDocument, out Guid guidEditorType) {
            // Ideally we should at this point initalize a File extension to EditorFactory guid Map e.g.
            // in the registry hive so that more editors can be added without changing this part of the
            // code. Dynamic languages only make usage of one Editor Factory and therefore we will return 
            // that guid
            guidEditorType = GetEditorFactoryType().GUID;
            return VSConstants.S_OK;
        }

        public int GetSpecificLanguageService(string mkDocument, out Guid guidLanguageService) {
            guidLanguageService = Guid.Empty;
            return VSConstants.E_NOTIMPL;
        }

        public int SetSpecificEditorProperty(string mkDocument, int propid, object value) {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IVsDeferredSaveProject Members

        /// <summary>
        /// Implements deferred save support.  Enabled by unchecking Tools->Options->Solutions and Projects->Save New Projects Created.
        /// 
        /// In this mode we save the project when the user selects Save All.  We need to move all the files in the project
        /// over to the new location.
        /// </summary>
        public virtual int SaveProjectToLocation(string pszProjectFilename) {
            string oldName = Url;
            string basePath = Path.GetDirectoryName(this.FileName) + Path.DirectorySeparatorChar;
            string newName = Path.GetDirectoryName(pszProjectFilename);
            
            // we don't use RenameProjectFile because it sends the OnAfterRenameProject event too soon
            // and causes VS to think the solution has changed on disk.  We need to send it after all 
            // updates are complete.

            // save the new project to to disk
            SaveMSBuildProjectFileAs(pszProjectFilename);

            // remove all the children, saving any dirty files, and collecting the list of open files
            MoveFilesForDeferredSave(this, basePath, newName);

            _projectDir = newName;

            // save the project again w/ updated file info
            BuildProject.Save();
                
            SetProjectFileDirty(false);

            // update VS that we've changed the project
            this.OnPropertyChanged(this, (int)__VSHPROPID.VSHPROPID_Caption, 0);

            IVsUIShell shell = this.Site.GetService(typeof(SVsUIShell)) as IVsUIShell;
            IVsSolution vsSolution = (IVsSolution)this.GetService(typeof(SVsSolution));
            // Update solution
            ErrorHandler.ThrowOnFailure(vsSolution.OnAfterRenameProject((IVsProject)this, oldName, pszProjectFilename, 0));

            ErrorHandler.ThrowOnFailure(shell.RefreshPropertyBrowser(0));

            return VSConstants.S_OK;
        }

        private static string GetNewFilePathForDeferredSave(string baseOldPath, string baseNewPath, string itemPath) {
            var relativeName = itemPath.Substring(baseOldPath.Length);
            return Path.Combine(baseNewPath, relativeName);
        }

        private void MoveFilesForDeferredSave(HierarchyNode node, string basePath, string baseNewPath) {
            if (node != null) {
                for (var child = node.FirstChild; child != null; child = child.NextSibling) {
                    bool isOpen, isDirty, isOpenedByUs;
                    uint docCookie;
                    IVsPersistDocData persist;
                    var docMgr = child.GetDocumentManager();
                    if (docMgr != null) {
                        docMgr.GetDocInfo(out isOpen, out isDirty, out isOpenedByUs, out docCookie, out persist);
                        int cancelled;
                        if (isDirty) {
                            child.SaveItem(VSSAVEFLAGS.VSSAVE_Save, null, docCookie, IntPtr.Zero, out cancelled);
                        }
                                                
                        FileNode fn = child as FileNode;
                        if (fn != null) {
                            string newLoc = GetNewFilePathForDeferredSave(basePath, baseNewPath, child.Url);
                            
                            // make sure the directory is there
                            Directory.CreateDirectory(Path.GetDirectoryName(newLoc));
                            fn.RenameDocument(child.Url, newLoc);
                        }

                        FolderNode folder = child as FolderNode;
                        if (folder != null) {
                            folder.VirtualNodeName = GetNewFilePathForDeferredSave(basePath, baseNewPath, child.Url);
                        }
                    }

                    MoveFilesForDeferredSave(child, basePath, baseNewPath);
                }
            }
        }

        #endregion
    }
}
