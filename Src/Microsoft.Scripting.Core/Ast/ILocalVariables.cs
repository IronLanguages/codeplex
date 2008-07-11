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

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions {

    /// <summary>
    /// Provides read/write access to variables in a scope.
    /// Created by LocalScopeExpression
    /// 
    /// TODO: review public API
    /// </summary>
    public interface ILocalVariables : IList<object> {
        ReadOnlyCollection<string> Names { get; }
    }
}
