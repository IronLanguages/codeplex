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
    public class DynamicExpression : Expression {
        private readonly ReadOnlyCollection<Expression> _arguments;
        private readonly CallSiteBinder _binder;
        private readonly Type _delegateType;

        internal DynamicExpression(Annotations annotations, Type delegateType, CallSiteBinder binder, ReadOnlyCollection<Expression> arguments)
            : base(annotations) {
            Debug.Assert(delegateType.GetMethod("Invoke").GetReturnType() == typeof(object) || GetType() != typeof(DynamicExpression));
            _delegateType = delegateType;
            _binder = binder;
            _arguments = arguments;
        }

        internal static DynamicExpression Make(Type returnType, Annotations annotations, Type delegateType, CallSiteBinder binder, ReadOnlyCollection<Expression> arguments) {
            if (returnType == typeof(object)) {
                return new DynamicExpression(annotations, delegateType, binder, arguments);
            } else {
                return new TypedDynamicExpression(returnType, annotations, delegateType, binder, arguments);
            }
        }

        protected override Type GetExpressionType() {
            return typeof(object);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Dynamic;
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

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitDynamic(this);
        }
    }

    internal class TypedDynamicExpression : DynamicExpression {
        private readonly Type _returnType;

        internal TypedDynamicExpression(Type returnType, Annotations annotations, Type delegateType, CallSiteBinder binder, ReadOnlyCollection<Expression> arguments)
            : base(annotations, delegateType, binder, arguments) {
            Debug.Assert(delegateType.GetMethod("Invoke").GetReturnType() == returnType);
            _returnType = returnType;
        }

        protected override Type GetExpressionType() {
            return _returnType;
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
            var pi = method.GetParametersCached();
            ContractUtils.Requires(pi.Length > 0 && pi[0].ParameterType == typeof(CallSite), "delegateType", Strings.FirstArgumentMustBeCallSite);

            var args = arguments.ToReadOnly();
            ValidateArgumentTypes(method, ExpressionType.Dynamic, ref args);

            return DynamicExpression.Make(method.GetReturnType(), annotations, delegateType, binder, args);
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, params Expression[] arguments) {
            return Dynamic(binder, returnType, null, (IEnumerable<Expression>)arguments);
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0) {
            return MakeDynamic(binder, returnType, null, new ReadOnlyCollection<Expression>(new Expression[] { arg0 }));
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1) {
            return MakeDynamic(binder, returnType, null, new ReadOnlyCollection<Expression>(new Expression[] { arg0, arg1 }));
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2) {
            return MakeDynamic(binder, returnType, null, new ReadOnlyCollection<Expression>(new Expression[] { arg0, arg1, arg2 }));
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            return MakeDynamic(binder, returnType, null, new ReadOnlyCollection<Expression>(new Expression[] { arg0, arg1, arg2, arg3 }));
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Annotations annotations, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.RequiresNotNull(returnType, "returnType");

            return MakeDynamic(binder, returnType, annotations, arguments.ToReadOnly());
        }

        private static DynamicExpression MakeDynamic(CallSiteBinder binder, Type returnType, Annotations annotations, ReadOnlyCollection<Expression> args) {
            ContractUtils.RequiresNotNull(binder, "binder");

            for (int i = 0; i < args.Count; i++) {
                Expression arg = args[i];

                RequiresCanRead(arg, "arguments");
                var type = arg.Type;
                ContractUtils.RequiresNotNull(type, "type");
                TypeUtils.ValidateType(type);
                ContractUtils.Requires(type != typeof(void), Strings.ArgumentTypeCannotBeVoid);
            }

            Type delegateType = DelegateHelpers.MakeCallSiteDelegate(args, returnType);

            // Since we made a delegate with argument types that exactly match,
            // we can skip delegate and argument validation
            return DynamicExpression.Make(returnType, annotations, delegateType, binder, args);
        }
    }
}
