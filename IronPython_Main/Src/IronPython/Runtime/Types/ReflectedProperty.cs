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

using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Utils = Microsoft.Scripting.Utils;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IronPython.Runtime.Types {
    [PythonSystemType("property#")]
    public class ReflectedProperty : ReflectedGetterSetter, ICodeFormattable {
        public ReflectedProperty(PropertyInfo info, MethodInfo getter, MethodInfo setter, NameType nt)
            : base(info, getter, setter, nt) {
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            if (Setter == null) return false;

            if (instance == null) {
                if (Setter.IsStatic && DeclaringType != ((PythonType)owner).UnderlyingSystemType)
                    return false;
            } else if (instance != null) {
                if (Setter.IsStatic)
                    return false;
            }

            return CallSetter(context, instance, Utils.ArrayUtils.EmptyObjects, value);
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Properties, this);

            value = CallGetter(context, instance, Utils.ArrayUtils.EmptyObjects);
            return true;
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            __delete__(instance);
            return true;
        }

        internal sealed override bool IsVisible(CodeContext context, PythonType owner) {
            if (context.ModuleContext.ShowCls)
                return true;

            return NameType == NameType.PythonProperty;
        }

        internal override bool IsSetDescriptor(CodeContext context, PythonType owner) {
            return Setter != null;
        }

        #region Public Python APIs

        /// <summary>
        /// Convenience function for users to call directly
        /// </summary>
        public object GetValue(CodeContext context, object instance) {
            object value;
            if (TryGetValue(context, instance, DynamicHelpers.GetPythonType(instance), out value)) {
                return value;
            }
            throw new InvalidOperationException("cannot get property");
        }

        /// <summary>
        /// Convenience function for users to call directly
        /// </summary>
        public void SetValue(CodeContext context, object instance, object value) {
            if (!TrySetValue(context, instance, DynamicHelpers.GetPythonType(instance), value)) {
                throw new InvalidOperationException("cannot set property");
            }
        }

        public void __set__(object instance, object value) {
            // TODO: Throw?  currently we have a test that verifies we never throw when this is called directly.
            TrySetValue(DefaultContext.Default, instance, DynamicHelpers.GetPythonType(instance), value);
        }

        [SpecialName]
        public void __delete__(object instance) {
            if (Setter != null)
                throw PythonOps.AttributeErrorForReadonlyAttribute(
                    DynamicHelpers.GetPythonTypeFromType(DeclaringType).Name,
                    SymbolTable.StringToId(Name));
            else
                throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(
                    DynamicHelpers.GetPythonTypeFromType(DeclaringType).Name,
                    SymbolTable.StringToId(Name));
        }

        public string __doc__ {
            get {
                return DocBuilder.DocOneInfo(Info);
            }
        }

        #endregion

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return string.Format("<property# {0} on {1}>",
                Name,
                PythonTypeOps.GetName(DynamicHelpers.GetPythonTypeFromType(DeclaringType)));
        }

        #endregion
    }
}
