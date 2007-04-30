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
using System.Diagnostics;
using Microsoft.Scripting;

using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Generation {

    sealed class IndexEnvironmentReference : Storage {
        private Type _type;
        private int _index;

        public IndexEnvironmentReference(int index, Type type) {
            _type = type;
            _index = index;
        }

        public override bool RequireAccessSlot {
            get { return true; }
        }

        public override Slot CreateSlot(Slot instance) {
            return new IndexSlot(instance, _index, _type);
        }
    }   
}
