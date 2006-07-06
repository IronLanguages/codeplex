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
        ///<summary>Symbol for '__reduce__'</summary> 
        public static readonly SymbolId Reduce = new SymbolId(ReduceId);
        ///<summary>Symbol for '__reduce_ex__'</summary> 
        public static readonly SymbolId ReduceEx = new SymbolId(ReduceExId);
        ///<summary>Symbol for '__nonzero__'</summary> 
        public static readonly SymbolId NonZero = new SymbolId(NonZeroId);
        ///<summary>Symbol for '__pos__'</summary> 
        public static readonly SymbolId Positive = new SymbolId(PositiveId);
        ///<summary>Symbol for '__hash__'</summary> 
        public static readonly SymbolId Hash = new SymbolId(HashId);
        ///<summary>Symbol for '__cmp__'</summary> 
        public static readonly SymbolId Cmp = new SymbolId(CmpId);
        ///<summary>Symbol for '__divmod__'</summary> 
        public static readonly SymbolId DivMod = new SymbolId(DivModId);
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

        public const int OpAddId                  =   1;
        public const int OpReverseAddId           =   2;
        public const int OpInPlaceAddId           =   3;
        public const int OpSubtractId             =   4;
        public const int OpReverseSubtractId      =   5;
        public const int OpInPlaceSubtractId      =   6;
        public const int OpPowerId                =   7;
        public const int OpReversePowerId         =   8;
        public const int OpInPlacePowerId         =   9;
        public const int OpMultiplyId             =  10;
        public const int OpReverseMultiplyId      =  11;
        public const int OpInPlaceMultiplyId      =  12;
        public const int OpFloorDivideId          =  13;
        public const int OpReverseFloorDivideId   =  14;
        public const int OpInPlaceFloorDivideId   =  15;
        public const int OpDivideId               =  16;
        public const int OpReverseDivideId        =  17;
        public const int OpInPlaceDivideId        =  18;
        public const int OpTrueDivideId           =  19;
        public const int OpReverseTrueDivideId    =  20;
        public const int OpInPlaceTrueDivideId    =  21;
        public const int OpModId                  =  22;
        public const int OpReverseModId           =  23;
        public const int OpInPlaceModId           =  24;
        public const int OpLeftShiftId            =  25;
        public const int OpReverseLeftShiftId     =  26;
        public const int OpInPlaceLeftShiftId     =  27;
        public const int OpRightShiftId           =  28;
        public const int OpReverseRightShiftId    =  29;
        public const int OpInPlaceRightShiftId    =  30;
        public const int OpBitwiseAndId           =  31;
        public const int OpReverseBitwiseAndId    =  32;
        public const int OpInPlaceBitwiseAndId    =  33;
        public const int OpBitwiseOrId            =  34;
        public const int OpReverseBitwiseOrId     =  35;
        public const int OpInPlaceBitwiseOrId     =  36;
        public const int OpXorId                  =  37;
        public const int OpReverseXorId           =  38;
        public const int OpInPlaceXorId           =  39;
        public const int OpLessThanId             =  40;
        public const int OpGreaterThanId          =  41;
        public const int OpLessThanOrEqualId      =  42;
        public const int OpGreaterThanOrEqualId   =  43;
        public const int OpEqualId                =  44;
        public const int OpNotEqualId             =  45;
        public const int OpLessThanGreaterThanId  =  46;

        // *** END GENERATED CODE ***

        #endregion

        #region Generated SymbolTable Other Values

        // *** BEGIN GENERATED CODE ***

        public const int OpNegateId               =  47; // "__neg__"
        public const int OpOnesComplementId       =  48; // "__invert__"
        public const int DictId                   =  49; // "__dict__"
        public const int ModuleId                 =  50; // "__module__"
        public const int GetAttributeId           =  51; // "__getattribute__"
        public const int BasesId                  =  52; // "__bases__"
        public const int SubclassesId             =  53; // "__subclasses__"
        public const int NameId                   =  54; // "__name__"
        public const int ClassId                  =  55; // "__class__"
        public const int BuiltinsId               =  56; // "__builtins__"
        public const int GetAttrId                =  57; // "__getattr__"
        public const int SetAttrId                =  58; // "__setattr__"
        public const int DelAttrId                =  59; // "__delattr__"
        public const int GetItemId                =  60; // "__getitem__"
        public const int SetItemId                =  61; // "__setitem__"
        public const int DelItemId                =  62; // "__delitem__"
        public const int InitId                   =  63; // "__init__"
        public const int NewInstId                =  64; // "__new__"
        public const int UnassignId               =  65; // "__del__"
        public const int StringId                 =  66; // "__str__"
        public const int ReprId                   =  67; // "__repr__"
        public const int ContainsId               =  68; // "__contains__"
        public const int LengthId                 =  69; // "__len__"
        public const int ReversedId               =  70; // "__reversed__"
        public const int IteratorId               =  71; // "__iter__"
        public const int NextId                   =  72; // "__next__"
        public const int WeakRefId                =  73; // "__weakref__"
        public const int FileId                   =  74; // "__file__"
        public const int ImportId                 =  75; // "__import__"
        public const int DocId                    =  76; // "__doc__"
        public const int CallId                   =  77; // "__call__"
        public const int AbsoluteValueId          =  78; // "__abs__"
        public const int CoerceId                 =  79; // "__coerce__"
        public const int ConvertToIntId           =  80; // "__int__"
        public const int ConvertToFloatId         =  81; // "__float__"
        public const int ConvertToLongId          =  82; // "__long__"
        public const int ConvertToComplexId       =  83; // "__complex__"
        public const int ConvertToHexId           =  84; // "__hex__"
        public const int ConvertToOctalId         =  85; // "__oct__"
        public const int ReduceId                 =  86; // "__reduce__"
        public const int ReduceExId               =  87; // "__reduce_ex__"
        public const int NonZeroId                =  88; // "__nonzero__"
        public const int PositiveId               =  89; // "__pos__"
        public const int HashId                   =  90; // "__hash__"
        public const int CmpId                    =  91; // "__cmp__"
        public const int DivModId                 =  92; // "__divmod__"
        public const int PathId                   =  93; // "__path__"
        public const int GetDescriptorId          =  94; // "__get__"
        public const int SetDescriptorId          =  95; // "__set__"
        public const int DeleteDescriptorId       =  96; // "__delete__"
        public const int AllId                    =  97; // "__all__"
        public const int ClrExceptionKeyId        =  98; // "clsException"
        public const int KeysId                   =  99; // "keys"
        public const int ArgumentsId              = 100; // "args"
        public const int ConsoleWriteId           = 101; // "write"
        public const int ConsoleReadLineId        = 102; // "readline"
        public const int ExceptionMessageId       = 103; // "msg"
        public const int ExceptionFilenameId      = 104; // "filename"
        public const int ExceptionLineNumberId    = 105; // "lineno"
        public const int ExceptionOffsetId        = 106; // "offset"
        public const int TextId                   = 107; // "text"
        public const int SoftspaceId              = 108; // "softspace"
        public const int GeneratorNextId          = 109; // "next"
        public const int SetDefaultEncodingId     = 110; // "setdefaultencoding"
        public const int SysExitFuncId            = 111; // "exitfunc"
        public const int NoneId                   = 112; // "None"
        public const int MetaClassId              = 113; // "__metaclass__"
        public const int MethodResolutionOrderId  = 114; // "__mro__"
        public const int GetSliceId               = 115; // "__getslice__"
        public const int SetSliceId               = 116; // "__setslice__"
        public const int DeleteSliceId            = 117; // "__delslice__"
        public const int FutureId                 = 118; // "__future__"
        public const int DivisionId               = 119; // "division"
        public const int NestedScopesId           = 120; // "nested_scopes"
        public const int GeneratorsId             = 121; // "generators"
        public const int AsId                     = 122; // "as"
        public const int StarId                   = 123; // "*"
        public const int StarStarId               = 124; // "**"
        public const int LocalsId                 = 125; // "locals"
        public const int VarsId                   = 126; // "vars"
        public const int DirId                    = 127; // "dir"
        public const int EvalId                   = 128; // "eval"
        public const int UnderscoreId             = 129; // "_"
        public const int GeneratorParmNameId      = 130; // "__gen_$_parm__"
        public const int EnvironmentParmNameId    = 131; // "$env"
        public const int IterId                   = 132; // "iter"
        public const int SlotsId                  = 133; // "__slots__"
        public const int LastWellKnownId          = 134; // "LastWellKnown"

        // *** END GENERATED CODE ***

        #endregion

        private static void Initialize() {
            #region Generated SymbolTable Ops Added

            // *** BEGIN GENERATED CODE ***

            PublishWellKnownSymbol("__add__", OpAdd);  // 1 
            PublishWellKnownSymbol("__radd__", OpReverseAdd);  // 2 
            PublishWellKnownSymbol("__iadd__", OpInPlaceAdd);  // 3 
            PublishWellKnownSymbol("__sub__", OpSubtract);  // 4 
            PublishWellKnownSymbol("__rsub__", OpReverseSubtract);  // 5 
            PublishWellKnownSymbol("__isub__", OpInPlaceSubtract);  // 6 
            PublishWellKnownSymbol("__pow__", OpPower);  // 7 
            PublishWellKnownSymbol("__rpow__", OpReversePower);  // 8 
            PublishWellKnownSymbol("__ipow__", OpInPlacePower);  // 9 
            PublishWellKnownSymbol("__mul__", OpMultiply);  // 10 
            PublishWellKnownSymbol("__rmul__", OpReverseMultiply);  // 11 
            PublishWellKnownSymbol("__imul__", OpInPlaceMultiply);  // 12 
            PublishWellKnownSymbol("__floordiv__", OpFloorDivide);  // 13 
            PublishWellKnownSymbol("__rfloordiv__", OpReverseFloorDivide);  // 14 
            PublishWellKnownSymbol("__ifloordiv__", OpInPlaceFloorDivide);  // 15 
            PublishWellKnownSymbol("__div__", OpDivide);  // 16 
            PublishWellKnownSymbol("__rdiv__", OpReverseDivide);  // 17 
            PublishWellKnownSymbol("__idiv__", OpInPlaceDivide);  // 18 
            PublishWellKnownSymbol("__truediv__", OpTrueDivide);  // 19 
            PublishWellKnownSymbol("__rtruediv__", OpReverseTrueDivide);  // 20 
            PublishWellKnownSymbol("__itruediv__", OpInPlaceTrueDivide);  // 21 
            PublishWellKnownSymbol("__mod__", OpMod);  // 22 
            PublishWellKnownSymbol("__rmod__", OpReverseMod);  // 23 
            PublishWellKnownSymbol("__imod__", OpInPlaceMod);  // 24 
            PublishWellKnownSymbol("__lshift__", OpLeftShift);  // 25 
            PublishWellKnownSymbol("__rlshift__", OpReverseLeftShift);  // 26 
            PublishWellKnownSymbol("__ilshift__", OpInPlaceLeftShift);  // 27 
            PublishWellKnownSymbol("__rshift__", OpRightShift);  // 28 
            PublishWellKnownSymbol("__rrshift__", OpReverseRightShift);  // 29 
            PublishWellKnownSymbol("__irshift__", OpInPlaceRightShift);  // 30 
            PublishWellKnownSymbol("__and__", OpBitwiseAnd);  // 31 
            PublishWellKnownSymbol("__rand__", OpReverseBitwiseAnd);  // 32 
            PublishWellKnownSymbol("__iand__", OpInPlaceBitwiseAnd);  // 33 
            PublishWellKnownSymbol("__or__", OpBitwiseOr);  // 34 
            PublishWellKnownSymbol("__ror__", OpReverseBitwiseOr);  // 35 
            PublishWellKnownSymbol("__ior__", OpInPlaceBitwiseOr);  // 36 
            PublishWellKnownSymbol("__xor__", OpXor);  // 37 
            PublishWellKnownSymbol("__rxor__", OpReverseXor);  // 38 
            PublishWellKnownSymbol("__ixor__", OpInPlaceXor);  // 39 
            PublishWellKnownSymbol("__lt__", OpLessThan);  // 40 
            PublishWellKnownSymbol("__gt__", OpGreaterThan);  // 41 
            PublishWellKnownSymbol("__le__", OpLessThanOrEqual);  // 42 
            PublishWellKnownSymbol("__ge__", OpGreaterThanOrEqual);  // 43 
            PublishWellKnownSymbol("__eq__", OpEqual);  // 44 
            PublishWellKnownSymbol("__ne__", OpNotEqual);  // 45 
            PublishWellKnownSymbol("__lg__", OpLessThanGreaterThan);  // 46 

            // *** END GENERATED CODE ***

            #endregion

            #region Generated SymbolTable Other Added

            // *** BEGIN GENERATED CODE ***

            PublishWellKnownSymbol("__neg__", OpNegate);  // 47
            PublishWellKnownSymbol("__invert__", OpOnesComplement);  // 48
            PublishWellKnownSymbol("__dict__", Dict);  // 49
            PublishWellKnownSymbol("__module__", Module);  // 50
            PublishWellKnownSymbol("__getattribute__", GetAttribute);  // 51
            PublishWellKnownSymbol("__bases__", Bases);  // 52
            PublishWellKnownSymbol("__subclasses__", Subclasses);  // 53
            PublishWellKnownSymbol("__name__", Name);  // 54
            PublishWellKnownSymbol("__class__", Class);  // 55
            PublishWellKnownSymbol("__builtins__", Builtins);  // 56
            PublishWellKnownSymbol("__getattr__", GetAttr);  // 57
            PublishWellKnownSymbol("__setattr__", SetAttr);  // 58
            PublishWellKnownSymbol("__delattr__", DelAttr);  // 59
            PublishWellKnownSymbol("__getitem__", GetItem);  // 60
            PublishWellKnownSymbol("__setitem__", SetItem);  // 61
            PublishWellKnownSymbol("__delitem__", DelItem);  // 62
            PublishWellKnownSymbol("__init__", Init);  // 63
            PublishWellKnownSymbol("__new__", NewInst);  // 64
            PublishWellKnownSymbol("__del__", Unassign);  // 65
            PublishWellKnownSymbol("__str__", String);  // 66
            PublishWellKnownSymbol("__repr__", Repr);  // 67
            PublishWellKnownSymbol("__contains__", Contains);  // 68
            PublishWellKnownSymbol("__len__", Length);  // 69
            PublishWellKnownSymbol("__reversed__", Reversed);  // 70
            PublishWellKnownSymbol("__iter__", Iterator);  // 71
            PublishWellKnownSymbol("__next__", Next);  // 72
            PublishWellKnownSymbol("__weakref__", WeakRef);  // 73
            PublishWellKnownSymbol("__file__", File);  // 74
            PublishWellKnownSymbol("__import__", Import);  // 75
            PublishWellKnownSymbol("__doc__", Doc);  // 76
            PublishWellKnownSymbol("__call__", Call);  // 77
            PublishWellKnownSymbol("__abs__", AbsoluteValue);  // 78
            PublishWellKnownSymbol("__coerce__", Coerce);  // 79
            PublishWellKnownSymbol("__int__", ConvertToInt);  // 80
            PublishWellKnownSymbol("__float__", ConvertToFloat);  // 81
            PublishWellKnownSymbol("__long__", ConvertToLong);  // 82
            PublishWellKnownSymbol("__complex__", ConvertToComplex);  // 83
            PublishWellKnownSymbol("__hex__", ConvertToHex);  // 84
            PublishWellKnownSymbol("__oct__", ConvertToOctal);  // 85
            PublishWellKnownSymbol("__reduce__", Reduce);  // 86
            PublishWellKnownSymbol("__reduce_ex__", ReduceEx);  // 87
            PublishWellKnownSymbol("__nonzero__", NonZero);  // 88
            PublishWellKnownSymbol("__pos__", Positive);  // 89
            PublishWellKnownSymbol("__hash__", Hash);  // 90
            PublishWellKnownSymbol("__cmp__", Cmp);  // 91
            PublishWellKnownSymbol("__divmod__", DivMod);  // 92
            PublishWellKnownSymbol("__path__", Path);  // 93
            PublishWellKnownSymbol("__get__", GetDescriptor);  // 94
            PublishWellKnownSymbol("__set__", SetDescriptor);  // 95
            PublishWellKnownSymbol("__delete__", DeleteDescriptor);  // 96
            PublishWellKnownSymbol("__all__", All);  // 97
            PublishWellKnownSymbol("clsException", ClrExceptionKey);  // 98
            PublishWellKnownSymbol("keys", Keys);  // 99
            PublishWellKnownSymbol("args", Arguments);  // 100
            PublishWellKnownSymbol("write", ConsoleWrite);  // 101
            PublishWellKnownSymbol("readline", ConsoleReadLine);  // 102
            PublishWellKnownSymbol("msg", ExceptionMessage);  // 103
            PublishWellKnownSymbol("filename", ExceptionFilename);  // 104
            PublishWellKnownSymbol("lineno", ExceptionLineNumber);  // 105
            PublishWellKnownSymbol("offset", ExceptionOffset);  // 106
            PublishWellKnownSymbol("text", Text);  // 107
            PublishWellKnownSymbol("softspace", Softspace);  // 108
            PublishWellKnownSymbol("next", GeneratorNext);  // 109
            PublishWellKnownSymbol("setdefaultencoding", SetDefaultEncoding);  // 110
            PublishWellKnownSymbol("exitfunc", SysExitFunc);  // 111
            PublishWellKnownSymbol("None", None);  // 112
            PublishWellKnownSymbol("__metaclass__", MetaClass);  // 113
            PublishWellKnownSymbol("__mro__", MethodResolutionOrder);  // 114
            PublishWellKnownSymbol("__getslice__", GetSlice);  // 115
            PublishWellKnownSymbol("__setslice__", SetSlice);  // 116
            PublishWellKnownSymbol("__delslice__", DeleteSlice);  // 117
            PublishWellKnownSymbol("__future__", Future);  // 118
            PublishWellKnownSymbol("division", Division);  // 119
            PublishWellKnownSymbol("nested_scopes", NestedScopes);  // 120
            PublishWellKnownSymbol("generators", Generators);  // 121
            PublishWellKnownSymbol("as", As);  // 122
            PublishWellKnownSymbol("*", Star);  // 123
            PublishWellKnownSymbol("**", StarStar);  // 124
            PublishWellKnownSymbol("locals", Locals);  // 125
            PublishWellKnownSymbol("vars", Vars);  // 126
            PublishWellKnownSymbol("dir", Dir);  // 127
            PublishWellKnownSymbol("eval", Eval);  // 128
            PublishWellKnownSymbol("_", Underscore);  // 129
            PublishWellKnownSymbol("__gen_$_parm__", GeneratorParmName);  // 130
            PublishWellKnownSymbol("$env", EnvironmentParmName);  // 131
            PublishWellKnownSymbol("iter", Iter);  // 132
            PublishWellKnownSymbol("__slots__", Slots);  // 133

            // *** END GENERATED CODE ***

            #endregion
        }


    }
}
