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
using Microsoft.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using Microsoft.Scripting;
using System.Threading;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations {
    
    // These operations get linked into all new-style classes. 
    public static class UserTypeOps {
        public static string ToStringReturnHelper(object o) {
            if (o is string && o != null) {
                return (string)o;
            }
            throw PythonOps.TypeError("__str__ returned non-string type ({0})", PythonTypeOps.GetName(o));
        }

        public static IAttributesCollection SetDictHelper(ref IAttributesCollection iac, IAttributesCollection value) {
            if (System.Threading.Interlocked.CompareExchange<IAttributesCollection>(ref iac, value, null) == null)
                return value;
            return iac;
        }

        public static object GetPropertyHelper(object prop, object instance, SymbolId name) {
            PythonTypeSlot desc = prop as PythonTypeSlot;
            if (desc == null) {
                throw PythonOps.TypeError("Expected property for {0}, but found {1}",
                    name.ToString(), DynamicHelpers.GetPythonType(prop).Name);
            }
            object value;
            desc.TryGetValue(DefaultContext.Default, instance, DynamicHelpers.GetPythonType(instance), out value);
            return value;
        }

        public static void SetPropertyHelper(object prop, object instance, object newValue, SymbolId name) {
            PythonTypeSlot desc = prop as PythonTypeSlot;
            if (desc == null) {
                throw PythonOps.TypeError("Expected settable property for {0}, but found {1}",
                    name.ToString(), DynamicHelpers.GetPythonType(prop).Name);
            }
            desc.TrySetValue(DefaultContext.Default, instance, DynamicHelpers.GetPythonType(instance), newValue);
        }

        public static void SetFinalizerWorker(ref WeakRefTracker tracker, WeakRefTracker newVal) {
            if (Interlocked.CompareExchange(ref tracker, newVal, null) != null) {
                GC.SuppressFinalize(newVal);
            }
        }

        public static void AddRemoveEventHelper(object method, object instance, PythonType dt, object eventValue, SymbolId name) {
            object callable = method;

            // TODO: dt gives us a PythonContext which we should use

            PythonTypeSlot dts = method as PythonTypeSlot;
            if (dts != null) {
                if (!dts.TryGetValue(DefaultContext.Default, instance, dt, out callable))
                    throw PythonOps.AttributeErrorForMissingAttribute(dt.Name, name);
            }

            if (!PythonOps.IsCallable(DefaultContext.Default, callable)) {
                throw PythonOps.TypeError("Expected callable value for {0}, but found {1}", name.ToString(),
                    PythonTypeOps.GetName(method));
            }

            PythonCalls.Call(callable, eventValue);
        }

        public static DynamicMetaObject/*!*/ GetMetaObjectHelper(IPythonObject self, Expression/*!*/ parameter, DynamicMetaObject baseMetaObject) {
            return new Binding.MetaUserObject(parameter, BindingRestrictions.Empty, baseMetaObject, self);
        }

        public static bool TryGetMixedNewStyleOldStyleSlot(CodeContext context, object instance, SymbolId name, out object value) {
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                IAttributesCollection iac = sdo.Dict;
                if (iac != null && iac.TryGetValue(name, out value)) {
                    return true;
                }
            }

            PythonType dt = DynamicHelpers.GetPythonType(instance);

            foreach (PythonType type in dt.ResolutionOrder) {
                PythonTypeSlot dts;
                if (type != TypeCache.Object && type.OldClass != null) {
                    // we're an old class, check the old-class way
                    OldClass oc = type.OldClass;

                    if (oc.TryGetBoundCustomMember(context, name, out value)) {
                        value = oc.GetOldStyleDescriptor(context, value, instance, oc);
                        return true;
                    }
                } else if (type.TryLookupSlot(context, name, out dts)) {
                    // we're a dynamic type, check the dynamic type way
                    return dts.TryGetValue(context, instance, dt, out value);
                }
            }

            value = null;
            return false;
        }

        public static object SetDictionaryValue(IPythonObject self, SymbolId name, object value) {
            IAttributesCollection dict = GetDictionary(self);

            return dict[name] = value;
        }

        public static void RemoveDictionaryValue(IPythonObject self, SymbolId name) {
            IAttributesCollection dict = self.Dict;
            if (dict != null) {
                if (dict.Remove(name)) {
                    return;
                }
            }

            throw PythonOps.AttributeErrorForMissingAttribute(self.PythonType, name);
        }

        internal static IAttributesCollection GetDictionary(IPythonObject self) {
            IAttributesCollection dict = self.Dict;
            if (dict == null) {
                dict = self.SetDict(PythonDictionary.MakeSymbolDictionary());
            }
            return dict;
        }

        /// <summary>
        /// Object.ToString() displays the CLI type name.  But we want to display the class name (e.g.
        /// '&lt;foo object at 0x000000000000002C&gt;' unless we've overridden __repr__ but not __str__ in 
        /// which case we'll display the result of __repr__.
        /// </summary>
        public static string ToStringHelper(IPythonObject o) {
            return ObjectOps.__str__(DefaultContext.Default, o);
        }

        public static bool TryGetNonInheritedMethodHelper(PythonType dt, object instance, SymbolId name, out object callTarget) {
            // search MRO for other user-types in the chain that are overriding the method
            foreach (PythonType type in dt.ResolutionOrder) {
                if (type.IsSystemType) break;           // hit the .NET types, we're done

                if (LookupValue(type, instance, name, out callTarget)) {
                    return true;
                }
            }

            // check instance
            IPythonObject isdo = instance as IPythonObject;
            IAttributesCollection iac;
            if (isdo != null && (iac = isdo.Dict) != null) {
                if (iac.TryGetValue(name, out callTarget))
                    return true;
            }

            callTarget = null;
            return false;
        }

        private static bool LookupValue(PythonType dt, object instance, SymbolId name, out object value) {
            PythonTypeSlot dts;
            if (dt.TryLookupSlot(DefaultContext.Default, name, out dts) &&
                dts.TryGetValue(DefaultContext.Default, instance, dt, out value)) {
                return true;
            }
            value = null;
            return false;
        }

        public static bool TryGetNonInheritedValueHelper(PythonType dt, object instance, SymbolId name, out object callTarget) {
            PythonTypeSlot dts;
            // search MRO for other user-types in the chain that are overriding the method
            foreach (PythonType type in dt.ResolutionOrder) {
                if (type.IsSystemType) break;           // hit the .NET types, we're done

                if (type.TryLookupSlot(DefaultContext.Default, name, out dts)) {
                    callTarget = dts;
                    return true;
                }
            }

            // check instance
            IPythonObject isdo = instance as IPythonObject;
            IAttributesCollection iac;
            if (isdo != null && (iac = isdo.Dict) != null) {
                if (iac.TryGetValue(name, out callTarget))
                    return true;
            }

            callTarget = null;
            return false;
        }

        public static object GetAttribute(CodeContext/*!*/ context, object self, string name, PythonTypeSlot getAttributeSlot, PythonTypeSlot getAttrSlot, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, string, object>>>/*!*/ callSite) {
            object value;
            if (callSite.Data == null) {
                callSite.Data = MakeGetAttrSite(context);
            }

            try {
                if (getAttributeSlot.TryGetBoundValue(context, self, ((IPythonObject)self).PythonType, out value)) {
                    return callSite.Data.Target(callSite.Data, context, value, name);
                } 
            } catch (MissingMemberException) {
                if (getAttrSlot != null && getAttrSlot.TryGetBoundValue(context, self, ((IPythonObject)self).PythonType, out value)) {
                    return callSite.Data.Target(callSite.Data, context, value, name);
                }

                throw;
            }

            if (getAttrSlot != null && getAttrSlot.TryGetBoundValue(context, self, ((IPythonObject)self).PythonType, out value)) {
                return callSite.Data.Target(callSite.Data, context, value, name);
            }

            throw PythonOps.AttributeError(name);
        }

        public static object GetAttributeNoThrow(CodeContext/*!*/ context, object self, string name, PythonTypeSlot getAttributeSlot, PythonTypeSlot getAttrSlot, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, string, object>>>/*!*/ callSite) {
            object value;
            if (callSite.Data == null) {
                callSite.Data = MakeGetAttrSite(context);
            }

            try {
                if (getAttributeSlot.TryGetBoundValue(context, self, ((IPythonObject)self).PythonType, out value)) {
                    return callSite.Data.Target(callSite.Data, context, value, name);
                }
            } catch (MissingMemberException) {
                try {
                    if (getAttrSlot != null && getAttrSlot.TryGetBoundValue(context, self, ((IPythonObject)self).PythonType, out value)) {
                        return callSite.Data.Target(callSite.Data, context, value, name);
                    }

                    return OperationFailed.Value;
                } catch (MissingMemberException) {
                    return OperationFailed.Value;
                }
            }

            try {
                if (getAttrSlot != null && getAttrSlot.TryGetBoundValue(context, self, ((IPythonObject)self).PythonType, out value)) {
                    return callSite.Data.Target(callSite.Data, context, value, name);
                }
            } catch (MissingMemberException) {
            }

            return OperationFailed.Value;
        }

        private static CallSite<Func<CallSite, CodeContext, object, string, object>> MakeGetAttrSite(CodeContext context) {
            return CallSite<Func<CallSite, CodeContext, object, string, object>>.Create(
                PythonContext.GetContext(context).DefaultBinderState.InvokeOne
            );
        }

        #region IValueEquality Helpers

        public static int GetValueHashCodeHelper(object self) {
            // new-style classes only lookup in slots, not in instance
            // members
            object func;
            if (DynamicHelpers.GetPythonType(self).TryGetBoundMember(DefaultContext.Default, self, Symbols.Hash, out func)) {
                return Converter.ConvertToInt32(PythonCalls.Call(func));
            }

            return self.GetHashCode();
        }

        public static bool ValueEqualsHelper(object self, object other) {
            object res = RichEqualsHelper(self, other);
            if (res != NotImplementedType.Value && res != null && res.GetType() == typeof(bool))
                return (bool)res;

            return false;
        }

        private static object RichEqualsHelper(object self, object other) {
            object res;

            if (PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default, self, other, Symbols.OperatorEquals, out res))
                return res;

            return NotImplementedType.Value;
        }

        #endregion
    }
}
