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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;   
using Microsoft.Scripting;

namespace Microsoft.Scripting.Runtime {
    public abstract class Generator : IEnumerator, IEnumerator<object>, IDisposable {
        private object _current;
        private readonly CodeContext _context;

        public int location = Int32.MaxValue;

        protected Generator(CodeContext context) {
            _context = context;
        }

        public object Current {
            get { return _current; }
            protected set { _current = value; }
        }

        public CodeContext Context {
            get { return _context; }
        }

        /// <summary>
        /// Protected so languages can expose their own set of public API surface
        /// </summary>
        protected abstract bool MoveNext();

        public void Reset() {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Explicitly implemented so languages can expose their own set of public API surface
        /// </summary>
        /// <returns></returns>
        bool IEnumerator.MoveNext() {
            return MoveNext();
        }

        #region IDisposable Members

        void IDisposable.Dispose() {
            // nothing needed to dispose
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
