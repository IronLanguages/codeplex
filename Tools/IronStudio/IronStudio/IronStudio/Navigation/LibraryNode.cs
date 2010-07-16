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
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.IronStudio.Navigation {

    /// <summary>
    /// Single node inside the tree of the libraries in the object browser or class view.
    /// </summary>
    public class LibraryNode : SimpleObjectList<LibraryNode>, IVsNavInfoNode, ISimpleObject {
        private string _name;
        private LibraryNodeCapabilities _capabilities;
        private readonly LibraryNodeType _type;
        private readonly CommandID _contextMenuID;
        private readonly string _tooltip;
        private readonly Dictionary<LibraryNodeType, LibraryNode> _filteredView;

        public LibraryNode(string name)
            : this(name, LibraryNodeType.None, LibraryNodeCapabilities.None, null) { }

        public LibraryNode(string name, LibraryNodeType type)
            : this(name, type, LibraryNodeCapabilities.None, null) { }

        public LibraryNode(string name, LibraryNodeType type, LibraryNodeCapabilities capabilities, CommandID contextMenuID) {
            _capabilities = capabilities;
            _contextMenuID = contextMenuID;
            _name = name;
            _tooltip = name;
            _type = type;
            _filteredView = new Dictionary<LibraryNodeType, LibraryNode>();
        }

        public LibraryNode(LibraryNode node) {
            _capabilities = node._capabilities;
            _contextMenuID = node._contextMenuID;
            _name = node._name;
            _tooltip = node._tooltip;
            _type = node._type;
            Children.AddRange(node.Children);
            _filteredView = new Dictionary<LibraryNodeType, LibraryNode>();
        }

        protected void SetCapabilityFlag(LibraryNodeCapabilities flag, bool value) {
            if (value) {
                _capabilities |= flag;
            } else {
                _capabilities &= ~flag;
            }
        }

        /// <summary>
        /// Get or Set if the node can be deleted.
        /// </summary>
        public bool CanDelete {
            get { return (0 != (_capabilities & LibraryNodeCapabilities.AllowDelete)); }
            set { SetCapabilityFlag(LibraryNodeCapabilities.AllowDelete, value); }
        }

        /// <summary>
        /// Get or Set if the node can be associated with some source code.
        /// </summary>
        public bool CanGoToSource {
            get { return (0 != (_capabilities & LibraryNodeCapabilities.HasSourceContext)); }
            set { SetCapabilityFlag(LibraryNodeCapabilities.HasSourceContext, value); }
        }

        /// <summary>
        /// Get or Set if the node can be renamed.
        /// </summary>
        public bool CanRename {
            get { return (0 != (_capabilities & LibraryNodeCapabilities.AllowRename)); }
            set { SetCapabilityFlag(LibraryNodeCapabilities.AllowRename, value); }
        }

        /// <summary>
        /// 
        /// </summary>

        public override uint Capabilities { get { return (uint)_capabilities; } }

        public string TooltipText {
            get { return _tooltip; }
        }

        internal void AddNode(LibraryNode node) {
            lock (Children) {
                Children.Add(node);
            }
            Update();
        }

        internal void RemoveNode(LibraryNode node) {
            lock (Children) {
                Children.Remove(node);
            }
            Update();
        }

        public virtual object BrowseObject {
            get { return null; }
        }

        public override uint CategoryField(LIB_CATEGORY category) {
            uint fieldValue = 0;
            switch (category) {
                case LIB_CATEGORY.LC_LISTTYPE: {
                        LibraryNodeType subTypes = LibraryNodeType.None;
                        foreach (LibraryNode node in Children) {
                            subTypes |= node._type;
                        }
                        fieldValue = (uint)subTypes;
                    }
                    break;
                case (LIB_CATEGORY)_LIB_CATEGORY2.LC_HIERARCHYTYPE:
                    fieldValue = (uint)_LIBCAT_HIERARCHYTYPE.LCHT_UNKNOWN;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return fieldValue;
        }

        protected virtual LibraryNode Clone() {
            return new LibraryNode(this);
        }

        /// <summary>
        /// Performs the operations needed to delete this node.
        /// </summary>
        public virtual void Delete() {
        }

        /// <summary>
        /// Perform a Drag and Drop operation on this node.
        /// </summary>
        public virtual void DoDragDrop(OleDataObject dataObject, uint keyState, uint effect) {
        }

        public virtual uint EnumClipboardFormats(_VSOBJCFFLAGS flags, VSOBJCLIPFORMAT[] formats) {
            return 0;
        }

        public virtual void FillDescription(_VSOBJDESCOPTIONS flags, IVsObjectBrowserDescription3 description) {
            description.ClearDescriptionText();            
            description.AddDescriptionText3(_name, VSOBDESCRIPTIONSECTION.OBDS_NAME, null);
        }

        public IVsSimpleObjectList2 FilterView(uint filterType) {
            var libraryNodeType = (LibraryNodeType)filterType;
            LibraryNode filtered = null;
            if (_filteredView.TryGetValue(libraryNodeType, out filtered)) {
                return filtered as IVsSimpleObjectList2;
            }
            filtered = this.Clone();
            for (int i = 0; i < filtered.Children.Count; ) {
                if (0 == (filtered.Children[i]._type & libraryNodeType)) {
                    filtered.Children.RemoveAt(i);
                } else {
                    i += 1;
                }
            }
            _filteredView.Add(libraryNodeType, filtered);
            return filtered as IVsSimpleObjectList2;
        }

        public virtual void GotoSource(VSOBJGOTOSRCTYPE gotoType) {
            // Do nothing.
        }

        public string Name {
            get { return _name; }
        }

        public virtual string GetTextRepresentation(VSTREETEXTOPTIONS options) {
            return Name;
        }

        public LibraryNodeType NodeType {
            get { return _type; }
        }

        /// <summary>
        /// Finds the source files associated with this node.
        /// </summary>
        /// <param name="hierarchy">The hierarchy containing the items.</param>
        /// <param name="itemId">The item id of the item.</param>
        /// <param name="itemsCount">Number of items.</param>
        public virtual void SourceItems(out IVsHierarchy hierarchy, out uint itemId, out uint itemsCount) {
            hierarchy = null;
            itemId = 0;
            itemsCount = 0;
        }

        public virtual void Rename(string newName, uint flags) {
            this._name = newName;
        }

        public virtual string UniqueName {
            get { return Name; }
        }
        
        public CommandID ContextMenuID {
            get {
                return _contextMenuID;
            }
        }

        public virtual StandardGlyphGroup GlyphType {
            get {
                return StandardGlyphGroup.GlyphGroupModule;
            }
        }

        public VSTREEDISPLAYDATA DisplayData {
            get {
                var res = new VSTREEDISPLAYDATA();
                res.Image = res.SelectedImage = (ushort)GlyphType;
                return res;
            }
        }
        
        #region IVsNavInfoNode Members

        int IVsNavInfoNode.get_Name(out string pbstrName) {
            pbstrName = UniqueName;
            return VSConstants.S_OK;
        }

        int IVsNavInfoNode.get_Type(out uint pllt) {
            pllt = (uint)_type;
            return VSConstants.S_OK;
        }

        #endregion

        /// <summary>
        /// Enumeration of the capabilities of a node. It is possible to combine different values
        /// to support more capabilities.
        /// This enumeration is a copy of _LIB_LISTCAPABILITIES with the Flags attribute set.
        /// </summary>
        [Flags()]
        public enum LibraryNodeCapabilities {
            None = _LIB_LISTCAPABILITIES.LLC_NONE,
            HasBrowseObject = _LIB_LISTCAPABILITIES.LLC_HASBROWSEOBJ,
            HasDescriptionPane = _LIB_LISTCAPABILITIES.LLC_HASDESCPANE,
            HasSourceContext = _LIB_LISTCAPABILITIES.LLC_HASSOURCECONTEXT,
            HasCommands = _LIB_LISTCAPABILITIES.LLC_HASCOMMANDS,
            AllowDragDrop = _LIB_LISTCAPABILITIES.LLC_ALLOWDRAGDROP,
            AllowRename = _LIB_LISTCAPABILITIES.LLC_ALLOWRENAME,
            AllowDelete = _LIB_LISTCAPABILITIES.LLC_ALLOWDELETE,
            AllowSourceControl = _LIB_LISTCAPABILITIES.LLC_ALLOWSCCOPS,
        }

        /// <summary>
        /// Enumeration of the possible types of node. The type of a node can be the combination
        /// of one of more of these values.
        /// This is actually a copy of the _LIB_LISTTYPE enumeration with the difference that the
        /// Flags attribute is set so that it is possible to specify more than one value.
        /// </summary>
        [Flags()]
        public enum LibraryNodeType {
            None = 0,
            Hierarchy = _LIB_LISTTYPE.LLT_HIERARCHY,
            Namespaces = _LIB_LISTTYPE.LLT_NAMESPACES,
            Classes = _LIB_LISTTYPE.LLT_CLASSES,
            Members = _LIB_LISTTYPE.LLT_MEMBERS,
            Package = _LIB_LISTTYPE.LLT_PACKAGE,
            PhysicalContainer = _LIB_LISTTYPE.LLT_PHYSICALCONTAINERS,
            Containment = _LIB_LISTTYPE.LLT_CONTAINMENT,
            ContainedBy = _LIB_LISTTYPE.LLT_CONTAINEDBY,
            UsesClasses = _LIB_LISTTYPE.LLT_USESCLASSES,
            UsedByClasses = _LIB_LISTTYPE.LLT_USEDBYCLASSES,
            NestedClasses = _LIB_LISTTYPE.LLT_NESTEDCLASSES,
            InheritedInterface = _LIB_LISTTYPE.LLT_INHERITEDINTERFACES,
            InterfaceUsedByClasses = _LIB_LISTTYPE.LLT_INTERFACEUSEDBYCLASSES,
            Definitions = _LIB_LISTTYPE.LLT_DEFINITIONS,
            References = _LIB_LISTTYPE.LLT_REFERENCES,
            DeferExpansion = _LIB_LISTTYPE.LLT_DEFEREXPANSION,
        }
    }
}
