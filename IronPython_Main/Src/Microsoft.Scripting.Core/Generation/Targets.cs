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
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;

using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    public enum TargetBlockType {
        Normal,
        Try,
        Finally,
        Catch,
        Else,
        LoopInFinally
    }

    internal class Targets {
        private readonly Label? _breakLabel;
        private readonly Label? _continueLabel;
        private readonly Slot _finallyReturns;
        private readonly LabelTarget _label;
        private readonly TargetBlockType _blockType;

        private Label? _leaveLabel;

        internal Targets(Label? breakLabel, Label? continueLabel, TargetBlockType blockType, Slot finallyReturns, LabelTarget label) {
            _breakLabel = breakLabel;
            _continueLabel = continueLabel;
            _blockType = blockType;
            _finallyReturns = finallyReturns;
            _label = label;
        }

        internal static Label? NoLabel {
            get { return null; }
        }

        internal Label? BreakLabel {
            get { return _breakLabel; }
        }

        internal Label? ContinueLabel {
            get { return _continueLabel; }
        }

        internal Slot FinallyReturns {
            get { return _finallyReturns; }
        }

        internal TargetBlockType BlockType {
            get { return _blockType; }
        }

        internal LabelTarget Label {
            get { return _label; }
        }

        internal Label? LeaveLabel {
            get { return _leaveLabel; }
            set { _leaveLabel = value; }
        }
    }
}
