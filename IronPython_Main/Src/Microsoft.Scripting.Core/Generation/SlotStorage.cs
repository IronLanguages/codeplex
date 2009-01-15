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


namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Represents variable storage that targets a fixed slot
    /// Examples include CLR locals, arguments, and module global wrappers
    /// </summary>
    internal sealed class SlotStorage : Storage {
        private Slot _slot;

        internal SlotStorage(Slot slot) {
            _slot = slot;
        }

        internal override Slot CreateSlot(Slot instance) {
            return _slot;
        }
    }
}
