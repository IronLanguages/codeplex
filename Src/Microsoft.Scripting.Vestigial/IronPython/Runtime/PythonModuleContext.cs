/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using Microsoft.Scripting;
using System;

namespace IronPython.Runtime {
    public sealed class PythonModuleContext : ICloneable {
        private bool _showCls;
        private bool _trueDivision;
        
        public bool ShowCls {
            get {
                return _showCls;
            }
            set {
                _showCls = value;
            }
        }

        public bool TrueDivision {
            get {
                return _trueDivision;
            }
            set {
                _trueDivision = value;
            }
        }

        public PythonModuleContext() {
        }

        public object Clone() {
            return MemberwiseClone();
        }
    }
}
