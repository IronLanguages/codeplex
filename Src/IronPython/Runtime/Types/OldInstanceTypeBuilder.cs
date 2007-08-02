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
using IronPython.Runtime.Operations;
using System.Diagnostics;

namespace IronPython.Runtime.Types {
    public class OldInstanceTypeBuilder {        
        public static DynamicType Build(OldClass oc) {
            DynamicTypeBuilder dtb = new DynamicTypeBuilder(oc.Name, typeof(OldInstance));
            foreach (OldClass subtype in oc.BaseClasses) {
                dtb.AddBaseType(subtype.TypeObject);                
            }
            dtb.AddSlot(Symbols.Class, new DynamicTypeValueSlot(oc));

            // a customizer is defined on old-instances even though we usually hit ICustomAttributes.
            // This is because if we have a type that inherits from both new-style & old-style 
            // classes then we want to use a custom lookup
            dtb.SetCustomBoundGetter(TryGetMemberCustomizer);
            dtb.SetCustomSetter(SetMemberCustomizer);
            dtb.SetCustomDeleter(DeleteMemberCustomizer);

            return (DynamicType)dtb.Finish(false);
        }

        private static void SetMemberCustomizer(CodeContext context, object instance, SymbolId name, object value) {
            DynamicHelpers.GetDynamicType(instance).TrySetNonCustomMember(context, instance, name, value);
        }

        private static void DeleteMemberCustomizer(CodeContext context, object instance, SymbolId name) {
            DynamicHelpers.GetDynamicType(instance).TryDeleteNonCustomMember(context, instance, name);
        }

        internal static bool TryGetMemberCustomizer(CodeContext context, object instance, SymbolId name, out object value) {
            ISuperDynamicObject sdo = instance as ISuperDynamicObject;
            if (sdo != null) {
                IAttributesCollection iac = sdo.Dict;
                if (iac != null && iac.TryGetValue(name, out value)) {
                    return true;
                }
            }

            DynamicType dt = DynamicHelpers.GetDynamicType(instance);

            foreach (DynamicType type in dt.ResolutionOrder) {
                DynamicTypeSlot dts;
                if (type != TypeCache.Object && type.TryLookupSlot(context, Symbols.Class, out dts) &&
                    dts.TryGetValue(context, instance, type, out value)) {

                    // we're an old class, check the old-class way
                    OldClass oc = value as OldClass;
                    Debug.Assert(oc != null);

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
    }
}