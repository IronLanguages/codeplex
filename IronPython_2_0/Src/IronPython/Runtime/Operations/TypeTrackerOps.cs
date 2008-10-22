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
using System; using Microsoft;
using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations {
    public static class TypeTrackerOps {
        [SpecialName, PropertyMethod]
        public static IDictionary Get__dict__(CodeContext context, TypeTracker self) {
            return new DictProxy(DynamicHelpers.GetPythonTypeFromType(self.Type));
        }
    }
}
