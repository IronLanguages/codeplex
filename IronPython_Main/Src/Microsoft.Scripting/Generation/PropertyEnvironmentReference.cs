/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Reflection;

namespace Microsoft.Scripting.Generation {    
    class PropertyEnvironmentReference : Storage {
        private PropertyInfo _property;
        private Type _type;

        public PropertyEnvironmentReference(PropertyInfo property, Type type) {
            _property = property;
            _type = type;
        }

        public override bool RequireAccessSlot {
            get { return true; }
        }

        public override Slot CreateSlot(Slot instance) {
            Slot slot = new PropertySlot(instance, _property);
            if (_type != _property.PropertyType) {
                slot = new CastSlot(slot, _type);
            }
            return slot;
        }
    }    
}
