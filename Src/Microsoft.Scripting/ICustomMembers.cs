/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Scripting {
    /// <summary>
    /// This interface objects to specify how to look up members (for code like "obj.member").
    /// If an object does not implement this interface, its DynamicType is then asked to find the member.
    /// </summary>
    public interface ICustomMembers {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        bool TryGetCustomMember(CodeContext context, SymbolId name, out object value);
        bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value);
        void SetCustomMember(CodeContext context, SymbolId name, object value);
        bool DeleteCustomMember(CodeContext context, SymbolId name);

        /// <returns>The returned List contains all the members of the instance. ie. all the keys in the 
        /// dictionary of the object. Note that it can contain objects that are not strings. Such keys can be
        /// added using syntax like:
        ///     obj.__dict__[100] = someOtherObject
        /// </returns>
        IList<object> GetCustomMemberNames(CodeContext context);
        IDictionary<object, object> GetCustomMemberDictionary(CodeContext context);
    }
}
