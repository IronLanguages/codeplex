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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// MethodTarget represents how a method is bound to the arguments of the call-site
    /// 
    /// Contrast this with MethodCandidate which represents the logical view of the invocation of a method
    /// </summary>
    public sealed class MethodTarget {
        private MethodBinder _binder;
        private readonly MethodBase _method;
        private int _parameterCount;
        private IList<ArgBuilder> _argBuilders;
        private ArgBuilder _instanceBuilder;
        private ReturnBuilder _returnBuilder;

        internal MethodTarget(MethodBinder binder, MethodBase method, int parameterCount, ArgBuilder instanceBuilder, IList<ArgBuilder> argBuilders, ReturnBuilder returnBuilder) {
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

        public string GetSignatureString(CallTypes callType) {
            StringBuilder buf = new StringBuilder();
            Type[] types = GetParameterTypes();
            if (callType == CallTypes.ImplicitInstance) {
                types = ArrayUtils.RemoveFirst(types);
            }

            string comma = "";
            buf.Append("(");
            foreach (Type t in types) {
                buf.Append(comma);
                buf.Append(t.Name);
                comma = ", ";
            }
            buf.Append(")");
            return buf.ToString();
        }

        [Confined]
        public override string ToString() {
            return string.Format("MethodTarget({0} on {1})", Method, Method.DeclaringType.FullName);
        }

        internal Expression MakeExpression(RuleBuilder rule, IList<Expression> parameters) {
            Assert.NotNullItems(parameters);
            Debug.Assert(rule != null);

            return MakeExpression(
                new MethodBinderContext(_binder._binder, rule.Context),
                parameters
            );
        }

        internal Expression MakeExpression(Expression contextExpression, IList<Expression> parameters) {
            Assert.NotNullItems(parameters);
            Debug.Assert(contextExpression != null);

            return MakeExpression(
                new MethodBinderContext(_binder._binder, contextExpression),
                parameters
            );
        }

        internal Expression MakeExpression(MethodBinderContext context, IList<Expression> parameters) {
            Expression[] args = new Expression[_argBuilders.Count];
            for (int i = 0; i < _argBuilders.Count; i++) {
                args[i] = _argBuilders[i].ToExpression(context, parameters);
            }

            MethodBase mb = Method;
            MethodInfo mi = mb as MethodInfo;
            Expression ret, call;
            if (!mb.IsPublic || !mb.DeclaringType.IsVisible) {
                if (mi != null) {
                    mi = CompilerHelpers.GetCallableMethod(mi, _binder._binder.PrivateBinding);
                    if (mi != null) mb = mi;
                }
            }

            ConstructorInfo ci = mb as ConstructorInfo; // to stop fxcop from complaining about multiple casts
            Debug.Assert(mi != null || ci != null);
            if (mb.IsPublic && mb.DeclaringType.IsVisible) {
                // public method
                if (mi != null) {
                    Expression instance = mi.IsStatic ? null : _instanceBuilder.ToExpression(context, parameters);
                    call = Ast.SimpleCallHelper(instance, mi, args);
                } else {
                    call = Ast.SimpleNewHelper(ci, args);
                }
            } else {
                // Private binding, invoke via reflection
                if (mi != null) {
                    Expression instance = mi.IsStatic ? Ast.Null() : _instanceBuilder.ToExpression(context, parameters);
                    call = Ast.Call(
                        typeof(BinderOps).GetMethod("InvokeMethod"),
                        Ast.Constant(mi),
                        Ast.ConvertHelper(instance, typeof(object)),
                        Ast.NewArrayHelper(typeof(object), args)
                    );
                } else {
                    call = Ast.Call(
                        typeof(BinderOps).GetMethod("InvokeConstructor"),
                        Ast.Constant(ci),
                        Ast.NewArrayHelper(typeof(object), args)
                    );
                }
            }

            ret = _returnBuilder.ToExpression(context, _argBuilders, parameters, call);

            List<Expression> updates = null;
            for (int i = 0; i < _argBuilders.Count; i++) {
                Expression next = _argBuilders[i].UpdateFromReturn(context, parameters);
                if (next != null) {
                    if (updates == null) {
                        updates = new List<Expression>();
                    }
                    updates.Add(next);
                }
            }

            if (updates != null) {
                if (ret.Type != typeof(void)) {
                    VariableExpression temp = Ast.Variable(ret.Type, "$ret");
                    updates.Insert(0, Ast.Assign(temp, ret));
                    updates.Add(temp);
                    ret = Ast.Scope(Ast.Comma(updates.ToArray()), temp);
                } else {
                    updates.Insert(0, ret);
                    ret = Ast.Convert(
                        Ast.Comma(updates.ToArray()),
                        typeof(void)
                    );
                }
            }

            if (context.Temps != null) {
                ret = Ast.Scope(ret, context.Temps);
            }

            return ret;
        }

        /// <summary>
        /// Creates a call to this MethodTarget with the specified parameters.  Casts are inserted to force
        /// the types to the provided known types.
        /// 
        /// TODO: Remove RuleBuilder and knownTypes once we're fully meta
        /// </summary>
        /// <param name="rule">The rule being generated for the call</param>
        /// <param name="parameters">The explicit arguments</param>
        /// <param name="knownTypes">If non-null, the type for each element in parameters</param>
        /// <returns></returns>
        internal Expression MakeExpression(RuleBuilder rule, IList<Expression> parameters, IList<Type> knownTypes) {
            Debug.Assert(knownTypes == null || parameters.Count == knownTypes.Count);

            IList<Expression> args = parameters;
            if (knownTypes != null) {
                args = new Expression[parameters.Count];
                for (int i = 0; i < args.Count; i++) {
                    args[i] = parameters[i];
                    if (knownTypes[i] != null && !knownTypes[i].IsAssignableFrom(parameters[i].Type)) {
                        args[i] = Ast.Convert(parameters[i], CompilerHelpers.GetVisibleType(knownTypes[i]));
                    }
                }
            }

            return MakeExpression(rule, args);
        }

        private static int FindMaxPriority(IList<ArgBuilder> abs, int ceiling) {
            int max = 0;
            foreach (ArgBuilder ab in abs) {
                if (ab.Priority > ceiling) continue;

                max = System.Math.Max(max, ab.Priority);
            }
            return max;
        }

        internal int CompareEqualParameters(MethodTarget other) {
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
            for (int i = Int32.MaxValue; i >= 0; ) {
                int maxPriorityThis = FindMaxPriority(this._argBuilders, i);
                int maxPriorityOther = FindMaxPriority(other._argBuilders, i);

                if (maxPriorityThis < maxPriorityOther) return +1;
                if (maxPriorityOther < maxPriorityThis) return -1;

                i = maxPriorityThis - 1;
            }

            return 0;
        }

        private static int Compare(int x, int y) {
            if (x < y) return -1;
            else if (x > y) return +1;
            else return 0;
        }

        internal MethodTarget MakeParamsExtended(int argCount, SymbolId[] names, int[] nameIndexes) {
            Debug.Assert(BinderHelpers.IsParamsMethod(Method));

            List<ArgBuilder> newArgBuilders = new List<ArgBuilder>(_argBuilders.Count);

            // current argument that we consume, initially skip this if we have it.
            int curArg = CompilerHelpers.IsStatic(_method) ? 0 : 1;
            int kwIndex = -1;

            foreach (ArgBuilder ab in _argBuilders) {
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
                            curArg,
                            paramsUsed,
                            sab.Type.GetElementType()));

                        curArg += paramsUsed;
                    } else if (sab.IsParamsDict) {
                        // consume all the kw arguments
                        kwIndex = newArgBuilders.Count;
                    } else {
                        // consume the next argument
                        newArgBuilders.Add(new SimpleArgBuilder(curArg++, sab.Type));
                    }
                } else {
                    // CodeContext, null, default, etc...  we don't consume an 
                    // actual incoming argument.
                    newArgBuilders.Add(ab);
                }
            }

            if (kwIndex != -1) {
                newArgBuilders.Insert(kwIndex, new ParamsDictArgBuilder(curArg, names, nameIndexes));
            }

            return new MethodTarget(_binder, Method, argCount, _instanceBuilder, newArgBuilders, _returnBuilder);
        }

        private int GetConsumedArguments() {
            int consuming = 0;
            foreach (ArgBuilder argb in _argBuilders) {
                SimpleArgBuilder sab = argb as SimpleArgBuilder;
                if (sab != null && !sab.IsParamsDict) consuming++;
            }
            return consuming;
        }
    }
}
