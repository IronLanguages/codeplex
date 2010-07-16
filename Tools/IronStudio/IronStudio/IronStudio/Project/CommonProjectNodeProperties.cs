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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;

namespace Microsoft.IronStudio.Project {

    [ComVisible(true)]
    [CLSCompliant(false)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid(CommonConstants.ProjectNodePropertiesGuid)]
    public class CommonProjectNodeProperties : ProjectNodeProperties {

        public CommonProjectNodeProperties(ProjectNode node)
            : base(node) {
        }

        #region properties
        /// <summary>
        /// Returns/Sets the StartupFile project property
        /// </summary>
        [Browsable(false)]
        public string StartupFile {
            get {
                return this.Node.ProjectMgr.GetProjectProperty(CommonConstants.StartupFile, true);
            }            
        }
                
        /// <summary>
        /// Returns/Sets the WorkingDirectory project property
        /// </summary>
        [Browsable(false)]
        public string SearchPath {
            get {
                return this.Node.ProjectMgr.GetProjectProperty(CommonConstants.SearchPath, true);
            }
        }

        //We don't need this property, but still have to provide it, otherwise
        //Add New Item wizard (which seems to be unmanaged) fails.
        [Browsable(false)]
        public string RootNamespace {
            get {
                return "";
            }
            set {
                //Do nothing
            }
        }
        #endregion
    }
}
