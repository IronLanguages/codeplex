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
using System.Reflection.Emit;
using System.Diagnostics;

using Microsoft.Scripting.Utils;
using Microsoft.Contracts;
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Argument access
    /// </summary>
    class ArgSlot : Slot {
        private Type _argType;
        private int _index;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")] // TODO: fix
        private LambdaCompiler _codeGen;

        public ArgSlot(int index, Type type, LambdaCompiler codeGen) {
            this._index = index;
            this._argType = type;
            this._codeGen = codeGen;
        }

        public override void EmitGet(LambdaCompiler cg) {
            Contract.RequiresNotNull(cg, "cg");
            Debug.Assert(cg == this._codeGen);
            cg.EmitTrueArgGet(_index);
        }

        public override void EmitGetAddr(LambdaCompiler cg) {
            Contract.RequiresNotNull(cg, "cg");
            Debug.Assert(cg == this._codeGen);
            cg.EmitArgAddr(_index);
        }

        public override void EmitSet(LambdaCompiler cg) {
            Contract.RequiresNotNull(cg, "cg");
            Debug.Assert(cg == this._codeGen);
            cg.Emit(OpCodes.Starg, _index);
        }

        public override Type Type {
            get {
                return _argType;
            }
        }

        [Confined]
        public override string/*!*/ ToString() {
            return String.Format("ArgSlot Index: {0} Type: {1}", _index, _argType.FullName);
        }
    }
}
