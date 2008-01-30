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
using Microsoft.Scripting.Generation;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    using Ast = Microsoft.Scripting.Ast.Ast;

    [PythonSystemType("method_descriptor")]
    public sealed class BuiltinMethodDescriptor : PythonTypeSlot, IDynamicObject, ICodeFormattable {
        internal readonly BuiltinFunction/*!*/ _template;

        internal BuiltinMethodDescriptor(BuiltinFunction/*!*/ function) {
            _template = function;
        }

        #region Internal APIs

        internal object UncheckedGetAttribute(object instance) {
            if (instance == null) return this;
            return new BoundBuiltinFunction(_template, instance);
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            if (instance != null) {
                CheckSelf(context, instance);
                value = UncheckedGetAttribute(instance);
                return true;
            }
            value = this;
            return true;
        }

        internal override bool TryGetBoundValue(CodeContext context, object instance, PythonType owner, out object value) {
            return TryGetValue(context, instance, owner, out value);
        }

        internal BuiltinFunction/*!*/ Template {
            get { return _template; }
        }

        internal Type DeclaringType {
            get {
                return _template.DeclaringType;
            }
        }

        internal static void CheckSelfWorker(CodeContext context, object self, BuiltinFunction template) {
            // to a fast check on the CLR types, if they match we can avoid the slower
            // check that involves looking up dynamic types. (self can be null on
            // calls like set.add(None) 
            if (self != null && self.GetType() == template.DeclaringType) return;

            Type selfType = CompilerHelpers.GetType(self);
            Debug.Assert(selfType != null);

            if (!selfType.IsAssignableFrom(template.DeclaringType)) {
                // if a conversion exists to the type allow the call.
                context.LanguageContext.Binder.Convert(self, template.DeclaringType);
            }
            return;
        }

        #endregion

        #region Private Helpers

        private void CheckSelf(CodeContext context, object self) {
            // if the type has been re-optimized (we also have base type info in here) 
            // then we can't do the type checks right now :(.
            if ((_template.FunctionType & FunctionType.SkipThisCheck) != 0)
                return;

            if ((_template.FunctionType & FunctionType.FunctionMethodMask) == FunctionType.Method) {
                CheckSelfWorker(context, self, _template);
            }
        }

        #endregion

        #region IContextAwareMember Members

        internal override bool IsVisible(CodeContext context, PythonType owner) {
            return _template.IsVisible(context, owner);
        }

        #endregion

        #region IDynamicObject Members

        LanguageContext IDynamicObject.LanguageContext {
            get { return DefaultContext.Default.LanguageContext; }
        }

        StandardRule<T> IDynamicObject.GetRule<T>(DynamicAction action, CodeContext context, object[] args) {
            if (action.Kind == DynamicActionKind.Call) {
                return MakeCallRule<T>((CallAction)action, context, args);
            } else if (action.Kind == DynamicActionKind.DoOperation) {
                return MakeDoOperationRule<T>((DoOperationAction)action, context, args);
            }
            return null;
        }

        private StandardRule<T> MakeDoOperationRule<T>(DoOperationAction doOperationAction, CodeContext context, object[] args) {
            switch (doOperationAction.Operation) {
                case Operators.IsCallable:
                    return PythonBinderHelper.MakeIsCallableRule<T>(context, this, true);
                case Operators.CallSignatures:
                    return IronPython.Runtime.Calls.PythonDoOperationBinderHelper<T>.MakeCallSignatureRule(context.LanguageContext.Binder, Template.Targets, DynamicHelpers.GetPythonType(args[0]));
            }
            return null;
        }

        private StandardRule<T> MakeCallRule<T>(CallAction action, CodeContext context, object[] args) {
            CallBinderHelper<T, CallAction> helper = new CallBinderHelper<T, CallAction>(context, action, args, Template.Targets, Template.Level, Template.IsReversedOperator);
            StandardRule<T> rule = helper.MakeRule();
            if (Template.IsBinaryOperator && rule.IsError && args.Length == 3) { // 1 built-in method descriptor + 2 args
                // BinaryOperators return NotImplemented on failure.
                rule.Target = rule.MakeReturn(context.LanguageContext.Binder, Ast.ReadField(null, typeof(PythonOps), "NotImplemented"));
            }
            rule.AddTest(
                Template.MakeFunctionTest(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("GetBuiltinMethodDescriptorTemplate"),
                        Ast.Convert(rule.Parameters[0], typeof(BuiltinMethodDescriptor))
                    )
                )
            );
            return rule;
        }

        #endregion

        #region Public Python API

        public string __name__ {
            get {
                return Template.__name__;
            }
        }

        public string __doc__ {
            get {
                return Template.__doc__;
            }
        }
      
        public object __call__(CodeContext context, [ParamDictionary]IDictionary<object, object> dictArgs, params object[] args) {
            return _template.__call__(context, dictArgs, args);
        }

        #endregion

        #region ICodeFormattable Members

        string/*!*/ ICodeFormattable.ToCodeString(CodeContext context) {
            return String.Format("<method {0} of {1} objects>",
                PythonOps.StringRepr(Template.Name),
                PythonOps.StringRepr(PythonTypeOps.GetName(DeclaringType)));
        }

        #endregion
    }
}
