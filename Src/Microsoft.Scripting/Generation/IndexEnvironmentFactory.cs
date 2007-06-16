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
using System.Reflection.Emit;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Scripting;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Represents an environment factory that is backed by an object array.
    /// 
    /// The user exposed object type is FunctionEnvironmentNDictionary.  Used for environments with more than 128 
    /// members.
    /// </summary>
    class IndexEnvironmentFactory : EnvironmentFactory {
        private int size;
        private int index = 1;  // first index reserved for storing our dictionary

        public IndexEnvironmentFactory(int size) {
            this.size = size;
        }

        public override Type EnvironmentType {
            get {
                return typeof(FunctionEnvironmentNDictionary);
            }
        }

        public override Type StorageType {
            get {
                return typeof(object[]);
            }
        }

        public override Storage MakeEnvironmentReference(SymbolId name, Type type) {
            if (index < size) {
                return new IndexEnvironmentReference(index++, type);
            } else {
                throw new InvalidOperationException("not enough environment references available");
            }
        }

        public override void EmitGetStorageFromContext(CodeGen cg) {
            cg.EmitCodeContext();
            cg.EmitCall(typeof(RuntimeHelpers), "GetLocalDictionary");
            cg.Emit(OpCodes.Castclass, EnvironmentType);
            cg.EmitPropertyGet(typeof(FunctionEnvironmentNDictionary), "EnvironmentValues");
        }

        public override EnvironmentSlot CreateEnvironmentSlot(CodeGen cg) {
            return new FunctionEnvironmentSlot(cg.GetNamedLocal(StorageType, "$environment"), StorageType);
        }

        public override void EmitStorage(CodeGen cg) {            
            cg.EmitInt(size);
            cg.Emit(OpCodes.Newarr, typeof(object));

        }
        public override void EmitNewEnvironment(CodeGen cg) {
            ConstructorInfo ctor = EnvironmentType.GetConstructor(
                new Type[] {
                    typeof(object[]),
                    typeof(SymbolId[]),
                });
            cg.EmitNew(ctor);
            cg.EmitCall(typeof(RuntimeHelpers), "AddFunctionEnvironmentToArray");
        }
    }
}
