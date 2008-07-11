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

using System.Diagnostics;
using System.Reflection.Emit;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace System.Linq.Expressions {

    // The part of the LambdaCompiler dealing with low level control flow
    // break, contiue, return, exceptions, etc
    partial class LambdaCompiler {

        private struct ReturnBlock {
            internal LocalBuilder ReturnValue;
            internal Label ReturnStart;
        }

        private void PushExceptionBlock(TargetBlockType type, LocalBuilder returnFlag) {
            if (_targets.Count == 0) {
                _targets.Push(new Targets(Targets.NoLabel, Targets.NoLabel, type, returnFlag, null));
            } else {
                Targets t = _targets.Peek();
                _targets.Push(new Targets(t.BreakLabel, t.ContinueLabel, type, returnFlag ?? t.FinallyReturns, null));
            }
        }

        private void PushTargets(Label? breakTarget, Label? continueTarget, LabelTarget label) {
            if (_targets.Count == 0) {
                _targets.Push(new Targets(breakTarget, continueTarget, BlockType, null, label));
            } else {
                Targets t = _targets.Peek();
                TargetBlockType bt = t.BlockType;
                if (bt == TargetBlockType.Finally && label != null) {
                    bt = TargetBlockType.LoopInFinally;
                }
                _targets.Push(new Targets(breakTarget, continueTarget, bt, t.FinallyReturns, label));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "type")]
        private void PopTargets(TargetBlockType type) {
            Targets t = _targets.Pop();
            Debug.Assert(t.BlockType == type);
        }

        private void PopTargets() {
            _targets.Pop();
        }

        // TODO: Cleanup, hacky!!!
        private void CheckAndPushTargets(LabelTarget label) {
            for (int i = _targets.Count - 1; i >= 0; i--) {
                if (_targets[i].Label == label) {
                    PushTargets(_targets[i].BreakLabel, _targets[i].ContinueLabel, null);
                    return;
                }
            }

            throw new InvalidOperationException("Statement not on the stack");
        }

        private void EmitBreak() {
            Targets t = _targets.Peek();
            int finallyIndex = -1;
            switch (t.BlockType) {
                default:
                case TargetBlockType.Normal:
                case TargetBlockType.LoopInFinally:
                    if (t.BreakLabel.HasValue)
                        _ilg.Emit(OpCodes.Br, t.BreakLabel.Value);
                    else
                        throw new InvalidOperationException();
                    break;
                case TargetBlockType.Try:
                case TargetBlockType.Else:
                case TargetBlockType.Catch:
                    for (int i = _targets.Count - 1; i >= 0; i--) {
                        if (_targets[i].BlockType == TargetBlockType.Finally) {
                            finallyIndex = i;
                            break;
                        }

                        if (_targets[i].BlockType == TargetBlockType.LoopInFinally ||
                            !_targets[i].BreakLabel.HasValue)
                            break;
                    }

                    if (finallyIndex == -1) {
                        if (t.BreakLabel.HasValue)
                            _ilg.Emit(OpCodes.Leave, t.BreakLabel.Value);
                        else
                            throw new InvalidOperationException();
                    } else {
                        if (!_targets[finallyIndex].LeaveLabel.HasValue)
                            _targets[finallyIndex].LeaveLabel = _ilg.DefineLabel();

                        _ilg.EmitInt(LambdaCompiler.BranchForBreak);
                        _ilg.Emit(OpCodes.Stloc, _targets[finallyIndex].FinallyReturns);

                        _ilg.Emit(OpCodes.Leave, _targets[finallyIndex].LeaveLabel.Value);
                    }
                    break;
                case TargetBlockType.Finally:
                    _ilg.EmitInt(LambdaCompiler.BranchForBreak);
                    _ilg.Emit(OpCodes.Stloc, t.FinallyReturns);
                    _ilg.Emit(OpCodes.Endfinally);
                    break;
            }
        }

        private void EmitContinue() {
            Targets t = _targets.Peek();
            switch (t.BlockType) {
                default:
                case TargetBlockType.Normal:
                case TargetBlockType.LoopInFinally:
                    if (t.ContinueLabel.HasValue)
                        _ilg.Emit(OpCodes.Br, t.ContinueLabel.Value);
                    else
                        throw new InvalidOperationException();
                    break;
                case TargetBlockType.Try:
                case TargetBlockType.Else:
                case TargetBlockType.Catch:
                    if (t.ContinueLabel.HasValue)
                        _ilg.Emit(OpCodes.Leave, t.ContinueLabel.Value);
                    else
                        throw new InvalidOperationException();
                    break;
                case TargetBlockType.Finally:
                    _ilg.EmitInt(LambdaCompiler.BranchForContinue);
                    _ilg.Emit(OpCodes.Stloc, t.FinallyReturns);
                    _ilg.Emit(OpCodes.Endfinally);
                    break;
            }
        }

        private void EmitReturn() {
            int finallyIndex = -1;
            switch (BlockType) {
                default:
                case TargetBlockType.Normal:
                    _ilg.Emit(OpCodes.Ret);
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

                    EnsureReturnBlock();
                    Debug.Assert(_returnBlock.HasValue);
                    if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                        _ilg.Emit(OpCodes.Stloc, _returnBlock.Value.ReturnValue);
                    }

                    if (finallyIndex == -1) {
                        // emit the real return
                        _ilg.Emit(OpCodes.Leave, _returnBlock.Value.ReturnStart);
                    } else {
                        // need to leave into the inner most finally block,
                        // the finally block will fall through and check
                        // the return value.
                        if (!_targets[finallyIndex].LeaveLabel.HasValue)
                            _targets[finallyIndex].LeaveLabel = _ilg.DefineLabel();

                        _ilg.EmitInt(LambdaCompiler.BranchForReturn);
                        _ilg.Emit(OpCodes.Stloc, _targets[finallyIndex].FinallyReturns);

                        _ilg.Emit(OpCodes.Leave, _targets[finallyIndex].LeaveLabel.Value);
                    }
                    break;
                case TargetBlockType.LoopInFinally:
                case TargetBlockType.Finally: {
                        Targets t = _targets.Peek();
                        EnsureReturnBlock();
                        if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                            _ilg.Emit(OpCodes.Stloc, _returnBlock.Value.ReturnValue);
                        }
                        // Assert check ensures that those who pushed the block with finallyReturns as null 
                        // should not yield in their lambdas.
                        Debug.Assert(t.FinallyReturns != null);
                        _ilg.EmitInt(LambdaCompiler.BranchForReturn);
                        _ilg.Emit(OpCodes.Stloc, t.FinallyReturns);
                        _ilg.Emit(OpCodes.Endfinally);
                        break;
                    }
            }
        }


        private void EmitReturnValue() {
            EnsureReturnBlock();
            if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                _ilg.Emit(OpCodes.Ldloc, _returnBlock.Value.ReturnValue);
            }
        }

        private void EmitReturn(Expression expr) {
            if (IsGeneratorBody) {
                EmitReturnInGenerator(expr);
            } else {
                if (expr == null) {
                    Debug.Assert(CompilerHelpers.GetReturnType(_method) == typeof(void));
                } else {
                    Type result = CompilerHelpers.GetReturnType(_method);
                    Debug.Assert(result.IsAssignableFrom(expr.Type));
                    EmitExpression(expr);
                    if (!TypeUtils.CanAssign(result, expr.Type)) {
                        EmitImplicitCast(expr.Type, result);
                    }
                }
                EmitReturn();
            }
        }

        private void EmitReturnInGenerator(Expression expr) {
            EmitSetGeneratorReturnValue(expr);

            _ilg.EmitInt(0);
            EmitReturn();
        }

        private void EmitYield(Expression expr, YieldTarget target) {
            ContractUtils.RequiresNotNull(expr, "expr");

            EmitSetGeneratorReturnValue(expr);
            EmitUpdateGeneratorLocation(target.Index);

            // Mark that we are yielding, which will ensure we skip
            // all of the finally bodies that are on the way to exit

            _ilg.EmitInt(GotoRouterYielding);
            _ilg.Emit(OpCodes.Stloc, GotoRouter);

            _ilg.EmitInt(1);
            EmitReturn();

            _ilg.MarkLabel(target.EnsureLabel(this));
            // Reached the routing destination, set router to GotoRouterNone
            _ilg.EmitInt(GotoRouterNone);
            _ilg.Emit(OpCodes.Stloc, GotoRouter);
        }

        private void EmitSetGeneratorReturnValue(Expression expr) {
            EmitLambdaArgument(1);
            EmitExpressionAsObjectOrNull(expr);
            _ilg.Emit(OpCodes.Stind_Ref);
        }

        private void EmitUpdateGeneratorLocation(int index) {
            EmitLambdaArgument(0);
            _ilg.EmitInt(index);
            _ilg.EmitFieldSet(typeof(Generator).GetField("location"));
        }

        private void EnsureReturnBlock() {
            if (!_returnBlock.HasValue) {
                ReturnBlock val = new ReturnBlock();

                if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                    val.ReturnValue = GetNamedLocal(CompilerHelpers.GetReturnType(_method), "retval");
                }
                val.ReturnStart = _ilg.DefineLabel();

                _returnBlock = val;
            }
        }

        private void EndExceptionBlock() {
            if (_targets.Count > 0) {
                Targets t = _targets.Peek();
                Debug.Assert(t.BlockType != TargetBlockType.LoopInFinally);
                if (t.BlockType == TargetBlockType.Finally && t.LeaveLabel.HasValue) {
                    _ilg.MarkLabel(t.LeaveLabel.Value);
                }
            }

            _ilg.EndExceptionBlock();
        }        

        private TryStatementInfo GetTsi(TryStatement node) {
            if (_generatorInfo == null) {
                return null;
            }
            return _generatorInfo.TryGetTsi(node);
        }

        private YieldTarget GetYieldTarget(YieldStatement node) {
            if (_generatorInfo == null) {
                return null;
            }
            return _generatorInfo.TryGetYieldTarget(node);
        }    
    }
}
