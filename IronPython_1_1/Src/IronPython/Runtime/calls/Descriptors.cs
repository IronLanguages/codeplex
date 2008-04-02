/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Runtime.InteropServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Calls {
    [PythonType("staticmethod")]
    public class StaticMethod : IDescriptor {
        private object func;

        public StaticMethod(object func) {
            this.func = func;
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
    public class ClassMethod : IDescriptor {
        internal object func;

        public ClassMethod(object func) {
            if (!Ops.IsCallable(func))
                throw Ops.TypeError("{0} object is not callable", Ops.StringRepr(Ops.GetDynamicType(func)));
            this.func = func;
        }

        #region IDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(object instance) { return GetAttribute(instance, null); }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            if (owner == null) {
                if (instance == null) throw Ops.TypeError("__get__(None, None) is invalid");
                owner = Ops.GetDynamicType(instance);
            }
            return new Method(func, owner, Ops.GetDynamicType(owner));
        }
        #endregion
    }

    [PythonType("property")]
    public class Property : IDataDescriptor {
        private object fget, fset, fdel, doc;

        public Property([DefaultParameterValueAttribute(null)]object fget,
                        [DefaultParameterValueAttribute(null)]object fset,
                        [DefaultParameterValueAttribute(null)]object fdel,
                        [DefaultParameterValueAttribute(null)]object doc) {
            this.fget = fget; this.fset = fset; this.fdel = fdel; this.doc = doc;
        }

        public object Documentation {
            [PythonName("__doc__")]
            get { return doc; }
            [PythonName("__doc__")]
            set {
                throw Ops.TypeError("'property' object is immutable");
            }
        }

        public object Deleter {
            [PythonName("fdel")]
            get { return fdel; }
            [PythonName("fdel")]
            set {
                throw Ops.TypeError("'property' object is immutable");
            }
        }

        public object Setter {
            [PythonName("fset")]
            get { return fset; }
            [PythonName("fset")]
            set {
                throw Ops.TypeError("'property' object is immutable");
            }
        }

        public object Getter {
            [PythonName("fget")]
            get { return fget; }
            [PythonName("fget")]
            set {
                throw Ops.TypeError("'property' object is immutable");
            }
        }

        #region IDataDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(object instance) { return GetAttribute(instance, null); }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            if (instance == null) return this;
            if (fget != null) return Ops.Call(fget, instance);
            throw Ops.AttributeError("unreadable attribute");
        }

        [PythonName("__set__")]
        public bool SetAttribute(object instance, object value) {
            if (fset != null) {
                Ops.Call(fset, instance, value);
                return true;
            } else {
                if (instance == null) return false;

                throw Ops.AttributeError("readonly attribute");
            }
        }

        [PythonName("__delete__")]
        public bool DeleteAttribute(object instance) {
            if (fdel != null) {
                Ops.Call(fdel, instance);
                return true;
            } else {
                if (instance == null) return false;

                throw Ops.AttributeError("undeletable attribute");
            }
        }
        #endregion
    }

}