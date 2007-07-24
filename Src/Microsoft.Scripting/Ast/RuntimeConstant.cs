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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public sealed class RuntimeConstant : CompilerConstant {
        private object/*!*/ _value;

        internal RuntimeConstant(object/*!*/ value) {
            if (value == null) throw new ArgumentNullException("value");
            _value = value;
        }

        public override Type Type {
            get { return _value.GetType(); }
        }

        public override void EmitCreation(CodeGen cg) {
            throw new InvalidOperationException();
        }

        public override object Create() {
            return _value;
        }
    }
}
