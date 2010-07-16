/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

namespace IronPython.Runtime {
    public abstract class SiteLocalStorage {
    }

    /// <summary>
    /// Provides storage which is flowed into a callers site.  The same storage object is 
    /// flowed for multiple calls enabling the callee to cache data that can be re-used
    /// across multiple calls.
    /// 
    /// Data is a public field so that this works properly with DynamicSite's as the reference
    /// type (and EnsureInitialize)
    /// </summary>
    public class SiteLocalStorage<T> : SiteLocalStorage {
        public T Data;
    }
}
