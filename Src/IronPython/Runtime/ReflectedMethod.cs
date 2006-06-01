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
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using System.Diagnostics;
using IronPython.Compiler;
using IronPython.Modules;
using IronMath;

namespace IronPython.Runtime {
    /// <summary>
    /// ReflectedMethod's are unoptimized BuiltinFunction's.  They use late-bound
    /// invocation to call their target.
    /// 
    /// ReflectedMethod's also currently support certain functionality optimized
    /// methods don't. This includes:
    ///    
    /// context-aware calls
    /// generic-type binding
    /// TryCall
    /// 
    /// </summary>
    public partial class ReflectedMethodBase : BuiltinFunction, ITryCallable, ICallableWithCallerContext {
        protected BuiltinFunction optimizedTarget;
        ParamTree[] paramTrees = null;

        #region Constructors

        protected ReflectedMethodBase() { }
        
        public ReflectedMethodBase(string name, MethodBase target)
            : base(name, new MethodBase[] { target }, FunctionType.PythonVisible) {
            UpdateFunctionInfo(target);
        }

        public ReflectedMethodBase(string name, MethodBase target, FunctionType functionType)
            : base(name, new MethodBase[] { target }, functionType) {
            UpdateFunctionInfo(target);
        }

        public ReflectedMethodBase(string name, MethodBase[] targets)
            : base(name, targets, FunctionType.None) {
            for (int i = 0; i < targets.Length; i++) {
                UpdateFunctionInfo(targets[i]);
            }
        }

        #endregion

        #region Public APIs
        public void AddMethod(MethodBase info) {
            if (targets != null) {
                MethodBase[] ni = new MethodBase[targets.Length + 1];
                targets.CopyTo(ni, 0);
                ni[targets.Length] = info;
                targets = ni;
            } else {
                targets = new MethodBase[] { info };
            }
            UpdateFunctionInfo(info);
        }

        public virtual bool TryCall(object[] args, out object ret) {
            return TryCallWorker(args, out ret);
        }
        
        #endregion

        #region Protected APIs

        protected virtual void OptimizeMethod() {
            BuiltinFunction optimized = ReflectOptimizer.MakeFunction(this);
            if (optimized != null) {
                Debug.Assert(optimized.Targets.Length == Targets.Length);

                ReflectedType declType = (ReflectedType)DeclaringType;
                declType.Initialize();

                object prevVal;
                SymbolId myId = SymbolTable.StringToId(Name);
                if (declType.dict.TryGetValue(myId, out prevVal)) {
                    ClassMethod cm;
                    if ((cm = prevVal as ClassMethod) != null) {
                        cm.func = optimized;
                    } else {
                        if (myId == SymbolTable.NewInst) declType.dict[myId] = declType.ctor = optimized;
                        else declType.dict[myId] = optimized.GetDescriptor();
                    }

                    optimizedTarget = optimized;
                }
            }
        }

        #endregion

        #region Callables

        [PythonName("__call__")]
        public override object Call(params object[] args) {
            object ret;
            if (TryCallWorker(args, out ret)) {
                return ret;
            }

            throw BadArgumentError(args.Length);
        }


        [PythonName("__call__")]
        public override object Call(ICallerContext context, params object[] args) {
            if (IsContextAware) {
                object[] allArgs = new object[args.Length + 1];
                allArgs[0] = context;
                for (int i = 0; i < args.Length; i++) allArgs[i + 1] = args[i];
                return Call(allArgs);
            } else {
                return Call(args);
            }
        }

        #endregion

        #region Private implementation details
        
        private static object TryConvert(object value, Type to, out Conversion conversion) {
            if (to.IsByRef) {
                to = to.GetElementType();
            }
            if (to == typeof(object)) { //!!! avoid un-wrapping COM objects (ReflectOptimizer does the same - need to get rid of wrapping entirely!)
                conversion = Conversion.Identity;
                return value;
            }

            return Converter.TryConvert(value, to, out conversion);
        }



        private bool TryOptimizedCall(object[] args, out object ret) {
            // create the target if it doesn't already exist.  We cache the 
            // target incase the user stores the reflected method somewhere
            // and continues to call the unoptimized version.
            if (optimizedTarget == null) {
                OptimizeMethod();
            }

            if (optimizedTarget != null) {
                BuiltinFunction optimized = optimizedTarget;
                if (HasInstance) {
                    optimized = optimized.Clone();
                    optimized.inst = inst;
                }

                ret = optimized.Call(args);
                return true;
            }

            ret = null;
            return false;
        }

        class DispatchWalker {
            private object[] args;
            private ParamTree pt;
            private MethodBinding result;
            private object inst;
            private int curArg;
 
            public DispatchWalker(ParamTree paramTree, object instance, object[] arguments) {
                args = arguments;
                pt = paramTree;
                inst = instance;
            }
            
            /// <summary>
            /// Attempts to bind to a method in the param tree.   Same algorithm as the ReflectOptimizer generates
            /// in-line.  We walk the parameter tree (which is losely ordered) checking each parameter.  If a conversion
            /// is an identity conversion nothing can be it and the dispatch immediately continues to the next parameter.  Otherwise
            /// we collect all of the bad-conversions and attempt to dispatch to them in order of quality.
            /// 
            /// While this is using the same logic for performing dispatch the implementation is actually slightly different.
            /// The reflect optimizer holds onto a single converted value at a time within locals.  Here we maintain the converted
            /// values on the stack, and once we've found the ideal conversion we start filling them in as we return control up
            /// to our callers w/ the last argument first. When we get out to the top we've filled in all of the arguments including
            /// the instance.  This greatly simplifies dealing w/ various combinations of instances / no-instances and bound and 
            /// unbound methods.
            /// </summary>
            public bool TryGetBinding(out MethodBinding mb) {
                if (pt.ArgumentCount == 0) {
                    // zero-argument case: walking the tree does no good, because
                    // the tree is empty.  
                    Debug.Assert(inst == null && args.Length == 0);
                    Debug.Assert(pt.Methods.Count == 1);

                    if (GetFinalCall(0, 0, pt.Methods[0])) {
                        mb = result;
                        return true;
                    }
                } else {
                    int startDepth = 0;
                    if (inst != null) startDepth = -1;

                    if (WalkWorker(startDepth, 0, pt)) {
                        mb = result;
                        return true;
                    }
                }

                // no binding available
                mb = new MethodBinding();
                return false;
            }

            private object GetArgument(int arg){
                if (arg < 0) return inst;
                return args[arg];
            }

            /// <summary>
            /// Walks an argument in the tree, recursing to further arguments to perform the dispatch
            /// as necessary.  the walk initially starts at either 0 (no instance) or -1 (instance
            /// available).
            /// </summary>
            private bool WalkWorker(int param, int outArgs, ParamTreeNode root) {
                object val;
                List<RetryInfo> retryList = null;

                if (param-outArgs >= args.Length) return GetBoundToDefaults(param, outArgs, root);
                if (root.Children.Count == 0) return GetBoundToParams(param, outArgs, root);
                
                foreach (ParamTreeNode node in root.Children) {
                    if (node.ParamType == null) continue;   // method w/ less arguments then we're providing...

                    if ((node.Flags & ParamTree.NodeFlags.Out) != 0) {
                        if (WalkWorker(param+1, outArgs+1, node)) {
                            return true;
                        }
                        continue;
                    }

                    Conversion conv;
                    val = TryConvert(GetArgument(param-outArgs), node.ParamType, out conv);

                    if (conv == Conversion.Identity) {
                        // nothing can beat us, dispatch now...
                        if (GetFinalCallOrNextArg(param, outArgs, node, val)) return true;

                        // this method fails dispatch on later argument, don't put it 
                        // in the retry list.
                        continue; 
                    }

                    if (conv == Conversion.None) {
                        if (node.ParamType != typeof(InstanceArgument) || GetArgument(param) == null) {
                            continue;
                        }

                        // mixed method/function dispatch, we're calling an unbound method w/ 
                        // the 1st parameter matching the declaring type.  We favor calling the
                        // static method so this will get an Implicit conversion level.

                        val = TryConvert(GetArgument(param), node.Methods[0].DeclaringType, out conv);
                        if (conv == Conversion.Identity) conv = Conversion.Implicit;
                    }

                    retryList = UpdateRetryList(val, retryList, node, conv);
                }


                if (retryList != null && HandleNonIdentityConversion(param, outArgs, retryList)) {
                    return true;
                }

                return GetBoundToParams(param, outArgs, root);            
            }

            /// <summary>
            /// After checking all the user-provided parameters attempt to bind to the 
            /// method.  This is equivalent to ReflectOptimizer's EmitFinalCall.
            /// </summary>
            private bool GetFinalCall(int param, int outArgs, MethodTracker method) {
                result = new MethodBinding();
                result.method = method.Method;
                result.tracker = method;
                result.arguments = new object[method.SigLength];

                // we store the arguments backwards into the arguments array as we
                // return up the stack.  User supplied arguments start at the 
                // maximum argument the user provided.  If they provided an
                // instance to an unbound method then we start writing one earlier.
                curArg = args.Length - 1;
                if (inst == null && !method.IsStatic) curArg--;

                int sigLen = method.GetInArgCount();
                if (param - outArgs < sigLen) {
                    // we don't have enough arguments to bind to this method, but
                    // we could still have default values left over, or a params
                    // method for which we're not providing any parameters.
                    ParameterInfo[] pis = method.GetParameters();

                    for (int i = pis.Length - 1; i >= curArg + 1; i--) {
                        if (method.IsParamsMethod && i == pis.Length - 1) {
                            // bind an empty param array
                            result.arguments[i] = CreateParamsArray(0, pis);
                        } else if (pis[i].DefaultValue != DBNull.Value) {
                            // bind the default value
                            result.arguments[i] = pis[i].DefaultValue;
                        } else if (!pis[i].IsIn && pis[i].IsOut) {
                            // out only parameter, it's ok, leave as null
                            continue;
                        } else if(outArgs != 0 && (curArg+outArgs)  == i) {
                            // out-args preceding in-arg...
                            curArg += outArgs;
                            break;
                        } else {
                            // not enough defaults, can't bind
                            return false;
                        }
                    }
                }
                return true;
            }

            /// <summary>
            /// Creates the retry list (if necessary) and adds a conversion to the list.  This
            /// is equivalent to ReflectOptimizer's EmitUpdateRetryList.
            /// </summary>
            private static List<RetryInfo> UpdateRetryList(object val, List<RetryInfo> retryList, ParamTreeNode node, Conversion conv) {
                if (retryList == null) retryList = new List<RetryInfo>();

                // Update the retry list...
                bool fFound = false;
                for (int i = 0; i < retryList.Count; i++) {
                    if (retryList[i].Conversion > conv) {
                        retryList.Insert(i, new RetryInfo(conv, node, val));
                        fFound = true;
                        break;
                    }
                }

                if (!fFound) retryList.Add(new RetryInfo(conv, node, val));
                return retryList;
            }

            /// <summary>
            /// Attempts to try calls on non-identity conversions.  This is equivalent to
            /// ReflectOptimizer's EmitNonIdentityConversion
            /// </summary>
            /// <param name="param"></param>
            /// <param name="retryList"></param>
            /// <returns></returns>
            private bool HandleNonIdentityConversion(int param, int outArgs, List<RetryInfo> retryList) {
                // retry list is sorted in order of best conversions, try one after another.
                for (int i = 0; i < retryList.Count; i++) {
                    if (GetFinalCallOrNextArg(param, outArgs, retryList[i].Node, retryList[i].ConvertedValue))
                        return true;
                }
                return false;
            }

            /// <summary>
            /// Attempts to bind the remaining (missing) parameters from default values.  Default values
            /// also include a virtual-default as an empty params array.
            /// </summary>
            private bool GetBoundToDefaults(int param, int outArgs, ParamTreeNode root) {
                for(int i = 0; i<root.Methods.Count; i++) {
                    int curParam = param;
                    int curOut = outArgs;

                    // skip any remaining out params...
                    ParameterInfo[] pis = root.Methods[i].GetParameters();
                    while (curParam < root.Methods[i].SigLength && (pis[curParam].IsOut && !pis[curParam].IsIn)) {
                        curOut++;
                        curParam++;
                    }

                    if (curParam >= root.Methods[i].SigLength-1 && root.Methods[i].IsParamsMethod) {
                        // rest of the values go to the params method...
                        // no arguments for parameter array, we default to an empty one
                        if (GetFinalCall(curParam, curOut, root.Methods[i])) {
                            return true;
                        }
                    } else if (curParam < root.Methods[i].GetParameters().Length && 
                        (pt.ArgumentCount - root.Methods[i].DefaultCount) == curParam) {
                        // rest of the values are defaults...
                        if (GetFinalCallOrNextArg(curParam, curOut, root, root.Methods[i].GetParameters()[curParam].DefaultValue))
                            return true;
                    }
                }

                // no binding available.
                return false;
            }

            /// <summary>
            /// Attempts to bind the remaining (extra) parameters into a params array.  
            /// </summary>
            private bool GetBoundToParams(int param, int outArgs, ParamTreeNode root) {
                for (int i = 0; i < root.Methods.Count; i++) {
                    if (!root.Methods[i].IsParamsMethod) continue;
                    
                    ParameterInfo[] pis = root.Methods[i].GetParameters();
                    Conversion conv;

                    object val = TryConvert(GetArgument(param-outArgs), pis[pis.Length - 1].ParameterType.GetElementType(), out conv);
                    if (conv != Conversion.None) {
                        if (param-outArgs == args.Length - 1 && GetFinalCall(param, outArgs, root.Methods[i])) {
                            StoreArgument(val);
                            return true;
                        }

                        if (GetBoundToParams(param + 1, outArgs, root)) {
                            StoreArgument(val);
                            return true;
                        }
                    } else if (param-outArgs == args.Length - 1 && GetArgument(param) == null) {
                        // explicitly passing null to the params array, ok...
                        if (GetFinalCall(param, outArgs, root.Methods[i])) {
                            curArg--;
                            return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>
            /// Attempts to bind the next argument, if one is left, or attempst to bind to
            /// the final method.  This is equivalent to ReflectOptimizer's EmitCallOrNextParam
            /// </summary>
            private bool GetFinalCallOrNextArg(int param, int outArgs, ParamTreeNode node, object val) {
                Debug.Assert(node.Methods.Count != 0);

                if (param-outArgs == args.Length - 1 && GetFinalCall(param, outArgs, node.Methods[0])) {
                    StoreArgument(val);
                    return true;
                }
                
                if (WalkWorker(param + 1, outArgs, node)) {
                    StoreArgument(val);
                    return true;
                }
                return false;                
            }

            /// <summary>
            /// Stores an argument for the call.  This is equivalent to ReflectOptimizer's
            /// EmitParameters for a single parameter.  Note we have no equivalent to 
            /// EmitParamsArgumentsFromArguments vs EmitParamsArguments because our
            /// arguments always come from our args array
            /// </summary>
            private void StoreArgument(object val) {
                ParameterInfo[] pis = result.tracker.GetParameters();

                if (!result.tracker.IsParamsMethod || curArg < (result.tracker.GetInArgCount()-1)) {
                    StoreNormalArgument(val, pis);
                } else {
                    StoreParamsArgument(val, pis);
                }                
            }

            /// <summary>
            /// Stores either the this argument or another normal argument into
            /// the parameter array.  Equivalent to ReflectOptimizer's EmitParameter.
            /// </summary>
            private void StoreNormalArgument(object val, ParameterInfo[] pis) {
                // normal argument or instance argument
                while (curArg >= 0 && pis[curArg].IsOut && !pis[curArg].IsIn) {
                    // skip over out params...
                    curArg--;
                }

                if (curArg == -1) {
                    Debug.Assert(result.instance == null);
                    InstanceArgument ia = val as InstanceArgument;
                    if (ia != null) val = ia.ArgumentValue;

                    result.instance = val;
                } else {
                    Debug.Assert(result.arguments[curArg] == null);
                    result.arguments[curArg--] = val;
                }
            }

            /// <summary>
            /// Stores an argument into the params array for this call.  Equivalent
            /// to params EmitParamsArguments for a single argument.
            /// </summary>
            private void StoreParamsArgument(object val, ParameterInfo[] pis) {
                // params argument
                Debug.Assert(result.tracker.IsParamsMethod);

                int paramArrayIndex = result.tracker.SigLength - 1;
                int newArgIndex = curArg - (result.tracker.GetInArgCount()-1);

                if (result.arguments[paramArrayIndex] == null) {
                    // first param value to be stored, allocate the param array
                    if (val != null && newArgIndex == 0 && pis[paramArrayIndex].ParameterType == val.GetType()) {
                        // array stored into param array...
                        result.arguments[paramArrayIndex] = val;
                        curArg--;
                        return;
                    } else {
                        result.arguments[paramArrayIndex] = CreateParamsArray(newArgIndex + 1, pis);
                    }
                }

                Array a = (Array)result.arguments[paramArrayIndex];

                Debug.Assert(newArgIndex >= 0, "negative index " + newArgIndex.ToString());
                Debug.Assert(newArgIndex < a.Length, String.Format("index too large {0} vs {1}", newArgIndex, a.Length));

                a.SetValue(val, newArgIndex);
                curArg--;
            }

            /// <summary>
            /// Creates a params array of the specified size for the params type
            /// if the given ParamterInfo array.
            /// </summary>
            private object CreateParamsArray(int size, ParameterInfo[] pis) {
                Type t = pis[pis.Length - 1].ParameterType.GetElementType();

                if (t == typeof(object)) return new object[size];
                else return Array.CreateInstance(t, size);
            }

            struct RetryInfo {
                public RetryInfo(Conversion conv, ParamTreeNode node, object value) {
                    Conversion = conv;
                    Node = node;
                    ConvertedValue = value;
                }

                public Conversion Conversion;
                public ParamTreeNode Node;
                public object ConvertedValue;
            }
        }

        private bool TryCallWorker(object[] args, out object ret) {
            // check to see if we can lazily optimize this function,
            // and if optimize it and call the optimized version.
            if ((FunctionType & FunctionType.OptimizeChecked) == 0) {
                if (TryOptimizedCall(args, out ret)) {
                    return true;
                }

                FunctionType |= FunctionType.OptimizeChecked;

            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Methods, this);

            object instance;
            ParamTree pt = GetParamTreeAndInstance(args.Length, out instance);

            if (pt != null) {
                DispatchWalker dw = new DispatchWalker(pt, instance, args);
                MethodBinding mb;
                if (dw.TryGetBinding(out mb)) {
                    ret = Invoke(mb);
                    return true;
                }
            }
            ret = null;
            return false;
        }

        /// <summary>
        /// Gets the appropriate param tree and the instance we should use for this param tree.
        /// 
        /// We need to hand back the instance here as well because we could end up w/ a mixed-dispatch
        /// case where we have a static & instance methods colliding under the same name but w/ a different
        /// number of arguments.  In that case we're willing to drop the instance & bind to a static method, but
        /// only if there's no other override to try.
        /// </summary>
        private ParamTree GetParamTreeAndInstance(int argCnt, out object instance) {
            instance = HasInstance ? Instance: null;
            if (paramTrees == null) paramTrees = ParamTree.BuildAllTrees(MethodTracker.GetTrackerArray(Targets), FunctionType);

            ParamTree possibility = null;
            for (int i = 0; i < paramTrees.Length; i++) {
                if (paramTrees[i].ArgumentCount == argCnt) {
                    if (instance == null) return paramTrees[i];

                    if ((paramTrees[i].FunctionType & FunctionType.FunctionMethodMask) == (FunctionType.Method | FunctionType.Function)) {
                        // if we have no other matches, go w/ this one... (dropping the instance)
                        possibility = paramTrees[i];
                    }
                } else if (instance != null && paramTrees[i].ArgumentCount == argCnt + 1) {
                    return paramTrees[i];
                }
            }

            if (possibility != null) {
                instance = null;
                return possibility;
            }

            if ((paramTrees[paramTrees.Length - 1].FunctionType & FunctionType.Params) != 0)
                return paramTrees[paramTrees.Length - 1];

            return null;
        }
  
        #endregion
    }

    public class ReflectedConstructor : ReflectedMethodBase {
        public ReflectedConstructor(string name, ConstructorInfo info) : base(name, info) { }

        public override string ToString() {
            return string.Format("<constructor# for {0}>", targets[0].DeclaringType.FullName);
        }

        #region Protected overrides

        protected override void OptimizeMethod() {
            BuiltinFunction optimized = ReflectOptimizer.MakeFunction(this);
            if (optimized != null) {
                ReflectedType declType = (ReflectedType)DeclaringType;
                if (declType.dict == null) declType.dict = new Dict();

                object newmeth;
                NewMethod newMethAsMeth;
                if (declType.dict.TryGetValue(SymbolTable.NewInst, out newmeth) && (newMethAsMeth = newmeth as NewMethod) != null) {
                    newMethAsMeth.Optimize(optimized);
                } else {
                    declType.dict[SymbolTable.NewInst] = optimized;
                    declType.ctor = optimized;
                }

                optimizedTarget = optimized;
            }
        }

        protected override bool HasInstance {
            get {
                return false;
            }
        }

        protected override object Instance {
            get {
                return null;
            }
        }

        #endregion      

    }

    public class ReflectedMethod : ReflectedMethodBase, IMapping, IContextAwareMember {
        [PythonName("__new__")]
        public static object MakeNew(object cls, object callable, object instance){
            return Ops.GetDescriptor(callable, instance, null);
        }

        public ReflectedMethod() { }

        public ReflectedMethod(string name, MethodInfo info, NameType nt) :
            base(name, info, nt == NameType.PythonMethod ? FunctionType.PythonVisible | FunctionType.Method : FunctionType.Method) {
        }

        public ReflectedMethod(string name, MethodInfo info, FunctionType funcType)
            :
            base(name, info, funcType) {
        }

        protected ReflectedMethod(string name, MethodBase[] infos, object instance, FunctionType funcType)
            : base(name, infos) {
            base.inst = instance;
            FunctionType = funcType;
        }

        public override BuiltinFunction Clone() {
            return new ReflectedMethod(Name, targets, null, FunctionType);
        }
               

        #region IContextAwareMember Members

        bool IContextAwareMember.IsVisible(ICallerContext context) {
            return (context.ContextFlags & CallerContextFlags.ShowCls) != 0 ||
                IsPythonVisible;
        }

        #endregion
    }

    public class ReflectedUnboundMethod : ReflectedMethod {
        public ReflectedUnboundMethod() { }

        public ReflectedUnboundMethod(string name, MethodInfo info, NameType nt) : base(name, info, nt) { }

        public ReflectedUnboundMethod(string name, MethodInfo info, FunctionType funcType) : base(name, info, funcType) { }

        private ReflectedUnboundMethod(string name, MethodBase[] infos, object instance, FunctionType funcType) : base(name, infos, instance, funcType) { }

        //!!! performance improvements by handling IFastCallable
        public override bool TryCall(object[] args, out object ret) {
            if (Instance == null) return base.TryCall(args, out ret);

            object[] nargs = new object[args.Length + 1];
            nargs[0] = Instance;
            args.CopyTo(nargs, 1);

            return base.TryCall(nargs, out ret);
        }

        public override object Call(params object[] args) {
            if (Instance == null) return base.Call(args);

            object[] nargs = new object[args.Length + 1];
            nargs[0] = Instance;
            args.CopyTo(nargs, 1);

            return base.Call(nargs);
        }

        [PythonName("__call__")]
        public override object Call(ICallerContext context, params object[] args) {
            if (Instance == null) return base.Call(context, args);

            if (IsContextAware) {
                object[] allArgs = new object[args.Length + 2];
                allArgs[0] = context;
                allArgs[1] = Instance;
                for (int i = 0; i < args.Length; i++) allArgs[i + 2] = args[i];
                return base.Call(allArgs);
            } else {
                return Call(args);
            }
        }

        protected override bool HasInstance {
            get {
                return false;
            }
        }

        public override BuiltinFunction Clone() {
            return new ReflectedUnboundMethod(Name, targets, null, FunctionType);
        }
       
    }

    public class ReflectedUnboundReverseOp : ReflectedMethod {
        public ReflectedUnboundReverseOp() { }

        public ReflectedUnboundReverseOp(string name, MethodInfo info, NameType nt)
            : base(name, info, nt) {
        }

        private ReflectedUnboundReverseOp(string name, MethodBase[] infos, object instance, FunctionType funcType)
            : base(name, infos, instance, funcType) {
        }

        public override bool TryCall(object[] args, out object ret) {
            if (Instance == null) return base.TryCall(args, out ret);

            object[] nargs = new object[args.Length + 1];
            args.CopyTo(nargs, 0);
            nargs[args.Length] = Instance;

            return base.TryCall(nargs, out ret);
        }

        public override object Call(params object[] args) {
            if (Instance == null) {
                return base.Call(args);
            }
            object[] nargs = new object[args.Length + 1];
            args.CopyTo(nargs, 0);
            nargs[args.Length] = Instance;

            return base.Call(nargs);
        }

        protected override bool HasInstance {
            get {
                return false;
            }
        }

        public override BuiltinFunction Clone() {
            return new ReflectedUnboundReverseOp(Name, targets, null, FunctionType);
        }       

    }

    // Used to map signatures to specific targets on the embedded reflected method.
    public class BuiltinFunctionOverloadMapper {

        private BuiltinFunction function;

        public BuiltinFunctionOverloadMapper(BuiltinFunction builtinFunction) {
            this.function = builtinFunction;
        }

        public override string ToString() {
            Dict overloadList = new Dict();
            foreach (MethodBase mb in function.Targets) {
                string key = ReflectionUtil.CreateAutoDoc(mb);
                overloadList[key] = function;
            }
            return overloadList.ToString();
        }

        public object this[object key] {
            get {
                // Retrieve the signature from the index.
                Type[] sig;
                Tuple sigTuple = key as Tuple;

                if (sigTuple != null) {
                    sig = new Type[sigTuple.Count];
                    for (int i = 0; i < sig.Length; i++) {
                        sig[i] = Converter.ConvertToType(sigTuple[i]);
                    }
                } else {
                    sig = new Type[] { Converter.ConvertToType(key) };
                }

                // We can still end up with more than one target since generic and non-generic
                // methods can share the same name and signature. So we'll build up a new
                // reflected method with all the candidate targets. A caller can then index this
                // reflected method if necessary in order to provide generic type arguments and
                // fully disambiguate the target.
                ReflectedMethod rm = new ReflectedMethod();
                rm.Name = function.Name;
                rm.FunctionType = function.FunctionType|FunctionType.OptimizeChecked; // don't allow optimization that would whack the real entry
                rm.inst = function.inst;

                // Search for targets with the right number of arguments.
                int args = sig.Length;
                foreach (MethodBase mb in function.Targets) {
                    ParameterInfo[] pis = mb.GetParameters();
                    if (pis.Length != args)
                        continue;

                    // Check each parameter type for an exact match.
                    bool match = true;
                    for (int i = 0; i < args; i++)
                        if (pis[i].ParameterType != sig[i]) {
                            match = false;
                            break;
                        }
                    if (!match)
                        continue;

                    // Okay, we have a match, add it to the list.
                    rm.AddMethod(mb);
                }
                if (rm.Targets == null)
                    throw Ops.TypeError("No match found for the method signature {0}", key);

                return rm;
            }
            set {
                throw new NotImplementedException();
            }
        }
    }
}
