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
using System.Reflection.Emit;

namespace Microsoft.Scripting.Generation {    
    public class FunctionEnvironmentSlot : EnvironmentSlot {
        private Type _storageType;

        public FunctionEnvironmentSlot(Slot storage, Type storageType)
            : base(storage) {
            _storageType = storageType;
        }

        public override void EmitGetDictionary(CodeGen cg) {
            EmitGet(cg);
            if (_storageType == typeof(object[])) {
                cg.Emit(OpCodes.Ldc_I4_0);
                cg.Emit(OpCodes.Ldelem_Ref);
                cg.Emit(OpCodes.Castclass, typeof(FunctionEnvironmentNDictionary));
            } else {
                cg.EmitPropertyGet(_storageType, "Item000");
            }
        }        
    }
}
