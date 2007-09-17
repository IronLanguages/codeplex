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
using System.Reflection;
using System.Diagnostics;

using IronPython.Runtime;

using Microsoft.Scripting;

namespace IronPython.Runtime.Types {
    public partial class PythonExtensionTypeAttribute {
        public static void InitializeOperatorTable() {
            Dictionary<SymbolId, OperatorMapping> pyOp = new Dictionary<SymbolId, OperatorMapping>();

            #region Generated PythonOperator Mapping

            // *** BEGIN GENERATED CODE ***

            pyOp[Symbols.OperatorAdd] = new OperatorMapping(Operators.Add, false, true, false, true);
            pyOp[Symbols.OperatorReverseAdd] = new OperatorMapping(Operators.ReverseAdd, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceAdd] = new OperatorMapping(Operators.InPlaceAdd, false, true, false);
            pyOp[Symbols.OperatorSubtract] = new OperatorMapping(Operators.Subtract, false, true, false, true);
            pyOp[Symbols.OperatorReverseSubtract] = new OperatorMapping(Operators.ReverseSubtract, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceSubtract] = new OperatorMapping(Operators.InPlaceSubtract, false, true, false);
            pyOp[Symbols.OperatorPower] = new OperatorMapping(Operators.Power, false, true, false, true);
            pyOp[Symbols.OperatorReversePower] = new OperatorMapping(Operators.ReversePower, false, true, false, true);
            pyOp[Symbols.OperatorInPlacePower] = new OperatorMapping(Operators.InPlacePower, false, true, false);
            pyOp[Symbols.OperatorMultiply] = new OperatorMapping(Operators.Multiply, false, true, false, true);
            pyOp[Symbols.OperatorReverseMultiply] = new OperatorMapping(Operators.ReverseMultiply, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceMultiply] = new OperatorMapping(Operators.InPlaceMultiply, false, true, false);
            pyOp[Symbols.OperatorFloorDivide] = new OperatorMapping(Operators.FloorDivide, false, true, false, true);
            pyOp[Symbols.OperatorReverseFloorDivide] = new OperatorMapping(Operators.ReverseFloorDivide, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceFloorDivide] = new OperatorMapping(Operators.InPlaceFloorDivide, false, true, false);
            pyOp[Symbols.OperatorDivide] = new OperatorMapping(Operators.Divide, false, true, false, true);
            pyOp[Symbols.OperatorReverseDivide] = new OperatorMapping(Operators.ReverseDivide, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceDivide] = new OperatorMapping(Operators.InPlaceDivide, false, true, false);
            pyOp[Symbols.OperatorTrueDivide] = new OperatorMapping(Operators.TrueDivide, false, true, false, true);
            pyOp[Symbols.OperatorReverseTrueDivide] = new OperatorMapping(Operators.ReverseTrueDivide, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceTrueDivide] = new OperatorMapping(Operators.InPlaceTrueDivide, false, true, false);
            pyOp[Symbols.OperatorMod] = new OperatorMapping(Operators.Mod, false, true, false, true);
            pyOp[Symbols.OperatorReverseMod] = new OperatorMapping(Operators.ReverseMod, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceMod] = new OperatorMapping(Operators.InPlaceMod, false, true, false);
            pyOp[Symbols.OperatorLeftShift] = new OperatorMapping(Operators.LeftShift, false, true, false, true);
            pyOp[Symbols.OperatorReverseLeftShift] = new OperatorMapping(Operators.ReverseLeftShift, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceLeftShift] = new OperatorMapping(Operators.InPlaceLeftShift, false, true, false);
            pyOp[Symbols.OperatorRightShift] = new OperatorMapping(Operators.RightShift, false, true, false, true);
            pyOp[Symbols.OperatorReverseRightShift] = new OperatorMapping(Operators.ReverseRightShift, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceRightShift] = new OperatorMapping(Operators.InPlaceRightShift, false, true, false);
            pyOp[Symbols.OperatorBitwiseAnd] = new OperatorMapping(Operators.BitwiseAnd, false, true, false, true);
            pyOp[Symbols.OperatorReverseBitwiseAnd] = new OperatorMapping(Operators.ReverseBitwiseAnd, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceBitwiseAnd] = new OperatorMapping(Operators.InPlaceBitwiseAnd, false, true, false);
            pyOp[Symbols.OperatorBitwiseOr] = new OperatorMapping(Operators.BitwiseOr, false, true, false, true);
            pyOp[Symbols.OperatorReverseBitwiseOr] = new OperatorMapping(Operators.ReverseBitwiseOr, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceBitwiseOr] = new OperatorMapping(Operators.InPlaceBitwiseOr, false, true, false);
            pyOp[Symbols.OperatorXor] = new OperatorMapping(Operators.Xor, false, true, false, true);
            pyOp[Symbols.OperatorReverseXor] = new OperatorMapping(Operators.ReverseXor, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceXor] = new OperatorMapping(Operators.InPlaceXor, false, true, false);
            pyOp[Symbols.OperatorLessThan] = new OperatorMapping(Operators.LessThan, false, true, false, true);
            pyOp[Symbols.OperatorGreaterThan] = new OperatorMapping(Operators.GreaterThan, false, true, false, true);
            pyOp[Symbols.OperatorLessThanOrEqual] = new OperatorMapping(Operators.LessThanOrEqual, false, true, false, true);
            pyOp[Symbols.OperatorGreaterThanOrEqual] = new OperatorMapping(Operators.GreaterThanOrEqual, false, true, false, true);
            pyOp[Symbols.OperatorEquals] = new OperatorMapping(Operators.Equals, false, true, false, true);
            pyOp[Symbols.OperatorNotEquals] = new OperatorMapping(Operators.NotEquals, false, true, false, true);
            pyOp[Symbols.OperatorLessThanGreaterThan] = new OperatorMapping(Operators.LessThanGreaterThan, false, true, false, true);

            // *** END GENERATED CODE ***

            #endregion

            pyOp[Symbols.GetItem] = new OperatorMapping(Operators.GetItem, false, true, false);
            pyOp[Symbols.SetItem] = new OperatorMapping(Operators.SetItem, false, false, true);
            pyOp[Symbols.DelItem] = new OperatorMapping(Operators.DeleteItem, false, true, false);
            pyOp[Symbols.Cmp] = new OperatorMapping(Operators.Compare, false, true, false);
            pyOp[Symbols.Positive] = new OperatorMapping(Operators.Positive, true, false, false);
            pyOp[Symbols.OperatorNegate] = new OperatorMapping(Operators.Negate, true, false, false);
            pyOp[Symbols.OperatorOnesComplement] = new OperatorMapping(Operators.OnesComplement, true, false, false);
            pyOp[Symbols.Repr] = new OperatorMapping(Operators.CodeRepresentation, true, false, false);
            pyOp[Symbols.Length] = new OperatorMapping(Operators.Length, true, false, false);
            pyOp[Symbols.Call] = new OperatorMapping(Operators.Call, false, true, false);
            pyOp[Symbols.DivMod] = new OperatorMapping(Operators.DivMod, false, true, false);
            pyOp[Symbols.ReverseDivMod] = new OperatorMapping(Operators.ReverseDivMod, false, true, false);
            pyOp[Symbols.String] = new OperatorMapping(Operators.ConvertToString, true, false, false);
            pyOp[Symbols.GeneratorNext] = new OperatorMapping(Operators.MoveNext, true, false, false);
            pyOp[Symbols.GetBoundAttr] = new OperatorMapping(Operators.GetBoundMember, false, true, false);
            pyOp[Symbols.Unassign] = new OperatorMapping(Operators.Unassign, true, false, false);
            pyOp[Symbols.Coerce] = new OperatorMapping(Operators.Coerce, false, true, false);
            pyOp[Symbols.GetDescriptor] = new OperatorMapping(Operators.GetDescriptor, false, false, true);
            pyOp[Symbols.Missing] = new OperatorMapping(Operators.Missing, false, true, false);
            pyOp[Symbols.OperatorPower] = new OperatorMapping(Operators.Power, false, true, true);
            pyOp[Symbols.SetDescriptor] = new OperatorMapping(Operators.SetDescriptor, false, false, true);
            pyOp[Symbols.DeleteDescriptor] = new OperatorMapping(Operators.DeleteDescriptor, false, true, false);
            pyOp[Symbols.Contains] = new OperatorMapping(Operators.Contains, false, true, false);

            pyOp[Symbols.AbsoluteValue] = new OperatorMapping(Operators.AbsoluteValue, true, false, false);
            pyOp[Symbols.ConvertToLong] = new OperatorMapping(Operators.ConvertToBigInteger, true, false, false);
            pyOp[Symbols.ConvertToComplex] = new OperatorMapping(Operators.ConvertToComplex, true, false, false);
            pyOp[Symbols.ConvertToFloat] = new OperatorMapping(Operators.ConvertToDouble, true, false, false);
            pyOp[Symbols.ConvertToInt] = new OperatorMapping(Operators.ConvertToInt32, true, false, false);
            pyOp[Symbols.ConvertToHex] = new OperatorMapping(Operators.ConvertToHex, true, false, false);
            pyOp[Symbols.ConvertToOctal] = new OperatorMapping(Operators.ConvertToOctal, true, false, false);
            pyOp[Symbols.NonZero] = new OperatorMapping(Operators.ConvertToBoolean, true, false, false);
            pyOp[Symbols.GetState] = new OperatorMapping(Operators.GetState, true, false, false);
            pyOp[Symbols.Hash] = new OperatorMapping(Operators.ValueHash, true, false, false);


            // build reverse operator table
            Dictionary<OperatorMapping, SymbolId> revOp = new Dictionary<OperatorMapping, SymbolId>();
            foreach (KeyValuePair<SymbolId, OperatorMapping> kvp in pyOp) {
                revOp[kvp.Value] = kvp.Key;
            }

            _pythonOperatorTable = pyOp;
            _reverseOperatorTable = revOp;
        }
    }
}
