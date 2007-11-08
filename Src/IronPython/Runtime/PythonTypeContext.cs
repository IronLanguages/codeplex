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

using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Context object to store Python specific information on PythonType objects.
    /// </summary>
    class PythonTypeContext : IWeakReferenceable {
        private WeakRefTracker _tracker;
        private bool _isPythonType;

        #region IWeakReferenceable Members

        public WeakRefTracker GetWeakRef() {
            return _tracker;
        }

        public bool SetWeakRef(WeakRefTracker value) {
            _tracker = value;
            return true;
        }

        public void SetFinalizer(WeakRefTracker value) {
            SetWeakRef(value);
        }

        #endregion

        public bool IsPythonType {
            get {
                return _isPythonType;
            }
            set {
                _isPythonType = value;
            }
        }

        /// <summary>
        /// Checks to see if the declaring type is the same as the owning type or
        /// if the declaring type's members should be shown for the owning type.
        /// </summary>
        public static bool IsMineOrVisible(PythonType owner, PythonType declaring) {
            if (owner != null && !object.ReferenceEquals(owner, declaring)) {
                PythonTypeContext ctx = owner.GetContextTag(PythonContext.Id) as PythonTypeContext;
                if (ctx != null && ctx.IsPythonType) {
                    // see if this methods type is a non-python type
                    ctx = declaring.GetContextTag(PythonContext.Id) as PythonTypeContext;
                    if (ctx == null || !ctx.IsPythonType) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
