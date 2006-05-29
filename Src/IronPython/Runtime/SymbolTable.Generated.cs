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

namespace IronPython.Runtime {
    static partial class SymbolTable {
        #region Generated SymbolTable Ops Symbols

        // *** BEGIN GENERATED CODE ***

        public static readonly SymbolId OpAdd = new SymbolId(OpAddId);
        public static readonly SymbolId OpReverseAdd = new SymbolId(OpReverseAddId);
        public static readonly SymbolId OpInPlaceAdd = new SymbolId(OpInPlaceAddId);
        public static readonly SymbolId OpSubtract = new SymbolId(OpSubtractId);
        public static readonly SymbolId OpReverseSubtract = new SymbolId(OpReverseSubtractId);
        public static readonly SymbolId OpInPlaceSubtract = new SymbolId(OpInPlaceSubtractId);
        public static readonly SymbolId OpPower = new SymbolId(OpPowerId);
        public static readonly SymbolId OpReversePower = new SymbolId(OpReversePowerId);
        public static readonly SymbolId OpInPlacePower = new SymbolId(OpInPlacePowerId);
        public static readonly SymbolId OpMultiply = new SymbolId(OpMultiplyId);
        public static readonly SymbolId OpReverseMultiply = new SymbolId(OpReverseMultiplyId);
        public static readonly SymbolId OpInPlaceMultiply = new SymbolId(OpInPlaceMultiplyId);
        public static readonly SymbolId OpFloorDivide = new SymbolId(OpFloorDivideId);
        public static readonly SymbolId OpReverseFloorDivide = new SymbolId(OpReverseFloorDivideId);
        public static readonly SymbolId OpInPlaceFloorDivide = new SymbolId(OpInPlaceFloorDivideId);
        public static readonly SymbolId OpDivide = new SymbolId(15);
        public static readonly SymbolId OpReverseDivide = new SymbolId(16);
        public static readonly SymbolId OpInPlaceDivide = new SymbolId(17);
        public static readonly SymbolId OpTrueDivide = new SymbolId(18);
        public static readonly SymbolId OpReverseTrueDivide = new SymbolId(19);
        public static readonly SymbolId OpInPlaceTrueDivide = new SymbolId(20);
        public static readonly SymbolId OpMod = new SymbolId(OpModId);
        public static readonly SymbolId OpReverseMod = new SymbolId(OpReverseModId);
        public static readonly SymbolId OpInPlaceMod = new SymbolId(OpInPlaceModId);
        public static readonly SymbolId OpLeftShift = new SymbolId(OpLeftShiftId);
        public static readonly SymbolId OpReverseLeftShift = new SymbolId(OpReverseLeftShiftId);
        public static readonly SymbolId OpInPlaceLeftShift = new SymbolId(OpInPlaceLeftShiftId);
        public static readonly SymbolId OpRightShift = new SymbolId(OpRightShiftId);
        public static readonly SymbolId OpReverseRightShift = new SymbolId(OpReverseRightShiftId);
        public static readonly SymbolId OpInPlaceRightShift = new SymbolId(OpInPlaceRightShiftId);
        public static readonly SymbolId OpBitwiseAnd = new SymbolId(OpBitwiseAndId);
        public static readonly SymbolId OpReverseBitwiseAnd = new SymbolId(OpReverseBitwiseAndId);
        public static readonly SymbolId OpInPlaceBitwiseAnd = new SymbolId(OpInPlaceBitwiseAndId);
        public static readonly SymbolId OpBitwiseOr = new SymbolId(OpBitwiseOrId);
        public static readonly SymbolId OpReverseBitwiseOr = new SymbolId(OpReverseBitwiseOrId);
        public static readonly SymbolId OpInPlaceBitwiseOr = new SymbolId(OpInPlaceBitwiseOrId);
        public static readonly SymbolId OpXor = new SymbolId(OpXorId);
        public static readonly SymbolId OpReverseXor = new SymbolId(OpReverseXorId);
        public static readonly SymbolId OpInPlaceXor = new SymbolId(OpInPlaceXorId);
        public static readonly SymbolId OpLessThan = new SymbolId(OpLessThanId);
        public static readonly SymbolId OpGreaterThan = new SymbolId(OpGreaterThanId);
        public static readonly SymbolId OpLessThanOrEqual = new SymbolId(OpLessThanOrEqualId);
        public static readonly SymbolId OpGreaterThanOrEqual = new SymbolId(OpGreaterThanOrEqualId);
        public static readonly SymbolId OpEqual = new SymbolId(OpEqualId);
        public static readonly SymbolId OpNotEqual = new SymbolId(OpNotEqualId);
        public static readonly SymbolId OpLessThanGreaterThan = new SymbolId(OpLessThanGreaterThanId);

        // *** END GENERATED CODE ***

        #endregion

        #region Generated SymbolTable Other Symbols

        // *** BEGIN GENERATED CODE ***

        public static readonly SymbolId OpNegate = new SymbolId(OpNegateId);
        public static readonly SymbolId OpOnesComplement = new SymbolId(OpOnesComplementId);
        public static readonly SymbolId Dict = new SymbolId(DictId);
        public static readonly SymbolId Module = new SymbolId(ModuleId);
        public static readonly SymbolId GetAttribute = new SymbolId(GetAttributeId);
        public static readonly SymbolId Bases = new SymbolId(BasesId);
        public static readonly SymbolId Subclasses = new SymbolId(SubclassesId);
        public static readonly SymbolId Name = new SymbolId(NameId);
        public static readonly SymbolId Class = new SymbolId(ClassId);
        public static readonly SymbolId Builtins = new SymbolId(BuiltinsId);
        public static readonly SymbolId GetAttr = new SymbolId(GetAttrId);
        public static readonly SymbolId SetAttr = new SymbolId(SetAttrId);
        public static readonly SymbolId DelAttr = new SymbolId(DelAttrId);
        public static readonly SymbolId GetItem = new SymbolId(GetItemId);
        public static readonly SymbolId SetItem = new SymbolId(SetItemId);
        public static readonly SymbolId DelItem = new SymbolId(DelItemId);
        public static readonly SymbolId Init = new SymbolId(InitId);
        public static readonly SymbolId NewInst = new SymbolId(NewInstId);
        public static readonly SymbolId Unassign = new SymbolId(UnassignId);
        public static readonly SymbolId String = new SymbolId(StringId);
        public static readonly SymbolId Repr = new SymbolId(ReprId);
        public static readonly SymbolId Contains = new SymbolId(ContainsId);
        public static readonly SymbolId Length = new SymbolId(LengthId);
        public static readonly SymbolId Reversed = new SymbolId(ReversedId);
        public static readonly SymbolId Iterator = new SymbolId(IteratorId);
        public static readonly SymbolId Next = new SymbolId(NextId);
        public static readonly SymbolId WeakRef = new SymbolId(WeakRefId);
        public static readonly SymbolId File = new SymbolId(FileId);
        public static readonly SymbolId Import = new SymbolId(ImportId);
        public static readonly SymbolId Doc = new SymbolId(DocId);
        public static readonly SymbolId Call = new SymbolId(CallId);
        public static readonly SymbolId AbsoluteValue = new SymbolId(AbsoluteValueId);
        public static readonly SymbolId Coerce = new SymbolId(CoerceId);
        public static readonly SymbolId ConvertToInt = new SymbolId(ConvertToIntId);
        public static readonly SymbolId ConvertToFloat = new SymbolId(ConvertToFloatId);
        public static readonly SymbolId ConvertToLong = new SymbolId(ConvertToLongId);
        public static readonly SymbolId ConvertToComplex = new SymbolId(ConvertToComplexId);
        public static readonly SymbolId ConvertToHex = new SymbolId(ConvertToHexId);
        public static readonly SymbolId ConvertToOctal = new SymbolId(ConvertToOctalId);
        public static readonly SymbolId NonZero = new SymbolId(NonZeroId);
        public static readonly SymbolId Positive = new SymbolId(PositiveId);
        public static readonly SymbolId Hash = new SymbolId(HashId);
        public static readonly SymbolId Cmp = new SymbolId(CmpId);
        public static readonly SymbolId Path = new SymbolId(PathId);
        public static readonly SymbolId GetDescriptor = new SymbolId(GetDescriptorId);
        public static readonly SymbolId SetDescriptor = new SymbolId(SetDescriptorId);
        public static readonly SymbolId DeleteDescriptor = new SymbolId(DeleteDescriptorId);
        public static readonly SymbolId All = new SymbolId(AllId);
        public static readonly SymbolId ClrExceptionKey = new SymbolId(ClrExceptionKeyId);
        public static readonly SymbolId Keys = new SymbolId(KeysId);
        public static readonly SymbolId Arguments = new SymbolId(ArgumentsId);
        public static readonly SymbolId ConsoleWrite = new SymbolId(ConsoleWriteId);
        public static readonly SymbolId ConsoleReadLine = new SymbolId(ConsoleReadLineId);
        public static readonly SymbolId ExceptionMessage = new SymbolId(ExceptionMessageId);
        public static readonly SymbolId ExceptionFilename = new SymbolId(ExceptionFilenameId);
        public static readonly SymbolId ExceptionLineNumber = new SymbolId(ExceptionLineNumberId);
        public static readonly SymbolId ExceptionOffset = new SymbolId(ExceptionOffsetId);
        public static readonly SymbolId Text = new SymbolId(TextId);
        public static readonly SymbolId Softspace = new SymbolId(SoftspaceId);
        public static readonly SymbolId GeneratorNext = new SymbolId(GeneratorNextId);
        public static readonly SymbolId SetDefaultEncoding = new SymbolId(SetDefaultEncodingId);
        public static readonly SymbolId SysExitFunc = new SymbolId(SysExitFuncId);
        public static readonly SymbolId MetaClass = new SymbolId(MetaClassId);
        public static readonly SymbolId MethodResolutionOrder = new SymbolId(MethodResolutionOrderId);
        public static readonly SymbolId GetSlice = new SymbolId(GetSliceId);
        public static readonly SymbolId SetSlice = new SymbolId(SetSliceId);
        public static readonly SymbolId DeleteSlice = new SymbolId(DeleteSliceId);
        public static readonly SymbolId OpPositive = new SymbolId(OpPositiveId);
        public static readonly SymbolId LastWellKnownId = new SymbolId(LastWellKnownIdId);

        // *** END GENERATED CODE ***

        #endregion

        // well known IDs, changing these breaks binary compatibility.
        #region Generated SymbolTable Ops Values

        // *** BEGIN GENERATED CODE ***

        public const int OpAddId = 0;
        public const int OpReverseAddId = 1;
        public const int OpInPlaceAddId = 2;
        public const int OpSubtractId = 3;
        public const int OpReverseSubtractId = 4;
        public const int OpInPlaceSubtractId = 5;
        public const int OpPowerId = 6;
        public const int OpReversePowerId = 7;
        public const int OpInPlacePowerId = 8;
        public const int OpMultiplyId = 9;
        public const int OpReverseMultiplyId = 10;
        public const int OpInPlaceMultiplyId = 11;
        public const int OpFloorDivideId = 12;
        public const int OpReverseFloorDivideId = 13;
        public const int OpInPlaceFloorDivideId = 14;
        public const int OpDivideId = 15;
        public const int OpReverseDivideId = 16;
        public const int OpInPlaceDivideId = 17;
        public const int OpTrueDivideId = 18;
        public const int OpReverseTrueDivideId = 19;
        public const int OpInPlaceTrueDivideId = 20;
        public const int OpModId = 21;
        public const int OpReverseModId = 22;
        public const int OpInPlaceModId = 23;
        public const int OpLeftShiftId = 24;
        public const int OpReverseLeftShiftId = 25;
        public const int OpInPlaceLeftShiftId = 26;
        public const int OpRightShiftId = 27;
        public const int OpReverseRightShiftId = 28;
        public const int OpInPlaceRightShiftId = 29;
        public const int OpBitwiseAndId = 30;
        public const int OpReverseBitwiseAndId = 31;
        public const int OpInPlaceBitwiseAndId = 32;
        public const int OpBitwiseOrId = 33;
        public const int OpReverseBitwiseOrId = 34;
        public const int OpInPlaceBitwiseOrId = 35;
        public const int OpXorId = 36;
        public const int OpReverseXorId = 37;
        public const int OpInPlaceXorId = 38;
        public const int OpLessThanId = 39;
        public const int OpGreaterThanId = 40;
        public const int OpLessThanOrEqualId = 41;
        public const int OpGreaterThanOrEqualId = 42;
        public const int OpEqualId = 43;
        public const int OpNotEqualId = 44;
        public const int OpLessThanGreaterThanId = 45;

        // *** END GENERATED CODE ***

        #endregion

        #region Generated SymbolTable Other Values

        // *** BEGIN GENERATED CODE ***

        public const int OpNegateId = 46;
        public const int OpOnesComplementId = 47;
        public const int DictId = 48;
        public const int ModuleId = 49;
        public const int GetAttributeId = 50;
        public const int BasesId = 51;
        public const int SubclassesId = 52;
        public const int NameId = 53;
        public const int ClassId = 54;
        public const int BuiltinsId = 55;
        public const int GetAttrId = 56;
        public const int SetAttrId = 57;
        public const int DelAttrId = 58;
        public const int GetItemId = 59;
        public const int SetItemId = 60;
        public const int DelItemId = 61;
        public const int InitId = 62;
        public const int NewInstId = 63;
        public const int UnassignId = 64;
        public const int StringId = 65;
        public const int ReprId = 66;
        public const int ContainsId = 67;
        public const int LengthId = 68;
        public const int ReversedId = 69;
        public const int IteratorId = 70;
        public const int NextId = 71;
        public const int WeakRefId = 72;
        public const int FileId = 73;
        public const int ImportId = 74;
        public const int DocId = 75;
        public const int CallId = 76;
        public const int AbsoluteValueId = 77;
        public const int CoerceId = 78;
        public const int ConvertToIntId = 79;
        public const int ConvertToFloatId = 80;
        public const int ConvertToLongId = 81;
        public const int ConvertToComplexId = 82;
        public const int ConvertToHexId = 83;
        public const int ConvertToOctalId = 84;
        public const int NonZeroId = 85;
        public const int PositiveId = 86;
        public const int HashId = 87;
        public const int CmpId = 88;
        public const int PathId = 89;
        public const int GetDescriptorId = 90;
        public const int SetDescriptorId = 91;
        public const int DeleteDescriptorId = 92;
        public const int AllId = 93;
        public const int ClrExceptionKeyId = 94;
        public const int KeysId = 95;
        public const int ArgumentsId = 96;
        public const int ConsoleWriteId = 97;
        public const int ConsoleReadLineId = 98;
        public const int ExceptionMessageId = 99;
        public const int ExceptionFilenameId = 100;
        public const int ExceptionLineNumberId = 101;
        public const int ExceptionOffsetId = 102;
        public const int TextId = 103;
        public const int SoftspaceId = 104;
        public const int GeneratorNextId = 105;
        public const int SetDefaultEncodingId = 106;
        public const int SysExitFuncId = 107;
        public const int MetaClassId = 108;
        public const int MethodResolutionOrderId = 109;
        public const int GetSliceId = 110;
        public const int SetSliceId = 111;
        public const int DeleteSliceId = 112;
        public const int OpPositiveId = 113;
        public const int LastWellKnownIdId = 114;

        // *** END GENERATED CODE ***

        #endregion

        static SymbolTable() {
            #region Generated SymbolTable Ops Added

            // *** BEGIN GENERATED CODE ***

            StringToId("__add__");  // 0 
            StringToId("__radd__");  // 1 
            StringToId("__iadd__");  // 2 
            StringToId("__sub__");  // 3 
            StringToId("__rsub__");  // 4 
            StringToId("__isub__");  // 5 
            StringToId("__pow__");  // 6 
            StringToId("__rpow__");  // 7 
            StringToId("__ipow__");  // 8 
            StringToId("__mul__");  // 9 
            StringToId("__rmul__");  // 10 
            StringToId("__imul__");  // 11 
            StringToId("__floordiv__");  // 12 
            StringToId("__rfloordiv__");  // 13 
            StringToId("__ifloordiv__");  // 14 
            StringToId("__div__");  // 15 
            StringToId("__rdiv__");  // 16 
            StringToId("__idiv__");  // 17 
            StringToId("__truediv__");  // 18 
            StringToId("__rtruediv__");  // 19 
            StringToId("__itruediv__");  // 20 
            StringToId("__mod__");  // 21 
            StringToId("__rmod__");  // 22 
            StringToId("__imod__");  // 23 
            StringToId("__lshift__");  // 24 
            StringToId("__rlshift__");  // 25 
            StringToId("__ilshift__");  // 26 
            StringToId("__rshift__");  // 27 
            StringToId("__rrshift__");  // 28 
            StringToId("__irshift__");  // 29 
            StringToId("__and__");  // 30 
            StringToId("__rand__");  // 31 
            StringToId("__iand__");  // 32 
            StringToId("__or__");  // 33 
            StringToId("__ror__");  // 34 
            StringToId("__ior__");  // 35 
            StringToId("__xor__");  // 36 
            StringToId("__rxor__");  // 37 
            StringToId("__ixor__");  // 38 
            StringToId("__lt__");  // 39 
            StringToId("__gt__");  // 40 
            StringToId("__le__");  // 41 
            StringToId("__ge__");  // 42 
            StringToId("__eq__");  // 43 
            StringToId("__ne__");  // 44 
            StringToId("__lg__");  // 45 

            // *** END GENERATED CODE ***

            #endregion

            #region Generated SymbolTable Other Added

            // *** BEGIN GENERATED CODE ***

            StringToId("__neg__");  // 46
            StringToId("__invert__");  // 47
            StringToId("__dict__");  // 48
            StringToId("__module__");  // 49
            StringToId("__getattribute__");  // 50
            StringToId("__bases__");  // 51
            StringToId("__subclasses__");  // 52
            StringToId("__name__");  // 53
            StringToId("__class__");  // 54
            StringToId("__builtins__");  // 55
            StringToId("__getattr__");  // 56
            StringToId("__setattr__");  // 57
            StringToId("__delattr__");  // 58
            StringToId("__getitem__");  // 59
            StringToId("__setitem__");  // 60
            StringToId("__delitem__");  // 61
            StringToId("__init__");  // 62
            StringToId("__new__");  // 63
            StringToId("__del__");  // 64
            StringToId("__str__");  // 65
            StringToId("__repr__");  // 66
            StringToId("__contains__");  // 67
            StringToId("__len__");  // 68
            StringToId("__reversed__");  // 69
            StringToId("__iter__");  // 70
            StringToId("__next__");  // 71
            StringToId("__weakref__");  // 72
            StringToId("__file__");  // 73
            StringToId("__import__");  // 74
            StringToId("__doc__");  // 75
            StringToId("__call__");  // 76
            StringToId("__abs__");  // 77
            StringToId("__coerce__");  // 78
            StringToId("__int__");  // 79
            StringToId("__float__");  // 80
            StringToId("__long__");  // 81
            StringToId("__complex__");  // 82
            StringToId("__hex__");  // 83
            StringToId("__oct__");  // 84
            StringToId("__nonzero__");  // 85
            StringToId("__pos__");  // 86
            StringToId("__hash__");  // 87
            StringToId("__cmp__");  // 88
            StringToId("__path__");  // 89
            StringToId("__get__");  // 90
            StringToId("__set__");  // 91
            StringToId("__delete__");  // 92
            StringToId("__all__");  // 93
            StringToId("clsException");  // 94
            StringToId("keys");  // 95
            StringToId("args");  // 96
            StringToId("write");  // 97
            StringToId("readline");  // 98
            StringToId("msg");  // 99
            StringToId("filename");  // 100
            StringToId("lineno");  // 101
            StringToId("offset");  // 102
            StringToId("text");  // 103
            StringToId("softspace");  // 104
            StringToId("next");  // 105
            StringToId("setdefaultencoding");  // 106
            StringToId("exitfunc");  // 107
            StringToId("__metaclass__");  // 108
            StringToId("__mro__");  // 109
            StringToId("__getslice__");  // 110
            StringToId("__setslice__");  // 111
            StringToId("__delslice__");  // 112
            StringToId("__pos__");  // 113
            StringToId("LastWellKnownId");  // 114

            // *** END GENERATED CODE ***

            #endregion
        }


    }
}
