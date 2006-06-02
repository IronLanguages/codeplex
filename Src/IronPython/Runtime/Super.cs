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

namespace IronPython.Runtime {
    [PythonType("super")]
    public class Super : IDynamicObject, ICustomAttributes, IDescriptor {
        private static DynamicType SuperType = Ops.GetDynamicTypeFromType(typeof(Super));

        public readonly PythonType __thisclass__;
        public readonly object __self__;
        public readonly object __self_class__;

        public Super(PythonType type) : this(type, null) { }
        public Super(PythonType type, object obj) {
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
            }
        }

        public override string ToString() {
            return string.Format("<super: {0}, {1}>", Ops.StringRepr(__thisclass__), Ops.StringRepr(__self__));
        }

        #region IDynamicObject Members
        public DynamicType GetDynamicType() {
            return Ops.GetDynamicTypeFromType(typeof(Super));
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
            PythonType mroType = __self_class__ as PythonType;
            if (mroType == null) mroType = __thisclass__;
            Tuple mro = mroType.MethodResolutionOrder;

            int lookupType;
            for (lookupType = 0; lookupType < mro.Count; lookupType++) {
                if (mro[lookupType] == __thisclass__) break;                
            }

            
            // then skip our class, and lookup in everything
            // above us until we get a hit.
            lookupType++;   
            while (lookupType < mro.Count) {
                PythonType pt = mro[lookupType] as PythonType;
                if (pt != null) {
                    // new-style class, or reflected type, lookup slot
                    if (pt.TryGetSlot(context, name, out value)) {
                        MethodWrapper mw = value as MethodWrapper;
                        if (mw == null || !mw.IsSuperTypeMethod()) {
                            value = Ops.GetDescriptor(value, __self__, __self_class__);
                            return true;
                        }
                    }
                } else {
                    // old-style class, lookup attribute
                    DynamicType dt = mro[lookupType] as DynamicType;
                    System.Diagnostics.Debug.Assert(dt != null);

                    if (Ops.TryGetAttr(context, dt, name, out value)) {
                        value = Ops.GetDescriptor(value, __self__, __self_class__);
                        return true;
                    }
                }

                lookupType++;
            }
            
            return SuperType.TryGetAttr(context, this, name, out value);
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            SuperType.SetAttr(context, this, name, value);
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            SuperType.DelAttr(context, this, name);
        }

        public List GetAttrNames(ICallerContext context) {
            return SuperType.GetAttrNames(context, this);
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            return SuperType.GetAttrDict(context, this);
        }

        #endregion

        #region IDescriptor Members

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            return new Super(__thisclass__, instance);
        }

        #endregion
    }
}
