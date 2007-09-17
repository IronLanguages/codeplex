/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
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
using System.Reflection.Emit;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Compiler.Generation;
using IronPython.Compiler;

[assembly: PythonExtensionType(typeof(Delegate), typeof(DelegateOps))]
namespace IronPython.Runtime.Types {
    public static class DelegateOps {
        [StaticExtensionMethod("__new__")]
        public static object MakeNew(DynamicType type, object function) {
            if (type == null) throw PythonOps.TypeError("expected type for 1st param, got {0}", DynamicTypeOps.GetName(type));

            return DynamicHelpers.GetDelegate(function, type.UnderlyingSystemType, null);
        }
    }    
}
