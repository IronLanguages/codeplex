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
using System.Scripting;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Generation;
using System.Scripting.Runtime;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    using Ast = System.Linq.Expressions.Expression;

    [PythonSystemType("method_descriptor")]
    public sealed class BuiltinMethodDescriptor : PythonTypeSlot, IOldDynamicObject, IDynamicObject, ICodeFormattable {
        internal readonly BuiltinFunction/*!*/ _template;

        internal BuiltinMethodDescriptor(BuiltinFunction/*!*/ function) {
            _template = function;
        }

        #region Internal APIs

        internal object UncheckedGetAttribute(object instance) {
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

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        internal override bool TryGetBoundValue(CodeContext context, object instance, PythonType owner, out object value) {
            return TryGetValue(context, instance, owner, out value);
        }

        internal BuiltinFunction/*!*/ Template {
            get { return _template; }
        }

        internal Type/*!*/ DeclaringType {
            get {
                return _template.DeclaringType;
            }
        }

        internal static void CheckSelfWorker(CodeContext/*!*/ context, object self, BuiltinFunction template) {
            // to a fast check on the CLR types, if they match we can avoid the slower
            // check that involves looking up dynamic types. (self can be null on
            // calls like set.add(None) 
            Type selfType = CompilerHelpers.GetType(self);
            if (selfType != template.DeclaringType && !template.DeclaringType.IsAssignableFrom(selfType)) {
                // if a conversion exists to the type allow the call.
                context.LanguageContext.Binder.Convert(self, template.DeclaringType);
            }
        }

        internal override bool IsAlwaysVisible {
            get {
                return _template.IsAlwaysVisible;
            }
        }

        #endregion

        #region Private Helpers

        private void CheckSelf(CodeContext/*!*/ context, object self) {
            if ((_template.FunctionType & FunctionType.FunctionMethodMask) == FunctionType.Method) {
                CheckSelfWorker(context, self, _template);
            }
        }

        #endregion

        #region IOldDynamicObject Members

        RuleBuilder<T> IOldDynamicObject.GetRule<T>(OldDynamicAction action, CodeContext context, object[] args) {
            if (action.Kind == DynamicActionKind.Call) {
                return MakeCallRule<T>((OldCallAction)action, context, args);
            } else if (action.Kind == DynamicActionKind.DoOperation) {
                return MakeDoOperationRule<T>((OldDoOperationAction)action, context, args);
            }
            return null;
        }

        private RuleBuilder<T> MakeDoOperationRule<T>(OldDoOperationAction doOperationAction, CodeContext context, object[] args) where T : class {
            switch (doOperationAction.Operation) {
                case Operators.IsCallable:
                    return PythonBinderHelper.MakeIsCallableRule<T>(context, this, true);
                case Operators.CallSignatures:
                    return IronPython.Runtime.Binding.PythonDoOperationBinderHelper<T>.MakeCallSignatureRule(context.LanguageContext.Binder, Template.Targets, DynamicHelpers.GetPythonType(args[0]));
            }
            return null;
        }

        private RuleBuilder<T> MakeCallRule<T>(OldCallAction action, CodeContext context, object[] args) where T : class {
            CallBinderHelper<T, OldCallAction> helper = new CallBinderHelper<T, OldCallAction>(context, action, args, Template.Targets, Template.Level, Template.IsReversedOperator);
            RuleBuilder<T> rule = helper.MakeRule();
            if (Template.IsBinaryOperator && rule.IsError && args.Length == 3) { // 1 built-in method descriptor + 2 args
                // BinaryOperators return NotImplemented on failure.
                rule.Target = rule.MakeReturn(context.LanguageContext.Binder, Ast.Property(null, typeof(PythonOps), "NotImplemented"));
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

        public object __call__(CodeContext context, SiteLocalStorage<CallSite<DynamicSiteTarget<CodeContext, object, object[], IAttributesCollection, object>>> storage, [ParamDictionary]IAttributesCollection dictArgs, params object[] args) {
            return _template.__call__(context, storage, dictArgs, args);
        }

        public PythonType/*!*/ __objclass__ {
            get {
                return DynamicHelpers.GetPythonTypeFromType(_template.DeclaringType);
            }
        }

        public int __cmp__(object other) {
            BuiltinMethodDescriptor bmd = other as BuiltinMethodDescriptor;
            if (bmd == null) {
                throw PythonOps.TypeError("instancemethod.__cmp__(x,y) requires y to be a 'instancemethod', not a {0}", PythonTypeOps.GetName(other));
            }

            long result = PythonOps.Id(__objclass__) - PythonOps.Id(bmd.__objclass__);
            if (result != 0) {
                return (result > 0) ? 1 : -1;
            }
            return (int)StringOps.__cmp__(__name__, bmd.__name__);
        }

        #endregion

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return String.Format("<method {0} of {1} objects>",
                PythonOps.StringRepr(Template.Name),
                PythonOps.StringRepr(DynamicHelpers.GetPythonTypeFromType(DeclaringType).Name));
        }

        #endregion

        #region IDynamicObject Members

        MetaObject/*!*/ IDynamicObject.GetMetaObject(Expression/*!*/ parameter) {
            return new Binding.MetaBuiltinMethodDescriptor(parameter, Restrictions.Empty, this);
        }

        #endregion
    }
}
