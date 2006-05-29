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
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Text;

using IronPython.Runtime;

namespace IronPython.Compiler {
    /// <summary>
    /// Represents a tree of parameters & their associated methods.
    /// Each branch down the tree represents the next parameter. There 
    /// is a branch / parameter type. 
    /// </summary>
    [DebuggerDisplay("ParamTree: {ToString()}")]
    class ParamTree : ParamTreeNode {
        #region Private member variables
        int argumentCount;
        FunctionType funcType;
        int[] extraReturnPositions;     // usually very small
#if DEBUG
        bool fFinished;
#endif
        #endregion

        #region Public API Surface
        public ParamTree(int argumentCount) : base(null, NodeFlags.None){
            this.argumentCount = argumentCount;
        }

        /// <summary>
        /// Generates all the parameter trees for a set of methods.  Trees are returned in an array sorted
        /// by number of parameters.
        /// </summary>
        public static ParamTree[] BuildAllTrees(MethodTracker[] methods, FunctionType funcType) {
            int maxArgCnt = 0;
            int minArgCnt = Int32.MaxValue;
            int maxParamArgCnt = 0;
            List<MethodTracker> paramsMethods = null;

            for (int i = 0; i < methods.Length; i++) {
                if (methods[i].Method.ContainsGenericParameters) continue;

                MethodBase curMethod = methods[i].Method;
                int paramLen = GetPythonExposedArgCount(methods[i]);

                if (methods[i].IsParamsMethod ) {
                    // params methods & methods that exceed our fast calalble count go together
                    if (paramsMethods == null) paramsMethods = new List<MethodTracker>();

                    if (paramLen > maxParamArgCnt) {
                        maxParamArgCnt = paramLen;
                    }

                    paramsMethods.Add(methods[i]);
                    continue;
                }

                if (paramLen > maxArgCnt) maxArgCnt = paramLen;

                int minParanLen = paramLen - methods[i].DefaultCount;
                if (minParanLen < minArgCnt) {
                    minArgCnt = minParanLen;
                }
            }

            List<ParamTree> targets = new List<ParamTree>();

            // then generate the methods for each effective # of args that we have

            for (int sigLen = minArgCnt; sigLen <= maxArgCnt; sigLen++) {
                List<MethodTracker> methodSet = new List<MethodTracker>(methods.Length);

                for (int i = 0; i < methods.Length; i++) {
                    if (methods[i].Method.ContainsGenericParameters) continue;

                    MethodBase curMethod = methods[i].Method;
                    int paramLen = GetPythonExposedArgCount(methods[i]);

                    // skip params methods, we deal with them last...
                    if (methods[i].IsParamsMethod) {
                        if (paramLen <= sigLen) {
                            methodSet.Add(methods[i]);
                        }
                    } else if (MatchesArgCount(methods[i], sigLen, paramLen)) {
                        methodSet.Add(methods[i]);
                    }
                }

                // we can have gaps (e.g. foo(x) and foo(x,y,z) won't have args for foo(x,y)).
                if (methodSet.Count != 0) {
                    targets.Add(Build(funcType, methodSet, sigLen));
                }
            }

            if (paramsMethods != null) {
                targets.Add(Build(funcType|FunctionType.Params, paramsMethods, maxParamArgCnt));
            }

            return targets.ToArray();
        }

        public static ParamTree Build(FunctionType functionType, IList<MethodTracker> methods, int argCnt) {
            ParamTree pt = new ParamTree(argCnt);
            pt.funcType = functionType;

            for (int i = 0; i < methods.Count; i++) {
                pt.AddMethod(methods[i]);
            }

            pt.FinishTree();

            //pt.Walk(new ParamTreeDumper(pt, methods[0].Name));
            return pt;
        }

        public IList<int> ReturnPositions {
            get {
                return extraReturnPositions;
            }
        }

        public int ArgumentCount {
            get {
                return argumentCount;
            }
        }

        public FunctionType FunctionType {
            get {
                return funcType;
            }
        }

        /// <summary>
        /// Performs a depth-first walk of the tree
        /// </summary>
        public void Walk(IParamWalker callback) {
            callback.PreArgument(0, Children.Count);

            for (int i = 0; i < Children.Count; i++) {
                WalkWorker(callback, Children[i], 0);
            }
            callback.PostArgument(0);
        }

        /// <summary>
        /// Represent settings which make param nodes unique inface of a type that is identical.
        /// </summary>
        public enum NodeFlags {
            None = 0x00,
            ByRef = 0x01,
            Out = 0x02,
            Params = 0x04,
            MixedInstanceStatic = 0x08
        }

        #endregion

        #region Private helper methods

        public void AddMethod(MethodTracker method) {
#if DEBUG
            if (fFinished) throw new InvalidOperationException("Cannot add methods to finished param trees");
#endif

            ParamTreeNode curNode = this;
            ParameterInfo[] pis = method.GetParameters();
            bool fIsParams = false;

            if (pis.Length > 0 && ReflectionUtil.IsParamArray(pis[pis.Length - 1])) fIsParams = true;

            if (!method.IsStatic) {
                Type instType = method.DeclaringType;
                if ((funcType & FunctionType.FunctionMethodMask) == FunctionType.FunctionMethodMask) {
                    instType = typeof(InstanceArgument);
                }

                curNode = curNode.AppendChild(method, instType, NodeFlags.None, argumentCount == 1);
                AppendParameters(method, curNode, pis, fIsParams, 1, 0);
            } else {
                int depthStart = 0;
                if ((funcType & FunctionType.OpsFunction) != 0 && 
                    (funcType & FunctionType.FunctionMethodMask) == FunctionType.FunctionMethodMask) {
                    // this is an ops function that is combined w/ an ops method.  We need to support
                    // disambiguating between bound & unbound method calls, so we transform arg 1
                    // to an instance just like we do for non-static functions, and then skip the 1st
                    // parameter in AppendParameters.  In either case we want both depths to
                    // start at 1 (we're appending @ depth 1, and reading from param 1 because we've
                    // used param 0) so we just use depthStart here for both.
                    depthStart = 1;
                    curNode = curNode.AppendChild(method, typeof(InstanceArgument), NodeFlags.None, argumentCount == 1);
                }

                AppendParameters(method, curNode, pis, fIsParams, depthStart, depthStart);
            }
            
            Methods.Add(method);
        }

        private void AppendParameters(MethodTracker method, ParamTreeNode curNode, ParameterInfo[] pis, bool fIsParams, int depthStart, int pisStart) {
            int argCnt = argumentCount;
            for (int i = depthStart; i < argCnt; i++) {                

                NodeFlags flags;
                Type curType = GetCurrentType(pis, i - depthStart+pisStart, fIsParams, out flags);

                if((flags & NodeFlags.Out) != 0 && (i - depthStart+pisStart) < pis.Length){
                    // got an out param, need one more argument still...
                    argCnt++;
                }

                bool fLastArg = (i == argumentCount - 1);

                curNode = curNode.AppendChild(method, curType, flags, fLastArg);
            }
        }

        /// <summary>
        /// Marks the tree as being finished allowing it to calculate
        /// any final state and throw away any temporary state necessary
        /// for building the tree.  A finished tree cannot have new methods added.
        /// </summary>
        public void FinishTree() {
#if DEBUG
            fFinished = true;
#endif
            if (argumentCount == 0) return;

            // if we have a sole param array our signature should
            // be a param array as well...
            ParameterInfo[] pis = Methods[0].GetParameters();

            // if we dont have static & instance methods in this set then
            // the reflect optimizer needs to generate code to check both
            // instance & argument values.  We mark methods that are truly mixed.
            bool fStaticThis=false, fInstance=false;
            for (int i = 0; i < Methods.Count; i++) {
                if (Methods[i].IsStatic) {
                    if (pis.Length > 0 && pis[0].ParameterType == Methods[i].DeclaringType) {
                        fStaticThis = true;
                    }
                } else {
                    fInstance = true;
                }
            }

            if (fStaticThis && fInstance) {
                for (int i = 0; i < Children.Count; i++) {
                    Children[i].Flags |= NodeFlags.MixedInstanceStatic;
                }
            }

            FinishNodes(this);
        }

        private void FinishNodes(ParamTreeNode current) {
            current.FinishNode();
            foreach (ParamTreeNode node in current.Children) {
                FinishNodes(node);
            }
        }

        private Type GetCurrentType(ParameterInfo[] pis, int curArg, bool fIsParams, out NodeFlags flags) {
            
            if (fIsParams && curArg >= (pis.Length - 1)) {
                flags = NodeFlags.Params;

                return pis[pis.Length - 1].ParameterType.GetElementType();
            } else if(curArg < pis.Length) {
                flags = NodeFlags.None;

                // out & by-ref: simply update our list of out-positions, and mark
                // the node w/ the appropriate flags.
                if (pis[curArg].IsOut && !pis[curArg].IsIn) flags |= NodeFlags.Out;
                else if (pis[curArg].ParameterType.IsByRef) flags |= NodeFlags.ByRef;

                if(flags != NodeFlags.None){
                    if (extraReturnPositions != null) {
                        int[] newpos = new int[extraReturnPositions.Length + 1];
                        Array.Copy(extraReturnPositions, newpos, extraReturnPositions.Length);
                        extraReturnPositions = newpos;
                    } else {
                        extraReturnPositions = new int[1];
                    }

                    extraReturnPositions[extraReturnPositions.Length - 1] = curArg;
                }
                
                return pis[curArg].ParameterType;
            } else {
                flags = NodeFlags.Params;

                // we're generating a params method w/ a mixed tree-depth.
                Debug.Assert((FunctionType & FunctionType.Params) != 0);
                return null;
            }
        }

        private void WalkWorker(IParamWalker callback, ParamTreeNode curNode, int depth) {
            callback.Walk(depth, curNode);

            if (curNode.Children.Count != 0) {
                callback.PreArgument(depth + 1, curNode.Children.Count);

                for (int i = 0; i < curNode.Children.Count; i++) {
                    WalkWorker(callback, curNode.Children[i], depth + 1);
                }

                callback.PostArgument(depth + 1);
            }
        }

        private static int GetPythonExposedArgCount(MethodTracker method) {
            int baseArgs = method.StaticArgs;
            ParameterInfo[] pis = method.GetParameters();
            for (int i = 0; i < pis.Length; i++) {
                if (pis[i].IsOut && !pis[i].IsIn) {
                    baseArgs--;
                }
            }
            return baseArgs;
        }

        private static bool MatchesArgCount(MethodTracker method, int sigLen, int paramLen) {
            return paramLen == sigLen || (paramLen > sigLen && (paramLen - method.DefaultCount) <= sigLen);
        }

        #endregion

#if DEBUG
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            Walk(new ParamTreeDumper(this, Methods[0].Name, sb));
            return sb.ToString();
        }
#endif

    }

    class ParamTreeNode {
        public Type ParamType;
        public ParamTree.NodeFlags Flags;
        public IList<MethodTracker> Methods;
        public IList<ParamTreeNode> Children;

        public ParamTreeNode(Type t, ParamTree.NodeFlags flags) {
            ParamType = t;
            Flags = flags;
            Methods = new List<MethodTracker>();
            Children = new List<ParamTreeNode>();
        }

        public void FinishNode() {
            // compress List<T>'s down to pure arrays after
            // everything has been added.
            Methods = CopyIListToArray(Methods);
            Children = CopyIListToArray(Children);
        }

        private static T[] CopyIListToArray<T>(IList<T> list) {
            T[] array = new T[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        public ParamTreeNode AppendChild(MethodTracker method, Type curType, ParamTree.NodeFlags flags, bool fLastArg) {
            bool fFound = false;
            ParamTreeNode res = this;

            for (int i = 0; i < Children.Count; i++) {
                if (Children[i].ParamType == curType && Children[i].Flags == flags) {
                    res = Children[i];
                    res.Methods.Add(method);
                    fFound = true;
                    break;
                }
            }

            if (!fFound) {
                res = InsertNewNode(curType, flags);
                res.Methods.Add(method);
            } else if (fLastArg) {
                // last node, we shouldn't be adding
                // extra methods here.  We have two ways
                // we get here:
                //  1. We have a static and non-static overload, prefer the instance method
                //  2. We have default values for one of the methods, prefer the one w/o defaults.
                // 
                // Both of these are identical checks: We prefer the method w/ less parameters.

                Debug.Assert(res.Methods.Count == 2);

                if (method.GetParameters().Length < res.Methods[0].GetParameters().Length) {
                    // remove the old one.
                    res.Methods.RemoveAt(0);
                } else {
                    // remove the new one.
                    res.Methods.RemoveAt(1);
                }
            }

            return res;

        }

        private ParamTreeNode InsertNewNode(Type newNodeType, ParamTree.NodeFlags flags) {
            ParamTreeNode newNode = new ParamTreeNode(newNodeType, flags);

            if (newNodeType != null) {
                // we've made the node, use the element type for the checks below...
                if (newNodeType.IsByRef) newNodeType = newNodeType.GetElementType();

                // insert based upon subclassing order (if we're a subclass
                // of someone we need to come first). When we walk the tree we'll
                // then emit checks for more specific types before less specific types.

                OpsReflectedType newDt = Ops.GetDynamicTypeFromType(newNodeType) as OpsReflectedType;
                Type extensibleType = null;
                if (newDt != null) {
                    extensibleType = newDt.GetTypeToExtend();
                }

                if (newNodeType != typeof(object)) {
                    for (int i = 0; i < Children.Count; i++) {
                        Type childParamType = Children[i].ParamType;
                        
                        if (childParamType == null) {
                            // params method w/ mixed levels, no subclasses checks to be done.
                            continue;   
                        }

                        if (childParamType.IsByRef) childParamType = childParamType.GetElementType();

                        // extensible types are logically subclasses of their "parent" type, and
                        // therefore their check needs to go first.  If extensible is added first
                        // then the real type will come next automatically, otherwise we'll
                        // check for the type here & early out if we're adding the extensible type.
                        if (newNodeType == extensibleType && childParamType == newDt.type) {
                            Children.Insert(i, newNode);
                            return newNode;
                        }

                        if (newNodeType.IsSubclassOf(childParamType) ||
                            (newNodeType.IsClass && childParamType.IsInterface)) {      // classes before interfaces
                            Children.Insert(i, newNode);
                            return newNode;
                        }
                    }
                }
            }

            Children.Add(newNode);

            return newNode;
        }
    }

    interface IParamWalker {
        void PreArgument(int param, int typeCount);
        void Walk(int param, ParamTreeNode node);
        void PostArgument(int param);
    }

#if DEBUG
    class ParamTreeDumper : IParamWalker
    {
        private ParamTree pt;
        private StringBuilder sb;
        public ParamTreeDumper(ParamTree pt, string name, StringBuilder sb){
            this.sb = sb;
            this.pt = pt;
            Write(String.Format("Param tree for: {0}", name));
        }

        public ParamTreeDumper(ParamTree pt, string name) : this(pt,name,null) {
        }

        #region IParamWalker Members

        public void PreArgument(int param, int typeCount) {
        }

        public void Walk(int param, ParamTreeNode node) {
            for(int i = 0; i<param+1; i++) Write("    ");
            WriteLine(String.Format("{0} {1}", node.ParamType, node.Flags));
        }

        public void PostArgument(int param) {
        }

        private void Write(string s) {
            if (sb == null) Console.Write(s);
            else sb.Append(s);
        }

        private void WriteLine(string s) {
            if (sb == null) Console.WriteLine(s);
            else sb.AppendLine(s);
        }

        #endregion
    }
#endif
}
