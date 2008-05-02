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
using System.Text;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

using IronPython.Compiler;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    partial class PythonExtensionTypeAttribute : ExtensionTypeAttribute {
        private bool _enableDerivation;
        private Type _derivationType;

        public PythonExtensionTypeAttribute(Type extends, Type extensionType)
            : base(extends, extensionType) {
        }

        public bool EnableDerivation {
            get {
                return _enableDerivation;
            }
            set {
                _enableDerivation = value;
            }
        }

        /// <summary>
        ///  TODO: Remove me and need to have custom derivation types.
        /// </summary>
        public Type DerivationType {
            get {
                return _derivationType;
            }
            set {
                _derivationType = value;
            }
        }
    }
}
