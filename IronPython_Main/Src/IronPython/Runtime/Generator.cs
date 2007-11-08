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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;   

using Microsoft.Scripting;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;

namespace IronPython.Runtime {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix"), PythonType("generator")]
    public sealed class PythonGenerator : Generator, IEnumerable, IEnumerable<object> {
        private NextTarget _next;

        public delegate bool NextTarget(PythonGenerator generator, out object ret);

        public PythonGenerator(CodeContext context, NextTarget next)
        : base(context) {
            _next = next;
        }

        public override bool MoveNext() {
            bool ret;
            object next;

            try {
                ret = _next(this, out next);
            } catch (StopIterationException) {
                next = null;
                ret = false;
            }
            this.Current = next;
            return ret;
        }

        [PythonName("next")]
        public object Next() {
            if (!MoveNext()) {
                throw PythonOps.StopIteration();
            }
            return Current;
        }

        public override string ToString() {
            return string.Format("<generator object at {0}>", PythonOps.HexId(this));
        }
        
        #region IEnumerable Members

        [PythonName("__iter__")]
        public IEnumerator GetEnumerator() {
            return this;
        }

        #endregion      

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return this;
        }

        #endregion
    }
}
