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

        ///<summary>SymbolId for '__add__'</summary>
        public static readonly SymbolId OpAdd = new SymbolId(OpAddId);
        ///<summary>SymbolId for '__radd__'</summary>
        public static readonly SymbolId OpReverseAdd = new SymbolId(OpReverseAddId);
        ///<summary>SymbolId for '__iadd__'</summary>
        public static readonly SymbolId OpInPlaceAdd = new SymbolId(OpInPlaceAddId);
        ///<summary>SymbolId for '__sub__'</summary>
        public static readonly SymbolId OpSubtract = new SymbolId(OpSubtractId);
        ///<summary>SymbolId for '__rsub__'</summary>
        public static readonly SymbolId OpReverseSubtract = new SymbolId(OpReverseSubtractId);
        ///<summary>SymbolId for '__isub__'</summary>
        public static readonly SymbolId OpInPlaceSubtract = new SymbolId(OpInPlaceSubtractId);
        ///<summary>SymbolId for '__pow__'</summary>
        public static readonly SymbolId OpPower = new SymbolId(OpPowerId);
        ///<summary>SymbolId for '__rpow__'</summary>
        public static readonly SymbolId OpReversePower = new SymbolId(OpReversePowerId);
        ///<summary>SymbolId for '__ipow__'</summary>
        public static readonly SymbolId OpInPlacePower = new SymbolId(OpInPlacePowerId);
        ///<summary>SymbolId for '__mul__'</summary>
        public static readonly SymbolId OpMultiply = new SymbolId(OpMultiplyId);
        ///<summary>SymbolId for '__rmul__'</summary>
        public static readonly SymbolId OpReverseMultiply = new SymbolId(OpReverseMultiplyId);
        ///<summary>SymbolId for '__imul__'</summary>
        public static readonly SymbolId OpInPlaceMultiply = new SymbolId(OpInPlaceMultiplyId);
        ///<summary>SymbolId for '__floordiv__'</summary>
        public static readonly SymbolId OpFloorDivide = new SymbolId(OpFloorDivideId);
        ///<summary>SymbolId for '__rfloordiv__'</summary>
        public static readonly SymbolId OpReverseFloorDivide = new SymbolId(OpReverseFloorDivideId);
        ///<summary>SymbolId for '__ifloordiv__'</summary>
        public static readonly SymbolId OpInPlaceFloorDivide = new SymbolId(OpInPlaceFloorDivideId);
        ///<summary>SymbolId for '__div__'</summary>
        public static readonly SymbolId OpDivide = new SymbolId(OpDivideId);
        ///<summary>SymbolId for '__rdiv__'</summary>
        public static readonly SymbolId OpReverseDivide = new SymbolId(OpReverseDivideId);
        ///<summary>SymbolId for '__idiv__'</summary>
        public static readonly SymbolId OpInPlaceDivide = new SymbolId(OpInPlaceDivideId);
        ///<summary>SymbolId for '__truediv__'</summary>
        public static readonly SymbolId OpTrueDivide = new SymbolId(OpTrueDivideId);
        ///<summary>SymbolId for '__rtruediv__'</summary>
        public static readonly SymbolId OpReverseTrueDivide = new SymbolId(OpReverseTrueDivideId);
        ///<summary>SymbolId for '__itruediv__'</summary>
        public static readonly SymbolId OpInPlaceTrueDivide = new SymbolId(OpInPlaceTrueDivideId);
        ///<summary>SymbolId for '__mod__'</summary>
        public static readonly SymbolId OpMod = new SymbolId(OpModId);
        ///<summary>SymbolId for '__rmod__'</summary>
        public static readonly SymbolId OpReverseMod = new SymbolId(OpReverseModId);
        ///<summary>SymbolId for '__imod__'</summary>
        public static readonly SymbolId OpInPlaceMod = new SymbolId(OpInPlaceModId);
        ///<summary>SymbolId for '__lshift__'</summary>
        public static readonly SymbolId OpLeftShift = new SymbolId(OpLeftShiftId);
        ///<summary>SymbolId for '__rlshift__'</summary>
        public static readonly SymbolId OpReverseLeftShift = new SymbolId(OpReverseLeftShiftId);
        ///<summary>SymbolId for '__ilshift__'</summary>
        public static readonly SymbolId OpInPlaceLeftShift = new SymbolId(OpInPlaceLeftShiftId);
        ///<summary>SymbolId for '__rshift__'</summary>
        public static readonly SymbolId OpRightShift = new SymbolId(OpRightShiftId);
        ///<summary>SymbolId for '__rrshift__'</summary>
        public static readonly SymbolId OpReverseRightShift = new SymbolId(OpReverseRightShiftId);
        ///<summary>SymbolId for '__irshift__'</summary>
        public static readonly SymbolId OpInPlaceRightShift = new SymbolId(OpInPlaceRightShiftId);
        ///<summary>SymbolId for '__and__'</summary>
        public static readonly SymbolId OpBitwiseAnd = new SymbolId(OpBitwiseAndId);
        ///<summary>SymbolId for '__rand__'</summary>
        public static readonly SymbolId OpReverseBitwiseAnd = new SymbolId(OpReverseBitwiseAndId);
        ///<summary>SymbolId for '__iand__'</summary>
        public static readonly SymbolId OpInPlaceBitwiseAnd = new SymbolId(OpInPlaceBitwiseAndId);
        ///<summary>SymbolId for '__or__'</summary>
        public static readonly SymbolId OpBitwiseOr = new SymbolId(OpBitwiseOrId);
        ///<summary>SymbolId for '__ror__'</summary>
        public static readonly SymbolId OpReverseBitwiseOr = new SymbolId(OpReverseBitwiseOrId);
        ///<summary>SymbolId for '__ior__'</summary>
        public static readonly SymbolId OpInPlaceBitwiseOr = new SymbolId(OpInPlaceBitwiseOrId);
        ///<summary>SymbolId for '__xor__'</summary>
        public static readonly SymbolId OpXor = new SymbolId(OpXorId);
        ///<summary>SymbolId for '__rxor__'</summary>
        public static readonly SymbolId OpReverseXor = new SymbolId(OpReverseXorId);
        ///<summary>SymbolId for '__ixor__'</summary>
        public static readonly SymbolId OpInPlaceXor = new SymbolId(OpInPlaceXorId);
        ///<summary>SymbolId for '__lt__'</summary>
        public static readonly SymbolId OpLessThan = new SymbolId(OpLessThanId);
        ///<summary>SymbolId for '__gt__'</summary>
        public static readonly SymbolId OpGreaterThan = new SymbolId(OpGreaterThanId);
        ///<summary>SymbolId for '__le__'</summary>
        public static readonly SymbolId OpLessThanOrEqual = new SymbolId(OpLessThanOrEqualId);
        ///<summary>SymbolId for '__ge__'</summary>
        public static readonly SymbolId OpGreaterThanOrEqual = new SymbolId(OpGreaterThanOrEqualId);
        ///<summary>SymbolId for '__eq__'</summary>
        public static readonly SymbolId OpEqual = new SymbolId(OpEqualId);
        ///<summary>SymbolId for '__ne__'</summary>
        public static readonly SymbolId OpNotEqual = new SymbolId(OpNotEqualId);
        ///<summary>SymbolId for '__lg__'</summary>
        public static readonly SymbolId OpLessThanGreaterThan = new SymbolId(OpLessThanGreaterThanId);

        // *** END GENERATED CODE ***

        #endregion

        #region Generated SymbolTable Other Symbols

        // *** BEGIN GENERATED CODE ***

        ///<summary>Symbol for '__neg__'</summary> 
        public static readonly SymbolId OpNegate = new SymbolId(OpNegateId);
        ///<summary>Symbol for '__invert__'</summary> 
        public static readonly SymbolId OpOnesComplement = new SymbolId(OpOnesComplementId);
        ///<summary>Symbol for '__dict__'</summary> 
        public static readonly SymbolId Dict = new SymbolId(DictId);
        ///<summary>Symbol for '__module__'</summary> 
        public static readonly SymbolId Module = new SymbolId(ModuleId);
        ///<summary>Symbol for '__getattribute__'</summary> 
        public static readonly SymbolId GetAttribute = new SymbolId(GetAttributeId);
        ///<summary>Symbol for '__bases__'</summary> 
        public static readonly SymbolId Bases = new SymbolId(BasesId);
        ///<summary>Symbol for '__subclasses__'</summary> 
        public static readonly SymbolId Subclasses = new SymbolId(SubclassesId);
        ///<summary>Symbol for '__name__'</summary> 
        public static readonly SymbolId Name = new SymbolId(NameId);
        ///<summary>Symbol for '__class__'</summary> 
        public static readonly SymbolId Class = new SymbolId(ClassId);
        ///<summary>Symbol for '__builtins__'</summary> 
        public static readonly SymbolId Builtins = new SymbolId(BuiltinsId);
        ///<summary>Symbol for '__getattr__'</summary> 
        public static readonly SymbolId GetAttr = new SymbolId(GetAttrId);
        ///<summary>Symbol for '__setattr__'</summary> 
        public static readonly SymbolId SetAttr = new SymbolId(SetAttrId);
        ///<summary>Symbol for '__delattr__'</summary> 
        public static readonly SymbolId DelAttr = new SymbolId(DelAttrId);
        ///<summary>Symbol for '__getitem__'</summary> 
        public static readonly SymbolId GetItem = new SymbolId(GetItemId);
        ///<summary>Symbol for '__setitem__'</summary> 
        public static readonly SymbolId SetItem = new SymbolId(SetItemId);
        ///<summary>Symbol for '__delitem__'</summary> 
        public static readonly SymbolId DelItem = new SymbolId(DelItemId);
        ///<summary>Symbol for '__init__'</summary> 
        public static readonly SymbolId Init = new SymbolId(InitId);
        ///<summary>Symbol for '__new__'</summary> 
        public static readonly SymbolId NewInst = new SymbolId(NewInstId);
        ///<summary>Symbol for '__del__'</summary> 
        public static readonly SymbolId Unassign = new SymbolId(UnassignId);
        ///<summary>Symbol for '__str__'</summary> 
        public static readonly SymbolId String = new SymbolId(StringId);
        ///<summary>Symbol for '__repr__'</summary> 
        public static readonly SymbolId Repr = new SymbolId(ReprId);
        ///<summary>Symbol for '__contains__'</summary> 
        public static readonly SymbolId Contains = new SymbolId(ContainsId);
        ///<summary>Symbol for '__len__'</summary> 
        public static readonly SymbolId Length = new SymbolId(LengthId);
        ///<summary>Symbol for '__reversed__'</summary> 
        public static readonly SymbolId Reversed = new SymbolId(ReversedId);
        ///<summary>Symbol for '__iter__'</summary> 
        public static readonly SymbolId Iterator = new SymbolId(IteratorId);
        ///<summary>Symbol for '__next__'</summary> 
        public static readonly SymbolId Next = new SymbolId(NextId);
        ///<summary>Symbol for '__weakref__'</summary> 
        public static readonly SymbolId WeakRef = new SymbolId(WeakRefId);
        ///<summary>Symbol for '__file__'</summary> 
        public static readonly SymbolId File = new SymbolId(FileId);
        ///<summary>Symbol for '__import__'</summary> 
        public static readonly SymbolId Import = new SymbolId(ImportId);
        ///<summary>Symbol for '__doc__'</summary> 
        public static readonly SymbolId Doc = new SymbolId(DocId);
        ///<summary>Symbol for '__call__'</summary> 
        public static readonly SymbolId Call = new SymbolId(CallId);
        ///<summary>Symbol for '__abs__'</summary> 
        public static readonly SymbolId AbsoluteValue = new SymbolId(AbsoluteValueId);
        ///<summary>Symbol for '__coerce__'</summary> 
        public static readonly SymbolId Coerce = new SymbolId(CoerceId);
        ///<summary>Symbol for '__int__'</summary> 
        public static readonly SymbolId ConvertToInt = new SymbolId(ConvertToIntId);
        ///<summary>Symbol for '__float__'</summary> 
        public static readonly SymbolId ConvertToFloat = new SymbolId(ConvertToFloatId);
        ///<summary>Symbol for '__long__'</summary> 
        public static readonly SymbolId ConvertToLong = new SymbolId(ConvertToLongId);
        ///<summary>Symbol for '__complex__'</summary> 
        public static readonly SymbolId ConvertToComplex = new SymbolId(ConvertToComplexId);
        ///<summary>Symbol for '__hex__'</summary> 
        public static readonly SymbolId ConvertToHex = new SymbolId(ConvertToHexId);
        ///<summary>Symbol for '__oct__'</summary> 
        public static readonly SymbolId ConvertToOctal = new SymbolId(ConvertToOctalId);
        ///<summary>Symbol for '__nonzero__'</summary> 
        public static readonly SymbolId NonZero = new SymbolId(NonZeroId);
        ///<summary>Symbol for '__pos__'</summary> 
        public static readonly SymbolId Positive = new SymbolId(PositiveId);
        ///<summary>Symbol for '__hash__'</summary> 
        public static readonly SymbolId Hash = new SymbolId(HashId);
        ///<summary>Symbol for '__cmp__'</summary> 
        public static readonly SymbolId Cmp = new SymbolId(CmpId);
        ///<summary>Symbol for '__path__'</summary> 
        public static readonly SymbolId Path = new SymbolId(PathId);
        ///<summary>Symbol for '__get__'</summary> 
        public static readonly SymbolId GetDescriptor = new SymbolId(GetDescriptorId);
        ///<summary>Symbol for '__set__'</summary> 
        public static readonly SymbolId SetDescriptor = new SymbolId(SetDescriptorId);
        ///<summary>Symbol for '__delete__'</summary> 
        public static readonly SymbolId DeleteDescriptor = new SymbolId(DeleteDescriptorId);
        ///<summary>Symbol for '__all__'</summary> 
        public static readonly SymbolId All = new SymbolId(AllId);
        ///<summary>Symbol for 'clsException'</summary> 
        public static readonly SymbolId ClrExceptionKey = new SymbolId(ClrExceptionKeyId);
        ///<summary>Symbol for 'keys'</summary> 
        public static readonly SymbolId Keys = new SymbolId(KeysId);
        ///<summary>Symbol for 'args'</summary> 
        public static readonly SymbolId Arguments = new SymbolId(ArgumentsId);
        ///<summary>Symbol for 'write'</summary> 
        public static readonly SymbolId ConsoleWrite = new SymbolId(ConsoleWriteId);
        ///<summary>Symbol for 'readline'</summary> 
        public static readonly SymbolId ConsoleReadLine = new SymbolId(ConsoleReadLineId);
        ///<summary>Symbol for 'msg'</summary> 
        public static readonly SymbolId ExceptionMessage = new SymbolId(ExceptionMessageId);
        ///<summary>Symbol for 'filename'</summary> 
        public static readonly SymbolId ExceptionFilename = new SymbolId(ExceptionFilenameId);
        ///<summary>Symbol for 'lineno'</summary> 
        public static readonly SymbolId ExceptionLineNumber = new SymbolId(ExceptionLineNumberId);
        ///<summary>Symbol for 'offset'</summary> 
        public static readonly SymbolId ExceptionOffset = new SymbolId(ExceptionOffsetId);
        ///<summary>Symbol for 'text'</summary> 
        public static readonly SymbolId Text = new SymbolId(TextId);
        ///<summary>Symbol for 'softspace'</summary> 
        public static readonly SymbolId Softspace = new SymbolId(SoftspaceId);
        ///<summary>Symbol for 'next'</summary> 
        public static readonly SymbolId GeneratorNext = new SymbolId(GeneratorNextId);
        ///<summary>Symbol for 'setdefaultencoding'</summary> 
        public static readonly SymbolId SetDefaultEncoding = new SymbolId(SetDefaultEncodingId);
        ///<summary>Symbol for 'exitfunc'</summary> 
        public static readonly SymbolId SysExitFunc = new SymbolId(SysExitFuncId);
        ///<summary>Symbol for 'None'</summary> 
        public static readonly SymbolId None = new SymbolId(NoneId);
        ///<summary>Symbol for '__metaclass__'</summary> 
        public static readonly SymbolId MetaClass = new SymbolId(MetaClassId);
        ///<summary>Symbol for '__mro__'</summary> 
        public static readonly SymbolId MethodResolutionOrder = new SymbolId(MethodResolutionOrderId);
        ///<summary>Symbol for '__getslice__'</summary> 
        public static readonly SymbolId GetSlice = new SymbolId(GetSliceId);
        ///<summary>Symbol for '__setslice__'</summary> 
        public static readonly SymbolId SetSlice = new SymbolId(SetSliceId);
        ///<summary>Symbol for '__delslice__'</summary> 
        public static readonly SymbolId DeleteSlice = new SymbolId(DeleteSliceId);
        ///<summary>Symbol for '__future__'</summary> 
        public static readonly SymbolId Future = new SymbolId(FutureId);
        ///<summary>Symbol for 'division'</summary> 
        public static readonly SymbolId Division = new SymbolId(DivisionId);
        ///<summary>Symbol for 'nested_scopes'</summary> 
        public static readonly SymbolId NestedScopes = new SymbolId(NestedScopesId);
        ///<summary>Symbol for 'generators'</summary> 
        public static readonly SymbolId Generators = new SymbolId(GeneratorsId);
        ///<summary>Symbol for 'as'</summary> 
        public static readonly SymbolId As = new SymbolId(AsId);
        ///<summary>Symbol for '*'</summary> 
        public static readonly SymbolId Star = new SymbolId(StarId);
        ///<summary>Symbol for '**'</summary> 
        public static readonly SymbolId StarStar = new SymbolId(StarStarId);
        ///<summary>Symbol for 'locals'</summary> 
        public static readonly SymbolId Locals = new SymbolId(LocalsId);
        ///<summary>Symbol for 'vars'</summary> 
        public static readonly SymbolId Vars = new SymbolId(VarsId);
        ///<summary>Symbol for 'dir'</summary> 
        public static readonly SymbolId Dir = new SymbolId(DirId);
        ///<summary>Symbol for 'eval'</summary> 
        public static readonly SymbolId Eval = new SymbolId(EvalId);
        ///<summary>Symbol for '_'</summary> 
        public static readonly SymbolId Underscore = new SymbolId(UnderscoreId);
        ///<summary>Symbol for '__gen_$_parm__'</summary> 
        public static readonly SymbolId GeneratorParmName = new SymbolId(GeneratorParmNameId);
        ///<summary>Symbol for '$env'</summary> 
        public static readonly SymbolId EnvironmentParmName = new SymbolId(EnvironmentParmNameId);
        ///<summary>Symbol for 'iter'</summary> 
        public static readonly SymbolId Iter = new SymbolId(IterId);
        ///<summary>Symbol for '__slots__'</summary> 
        public static readonly SymbolId Slots = new SymbolId(SlotsId);

        // *** END GENERATED CODE ***

        #endregion

        // well known IDs, changing these breaks binary compatibility.
        #region Generated SymbolTable Ops Values

        // *** BEGIN GENERATED CODE ***

        public const int OpAddId = 1;
        public const int OpReverseAddId = 2;
        public const int OpInPlaceAddId = 3;
        public const int OpSubtractId = 4;
        public const int OpReverseSubtractId = 5;
        public const int OpInPlaceSubtractId = 6;
        public const int OpPowerId = 7;
        public const int OpReversePowerId = 8;
        public const int OpInPlacePowerId = 9;
        public const int OpMultiplyId = 10;
        public const int OpReverseMultiplyId = 11;
        public const int OpInPlaceMultiplyId = 12;
        public const int OpFloorDivideId = 13;
        public const int OpReverseFloorDivideId = 14;
        public const int OpInPlaceFloorDivideId = 15;
        public const int OpDivideId = 16;
        public const int OpReverseDivideId = 17;
        public const int OpInPlaceDivideId = 18;
        public const int OpTrueDivideId = 19;
        public const int OpReverseTrueDivideId = 20;
        public const int OpInPlaceTrueDivideId = 21;
        public const int OpModId = 22;
        public const int OpReverseModId = 23;
        public const int OpInPlaceModId = 24;
        public const int OpLeftShiftId = 25;
        public const int OpReverseLeftShiftId = 26;
        public const int OpInPlaceLeftShiftId = 27;
        public const int OpRightShiftId = 28;
        public const int OpReverseRightShiftId = 29;
        public const int OpInPlaceRightShiftId = 30;
        public const int OpBitwiseAndId = 31;
        public const int OpReverseBitwiseAndId = 32;
        public const int OpInPlaceBitwiseAndId = 33;
        public const int OpBitwiseOrId = 34;
        public const int OpReverseBitwiseOrId = 35;
        public const int OpInPlaceBitwiseOrId = 36;
        public const int OpXorId = 37;
        public const int OpReverseXorId = 38;
        public const int OpInPlaceXorId = 39;
        public const int OpLessThanId = 40;
        public const int OpGreaterThanId = 41;
        public const int OpLessThanOrEqualId = 42;
        public const int OpGreaterThanOrEqualId = 43;
        public const int OpEqualId = 44;
        public const int OpNotEqualId = 45;
        public const int OpLessThanGreaterThanId = 46;

        // *** END GENERATED CODE ***

        #endregion

        #region Generated SymbolTable Other Values

        // *** BEGIN GENERATED CODE ***

        public const int OpNegateId = 47;
        public const int OpOnesComplementId = 48;
        public const int DictId = 49;
        public const int ModuleId = 50;
        public const int GetAttributeId = 51;
        public const int BasesId = 52;
        public const int SubclassesId = 53;
        public const int NameId = 54;
        public const int ClassId = 55;
        public const int BuiltinsId = 56;
        public const int GetAttrId = 57;
        public const int SetAttrId = 58;
        public const int DelAttrId = 59;
        public const int GetItemId = 60;
        public const int SetItemId = 61;
        public const int DelItemId = 62;
        public const int InitId = 63;
        public const int NewInstId = 64;
        public const int UnassignId = 65;
        public const int StringId = 66;
        public const int ReprId = 67;
        public const int ContainsId = 68;
        public const int LengthId = 69;
        public const int ReversedId = 70;
        public const int IteratorId = 71;
        public const int NextId = 72;
        public const int WeakRefId = 73;
        public const int FileId = 74;
        public const int ImportId = 75;
        public const int DocId = 76;
        public const int CallId = 77;
        public const int AbsoluteValueId = 78;
        public const int CoerceId = 79;
        public const int ConvertToIntId = 80;
        public const int ConvertToFloatId = 81;
        public const int ConvertToLongId = 82;
        public const int ConvertToComplexId = 83;
        public const int ConvertToHexId = 84;
        public const int ConvertToOctalId = 85;
        public const int NonZeroId = 86;
        public const int PositiveId = 87;
        public const int HashId = 88;
        public const int CmpId = 89;
        public const int PathId = 90;
        public const int GetDescriptorId = 91;
        public const int SetDescriptorId = 92;
        public const int DeleteDescriptorId = 93;
        public const int AllId = 94;
        public const int ClrExceptionKeyId = 95;
        public const int KeysId = 96;
        public const int ArgumentsId = 97;
        public const int ConsoleWriteId = 98;
        public const int ConsoleReadLineId = 99;
        public const int ExceptionMessageId = 100;
        public const int ExceptionFilenameId = 101;
        public const int ExceptionLineNumberId = 102;
        public const int ExceptionOffsetId = 103;
        public const int TextId = 104;
        public const int SoftspaceId = 105;
        public const int GeneratorNextId = 106;
        public const int SetDefaultEncodingId = 107;
        public const int SysExitFuncId = 108;
        public const int NoneId = 109;
        public const int MetaClassId = 110;
        public const int MethodResolutionOrderId = 111;
        public const int GetSliceId = 112;
        public const int SetSliceId = 113;
        public const int DeleteSliceId = 114;
        public const int FutureId = 115;
        public const int DivisionId = 116;
        public const int NestedScopesId = 117;
        public const int GeneratorsId = 118;
        public const int AsId = 119;
        public const int StarId = 120;
        public const int StarStarId = 121;
        public const int LocalsId = 122;
        public const int VarsId = 123;
        public const int DirId = 124;
        public const int EvalId = 125;
        public const int UnderscoreId = 126;
        public const int GeneratorParmNameId = 127;
        public const int EnvironmentParmNameId = 128;
        public const int IterId = 129;
        public const int SlotsId = 130;
        public const int LastWellKnownId = 131;

        // *** END GENERATED CODE ***

        #endregion

        private static void Initialize() {
            #region Generated SymbolTable Ops Added

            // *** BEGIN GENERATED CODE ***

            StringToId("__add__");  // 1 
            StringToId("__radd__");  // 2 
            StringToId("__iadd__");  // 3 
            StringToId("__sub__");  // 4 
            StringToId("__rsub__");  // 5 
            StringToId("__isub__");  // 6 
            StringToId("__pow__");  // 7 
            StringToId("__rpow__");  // 8 
            StringToId("__ipow__");  // 9 
            StringToId("__mul__");  // 10 
            StringToId("__rmul__");  // 11 
            StringToId("__imul__");  // 12 
            StringToId("__floordiv__");  // 13 
            StringToId("__rfloordiv__");  // 14 
            StringToId("__ifloordiv__");  // 15 
            StringToId("__div__");  // 16 
            StringToId("__rdiv__");  // 17 
            StringToId("__idiv__");  // 18 
            StringToId("__truediv__");  // 19 
            StringToId("__rtruediv__");  // 20 
            StringToId("__itruediv__");  // 21 
            StringToId("__mod__");  // 22 
            StringToId("__rmod__");  // 23 
            StringToId("__imod__");  // 24 
            StringToId("__lshift__");  // 25 
            StringToId("__rlshift__");  // 26 
            StringToId("__ilshift__");  // 27 
            StringToId("__rshift__");  // 28 
            StringToId("__rrshift__");  // 29 
            StringToId("__irshift__");  // 30 
            StringToId("__and__");  // 31 
            StringToId("__rand__");  // 32 
            StringToId("__iand__");  // 33 
            StringToId("__or__");  // 34 
            StringToId("__ror__");  // 35 
            StringToId("__ior__");  // 36 
            StringToId("__xor__");  // 37 
            StringToId("__rxor__");  // 38 
            StringToId("__ixor__");  // 39 
            StringToId("__lt__");  // 40 
            StringToId("__gt__");  // 41 
            StringToId("__le__");  // 42 
            StringToId("__ge__");  // 43 
            StringToId("__eq__");  // 44 
            StringToId("__ne__");  // 45 
            StringToId("__lg__");  // 46 

            // *** END GENERATED CODE ***

            #endregion

            #region Generated SymbolTable Other Added

            // *** BEGIN GENERATED CODE ***

            StringToId("__neg__");  // 47
            StringToId("__invert__");  // 48
            StringToId("__dict__");  // 49
            StringToId("__module__");  // 50
            StringToId("__getattribute__");  // 51
            StringToId("__bases__");  // 52
            StringToId("__subclasses__");  // 53
            StringToId("__name__");  // 54
            StringToId("__class__");  // 55
            StringToId("__builtins__");  // 56
            StringToId("__getattr__");  // 57
            StringToId("__setattr__");  // 58
            StringToId("__delattr__");  // 59
            StringToId("__getitem__");  // 60
            StringToId("__setitem__");  // 61
            StringToId("__delitem__");  // 62
            StringToId("__init__");  // 63
            StringToId("__new__");  // 64
            StringToId("__del__");  // 65
            StringToId("__str__");  // 66
            StringToId("__repr__");  // 67
            StringToId("__contains__");  // 68
            StringToId("__len__");  // 69
            StringToId("__reversed__");  // 70
            StringToId("__iter__");  // 71
            StringToId("__next__");  // 72
            StringToId("__weakref__");  // 73
            StringToId("__file__");  // 74
            StringToId("__import__");  // 75
            StringToId("__doc__");  // 76
            StringToId("__call__");  // 77
            StringToId("__abs__");  // 78
            StringToId("__coerce__");  // 79
            StringToId("__int__");  // 80
            StringToId("__float__");  // 81
            StringToId("__long__");  // 82
            StringToId("__complex__");  // 83
            StringToId("__hex__");  // 84
            StringToId("__oct__");  // 85
            StringToId("__nonzero__");  // 86
            StringToId("__pos__");  // 87
            StringToId("__hash__");  // 88
            StringToId("__cmp__");  // 89
            StringToId("__path__");  // 90
            StringToId("__get__");  // 91
            StringToId("__set__");  // 92
            StringToId("__delete__");  // 93
            StringToId("__all__");  // 94
            StringToId("clsException");  // 95
            StringToId("keys");  // 96
            StringToId("args");  // 97
            StringToId("write");  // 98
            StringToId("readline");  // 99
            StringToId("msg");  // 100
            StringToId("filename");  // 101
            StringToId("lineno");  // 102
            StringToId("offset");  // 103
            StringToId("text");  // 104
            StringToId("softspace");  // 105
            StringToId("next");  // 106
            StringToId("setdefaultencoding");  // 107
            StringToId("exitfunc");  // 108
            StringToId("None");  // 109
            StringToId("__metaclass__");  // 110
            StringToId("__mro__");  // 111
            StringToId("__getslice__");  // 112
            StringToId("__setslice__");  // 113
            StringToId("__delslice__");  // 114
            StringToId("__future__");  // 115
            StringToId("division");  // 116
            StringToId("nested_scopes");  // 117
            StringToId("generators");  // 118
            StringToId("as");  // 119
            StringToId("*");  // 120
            StringToId("**");  // 121
            StringToId("locals");  // 122
            StringToId("vars");  // 123
            StringToId("dir");  // 124
            StringToId("eval");  // 125
            StringToId("_");  // 126
            StringToId("__gen_$_parm__");  // 127
            StringToId("$env");  // 128
            StringToId("iter");  // 129
            StringToId("__slots__");  // 130

            // *** END GENERATED CODE ***

            #endregion
        }


    }
}
