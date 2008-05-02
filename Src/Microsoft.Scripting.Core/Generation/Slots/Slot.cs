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
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Slot refers to a reference to an object. For eg, a global variable, a local variable, etc.
    /// A Slot is referred to using a Name. The Namespace is used to map a Name to a Slot.
    /// Multiple Names can refer to the same Slot.
    /// For eg. multiple closures can refer to the same Slot of a local variable in the enclosing
    /// function. Though each closure will use the same string (the name of the variable), each
    /// string is considered a unique Name or symbol.
    /// </summary>
    abstract class Slot {
        private bool _local;
        private Type _knownType;
        public abstract void EmitGet(ILGen cg);
        public abstract void EmitGetAddr(ILGen cg);

        #region emit Set methods
        // Must override at least one of these two methods or get infinite loop
        // 
        public virtual void EmitSet(ILGen cg, Slot val) {
            ContractUtils.RequiresNotNull(val, "val");
            ContractUtils.RequiresNotNull(cg, "cg");

            val.EmitGet(cg);
            EmitSet(cg);
        }

        // This override assumes that the IL stack already holds the value to be assigned from.
        // This default implementation assumes the top of stack is typeof(Object).
        public virtual void EmitSet(ILGen cg) {
            ContractUtils.RequiresNotNull(cg, "cg");

            // localTmpVal = <top of IL stack>
            Slot localTmpVal = cg.GetLocalTmp(typeof(object));
            localTmpVal.EmitSet(cg);

            // <slot> = localTmpVal
            EmitSet(cg, localTmpVal);

            cg.FreeLocalTmp(localTmpVal);
        }
        #endregion // emit Set methods

        // Slots are to be implemented as dictionaries for Python. However,
        // for performance and better integration with the CLI, the engine
        // implements many Slot types as CLI entities, which cannot be
        // deleted once they are created. Hence, to implement "del",
        // an alternate scheme is used. We just assign Uninitialized.instance
        // to represent that the Slot has been deleted.
        // Any access to the Slot first checks if it is holding Uninitialized.instance,
        // which means that it should virtually not exist

        public virtual void EmitSetUninitialized(ILGen cg) {
            ContractUtils.RequiresNotNull(cg, "cg");

            // Emit the following:
            //     <name> = Uninitialized.instance;

            // This pushes on typeof(Uninitialized), which is a System.Object. Slots that support
            // value-types must override this to deal with boxing.
            Debug.Assert(!Type.IsValueType);
            cg.EmitUninitialized();


            EmitSet(cg);
        }

        public virtual void EmitDelete(ILGen cg, SymbolId name) {
            ContractUtils.RequiresNotNull(cg, "cg");
            EmitSetUninitialized(cg);
        }

        public abstract Type Type { get; }

        /// <summary>
        /// True if the slot represents a local variable
        /// </summary>
        public bool Local {
            get { return _local; }
            set { _local = value; }
        }

        public Type KnownType {
            get { return _knownType; }
            set { _knownType = value; }
        }

        [Confined]
        public override string/*!*/ ToString() {
            return String.Format("{0} Type: {1}", GetType().Name, Type.FullName);
        }
    }
}
