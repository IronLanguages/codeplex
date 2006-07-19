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
using System.Text;
using System.Collections.Generic;

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using System.Diagnostics;

namespace IronPython.Runtime {
    [PythonType("super")]
    public class Super : IDynamicObject, ICustomAttributes, IDescriptor {
        private DynamicType __thisclass__;
        private object __self__;
        private object __self_class__;

        public Super() {
        }

        [PythonName("__init__")]
        public void Initialize(DynamicType type) {
            Initialize(type, null);
        }

        [PythonName("__init__")]
        public void Initialize(DynamicType type, object obj) {
            if (obj != null) {
                DynamicType dt = obj as DynamicType;
                if (Modules.Builtin.IsInstance(obj, type)) {
                    this.__thisclass__ = type;
                    this.__self__ = obj;
                    this.__self_class__ = Ops.GetDynamicType(obj);
                } else if (dt != null && dt.IsSubclassOf(type)) {
                    this.__thisclass__ = type;
                    this.__self_class__ = obj;
                    this.__self__ = obj;
                } else {
                    throw Ops.TypeError("super(type, obj): obj must be an instance or subtype of type, not {0}", Ops.GetDynamicType(obj));
                }
            } else {
                this.__thisclass__ = type;
                this.__self__ = null;
                this.__self_class__ = null;
            }
        }

        public DynamicType ThisClass {
            [PythonName("__thisclass__")]
            get { return __thisclass__; }
        }

        public object Self {
            [PythonName("__self__")]
            get { return __self__; }
        }

        public object SelfClass {
            [PythonName("__self_class__")]
            get { return __self_class__; }
        }

        public override string ToString() {
            string selfRepr;
            if (__self__ == this)
                selfRepr = "<super object>";
            else
                selfRepr = Ops.StringRepr(__self__);
            return string.Format("<{0}: {1}, {2}>", Ops.GetDynamicType(this).Name, Ops.StringRepr(__thisclass__), selfRepr);
        }

        #region IDynamicObject Members
        public DynamicType GetDynamicType() {
            if (GetType() == typeof(Super))
                return TypeCache.Super;

            ISuperDynamicObject sdo = this as ISuperDynamicObject;
            Debug.Assert(sdo != null);

            return sdo.GetDynamicType();
        }
        #endregion

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            ICustomBaseAccess baseAccess = __self__ as ICustomBaseAccess;
            if (baseAccess != null && baseAccess.TryGetBaseAttr(context, name, out value)) {
                value = Ops.GetDescriptor(value, __self__, this);
                return true;
            }

            // first find where we are in the mro...
            DynamicType mroType = __self_class__ as DynamicType;

            if (mroType != null) { // can be null if the user does super.__new__
                Tuple mro = mroType.MethodResolutionOrder;
                
                int lookupType;
                bool foundThis = false;
                for (lookupType = 0; lookupType < mro.Count; lookupType++) {
                    if (mro[lookupType] == __thisclass__) {
                        foundThis = true;
                        break;
                    }
                }

                if (!foundThis) {
                    // __self__ is not a subclass of __thisclass__, we need to
                    // search __thisclass__'s mro and return a method from one
                    // of it's bases.
                    lookupType = 0;
                    mro = __thisclass__.MethodResolutionOrder;
                }

                // if we're super on a class then we have no self.
                object self = __self__ == __self_class__ ? null : __self__;

                // then skip our class, and lookup in everything
                // above us until we get a hit.
                lookupType++;
                while (lookupType < mro.Count) {
                    if (TryLookupInBase(context, mro[lookupType], name, self, out value))
                        return true;

                    lookupType++;
                }
            }

            return GetDynamicType().TryGetAttr(context, this, name, out value);
        }

        private bool TryLookupInBase(ICallerContext context, object type, SymbolId name, object self, out object value) {
            DynamicType pt = type as DynamicType;           

            if (pt != null) {
                // new-style class, or reflected type, lookup slot
                if (pt.TryGetSlot(context, name, out value)) {
                    MethodWrapper mw = value as MethodWrapper;
                    if (mw == null || !mw.IsSuperTypeMethod()) {
                        value = Ops.GetDescriptor(value, self, DescriptorContext);
                        return true;
                    }
                }
            } else {
                // old-style class, lookup attribute
                OldClass dt = type as OldClass;
                System.Diagnostics.Debug.Assert(dt != null);

                if (Ops.TryGetAttr(context, dt, name, out value)) {
                    value = Ops.GetDescriptor(value, self, DescriptorContext);
                    return true;
                }
            }
            value = null;
            return false;
        }

        private object DescriptorContext {
            get {
                if(!Ops.GetDynamicType(__self__).IsSubclassOf(__thisclass__)) {
                    return __thisclass__;
                }

                return __self_class__;
            }
        }
        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            GetDynamicType().SetAttr(context, this, name, value);            
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            GetDynamicType().DelAttr(context, this, name);
        }

        public List GetAttrNames(ICallerContext context) {
            return GetDynamicType().GetAttrNames(context, this);
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            return GetDynamicType().GetAttrDict(context, this);
        }

        #endregion

        #region IDescriptor Members

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            DynamicType selfType = GetDynamicType();

            if (selfType == TypeCache.Super) {
                Super res = new Super();
                res.Initialize(__thisclass__, instance);
                return res;
            }

            return selfType.Call(DefaultContext.Default, __thisclass__, instance);
        }

        #endregion
    }
}
