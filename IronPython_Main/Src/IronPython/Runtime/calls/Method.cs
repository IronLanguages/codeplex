/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Compiler;
using IronPython.Runtime.Types;
using IronPython.Hosting;


namespace IronPython.Runtime.Calls {
    [PythonType("instancemethod")]
    public sealed partial class Method : PythonTypeSlot, IWeakReferenceable, ICustomMembers, IDynamicObject {
        private object _func;
        private object _inst;
        private object _declaringClass;
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

        public string Name {
            [PythonName("__name__")]
            get { return (string)PythonOps.GetBoundAttr(DefaultContext.Default, _func, Symbols.Name); }
        }

        public string Documentation {
            [PythonName("__doc__")]
            get {
                return PythonOps.GetBoundAttr(DefaultContext.Default, _func, Symbols.Doc) as string;
            }
        }

        public object Function {
            [PythonName("im_func")]
            get {
                return _func;
            }
        }

        public object Self {
            [PythonName("im_self")]
            get {
                return _inst;
            }
        }

        public object DeclaringClass {
            [PythonName("im_class")]
            get {
                return PythonOps.ToPythonType((PythonType)_declaringClass);
            }
        }

        private Exception BadSelf(object got) {
            OldClass dt = DeclaringClass as OldClass;            

            string firstArg;
            if (got == null) {
                firstArg = "nothing";
            } else {
                firstArg = PythonOps.GetPythonTypeName(got) + " instance";
            }

            return PythonOps.TypeError("unbound method {0}() must be called with {1} instance as first argument (got {2} instead)",
                Name,
                (dt != null) ? dt.Name : DeclaringClass,
                firstArg);
        }

        /// <summary>
        /// Validates that the current self object is usable for this method.  Called from generated code.
        /// </summary>
        public object CheckSelf(object self) {
            if (!PythonOps.IsInstance(self, DeclaringClass)) throw BadSelf(self);
            return self;
        }

        private object[] AddInstToArgs(object[] args) {
            if (_inst == null) {
                if (args.Length < 1) throw BadSelf(null);
                CheckSelf(args[0]);
                return args;
            }

            object[] nargs = new object[args.Length + 1];
            args.CopyTo(nargs, 1);
            nargs[0] = _inst;
            return nargs;
        }

        #region Object Overrides
        private string DeclaringClassAsString() {
            if (DeclaringClass == null) return "?";
            PythonType dt = DeclaringClass as PythonType;
            if (dt != null) return PythonTypeOps.GetName(dt);
            OldClass oc = DeclaringClass as OldClass;
            if (oc != null) return oc.Name;
            return DeclaringClass.ToString();
        }

        public override string ToString() {
            if (_inst != null) {
                return string.Format("<bound method {0}.{1} of {2}>",
                    DeclaringClassAsString(),
                    PythonOps.GetBoundAttr(DefaultContext.Default, _func, Symbols.Name),
                    PythonOps.StringRepr(_inst));
            } else {
                return string.Format("<unbound method {0}.{1}>", DeclaringClassAsString(), Name);
            }
        }

        public override bool Equals(object obj) {
            Method other = obj as Method;
            if (other == null) return false;

            return other._inst == _inst && other._func == _func;
        }

        public override int GetHashCode() {
            if (_inst == null) return _func.GetHashCode();

            return _inst.GetHashCode() ^ _func.GetHashCode();
        }
        #endregion

        #region IDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(object instance) { return GetAttribute(instance, DeclaringClass); }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            if (this.Self == null) {
                if (owner == DeclaringClass || PythonOps.IsSubClass((PythonType)owner, DeclaringClass)) {
                    return new Method(_func, instance, owner);
                }
            }
            return this;
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

        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetBoundCustomMember(context, name, out value);
        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            if (name == Symbols.Module) {
                // Get the module name from the function and pass that out.  Note that CPython's method has
                // no __module__ attribute and this value can be gotten via a call to method.__getattribute__ 
                // there as well.
                value = PythonOps.GetBoundAttr(context, _func, Symbols.Module);
                return true;
            }

            if (TypeCache.Method.TryGetBoundMember(context, this, name, out value)) return true;

            // Forward to the func
            return PythonOps.TryGetBoundAttr(context, _func, name, out value);
        }

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            TypeCache.Method.SetMember(context, this, name, value);
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            TypeCache.Method.DeleteMember(context, this, name);
            return true;
        }

        public IList<object> GetCustomMemberNames(CodeContext context) {
            List ret = new List();
            foreach(SymbolId si in TypeCache.Method.GetMemberNames(context, this)) {
                ret.AddNoLock(SymbolTable.IdToString(si));
            }

            ret.AddNoLockNoDups(SymbolTable.IdToString(Symbols.Module));

            IAttributesCollection dict = ((PythonFunction)_func).Dictionary;
            if (dict != null) {
                // Check the func
                foreach (KeyValuePair<object, object> kvp in ((PythonFunction)_func).Dictionary) {
                    ret.AddNoLockNoDups(kvp.Key);
                }
            }

            return ret;
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            return TypeCache.Method.GetMemberDictionary(context, this).AsObjectKeyedDictionary();
        }

        #endregion

        #region PythonTypeSlot Overrides

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = GetAttribute(instance, owner);
            return true;
        }

        #endregion

        #region IDynamicObject Members

        LanguageContext IDynamicObject.LanguageContext {
            get { return DefaultContext.Default.LanguageContext; }
        }

        StandardRule<T> IDynamicObject.GetRule<T>(DynamicAction action, CodeContext context, object[] args) {
            Assert.NotNull(action, context, args);

            if (action.Kind == DynamicActionKind.Call) {
                return GetCallRule<T>((CallAction)action, context);
            }
            
            // get default rule:
            return null;
        }

        private StandardRule<T> GetCallRule<T>(CallAction action, CodeContext context) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(Method));
            
            Expression[] notNullArgs = GetNotNullInstanceArguments(rule);
            Expression nullSelf;
            if (rule.Parameters.Length != 1) {
                Expression[] nullArgs = GetNullInstanceArguments(rule, action);
                nullSelf = Ast.Action.Call(action, typeof(object), nullArgs);
            } else {
                // no instance, CheckSelf on null throws.                
                nullSelf = CheckSelf(rule, Ast.Null());
            }

            rule.SetTarget(
                rule.MakeReturn(context.LanguageContext.Binder,
                    Ast.Condition(
                        Ast.NotEqual(
                            Ast.ReadProperty(
                                Ast.Convert(rule.Parameters[0], typeof(Method)),
                                typeof(Method).GetProperty("Self")
                            ),
                            Ast.Null()
                        ),
                        Ast.Action.Call(GetNotNullCallAction(action), typeof(object), notNullArgs),
                        nullSelf
                    )
                )
            );
            return rule;
        }

        private static Expression[] GetNullInstanceArguments<T>(StandardRule<T> rule, CallAction action) {
            Debug.Assert(rule.Parameters.Length > 1);

            Expression[] args = (Expression[])rule.Parameters.Clone();
            args[0] = Ast.ReadProperty(
                Ast.Convert(rule.Parameters[0], typeof(Method)),
                typeof(Method).GetProperty("Function")
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
                                        Ast.ReadProperty(
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

        private static Expression CheckSelf<T>(StandardRule<T> rule, Expression self) {
            return Ast.Call(
                Ast.Convert(rule.Parameters[0], typeof(Method)),
                typeof(Method).GetMethod("CheckSelf"),
                Ast.ConvertHelper(self, typeof(object))
            );
        }

        private static Expression[] GetNotNullInstanceArguments<T>(StandardRule<T> rule) {
            Expression[] args = ArrayUtils.Insert(
                (Expression)Ast.ReadProperty(
                    Ast.Convert(rule.Parameters[0], typeof(Method)),
                    typeof(Method).GetProperty("Function")
                ),
                rule.Parameters);

            args[1] = Ast.ReadProperty(
                Ast.Convert(rule.Parameters[0], typeof(Method)),
                typeof(Method).GetProperty("Self")
            );
            return args;
        }

        private static CallAction GetNotNullCallAction(CallAction action) {
            return CallAction.Make(action.Signature.InsertArgument(new ArgumentInfo(ArgumentKind.Simple)));
        }

        #endregion
    }
}
