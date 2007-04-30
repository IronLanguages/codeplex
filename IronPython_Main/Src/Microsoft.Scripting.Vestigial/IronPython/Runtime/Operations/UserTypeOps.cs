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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;

using System.Diagnostics;

namespace IronPython.Runtime.Operations {
    public static class UserTypeOps {
        public static string ToStringReturnHelper(object o) {
            if (o is string && o != null) {
                return (string)o;
            }
            throw Ops.TypeError("__str__ returned non-string type ({0})", Ops.GetDynamicType(o).Name);
        }

        public static IAttributesCollection SetDictHelper(ref IAttributesCollection iac, IAttributesCollection value) {
            if (System.Threading.Interlocked.CompareExchange<IAttributesCollection>(ref iac, value, null) == null)
                return value;
            return iac;
        }

        public static object GetPropertyHelper(object prop, object instance, SymbolId name) {
            DynamicTypeSlot desc = prop as DynamicTypeSlot;
            if (desc == null) {
                throw Ops.TypeError("Expected property for {0}, but found {1}",
                    name.ToString(), Ops.GetDynamicType(prop).Name);
            }
            object value;
            desc.TryGetValue(DefaultContext.Default, instance, Ops.GetDynamicType(instance), out value);
            return value;
        }

        public static void SetPropertyHelper(object prop, object instance, object newValue, SymbolId name) {
            DynamicTypeSlot desc = prop as DynamicTypeSlot;
            if (desc == null) {
                throw Ops.TypeError("Expected settable property for {0}, but found {1}",
                    name.ToString(), Ops.GetDynamicType(prop).Name);
            }
            desc.TrySetValue(DefaultContext.Default, instance, Ops.GetDynamicType(instance), newValue);
        }

        public static void AddRemoveEventHelper(object method, object instance, DynamicType dt, object eventValue, SymbolId name) {
            object callable = method;
            
            DynamicTypeSlot dts = method as DynamicTypeSlot;
            if(dts != null) {
                if (!dts.TryGetValue(DefaultContext.Default, instance, dt, out callable))
                    throw Ops.AttributeErrorForMissingAttribute(dt.Name, name);
            } 

            if (!Ops.IsCallable(callable)) {
                throw Ops.TypeError("Expected callable value for {0}, but found {1}", name.ToString(),
                    Ops.GetDynamicType(method).Name);
            }

            PythonCalls.Call(callable, eventValue);
        }

        /// <summary>
        /// Object.ToString() displays the CLI type name.  But we want to display the class name (e.g.
        /// '&lt;foo object at 0x000000000000002C&gt;' unless we've overridden __repr__ but not __str__ in 
        /// which case we'll display the result of __repr__.
        /// </summary>
        public static string ToStringHelper(IDynamicObject o) {

            object ret;
            DynamicType ut = o.DynamicType;
            Debug.Assert(ut != null);

            DynamicTypeSlot dts;
            if (ut.TryResolveSlot(DefaultContext.Default, Symbols.Repr, out dts) &&
                dts.TryGetValue(DefaultContext.Default, o, ut, out ret)) {

                string strRet;
                if (ret != null && Converter.TryConvertToString(PythonCalls.Call(ret), out strRet)) return strRet;
                throw Ops.TypeError("__repr__ returned non-string type ({0})", Ops.GetDynamicType(ret).Name);
            }

            return TypeHelpers.ReprMethod(o);
        }

        public static bool TryGetNonInheritedMethodHelper(DynamicType dt, object instance, SymbolId name, out object callTarget) {
            // search MRO for other user-types in the chain that are overriding the method
            foreach(DynamicType type in dt.ResolutionOrder) {
                if (type.IsSystemType) break;           // hit the .NET types, we're done

                if (LookupValue(type, instance, name, out callTarget)) {
                    return true;
                }
            }

            // check instance
            ISuperDynamicObject isdo = instance as ISuperDynamicObject;
            IAttributesCollection iac;
            if (isdo != null && (iac = isdo.Dict) != null) {
                if (iac.TryGetValue(name, out callTarget))
                    return true;
            }

            callTarget = null;
            return false;
        }

        private static bool LookupValue(DynamicType dt, object instance, SymbolId name, out object value) {
            DynamicTypeSlot dts;
            if (dt.TryLookupSlot(DefaultContext.Default, name, out dts) &&
                dts.TryGetValue(DefaultContext.Default, instance, dt, out value)) {
                return true;
            }
            value = null;
            return false;
        }

        public static bool TryGetNonInheritedValueHelper(DynamicType dt, object instance, SymbolId name, out object callTarget) {
            DynamicTypeSlot dts;
            // search MRO for other user-types in the chain that are overriding the method
           foreach(DynamicType type in dt.ResolutionOrder) {
                if (type.IsSystemType) break;           // hit the .NET types, we're done

                if (type.TryLookupSlot(DefaultContext.Default, name, out dts)) {
                    callTarget = dts;
                    return true;
                }
            }
            
            // check instance
            ISuperDynamicObject isdo = instance as ISuperDynamicObject;
            IAttributesCollection iac;
            if (isdo != null && (iac = isdo.Dict) != null) {
                if (iac.TryGetValue(name, out callTarget))
                    return true;
            }

            callTarget = null;
            return false;
        }

        #region IValueEquality Helpers

        public static int GetValueHashCodeHelper(object self) {
            // new-style classes only lookup in slots, not in instance
            // members
            object func;
            if (Ops.GetDynamicType(self).TryGetBoundMember(DefaultContext.Default, self, Symbols.Hash, out func)) {
                return Converter.ConvertToInt32(PythonCalls.Call(func));
            }

            return self.GetHashCode();
        }

        public static bool ValueEqualsHelper(object self, object other) {
            object res = RichEqualsHelper(self, other);
            if (res != Ops.NotImplemented && res != null && res.GetType() == typeof(bool))
                return (bool)res;

            return false;
        }

        private static object RichEqualsHelper(object self, object other) {
            object res;

            if (Ops.GetDynamicType(self).TryInvokeBinaryOperator(DefaultContext.Default, Operators.Equal, self, other, out res))
                return res;

            return Ops.NotImplemented;
        }

        public static bool ValueNotEqualsHelper(object self, object other) {
            object res;
            if (Ops.GetDynamicType(self).TryInvokeBinaryOperator(DefaultContext.Default, Operators.NotEqual, self, other, out res) && 
                res != Ops.NotImplemented &&
                res != null &&
                res.GetType() == typeof(bool))
                return (bool)res;

            return false;
        }

        #endregion
    }
}
