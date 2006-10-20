/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

namespace IronPython.Modules {
    public partial class ProxyDynamicType : ReflectedType {

        protected override void AddOps() {
            #region Generated WeakRef Operators Initialization

            // *** BEGIN GENERATED CODE ***

            dict[SymbolTable.OpAdd] = new SlotWrapper(SymbolTable.OpAdd, this);
            dict[SymbolTable.OpReverseAdd] = new SlotWrapper(SymbolTable.OpReverseAdd, this);
            dict[SymbolTable.OpInPlaceAdd] = new SlotWrapper(SymbolTable.OpInPlaceAdd, this);
            dict[SymbolTable.OpSubtract] = new SlotWrapper(SymbolTable.OpSubtract, this);
            dict[SymbolTable.OpReverseSubtract] = new SlotWrapper(SymbolTable.OpReverseSubtract, this);
            dict[SymbolTable.OpInPlaceSubtract] = new SlotWrapper(SymbolTable.OpInPlaceSubtract, this);
            dict[SymbolTable.OpPower] = new SlotWrapper(SymbolTable.OpPower, this);
            dict[SymbolTable.OpReversePower] = new SlotWrapper(SymbolTable.OpReversePower, this);
            dict[SymbolTable.OpInPlacePower] = new SlotWrapper(SymbolTable.OpInPlacePower, this);
            dict[SymbolTable.OpMultiply] = new SlotWrapper(SymbolTable.OpMultiply, this);
            dict[SymbolTable.OpReverseMultiply] = new SlotWrapper(SymbolTable.OpReverseMultiply, this);
            dict[SymbolTable.OpInPlaceMultiply] = new SlotWrapper(SymbolTable.OpInPlaceMultiply, this);
            dict[SymbolTable.OpFloorDivide] = new SlotWrapper(SymbolTable.OpFloorDivide, this);
            dict[SymbolTable.OpReverseFloorDivide] = new SlotWrapper(SymbolTable.OpReverseFloorDivide, this);
            dict[SymbolTable.OpInPlaceFloorDivide] = new SlotWrapper(SymbolTable.OpInPlaceFloorDivide, this);
            dict[SymbolTable.OpDivide] = new SlotWrapper(SymbolTable.OpDivide, this);
            dict[SymbolTable.OpReverseDivide] = new SlotWrapper(SymbolTable.OpReverseDivide, this);
            dict[SymbolTable.OpInPlaceDivide] = new SlotWrapper(SymbolTable.OpInPlaceDivide, this);
            dict[SymbolTable.OpTrueDivide] = new SlotWrapper(SymbolTable.OpTrueDivide, this);
            dict[SymbolTable.OpReverseTrueDivide] = new SlotWrapper(SymbolTable.OpReverseTrueDivide, this);
            dict[SymbolTable.OpInPlaceTrueDivide] = new SlotWrapper(SymbolTable.OpInPlaceTrueDivide, this);
            dict[SymbolTable.OpMod] = new SlotWrapper(SymbolTable.OpMod, this);
            dict[SymbolTable.OpReverseMod] = new SlotWrapper(SymbolTable.OpReverseMod, this);
            dict[SymbolTable.OpInPlaceMod] = new SlotWrapper(SymbolTable.OpInPlaceMod, this);
            dict[SymbolTable.OpLeftShift] = new SlotWrapper(SymbolTable.OpLeftShift, this);
            dict[SymbolTable.OpReverseLeftShift] = new SlotWrapper(SymbolTable.OpReverseLeftShift, this);
            dict[SymbolTable.OpInPlaceLeftShift] = new SlotWrapper(SymbolTable.OpInPlaceLeftShift, this);
            dict[SymbolTable.OpRightShift] = new SlotWrapper(SymbolTable.OpRightShift, this);
            dict[SymbolTable.OpReverseRightShift] = new SlotWrapper(SymbolTable.OpReverseRightShift, this);
            dict[SymbolTable.OpInPlaceRightShift] = new SlotWrapper(SymbolTable.OpInPlaceRightShift, this);
            dict[SymbolTable.OpBitwiseAnd] = new SlotWrapper(SymbolTable.OpBitwiseAnd, this);
            dict[SymbolTable.OpReverseBitwiseAnd] = new SlotWrapper(SymbolTable.OpReverseBitwiseAnd, this);
            dict[SymbolTable.OpInPlaceBitwiseAnd] = new SlotWrapper(SymbolTable.OpInPlaceBitwiseAnd, this);
            dict[SymbolTable.OpBitwiseOr] = new SlotWrapper(SymbolTable.OpBitwiseOr, this);
            dict[SymbolTable.OpReverseBitwiseOr] = new SlotWrapper(SymbolTable.OpReverseBitwiseOr, this);
            dict[SymbolTable.OpInPlaceBitwiseOr] = new SlotWrapper(SymbolTable.OpInPlaceBitwiseOr, this);
            dict[SymbolTable.OpXor] = new SlotWrapper(SymbolTable.OpXor, this);
            dict[SymbolTable.OpReverseXor] = new SlotWrapper(SymbolTable.OpReverseXor, this);
            dict[SymbolTable.OpInPlaceXor] = new SlotWrapper(SymbolTable.OpInPlaceXor, this);
            dict[SymbolTable.OpLessThan] = new SlotWrapper(SymbolTable.OpLessThan, this);
            dict[SymbolTable.OpGreaterThan] = new SlotWrapper(SymbolTable.OpGreaterThan, this);
            dict[SymbolTable.OpLessThanOrEqual] = new SlotWrapper(SymbolTable.OpLessThanOrEqual, this);
            dict[SymbolTable.OpGreaterThanOrEqual] = new SlotWrapper(SymbolTable.OpGreaterThanOrEqual, this);
            dict[SymbolTable.OpEqual] = new SlotWrapper(SymbolTable.OpEqual, this);
            dict[SymbolTable.OpNotEqual] = new SlotWrapper(SymbolTable.OpNotEqual, this);
            dict[SymbolTable.OpLessThanGreaterThan] = new SlotWrapper(SymbolTable.OpLessThanGreaterThan, this);

            // *** END GENERATED CODE ***

            #endregion

        }
    }
}
