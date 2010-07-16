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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;

namespace Microsoft.IronStudio.Project {
    public class CommonNonCodeFileNode : CommonFileNode {
        public CommonNonCodeFileNode(CommonProjectNode root, ProjectElement e)
            : base(root, e) {
        }


        /// <summary>
        /// Open a file depending on the SubType property associated with the file item in the project file
        /// </summary>
        protected override void DoDefaultAction() {
            FileDocumentManager manager = this.GetDocumentManager() as FileDocumentManager;
            Debug.Assert(manager != null, "Could not get the FileDocumentManager");

            Guid viewGuid = Guid.Empty;
            IVsWindowFrame frame;
            manager.Open(false, false, viewGuid, out frame, WindowFrameShowAction.Show);
        }

    }
}
