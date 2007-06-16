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
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Scripting;

namespace Microsoft.Scripting {
    public class ReflectedField : DynamicTypeSlot, IContextAwareMember {
        public readonly FieldInfo info;
        private NameType nameType;

        public ReflectedField(FieldInfo info, NameType nameType) {
            this.nameType = nameType;
            this.info = info;
        }

        public ReflectedField(FieldInfo info)
            : this(info, NameType.PythonField) {
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance == null) {
                if (info.IsStatic) {
                    value = info.GetValue(null);
                } else {
                    value = this;
                }
            } else {
                value = info.GetValue(context.LanguageContext.Binder.Convert(instance, info.DeclaringType));
            }

            return true;
        }

        private bool ShouldSetOrDelete(object instance, DynamicMixin type) {
            DynamicType dt = type as DynamicType;
            
            // statics must be assigned through their type, not a base type.  Non-statics can
            // be assigned through their instances.
            return (dt != null && info.DeclaringType == dt.UnderlyingSystemType) || !info.IsStatic;
        }

        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            if (ShouldSetOrDelete(instance, owner)) {
                DoSet(context, instance, value);
                return true;
            }

            return false;
        }

        public override bool TryDeleteValue(CodeContext context, object instance, DynamicMixin owner) {
            if (ShouldSetOrDelete(instance, owner)) {
                return base.TryDeleteValue(context, instance, owner);
            }
            return false;
        }

        private void DoSet(CodeContext context, object instance, object val) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Fields, this);
            if (instance != null && instance.GetType().IsValueType)
                throw new ArgumentException(String.Format("Attempt to update field '{0}' on value type '{1}'; value type fields cannot be directly modified", info.Name, info.DeclaringType.Name));
            if (info.IsInitOnly)
                throw new MissingFieldException(String.Format("Cannot set field {1} on type {0}", info.DeclaringType.Name, SymbolTable.StringToId(info.Name)));

            info.SetValue(instance, context.LanguageContext.Binder.Convert(val, info.FieldType));
        }

        #region IContextAwareMember Members

        public override bool IsVisible(CodeContext context, DynamicMixin owner) {
            return nameType == NameType.PythonField || context.ModuleContext.ShowCls;
        }

        #endregion
    }
}
