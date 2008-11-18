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

namespace Microsoft.Scripting.Runtime {

    /// <summary>
    /// Provides a list of all the members of an instance.  ie. all the keys in the 
    /// dictionary of the object. Note that it can contain objects that are not strings. 
    /// 
    /// Such keys can be added in IronPython using syntax like:
    ///     obj.__dict__[100] = someOtherObject
    /// </summary>
    public interface IMembersList {
        IList<object> GetMemberNames(CodeContext context);
    }
}
