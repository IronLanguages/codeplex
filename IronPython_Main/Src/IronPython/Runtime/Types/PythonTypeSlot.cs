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
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// A TypeSlot is an item that gets stored in a type's dictionary.  Slots provide an 
    /// opportunity to customize access at runtime when a value is get or set from a dictionary.
    /// </summary>
    public class PythonTypeSlot {
        /// <summary>
        /// Gets the value stored in the slot for the given instance. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        internal virtual bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = null;
            return false;
        }

        /// <summary>
        /// Gets the value stored in the slot for the given instance, bound to the instance if possible
        /// </summary>
        internal virtual bool TryGetBoundValue(CodeContext context, object instance, PythonType owner, out object value) {
            return TryGetValue(context, instance, owner, out value);
        }

        /// <summary>
        /// Sets the value of the slot for the given instance.
        /// </summary>
        /// <returns>true if the value was set, false if it can't be set</returns>
        internal virtual bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            return false;
        }

        /// <summary>
        /// Deletes the value stored in the slot from the instance.
        /// </summary>
        /// <returns>true if the value was deleted, false if it can't be deleted</returns>
        internal virtual bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {            
            object dummy;
            return DynamicHelpers.GetPythonType(this).TryInvokeBinaryOperator(context,
                Operators.DeleteDescriptor,
                this,
                instance ?? owner,
                out dummy);
        }

        internal virtual bool IsVisible(CodeContext context, PythonType owner) {
            return true;
        }

        internal virtual bool IsSetDescriptor(CodeContext context, PythonType owner) {
            return false;
        }
    }
}
