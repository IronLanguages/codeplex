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
using System.Reflection;

using Microsoft.Scripting;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Just like a reflected property, but we also allow deleting of values (setting them to
    /// Uninitialized.instance)
    /// </summary>
    [PythonType("member_descriptor")]
    public class ReflectedSlotProperty : ReflectedProperty, ICodeFormattable {

        public ReflectedSlotProperty(PropertyInfo info, MethodInfo getter, MethodInfo setter, NameType nt)
            : base(info, getter, setter, nt) {
        }

        [PythonName("__delete__")]
        public override bool TryDeleteValue(CodeContext context, object instance, DynamicMixin owner) {
            if (instance != null) {
                TrySetValue(context, instance, owner, Uninitialized.Instance);
                return true;
            }
            return false;
        }

        public override string ToString() {
            return String.Format("<member '{0}'>", Name); // <member '{0}' of '{1}' objects> - but we don't know our type name
        }

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            return ToString();
        }

        #endregion
    }

}
