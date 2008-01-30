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
using Microsoft.Scripting;

namespace IronPython.Runtime {
    public static partial class Symbols {
        #region Generated Symbols - Ops Symbols

        // *** BEGIN GENERATED CODE ***

        private static SymbolId _OperatorAdd;
        ///<summary>SymbolId for '__add__'</summary>
        public static SymbolId OperatorAdd {
            get {
                if (_OperatorAdd == SymbolId.Empty) _OperatorAdd = MakeSymbolId("__add__");
                return _OperatorAdd;
            }
        }
        private static SymbolId _OperatorReverseAdd;
        ///<summary>SymbolId for '__radd__'</summary>
        public static SymbolId OperatorReverseAdd {
            get {
                if (_OperatorReverseAdd == SymbolId.Empty) _OperatorReverseAdd = MakeSymbolId("__radd__");
                return _OperatorReverseAdd;
            }
        }
        private static SymbolId _OperatorInPlaceAdd;
        ///<summary>SymbolId for '__iadd__'</summary>
        public static SymbolId OperatorInPlaceAdd {
            get {
                if (_OperatorInPlaceAdd == SymbolId.Empty) _OperatorInPlaceAdd = MakeSymbolId("__iadd__");
                return _OperatorInPlaceAdd;
            }
        }
        private static SymbolId _OperatorSubtract;
        ///<summary>SymbolId for '__sub__'</summary>
        public static SymbolId OperatorSubtract {
            get {
                if (_OperatorSubtract == SymbolId.Empty) _OperatorSubtract = MakeSymbolId("__sub__");
                return _OperatorSubtract;
            }
        }
        private static SymbolId _OperatorReverseSubtract;
        ///<summary>SymbolId for '__rsub__'</summary>
        public static SymbolId OperatorReverseSubtract {
            get {
                if (_OperatorReverseSubtract == SymbolId.Empty) _OperatorReverseSubtract = MakeSymbolId("__rsub__");
                return _OperatorReverseSubtract;
            }
        }
        private static SymbolId _OperatorInPlaceSubtract;
        ///<summary>SymbolId for '__isub__'</summary>
        public static SymbolId OperatorInPlaceSubtract {
            get {
                if (_OperatorInPlaceSubtract == SymbolId.Empty) _OperatorInPlaceSubtract = MakeSymbolId("__isub__");
                return _OperatorInPlaceSubtract;
            }
        }
        private static SymbolId _OperatorPower;
        ///<summary>SymbolId for '__pow__'</summary>
        public static SymbolId OperatorPower {
            get {
                if (_OperatorPower == SymbolId.Empty) _OperatorPower = MakeSymbolId("__pow__");
                return _OperatorPower;
            }
        }
        private static SymbolId _OperatorReversePower;
        ///<summary>SymbolId for '__rpow__'</summary>
        public static SymbolId OperatorReversePower {
            get {
                if (_OperatorReversePower == SymbolId.Empty) _OperatorReversePower = MakeSymbolId("__rpow__");
                return _OperatorReversePower;
            }
        }
        private static SymbolId _OperatorInPlacePower;
        ///<summary>SymbolId for '__ipow__'</summary>
        public static SymbolId OperatorInPlacePower {
            get {
                if (_OperatorInPlacePower == SymbolId.Empty) _OperatorInPlacePower = MakeSymbolId("__ipow__");
                return _OperatorInPlacePower;
            }
        }
        private static SymbolId _OperatorMultiply;
        ///<summary>SymbolId for '__mul__'</summary>
        public static SymbolId OperatorMultiply {
            get {
                if (_OperatorMultiply == SymbolId.Empty) _OperatorMultiply = MakeSymbolId("__mul__");
                return _OperatorMultiply;
            }
        }
        private static SymbolId _OperatorReverseMultiply;
        ///<summary>SymbolId for '__rmul__'</summary>
        public static SymbolId OperatorReverseMultiply {
            get {
                if (_OperatorReverseMultiply == SymbolId.Empty) _OperatorReverseMultiply = MakeSymbolId("__rmul__");
                return _OperatorReverseMultiply;
            }
        }
        private static SymbolId _OperatorInPlaceMultiply;
        ///<summary>SymbolId for '__imul__'</summary>
        public static SymbolId OperatorInPlaceMultiply {
            get {
                if (_OperatorInPlaceMultiply == SymbolId.Empty) _OperatorInPlaceMultiply = MakeSymbolId("__imul__");
                return _OperatorInPlaceMultiply;
            }
        }
        private static SymbolId _OperatorFloorDivide;
        ///<summary>SymbolId for '__floordiv__'</summary>
        public static SymbolId OperatorFloorDivide {
            get {
                if (_OperatorFloorDivide == SymbolId.Empty) _OperatorFloorDivide = MakeSymbolId("__floordiv__");
                return _OperatorFloorDivide;
            }
        }
        private static SymbolId _OperatorReverseFloorDivide;
        ///<summary>SymbolId for '__rfloordiv__'</summary>
        public static SymbolId OperatorReverseFloorDivide {
            get {
                if (_OperatorReverseFloorDivide == SymbolId.Empty) _OperatorReverseFloorDivide = MakeSymbolId("__rfloordiv__");
                return _OperatorReverseFloorDivide;
            }
        }
        private static SymbolId _OperatorInPlaceFloorDivide;
        ///<summary>SymbolId for '__ifloordiv__'</summary>
        public static SymbolId OperatorInPlaceFloorDivide {
            get {
                if (_OperatorInPlaceFloorDivide == SymbolId.Empty) _OperatorInPlaceFloorDivide = MakeSymbolId("__ifloordiv__");
                return _OperatorInPlaceFloorDivide;
            }
        }
        private static SymbolId _OperatorDivide;
        ///<summary>SymbolId for '__div__'</summary>
        public static SymbolId OperatorDivide {
            get {
                if (_OperatorDivide == SymbolId.Empty) _OperatorDivide = MakeSymbolId("__div__");
                return _OperatorDivide;
            }
        }
        private static SymbolId _OperatorReverseDivide;
        ///<summary>SymbolId for '__rdiv__'</summary>
        public static SymbolId OperatorReverseDivide {
            get {
                if (_OperatorReverseDivide == SymbolId.Empty) _OperatorReverseDivide = MakeSymbolId("__rdiv__");
                return _OperatorReverseDivide;
            }
        }
        private static SymbolId _OperatorInPlaceDivide;
        ///<summary>SymbolId for '__idiv__'</summary>
        public static SymbolId OperatorInPlaceDivide {
            get {
                if (_OperatorInPlaceDivide == SymbolId.Empty) _OperatorInPlaceDivide = MakeSymbolId("__idiv__");
                return _OperatorInPlaceDivide;
            }
        }
        private static SymbolId _OperatorTrueDivide;
        ///<summary>SymbolId for '__truediv__'</summary>
        public static SymbolId OperatorTrueDivide {
            get {
                if (_OperatorTrueDivide == SymbolId.Empty) _OperatorTrueDivide = MakeSymbolId("__truediv__");
                return _OperatorTrueDivide;
            }
        }
        private static SymbolId _OperatorReverseTrueDivide;
        ///<summary>SymbolId for '__rtruediv__'</summary>
        public static SymbolId OperatorReverseTrueDivide {
            get {
                if (_OperatorReverseTrueDivide == SymbolId.Empty) _OperatorReverseTrueDivide = MakeSymbolId("__rtruediv__");
                return _OperatorReverseTrueDivide;
            }
        }
        private static SymbolId _OperatorInPlaceTrueDivide;
        ///<summary>SymbolId for '__itruediv__'</summary>
        public static SymbolId OperatorInPlaceTrueDivide {
            get {
                if (_OperatorInPlaceTrueDivide == SymbolId.Empty) _OperatorInPlaceTrueDivide = MakeSymbolId("__itruediv__");
                return _OperatorInPlaceTrueDivide;
            }
        }
        private static SymbolId _OperatorMod;
        ///<summary>SymbolId for '__mod__'</summary>
        public static SymbolId OperatorMod {
            get {
                if (_OperatorMod == SymbolId.Empty) _OperatorMod = MakeSymbolId("__mod__");
                return _OperatorMod;
            }
        }
        private static SymbolId _OperatorReverseMod;
        ///<summary>SymbolId for '__rmod__'</summary>
        public static SymbolId OperatorReverseMod {
            get {
                if (_OperatorReverseMod == SymbolId.Empty) _OperatorReverseMod = MakeSymbolId("__rmod__");
                return _OperatorReverseMod;
            }
        }
        private static SymbolId _OperatorInPlaceMod;
        ///<summary>SymbolId for '__imod__'</summary>
        public static SymbolId OperatorInPlaceMod {
            get {
                if (_OperatorInPlaceMod == SymbolId.Empty) _OperatorInPlaceMod = MakeSymbolId("__imod__");
                return _OperatorInPlaceMod;
            }
        }
        private static SymbolId _OperatorLeftShift;
        ///<summary>SymbolId for '__lshift__'</summary>
        public static SymbolId OperatorLeftShift {
            get {
                if (_OperatorLeftShift == SymbolId.Empty) _OperatorLeftShift = MakeSymbolId("__lshift__");
                return _OperatorLeftShift;
            }
        }
        private static SymbolId _OperatorReverseLeftShift;
        ///<summary>SymbolId for '__rlshift__'</summary>
        public static SymbolId OperatorReverseLeftShift {
            get {
                if (_OperatorReverseLeftShift == SymbolId.Empty) _OperatorReverseLeftShift = MakeSymbolId("__rlshift__");
                return _OperatorReverseLeftShift;
            }
        }
        private static SymbolId _OperatorInPlaceLeftShift;
        ///<summary>SymbolId for '__ilshift__'</summary>
        public static SymbolId OperatorInPlaceLeftShift {
            get {
                if (_OperatorInPlaceLeftShift == SymbolId.Empty) _OperatorInPlaceLeftShift = MakeSymbolId("__ilshift__");
                return _OperatorInPlaceLeftShift;
            }
        }
        private static SymbolId _OperatorRightShift;
        ///<summary>SymbolId for '__rshift__'</summary>
        public static SymbolId OperatorRightShift {
            get {
                if (_OperatorRightShift == SymbolId.Empty) _OperatorRightShift = MakeSymbolId("__rshift__");
                return _OperatorRightShift;
            }
        }
        private static SymbolId _OperatorReverseRightShift;
        ///<summary>SymbolId for '__rrshift__'</summary>
        public static SymbolId OperatorReverseRightShift {
            get {
                if (_OperatorReverseRightShift == SymbolId.Empty) _OperatorReverseRightShift = MakeSymbolId("__rrshift__");
                return _OperatorReverseRightShift;
            }
        }
        private static SymbolId _OperatorInPlaceRightShift;
        ///<summary>SymbolId for '__irshift__'</summary>
        public static SymbolId OperatorInPlaceRightShift {
            get {
                if (_OperatorInPlaceRightShift == SymbolId.Empty) _OperatorInPlaceRightShift = MakeSymbolId("__irshift__");
                return _OperatorInPlaceRightShift;
            }
        }
        private static SymbolId _OperatorBitwiseAnd;
        ///<summary>SymbolId for '__and__'</summary>
        public static SymbolId OperatorBitwiseAnd {
            get {
                if (_OperatorBitwiseAnd == SymbolId.Empty) _OperatorBitwiseAnd = MakeSymbolId("__and__");
                return _OperatorBitwiseAnd;
            }
        }
        private static SymbolId _OperatorReverseBitwiseAnd;
        ///<summary>SymbolId for '__rand__'</summary>
        public static SymbolId OperatorReverseBitwiseAnd {
            get {
                if (_OperatorReverseBitwiseAnd == SymbolId.Empty) _OperatorReverseBitwiseAnd = MakeSymbolId("__rand__");
                return _OperatorReverseBitwiseAnd;
            }
        }
        private static SymbolId _OperatorInPlaceBitwiseAnd;
        ///<summary>SymbolId for '__iand__'</summary>
        public static SymbolId OperatorInPlaceBitwiseAnd {
            get {
                if (_OperatorInPlaceBitwiseAnd == SymbolId.Empty) _OperatorInPlaceBitwiseAnd = MakeSymbolId("__iand__");
                return _OperatorInPlaceBitwiseAnd;
            }
        }
        private static SymbolId _OperatorBitwiseOr;
        ///<summary>SymbolId for '__or__'</summary>
        public static SymbolId OperatorBitwiseOr {
            get {
                if (_OperatorBitwiseOr == SymbolId.Empty) _OperatorBitwiseOr = MakeSymbolId("__or__");
                return _OperatorBitwiseOr;
            }
        }
        private static SymbolId _OperatorReverseBitwiseOr;
        ///<summary>SymbolId for '__ror__'</summary>
        public static SymbolId OperatorReverseBitwiseOr {
            get {
                if (_OperatorReverseBitwiseOr == SymbolId.Empty) _OperatorReverseBitwiseOr = MakeSymbolId("__ror__");
                return _OperatorReverseBitwiseOr;
            }
        }
        private static SymbolId _OperatorInPlaceBitwiseOr;
        ///<summary>SymbolId for '__ior__'</summary>
        public static SymbolId OperatorInPlaceBitwiseOr {
            get {
                if (_OperatorInPlaceBitwiseOr == SymbolId.Empty) _OperatorInPlaceBitwiseOr = MakeSymbolId("__ior__");
                return _OperatorInPlaceBitwiseOr;
            }
        }
        private static SymbolId _OperatorXor;
        ///<summary>SymbolId for '__xor__'</summary>
        public static SymbolId OperatorXor {
            get {
                if (_OperatorXor == SymbolId.Empty) _OperatorXor = MakeSymbolId("__xor__");
                return _OperatorXor;
            }
        }
        private static SymbolId _OperatorReverseXor;
        ///<summary>SymbolId for '__rxor__'</summary>
        public static SymbolId OperatorReverseXor {
            get {
                if (_OperatorReverseXor == SymbolId.Empty) _OperatorReverseXor = MakeSymbolId("__rxor__");
                return _OperatorReverseXor;
            }
        }
        private static SymbolId _OperatorInPlaceXor;
        ///<summary>SymbolId for '__ixor__'</summary>
        public static SymbolId OperatorInPlaceXor {
            get {
                if (_OperatorInPlaceXor == SymbolId.Empty) _OperatorInPlaceXor = MakeSymbolId("__ixor__");
                return _OperatorInPlaceXor;
            }
        }
        private static SymbolId _OperatorLessThan;
        ///<summary>SymbolId for '__lt__'</summary>
        public static SymbolId OperatorLessThan {
            get {
                if (_OperatorLessThan == SymbolId.Empty) _OperatorLessThan = MakeSymbolId("__lt__");
                return _OperatorLessThan;
            }
        }
        private static SymbolId _OperatorGreaterThan;
        ///<summary>SymbolId for '__gt__'</summary>
        public static SymbolId OperatorGreaterThan {
            get {
                if (_OperatorGreaterThan == SymbolId.Empty) _OperatorGreaterThan = MakeSymbolId("__gt__");
                return _OperatorGreaterThan;
            }
        }
        private static SymbolId _OperatorLessThanOrEqual;
        ///<summary>SymbolId for '__le__'</summary>
        public static SymbolId OperatorLessThanOrEqual {
            get {
                if (_OperatorLessThanOrEqual == SymbolId.Empty) _OperatorLessThanOrEqual = MakeSymbolId("__le__");
                return _OperatorLessThanOrEqual;
            }
        }
        private static SymbolId _OperatorGreaterThanOrEqual;
        ///<summary>SymbolId for '__ge__'</summary>
        public static SymbolId OperatorGreaterThanOrEqual {
            get {
                if (_OperatorGreaterThanOrEqual == SymbolId.Empty) _OperatorGreaterThanOrEqual = MakeSymbolId("__ge__");
                return _OperatorGreaterThanOrEqual;
            }
        }
        private static SymbolId _OperatorEquals;
        ///<summary>SymbolId for '__eq__'</summary>
        public static SymbolId OperatorEquals {
            get {
                if (_OperatorEquals == SymbolId.Empty) _OperatorEquals = MakeSymbolId("__eq__");
                return _OperatorEquals;
            }
        }
        private static SymbolId _OperatorNotEquals;
        ///<summary>SymbolId for '__ne__'</summary>
        public static SymbolId OperatorNotEquals {
            get {
                if (_OperatorNotEquals == SymbolId.Empty) _OperatorNotEquals = MakeSymbolId("__ne__");
                return _OperatorNotEquals;
            }
        }
        private static SymbolId _OperatorLessThanGreaterThan;
        ///<summary>SymbolId for '__lg__'</summary>
        public static SymbolId OperatorLessThanGreaterThan {
            get {
                if (_OperatorLessThanGreaterThan == SymbolId.Empty) _OperatorLessThanGreaterThan = MakeSymbolId("__lg__");
                return _OperatorLessThanGreaterThan;
            }
        }

        // *** END GENERATED CODE ***

        #endregion

        #region Generated Symbols - Other Symbols

        // *** BEGIN GENERATED CODE ***

        private static SymbolId _OperatorNegate;
        private static SymbolId _OperatorOnesComplement;
        private static SymbolId _Dict;
        private static SymbolId _Module;
        private static SymbolId _GetAttribute;
        private static SymbolId _Bases;
        private static SymbolId _Subclasses;
        private static SymbolId _Name;
        private static SymbolId _Class;
        private static SymbolId _Builtins;
        private static SymbolId _GetBoundAttr;
        private static SymbolId _SetAttr;
        private static SymbolId _DelAttr;
        private static SymbolId _GetItem;
        private static SymbolId _SetItem;
        private static SymbolId _DelItem;
        private static SymbolId _Init;
        private static SymbolId _NewInst;
        private static SymbolId _Unassign;
        private static SymbolId _String;
        private static SymbolId _Repr;
        private static SymbolId _Contains;
        private static SymbolId _Length;
        private static SymbolId _Reversed;
        private static SymbolId _Iterator;
        private static SymbolId _Next;
        private static SymbolId _WeakRef;
        private static SymbolId _File;
        private static SymbolId _Import;
        private static SymbolId _Doc;
        private static SymbolId _Call;
        private static SymbolId _AbsoluteValue;
        private static SymbolId _Coerce;
        private static SymbolId _ConvertToInt;
        private static SymbolId _ConvertToFloat;
        private static SymbolId _ConvertToLong;
        private static SymbolId _ConvertToComplex;
        private static SymbolId _ConvertToHex;
        private static SymbolId _ConvertToOctal;
        private static SymbolId _Reduce;
        private static SymbolId _ReduceExtended;
        private static SymbolId _NonZero;
        private static SymbolId _Positive;
        private static SymbolId _Hash;
        private static SymbolId _Cmp;
        private static SymbolId _DivMod;
        private static SymbolId _ReverseDivMod;
        private static SymbolId _Path;
        private static SymbolId _GetDescriptor;
        private static SymbolId _SetDescriptor;
        private static SymbolId _DeleteDescriptor;
        private static SymbolId _All;
        private static SymbolId _ClrExceptionKey;
        private static SymbolId _Keys;
        private static SymbolId _Arguments;
        private static SymbolId _ConsoleWrite;
        private static SymbolId _ConsoleReadLine;
        private static SymbolId _ExceptionMessage;
        private static SymbolId _ExceptionFilename;
        private static SymbolId _ExceptionLineNumber;
        private static SymbolId _ExceptionOffset;
        private static SymbolId _Text;
        private static SymbolId _Softspace;
        private static SymbolId _GeneratorNext;
        private static SymbolId _SetDefaultEncoding;
        private static SymbolId _SysExitFunc;
        private static SymbolId _None;
        private static SymbolId _MetaClass;
        private static SymbolId _MethodResolutionOrder;
        private static SymbolId _GetSlice;
        private static SymbolId _SetSlice;
        private static SymbolId _DeleteSlice;
        private static SymbolId _Future;
        private static SymbolId _Division;
        private static SymbolId _NestedScopes;
        private static SymbolId _Generators;
        private static SymbolId _As;
        private static SymbolId _Star;
        private static SymbolId _StarStar;
        private static SymbolId _Locals;
        private static SymbolId _Vars;
        private static SymbolId _Dir;
        private static SymbolId _Eval;
        private static SymbolId _ExecFile;
        private static SymbolId _Underscore;
        private static SymbolId _GeneratorParmName;
        private static SymbolId _EnvironmentParmName;
        private static SymbolId _Iter;
        private static SymbolId _Slots;
        private static SymbolId _GetInitArgs;
        private static SymbolId _GetNewArgs;
        private static SymbolId _GetState;
        private static SymbolId _SetState;
        private static SymbolId _BuildNewObject;
        private static SymbolId _Reconstructor;
        private static SymbolId _IterItems;
        private static SymbolId _RealPart;
        private static SymbolId _ImaginaryPart;
        private static SymbolId _Missing;
        private static SymbolId _With;
        private static SymbolId _WithStmt;
        private static SymbolId _Append;
        private static SymbolId _Extend;
        private static SymbolId _Update;
        private static SymbolId _ThisArgument;
        private static SymbolId _Index;
        ///<summary>Symbol for '__neg__'</summary> 
        public static SymbolId OperatorNegate {
            get {
                if (_OperatorNegate == SymbolId.Empty) _OperatorNegate = MakeSymbolId("__neg__");
                return _OperatorNegate;
            }
        }
        ///<summary>Symbol for '__invert__'</summary> 
        public static SymbolId OperatorOnesComplement {
            get {
                if (_OperatorOnesComplement == SymbolId.Empty) _OperatorOnesComplement = MakeSymbolId("__invert__");
                return _OperatorOnesComplement;
            }
        }
        ///<summary>Symbol for '__dict__'</summary> 
        public static SymbolId Dict {
            get {
                if (_Dict == SymbolId.Empty) _Dict = MakeSymbolId("__dict__");
                return _Dict;
            }
        }
        ///<summary>Symbol for '__module__'</summary> 
        public static SymbolId Module {
            get {
                if (_Module == SymbolId.Empty) _Module = MakeSymbolId("__module__");
                return _Module;
            }
        }
        ///<summary>Symbol for '__getattribute__'</summary> 
        public static SymbolId GetAttribute {
            get {
                if (_GetAttribute == SymbolId.Empty) _GetAttribute = MakeSymbolId("__getattribute__");
                return _GetAttribute;
            }
        }
        ///<summary>Symbol for '__bases__'</summary> 
        public static SymbolId Bases {
            get {
                if (_Bases == SymbolId.Empty) _Bases = MakeSymbolId("__bases__");
                return _Bases;
            }
        }
        ///<summary>Symbol for '__subclasses__'</summary> 
        public static SymbolId Subclasses {
            get {
                if (_Subclasses == SymbolId.Empty) _Subclasses = MakeSymbolId("__subclasses__");
                return _Subclasses;
            }
        }
        ///<summary>Symbol for '__name__'</summary> 
        public static SymbolId Name {
            get {
                if (_Name == SymbolId.Empty) _Name = MakeSymbolId("__name__");
                return _Name;
            }
        }
        ///<summary>Symbol for '__class__'</summary> 
        public static SymbolId Class {
            get {
                if (_Class == SymbolId.Empty) _Class = MakeSymbolId("__class__");
                return _Class;
            }
        }
        ///<summary>Symbol for '__builtins__'</summary> 
        public static SymbolId Builtins {
            get {
                if (_Builtins == SymbolId.Empty) _Builtins = MakeSymbolId("__builtins__");
                return _Builtins;
            }
        }
        ///<summary>Symbol for '__getattr__'</summary> 
        public static SymbolId GetBoundAttr {
            get {
                if (_GetBoundAttr == SymbolId.Empty) _GetBoundAttr = MakeSymbolId("__getattr__");
                return _GetBoundAttr;
            }
        }
        ///<summary>Symbol for '__setattr__'</summary> 
        public static SymbolId SetAttr {
            get {
                if (_SetAttr == SymbolId.Empty) _SetAttr = MakeSymbolId("__setattr__");
                return _SetAttr;
            }
        }
        ///<summary>Symbol for '__delattr__'</summary> 
        public static SymbolId DelAttr {
            get {
                if (_DelAttr == SymbolId.Empty) _DelAttr = MakeSymbolId("__delattr__");
                return _DelAttr;
            }
        }
        ///<summary>Symbol for '__getitem__'</summary> 
        public static SymbolId GetItem {
            get {
                if (_GetItem == SymbolId.Empty) _GetItem = MakeSymbolId("__getitem__");
                return _GetItem;
            }
        }
        ///<summary>Symbol for '__setitem__'</summary> 
        public static SymbolId SetItem {
            get {
                if (_SetItem == SymbolId.Empty) _SetItem = MakeSymbolId("__setitem__");
                return _SetItem;
            }
        }
        ///<summary>Symbol for '__delitem__'</summary> 
        public static SymbolId DelItem {
            get {
                if (_DelItem == SymbolId.Empty) _DelItem = MakeSymbolId("__delitem__");
                return _DelItem;
            }
        }
        ///<summary>Symbol for '__init__'</summary> 
        public static SymbolId Init {
            get {
                if (_Init == SymbolId.Empty) _Init = MakeSymbolId("__init__");
                return _Init;
            }
        }
        ///<summary>Symbol for '__new__'</summary> 
        public static SymbolId NewInst {
            get {
                if (_NewInst == SymbolId.Empty) _NewInst = MakeSymbolId("__new__");
                return _NewInst;
            }
        }
        ///<summary>Symbol for '__del__'</summary> 
        public static SymbolId Unassign {
            get {
                if (_Unassign == SymbolId.Empty) _Unassign = MakeSymbolId("__del__");
                return _Unassign;
            }
        }
        ///<summary>Symbol for '__str__'</summary> 
        public static SymbolId String {
            get {
                if (_String == SymbolId.Empty) _String = MakeSymbolId("__str__");
                return _String;
            }
        }
        ///<summary>Symbol for '__repr__'</summary> 
        public static SymbolId Repr {
            get {
                if (_Repr == SymbolId.Empty) _Repr = MakeSymbolId("__repr__");
                return _Repr;
            }
        }
        ///<summary>Symbol for '__contains__'</summary> 
        public static SymbolId Contains {
            get {
                if (_Contains == SymbolId.Empty) _Contains = MakeSymbolId("__contains__");
                return _Contains;
            }
        }
        ///<summary>Symbol for '__len__'</summary> 
        public static SymbolId Length {
            get {
                if (_Length == SymbolId.Empty) _Length = MakeSymbolId("__len__");
                return _Length;
            }
        }
        ///<summary>Symbol for '__reversed__'</summary> 
        public static SymbolId Reversed {
            get {
                if (_Reversed == SymbolId.Empty) _Reversed = MakeSymbolId("__reversed__");
                return _Reversed;
            }
        }
        ///<summary>Symbol for '__iter__'</summary> 
        public static SymbolId Iterator {
            get {
                if (_Iterator == SymbolId.Empty) _Iterator = MakeSymbolId("__iter__");
                return _Iterator;
            }
        }
        ///<summary>Symbol for '__next__'</summary> 
        public static SymbolId Next {
            get {
                if (_Next == SymbolId.Empty) _Next = MakeSymbolId("__next__");
                return _Next;
            }
        }
        ///<summary>Symbol for '__weakref__'</summary> 
        public static SymbolId WeakRef {
            get {
                if (_WeakRef == SymbolId.Empty) _WeakRef = MakeSymbolId("__weakref__");
                return _WeakRef;
            }
        }
        ///<summary>Symbol for '__file__'</summary> 
        public static SymbolId File {
            get {
                if (_File == SymbolId.Empty) _File = MakeSymbolId("__file__");
                return _File;
            }
        }
        ///<summary>Symbol for '__import__'</summary> 
        public static SymbolId Import {
            get {
                if (_Import == SymbolId.Empty) _Import = MakeSymbolId("__import__");
                return _Import;
            }
        }
        ///<summary>Symbol for '__doc__'</summary> 
        public static SymbolId Doc {
            get {
                if (_Doc == SymbolId.Empty) _Doc = MakeSymbolId("__doc__");
                return _Doc;
            }
        }
        ///<summary>Symbol for '__call__'</summary> 
        public static SymbolId Call {
            get {
                if (_Call == SymbolId.Empty) _Call = MakeSymbolId("__call__");
                return _Call;
            }
        }
        ///<summary>Symbol for '__abs__'</summary> 
        public static SymbolId AbsoluteValue {
            get {
                if (_AbsoluteValue == SymbolId.Empty) _AbsoluteValue = MakeSymbolId("__abs__");
                return _AbsoluteValue;
            }
        }
        ///<summary>Symbol for '__coerce__'</summary> 
        public static SymbolId Coerce {
            get {
                if (_Coerce == SymbolId.Empty) _Coerce = MakeSymbolId("__coerce__");
                return _Coerce;
            }
        }
        ///<summary>Symbol for '__int__'</summary> 
        public static SymbolId ConvertToInt {
            get {
                if (_ConvertToInt == SymbolId.Empty) _ConvertToInt = MakeSymbolId("__int__");
                return _ConvertToInt;
            }
        }
        ///<summary>Symbol for '__float__'</summary> 
        public static SymbolId ConvertToFloat {
            get {
                if (_ConvertToFloat == SymbolId.Empty) _ConvertToFloat = MakeSymbolId("__float__");
                return _ConvertToFloat;
            }
        }
        ///<summary>Symbol for '__long__'</summary> 
        public static SymbolId ConvertToLong {
            get {
                if (_ConvertToLong == SymbolId.Empty) _ConvertToLong = MakeSymbolId("__long__");
                return _ConvertToLong;
            }
        }
        ///<summary>Symbol for '__complex__'</summary> 
        public static SymbolId ConvertToComplex {
            get {
                if (_ConvertToComplex == SymbolId.Empty) _ConvertToComplex = MakeSymbolId("__complex__");
                return _ConvertToComplex;
            }
        }
        ///<summary>Symbol for '__hex__'</summary> 
        public static SymbolId ConvertToHex {
            get {
                if (_ConvertToHex == SymbolId.Empty) _ConvertToHex = MakeSymbolId("__hex__");
                return _ConvertToHex;
            }
        }
        ///<summary>Symbol for '__oct__'</summary> 
        public static SymbolId ConvertToOctal {
            get {
                if (_ConvertToOctal == SymbolId.Empty) _ConvertToOctal = MakeSymbolId("__oct__");
                return _ConvertToOctal;
            }
        }
        ///<summary>Symbol for '__reduce__'</summary> 
        public static SymbolId Reduce {
            get {
                if (_Reduce == SymbolId.Empty) _Reduce = MakeSymbolId("__reduce__");
                return _Reduce;
            }
        }
        ///<summary>Symbol for '__reduce_ex__'</summary> 
        public static SymbolId ReduceExtended {
            get {
                if (_ReduceExtended == SymbolId.Empty) _ReduceExtended = MakeSymbolId("__reduce_ex__");
                return _ReduceExtended;
            }
        }
        ///<summary>Symbol for '__nonzero__'</summary> 
        public static SymbolId NonZero {
            get {
                if (_NonZero == SymbolId.Empty) _NonZero = MakeSymbolId("__nonzero__");
                return _NonZero;
            }
        }
        ///<summary>Symbol for '__pos__'</summary> 
        public static SymbolId Positive {
            get {
                if (_Positive == SymbolId.Empty) _Positive = MakeSymbolId("__pos__");
                return _Positive;
            }
        }
        ///<summary>Symbol for '__hash__'</summary> 
        public static SymbolId Hash {
            get {
                if (_Hash == SymbolId.Empty) _Hash = MakeSymbolId("__hash__");
                return _Hash;
            }
        }
        ///<summary>Symbol for '__cmp__'</summary> 
        public static SymbolId Cmp {
            get {
                if (_Cmp == SymbolId.Empty) _Cmp = MakeSymbolId("__cmp__");
                return _Cmp;
            }
        }
        ///<summary>Symbol for '__divmod__'</summary> 
        public static SymbolId DivMod {
            get {
                if (_DivMod == SymbolId.Empty) _DivMod = MakeSymbolId("__divmod__");
                return _DivMod;
            }
        }
        ///<summary>Symbol for '__rdivmod__'</summary> 
        public static SymbolId ReverseDivMod {
            get {
                if (_ReverseDivMod == SymbolId.Empty) _ReverseDivMod = MakeSymbolId("__rdivmod__");
                return _ReverseDivMod;
            }
        }
        ///<summary>Symbol for '__path__'</summary> 
        public static SymbolId Path {
            get {
                if (_Path == SymbolId.Empty) _Path = MakeSymbolId("__path__");
                return _Path;
            }
        }
        ///<summary>Symbol for '__get__'</summary> 
        public static SymbolId GetDescriptor {
            get {
                if (_GetDescriptor == SymbolId.Empty) _GetDescriptor = MakeSymbolId("__get__");
                return _GetDescriptor;
            }
        }
        ///<summary>Symbol for '__set__'</summary> 
        public static SymbolId SetDescriptor {
            get {
                if (_SetDescriptor == SymbolId.Empty) _SetDescriptor = MakeSymbolId("__set__");
                return _SetDescriptor;
            }
        }
        ///<summary>Symbol for '__delete__'</summary> 
        public static SymbolId DeleteDescriptor {
            get {
                if (_DeleteDescriptor == SymbolId.Empty) _DeleteDescriptor = MakeSymbolId("__delete__");
                return _DeleteDescriptor;
            }
        }
        ///<summary>Symbol for '__all__'</summary> 
        public static SymbolId All {
            get {
                if (_All == SymbolId.Empty) _All = MakeSymbolId("__all__");
                return _All;
            }
        }
        ///<summary>Symbol for 'clsException'</summary> 
        public static SymbolId ClrExceptionKey {
            get {
                if (_ClrExceptionKey == SymbolId.Empty) _ClrExceptionKey = MakeSymbolId("clsException");
                return _ClrExceptionKey;
            }
        }
        ///<summary>Symbol for 'keys'</summary> 
        public static SymbolId Keys {
            get {
                if (_Keys == SymbolId.Empty) _Keys = MakeSymbolId("keys");
                return _Keys;
            }
        }
        ///<summary>Symbol for 'args'</summary> 
        public static SymbolId Arguments {
            get {
                if (_Arguments == SymbolId.Empty) _Arguments = MakeSymbolId("args");
                return _Arguments;
            }
        }
        ///<summary>Symbol for 'write'</summary> 
        public static SymbolId ConsoleWrite {
            get {
                if (_ConsoleWrite == SymbolId.Empty) _ConsoleWrite = MakeSymbolId("write");
                return _ConsoleWrite;
            }
        }
        ///<summary>Symbol for 'readline'</summary> 
        public static SymbolId ConsoleReadLine {
            get {
                if (_ConsoleReadLine == SymbolId.Empty) _ConsoleReadLine = MakeSymbolId("readline");
                return _ConsoleReadLine;
            }
        }
        ///<summary>Symbol for 'msg'</summary> 
        public static SymbolId ExceptionMessage {
            get {
                if (_ExceptionMessage == SymbolId.Empty) _ExceptionMessage = MakeSymbolId("msg");
                return _ExceptionMessage;
            }
        }
        ///<summary>Symbol for 'filename'</summary> 
        public static SymbolId ExceptionFilename {
            get {
                if (_ExceptionFilename == SymbolId.Empty) _ExceptionFilename = MakeSymbolId("filename");
                return _ExceptionFilename;
            }
        }
        ///<summary>Symbol for 'lineno'</summary> 
        public static SymbolId ExceptionLineNumber {
            get {
                if (_ExceptionLineNumber == SymbolId.Empty) _ExceptionLineNumber = MakeSymbolId("lineno");
                return _ExceptionLineNumber;
            }
        }
        ///<summary>Symbol for 'offset'</summary> 
        public static SymbolId ExceptionOffset {
            get {
                if (_ExceptionOffset == SymbolId.Empty) _ExceptionOffset = MakeSymbolId("offset");
                return _ExceptionOffset;
            }
        }
        ///<summary>Symbol for 'text'</summary> 
        public static SymbolId Text {
            get {
                if (_Text == SymbolId.Empty) _Text = MakeSymbolId("text");
                return _Text;
            }
        }
        ///<summary>Symbol for 'softspace'</summary> 
        public static SymbolId Softspace {
            get {
                if (_Softspace == SymbolId.Empty) _Softspace = MakeSymbolId("softspace");
                return _Softspace;
            }
        }
        ///<summary>Symbol for 'next'</summary> 
        public static SymbolId GeneratorNext {
            get {
                if (_GeneratorNext == SymbolId.Empty) _GeneratorNext = MakeSymbolId("next");
                return _GeneratorNext;
            }
        }
        ///<summary>Symbol for 'setdefaultencoding'</summary> 
        public static SymbolId SetDefaultEncoding {
            get {
                if (_SetDefaultEncoding == SymbolId.Empty) _SetDefaultEncoding = MakeSymbolId("setdefaultencoding");
                return _SetDefaultEncoding;
            }
        }
        ///<summary>Symbol for 'exitfunc'</summary> 
        public static SymbolId SysExitFunc {
            get {
                if (_SysExitFunc == SymbolId.Empty) _SysExitFunc = MakeSymbolId("exitfunc");
                return _SysExitFunc;
            }
        }
        ///<summary>Symbol for 'None'</summary> 
        public static SymbolId None {
            get {
                if (_None == SymbolId.Empty) _None = MakeSymbolId("None");
                return _None;
            }
        }
        ///<summary>Symbol for '__metaclass__'</summary> 
        public static SymbolId MetaClass {
            get {
                if (_MetaClass == SymbolId.Empty) _MetaClass = MakeSymbolId("__metaclass__");
                return _MetaClass;
            }
        }
        ///<summary>Symbol for '__mro__'</summary> 
        public static SymbolId MethodResolutionOrder {
            get {
                if (_MethodResolutionOrder == SymbolId.Empty) _MethodResolutionOrder = MakeSymbolId("__mro__");
                return _MethodResolutionOrder;
            }
        }
        ///<summary>Symbol for '__getslice__'</summary> 
        public static SymbolId GetSlice {
            get {
                if (_GetSlice == SymbolId.Empty) _GetSlice = MakeSymbolId("__getslice__");
                return _GetSlice;
            }
        }
        ///<summary>Symbol for '__setslice__'</summary> 
        public static SymbolId SetSlice {
            get {
                if (_SetSlice == SymbolId.Empty) _SetSlice = MakeSymbolId("__setslice__");
                return _SetSlice;
            }
        }
        ///<summary>Symbol for '__delslice__'</summary> 
        public static SymbolId DeleteSlice {
            get {
                if (_DeleteSlice == SymbolId.Empty) _DeleteSlice = MakeSymbolId("__delslice__");
                return _DeleteSlice;
            }
        }
        ///<summary>Symbol for '__future__'</summary> 
        public static SymbolId Future {
            get {
                if (_Future == SymbolId.Empty) _Future = MakeSymbolId("__future__");
                return _Future;
            }
        }
        ///<summary>Symbol for 'division'</summary> 
        public static SymbolId Division {
            get {
                if (_Division == SymbolId.Empty) _Division = MakeSymbolId("division");
                return _Division;
            }
        }
        ///<summary>Symbol for 'nested_scopes'</summary> 
        public static SymbolId NestedScopes {
            get {
                if (_NestedScopes == SymbolId.Empty) _NestedScopes = MakeSymbolId("nested_scopes");
                return _NestedScopes;
            }
        }
        ///<summary>Symbol for 'generators'</summary> 
        public static SymbolId Generators {
            get {
                if (_Generators == SymbolId.Empty) _Generators = MakeSymbolId("generators");
                return _Generators;
            }
        }
        ///<summary>Symbol for 'as'</summary> 
        public static SymbolId As {
            get {
                if (_As == SymbolId.Empty) _As = MakeSymbolId("as");
                return _As;
            }
        }
        ///<summary>Symbol for '*'</summary> 
        public static SymbolId Star {
            get {
                if (_Star == SymbolId.Empty) _Star = MakeSymbolId("*");
                return _Star;
            }
        }
        ///<summary>Symbol for '**'</summary> 
        public static SymbolId StarStar {
            get {
                if (_StarStar == SymbolId.Empty) _StarStar = MakeSymbolId("**");
                return _StarStar;
            }
        }
        ///<summary>Symbol for 'locals'</summary> 
        public static SymbolId Locals {
            get {
                if (_Locals == SymbolId.Empty) _Locals = MakeSymbolId("locals");
                return _Locals;
            }
        }
        ///<summary>Symbol for 'vars'</summary> 
        public static SymbolId Vars {
            get {
                if (_Vars == SymbolId.Empty) _Vars = MakeSymbolId("vars");
                return _Vars;
            }
        }
        ///<summary>Symbol for 'dir'</summary> 
        public static SymbolId Dir {
            get {
                if (_Dir == SymbolId.Empty) _Dir = MakeSymbolId("dir");
                return _Dir;
            }
        }
        ///<summary>Symbol for 'eval'</summary> 
        public static SymbolId Eval {
            get {
                if (_Eval == SymbolId.Empty) _Eval = MakeSymbolId("eval");
                return _Eval;
            }
        }
        ///<summary>Symbol for 'execfile'</summary> 
        public static SymbolId ExecFile {
            get {
                if (_ExecFile == SymbolId.Empty) _ExecFile = MakeSymbolId("execfile");
                return _ExecFile;
            }
        }
        ///<summary>Symbol for '_'</summary> 
        public static SymbolId Underscore {
            get {
                if (_Underscore == SymbolId.Empty) _Underscore = MakeSymbolId("_");
                return _Underscore;
            }
        }
        ///<summary>Symbol for '__gen_$_parm__'</summary> 
        public static SymbolId GeneratorParmName {
            get {
                if (_GeneratorParmName == SymbolId.Empty) _GeneratorParmName = MakeSymbolId("__gen_$_parm__");
                return _GeneratorParmName;
            }
        }
        ///<summary>Symbol for '$env'</summary> 
        public static SymbolId EnvironmentParmName {
            get {
                if (_EnvironmentParmName == SymbolId.Empty) _EnvironmentParmName = MakeSymbolId("$env");
                return _EnvironmentParmName;
            }
        }
        ///<summary>Symbol for 'iter'</summary> 
        public static SymbolId Iter {
            get {
                if (_Iter == SymbolId.Empty) _Iter = MakeSymbolId("iter");
                return _Iter;
            }
        }
        ///<summary>Symbol for '__slots__'</summary> 
        public static SymbolId Slots {
            get {
                if (_Slots == SymbolId.Empty) _Slots = MakeSymbolId("__slots__");
                return _Slots;
            }
        }
        ///<summary>Symbol for '__getinitargs__'</summary> 
        public static SymbolId GetInitArgs {
            get {
                if (_GetInitArgs == SymbolId.Empty) _GetInitArgs = MakeSymbolId("__getinitargs__");
                return _GetInitArgs;
            }
        }
        ///<summary>Symbol for '__getnewargs__'</summary> 
        public static SymbolId GetNewArgs {
            get {
                if (_GetNewArgs == SymbolId.Empty) _GetNewArgs = MakeSymbolId("__getnewargs__");
                return _GetNewArgs;
            }
        }
        ///<summary>Symbol for '__getstate__'</summary> 
        public static SymbolId GetState {
            get {
                if (_GetState == SymbolId.Empty) _GetState = MakeSymbolId("__getstate__");
                return _GetState;
            }
        }
        ///<summary>Symbol for '__setstate__'</summary> 
        public static SymbolId SetState {
            get {
                if (_SetState == SymbolId.Empty) _SetState = MakeSymbolId("__setstate__");
                return _SetState;
            }
        }
        ///<summary>Symbol for '__newobj__'</summary> 
        public static SymbolId BuildNewObject {
            get {
                if (_BuildNewObject == SymbolId.Empty) _BuildNewObject = MakeSymbolId("__newobj__");
                return _BuildNewObject;
            }
        }
        ///<summary>Symbol for '_reconstructor'</summary> 
        public static SymbolId Reconstructor {
            get {
                if (_Reconstructor == SymbolId.Empty) _Reconstructor = MakeSymbolId("_reconstructor");
                return _Reconstructor;
            }
        }
        ///<summary>Symbol for 'iteritems'</summary> 
        public static SymbolId IterItems {
            get {
                if (_IterItems == SymbolId.Empty) _IterItems = MakeSymbolId("iteritems");
                return _IterItems;
            }
        }
        ///<summary>Symbol for 'real'</summary> 
        public static SymbolId RealPart {
            get {
                if (_RealPart == SymbolId.Empty) _RealPart = MakeSymbolId("real");
                return _RealPart;
            }
        }
        ///<summary>Symbol for 'imag'</summary> 
        public static SymbolId ImaginaryPart {
            get {
                if (_ImaginaryPart == SymbolId.Empty) _ImaginaryPart = MakeSymbolId("imag");
                return _ImaginaryPart;
            }
        }
        ///<summary>Symbol for '__missing__'</summary> 
        public static SymbolId Missing {
            get {
                if (_Missing == SymbolId.Empty) _Missing = MakeSymbolId("__missing__");
                return _Missing;
            }
        }
        ///<summary>Symbol for 'with'</summary> 
        public static SymbolId With {
            get {
                if (_With == SymbolId.Empty) _With = MakeSymbolId("with");
                return _With;
            }
        }
        ///<summary>Symbol for 'with_statement'</summary> 
        public static SymbolId WithStmt {
            get {
                if (_WithStmt == SymbolId.Empty) _WithStmt = MakeSymbolId("with_statement");
                return _WithStmt;
            }
        }
        ///<summary>Symbol for 'append'</summary> 
        public static SymbolId Append {
            get {
                if (_Append == SymbolId.Empty) _Append = MakeSymbolId("append");
                return _Append;
            }
        }
        ///<summary>Symbol for 'extend'</summary> 
        public static SymbolId Extend {
            get {
                if (_Extend == SymbolId.Empty) _Extend = MakeSymbolId("extend");
                return _Extend;
            }
        }
        ///<summary>Symbol for 'update'</summary> 
        public static SymbolId Update {
            get {
                if (_Update == SymbolId.Empty) _Update = MakeSymbolId("update");
                return _Update;
            }
        }
        ///<summary>Symbol for 'this'</summary> 
        public static SymbolId ThisArgument {
            get {
                if (_ThisArgument == SymbolId.Empty) _ThisArgument = MakeSymbolId("this");
                return _ThisArgument;
            }
        }
        ///<summary>Symbol for '__index__'</summary> 
        public static SymbolId Index {
            get {
                if (_Index == SymbolId.Empty) _Index = MakeSymbolId("__index__");
                return _Index;
            }
        }

        // *** END GENERATED CODE ***

        #endregion

        public static SymbolId OperatorToSymbol(Operators op) {
            switch (op) {
                #region Generated OperatorToSymbol

                // *** BEGIN GENERATED CODE ***

                case Operators.Add: return Symbols.OperatorAdd;
                case Operators.ReverseAdd: return Symbols.OperatorReverseAdd;
                case Operators.InPlaceAdd: return Symbols.OperatorInPlaceAdd;
                case Operators.Subtract: return Symbols.OperatorSubtract;
                case Operators.ReverseSubtract: return Symbols.OperatorReverseSubtract;
                case Operators.InPlaceSubtract: return Symbols.OperatorInPlaceSubtract;
                case Operators.Power: return Symbols.OperatorPower;
                case Operators.ReversePower: return Symbols.OperatorReversePower;
                case Operators.InPlacePower: return Symbols.OperatorInPlacePower;
                case Operators.Multiply: return Symbols.OperatorMultiply;
                case Operators.ReverseMultiply: return Symbols.OperatorReverseMultiply;
                case Operators.InPlaceMultiply: return Symbols.OperatorInPlaceMultiply;
                case Operators.FloorDivide: return Symbols.OperatorFloorDivide;
                case Operators.ReverseFloorDivide: return Symbols.OperatorReverseFloorDivide;
                case Operators.InPlaceFloorDivide: return Symbols.OperatorInPlaceFloorDivide;
                case Operators.Divide: return Symbols.OperatorDivide;
                case Operators.ReverseDivide: return Symbols.OperatorReverseDivide;
                case Operators.InPlaceDivide: return Symbols.OperatorInPlaceDivide;
                case Operators.TrueDivide: return Symbols.OperatorTrueDivide;
                case Operators.ReverseTrueDivide: return Symbols.OperatorReverseTrueDivide;
                case Operators.InPlaceTrueDivide: return Symbols.OperatorInPlaceTrueDivide;
                case Operators.Mod: return Symbols.OperatorMod;
                case Operators.ReverseMod: return Symbols.OperatorReverseMod;
                case Operators.InPlaceMod: return Symbols.OperatorInPlaceMod;
                case Operators.LeftShift: return Symbols.OperatorLeftShift;
                case Operators.ReverseLeftShift: return Symbols.OperatorReverseLeftShift;
                case Operators.InPlaceLeftShift: return Symbols.OperatorInPlaceLeftShift;
                case Operators.RightShift: return Symbols.OperatorRightShift;
                case Operators.ReverseRightShift: return Symbols.OperatorReverseRightShift;
                case Operators.InPlaceRightShift: return Symbols.OperatorInPlaceRightShift;
                case Operators.BitwiseAnd: return Symbols.OperatorBitwiseAnd;
                case Operators.ReverseBitwiseAnd: return Symbols.OperatorReverseBitwiseAnd;
                case Operators.InPlaceBitwiseAnd: return Symbols.OperatorInPlaceBitwiseAnd;
                case Operators.BitwiseOr: return Symbols.OperatorBitwiseOr;
                case Operators.ReverseBitwiseOr: return Symbols.OperatorReverseBitwiseOr;
                case Operators.InPlaceBitwiseOr: return Symbols.OperatorInPlaceBitwiseOr;
                case Operators.Xor: return Symbols.OperatorXor;
                case Operators.ReverseXor: return Symbols.OperatorReverseXor;
                case Operators.InPlaceXor: return Symbols.OperatorInPlaceXor;
                case Operators.LessThan: return Symbols.OperatorLessThan;
                case Operators.GreaterThan: return Symbols.OperatorGreaterThan;
                case Operators.LessThanOrEqual: return Symbols.OperatorLessThanOrEqual;
                case Operators.GreaterThanOrEqual: return Symbols.OperatorGreaterThanOrEqual;
                case Operators.Equals: return Symbols.OperatorEquals;
                case Operators.NotEquals: return Symbols.OperatorNotEquals;
                case Operators.LessThanGreaterThan: return Symbols.OperatorLessThanGreaterThan;

                // *** END GENERATED CODE ***

                #endregion

                // unary operators
                case Operators.OnesComplement: return Symbols.OperatorOnesComplement;
                case Operators.Negate: return Symbols.OperatorNegate;
                case Operators.Positive: return Symbols.Positive;
                case Operators.AbsoluteValue: return Symbols.AbsoluteValue;
                case Operators.ConvertToBigInteger: return Symbols.ConvertToLong;
                case Operators.ConvertToBoolean: return Symbols.NonZero;
                case Operators.ConvertToComplex: return Symbols.ConvertToComplex;
                case Operators.ConvertToDouble: return Symbols.ConvertToFloat;
                case Operators.ConvertToHex: return Symbols.ConvertToHex;
                case Operators.ConvertToInt32: return Symbols.ConvertToInt;
                case Operators.ConvertToOctal: return Symbols.ConvertToOctal;
                case Operators.ConvertToString: return Symbols.String;

                default:
                    throw new InvalidOperationException(op.ToString());
            }
        }

        public static SymbolId OperatorToReversedSymbol(Operators op) {
            switch(op){
                #region Generated OperatorToReversedSymbol

                // *** BEGIN GENERATED CODE ***

                case Operators.Add: return Symbols.OperatorReverseAdd;
                case Operators.Subtract: return Symbols.OperatorReverseSubtract;
                case Operators.Power: return Symbols.OperatorReversePower;
                case Operators.Multiply: return Symbols.OperatorReverseMultiply;
                case Operators.FloorDivide: return Symbols.OperatorReverseFloorDivide;
                case Operators.Divide: return Symbols.OperatorReverseDivide;
                case Operators.TrueDivide: return Symbols.OperatorReverseTrueDivide;
                case Operators.Mod: return Symbols.OperatorReverseMod;
                case Operators.LeftShift: return Symbols.OperatorReverseLeftShift;
                case Operators.RightShift: return Symbols.OperatorReverseRightShift;
                case Operators.BitwiseAnd: return Symbols.OperatorReverseBitwiseAnd;
                case Operators.BitwiseOr: return Symbols.OperatorReverseBitwiseOr;
                case Operators.Xor: return Symbols.OperatorReverseXor;
                case Operators.LessThan: return Symbols.OperatorGreaterThan;
                case Operators.LessThanOrEqual: return Symbols.OperatorGreaterThanOrEqual;
                case Operators.GreaterThan: return Symbols.OperatorLessThan;
                case Operators.GreaterThanOrEqual: return Symbols.OperatorLessThanOrEqual;
                case Operators.Equals: return Symbols.OperatorEquals;
                case Operators.NotEquals: return Symbols.OperatorNotEquals;

                // *** END GENERATED CODE ***

                #endregion
                default:
                    throw new InvalidOperationException();
            }
        }

        
    }
}
