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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Scripting.Actions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime {

    [PythonType("instancemethod")]
    public sealed partial class Method : PythonTypeSlot, IWeakReferenceable, IMembersList, IOldDynamicObject, IDynamicObject, ICodeFormattable {
        private readonly object _func;
        private readonly object _inst;
        private readonly object _declaringClass;
        private WeakRefTracker _weakref;

        public Method(object function, object instance, object @class) {
            this._func = function;
            this._inst = instance;
            this._declaringClass = @class;
        }

        public Method(object function, object instance) {
            this._func = function;
            this._inst = instance;
        }

        public string __name__ {
            get { return (string)PythonOps.GetBoundAttr(DefaultContext.Default, _func, Symbols.Name); }
        }

        public string __doc__ {
            get {
                return PythonOps.GetBoundAttr(DefaultContext.Default, _func, Symbols.Doc) as string;
            }
        }

        public object im_func {
            get {
                return _func;
            }
        }

        public object im_self {
            get {
                return _inst;
            }
        }

        public object im_class {
            get {
                // we could have an OldClass (or any other object) here if the user called the ctor directly
                return PythonOps.ToPythonType(_declaringClass as PythonType) ?? _declaringClass;
            }
        }

        private Exception BadSelf(object got) {
            OldClass dt = im_class as OldClass;            

            string firstArg;
            if (got == null) {
                firstArg = "nothing";
            } else {
                firstArg = PythonOps.GetPythonTypeName(got) + " instance";
            }

            return PythonOps.TypeError("unbound method {0}() must be called with {1} instance as first argument (got {2} instead)",
                __name__,
                (dt != null) ? dt.__name__ : im_class,
                firstArg);
        }

        /// <summary>
        /// Validates that the current self object is usable for this method.  
        /// </summary>
        internal object CheckSelf(object self) {
            if (!PythonOps.IsInstance(self, im_class)) throw BadSelf(self);
            return self;
        }
        
        #region Object Overrides
        private string DeclaringClassAsString() {
            if (im_class == null) return "?";
            PythonType dt = im_class as PythonType;
            if (dt != null) return dt.Name;
            OldClass oc = im_class as OldClass;
            if (oc != null) return oc.__name__;
            return im_class.ToString();
        }

        public override bool Equals(object obj) {
            Method other = obj as Method;
            if (other == null) return false;

            return
                PythonOps.EqualRetBool(_inst, other._inst) &&
                PythonOps.EqualRetBool(_func, other._func);
        }

        public override int GetHashCode() {
            if (_inst == null) return PythonOps.Hash(DefaultContext.Default, _func);

            return PythonOps.Hash(DefaultContext.Default, _inst) ^ PythonOps.Hash(DefaultContext.Default, _func);
        }
        
        #endregion

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            return _weakref;
        }

        bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            _weakref = value;
            return true;
        }

        void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
            ((IWeakReferenceable)this).SetWeakRef(value);
        }

        #endregion

        #region Custom member access

        [SpecialName]
        public object GetCustomMember(CodeContext context, string name) {
            if (name == "__module__") {
                // Get the module name from the function and pass that out.  Note that CPython's method has
                // no __module__ attribute and this value can be gotten via a call to method.__getattribute__ 
                // there as well.
                return PythonOps.GetBoundAttr(context, _func, Symbols.Module);                
            }
            
            object value;
            SymbolId symbol = SymbolTable.StringToId(name);
            if (TypeCache.Method.TryGetBoundMember(context, this, symbol, out value) ||       // look on method
                PythonOps.TryGetBoundAttr(context, _func, symbol, out value)) {               // Forward to the func
                return value;
            }

            
            return OperationFailed.Value;
        }

        [SpecialName]
        public void SetMemberAfter(CodeContext context, string name, object value) {
            TypeCache.Method.SetMember(context, this, SymbolTable.StringToId(name), value);
        }

        [SpecialName]
        public void DeleteMember(CodeContext context, string name) {
            TypeCache.Method.DeleteMember(context, this, SymbolTable.StringToId(name));
        }

        IList<object> IMembersList.GetMemberNames(CodeContext context) {
            List ret = TypeCache.Method.GetMemberNames(context);

            ret.AddNoLockNoDups(SymbolTable.IdToString(Symbols.Module));

            PythonFunction pf = _func as PythonFunction;
            if (pf != null) {
                IAttributesCollection dict = pf.func_dict;
                
                // Check the func
                foreach (KeyValuePair<object, object> kvp in dict) {
                    ret.AddNoLockNoDups(kvp.Key);
                }                
            }

            return ret;
        }

        #endregion

        #region PythonTypeSlot Overrides

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            if (this.im_self == null) {
                if (owner == null || owner == im_class || PythonOps.IsSubClass(owner, im_class)) {
                    value = new Method(_func, instance, owner);
                    return true;
                }
            }
            value = this;
            return true;
        }

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        #endregion

        #region IOldDynamicObject Members

        RuleBuilder<T> IOldDynamicObject.GetRule<T>(OldDynamicAction action, CodeContext context, object[] args) {
            Assert.NotNull(action, context, args);

            if (action.Kind == DynamicActionKind.Call) {
                return GetCallRule<T>((OldCallAction)action, context);
            }
            if (action.Kind == DynamicActionKind.DoOperation) {
                return MakeDoOperationRule<T>((OldDoOperationAction)action, context, args);
            }

            // get default rule:
            return null;
        }
        
        private RuleBuilder<T> MakeDoOperationRule<T>(OldDoOperationAction doOperationAction, CodeContext context, object[] args) where T : class {
            switch (doOperationAction.Operation) {
                case Operators.IsCallable:
                    return PythonBinderHelper.MakeIsCallableRule<T>(context, this, true);
            }
            return null;
        }

        private RuleBuilder<T> GetCallRule<T>(OldCallAction action, CodeContext context) where T : class {
            RuleBuilder<T> rule = new RuleBuilder<T>();
            rule.MakeTest(typeof(Method));
            
            Expression[] notNullArgs = GetNotNullInstanceArguments(rule);
            Expression nullSelf;
            if (rule.Parameters.Count != 1) {
                Expression[] nullArgs = GetNullInstanceArguments(rule, action);
                nullSelf = AstUtils.Call(action, typeof(object), ArrayUtils.Insert<Expression>(rule.Context, nullArgs));
            } else {
                // no instance, CheckSelf on null throws.                
                nullSelf = CheckSelf(rule, Ast.Null());
            }

            rule.Target = rule.MakeReturn(context.LanguageContext.Binder,
                Ast.Condition(
                    Ast.NotEqual(
                        Ast.Property(
                            Ast.Convert(rule.Parameters[0], typeof(Method)),
                            typeof(Method).GetProperty("im_self")
                        ),
                        Ast.Null()
                    ),
                    AstUtils.Call(GetNotNullCallAction(context, action), typeof(object), ArrayUtils.Insert<Expression>(rule.Context, notNullArgs)),
                    nullSelf
                )
            );
            return rule;
        }

        private static Expression[] GetNullInstanceArguments<T>(RuleBuilder<T> rule, OldCallAction action) where T : class {
            Debug.Assert(rule.Parameters.Count > 1);

            Expression[] args = ArrayUtils.MakeArray(rule.Parameters);
            args[0] = Ast.Property(
                Ast.Convert(rule.Parameters[0], typeof(Method)),
                typeof(Method).GetProperty("im_func")
            );
            Expression self;

            ArgumentKind firstArgKind = action.Signature.GetArgumentKind(0);

            if (firstArgKind == ArgumentKind.Simple || firstArgKind == ArgumentKind.Instance) {
                self = rule.Parameters[1];
            } else if (firstArgKind != ArgumentKind.List) {
                self = Ast.Constant(null);
            } else {                
                // list, check arg[0] and then return original list.  If not a list,
                // or we have no items, then check against null & throw.
                args[1] =
                    Ast.Comma(
                        CheckSelf<T>(
                            rule,
                            Ast.Condition(
                                Ast.AndAlso(
                                    Ast.TypeIs(rule.Parameters[1], typeof(IList<object>)),
                                    Ast.NotEqual(
                                        Ast.Property(
                                            Ast.Convert(rule.Parameters[1], typeof(ICollection)),
                                            typeof(ICollection).GetProperty("Count")
                                        ),
                                        Ast.Constant(0)
                                    )
                                ),
                                Ast.Call(
                                    Ast.Convert(rule.Parameters[1], typeof(IList<object>)),
                                    typeof(IList<object>).GetMethod("get_Item"),
                                    Ast.Constant(0)
                                ),
                                Ast.Null()
                            )
                        ),
                        rule.Parameters[1]
                    );
                return args;
            }
            
            args[1] = CheckSelf<T>(rule, self);
            return args;
        }

        private static Expression CheckSelf<T>(RuleBuilder<T> rule, Expression self) where T : class {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MethodCheckSelf"),
                Ast.Convert(rule.Parameters[0], typeof(Method)),
                Ast.ConvertHelper(self, typeof(object))
            );
        }

        private static Expression[] GetNotNullInstanceArguments<T>(RuleBuilder<T> rule) where T : class {
            Expression[] args = ArrayUtils.Insert(
                (Expression)Ast.Property(
                    Ast.Convert(rule.Parameters[0], typeof(Method)),
                    typeof(Method).GetProperty("im_func")
                ),
                rule.Parameters);

            args[1] = Ast.Property(
                Ast.Convert(rule.Parameters[0], typeof(Method)),
                typeof(Method).GetProperty("im_self")
            );
            return args;
        }

        private static OldCallAction GetNotNullCallAction(CodeContext context, OldCallAction action) {
            return OldCallAction.Make(PythonContext.GetContext(context).Binder, action.Signature.InsertArgument(new ArgumentInfo(ArgumentKind.Simple)));
        }

        #endregion

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            object name;
            if (!PythonOps.TryGetBoundAttr(context, _func, Symbols.Name, out name)) {
                name = "?";
            }

            if (_inst != null) {
                return string.Format("<bound method {0}.{1} of {2}>",
                    DeclaringClassAsString(),
                    name,
                    PythonOps.Repr(context, _inst));
            } else {
                return string.Format("<unbound method {0}.{1}>", DeclaringClassAsString(), name);
            }
        }

        #endregion

        #region IDynamicObject Members

        MetaObject/*!*/ IDynamicObject.GetMetaObject(Expression/*!*/ parameter) {
            return new Binding.MetaMethod(parameter, Restrictions.Empty, this);
        }

        #endregion
    }
}
