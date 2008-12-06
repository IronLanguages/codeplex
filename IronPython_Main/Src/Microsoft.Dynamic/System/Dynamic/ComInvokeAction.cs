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


#if !SILVERLIGHT

using System.Collections.Generic;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting;

namespace Microsoft.Scripting {
    class ComInvokeAction : InvokeBinder {
        public override object CacheIdentity {
            get { return this; }
        }

        internal ComInvokeAction(params ArgumentInfo[] arguments)
            : base(arguments) {
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            return base.Equals(obj as ComInvokeAction);
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
            return errorSuggestion ?? DynamicMetaObject.CreateThrow(target, args, typeof(NotSupportedException), "Cannot perform call");
        }
    }
}

#endif
