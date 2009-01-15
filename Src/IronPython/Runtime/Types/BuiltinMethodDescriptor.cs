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
using Microsoft.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using Microsoft.Scripting;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Ast = Microsoft.Linq.Expressions.Expression;

namespace IronPython.Runtime.Types {

    [PythonType("method_descriptor")]
    public sealed class BuiltinMethodDescriptor : PythonTypeSlot, IDynamicObject, ICodeFormattable {
        internal readonly BuiltinFunction/*!*/ _template;

        internal BuiltinMethodDescriptor(BuiltinFunction/*!*/ function) {
            _template = function;
        }

        #region Internal APIs

        internal object UncheckedGetAttribute(object instance) {
            return _template.BindToInstance(instance);
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

        internal override Expression/*!*/ MakeGetExpression(PythonBinder/*!*/ binder, Expression/*!*/ codeContext, Expression instance, Expression/*!*/ owner, Expression/*!*/ error) {
            if (instance != null) {
                return Ast.Call(
                    typeof(PythonOps).GetMethod("MakeBoundBuiltinFunction"),
                    Ast.Constant(_template),
                    instance
                );
            }

            return Ast.Constant(this);
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

        public Type/*!*/ DeclaringType {
            [PythonHidden]
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

        public object __call__(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], IAttributesCollection, object>>> storage, [ParamDictionary]IAttributesCollection dictArgs, params object[] args) {
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
            return String.Format("<method '{0}' of '{1}' objects>",
                Template.Name,
                DynamicHelpers.GetPythonTypeFromType(DeclaringType).Name);
        }

        #endregion

        #region IDynamicObject Members

        DynamicMetaObject/*!*/ IDynamicObject.GetMetaObject(Expression/*!*/ parameter) {
            return new Binding.MetaBuiltinMethodDescriptor(parameter, BindingRestrictions.Empty, this);
        }

        #endregion
    }
}
