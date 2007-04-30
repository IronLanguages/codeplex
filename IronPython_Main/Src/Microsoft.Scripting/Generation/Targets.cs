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
using System.Reflection.Emit;

using Microsoft.Scripting.Internal.Ast;

namespace Microsoft.Scripting.Internal.Generation {
    public enum TargetBlockType {
        Normal,
        Try,
        Finally,
        Catch,
        Else,
        With,
        LoopInFinally
    }

    class Targets {
        public static readonly Nullable<Label> NoLabel = null;
        public readonly Nullable<Label> breakLabel;
        public readonly Nullable<Label> continueLabel;
        public Nullable<Label> leaveLabel;
        private TargetBlockType _blockType;
        public readonly Slot finallyReturns;
        public Slot isBlockYielded;
        public Statement statement;

        public TargetBlockType BlockType {
            get {
                return _blockType;
            }
        }

        public Targets(Nullable<Label> breakLabel, Nullable<Label> continueLabel, TargetBlockType blockType, Slot finallyReturns, Slot isBlockYielded, Statement statement) {
            this.breakLabel = breakLabel;
            this.continueLabel = continueLabel;
            this._blockType = blockType;
            this.finallyReturns = finallyReturns;
            this.leaveLabel = null;
            this.isBlockYielded = isBlockYielded;
            this.statement = statement;
        }
    }
}
