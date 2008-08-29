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
        private string _typeName;
        private string _displayName;
        private readonly List<string> _names;
        private readonly List<string> _fileExtensions;
        private readonly Dictionary<string, object> _options;

        public string TypeName {
            get { return _typeName; }
            set {
                ContractUtils.RequiresNotEmpty(value, "value");
                _typeName = value;
            }
        }

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
        public List<string> Names {
            get { return _names; }
        }

        /// <remarks>
        /// Case-insensitive file extension, optionally starts with a dot.
        /// </remarks>
        public List<string> FileExtensions {
            get { return _fileExtensions; }
        }

        /// <remarks>
        /// Option names are case-sensitive.
        /// </remarks>
        public Dictionary<string, object> Options {
            get { return _options; }
        }

        public LanguageSetup(string typeName, string displayName)
            : this(typeName, displayName, ArrayUtils.EmptyStrings, ArrayUtils.EmptyStrings) {
        }

        public LanguageSetup(string typeName, string displayName, IEnumerable<string> names, IEnumerable<string> fileExtensions) {
            ContractUtils.RequiresNotEmpty(typeName, "typeName");
            ContractUtils.RequiresNotNull(displayName, "displayName");
            ContractUtils.RequiresNotNull(names, "names");
            ContractUtils.RequiresNotNull(fileExtensions, "fileExtensions");

            _typeName = typeName;
            _displayName = displayName;
            _names = new List<string>(names);
            _fileExtensions = new List<string>(fileExtensions);
            _options = new Dictionary<string, object>();
        }
    }
}
