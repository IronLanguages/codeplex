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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Indicates that a MetaObject is already representing a restricted type.  Useful
    /// when we're already restricted to a known type but this isn't captured in
    /// the type info (e.g. the type is not sealed).
    /// </summary>
    public interface IRestrictedMetaObject {
        DynamicMetaObject Restrict(Type type);
    }
}
