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

namespace IronPython.Runtime.Calls {
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

        public IAttributesDictionary Globals {
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
}
