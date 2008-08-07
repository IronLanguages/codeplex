/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    [Serializable]
    public sealed class LanguageSetup {
        private string _displayName;
        private readonly IList<string> _ids;
        private readonly IList<string> _fileExtensions;
        private IDictionary<string, object> _options;

        public string DisplayName {
            get { return _displayName; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _displayName = value;
            }
        }

        public IList<string> Ids {
            get { return _ids; }
        }

        // optionally start with a dot
        public IList<string> FileExtensions {
            get { return _fileExtensions; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IDictionary<string, object> Options {
            get {
                if (_options == null) {
                    _options = new Dictionary<string, object>();
                }
                return _options;
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _options = value;
            }
        }

        public bool HasOptions {
            get { return _options != null; }
        }

        public LanguageSetup(string displayName) {
            ContractUtils.RequiresNotNull(displayName, "displayName");
            _displayName = displayName;
            _ids = new List<string>();
            _fileExtensions = new List<string>();
        }

        public LanguageSetup(string displayName, IList<string> ids, IList<string> fileExtensions) {
            ContractUtils.RequiresNotNull(displayName, "displayName");
            ContractUtils.RequiresNotNull(ids, "ids");
            ContractUtils.RequiresNotNull(fileExtensions, "fileExtensions");

            _displayName = displayName;
            _ids = ids;
            _fileExtensions = fileExtensions;
        }
    }
}
