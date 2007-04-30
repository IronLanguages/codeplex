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
using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using System.Reflection.Emit;

using System.Resources;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Globalization;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Internal.Generation {

    public delegate void EmitArrayHelper(int index);

    /// <summary>
    /// CodeGen is a helper class to make code generation a simple task.  Rather than interacting
    /// at the IL level CodeGen raises the abstraction level to enable emitting of values, expressions,
    /// handling the details of exception handling, loops, etc...
    /// </summary>
    public class CodeGen : IDisposable {
        private CodeGenOptions options;
        private TypeGen _typeGen;
        private AssemblyGen _assemblyGen;
        private ISymbolDocumentWriter _debugSymbolWriter;
        private ScopeAllocator _allocator;

        private readonly MethodBase _methodInfo;
        private readonly ILGenerator _ilg;
        private MethodInfo _methodToOverride;
        private ListStack<Targets> _targets = new ListStack<Targets>();
        private List<Slot> _freeSlots = new List<Slot>();
        private IList<Label> _yieldLabels;
        private Nullable<ReturnBlock> _returnBlock;

        // Key slots
        private EnvironmentSlot _environmentSlot;   // reference to function's own environment
        private Slot _contextSlot;                  // code context
        private Slot _paramsSlot;                   // slot for the parameter array, if any

        // Runtime line # tracking
        private Slot _currentLineSlot;              // used to track the current line # at runtime
        private int _currentLine;                   // last line number emitted to avoid dupes

        private Slot[] _argumentSlots;
        private CompilerContext _context;
        private ActionBinder _binder;

        private ConstantPool _constantPool;

        private int _curLine;
        private TextWriter _ilOut;

        private bool _generator;                    // true if emitting generator, false otherwise

        public const int FinallyExitsNormally = 0;
        public const int BranchForReturn = 1;
        public const int BranchForBreak = 2;
        public const int BranchForContinue = 3;

        public CodeGen(TypeGen typeGen, AssemblyGen assemblyGen, MethodBase mi, ILGenerator ilg,
            IList<Type> paramTypes, ConstantPool constantPool) {
            Debug.Assert(typeGen == null || typeGen.AssemblyGen == assemblyGen);
            this._typeGen = typeGen;
            this._assemblyGen = assemblyGen;
            this._methodInfo = mi;
            this._ilg = ilg;
            this._constantPool = constantPool;

            if (_typeGen == null) this.DynamicMethod = true;

            Debug.Assert(constantPool == null || mi.IsStatic);

            int firstArg;
            if (constantPool != null) {
                Debug.Assert(paramTypes.Count > 0);
                constantPool.SetCodeGen(this, new ArgSlot(0, constantPool.SlotType, this));
                firstArg = 1;
            } else {
                firstArg = 0;
            }

            int thisOffset = !mi.IsStatic ? 1 : 0;
            
            _argumentSlots = new Slot[paramTypes.Count - firstArg];
            for (int i = 0; i < _argumentSlots.Length; i++) {
                _argumentSlots[i] = new ArgSlot(i + firstArg + thisOffset, paramTypes[i + firstArg], this);
            }

            if (typeGen != null)
                this._debugSymbolWriter = typeGen.AssemblyGen.SymbolWriter;

            ILDebug = assemblyGen.ILDebug;

            EmitLineInfo = ScriptDomainManager.Options.DynamicStackTraceSupport;
            WriteSignature(mi, paramTypes);
        }

        public override string ToString() {
            return _methodInfo.ToString();
        }

        public bool EmitDebugInfo {
            get { return _debugSymbolWriter != null; }
        }

        //[Obsolete("use Methodbase instead")]
        public MethodInfo MethodInfo {
            get { return (MethodInfo)_methodInfo; }
        }

        public MethodBase MethodBase {
            get {
                return _methodInfo;
            }
        }

        // TODO: Make internal once the callers are in the same dll.
        public bool IsGenerator {
            get { return _generator; }
            set { _generator  = value; }
        }

        public CompilerContext Context {
            get {
                Debug.Assert(_context != null);
                return _context;
            }
            set {
                _context = value;
                this.Binder = _context.SourceUnit.Engine.DefaultBinder;
            }
        }

        public ActionBinder Binder {
            get {
                if (_binder == null) {
                    throw new InvalidOperationException("no Binder has been set");
                }
                return _binder;
            }
            set {
                _binder = value;
            }
        }

        public TargetBlockType BlockType {
            get {
                if (_targets.Count == 0) return TargetBlockType.Normal;
                Targets t = _targets.Peek();
                return t.BlockType;
            }
        }

        public Nullable<Label> BlockContinueLabel {
            get {
                if (_targets.Count == 0) return Targets.NoLabel;
                Targets t = _targets.Peek();
                return t.continueLabel;
            }
        }

        public bool InLoop() {
            return (_targets.Count != 0 &&
                (_targets.Peek()).breakLabel != Targets.NoLabel);
        }

        public void PushExceptionBlock(TargetBlockType type, Slot returnFlag, Slot isBlockYielded) {
            Debug.Assert(isBlockYielded == null || isBlockYielded.Type == typeof(int));

            if (_targets.Count == 0) {
                _targets.Push(new Targets(Targets.NoLabel, Targets.NoLabel, type, returnFlag, isBlockYielded, null));
            } else {
                Targets t = _targets.Peek();
                _targets.Push(new Targets(t.breakLabel, t.continueLabel, type, returnFlag ?? t.finallyReturns, isBlockYielded ?? t.isBlockYielded, null));
            }
        }

        public void PushWithTryBlock(Slot isTryYielded) {
            Debug.Assert(isTryYielded == null || isTryYielded.Type == typeof(int));

            PushExceptionBlock(TargetBlockType.With, null, isTryYielded);
        }

        public void PushTryBlock(Slot isTryYielded) {
            Debug.Assert(isTryYielded == null || isTryYielded.Type == typeof(int));

            PushExceptionBlock(TargetBlockType.Try, null, isTryYielded);
        }

        public void PushFinallyBlock(Slot returnFlag, Slot isFinallyYielded) {
            Debug.Assert(isFinallyYielded == null || isFinallyYielded.Type == typeof(int));

            PushExceptionBlock(TargetBlockType.Finally, returnFlag, isFinallyYielded);
        }

        public void PushTargets(Nullable<Label> breakTarget, Nullable<Label> continueTarget, Statement statement) {
            if (_targets.Count == 0) {
                _targets.Push(new Targets(breakTarget, continueTarget, BlockType, null, null, statement));
            } else {
                Targets t = _targets.Peek();
                TargetBlockType bt = t.BlockType;
                if (bt == TargetBlockType.Finally) {
                    bt = TargetBlockType.LoopInFinally;
                }
                _targets.Push(new Targets(breakTarget, continueTarget, bt, t.finallyReturns, t.isBlockYielded, statement));
            }
        }

        public void PopTargets(TargetBlockType type) {
            Targets t = _targets.Pop();
            Debug.Assert(t.BlockType == type);
        }

        public void PopTargets() {
            _targets.Pop();
        }

        public void CheckAndPushTargets(Statement statement) {
            for (int i = _targets.Count - 1; i >= 0; i--) {
                if (_targets[i].statement == statement) {
                    PushTargets(_targets[i].breakLabel, _targets[i].continueLabel, null);
                    return;
                }
            }

            throw new InvalidOperationException("Statement not on the stack");
        }

        public void EmitBreak() {
            Targets t = _targets.Peek();
            int finallyIndex = -1;
            switch (t.BlockType) {
                default:
                case TargetBlockType.Normal:
                case TargetBlockType.LoopInFinally:
                    if (t.breakLabel.HasValue)
                        Emit(OpCodes.Br, t.breakLabel.Value);
                    else
                        throw new InvalidOperationException();
                    break;
                case TargetBlockType.Try:
                case TargetBlockType.With:
                case TargetBlockType.Else:
                case TargetBlockType.Catch:
                    for (int i = _targets.Count - 1; i >= 0; i--) {
                        if (_targets[i].BlockType == TargetBlockType.Finally) {
                            finallyIndex = i;
                            break;
                        }

                        if (_targets[i].BlockType == TargetBlockType.LoopInFinally)
                            break;
                    }

                    if (finallyIndex == -1) {
                        if (t.breakLabel.HasValue)
                            Emit(OpCodes.Leave, t.breakLabel.Value);
                        else
                            throw new InvalidOperationException();
                    } else {
                        if(!_targets[finallyIndex].leaveLabel.HasValue)
                            _targets[finallyIndex].leaveLabel = DefineLabel();

                        EmitInt(CodeGen.BranchForBreak);
                        _targets[finallyIndex].finallyReturns.EmitSet(this);

                        Emit(OpCodes.Leave, _targets[finallyIndex].leaveLabel.Value);
                    }
                    break;
                case TargetBlockType.Finally:
                    EmitInt(CodeGen.BranchForBreak);
                    t.finallyReturns.EmitSet(this);
                    Emit(OpCodes.Endfinally);
                    break;
            }
        }

        public void EmitContinue() {
            Targets t = _targets.Peek();
            switch (t.BlockType) {
                default:
                case TargetBlockType.Normal:
                case TargetBlockType.LoopInFinally:
                    if (t.continueLabel.HasValue)
                        Emit(OpCodes.Br, t.continueLabel.Value);
                    else
                        throw new InvalidOperationException();
                    break;
                case TargetBlockType.Try:
                case TargetBlockType.With:
                case TargetBlockType.Else:
                case TargetBlockType.Catch:
                    if (t.continueLabel.HasValue)
                        Emit(OpCodes.Leave, t.continueLabel.Value);
                    else
                        throw new InvalidOperationException();
                    break;
                case TargetBlockType.Finally:
                    EmitInt(CodeGen.BranchForContinue);
                    t.finallyReturns.EmitSet(this);
                    Emit(OpCodes.Endfinally);
                    break;
            }
        }

        public void EmitReturn() {
            int finallyIndex = -1;
            switch (BlockType) {
                default:
                case TargetBlockType.Normal:
                    Emit(OpCodes.Ret);
                    break;
                case TargetBlockType.Catch:
                case TargetBlockType.Try:
                case TargetBlockType.Else:
                    // with has it's own finally block, so no need to search...
                    for (int i = _targets.Count - 1; i >= 0; i--) {
                        if (_targets[i].BlockType == TargetBlockType.Finally) {
                            finallyIndex = i;
                            break;
                        }
                    }
                    goto case TargetBlockType.With;
                case TargetBlockType.With:
                    EnsureReturnBlock();
                    Debug.Assert(_returnBlock.HasValue);
                    if (CompilerHelpers.GetReturnType(_methodInfo) != typeof(void)) {
                        _returnBlock.Value.returnValue.EmitSet(this);
                    }

                    if (finallyIndex == -1) {
                        // emit the real return
                        Emit(OpCodes.Leave, _returnBlock.Value.returnStart);
                    } else {
                        // need to leave into the inner most finally block,
                        // the finally block will fall through and check
                        // the return value.
                        if(!_targets[finallyIndex].leaveLabel.HasValue)
                            _targets[finallyIndex].leaveLabel = DefineLabel();

                        EmitInt(CodeGen.BranchForReturn);
                        _targets[finallyIndex].finallyReturns.EmitSet(this);

                        Emit(OpCodes.Leave, _targets[finallyIndex].leaveLabel.Value);
                    }
                    break;
                case TargetBlockType.LoopInFinally:
                case TargetBlockType.Finally: {
                        Targets t = _targets.Peek();
                        EnsureReturnBlock();
                        if (CompilerHelpers.GetReturnType(_methodInfo) != typeof(void)) {
                            _returnBlock.Value.returnValue.EmitSet(this);
                        }
                        // Assert check ensures that those who pushed the block with finallyReturns as null 
                        // should not yield in their blocks.
                        Debug.Assert(t.finallyReturns != null);
                        EmitInt(CodeGen.BranchForReturn);
                        t.finallyReturns.EmitSet(this);
                        Emit(OpCodes.Endfinally);
                        break;
                    }
            }
        }


        public void EmitTestTrue(Expression e) {
            e.EmitAs(this, typeof(bool));
        }

        public void EmitTestTrue() {
            EmitConvertFromObject(typeof(bool));
        }

        /// <summary>
        /// This method should support emitting a primitive cast from one type to
        /// another.  This is different from EmitConvert in that it doesn't use
        /// any language-specific conversion rules but only operates on IL types
        /// using primitive operations.
        /// TODO - Add support for casting between numeric value types.
        /// </summary>
        public void EmitCast(Type fromType, Type toType) {
            if (fromType == toType) {
                // Types are the same, no action necessary
                return;
            }

            if (toType.IsAssignableFrom(fromType)) {
                // Converting to Nullable<>?
                if (toType.IsGenericType && (toType.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                    Type genericArgument = toType.GetGenericArguments()[0];
                    // Cast the type on the top of the stack to the type of the generic argument
                    EmitCast(fromType, genericArgument);
                    EmitNew(toType.GetConstructor(new Type[] { genericArgument }));
                    return;
                }

                if (toType.IsValueType == fromType.IsValueType) {
                    return;
                }
            }

            if (toType == typeof(object)) {
                EmitConvertToObject(fromType);
            } else {
                if (toType.IsValueType && fromType.IsValueType) {
                    throw new NotImplementedException();
                } else {
                    this.Emit(OpCodes.Unbox_Any, toType);
                }
            }
        }

        // TODO: Extract common code with EmitCast to a utility function.
        public void EmitConvert(Type fromType, Type toType) {
            if (fromType == null) throw new ArgumentNullException("fromType");
            if (toType == null) throw new ArgumentNullException("toType");

            if (fromType == toType) {
                // Types are the same, no action necessary
                return;
            }

            if (toType.IsAssignableFrom(fromType)) {
                // Converting to Nullable<>?
                if (toType.IsGenericType && (toType.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                    Type genericArgument = toType.GetGenericArguments()[0];
                    // Cast the type on the top of the stack to the type of the generic argument
                    EmitCast(fromType, genericArgument);
                    EmitNew(toType.GetConstructor(new Type[] { genericArgument }));
                    return;
                }

                if (toType.IsValueType == fromType.IsValueType) {
                    return;
                }
            }

            //TODO this is clearly not the most efficient conversion pattern...
            this.EmitConvertToObject(fromType);
            this.EmitConvertFromObject(toType);
        }

        public void EmitConvertFromObject(Type paramType) {
            if (paramType == null) throw new ArgumentNullException("paramType");
            if (paramType == typeof(object)) return;

            Binder.EmitConvertFromObject(this, paramType);
        }

        /// <summary>
        /// Converts the value on the top of the stack to a System.Object.  If there is nothing
        /// to convert then this will push a null on the stack.  For almost all value types this method
        /// will box them in the standard way.  Int32 and Boolean are handled with optimized conversions
        /// that reuse the same object for small values.  For Int32 this is purely a performance
        /// optimization.  For Boolean this is use to ensure that True and False are always the same
        /// objects.
        /// </summary>
        /// <param name="retType"></param>
        public void EmitConvertToObject(Type retType) {
            if (retType == null) throw new ArgumentNullException("retType");

            if (retType == typeof(void)) {
                Emit(OpCodes.Ldnull);
            } else if (retType.IsValueType) {
                if (retType == typeof(int)) {
                    EmitCall(typeof(RuntimeHelpers), "Int32ToObject");
                } else if (retType == typeof(bool)) {
                    EmitCall(typeof(RuntimeHelpers), "BooleanToObject");
                } else {
                    Emit(OpCodes.Box, retType);
                }
            }
            // otherwise it's already an object
        }

        public void EmitReturnValue() {
            EnsureReturnBlock();
            if (CompilerHelpers.GetReturnType(_methodInfo) != typeof(void)) {
                _returnBlock.Value.returnValue.EmitGet(this);
            }
        }


        public void EmitReturn(Expression expr) {
            if (_yieldLabels != null) {
                EmitReturnInGenerator(expr);
            } else {
                if (expr == null) {
                    EmitNull();
                    EmitReturnFromObject();
                } else {
                    expr.EmitAs(this, CompilerHelpers.GetReturnType(_methodInfo));
                    EmitReturn();
                }
            }
        }

        public void EmitReturnFromObject() {
            EmitConvertFromObject(CompilerHelpers.GetReturnType(_methodInfo));
            EmitReturn();
        }

        public void EmitReturnInGenerator(Expression expr) {
            EmitSetGeneratorReturnValue(expr);

            EmitInt(0);
            EmitReturn();
        }

        public void EmitYield(Expression expr, int index, Label label) {
            if (expr == null) throw new ArgumentNullException("expr");

            if (BlockType != TargetBlockType.Normal) {
                Targets t = _targets.Peek();
                // Assert that those who pushed the block with this variable as null 
                // must not yield in thier blocks
                Debug.Assert(t.isBlockYielded != null);
                Emit(OpCodes.Ldc_I4_1);
                t.isBlockYielded.EmitSet(this);
            }

            EmitSetGeneratorReturnValue(expr);
            EmitUpdateGeneratorLocation(index);

            EmitInt(1);
            EmitReturn();

            MarkLabel(label);
        }

        private void EmitSetGeneratorReturnValue(Expression expr) {
            ArgumentSlots[1].EmitGet(this);
            EmitExprOrNull(expr);
            Emit(OpCodes.Stind_Ref);
        }

        public void EmitUpdateGeneratorLocation(int index) {
            ArgumentSlots[0].EmitGet(this);
            EmitInt(index);
            EmitFieldSet(typeof(Generator).GetField("location"));
        }

        public void EmitGetGeneratorLocation() {
            ArgumentSlots[0].EmitGet(this);
            EmitFieldGet(typeof(Generator), "location");
        }

        public void EmitUninitialized() {            
            EmitFieldGet(typeof(Uninitialized), "Instance");
        }

        public void EmitPosition(SourceLocation start, SourceLocation end) {
            if (EmitDebugInfo) {

                Debug.Assert(start != SourceLocation.Invalid);
                Debug.Assert(end != SourceLocation.Invalid);

                if (start == SourceLocation.None || end == SourceLocation.None) {
                    return;
                }
                Debug.Assert(start.Line > 0 && end.Line > 0);

                MarkSequencePoint(
                    _debugSymbolWriter,
                    start.Line, start.Column,
                    end.Line, end.Column
                    );

                Emit(OpCodes.Nop);
            }

            EmitCurrentLine(start.Line);
        }

        public void EmitSequencePointNone() {
            if (EmitDebugInfo) {
                MarkSequencePoint(
                    _debugSymbolWriter,
                    SourceLocation.None.Line, SourceLocation.None.Column,
                    SourceLocation.None.Line, SourceLocation.None.Column
                    );
            }
        }

        public Slot GetLocalTmp(Type type) {
            if (type == null) throw new ArgumentNullException("type");

            for (int i = 0; i < _freeSlots.Count; i++) {
                Slot slot = _freeSlots[i];
                if (slot.Type == type) {
                    _freeSlots.RemoveAt(i);
                    return slot;
                }
            }

            return new LocalSlot(DeclareLocal(type), this);
        }

        public Slot GetNamedLocal(Type type, string name) {
            if (type == null) throw new ArgumentNullException("type");
            if (name == null) throw new ArgumentNullException("name");

            LocalBuilder lb = DeclareLocal(type);
            if (EmitDebugInfo) lb.SetLocalSymInfo(name);
            return new LocalSlot(lb, this);
        }

        public void FreeLocalTmp(Slot slot) {
            Debug.Assert(!_freeSlots.Contains(slot));
            _freeSlots.Add(slot);
        }

        public Slot CopyTopOfStack(Type type) {
            if (type == typeof(void)) return new VoidSlot();

            this.Emit(OpCodes.Dup);
            Slot ret = GetLocalTmp(type);
            this.EmitConvertFromObject(type);
            ret.EmitSet(this);
            return ret;
        }

        public ScopeAllocator Allocator {
            get {
                Debug.Assert(_allocator != null);
                return _allocator;
            }
            set { _allocator = value; }
        }

        /// <summary>
        /// The slot used for the current frames environment.  If this method defines has one or more closure functions
        /// defined within it then the environment will contain all of the variables that the closure variables lifted
        /// into the environment.
        /// </summary>
        public EnvironmentSlot EnvironmentSlot {
            get {
                Debug.Assert(_environmentSlot != null);
                return _environmentSlot;
            }
            set { _environmentSlot = value; }
        }

        public Slot ContextSlot {
            get { return _contextSlot; }
            set {
                //Debug.Assert(_contextSlot == null); // shouldn't change after creation

                if (value == null) throw new ArgumentNullException("value");
                if (!typeof(CodeContext).IsAssignableFrom(value.Type))
                    throw new ArgumentException("ContextSlot must be assignable from CodeContext", "value");

                _contextSlot = value; 
            }
        }

        public Slot ParamsSlot {
            get { return _paramsSlot; }
            set { _paramsSlot = value; }
        }

        [Conditional("DEBUG")]
        public void EmitDebugMarker(string marker) {
            EmitString(marker);
            Emit(OpCodes.Pop);
        }

        [Conditional("DEBUG")]
        public void EmitAssertNotNull() {
            EmitAssertNotNull("Accessing null reference.");
        }

        /// <summary>
        /// asserts the value at the top of the stack is not null
        /// </summary>
        [Conditional("DEBUG")]
        public void EmitAssertNotNull(string message) {
            Emit(OpCodes.Dup);
            Emit(OpCodes.Ldnull);
            Emit(OpCodes.Ceq);
            Emit(OpCodes.Ldc_I4_0);
            Emit(OpCodes.Ceq);

            if (message == null) {
                EmitCall(typeof(Debug), "Assert", new Type[] { typeof(bool) });
            } else {
                EmitString(message);
                EmitCall(typeof(Debug), "Assert", new Type[] { typeof(bool), typeof(string) });
            }
        }

        public void SetCustomAttribute(CustomAttributeBuilder cab) {
            MethodBuilder builder = _methodInfo as MethodBuilder;
            if (builder != null) {
                builder.SetCustomAttribute(cab);
            }
        }

        public ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string strParamName) {
            MethodBuilder builder = _methodInfo as MethodBuilder;
            if (builder != null) {
                return builder.DefineParameter(position, attributes, strParamName);
            }
            DynamicMethod dm = _methodInfo as DynamicMethod;
            if (dm != null) {
                return dm.DefineParameter(position, attributes, strParamName);
            }

            throw new InvalidOperationException(Resources.InvalidOperation_DefineParameterBakedMethod);
        }

        public void EmitGet(Slot slot, SymbolId name, bool check) {
            if (slot == null) {
                throw new ArgumentNullException("slot");
            }
            slot.EmitGet(this);
            if (check) {
                slot.EmitCheck(this, name);
            }
        }

        public void EmitSet(Slot slot) {
            if (slot == null) {
                throw new ArgumentNullException("slot");
            }
            slot.EmitSet(this);
        }

        public void EmitDel(Slot slot, SymbolId name, bool check) {
            if (slot == null) {
                throw new ArgumentNullException("slot");
            }
            slot.EmitDelete(this, name, check);
        }

        public virtual void EmitGetCurrentLine() {
            if (_currentLineSlot != null) {
                _currentLineSlot.EmitGet(this);
            } else {
                EmitInt(0);
            }
        }

        public virtual void EmitCurrentLine(int line) {
            if (!EmitLineInfo) return;

            line = _context.SourceUnit.MapLine(line);
            if (line != _currentLine) {
                if (_currentLineSlot == null) {
                    _currentLineSlot = GetNamedLocal(typeof(int), "$line");
                }

                EmitInt(_currentLine = line);
                _currentLineSlot.EmitSet(this);
            }
        }

        private void EnsureReturnBlock() {
            if (!_returnBlock.HasValue) {
                ReturnBlock val = new ReturnBlock();

                if (CompilerHelpers.GetReturnType(_methodInfo) != typeof(void)) {
                    val.returnValue = GetNamedLocal(CompilerHelpers.GetReturnType(_methodInfo), "retval");
                }
                val.returnStart = DefineLabel();

                _returnBlock = val;
            }
        }

        public void Finish() {
            Debug.Assert(_targets.Count == 0);

            if (_returnBlock.HasValue) {
                MarkLabel(_returnBlock.Value.returnStart);
                if (CompilerHelpers.GetReturnType(_methodInfo) != typeof(void))
                    _returnBlock.Value.returnValue.EmitGet(this);
                Emit(OpCodes.Ret);
            }

            if (_methodToOverride != null) {
                _typeGen.TypeBuilder.DefineMethodOverride(this.MethodInfo, _methodToOverride);
            }

            if (DynamicMethod) {
                this.CreateDelegateMethodInfo();
            }
        }

        public void EmitCodeContext() {
            Debug.Assert(ContextSlot != null);

            ContextSlot.EmitGet(this);
        }

        public void EmitLanguageContext() {
            EmitCodeContext();
            EmitPropertyGet(typeof(CodeContext), "LanguageContext");
        }

        public void EmitEnvironmentOrNull() {
            if (_environmentSlot != null) {
                _environmentSlot.EmitGet(this);
            } else {
                EmitExprOrNull(null);
            }
        }

        public void EmitThis() {
            if (_methodInfo.IsStatic) throw new InvalidOperationException(Resources.InvalidOperation_ThisInStaticMethod);
            //!!! want to confirm this doesn't have a constant pool too
            Emit(OpCodes.Ldarg_0);
        }

        public void EmitExprOrNull(Expression e) {
            if (e == null) Emit(OpCodes.Ldnull);
            else e.Emit(this);
        }

        /// <summary>
        /// Emits a strongly typed array from a list of expressions.
        /// </summary>
        public void EmitArrayFromExpressions(Type elementType, IList<Expression> items) {
            if (elementType == null) throw new ArgumentNullException("elementType");
            if (items == null) throw new ArgumentNullException("items");

            EmitArray(elementType, items.Count, delegate(int index) {
                items[index].EmitAs(this, elementType);
            });
        }

        /// <summary>
        /// Emits an array of constant values provided in the given list.  The array
        /// is strongly typed.
        /// </summary>
        public void EmitArray<T>(IList<T> items) {
            if (items == null) throw new ArgumentNullException("items");

            EmitInt(items.Count);
            Emit(OpCodes.Newarr, typeof(T));
            for (int i = 0; i < items.Count; i++) {
                Emit(OpCodes.Dup);
                EmitInt(i);
                EmitRawConstant(items[i]);
                EmitStoreElement(typeof(T));
            }
        }

        /// <summary>
        /// Emits an array of values of count size.  The items are emitted via the callback
        /// which is provided with the current item index to emit.
        /// </summary>
        public void EmitArray(Type elementType, int count, EmitArrayHelper emit) {
            if (elementType == null) throw new ArgumentNullException("elementType");
            if (emit == null) throw new ArgumentNullException("emit");
            if (count < 0) throw new ArgumentOutOfRangeException("count");

            EmitInt(count);
            Emit(OpCodes.Newarr, elementType);
            for (int i = 0; i < count; i++) {
                Emit(OpCodes.Dup);
                EmitInt(i);
                
                emit(i);

                EmitStoreElement(elementType);
            }
        }

        public void EmitTrueArgGet(int i) {
            switch (i) {
                case 0: this.Emit(OpCodes.Ldarg_0); break;
                case 1: this.Emit(OpCodes.Ldarg_1); break;
                case 2: this.Emit(OpCodes.Ldarg_2); break;
                case 3: this.Emit(OpCodes.Ldarg_3); break;
                default:
                    if (i >= -128 && i <= 127) {
                        Emit(OpCodes.Ldarg_S, i);
                    } else {
                        this.Emit(OpCodes.Ldarg, i);
                    }
                    break;
            }
        }

        public void EmitArgGet(int i) {
            if (_methodInfo == null || !_methodInfo.IsStatic) {
                if (i == Int32.MaxValue) throw new InvalidOperationException(Resources.InvalidOperation_TooManyArguments);
                i += 1; // making room for this
            }
            EmitTrueArgGet(i);
        }

        public void EmitArgAddr(int i) {

            if (_methodInfo == null || !_methodInfo.IsStatic) {
                if (i == Int32.MaxValue) throw new InvalidOperationException(Resources.InvalidOperation_TooManyArguments);

                i += 1; // making room for this
            }
            EmitTrueArgAddr(i);
        }

        public void EmitTrueArgAddr(int i) {
            if (i >= -128 && i <= 127) {
                Emit(OpCodes.Ldarga_S, i);
            } else {
                this.Emit(OpCodes.Ldarga, i);
            }
        }

        //[Obsolete("Replace string with SymbolId")]
        public void EmitSymbolId(string name) {
            if (name == null) throw new ArgumentNullException("name");

            SymbolId id = SymbolTable.StringToId(name);
            EmitSymbolId(id);
        }

        /// <summary>
        /// Emits a symbol id.  
        /// </summary>
        public virtual void EmitSymbolId(SymbolId id) {
            if (DynamicMethod) {
                EmitInt(id.Id);
                EmitNew(typeof(SymbolId), new Type[] { typeof(int) });
            } else {
                //TODO - This code is Python-centric, investigate perf issues
                //around removing it and consider re-adding a generic version
                //FieldInfo fi = Symbols.GetFieldInfo(id);
                //if (fi != null) {
                //    Emit(OpCodes.Ldsfld, fi);
                //} else {
                _typeGen.EmitIndirectedSymbol(this, id);
            }
        }

        public void EmitSymbolIdInt(string name) {
            if (name == null) throw new ArgumentNullException("name");

            EmitSymbolIdId(SymbolTable.StringToId(name));
        }

        public void EmitSymbolIdId(SymbolId id) {
            if (DynamicMethod) {
                EmitInt(id.Id);
            } else {
                EmitSymbolId(id);
                Slot slot = GetLocalTmp(typeof(SymbolId));
                slot.EmitSet(this);
                slot.EmitGetAddr(this);
                EmitPropertyGet(typeof(SymbolId), "Id");
                FreeLocalTmp(slot);
            }
        }

        public void EmitSymbolIdArray(IList<SymbolId> items) {
            if (items == null) throw new ArgumentNullException("items");

            EmitInt(items.Count);
            Emit(OpCodes.Newarr, typeof(SymbolId));
            for (int i = 0; i < items.Count; i++) {
                Emit(OpCodes.Dup);
                EmitInt(i);
                Emit(OpCodes.Ldelema, typeof(SymbolId));
                EmitSymbolIdId(items[i]);
                Emit(OpCodes.Call, typeof(SymbolId).GetConstructor(new Type[] { typeof(int) }));
            }
        }

        [CLSCompliant(false)]
        public void EmitUInt(uint i)
        {
            EmitInt((int)i);
            Emit(OpCodes.Conv_U4);
        }

        public void EmitInt(int i) {
            OpCode c;
            switch (i) {
                case -1: c = OpCodes.Ldc_I4_M1; break;
                case 0: c = OpCodes.Ldc_I4_0; break;
                case 1: c = OpCodes.Ldc_I4_1; break;
                case 2: c = OpCodes.Ldc_I4_2; break;
                case 3: c = OpCodes.Ldc_I4_3; break;
                case 4: c = OpCodes.Ldc_I4_4; break;
                case 5: c = OpCodes.Ldc_I4_5; break;
                case 6: c = OpCodes.Ldc_I4_6; break;
                case 7: c = OpCodes.Ldc_I4_7; break;
                case 8: c = OpCodes.Ldc_I4_8; break;
                default:
                    if (i >= -128 && i <= 127) {
                        Emit(OpCodes.Ldc_I4_S, (byte)i);
                    } else {
                        Emit(OpCodes.Ldc_I4, i);
                    }
                    return;
            }
            Emit(c);
        }

        public void EmitPropertyGet(Type type, string name) {
            if (type == null) throw new ArgumentNullException("type");
            if (name == null) throw new ArgumentNullException("name");

            EmitPropertyGet(type.GetProperty(name));
        }

        public void EmitPropertyGet(PropertyInfo pi) {
            if (pi == null) throw new ArgumentNullException("pi");

            if (!pi.CanRead) throw new InvalidOperationException(Resources.CantReadProperty);

            EmitCall(pi.GetGetMethod());
        }

        public void EmitPropertySet(Type type, string name) {
            if (type == null) throw new ArgumentNullException("type");
            if (name == null) throw new ArgumentNullException("name");

            EmitPropertySet(type.GetProperty(name));
        }

        public void EmitPropertySet(PropertyInfo pi) {
            if (pi == null) throw new ArgumentNullException("pi");

            if (!pi.CanRead) throw new InvalidOperationException(Resources.CantWriteProperty);

            EmitCall(pi.GetSetMethod());
        }

        public void EmitFieldGet(Type type, String name) {
            if (type == null) throw new ArgumentNullException("type");
            if (name == null) throw new ArgumentNullException("name");

            EmitFieldGet(type.GetField(name));
        }

        public void EmitFieldGet(FieldInfo fi) {
            if (fi == null) throw new ArgumentNullException("fi");

            if (fi.IsStatic) {
                Emit(OpCodes.Ldsfld, fi);
            } else {
                Emit(OpCodes.Ldfld, fi);
            }
        }
        public void EmitFieldSet(FieldInfo fi) {
            if (fi == null) throw new ArgumentNullException("fi");

            if (fi.IsStatic) {
                Emit(OpCodes.Stsfld, fi);
            } else {
                Emit(OpCodes.Stfld, fi);
            }
        }

        public void EmitNew(ConstructorInfo ci) {
            if (ci == null) throw new ArgumentNullException("ci");

            if (ci.DeclaringType.ContainsGenericParameters) {
                EmitIllegalNew(ci);
            }
            Emit(OpCodes.Newobj, ci);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void EmitIllegalNew(ConstructorInfo ci) {
            if (ci == null) throw new ArgumentNullException("ci");

            //TODO Python would like a 'TypeError' == 'ArgumentTypeException' here
            throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.IllegalNew_GenericParams, ci.DeclaringType));
        }

        public void EmitNew(Type type, Type[] paramTypes) {
            if (type == null) throw new ArgumentNullException("type");
            if (paramTypes == null) throw new ArgumentNullException("paramTypes");

            EmitNew(type.GetConstructor(paramTypes));
        }

        public void EmitCall(MethodInfo mi) {
            if (mi == null) throw new ArgumentNullException("mi");

            if (mi.IsVirtual && !mi.DeclaringType.IsValueType) {
                Emit(OpCodes.Callvirt, mi);
            } else {
                Emit(OpCodes.Call, mi);
            }
        }

        public void EmitCall(Type type, String name) {
            if (type == null) throw new ArgumentNullException("type");
            if (name == null) throw new ArgumentNullException("name");

            EmitCall(type.GetMethod(name));
        }

        public void EmitCall(Type type, String name, Type[] paramTypes) {
            if (type == null) throw new ArgumentNullException("type");
            if (name == null) throw new ArgumentNullException("name");
            if (paramTypes == null) throw new ArgumentNullException("paramTypes");

            EmitCall(type.GetMethod(name, paramTypes));
        }

        public void EmitCallDelegate(Delegate @delegate) {
            if (@delegate == null) throw new ArgumentNullException("delegate");
            EmitCall(@delegate.Method);
        }

        public delegate void EmitAsHelper(CodeGen cg, Type asType);

        public void EmitInPlaceOperator(Operators op, Type leftType, EmitAsHelper leftEmit,
                                        Type rightType, EmitAsHelper rightEmit) {
            switch (op) {
                case Operators.InPlaceAdd:
                case Operators.InPlaceSubtract:
                case Operators.InPlaceMultiply:
                case Operators.InPlaceDivide:
                case Operators.InPlaceTrueDivide:
                case Operators.InPlaceMod:
                case Operators.InPlaceBitwiseAnd:
                case Operators.InPlaceBitwiseOr:
                case Operators.InPlaceXor:
                case Operators.InPlaceLeftShift:
                case Operators.InPlaceRightShift:
                case Operators.InPlacePower:
                case Operators.InPlaceFloorDivide:
                case Operators.InPlaceRightShiftUnsigned:
                    break;
                default:
                    throw new ArgumentException("op");
            }

            // Can generate dynamic site
            bool fast;
            Action action = DoOperationAction.Make(op);
            Slot site = CreateDynamicSite(
                action,
                new Type[] { leftType, rightType, typeof(object) },
                out fast
            );

            site.EmitGet(this);
            if (!fast) {
                EmitCodeContext();
            }
            leftEmit(this, leftType);
            rightEmit(this, rightType);
            EmitCall(site.Type, "Invoke");
        }

        public void EmitName(SymbolId name) {
            if (name == SymbolId.Empty) throw new ArgumentException(Resources.EmptySymbolId, "name");

            EmitString(SymbolTable.IdToString(name));
        }

        public void EmitType(TypeGen typeGen) {
            if (typeGen == null) throw new ArgumentNullException("typeGen");

            Emit(OpCodes.Ldtoken, typeGen.TypeBuilder);
            EmitCall(typeof(Type), "GetTypeFromHandle");
        }

        public void EmitType(Type type) {
            if (type == null) throw new ArgumentNullException("type");

            Emit(OpCodes.Ldtoken, type);
            EmitCall(typeof(Type), "GetTypeFromHandle");
        }

        // Not to be used with virtual methods
        public void EmitDelegate(CodeGen delegateFunction, Type delegateType) {            
            if (delegateFunction == null) throw new ArgumentNullException("delegateFunction");
            if (delegateType == null) throw new ArgumentNullException("delegateType");

            if (delegateFunction.MethodInfo is DynamicMethod || delegateFunction.ConstantPool != null) {
                Delegate d = delegateFunction.CreateDelegate(delegateType);
                this.ConstantPool.AddData(d).EmitGet(this);
            } else {
                EmitNull();
                Emit(OpCodes.Ldftn, delegateFunction.MethodInfo);
                Emit(OpCodes.Newobj, (ConstructorInfo)(delegateType.GetMember(".ctor")[0]));
            }
        }

        /// <summary>
        /// Emits a Ldind* instruction for the appropriate type
        /// </summary>
        public void EmitLoadValueIndirect(Type type) {
            if (type == null) throw new ArgumentNullException("type");

            if (type.IsValueType) {
                if (type == typeof(int)) Emit(OpCodes.Ldind_I4);
                else if (type == typeof(uint)) Emit(OpCodes.Ldind_U4);
                else if (type == typeof(short)) Emit(OpCodes.Ldind_I2);
                else if (type == typeof(ushort)) Emit(OpCodes.Ldind_U2);
                else if (type == typeof(long) || type == typeof(ulong)) Emit(OpCodes.Ldind_I8);
                else if (type == typeof(char)) Emit(OpCodes.Ldind_I2);
                else if (type == typeof(bool)) Emit(OpCodes.Ldind_I1);
                else if (type == typeof(float)) Emit(OpCodes.Ldind_R4);
                else if (type == typeof(double)) Emit(OpCodes.Ldind_R8);
                else Emit(OpCodes.Ldobj, type);
            } else {
                Emit(OpCodes.Ldind_Ref);
            }

        }

        /// <summary>
        /// Emits a Stind* instruction for the appropriate type.
        /// </summary>
        public void EmitStoreValueIndirect(Type type) {
            if (type == null) throw new ArgumentNullException("type");

            if (type.IsValueType) {
                if (type == typeof(int)) Emit(OpCodes.Stind_I4);
                else if (type == typeof(short)) Emit(OpCodes.Stind_I2);
                else if (type == typeof(long) || type == typeof(ulong)) Emit(OpCodes.Stind_I8);
                else if (type == typeof(char)) Emit(OpCodes.Stind_I2);
                else if (type == typeof(bool)) Emit(OpCodes.Stind_I1);
                else if (type == typeof(float)) Emit(OpCodes.Stind_R4);
                else if (type == typeof(double)) Emit(OpCodes.Stind_R8);
                else Emit(OpCodes.Stobj, type);
            } else {
                Emit(OpCodes.Stind_Ref);
            }

        }

        /// <summary>
        /// Emits the Ldelem* instruction for the appropriate type
        /// </summary>
        /// <param name="type"></param>
        public void EmitLoadElement(Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            if (type.IsValueType) {
                if (type == typeof(System.SByte)) {
                    Emit(OpCodes.Ldelem_I1);
                } else if (type == typeof(System.Int16)) {
                    Emit(OpCodes.Ldelem_I2);
                } else if (type == typeof(System.Int32)) {
                    Emit(OpCodes.Ldelem_I4);
                } else if (type == typeof(System.Int64)) {
                    Emit(OpCodes.Ldelem_I8);
                } else if (type == typeof(System.Single)) {
                    Emit(OpCodes.Ldelem_R4);
                } else if (type == typeof(System.Double)) {
                    Emit(OpCodes.Ldelem_R8);
                } else if (type == typeof(System.Byte)) {
                    Emit(OpCodes.Ldelem_U1);
                } else if (type == typeof(System.UInt16)) {
                    Emit(OpCodes.Ldelem_U2);
                } else if (type == typeof(System.UInt32)) {
                    Emit(OpCodes.Ldelem_U4);
                } else {
                    Emit(OpCodes.Ldelem, type);
                }
            } else {
                Emit(OpCodes.Ldelem_Ref);
            }
        }

        /// <summary>
        /// Emits a Stelem* instruction for the appropriate type.
        /// </summary>
        public void EmitStoreElement(Type type) {
            if (type == null) throw new ArgumentNullException("type");

            if (type.IsValueType) {
                if (type == typeof(int) || type == typeof(uint)) Emit(OpCodes.Stelem_I4);
                else if (type == typeof(short) || type == typeof(ushort)) Emit(OpCodes.Stelem_I2);
                else if (type == typeof(long) || type == typeof(ulong)) Emit(OpCodes.Stelem_I8);
                else if (type == typeof(char)) Emit(OpCodes.Stelem_I2);
                else if (type == typeof(bool)) Emit(OpCodes.Stelem_I4);
                else if (type == typeof(float)) Emit(OpCodes.Stelem_R4);
                else if (type == typeof(double)) Emit(OpCodes.Stelem_R8);
                else Emit(OpCodes.Stelem, type);
            } else {
                Emit(OpCodes.Stelem_Ref);
            }
        }

        public void EmitNull() {
            Emit(OpCodes.Ldnull);
        }

        public void EmitString(string value) {
            if (value == null) throw new ArgumentNullException("value");

            Emit(OpCodes.Ldstr, (string)value);
        }

        public void EmitStringOrNull(string value) {
            if (value == null) Emit(OpCodes.Ldnull);
            else EmitString(value);
        }

        public void EmitConstant(CompilerConstant value) {
            if (value == null) {
                EmitConstant((object)null);
                return;
            }

            if (!CacheConstants) {
                //TODO cache these so that we use the same slot for the same values
                _constantPool.AddData(value.Create()).EmitGet(this);
                return;
            }

            _typeGen.GetOrMakeConstant(value).EmitGet(this);
        }

        public void EmitConstant(object value) {
            if (!CacheConstants) {
                EmitConstantBoxed(value);
                return;
            }
            
            string strVal;
            if (value == null) {
                EmitNull();
            } else if ((strVal = value as string) != null) {
                EmitString(strVal);
            } else {
                Slot s = _typeGen.GetOrMakeConstant(value);
                s.EmitGet(this);
            }
        }


        public Slot GetOrMakeConstant(object value, Type type) {
            return _typeGen.GetOrMakeConstant(value, type);
        }

        public void EmitConstantBoxed(object value) {
            EmitRawConstant(value);
            if (value != null) {
                Type t = value.GetType();
                EmitConvertToObject(t);
            }
        }

        public void EmitRawConstant(object value) {
            if (value is CompilerConstant) {
                ((CompilerConstant)value).EmitCreation(this);
            } else {
                string strVal;
                BigInteger bi;
                string[] sa;

                if (value == null) {
                    EmitNull();
                } else if (value is int) {
                    EmitInt((int)value);
                } else if (value is double) {
                    Emit(OpCodes.Ldc_R8, (double)value);
                } else if (value is long) {
                    Emit(OpCodes.Ldc_I8, (long)value);
                } else if (value is Complex64) {
                    Complex64 c = (Complex64)value;
                    if (c.Real != 0.0) throw new NotImplementedException();
                    Emit(OpCodes.Ldc_R8, c.Imag);
                    EmitCall(typeof(Complex64), "MakeImaginary");
                } else if (!Object.ReferenceEquals((bi = value as BigInteger), null)) {
                    int ival;
                    if (bi.AsInt32(out ival)) {
                        EmitInt(ival);
                        EmitCall(typeof(BigInteger), "Create", new Type[] { typeof(int) });
                        return;
                    }
                    long lval;
                    if (bi.AsInt64(out lval)) {
                        Emit(OpCodes.Ldc_I8, lval);
                        EmitCall(typeof(BigInteger), "Create", new Type[] { typeof(long) });
                        return;
                    }

                    EmitInt(bi.Sign);
                    EmitArray(bi.GetBits());
                    EmitNew(typeof(BigInteger), new Type[] { typeof(short), typeof(uint[]) });
                    return;
                } else if ((strVal = value as string) != null) {
                    EmitString(strVal);
                } else if (value is bool) {
                    if ((bool)value) {
                        Emit(OpCodes.Ldc_I4_1);
                    } else {
                        Emit(OpCodes.Ldc_I4_0);
                    }
                } else if ((sa = value as string[]) != null) {
                    EmitArray(sa);
                } else if (value is Missing) {
                    // parameter marked as Optional gets single Missing
                    // instance as parameter.
                    Emit(OpCodes.Ldsfld, typeof(Missing).GetField("Value"));
                } else if (value.GetType().IsEnum) {
                    EmitRawEnum((Enum)value);
                } else if (value is uint) {
                    EmitUInt((uint)value);
                } else if (value is char) {
                    EmitInt((int)(char)value);
                    Emit(OpCodes.Conv_U2);
                } else if (value is byte) {
                    EmitInt((int)(byte)value);
                    Emit(OpCodes.Conv_U1);
                } else if (value is sbyte) {
                    EmitInt((int)(sbyte)value);
                    Emit(OpCodes.Conv_I1);
                } else if (value is short) {
                    EmitInt((int)(short)value);
                    Emit(OpCodes.Conv_I2);
                } else if (value is ushort) {
                    EmitInt((int)(ushort)value);
                    Emit(OpCodes.Conv_U2);
                } else if (value is ulong) {
                    Emit(OpCodes.Ldc_I8, (long)(ulong)value);
                    Emit(OpCodes.Conv_U8);
                } else if (value is SymbolId) {
                    EmitSymbolId((SymbolId)value);
                } else if (value is Type) {
                    EmitType((Type)value);
                } else if (value is RuntimeTypeHandle) {
                    Emit(OpCodes.Ldtoken, Type.GetTypeFromHandle((RuntimeTypeHandle)value));
                } else {
                    throw new NotImplementedException("generate: " + value + " type: " + value.GetType());
                }
            }
        }

        public void EmitUnbox(Type type) {
            if (type == null) throw new ArgumentNullException("type");

            Emit(OpCodes.Unbox_Any, type);
        }

        private void EmitRawEnum(object value) {
            if (value == null) throw new ArgumentNullException("value");

            switch (((Enum)value).GetTypeCode()) {
                case TypeCode.Int32: EmitRawConstant((int)value); break;
                case TypeCode.Int64: EmitRawConstant((long)value); break;
                case TypeCode.Int16: EmitRawConstant((short)value); break;
                case TypeCode.UInt32: EmitRawConstant((uint)value); break;
                case TypeCode.UInt64: EmitRawConstant((ulong)value); break;
                case TypeCode.SByte: EmitRawConstant((sbyte)value); break;
                case TypeCode.UInt16: EmitRawConstant((ushort)value); break;
                case TypeCode.Byte: EmitRawConstant((byte)value); break;
                default:
                    throw new NotImplementedException(String.Format(CultureInfo.CurrentCulture, Resources.NotImplemented_EnumEmit, value.GetType(), value));
            }
        }

        public void EmitMissingValue(Type type) {
            CodeGen cg = this;
            switch (Type.GetTypeCode(type)) {
                default:
                case TypeCode.Object:
                    // struct
                    if (type.IsSealed && type.IsValueType && !type.IsEnum) {
                        Slot s = cg.GetLocalTmp(type);
                        s.EmitGetAddr(cg);
                        cg.Emit(OpCodes.Initobj, type);
                        s.EmitGet(cg);
                    } else if (type == typeof(object)) {
                        // parameter of type object receives the actual Missing value
                        cg.Emit(OpCodes.Ldsfld, typeof(Missing).GetField("Value"));
                    } else if (!type.IsValueType) {
                        cg.Emit(OpCodes.Ldnull);
                    } else {
                        EmitTypeError("Cannot create default value for type {0}", type);
                    }
                    break;

                case TypeCode.Empty:
                case TypeCode.DBNull:
                    cg.Emit(OpCodes.Ldnull);
                    break;

                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    cg.Emit(OpCodes.Ldc_I4_0); break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    cg.Emit(OpCodes.Ldc_I4_0);
                    cg.Emit(OpCodes.Conv_I8);
                    break;

                case TypeCode.Single:
                    cg.Emit(OpCodes.Ldc_R4, default(Single));
                    break;
                case TypeCode.Double:
                    cg.Emit(OpCodes.Ldc_R8, default(Double));
                    break;
                case TypeCode.Decimal:
                    cg.Emit(OpCodes.Ldc_I4_0);
                    cg.EmitNew(typeof(Decimal).GetConstructor(new Type[] { typeof(int) }));
                    break;
                case TypeCode.DateTime:
                    Slot dt = cg.GetLocalTmp(typeof(DateTime));
                    dt.EmitGetAddr(cg);
                    cg.Emit(OpCodes.Initobj, typeof(DateTime));
                    dt.EmitGet(cg);
                    break;
                case TypeCode.String:
                    cg.Emit(OpCodes.Ldnull); break;
            }
        }

        public void EmitTypeError(string format, params object[] args) {
            EmitString(String.Format(format, args));
            EmitCall(typeof(RuntimeHelpers), "SimpleTypeError");
            Emit(OpCodes.Throw);
        }

        public Slot GetArgumentSlot(int index) {
            return _argumentSlots[index];
        }

        public void Flush() {
            if (_ilOut != null) {
                _ilOut.Flush();
                _ilOut.Close();
                _ilOut = null;
            }
        }

        public MethodInfo CreateDelegateMethodInfo() {
            Flush();

            if (_methodInfo is DynamicMethod) {
                return (MethodInfo)_methodInfo;
            } else if (_methodInfo is MethodBuilder) {
                MethodBuilder mb = _methodInfo as MethodBuilder;
                Type methodType = _typeGen.FinishType();
                return methodType.GetMethod(mb.Name);
            } else {
                throw new InvalidOperationException();
            }
        }

        public bool IsDynamicMethod {
            get {
                return _methodInfo is DynamicMethod;
            }
        }
        public Delegate CreateDelegate(Type delegateType) {
            if (delegateType == null) throw new ArgumentNullException("delegateType");

            if (_constantPool != null) {
                return CreateDelegate(CreateDelegateMethodInfo(), delegateType, _constantPool.Data);
            } else {
                return CreateDelegate(CreateDelegateMethodInfo(), delegateType);
            }
        }

        public Delegate CreateDelegate(Type delegateType, object target) {
            if (delegateType == null) throw new ArgumentNullException("delegateType");
            Debug.Assert(_constantPool == null);

            return CreateDelegate(CreateDelegateMethodInfo(), delegateType, target);
        }

        public static Delegate CreateDelegate(MethodInfo methodInfo, Type delegateType) {
            if (delegateType == null) throw new ArgumentNullException("delegateType");
            if (methodInfo == null) throw new ArgumentNullException("methodInfo");

            DynamicMethod dm = methodInfo as DynamicMethod;
            if (dm != null) {
                return dm.CreateDelegate(delegateType);
            } else {
                return Delegate.CreateDelegate(delegateType, methodInfo);
            }
        }

        public static Delegate CreateDelegate(MethodInfo methodInfo, Type delegateType, object target) {
            if (methodInfo == null) throw new ArgumentNullException("methodInfo");
            if (delegateType == null) throw new ArgumentNullException("delegateType");

            DynamicMethod dm = methodInfo as DynamicMethod;
            if (dm != null) {
                return dm.CreateDelegate(delegateType, target);
            } else {
                return Delegate.CreateDelegate(delegateType, target, methodInfo);
            }
        }

        public CodeGen DefineMethod(string name, Type retType, IList<Type> paramTypes, string[] paramNames, ConstantPool constantPool) {
            if (paramTypes == null) throw new ArgumentNullException("paramTypes");
            //if (paramNames == null) throw new ArgumentNullException("paramNames");
            for (int i = 0; i < paramTypes.Count; i++) {
                if (paramTypes[i] == null) throw new ArgumentNullException("paramTypes[" + i.ToString() + "]");
            }

            CodeGen res;
            if (!DynamicMethod) {
                res = _typeGen.DefineMethod(name, retType, paramTypes, paramNames, constantPool);
            } else {
                if (CompilerHelpers.NeedDebuggableDynamicCodeGenerator(_context)) {
#if SILVERLIGHT
                    res = CompilerHelpers.CreateDebuggableDynamicCodeGenerator(_context, name, retType, paramTypes, constantPool);
#else
                    // TODO: Consolidate the SILVERLIGHT and non-SILVERLIGHT code paths
                    res = CompilerHelpers.CreateDebuggableDynamicCodeGenerator(_context, name, retType, paramTypes, paramNames, constantPool);
#endif
                } else {
                    res = CompilerHelpers.CreateDynamicCodeGenerator(name, retType, paramTypes, constantPool);
                }
            }

            if (_context != null) res.Context = _context;
            return res;
        }

        public TypeGen DefineHelperType(string name, Type parent) {
            if (!DynamicMethod) {
                return _typeGen.DefineNestedType(name, parent);
            } else {
                return _assemblyGen.DefinePublicType(name, parent);
            }
        }

        #region ILGenerator methods

        public void BeginCatchBlock(Type exceptionType) {
            _ilg.BeginCatchBlock(exceptionType);
        }
        public Label BeginExceptionBlock() {
            WriteIL(" try {");
            return _ilg.BeginExceptionBlock();
        }

        public void BeginFaultBlock() {
            WriteIL("} fault {");
            _ilg.BeginFaultBlock();
        }

        public void BeginFinallyBlock() {
            WriteIL("} finally {");
            _ilg.BeginFinallyBlock();
        }
        public LocalBuilder DeclareLocal(Type localType) {
            LocalBuilder lb = _ilg.DeclareLocal(localType);
            WriteIL(String.Format("Local: {0}, {1}", lb.LocalIndex, localType));
            return lb;
        }
        public Label DefineLabel() {
            return _ilg.DefineLabel();
        }
        public void Emit(OpCode opcode) {
            WriteIL(opcode);
            _ilg.Emit(opcode);
        }
        public void Emit(OpCode opcode, byte arg) {
            WriteIL(opcode, arg);
            _ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, ConstructorInfo con) {
            WriteIL(opcode, con);
            _ilg.Emit(opcode, con);
        }
        public void Emit(OpCode opcode, double arg) {
            WriteIL(opcode, arg);
            _ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, FieldInfo field) {
            WriteIL(opcode, field);
            _ilg.Emit(opcode, field);
        }
        public void Emit(OpCode opcode, float arg) {
            WriteIL(opcode, arg);
            _ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, int arg) {
            WriteIL(opcode, arg);
            _ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, Label label) {
            WriteIL(opcode, label);
            _ilg.Emit(opcode, label);
        }
        public void Emit(OpCode opcode, Label[] labels) {
            WriteIL(opcode, labels);
            _ilg.Emit(opcode, labels);
        }
        public void Emit(OpCode opcode, LocalBuilder local) {
            WriteIL(opcode, local);
            _ilg.Emit(opcode, local);
        }
        public void Emit(OpCode opcode, long arg) {
            WriteIL(opcode, arg);
            _ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, MethodInfo meth) {
            WriteIL(opcode, meth);
            _ilg.Emit(opcode, meth);
        }
        [CLSCompliant(false)]
        public void Emit(OpCode opcode, sbyte arg)
        {
            WriteIL(opcode, arg);
            _ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, short arg) {
            WriteIL(opcode, arg);
            _ilg.Emit(opcode, arg);
        }
        
#if !SILVERLIGHT
        public void Emit(OpCode opcode, SignatureHelper signature) {
            WriteIL(opcode, signature);
            _ilg.Emit(opcode, signature);
        }
#endif

        public void Emit(OpCode opcode, string str) {
            WriteIL(opcode, str);
            _ilg.Emit(opcode, str);
        }
        public void Emit(OpCode opcode, Type cls) {
            WriteIL(opcode, cls);
            _ilg.Emit(opcode, cls);
        }
        public void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes) {
            WriteIL(opcode, methodInfo, optionalParameterTypes);
            _ilg.EmitCall(opcode, methodInfo, optionalParameterTypes);
        }
        public void EndExceptionBlock() {
            if (_targets.Count > 0) {
                Targets t = _targets.Peek();
                Debug.Assert(t.BlockType != TargetBlockType.LoopInFinally);
                if (t.BlockType == TargetBlockType.Finally && t.leaveLabel.HasValue) {
                    MarkLabel(t.leaveLabel.Value);
                }
            }

            WriteIL(" }");
            _ilg.EndExceptionBlock();
        }
        public void MarkLabel(Label loc) {
            WriteIL(loc);
            _ilg.MarkLabel(loc);
        }
        public void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn) {
            if (_context != null) {
                startLine = _context.SourceUnit.MapLine(startLine);
                endLine = _context.SourceUnit.MapLine(endLine);
            }
            _ilg.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
        }
        public void EmitWriteLine(string value) {
            _ilg.EmitWriteLine(value);
        }

        #endregion

        #region IL Debugging Support

#if !SILVERLIGHT
        static int count;
#endif
        [Conditional("DEBUG")]
        private void InitializeILWriter() {
#if !SILVERLIGHT
            Debug.Assert(ILDebug);
            // This ensures that it is not a DynamicMethod
            Debug.Assert(_typeGen != null);

            string full_method_name = ((_methodInfo.DeclaringType != null) ? _methodInfo.DeclaringType.FullName + "." : "") + _methodInfo.Name;
            
            string filename = String.Format("gen_{0}_{1}.il", Utils.ToValidFileName(full_method_name), 
                System.Threading.Interlocked.Increment(ref count));
            
            string tempFolder = Path.Combine(Path.GetTempPath(), "IronPython");
            Directory.CreateDirectory(tempFolder);
            filename = Path.Combine(tempFolder, filename);

            if (EmitDebugInfo) {
                _debugSymbolWriter = _typeGen.AssemblyGen.ModuleBuilder.DefineDocument(filename,
                    SymbolGuids.LanguageType_ILAssembly,
                    SymbolGuids.LanguageVendor_Microsoft,
                    SymbolGuids.DocumentType_Text);
            }

            _ilOut = new StreamWriter(ScriptDomainManager.CurrentManager.PAL.OpenOutputFileStream(filename));
#endif
        }

        [Conditional("DEBUG")]
        private void WriteSignature(MethodBase method, IList<Type> paramTypes) {
            WriteIL("{0} {1} (", method.Name, method.Attributes);
            foreach (Type type in paramTypes) {
                WriteIL("\t{0}", type.FullName);
            }
            WriteIL(")");
        }
        [Conditional("DEBUG")]
        private void WriteIL(string format, object arg0) {
            WriteIL(String.Format(CultureInfo.CurrentCulture, format, arg0));
        }
        [Conditional("DEBUG")]
        private void WriteIL(string format, object arg0, object arg1) {
            WriteIL(String.Format(CultureInfo.CurrentCulture, format, arg0, arg1));
        }
        [Conditional("DEBUG")]
        private void WriteIL(string format, object arg0, object arg1, object arg2) {
            WriteIL(String.Format(CultureInfo.CurrentCulture, format, arg0, arg1, arg2));
        }

        [Conditional("DEBUG")]
        private void WriteIL(string format, object arg0, object arg1, object arg2, object arg3, object arg4) {
            WriteIL(String.Format(CultureInfo.CurrentCulture, format, arg0, arg1, arg2, arg3, arg4));
        }
        [Conditional("DEBUG")]
        private void WriteIL(string str) {
            if (!ILDebug) return;

            if (_ilOut == null) {
                InitializeILWriter();
            }

            _curLine++;
            _ilOut.WriteLine(str);
            _ilOut.Flush();

            int lines = 0;
            for (int i = 0; i < str.Length; i++) {
                if (str[i] == '\n') lines++;
            }

            if (_debugSymbolWriter != null) {
                MarkSequencePoint(
                 _debugSymbolWriter,
                 _curLine, 1,
                 _curLine+lines, str.Length + 1
                 );
            }

            _curLine += lines;
        }

        private static string MakeSignature(MethodBase mb) {
            MethodBuilder builder = mb as MethodBuilder;
            if (builder != null) return builder.Signature;
            ConstructorBuilder cb = mb as ConstructorBuilder;
            if (cb != null) return cb.Signature;

            ParameterInfo[] parameters = mb.GetParameters();
            if (parameters.Length > 0) {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                bool comma = false;
                foreach (ParameterInfo pi in parameters) {
                    if (comma) {
                        sb.Append(", ");
                    }
                    sb.Append(pi.ParameterType.FullName);
                    sb.Append(" ");
                    sb.Append(pi.Name);
                    comma = true;
                }
                return sb.ToString();
            } else return String.Empty;
        }

        [Conditional("DEBUG")]
        private void WriteIL(OpCode op) {
            if(ILDebug) WriteIL(op.ToString());
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, byte arg) {
            if(ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, ConstructorInfo con) {
            if(ILDebug) WriteIL("{0}\t{1}({2})", opcode, con.DeclaringType, MakeSignature(con));
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, double arg) {
            if(ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, FieldInfo field) {
            if(ILDebug) WriteIL("{0}\t{1}.{2}", opcode, field.DeclaringType, field.Name);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, float arg) {
            if(ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, int arg) {
            if(ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, Label label) {
            if(ILDebug) WriteIL("{0}\tlabel_{1}", opcode, GetLabelId(label));
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, Label[] labels) {
            if(ILDebug) {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append(opcode.ToString());
                sb.Append("\t[");
                for (int i = 0; i < labels.Length; i++) {
                    if (i != 0) sb.Append(", ");
                    sb.Append("label_" + GetLabelId(labels[i]).ToString(CultureInfo.CurrentCulture));
                }
                sb.Append("]");
                WriteIL(sb.ToString());
            }
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, LocalBuilder local) {
            if(ILDebug) WriteIL("{0}\t{1}", opcode, local);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, long arg) {
            if(ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, MethodInfo meth) {
            if(ILDebug) WriteIL("{0}\t{1} {2}.{3}({4})", opcode, meth.ReturnType.FullName, meth.DeclaringType, meth.Name, MakeSignature(meth));
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, sbyte arg) {
            if(ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, short arg) {
            if(ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, SignatureHelper signature) {
            if(ILDebug) WriteIL("{0}\t{1}", opcode, signature);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, string str) {
            if(ILDebug) WriteIL("{0}\t{1}", opcode, str);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, Type cls) {
            if(ILDebug) WriteIL("{0}\t{1}", opcode, cls.FullName);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, MethodInfo meth, Type[] optionalParameterTypes) {
            if(ILDebug) WriteIL("{0}\t{1} {2}.{3}({4})", opcode, meth.ReturnType.FullName, meth.DeclaringType, meth.Name, MakeSignature(meth));
        }
        [Conditional("DEBUG")]
        private void WriteIL(Label l) {
            if(ILDebug) WriteIL("label_{0}:", GetLabelId(l).ToString(CultureInfo.CurrentCulture));
        }

        private static int GetLabelId(Label l) {
            return l.GetHashCode();
        }
        #endregion

        [Conditional("DEBUG")]
        public void Comment(string commentText) {
            Slot slot = GetLocalTmp(typeof(string));
            EmitString(commentText);
            slot.EmitSet(this);
            FreeLocalTmp(slot);
        }

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_ilOut != null) {
                    _ilOut.Dispose();
                }
            }
        }

        #endregion


        public MethodInfo MethodToOverride {
            get { return _methodToOverride; }
            set { _methodToOverride = value; }
        }
        
        /// <summary>
        /// Gets a list which can be used to inject references to objects from IL.  
        /// 
        /// For successful use derived classes must save this _constantPool list and then
        /// override EmitGetConstantPool index to emit the index and get the value.
        /// </summary>
        public ConstantPool ConstantPool {
            get {
                return _constantPool; 
            }
            set {
                _constantPool = value;
            }
        }
        
        public IList<Label> YieldLabels {
            get { return _yieldLabels; }
            set { _yieldLabels = value; }
        }
        
        public IList<Slot> ArgumentSlots {
            get { return _argumentSlots; }
        }

        public bool DynamicMethod {
            get {
                return (options & CodeGenOptions.DynamicMethod) != 0;
            }
            set {
                if (value) options |= CodeGenOptions.DynamicMethod;
                else options &= ~CodeGenOptions.DynamicMethod;
            }
        }

        /// <summary>
        /// True if CodeGen should output a text file containing the generated IL, false otherwise.
        /// </summary>
        public bool ILDebug {
            get {
                return (options & CodeGenOptions.ILDebug) != 0;
            }
            set {
                if (value) options |= CodeGenOptions.ILDebug;
                else options &= ~CodeGenOptions.ILDebug;
            }
        }

        /// <summary>
        /// True if CodeGen should store all constants in static fields and emit loads of those fields,
        /// false if constants should be emitted and boxed at runtime.
        /// </summary>
        public bool CacheConstants {
            get {
                return (options & CodeGenOptions.CacheConstants) != 0;
            }
            set {
                if (value) options |= CodeGenOptions.CacheConstants;
                else options &= ~CodeGenOptions.CacheConstants;
            }
        }

        /// <summary>
        /// True if line information should be tracked during code execution to provide
        /// runtime line-information in non-debug builds, false otherwise.
        /// </summary>
        public bool EmitLineInfo {
            get {
                return (options & CodeGenOptions.EmitLineInfo) != 0;
            }
            set {
                if (value) options |= CodeGenOptions.EmitLineInfo;
                else options &= ~CodeGenOptions.EmitLineInfo;
            }
        }

        /// <summary>
        /// Gets the TypeGen object which this CodeGen is emitting into.  TypeGen can be
        /// null if the method is a dynamic method.
        /// </summary>
        public TypeGen TypeGen {
            get {
                return _typeGen;
            }
        }

        struct ReturnBlock {
            public Slot returnValue;
            public Label returnStart;
        }

        private bool CanUseFastSite() {
            // TypeGen is required for fast sites.
            if (_typeGen == null) {
                return false;
            }

            // Fast sites are disabled for dynamic methods
            if (DynamicMethod) {
                return false;
            }

            // Fast sites only possible with global constext
            if (!(this.ContextSlot is StaticFieldSlot)) {
                return false;
            }

            return true;
        }

        public Slot CreateDynamicSite(Action action, Type[] siteTypes, out bool fast) {
            if (fast = CanUseFastSite()) {
                // TODO: Switch this to use a CompilerConstant

                // Use fast dynamic site (with cached CodeContext)
                return DynamicSiteHelpers.MakeFastSlot(action, this, siteTypes);
            } else if (this.ConstantPool == null) {
                Debug.Assert(this.TypeGen != null);
                // Use unoptimized dynamic site, store it in the static field
                return DynamicSiteHelpers.MakeSlot(action, this, siteTypes);
            } else {
                // Create unoptimized dynamic site and put it in the constant pool
                DynamicSite sds = DynamicSiteHelpers.MakeSite(
                    action,
                    DynamicSiteHelpers.MakeDynamicSiteType(siteTypes)
                    );
                return this.ConstantPool.AddData(sds);
            }
        }

        public Slot GetTemporarySlot(string name, Type type) {
            Slot temp;
            
            if (IsGenerator) {
                temp = _allocator.GetGeneratorTemp();
                if (type != typeof(object)) {
                    temp = new CastSlot(temp, type);
                }
            } else {
                temp = GetNamedLocal(type, name);
            }
            return temp;
        }
    }
}
