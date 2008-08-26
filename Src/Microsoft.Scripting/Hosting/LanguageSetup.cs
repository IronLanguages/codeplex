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
using System.Reflection;

namespace Microsoft.Scripting.Hosting {
    [Serializable]
    public sealed class LanguageSetup {
        private string _displayName;
        private IList<string> _names;
        private IList<string> _fileExtensions;
        private IDictionary<string, object> _options;

        public string DisplayName {
            get { return _displayName; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _displayName = value;
            }
        }

        /// <remarks>
        /// Case-insensitive language names.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<string> Names {
            get {
                if (_names == null) {
                    _names = new List<string>();
                }
                return _names;
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _names = value;
            }
        }

        /// <remarks>
        /// Case-insensitive file extension, optionally starts with a dot.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<string> FileExtensions {
            get {
                if (_fileExtensions == null) {
                    _fileExtensions = new List<string>();
                }
                return _fileExtensions; 
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _fileExtensions = value;
            }
        }

        /// <remarks>
        /// Option names are case-sensitive.
        /// </remarks>
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
        }

        public LanguageSetup(string displayName, IList<string> names, IList<string> fileExtensions) {
            ContractUtils.RequiresNotNull(displayName, "displayName");
            ContractUtils.RequiresNotNull(names, "names");
            ContractUtils.RequiresNotNull(fileExtensions, "fileExtensions");

            _displayName = displayName;
            _names = names;
            _fileExtensions = fileExtensions;
        }
    }
}
