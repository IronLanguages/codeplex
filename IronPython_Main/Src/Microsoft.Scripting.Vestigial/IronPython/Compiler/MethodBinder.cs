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
    public enum BinderType {
        Normal,
        BinaryOperator,
        ComparisonOperator
    }

    public class MethodBinder {
        internal string _name;
        private BinderType _binderType;
        internal Dictionary<int, TargetSet> _targetSets = new Dictionary<int, TargetSet>();
        internal List<ParamsMethodMaker> _paramsMakers = new List<ParamsMethodMaker>();
        internal ActionBinder _binder;

        public ActionBinder ActionBinder {
            get { return _binder; }
        }

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

        public static FastCallable MakeFastCallable(ActionBinder binder, string name, MethodInfo mi, BinderType binderType) {
            //??? In the future add optimization for simple case of nothing tricky in mi
            return new MethodBinder(binder, name, new MethodBase[] { mi }, binderType).MakeFastCallable();
        }

        public static FastCallable MakeFastCallable(ActionBinder binder, string name, MethodBase[] mis, BinderType binderType) {
            return new MethodBinder(binder, name, mis, binderType).MakeFastCallable();
        }

        public static MethodBinder MakeBinder(ActionBinder binder, string name, MethodBase[] mis, BinderType binderType) {
            return new MethodBinder(binder, name, mis, binderType);
        }

        public MethodCandidate MakeBindingTarget(CallType callType, DynamicType[] types) {
            TargetSet ts = this.GetTargetSet(types.Length);
            if (ts != null) {
                return ts.MakeBindingTarget(callType, types);
            }
            return null;
        }
        
        //public StandardRule<T> MakeBindingRule<T>(CallType callType, DynamicType[] types) {
        //    TargetSet ts = this.GetTargetSet(types.Length);
        //    if (ts != null) {
        //        return ts.MakeBindingRule<T>(callType, types);
        //    } else {
        //        string message = BadArgumentCount(callType, types.Length).Message;
        //        return StandardRule<T>.TypeError(message, types);
        //    }
        //}


        private MethodBinder(ActionBinder binder,string name, MethodBase[] methods, BinderType binderType) {
            this._binder = binder;
            this._name = name;
            _binderType = binderType;
            foreach (MethodBase method in methods) {
                if (IsUnsupported(method)) continue;
                if (methods.Length > 1 && IsKwDictMethod(method)) continue;
                AddBasicMethodTargets(method);
            }

            foreach (ParamsMethodMaker maker in _paramsMakers) {
                foreach (int count in _targetSets.Keys) {
                    MethodCandidate target = maker.MakeTarget(binder, count);
                    if (target != null) AddTarget(target);
                }
            }
        }

        public string Name { get { return _name; } }
        public int MaximumArgs {
            get {
                int minArgs, maxArgs;
                GetMinAndMaxArgs(out minArgs, out maxArgs);
                return maxArgs;
            }
        }
        public int MinimumArgs { get { int minArgs, maxArgs; GetMinAndMaxArgs(out minArgs, out maxArgs); return minArgs; } }

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

        private Delegate MakeFastCallable(bool needsContext, int nargs) {
            TargetSet ts = GetTargetSet(nargs);
            if (ts == null) return null;

            return ts.MakeCallTarget(needsContext);
        }

        public object CallWithContextN(CodeContext context, object[] args) {
            return Call(context, CallType.None, args);
        }

        public object CallN(object[] args) {
            return Call(null, CallType.None, args);
        }

        private Delegate MakeFastCallableN(bool needsContext) {
            int minArgs, maxArgs;
            GetMinAndMaxArgs(out minArgs, out maxArgs);

            if (maxArgs <= 5 && _paramsMakers.Count == 0) return null;
            if (needsContext) return new CallTargetWithContextN(this.CallWithContextN);
            else return new CallTargetN(this.CallN);
        }

        private void GetMinAndMaxArgs(out int minArgs, out int maxArgs) {
            List<int> argCounts = new List<int>(_targetSets.Keys);
            argCounts.Sort();
            minArgs = argCounts[0];
            maxArgs = argCounts[argCounts.Count - 1];
        }

        public FastCallable MakeFastCallable() {
            bool needsContext = false;
            // If we have any instance/static conflicts then we'll use the slow path for everything
            foreach (TargetSet ts in _targetSets.Values) {
                if (ts.HasConflict) return new FastCallableUgly(this);
                if (ts.NeedsContext) needsContext = true;
            }

            if (_targetSets.Count == 0) return new FastCallableUgly(this);

            if (_targetSets.Count == 1 && _paramsMakers.Count == 0) {
                TargetSet ts = new List<TargetSet>(_targetSets.Values)[0];
                if (ts._count <= CallTargets.MaximumCallArgs) return ts.MakeFastCallable();
            }

            int minArgs, maxArgs;
            GetMinAndMaxArgs(out minArgs, out maxArgs);
            if (needsContext) {
                FastCallableWithContextAny ret = new FastCallableWithContextAny(_name, minArgs, maxArgs);
                ret.target0 = (CallTargetWithContext0)MakeFastCallable(needsContext, 0);
                ret.target1 = (CallTargetWithContext1)MakeFastCallable(needsContext, 1);
                ret.target2 = (CallTargetWithContext2)MakeFastCallable(needsContext, 2);
                ret.target3 = (CallTargetWithContext3)MakeFastCallable(needsContext, 3);
                ret.target4 = (CallTargetWithContext4)MakeFastCallable(needsContext, 4);
                ret.target5 = (CallTargetWithContext5)MakeFastCallable(needsContext, 5);
                ret.targetN = (CallTargetWithContextN)MakeFastCallableN(needsContext);
                return ret;
            } else {
                FastCallableAny ret = new FastCallableAny(_name, minArgs, maxArgs);
                ret.target0 = (CallTarget0)MakeFastCallable(needsContext, 0);
                ret.target1 = (CallTarget1)MakeFastCallable(needsContext, 1);
                ret.target2 = (CallTarget2)MakeFastCallable(needsContext, 2);
                ret.target3 = (CallTarget3)MakeFastCallable(needsContext, 3);
                ret.target4 = (CallTarget4)MakeFastCallable(needsContext, 4);
                ret.target5 = (CallTarget5)MakeFastCallable(needsContext, 5);
                ret.targetN = (CallTargetN)MakeFastCallableN(needsContext);
                return ret;
            }
        }

        private Exception BadArgumentCount(CallType callType, int argCount) {
            if (_targetSets.Count == 0) return Ops.TypeError("no callable targets, if this is a generic method make sure specify the type parameters");
            int minArgs, maxArgs;
            GetMinAndMaxArgs(out minArgs, out maxArgs);

            if (callType == CallType.ImplicitInstance) {
                argCount -= 1;
                minArgs -= 1;
                maxArgs -= 1;
            }

            // This generates Python style error messages assuming that all arg counts in between min and max are allowed
            //It's possible that discontinuous sets of arg counts will produce a weird error message
            return RuntimeHelpers.TypeErrorForIncorrectArgumentCount(_name, maxArgs, maxArgs - minArgs, argCount);
        }


        private TargetSet BuildTargetSet(int count) {
            TargetSet ts = new TargetSet(this, count);
            foreach (ParamsMethodMaker maker in _paramsMakers) {
                MethodCandidate target = maker.MakeTarget(_binder, count);
                if (target != null) ts.Add(target);
            }
            return ts;
        }

        private TargetSet GetTargetSet(int nargs) {
            TargetSet ts;
            if (_targetSets.TryGetValue(nargs, out ts)) {
                return ts;
            } else if (_paramsMakers.Count > 0) {
                ts = BuildTargetSet(nargs);
                if (ts._targets.Count > 0) {
                    return ts;
                }
            }
            return null;
        }

        public object Call(CodeContext context, CallType callType, object[] args) {
            TargetSet ts = GetTargetSet(args.Length);
            if (ts != null) return ts.Call(context, callType, args);
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
                    ParameterWrapper param = new ParameterWrapper(_binder, refType, true);
                    parameters.Add(param);
                    argBuilders.Add(new ReferenceArgBuilder(argIndex++, param.Type));
                } else {
                    ParameterWrapper param = new ParameterWrapper(_binder, pi);
                    parameters.Add(param);
                    argBuilders.Add(new SimpleArgBuilder(argIndex++, param.Type));
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

        private static MethodCandidate MakeMethodCandidate(ActionBinder binder,MethodBase method, List<ParameterWrapper> parameters, ArgBuilder instanceBuilder, List<ArgBuilder> argBuilders, ReturnBuilder returnBuilder) {
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
                        ParameterWrapper param = new ParameterWrapper(_binder, pi.ParameterType.GetElementType());
                        parameters.Add(param);
                        argBuilders.Add(new SimpleArgBuilder(argIndex++, pi.ParameterType.GetElementType()));
                    }
                } else {
                    ParameterWrapper param = new ParameterWrapper(_binder, pi);
                    parameters.Add(param);
                    argBuilders.Add(new SimpleArgBuilder(argIndex++, param.Type));
                }
            }

            ReturnBuilder returnBuilder = new ByRefReturnBuilder(CompilerHelpers.GetReturnType(method), returnArgs);

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

    public class ParamsMethodMaker {
        private MethodCandidate baseTarget;

        public ParamsMethodMaker(MethodCandidate baseTarget) {
            this.baseTarget = baseTarget;
        }

        public MethodCandidate MakeTarget(ActionBinder binder, int count) {
            return baseTarget.MakeParamsExtended(binder, count);
        }
    }

    public class ByRefReturnBuilder : ReturnBuilder {
        private IList<int> returnArgs;
        public ByRefReturnBuilder(Type returnType, IList<int> returnArgs)
            : base(returnType) {
            this.returnArgs = returnArgs;
        }

        protected static object GetValue(object[] args, object ret, int index) {
            if (index == -1) return ret;
            return args[index];
        }

        public override object Build(CodeContext context, object[] args, object ret) {
            if (returnArgs.Count == 1) {
                return GetValue(args, ret, returnArgs[0]);
            } else {
                object[] retValues = new object[returnArgs.Count];
                int rIndex = 0;
                foreach (int index in returnArgs) {
                    retValues[rIndex++] = GetValue(args, ret, index);
                }
                return Tuple.MakeTuple(retValues);
            }
        }

        public override int CountOutParams {
            get { return returnArgs.Count; }
        }

        public override bool CanGenerate {
            get {
                return false;
            }
        }

        public override void Generate(CodeGen cg, IList<Slot> argSlots) {
            throw new NotImplementedException();
        }
    }
    
    public class TargetSet {
        private MethodBinder _binder;
        internal int _count;
        internal List<MethodCandidate> _targets;
        private bool _hasConflict = false;

        public TargetSet(MethodBinder binder, int count) {
            this._binder = binder;
            this._count = count;
            _targets = new List<MethodCandidate>();
        }


        public bool HasConflict {
            get { return _hasConflict || _binder.IsBinaryOperator; }
        }

        public bool NeedsContext {
            get { return _targets.Count != 1 || _targets[0].Target.NeedsContext || !_targets[0].Target.CanMakeCallTarget(); }
        }

        public MethodCandidate MakeBindingTarget(CallType callType, DynamicType[] types) {
            List<MethodCandidate> targets = SelectTargets(callType, types);

            if (targets.Count == 1) return targets[0];

            return null;
        }

        //public StandardRule<T> MakeBindingRule<T>(CallType callType, DynamicType[] types) {
        //    List<MethodCandidate> targets = SelectTargets(callType, types);
        //    if (targets.Count == 0) {
        //        return StandardRule<T>.TypeError(NoApplicableTargetMessage(callType, types), types);
        //    }
        //    if (targets.Count > 1) {
        //        return StandardRule<T>.TypeError(MultipleTargetsMessage(targets, callType, types), types);
        //    }

        //    return StandardRule<T>.Simple(targets[0].Target, types);
        //}

        public FastCallable MakeFastCallable() {
            return FastCallable.Make(_binder._name, NeedsContext, _count, MakeCallTarget(NeedsContext));
        }

        public Delegate MakeCallTarget(bool needsContext) {
            if (_targets.Count == 1) {
                Delegate ret = _targets[0].Target.MakeCallTarget(needsContext);
                if (ret != null) return ret;
            }

            switch (_count) {
                case 0:
                    return new CallTargetWithContext0(Call0);
                case 1:
                    return new CallTargetWithContext1(Call1);
                case 2:
                    return new CallTargetWithContext2(Call2);
                case 3:
                    return new CallTargetWithContext3(Call3);
                case 4:
                    return new CallTargetWithContext4(Call4);
                case 5:
                    return new CallTargetWithContext5(Call5);
                default:
                    return null;
            }
        }


        public object Call0(CodeContext context) {
            return Call(context, CallType.None, new object[] { });
        }
        public object Call1(CodeContext context, object arg0) {
            return Call(context, CallType.None, new object[] { arg0 });
        }
        public object Call2(CodeContext context, object arg0, object arg1) {
            return Call(context, CallType.None, new object[] { arg0, arg1 });
        }
        public object Call3(CodeContext context, object arg0, object arg1, object arg2) {
            return Call(context, CallType.None, new object[] { arg0, arg1, arg2 });
        }
        public object Call4(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            return Call(context, CallType.None, new object[] { arg0, arg1, arg2, arg3 });
        }
        public object Call5(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return Call(context, CallType.None, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }
        public object CallN(CodeContext context, object[] args) {
            return Call(context, CallType.None, args);
        }

        public object Call(CodeContext context, CallType callType, object[] args) {
            DynamicType[] types = new DynamicType[args.Length];
            for (int i=0; i < types.Length; i++) {
                types[i] = Ops.GetDynamicType(args[i]);
            }
            List<MethodCandidate> targets = SelectTargets(callType, types);

            if (targets.Count == 1) {
                if (_binder.IsBinaryOperator) {
                    if (!targets[0].CheckArgs(context, args)) {
                        return Ops.NotImplemented;
                    }
                }
                return targets[0].Target.Call(context, args);
            }

            if (_binder.IsBinaryOperator) {
                return Ops.NotImplemented;
            }

            if (targets.Count == 0) {
                throw NoApplicableTarget(callType, types);
            } else {
                throw MultipleTargets(targets, callType, types);
            }
        }

        public List<MethodCandidate> SelectTargets(CallType callType, DynamicType[] types) {
            if (_targets.Count == 1 && !_binder.IsBinaryOperator) return _targets;

            List<MethodCandidate> applicableTargets = new List<MethodCandidate>();
            foreach (MethodCandidate target in _targets) {
                if (target.IsApplicable(types, NarrowingLevel.None)) {
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
                if (target.IsApplicable(types, NarrowingLevel.Preferred)) {
                    applicableTargets.Add(new MethodCandidate(target, NarrowingLevel.Preferred));
                }
            }

            if (applicableTargets.Count == 0) {
                foreach (MethodCandidate target in _targets) {
                    NarrowingLevel nl = _binder.IsBinaryOperator ? NarrowingLevel.Operator : NarrowingLevel.All;
                    if (target.IsApplicable(types, nl)) {
                        applicableTargets.Add(new MethodCandidate(target, nl));
                    }
                }
            }

            return applicableTargets;

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

        private Exception NoApplicableTarget(CallType callType, DynamicType[] types) {
            return Ops.TypeError(NoApplicableTargetMessage(callType, types));
        }

        private Exception MultipleTargets(List<MethodCandidate> applicableTargets, CallType callType, DynamicType[] types) {
            return Ops.TypeError(MultipleTargetsMessage(applicableTargets, callType, types));
        }

        private string NoApplicableTargetMessage(CallType callType, DynamicType[] types) {
            return TypeErrorForOverloads("no overloads of {0} could match {1}", _targets, callType, types);
        }

        private string MultipleTargetsMessage(List<MethodCandidate> applicableTargets, CallType callType, DynamicType[] types) {
            return TypeErrorForOverloads("multiple overloads of {0} could match {1}", applicableTargets, callType, types);
        }

        private static string GetArgTypeNames(DynamicType[] types, CallType callType) {
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

        private string TypeErrorForOverloads(string message, List<MethodCandidate> targets, CallType callType, DynamicType[] types) {
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
            for (int i = 0; i < _targets.Count; i++) {
                if (_targets[i].CompareParameters(target) == 0) {
                    switch (_targets[i].Target.CompareEqualParameters(target.Target)) {
                        case -1:
                            // the new method is strictly better than the existing one so remove the existing one
                            _targets.RemoveAt(i);
                            i -= 1; // modify the index since we removed a target from the list
                            break;
                        case +1:
                            // the new method is strictly worse than the existing one so skip it
                            return;
                        case 0:
                            // the two methods are identical ignoring CallType so list a conflict
                            _hasConflict = true;
                            break;
                    }
                }
            }
            _targets.Add(target);
        }

        public override string ToString() {
            return string.Format("TargetSet({0} on {1}, nargs={2})", _targets[0].Target.Method.Name, _targets[0].Target.Method.DeclaringType.FullName, _count);
        }
    }

    class FastCallableUgly : FastCallable {
        private MethodBinder binder;

        internal FastCallableUgly(MethodBinder binder) {
            this.binder = binder;
        }

        public override object Call(CodeContext context) {
            return binder.Call(context, CallType.None, new object[] { });
        }
        public override object Call(CodeContext context, object arg0) {
            return binder.Call(context, CallType.None, new object[] { arg0 });
        }
        public override object Call(CodeContext context, object arg0, object arg1) {
            return binder.Call(context, CallType.None, new object[] { arg0, arg1 });
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2) {
            return binder.Call(context, CallType.None, new object[] { arg0, arg1, arg2 });
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            return binder.Call(context, CallType.None, new object[] { arg0, arg1, arg2, arg3 });
        }
        public override object Call(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return binder.Call(context, CallType.None, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }

        public override object Call(CodeContext context, params object[] args) {
            return binder.Call(context, CallType.None, args);
        }

        public override object CallInstance(CodeContext context, object arg0) {
            return binder.Call(context, CallType.ImplicitInstance, new object[] { arg0 });
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1) {
            return binder.Call(context, CallType.ImplicitInstance, new object[] { arg0, arg1 });
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2) {
            return binder.Call(context, CallType.ImplicitInstance, new object[] { arg0, arg1, arg2 });
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2, object arg3) {
            return binder.Call(context, CallType.ImplicitInstance, new object[] { arg0, arg1, arg2, arg3 });
        }
        public override object CallInstance(CodeContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
            return binder.Call(context, CallType.ImplicitInstance, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }

        public override object CallInstance(CodeContext context, object instance, params object[] args) {
            object[] nargs = new object[args.Length + 1];
            nargs[0] = instance;
            args.CopyTo(nargs, 1);
            args = nargs;
            return binder.Call(context, CallType.ImplicitInstance, args);
        }
    }
}
