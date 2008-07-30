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
using System.Runtime.CompilerServices;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Builder for types that derive from System.Delegate.
    /// </summary>
    public class DelegateBuilder {
        private Type _type;

        public DelegateBuilder(Type type) {
            _type = type;
        }

        [SpecialName]
        public object Call(CodeContext context, object[] args) {
            Assert.NotNull(args);
            return BinderOps.GetDelegate(context, args[0], _type);
        }
    }
}