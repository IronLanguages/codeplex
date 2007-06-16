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
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;

namespace Microsoft.Scripting.Generation {
    public class DelegateConstant : CompilerConstant {
        private Delegate _target;

        public DelegateConstant(Delegate target) {
            _target = target;
        }

        public override Type Type {
            get { return _target.GetType(); }
        }

        public override void EmitCreation(CodeGen cg) {
            Debug.Assert(!(_target.Method is DynamicMethod));

            ConstructorInfo ci = _target.GetType().GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
            
            cg.EmitNull();                                  
            cg.Emit(OpCodes.Ldftn, _target.Method);         
            cg.Emit(OpCodes.Newobj, ci);
        }

        public override object Create() {
            return _target;
        }
    }
}
