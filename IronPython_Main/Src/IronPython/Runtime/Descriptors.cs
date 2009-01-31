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
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Runtime.InteropServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {
    [PythonType]
    public class staticmethod : PythonTypeSlot {
        private object _func;

        public staticmethod(object func) {
            this._func = func;
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = __get__(instance, PythonOps.ToPythonType(owner));
            return true;
        }

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        #region IDescriptor Members

        public object __get__(object instance) { return __get__(instance, null); }

        public object __get__(object instance, object owner) {
            return _func;
        }

        #endregion
    }

    [PythonType]
    public class classmethod : PythonTypeSlot {
        internal object func;

        public classmethod(CodeContext/*!*/ context, object func) {
            if (!PythonOps.IsCallable(context, func))
                throw PythonOps.TypeError("{0} object is not callable", PythonTypeOps.GetName(func));
            this.func = func;
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = __get__(instance, PythonOps.ToPythonType(owner));
            return true;
        }

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        #region IDescriptor Members

        public object __get__(object instance) { return __get__(instance, null); }

        public object __get__(object instance, object owner) {
            if (owner == null) {
                if (instance == null) throw PythonOps.TypeError("__get__(None, None) is invalid");
                owner = DynamicHelpers.GetPythonType(instance);
            }
            return new Method(func, owner, DynamicHelpers.GetPythonType(owner));
        }

        #endregion
    }

    [PythonType("property")]
    public class PythonProperty : PythonTypeSlot {
        private object _fget, _fset, _fdel, _doc;

        public PythonProperty() {
        }

        public PythonProperty(params object[] args) {
        }

        public PythonProperty(
            [ParamDictionary]IAttributesCollection dict, params object[] args) {
        }

        public void __init__([DefaultParameterValue(null)]object fget,
                        [DefaultParameterValue(null)]object fset,
                        [DefaultParameterValue(null)]object fdel,
                        [DefaultParameterValue(null)]object doc) {
            _fget = fget; _fset = fset; _fdel = fdel; _doc = doc;
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = __get__(context, instance, PythonOps.ToPythonType(owner));
            return true;
        }

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            return __set__(context, instance, value);
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            return __delete__(context, instance);
        }

        internal override bool IsSetDescriptor(CodeContext context, PythonType owner) {
            return true;
        }
        
        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static object Get__doc__(CodeContext context, PythonProperty self) {
            if (self._doc == null && PythonOps.HasAttr(context, self._fget, (SymbolId)"__doc__")) {
                return PythonOps.GetBoundAttr(context, self._fget, (SymbolId)"__doc__");
            } else if (self._doc == null) {
                System.Console.WriteLine("No attribute __doc__");
            }
            return self._doc;
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static void Set__doc__(PythonProperty self) {
            throw PythonOps.TypeError("'property' object is immutable");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public object fdel {
            get { return _fdel; }
            set {
                throw PythonOps.TypeError("'property' object is immutable");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public object fset {
            get { return _fset; }
            set {
                throw PythonOps.TypeError("'property' object is immutable");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public object fget {
            get { return _fget; }
            set {
                throw PythonOps.TypeError("'property' object is immutable");
            }
        }

        public object __get__(CodeContext/*!*/ context, object instance) { return __get__(context, instance, null); }

        public new object __get__(CodeContext/*!*/ context, object instance, object owner) {
            if (instance == null) {
                return this;
            } else if (fget != null) {
                var site = PythonContext.GetContext(context).PropertyGetSite;

                return site.Target(site, context, fget, instance);
            }
            throw PythonOps.AttributeError("unreadable attribute");
        }

        public bool __set__(CodeContext/*!*/ context, object instance, object value) {
            if (instance == null) {
                return false;
            } else if (fset != null) {
                var site = PythonContext.GetContext(context).PropertySetSite;

                site.Target(site, context, fset, instance, value);
                return true;
            } else {
                throw PythonOps.AttributeError("readonly attribute");
            }
        }

        public new bool __delete__(CodeContext/*!*/ context, object instance) {
            if (fdel != null) {
                var site = PythonContext.GetContext(context).PropertyDeleteSite;

                site.Target(site, context, fdel, instance);
                return true;
            } else if (instance == null) {
                return false;
            } else {
                throw PythonOps.AttributeError("undeletable attribute");
            }
        }

        public PythonProperty getter(object fget) {
            PythonProperty res = new PythonProperty();
            res.__init__(fget, _fset, _fdel, _doc);
            return res;
        }

        public PythonProperty setter(object fset) {
            PythonProperty res = new PythonProperty();
            res.__init__(_fget, fset, _fdel, _doc);
            return res;
        }

        public PythonProperty deleter(object fdel) {
            PythonProperty res = new PythonProperty();
            res.__init__(_fget, _fset, fdel, _doc);
            return res;
        }
    }

}
