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
using Microsoft.Scripting;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace IronPython.Runtime {
    public static partial class Symbols {
        //
        // This is only called from the properties which get symbols.
        //
        private static SymbolId MakeSymbolId(string name) {
            return SymbolTable.StringToId(name);
        }
    }
}
