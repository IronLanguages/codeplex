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
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonExtensionType(typeof(ClrModule), typeof(ClrModuleOps))]
namespace IronPython.Runtime.Operations {
    public static class ClrModuleOps {
        #region Runtime Type Checking support

        [PythonName("accepts")]
        public static object Accepts(ClrModule self, params object[] types) {
            return new ArgChecker(types);
        }

        [PythonName("returns")]
        public static object Returns(ClrModule self, object type) {
            return new ReturnChecker(type);
        }

        [PythonName("Self")]
        public static object Self(ClrModule self) {
            return null;
        }
        #endregion

    }
}
