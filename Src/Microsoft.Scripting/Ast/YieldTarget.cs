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

using System.Reflection.Emit;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public struct YieldTarget {
        private Label _topBranchTarget;
        private Label _yieldContinuationTarget;

        public YieldTarget(Label topBranchTarget) {
            this._topBranchTarget = topBranchTarget;
            _yieldContinuationTarget = new Label();
        }

        public Label TopBranchTarget {
            get { return _topBranchTarget; }
            set { _topBranchTarget = value; }
        }

        public Label YieldContinuationTarget {
            get { return _yieldContinuationTarget; }
            set { _yieldContinuationTarget = value; }
        }

        internal YieldTarget FixForTryCatchFinally(CodeGen cg) {
            _yieldContinuationTarget = cg.DefineLabel();
            return this;
        }
    }
}
