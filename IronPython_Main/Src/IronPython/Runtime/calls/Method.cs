/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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

using IronPython.Runtime.Operations;
using IronPython.Compiler;
using IronPython.Runtime.Types;
using IronPython.Hosting;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace IronPython.Runtime.Calls {
    [PythonType("instancemethod")]
    public sealed partial class Method : FastCallable, IFancyCallable, IWeakReferenceable, ICustomMembers, IDynamicObject {
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
                return PythonOps.ToPythonType((DynamicType)_declaringClass);
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

        private object CheckSelf(object self) {
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

        [OperatorMethod]
        public override object Call(CodeContext context, params object[] args) {
            FastCallable fc = _func as FastCallable;
            if (fc != null) {
                if (_inst != null) {
                    return fc.CallInstance(context, _inst, args);
                } else {
                    if (args.Length > 0) CheckSelf(args[0]);
                    return fc.Call(context, args);
                }
            }
            return PythonOps.CallWithContext(context, _func, AddInstToArgs(args));
        }

        public override object CallInstance(CodeContext context, object instance, params object[] args) {
            FastCallable fc = _func as FastCallable;
            if (fc != null) {
                if (_inst != null) return fc.CallInstance(context, instance, AddInstToArgs(args));
                else return fc.CallInstance(context, instance, args); //??? check instance type
            }
            return PythonOps.CallWithContext(context, _func, PrependInstance(instance, AddInstToArgs(args)));
        }

        [OperatorMethod]
        public object Call(CodeContext context, object[] args, string[] names) {
            return PythonOps.CallWithKeywordArgs(context, _func, AddInstToArgs(args), names);
        }

        #region Object Overrides
        private string DeclaringClassAsString() {
            if (DeclaringClass == null) return "?";
            DynamicType dt = DeclaringClass as DynamicType;
            if (dt != null) return DynamicTypeOps.GetName(dt);
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
                if (owner == DeclaringClass || PythonOps.IsSubClass((DynamicType)owner, DeclaringClass)) {
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
            return PythonOps.TryGetAttr(context, _func, name, out value);
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

        #region DynamicTypeSlot Overrides

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = GetAttribute(instance, owner);
            return true;
        }

        #endregion

        #region IDynamicObject Members

        LanguageContext IDynamicObject.LanguageContext {
            get { return DefaultContext.Default.LanguageContext; }
        }

        StandardRule<T> IDynamicObject.GetRule<T>(Action action, CodeContext context, object[] args) {
            Utils.Assert.NotNull(action, context, args);
            
            // get default rule:
            return null;
        }

        #endregion
    }
}
