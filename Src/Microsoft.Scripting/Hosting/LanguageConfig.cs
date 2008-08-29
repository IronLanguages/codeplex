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
using System.Security.Permissions;
using Microsoft.Scripting.Utils;
using System.Collections.ObjectModel;

namespace Microsoft.Scripting.Hosting {
    [Serializable]
    public sealed class LanguageConfig {
        private readonly string _typeName;
        private readonly string _displayName;
        private readonly ReadOnlyCollection<string> _names;
        private readonly ReadOnlyCollection<string> _fileExtensions;

        internal LanguageConfig(string typeName, string displayName, string[] simpleNames, string[] fileExtensions) {
            _typeName = typeName;
            _displayName = displayName;
            _names = new ReadOnlyCollection<string>(simpleNames);
            _fileExtensions = new ReadOnlyCollection<string>(fileExtensions);
        }

        /// <summary>
        /// Fully qualified type name of the language provider class
        /// </summary>
        public string TypeName {
            get { return _typeName; }
        }

        /// <summary>
        /// Display name of the language
        /// </summary>
        public string DisplayName {
            get { return _displayName; }
        }

        /// <summary>
        /// Names of the language
        /// </summary>
        public IList<string> Names {
            get { return _names; }
        }

        /// <summary>
        /// File extensions associated with this language
        /// </summary>
        public IList<string> FileExtensions {
            get { return _fileExtensions; }
        }
    }
}
