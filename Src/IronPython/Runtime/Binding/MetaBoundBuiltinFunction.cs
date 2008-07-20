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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Utils;

using Microsoft.Scripting.Generation;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    class MetaBoundBuiltinFunction : MetaPythonObject, IInvokableWithContext {
        public MetaBoundBuiltinFunction(Expression/*!*/ expression, Restrictions/*!*/ restrictions, BoundBuiltinFunction/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region IInvokableWithContext Members

        public MetaObject InvokeWithContext(InvokeAction call, Expression codeContext, MetaObject[] args) {
            args = ArrayUtils.RemoveFirst(args);

            CallSignature signature = BindingHelpers.GetCallSignature(call);
            Expression instance = Ast.Property(
                Ast.Convert(
                    Expression,
                    typeof(BoundBuiltinFunction)
                ),
                typeof(BoundBuiltinFunction).GetProperty("__self__")
            );

            MetaObject self = GetInstance(
                instance,
                CompilerHelpers.GetType(Value.__self__),
                Restrictions.Merge(
                    Restrictions.TypeRestriction(
                        Expression,
                        typeof(BoundBuiltinFunction)
                    )
                ).Merge(
                    Restrictions.ExpressionRestriction(
                        Value.Target.MakeFunctionTest(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("GetBoundBuiltinFunctionTarget"),
                                Ast.Convert(Expression, typeof(BoundBuiltinFunction))
                            )
                        )
                    )
                )
            );

            MetaObject res;
            BinderState state = BinderState.GetBinderState(call);
            BindingTarget dummy;
            if (Value.Target.IsReversedOperator) {
                res = state.Binder.CallMethod(
                    codeContext,
                    Value.Target.Targets,
                    ArrayUtils.Append(args, self),
                    GetReversedSignature(signature),
                    self.Restrictions,
                    NarrowingLevel.None,
                    Value.Target.IsBinaryOperator ?
                        PythonNarrowing.BinaryOperator :
                        NarrowingLevel.All,
                    Value.Target.Name,
                    out dummy
                );
            } else {
                res = state.Binder.CallInstanceMethod(
                    codeContext,
                    Value.Target.Targets,
                    self,
                    args,
                    signature,
                    self.Restrictions,
                    NarrowingLevel.None,
                    Value.Target.IsBinaryOperator ?
                        PythonNarrowing.BinaryOperator :
                        NarrowingLevel.All,
                    Value.Target.Name,
                    out dummy
                );
            }

            if (Value.Target.IsBinaryOperator && args.Length == 1 && res.Expression.NodeType == ExpressionType.ThrowStatement) { // 1 bound function + 1 args
                // binary operators return NotImplemented on a failure to call them
                res = new MetaObject(
                    Ast.Property(null, typeof(PythonOps), "NotImplemented"),
                    res.Restrictions
                );
            }

            return res;
        }

        #endregion

        #region MetaObject Overrides

        public override MetaObject/*!*/ Invoke(InvokeAction/*!*/ call, params MetaObject/*!*/[]/*!*/ args) {
            return InvokeWithContext(call, Ast.Constant(BinderState.GetBinderState(call).Context), args);
        }

        private static CallSignature GetReversedSignature(CallSignature signature) {
            return new CallSignature(ArrayUtils.Append(signature.GetArgumentInfos(), new ArgumentInfo(ArgumentKind.Simple)));
        }

        public override MetaObject Convert(ConvertAction/*!*/ conversion, MetaObject/*!*/[]/*!*/ args) {
            if (conversion.ToType.IsSubclassOf(typeof(Delegate))) {
                return MakeDelegateTarget(conversion, conversion.ToType, Restrict(typeof(BoundBuiltinFunction)));
            }
            return conversion.Fallback(args);
        }

        #endregion

        #region Misc helpers

        private MetaObject/*!*/ GetInstance(Expression/*!*/ instance, Type/*!*/ testType, Restrictions/*!*/ restrictions) {
            Assert.NotNull(instance, testType);
            object instanceValue = Value.__self__;

            restrictions = restrictions.Merge(Restrictions.TypeRestriction(instance, testType));

            // cast the instance to the correct type
            if (CompilerHelpers.IsStrongBox(instanceValue)) {
                instance = ReadStrongBoxValue(instance);
                instanceValue = ((IStrongBox)instanceValue).Value;
            } else if (!testType.IsEnum) {
                // We need to deal w/ wierd types like MarshalByRefObject.  
                // We could have an MBRO whos DeclaringType is completely different.  
                // Therefore we special case it here and cast to the declaring type

                // TODO: Private binding
                //if (selfType.IsVisible /*|| !PythonContext.GetContext(context).DomainManager.GlobalOptions.PrivateBinding*/) { 

                Type selfType = CompilerHelpers.GetType(Value.__self__);
                selfType = CompilerHelpers.GetVisibleType(selfType);

                if (selfType == typeof(object) && Value.Target.DeclaringType.IsInterface) {
                    selfType = Value.Target.DeclaringType;
                }

                if (Value.Target.DeclaringType.IsInterface && selfType.IsValueType) {
                    // explitit interface implementation dispatch on a value type, don't
                    // unbox the value type before the dispatch.
                    instance = Ast.Convert(instance, Value.Target.DeclaringType);
                } else if (selfType.IsValueType) {
                    // We might be calling a a mutating method (like
                    // Rectangle.Intersect). If so, we want it to mutate
                    // the boxed value directly
                    instance = Ast.Unbox(instance, selfType);
                } else {
#if SILVERLIGHT
                    instance = Ast.Convert(instance, selfType);
#else
                    Type convType = selfType == typeof(MarshalByRefObject) ? CompilerHelpers.GetVisibleType(Value.Target.DeclaringType) : selfType;

                    instance = Ast.Convert(instance, convType);
#endif
                }
            } else {
                // we don't want to cast the enum to it's real type, it will unbox it 
                // and turn it into it's underlying type.  We presumably want to call 
                // a method on the Enum class though - so we cast to Enum instead.
                instance = Ast.Convert(instance, typeof(Enum));
            }
            return new MetaObject(
                instance,
                restrictions,
                instanceValue
            );
        }

        private MemberExpression/*!*/ ReadStrongBoxValue(Expression instance) {
            return Ast.Field(
                Ast.Convert(instance, Value.__self__.GetType()),
                Value.__self__.GetType().GetField("Value")
            );
        }

        public new BoundBuiltinFunction/*!*/ Value {
            get {
                return (BoundBuiltinFunction)base.Value;
            }
        }

        #endregion        
    }
}
