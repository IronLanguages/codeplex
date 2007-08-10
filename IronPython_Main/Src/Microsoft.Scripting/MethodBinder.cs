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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Types;

namespace Microsoft.Scripting {
    public class MethodBinder {
        internal string _name;
        private BinderType _binderType;
        internal Dictionary<int, TargetSet> _targetSets = new Dictionary<int, TargetSet>();
        internal List<ParamsMethodMaker> _paramsMakers;
        internal ActionBinder _binder;

        private static bool IsUnsupported(MethodBase method) {
            return (method.CallingConvention & CallingConventions.VarArgs) != 0
                || method.ContainsGenericParameters;
        }

        private static bool IsKwDictMethod(MethodBase method) {
            ParameterInfo[] pis = method.GetParameters();
            for (int i = pis.Length - 1; i >= 0; i--) {
                if (pis[i].IsDefined(typeof(ParamDictionaryAttribute), false))
                    return true;
            }
            return false;
        }

        public static MethodBinder MakeBinder(ActionBinder binder, string name, MethodBase[] mis, BinderType binderType) {
            return new MethodBinder(binder, name, mis, binderType);
        }

        public AbstractValue AbstractCall(CallType callType, IList<AbstractValue> args) {
            TargetSet ts = this.GetTargetSet(args.Count);
            if (ts != null) {
                return ts.AbstractCall(callType, args);
            } else {
                return AbstractValue.TypeError(BadArgumentCount(callType, args.Count).Message);
            }
        }

        public MethodCandidate MakeBindingTarget(CallType callType, Type[] types) {
            return MakeBindingTarget(callType, types, SymbolId.EmptySymbols);
        }

        public MethodCandidate MakeBindingTarget(CallType callType, Type[] types, out Type[] argumentTests) {
            return MakeBindingTarget(callType, types, SymbolId.EmptySymbols, out argumentTests);
        }

        public MethodCandidate MakeBindingTarget(CallType callType, Type[] types, SymbolId[] names) {
            Type[] argumentTests;
            return MakeBindingTarget(callType, types, names, out argumentTests);
        }

        public MethodCandidate MakeBindingTarget(CallType callType, Type[] types, SymbolId[] names, out Type[] argumentTests) {
            TargetSet ts = this.GetTargetSet(types.Length);
            if (ts != null) {
                return ts.MakeBindingTarget(callType, types, names, out argumentTests);
            }
            argumentTests = null;
            return null;
        }

        private MethodBinder(ActionBinder binder, string name, MethodBase[] methods, BinderType binderType) {
            this._binder = binder;
            this._name = name;
            _binderType = binderType;
            foreach (MethodBase method in methods) {
                if (IsUnsupported(method)) continue;
                if (methods.Length > 1 && IsKwDictMethod(method)) continue;
                AddBasicMethodTargets(method);
            }

            if (_paramsMakers != null) {
                foreach (ParamsMethodMaker maker in _paramsMakers) {
                    foreach (int count in _targetSets.Keys) {
                        MethodCandidate target = maker.MakeTarget(binder, count);
                        if (target != null) AddTarget(target);
                    }
                }
            }
        }

        public string Name { get { return _name; } }

        internal bool IsBinaryOperator {
            get {
                return _binderType == BinderType.BinaryOperator || IsComparison;
            }
        }

        private bool IsComparison {
            get {
                return _binderType == BinderType.ComparisonOperator;
            }
        }

        private void GetMinAndMaxArgs(out int minArgs, out int maxArgs) {
            List<int> argCounts = new List<int>(_targetSets.Keys);
            argCounts.Sort();
            minArgs = argCounts[0];
            maxArgs = argCounts[argCounts.Count - 1];
        }

        private Exception BadArgumentCount(CallType callType, int argCount) {
            if (_targetSets.Count == 0) return new ArgumentTypeException("no callable targets, if this is a generic method make sure to specify the type parameters");
            int minArgs, maxArgs;
            GetMinAndMaxArgs(out minArgs, out maxArgs);

            if (callType == CallType.ImplicitInstance) {
                argCount -= 1;
                if (maxArgs > 0) {
                    minArgs -= 1;
                    maxArgs -= 1;
                }
            }

            // This generates Python style error messages assuming that all arg counts in between min and max are allowed
            //It's possible that discontinuous sets of arg counts will produce a weird error message
            return RuntimeHelpers.TypeErrorForIncorrectArgumentCount(_name, maxArgs, maxArgs - minArgs, argCount);
        }


        private TargetSet BuildTargetSet(int count) {
            TargetSet ts = new TargetSet(this, count);
            if (_paramsMakers != null) {
                foreach (ParamsMethodMaker maker in _paramsMakers) {
                    MethodCandidate target = maker.MakeTarget(_binder, count);
                    if (target != null) ts.Add(target);
                }
            }
            return ts;
        }

        private TargetSet GetTargetSet(int nargs) {
            TargetSet ts;
            if (_targetSets.TryGetValue(nargs, out ts)) {
                return ts;
            } else if (_paramsMakers != null) {
                ts = BuildTargetSet(nargs);
                if (ts._targets.Count > 0) {
                    return ts;
                }
            }
            return null;
        }

        public object CallInstanceReflected(CodeContext context, object instance, params object[] args) {
            return CallReflected(context, CallType.ImplicitInstance, ArrayUtils.Insert(instance, args));
        }

        public object CallReflected(CodeContext context, CallType callType, params object[] args) {
            TargetSet ts = GetTargetSet(args.Length);
            if (ts != null) return ts.CallReflected(context, callType, args);
            throw BadArgumentCount(callType, args.Length);
        }

        private void AddTarget(MethodCandidate target) {
            int count = target.Target.ParameterCount;
            TargetSet set;
            if (!_targetSets.TryGetValue(count, out set)) {
                set = new TargetSet(this, count);
                _targetSets[count] = set;
            }
            set.Add(target);
        }

        private void AddSimpleTarget(MethodCandidate target) {
            AddTarget(target);
            if (CompilerHelpers.IsParamsMethod(target.Target.Method)) {
                ParamsMethodMaker maker = new ParamsMethodMaker(target);
                if (_paramsMakers == null) _paramsMakers = new List<ParamsMethodMaker>();
                _paramsMakers.Add(maker);
            }
        }

        private void AddBasicMethodTargets(MethodBase method) {
            List<ParameterWrapper> parameters = new List<ParameterWrapper>();
            int argIndex = 0;
            ArgBuilder instanceBuilder;
            if (!CompilerHelpers.IsStatic(method)) {
                parameters.Add(new ParameterWrapper(_binder, method.DeclaringType, true));
                instanceBuilder = new SimpleArgBuilder(argIndex++, parameters[0].Type);
            } else {
                instanceBuilder = new NullArgBuilder();
            }

            List<ArgBuilder> argBuilders = new List<ArgBuilder>();
            List<ArgBuilder> defaultBuilders = new List<ArgBuilder>();
            bool hasByRef = false;

            int paramCount = 0;
            foreach (ParameterInfo pi in method.GetParameters()) {
                if (pi.ParameterType == typeof(CodeContext) && paramCount == 0) {
                    argBuilders.Add(new ContextArgBuilder());
                    continue;
                }
                paramCount++;

                if (pi.DefaultValue != DBNull.Value) {
                    defaultBuilders.Add(new DefaultArgBuilder(pi.ParameterType, pi.DefaultValue));
                } else if (defaultBuilders.Count > 0) {
                    // If we get a bad method with non-contiguous default values, then just use the contiguous list
                    defaultBuilders.Clear();
                }

                if (pi.ParameterType.IsByRef) {
                    hasByRef = true;
                    Type refType = typeof(Reference<>).MakeGenericType(pi.ParameterType.GetElementType());
                    ParameterWrapper param = new ParameterWrapper(_binder, refType, true, SymbolTable.StringToId(pi.Name));
                    parameters.Add(param);
                    argBuilders.Add(new ReferenceArgBuilder(argIndex++, param.Type));
                } else {
                    ParameterWrapper param = new ParameterWrapper(_binder, pi);
                    parameters.Add(param);
                    argBuilders.Add(new SimpleArgBuilder(argIndex++, param.Type, CompilerHelpers.IsParamArray(pi)));
                }
            }


            ReturnBuilder returnBuilder = new ReturnBuilder(CompilerHelpers.GetReturnType(method));

            for (int i = 1; i < defaultBuilders.Count + 1; i++) {
                List<ArgBuilder> defaultArgBuilders = argBuilders.GetRange(0, argBuilders.Count - i);
                defaultArgBuilders.AddRange(defaultBuilders.GetRange(defaultBuilders.Count - i, i));
                AddTarget(MakeMethodCandidate(_binder, method, parameters.GetRange(0, parameters.Count - i),
                    instanceBuilder, defaultArgBuilders, returnBuilder));
            }

            if (hasByRef) AddSimpleTarget(MakeByRefReducedMethodTarget(method));
            AddSimpleTarget(MakeMethodCandidate(_binder, method, parameters, instanceBuilder, argBuilders, returnBuilder));
        }

        private static MethodCandidate MakeMethodCandidate(ActionBinder binder, MethodBase method, List<ParameterWrapper> parameters, ArgBuilder instanceBuilder, List<ArgBuilder> argBuilders, ReturnBuilder returnBuilder) {
            return new MethodCandidate(
                new MethodTarget(binder, method, parameters.Count, instanceBuilder, argBuilders, returnBuilder),
                parameters);
        }


        private MethodCandidate MakeByRefReducedMethodTarget(MethodBase method) {
            List<ParameterWrapper> parameters = new List<ParameterWrapper>();
            int argIndex = 0;
            ArgBuilder instanceBuilder;
            if (!CompilerHelpers.IsStatic(method)) {
                parameters.Add(new ParameterWrapper(_binder, method.DeclaringType, true));
                instanceBuilder = new SimpleArgBuilder(argIndex++, parameters[0].Type);
            } else {
                instanceBuilder = new NullArgBuilder();
            }

            List<ArgBuilder> argBuilders = new List<ArgBuilder>();

            List<int> returnArgs = new List<int>();
            if (CompilerHelpers.GetReturnType(method) != typeof(void)) {
                returnArgs.Add(-1);
            }

            int paramCount = 0;
            foreach (ParameterInfo pi in method.GetParameters()) {
                if (pi.ParameterType == typeof(CodeContext) && paramCount == 0) {
                    argBuilders.Add(new ContextArgBuilder());
                    continue;
                }
                paramCount++;

                if (pi.ParameterType.IsByRef) {
                    returnArgs.Add(argBuilders.Count);
                    if (CompilerHelpers.IsOutParameter(pi)) {
                        argBuilders.Add(new NullArgBuilder()); //TODO better option here
                    } else {
                        ParameterWrapper param = new ParameterWrapper(_binder, pi.ParameterType.GetElementType(), SymbolTable.StringToId(pi.Name));
                        parameters.Add(param);
                        argBuilders.Add(new SimpleArgBuilder(argIndex++, pi.ParameterType.GetElementType()));
                    }
                } else {
                    ParameterWrapper param = new ParameterWrapper(_binder, pi);
                    parameters.Add(param);
                    argBuilders.Add(new SimpleArgBuilder(argIndex++, param.Type, CompilerHelpers.IsParamArray(pi)));
                }
            }

            ReturnBuilder returnBuilder = new ByRefReturnBuilder(_binder, CompilerHelpers.GetReturnType(method), returnArgs);

            return MakeMethodCandidate(_binder, method, parameters, instanceBuilder, argBuilders, returnBuilder);
        }

        public override string ToString() {
            string res = "";
            for (int i = 0; i < _targetSets.Count; i++) {
                res += _targetSets[i] + Environment.NewLine;
            }
            return res;
        }
    }

    public class TargetSet {
        private MethodBinder _binder;
        internal int _count;
        internal List<MethodCandidate> _targets;

        public TargetSet(MethodBinder binder, int count) {
            this._binder = binder;
            this._count = count;
            _targets = new List<MethodCandidate>();
        }
       
        public MethodCandidate MakeBindingTarget(CallType callType, Type[] types, SymbolId[] names, out Type[] argTests) {
            List<MethodCandidate> targets = SelectTargets(callType, types, names);

            if (targets.Count == 1) {                
                argTests = GetTypesForTest(targets[0], types, callType, _targets);
                return targets[0];
            }
            argTests = null;
            return null;
        }

        public AbstractValue AbstractCall(CallType callType, IList<AbstractValue> args) {
            Type[] types = AbstractValue.GetTypes(args);
            List<MethodCandidate> targets = SelectTargets(callType, types);

            if (targets.Count == 1) {
                return targets[0].Target.AbstractCall(new AbstractContext(_binder._binder, null), args);
            } else {
                if (targets.Count == 0) {
                    return AbstractValue.TypeError(NoApplicableTargetMessage(callType, types));
                } else {
                    return AbstractValue.TypeError(MultipleTargetsMessage(targets, callType, types));
                }
            }
        }

        public object CallReflected(CodeContext context, CallType callType, object[] args) {
            return CallReflected(context, callType, args, SymbolId.EmptySymbols);
        }

        public object CallReflected(CodeContext context, CallType callType, object[] args, SymbolId[] names) {
            List<MethodCandidate> targets = FindTarget(callType, args);

            if (targets.Count == 1) {
                if (_binder.IsBinaryOperator) {
                    if (!targets[0].CheckArgs(context, args, names)) {
                        return context.LanguageContext.GetNotImplemented(targets[0]);
                    }
                }
                return targets[0].Target.CallReflected(context, args);
            }

            return CallFailed(context, targets, callType, args);
        }

        private object CallFailed(CodeContext context, List<MethodCandidate> targets, CallType callType, object[] args) {
            if (_binder.IsBinaryOperator) {
                return context.LanguageContext.GetNotImplemented(targets.ToArray());
            }

            if (targets.Count == 0) {
                throw NoApplicableTarget(callType, CompilerHelpers.GetTypes(args));
            } else {
                throw MultipleTargets(targets, callType, CompilerHelpers.GetTypes(args));
            }
        }

        private List<MethodCandidate> FindTarget(CallType callType, object[] args) {
            return SelectTargets(callType, CompilerHelpers.GetTypes(args));
        }

        private List<MethodCandidate> SelectTargets(CallType callType, Type[] types) {
            return SelectTargets(callType, types, SymbolId.EmptySymbols);
        }

        private List<MethodCandidate> SelectTargets(CallType callType, Type[] types, SymbolId[] names) {
            if (_targets.Count == 1 && !_binder.IsBinaryOperator && names.Length == 0) return _targets;

            List<MethodCandidate> applicableTargets = new List<MethodCandidate>();
            foreach (MethodCandidate target in _targets) {
                if (target.IsApplicable(types, names, NarrowingLevel.None)) {
                    applicableTargets.Add(target);
                }
            }

            if (applicableTargets.Count == 1) {
                return applicableTargets;
            }
            if (applicableTargets.Count > 1) {
                MethodCandidate target = FindBest(callType, applicableTargets);
                if (target != null) {
                    return new List<MethodCandidate>(new MethodCandidate[] { target });
                } else {
                    return applicableTargets;
                }
            }

            //no targets are applicable without narrowing conversions, so try those

            foreach (MethodCandidate target in _targets) {
                if (target.IsApplicable(types, names, NarrowingLevel.Preferred)) {
                    applicableTargets.Add(new MethodCandidate(target, NarrowingLevel.Preferred));
                }
            }

            if (applicableTargets.Count == 0) {
                foreach (MethodCandidate target in _targets) {
                    NarrowingLevel nl = _binder.IsBinaryOperator ? NarrowingLevel.Operator : NarrowingLevel.All;
                    if (target.IsApplicable(types, names, nl)) {
                        applicableTargets.Add(new MethodCandidate(target, nl));
                    }
                }
            }

            return applicableTargets;
        }
       
        private Type[] GetTypesForTest(MethodCandidate target, Type[] types, CallType callType, IList<MethodCandidate> candidates) {
            // if we have a single target we need no tests.
            // if we have a binary operator we have to test to return NotImplemented
            if (_targets.Count == 1 && !_binder.IsBinaryOperator) return null;

            Type[] tests = new Type[types.Length];
            for (int i = 0; i < types.Length; i++) {
                if (_binder.IsBinaryOperator || AreArgumentTypesOverloaded(types, i, candidates)) {
                    tests[i] = types[i];
                }                
            }
                                  
            return tests;
        }

        private static bool AreArgumentTypesOverloaded(Type[] types, int index, IList<MethodCandidate> methods) {
            Type argType = null;
            for (int i = 0; i < methods.Count; i++) {
                IList<ParameterWrapper> pis = methods[i].Parameters;
                if (pis.Count == 0) continue;

                int readIndex = index;
                if (pis[0].Type == typeof(CodeContext)) {
                    readIndex++;
                }

                Type curType;
                if (readIndex < pis.Count) {
                    if (readIndex == -1) {
                        curType = methods[i].Target.Method.DeclaringType;
                    } else if (pis[readIndex].IsParameterArray) {
                        if (index == types.Length - 1) {
                            return true;    // TODO: Optimize this case
                        }
                        curType = pis[pis.Count - 1].Type.GetElementType();
                    } else {
                        curType = pis[readIndex].Type;
                    }
                } else if (pis[pis.Count - 1].IsParameterArray) {
                    curType = pis[pis.Count - 1].Type.GetElementType();
                } else {
                    continue;
                }

                if (argType == null) {
                    argType = curType;
                } else if (argType != curType) {
                    return true;
                }
            }
            return false;
        }

        private static bool IsBest(MethodCandidate candidate, List<MethodCandidate> applicableTargets, CallType callType) {
            foreach (MethodCandidate target in applicableTargets) {
                if (candidate == target) continue;
                if (candidate.CompareTo(target, callType) != +1) return false;
            }
            return true;
        }

        private static MethodCandidate FindBest(CallType callType, List<MethodCandidate> applicableTargets) {
            foreach (MethodCandidate candidate in applicableTargets) {
                if (IsBest(candidate, applicableTargets, callType)) return candidate;
            }
            return null;
        }

        private Exception NoApplicableTarget(CallType callType, Type[] types) {
            return new ArgumentTypeException(NoApplicableTargetMessage(callType, types));
        }

        private Exception MultipleTargets(List<MethodCandidate> applicableTargets, CallType callType, Type[] types) {
            return new ArgumentTypeException(MultipleTargetsMessage(applicableTargets, callType, types));
        }

        private string NoApplicableTargetMessage(CallType callType, Type[] types) {
            return TypeErrorForOverloads("no overloads of {0} could match {1}", _targets, callType, types);
        }

        private string MultipleTargetsMessage(List<MethodCandidate> applicableTargets, CallType callType, Type[] types) {
            return TypeErrorForOverloads("multiple overloads of {0} could match {1}", applicableTargets, callType, types);
        }

        private static string GetArgTypeNames(Type[] types, CallType callType) {
            StringBuilder buf = new StringBuilder();
            buf.Append("(");
            bool isFirstArg = true;
            int i = 0;
            if (callType == CallType.ImplicitInstance) i = 1;
            for (; i < types.Length; i++) {
                if (isFirstArg) isFirstArg = false;
                else buf.Append(", ");
                buf.Append(types[i].Name);
            }
            buf.Append(")");
            return buf.ToString();
        }

        private string TypeErrorForOverloads(string message, List<MethodCandidate> targets, CallType callType, Type[] types) {
            StringBuilder buf = new StringBuilder();
            buf.AppendFormat(message, _binder._name, GetArgTypeNames(types, callType));
            buf.AppendLine();
            foreach (MethodCandidate target in targets) {
                buf.Append("  ");
                buf.AppendLine(target.ToSignatureString(_binder._name, callType));
            }
            return buf.ToString();
        }

        public void Add(MethodCandidate target) {            
            _targets.Add(target);
        }

        public override string ToString() {
            return string.Format("TargetSet({0} on {1}, nargs={2})", _targets[0].Target.Method.Name, _targets[0].Target.Method.DeclaringType.FullName, _count);
        }
    }
}
