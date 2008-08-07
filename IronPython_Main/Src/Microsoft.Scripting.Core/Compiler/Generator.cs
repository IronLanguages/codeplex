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

using System.Collections;
using System.Collections.Generic;

namespace System.Runtime.CompilerServices {
    public delegate bool GeneratorNext(Generator generator, out object next);

    // TODO: should implement IEnumerable too, see C# generators
    public sealed class Generator : IEnumerator, IEnumerator<object>, IDisposable {
        private object _current;
        private readonly GeneratorNext _next;

        public int location = Int32.MaxValue;

        public Generator(GeneratorNext next) {
            _next = next;
        }

        public object Current {
            get { return _current; }
        }

        public bool MoveNext() {
            return _next(this, out _current);
        }

        public void Reset() {
            throw new NotImplementedException();
        }

        #region IDisposable Members

        void IDisposable.Dispose() {
            // nothing needed to dispose
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
