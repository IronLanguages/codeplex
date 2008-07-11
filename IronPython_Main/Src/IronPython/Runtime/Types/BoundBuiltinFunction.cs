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
using System.Diagnostics;
using System.Scripting;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace IronPython.Runtime.Types {
    using Ast = System.Linq.Expressions.Expression;

    [PythonSystemType("builtin_function_or_method")]
    public sealed partial class BoundBuiltinFunction : PythonTypeSlot, IOldDynamicObject, ICodeFormattable, IValueEquality {
        private readonly BuiltinFunction/*!*/ _target;
        private readonly object _instance;

        /// <summary>
        /// creates a new BoundBuiltinFunction.  instance may be null when creating
        /// a method bound to a method on "None" or object.
        /// </summary>
        internal BoundBuiltinFunction(BuiltinFunction/*!*/ target, object instance) {
            Debug.Assert(target != null);

            _target = target;
            _instance = instance;
        }

        #region Object overrides

        public override bool Equals(object obj) {
            BoundBuiltinFunction other = obj as BoundBuiltinFunction;
            if (other == null) return false;

            return other._instance == _instance && other._target == _target;
        }

        public override int GetHashCode() {
            return _instance.GetHashCode() ^ _target.GetHashCode();
        }

        #endregion

        #region PythonTypeSlot Overrides

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = this;
            return true;
        }

        #endregion

        #region IOldDynamicObject Members

        RuleBuilder<T> IOldDynamicObject.GetRule<T>(OldDynamicAction action, CodeContext context, object[] args) {
            switch(action.Kind) {
                case DynamicActionKind.Call: return MakeCallRule<T>((OldCallAction)action, context, args);
                case DynamicActionKind.DoOperation: return MakeDoOperationRule<T>((OldDoOperationAction)action, context, args);
            }
            return null;
        }

        private RuleBuilder<T> MakeDoOperationRule<T>(OldDoOperationAction doOperationAction, CodeContext context, object[] args) where T : class {
            switch(doOperationAction.Operation) {
                case Operators.CallSignatures:
                    return PythonDoOperationBinderHelper<T>.MakeCallSignatureRule(context.LanguageContext.Binder, Target.Targets, DynamicHelpers.GetPythonType(args[0]));
                case Operators.IsCallable:
                    return PythonBinderHelper.MakeIsCallableRule<T>(context, this, true);
            }
            return null;
        }

        private RuleBuilder<T> MakeCallRule<T>(OldCallAction action, CodeContext context, object[] args) where T : class {
            CallBinderHelper<T, OldCallAction> helper = new CallBinderHelper<T, OldCallAction>(
                context, 
                action, 
                args, 
                Target.Targets, 
                Target.Level,
                Target.IsReversedOperator);
            RuleBuilder<T> rule = helper.Rule;
            Expression instance = Ast.Property(
                Ast.Convert(
                    rule.Parameters[0],
                    typeof(BoundBuiltinFunction)
                ),
                typeof(BoundBuiltinFunction).GetProperty("__self__")
            );

            Expression instanceVal = instance;
            Type testType = CompilerHelpers.GetType(__self__);

            // cast the instance to the correct type
            if (CompilerHelpers.IsStrongBox(__self__)) {
                instance = ReadStrongBoxValue(instance);
            } else if (!testType.IsEnum) {
                // We need to deal w/ wierd types like MarshalByRefObject.  
                // We could have an MBRO whos DeclaringType is completely different.  
                // Therefore we special case it here and cast to the declaring type
                Type selfType = CompilerHelpers.GetType(__self__);
                if (!selfType.IsVisible && PythonContext.GetContext(context).DomainManager.GlobalOptions.PrivateBinding) {
                    helper.InstanceType = selfType;
                } else {
                    selfType = CompilerHelpers.GetVisibleType(selfType);

                    if (selfType == typeof(object) && Target.DeclaringType.IsInterface) {
                        selfType = Target.DeclaringType;
                    }

                    if (Target.DeclaringType.IsInterface && selfType.IsValueType) {
                        // explitit interface implementation dispatch on a value type, don't
                        // unbox the value type before the dispatch.
                        instance = Ast.Convert(instance, Target.DeclaringType);
                    } else {
#if SILVERLIGHT
                    instance = Ast.Convert(instance, selfType);
#else
                        Type convType = selfType == typeof(MarshalByRefObject) ? CompilerHelpers.GetVisibleType(Target.DeclaringType) : selfType;

                        instance = Ast.Convert(instance, convType);
#endif
                    }
                }
            } else {
                // we don't want to cast the enum to it's real type, it will unbox it 
                // and turn it into it's underlying type.  We presumably want to call 
                // a method on the Enum class though - so we cast to Enum instead.
                instance = Ast.Convert(instance, typeof(Enum));
            }

            helper.Instance = instance;

            RuleBuilder<T> newRule = helper.MakeRule();
            if (newRule == rule) {
                // work around ActionOnCall, we should flow the rule in eventually.
                // For the time being it contains sufficient tests so we don't need
                // to add more.
                rule.AddTest(
                    Target.MakeFunctionTest(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("GetBoundBuiltinFunctionTarget"),
                            Ast.Convert(rule.Parameters[0], typeof(BoundBuiltinFunction))
                        )
                    )
                );
                rule.AddTest(rule.MakeTypeTest(testType, instanceVal));
            }

            if (newRule.IsError && Target.IsBinaryOperator && args.Length == 2) { // 1 bound function + 1 args
                // BinaryOperators return NotImplemented on failure.
                newRule.Target = rule.MakeReturn(context.LanguageContext.Binder, Ast.Property(null, typeof(PythonOps), "NotImplemented"));
            }

            return newRule;
        }

        private MemberExpression/*!*/ ReadStrongBoxValue(Expression instance) {
            return Ast.Field(
                Ast.Convert(instance, __self__.GetType()),
                __self__.GetType().GetField("Value")
            );
        }

        #endregion

        #region Public Python API Surface

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cls")]
        public static object/*!*/ __new__(object cls, object newFunction, object inst) {
            return new Method(newFunction, inst, null);
        }

        public object __self__ {
            get {
                return _instance;
            }
        }

        public string __name__ {
            get {
                return _target.Name;
            }
        }

        public string __doc__ {
            get {
                return Target.__doc__;
            }
        }

        public object __call__(CodeContext context, SiteLocalStorage<DynamicSite<object, object[], IAttributesCollection, object>> storage, [ParamDictionary]IAttributesCollection dictArgs, params object[] args) {
            return Target.Call(context, storage, __self__, args, dictArgs);
        }

        public object/*!*/ this[PythonTuple key] {
            [PythonHidden]
            get {
                return new BoundBuiltinFunction(Target[key], __self__);
            }
        }

        public object/*!*/ this[params object[] key] {
            [PythonHidden]
            get {
                return new BoundBuiltinFunction(Target[key], __self__);
            }
        }

        public BuiltinFunctionOverloadMapper/*!*/ Overloads {
            [PythonHidden]
            get {
                return new BuiltinFunctionOverloadMapper(Target, __self__);
            }
        }

        #endregion

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return string.Format("<built-in method {0} of {1} object at {2}>",
                    __name__,
                    PythonOps.GetPythonTypeName(__self__),
                    PythonOps.HexId(__self__));
        }

        #endregion

        #region Internal API

        internal BuiltinFunction/*!*/ Target {
            get {
                return _target;
            }
        }

        #endregion

        #region IValueEquality Members

        int IValueEquality.GetValueHashCode() {
            return PythonOps.Hash(_instance) ^ _target.GetHashCode();
        }

        bool IValueEquality.ValueEquals(object other) {
            BoundBuiltinFunction bbf = other as BoundBuiltinFunction;
            if (bbf == null) return false;

            return PythonOps.EqualRetBool(bbf._instance, _instance) && bbf._target == _target;
        }

        #endregion
    }
}
