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
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {

    /// <summary>
    /// Base class for all other slot factories.  Supports creating either strongly typed
    /// slots or slots that are always of type object.
    /// </summary>
    abstract class SlotFactory {
        /// <summary>
        /// Overriden by the base type.  Creates a new slot of the given name and type.  Only called once for each name.
        /// </summary>
        public abstract Slot CreateSlot(SymbolId name, Type type);

        /// <summary>
        /// Called before emitting code into the specified Compiler.  Provides an opportunity to setup any
        /// method-local state for the slot factory.
        /// </summary>
        /// <param name="cg"></param>
        public virtual void PrepareForEmit(LambdaCompiler cg) {
        }
    }
}
