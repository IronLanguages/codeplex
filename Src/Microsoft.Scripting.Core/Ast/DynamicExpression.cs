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
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Linq.Expressions.Compiler;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    /// <summary>
    /// A late-bound operation. The precise semantics is determined by the
    /// Binder. If the Binder is one of the standard dynamic operations
    /// supported by MetaObject, the run-time behavior can be infered from the
    /// StandardAction
    /// </summary>
    public sealed class DynamicExpression : Expression {
        private readonly ReadOnlyCollection<Expression> _arguments;
        private readonly CallSiteBinder _binder;
        private readonly Type _delegateType;

        internal DynamicExpression(Type returnType, Annotations annotations, Type delegateType, CallSiteBinder binder, ReadOnlyCollection<Expression> arguments)
            : base(ExpressionType.Dynamic, returnType, annotations) {
            Debug.Assert(returnType == delegateType.GetMethod("Invoke").GetReturnType());
            _delegateType = delegateType;
            _binder = binder;
            _arguments = arguments;
        }

        /// <summary>
        /// The CallSiteBinder, which determines the runtime behavior of the
        /// dynamic site
        /// </summary>
        public CallSiteBinder Binder {
            get { return _binder; }
        }

        /// <summary>
        /// The type of the CallSite's delegate
        /// </summary>
        public Type DelegateType {
            get { return _delegateType; }
        }

        /// <summary>
        /// Arguments to the dynamic operation
        /// </summary>
        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }

        /// <summary>
        /// If the CallSiteBinder is a StandardAction, this returns it.
        /// Otherwise, returns null.
        /// </summary>
        public StandardAction StandardAction {
            get { return _binder as StandardAction; }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitDynamic(this);
        }
    }

    public partial class Expression {

        public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, params Expression[] arguments) {
            return MakeDynamic(delegateType, binder, null, arguments);
        }
        public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Annotations annotations, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(binder, "binder");
            ContractUtils.Requires(delegateType.IsSubclassOf(typeof(Delegate)), "delegateType", Strings.TypeMustBeDerivedFromSystemDelegate);

            var method = delegateType.GetMethod("Invoke");
            var pi = method.GetParameters();
            ContractUtils.Requires(pi.Length > 0 && pi[0].ParameterType == typeof(CallSite), "delegateType", Strings.FirstArgumentMustBeCallSite);

            var args = arguments.ToReadOnly();
            ValidateArgumentTypes(method, ExpressionType.Dynamic, ref args);

            return new DynamicExpression(method.GetReturnType(), annotations, delegateType, binder, args);
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, params Expression[] arguments) {
            return Dynamic(binder, returnType, null, (IEnumerable<Expression>)arguments);
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Annotations annotations, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(binder, "binder");
            
            var args = arguments.ToReadOnly();
            var types = new List<Type>();
            types.Add(typeof(CallSite));
            foreach (var arg in args) {
                RequiresCanRead(arg, "arguments");
                var type = arg.Type;
                ContractUtils.RequiresNotNull(type, "type");
                TypeUtils.ValidateType(type);
                ContractUtils.Requires(type != typeof(void), Strings.ArgumentTypeCannotBeVoid);
                types.Add(type);
            }
            types.Add(returnType);

            Type delegateType = DelegateHelpers.MakeDelegate(types.ToArray());

            // Since we made a delegate with argument types that exactly match,
            // we can skip delegate and argument validation
            return new DynamicExpression(returnType, annotations, delegateType, binder, args);
        }

        [Obsolete("use Dynamic instead")]
        public static DynamicExpression ActionExpression(CallSiteBinder action, Type resultType, params Expression[] arguments) {
            return Dynamic(action, resultType, null, (IEnumerable<Expression>)arguments);
        }
        [Obsolete("use Dynamic instead")]
        public static DynamicExpression ActionExpression(CallSiteBinder action, Type resultType, IEnumerable<Expression> arguments) {
            return Dynamic(action, resultType, null, arguments);
        }
        [Obsolete("use Dynamic instead")]
        public static DynamicExpression ActionExpression(CallSiteBinder action, Type resultType, Annotations annotations, params Expression[] arguments) {
            return Dynamic(action, resultType, annotations, (IEnumerable<Expression>)arguments);
        }
        [Obsolete("use Dynamic instead")]
        public static DynamicExpression ActionExpression(CallSiteBinder action, Type resultType, Annotations annotations, IEnumerable<Expression> arguments) {
            return Dynamic(action, resultType, annotations, arguments);
        }
    }
}
