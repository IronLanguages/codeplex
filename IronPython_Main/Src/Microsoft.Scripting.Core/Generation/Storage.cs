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
    /// Represents storage for a variable. It knows how to access the variable
    /// given a pointer to its environment.
    /// 
    /// In the canonical case of an environment slot, this object will hold
    /// on to the tuple index of this variable. See EnvironmentStorage
    /// </summary>
    internal abstract class Storage {
        internal abstract Slot CreateSlot(Slot instance);
    }
}
