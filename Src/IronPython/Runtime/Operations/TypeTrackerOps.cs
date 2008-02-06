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

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

[assembly: PythonExtensionType(typeof(TypeTracker), typeof(TypeTrackerOps))]
namespace IronPython.Runtime.Operations {
    public static class TypeTrackerOps {
        [PropertyMethod]
        public static IDictionary<object, object> Get__dict__(CodeContext context, TypeTracker self) {
            return new PythonDictionary(((ICustomMembers)DynamicHelpers.GetPythonTypeFromType(self.Type)).GetCustomMemberDictionary(context));
        }
    }
}
