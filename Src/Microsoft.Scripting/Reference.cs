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

namespace Microsoft.Scripting {
    /// <summary>
    /// A standard wrapper that can be used by languages which don't support by-ref calling
    /// conventions to call methods that expect by-ref parameters.  A reference to the contained
    /// value should be passed to methods on input and the Value should be updated on return.
    /// </summary>
    /// <typeparam name="T">The type of the by-ref value.</typeparam>
    public class Reference<T> : IReference {
        private T _value;
        public Reference() {
            _value = default(T);
        }

        public Reference(T value) {
            this._value = value;
        }

        public T Value {
            get { return _value; }
            set { this._value = value; }
        }

        public override string ToString() {
            if ((object)Value == this) {
                return "Reference (...)";
            }
            return string.Format("Reference({0})", Value);
        }

        #region IReference Members

        object IReference.Value {
            get { return _value; }
            set { this._value = (T)value; }
        }

        #endregion
    }

    public interface IReference {
        object Value { get; set; }
    }
}
