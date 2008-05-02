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
using System.Runtime.InteropServices;

using Microsoft.Scripting;

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Calls {
    [PythonSystemType]
    public class staticmethod : PythonTypeSlot {
        private object _func;

        public staticmethod(object func) {
            this._func = func;
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = __get__(instance, PythonOps.ToPythonType(owner));
            return true;
        }

        #region IDescriptor Members

        public object __get__(object instance) { return __get__(instance, null); }

        public object __get__(object instance, object owner) {
            return _func;
        }

        #endregion
    }

    [PythonSystemType]
    public class classmethod : PythonTypeSlot {
        internal object func;

        public classmethod(object func) {
            if (!PythonOps.IsCallable(func))
                throw PythonOps.TypeError("{0} object is not callable", PythonTypeOps.GetName(func));
            this.func = func;
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = __get__(instance, PythonOps.ToPythonType(owner));
            return true;
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

    [PythonSystemType]
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
            _fget = fget; _fset = fset; _fdel = fdel;

                        
            if (doc == null) {
                PythonOps.TryGetBoundAttr(_fget, Symbols.Doc, out doc);
            }
            
            _doc = doc;
            
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = __get__(instance, PythonOps.ToPythonType(owner));
            return true;
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            return __set__(instance, value);
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            return __delete__(instance);
        }

        internal override bool IsSetDescriptor(CodeContext context, PythonType owner) {
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public object __doc__ {
            get { return _doc; }
            set {
                throw PythonOps.TypeError("'property' object is immutable");
            }
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

        public object __get__(object instance) { return __get__(instance, null); }

        public object __get__(object instance, object owner) {
            if (instance == null) return this;
            if (fget != null) return PythonCalls.Call(fget, instance);
            throw PythonOps.AttributeError("unreadable attribute");
        }

        public bool __set__(object instance, object value) {
            if (fset != null) {
                PythonCalls.Call(fset, instance, value);
                return true;
            } else {
                if (instance == null) return false;

                throw PythonOps.AttributeError("readonly attribute");
            }
        }

        public bool __delete__(object instance) {
            if (fdel != null) {
                PythonCalls.Call(fdel, instance);
                return true;
            } else {
                if (instance == null) return false;

                throw PythonOps.AttributeError("undeletable attribute");
            }
        }
    }

}