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
using System.Reflection.Emit;
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    internal sealed class LocalStorageAllocator : StorageAllocator {
        private LambdaCompiler _codeGen;

        internal LocalStorageAllocator(LambdaCompiler codeGen) {
            _codeGen = codeGen;
        }

        internal override Storage AllocateStorage(Expression variable) {
            LocalBuilder b = _codeGen.IL.DeclareLocal(variable.Type);
            if (_codeGen.EmitDebugSymbols) b.SetLocalSymInfo(SymbolTable.IdToString(VariableInfo.GetName(variable)));
            return new SlotStorage(new LocalSlot(b, _codeGen.IL));
        }
    }
}
