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
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;

namespace Microsoft.IronStudio.Project {
    [ComVisible(true), CLSCompliant(false)]
    [Guid(CommonConstants.SearchPathsPropertiesGuid)]
    public class CommonSearchPathNodeProperties : NodeProperties {
        #region properties
		[SRCategoryAttribute(SR.Misc)]
		[LocDisplayName(SR.FolderName)]
		[SRDescriptionAttribute(SR.FolderNameDescription)]
		[AutomationBrowsable(false)]
		public string FolderName
		{
			get
			{
                return new DirectoryInfo(this.Node.Url).Name;
			}			
		}

        [SRCategoryAttribute(SR.Misc)]
        [LocDisplayName(SR.FullPath)]
        [SRDescriptionAttribute(SR.FullPathDescription)]
        [AutomationBrowsable(true)]
        public string FullPath {
            get {
                return this.Node.VirtualNodeName;
            }
        }

		#region properties - used for automation only
		[Browsable(false)]
		[AutomationBrowsable(true)]
		public string FileName
		{
			get
			{
				return this.Node.VirtualNodeName;
			}		
		}
		
		#endregion

		#endregion

		#region ctors
        public CommonSearchPathNodeProperties(HierarchyNode node)
			: base(node) { }
		#endregion		
    }    
}
