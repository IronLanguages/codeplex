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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Provides documentation for a single parameter.
    /// </summary>
    [Serializable]
    public class ParameterDoc {
        private readonly string _name, _typeName;
        private readonly ParameterFlags _flags;

        public ParameterDoc(string name)
            : this(name, ParameterFlags.None) {
        }

        public ParameterDoc(string name, ParameterFlags paramFlags)
            : this(name, null, paramFlags) {
        }

        public ParameterDoc(string name, string typeName)
            : this(name, typeName, ParameterFlags.None) {
        }

        public ParameterDoc(string name, string typeName, ParameterFlags paramFlags) {
            ContractUtils.RequiresNotNull(name, "name");

            _name = name;
            _flags = paramFlags;
            _typeName = typeName;
        }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// The type name of the parameter or null if no type information is available.
        /// </summary>
        public string TypeName {
            get {
                return _typeName;
            }
        }

        /// <summary>
        /// Provides addition information about the parameter such as if it's a parameter array.
        /// </summary>
        public ParameterFlags Flags {
            get {
                return _flags;
            }
        }
    }

}
