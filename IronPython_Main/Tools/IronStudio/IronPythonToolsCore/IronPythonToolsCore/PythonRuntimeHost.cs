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

using System.Collections.Generic;
using System.ComponentModel.Composition;
using IronPython.Hosting;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Core;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronPythonTools {
    [Export(typeof(IPythonRuntimeHost))]
    public sealed class PythonRuntimeHost : IPythonRuntimeHost {
        private readonly IContentType _contentType;
        private readonly ScriptEngine _engine;
        private bool _enterOutliningOnOpen, _intersectMembers, _hideAdvancedMembers;

        [ImportingConstructor]
        internal PythonRuntimeHost(IContentTypeRegistryService/*!*/ contentTypeRegistryService, IFileExtensionRegistryService/*!*/ fileExtensionRegistryService) {
            _engine = Python.CreateEngine(new Dictionary<string, object> { { "NoAssemblyResolveHook", true } });
            _contentType = CoreUtils.RegisterContentType(
                contentTypeRegistryService,
                fileExtensionRegistryService,
                PythonCoreConstants.ContentType,
                new[] { CoreConstants.DlrContentTypeName },
                _engine.Setup.FileExtensions
            );   
        }

        public ScriptEngine ScriptEngine {
            get {
                return _engine;
            }
        }

        public IContentType ContentType {
            get { 
                return _contentType; 
            }
        }

        public bool EnterOutliningModeOnOpen {
            get {
                return _enterOutliningOnOpen;
            }
            set {
                _enterOutliningOnOpen = value;
            }
        }

        public bool IntersectMembers {
            get {
                return _intersectMembers;
            }
            set {
                _intersectMembers = value;
            }
        }

        public bool HideAdvancedMembers {
            get {
                return _hideAdvancedMembers;
            }
            set {
                _hideAdvancedMembers = value;
            }
        }
    }
}
