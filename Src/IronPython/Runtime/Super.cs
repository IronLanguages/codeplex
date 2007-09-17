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
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Types;

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    [PythonType("super")]
    public class Super : DynamicTypeSlot, ICustomMembers {
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
                if (PythonOps.IsInstance(obj, type)) {
                    this.__thisclass__ = type;
                    this.__self__ = obj;
                    this.__self_class__ = DynamicHelpers.GetDynamicType(obj);
                } else if (dt != null && dt.IsSubclassOf(type)) {
                    this.__thisclass__ = type;
                    this.__self_class__ = obj;
                    this.__self__ = obj;
                } else {
                    throw PythonOps.TypeError("super(type, obj): obj must be an instance or subtype of type {1}, not {0}", DynamicTypeOps.GetName(obj), DynamicTypeOps.GetName(type));
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
                selfRepr = PythonOps.StringRepr(__self__);
            return string.Format("<{0}: {1}, {2}>", DynamicTypeOps.GetName(this), PythonOps.StringRepr(__thisclass__), selfRepr);
        }

        // TODO needed because ICustomMembers is too hard to implement otherwise.  Let's fix that and get rid of this.
        private DynamicType DynamicType {
            get {
                if (GetType() == typeof(Super))
                    return TypeCache.Super;

                ISuperDynamicObject sdo = this as ISuperDynamicObject;
                Debug.Assert(sdo != null);

                return sdo.DynamicType;
            }
        }


        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetBoundCustomMember(context, name, out value);

        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            // first find where we are in the mro...
            DynamicMixin mroType = __self_class__ as DynamicMixin;

            if (mroType != null) { // can be null if the user does super.__new__
                IList<DynamicMixin> mro = mroType.ResolutionOrder;

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
                    mro = __thisclass__.ResolutionOrder;
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

            return DynamicType.TryGetBoundMember(context, this, name, out value);
        }

        private bool TryLookupInBase(CodeContext context, object type, SymbolId name, object self, out object value) {
            DynamicType pt = type as DynamicType;

            DynamicTypeSlot dts;
            object ocObj;
            if (pt == TypeCache.Object ||
                !pt.TryLookupSlot(context, Symbols.Class, out dts)) {
                // new-style class, or reflected type, lookup slot
                if (pt.TryLookupSlot(context, name, out dts) && 
                    dts.TryGetValue(context, self, DescriptorContext, out value)) {
                    return true;
                }
            } else if(dts.TryGetValue(context, null, pt, out ocObj)) {
                // old-style class, lookup attribute                
                OldClass dt = ocObj as OldClass;
                Debug.Assert(dt != null);

                if (PythonOps.TryGetBoundAttr(context, dt, name, out value)) {
                    value = dt.GetOldStyleDescriptor(context, value, self, DescriptorContext);
                    return true;
                }
            }
            value = null;
            return false;
        }

        private DynamicType DescriptorContext {
            get {
                if (!DynamicHelpers.GetDynamicType(__self__).IsSubclassOf(__thisclass__)) {
                    return __thisclass__;
                }

                DynamicType dt = __self_class__ as DynamicType;
                if (dt != null) return dt;

                return ((OldClass)__self_class__).TypeObject;
            }
        }
        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            DynamicType.SetMember(context, this, name, value);
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            DynamicType.DeleteMember(context, this, name);
            return true;
        }

        public IList<object> GetCustomMemberNames(CodeContext context) {
            List res = new List();
            foreach (SymbolId si in DynamicType.GetMemberNames(context, this)) {
                res.AddNoLock(si.ToString());
            }
            return res;
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            return DynamicType.GetMemberDictionary(context, this).AsObjectKeyedDictionary();
        }

        #endregion

        #region IDescriptor Members

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            DynamicType selfType = DynamicType;

            if (selfType == TypeCache.Super) {
                Super res = new Super();
                res.Initialize(__thisclass__, instance);
                return res;
            }

            return PythonCalls.Call(selfType, __thisclass__, instance);
        }

        #endregion

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = GetAttribute(instance, owner);
            return true;
        }
    }
}
