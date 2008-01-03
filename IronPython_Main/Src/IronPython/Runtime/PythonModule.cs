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

using Microsoft.Scripting;
using System;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime {
    [Flags]
    public enum ModuleOptions {
        None = 0,
        PublishModule = 1,
        TrueDivision = 2,
        ShowClsMethods = 4,
        Optimized = 8,
        Initialize = 16
    }

    public class PythonModule : ScopeExtension {
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

        internal PythonModule(Scope scope)
            : base(scope) {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        protected PythonModule(PythonModule module)
            : base(module) {
            _trueDivision = module._trueDivision;
            _isPythonCreatedModule = module._isPythonCreatedModule;
        }

        protected override void ModuleReloading() {
            base.ModuleReloading();
            _trueDivision = false;
        }

        internal void SetName(object value) {
            Scope.SetName(Symbols.Name, value); // TODO: set in Python specific dict
        }

        internal object GetName() {
            object result;
            Scope.TryLookupName(DefaultContext.DefaultPythonContext, Symbols.Name, out result);
            return result;
        }

        internal void SetFile(object value) {
            Scope.SetName(Symbols.File, value); // TODO: set in Python specific dict
        }

        internal object GetFile() {
            object result;
            Scope.TryLookupName(DefaultContext.DefaultPythonContext, Symbols.File, out result);
            return result;
        }

        internal PythonModule/*!*/ Clone() {
            return (PythonModule)this.MemberwiseClone();
        }
    }
}
