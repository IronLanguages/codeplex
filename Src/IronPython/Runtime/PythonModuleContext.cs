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
    public sealed class PythonModuleContext : ModuleContext {
        private bool _trueDivision;
        private bool _isPythonCreatedModule;

        public bool TrueDivision {
            get {
                return _trueDivision;
            }
            set {
                _trueDivision = value;
            }
        }

        public bool IsPythonCreatedModule {
            get {
                return _isPythonCreatedModule;
            }
            set {
                _isPythonCreatedModule = value;
            }
        }

        public PythonModuleContext(ScriptModule module)
            : base(module) {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        private PythonModuleContext(PythonModuleContext context)
            : base(context) {
            _trueDivision = context._trueDivision;
            _isPythonCreatedModule = context._isPythonCreatedModule;
        }

        protected override void ModuleReloading() {
            base.ModuleReloading();
            _trueDivision = false;
        }
    }
}
