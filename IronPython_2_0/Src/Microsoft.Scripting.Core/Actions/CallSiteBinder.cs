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


namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Class responsible for binding dynamic operations on the dynamic site.
    /// </summary>
    public abstract class CallSiteBinder {
        protected CallSiteBinder() {
        }

        /// <summary>
        /// Key used for the DLR caching
        /// </summary>
        public abstract object HashCookie { get; }

        /// <summary>
        /// The bind call to produce the binding.
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="args">Array of arguments to the call</param>
        /// <returns>New rule.</returns>
        public abstract Rule<T> Bind<T>(object[] args) where T : class;
    }
}
