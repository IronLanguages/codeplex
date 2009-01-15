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
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Allocates storage for a variable
    /// 
    /// This is called for each variable that is defined on a given lambda.
    /// It returns a "Storage" object, which becomes associated with that
    /// variable. The "Storage" object knows how to create reference slots
    /// for itself, given the closure access pointer for this frame.
    /// </summary>
    internal abstract class StorageAllocator {
        internal virtual void PrepareForEmit(LambdaCompiler cg) {}
        internal abstract Storage AllocateStorage(Expression variable);
    }
}
