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
using System.Runtime.InteropServices;
using Microsoft.Scripting;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Calls {
    [PythonType("staticmethod")]
    public class StaticMethod : DynamicTypeSlot {
        private object func;

        public StaticMethod(object func) {
            this.func = func;
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = GetAttribute(instance, PythonOps.ToPythonType(owner));
            return true;
        }

        #region IDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(object instance) { return GetAttribute(instance, null); }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            return func;
        }
        #endregion
    }

    [PythonType("classmethod")]
    public class ClassMethod : DynamicTypeSlot {
        internal object func;

        public ClassMethod(object func) {
            if (!PythonOps.IsCallable(func))
                throw PythonOps.TypeError("{0} object is not callable", DynamicTypeOps.GetName(func));
            this.func = func;
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = GetAttribute(instance, PythonOps.ToPythonType(owner));
            return true;
        }

        #region IDescriptor Members

        [PythonName("__get__")]
        public object GetAttribute(object instance) { return GetAttribute(instance, null); }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            if (owner == null) {
                if (instance == null) throw PythonOps.TypeError("__get__(None, None) is invalid");
                owner = DynamicHelpers.GetDynamicType(instance);
            }
            return new Method(func, owner, DynamicHelpers.GetDynamicType(owner));
        }
        #endregion
    }

    [PythonType("property")]
    public class PythonProperty : DynamicTypeSlot {
        private object fget, fset, fdel, doc;

        public PythonProperty([DefaultParameterValueAttribute(null)]object fget,
                        [DefaultParameterValueAttribute(null)]object fset,
                        [DefaultParameterValueAttribute(null)]object fdel,
                        [DefaultParameterValueAttribute(null)]object doc) {
            this.fget = fget; this.fset = fset; this.fdel = fdel; this.doc = doc;
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = GetAttribute(instance, PythonOps.ToPythonType(owner));
            return true;
        }

        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            return SetAttribute(instance, value);
        }

        public override bool TryDeleteValue(CodeContext context, object instance, DynamicMixin owner) {
            return DeleteAttribute(instance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public object Documentation {
            [PythonName("__doc__")]
            get { return doc; }
            [PythonName("__doc__")]
            set {
                throw PythonOps.TypeError("'property' object is immutable");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public object Deleter {
            [PythonName("fdel")]
            get { return fdel; }
            [PythonName("fdel")]
            set {
                throw PythonOps.TypeError("'property' object is immutable");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public object Setter {
            [PythonName("fset")]
            get { return fset; }
            [PythonName("fset")]
            set {
                throw PythonOps.TypeError("'property' object is immutable");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public object Getter {
            [PythonName("fget")]
            get { return fget; }
            [PythonName("fget")]
            set {
                throw PythonOps.TypeError("'property' object is immutable");
            }
        }

        #region IDataDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(object instance) { return GetAttribute(instance, null); }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            if (instance == null) return this;
            if (fget != null) return PythonCalls.Call(fget, instance);
            throw PythonOps.AttributeError("unreadable attribute");
        }

        [PythonName("__set__")]
        public bool SetAttribute(object instance, object value) {
            if (fset != null) {
                PythonCalls.Call(fset, instance, value);
                return true;
            } else {
                if (instance == null) return false;

                throw PythonOps.AttributeError("readonly attribute");
            }
        }

        [PythonName("__delete__")]
        public bool DeleteAttribute(object instance) {
            if (fdel != null) {
                PythonCalls.Call(fdel, instance);
                return true;
            } else {
                if (instance == null) return false;

                throw PythonOps.AttributeError("undeletable attribute");
            }
        }
        #endregion
    }

}