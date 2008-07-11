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
using System.Diagnostics;
using System.Reflection;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Used as the value for the RuntimeHelpers.GetDelegate method caching system
    /// </summary>
    internal sealed class DelegateInfo {
        private readonly MethodInfo _method;
        private readonly object[] _constants;

        internal DelegateInfo(MethodInfo method, object[] constants) {
            Assert.NotNull(method, constants);

            _method = method;
            _constants = constants;
        }

        internal Delegate CreateDelegate(Type delegateType, object target) {
            Assert.NotNull(delegateType, target);

            object[] clone = (object[])_constants.Clone();

            Debug.Assert(clone[0] == DelegateSignatureInfo.TargetPlaceHolder);

            clone[0] = target;
            return ReflectionUtils.CreateDelegate(_method, delegateType, clone);
        }
    }
}
