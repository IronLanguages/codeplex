/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    public class CompiledType : ReflectedType {
        public static CompiledType GetTypeForType(Type t) {
            return new CompiledType(t);
        }

        public CompiledType(Type t)
            : base(t) {                      
        }

        protected override void RawSetSlot(SymbolId name, object value) {            
            dict[name] = value;
        }

        public override bool TryGetAttr(ICallerContext context, SymbolId name, out object ret) {
            return base.TryGetAttr(context, name, out ret);
        }
        
        public override void SetAttr(ICallerContext context, object self, SymbolId name, object value) {
            object slot;
            bool success = TryGetSlot(context, name, out slot);
            if (success) {
                success = Ops.SetDescriptor(slot, self, value);
            }

            if (!success) {
                // otherwise update the instance
                IAttributesDictionary dict = ((ISuperDynamicObject)self).GetDict();
                dict[name] = value;
            }
        }

        public override bool TryGetAttr(ICallerContext context, object self, SymbolId name, out object ret) {
            if (base.TryGetAttr(context, self, name, out ret)) {
                return true;
            }

            // This will force creation of the instances dict
            IAttributesDictionary dict = ((ISuperDynamicObject)self).GetDict();
            return dict.TryGetValue(name, out ret);
        }

        public override bool IsPythonType {
            get {
                return true;
            }
        }
    }
}
