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

using System; using Microsoft;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Contracts;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    using Ast = Microsoft.Linq.Expressions.Expression;

    /// <summary>
    /// MethodTarget represents how a method is bound to the arguments of the call-site
    /// 
    /// Contrast this with MethodCandidate which represents the logical view of the invocation of a method
    /// </summary>
    public sealed class MethodTarget {
        private OverloadResolver _methodBinder;
        private readonly MethodBase _method;
        private int _parameterCount;
        private IList<ArgBuilder> _argBuilders;
        private ArgBuilder _instanceBuilder;
        private ReturnBuilder _returnBuilder;

        internal MethodTarget(OverloadResolver binder, MethodBase method, int parameterCount, ArgBuilder instanceBuilder, IList<ArgBuilder> argBuilders, ReturnBuilder returnBuilder) {
            _methodBinder = binder;
            _method = method;
            _parameterCount = parameterCount;
            _instanceBuilder = instanceBuilder;
            _argBuilders = argBuilders;
            _returnBuilder = returnBuilder;

            //argBuilders.TrimExcess();
        }

        public OverloadResolver MethodBinder {
            get { return _methodBinder; }
        }

        public MethodBase Method {
            get { return _method; }
        }

        public int ParameterCount {
            get { return _parameterCount; }
        }

        public Type ReturnType {
            get {
                return _returnBuilder.ReturnType;
            }
        }

        public Type[] GetParameterTypes() {
            List<Type> res = new List<Type>(_argBuilders.Count);
            for (int i = 0; i < _argBuilders.Count; i++) {
                Type t = _argBuilders[i].Type;
                if (t != null) {
                    res.Add(t);
                }
            }

            return res.ToArray();
        }

        internal OptimizingCallDelegate MakeDelegate(RestrictionInfo restrictionInfo) {
            MethodInfo mi = Method as MethodInfo;
            if (mi == null) {
                return null;
            }

            Type declType = mi.GetBaseDefinition().DeclaringType;
            if (declType != null &&
                declType.Assembly == typeof(string).Assembly &&
                declType.IsSubclassOf(typeof(MemberInfo))) {
                // members of reflection are off limits via reflection in partial trust
                return null;
            }

            if (_returnBuilder.CountOutParams > 0) {
                return null;
            }

            // if we have a non-visible method see if we can find a better method which
            // will call the same thing but is visible.  If this fails we still bind anyway - it's
            // the callers responsibility to filter out non-visible methods.
            mi = CompilerHelpers.TryGetCallableMethod(mi);

            Func<object[], object>[] builders = new Func<object[],object>[_argBuilders.Count];
            bool[] hasBeenUsed = new bool[restrictionInfo.Objects.Length];

            for (int i = 0; i < _argBuilders.Count; i++) {
                var builder = _argBuilders[i].ToDelegate(_methodBinder, restrictionInfo.Objects, hasBeenUsed);
                if (builder == null) {
                    return null;
                }

                builders[i] = builder;
            }
            
            if (_instanceBuilder != null && !(_instanceBuilder is NullArgBuilder)) {
                return new Caller(mi, builders, _instanceBuilder.ToDelegate(_methodBinder, restrictionInfo.Objects, hasBeenUsed)).CallWithInstance;
            } else {
                return new Caller(mi, builders, null).Call;
            }
        }

        class Caller {
            private readonly Func<object[], object>[] _argBuilders;
            private readonly Func<object[], object> _instanceBuilder;
            private readonly MethodInfo _mi;
            private ReflectedCaller _caller;            
            private int _hitCount;

            public Caller(MethodInfo mi, Func<object[], object>[] argBuilders, Func<object[], object> instanceBuilder) {
                _mi = mi;
                _argBuilders = argBuilders;
                _instanceBuilder = instanceBuilder;                
            }

            public object Call(object[] args, out bool shouldOptimize) {
                shouldOptimize = TrackUsage(args);

                try {
                    if (_caller != null) {
                        return _caller.Invoke(GetArguments(args));
                    }
                    return _mi.Invoke(null, GetArguments(args));
                } catch (TargetInvocationException tie) {
                    ExceptionHelpers.UpdateForRethrow(tie.InnerException);
                    throw tie.InnerException;
                }
            }

            public object CallWithInstance(object[] args, out bool shouldOptimize) {
                shouldOptimize = TrackUsage(args);

                try {
                    if (_caller != null) {
                        return _caller.InvokeInstance(_instanceBuilder(args), GetArguments(args));
                    }

                    return _mi.Invoke(_instanceBuilder(args), GetArguments(args));
                } catch (TargetInvocationException tie) {
                    ExceptionHelpers.UpdateForRethrow(tie.InnerException);
                    throw tie.InnerException;
                }
            }

            private object[] GetArguments(object[] args) {
                object[] finalArgs = new object[_argBuilders.Length];
                for (int i = 0; i < finalArgs.Length; i++) {
                    finalArgs[i] = _argBuilders[i](args);
                }
                return finalArgs;
            }

            private bool TrackUsage(object[] args) {
                bool shouldOptimize;
                _hitCount++;
                shouldOptimize = false;

                bool forceCaller = false;
                if (_hitCount <= 100 && _caller == null) {
                    foreach (object o in args) {
                        // can't pass Missing.Value via reflection, use a ReflectedCaller
                        if (o == Missing.Value) {
                            forceCaller = true;
                        }
                    }
                }

                if (_hitCount > 100) {
                    shouldOptimize = true;
                } else if ((_hitCount > 5 || forceCaller) && _caller == null) {
                    _caller = ReflectedCaller.Create(_mi);
                }
                return shouldOptimize;
            }            
        }

        internal Expression MakeExpression(IList<Expression> args) {
            bool[] usageMarkers;
            Expression[] spilledArgs;
            Expression[] callArgs = GetArgumentExpressions(args, out usageMarkers, out spilledArgs);
            
            MethodBase mb = Method;
            MethodInfo mi = mb as MethodInfo;
            Expression ret, call;
            if (mi != null) {
                // if we have a non-visible method see if we can find a better method which
                // will call the same thing but is visible.  If this fails we still bind anyway - it's
                // the callers responsibility to filter out non-visible methods.
                mb = CompilerHelpers.TryGetCallableMethod(mi);
            }

            ConstructorInfo ci = mb as ConstructorInfo;
            Debug.Assert(mi != null || ci != null);
            if (CompilerHelpers.IsVisible(mb)) {
                // public method
                if (mi != null) {
                    Expression instance = mi.IsStatic ? null : _instanceBuilder.ToExpression(_methodBinder, args, usageMarkers);
                    call = AstUtils.SimpleCallHelper(instance, mi, callArgs);
                } else {
                    call = AstUtils.SimpleNewHelper(ci, callArgs);
                }
            } else {
                // Private binding, invoke via reflection
                if (mi != null) {
                    Expression instance = mi.IsStatic ? AstUtils.Constant(null) : _instanceBuilder.ToExpression(_methodBinder, args, usageMarkers);
                    Debug.Assert(instance != null, "Can't skip instance expression");

                    call = Ast.Call(
                        typeof(BinderOps).GetMethod("InvokeMethod"),
                        AstUtils.Constant(mi),
                        AstUtils.Convert(instance, typeof(object)),
                        AstUtils.NewArrayHelper(typeof(object), callArgs)
                    );
                } else {
                    call = Ast.Call(
                        typeof(BinderOps).GetMethod("InvokeConstructor"),
                        AstUtils.Constant(ci),
                        AstUtils.NewArrayHelper(typeof(object), callArgs)
                    );
                }
            }

            if (spilledArgs != null) {
                call = Expression.Block(spilledArgs.AddLast(call));
            }

            ret = _returnBuilder.ToExpression(_methodBinder, _argBuilders, args, call);

            List<Expression> updates = null;
            for (int i = 0; i < _argBuilders.Count; i++) {
                Expression next = _argBuilders[i].UpdateFromReturn(_methodBinder, args);
                if (next != null) {
                    if (updates == null) {
                        updates = new List<Expression>();
                    }
                    updates.Add(next);
                }
            }

            if (updates != null) {
                if (ret.Type != typeof(void)) {
                    ParameterExpression temp = Ast.Variable(ret.Type, "$ret");
                    updates.Insert(0, Ast.Assign(temp, ret));
                    updates.Add(temp);
                    ret = Ast.Block(new [] { temp }, updates.ToArray());
                } else {
                    updates.Insert(0, ret);
                    ret = Ast.Block(typeof(void), updates.ToArray());
                }
            }

            if (_methodBinder.Temps != null) {
                ret = Ast.Block(_methodBinder.Temps, ret);
            }

            return ret;
        }

        private Expression[] GetArgumentExpressions(IList<Expression> parameters, out bool[] usageMarkers, out Expression[] spilledArgs) {
            int minPriority = Int32.MaxValue;
            int maxPriority = Int32.MinValue;
            foreach (ArgBuilder ab in _argBuilders) {
                minPriority = System.Math.Min(minPriority, ab.Priority);
                maxPriority = System.Math.Max(maxPriority, ab.Priority);
            }

            var args = new Expression[_argBuilders.Count];
            Expression[] actualArgs = null;
            usageMarkers = new bool[parameters.Count];
            for (int priority = minPriority; priority <= maxPriority; priority++) {
                for (int i = 0; i < _argBuilders.Count; i++) {
                    if (_argBuilders[i].Priority == priority) {
                        args[i] = _argBuilders[i].ToExpression(_methodBinder, parameters, usageMarkers);

                        // see if this has a temp that needs to be passed as the actual argument
                        Expression byref = _argBuilders[i].ByRefArgument;
                        if (byref != null) {
                            if (actualArgs == null) {
                                actualArgs = new Expression[_argBuilders.Count];
                            }
                            actualArgs[i] = byref;
                        }
                    }
                }
            }
            
            if (actualArgs != null) {                
                for (int i = 0; i < args.Length;  i++) {
                    if (args[i] != null && actualArgs[i] == null) {
                        actualArgs[i] = _methodBinder.GetTemporary(args[i].Type, null);
                        args[i] = Expression.Assign(actualArgs[i], args[i]);
                    }
                }

                spilledArgs = RemoveNulls(args);
                return RemoveNulls(actualArgs);
            }
            
            spilledArgs = null;
            return RemoveNulls(args);
        }

        private static Expression[] RemoveNulls(Expression[] args) {
            int newLength = args.Length;
            for (int i = 0; i < args.Length; i++) {
                if (args[i] == null) {
                    newLength--;
                }
            }
            
            var result = new Expression[newLength];
            for (int i = 0, j = 0; i < args.Length; i++) {
                if (args[i] != null) {
                    result[j++] = args[i];
                }
            }
            return result;
        }

        private static int FindMaxPriority(IList<ArgBuilder> abs, int ceiling) {
            int max = 0;
            foreach (ArgBuilder ab in abs) {
                if (ab.Priority > ceiling) continue;

                max = System.Math.Max(max, ab.Priority);
            }
            return max;
        }

        internal static Candidate CompareEquivalentParameters(MethodTarget one, MethodTarget two) {
            // Prefer normal methods over explicit interface implementations
            if (two.Method.IsPrivate && !one.Method.IsPrivate) return Candidate.One;
            if (one.Method.IsPrivate && !two.Method.IsPrivate) return Candidate.Two;

            // Prefer non-generic methods over generic methods
            if (one.Method.IsGenericMethod) {
                if (!two.Method.IsGenericMethod) {
                    return Candidate.Two;
                } else {
                    //!!! Need to support selecting least generic method here
                    return Candidate.Equivalent;
                }
            } else if (two.Method.IsGenericMethod) {
                return Candidate.One;
            }

            //prefer methods without out params over those with them
            switch (Compare(one._returnBuilder.CountOutParams, two._returnBuilder.CountOutParams)) {
                case 1: return Candidate.Two;
                case -1: return Candidate.One;
            }

            //prefer methods using earlier conversions rules to later ones            
            for (int i = Int32.MaxValue; i >= 0; ) {
                int maxPriorityThis = FindMaxPriority(one._argBuilders, i);
                int maxPriorityOther = FindMaxPriority(two._argBuilders, i);

                if (maxPriorityThis < maxPriorityOther) return Candidate.One;
                if (maxPriorityOther < maxPriorityThis) return Candidate.Two;

                i = maxPriorityThis - 1;
            }

            return Candidate.Equivalent;
        }

        private static int Compare(int x, int y) {
            if (x < y) return -1;
            else if (x > y) return +1;
            else return 0;
        }

        internal MethodTarget MakeParamsExtended(int argCount, string[] names, int[] nameIndexes) {
            Debug.Assert(BinderHelpers.IsParamsMethod(Method));

            List<ArgBuilder> newArgBuilders = new List<ArgBuilder>(_argBuilders.Count);

            // current argument that we consume, initially skip this if we have it.
            int curArg = CompilerHelpers.IsStatic(_method) ? 0 : 1;
            int kwIndex = -1;
            ArgBuilder paramsDictBuilder = null;

            foreach (ArgBuilder ab in _argBuilders) {
                // TODO: define a virtual method on ArgBuilder implementing this functionality:

                SimpleArgBuilder sab = ab as SimpleArgBuilder;
                if (sab != null) {
                    // we consume one or more incoming argument(s)
                    if (sab.IsParamsArray) {
                        // consume all the extra arguments
                        int paramsUsed = argCount -
                            GetConsumedArguments() -
                            names.Length +
                            (CompilerHelpers.IsStatic(_method) ? 1 : 0);

                        newArgBuilders.Add(new ParamsArgBuilder(
                            sab.ParameterInfo,
                            sab.Type.GetElementType(),
                            curArg,
                            paramsUsed
                        ));

                        curArg += paramsUsed;
                    } else if (sab.IsParamsDict) {
                        // consume all the kw arguments
                        kwIndex = newArgBuilders.Count;
                        paramsDictBuilder = sab;
                    } else {
                        // consume the argument, adjust its position:
                        newArgBuilders.Add(sab.MakeCopy(curArg++));
                    }
                } else if (ab is KeywordArgBuilder) {
                    newArgBuilders.Add(ab);
                    curArg++;
                } else {
                    // CodeContext, null, default, etc...  we don't consume an 
                    // actual incoming argument.
                    newArgBuilders.Add(ab);
                }
            }

            if (kwIndex != -1) {
                newArgBuilders.Insert(kwIndex, new ParamsDictArgBuilder(paramsDictBuilder.ParameterInfo, curArg, names, nameIndexes));
            }

            return new MethodTarget(_methodBinder, Method, argCount, _instanceBuilder, newArgBuilders, _returnBuilder);
        }

        private int GetConsumedArguments() {
            int consuming = 0;
            foreach (ArgBuilder argb in _argBuilders) {
                SimpleArgBuilder sab = argb as SimpleArgBuilder;
                if (sab != null && !sab.IsParamsDict || argb is KeywordArgBuilder) {
                    consuming++;
                }
            }
            return consuming;
        }

        [Confined]
        public override string ToString() {
            return string.Format("MethodTarget({0} on {1})", Method, Method.DeclaringType.FullName);
        }
    }

    public delegate object OptimizingCallDelegate(object[]args, out bool shouldOptimize);
}
