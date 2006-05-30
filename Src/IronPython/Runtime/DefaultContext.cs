/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using IronPython.Compiler;

namespace IronPython.Runtime {
    class DefaultContext : ICallerContext{
        public static DefaultContext Default = new DefaultContext(CallerContextFlags.None);
        public static DefaultContext DefaultCLS = new DefaultContext(CallerContextFlags.ShowCls);

        CallerContextFlags flags;
        
        public DefaultContext(CallerContextFlags contextFlags) {
            flags = contextFlags;
        }

        #region ICallerContext Members

        public PythonModule Module {
            get { return null; }
        }

        public SystemState SystemState {
            get {
                throw new InvalidOperationException("SystemState on default context is not supported");
            }
        }

        public object Locals {
            get { return null; }
        }

        public object Globals {
            get { return null; }
        }

        public object GetStaticData(int index) {
            return null;
        }

        public CallerContextFlags ContextFlags {
            get { return flags; }
            set { }
        }

        public bool TrueDivision {
            get { return false; }
            set { }
        }

        public CompilerContext CreateCompilerContext() {
            return new CompilerContext();
        }

        #endregion
    }


    public class EngineContext : ICallerContext {
        SystemState systemState;
        PythonModule module;

        public EngineContext() {
            systemState = new SystemState();
            module = new PythonModule("__main__", new Dict(), systemState);
        }

        public void ResetModule(PythonModule mod) {
            module = mod;
        }

        #region ICallerContext Members

        public PythonModule Module {
            get { return module; }
        }

        public SystemState SystemState {
            get {
                return systemState;
            }
        }

        public object Locals {
            get { return ((ICallerContext)module).Locals; }
        }

        public object Globals {
            get { return ((ICallerContext)module).Globals; }
        }

        public object GetStaticData(int index) {
            return ((ICallerContext)module).GetStaticData(index);
        }

        public CallerContextFlags ContextFlags {
            get {
                return ((ICallerContext)module).ContextFlags;
            }
            set {
                ((ICallerContext)module).ContextFlags = value;
            }
        }

        public bool TrueDivision {
            get { return ((ICallerContext)module).TrueDivision; }
            set { ((ICallerContext)module).TrueDivision = value; }
        }

        public CompilerContext CreateCompilerContext() {
            return ((ICallerContext)module).CreateCompilerContext();
        }

        #endregion
    }
}
