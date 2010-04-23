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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Reflection;
using Microsoft.Scripting.Interpreter;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Scripting.Ast {
    public abstract class LightDynamicExpression : Expression, IInstructionProvider {
        private readonly CallSiteBinder _binder;

        protected LightDynamicExpression(CallSiteBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            _binder = binder;
        }

        public override bool CanReduce {
            get { return true; }
        }

        public CallSiteBinder Binder {
            get { return _binder; }
        }

        public override ExpressionType NodeType {
            get { return ExpressionType.Extension; }
        }

        public override Type Type {
            get { return typeof(object); }
        }

        #region IInstructionProvider Members

        public void AddInstructions(LightCompiler compiler) {
            Instruction instr = DynamicInstructionN.CreateUntypedInstruction(_binder, ArgumentCount);
            if (instr == null) {
                var lightBinder = _binder as ILightCallSiteBinder;
                if (lightBinder == null || !lightBinder.AcceptsArgumentArray) {
                    compiler.Compile(Reduce());
                    return;
                }

                Debug.Assert(Type == typeof(object));
                instr = new DynamicSplatInstruction(ArgumentCount, CallSite<Func<CallSite, ArgumentArray, object>>.Create(_binder));
            }

            for (int i = 0; i < ArgumentCount; i++) {
                compiler.Compile(GetArgument(i));
            }

            compiler.Instructions.Emit(instr);
        }

        #endregion

        public abstract override Expression Reduce();
        protected abstract int ArgumentCount { get; }
        protected abstract Expression GetArgument(int index);
    }

    #region Specialized Subclasses

    public class LightDynamicExpression1 : LightDynamicExpression {
        private readonly Expression _arg0;

        internal protected LightDynamicExpression1(CallSiteBinder binder, Expression arg0) 
            : base(binder) {
            ContractUtils.RequiresNotNull(arg0, "arg0");
            _arg0 = arg0;
        }

        public override Expression Reduce() {
            return Expression.Dynamic(Binder, Type, _arg0);
        }

        protected sealed override int ArgumentCount {
            get { return 1; }
        }

        public Expression Argument0 {
            get { return _arg0; }
        }

        protected sealed override Expression GetArgument(int index) {
            switch (index) {
                case 0: return _arg0;
                default: throw Assert.Unreachable;
            }
        }
    }

    public class LightTypedDynamicExpression1 : LightDynamicExpression1 {
        private readonly Type _returnType;

        internal protected LightTypedDynamicExpression1(CallSiteBinder binder, Type returnType, Expression arg0)
            : base(binder, arg0) {
            ContractUtils.RequiresNotNull(returnType, "returnType");
            _returnType = returnType;
        }

        public sealed override Type Type {
            get { return _returnType; }
        }
    }

    public class LightDynamicExpression2 : LightDynamicExpression {
        private readonly Expression _arg0, _arg1;

        internal protected LightDynamicExpression2(CallSiteBinder binder, Expression arg0, Expression arg1)
            : base(binder) {
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            _arg0 = arg0;
            _arg1 = arg1;
        }

        public override Expression Reduce() {
            return Expression.Dynamic(Binder, Type, _arg0, _arg1);
        }

        protected override int ArgumentCount {
            get { return 2; }
        }

        public Expression Argument0 {
            get { return _arg0; }
        }

        public Expression Argument1 {
            get { return _arg1; }
        }

        protected override Expression GetArgument(int index) {
            switch (index) {
                case 0: return _arg0;
                case 1: return _arg1;
                default: throw Assert.Unreachable;
            }
        }
    }

    public class LightTypedDynamicExpression2 : LightDynamicExpression2 {
        private readonly Type _returnType;

        internal protected LightTypedDynamicExpression2(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1)
            : base(binder, arg0, arg1) {
            ContractUtils.RequiresNotNull(returnType, "returnType");
            _returnType = returnType;
        }

        public sealed override Type Type {
            get { return _returnType; }
        }
    }

    public class LightDynamicExpression3 : LightDynamicExpression {
        private readonly Expression _arg0, _arg1, _arg2;

        internal protected LightDynamicExpression3(CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2)
            : base(binder) {
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            ContractUtils.RequiresNotNull(arg2, "arg2");
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
        }

        public override Expression Reduce() {
            return Expression.Dynamic(Binder, Type, _arg0, _arg1, _arg2);
        }

        protected sealed override int ArgumentCount {
            get { return 3; }
        }

        public Expression Argument0 {
            get { return _arg0; }
        }

        public Expression Argument1 {
            get { return _arg1; }
        }

        public Expression Argument2 {
            get { return _arg2; }
        }

        protected sealed override Expression GetArgument(int index) {
            switch (index) {
                case 0: return _arg0;
                case 1: return _arg1;
                case 2: return _arg2;
                default: throw Assert.Unreachable;
            }
        }
    }

    internal class LightTypedDynamicExpression3 : LightDynamicExpression3 {
        private readonly Type _returnType;

        internal protected LightTypedDynamicExpression3(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2)
            : base(binder, arg0, arg1, arg2) {
            ContractUtils.RequiresNotNull(returnType, "returnType");
            _returnType = returnType;
        }

        public sealed override Type Type {
            get { return _returnType; }
        }
    }

    public class LightDynamicExpression4 : LightDynamicExpression {
        private readonly Expression _arg0, _arg1, _arg2, _arg3;

        internal protected LightDynamicExpression4(CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
            : base(binder) {
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            ContractUtils.RequiresNotNull(arg2, "arg2");
            ContractUtils.RequiresNotNull(arg3, "arg3");
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
        }

        public override Expression Reduce() {
            return Expression.Dynamic(Binder, Type, _arg0, _arg1, _arg2, _arg3);
        }

        protected sealed override int ArgumentCount {
            get { return 4; }
        }

        public Expression Argument0 {
            get { return _arg0; }
        }

        public Expression Argument1 {
            get { return _arg1; }
        }

        public Expression Argument2 {
            get { return _arg2; }
        }

        public Expression Argument3 {
            get { return _arg3; }
        }

        protected sealed override Expression GetArgument(int index) {
            switch (index) {
                case 0: return _arg0;
                case 1: return _arg1;
                case 2: return _arg2;
                case 3: return _arg3;
                default: throw Assert.Unreachable;
            }
        }
    }

    public class LightTypedDynamicExpression4 : LightDynamicExpression4 {
        private readonly Type _returnType;

        internal LightTypedDynamicExpression4(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
            : base(binder, arg0, arg1, arg2, arg3) {
            ContractUtils.RequiresNotNull(returnType, "returnType");
            _returnType = returnType;
        }

        public sealed override Type Type {
            get { return _returnType; }
        }
    }

    public class LightTypedDynamicExpressionN : LightDynamicExpression {
        private readonly IList<Expression> _args;
        private readonly Type _returnType;

        internal protected LightTypedDynamicExpressionN(CallSiteBinder binder, Type returnType, IList<Expression> args) 
            : base(binder) {
            ContractUtils.RequiresNotNull(returnType, "returnType");
            ContractUtils.RequiresNotEmpty(args, "args");
            _args = args;
            _returnType = returnType;
        }

        public override Expression Reduce() {
            return Expression.Dynamic(Binder, Type, _args);
        }

        protected sealed override int ArgumentCount {
            get { return _args.Count; }
        }

        public sealed override Type Type {
            get { return _returnType; }
        }

        public IList<Expression> Arguments {
            get { return _args; }
        }

        protected sealed override Expression GetArgument(int index) {
            return _args[index];
        }
    }

    #endregion

    public static partial class Utils {
        public static LightDynamicExpression LightDynamic(CallSiteBinder binder, Expression arg0) {
            return LightDynamic(binder, typeof(object), arg0);
        }

        public static LightDynamicExpression LightDynamic(CallSiteBinder binder, Type returnType, Expression arg0) {
            return returnType == typeof(object) ? 
                new LightDynamicExpression1(binder, arg0) :
                (LightDynamicExpression)new LightTypedDynamicExpression1(binder, returnType, arg0);
        }

        public static LightDynamicExpression LightDynamic(CallSiteBinder binder, Expression arg0, Expression arg1) {
            return LightDynamic(binder, typeof(object), arg0, arg1);
        }

        public static LightDynamicExpression LightDynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1) {
            return returnType == typeof(object) ?
                new LightDynamicExpression2(binder, arg0, arg1) :
                (LightDynamicExpression)new LightTypedDynamicExpression2(binder, returnType, arg0, arg1);
        }

        public static LightDynamicExpression LightDynamic(CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2) {
            return LightDynamic(binder, typeof(object), arg0, arg1, arg2);
        }

        public static LightDynamicExpression LightDynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2) {
            return returnType == typeof(object) ?
                new LightDynamicExpression3(binder, arg0, arg1, arg2) :
                (LightDynamicExpression)new LightTypedDynamicExpression3(binder, returnType, arg0, arg1, arg2);
        }

        public static LightDynamicExpression LightDynamic(CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            return LightDynamic(binder, typeof(object), arg0, arg1, arg2, arg3);
        }

        public static LightDynamicExpression LightDynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            return returnType == typeof(object) ?
                new LightDynamicExpression4(binder, arg0, arg1, arg2, arg3) :
                (LightDynamicExpression)new LightTypedDynamicExpression4(binder, returnType, arg0, arg1, arg2, arg3);
        }

        public static LightDynamicExpression LightDynamic(CallSiteBinder binder, ReadOnlyCollectionBuilder<Expression> arguments) {
            return LightDynamic(binder, typeof(object), arguments);
        }

        public static LightDynamicExpression LightDynamic(CallSiteBinder binder, Type returnType, ReadOnlyCollectionBuilder<Expression> arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            return new LightTypedDynamicExpressionN(binder, returnType, arguments.ToReadOnlyCollection());
        }

        public static LightDynamicExpression LightDynamic(CallSiteBinder binder, ExpressionCollectionBuilder<Expression> arguments) {
            return LightDynamic(binder, typeof(object), arguments);
        }

        public static LightDynamicExpression LightDynamic(CallSiteBinder binder, Type returnType, ExpressionCollectionBuilder<Expression> arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            switch (arguments.Count) {
                case 1: return LightDynamic(binder, returnType, arguments.Expression0);
                case 2: return LightDynamic(binder, returnType, arguments.Expression0, arguments.Expression1);
                case 3: return LightDynamic(binder, returnType, arguments.Expression0, arguments.Expression1, arguments.Expression2);
                case 4: return LightDynamic(binder, returnType, arguments.Expression0, arguments.Expression1, arguments.Expression2, arguments.Expression3);
                default: return LightDynamic(binder, returnType, arguments.Expressions);
            }
        }
    }
}
