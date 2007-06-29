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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Generation {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class MethodTarget  {
        private ActionBinder _binder;
        private MethodBase _method;
        private int _parameterCount;
        private IList<ArgBuilder> _argBuilders;
        private ArgBuilder _instanceBuilder;
        private ReturnBuilder _returnBuilder;

        public MethodTarget(ActionBinder binder, MethodBase method, int parameterCount, ArgBuilder instanceBuilder, IList<ArgBuilder> argBuilders, ReturnBuilder returnBuilder) {
            this._binder = binder;
            this._method = method;
            this._parameterCount = parameterCount;
            this._instanceBuilder = instanceBuilder;
            this._argBuilders = argBuilders;
            this._returnBuilder = returnBuilder;

            //argBuilders.TrimExcess();
        }

        public MethodBase Method {
            get { return _method; }
            set { _method = value; }
        }

        public bool NeedsContext {
            get { return _argBuilders.Count > 0 && _argBuilders[0].NeedsContext; }
        }

        public int ParameterCount {
            get { return _parameterCount; }
        }

        public bool CheckArgs(CodeContext context, object[] args) {
            //if (!instanceBuilder.Check(context, args)) return false;
            //foreach (ArgBuilder arg in argBuilders) {
            //    if (!arg.Check(context, args)) return false;
            //}
            //return true;
            try {
                _instanceBuilder.Build(context, args);
                for (int i = 0; i < _argBuilders.Count; i++) {
                    _argBuilders[i].Build(context, args);
                }
                return true;
            } catch (OverflowException) {
                return false;
            } catch (ArgumentTypeException) {
                return false;
            }
        }


        public object CallReflected(CodeContext context, object[] args) {
            if (ScriptDomainManager.Options.EngineDebug) {
                PerfTrack.NoteEvent(PerfTrack.Categories.Methods, this);
            }

            object instance = _instanceBuilder.Build(context, args);
            object[] callArgs = new object[_argBuilders.Count];
            for (int i = 0; i < callArgs.Length; i++) {
                callArgs[i] = _argBuilders[i].Build(context, args);
            }

            object result;
            try {
                if (Method is ConstructorInfo) {
                    result = ((ConstructorInfo)Method).Invoke(callArgs);
                } else {
                    result = Method.Invoke(instance, callArgs);
                }
#if SILVERLIGHT && DEBUG // TODO: drop when Silverlight gets fixed
            } catch (System.Security.SecurityException) {
                throw new System.Security.SecurityException(String.Format("Access to method '{0}' denied.", 
                    Utils.Reflection.FormatSignature(Method)));
#endif
            } catch (TargetInvocationException tie) {
                throw ExceptionHelpers.UpdateForRethrow(tie.InnerException);
            }

            //This is only used to support explicit Reference arguments
            for (int i = 0; i < callArgs.Length; i++) {
                _argBuilders[i].UpdateFromReturn(callArgs[i], args);
            }

            return _returnBuilder.Build(context, callArgs, result);
        }

        public bool CanMakeCallTarget() {
            if (!CompilerHelpers.CanOptimizeMethod(Method)) return false;

            if (ParameterCount > CallTargets.MaximumCallArgs) {
                return false;
            }

            if (!_returnBuilder.CanGenerate) return false;

            foreach (ArgBuilder ab in _argBuilders) {
                if (!ab.CanGenerate) return false;
            }
            return true;
        }

        public Expression MakeExpression(ActionBinder binder, Expression[] parameters) {
            Expression[] args = new Expression[_argBuilders.Count];
            for (int i = 0; i < _argBuilders.Count; i++) {
                args[i] = _argBuilders[i].ToExpression(binder, parameters);
            }

            MethodInfo mi = Method as MethodInfo;
            if (mi != null) {
                Expression instance = mi.IsStatic ? null : _instanceBuilder.ToExpression(binder, parameters);
                return Ast.Call(instance, mi, args);
            } else {
                return Ast.New((ConstructorInfo)Method, args);
            }
        }

        public AbstractValue AbstractCall(AbstractContext context, IList<AbstractValue> args) {
            AbstractValue[] callArgs = new AbstractValue[_argBuilders.Count];
            for (int i = 0; i < _argBuilders.Count; i++) {
                callArgs[i] = _argBuilders[i].AbstractBuild(context, args);
            }

            Expression[] argExprs = new Expression[callArgs.Length];
            for (int i = 0; i < callArgs.Length; i++) {
                Expression expr = callArgs[i].Expression;
                if (expr == null) {
                    argExprs = null;
                    break;
                } else {
                    argExprs[i] = expr;
                }
            }

            Expression callExpr = null;
            if (argExprs != null) {
                MethodInfo mi = Method as MethodInfo;
                if (mi != null) {
                    Expression instance = mi.IsStatic ? null : _instanceBuilder.AbstractBuild(context, args).Expression;
                    callExpr = Ast.Call(instance, mi, argExprs);
                } else {
                    callExpr = Ast.New((ConstructorInfo)Method, argExprs);
                }
            }

            return AbstractValue.LimitType(this.ReturnType, callExpr);
        }

        private static int FindMaxPriority(IList<ArgBuilder> abs) {
            int max = -1;
            foreach (ArgBuilder ab in abs) {
                max = System.Math.Max(max, ab.Priority);
            }
            return max;
        }

        public int CompareEqualParameters(MethodTarget other) {
            // Prefer normal methods over explicit interface implementations
            if (other.Method.IsPrivate && !this.Method.IsPrivate) return +1;
            if (this.Method.IsPrivate && !other.Method.IsPrivate) return -1;

            // Prefer non-generic methods over generic methods
            if (Method.IsGenericMethod) {
                if (!other.Method.IsGenericMethod) {
                    return -1;
                } else {
                    //!!! Need to support selecting least generic method here
                    return 0;
                }
            } else if (other.Method.IsGenericMethod) {
                return +1;
            }

            //prefer methods without out params over those with them
            switch (Compare(_returnBuilder.CountOutParams, other._returnBuilder.CountOutParams)) {
                case 1: return -1;
                case -1: return 1;
            }

            //prefer methods using earlier conversions rules to later ones
            int maxPriorityThis = FindMaxPriority(this._argBuilders);
            int maxPriorityOther = FindMaxPriority(other._argBuilders);

            if (maxPriorityThis < maxPriorityOther) return +1;
            if (maxPriorityOther < maxPriorityThis) return -1;

            return 0;
        }

        protected static int Compare(int x, int y) {
            if (x < y) return -1;
            else if (x > y) return +1;
            else return 0;
        }

        public override string ToString() {
            return string.Format("MethodTarget({0} on {1})", Method, Method.DeclaringType.FullName);
        }

        public Type ReturnType {
            get {
                return _returnBuilder.ReturnType;
            }
        }

        public MethodTarget MakeParamsExtended(int argCount) {
            Debug.Assert(CompilerHelpers.IsParamsMethod(Method));

            if (argCount < ParameterCount - 1) return null;

            List<ArgBuilder> newArgBuilders = new List<ArgBuilder>(_argBuilders);

            Type elementType = ((SimpleArgBuilder)_argBuilders[_argBuilders.Count - 1]).Type.GetElementType();
            int start = ParameterCount - 1;
            newArgBuilders[newArgBuilders.Count-1] = new ParamsArgBuilder(start, argCount - start, elementType);

            return new MethodTarget(_binder, Method, argCount, _instanceBuilder, newArgBuilders, _returnBuilder);
        }
    }
}
