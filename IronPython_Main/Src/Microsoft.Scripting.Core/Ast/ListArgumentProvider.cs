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
using System.Collections.Generic;
using System.Text;
using Microsoft.Linq.Expressions;

namespace Microsoft.Linq.Expressions {
    /// <summary>
    /// Provides a wrapper around an IArgumentProvider which exposes the argument providers
    /// members out as an IList of Expression.  This is used to avoid allocating an array
    /// which needs to be stored inside of a ReadOnlyCollection.  Instead this type has
    /// the same amount of overhead as an array without duplicating the storage of the
    /// elements.  This ensures that internally we can avoid creating and copying arrays
    /// while users of the Expression trees also don't pay a size penalty for this internal
    /// optimization.  See IArgumentProvider for more general information on the Expression
    /// tree optimizations being used here.
    /// </summary>
    class ListArgumentProvider : IList<Expression> {
        private readonly IArgumentProvider _provider;
        private readonly Expression _arg0;

        internal ListArgumentProvider(IArgumentProvider provider, Expression arg0) {
            _provider = provider;
            _arg0 = arg0;
        }

        #region IList<Expression> Members

        public int IndexOf(Expression item) {
            if (_arg0 == item) {
                return 0;
            }

            for (int i = 1; i < _provider.ArgumentCount; i++) {
                if (_provider.GetArgument(i) == item) {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, Expression item) {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        public Expression this[int index] {
            get {
                if (index == 0) {
                    return _arg0;
                }

                return _provider.GetArgument(index);
            }
            set {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<Expression> Members

        public void Add(Expression item) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(Expression item) {
            return IndexOf(item) != -1;
        }

        public void CopyTo(Expression[] array, int arrayIndex) {
            array[arrayIndex++] = _arg0;
            for (int i = 1; i < _provider.ArgumentCount; i++) {
                array[arrayIndex++] = _provider.GetArgument(i);
            }
        }

        public int Count {
            get { return _provider.ArgumentCount; }
        }

        public bool IsReadOnly {
            get { return true; }
        }

        public bool Remove(Expression item) {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<Expression> Members

        public IEnumerator<Expression> GetEnumerator() {
            yield return _arg0;

            for (int i = 1; i < _provider.ArgumentCount; i++) {
                yield return _provider.GetArgument(i);
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            yield return _arg0;

            for (int i = 1; i < _provider.ArgumentCount; i++) {
                yield return _provider.GetArgument(i);
            }
        }

        #endregion
    }
}