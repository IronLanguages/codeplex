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

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    using Ast = Microsoft.Scripting.Ast.Ast;

    [PythonSystemType("builtin_function_or_method")]
    public sealed partial class BoundBuiltinFunction : PythonTypeSlot, IDynamicObject, ICodeFormattable {
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

        #region IDynamicObject Members

        LanguageContext IDynamicObject.LanguageContext {
            get { return DefaultContext.Default.LanguageContext; }
        }

        StandardRule<T> IDynamicObject.GetRule<T>(DynamicAction action, CodeContext context, object[] args) {
            switch(action.Kind) {
                case DynamicActionKind.Call: return MakeCallRule<T>((CallAction)action, context, args);
                case DynamicActionKind.DoOperation: return MakeDoOperationRule<T>((DoOperationAction)action, context, args);
            }
            return null;
        }

        private StandardRule<T> MakeDoOperationRule<T>(DoOperationAction doOperationAction, CodeContext context, object[] args) {
            switch(doOperationAction.Operation) {
                case Operators.CallSignatures:
                    return PythonDoOperationBinderHelper<T>.MakeCallSignatureRule(context.LanguageContext.Binder, Target.Targets, DynamicHelpers.GetPythonType(args[0]));
                case Operators.IsCallable:
                    return PythonBinderHelper.MakeIsCallableRule<T>(context, this, true);
            }
            return null;
        }

        private StandardRule<T> MakeCallRule<T>(CallAction action, CodeContext context, object[] args) {
            CallBinderHelper<T, CallAction> helper = new CallBinderHelper<T, CallAction>(
                context, 
                action, 
                args, 
                Target.Targets, 
                Target.Level,
                Target.IsReversedOperator);
            StandardRule<T> rule = helper.Rule;
            Expression instance = Ast.ReadProperty(
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
#if SILVERLIGHT
                    instance = Ast.Convert(instance, selfType);
#else
                    Type convType = selfType == typeof(MarshalByRefObject) ? CompilerHelpers.GetVisibleType(Target.DeclaringType) : selfType;

                    instance = Ast.Convert(instance, convType);
#endif
                }
            } else {
                // we don't want to cast the enum to it's real type, it will unbox it 
                // and turn it into it's underlying type.  We presumably want to call 
                // a method on the Enum class though - so we cast to Enum instead.
                instance = Ast.Convert(instance, typeof(Enum));
            }

            helper.Instance = instance;

            StandardRule<T> newRule = helper.MakeRule();
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
                newRule.Target = rule.MakeReturn(context.LanguageContext.Binder, Ast.ReadField(null, typeof(PythonOps), "NotImplemented"));
            }

            return newRule;
        }

        private MemberExpression/*!*/ ReadStrongBoxValue(Expression instance) {
            return Ast.ReadField(
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

        public object __call__(CodeContext context, [ParamDictionary]IDictionary<object, object> dictArgs, params object[] args) {
            object[] realArgs;
            string[] argNames;
            Target.DictArgsHelper(dictArgs, args, out realArgs, out argNames);

            return Target.CallHelper(context, ArrayUtils.Insert(__self__, realArgs), argNames, null);
        }

        public object/*!*/ this[PythonTuple key] {
            get {
                return new BoundBuiltinFunction(Target[key], __self__);
            }
        }

        public object/*!*/ this[params object[] key] {
            get {
                return new BoundBuiltinFunction(Target[key], __self__);
            }
        }

        public BuiltinFunctionOverloadMapper/*!*/ Overloads {
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
    }
}
