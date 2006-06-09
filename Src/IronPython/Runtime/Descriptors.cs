/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace IronPython.Runtime {
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
        public object fget, fset, fdel, __doc__;

        //public Property(object fget) : this(fget, null, null, null) { }
        //public Property(object fget, object fset) : this(fget, fset, null, null) { }
        //public Property([Optional]object fget, [Optional]object fset, [Optional]object fdel) : this(fget, fset, fdel, null) { }

        public Property([DefaultParameterValueAttribute(null)]object fget,
                        [DefaultParameterValueAttribute(null)]object fset,
                        [DefaultParameterValueAttribute(null)]object fdel,
                        [DefaultParameterValueAttribute(null)]object doc) {
            this.fget = fget; this.fset = fset; this.fdel = fdel; this.__doc__ = doc;
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