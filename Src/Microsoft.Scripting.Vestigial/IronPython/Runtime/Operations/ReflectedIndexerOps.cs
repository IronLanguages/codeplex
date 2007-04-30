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

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

[assembly: PythonExtensionType(typeof(ReflectedIndexer), typeof(ReflectedIndexerOps))]
namespace IronPython.Runtime.Operations {
    public static class ReflectedIndexerOps {
        [PythonName("__get__")]
        public static object GetAttribute(ReflectedIndexer self, object instance, object owner) {
            object val;
            self.TryGetValue(DefaultContext.Default, instance, owner as DynamicType, out val);
            return val;
        }

    }
}
