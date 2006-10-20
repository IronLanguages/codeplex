/* *********************************************************************************
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
using System.Collections.Generic;

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

using System.Diagnostics;
using IronPython.Runtime;

using IronMath;

namespace IronPython.Compiler {
    class ReflectOptimizer {
#if DEBUG
        private static int typeCount;
#endif

        #region Public API Surface

        /// <summary>
        /// EmitFunction emits optimized callers for the specified infos into
        /// the given type generator.
        /// </summary>
        /// <param name="tg"></param>
        /// <param name="infos"></param>
        public static void EmitFunction(TypeGen tg, MethodTracker[] infos){
            GenerateAllTargets(tg, infos[0].Name, infos, FunctionType.None);
        }

        /// <summary>
        /// MakeFunction creates an optimized BuiltinFunction from a BuiltinFunction.  Usually
        /// the builtin function is a reflected method, but it could also be a builtin function
        /// that is getting more methods added to it.
        /// </summary>
        public static BuiltinFunction MakeFunction(BuiltinFunction rm) {
            string funcName = rm.Name;
            if (Options.OptimizeReflectCalls && CanOptimize(rm.Targets)) {
                MethodInfo[] targets = GenerateAllTargets(MethodTracker.GetTrackerArray(rm.Targets), rm.FunctionType);

                return BuiltinFunction.Make(
                    funcName,
                    targets,
                    rm.Targets,
                    rm.FunctionType);
            }
            return null;
        }        

        #endregion

        #region Internal Implementation Details        
               
        private static bool CanOptimize(MethodBase[] infos) {
            for (int i = 0; i < infos.Length; i++) {
                if (!CanOptimize(infos[i])) {
                    return false;
                }
            }

            return true;
        }

        private static bool CanOptimize(MethodBase mi) {
            if (mi.ContainsGenericParameters) return false;
            if (mi.IsAbstract) return false;
            if (mi.IsFamily || mi.IsPrivate || mi.IsFamilyOrAssembly) return false;
            if (!mi.DeclaringType.IsVisible) {
                return false;
            } 

            // need support for variable depth params-args in the ParamTree to support this.
            if (mi.GetParameters().Length > (mi.IsStatic ? Ops.MaximumCallArgs : Ops.MaximumCallArgs - 1)) return false; 

            // can't handle operators due to forward & reverse forms being in seperate ReflectedMethods
            if (mi.IsSpecialName && mi.Name.StartsWith("op_")) return false;

            return true;
        }
       
        private static MethodInfo[] GenerateAllTargets(MethodTracker [] methods, FunctionType funcType){
            TypeGen tg = null;
            string name = methods[0].Name;
#if DEBUG
            // debug only: we want to be able to inspect the generated optimized methods
            // w/ ildasm, as well as perform verification over them.  Therefore we save
            // most methods here.  We don't save methods that are optimized in Snippets
            // (we get an exception making a reference to the type) and we also don't save
            // non-GAC / non-IronPython assemblies (eg test1.dll) that get created during
            // the test pass (as they disappear, and then we can't verify the assembly).

            bool fSaveType = false;
            if (Options.SaveAndReloadBinaries && 
                methods[0].DeclaringType.Assembly != OutputGenerator.Snippets.myAssembly &&
                (methods[0].DeclaringType.Assembly.GlobalAssemblyCache || methods[0].DeclaringType.Assembly.FullName.StartsWith("IronPython"))) {
                fSaveType = true;

                tg = OutputGenerator.Snippets.DefinePublicType("IronPython.OptimizedMethods" + System.Threading.Interlocked.Increment(ref typeCount).ToString(), typeof(object));
                name = "Optimized" + methods[0].Name;
            }
#endif
            MethodInfo [] res = GenerateAllTargets(tg, name, methods, funcType);
#if DEBUG
            if (fSaveType) {
                // get the baked methods and return those, not the method builders..
                Type t = tg.FinishType();
                MethodInfo [] mis= t.GetMethods();

                int writeCount = 0;
                foreach (MethodInfo mi in mis) {
                    if (mi.Name == name) res[writeCount++] = mi;
                }                                
            }
#endif

            return res;
        }

        /// <summary>
        /// Generates all the target methods for the given method set.
        /// </summary>
        private static MethodInfo[] GenerateAllTargets(TypeGen tg, string name, MethodTracker[] methods, FunctionType funcType) {
            List<MethodInfo> targets = new List<MethodInfo>();
            ParamTree[] trees = ParamTree.BuildAllTrees(methods, funcType);

            for (int i = 0; i < trees.Length; i++) {
                targets.Add(GenerateTargetMethod(tg, trees[i], name));
            }

            return targets.ToArray();
        }

        /// <summary>
        /// Generates a Target[argCnt] method where argCnt can be N for params or a number
        /// </summary>
        private static MethodInfo GenerateTargetMethod(TypeGen tg, ParamTree prms, string name) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Compiler, name);

            MethodInfo res;
            CodeGen cg = DefineMethod(tg, name, prms.ArgumentCount, (prms.FunctionType & FunctionType.Params) !=0 );
            MultiCallGenerator mcg = new MultiCallGenerator(cg, prms, (prms.FunctionType & FunctionType.Params) != 0);

            mcg.DoGenerate();

            if (tg == null) {
                res = cg.CreateDelegateMethodInfo();
            } else {
                res = cg.MethodInfo;
            }
            return res;
        }
                
        /// <summary>
        /// Helper function for defining the optimized method either in a snippets
        /// assembly or as a dynamic method and setting the parameter information
        /// correctly.
        /// </summary>
        private static CodeGen DefineMethod(TypeGen tg, string name, int argCnt, bool paramsMethod) {
            Type[] sig;
            if (paramsMethod) sig = new Type[] { typeof(object[]) };
            else sig = CompilerHelpers.MakeRepeatedArray(typeof(object), argCnt);

            CodeGen cg;
            if (tg == null) {
                cg = OutputGenerator.Snippets.DefineDynamicMethod(name,
                                                          typeof(object),
                                                          sig);                
            } else {
                // define the method on the type, it'll take care of ParameterBuilder
                // for us.
                string[] argNames = CompilerHelpers.MakeRepeatedArray("", argCnt);
                cg = tg.DefineMethod(
                    name,
                    typeof(object),
                    sig,
                    argNames);
            }
            return cg;
        }

        /// <summary>
        /// Our basic strategy is to try and find an identity match. If 
        /// we get all idenity matches then no other method can beat it
        /// and we can call it. If we get a parameter that is a non-identity
        /// match but there's no other matches then it's as good as an identity
        /// match.
        /// 
        /// If we get multiple non-perfect matches then life is a little harder. 
        /// For that case we have to try all the sub-params. Again, if we
        /// get all identity matches we're good. If we have no conversions we're
        /// also good, we just pop back out and continue trying more If we have
        /// some conversions things are interesting - but we're actually ok.
        /// ReflectedMethodBase.TryCall() always has the "left" most method win
        /// if you have 2 inversed parameters (beter/worse & worse/better). So
        /// we don't need to inspect anyone else - because even if they do have a
        /// better later parameter the earlier better parameter effectively
        /// overrules it. (!!!the one exception would be earlier tied parameters -what about those?)
        /// 
        /// The code we're generating is basically the same as this pseudo
        /// code (except we're inlining it all into one method):
        /// 
        /// void TryParam(int n)
        ///     PreArgument:
        ///         int convCount = 0;
        ///         retrys = [] // should be done lazily
        /// 
        ///     Walk:
        ///         foreach(type in allTypesForParam(n))
        ///             res = Converter.TryConvert(value, type, out conv);      - EmitTypeCheck
        ///             if(conv == Conv.Identity)                               - EmitIdentityCheck()
        ///                 if(n != paramCnt) TryParam(n+1);                    - EmitCallOrNextParam()
        ///                 else CallMethod(); 
        ///             else if(conv == Conv.None) continue;                    - EmitNoConversionContinue() 
        /// 
        ///         - EmitNonIdentityConversion:
        ///         // insert sorted
        ///         convCount++;                                                - EmitIncrementConversionCount()
        ///         for(int i = 0; i<retrys.count;i++)
        ///             ctv = retrys[i]
        ///             if(ctv[0] > conv)
        ///                 retrys.insert(i, (conv, type, value))
        ///                 fFound = true

        ///         if(!fFound)
        ///             retrys.append( (conv, type, value))
        /// 
        ///     PostArgument:
        ///         EmitHandleNoIdentityConversion:
        ///         if(convCount == 0) throw TypeError()
        ///         if(convCount == 1) TryParam(n+1) // switch to the TryParam/CallMethod block above
        /// 
        ///         foreach((conv, type, value) in retrys)
        ///             TryParam(n+1)
        /// 
        /// the parameters are pre-sorted (via our tree) so that we will go from
        /// most specific to least specific classes. This should give identical
        /// results to ReflectedMethodBase.TryCall. We also take opportunities at
        /// optimizing this algorithm because it is inlined.
        /// 
        /// </summary>

        class MultiCallGenerator  {
            private const int RetrysConversionIndex = 0;
            private const int RetrysTypeIndex = 1;
            private const int RetrysValueIndex = 2;

            private readonly CodeGen cg;
            private readonly IList<MethodTracker> allMeths;
            private readonly int argCnt;
            private readonly bool isParams;             // true if the call target we're generating has a signature of params object [].

            private PerParamState[] paramState;
            private PerParamSlots[] paramSlots;
            private Slot outConv; // temp, the result of the last conversion
            private Slot bestConversion;    // for one arg case ,stores the best conversion value.
            private List<KeyValuePair<int, Slot>> outParams;
            private ParamTree prmTree;
            private int nonIdentityCount;
            private Label noConversions; // label where we throw an exception when there's no conversions

            public MultiCallGenerator(CodeGen codeGen, ParamTree paramTree, bool paramsMethod) {
                cg = codeGen;
                argCnt = paramTree.ArgumentCount;
                allMeths = paramTree.Methods;
                paramState = new PerParamState[argCnt];
                paramSlots = new PerParamSlots[argCnt];
                prmTree = paramTree;
                isParams = paramsMethod;

                for (int i = 0; i < argCnt; i++) {
                    paramSlots[i] = new PerParamSlots(cg);
                }

                outConv = cg.GetLocalTmp(typeof(int)); // temp for result of type conversion
                noConversions = cg.DefineLabel();
            }

            public void DoGenerate() {
                PreArgument(0, prmTree.Children.Count);

                for (int i = 0; i < prmTree.Children.Count; i++) {
                    Walk(0, 0, prmTree.Children[i]);
                }

                PostArgument(0);

                FinishMethod();
            }


            /// <summary>
            /// Finishes the creation of the multi-call method writing
            /// out the method epilogue and any common code blocks.
            /// </summary>
            public void FinishMethod() {
                if (nonIdentityCount != 0) {
                    cg.MarkLabel(noConversions);

                    // no conversions for this argument, throw an exception.

                    cg.EmitString("bad args for method");
                    cg.EmitObjectArray(new Expr[0]);
                    cg.EmitCall(typeof(Ops), "TypeError");
                    cg.Emit(OpCodes.Throw);
                }
#if RO_DEBUG
                else {
                    // code markers throw off verifiablity, force a return.
                    cg.Emit(OpCodes.Ldnull);
                    cg.Emit(OpCodes.Ret);
                }
#endif
            }

            #region IParamWalker Members

            /// <summary>
            /// Called at the beginning of each argument allowing
            /// us to setup various state to track the various types
            /// used for this argument.
            /// </summary>
            public void PreArgument(int param, int typeCount) {
                switch (argCnt) {
                    case 0:
                        // we could have multiple methods w/ defaults, we dispatch
                        // to the one w/ the least # of defaults.
                        MethodTracker best = allMeths[0];
                        for (int i = 1; i < allMeths.Count; i++) {
                            if (allMeths[i].GetParameters().Length < best.GetParameters().Length)
                                best = allMeths[i];
                        }

                        EmitFinalCall(best);
                        break;
                    case 1:
                        // single argument, retrys are simpler...
                        bestConversion = cg.GetLocalTmp(typeof(object));
                        cg.EmitInt((int)Conversion.None);
                        paramSlots[0].SingleParamConversion.EmitSet(cg);
                        paramState[param] = new PerParamState(typeCount);
                        break;
                    default:
                        EmitCodeMarker("Begin PreArgument " + param.ToString());

                        // zero initialize the conv count, we'll use it to
                        // lazy initialize other state later.
                        //!!! very first parameter we could skip this...
                        //EmitCodeLogging("Resetting conversion count: " + param);
                        cg.EmitInt(0);
                        paramSlots[param].ConvertedCount.EmitSet(cg);
                        paramState[param] = new PerParamState(typeCount);

                        EmitCodeMarker("End PreArgument " + param.ToString());
                        break;
                }
            }

            /// <summary>
            /// Called for each parameter type at each parameter index.
            /// </summary>
            public void Walk(int param, int outParams, ParamTreeNode node) {
                IList<MethodTracker> methods = node.Methods;
                Type paramType = node.ParamType;

                EmitCodeMarker("Begin Argument " + param.ToString() + " of " + argCnt.ToString());

                if ((node.Flags & ParamTree.NodeFlags.Out) == 0) {
                    if (param < argCnt) {
                        MarkThisParameter(param);
                        paramState[param].lastType = paramType;

                        if (paramType != typeof(object)) {
                            Label nextParam;
                            Label okConv = CreateOkConversionLabel(param);

                            // check and see if we pass an empty param array to th
                            // final function...
                            EmitEmptyParamsCheck(param+outParams, okConv);

                            if ((node.Flags & ParamTree.NodeFlags.Params) != 0) {
                                EmitNullParamsCheck(param, node, okConv);
                            }

                            EmitTypeCheck(cg, GetArgumentSlot(param), outConv, node);
                            paramSlots[param].ConversionValue.EmitSet(cg);

                            Label notIdentity = EmitIdentityCheck();

                            cg.MarkLabel(okConv);

                            nextParam = EmitCallOrNextParam(param, methods);

                            // we don't have an identity conversion
                            cg.MarkLabel(notIdentity);

                            EmitNoConversionContinue(param);

                            // we have some other conversion
                            EmitNonIdentityConversion(param);

                            // and onto the next parameter...
                            cg.MarkLabel(nextParam);
                        } else if (param == (argCnt - 1)) {
                            // last argument, we should only have 1 method here.
                            Debug.Assert(methods.Count == 1);

                            EmitFinalCall(methods[0]);
                        }
                        // otherwise no need to save object - we'll just load it from the argument
                    } else {
                        // we better not be ambigious now...
                        Debug.Assert(methods.Count == 1);

                        EmitDefaultValue(param, methods[0]);
                    }

                    EmitCodeMarker("End Argument " + param.ToString());
                }

                WalkChildren(param, outParams, node);       
            }

            private void WalkChildren(int param, int outParams, ParamTreeNode node) {
                if (node.Children.Count != 0) {
                    if ((node.Flags & ParamTree.NodeFlags.Out) != 0) {
                        foreach (ParamTreeNode innerNode in node.Children) {
                            Walk(param, outParams + 1, innerNode);
                        }
                    } else {
                        PreArgument(param + 1, node.Children.Count);

                        foreach (ParamTreeNode innerNode in node.Children) {
                            Walk(param + 1, outParams, innerNode);
                        }

                        PostArgument(param + 1);
                    }
                }
            }

            /// <summary>
            /// Called after processing each argument allowing
            /// us to cleanup from failed conversions or handle
            /// dispatch between one or more contenders.
            /// </summary>
            public void PostArgument(int param) {
                switch (argCnt) {
                    case 0:
                        // no collisions are possible.
                        break;
                    case 1:
                        // we should know the best conversion now and
                        // should dispatch straight to it.
                        cg.MarkLabel(paramState[param].nextArgLabel);

                        bestConversion.EmitGet(cg);
                        paramSlots[param].ConversionValue.EmitSet(cg);

                        EmitSingleConversionJump_OneArg();
                        break;
                    default:
                        EmitCodeMarker("Begin PostArgument " + param.ToString());

                        cg.MarkLabel(paramState[param].nextArgLabel);
                        EmitHandleNoIdentityConversion(param);

                        EmitCodeMarker("End PostArgument " + param.ToString());
                        break;
                }
            }

            #endregion

            #region Private implementation details

            private void EmitEmptyParamsCheck(int param, Label okConv) {
                if (isParams && param >= GetMinimumArgCount()) {
                    // empty params array - that's a perfect conversion.
                    // typical example is 'abc'.Split() where we want to pass
                    // an empty array for the dispatch.
                    cg.EmitInt(param);
                    cg.EmitArgGet(0);
                    cg.EmitCall(typeof(Array), "get_Length");

                    cg.Emit(OpCodes.Bge, okConv);
                }
            }

            private void EmitNullParamsCheck(int param, ParamTreeNode node, Label okConv) {
                if (param == (argCnt - 1)) {
                    EmitArgument(param);
                    cg.Emit(OpCodes.Ldnull);
                    cg.Emit(OpCodes.Beq, okConv);
                } else if (param == argCnt) {
                    // check to see if we're passing an array to params array
                    EmitArgument(param);
                    cg.Emit(OpCodes.Dup);
                    cg.EmitCall(typeof(object), "GetType");
                    cg.EmitType(node.ParamType);
                    cg.Emit(OpCodes.Beq, okConv);
                    cg.Emit(OpCodes.Pop);   // pop off argument value
                }
            }

            private Slot GetOrCreateOutSlot(int param, Type type){
                if (outParams == null)  outParams = new List<KeyValuePair<int, Slot>>();

                for (int i = 0; i < outParams.Count; i++) {
                    if (outParams[i].Key == param && outParams[i].Value.Type == type) {
                        return outParams[i].Value;
                    }
                }

                Slot slot = new LocalSlot(cg.DeclareLocal(type), cg);
                outParams.Add(new KeyValuePair<int, Slot>(param, slot));
                return slot;
            }

            private Slot GetArgumentSlot(int param) {
                if (isParams) return new ParamArraySlot(cg.GetArgumentSlot(0), param);

                return cg.GetArgumentSlot(param);
            }

            /// <summary>
            /// Emits one of our arguments passed to us.  param is the index
            /// to be emitted.  0 for the first, 1 for the 2nd, etc...  
            /// </summary>
            private void EmitArgument(int param) {
                GetArgumentSlot(param).EmitGet(cg);
            }

            private void EmitArgumentAddress(int param) {
                GetArgumentSlot(param).EmitGetAddr(cg);
            }

            private int GetMinimumArgCount() {
                int minArgCnt = Int32.MaxValue;
                for (int i = 0; i < allMeths.Count; i++) {
                    if (allMeths[i].IsParamsMethod) {
                        minArgCnt = Math.Min(minArgCnt, allMeths[i].StaticArgs-1);
                    } else {
                        minArgCnt = Math.Min(minArgCnt, allMeths[i].StaticArgs);
                    }
                }
                return minArgCnt;
            }
          
            /// <summary>
            /// Emits code for when we don't have an identity conversion
            /// for a specific parameter. We could either have no conversions
            /// (in which case the method is uncallable), one conversion that
            /// wasn't an identity conversion, or multiple conversions that 
            /// we need to do more checking for.
            /// </summary>
            private void EmitHandleNoIdentityConversion(int param) {
                EmitCodeMarker("Begin Handle No Identity Conversion " + param.ToString());
                // if the last type was object & we only had one
                // type then there's no possibility of needing 
                // additional type checks - our type check always succeeds.

                if (paramState[param].lastType != typeof(object) ||
                    paramState[param].curTypeIndex != 1) {

                    nonIdentityCount++;

                    Label multipleConversions = cg.DefineLabel();
                    paramSlots[param].ConvertedCount.EmitGet(cg);

                    cg.EmitInt(1);
                    cg.Emit(OpCodes.Bne_Un, multipleConversions);

                    EmitSingleConversionJump(param);

                    cg.MarkLabel(multipleConversions);

                    // check if we have zero conversions
                    paramSlots[param].ConvertedCount.EmitGet(cg);
                    cg.EmitInt(0);

                    // we could have more arguments to check, pop back up and keep processing those - we may
                    // have to pop up more than one level if we're at the last parameter for the previous arg.
                    int curParam = param-1;
                    while(curParam > 0 && paramState[curParam].curTypeIndex == paramState[curParam].TypeCount) {
                        curParam--;
                    }

                    if (curParam < 0) {
                        // conversions failed, branch to single noConversions label
                        cg.Emit(OpCodes.Beq, noConversions);
                    } else {
                        // branch to next argument to be processed at the correct level.
                        cg.Emit(OpCodes.Beq, paramState[curParam].nextArgLabel);   
                    } 

                    // more than one conversion, we try each one 
                    // in order (only if we have more than 1 param type to try)
                    if (paramState[param].curTypeIndex != 1) {  
                        EmitCallRetrys(param);
                    }
                }

                EmitCodeMarker("End Handle No Identity Conversion " + param.ToString());
            }

            /// <summary>
            /// Emits a jump for the case where we have only
            /// a single conversion. we just branch back to the
            /// label that originally succeeded and proceed as
            /// if we had an identity conversion. This is
            /// just a single switch statement that points
            /// back at all the relevant labels.
            /// </summary>
            private void EmitSingleConversionJump(int param) {
                // re-load the value because we could have corrupted 
                // it w/ intervening type checks.
                paramSlots[param].Retrys.EmitGet(cg);                
                cg.EmitInt(0);
                cg.EmitCall(typeof(List<object[]>), "get_Item");
                cg.EmitInt(RetrysValueIndex);
                cg.Emit(OpCodes.Ldelem_Ref);
                paramSlots[param].ConversionValue.EmitSet(cg);

                if (paramState[param].curTypeIndex != 1) {
                    paramSlots[param].SingleBestCandidate.EmitGet(cg);

                    Label[] jumpTable = new Label[paramState[param].curTypeIndex];

                    for (int i = 0; i < jumpTable.Length; i++) {
                        jumpTable[i] = paramState[param].okConversions[i];
                    }

                    cg.Emit(OpCodes.Switch, jumpTable);

                    AssertUnreachable();
                } else {
                    Debug.Assert(paramState[param].curTypeIndex != 0);

                    // only one place to go to, just do a simple branch.
                    cg.Emit(OpCodes.Br, paramState[param].okConversions[0]);
                }
            }

            private void EmitSingleConversionJump_OneArg() {
                nonIdentityCount++;

                paramSlots[0].SingleParamConversion.EmitGet(cg);
                cg.EmitInt((int)Conversion.None);

                cg.Emit(OpCodes.Beq, noConversions);
                nonIdentityCount++;

                Label[] jumpTable = new Label[paramState[0].curTypeIndex];

                for (int i = 0; i < jumpTable.Length; i++) {
                    jumpTable[i] = paramState[0].okConversions[i];
                }

                paramSlots[0].SingleBestCandidate.EmitGet(cg);
                cg.Emit(OpCodes.Switch, jumpTable);

                AssertUnreachable();

            }

            /// <summary>
            /// We have a non-identity conversion. We update our candidate data
            /// structures for after we've processed all the parameter types here.
            /// </summary>
            private void EmitNonIdentityConversion(int param) {
                int curTypeIndex = paramState[param].curTypeIndex - 1;
                if (argCnt != 1) {
                    EmitIncrementConversionCount(param);

                    // lone candidate is a single index

                    EmitUpdateLoneCandidate(param, curTypeIndex);

                    EmitUpdateRetryList(param, curTypeIndex);

                    // and branch to check the next type
                    // for this parameter (skipping all of the
                    // checks for additional arguments for the
                    // current method set)

                    cg.Emit(OpCodes.Br, paramState[param].nextArgLabel);
                } else {
                    Label worse = cg.DefineLabel();
                    paramSlots[param].SingleParamConversion.EmitGet(cg);
                    outConv.EmitGet(cg);
                    cg.Emit(OpCodes.Blt, worse);

                    // update our best conversion
                    outConv.EmitGet(cg);
                    paramSlots[param].SingleParamConversion.EmitSet(cg);

                    paramSlots[param].ConversionValue.EmitGet(cg);
                    bestConversion.EmitSet(cg);

                    // single argument, we only need to remember the best
                    EmitUpdateLoneCandidate(param, curTypeIndex);

                    cg.MarkLabel(worse);
                }
            }

            private void EmitIncrementConversionCount(int param) {
                //EmitCodeLogging("Incrementing conversion count " + param.ToString());

                // increment our conversion count
                paramSlots[param].ConvertedCount.EmitGet(cg); // once for increment
                paramSlots[param].ConvertedCount.EmitGet(cg); // once for zero check

                cg.EmitInt(1);
                cg.Emit(OpCodes.Add);
                paramSlots[param].ConvertedCount.EmitSet(cg);

                Label notZero = cg.DefineLabel();
                cg.EmitInt(0);
                cg.Emit(OpCodes.Bne_Un, notZero);

                // first time at this level, init retrys.
                cg.EmitNew(typeof(List<object[]>), new Type[0]);
                paramSlots[param].Retrys.EmitSet(cg);
                cg.MarkLabel(notZero);
            }

            private void EmitUpdateLoneCandidate(int param, int curTypeIndex) {
                // update our best candidate

                cg.EmitInt(curTypeIndex);
                paramSlots[param].SingleBestCandidate.EmitSet(cg);
            }

            private void EmitRetryTrio(int param, int curTypeIndex) {
                cg.EmitInt(3);

                cg.Emit(OpCodes.Newarr, typeof(object));
                cg.Emit(OpCodes.Dup);
                cg.EmitInt(RetrysConversionIndex);

                // conversion
                outConv.EmitGet(cg);
                cg.Emit(OpCodes.Box, typeof(int));// Conversion
                cg.Emit(OpCodes.Stelem_Ref);

                // index for the jump table
                cg.Emit(OpCodes.Dup);
                cg.EmitInt(RetrysTypeIndex);
                cg.EmitInt(curTypeIndex);
                cg.Emit(OpCodes.Box, typeof(int));
                cg.Emit(OpCodes.Stelem_Ref);

                // converted value
                cg.Emit(OpCodes.Dup);
                cg.EmitInt(RetrysValueIndex);
                paramSlots[param].ConversionValue.EmitGet(cg);
                cg.Emit(OpCodes.Stelem_Ref);
            }

            private void EmitLoop(CodeLoop init, CodeLoop conditional, CodeLoop increment, CodeContinuable body) {
                Slot curIndex = cg.GetLocalTmp(typeof(int));

                init(curIndex);

                Label searchLoop = cg.DefineLabel();
                Label continueLoop = cg.DefineLabel();

                cg.MarkLabel(searchLoop);

                conditional(curIndex);

                body(curIndex, continueLoop);

                cg.MarkLabel(continueLoop);
                increment(curIndex);

                cg.Emit(OpCodes.Br, searchLoop);
                cg.FreeLocalTmp(curIndex);
            }

            private void EmitUpdateRetryList(int param, int curTypeIndex) {
                EmitCodeMarker("EmitUpdateRetryList Begin");

                Slot trio = cg.GetLocalTmp(typeof(object[]));

                EmitRetryTrio(param, curTypeIndex);
                trio.EmitSet(cg);

                Label noMatch = cg.DefineLabel();
                Label searchDone = cg.DefineLabel();

                EmitLoop(

                // init
                delegate(Slot curIndex) {
                    cg.EmitInt(0);
                    curIndex.EmitSet(cg);
                },

                // loop conditional
                delegate(Slot curIndex) {
                    curIndex.EmitGet(cg);
                    paramSlots[param].Retrys.EmitGet(cg);
                    cg.EmitCall(typeof(List<object[]>), "get_Count");
                    cg.Emit(OpCodes.Beq, noMatch);
                },

                // increment & continue loop
                delegate(Slot curIndex) {
                    // increment & continue
                    curIndex.EmitGet(cg);
                    cg.EmitInt(1);

                    cg.Emit(OpCodes.Add);
                    curIndex.EmitSet(cg);
                },

                // loop body

                delegate(Slot curIndex, Label continueLoop) {
                    // if( ((Conversion)list[curIndex][0]) < lastConversion)

                    paramSlots[param].Retrys.EmitGet(cg);
                    curIndex.EmitGet(cg);
                    cg.EmitCall(typeof(List<object[]>), "get_Item");

                    cg.EmitInt(RetrysConversionIndex);

                    cg.Emit(OpCodes.Ldelem_Ref);
                    cg.Emit(OpCodes.Unbox_Any, typeof(int)); //Conversion

                    outConv.EmitGet(cg);
                    cg.Emit(OpCodes.Clt);
                    cg.Emit(OpCodes.Brfalse, continueLoop);

                    // list.Insert(curIndex, trio)
                    paramSlots[param].Retrys.EmitGet(cg);
                    curIndex.EmitGet(cg);
                    trio.EmitGet(cg);
                    cg.EmitCall(typeof(List<object[]>), "Insert");
                    cg.Emit(OpCodes.Br, searchDone);
                    // curIndex = curIndex + 1

                    // } 

                });

                cg.MarkLabel(noMatch);

                // no match, list.Add(trio)
                paramSlots[param].Retrys.EmitGet(cg);
                trio.EmitGet(cg);
                cg.EmitCall(typeof(List<object[]>), "Add");
                cg.MarkLabel(searchDone);
                cg.FreeLocalTmp(trio);

                EmitCodeMarker("EmitUpdateRetryList End");
            }

            private void EmitCallRetrys(int param) {
                Label loopDone = cg.DefineLabel();
                EmitLoop(

                // loop init
                delegate(Slot curIndex) {
                    cg.EmitInt(0);
                    curIndex.EmitSet(cg);
                },

                // loop conditional
                delegate(Slot curIndex) {
                    curIndex.EmitGet(cg);
                    paramSlots[param].Retrys.EmitGet(cg);
                    cg.EmitCall(typeof(List<object[]>), "get_Count");
                    cg.Emit(OpCodes.Beq, loopDone);
                },

                // loop increment
                delegate(Slot curIndex) {
                    curIndex.EmitGet(cg);
                    cg.EmitInt(1);
                    cg.Emit(OpCodes.Add);
                    curIndex.EmitSet(cg);
                },

                // loop body 
                delegate(Slot curIndex, Label continueLoop) {
                    // get the current objet array
                    Slot curArray = cg.GetLocalTmp(typeof(object[]));

                    paramSlots[param].Retrys.EmitGet(cg);
                    curIndex.EmitGet(cg);
                    cg.EmitCall(typeof(List<object[]>), "get_Item");
                    curArray.EmitSet(cg);

                    // update convValue...

                    curArray.EmitGet(cg);
                    cg.EmitInt(RetrysValueIndex);
                    cg.Emit(OpCodes.Ldelem_Ref);
                    paramSlots[param].ConversionValue.EmitSet(cg);

                    // finally branch to the conversion...
                    curArray.EmitGet(cg);

                    cg.EmitInt(RetrysTypeIndex);
                    cg.Emit(OpCodes.Ldelem_Ref);
                    cg.Emit(OpCodes.Unbox_Any, typeof(int));

                    Label[] jumpTable = new Label[paramState[param].curTypeIndex];
                    for (int i = 0; i < jumpTable.Length; i++) {
                        jumpTable[i] = paramState[param].okConversions[i];
                    }

                    cg.Emit(OpCodes.Switch, jumpTable);

                    AssertUnreachable();
                    cg.FreeLocalTmp(curArray);
                }

                );

                cg.MarkLabel(loopDone);
                // and if we didn't find anything it's
                // onto the next argument...
            }

            [Conditional("DEBUG")]
            private void AssertUnreachable() {
                // assert that we never fall through

                cg.EmitInt(0);
                cg.EmitString("should be unreachable");
                cg.EmitCall(typeof(Debug), "Assert", new Type[] { typeof(bool), typeof(string) });
            }

            [Conditional("RO_DEBUG")]
            private void EmitCodeMarker(string marker) {
                cg.EmitString("---------- " + marker + " ----------");
                cg.Emit(OpCodes.Pop);
            }

            [Conditional("RO_DEBUG")]
            private void EmitCodeLogging(string logMsg) {
                cg.EmitString(logMsg);
                cg.EmitCall(typeof(Console), "WriteLine", new Type[] { typeof(string) });
            }

            private void MarkThisParameter(int param) {
                paramState[param].curTypeIndex++;

                if (paramState[param].LabelAssigned) {
                    cg.MarkLabel(paramState[param].nextArgLabel);
                }
                paramState[param].nextArgLabel = cg.DefineLabel();
            }

            private Label CreateOkConversionLabel(int param) {
                int curIndex = paramState[param].curTypeIndex - 1;
                return paramState[param].okConversions[curIndex] = cg.DefineLabel();                
            }
            /// <summary>
            /// Emits code for a successful identity conversion. We either dispatch
            /// the final method call (in a chain of identity conversions) or we
            /// proceed to checking the next parameter.
            /// </summary>
            private Label EmitCallOrNextParam(int param, IList<MethodTracker> methods) {
                // mark this label for back-branches in the non-identity conversions cases.
                Label nextParam = cg.DefineLabel();

                // we have an identity conversion
                if (param == (argCnt - 1)) {
                    // last argument, we should only have 1 method here.
                    Debug.Assert(methods.Count == 1);
                    EmitFinalCall(methods[0]);
                } else {
                    // we have more arguments...
                    cg.Emit(OpCodes.Br, nextParam);
                }
                return nextParam;
            }

            /// <summary>
            /// Checks to see if we have an identity conversion
            /// </summary>
            private Label EmitIdentityCheck() {
                cg.EmitInt((int)Conversion.Identity);
                outConv.EmitGet(cg);
                Label notIdentity = cg.DefineLabel();
                cg.Emit(OpCodes.Bne_Un, notIdentity);
                return notIdentity;
            }

            /// <summary>
            /// Checks to see if we have no conversion, branches to 
            /// the next argument if we don't have one
            /// </summary>
            private void EmitNoConversionContinue(int param) {
                // in the single argument case we'll do a check to
                // see if this is the best conversion, no need to do this check.
                if (argCnt != 1) {
                    cg.EmitInt((int)Conversion.None);
                    outConv.EmitGet(cg);
                    cg.Emit(OpCodes.Beq, paramState[param].nextArgLabel);
                }

            }

            /// <summary>
            /// Emits a call to the target method.
            /// </summary>
            private void EmitFinalCall(MethodTracker method) {
                EmitCodeMarker("EmitFinalCall Start");
                ParameterInfo[] pis = method.GetParameters();
                int paramOffset = 0;

                EmitParamCountCheck(pis, method.GetInArgCount());

                if (!method.IsStatic) {
                    EmitParameterSelf(method);
                    paramOffset = 1;
                }

                EmitParameters(method, pis, paramOffset);

                EmitCallAndReturn(method);

                EmitCodeMarker("EmitFinalCall End");
            }

            private void EmitParamCountCheck(ParameterInfo[] pis, int inArgs) {
                if (isParams && pis.Length > 1) {
                    // make sure we have enough arguments...                    
                    cg.EmitInt(pis.Length - 1 - (pis.Length - inArgs));

                    cg.EmitArgGet(0);
                    cg.EmitCall(typeof(Array), "get_Length");

                    cg.Emit(OpCodes.Bgt, noConversions);
                    nonIdentityCount++;
                }
            }

            private void EmitParameters(MethodTracker method, ParameterInfo[] pis, int paramOffset) {
                int outParams = 0;
                for (int i = 0; i < pis.Length; i++) {
                    if (pis[i].IsOut && !pis[i].IsIn) outParams++;

                    if (pis[i].IsDefined(typeof(ParamArrayAttribute), false)) {
                        if (isParams) {
                            // params parameter being called from a method w/ params object[] signature
                            EmitParamsArguments(pis, paramOffset+i, outParams, pis[i].ParameterType);
                        } else {
                            // params parameter being called from a non-params method, remaining
                            // arguments need to be put into an array for the parameter
                            EmitParamsArgumentsFromArguments(pis, paramOffset, i, outParams);
                        }
                        break;
                    }

                    // non-params parameter
                    EmitParameter(method, pis[i], i + paramOffset, i + paramOffset-outParams);
                }
            }

            /// <summary>
            /// Emits the call to the method or constructor and performs
            /// the necessary transformations on the return type such
            /// as boxing or creating a tuple w/ out or by-ref params.
            /// </summary>
            private void EmitCallAndReturn(MethodTracker method) {
                Type returnType;
                MethodInfo mi = method.Method as MethodInfo;

                if (mi != null) {
                    cg.EmitCall(mi);
                    returnType = mi.ReturnType;
                } else {
                    cg.EmitNew((ConstructorInfo)method.Method);
                    returnType = ((ConstructorInfo)method.Method).DeclaringType;
                }

                // check for out params...
                bool fIsByRef = false;
                ParameterInfo []pis = method.GetParameters();
                for (int i = 0; i < pis.Length; i++) {
                    if (pis[i].ParameterType.IsByRef) {
                        fIsByRef = true;
                        break;
                    }
                }

                if (!fIsByRef) {
                    // no out or by-ref parameters, just cast the return type back
                    cg.EmitCastToObject(returnType);
                    cg.EmitCall(typeof(Ops), "ToPython", new Type[] { typeof(object) });
                } else {
                    EmitByRefReturn(method, returnType);
                }

                cg.EmitReturn();
            }

            struct RefParamInfo {
                public RefParamInfo(int index, Type type, bool isOut) {
                    Index = index;
                    Type = type;
                    IsOut = isOut;
                }

                public int Index;
                public Type Type;
                public bool IsOut;

            }
            /// <summary>
            /// For by-ref / out params we return a tuple that contains the argument
            /// values.  This function handles that emission.  This is called w/ the
            /// raw (unboxed) return value on the stack.
            /// </summary>
            private void EmitByRefReturn(MethodTracker method, Type returnType) {
                // first make a list of all the by-ref / out params...
                ParameterInfo[] pis = method.GetParameters();
                List<RefParamInfo> byRefTypes = new List<RefParamInfo>();
                for (int i = 0; i < pis.Length; i++) {
                    if (pis[i].ParameterType.IsByRef) {
                        int paramIndex = i;
                        if (!CompilerHelpers.IsStatic(method.Method)) paramIndex++;
                        byRefTypes.Add(new RefParamInfo(paramIndex, pis[i].ParameterType.GetElementType(), pis[i].IsOut && !pis[i].IsIn));
                    }
                }

                // then package them up as appropriate for the method.
                if (returnType != typeof(void)) {
                    // if we have a return type then we want to include that value as well
                    Slot retSlot = cg.GetLocalTmp(typeof(object));
                    cg.EmitCastToObject(returnType);
                    cg.EmitCall(typeof(Ops), "ToPython", new Type[] { typeof(object) });
                    retSlot.EmitSet(cg);

                    cg.EmitObjectArray(byRefTypes.Count+1, delegate(int index) {
                        if (index == 0) {
                            // return value
                            retSlot.EmitGet(cg);
                        } else {
                            EmitByRefParam(byRefTypes[index - 1]);
                        }
                    });
                    // finally make a tuple out of the array on the stack
                    cg.EmitCall(typeof(Tuple), "Make");
                } else if (byRefTypes.Count != 1) {
                    // if there's more than one param they all go in an array 
                    cg.EmitObjectArray(byRefTypes.Count, delegate(int index) {
                        EmitByRefParam(byRefTypes[index]);
                    });
                    // finally make a tuple out of the array on the stack
                    cg.EmitCall(typeof(Tuple), "Make");
                } else {
                    // void return type, w/ a single out param... it becomes our return type, not a tuple.
                    EmitByRefParam(byRefTypes[0]);
                }
                
            }

            private void EmitByRefParam(RefParamInfo rpi) {
                if (!rpi.IsOut && rpi.Type == typeof(object)) {
                    // value that we used our arg slot for...
                    EmitArgument(rpi.Index);
                } else {
                    // value that we created a slot for.
                    GetOrCreateOutSlot(rpi.Index, rpi.Type).EmitGet(cg);
                    cg.EmitCastToObject(rpi.Type);
                }
                cg.EmitCall(typeof(Ops), "ToPython", new Type[] { typeof(object) });
            }

            /// <summary>
            /// Emits the this/self parameter for the specified method, handling
            /// various cast/conversion issues.
            /// </summary>
            private void EmitParameterSelf(MethodTracker method) {
                EmitCodeMarker("Param: Self");
                if (method.DeclaringType != typeof(object)) {
                    if (method.DeclaringType.IsValueType) {
                        paramSlots[0].ConversionValue.EmitGet(cg);
                        cg.Emit(OpCodes.Unbox, method.DeclaringType);
                    } else {
                        paramSlots[0].ConversionValue.EmitGet(cg);
                        if (method.DeclaringType.IsValueType) {
                            cg.Emit(OpCodes.Unbox_Any, method.DeclaringType);
                        } else {
                            cg.Emit(OpCodes.Castclass, method.DeclaringType);
                        }
                    }
                } else
                    EmitArgument(0);
            }

            private void EmitParameter(MethodTracker method, ParameterInfo pi, int param, int inParam) {
                EmitCodeMarker("Param: " + param.ToString());

                if (inParam >= argCnt && !(pi.IsOut && !pi.IsIn)) {
                    EmitDefaultValue(param, method);
                } else if (pi.ParameterType == typeof(object)) {
                    // for object we just directly load the argument (and it's
                    // never saved)                    
                    if (!pi.ParameterType.IsByRef) {
                        // normal argument
                        EmitArgument(inParam);
                    } else {
                        // by ref argument, we can use the arg slots which is object.
                        if (pi.IsOut && !pi.IsIn) {
                            Slot outSlot = GetOrCreateOutSlot(param, pi.ParameterType.GetElementType());
                            outSlot.EmitGetAddr(cg);
                        } else {
                            EmitArgumentAddress(inParam);
                        }
                    }
                } else {
                    // we have a conversion, we load the value and 
                    // cast it to the correct value.  If it's by-ref
                    // we need to use a temporary strongly typed slot.
                    if (pi.IsIn || !pi.IsOut) {
                        Type paramType = pi.ParameterType;
                        if (paramType.IsByRef) paramType = paramType.GetElementType();

                        paramSlots[inParam].ConversionValue.EmitGet(cg);

                        if (paramType.IsValueType) {
                            cg.Emit(OpCodes.Unbox_Any, paramType);
                        } else {
                            cg.Emit(OpCodes.Castclass, paramType);                        
                        }
                    }

                    if (pi.ParameterType.IsByRef) {
                        // by ref argument
                        Slot outSlot = GetOrCreateOutSlot(param, pi.ParameterType.GetElementType());

                        if (pi.IsOut && !pi.IsIn) {
                            outSlot.EmitGetAddr(cg);
                        } else {
                            outSlot.EmitSet(cg);
                            outSlot.EmitGetAddr(cg);
                        }
                    }
                }
            }

            private void EmitParamsArgumentsFromArguments(ParameterInfo[] pis, int paramOffset, int paramIndex, int outParams) {
                Label done = cg.DefineLabel();
                Type arrType = pis[paramIndex].ParameterType.GetElementType();

                if (paramOffset + paramIndex == pis.Length) {
                    Label emitConv = cg.DefineLabel();
                    Label checkArray = cg.DefineLabel();
                    EmitArgument(paramOffset + paramIndex - outParams);
                    cg.Emit(OpCodes.Ldnull);
                    cg.Emit(OpCodes.Bne_Un, checkArray);

                    cg.EmitInt(0);
                    cg.Emit(OpCodes.Newarr, arrType);

                    cg.Emit(OpCodes.Br, done);

                    cg.MarkLabel(checkArray);
                    // check and see if we're just passing an array in...
                    EmitArgument(paramOffset + paramIndex - outParams);
                    cg.EmitCall(typeof(object), "GetType");
                    cg.EmitType(pis[paramIndex].ParameterType);
                    cg.Emit(OpCodes.Bne_Un, emitConv);

                    EmitArgument(paramOffset + paramIndex - outParams);
                    cg.Emit(OpCodes.Br, done);

                    cg.MarkLabel(emitConv);
                }

                cg.EmitInt(argCnt - paramOffset);
                cg.Emit(OpCodes.Newarr, arrType);
                for (int i = paramOffset+paramIndex-outParams; i < argCnt; i++) {
                    cg.Emit(OpCodes.Dup);
                    cg.EmitInt(i-paramOffset);
                    EmitArgument(paramOffset);
                    cg.EmitCastFromObject(arrType);
                    cg.EmitStoreElement(arrType);
                }
                
                cg.MarkLabel(done);
            }

            private void EmitParamsArguments(ParameterInfo[] pis, int paramOffset, int outParams, Type paramType) {
                EmitCodeMarker("Param (array): " + paramOffset.ToString() );

                int paramArrayIndex = paramOffset - outParams;

                // if we only have one parameter, and it is null, then the user
                // is indicating they want to pass in no extra parameters.  Do that check first.
                Slot tmpArr = cg.GetLocalTmp(paramType);

                cg.EmitArgGet(0);
                cg.EmitCall(typeof(Array), "get_Length");
                if (paramArrayIndex != 0) {
                    // subtract off all the arguments we've used
                    cg.EmitInt(paramArrayIndex);
                    cg.Emit(OpCodes.Sub);
                }
                Label notSingleNullArg = cg.DefineLabel();
                Label singleNullArg = cg.DefineLabel();
                cg.EmitInt(1);
                cg.Emit(OpCodes.Bne_Un, notSingleNullArg);

                // only 1 effective argument...
                cg.EmitArgGet(0);
                cg.EmitInt(paramOffset);
                cg.Emit(OpCodes.Ldelem_Ref);

                cg.Emit(OpCodes.Ldnull);
                cg.Emit(OpCodes.Bne_Un, notSingleNullArg);  

                // we have a single null argument...
                cg.Emit(OpCodes.Ldnull);
                cg.Emit(OpCodes.Br, singleNullArg);
                cg.FreeLocalTmp(tmpArr);

                cg.MarkLabel(notSingleNullArg);
                if (paramType == typeof(object)) {
                    if (paramArrayIndex == 0) {
                        // great, we can just pass the object array
                        // straight on through!  Woo-hoo!
                        cg.EmitArgGet(0);
                    } else {
                        // good, we can just do a simple Array.Copy!

                        //Array.Copy(SourceFilter, sourceIndex, dest, destIndex, length);
                        cg.EmitArgGet(0);   // get the array for the source for copy

                        cg.EmitInt(paramArrayIndex);    // sourceIndex

                        cg.EmitArgGet(0);
                        cg.EmitCall(typeof(Array), "get_Length");
                        cg.EmitInt(paramArrayIndex);
                        cg.Emit(OpCodes.Sub);
                        cg.Emit(OpCodes.Newarr, paramType.GetElementType());    // destination
                        cg.Emit(OpCodes.Dup);
                        tmpArr = cg.GetLocalTmp(paramType);
                        tmpArr.EmitSet(cg);


                        cg.EmitInt(0);  // destIndex

                        cg.EmitArgGet(0);   // length
                        cg.EmitCall(typeof(Array), "get_Length");
                        cg.EmitInt(paramArrayIndex);
                        cg.Emit(OpCodes.Sub);

                        cg.EmitCall(typeof(Array), "Copy", new Type[]{
                            typeof(Array), 
                            typeof(int), 
                            typeof(Array), 
                            typeof(int), 
                            typeof(int)});

                        tmpArr.EmitGet(cg);

                        cg.FreeLocalTmp(tmpArr);
                    }
                } else {
                    // painful, we need to emit type cases...

                    // step 1 allocate the array

                    tmpArr = cg.GetLocalTmp(paramType);
                    cg.EmitArgGet(0);
                    cg.EmitCall(typeof(Array), "get_Length");
                    if (paramArrayIndex != 0) {
                        cg.EmitInt(paramArrayIndex);
                        cg.Emit(OpCodes.Sub);
                    }
                    cg.Emit(OpCodes.Newarr, paramType.GetElementType());    // destination
                    tmpArr.EmitSet(cg);

                    // loop through the items and store them into the array.
                    Label done = cg.DefineLabel();
                    EmitLoop(
                        // init
                        delegate(Slot curIndex) {
                            cg.EmitInt(paramArrayIndex);
                            curIndex.EmitSet(cg);
                        },

                        // conditional
                        delegate(Slot curIndex) {
                            curIndex.EmitGet(cg);
                            cg.EmitArgGet(0);
                            cg.EmitCall(typeof(Array), "get_Length");
                            cg.Emit(OpCodes.Beq, done);
                        },

                        // increment
                        delegate(Slot curIndex) {
                            cg.EmitInt(1);
                            curIndex.EmitGet(cg);
                            cg.Emit(OpCodes.Add);
                            curIndex.EmitSet(cg);
                        },

                        // body
                        delegate(Slot curIndex, Label contLabel) {
                            // load the array onto the stack for the stelem
                            tmpArr.EmitGet(cg);

                            // push the array index onto the stack
                            curIndex.EmitGet(cg);
                            if (paramArrayIndex != 0) {
                                cg.EmitInt(paramArrayIndex);
                                cg.Emit(OpCodes.Sub);
                            }

                            // load the value to be stored onto the stack
                            cg.EmitArgGet(0);
                            curIndex.EmitGet(cg);
                            cg.Emit(OpCodes.Ldelem_Ref);

                            // convert that into the appropriate element type
                            cg.EmitCastFromObject(paramType.GetElementType());

                            // and store it back into the new array
                            cg.EmitStoreElement(paramType.GetElementType());
                        });
                    cg.MarkLabel(done);

                    tmpArr.EmitGet(cg);
                    cg.FreeLocalTmp(tmpArr);
                }


                cg.MarkLabel(singleNullArg);
            }

            private void EmitDefaultValue(int param, MethodTracker method) {
                ParameterInfo[] pis = method.GetParameters();
                if (!method.IsStatic) param--;  // ignore this ptr for defaults.

                Debug.Assert(pis[param].DefaultValue != DBNull.Value, "Expected a param w/ a default value");

                cg.EmitRawConstant(pis[param].DefaultValue);
            }

            /// <summary>
            /// Emits a type check for the specified value & type. The result
            /// is on the stack (as an object) and the conversion is written
            /// to outConv.
            /// </summary>
            private void EmitTypeCheck(CodeGen cg, Slot value, Slot outConv, ParamTreeNode node) {
                Type paramType = node.ParamType;

                // Emits the right type check for the given type.  That can be either a call to TryConvertTo*
                // or an inline isinst if TryConvertTo* doesn't know about the conversion.  If we have a set
                // of overloads that mix static & non-statics then we need to handle the InstanceArgument and
                // possibly the real declaring type here as well.  
                Label done = cg.DefineLabel();
                value.EmitGet(cg);
                if (paramType == typeof(InstanceArgument)) {
                    paramType = EmitInstanceTypeCheck(cg, value, outConv, node, paramType, done);
                }

                if (paramType != typeof(InstanceArgument) || 
                    (node.Flags & ParamTree.NodeFlags.MixedInstanceStatic) == 0) {
                    EmitTypeCheckWorker(cg, outConv, node, paramType);
                }

                cg.MarkLabel(done);
            }

            private static void EmitTypeCheckWorker(CodeGen cg, Slot outConv, ParamTreeNode node, Type paramType) {
                if ((node.Flags & ParamTree.NodeFlags.ByRef) != 0) paramType = paramType.GetElementType();

                if (Converter.HasConversion(paramType)) cg.EmitTryCastFromObject(paramType, outConv);
                else EmitInlineTypeCheck(paramType, cg, outConv, null);
            }
            
            private Type EmitInstanceTypeCheck(CodeGen cg, Slot value, Slot outConv, ParamTreeNode node, Type paramType, Label done) {
                Type instType = allMeths[0].DeclaringType;
                ReflectedType type;
                if (OpsReflectedType.OpsTypeToType.TryGetValue(allMeths[0].DeclaringType, out type)) {
                    instType = type.type;
                }

                Label notInst = cg.DefineLabel();
                cg.Emit(OpCodes.Isinst, typeof(InstanceArgument));
                cg.Emit(OpCodes.Dup);
                cg.Emit(OpCodes.Brfalse, notInst);
                cg.EmitFieldGet(typeof(InstanceArgument), "ArgumentValue");
                
                // we still need to emit a conversion because the Instance type
                // and user supplied type may not line up (e.g. ops arguments
                // for floats).
                
                EmitTypeCheckWorker(cg, outConv, node, instType);

                cg.Emit(OpCodes.Br, done);

                if ((node.Flags & ParamTree.NodeFlags.MixedInstanceStatic) != 0) {
                    // not an InstanceArgument, and we'll not check for
                    // the real type so set the failure now.
                    cg.MarkLabel(notInst);

                    cg.EmitInt((int)Conversion.None);
                    outConv.EmitSet(cg);
                } else {
                    // not an InstanceArgument, check for real type next.
                    cg.MarkLabel(notInst);
                    paramType = instType;
                    cg.Emit(OpCodes.Pop);
                    value.EmitGet(cg);
                }

                return paramType;
            }

            delegate void InlineGetValue();
            private static void EmitInlineTypeCheck(Type t, CodeGen cg, Slot outConv, InlineGetValue getValue) {
                Label notInst = cg.DefineLabel();
                Label done = cg.DefineLabel();
                Label skipCheck = cg.DefineLabel();
                if (!t.IsValueType || 
                    (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                    cg.Emit(OpCodes.Dup);
                    cg.Emit(OpCodes.Ldnull);
                    cg.Emit(OpCodes.Beq, skipCheck);    // null value on stack from dup as value.   
                    // value left on stack for isinst
                } 

                cg.Emit(OpCodes.Isinst, t);
                cg.Emit(OpCodes.Dup);
                cg.Emit(OpCodes.Brfalse, notInst);

                // instance parameter
                cg.MarkLabel(skipCheck);
                cg.EmitInt((int)Conversion.Identity);
                outConv.EmitSet(cg);

                // leave value on the stack
                if (getValue != null) getValue();
                cg.Emit(OpCodes.Br, done);

                // not an instance parameter...
                cg.MarkLabel(notInst);
                cg.EmitInt((int)Conversion.None);
                outConv.EmitSet(cg);

                cg.MarkLabel(done);
            }


            #endregion

            #region Private data structures
            /// <summary>
            /// per-param state. This gets cleared in PreArgument and used
            /// by PostArgument. Because while we're processing each argument
            /// we will also process sub arguments we keep an array of this state.
            /// </summary>
            private class PerParamState {
                public PerParamState(int typeCount) {
                    TypeCount = typeCount;
                    okConversions = new Label[typeCount];
                }

                public Label[] okConversions;
                public Label nextArgLabel {
                    get {
                        Debug.Assert(LabelAssigned, "accessing label before it is assigned");
                        return myLabel;
                    }
                    set {
                        myLabel = value;
                        LabelAssigned = true;
                    }
                }

                public int TypeCount;
                public int curTypeIndex;
                public Type lastType;
                public bool LabelAssigned;

                private Label myLabel;
            }

            /// <summary>
            /// param slots. This is our virtual "stack space" for
            /// the "TryParam" method described above. We have one
            /// per argument. The values aren't guaranteed initialization
            /// unless initialized in PreArgument - instead we share these
            /// values between all the logical stack frames.
            /// 
            /// Because there's one of these per an argument keeping the
            /// number of slots here small is a good idea. If adding slots
            /// for a non-optimial code path a single array of the slot
            /// type is preferred over a per-argument Slot.
            /// </summary>
            private class PerParamSlots {
                Slot convCount, convValue, retrys, loneCandidate, singleParamConv;
                CodeGen cg;
                public PerParamSlots(CodeGen codeGen) {
                    cg = codeGen;
                }

                /// <summary>
                /// the result of the last converted value.
                /// </summary>
                public Slot ConversionValue {
                    get {
                        if (convValue == null) convValue = cg.GetLocalTmp(typeof(object));

                        return convValue;
                    }
                }

                /// <summary>
                /// the count of conversions, if only 1 or zero we can quickly dispatch
                /// </summary>
                public Slot ConvertedCount {
                    get {
                        if (convCount == null) convCount = cg.GetLocalTmp(typeof(int));

                        Debug.Assert(singleParamConv == null);

                        return convCount;
                    }
                }

                /// <summary>
                /// integer value of the best candidate used for fast switch dispatch
                /// </summary>
                public Slot SingleBestCandidate {
                    get {
                        if (loneCandidate == null) loneCandidate = cg.GetLocalTmp(typeof(int));

                        return loneCandidate;
                    }
                }

                /// <summary>
                /// the list of retry candidates.
                /// </summary>
                public Slot Retrys {
                    get {
                        if (retrys == null) retrys = cg.GetLocalTmp(typeof(List<object[]>));

                        return retrys;
                    }
                }

                public Slot SingleParamConversion {
                    get {
                        if (singleParamConv == null) singleParamConv = cg.GetLocalTmp(typeof(int));

                        // we should only use SingleParamConversion or conversion count, not both.
                        Debug.Assert(convCount == null);

                        return singleParamConv;
                    }
                }
            }

            delegate void CodeLoop(Slot indexSlot);
            delegate void CodeContinuable(Slot indexSlot, Label loopDone);

            #endregion

        }
        #endregion
    }
}