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
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Provides a slot object for the dictionary to allow setting of the dictionary.
    /// </summary>
    [PythonType("getset_descriptor")]
    public sealed class PythonTypeDictSlot : PythonTypeSlot, ICodeFormattable {
        public PythonTypeDictSlot() {
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                IAttributesCollection res = sdo.Dict;
                if (res != null || (res = sdo.SetDict(new SymbolDictionary()))!=null) {
                    value = res;
                    return true;
                }
            }

            if (instance == null) {
                value = new DictProxy(owner);
                return true;
            }

            value = new DictProxy(instance as PythonType);
            return true;
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                if (!(value is IAttributesCollection))
                    throw PythonOps.TypeError("__dict__ must be set to a dictionary, not '{0}'", owner.Name);

                return sdo.ReplaceDict((IAttributesCollection)value);
            }

            if (instance == null) throw PythonOps.TypeError("'__dict__' of '{0}' objects is not writable", owner.Name);
            return false;
        }

        internal override bool IsSetDescriptor(CodeContext context, PythonType owner) {
            return true;
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                return sdo.ReplaceDict(null);
            }

            if (instance == null) throw PythonOps.TypeError("'__dict__' of '{0}' objects is not writable", owner.Name);
            return false;
        }

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            return String.Format("<attribute '__dict__' of 'type' objects");
        }

        #endregion
    }

}
