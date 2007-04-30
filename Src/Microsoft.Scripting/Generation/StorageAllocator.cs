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
using System.Reflection;
using System.Diagnostics;
using Microsoft.Scripting;

namespace Microsoft.Scripting.Internal.Generation {
    /// <summary>
    /// Represents a namespace. Slots can be created, look up by name, or relocated into
    /// this namespace.
    /// </summary>
    public abstract class StorageAllocator {
        public virtual void PrepareForEmit(CodeGen cg) {
        }

        // TODO: change the parameter to take Variable !!!
        public abstract Storage AllocateStorage(SymbolId name, Type type);
    }
}
