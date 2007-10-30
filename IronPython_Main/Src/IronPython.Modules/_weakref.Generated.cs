/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

namespace IronPython.Modules {
    partial class ProxyPythonTypeBuilder : OpsReflectedTypeBuilder {

        public new static PythonType Build(Type t) {
            return new ProxyPythonTypeBuilder().DoBuild(t);
        }

        private void AddWrapperOperator(SymbolId id, PythonType weakrefType) {
            SlotWrapper sw = new SlotWrapper(id, weakrefType);

            AddOperator(sw, PythonExtensionTypeAttribute._pythonOperatorTable[id]);
            SetValue(id, ContextId.Empty, sw);
        }

        protected override void AddOps() {
            PythonType res = Builder.UnfinishedType;
            PythonTypeBuilder dtb = Builder;

            #region Generated WeakRef Operators Initialization

            // *** BEGIN GENERATED CODE ***

            AddWrapperOperator(Symbols.OperatorAdd, res);
            AddWrapperOperator(Symbols.OperatorReverseAdd, res);
            AddWrapperOperator(Symbols.OperatorInPlaceAdd, res);
            AddWrapperOperator(Symbols.OperatorSubtract, res);
            AddWrapperOperator(Symbols.OperatorReverseSubtract, res);
            AddWrapperOperator(Symbols.OperatorInPlaceSubtract, res);
            AddWrapperOperator(Symbols.OperatorPower, res);
            AddWrapperOperator(Symbols.OperatorReversePower, res);
            AddWrapperOperator(Symbols.OperatorInPlacePower, res);
            AddWrapperOperator(Symbols.OperatorMultiply, res);
            AddWrapperOperator(Symbols.OperatorReverseMultiply, res);
            AddWrapperOperator(Symbols.OperatorInPlaceMultiply, res);
            AddWrapperOperator(Symbols.OperatorFloorDivide, res);
            AddWrapperOperator(Symbols.OperatorReverseFloorDivide, res);
            AddWrapperOperator(Symbols.OperatorInPlaceFloorDivide, res);
            AddWrapperOperator(Symbols.OperatorDivide, res);
            AddWrapperOperator(Symbols.OperatorReverseDivide, res);
            AddWrapperOperator(Symbols.OperatorInPlaceDivide, res);
            AddWrapperOperator(Symbols.OperatorTrueDivide, res);
            AddWrapperOperator(Symbols.OperatorReverseTrueDivide, res);
            AddWrapperOperator(Symbols.OperatorInPlaceTrueDivide, res);
            AddWrapperOperator(Symbols.OperatorMod, res);
            AddWrapperOperator(Symbols.OperatorReverseMod, res);
            AddWrapperOperator(Symbols.OperatorInPlaceMod, res);
            AddWrapperOperator(Symbols.OperatorLeftShift, res);
            AddWrapperOperator(Symbols.OperatorReverseLeftShift, res);
            AddWrapperOperator(Symbols.OperatorInPlaceLeftShift, res);
            AddWrapperOperator(Symbols.OperatorRightShift, res);
            AddWrapperOperator(Symbols.OperatorReverseRightShift, res);
            AddWrapperOperator(Symbols.OperatorInPlaceRightShift, res);
            AddWrapperOperator(Symbols.OperatorBitwiseAnd, res);
            AddWrapperOperator(Symbols.OperatorReverseBitwiseAnd, res);
            AddWrapperOperator(Symbols.OperatorInPlaceBitwiseAnd, res);
            AddWrapperOperator(Symbols.OperatorBitwiseOr, res);
            AddWrapperOperator(Symbols.OperatorReverseBitwiseOr, res);
            AddWrapperOperator(Symbols.OperatorInPlaceBitwiseOr, res);
            AddWrapperOperator(Symbols.OperatorXor, res);
            AddWrapperOperator(Symbols.OperatorReverseXor, res);
            AddWrapperOperator(Symbols.OperatorInPlaceXor, res);
            AddWrapperOperator(Symbols.OperatorLessThan, res);
            AddWrapperOperator(Symbols.OperatorGreaterThan, res);
            AddWrapperOperator(Symbols.OperatorLessThanOrEqual, res);
            AddWrapperOperator(Symbols.OperatorGreaterThanOrEqual, res);
            AddWrapperOperator(Symbols.OperatorEquals, res);
            AddWrapperOperator(Symbols.OperatorNotEquals, res);
            AddWrapperOperator(Symbols.OperatorLessThanGreaterThan, res);

            // *** END GENERATED CODE ***

            #endregion

            AddWrapperOperator(Symbols.String, res);
            AddWrapperOperator(Symbols.GetItem, res);
            AddWrapperOperator(Symbols.SetItem, res);
            AddWrapperOperator(Symbols.DelItem, res);
            AddWrapperOperator(Symbols.Length, res);
            AddWrapperOperator(Symbols.Positive, res);
            AddWrapperOperator(Symbols.OperatorNegate, res);
            AddWrapperOperator(Symbols.OperatorOnesComplement, res);
            AddWrapperOperator(Symbols.Contains, res);
            AddWrapperOperator(Symbols.Call, res);
            AddWrapperOperator(Symbols.AbsoluteValue, res);
            AddWrapperOperator(Symbols.ConvertToComplex, res);
            AddWrapperOperator(Symbols.ConvertToFloat, res);
            AddWrapperOperator(Symbols.ConvertToHex, res);
            AddWrapperOperator(Symbols.ConvertToLong, res);

            base.AddOps();
        }
    }
}
