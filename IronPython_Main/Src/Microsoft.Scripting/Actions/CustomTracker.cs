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
    /// A custom member tracker which enables languages to plug in arbitrary
    /// members into the lookup process.
    /// </summary>
    public abstract class CustomTracker : MemberTracker {
        protected CustomTracker() {
        }

        public sealed override TrackerTypes MemberType {
            get { return TrackerTypes.Custom; }
        }
    }
}
