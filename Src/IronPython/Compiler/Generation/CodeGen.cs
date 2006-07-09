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
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;

using System.Reflection;
using System.Reflection.Emit;

using System.Resources;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;

using IronPython.Runtime;
using IronMath;
using IronPython.Compiler.Ast;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Compiler.Generation {
    /// <summary>
    /// Summary description for CodeGen.
    /// </summary>

    struct ReturnBlock {
        public Slot returnValue;
        public Label returnStart;
    }

    struct Targets {
        public enum TargetBlockType {
            Normal,
            Try,
            Finally,
            Catch
        }

        public static readonly Label NoLabel = new Label();
        public readonly Label breakLabel;
        public readonly Label continueLabel;
        private TargetBlockType blockType;
        public readonly Slot finallyReturns;

        public TargetBlockType BlockType {
            get {
                return blockType;
            }
        }

        public Targets(Label breakLabel, Label continueLabel)
            : this(breakLabel, continueLabel, TargetBlockType.Normal, null) {
        }

        public Targets(Label breakLabel, Label continueLabel, TargetBlockType blockType, Slot finallyReturns) {
            this.breakLabel = breakLabel;
            this.continueLabel = continueLabel;
            this.blockType = blockType;
            this.finallyReturns = finallyReturns;
        }
    }

    internal class CodeGen : IDisposable {
        //@todo make all fields private
        internal TypeGen typeGen;
        private AssemblyGen assemblyGen;
        public List<object> staticData;
        private int staticDataIndex;
        private ISymbolDocumentWriter debugSymbolWriter;

        internal readonly MethodBase methodInfo;
        private readonly ILGenerator ilg;
        public MethodInfo methodToOverride;
        private Namespace names;
        private Stack<Targets> targets = new Stack<Targets>();
        private ArrayList freeSlots = new ArrayList();

        public Label[] yieldLabels;
        public bool printExprStmts = false;
        public bool doNotCacheConstants = Options.DoNotCacheConstants;
        private ReturnBlock returnBlock;

        // Key slots
        private Slot environmentSlot;           // reference to function's own environment
        private Slot staticLinkSlot;            // static link to outer scopes' environments
        private Slot moduleSlot;                // module
        private Slot contextSlot;               // caller context
        private Slot currentLineSlot;           // slot used for tracking the current source line

        public ArgSlot[] argumentSlots;
        private CompilerContext context;

        private int curLine;
        TextWriter ilOut;


        public CodeGen(TypeGen typeGen, MethodBase mi, ILGenerator ilg, ParameterInfo[] paramInfos)
            : this(typeGen, mi, ilg, CompilerHelpers.GetTypes(paramInfos)) {
            //??? Set names here
        }

        public CodeGen(TypeGen typeGen, MethodBase mi, ILGenerator ilg, Type[] paramTypes)
            : this(typeGen, typeGen.myAssembly, typeGen.moduleSlot, mi, ilg, paramTypes) {
        }

        public CodeGen(AssemblyGen assemblyGen, Slot moduleSlot, MethodBase mi, ILGenerator ilg, Type[] paramTypes)
            : this(null, assemblyGen, moduleSlot, mi, ilg, paramTypes) {
        }

        CodeGen(TypeGen typeGen, AssemblyGen assemblyGen, Slot moduleSlot, MethodBase mi, ILGenerator ilg, Type[] paramTypes) {
            Debug.Assert(typeGen == null || typeGen.myAssembly == assemblyGen);
            this.typeGen = typeGen;
            this.assemblyGen = assemblyGen;
            this.moduleSlot = moduleSlot;
            this.methodInfo = mi;
            this.ilg = ilg;

            argumentSlots = new ArgSlot[paramTypes.Length];
            int thisOffset = mi.IsStatic ? 0 : 1;
            for (int i = 0; i < paramTypes.Length; i++) {
                argumentSlots[i] = new ArgSlot(i + thisOffset, paramTypes[i], this);
            }

            if (typeGen != null)
                this.debugSymbolWriter = typeGen.myAssembly.sourceFile;

            WriteSignature(mi.Name, paramTypes);
        }

        public override string ToString() {
            return methodInfo.ToString();
        }

        public bool EmitDebugInfo {
            get { return debugSymbolWriter != null; }
        }

        public MethodInfo MethodInfo {
            get { return (MethodInfo)methodInfo; }
        }

        public bool IsGenerator() {
            return yieldLabels != null;
        }

        public CompilerContext Context {
            get {
                Debug.Assert(context != null);
                return context;
            }
            internal set {
                context = value;
            }
        }

        public Targets.TargetBlockType BlockType {
            get {
                if (targets.Count == 0) return Targets.TargetBlockType.Normal;
                Targets t = targets.Peek();
                return t.BlockType;
            }
        }

        public bool InLoop() {
            return (targets.Count != 0 &&
                (targets.Peek()).breakLabel != Targets.NoLabel);
        }

        public void PushExceptionBlock(Targets.TargetBlockType type, Slot returnFlag) {
            if (targets.Count == 0) {
                targets.Push(new Targets(Targets.NoLabel, Targets.NoLabel, type, returnFlag));
            } else {
                Targets t = targets.Peek();
                targets.Push(new Targets(t.breakLabel, t.continueLabel, type, returnFlag));
            }
        }

        public void PushTryBlock() {
            PushExceptionBlock(Targets.TargetBlockType.Try, null);
        }

        public void PushFinallyBlock(Slot returnFlag) {
            PushExceptionBlock(Targets.TargetBlockType.Finally, returnFlag);
        }

        public void PushTargets(Label breakTarget, Label continueTarget) {
            targets.Push(new Targets(breakTarget, continueTarget, BlockType, null));
        }

        public void PopTargets() {
            targets.Pop();
        }

        public void EmitBreak() {
            Targets t = targets.Peek();
            switch (t.BlockType) {
                default:
                case Targets.TargetBlockType.Normal:
                    Emit(OpCodes.Br, t.breakLabel);
                    break;
                case Targets.TargetBlockType.Try:
                    Emit(OpCodes.Leave, t.breakLabel);
                    break;
                case Targets.TargetBlockType.Finally:
                    Emit(OpCodes.Endfinally);
                    break;
                case Targets.TargetBlockType.Catch:
                    EmitFlowControlInCatch(delegate() {
                        EmitBreak();
                    });
                    break;
            }
        }

        public void EmitContinue() {
            Targets t = targets.Peek();
            switch (t.BlockType) {
                default:
                case Targets.TargetBlockType.Normal:
                    Emit(OpCodes.Br, t.continueLabel);
                    break;
                case Targets.TargetBlockType.Try:
                    Emit(OpCodes.Leave, t.continueLabel);
                    break;
                case Targets.TargetBlockType.Finally:
                    Emit(OpCodes.Endfinally);
                    break;
                case Targets.TargetBlockType.Catch:
                    EmitFlowControlInCatch(delegate() {
                        EmitContinue();
                    });
                    break;
            }
        }

        public void EmitReturn() {
            switch (BlockType) {
                default:
                case Targets.TargetBlockType.Normal:
                    Emit(OpCodes.Ret);
                    break;
                case Targets.TargetBlockType.Try:
                    EnsureReturnBlock();
                    if (CompilerHelpers.GetReturnType(methodInfo) != typeof(void)) {
                        returnBlock.returnValue.EmitSet(this);
                    }
                    Emit(OpCodes.Leave, returnBlock.returnStart);
                    break;
                case Targets.TargetBlockType.Finally: {
                        Targets t = targets.Peek();
                        EnsureReturnBlock();
                        if (CompilerHelpers.GetReturnType(methodInfo) != typeof(void)) {
                            returnBlock.returnValue.EmitSet(this);
                        } 
                        Emit(OpCodes.Ldc_I4_1);
                        t.finallyReturns.EmitSet(this);
                        Emit(OpCodes.Endfinally);
                        break;
                    }
                case Targets.TargetBlockType.Catch:
                    EmitFlowControlInCatch(delegate() {
                        // clear the current exception
                        EmitCallerContext();
                        EmitCall(typeof(Ops), "ClearException");

                        // emit the real return
                        EmitReturn();
                    });

                    break;
            }
        }

        delegate void FlowDelegate();

        /// <summary>
        /// Emits flow control (branch, break, continue, return) inside of
        /// a Python catch block.    FlowDelegate is a delegate that performs the actual
        /// emission of the flow control after this function temporarily updates our
        /// flow control state.
        /// </summary>
        private void EmitFlowControlInCatch(FlowDelegate fd) {
            int popCount;

            // pop off all catch blocks
            for (popCount = 0; targets.Peek().BlockType == Targets.TargetBlockType.Catch; popCount++) {
                targets.Pop();
            }

            // emit the flow control
            fd();

            // push catch blocks back on.
            while (popCount > 0) {
                PushExceptionBlock(Targets.TargetBlockType.Catch, null);
                popCount--;
            }
        }

        public void EmitReturn(Expression expr) {
            if (yieldLabels != null) {
                EmitReturnInGenerator(expr);
                return;
            }
            EmitExprOrNone(expr);
            EmitReturnFromObject();
        }   

        public void EmitReturnFromObject() {
            EmitConvertFromObject(CompilerHelpers.GetReturnType(methodInfo));
            EmitReturn();
        }

        public void EmitReturnInGenerator(Expression expr) {
            Emit(OpCodes.Ldarg_1);
            //??? is an expr legal
            EmitExprOrNone(expr);
            Emit(OpCodes.Stind_Ref);

            Emit(OpCodes.Ldc_I4_0);
            EmitReturn();
        }

        public void EmitYield(Expression expr, int index, Label label) {
            Emit(OpCodes.Ldarg_1);
            expr.Emit(this);
            Emit(OpCodes.Stind_Ref);

            Emit(OpCodes.Ldarg_0);
            EmitInt(index);
            EmitFieldSet(typeof(Generator).GetField("location"));

            Emit(OpCodes.Ldc_I4_1);
            EmitReturn();

            MarkLabel(label);
        }

        public void EmitPosition(Location start, Location end) {
            EmitCurrentLine(start.Line);

            if (!EmitDebugInfo) return;

            Debug.Assert(start.Line != 0 && end.Line != 0);

            MarkSequencePoint(
                debugSymbolWriter,
                start.Line, start.Column + 1,
                end.Line, end.Column + 1
                );

            Emit(OpCodes.Nop);
        }

        public Slot GetLocalTmp(Type type) {
            for (int i = 0; i < freeSlots.Count; i++) {
                Slot slot = (Slot)freeSlots[i];
                if (slot.Type == type) {
                    freeSlots.RemoveAt(i);
                    return slot;
                }
            }

            return new LocalSlot(DeclareLocal(type), this);
        }

        public Slot GetNamedLocal(Type type, string name) {
            LocalBuilder lb = DeclareLocal(type);
            if (EmitDebugInfo) lb.SetLocalSymInfo(name);
            return new LocalSlot(lb, this);
        }

        public Slot GetFrameSlot(Type type) {
            return GetNamedLocal(type, "$frame");
        }

        public void FreeLocalTmp(Slot slot) {
            Debug.Assert(!freeSlots.Contains(slot));
            freeSlots.Add(slot);
        }

        public Namespace Names {
            get {
                Debug.Assert(names != null);
                return names;
            }
            set { names = value; }
        }

        public Slot StaticLinkSlot {
            get { return staticLinkSlot; }
            set { staticLinkSlot = value; }
        }

        public Slot EnvironmentSlot {
            get {
                Debug.Assert(environmentSlot != null);
                return environmentSlot;
            }
            set { environmentSlot = value; }
        }

        public Slot ModuleSlot {
            get {
                Debug.Assert(moduleSlot != null);
                return moduleSlot;
            }
            set { moduleSlot = value; }
        }

        public Slot ContextSlot {
            get { return contextSlot; }
            set { contextSlot = value; }
        }

        public void SetCustomAttribute(CustomAttributeBuilder cab) {
            MethodBuilder builder = methodInfo as MethodBuilder;
            if (builder != null) {
                builder.SetCustomAttribute(cab);
            }
        }

        public ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string strParamName) {
            MethodBuilder builder = methodInfo as MethodBuilder;
            if (builder != null) {
                return builder.DefineParameter(position, attributes, strParamName);
            }
            DynamicMethod dm = methodInfo as DynamicMethod;
            if (dm != null) {
                return dm.DefineParameter(position, attributes, strParamName);
            }

            throw new InvalidOperationException("Attempt to define parameter on non-methodbuilder and non-dynamic methd");
        }

        public void EmitGet(SymbolId name, bool check) {
            Slot s = names[name];
            s.EmitGet(this);
            if (check) {
                s.EmitCheck(this, name);
            }
        }

        public void EmitGetGlobal(SymbolId name) {
            Slot s = names.Globals.GetOrMakeSlot(name);
            s.EmitGet(this);
        }

        public void EmitSet(SymbolId name) {
            names[name].EmitSet(this);
        }

        public void EmitDel(SymbolId name, bool check) {
            names[name].EmitDelete(this, name, check);
        }

        public void EmitGetCurrentLine() {
            if (currentLineSlot != null) {
                currentLineSlot.EmitGet(this);
            } else {
                EmitInt(0);
            }
        }

        public void EmitCurrentLine(int line) {
            if (Options.TraceBackSupport) {
                if (currentLineSlot == null) {
                    currentLineSlot = GetNamedLocal(typeof(int), "$line");
                }

                EmitInt(line);
                currentLineSlot.EmitSet(this);
            }
        }

        public void EmitUninitialized() {
            Emit(OpCodes.Ldsfld, typeof(Uninitialized).GetField("instance"));
        }

        bool returnBlockCreated = false;
        private void EnsureReturnBlock() {
            if (!returnBlockCreated) {
                if (CompilerHelpers.GetReturnType(methodInfo) != typeof(void)) {
                    returnBlock.returnValue = GetNamedLocal(CompilerHelpers.GetReturnType(methodInfo), "retval");
                }
                returnBlock.returnStart = DefineLabel();
                returnBlockCreated = true;
            }
        }

        public void Finish() {
            if (returnBlockCreated) {
                MarkLabel(returnBlock.returnStart);
                if (CompilerHelpers.GetReturnType(methodInfo) != typeof(void)) 
                    returnBlock.returnValue.EmitGet(this);
                Emit(OpCodes.Ret);
            }

            if (methodToOverride != null) {
                typeGen.myType.DefineMethodOverride(this.MethodInfo, methodToOverride);
            }
        }

        public void EmitCallerContext() {
            if (contextSlot != null) {
                contextSlot.EmitGet(this);
            } else {
                this.EmitModuleInstance();
            }
        }

        public void EmitSystemState() {
            EmitCallerContext();
            EmitCall(typeof(ICallerContext), "get_SystemState");
        }

        public void EmitStaticLinkOrNull() {
            if (staticLinkSlot != null) {
                staticLinkSlot.EmitGet(this);
            } else {
                EmitExprOrNone(null);
            }
        }

        public void EmitEnvironmentOrNull() {
            if (environmentSlot != null) {
                environmentSlot.EmitGet(this);
            } else {
                EmitExprOrNone(null);
            }
        }
        public void EmitContextOrNull() {
            if (contextSlot != null) {
                contextSlot.EmitGet(this);
            } else {
                EmitExprOrNone(null);
            }
        }

        public void EmitModuleInstance() {
            moduleSlot.EmitGet(this);
        }

        public void EmitThis() {
            Debug.Assert(!methodInfo.IsStatic);
            Emit(OpCodes.Ldarg_0);
        }

        public void EmitExprOrNone(Expression e) {
            if (e == null) Emit(OpCodes.Ldnull);
            else e.Emit(this);
        }

        public void EmitTestTrue(Expression e) {
            // Optimize the common case of <a> <cmp> <b> where <cmp> is a comparison operator and
            // we have a more efficient route to return a bool directly. We don't bother with the
            // more convoluted case of <a> <cmp> <b> <cmp> <c> but that's less likely to occur
            // anyway.
            BinaryExpression be = e as BinaryExpression;
            if (be != null && BinaryExpression.IsComparison(be) && !BinaryExpression.IsComparison(be.Right)) {
                string call;
                if (be.Operator == BinaryOperator.Equal)
                    call = "EqualRetBool";
                else if (be.Operator == BinaryOperator.NotEqual)
                    call = "NotEqualRetBool";
                else if (be.Operator == BinaryOperator.LessThan)
                    call = "LessThanRetBool";
                else if (be.Operator == BinaryOperator.LessThanOrEqual)
                    call = "LessThanOrEqualRetBool";
                else if (be.Operator == BinaryOperator.GreaterThan)
                    call = "GreaterThanRetBool";
                else if (be.Operator == BinaryOperator.GreaterThanOrEqual)
                    call = "GreaterThanOrEqualRetBool";
                else if (be.Operator == BinaryOperator.In)
                    call = "InRetBool";
                else if (be.Operator == BinaryOperator.NotIn)
                    call = "NotInRetBool";
                else if (be.Operator == BinaryOperator.Is)
                    call = "IsRetBool";
                else if (be.Operator == BinaryOperator.IsNot)
                    call = "IsNotRetBool";
                else
                    throw new NotImplementedException("optimized comparison: " + be.Operator.Symbol);
                be.Left.Emit(this);
                be.Right.Emit(this);
                EmitCall(typeof(Ops), call);
            } else {
                e.Emit(this);
                EmitCall(typeof(Ops), "IsTrue");
            }
        }

        public void EmitTestTrue() {
            EmitCall(typeof(Ops), "IsTrue");
        }

        public delegate void EmitObjectArrayHelper(int index);

        public void EmitObjectArray(int length, EmitObjectArrayHelper emit) {
            EmitInt(length);
            Emit(OpCodes.Newarr, typeof(object));
            for (int i = 0; i < length; i++) {
                Emit(OpCodes.Dup);
                EmitInt(i);

                emit(i);

                Emit(OpCodes.Stelem_Ref);
            }
        }

        public void EmitObjectArray(IList<Expression> items) {
            EmitObjectArray(items.Count, delegate(int index) {
                items[index].Emit(this);
            });
        }

        public void EmitStringArray(IList<string> items) {
            EmitInt(items.Count);
            Emit(OpCodes.Newarr, typeof(string));
            for (int i = 0; i < items.Count; i++) {
                Emit(OpCodes.Dup);
                EmitInt(i);
                EmitStringOrNull(items[i]);
                Emit(OpCodes.Stelem_Ref);
            }
        }

        public void EmitIntArray(IList<int> items) {
            EmitInt(items.Count);
            Emit(OpCodes.Newarr, typeof(int));
            for (int i = 0; i < items.Count; i++) {
                Emit(OpCodes.Dup);
                EmitInt(i);
                EmitInt(items[i]);
                Emit(OpCodes.Stelem_I4);
            }
        }

        public void EmitStelem(Type t) {
            if (t.IsValueType) {
                Emit(OpCodes.Stelem, t);
            } else {
                Emit(OpCodes.Stelem_Ref);
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
            if (methodInfo == null || !methodInfo.IsStatic) i += 1; // making room for this
            EmitTrueArgGet(i);
        }

        public void EmitArgAddr(int i) {
            if (methodInfo == null || !methodInfo.IsStatic) i += 1; // making room for this
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
            SymbolId id = SymbolTable.StringToId(name);
            EmitSymbolId(id);
        }

        /// <summary>
        /// Emits a symbol id.  
        /// </summary>
        public void EmitSymbolId(SymbolId id) {
            EmitSymbolIdId(id);
            EmitNew(typeof(SymbolId), new Type[] { typeof(int) });
        }

        public void EmitSymbolIdInt(string name) {
            SymbolId id = SymbolTable.StringToId(name);
            id = EmitSymbolIdId(id);
        }

        public SymbolId EmitSymbolIdId(SymbolId id) {
            if (id.Id >= SymbolTable.LastWellKnownId && typeGen != null) {
                // doing some form of static compilation, and the ID
                // is not well known...  we need to emit an indirection...
                typeGen.EmitIndirectedSymbol(this, id);
            } else {
                // either this is a well-known ID or we're not doing
                // a saved compilation.
                EmitInt(id.Id);
            }
            return id;
        }

        public void EmitSymbolIdArray(IList<SymbolId> items) {
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

        public void EmitUInt(uint i) {
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
                        Emit(OpCodes.Ldc_I4_S, i);
                    } else {
                        Emit(OpCodes.Ldc_I4, i);
                    }
                    return;
            }
            Emit(c);
        }

        public void EmitFieldGet(Type tp, String name) {
            EmitFieldGet(tp.GetField(name));
        }

        public void EmitFieldGet(FieldInfo fi) {
            if (fi.IsStatic) {
                Emit(OpCodes.Ldsfld, fi);
            } else {
                Emit(OpCodes.Ldfld, fi);
            }
        }
        public void EmitFieldSet(FieldInfo fi) {
            if (fi.IsStatic) {
                Emit(OpCodes.Stsfld, fi);
            } else {
                Emit(OpCodes.Stfld, fi);
            }
        }

        public void EmitNew(ConstructorInfo ci) {
            Debug.Assert(ci != null, "null constructor info, calling wrong constructor?");
            Emit(OpCodes.Newobj, ci);
        }

        public void EmitNew(Type tp, Type[] paramTypes) {
            EmitNew(tp.GetConstructor(paramTypes));
        }

        public void EmitCall(MethodInfo mi) {
            Debug.Assert(mi != null, "null method info, calling internal or wrong method?");

            if (mi.IsVirtual && !mi.DeclaringType.IsValueType) {
                Emit(OpCodes.Callvirt, mi);
            } else {
                Emit(OpCodes.Call, mi);
            }
        }

        public void EmitCall(Type tp, String name) {
            EmitCall(tp.GetMethod(name));
        }

        public void EmitCall(Type tp, String name, Type[] paramTypes) {
            EmitCall(tp.GetMethod(name, paramTypes));
        }

        public void EmitName(SymbolId name) {
            EmitString(name.GetString());
        }

        public void EmitType(Type type) {
            Emit(OpCodes.Ldtoken, type);
            EmitCall(typeof(Type), "GetTypeFromHandle");
        }

        public void EmitDelegate(CodeGen delegateFunction, Type delegateType, Slot targetSlot) {
            if (delegateFunction.MethodInfo is DynamicMethod) {
                this.EmitCallerContext();
                this.EmitInt(delegateFunction.staticDataIndex);
                this.EmitCall(typeof(ICallerContext), "GetStaticData");
                this.Emit(OpCodes.Castclass, typeof(DynamicMethod));
                EmitType(delegateType);
                if (targetSlot == null) EmitConstant(null);
                else targetSlot.EmitGet(this);
                EmitCall(typeof(Ops), "CreateDynamicDelegate");
                Emit(OpCodes.Castclass, delegateType);
            } else {
                if (delegateFunction.MethodInfo.IsVirtual) {
                    targetSlot.EmitGet(this);
                    Emit(OpCodes.Dup);
                    Emit(OpCodes.Ldvirtftn, delegateFunction.MethodInfo);
                } else {
                    if (targetSlot == null) EmitConstant(null);
                    else targetSlot.EmitGet(this);
                    Emit(OpCodes.Ldftn, delegateFunction.MethodInfo);
                }
                Emit(OpCodes.Newobj, (ConstructorInfo)(delegateType.GetMember(".ctor")[0]));
            }
        }
        

        public void EmitConvertFromObject(Type paramType) {
            if (paramType == typeof(object)) return;

            if (paramType == typeof(void)) {
                Emit(OpCodes.Pop);
            } else if (paramType == typeof(char)) {
                EmitCall(typeof(Converter), "ConvertToChar");
            } else if (paramType == typeof(int)) {
                EmitCall(typeof(Converter), "ConvertToInt32");
            } else if (paramType == typeof(string)) {
                EmitCall(typeof(Converter), "ConvertToString");
            } else if (paramType == typeof(long)) {
                EmitCall(typeof(Converter), "ConvertToInt64");
            } else if (paramType == typeof(double)) {
                EmitCall(typeof(Converter), "ConvertToDouble");
            } else if (paramType == typeof(bool)) {
                EmitCall(typeof(Converter), "ConvertToBoolean");
            } else if (paramType == typeof(BigInteger)) {
                EmitCall(typeof(Converter), "ConvertToBigInteger");
            } else if (paramType == typeof(Complex64)) {
                EmitCall(typeof(Converter), "ConvertToComplex64");
            } else if (paramType == typeof(IEnumerable)) {
                EmitCall(typeof(Converter), "ConvertToIEnumerable");
            } else if (paramType == typeof(float)) {
                EmitCall(typeof(Converter), "ConvertToSingle");
            } else if (paramType == typeof(byte)) {
                EmitCall(typeof(Converter), "ConvertToByte");
            } else if (paramType == typeof(sbyte)) {
                EmitCall(typeof(Converter), "ConvertToSByte");
            } else if (paramType == typeof(short)) {
                EmitCall(typeof(Converter), "ConvertToInt16");
            } else if (paramType == typeof(uint)) {
                EmitCall(typeof(Converter), "ConvertToUInt32");
            } else if (paramType == typeof(ulong)) {
                EmitCall(typeof(Converter), "ConvertToUInt64");
            } else if (paramType == typeof(ushort)) {
                EmitCall(typeof(Converter), "ConvertToUInt16");
            } else if (paramType == typeof(Type)) {
                EmitCall(typeof(Converter), "ConvertToType");
            } else if (typeof(Delegate).IsAssignableFrom(paramType)) {
                EmitType(paramType);
                EmitCall(typeof(Converter), "ConvertToDelegate");
                Emit(OpCodes.Castclass, paramType);                
            } else {
                Label end = DefineLabel();
                Emit(OpCodes.Dup);
                Emit(OpCodes.Isinst, paramType);
                
                Emit(OpCodes.Brtrue_S, end);
                Emit(OpCodes.Ldtoken, paramType);
                if (paramType.IsValueType) {
                    if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                        EmitCall(typeof(Converter), "ConvertToNullableType");
                    } else {
                        EmitCall(typeof(Converter), "ConvertToValueType");
                    }
                } else {
                    EmitCall(typeof(Converter), "ConvertToReferenceType");
                }
                MarkLabel(end);

                Emit(OpCodes.Unbox_Any, paramType); //??? this check may be redundant
            }
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
            if (retType == typeof(void)) {
                Emit(OpCodes.Ldnull);
            } else if (retType.IsValueType) {
                if (retType == typeof(int)) {
                    EmitCall(typeof(Ops), "Int2Object");
                } else if (retType == typeof(bool)) {
                    EmitCall(typeof(Ops), "Bool2Object");
                } else {
                    Emit(OpCodes.Box, retType);
                }
            }
            // otherwise it's already an object
        }

        public void EmitLoadValueIndirect(Type t) {
            if (t.IsValueType) {
                if (t == typeof(int)) Emit(OpCodes.Ldind_I4);
                else if (t == typeof(uint)) Emit(OpCodes.Ldind_U4);
                else if (t == typeof(short)) Emit(OpCodes.Ldind_I2);
                else if (t == typeof(ushort)) Emit(OpCodes.Ldind_U2);
                else if (t == typeof(long) || t == typeof(ulong)) Emit(OpCodes.Ldind_I8);
                else if (t == typeof(char)) Emit(OpCodes.Ldind_I2);
                else if (t == typeof(bool)) Emit(OpCodes.Ldind_I4);
                else if (t == typeof(float)) Emit(OpCodes.Ldind_R4);
                else if (t == typeof(double)) Emit(OpCodes.Ldind_R8);
                else Emit(OpCodes.Ldobj, t);
            } else {
                Emit(OpCodes.Ldind_Ref);
            }

        }

        public void EmitStoreValueIndirect(Type t) {
            if (t.IsValueType) {
                if (t == typeof(int)) Emit(OpCodes.Stind_I4);
                else if (t == typeof(short)) Emit(OpCodes.Stind_I2);
                else if (t == typeof(long) || t == typeof(ulong)) Emit(OpCodes.Stind_I8);
                else if (t == typeof(char)) Emit(OpCodes.Stind_I2);
                else if (t == typeof(bool)) Emit(OpCodes.Stind_I4);
                else if (t == typeof(float)) Emit(OpCodes.Stind_R4);
                else if (t == typeof(double)) Emit(OpCodes.Stind_R8);
                else Emit(OpCodes.Stobj, t);
            } else {
                Emit(OpCodes.Stind_Ref);
            }

        }
        public void EmitStoreElement(Type t) {
            if (t.IsValueType) {
                if (t == typeof(int) || t == typeof(uint)) Emit(OpCodes.Stelem_I4);
                else if (t == typeof(short) || t == typeof(ushort)) Emit(OpCodes.Stelem_I2);
                else if (t == typeof(long) || t == typeof(ulong)) Emit(OpCodes.Stelem_I8);
                else if (t == typeof(char)) Emit(OpCodes.Stelem_I2);
                else if (t == typeof(bool)) Emit(OpCodes.Stelem_I4);
                else if (t == typeof(float)) Emit(OpCodes.Stelem_R4);
                else if (t == typeof(double)) Emit(OpCodes.Stelem_R8);
                else Emit(OpCodes.Stelem, t);
            } else {
                Emit(OpCodes.Stelem_Ref);
            }
        }

        public void EmitPythonNone() {
            Emit(OpCodes.Ldnull);
        }

        public void EmitString(string value) {
            Emit(OpCodes.Ldstr, (string)value);
        }

        public void EmitStringOrNull(string value) {
            if (value == null) Emit(OpCodes.Ldnull);
            else EmitString(value);
        }

        public void EmitConstant(object value) {
            if (doNotCacheConstants) {
                EmitConstantBoxed(value);
                return;
            }

            if (value == null) {
                EmitPythonNone();
            } else if (value is string) {
                EmitString((string)value);
            } else {
                Slot s = typeGen.GetOrMakeConstant(value);
                s.EmitGet(this);
            }
        }


        public Slot GetOrMakeConstant(object value, Type type) {
            return typeGen.GetOrMakeConstant(value, type);
        }

        public void EmitConstantBoxed(object value) {
            EmitRawConstant(value);
            if (value != null) {
                Type t = value.GetType();

                if (t.IsValueType) Emit(OpCodes.Box, t);
            }
        }
        public void EmitRawConstant(object value) {
            if (value == null) {
                EmitPythonNone();
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
            } else if (value is BigInteger) {
                BigInteger i = (BigInteger)value;
                int ival;
                if (i.AsInt32(out ival)) {
                    EmitInt(ival);
                    EmitCall(typeof(BigInteger), "Create", new Type[] { typeof(int) });
                    return;
                }
                long lval;
                if (i.AsInt64(out lval)) {
                    Emit(OpCodes.Ldc_I8, lval);
                    EmitCall(typeof(BigInteger), "Create", new Type[] { typeof(long) });
                    return;
                }

                EmitString(i.ToString((uint)16));
                EmitCall(typeof(Ops), "MakeIntegerFromHex");
                return;
            } else if (value is string) {
                EmitString((string)value);
            } else if (value is bool) {
                if ((bool)value) {
                    Emit(OpCodes.Ldc_I4_1);
                } else {
                    Emit(OpCodes.Ldc_I4_0);
                }
            } else if (value is string[]) {
                EmitStringArray((string[])value);
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
            } else {
                throw new NotImplementedException("generate: " + value + " type: " + value.GetType());
            }
        }

        private void EmitRawEnum(object value) {
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
                    throw new NotImplementedException("generate: " + value + " type: " + value.GetType());
            }
        }

        public ArgSlot GetArgumentSlot(int index) {
            return argumentSlots[index];
        }

        public void Flush() {
            if (ilOut != null) {
                ilOut.Flush();
                ilOut.Close();
                ilOut = null;
            }
        }

        public MethodInfo CreateDelegateMethodInfo() {
            Flush();

            if (methodInfo is DynamicMethod) {
                return (MethodInfo)methodInfo;
            } else if (methodInfo is MethodBuilder) {
                MethodBuilder mb = methodInfo as MethodBuilder;
                Type methodType = typeGen.FinishType();
                return methodType.GetMethod(mb.Name);
            } else {
                throw new ArgumentException();
            }
        }

        public bool IsDynamicMethod {
            get {
                return methodInfo is DynamicMethod;
            }
        }
        public Delegate CreateDelegate(Type delegateType) {
            return CreateDelegate(CreateDelegateMethodInfo(), delegateType);
        }

        public Delegate CreateDelegate(Type delegateType, object target) {
            return CreateDelegate(CreateDelegateMethodInfo(), delegateType, target);
        }

        public static Delegate CreateDelegate(MethodInfo methodInfo, Type delegateType) {
            if (methodInfo is DynamicMethod) {
                return ((DynamicMethod)methodInfo).CreateDelegate(delegateType);
            } else {
                return Delegate.CreateDelegate(delegateType, methodInfo);
            }
        }

        public static Delegate CreateDelegate(MethodInfo methodInfo, Type delegateType, object target) {
            if (methodInfo is DynamicMethod) {
                return ((DynamicMethod)methodInfo).CreateDelegate(delegateType, target);
            } else {
                return Delegate.CreateDelegate(delegateType, target, methodInfo);
            }
        }

        private CodeGen DefineDynamicMethod(string name, Type retType, Type[] paramTypes) {
            CodeGen ret = assemblyGen.DefineDynamicMethod(name, retType, paramTypes);

            ret.staticData = this.staticData;
            ret.staticDataIndex = staticData.Count;
            staticData.Add(ret.MethodInfo);

            Slot contextSlot = ret.GetArgumentSlot(0);
            ret.contextSlot = contextSlot;
            ret.doNotCacheConstants = this.doNotCacheConstants;

            return ret;
        }

        public CodeGen DefineUserHiddenMethod(string name, Type retType, Type[] paramTypes) {
            if (typeGen != null) {
                return typeGen.DefineUserHiddenMethod(CompilerHelpers.PublicStatic, name, retType, paramTypes);
            } else {
                return DefineDynamicMethod(name, retType, paramTypes);
            }
        }

        public CodeGen DefineMethod(string name, Type retType, Type[] paramTypes, SymbolId[] paramNames) {
            string[] stringParamNames = new string[paramNames.Length];
            for (int i = 0; i < paramNames.Length; i++) {
                stringParamNames[i] = paramNames[i].GetString();
            }
            return DefineMethod(name, retType, paramTypes, stringParamNames);
        }

        public CodeGen DefineMethod(string name, Type retType, Type[] paramTypes, string[] paramNames) {
            if (typeGen != null) {
                return typeGen.DefineMethod(name, retType, paramTypes, paramNames);
            } else {
                return DefineDynamicMethod(name, retType, paramTypes);
            }
        }

        public TypeGen DefineHelperType(string name, Type parent) {
            if (typeGen != null) {
                return typeGen.DefineNestedType(name, parent);
            } else {
                return assemblyGen.DefinePublicType(name, parent);
            }
        }

        internal static Namespace CreateStaticFieldNamespace(TypeGen typeGen) {
            StaticFieldSlotFactory sfsf = new StaticFieldSlotFactory(typeGen);
            Namespace ns = new Namespace(sfsf);
            ns.Globals = new GlobalFieldNamespace(sfsf);
            return ns;
        }

        internal static Namespace CreateLocalNamespace(CodeGen codeGen) {
            Namespace ns = new Namespace(new LocalSlotFactory(codeGen));
            return ns;
        }

        internal static Namespace CreateFrameNamespace(Slot frame) {
            Namespace ns = new Namespace(new LocalFrameSlotFactory(frame));
            GlobalEnvironmentFactory gef = new GlobalEnvironmentFactory();
            ns.Globals = new GlobalEnvironmentNamespace(new EnvironmentNamespace(gef), frame);
            return ns;
        }

        #region ILGenerator methods

        public void BeginCatchBlock(Type exceptionType) {
            ilg.BeginCatchBlock(exceptionType);
        }
        public Label BeginExceptionBlock() {
            return ilg.BeginExceptionBlock();
        }

        public void BeginFaultBlock() {
            ilg.BeginFaultBlock();
        }

        public void BeginFinallyBlock() {
            ilg.BeginFinallyBlock();
        }
        public LocalBuilder DeclareLocal(Type localType) {
            return ilg.DeclareLocal(localType);
        }
        public Label DefineLabel() {
            return ilg.DefineLabel();
        }
        public void Emit(OpCode opcode) {
            WriteIL(opcode);
            ilg.Emit(opcode);
        }
        public void Emit(OpCode opcode, byte arg) {
            WriteIL(opcode, arg);
            ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, ConstructorInfo con) {
            WriteIL(opcode, con);
            ilg.Emit(opcode, con);
        }
        public void Emit(OpCode opcode, double arg) {
            WriteIL(opcode, arg);
            ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, FieldInfo field) {
            WriteIL(opcode, field);
            ilg.Emit(opcode, field);
        }
        public void Emit(OpCode opcode, float arg) {
            WriteIL(opcode, arg);
            ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, int arg) {
            WriteIL(opcode, arg);
            ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, Label label) {
            WriteIL(opcode, label);
            ilg.Emit(opcode, label);
        }
        public void Emit(OpCode opcode, Label[] labels) {
            WriteIL(opcode, labels);
            ilg.Emit(opcode, labels);
        }
        public void Emit(OpCode opcode, LocalBuilder local) {
            WriteIL(opcode, local);
            ilg.Emit(opcode, local);
        }
        public void Emit(OpCode opcode, long arg) {
            WriteIL(opcode, arg);
            ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, MethodInfo meth) {
            WriteIL(opcode, meth);
            ilg.Emit(opcode, meth);
        }
        public void Emit(OpCode opcode, sbyte arg) {
            WriteIL(opcode, arg);
            ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, short arg) {
            WriteIL(opcode, arg);
            ilg.Emit(opcode, arg);
        }
        public void Emit(OpCode opcode, SignatureHelper signature) {
            WriteIL(opcode, signature);
            ilg.Emit(opcode, signature);
        }
        public void Emit(OpCode opcode, string str) {
            WriteIL(opcode, str);
            ilg.Emit(opcode, str);
        }
        public void Emit(OpCode opcode, Type cls) {
            WriteIL(opcode, cls);
            ilg.Emit(opcode, cls);
        }
        public void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes) {
            WriteIL(opcode, methodInfo, optionalParameterTypes);
            ilg.EmitCall(opcode, methodInfo, optionalParameterTypes);
        }
        public void EndExceptionBlock() {
            ilg.EndExceptionBlock();
        }
        public void MarkLabel(Label loc) {
            WriteIL(loc);
            ilg.MarkLabel(loc);
        }
        public void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn) {
            ilg.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
        }
        public void EmitWriteLine(string value) {
            ilg.EmitWriteLine(value);
        }

        internal void EmitTraceBackTryBlockStart() {
            if (Options.TraceBackSupport) {
                // push a try for traceback support
                PushTryBlock();
                BeginExceptionBlock();
            }
        }

        internal void EmitTraceBackFaultBlock(string name, string filename) {
            if (Options.TraceBackSupport) {
                // push a fault block (runs only if there's an exception, doesn't handle the exception)
                PopTargets();
                if (IsDynamicMethod) {
                    BeginCatchBlock(typeof(Exception));
                } else {
                    BeginFaultBlock();
                }

                EmitCallerContext();
                EmitString(name);
                EmitString(filename);
                EmitGetCurrentLine();
                EmitCall(typeof(Ops), "UpdateTraceBack");

                // end the exception block
                if (IsDynamicMethod) {
                    Emit(OpCodes.Rethrow);
                }
                EndExceptionBlock();
            }
        }
        #endregion

        #region IL Debugging Support

        static int count = 0;

        [Conditional("DEBUG")]
        private void InitializeILWriter() {
            Debug.Assert(Options.ILDebug);
            // This ensures that it is not a DynamicMethod
            Debug.Assert(typeGen != null);

            string mname = methodInfo.Name;
            foreach (char ch in System.IO.Path.GetInvalidFileNameChars()) {
                mname = mname.Replace(ch, '_');
            }
            string filename = Environment.GetEnvironmentVariable("TEMP") + "\\gen_" + mname + "_" + System.Threading.Interlocked.Increment(ref count) + ".il";
            ilOut = new StreamWriter(filename);

            debugSymbolWriter = typeGen.myAssembly.myModule.DefineDocument(
                filename,
                SymLanguageType.ILAssembly,
                SymLanguageVendor.Microsoft,
                SymDocumentType.Text);
        }

        [Conditional("DEBUG")]
        private void WriteSignature(string name, Type[] paramTypes) {
            WriteIL("{0} (", name);
            foreach (Type type in paramTypes) {
                WriteIL("\t{0}", type.FullName);
            }
            WriteIL(")");
        }
        [Conditional("DEBUG")]
        private void WriteIL(string format, object arg0) {
            WriteIL(String.Format(format, arg0));
        }
        [Conditional("DEBUG")]
        private void WriteIL(string format, object arg0, object arg1) {
            WriteIL(String.Format(format, arg0, arg1));
        }
        [Conditional("DEBUG")]
        private void WriteIL(string format, object arg0, object arg1, object arg2) {
            WriteIL(String.Format(format, arg0, arg1, arg2));
        }

        [Conditional("DEBUG")]
        private void WriteIL(string format, object arg0, object arg1, object arg2, object arg3, object arg4) {
            WriteIL(String.Format(format, arg0, arg1, arg2, arg3, arg4));
        }
        [Conditional("DEBUG")]
        private void WriteIL(string str) {
            if (!Options.ILDebug) return;

            if (ilOut == null) {
                InitializeILWriter();
            }

            curLine++;
            ilOut.WriteLine(str);

            if (debugSymbolWriter != null) {
                MarkSequencePoint(
                 debugSymbolWriter,
                 curLine, 1,
                 curLine, str.Length + 1
                 );
            }
        }

        private static string MakeSignature(MethodBase mb) {
            if (mb is MethodBuilder) return ((MethodBuilder)mb).Signature;
            if (mb is ConstructorBuilder) return ((ConstructorBuilder)mb).Signature;

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
            if (Options.ILDebug) WriteIL(op.ToString());
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, byte arg) {
            if (Options.ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, ConstructorInfo con) {
            if (Options.ILDebug) WriteIL("{0}\t{1}({2})", opcode, con.DeclaringType, MakeSignature(con));
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, double arg) {
            if (Options.ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, FieldInfo field) {
            if (Options.ILDebug) WriteIL("{0}\t{1}.{2}", opcode, field.DeclaringType, field.Name);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, float arg) {
            if (Options.ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, int arg) {
            if (Options.ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, Label label) {
            if (Options.ILDebug) WriteIL("{0}\tlabel_{1}", opcode, GetLabelId(label));
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, Label[] labels) {
            if (Options.ILDebug) {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append(opcode.ToString());
                sb.Append("\t[");
                for (int i = 0; i < labels.Length; i++) {
                    if (i != 0) sb.Append(", ");
                    sb.Append("label_" + GetLabelId(labels[i]).ToString());
                }
                sb.Append("]");
                WriteIL(sb.ToString());
            }
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, LocalBuilder local) {
            if (Options.ILDebug) WriteIL("{0}\t{1}", opcode, local);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, long arg) {
            if (Options.ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, MethodInfo meth) {
            if (Options.ILDebug) WriteIL("{0}\t{1} {2}.{3}({4})", opcode, meth.ReturnType.FullName, meth.DeclaringType, meth.Name, MakeSignature(meth));
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, sbyte arg) {
            if (Options.ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, short arg) {
            if (Options.ILDebug) WriteIL("{0}\t{1}", opcode, arg);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, SignatureHelper signature) {
            if (Options.ILDebug) WriteIL("{0}\t{1}", opcode, signature);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, string str) {
            if (Options.ILDebug) WriteIL("{0}\t{1}", opcode, str);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, Type cls) {
            if (Options.ILDebug) WriteIL("{0}\t{1}", opcode, cls.FullName);
        }
        [Conditional("DEBUG")]
        private void WriteIL(OpCode opcode, MethodInfo meth, Type[] optionalParameterTypes) {
            if (Options.ILDebug) WriteIL("{0}\t{1} {2}.{3}({4})", opcode, meth.ReturnType.FullName, meth.DeclaringType, meth.Name, MakeSignature(meth));
        }
        [Conditional("DEBUG")]
        private void WriteIL(Label l) {
            if (Options.ILDebug) WriteIL("label_{0}:", GetLabelId(l).ToString());
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
            if (ilOut != null) {
                ilOut.Dispose();
            }
        }

        #endregion
    }
}
