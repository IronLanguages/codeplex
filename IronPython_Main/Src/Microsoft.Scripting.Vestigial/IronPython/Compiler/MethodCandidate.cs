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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Text;

using PythonBinder = IronPython.Runtime.Calls.PythonBinder;
using Converter = IronPython.Runtime.Converter;
using Tuple = IronPython.Runtime.Tuple;
using Ops = IronPython.Runtime.Operations.Ops;
using TypeCache = IronPython.Runtime.Types.TypeCache;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Hosting;

namespace IronPython.Compiler {
    public class MethodCandidate {
        private MethodTarget _target;
        private List<ParameterWrapper> _parameters;
        private NarrowingLevel _narrowingLevel;

        public MethodCandidate(MethodCandidate previous, NarrowingLevel narrowingLevel) {
            this._target = previous.Target;
            this._parameters = previous._parameters;
            _narrowingLevel = narrowingLevel;
        }

        public MethodCandidate(MethodTarget target, List<ParameterWrapper> parameters) {
            _target = target;
            _parameters = parameters;
            _narrowingLevel = NarrowingLevel.None;
            parameters.TrimExcess();
        }

        public MethodTarget Target {
            get { return _target; }
        }

        public bool IsApplicable(DynamicType[] types, NarrowingLevel allowNarrowing) {
            for (int i = 0; i < types.Length; i++) {
                if (!_parameters[i].HasConversionFrom(types[i], allowNarrowing)) return false;
            }
            return true;
        }

        public bool CheckArgs(CodeContext context, object[] args) {
            DynamicType[] types = new DynamicType[args.Length];
            for (int i = 0; i < types.Length; i++) {
                types[i] = DynamicHelpers.GetDynamicType(args[i]);
            }
            if (this.IsApplicable(types, NarrowingLevel.None)) return true;

            return Target.CheckArgs(context, args);
        }

        public int? CompareParameters(MethodCandidate other) {
            return ParameterWrapper.CompareParameters(this._parameters, other._parameters);
        }

        public int CompareTo(MethodCandidate other, CallType callType) {
            int? cmpParams = CompareParameters(other);
            if (cmpParams == +1 || cmpParams == -1) return (int)cmpParams;

            int ret = Target.CompareEqualParameters(other.Target);
            if (ret != 0) return ret;

            if (CompilerHelpers.IsStatic(Target.Method) && !CompilerHelpers.IsStatic(other.Target.Method)) {
                return callType == CallType.ImplicitInstance ? -1 : +1;
            } else if (!CompilerHelpers.IsStatic(Target.Method) && CompilerHelpers.IsStatic(other.Target.Method)) {
                return callType == CallType.ImplicitInstance ? +1 : -1;
            }

            return 0;
        }

        public MethodCandidate MakeParamsExtended(ActionBinder binder, int count) {
            if (count < _parameters.Count - 1) return null;

            List<ParameterWrapper> newParameters = _parameters.GetRange(0, _parameters.Count - 1);
            Type elementType = _parameters[_parameters.Count - 1].Type.GetElementType();

            while (newParameters.Count < count) {
                ParameterWrapper param = new ParameterWrapper(binder, elementType);
                newParameters.Add(param);
            }

            return new MethodCandidate(_target.MakeParamsExtended(count), newParameters);
        }

        public override string ToString() {
            return string.Format("MethodCandidate({0})", Target);
        }

        public string ToSignatureString(string name, CallType callType) {
            StringBuilder buf = new StringBuilder(name);
            buf.Append("(");
            bool isFirstArg = true;
            int i = 0;
            if (callType == CallType.ImplicitInstance) i = 1;
            for (; i < _parameters.Count; i++) {
                if (isFirstArg) isFirstArg = false;
                else buf.Append(", ");
                buf.Append(_parameters[i].ToSignatureString());
            }
            buf.Append(")");
            return buf.ToString(); //@todo add helper info for more interesting signatures
        }

        public NarrowingLevel NarrowingLevel {
            get {
                return _narrowingLevel;
            }
        }
    }





//    /// <summary>
//    /// Represents a possible method to dispatch to.
//    /// </summary>
//    class MethodCandidate {
//        private MethodTarget _target;
//        private List<Type> _parameters;

//        private MethodCandidate(MethodTarget target, List<Type> parameters) {
//            this._target = target;
//            this._parameters = parameters;
//        }

//        public static IEnumerable<MethodCandidate> MakeNonParamsCandidates(MethodBase method) {
//            int baseParams = GetBaseParams(method);
//            int minParams = baseParams - GetMaximumDefaultParams(method);
//            for (int i = minParams; i <= baseParams; i++) {
//                yield return MakeSimple(method, i);
//            }            

//            int outParams = CompilerHelpers.GetOutParameterCount(method);
//            if (outParams > 0) {
//                yield return MakeByRefReduced(method);
//            }
//        }

//        public static MethodCandidate MakeSimple(MethodBase method, int argCount) {
//            CandidateBuilder b = new CandidateBuilder(method);
//            b.AddInitialBuilders();
//            b.AddStandardBuilders(argCount);
//            return b.FinishCandidate();
//        }

//        public static MethodCandidate MakeByRefReduced(MethodBase method) {
//            CandidateBuilder b = new CandidateBuilder(method);
//            b.AddInitialBuilders();
//            b.AddByRefReducedBuilders();
//            return b.FinishCandidate();
//        }

//        public static MethodCandidate MakeParamsExpanded(MethodBase method, int argCount) {
//            if (argCount < GetBaseParams(method)) return null;

//            CandidateBuilder b = new CandidateBuilder(method);
//            b.AddInitialBuilders();
//            b.AddParamBuilders(argCount);
//            return b.FinishCandidate();
//        }

//        private static int GetBaseParams(MethodBase method) {
//            ParameterInfo[] pis = method.GetParameters();
//            int count = pis.Length;
//            if (!CompilerHelpers.IsStatic(method)) {
//                count += 1;
//            }
//            if (pis.Length > 0 && pis[0].ParameterType == typeof(CodeContext)) {
//                count -= 1;
//            }
//            return count;
//        }

//        private static int GetMaximumDefaultParams(MethodBase method) {
//            ParameterInfo[] pis = method.GetParameters();
//            int count = 0;
//            for (; count < pis.Length; count++) {
//                ParameterInfo pi = pis[pis.Length-count-1];
//                if (pi.DefaultValue != DBNull.Value) continue;
//                if (CompilerHelpers.IsParamArray(pi)) continue;
//                return count;
//            }
//            return count;
//        }

//        private class CandidateBuilder {
//            private MethodBase _method;
//            private ParameterInfo[] _paramInfos;
//            private int _paramIndex = 0;

//            private ArgBuilder _instanceBuilder;
//            private ReturnBuilder _returnBuilder;
//            private List<Type> _parameters = new List<Type>();
//            private List<ArgBuilder> _argBuilders = new List<ArgBuilder>();

//            public CandidateBuilder(MethodBase method) {
//                this._method = method;
//                this._paramInfos = method.GetParameters();
//            }

//            public void AddInitialBuilders() {
//                if (!CompilerHelpers.IsStatic(_method)) {
//                    _instanceBuilder = new SimpleArgBuilder(_parameters.Count, _method.DeclaringType);
//                    _parameters.Add(_method.DeclaringType);
//                } else {
//                    _instanceBuilder = new NullArgBuilder();
//                }

//                if (_paramInfos.Length > 0 && _paramInfos[0].ParameterType == typeof(CodeContext)) {
//                    _argBuilders.Add(new ContextArgBuilder());
//                    _paramIndex += 1;
//                }
//            }

//            private void AddSimpleArg(ParameterInfo pi) {
//                Type type = pi.ParameterType;
//                if (type.IsByRef) {
//                    _argBuilders.Add(new ReferenceArgBuilder(_parameters.Count, type));
//                } else {
//                    _argBuilders.Add(new SimpleArgBuilder(_parameters.Count, type));
//                }
//                _parameters.Add(type);
//            }

//            private void AddDefaultArg(ParameterInfo pi) {
//                if (pi.DefaultValue != DBNull.Value) {
//                    _argBuilders.Add(new DefaultArgBuilder(pi.ParameterType, pi.DefaultValue));
//                } else {
//                    Debug.Assert(CompilerHelpers.IsParamArray(pi));
//                    _argBuilders.Add(new NullArgBuilder()); //???
//                }
//            }

//            public void AddStandardBuilders(int argCount) {
//                while (_paramIndex < _paramInfos.Length) {
//                    ParameterInfo pi = _paramInfos[_paramIndex++];
//                    if (_parameters.Count < argCount) {
//                        AddSimpleArg(pi);
//                    } else {
//                        AddDefaultArg(pi);
//                    }
//                }
//                _returnBuilder = new ReturnBuilder(CompilerHelpers.GetReturnType(_method));
//            }

//            public void AddByRefReducedBuilders() {
//                List<int> returnArgs = new List<int>();
//                if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
//                    returnArgs.Add(-1);
//                }

//                while (_paramIndex < _paramInfos.Length) {
//                    ParameterInfo pi = _paramInfos[_paramIndex++];
//                    if (pi.ParameterType.IsByRef) {
//                        returnArgs.Add(_parameters.Count);
//                        if (CompilerHelpers.IsOutParameter(pi)) {
//                            _argBuilders.Add(new NullArgBuilder()); //!!!
//                        } else {
//                            Type elementType = pi.ParameterType.GetElementType();
//                            _argBuilders.Add(new SimpleArgBuilder(_parameters.Count, elementType));
//                            _parameters.Add(elementType);
//                        }
//                    } else {
//                        AddSimpleArg(pi);
//                    }
//                }

//                _returnBuilder = new ByRefReturnBuilder(CompilerHelpers.GetReturnType(_method), returnArgs);
//            }

//            public void AddParamBuilders(int argCount) {
//                while (_paramIndex < _paramInfos.Length-1) {
//                    ParameterInfo pi = _paramInfos[_paramIndex++];
//                    AddSimpleArg(pi);
//                }

//                Type elementType = _paramInfos[_paramIndex].ParameterType.GetElementType();

//                _argBuilders.Add(new ParamsArgBuilder(_parameters.Count, argCount - _parameters.Count, elementType));

//                while (_parameters.Count < argCount) {
//                    _parameters.Add(elementType);
//                }

//                _returnBuilder = new ReturnBuilder(CompilerHelpers.GetReturnType(_method));
//            }

//            public MethodCandidate FinishCandidate() {
//                _argBuilders.TrimExcess();
//                _parameters.TrimExcess();
//                MethodTarget target = new MethodTarget(_method, _parameters.Count, _instanceBuilder, _argBuilders, _returnBuilder);
//                return new MethodCandidate(target, _parameters);
//            }
//        }

//        public IList<Type> Parameters {
//            get { return _parameters; }
//        }

//        public MethodTarget Target {
//            get { return _target; }
//        }

//        public bool IsApplicable(DynamicType[] types, NarrowingLevel allowNarrowing) {
//            for (int i = 0; i < types.Length; i++) {
//                if (!MethodBinder.HasConversionFrom(_parameters[i], types[i], allowNarrowing)) return false;
//            }
//            return true;
//        }

//        public int CompareTo(MethodCandidate other, CallType callType) {
//            int? cmpParams = MethodBinder.CompareParameters(this._parameters, other._parameters);
//            if (cmpParams == +1 || cmpParams == -1) return (int)cmpParams;

//            int ret = CompareEqualParameters(other);
//            if (ret != 0) return ret;

//            if (_target.Method.IsStatic && !other._target.Method.IsStatic) {
//                return callType == CallType.ImplicitInstance ? -1 : +1;
//            } else if (!_target.Method.IsStatic && other._target.Method.IsStatic) {
//                return callType == CallType.ImplicitInstance ? +1 : -1;
//            }

//            return 0;
//        }

//        private static int FindMaxPriority(List<ArgBuilder> abs) {
//            int max = -1;
//            foreach (ArgBuilder ab in abs) {
//                max = Math.Max(max, ab.Priority);
//            }
//            return max;
//        }

//        public int CompareEqualParameters(MethodCandidate other) {
//            // Prefer normal methods over explicit interface implementations
//            if (other.Target.Method.IsPrivate && !this.Target.Method.IsPrivate) return +1;
//            if (this.Target.Method.IsPrivate && !other.Target.Method.IsPrivate) return -1;

//            // Prefer non-generic methods over generic methods
//            if (Target.Method.IsGenericMethod) {
//                if (!other.Target.Method.IsGenericMethod) {
//                    return -1;
//                } else {
//                    //!!! Need to support selecting least generic method here
//                    return 0;
//                }
//            } else if (other.Target.Method.IsGenericMethod) {
//                return +1;
//            }

//            //prefer methods without out params over those with them
//            switch (Compare(Target.returnBuilder.CountOutParams, other.Target.returnBuilder.CountOutParams)) {
//                case 1: return -1;
//                case -1: return 1;
//            }

//            //prefer methods using earlier conversions rules to later ones
//            int maxPriorityThis = FindMaxPriority(Target.argBuilders);
//            int maxPriorityOther = FindMaxPriority(other.Target.argBuilders);

//            if (maxPriorityThis < maxPriorityOther) return +1;
//            if (maxPriorityOther < maxPriorityThis) return -1;

//            return 0;
//        }

//        protected static int Compare(int x, int y) {
//            if (x < y) return -1;
//            else if (x > y) return +1;
//            else return 0;
//        }

//        public override string ToString() {
//            return string.Format("MethodCandidate({0})", Target);
//        }

//        public string ToSignatureString(string name, CallType callType) {
//            StringBuilder buf = new StringBuilder(name);
//            buf.Append("(");
//            bool isFirstArg = true;
//            int i = 0;
//            if (callType == CallType.ImplicitInstance) i = 1;
//            for (; i < _parameters.Count; i++) {
//                if (isFirstArg) isFirstArg = false;
//                else buf.Append(", ");
//                buf.Append(_parameters[i].Name);
//            }
//            buf.Append(")");
//            return buf.ToString(); //@todo add helper info for more interesting signatures
//        }
//    }
}
